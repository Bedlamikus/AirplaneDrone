using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class TVStaticController : MonoBehaviour
{
    [Header("Shader Settings")]
    [SerializeField] private Material staticMaterial;
    [SerializeField] private Transform playerTransform;
    
    [Header("Static Effect")]
    [SerializeField] private Vector3 centerPosition = Vector3.zero;
    [SerializeField] private float minHeight = 5f; // Минимальная высота, при превышении которой начинается шум
    [SerializeField] private float maxHeight = 20f; // Максимальная высота, при превышении которой показывается сообщение
    
    [Header("Noise Settings")]
    [SerializeField] private float noiseScale = 100f;
    [SerializeField] private float noiseSpeed = 1f;
    [SerializeField] [Range(0f, 1f)] private float noiseIntensity = 0.5f;
    
    [Header("Scanline Settings")]
    [SerializeField] private float scanlineFrequency = 200f;
    [SerializeField] [Range(0f, 1f)] private float scanlineIntensity = 0.3f;

    [Header("Message signal lost")]
    [SerializeField] private Image lostSignalUIMessage;

    private bool materialIsInstance = false; // Флаг, указывающий, был ли материал создан автоматически
    private bool hasTriggeredOutOfBounds = false; // Флаг для предотвращения повторных вызовов события
    private bool isRestartCooldown = false; // Флаг блокировки проверки зоны во время рестарта
    private float restartCooldownEndTime = 0f; // Время окончания блокировки проверки зоны
    
    private void Start()
    {
        Renderer renderer = GetComponent<Renderer>();
        
        // Если материал не назначен, пытаемся получить его из Renderer
        if (staticMaterial == null && renderer != null)
        {
            // Используем material вместо sharedMaterial, чтобы создать instance
            // Это позволяет изменять параметры без влияния на другие объекты
            staticMaterial = renderer.material;
            materialIsInstance = true; // Помечаем, что материал был создан автоматически
        }
        else if (renderer != null && renderer.sharedMaterial == staticMaterial)
        {
            // Если материал назначен вручную, но это sharedMaterial,
            // создаем instance для безопасного изменения
            staticMaterial = renderer.material;
            materialIsInstance = true;
        }
        
        // Если игрок не назначен, пытаемся найти его автоматически
        if (playerTransform == null)
        {
            InputPlayer inputPlayer = FindObjectOfType<InputPlayer>();
            if (inputPlayer != null)
            {
                playerTransform = inputPlayer.transform;
            }
        }

        StartCoroutine(LostSignalRoutine());

        GlobalEvents.OnScenarioStart.AddListener(RestartFlag);
        GlobalEvents.OnRestartCurrentScenario.AddListener(OnRestartScenario);
    }
    
    private void RestartFlag()
    {
        isRestarting = false;
        hasTriggeredOutOfBounds = false;
    }
    
    /// <summary>
    /// Обработчик события рестарта сценария - блокируем проверку зоны на 1 секунду
    /// </summary>
    private void OnRestartScenario()
    {
        // Сбрасываем позицию игрока в шейдере в 0
        if (staticMaterial != null)
        {
            staticMaterial.SetVector("_PlayerPosition", Vector3.zero);
        }
        
        // Блокируем проверку зоны на 1 секунду
        isRestartCooldown = true;
        restartCooldownEndTime = Time.time + 1f;
    }

    private bool isRestarting = false;

    private void Update()
    {
        if (staticMaterial == null) return;

        if (isRestarting) return;
        
        // Проверяем, не истекла ли блокировка проверки зоны при рестарте
        if (isRestartCooldown && Time.time >= restartCooldownEndTime)
        {
            isRestartCooldown = false;
        }
        
        // Обновляем позицию игрока в шейдере только если нет блокировки рестарта
        if (playerTransform != null && !isRestartCooldown)
        {
            Vector3 playerPos = playerTransform.position;
            float playerHeight = playerPos.y;
            
            // Для расчета по высоте передаем данные так, чтобы шейдер вычислял расстояние только по Y оси
            // НО только если высота положительная и выше minHeight
            if (playerHeight > 0f && playerHeight > minHeight)
            {
                // Устанавливаем центр на уровне земли под игроком (X и Z совпадают, Y = 0)
                // Тогда расстояние будет равно высоте игрока
                Vector3 heightBasedCenter = new Vector3(playerPos.x, 0f, playerPos.z);
                
                staticMaterial.SetVector("_CenterPosition", heightBasedCenter);
                staticMaterial.SetVector("_PlayerPosition", playerPos);
                
                // Передаем высоты для расчета эффекта шума
                staticMaterial.SetFloat("_MinRadius", minHeight);
                staticMaterial.SetFloat("_MaxRadius", maxHeight);
            }
            else
            {
                // Если высота отрицательная или ниже/равна minHeight - устанавливаем центр так же, как позицию игрока
                // Это сделает расстояние = 0, и шума не будет
                staticMaterial.SetVector("_CenterPosition", playerPos);
                staticMaterial.SetVector("_PlayerPosition", playerPos);
                
                // Устанавливаем minRadius очень большим, чтобы шейдер точно не показывал эффект
                staticMaterial.SetFloat("_MinRadius", 999999f);
                staticMaterial.SetFloat("_MaxRadius", 999999f);
            }
            
            // Проверяем границы только если нет блокировки
            CheckBoundaries();
        }
        else
        {
            // При блокировке используем базовый центр
            staticMaterial.SetVector("_CenterPosition", centerPosition);
        }
        
        staticMaterial.SetFloat("_NoiseScale", noiseScale);
        staticMaterial.SetFloat("_NoiseSpeed", noiseSpeed);
        staticMaterial.SetFloat("_NoiseIntensity", noiseIntensity);
        staticMaterial.SetFloat("_ScanlineFrequency", scanlineFrequency);
        staticMaterial.SetFloat("_ScanlineIntensity", scanlineIntensity);
        staticMaterial.SetFloat("_NoiseScale", noiseScale);
        staticMaterial.SetFloat("_NoiseSpeed", noiseSpeed);
        staticMaterial.SetFloat("_NoiseIntensity", noiseIntensity);
        staticMaterial.SetFloat("_ScanlineFrequency", scanlineFrequency);
        staticMaterial.SetFloat("_ScanlineIntensity", scanlineIntensity);
    }

    /// <summary>
    /// Проверка выхода самолета за границы по высоте
    /// </summary>
    private void CheckBoundaries()
    {
        if (playerTransform == null) return;

        // Получаем высоту самолета (Y координата)
        float playerHeight = playerTransform.position.y;

        // Проверяем только превышение максимальной высоты ВВЕРХ (не учитываем отрицательные высоты)
        // Если самолет вылетел выше максимальной высоты и событие еще не было вызвано, вызываем событие
        if (playerHeight > maxHeight && !hasTriggeredOutOfBounds)
        {
            hasTriggeredOutOfBounds = true;
            isRestarting = true;
            GlobalEvents.OnAirplaneOutOfBounds?.Invoke();
        }
        
        // Если самолет вернулся обратно ниже или равно максимальной высоты, сбрасываем флаг для возможности повторного вызова
        if (playerHeight <= maxHeight)
        {
            hasTriggeredOutOfBounds = false;
        }
    }
    
    private void OnDestroy()
    {
        // Отписываемся от событий
        GlobalEvents.OnScenarioStart.RemoveListener(RestartFlag);
        GlobalEvents.OnRestartCurrentScenario.RemoveListener(OnRestartScenario);
        
        // Уничтожаем только instance материалы, созданные автоматически
        // Не трогаем оригинальные материалы из проекта
        if (staticMaterial != null && Application.isPlaying && materialIsInstance)
        {
            Destroy(staticMaterial);
        }
    }

    private IEnumerator LostSignalRoutine()
    {
        while (true)
        {
            if (playerTransform == null)
            {
                yield return null;
                continue;
            }
            
            // Получаем высоту самолета
            float currentHeight = playerTransform.position.y;
            
            // Рассчитываем прозрачность на основе высоты
            // Шум показываем ТОЛЬКО если самолет выше minHeight И выше 0 (не учитываем отрицательные высоты)
            float alpha = 0f;
            if (currentHeight > 0f && currentHeight > minHeight && currentHeight < maxHeight)
            {
                // Нормализуем высоту между minHeight и maxHeight (0 до 1)
                // Если высота = minHeight, то alpha = 0, если = maxHeight, то alpha = 1
                alpha = (currentHeight - minHeight) / (maxHeight - minHeight);
            }
            else if (currentHeight > 0f && currentHeight >= maxHeight)
            {
                // Если выше или равно максимальной И выше 0 - максимальная прозрачность
                alpha = 1f;
            }
            // Если высота <= 0 или <= minHeight - alpha остается 0 (шума нет)
            
            yield return TransparentSignalMessage(alpha);
            yield return TransparentSignalMessage(0f);
        }
    }

    private IEnumerator TransparentSignalMessage(float alpha)
    {
        var color = lostSignalUIMessage.color;
        var startAlpha = color.a;
        float needTime = 0.1f;
        float timer = 0f;
        while (timer < needTime)
        {
            timer += Time.deltaTime;
            float a = Mathf.Lerp(startAlpha, alpha, timer / needTime);
            color.a = a;
            lostSignalUIMessage.color = color;
            yield return null;
        }
        color.a = alpha;
        lostSignalUIMessage.color = color;
    }
}

