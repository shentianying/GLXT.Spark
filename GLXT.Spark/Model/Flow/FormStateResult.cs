using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GLXT.Spark.Model.Flow
{
    /// <summary>
    /// 表单状态返回
    /// </summary>
    public class FormStateResult
    {
        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 值
        /// </summary>
        public int? Value { get; set; }

        /// <summary>
        /// 默认选中
        /// </summary>
        public bool IsCheck { get; set; }
    }
}
