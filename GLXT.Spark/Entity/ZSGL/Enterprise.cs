using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace GLXT.Spark.Entity.ZSGL
{
    /// <summary>
    /// 意向企业
    /// </summary>
    [Table("zsglEnterprise")]
    public class Enterprise : BaseCreateUser
    {
        ///<summary>
        ///int:主键ID
        ///</summary>
        public int Id { get; set; }

        /// <summary>
        /// 公司Id
        /// </summary>
        public int CompanyId { get; set; }

        /// <summary>
        /// 法定代表人
        /// </summary>
        public string LegalPerson { get; set; }

        /// <summary>
        /// 公司名称
        /// </summary>
        public string CompanyName { get; set; }

        /// <summary>
        /// 从业人数
        /// </summary>
        public int EmployeeNum { get; set; }

        /// <summary>
        /// 企业产值
        /// </summary>
        public decimal Output { get; set; }

        /// <summary>
        /// 税收
        /// </summary>
        public decimal Tax { get; set; }

        /// <summary>
        /// 联系人
        /// </summary>
        public string LinkMan { get; set; }

        /// <summary>
        /// 联系电话
        /// </summary>
        public string LinkTel { get; set; }

        /// <summary>
        /// 官网
        /// </summary>
        public string OfficialNet { get; set; }

        /// <summary>
        /// 邮箱
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// 建筑面积
        /// </summary>
        public decimal Area { get; set; }

        /// <summary>
        /// 地址
        /// </summary>
        public string Address { get; set; }

        /// <summary>
        /// 曾用名
        /// </summary>
        public string FormerName { get; set; }

        /// <summary>
        /// 经营状态
        /// </summary>
        public int OperationState { get; set; }

        /// <summary>
        /// 注册资本
        /// </summary>
        public decimal RegCapital { get; set; }

        /// <summary>
        /// 实缴资本
        /// </summary>
        public decimal PaidCapital { get; set; }

        /// <summary>
        /// 所属行业
        /// </summary>
        public string Occupation { get; set; }

        /// <summary>
        /// 统一社会信用代码
        /// </summary>
        public string UniSocialCreditCode { get; set; }

        /// <summary>
        /// 纳税人识别号
        /// </summary>
        public string TaxNum { get; set; }

        /// <summary>
        /// 工商注册号
        /// </summary>
        public string BusinessLicense { get; set; }

        /// <summary>
        /// 组织机构代码
        /// </summary>
        public string OrgCode { get; set; }

        /// <summary>
        /// 成立日期
        /// </summary>
        public DateTime? SetDate { get; set; }

        /// <summary>
        /// 营业期限起
        /// </summary>
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// 营业期限止
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// 企业类型
        /// </summary>
        public int EnterpriseType { get; set; }

        /// <summary>
        /// 核准日期
        /// </summary>
        public DateTime? CheckDate { get; set; }

        /// <summary>
        /// string：备注
        /// </summary>
        public string Remark { get; set; }

        /// <summary>
        /// bool：是否使用
        /// </summary>
        public bool InUse { get; set; }
    }
}
