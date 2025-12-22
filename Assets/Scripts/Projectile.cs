using UnityEngine;
using System.Collections;

public class Projectile : MonoBehaviour
{
    [SerializeField] private Rigidbody rb;
    [SerializeField] private GameObject particleObject;
    [SerializeField] private float delaySeconds = 1f;
    
    private bool isFired = false;
    private bool isCollided = false;
    private Vector3 savedVelocity = Vector3.zero;

    private void Start()
    {
        GlobalEvents.OnFire.AddListener(Fire);
        GlobalEvents.OnAirplaneVelocity.AddListener(OnAirplaneVelocityReceived);
    }

    private void OnAirplaneVelocityReceived(Vector3 velocity)
    {
        // Сохраняем последнюю переданную скорость самолета
        savedVelocity = velocity;
    }

    private void Fire()
    {
        // Проверяем, что снаряд еще не был выпущен
        if (isFired) return;
        
        isFired = true;
        
        // Отвязываем от родителя
        transform.parent = null;
        
        // Отключаем kinematic режим
        if (rb != null)
        {
            rb.isKinematic = false;
            
            // Используем сохраненную скорость, если она есть, иначе пытаемся получить из родителя
            Vector3 velocityToApply = savedVelocity;
            
            // Если скорость не была сохранена (события вызвались в неправильном порядке),
            // пытаемся получить скорость из родителя (самолета)
            if (velocityToApply == Vector3.zero || savedVelocity.magnitude < 0.1f)
            {
                // Пытаемся найти самолет через родителя или по тегу
                Transform parentTransform = transform.parent;
                if (parentTransform == null)
                {
                    // Если уже отвязались, пытаемся найти самолет через InputPlayer
                    InputPlayer inputPlayer = FindObjectOfType<InputPlayer>();
                    if (inputPlayer != null)
                    {
                        Rigidbody airplaneRb = inputPlayer.GetComponent<Rigidbody>();
                        if (airplaneRb != null)
                        {
                            velocityToApply = airplaneRb.velocity;
                        }
                    }
                }
                else
                {
                    Rigidbody parentRb = parentTransform.GetComponentInParent<Rigidbody>();
                    if (parentRb != null)
                    {
                        velocityToApply = parentRb.velocity;
                    }
                }
            }
            
            // Применяем скорость самолета
            rb.velocity = velocityToApply;
            
            Debug.Log($"Projectile fired with velocity: {velocityToApply}, savedVelocity was: {savedVelocity}");
            
            // Рассчитываем точку падения (без учета drag для простоты)
            // Если нужен учет drag, используйте CalculateLandingPointWithDrag с rb.drag
            Vector3 landingPoint = ProjectileTrajectoryCalculator.CalculateLandingPoint(
                transform.position, 
                velocityToApply,
                Physics.gravity,
                0f // Уровень земли (можно настроить)
            );
            
            if (landingPoint != Vector3.zero)
            {
                float timeToLand = ProjectileTrajectoryCalculator.CalculateTimeToLand(
                    transform.position,
                    velocityToApply,
                    Physics.gravity,
                    0f
                );
                Debug.Log($"Projectile will land at: {landingPoint}, Time to land: {timeToLand:F2} seconds");
            }
            else
            {
                Debug.LogWarning("Projectile trajectory: Could not calculate landing point (projectile may not reach ground level)");
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Проверяем столкновение с объектом, имеющим тег Obstacle
        if (collision.gameObject.CompareTag("Obstacle"))
        {
            if (isCollided) return; // Предотвращаем множественные срабатывания
            
            isCollided = true;
            
            // Вызываем Fire снова на всякий случай (принудительно отвязываем)
            Fire();
            
            // Запускаем корутину с таймером
            StartCoroutine(DelayedParticleEffect());
        }
    }

    private IEnumerator DelayedParticleEffect()
    {
        particleObject.SetActive(true);

        // Отвязываем ParticleObject от Projectile
        particleObject.transform.parent = null;

        // Настраиваем ParticleSystem для автоматического удаления после воспроизведения
        ParticleSystem ps = particleObject.GetComponent<ParticleSystem>();
        if (ps != null)
        {
            // Если ParticleSystem на корневом объекте, используем stopAction.Destroy
            var main = ps.main;
            main.stopAction = ParticleSystemStopAction.Destroy;
        }
        // Ждем N секунд
        yield return new WaitForSeconds(delaySeconds);

        // Удаляем Projectile
        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        // Отписываемся от события при уничтожении объекта
        GlobalEvents.OnFire.RemoveListener(Fire);
        GlobalEvents.OnAirplaneVelocity.RemoveListener(OnAirplaneVelocityReceived);
    }
}

