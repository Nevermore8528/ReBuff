using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
//using FFXIVClientStructs.FFXIV.Common.Math;
using ImGuiNET;
using Newtonsoft.Json;
using ReBuff.Helpers;
//using static Lumina.Data.Parsing.Common;

namespace ReBuff.Config
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
        public Vector2 BarSize = new Vector2(150, 26);
        public bool ShowBorder = false;
        public int BorderThickness = 1;
        public ConfigColor BorderColor = new ConfigColor(0, 0, 0, 1);
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

        public int IconOption = 11;
        public ushort CustomIcon = 0;
        public int AutoIcon = 0;
        public bool CropIcon = false;
        public bool Direction = false;

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
        // Reserved for no-alpha
        public ConfigColor naIconColor = new ConfigColor(1, 0, 0, 1);
        public ConfigColor naIconColor2 = new ConfigColor(1, 0, 0, 1);
        public ConfigColor naIconColor3 = new ConfigColor(1, 0, 0, 1);
        public ConfigColor naIconColor4 = new ConfigColor(1, 0, 0, 1);
        public ConfigColor naIconColor5 = new ConfigColor(1, 0, 0, 1);

        public float Unit = 100;
        public float MaxUnit = 0;
        public int UnitOption = 5;

        public string orientation1 = "Right";
        public string orientation2 = "Left";


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
                            List<ushort> icons = _iconSearchResults.Select(t => t.Icon).Distinct().ToList();
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
                    ImGui.Combo("Icon Anchor", ref Unsafe.As<DrawAnchor, int>(ref this.ParentAnchor), _anchorOptions, _anchorOptions.Length);
                }

                if (IsVertical && (this.IconOption == 12 || this.IconOption == 14))
                {
                    orientation1 = "Right";
                    orientation2 = "Left";
                } else if (IsVertical == false && (this.IconOption == 12 || this.IconOption == 14))
                {
                    orientation1 = "Top";
                    orientation2 = "Bottom";
                } else if (this.ValueColor && (this.IconOption == 11 || this.IconOption == 13))
                {
                    orientation1 = "Low Value";
                    orientation2 = "High Value";
                }
                else if (this.ValueColor == false && (this.IconOption == 11 || this.IconOption == 13))
                {
                    orientation1 = "Foreground";
                    orientation2 = "Unused";
                }

                if (this.IconOption == 11 || this.IconOption == 13)
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

                ImGui.Checkbox("Rounding", ref this.Rounding);

                if (this.IconOption == 11 || this.IconOption == 13)
                {
                    ImGui.SameLine();
                    ImGui.Checkbox("Color by Value", ref this.ValueColor);
                }

                if (this.IconOption == 12 || this.IconOption == 14 || this.ValueColor)
                {
                    ImGui.SameLine();
                    ImGui.Checkbox("Use HSV", ref this.ColorMode);
                }

                if (this.Rounding || this.ValueColor)
                {
                    ImGui.BulletText("Foreground color transparency is ignored when Rounding or Color by Value are enabled.");
                    if (this.IconOption == 12 || this.IconOption == 14)
                        ImGui.BulletText("At certain sizes, color clipping may occur at low values.");
                }

                DrawHelpers.DrawSpacing(1);
                if (this.IconOption != 13 && this.IconOption != 14)
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
                } else
                {
                    IsVertical = false;
                }

                if (this.IconOption >= 11 && this.IconOption <= 14)
                {
                        // determine which units we're working with from data
                        DrawHelpers.DrawSpacing(1);
                        ImGui.RadioButton("HP", ref this.UnitOption, 0);
                        ImGui.SameLine();
                        ImGui.RadioButton("MP", ref this.UnitOption, 1);
                        ImGui.SameLine();
                        ImGui.RadioButton("GP", ref this.UnitOption, 2);
                        ImGui.SameLine();
                        ImGui.RadioButton("CP", ref this.UnitOption, 3);
                        ImGui.SameLine();
                        ImGui.RadioButton("Stacks", ref this.UnitOption, 4);
                        ImGui.SameLine();
                        ImGui.RadioButton("Time", ref this.UnitOption, 5);
                }

                ImGui.Checkbox("Reverse Direction", ref this.Direction);

                ImGui.Checkbox("Glow", ref this.Glow);
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

            }

            ImGui.EndChild();
        }

        private void DrawIconPreview(Vector2 iconPos, Vector2 iconSize, ushort icon, bool crop, bool desaturate, bool text)
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