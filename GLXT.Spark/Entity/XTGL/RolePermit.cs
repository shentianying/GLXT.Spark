using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace GLXT.Spark.Entity.XTGL
{
    /// <summary>
    /// 角色权限表
    /// </summary>
    [Table("xtglRolePermit")]
    public class RolePermit
    {
        public int Id { get; set; }
        public int RoleId { get; set; }
        public int PermitId { get; set; }

        public Role Role { get; set; }
        public Permit Permit { get; set; }
    }
}
