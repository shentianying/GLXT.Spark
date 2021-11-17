using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GLXT.Spark.Entity.XTGL;

namespace GLXT.Spark.ViewModel.XTGL
{
    /// <summary>
    /// 带金额审批
    /// </summary>
    public class CheckAmountViewModel
    {
        public int id { get; set; }

        public decimal amount { get; set; }

        public Attitude attitude { get; set; }
    }
}
