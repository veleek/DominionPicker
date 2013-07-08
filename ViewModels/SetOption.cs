using Ben.Utilities;
using System.Xml.Serialization;

namespace Ben.Dominion
{
    public class SetOption : NotifyPropertyChangedBase
    {
        private bool enabled;
        private CardSet set;
        private CardSetViewModel setView;

        [XmlAttribute]
        public bool Enabled
        {
            get { return enabled; }
            set { SetProperty(ref enabled, value, "Enabled"); }
        }

        [XmlAttribute]
        public CardSet Set
        {
            get { return set; }
            set 
            {
                if (SetProperty(ref set, value, "Set"))
                {
                    this.SetView = set;
                }
            }
        }

        [XmlIgnore]
        public CardSetViewModel SetView
        {
            get { return setView; }
            set
            {
                if (SetProperty(ref setView, value, "SetView"))
                {
                    NotifyPropertyChanged("Set");
                }
            }
        }

        public override string ToString()
        {
            return Set.ToString();
        }
    }

}
