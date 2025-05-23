using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(BossController))]
public class BossAnimationController : MonoBehaviour
{

    [Header("Animation Settings")]
    [SerializeField] private float attackAnimationDuration = 1f;
    [SerializeField] private float specialAttackAnimationDuration = 0.8f;
    [SerializeField] private float deathAnimationDuration = 1.5f;
    
    [Header("Attack Effects")]
    [SerializeField] private GameObject leftSwipeEffect;
    [SerializeField] private GameObject rightSwipeEffect;
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private GameObject specialAttackEffect;
    [SerializeField] private GameObject lightningStrikePrefab;
    [SerializeField] private float lightningLifetime = 0.6f; // match animation length
    [SerializeField] private GameObject warningFlashPrefab;
    [SerializeField] private GameObject warningFastFlashPrefab;
    [SerializeField] private Transform leftFlashPoint;
    [SerializeField] private Transform rightFlashPoint;
    [SerializeField] private float warningFlashDuration = .1f;

    [Header("Spawn Points")]
    [SerializeField] private Transform leftSwipeSpawnPoint;
    [SerializeField] private Transform rightSwipeSpawnPoint;
    [SerializeField] private Transform projectileSpawnPoint;
    
    [Header("Grid Setup")]
    [SerializeField] private Vector2 gridSize = new Vector2(3, 3); // 3x3 grid
    [SerializeField] private float gridSpacing = 1.5f;
    [SerializeField] private Transform gridCenter; // This should be set to the player area center
    [SerializeField] private bool visualizeGrid = true; // For debugging in editor

    [Header("Sound Effects")]
    [SerializeField] private AudioClip flashSound;
    [SerializeField] private AudioClip thunderSound;
    [SerializeField] private AudioClip swipeSound;

    private AudioSource audioSource;
    
    // Animation states - can be used as animator parameters
    private const string IDLE = "Idle";
    private const string ATTACK = "Attack";
    private const string SPECIAL_ATTACK = "SpecialAttack";
    private const string LEFT_SWIPE = "LeftSwipe";
    private const string RIGHT_SWIPE = "RightSwipe";
    private const string DEATH = "Die";
    private const string TRIPLE_LEFT_SWIPE = "TripleLeftSwipe";
    private const string TRIPLE_RIGHT_SWIPE = "TripleRightSwipe";
    
    private Animator animator;
    private BossController bossController;
    private bool isPlayingAnimation = false;
    private TurnManager turnManager;
    private PlayerController playerController;
    public enum AttackDirection
    {
        None,
        Left,
        Right,
        Both
    }
    private AttackType currentPattern;
    private AttackDirection currentAttackDirection = AttackDirection.None;

    void Awake()
    {
        animator = GetComponent<Animator>();
        bossController = GetComponent<BossController>();
	    audioSource = GetComponent<AudioSource>();

    	if (audioSource == null)
    	{	
        	audioSource = gameObject.AddComponent<AudioSource>();
        	audioSource.playOnAwake = false;
    	}
    }
    
    void Start()
    {
        // Find the player
        playerController = FindFirstObjectByType<PlayerController>();
        
        // Setup grid center
        SetupGridCenter();
        
        // Subscribe to boss events
        if (bossController != null)
        {
            // Play idle animation by default
            PlayAnimation(IDLE);
        }
    }
    
    /// <summary>
    /// Setup the grid center based on available references
    /// </summary>
    private void SetupGridCenter()
    {
        // If gridCenter is already set in the inspector, use that
        if (gridCenter != null)
            return;
            
        // Try to find a player area transform first
        Transform playerArea = GameObject.FindGameObjectWithTag("PlayerArea")?.transform;
        
        if (playerArea != null)
        {
            gridCenter = playerArea;
            Debug.Log("Grid center set to PlayerArea");
        }
        else if (playerController != null)
        {
            // If no player area is found, use player's position
            gridCenter = playerController.transform;
            Debug.Log("Grid center set to Player position");
        }
        else
        {
            // Last resort, create an object at a fixed position for the grid
            GameObject gridObj = new GameObject("PlayerBattleArea");
            gridObj.transform.position = new Vector3(0, -2, 0); // Adjust as needed for your game
            gridCenter = gridObj.transform;
            Debug.LogWarning("No player area found! Created a default grid center at " + gridObj.transform.position);
        }
    }
    
    /// <summary>
    /// Play the specified animation with parameters if needed
    /// </summary>
    public void PlayAnimation(string animationName, float crossFadeTime = 0.1f)
    {
        if (animator != null)
        {
            animator.CrossFade(animationName, crossFadeTime);
        }
    }
    
    [Header("Attack Pattern")]
    [SerializeField] private bool useRandomAttacks = true;
    [SerializeField] private AttackType[] availableAttacks;


    public enum AttackType
    {
        Projectile,
        LeftSwipe,
        RightSwipe,
        SpecialAttack,
	    TripleLeftSwipe,
	    TripleRightSwipe
    }
    
    /// <summary>
    /// Follow the predetermined attack pattern
    /// </summary>
    public void PlayNextAttackInPattern()
    {
        if (isPlayingAnimation || availableAttacks == null || availableAttacks.Length == 0)
            return;
            
        // Get the next attack in the pattern
        AttackType nextAttack = availableAttacks[Random.Range(0, availableAttacks.Length)];
	    SetCurrentAttackType(nextAttack);

        // Execute the attack
        switch (nextAttack)
        {
            case AttackType.Projectile:
                StartCoroutine(PlayProjectileAttack());
                break;
            case AttackType.LeftSwipe:
                StartCoroutine(PlayLeftSwipeAttack());
                break;
            case AttackType.RightSwipe:
                StartCoroutine(PlayRightSwipeAttack());
                break;
            case AttackType.SpecialAttack:
                StartCoroutine(PlaySpecialAttackAnimation());
                break;
	    case AttackType.TripleLeftSwipe:
                StartCoroutine(PlayTripleLeftSwipeAttack());
                break;
	    case AttackType.TripleRightSwipe:
                StartCoroutine(PlayTripleRightSwipeAttack());
                break;
        }
    }
    
    /// <summary>
    /// Generate grid cell positions for attacks
    /// </summary>
    private Vector3[,] GetGridPositions()
    {
        // Ensure grid center is set
        if (gridCenter == null)
        {
            SetupGridCenter();
        }
        
        Vector3[,] positions = new Vector3[(int)gridSize.x, (int)gridSize.y];
        
        float startX = gridCenter.position.x - (gridSize.x - 1) * gridSpacing / 2;
        float startY = gridCenter.position.y - (gridSize.y - 1) * gridSpacing / 2;
        
        for (int x = 0; x < gridSize.x; x++)
        {
            for (int y = 0; y < gridSize.y; y++)
            {
                positions[x, y] = new Vector3(
                    startX + x * gridSpacing,
                    startY + y * gridSpacing,
                    0
                );
            }
        }
        
        return positions;
    }

    private IEnumerator SpawnLightningSequence(bool isRightSide)
{
    int repetitions = 3;
    lightningLifetime = 0.3f;
    float delayBetweenStrikes = 1f; // Adjust delay as needed

    for (int i = 0; i < repetitions; i++)
    {
        SpawnLightningInQuadrant(isRightSide);
        yield return new WaitForSeconds(delayBetweenStrikes);
    }
}
    
    /// <summary>
    /// Left swipe attack - hits left 2 columns of 3x3 grid
    /// </summary>
    private IEnumerator PlayLeftSwipeAttack()
    {
        isPlayingAnimation = true;
        currentAttackDirection = AttackDirection.Right;

        // Play animation
        PlayAnimation(LEFT_SWIPE);
        yield return new WaitForSeconds(1f);
        SpawnLightningInQuadrant(false);

        // Wait for animation to reach the hit frame
        yield return new WaitForSeconds(attackAnimationDuration * 0.5f);
        
        // Get grid positions
        Vector3[,] gridPositions = GetGridPositions();
        
        // Hit the left 2 columns
        for (int x = 0; x < 2; x++)
        {
            for (int y = 0; y < gridSize.y; y++)
            {
                // Spawn effect at the attack point
                if (leftSwipeEffect != null)
                {
                    Instantiate(leftSwipeEffect, gridPositions[x, y], leftSwipeSpawnPoint.rotation);
                }
                
                // Check if player is in this grid cell and deal damage
                CheckPlayerInCell(gridPositions[x, y], gridSpacing / 2);
            }
        }
        
        // Wait for animation to complete
        yield return new WaitForSeconds(attackAnimationDuration * 0.5f);

        currentAttackDirection = AttackDirection.None;

        // Return to idle
        isPlayingAnimation = false;
    }
    
    /// <summary>
    /// Right swipe attack - hits right 2 columns of 3x3 grid
    /// </summary>
    private IEnumerator PlayRightSwipeAttack()
    {
        isPlayingAnimation = true;
        currentAttackDirection = AttackDirection.Left;

        // Play animation
        PlayAnimation(RIGHT_SWIPE);
        yield return new WaitForSeconds(1f);
        SpawnLightningInQuadrant(true);

        // Wait for animation to reach the hit frame
        yield return new WaitForSeconds(attackAnimationDuration * 0.5f);
        
        // Get grid positions
        Vector3[,] gridPositions = GetGridPositions();
        
        // Hit the right 2 columns
        for (int x = (int)gridSize.x - 2; x < gridSize.x; x++)
        {
            for (int y = 0; y < gridSize.y; y++)
            {
                // Spawn effect at the attack point
                if (rightSwipeEffect != null)
                {
                    Instantiate(rightSwipeEffect, gridPositions[x, y], rightSwipeSpawnPoint.rotation);
                }
                
                // Check if player is in this grid cell and deal damage
                CheckPlayerInCell(gridPositions[x, y], gridSpacing / 2);
            }
        }
        
        // Wait for animation to complete
        yield return new WaitForSeconds(attackAnimationDuration * 0.5f);

        currentAttackDirection = AttackDirection.None;

        // Return to idle
        isPlayingAnimation = false;
    }

    private IEnumerator PlayTripleLeftSwipeAttack()
    {
        isPlayingAnimation = true;
	currentAttackDirection = AttackDirection.Right;
        
        // Play animation
        PlayAnimation(TRIPLE_LEFT_SWIPE);
        yield return new WaitForSeconds(1f);
        StartCoroutine(SpawnLightningSequence(false));

        // Wait for animation to reach the hit frame
        yield return new WaitForSeconds(attackAnimationDuration * 0.5f);
        
        // Get grid positions
        Vector3[,] gridPositions = GetGridPositions();
        
        // Hit the left 2 columns
        for (int x = 0; x < 2; x++)
        {
            for (int y = 0; y < gridSize.y; y++)
            {
                // Spawn effect at the attack point
                if (leftSwipeEffect != null)
                {
                    Instantiate(leftSwipeEffect, gridPositions[x, y], leftSwipeSpawnPoint.rotation);
                }
                
                // Check if player is in this grid cell and deal damage
                CheckPlayerInCell(gridPositions[x, y], gridSpacing / 2);
            }
        }

        yield return new WaitForSeconds(1f);

        for (int x = 0; x < 2; x++)
        {
            for (int y = 0; y < gridSize.y; y++)
            {
                // Spawn effect at the attack point
                if (leftSwipeEffect != null)
                {
                    Instantiate(leftSwipeEffect, gridPositions[x, y], leftSwipeSpawnPoint.rotation);
                }

                // Check if player is in this grid cell and deal damage
                CheckPlayerInCell(gridPositions[x, y], gridSpacing / 2);
            }
        }

        yield return new WaitForSeconds(1f);

        for (int x = 0; x < 2; x++)
        {
            for (int y = 0; y < gridSize.y; y++)
            {
                // Spawn effect at the attack point
                if (leftSwipeEffect != null)
                {
                    Instantiate(leftSwipeEffect, gridPositions[x, y], leftSwipeSpawnPoint.rotation);
                }

                // Check if player is in this grid cell and deal damage
                CheckPlayerInCell(gridPositions[x, y], gridSpacing / 2);
            }
        }

        // Wait for animation to complete
        yield return new WaitForSeconds(attackAnimationDuration + 3.5f * 0.5f);
	    currentAttackDirection = AttackDirection.None;
        
        Debug.Log("Finished triple left swipe attack");

        // Return to idle
        isPlayingAnimation = false;
    }

    private IEnumerator PlayTripleRightSwipeAttack()
    {
        isPlayingAnimation = true;
	    currentAttackDirection = AttackDirection.Left;
        
        // Play animation
        PlayAnimation(TRIPLE_RIGHT_SWIPE);
        yield return new WaitForSeconds(1f);
        StartCoroutine(SpawnLightningSequence(true));

        // Wait for animation to reach the hit frame
        yield return new WaitForSeconds(attackAnimationDuration * 0.5f);
        
        // Get grid positions
        Vector3[,] gridPositions = GetGridPositions();

        // Hit the right 2 columns
        for (int x = (int)gridSize.x - 2; x < gridSize.x; x++)
        {
            for (int y = 0; y < gridSize.y; y++)
            {
                // Spawn effect at the attack point
                if (rightSwipeEffect != null)
                {
                    Instantiate(rightSwipeEffect, gridPositions[x, y], rightSwipeSpawnPoint.rotation);
                }

                // Check if player is in this grid cell and deal damage
                CheckPlayerInCell(gridPositions[x, y], gridSpacing / 2);
            }
        }

        yield return new WaitForSeconds(1f);

        // Hit the right 2 columns
        for (int x = (int)gridSize.x - 2; x < gridSize.x; x++)
        {
            for (int y = 0; y < gridSize.y; y++)
            {
                // Spawn effect at the attack point
                if (rightSwipeEffect != null)
                {
                    Instantiate(rightSwipeEffect, gridPositions[x, y], rightSwipeSpawnPoint.rotation);
                }

                // Check if player is in this grid cell and deal damage
                CheckPlayerInCell(gridPositions[x, y], gridSpacing / 2);
            }
        }

        yield return new WaitForSeconds(1f);

        // Hit the right 2 columns
        for (int x = (int)gridSize.x - 2; x < gridSize.x; x++)
        {
            for (int y = 0; y < gridSize.y; y++)
            {
                // Spawn effect at the attack point
                if (rightSwipeEffect != null)
                {
                    Instantiate(rightSwipeEffect, gridPositions[x, y], rightSwipeSpawnPoint.rotation);
                }

                // Check if player is in this grid cell and deal damage
                CheckPlayerInCell(gridPositions[x, y], gridSpacing / 2);
            }
        }

        // Wait for animation to complete
        yield return new WaitForSeconds(attackAnimationDuration + 3.5f);
        
        Debug.Log("Finished triple right swipe attack");
        // Return to idle
        isPlayingAnimation = false;
    }
    
    /// <summary>
    /// Projectile attack - shoots a projectile at the player
    /// </summary>
    private IEnumerator PlayProjectileAttack()
    {
        isPlayingAnimation = true;
        currentAttackDirection = AttackDirection.Both;

        // Play animation
        PlayAnimation(ATTACK);
        
        // Wait for animation to reach the projectile launch frame
        yield return new WaitForSeconds(attackAnimationDuration * 0.5f);
        
        // Spawn projectile at the spawn point
        if (projectilePrefab != null && projectileSpawnPoint != null)
        {
            GameObject projectile = Instantiate(projectilePrefab, projectileSpawnPoint.position, projectileSpawnPoint.rotation);
            
            // Target the player
            PlayerController player = FindFirstObjectByType<PlayerController>();
            if (player != null && projectile.GetComponent<Projectile>() != null)
            {
                projectile.GetComponent<Projectile>().SetTarget(player.transform);
            }
        }
        
        // Wait for animation to complete
        yield return new WaitForSeconds(attackAnimationDuration * 0.5f);

        currentAttackDirection = AttackDirection.None;

        // Return to idle
        PlayAnimation(IDLE);
        isPlayingAnimation = false;
    }
    
    /// <summary>
    /// Special attack - hits entire 3x3 grid
    /// </summary>
    public IEnumerator PlaySpecialAttackAnimation()
    {
        isPlayingAnimation = true;
        currentAttackDirection = AttackDirection.Both;

        // Play animation
        PlayAnimation(SPECIAL_ATTACK);
	    yield return new WaitForSeconds(1f);
        SpawnLightningInQuadrant(true);
	    SpawnLightningInQuadrant(false);
        
        // Wait for animation to reach the climax
        yield return new WaitForSeconds(specialAttackAnimationDuration * 0.6f);
        
        // Get grid positions
        Vector3[,] gridPositions = GetGridPositions();
        
        // Hit the entire grid
        for (int x = 0; x < gridSize.x; x++)
        {
            for (int y = 0; y < gridSize.y; y++)
            {
                // Spawn effect
                if (specialAttackEffect != null)
                {
                    Instantiate(specialAttackEffect, gridPositions[x, y], Quaternion.identity);
                }
                
                // Check if player is in this grid cell and deal damage
                CheckPlayerInCell(gridPositions[x, y], gridSpacing / 2);
            }
        }
        
        // Wait for animation to complete
        yield return new WaitForSeconds(specialAttackAnimationDuration * 0.4f);

        currentAttackDirection = AttackDirection.None;

        // Return to idle
        PlayAnimation(IDLE);
        isPlayingAnimation = false;
    }

    /// <summary>
    /// Check if the player is in a specific grid cell and deal damage if they are
    /// </summary>
    public void CheckPlayerInCell(Vector3 cellPosition, float cellRadius)
    {
        PlayerController player = FindFirstObjectByType<PlayerController>();
        if (player != null && bossController != null)
        {
            // Check if player is in this cell
            float distance = Vector3.Distance(player.transform.position, cellPosition);

            // Only deal damage if:
            // 1. Player is in the cell AND
            // 2. Player is not dodging OR is dodging but in the wrong direction
            if (distance <= cellRadius)
            {
                bool shouldTakeDamage = false;

                if (!player.IsDodging())
                {
                    // Player is not dodging at all
                    shouldTakeDamage = true;
                }
                else
                {
                    // Player is dodging, but check if it's in the wrong direction
                    bool isDodgingInWrongDirection = IsPlayerDodgingInWrongDirection();

                    if (isDodgingInWrongDirection)
                    {
                        shouldTakeDamage = true;
                    }
                }

                if (shouldTakeDamage)
                {
                    player.TakeDamage(bossController.attackPower);
                }
            }
        }
    }

    /// <summary>
    /// Determines if the player is dodging in the wrong direction relative to the attack
    /// </summary>
    private bool IsPlayerDodgingInWrongDirection()
    {
        BossController boss = GetComponent<BossController>();
        if (boss == null) return false;

        AttackDirection attackDir = currentAttackDirection;

        // Get dodge direction from player controller
        PlayerController player = FindFirstObjectByType<PlayerController>();
        if (player == null) return false;

        // We need to add a property to PlayerController to know the dodge direction
        bool isDodgingLeft = player.IsCurrentlyDodgingLeft();

        // Check if dodge direction is wrong for current attack
        if (attackDir == AttackDirection.Left && isDodgingLeft)
        {
            // Boss attacking from right, player dodging left = CORRECT
            return false;
        }
        else if (attackDir == AttackDirection.Right && !isDodgingLeft)
        {
            // Boss attacking from left, player dodging right = CORRECT
            return false;
        }
        else if (attackDir == AttackDirection.Both)
        {
            // For attacks that hit everywhere (like special attack), any dodge is wrong
            return false;
        }
        else if (attackDir == AttackDirection.None)
        {
            // If no attack direction, dodge is pointless
            return true;
        }

        // In all other cases, player is dodging in the wrong direction
        return true;
    }

    /// <summary>
    /// Death animation and cleanup
    /// </summary>
    public IEnumerator PlayDeathAnimation()
    {
        isPlayingAnimation = true;
        
        // Play animation
        PlayAnimation(DEATH);
        
        // Wait for animation to complete
        yield return new WaitForSeconds(deathAnimationDuration);
        
        // Keep the dead state (don't return to idle)
        isPlayingAnimation = false;
    }
    
    /// <summary>
    /// Check if an animation is currently playing
    /// </summary>
    public bool IsAnimationPlaying()
    {
        return isPlayingAnimation;
    }
    
    /// <summary>
    /// Animation event callback that can be triggered from animation frames
    /// </summary>
    public void AnimationEventDealDamage()
    {
        // This method can be called from animation events to deal damage at specific frames
        PlayerController player = FindFirstObjectByType<PlayerController>();
        if (player != null && bossController != null && !player.IsDodging())
        {
            player.TakeDamage(bossController.attackPower);
        }
    }
    
    /// <summary>
    /// Animation event callback for special attack damage
    /// </summary>
    public void AnimationEventSpecialAttackDamage()
    {
        // Special attack now handled by grid system in PlaySpecialAttackAnimation
    }

    public AttackDirection GetCurrentAttackDirection()
    {
        return currentAttackDirection;
    }

    /// <summary>
    /// Visualize the grid in the editor (for debugging)
    /// </summary>
    void OnDrawGizmosSelected()
    {
        if (!visualizeGrid)
            return;
            
        // Setup temporary grid center for editor visualization
        Transform tempGridCenter = gridCenter;
        
        if (tempGridCenter == null)
        {
            // Try to find player area or player for editor visualization
            tempGridCenter = GameObject.FindGameObjectWithTag("PlayerArea")?.transform;
            
            if (tempGridCenter == null)
                tempGridCenter = FindFirstObjectByType<PlayerController>()?.transform;
                
            if (tempGridCenter == null)
                tempGridCenter = transform; // Fallback to self
        }
            
        Vector3[,] positions = new Vector3[(int)gridSize.x, (int)gridSize.y];
        
        float startX = tempGridCenter.position.x - (gridSize.x - 1) * gridSpacing / 2;
        float startY = tempGridCenter.position.y - (gridSize.y - 1) * gridSpacing / 2;
        
        for (int x = 0; x < gridSize.x; x++)
        {
            for (int y = 0; y < gridSize.y; y++)
            {
                positions[x, y] = new Vector3(
                    startX + x * gridSpacing,
                    startY + y * gridSpacing,
                    0
                );
                
                // Draw different colors for different attack zones
                if (x < 2) // Left swipe zone
                    Gizmos.color = new Color(1, 0, 0, 0.3f); // Red
                else if (x >= gridSize.x - 2) // Right swipe zone
                    Gizmos.color = new Color(0, 0, 1, 0.3f); // Blue
                else // Middle column
                    Gizmos.color = new Color(0, 1, 0, 0.3f); // Green
                
                Gizmos.DrawSphere(positions[x, y], 0.1f);
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(positions[x, y], gridSpacing / 2);
            }
        }
    }

    private void SpawnLightningInQuadrant(bool isRightSide)
    {
        Camera mainCam = Camera.main;
        if (mainCam == null) return;

        float screenWidth = Screen.width;
        float screenHeight = Screen.height;

        // Define bounds of the quadrant in screen space
        float minX = isRightSide ? screenWidth / 2f : 0f;
        float maxX = isRightSide ? screenWidth : screenWidth / 2f;
        float minY = 0f;
        float maxY = screenHeight / 2f;

        int columns = 5;
        int rows = 3;

        for (int x = 0; x < columns; x++)
        {
            for (int y = 0; y < rows; y++)
            {
                float screenX = Mathf.Lerp(minX, maxX, (x + 0.5f) / columns);
                float screenY = Mathf.Lerp(minY, maxY, (y + 0.5f) / rows);
                Vector3 screenPos = new Vector3(screenX, screenY, 10f); // z = 10 for camera depth

                Vector3 worldPos = mainCam.ScreenToWorldPoint(screenPos);

                GameObject lightning = Instantiate(lightningStrikePrefab, worldPos, Quaternion.identity);
		        if (thunderSound != null) audioSource.PlayOneShot(thunderSound, .3f);
                Destroy(lightning, lightningLifetime);
            }
        }
    }

   private void SpawnLeftFlash()
	{
if (warningFlashPrefab != null && leftFlashPoint != null)
    {
        GameObject flash = Instantiate(warningFlashPrefab, leftFlashPoint.position, Quaternion.identity);
	if (flashSound != null) audioSource.PlayOneShot(flashSound, 1f);
        Destroy(flash, warningFlashDuration);
    }
}

private void SpawnRightFlash()
	{
if (warningFlashPrefab != null && rightFlashPoint != null)
    {
	if (flashSound != null) audioSource.PlayOneShot(flashSound, .7f);
        GameObject flash = Instantiate(warningFlashPrefab, rightFlashPoint.position, Quaternion.identity);

        Destroy(flash, warningFlashDuration);
    }
}

    private void PlaySwipeSound()
    {
	if (swipeSound != null) audioSource.PlayOneShot(swipeSound, 1f);
    }

private void SetCurrentAttackType(AttackType attack)
{
	currentPattern = attack;
}

public AttackType GetCurrentAttackType()
{
	return currentPattern;
}
}


    