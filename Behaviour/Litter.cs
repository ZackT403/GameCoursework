using UnityEngine;
using UnityEngine.EventSystems;

public class Litter : MonoBehaviour
{
    void OnMouseDown()
    {
        // prevent cleaning if clicking on UI
        if (EventSystem.current.IsPointerOverGameObject()){
            return;
        }

        // prevent cleaning while trying to build
        if (BuildingManager.Instance != null && BuildingManager.Instance.IsHoldingObject()){
            return;
        }
        // clean up litter if clicked
        CleanUp();
    }

    public void CleanUp()
    {
        // destory litter        
        Destroy(gameObject);
    }
}