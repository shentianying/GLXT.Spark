using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using GLXT.Spark.Entity.RSGL;

namespace GLXT.Spark.Entity.XTGL
{
    /// <summary>
    /// 用户角色
    /// </summary>
    [Table("xtglUserRole")]
    public class UserRole
    {
        public int Id { get; set; }
        /// <summary>
        /// 用户Id
        /// </summary>
        public int UserId { get; set; }
        /// <summary>
        /// 角色Id
        /// </summary>
        public int RoleId { get; set; }
        /// <summary>
        /// 导航属性
        /// </summary>
        [ForeignKey("UserId")]
        public Person Person { get; set; }
        [ForeignKey("RoleId")]
        public Role Role { get; set; }

    }
}
