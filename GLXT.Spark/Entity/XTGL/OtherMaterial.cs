using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace GLXT.Spark.Entity.XTGL
{
    /// <summary>
    /// 其他材料
    /// </summary>
    [Table("xtglOtherMaterial")]
    public class OtherMaterial : BaseCreateUser
    {
        ///<summary>
        ///int:主键ID
        ///</summary>
        public int Id { get; set; }

        ///<summary>
        ///int:可选择公司Id
        ///</summary>
        public int CompanyId { get; set; }

        ///<summary>
        ///string:编号
        ///</summary>
        public string Number { get; set; }

        ///<summary>
        ///string:名称
        ///</summary>
        public string Name { get; set; }

        ///<summary>
        ///int:类别
        ///</summary>
        public int MaterialType { get; set; }

        ///<summary>
        ///string:规格型号
        ///</summary>
        public string Spec { get; set; }

        ///<summary>
        ///string:计量单位
        ///</summary>
        public string Unit { get; set; }

        ///<summary>
        ///bool:是否禁用
        ///</summary>
        public bool IsForbidden { get; set; }
    }
}
