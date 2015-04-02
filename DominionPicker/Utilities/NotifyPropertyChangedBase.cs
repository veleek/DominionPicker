using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Windows;
using GalaSoft.MvvmLight.Threading;

namespace Ben.Utilities
{
    /// <summary>
    /// A base class for types that support INotifyPropertyChanged to allow proper data binding
    /// </summary>
    [DataContract]
    public class NotifyPropertyChangedBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void NotifyPropertyChanged(String property)
        {
            var propertyChanged = this.PropertyChanged;
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
            var handler = this.PropertyChanged;

            if (handler != null)
            {
                DispatcherHelper.CheckBeginInvokeOnUI(
                    () => handler(this, new PropertyChangedEventArgs(propertyName)));
            }
        }

        protected bool SetProperty<TProperty>(ref TProperty field, TProperty value, [CallerMemberName]string propertyName = null)
        {
            if (Equals(value, field))
            {
                return false;
            }

            field = value;
            this.OnPropertyChanged(propertyName);
            return true;
        }
    }

    /// <summary>
    /// A base class for types that support INotifyPropertyChanged and INotifyDataErrorInfo
    /// </summary>
    [DataContract]
    public abstract class NotifyBase : NotifyPropertyChangedBase, INotifyDataErrorInfo
    {
        #region INotifyDataErrorInfo

        private readonly Dictionary<string, string> propertyErrors = new Dictionary<string, string>();
        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

        public IEnumerable GetErrors(string propertyName)
        {
            string errorValue;

            if (!this.propertyErrors.TryGetValue(propertyName, out errorValue))
            {
                return null;
            }

            return new[] {errorValue};
        }

        public bool HasErrors
        {
            get { return this.propertyErrors.Count > 0; }
        }

        public void AddDataError(String error)
        {
            this.AddDataError(String.Empty, error);
        }

        public void AddDataError(String propertyName, String error)
        {
            if (!this.propertyErrors.ContainsKey(propertyName) || this.propertyErrors[propertyName] != error)
            {
                this.propertyErrors[propertyName] = error;
                this.RaiseErrorsChanged(propertyName);
            }
        }

        public void RemoveDataError()
        {
            this.RemoveDataError(String.Empty);
        }

        public void RemoveDataError(String propertyName)
        {
            if (this.propertyErrors.ContainsKey(propertyName))
            {
                this.propertyErrors.Remove(propertyName);
                this.RaiseErrorsChanged(propertyName);
            }
        }

        private void RaiseErrorsChanged(String propertyName)
        {
            var errorsChanged = this.ErrorsChanged;
            if (errorsChanged != null)
            {
                errorsChanged(this, new DataErrorsChangedEventArgs(propertyName));
            }
        }

        #endregion INotifyDataErrorInfo
    }
}