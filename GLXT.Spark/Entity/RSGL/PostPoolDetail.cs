using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace GLXT.Spark.Entity.RSGL
{

    /// <summary>
    /// 岗位池明细表
    /// </summary>
    [Table("rsglPostPoolDetail")]
    public class PostPoolDetail
    {
        /// <summary>
        /// int:主键ID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// int:岗位池ID
        /// </summary>
        public int PostPoolId { get; set; }

        /// <summary>
        /// int:岗位ID
        /// </summary>
        public int PostId { get; set; }

        /// <summary>
        /// string:任职资格
        /// </summary>
        public string Qualifications { get; set; }

        /// <summary>
        /// string:岗位职责
        /// </summary>
        public string PostDuty { get; set; }

        /// <summary>
        /// int:行政领导ID
        /// </summary>
        public int AdminLeaderId { get; set; }

        /// <summary>
        /// int:条线领导
        /// </summary>
        public int LineLeaderId { get; set; }

        /// <summary>
        /// int:角色Id
        /// </summary>
        public int RoleId { get; set; }

        /// <summary>
        /// bool:是否使用
        /// </summary>
        public bool InUse { get; set; }

        //导航属性
        [ForeignKey("PostId")]
        public Post Post { get; set; }
        [ForeignKey("PostPoolId")]
        public PostPool PostPool { get; set; }
    }
}
