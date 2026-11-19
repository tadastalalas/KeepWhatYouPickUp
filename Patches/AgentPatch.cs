using HarmonyLib;
using KeepWhatYouPickUp.Missions;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace KeepWhatYouPickUp.Patches;

// KWYPU-FEATURE: broken shields / spent javelins seen for KO/killed heroes | gate: setting:Enabled + setting:Scope | Prefix: slots must be read before native death drops weapons (never skips)
[HarmonyPatch(typeof(Agent), nameof(Agent.Die), typeof(Blow), typeof(Agent.KillInfo))]
internal class AgentDiePatch
{
    private static void Prefix(Agent __instance)
    {
        if (__instance == null) return;
        KeepPickupsMissionBehavior.Current?.RecordDeath(__instance);
    }
}

// KWYPU-FEATURE: keep picked-up weapons + pickup warnings | gate: setting:Enabled + setting:Scope | Postfix: slot is filled before OnItemPickup returns
[HarmonyPatch(typeof(Agent), nameof(Agent.OnItemPickup))]
internal class AgentOnItemPickupPatch
{
    private static void Postfix(Agent __instance, SpawnedItemEntity spawnedItemEntity)
    {
        if (__instance == null || spawnedItemEntity == null) return;
        KeepPickupsMissionBehavior.Current?.RecordPickup(__instance, spawnedItemEntity);
    }
}