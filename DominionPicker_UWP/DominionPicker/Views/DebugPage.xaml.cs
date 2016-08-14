using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.IsolatedStorage;
using System.Threading;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Ben.Dominion.Resources;
using Ben.Utilities;
using System;

namespace Ben.Dominion
{

   public partial class DebugPage
   {

      public DebugPage()
      {
         this.InitializeComponent();
         this.Loaded += this.DebugPage_Loaded;
         this.DataContext = AppLog.Instance;
      }

      private void DebugPage_Loaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
      {
         //WINDOWS_PHONE_SL_TO_UWP: (1101) System.Threading.Thread.CurrentThread was not upgraded
         AppLog.Instance.Log("Strings.Title: {0} Culture: {1}", Strings.Application_Title, System.Globalization.CultureInfo.CurrentCulture.Name);
      }

      private async void SavePickerState_OnClick(object sender, Windows.UI.Xaml.RoutedEventArgs e)
      {
         string tag = sender.GetTag<string>();
         string[] tagParts = tag.Split(',');
         using ( var savedStateStream = await Windows.Storage.ApplicationData.Current.LocalFolder.OpenStreamForReadAsync(tagParts[0]) )
         {
            using ( IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForApplication() )
            {
               using ( Stream currentStateStream = store.OpenFile(tagParts.Length > 1 ? tagParts[1] : "PickerState.xml", FileMode.Create) )
               {
                  savedStateStream.CopyTo(currentStateStream);
               }
            }
         }
      }

   }

}