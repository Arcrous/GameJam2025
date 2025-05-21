using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class OnlyOneBoss : MonoBehaviour
{
    [Header("Only One Theme Implementation")]
    [SerializeField] private TraitType onlyOneTrait; // The ONE trait that can deal critical damage
    [SerializeField] private float onlyOneMultiplier = 5.0f; // Massive multiplier for the ONE trait
    [SerializeField] private GameObject onlyOneVFX; // Special effect for the ONE trait
    public bool hinted = false;
    
    [Header("UI")]
    [SerializeField] private GameObject onlyOneRevealPanel;
    [SerializeField] private TextMeshProUGUI onlyOneRevealText;
    [SerializeField] private float revealDuration = 3f;
    
    private BossController bossController;
    private bool onlyOneRevealed = false;
    
    void Awake()
    {
        bossController = GetComponent<BossController>();
        
        // Choose the "Only One" trait randomly
        DetermineOnlyOneTrait();
        
        // Hide the reveal panel if it exists
        if (onlyOneRevealPanel != null)
            onlyOneRevealPanel.SetActive(false);
    }
    
    private void DetermineOnlyOneTrait()
    {
        // Get all possible traits
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
        else
        {
            Debug.LogError("No traits available in TraitType enum!");
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
            
            // Apply massive multiplier
            return onlyOneMultiplier;
        }
        
        return 1.0f; // No bonus for other traits
    }
    
    private void RevealOnlyOneTrait()
    {
        onlyOneRevealed = true;
        
        // Get trait info
        Trait trait = TraitManager.Instance.GetTraitByType(onlyOneTrait);
        
        // Display special message in UI
        if (onlyOneRevealPanel != null && onlyOneRevealText != null && trait != null)
        {
            onlyOneRevealText.text = "THAT'S THE ONE!\n" + 
                                     trait.displayName + " is the ONE fatal weakness of this enemy!";
            
            // Color the text to match the trait
            onlyOneRevealText.color = trait.displayColor;
            
            // Show the panel
            onlyOneRevealPanel.SetActive(true);
            
            // Hide it after a few seconds
            StartCoroutine(HideRevealPanel());
        }
        
        Debug.Log("REVEALED: " + onlyOneTrait + " is the ONLY ONE trait that can deal critical damage!");
    }
    
    private IEnumerator HideRevealPanel()
    {
        yield return new WaitForSeconds(revealDuration);
        
        if (onlyOneRevealPanel != null)
            onlyOneRevealPanel.SetActive(false);
    }
    
    // Called from UI to provide a hint about the special trait
    public void ProvideOnlyOneHint()
    {
        if(hinted) return; // Prevent multiple hints
        // This could be triggered by a button that costs player something
        Trait trait = TraitManager.Instance.GetTraitByType(onlyOneTrait);
        if (trait != null)
        {
            // Show hint UI
            UIManager uiManager = FindFirstObjectByType<UIManager>();
            if (uiManager != null && onlyOneRevealText != null)
            {
                string hintText = "The boss seems to react strongly to the color: " + ColorToString(trait.displayColor);

                // Could add this to a hints panel in UIManager
                Debug.Log("HINT: " + hintText);
            }
            hinted = true; // Mark as hinted
        }
    }
    
    // Helper method to describe color in text
    private string ColorToString(Color color)
    {
        if (color.r > 0.7f && color.g < 0.5f && color.b < 0.5f) return "red";
        if (color.r < 0.5f && color.g > 0.7f && color.b < 0.5f) return "green";
        if (color.r < 0.5f && color.g < 0.5f && color.b > 0.7f) return "blue";
        if (color.r > 0.7f && color.g > 0.7f && color.b < 0.5f) return "yellow";
        if (color.r > 0.7f && color.g < 0.5f && color.b > 0.7f) return "purple";
        if (color.r < 0.5f && color.g > 0.7f && color.b > 0.7f) return "cyan";
        if (color.r > 0.7f && color.g > 0.5f && color.b > 0.5f) return "pink";
        if (color.r < 0.3f && color.g < 0.3f && color.b < 0.3f) return "black";
        if (color.r > 0.7f && color.g > 0.7f && color.b > 0.7f) return "white";
        
        return "mysterious";
    }
    
    // Public getter for the OnlyOne trait (useful for debugging or special UI)
    public TraitType GetOnlyOneTrait()
    {
        return onlyOneTrait;
    }
}