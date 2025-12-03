using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("Main HUD")]
    public TextMeshProUGUI moneyText;
    public TextMeshProUGUI dayText;
    public TextMeshProUGUI satisfactionText;
    public TextMeshProUGUI startButtonText; 
    
    public TextMeshProUGUI timeText; 

    [Header("Popups")]
    public GameObject errorPanel;     
    public TextMeshProUGUI errorText; 
    public GameObject gameOverPanel;
    public TextMeshProUGUI gameOverTitle;
    public TextMeshProUGUI gameOverReason;

    void Awake()
    {
        Instance = this;
    }

    public void UpdateTimerUI(float timeRemaining)
    {
        // update the timer
        if (timeText != null)
        {
            // format the time so it looks good
            int minutes = Mathf.FloorToInt(timeRemaining / 60);
            int seconds = Mathf.FloorToInt(timeRemaining % 60);
            // format time string            
            timeText.text = string.Format("Time: {0:00}:{1:00}", minutes, seconds);
            // change colour to red if less than 10 seconds            
            if (timeRemaining <= 10){
                timeText.color = Color.red;
            } 
            else{
                timeText.color = Color.white;
            } 
        }
    }
    public void SetStartButtonState(bool isActive, string text)
    {
        if (startButtonText != null)
        {
            // update start button text
            startButtonText.text = text;
            // find the button componenet             
            UnityEngine.UI.Button btn = startButtonText.GetComponentInParent<UnityEngine.UI.Button>();
            if (btn != null)
            {
                // toggle button interactability
                btn.interactable = isActive; 
            }
        }
    }
    public void ShowGameOver(bool isWin, string reason)
    // obsoletes currently no game over conditions 
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            
            if (isWin)
            {
                gameOverTitle.text = "<color=green>FESTIVAL SUCCESS!</color>";
                gameOverTitle.color = Color.green;
            }
            else
            {
                gameOverTitle.text = "<color=red>GAME OVER</color>";
                gameOverTitle.color = Color.red;
            }

            gameOverReason.text = reason;
        }
    }

    public void RestartGame()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void ShowError(string message) {
        //show error panel with custom message
        if(errorPanel!=null){
            errorPanel.SetActive(true);
            errorText.text=message;
        } 
    }
    public void CloseErrorPanel() {
        // close error pannel
        if(errorPanel!=null){
            errorPanel.SetActive(false);
        }
    }
    public void UpdateMoneyUI(int amount) {
        // show money ammount
        if(moneyText!=null){
            moneyText.text = "£" + amount.ToString("N0");
        }  
    }
    public void UpdateDayUI(int day) {
        // show current day
        if(dayText!=null){
            dayText.text = "Day " + day; 
        }
    }
    public void UpdateSatisfactionUI(int value) {
        // show current satisfaction
        if(satisfactionText!=null) {
            satisfactionText.text = "Happiness: " + value + "%";
        } 
    }
    public void UpdateButtonText(string newText) {
        if(startButtonText!=null){
            startButtonText.text = newText; 
        } 
    }
}