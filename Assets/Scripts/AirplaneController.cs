using System.Collections;
using UnityEngine;

public class AirplaneController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float forwardSpeed = 10f;
    [SerializeField] private float boostSpeed = 20f;
    [SerializeField] private float pitchSpeed = 50f;
    [SerializeField] private float rollSpeed = 50f;
    [SerializeField] private float forceArround = 50f;

    [SerializeField] private InputPlayer inputPlayer;
    
    [Header("Destruction Settings")]
    [Tooltip("Сила взрыва при столкновении")]
    [SerializeField] private float explosionForce = 1000f;
    [Tooltip("Радиус взрыва")]
    [SerializeField] private float explosionRadius = 5f;

    private Rigidbody rb;
    private bool isPaused = false;
    private bool isOutOfBounds = false; // Флаг выхода за границы
    public bool isDestroyed = false; // Флаг разрушения самолета
    [SerializeField] private AirplanePart[] airplaneParts; // Массив частей самолета

    public Transform spawnPosition;

    private void Start()
    {
        // Получаем или добавляем Rigidbody
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }

        // Настройки Rigidbody для самолета
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        rb.useGravity = false; // Отключаем гравитацию для самолета
        
        // Ставим самолет на паузу на старте
        Pause();
        
        // Подписываемся на события выхода за границы и рестарта сценария
        GlobalEvents.OnAirplaneOutOfBounds.AddListener(OnAirplaneOutOfBounds);
        GlobalEvents.OnRestartCurrentScenario.AddListener(OnRestartScenario);
        GlobalEvents.OnScenarioStart.AddListener(OnRestartScenario);
        GlobalEvents.Resume.AddListener(OnRestartScenario);
        GlobalEvents.Pause.AddListener(Pause);
    }
    private void FixedUpdate()
    {
        // Не обновляем физику, если самолет на паузе или вышел за границы
        if (inputPlayer == null || rb == null || isPaused || isOutOfBounds) return;

        // Определяем текущую скорость в зависимости от нажатия пробела
        float currentSpeed = inputPlayer.Boost ? boostSpeed : forwardSpeed;

        // Постоянное движение вперед через velocity для правильной обработки коллизий
        rb.AddForce(transform.forward * currentSpeed, ForceMode.Force);
        rb.AddForce(-transform.forward * currentSpeed * 0.8f, ForceMode.Force);
        // Вращение через AddRelativeTorque (момент силы в локальных координатах)
        Vector3 torque = Vector3.zero;
        
        // Управление наклоном носа (Pitch) - момент силы по локальной оси X
        float pitchInput = inputPlayer.Pitch;
        if (Mathf.Abs(pitchInput) > 0.01f)
        {
            torque.x = pitchInput * pitchSpeed;
        }

        // Управление креном (Roll) - момент силы по локальной оси Z
        float rollInput = inputPlayer.Roll;
        if (Mathf.Abs(rollInput) > 0.01f)
        {
            torque.z = rollInput * rollSpeed;
        }

        // Применяем момент силы в локальных координатах
        if (torque != Vector3.zero)
        {
            rb.AddRelativeTorque(torque, ForceMode.Force);
        }
    }
    

    private void OnCollisionEnter(Collision collision)
    {
        bool obstacle = collision.gameObject.CompareTag("Obstacle");
        if (obstacle == false) return;
        
        // Если самолет уже разрушен, не обрабатываем столкновение повторно
        if (isDestroyed) return;

        var forcePoint = collision.contacts[0].point;

        // Разрушаем самолет
        DestroyAirplane(forcePoint);
        
    }
    
    /// <summary>
    /// Разрушить самолет на части
    /// </summary>
    private void DestroyAirplane(Vector3 explosionPoint)
    {
        if (isDestroyed) return;
        
        isDestroyed = true;
        
        // Ставим самолет на паузу
        Pause();
        
        // Отделяем все части самолета
        if (airplaneParts != null && airplaneParts.Length > 0)
        {
            foreach (var part in airplaneParts)
            {
                if (part != null)
                {
                    part.Detach(explosionPoint, explosionForce);
                }
            }
        }
        
        // Вызываем событие разрушения самолета
        GlobalEvents.OnAirplaneDestroyed?.Invoke();
    }
    
    /// <summary>
    /// Собрать самолет обратно (вызывается при рестарте)
    /// </summary>
    public void ReassembleAirplane()
    {
        StartCoroutine(Respawn());
    }

    private IEnumerator Respawn()
    {
        // Сначала делаем основной Rigidbody кинематическим
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        // Теперь собираем все части обратно
        if (airplaneParts != null && airplaneParts.Length > 0)
        {
            foreach (var part in airplaneParts)
            {
                if (part != null)
                {
                    part.Attach();
                }
            }
        }

        transform.position = spawnPosition.position;
        transform.rotation = spawnPosition.rotation;
        yield return null;

        isDestroyed = false;
        Resume();
    }

    private bool isDie = false;

    internal void Die()
    {
        if (isDie == true) return;
        
        isDie = true;
        StartCoroutine(DieRoutine());
    }

    private IEnumerator DieRoutine()
    {
        yield return null;
    }

    public void Pause()
    {
        if (isPaused) return;
        
        isPaused = true;
        if (rb != null)
        {
            rb.isKinematic = true; // Замораживаем физику
        }
    }

    public void Resume()
    {
        isPaused = false;
        if (rb != null)
        {
            rb.isKinematic = false; // Размораживаем физику
        }
    }
    /// <summary>
    /// Обработчик события выхода самолета за границы
    /// </summary>
    private void OnAirplaneOutOfBounds()
    {
        // Сразу ставим самолет на паузу и останавливаем все обновления
        isOutOfBounds = true;
        Pause();
        
        // Останавливаем все физические силы
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
        
        Debug.Log("AirplaneController: Airplane out of bounds - paused and stopped all updates");
    }
    
    /// <summary>
    /// Обработчик события рестарта сценария
    /// </summary>
    private void OnRestartScenario()
    {
        ReassembleAirplane();
    }
    
    private void OnDestroy()
    {
        // Отписываемся от событий при уничтожении
        GlobalEvents.OnAirplaneOutOfBounds.RemoveListener(OnAirplaneOutOfBounds);
        GlobalEvents.OnRestartCurrentScenario.RemoveListener(OnRestartScenario);
    }
}

