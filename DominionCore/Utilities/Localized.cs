using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ben.Dominion.Resources;

namespace Ben.Data
{
    public static class Localized
    {
#if NETFX_CORE
        public static Localizer Strings = new Localizer(
            "DominionCore.Universal/Strings", 
            () => Windows.ApplicationModel.Resources.ResourceLoader.GetForViewIndependentUse("DominionCore.Universal/Strings"), 
            () => Dominion.Resources.Strings.Culture,
            Dominion.Resources.Strings.GetString);
        public static Localizer CardData = new Localizer(
            "DominionCore.Universal/CardDataStrings", 
            () => Windows.ApplicationModel.Resources.ResourceLoader.GetForViewIndependentUse("DominionCore.Universal/CardDataStrings"), 
            () => CardDataStrings.Culture,
            CardDataStrings.GetString);
#else
        public static Localizer Strings = new Localizer(
            string.Empty,
            () => Dominion.Resources.Strings.ResourceManager,
            () => Dominion.Resources.Strings.Culture,
            Dominion.Resources.Strings.ResourceManager.GetString
        );

        public static Localizer CardData = new Localizer(
            string.Empty,
            () => CardDataStrings.ResourceManager,
            () => CardDataStrings.Culture,
            CardDataStrings.ResourceManager.GetString);
#endif
    }
}
