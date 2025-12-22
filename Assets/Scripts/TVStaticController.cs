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
    [SerializeField] private float minRadius = 5f;
    [SerializeField] private float maxRadius = 20f;
    
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
    }
    
    private void RestartFlag()
    {
        isRestarting = false;
        hasTriggeredOutOfBounds = false;
    }

    private bool isRestarting = false;

    private void Update()
    {
        if (staticMaterial == null) return;

        if (isRestarting) return;

        // Обновляем позицию игрока в шейдере
        if (playerTransform != null)
        {
            staticMaterial.SetVector("_PlayerPosition", playerTransform.position);
            
            // Проверяем границы
            CheckBoundaries();
        }
        
        // Обновляем остальные параметры
        staticMaterial.SetVector("_CenterPosition", centerPosition);
        staticMaterial.SetFloat("_MinRadius", minRadius);
        staticMaterial.SetFloat("_MaxRadius", maxRadius);
        staticMaterial.SetFloat("_NoiseScale", noiseScale);
        staticMaterial.SetFloat("_NoiseSpeed", noiseSpeed);
        staticMaterial.SetFloat("_NoiseIntensity", noiseIntensity);
        staticMaterial.SetFloat("_ScanlineFrequency", scanlineFrequency);
        staticMaterial.SetFloat("_ScanlineIntensity", scanlineIntensity);
    }

    /// <summary>
    /// Проверка выхода самолета за границы
    /// </summary>
    private void CheckBoundaries()
    {
        if (playerTransform == null) return;

        // Вычисляем расстояние от центра до игрока
        float distance = Vector3.Distance(playerTransform.position, centerPosition);

        // Если самолет вышел за максимальную границу и событие еще не было вызвано, вызываем событие
        if (distance >= maxRadius && !hasTriggeredOutOfBounds)
        {
            hasTriggeredOutOfBounds = true;
            isRestarting = true;
            GlobalEvents.OnAirplaneOutOfBounds?.Invoke();
        }
        
        // Если самолет вернулся обратно в границы, сбрасываем флаг для возможности повторного вызова
        if (distance < maxRadius)
        {
            hasTriggeredOutOfBounds = false;
        }
    }
    
    private void OnDestroy()
    {
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
            float currentRadius = (playerTransform.position - centerPosition).magnitude;
            float alpha = Mathf.Clamp(currentRadius - minRadius, 0, maxRadius - currentRadius);
            
            if (alpha > 0)
            {
                alpha = alpha / (maxRadius - minRadius);
            }
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

