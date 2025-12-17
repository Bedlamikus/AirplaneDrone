using UnityEngine;
using UnityEngine.Events;

public static class GlobalEvents
{
    // Событие пролета через цель с id цели
    public static UnityEvent<int> OnTargetPassed = new UnityEvent<int>();
}

