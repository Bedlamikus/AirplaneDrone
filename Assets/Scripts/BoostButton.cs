using UnityEngine;
using UnityEngine.EventSystems;

public class BoostButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    public bool isBoost = false;

    public void OnPointerDown(PointerEventData eventData)
    {
        isBoost = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isBoost = false;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isBoost = false;
    }
}
