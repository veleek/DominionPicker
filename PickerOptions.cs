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

    public class BooleanPickerOption : PickerOption 
    {
        public BooleanPickerOption() { }
        public BooleanPickerOption(String name) : base(name, false) { }
        public BooleanPickerOption(String name, Boolean value) : base(name, value) { }
    }

    public class IntPickerOption : BooleanPickerOption
    {
        private Int32 selectedValue;
        public Int32 SelectedValue
        {
            get { return selectedValue; }
            set
            {
                if (value != selectedValue)
                {
                    selectedValue = value;
                    NotifyPropertyChanged("SelectedValue");
                }
            }
        }
        public List<Int32> ValidValues { get; set; }

        public IntPickerOption() { }
        public IntPickerOption(String name, Int32 selectedValue, List<Int32> validValues)
            : base(name)
        {
            this.SelectedValue = selectedValue;
            this.ValidValues = validValues;
        }

        
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
                if (o is IntPickerOption)
                {
                    return IntOptionTemplate;
                }
                else if (o is BooleanPickerOption)
                {
                    return BooleanOptionTemplate;
                }                

                return BasicOptionTemplate;
            }

            return null;
        }
    }
}