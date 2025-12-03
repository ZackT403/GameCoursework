using UnityEngine;
using System.Collections;

public class UpgradeManager : MonoBehaviour
{
    public static UpgradeManager Instance;

    [Header("Upgrade States")]
    public bool hasAutoCleaners = false;
    public int buildingLevel = 1;

    void Awake()
    {
        Instance = this;
    }

    public void UnlockAutoCleaners(int cost)
    {
        // handle hiring cleaners
        // if cleaner is already hired ignore
        if (hasAutoCleaners) {
            return;
        };
        // spend money and start the cleaners
        if (MoneyManager.Instance.SpendMoney(cost))
        {
            hasAutoCleaners = true;            
            StartCoroutine(AutoCleanRoutine());
        }
        else
        {
            if (UIManager.Instance != null){
                UIManager.Instance.ShowError("Not enough money to hire cleaners!");
            }
        }
    }

    IEnumerator AutoCleanRoutine()
    {
        while (true)
        {
            // wait 5 seconds before cleaning
            yield return new WaitForSeconds(5.0f);
            // only clean if the game is live
            if (GameManager.Instance != null && GameManager.Instance.isLive)
            {
                // find all the litter in the scene
                GameObject[] trash = GameObject.FindGameObjectsWithTag("Litter");
                // clean 3 peices of rubbish per cycle
                int cleanedCount = 0;
                foreach(GameObject t in trash)
                {
                    // remove the litter
                    Destroy(t);
                    cleanedCount++;
                    if (cleanedCount >= 3){
                        break;
                    }
                }
            }
        }
    }

    public void UpgradeBuildings(int cost)
    {
        // handle upgrade building
        if (MoneyManager.Instance.SpendMoney(cost))
        {
            buildingLevel++;
            // notify all active buildings to update stats            
            UpdateAllBuildings();
        }
        else
        {
            if (UIManager.Instance != null){
                UIManager.Instance.ShowError("Not enough money for upgrade!");
            }
        }
    }

    void UpdateAllBuildings()
    {
        // find all buildings
        Attraction[] buildings = FindObjectsByType<Attraction>(FindObjectsSortMode.None);
        // apply the new levels to each
        foreach (Attraction attr in buildings)
        {
            attr.ApplyLevelStats();
        }
    }
}