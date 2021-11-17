using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace GLXT.Spark.Entity.XTGL
{
    /// <summary>
    /// 字典
    /// </summary>
   [Table("xtglCity")]
    public class City
    {
        public int Id { get; set; }
        [Column("PId")]
        public int Pid { get; set; }
        public string Name { get; set; }
        public int Level { get; set; }
        public int Sort { get; set; }
        public bool InUse { get; set; } = true;
    }
}
