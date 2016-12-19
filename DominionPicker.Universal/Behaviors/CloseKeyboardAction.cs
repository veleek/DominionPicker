using Microsoft.Xaml.Interactivity;
using Windows.System;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace Ben.Dominion.Behaviors
{
    public class CloseKeyboardOnEnterBehavior : Behavior
    {
        public TextBox AssociatedTextBox => this.AssociatedObject as TextBox;

        protected override void OnAttached()
        {
            this.AssociatedTextBox.KeyUp += AssociatedTextBox_KeyUp;
        }

        private void AssociatedTextBox_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.Enter)
            {
                e.Handled = true;
                Windows.UI.ViewManagement.InputPane.GetForCurrentView().TryHide();
            }
        }
    }

    public class CloseKeyboardAction : IAction
    {
        public object Execute(object sender, object parameter)
        {
            return Windows.UI.ViewManagement.InputPane.GetForCurrentView().TryHide();
        }
    }
}
