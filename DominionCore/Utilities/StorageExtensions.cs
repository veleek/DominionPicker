using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;

namespace Ben.Utilities
{
    public static class StorageExtensions
    {
        public static async Task<Stream> SafeOpenStreamForReadAsync(this StorageFolder folder, string relativePath)
        {
            relativePath = relativePath.Replace('/', '\\');
            return await folder.OpenStreamForReadAsync(relativePath);
        }

        public static async Task<IStorageItem> TryGetItemAsync(this IStorageFolder folder, string relativePath)
        {
            try
            {
                relativePath = relativePath.Replace('/', '\\');
                IStorageItem item = await folder.GetItemAsync(relativePath);
                return item;
            }
            catch
            {
                return null;
            }
        }
    }
}
