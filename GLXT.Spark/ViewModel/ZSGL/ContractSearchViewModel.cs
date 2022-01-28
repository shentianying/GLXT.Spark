using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GLXT.Spark.ViewModel.ZSGL
{
    /// <summary>
    /// 合同搜索项
    /// </summary>
    public class ContractSearchViewModel
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
        /// 是否永久
        /// </summary>
        public bool? isForever { get; set; }

        /// <summary>
        /// 日期起
        /// </summary>
        public DateTime? date1 { get; set; }
        /// <summary>
        /// 日期止
        /// </summary>
        public DateTime? date2 { get; set; }
    }
}
