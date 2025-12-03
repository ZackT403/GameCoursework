using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance;

    [Header("UI References")]
    public GameObject settingsPanel;
    public Slider speedSlider;
    public Toggle contrastToggle;
    public Toggle hardModeToggle;
    public Image[] uiPanels; 
    // orignal pannel colour 
    private Dictionary<Image, Color> originalColors = new Dictionary<Image, Color>();

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        if (settingsPanel != null){
            settingsPanel.SetActive(false);
        } 
        // store original colors for contrast toggle        
        foreach (Image img in uiPanels)
        {
            if (img != null)
            {
                originalColors[img] = img.color;
            }
        }
        // ensure settings are synced at the start prevents toggle behaving weirdly
        if (contrastToggle != null)
        {
            contrastToggle.isOn = false;
            contrastToggle.onValueChanged.AddListener(OnContrastChanged);
        }
        // init speed slider and add a listener
        if (speedSlider != null)
        {
            speedSlider.value = 1.0f;
            speedSlider.onValueChanged.AddListener(OnSpeedChanged);
        }
    }

    public void ToggleSettings()
    {
        // show or hide settings pannel
        bool isActive = settingsPanel.activeSelf;
        settingsPanel.SetActive(!isActive);
    }

    public void OnSpeedChanged(float value)
    {
        // change game speed based on slider value
        Time.timeScale = value;
    }

    public void OnContrastChanged(bool isHighContrast)
    {
        // define the high contrast colour scheme
        Color highContrastColor = new Color(0, 0, 0, 1); 

        foreach (Image img in uiPanels)
        {
            if (img == null){
                continue;
            }

            if (isHighContrast)
            {
                // apply the new colour schemes
                img.color = highContrastColor;
            }
            else
            {   
                // return to original colours
                if (originalColors.ContainsKey(img))
                {
                    img.color = originalColors[img];
                }
            }
        }
    }

}