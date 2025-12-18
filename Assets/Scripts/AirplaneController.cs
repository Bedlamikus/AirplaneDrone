using System;
using System.Collections;
using UnityEngine;

public class AirplaneController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float forwardSpeed = 10f;
    [SerializeField] private float boostSpeed = 20f;
    [SerializeField] private float pitchSpeed = 50f;
    [SerializeField] private float rollSpeed = 50f;

    private InputPlayer inputPlayer;
    private Rigidbody rb;

    private void Start()
    {
        inputPlayer = GetComponent<InputPlayer>();
        if (inputPlayer == null)
        {
            inputPlayer = gameObject.AddComponent<InputPlayer>();
        }

        // Получаем или добавляем Rigidbody
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }

        // Настройки Rigidbody для самолета
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        rb.useGravity = false; // Отключаем гравитацию для самолета
        rb.drag = 0f; // Отключаем сопротивление воздуха для прямолинейного движения
    }

    private void FixedUpdate()
    {
        if (inputPlayer == null || rb == null) return;

        // Определяем текущую скорость в зависимости от нажатия пробела
        float currentSpeed = inputPlayer.Boost ? boostSpeed : forwardSpeed;

        // Постоянное движение вперед через velocity для правильной обработки коллизий
        rb.velocity = transform.forward * currentSpeed;

        // Вращение через AddRelativeTorque (момент силы в локальных координатах)
        Vector3 torque = Vector3.zero;
        
        // Управление наклоном носа (Pitch) - момент силы по локальной оси X
        float pitchInput = inputPlayer.Pitch;
        if (Mathf.Abs(pitchInput) > 0.01f)
        {
            torque.x = pitchInput * pitchSpeed;
        }

        // Управление креном (Roll) - момент силы по локальной оси Z
        float rollInput = inputPlayer.Roll;
        if (Mathf.Abs(rollInput) > 0.01f)
        {
            torque.z = rollInput * rollSpeed;
        }

        // Применяем момент силы в локальных координатах
        if (torque != Vector3.zero)
        {
            rb.AddRelativeTorque(torque, ForceMode.Force);
        }
    }

    private bool isDie = false;

    internal void Die()
    {
        if (isDie == true) return;
        
        isDie = true;
        StartCoroutine(DieRoutine());
    }

    private IEnumerator DieRoutine()
    {
        yield return null;
    }
}

