using GLXT.Spark.Entity.XTGL;
using GLXT.Spark.ViewModel.XTGL.UpFile;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace GLXT.Spark.Entity.ZSGL
{
    /// <summary>
    /// 合同签订
    /// </summary>
    [Table("zsglContract")]
    public class Contract : BaseCreateUser
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
        /// 编号
        /// </summary>
        public string Number { get; set; }

        /// <summary>
        /// 签订公司Id
        /// </summary>
        public int EnterpriseId { get; set; }

        /// <summary>
        /// 签订区域Id
        /// </summary>
        public int RegionId { get; set; }
         
        /// <summary>
        /// 合同开始时间
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// 合同结束时间
        /// </summary>
        public DateTime EndDate { get; set; }

        /// <summary>
        /// bool：是否永久合同
        /// </summary>
        public bool IsForever { get; set; }

        /// <summary>
        /// 导航属性
        /// </summary>
        [ForeignKey("EnterpriseId")]
        [DisplayName("意向企业")]
        public Enterprise Enterprise { get; set; }

        //和数据库没关系的字段
        /// <summary>
        /// 附件列表
        /// </summary>
        //合同附件
        [NotMapped]
        public List<UpFile> UpFile { get; set; }
        [NotMapped]
        public List<FileList> FileList { get; set; }
    }
}
