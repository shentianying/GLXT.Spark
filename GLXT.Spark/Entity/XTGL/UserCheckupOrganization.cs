using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using GLXT.Spark.Entity.RSGL;

namespace GLXT.Spark.Entity.XTGL
{
    /// <summary>
    /// 用户审核范围权限
    /// </summary>
    [Table("xtglUserCheckupOrganization")]
    public class UserCheckupOrganization
    {
        public int Id { get; set; }
        public int PersonId { get; set; }
        public int RoleId { get; set; }
        public int OrganizationId { get; set; }

        //导航属性
        [ForeignKey("PersonId")]
        public Person Person { get; set; }
        [ForeignKey("RoleId")]
        public Role Role { get; set; }
        [ForeignKey("OrganizationId")]
        public Organization Organization { get; set; }

    }
}
