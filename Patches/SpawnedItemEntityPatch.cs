using HarmonyLib;
using KeepWhatYouPickUp.Core;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace KeepWhatYouPickUp.Patches;

// KWYPU-FEATURE: player cannot pick up items above the hero's skill | gate: setting:BlockUnusablePickup | Postfix: same gate vanilla uses to block empty quivers; covers the focus "interactable" check and native pickup
[HarmonyPatch(typeof(SpawnedItemEntity), nameof(SpawnedItemEntity.IsDisabledForAgent))]
internal class SpawnedItemEntityPatch
{
    private static void Postfix(SpawnedItemEntity __instance, Agent agent, ref bool __result)
    {
        if (__result || __instance == null || agent == null || !agent.IsMainAgent) return;
        if (!ModSettings.BlockUnusablePickup) return;
        MissionWeapon w = __instance.WeaponCopy;
        if (!w.IsEmpty && !EquipmentPersistence.MeetsSkill(agent.Character, w.Item)) __result = true;
    }
}