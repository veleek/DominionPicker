using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Windows.UI.Xaml;
using System.Collections;
using System.Runtime.Serialization;
using Windows.UI.Xaml.Media;

namespace Ben.Utilities
{

   public static class FrameworkExtensions
   {

      public static T GetTag<T>(this Object obj)
      {
         T t = default(T);
            FrameworkElement element = obj as FrameworkElement;
         if ( element != null )
         {
            t = (T)element.Tag;
         }
         return t;
      }

      public static T GetContext<T>(this Object obj)
      {
         T t = default(T);
            FrameworkElement element = obj as FrameworkElement;
         if ( element != null )
         {
            t = (T)element.DataContext;
         }
         return t;
      }

      public static TResource GetResource<TResource>(this FrameworkElement element, object key)
      {
         return (TResource)element.Resources[key];
      }

      /// <summary>
      /// Finds a Child of a given item in the visual tree. 
      /// </summary>
      /// <param name="parent">A direct parent of the queried item.</param>
      /// <typeparam name="T">The type of the queried item.</typeparam>
      /// <param name="childName">x:Name or Name of child. </param>
      /// <returns>The first parent item that matches the submitted type parameter. 
      /// If not matching item can be found, 
      /// a null parent is being returned.</returns>
      public static T FindChild<T>(this DependencyObject parent, string childName)
         where T : DependencyObject
        {
         if ( parent == null )
         {
            return null;
         }
         if ( string.IsNullOrWhiteSpace(childName) )
         {
            throw new ArgumentNullException("childName");
         }
         T foundChild = null;
         int childrenCount = Windows.UI.Xaml.Media.VisualTreeHelper.GetChildrenCount(parent);
         for ( int i = 0; i < childrenCount; i++ )
         {
            var child = Windows.UI.Xaml.Media.VisualTreeHelper.GetChild(parent, i);
            // If the child is not of the request child type child
            T childType = child as T;
            if ( childType == null )
            {
               // recursively drill down the tree
               foundChild = FindChild<T>(child, childName);
               // If the child is found, break so we do not overwrite the found child. 
               if ( foundChild != null )
                  break;
            }
            else if ( !string.IsNullOrEmpty(childName) )
            {
               var frameworkElement = child as FrameworkElement;
               // If the child's name is set for search
               if ( frameworkElement != null && frameworkElement.Name == childName )
               {
                  // if the child's name is of the request name
                  foundChild = (T)child;
                  break;
               }
            }
            else
            {
               // child element found.
               foundChild = (T)child;
               break;
            }
         }
         return foundChild;
      }

      public static ObservableCollection<TSource> ToObservableCollection<TSource>(this IEnumerable<TSource> source)
      {
         ObservableCollection<TSource> collection = new ObservableCollection<TSource>();
         foreach ( TSource t in source )
         {
            collection.Add(t);
         }
         return collection;
      }

   }

   public static class SystemExtensions
   {

      public static Double GetMagnitude(this Windows.Foundation.Point p)
      {
         return Math.Sqrt(p.X * p.X + p.Y * p.Y);
      }

   }

}