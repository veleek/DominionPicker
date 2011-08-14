﻿using System;
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
            this.Focus();
        }

        public Boolean IsOpen
        {
            get { return AddFavoritePopup.IsOpen; }
            set 
            {
                AddFavoritePopup.IsOpen = value;

                if (value)
                {
                    if (FavoriteNameTextBox.Text == String.Empty)
                    {
                        FavoriteNameTextBox.Text = "Enter a name";
                    }
                    FavoriteNameTextBox.Focus();
                    FavoriteNameTextBox.SelectAll();
                }
            }
        }
        
        private void AddFavoriteAccept_Click(object sender, RoutedEventArgs e)
        {
            AddFavorite();
        }

        private void FavoriteNameTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                AddFavoriteAcceptButton.Focus();
                AddFavorite();
            }
        }

        private void AddFavorite()
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
