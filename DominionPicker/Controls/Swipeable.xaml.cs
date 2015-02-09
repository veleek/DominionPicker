using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Ben.Controls
{
    public partial class Swipeable : UserControl
    {
        public Orientation EnabledSwipeDirection
        {
            get { return (Orientation) this.GetValue(EnabledSwipeDirectionProperty); }
            set { this.SetValue(EnabledSwipeDirectionProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty EnabledSwipeDirectionProperty =
            DependencyProperty.Register("EnabledSwipeDirection", typeof (Orientation), typeof (Swipeable),
                new PropertyMetadata(null, SwipeDirectionChanged));

        private static void SwipeDirectionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Swipeable swipeable = d as Swipeable;
            if (swipeable == null)
            {
                throw new ArgumentException("Expected swipeable object", "d");
            }

            swipeable.OnSwipeDirectionChanged((Orientation)e.NewValue);
        }

        private void OnSwipeDirectionChanged(Orientation newValue)
        {
            // Enable and disable some directions
        }

        public Swipeable()
        {
            this.InitializeComponent();

            this.ManipulationDelta += this.Swipable_ManipulationDelta;
            this.ManipulationCompleted += this.Swipable_ManipulationCompleted;
        }

        private void Swipable_ManipulationCompleted(object sender, ManipulationCompletedEventArgs e)
        {
            if (e.FinalVelocities.LinearVelocity.X > 10)
            {
                this.SwipeTransform.TranslateX = 0;
            }
        }

        private void Swipable_ManipulationDelta(object sender, ManipulationDeltaEventArgs e)
        {
            this.SwipeTransform.TranslateX = e.CumulativeManipulation.Translation.X;
        }
    }
}