using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace GLXT.Spark.Entity.XTGL
{
    /// <summary>
    /// 流程节点
    /// </summary>
    [Table("xtglFlowNode")]
    public class FlowNode
    {
        public int Id { get; set; }
        /// <summary>
        /// 流程Id
        /// </summary>
        public int FlowId { get; set; }
        /// <summary>
        /// 分组号
        /// </summary>
        public int Group { get; set; }
        /// <summary>
        /// 审批模式
        /// </summary>
        public int Mode { get; set; }
        /// <summary>
        /// 操作类型（无操作时为0）
        /// </summary>
        public int Option { get; set; }
        /// <summary>
        /// 角色类型（角色（岗位）、制单人、审核人、行政领导、条线领导）
        /// </summary>
        public int RoleType { get; set; }
        /// <summary>
        /// 角色Id
        /// </summary>
        public int RoleId { get; set; }
        /// <summary>
        /// 节点类型（制单、审核、完成、支付、事后审核等）
        /// </summary>
        public int State { get; set; } = 0;
        /// <summary>
        /// 最长审批天数，超时将自动跳到下一流程。不限制时0
        /// </summary>
        public int MaxDays { get; set; } = 0;

        /// <summary>
        /// 紧急程度 0，普通 1重要 2 非常重要
        /// </summary>
        public int Grade { get; set; } = 0;

        // 导航属性
        public Flow Flow { get; set; }
        [ForeignKey("RoleId")]
        public Role Role { get; set; }
    }
}
