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
        public virtual ModelStatus ChangeStatus
        {
            get
            {
                if (this.IsDeleted)
                {
                    return ModelStatus.Deleted;
                }

                if (this.CreatedDate == this.UpdatedDate)
                {
                    return ModelStatus.New;
                }

                return ModelStatus.Updated;
            }
        }

        //// public virtual int CreatedBy { get; set; }
        public virtual DateTime CreatedDate { get; set; }

        public virtual bool IsDeleted { get; set; }

        //// public virtual int UpdatedBy { get; set; }
        public virtual DateTime UpdatedDate { get; set; }
    }
}
