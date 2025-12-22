using UnityEngine;
using System.Collections;

public class Projectile : MonoBehaviour
{
    [SerializeField] private Rigidbody rb;
    [SerializeField] private GameObject particleObject;
    [SerializeField] private float delaySeconds = 1f;
    [SerializeField] private float maxFlightTime = 15f; // Максимальное время полета до самоуничтожения
    [SerializeField] private float explosionRadius = 5f; // Радиус взрыва
    [SerializeField] private float explosionForce = 10f; // Сила взрыва
    
    private bool isFired = false;
    private bool isCollided = false;
    private bool isTrajectoryActive = false;
    private Vector3 savedVelocity = Vector3.zero;
    private Vector3 initialPosition;
    private Vector3 initialVelocity;
    private Vector3 gravity;
    private float trajectoryTime = 0f;
    private float flightStartTime = 0f; // Время начала полета

    private void Start()
    {
        GlobalEvents.OnFire.AddListener(Fire);
        GlobalEvents.OnAirplaneVelocity.AddListener(OnAirplaneVelocityReceived);
        
        // Убеждаемся, что Rigidbody kinematic
        if (rb != null)
        {
            rb.isKinematic = true;
        }
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

        StartCoroutine(FireRoutine());
    }

    private IEnumerator FireRoutine()
    {
        yield return new WaitForSeconds(0.1f);

        // Отвязываем от родителя
        transform.parent = null;
        rb.isKinematic = false;

        // Используем сохраненную скорость из OnAirplaneVelocityReceived
        // Сохраняем начальные параметры для траектории
        initialPosition = transform.position;
        initialVelocity = savedVelocity;
        gravity = Physics.gravity;
        trajectoryTime = 0f;
        flightStartTime = Time.time; // Запоминаем время начала полета
        
        // Запускаем движение по траектории
        isTrajectoryActive = true;
        
        // Запускаем корутину самоуничтожения
        StartCoroutine(SelfDestructRoutine());
        
        Debug.Log($"Projectile fired with velocity: {savedVelocity}");
    }
    
    private void Update()
    {
        // Двигаемся по траектории, если она активна
        if (isTrajectoryActive && !isCollided)
        {
            trajectoryTime += Time.deltaTime;
            
            // Формула движения под действием гравитации: position(t) = startPos + v0*t + 0.5*g*t^2
            Vector3 newPosition = initialPosition + initialVelocity * trajectoryTime + 0.5f * gravity * trajectoryTime * trajectoryTime;
            transform.position = newPosition;
        }
    }
    
    private IEnumerator SelfDestructRoutine()
    {
        // Ждем максимальное время полета
        yield return new WaitForSeconds(maxFlightTime);
        
        // Если снаряд все еще летит и не столкнулся, уничтожаем его
        if (!isCollided && isTrajectoryActive)
        {
            Debug.Log("Projectile self-destructed after max flight time");
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Проверяем столкновение с объектом, имеющим тег Obstacle
        if (other.CompareTag("Obstacle"))
        {
            OnObstacleHit();
        }
    }
    
    private void OnCollisionEnter(Collision collision)
    {
        // Проверяем столкновение с объектом, имеющим тег Obstacle
        if (collision.gameObject.CompareTag("Obstacle"))
        {
            OnObstacleHit();
        }
    }
    
    private void OnObstacleHit()
    {
        if (isCollided) return; // Предотвращаем множественные срабатывания
        
        isCollided = true;
        isTrajectoryActive = false; // Останавливаем движение по траектории
        
        // Выполняем взрыв - наносим урон всем объектам Damaged в радиусе
        Explode();
        
        // Включаем физику
        if (rb != null)
        {
            rb.isKinematic = false;
            // Вычисляем текущую скорость на основе траектории
            Vector3 currentVelocity = initialVelocity + gravity * trajectoryTime;
            rb.velocity = currentVelocity;
        }
        
        // Запускаем корутину с таймером
        StartCoroutine(DelayedParticleEffect());
    }
    
    private void Explode()
    {
        Vector3 explosionPosition = transform.position;
        
        // Находим все коллайдеры в радиусе взрыва
        Collider[] colliders = Physics.OverlapSphere(explosionPosition, explosionRadius);
        
        Debug.Log($"Projectile: Explosion at {explosionPosition}, found {colliders.Length} colliders in radius {explosionRadius}");
        
        foreach (Collider hit in colliders)
        {
            // Проверяем, есть ли компонент Damaged
            Damaged damaged = hit.GetComponent<Damaged>();
            if (damaged != null)
            {
                // Наносим урон объекту
                damaged.TakeDamage(explosionPosition, explosionForce);
            }
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

