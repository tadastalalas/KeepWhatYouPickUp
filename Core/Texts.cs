using TaleWorlds.Localization;

namespace KeepWhatYouPickUp.Core;

internal static class Texts
{
    #region Settings
    public static readonly TextObject ModName = new(Ids.ModName);
    #endregion

    #region Pickup
    public static readonly TextObject PickupNotKept = new(Ids.PickupNotKept);

    internal static TextObject PickupSkill(TextObject item, TextObject skill, int required, int current) =>
        new TextObject(Ids.PickupSkill)
            .SetTextVariable("ITEM", item)
            .SetTextVariable("SKILL", skill)
            .SetTextVariable("REQUIRED", required)
            .SetTextVariable("CURRENT", current);

    internal static TextObject PickupCivilian(TextObject item) => new TextObject(Ids.PickupCivilian).SetTextVariable("ITEM", item);
    #endregion

    #region Summary
    internal static TextObject SummaryKept(string items) => new TextObject(Ids.SummaryKept).SetTextVariable("ITEMS", items);

    internal static TextObject SummaryStored(string items) => new TextObject(Ids.SummaryStored).SetTextVariable("ITEMS", items);
    #endregion

    internal static class Ids
    {
        #region Settings
        public const string EnforceSkillHint = "{=KWYPU_c8WfQ0r}Picked-up weapons above the hero's skill go to the party inventory instead of being equipped.";
        public const string LoseSpentName = "{=KWYPU_Rk5wD2g}Lose Broken Shields And Spent Throwing Weapons";
        public const string LoseSpentHint = "{=KWYPU_p9Ns4Ty}Shields that break and throwing weapons or ammo used up in battle are lost after the mission. Off = they come back, as in vanilla.";
        public const string ModName = "{=KWYPU_aN3kT7v}Keep What You Pick Up";
        public const string GroupGeneral = "{=KWYPU_G1hQm4s}General";
        public const string GroupDropped = "{=KWYPU_D8rVx2p}Dropped Items";
        public const string GroupTechnical = "{=KWYPU_bZ6wL0e}Technical settings";
        public const string EnabledName = "{=KWYPU_E5tYu9c}Enabled";
        public const string ScopeName = "{=KWYPU_S2jKf6n}Applies To";
        public const string ScopeHint = "{=KWYPU_h4PzR1m}Whose picked-up weapons persist after a mission.";
        public const string ScopePlayer = "{=KWYPU_o7Xc3Wd}Player only";
        public const string ScopeClan = "{=KWYPU_C9vBn5q}Player + player clan heroes";
        public const string ScopeAll = "{=KWYPU_A1lMs8g}Player + all heroes";
        public const string EnforceSkillName = "{=KWYPU_k6Ue2Hy}Enforce Skill Requirement";
        public const string HighlightName = "{=KWYPU_r3Fo9Jt}Highlight Unusable Items Red";
        public const string HighlightHint = "{=KWYPU_w8Gi1Za}Items the player lacks the skill for get a red contour instead of green.";
        public const string BlockName = "{=KWYPU_B4yNd7x}Cannot Pick Up Unavailable Items";
        public const string BlockHint = "{=KWYPU_u2Qe5Ks}Player cannot pick up items that need more skill than the hero has.";
        public const string PersistDropsName = "{=KWYPU_P6mWa3r}Persist Dropped Items In Scenes";
        public const string PersistDropsHint = "{=KWYPU_f9Tc0Lh}Items the player drops stay for the whole mission; in settlement scenes they are kept for later visits.";
        public const string StolenName = "{=KWYPU_T7sJb4o}Stolen Equipment Chance";
        public const string StolenHint = "{=KWYPU_i1Vk8Rn}Chance per in-game day that an item left in a settlement scene is stolen. 0 = never.";
        public const string LoggingEnabled = "{=KWYPU_L5dXg2w}Logging for debugging";
        public const string LoggingEnabledHint = "{=KWYPU_y0Hp6Me}Logging for debugging (English only). [Default: disabled]";
        #endregion

        #region Pickup
        public const string PickupSkill = "{=KWYPU_q7Ld2xA}{ITEM} needs {SKILL} {REQUIRED} (you have {CURRENT}). It will be moved to your inventory when you leave.";
        public const string PickupCivilian = "{=KWYPU_Vb4nR8e}{ITEM} can't be worn with civilian clothes. It will be moved to your inventory when you leave.";
        public const string PickupNotKept = "{=KWYPU_m3TzK9w}Weapons picked up here are not kept when you leave.";
        #endregion

        #region Summary
        public const string SummaryKept = "{=KWYPU_Hs6pY1c}Weapons kept: {ITEMS}";
        public const string SummaryStored = "{=KWYPU_xE2jN5u}Moved to your inventory: {ITEMS}";
        #endregion
    }
}
