using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GLXT.Spark.Entity.XTGL;
using GLXT.Spark.ViewModel.XTGL.UpFile;

namespace GLXT.Spark.ViewModel.XTGL
{
    /// <summary>
    /// 上传附件的审批视图
    /// </summary>
    public class CheckFileViewModel
    {
        /// <summary>
        /// id
        /// </summary>
        public int id { get; set; }

        /// <summary>
        /// 附件
        /// </summary>
        public List<FileList> fileList { get; set; }

        /// <summary>
        /// 审批信息
        /// </summary>
        public Attitude attitude { get; set; }
    }
}
