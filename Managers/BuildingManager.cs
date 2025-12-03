using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems; // Required for UI blocking

public class BuildingManager : MonoBehaviour
{
    public static BuildingManager Instance;

    [Header("Settings")]
    public Tilemap groundTilemap;
    public LayerMask buildingLayer;
    public Collider2D buildableZone; 
    
    private GameObject pendingObject;
    private int pendingCost;
    private bool isMovingExistingObject = false; 
    private Vector3 originalPosition; 

    private SpriteRenderer pendingSprite;
    private BoxCollider2D pendingCollider;

    void Awake()
    {
        Instance = this;
    }
    // two fucntions to check if holding an object
    // litter verion of function
    public bool IsHoldingObject()
    {
        return pendingObject != null;
    }

    // attraction version of function
    public bool IsHoldingObject(GameObject obj)
    {
        return pendingObject == obj;
    }


    void Update()
    {
        // do not allow building while event is live
        if (GameManager.Instance != null && GameManager.Instance.isLive)
        {
            if (pendingObject != null){ 
                CancelBuilding();
            }
            return; 
        }

        // make object follow mouse
        if (pendingObject != null)
        {
            HandleMovement();
            HandleRotation();
        }

        // stop if clicking on a ui button
        if (EventSystem.current.IsPointerOverGameObject()){
            return;
        }

        // handle picking up and placing buildings
        if (pendingObject == null)
        {
            // if clickd try to pick up object
            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                TryPickupObject();
            }
        }
        else
        {
            // if holding object then handle placement 
            HandleBuildingActions();
        }
    }

    void TryPickupObject()
    {
        Vector2 screenPos = Mouse.current.position.ReadValue();
        Vector3 worldPoint = Camera.main.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, 10f));
        Vector2 mousePos2D = new Vector2(worldPoint.x, worldPoint.y);

        // get all hits at mouse positon
        RaycastHit2D[] hits = Physics2D.RaycastAll(mousePos2D, Vector2.zero);

        foreach (RaycastHit2D hit in hits)
        {
            GameObject obj = hit.collider.gameObject;

            // check if litter is clicked
            Litter litter = obj.GetComponent<Litter>();
            if (litter != null)
            {
                // handle cleaning litter
                litter.CleanUp();
                return; 
            }

            // check if an attraction is clicked
            Attraction attr = obj.GetComponent<Attraction>();
            
            // check if the attraciton is placed
            if (attr != null && attr.isPlaced)
            {
                // if its a clogged toilet it must be cleaned not picked up
                if (attr.isClogged)
                {
                    if (ToiletMiniGame.Instance != null)
                    {
                        // if toilet is clicked spawn mini game
                        ToiletMiniGame.Instance.OpenMiniGame(attr);
                    }
                    return; 
                }
                // pickup logic
                pendingObject = obj;
                pendingCost = attr.constructionCost;
                isMovingExistingObject = true;
                originalPosition = obj.transform.position;

                attr.PickupAttraction(); // disable attraction logic
                
                pendingCollider = obj.GetComponent<BoxCollider2D>();
                // disable collider while moving
                if (pendingCollider != null){
                    pendingCollider.enabled = false;
                } 
                // get sprite for colour changes
                pendingSprite = obj.GetComponent<SpriteRenderer>();                
                return; 
            }
        }
    }

    void HandleBuildingActions()
    {
        bool canPlace = IsValidPosition();
        // change colour to red if it cant be placed
        UpdateColor(canPlace);

        // left click to place
        if (Mouse.current.leftButton.wasPressedThisFrame && canPlace) 
        {
            // place object if moving existing
            if (isMovingExistingObject)
            {
                PlaceObject(true);
            }
            else
            {
                // check if enough money to build
                if (MoneyManager.Instance.CanAfford(pendingCost))
                {
                    PlaceObject(false);
                }
                else
                {
                    if (UIManager.Instance != null)
                        UIManager.Instance.ShowError("Not enough money to build this!");
                }
            }
        }
        // if delete key pressed sell the object
        if (Keyboard.current.deleteKey.wasPressedThisFrame)
        {
            SellObject();
        }

        // right click to cancle placing building
        if (Mouse.current.rightButton.wasPressedThisFrame)
        {
            CancelBuilding();
        }
    }


    public void SelectObjectToPlace(GameObject prefab, int cost)
    {
        // block if event is live
        if (GameManager.Instance.isLive){
            return;
        } 
        // destory existing object if any
        if (pendingObject != null){
            Destroy(pendingObject);
        }
        // instantiate new object to place
        pendingObject = Instantiate(prefab);
        // track cost
        pendingCost = cost;
        isMovingExistingObject = false; 
        
        pendingSprite = pendingObject.GetComponent<SpriteRenderer>();
        pendingCollider = pendingObject.GetComponent<BoxCollider2D>();
        
        // save cost for later if refund is needed
        Attraction attr = pendingObject.GetComponent<Attraction>();
        if (attr != null){
            attr.constructionCost = cost;
        }

        // disable collider while moving
        if (pendingCollider != null){
            pendingCollider.enabled = false;
        }
    }

    void PlaceObject(bool isFree)
    {
        // enable colliders
        if (pendingCollider != null){
            pendingCollider.enabled = true;
        }
        // re enable attraction logic
        Attraction attr = pendingObject.GetComponent<Attraction>();
        if (attr != null){
            attr.PlaceAttraction();
        } 
        // deduct money if not free
        if (!isFree)
        {
            MoneyManager.Instance.SpendMoney(pendingCost);
        }
        
        pendingObject = null;
    }

    void SellObject()
    {   
        // sell object for half the price
        int refundAmount = pendingCost / 2;
        // add refunded money
        MoneyManager.Instance.AddMoney(refundAmount);
        // despwawn object
        Destroy(pendingObject);
        pendingObject = null;
    }

    void CancelBuilding()
    {
        if (isMovingExistingObject)
        {
            // place back to original position
            pendingObject.transform.position = originalPosition;
            PlaceObject(true); 
        }
        else
        {
            // if its a new object just delete it 
            Destroy(pendingObject);
            pendingObject = null;
        }
    }

    void HandleMovement()
    {
        if (Mouse.current == null){
            return;
        }
        // get mouse postition in world space
        Vector2 screenPos = Mouse.current.position.ReadValue();
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, 10f));
        mousePos.z = 0;

        // grid snapping if the tilemap exists 
        if (groundTilemap != null)
        {
            Vector3Int cellPos = groundTilemap.WorldToCell(mousePos);
            Vector3 snappedPos = groundTilemap.CellToWorld(cellPos); 
            pendingObject.transform.position = snappedPos;
        }
        else 
        {
            pendingObject.transform.position = mousePos;
        }
    }

    void HandleRotation()
    {
        // rotate the buildings if r is pressed
        if (Keyboard.current.rKey.wasPressedThisFrame)
        {
            BuildingRotator rotator = pendingObject.GetComponent<BuildingRotator>();
            if (rotator != null) rotator.Rotate();
        }
    }

    bool IsValidPosition()
    {
        // calculate if the pending object can be placed in its current postition
        float scaleX = pendingObject.transform.localScale.x;
        float scaleY = pendingObject.transform.localScale.y;

        Vector2 scaledOffset;
        scaledOffset.x = pendingCollider.offset.x * scaleX;
        scaledOffset.y = pendingCollider.offset.y * scaleY;

        Vector2 center = (Vector2)pendingObject.transform.position + scaledOffset;
        
        // check if the object is in the buildable zone 
        if (buildableZone != null)
        {
            if (!buildableZone.OverlapPoint(center)){
                return false;
            } 
        }

        // check if the object overlaps any other buildings
        Vector2 realSize;
        realSize.x = pendingCollider.size.x * scaleX;
        realSize.y = pendingCollider.size.y * scaleY;
        // reduce the size slightly to prevent edge cases
        Vector2 checkSize = realSize * 0.9f; 
        float angle = pendingObject.transform.rotation.eulerAngles.z;
        // check for overlaps with colliders in building layer
        Collider2D[] hits = Physics2D.OverlapBoxAll(center, checkSize, angle, buildingLayer);
        // dont care about trigger colliders
        foreach (Collider2D hit in hits)
        {
            if (!hit.isTrigger){
                return false;
            } 
        }

        return true;
    }

    void UpdateColor(bool isValid)
    {   
        // change to red to indicate it cant be placed 
        if (pendingSprite != null) 
            pendingSprite.color = isValid ? Color.white : Color.red;
    }
}