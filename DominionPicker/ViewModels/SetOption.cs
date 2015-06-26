using Ben.Utilities;
using System.Xml.Serialization;

namespace Ben.Dominion
{
    public class SetOption : NotifyPropertyChangedBase
    {
        private bool enabled;
        private CardSet set;

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
            set { SetProperty(ref set, value, "Set"); }
        }

        public override string ToString()
        {
            return Set.ToString();
        }
    }

}
