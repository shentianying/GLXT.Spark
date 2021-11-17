using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using GLXT.Spark.Entity.XTGL;

namespace GLXT.Spark.Entity.RSGL
{

    /// <summary>
    /// 岗位池
    /// </summary>
    [Table("rsglPostPool")]
    public class PostPool :BaseCreateUser
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
        public int OrgId { get; set; }

        /// <summary>
        /// string:类别
        /// </summary>
        public int Category { get; set; }

        /// <summary>
        /// 是否使用（）
        /// </summary>
        public bool InUse { get; set; }

        public List<PostPoolDetail> PostPoolDetail { get; set; }

        //导航属性
        [ForeignKey("OrgId")]
        public virtual Organization Organization { get; set; }

        
    }
}
