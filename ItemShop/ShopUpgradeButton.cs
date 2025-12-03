using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopUpgradeButton : MonoBehaviour
{
    public enum UpgradeType { AutoCleaners, BuildingProfit }
    
    [Header("Settings")]
    public UpgradeType type;
    public int cost = 500;
    
    private Button btn;
    private TextMeshProUGUI btnText;
    private string originalText;

    void Start()
    {   
        // init buttons
        btn = GetComponent<Button>();
        btnText = GetComponentInChildren<TextMeshProUGUI>();
        if (btnText != null){
            originalText = btnText.text;
        }
        btn.onClick.AddListener(OnClick);
        
        // Initial text setup
        UpdateText();
    }

    void Update()
    {
        // check if already purchased cleanrers
        if (type == UpgradeType.AutoCleaners && UpgradeManager.Instance.hasAutoCleaners)
        {
            btn.interactable = false;
            if (btnText != null){
                btnText.text = "HIRED (Active)";
            }
            return;
        }
        
        if (MoneyManager.Instance != null)
        {
            if (MoneyManager.Instance.CanAfford(cost))
            {
                btn.interactable = true; // show if affordable
            }
            else
            {
                btn.interactable = false; // grey out if not
            }
        }
    }

    void OnClick()
    {
        // unlock cleaners
        if (type == UpgradeType.AutoCleaners)
        {

            UpgradeManager.Instance.UnlockAutoCleaners(cost);
        }
        else if (type == UpgradeType.BuildingProfit)
        {
            // upgrade buildings 
            UpgradeManager.Instance.UpgradeBuildings(cost);
            // double cost of next upgrade
            cost *= 2; 
            UpdateText();
        }
    }

    void UpdateText()
    {
        // set button text with new cost and level
        if (btnText != null)
        {
            if (type == UpgradeType.BuildingProfit)
            {
                btnText.text = $"Upgrade Stalls\n(Lvl {UpgradeManager.Instance.buildingLevel + 1}) - £{cost}";
            }
        }
    }
}