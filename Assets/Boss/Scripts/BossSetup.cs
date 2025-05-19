using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Add this component to your boss GameObject in the scene
public class BossSetup : MonoBehaviour 
{
    [Header("References")]
    [SerializeField] private BossController bossController;
    [SerializeField] private Transform bossSpawnPoint;
    
    void Start()
    {
        if (bossController == null)
            bossController = GetComponent<BossController>();
            
        // Make sure boss is properly positioned
        if (bossSpawnPoint != null)
            transform.position = bossSpawnPoint.position;
    }
    
    public void InitializeBoss()
    {
        // This method can be called from GameManager when starting a new game
        
        // Reset boss health and state
        if (bossController != null)
        {
            bossController.currentHealth = bossController.maxHealth;
        }
        
        // Register boss with TurnManager
        PlayerController player = FindFirstObjectByType<PlayerController>();
        if (player != null && bossController != null)
        {
            TurnManager.Instance.StartBattle(player, bossController);
        }
    }
}