using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace GLXT.Spark.Entity.XTGL
{
    /// <summary>
    /// 登录日志
    /// </summary>
    [Table("xtglLog")]
    public class Log
    {
        /// <summary>
        /// Id
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// 人员ID
        /// </summary>
        public int PersonId { get; set; }
        /// <summary>
        /// IP地址
        /// </summary>
        public string IPAddress { get; set; }
        /// <summary>
        /// 登录时间
        /// </summary>
        public DateTime LoginDate { get; set; } = DateTime.Now;
        /// <summary>
        /// 活动时间
        /// </summary>
        public DateTime ActiveDate { get; set; } = DateTime.Now;
        /// <summary>
        /// 退出时间
        /// </summary>
        public DateTime? LogoutDate { get; set; }
        /// <summary>
        /// 登录类型（1、电脑登录；2、手机登录；3、临时登录）
        /// </summary>
        public int LoginTypeId { get; set; } = 1;
        /// <summary>
        /// 是否在线
        /// </summary>
        public bool OnLine { get; set; } = true;

    }
}
