using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace GLXT.Spark.Entity
{
    /// <summary>
    /// 制单人、制单日期、更新人、更新日期等字段
    /// </summary>
    public class BaseCreateUser
    {
        /// <summary>
        /// 制单人ID
        /// </summary>
        public int? CreateUserId { get; set; }
        /// <summary>
        /// 制单人姓名
        /// </summary>
        [StringLength(40)]
        public string CreateUserName { get; set; }
        /// <summary>
        /// 制单日期
        /// </summary>
        public DateTime CreateDate { get; set; } = DateTime.Now;
        /// <summary>
        /// 更新人ID
        /// </summary>
        public int? LastEditUserId { get; set; }
        /// <summary>
        /// 更新人姓名
        /// </summary>
        [StringLength(40)]
        public string LastEditUserName { get; set; }
        /// <summary>
        /// 更新日期
        /// </summary>
        public DateTime LastEditDate { get; set; } = DateTime.Now;
    }
}
