namespace Ben.Dominion.Models
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
        /// Nevery select Shelters instead of Estates.
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
