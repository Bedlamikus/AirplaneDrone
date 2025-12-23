using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Главный менеджер для управления миром-каньоном
/// </summary>
public class WorldManager : MonoBehaviour
{
    [Header("World Settings")]
    [SerializeField] private bool generateOnStart = true;
    [SerializeField] private bool useRandomSeed = true; // Случайный seed при каждом запуске
    
    [Header("Rendering Settings")]
    [SerializeField] private Material blockMaterial;
    [SerializeField] private Texture2D blockTextureAtlas;
    [SerializeField] private int atlasSize = 4;
    
    [Header("Chunk Generation Settings")]
    [SerializeField] private int chunksPerFrame = 1; // Количество чанков, генерируемых за кадр
    
    [Header("Progress")]
    [SerializeField] private UnityEngine.UI.Slider progressBar; // Опциональная полоса загрузки (может быть null)
    
    private WorldData worldData;
    private List<GameObject> chunkObjects = new List<GameObject>();
    private int totalChunks;
    private int generatedChunks;
    private bool isGenerating = false;
    
    // Событие для отслеживания прогресса
    public System.Action<float> OnGenerationProgress;
    
    private void Start()
    {
        if (generateOnStart)
        {
            StartCoroutine(GenerateWorldCoroutine());
        }
    }
    
    /// <summary>
    /// Генерировать весь мир через корутину
    /// </summary>
    public IEnumerator GenerateWorldCoroutine()
    {
        if (isGenerating)
        {
            Debug.LogWarning("WorldManager: Генерация уже идет!");
            yield break;
        }
        
        isGenerating = true;
        generatedChunks = 0;
        
        Debug.Log("WorldManager: Начинаем генерацию мира...");
        
        // Генерируем случайный seed, если нужно
        int seed = useRandomSeed ? Random.Range(int.MinValue, int.MaxValue) : 0;
        Random.InitState(seed);
        Debug.Log($"WorldManager: Используется seed: {seed}");
        
        // Создаем данные мира
        worldData = new WorldData();
        
        // Генерируем каньон в локальных координатах (относительно позиции WorldManager)
        CanyonGenerator.GenerateCanyon(worldData, seed, transform.position);
        
        Debug.Log($"WorldManager: Мир сгенерирован. Размер: {WorldData.WORLD_WIDTH}x{WorldData.WORLD_HEIGHT}x{WorldData.WORLD_DEPTH}");
        
        // Настраиваем материал
        if (blockMaterial != null)
        {
            if (blockTextureAtlas != null)
            {
                blockMaterial.SetTexture("_MainTex", blockTextureAtlas);
            }
            blockMaterial.SetInt("_AtlasSize", atlasSize);
        }
        
        // Вычисляем общее количество чанков
        totalChunks = WorldData.WORLD_WIDTH_CHUNKS * WorldData.WORLD_HEIGHT_CHUNKS * WorldData.WORLD_DEPTH_CHUNKS;
        
        // Создаем родительский объект для чанков
        GameObject chunksParent = new GameObject("Chunks");
        chunksParent.transform.SetParent(transform);
        
        // Генерируем чанки по одному через корутину
        for (int cx = 0; cx < WorldData.WORLD_WIDTH_CHUNKS; cx++)
        {
            for (int cy = 0; cy < WorldData.WORLD_HEIGHT_CHUNKS; cy++)
            {
                for (int cz = 0; cz < WorldData.WORLD_DEPTH_CHUNKS; cz++)
                {
                    // Создаем GameObject для чанка
                    GameObject chunkObj = new GameObject($"Chunk_{cx}_{cy}_{cz}");
                    chunkObj.transform.SetParent(chunksParent.transform);
                    chunkObj.tag = "Obstacle"; // Для коллизий с самолетом
                    
                    // Добавляем ChunkRenderer
                    ChunkRenderer chunkRenderer = chunkObj.AddComponent<ChunkRenderer>();
                    chunkRenderer.Initialize(worldData, blockMaterial, cx, cy, cz);
                    
                    chunkObjects.Add(chunkObj);
                    generatedChunks++;
                    
                    // Обновляем прогресс
                    float progress = (float)generatedChunks / totalChunks;
                    UpdateProgress(progress);
                    
                    // Ждем перед следующим чанком (для плавной загрузки)
                    if (chunksPerFrame > 0 && generatedChunks % chunksPerFrame == 0)
                    {
                        yield return null; // Пропускаем кадр
                    }
                }
            }
        }
        
        Debug.Log($"WorldManager: Все чанки созданы! Всего: {generatedChunks}");
        isGenerating = false;
        UpdateProgress(1f);
        
        // Вызываем событие завершения генерации мира
        GlobalEvents.OnWorldGenerationComplete?.Invoke();
    }
    
    /// <summary>
    /// Обновить прогресс загрузки
    /// </summary>
    private void UpdateProgress(float progress)
    {
        if (progressBar != null)
        {
            progressBar.value = progress;
        }
        
        OnGenerationProgress?.Invoke(progress);
    }
    
    /// <summary>
    /// Получить данные мира
    /// </summary>
    public WorldData GetWorldData()
    {
        return worldData;
    }
    
    /// <summary>
    /// Получить тип блока в позиции мира
    /// </summary>
    public int GetBlockAt(Vector3 worldPosition)
    {
        if (worldData == null) return WorldData.BLOCK_AIR;
        
        int x = Mathf.FloorToInt(worldPosition.x);
        int y = Mathf.FloorToInt(worldPosition.y);
        int z = Mathf.FloorToInt(worldPosition.z);
        
        return worldData.GetBlock(x, y, z);
    }
    
    /// <summary>
    /// Перегенерировать мир
    /// </summary>
    public void RegenerateWorld()
    {
        // Удаляем старые чанки
        foreach (var chunk in chunkObjects)
        {
            if (chunk != null)
            {
                Destroy(chunk);
            }
        }
        chunkObjects.Clear();
        
        // Запускаем новую генерацию
        StartCoroutine(GenerateWorldCoroutine());
    }
    
    /// <summary>
    /// Проверить, идет ли генерация
    /// </summary>
    public bool IsGenerating()
    {
        return isGenerating;
    }
}

