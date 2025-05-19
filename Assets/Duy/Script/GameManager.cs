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

    private LineageSystem lineageSystem;
    private PlayerController currentPlayer;
    private List<TraitType> bossWeaknesses = new List<TraitType>();
    private bool gameActive = false;

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
    }

    public void StartNewGame()
    {
        gameActive = true;

        // Generate random boss weaknesses
        GenerateBossWeaknesses();

        // Start lineage system
        lineageSystem.StartNewGame();
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
    }

    public void TraitSelected(TraitType selectedTrait)
    {
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
        }
    }

    private void UpdatePlayerTraits()
    {
        // This would update the player with traits from lineage system
        // Will implement in PlayerTraitSystem below
        PlayerTraitSystem traitSystem = currentPlayer.GetComponent<PlayerTraitSystem>();
        if (traitSystem != null)
        {
            traitSystem.SetTraits(lineageSystem.GetInheritedTraits());
        }
    }

    private void ResumeGameplay()
    {
        // Resume turn-based gameplay
        // Would typically call TurnManager.ResumeGame() or similar
    }

    public void PlayerDied()
    {
        // Clean up current player
        if (currentPlayer != null)
        {
            Destroy(currentPlayer.gameObject);
            currentPlayer = null;
        }

        // Advance to next generation
        lineageSystem.AdvanceGeneration();
    }

    public void GameOver(bool victory)
    {
        gameActive = false;

        // Show game over UI
        FindObjectOfType<UIManager>().ShowGameOver(victory);
    }

    public List<TraitType> GetBossWeaknesses()
    {
        return new List<TraitType>(bossWeaknesses);
    }

    public int GetMaxGenerations()
    {
        return bossWeaknessCount + 1;
    }
}
