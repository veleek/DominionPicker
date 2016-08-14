using System.Collections.Generic;
using Windows.UI.Xaml.Media;

namespace Ben.Dominion.Sample
{
    public class SampleCard : Card
    {
        public new string CoinCost { get; set; }

        public new bool HasPotion { get; set; }

        public new Brush BackgroundColor { get; set; }
    }

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

    public class SampleMainViewModel : MainViewModel
    {
    }
}