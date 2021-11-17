using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GLXT.Spark.Entity.XTGL;

namespace GLXT.Spark.ViewModel.XTGL
{

    /// <summary>
    /// 常规审批
    /// </summary>
    public class CheckViewModel
    {

        /// <summary>
        /// id
        /// </summary>
        public int id { get; set; }

        /// <summary>
        /// 审批信息
        /// </summary>
        public Attitude attitude { get; set; }
    }
}
