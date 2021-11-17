using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace GLXT.Spark.Entity.XTGL
{
    /// <summary>
    /// 公司信息
    /// </summary>
    [Table("xtglAccountSet")]
    public class AccountSet
    {
        public int Id { get; set; }
        /// <summary>
        /// 名称
        /// </summary>
        [StringLength(90)]
        public string Name { get; set; }
        /// <summary>
        /// 全名
        /// </summary>
        [StringLength(190)]
        public string FullName { get; set; }
        /// <summary>
        /// 缩写
        /// </summary>
        [StringLength(45)]
        public string ShortName { get; set; }
        /// <summary>
        /// 税号
        /// </summary>
        [StringLength(40)]
        public string TaxNumber { get; set; }
        /// <summary>
        /// 地址
        /// </summary>
        [StringLength(190)]
        public string Address { get; set; }
        /// <summary>
        /// 联系电话
        /// </summary>
        [StringLength(40)]
        public string PhoneNumber { get; set; }
        /// <summary>
        /// 开户行
        /// </summary>
        [StringLength(100)]
        public string Bank { get; set; }
        /// <summary>
        /// 账号
        /// </summary>
        [StringLength(40)]
        public string AccountNumber { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        [StringLength(1900)]
        public string Remark { get; set; }
        /// <summary>
        /// 是否使用 1：可用 0：禁用
        /// </summary>
        public bool InUse { get; set; } = true;
        /// <summary>
        /// 创建人ID
        /// </summary>
        public int CreateUserId { get; set; }
        /// <summary>
        /// 创建人姓名
        /// </summary>
        [StringLength(40)]
        public string CreateUserName { get; set; }
        /// <summary>
        /// 创建日期
        /// </summary>
        public DateTime CreateDate { get; set; } = DateTime.Now;
        /// <summary>
        /// 更新人ID
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
    }
}
