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

    public void TriggerDodge()
    {
        if (currentState != TurnState.EnemyTurn) return;

        SetTurnState(TurnState.Dodging);

        // Player attempts to dodge
        if (player != null)
            player.TryDodge();

        // Back to player turn if successful dodge
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

        // Handle enemy turn automatically
        if (nextState == TurnState.EnemyTurn && boss != null)
        {
            //boss.TakeTurn();
        }
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
        if (currentState != TurnState.EnemyTurn) return;

        StartCoroutine(DelayedTurnChange(TurnState.PlayerTurn));
    }
}