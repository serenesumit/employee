using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core
{
    public enum Method
    {
        GET,

        POST,

        PUT,

        DELETE
    }

    public enum ModelStatus
    {
        New = 1,

        Updated = 2,

        Deleted = 3
    }
}
