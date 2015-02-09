using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.IsolatedStorage;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using Ben.Dominion.Resources;
using Ben.Utilities;

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

        private void DebugPage_Loaded(object sender, RoutedEventArgs e)
        {
            AppLog.Instance.Log("Strings.Title: {0} Culture: {1}", Strings.Application_Title,
                Thread.CurrentThread.CurrentCulture.Name);
        }

        private void SavePickerState_OnClick(object sender, RoutedEventArgs e)
        {
            string tag = sender.GetTag<string>();
            string[] tagParts = tag.Split(',');

            using (var savedStateStream = Microsoft.Xna.Framework.TitleContainer.OpenStream(tagParts[0]))
            {
                using (IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    using (Stream currentStateStream = store.OpenFile(tagParts.Length > 1 ? tagParts[1] : "PickerState.xml", FileMode.Create))
                    {
                        savedStateStream.CopyTo(currentStateStream);
                    }
                }
            }
        }
    }
}