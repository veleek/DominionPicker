using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Input;

namespace Ben.Dominion.Behaviors
{
    public class OnKeyAction : DelegatingAction
    {
        public static readonly DependencyProperty KeyProperty =
            DependencyProperty.Register("Key", typeof(VirtualKey), typeof(OnKeyAction), new PropertyMetadata(VirtualKey.None));

        public VirtualKey Key
        {
            get { return (VirtualKey)GetValue(KeyProperty); }
            set { SetValue(KeyProperty, value); }
        }

        public override object Execute(object sender, object parameter)
        {
            if (this.Key == VirtualKey.None)
            {
                return null;
            }

            var args = parameter as KeyRoutedEventArgs;
            if (!this.Key.HasFlag(args.Key))
            {
                return null;
            }

            return base.Execute(sender, parameter);
        }
    }

}
