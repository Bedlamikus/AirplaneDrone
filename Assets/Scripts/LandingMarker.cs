using UnityEngine;

/// <summary>
/// Компонент для отображения метки точки падения снаряда с оптимизацией производительности
/// </summary>
public class LandingMarker : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform projectileTransform; // Трансформ снаряда (опционально, если нужно отслеживать его позицию)
    [SerializeField] private Transform markerTransform; // Трансформ метки для позиционирования (опционально, если это не сам объект)
    
    [Header("Settings")]
    [SerializeField] private float updateInterval = 0.1f; // Интервал обновления в секундах (0 = каждый кадр)
    [SerializeField] private float groundLevel = 0f; // Уровень земли по Y
    [SerializeField] private bool useDrag = false; // Использовать ли расчет с учетом drag (медленнее)
    [SerializeField] private float drag = 0f; // Коэффициент сопротивления воздуха (если useDrag = true)
    
    
    private Vector3 currentVelocity = Vector3.zero;
    private Vector3 currentPosition = Vector3.zero;
    private Vector3 lastCalculatedLandingPoint = Vector3.zero;
    private float lastUpdateTime = 0f;
    private float cachedGravityY = -9.81f;
    
    private void Start()
    {
        // Кешируем гравитацию
        cachedGravityY = Physics.gravity.y;
        
        // Если markerTransform не назначен, используем transform этого объекта
        if (markerTransform == null)
        {
            markerTransform = transform;
        }
        
        // Если projectileTransform не назначен, пытаемся найти Projectile в дочерних объектах
        if (projectileTransform == null)
        {
            Projectile projectile = GetComponentInChildren<Projectile>();
            if (projectile != null)
            {
                projectileTransform = projectile.transform;
            }
        }
    }
    
    /// <summary>
    /// Установить текущую позицию и скорость снаряда для расчета
    /// </summary>
    public void SetProjectileState(Vector3 position, Vector3 velocity)
    {
        currentPosition = position;
        currentVelocity = velocity;
        
        // Если интервал обновления = 0, обновляем сразу
        if (updateInterval <= 0f)
        {
            UpdateMarker();
        }
    }
    
    private void Update()
    {
        // Если назначен projectileTransform, получаем данные оттуда
        if (projectileTransform != null)
        {
            currentPosition = projectileTransform.position;
            
            Rigidbody rb = projectileTransform.GetComponent<Rigidbody>();
            if (rb != null && !rb.isKinematic)
            {
                currentVelocity = rb.velocity;
            }
        }
        
        // Обновляем маркер с заданным интервалом
        if (Time.time - lastUpdateTime >= updateInterval)
        {
            UpdateMarker();
            lastUpdateTime = Time.time;
        }
    }
    
    private void UpdateMarker()
    {
        if (markerTransform == null) return;
        
        Vector3 landingPoint;
        
        if (useDrag && drag > 0f)
        {
            // Используем полный расчет с drag (медленнее, но точнее)
            landingPoint = ProjectileTrajectoryCalculator.CalculateLandingPointWithDrag(
                currentPosition,
                currentVelocity,
                drag,
                Physics.gravity,
                groundLevel
            );
        }
        else
        {
            // Используем оптимизированный расчет без drag (быстро)
            landingPoint = ProjectileTrajectoryCalculator.CalculateLandingPointOptimized(
                currentPosition,
                currentVelocity,
                cachedGravityY,
                groundLevel
            );
        }
        
        if (landingPoint != Vector3.zero)
        {
            lastCalculatedLandingPoint = landingPoint;
            markerTransform.position = landingPoint;
        }
    }
    
    /// <summary>
    /// Получить последнюю рассчитанную точку падения
    /// </summary>
    public Vector3 GetLandingPoint()
    {
        return lastCalculatedLandingPoint;
    }
    
    /// <summary>
    /// Принудительно обновить маркер (использовать перед показом маркера)
    /// </summary>
    public void ForceUpdate()
    {
        UpdateMarker();
    }
}

