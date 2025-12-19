using UnityEngine;
using UnityEngine.EventSystems;

public class TouchCameraControl : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    [Header("Settings")]
    [SerializeField] private float touchSensitivity = 2f; // Чувствительность касаний (аналогично mouseSensitivity)

    // Публичные свойства для чтения данных касания
    public float TouchDeltaX { get; private set; } // Дельту движения по X
    public float TouchDeltaY { get; private set; } // Дельту движения по Y
    public bool IsTouching { get; private set; } // Флаг активного касания

    private bool isDragging = false;
    private Vector2 lastTouchPosition;

    public void OnPointerDown(PointerEventData eventData)
    {
        isDragging = true;
        IsTouching = true;
        lastTouchPosition = eventData.position;
        TouchDeltaX = 0f;
        TouchDeltaY = 0f;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging) return;

        // Вычисляем дельту движения в пикселях
        Vector2 delta = eventData.delta;

        // Нормализуем дельту относительно размера экрана для соответствия Input.GetAxis
        // Input.GetAxis("Mouse X/Y") возвращает значения относительно разрешения
        // Учитываем масштаб canvas если нужно, но обычно достаточно просто поделить на чувствительность
        TouchDeltaX = delta.x * touchSensitivity * 0.01f; // Масштабируем для соответствия мыши
        TouchDeltaY = delta.y * touchSensitivity * 0.01f;
        
        lastTouchPosition = eventData.position;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isDragging = false;
        IsTouching = false;
        TouchDeltaX = 0f;
        TouchDeltaY = 0f;
    }
}

