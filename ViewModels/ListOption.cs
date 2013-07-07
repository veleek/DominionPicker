using System.Xml.Serialization;
using Ben.Utilities;

namespace Ben.Dominion
{
    public class ListOption<TOptionType> : NotifyPropertyChangedBase
    {
        private bool enabled;
        private TOptionType optionValue;

        [XmlAttribute]
        public bool Enabled
        {
            get { return enabled; }
            set { SetProperty(ref enabled, value, "Enabled"); }
        }

        [XmlText]
        public TOptionType OptionValue
        {
            get { return optionValue; }
            set { SetProperty(ref optionValue, value, "OptionValue"); }
        }

        public bool Is(TOptionType value)
        {
            return OptionValue.Equals(value);
        }
    }

    public class PlusOption : ListOption<string>
    {
    }

    public enum PlusOptionValue
    {
        Require,
        RequirePlus2,
        Prevent,
        PreventPlus2
    }
}
