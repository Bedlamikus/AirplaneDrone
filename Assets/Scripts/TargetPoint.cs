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

        // Проверяем, что в other.transform.parent лежит самолет
        if (other.transform.parent != null)
        {
            AirplaneController airplane = other.transform.parent.GetComponent<AirplaneController>();
            if (airplane != null)
            {
                // Цель зачтена - отправляем событие с id цели
                isPassed = true;
                GlobalEvents.OnTargetPassed?.Invoke(targetId);
                
                // Видимость цели управляется TargetManager
            }
        }
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
}

