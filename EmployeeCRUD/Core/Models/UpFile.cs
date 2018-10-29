using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Models
{
    #region using

    using System;

    #endregion

    public class UpFile
    {
        public DateTime CreatedDate { get; set; }

        public string FileType { get; set; }

        public string FullPath { get; set; }

        public Guid ResumeId { get; set; }

        public string Name { get; set; }

        public string Path { get; set; }

        public int Size { get; set; }
    }
}
