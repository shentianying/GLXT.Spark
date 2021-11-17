using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GLXT.Spark.Enums
{
    /// <summary>
    ///  审批组件的状态
    /// </summary>
    public enum AttitudeTypeEnum
    {
        制单 =0,
        转到下一步 = 1,
        只填写意见不转下一步 =2,
        退回=3,
        作废=-1,
        撤销=-2,
        完成=10000
    }
}
