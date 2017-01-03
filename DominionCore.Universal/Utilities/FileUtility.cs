using System;
using System.IO;
using System.Threading.Tasks;

using Windows.Storage;
using Windows.ApplicationModel;


namespace Ben.Utilities
{
    public class FileUtility
    {
        public static async Task<Stream> OpenApplicationStreamAsync(string path)
        {
            return await Package.Current.InstalledLocation.SafeOpenStreamForReadAsync(path).ConfigureAwait(false);
        }

        public static async Task<Stream> OpenUserStreamAsync(string path)
        {
            StorageFolder folder = ApplicationData.Current.LocalFolder;
            IStorageFile file = await folder.TryGetItemAsync(path) as IStorageFile;

            if (file == null)
            {
                return null;
            }

            return await file.OpenStreamForReadAsync();
        }
    }
}
