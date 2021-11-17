using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GLXT.Spark.Entity.XTGL;
using GLXT.Spark.Model.Flow;

namespace GLXT.Spark.IService
{
    /// <summary>
    /// 流程
    /// </summary>
    public interface IBillFlowService
    {
        #region 流程管理

        #region 流程生成
        /// <summary>
        /// 流程撤销
        /// </summary>
        /// <param name="bf">流程对象</param>
        /// <returns></returns>
        public (bool isOk, string msg,int state) BillFlowRevoke(BillFlow bf);
        /// <summary>
        /// 判断是否能撤销
        /// </summary>
        /// <param name="bf"></param>
        /// <returns></returns>
        public (bool isOk, string msg) CheckBillFlowRevoke(BillFlow bf);
        /// <summary>
        /// 审批退回到某一节点，获取前面的所有节点列表
        /// </summary>
        /// <param name="billId"></param>
        /// <param name="bf"></param>
        /// <param name="toBeforeState"></param>
        /// <returns></returns>
        public List<object> GetBillFlowNodeForBack(int billId, bool toBeforeState, BillFlow bf = null);
        /// <summary>
        /// 获取流程数据
        /// </summary>
        /// <param name="billFlowId">单据中的字段billFlowId</param>
        /// <returns>BillFlow</returns>
        public BillFlow GetBillFlowById(int billFlowId);
        /// <summary>
        /// 根据单据billId查找 流程数据
        /// </summary>
        /// <param name="billId">单据表中 billId</param>
        /// <param name="formId">单据表中 formId</param>
        /// <returns></returns>
        public BillFlow GetBillFlowByBillId(int billId,int formId);
        /// <summary>
        /// 验证流程待审批节点中当前用户是否审批
        /// </summary>
        /// <param name="billFlowId">流程id</param>
        /// <param name="billFlow">流程数据</param>
        /// <returns></returns>
        public (bool isOk, string msg, int code) CheckBillFlowNodeIsChecked(int billFlowId, BillFlow billFlow = null);
        /// <summary>
        /// 验证流程
        /// </summary>
        /// <param name="billFlow">流程数据</param>
        /// <returns></returns>
        public (bool isOk, string msg) CheckThisBillFlow(BillFlow billFlow = null);
        /// <summary>
        /// 流程下一步动作
        /// </summary>
        /// <param name="attitudeType">审批类型（下一步，退回，作废之类的）</param>
        /// <param name="billId">单据id</param>
        /// <param name="formId">表单Id</param>
        /// <param name="backGroup">回退到制单，还是某个节点</param>
        /// <returns></returns>
        public (bool isOk, string msg, int state) BillFlowNextAction(int attitudeType, int billId, int formId, int backGroup = 0);
        public (bool isOk, string msg) BillFlowAutoFinishAction(BillFlow bf);
        /// <summary>
        /// 根据表单内容生成审批流程，返回生成结果
        /// </summary>
        /// <typeparam name="T">表单类型</typeparam>
        /// <param name="billObj">表单数据</param>
        /// <param name="refresh">是否重新生成流程</param>
        /// <param name="orgIds">组织机构（为空时取表单中的OrgId）</param>
        /// <returns>item1为是否成功，item2为错误消息</returns>
        public Tuple<bool, string> GetBillFlow<T>(T billObj, bool refresh, params int[] orgIds) where T : class;

        /// <summary>
        /// 根据表单内容生成审批流程（可定义不同状态分属不同组织机构），返回生成结果
        /// </summary>
        /// <typeparam name="T">表单类型</typeparam>
        /// <param name="billObj">表单数据</param>
        /// <param name="refresh">是否重新生成流程</param>
        /// <param name="orgIds">组织机构</param>
        /// <param name="stateOrgs">单据状态及对应组织机构列表</param>
        /// <returns>item1为是否成功，item2为错误消息</returns>
        public Tuple<bool, string> GetBillFlow<T>(T billObj, bool refresh, int[] orgIds, params KeyValuePair<int, int[]>[] stateOrgs) where T : class;

        /// <summary>
        /// 根据表单数据返回对应的流程Id
        /// </summary>
        /// <typeparam name="T">表单类型</typeparam>
        /// <param name="billObj">表单数据</param>
        /// <returns>流程Id</returns>
        public int GetFlowId<T>(T billObj) where T : class;

        #endregion

        #region 流程保存、提交
        /// <summary>
        /// 检验流程中人员是否完整
        /// </summary>
        /// <param name="billFlow"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
        public bool CheckBillFlow(BillFlow billFlow, out string msg);

        /// <summary>
        /// 流程保存
        /// </summary>
        /// <param name="billFlow">流程对象</param>
        /// <returns>流程对象</returns>
        public BillFlow SaveBillFlow(BillFlow billFlow);

        /// <summary>
        /// 提交流程
        /// </summary>
        /// <param name="billFlow">流程对象</param>
        /// <returns>BillFlow</returns>
        public BillFlow SubmitBillFlow(BillFlow billFlow);
        #endregion

        #region 流程中字段类型及可用操作

        /// <summary>
        /// 查询对象的某一属性值
        /// </summary>
        /// <param name="billObj">对象</param>
        /// <param name="field">属性（可查询级联属性，如Organization.Name）</param>
        /// <returns>属性值</returns>
        public object GetFieldVelue<T>(T billObj, string field) where T : class;

        /// <summary>
        /// 返回字段类型及可用操作符
        /// </summary>
        /// <returns></returns>
        public List<FieldType> GetFieldTypeList();

        #endregion

        #region attitude 审批记录
        /// <summary>
        /// 根据流程Id获取审批记录
        /// </summary>
        /// <param name="formId"></param>
        /// <param name="billId"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public List<Attitude> GetAttitudeByBillId(int billId, int formId, int type = 1);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="formId">表单Id</param>
        /// <param name="billId">单据Id</param>
        /// <param name="billFlow">流程</param>
        /// <returns></returns>
        public List<BillFlowNode> GetBillFlowNodeByBillId(int billId,int formId, BillFlow billFlow = null);
        #endregion

        #region attitude 流程审批
        /// <summary>
        /// 增加审批意见
        /// </summary>
        /// <param name="attitude"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
        //public bool AddAttitude(Attitude attitude, out string msg);
        public (bool isOk, string msg) AddAttitude(Attitude attitude);
        #endregion

        #region 流程表单
        /// <summary>
        /// 根据表单名获取表单状态
        /// </summary>
        /// <param name="TableName">表单名</param>
        /// <param name="DefaultValue">默认值</param>
        /// <returns></returns>
        public List<FormStateResult> GetFormState(string TableName, int DefaultValue);
        #endregion
        #endregion
    }
}
