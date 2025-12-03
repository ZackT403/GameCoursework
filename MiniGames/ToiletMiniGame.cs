using UnityEngine;
using UnityEngine.UI;
using TMPro; // Add this for text support

public class ToiletMiniGame : MonoBehaviour
{
    public static ToiletMiniGame Instance;

    [Header("UI References")]
    public GameObject panel;
    public Slider plungerSlider;
    public TextMeshProUGUI progressText;
    
    [Header("Settings")]
    public int scrubsToWin = 15;
    
    private int currentScrubs = 0;
    private Attraction currentToilet;
    private bool wasAtTop = false; 

    void Awake()
    {
        // hide panel by default
        Instance = this;
        if (panel != null){
            panel.SetActive(false);
        } 
    }

    public void OpenMiniGame(Attraction toilet)
    {
        if (toilet == null){
            return;
        } 

        currentToilet = toilet;
        panel.SetActive(true);
        // reset mini game state        
        currentScrubs = 0;
        plungerSlider.value = 0.5f; // set the slider to the middle
        UpdateUI();
        // ensure teh slider can be interacted with        
        plungerSlider.interactable = true;
    }

    public void OnSliderMoved(float val)
    {
        // set the zones that determine a scrubs
        bool isAtTop = (val > 0.85f);
        bool isAtBottom = (val < 0.15f);
        // check if slider has moved from zones
        if (wasAtTop && isAtBottom)
        {
            // add to scrub count
            CountScrub();
            wasAtTop = false;
        }
        else if (!wasAtTop && isAtTop)
        {
            // add to scrub count
            CountScrub();
            wasAtTop = true; 
        }
    }

    void CountScrub()
    {
        currentScrubs++;
        UpdateUI();
        // if scrubs is enough win
        if (currentScrubs >= scrubsToWin)
        {
            WinGame();
        }
    }

    void UpdateUI()
    {
        if (progressText != null)
        {
            // update ui to show how many more scrubs are needed
            progressText.text = $"Plunging: {currentScrubs} / {scrubsToWin}";
        }
    }

    void WinGame()
    {
        // prevent slider from being moved
        plungerSlider.interactable = false;
        
        if (currentToilet != null)
        {
            // remove clog from toilet
            currentToilet.FixClog();
        }

        Invoke("CloseGame", 0.5f);
    }

    public void CloseGame()
    {
        // close pannel
        panel.SetActive(false);
        currentToilet = null;
    }
}