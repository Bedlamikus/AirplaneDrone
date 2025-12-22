using UnityEngine;
using TMPro;

public class Scenario : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform airplaneSpawnPoint; // Точка спавна самолета
    [SerializeField] private AirplaneController airplane; // Ссылка на самолет
    [SerializeField] private TMP_Text endMessageText; // UI текст сообщения об окончании сценария
    [SerializeField] private TMP_Text timeMessageText; // UI текст сообщения со временем прохождения
    [SerializeField] private GameObject endMessageUI; // UI объект сообщения (GameObject с TMP_Text)
    [SerializeField] private TMP_Text outOfBoundsMessageText; // UI текст сообщения о выходе за границы
    [SerializeField] private GameObject outOfBoundsMessageUI; // UI объект сообщения о выходе за границы

    [Header("End Messages")]
    private readonly string[] endMessages = new string[]
    {
        "Well done, you did it!",
        "Fantastic job!",
        "You're amazing!",
        "Outstanding work!",
        "You're a superstar!",
        "Incredible! You made it!",
        "Wonderful! You succeeded!",
        "You're brilliant!",
        "Amazing effort!",
        "You're incredible!",
        "Superb! Well played!",
        "You're awesome!",
        "Excellent work!",
        "You did it! Congratulations!",
        "Terrific job!",
        "You're fantastic!",
        "Marvelous! You completed it!",
        "You're outstanding!",
        "Wonderful work!",
        "You're a champion!"
    };

    private readonly string[] timeMessages = new string[]
    {
        "This level you completed in {0} seconds. Keep it up!",
        "You finished this level in {0} seconds. Way to go!",
        "Great job! You completed this level in {0} seconds. Keep going!",
        "Amazing! This level took you {0} seconds. You're doing great!",
        "Fantastic! You finished in {0} seconds. Keep it up!",
        "Wonderful! This level completed in {0} seconds. So proud!",
        "Excellent! You did it in {0} seconds. Keep going!",
        "Outstanding! {0} seconds for this level. Way to go!",
        "Incredible! You finished in {0} seconds. Keep it up!",
        "Superb! This level took {0} seconds. You're amazing!",
        "Terrific! Completed in {0} seconds. Keep going!",
        "Brilliant! You finished this level in {0} seconds. So proud!",
        "Marvelous! {0} seconds for this level. Keep it up!",
        "Awesome! You completed it in {0} seconds. Way to go!",
        "Perfect! This level finished in {0} seconds. Keep going!",
        "Splendid! You did it in {0} seconds. You're amazing!",
        "Magnificent! Completed this level in {0} seconds. Keep it up!",
        "Remarkable! {0} seconds for this level. So proud!",
        "Impressive! You finished in {0} seconds. Keep going!",
        "Stellar! This level took {0} seconds. Way to go!"
    };

    private readonly string[] outOfBoundsMessages = new string[]
    {
        "You went out of bounds! Stay in the zone!",
        "Out of bounds! Please stay within the area!",
        "You left the safe zone! Get back in bounds!",
        "Boundary exceeded! Return to the play area!",
        "You're out of bounds! Stay within limits!",
        "Boundary violation! Please stay inside!",
        "You went too far! Keep within bounds!",
        "Out of range! Stay in the designated area!",
        "Boundary crossed! Return to safe zone!",
        "You're outside the zone! Get back in!",
        "Boundary exceeded! Stay within limits!",
        "Out of bounds detected! Return to play area!",
        "You left the safe area! Stay in bounds!",
        "Boundary violation! Get back inside!",
        "You're out of range! Stay within the zone!",
        "Boundary crossed! Please stay inside!",
        "Out of bounds! Keep within the area!",
        "You went too far! Stay in bounds!",
        "Boundary exceeded! Return to safe area!",
        "Out of zone! Stay within the limits!"
    };

    [Header("Scenario State")]
    private bool isScenarioActive = false;
    private float scenarioStartTime = 0f;
    private float scenarioEndTime = 0f;
    private Damaged[] cachedDamagedObjects; // Кешированный массив объектов Damaged в сценарии

    private void OnEnable()
    {
        GlobalEvents.OnAirplaneOutOfBounds.AddListener(OnAirplaneOutOfBounds);
    }

    private void OnDisable()
    {
        GlobalEvents.OnAirplaneOutOfBounds.RemoveListener(OnAirplaneOutOfBounds);
    }

    private void Start()
    {
        // Выключаем сообщение об окончании в начале
        if (endMessageUI != null)
        {
            endMessageUI.SetActive(false);
        }
        
        // Выключаем сообщение о выходе за границы в начале
        if (outOfBoundsMessageUI != null)
        {
            outOfBoundsMessageUI.SetActive(false);
        }
    }

    private void Update()
    {
        // Проверяем, все ли объекты Damaged скрыты, если сценарий активен
        if (isScenarioActive && cachedDamagedObjects != null)
        {
            CheckAllDamagedHidden();
        }
    }
    
    /// <summary>
    /// Проверка, все ли объекты Damaged скрыты
    /// </summary>
    private void CheckAllDamagedHidden()
    {
        // Проверяем, все ли объекты скрыты
        bool allHidden = true;
        foreach (var damaged in cachedDamagedObjects)
        {
            if (damaged != null && damaged.gameObject.activeSelf)
            {
                allHidden = false;
                break;
            }
        }
        
        // Если все объекты скрыты, завершаем сценарий
        if (allHidden)
        {
            EndScenario();
        }
    }

    /// <summary>
    /// Обработчик события выхода самолета за границы
    /// </summary>
    private void OnAirplaneOutOfBounds()
    {
        // Ставим самолет на паузу
        if (airplane != null)
        {
            airplane.Pause();
        }

        // Показываем сообщение о выходе за границы
        ShowOutOfBoundsMessage();
    }

    /// <summary>
    /// Показать сообщение о выходе за границы
    /// </summary>
    private void ShowOutOfBoundsMessage()
    {
        // Выбираем случайное сообщение о выходе за границы
        if (outOfBoundsMessages.Length > 0)
        {
            int randomIndex = Random.Range(0, outOfBoundsMessages.Length);
            string selectedMessage = outOfBoundsMessages[randomIndex];
            
            if (outOfBoundsMessageText != null)
            {
                outOfBoundsMessageText.text = selectedMessage;
            }
        }

        // Показываем UI сообщение о выходе за границы
        if (outOfBoundsMessageUI != null)
        {
            outOfBoundsMessageUI.SetActive(true);
        }
    }

    /// <summary>
    /// Респавн самолета в точке спавна
    /// </summary>
    public void RespawnAirplane()
    {
        if (airplane == null || airplaneSpawnPoint == null)
        {
            Debug.LogWarning("Scenario: Airplane or spawn point is not assigned!");
            return;
        }

        Debug.Log($"Scenario: RespawnAirplane called, moving from {airplane.transform.position} to {airplaneSpawnPoint.position}");

        // Сбрасываем позицию и поворот самолета
        airplane.transform.position = airplaneSpawnPoint.position;
        airplane.transform.rotation = airplaneSpawnPoint.rotation;

        // Размораживаем самолет сначала (делаем его не кинематическим)
        airplane.Resume();
        
        // Сбрасываем физику самолета (после разморозки)
        Rigidbody rb = airplane.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
        
        // Скрываем сообщение о выходе за границы
        if (outOfBoundsMessageUI != null)
        {
            outOfBoundsMessageUI.SetActive(false);
        }
        
        Debug.Log($"Scenario: Airplane respawned at {airplane.transform.position}");

        GlobalEvents.OnScenarioStart.Invoke();
    }

    /// <summary>
    /// Запуск сценария (вызывается при спавне самолета)
    /// </summary>
    public void StartScenario()
    {
        isScenarioActive = true;
        scenarioStartTime = Time.time;
        scenarioEndTime = 0f;

        // Кешируем все объекты Damaged в сценарии при первом запуске или восстанавливаем их
        if (cachedDamagedObjects == null || cachedDamagedObjects.Length == 0)
        {
            cachedDamagedObjects = GetComponentsInChildren<Damaged>();
            Debug.Log($"Scenario: Cached {cachedDamagedObjects?.Length ?? 0} Damaged objects");
        }
        else
        {
            // Восстанавливаем все объекты Damaged
            RestoreAllDamaged();
        }

        // Выключаем сообщение об окончании если оно было включено
        if (endMessageUI != null)
        {
            endMessageUI.SetActive(false);
        }
        
        // Выключаем сообщение о выходе за границы если оно было включено
        if (outOfBoundsMessageUI != null)
        {
            outOfBoundsMessageUI.SetActive(false);
        }
        
        // Очищаем текст
        if (endMessageText != null)
        {
            endMessageText.text = "";
        }
        
        if (timeMessageText != null)
        {
            timeMessageText.text = "";
        }
        
        if (outOfBoundsMessageText != null)
        {
            outOfBoundsMessageText.text = "";
        }
    }

    /// <summary>
    /// Окончание сценария
    /// </summary>
    public void EndScenario()
    {
        if (!isScenarioActive) return;

        isScenarioActive = false;
        scenarioEndTime = Time.time;

        // Замораживаем самолет (пауза)
        if (airplane != null)
        {
            airplane.Pause();
        }

        // Получаем время прохождения сценария
        float scenarioDuration = GetScenarioTime();
        int seconds = Mathf.RoundToInt(scenarioDuration);

        // Выбираем случайное сообщение и показываем его
        if (endMessages.Length > 0)
        {
            int randomIndex = Random.Range(0, endMessages.Length);
            string selectedMessage = endMessages[randomIndex];
            
            if (endMessageText != null)
            {
                endMessageText.text = selectedMessage;
            }
        }

        // Выбираем случайное сообщение со временем и показываем его
        if (timeMessages.Length > 0 && timeMessageText != null)
        {
            int randomTimeIndex = Random.Range(0, timeMessages.Length);
            string timeMessage = string.Format(timeMessages[randomTimeIndex], seconds);
            timeMessageText.text = timeMessage;
        }

        // Показываем UI сообщение об окончании
        if (endMessageUI != null)
        {
            endMessageUI.SetActive(true);
        }
    }

    /// <summary>
    /// Получить время с момента спавна до окончания сценария
    /// Если сценарий еще не закончился, возвращает текущее время с начала
    /// </summary>
    public float GetScenarioTime()
    {
        if (!isScenarioActive)
        {
            if (scenarioEndTime > 0f)
            {
                // Сценарий закончился - возвращаем время до окончания
                return scenarioEndTime - scenarioStartTime;
            }
            else
            {
                // Сценарий еще не начинался
                return 0f;
            }
        }
        else
        {
            // Сценарий активен - возвращаем текущее время
            return Time.time - scenarioStartTime;
        }
    }

    /// <summary>
    /// Проверка активен ли сценарий
    /// </summary>
    public bool IsScenarioActive()
    {
        return isScenarioActive;
    }
    
    /// <summary>
    /// Восстановить все объекты Damaged в начальное состояние
    /// </summary>
    private void RestoreAllDamaged()
    {
        if (cachedDamagedObjects == null) return;
        
        foreach (var damaged in cachedDamagedObjects)
        {
            if (damaged != null)
            {
                damaged.Reset();
            }
        }
        
        Debug.Log($"Scenario: Restored {cachedDamagedObjects.Length} Damaged objects");
    }
}

