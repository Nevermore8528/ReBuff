using Dalamud.Game.ClientState.JobGauge.Enums;
using Dalamud.Game.ClientState.Objects;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Plugin.Services;
using ImGuiNET;
using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using XIVAuras.Helpers;
using CharacterStruct = FFXIVClientStructs.FFXIV.Client.Game.Character.Character;

namespace XIVAuras.Config
{
    public class CharacterStateTrigger : TriggerOptions
    {
        [JsonIgnore] private static readonly string[] _sourceOptions = Enum.GetNames<TriggerSource>();
        [JsonIgnore] private static readonly string[] _petOptions = new[] { "Has Pet", "Has No Pet" };

        [JsonIgnore] private string _hpValueInput = string.Empty;
        [JsonIgnore] private string _mpValueInput = string.Empty;
        [JsonIgnore] private string _levelValueInput = string.Empty;
        [JsonIgnore] private string _cpValueInput = string.Empty;
        [JsonIgnore] private string _gpValueInput = string.Empty;
        [JsonIgnore] private string _gaugeValueInput = string.Empty;
        public int JobValue;
        [JsonIgnore]
        private static readonly string[] _jobOptions = new[] {
        "--Tanks--",            // 0,
        "DRK", "GNB", "PLD", "WAR",         // 1, 2, 3, 4,
        "--Healers--",          // 5,
        "AST", "SCH", "SGE", "WHM",         // 6, 7, 8, 9,
        "--Casters--",          // 10,
        "BLM", "RDM", "SMN",                // 11, 12, 13,
        "--Ranged--",           // 14,
        "BRD", "DNC", "MCH",                // 15, 16, 17,
        "--Melee--",            // 18,
        "DRG", "MNK", "NIN", "RPR", "SAM",  // 19, 20, 21, 22, 23
        };
        public int PrevJobValue;
        public int PrevUnitOption;
        public int UnitOption;
        public float Unit = 0;
        public float MaxUnit = 0;

        public TriggerSource TriggerSource = TriggerSource.Player;

        public override TriggerType Type => TriggerType.CharacterState;
        public override TriggerSource Source => this.TriggerSource;

        public bool Level = false;
        public TriggerDataOp LevelOp = TriggerDataOp.GreaterThan;
        public float LevelValue;

        public bool Hp = false;
        public TriggerDataOp HpOp = TriggerDataOp.GreaterThan;
        public float HpValue;
        public bool MaxHp;

        public bool Mp = false;
        public TriggerDataOp MpOp = TriggerDataOp.GreaterThan;
        public float MpValue;
        public bool MaxMp;

        public bool Cp = false;
        public TriggerDataOp CpOp = TriggerDataOp.GreaterThan;
        public float CpValue;
        public bool MaxCp;

        public bool Gp = false;
        public TriggerDataOp GpOp = TriggerDataOp.GreaterThan;
        public float GpValue;
        public bool MaxGp;

        public bool Gauge = false;
        public TriggerDataOp GaugeOp = TriggerDataOp.GreaterThan;
        public float GaugeValue;
        public bool MaxGauge;

        public bool DutyAction = false;
        public uint? DutyAction1;
        public uint? DutyAction2;

        public bool PetCheck;
        public int PetValue;

        public override bool IsTriggered(bool preview, out DataSource data)
        {
            data = new DataSource();

            if (preview)
            {
                return true;
            }

            GameObject? actor = this.TriggerSource switch
            {
                TriggerSource.Player => Singletons.Get<IClientState>().LocalPlayer,
                TriggerSource.Target => Utils.FindTarget(),
                TriggerSource.TargetOfTarget => Utils.FindTargetOfTarget(),
                TriggerSource.FocusTarget => Singletons.Get<ITargetManager>().FocusTarget,
                _ => null
            };

            if (actor is not null)
            {
                data.Name = actor.Name.ToString();
            }

            if (actor is Character chara)
            {
                data.Hp = chara.CurrentHp;
                data.MaxHp = chara.MaxHp;
                data.Mp = chara.CurrentMp;
                data.MaxMp = chara.MaxMp;
                data.Cp = chara.CurrentCp;
                data.MaxCp = chara.MaxCp;
                data.Gp = chara.CurrentGp;
                data.MaxGp = chara.MaxGp;
                data.Level = chara.Level;
                data.Distance = chara.YalmDistanceX;
                data.HasPet = this.TriggerSource == TriggerSource.Player &&
                    Singletons.Get<IBuddyList>().PetBuddy != null;

                unsafe
                {
                    data.Job = (Job)((CharacterStruct*)chara.Address)->CharacterData.ClassJob;
                }
            }

            this.DutyAction1 = data.DutyAction1;
            this.DutyAction2 = data.DutyAction2;

            if (this.UnitOption >= 100)
            {
                switch (this.UnitOption)
                {   // Job Gauge Data

                    // DRK
                    case 100:
                        this.Unit = data.Blood;
                        this.MaxUnit = 100;
                        break;
                    case 101:
                        this.Unit = data.DarksideTimer_long;
                        this.MaxUnit = 60000;
                        break;
                    case 102:
                        this.Unit = data.ShadowTimer_long;
                        this.MaxUnit = 20000;
                        break;
                    case 103:
                        this.Unit = 0;
                        if (data.DarkArtsState)
                            this.Unit = 1;
                        this.MaxUnit = 1;
                        break;

                    // GNB
                    case 200:
                        this.Unit = data.Cartridges;
                        this.MaxUnit = 3;
                        break;

                    // PLD
                    case 300:
                        this.Unit = data.OathGauge;
                        this.MaxUnit = 100;
                        break;

                    // WAR
                    case 400:
                        this.Unit = data.BeastGauge;
                        this.MaxUnit = 100;
                        break;

                    // AST
                    case 600:
                        if (data.GetSeals[0] == SealType.NONE)
                        { this.Unit = 0; data.ASTSeal1 = ""; }
                        else if (data.GetSeals[0] == SealType.SUN)
                        { this.Unit = 1; data.ASTSeal1 = "Solar"; }
                        else if (data.GetSeals[0] == SealType.MOON)
                        { this.Unit = 2; data.ASTSeal1 = "Lunar"; }
                        else if (data.GetSeals[0] == SealType.CELESTIAL)
                        { this.Unit = 3; data.ASTSeal1 = "Celestial"; }
                        this.MaxUnit = 3;
                        break;
                    case 601:
                        if (data.GetSeals[1] == SealType.NONE)
                        { this.Unit = 0; data.ASTSeal2 = ""; }
                        else if (data.GetSeals[1] == SealType.SUN)
                        { this.Unit = 1; data.ASTSeal2 = "Solar"; }
                        else if (data.GetSeals[1] == SealType.MOON)
                        { this.Unit = 2; data.ASTSeal2 = "Lunar"; }
                        else if (data.GetSeals[1] == SealType.CELESTIAL)
                        { this.Unit = 3; data.ASTSeal2 = "Celestial"; }
                        this.MaxUnit = 3;
                        break;
                    case 602:
                        if (data.GetSeals[2] == SealType.NONE)
                        { this.Unit = 0; data.ASTSeal3 = ""; }
                        else if (data.GetSeals[2] == SealType.SUN)
                        { this.Unit = 1; data.ASTSeal3 = "Solar"; }
                        else if (data.GetSeals[2] == SealType.MOON)
                        { this.Unit = 2; data.ASTSeal3 = "Lunar"; }
                        else if (data.GetSeals[2] == SealType.CELESTIAL)
                        { this.Unit = 3; data.ASTSeal3 = "Celestial"; }
                        this.MaxUnit = 3;
                        break;

                    // SCH
                    case 700:
                        this.Unit = data.SCHAetherflow;
                        this.MaxUnit = 3;
                        break;
                    case 701:
                        this.Unit = data.FairyGauge;
                        this.MaxUnit = 100;
                        break;
                    case 702:
                        this.Unit = data.SeraphTimer;
                        this.MaxUnit = 22;
                        break;

                    // SGE
                    case 800:
                        this.Unit = 0;
                        if (data.Eukrasia)
                            this.Unit = 1;
                        this.MaxUnit = 1;
                        break;
                    case 801:
                        this.Unit = data.AddersgallTimer;
                        this.MaxUnit = 20000;
                        break;
                    case 802:
                        this.Unit = data.AddersgallStacks;
                        this.MaxUnit = 3;
                        break;
                    case 803:
                        this.Unit = data.AdderstingStacks;
                        this.MaxUnit = 3;
                        break;

                    // SGE
                    case 900:
                        this.Unit = data.LilyTimer;
                        this.MaxUnit = 20000;
                        break;
                    case 901:
                        this.Unit = data.Lilies;
                        this.MaxUnit = 3;
                        break;
                    case 902:
                        this.Unit = data.BloodLilies;
                        this.MaxUnit = 3;
                        break;
                    // SGE
                    case 1100:
                        this.Unit = data.ElementTimer;
                        this.MaxUnit = 15000;
                        break;
                    case 1101:
                        this.Unit = 0;
                        if (data.InUmbralIce)
                            this.Unit = 1;
                        this.MaxUnit = 1;
                        break;
                    case 1102:
                        this.Unit = 0;
                        if (data.InAstralFire)
                            this.Unit = 1;
                        this.MaxUnit = 1;
                        break;
                    case 1103:
                        this.Unit = data.UmbralHearts;
                        this.MaxUnit = 3;
                        break;
                    case 1104:
                        this.Unit = data.UmbralIceStacks;
                        this.MaxUnit = 3;
                        break;
                    case 1105:
                        this.Unit = data.AstralFireStacks;
                        this.MaxUnit = 3;
                        break;
                    case 1106:
                        this.Unit = data.EnochianTimer;
                        this.MaxUnit = 30000;
                        break;
                    case 1107:
                        this.Unit = data.PolyglotStacks;
                        this.MaxUnit = 1;
                        if (data.CurrentLevel >= 80)
                        { this.MaxUnit = 2; }
                        break;
                    case 1108:
                        this.Unit = 0;
                        if (data.IsEnochianActive)
                            this.Unit = 1;
                        this.MaxUnit = 1;
                        break;
                    case 1109:
                        this.Unit = 0;
                        if (data.IsParadoxActive)
                            this.Unit = 1;
                        this.MaxUnit = 1;
                        break;
                    // RDM
                    case 1200:
                        this.Unit = data.WhiteMana;
                        this.MaxUnit = 100;
                        break;
                    case 1201:
                        this.Unit = data.BlackMana;
                        this.MaxUnit = 100;
                        break;
                    case 1202:
                        this.Unit = data.ManaStacks;
                        this.MaxUnit = 3;
                        break;
                    // SMN
                    case 1300:
                        this.Unit = data.SMNAetherflow;
                        this.MaxUnit = 2;
                        break;
                    case 1301:
                        this.Unit = data.SummonTimer;
                        this.MaxUnit = 15000;
                        break;
                    case 1302:
                        this.Unit = data.AttunmentTimer;
                        this.MaxUnit = 30000;
                        break;
                    case 1303:
                        this.Unit = 0;
                        if (data.IsIfritReady)
                            this.Unit = 1;
                        this.MaxUnit = 1;
                        break;
                    case 1304:
                        this.Unit = 0;
                        if (data.IsGarudaReady)
                            this.Unit = 1;
                        this.MaxUnit = 1;
                        break;
                    case 1305:
                        this.Unit = 0;
                        if (data.IsTitanReady)
                            this.Unit = 1;
                        this.MaxUnit = 1;
                        break;
                    case 1306:
                        this.Unit = 0;
                        if (data.IsIfritAttuned)
                            this.Unit = 1;
                        this.MaxUnit = 1;
                        break;
                    case 1307:
                        this.Unit = 0;
                        if (data.IsGarudaAttuned)
                            this.Unit = 1;
                        this.MaxUnit = 1;
                        break;
                    case 1308:
                        this.Unit = 0;
                        if (data.IsTitanAttuned)
                            this.Unit = 1;
                        this.MaxUnit = 1;
                        break;
                    case 1309:
                        this.Unit = 0;
                        if (data.IsBahamutReady)
                            this.Unit = 1;
                        this.MaxUnit = 1;
                        break;
                    case 1310:
                        this.Unit = 0;
                        if (data.IsPhoenixReady)
                            this.Unit = 1;
                        this.MaxUnit = 1;
                        break;
                    // BRD
                    case 1500:
                        this.Unit = data.SongTimer;
                        this.MaxUnit = 45000;
                        break;
                    case 1501:
                        this.Unit = data.Repertoire;
                        this.MaxUnit = 0;
                        if (data.Song == Song.NONE)
                        { this.MaxUnit = 0; }
                        if (data.Song == Song.WANDERER)
                        { this.MaxUnit = 3; }
                        if (data.Song == Song.ARMY)
                        { this.MaxUnit = 4; }
                        if (data.Song == Song.MAGE)
                        { this.MaxUnit = 0; }
                        break;
                    case 1502:
                        this.Unit = data.SoulVoice;
                        this.MaxUnit = 100;
                        break;
                    case 1503:
                        if (data.Song == Song.NONE)
                        { this.Unit = 0; data.CurrentSong = ""; }
                        else if (data.Song == Song.WANDERER)
                        { this.Unit = 1; data.CurrentSong = "Wanderer's Minuet"; }
                        else if (data.Song == Song.MAGE)
                        { this.Unit = 2; data.CurrentSong = "Mage's Ballad"; }
                        else if (data.Song == Song.ARMY)
                        { this.Unit = 3; data.CurrentSong = "Army's Paeon"; }
                        this.MaxUnit = 3;
                        break;
                    case 1504:
                        if (data.Coda[0] == Song.NONE)
                        { this.Unit = 0; data.Coda1 = ""; }
                        else if (data.Coda[0] == Song.WANDERER)
                        { this.Unit = 1; data.Coda1 = "Wanderer's Minuet"; }
                        else if (data.Coda[0] == Song.MAGE)
                        { this.Unit = 2; data.Coda1 = "Mage's Ballad"; }
                        else if (data.Coda[0] == Song.ARMY)
                        { this.Unit = 3; data.Coda1 = "Army's Paeon"; }
                        this.MaxUnit = 3;
                        break;
                    case 1505:
                        if (data.Coda[1] == Song.NONE)
                        { this.Unit = 0; data.Coda2 = ""; }
                        else if (data.Coda[1] == Song.WANDERER)
                        { this.Unit = 1; data.Coda2 = "Wanderer's Minuet"; }
                        else if (data.Coda[1] == Song.MAGE)
                        { this.Unit = 2; data.Coda2 = "Mage's Ballad"; }
                        else if (data.Coda[1] == Song.ARMY)
                        { this.Unit = 3; data.Coda2 = "Army's Paeon"; }
                        this.MaxUnit = 3;
                        break;
                    case 1506:
                        if (data.Coda[2] == Song.NONE)
                        { this.Unit = 0; data.Coda3 = ""; }
                        else if (data.Coda[2] == Song.WANDERER)
                        { this.Unit = 1; data.Coda3 = "Wanderer's Minuet"; }
                        else if (data.Coda[2] == Song.MAGE)
                        { this.Unit = 2; data.Coda3 = "Mage's Ballad"; }
                        else if (data.Coda[2] == Song.ARMY)
                        { this.Unit = 3; data.Coda3 = "Army's Paeon"; }
                        this.MaxUnit = 3;
                        break;
                    // DNC
                    case 1600:
                        this.Unit = data.Feathers;
                        this.MaxUnit = 4;
                        break;
                    case 1601:
                        this.Unit = data.Esprit;
                        this.MaxUnit = 100;
                        break;
                    case 1602:
                        if ((data.NextStepID != 15999 && data.NextStepID != 16000 &&
                            data.NextStepID != 16001 && data.NextStepID != 16002) ||
                            data.CompletedSteps == 4)
                        { this.Unit = 0; data.NextStep = ""; }
                        else if (data.NextStepID == 15999 && data.CompletedSteps < 4)
                        { this.Unit = 1; data.NextStep = "Emboite"; data.Icon = 3455; }
                        else if (data.NextStepID == 16000 && data.CompletedSteps < 4)
                        { this.Unit = 2; data.NextStep = "Entrechat"; data.Icon = 3456; }
                        else if (data.NextStepID == 16001 && data.CompletedSteps < 4)
                        { this.Unit = 3; data.NextStep = "Jete"; data.Icon = 3457; }
                        else if (data.NextStepID == 16002 && data.CompletedSteps < 4)
                        { this.Unit = 4; data.NextStep = "Pirouette"; data.Icon = 3458; }
                        this.MaxUnit = 4;
                        break;
                    case 1603:
                        if (data.Steps[0] != 15999 && data.Steps[0] != 16000 &&
                            data.Steps[0] != 16001 && data.Steps[0] != 16002)
                        { this.Unit = 0; data.Step1 = ""; }
                        else if (data.Steps[0] == 15999)
                        { this.Unit = 1; data.Step1 = "Emboite"; data.Icon = 3455; }
                        else if (data.Steps[0] == 16000)
                        { this.Unit = 2; data.Step1 = "Entrechat"; data.Icon = 3456; }
                        else if (data.Steps[0] == 16001)
                        { this.Unit = 3; data.Step1 = "Jete"; data.Icon = 3457; }
                        else if (data.Steps[0] == 16002)
                        { this.Unit = 4; data.Step1 = "Pirouette"; data.Icon = 3458; }
                        this.MaxUnit = 4;
                        break;
                    case 1604:
                        if (data.Steps[1] != 15999 && data.Steps[1] != 16000 &&
                            data.Steps[1] != 16001 && data.Steps[1] != 16002)
                        { this.Unit = 0; data.Step2 = ""; }
                        else if (data.Steps[1] == 15999)
                        { this.Unit = 1; data.Step2 = "Emboite"; data.Icon = 3455; }
                        else if (data.Steps[1] == 16000)
                        { this.Unit = 2; data.Step2 = "Entrechat"; data.Icon = 3456; }
                        else if (data.Steps[1] == 16001)
                        { this.Unit = 3; data.Step2 = "Jete"; data.Icon = 3457; }
                        else if (data.Steps[1] == 16002)
                        { this.Unit = 4; data.Step2 = "Pirouette"; data.Icon = 3458; }
                        this.MaxUnit = 4;
                        break;
                    case 1605:
                        if (data.Steps[2] != 15999 && data.Steps[2] != 16000 &&
                            data.Steps[2] != 16001 && data.Steps[2] != 16002)
                        { this.Unit = 0; data.Step3 = ""; }
                        else if (data.Steps[2] == 15999)
                        { this.Unit = 1; data.Step3 = "Emboite"; data.Icon = 3455; }
                        else if (data.Steps[2] == 16000)
                        { this.Unit = 2; data.Step3 = "Entrechat"; data.Icon = 3456; }
                        else if (data.Steps[2] == 16001)
                        { this.Unit = 3; data.Step3 = "Jete"; data.Icon = 3457; }
                        else if (data.Steps[2] == 16002)
                        { this.Unit = 4; data.Step3 = "Pirouette"; data.Icon = 3458; }
                        this.MaxUnit = 4;
                        break;
                    case 1606:
                        if (data.Steps[3] != 15999 && data.Steps[3] != 16000 &&
                            data.Steps[3] != 16001 && data.Steps[3] != 16002)
                        { this.Unit = 0; data.Step4 = ""; }
                        else if (data.Steps[3] == 15999)
                        { this.Unit = 1; data.Step4 = "Emboite"; data.Icon = 3455; }
                        else if (data.Steps[3] == 16000)
                        { this.Unit = 2; data.Step4 = "Entrechat"; data.Icon = 3456; }
                        else if (data.Steps[3] == 16001)
                        { this.Unit = 3; data.Step4 = "Jete"; data.Icon = 3457; }
                        else if (data.Steps[3] == 16002)
                        { this.Unit = 4; data.Step4 = "Pirouette"; data.Icon = 3458; }
                        this.MaxUnit = 4;
                        break;
                    case 1607:
                        this.Unit = data.CompletedSteps;
                        break;
                    // MCH
                    case 1700:
                        this.Unit = data.Heat;
                        this.MaxUnit = 100;
                        break;
                    case 1701:
                        this.Unit = 0;
                        if (data.IsOverheated)
                            this.Unit = 1;
                        this.MaxUnit = 1;
                        break;
                    case 1702:
                        this.Unit = data.OverheatTimer;
                        this.MaxUnit = 10000;
                        break;
                    case 1703:
                        this.Unit = data.Battery;
                        this.MaxUnit = 100;
                        break;
                    case 1704:
                        this.Unit = data.LastSummonBatteryPower;
                        this.MaxUnit = 100;
                        break;
                    case 1705:
                        this.Unit = data.RookQueenTimer;
                        this.MaxUnit = 20000;
                        break;
                    case 1706:
                        this.Unit = 0;
                        if (data.IsRobotActive)
                            this.Unit = 1;
                        this.MaxUnit = 1;
                        break;
                    // DRG
                    case 1900:
                        this.Unit = data.LoTDTimer;
                        if (data.CurrentLevel < 78)
                        { this.MaxUnit = 20000; }
                        else
                        { this.MaxUnit = 30000; }
                        break;
                    case 1901:
                        this.Unit = 0;
                        if (data.IsLoTDActive)
                            this.Unit = 1;
                        this.MaxUnit = 1;
                        break;
                    case 1902:
                        this.Unit = data.EyeCount;
                        this.MaxUnit = 2;
                        break;
                    case 1903:
                        this.Unit = data.FMFocus;
                        this.MaxUnit = 2;
                        break;
                    // MNK
                    case 2000:
                        this.Unit = data.Chakra;
                        this.MaxUnit = 5;
                        break;
                    case 2001:
                        this.Unit = 0;
                        if (data.AvaliableNadi == Nadi.NONE)
                        { this.Unit = 0; data.Nadi = ""; }
                        else if (data.AvaliableNadi == Nadi.SOLAR)
                        { this.Unit = 1; data.Nadi = "Solar"; }
                        else if (data.AvaliableNadi == Nadi.LUNAR)
                        { this.Unit = 1; data.Nadi = "Lunar"; }
                        this.MaxUnit = 2;
                        break;
                    case 2002:
                        if (data.BeastChakra[0] == BeastChakra.NONE)
                        { this.Unit = 0; data.BeastChakra1 = ""; }
                        else if (data.BeastChakra[0] == BeastChakra.COEURL)
                        { this.Unit = 1; data.BeastChakra1 = "Coeurl"; }
                        else if (data.BeastChakra[0] == BeastChakra.OPOOPO)
                        { this.Unit = 2; data.BeastChakra1 = "Opo-opo"; }
                        else if (data.BeastChakra[0] == BeastChakra.RAPTOR)
                        { this.Unit = 3; data.BeastChakra1 = "Raptor"; }
                        this.MaxUnit = 3;
                        break;
                    case 2003:
                        if (data.BeastChakra[1] == BeastChakra.NONE)
                        { this.Unit = 0; data.BeastChakra2 = ""; }
                        else if (data.BeastChakra[1] == BeastChakra.COEURL)
                        { this.Unit = 1; data.BeastChakra2 = "Coeurl"; }
                        else if (data.BeastChakra[1] == BeastChakra.OPOOPO)
                        { this.Unit = 2; data.BeastChakra2 = "Opo-opo"; }
                        else if (data.BeastChakra[1] == BeastChakra.RAPTOR)
                        { this.Unit = 3; data.BeastChakra2 = "Raptor"; }
                        this.MaxUnit = 3;
                        break;
                    case 2004:
                        if (data.BeastChakra[2] == BeastChakra.NONE)
                        { this.Unit = 0; data.BeastChakra3 = ""; }
                        else if (data.BeastChakra[2] == BeastChakra.COEURL)
                        { this.Unit = 1; data.BeastChakra3 = "Coeurl"; }
                        else if (data.BeastChakra[2] == BeastChakra.OPOOPO)
                        { this.Unit = 2; data.BeastChakra3 = "Opo-opo"; }
                        else if (data.BeastChakra[2] == BeastChakra.RAPTOR)
                        { this.Unit = 3; data.BeastChakra3 = "Raptor"; }
                        this.MaxUnit = 3;
                        break;
                    // NIN
                    case 2100:
                        this.Unit = data.Ninki;
                        this.MaxUnit = 100;
                        break;
                    case 2101:
                        this.Unit = data.HutonTimer;
                        this.MaxUnit = 60;
                        break;
                    // RPR
                    case 2200:
                        this.Unit = data.Soul;
                        this.MaxUnit = 100;
                        break;
                    case 2201:
                        this.Unit = data.EnshroudTimer;
                        this.MaxUnit = 30000;
                        break;
                    case 2202:
                        this.Unit = data.Shroud;
                        this.MaxUnit = 100;
                        break;
                    case 2203:
                        this.Unit = data.LemureShroud;
                        this.MaxUnit = 5;
                        break;
                    case 2204:
                        this.Unit = data.VoidShroud;
                        this.MaxUnit = 5;
                        break;
                    // SAM
                    case 2300:
                        this.Unit = data.Kenki;
                        this.MaxUnit = 100;
                        break;
                    case 2301:
                        this.Unit = data.MeditationStacks;
                        this.MaxUnit = 3;
                        break;
                    case 2302:
                        this.Unit = 0;
                        if (data.HasGetsu == true)
                            this.Unit = 1;
                        this.MaxUnit = 1;
                        break;
                    case 2303:
                        this.Unit = 0;
                        if (data.HasSetsu == true)
                            this.Unit = 1;
                        this.MaxUnit = 1;
                        break;
                    case 2304:
                        this.Unit = 0;
                        if (data.HasKa == true)
                            this.Unit = 1;
                        this.MaxUnit = 1;
                        break;
                }

                data.Gauge = this.Unit;
                data.MaxGauge = this.MaxUnit;
                if (data.MaxGauge > 1000)
                {
                    data.Gauge /= 1000;
                    data.MaxGauge /= 1000;
                }

                data.DutyAction1 = data.GetDutyActionData(0);
                data.DutyAction2 = data.GetDutyActionData(1);
            }

            return preview ||
                (!this.Hp || Utils.GetResult(data.Hp, this.HpOp, this.MaxHp ? data.MaxHp : this.HpValue)) &&
                (!this.Mp || Utils.GetResult(data.Mp, this.MpOp, this.MaxMp ? data.MaxMp : this.MpValue)) &&
                (!this.Cp || Utils.GetResult(data.Cp, this.CpOp, this.MaxCp ? data.MaxCp : this.CpValue)) &&
                (!this.Gp || Utils.GetResult(data.Gp, this.GpOp, this.MaxGp ? data.MaxGp : this.GpValue)) &&
                (!this.Gauge || Utils.GetResult(data.Gauge, this.GaugeOp, this.MaxGauge ? data.MaxGauge : this.GaugeValue)) &&
                (!this.Level || Utils.GetResult(data.Level, this.LevelOp, this.LevelValue)) &&
                (!this.PetCheck || (this.PetValue == 0 ? data.HasPet : !data.HasPet));
        }

        public override void DrawTriggerOptions(Vector2 size, float padX, float padY)
        {
            ImGui.Combo("Trigger Source", ref Unsafe.As<TriggerSource, int>(ref this.TriggerSource), _sourceOptions, _sourceOptions.Length);
            DrawHelpers.DrawSpacing(1);

            ImGui.TextUnformatted("Trigger Conditions");
            string[] operatorOptions = TriggerOptions.OperatorOptions;
            float optionsWidth = 100 + padX;
            float opComboWidth = 55;
            float valueInputWidth = 45;
            float padWidth = 0;


            DrawHelpers.DrawNestIndicator(1);
            ImGui.Checkbox("Level", ref this.Level);
            if (this.Level)
            {
                ImGui.SameLine();
                padWidth = ImGui.CalcItemWidth() - ImGui.GetCursorPosX() - optionsWidth + padX;
                ImGui.SetCursorPosX(ImGui.GetCursorPosX() + padWidth);
                ImGui.PushItemWidth(opComboWidth);
                ImGui.Combo("##LevelOpCombo", ref Unsafe.As<TriggerDataOp, int>(ref this.LevelOp), operatorOptions, operatorOptions.Length);
                ImGui.PopItemWidth();
                ImGui.SameLine();

                if (string.IsNullOrEmpty(_levelValueInput))
                {
                    _levelValueInput = this.LevelValue.ToString();
                }

                ImGui.PushItemWidth(valueInputWidth);
                if (ImGui.InputText("##LevelValue", ref _levelValueInput, 10, ImGuiInputTextFlags.CharsDecimal))
                {
                    if (float.TryParse(_levelValueInput, out float value))
                    {
                        this.LevelValue = value;
                    }

                    _levelValueInput = this.LevelValue.ToString();
                }

                ImGui.PopItemWidth();
            }

            DrawHelpers.DrawNestIndicator(1);
            ImGui.Checkbox("HP", ref this.Hp);
            if (this.Hp)
            {
                ImGui.SameLine();
                padWidth = ImGui.CalcItemWidth() - ImGui.GetCursorPosX() - optionsWidth + padX;
                ImGui.SetCursorPosX(ImGui.GetCursorPosX() + padWidth);
                ImGui.PushItemWidth(opComboWidth);
                ImGui.Combo("##HpOpCombo", ref Unsafe.As<TriggerDataOp, int>(ref this.HpOp), operatorOptions, operatorOptions.Length);
                ImGui.PopItemWidth();
                ImGui.SameLine();

                if (string.IsNullOrEmpty(_hpValueInput))
                {
                    _hpValueInput = this.HpValue.ToString();
                }

                if (!this.MaxHp)
                {
                    ImGui.PushItemWidth(valueInputWidth);
                    if (ImGui.InputText("##HpValue", ref _hpValueInput, 10, ImGuiInputTextFlags.CharsDecimal))
                    {
                        if (float.TryParse(_hpValueInput, out float value))
                        {
                            this.HpValue = value;
                        }

                        _hpValueInput = this.HpValue.ToString();
                    }

                    ImGui.PopItemWidth();
                    ImGui.SameLine();
                }

                ImGui.Checkbox("Max HP", ref this.MaxHp);
            }

            DrawHelpers.DrawNestIndicator(1);
            ImGui.Checkbox("MP", ref this.Mp);
            if (this.Mp)
            {
                ImGui.SameLine();
                padWidth = ImGui.CalcItemWidth() - ImGui.GetCursorPosX() - optionsWidth + padX;
                ImGui.SetCursorPosX(ImGui.GetCursorPosX() + padWidth);
                ImGui.PushItemWidth(opComboWidth);
                ImGui.Combo("##MpOpCombo", ref Unsafe.As<TriggerDataOp, int>(ref this.MpOp), operatorOptions, operatorOptions.Length);
                ImGui.PopItemWidth();
                ImGui.SameLine();

                if (string.IsNullOrEmpty(_mpValueInput))
                {
                    _mpValueInput = this.MpValue.ToString();
                }

                if (!this.MaxMp)
                {
                    ImGui.PushItemWidth(valueInputWidth);
                    if (ImGui.InputText("##MpValue", ref _mpValueInput, 10, ImGuiInputTextFlags.CharsDecimal))
                    {
                        if (float.TryParse(_mpValueInput, out float value))
                        {
                            this.MpValue = value;
                        }

                        _mpValueInput = this.MpValue.ToString();
                    }

                    ImGui.PopItemWidth();
                    ImGui.SameLine();
                }

                ImGui.Checkbox("Max MP", ref this.MaxMp);
            }

            DrawHelpers.DrawNestIndicator(1);
            ImGui.Checkbox("CP", ref this.Cp);
            if (this.Cp)
            {
                ImGui.SameLine();
                padWidth = ImGui.CalcItemWidth() - ImGui.GetCursorPosX() - optionsWidth + padX;
                ImGui.SetCursorPosX(ImGui.GetCursorPosX() + padWidth);
                ImGui.PushItemWidth(opComboWidth);
                ImGui.Combo("##CpOpCombo", ref Unsafe.As<TriggerDataOp, int>(ref this.CpOp), operatorOptions, operatorOptions.Length);
                ImGui.PopItemWidth();
                ImGui.SameLine();

                if (string.IsNullOrEmpty(_cpValueInput))
                {
                    _cpValueInput = this.CpValue.ToString();
                }

                if (!this.MaxCp)
                {
                    ImGui.PushItemWidth(valueInputWidth);
                    if (ImGui.InputText("##CpValue", ref _cpValueInput, 10, ImGuiInputTextFlags.CharsDecimal))
                    {
                        if (float.TryParse(_cpValueInput, out float value))
                        {
                            this.CpValue = value;
                        }

                        _cpValueInput = this.CpValue.ToString();
                    }

                    ImGui.PopItemWidth();
                    ImGui.SameLine();
                }

                ImGui.Checkbox("Max CP", ref this.MaxCp);
            }

            DrawHelpers.DrawNestIndicator(1);
            ImGui.Checkbox("GP", ref this.Gp);
            if (this.Gp)
            {
                ImGui.SameLine();
                padWidth = ImGui.CalcItemWidth() - ImGui.GetCursorPosX() - optionsWidth + padX;
                ImGui.SetCursorPosX(ImGui.GetCursorPosX() + padWidth);
                ImGui.PushItemWidth(opComboWidth);
                ImGui.Combo("##GpOpCombo", ref Unsafe.As<TriggerDataOp, int>(ref this.GpOp), operatorOptions, operatorOptions.Length);
                ImGui.PopItemWidth();
                ImGui.SameLine();

                if (string.IsNullOrEmpty(_gpValueInput))
                {
                    _gpValueInput = this.GpValue.ToString();
                }

                if (!this.MaxGp)
                {
                    ImGui.PushItemWidth(valueInputWidth);
                    if (ImGui.InputText("##GpValue", ref _gpValueInput, 10, ImGuiInputTextFlags.CharsDecimal))
                    {
                        if (float.TryParse(_gpValueInput, out float value))
                        {
                            this.GpValue = value;
                        }

                        _gpValueInput = this.GpValue.ToString();
                    }

                    ImGui.PopItemWidth();
                    ImGui.SameLine();
                }

                ImGui.Checkbox("Max GP", ref this.MaxGp);
            }

            if (this.TriggerSource == TriggerSource.Player)
            {
                DrawHelpers.DrawNestIndicator(1);
                ImGui.Checkbox("Pet", ref this.PetCheck);
                if (this.PetCheck)
                {
                    ImGui.SameLine();
                    padWidth = ImGui.CalcItemWidth() - ImGui.GetCursorPosX() - optionsWidth + padX;
                    ImGui.SetCursorPosX(ImGui.GetCursorPosX() + padWidth);
                    ImGui.PushItemWidth(optionsWidth);
                    ImGui.Combo("##PetCombo", ref this.PetValue, _petOptions, _petOptions.Length);
                    ImGui.PopItemWidth();
                }
            }

            if (this.TriggerSource == TriggerSource.Player)
            {
                DrawHelpers.DrawNestIndicator(1);
                ImGui.Checkbox("Gauge", ref this.Gauge);
            }
            else { this.Gauge = false; }
            if (this.Gauge)
            {
                ImGui.Combo("##JobCombo", ref this.JobValue, _jobOptions, _jobOptions.Length);
                // These are just spacers in the menu, skip to next option if selected.
                if (this.JobValue == 0) { this.JobValue = 1; }
                if (this.JobValue == 5) { this.JobValue = 6; }
                if (this.JobValue == 10) { this.JobValue = 11; }
                if (this.JobValue == 14) { this.JobValue = 15; }
                if (this.JobValue == 18) { this.JobValue = 19; }

                // --Tanks--
                if (this.JobValue == 1) // DRK
                {
                    ImGui.RadioButton("Blood Gauge", ref this.UnitOption, 100);
                    ImGui.SameLine();
                    ImGui.RadioButton("Darkside Timer", ref this.UnitOption, 101);
                    ImGui.SameLine();
                    ImGui.RadioButton("Shadow Timer", ref this.UnitOption, 102);
                    ImGui.SameLine();
                    ImGui.RadioButton("Dark Arts Active", ref this.UnitOption, 103);
                }
                if (this.JobValue == 2) // GNB
                {
                    ImGui.RadioButton("Cartridges", ref this.UnitOption, 200);
                }
                if (this.JobValue == 3) // PLD
                {
                    ImGui.RadioButton("Oath Gauge", ref this.UnitOption, 300);
                }
                if (this.JobValue == 4) // WAR
                {
                    ImGui.RadioButton("Beast Gauge", ref this.UnitOption, 400);
                }

                //--Healers--
                if (this.JobValue == 6) // AST
                {
                    ImGui.RadioButton("1st Seal", ref this.UnitOption, 600);
                    ImGui.SameLine();
                    ImGui.RadioButton("2nd Seal", ref this.UnitOption, 601);
                    ImGui.SameLine();
                    ImGui.RadioButton("3rd Seal", ref this.UnitOption, 602);

                    ImGui.TextUnformatted("Card Info: 1 = Solar, 2 = Lunar, 3 = Celestial");
                }

                if (this.JobValue == 7) // SCH
                {
                    ImGui.RadioButton("Aetherflow", ref this.UnitOption, 700);
                    ImGui.SameLine();
                    ImGui.RadioButton("Fairy Gauge", ref this.UnitOption, 701);
                    ImGui.SameLine();
                    ImGui.RadioButton("Seraph Timer", ref this.UnitOption, 702);
                }

                if (this.JobValue == 8) // SGE
                {
                    ImGui.RadioButton("Eukrasia", ref this.UnitOption, 800);
                    ImGui.SameLine();
                    ImGui.RadioButton("Addersgall Timer", ref this.UnitOption, 801);
                    ImGui.SameLine();
                    ImGui.RadioButton("Addersgall Stacks", ref this.UnitOption, 802);
                    ImGui.SameLine();
                    ImGui.RadioButton("Addersting Stacks", ref this.UnitOption, 803);
                }

                if (this.JobValue == 9) // WHM
                {
                    ImGui.RadioButton("Lily Timer", ref this.UnitOption, 900);
                    ImGui.SameLine();
                    ImGui.RadioButton("Lily Stacks", ref this.UnitOption, 901);
                    ImGui.SameLine();
                    ImGui.RadioButton("Blood Lily Stacks", ref this.UnitOption, 902);
                }

                //--Casters--
                if (this.JobValue == 11) // BLM
                {
                    ImGui.RadioButton("Element Timer", ref this.UnitOption, 1100);
                    ImGui.SameLine();
                    ImGui.RadioButton("In Umbral Ice", ref this.UnitOption, 1101);
                    ImGui.SameLine();
                    ImGui.RadioButton("In Astral Fire", ref this.UnitOption, 1102);

                    ImGui.RadioButton("Umbral Heart Stacks", ref this.UnitOption, 1103);
                    ImGui.SameLine();
                    ImGui.RadioButton("Umbral Ice Stacks", ref this.UnitOption, 1104);
                    ImGui.SameLine();
                    ImGui.RadioButton("Astral Fire Stacks", ref this.UnitOption, 1105);

                    ImGui.RadioButton("Enochian Timer", ref this.UnitOption, 1106);
                    ImGui.SameLine();
                    ImGui.RadioButton("Polyglot Stacks", ref this.UnitOption, 1107);
                    ImGui.SameLine();
                    ImGui.RadioButton("Enochian Active", ref this.UnitOption, 1108);
                    ImGui.SameLine();
                    ImGui.RadioButton("Paradox Active", ref this.UnitOption, 1109);
                }

                if (this.JobValue == 12) // RDM
                {
                    ImGui.RadioButton("White Mana", ref this.UnitOption, 1200);
                    ImGui.SameLine();
                    ImGui.RadioButton("Black Mana", ref this.UnitOption, 1201);
                    ImGui.SameLine();
                    ImGui.RadioButton("Mana Stacks", ref this.UnitOption, 1202);

                }

                if (this.JobValue == 13) // SMN
                {
                    ImGui.RadioButton("Aetherflow", ref this.UnitOption, 1300);
                    ImGui.SameLine();
                    ImGui.RadioButton("Summon Timer", ref this.UnitOption, 1301);
                    ImGui.SameLine();
                    ImGui.RadioButton("Attunement Timer", ref this.UnitOption, 1302);

                    ImGui.RadioButton("Ifrit Ready", ref this.UnitOption, 1303);
                    ImGui.SameLine();
                    ImGui.RadioButton("Garuda Ready", ref this.UnitOption, 1304);
                    ImGui.SameLine();
                    ImGui.RadioButton("Titan Ready", ref this.UnitOption, 1305);

                    ImGui.RadioButton("Ifrit Attuned", ref this.UnitOption, 1306);
                    ImGui.SameLine();
                    ImGui.RadioButton("Garuda Attuned", ref this.UnitOption, 1307);
                    ImGui.SameLine();
                    ImGui.RadioButton("Titan Attuned", ref this.UnitOption, 1308);

                    ImGui.RadioButton("Bahamut Ready", ref this.UnitOption, 1309);
                    ImGui.SameLine();
                    ImGui.RadioButton("Phoenix Ready", ref this.UnitOption, 1310);
                }

                //--Ranged--
                if (this.JobValue == 15) // BRD
                {
                    ImGui.RadioButton("Song Timer", ref this.UnitOption, 1500);
                    ImGui.SameLine();
                    ImGui.RadioButton("Repertoire", ref this.UnitOption, 1501);
                    ImGui.SameLine();
                    ImGui.RadioButton("Soul Voice", ref this.UnitOption, 1502);

                    ImGui.RadioButton("Song", ref this.UnitOption, 1503);
                    ImGui.SameLine();
                    ImGui.RadioButton("Coda 1", ref this.UnitOption, 1504);
                    ImGui.SameLine();
                    ImGui.RadioButton("Coda 2", ref this.UnitOption, 1505);
                    ImGui.SameLine();
                    ImGui.RadioButton("Coda 3", ref this.UnitOption, 1506);

                    ImGui.TextUnformatted("Song Info: 1 = Wanderer, 2 = Mage, 3 = Army");
                }
                if (this.JobValue == 16) // DNC
                {
                    ImGui.RadioButton("Feathers", ref this.UnitOption, 1600);
                    ImGui.SameLine();
                    ImGui.RadioButton("Esprit", ref this.UnitOption, 1601);
                    ImGui.SameLine();
                    ImGui.RadioButton("Completed Steps", ref this.UnitOption, 1607);
                    ImGui.SameLine();
                    ImGui.RadioButton("Next Step", ref this.UnitOption, 1602);

                    ImGui.TextUnformatted("Steps:");
                    ImGui.RadioButton("1st", ref this.UnitOption, 1603);
                    ImGui.SameLine();
                    ImGui.RadioButton("2nd", ref this.UnitOption, 1604);
                    ImGui.SameLine();
                    ImGui.RadioButton("3rd", ref this.UnitOption, 1605);
                    ImGui.SameLine();
                    ImGui.RadioButton("4th", ref this.UnitOption, 1606);

                    ImGui.TextUnformatted("Step Info: 1 = Emboite, 2 = Entrechat, 3 = Jete, 4 = Pirouette");
                }
                if (this.JobValue == 17) // MCH
                {
                    ImGui.RadioButton("Heat Gauge", ref this.UnitOption, 1700);
                    ImGui.SameLine();
                    ImGui.RadioButton("Is Overheated", ref this.UnitOption, 1701);
                    ImGui.SameLine();
                    ImGui.RadioButton("Overheat Timer", ref this.UnitOption, 1702);

                    ImGui.RadioButton("Battery Gauge", ref this.UnitOption, 1703);
                    ImGui.SameLine();
                    ImGui.RadioButton("Robot Battery", ref this.UnitOption, 1704);
                    ImGui.SameLine();
                    ImGui.RadioButton("Robot Timer", ref this.UnitOption, 1705);
                    ImGui.SameLine();
                    ImGui.RadioButton("Robot Active", ref this.UnitOption, 1706);
                }
                //--Melee--
                if (this.JobValue == 19) // DRG
                {
                    ImGui.RadioButton("Life of the Dragon Timer", ref this.UnitOption, 1900);
                    ImGui.SameLine();
                    ImGui.RadioButton("Life of the Dragon Active", ref this.UnitOption, 1901);

                    ImGui.RadioButton("First Brood's Gaze", ref this.UnitOption, 1902);
                    ImGui.SameLine();
                    ImGui.RadioButton("Firstminds' Focus", ref this.UnitOption, 1903);
                }
                if (this.JobValue == 20) // MNK
                {
                    ImGui.RadioButton("Chakra Gauge", ref this.UnitOption, 2000);
                    ImGui.SameLine();
                    ImGui.RadioButton("Nadi", ref this.UnitOption, 2001);

                    ImGui.RadioButton("Beast Chakra 1", ref this.UnitOption, 2002);
                    ImGui.SameLine();
                    ImGui.RadioButton("Beast Chakra 2", ref this.UnitOption, 2003);
                    ImGui.SameLine();
                    ImGui.RadioButton("Beast Chakra 3", ref this.UnitOption, 2004);

                    ImGui.TextUnformatted("Nadi Info: 1 = Solar, 2 = Lunar");

                    ImGui.TextUnformatted("Chakra Info: 1 = Coeurl, 2 = Opo-opo, 3 = Raptor");
                }
                if (this.JobValue == 21) // NIN
                {
                    ImGui.RadioButton("Ninki Gauge", ref this.UnitOption, 2100);
                    ImGui.SameLine();
                    ImGui.RadioButton("Huton Timer", ref this.UnitOption, 2101);
                }
                if (this.JobValue == 22) // RPR
                {
                    ImGui.RadioButton("Soul Gauge", ref this.UnitOption, 2200);
                    ImGui.SameLine();
                    ImGui.RadioButton("Enshroud Timer", ref this.UnitOption, 2201);

                    ImGui.RadioButton("Shroud Gauge", ref this.UnitOption, 2202);
                    ImGui.SameLine();
                    ImGui.RadioButton("Lemure Shroud Gauge", ref this.UnitOption, 2203);
                    ImGui.SameLine();
                    ImGui.RadioButton("Void Shroud Gauge", ref this.UnitOption, 2204);
                }
                if (this.JobValue == 23) // SAM
                {
                    ImGui.RadioButton("Kenki Gauge", ref this.UnitOption, 2300);
                    ImGui.SameLine();
                    ImGui.RadioButton("Meditation Stacks", ref this.UnitOption, 2301);
                    ImGui.SameLine();
                    ImGui.RadioButton("Has Getsu", ref this.UnitOption, 2302);
                    ImGui.SameLine();
                    ImGui.RadioButton("Has Setsu", ref this.UnitOption, 2303);
                    ImGui.SameLine();
                    ImGui.RadioButton("Has Ka", ref this.UnitOption, 2304);
                }
                ImGui.TextUnformatted($"Current Gauge value: {this.Unit}");
                ImGui.SameLine();
                padWidth = ImGui.CalcItemWidth() - ImGui.GetCursorPosX() - optionsWidth + padX;
                ImGui.SetCursorPosX(ImGui.GetCursorPosX() + padWidth);
                ImGui.PushItemWidth(opComboWidth);
                ImGui.Combo("##GaugeCombo", ref Unsafe.As<TriggerDataOp, int>(ref this.GaugeOp), operatorOptions, operatorOptions.Length);
                ImGui.PopItemWidth();
                ImGui.SameLine();

                if (string.IsNullOrEmpty(_gaugeValueInput))
                {
                    _gaugeValueInput = this.GaugeValue.ToString();
                }

                if (!this.MaxGauge)
                {
                    ImGui.PushItemWidth(valueInputWidth);
                    if (ImGui.InputText("##GaugeValue", ref _gaugeValueInput, 10, ImGuiInputTextFlags.CharsDecimal))
                    {
                        if (float.TryParse(_gaugeValueInput, out float value))
                        {
                            this.GaugeValue = value;
                        }

                        _gaugeValueInput = this.GaugeValue.ToString();
                    }

                    ImGui.PopItemWidth();
                    ImGui.SameLine();
                }

                ImGui.Checkbox("Max Gauge", ref this.MaxGauge);
            }

        }
    }
}