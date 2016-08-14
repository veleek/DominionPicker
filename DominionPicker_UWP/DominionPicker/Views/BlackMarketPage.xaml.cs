using System;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Ben.Utilities;
using GalaSoft.MvvmLight.Threading;
using GestureEventArgs = Windows.UI.Xaml.Input.TappedRoutedEventArgs;

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
                   DispatcherHelper.Initialize();
                   MainViewModel.Instance.BlackMarket.Reset();
               };
        }

        private void DrawButton_Click(object sender, RoutedEventArgs e)
        {
            MainViewModel.Instance.BlackMarket.Draw();
        }

        private void DiscardButton_Click(object sender, RoutedEventArgs e)
        {
            MainViewModel.Instance.BlackMarket.Discard();
        }

        private void Reset_Click(object sender, RoutedEventArgs e)
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