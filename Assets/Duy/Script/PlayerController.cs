using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class PlayerController : MonoBehaviour
{
    [Header("Stats")]
    public int currentHealth;
    public int maxHealth = 100;
    public int attackPower = 10;
    public int chargePower = 0;

    [Header("Combat")]
    public float dodgeCooldown = 0.5f;
    private bool canDodge = true;
    private bool isDodging = false;

    [Header("Dodge QTE")]
    [SerializeField] private float dodgeDistance = 1.5f; // How far to dodge
    [SerializeField] private float dodgeDuration = 0.25f; // How long the dodge movement takes
    [SerializeField] private float dodgeInvulnerabilityTime = 0.5f; // How long player is invulnerable during dodge
    [SerializeField] private GameObject dodgeLeftEffect;
    [SerializeField] private GameObject dodgeRightEffect;
    [SerializeField] private AudioClip dodgeSound;
    [SerializeField] private AudioClip hitSound;
    [SerializeField] private KeyCode dodgeLeftKey = KeyCode.A;
    [SerializeField] private KeyCode dodgeRightKey = KeyCode.D;
    private bool dodgingLeft = false;

    [Header("Movement Boundaries")]
    [SerializeField] private float minX = -5f; // Left boundary
    [SerializeField] private float maxX = 5f;  // Right boundary

    [Header("References")]
    [SerializeField] GameObject slashEffect;
    [SerializeField] GameObject counterEffect;
    [SerializeField] GameObject chargeEffect;
    [SerializeField] Transform attackPoint;
    [SerializeField] private AudioClip attackSound;
    [SerializeField] TMPro.TextMeshProUGUI dodgeCounterText;
    public Image healthBar;
    [SerializeField] float fillSpeed = 0.5f; // Speed of health bar fill animation
    [SerializeField] Ease easingType; // Speed of health bar fill animation

    [Header("Visual Feedback")]
    [SerializeField] private Material flashMaterial;
    [SerializeField] private float flashDuration = 0.1f;

    // Internal state
    private float dodgeTimer = 0f;
    private bool isDodgeMoving = false;
    private bool isInvulnerable = false;
    private Vector3 startPosition; // Original position before dodge
    private Vector3 targetPosition; // Target position for dodge
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private Material originalMaterial;
    private AudioSource audioSource;
    private bool isReturning = false; // Flag to track return movement
    private int dodgeCount = 0; // Track successful dodges for counter attacks

    void Awake()
    {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>();

        dodgeCounterText = GameObject.Find("DodgeCounterText").GetComponent<TMPro.TextMeshProUGUI>();
        healthBar = GameObject.Find("HealthFillPlayer").GetComponent<Image>();

        // Create audio source if it doesn't exist
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }

        // Cache original material
        if (spriteRenderer != null)
        {
            originalMaterial = spriteRenderer.material;
        }
    }

    void Start()
    {
        currentHealth = maxHealth;
    }

    void Update()
    {
        if (TurnManager.Instance.GetCurrentState() == TurnState.EnemyTurn && canDodge && !isDodging && !isDodgeMoving && !isReturning)
        {
            // Check for dodge input
            if (Input.GetKeyDown(dodgeLeftKey))
            {
                DodgeInDirection(true); // True = left
            }
            else if (Input.GetKeyDown(dodgeRightKey))
            {
                DodgeInDirection(false); // False = right
            }
        }

        // Handle dodge movement
        if (isDodging && isDodgeMoving)
        {
            dodgeTimer += Time.deltaTime;
            float t = Mathf.Clamp01(dodgeTimer / dodgeDuration);

            // Use Lerp for smoother movement
            transform.position = Vector3.Lerp(startPosition, targetPosition, t);

            // End dodge movement
            if (t >= 1.0f)
            {
                isDodgeMoving = false;

                // After dodge animation completes, return to original position after short delay
                StartCoroutine(ReturnToPositionAfterDelay(0.2f));
            }
        }
    }

    public bool IsCurrentlyDodgingLeft()
    {
        return isDodging && dodgingLeft;
    }


    // New function for QTE-based dodge
    public void DodgeInDirection(bool dodgeLeft)
    {
        if (!canDodge || isDodging || isDodgeMoving || isReturning)
            return;

        // Start dodge sequence
        isDodging = true;
        canDodge = false;
        isDodgeMoving = true;
        dodgeTimer = 0f;

        // Track dodge direction
        dodgingLeft = dodgeLeft;

        // Get current boss attack direction
        BossAnimationController bossAnimController = FindFirstObjectByType<BossAnimationController>();
        BossAnimationController.AttackDirection attackDir = BossAnimationController.AttackDirection.None;

        if (bossAnimController != null)
        {
            attackDir = bossAnimController.GetCurrentAttackDirection();
        }

        // Check if this is a successful dodge (dodging in the correct direction)
        bool successfulDodge = false;

        if (attackDir == BossAnimationController.AttackDirection.Left && dodgeLeft)
        {
            // Boss attacking from right, player dodged left = success
            successfulDodge = true;
        }
        else if (attackDir == BossAnimationController.AttackDirection.Right && !dodgeLeft)
        {
            // Boss attacking from left, player dodged right = success
            successfulDodge = true;
        }
        else if (attackDir == BossAnimationController.AttackDirection.Both)
        {
            // Both directions are valid for dodge (like projectile attack)
            successfulDodge = true;
        }

        // Increment dodge counter only if successful
        if (successfulDodge)
        {
            dodgeCount++;
            Debug.Log("Successful dodge! Dodge count: " + dodgeCount);
            UpdateDodgeCounterUI();
        }

        // Set start and target positions
        startPosition = transform.position;
        float direction = dodgeLeft ? -1f : 1f;

        // Calculate target position but constrain it within boundaries
        Vector3 potentialTarget = startPosition + new Vector3(direction * dodgeDistance, 0, 0);
        float clampedX = Mathf.Clamp(potentialTarget.x, minX, maxX);
        targetPosition = new Vector3(clampedX, startPosition.y, startPosition.z);

        // If the target position can't move the full distance, adjust dodge distance
        if (potentialTarget.x != clampedX)
        {
            // Optional: Visual/audio feedback that dodge is limited
            Debug.Log("Dodge limited by boundary");
        }

        // Play dodge sound
        if (audioSource != null && dodgeSound != null)
        {
            audioSource.PlayOneShot(dodgeSound);
        }

        if (dodgeLeft)
        {
            // Show dodge effect
            if (dodgeLeftEffect != null)
            {
                GameObject dodgeVFX = Instantiate(dodgeLeftEffect, transform.position, transform.rotation);
                Destroy(dodgeVFX, 0.61f);
            }
        }
        else
        {
            // Show dodge effect
            if (dodgeRightEffect != null)
            {
                GameObject dodgeVFX = Instantiate(dodgeRightEffect, transform.position, transform.rotation);
                Destroy(dodgeVFX, 0.61f);
            }
        }

        // Start the invulnerability
        StartCoroutine(DodgeInvulnerabilityCoroutine());
    }

    private IEnumerator DodgeInvulnerabilityCoroutine()
    {
        // Wait for invulnerability duration
        yield return new WaitForSeconds(dodgeInvulnerabilityTime);

        // End invulnerability but keeping dodge state until return is complete
        isInvulnerable = false;

        // Wait for any return movement to complete before ending dodge state
        yield return new WaitUntil(() => !isReturning);

        // End dodge state
        isDodging = false;

        // Start cooldown - making sure we don't subtract time that was already spent in the dodge+return animations
        yield return new WaitForSeconds(dodgeCooldown);
        canDodge = true;
    }

    private IEnumerator ReturnToPositionAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        // Set returning flag
        isReturning = true;

        // Return to original position
        float returnTimer = 0f;
        Vector3 returnStart = transform.position;

        while (returnTimer < dodgeDuration)
        {
            returnTimer += Time.deltaTime;
            float t = Mathf.Clamp01(returnTimer / dodgeDuration);

            // Use Lerp for smoother movement
            transform.position = Vector3.Lerp(returnStart, startPosition, t);

            yield return null;
        }

        transform.position = startPosition;
        isReturning = false;
    }

    public void Attack()
    {
        Debug.Log("Player attacked!");

        // Play attack animation
        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }

        // Instantiate slash effect
        if (slashEffect != null && attackPoint != null)
        {
            GameObject effect = Instantiate(slashEffect, attackPoint.position, attackPoint.rotation);
            Destroy(effect, 0.5f);
        }

        // Find the boss and apply damage
        BossController boss = FindFirstObjectByType<BossController>();
        if (boss != null)
        {
            Debug.Log("No boss found to attack!");

            // Get player traits
            PlayerTraitSystem traitSystem = GetComponent<PlayerTraitSystem>();
            List<TraitType> playerTraits = new List<TraitType>();

            if (traitSystem != null)
            {
                playerTraits = traitSystem.GetPlayerTraits();
            }

            Debug.Log("Player attacked boss!");

            if (chargePower > 0)
            {
                int finalDamage = attackPower * chargePower;
                boss.TakeDamage(finalDamage, playerTraits);
            }
            else
            {
                boss.TakeDamage(attackPower, playerTraits);
            }
        }

        if(chargePower > 0)
        {
            // Reset charge power after attack
            chargePower = 0;
        }

        // Play attack sound
        if (audioSource != null && attackSound != null)
        {
            audioSource.PlayOneShot(attackSound);
        }
    }

    public void CounterAttack()
    {
        if (dodgeCount <= 0) return;

        // Calculate bonus damage based on dodge count
        int bonusDamage = attackPower * dodgeCount;
        Debug.Log($"Counter attacking with {bonusDamage} bonus damage!");

        // Play special counter attack animation if available
        if (animator != null)
        {
            animator.SetTrigger("CounterAttack");
        }

        // Instantiate slash effect
        if (counterEffect != null && attackPoint != null)
        {
            GameObject effect = Instantiate(counterEffect, attackPoint.position, attackPoint.rotation);
            Destroy(effect, 0.5f);
        }

        // Deal damage to boss
        BossController boss = FindFirstObjectByType<BossController>();
        if (boss != null)
        {
            // Get player traits
            PlayerTraitSystem traitSystem = GetComponent<PlayerTraitSystem>();
            List<TraitType> playerTraits = new List<TraitType>();

            if (traitSystem != null)
            {
                playerTraits = traitSystem.GetPlayerTraits();
            }

            // Apply counter attack damage
            boss.TakeDamage(bonusDamage, playerTraits);
        }

        // Reset dodge counter
        dodgeCount = 0;
    }

    public void Charge()
    {
        chargePower += 1;
        if (chargeEffect != null)
        {
            GameObject chargeFX = Instantiate(chargeEffect, this.transform.position, this.transform.rotation);
            Destroy(chargeFX, 0.5f);
        }
    }

    public bool IsDodging()
    {
        return isDodging || isInvulnerable;
    }

    public void TakeDamage(int damage)
    {
        // Skip damage if invulnerable
        if (isInvulnerable)
        {
            Debug.Log("Player dodged attack!");
            return;
        }

        // Apply damage and check if player died
        currentHealth -= damage;
        Debug.Log($"Player took {damage} damage. Health: {currentHealth}/{maxHealth}");

        // Update health bar
        UpdateHealthBar();
        
        if (currentHealth <= maxHealth * 0.25)
        {
            FindFirstObjectByType<OnlyOneBoss>().ProvideOnlyOneHint();

        }

        // Play hit sound
        if (audioSource != null && hitSound != null)
        {
            audioSource.PlayOneShot(hitSound);
        }

        // Visual feedback
        StartCoroutine(FlashSprite());

        // Check for death
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

    // Visual feedback for taking damage
    private IEnumerator FlashSprite()
    {
        if (spriteRenderer != null && flashMaterial != null)
        {
            // Switch to flash material
            spriteRenderer.material = flashMaterial;

            // Wait for flash duration
            yield return new WaitForSeconds(flashDuration);

            // Switch back to original material
            spriteRenderer.material = originalMaterial;
        }
    }

    private void Die()
    {
        Debug.Log("Player died!");

        // Play death animation
        if (animator != null)
        {
            animator.SetTrigger("Die");
        }

        // Disable input
        enabled = false;

        // Call GameManager to handle next generation
        FindFirstObjectByType<GameManager>().PlayerDied();
    }

    // Check if player can dodge currently
    public bool CanDodge()
    {
        return canDodge && !isDodging && !isDodgeMoving && !isReturning;
    }

    // Get current dodge count
    public int GetDodgeCount()
    {
        return dodgeCount;
    }

    // Reset dodge count (call after using counter attack)
    public void ResetDodgeCount()
    {
        dodgeCount = 0;
        UpdateDodgeCounterUI();
    }

    void UpdateDodgeCounterUI()
    {
        if (dodgeCounterText != null && dodgeCount > 0)
        {
            dodgeCounterText.text = "x" + dodgeCount;
        }
        else if (dodgeCounterText != null && dodgeCount <= 0)
        {
            dodgeCounterText.text = "";
        }
    }

    // For animation events
    public void OnAttackAnimationComplete()
    {
        // This gets called from animation event at the end of attack animation
    }

    // For animation events
    public void OnDeathAnimationComplete()
    {
        // This gets called from animation event at the end of death animation
    }
}
