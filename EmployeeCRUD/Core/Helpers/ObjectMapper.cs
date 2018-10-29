using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Helpers
{
    using Core.Models;
    #region using

    using System;
    using System.Linq;

    #endregion

    public class ObjectMapper : IObjectMapper
    {
        public UpFile AzureBlobUriToUpFile(Uri uri)
        {
            // NOTE: It is assumed that the Uri to a app storage asset is in the follow format.
            // /{Container Name}/{App ID}/{Platform}/{Path}/{File}
            // For example: /assets/00000000-0000-0000-1111-000000000001/android/Android.UI/res/drawable-hdpi/ic_launcher.png

            // Check if we have a valid Uri to an asset.
            // The value 40 is the fixed length of the App ID and path separators.
            var path = uri.AbsolutePath.Replace("%20", " ");
            if (path.Length < (Constants.Azure.Containers.PageAssets.Length + 40))
            {
                return null;
            }

            var asset = new UpFile();

            // Remove the Asset container name and leading and trailing path separators.
            // Example: 
            // Before: /assets/00000000-0000-0000-1111-000000000001/android/Android.UI/res/drawable-hdpi/ic_launcher.png
            // After: 00000000-0000-0000-1111-000000000001/android/Android.UI/res/drawable-hdpi/ic_launcher.png
            path = path.Substring(Constants.Azure.Containers.PageAssets.Length + 2);
            asset.ResumeId = Guid.ParseExact(path.Substring(0, 36), "D");

            // Remove the App ID and trailing path separator.
            // Example: 
            // Before: 00000000-0000-0000-1111-000000000001/android/Android.UI/res/drawable-hdpi/ic_launcher.png
            // After: android/Android.UI/res/drawable-hdpi/ic_launcher.png
            path = path.Substring(37);
            asset.FullPath = path;
            asset.Path = path.Substring(0, path.LastIndexOf("/"));
            asset.Name = path.Substring(path.LastIndexOf("/") + 1);
            asset.FileType = asset.Name.Split('.').Last();

            return asset;
        }

     
    }
}
