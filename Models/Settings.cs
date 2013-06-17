using Ben.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Ben.Dominion.Models
{
    [DataContract]
    public class Settings
    {
        /// <summary>
        /// Gets or sets a value that indicates whether extra 
        /// items should be shown in the picker results
        /// </summary>
        public bool ShowExtras { get; set; }

        /// <summary>
        /// Gets or sets a value that indicates whether to pick
        /// if Platinum and Colony should be used
        /// </summary>
        public bool PickPlatinumColony { get; set; }

        /// <summary>
        /// Gets or sets a value that indicates whether to pick
        /// if Shelters or Estates should be used
        /// </summary>
        public bool PickSheltersOrEstates { get; set; }

    }
}
