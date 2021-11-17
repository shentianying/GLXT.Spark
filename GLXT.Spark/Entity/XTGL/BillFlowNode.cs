using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using GLXT.Spark.Entity.RSGL;
using GLXT.Spark.ViewModel.RSGL.Person;

namespace GLXT.Spark.Entity.XTGL
{
    /// <summary>
    /// 单据审批流程节点
    /// </summary>
    [Table("xtglBillFlowNode")]
    public class BillFlowNode
    {
        public int Id { get; set; }
        /// <summary>
        /// 单据流程Id
        /// </summary>
        public int BillFlowId { get; set; }
        /// <summary>
        /// 分组号
        /// </summary>
        public int Group { get; set; }
        /// <summary>
        /// 审批模式
        /// </summary>
        public int Mode { get; set; }
        /// <summary>
        /// 角色类型（角色（岗位）、制单人、审核人、行政领导、条线领导）
        /// </summary>
        public int RoleType { get; set; }
        /// <summary>
        /// 角色Id
        /// </summary>
        public int RoleId { get; set; }
        /// <summary>
        /// 单据状态（制单、审核、支付、事后审核等）
        /// </summary>
        public int State { get; set; } = 0;
        /// <summary>
        /// 表单审批到该结点时是否要执行某种操作
        /// </summary>
        public int Option { get; set; }
        /// <summary>
        /// 最长审批天数，超时将自动跳到下一流程。不限制时0
        /// </summary>
        public int MaxDays { get; set; }

        /// <summary>
        /// 紧急程度 0，普通 1重要 2 非常重要
        /// </summary>
        public int Grade { get; set; } = 0;
        /// <summary>
        /// 本节点对应的组织机构Id（仅在流程生成时做对比使用）
        /// </summary>
        public int OrgId { get; set; }
        /// <summary>
        /// 应审批人Id
        /// </summary>
        public int PersonId { get; set; }
        /// <summary>
        /// 到当前状态日期
        /// </summary>
        public DateTime? ReceiveDate { get; set; }
        /// <summary>
        /// 是否为当前待审批状态
        /// </summary>
        public bool IsCurrentState { get; set; } = false;
        /// <summary>
        /// 是否已审批
        /// </summary>
        public bool IsChecked { get; set; } = false;
        /// <summary>
        /// 是否为超时自动审批
        /// </summary>
        public bool IsAutoChecked { get; set; } = false;
        /// <summary>
        /// 实际审批人Id
        /// </summary>
        public int? CheckupPersonId { get; set; }
        /// <summary>
        /// 审批时间
        /// </summary>
        public DateTime? CheckupDate { get; set; }
        /// <summary>
        /// 备注信息
        /// </summary>
        public string Remark { get; set; }
        /// <summary>
        /// 节点类型（0、流程节点；1、新增节点）
        /// </summary>
        public int NodeType { get; set; } = 0;
        /// <summary>
        /// 制单人ID
        /// </summary>
        public int? CreateUserId { get; set; }
        /// <summary>
        /// 制单人姓名
        /// </summary>
        [StringLength(40)]
        public string CreateUserName { get; set; }
        /// <summary>
        /// 制单日期
        /// </summary>
        public DateTime? CreateDate { get; set; } = DateTime.Now;

        //导航属性
        /// <summary>
        /// 表单
        /// </summary>
        [ForeignKey("PersonId")]
        public Person Person { get; set; }

        /// <summary>
        /// 角色
        /// </summary>
        [ForeignKey("RoleId")]
        public Role Role { get; set; }

        [ForeignKey("OrgId")]
        public Organization Organization { get; set; }

        //与数据库没关系的字段
        /// <summary>
        /// 流程中的固定节点（流程生成时使用）
        /// </summary>
        [NotMapped]
        public bool IsFixedNode { get; set; } = false;
        /// <summary>
        /// 该节点下可选人员（流程生成时使用）
        /// </summary>
        [NotMapped]
        public List<Person> PersonList { get; set; }

        [ForeignKey("BillFlowId")]
        public BillFlow BillFlow { get; set; }
    }
}
