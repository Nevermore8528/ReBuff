using System.Numerics;
using ImGuiNET;
using Newtonsoft.Json;
using XIVAuras.Auras;

namespace XIVAuras.Config
{
    public class GroupConfig : IConfigPage
    {
        [JsonIgnore] private static Vector2 _screenSize = ImGui.GetMainViewport().Size;

        public string Name => "Group";

        public Vector2 Position = new Vector2(0, 0);

        [JsonIgnore] private Vector2 _iconSize = new Vector2(40, 40);
        public Vector2 _iconPos = new Vector2(0, -40);
        [JsonIgnore] private float _mX = 1f;
        [JsonIgnore] private float _mY = 1f;
        [JsonIgnore] private bool _recusiveResize = false;
        [JsonIgnore] public bool _recusiveSort = false;
        [JsonIgnore] private bool _conditionsResize = false;
        [JsonIgnore] public bool _conditionsSort = false;
        [JsonIgnore] private bool _positionOnly = false;
        public bool _keepsorted = false;
        [JsonIgnore] public int _AuraCount = 0;

        public IConfigPage GetDefault() => new GroupConfig();

        public void DrawConfig(IConfigurable parent, Vector2 size, float padX, float padY)
        {
            if (ImGui.BeginChild("##GroupConfig", new Vector2(size.X, size.Y), true))
            {
                ImGui.DragFloat2("Group Position", ref this.Position);

                ImGui.NewLine();
                ImGui.Text("Resize Icons");
                ImGui.DragFloat2("Icon Size##Size", ref _iconSize, 1, 0, _screenSize.Y);
                ImGui.Checkbox("Recursive##Size", ref _recusiveResize);
                if (ImGui.IsItemHovered())
                {
                    ImGui.SetTooltip("Check to recursively resize icons in sub-groups, doesn't work correctly on bars");
                }

                ImGui.SameLine();
                ImGui.Checkbox("Conditions##Size", ref _conditionsResize);
                if (ImGui.IsItemHovered())
                {
                    ImGui.SetTooltip("Check to resize conditions");
                }

                ImGui.SameLine();
                float padWidth = ImGui.CalcItemWidth() - ImGui.GetCursorPosX() - 60 + padX;
                ImGui.SetCursorPosX(ImGui.GetCursorPosX() + padWidth);
                if (ImGui.Button("Resize", new Vector2(60, 0)))
                {
                    if (parent is AuraGroup g)
                    {
                        g.ResizeIcons(_iconSize, _recusiveResize, _conditionsResize);
                    }
                }

                ImGui.NewLine();
                ImGui.Text("Sort Icons");
                ImGui.DragFloat2("Spacing##Sort", ref _iconPos, 1, -100, 100);
                ImGui.Checkbox("Recursive##Sort", ref _recusiveSort);
                if (ImGui.IsItemHovered())
                {
                    ImGui.SetTooltip("Check to recursively sort icons in sub-groups");
                }

                ImGui.SameLine();
                ImGui.Checkbox("Conditions##Sort", ref _conditionsSort);
                if (ImGui.IsItemHovered())
                {
                    ImGui.SetTooltip("Check to Sort conditions");
                }

                ImGui.SameLine();
                float padWidth2 = ImGui.CalcItemWidth() - ImGui.GetCursorPosX() - 60 + padX;
                ImGui.SetCursorPosX(ImGui.GetCursorPosX() + padWidth2);
                if (ImGui.Button("Sort", new Vector2(60, 0)))
                {
                    if (parent is AuraGroup g)
                    {
                        g.SortIcons(_iconPos, _iconPos, _recusiveSort, _conditionsSort, _AuraCount);
                    }
                }

                ImGui.Checkbox("Continual##Sort", ref _keepsorted);
                if (ImGui.IsItemHovered())
                {
                    ImGui.SetTooltip("Check to keep items sorted based on visibility, independent of groups");
                }
                if (_keepsorted)
                {
                    if (parent is AuraGroup g)
                    {
                        g.SortVisible(_iconPos, _iconPos, _recusiveSort, _conditionsSort, _AuraCount);
                    }
                }

                if (parent is AuraGroup group)
                {
                    ImGui.NewLine();
                    ImGui.Text("Scale Resolution (BACK UP YOUR CONFIG FIRST!)");
                    ImGui.DragFloat("X Multiplier", ref _mX, 0.01f, 0.01f, 100f);
                    ImGui.DragFloat("Y Multiplier", ref _mY, 0.01f, 0.01f, 100f);
                    ImGui.Checkbox("Scale positions only", ref this._positionOnly);
                    ImGui.SameLine();
                    padWidth = ImGui.CalcItemWidth() - ImGui.GetCursorPosX() - 60 + padX;
                    ImGui.SetCursorPosX(ImGui.GetCursorPosX() + padWidth);
                    if (ImGui.Button("Scale", new Vector2(60, 0)))
                    {
                        group.ScaleResolution(new(_mX, _mY), _positionOnly);
                    }
                }


                ImGui.EndChild();
            }
        }
    }
}
