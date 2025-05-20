using System.Collections;
using UnityEngine;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Game Settings")]
    [SerializeField] private int bossWeaknessCount = 4;

    [Header("References")]
    [SerializeField] private PlayerController playerPrefab;
    [SerializeField] private Transform playerSpawnPoint;
    [SerializeField] private BossController bossController;

    private LineageSystem lineageSystem;
    private PlayerController currentPlayer;
    private List<TraitType> bossWeaknesses = new List<TraitType>();
    private bool gameActive = false;
    private bool traitSelectionInProgress = false;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        lineageSystem = GetComponent<LineageSystem>();
        if (lineageSystem == null)
            lineageSystem = gameObject.AddComponent<LineageSystem>();

        // Set max generations based on boss weaknesses
        if (lineageSystem != null)
        {
            lineageSystem.maxGenerations = bossWeaknessCount + 1;
        }
    }

    public void StartNewGame()
    {
        Debug.Log("Starting new game...");

        // Clear any existing game state
        if (currentPlayer != null)
        {
            Destroy(currentPlayer.gameObject);
            currentPlayer = null;
        }

        gameActive = true;
        traitSelectionInProgress = true;

        // Generate random boss weaknesses
        GenerateBossWeaknesses();

        // Initialize LineageSystem for a fresh start
        if (lineageSystem != null)
        {
            lineageSystem.StartNewGame();
        }
        else
        {
            Debug.LogError("LineageSystem component missing!");
        }
    }

    private void GenerateBossWeaknesses()
    {
        bossWeaknesses.Clear();
        List<Trait> randomTraits = TraitManager.Instance.GetRandomTraits(bossWeaknessCount);

        foreach (Trait trait in randomTraits)
        {
            bossWeaknesses.Add(trait.type);
        }

        Debug.Log("Boss weaknesses generated: " + string.Join(", ", bossWeaknesses));

        // If we have a boss controller, update its weaknesses
        if (bossController != null)
        {
            // There's no direct setter for weaknesses, would need to modify BossController
            // For now, we just generate them and they're fetched when needed
        }
    }

    public void TraitSelected(TraitType selectedTrait)
    {
        if (!traitSelectionInProgress)
        {
            Debug.LogWarning("Trait selection received but not in progress!");
            return;
        }

        traitSelectionInProgress = false;
        Debug.Log("Trait selected: " + selectedTrait);

        // Add trait to lineage
        lineageSystem.AddTrait(selectedTrait);

        // Spawn player if not exists
        if (currentPlayer == null)
        {
            SpawnPlayer();
        }
        else
        {
            // Update player with new traits
            UpdatePlayerTraits();
        }

        // Resume gameplay
        ResumeGameplay();
    }

    private void SpawnPlayer()
    {
        if (playerPrefab != null && playerSpawnPoint != null)
        {
            currentPlayer = Instantiate(playerPrefab, playerSpawnPoint.position, Quaternion.identity);
            UpdatePlayerTraits();
            Debug.Log("Player spawned");
        }
        else
        {
            Debug.LogError("Cannot spawn player - missing prefab or spawn point");
        }
    }

    private void UpdatePlayerTraits()
    {
        if (currentPlayer == null)
        {
            Debug.LogError("Cannot update player traits - player is null");
            return;
        }

        // Update the player with traits from lineage system
        PlayerTraitSystem traitSystem = currentPlayer.GetComponent<PlayerTraitSystem>();
        if (traitSystem != null)
        {
            traitSystem.SetTraits(lineageSystem.GetInheritedTraits());
            Debug.Log("Player traits updated");
        }
        else
        {
            Debug.LogError("PlayerTraitSystem component not found on player");
        }
    }

    private void ResumeGameplay()
    {
        // Resume turn-based gameplay
        TurnManager turnManager = TurnManager.Instance;
        if (turnManager != null && currentPlayer != null && bossController != null)
        {
            // Start or resume battle
            turnManager.StartBattle(currentPlayer, bossController);
            Debug.Log("Battle resumed with turn manager");
        }
        else
        {
            Debug.LogError("Cannot resume gameplay - missing components");
        }
    }

    public void PlayerDied()
    {
        Debug.Log("Player died");

        // Clean up current player
        if (currentPlayer != null)
        {
            Destroy(currentPlayer.gameObject);
            currentPlayer = null;
        }

        // Advance to next generation
        lineageSystem.AdvanceGeneration();

        // Check if we've reached game over condition
        if (lineageSystem.IsGameOver())
        {
            GameOver(false);
        }
        else
        {
            // Start trait selection for next generation
            traitSelectionInProgress = true;

            // Show trait selection UI
            UIManager uiManager = FindFirstObjectByType<UIManager>();
            if (uiManager != null)
            {
                uiManager.ShowTraitSelection();
            }
        }
    }

    public void GameOver(bool victory)
    {
        gameActive = false;
        Debug.Log("Game over. Victory: " + victory);

        // Show game over UI
        UIManager uiManager = FindFirstObjectByType<UIManager>();
        if (uiManager != null)
        {
            uiManager.ShowGameOver(victory);
        }
    }

    public List<TraitType> GetBossWeaknesses()
    {
        return new List<TraitType>(bossWeaknesses);
    }

    public int GetMaxGenerations()
    {
        return bossWeaknessCount + 1;
    }

    public bool IsGameActive()
    {
        return gameActive;
    }
}