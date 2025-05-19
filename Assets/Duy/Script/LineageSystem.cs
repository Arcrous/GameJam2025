using System.Collections;
using UnityEngine;
using System.Collections.Generic;

public class LineageSystem : MonoBehaviour
{
    [SerializeField] private int maxGenerations = 7; // Max boss weaknesses + 1

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
    }

    public void AddTrait(TraitType traitType)
    {
        inheritedTraits.Add(traitType);

        // Notify any listeners (UI, player controller, etc.)
        if (OnGenerationChanged != null)
            OnGenerationChanged(currentGeneration, inheritedTraits);
    }

    public void AdvanceGeneration()
    {
        currentGeneration++;

        if (currentGeneration > maxGenerations)
        {
            // Game over - too many generations
            GameManager.Instance.GameOver(false);
            return;
        }

        // Let UI know we need to choose a new trait
        RequestTraitSelection();
    }

    private void RequestTraitSelection()
    {
        // This would typically call a UI manager to show trait selection screen
        FindObjectOfType<UIManager>().ShowTraitSelection();
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