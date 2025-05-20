using System.Collections.Generic;
using UnityEngine;
using System.Collections;

public enum TurnState
{
    PlayerTurn,
    EnemyTurn,
    Dodging,
    GameOver
}

public class TurnManager : MonoBehaviour
{
    public static TurnManager Instance;

    [SerializeField] private float turnDelay = 1.0f;

    private TurnState currentState;
    private PlayerController player;
    private BossController boss;
    private bool processingTurn = false;

    public delegate void TurnChangeDelegate(TurnState newState);
    public event TurnChangeDelegate OnTurnChanged;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        currentState = TurnState.GameOver; // Start inactive
    }

    public void StartBattle(PlayerController newPlayer, BossController newBoss)
    {
        player = newPlayer;
        boss = newBoss;

        // Start with player's turn
        SetTurnState(TurnState.PlayerTurn);
    }

    public void EndPlayerTurn()
    {
        if (currentState != TurnState.PlayerTurn) return;

        StartCoroutine(DelayedTurnChange(TurnState.EnemyTurn));
    }

    private IEnumerator ProcessCounterAttack()
    {
        processingTurn = true;

        // Small delay for visual clarity before counter attack
        yield return new WaitForSeconds(0.5f);

        Debug.Log("Processing counter attack from dodges: " + player.GetDodgeCount());

        // Execute counter attack
        player.CounterAttack();

        // Wait for counter attack animation to finish
        yield return new WaitForSeconds(1.0f);

        // Reset dodge counter after using it
        player.ResetDodgeCount();

        // Now move to player turn
        processingTurn = false;
        StartCoroutine(DelayedTurnChange(TurnState.PlayerTurn));
    }

    public void EndBattle(bool playerWon)
    {
        SetTurnState(TurnState.GameOver);
        GameManager.Instance.GameOver(playerWon);
    }

    public IEnumerator DelayedTurnChange(TurnState nextState)
    {
        yield return new WaitForSeconds(turnDelay);
        SetTurnState(nextState);
    }

    private void SetTurnState(TurnState newState)
    {
        currentState = newState;

        if (OnTurnChanged != null)
            OnTurnChanged(currentState);
    }

    public TurnState GetCurrentState()
    {
        return currentState;
    }

    public void EndEnemyTurn()
    {
        Debug.Log("Enemy turn is ending");
        if (currentState != TurnState.EnemyTurn) return;

        // Check if player has accumulated dodge count for counter attack
        if (player != null && player.GetDodgeCount() > 0)
        {
            Debug.Log("Player has dodge count: " + player.GetDodgeCount() + ". Triggering counter attack!");
            StartCoroutine(ProcessCounterAttack());
        }
        else
        {
            StartCoroutine(DelayedTurnChange(TurnState.PlayerTurn));
        }
    }
}