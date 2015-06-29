using Ben.Utilities;
using System.Xml.Serialization;

namespace Ben.Dominion
{
    public class SetOption : NotifyPropertyChangedBase
    {
        private bool? enabled;
        private CardSet set;

        [XmlAttribute]
        public bool? Enabled
        {
            get { return enabled; }
            set { SetProperty(ref enabled, value, "Enabled"); }
        }

        // If we're in the 'tri' state, then this set is 'required' for the output.
        public bool Required
        {
            get { return this.Enabled == null; }
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
