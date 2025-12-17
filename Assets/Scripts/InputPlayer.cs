using UnityEngine;

public class InputPlayer : MonoBehaviour
{
    // Собранные значения ввода
    public float Pitch { get; private set; }      // W/S - наклон носа
    public float Roll { get; private set; }       // A/D - крен
    public float MouseX { get; private set; }    // Движение мыши по X
    public float MouseY { get; private set; }    // Движение мыши по Y
    public bool RightMouseButton { get; private set; } // Правая кнопка мыши
    public bool Boost { get; private set; }      // Пробел - ускорение

    private void Update()
    {
        CollectInput();
    }

    private void CollectInput()
    {
        // S - поднимает нос (pitch up), W - опускает нос (pitch down)
        Pitch = 0f;
        if (Input.GetKey(KeyCode.W))
            Pitch = 1f;  // Опускает нос
        if (Input.GetKey(KeyCode.S))
            Pitch = -1f; // Поднимает нос

        // A - поворот против часовой стрелки (roll left), D - по часовой (roll right)
        Roll = 0f;
        if (Input.GetKey(KeyCode.A))
            Roll = 1f;   // Против часовой стрелки
        if (Input.GetKey(KeyCode.D))
            Roll = -1f;  // По часовой стрелке

        // Движение мыши по горизонтали
        MouseX = Input.GetAxis("Mouse X");

        // Движение мыши по вертикали
        MouseY = Input.GetAxis("Mouse Y");

        // Правая кнопка мыши
        RightMouseButton = Input.GetMouseButton(1);

        // Пробел - ускорение
        Boost = Input.GetKey(KeyCode.Space);
    }
}

