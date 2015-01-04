using System;
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

        [XmlIgnore]
        public virtual TOptionType OptionValue
        {
            get { return optionValue; }
            set { SetProperty(ref optionValue, value, "OptionValue"); }
        }

        /// <summary>
        /// The string value of the actual value of this list option
        /// </summary>
        /// <remarks>
        /// This is a hack around an issue in the XmlSerializer.  Basically, if you are serializing
        /// a value in a base class (ListOption in this case) as XmlText, then the type of that 
        /// value MUST be string.  This is slightly different than the XmlSerializer in the older
        /// version of Windows Phone, thus this was never required before.  So, we only use this in
        /// a few places and with basic types (ints, strings, etc.) so Convert.ChangeType() should
        /// work well enough until this is replaced with something better.
        /// </remarks>
        [XmlText]
        public string StringOptionValue
        {
            get { return optionValue.ToString(); }
            set { optionValue = (TOptionType)Convert.ChangeType(value, typeof (TOptionType)); }
        }

        public object Dummy { get; set; }

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
