using KeepWhatYouPickUp.Core;
using MCM.Abstractions.Attributes;
using MCM.Abstractions.Attributes.v2;
using MCM.Abstractions.Base.Global;
using MCM.Common;

namespace KeepWhatYouPickUp.Data;

// Used only when MCM is loaded. The only file that may use MCM types.
internal sealed class MCMSettings : AttributeGlobalSettings<MCMSettings>
{
    // Released Id: changing it resets every player's saved settings.
    private const string SettingsId = ModInfo.ModuleId;
    private const string SaveFormat = "json2";
    private const string PercentFormat = "0'%'";

    public override string Id => SettingsId;

    public override string DisplayName => Texts.ModName.ToString();

    public override string FolderName => ModInfo.ModuleId;

    public override string FormatType => SaveFormat;

    [SettingPropertyBool(Texts.Ids.EnabledName, Order = 0, RequireRestart = false)]
    [SettingPropertyGroup(Texts.Ids.GroupGeneral, GroupOrder = 0)]
    public bool Enabled { get; set; } = ModConfiguration.EnabledDefault;

    // Item order = HeroScope values (MCM saves the index).
    [SettingPropertyDropdown(Texts.Ids.ScopeName, Order = 1, RequireRestart = false, HintText = Texts.Ids.ScopeHint)]
    [SettingPropertyGroup(Texts.Ids.GroupGeneral, GroupOrder = 0)]
    public Dropdown<string> Scope { get; set; } = new([Texts.Ids.ScopePlayer, Texts.Ids.ScopeClan, Texts.Ids.ScopeAll], (int)ModConfiguration.ScopeDefault);

    [SettingPropertyBool(Texts.Ids.EnforceSkillName, Order = 2, RequireRestart = false, HintText = Texts.Ids.EnforceSkillHint)]
    [SettingPropertyGroup(Texts.Ids.GroupGeneral, GroupOrder = 0)]
    public bool EnforceSkillRequirement { get; set; } = ModConfiguration.EnforceSkillRequirementDefault;

    [SettingPropertyBool(Texts.Ids.HighlightName, Order = 3, RequireRestart = false, HintText = Texts.Ids.HighlightHint)]
    [SettingPropertyGroup(Texts.Ids.GroupGeneral, GroupOrder = 0)]
    public bool HighlightUnusable { get; set; } = ModConfiguration.HighlightUnusableDefault;

    [SettingPropertyBool(Texts.Ids.BlockName, Order = 4, RequireRestart = false, HintText = Texts.Ids.BlockHint)]
    [SettingPropertyGroup(Texts.Ids.GroupGeneral, GroupOrder = 0)]
    public bool BlockUnusablePickup { get; set; } = ModConfiguration.BlockUnusablePickupDefault;

    [SettingPropertyBool(Texts.Ids.LoseSpentName, Order = 5, RequireRestart = false, HintText = Texts.Ids.LoseSpentHint)]
    [SettingPropertyGroup(Texts.Ids.GroupGeneral, GroupOrder = 0)]
    public bool LoseSpentWeapons { get; set; } = ModConfiguration.LoseSpentWeaponsDefault;

    [SettingPropertyBool(Texts.Ids.PersistDropsName, Order = 0, RequireRestart = false, HintText = Texts.Ids.PersistDropsHint)]
    [SettingPropertyGroup(Texts.Ids.GroupDropped, GroupOrder = 1)]
    public bool PersistSceneDrops { get; set; } = ModConfiguration.PersistSceneDropsDefault;

    [SettingPropertyFloatingInteger(Texts.Ids.StolenName, ModConfiguration.StolenChanceMin, ModConfiguration.StolenChanceMax, PercentFormat, Order = 1, RequireRestart = false, HintText = Texts.Ids.StolenHint)]
    [SettingPropertyGroup(Texts.Ids.GroupDropped, GroupOrder = 1)]
    public float StolenChance { get; set; } = ModConfiguration.StolenChanceDefault;

    // "Technical settings" is always the last group and "Logging for debugging" its last entry.
    [SettingPropertyBool(Texts.Ids.LoggingEnabled, Order = 0, RequireRestart = false, HintText = Texts.Ids.LoggingEnabledHint)]
    [SettingPropertyGroup(Texts.Ids.GroupTechnical, GroupOrder = 2)]
    public bool LoggingEnabled { get; set; } = ModConfiguration.LoggingEnabledDefault;
}
