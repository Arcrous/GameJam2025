using UnityEngine;

/// <summary>
/// Simple projectile script for boss projectile attacks
/// </summary>
public class Projectile : MonoBehaviour
{
    [SerializeField] private float speed = 7f;
    [SerializeField] private float lifetime = 5f;
    [SerializeField] private int damage = 10;
    [SerializeField] private GameObject impactEffect;
    
    private Transform target;
    private Vector3 moveDirection;
    
    void Start()
    {
        // Destroy after lifetime
        Destroy(gameObject, lifetime);
        
        // If no target is set, move forward
        if (target == null)
        {
            moveDirection = transform.right; // Assuming right is forward for the projectile
        }
    }
    
    void Update()
    {
        // If we have a target, constantly update direction to follow
        if (target != null)
        {
            moveDirection = (target.position - transform.position).normalized;
            
            // Optional: Rotate the projectile to face the direction
            float angle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }
        
        // Move the projectile
        transform.position += moveDirection * speed * Time.deltaTime;
    }
    
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        // Check if we hit the player
        PlayerController player = other.GetComponent<PlayerController>();
        if (player != null && !player.IsDodging())
        {
            // Apply damage
            player.TakeDamage(damage);
            
            // Show impact effect
            if (impactEffect != null)
            {
                Instantiate(impactEffect, transform.position, Quaternion.identity);
            }
            
            // Destroy the projectile
            Destroy(gameObject);
        }
        
        // If we hit something that's not the player (like a wall)
        if (player == null && !other.CompareTag("Boss") && !other.CompareTag("Projectile"))
        {
            // Show impact effect
            if (impactEffect != null)
            {
                Instantiate(impactEffect, transform.position, Quaternion.identity);
            }
            
            // Destroy the projectile
            Destroy(gameObject);
        }
    }
}