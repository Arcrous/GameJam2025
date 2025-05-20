using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Stats")]
    public int currentHealth;
    public int maxHealth = 100;
    public int attackPower = 10;
    
    [Header("Combat")]
    public float dodgeCooldown = 0.5f;
    private bool canDodge = true;
    private bool isDodging = false;
    
    [Header("References")]
    public GameObject slashEffect;
    public Transform attackPoint;
    
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    
    void Start()
    {
        currentHealth = maxHealth;
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }
    
    public void Attack()
    {
        Debug.Log("Player attacked!");
        // Play attack animation or visual effect
        if (animator != null)
            animator.SetTrigger("Attack");

        // Instantiate slash effect if available
        if (slashEffect != null && attackPoint != null)
            Instantiate(slashEffect, attackPoint.position, attackPoint.rotation);
    }
    
    public void TryDodge()
    {
        if (canDodge && !isDodging)
        {
            StartCoroutine(DodgeCoroutine());
        }
    }
    
    private System.Collections.IEnumerator DodgeCoroutine()
    {
        isDodging = true;
        canDodge = false;
        
        // Move sprite down for dodge (simple implementation)
        Vector3 originalPosition = transform.position;
        transform.position = new Vector3(transform.position.x, transform.position.y - 0.5f, transform.position.z);
        
        // Show dodge/slash effect
        if (slashEffect != null && attackPoint != null)
            Instantiate(slashEffect, attackPoint.position, attackPoint.rotation);
        
        yield return new WaitForSeconds(0.2f);
        
        // Return to original position
        transform.position = originalPosition;
        isDodging = false;
        
        yield return new WaitForSeconds(dodgeCooldown);
        canDodge = true;
    }
    
    public bool IsDodging()
    {
        return isDodging;
    }
    
    public void TakeDamage(int damage)
    {
        if (!isDodging) // Only take damage if not dodging
        {
            currentHealth -= damage;
            if (currentHealth <= 0)
            {
                Die();
            }
        }
    }
    
    private void Die()
    {
        Debug.Log("Player died!");
        // Call GameManager to handle next generation
        FindFirstObjectByType<GameManager>().PlayerDied();
    }
}
