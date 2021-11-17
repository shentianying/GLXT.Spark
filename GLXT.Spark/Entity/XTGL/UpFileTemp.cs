using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace GLXT.Spark.Entity.XTGL
{
    [Table("xtglUpFileTemp")]
    public class UpFileTemp
    {
        public int Id { get; set; }
        [StringLength(100)]
        public string FileName { get; set; }
        [StringLength(100)]
        public string FileType { get; set; }
        [StringLength(100)]
        public string FileValue { get; set; }
        [StringLength(100)]
        public string FilePath { get; set; }
        public long FileSize { get; set; }
        public DateTime CreateDate { get; set; } = DateTime.Now;
    }
}
