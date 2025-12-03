using UnityEngine;

public class BuildingRotator : MonoBehaviour
{
    [Header("Art Assets")]
    public Sprite frontView;
    public Sprite rightView;
    public Sprite backView;
    public Sprite leftView;

    [Header("Crowd Zones (Drag Child Objects Here)")]
    public BoxCollider2D zoneFront;
    public BoxCollider2D zoneRight;
    public BoxCollider2D zoneBack;
    public BoxCollider2D zoneLeft;

    private SpriteRenderer sr;
    private BoxCollider2D solidCollider;
    private Attraction attractionScript;
    private int currentDirection = 0;
    void Awake()
    {
        // get components
        sr = GetComponent<SpriteRenderer>();
        solidCollider = GetComponent<BoxCollider2D>();
        attractionScript = GetComponent<Attraction>();
    }

    public void Rotate()
    {
        currentDirection++;
        if (currentDirection > 3){
            currentDirection = 0;
        }

        // swap sprites based on rotation
        switch (currentDirection)
        {
            case 0: sr.sprite = frontView; break;
            case 1: sr.sprite = rightView; break;
            case 2: sr.sprite = backView; break;
            case 3: sr.sprite = leftView; break;
        }

        // flip the solid collider dimenstions 
        Vector2 size = solidCollider.size;
        solidCollider.size = new Vector2(size.y, size.x);
        
        Vector2 offset = solidCollider.offset;
        solidCollider.offset = new Vector2(offset.y, offset.x); 

        // swap to diffrent crowd zone
        UpdateCrowdZone();
    }

    void UpdateCrowdZone()
    {
        // hide all zones
        if (zoneFront) zoneFront.gameObject.SetActive(false);
        if (zoneRight) zoneRight.gameObject.SetActive(false);
        if (zoneBack) zoneBack.gameObject.SetActive(false);
        if (zoneLeft) zoneLeft.gameObject.SetActive(false);

        BoxCollider2D newActiveZone = null;

        // pick the correct zone based on direction
        switch (currentDirection)
        {
            case 0: newActiveZone = zoneFront; break;
            case 1: newActiveZone = zoneRight; break;
            case 2: newActiveZone = zoneBack; break;
            case 3: newActiveZone = zoneLeft; break;
        }

        // activate the new zone and tell attraction manager about it
        if (newActiveZone != null)
        {
            newActiveZone.gameObject.SetActive(true);
            
            if (attractionScript != null)
            {
                attractionScript.crowdZone = newActiveZone;
            }
        }
    }
}