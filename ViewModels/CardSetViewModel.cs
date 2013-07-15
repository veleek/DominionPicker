using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Ben.Utilities;
using Ben.Dominion.Resources;

namespace Ben.Dominion
{
    public class CardSetViewModel : NotifyPropertyChangedBase
    {
        private CardSet set;
        private string displayName;

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
                    this.displayName = Strings.ResourceManager.GetString("Set_" + setName, Strings.Culture) ?? "Resource Problem";
                    this.NotifyPropertyChanged("DisplayName");
                }
            }
        }

        public string DisplayName
        {
            get 
            {
                return displayName;
            }
        }

        public static implicit operator CardSetViewModel(CardSet set)
        {
            return new CardSetViewModel(set);
        }
    }
}
