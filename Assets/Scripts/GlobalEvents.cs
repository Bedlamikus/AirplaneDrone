using UnityEngine;
using UnityEngine.Events;

public static class GlobalEvents
{
    // Событие пролета через цель с id цели
    public static UnityEvent<int> OnTargetPassed = new();

    // Событие завершения всех целей
    public static UnityEvent OnAllTargetsCompleted = new();
    
    // Событие старта нового сценария (передается индекс сценария, -1 означает запуск следующего)
    public static UnityEvent<int> OnStartNewScenario = new();
    
    // Событие выхода самолета за границы
    public static UnityEvent OnAirplaneOutOfBounds = new();
    
    // Событие перезапуска текущего сценария
    public static UnityEvent OnRestartCurrentScenario = new();
    
    // Событие старта сценария (используется для сброса флагов)
    public static UnityEvent OnScenarioStart = new();
    
    // Событие завершения генерации мира
    public static UnityEvent OnWorldGenerationComplete = new();
}

