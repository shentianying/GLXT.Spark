using System;
using System.ComponentModel.DataAnnotations;
using GLXT.Spark.Entity.XTGL;

namespace GLXT.Spark.Entity
{
    /// <summary>
    /// 包含审批流
    /// </summary>
    public class BaseCheckup : BaseCreateUser
    {
        /// <summary>
        /// 单据审批流程Id
        /// </summary>
        public int? BillFlowId { get; set; }
        /// <summary>
        /// 提交人ID
        /// </summary>
        public int? SubmitUserId { get; set; }
        /// <summary>
        /// 提交人姓名
        /// </summary>
        [StringLength(40)]
        public string SubmitUserName { get; set; }
        /// <summary>
        /// 提交日期
        /// </summary>
        public DateTime? SubmitDate { get; set; }

        //导航属性
        /// <summary>
        /// 单据审批流
        /// </summary>
        public BillFlow BillFlow { get; set; }
    }
}
