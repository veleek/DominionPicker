using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Collections.ObjectModel;
using System.Collections.Generic;

namespace Ben.Dominion
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
}
