using System;
using System.Collections.Generic;
using System.IO;

using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

#if NETFX_CORE
using Windows.Storage;
using Windows.ApplicationModel;
using System.Reflection.Context;
#else
using System.IO.IsolatedStorage;
#endif

namespace Ben.Utilities
{
    public class FileUtility
    {
        public static async Task<Stream> OpenApplicationStreamAsync(string path)
        {
#if NETFX_CORE
            StorageFolder packageLocation = Package.Current.InstalledLocation;
            Assembly callingAssembly = (Assembly)typeof(Assembly).GetMethod("GetCallingAssembly").Invoke(null, new object[0]);
            return await packageLocation.SafeOpenStreamForReadAsync(callingAssembly.FullName + path);
#else
            await Task.Yield();
            // One WP8 all resources go to the root folder so we need to strip off the beginning part
            // and make it a relative path.
            path = path.Replace('\\', '/').TrimStart('/');
            path = "." + path.Substring(path.IndexOf('/'));

            return Microsoft.Xna.Framework.TitleContainer.OpenStream(path);
#endif
        }

        public static async Task<Stream> OpenUserStreamAsync(string path)
        {
            await Task.Yield();
#if NETFX_CORE
            StorageFolder folder = ApplicationData.Current.LocalFolder;
            IStorageFile file = await folder.TryGetItemAsync(path) as IStorageFile;

            if (file == null)
            {
                return null;
            }

            return await file.OpenStreamForReadAsync();
#else
            using (IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (store.FileExists(path))
                {
                    return store.OpenFile(path, FileMode.Open);
                }
            }

            return null;
#endif
        }
    }
}
