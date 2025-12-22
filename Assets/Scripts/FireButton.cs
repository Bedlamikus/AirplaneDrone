using UnityEngine;
using UnityEngine.EventSystems;

public class FireButton : MonoBehaviour, IPointerDownHandler
{
    [SerializeField] private InputPlayer inputPlayer; // Ссылка на InputPlayer (самолет) для получения скорости
    
    private void Start()
    {
        // Если inputPlayer не назначен, пытаемся найти его автоматически
        if (inputPlayer == null)
        {
            inputPlayer = FindObjectOfType<InputPlayer>();
        }
    }
    
    public void OnPointerDown(PointerEventData eventData)
    {
        // СНАЧАЛА отправляем скорость самолета, ПОТОМ вызываем событие Fire
        // Это гарантирует, что снаряды получат скорость до обработки события Fire
        
        if (inputPlayer != null)
        {
            Rigidbody airplaneRb = inputPlayer.GetComponent<Rigidbody>();
            if (airplaneRb != null)
            {
                // Отправляем скорость самолета ПЕРЕД вызовом события Fire
                GlobalEvents.OnAirplaneVelocity?.Invoke(airplaneRb.velocity);
            }
        }
        
        // Вызываем событие Fire при нажатии на кнопку
        GlobalEvents.OnFire?.Invoke();
    }
}

