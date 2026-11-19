using System.Collections.Generic;
using KeepWhatYouPickUp.Data;
using TaleWorlds.CampaignSystem;

namespace KeepWhatYouPickUp.Behaviors;

// KWYPU-FEATURE: player drops stay in settlement scenes between visits (saved per scene) | gate: setting:PersistSceneDrops
// Released class name = data-store id: never rename.
public sealed class SceneDropsCampaignBehavior : CampaignBehaviorBase
{
    // Released SyncData key (kept exception to the <prefix>_<field> casing).
    private const string SceneDropsKey = "KWYPU_SceneDrops";

    private Dictionary<string, List<DroppedItemRecord>> _store = [];

    public override void RegisterEvents() { }

    // SyncData keys: KWYPU_SceneDrops
    public override void SyncData(IDataStore dataStore)
    {
        dataStore.SyncData(SceneDropsKey, ref _store);
        _store ??= [];
    }

    internal List<DroppedItemRecord> Get(string key) => _store.TryGetValue(key, out List<DroppedItemRecord> list) ? list : null;

    internal void Set(string key, List<DroppedItemRecord> records)
    {
        if (records == null || records.Count == 0) _store.Remove(key);
        else _store[key] = records;
    }
}
