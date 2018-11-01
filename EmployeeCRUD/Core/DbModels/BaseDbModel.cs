using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.DbModels
{
    #region using

    using System;

    #endregion

    public abstract class BaseDbModel
    {
      
        //// public virtual int CreatedBy { get; set; }
        public virtual DateTime CreatedDate { get; set; }

        public virtual bool IsDeleted { get; set; }

        //// public virtual int UpdatedBy { get; set; }
        public virtual DateTime UpdatedDate { get; set; }
    }
}
