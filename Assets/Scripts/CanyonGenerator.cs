using UnityEngine;

/// <summary>
/// Генератор процедурного каньона
/// </summary>
public static class CanyonGenerator
{
    // Параметры генерации каньона
    private const float CANYON_WIDTH = 40f; // Ширина каньона в блоках
    private const float CANYON_HEIGHT = 25f; // Высота каньона в блоках
    private const float WALL_HEIGHT = 5f; // Высота стен над дном каньона
    private const float BRANCH_PROBABILITY = 0.15f; // Вероятность ветвления на каждом чанке
    private const float CURVE_INTENSITY = 0.3f; // Интенсивность изгибов
    private const float NOISE_SCALE = 0.1f; // Масштаб шума для неровностей
    
    /// <summary>
    /// Генерирует весь мир-каньон
    /// </summary>
    public static void GenerateCanyon(WorldData worldData, int seed = 0)
    {
        Random.InitState(seed);
        
        // Генерируем путь каньона (центр каньона по Z)
        float[] canyonCenterX = new float[WorldData.WORLD_DEPTH];
        float[] canyonWidth = new float[WorldData.WORLD_DEPTH];
        
        // Начальная позиция каньона (по центру по X)
        float currentX = WorldData.WORLD_WIDTH / 2f;
        float currentWidth = CANYON_WIDTH;
        
        // Генерируем направление движения каньона (для плавных изгибов)
        float direction = 0f; // Направление изменения X
        float targetDirection = 0f;
        
        // Генерируем путь каньона с изгибами и ветвлениями
        for (int z = 0; z < WorldData.WORLD_DEPTH; z++)
        {
            // Плавно изменяем направление
            if (z % 5 == 0) // Каждые 5 блоков обновляем целевое направление
            {
                targetDirection = (Random.value - 0.5f) * CURVE_INTENSITY;
            }
            
            // Интерполируем направление для плавности
            direction = Mathf.Lerp(direction, targetDirection, 0.1f);
            currentX += direction;
            
            // Ограничиваем, чтобы каньон не выходил за границы
            currentX = Mathf.Clamp(currentX, CANYON_WIDTH * 0.5f + 5f, WorldData.WORLD_WIDTH - CANYON_WIDTH * 0.5f - 5f);
            
            canyonCenterX[z] = currentX;
            canyonWidth[z] = currentWidth;
            
            // Ветвление (создаем дополнительный путь, который расширяет каньон)
            if (Random.value < BRANCH_PROBABILITY && z < WorldData.WORLD_DEPTH - 50 && z > 10)
            {
                // Создаем ветвь, которая расширяет каньон в сторону
                float branchOffset = (Random.value - 0.5f) * CANYON_WIDTH * 0.6f;
                float branchX = currentX + branchOffset;
                branchX = Mathf.Clamp(branchX, CANYON_WIDTH * 0.5f + 5f, WorldData.WORLD_WIDTH - CANYON_WIDTH * 0.5f - 5f);
                
                // Генерируем ветвь на следующие 15-30 блоков
                int branchLength = Random.Range(15, 30);
                for (int bz = 1; bz < branchLength && z + bz < WorldData.WORLD_DEPTH; bz++)
                {
                    if (z + bz < canyonCenterX.Length)
                    {
                        // Плавно смешиваем основной путь и ветвь
                        float blend = Mathf.SmoothStep(1f, 0f, bz / (float)branchLength);
                        float blendedX = Mathf.Lerp(canyonCenterX[z + bz], branchX, blend * 0.4f);
                        canyonCenterX[z + bz] = blendedX;
                    }
                }
            }
        }
        
        // Сглаживаем путь для устранения резких переходов
        for (int smoothPass = 0; smoothPass < 2; smoothPass++)
        {
            for (int z = 1; z < WorldData.WORLD_DEPTH - 1; z++)
            {
                canyonCenterX[z] = (canyonCenterX[z - 1] + canyonCenterX[z] + canyonCenterX[z + 1]) / 3f;
            }
        }
        
        // Генерируем блоки
        for (int x = 0; x < WorldData.WORLD_WIDTH; x++)
        {
            for (int z = 0; z < WorldData.WORLD_DEPTH; z++)
            {
                float centerX = canyonCenterX[z];
                float width = canyonWidth[z];
                float distanceFromCenter = Mathf.Abs(x - centerX);
                
                // Определяем, находится ли позиция внутри каньона
                bool isInCanyon = distanceFromCenter < width / 2f;
                
                if (isInCanyon)
                {
                    // Внутри каньона - создаем дно и воздух
                    int bottomY = Mathf.RoundToInt(CANYON_HEIGHT);
                    
                    // Дно каньона (блоки)
                    for (int y = 0; y <= bottomY; y++)
                    {
                        int blockType = WorldData.BLOCK_STONE;
                        if (y == bottomY)
                        {
                            blockType = WorldData.BLOCK_GRASS; // Верхний слой - трава
                        }
                        else if (y >= bottomY - 2)
                        {
                            blockType = WorldData.BLOCK_DIRT; // Несколько слоев земли
                        }
                        
                        worldData.SetBlock(x, y, z, blockType);
                    }
                    
                    // Воздух выше дна
                    for (int y = bottomY + 1; y < WorldData.WORLD_HEIGHT; y++)
                    {
                        worldData.SetBlock(x, y, z, WorldData.BLOCK_AIR);
                    }
                }
                else
                {
                    // Вне каньона - заполняем каменными блоками
                    // Высота стен зависит от расстояния от центра каньона
                    float wallHeight = CANYON_HEIGHT + WALL_HEIGHT;
                    float distanceFactor = (distanceFromCenter - width / 2f) / (WorldData.WORLD_WIDTH / 2f);
                    int maxY = Mathf.RoundToInt(wallHeight * (1f - distanceFactor * 0.3f));
                    
                    for (int y = 0; y < maxY && y < WorldData.WORLD_HEIGHT; y++)
                    {
                        int blockType = WorldData.BLOCK_STONE;
                        if (y == maxY - 1 && maxY > CANYON_HEIGHT + 2)
                        {
                            blockType = WorldData.BLOCK_GRASS;
                        }
                        else if (y >= maxY - 3 && maxY > CANYON_HEIGHT + 2)
                        {
                            blockType = WorldData.BLOCK_DIRT;
                        }
                        
                        worldData.SetBlock(x, y, z, blockType);
                    }
                    
                    // Воздух выше стен
                    for (int y = maxY; y < WorldData.WORLD_HEIGHT; y++)
                    {
                        worldData.SetBlock(x, y, z, WorldData.BLOCK_AIR);
                    }
                }
            }
        }
        
        // Добавляем bedrock в самый низ
        for (int x = 0; x < WorldData.WORLD_WIDTH; x++)
        {
            for (int z = 0; z < WorldData.WORLD_DEPTH; z++)
            {
                worldData.SetBlock(x, 0, z, WorldData.BLOCK_BEDROCK);
            }
        }
        
        // Добавляем стены по краям мира, чтобы самолет не мог вылететь
        // Стены по X (левая и правая)
        for (int y = 0; y < WorldData.WORLD_HEIGHT; y++)
        {
            for (int z = 0; z < WorldData.WORLD_DEPTH; z++)
            {
                // Левая стена (x = 0)
                worldData.SetBlock(0, y, z, WorldData.BLOCK_STONE);
                // Правая стена (x = WORLD_WIDTH - 1)
                worldData.SetBlock(WorldData.WORLD_WIDTH - 1, y, z, WorldData.BLOCK_STONE);
            }
        }
        
        // Стены по Z (передняя и задняя)
        for (int x = 0; x < WorldData.WORLD_WIDTH; x++)
        {
            for (int y = 0; y < WorldData.WORLD_HEIGHT; y++)
            {
                // Передняя стена (z = 0)
                worldData.SetBlock(x, y, 0, WorldData.BLOCK_STONE);
                // Задняя стена (z = WORLD_DEPTH - 1)
                worldData.SetBlock(x, y, WorldData.WORLD_DEPTH - 1, WorldData.BLOCK_STONE);
            }
        }
        
        // Верхняя стена (y = WORLD_HEIGHT - 1) - крыша
        for (int x = 0; x < WorldData.WORLD_WIDTH; x++)
        {
            for (int z = 0; z < WorldData.WORLD_DEPTH; z++)
            {
                worldData.SetBlock(x, WorldData.WORLD_HEIGHT - 1, z, WorldData.BLOCK_STONE);
            }
        }
    }
}

