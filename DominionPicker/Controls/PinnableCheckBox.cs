using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Ben.Utilities;

namespace Ben.Controls
{
    [TemplateVisualState(GroupName = "PinStates", Name = "Unpinned")]
    [TemplateVisualState(GroupName = "PinStates", Name = "Pinned")]
    public class PinnableCheckBox : CheckBox
    {
        private bool wasHeld;

        public bool IsPinned
        {
            get { return (bool)GetValue(IsPinnedProperty); }
            set { SetValue(IsPinnedProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Pinned.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsPinnedProperty =
            DependencyProperty.Register("IsPinned", typeof(bool), typeof(PinnableCheckBox), new PropertyMetadata(false, OnIsPinnedPropertyChanged));

        public PinnableCheckBox()
        {
            this.DefaultStyleKey = typeof(PinnableCheckBox);
            Test();
        }

        private void Test()
        {
            var pinMark = this.FindChild<Viewbox>("PinMark");

            if (pinMark != null)
            {
                var opacity = pinMark.Opacity;
            }
        }

        private static void OnIsPinnedPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            PinnableCheckBox checkBox = d as PinnableCheckBox;
            if (checkBox != null)
            {
                checkBox.ChangeVisualState(true);
            }
        }

        private void ChangeVisualState(bool useTransitions)
        {
            if (this.IsPinned)
            {
                VisualStateManager.GoToState(this, "Pinned", useTransitions);
                return;
            }

            if(!this.IsPinned)
            {
                VisualStateManager.GoToState(this, "Unpinned", useTransitions);
                return;
            }
        }

        protected override void OnClick()
        {
            // If the control was just held, we can ignore the click event.
            // This prevents changing the check state when pinning.
            if(this.wasHeld)
            {
                this.wasHeld = false;
                return;
            }

            base.OnClick();
        }

        protected override void OnHold(GestureEventArgs e)
        {
            this.IsPinned = !this.IsPinned;
            this.wasHeld = true;
            base.OnHold(e);
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            var pinMark = this.GetTemplateChild("PinMark");
        }
    }
}
