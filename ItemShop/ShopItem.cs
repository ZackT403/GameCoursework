using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopItem : MonoBehaviour
{
    [Header("Settings")]
    public GameObject prefabToBuild;
    public int cost = 1000;

    [Header("Limits")]
    public int buildLimit = 0; 
    public string limitTag = ""; 

    [Header("Progression")]
    public int requiredSatisfaction = 0;
    public GameObject lockIcon;
    
    private Button itemButton;
    private TextMeshProUGUI buttonText;
    private string originalText;

    void Start()
    {
        itemButton = GetComponent<Button>();
        
        buttonText = GetComponentInChildren<TextMeshProUGUI>();
        if (buttonText != null) originalText = buttonText.text;
        // check status and lock it if needed        
        UpdateLockStatus();
    }

    void Update()
    {
        // check status and lock and unlcok
        UpdateLockStatus();
    }

    void UpdateLockStatus()
    {
        // safety checks to prevent errors
        if (GameManager.Instance == null){
            return;
        }
        if (itemButton == null){
            return;
        }
        // get current happiness
        int currentHappy = GameManager.Instance.globalAverageSatisfaction;

        if (currentHappy < requiredSatisfaction)
        {
            // if not enough happiness lock it
            itemButton.interactable = false;
            
            if (lockIcon != null){
                lockIcon.SetActive(true);
            }
            
            if (buttonText != null){
                // add locked message
                buttonText.text = $"LOCKED\n(Need {requiredSatisfaction}%)";
            }
        }
        else
        {   
            // if happiness is sufficent unlcok the item
            itemButton.interactable = true;
            
            if (lockIcon != null){
                lockIcon.SetActive(false);
            }
            
            if (buttonText != null){
                // restore original text 
                buttonText.text = originalText;
            }
        }
    }

    public void OnClick()
    {
        // check if we can build more of this item
        if (buildLimit > 0 && limitTag != "")
        {
            int currentCount = GameObject.FindGameObjectsWithTag(limitTag).Length;
            if (currentCount >= buildLimit)
            {
                if (UIManager.Instance != null){
                    // provide message if the build limit is reached
                    UIManager.Instance.ShowError($"Limit Reached! You can only have {buildLimit}.");
                }
                return;
            }
        }

        // now check  if user can actually afford it 
        if (MoneyManager.Instance.CanAfford(cost))
        {
            if (BuildingManager.Instance != null)
            {
                BuildingManager.Instance.SelectObjectToPlace(prefabToBuild, cost);
            }
        }
        else
        {   
            // cant afford so show message
            if (UIManager.Instance != null)
                UIManager.Instance.ShowError("Not enough money!");
        }
    }
}