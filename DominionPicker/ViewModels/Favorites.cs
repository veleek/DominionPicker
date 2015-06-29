using System;
using System.Linq;
using System.Runtime.Serialization;
using Ben.Utilities;

namespace Ben.Dominion
{
    [DataContract]
    public class FavoriteThing<TFavoriteType> : NotifyPropertyChangedBase
    {
        private String name;
        [DataMember]
        public String Name
        {
            get { return this.name; }
            set { this.SetProperty(ref this.name, value); }
        }

        [DataMember]
        public TFavoriteType Value { get; set; }

        public FavoriteThing() { }

        public FavoriteThing(String name, TFavoriteType value)
        {
            this.Name = name;
            this.Value = value;
        }
    }

    [DataContract]
    public class FavoriteSetting : FavoriteThing<SettingsViewModel>
    {
        public FavoriteSetting() { }

        public FavoriteSetting(String name, SettingsViewModel value) : base(name, value) { }
    }

    [DataContract]
    public class FavoriteSet : FavoriteThing<PickerResult>
    {
        public FavoriteSet() { }

        public FavoriteSet(String name, PickerResult result)
            : base(name, result)
        { }
    }

    [DataContract(Name = "FavoriteSetting")]
    public class OldFavoriteSetting : FavoriteThing<PickerSettings>
    {
        public OldFavoriteSetting() { }

        public OldFavoriteSetting(String name, PickerSettings result)
            : base(name, result)
        {
        }
    }

    [DataContract(Name="FavoriteSet")]
    public class OldFavoriteSet : FavoriteThing<String>
    {
        public PickerResult Result
        {
            get { return PickerResult.FromList(this.Value.Split(',').Select(i => Int32.Parse(i))); }
        }

        public OldFavoriteSet() { }

        public OldFavoriteSet(String name, PickerResult result)
            : base(name, result.ToList().Aggregate<int, string>("", (a, b) => a + "," + b).Trim(','))
        { }
    }
}
