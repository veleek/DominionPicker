using Ben.Dominion.Resources;
using Ben.Utilities;
using System;
using System.Linq;

namespace Ben.Dominion
{
    public class CardTypeViewModel : NotifyPropertyChangedBase
    {
        private static readonly char[] SplitChars = new char[] { ',', ' '};
        private CardType type;

	    public CardTypeViewModel()
        {
        }

        public CardTypeViewModel(CardType type)
        {
            this.Type = type;
        }

        public CardType Type
        {
            get { return this.type; }
            set 
            {
                if (this.SetProperty(ref this.type, value, "Type"))
                {
                    var typeNames = this.type.ToString().Split(SplitChars, StringSplitOptions.RemoveEmptyEntries);
                    this.DisplayName = string.Join(", ", typeNames.Select(n => CardDataStrings.ResourceManager.GetString("Type_" + n, Strings.Culture) ?? "Resource Problem"));
                    this.NotifyPropertyChanged("DisplayName");
                }
            }
        }

        public string DisplayName { get; private set; }

	    public static implicit operator CardTypeViewModel(CardType type)
        {
            return new CardTypeViewModel(type);
        }
    }
}
