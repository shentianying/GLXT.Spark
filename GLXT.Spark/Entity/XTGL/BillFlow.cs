using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GLXT.Spark.Entity.XTGL
{
    /// <summary>
    /// 单据审批流程
    /// </summary>
    [Table("xtglBillFlow")]
    public class BillFlow : BaseCreateUser
    {
        public int Id { get; set; }
        /// <summary>
        /// 公司Id
        /// </summary>
        public int CompanyId { get; set; }
        /// <summary>
        /// 流程Id
        /// </summary>
        public int FlowId { get; set; }
        /// <summary>
        /// 表单Id
        /// </summary>
        public int FormId { get; set; }
        /// <summary>
        /// 单据Id
        /// </summary>
        public int BillId { get; set; }
        /// <summary>
        /// 单据编号
        /// </summary>
        [StringLength(50)]
        public string BillNumber { get; set; }
        /// <summary>
        /// 摘要
        /// </summary>
        [StringLength(500)]
        public string Summary { get; set; }
        /// <summary>
        /// 主表金额
        /// </summary>
        public decimal? Amount { get; set; }
        /// <summary>
        /// 状态
        /// </summary>
        public int State { get; set; } = 0;

        //导航属性
        /// <summary>
        /// 表单
        /// </summary>
        [ForeignKey("FormId")]
        public Form Form { get; set; }

        /// <summary>
        /// 流程节点
        /// </summary>
        public List<BillFlowNode> BillFlowNode { get; set; }
    }
}
