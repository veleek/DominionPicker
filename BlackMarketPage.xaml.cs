using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Ben.Utilities;

namespace Ben.Dominion
{
    public partial class BlackMarketPage : Microsoft.Phone.Controls.PhoneApplicationPage
    {
        public BlackMarketPage()
        {
            InitializeComponent();
        }

        private void DrawButton_Click(object sender, RoutedEventArgs e)
        {
            MainViewModel.Instance.BlackMarket.Draw();
        }

        private void DiscardButton_Click(object sender, RoutedEventArgs e)
        {
            MainViewModel.Instance.BlackMarket.Discard();
        }

        private void Reset_Click(object sender, System.EventArgs e)
        {
            MainViewModel.Instance.BlackMarket.Reset();
        }

        private void ScrollViewer_ManipulationCompleted(object sender, ManipulationCompletedEventArgs e)
        {
            var card = sender.GetContext<Card>();

            if (card != null && MainViewModel.Instance.BlackMarket.Hand.Contains(card))
            {
                var scrollViewer = sender as ScrollViewer;
                if (scrollViewer == null)
                {
                    return;
                }

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

        private void CardItem_Tap(object sender, GestureEventArgs e)
        {
            var card = sender.GetContext<Card>();
            MainViewModel.Instance.BlackMarket.Pick(card);
        }
    }
}