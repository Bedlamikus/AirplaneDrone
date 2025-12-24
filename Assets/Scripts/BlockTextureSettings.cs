using UnityEngine;

/// <summary>
/// Настройки текстур для блока (для каждой грани)
/// </summary>
[System.Serializable]
public class BlockFaceTexture
{
    [Tooltip("Индекс текстуры в атласе 4x4 (0-15). X координата в атласе")]
    [Range(0, 3)]
    public int textureX = 0;
    
    [Tooltip("Индекс текстуры в атласе 4x4 (0-15). Y координата в атласе")]
    [Range(0, 3)]
    public int textureY = 0;
}

/// <summary>
/// Настройки текстур для типа блока
/// </summary>
[System.Serializable]
public class BlockTextureSettings
{
    [Tooltip("Тип блока")]
    public BlockType blockType;
    
    [Tooltip("Текстура для верхней грани")]
    public BlockFaceTexture top = new BlockFaceTexture();
    
    [Tooltip("Текстура для нижней грани")]
    public BlockFaceTexture bottom = new BlockFaceTexture();
    
    [Tooltip("Текстура для боковых граней (Right, Left, Front, Back)")]
    public BlockFaceTexture sides = new BlockFaceTexture();
    
    /// <summary>
    /// Получить настройки текстуры для конкретной грани
    /// </summary>
    public BlockFaceTexture GetTextureForFace(BlockFace face)
    {
        switch (face)
        {
            case BlockFace.Top:
                return top;
            case BlockFace.Bottom:
                return bottom;
            default:
                return sides;
        }
    }
}

