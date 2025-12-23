using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Рендерер для отрисовки чанка блоков
/// </summary>
public class ChunkRenderer : MonoBehaviour
{
    private WorldData worldData;
    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;
    private MeshCollider meshCollider;
    
    // Кэш для меша
    private Mesh chunkMesh;
    
    // Параметры texture atlas
    [Header("Texture Atlas Settings")]
    [SerializeField] private Texture2D blockTextureAtlas;
    [SerializeField] private int atlasSize = 4; // Количество текстур в строке/столбце (4x4 = 16 текстур)
    
    private void Awake()
    {
        meshFilter = GetComponent<MeshFilter>();
        if (meshFilter == null)
        {
            meshFilter = gameObject.AddComponent<MeshFilter>();
        }
        
        meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer == null)
        {
            meshRenderer = gameObject.AddComponent<MeshRenderer>();
        }
        
        meshCollider = GetComponent<MeshCollider>();
        if (meshCollider == null)
        {
            meshCollider = gameObject.AddComponent<MeshCollider>();
        }
        
        chunkMesh = new Mesh();
        chunkMesh.name = "ChunkMesh";
        meshFilter.mesh = chunkMesh;
    }
    
    // Данные чанка
    private int chunkX, chunkY, chunkZ; // Координаты чанка
    
    /// <summary>
    /// Инициализировать рендерер с данными мира и координатами чанка
    /// </summary>
    public void Initialize(WorldData data, Material blockMaterial, int chunkX, int chunkY, int chunkZ)
    {
        worldData = data;
        this.chunkX = chunkX;
        this.chunkY = chunkY;
        this.chunkZ = chunkZ;
        
        if (blockMaterial != null)
        {
            meshRenderer.material = blockMaterial;
        }
        
        BuildMesh();
    }
    
    /// <summary>
    /// Построить меш из блоков чанка
    /// </summary>
    public void BuildMesh()
    {
        if (worldData == null) return;
        
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Vector2> uvs = new List<Vector2>();
        List<Vector3> normals = new List<Vector3>();
        
        // Вычисляем границы чанка в мировых координатах
        int startX = chunkX * WorldData.CHUNK_WIDTH;
        int startY = chunkY * WorldData.CHUNK_HEIGHT;
        int startZ = chunkZ * WorldData.CHUNK_DEPTH;
        
        int endX = Mathf.Min(startX + WorldData.CHUNK_WIDTH, WorldData.WORLD_WIDTH);
        int endY = Mathf.Min(startY + WorldData.CHUNK_HEIGHT, WorldData.WORLD_HEIGHT);
        int endZ = Mathf.Min(startZ + WorldData.CHUNK_DEPTH, WorldData.WORLD_DEPTH);
        
        // Проходим по блокам чанка и добавляем видимые грани
        for (int x = startX; x < endX; x++)
        {
            for (int y = startY; y < endY; y++)
            {
                for (int z = startZ; z < endZ; z++)
                {
                    int blockType = worldData.GetBlock(x, y, z);
                    
                    // Пропускаем воздух
                    if (blockType == WorldData.BLOCK_AIR) continue;
                    
                    // Добавляем грани блока, если они видимы
                    AddBlockFaces(x, y, z, blockType, vertices, triangles, uvs, normals);
                }
            }
        }
        
        // Применяем данные к мешу
        chunkMesh.Clear();
        if (vertices.Count > 0)
        {
            chunkMesh.vertices = vertices.ToArray();
            chunkMesh.triangles = triangles.ToArray();
            chunkMesh.uv = uvs.ToArray();
            chunkMesh.normals = normals.ToArray();
            chunkMesh.RecalculateBounds();
            
            // Обновляем коллайдер
            meshCollider.sharedMesh = chunkMesh;
        }
        else
        {
            // Если нет вершин, создаем пустой меш
            meshCollider.sharedMesh = null;
        }
    }
    
    /// <summary>
    /// Добавить грани блока, если они видимы
    /// </summary>
    private void AddBlockFaces(int x, int y, int z, int blockType, 
        List<Vector3> vertices, List<int> triangles, List<Vector2> uvs, List<Vector3> normals)
    {
        // Проверяем каждую грань (0=top, 1=bottom, 2=north, 3=south, 4=east, 5=west)
        for (int face = 0; face < 6; face++)
        {
            if (worldData.ShouldRenderFace(x, y, z, face))
            {
                AddFace(x, y, z, face, blockType, vertices, triangles, uvs, normals);
            }
        }
    }
    
    /// <summary>
    /// Добавить одну грань блока
    /// </summary>
    private void AddFace(int x, int y, int z, int face, int blockType,
        List<Vector3> vertices, List<int> triangles, List<Vector2> uvs, List<Vector3> normals)
    {
        int vertexIndex = vertices.Count;
        
        // Определяем вершины грани в зависимости от направления
        Vector3[] faceVertices = GetFaceVertices(x, y, z, face);
        Vector3 faceNormal = GetFaceNormal(face);
        
        // Добавляем вершины
        vertices.AddRange(faceVertices);
        
        // Добавляем нормали
        for (int i = 0; i < 4; i++)
        {
            normals.Add(faceNormal);
        }
        
        // Добавляем треугольники (два треугольника на грань)
        triangles.Add(vertexIndex);
        triangles.Add(vertexIndex + 1);
        triangles.Add(vertexIndex + 2);
        
        triangles.Add(vertexIndex);
        triangles.Add(vertexIndex + 2);
        triangles.Add(vertexIndex + 3);
        
        // Добавляем UV координаты для texture atlas
        Vector2[] faceUVs = GetFaceUVs(blockType, face);
        uvs.AddRange(faceUVs);
    }
    
    /// <summary>
    /// Получить вершины грани блока
    /// </summary>
    private Vector3[] GetFaceVertices(int x, int y, int z, int face)
    {
        Vector3[] vertices = new Vector3[4];
        float size = 1f; // Размер блока
        
        switch (face)
        {
            case 0: // Top
                vertices[0] = new Vector3(x, y + size, z);
                vertices[1] = new Vector3(x + size, y + size, z);
                vertices[2] = new Vector3(x + size, y + size, z + size);
                vertices[3] = new Vector3(x, y + size, z + size);
                break;
            case 1: // Bottom
                vertices[0] = new Vector3(x, y, z);
                vertices[1] = new Vector3(x, y, z + size);
                vertices[2] = new Vector3(x + size, y, z + size);
                vertices[3] = new Vector3(x + size, y, z);
                break;
            case 2: // North (forward, +Z)
                vertices[0] = new Vector3(x, y, z + size);
                vertices[1] = new Vector3(x, y + size, z + size);
                vertices[2] = new Vector3(x + size, y + size, z + size);
                vertices[3] = new Vector3(x + size, y, z + size);
                break;
            case 3: // South (backward, -Z)
                vertices[0] = new Vector3(x + size, y, z);
                vertices[1] = new Vector3(x + size, y + size, z);
                vertices[2] = new Vector3(x, y + size, z);
                vertices[3] = new Vector3(x, y, z);
                break;
            case 4: // East (+X)
                vertices[0] = new Vector3(x + size, y, z);
                vertices[1] = new Vector3(x + size, y, z + size);
                vertices[2] = new Vector3(x + size, y + size, z + size);
                vertices[3] = new Vector3(x + size, y + size, z);
                break;
            case 5: // West (-X)
                vertices[0] = new Vector3(x, y, z + size);
                vertices[1] = new Vector3(x, y, z);
                vertices[2] = new Vector3(x, y + size, z);
                vertices[3] = new Vector3(x, y + size, z + size);
                break;
        }
        
        return vertices;
    }
    
    /// <summary>
    /// Получить нормаль грани
    /// </summary>
    private Vector3 GetFaceNormal(int face)
    {
        switch (face)
        {
            case 0: return Vector3.up;      // Top
            case 1: return Vector3.down;   // Bottom
            case 2: return Vector3.forward; // North
            case 3: return Vector3.back;    // South
            case 4: return Vector3.right;  // East
            case 5: return Vector3.left;    // West
            default: return Vector3.up;
        }
    }
    
    /// <summary>
    /// Получить UV координаты для грани блока в texture atlas
    /// </summary>
    private Vector2[] GetFaceUVs(int blockType, int face)
    {
        // Вычисляем позицию текстуры в atlas
        // Предполагаем, что каждый блок имеет одну текстуру (можно расширить для разных граней)
        int textureIndex = blockType - 1; // BLOCK_AIR = 0, поэтому вычитаем 1
        
        // Ограничиваем индекс
        textureIndex = Mathf.Clamp(textureIndex, 0, atlasSize * atlasSize - 1);
        
        // Вычисляем координаты в atlas (0-1)
        int row = textureIndex / atlasSize;
        int col = textureIndex % atlasSize;
        
        float tileSize = 1f / atlasSize;
        float uMin = col * tileSize;
        float uMax = (col + 1) * tileSize;
        float vMin = 1f - (row + 1) * tileSize; // Переворачиваем V
        float vMax = 1f - row * tileSize;
        
        // UV координаты для четырех вершин грани
        return new Vector2[]
        {
            new Vector2(uMin, vMin), // Bottom-left
            new Vector2(uMax, vMin), // Bottom-right
            new Vector2(uMax, vMax), // Top-right
            new Vector2(uMin, vMax)  // Top-left
        };
    }
}

