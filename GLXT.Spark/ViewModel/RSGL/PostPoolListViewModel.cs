using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GLXT.Spark.ViewModel.RSGL
{
    public class PostPoolListViewModel
    {
        /// <summary>
        /// int:主键ID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// int:公司Id
        /// </summary>
        public int CompanyId { get; set; }

        /// <summary>
        /// string：名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// int:适用范围（组织机构ID）
        /// </summary>
        public string OrgName { get; set; }

        /// <summary>
        /// string:类别
        /// </summary>
        public int? Category { get; set; }

        /// <summary>
        /// 是否使用（）
        /// </summary>
        public bool InUse { get; set; }
    }
}
