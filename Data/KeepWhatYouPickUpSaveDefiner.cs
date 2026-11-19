using System.Collections.Generic;
using TaleWorlds.SaveSystem;

namespace KeepWhatYouPickUp.Data;

// Public: discovered via reflection by the save system. Released ids (base 974_120_000, class 1): never change;
// new types/containers are appended.
public sealed class KeepWhatYouPickUpSaveDefiner : SaveableTypeDefiner
{
    public KeepWhatYouPickUpSaveDefiner() : base(974_120_000) { }

    protected override void DefineClassTypes() => AddClassDefinition(typeof(DroppedItemRecord), 1);

    protected override void DefineContainerDefinitions()
    {
        ConstructContainerDefinition(typeof(List<DroppedItemRecord>));
        ConstructContainerDefinition(typeof(Dictionary<string, List<DroppedItemRecord>>));
    }
}