using System.Collections;
using UnityEngine;
using System.Collections.Generic;

public class LineageSystem : MonoBehaviour
{
    public int maxGenerations = 7; // Max boss weaknesses + 1

    private int currentGeneration = 1;
    private List<TraitType> inheritedTraits = new List<TraitType>();

    public delegate void GenerationChangeDelegate(int newGeneration, List<TraitType> traits);
    public event GenerationChangeDelegate OnGenerationChanged;

    public void StartNewGame()
    {
        currentGeneration = 1;
        inheritedTraits.Clear();

        // Let UI know we need to choose a trait
        RequestTraitSelection();

        // Notify any listeners of generation change
        if (OnGenerationChanged != null)
            OnGenerationChanged(currentGeneration, inheritedTraits);
    }

    public void AddTrait(TraitType traitType)
    {
        // Check if we already have this trait
        if (inheritedTraits.Contains(traitType))
        {
            Debug.LogWarning("Attempting to add duplicate trait: " + traitType);
            return;
        }

        inheritedTraits.Add(traitType);
        Debug.Log("Trait added to generation " + currentGeneration + ": " + traitType);

        // Notify any listeners (UI, player controller, etc.)
        if (OnGenerationChanged != null)
            OnGenerationChanged(currentGeneration, inheritedTraits);
    }

    public void AdvanceGeneration()
    {
        currentGeneration++;
        Debug.Log("Advanced to generation " + currentGeneration);

        if (currentGeneration > maxGenerations)
        {
            // Game over - too many generations
            Debug.Log("Game Over - Max generations reached: " + currentGeneration);
            GameManager.Instance.GameOver(false);
            return;
        }

        // Let UI know we need to choose a new trait
        RequestTraitSelection();

        // Notify any listeners of generation change
        if (OnGenerationChanged != null)
            OnGenerationChanged(currentGeneration, inheritedTraits);
    }

    private void RequestTraitSelection()
    {
        // This would typically call a UI manager to show trait selection screen
        UIManager uiManager = FindFirstObjectByType<UIManager>();
        if (uiManager != null)
        {
            uiManager.ShowTraitSelection();
        }
        else
        {
            Debug.LogError("UIManager not found when requesting trait selection");
        }
    }

    public int GetCurrentGeneration()
    {
        return currentGeneration;
    }

    public List<TraitType> GetInheritedTraits()
    {
        return new List<TraitType>(inheritedTraits);
    }

    public bool IsGameOver()
    {
        return currentGeneration > maxGenerations;
    }
}