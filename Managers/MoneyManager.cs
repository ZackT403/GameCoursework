using UnityEngine;

public class MoneyManager : MonoBehaviour
{
    public static MoneyManager Instance;

    public int startingMoney = 20000;
    public int currentMoney;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        // set current money amount in the ui
        currentMoney = startingMoney;
        UpdateUI();
    }

    public bool CanAfford(int cost)
    {
        // helper to check if player has enough money   
        return currentMoney >= cost;
    }

    public bool SpendMoney(int cost)
    {   
        // subtract money 
        // return true if successful
        // false if not enough cash
        if (currentMoney >= cost)
        {
            currentMoney -= cost;
            UpdateUI();
            return true;
        }
        else
        {
            return false;
        }
    }

    public void AddMoney(int amount)
    {
        // add money to player balance
        currentMoney += amount;
        UpdateUI();
    }

    void UpdateUI()
    {
        // update money in ui
        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateMoneyUI(currentMoney);
        }
    }
}