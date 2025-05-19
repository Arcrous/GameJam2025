using System.Collections;
using UnityEngine;
using System.Collections.Generic;

public class PlayerTraitSystem : MonoBehaviour
{
    private List<TraitType> playerTraits = new List<TraitType>();
    private PlayerController playerController;

    [Header("Trait Visual Effects")]
    public ParticleSystem traitParticleSystem;
    public SpriteRenderer traitAura;

    void Start()
    {
        playerController = GetComponent<PlayerController>();
    }

    public void SetTraits(List<TraitType> traits)
    {
        playerTraits = new List<TraitType>(traits);
        ApplyTraitEffects();
        UpdateVisualEffects();
    }

    private void ApplyTraitEffects()
    {
        if (playerController == null) return;

        // Reset to base stats
        playerController.maxHealth = 100;
        playerController.attackPower = 10;

        // Apply trait bonuses
        foreach (TraitType traitType in playerTraits)
        {
            Trait trait = TraitManager.Instance.GetTraitByType(traitType);
            if (trait != null)
            {
                // Apply trait effects
                playerController.attackPower = Mathf.RoundToInt(playerController.attackPower * trait.damageMultiplier);

                // Specific trait bonuses
                switch (traitType)
                {
                    case TraitType.Earth:
                        playerController.maxHealth += 25; // Earth increases health
                        break;
                    case TraitType.Steel:
                        playerController.attackPower += 5; // Steel adds flat damage
                        break;
                        // Add other specific trait effects as needed
                }
            }
        }

        // Update current health to max
        playerController.currentHealth = playerController.maxHealth;
    }

    private void UpdateVisualEffects()
    {
        // Update visual effects based on traits
        // This would change particle colors, aura effects, etc.
        if (playerTraits.Count > 0)
        {
            // For simplicity, just use the last acquired trait for visual effects
            TraitType primaryTrait = playerTraits[playerTraits.Count - 1];
            Trait trait = TraitManager.Instance.GetTraitByType(primaryTrait);

            if (trait != null && traitAura != null)
            {
                traitAura.color = trait.displayColor;
                traitAura.enabled = true;
            }

            if (traitParticleSystem != null)
            {
                var main = traitParticleSystem.main;
                main.startColor = trait.displayColor;
                traitParticleSystem.Play();
            }
        }
    }

    public List<TraitType> GetPlayerTraits()
    {
        return new List<TraitType>(playerTraits);
    }

    public bool HasTrait(TraitType traitType)
    {
        return playerTraits.Contains(traitType);
    }

    public float CalculateDamageAgainstBoss(List<TraitType> bossWeaknesses)
    {
        float damageMultiplier = 1.0f;

        // Check if any player traits match boss weaknesses
        foreach (TraitType playerTrait in playerTraits)
        {
            if (bossWeaknesses.Contains(playerTrait))
            {
                damageMultiplier += 0.5f; // 50% extra damage per weakness match
            }
        }

        return playerController.attackPower * damageMultiplier;
    }
}