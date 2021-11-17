using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GLXT.Spark.Entity.RSGL;
using GLXT.Spark.Entity.XTGL;

namespace GLXT.Spark.ViewModel.RSGL.Person
{
    public class PersonViewModel
    {
        /// <summary>
        /// id号
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// 姓名
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 工号
        /// </summary>
        public string Number { get; set; }
        public int CompanyId { get; set; }
        /// <summary>
        /// 组织机构id
        /// </summary>
        public int OrganizationId { get; set; }
        /// <summary>
        /// 所属岗位
        /// </summary>
        public int? PostId { get; set; }
        /// <summary>
        /// 证件类型
        /// </summary>
        public string IDType { get; set; }
        /// <summary>
        /// 证件号码
        /// </summary>
        public string IDNumber { get; set; }
        /// <summary>
        /// 电话
        /// </summary>
        public string PhoneNumber { get; set; }
        /// <summary>
        /// 身份证住址
        /// </summary>
        public string IDAddress { get; set; }
        /// <summary>
        /// 家庭住址
        /// </summary>
        public string HomeAddres { get; set; }
        /// <summary>
        /// 性别
        /// </summary>
        public string Gender { get; set; }
        /// <summary>
        /// 民族
        /// </summary>
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

        ///// <summary>
        ///// 密码
        ///// </summary>
        //public string Password { get; set; }

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
        public string LastEditUserName { get; set; }
        /// <summary>
        /// 更新日期
        /// </summary>
        public DateTime LastEditDate { get; set; } = DateTime.Now;

        public List<UserOrganization> userOrgList { get; set; }

        public Organization Company { get; set; }
        public Organization Organization { get; set; }
        public Post Post { get; set; }
    }
}
