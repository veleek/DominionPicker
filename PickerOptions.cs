using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Runtime.Serialization;

namespace Ben.Dominion
{
    public class PickerOption : INotifyPropertyChanged, IEquatable<PickerOption>
    {
        public String Name { get; set; }
        public String Notes { get; set; }

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

        public PickerOption() : this("", null) { }

        public PickerOption(String name) : this(name, null) { }

        public PickerOption(String name, Object value)
        {
            this.Name = name;
            this.optionValue = value;
            this.Notes = "";
        }

        public virtual PickerOption Clone()
        {
            return this.Clone<PickerOption>();
        }

        public T Clone<T>()
            where T : PickerOption
        {
            T clone = this.MemberwiseClone() as T;
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

        public override bool Equals(object obj)
        {
            PickerOption option = obj as PickerOption;

            if (option == null)
            {
                return false;
            }

            return this.Equals(option);
        }

        public bool Equals(PickerOption other)
        {
            return this.Name.Equals(other.Name) 
                && this.OptionValue.Equals(other.OptionValue)
                && this.Notes.Equals(other.Notes);
        }
    }
    
    public class BooleanPickerOption : PickerOption 
    {
        public Boolean IsEnabled
        {
            get { return (Boolean)OptionValue; }
            set { OptionValue = value; }
        }
    
        public BooleanPickerOption() { }
        public BooleanPickerOption(String name) : base(name, false) { }
        public BooleanPickerOption(String name, Boolean value) : base(name, value) { }
    }

    /// <summary>
    /// This is just used to make picking the template easier
    /// </summary>
    public abstract class ListPickerOption : BooleanPickerOption
    {
        /// <summary>
        /// Don't use this.  It's presence works around what appears to be a
        /// bug in WP7 where no data will be serialized for a class if it
        /// doesn't have a direct DataMember.
        /// </summary>
        public int Unused { get; set; }

        public ListPickerOption() { }
        public ListPickerOption(String name) : base(name, false) { }
    }

    public class ListPickerOption<T> : ListPickerOption
    {
        private T selectedValue;
        public T SelectedValue
        {
            get { return selectedValue; }
            set
            {
                if (!value.Equals(selectedValue))
                {
                    selectedValue = value;
                    NotifyPropertyChanged("SelectedValue");
                }
            }
        }

        public List<T> ValidValues { get; set; }

        public ListPickerOption() { }
        public ListPickerOption(String name, List<T> validValues) : this(name, validValues, validValues[0]) { }
        public ListPickerOption(String name, List<T> validValues, T selectedValue)
            : base(name)
        {
            this.SelectedValue = selectedValue;
            this.ValidValues = validValues;
        }
    }

    public class PolicyOption : ListPickerOption<String>
    {
        public static String Require = "Require";
        public static String Prevent = "Prevent";
        public static List<String> PolicyOptions = new List<String> { Require, Prevent };

        public Boolean IsRequired
        {
            get { return SelectedValue == Require; }
            set { SelectedValue = value ? Require : Prevent; }
        }

        public PolicyOption() { }
        public PolicyOption(String name) : base(name, PolicyOptions) {}
    }

    public class OptionTemplateSelector : DataTemplateSelector
    {
        public DataTemplate BasicOptionTemplate { get; set; }
        public DataTemplate BooleanOptionTemplate { get; set; }
        public DataTemplate ListOptionTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            PickerOption o = item as PickerOption;

            if (o != null)
            {
                if (o is ListPickerOption)
                {
                    return ListOptionTemplate;
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