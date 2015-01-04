using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace Ben.Dominion
{
    public partial class AddFavoritePopupControl : UserControl
    {        
        public event EventHandler<FavoriteEventArgs> SaveFavorite;

        public String FavoriteName
        {
            get { return (String)GetValue(FavoriteNameProperty); }
            set { SetValue(FavoriteNameProperty, value); }
        }

        // Using a DependencyProperty as the backing store for FavoriteName.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FavoriteNameProperty =
            DependencyProperty.Register("FavoriteName", typeof(String), typeof(AddFavoritePopupControl), new PropertyMetadata(String.Empty, OnFavoriteNameChanged));

        public static void OnFavoriteNameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as AddFavoritePopupControl).OnFavoriteNameChanged(e.NewValue as String);
        }

        public AddFavoritePopupControl()
        {
            InitializeComponent();
        }

        public Boolean IsOpen
        {
            get { return AddFavoritePopup.Visibility == Visibility.Visible; }
            set 
            {
                AddFavoritePopup.Visibility = value ? Visibility.Visible : Visibility.Collapsed;

                if (value)
                {
                    if (String.IsNullOrEmpty(FavoriteName))
                    {
                        FavoriteName = "Enter a name";
                    }

                    FavoriteNameTextBox.Focus();
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

        private void FavoriteNameTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            Boolean isEmpty = String.IsNullOrEmpty(FavoriteNameTextBox.Text);

            AddFavoriteAcceptButton.IsEnabled = !isEmpty;

            if(!isEmpty && e.Key == Key.Enter)
            {
                AddFavoriteAcceptButton.Focus();
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
            Dispatcher.BeginInvoke(() => { this.IsOpen = false; });
        }
    }

    public class FavoriteEventArgs : EventArgs
    {
        public String FavoriteName { get; set; }

        public FavoriteEventArgs(String favoriteName)
        {
            this.FavoriteName = favoriteName;
        }
    }
}
