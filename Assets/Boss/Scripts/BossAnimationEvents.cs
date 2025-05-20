using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Helper component to connect animation events to the boss animation controller.
/// Add this to the same GameObject that has the Animator component.
/// </summary>
public class BossAnimationEvents : MonoBehaviour
{
    private BossAnimationController animationController;
    private BossController bossController;
    
    void Awake()
    {
        animationController = GetComponent<BossAnimationController>();
        bossController = GetComponent<BossController>();
    }
    
    // Called from animation frames
    public void OnAttackFrame()
    {
        if (animationController != null)
        {
            animationController.AnimationEventDealDamage();
        }
    }
    
    // Called from special attack animation
    public void OnSpecialAttackFrame()
    {
        if (animationController != null)
        {
            animationController.AnimationEventSpecialAttackDamage();
        }
    }
    
    // Called when death animation completes
    public void OnDeathAnimationComplete()
    {
        // This could be used to trigger any post-death effects
        Debug.Log("Boss death animation completed");
    }
    
    // Called when attack animation starts
    public void OnAttackStart()
    {
        // Can be used to trigger sound effects or other effects
    }
    
    // Called when attack animation ends
    public void OnAttackEnd()
    {
        // Can be used to clean up after an attack
    }
}