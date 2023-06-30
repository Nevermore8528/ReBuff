using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using ImGuiNET;
using XIVAuras.Auras;
using XIVAuras.Config;
using XIVAuras.Helpers;
using XIVAuras;
using Dalamud.Game.ClientState.JobGauge.Enums;
using FFXIVClientStructs.FFXIV.Client.Game.Gauge;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Runtime.InteropServices.Marshalling;

namespace XIVAuras.Auras
{
    public class AuraBar : AuraListItem
    {
        public override AuraType Type => AuraType.Bar;

        public BarStyleConfig BarStyleConfig { get; set; }
        public LabelListConfig LabelListConfig { get; set; }
        public TriggerConfig TriggerConfig { get; set; }
        public StyleConditions<BarStyleConfig> StyleConditions { get; set; }
        public VisibilityConfig VisibilityConfig { get; set; }

        private static Vector2 StartPosition { get; set; }

        // Constuctor for deserialization
        public AuraBar() : this(string.Empty) { }

        public AuraBar(string name) : base(name)
        {
            this.Name = name;
            this.BarStyleConfig = new BarStyleConfig();
            this.LabelListConfig = new LabelListConfig();
            this.TriggerConfig = new TriggerConfig();
            this.StyleConditions = new StyleConditions<BarStyleConfig>();
            this.VisibilityConfig = new VisibilityConfig();
        }

        public override void ImportPage(IConfigPage page)
        {
            switch (page)
            {
                case BarStyleConfig newPage:
                    this.BarStyleConfig = newPage;
                    break;
                case LabelListConfig newPage:
                    this.LabelListConfig = newPage;
                    break;
                case TriggerConfig newPage:
                    this.TriggerConfig = newPage;
                    break;
                case StyleConditions<BarStyleConfig> newPage:
                    newPage.UpdateTriggerCount(0);
                    newPage.UpdateDefaultStyle(this.BarStyleConfig);
                    this.StyleConditions = newPage;
                    break;
                case VisibilityConfig newPage:
                    this.VisibilityConfig = newPage;
                    break;
            }
        }

        public override IEnumerable<IConfigPage> GetConfigPages()
        {
            yield return this.BarStyleConfig;
            yield return this.LabelListConfig;
            yield return this.TriggerConfig;

            // ugly hack
            this.StyleConditions.UpdateTriggerCount(this.TriggerConfig.TriggerOptions.Count);
            this.StyleConditions.UpdateDefaultStyle(this.BarStyleConfig);

            yield return this.StyleConditions;
            yield return this.VisibilityConfig;
        }

        public override void Draw(Vector2 pos, Vector2? parentSize = null, bool parentVisible = true)
        {
            if (!this.TriggerConfig.TriggerOptions.Any())
            {
                return;
            }

            bool visible = this.VisibilityConfig.IsVisible(parentVisible);
            if (!visible && !this.Preview)
            {
                return;
            }

            bool triggered = this.TriggerConfig.IsTriggered(this.Preview, out DataSource[] datas, out int triggeredIndex);
            DataSource data = datas[triggeredIndex];
            BarStyleConfig style = this.StyleConditions.GetStyle(datas, triggeredIndex) ?? this.BarStyleConfig;

            Vector2 localPos = pos + style.Position;
            Vector2 size = style.Size;
            Vector2 iconsize = style.IconSize;

            if (Singletons.Get<PluginManager>().ShouldClip())
            {
                ClipRect? clipRect = Singletons.Get<ClipRectsHelper>().GetClipRectForArea(localPos, size);
                if (clipRect.HasValue)
                    return;
            }

            if (triggered || this.Preview)
            {
                this.UpdateStartData(data);
                this.UpdateDragData(localPos, size);

                var IsVertical = false;
                if (size.Y > size.X && style.IconOption >= 11)
                {
                    IsVertical = true;
                }
                else if (size.Y <= size.X || style.IconOption < 11)
                {
                    IsVertical = false;
                }

                bool desaturate = style.DesaturateIcon;
                float alpha = style.Opacity;

                int stackCount = style.Stacks;
                if (stackCount <= 0) { stackCount = 0; }

                Vector2 IconAOEstart = new Vector2(localPos.X - iconsize.X, localPos.Y - iconsize.Y);
                Vector2 IconAOEend = new Vector2(size.X + iconsize.X, size.Y + iconsize.Y);
                Vector2 IconPos = Utils.GetAnchoredPosition(IconAOEstart + style.IconPosition, -IconAOEend, style.ParentAnchor);

                DrawHelpers.DrawInWindow($"##{this.ID}", localPos, size, this.Preview, this.SetPosition, (drawList) =>
                {
                    if (this.Preview)
                    {
                        data = this.UpdatePreviewData(data);
                        if (this.LastFrameWasDragging)
                        {
                            localPos = ImGui.GetWindowPos();
                            style.Position = localPos - pos;
                        }
                    }

                    if (style.IconOption >= 6)
                    {
                        switch (style.UnitOption)
                        {
                            // poll UnitOption to determine what kind of dynamic bar we're making
                            // currently limited to static max units. Considering options for cooldowns
                            case 0:
                                style.Unit = data.Hp;
                                style.MaxUnit = data.MaxHp;
                                break;
                            case 1:
                                style.Unit = data.Mp;
                                style.MaxUnit = data.MaxMp;
                                break;
                            case 2:
                                style.Unit = data.Gp;
                                style.MaxUnit = data.MaxGp;
                                break;
                            case 3:
                                style.Unit = data.Cp;
                                style.MaxUnit = data.MaxCp;
                                break;
                            case 4:
                                style.Unit = data.Stacks;
                                style.MaxUnit = data.MaxStacks;
                                break;
                            case 5:
                                style.Unit = data.Value;
                                style.MaxUnit = data.MaxValue;
                                // style.MaxUnit = style.MaxUnit; // Already set by user
                                break;

                                // Job Gauge Data

                                // DRK
                            case 100:
                                style.Unit = data.Blood;
                                style.MaxUnit = 100;
                                break;
                            case 101:
                                style.Unit = data.DarksideTimer_long;
                                style.MaxUnit = 60000;
                                break;
                            case 102:
                                style.Unit = data.ShadowTimer_long;
                                style.MaxUnit = 20000;
                                break;
                            case 103:
                                style.Unit = 0;
                                if (data.DarkArtsState)
                                    style.Unit = 1;
                                style.MaxUnit = 1;
                                break;

                            // GNB
                            case 200:
                                style.Unit = data.Cartridges;
                                style.MaxUnit = 3;
                                break;

                            // PLD
                            case 300:
                                style.Unit = data.OathGauge;
                                style.MaxUnit = 100;
                                break;

                            // WAR
                            case 400:
                                style.Unit = data.BeastGauge;
                                style.MaxUnit = 100;
                                break;

                            // AST
                            case 600:
                                if (data.GetSeals[0] == SealType.NONE)
                                { style.Unit = 0; data.ASTSeal1 = ""; }
                                else if (data.GetSeals[0] == SealType.SUN)
                                { style.Unit = 1; data.ASTSeal1 = "Solar"; style.IconColor = style.IconColor4; }
                                else if (data.GetSeals[0] == SealType.MOON)
                                { style.Unit = 2; data.ASTSeal1 = "Lunar"; style.IconColor = style.IconColor2; }
                                else if (data.GetSeals[0] == SealType.CELESTIAL)
                                { style.Unit = 3; data.ASTSeal1 = "Celestial"; style.IconColor = style.IconColor3; }
                                style.MaxUnit = 3;
                                break;
                            case 601:
                                if (data.GetSeals[1] == SealType.NONE)
                                { style.Unit = 0; data.ASTSeal2 = ""; }
                                else if (data.GetSeals[1] == SealType.SUN)
                                { style.Unit = 1; data.ASTSeal2 = "Solar"; style.IconColor = style.IconColor4; }
                                else if (data.GetSeals[1] == SealType.MOON)
                                { style.Unit = 2; data.ASTSeal2 = "Lunar"; style.IconColor = style.IconColor2; }
                                else if (data.GetSeals[1] == SealType.CELESTIAL)
                                { style.Unit = 3; data.ASTSeal2 = "Celestial"; style.IconColor = style.IconColor3; }
                                style.MaxUnit = 3;
                                break;
                            case 602:
                                if (data.GetSeals[2] == SealType.NONE)
                                { style.Unit = 0; data.ASTSeal3 = ""; }
                                else if (data.GetSeals[2] == SealType.SUN)
                                { style.Unit = 1; data.ASTSeal3 = "Solar"; style.IconColor = style.IconColor4; }
                                else if (data.GetSeals[2] == SealType.MOON)
                                { style.Unit = 2; data.ASTSeal3 = "Lunar"; style.IconColor = style.IconColor2; }
                                else if (data.GetSeals[2] == SealType.CELESTIAL)
                                { style.Unit = 3; data.ASTSeal3 = "Celestial"; style.IconColor = style.IconColor3; }
                                style.MaxUnit = 3;
                                break;

                            // SCH
                            case 700:
                                style.Unit = data.SCHAetherflow;
                                style.MaxUnit = 3;
                                break;
                            case 701:
                                style.Unit = data.FairyGauge;
                                style.MaxUnit = 100;
                                break;
                            case 702:
                                style.Unit = data.SeraphTimer;
                                style.MaxUnit = 22000;
                                break;

                            // SGE
                            case 800:
                                style.Unit = 0;
                                if (data.Eukrasia)
                                    style.Unit = 1;
                                style.MaxUnit = 1;
                                break;
                            case 801:
                                style.Unit = data.AddersgallTimer;
                                style.MaxUnit = 20000;
                                break;
                            case 802:
                                style.Unit = data.AddersgallStacks;
                                style.MaxUnit = 3;
                                break;
                            case 803:
                                style.Unit = data.AdderstingStacks;
                                style.MaxUnit = 3;
                                break;

                            // SGE
                            case 900:
                                style.Unit = data.LilyTimer;
                                style.MaxUnit = 20000;
                                break;
                            case 901:
                                style.Unit = data.Lilies;
                                style.MaxUnit = 3;
                                break;
                            case 902:
                                style.Unit = data.BloodLilies;
                                style.MaxUnit = 3;
                                break;
                            // SGE
                            case 1100:
                                style.Unit = data.ElementTimer;
                                style.MaxUnit = 15000;
                                break;
                            case 1101:
                                style.Unit = 0;
                                if (data.InUmbralIce)
                                    style.Unit = 1;
                                style.MaxUnit = 1;
                                break;
                            case 1102:
                                style.Unit = 0;
                                if (data.InAstralFire)
                                    style.Unit = 1;
                                style.MaxUnit = 1;
                                break;
                            case 1103:
                                style.Unit = data.UmbralHearts;
                                style.MaxUnit = 3;
                                break;
                            case 1104:
                                style.Unit = data.UmbralIceStacks;
                                style.MaxUnit = 3;
                                break;
                            case 1105:
                                style.Unit = data.AstralFireStacks;
                                style.MaxUnit = 3;
                                break;
                            case 1106:
                                style.Unit = data.EnochianTimer;
                                style.MaxUnit = 30000;
                                break;
                            case 1107:
                                style.Unit = data.PolyglotStacks;
                                style.MaxUnit = 1;
                                if (data.CurrentLevel >= 80)
                                { style.MaxUnit = 2; }
                                break;
                            case 1108:
                                style.Unit = 0;
                                if (data.IsEnochianActive)
                                    style.Unit = 1;
                                style.MaxUnit = 1;
                                break;
                            case 1109:
                                style.Unit = 0;
                                if (data.IsParadoxActive)
                                    style.Unit = 1;
                                style.MaxUnit = 1;
                                break;
                            // RDM
                            case 1200:
                                style.Unit = data.WhiteMana;
                                style.MaxUnit = 100;
                                break;
                            case 1201:
                                style.Unit = data.BlackMana;
                                style.MaxUnit = 100;
                                break;
                            case 1202:
                                style.Unit = data.ManaStacks;
                                style.MaxUnit = 3;
                                break;
                            // SMN
                            case 1300:
                                style.Unit = data.SMNAetherflow;
                                style.MaxUnit = 2;
                                break;
                            case 1301:
                                style.Unit = data.SummonTimer;
                                style.MaxUnit = 15000;
                                break;
                            case 1302:
                                style.Unit = data.AttunmentTimer;
                                style.MaxUnit = 30000;
                                break;
                            case 1303:
                                style.Unit = 0;
                                if (data.IsIfritReady)
                                    style.Unit = 1;
                                style.MaxUnit = 1;
                                break;
                            case 1304:
                                style.Unit = 0;
                                if (data.IsGarudaReady)
                                    style.Unit = 1;
                                style.MaxUnit = 1;
                                break;
                            case 1305:
                                style.Unit = 0;
                                if (data.IsTitanReady)
                                    style.Unit = 1;
                                style.MaxUnit = 1;
                                break;
                            case 1306:
                                style.Unit = 0;
                                if (data.IsIfritAttuned)
                                    style.Unit = 1;
                                style.MaxUnit = 1;
                                break;
                            case 1307:
                                style.Unit = 0;
                                if (data.IsGarudaAttuned)
                                    style.Unit = 1;
                                style.MaxUnit = 1;
                                break;
                            case 1308:
                                style.Unit = 0;
                                if (data.IsTitanAttuned)
                                    style.Unit = 1;
                                style.MaxUnit = 1;
                                break;
                            case 1309:
                                style.Unit = 0;
                                if (data.IsBahamutReady)
                                    style.Unit = 1;
                                style.MaxUnit = 1;
                                break;
                            case 1310:
                                style.Unit = 0;
                                if (data.IsPhoenixReady)
                                    style.Unit = 1;
                                style.MaxUnit = 1;
                                break;
                                // BRD
                            case 1500:
                                style.Unit = data.SongTimer;
                                style.MaxUnit = 45000;
                                break;
                            case 1501:
                                style.Unit = data.Repertoire;
                                style.MaxUnit = 0;
                                if (data.Song == Song.NONE)
                                { style.MaxUnit = 0; }
                                if (data.Song == Song.WANDERER)
                                { style.MaxUnit = 3; }
                                if (data.Song == Song.ARMY)
                                { style.MaxUnit = 4; }
                                if (data.Song == Song.MAGE)
                                { style.MaxUnit = 0; }
                                break;
                            case 1502:
                                style.Unit = data.SoulVoice;
                                style.MaxUnit = 100;
                                break;
                            case 1503:
                                if (data.Song == Song.NONE)
                                { style.Unit = 0; data.CurrentSong = ""; }
                                else if (data.Song == Song.WANDERER)
                                { style.Unit = 1; data.CurrentSong = "Wanderer's Minuet"; style.IconColor = style.IconColor4; }
                                else if (data.Song == Song.MAGE)
                                { style.Unit = 2; data.CurrentSong = "Mage's Ballad"; style.IconColor = style.IconColor2; }
                                else if (data.Song == Song.ARMY)
                                { style.Unit = 3; data.CurrentSong = "Army's Paeon"; style.IconColor = style.IconColor3; }
                                style.MaxUnit = 3;
                                break;
                            case 1504:
                                if (data.Coda[0] == Song.NONE)
                                { style.Unit = 0; data.Coda1 = ""; }
                                else if (data.Coda[0] == Song.WANDERER)
                                { style.Unit = 1; data.Coda1 = "Wanderer's Minuet"; style.IconColor = style.IconColor4; }
                                else if (data.Coda[0] == Song.MAGE)
                                { style.Unit = 2; data.Coda1 = "Mage's Ballad"; style.IconColor = style.IconColor2; }
                                else if (data.Coda[0] == Song.ARMY)
                                { style.Unit = 3; data.Coda1 = "Army's Paeon"; style.IconColor = style.IconColor3; }
                                style.MaxUnit = 3;
                                break;
                            case 1505:
                                if (data.Coda[1] == Song.NONE)
                                { style.Unit = 0; data.Coda2 = ""; }
                                else if (data.Coda[1] == Song.WANDERER)
                                { style.Unit = 1; data.Coda2 = "Wanderer's Minuet"; style.IconColor = style.IconColor4; }
                                else if (data.Coda[1] == Song.MAGE)
                                { style.Unit = 2; data.Coda2 = "Mage's Ballad"; style.IconColor = style.IconColor2; }
                                else if (data.Coda[1] == Song.ARMY)
                                { style.Unit = 3; data.Coda2 = "Army's Paeon"; style.IconColor = style.IconColor3; }
                                style.MaxUnit = 3;
                                break;
                            case 1506:
                                if (data.Coda[2] == Song.NONE)
                                { style.Unit = 0; data.Coda3 = ""; }
                                else if (data.Coda[2] == Song.WANDERER)
                                { style.Unit = 1; data.Coda3 = "Wanderer's Minuet"; style.IconColor = style.IconColor4; }
                                else if (data.Coda[2] == Song.MAGE)
                                { style.Unit = 2; data.Coda3 = "Mage's Ballad"; style.IconColor = style.IconColor2; }
                                else if (data.Coda[2] == Song.ARMY)
                                { style.Unit = 3; data.Coda3 = "Army's Paeon"; style.IconColor = style.IconColor3; }
                                style.MaxUnit = 3;
                                break;
                                // DNC
                            case 1600:
                                style.Unit = data.Feathers;
                                style.MaxUnit = 4;
                                break;
                            case 1601:
                                style.Unit = data.Esprit;
                                style.MaxUnit = 100;
                                break;
                            case 1602:
                                if (data.NextStepID != 15999 && data.NextStepID != 16000 &&
                                    data.NextStepID != 16001 && data.NextStepID != 16002)
                                { style.Unit = 0; data.NextStep = ""; }
                                else if (data.NextStepID == 15999)
                                { style.Unit = 1; data.NextStep = "Emboite"; style.IconColor = style.IconColor4; }
                                else if (data.NextStepID == 16000)
                                { style.Unit = 2; data.NextStep = "Entrechat"; style.IconColor = style.IconColor2; }
                                else if (data.NextStepID == 16001)
                                { style.Unit = 3; data.NextStep = "Jete"; style.IconColor = style.IconColor3; }
                                else if (data.NextStepID == 16002)
                                { style.Unit = 4; data.NextStep = "Pirouette"; style.IconColor = style.IconColor6; }
                                style.MaxUnit = 4;
                                break;
                            case 1603:
                                if (data.Steps[0] != 15999 && data.Steps[0] != 16000 &&
                                    data.Steps[0] != 16001 && data.Steps[0] != 16002)
                                { style.Unit = 0; data.Step1 = ""; }
                                else if (data.Steps[0] == 15999)
                                { style.Unit = 1; data.Step1 = "Emboite"; style.IconColor = style.IconColor4; }
                                else if (data.Steps[0] == 16000)
                                { style.Unit = 2; data.Step1 = "Entrechat"; style.IconColor = style.IconColor2; }
                                else if (data.Steps[0] == 16001)
                                { style.Unit = 3; data.Step1 = "Jete"; style.IconColor = style.IconColor3; }
                                else if (data.Steps[0] == 16002)
                                { style.Unit = 4; data.Step1 = "Pirouette"; style.IconColor = style.IconColor6; }
                                style.MaxUnit = 4;
                                break;
                            case 1604:
                                if (data.Steps[1] != 15999 && data.Steps[1] != 16000 &&
                                    data.Steps[1] != 16001 && data.Steps[1] != 16002)
                                { style.Unit = 0; data.Step2 = ""; }
                                else if (data.Steps[1] == 15999)
                                { style.Unit = 1; data.Step2 = "Emboite"; style.IconColor = style.IconColor4; }
                                else if (data.Steps[1] == 16000)
                                { style.Unit = 2; data.Step2 = "Entrechat"; style.IconColor = style.IconColor2; }
                                else if (data.Steps[1] == 16001)
                                { style.Unit = 3; data.Step2 = "Jete"; style.IconColor = style.IconColor3; }
                                else if (data.Steps[1] == 16002)
                                { style.Unit = 4; data.Step2 = "Pirouette"; style.IconColor = style.IconColor6; }
                                style.MaxUnit = 4;
                                break;
                            case 1605:
                                if (data.Steps[2] != 15999 && data.Steps[2] != 16000 &&
                                    data.Steps[2] != 16001 && data.Steps[2] != 16002)
                                { style.Unit = 0; data.Step3 = ""; }
                                else if (data.Steps[2] == 15999)
                                { style.Unit = 1; data.Step3 = "Emboite"; style.IconColor = style.IconColor4; }
                                else if (data.Steps[2] == 16000)
                                { style.Unit = 2; data.Step3 = "Entrechat"; style.IconColor = style.IconColor2; }
                                else if (data.Steps[2] == 16001)
                                { style.Unit = 3; data.Step3 = "Jete"; style.IconColor = style.IconColor3; }
                                else if (data.Steps[2] == 16002)
                                { style.Unit = 4; data.Step3 = "Pirouette"; style.IconColor = style.IconColor6; }
                                style.MaxUnit = 4;
                                break;
                            case 1606:
                                if (data.Steps[3] != 15999 && data.Steps[3] != 16000 &&
                                    data.Steps[3] != 16001 && data.Steps[3] != 16002)
                                { style.Unit = 0; data.Step4 = ""; }
                                else if (data.Steps[3] == 15999)
                                { style.Unit = 1; data.Step4 = "Emboite"; style.IconColor = style.IconColor4; }
                                else if (data.Steps[3] == 16000)
                                { style.Unit = 2; data.Step4 = "Entrechat"; style.IconColor = style.IconColor2; }
                                else if (data.Steps[3] == 16001)
                                { style.Unit = 3; data.Step4 = "Jete"; style.IconColor = style.IconColor3; }
                                else if (data.Steps[3] == 16002)
                                { style.Unit = 4; data.Step4 = "Pirouette"; style.IconColor = style.IconColor6; }
                                style.MaxUnit = 4;
                                break;
                            // MCH
                            case 1700:
                                style.Unit = data.Heat;
                                style.MaxUnit = 100;
                                break;
                            case 1701:
                                style.Unit = 0;
                                if (data.IsOverheated)
                                    style.Unit = 1;
                                style.MaxUnit = 1;
                                break;
                            case 1702:
                                style.Unit = data.OverheatTimer;
                                style.MaxUnit = 10000;
                                break;
                            case 1703:
                                style.Unit = data.Battery;
                                style.MaxUnit = 100;
                                break;
                            case 1704:
                                style.Unit = data.LastSummonBatteryPower;
                                style.MaxUnit = 100;
                                break;
                            case 1705:
                                style.Unit = data.RookQueenTimer;
                                style.MaxUnit = 20000;
                                break;
                            case 1706:
                                style.Unit = 0;
                                if (data.IsRobotActive)
                                    style.Unit = 1;
                                style.MaxUnit = 1;
                                break;
                                // DRG
                            case 1900:
                                style.Unit = data.LoTDTimer;
                                if (data.CurrentLevel < 78)
                                    { style.MaxUnit = 20000; }
                                else
                                    { style.MaxUnit = 30000; }
                                break;
                            case 1901:
                                style.Unit = 0;
                                if (data.IsLoTDActive)
                                    style.Unit = 1;
                                style.MaxUnit = 1;
                                break;
                            case 1902:
                                style.Unit = data.EyeCount;
                                style.MaxUnit = 2;
                                break;
                            case 1903:
                                style.Unit = data.FMFocus;
                                style.MaxUnit = 2;
                                break;
                            // MNK
                            case 2000:
                                style.Unit = data.Chakra;
                                style.MaxUnit = 5;
                                break;
                            case 2001:
                                style.Unit = 0;
                                if (data.AvaliableNadi == Nadi.NONE)
                                { style.Unit = 0; data.Nadi = ""; }
                                else if (data.AvaliableNadi == Nadi.SOLAR)
                                { style.Unit = 1; data.Nadi = "Solar"; style.IconColor = style.IconColor4; }
                                else if (data.AvaliableNadi == Nadi.LUNAR)
                                { style.Unit = 1; data.Nadi = "Lunar"; style.IconColor = style.IconColor2; }
                                style.MaxUnit = 2;
                                break;
                            case 2002:
                                if (data.BeastChakra[0] == BeastChakra.NONE)
                                { style.Unit = 0; data.BeastChakra1 = ""; }
                                else if (data.BeastChakra[0] == BeastChakra.COEURL)
                                { style.Unit = 1; data.BeastChakra1 = "Coeurl"; style.IconColor = style.IconColor4; }
                                else if (data.BeastChakra[0] == BeastChakra.OPOOPO)
                                { style.Unit = 2; data.BeastChakra1 = "Opo-opo"; style.IconColor = style.IconColor2; }
                                else if (data.BeastChakra[0] == BeastChakra.RAPTOR)
                                { style.Unit = 3; data.BeastChakra1 = "Raptor"; style.IconColor = style.IconColor3; }
                                style.MaxUnit = 3;
                                break;
                            case 2003:
                                if (data.BeastChakra[1] == BeastChakra.NONE)
                                { style.Unit = 0; data.BeastChakra2 = ""; }
                                else if (data.BeastChakra[1] == BeastChakra.COEURL)
                                { style.Unit = 1; data.BeastChakra2 = "Coeurl"; style.IconColor = style.IconColor4; }
                                else if (data.BeastChakra[1] == BeastChakra.OPOOPO)
                                { style.Unit = 2; data.BeastChakra2 = "Opo-opo"; style.IconColor = style.IconColor2; }
                                else if (data.BeastChakra[1] == BeastChakra.RAPTOR)
                                { style.Unit = 3; data.BeastChakra2 = "Raptor"; style.IconColor = style.IconColor3; }
                                style.MaxUnit = 3;
                                break;
                            case 2004:
                                if (data.BeastChakra[2] == BeastChakra.NONE)
                                { style.Unit = 0; data.BeastChakra3 = ""; }
                                else if (data.BeastChakra[2] == BeastChakra.COEURL)
                                { style.Unit = 1; data.BeastChakra3 = "Coeurl"; style.IconColor = style.IconColor4; }
                                else if (data.BeastChakra[2] == BeastChakra.OPOOPO)
                                { style.Unit = 2; data.BeastChakra3 = "Opo-opo"; style.IconColor = style.IconColor2; }
                                else if (data.BeastChakra[2] == BeastChakra.RAPTOR)
                                { style.Unit = 3; data.BeastChakra3 = "Raptor"; style.IconColor = style.IconColor3; }
                                style.MaxUnit = 3;
                                break;
                            // NIN
                            case 2100:
                                style.Unit = data.Ninki;
                                style.MaxUnit = 100;
                                break;
                            case 2101:
                                style.Unit = data.HutonTimer;
                                style.MaxUnit = 60;
                                break;
                            // RPR
                            case 2200:
                                style.Unit = data.Soul;
                                style.MaxUnit = 100;
                                break;
                            case 2201:
                                style.Unit = data.EnshroudTimer;
                                style.MaxUnit = 30000;
                                break;
                            case 2202:
                                style.Unit = data.Shroud;
                                style.MaxUnit = 100;
                                break;
                            case 2203:
                                style.Unit = data.LemureShroud;
                                style.MaxUnit = 5;
                                break;
                            case 2204:
                                style.Unit = data.VoidShroud;
                                style.MaxUnit = 5;
                                break;
                            // SAM
                            case 2300:
                                style.Unit = data.Kenki;
                                style.MaxUnit = 100;
                                break;
                            case 2301:
                                style.Unit = data.MeditationStacks;
                                style.MaxUnit = 3;
                                break;
                            case 2302:
                                style.Unit = 0;
                                if (data.HasGetsu == true)
                                    style.Unit = 1;
                                style.MaxUnit = 1;
                                break;
                            case 2303:
                                style.Unit = 0;
                                if (data.HasSetsu == true)
                                    style.Unit = 1;
                                style.MaxUnit = 1;
                                break;
                            case 2304:
                                style.Unit = 0;
                                if (data.HasKa == true)
                                    style.Unit = 1;
                                style.MaxUnit = 1;
                                break;

                        }

                        data.Gauge = style.Unit;
                        data.MaxGauge = style.MaxUnit;
                        if (data.MaxGauge > 1000)
                        {
                            data.Gauge /= 1000;
                            data.MaxGauge /= 1000;
                        }

                        if (IsVertical == false)
                        {
                            var originalSizex = size.X;
                            var originalSizey = size.Y;
                            // treat originalSizex as 100(%) and compare it against current Unit/MaxUnit
                            // to get percentage of bar to be filled. Keep originalSizey to maintain bar height.
                            // localpos does not change, bar fills left -> right. Ta-dah, very basic bars.
                            var dynamicSizex = (originalSizex * (style.Unit / style.MaxUnit));
                            var dynamicSizey = originalSizey;
                            var reverseSizex = Math.Abs((originalSizex * (style.Unit / style.MaxUnit)) - originalSizex);
                            Vector2 dynamicsize = new Vector2(dynamicSizex, dynamicSizey);
                            Vector2 dynamicreverse = new Vector2(reverseSizex, dynamicSizey);

                            // Get the center value (Y/2) of the bar to place rounded ends correctly
                            var circleY = (size.Y / 2);
                            Vector2 horzCircle = new Vector2(circleY, circleY);
                            Vector2 horzcircleOffset = new Vector2(circleY, 0);
                            Vector2 vertcircleOffset = new Vector2(0, circleY);
                            var rounddynamicSizex = ((originalSizex - size.Y) * (style.Unit / style.MaxUnit));
                            var roundreverseSizex = Math.Abs(((originalSizex - size.Y) * (style.Unit / style.MaxUnit)) - (originalSizex - size.Y));
                            Vector2 rounddynamicsize = new Vector2(rounddynamicSizex, dynamicSizey);
                            Vector2 rounddynamicreverse = new Vector2(roundreverseSizex, dynamicSizey);

                            // grab colors and break into vector4 to convert to hsv
                            var rightColor = ImGui.ColorConvertU32ToFloat4(style.naIconColor.Base);
                            var leftColor = ImGui.ColorConvertU32ToFloat4(style.naIconColor2.Base);
                            if (style.Direction)
                            {
                                rightColor = ImGui.ColorConvertU32ToFloat4(style.naIconColor2.Base);
                                leftColor = ImGui.ColorConvertU32ToFloat4(style.naIconColor.Base);
                            }
                            Vector4 rightHSV = new Vector4(0.5f, 1.0f, 1.0f, 1.0f);
                            Vector4 leftHSV = new Vector4(0.5f, 1.0f, 1.0f, 1.0f);
                            Vector4 colorShiftRGB = new Vector4(0, 0, 0, 1);
                            // 0 = left color, 1 = right color
                            var ratio = (style.Unit / style.MaxUnit);
                            // 
                            ImGui.ColorConvertRGBtoHSV(rightColor.X, rightColor.Y, rightColor.Z, out rightHSV.X, out rightHSV.Y, out rightHSV.Z);
                            ImGui.ColorConvertRGBtoHSV(leftColor.X, leftColor.Y, leftColor.Z, out leftHSV.X, out leftHSV.Y, out leftHSV.Z);
                            //
                            if (style.ColorMode)
                            {
                                Vector4 colorShiftHSV = new Vector4((ConfigColor.LinearInterpolation(rightHSV.X, leftHSV.X, ratio)),
                                                                    (ConfigColor.LinearInterpolation(rightHSV.Y, leftHSV.Y, ratio)),
                                                                    (ConfigColor.LinearInterpolation(rightHSV.Z, leftHSV.Z, ratio)), 1);
                                ImGui.ColorConvertHSVtoRGB(colorShiftHSV.X, colorShiftHSV.Y, colorShiftHSV.Z, out colorShiftRGB.X, out colorShiftRGB.Y, out colorShiftRGB.Z);

                            }
                            else
                            {
                                colorShiftRGB = new Vector4((ConfigColor.LinearInterpolation(rightColor.X, leftColor.X, ratio)),
                                                            (ConfigColor.LinearInterpolation(rightColor.Y, leftColor.Y, ratio)),
                                                            (ConfigColor.LinearInterpolation(rightColor.Z, leftColor.Z, ratio)), 1);
                            }
                            var colorShiftU32 = ImGui.ColorConvertFloat4ToU32(colorShiftRGB);

                            if (style.Rounding == false)
                            {
                                if (style.IconOption < 11)
                                {
                                    if (style.Chevron)
                                    {
                                        if (style.ValueColor == false)
                                        {
                                            colorShiftU32 = style.IconColor.Base;
                                        }
                                        if (style.IconOption == 6)
                                        {
                                            // treat localpos as the centerpoint of the quad
                                            var getskewX = style.Skew.X;
                                            var getskewY = style.Skew.Y;
                                            var getlocalPosX = localPos.X + size.X;
                                            var getlocalPosY = localPos.Y + size.X;
                                            if (getskewX < 0)
                                            { getlocalPosX = localPos.X + size.X - getskewX; }

                                            Vector2 pointA = new Vector2(getlocalPosX - size.X + getskewX, getlocalPosY - size.X + -getskewY);
                                            Vector2 pointB = new Vector2(getlocalPosX - size.X, getlocalPosY);
                                            Vector2 pointC = new Vector2(getlocalPosX - size.X + getskewX, getlocalPosY + size.X + getskewY);

                                            Vector2 pointX = new Vector2(getlocalPosX + size.X + getskewX, getlocalPosY - size.X + -getskewY);
                                            Vector2 pointY = new Vector2(getlocalPosX + size.X, getlocalPosY);
                                            Vector2 pointZ = new Vector2(getlocalPosX + size.X + getskewX, getlocalPosY + size.X + getskewY);

                                            drawList.AddQuadFilled(pointA, pointX, pointY, pointB, style.IconColor5.Base); // Inactive
                                            drawList.AddQuadFilled(pointB.AddY(-1), pointY.AddY(-1), pointZ, pointC, style.IconColor5.Base); // Inactive

                                            if (style.Unit > 0)
                                            {
                                                drawList.AddQuadFilled(pointA, pointX, pointY, pointB, colorShiftU32);
                                                drawList.AddQuadFilled(pointB.AddY(-1), pointY.AddY(-1), pointZ, pointC, colorShiftU32);
                                            }
                                        }
                                        if (style.IconOption == 7)
                                        {
                                            for (int i = 0; i < style.MaxUnit; i++)
                                            {
                                                Vector2 spread = new Vector2(style.Spread.X, style.Spread.Y);
                                                // treat localpos as the centerpoint of the quad
                                                var getskewX = style.Skew.X;
                                                var getskewY = style.Skew.Y;
                                                var getlocalPosX = localPos.X + size.X + (spread.X * i);
                                                var getlocalPosY = localPos.Y + size.X + (spread.Y * i);
                                                if (getskewX < 0)
                                                { getlocalPosX = localPos.X + size.X - getskewX + (spread.X * i); }

                                                Vector2 pointA = new Vector2(getlocalPosX - size.X + getskewX, getlocalPosY - size.X + -getskewY);
                                                Vector2 pointB = new Vector2(getlocalPosX - size.X, getlocalPosY);
                                                Vector2 pointC = new Vector2(getlocalPosX - size.X + getskewX, getlocalPosY + size.X + getskewY);

                                                Vector2 pointX = new Vector2(getlocalPosX + size.X + getskewX, getlocalPosY - size.X + -getskewY);
                                                Vector2 pointY = new Vector2(getlocalPosX + size.X, getlocalPosY);
                                                Vector2 pointZ = new Vector2(getlocalPosX + size.X + getskewX, getlocalPosY + size.X + getskewY);

                                                drawList.AddQuadFilled(pointA, pointX, pointY, pointB, style.IconColor5.Base); // Inactive
                                                drawList.AddQuadFilled(pointB.AddY(-1), pointY.AddY(-1), pointZ, pointC, style.IconColor5.Base); // Inactive

                                                if (style.Unit >= i + 1)
                                                {
                                                    drawList.AddQuadFilled(pointA, pointX, pointY, pointB, colorShiftU32);
                                                    drawList.AddQuadFilled(pointB.AddY(-1), pointY.AddY(-1), pointZ, pointC, colorShiftU32);
                                                }

                                            }
                                        }
                                    }
                                    else if (style.VertChevron)
                                    {
                                        if (style.ValueColor == false)
                                        {
                                            colorShiftU32 = style.IconColor.Base;
                                        }
                                        if (style.IconOption == 6)
                                        {
                                            // treat localpos as the centerpoint of the quad
                                            var getskewX = style.Skew.X;
                                            var getskewY = style.Skew.Y;
                                            var getlocalPosX = localPos.X + size.X;
                                            var getlocalPosY = localPos.Y + size.X;
                                            if (getskewY < 0)
                                            { getlocalPosY = localPos.Y + size.X - getskewY; }

                                            Vector2 pointA = new Vector2(getlocalPosX - size.X - getskewX, getlocalPosY - size.X + getskewY);
                                        Vector2 pointB = new Vector2(getlocalPosX, getlocalPosY - size.X);
                                        Vector2 pointC = new Vector2(getlocalPosX + size.X + getskewX, getlocalPosY - size.X + getskewY);

                                        Vector2 pointX = new Vector2(getlocalPosX - size.X - getskewX, getlocalPosY + size.X + getskewY);
                                        Vector2 pointY = new Vector2(getlocalPosX, getlocalPosY + size.X);
                                        Vector2 pointZ = new Vector2(getlocalPosX + size.X + getskewX, getlocalPosY + size.X + getskewY);

                                            drawList.AddQuadFilled(pointA, pointB, pointY, pointX, style.IconColor5.Base); // Inactive
                                            drawList.AddQuadFilled(pointB, pointC, pointZ, pointY, style.IconColor5.Base); // Inactive

                                            if (style.Unit > 0)
                                            {
                                                drawList.AddQuadFilled(pointA, pointB, pointY, pointX, colorShiftU32);
                                                drawList.AddQuadFilled(pointB, pointC, pointZ, pointY, colorShiftU32);
                                            }
                                        }
                                        if (style.IconOption == 7)
                                        {
                                            for (int i = 0; i < style.MaxUnit; i++)
                                            {
                                                Vector2 spread = new Vector2(style.Spread.X, style.Spread.Y);
                                                // treat localpos as the centerpoint of the quad
                                                var getskewX = style.Skew.X;
                                                var getskewY = style.Skew.Y;
                                                var getlocalPosX = localPos.X + size.X + (spread.X * i);
                                                var getlocalPosY = localPos.Y + size.X + (spread.Y * i);
                                                if (getskewY < 0)
                                                { getlocalPosY = localPos.Y + size.X - getskewY + (spread.Y * i); }

                                                Vector2 pointA = new Vector2(getlocalPosX - size.X - getskewX, getlocalPosY - size.X + getskewY);
                                                Vector2 pointB = new Vector2(getlocalPosX, getlocalPosY - size.X);
                                                Vector2 pointC = new Vector2(getlocalPosX + size.X + getskewX, getlocalPosY - size.X + getskewY);

                                                Vector2 pointX = new Vector2(getlocalPosX - size.X - getskewX, getlocalPosY + size.X + getskewY);
                                                Vector2 pointY = new Vector2(getlocalPosX, getlocalPosY + size.X);
                                                Vector2 pointZ = new Vector2(getlocalPosX + size.X + getskewX, getlocalPosY + size.X + getskewY);

                                                drawList.AddQuadFilled(pointA, pointB, pointY, pointX, style.IconColor5.Base); // Inactive
                                                drawList.AddQuadFilled(pointB, pointC, pointZ, pointY, style.IconColor5.Base); // Inactive

                                                if (style.Unit >= i + 1)
                                                {
                                                    drawList.AddQuadFilled(pointA, pointB, pointY, pointX, colorShiftU32);
                                                    drawList.AddQuadFilled(pointB, pointC, pointZ, pointY, colorShiftU32);
                                                }

                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (style.ValueColor == false)
                                        {
                                            colorShiftU32 = style.IconColor.Base;
                                        }
                                        if (style.IconOption == 6)
                                        {
                                            // treat localpos as the centerpoint of the quad
                                            var getlocalPosX = localPos.X + size.X;
                                            var getlocalPosY = localPos.Y + size.X;
                                            Vector2 quadN = new Vector2(getlocalPosX, getlocalPosY + size.X);
                                            Vector2 quadE = new Vector2(getlocalPosX + size.X, getlocalPosY);
                                            Vector2 quadS = new Vector2(getlocalPosX, getlocalPosY - size.X);
                                            Vector2 quadW = new Vector2(getlocalPosX - size.X, getlocalPosY);

                                            drawList.AddQuadFilled(quadN, quadE, quadS, quadW, style.IconColor5.Base); // Inactive

                                            if (style.Unit > 0)
                                            {
                                                drawList.AddQuadFilled(quadN, quadE, quadS, quadW, colorShiftU32);
                                            }
                                        }
                                        if (style.IconOption == 7)
                                        {
                                            for (int i = 0; i < style.MaxUnit; i++)
                                            {
                                                Vector2 spread = new Vector2(style.Spread.X, style.Spread.Y);
                                                // treat localpos as the centerpoint of the quad
                                                var getlocalPosX = localPos.X + size.X + (spread.X * i);
                                                var getlocalPosY = localPos.Y + size.X + (spread.Y * i);
                                                Vector2 quadN = new Vector2(getlocalPosX, getlocalPosY + size.X);
                                                Vector2 quadE = new Vector2(getlocalPosX + size.X, getlocalPosY);
                                                Vector2 quadS = new Vector2(getlocalPosX, getlocalPosY - size.X);
                                                Vector2 quadW = new Vector2(getlocalPosX - size.X, getlocalPosY);

                                                drawList.AddQuadFilled(quadN, quadE, quadS, quadW, style.IconColor5.Base); // Inactive

                                                if (style.Unit >= i + 1)
                                                {
                                                    drawList.AddQuadFilled(quadN, quadE, quadS, quadW, colorShiftU32);
                                                }

                                            }
                                        }
                                    }

                                }

                                if (style.IconOption == 11 || style.IconOption == 13)
                                {
                                    drawList.AddRectFilled(localPos, localPos + size, style.IconColor5.Base); // Background

                                    if (style.ValueColor == false)
                                    {
                                        colorShiftU32 = style.IconColor.Base;
                                    }
                                    if (style.Direction)
                                    {
                                        drawList.AddRectFilled(localPos, localPos + dynamicreverse, colorShiftU32); // Foreground
                                    }
                                    else
                                    {
                                        drawList.AddRectFilled(localPos, localPos + dynamicsize, colorShiftU32); // Foreground
                                    }
                                }

                                if (style.IconOption == 12 || style.IconOption == 14)
                                {
                                    drawList.AddRectFilled(localPos, localPos + size, style.IconColor5.Base); // Background

                                    if (style.Direction)
                                    {
                                        drawList.AddRectFilledMultiColor(localPos, localPos + dynamicreverse, style.IconColor.Base, style.IconColor2.Base, style.IconColor2.Base, style.IconColor.Base); // Foreground
                                    }
                                    else
                                    {
                                        drawList.AddRectFilledMultiColor(localPos, localPos + dynamicsize, style.IconColor.Base, style.IconColor2.Base, style.IconColor2.Base, style.IconColor.Base); // Foreground
                                    }
                                }

                            }
                            else if (style.Rounding == true)
                            {
                                if (style.IconOption < 11)
                                {
                                    if (style.ValueColor == false)
                                    {
                                        colorShiftU32 = style.IconColor.Base;
                                    }
                                    if (style.IconOption == 6)
                                    {
                                        // treat localpos as the centerpoint of the quad
                                        var getlocalPosX = localPos.X + size.X;
                                        var getlocalPosY = localPos.Y + size.X;
                                        Vector2 quadN = new Vector2(getlocalPosX, getlocalPosY + size.X);
                                        Vector2 quadE = new Vector2(getlocalPosX + size.X, getlocalPosY);
                                        Vector2 quadS = new Vector2(getlocalPosX, getlocalPosY - size.X);
                                        Vector2 quadW = new Vector2(getlocalPosX - size.X, getlocalPosY);

                                        drawList.AddCircleFilled(localPos, size.X, style.IconColor5.Base); // Inactive

                                        if (style.Unit > 0)
                                        {
                                            drawList.AddCircleFilled(localPos, size.X, colorShiftU32);
                                        }
                                    }
                                    if (style.IconOption == 7)
                                    {
                                        for (int i = 0; i < style.MaxUnit; i++)
                                        {
                                            Vector2 spread = new Vector2(style.Spread.X, style.Spread.Y);
                                            // treat localpos as the centerpoint of the quad
                                            var getlocalPosX = localPos.X + size.X + (spread.X * i);
                                            var getlocalPosY = localPos.Y + size.X + (spread.Y * i);
                                            Vector2 newlocalPos = new Vector2(getlocalPosX, getlocalPosY);
                                            Vector2 quadN = new Vector2(getlocalPosX, getlocalPosY + size.X);
                                            Vector2 quadE = new Vector2(getlocalPosX + size.X, getlocalPosY);
                                            Vector2 quadS = new Vector2(getlocalPosX, getlocalPosY - size.X);
                                            Vector2 quadW = new Vector2(getlocalPosX - size.X, getlocalPosY);

                                            drawList.AddCircleFilled(newlocalPos, size.X, style.IconColor5.Base); // Inactive

                                            if (style.Unit >= i + 1)
                                            {
                                                drawList.AddCircleFilled(newlocalPos, size.X, colorShiftU32);
                                            }

                                        }
                                    }

                                }

                                if (style.IconOption == 11 || style.IconOption == 13)
                                {
                                    drawList.AddRectFilled(localPos, localPos + size, style.IconColor5.Base, 1024); // Background

                                    if (style.ValueColor == false)
                                    {
                                        colorShiftU32 = style.naIconColor.Base;
                                    }
                                    if (style.Direction)
                                    {
                                        if (roundreverseSizex > 0)
                                        {
                                            drawList.AddCircleFilled((localPos + horzCircle), circleY, colorShiftU32); // Left Rounded end
                                            drawList.AddCircleFilled((localPos + horzcircleOffset + rounddynamicreverse - vertcircleOffset), circleY, colorShiftU32); // Right Rounded end
                                        }
                                        drawList.AddRectFilled((localPos + horzcircleOffset), (localPos + horzcircleOffset + rounddynamicreverse), colorShiftU32); // Foreground
                                    }
                                    else
                                    {
                                        if (rounddynamicSizex > 0)
                                        {
                                            drawList.AddCircleFilled((localPos + horzCircle), circleY, colorShiftU32); // Left Rounded end
                                            drawList.AddCircleFilled((localPos + horzcircleOffset + rounddynamicsize - vertcircleOffset), circleY, colorShiftU32); // Right Rounded end
                                        }
                                        drawList.AddRectFilled((localPos + horzcircleOffset), (localPos + horzcircleOffset + rounddynamicsize), colorShiftU32); // Foreground
                                    }
                                }

                                if (style.IconOption == 12 || style.IconOption == 14)
                                {
                                    drawList.AddRectFilled(localPos, localPos + size, style.IconColor5.Base, 1024); // Background

                                    if (style.Direction)
                                    {
                                        if (roundreverseSizex > 0)
                                        {
                                            drawList.AddCircleFilled((localPos + horzcircleOffset + rounddynamicreverse - vertcircleOffset), circleY, colorShiftU32); // Right Rounded end
                                            drawList.AddCircleFilled((localPos + horzCircle), circleY, style.naIconColor.Base); // Left Rounded end
                                        }
                                        drawList.AddRectFilledMultiColor((localPos + horzcircleOffset), (localPos + horzcircleOffset + rounddynamicreverse), style.naIconColor.Base, colorShiftU32, colorShiftU32, style.naIconColor.Base); // Foreground
                                    }
                                    else
                                    {
                                        if (rounddynamicSizex > 0)
                                        {
                                            drawList.AddCircleFilled((localPos + horzcircleOffset + rounddynamicsize - vertcircleOffset), circleY, colorShiftU32); // Right Rounded end
                                            drawList.AddCircleFilled((localPos + horzCircle), circleY, style.naIconColor.Base); // Left Rounded end
                                        }
                                        drawList.AddRectFilledMultiColor((localPos + horzcircleOffset), (localPos + horzcircleOffset + rounddynamicsize), style.naIconColor.Base, colorShiftU32, colorShiftU32, style.naIconColor.Base); // Foreground
                                    }
                                }

                            }
                        }
                        else if (IsVertical)
                        {
                            var originalSizex = size.X;
                            var originalSizey = size.Y;
                            // treat originalSizey as 100(%) and compare it against current Unit/MaxUnit
                            // to get percentage of bar to be filled. Keep originalSizey to maintain bar height.
                            // localpos does not change, bar fills Top -> bottom. Ta-dah, very basic bars.
                            var dynamicSizey = (originalSizey * (style.Unit / style.MaxUnit));
                            var dynamicSizex = originalSizex;
                            var reverseSizey = Math.Abs((originalSizey * (style.Unit / style.MaxUnit)) - originalSizey);
                            Vector2 dynamicsize = new Vector2(dynamicSizex, dynamicSizey);
                            Vector2 dynamicreverse = new Vector2(dynamicSizex, reverseSizey);

                            // Get the center value (X/2) of the bar to place rounded ends correctly
                            var circleX = (size.X / 2);
                            Vector2 vertCircle = new Vector2(circleX, circleX);
                            Vector2 horzcircleOffset = new Vector2(circleX, 0);
                            Vector2 vertcircleOffset = new Vector2(0, circleX);
                            var rounddynamicSizey = ((originalSizey - size.X) * (style.Unit / style.MaxUnit));
                            var roundreverseSizey = Math.Abs(((originalSizey - size.X) * (style.Unit / style.MaxUnit)) - (originalSizey - size.X));
                            Vector2 rounddynamicsize = new Vector2(dynamicSizex, rounddynamicSizey);
                            Vector2 rounddynamicreverse = new Vector2(dynamicSizex, roundreverseSizey);

                            // grab colors and break into vector4 to convert to hsv
                            var rightColor = ImGui.ColorConvertU32ToFloat4(style.naIconColor.Base);
                            var leftColor = ImGui.ColorConvertU32ToFloat4(style.naIconColor2.Base);
                            if (style.Direction)
                            {
                                rightColor = ImGui.ColorConvertU32ToFloat4(style.naIconColor2.Base);
                                leftColor = ImGui.ColorConvertU32ToFloat4(style.naIconColor.Base);
                            }
                            Vector4 rightHSV = new Vector4(0.5f, 1.0f, 1.0f, 1.0f);
                            Vector4 leftHSV = new Vector4(0.5f, 1.0f, 1.0f, 1.0f);
                            Vector4 colorShiftRGB = new Vector4(0, 0, 0, 1);
                            // 0 = left color, 1 = right color
                            var ratio = (style.Unit / style.MaxUnit);
                            // 
                            ImGui.ColorConvertRGBtoHSV(rightColor.X, rightColor.Y, rightColor.Z, out rightHSV.X, out rightHSV.Y, out rightHSV.Z);
                            ImGui.ColorConvertRGBtoHSV(leftColor.X, leftColor.Y, leftColor.Z, out leftHSV.X, out leftHSV.Y, out leftHSV.Z);
                            //
                            if (style.ColorMode)
                            {
                                Vector4 colorShiftHSV = new Vector4((ConfigColor.LinearInterpolation(rightHSV.X, leftHSV.X, ratio)),
                                                                    (ConfigColor.LinearInterpolation(rightHSV.Y, leftHSV.Y, ratio)),
                                                                    (ConfigColor.LinearInterpolation(rightHSV.Z, leftHSV.Z, ratio)), 1);
                                ImGui.ColorConvertHSVtoRGB(colorShiftHSV.X, colorShiftHSV.Y, colorShiftHSV.Z, out colorShiftRGB.X, out colorShiftRGB.Y, out colorShiftRGB.Z);

                            }
                            else
                            {
                                colorShiftRGB = new Vector4((ConfigColor.LinearInterpolation(rightColor.X, leftColor.X, ratio)),
                                                            (ConfigColor.LinearInterpolation(rightColor.Y, leftColor.Y, ratio)),
                                                            (ConfigColor.LinearInterpolation(rightColor.Z, leftColor.Z, ratio)), 1);
                            }
                            var colorShiftU32 = ImGui.ColorConvertFloat4ToU32(colorShiftRGB);

                            if (style.Rounding == false)
                            {
                                if (style.IconOption < 11)
                                {
                                    if (style.ValueColor == false)
                                    {
                                        colorShiftU32 = style.IconColor.Base;
                                    }
                                    if (style.IconOption == 6)
                                    {
                                        // treat localpos as the centerpoint of the quad
                                        var getlocalPosX = localPos.X + size.X;
                                        var getlocalPosY = localPos.Y + size.X;
                                        Vector2 quadN = new Vector2(getlocalPosX, getlocalPosY + size.X);
                                        Vector2 quadE = new Vector2(getlocalPosX + size.X, getlocalPosY);
                                        Vector2 quadS = new Vector2(getlocalPosX, getlocalPosY - size.X);
                                        Vector2 quadW = new Vector2(getlocalPosX - size.X, getlocalPosY);

                                        drawList.AddQuadFilled(quadN, quadE, quadS, quadW, style.IconColor5.Base); // Inactive

                                        if (style.Unit > 0)
                                        {
                                            drawList.AddQuadFilled(quadN, quadE, quadS, quadW, colorShiftU32);
                                        }
                                    }
                                    if (style.IconOption == 7)
                                    {
                                        for (int i = 0; i < style.MaxUnit; i++)
                                        {
                                            Vector2 spread = new Vector2(style.Spread.X, style.Spread.Y);
                                            // treat localpos as the centerpoint of the quad
                                            var getlocalPosX = localPos.X + size.X + (spread.X * i);
                                            var getlocalPosY = localPos.Y + size.X + (spread.Y * i);
                                            Vector2 quadN = new Vector2(getlocalPosX, getlocalPosY + size.X);
                                            Vector2 quadE = new Vector2(getlocalPosX + size.X, getlocalPosY);
                                            Vector2 quadS = new Vector2(getlocalPosX, getlocalPosY - size.X);
                                            Vector2 quadW = new Vector2(getlocalPosX - size.X, getlocalPosY);

                                            drawList.AddQuadFilled(quadN, quadE, quadS, quadW, style.IconColor5.Base); // Inactive

                                            if (style.Unit >= i + 1)
                                            {
                                                drawList.AddQuadFilled(quadN, quadE, quadS, quadW, colorShiftU32);
                                            }

                                        }
                                    }

                                }

                                if (style.IconOption == 11 || style.IconOption == 13)
                                {
                                    drawList.AddRectFilled(localPos, localPos + size, style.IconColor5.Base); // Background
                                    if (style.ValueColor == false)
                                    {
                                        colorShiftU32 = style.IconColor.Base;
                                    }
                                    if (style.Direction)
                                    {
                                        drawList.AddRectFilled(localPos.AddY(dynamicSizey), localPos + size, colorShiftU32); // Foreground
                                    }
                                    else
                                    {
                                        drawList.AddRectFilled(localPos.AddY(reverseSizey), localPos + size, colorShiftU32); // Foreground
                                    }
                                }

                                if (style.IconOption == 12 || style.IconOption == 14)
                                {
                                    drawList.AddRectFilled(localPos, localPos + size, style.IconColor5.Base); // Background
                                    if (style.Direction)
                                    {
                                        drawList.AddRectFilledMultiColor(localPos.AddY(dynamicSizey), localPos + size, style.IconColor.Base, style.IconColor.Base, style.IconColor2.Base, style.IconColor2.Base); // Foreground
                                    }
                                    else
                                    {
                                        drawList.AddRectFilledMultiColor(localPos.AddY(reverseSizey), localPos + size, style.IconColor.Base, style.IconColor.Base, style.IconColor2.Base, style.IconColor2.Base); // Foreground
                                    }
                                }

                            }
                            else if (style.Rounding == true)
                            {
                                if (style.IconOption < 11)
                                {
                                    if (style.ValueColor == false)
                                    {
                                        colorShiftU32 = style.IconColor.Base;
                                    }
                                    if (style.IconOption == 6)
                                    {
                                        // treat localpos as the centerpoint of the quad
                                        var getlocalPosX = localPos.X + size.X;
                                        var getlocalPosY = localPos.Y + size.X;
                                        Vector2 quadN = new Vector2(getlocalPosX, getlocalPosY + size.X);
                                        Vector2 quadE = new Vector2(getlocalPosX + size.X, getlocalPosY);
                                        Vector2 quadS = new Vector2(getlocalPosX, getlocalPosY - size.X);
                                        Vector2 quadW = new Vector2(getlocalPosX - size.X, getlocalPosY);

                                        drawList.AddCircleFilled(localPos, size.X, style.IconColor5.Base); // Inactive

                                        if (style.Unit > 0)
                                        {
                                            drawList.AddCircleFilled(localPos, size.X, colorShiftU32);
                                        }
                                    }
                                    if (style.IconOption == 7)
                                    {
                                        for (int i = 0; i < style.MaxUnit; i++)
                                        {
                                            Vector2 spread = new Vector2(style.Spread.X, style.Spread.Y);
                                            // treat localpos as the centerpoint of the quad
                                            var getlocalPosX = localPos.X + size.X + (spread.X * i);
                                            var getlocalPosY = localPos.Y + size.X + (spread.Y * i);
                                            Vector2 newlocalPos = new Vector2(getlocalPosX, getlocalPosY);
                                            Vector2 quadN = new Vector2(getlocalPosX, getlocalPosY + size.X);
                                            Vector2 quadE = new Vector2(getlocalPosX + size.X, getlocalPosY);
                                            Vector2 quadS = new Vector2(getlocalPosX, getlocalPosY - size.X);
                                            Vector2 quadW = new Vector2(getlocalPosX - size.X, getlocalPosY);

                                            drawList.AddCircleFilled(newlocalPos, size.X, style.IconColor5.Base); // Inactive

                                            if (style.Unit >= i + 1)
                                            {
                                                drawList.AddCircleFilled(newlocalPos, size.X, colorShiftU32);
                                            }

                                        }
                                    }

                                }

                                if (style.IconOption == 11 || style.IconOption == 13)
                                {
                                    drawList.AddRectFilled(localPos, localPos + size, style.IconColor5.Base, 1024); // Background

                                    if (style.ValueColor == false)
                                    {
                                        colorShiftU32 = style.naIconColor.Base;
                                    }
                                    if (style.Direction)
                                    {
                                        if (rounddynamicSizey > 0)
                                        {
                                            drawList.AddCircleFilled((localPos.AddY(rounddynamicSizey) + vertCircle), circleX, colorShiftU32); // Top Rounded end
                                            drawList.AddCircleFilled((localPos + size - vertCircle), circleX, colorShiftU32); // Bottom Rounded end
                                        }
                                        drawList.AddRectFilled((localPos.AddY(rounddynamicSizey) + vertcircleOffset), (localPos + size - vertcircleOffset), colorShiftU32); // Foreground
                                    }
                                    else
                                    {
                                        if (roundreverseSizey > 0)
                                        {
                                            drawList.AddCircleFilled((localPos.AddY(roundreverseSizey) + vertCircle), circleX, colorShiftU32); // Top Rounded end
                                            drawList.AddCircleFilled((localPos + size - vertCircle), circleX, colorShiftU32); // Bottom Rounded end
                                        }
                                        drawList.AddRectFilled((localPos.AddY(roundreverseSizey) + vertcircleOffset), (localPos + size - vertcircleOffset), colorShiftU32); // Foreground
                                    }
                                }

                                if (style.IconOption == 12 || style.IconOption == 14)
                                {
                                    drawList.AddRectFilled(localPos, localPos + size, style.IconColor5.Base, 1024); // Background

                                    if (style.Direction)
                                    {
                                        if (rounddynamicSizey > 0)
                                        {
                                            drawList.AddCircleFilled((localPos.AddY(rounddynamicSizey) + vertCircle), circleX, colorShiftU32); // Top Rounded end
                                            drawList.AddCircleFilled((localPos + size - vertCircle), circleX, style.naIconColor.Base); // Bottom Rounded end
                                        }
                                        drawList.AddRectFilledMultiColor((localPos.AddY(rounddynamicSizey) + vertcircleOffset), (localPos + size - vertcircleOffset), colorShiftU32, colorShiftU32, style.naIconColor.Base, style.naIconColor.Base); // Foreground
                                    }
                                    else
                                    {
                                        if (roundreverseSizey > 0)
                                        {
                                            drawList.AddCircleFilled((localPos.AddY(roundreverseSizey) + vertCircle), circleX, colorShiftU32); // Top Rounded end
                                            drawList.AddCircleFilled((localPos + size - vertCircle), circleX, style.naIconColor.Base); // Bottom Rounded end
                                        }
                                        drawList.AddRectFilledMultiColor((localPos.AddY(roundreverseSizey) + vertcircleOffset), (localPos + size - vertcircleOffset), colorShiftU32, colorShiftU32, style.naIconColor.Base, style.naIconColor.Base); // Foreground
                                    }
                                }

                            }
                        }
                    }

                    if (style.ShowBorder)
                    {
                        for (int i = 0; i < style.BorderThickness; i++)
                        {
                            Vector2 offset = new Vector2(i, i);
                            Vector4 color = style.BorderColor.Vector.AddTransparency(alpha);
                            Vector4 color2 = new Vector4(0f, 0f, 0f, alpha / 2);

                            if (style.Rounding == false)
                            {
                                if (style.IconOption >= 11)
                                {
                                    drawList.AddRect(localPos + offset, localPos + size - offset, ImGui.ColorConvertFloat4ToU32(color));
                                }

                                if (style.Chevron)
                                {
                                    if (style.IconOption == 6)
                                    {
                                        // treat localpos as the centerpoint of the quad
                                        var getskewX = style.Skew.X;
                                        var getskewY = style.Skew.Y;
                                        var getlocalPosX = localPos.X + size.X;
                                        var getlocalPosY = localPos.Y + size.X;
                                        if (getskewX < 0)
                                        { getlocalPosX = localPos.X + size.X - getskewX; }
                                        var thickness = offset.X;

                                        Vector2 pointA = new Vector2(getlocalPosX - size.X + getskewX, getlocalPosY - size.X + -getskewY);
                                        Vector2 pointB = new Vector2(getlocalPosX - size.X, getlocalPosY);
                                        Vector2 pointC = new Vector2(getlocalPosX - size.X + getskewX, getlocalPosY + size.X + getskewY);

                                        Vector2 pointX = new Vector2(getlocalPosX + size.X + getskewX, getlocalPosY - size.X + -getskewY);
                                        Vector2 pointY = new Vector2(getlocalPosX + size.X, getlocalPosY);
                                        Vector2 pointZ = new Vector2(getlocalPosX + size.X + getskewX, getlocalPosY + size.X + getskewY);

                                        Vector2[] points = { pointA, pointB, pointC, pointZ, pointY, pointX };
                                        drawList.AddPolyline(ref points[0], 6, ImGui.ColorConvertFloat4ToU32(color), ImDrawFlags.Closed, thickness);

                                    }
                                    if (style.IconOption == 7)
                                    {
                                        for (int n = 0; n < style.MaxUnit; n++)
                                        {
                                            Vector2 spread = new Vector2(style.Spread.X, style.Spread.Y);
                                            // treat localpos as the centerpoint of the quad
                                            var getskewX = style.Skew.X;
                                            var getskewY = style.Skew.Y;
                                            var getlocalPosX = localPos.X + size.X + (spread.X * n);
                                            var getlocalPosY = localPos.Y + size.X + (spread.Y * n);
                                            if (getskewX < 0)
                                            { getlocalPosX = localPos.X + size.X - getskewX + (spread.X * n); }
                                            var thickness = offset.X;

                                            Vector2 pointA = new Vector2(getlocalPosX - size.X + getskewX, getlocalPosY - size.X + -getskewY);
                                            Vector2 pointB = new Vector2(getlocalPosX - size.X, getlocalPosY);
                                            Vector2 pointC = new Vector2(getlocalPosX - size.X + getskewX, getlocalPosY + size.X + getskewY);

                                            Vector2 pointX = new Vector2(getlocalPosX + size.X + getskewX, getlocalPosY - size.X + -getskewY);
                                            Vector2 pointY = new Vector2(getlocalPosX + size.X, getlocalPosY);
                                            Vector2 pointZ = new Vector2(getlocalPosX + size.X + getskewX, getlocalPosY + size.X + getskewY);

                                            Vector2[] points = { pointA, pointB, pointC, pointZ, pointY, pointX };
                                            drawList.AddPolyline(ref points[0], 6, ImGui.ColorConvertFloat4ToU32(color), ImDrawFlags.Closed, thickness);

                                        }
                                    }

                                }
                                else if (style.VertChevron)
                                {
                                    if (style.IconOption == 6)
                                    {
                                        // treat localpos as the centerpoint of the quad
                                        var getskewX = style.Skew.X;
                                        var getskewY = style.Skew.Y;
                                        var getlocalPosX = localPos.X + size.X;
                                        var getlocalPosY = localPos.Y + size.X;
                                        if (getskewY < 0)
                                        { getlocalPosY = localPos.Y + size.X - getskewY; }
                                        var thickness = offset.X;

                                        Vector2 pointA = new Vector2(getlocalPosX - size.X - getskewX, getlocalPosY - size.X + getskewY);
                                        Vector2 pointB = new Vector2(getlocalPosX, getlocalPosY - size.X);
                                        Vector2 pointC = new Vector2(getlocalPosX + size.X + getskewX, getlocalPosY - size.X + getskewY);

                                        Vector2 pointX = new Vector2(getlocalPosX - size.X - getskewX, getlocalPosY + size.X + getskewY);
                                        Vector2 pointY = new Vector2(getlocalPosX, getlocalPosY + size.X);
                                        Vector2 pointZ = new Vector2(getlocalPosX + size.X + getskewX, getlocalPosY + size.X + getskewY);

                                        Vector2[] points = { pointA, pointB, pointC, pointZ, pointY, pointX };
                                        drawList.AddPolyline(ref points[0], 6, ImGui.ColorConvertFloat4ToU32(color), ImDrawFlags.Closed, thickness);

                                    }
                                    if (style.IconOption == 7)
                                    {
                                        for (int n = 0; n < style.MaxUnit; n++)
                                        {
                                            Vector2 spread = new Vector2(style.Spread.X, style.Spread.Y);
                                            // treat localpos as the centerpoint of the quad
                                            var getskewX = style.Skew.X;
                                            var getskewY = style.Skew.Y;
                                            var getlocalPosX = localPos.X + size.X + (spread.X * n);
                                            var getlocalPosY = localPos.Y + size.X + (spread.Y * n);
                                            if (getskewY < 0)
                                            { getlocalPosY = localPos.Y + size.X - getskewY + (spread.Y * n); }
                                            var thickness = offset.X;

                                            Vector2 pointA = new Vector2(getlocalPosX - size.X - getskewX, getlocalPosY - size.X + getskewY);
                                            Vector2 pointB = new Vector2(getlocalPosX, getlocalPosY - size.X);
                                            Vector2 pointC = new Vector2(getlocalPosX + size.X + getskewX, getlocalPosY - size.X + getskewY);

                                            Vector2 pointX = new Vector2(getlocalPosX - size.X - getskewX, getlocalPosY + size.X + getskewY);
                                            Vector2 pointY = new Vector2(getlocalPosX, getlocalPosY + size.X);
                                            Vector2 pointZ = new Vector2(getlocalPosX + size.X + getskewX, getlocalPosY + size.X + getskewY);

                                            Vector2[] points = { pointA, pointB, pointC, pointZ, pointY, pointX };
                                            drawList.AddPolyline(ref points[0], 6, ImGui.ColorConvertFloat4ToU32(color), ImDrawFlags.Closed, thickness);

                                        }
                                    }

                                }
                                else
                                {
                                    if (style.IconOption == 6)
                                    {
                                        // treat localpos as the centerpoint of the quad
                                        var getlocalPosX = localPos.X + size.X;
                                        var getlocalPosY = localPos.Y + size.X;
                                        Vector2 quadN = new Vector2(getlocalPosX, getlocalPosY + size.X - offset.X);
                                        Vector2 quadE = new Vector2(getlocalPosX + size.X - offset.X, getlocalPosY);
                                        Vector2 quadS = new Vector2(getlocalPosX, getlocalPosY - size.X + offset.X);
                                        Vector2 quadW = new Vector2(getlocalPosX - size.X + offset.X, getlocalPosY);

                                        drawList.AddQuad(quadN, quadE, quadS, quadW, ImGui.ColorConvertFloat4ToU32(color));
                                    }
                                    if (style.IconOption == 7)
                                    {
                                        for (int n = 0; n < style.MaxUnit; n++)
                                        {
                                            Vector2 spread = new Vector2(style.Spread.X, style.Spread.Y);
                                            // treat localpos as the centerpoint of the quad
                                            var getlocalPosX = localPos.X + size.X + (spread.X * n);
                                            var getlocalPosY = localPos.Y + size.X + (spread.Y * n);
                                            Vector2 quadN = new Vector2(getlocalPosX, getlocalPosY + size.X - offset.X);
                                            Vector2 quadE = new Vector2(getlocalPosX + size.X - offset.X, getlocalPosY);
                                            Vector2 quadS = new Vector2(getlocalPosX, getlocalPosY - size.X + offset.X);
                                            Vector2 quadW = new Vector2(getlocalPosX - size.X + offset.X, getlocalPosY);

                                            drawList.AddQuad(quadN, quadE, quadS, quadW, ImGui.ColorConvertFloat4ToU32(color)); // Inactive

                                        }
                                    }
                                }
                            }
                            else if (style.Rounding == true)
                            {
                                if (style.IconOption >= 11)
                                {
                                    drawList.AddRect(localPos + offset, localPos + size - offset, ImGui.ColorConvertFloat4ToU32(color), 1024);
                                }

                                if (style.IconOption == 6)
                                {
                                    // treat localpos as the centerpoint of the quad
                                    var getlocalPosX = localPos.X + size.X;
                                    var getlocalPosY = localPos.Y + size.X;

                                    drawList.AddCircle(localPos, size.X - offset.X, ImGui.ColorConvertFloat4ToU32(color));
                                }
                                if (style.IconOption == 7)
                                {
                                    for (int n = 0; n < style.MaxUnit; n++)
                                    {
                                        Vector2 spread = new Vector2(style.Spread.X, style.Spread.Y);
                                        // treat localpos as the centerpoint of the quad
                                        var getlocalPosX = localPos.X + size.X + (spread.X * n);
                                        var getlocalPosY = localPos.Y + size.X + (spread.Y * n);
                                        Vector2 newlocalPos = new Vector2(getlocalPosX, getlocalPosY);

                                        drawList.AddCircle(newlocalPos, size.X - offset.X, ImGui.ColorConvertFloat4ToU32(color)); // Inactive

                                    }
                                }
                            }
                        }
                    }

                    if (style.Glow && style.IconOption != 6)
                    {
                        if (style.Rounding)
                        {
                            this.DrawIconGlowRound(localPos, size, style.GlowThickness, style.GlowSegments, style.GlowSpeed, style.GlowColor, style.GlowColor2, drawList);

                        }
                        else
                        {
                            this.DrawIconGlow(localPos, size, style.GlowThickness, style.GlowSegments, style.GlowSpeed, style.GlowColor, style.GlowColor2, drawList);
                        }
                    }
                });

                DrawHelpers.DrawInWindow($"##{this.ID}IconPreview", IconPos, iconsize, this.Preview, this.SetPosition, (drawListIcon) =>
                {
                    if (this.Preview)
                    {
                        data = this.UpdatePreviewData(data);
                        if (this.LastFrameWasDragging)
                        {
                            localPos = ImGui.GetWindowPos();
                            style.Position = localPos - pos;
                        }
                    }

                    //
                    // Seperate space for Icons on bars
                    // For some reason these work normally, but are not visible in Preview Mode
                    //

                    if (style.IconOption == 13 || style.IconOption == 14)
                    {
                        if (style.AutoIcon > 0)
                        {
                            uint icon = style.CustomIcon;

                            if (icon > 0)
                            {
                                DrawHelpers.DrawIcon(icon, IconPos, iconsize, style.CropIcon, stackCount, desaturate, alpha, drawListIcon);
                            }
                        }
                        else if (style.AutoIcon == 0)
                        {
                            uint icon = data.Icon;

                            if (icon > 0)
                            {
                                DrawHelpers.DrawIcon(icon, IconPos, iconsize, style.CropIcon, stackCount, desaturate, alpha, drawListIcon);
                            }
                        }

                        if (style.ShowBorder2)
                        {
                            for (int i = 0; i < style.BorderThickness2; i++)
                            {
                                Vector2 offset = new Vector2(i, i);
                                Vector4 color = style.BorderColor2.Vector.AddTransparency(alpha);

                                drawListIcon.AddRect(IconPos + offset, IconPos + iconsize - offset, ImGui.ColorConvertFloat4ToU32(color));
                            }
                        }
                    }

                    //
                    // End Icon space
                    //
                });
            }

            else
            {
                this.StartData = null;
                this.StartTime = null;
            }

            foreach (AuraLabel label in this.LabelListConfig.AuraLabels)
            {
                if (!this.Preview && this.LastFrameWasPreview)
                {
                    label.Preview = false;
                }
                else
                {
                    label.Preview |= this.Preview;
                }

                if (triggered || label.Preview)
                {
                    label.SetData(datas, triggeredIndex);
                    label.Draw(localPos, size, visible);
                }
            }

            this.LastFrameWasPreview = this.Preview;
        }

        private void DrawIconGlow(Vector2 pos, Vector2 size, int thickness, int segments, float speed, ConfigColor col1, ConfigColor col2, ImDrawListPtr drawList)
        {
            speed = Math.Abs(speed);
            int mod = speed == 0 ? 1 : (int)(250 / speed);
            float prog = (float)(DateTimeOffset.Now.ToUnixTimeMilliseconds() % mod) / mod;

            float offset = thickness / 2 + thickness % 2;
            Vector2 pad = new Vector2(offset);
            Vector2 c1 = new Vector2(pos.X, pos.Y);
            Vector2 c2 = new Vector2(pos.X + size.X, pos.Y);
            Vector2 c3 = new Vector2(pos.X + size.X, pos.Y + size.Y);
            Vector2 c4 = new Vector2(pos.X, pos.Y + size.Y);

            DrawHelpers.DrawSegmentedLineHorizontal(drawList, c1, size.X, thickness, prog, segments, col1, col2);
            DrawHelpers.DrawSegmentedLineVertical(drawList, c2.AddX(-thickness), thickness, size.Y, prog, segments, col1, col2);
            DrawHelpers.DrawSegmentedLineHorizontal(drawList, c3.AddY(-thickness), -size.X, thickness, prog, segments, col1, col2);
            DrawHelpers.DrawSegmentedLineVertical(drawList, c4, thickness, -size.Y, prog, segments, col1, col2);
        }

        private void DrawIconGlowRound(Vector2 pos, Vector2 size, int thickness, int segments, float speed, ConfigColor col1, ConfigColor col2, ImDrawListPtr drawList)
        {
            BarStyleConfig style = this.BarStyleConfig;

            // Get the center value of the bar to place rounded ends correctly
            var circleY = (size.Y / 2);
            var circleX = (size.X / 2);
            Vector2 horzCircle = new Vector2(circleY, circleY);
            Vector2 vertCircle = new Vector2(circleX, circleX);
            Vector2 horzcircleOffset = new Vector2(circleY, 0);
            Vector2 vertcircleOffset = new Vector2(0, circleX);

            speed = Math.Abs(speed);
            int mod = speed == 0 ? 1 : (int)(250 / speed);
            float prog = (float)(DateTimeOffset.Now.ToUnixTimeMilliseconds() % mod) / mod;

            float offset = thickness / 2 + thickness % 2;
            Vector2 pad = new Vector2(offset);
            Vector2 c1 = new Vector2(pos.X, pos.Y);
            Vector2 c2 = new Vector2(pos.X + size.X, pos.Y);
            Vector2 c3 = new Vector2(pos.X + size.X, pos.Y + size.Y);
            Vector2 c4 = new Vector2(pos.X, pos.Y + size.Y);

            if (size.X > size.Y)
            {
                DrawHelpers.DrawSegmentedLineHorizontal(drawList, c1 + horzcircleOffset, size.X - size.Y, thickness, prog, segments, col1, col2);
                DrawHelpers.DrawSegmentedLineHorizontal(drawList, (c3.AddY(-thickness) - horzcircleOffset), -size.X + size.Y, thickness, prog, segments, col1, col2);
            }
            else
            {
                DrawHelpers.DrawSegmentedLineVertical(drawList, c1 + vertcircleOffset, thickness, size.Y - size.X, prog, segments, col1, col2);
                DrawHelpers.DrawSegmentedLineVertical(drawList, (c3.AddX(-thickness) - vertcircleOffset), thickness, -size.Y + size.X, prog, segments, col1, col2);
            }
        }

        public void Reposition(Vector2 position, Vector2 iconposition, bool conditions, int AuraCount)
        {
            BarStyleConfig style = this.BarStyleConfig;
            position = new Vector2((position.X * AuraCount), (position.Y * AuraCount));
            iconposition = new Vector2(position.X - 30, position.Y - 9);

            this.BarStyleConfig.Position = position;

            if (conditions)
            {
                foreach (var condition in this.StyleConditions.Conditions)
                {
                    condition.Style.Position = position;
                    condition.Style.IconPosition = position;
                }
            }
        }
        public void UnReposition(Vector2 position, Vector2 barposition, bool conditions, int AuraCount)
        {
            position = new Vector2(position.X, position.Y);

            this.BarStyleConfig.Position = position;

            if (conditions)
            {
                foreach (var condition in this.StyleConditions.Conditions)
                {
                    condition.Style.Position = position;
                }
            }
        }

        public void Resize(Vector2 size, bool conditions)
        {
            this.BarStyleConfig.Size = size;

            if (conditions)
            {
                foreach (var condition in this.StyleConditions.Conditions)
                {
                    condition.Style.Size = size;
                }
            }
        }

        public void ScaleResolution(Vector2 scaleFactor, bool positionOnly)
        {
            this.BarStyleConfig.Position *= scaleFactor;

            if (!positionOnly)
                this.BarStyleConfig.Size *= scaleFactor;

            foreach (var condition in this.StyleConditions.Conditions)
            {
                condition.Style.Position *= scaleFactor;

                if (!positionOnly)
                    condition.Style.Size *= scaleFactor;
            }
        }

        public static AuraIcon GetDefaultAuraIcon(string name)
        {
            AuraIcon newIcon = new AuraIcon(name);
            newIcon.ImportPage(newIcon.LabelListConfig.GetDefault());
            return newIcon;
        }
    }
}