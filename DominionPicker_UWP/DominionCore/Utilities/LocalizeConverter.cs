using System;
using System.Globalization;
using System.Reflection;
using Windows.UI.Xaml.Data;

namespace Ben.Data
{

   public class LocalizeConverter
      : IValueConverter
    {

      public Localizer Localizer { get; protected set; }


      public LocalizeConverter(Localizer localizer)
      {
         this.Localizer = localizer;
      }

      public object Convert(object value, Type targetType, object parameter, string culture)
      {
         if ( !targetType.GetTypeInfo().IsAssignableFrom(typeof(String).GetTypeInfo()) )
         {
            throw new ArgumentException(string.Format("Unable to convert to type {0}", targetType));
         }
         return Localizer.GetLocalizedValue(value, parameter as string, culture);
      }

      public object ConvertBack(object value, Type targetType, object parameter, string culture)
      {
         throw new InvalidOperationException("Unable to convert localized value back into object");
      }

   }

   public class StringsLocalizeConverter
      : LocalizeConverter
   {

      public StringsLocalizeConverter()
      : base(Localized.Strings)
      {
      }
   }

   public class CardDataLocalizeConverter
      : LocalizeConverter
   {

      public CardDataLocalizeConverter()
      : base(Localized.CardData)
      {
      }
   }

}