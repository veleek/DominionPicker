using System;
using System.IO.IsolatedStorage;

namespace Ben.Utilities
{
    public static class IsolatedStorageExtensions
    {
        public static TValue TryGetOrDefault<TValue>(this IsolatedStorageSettings settings, string key, TValue defaultValue = default(TValue))
        {
            TValue value;
            return settings.TryGetValue(key, out value) ? value : defaultValue;
        }

        public static int Increment(this IsolatedStorageSettings settings, string key)
        {
            Int32 value = 0;
            settings.TryGetValue(key, out value);
            value++;
            settings[key] = value;

            return value;
        }

        public static T CompareExchange<T>(this IsolatedStorageSettings settings, string key, T value, T comparand)
        {
            T current;
            if (settings.TryGetValue(key, out current))
            {

            }

            return current;
        }

        public static T Replace<T>(this IsolatedStorageSettings settings, string key, T value)
        {
            T current;
            settings.TryGetValue(key, out current);
            settings[key] = value;

            return current;
        }
    }
}
