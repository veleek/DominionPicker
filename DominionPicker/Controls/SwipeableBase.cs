using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Ben.Controls
{
    [TemplatePart(Name = "ScrollContentPresenter", Type = typeof (ScrollContentPresenter))]
    public class SwipeableBase : ContentControl
    {
        private ScrollContentPresenter contentPresenter;

        public SwipeableBase()
        {
            this.DefaultStyleKey = typeof (SwipeableBase);

            this.ManipulationStarted += this.SwipeableBase_ManipulationStarted;
            this.ManipulationDelta += this.SwipeableBase_ManipulationDelta;
            this.ManipulationCompleted += this.SwipeableBase_ManipulationCompleted;

            this.Loaded += this.SwipeableBase_Loaded;
        }

        private ScrollContentPresenter CP
        {
            get { return this.ContentPresenter; }
        }

        private ScrollContentPresenter ContentPresenter
        {
            get
            {
                return this.contentPresenter ??
                       (this.contentPresenter =
                           this.GetTemplateChild("PART_ScrollContentPresenter") as ScrollContentPresenter);
            }
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            var scrollContentPresenter = this.GetTemplateChild("PART_ScrollContentPresenter") as ScrollContentPresenter;
        }

        private void SwipeableBase_Loaded(System.Object sender, RoutedEventArgs e)
        {
            //this.ContentPresenter.ExtentHeight = this.ContentPresenter.ActualHeight * 3;
            //this.ContentPresenter.ExtentWidth = this.ContentPresenter.ActualWidth * 3;
            this.ContentPresenter.CanHorizontallyScroll = true;
            this.ContentPresenter.CanVerticallyScroll = true;

            var members = typeof (ScrollContentPresenter).GetMembers(BindingFlags.Instance | BindingFlags.NonPublic);
            var p = typeof (ScrollContentPresenter).GetProperty("IsScrollClient",
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Default);

            var isScroll = p.GetValue(this.CP);

        }

        private void SwipeableBase_ManipulationCompleted(object sender, ManipulationCompletedEventArgs e)
        {
        }

        private void SwipeableBase_ManipulationDelta(object sender, ManipulationDeltaEventArgs e)
        {
            this.ContentPresenter.SetHorizontalOffset(e.CumulativeManipulation.Translation.X);

            var current = this.ContentPresenter.VerticalOffset;
            var next = e.CumulativeManipulation.Translation.Y;
            this.ContentPresenter.SetVerticalOffset(e.CumulativeManipulation.Translation.Y);
            var after = this.ContentPresenter.VerticalOffset;
        }

        private void SwipeableBase_ManipulationStarted(object sender, ManipulationStartedEventArgs e)
        {
        }
    }
}