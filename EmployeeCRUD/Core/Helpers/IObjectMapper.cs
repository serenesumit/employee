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


    #endregion

    public interface IObjectMapper
    {
        UpFile AzureBlobUriToUpFile(Uri uri);

       
    }
}
