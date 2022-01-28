using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace GLXT.Spark.Entity.QYGL
{
    /// <summary>
    /// 访客信息
    /// </summary>
    [Table("qyglVisitor")]
    public class Visitor : BaseCreateUser
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
        /// 姓名
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 手机号
        /// </summary>
        public string Telphone { get; set; }

        /// <summary>
        /// 预约时间
        /// </summary>
        public DateTime OrderTime { get; set; }

        /// <summary>
        /// 预约成功时间
        /// </summary>
        public DateTime? OrderSuccessTime { get; set; }

        /// <summary>
        /// 到访时间
        /// </summary>
        public DateTime? VisitTime { get; set; }

        /// <summary>
        /// 离开时间
        /// </summary>
        public DateTime? LeaveTime { get; set; }

        /// <summary>
        /// 拜访部门
        /// </summary>
        public int? OrgId { get; set; }

        /// <summary>
        /// 拜访区域
        /// </summary>
        public int? RegionId { get; set; }

        /// <summary>
        /// 接待人
        /// </summary>
        public string ReceivePerson { get; set; }

        /// <summary>
        /// 接待人联系方式
        /// </summary>
        public string ReceiveTel { get; set; }

        /// <summary>
        /// 车牌
        /// </summary>
        public string CarNum { get; set; }

        /// <summary>
        /// 拜访原因
        /// </summary>
        public string VisitReason { get; set; }

        /// <summary>
        /// 同行人数
        /// </summary>
        public int PeerNum { get; set; }
    }
}
