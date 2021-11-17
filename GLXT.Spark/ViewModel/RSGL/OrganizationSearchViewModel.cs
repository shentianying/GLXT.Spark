using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GLXT.Spark.ViewModel.RSGL
{
    public class OrganizationSearchViewModel
    {
        /// <summary>
        /// 部门组
        /// </summary>
        public int[] orgIds { get; set; }

        /// <summary>
        /// 岗位Id
        /// </summary>
        public int postId { get; set; }

        /// <summary>
        /// 名称
        /// </summary>
        public string name { get; set; }

        /// <summary>
        /// 类别组
        /// </summary>
        public int[] categoryIds { get; set; }

        /// <summary>
        /// 是否项目
        /// </summary>
        public bool? isProject { get; set; }
    }
}
