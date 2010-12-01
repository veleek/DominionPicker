using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using System.Windows.Data;
using System.ComponentModel;

namespace Ben.Dominion
{
    public partial class MainPage : PhoneApplicationPage
    {
        // Constructor
        public MainPage()
        {
            InitializeComponent();

            // Set the data context of the listbox control to the sample data
            this.Loaded += new RoutedEventHandler(MainPage_Loaded);
        }

        // Load data for the ViewModel Items
        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
        }
    }

    public class Cards
    {
        public static IEnumerable<CardSet> AllSets { get { return Enumerable.Range(1, 5).Select(x => (CardSet)x); } }
        public static IEnumerable<CardType> AllTypes { get{ return Enumerable.Range(1, 7).Select(x => (CardType)x); } }

        public static List<Card> AllCards { get; set; }
    }

    public class PickerSettings
    {
        public CardSet Sets { get; set; }

        public PickerOption MinimumCardsPerSet = new IntPickerOption("Minimum cards per set", 3, Enumerable.Range(1, 10).ToList());
        public PickerOption RequirePlusActions = new PickerOption("Require a card that gives actions", false);
        public PickerOption RequirePlusBuys = new PickerOption("Require a card that gives buys", false);
        public PickerOption RequireDefense = new PickerOption("If there's an attack, require a defense card", false);

        public List<PickerOption> AllOptions
        {
            get
            {
                return new List<PickerOption> 
                { 
                    MinimumCardsPerSet,
                    RequirePlusActions,
                    RequirePlusBuys,
                    RequireDefense,
                };

            }
        }

        public PickerSettings() { }

        public PickerSettings Clone()
        {
            PickerSettings clone = new PickerSettings();
            clone.Sets = this.Sets;
            clone.MinimumCardsPerSet = this.MinimumCardsPerSet.Clone();
            clone.RequirePlusActions = this.RequirePlusActions.Clone();
            clone.RequirePlusBuys = this.RequirePlusBuys.Clone();
            clone.RequireDefense = this.RequireDefense.Clone();

            return clone;
        }
    }

    public class PickerOption : INotifyPropertyChanged
    {
        public String Name { get; set; }
        private Object optionValue;
        public Object OptionValue
        {
            get { return optionValue; }
            set
            {
                if (value != optionValue)
                {
                    optionValue = value;
                    NotifyPropertyChanged("OptionValue");
                }
            }
        }

        public PickerOption() { }

        public PickerOption(String name)
        {
            this.Name = name;
        }

        public PickerOption(String name, Object value)
        {
            this.Name = name;
            this.optionValue = value;
        }

        public virtual PickerOption Clone()
        {
            PickerOption clone = this.MemberwiseClone() as PickerOption;
            return clone;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void NotifyPropertyChanged(String propertyName)
        {
            OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
        }

        protected void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            PropertyChangedEventHandler h = PropertyChanged;
            if (h != null)
            {
                h(this, e);
            }
        }
    }

    public class PickerOption<T>
    {
        public T Value { get; set; }
    }

    public class BooleanPickerOption : PickerOption<Boolean> { }

    public class IntPickerOption : PickerOption
    {
        public IntPickerOption(String name, Int32 value, List<Int32> validValues)
            : base(name, value)
        {
            this.ValidValues = validValues;
        }

        public List<Int32> ValidValues { get; set; }
    }

    public class OptionTemplateSelector : DataTemplateSelector
    {
        public DataTemplate BasicOptionTemplate { get; set; }
        public DataTemplate BooleanOptionTemplate { get; set; }
        public DataTemplate IntOptionTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            PickerOption o = item as PickerOption;

            if (o != null)
            {
                if (o.OptionValue is Boolean)
                {
                    return BooleanOptionTemplate;
                }
                else if (o.OptionValue is Int32)
                {
                    return IntOptionTemplate;
                }

                return BasicOptionTemplate;
            }

            return null;
        }
    }

    public enum CardSet
    {
        None,
        Base,
        Intrigue,
        Seaside,
        Alchemy,
        Prosperity,
    Promo,
    }

    public enum CardType
    {
        None,
        Treasure,
        Victory,
        Curse,
        Action,
        Reaction,
        Attack,
        Duration,
    }

    public class Card
    {
        public Boolean InSet(CardSet cardSets)
        {
            return (cardSets & Set) == Set;
        }
        
        public String Name { get; set; }
        public CardSet Set { get; set; }
        public CardType Type { get; set; }
        public Int32 Cost { get; set; }

        public Int32 Cards { get; set; }
        public Int32 Actions { get; set; }
        public Int32 Buys { get; set; }
        public Int32 Treasure { get; set; }

        public Boolean PlusAction { get { return Actions > 0; } }
        public Boolean PlusBuy { get { return Buys > 0; } }
    }
}