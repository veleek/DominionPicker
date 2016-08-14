using Ben.Utilities;
using System.Xml.Serialization;

namespace Ben.Dominion
{
    /// <summary>
    /// Represents the state of a set selection for the picker.
    /// </summary>
    public class SetOption : NotifyPropertyChangedBase
    {
        private bool enabled;
        private bool required;
        private CardSet set;

        /// <summary>
        /// Gets or sets a value that indicates whether this set should be allowed in the result.
        /// </summary>
        [XmlAttribute]
        public bool Enabled
        {
            get
            {
                return enabled;
            }
            set
            {
                SetProperty(ref enabled, value);
            }
        }

        /// <summary>
        /// Gets or sets a value that indicates whether this set is required to be in the output.
        /// </summary>
        /// <remarks>
        /// Just because a set is required doesn't necessarily mean it will be in the result.  If
        /// there are more 'required' sets than any other restrictions allow, a random selection 
        /// of the maximum number of required sets will be selected.
        /// </remarks>
        public bool Required
        {
            get
            {
                return this.required;
            }
            set
            {
                SetProperty(ref required, value);
            }
        }

        /// <summary>
        /// Gets or set the card set that this option refers to.
        /// </summary>
        [XmlAttribute]
        public CardSet Set
        {
            get
            {
                return set;
            }
            set
            {
                SetProperty(ref set, value, "Set");
            }
        }

        public override string ToString()
        {
            string currentState = Required ? "Required" : (Enabled ? "Allowed" : "Not Allowed");
            return $"{Set} - {currentState}";
        }
    }
}