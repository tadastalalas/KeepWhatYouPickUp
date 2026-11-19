using HarmonyLib;
using KeepWhatYouPickUp.Behaviors;
using KeepWhatYouPickUp.Core;
using KeepWhatYouPickUp.Missions;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace KeepWhatYouPickUp;

public sealed class SubModule : MBSubModuleBase
{
    private Harmony? _harmony;

    // Every game (campaign and custom battle): the pickup block patch also runs outside the campaign.
    protected override void OnGameStart(Game game, IGameStarter gameStarterObject)
    {
        base.OnGameStart(game, gameStarterObject);
        ModSettings.Reload();
        _harmony = new Harmony(ModInfo.HarmonyId);
        // Assembly passed explicitly: the no-argument PatchAll() takes it from the call stack, which under the
        // module loader can be the loader's assembly, so no patch would be applied.
        _harmony.PatchAll(typeof(SubModule).Assembly);
        if (gameStarterObject is CampaignGameStarter starter) starter.AddBehavior(new SceneDropsCampaignBehavior());
    }

    public override void OnMissionBehaviorInitialize(Mission mission)
    {
        base.OnMissionBehaviorInitialize(mission);
        if (mission != null && Campaign.Current != null)
        {
            mission.AddMissionBehavior(new KeepPickupsMissionBehavior());
            mission.AddMissionBehavior(new SceneDropsMissionBehavior());
            mission.AddMissionBehavior(new UnusableItemHighlightBehavior());
        }
    }

    public override void OnGameEnd(Game game)
    {
        base.OnGameEnd(game);
        _harmony?.UnpatchAll(ModInfo.HarmonyId);
    }
}