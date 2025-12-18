using UnityEngine;
using UnityEngine.Events;

public static class GlobalEvents
{
    // Событие пролета через цель с id цели
    public static UnityEvent<int> OnTargetPassed = new UnityEvent<int>();
    
    // Событие завершения всех целей
    public static UnityEvent OnAllTargetsCompleted = new UnityEvent();
    
    // Событие старта нового сценария (передается индекс сценария, -1 означает запуск следующего)
    public static UnityEvent<int> OnStartNewScenario = new UnityEvent<int>();
}

