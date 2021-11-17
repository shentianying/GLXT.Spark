using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace GLXT.Spark.Entity.XTGL
{
    /// <summary>
    /// 表单字段（与审批流程有关）
    /// </summary>
    [Table("xtglFormFlowField")]
    public class FormFlowField : BaseCreateUser
    {
        public int Id { get; set; }
        /// <summary>
        /// 表单Id
        /// </summary>
        [Column("FormId")]
        public int FormId { get; set; }
        /// <summary>
        /// 字段（属性）
        /// </summary>
        [StringLength(100)]
        public string Field { get; set; }
        /// <summary>
        /// 字段属性
        /// </summary>
        [StringLength(200)]
        public string FieldName { get; set; }
        /// <summary>
        /// 字段类型
        /// </summary>
        [StringLength(50)]
        public string FieldType { get; set; }
        /// <summary>
        /// 字典类型
        /// </summary>
        [StringLength(50)]
        public string DicType { get; set; }
        /// <summary>
        /// 是否可用
        /// </summary>
        public bool InUse { get; set; }

        //导航属性
        /// <summary>
        /// 表单
        /// </summary>
        [ForeignKey("FormId")]        
        public Form Form  { get; set; }
    }
}
