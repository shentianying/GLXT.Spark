using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using GLXT.Spark.Entity.XTGL;

namespace GLXT.Spark.Entity.RSGL
{
    /// <summary>
    /// 人员信息
    /// </summary>
    [Table("rsglPerson")]
    public class Person
    {
        /// <summary>
        /// id号
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// 姓名
        /// </summary>
        [StringLength(40)]
        public string Name { get; set; }
        /// <summary>
        /// 头像
        /// </summary>
        public string Avatar { get; set; }

        /// <summary>
        /// 工号
        /// </summary>
        [StringLength(40)]
        public string Number { get; set; }
        /// <summary>
        /// 公司
        /// </summary>
        public int CompanyId { get; set; }
        /// <summary>
        /// 部门
        /// </summary>
        public int OrganizationId { get; set; }
        /// <summary>
        /// 所属岗位
        /// </summary>
        public int PostId { get; set; }
        /// <summary>
        /// 证件类型
        /// </summary>
        [Column("IDType")]
        [StringLength(40)]
        public string IDType { get; set; }
        /// <summary>
        /// 证件号码
        /// </summary>
        [Column("IDNumber")]
        [StringLength(40)]
        public string IDNumber { get; set; }
        /// <summary>
        /// 电话
        /// </summary>
        [StringLength(40)]
        public string PhoneNumber { get; set; }
        /// <summary>
        /// 身份证住址
        /// </summary>
        [Column("IDAddress")]
        [StringLength(100)]
        public string IDAddress { get; set; }
        /// <summary>
        /// 家庭住址
        /// </summary>
        [StringLength(100)]
        public string HomeAddres { get; set; }
        /// <summary>
        /// 性别
        /// </summary>
        [StringLength(10)]
        public string Gender { get; set; }
        /// <summary>
        /// 民族
        /// </summary>
        [StringLength(20)]
        public string Nation { get; set; }
        /// <summary>
        /// 出生日期
        /// </summary>
        public DateTime? BirthDate { get; set; }

        ///<summary>
        ///int:开户行
        ///</summary>
        public int? AWBank { get; set; }

        ///<summary>
        ///string:开户帐户
        ///</summary>
        public string Account { get; set; }

        ///<summary>
        ///bool:是否已确认帐号
        ///</summary>
        public bool IsConfirmAccount { get; set; }

        ///<summary>
        ///int:帐号确认人ID
        ///</summary>
        public int? ConfirmAccountUserID { get; set; }

        ///<summary>
        ///string:帐号确认人姓名
        ///</summary>
        public string ConfirmAccountUserName { get; set; }

        ///<summary>
        ///DateTime:帐号确认日期
        ///</summary>
        public DateTime? ConfirmAccountDate { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        [StringLength(1900)]
        public string Remark { get; set; }
        /// <summary>
        /// 人员类型ID
        /// </summary>
        public int PersonTypeID { get; set; }
        /// <summary>
        /// 是否可用（1、可用；0、禁用）
        /// </summary>
        public bool InUse { get; set; }

        /// <summary>
        /// 是否用户 1.是 0 不是
        /// </summary>
        public bool IsUser { get; set; }

        /// <summary>
        /// 密码
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// 过期日期
        /// </summary>
        public DateTime? ExpirationDate { get; set; }

        /// <summary>
        /// 不能登录原因
        /// </summary>
        public string DisableMsg { get; set; }

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
        [ForeignKey("CreateUserId")]
        public Person CreateUser { get; set; }

        [ForeignKey("LastEditUserId")]
        public Person LastEditUser { get; set; }

        [ForeignKey("CompanyId")]
        public Organization Company { get; set; }
        [ForeignKey("OrganizationId")]
        public Organization Organization { get; set; }
        [ForeignKey("PostId")]
        public Post Post { get; set; }
        public List<UserRole> UserRoles { get; set; }
        public List<UserOrganization> UserOrganizationList { get; set; }
        //public List<UserPost> UserPosts { get; set; }

    }

    public static class PersonExtend
    {
        //扩展方法
        /// <summary>
        /// 人员信息中去除敏信息，只保留基本信息
        /// </summary>
        /// <param name="person">Person</param>
        /// <returns>Person</returns>
        public static Person ToBasicInfo(this Person person)
        {
            return new Person()
            {
                Id = person.Id,
                Name = person.Name,
                PhoneNumber = person.PhoneNumber,
                Avatar = person.Avatar,
                Company = person.Company,
                OrganizationId = person.OrganizationId,
                Post = person.Post
            };
        }
    }
}
