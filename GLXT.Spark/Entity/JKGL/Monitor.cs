using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace GLXT.Spark.Entity.JKGL
{
    /// <summary>
    /// 企业信息
    /// </summary>
    [Table("jkglMonitor")]
    public class Monitor : BaseCreateUser
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
        /// int：上级
        /// </summary>
        public int PId { get; set; }

        /// <summary>
        /// string：监控名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// string：监控IP地址
        /// </summary>
        public string IPAddress { get; set; }

        /// <summary>
        /// string：登录账号
        /// </summary>
        public string LoginName { get; set; }

        /// <summary>
        /// string：登录密码
        /// </summary>
        public string LoginPassword { get; set; }

        /// <summary>
        /// string：备注
        /// </summary>
        public string Remark { get; set; }

        /// <summary>
        /// string：是否是监控
        /// </summary>
        public bool IsMonitor { get; set; }
    }
}
