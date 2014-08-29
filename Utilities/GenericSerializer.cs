using System;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Windows;
using System.Xml.Serialization;

namespace Ben.Utilities
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
        private const Boolean LoadLocalTypes = true;
        private static readonly Assembly CurrentAssembly = Assembly.GetExecutingAssembly();
        private static readonly List<string> BlacklistedTypes = new List<string> { "ConfigurationModel" };

        private static Type[] localTypes;
        public static Type[] LocalTypes 
        {
            get
            {
                if (localTypes == null)
                {
                    if (LoadLocalTypes)
                    {
                        localTypes = CurrentAssembly.GetTypes()
                            .Where(t => t.IsPublic
                                        && !t.IsStatic()
                                        && !t.IsGenericTypeDefinition
                                        && t.IsFullyDefinedInAssembly(CurrentAssembly)
                                        && ! BlacklistedTypes.Contains(t.Name))
                            .ToArray();
                    }
                    else
                    {
                        localTypes = Type.EmptyTypes;
                    }
                }

                return localTypes;
            }
        }
    }

    public static class GenericXmlSerializer
    {
        public static string Serialize(Object o)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                Serialize(stream, o);
                String data = Encoding.UTF8.GetString(stream.GetBuffer(), 0, (int)stream.Length);
                return data;
            }
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
            catch (SerializationException e)
            {
                String msg = e.Message + Environment.NewLine + (e.InnerException != null ? e.InnerException.Message : "");
                MessageBox.Show(msg, "Serialize Exception", MessageBoxButton.OK);
            }
        }

        public static T Deserialize<T>(String value)
            where T : class
        {
            using (MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(value)))
            {
                return Deserialize<T>(ms);
            }
        }

        public static T Deserialize<T>(Stream stream)
            where T : class
        {
            return Deserialize<T>(stream, Type.EmptyTypes);
        }

        public static T Deserialize<T>(Stream stream, Type[] extraTypes)
            where T : class
        {
            T t = null;
            if (stream == null)
            {
                return null;
            }
           
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(T), extraTypes);
                //XmlSerializer serializer = new XmlSerializer(typeof(T), SerializerHelper.LocalTypes);

                t = serializer.Deserialize(stream) as T;
            }
            catch (SerializationException e)
            {
                String msg = e.Message + (e.InnerException != null ? Environment.NewLine + e.InnerException.Message : "");
                MessageBox.Show(msg, "Serialize Exception", MessageBoxButton.OK);
            }

            return t;
        }
    }

    public static class GenericContractSerializer
    {
        public static void Test<T>(T original)
            where T : class
        {
            using (MemoryStream stream = new MemoryStream())
            {
                Serialize(stream, original);
                //String data = Encoding.UTF8.GetString(stream.GetBuffer(), 0, (int)stream.Length);
                stream.Seek(0, SeekOrigin.Begin);
                T copy = Deserialize<T>(stream);

                if (copy == null) 
                {
                    MessageBox.Show("Unable to deserialize object");
                }
                else if (!copy.Equals(original))
                {
                    MessageBox.Show("Deserialized object is not equal to original");
                }
            }
        }

        public static string Serialize<T>(T t)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                Serialize(stream, t);
                return Encoding.UTF8.GetString(stream.GetBuffer(), 0, (int)stream.Length);
            }
        }

        public static void Serialize<T>(Stream stream, T t)
        {
            if (t == null)
            {
                return;
            }

            try
            {
                var serializer = new DataContractSerializer(t.GetType(), SerializerHelper.LocalTypes);
                serializer.WriteObject(stream, t);
            }
            catch (SerializationException e)
            {
                String msg = e.Message + Environment.NewLine + (e.InnerException != null ? e.InnerException.Message : "");
                MessageBox.Show(msg, "Serialize Exception", MessageBoxButton.OK);
            }
        }

        public static void SerializeToIsolatedStorage<T>(string path, T t)
        {
            using (IsolatedStorageFile file = IsolatedStorageFile.GetUserStoreForApplication())
            {
                using (IsolatedStorageFileStream stream = file.OpenFile(path, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    Serialize(stream, t);
                }
            }
        }

        public static T Deserialize<T>(String value)
            where T : class
        {
            using (MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(value)))
            {
                return Deserialize<T>(ms);
            }
        }

        public static T Deserialize<T>(Stream stream)
            where T : class
        {
            if (stream == null)
            {
                return null;
            }

            var serializer = new DataContractSerializer(typeof(T), SerializerHelper.LocalTypes);
            T t = serializer.ReadObject(stream) as T;

            return t;
        }

        public static T DeserializeFromIsolatedStorage<T>(String path)
            where T : class
        {
            using (IsolatedStorageFile file = IsolatedStorageFile.GetUserStoreForApplication())
            {
                using (IsolatedStorageFileStream stream = file.OpenFile(path, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    return Deserialize<T>(stream);
                }
            }
        }
    }
}
