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
        public static Localizer Strings = new Localizer("DominionCore/Strings", () => Windows.ApplicationModel.Resources.ResourceLoader.GetForViewIndependentUse("Strings"), () => Dominion.Resources.Strings.Culture);
        public static Localizer CardData = new Localizer("DominionCore/CardDataStrings", () => Windows.ApplicationModel.Resources.ResourceLoader.GetForViewIndependentUse("CardDataStrings"), () => CardDataStrings.Culture);
#else
        public static Localizer Strings = new Localizer(
            string.Empty,
            () => Dominion.Resources.Strings.ResourceManager,
            () => Dominion.Resources.Strings.Culture);

        public static Localizer CardData = new Localizer(
            string.Empty,
            () => CardDataStrings.ResourceManager,
            () => CardDataStrings.Culture);
#endif
    }
}
