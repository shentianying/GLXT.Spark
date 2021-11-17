using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GLXT.Spark.ViewModel.RSGL
{
    public class PostSearchViewModel
    {
        /// <summary>
        /// 岗位名称
        /// </summary>
        public string name { get; set; }
        /// <summary>
        /// 当前页面
        /// </summary>
        public int currentPage { get; set; }
        /// <summary>
        /// 页数
        /// </summary>
        public int pageSize { get; set; }
        /// <summary>
        /// 岗位序列组
        /// </summary>
        public int[] postSequenceIds { get; set; }
        /// <summary>
        /// 所属条线组
        /// </summary>
        public int[] bussinessLineIds { get; set; }
    }
}
