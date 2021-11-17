using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace GLXT.Spark.Entity.XTGL
{

    /// <summary>
    /// 权限表
    /// </summary>
    [Table("xtglPermit")]
    public class Permit
    {
        public int Id { get; set; }
        /// <summary>
        /// 所属菜单
        /// </summary>
        public int PageId { get; set; }
        /// <summary>
        /// 名称
        /// </summary>
        [StringLength(90)]
        public string Name { get; set; }
        /// <summary>
        /// 标识码
        /// </summary>
        [StringLength(90)]
        public string Code { get; set; }
        /// <summary>
        /// controller
        /// </summary>
        [StringLength(45)]
        public string Controller { get; set; }
        /// <summary>
        /// action
        /// </summary>
        [StringLength(45)]
        public string Action { get; set; }
        /// <summary>
        /// 排序
        /// </summary>
        public int Sort { get; set; }
        /// <summary>
        /// 是否是查看
        /// </summary>
        public bool IsView { get; set; } = false;
        
        //导航属性
        [ForeignKey("PageId")]
        public Page Page { get; set; }
    }
}
