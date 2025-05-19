using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnlyOneBoss : MonoBehaviour
{
    [Header("Only One Theme Implementation")]
    [SerializeField] private TraitType onlyOneTrait; // The ONE trait that can deal critical damage
    [SerializeField] private float onlyOneMultiplier = 5.0f; // Massive multiplier for the ONE trait
    [SerializeField] private GameObject onlyOneVFX; // Special effect for the ONE trait
    
    private BossController bossController;
    private bool onlyOneRevealed = false;
    
    void Start()
    {
        bossController = GetComponent<BossController>();
        
        // Choose the "Only One" trait randomly
        DetermineOnlyOneTrait();
    }
    
    private void DetermineOnlyOneTrait()
    {
        // Choose one random trait from all the traits
        List<TraitType> allTraits = new List<TraitType>();
        foreach (TraitType traitType in System.Enum.GetValues(typeof(TraitType)))
        {
            allTraits.Add(traitType);
        }
        
        // Pick one as the special "Only One" trait that can deal critical damage
        if (allTraits.Count > 0)
        {
            onlyOneTrait = allTraits[Random.Range(0, allTraits.Count)];
            Debug.Log("The ONLY ONE trait that can deal critical damage is: " + onlyOneTrait);
        }
    }
    
    // Called by BossController when taking damage
    public float GetBonusDamageMultiplier(List<TraitType> playerTraits)
    {
        // If player has the "Only One" trait, they deal massive damage
        if (playerTraits.Contains(onlyOneTrait))
        {
            // Reveal the special trait if it's the first time
            if (!onlyOneRevealed)
            {
                RevealOnlyOneTrait();
            }
            
            // Play special effect
            if (onlyOneVFX != null)
            {
                Instantiate(onlyOneVFX, transform.position, Quaternion.identity);
            }
            
            // Return massive multiplier
            return onlyOneMultiplier;
        }
        
        return 1.0f; // No bonus for other traits
    }
    
    private void RevealOnlyOneTrait()
    {
        onlyOneRevealed = true;
        
        // Display special message
        UIManager uiManager = FindObjectOfType<UIManager>();
        if (uiManager != null)
        {
            // If you add a method in UIManager to show a special message
            // uiManager.ShowOnlyOneMessage(onlyOneTrait);
            Debug.Log("REVEALED: " + onlyOneTrait + " is the ONLY ONE trait that can deal critical damage!");
        }
    }
    
    // Called from UI to provide a hint about the special trait
    public void ProvideOnlyOneHint()
    {
        // This could be triggered by a button that costs player something
        // Show a cryptic message about the "Only One" trait
        Trait trait = TraitManager.Instance.GetTraitByType(onlyOneTrait);
        if (trait != null)
        {
            Debug.Log("The boss seems to react strongly to the color: " + trait.displayColor);
            // Display hint in UI
        }
    }
}