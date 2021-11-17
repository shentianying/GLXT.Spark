using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GLXT.Spark.Entity.XTGL
{
    /// <summary>
    /// 表单
    /// </summary>
    [Table("xtglForm")]
    public class Form : BaseCreateUser
    {
        public int Id { get; set; }
        /// <summary>
        /// 是否需要审批
        /// </summary>
        public bool NeedCheckup { get; set; }
        /// <summary>
        /// 表单名称（中文）
        /// </summary>
        [StringLength(200)]
        public string Name { get; set; }
        /// <summary>
        /// 数据库表名
        /// </summary>
        [StringLength(100)]
        public string Value { get; set; }
        /// <summary>
        /// 是否可用
        /// </summary>
        public bool InUse { get; set; }
        /// <summary>
        /// 页面外键
        /// </summary>

        public int? PageId { get; set; }

        //导航属性
        /// <summary>
        /// 表单字段（用于流程控制）
        /// </summary>
        public List<FormFlowField> FormFlowField { get; set; }
        public List<FormState> FormState { get; set; }
        [ForeignKey("PageId")]
        public Page Page { get; set; }
    }
}
