using UnityEngine;
using UnityEngine.UI;
using TMPro; // Soporta TextMeshPro

public class CoinManager : MonoBehaviour
{
    public static CoinManager Instance { get; private set; }

    [Header("UI")]
    [Tooltip("Asigna un TMP_Text (TextMeshPro)")]

    public TMP_Text coinTextTMP; // TextMeshProUGUI
    public string prefix = "Coins: ";

    public int Coins { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        UpdateUI();
    }

    public void Add(int amount)
    {
        if (amount <= 0) return;
        Coins += amount;
        UpdateUI();
    }

    public void ResetCoins(int value = 0)
    {
        Coins = Mathf.Max(0, value);
        UpdateUI();
    }

    private void UpdateUI()
    {
        string value = prefix + Coins.ToString();
        if (coinTextTMP != null)
        {
            coinTextTMP.text = value;
        }
    }
}
