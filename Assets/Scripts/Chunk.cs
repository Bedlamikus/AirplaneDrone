using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Представляет один чанк мира размером 16x16x32
/// </summary>
public class Chunk
{
    public const int CHUNK_SIZE = 16;
    public const int CHUNK_HEIGHT = 32;
    
    public int ChunkX { get; private set; }
    public int ChunkZ { get; private set; }
    
    // Массив блоков: [x, y, z]
    private BlockType[,,] blocks;
    
    private GameObject chunkObject;
    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;
    private MeshCollider meshCollider;
    
    private bool needsUpdate = false;
    
    // Делегат для проверки блоков в соседних чанках
    public System.Func<int, int, int, BlockType> GetBlockFromWorld;
    
    // Настройки текстур для блоков
    private BlockTextureSettings[] textureSettings;
    
    public Chunk(int chunkX, int chunkZ, BlockTextureSettings[] textureSettings = null)
    {
        ChunkX = chunkX;
        ChunkZ = chunkZ;
        blocks = new BlockType[CHUNK_SIZE, CHUNK_HEIGHT, CHUNK_SIZE];
        this.textureSettings = textureSettings;
    }
    
    /// <summary>
    /// Установить тип блока
    /// </summary>
    public void SetBlock(int x, int y, int z, BlockType blockType)
    {
        if (IsValidPosition(x, y, z))
        {
            blocks[x, y, z] = blockType;
            needsUpdate = true;
        }
    }
    
    /// <summary>
    /// Получить тип блока
    /// </summary>
    public BlockType GetBlock(int x, int y, int z)
    {
        if (IsValidPosition(x, y, z))
        {
            return blocks[x, y, z];
        }
        return BlockType.Air;
    }
    
    /// <summary>
    /// Проверить валидность позиции
    /// </summary>
    private bool IsValidPosition(int x, int y, int z)
    {
        return x >= 0 && x < CHUNK_SIZE &&
               y >= 0 && y < CHUNK_HEIGHT &&
               z >= 0 && z < CHUNK_SIZE;
    }
    
    /// <summary>
    /// Создать GameObject для чанка
    /// </summary>
    public void CreateGameObject(Transform parent, Material blockMaterial)
    {
        chunkObject = new GameObject($"Chunk_{ChunkX}_{ChunkZ}");
        chunkObject.transform.SetParent(parent);
        // Позиционируем чанк локально относительно родителя, начиная с (0,0,0)
        chunkObject.transform.localPosition = new Vector3(
            ChunkX * CHUNK_SIZE,
            0,
            ChunkZ * CHUNK_SIZE
        );
        
        // Устанавливаем тег Obstacle
        chunkObject.tag = "Obstacle";
        
        meshFilter = chunkObject.AddComponent<MeshFilter>();
        meshRenderer = chunkObject.AddComponent<MeshRenderer>();
        meshCollider = chunkObject.AddComponent<MeshCollider>();
        
        meshRenderer.material = blockMaterial;
        
        UpdateMesh();
    }
    
    /// <summary>
    /// Обновить меш чанка
    /// </summary>
    public void UpdateMesh()
    {
        if (chunkObject == null)
            return;
        
        MeshData meshData = new MeshData();
        
        for (int x = 0; x < CHUNK_SIZE; x++)
        {
            for (int y = 0; y < CHUNK_HEIGHT; y++)
            {
                for (int z = 0; z < CHUNK_SIZE; z++)
                {
                    BlockType blockType = blocks[x, y, z];
                    
                    if (blockType != BlockType.Air)
                    {
                        // Проверяем, видна ли грань блока
                        if (IsBlockVisible(x, y, z, 0, 1, 0)) // Top
                            AddBlockFace(meshData, x, y, z, BlockFace.Top, blockType);
                        
                        if (IsBlockVisible(x, y, z, 0, -1, 0)) // Bottom
                            AddBlockFace(meshData, x, y, z, BlockFace.Bottom, blockType);
                        
                        if (IsBlockVisible(x, y, z, 1, 0, 0)) // Right
                            AddBlockFace(meshData, x, y, z, BlockFace.Right, blockType);
                        
                        if (IsBlockVisible(x, y, z, -1, 0, 0)) // Left
                            AddBlockFace(meshData, x, y, z, BlockFace.Left, blockType);
                        
                        if (IsBlockVisible(x, y, z, 0, 0, 1)) // Front
                            AddBlockFace(meshData, x, y, z, BlockFace.Front, blockType);
                        
                        if (IsBlockVisible(x, y, z, 0, 0, -1)) // Back
                            AddBlockFace(meshData, x, y, z, BlockFace.Back, blockType);
                    }
                }
            }
        }
        
        Mesh mesh = meshData.ToMesh();
        meshFilter.mesh = mesh;
        meshCollider.sharedMesh = mesh;
        
        needsUpdate = false;
    }
    
    /// <summary>
    /// Проверить, видна ли грань блока (не закрыта соседним блоком)
    /// </summary>
    private bool IsBlockVisible(int x, int y, int z, int dx, int dy, int dz)
    {
        int nx = x + dx;
        int ny = y + dy;
        int nz = z + dz;
        
        BlockType neighbor;
        
        // Если соседняя позиция внутри чанка
        if (nx >= 0 && nx < CHUNK_SIZE &&
            ny >= 0 && ny < CHUNK_HEIGHT &&
            nz >= 0 && nz < CHUNK_SIZE)
        {
            neighbor = blocks[nx, ny, nz];
        }
        // Если соседняя позиция вне чанка, проверяем через делегат
        else if (GetBlockFromWorld != null)
        {
            int worldX = ChunkX * CHUNK_SIZE + x + dx;
            int worldZ = ChunkZ * CHUNK_SIZE + z + dz;
            neighbor = GetBlockFromWorld(worldX, ny, worldZ);
        }
        else
        {
            // Если делегат не установлен, считаем грань видимой
            return true;
        }
        
        // Грань видима только если соседний блок - воздух
        return neighbor == BlockType.Air;
    }
    
    /// <summary>
    /// Добавить грань блока к мешу
    /// </summary>
    private void AddBlockFace(MeshData meshData, int x, int y, int z, BlockFace face, BlockType blockType)
    {
        Vector3 pos = new Vector3(x, y, z);
        int startIndex = meshData.vertices.Count;
        
        Vector3[] vertices = GetFaceVertices(face, pos);
        int[] triangles = GetFaceTriangles(face, startIndex);
        Vector2[] uvs = GetFaceUVs(face, blockType);
        
        meshData.vertices.AddRange(vertices);
        meshData.triangles.AddRange(triangles);
        meshData.uvs.AddRange(uvs);
    }
    
    /// <summary>
    /// Получить вершины грани блока (порядок против часовой стрелки при взгляде снаружи)
    /// </summary>
    private Vector3[] GetFaceVertices(BlockFace face, Vector3 pos)
    {
        switch (face)
        {
            case BlockFace.Top: // Смотрим сверху вниз
                return new Vector3[]
                {
                    pos + new Vector3(0, 1, 0),
                    pos + new Vector3(0, 1, 1),
                    pos + new Vector3(1, 1, 1),
                    pos + new Vector3(1, 1, 0)
                };
            case BlockFace.Bottom: // Смотрим снизу вверх
                return new Vector3[]
                {
                    pos + new Vector3(0, 0, 0),
                    pos + new Vector3(1, 0, 0),
                    pos + new Vector3(1, 0, 1),
                    pos + new Vector3(0, 0, 1)
                };
            case BlockFace.Right: // Смотрим справа налево (снаружи блока)
                return new Vector3[]
                {
                    pos + new Vector3(1, 0, 0),
                    pos + new Vector3(1, 1, 0),
                    pos + new Vector3(1, 1, 1),
                    pos + new Vector3(1, 0, 1)
                };
            case BlockFace.Left: // Смотрим слева направо (снаружи блока)
                return new Vector3[]
                {
                    pos + new Vector3(0, 0, 1),
                    pos + new Vector3(0, 1, 1),
                    pos + new Vector3(0, 1, 0),
                    pos + new Vector3(0, 0, 0)
                };
            case BlockFace.Front: // Смотрим спереди (снаружи блока, +Z)
                return new Vector3[]
                {
                    pos + new Vector3(0, 0, 1),
                    pos + new Vector3(1, 0, 1),
                    pos + new Vector3(1, 1, 1),
                    pos + new Vector3(0, 1, 1)
                };
            case BlockFace.Back: // Смотрим сзади (снаружи блока, -Z)
                return new Vector3[]
                {
                    pos + new Vector3(1, 0, 0),
                    pos + new Vector3(0, 0, 0),
                    pos + new Vector3(0, 1, 0),
                    pos + new Vector3(1, 1, 0)
                };
            default:
                return new Vector3[0];
        }
    }
    
    /// <summary>
    /// Получить треугольники грани блока
    /// </summary>
    private int[] GetFaceTriangles(BlockFace face, int startIndex)
    {
        // Порядок вершин против часовой стрелки для правильных нормалей (направленных наружу)
        return new int[]
        {
            startIndex, startIndex + 1, startIndex + 2,
            startIndex, startIndex + 2, startIndex + 3
        };
    }
    
    /// <summary>
    /// Получить UV координаты для грани блока (для texture atlas 4x4)
    /// </summary>
    private Vector2[] GetFaceUVs(BlockFace face, BlockType blockType)
    {
        int atlasSize = 4; // Размер атласа 4x4
        float tileSize = 1f / atlasSize;
        float uvOffset = 0.01f; // Небольшой offset для предотвращения bleeding
        
        BlockFaceTexture texture = GetTextureForBlock(blockType, face);
        
        float u = texture.textureX * tileSize;
        float v = 1f - (texture.textureY + 1) * tileSize; // Инвертируем V координату
        
        // Базовые UV координаты
        Vector2 uv00 = new Vector2(u + uvOffset, v + uvOffset);
        Vector2 uv10 = new Vector2(u + tileSize - uvOffset, v + uvOffset);
        Vector2 uv11 = new Vector2(u + tileSize - uvOffset, v + tileSize - uvOffset);
        Vector2 uv01 = new Vector2(u + uvOffset, v + tileSize - uvOffset);
        
        // Для граней Right и Left нужно повернуть UV на 90 градусов
        switch (face)
        {
            case BlockFace.Right:
                // Поворачиваем UV на 90 градусов против часовой стрелки
                return new Vector2[]
                {
                    uv00,  // (0,0)
                    uv01,  // (0,1)
                    uv11,  // (1,1)
                    uv10   // (1,0)
                };
            case BlockFace.Left:
                // Поворачиваем UV на 90 градусов по часовой стрелке
                return new Vector2[]
                {
                    uv10,  // (1,0)
                    uv11,  // (1,1)
                    uv01,  // (0,1)
                    uv00   // (0,0)
                };
            default:
                // Стандартный порядок для остальных граней
                return new Vector2[]
                {
                    uv00,
                    uv10,
                    uv11,
                    uv01
                };
        }
    }
    
    /// <summary>
    /// Получить настройки текстуры для блока и грани
    /// </summary>
    private BlockFaceTexture GetTextureForBlock(BlockType blockType, BlockFace face)
    {
        // Если есть настройки текстур, используем их
        if (textureSettings != null)
        {
            foreach (var settings in textureSettings)
            {
                if (settings.blockType == blockType)
                {
                    return settings.GetTextureForFace(face);
                }
            }
        }
        
        // Иначе используем значения по умолчанию
        return GetDefaultTexture(blockType, face);
    }
    
    /// <summary>
    /// Получить текстуру по умолчанию для блока (если настройки не заданы)
    /// </summary>
    private BlockFaceTexture GetDefaultTexture(BlockType blockType, BlockFace face)
    {
        BlockFaceTexture texture = new BlockFaceTexture();
        
        switch (blockType)
        {
            case BlockType.Grass:
                if (face == BlockFace.Top)
                {
                    texture.textureX = 0;
                    texture.textureY = 0;
                }
                else if (face == BlockFace.Bottom)
                {
                    texture.textureX = 2;
                    texture.textureY = 0;
                }
                else
                {
                    texture.textureX = 3;
                    texture.textureY = 0;
                }
                break;
            case BlockType.Dirt:
                texture.textureX = 2;
                texture.textureY = 0;
                break;
            case BlockType.Stone:
                texture.textureX = 1;
                texture.textureY = 0;
                break;
            case BlockType.Water:
                texture.textureX = 0;
                texture.textureY = 1;
                break;
            case BlockType.Wood:
                if (face == BlockFace.Top || face == BlockFace.Bottom)
                {
                    texture.textureX = 1;
                    texture.textureY = 1;
                }
                else
                {
                    texture.textureX = 2;
                    texture.textureY = 1;
                }
                break;
            case BlockType.Leaves:
                texture.textureX = 3;
                texture.textureY = 1;
                break;
            case BlockType.Sand:
                texture.textureX = 0;
                texture.textureY = 2;
                break;
            case BlockType.Gravel:
                texture.textureX = 1;
                texture.textureY = 2;
                break;
            case BlockType.Clay:
                texture.textureX = 2;
                texture.textureY = 2;
                break;
            default:
                texture.textureX = 0;
                texture.textureY = 0;
                break;
        }
        
        return texture;
    }
    
    public void Destroy()
    {
        if (chunkObject != null)
        {
            Object.Destroy(chunkObject);
        }
    }
}

/// <summary>
/// Грани блока
/// </summary>
public enum BlockFace
{
    Top,
    Bottom,
    Right,
    Left,
    Front,
    Back
}

/// <summary>
/// Данные для построения меша
/// </summary>
public class MeshData
{
    public List<Vector3> vertices = new List<Vector3>();
    public List<int> triangles = new List<int>();
    public List<Vector2> uvs = new List<Vector2>();
    
    public Mesh ToMesh()
    {
        Mesh mesh = new Mesh();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.RecalculateNormals();
        return mesh;
    }
}

