using UnityEngine;

/// <summary>
/// Утилита для расчета траектории и точки падения снаряда
/// </summary>
public static class ProjectileTrajectoryCalculator
{
    /// <summary>
    /// Рассчитать точку падения снаряда без учета сопротивления воздуха (drag = 0)
    /// </summary>
    /// <param name="startPosition">Начальная позиция снаряда</param>
    /// <param name="initialVelocity">Начальная скорость снаряда</param>
    /// <param name="gravity">Вектор гравитации (по умолчанию Physics.gravity)</param>
    /// <param name="groundLevel">Уровень земли по Y (по умолчанию 0)</param>
    /// <returns>Точка падения в мировых координатах, или Vector3.zero если снаряд не упадет</returns>
    public static Vector3 CalculateLandingPoint(Vector3 startPosition, Vector3 initialVelocity, Vector3? gravity = null, float groundLevel = 0f)
    {
        Vector3 g = gravity ?? Physics.gravity;
        
        // Если гравитация не направлена вниз или равна нулю, снаряд не упадет
        if (g.y >= 0 || Mathf.Approximately(g.y, 0f))
        {
            Debug.LogWarning("ProjectileTrajectoryCalculator: Gravity is not pointing down or is zero!");
            return Vector3.zero;
        }
        
        float v0y = initialVelocity.y; // Вертикальная компонента начальной скорости
        float y0 = startPosition.y;    // Начальная высота
        float gy = g.y;                // Вертикальная компонента гравитации (отрицательная)
        
        // Квадратное уравнение для времени падения: 0.5 * gy * t^2 + v0y * t + (y0 - groundLevel) = 0
        // Приводим к виду: a*t^2 + b*t + c = 0
        float a = 0.5f * gy;
        float b = v0y;
        float c = y0 - groundLevel;
        
        // Дискриминант
        float discriminant = b * b - 4f * a * c;
        
        // Если дискриминант отрицательный, снаряд никогда не достигнет уровня земли
        if (discriminant < 0)
        {
            Debug.LogWarning("ProjectileTrajectoryCalculator: Projectile will never reach ground level!");
            return Vector3.zero;
        }
        
        // Находим время падения (берем положительный корень)
        float t1 = (-b + Mathf.Sqrt(discriminant)) / (2f * a);
        float t2 = (-b - Mathf.Sqrt(discriminant)) / (2f * a);
        
        // Берем положительное время
        float timeToLand = Mathf.Max(t1, t2);
        if (timeToLand < 0)
        {
            // Оба корня отрицательны - снаряд уже ниже уровня земли или движется вверх от уровня земли
            return Vector3.zero;
        }
        
        // Рассчитываем горизонтальное смещение
        Vector3 horizontalVelocity = new Vector3(initialVelocity.x, 0f, initialVelocity.z);
        Vector3 horizontalDisplacement = horizontalVelocity * timeToLand;
        
        // Точка падения
        Vector3 landingPoint = startPosition + horizontalDisplacement;
        landingPoint.y = groundLevel;
        
        return landingPoint;
    }
    
    /// <summary>
    /// Рассчитать точку падения снаряда с учетом сопротивления воздуха (drag)
    /// Использует численное интегрирование для более точного расчета
    /// </summary>
    /// <param name="startPosition">Начальная позиция снаряда</param>
    /// <param name="initialVelocity">Начальная скорость снаряда</param>
    /// <param name="drag">Коэффициент сопротивления воздуха (Rigidbody.drag)</param>
    /// <param name="gravity">Вектор гравитации (по умолчанию Physics.gravity)</param>
    /// <param name="groundLevel">Уровень земли по Y (по умолчанию 0)</param>
    /// <param name="maxTime">Максимальное время для симуляции (по умолчанию 60 секунд)</param>
    /// <param name="timeStep">Шаг времени для симуляции (по умолчанию 0.01 секунды)</param>
    /// <returns>Точка падения в мировых координатах, или Vector3.zero если снаряд не упадет за maxTime</returns>
    public static Vector3 CalculateLandingPointWithDrag(Vector3 startPosition, Vector3 initialVelocity, float drag, Vector3? gravity = null, float groundLevel = 0f, float maxTime = 60f, float timeStep = 0.01f)
    {
        Vector3 g = gravity ?? Physics.gravity;
        Vector3 position = startPosition;
        Vector3 velocity = initialVelocity;
        float time = 0f;
        
        while (time < maxTime)
        {
            // Обновляем скорость с учетом сопротивления воздуха: v = v * (1 - drag * dt)
            velocity = velocity * (1f - drag * timeStep);
            
            // Обновляем скорость с учетом гравитации: v = v + g * dt
            velocity = velocity + g * timeStep;
            
            // Обновляем позицию: p = p + v * dt
            position = position + velocity * timeStep;
            
            // Проверяем, достиг ли снаряд уровня земли
            if (position.y <= groundLevel)
            {
                // Интерполируем точное пересечение с уровнем земли
                float prevY = position.y - velocity.y * timeStep;
                float t = (groundLevel - prevY) / (position.y - prevY);
                position = Vector3.Lerp(position - velocity * timeStep, position, t);
                position.y = groundLevel;
                return position;
            }
            
            // Если снаряд движется вверх и скорость мала, он может не упасть
            if (velocity.y > 0 && velocity.magnitude < 0.01f)
            {
                break;
            }
            
            time += timeStep;
        }
        
        Debug.LogWarning($"ProjectileTrajectoryCalculator: Projectile did not reach ground level within {maxTime} seconds!");
        return Vector3.zero;
    }
    
    /// <summary>
    /// Рассчитать время полета до падения без учета сопротивления воздуха
    /// </summary>
    /// <param name="startPosition">Начальная позиция снаряда</param>
    /// <param name="initialVelocity">Начальная скорость снаряда</param>
    /// <param name="gravity">Вектор гравитации (по умолчанию Physics.gravity)</param>
    /// <param name="groundLevel">Уровень земли по Y (по умолчанию 0)</param>
    /// <returns>Время полета в секундах, или -1 если снаряд не упадет</returns>
    public static float CalculateTimeToLand(Vector3 startPosition, Vector3 initialVelocity, Vector3? gravity = null, float groundLevel = 0f)
    {
        Vector3 g = gravity ?? Physics.gravity;
        
        if (g.y >= 0 || Mathf.Approximately(g.y, 0f))
        {
            return -1f;
        }
        
        float v0y = initialVelocity.y;
        float y0 = startPosition.y;
        float gy = g.y;
        
        float a = 0.5f * gy;
        float b = v0y;
        float c = y0 - groundLevel;
        
        float discriminant = b * b - 4f * a * c;
        
        if (discriminant < 0)
        {
            return -1f;
        }
        
        float t1 = (-b + Mathf.Sqrt(discriminant)) / (2f * a);
        float t2 = (-b - Mathf.Sqrt(discriminant)) / (2f * a);
        
        float timeToLand = Mathf.Max(t1, t2);
        return timeToLand > 0 ? timeToLand : -1f;
    }
    
    /// <summary>
    /// Оптимизированная версия для частых вызовов (каждый кадр)
    /// Кеширует гравитацию и уровень земли, убирает лишние проверки
    /// </summary>
    /// <param name="startPosition">Начальная позиция снаряда</param>
    /// <param name="initialVelocity">Начальная скорость снаряда</param>
    /// <param name="gravityY">Вертикальная компонента гравитации (обычно отрицательная, например -9.81)</param>
    /// <param name="groundLevel">Уровень земли по Y (по умолчанию 0)</param>
    /// <returns>Точка падения в мировых координатах, или Vector3.zero если снаряд не упадет</returns>
    public static Vector3 CalculateLandingPointOptimized(Vector3 startPosition, Vector3 initialVelocity, float gravityY, float groundLevel = 0f)
    {
        // Предполагаем, что гравитация всегда направлена вниз (gravityY < 0)
        // Если это не так, нужно использовать полную версию
        
        float v0y = initialVelocity.y;
        float y0 = startPosition.y;
        
        // Квадратное уравнение: 0.5 * gy * t^2 + v0y * t + (y0 - groundLevel) = 0
        float a = 0.5f * gravityY;
        float b = v0y;
        float c = y0 - groundLevel;
        
        // Дискриминант
        float discriminant = b * b - 4f * a * c;
        
        // Быстрая проверка без Debug.LogWarning для производительности
        if (discriminant < 0f)
        {
            return Vector3.zero;
        }
        
        // Вычисляем оба корня одновременно
        float sqrtD = Mathf.Sqrt(discriminant);
        float t1 = (-b + sqrtD) / (2f * a);
        float t2 = (-b - sqrtD) / (2f * a);
        
        float timeToLand = t1 > t2 ? t1 : t2;
        if (timeToLand < 0f)
        {
            return Vector3.zero;
        }
        
        // Горизонтальное смещение (избегаем создания промежуточных Vector3 где возможно)
        float hx = initialVelocity.x * timeToLand;
        float hz = initialVelocity.z * timeToLand;
        
        return new Vector3(startPosition.x + hx, groundLevel, startPosition.z + hz);
    }
}

