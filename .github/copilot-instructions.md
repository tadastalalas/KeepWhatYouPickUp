KeepWhatYouPickUp: workspace instructions. They override the global file.

- <Prefix>: KWYPU (Kwypu in type names). <Root> namespace and ModuleId: KeepWhatYouPickUp.
- Logger: `Diagnostics\KwypuLog.cs` (Debug / Warn / Error).
- Harmony: PatchAll runs in `SubModule.OnGameStart` (assembly passed explicitly), UnpatchAll in `OnGameEnd`,
  Harmony id "KeepWhatYouPickUp" (`Core\ModInfo.cs` HarmonyId). Patches are inactive before the first game starts.
- `ModSettings.Reload()` runs in `SubModule.OnGameStart` for every game (campaign and custom battle), before PatchAll.
- Mission behaviors are added only in campaign missions (`SubModule.OnMissionBehaviorInitialize`, `Campaign.Current != null`).
- Rules facade: `Core\EquipmentPersistence.cs` (IsEligible, IsEnforced, IsSpentLost, MeetsSkill).
- Patches: `Patches\AgentPatch.cs` (`AgentDiePatch` Agent.Die, `AgentOnItemPickupPatch` Agent.OnItemPickup),
  `Patches\SpawnedItemEntityPatch.cs` (IsDisabledForAgent).
- String-named reflection (not checked by the compiler): `SpawnedItemEntity.HasLifeTime` setter and `_deletionTimer`
  field in `Missions\SceneDropsMissionBehavior.cs`. Re-verify on every new game version.
- Settings: "Applies To" dropdown maps to `Data\HeroScope.cs`; enum values = MCM dropdown index, append only.
- Saves data: SyncData key `KWYPU_SceneDrops` (`Behaviors\SceneDropsCampaignBehavior.cs`), SaveDefiner
  `Data\KeepWhatYouPickUpSaveDefiner.cs` base 974_120_000 (class `DroppedItemRecord` = 1, two containers).
- Kept exceptions (released, never change):
  - MCM Id and FolderName "KeepWhatYouPickUp", FormatType "json2", MCMSettings property names (MCM saves by name).
  - SyncData key `KWYPU_SceneDrops` (uppercase prefix, breaks the `<prefix>_<field>` casing rule).
  - CampaignBehavior class name `SceneDropsCampaignBehavior` (data-store id).
  - SaveDefiner base id 974_120_000, class id 1, `DroppedItemRecord` SaveableField ids.
  - Text ids KWYPU_ + 7 characters as shipped. Never rename them.
