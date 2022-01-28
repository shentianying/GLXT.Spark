using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace GLXT.Spark.Entity.QYGL
{
    /// <summary>
    /// 房产资源
    /// </summary>
    [Table("qyglResources")]
    public class Resources : BaseCreateUser
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
        /// 上级
        /// </summary>
        [JsonProperty(PropertyName = "pid")]
        public int PId { get; set; } = -1;
        /// <summary>
        /// 名称
        /// </summary>
        [StringLength(90), Display(Name = "名称")]
        public string Name { get; set; }
        /// <summary>
        /// 缩写
        /// </summary>
        [StringLength(40)]
        public string ShortName { get; set; }

        /// <summary>
        /// 排序号
        /// </summary>
        public int Sort { get; set; }
        /// <summary>
        /// 是否可选择（1、可选择；2、不可选择）
        /// </summary>
        public bool Optional { get; set; }
        
        /// <summary>
        /// 是否可用（1、可用；0、禁用）
        /// </summary>
        public bool InUse { get; set; }

        /// <summary>
        /// 是否出租
        /// </summary>
        public bool IsRent { get; set; }
    }
}
