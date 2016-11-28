using System.Reflection;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using Newtonsoft.Json;

namespace Ben.Utilities
{

    public static class TypeExtensions
    {
        public static Boolean IsFullyDefinedInAssembly(this TypeInfo type, Assembly assembly)
        {
            return type.BaseType == null || type.BaseType == typeof(Object) || type.BaseType.GetTypeInfo().Assembly == assembly;
        }

        public static Boolean IsStatic(this TypeInfo type)
        {
            return type.IsAbstract && type.IsSealed;
        }
    }

    public static class SerializerHelper
    {
        public static Boolean LoadLocalTypes = true;
        private static readonly Assembly CurrentAssembly = typeof(SerializerHelper).GetTypeInfo().Assembly;
        private static readonly List<string> BlacklistedTypes = new List<string>
        {
            "ConfigurationModel"
        };
        private static TypeInfo[] localTypes;

        public static TypeInfo[] LocalTypes
        {
            get
            {
                if (localTypes == null)
                {
                    if (LoadLocalTypes)
                    {
                        localTypes = CurrentAssembly.GetTypes().Select(t => t.GetTypeInfo()).Where(t => t.IsPublic && !t.IsStatic() && !t.IsGenericTypeDefinition && t.IsFullyDefinedInAssembly(CurrentAssembly) && !BlacklistedTypes.Contains(t.Name)).ToArray();
                    }
                    else
                    {
                        localTypes = new TypeInfo[0];
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
                ArraySegment<byte> buffer;
                stream.TryGetBuffer(out buffer);
                String data = Encoding.UTF8.GetString(buffer.Array, 0, (int)stream.Length);
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
                XmlSerializer s = new XmlSerializer(obj.GetType());
                s.Serialize(stream, obj);
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(string.Format("Failed to serialize {0}", obj.GetType()), e);
                throw;
            }
        }

        public static T Deserialize<T>(String value)
           where T : class
        {
            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(value)))
            {
                return Deserialize<T>(stream);
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
                TextReader reader = new StreamReader(stream, Encoding.UTF8);

                try
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(T));
                    t = serializer.Deserialize(reader) as T;
                }
                catch (SerializationException e)
                {
                    String msg = e.Message + (e.InnerException != null ? Environment.NewLine + e.InnerException.Message : "");
                    var messageDialogTask = new Windows.UI.Popups.MessageDialog(msg, "Serialize Exception").ShowAsync();
                }
            }
            catch
            {
                throw;
            }

            return t;
        }

    }

    public static class GenericContractSerializer
    {

        public static async System.Threading.Tasks.Task Serialize<T>(Stream stream, T t)
        {
            if (t == null)
            {
                return;
            }
            try
            {
                var serializer = new DataContractSerializer(t.GetType(), SerializerHelper.LocalTypes.Select(ti => ti.AsType()));
                serializer.WriteObject(stream, t);
            }
            catch (SerializationException e)
            {
                String msg = e.Message + Environment.NewLine + (e.InnerException != null ? e.InnerException.Message : "");
                await (new Windows.UI.Popups.MessageDialog(msg, "Serialize Exception")).ShowAsync();
            }
        }

        public static async void SerializeToIsolatedStorage<T>(string path, T t)
        {
            using (IsolatedStorageFile file = IsolatedStorageFile.GetUserStoreForApplication())
            {
                using (System.IO.Stream stream = file.OpenFile(path, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    await Serialize(stream, t);
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
            var serializer = new DataContractSerializer(typeof(T), SerializerHelper.LocalTypes.Select(ti => ti.AsType()));
            T t = serializer.ReadObject(stream) as T;
            return t;
        }

        public static T DeserializeFromIsolatedStorage<T>(String path)
           where T : class
        {
            using (IsolatedStorageFile file = IsolatedStorageFile.GetUserStoreForApplication())
            {
                using (System.IO.Stream stream = file.OpenFile(path, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    return Deserialize<T>(stream);
                }
            }
        }

    }

    public interface ISerializer<TObject>
    {

        /// <summary>
        /// Deserialize an object from a string
        /// </summary>
        /// <param name="value">The string to deserialize</param>
        /// <returns>The object represented by the serialized string</returns>
        TObject Deserialize(String value);

        /// <summary>
        /// Deserialize an object from a stream
        /// </summary>
        /// <param name="value">The stream to deserialize</param>
        /// <returns>The object represented by the serialized stream</returns>
        TObject Deserialize(Stream value);

        /// <summary>
        /// Serialize an object into a string
        /// </summary>
        /// <param name="value">The object to serialize</param>
        /// <returns>The string representation of the object</returns>
        string Serialize(TObject value);

        /// <summary>
        /// Serialize an object into a stream
        /// </summary>
        /// <param name="stream">The stream to serialize the object into</param>
        /// <param name="value">The object to serialize</param>
        void Serialize(Stream stream, TObject value);

    }

    public abstract class Serializer<TObject>
       : ISerializer<TObject>
    {

        public TObject Deserialize(String value)
        {
            return this.Deserialize(new StringReader(value));
        }

        public TObject Deserialize(Stream value)
        {
            return this.Deserialize(new StreamReader(value));
        }

        public abstract TObject Deserialize(TextReader reader);

        public string Serialize(TObject value)
        {
            StringWriter writer = new StringWriter();
            this.Serialize(writer, value);
            return writer.ToString();
        }

        public void Serialize(Stream stream, TObject value)
        {
            this.Serialize(new StreamWriter(stream), value);
        }

        public abstract void Serialize(TextWriter writer, TObject value);

    }

    public class XmlSerializer<TObject>
       : Serializer<TObject>
    {
        private static XmlSerializer serializer;

        private static XmlSerializer Serializer
        {
            get
            {
                return serializer ?? (serializer = new XmlSerializer(typeof(TObject)));
            }
        }

        public override TObject Deserialize(TextReader reader)
        {
            if (reader == null)
            {
                throw new ArgumentNullException("reader");
            }
            return (TObject)Serializer.Deserialize(reader);
        }

        public override void Serialize(TextWriter writer, TObject value)
        {
            if (writer == null)
            {
                throw new ArgumentNullException("writer");
            }
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            Serializer.Serialize(writer, value);
        }

    }

    public class DataContractSerializer<TObject>
       : Serializer<TObject>
    {
        private static DataContractSerializer serializer;

        private static DataContractSerializer Serializer
        {
            get
            {
                return serializer ?? (serializer = new DataContractSerializer(typeof(TObject)));
            }
        }

        public override TObject Deserialize(TextReader reader)
        {
            if (reader == null)
            {
                throw new ArgumentNullException("reader");
            }
            XmlReader xmlReader = XmlReader.Create(reader);
            return (TObject)Serializer.ReadObject(xmlReader);
        }

        public override void Serialize(TextWriter writer, TObject value)
        {
            if (writer == null)
            {
                throw new ArgumentNullException("writer");
            }
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            XmlWriter xmlWriter = XmlWriter.Create(writer);
            Serializer.WriteObject(xmlWriter, value);
        }

    }

    public class JsonSerializer<TObject>
       : Serializer<TObject>
    {
        private static JsonSerializer serializer = new JsonSerializer();

        private static JsonSerializer Serializer
        {
            get
            {
                return serializer;
            }
        }

        public override TObject Deserialize(TextReader reader)
        {
            if (reader == null)
            {
                throw new ArgumentNullException("reader");
            }
            JsonReader jsonReader = new JsonTextReader(reader);
            return Serializer.Deserialize<TObject>(jsonReader);
        }

        public override void Serialize(TextWriter writer, TObject value)
        {
            if (writer == null)
            {
                throw new ArgumentNullException("writer");
            }
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            Serializer.Serialize(writer, value);
        }

    }

    public static class Serializer
    {

        private static ISerializer<TObject> GetSerializer<TObject>()
        {
            return new XmlSerializer<TObject>();
        }

        /// <summary>
        /// Deserialize an object from a string
        /// </summary>
        /// <param name="value">The string to deserialize</param>
        /// <returns>The object represented by the serialized string</returns>
        public static TObject Deserialize<TObject>(String value)
        {
            return GetSerializer<TObject>().Deserialize(value);
        }

        /// <summary>
        /// Deserialize an object from a stream
        /// </summary>
        /// <param name="value">The stream to deserialize</param>
        /// <returns>The object represented by the serialized stream</returns>
        public static TObject Deserialize<TObject>(Stream value)
        {
            return GetSerializer<TObject>().Deserialize(value);
        }

        /// <summary>
        /// Serialize an object into a string
        /// </summary>
        /// <param name="value">The object to serialize</param>
        /// <returns>The string representation of the object</returns>
        public static string Serialize<TObject>(TObject value)
        {
            return GetSerializer<TObject>().Serialize(value);
        }

        /// <summary>
        /// Serialize an object into a stream
        /// </summary>
        /// <param name="stream">The stream to serialize the object into</param>
        /// <param name="value">The object to serialize</param>
        public static void Serialize<TObject>(Stream stream, TObject value)
        {
            GetSerializer<TObject>().Serialize(stream, value);
        }
    }
}