using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Xml.Serialization;

namespace Ben.Dominion
{
    public static class GenericSerializer
    {
        private static Type[] LocalTypes;
        private static Assembly CurrentAssembly;

        static GenericSerializer()
        {
            CurrentAssembly = Assembly.GetExecutingAssembly();
            /*LocalTypes = CurrentAssembly.GetTypes()
                .Where(t => t.IsPublic
                            && !t.IsStatic()
                            && !t.IsGenericTypeDefinition
                            && t.IsFullyDefinedInAssembly(CurrentAssembly))
                .ToArray();
             */
            LocalTypes = new Type[]
            {
                typeof(Cards),
                typeof(Card),
            };
        }

        public static Boolean IsFullyDefinedInAssembly(this Type type, Assembly assembly)
        {
            return type.BaseType == null || type.BaseType == typeof(Object) || type.BaseType.Assembly == assembly;
        }

        public static Boolean IsStatic(this Type type)
        {
            return type.IsAbstract && type.IsSealed;
        }

        public static string Serialize(Object o)
        {
            String data = "";

            using (MemoryStream stream = new MemoryStream())
            {
                Serialize(stream, o);
                data = Encoding.UTF8.GetString(stream.GetBuffer(), 0, (int)stream.Length);
            }

            return data;
        }

        public static void Serialize(Stream stream, Object obj)
        {
            if (obj == null)
            {
                return;
            }

            try
            {
                XmlSerializer s = new XmlSerializer(obj.GetType(), LocalTypes);

                s.Serialize(stream, obj);
            }
            catch (Exception e)
            {
                String msg = e.Message + Environment.NewLine + e.InnerException != null ? e.InnerException.Message : "";
                MessageBox.Show(msg, "Serialize Exception", MessageBoxButton.OK);
            }
        }

        public static T Deserialize<T>(Stream stream)
            where T : class
        {
            T t = null;
            if (stream == null)
            {
                return null;
            }

            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(T), LocalTypes);

                t = serializer.Deserialize(stream) as T;
            }
            catch (Exception e)
            {
                String msg = e.Message + Environment.NewLine + (e.InnerException != null ? e.InnerException.Message : "");
                MessageBox.Show(msg, "Serialize Exception", MessageBoxButton.OK);
            }

            return t;
        }
    }
}
