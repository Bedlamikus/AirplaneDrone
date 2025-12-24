using UnityEngine;

/// <summary>
/// Генератор мира с горами и впадинами
/// </summary>
public static class TerrainGenerator
{
    // Базовые параметры слоев
    private const int GRASS_LAYER = 1;      // 1 слой травы
    private const int DIRT_LAYERS = 2;      // 2 слоя земли
    
    // Параметры для слоистой структуры сталагмитов
    private const int STALAGMITE_LAYER_THICKNESS = 4;  // Толщина каждого слоя в сталагмитах
    
    /// <summary>
    /// Генерирует тип блока с учетом рельефа (каньон: горы по краям, углубление в центре)
    /// </summary>
    public static BlockType GenerateBlock(int worldX, int worldY, int worldZ, int seed, float noiseScale, float heightMultiplier, int worldWidthInBlocks, float canyonWidthFactor = 1.0f)
    {
        if (worldY < 0 || worldY >= Chunk.CHUNK_HEIGHT)
        {
            return BlockType.Air;
        }
        
        // Генерируем высоту поверхности для каньона
        float baseHeight = GetCanyonHeight(worldX, worldZ, seed, noiseScale, heightMultiplier, worldWidthInBlocks, canyonWidthFactor);
        int surfaceHeight = Mathf.FloorToInt(baseHeight);
        
        // Ограничиваем высоту поверхности разумными пределами
        surfaceHeight = Mathf.Clamp(surfaceHeight, 2, Chunk.CHUNK_HEIGHT - 5);
        
        // Выше поверхности - воздух
        if (worldY > surfaceHeight)
        {
            return BlockType.Air;
        }
        
        // Слой травы (верхний)
        if (worldY == surfaceHeight)
        {
            return BlockType.Grass;
        }
        
        // Слои земли (под травой)
        if (worldY > surfaceHeight - DIRT_LAYERS)
        {
            return BlockType.Dirt;
        }
        
        // Остальное - камень
        return BlockType.Stone;
    }
    
    /// <summary>
    /// Проверяет, должен ли блок быть заменен на сталагмит (вызывается ПОСЛЕ генерации мира)
    /// Возвращает тип блока для сталагмита с учетом слоистой структуры
    /// </summary>
    public static BlockType? GetStalagmiteBlock(int worldX, int worldY, int worldZ, int seed, float stalagmiteDensity, int worldLengthInBlocks)
    {
        if (worldY < 0 || worldY >= Chunk.CHUNK_HEIGHT)
        {
            return null;
        }
        
        if (!IsStalagmite(worldX, worldY, worldZ, seed, stalagmiteDensity, worldLengthInBlocks))
        {
            return null;
        }
        
        // Возвращаем тип блока для сталагмита с учетом слоистой структуры
        return GetStalagmiteBlockType(worldY, seed);
    }
    
    /// <summary>
    /// Получить тип блока для сталагмита с учетом слоистой структуры
    /// Одинаковая раскраска по высоте для всех скал
    /// </summary>
    private static BlockType GetStalagmiteBlockType(int worldY, int seed)
    {
        // Вычисляем номер слоя на основе высоты (одинаково для всех скал)
        int layerIndex = worldY / STALAGMITE_LAYER_THICKNESS;
        
        // Используем seed для создания вариаций, но на одной высоте все скалы одинаковые
        System.Random layerRng = new System.Random(seed + layerIndex * 1000);
        float layerVariation = (float)layerRng.NextDouble();
        
        // Определяем тип блока в зависимости от слоя (одинаково для всех скал на одной высоте)
        int layerType = layerIndex % 6; // 6 типов слоев для разнообразия
        
        switch (layerType)
        {
            case 0:
                // Первый слой (низ) - камень
                return BlockType.Stone;
            case 1:
                // Второй слой - песчаник (песок)
                return BlockType.Sand;
            case 2:
                // Третий слой - камень
                return BlockType.Stone;
            case 3:
                // Четвертый слой - глина
                return BlockType.Clay;
            case 4:
                // Пятый слой - камень
                return BlockType.Stone;
            case 5:
                // Шестой слой - гравий (для разнообразия)
                if (layerVariation > 0.6f)
                {
                    return BlockType.Gravel;
                }
                return BlockType.Stone;
            default:
                return BlockType.Stone;
        }
    }
    
    /// <summary>
    /// Получить высоту поверхности для каньона (горы по краям, углубление в центре по ширине)
    /// С холмами и возвышенностями по длине, сужением/расширением каньона
    /// </summary>
    private static float GetCanyonHeight(int worldX, int worldZ, int seed, float noiseScale, float heightMultiplier, int worldWidthInBlocks, float canyonWidthFactor)
    {
        // Используем seed для смещения координат шума
        System.Random rng = new System.Random(seed);
        float offsetZ = (float)(rng.NextDouble() * 10000);
        
        // Координаты уже центрированы, центр находится в 0
        // Вычисляем расстояние от центра по X (абсолютное значение)
        float distanceFromCenter = Mathf.Abs(worldX);
        float maxDistance = worldWidthInBlocks / 2f;
        
        // Генерируем ширину каньона вдоль оси Z (сужение/расширение)
        float canyonWidthNoiseZ = (worldZ + offsetZ) * noiseScale * 0.3f; // Низкая частота для плавных изменений
        float canyonWidthVariation = Mathf.PerlinNoise(0, canyonWidthNoiseZ);
        // canyonWidthVariation от 0 до 1, преобразуем в диапазон 0.5-1.5 (сужение/расширение)
        float currentCanyonWidth = 0.5f + canyonWidthVariation * 1.0f; // От 0.5 до 1.5
        
        // Применяем фактор ширины каньона
        float effectiveMaxDistance = maxDistance * canyonWidthFactor * currentCanyonWidth;
        float normalizedDistance = Mathf.Clamp01(distanceFromCenter / effectiveMaxDistance);
        
        // Базовая высота углубления в центре (минимальная) с вариациями по длине
        float baseCanyonHeight = 3f;
        
        // Добавляем холмы и возвышенности по длине (ось Z)
        float hillNoiseZ = (worldZ + offsetZ) * noiseScale * 0.2f; // Низкая частота для крупных холмов
        float hillVariation = Mathf.PerlinNoise(0, hillNoiseZ);
        float canyonBaseHeight = baseCanyonHeight + (hillVariation - 0.3f) * heightMultiplier * 0.4f; // Холмы в центре каньона
        
        // Высота гор по краям (максимальная или почти максимальная) с вариациями
        float mountainBaseHeight = Chunk.CHUNK_HEIGHT - 2f;
        float mountainHeight = mountainBaseHeight + (hillVariation - 0.5f) * heightMultiplier * 0.3f; // Вариации высоты гор
        
        // Используем более крутую функцию для резкого перехода от углубления к горам
        // normalizedDistance = 0 в центре (низко), = 1 по краям (высоко)
        // Используем степенную функцию с большим показателем для очень крутого спуска
        float canyonProfile = Mathf.Pow(normalizedDistance, 5f);
        
        float height = canyonBaseHeight + canyonProfile * (mountainHeight - canyonBaseHeight);
        
        // Добавляем мелкие детали для реалистичности
        float noiseDetailZ = (worldZ + offsetZ) * noiseScale * 2f;
        float noiseDetail = Mathf.PerlinNoise(0, noiseDetailZ);
        height += (noiseDetail - 0.5f) * heightMultiplier * 0.15f;
        
        return height;
    }
    
    /// <summary>
    /// Проверить, находится ли блок внутри сталагмита (вертикальной скалы с конусной формой)
    /// Сталагмиты генерируются полностью независимо, используя только абсолютные координаты и seed
    /// </summary>
    private static bool IsStalagmite(int worldX, int worldY, int worldZ, int seed, float stalagmiteDensity, int worldLengthInBlocks)
    {
        // Вычисляем начало мира по Z (координаты центрированы)
        int worldStartZ = -(worldLengthInBlocks / 2);
        
        // Первые два ряда чанков (32 блока) от начала мира по длине без скал
        // 2 ряда чанков = 2 * CHUNK_SIZE = 32 блока
        int noStalagmiteZoneSize = 2 * Chunk.CHUNK_SIZE; // 32 блока
        if (worldZ >= worldStartZ && worldZ < worldStartZ + noStalagmiteZoneSize)
        {
            return false;
        }
        
        // Используем абсолютные координаты (без учета центрирования мира)
        // Преобразуем в положительные координаты для единообразной генерации
        uint absX = (uint)(worldX + 10000); // Смещаем чтобы убрать отрицательные значения
        uint absZ = (uint)(worldZ + 10000);
        
        // Используем сетку для более равномерного распределения
        int gridX = (int)(absX / 6); // Сетка 6x6 блока
        int gridZ = (int)(absZ / 6);
        
        // Используем только координаты и seed для генерации - никаких данных о рельефе или мире
        System.Random gridRng = new System.Random(seed + gridX * 73856093 + gridZ * 19349663);
        float chance = (float)gridRng.NextDouble();
        
        if (chance > stalagmiteDensity)
        {
            return false; // Здесь нет сталагмита
        }
        
        // Определяем параметры сталагмита (разные по ширине и длине)
        int baseSizeX = gridRng.Next(3, 6); // Размер основания по X: 3-5 блоков
        int baseSizeZ = gridRng.Next(3, 6); // Размер основания по Z: 3-5 блоков (может отличаться от X)
        int topSizeX = gridRng.Next(1, 3);  // Размер вершины по X: 1-2 блока
        int topSizeZ = gridRng.Next(1, 3);  // Размер вершины по Z: 1-2 блока (может отличаться от X)
        
        // Центр сталагмита в сетке (в абсолютных координатах)
        int centerX = gridX * 6 + 3;
        int centerZ = gridZ * 6 + 3;
        
        // Вычисляем расстояние от центра сталагмита (в абсолютных координатах)
        float distX = Mathf.Abs((int)absX - centerX);
        float distZ = Mathf.Abs((int)absZ - centerZ);
        
        // Сталагмит идет от самого основания мира (Y=0) до максимальной высоты чанка
        // Высота сталагмита ВСЕГДА максимальная - до высоты мира минус 1 (CHUNK_HEIGHT - 1)
        // CHUNK_HEIGHT = 32, значит максимальная высота = 31 (индексы 0-31)
        const int MAX_WORLD_HEIGHT = Chunk.CHUNK_HEIGHT - 1; // Всегда максимальная высота мира
        int stalagmiteTop = MAX_WORLD_HEIGHT;
        int stalagmiteHeight = MAX_WORLD_HEIGHT; // Высота от Y=0 до вершины (всегда максимальная)
        
        // Сталагмит начинается строго от Y=0 (основание мира) и идет до максимальной высоты
        if (worldY < 0 || worldY > MAX_WORLD_HEIGHT)
        {
            return false;
        }
        
        // Вычисляем текущий размер сталагмита на этой высоте (линейная интерполяция)
        // heightRatio = 0 внизу (Y=0), = 1 вверху (Y=максимальная высота мира)
        float heightRatio = (float)worldY / stalagmiteHeight;
        float currentSizeX = Mathf.Lerp(baseSizeX, topSizeX, heightRatio);
        float currentSizeZ = Mathf.Lerp(baseSizeZ, topSizeZ, heightRatio);
        
        // Проверяем, находится ли блок внутри эллиптического конуса сталагмита
        // Используем эллиптическую форму для разных размеров по X и Z
        // Защита от деления на ноль и слишком маленьких размеров
        float effectiveSizeX = Mathf.Max(currentSizeX, 0.5f);
        float effectiveSizeZ = Mathf.Max(currentSizeZ, 0.5f);
        
        float normalizedDistX = distX / effectiveSizeX;
        float normalizedDistZ = distZ / effectiveSizeZ;
        
        // Проверяем, находится ли точка внутри эллипса
        if (normalizedDistX * normalizedDistX + normalizedDistZ * normalizedDistZ < 1.0f)
        {
            return true;
        }
        
        return false;
    }
}

