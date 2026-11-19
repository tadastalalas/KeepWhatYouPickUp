using KeepWhatYouPickUp.Core;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.MountAndBlade;

namespace KeepWhatYouPickUp.Missions;

// KWYPU-FEATURE: red contour on ground items above the player's skill | gate: setting:HighlightUnusable
internal sealed class UnusableItemHighlightBehavior : MissionLogic
{
    private SpawnedItemEntity _highlighted;

    // Runs after SpawnedItemEntity.OnFocusGain, so this overrides vanilla's green contour.
    public override void OnFocusGained(Agent agent, IFocusable focusableObject, bool isInteractable)
    {
        if (agent == null || !agent.IsMainAgent || focusableObject is not SpawnedItemEntity item) return;
        if (!ModSettings.HighlightUnusable) return;
        MissionWeapon w = item.WeaponCopy;
        if (w.IsEmpty || EquipmentPersistence.MeetsSkill(agent.Character, w.Item) || !item.GameEntity.IsValid) return;
        GameEntity.CreateFromWeakEntity(item.GameEntity)?.SetContourColor(Balance.UnusableItemContourColor, true);
        _highlighted = item;
    }

    public override void OnFocusLost(Agent agent, IFocusable focusableObject)
    {
        if (_highlighted == null || focusableObject != _highlighted) return;
        if (!_highlighted.IsRemoved && _highlighted.GameEntity.IsValid) GameEntity.CreateFromWeakEntity(_highlighted.GameEntity)?.SetContourColor(null);
        _highlighted = null;
    }

    public override void OnRemoveBehavior()
    {
        _highlighted = null;
        base.OnRemoveBehavior();
    }
}