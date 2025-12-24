using UnityEngine;

/// <summary>
/// Компонент для части самолета, которая может быть отделена при разрушении
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class AirplanePart : MonoBehaviour
{
    private Rigidbody partRb;
    private Transform originalParent;
    private Vector3 originalLocalPosition;
    private Quaternion originalLocalRotation;
    private bool isDetached = false;
    
    private void Awake()
    {
        partRb = GetComponent<Rigidbody>();
        
        // Сохраняем исходное состояние
        originalParent = transform.parent;
        originalLocalPosition = transform.localPosition;
        originalLocalRotation = transform.localRotation;
        
        // Изначально часть должна быть кинематической (прикреплена к самолету)
        if (partRb != null)
        {
            partRb.isKinematic = true;
            partRb.useGravity = false;
        }
    }
    
    /// <summary>
    /// Отделить часть от самолета (разрушение)
    /// </summary>
    public void Detach(Vector3 explosionPoint, float explosionForce)
    {
        Debug.Log($"Part {name}: isDetached = {isDetached}");
        if (isDetached) return;
        
        isDetached = true;
        
        // Отделяем от родителя
        transform.SetParent(null);
        
        // Активируем физику
        if (partRb != null)
        {
            partRb.isKinematic = false;
            partRb.useGravity = true;
            
            // Применяем силу взрыва
            Vector3 direction = (transform.position - explosionPoint).normalized;
            partRb.AddForce(direction * explosionForce, ForceMode.Impulse);
            
            // Добавляем случайное вращение
            partRb.AddTorque(Random.insideUnitSphere * explosionForce * 0.5f, ForceMode.Impulse);
        }
    }
    
    /// <summary>
    /// Присоединить часть обратно к самолету (сборка)
    /// </summary>
    public void Attach()
    {
        if (!isDetached) return;
        
        isDetached = false;
        
        // Сначала делаем часть кинематической
        if (partRb != null)
        {
            partRb.isKinematic = true;
            partRb.useGravity = false;
        }
        
        // Возвращаем к родителю
        transform.SetParent(originalParent);
        
        // Восстанавливаем исходную позицию и поворот через transform
        transform.localPosition = originalLocalPosition;
        transform.localRotation = originalLocalRotation;
    }
    
    /// <summary>
    /// Проверить, отделена ли часть
    /// </summary>
    public bool IsDetached()
    {
        return isDetached;
    }
}

