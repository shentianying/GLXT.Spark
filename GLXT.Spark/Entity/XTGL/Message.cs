using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace GLXT.Spark.Entity.XTGL
{
    /// <summary>
    /// 信息表
    /// </summary>
    [Table("xtglMessage")]
    public class Message
    {
        public int Id { get; set; }
        /// <summary>
        /// 标题
        /// </summary>
        public string Title { get; set; }
        /// <summary>
        /// 内容
        /// </summary>
        public string Content { get; set; }
        /// <summary>
        /// 消息类型 0 系统消息 1 流程消息
        /// </summary>
        public int Type { get; set; }


        /// <summary>
        /// 发送人Id 0 系统发送
        /// </summary>
        public int? SenderId { get; set; }
        /// <summary>
        /// 发送时间
        /// </summary>
        public DateTime SendTime { get; set; }
        /// <summary>
        /// 发送人姓名
        /// </summary>
        public string SendName { get; set; }
        /// <summary>
        ///  是否使用
        /// </summary>
        public bool InUse { get; set; } = true;
        /// <summary>
        /// 页面Id
        /// </summary>
        public int? PageId { get; set; } = 0;
        public int BillId { get; set; } = 0;
        public string Str { get; set; } = Utils.Common.GenerateRandomNumber(3);
        public int BillFlowId { get; set; } = 0;
        public List<MessageUser> MessageUser { get;set;}
    }
}
