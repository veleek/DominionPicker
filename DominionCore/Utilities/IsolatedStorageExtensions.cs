using System;
using System.Reflection;
using Windows.Storage;

namespace Ben.Utilities
{

    public static class ApplicationDataContainerExtensions
    {
        public static TEnum TryGetEnumOrDefault<TEnum>(this ApplicationDataContainer settings, string key, TEnum defaultValue = default(TEnum))
            where TEnum : struct
        {
            string enumRawValue;
            TEnum enumValue;
            if (settings.TryGetValue(key, out enumRawValue) && Enum.TryParse(enumRawValue, out enumValue))
            {
                return enumValue;
            }

            return defaultValue;
        }

        public static TValue TryGetOrDefault<TValue>(this ApplicationDataContainer settings, string key, TValue defaultValue = default(TValue))
        {
            TValue value;
            return settings.TryGetValue(key, out value) ? value : defaultValue;
        }

        public static int Increment(this ApplicationDataContainer settings, string key)
        {
            Int32 value = 0;
            settings.TryGetValue(key, out value);
            value++;
            settings.Values[key] = value;
            return value;
        }

        public static T CompareExchange<T>(this ApplicationDataContainer settings, string key, T value, T comparand)
        {
            T original;
            settings.TryGetValue(key, out original);

            // If they are equal, then replace the current value with the provided value
            if (original == null && comparand == null || (original != null && original.Equals(comparand)))
            {
                settings.Values[key] = value;
            }
            return original;
        }

        public static T Replace<T>(this ApplicationDataContainer settings, string key, T value)
        {
            T original;
            settings.TryGetValue(key, out original);
            settings.Values[key] = typeof(T).GetTypeInfo().IsEnum ? (object)value.ToString() : value;
            return original;
        }

        public static bool TryReplace<T>(this ApplicationDataContainer settings, string key, T value)
        {
            T original = settings.Replace(key, value);
            return !(original == null && value == null || original != null && original.Equals(value));
        }

        public static bool TryGetValue<T>(this ApplicationDataContainer settings, string key, out T value)
        {
            object tmp;
            bool result;
            value = default(T);

            if (result = settings.Values.TryGetValue(key, out tmp))
            {
                if (typeof(T).GetTypeInfo().IsEnum)
                {
                    if (!(tmp is string))
                    {
                        throw new InvalidOperationException($"Unable to parse enum from {tmp.GetType().Name} in app settings.");
                    }

                    try
                    {
                        value = (T)Enum.Parse(typeof(T), tmp as string);
                    }
                    catch
                    {
                        // Ignore this for now.
                    }
                }
                else
                {
                    value = (T)tmp;
                }
            }
            return result;
        }
    }
}