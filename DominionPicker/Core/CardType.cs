using System;

namespace Ben.Dominion
{
    [Flags]
    public enum CardType
    {
        None = 0x0000,
        Treasure = 0x0001,
        Victory = 0x0002,
        Curse = 0x0004,
        Action = 0x0008,
        Reaction = 0x0010,
        Attack = 0x0020,
        Duration = 0x0040,
        Prize = 0x0080,
        Ruins = 0x0100,
        Shelter = 0x0200,
        Looter = 0x0400,
        Knight = 0x0800,
        Reserve = 0x1000,
        Traveller = 0x2000,
        Event = 0x4000,
    }
}
