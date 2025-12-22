using UnityEngine;
using System.Collections;

public class Damaged : MonoBehaviour
{
    [SerializeField] private Rigidbody rb;
    [SerializeField] private float forceMultiplier = 1f; // Множитель силы удара
    [SerializeField] private float hideDelay = 5f; // Задержка перед скрытием после получения урона
    
    private bool hasTakenDamage = false;
    private Vector3 initialPosition; // Начальная позиция
    private Quaternion initialRotation; // Начальный поворот

    private void Start()
    {
        // Запоминаем начальную позицию и поворот
        initialPosition = transform.position;
        initialRotation = transform.rotation;
        
        // Получаем или добавляем Rigidbody
        if (rb == null)
        {
            rb = GetComponent<Rigidbody>();
        }
        
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }
        
        // Убеждаемся, что Rigidbody kinematic до получения урона
        rb.isKinematic = true;
    }
    
    /// <summary>
    /// Восстановить объект в начальное состояние
    /// </summary>
    public void Reset()
    {
        hasTakenDamage = false;
        
        // Восстанавливаем позицию и поворот
        transform.position = initialPosition;
        transform.rotation = initialRotation;
        
        // Восстанавливаем Rigidbody
        if (rb != null)
        {
            // Для kinematic объектов нельзя устанавливать velocity и angularVelocity
            // Сначала отключаем kinematic, сбрасываем скорости, потом включаем обратно
            rb.isKinematic = false;
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
        }
        
        // Показываем объект
        gameObject.SetActive(true);
    }

    /// <summary>
    /// Применить урон к объекту
    /// </summary>
    /// <param name="damagePosition">Позиция источника урона (для расчета направления силы)</param>
    /// <param name="force">Сила удара</param>
    public void TakeDamage(Vector3 damagePosition, float force = 1f)
    {
        // Предотвращаем повторное получение урона
        if (hasTakenDamage) return;
        
        hasTakenDamage = true;
        
        if (rb == null)
        {
            Debug.LogWarning($"Damaged: Rigidbody is null on {gameObject.name}");
            return;
        }
        
        // Отключаем kinematic режим
        rb.isKinematic = false;
        
        // Вычисляем направление от источника урона к объекту
        Vector3 direction = (transform.position - damagePosition).normalized;
        
        // Если объект находится в той же позиции, используем случайное направление
        if (direction == Vector3.zero)
        {
            direction = Random.onUnitSphere;
        }
        
        // Применяем силу следующим кадром
        StartCoroutine(ApplyForceNextFrame(direction, force * forceMultiplier));
        
        // Запускаем корутину скрытия через 5 секунд
        StartCoroutine(HideAfterDelay());
        
        Debug.Log($"Damaged: {gameObject.name} took damage from position {damagePosition}, applying force: {direction * force * forceMultiplier}");
    }
    
    private IEnumerator HideAfterDelay()
    {
        yield return new WaitForSeconds(hideDelay);
        // Скрываем объект вместо удаления
        gameObject.SetActive(false);
    }
    
    private System.Collections.IEnumerator ApplyForceNextFrame(Vector3 direction, float force)
    {
        yield return null; // Ждем следующий кадр
        
        if (rb != null)
        {
            rb.AddForce(direction * force, ForceMode.Impulse);
        }
    }
}

