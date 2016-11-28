using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using GalaSoft.MvvmLight.Threading;
using System.Threading.Tasks;

namespace Ben.Utilities
{

    /// <summary>
    /// A base class for types that support INotifyPropertyChanged to allow proper data binding
    /// </summary>
    [DataContract]
    public class NotifyPropertyChangedBase
       : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName]string propertyName = null)
        {
            var handler = this.PropertyChanged;
            if (handler != null)
            {
                Task updateUiAction = DispatcherHelper.CheckBeginInvokeOnUI(() => handler(this, new PropertyChangedEventArgs(propertyName)));
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
    public abstract class NotifyBase
       : NotifyPropertyChangedBase, INotifyDataErrorInfo
    {
        private readonly Dictionary<string, string> propertyErrors = new Dictionary<string, string>();
        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

        public IEnumerable GetErrors(string propertyName)
        {
            string errorValue;
            if (!this.propertyErrors.TryGetValue(propertyName, out errorValue))
            {
                return null;
            }
            return new[] { errorValue };
        }

        public bool HasErrors
        {
            get
            {
                return this.propertyErrors.Count > 0;
            }
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
            this.ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
        }
    }

}