﻿using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FFXIVClientStructs.FFXIV.Component.GUI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace XIVAuras.Helpers
{
    public static class CharacterState
    {
        private static readonly uint[] _goldenSaucerIDs = { 144, 388, 389, 390, 391, 579, 792, 899, 941 };

        public static bool IsCharacterBusy()
        {

            ICondition condition = Singletons.Get<ICondition>();
            return condition[ConditionFlag.BetweenAreas] ||
                condition[ConditionFlag.WatchingCutscene78] ||
                condition[ConditionFlag.OccupiedInCutSceneEvent] ||
                condition[ConditionFlag.CreatingCharacter] ||
                condition[ConditionFlag.BetweenAreas] ||
                condition[ConditionFlag.BetweenAreas51] ||
                condition[ConditionFlag.OccupiedSummoningBell] ||
                condition[ConditionFlag.OccupiedInEvent] ||
                condition[ConditionFlag.OccupiedInQuestEvent];
        }

        public static bool IsInCombat()
        {
            ICondition condition = Singletons.Get<ICondition>();
            return condition[ConditionFlag.InCombat];
        }

        public static bool IsInDuty()
        {
            ICondition condition = Singletons.Get<ICondition>();
            return condition[ConditionFlag.BoundByDuty];
        }

        public static bool IsPerforming()
        {
            ICondition condition = Singletons.Get<ICondition>();
            return condition[ConditionFlag.Performing];
        }

        public static bool IsInPvP()
        {
            var clientState = Singletons.Get<IClientState>();
            return clientState.IsPvP || clientState.TerritoryType == 250;
        }

        public static bool IsInGoldenSaucer()
        {
            return _goldenSaucerIDs.Any(id => id == Singletons.Get<IClientState>().TerritoryType);
        }

        public static Job GetCharacterJob()
        {
            var player = Singletons.Get<IClientState>().LocalPlayer;
            if (player is null)
            {
                return Job.UKN;
            }

            unsafe
            {
                return (Job)((Character*)player.Address)->CharacterData.ClassJob;
            }
        }

        public static int GetCharacterLevel()
        {
            return Singletons.Get<IClientState>().LocalPlayer?.Level ?? 0;
        }

        public static bool IsWeaponDrawn()
        {
            var player = Singletons.Get<IClientState>().LocalPlayer;
            return player != null && player.StatusFlags.HasFlag(StatusFlags.WeaponOut);
        }

        public static unsafe bool ShouldBeVisible()
        {
            if (Singletons.Get<IClientState>().LocalPlayer == null || IsCharacterBusy())
            {
                return false;
            }

            var gameGui = Singletons.Get<IGameGui>();
            var parameterWidget = (AtkUnitBase*)gameGui.GetAddonByName("_ParameterWidget", 1);
            var fadeMiddleWidget = (AtkUnitBase*)gameGui.GetAddonByName("FadeMiddle", 1);
            var paramenterVisible = parameterWidget != null && parameterWidget->IsVisible;
            var fadeMiddleVisible = fadeMiddleWidget != null && fadeMiddleWidget->IsVisible;
            return paramenterVisible && !fadeMiddleVisible;
        }

        public static bool IsJobType(Job job, JobType type, IEnumerable<Job>? jobList = null) => type switch
        {
            JobType.All => true,
            JobType.Tanks => job is Job.GLA or Job.MRD or Job.PLD or Job.WAR or Job.DRK or Job.GNB,
            JobType.Casters => job is Job.THM or Job.ACN or Job.BLM or Job.SMN or Job.RDM or Job.BLU,
            JobType.Melee => job is Job.PGL or Job.LNC or Job.ROG or Job.MNK or Job.DRG or Job.NIN or Job.SAM or Job.RPR,
            JobType.Ranged => job is Job.ARC or Job.BRD or Job.MCH or Job.DNC,
            JobType.Healers => job is Job.CNJ or Job.WHM or Job.SCH or Job.AST or Job.SGE,
            JobType.DoH => job is Job.CRP or Job.BSM or Job.ARM or Job.GSM or Job.LTW or Job.WVR or Job.ALC or Job.CUL,
            JobType.DoL => job is Job.MIN or Job.BOT or Job.FSH,
            JobType.Combat => IsJobType(job, JobType.DoW) || IsJobType(job, JobType.DoM),
            JobType.DoW => IsJobType(job, JobType.Tanks) || IsJobType(job, JobType.Melee) || IsJobType(job, JobType.Ranged),
            JobType.DoM => IsJobType(job, JobType.Casters) || IsJobType(job, JobType.Healers),
            JobType.Crafters => IsJobType(job, JobType.DoH) || IsJobType(job, JobType.DoL),
            JobType.Custom => jobList is not null && jobList.Contains(job),
            _ => false
        };
    }
}
