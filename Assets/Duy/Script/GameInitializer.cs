using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameInitializer : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameManager gameManager;
    [SerializeField] private BossSetup bossSetup;
    [SerializeField] private UIManager uiManager;

    private void Awake()
    {
        // Ensure we have references to needed components
        if (gameManager == null)
            gameManager = FindFirstObjectByType<GameManager>();

        if (bossSetup == null)
            bossSetup = FindFirstObjectByType<BossSetup>();

        if (uiManager == null)
            uiManager = FindFirstObjectByType<UIManager>();
    }

    private void Start()
    {
        // Delay the initialization to ensure all components are ready
        StartCoroutine(DelayedInitialization());
    }

    private IEnumerator DelayedInitialization()
    {
        // Wait for one frame to ensure all objects are initialized
        yield return null;

        // Now initialize the game sequence - first the game manager
        if (gameManager != null)
        {
            gameManager.StartNewGame();
        }
        else
        {
            Debug.LogError("GameInitializer: No GameManager found!");
        }

        // Boss setup after game initialized
        if (bossSetup != null)
        {
            // Wait a frame to ensure GameManager initialization is complete
            yield return null;
            bossSetup.InitializeBoss();
        }

        // Make sure the UI is updated
        if (uiManager != null)
        {
            uiManager.UpdateHUD();
        }
    }

    public void RestartGame()
    {
        StartCoroutine(DelayedInitialization());
    }
}