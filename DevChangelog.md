# KeepWhatYouPickUp developer changelog

## KWYPU v1.5.7.1

- Harmony id `KeepWhatYouPickUp` → `KeepWhatYouPickUp.Harmony` (`Core\ModInfo.cs` `HarmonyId = ModuleId + ".Harmony"`).
  - Why: `Bannerlord.ModuleLoader.KeepWhatYouPickUp.dll` patches under the Harmony id `KeepWhatYouPickUp`; `UnpatchAll(ModInfo.HarmonyId)` in `SubModule.OnGameEnd` removed the loader's patches too, so the next save loaded in the same session got no `OnGameStart` (no behaviors, no patches, possible vanilla crash on the mod's saved objects).
  - `SubModule.cs` unchanged: `PatchAll(typeof(SubModule).Assembly)` was already explicit since 1.5.7.0; `UnpatchAll` passes the own id.
  - No other mod references the old id.
  - Save-compat: none (Harmony ids are not saved).
  - Test: Release build, load a save, a patched feature works (pickup blocked/kept); load it again without restarting, still works.

## KWYPU v1.5.7.0

- Merged the unreleased 1.1.0 changes into this version (1.1.0 never shipped).
- Gameplay (from 1.1.0):
  - `KeepPickupsMissionBehavior` + `EquipmentPersistence.Apply`: own items the hero dropped while alive (`Mission.OnItemDrop`, only when `agent.Health > 0`) are the only ones that clear a slot. Broken shields and spent javelins/ammo leave the slot without a drop and are kept, unless `LoseSpentWeapons`.
  - `AgentDiePatch` (`Agent.Die` Prefix) snapshots the weapon slots right before native KO/death so the same rule works for knocked-out/killed heroes.
  - Unusable pickups (skill too low with `EnforceSkillRequirement`, or a non-civilian item in the civilian set) go to the party `ItemRoster` (`EquipmentPersistence.Store`). Heroes without a party/roster keep them equipped if the item fits the slot.
  - `TakeMatch` always consumes a matching drop record, fixing the duplicate when the own weapon was dropped for an item that cannot be equipped.
  - `SceneDropsMissionBehavior.OnItemDrop` ignores drops at `Health <= 0` (KO/death drops), so settlement scenes no longer store a copy of gear the hero keeps.
  - Pickup warnings (`Texts.PickupSkill`, `PickupCivilian`, `PickupNotKept`) and leave summary (`SummaryKept`, `SummaryStored`), on screen via `InformationManager` (player notifications, not diagnostics).
  - `StolenChance` default 10 → 3 (% per day).
  - New setting `LoseSpentWeapons` (default false).
  - Localization: `Core\Texts.cs` + English + 12 languages for every message and setting.
- Migrated to the standard mod layout. No gameplay change.
- csproj: `Bannerlord.BuildResourcesLite.Sdk/1.2.3.41`, `netstandard2.0`, one DLL per game version (`$(ModuleId).v$(GameVersion)`) loaded by `Bannerlord.ModuleLoader.Injector` 1.0.1.50.
  - `<Version>` 1.1.0 → 1.5.7.0. `ModuleName` → `Keep What You Pick Up`. `ModuleUrl` kept (Nexus mod 13870).
  - Removed `net472;net6` targets, game `HintPath` references, `Bannerlord.BuildResources`, `Nullable`, `IsExternalInit`, `Harmony.Extensions` (unused).
  - Kept `Lib.Harmony` 2.4.2, `Bannerlord.MCM` 5.12.3 (`IncludeAssets="compile"`), `BUTR.Harmony.Analyzer` 1.0.1.50.
- `_Module\SubModule.xml`: single `Bannerlord Module Loader` SubModule entry; Harmony first via `$harmonyversion$`; MCM optional (`DependedModuleMetadata` with `$mcmversion$`, no longer in `DependedModules`). Removed hardcoded versions.
- Files moved (namespace = folder, file-scoped):
  - `EquipmentPersistence.cs` → `Core\` (rules facade).
  - `DroppedItemRecord.cs`, `KeepWhatYouPickUpSaveDefiner.cs` → `Data\`.
  - `SceneDropsCampaignBehavior.cs` → `Behaviors\` (now `public`; class name unchanged).
  - `KeepPickupsMissionBehavior.cs`, `SceneDropsMissionBehavior.cs`, `UnusableItemHighlightBehavior.cs` → `Missions\`.
  - `Patches\AgentDiePatch.cs` + `Patches\AgentOnItemPickupPatch.cs` → `Patches\AgentPatch.cs` (classes `AgentDiePatch`, `AgentOnItemPickupPatch`).
  - `Patches\SpawnedItemEntityIsDisabledForAgentPatch.cs` → `Patches\SpawnedItemEntityPatch.cs` (class `SpawnedItemEntityPatch`).
  - Patch classes `internal static class` → `internal class`.
  - `_Module\KWYPU_Changelog.txt` → `_Module\Changelog.txt`.
- Settings → SETTINGS pattern:
  - `Settings.cs` → `Data\MCMSettings.cs` (MCM Id and FolderName `KeepWhatYouPickUp` kept, `json2`, property names and order kept, Scope dropdown order kept).
  - Group `Debug` → `Technical settings` (GroupOrder 2). `EnableLogging` → `LoggingEnabled` (resets that one setting to off; harmless).
  - New `Data\ModConfiguration.cs` (defaults/ranges), `Data\HeroScope.cs` (`PlayerOnly`, `PlayerAndClan`, `AllHeroes` = dropdown index), `Core\ModSettings.cs`, `_Module\ModConfiguration.xml`.
  - xml keys: `General/Enabled`, `General/Scope`, `General/EnforceSkillRequirement`, `General/HighlightUnusable`, `General/BlockUnusablePickup`, `General/LoseSpentWeapons`, `DroppedItems/PersistSceneDrops`, `DroppedItems/StolenChance` (0–100, clamped, NaN → 3), `TechnicalSettings/LoggingEnabled`.
  - MCM Scope index mapping unchanged: 0 → player, 1 → player clan, anything else → all heroes.
  - `ModSettings.Reload()` runs in `SubModule.OnGameStart` for every game (the `BlockUnusablePickup` patch also runs in custom battles).
  - Fallback when MCM is loaded but `MCMSettings.Instance` is null: now the xml values (old code used hardcoded fallbacks, e.g. StolenChance 0, HighlightUnusable off).
- `Core\Balance.cs`: `UnusableItemContourColor` (was `UnusableItemHighlightBehavior.Red`), `PercentDivisor` (StolenChance / 100).
- `SceneDropsMissionBehavior`: literals `"arena"`, `"|"`, `"HasLifeTime"`, `"_deletionTimer"` → named consts. One-time `KwypuLog.Warn` if the `HasLifeTime` setter or `_deletionTimer` field is not found.
- Logging: `Log.cs` → `Diagnostics\KwypuLog.cs` (Debug/Warn/Error). File `Modules\KeepWhatYouPickUp\logs\KeepWhatYouPickUp_<yyyyMMdd>.log` (was `logs\KeepWhatYouPickUp.log`). Debug lines on frequent paths (`OnItemDrop`, `Persist`, scene spawn/save) test `ModSettings.LoggingEnabled` before building the string.
- Texts: ids kept, English + 12 translations changed for `KWYPU_bZ6wL0e` (Technical settings), `KWYPU_L5dXg2w` (Logging for debugging), `KWYPU_y0Hp6Me` (Logging for debugging (English only). [Default: disabled]). Consts renamed `GroupDebug` → `GroupTechnical`, `LoggingName` → `LoggingEnabled`, `LoggingHint` → `LoggingEnabledHint`.
- 1.5.7 check (`BannerlordSource1.5.7`): no `#if` needed.
  - Patch targets: `Agent.Die(Blow, Agent.KillInfo)`, `Agent.OnItemPickup(SpawnedItemEntity spawnedItemEntity, EquipmentIndex, out bool)`, `SpawnedItemEntity.IsDisabledForAgent(Agent agent)`.
  - String-named reflection: `SpawnedItemEntity.HasLifeTime` setter, `SpawnedItemEntity._deletionTimer` (Timer). Re-verify on every new game version.
  - APIs: `Mission.OnItemDrop`, `Mission.SpawnWeaponWithNewEntity`, `WeaponSpawnFlags.WithStaticPhysics`, `IsFieldBattle`/`IsSiegeBattle`/`IsSallyOutBattle`/`IsNavalBattle`/`IsNavalRaidBattle`, `WeakGameEntity`, `GameEntity.CreateFromWeakEntity`, `SetContourColor`, `Equipment.AddEquipmentToSlotWithoutAgent`, `ArenaPracticeFightMissionController`, `TournamentFightMissionController`.
  - All of these exist in 1.4.7, 1.4.8 and 1.5.7: `Build-AllVersions.ps1` passes for all three with 0 errors, so no `#if` is needed.
- Save compatibility: none. SyncData key `KWYPU_SceneDrops`, behavior class name `SceneDropsCampaignBehavior`, SaveDefiner base `974_120_000` / class id 1 unchanged; only namespaces changed (save ids are numeric).
- Test: `..\Build-AllVersions.ps1` (all listed versions); with MCM and without MCM; `ModConfiguration.xml` missing / broken / partial / out-of-range StolenChance / invalid Scope; pickups kept, unusable pickups stored, red contour, pickup block, settlement drops kept between visits; an old save with stored drops loads them.
