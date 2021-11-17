using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using GLXT.Spark.ViewModel.XTGL.UpFile;

namespace GLXT.Spark.Entity.XTGL
{
    /// <summary>
    /// 审批意见
    /// </summary>
    [Table("xtglAttitude")]
    public class Attitude : BaseCreateUser
    {
        public int Id { get; set; }
        /// <summary>
        /// 表单Id
        /// </summary>
        public int FormId { get; set; }
        /// <summary>
        /// 单据Id
        /// </summary>
        public int BillId { get; set; }
        /// <summary>
        /// 审批意见类型 1= 审批信息
        /// </summary>
        public int Type { get; set; } = 1;
        /// <summary>
        /// 操作类型
        /// </summary>
        public string Operation { get; set; }
        /// <summary>
        /// 意见抬头
        /// </summary>
        [StringLength(200)]
        public string Title { get; set; }
        /// <summary>
        /// 意见内容
        /// </summary>
        [StringLength(2000)]
        public string Content { get; set; }
        /// <summary>
        /// 到达本节点时间
        /// </summary>
        public DateTime? ReceiveDate { get; set; }

        //导航属性
        /// <summary>
        /// 表单
        /// </summary>
        [ForeignKey("FormId")]
        public Form Form { get; set; }
        
        /// <summary>
        /// 退回到的点
        /// </summary>
        [NotMapped]
        public int BackGroup { get; set; }
        /// <summary>
        /// 审批类型
        /// </summary>
        [NotMapped]
        public int AttitudeType { get; set; }
        [NotMapped]
        public List<FileList> FileList { get; set; } // 用于接收前端数据
        [NotMapped]
        public List<UpFile> UpFiles { get; set; } // 用于返回前端数据

    }
}
