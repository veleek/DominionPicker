using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Ben.Utilities;
using GalaSoft.MvvmLight.Threading;
using Microsoft.Phone.Controls;

namespace Ben.Dominion
{
    public partial class BlackMarketPage : PhoneApplicationPage
    {
        public BlackMarketPage()
        {
            InitializeComponent();

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

        private void Reset_Click(object sender, EventArgs e)
        {
            MainViewModel.Instance.BlackMarket.Reset();
        }

        private void ScrollViewer_ManipulationCompleted(object sender, ManipulationCompletedEventArgs e)
        {
            ScrollContentPresenter contentPresenter = sender as ScrollContentPresenter;
            if (contentPresenter == null)
            {
                return;
            }

            var scrollViewer = contentPresenter.ScrollOwner;
            var card = scrollViewer.GetContext<Card>();

            if (card != null && MainViewModel.Instance.BlackMarket.Hand.Contains(card))
            {
                var v = e.FinalVelocities.LinearVelocity;
                var m = v.GetMagnitude();
                var a = Math.Abs(v.X);

                // If it's an intertal manipulation, and it's 80% in the X direction
                // and the total manipulitation is greater than 400
                if (e.IsInertial && (a / m) > 0.8 && m > 400)
                {
                    MainViewModel.Instance.BlackMarket.Replace(card);
                }
                
            }
        }

        private void CardItem_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            var card = sender.GetContext<Card>();
            MainViewModel.Instance.BlackMarket.Pick(card);
        }
    }
}