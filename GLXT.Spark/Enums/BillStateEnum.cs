using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace GLXT.Spark.Enums
{
    /// <summary>
    /// 单据状态
    /// </summary>
    public enum BillStateEnum
    {
        /// <summary>
        /// 制单
        /// </summary>
        [Description("制单")]
        Create =0,
        /// <summary>
        /// 审批
        /// </summary>
        [Description("审批")]
        Checkup = 1,
        /// <summary>
        /// 用章
        /// </summary>
        [Description("用章")]
        Seal = 100,
        /// <summary>
        /// 支付
        /// </summary>
        [Description("支付")]
        payment = 1000,
        /// <summary>
        /// 存档
        /// </summary>
        [Description("存档")]
        Save = 1500,
        /// <summary>
        /// 完成
        /// </summary>
        [Description("完成")]
        Finish = 10000,
        /// <summary>
        /// 作废
        /// </summary>
        [Description("作废")]
        Invalid = -1
    }
}
