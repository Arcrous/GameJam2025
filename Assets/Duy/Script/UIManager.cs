using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class UIManager : MonoBehaviour
{
    [Header("Trait Selection")]
    public GameObject traitSelectionPanel;
    public Transform traitButtonContainer;
    public Button traitButtonPrefab;

    [Header("Game Over")]
    public GameObject gameOverPanel;
    public TextMeshProUGUI gameOverText;
    public Button restartButton;

    [Header("HUD")]
    public TextMeshProUGUI generationText;
    public Transform traitIconsContainer;
    public Image traitIconPrefab;

    private void Start()
    {
        if (restartButton != null)
            restartButton.onClick.AddListener(RestartGame);

        HideAllPanels();
    }

    private void HideAllPanels()
    {
        if (traitSelectionPanel != null)
            traitSelectionPanel.SetActive(false);

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
    }

    public void ShowTraitSelection()
    {
        HideAllPanels();

        if (traitSelectionPanel != null)
        {
            traitSelectionPanel.SetActive(true);

            // Clear previous buttons
            foreach (Transform child in traitButtonContainer)
            {
                Destroy(child.gameObject);
            }

            // Get traits that haven't been selected yet
            List<Trait> availableTraits = new List<Trait>();
            List<TraitType> currentTraits = new List<TraitType>();

            LineageSystem lineageSystem = FindObjectOfType<LineageSystem>();
            if (lineageSystem != null)
            {
                currentTraits = lineageSystem.GetInheritedTraits();
            }

            foreach (TraitType traitType in System.Enum.GetValues(typeof(TraitType)))
            {
                if (!currentTraits.Contains(traitType))
                {
                    Trait trait = TraitManager.Instance.GetTraitByType(traitType);
                    if (trait != null)
                        availableTraits.Add(trait);
                }
            }

            // Choose 3 random traits to offer
            List<Trait> offeredTraits = new List<Trait>();
            while (offeredTraits.Count < 3 && availableTraits.Count > 0)
            {
                int randomIndex = Random.Range(0, availableTraits.Count);
                offeredTraits.Add(availableTraits[randomIndex]);
                availableTraits.RemoveAt(randomIndex);
            }

            // Create buttons for each trait
            foreach (Trait trait in offeredTraits)
            {
                Button traitButton = Instantiate(traitButtonPrefab, traitButtonContainer);
                TextMeshProUGUI buttonText = traitButton.GetComponentInChildren<TextMeshProUGUI>();
                if (buttonText != null)
                {
                    buttonText.text = trait.displayName;
                    buttonText.color = trait.displayColor;
                }

                // Store trait type in button
                TraitType traitType = trait.type;
                traitButton.onClick.AddListener(() => {
                    OnTraitSelected(traitType);
                });

                // Add tooltip/description if needed
            }
        }
    }

    private void OnTraitSelected(TraitType traitType)
    {
        GameManager.Instance.TraitSelected(traitType);
        UpdateHUD();
    }

    public void ShowGameOver(bool victory)
    {
        HideAllPanels();

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);

            if (gameOverText != null)
            {
                if (victory)
                {
                    gameOverText.text = "Victory!\nYou defeated the boss!";
                }
                else
                {
                    gameOverText.text = "Game Over\nToo many generations have fallen.";
                }
            }
        }
    }

    private void RestartGame()
    {
        // Call GameManager to restart
        GameManager.Instance.StartNewGame();
    }

    public void UpdateHUD()
    {
        // Update generation text
        LineageSystem lineageSystem = FindObjectOfType<LineageSystem>();
        if (lineageSystem != null && generationText != null)
        {
            generationText.text = "Generation: " + lineageSystem.GetCurrentGeneration();
        }

        // Update trait icons
        if (traitIconsContainer != null)
        {
            // Clear existing icons
            foreach (Transform child in traitIconsContainer)
            {
                Destroy(child.gameObject);
            }

            // Add icon for each trait
            List<TraitType> traits = lineageSystem.GetInheritedTraits();
            foreach (TraitType traitType in traits)
            {
                Trait trait = TraitManager.Instance.GetTraitByType(traitType);
                if (trait != null)
                {
                    Image icon = Instantiate(traitIconPrefab, traitIconsContainer);
                    icon.color = trait.displayColor;

                    // If you have trait icons
                    if (trait.icon != null)
                        icon.sprite = trait.icon;
                }
            }
        }
    }
}