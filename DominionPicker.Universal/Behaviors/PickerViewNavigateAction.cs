using Ben.Dominion.Views;
using Microsoft.Xaml.Interactivity;
using Windows.UI.Xaml;

namespace Ben.Dominion.Behaviors
{
    public class PickerViewNavigateAction : DependencyObject, IAction
    {
        public static readonly DependencyProperty ViewProperty =
            DependencyProperty.Register("View", typeof(PickerView), typeof(PickerViewNavigateAction), new PropertyMetadata(PickerView.None));

        public static readonly DependencyProperty ParameterProperty =
            DependencyProperty.Register("Parameter", typeof(object), typeof(PickerViewNavigateAction), new PropertyMetadata(null));

        public PickerViewNavigateAction()
        {

        }

        public PickerView View
        {
            get { return (PickerView)GetValue(ViewProperty); }
            set { SetValue(ViewProperty, value); }
        }

        public object Parameter
        {
            get { return (object)GetValue(ParameterProperty); }
            set { SetValue(ParameterProperty, value); }
        }

        public object Execute(object sender, object parameter)
        {
            if(this.View == PickerView.None)
            {
                return false;
            }

            this.View.Go(this.Parameter);
            return true;
        }
    }
}
