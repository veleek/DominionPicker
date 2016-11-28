using System;
using System.Diagnostics;
using Windows.UI.Xaml.Data;

namespace Ben.Data.Converters
{
    public class ValueWhenConverter : IValueConverter
    {
        public object Value { get; set; }
        public object Otherwise { get; set; }
        public object When { get; set; }
        public object OtherwiseValueBack { get; set; }
        public bool Debug { get; set; }

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (Debug) Debugger.Break();

            try
            {
                if (object.Equals(value, parameter ?? When)) return Value;
            }
            catch
            {
            }

            return Otherwise;

        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (Debug) Debugger.Break();

            if (OtherwiseValueBack == null)
                throw new InvalidOperationException("Cannot ConvertBack if no OtherwiseValueBack is set!");

            try
            {
                if (object.Equals(value, Value)) return When;
            }
            catch
            {
            }

            return OtherwiseValueBack;
        }
    }

    public class ValueWhenWithParameterConverter : IValueConverter
    {
        public object Value { get; set; }

        public object Otherwise { get; set; }

        public object When { get; set; }

        public object OtherwiseValueBack { get; set; }

        public UseParameterAsOption UseParameterAs { get; set; }

        public bool Debug { get; set; }

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (Debug) Debugger.Break();

            try
            {
                if (object.Equals(value, ParamValue(UseParameterAsOption.When, parameter, When)))
                {
                    return ParamValue(UseParameterAsOption.Value, parameter, Value);
                }
            }
            catch
            {
            }

            return ParamValue(UseParameterAsOption.Otherwise, parameter, Otherwise);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (Debug) Debugger.Break();

            if (OtherwiseValueBack == null)
                throw new InvalidOperationException("Cannot ConvertBack if no OtherwiseValueBack is set!");

            try
            {
                if (object.Equals(value, ParamValue(UseParameterAsOption.Value, parameter, Value)))
                    return ParamValue(UseParameterAsOption.When, parameter, When);
            }
            catch
            {
            }

            return ParamValue(UseParameterAsOption.OtherwiseValueBack, parameter, OtherwiseValueBack);
        }

        private object ParamValue(UseParameterAsOption option, object parameter, object value = null)
        {
            return this.UseParameterAs == option ? parameter : value;
        }

        public enum UseParameterAsOption
        {
            None,
            Value,
            When,
            Otherwise,
            OtherwiseValueBack,
        }
    }
}
