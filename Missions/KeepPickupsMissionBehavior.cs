using System.Collections.Generic;
using KeepWhatYouPickUp.Core;
using KeepWhatYouPickUp.Diagnostics;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Missions;

namespace KeepWhatYouPickUp.Missions;

// KWYPU-FEATURE: picked-up weapons stay on the hero after the mission; unusable ones go to the party inventory | gate: setting:Enabled + setting:Scope
internal sealed class KeepPickupsMissionBehavior : MissionLogic
{
    private readonly Dictionary<Agent, EquipmentElement[]> _snapshots = [];
    private readonly Dictionary<Agent, List<EquipmentElement>> _drops = [];
    private readonly HashSet<Hero> _done = [];
    private bool _notKeptShown;

    internal static KeepPickupsMissionBehavior Current { get; private set; }

    public override void OnBehaviorInitialize()
    {
        base.OnBehaviorInitialize();
        Current = this;
    }

    public override void AfterStart()
    {
        Mission.OnItemDrop -= OnItemDrop;
        Mission.OnItemDrop += OnItemDrop;
    }

    public override void OnRemoveBehavior()
    {
        Mission.OnItemDrop -= OnItemDrop;
        Current = null;
        _snapshots.Clear();
        _drops.Clear();
        _done.Clear();
        base.OnRemoveBehavior();
    }

    // Called from the Agent.Die prefix: exact slots right before native KO/death, so broken shields and spent javelins are seen.
    internal void RecordDeath(Agent agent)
    {
        if (EquipmentPersistence.IsEligible(agent, out _)) Snapshot(agent);
    }

    internal void RecordPickup(Agent agent, SpawnedItemEntity spawned)
    {
        if (!EquipmentPersistence.IsEligible(agent, out Hero hero)) return;
        MissionWeapon w = spawned.WeaponCopy;
        EquipmentElement item = w.IsEmpty ? EquipmentElement.Invalid : new EquipmentElement(w.Item, w.ItemModifier);
        if (_drops.TryGetValue(agent, out List<EquipmentElement> drops)) EquipmentPersistence.TakeMatch(drops, item);
        Snapshot(agent);
        if (agent.IsMainAgent && !item.IsEmpty) NotifyPickup(agent, hero, item);
    }

    // Fired only from Mission.SpawnWeaponAsDropFromAgentAux, after the slot is emptied. Drops while alive only:
    // weapons falling from a knocked-out or killed hero must not cost them their gear.
    private void OnItemDrop(Agent agent, SpawnedItemEntity spawned)
    {
        if (agent == null || spawned == null || agent.Health <= 0f) return;
        if (!EquipmentPersistence.IsEligible(agent, out _)) return;
        MissionWeapon w = spawned.WeaponCopy;
        if (w.IsEmpty) return;
        if (!_drops.TryGetValue(agent, out List<EquipmentElement> drops))
        {
            drops = [];
            _drops[agent] = drops;
        }
        drops.Add(new EquipmentElement(w.Item, w.ItemModifier));
        Snapshot(agent);
        if (ModSettings.LoggingEnabled) KwypuLog.Debug($"Drop hero={agent.Name} item={w.Item.StringId}");
    }

    // Last known slots (pickup, drop, right before death); used for KO/killed agents whose slots native may clear on death.
    private EquipmentElement[] Snapshot(Agent agent)
    {
        if (!_snapshots.TryGetValue(agent, out EquipmentElement[] snap))
        {
            snap = new EquipmentElement[(int)EquipmentIndex.ExtraWeaponSlot];
            _snapshots[agent] = snap;
        }
        MissionEquipment live = agent.Equipment;
        for (int i = 0; i < snap.Length; i++)
        {
            MissionWeapon w = live[i];
            snap[i] = w.IsEmpty ? EquipmentElement.Invalid : new EquipmentElement(w.Item, w.ItemModifier);
        }
        return snap;
    }

    private void NotifyPickup(Agent agent, Hero hero, EquipmentElement item)
    {
        Equipment target = EquipmentPersistence.ResolveTarget(agent, hero);
        if (target == null)
        {
            if (_notKeptShown) return;
            _notKeptShown = true;
            Show(Texts.PickupNotKept);
            return;
        }
        if (EquipmentPersistence.HasWeapon(target, item)) return;
        ItemObject it = item.Item;
        if (EquipmentPersistence.IsEnforced && !EquipmentPersistence.MeetsSkill(agent.Character, it))
            Show(Texts.PickupSkill(item.GetModifiedItemName(), it.RelevantSkill.Name, it.Difficulty, agent.Character.GetSkillValue(it.RelevantSkill)));
        else if (EquipmentPersistence.IsCivilianBlocked(target, hero, it))
            Show(Texts.PickupCivilian(item.GetModifiedItemName()));
    }

    public override void OnAgentRemoved(Agent affectedAgent, Agent affectorAgent, AgentState agentState, KillingBlow blow)
    {
        if (affectedAgent == null) return;
        _snapshots.TryGetValue(affectedAgent, out EquipmentElement[] snap);
        Persist(affectedAgent, snap, agentState == AgentState.Killed || agentState == AgentState.Unconscious);
        _snapshots.Remove(affectedAgent);
        _drops.Remove(affectedAgent);
    }

    protected override void OnEndMission()
    {
        AgentReadOnlyList agents = Mission.Agents;
        for (int i = 0; i < agents.Count; i++)
        {
            Agent a = agents[i];
            _snapshots.TryGetValue(a, out EquipmentElement[] snap);
            Persist(a, snap, false);
        }
        _snapshots.Clear();
        _drops.Clear();
    }

    private void Persist(Agent agent, EquipmentElement[] snap, bool useSnapshotForEmpty)
    {
        if (!EquipmentPersistence.IsEligible(agent, out Hero hero) || !_done.Add(hero)) return;
        _drops.TryGetValue(agent, out List<EquipmentElement> drops);
        List<EquipmentElement> kept = [];
        List<EquipmentElement> stored = [];
        EquipmentPersistence.Apply(agent, hero, snap, useSnapshotForEmpty, drops, kept, stored);
        EquipmentPersistence.Store(hero, stored);
        if (hero == Hero.MainHero)
        {
            if (kept.Count > 0) Show(Texts.SummaryKept(Names(kept)));
            if (stored.Count > 0) Show(Texts.SummaryStored(Names(stored)));
        }
        if (ModSettings.LoggingEnabled) KwypuLog.Debug($"Persist hero={hero.Name} state={agent.State} kept={kept.Count} stored={stored.Count}");
    }

    private static string Names(List<EquipmentElement> items)
    {
        string[] names = new string[items.Count];
        for (int i = 0; i < items.Count; i++) names[i] = items[i].GetModifiedItemName().ToString();
        return string.Join(", ", names);
    }

    private static void Show(TextObject text) => InformationManager.DisplayMessage(new InformationMessage(text.ToString(), Colors.Yellow));
}
