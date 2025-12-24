using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;

/// <summary>
/// Основной менеджер генерации плоского кубического мира
/// </summary>
public class WorldGenerator : MonoBehaviour
{
    [Header("World Settings")]
    [Tooltip("Ширина мира в чанках (по оси X)")]
    [SerializeField] private int worldWidthInChunks = 4;
    
    [Tooltip("Длина мира в чанках (по оси Z)")]
    [SerializeField] private int worldLengthInChunks = 4;
    
    [Tooltip("Материал для блоков (должен использовать текстуру 4x4)")]
    [SerializeField] private Material blockMaterial;
    
    [Header("Block Textures")]
    [Tooltip("Настройки текстур для каждого типа блока. Если пусто, используются значения по умолчанию")]
    [SerializeField] private BlockTextureSettings[] blockTextureSettings = new BlockTextureSettings[0];
    
    [Header("Terrain Generation")]
    [Tooltip("Seed для генерации (0 = случайный)")]
    [SerializeField] private int seed = 0;
    
    [Tooltip("Использовать случайный seed при каждом запуске")]
    [SerializeField] private bool useRandomSeed = true;
    
    [Tooltip("Масштаб шума (меньше = более плавный рельеф, больше = более детальный)")]
    [Range(0.01f, 0.2f)]
    [SerializeField] private float noiseScale = 0.03f;
    
    [Tooltip("Множитель высоты (больше = выше горы и глубже впадины)")]
    [Range(1f, 20f)]
    [SerializeField] private float heightMultiplier = 10f;
    
    [Tooltip("Ширина каньона (1.0 = стандартная, меньше = уже, больше = шире)")]
    [Range(0.3f, 2.0f)]
    [SerializeField] private float canyonWidthFactor = 1.0f;
    
    [Tooltip("Плотность сталагмитов (вертикальных скал) - вероятность появления (0-1)")]
    [Range(0f, 0.5f)]
    [SerializeField] private float stalagmiteDensity = 0.15f;
    
    [Header("Generation Settings")]
    [Tooltip("Количество чанков, генерируемых за кадр")]
    [SerializeField] private int chunksPerFrame = 1;
    
    [Tooltip("Автоматически генерировать мир при старте")]
    [SerializeField] private bool generateOnStart = true;
    
    private Dictionary<Vector2Int, Chunk> chunks = new Dictionary<Vector2Int, Chunk>();
    private Queue<Vector2Int> chunksToGenerate = new Queue<Vector2Int>();
    public bool isGenerating = true;

    public UnityEvent EndGeneration = new();

    public void GenerateWorld()
    {
        if (useRandomSeed && seed == 0)
        {
            seed = Random.Range(0, int.MaxValue);
        }

        // Запускаем генерацию мира после инициализации всех компонентов
        StartCoroutine(GenerateWorldCoroutine());
    }
    
    /// <summary>
    /// Корутина генерации мира
    /// </summary>
    private IEnumerator GenerateWorldCoroutine()
    {
        isGenerating = true;

        // Очищаем старые чанки
        ClearWorld();
        
        // Добавляем все чанки в очередь
        for (int x = 0; x < worldWidthInChunks; x++)
        {
            for (int z = 0; z < worldLengthInChunks; z++)
            {
                chunksToGenerate.Enqueue(new Vector2Int(x, z));
            }
        }
        
        int totalChunks = chunksToGenerate.Count;
        int generatedChunks = 0;
        
        // Генерируем чанки по частям
        while (chunksToGenerate.Count > 0)
        {
            for (int i = 0; i < chunksPerFrame && chunksToGenerate.Count > 0; i++)
            {
                Vector2Int chunkPos = chunksToGenerate.Dequeue();
                GenerateChunk(chunkPos.x, chunkPos.y);
                generatedChunks++;
            }
            
            // Показываем прогресс
            float progress = (float)generatedChunks / totalChunks;
            Debug.Log($"WorldGenerator: Progress {progress:P0} ({generatedChunks}/{totalChunks})");
            
            yield return null; // Ждем следующий кадр
        }
        
        // Обновляем все меши чанков еще раз после завершения генерации
        // Это нужно для правильной отрисовки границ между чанками
        Debug.Log("WorldGenerator: Updating all chunk meshes for border correction...");
        foreach (var chunk in chunks.Values)
        {
            chunk.UpdateMesh();
        }
        
        isGenerating = false;
        Debug.Log("WorldGenerator: World generation complete!");
        Debug.Log($"WorldGenerator: Seed = {seed} (используйте этот seed для воссоздания этого мира)");

        // Отправляем локальное событие о завершении генерации в сценарий
        EndGeneration.Invoke();
    }
    
    /// <summary>
    /// Генерирует один чанк
    /// </summary>
    private void GenerateChunk(int chunkX, int chunkZ)
    {
        Vector2Int chunkPos = new Vector2Int(chunkX, chunkZ);
        
        // Создаем чанк с настройками текстур
        Chunk chunk = new Chunk(chunkX, chunkZ, blockTextureSettings);
        chunks[chunkPos] = chunk;
        
        // Вычисляем смещение для центрирования мира
        int worldCenterOffsetX = -(worldWidthInChunks * Chunk.CHUNK_SIZE / 2);
        int worldCenterOffsetZ = -(worldLengthInChunks * Chunk.CHUNK_SIZE / 2);
        
        // Генерируем блоки в чанке (плоский мир)
        for (int x = 0; x < Chunk.CHUNK_SIZE; x++)
        {
            for (int z = 0; z < Chunk.CHUNK_SIZE; z++)
            {
                for (int y = 0; y < Chunk.CHUNK_HEIGHT; y++)
                {
                    // Вычисляем мировые координаты с учетом центрирования
                    int worldX = chunkX * Chunk.CHUNK_SIZE + x + worldCenterOffsetX;
                    int worldZ = chunkZ * Chunk.CHUNK_SIZE + z + worldCenterOffsetZ;
                    
                    // Первый ряд (z=0 в локальных координатах чанка) первого чанка по Z должен быть полностью заполнен блоками камня
                    // Это соответствует первому ряду мира по локальным координатам
                    if (chunkZ == 0 && z == 0)
                    {
                        chunk.SetBlock(x, y, z, BlockType.Stone);
                        continue;
                    }
                    
                    // Вычисляем размер мира по ширине в блоках
                    int worldWidthInBlocks = worldWidthInChunks * Chunk.CHUNK_SIZE;
                    
                    // Сначала генерируем блок с учетом рельефа (каньон: горы по краям, углубление в центре)
                    BlockType blockType = TerrainGenerator.GenerateBlock(worldX, y, worldZ, seed, noiseScale, heightMultiplier, worldWidthInBlocks, canyonWidthFactor);
                    
                    // Затем проверяем сталагмиты ПОСЛЕ генерации мира (независимо от рельефа)
                    // Сталагмиты имеют слоистую структуру с одинаковой раскраской по высоте
                    int worldLengthInBlocks = worldLengthInChunks * Chunk.CHUNK_SIZE;
                    BlockType? stalagmiteBlock = TerrainGenerator.GetStalagmiteBlock(worldX, y, worldZ, seed, stalagmiteDensity, worldLengthInBlocks);
                    if (stalagmiteBlock.HasValue)
                    {
                        blockType = stalagmiteBlock.Value; // Заменяем на блок сталагмита с учетом слоистой структуры
                    }
                    
                    chunk.SetBlock(x, y, z, blockType);
                }
            }
        }
        
        // Устанавливаем делегат для проверки блоков в соседних чанках
        chunk.GetBlockFromWorld = GetBlock;
        
        // Создаем GameObject для чанка (прямо как дочерний к WorldGenerator)
        if (blockMaterial != null)
        {
            chunk.CreateGameObject(transform, blockMaterial);
        }
        
        // Обновляем меш сразу после генерации
        chunk.UpdateMesh();
    }
    
    /// <summary>
    /// Получить чанк по координатам
    /// </summary>
    public Chunk GetChunk(int chunkX, int chunkZ)
    {
        Vector2Int pos = new Vector2Int(chunkX, chunkZ);
        chunks.TryGetValue(pos, out Chunk chunk);
        return chunk;
    }
    
    /// <summary>
    /// Получить тип блока по мировым координатам
    /// </summary>
    public BlockType GetBlock(int worldX, int worldY, int worldZ)
    {
        // Правильно вычисляем координаты чанка с учетом отрицательных координат
        int chunkX = worldX >= 0 ? worldX / Chunk.CHUNK_SIZE : (worldX + 1) / Chunk.CHUNK_SIZE - 1;
        int chunkZ = worldZ >= 0 ? worldZ / Chunk.CHUNK_SIZE : (worldZ + 1) / Chunk.CHUNK_SIZE - 1;
        
        Chunk chunk = GetChunk(chunkX, chunkZ);
        if (chunk == null)
        {
            return BlockType.Air;
        }
        
        // Вычисляем локальные координаты в чанке
        int localX = worldX - chunkX * Chunk.CHUNK_SIZE;
        int localZ = worldZ - chunkZ * Chunk.CHUNK_SIZE;
        
        // Проверяем границы на всякий случай
        if (localX < 0 || localX >= Chunk.CHUNK_SIZE || 
            localZ < 0 || localZ >= Chunk.CHUNK_SIZE ||
            worldY < 0 || worldY >= Chunk.CHUNK_HEIGHT)
        {
            return BlockType.Air;
        }
        
        return chunk.GetBlock(localX, worldY, localZ);
    }
    
    /// <summary>
    /// Очистить весь мир
    /// </summary>
    public void ClearWorld()
    {
        foreach (var chunk in chunks.Values)
        {
            chunk.Destroy();
        }
        chunks.Clear();
        chunksToGenerate.Clear();
    }
    
    /// <summary>
    /// Перегенерировать мир
    /// </summary>
    public void RegenerateWorld()
    {
        if (useRandomSeed)
        {
            seed = Random.Range(0, int.MaxValue);
        }
        GenerateWorld();
    }
    
    private void OnDestroy()
    {
        // При уничтожении просто очищаем чанки, не создавая новый chunksParent
        foreach (var chunk in chunks.Values)
        {
            chunk.Destroy();
        }
        chunks.Clear();
        chunksToGenerate.Clear();
        
        // Не уничтожаем chunksParent здесь, так как он будет уничтожен автоматически
        // вместе с родительским объектом
    }
}

