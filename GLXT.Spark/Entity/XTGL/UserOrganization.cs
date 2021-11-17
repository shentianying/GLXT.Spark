using System.ComponentModel.DataAnnotations.Schema;
using GLXT.Spark.Entity.RSGL;

namespace GLXT.Spark.Entity.XTGL
{
    /// <summary>
    /// 用户公司权限
    /// </summary>
    [Table("xtglUserOrganization")]
    public class UserOrganization
    {
        public int Id {get;set;}
        public int UserId { get; set; }
        public int CompanyId { get; set; }
        public int OrganizationId { get; set; }
        public bool Selected { get; set; }

        [ForeignKey("UserId")]
        public Person person { get; set; }
        [ForeignKey("OrganizationId")]
        public Organization Organization { get; set; }
        [ForeignKey("CompanyId")]
        public Organization Company { get; set; }


    }
}
