using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GLXT.Spark.ViewModel.ZSGL
{
    /// <summary>
    /// 意向企业搜索项
    /// </summary>
    public class EnterpriseSearchViewModel
    {
        /// <summary>
        /// 当前页面
        /// </summary>
        public int currentPage { get; set; }
        /// <summary>
        /// 页数
        /// </summary>
        public int pageSize { get; set; }
        /// <summary>
        /// 名称
        /// </summary>
        public string name { get; set; }
        /// <summary>
        /// 经营状态
        /// </summary>
        public int[] operationStates { get; set; }
        /// <summary>
        /// 注册资本起
        /// </summary>
        public decimal? amount1 { get; set; }
        /// <summary>
        /// 注册资本止
        /// </summary>
        public decimal? amount2 { get; set; }
    }
}
