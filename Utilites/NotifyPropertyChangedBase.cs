using System;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Runtime.Serialization;
using System.Windows;

namespace Ben.Utilities
{
    [DataContract]
    public class NotifyPropertyChangedBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void NotifyPropertyChanged(String property)
        {
            var propertyChanged = PropertyChanged;
            if (propertyChanged != null)
            {
                // So basically INotifyPropertyChanged is used (in general) to notify the UI that
                // a property has changed so that any bindings can be updated.  Sure there are reasons
                // other than that to us it, but I think that that's the 90% case.  So we need to make
                // sure that the callback happens on the UI thread since it's probably a binding that
                // needs to refreshed.

                PropertyChangedEventArgs args = new PropertyChangedEventArgs(property);

                // If we're not on the UI thread
                if (Deployment.Current.Dispatcher.CheckAccess())
                {
                    // Then directly invoke the callback
                    propertyChanged(this, args);
                }
                else
                {
                    // otherwise get the dispatcher to do it for us
                    Deployment.Current.Dispatcher.BeginInvoke(() => propertyChanged(this, args));
                }
            }
        }

        protected virtual void OnPropertyChanged(string propertyName)
		{
			var handler = PropertyChanged;

			if (handler != null)
			{
				handler(this, new PropertyChangedEventArgs(propertyName));
			}
		}

        protected void SetProperty<TProperty>(ref TProperty field, TProperty value, string propertyName)
        {
            if (Equals(value, field))
            {
                return;
            }

            field = value;
            OnPropertyChanged(propertyName);
        }
    }
}
