using System;

namespace KeepWhatYouPickUp.Data;

// Shape of ModConfiguration.xml and the only place where defaults live (MCMSettings uses them too).
// One section per MCM group, same names, same order.
public class ModConfiguration
{
    public const bool EnabledDefault = true;
    public const HeroScope ScopeDefault = HeroScope.PlayerOnly;
    public const bool EnforceSkillRequirementDefault = true;
    public const bool HighlightUnusableDefault = true;
    public const bool BlockUnusablePickupDefault = false;
    public const bool LoseSpentWeaponsDefault = false;
    public const bool PersistSceneDropsDefault = true;
    // Percent per in-game day.
    public const float StolenChanceDefault = 3f;
    public const float StolenChanceMin = 0f;
    public const float StolenChanceMax = 100f;
    public const bool LoggingEnabledDefault = false;

    public GeneralSection General { get; set; } = new();

    public DroppedItemsSection DroppedItems { get; set; } = new();

    // Always the last section.
    public TechnicalSettingsSection TechnicalSettings { get; set; } = new();

    // Out-of-range values are fixed in memory only; the file is left as the player wrote it.
    internal void Clamp()
    {
        if (!Enum.IsDefined(typeof(HeroScope), General.Scope)) General.Scope = ScopeDefault;
        float stolen = DroppedItems.StolenChance;
        DroppedItems.StolenChance = float.IsNaN(stolen) ? StolenChanceDefault
            : stolen < StolenChanceMin ? StolenChanceMin
            : stolen > StolenChanceMax ? StolenChanceMax
            : stolen;
    }

    public class GeneralSection
    {
        public bool Enabled { get; set; } = EnabledDefault;
        public HeroScope Scope { get; set; } = ScopeDefault;
        public bool EnforceSkillRequirement { get; set; } = EnforceSkillRequirementDefault;
        public bool HighlightUnusable { get; set; } = HighlightUnusableDefault;
        public bool BlockUnusablePickup { get; set; } = BlockUnusablePickupDefault;
        public bool LoseSpentWeapons { get; set; } = LoseSpentWeaponsDefault;
    }

    public class DroppedItemsSection
    {
        public bool PersistSceneDrops { get; set; } = PersistSceneDropsDefault;
        public float StolenChance { get; set; } = StolenChanceDefault;
    }

    public class TechnicalSettingsSection
    {
        // Always the last entry.
        public bool LoggingEnabled { get; set; } = LoggingEnabledDefault;
    }
}
