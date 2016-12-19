using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Ben.Dominion.Views
{
    public sealed partial class AddFavoriteDialog : ContentDialog
    {
        public static readonly DependencyProperty FavoriteNameProperty =
            DependencyProperty.Register("FavoriteName", typeof(string), typeof(AddFavoriteDialog), new PropertyMetadata(null));

        public event EventHandler<FavoriteEventArgs> SaveFavorite;

        public AddFavoriteDialog()
        {
            this.InitializeComponent();
        }

        public string FavoriteName
        {
            get { return (string)GetValue(FavoriteNameProperty); }
            set { SetValue(FavoriteNameProperty, value); }
        }

        private void AcceptButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            this.AddFavorite();
        }

        private void CancelButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }

        private void AddFavorite()
        {
            if (String.IsNullOrEmpty(this.FavoriteName))
            {
                throw new InvalidOperationException("Somehow this got called with an empty name");
            }

            SaveFavorite?.Invoke(this, new FavoriteEventArgs(FavoriteName));
        }
    }

    public class FavoriteEventArgs
       : EventArgs
    {
        public String FavoriteName { get; }

        public FavoriteEventArgs(String favoriteName)
        {
            this.FavoriteName = favoriteName;
        }
    }
}
