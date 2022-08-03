using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GLXT.Spark.ViewModel.GGGL
{
    public class AdvertisementSearchViewModel
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
        /// 类型
        /// </summary>
        public int[] types { get; set; }
    }
}
