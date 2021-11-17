using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GLXT.Spark.Enums
{
    /// <summary>
    ///  审批状态
    /// </summary>
    public enum AttitudeEnum
    {
        已审批=1,
        未审批=2,
        待审批中没有我的审批=3,
        没有待审批节点已完成=4,
    }
}
