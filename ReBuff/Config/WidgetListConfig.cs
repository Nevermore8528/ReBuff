using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using Dalamud.Interface;
using Dalamud.Interface.Internal.Notifications;
using ImGuiNET;
using Newtonsoft.Json;
using ReBuff.Widgets;
using ReBuff.Helpers;

namespace ReBuff.Config
{
    public class WidgetListConfig : IConfigPage
    {
        private const float MenuBarHeight = 40;

        [JsonIgnore] private WidgetType _selectedType = WidgetType.Icon;
        [JsonIgnore] private string _input = string.Empty;
        [JsonIgnore] private string[] _options = new string[] { "Icon", "Bar", "Group" };
        [JsonIgnore] private int _swapX = -1;
        [JsonIgnore] private int _swapY = -1;

        public string Name => "Widgets";

        public List<WidgetListItem> Widgets { get; init; }

        public WidgetListConfig()
        {
            this.Widgets = new List<WidgetListItem>();
        }

        public IConfigPage GetDefault() => new WidgetListConfig();

        public void DrawConfig(IConfigurable parent, Vector2 size, float padX, float padY)
        {
            this.DrawCreateMenu(size, padX);
            this.DrawWidgetTable(size.AddY(-padY), padX);
        }

        private void DrawCreateMenu(Vector2 size, float padX)
        {
            Vector2 buttonSize = new Vector2(40, 0);
            float comboWidth = 100;
            float textInputWidth = size.X - buttonSize.X * 2 - comboWidth - padX * 5;

            if (ImGui.BeginChild("##Buttons", new Vector2(size.X, MenuBarHeight), true))
            {
                ImGui.PushItemWidth(textInputWidth);
                ImGui.InputTextWithHint("##Input", "New Widget Name", ref _input, 100);
                ImGui.PopItemWidth();

                ImGui.SameLine();
                ImGui.PushItemWidth(comboWidth);
                ImGui.Combo("##Type", ref Unsafe.As<WidgetType, int>(ref _selectedType), _options, _options.Length);

                ImGui.SameLine();
                DrawHelpers.DrawButton(string.Empty, FontAwesomeIcon.Plus, () => CreateWidget(_selectedType, _input), "Create new Widget or Group", buttonSize);

                ImGui.SameLine();
                DrawHelpers.DrawButton(string.Empty, FontAwesomeIcon.Download, () => ImportWidget(), "Import new Widget or Group from Clipboard", buttonSize);
                ImGui.PopItemWidth();

                ImGui.EndChild();
            }
        }

        private void DrawWidgetTable(Vector2 size, float padX)
        {
            ImGuiTableFlags flags =
                ImGuiTableFlags.RowBg |
                ImGuiTableFlags.Borders |
                ImGuiTableFlags.BordersOuter |
                ImGuiTableFlags.BordersInner |
                ImGuiTableFlags.ScrollY |
                ImGuiTableFlags.NoSavedSettings;

            if (ImGui.BeginTable("##Widgets_Table", 4, flags, new Vector2(size.X, size.Y - MenuBarHeight)))
            {
                Vector2 buttonSize = new Vector2(30, 0);
                int buttonCount = this.Widgets.Count > 1 ? 5 : 3;
                float actionsWidth = buttonSize.X * buttonCount + padX * (buttonCount - 1);
                float typeWidth = 75;

                ImGui.TableSetupColumn("Name", ImGuiTableColumnFlags.WidthStretch, 0, 0);
                ImGui.TableSetupColumn("Type", ImGuiTableColumnFlags.WidthFixed, typeWidth, 1);
                ImGui.TableSetupColumn("Pre.", ImGuiTableColumnFlags.WidthFixed, 23, 2);
                ImGui.TableSetupColumn("Actions", ImGuiTableColumnFlags.WidthFixed, actionsWidth, 3);

                ImGui.TableSetupScrollFreeze(0, 1);
                ImGui.TableHeadersRow();

                for (int i = 0; i < this.Widgets.Count; i++)
                {
                    WidgetListItem widget = this.Widgets[i];

                    if (!string.IsNullOrEmpty(_input) &&
                        !widget.Name.Contains(_input, StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    ImGui.PushID(i.ToString());
                    ImGui.TableNextRow(ImGuiTableRowFlags.None, 28);

                    if (ImGui.TableSetColumnIndex(0))
                    {
                        ImGui.SetCursorPosY(ImGui.GetCursorPosY() + 3f);
                        ImGui.Text(widget.Name);
                    }

                    if (ImGui.TableSetColumnIndex(1))
                    {
                        ImGui.SetCursorPosY(ImGui.GetCursorPosY() + 3f);
                        ImGui.Text(widget.Type.ToString());
                    }

                    if (ImGui.TableSetColumnIndex(2))
                    {
                        ImGui.SetCursorPosY(ImGui.GetCursorPosY() + 1f);
                        ImGui.Checkbox("##Preview", ref widget.Preview);
                        if (ImGui.IsItemHovered())
                        {
                            ImGui.SetTooltip("Preview");
                        }
                    }

                    if (ImGui.TableSetColumnIndex(3))
                    {
                        ImGui.SetCursorPosY(ImGui.GetCursorPosY() + 1f);
                        DrawHelpers.DrawButton(string.Empty, FontAwesomeIcon.Pen, () => EditWidget(widget), "Edit", buttonSize);

                        if (this.Widgets.Count > 1)
                        {
                            ImGui.SameLine();
                            DrawHelpers.DrawButton(string.Empty, FontAwesomeIcon.ArrowUp, () => Swap(i, i - 1), "Move Up", buttonSize);

                            ImGui.SameLine();
                            DrawHelpers.DrawButton(string.Empty, FontAwesomeIcon.ArrowDown, () => Swap(i, i + 1), "Move Down", buttonSize);
                        }

                        ImGui.SameLine();
                        DrawHelpers.DrawButton(string.Empty, FontAwesomeIcon.Upload, () => ExportWidget(widget), "Export", buttonSize);

                        ImGui.SameLine();
                        DrawHelpers.DrawButton(string.Empty, FontAwesomeIcon.Trash, () => DeleteWidget(widget), "Delete", buttonSize);
                    }
                }

                ImGui.EndTable();
            }

            if (_swapX < this.Widgets.Count && _swapX >= 0 &&
                _swapY < this.Widgets.Count && _swapY >= 0)
            {
                WidgetListItem temp = this.Widgets[_swapX];
                this.Widgets[_swapX] = this.Widgets[_swapY];
                this.Widgets[_swapY] = temp;

                _swapX = -1;
                _swapY = -1;
            }
        }

        private void Swap(int x, int y)
        {
            _swapX = x;
            _swapY = y;
        }

        private void CreateWidget(WidgetType type, string name)
        {
            if (!string.IsNullOrEmpty(name))
            {
                WidgetListItem? newWidget = type switch
                {
                    WidgetType.Group => new WidgetGroup(name),
                    WidgetType.Icon => WidgetIcon.GetDefaultWidgetIcon(name),
                    WidgetType.Bar => new WidgetBar(name),
                    _ => null
                };

                if (newWidget is not null)
                {
                    this.Widgets.Add(newWidget);
                }
            }

            _input = string.Empty;
        }

        private void EditWidget(WidgetListItem widget)
        {
            Singletons.Get<PluginManager>().Edit(widget);
        }

        private void DeleteWidget(WidgetListItem widget)
        {
            this.Widgets.Remove(widget);
        }

        private void ImportWidget()
        {
            string importString = string.Empty;
            try
            {
                importString = ImGui.GetClipboardText();
            }
            catch
            {
                DrawHelpers.DrawNotification("Failed to read from clipboard!", NotificationType.Error);
                return;
            }

            WidgetListItem? newWidget = ConfigHelpers.GetFromImportString<WidgetListItem>(importString);
            if (newWidget is not null)
            {
                this.Widgets.Add(newWidget);
            }
            else
            {
                DrawHelpers.DrawNotification("Failed to Import Widget!", NotificationType.Error);
            }

            _input = string.Empty;
        }

        private void ExportWidget(WidgetListItem widget)
        {
            ConfigHelpers.ExportToClipboard<WidgetListItem>(widget);
        }
    }
}
