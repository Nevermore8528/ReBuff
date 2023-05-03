using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using Dalamud.Game;
using Dalamud.Game.ClientState.Objects.SubKinds;
using FFXIVClientStructs.FFXIV.Client.UI;

namespace PartyListGrabber.System;
// Shamelessly stolen from NoTankYou
// I'll figure out how to rework this myself later

public readonly unsafe struct PartyListAddonData
{
    private static readonly Dictionary<uint, Stopwatch> TimeSinceLastTargetable = new();

    public AddonPartyList.PartyListMemberStruct UserInterface { get; init; }
    public PartyMemberData AgentData { get; init; }
    public PlayerCharacter? PlayerCharacter { get; init; }

    private bool Targetable => UserInterface.PartyMemberComponent->OwnerNode->AtkResNode.Color.A != 0x99;

    public bool IsTargetable()
    {
        if (!AgentData.ValidData) return false;

        TimeSinceLastTargetable.TryAdd(AgentData.ObjectID, Stopwatch.StartNew());
        var stopwatch = TimeSinceLastTargetable[AgentData.ObjectID];

        if (Targetable)
        {
            // Returns true if the party member has been targetable for 2second or more
            return stopwatch.Elapsed >= TimeSpan.FromSeconds(2);
        }
        else
        {
            // Returns false, and continually resets the stopwatch
            stopwatch.Restart();
            return false;
        }
    }
}

public readonly struct PartyFramePositionInfo
{
    public Vector2 Position { get; init; }
    public Vector2 Size { get; init; }
    public Vector2 Scale { get; init; }

    public override string ToString() => $"{{Position: {Position}, Size: {Size}, Scale: {Scale}}}";
}

public unsafe class PartyListAddon : IEnumerable<PartyListAddonData>, IDisposable
{
    public IEnumerator<PartyListAddonData> GetEnumerator()
    {
        return addonData.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    private static AddonPartyList* PartyList => (AddonPartyList*)Service.GameGui.GetAddonByName("_PartyList");
    public static bool DataAvailable => PartyList != null && PartyList->AtkUnitBase.RootNode != null;

    private readonly List<PartyListAddonData> addonData = new();

    public PartyListAddon()
    {
        Service.Framework.Update += OnFrameworkUpdate;
    }

    public void Dispose()
    {
        Service.Framework.Update -= OnFrameworkUpdate;
    }

    private void OnFrameworkUpdate(Framework framework)
    {
        addonData.Clear();
        if (!DataAvailable) return;
        if (PartyList->MemberCount <= 0) return;

        foreach (var index in Enumerable.Range(0, PartyList->MemberCount))
        {
            var agentData = HudAgent.GetPartyMember(index);
            var playerCharacter = HudAgent.GetPlayerCharacter(index);
            var userInterface = PartyList->PartyMember[index];

            addonData.Add(new PartyListAddonData
            {
                AgentData = agentData,
                PlayerCharacter = playerCharacter,
                UserInterface = userInterface,
            });
        }
    }
}