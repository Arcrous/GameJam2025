using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    // Call this to start the game
    public void StartGame()
    {
        // Replace "GameScene" with the actual name of your gameplay scene
        SceneManager.LoadScene("Test");
    }

    // Call this to load the instructions scene
    public void Instructions()
    {
        // Replace "Instructions" with your instructions scene name
        SceneManager.LoadScene("Instructions");
    }

    // Call this to quit the game
    public void Quit()
    {
        Debug.Log("Quit game"); // Only visible in the editor
        Application.Quit();
    }
}