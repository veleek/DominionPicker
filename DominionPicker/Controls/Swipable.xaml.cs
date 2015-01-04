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

namespace Ben.Controls
{
    public partial class Swipable : UserControl
    {
        public Orientation EnabledSwipeDirection
        {
            get { return (Orientation)GetValue(EnabledSwipeDirectionProperty); }
            set { SetValue(EnabledSwipeDirectionProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty EnabledSwipeDirectionProperty =
            DependencyProperty.Register("EnabledSwipeDirection", typeof(Orientation), typeof(Swipable), new PropertyMetadata(null));
        
        public Swipable()
        {
            InitializeComponent();

            this.ManipulationDelta += Swipable_ManipulationDelta;
            this.ManipulationCompleted += Swipable_ManipulationCompleted;
        }

        void Swipable_ManipulationCompleted(object sender, ManipulationCompletedEventArgs e)
        {
            if (e.FinalVelocities.LinearVelocity.X > 10)
            {
                SwipeTransform.TranslateX = 0;
            }
        }

        void Swipable_ManipulationDelta(object sender, ManipulationDeltaEventArgs e)
        {
            SwipeTransform.TranslateX = e.CumulativeManipulation.Translation.X;
        }

    }
}
