using UnityEngine;

public static class PlayerData
{
    private const string COINS_KEY = "PlayerCoins";

    /// <summary>
    /// Получить текущее количество монет
    /// </summary>
    public static int GetCoins()
    {
        return PlayerPrefs.GetInt(COINS_KEY, 0);
    }

    /// <summary>
    /// Добавить монеты
    /// </summary>
    public static void AddCoins(int amount)
    {
        int currentCoins = GetCoins();
        PlayerPrefs.SetInt(COINS_KEY, currentCoins + amount);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// Установить количество монет
    /// </summary>
    public static void SetCoins(int amount)
    {
        PlayerPrefs.SetInt(COINS_KEY, amount);
        PlayerPrefs.Save();
    }
}

