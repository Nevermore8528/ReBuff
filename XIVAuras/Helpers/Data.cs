using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using Dalamud.Game.ClientState.JobGauge.Enums;
using Dalamud.Game.ClientState.JobGauge.Types;
using Dalamud.Game.ClientState.Objects.SubKinds;
using FFXIVClientStructs.FFXIV.Client.Game.Gauge;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using Newtonsoft.Json;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace XIVAuras.Helpers
{
    [StructLayout(LayoutKind.Explicit, Size = 0x8)]
    public struct Combo {
        [FieldOffset(0x00)] public float Timer;
        [FieldOffset(0x04)] public uint Action;
    }

    public class TriggerData
    {
        public string Name;
        public uint Id;
        public uint Icon;
        public byte MaxStacks;
        public uint[] ComboId;
        public CombatType CombatType;

        public TriggerData(string name, uint id, uint icon, byte maxStacks = 0, uint[]? comboId = null, CombatType combatType = CombatType.PvE)
        {
            Name = name;
            Id = id;
            Icon = icon;
            MaxStacks = maxStacks;
            ComboId = comboId ?? new uint[0];
            CombatType = combatType;
        }
    }

    public struct RecastInfo
    {
        public float RecastTime;
        public float RecastTimeElapsed;
        public ushort MaxCharges;

        public RecastInfo(float recastTime, float recastTimeElapsed, ushort maxCharges)
        {
            RecastTime = recastTime;
            RecastTimeElapsed = recastTimeElapsed;
            MaxCharges = maxCharges;
        }
    }

    public class DataSource
    {
        [JsonIgnore]
        private static readonly Dictionary<string, FieldInfo> _fields =
            typeof(DataSource).GetFields().ToDictionary((x) => x.Name.ToLower());

        [JsonIgnore] public static readonly string[] TextTags = new string[]
        {
            "Text tags available for Cooldown/Status triggers:",
            "[value]    =>  The value tracked by the trigger, represents cooldown/duration.",
            "[maxvalue]",
            "[stacks]",
            "[maxstacks]",
            "",
            "Text tags available for CharacterState triggers:",
            "[name]",
            "[name_first]",
            "[name_last]",
            "[job]",
            "[jobname]",
            "[hp]",
            "[maxhp]",
            "[mp]",
            "[maxmp]",
            "[gp]",
            "[maxgp]",
            "[cp]",
            "[maxcp]",
        };

        public string GetFormattedString(string format, string numberFormat, int rounding)
        {
            return TextTagFormatter.TextTagRegex.Replace(
                format,
                new TextTagFormatter(this, numberFormat, rounding, _fields).Evaluate);
        }

        public DataSource()
        {
            this.Name_First = new LazyString<string?>(() => this.Name, LazyStringConverters.FirstName);
            this.Name_Last = new LazyString<string?>(() => this.Name, LazyStringConverters.LastName);
            this.JobName = new LazyString<Job>(() => this.Job, LazyStringConverters.JobName);
        }

        public uint? GetDutyActionData(ushort a)
        {
            ActionHelpers helper = Singletons.Get<ActionHelpers>();
            return helper.GetDutyAction(a);
        }

        public float GetDataForSourceType(TriggerDataSource sourcetype) => sourcetype switch
        {
            TriggerDataSource.Value => this.Value,
            TriggerDataSource.Stacks => this.Stacks,
            TriggerDataSource.MaxStacks => this.MaxStacks,
            TriggerDataSource.HP => this.Hp,
            TriggerDataSource.MP => this.Mp,
            TriggerDataSource.CP => this.Cp,
            TriggerDataSource.GP => this.Gp,
            TriggerDataSource.Level => this.Level,
            TriggerDataSource.Distance => this.Distance,
            _ => 0
        };

        public uint Id;
        public float Value;
        public float MaxValue;
        public int Stacks;
        public int MaxStacks;
        public uint Icon;

        public string Name = string.Empty;
        public LazyString<string?>? Name_First;
        public LazyString<string?>? Name_Last;
        public Job Job;
        public LazyString<Job>? JobName;

        public uint Level;
        public float Hp;
        public float MaxHp;
        public float Mp;
        public float MaxMp;
        public float Gp;
        public float MaxGp;
        public float Cp;
        public float MaxCp;
        public bool HasPet;
        public float Distance;

        // JobGauge Data

        // PLD
        public float OathGauge = PluginManager.JobGauges.Get<PLDGauge>().OathGauge;
        // War
        public float BeastGauge = PluginManager.JobGauges.Get<WARGauge>().BeastGauge;
        // DRK
        public float Blood = PluginManager.JobGauges.Get<DRKGauge>().Blood;
        public float DarksideTimer_long = PluginManager.JobGauges.Get<DRKGauge>().DarksideTimeRemaining;
        public float DarksideTimer = PluginManager.JobGauges.Get<DRKGauge>().DarksideTimeRemaining/1000;
        public float ShadowTimer_long = PluginManager.JobGauges.Get<DRKGauge>().ShadowTimeRemaining;
        public float ShadowTimer = PluginManager.JobGauges.Get<DRKGauge>().ShadowTimeRemaining/1000;
        public bool DarkArtsState = PluginManager.JobGauges.Get<DRKGauge>().HasDarkArts;
        // GNB
        public float Cartridges = PluginManager.JobGauges.Get<GNBGauge>().Ammo;
        public float MaxTimerDuration = PluginManager.JobGauges.Get<GNBGauge>().MaxTimerDuration;
        public float ComboStep = PluginManager.JobGauges.Get<GNBGauge>().AmmoComboStep;

        // BLM
        public float EnochianTimer = PluginManager.JobGauges.Get<BLMGauge>().EnochianTimer;
        public float ElementTimer = PluginManager.JobGauges.Get<BLMGauge>().ElementTimeRemaining;
        public float ElementTimer_short = PluginManager.JobGauges.Get<BLMGauge>().ElementTimeRemaining / 1000;
        public float PolyglotStacks = PluginManager.JobGauges.Get<BLMGauge>().PolyglotStacks;
        public float UmbralHearts = PluginManager.JobGauges.Get<BLMGauge>().UmbralHearts;
        public float UmbralIceStacks = PluginManager.JobGauges.Get<BLMGauge>().UmbralIceStacks;
        public float AstralFireStacks = PluginManager.JobGauges.Get<BLMGauge>().AstralFireStacks;
        public bool InUmbralIce = PluginManager.JobGauges.Get<BLMGauge>().InUmbralIce;
        public bool InAstralFire = PluginManager.JobGauges.Get<BLMGauge>().InAstralFire;
        public bool IsEnochianActive = PluginManager.JobGauges.Get<BLMGauge>().IsEnochianActive;
        public bool IsParadoxActive = PluginManager.JobGauges.Get<BLMGauge>().IsParadoxActive;
        // SMN
        public float SummonTimer = PluginManager.JobGauges.Get<SMNGauge>().SummonTimerRemaining;
        public float AttunmentTimer = PluginManager.JobGauges.Get<SMNGauge>().AttunmentTimerRemaining;
        public bool IsBahamutReady = PluginManager.JobGauges.Get<SMNGauge>().IsBahamutReady;
        public bool IsPhoenixReady = PluginManager.JobGauges.Get<SMNGauge>().IsPhoenixReady;
        public bool IsIfritReady = PluginManager.JobGauges.Get<SMNGauge>().IsIfritReady;
        public bool IsGarudaReady = PluginManager.JobGauges.Get<SMNGauge>().IsGarudaReady;
        public bool IsTitanReady = PluginManager.JobGauges.Get<SMNGauge>().IsTitanReady;
        public bool IsIfritAttuned = PluginManager.JobGauges.Get<SMNGauge>().IsIfritAttuned;
        public bool IsGarudaAttuned = PluginManager.JobGauges.Get<SMNGauge>().IsGarudaAttuned;
        public bool IsTitanAttuned = PluginManager.JobGauges.Get<SMNGauge>().IsTitanAttuned;
        public float SMNAetherflow = PluginManager.JobGauges.Get<SMNGauge>().AetherflowStacks;
        // RDM
        public float WhiteMana = PluginManager.JobGauges.Get<RDMGauge>().WhiteMana;
        public float BlackMana = PluginManager.JobGauges.Get<RDMGauge>().BlackMana;
        public float ManaStacks = PluginManager.JobGauges.Get<RDMGauge>().ManaStacks;
        // BLU
        // lol no

        // WHM
        public float LilyTimer = PluginManager.JobGauges.Get<WHMGauge>().LilyTimer;
        public float Lilies = PluginManager.JobGauges.Get<WHMGauge>().Lily;
        public float BloodLilies = PluginManager.JobGauges.Get<WHMGauge>().BloodLily;
        // AST
        public CardType DrawnCard = PluginManager.JobGauges.Get<ASTGauge>().DrawnCard;
        public CardType DrawnCrownCard = PluginManager.JobGauges.Get<ASTGauge>().DrawnCrownCard;
        public SealType[] GetSeals = PluginManager.JobGauges.Get<ASTGauge>().Seals;
        // if (data.GetSeals[array] == SealType.TYPE) { //Do something; }
        public string? ASTSeal1;
        public string? ASTSeal2;
        public string? ASTSeal3;
        public bool HasSolarSeal = PluginManager.JobGauges.Get<ASTGauge>().ContainsSeal(SealType.SUN);
        public bool HasLunarSeal = PluginManager.JobGauges.Get<ASTGauge>().ContainsSeal(SealType.MOON);
        public bool HasCelestialSeal = PluginManager.JobGauges.Get<ASTGauge>().ContainsSeal(SealType.CELESTIAL);
        // SCH
        public float SCHAetherflow = PluginManager.JobGauges.Get<SCHGauge>().Aetherflow;
        public float FairyGauge = PluginManager.JobGauges.Get<SCHGauge>().FairyGauge;
        public float SeraphTimer = PluginManager.JobGauges.Get<SCHGauge>().SeraphTimer;
        // SGE
        public bool Eukrasia = PluginManager.JobGauges.Get<SGEGauge>().Eukrasia;
        public float AddersgallTimer = PluginManager.JobGauges.Get<SGEGauge>().AddersgallTimer;
        public float AddersgallStacks = PluginManager.JobGauges.Get<SGEGauge>().Addersgall;
        public float AdderstingStacks = PluginManager.JobGauges.Get<SGEGauge>().Addersting;

        //DNC
        public float Feathers = PluginManager.JobGauges.Get<DNCGauge>().Feathers;
        public float Esprit = PluginManager.JobGauges.Get<DNCGauge>().Esprit;
        public uint[] Steps = PluginManager.JobGauges.Get<DNCGauge>().Steps;
        //if (data.Steps[array] == DanceStep.Step) { //Do Something; }
        public uint NextStepID = PluginManager.JobGauges.Get<DNCGauge>().NextStep;
        public string? Step1;
        public string? Step2;
        public string? Step3;
        public string? Step4;
        public string? NextStep;
        //BRD
        public float SongTimer = PluginManager.JobGauges.Get<BRDGauge>().SongTimer;
        public float Repertoire = PluginManager.JobGauges.Get<BRDGauge>().Repertoire;
        public float SoulVoice = PluginManager.JobGauges.Get<BRDGauge>().SoulVoice;
        public Song Song = PluginManager.JobGauges.Get<BRDGauge>().Song;
        public string? CurrentSong;
        public string? Coda1;
        public string? Coda2;
        public string? Coda3;
        public Song LastSong = PluginManager.JobGauges.Get<BRDGauge>().LastSong;
        public Song[] Coda = PluginManager.JobGauges.Get<BRDGauge>().Coda;
        //MCH
        public float OverheatTimer = PluginManager.JobGauges.Get<MCHGauge>().OverheatTimeRemaining;
        public float RookQueenTimer = PluginManager.JobGauges.Get<MCHGauge>().SummonTimeRemaining;
        public float Heat = PluginManager.JobGauges.Get<MCHGauge>().Heat;
        public float Battery = PluginManager.JobGauges.Get<MCHGauge>().Battery;
        public float LastSummonBatteryPower = PluginManager.JobGauges.Get<MCHGauge>().LastSummonBatteryPower;
        public bool IsOverheated = PluginManager.JobGauges.Get<MCHGauge>().IsOverheated;
        public bool IsRobotActive = PluginManager.JobGauges.Get<MCHGauge>().IsRobotActive;

        //MNK
        public float Chakra = PluginManager.JobGauges.Get<MNKGauge>().Chakra;
        public BeastChakra[] BeastChakra = PluginManager.JobGauges.Get<MNKGauge>().BeastChakra;
        public Nadi AvaliableNadi = PluginManager.JobGauges.Get<MNKGauge>().Nadi;
        public float BlitzTimer = PluginManager.JobGauges.Get<MNKGauge>().BlitzTimeRemaining;
        public string? BeastChakra1;
        public string? BeastChakra2;
        public string? BeastChakra3;
        public string? Nadi;
        //SAM
        public Kaeshi Kaeshi = PluginManager.JobGauges.Get<SAMGauge>().Kaeshi;
        public float Kenki = PluginManager.JobGauges.Get<SAMGauge>().Kenki;
        public float MeditationStacks = PluginManager.JobGauges.Get<SAMGauge>().MeditationStacks;
        public Sen Sen = PluginManager.JobGauges.Get<SAMGauge>().Sen;
        public bool HasGetsu = PluginManager.JobGauges.Get<SAMGauge>().HasGetsu;
        public bool HasSetsu = PluginManager.JobGauges.Get<SAMGauge>().HasSetsu;
        public bool HasKa = PluginManager.JobGauges.Get<SAMGauge>().HasKa;
        //RPR
        public float Soul = PluginManager.JobGauges.Get<RPRGauge>().Soul;
        public float Shroud = PluginManager.JobGauges.Get<RPRGauge>().Shroud;
        public float EnshroudTimer = PluginManager.JobGauges.Get<RPRGauge>().EnshroudedTimeRemaining;
        public float LemureShroud = PluginManager.JobGauges.Get<RPRGauge>().LemureShroud;
        public float VoidShroud = PluginManager.JobGauges.Get<RPRGauge>().VoidShroud;
        //DRG
        public float LoTDTimer = PluginManager.JobGauges.Get<DRGGauge>().LOTDTimer;
        public bool IsLoTDActive = PluginManager.JobGauges.Get<DRGGauge>().IsLOTDActive;
        public float EyeCount = PluginManager.JobGauges.Get<DRGGauge>().EyeCount;
        public float FMFocus = PluginManager.JobGauges.Get<DRGGauge>().FirstmindsFocusCount;
        //NIN
        public float HutonTimer = PluginManager.JobGauges.Get<NINGauge>().HutonTimer;
        public float Ninki = PluginManager.JobGauges.Get<NINGauge>().Ninki;
        public float HutonCount = PluginManager.JobGauges.Get<NINGauge>().HutonManualCasts;

        public float Gauge;
        public float MaxGauge;
        public float CurrentLevel = CharacterState.GetCharacterLevel();

        // Duty Actions
        public uint? DutyAction1;
        public uint? DutyAction2;
    }
}
