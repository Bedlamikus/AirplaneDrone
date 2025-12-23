using UnityEngine;

/// <summary>
/// Утилита для создания простой texture atlas из отдельных текстур блоков
/// Используйте этот скрипт в Editor для создания atlas, если у вас есть отдельные текстуры
/// </summary>
#if UNITY_EDITOR
using UnityEditor;

public class BlockTextureAtlasGenerator : MonoBehaviour
{
    [Header("Texture Atlas Settings")]
    [SerializeField] private Texture2D[] blockTextures = new Texture2D[16]; // Массив текстур блоков
    [SerializeField] private int atlasSize = 4; // Размер atlas (4x4 = 16 текстур)
    [SerializeField] private int textureSize = 64; // Размер каждой текстуры в atlas
    
    /// <summary>
    /// Создать texture atlas из массива текстур
    /// </summary>
    [ContextMenu("Generate Atlas")]
    public void GenerateAtlas()
    {
        if (blockTextures == null || blockTextures.Length == 0)
        {
            Debug.LogWarning("BlockTextureAtlasGenerator: Нет текстур для создания atlas!");
            return;
        }
        
        int totalSize = atlasSize * textureSize;
        Texture2D atlas = new Texture2D(totalSize, totalSize, TextureFormat.RGBA32, false);
        
        // Заполняем atlas белым цветом по умолчанию
        Color[] defaultPixels = new Color[textureSize * textureSize];
        for (int i = 0; i < defaultPixels.Length; i++)
        {
            defaultPixels[i] = Color.white;
        }
        
        // Размещаем текстуры в atlas
        for (int i = 0; i < blockTextures.Length && i < atlasSize * atlasSize; i++)
        {
            if (blockTextures[i] == null) continue;
            
            int row = i / atlasSize;
            int col = i % atlasSize;
            
            int x = col * textureSize;
            int y = (atlasSize - 1 - row) * textureSize; // Переворачиваем по Y
            
            // Копируем пиксели текстуры в atlas
            Color[] pixels = blockTextures[i].GetPixels(0, 0, textureSize, textureSize);
            atlas.SetPixels(x, y, textureSize, textureSize, pixels);
        }
        
        atlas.Apply();
        
        // Сохраняем atlas как asset
        string path = "Assets/Images/BlockAtlas.png";
        System.IO.File.WriteAllBytes(path, atlas.EncodeToPNG());
        AssetDatabase.Refresh();
        
        Debug.Log($"BlockTextureAtlasGenerator: Atlas создан и сохранен в {path}");
    }
}
#endif

