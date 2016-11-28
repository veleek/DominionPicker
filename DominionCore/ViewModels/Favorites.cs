using System;
using System.Linq;
using System.Runtime.Serialization;
using Ben.Utilities;

namespace Ben.Dominion.ViewModels
{

   [DataContract]
   public class FavoriteThing<TFavoriteType>
      : NotifyPropertyChangedBase
   {
      private String name;

      [DataMember]
      public String Name
      {
         get
         {
            return this.name;
         }
         set
         {
            this.SetProperty(ref this.name, value);
         }
      }

      [DataMember]
      public TFavoriteType Value { get; set; }


      public FavoriteThing()
      {
      }

      public FavoriteThing(String name, TFavoriteType value)
      {
         this.Name = name;
         this.Value = value;
      }
   }

   [DataContract]
   public class FavoriteSetting
      : FavoriteThing<SettingsViewModel>
   {

      public FavoriteSetting()
      {
      }

      public FavoriteSetting(String name, SettingsViewModel value)
      : base(name, value)
      {
      }
   }

   [DataContract]
   public class FavoriteSet
      : FavoriteThing<PickerResult>
   {

      public FavoriteSet()
      {
      }

      public FavoriteSet(String name, PickerResult result)
      : base(name, result)
      {
      }
   }
}