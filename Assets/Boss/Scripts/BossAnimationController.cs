using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(BossController))]
public class BossAnimationController : MonoBehaviour
{
    [Header("Animation Settings")]
    [SerializeField] private float attackAnimationDuration = 0.5f;
    [SerializeField] private float specialAttackAnimationDuration = 0.8f;
    [SerializeField] private float deathAnimationDuration = 1.5f;
    
    [Header("Attack Effects")]
    [SerializeField] private GameObject leftSwipeEffect;
    [SerializeField] private GameObject rightSwipeEffect;
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private GameObject specialAttackEffect;
    
    [Header("Spawn Points")]
    [SerializeField] private Transform leftSwipeSpawnPoint;
    [SerializeField] private Transform rightSwipeSpawnPoint;
    [SerializeField] private Transform projectileSpawnPoint;
    
    // Animation states - can be used as animator parameters
    private const string IDLE = "Idle";
    private const string ATTACK = "Attack";
    private const string SPECIAL_ATTACK = "SpecialAttack";
    private const string LEFT_SWIPE = "LeftSwipe";
    private const string RIGHT_SWIPE = "RightSwipe";
    private const string DEATH = "Die";
    
    private Animator animator;
    private BossController bossController;
    private bool isPlayingAnimation = false;
    
    void Awake()
    {
        animator = GetComponent<Animator>();
        bossController = GetComponent<BossController>();
    }
    
    void Start()
    {
        // Subscribe to boss events
        if (bossController != null)
        {
            // Play idle animation by default
            PlayAnimation(IDLE);
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
    
    /// <summary>
    /// Handle standard attack - chooses randomly between left swipe, right swipe or projectile
    /// </summary>
    public void PlayRandomAttackAnimation()
    {
        if (isPlayingAnimation)
            return;
            
        // Pick a random attack type
        int attackType = Random.Range(0, 3);
        
        switch (attackType)
        {
            case 0:
                StartCoroutine(PlayLeftSwipeAttack());
                break;
            case 1:
                StartCoroutine(PlayRightSwipeAttack());
                break;
            case 2:
                StartCoroutine(PlayProjectileAttack());
                break;
        }
    }
    
    /// <summary>
    /// Left swipe attack animation and effect
    /// </summary>
    private IEnumerator PlayLeftSwipeAttack()
    {
        isPlayingAnimation = true;
        
        // Play animation
        PlayAnimation(LEFT_SWIPE);
        
        // Wait for animation to reach the hit frame
        yield return new WaitForSeconds(attackAnimationDuration * 0.5f);
        
        // Spawn effect at the attack point
        if (leftSwipeEffect != null && leftSwipeSpawnPoint != null)
        {
            Instantiate(leftSwipeEffect, leftSwipeSpawnPoint.position, leftSwipeSpawnPoint.rotation);
        }
        
        // Wait for animation to complete
        yield return new WaitForSeconds(attackAnimationDuration * 0.5f);
        
        // Return to idle
        PlayAnimation(IDLE);
        isPlayingAnimation = false;
    }
    
    /// <summary>
    /// Right swipe attack animation and effect  
    /// </summary>
    private IEnumerator PlayRightSwipeAttack()
    {
        isPlayingAnimation = true;
        
        // Play animation
        PlayAnimation(RIGHT_SWIPE);
        
        // Wait for animation to reach the hit frame
        yield return new WaitForSeconds(attackAnimationDuration * 0.5f);
        
        // Spawn effect at the attack point
        if (rightSwipeEffect != null && rightSwipeSpawnPoint != null)
        {
            Instantiate(rightSwipeEffect, rightSwipeSpawnPoint.position, rightSwipeSpawnPoint.rotation);
        }
        
        // Wait for animation to complete
        yield return new WaitForSeconds(attackAnimationDuration * 0.5f);
        
        // Return to idle
        PlayAnimation(IDLE);
        isPlayingAnimation = false;
    }
    
    /// <summary>
    /// Projectile attack animation and spawning
    /// </summary>
    private IEnumerator PlayProjectileAttack()
    {
        isPlayingAnimation = true;
        
        // Play animation
        PlayAnimation(ATTACK);
        
        // Wait for animation to reach the projectile launch frame
        yield return new WaitForSeconds(attackAnimationDuration * 0.5f);
        
        // Spawn projectile at the spawn point
        if (projectilePrefab != null && projectileSpawnPoint != null)
        {
            GameObject projectile = Instantiate(projectilePrefab, projectileSpawnPoint.position, projectileSpawnPoint.rotation);
            
            // Target the player
            PlayerController player = FindObjectOfType<PlayerController>();
            if (player != null && projectile.GetComponent<Projectile>() != null)
            {
                projectile.GetComponent<Projectile>().SetTarget(player.transform);
            }
        }
        
        // Wait for animation to complete
        yield return new WaitForSeconds(attackAnimationDuration * 0.5f);
        
        // Return to idle
        PlayAnimation(IDLE);
        isPlayingAnimation = false;
    }
    
    /// <summary>
    /// Special attack animation and effect
    /// </summary>
    public IEnumerator PlaySpecialAttackAnimation()
    {
        isPlayingAnimation = true;
        
        // Play animation
        PlayAnimation(SPECIAL_ATTACK);
        
        // Wait for animation to reach the climax
        yield return new WaitForSeconds(specialAttackAnimationDuration * 0.6f);
        
        // Spawn effect
        if (specialAttackEffect != null)
        {
            Instantiate(specialAttackEffect, transform.position, Quaternion.identity);
        }
        
        // Wait for animation to complete
        yield return new WaitForSeconds(specialAttackAnimationDuration * 0.4f);
        
        // Return to idle
        PlayAnimation(IDLE);
        isPlayingAnimation = false;
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
        PlayerController player = FindObjectOfType<PlayerController>();
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
        // This method can be called from animation events to deal special attack damage
        PlayerController player = FindObjectOfType<PlayerController>();
        if (player != null && bossController != null && !player.IsDodging())
        {
            player.TakeDamage(bossController.attackPower * 2);
        }
    }
}