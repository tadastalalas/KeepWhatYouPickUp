using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using KeepWhatYouPickUp.Behaviors;
using KeepWhatYouPickUp.Core;
using KeepWhatYouPickUp.Data;
using KeepWhatYouPickUp.Diagnostics;
using SandBox.Missions.MissionLogics.Arena;
using SandBox.Tournaments.MissionLogics;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Settlements.Locations;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ObjectSystem;

namespace KeepWhatYouPickUp.Missions;

// KWYPU-FEATURE: player drops never despawn during the mission and stay in settlement scenes between visits, with a daily steal chance | gate: setting:PersistSceneDrops + setting:StolenChance
internal sealed class SceneDropsMissionBehavior : MissionLogic
{
    // Vanilla location id of the arena scene (tournaments/practice): drops there are never stored.
    private const string ArenaLocationId = "arena";
    private const string SceneKeySeparator = "|";
    private const string HasLifeTimePropertyName = "HasLifeTime";
    private const string DeletionTimerFieldName = "_deletionTimer";

    private static readonly MethodInfo HasLifeTimeSetter = AccessTools.PropertySetter(typeof(SpawnedItemEntity), HasLifeTimePropertyName);
    private static readonly FieldInfo DeletionTimerField = AccessTools.Field(typeof(SpawnedItemEntity), DeletionTimerFieldName);
    private static bool _reflectionWarned;

    private readonly List<(SpawnedItemEntity Entity, float DropDays)> _tracked = [];
    private string _sceneKey;

    private static bool Enabled => ModSettings.PersistSceneDrops;

    public override void OnRemoveBehavior()
    {
        Mission.OnItemDrop -= OnItemDrop;
        _tracked.Clear();
        base.OnRemoveBehavior();
    }

    public override void AfterStart()
    {
        Mission.OnItemDrop -= OnItemDrop;
        Mission.OnItemDrop += OnItemDrop;
        WarnMissingReflection();
        _sceneKey = ResolveSceneKey();
        bool log = ModSettings.LoggingEnabled;
        if (log) KwypuLog.Debug($"AfterStart key={_sceneKey ?? "null"} settlement={Settlement.CurrentSettlement?.StringId ?? "null"} loc={CampaignMission.Current?.Location?.StringId ?? "null"} combat={Mission.CombatType} teamAI={Mission.MissionTeamAIType} scene={Mission.SceneName}");
        if (_sceneKey == null) return;
        SceneDropsCampaignBehavior store = Campaign.Current?.GetCampaignBehavior<SceneDropsCampaignBehavior>();
        List<DroppedItemRecord> records = store?.Get(_sceneKey);
        if (log) KwypuLog.Debug($"AfterStart store={(store == null ? "null" : "ok")} records={records?.Count ?? -1}");
        if (records == null) return;
        float now = (float)CampaignTime.Now.ToDays;
        float chance = ModSettings.StolenChance / Balance.PercentDivisor;
        for (int i = records.Count - 1; i >= 0; i--)
        {
            DroppedItemRecord r = records[i];
            if (IsStolen(r, now, chance))
            {
                if (log) KwypuLog.Debug($"Stolen {r.ItemId}");
                records.RemoveAt(i);
                continue;
            }
            SpawnedItemEntity e = Spawn(r);
            if (log) KwypuLog.Debug($"Spawn {r.ItemId} -> {(e == null ? "failed" : "ok")}");
            if (e == null) records.RemoveAt(i);
            else _tracked.Add((e, r.DropDays));
        }
        store.Set(_sceneKey, records);
    }

    protected override void OnEndMission()
    {
        Mission.OnItemDrop -= OnItemDrop;
        bool log = ModSettings.LoggingEnabled;
        if (log) KwypuLog.Debug($"OnEndMission key={_sceneKey ?? "null"} tracked={_tracked.Count}");
        if (_sceneKey == null) return;
        SceneDropsCampaignBehavior store = Campaign.Current?.GetCampaignBehavior<SceneDropsCampaignBehavior>();
        if (store == null)
        {
            KwypuLog.Debug("OnEndMission store=null");
            return;
        }
        List<DroppedItemRecord> records = new(_tracked.Count);
        for (int i = 0; i < _tracked.Count; i++)
        {
            (SpawnedItemEntity e, float dropDays) = _tracked[i];
            if (e.IsRemoved)
            {
                KwypuLog.Debug("OnEndMission skip: entity removed");
                continue;
            }
            MissionWeapon w = e.WeaponCopy;
            if (w.IsEmpty)
            {
                KwypuLog.Debug("OnEndMission skip: weapon empty");
                continue;
            }
            records.Add(DroppedItemRecord.From(w.Item.StringId, w.ItemModifier?.StringId, w.Amount, w.Banner?.Serialize(), e.GameEntity.GetGlobalFrame(), dropDays));
            if (log) KwypuLog.Debug($"OnEndMission save {w.Item.StringId} amount={w.Amount}");
        }
        store.Set(_sceneKey, records);
        if (log) KwypuLog.Debug($"OnEndMission saved={records.Count}");
    }

    private void OnItemDrop(Agent agent, SpawnedItemEntity spawned)
    {
        bool log = ModSettings.LoggingEnabled;
        if (log) KwypuLog.Debug($"OnItemDrop main={agent?.IsMainAgent} item={spawned?.WeaponCopy.Item?.StringId ?? "null"} key={_sceneKey ?? "null"} enabled={Enabled}");
        // Health <= 0: knockout/death drop; KeepPickups keeps that gear on the hero, so storing it too would duplicate it.
        if (agent == null || spawned == null || !agent.IsMainAgent || !Enabled || agent.Health <= 0f) return;
        // Player drops never despawn during the session; vanilla deletes lifetime items after 180s.
        HasLifeTimeSetter?.Invoke(spawned, [false]);
        DeletionTimerField?.SetValue(spawned, new Timer(Mission.CurrentTime, float.MaxValue, true));
        bool storable = IsStorable(spawned.WeaponCopy);
        if (log) KwypuLog.Debug($"OnItemDrop storable={storable} setterFound={HasLifeTimeSetter != null} timerFound={DeletionTimerField != null}");
        if (_sceneKey != null && storable)
        {
            _tracked.Add((spawned, (float)CampaignTime.Now.ToDays));
            if (log) KwypuLog.Debug($"OnItemDrop tracked={_tracked.Count}");
        }
    }

    // String-named vanilla members: re-verify on every new game version.
    private static void WarnMissingReflection()
    {
        if (_reflectionWarned || HasLifeTimeSetter != null && DeletionTimerField != null) return;
        _reflectionWarned = true;
        KwypuLog.Warn($"SpawnedItemEntity member not found (setter {HasLifeTimePropertyName}: {HasLifeTimeSetter != null}, field {DeletionTimerFieldName}: {DeletionTimerField != null}). Player drops may despawn during the mission.");
    }

    private string ResolveSceneKey()
    {
        if (!Enabled || Campaign.Current == null) return null;
        Settlement settlement = Settlement.CurrentSettlement;
        Location location = CampaignMission.Current?.Location;
        if (settlement == null || settlement.IsHideout || location == null || location.StringId == ArenaLocationId) return null;
        if (Mission.GetMissionBehavior<TournamentFightMissionController>() != null
            || Mission.GetMissionBehavior<ArenaPracticeFightMissionController>() != null
            || Mission.IsFieldBattle || Mission.IsSiegeBattle || Mission.IsSallyOutBattle
            || Mission.IsNavalBattle || Mission.IsNavalRaidBattle) return null;
        return settlement.StringId + SceneKeySeparator + Mission.SceneName;
    }

    private SpawnedItemEntity Spawn(DroppedItemRecord r)
    {
        ItemObject item = MBObjectManager.Instance.GetObject<ItemObject>(r.ItemId);
        if (item == null) return null;
        ItemModifier modifier = r.ModifierId == null ? null : MBObjectManager.Instance.GetObject<ItemModifier>(r.ModifierId);
        Banner banner = string.IsNullOrEmpty(r.BannerKey) ? null : new Banner(r.BannerKey);
        MissionWeapon weapon = new(item, modifier, banner, r.Amount);
        MatrixFrame frame = r.ToFrame();
        return Mission.SpawnWeaponWithNewEntity(ref weapon, Mission.WeaponSpawnFlags.WithStaticPhysics, frame)?.GetFirstScriptOfType<SpawnedItemEntity>();
    }

    // Per-day roll: survival = (1 - chance)^days; clock advanced by whole days so remainder carries over.
    private static bool IsStolen(DroppedItemRecord r, float now, float chance)
    {
        int days = (int)(now - r.DropDays);
        if (days <= 0) return false;
        r.DropDays += days;
        return chance > 0f && MBRandom.RandomFloat > MathF.Pow(1f - chance, days);
    }

    private static bool IsStorable(in MissionWeapon w) =>
        !w.IsEmpty && !w.IsBanner() && !w.Item.ItemFlags.HasAnyFlag(ItemFlags.DropOnWeaponChange | ItemFlags.DropOnAnyAction);
}