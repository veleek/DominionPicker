using System.IO;
using System.Threading.Tasks;

using System.IO.IsolatedStorage;

namespace Ben.Utilities
{
    public class FileUtility
    {
        public static async Task<Stream> OpenApplicationStreamAsync(string path)
        {
            await Task.Yield();
            // One WP8 all resources go to the root folder so we need to strip off the beginning part
            // and make it a relative path.
            path = path.Replace('\\', '/').TrimStart('/');
            path = "." + path.Substring(path.IndexOf('/'));

            return Microsoft.Xna.Framework.TitleContainer.OpenStream(path);
        }

        public static async Task<Stream> OpenUserStreamAsync(string path)
        {
            await Task.Yield();
            using (IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (store.FileExists(path))
                {
                    return store.OpenFile(path, FileMode.Open);
                }
            }

            return null;
        }
    }
}
