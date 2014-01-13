using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Collections;
using System.Runtime.Serialization;

namespace Ben.Utilities
{
    public static class FrameworkExtensions
    {
        public static T GetTag<T>(this Object obj)
        {
            T t = default(T);
            FrameworkElement element = obj as FrameworkElement;

            if (element != null)
            {
                t = (T)element.Tag;
            }

            return t;
        }

        public static T GetContext<T>(this Object obj)
        {
            T t = default(T);
            FrameworkElement element = obj as FrameworkElement;

            if (element != null)
            {
                t = (T)element.DataContext;
            }

            return t;
        }

        public static ObservableCollection<TSource> ToObservableCollection<TSource>(this IEnumerable<TSource> source)
        {
            ObservableCollection<TSource> collection = new ObservableCollection<TSource>();

            foreach (TSource t in source)
            {
                collection.Add(t);
            }

            return collection;
        }
    }

    public static class SystemExtensions
    {
        public static Double GetMagnitude(this Point p)
        {
            return Math.Sqrt(p.X * p.X + p.Y * p.Y);
        }
    }

    [DataContract]
    public abstract class NotifyBase : NotifyPropertyChangedBase, INotifyDataErrorInfo
    {
        public NotifyBase() { }

        #region INotifyDataErrorInfo

        private Dictionary<string, string> propertyErrors = new Dictionary<string, string>();
        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

        public IEnumerable GetErrors(string propertyName)
        {
            string errorValue;

            if (!propertyErrors.TryGetValue(propertyName, out errorValue))
            {
                return null;
            }

            return new string[] { errorValue };
        }

        public bool HasErrors
        {
            get { return propertyErrors.Count > 0; }
        }

        public void AddDataError(String error)
        {
            AddDataError(String.Empty, error);
        }

        public void AddDataError(String propertyName, String error)
        {
            if (!propertyErrors.ContainsKey(propertyName) || propertyErrors[propertyName] != error)
            {
                propertyErrors[propertyName] = error;
                RaiseErrorsChanged(propertyName);
            }
        }

        public void RemoveDataError()
        {
            RemoveDataError(String.Empty);
        }

        public void RemoveDataError(String propertyName)
        {
            if (propertyErrors.ContainsKey(propertyName))
            {
                propertyErrors.Remove(propertyName);
                RaiseErrorsChanged(propertyName);
            }
        }

        private void RaiseErrorsChanged(String propertyName)
        {
            var errorsChanged = ErrorsChanged;
            if (errorsChanged != null)
            {
                errorsChanged(this, new DataErrorsChangedEventArgs(propertyName));
            }
        }

        #endregion INotifyDataErrorInfo
    }
}
