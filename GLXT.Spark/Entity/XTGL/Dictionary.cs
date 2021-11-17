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
   [Table("xtglDictionary")]
    public class Dictionary
    {
        public int Id { get; set; }
        public int PId { get; set; }
        public int CompanyId { get; set; }
        public string Name { get; set; }
        public int Value { get; set; }
        public string Remark { get; set; }
        public string Type { get; set; }
        public string TypeName { get; set; }
        public int Sort { get; set; }
        public bool InUse { get; set; } = true;

        [ForeignKey("CompanyId")]
        public Organization Company { get; set; }
    }
}
