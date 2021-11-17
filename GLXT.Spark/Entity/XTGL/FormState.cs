using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace GLXT.Spark.Entity.XTGL
{
    /// <summary>
    /// 表单状态表
    /// </summary>
    [Table("xtglFormState")]
    public class FormState
    {
        [Key]
        public int Id { get; set; }
        /// <summary>
        /// 表单Id
        /// </summary>
        [Column("FormId")]
        public int FormId { get; set; }
        /// <summary>
        /// 名称
        /// </summary>
        [StringLength(50)]
        public string Name { get; set; }
        /// <summary>
        /// 值
        /// </summary>
        public int Value { get; set; }
        /// <summary>
        /// 该节点是否需要人员审核
        /// </summary>
        public bool IsCheckup { get; set; }
        /// <summary>
        /// 是否显示在流程中（像作废不需要在流程中显示）
        /// </summary>
        public bool IsFlowNode { get; set; }
        /// <summary>
        /// 是否可用
        /// </summary>
        public bool InUse { get; set; }

        //导航属性
        /// <summary>
        /// 表单
        /// </summary>
        [ForeignKey("FormId")]
        public Form Form { get; set; }
        public List<FormStateOption> FormStateOption { get; set; }
    }
}
