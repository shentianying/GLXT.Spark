using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace GLXT.Spark.Entity.RSGL
{
    [Table("rsglPost")]
    public class Post:BaseCreateUser
    {
        ///<summary>
        ///int:主键ID
        ///</summary>
        public int Id { get; set; }

        /// <summary>
        /// string：岗位名称
        /// </summary>
        [StringLength(45)]
        public string Name { get; set; }

        /// <summary>
        /// 公司Id
        /// </summary>
        public int CompanyId { get; set; }

        /// <summary>
        /// int：岗位序列ID（1、业务管理序列；2、综合管理序列；3、项目经理序列；4、专业技术序列）
        /// </summary>
        public int PostSequenceID { get; set; }

        /// <summary>
        /// int：所属条线ID（1、；2、；3、；4、；5、...）
        /// </summary>
        public int BussinessLineID { get; set; }

        /// <summary>
        /// 职级范围（1-15）
        /// </summary>
        public int RankRangeMin { get; set; }
        public int RankRangeMax { get; set; }
        public bool InUse { get; set; }
    }
}
