using System.Xml.Serialization;
using Ben.Dominion.Resources;
using Ben.Utilities;

namespace Ben.Dominion
{
    public class CardSetViewModel : NotifyPropertyChangedBase
    {
        private CardSet set;

        public CardSetViewModel()
        {
        }

        public CardSetViewModel(CardSet set)
        {
            this.Set = set;
        }

        public CardSet Set
        {
            get { return this.set; }
            set 
            {
                if (this.SetProperty(ref this.set, value, "Set"))
                {
                    string setName = this.set.ToString();
                    this.DisplayName = CardDataStrings.ResourceManager.GetString("Set_" + setName, Strings.Culture) ?? "Resource Problem";
                    this.NotifyPropertyChanged("DisplayName");
                }
            }
        }

        [XmlIgnore]
        public string DisplayName { get; private set; }

        public static implicit operator CardSetViewModel(CardSet set)
        {
            return new CardSetViewModel(set);
        }
    }
}
