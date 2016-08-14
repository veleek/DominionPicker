using System.Collections.ObjectModel;
using Ben.Utilities;

namespace Ben.Dominion
{

   public class FavoritesViewModel
      : NotifyPropertyChangedBase
   {

      public FavoritesViewModel()
      {
         this.FavoriteSettings = new ObservableCollection<FavoriteSetting>();
         this.FavoriteSets = new ObservableCollection<FavoriteSet>();
      }

      /// <summary>
      /// A saved list of favorite settings
      /// </summary>
      /// <remarks>
      /// Observable collection implements INotifyPropertyChanged, and we shouldn't be 
      /// actually changing the collection instances in this class, so don't bother with 
      /// NotifyPropertyChanged stuff on this class.
      /// </remarks>
      public ObservableCollection<FavoriteSetting> FavoriteSettings { get; set; }

      /// <summary>
      /// A saved list of favorite sets
      /// </summary>
      /// <remarks>
      /// Observable collection implements INotifyPropertyChanged, and we shouldn't be 
      /// actually changing the collection instances in this class, so don't bother with 
      /// NotifyPropertyChanged stuff on this class.
      /// </remarks>
      public ObservableCollection<FavoriteSet> FavoriteSets { get; set; }

   }

}