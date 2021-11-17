using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GLXT.Spark.Entity.XTGL
{
    /// <summary>
    /// 流程
    /// </summary>
    [Table("xtglFlow")]
    public class Flow:BaseCreateUser
    {
        public int Id { get; set; }
        /// <summary>
        /// 公司Id
        /// </summary>
        public int CompanyId { get; set; }
        /// <summary>
        /// 表单Id
        /// </summary>
        public int FormId { get; set; }
        /// <summary>
        /// 流程名称
        /// </summary>
        [StringLength(200)]
        public string Name { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        [StringLength(1000)]
        public string Remark { get; set; }
        /// <summary>
        /// 条件说明
        /// </summary>
        [StringLength(1000)]
        public string ConditionDescription { get; set; }
        
        /// <summary>
        /// 排序号
        /// </summary>
        public double Sort { get; set; }
        /// <summary>
        /// 是否可用
        /// </summary>
        public bool InUse { get; set; }

        //导航属性
        /// <summary>
        /// 流程节点
        /// </summary>
        public List<FlowNode> FlowNode { get; set; }
        /// <summary>
        /// 表单
        /// </summary>
        [ForeignKey("FormId")]        
        public Form Form { get; set; }
        /// <summary>
        /// 流程条件
        /// </summary>
        public List<FlowCondition> FlowCondition { get; set; }
    } 
}
