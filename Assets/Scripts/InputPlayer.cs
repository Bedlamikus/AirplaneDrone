using UnityEngine;

public class InputPlayer : MonoBehaviour
{
    [Header("Joystick")]
    [SerializeField] private FixedJoystick joystick; // Мобильный джойстик (назначается в инспекторе)
    [SerializeField] private bool isMobilePlatform = false;
    [SerializeField] private TouchCameraControl touchCameraControl; // Контроллер сенсорного управления камерой
    [SerializeField] private BoostButton boostButton;

    // Собранные значения ввода
    public float Pitch { get; private set; }      // W/S или джойстик Vertical - наклон носа
    public float Roll { get; private set; }       // A/D или джойстик Horizontal - крен
    public float MouseX { get; private set; }    // Движение мыши по X
    public float MouseY { get; private set; }    // Движение мыши по Y
    public bool RightMouseButton { get; private set; } // Правая кнопка мыши
    public bool Boost { get; private set; }      // Пробел - ускорение
    public bool IsCameraRotating { get; private set; } // Флаг вращения камеры (для сенсорного ввода)

    private void Start()
    {
        // Скрываем/показываем UI элементы в зависимости от платформы
        UpdateMobileUI();
    }

    private void Update()
    {
        CollectInput();
    }

    /// <summary>
    /// Обновить видимость мобильных UI элементов
    /// </summary>
    private void UpdateMobileUI()
    {
        // Управляем видимостью джойстика
        if (joystick != null)
        {
            joystick.gameObject.SetActive(isMobilePlatform);
        }

        // Управляем видимостью панели для тача
        if (touchCameraControl != null)
        {
            touchCameraControl.gameObject.SetActive(isMobilePlatform);
        }

        if (boostButton != null)
        {
            boostButton.gameObject.SetActive(isMobilePlatform);
        }
    }

    private void CollectInput()
    {
        // Управление наклоном носа (Pitch)
        // Если есть джойстик и он используется, используем его вертикальную ось
        if (isMobilePlatform)
        {
            Pitch = joystick.Direction.y; // Вертикальная ось джойстика
        }
        else
        {
            // Иначе используем клавиатуру: W - опускает нос, S - поднимает нос
            Pitch = 0f;
            if (Input.GetKey(KeyCode.W))
                Pitch = 1f;  // Опускает нос
            if (Input.GetKey(KeyCode.S))
                Pitch = -1f; // Поднимает нос
        }

        // Управление креном (Roll)
        // Если есть джойстик и он используется, используем его горизонтальную ось
        if (isMobilePlatform)
        {
            Roll = -joystick.Horizontal; // Горизонтальная ось джойстика
        }
        else
        {
            // Иначе используем клавиатуру: A - против часовой стрелки, D - по часовой
            Roll = 0f;
            if (Input.GetKey(KeyCode.A))
                Roll = 1f;   // Против часовой стрелки
            if (Input.GetKey(KeyCode.D))
                Roll = -1f;  // По часовой стрелке
        }

        // Управление камерой через мышь или сенсор
        if (isMobilePlatform && touchCameraControl != null && touchCameraControl.IsTouching)
        {
            // Используем сенсорный ввод для управления камерой
            MouseX = touchCameraControl.TouchDeltaX;
            MouseY = touchCameraControl.TouchDeltaY;
            IsCameraRotating = true;
        }
        else
        {
            // Используем мышь для управления камерой
            MouseX = Input.GetAxis("Mouse X");
            MouseY = Input.GetAxis("Mouse Y");
            IsCameraRotating = Input.GetMouseButton(1);
        }

        // Правая кнопка мыши
        RightMouseButton = Input.GetMouseButton(1);

        // Пробел - ускорение
        Boost = Input.GetKey(KeyCode.Space);
        if (isMobilePlatform == true)
            Boost = boostButton.isBoost;
    }

}

