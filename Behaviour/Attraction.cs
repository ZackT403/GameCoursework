using UnityEngine;

public enum AttractionType { Stage, Food, Toilet, Merch, Bin, Popcorn, CandyFloss, BBQStand, FerrisWheel }

public class Attraction : MonoBehaviour
{
    [Header("Identity")]
    public string attractionName = "Attraction";
    public AttractionType type;

    [Header("Stats")]
    [Range(0, 100)] public int appealScore = 10;
    public int baseSatisfaction = 5;
    
    [Header("Economy")]
    [Range(0f, 1f)] public float profitChance = 0.3f;
    public int productPrice = 5;      
    
    // original product price before any upgrades
    public int baseProductPrice = 5; 

    [HideInInspector] public int constructionCost = 0;

    [Header("Maintenance")]
    public bool isClogged = false;
    public GameObject clogIcon;

    [Header("Crowd Logic")]
    public BoxCollider2D crowdZone; 
    public bool isPlaced = false;

    void Start()
    {
        // give name if not set
        if (attractionName == "Attraction"){
            attractionName = gameObject.name.Replace("(Clone)", "").Trim();
        }

        // if upgrade exists then apply that upgrade
        ApplyLevelStats();

        // regisyer with manager if already placed
        if (!isPlaced)
        {
            bool isGhost = false;
            if (BuildingManager.Instance != null && BuildingManager.Instance.IsHoldingObject(gameObject)){
                isGhost = true;
            }

            if (!isGhost)
            {
                isPlaced = true;
                if (AttractionManager.Instance != null){
                    AttractionManager.Instance.RegisterAttraction(this);
                }
            }
        }
    }
    // aplpy upgrade level stats to attraction
    public void ApplyLevelStats()
    {
        if (UpgradeManager.Instance != null)
        {
            float multiplier = 1.0f + ((UpgradeManager.Instance.buildingLevel - 1) * 0.5f);
            productPrice = Mathf.RoundToInt(baseProductPrice * multiplier);
        }
    }

    public void PlaceAttraction()
    {
        // handle placement logic and registration
        isPlaced = true;
        if (AttractionManager.Instance != null){ 
            AttractionManager.Instance.RegisterAttraction(this);
        }   
    }

    public void PickupAttraction()
    {
        // handle pickup logic and unregistration
        isPlaced = false;
        if (AttractionManager.Instance != null){ 
            AttractionManager.Instance.UnregisterAttraction(this);
        }   
    }

    void OnDestroy()
    {
        // handle deletng the attraction
        if (isPlaced && AttractionManager.Instance != null){ 
            AttractionManager.Instance.UnregisterAttraction(this);
        }   
    }

    public void TryClogToilet()
    {
        // if the attraction type is a toiulet attempt to clog for minigame
        if (type != AttractionType.Toilet){
            return;
        }
        if (Random.value < 0.3f) 
        {
            isClogged = true;
            if (clogIcon != null){
                // display the icon so user knows it changed
                clogIcon.SetActive(true);
            }
            // reduce appeal when its clogged so no one wants to use it
            appealScore = 0; 
        }
    }

    public void FixClog()
    {   
        // fix the toilet and reset stats
        isClogged = false;
        if (clogIcon != null){
            clogIcon.SetActive(false);
        }   
        appealScore = 5; 
    }

    public Vector3 GetRandomStandingSpot()
    {
        // generate a random spot for the crowd to walk too
        if (crowdZone == null){
            return transform.position;
        } 
        Bounds bounds = crowdZone.bounds;
        return new Vector3(
            Random.Range(bounds.min.x, bounds.max.x),
            Random.Range(bounds.min.y, bounds.max.y),
            0
        );
    }
}