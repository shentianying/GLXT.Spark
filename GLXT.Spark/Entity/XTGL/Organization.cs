using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace GLXT.Spark.Entity.XTGL
{
    /// <summary>
    /// 组织机构
    /// </summary>
    [Table("xtglOrganization")]
    public class Organization
    {
        public int Id { get; set; }
        [JsonProperty(PropertyName = "pid")]
        public int PId { get; set; } = -1;
        /// <summary>
        /// 名称
        /// </summary>
        [StringLength(90), Display(Name = "名称")]
        public string Name { get; set; }
        /// <summary>
        /// 缩写
        /// </summary>
        [StringLength(40)]
        public string ShortName { get; set; }
        /// <summary>
        /// 所属公司 多个以 “,”逗号隔开
        /// </summary>
        [StringLength(190)]
        public string AccountSetIds { get; set; }
        /// <summary>
        /// 排序号
        /// </summary>
        public int Sort { get; set; }
        /// <summary>
        /// 是否可选择（1、可选择；2、不可选择）
        /// </summary>
        public bool Optional { get; set; }
        /// <summary>
        /// 是否是项目（1、是；0、否）
        /// </summary>
        public bool IsProject { get; set; }
        /// <summary>
        /// 项目Id
        /// </summary>
        public int? ProjectId { get; set; }

        /// <summary>
        /// int:岗位池类别Id
        /// </summary>
        public int CategoryId { get; set; }
        /// <summary>
        /// 是否可用（1、可用；0、禁用）
        /// </summary>
        public bool InUse { get; set; }

        /// <summary>
        /// 是否统计余额
        /// </summary>
        public bool IsSum { get; set; }

        /// <summary>
        /// 制单人Id
        /// </summary>
        public int CreateUserId { get; set; }
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
        /// 更新人Id
        /// </summary>
        public int LastEditUserId { get; set; }
        /// <summary>
        /// 更新人姓名
        /// </summary>
        [StringLength(40)]
        public string LastEditUserName { get; set; }
        /// <summary>
        /// 更新日期
        /// </summary>
        public DateTime LastEditDate { get; set; } = DateTime.Now;

        //导航属性
        //[ForeignKey("CreateUserId")]
        //[InverseProperty("Organization")]
        //public Person CreateUser { get; set; }
        //[ForeignKey("LastEditUserId")]
        //public Person LastEditUser { get; set; }
    }
}
