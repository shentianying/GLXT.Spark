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
    /// 人员岗位
    /// </summary>
    [Table("rsglPersonPost")]
    public class PersonPost:BaseCreateUser
    {
        public int Id { get; set; }

        /// <summary>
        /// 用户Id
        /// </summary>
        public int PersonId { get; set; }

        /// <summary>
        /// 岗位池Id
        /// </summary>
        public int PostPoolDetailId { get; set; }

        /// <summary>
        /// 组织机构Id
        /// </summary>
        public int OrgId { get; set; }

        /// <summary>
        /// 岗位Id
        /// </summary>
        public int PostId { get; set; }

        /// <summary>
        /// 角色Id
        /// </summary>
        public int RoleId { get; set; }

        /// <summary>
        /// 职务Id
        /// </summary>
        public int PositionId { get; set; }
        /// <summary>
        /// 是否是主岗位
        /// </summary>
        public bool IsMain { get; set;}

        /// <summary>
        /// 是否使用
        /// </summary>
        public bool InUse { get; set; }

        //导航属性
        [ForeignKey("PersonId")]
        public Person Person { get; set; }
        [ForeignKey("OrgId")]
        public Organization Organization { get; set; }
        [ForeignKey("PostId")]
        public Post Post { get; set; }
        [ForeignKey("PostPoolDetailId")]
        public PostPoolDetail PostPoolDetail { get; set; }
        [ForeignKey("RoleId")]
        public Role Role { get; set; }
    }
}
