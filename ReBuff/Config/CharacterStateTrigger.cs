using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using Dalamud.Game.ClientState;
using Dalamud.Game.ClientState.Buddy;
using Dalamud.Game.ClientState.Objects;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.ClientState.Party;
using Dalamud.Logging;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using ImGuiNET;
using PartyListGrabber.System;
using ReBuff.Helpers;
using CharacterStruct = FFXIVClientStructs.FFXIV.Client.Game.Character.Character;

namespace ReBuff.Config
{
    public class CharacterStateTrigger : TriggerOptions
    {

        public Job ShowForJobNames = Helpers.Job.UKN;
        public JobType ShowForJobNames2 = Helpers.JobType.All;
        public string CustomJobString = string.Empty;
        public List<Job> CustomJobList = new List<Job>();

        [JsonIgnore] private string _customJobInput = string.Empty;
        [JsonIgnore] private static readonly string[] _sourceOptions = Enum.GetNames<TriggerSource>();
        [JsonIgnore] private static readonly string[] _petOptions = new[] { "Has Pet", "Has No Pet" };
        [JsonIgnore] private static readonly string[] _keyOptions = new[] { "R1", "L1" };

        [JsonIgnore] private string _hpValueInput = string.Empty;
        [JsonIgnore] private string _mpValueInput = string.Empty;
        [JsonIgnore] private string _levelValueInput = string.Empty;
        [JsonIgnore] private string _cpValueInput = string.Empty;
        [JsonIgnore] private string _gpValueInput = string.Empty;

        public TriggerSource TriggerSource = TriggerSource.Player;

        public override TriggerType Type => TriggerType.CharacterState;
        public override TriggerSource Source => this.TriggerSource;

        public bool IsJob;
        public bool IsRole;

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

        public bool PetCheck;
        public int PetValue;
        public bool controllerState;
        public int keyValue;

        public bool Job;

        public bool HasTarget;
        public bool HasFocus;

    public override bool IsTriggered(bool preview, out DataSource data)
        {
            data = new DataSource();
            
            if (preview)
            {
                return true;
            }

            GameObject ? actor = this.TriggerSource switch
            {
                TriggerSource.Player => Singletons.Get<ClientState>().LocalPlayer,
                TriggerSource.Target => Utils.FindTarget(),
                TriggerSource.TargetOfTarget => Utils.FindTargetOfTarget(),
                TriggerSource.FocusTarget => Singletons.Get<TargetManager>().FocusTarget,
                TriggerSource.Party_1 => Utils.GetPartySlot(0),
                TriggerSource.Party_2 => Utils.GetPartySlot(1),
                TriggerSource.Party_3 => Utils.GetPartySlot(2),
                TriggerSource.Party_4 => Utils.GetPartySlot(3),
                TriggerSource.Party_5 => Utils.GetPartySlot(4),
                TriggerSource.Party_6 => Utils.GetPartySlot(5),
                TriggerSource.Party_7 => Utils.GetPartySlot(6),
                TriggerSource.Party_8 => Utils.GetPartySlot(7),
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
                data.HP_Percent = chara.CurrentHp / chara.MaxHp * 100f;
                data.Mp = chara.CurrentMp;
                data.MaxMp = chara.MaxMp;
                data.Cp = chara.CurrentCp;
                data.MaxCp = chara.MaxCp;
                data.Gp = chara.CurrentGp;
                data.MaxGp = chara.MaxGp;
                data.Level = chara.Level;
                data.Distance = chara.YalmDistanceX;
                if (TriggerSource == TriggerSource.Target)
                {
                    data.HasTarget = Utils.FindTargetOfTarget();
                }
                else
                {
                    data.HasTarget = Utils.FindTarget();
                }
                data.JobNum = chara.ClassJob.Id;
                data.HasFocus = Singletons.Get<TargetManager>().FocusTarget;
                data.HasPet = this.TriggerSource == TriggerSource.Player &&
                    Singletons.Get<BuddyList>().PetBuddy != null;
                unsafe
                {
                    data.Job = (Job)((CharacterStruct*)chara.Address)->ClassJob;
                }
                // determine role here as well 
                if (CharacterState.IsJobType(data.Job, this.ShowForJobNames2))
                {
                    data.IsRole = true;
                }
                if (data.Job == this.ShowForJobNames)
                {
                    data.IsJob = true;
                }
            }

            return preview ||
                (!this.Hp || Utils.GetResult(data.Hp, this.HpOp, this.MaxHp ? data.MaxHp : this.HpValue)) &&
                (!this.Mp || Utils.GetResult(data.Mp, this.MpOp, this.MaxMp ? data.MaxMp : this.MpValue)) &&
                (!this.Cp || Utils.GetResult(data.Cp, this.CpOp, this.MaxCp ? data.MaxCp : this.CpValue)) &&
                (!this.Gp || Utils.GetResult(data.Gp, this.GpOp, this.MaxGp ? data.MaxGp : this.GpValue)) &&
                (!this.Level || Utils.GetResult(data.Level, this.LevelOp, this.LevelValue)) &&
                (!this.HasTarget || data.HasTarget) &&
                (!this.HasFocus || data.HasFocus) &&
                (!this.IsJob|| data.IsJob) &&
                (!this.IsRole || data.IsRole) &&
                (!this.Level || Utils.GetResult(data.Level, this.LevelOp, this.LevelValue)) &&
                (!this.PetCheck || (this.PetValue == 0 ? data.HasPet : !data.HasPet));
        }

        public override void DrawTriggerOptions(Vector2 size, float padX, float padY)
        {
            ImGui.Combo("Trigger Source", ref Unsafe.As<TriggerSource, int>(ref this.TriggerSource), _sourceOptions, _sourceOptions.Length);
            DrawHelpers.DrawSpacing(1);
            
            ImGui.Text("Trigger Conditions");
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
                ImGui.Checkbox("Has Target", ref this.HasTarget);
            }

            if (this.TriggerSource == TriggerSource.Target)
            {
                DrawHelpers.DrawNestIndicator(1);
                ImGui.Checkbox("Has Target", ref this.HasTarget);
            }

            if (this.TriggerSource == TriggerSource.Player)
            {
                DrawHelpers.DrawNestIndicator(1);
                ImGui.Checkbox("Has Focus Target", ref this.HasFocus);
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

            // Figure this out later

            /*
            if (this.TriggerSource == TriggerSource.Player)
            {
                DrawHelpers.DrawNestIndicator(1);
                ImGui.Checkbox("Held Key", ref this.controllerState);
                if (this.controllerState)
                {
                    ImGui.SameLine();
                    padWidth = ImGui.CalcItemWidth() - ImGui.GetCursorPosX() - optionsWidth + padX;
                    ImGui.SetCursorPosX(ImGui.GetCursorPosX() + padWidth);
                    ImGui.PushItemWidth(optionsWidth);
                    ImGui.Combo("##KeyCombo", ref this.keyValue, _keyOptions, _keyOptions.Length);
                    ImGui.PopItemWidth();
                }
            }*/

            DrawHelpers.DrawSpacing();
            string[] jobTypeOptions = Enum.GetNames(typeof(JobName));
            ImGui.Checkbox("Job", ref this.IsJob);

            if (this.IsJob)
            {
                ImGui.SameLine();
                padWidth = ImGui.CalcItemWidth() - ImGui.GetCursorPosX() - optionsWidth + padX;
                ImGui.SetCursorPosX(ImGui.GetCursorPosX() + padWidth);
                ImGui.PushItemWidth(optionsWidth);
                ImGui.Combo("##Is Job", ref Unsafe.As<Job, int>(ref this.ShowForJobNames), jobTypeOptions, jobTypeOptions.Length);
                ImGui.PopItemWidth();
            }

            DrawHelpers.DrawSpacing();
            string[] jobTypeOptions2 = Enum.GetNames(typeof(JobType));
            ImGui.Checkbox("Role", ref this.IsRole);

            if (this.IsRole)
            {
                ImGui.SameLine();
                padWidth = ImGui.CalcItemWidth() - ImGui.GetCursorPosX() - optionsWidth + padX;
                ImGui.SetCursorPosX(ImGui.GetCursorPosX() + padWidth);
                ImGui.PushItemWidth(optionsWidth);
                ImGui.Combo("##Is Role", ref Unsafe.As<JobType, int>(ref this.ShowForJobNames2), jobTypeOptions2, jobTypeOptions2.Length);
                ImGui.PopItemWidth();
            }
        }
    }
}