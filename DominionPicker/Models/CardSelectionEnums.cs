using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ben.Dominion.Models
{
    public enum PlatinumColonyOption
    {
        /// <summary>
        /// Never show Platinum and Colony in the list of required cards.
        /// </summary>
        Never,
        /// <summary>
        /// Using the propsperity rules, randomly select a card from the set of Kingdom cards 
        /// and if that card is from Prosperity, include Platinum/Colony
        /// </summary>
        Randomly,
        /// <summary>
        /// If you are using any cards from Prosperity, always include Platinum and Colony
        /// </summary>
        Always,
    }

    public enum SheltersOption
    {
        /// <summary>
        /// Nevery select Shelters instead of Estates.
        /// </summary>
        Never,
        /// <summary>
        /// As per the Dark Ages rules, randomly select a card from the set of Kingdom cards 
        /// and if that card is from Dark Ages, use Shelters.
        /// </summary>
        Randomly,
        /// <summary>
        /// If you are using any cards from Dark Ages, always use Shelters
        /// </summary>
        Always,
    }
}
