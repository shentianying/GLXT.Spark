using GLXT.Spark.Entity.XTGL;
using GLXT.Spark.ViewModel.XTGL.UpFile;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace GLXT.Spark.Entity.HDGL
{
    /// <summary>
    /// 商场活动
    /// </summary>
    [Table("hdglMonitor")]
    public class Acitivity : BaseCreateUser
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
        /// 类型
        /// </summary>
        public int Type { get; set; }

        /// <summary>
        /// 位置
        /// </summary>
        public string Location { get; set; }

        /// <summary>
        /// 主题
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// 内容
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// 开始时间
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// 结束时间
        /// </summary>
        public DateTime EndDate { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; }

        //和数据库没关系的字段
        /// <summary>
        /// 附件列表
        /// </summary>
        //附件
        [NotMapped]
        public List<UpFile> UpFile { get; set; }
        [NotMapped]
        public List<FileList> FileList { get; set; }
    }
}
