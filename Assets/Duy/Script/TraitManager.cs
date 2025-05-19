using System.Collections;
using UnityEngine;
using System.Collections.Generic;

public enum TraitType
{
    Fire,
    Ice,
    Lightning,
    Earth,
    Wind,
    Water,
    Light,
    Shadow,
    Poison,
    Steel
}

[System.Serializable]
public class Trait
{
    public TraitType type;
    public string displayName;
    public string description;
    public Color displayColor;
    public Sprite icon;

    // Trait effects
    public float damageMultiplier = 1.0f;
    public float dodgeTimeBonus = 0.0f;
    public bool causesDoT = false;
    public bool canStun = false;
    public float criticalChance = 0.0f;

    // For special effects
    public GameObject specialEffectPrefab;
}

public class TraitManager : MonoBehaviour
{
    public static TraitManager Instance;

    [SerializeField]
    private List<Trait> allTraits = new List<Trait>();

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        DontDestroyOnLoad(gameObject);

        InitializeTraits();
    }

    private void InitializeTraits()
    {
        // Populate with default values if not set in inspector
        if (allTraits.Count == 0)
        {
            // Fire trait
            Trait fireTrait = new Trait();
            fireTrait.type = TraitType.Fire;
            fireTrait.displayName = "Fire";
            fireTrait.description = "Burns enemies over time";
            fireTrait.displayColor = new Color(1f, 0.4f, 0.1f);
            fireTrait.causesDoT = true;
            allTraits.Add(fireTrait);

            // Ice trait
            Trait iceTrait = new Trait();
            iceTrait.type = TraitType.Ice;
            iceTrait.displayName = "Ice";
            iceTrait.description = "Slows enemy attacks";
            iceTrait.displayColor = new Color(0.7f, 0.9f, 1f);
            allTraits.Add(iceTrait);

            // Lightning trait
            Trait lightningTrait = new Trait();
            lightningTrait.type = TraitType.Lightning;
            lightningTrait.displayName = "Lightning";
            lightningTrait.description = "Has a chance to stun";
            lightningTrait.displayColor = new Color(1f, 1f, 0.4f);
            lightningTrait.canStun = true;
            allTraits.Add(lightningTrait);

            // Add the other 7 traits with their properties...
            // Earth
            Trait earthTrait = new Trait();
            earthTrait.type = TraitType.Earth;
            earthTrait.displayName = "Earth";
            earthTrait.description = "Increases damage resistance";
            earthTrait.displayColor = new Color(0.6f, 0.4f, 0.2f);
            allTraits.Add(earthTrait);

            // Wind
            Trait windTrait = new Trait();
            windTrait.type = TraitType.Wind;
            windTrait.displayName = "Wind";
            windTrait.description = "Improves dodge timing";
            windTrait.displayColor = new Color(0.8f, 1f, 0.8f);
            windTrait.dodgeTimeBonus = 0.1f;
            allTraits.Add(windTrait);

            // Water
            Trait waterTrait = new Trait();
            waterTrait.type = TraitType.Water;
            waterTrait.displayName = "Water";
            waterTrait.description = "Recovers health over time";
            waterTrait.displayColor = new Color(0.2f, 0.6f, 1f);
            allTraits.Add(waterTrait);

            // Light
            Trait lightTrait = new Trait();
            lightTrait.type = TraitType.Light;
            lightTrait.displayName = "Light";
            lightTrait.description = "Increases critical hit chance";
            lightTrait.displayColor = new Color(1f, 1f, 0.8f);
            lightTrait.criticalChance = 0.15f;
            allTraits.Add(lightTrait);

            // Shadow
            Trait shadowTrait = new Trait();
            shadowTrait.type = TraitType.Shadow;
            shadowTrait.displayName = "Shadow";
            shadowTrait.description = "Has a chance to dodge automatically";
            shadowTrait.displayColor = new Color(0.4f, 0.4f, 0.5f);
            allTraits.Add(shadowTrait);

            // Poison
            Trait poisonTrait = new Trait();
            poisonTrait.type = TraitType.Poison;
            poisonTrait.displayName = "Poison";
            poisonTrait.description = "Applies strong damage over time";
            poisonTrait.displayColor = new Color(0.6f, 1f, 0.4f);
            poisonTrait.causesDoT = true;
            allTraits.Add(poisonTrait);

            // Steel
            Trait steelTrait = new Trait();
            steelTrait.type = TraitType.Steel;
            steelTrait.displayName = "Steel";
            steelTrait.description = "Increases base damage";
            steelTrait.displayColor = new Color(0.7f, 0.7f, 0.7f);
            steelTrait.damageMultiplier = 1.3f;
            allTraits.Add(steelTrait);
        }
    }

    public Trait GetTraitByType(TraitType type)
    {
        return allTraits.Find(t => t.type == type);
    }

    public List<Trait> GetRandomTraits(int count)
    {
        List<Trait> availableTraits = new List<Trait>(allTraits);
        List<Trait> randomTraits = new List<Trait>();

        for (int i = 0; i < count && availableTraits.Count > 0; i++)
        {
            int randomIndex = Random.Range(0, availableTraits.Count);
            randomTraits.Add(availableTraits[randomIndex]);
            availableTraits.RemoveAt(randomIndex);
        }

        return randomTraits;
    }
}