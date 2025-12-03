using UnityEngine;
using UnityEngine.SceneManagement; // Required to change scenes

public class MainMenuController : MonoBehaviour
{
    public void PlayGame()
    {
        // loads the game
        SceneManager.LoadScene("DurhamFestival"); 
    }

    public void QuitGame()
    {
        // closes the game
        Debug.Log("Quit Game requested");
        Application.Quit();
    }
}