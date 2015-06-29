using System;
using System.Globalization;
using System.Windows.Data;

namespace Ben.Data
{
    public class LocalizeConverter : IValueConverter
    {
        public Localizer Localizer { get; protected set; }

        public LocalizeConverter(Localizer localizer)
        {
            this.Localizer = localizer;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(!targetType.IsAssignableFrom(typeof(String)))
            {
                throw new ArgumentException(string.Format("Unable to convert to type {0}", targetType));
            }

            return Localizer.GetLocalizedValue(value, parameter as string,  culture);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new InvalidOperationException("Unable to convert localized value back into object");
        }
    }

    public class StringsLocalizeConverter : LocalizeConverter
    {
        public StringsLocalizeConverter() : base(Localized.Strings)
        {
        }
    }

    public class CardDataLocalizeConverter : LocalizeConverter
    {
        public CardDataLocalizeConverter() : base(Localized.CardData)
        {
        }
    }
}

