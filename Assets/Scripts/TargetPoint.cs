using UnityEngine;

public class TargetPoint : MonoBehaviour
{
    [Header("Target Settings")]
    [SerializeField] private int targetId;
    [SerializeField] private int coinsReward = 10;

    private bool isPassed = false;

    private void OnTriggerEnter(Collider other)
    {
        // Проверяем, что цель еще не пройдена
        if (isPassed) return;

        AirplaneController airplane = other.GetComponent<AirplaneController>();
        if (airplane == null) return;

        isPassed = true;
        GlobalEvents.OnTargetPassed?.Invoke(targetId);
    }

    /// <summary>
    /// Получить id цели
    /// </summary>
    public int GetTargetId()
    {
        return targetId;
    }

    /// <summary>
    /// Получить награду за цель
    /// </summary>
    public int GetCoinsReward()
    {
        return coinsReward;
    }

    /// <summary>
    /// Сбросить состояние цели (для повторного использования)
    /// </summary>
    public void ResetTarget()
    {
        isPassed = false;
        gameObject.SetActive(true);
    }

    public void SetActive(bool flag)
    {
        transform.parent.gameObject.SetActive(flag);
    }
}

