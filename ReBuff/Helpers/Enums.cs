namespace ReBuff.Helpers
{
    public enum CombatType
    {
        PvE,
        PvP
    }

    public enum WidgetType
    {
        Icon,
        Bar,
        Group,
        Label
    }
    
    public enum TriggerType
    {
        Status,
        Cooldown,
        CharacterState,
        ItemCooldown
    }

    public enum TriggerSource
    {
        Player,
        Target,
        TargetOfTarget,
        FocusTarget,
        Party_1,
        Party_2,
        Party_3,
        Party_4,
        Party_5,
        Party_6,
        Party_7,
        Party_8
    }
    
    public enum TriggerCond
    {
        And,
        Or,
        Xor
    }

    public enum TriggerDataSource
    {
        Value,
        Stacks,
        MaxStacks,
        HP,
        MP,
        CP,
        GP,
        Level,
        Distance,
        HPPercent,
        ClassJob
    }

    public enum TriggerDataOp
    {
        Equals,
        NotEquals,
        LessThan,
        GreaterThan,
        LessThanEq,
        GreaterThanEq
    }

    public enum Job
    {
        UKN = 0,

        GLA = 1,
        MRD = 3,
        PLD = 19,
        WAR = 21,
        DRK = 32,
        GNB = 37,

        CNJ = 6,
        WHM = 24,
        SCH = 28,
        AST = 33,
        SGE = 40,

        PGL = 2,
        LNC = 4,
        ROG = 29,
        MNK = 20,
        DRG = 22,
        NIN = 30,
        SAM = 34,
        RPR = 39,

        ARC = 5,
        BRD = 23,
        MCH = 31,
        DNC = 38,

        THM = 7,
        ACN = 26,
        BLM = 25,
        SMN = 27,
        RDM = 35,
        BLU = 36,

        CRP = 8,
        BSM = 9,
        ARM = 10,
        GSM = 11,
        LTW = 12,
        WVR = 13,
        ALC = 14,
        CUL = 15,

        MIN = 16,
        BOT = 17,
        FSH = 18
    }

    public enum JobName
    {
        UKN = 0,

        GLA = 1,
        MRD = 3,
        PLD = 19,
        WAR = 21,
        DRK = 32,
        GNB = 37,

        CNJ = 6,
        WHM = 24,
        SCH = 28,
        AST = 33,
        SGE = 40,

        PGL = 2,
        LNC = 4,
        ROG = 29,
        MNK = 20,
        DRG = 22,
        NIN = 30,
        SAM = 34,
        RPR = 39,

        ARC = 5,
        BRD = 23,
        MCH = 31,
        DNC = 38,

        THM = 7,
        ACN = 26,
        BLM = 25,
        SMN = 27,
        RDM = 35,
        BLU = 36,

        CRP = 8,
        BSM = 9,
        ARM = 10,
        GSM = 11,
        LTW = 12,
        WVR = 13,
        ALC = 14,
        CUL = 15,

        MIN = 16,
        BOT = 17,
        FSH = 18
    }

    public enum JobType
    {
        All,
        Custom,
        Tanks,
        Casters,
        Melee,
        Ranged,
        Healers,
        DoW,
        DoM,
        Combat,
        Crafters,
        DoH,
        DoL,
        Ranged_Card,
        Melee_Card
    }

    public enum CardType
    {
        None = 0,
        Ranged,
        Melee
    }
    
    public enum DrawAnchor
    {
        Center = 0,
        Left = 1,
        Right = 2,
        Top = 3,
        TopLeft = 4,
        TopRight = 5,
        Bottom = 6,
        BottomLeft = 7,
        BottomRight = 8
    }
}
