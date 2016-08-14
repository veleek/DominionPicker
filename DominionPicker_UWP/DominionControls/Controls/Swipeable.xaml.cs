using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using System.Windows.Input;

namespace Ben.Controls
{

   public partial class Swipeable
      : Windows.UI.Xaml.Controls.UserControl
   {

      public Windows.UI.Xaml.Controls.Orientation EnabledSwipeDirection
      {
         get
         {
            return (Windows.UI.Xaml.Controls.Orientation)this.GetValue(EnabledSwipeDirectionProperty);
         }
         set
         {
            this.SetValue(EnabledSwipeDirectionProperty, value);
         }
      }

      // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
      public static readonly DependencyProperty EnabledSwipeDirectionProperty = DependencyProperty.Register("EnabledSwipeDirection", typeof(Windows.UI.Xaml.Controls.Orientation), typeof(Swipeable), new PropertyMetadata(null, SwipeDirectionChanged));

      private static void SwipeDirectionChanged(DependencyObject d, Windows.UI.Xaml.DependencyPropertyChangedEventArgs e)
      {
         Swipeable swipeable = d as Swipeable;
         if ( swipeable == null )
         {
            throw new ArgumentException("Expected swipeable object", "d");
         }
         swipeable.OnSwipeDirectionChanged((Windows.UI.Xaml.Controls.Orientation)e.NewValue);
      }

      private void OnSwipeDirectionChanged(Windows.UI.Xaml.Controls.Orientation newValue)
      {
      // Enable and disable some directions
      }


      public Swipeable()
      {
         this.InitializeComponent();
         this.ManipulationDelta += this.Swipable_ManipulationDelta;
         this.ManipulationCompleted += this.Swipable_ManipulationCompleted;
      }

      private void Swipable_ManipulationCompleted(object sender, Windows.UI.Xaml.Input.ManipulationCompletedRoutedEventArgs e)
      {
         if ( e.Velocities.Linear.X > 10 )
         {
            this.SwipeTransform.TranslateX = 0;
         }
      }

      private void Swipable_ManipulationDelta(object sender, Windows.UI.Xaml.Input.ManipulationDeltaRoutedEventArgs e)
      {
         this.SwipeTransform.TranslateX = e.Cumulative.Translation.X;
      }

   }

}