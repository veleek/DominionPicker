﻿using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Ben.Utilities;

namespace Ben.Dominion
{
    public partial class DominionCardControl : UserControl
    {
        public static readonly DependencyProperty CardProperty =
            DependencyProperty.Register("Card", typeof (Card), typeof (DominionCardControl), new PropertyMetadata(null));

        public static readonly DependencyProperty ShowSetIconProperty =
            DependencyProperty.Register("ShowSetIcon", typeof (bool), typeof (DominionCardControl), new PropertyMetadata(true));

        public static readonly DependencyProperty IsSwipeEnabledProperty =
            DependencyProperty.Register("IsSwipeEnabled", typeof (bool), typeof (DominionCardControl), new PropertyMetadata(true));

        public static readonly DependencyProperty HeaderProperty =
            DependencyProperty.Register("Header", typeof(string), typeof(DominionCardControl), new PropertyMetadata(null));

        public DominionCardControl()
        {
            this.InitializeComponent();
        }

        public event EventHandler Swipe;

        public Card Card
        {
            get { return (Card) this.GetValue(CardProperty); }
            set { this.SetValue(CardProperty, value); }
        }

        public string Header
        {
            get { return (string)this.GetValue(HeaderProperty); }
            set { this.SetValue(HeaderProperty, value); }
        }

        public bool IsSwipeEnabled
        {
            get { return (bool) this.GetValue(IsSwipeEnabledProperty); }
            set { this.SetValue(IsSwipeEnabledProperty, value); }
        }

        public bool ShowSetIcon
        {
            get { return (bool) this.GetValue(ShowSetIconProperty); }
            set { this.SetValue(ShowSetIconProperty, value); }
        }

        private void ScrollViewer_ManipulationCompleted(object sender, ManipulationCompletedEventArgs e)
        {
            var contentPresenter = sender as ScrollContentPresenter;
            if (contentPresenter == null)
            {
                return;
            }

            Point v = e.FinalVelocities.LinearVelocity;
            double m = v.GetMagnitude();
            double a = Math.Abs(v.X);

            // If it's an intertal manipulation, and it's 80% in the X direction
            // and the total manipulitation is greater than 400
            if (e.IsInertial && (a / m) > 0.8 && m > 400)
            {
                this.OnSwipe();
            }
        }

        protected virtual void OnSwipe()
        {
            this.Swipe.Invoke(this, EventArgs.Empty);
        }
    }
}