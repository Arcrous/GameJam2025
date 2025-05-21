using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using static BossAnimationController;

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
    private Dictionary<TraitType, GameObject> weaknessIconMap = new Dictionary<TraitType, GameObject>();
    private Color onlyOneGoldColor = new Color(1f, 0.84f, 0, 1); // Gold color for "Only One" trait

    [Header("UI")]
    public Image healthBar;
    [SerializeField] float fillSpeed = 0.5f; // Speed of health bar fill animation
    [SerializeField] Ease easingType; // Speed of health bar fill animation
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

        // Initialize health
        currentHealth = maxHealth;
    }

    public void CreateWeakness()
    {
        // Get weaknesses from GameManager
        weaknesses = GameManager.Instance.GetBossWeaknesses();
        // Setup weakness UI
        SetupWeaknessIcons();
    }

    private void SetupWeaknessIcons()
    {
        Debug.Log("Setting up weakness icons");
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
                    Image weaknessIcon = Instantiate(weaknessIconPrefab, weaknessIconsContainer).GetComponent<Image>();

                    //Get the border image
                    Image borderImage = weaknessIcon.transform.GetChild(0).GetComponentInChildren<Image>();

                    TextMeshProUGUI weaknessHint = borderImage.GetComponentInChildren<TextMeshProUGUI>();

                    //Set up regular weaknesses
                    if (borderImage != null)
                    {
                        borderImage.color = Color.white;
                    }

                    if (weaknessHint != null)
                    {
                        weaknessHint.color = trait.displayColor;
                    }
                }
            }

            OnlyOneBoss onlyOneBoss = GetComponent<OnlyOneBoss>();
            if (onlyOneBoss != null)
            {
                //Create the weakness icon
                Image onlyOneIcon = Instantiate(weaknessIconPrefab, weaknessIconsContainer).GetComponent<Image>();

                //Get the border image
                Image borderImage = onlyOneIcon.transform.GetChild(0).GetComponentInChildren<Image>();

                //Get the TextMeshPro component
                TextMeshProUGUI questionMark = borderImage.GetComponentInChildren<TextMeshProUGUI>();

                if (borderImage != null)
                {
                    borderImage.color = onlyOneGoldColor; // Set to gold color
                }

                if (questionMark != null)
                {
                    questionMark.color = Color.white;
                }
            }
        }
    }

    public void RevealWeakness(TraitType traitType)
    {
        foreach (Transform weaknessContainer in weaknessIconsContainer)
        {
            Image mainImage = weaknessContainer.GetComponent<Image>();
            Image borderImage = mainImage.transform.GetChild(0).GetComponentInChildren<Image>();
            TextMeshProUGUI questionMark = borderImage.GetComponentInChildren<TextMeshProUGUI>();

            //Check if this is the weakness we want to reveal
            OnlyOneBoss onlyOneBoss = GetComponent<OnlyOneBoss>();
            bool isOnlyOneTrait = onlyOneBoss != null && traitType == onlyOneBoss.GetOnlyOneTrait();

            bool isMatch = false;

            if (isOnlyOneTrait && borderImage.color == onlyOneGoldColor)
            {
                isMatch = true;
            }
            else if (!isOnlyOneTrait)
            {
                Trait trait = TraitManager.Instance.GetTraitByType(traitType);
                if (trait != null && questionMark.color == trait.displayColor)
                {
                    isMatch = true;
                }
            }

            if (isMatch)
            {
                //Get trait info
                Trait trait = TraitManager.Instance.GetTraitByType(traitType);
                if (trait != null)
                {
                    questionMark.text = ""; // Hide the question mark

                    //Update the border color and the icon
                    mainImage.color = Color.white;
                    mainImage.sprite = trait.icon; // Set the icon to the trait icon

                    if (!isOnlyOneTrait)
                    {
                        borderImage.color = trait.displayColor; // Set the border color to the trait color
                    }

                    if (weaknessRevealEffect != null)
                    {
                        Instantiate(weaknessRevealEffect, mainImage.transform.position, Quaternion.identity);
                    }
                    break;
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

    public AttackDirection GetCurrentAttackDirection()
    {
        if (animationController != null)
        {
            return animationController.GetCurrentAttackDirection();
        }
        return AttackDirection.None;
    }

    private IEnumerator ExecuteAttack()
    {
        Debug.Log("Boss is attacking");
        isAttacking = true;

        // Signal attack is coming to allow player to dodge
        if (attackIndicator != null)
            attackIndicator.SetActive(true);

        GetCurrentAttackDirection();

        // Wait for reaction time window
        yield return new WaitForSeconds(attackInterval);

        // Hide attack indicator
        if (attackIndicator != null)
            attackIndicator.SetActive(false);

        // Use animation controller to play the next attack in the pattern
        if (animationController != null)
        {
	    AttackType attackType = animationController.GetCurrentAttackType();
            animationController.PlayNextAttackInPattern();

            // Wait until animation is no longer playing
            while (animationController.IsAnimationPlaying())
            {
                yield return null;
            }

	    // Add attack type-specific delay after animation completes
        if (attackType == AttackType.TripleLeftSwipe || attackType == AttackType.TripleRightSwipe)
        {
            // Longer delay for triple attack patterns
            yield return new WaitForSeconds(2f);
        }
        else
        {
            // Standard delay for other patterns
            yield return new WaitForSeconds(0.5f);
        }

            // Remove this line as damage is now handled in CheckPlayerInCell
            // if (player != null && !player.IsDodging())
            // {
            //     player.TakeDamage(attackPower);
            // }

            turnManager.EndEnemyTurn();
        }
        else
        {
            // Fallback to original behavior if animation controller is missing
            if (animator != null)
                animator.SetTrigger("Attack");

            // Deal damage if player is not dodging or dodging in wrong direction
            if (player != null)
            {
                bool shouldDamage = false;

                if (!player.IsDodging())
                {
                    Debug.Log("Player is not dodging");
                    shouldDamage = true;
                }
                else
                {
                    // Get current attack direction
                    AttackDirection attackDir = GetCurrentAttackDirection();

                    // Check if player is dodging in wrong direction
                    bool isDodgingLeft = player.IsCurrentlyDodgingLeft();
                    if ((attackDir == AttackDirection.Left && !isDodgingLeft) ||
                        (attackDir == AttackDirection.Right && isDodgingLeft) /*||
                        (attackDir == AttackDirection.Both)*/)
                    {
                        Debug.Log("Player dodged in the wrong direction");
                        shouldDamage = true;
                    }
                }

                if (shouldDamage)
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
            }

            // End turn after attack is finished
            turnManager.EndEnemyTurn();
        }

        isAttacking = false;
    }

    public void TakeDamage(int baseDamage, List<TraitType> playerTraits)
    {
        Debug.Log("Boss taking damage: " + baseDamage);
        float damageMultiplier = 1.0f;
        bool wasWeak = false;

        // Check if player has traits that match boss weaknesses
        foreach (TraitType trait in playerTraits)
        {
            if (weaknesses.Contains(trait))
            {
                RevealWeakness(trait); // Show the weakness icon
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

        //If currentHealth is equal to 25% of maxHealth, increase boss damage;
        if (currentHealth <= maxHealth * 0.25f)
        {
            attackPower += 15; // Increase attack power by 5
        }

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
            healthBar.DOFillAmount((float)currentHealth / maxHealth, fillSpeed).SetEase(easingType);
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
}