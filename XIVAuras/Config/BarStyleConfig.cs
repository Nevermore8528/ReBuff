using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using ImGuiNET;
using Lumina.Excel.GeneratedSheets;
using Newtonsoft.Json;
using XIVAuras.Config;
using XIVAuras.Helpers;

namespace XIVAuras.Config
{
    public class BarStyleConfig : IConfigPage
    {
        [JsonIgnore]
        public string Name => "Bar";

        [JsonIgnore] private string _labelInput = string.Empty;
        [JsonIgnore] private string _iconSearchInput = string.Empty;
        [JsonIgnore] private List<TriggerData> _iconSearchResults = new List<TriggerData>();
        [JsonIgnore] Vector2 _screenSize = ImGui.GetMainViewport().Size;
        [JsonIgnore] private string[] _anchorOptions = Enum.GetNames(typeof(DrawAnchor));

        public DrawAnchor ParentAnchor = DrawAnchor.Center;
        public Vector2 Position = new Vector2(0, 0);
        public Vector2 IconPosition = new Vector2(0, 0);
        public Vector2 BarPosition = new Vector2(0, 0);
        public Vector2 IconSize = new Vector2(30, 42);
        public Vector2 Size = new Vector2(150, 26);
        public Vector2 Spread = new Vector2(150, 0);
        public Vector2 Skew = new Vector2(0, 0);
        public bool ShowBorder = false;
        public int BorderThickness = 1;
        public ConfigColor BorderColor = new ConfigColor(0, 0, 0, 1);
        public bool ShowBorder2 = false;
        public int BorderThickness2 = 1;
        public ConfigColor BorderColor2 = new ConfigColor(0, 0, 0, 1);
        public bool ShowProgressSwipe = false;
        public float ProgressSwipeOpacity = 0.6f;
        public bool InvertSwipe = false;
        public bool ShowSwipeLines = false;
        public ConfigColor ProgressLineColor = new ConfigColor(1, 1, 1, 1);
        public int ProgressLineThickness = 2;
        public bool GcdSwipe = false;
        public bool GcdSwipeOnly = false;

        public bool DesaturateIcon = false;
        public float IconRounding = 0;
        public bool Rounding = false;
        public bool ColorMode = false;
        public bool ValueColor = false;
        public bool IsVertical = false;
        public float Opacity = 1f;
        public float SkewX = 0;
        public float SkewY = 0;
        public float Offset = 0;

        public int IconOption = 11;
        public uint CustomIcon = 0;
        public int AutoIcon = 0;
        public bool CropIcon = false;
        public bool Direction = false;
        public int Stacks = 0;
        public bool ShowStacks = false;

        public bool Glow = false;
        public int GlowThickness = 2;
        public int GlowSegments = 8;
        public float GlowSpeed = 1f;
        public ConfigColor GlowColor = new ConfigColor(230f / 255f, 150f / 255f, 0f / 255f, 1f);
        public ConfigColor GlowColor2 = new ConfigColor(0f / 255f, 0f / 255f, 0f / 255f, 0f);

        public ConfigColor IconColor = new ConfigColor(1, 0, 0, 1);
        public ConfigColor IconColor2 = new ConfigColor(1, 0, 0, 1);
        public ConfigColor IconColor3 = new ConfigColor(1, 0, 0, 1);
        public ConfigColor IconColor4 = new ConfigColor(1, 0, 0, 1);
        public ConfigColor IconColor5 = new ConfigColor(1, 0, 0, 1);
        public ConfigColor IconColor6 = new ConfigColor(1, 0, 0, 1);
        // Reserved for no-alpha
        public ConfigColor naIconColor = new ConfigColor(1, 0, 0, 1);
        public ConfigColor naIconColor2 = new ConfigColor(1, 0, 0, 1);
        public ConfigColor naIconColor3 = new ConfigColor(1, 0, 0, 1);
        public ConfigColor naIconColor4 = new ConfigColor(1, 0, 0, 1);
        public ConfigColor naIconColor5 = new ConfigColor(1, 0, 0, 1);
        public ConfigColor naIconColor6 = new ConfigColor(1, 0, 0, 1);

        public float Unit = 100;
        public float MaxUnit = 0;
        public int UnitOption = 5;
        public bool Segmented = false;

        public string orientation1 = "Right";
        public string orientation2 = "Left";

        public int DataSourceOptions;
        public int JobValue;
        [JsonIgnore] private static readonly string[] _jobOptions = new[] { 
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
        [JsonIgnore]
        private static readonly string[] _indicatorOptions = new[] {
        "Diamond", "Circle", "Chevron", "Vertical Chevron",
        };
        public int indicatorValue;
        public bool Chevron = false;
        public bool VertChevron = false;

        public IConfigPage GetDefault() => new IconStyleConfig();

        public void DrawConfig(IConfigurable parent, Vector2 size, float padX, float padY)
        {
            if (ImGui.BeginChild("##BarStyleConfig", new Vector2(size.X, size.Y), true))
            {
                float height = 50;
                if ((this.IconOption == 13 || this.IconOption == 14) && this.CustomIcon > 0)
                {
                    Vector2 iconPos = ImGui.GetWindowPos() + new Vector2(padX, padX);
                    Vector2 iconSize = new Vector2(height, height);
                    this.DrawIconPreview(iconPos, iconSize, this.CustomIcon, this.CropIcon, this.DesaturateIcon, false);
                    ImGui.GetWindowDrawList().AddRect(
                        iconPos,
                        iconPos + iconSize,
                        ImGui.ColorConvertFloat4ToU32(ImGui.GetStyle().Colors[(int)ImGuiCol.Border]));

                    ImGui.SetCursorPosX(ImGui.GetCursorPosX() + height + padX);
                }

                ImGui.RadioButton("Indicator", ref this.IconOption, 6);
                ImGui.SameLine();
                ImGui.RadioButton("Multi Indicator", ref this.IconOption, 7);
                ImGui.SameLine();
                ImGui.RadioButton("Solid Color", ref this.IconOption, 11);
                ImGui.SameLine();
                ImGui.RadioButton("Multi Color", ref this.IconOption, 12);
                ImGui.SameLine();
                ImGui.RadioButton("Solid + Icon", ref this.IconOption, 13);
                ImGui.SameLine();
                ImGui.RadioButton("Multi + Icon", ref this.IconOption, 14);

                // Custom Icon
                if (this.IconOption == 13 || this.IconOption == 14)
                {
                    float width = ImGui.CalcItemWidth();
                    if (this.CustomIcon > 0)
                    {
                        width -= height + padX;
                        ImGui.SetCursorPosX(ImGui.GetCursorPosX() + height + padX);
                    }

                    if (AutoIcon > 0)
                    {
                        ImGui.PushItemWidth(width);
                        if (ImGui.InputTextWithHint("Search", "Search Icons by Name or ID", ref _iconSearchInput, 32, ImGuiInputTextFlags.EnterReturnsTrue))
                        {
                            _iconSearchResults.Clear();
                            if (ushort.TryParse(_iconSearchInput, out ushort iconId))
                            {
                                _iconSearchResults.Add(new TriggerData("", 0, iconId));
                            }
                            else if (!string.IsNullOrEmpty(_iconSearchInput))
                            {
                                _iconSearchResults.AddRange(ActionHelpers.FindActionEntries(_iconSearchInput));
                                _iconSearchResults.AddRange(StatusHelpers.FindStatusEntries(_iconSearchInput));
                                _iconSearchResults.AddRange(ActionHelpers.FindItemEntries(_iconSearchInput));
                            }
                        }
                        ImGui.PopItemWidth();


                        if (_iconSearchResults.Any() && ImGui.BeginChild("##IconPicker", new Vector2(size.X - padX * 2, 60), true))
                        {
                            List<uint> icons = _iconSearchResults.Select(t => t.Icon).Distinct().ToList();
                            for (int i = 0; i < icons.Count; i++)
                            {
                                Vector2 iconPos = ImGui.GetWindowPos().AddX(10) + new Vector2(i * (40 + padX), padY);
                                Vector2 iconSize = new Vector2(40, 40);
                                this.DrawIconPreview(iconPos, iconSize, icons[i], this.CropIcon, false, true);

                                if (ImGui.IsMouseHoveringRect(iconPos, iconPos + iconSize))
                                {
                                    if (ImGui.IsMouseClicked(ImGuiMouseButton.Left))
                                    {
                                        this.CustomIcon = icons[i];
                                        _iconSearchResults.Clear();
                                        _iconSearchInput = string.Empty;
                                    }
                                }
                            }

                            ImGui.EndChild();
                        }
                    }
                    ImGui.RadioButton("Automatic Icon", ref this.AutoIcon, 0);
                    ImGui.SameLine();
                    ImGui.RadioButton("Custom Icon", ref this.AutoIcon, 1);
                    ImGui.SameLine();
                    ImGui.Checkbox("Crop Icon", ref this.CropIcon);
                    ImGui.SameLine();
                    ImGui.Checkbox("Desaturate Icon", ref this.DesaturateIcon);

                    /* Get back to this eventually, I'm tired of seeing the wrong stack count lmao
                    ImGui.Checkbox("Show Stacks", ref this.ShowStacks);
                    if (this.ShowStacks)
                    {
                        ImGui.SliderInt("Stack Count", ref this.Stacks, 0, 16);
                        ImGui.BulletText("Getting the stack count automatically breaks some icons,");
                        ImGui.BulletText("please use this and conditional visibility as a workaround.");
                    }
                    if (this.ShowStacks == false) { this.Stacks = 0; } */
                    ImGui.Combo("Icon Anchor", ref Unsafe.As<DrawAnchor, int>(ref this.ParentAnchor), _anchorOptions, _anchorOptions.Length);

                }

                if (IsVertical && (this.IconOption == 12 || this.IconOption == 14))
                {
                    orientation1 = "Right";
                    orientation2 = "Left";
                }
                else if (IsVertical == false && (this.IconOption == 12 || this.IconOption == 14))
                {
                    orientation1 = "Top";
                    orientation2 = "Bottom";
                }
                else if (this.ValueColor && (this.IconOption == 11 || this.IconOption == 13 || this.IconOption < 11))
                {
                    orientation1 = "Low Value";
                    orientation2 = "High Value";
                }
                else if (this.ValueColor == false && (this.IconOption == 11 || this.IconOption == 13 || this.IconOption < 11))
                {
                    orientation1 = "Foreground";
                    orientation2 = "Unused";
                }
                
                if (this.IconOption < 11)
                {
                    if ((this.UnitOption >= 600 && this.UnitOption < 700) || this.UnitOption == 2001)
                    {
                        Vector4 vector5 = this.IconColor5.Vector;
                        ImGui.ColorEdit4("Background Color", ref vector5, ImGuiColorEditFlags.AlphaPreview | ImGuiColorEditFlags.AlphaBar);
                        this.IconColor5.Vector = vector5;
                        Vector4 vector4 = this.IconColor4.Vector;
                        ImGui.ColorEdit4($"Solar Color", ref vector4, ImGuiColorEditFlags.AlphaPreview | ImGuiColorEditFlags.AlphaBar);
                        this.IconColor4.Vector = vector4;
                        Vector4 vector2 = this.IconColor2.Vector;
                        ImGui.ColorEdit4($"Lunar Color", ref vector2, ImGuiColorEditFlags.AlphaPreview | ImGuiColorEditFlags.AlphaBar);
                        this.IconColor2.Vector = vector2;
                        if (this.UnitOption != 2001)
                        {
                            Vector4 vector3 = this.IconColor3.Vector;
                            ImGui.ColorEdit4($"Celestial Color", ref vector3, ImGuiColorEditFlags.AlphaPreview | ImGuiColorEditFlags.AlphaBar);
                            this.IconColor3.Vector = vector3;

                            this.naIconColor3.Vector = new Vector4(vector3.X, vector3.Y, vector3.Z, 1);
                        }

                        Vector4 vector = this.IconColor.Vector;
                        //ImGui.ColorEdit4($"True Color", ref vector, ImGuiColorEditFlags.AlphaPreview | ImGuiColorEditFlags.AlphaBar);
                        this.IconColor.Vector = vector;

                        this.naIconColor.Vector = new Vector4(vector.X, vector.Y, vector.Z, 1);
                        this.naIconColor2.Vector = new Vector4(vector2.X, vector2.Y, vector2.Z, 1);
                        this.naIconColor4.Vector = new Vector4(vector4.X, vector4.Y, vector4.Z, 1);
                        this.naIconColor5.Vector = new Vector4(vector5.X, vector5.Y, vector5.Z, 1);

                    }
                    else if (this.UnitOption >= 2002 && this.UnitOption < 2100)
                    {
                        Vector4 vector5 = this.IconColor5.Vector;
                        ImGui.ColorEdit4("Background Color", ref vector5, ImGuiColorEditFlags.AlphaPreview | ImGuiColorEditFlags.AlphaBar);
                        this.IconColor5.Vector = vector5;
                        Vector4 vector4 = this.IconColor4.Vector;
                        ImGui.ColorEdit4($"Coeurl Color", ref vector4, ImGuiColorEditFlags.AlphaPreview | ImGuiColorEditFlags.AlphaBar);
                        this.IconColor4.Vector = vector4;
                        Vector4 vector2 = this.IconColor2.Vector;
                        ImGui.ColorEdit4($"Opo-opo Color", ref vector2, ImGuiColorEditFlags.AlphaPreview | ImGuiColorEditFlags.AlphaBar);
                        this.IconColor2.Vector = vector2;
                        Vector4 vector3 = this.IconColor3.Vector;
                        ImGui.ColorEdit4($"Raptor Color", ref vector3, ImGuiColorEditFlags.AlphaPreview | ImGuiColorEditFlags.AlphaBar);
                        this.IconColor3.Vector = vector3;

                        Vector4 vector = this.IconColor.Vector;
                        //ImGui.ColorEdit4($"True Color", ref vector, ImGuiColorEditFlags.AlphaPreview | ImGuiColorEditFlags.AlphaBar);
                        this.IconColor.Vector = vector;

                        this.naIconColor.Vector = new Vector4(vector.X, vector.Y, vector.Z, 1);
                        this.naIconColor2.Vector = new Vector4(vector2.X, vector2.Y, vector2.Z, 1);
                        this.naIconColor3.Vector = new Vector4(vector3.X, vector3.Y, vector3.Z, 1);
                        this.naIconColor4.Vector = new Vector4(vector4.X, vector4.Y, vector4.Z, 1);
                        this.naIconColor5.Vector = new Vector4(vector5.X, vector5.Y, vector5.Z, 1);

                    }
                    else if (this.UnitOption >= 1602 && this.UnitOption < 1700)
                    {
                        Vector4 vector5 = this.IconColor5.Vector;
                        ImGui.ColorEdit4("Background Color", ref vector5, ImGuiColorEditFlags.AlphaPreview | ImGuiColorEditFlags.AlphaBar);
                        this.IconColor5.Vector = vector5;
                        Vector4 vector4 = this.IconColor4.Vector;
                        ImGui.ColorEdit4($"Emboite Color", ref vector4, ImGuiColorEditFlags.AlphaPreview | ImGuiColorEditFlags.AlphaBar);
                        this.IconColor4.Vector = vector4;
                        Vector4 vector2 = this.IconColor2.Vector;
                        ImGui.ColorEdit4($"Entrechat Color", ref vector2, ImGuiColorEditFlags.AlphaPreview | ImGuiColorEditFlags.AlphaBar);
                        this.IconColor2.Vector = vector2;
                        Vector4 vector3 = this.IconColor3.Vector;
                        ImGui.ColorEdit4($"Jete Color", ref vector3, ImGuiColorEditFlags.AlphaPreview | ImGuiColorEditFlags.AlphaBar);
                        this.IconColor3.Vector = vector3;
                        Vector4 vector6 = this.IconColor6.Vector;
                        ImGui.ColorEdit4($"Pirouette Color", ref vector6, ImGuiColorEditFlags.AlphaPreview | ImGuiColorEditFlags.AlphaBar);
                        this.IconColor6.Vector = vector6;

                        Vector4 vector = this.IconColor.Vector;
                        //ImGui.ColorEdit4($"True Color", ref vector, ImGuiColorEditFlags.AlphaPreview | ImGuiColorEditFlags.AlphaBar);
                        this.IconColor.Vector = vector;

                        this.naIconColor.Vector = new Vector4(vector.X, vector.Y, vector.Z, 1);
                        this.naIconColor2.Vector = new Vector4(vector2.X, vector2.Y, vector2.Z, 1);
                        this.naIconColor3.Vector = new Vector4(vector3.X, vector3.Y, vector3.Z, 1);
                        this.naIconColor4.Vector = new Vector4(vector4.X, vector4.Y, vector4.Z, 1);
                        this.naIconColor5.Vector = new Vector4(vector5.X, vector5.Y, vector5.Z, 1);
                        this.naIconColor6.Vector = new Vector4(vector6.X, vector6.Y, vector6.Z, 1);

                    }
                    else
                    {
                        Vector4 vector5 = this.IconColor5.Vector;
                        ImGui.ColorEdit4("Background Color", ref vector5, ImGuiColorEditFlags.AlphaPreview | ImGuiColorEditFlags.AlphaBar);
                        this.IconColor5.Vector = vector5;
                        Vector4 vector = this.IconColor.Vector;
                        ImGui.ColorEdit4($"{this.orientation1} Color", ref vector, ImGuiColorEditFlags.AlphaPreview | ImGuiColorEditFlags.AlphaBar);
                        this.IconColor.Vector = vector;
                        if (this.ValueColor)
                        {
                            Vector4 vector2 = this.IconColor2.Vector;
                            ImGui.ColorEdit4($"{this.orientation2} Color", ref vector2, ImGuiColorEditFlags.AlphaPreview | ImGuiColorEditFlags.AlphaBar);
                            this.IconColor2.Vector = vector2;
                            this.naIconColor2.Vector = new Vector4(vector2.X, vector2.Y, vector2.Z, 1);
                        }

                        this.naIconColor.Vector = new Vector4(vector.X, vector.Y, vector.Z, 1);
                        this.naIconColor5.Vector = new Vector4(vector5.X, vector5.Y, vector5.Z, 1);
                    }
                }
                else if (this.IconOption == 11 || this.IconOption == 13)
                {
                    Vector4 vector5 = this.IconColor5.Vector;
                    ImGui.ColorEdit4("Background Color", ref vector5, ImGuiColorEditFlags.AlphaPreview | ImGuiColorEditFlags.AlphaBar);
                    this.IconColor5.Vector = vector5;
                    Vector4 vector = this.IconColor.Vector;
                    ImGui.ColorEdit4($"{this.orientation1} Color", ref vector, ImGuiColorEditFlags.AlphaPreview | ImGuiColorEditFlags.AlphaBar);
                    this.IconColor.Vector = vector;
                    if (this.ValueColor)
                    {
                        Vector4 vector2 = this.IconColor2.Vector;
                        ImGui.ColorEdit4($"{this.orientation2} Color", ref vector2, ImGuiColorEditFlags.AlphaPreview | ImGuiColorEditFlags.AlphaBar);
                        this.IconColor2.Vector = vector2;
                        this.naIconColor2.Vector = new Vector4(vector2.X, vector2.Y, vector2.Z, 1);
                    }

                    this.naIconColor.Vector = new Vector4(vector.X, vector.Y, vector.Z, 1);
                    this.naIconColor5.Vector = new Vector4(vector5.X, vector5.Y, vector5.Z, 1);
                }

                else if (this.IconOption == 12 || this.IconOption == 14)
                {
                    Vector4 vector5 = this.IconColor5.Vector;
                    ImGui.ColorEdit4("Background Color", ref vector5, ImGuiColorEditFlags.AlphaPreview | ImGuiColorEditFlags.AlphaBar);
                    this.IconColor5.Vector = vector5;
                    Vector4 vector2 = this.IconColor2.Vector;
                    ImGui.ColorEdit4($"{this.orientation1} Color", ref vector2, ImGuiColorEditFlags.AlphaPreview | ImGuiColorEditFlags.AlphaBar);
                    this.IconColor2.Vector = vector2;
                    this.IconColor3.Vector = vector2;
                    Vector4 vector = this.IconColor.Vector;
                    ImGui.ColorEdit4($"{this.orientation2} Color", ref vector, ImGuiColorEditFlags.AlphaPreview | ImGuiColorEditFlags.AlphaBar);
                    this.IconColor.Vector = vector;
                    this.IconColor4.Vector = vector;

                    this.naIconColor.Vector = new Vector4(vector.X, vector.Y, vector.Z, 1);
                    this.naIconColor2.Vector = new Vector4(vector2.X, vector2.Y, vector2.Z, 1);
                    this.naIconColor3.Vector = new Vector4(vector2.X, vector2.Y, vector2.Z, 1);
                    this.naIconColor4.Vector = new Vector4(vector.X, vector.Y, vector.Z, 1);
                    this.naIconColor5.Vector = new Vector4(vector5.X, vector5.Y, vector5.Z, 1);
                }

                if (this.IconOption >= 11)
                {
                    ImGui.Checkbox("Rounding", ref this.Rounding);
                }
                else
                {
                    ImGui.Combo("##IndicatorCombo", ref this.indicatorValue, _indicatorOptions, _indicatorOptions.Length);
                    if (this.indicatorValue == 0) { this.Rounding = false; this.Chevron = false; this.VertChevron = false; }
                    if (this.indicatorValue == 1) { this.Rounding = true; this.Chevron = false; this.VertChevron = false; }
                    if (this.indicatorValue == 2) { this.Rounding = false; this.Chevron = true; this.VertChevron = false; }
                    if (this.indicatorValue == 3) { this.Rounding = false; this.Chevron = false; this.VertChevron = true; }
                    DrawHelpers.DrawSpacing(1);
                }

                if (this.IconOption == 11 || this.IconOption == 13 || this.IconOption < 11)
                {
                    ImGui.SameLine();
                    ImGui.Checkbox("Color by Value", ref this.ValueColor);
                }

                if (this.IconOption == 12 || this.IconOption == 14 || this.ValueColor)
                {
                    ImGui.SameLine();
                    ImGui.Checkbox("Use HSV", ref this.ColorMode);
                }

                if ((this.Rounding || this.ValueColor) && this.IconOption >= 11)
                {
                    ImGui.BulletText("Foreground color transparency is ignored when Rounding or Color by Value are enabled.");
                    if (this.IconOption == 12 || this.IconOption == 14)
                        ImGui.BulletText("At certain sizes, color clipping may occur at low values.");
                }

                DrawHelpers.DrawSpacing(1);

                if (this.IconOption < 11)
                {
                    ImGui.DragFloat2("Position", ref this.Position, 1, -_screenSize.X / 2, _screenSize.X / 2);
                    ImGui.DragFloat("Radius", ref this.Size.X, 1, 0, _screenSize.Y);
                    if (this.IconOption == 7)
                    {
                        ImGui.DragFloat2("Spread", ref this.Spread, 1, -_screenSize.Y, _screenSize.Y);
                    }
                    if (this.Chevron || this.VertChevron)
                    {
                        ImGui.DragFloat2("Offset", ref this.Skew, 1, 0 -_screenSize.Y, _screenSize.Y);
                    }
                }

                if (this.IconOption == 11 || this.IconOption == 12)
                {
                    ImGui.DragFloat2("Position", ref this.Position, 1, -_screenSize.X / 2, _screenSize.X / 2);
                    ImGui.DragFloat2("Size", ref this.Size, 1, 0, _screenSize.Y);
                }

                if (this.IconOption == 13 || this.IconOption == 14)
                {
                    ImGui.DragFloat2("Bar Position", ref this.Position, 1, -_screenSize.X / 2, _screenSize.X / 2);
                    ImGui.DragFloat2("Bar Size", ref this.Size, 1, 0, _screenSize.Y);
                    DrawHelpers.DrawSpacing(1);
                    ImGui.DragFloat2("Icon Position", ref this.IconPosition, 1, -_screenSize.X / 2, _screenSize.X / 2);
                    ImGui.DragFloat2("Icon Size", ref this.IconSize, 1, 0, _screenSize.Y);
                    ImGui.DragFloat("Icon Opacity", ref this.Opacity, .01f, 0, 1);
                }

                if (this.Size.X > this.Size.Y)
                {
                    IsVertical = true;
                }
                else
                {
                    IsVertical = false;
                }

                DrawHelpers.DrawSpacing(1);
                ImGui.RadioButton("Character Data", ref this.DataSourceOptions, 0);
                ImGui.SameLine();
                ImGui.RadioButton("Action/Status Data", ref this.DataSourceOptions, 1);
                ImGui.SameLine();
                ImGui.RadioButton("Job Gauge Data", ref this.DataSourceOptions, 2);

                if (this.DataSourceOptions == 0)
                {
                    if (this.UnitOption > 3) {
                        if (this.UnitOption >= 100) { this.PrevUnitOption = this.UnitOption; }
                        this.UnitOption = 0; this.PrevJobValue = this.JobValue; }
                    // determine which units we're working with from data
                    ImGui.RadioButton("HP", ref this.UnitOption, 0);
                    ImGui.SameLine();
                    ImGui.RadioButton("MP", ref this.UnitOption, 1);
                    ImGui.SameLine();
                    ImGui.RadioButton("GP", ref this.UnitOption, 2);
                    ImGui.SameLine();
                    ImGui.RadioButton("CP", ref this.UnitOption, 3);
                }
                if (this.DataSourceOptions == 1)
                {
                    if (this.UnitOption < 4 || this.UnitOption > 5) {
                        if (this.UnitOption >= 100) { this.PrevUnitOption = this.UnitOption; }
                        this.UnitOption = 5; this.PrevJobValue = this.JobValue; }
                    // determine which units we're working with from data
                    ImGui.RadioButton("Time", ref this.UnitOption, 5);
                    ImGui.SameLine();
                    ImGui.RadioButton("Stacks", ref this.UnitOption, 4);
                }

                if (this.DataSourceOptions == 2)
                {
                    ImGui.BulletText("This is experimental, not all job gauge data is avaliable yet.");
                    if (this.UnitOption < 100 && this.PrevUnitOption >= 100) { 
                        this.UnitOption = this.PrevUnitOption; this.JobValue = this.PrevJobValue; }
                    // determine which units we're working with from data
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
                        //ImGui.SameLine();
                        //ImGui.RadioButton("Max Timer Duration", ref this.UnitOption, 201);
                        //ImGui.SameLine();
                        //ImGui.RadioButton("Combo Step", ref this.UnitOption, 202);
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
                    }
                    if (this.JobValue == 16) // DNC
                    {
                        ImGui.RadioButton("Feathers", ref this.UnitOption, 1600);
                        ImGui.SameLine();
                        ImGui.RadioButton("Esprit", ref this.UnitOption, 1601);
                        ImGui.SameLine();
                        ImGui.RadioButton("Next Step", ref this.UnitOption, 1602);

                        ImGui.Text("Completed Steps:");
                        ImGui.RadioButton("1st", ref this.UnitOption, 1603);
                        ImGui.SameLine();
                        ImGui.RadioButton("2nd", ref this.UnitOption, 1604);
                        ImGui.SameLine();
                        ImGui.RadioButton("3rd", ref this.UnitOption, 1605);
                        ImGui.SameLine();
                        ImGui.RadioButton("4th", ref this.UnitOption, 1606);
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
                    }
                    if (this.JobValue == 21) // NIN
                    {
                        ImGui.RadioButton("Ninki Gauge", ref this.UnitOption, 2100);
                        ImGui.SameLine();
                        ImGui.RadioButton("Huton Timer", ref this.UnitOption, 2101);
                        //ImGui.SameLine();
                        //ImGui.RadioButton("Huton Count", ref this.UnitOption, 2102);
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

                        /* This doesnt appear to actually be on the job gauge, might discard
                        ImGui.Text("Active Kaeshi");
                        ImGui.SameLine();
                        ImGui.RadioButton("None", ref this.UnitOption, 2305);
                        ImGui.SameLine();
                        ImGui.RadioButton("Higanbana", ref this.UnitOption, 2306);
                        ImGui.SameLine();
                        ImGui.RadioButton("Goken", ref this.UnitOption, 2307);
                        ImGui.SameLine();
                        ImGui.RadioButton("Setsugekka", ref this.UnitOption, 2308);
                        ImGui.SameLine();
                        ImGui.RadioButton("Namikiri", ref this.UnitOption, 2309);*/
                    }
                }

                if (this.IconOption >= 11)
                {
                    DrawHelpers.DrawSpacing(1);
                    ImGui.Checkbox("Reverse Direction", ref this.Direction);
                    ImGui.Checkbox("Glow", ref this.Glow);
                }

                if (this.Glow)
                {
                    DrawHelpers.DrawNestIndicator(1);
                    ImGui.DragInt("Thickness##Glow", ref this.GlowThickness, 1, 1, 16);

                    DrawHelpers.DrawNestIndicator(1);
                    ImGui.DragInt("Glow Segments##Glow", ref this.GlowSegments, 1, 2, 16);

                    DrawHelpers.DrawNestIndicator(1);
                    ImGui.DragFloat("Animation Speed##Glow", ref this.GlowSpeed, 0.05f, 0, 2f);

                    DrawHelpers.DrawNestIndicator(1);
                    Vector4 vector = this.GlowColor.Vector;
                    ImGui.ColorEdit4("Glow Color##Glow", ref vector, ImGuiColorEditFlags.AlphaPreview | ImGuiColorEditFlags.AlphaBar);
                    this.GlowColor.Vector = vector;

                    DrawHelpers.DrawNestIndicator(1);
                    vector = this.GlowColor2.Vector;
                    ImGui.ColorEdit4("Glow Color 2##Glow", ref vector, ImGuiColorEditFlags.AlphaPreview | ImGuiColorEditFlags.AlphaBar);
                    this.GlowColor2.Vector = vector;
                }

                DrawHelpers.DrawSpacing(1);
                ImGui.Checkbox("Show Border", ref this.ShowBorder);
                if (this.ShowBorder)
                {
                    DrawHelpers.DrawNestIndicator(1);
                    ImGui.DragInt("Border Thickness", ref this.BorderThickness, 1, 1, 100);

                    DrawHelpers.DrawNestIndicator(1);
                    Vector4 vector = this.BorderColor.Vector;
                    ImGui.ColorEdit4("Border Color", ref vector, ImGuiColorEditFlags.AlphaPreview | ImGuiColorEditFlags.AlphaBar);
                    this.BorderColor.Vector = vector;
                }
                if (this.IconOption >= 13)
                {
                    ImGui.Checkbox("Show Border on Icon", ref this.ShowBorder2);
                    if (this.ShowBorder2)
                    {
                        DrawHelpers.DrawNestIndicator(1);
                        ImGui.DragInt("Icon Border Thickness", ref this.BorderThickness2, 1, 1, 100);

                        DrawHelpers.DrawNestIndicator(1);
                        Vector4 vector = this.BorderColor2.Vector;
                        ImGui.ColorEdit4("Icon Border Color", ref vector, ImGuiColorEditFlags.AlphaPreview | ImGuiColorEditFlags.AlphaBar);
                        this.BorderColor2.Vector = vector;
                    }
                }

            }

            ImGui.EndChild();
        }

        private void DrawIconPreview(Vector2 iconPos, Vector2 iconSize, uint icon, bool crop, bool desaturate, bool text)
        {
            ImDrawListPtr drawList = ImGui.GetWindowDrawList();
            DrawHelpers.DrawIcon(icon, iconPos, iconSize, crop, 0, desaturate, 1f, drawList);
            if (text)
            {
                string iconText = icon.ToString();
                Vector2 iconTextPos = iconPos + new Vector2(20 - ImGui.CalcTextSize(iconText).X / 2, 38);
                drawList.AddText(iconTextPos, 0xFFFFFFFF, iconText);
            }
        }

    }
}