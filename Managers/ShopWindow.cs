using UnityEngine;

public class ShopWindow : MonoBehaviour
{
    // This function flips the switch (On becomes Off, Off becomes On)
public void ToggleVisibility()
    {
        // show or hide shop window if button is clicked
        bool isActive = gameObject.activeSelf;
        gameObject.SetActive(!isActive);
    }
    
    public void CloseShop()
    {
        // close shop window
        gameObject.SetActive(false);
    }
}