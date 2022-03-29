using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GLXT.Spark.ViewModel.ZSGL
{
    /// <summary>
    /// 资讯搜索项
    /// </summary>
    public class BussinessInformationSearchViewModel
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
        public string keyName { get; set; }
        /// <summary>
        /// 类型
        /// </summary>
        public int? type { get; set; }

    }
}
