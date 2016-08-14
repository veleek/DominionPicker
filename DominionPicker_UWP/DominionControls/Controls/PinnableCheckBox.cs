using Windows.UI.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace Ben.Controls
{

    [TemplateVisualState(GroupName = "PinStates", Name = "Unpinned")]
    [TemplateVisualState(GroupName = "PinStates", Name = "Pinned")]
    public class PinnableCheckBox : CheckBox
    {
        private bool wasHeldToggle;
        private bool wasHeldRightTapped;

        public PinnableCheckBox()
        {
            this.DefaultStyleKey = typeof(PinnableCheckBox);
        }

        public static readonly DependencyProperty IsPinnedProperty =
            DependencyProperty.Register("IsPinned", typeof(bool), typeof(PinnableCheckBox),
                new PropertyMetadata(false, OnIsPinnedPropertyChanged));

        public bool IsPinned
        {
            get
            {
                return (bool)GetValue(IsPinnedProperty);
            }
            set
            {
                SetValue(IsPinnedProperty, value);
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

        protected override void OnToggle()
        {
            // If the control was just held, we can ignore the Toggle event.
            // This prevents changing the check state when pinning.
            if (this.wasHeldToggle)
            {
                this.wasHeldToggle = false;
                return;
            }

            base.OnToggle();
        }

        protected override void OnRightTapped(RightTappedRoutedEventArgs e)
        {
            // Tap and Hold results in a RightTapped event on release,
            // But we handle the hold so there is immediate feedback so
            // we don't want to change the pinned state if it was held.
            if (this.wasHeldRightTapped)
            {
                this.wasHeldRightTapped = false;
                return;
            }
            else
            {
                this.IsPinned = !this.IsPinned;
            }

            base.OnRightTapped(e);
        }

        protected override void OnHolding(HoldingRoutedEventArgs e)
        {
            if (e.HoldingState == HoldingState.Started)
            {
                this.IsPinned = !this.IsPinned;
                this.wasHeldToggle = true;
                this.wasHeldRightTapped = true;
            }

            base.OnHolding(e);
        }

        protected override void OnApplyTemplate()
        {
            this.ChangeVisualState(false);
            base.OnApplyTemplate();
        }

        private void ChangeVisualState(bool useTransitions)
        {
            if (this.IsPinned)
            {
                VisualStateManager.GoToState(this, "Pinned", useTransitions);
                return;
            }
            if (!this.IsPinned)
            {
                VisualStateManager.GoToState(this, "Unpinned", useTransitions);
                return;
            }
        }
    }
}