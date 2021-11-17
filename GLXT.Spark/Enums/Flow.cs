using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GLXT.Spark.Enums
{
    /// <summary>
    /// 审核节点类型
    /// </summary>
    public enum FlowRoleType
    {
        /// <summary>
        /// 角色（岗位）
        /// </summary>
        Role = 1,
        /// <summary>
        /// 制单人
        /// </summary>
        CreateUser = 2,
        /// <summary>
        /// 审核人（需手动选择）
        /// </summary>
        CheckPerson = 3,
        /// <summary>
        /// 行政领导
        /// </summary>
        AdminLeader = 4,
        /// <summary>
        /// 条线领导
        /// </summary>
        LineLeader = 5
    }
}
