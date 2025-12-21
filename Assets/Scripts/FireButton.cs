using UnityEngine;
using UnityEngine.EventSystems;

public class FireButton : MonoBehaviour, IPointerDownHandler
{
    public void OnPointerDown(PointerEventData eventData)
    {
        // Вызываем событие Fire при нажатии на кнопку
        GlobalEvents.OnFire?.Invoke();
    }
}

