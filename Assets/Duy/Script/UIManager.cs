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
    [SerializeField] private int traitsToShow = 3; // Configurable number of traits to show

    [Header("Game Over")]
    public GameObject gameOverPanel;
    public TextMeshProUGUI gameOverText;
    public Button restartButton;

    [Header("HUD")]
    public TextMeshProUGUI generationText;
    public Transform traitIconsContainer;
    public Image traitIconPrefab;
    public Image borderIconPrefab;


    [Header("Combat UI")]
    public Button attackButton;

    [Header("Debug")]
    [SerializeField] private bool showAllTraits = false; // Set to true to show all available traits

    private void Start()
    {
        if (restartButton != null)
            restartButton.onClick.AddListener(RestartGame);

        HideAllPanels();

        if (attackButton != null)
            attackButton.onClick.AddListener(OnAttackButtonClicked);

        // Subscribe to turn changes
        TurnManager.Instance.OnTurnChanged += UpdateButtonsForTurnState;
    }

    private void OnAttackButtonClicked()
    {
        PlayerController player = FindFirstObjectByType<PlayerController>();
        if (player != null)
        {
            player.Attack();

            // End player turn
            TurnManager.Instance.EndPlayerTurn();
        }
    }

    private void UpdateButtonsForTurnState(TurnState newState)
    {
        if (attackButton != null)
            attackButton.interactable = (newState == TurnState.PlayerTurn);
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

            LineageSystem lineageSystem = FindFirstObjectByType<LineageSystem>();
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

            // Get traits to display - either all or random selection
            List<Trait> traitsToDisplay = new List<Trait>();

            if (showAllTraits || availableTraits.Count <= traitsToShow)
            {
                // Show all available traits
                traitsToDisplay = new List<Trait>(availableTraits);
            }
            else
            {
                // Choose random traits to offer
                while (traitsToDisplay.Count < traitsToShow && availableTraits.Count > 0)
                {
                    int randomIndex = Random.Range(0, availableTraits.Count);
                    traitsToDisplay.Add(availableTraits[randomIndex]);
                    availableTraits.RemoveAt(randomIndex);
                }
            }

            // Create buttons for each trait
            foreach (Trait trait in traitsToDisplay)
            {
                Button traitButton = Instantiate(traitButtonPrefab, traitButtonContainer);
                TextMeshProUGUI buttonText = traitButton.GetComponentInChildren<TextMeshProUGUI>();
                if (buttonText != null)
                {
                    traitButton.GetComponent<Image>().sprite = trait.icon;
                    buttonText.text = trait.displayName;
                    buttonText.color = trait.displayColor;
                }

                // Store trait type in button
                TraitType traitType = trait.type;
                traitButton.onClick.AddListener(() => {
                    OnTraitSelected(traitType);
                });

                // Add tooltip with trait description
                TooltipTrigger tooltipTrigger = traitButton.gameObject.AddComponent<TooltipTrigger>();
                if (tooltipTrigger != null)
                {
                    tooltipTrigger.tooltipText = trait.description;
                }
            }

            // Update the HUD to show current generation
            UpdateHUD();
        }
    }

    private void OnTraitSelected(TraitType traitType)
    {
        // Hide the trait selection panel immediately
        if (traitSelectionPanel != null)
            traitSelectionPanel.SetActive(false);

        // Pass the selection to the GameManager
        GameManager.Instance.TraitSelected(traitType);

        // Update the HUD with new traits
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
        // Find GameInitializer first if possible for proper sequencing
        GameInitializer initializer = FindFirstObjectByType<GameInitializer>();
        if (initializer != null)
        {
            initializer.RestartGame();
        }
        else
        {
            // Fallback to direct restart
            GameManager.Instance.StartNewGame();
        }
    }

    public void UpdateHUD()
    {
        // Update generation text
        LineageSystem lineageSystem = FindFirstObjectByType<LineageSystem>();
        if (lineageSystem != null && generationText != null)
        {
            generationText.text = "Generation: " + lineageSystem.GetCurrentGeneration() +
                                  " / " + GameManager.Instance.GetMaxGenerations();
        }

        // Update trait icons
        if (traitIconsContainer != null && lineageSystem != null)
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
                    
                    Image border = Instantiate(borderIconPrefab, icon.transform);
                    border.color = trait.displayColor;

                    // If you have trait icons
                    if (trait.icon != null)
                        icon.sprite = trait.icon;

                    // Add tooltip with trait name and description
                    TooltipTrigger tooltipTrigger = border.gameObject.AddComponent<TooltipTrigger>();
                    if (tooltipTrigger != null)
                    {
                        tooltipTrigger.tooltipText = trait.displayName + ": " + trait.description;
                    }
                }
            }
        }
    }
}

// Simple tooltip trigger component - you'll need to implement a tooltip system
public class TooltipTrigger : MonoBehaviour
{
    public string tooltipText;

    // Add event system hooks if you want to implement tooltips later
}