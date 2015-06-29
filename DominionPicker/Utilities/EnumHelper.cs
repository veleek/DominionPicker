using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ben.Data
{
    class EnumHelper
    {
        private static readonly IDictionary<Type, object[]> Cache = new Dictionary<Type, object[]>();

        public static TEnum[] GetValues<TEnum>(bool excludeDefault = false)
        {
            return GetValues(typeof(TEnum), excludeDefault).Cast<TEnum>().ToArray();
        }

        public static object[] GetValues(Type type, bool excludeDefault = false)
        {
            if (!type.IsEnum)
            {
                throw new ArgumentException("Type '" + type.Name + "' is not an enum");
            }

            object[] values;
            if (!Cache.TryGetValue(type, out values))
            {
                var newValues = Enum.GetValues(type).Cast<object>();

                if (excludeDefault)
                {
                    newValues = newValues.Where(v => v != Activator.CreateInstance(type));
                }

                values = Cache[type] = newValues.ToArray();
            }

            return values;
        }
    }

    public static class EnumExtensions
    {
        public static TEnum[] GetValues<TEnum>(this TEnum enumValue, bool excludeDefault = false)
            where TEnum : struct
        {
            return EnumHelper.GetValues<TEnum>();
        }
    }
}
