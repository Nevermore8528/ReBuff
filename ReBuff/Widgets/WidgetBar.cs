using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using Dalamud.Interface;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Component.GUI;
using ImGuiNET;
using Lumina.Data.Files.Excel;
using ReBuff.Config;
using ReBuff.Helpers;

namespace ReBuff.Widgets
{
    public class WidgetBar : WidgetListItem
    {
        public override WidgetType Type => WidgetType.Bar;

        public BarStyleConfig BarStyleConfig { get; set; }
        public LabelListConfig LabelListConfig { get; set; }
        public TriggerConfig TriggerConfig { get; set; }
        public StyleConditions<BarStyleConfig> StyleConditions { get; set; }
        public VisibilityConfig VisibilityConfig { get; set; }

        private static Vector2 StartPosition { get; set; }

        // Constuctor for deserialization
        public WidgetBar() : this(string.Empty) { }

        public WidgetBar(string name) : base(name)
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
            Vector2 barsize = style.BarSize;
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

                // ToDo: Vertical Bars
                var IsVertical = false;
                if (size.Y > size.X)
                {
                    IsVertical = true;
                } else if (size.Y <= size.X)
                {
                    IsVertical = false;
                }

                bool desaturate = style.DesaturateIcon;
                float alpha = style.Opacity;
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

                    if (style.IconOption >= 11 && style.IconOption <= 14)
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
                                drawList.AddRectFilled(localPos, localPos + size, style.IconColor5.Base); // Background
                                if (style.IconOption == 11 || style.IconOption == 13)
                                {
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
                                /*drawList.AddCircleFilled((localPos + horzCircle), circleY, style.IconColor.Base); // Left Rounded end
                                drawList.AddCircleFilled((localPos + size - horzCircle), circleY, style.IconColor5.Base); // Right Rounded end
                                drawList.AddRectFilled((localPos + horzcircleOffset), (localPos - horzcircleOffset + size), style.IconColor5.Base); // Background*/

                                drawList.AddRectFilled(localPos, localPos + size, style.IconColor5.Base, 1024); // Background
                                if (style.IconOption == 11 || style.IconOption == 13)
                                {
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
                        } else if (IsVertical)
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
                                drawList.AddRectFilled(localPos, localPos + size, style.IconColor5.Base); // Background
                                if (style.IconOption == 11 || style.IconOption == 13)
                                {
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
                                /*drawList.AddCircleFilled((localPos + horzCircle), circleY, style.IconColor.Base); // Left Rounded end
                                drawList.AddCircleFilled((localPos + size - horzCircle), circleY, style.IconColor5.Base); // Right Rounded end
                                drawList.AddRectFilled((localPos + horzcircleOffset), (localPos - horzcircleOffset + size), style.IconColor5.Base); // Background*/

                                drawList.AddRectFilled(localPos, localPos + size, style.IconColor5.Base, 1024); // Background
                                if (style.IconOption == 11 || style.IconOption == 13)
                                {
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
                            Vector4 color2 = new Vector4(0f, 0f, 0f, alpha/2);

                            if (style.Rounding == false)
                            {
                                drawList.AddRect(localPos + offset, localPos + size - offset, ImGui.ColorConvertFloat4ToU32(color));
                            } else if (style.Rounding == true)
                            {
                                drawList.AddRect(localPos + offset, localPos + size - offset, ImGui.ColorConvertFloat4ToU32(color), 1024);
                            }
                        }
                    }

                    if (style.Glow)
                    {
                        if (style.Rounding)
                        {
                            this.DrawIconGlowRound(localPos, size, style.GlowThickness, style.GlowSegments, style.GlowSpeed, style.GlowColor, style.GlowColor2, drawList);

                        } else
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
                            ushort icon = style.CustomIcon;

                            if (icon > 0)
                            {
                                DrawHelpers.DrawIcon(icon, IconPos, iconsize, style.CropIcon, 0, desaturate, alpha, drawListIcon);
                            }
                        }
                        else if (style.AutoIcon == 0)
                        {
                            ushort icon = data.Icon;

                            if (icon > 0)
                            {
                                DrawHelpers.DrawIcon(icon, IconPos, iconsize, style.CropIcon, 0, desaturate, alpha, drawListIcon);
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

            foreach (WidgetLabel label in this.LabelListConfig.WidgetLabels)
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
            } else
            {
                DrawHelpers.DrawSegmentedLineVertical(drawList, c1 + vertcircleOffset, thickness, size.Y - size.X, prog, segments, col1, col2);
                DrawHelpers.DrawSegmentedLineVertical(drawList, (c3.AddX(-thickness) - vertcircleOffset), thickness, -size.Y + size.X, prog, segments, col1, col2);
            }
        }

        public void Reposition(Vector2 position, Vector2 iconposition, bool conditions, int WidgetCount)
        {
            BarStyleConfig style = this.BarStyleConfig;
            position = new Vector2((position.X * WidgetCount), (position.Y * WidgetCount));
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
        public void UnReposition(Vector2 position, Vector2 barposition, bool conditions, int WidgetCount)
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

        public static WidgetIcon GetDefaultWidgetIcon(string name)
        {
            WidgetIcon newIcon = new WidgetIcon(name);
            newIcon.ImportPage(newIcon.LabelListConfig.GetDefault());
            return newIcon;
        }
    }
}