# Features

```
| Feature | Gate | Files |
|---|---|---|
| Keep picked-up weapons after a mission (KO/killed uses slots read right before death) | setting:Enabled + setting:Scope | `Missions\KeepPickupsMissionBehavior.cs`, `Core\EquipmentPersistence.cs`, `Patches\AgentPatch.cs` |
| Broken shields / used-up javelins and ammo stay equipped (only player drops clear a slot) | setting:Enabled | `Missions\KeepPickupsMissionBehavior.cs`, `Core\EquipmentPersistence.cs` |
| Realism: broken shields / used-up javelins and ammo are lost after the mission | setting:LoseSpentWeapons (off by default) | `Core\EquipmentPersistence.cs` |
| Unusable pickups (skill too low, military item in civilian set) go to party inventory; AI heroes without a party keep them equipped | setting:EnforceSkillRequirement (skill) / always (civilian) | `Core\EquipmentPersistence.cs` |
| Pickup warnings + leave summary messages | setting:Enabled | `Missions\KeepPickupsMissionBehavior.cs`, `Core\Texts.cs` |
| Red outline on items above the player's skill | setting:HighlightUnusable | `Missions\UnusableItemHighlightBehavior.cs`, `Core\Balance.cs` |
| Block picking up items above the player's skill | setting:BlockUnusablePickup | `Patches\SpawnedItemEntityPatch.cs` |
| Player drops stay in settlement scenes between visits, daily steal chance | setting:PersistSceneDrops, setting:StolenChance | `Missions\SceneDropsMissionBehavior.cs`, `Behaviors\SceneDropsCampaignBehavior.cs`, `Data\DroppedItemRecord.cs`, `Data\KeepWhatYouPickUpSaveDefiner.cs` |
| Settings (MCM when loaded, else ModConfiguration.xml) | always | `Core\ModSettings.cs`, `Data\ModConfiguration.cs`, `Data\MCMSettings.cs`, `Data\HeroScope.cs`, `_Module\ModConfiguration.xml` |
| Localization (English + 12 languages, incl. all settings) | always | `Core\Texts.cs`, `Data\MCMSettings.cs`, `_Module\ModuleData\Languages\` |
| Log file (problems always, debug detail when on) | setting:LoggingEnabled | `Diagnostics\KwypuLog.cs`, `Core\ModInfo.cs` |
```
