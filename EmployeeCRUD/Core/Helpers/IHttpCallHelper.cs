using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Helpers
{
    #region using

    using System.Threading.Tasks;

    #endregion

    public interface IHttpCallHelper
    {
        string Get(string url);

        Task<string> GetAsync(string url);

        string Post(string url, string postData);

        Task<string> PostAsync(string url, string postData);
    }
}
