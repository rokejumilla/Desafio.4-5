using UnityEngine;
using System;

public class CurrencyManager : MonoBehaviour
{
    public static CurrencyManager Instance { get; private set; }
    [Tooltip("Clave para guardar monedas")]
    public string prefsKey = "coins_count";

    public int Coins { get; private set; }
    public event Action<int> OnCoinsChanged;

    private void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); return; }

        Coins = PlayerPrefs.GetInt(prefsKey, 0);
    }

    public void AddCoins(int amount)
    {
        if (amount <= 0) return;
        Coins += amount;
        PlayerPrefs.SetInt(prefsKey, Coins);
        PlayerPrefs.Save();
        OnCoinsChanged?.Invoke(Coins);
        Debug.Log($"Coins: {Coins}");
    }

    public void SetCoins(int value)
    {
        Coins = Mathf.Max(0, value);
        PlayerPrefs.SetInt(prefsKey, Coins);
        PlayerPrefs.Save();
        OnCoinsChanged?.Invoke(Coins);
    }
}
