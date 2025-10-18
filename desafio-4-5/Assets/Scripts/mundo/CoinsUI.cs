using UnityEngine;
using TMPro;

public class CoinsUI : MonoBehaviour
{
    public TextMeshProUGUI coinsText;

    private void Start()
    {
        if (CurrencyManager.Instance != null)
        {
            CurrencyManager.Instance.OnCoinsChanged += UpdateUI;
            UpdateUI(CurrencyManager.Instance.Coins);
        }
    }

    private void UpdateUI(int coins)
    {
        if (coinsText != null) coinsText.text = coins.ToString();
    }

    private void OnDestroy()
    {
        if (CurrencyManager.Instance != null)
            CurrencyManager.Instance.OnCoinsChanged -= UpdateUI;
    }
}
