using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;

namespace Ben.Dominion
{

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
}