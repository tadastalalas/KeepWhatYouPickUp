using System.Collections.Generic;
using KeepWhatYouPickUp.Data;
using KeepWhatYouPickUp.Diagnostics;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace KeepWhatYouPickUp.Core;

// KWYPU-FEATURE: rules facade: which heroes keep pickups, skill/civilian checks, writing kept weapons to the hero | gate: setting:Enabled + setting:Scope
internal static class EquipmentPersistence
{
    internal static bool IsEnforced => ModSettings.EnforceSkillRequirement;

    internal static bool IsSpentLost => ModSettings.LoseSpentWeapons;

    internal static bool IsEligible(Agent agent, out Hero hero)
    {
        hero = null;
        if (agent?.Equipment == null || !agent.IsHero || Campaign.Current == null) return false;
        hero = (agent.Character as CharacterObject)?.HeroObject;
        // IsAlive guard: dead heroes resolve BattleEquipment to the shared DeadBattleEquipment.
        if (hero == null || !hero.IsAlive) return false;
        if (!ModSettings.Enabled) return false;
        return ModSettings.Scope switch
        {
            HeroScope.PlayerOnly => hero == Hero.MainHero,
            HeroScope.PlayerAndClan => hero == Hero.MainHero || hero.Clan == Clan.PlayerClan,
            _ => true,
        };
    }

    // drops = own items the hero dropped while alive and did not pick back up.
    // kept = new items written to the hero's set; stored = picked-up items that cannot be equipped (go to inventory).
    internal static void Apply(Agent agent, Hero hero, EquipmentElement[] snapshot, bool useSnapshotForEmpty,
        List<EquipmentElement> drops, List<EquipmentElement> kept, List<EquipmentElement> stored)
    {
        Equipment target = ResolveTarget(agent, hero);
        if (target == null)
        {
            if (ModSettings.LoggingEnabled) KwypuLog.Debug($"Apply skip hero={hero.Name}: spawn gear matches no hero set");
            return;
        }
        MissionEquipment live = agent.Equipment;
        BasicCharacterObject character = agent.Character;
        // No inventory to move unusable pickups to (AI hero without a party): the hero keeps them equipped instead.
        bool canStore = StorageFor(hero) != null;
        for (EquipmentIndex i = EquipmentIndex.WeaponItemBeginSlot; i < EquipmentIndex.ExtraWeaponSlot; i++)
        {
            EquipmentElement original = target[i];
            if (original.IsQuestItem) continue;
            MissionWeapon w = live[i];
            EquipmentElement seen;
            if (!w.IsEmpty) seen = new EquipmentElement(w.Item, w.ItemModifier);
            else if (!useSnapshotForEmpty) seen = EquipmentElement.Invalid;
            else if (snapshot == null) continue;
            else seen = snapshot[(int)i];
            if (IsSame(seen, original)) continue;
            if (!seen.IsEmpty && (CanEquip(target, hero, i, seen.Item, character) || !canStore && Equipment.IsItemFitsToSlot(i, seen.Item)))
            {
                target.AddEquipmentToSlotWithoutAgent(i, seen);
                kept.Add(seen);
                continue;
            }
            if (!seen.IsEmpty) stored.Add(seen);
            // Broken shields and used-up javelins/ammo leave the slot without a drop; vanilla keeps those, unless LoseSpentWeapons.
            // TakeMatch first so a matching drop record is always consumed.
            if (!original.IsEmpty && (TakeMatch(drops, original) || IsSpentLost)) target.AddEquipmentToSlotWithoutAgent(i, EquipmentElement.Invalid);
        }
    }

    internal static void Store(Hero hero, List<EquipmentElement> items)
    {
        if (items.Count == 0) return;
        ItemRoster roster = StorageFor(hero);
        if (roster == null)
        {
            if (ModSettings.LoggingEnabled) KwypuLog.Debug($"Store skip hero={hero.Name}: no party roster, {items.Count} item(s) do not fit a weapon slot");
            return;
        }
        for (int i = 0; i < items.Count; i++) roster.AddToCounts(items[i], 1);
    }

    internal static ItemRoster StorageFor(Hero hero) =>
        (hero.PartyBelongedTo ?? (hero.Clan == Clan.PlayerClan ? MobileParty.MainParty : null))?.ItemRoster;

    // Matches spawn gear against hero sets so tournament/arena/custom loadouts are never written back.
    internal static Equipment ResolveTarget(Agent agent, Hero hero)
    {
        Equipment spawn = agent.SpawnEquipment;
        if (spawn == null) return null;
        if (Matches(spawn, hero.BattleEquipment)) return hero.BattleEquipment;
        if (Matches(spawn, hero.CivilianEquipment)) return hero.CivilianEquipment;
        return null;
    }

    internal static bool CanEquip(Equipment target, Hero hero, EquipmentIndex slot, ItemObject item, BasicCharacterObject character) =>
        Equipment.IsItemFitsToSlot(slot, item)
        && (!IsEnforced || MeetsSkill(character, item))
        && !IsCivilianBlocked(target, hero, item);

    internal static bool IsCivilianBlocked(Equipment target, Hero hero, ItemObject item) => target == hero.CivilianEquipment && !item.IsCivilian;

    internal static bool HasWeapon(Equipment target, EquipmentElement item)
    {
        for (EquipmentIndex i = EquipmentIndex.WeaponItemBeginSlot; i < EquipmentIndex.ExtraWeaponSlot; i++)
            if (IsSame(target[i], item)) return true;
        return false;
    }

    internal static bool IsSame(EquipmentElement a, EquipmentElement b) => a.Item == b.Item && a.ItemModifier == b.ItemModifier;

    internal static bool TakeMatch(List<EquipmentElement> list, EquipmentElement item)
    {
        if (list == null) return false;
        for (int i = 0; i < list.Count; i++)
        {
            if (!IsSame(list[i], item)) continue;
            list.RemoveAt(i);
            return true;
        }
        return false;
    }

    // Skips ExtraWeaponSlot (banners) and Horse/HorseHarness (stripped in sieges).
    private static bool Matches(Equipment a, Equipment b)
    {
        for (EquipmentIndex i = EquipmentIndex.WeaponItemBeginSlot; i < EquipmentIndex.ArmorItemEndSlot; i++)
        {
            if (i == EquipmentIndex.ExtraWeaponSlot) continue;
            if (a[i].Item != b[i].Item || a[i].ItemModifier != b[i].ItemModifier) return false;
        }
        return true;
    }

    internal static bool MeetsSkill(BasicCharacterObject c, ItemObject item)
    {
        SkillObject skill = item.RelevantSkill;
        return skill == null || item.Difficulty <= 0 || c == null || c.GetSkillValue(skill) >= item.Difficulty;
    }
}
