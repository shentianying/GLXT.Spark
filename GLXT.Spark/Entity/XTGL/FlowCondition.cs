using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace GLXT.Spark.Entity.XTGL
{
    /// <summary>
    /// 流程执行条件
    /// </summary>
    [Table("xtglFlowCondition")]
    public class FlowCondition
    {
        public int Id { get; set; }
        /// <summary>
        /// 流程Id
        /// </summary>
        public int FlowId { get; set; }
        /// <summary>
        /// 编号
        /// </summary>
        public int Code { get; set; }
        /// <summary>
        /// 父编号
        /// </summary>
        public int PCode { get; set; }
        /// <summary>
        /// 表单字段Id
        /// </summary>
        public int? FormFlowFieldId { get; set; }
        /// <summary>
        /// 运算符（包括逻辑运算符及比较运算符）
        /// </summary>
        [StringLength(50)]
        public string Operator { get; set; }
        /// <summary>
        /// 要对比的值
        /// </summary>
        [StringLength(500)]
        public string Value { get; set; }
        /// <summary>
        /// 是否为叶节点
        /// </summary>
        public bool IsLeaf { get; set; }

        // 导航属性
        /// <summary>
        /// 表单字段
        /// </summary>
        [ForeignKey("FormFlowFieldId")]
        public FormFlowField FormFlowField { get; set; }
    }
}
