using System;

using Windows.UI.Xaml;

using Ben.Dominion.ViewModels;
using Ben.Utilities;

using GalaSoft.MvvmLight.Threading;

using GestureEventArgs = Windows.UI.Xaml.Input.TappedRoutedEventArgs;
using Ben.Dominion.Controls;

namespace Ben.Dominion
{

    public partial class BlackMarketPage
       : Windows.UI.Xaml.Controls.Page
    {

        public BlackMarketPage()
        {
            this.InitializeComponent();
            this.Loaded += (s, e) =>
            {
                MainViewModel.Instance.BlackMarket.Reset();
            };
        }

        public BlackMarketViewModel ViewModel
        {
            get { return MainViewModel.Instance.BlackMarket; }
        }

        private void Reset_Click(object sender, RoutedEventArgs e)
        {
            MainViewModel.Instance.BlackMarket.Reset();
        }

        private void CardItem_Tap(object sender, GestureEventArgs gestureEventArgs)
        {
            DominionCardControl cardControl = sender as DominionCardControl; 
            MainViewModel.Instance.BlackMarket.Pick(cardControl.Card);
        }

        private void CardItem_Swipe(Object sender, EventArgs e)
        {
            var card = sender.GetContext<Card>();
            MainViewModel.Instance.BlackMarket.Replace(card);
        }

    }

}