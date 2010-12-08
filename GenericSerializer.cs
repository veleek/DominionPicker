using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Xml.Serialization;
using System.Runtime.Serialization;

namespace Ben.Dominion
{
    public static class TypeExtensions
    {
        public static Boolean IsFullyDefinedInAssembly(this Type type, Assembly assembly)
        {
            return type.BaseType == null || type.BaseType == typeof(Object) || type.BaseType.Assembly == assembly;
        }

        public static Boolean IsStatic(this Type type)
        {
            return type.IsAbstract && type.IsSealed;
        }
    }

    public static class SerializerHelper
    {
        public static Type[] LocalTypes { get; set; }
        private static readonly Boolean LoadLocalTypes = false;
        private static Assembly CurrentAssembly;

        static SerializerHelper()
        {
            CurrentAssembly = Assembly.GetExecutingAssembly();

            if (LoadLocalTypes)
            {
                LocalTypes = CurrentAssembly.GetTypes()
                    .Where(t => t.IsPublic
                                && !t.IsStatic()
                                && !t.IsGenericTypeDefinition
                                && t.IsFullyDefinedInAssembly(CurrentAssembly))
                    .ToArray();
            }
            else
            {
                LocalTypes = Type.EmptyTypes;
            }
        }
    }

    public static class GenericXmlSerializer
    {
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
                XmlSerializer s = new XmlSerializer(obj.GetType(), SerializerHelper.LocalTypes);

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
                XmlSerializer serializer = new XmlSerializer(typeof(T), SerializerHelper.LocalTypes);

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

    public class GenericContractSerializer
    {
        public static void Test<T>(T original)
            where T : class
        {
            using (MemoryStream stream = new MemoryStream())
            {
                Serialize(stream, original);
                String data = Encoding.UTF8.GetString(stream.GetBuffer(), 0, (int)stream.Length);
                stream.Seek(0, SeekOrigin.Begin);
                T copy = Deserialize<T>(stream);

                if (!copy.Equals(original)) { MessageBox.Show("Not equal!"); }
            }
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
                var serializer = new DataContractSerializer(obj.GetType(), SerializerHelper.LocalTypes);
                serializer.WriteObject(stream, obj);
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
                var serializer = new DataContractSerializer(typeof(T), SerializerHelper.LocalTypes);
                t = serializer.ReadObject(stream) as T;
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
