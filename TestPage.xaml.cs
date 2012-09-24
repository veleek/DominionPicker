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
using System.ComponentModel;
using System.Windows.Data;
using System.Diagnostics;

namespace Ben.Dominion
{
    public partial class TestPage : PhoneApplicationPage
    {
        public TestPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            CardsList.IsFlatList = false;
            CardsList.ItemsSource = Cards.CardsViewSource.GroupedView;

            var c = Cards.CardsViewSource.View.Groups[0] as CollectionViewGroup;
            var g = c;
            var h = g.ItemCount;
            Cards.CardsViewSource.View.Filter = i => (i as CardSelector).Card.Name.StartsWith("A");
            var g2 = (Cards.CardsViewSource.View.Groups[0] as CollectionViewGroup);
            var h2 = g2.ItemCount;
            var h3 = g.ItemCount;
            Debug.WriteLine(h + " " + h2 + " " + h3);
        }

        private void ApplicationBarIconButton_Click(object sender, EventArgs e)
        {
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var text = SearchTextBox.Text;
            Cards.CardsViewSource.View.Filter = x =>
            {
                return (x as CardSelector).Card.Name.Contains(text);
            };

            CardsList.ItemsSource = Cards.CardsViewSource.GroupedView;
        }
    }
}