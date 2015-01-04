using System.Collections.Generic;

namespace Ben.Dominion
{
    public class SampleCardList : List<Card>
    {
    }

    public class SampleCardSelectorList : List<CardSelector>
    {
    }

    public class SampleGrouping : List<SampleGroup>
    {
    }

    public class SampleGroup : List<CardSelector>
    {
        public CardSet Key { get; set; }
    }
}