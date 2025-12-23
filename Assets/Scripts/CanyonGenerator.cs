using UnityEngine;

/// <summary>
/// Генератор процедурного каньона
/// </summary>
public static class CanyonGenerator
{
    // Параметры генерации каньона
    private const float CANYON_WIDTH = 40f; // Ширина каньона в блоках
    private const float CANYON_BOTTOM_HEIGHT = 5f; // Высота дна каньона (низкая ложбина)
    private const float CANYON_TOP_HEIGHT = 25f; // Высота вершин склонов
    private const float SLOPE_STEEPNESS = 0.8f; // Крутизна склонов (0-1, где 1 = вертикально)
    private const float BRANCH_PROBABILITY = 0.15f; // Вероятность ветвления на каждом чанке
    private const float CURVE_INTENSITY = 0.3f; // Интенсивность изгибов
    private const float NOISE_SCALE = 0.1f; // Масштаб шума для неровностей
    
    /// <summary>
    /// Генерирует весь мир-каньон в локальных координатах
    /// </summary>
    public static void GenerateCanyon(WorldData worldData, int seed = 0, Vector3 worldOrigin = default)
    {
        Random.InitState(seed);
        
        // Генерируем путь каньона (центр каньона по Z)
        float[] canyonCenterX = new float[WorldData.WORLD_DEPTH];
        float[] canyonWidth = new float[WorldData.WORLD_DEPTH];
        
        // Начальная позиция каньона (по центру по X) в локальных координатах
        // Учитываем позицию WorldManager, но генерируем относительно (0,0,0)
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
        
        // Генерируем блоки с наклонными склонами
        for (int x = 0; x < WorldData.WORLD_WIDTH; x++)
        {
            for (int z = 0; z < WorldData.WORLD_DEPTH; z++)
            {
                float centerX = canyonCenterX[z];
                float width = canyonWidth[z];
                float distanceFromCenter = Mathf.Abs(x - centerX);
                
                float surfaceHeight;
                
                // Проверяем, находимся ли мы внутри каньона
                if (distanceFromCenter < width / 2f)
                {
                    // ВНУТРИ КАНЬОНА: создаем наклонные склоны
                    // В центре - низкая ложбина, по краям каньона - высокие склоны
                    float normalizedDistance = distanceFromCenter / (width / 2f); // 0 в центре, 1 на краю каньона
                    
                    // Высота поверхности: в центре низко (ложбина), по краям каньона высоко (склоны)
                    // Используем квадратичную кривую для плавных склонов
                    float heightFactor = normalizedDistance * normalizedDistance; // Квадратичная кривая для плавных склонов
                    surfaceHeight = Mathf.Lerp(CANYON_BOTTOM_HEIGHT, CANYON_TOP_HEIGHT, heightFactor);
                }
                else
                {
                    // ВНЕ КАНЬОНА: высокая ровная поверхность
                    surfaceHeight = CANYON_TOP_HEIGHT;
                }
                
                // Добавляем небольшие неровности для реалистичности
                float noise = Mathf.PerlinNoise(x * NOISE_SCALE, z * NOISE_SCALE) * 1.5f - 0.75f;
                surfaceHeight += noise;
                
                int surfaceY = Mathf.RoundToInt(surfaceHeight);
                surfaceY = Mathf.Clamp(surfaceY, 1, WorldData.WORLD_HEIGHT - 1);
                
                // Заполняем блоки от дна до поверхности
                for (int y = 0; y <= surfaceY; y++)
                {
                    int blockType = WorldData.BLOCK_STONE;
                    
                    // Верхний слой - трава (только если достаточно высоко)
                    if (y == surfaceY && surfaceY > CANYON_BOTTOM_HEIGHT + 3)
                    {
                        blockType = WorldData.BLOCK_GRASS;
                    }
                    // Несколько слоев земли под травой
                    else if (y >= surfaceY - 3 && surfaceY > CANYON_BOTTOM_HEIGHT + 3)
                    {
                        blockType = WorldData.BLOCK_DIRT;
                    }
                    
                    worldData.SetBlock(x, y, z, blockType);
                }
                
                // Воздух выше поверхности
                for (int y = surfaceY + 1; y < WorldData.WORLD_HEIGHT; y++)
                {
                    worldData.SetBlock(x, y, z, WorldData.BLOCK_AIR);
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
        
        // УБИРАЕМ верхнюю стену (крышу), чтобы каньон был открыт сверху
        // Каньон должен быть открытым, а не тоннелем
        
        // Логируем один слой в разрезе для отладки
        LogWorldSlice(worldData);
    }
    
    /// <summary>
    /// Выводит в лог один слой мира в разрезе (поперечное сечение по Z)
    /// </summary>
    private static void LogWorldSlice(WorldData worldData)
    {
        int sliceZ = WorldData.WORLD_DEPTH / 2; // Берем средний слой по Z
        
        Debug.Log($"=== РАЗРЕЗ МИРА на Z={sliceZ} (поперечное сечение по X) ===");
        Debug.Log($"Размер мира: {WorldData.WORLD_WIDTH} x {WorldData.WORLD_HEIGHT} x {WorldData.WORLD_DEPTH}");
        
        // Находим максимальную высоту в этом срезе
        int maxHeight = 0;
        for (int x = 0; x < WorldData.WORLD_WIDTH; x++)
        {
            for (int y = WorldData.WORLD_HEIGHT - 1; y >= 0; y--)
            {
                if (worldData.GetBlock(x, y, sliceZ) != WorldData.BLOCK_AIR)
                {
                    if (y > maxHeight) maxHeight = y;
                    break;
                }
            }
        }
        
        // Выводим срез по Y (вид сбоку)
        Debug.Log($"\n--- Поперечное сечение по X (вид сбоку, максимальная высота: {maxHeight}) ---");
        for (int y = maxHeight; y >= 0; y--)
        {
            System.Text.StringBuilder line = new System.Text.StringBuilder();
            line.Append($"{y:00} | ");
            
            for (int x = 0; x < WorldData.WORLD_WIDTH; x++)
            {
                int blockType = worldData.GetBlock(x, y, sliceZ);
                char symbol = GetBlockSymbol(blockType);
                line.Append(symbol);
            }
            
            Debug.Log(line.ToString());
        }
        
        // Выводим легенду
        Debug.Log("\n--- Легенда ---");
        Debug.Log("  = воздух");
        Debug.Log("# = камень");
        Debug.Log("~ = трава");
        Debug.Log(". = земля");
        Debug.Log("B = bedrock");
    }
    
    /// <summary>
    /// Возвращает символ для типа блока
    /// </summary>
    private static char GetBlockSymbol(int blockType)
    {
        switch (blockType)
        {
            case WorldData.BLOCK_AIR: return ' ';
            case WorldData.BLOCK_STONE: return '#';
            case WorldData.BLOCK_GRASS: return '~';
            case WorldData.BLOCK_DIRT: return '.';
            case WorldData.BLOCK_BEDROCK: return 'B';
            default: return '?';
        }
    }
}

