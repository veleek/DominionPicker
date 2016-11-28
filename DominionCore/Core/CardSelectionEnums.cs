namespace Ben.Dominion
{
    public enum PlatinumColonyOption
    {
        /// <summary>
        /// Never show Platinum and Colony in the list of required cards.
        /// </summary>
        Never,
        /// <summary>
        /// Using the propsperity rules, randomly select a card from the set of Kingdom cards 
        /// and if that card is from Prosperity, include Platinum/Colony
        /// </summary>
        Randomly,
        /// <summary>
        /// If you are using any cards from Prosperity, always include Platinum and Colony.
        /// </summary>
        AlwaysWithProsperity,
        /// <summary>
        /// Always show Platinum and Colony in the list of required cards.
        /// </summary>
        Always
    }

    public enum SheltersOption
    {
        /// <summary>
        /// Never select Shelters instead of Estates.
        /// </summary>
        Never,
        /// <summary>
        /// As per the Dark Ages rules, randomly select a card from the set of Kingdom cards 
        /// and if that card is from Dark Ages, use Shelters.
        /// </summary>
        Randomly,
        /// <summary>
        /// If you are using any cards from Dark Ages, always use Shelters.
        /// </summary>
        AlwaysWithDarkAges,
        /// <summary>
        /// Always use Shelters instead of Estates.
        /// </summary>
        Always,
    }

    /// <summary>
    /// Options for determining whether or not to select event cards to be included in the set.
    /// Note: Event cards will only be selected if you have indicated that you own any event cards.
    /// </summary>
    public enum EventsOption
    {
        /// <summary>
        /// Never select any Events.
        /// </summary>
        Never,
        /// <summary>
        /// Randomly select between 0 and 2 Events.
        /// </summary>
        Randomly,
        /// <summary>
        /// Randomly select between 0 and 2 Events but only if using any cards from a set that
        /// contains Events.
        /// </summary>
        RandomlyWithSet,
        /// <summary>
        /// Always select two Events.
        /// </summary>
        Always,
        /// <summary>
        /// Always select two Events but only if using any cards from a set that contains Events.
        /// </summary>
        AlwaysWithSet,
    }

    /// <summary>
    /// Options for setting various card requirements for generated sets.
    /// </summary>
    public enum CardRequirementOption
    {
        /// <summary>
        /// Don't take this option into account when generating a set.
        /// </summary>
        NoPreference,
        /// <summary>
        /// Require at least one card with a specific property.
        /// </summary>
        Require,
        /// <summary>
        /// Require at least one card with +2 of a specific property.
        /// </summary>
        RequirePlus2,
        /// <summary>
        /// Prevent cards with a specific property.
        /// </summary>
        Prevent,
        /// <summary>
        /// Prevent cards with +2 of a specific property.
        /// </summary>
        PreventPlus2,
    }

    public enum Token
    {
        None,
        Coin,
        Victory,
        Embargo,
        Plus1Card,
        Plus1Action,
        Plus1Buy,
        Plus1Coin,
        Minus1Card,
        Minus2Cost,
        Minus1Coin,
        Journey,
        Trashing,
        Estate,
    }

    public enum PlayerMat
    {
        None,
        Island,
        PirateShip,
        TradeRoute,
        NativeVillage,
        Tavern,
    }

    public enum AdditionalThing
    {
        NoBaneAvailable,
    }
}