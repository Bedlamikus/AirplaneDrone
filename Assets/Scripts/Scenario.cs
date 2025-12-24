using UnityEngine;
using UnityEngine.Events;
using System.Collections;

public class Scenario : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform airplaneSpawnPoint; // Точка спавна самолета
    [SerializeField] private AirplaneController airplane; // Ссылка на самолет
    [SerializeField] private WorldGenerator worldGenerator; // Генератор мира (дочерний объект)
    private ScenarioManager scenarioManager; // Ссылка на менеджер сценариев
    
    /// <summary>
    /// Локальное событие завершения генерации мира для этого сценария
    /// </summary>
    public UnityEvent OnWorldGenerationComplete = new UnityEvent();
    
    /// <summary>
    /// Установить ссылку на менеджер сценариев
    /// </summary>
    public void SetScenarioManager(ScenarioManager manager)
    {
        scenarioManager = manager;
    }

    [Header("Scenario State")]
    private bool isScenarioActive = false;
    private float scenarioStartTime = 0f;
    private float scenarioEndTime = 0f;

    private void Awake()
    {
        worldGenerator.EndGeneration.AddListener(StartScenario);
        worldGenerator.GenerateWorld();
    }

    /// <summary>
    /// Запуск сценария (вызывается при спавне самолета)
    /// </summary>
    public void StartScenario()
    {
        isScenarioActive = true;
        scenarioStartTime = Time.time;
        scenarioEndTime = 0f;
        FindObjectOfType<AirplaneController>().spawnPosition = airplaneSpawnPoint;

        if (worldGenerator.isGenerating == true) return;

        StartCoroutine(WaitSecondAndStartScenario());
    }

    private IEnumerator WaitSecondAndStartScenario()
    {
        yield return null;
        GlobalEvents.OnScenarioStart.Invoke();
    }

    /// <summary>
    /// Окончание сценария
    /// </summary>
    public void EndScenario()
    {
        if (!isScenarioActive) return;

        isScenarioActive = false;
        scenarioEndTime = Time.time;

        // Показываем сообщение об окончании через менеджер
        if (scenarioManager != null)
        {
            scenarioManager.ShowEndScenarioMessage(this);
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
    
}

