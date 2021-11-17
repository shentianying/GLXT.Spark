using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using GLXT.Spark.Entity.RSGL;

namespace GLXT.Spark.Entity.XTGL
{
    /// <summary>
    /// 表单
    /// </summary>
    [Table("xtglSystemException")]
    public class SystemExceptions
    {
        public int Id { get; set; }
        public string ErrorInfo { get; set; }
        public DateTime CreateTime { get; set; } = DateTime.Now;
        public string TriggerUserName { get; set; }
        public int TriggerUserId { get; set; }

        [ForeignKey("TriggerUserId")]
        public Person Person { get; set; }
    }
}
