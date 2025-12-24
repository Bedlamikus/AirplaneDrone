using UnityEngine;

/// <summary>
/// Триггер для окончания сценария
/// При попадании самолета в триггер вызывает событие окончания сценария
/// </summary>
[RequireComponent(typeof(Collider))]
public class EndScenarioTrigger : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("Однократный триггер (деактивируется после срабатывания)")]
    [SerializeField] private bool oneTimeTrigger = true;
    
    private bool hasTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        // Проверяем, что это самолет
        var airplane = other.GetComponent<AirplaneController>();
        if (airplane == null) return;
        
        // Если триггер однократный и уже сработал, игнорируем
        if (oneTimeTrigger && hasTriggered) return;
        
        // Вызываем событие окончания сценария
        GlobalEvents.OnScenarioEnd?.Invoke();
        
        hasTriggered = true;
        
        Debug.Log("EndScenarioTrigger: Scenario end triggered by airplane");
        
        // Деактивируем триггер, если он однократный
        if (oneTimeTrigger)
        {
            gameObject.SetActive(false);
        }
    }
    
    /// <summary>
    /// Сбросить состояние триггера (для повторного использования)
    /// </summary>
    public void ResetTrigger()
    {
        hasTriggered = false;
        gameObject.SetActive(true);
    }
}

