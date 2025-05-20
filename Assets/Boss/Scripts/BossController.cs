using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BossController : MonoBehaviour
{
    [Header("Stats")]
    public int currentHealth;
    public int maxHealth = 200;
    public int attackPower = 15;
    public float attackInterval = 1.5f;
    
    [Header("Weaknesses")]
    public int numWeaknesses = 4;
    private List<TraitType> weaknesses = new List<TraitType>();
    
    [Header("UI")]
    public Image healthBar;
    public Transform weaknessIconsContainer;
    public Image weaknessIconPrefab;
    public GameObject attackIndicator;
    
    [Header("Effects")]
    public GameObject attackEffect;
    public ParticleSystem damageParticle;
    public GameObject weaknessRevealEffect;
    
    private PlayerController player;
    private TurnManager turnManager;
    private Animator animator;
    private BossAnimationController animationController;
    private float damageMultiplierFromWeakness = 1.5f;
    private bool isAttacking = false;

    private void Start()
    {
        animator = GetComponent<Animator>();
        animationController = GetComponent<BossAnimationController>();
        turnManager = TurnManager.Instance;
        
        // Subscribe to turn change events
        if (turnManager != null)
        {
            turnManager.OnTurnChanged += HandleTurnChanged;
        }
        
        // Get player reference
        player = FindFirstObjectByType<PlayerController>();
        
        // Get weaknesses from GameManager
        weaknesses = GameManager.Instance.GetBossWeaknesses();
        
        // Initialize health
        currentHealth = maxHealth;
        
        // Setup weakness UI
        SetupWeaknessIcons();
    }
    
    private void SetupWeaknessIcons()
    {
        if (weaknessIconsContainer != null && weaknessIconPrefab != null)
        {
            // Clear any existing icons
            foreach (Transform child in weaknessIconsContainer)
            {
                Destroy(child.gameObject);
            }
            
            // Create icons for each weakness
            foreach (TraitType weakness in weaknesses)
            {
                Trait trait = TraitManager.Instance.GetTraitByType(weakness);
                if (trait != null)
                {
                    Image icon = Instantiate(weaknessIconPrefab, weaknessIconsContainer);
                    icon.color = trait.displayColor;
                    
                    // If you have trait icons
                    if (trait.icon != null)
                        icon.sprite = trait.icon;
                }
            }
        }
    }
    
    private void HandleTurnChanged(TurnState newState)
    {
        if (newState == TurnState.EnemyTurn)
        {
            Debug.Log("It's the boss turn");
            // It's the boss's turn
            StartCoroutine(ExecuteAttack());
        }
    }
    
    private IEnumerator ExecuteAttack()
    {
        Debug.Log("Boss is attacking");
        isAttacking = true;
        
        // Signal attack is coming to allow player to dodge
        if (attackIndicator != null)
            attackIndicator.SetActive(true);
        
        // Wait for reaction time window
        yield return new WaitForSeconds(attackInterval);
        
        // Hide attack indicator
        if (attackIndicator != null)
            attackIndicator.SetActive(false);
        
        // Use animation controller to play the next attack in the pattern
        if (animationController != null)
        {
            animationController.PlayNextAttackInPattern();
            
            // Wait until animation is no longer playing
            while (animationController.IsAnimationPlaying())
            {
                yield return null;
            }
        }
        else
        {
            // Fallback to original behavior if animation controller is missing
            if (animator != null)
                animator.SetTrigger("Attack");
                
            // Deal damage if player is not dodging
            if (player != null && !player.IsDodging())
            {
                // Instantiate attack effect if available
                if (attackEffect != null)
                {
                    Vector3 direction = player.transform.position - transform.position;
                    float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                    Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.forward);
                    
                    Instantiate(attackEffect, transform.position, rotation);
                }
                
                player.TakeDamage(attackPower);
            }

            // End turn after attack is finished
            turnManager.EndEnemyTurn();
        }
        
        isAttacking = false;
    }
    
    public void TakeDamage(int baseDamage, List<TraitType> playerTraits)
    {
        float damageMultiplier = 1.0f;
        bool wasWeak = false;
        
        // Check if player has traits that match boss weaknesses
        foreach (TraitType trait in playerTraits)
        {
            if (weaknesses.Contains(trait))
            {
                damageMultiplier += 0.5f; // Stack 50% more damage per weakness
                wasWeak = true;
            }
        }
        
        // Check for "Only One" trait damage bonus
        OnlyOneBoss onlyOneBoss = GetComponent<OnlyOneBoss>();
        if (onlyOneBoss != null)
        {
            float onlyOneMultiplier = onlyOneBoss.GetBonusDamageMultiplier(playerTraits);
            damageMultiplier *= onlyOneMultiplier;
        }
        
        // Calculate final damage
        int finalDamage = Mathf.RoundToInt(baseDamage * damageMultiplier);
        currentHealth -= finalDamage;
        
        // Show damage effect
        if (damageParticle != null)
        {
            damageParticle.Play();
            
            // If the attack hit a weakness, show a special effect
            if (wasWeak && weaknessRevealEffect != null)
                Instantiate(weaknessRevealEffect, transform.position, Quaternion.identity);
        }
        
        // Update health bar
        UpdateHealthBar();
        
        // Check if boss is defeated
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    
    private void UpdateHealthBar()
    {
        if (healthBar != null)
        {
            healthBar.fillAmount = (float)currentHealth / maxHealth;
        }
    }
    
    private void Die()
    {
        // Use animation controller for death animation
        if (animationController != null)
        {
            StartCoroutine(animationController.PlayDeathAnimation());
        }
        else if (animator != null)
        {
            animator.SetTrigger("Die");
        }
        
        // End the battle with player victory after a short delay to allow death animation
        StartCoroutine(DelayedGameEnd());
    }
    
    private IEnumerator DelayedGameEnd()
    {
        // Wait for death animation to complete
        yield return new WaitForSeconds(1.5f);
        
        // End the battle with player victory
        if (turnManager != null)
            turnManager.EndBattle(true);
    }
    
    public bool IsAttacking()
    {
        return isAttacking;
    }
    
    public List<TraitType> GetWeaknesses()
    {
        return new List<TraitType>(weaknesses);
    }
    
    // Method called from animation events or UI
    public void TriggerSpecialAttack()
    {
        // Use animation controller for special attack
        if (animationController != null)
        {
            StartCoroutine(animationController.PlaySpecialAttackAnimation());
        }
        else
        {
            // Fallback to original behavior
            StartCoroutine(SpecialAttackCoroutine());
        }
    }
    
    private IEnumerator SpecialAttackCoroutine()
    {
        isAttacking = true;
        
        // Signal attack is coming but with shorter reaction time
        if (attackIndicator != null)
            attackIndicator.SetActive(true);
        
        yield return new WaitForSeconds(attackInterval * 0.5f);
        
        if (animator != null)
            animator.SetTrigger("SpecialAttack");
        
        if (attackIndicator != null)
            attackIndicator.SetActive(false);
        
        // Deal more damage if player is not dodging
        if (player != null && !player.IsDodging())
        {
            player.TakeDamage(attackPower * 2);
        }
        
        isAttacking = false;
    }
    
    // Called by attack button from player UI
    public void ReceivePlayerAttack()
    {
        if (player != null)
        {
            PlayerTraitSystem traitSystem = player.GetComponent<PlayerTraitSystem>();
            if (traitSystem != null)
            {
                // Get player traits and calculate damage
                List<TraitType> playerTraits = traitSystem.GetPlayerTraits();
                TakeDamage(player.attackPower, playerTraits);
            }
            else
            {
                // Fallback if no trait system
                TakeDamage(player.attackPower, new List<TraitType>());
            }
        }
    }
}