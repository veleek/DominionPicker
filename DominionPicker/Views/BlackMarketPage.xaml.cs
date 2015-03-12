using System;
using System.Threading.Tasks;
using System.Windows;
using Ben.Utilities;
using GalaSoft.MvvmLight.Threading;
using Microsoft.Phone.Controls;
using GestureEventArgs = System.Windows.Input.GestureEventArgs;

namespace Ben.Dominion
{
    public partial class BlackMarketPage : PhoneApplicationPage
    {
        public BlackMarketPage()
        {
            this.InitializeComponent();

            this.Loaded += (s, e) =>
            {
                DispatcherHelper.Initialize();
                MainViewModel.Instance.BlackMarket.Reset();
            };
        }

        private async void DrawButton_Click(object sender, RoutedEventArgs e)
        {
            await Task.Run(() => MainViewModel.Instance.BlackMarket.Draw());
        }

        private void DiscardButton_Click(object sender, RoutedEventArgs e)
        {
            MainViewModel.Instance.BlackMarket.Discard();
        }

        private void Reset_Click(object sender, EventArgs e)
        {
            MainViewModel.Instance.BlackMarket.Reset();
        }

        private void CardItem_Tap(object sender, GestureEventArgs gestureEventArgs)
        {
            var card = sender.GetContext<Card>();
            MainViewModel.Instance.BlackMarket.Pick(card);
        }

        private void CardItem_Swipe(Object sender, EventArgs e)
        {
            var card = sender.GetContext<Card>();
            MainViewModel.Instance.BlackMarket.Replace(card);
        }
    }
}