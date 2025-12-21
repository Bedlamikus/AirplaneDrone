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
            // Применяем сохраненную скорость самолета
            rb.velocity = savedVelocity;
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

