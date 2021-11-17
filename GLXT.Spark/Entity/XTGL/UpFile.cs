using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace GLXT.Spark.Entity.XTGL
{
    [Table("xtglUpFile")]
    public class UpFile:BaseCreateUser
    {
        public int Id { get; set; }
        public int TableId { get; set; }
        [StringLength(50)]
        public string TableName { get; set; }
        [StringLength(50)]
        public string ColumnName { get; set; }
        [StringLength(100)]
        public string FileName { get; set; }
        [StringLength(100)]
        public string FileValue { get; set; }
        [StringLength(100)]
        public string FilePath { get; set; }
        [StringLength(100)]
        public string FileType { get; set; }
        public long FileSize { get; set; }

    }
}
