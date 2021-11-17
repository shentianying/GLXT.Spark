using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace GLXT.Spark.Entity.XTGL
{
    /// <summary>
    /// 信息用户表
    /// </summary>
   [Table("xtglMessageUser")]
    public class MessageUser
    {
        public int Id { get; set; }
        /// <summary>
        /// 信息表（message）外键
        /// </summary>
        public int MessageId { get; set; }
        /// <summary>
        /// 是否已读
        /// </summary>
        public bool IsRead { get; set; }
        /// <summary>
        /// 读取时间
        /// </summary>
        public DateTime? ReadTime { get; set; }
        /// <summary>
        /// 接收人Id
        /// </summary>
        public int ReceiverId { get; set; }

        // 导航属性
        [ForeignKey("MessageId")]
        public Message Message { get; set; }
    }
}
