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
using Microsoft.Phone.Controls;

namespace Ben.Dominion
{
    public partial class CardInfo : PhoneApplicationPage
    {
        public CardInfo()
        {
            InitializeComponent();

            this.Loaded += new RoutedEventHandler(CardInfo_Loaded);
        }

        void CardInfo_Loaded(object sender, RoutedEventArgs e)
        {
            this.DataContext = (App.Current as App).SelectedCard;
        }
    }
}