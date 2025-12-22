using UnityEngine;

public class Damaged : MonoBehaviour
{
    [SerializeField] private Rigidbody rb;
    [SerializeField] private float forceMultiplier = 1f; // Множитель силы удара
    
    private bool hasTakenDamage = false;

    private void Start()
    {
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
        
        Debug.Log($"Damaged: {gameObject.name} took damage from position {damagePosition}, applying force: {direction * force * forceMultiplier}");
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

