using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Documents;
using System.Windows.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using System.Threading.Tasks;
using Windows.Foundation;

namespace Ben.Dominion.TestControls
{

    public partial class AddFavoritePopupControl
       : Windows.UI.Xaml.Controls.UserControl
    {
        public event EventHandler<FavoriteEventArgs> SaveFavorite;

        public String FavoriteName
        {
            get
            {
                return (String)GetValue(FavoriteNameProperty);
            }
            set
            {
                SetValue(FavoriteNameProperty, value);
            }
        }

        // Using a DependencyProperty as the backing store for FavoriteName.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FavoriteNameProperty = DependencyProperty.Register("FavoriteName", typeof(String), typeof(AddFavoritePopupControl), new PropertyMetadata(String.Empty, OnFavoriteNameChanged));

        public static void OnFavoriteNameChanged(DependencyObject d, Windows.UI.Xaml.DependencyPropertyChangedEventArgs e)
        {
            (d as AddFavoritePopupControl).OnFavoriteNameChanged(e.NewValue as String);
        }


        public AddFavoritePopupControl()
        {
            InitializeComponent();
        }

        public Boolean IsOpen
        {
            get
            {
                return AddFavoritePopup.Visibility == Visibility.Visible;
            }
            set
            {
                AddFavoritePopup.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
                if (value)
                {
                    if (String.IsNullOrEmpty(FavoriteName))
                    {
                        FavoriteName = "Enter a name";
                    }
                    FavoriteNameTextBox.Focus(FocusState.Programmatic);
                    FavoriteNameTextBox.SelectAll();
                }
            }
        }

        private void OnFavoriteNameChanged(String newName)
        {
            AddFavoriteAcceptButton.IsEnabled = !String.IsNullOrEmpty(newName);
        }

        private void AddFavoriteAccept_Click(object sender, RoutedEventArgs e)
        {
            AddFavorite();
        }

        private void FavoriteNameTextBox_KeyUp(object sender, Windows.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            Boolean isEmpty = String.IsNullOrEmpty(FavoriteNameTextBox.Text);
            AddFavoriteAcceptButton.IsEnabled = !isEmpty;
            if (!isEmpty && e.Key == Windows.System.VirtualKey.Enter)
            {
                AddFavoriteAcceptButton.Focus(FocusState.Programmatic);
                AddFavorite();
            }
        }

        private void AddFavorite()
        {
            if (String.IsNullOrEmpty(FavoriteName))
            {
                throw new InvalidOperationException("Somehow this got called with an empty name");
            }
            var saveFavHandler = SaveFavorite;
            if (saveFavHandler != null)
            {
                saveFavHandler(this, new FavoriteEventArgs(FavoriteName));
            }
            //PickerState.Current.SaveFavorite(name);
            FavoriteName = String.Empty;
            // This allows prevents the popup from appearing closed before it's 
            // actually been dismissed, so we don't accidentally capture touch 
            // events on stuff underneath it.
            IAsyncAction updateUiTask = Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                this.IsOpen = false;
            });
        }

    }

    public class FavoriteEventArgs
       : EventArgs
    {

        public String FavoriteName { get; set; }


        public FavoriteEventArgs(String favoriteName)
        {
            this.FavoriteName = favoriteName;
        }
    }

}