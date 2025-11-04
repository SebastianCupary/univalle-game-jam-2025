using UnityEngine;
using TMPro;

public class BudgetManager : MonoBehaviour
{
    public static BudgetManager Instance { get; private set; }

    [Header("UI")]
    public TMP_Text budgetTextTMP;
    public string prefix = "Presupuesto: ";

    [Header("Level Budget")]
    [Tooltip("Define el presupuesto inicial para este nivel (puedes configurarlo por escena)")]
    public int startingBudget = 1000;

    public int TotalBudget { get; private set; }
    public int Spent { get; private set; }
    public int Remaining => Mathf.Max(0, TotalBudget - Spent);

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Inicializa usando el valor del nivel
        Initialize(startingBudget);
    }

    public void Initialize(int total)
    {
        TotalBudget = Mathf.Max(0, total);
        Spent = 0;
        UpdateUI();
    }

    public bool CanAfford(int cost)
    {
        return cost <= Remaining;
    }

    public bool Spend(int cost)
    {
        if (cost <= 0) return true;
        if (!CanAfford(cost)) return false;
        Spent += cost;
        UpdateUI();
        return true;
    }

    public void Refund(int cost)
    {
        if (cost <= 0) return;
        Spent = Mathf.Max(0, Spent - cost);
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (budgetTextTMP != null)
        {
            budgetTextTMP.text = prefix + Remaining.ToString();
        }
    }
}
