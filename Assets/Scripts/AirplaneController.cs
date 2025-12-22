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

    [Header("Projectile Landing Marker")]
    [SerializeField] private Transform landingPointMarker; // Дочерний объект для визуализации места падения
    [SerializeField] private Transform projectileSpawnPoint; // Точка спавна снаряда (если есть)
    [SerializeField] private float landingMarkerMoveSpeed = 5f; // Скорость плавного перемещения маркера
    [SerializeField] private float groundLevel = 0f; // Уровень земли

    [Header("Projectile Settings")]
    [SerializeField] private GameObject projectilePrefab; // Префаб снаряда
    [SerializeField] private float projectileRespawnTime = 3f; // Время респавна снаряда в секундах

    [SerializeField] private InputPlayer inputPlayer;

    private Rigidbody rb;
    private bool isPaused = false;
    private Vector3 calculatedLandingPoint = Vector3.zero;
    private bool hasValidLandingPoint = false;
    private Projectile currentProjectile = null; // Текущий снаряд
    private bool isRespawning = false; // Флаг процесса респавна
    private bool isOutOfBounds = false; // Флаг выхода за границы

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
        
        // Подписываемся на событие выстрела, чтобы передать скорость
        GlobalEvents.OnFire.AddListener(OnFireEvent);
        
        // Подписываемся на события выхода за границы и рестарта сценария
        GlobalEvents.OnAirplaneOutOfBounds.AddListener(OnAirplaneOutOfBounds);
        GlobalEvents.OnRestartCurrentScenario.AddListener(OnRestartScenario);
        
        // Скрываем маркер места падения при старте
        if (landingPointMarker != null)
        {
            landingPointMarker.gameObject.SetActive(false);
        }
        
        // Ищем существующий снаряд в дочерних объектах или создаем новый
        FindOrSpawnProjectile();
    }
    
    private void FindOrSpawnProjectile()
    {
        Debug.Log("AirplaneController: FindOrSpawnProjectile called");
        
        // Ищем существующий снаряд в дочерних объектах
        currentProjectile = GetComponentInChildren<Projectile>();
        
        if (currentProjectile != null)
        {
            Debug.Log($"AirplaneController: Found existing projectile: {currentProjectile.name}");
        }
        else
        {
            Debug.Log("AirplaneController: No existing projectile found");
        }
        
        // Если снаряда нет, создаем новый из префаба
        if (currentProjectile == null)
        {
            if (projectilePrefab != null)
            {
                Debug.Log($"AirplaneController: Spawning new projectile from prefab: {projectilePrefab.name}");
                SpawnProjectile();
            }
            else
            {
                Debug.LogWarning("AirplaneController: Projectile prefab is not assigned!");
            }
        }
    }
    
    private void SpawnProjectile()
    {
        if (projectilePrefab == null)
        {
            Debug.LogError("AirplaneController: Cannot spawn projectile - prefab is null!");
            return;
        }
        
        Debug.Log("AirplaneController: SpawnProjectile called");
        
        // Определяем позицию спавна
        Vector3 spawnPosition = projectileSpawnPoint != null ? projectileSpawnPoint.position : transform.position;
        Quaternion spawnRotation = projectileSpawnPoint != null ? projectileSpawnPoint.rotation : transform.rotation;
        
        Debug.Log($"AirplaneController: Spawning projectile at position: {spawnPosition}, rotation: {spawnRotation}");
        
        // Создаем снаряд из префаба
        GameObject projectileObj = Instantiate(projectilePrefab, spawnPosition, spawnRotation);
        Debug.Log($"AirplaneController: Projectile instantiated: {projectileObj.name}");
        
        // Привязываем к самолету
        projectileObj.transform.SetParent(transform);
        Debug.Log($"AirplaneController: Projectile parent set to: {transform.name}");
        
        // Получаем компонент Projectile
        currentProjectile = projectileObj.GetComponent<Projectile>();
        
        if (currentProjectile == null)
        {
            Debug.LogError("AirplaneController: Projectile prefab doesn't have Projectile component!");
        }
        else
        {
            Debug.Log($"AirplaneController: Projectile component found: {currentProjectile.name}");
        }
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
    
    private void Update()
    {
        // Не обновляем ничего, если самолет вышел за границы
        if (isOutOfBounds) return;
        
        // Обновляем маркер места падения на основе текущей скорости самолета
        if (landingPointMarker != null && !isPaused && rb != null)
        {
            UpdateLandingMarker();
        }
    }
    
    private void UpdateLandingMarker()
    {
        UpdateLandingMarker(false);
    }
    
    /// <summary>
    /// Обновить маркер места падения
    /// </summary>
    /// <param name="instant">Если true, маркер переместится мгновенно, иначе плавно</param>
    private void UpdateLandingMarker(bool instant)
    {
        // Получаем позицию точки выстрела (если есть projectileSpawnPoint, иначе используем позицию самолета)
        Vector3 firePosition = projectileSpawnPoint != null ? projectileSpawnPoint.position : transform.position;
        Vector3 currentVelocity = rb.velocity;
        
        // Рассчитываем точку падения на основе текущей скорости
        calculatedLandingPoint = ProjectileTrajectoryCalculator.CalculateLandingPoint(
            firePosition,
            currentVelocity,
            Physics.gravity,
            groundLevel
        );
        
        hasValidLandingPoint = calculatedLandingPoint != Vector3.zero;
        
        // Показываем маркер, если есть валидная точка падения
        if (hasValidLandingPoint)
        {
            if (!landingPointMarker.gameObject.activeSelf)
            {
                landingPointMarker.gameObject.SetActive(true);
            }
            
            // Перемещаем маркер к расчетной точке
            if (instant)
            {
                // Мгновенное перемещение
                landingPointMarker.position = calculatedLandingPoint;
            }
            else
            {
                // Плавное перемещение
                landingPointMarker.position = Vector3.MoveTowards(
                    landingPointMarker.position,
                    calculatedLandingPoint,
                    landingMarkerMoveSpeed * Time.deltaTime
                );
            }
        }
        else
        {
            // Скрываем маркер, если нет валидной точки падения (снаряд не достигнет уровня земли)
            if (landingPointMarker.gameObject.activeSelf)
            {
                landingPointMarker.gameObject.SetActive(false);
            }
        }
    }
    
    /// <summary>
    /// Принудительно обновить маркер места падения (мгновенно)
    /// </summary>
    public void ForceUpdateLandingMarker()
    {
        if (landingPointMarker != null && rb != null)
        {
            UpdateLandingMarker(true);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        bool obstacle = collision.gameObject.CompareTag("Obstacle");
        if (obstacle == false) return;

        var forcePoint = collision.contacts[0].point;
        rb.AddExplosionForce(forceArround,
            forcePoint,
            1f);

        Debug.Log($"Contact, position = {forcePoint}, ");
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
        if (!isPaused) return;
        
        isPaused = false;
        if (rb != null)
        {
            rb.isKinematic = false; // Размораживаем физику
        }
    }
    
    private void OnFireEvent()
    {
        Debug.Log("AirplaneController: OnFireEvent called");
        
        // При выстреле передаем текущую скорость самолета
        if (rb != null)
        {
            GlobalEvents.OnAirplaneVelocity?.Invoke(rb.velocity);
            Debug.Log($"AirplaneController: Airplane velocity sent: {rb.velocity}");
        }
        else
        {
            Debug.LogWarning("AirplaneController: Rigidbody is null, cannot send velocity");
        }
        
        // Если есть текущий снаряд и не идет процесс респавна, запускаем респавн через N секунд
        if (currentProjectile != null && !isRespawning)
        {
            Debug.Log($"AirplaneController: Projectile fired, starting respawn routine in {projectileRespawnTime} seconds");
            currentProjectile = null; // Очищаем ссылку на выпущенный снаряд
            StartCoroutine(RespawnProjectileRoutine());
        }
        else if (currentProjectile == null)
        {
            Debug.LogWarning("AirplaneController: No current projectile found when firing!");
        }
        else if (isRespawning)
        {
            Debug.Log("AirplaneController: Already respawning, skipping");
        }
    }
    
    private IEnumerator RespawnProjectileRoutine()
    {
        if (isRespawning)
        {
            Debug.LogWarning("AirplaneController: RespawnProjectileRoutine called but already respawning!");
            yield break; // Предотвращаем множественные респавны
        }
        
        Debug.Log($"AirplaneController: Starting respawn routine, waiting {projectileRespawnTime} seconds");
        isRespawning = true;
        
        // Ждем N секунд
        yield return new WaitForSeconds(projectileRespawnTime);
        
        Debug.Log("AirplaneController: Respawn wait finished, spawning new projectile");
        
        // Создаем новый снаряд
        SpawnProjectile();
        
        isRespawning = false;
        Debug.Log("AirplaneController: Respawn routine completed");
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
        // Сбрасываем флаг выхода за границы, чтобы обновления снова заработали
        // Самолет будет перенесен и разморожен в Scenario.RespawnAirplane()
        isOutOfBounds = false;
        
        Debug.Log("AirplaneController: Scenario restart - reset out of bounds flag");
    }
    
    private void OnDestroy()
    {
        // Отписываемся от событий при уничтожении
        GlobalEvents.OnFire.RemoveListener(OnFireEvent);
        GlobalEvents.OnAirplaneOutOfBounds.RemoveListener(OnAirplaneOutOfBounds);
        GlobalEvents.OnRestartCurrentScenario.RemoveListener(OnRestartScenario);
    }
}

