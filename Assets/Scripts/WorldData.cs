using UnityEngine;

/// <summary>
/// Класс для хранения данных мира в виде массива блоков
/// </summary>
public class WorldData
{
    // Размеры мира в блоках
    public const int CHUNK_HEIGHT = 30;
    public const int CHUNK_WIDTH = 32;
    public const int CHUNK_DEPTH = 32;
    
    // Размеры мира в чанках
    public const int WORLD_HEIGHT_CHUNKS = 1;
    public const int WORLD_WIDTH_CHUNKS = 4;
    public const int WORLD_DEPTH_CHUNKS = 100;
    
    // Размеры мира в блоках
    public const int WORLD_HEIGHT = WORLD_HEIGHT_CHUNKS * CHUNK_HEIGHT;
    public const int WORLD_WIDTH = WORLD_WIDTH_CHUNKS * CHUNK_WIDTH;
    public const int WORLD_DEPTH = WORLD_DEPTH_CHUNKS * CHUNK_DEPTH;
    
    // Типы блоков (16 типов для 4x4 texture atlas)
    public const int BLOCK_AIR = 0;
    public const int BLOCK_STONE = 1;
    public const int BLOCK_GRASS = 2;
    public const int BLOCK_DIRT = 3;
    public const int BLOCK_BEDROCK = 4;
    public const int BLOCK_SAND = 5;
    public const int BLOCK_GRAVEL = 6;
    public const int BLOCK_COBBLESTONE = 7;
    public const int BLOCK_WOOD = 8;
    public const int BLOCK_LEAVES = 9;
    public const int BLOCK_WATER = 10;
    public const int BLOCK_ICE = 11;
    public const int BLOCK_SNOW = 12;
    public const int BLOCK_CLAY = 13;
    public const int BLOCK_ORE = 14;
    public const int BLOCK_BRICK = 15;
    
    // Массив блоков: [x, y, z] = тип блока
    private int[,,] blocks;
    
    public WorldData()
    {
        blocks = new int[WORLD_WIDTH, WORLD_HEIGHT, WORLD_DEPTH];
    }
    
    /// <summary>
    /// Получить тип блока по координатам
    /// </summary>
    public int GetBlock(int x, int y, int z)
    {
        if (IsValidPosition(x, y, z))
        {
            return blocks[x, y, z];
        }
        return BLOCK_AIR;
    }
    
    /// <summary>
    /// Установить тип блока по координатам
    /// </summary>
    public void SetBlock(int x, int y, int z, int blockType)
    {
        if (IsValidPosition(x, y, z))
        {
            blocks[x, y, z] = blockType;
        }
    }
    
    /// <summary>
    /// Проверить, является ли позиция валидной
    /// </summary>
    public bool IsValidPosition(int x, int y, int z)
    {
        return x >= 0 && x < WORLD_WIDTH &&
               y >= 0 && y < WORLD_HEIGHT &&
               z >= 0 && z < WORLD_DEPTH;
    }
    
    /// <summary>
    /// Проверить, является ли блок воздухом
    /// </summary>
    public bool IsAir(int x, int y, int z)
    {
        return GetBlock(x, y, z) == BLOCK_AIR;
    }
    
    /// <summary>
    /// Проверить, нужно ли рендерить грань блока (если соседний блок - воздух)
    /// </summary>
    public bool ShouldRenderFace(int x, int y, int z, int face)
    {
        // face: 0=top, 1=bottom, 2=north, 3=south, 4=east, 5=west
        int nx = x, ny = y, nz = z;
        
        switch (face)
        {
            case 0: ny++; break; // top
            case 1: ny--; break; // bottom
            case 2: nz++; break; // north
            case 3: nz--; break; // south
            case 4: nx++; break; // east
            case 5: nx--; break; // west
        }
        
        return IsAir(nx, ny, nz);
    }
}

