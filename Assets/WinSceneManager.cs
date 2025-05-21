using UnityEngine;
using UnityEngine.SceneManagement;

public class WinSceneManager : MonoBehaviour
{
    // Call this to go back to the Main Menu scene
    public void MainMenu()
    {
        // Replace "MainMenu" with your main menu scene name
        SceneManager.LoadScene("Main Menu");
    }

    // Call this to quit the application
    public void Quit()
    {
        Debug.Log("Quit game");  // For editor/testing
        Application.Quit();

        // Note: Application.Quit() does not work in the editor,
        // so this line will only work in builds.
    }
}