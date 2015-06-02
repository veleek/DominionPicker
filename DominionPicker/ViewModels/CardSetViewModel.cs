using System;
using System.Xml.Serialization;
using Ben.Dominion.Resources;
using Ben.Utilities;

namespace Ben.Dominion
{
    public class CardSetViewModel : NotifyPropertyChangedBase, IEquatable<CardSetViewModel>
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
                if (this.SetProperty(ref this.set, value))
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

		public bool Equals(CardSetViewModel other)
		{
			if (ReferenceEquals(null, other))
			{
				return false;
			}
			if (ReferenceEquals(this, other))
			{
				return true;
			}
			return this.set == other.set;
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj))
			{
				return false;
			}
			if (ReferenceEquals(this, obj))
			{
				return true;
			}
			if (obj.GetType() != this.GetType())
			{
				return false;
			}
			return this.Equals((CardSetViewModel)obj);
		}

		public override int GetHashCode()
		{
			return (int)this.set;
		}
	}
}
