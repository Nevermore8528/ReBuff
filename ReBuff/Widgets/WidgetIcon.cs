using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Xml;
using ImGuiNET;
using ReBuff.Config;
using ReBuff.Helpers;

namespace ReBuff.Widgets
{
    public class WidgetIcon : WidgetListItem
    {
        public override WidgetType Type => WidgetType.Icon;

        public IconStyleConfig IconStyleConfig { get; set; }
        public LabelListConfig LabelListConfig { get; set; }
        public TriggerConfig TriggerConfig { get; set; }
        public StyleConditions<IconStyleConfig> StyleConditions { get; set; }
        public VisibilityConfig VisibilityConfig { get; set; }

        // Constructor for deserialization
        public WidgetIcon() : this(string.Empty) { }

        public WidgetIcon(string name) : base(name)
        {
            this.IconStyleConfig = new IconStyleConfig();
            this.LabelListConfig = new LabelListConfig();
            this.TriggerConfig = new TriggerConfig();
            this.StyleConditions = new StyleConditions<IconStyleConfig>();
            this.VisibilityConfig = new VisibilityConfig();
        }

        public override IEnumerable<IConfigPage> GetConfigPages()
        {
            yield return this.IconStyleConfig;
            yield return this.LabelListConfig;
            yield return this.TriggerConfig;

            // ugly hack
            this.StyleConditions.UpdateTriggerCount(this.TriggerConfig.TriggerOptions.Count);
            this.StyleConditions.UpdateDefaultStyle(this.IconStyleConfig);

            yield return this.StyleConditions;
            yield return this.VisibilityConfig;
        }

        public override void ImportPage(IConfigPage page)
        {
            switch (page)
            {
                case IconStyleConfig newPage:
                    this.IconStyleConfig = newPage;
                    break;
                case LabelListConfig newPage:
                    this.LabelListConfig = newPage;
                    break;
                case TriggerConfig newPage:
                    this.TriggerConfig = newPage;
                    break;
                case StyleConditions<IconStyleConfig> newPage:
                    newPage.UpdateTriggerCount(0);
                    newPage.UpdateDefaultStyle(this.IconStyleConfig);
                    this.StyleConditions = newPage;
                    break;
                case VisibilityConfig newPage:
                    this.VisibilityConfig = newPage;
                    break;
            }
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
            IconStyleConfig style = this.StyleConditions.GetStyle(datas, triggeredIndex) ?? this.IconStyleConfig;

            Vector2 localPos = pos + style.Position;
            Vector2 barlocalPos = pos + style.BarPosition;
            Vector2 size = style.Size;
            Vector2 barsize = style.BarSize;

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
                // drawList.AddRectFilledMultiColor(barlocalPos, barlocalPos + barsize, style.IconColor.Base, style.IconColor2.Base, style.IconColor3.Base, style.IconColor4.Base);

                if (style.IconOption == 4)
                {
                    DrawHelpers.DrawInWindow($"##{this.ID}Bar", barlocalPos, barsize, this.Preview, this.SetPosition, (drawList2) =>
                {
                    if (this.Preview)
                    {
                        data = this.UpdatePreviewData(data);
                        if (this.LastFrameWasDragging)
                        {
                            barlocalPos = ImGui.GetWindowPos();
                            style.Position = barlocalPos - pos;
                        }
                    }

                    drawList2.AddRectFilledMultiColor(barlocalPos, barlocalPos + barsize, style.IconColor.Base, style.IconColor2.Base, style.IconColor3.Base, style.IconColor4.Base);
                });
                }

                DrawHelpers.DrawInWindow($"##{this.ID}", localPos - size, size*2, this.Preview, this.SetPosition, (drawList) =>
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

                    if (style.IconOption == 2)
                    {
                        return;
                    }

                    bool desaturate = style.DesaturateIcon;
                    float alpha = style.Opacity;

                    if (style.IconOption == 3)
                    {
                        drawList.AddRectFilled(localPos, localPos + size, style.IconColor.Base);
                    }

                    if (style.IconOption == 6)
                    {
                        var ratio = 0f;
                        var overthreshold = false;
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

                        if (style.UnitOption == 6)
                            ratio = 100;
                        else if (style.UnitOption < 4 && style.UnitOption != 1)
                        {
                            ratio = ((style.Unit/style.MaxUnit) * 100);
                        }
                        else if (style.UnitOption == 1 || style.UnitOption >= 4)
                        {
                            ratio = style.Unit;
                        }

                        if (ratio > style.Threshold)
                        {
                            overthreshold = true;
                        }
                        else
                        {
                            overthreshold = false;
                        }

                        // treat localpos as the centerpoint of the quad
                        var getlocalPosX = localPos.X;
                        var getlocalPosY = localPos.Y;
                        Vector2 quadN = new Vector2 (getlocalPosX, getlocalPosY + size.X);
                        Vector2 quadE = new Vector2(getlocalPosX + size.X, getlocalPosY);
                        Vector2 quadS = new Vector2(getlocalPosX, getlocalPosY - size.X);
                        Vector2 quadW = new Vector2(getlocalPosX - size.X, getlocalPosY);

                        if (overthreshold == true)
                        {
                            drawList.AddQuadFilled(quadN, quadE, quadS, quadW, style.IconColor.Base);
                        } else
                        {
                            drawList.AddQuadFilled(quadN, quadE, quadS, quadW, style.IconColor2.Base);
                        }
                    }

                    else
                    {
                        ushort icon = style.IconOption switch
                        {
                            0 => data.Icon,
                            1 => style.CustomIcon,
                            4 => data.Icon,
                            _ => 0
                        };

                        if (icon > 0)
                        {
                            DrawHelpers.DrawIcon(icon, localPos, size, style.CropIcon, 0, desaturate, alpha, drawList);
                        }
                    }

                    if (style.ShowProgressSwipe)
                    {
                        if (style.GcdSwipe && (data.Value == 0 || data.MaxValue == 0 || style.GcdSwipeOnly))
                        {
                            ActionHelpers.GetGCDInfo(out var recastInfo);
                            DrawProgressSwipe(style, localPos, size, recastInfo.RecastTime - recastInfo.RecastTimeElapsed, recastInfo.RecastTime, alpha, drawList);
                        }
                        else
                        {
                            DrawProgressSwipe(style, localPos, size, data.Value, data.MaxValue, alpha, drawList);
                        }
                    }

                    if (style.ShowBorder)
                    {
                        for (int i = 0; i < style.BorderThickness; i++)
                        {
                            Vector2 offset = new Vector2(i, i);
                            Vector4 color = style.BorderColor.Vector.AddTransparency(alpha);

                            if (style.IconOption != 6)
                            {
                                drawList.AddRect(localPos + offset, localPos + size - offset, ImGui.ColorConvertFloat4ToU32(color));
                            }

                            if (style.IconOption == 6)
                            {
                                // treat localpos as the centerpoint of the quad
                                var getlocalPosX = localPos.X;
                                var getlocalPosY = localPos.Y;
                                Vector2 quadN = new Vector2(getlocalPosX, getlocalPosY + size.X - offset.X);
                                Vector2 quadE = new Vector2(getlocalPosX + size.X - offset.X, getlocalPosY);
                                Vector2 quadS = new Vector2(getlocalPosX, getlocalPosY - size.X + offset.X);
                                Vector2 quadW = new Vector2(getlocalPosX - size.X + offset.X, getlocalPosY);

                                drawList.AddQuad(quadN, quadE, quadS, quadW, ImGui.ColorConvertFloat4ToU32(color));
                            }
                        }
                    }

                    if (style.Glow)
                    {
                        this.DrawIconGlow(localPos, size, style.GlowThickness, style.GlowSegments, style.GlowSpeed, style.GlowColor, style.GlowColor2, drawList);
                    }
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

        private static void DrawProgressSwipe(
            IconStyleConfig style,
            Vector2 pos,
            Vector2 size,
            float triggeredValue,
            float startValue,
            float alpha,
            ImDrawListPtr drawList)
        {
            if (startValue > 0 && triggeredValue != 0)
            {
                bool invert = style.InvertSwipe;
                float percent = (invert ? 0 : 1) - (startValue - triggeredValue) / startValue;

                float radius = (float)Math.Sqrt(Math.Pow(Math.Max(size.X, size.Y), 2) * 2) / 2f;
                float startAngle = -(float)Math.PI / 2;
                float endAngle = startAngle - 2f * (float)Math.PI * percent;

                ImGui.PushClipRect(pos, pos + size, false);
                drawList.PathArcTo(pos + size / 2, radius / 2, startAngle, endAngle, (int)(100f * Math.Abs(percent)));
                uint progressAlpha = (uint)(style.ProgressSwipeOpacity * 255 * alpha) << 24;
                drawList.PathStroke(progressAlpha, ImDrawFlags.None, radius);
                if (style.ShowSwipeLines)
                {
                    Vector2 vec = new Vector2((float)Math.Cos(endAngle), (float)Math.Sin(endAngle));
                    Vector2 start = pos + size / 2;
                    Vector2 end = start + vec * radius;
                    float thickness = style.ProgressLineThickness;
                    Vector4 swipeLineColor = style.ProgressLineColor.Vector.AddTransparency(alpha);
                    uint color = ImGui.ColorConvertFloat4ToU32(swipeLineColor);

                    drawList.AddLine(start, end, color, thickness);
                    drawList.AddLine(start, new(pos.X + size.X / 2, pos.Y), color, thickness);
                    drawList.AddCircleFilled(start + new Vector2(thickness / 4, thickness / 4), thickness / 2, color);
                }

                ImGui.PopClipRect();
            }
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

        public void Reposition(Vector2 position, Vector2 barposition, bool conditions, int WidgetCount)
        {
                position = new Vector2((position.X * WidgetCount), (position.Y * WidgetCount));

                this.IconStyleConfig.Position = position;

                if (conditions)
                {
                    foreach (var condition in this.StyleConditions.Conditions)
                    {
                        condition.Style.Position = position;
                    }
                }
        }
        public void UnReposition(Vector2 position, Vector2 barposition, bool conditions, int WidgetCount)
        {
            position = new Vector2(position.X, position.Y);

            this.IconStyleConfig.Position = position;

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
            this.IconStyleConfig.Size = size;

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
            this.IconStyleConfig.Position *= scaleFactor;

            if (!positionOnly)
                this.IconStyleConfig.Size *= scaleFactor;
                
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