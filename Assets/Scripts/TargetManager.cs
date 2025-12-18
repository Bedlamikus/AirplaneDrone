using UnityEngine;
using System.Collections.Generic;

public class TargetManager : MonoBehaviour
{
    [Header("Target Settings")]
    [SerializeField] private List<TargetPoint> targets = new List<TargetPoint>();

    private int currentTargetIndex = 0;

    private void OnEnable()
    {
        GlobalEvents.OnTargetPassed.AddListener(OnTargetPassed);
    }

    private void OnDisable()
    {
        GlobalEvents.OnTargetPassed.RemoveListener(OnTargetPassed);
    }

    private void Start()
    {
        // Сортируем цели по id для правильного порядка
        targets.Sort((a, b) => a.GetTargetId().CompareTo(b.GetTargetId()));
        
        // Проверяем, что все цели имеют уникальные id
        ValidateTargetIds();
        
        // Скрываем все цели, показываем только текущую
        UpdateTargetsVisibility();
    }

    private void ValidateTargetIds()
    {
        HashSet<int> ids = new HashSet<int>();
        foreach (var target in targets)
        {
            if (ids.Contains(target.GetTargetId()))
            {
                Debug.LogWarning($"Target with id {target.GetTargetId()} is duplicated!");
            }
            ids.Add(target.GetTargetId());
        }
    }

    private void OnTargetPassed(int targetId)
    {
        // Проверяем, что пролетели через правильную цель (по порядку)
        if (currentTargetIndex < targets.Count)
        {
            TargetPoint currentTarget = targets[currentTargetIndex];
            
            if (currentTarget.GetTargetId() == targetId)
            {
                // Правильная цель - начисляем монеты
                int reward = currentTarget.GetCoinsReward();
                PlayerData.AddCoins(reward);
                Debug.Log($"Target {targetId} passed! Coins added: {reward}. Total coins: {PlayerData.GetCoins()}");
                
                // Переходим к следующей цели
                currentTargetIndex++;
                
                // Обновляем видимость целей (скрываем все, показываем только текущую)
                UpdateTargetsVisibility();
                
                if (currentTargetIndex >= targets.Count)
                {
                    Debug.Log("All targets completed!");
                }
            }
            else
            {
                // Неправильная цель - не начисляем монеты
                Debug.Log($"Wrong target passed! Expected id: {currentTarget.GetTargetId()}, got: {targetId}. No coins awarded.");
            }
        }
    }

    /// <summary>
    /// Получить текущую цель
    /// </summary>
    public TargetPoint GetCurrentTarget()
    {
        if (currentTargetIndex < targets.Count)
        {
            return targets[currentTargetIndex];
        }
        return null;
    }

    /// <summary>
    /// Получить индекс текущей цели
    /// </summary>
    public int GetCurrentTargetIndex()
    {
        return currentTargetIndex;
    }

    /// <summary>
    /// Сбросить все цели
    /// </summary>
    public void ResetAllTargets()
    {
        currentTargetIndex = 0;
        foreach (var target in targets)
        {
            target.ResetTarget();
        }
        // Обновляем видимость после сброса
        UpdateTargetsVisibility();
    }

    /// <summary>
    /// Добавить цель в список (для динамического добавления)
    /// </summary>
    public void AddTarget(TargetPoint target)
    {
        if (!targets.Contains(target))
        {
            targets.Add(target);
            targets.Sort((a, b) => a.GetTargetId().CompareTo(b.GetTargetId()));
        }
    }

    /// <summary>
    /// Обновить видимость целей - скрыть все, показать только текущую
    /// </summary>
    private void UpdateTargetsVisibility()
    {
        for (int i = 0; i < targets.Count; i++)
        {
            if (targets[i] != null)
            {
                // Показываем только текущую цель, остальные скрываем
                targets[i].SetActive(i == currentTargetIndex);
            }
        }
    }
}

