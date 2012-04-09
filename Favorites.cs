using System;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Runtime.Serialization;
using System.Collections.Generic;
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
            get { return name; }
            set
            {
                if (value != null)
                {
                    name = value;
                    NotifyPropertyChanged("Name");
                }
            }
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
    public class FavoriteSetting : FavoriteThing<PickerSettings>
    {
        public FavoriteSetting() { }

        public FavoriteSetting(String name, PickerSettings value) : base(name, value) { }
    }

    [DataContract]
    public class FavoriteSet : FavoriteThing<String>
    {
        public PickerResult Result
        {
            get { return PickerResult.FromList(Value.Split(',').Select(i => Int32.Parse(i))); }
        }

        public FavoriteSet() { }

        public FavoriteSet(String name, PickerResult result)
            : base(name, result.ToList().Aggregate<int, string>("", (a,b) => a + "," + b).Trim(','))
        { }
    }
}
