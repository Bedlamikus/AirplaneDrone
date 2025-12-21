using UnityEngine;
using UnityEngine.UI;

public class ButtonRestartScenario : MonoBehaviour
{
    [SerializeField] private Button button; // Кнопка UI (назначается в инспекторе)

    private void Start()
    {
        // Если кнопка не назначена, пытаемся получить её из этого объекта
        if (button == null)
        {
            button = GetComponent<Button>();
        }

        // Подписываемся на событие нажатия кнопки
        if (button != null)
        {
            button.onClick.AddListener(OnButtonClick);
        }
    }

    private void OnDestroy()
    {
        // Отписываемся при уничтожении объекта
        if (button != null)
        {
            button.onClick.RemoveListener(OnButtonClick);
        }
    }

    /// <summary>
    /// Обработчик нажатия на кнопку
    /// </summary>
    private void OnButtonClick()
    {
        // Вызываем событие для перезапуска текущего сценария
        GlobalEvents.OnRestartCurrentScenario?.Invoke();
    }
}

