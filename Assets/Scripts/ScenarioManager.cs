using UnityEngine;
using System.Collections.Generic;

public class ScenarioManager : MonoBehaviour
{
    [Header("Scenario Settings")]
    [SerializeField] private List<Scenario> scenarios = new List<Scenario>(); // Список сценариев

    private int currentScenarioIndex = 0; // Индекс текущего сценария

    private void OnEnable()
    {
        GlobalEvents.OnStartNewScenario.AddListener(OnStartNewScenario);
        GlobalEvents.OnRestartCurrentScenario.AddListener(OnRestartCurrentScenario);
    }

    private void OnDisable()
    {
        GlobalEvents.OnStartNewScenario.RemoveListener(OnStartNewScenario);
        GlobalEvents.OnRestartCurrentScenario.RemoveListener(OnRestartCurrentScenario);
    }

    private void Awake()
    {
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
        RestartCurrentScenario();
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
            Debug.LogWarning($"ScenarioManager: Invalid scenario index {index}! Available scenarios: {scenarios.Count}");
            return;
        }

        Scenario scenario = scenarios[index];
        if (scenario == null)
        {
            Debug.LogWarning($"ScenarioManager: Scenario at index {index} is null!");
            return;
        }

        // Активируем текущий сценарий
        scenario.gameObject.SetActive(true);

        Debug.Log($"ScenarioManager: Starting scenario {index}");

        // Респавним самолет в точке спавна сценария
        // (ШАГ а - скрытие UI/шума происходит в обработчиках OnRestartCurrentScenario,
        //  которые вызываются ПЕРЕД этим методом через событие от кнопки)
        scenario.RespawnAirplane();

        // Запускаем сценарий
        scenario.StartScenario();
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
            // Перезапускаем найденный активный сценарий
            StartScenario(activeScenarioIndex);
        }
        else
        {
            Debug.LogWarning("ScenarioManager: No active scenario found to restart!");
        }
    }
}

