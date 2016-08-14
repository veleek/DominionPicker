using System;
using System.Net;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Documents;
using System.Windows.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;

namespace Ben.Dominion
{

   public enum ResultSortOrder
   {None,
   Name,
   Cost,
   Set,
   }

   public static class ResultSortOrderExtensions
   {

      public static ResultSortOrder Next(this ResultSortOrder sortOrder)
      {
         switch ( sortOrder )
         {
            case ResultSortOrder.Name:
               return ResultSortOrder.Cost;
            case ResultSortOrder.Cost:
               return ResultSortOrder.Set;
            case ResultSortOrder.Set:
               return ResultSortOrder.Name;
            default:
               return ResultSortOrder.Name;
         }
      }

   }

}