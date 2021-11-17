using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace GLXT.Spark.Entity.XTGL
{
    /// <summary>
    /// 角色
    /// </summary>
   [Table("xtglRole")]
    public class Role
    {
        public int Id { get; set; }
        public string Name { get; set; }
        /// <summary>
        /// 状态 0：不正常 1：正常
        /// </summary>
        public int Status { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; }
        public int CompanyId { get; set; }
        public List<UserRole> UsersRoles { get; set; }
    }
}
