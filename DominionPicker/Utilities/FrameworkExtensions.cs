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

        public static TResource GetResource<TResource>(this FrameworkElement element, object key)
        {
            return (TResource)element.Resources[key];
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
}
