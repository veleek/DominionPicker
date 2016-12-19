using Microsoft.Xaml.Interactivity;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Markup;

namespace Ben.Dominion.Behaviors
{
    [ContentProperty(Name = "DelegateAction")]
    public class DelegatingAction : DependencyObject, IAction
    {
        public IAction DelegateAction
        {
            get { return (IAction)GetValue(DelegateActionProperty); }
            set { SetValue(DelegateActionProperty, value); }
        }

        // Using a DependencyProperty as the backing store for DelegateAction.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DelegateActionProperty =
            DependencyProperty.Register("DelegateAction", typeof(IAction), typeof(DelegatingAction), new PropertyMetadata(null));

        public virtual object Execute(object sender, object parameter)
        {
            return this.DelegateAction.Execute(sender, parameter);
        }
    }
}
