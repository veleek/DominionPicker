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
        public AddFavoritePopupControl()
        {
            InitializeComponent();
        }

        public Boolean IsOpen
        {
            get { return AddFavoritePopup.IsOpen; }
            set 
            {
                if (value)
                {
                    FavoriteNameTextBox.Text = "Enter a name";
                    FavoriteNameTextBox.SelectAll();
                }
                AddFavoritePopup.IsOpen = value; 
            }
        }
        
        private void AddFavoriteAccept_Click(object sender, RoutedEventArgs e)
        {
            String name = FavoriteNameTextBox.Text;

            if (!String.IsNullOrEmpty(name))
            {
                PickerState.Current.SaveFavorite(name);
                FavoriteNameTextBox.Text = "";
                AddFavoritePopup.IsOpen = false;
            }
            else
            {
                FavoriteNameTextBox.BorderBrush = new SolidColorBrush(Colors.Red);
                FavoriteNameTextBox.BorderThickness = new Thickness(2);
            }
        }
    }
}
