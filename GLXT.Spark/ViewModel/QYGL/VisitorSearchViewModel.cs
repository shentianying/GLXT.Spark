using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GLXT.Spark.ViewModel.QYGL
{

    /// <summary>
    /// 访客搜索项
    /// </summary>
    public class VisitorSearchViewModel
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
        /// 名称
        /// </summary>
        public string tel { get; set; }

        /// <summary>
        /// 车牌
        /// </summary>
        public string carNum { get; set; }

        /// <summary>
        /// 接待人
        /// </summary>
        public string receivor { get; set; }

        /// <summary>
        /// 进入时间起
        /// </summary>
        public DateTime? date1 { get; set; }

        /// <summary>
        /// 接待人
        /// </summary>
        public DateTime? date2 { get; set; }
    }
}
