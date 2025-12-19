using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Camera Settings")]
    [SerializeField] private float mouseSensitivity = 2f;

    [Header("Offset")]
    [SerializeField] private Vector3 offset = new Vector3(0f, 2f, -4f);

    [SerializeField] private Transform targetCamera;

    [SerializeField] private InputPlayer inputPlayer;
    
    private void Update()
    {
        if (inputPlayer == null) return;

        // Вращение камеры при зажатой правой кнопке мыши или сенсорном вводе
        if (inputPlayer.RightMouseButton || inputPlayer.IsCameraRotating)
        {
            float mouseX = inputPlayer.MouseX;
            float mouseY = inputPlayer.MouseY;
            
            // Вращение по горизонтали (ось Y) и вертикали (ось X) в локальных координатах
            Vector3 currentRotation = transform.localEulerAngles;
            currentRotation.x -= mouseY * mouseSensitivity;
            currentRotation.y += mouseX * mouseSensitivity;
            transform.localEulerAngles = currentRotation;
        }
    }


    private void LateUpdate()
    {
        if (inputPlayer == null) return;
        // Обновление позиции камеры с учетом смещения
        transform.position = inputPlayer.transform.position;
        targetCamera.localPosition = offset;
    }
}

