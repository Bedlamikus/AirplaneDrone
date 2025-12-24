using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class ScenarioManager : MonoBehaviour
{
    [Header("Scenario Settings")]
    [SerializeField] private List<Scenario> scenarios = new List<Scenario>(); // Список сценариев

    [Header("UI Messages")]
    [SerializeField] private TMP_Text endMessageText; // UI текст сообщения об окончании сценария
    [SerializeField] private TMP_Text timeMessageText; // UI текст сообщения со временем прохождения
    [SerializeField] private GameObject endMessageUI; // UI объект сообщения (GameObject с TMP_Text)
    [SerializeField] private TMP_Text outOfBoundsMessageText; // UI текст сообщения о выходе за границы / разрушении
    [SerializeField] private GameObject outOfBoundsMessageUI; // UI объект сообщения о выходе за границы / разрушении

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

    private readonly string[] destroyedMessages = new string[]
    {
        "Airplane destroyed! Let's start over!",
        "Crash! Time to restart!",
        "The airplane is destroyed. Let's begin again!",
        "Destruction! Starting fresh!",
        "Airplane crashed! Let's try again!",
        "The plane is destroyed. Let's start from the beginning!",
        "Crash landing! Time to restart!",
        "Airplane destroyed! Beginning anew!",
        "The aircraft is destroyed. Let's start over!",
        "Crash! Let's begin again!",
        "Airplane destroyed! Starting fresh!",
        "The plane crashed! Let's try again!",
        "Destruction! Let's start from the beginning!",
        "Airplane destroyed! Time to restart!",
        "Crash! Let's start over!",
        "The airplane is destroyed. Beginning anew!",
        "Airplane crashed! Let's begin again!",
        "Destruction! Let's try again!",
        "The plane is destroyed. Starting fresh!",
        "Crash landing! Let's start from the beginning!"
    };

    private int currentScenarioIndex = 0; // Индекс текущего сценария

    private void OnEnable()
    {
        GlobalEvents.OnStartNewScenario.AddListener(OnStartNewScenario);
        GlobalEvents.OnRestartCurrentScenario.AddListener(OnRestartCurrentScenario);
        GlobalEvents.OnAirplaneOutOfBounds.AddListener(OnAirplaneOutOfBounds);
        GlobalEvents.OnAirplaneDestroyed.AddListener(OnAirplaneDestroyed);
        GlobalEvents.OnScenarioEnd.AddListener(OnScenarioEnd);
    }

    private void OnDisable()
    {
        GlobalEvents.OnStartNewScenario.RemoveListener(OnStartNewScenario);
        GlobalEvents.OnRestartCurrentScenario.RemoveListener(OnRestartCurrentScenario);
        GlobalEvents.OnAirplaneOutOfBounds.RemoveListener(OnAirplaneOutOfBounds);
        GlobalEvents.OnAirplaneDestroyed.RemoveListener(OnAirplaneDestroyed);
        GlobalEvents.OnScenarioEnd.RemoveListener(OnScenarioEnd);
    }

    private void Awake()
    {
        // Выключаем сообщение об окончании в начале
        if (endMessageUI != null)
        {
            endMessageUI.SetActive(false);
        }
        
        // Выключаем сообщение о выходе за границы / разрушении в начале
        if (outOfBoundsMessageUI != null)
        {
            outOfBoundsMessageUI.SetActive(false);
        }

        // Деактивируем все сценарии на старте, чтобы скрыть дочерние объекты (точки)
        foreach (var scenario in scenarios)
        {
            if (scenario != null)
            {
                scenario.gameObject.SetActive(false);
            }
        }

        StartScenario(0);
        // Устанавливаем индекс следующего сценария на 1
        currentScenarioIndex = 1;
    }

    /// <summary>
    /// Обработчик события старта нового сценария
    /// </summary>
    /// <param name="scenarioIndex">Индекс сценария для запуска. -1 означает запуск следующего по порядку</param>
    private void OnStartNewScenario(int scenarioIndex)
    {
        if (scenarioIndex == -1)
        {
            // Запускаем следующий сценарий
            StartNextScenario();
        }
        else
        {
            // Запускаем конкретный сценарий по индексу
            StartScenario(scenarioIndex);
        }
    }

    /// <summary>
    /// Обработчик события перезапуска текущего сценария
    /// </summary>
    private void OnRestartCurrentScenario()
    {
        Debug.Log("[RESTART] Step 2: ScenarioManager - OnRestartCurrentScenario event received");
        RestartCurrentScenario();
    }

    /// <summary>
    /// Обработчик события окончания сценария
    /// </summary>
    private void OnScenarioEnd()
    {
        // Находим активный сценарий и вызываем EndScenario
        for (int i = 0; i < scenarios.Count; i++)
        {
            if (scenarios[i] != null && scenarios[i].gameObject.activeSelf)
            {
                scenarios[i].EndScenario();
                break;
            }
        }
    }

    /// <summary>
    /// Запустить следующий сценарий по порядку
    /// </summary>
    public void StartNextScenario()
    {
        if (scenarios.Count == 0)
        {
            Debug.LogWarning("ScenarioManager: No scenarios in list!");
            return;
        }

        // Вычисляем индекс предыдущего сценария (который сейчас активен)
        // currentScenarioIndex указывает на следующий сценарий для запуска
        int previousIndex = (currentScenarioIndex - 1 + scenarios.Count) % scenarios.Count;
        
        // Скрываем предыдущий сценарий (текущий активный)
        if (previousIndex >= 0 && previousIndex < scenarios.Count && scenarios[previousIndex] != null)
        {
            scenarios[previousIndex].gameObject.SetActive(false);
        }

        // Запускаем следующий сценарий по текущему индексу
        int scenarioToStart = currentScenarioIndex;
        StartScenario(scenarioToStart);
        
        // Переходим к следующему индексу (с зацикливанием)
        currentScenarioIndex = (currentScenarioIndex + 1) % scenarios.Count;
    }

    /// <summary>
    /// Запустить сценарий по индексу
    /// </summary>
    /// <param name="index">Индекс сценария в списке</param>
    public void StartScenario(int index)
    {
        if (index < 0 || index >= scenarios.Count)
        {
            Debug.LogWarning($"ScenarioManager: Invalid scenario index {index}! Available scenarios: {scenarios.Count}. Starting scenario 0 instead.");
            index = 0; // Запускаем сценарий с индексом 0, если индекс невалидный
        }

        Scenario scenario = scenarios[index];
        if (scenario == null)
        {
            Debug.LogWarning($"ScenarioManager: Scenario at index {index} is null!");
            return;
        }

        // Активируем текущий сценарий
        scenario.gameObject.SetActive(true);
        
        // Устанавливаем ссылку на менеджер в сценарии
        scenario.SetScenarioManager(this);
        scenario.StartScenario();
        // Скрываем сообщения при респавне
        HideMessagesOnRespawn();
        
        // Скрываем сообщения при старте сценария
        HideAllMessages();
    }

    /// <summary>
    /// Получить текущий индекс сценария
    /// </summary>
    public int GetCurrentScenarioIndex()
    {
        return currentScenarioIndex;
    }

    /// <summary>
    /// Установить текущий индекс сценария
    /// </summary>
    public void SetCurrentScenarioIndex(int index)
    {
        if (index >= 0 && index < scenarios.Count)
        {
            currentScenarioIndex = index;
        }
    }

    /// <summary>
    /// Перезапустить текущий активный сценарий
    /// </summary>
    public void RestartCurrentScenario()
    {
        Debug.Log("[RESTART] Step 3: ScenarioManager - RestartCurrentScenario called");
        
        if (scenarios.Count == 0)
        {
            Debug.LogWarning("ScenarioManager: No scenarios in list!");
            return;
        }

        // Находим активный сценарий
        int activeScenarioIndex = -1;
        for (int i = 0; i < scenarios.Count; i++)
        {
            if (scenarios[i] != null && scenarios[i].gameObject.activeSelf)
            {
                activeScenarioIndex = i;
                break;
            }
        }

        if (activeScenarioIndex >= 0)
        {
            Debug.Log($"[RESTART] Step 4: ScenarioManager - Found active scenario at index {activeScenarioIndex}, calling StartScenario");
            // Перезапускаем найденный активный сценарий
            StartScenario(activeScenarioIndex);
        }
    }

    /// <summary>
    /// Обработчик события выхода самолета за границы
    /// </summary>
    private void OnAirplaneOutOfBounds()
    {
        ShowOutOfBoundsMessage();
    }

    /// <summary>
    /// Обработчик события разрушения самолета
    /// </summary>
    private void OnAirplaneDestroyed()
    {
        ShowDestroyedMessage();
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
    /// Показать сообщение о разрушении самолета
    /// </summary>
    private void ShowDestroyedMessage()
    {
        // Выбираем случайное сообщение о разрушении
        if (destroyedMessages.Length > 0)
        {
            int randomIndex = Random.Range(0, destroyedMessages.Length);
            string selectedMessage = destroyedMessages[randomIndex];
            
            if (outOfBoundsMessageText != null)
            {
                outOfBoundsMessageText.text = selectedMessage;
            }
        }

        // Показываем UI сообщение о разрушении
        if (outOfBoundsMessageUI != null)
        {
            outOfBoundsMessageUI.SetActive(true);
        }
    }

    /// <summary>
    /// Скрыть все сообщения
    /// </summary>
    private void HideAllMessages()
    {
        // Выключаем сообщение об окончании если оно было включено
        if (endMessageUI != null)
        {
            endMessageUI.SetActive(false);
        }
        
        // Выключаем сообщение о выходе за границы / разрушении если оно было включено
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
    /// Показать сообщение об окончании сценария
    /// </summary>
    public void ShowEndScenarioMessage(Scenario scenario)
    {
        if (scenario == null) return;

        // Получаем время прохождения сценария
        float scenarioDuration = scenario.GetScenarioTime();
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
    /// Скрыть сообщения при респавне
    /// </summary>
    public void HideMessagesOnRespawn()
    {
        // Скрываем UI сообщение
        if (outOfBoundsMessageUI != null)
        {
            outOfBoundsMessageUI.SetActive(false);
        }
        
        // Очищаем текст сообщения
        if (outOfBoundsMessageText != null)
        {
            outOfBoundsMessageText.text = "";
        }
    }
}

