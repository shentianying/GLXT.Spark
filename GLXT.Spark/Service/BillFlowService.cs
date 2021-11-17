using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using GLXT.Spark.Entity;
using GLXT.Spark.Entity.RSGL;
using GLXT.Spark.Entity.XTGL;
using GLXT.Spark.Enums;
using GLXT.Spark.IService;
using GLXT.Spark.Model;
using GLXT.Spark.Model.Flow;
using GLXT.Spark.Utils;

namespace GLXT.Spark.Service
{
    /// <summary>
    /// 流程
    /// </summary>
    public class BillFlowService : IBillFlowService
    {
        private readonly DBContext _dbContext;
        private readonly IMapper _mapper;
        private readonly ICommonService _commonService;
        private readonly IPrincipalAccessor _principalAccessor;
        private readonly ISystemService _systemService;
        public BillFlowService(
            DBContext dBContext,
            IMapper mapper,
            ICommonService commonService,
            IPrincipalAccessor principalAccessor,
            ISystemService systemService)
        {
            _dbContext = dBContext;
            _mapper = mapper;
            _principalAccessor = principalAccessor;
            _commonService = commonService;
            _systemService = systemService;
        }

        #region 流程管理

        #region 流程生成

        /// <summary>
        /// 根据表单内容生成审批流程，返回生成结果
        /// </summary>
        /// <typeparam name="T">表单类型</typeparam>
        /// <param name="billObj">表单数据</param>
        /// <param name="refresh">是否重新生成流程</param>
        /// <param name="orgIds">组织机构（为空时取表单中的OrgId）</param>
        /// <returns>item1为是否成功，item2为错误消息</returns>
        public Tuple<bool, string> GetBillFlow<T>(T billObj, bool refresh, params int[] orgIds) where T : class
        {
            return GetBillFlow(billObj, refresh, orgIds, Array.Empty<KeyValuePair<int, int[]>>());
        }
        /// <summary>
        /// 审批退回到某一节点，获取前面的所有节点列表
        /// </summary>
        /// <returns></returns>
        public List<object> GetBillFlowNodeForBack(int billFlowId, bool toBeforeState,BillFlow bf = null)
        {
            // **步骤**
            // 1.获取我当前待审批节点 group
            // 2.获取billFlow中小于上面group的所有节点
            // 3.分组拼接前端需要的格式
            if (bf == null)
            {
                bf = _dbContext.BillFlow
                        .Include(i => i.BillFlowNode).ThenInclude(t => t.Person)
                        .FirstOrDefault(w => w.Id.Equals(billFlowId));
            }
            var checkResult = CheckBillFlowNodeIsChecked(bf.Id);
            if (checkResult.code.Equals((int)AttitudeEnum.未审批))
            {
                // 当前待审批节点
                var currentNode = bf.BillFlowNode.Where(w => w.IsCurrentState).ToList();
                if (currentNode.Any())
                {
                    // 待审批节点中是否含有我的审批项
                    var myBillFlowNode = currentNode.Where(a => a.PersonId.Equals(_principalAccessor.Claim().Id));
                    if (myBillFlowNode.Any())
                    {
                        // 我的待审批节点的group的值
                        var myGroup = myBillFlowNode.First().Group;
                        var myState = myBillFlowNode.First().State;

                        List<BillFlowNode> q1 = null;
                        if (toBeforeState)
                            // 我前面的所有group
                            q1 = bf.BillFlowNode.Where(w => w.Group < myGroup).ToList();
                        else
                            // 我当前所在状态的所有
                            q1 = bf.BillFlowNode.Where(w => w.Group < myGroup && w.State == myState).ToList();
                        // 如果待审批节点 上一步有审批节点
                        if (q1.Any())
                        {
                            // group分组
                            var gs = q1.Select(s => s.Group).Distinct();

                            var list = new List<object>();
                            foreach (var g in gs)
                            {
                                // 根据group 查找 billFlowNodes 节点
                                // 节点中的多项审批 拼接成 一个
                                var billFlowNodes = bf.BillFlowNode.Where(w => w.Group.Equals(g));
                                var str = string.Join('、', billFlowNodes.Select(s => s.Person.Name));
                                list.Add(new { name = str, value = g });
                            }
                            return list;
                        }
                    }
                }
            }
            return null;
        }
        /// <summary>
        /// ************处理审批流程动作(总入口)************
        /// </summary>
        public (bool isOk, string msg, int state) BillFlowNextAction(int attitudeType, int billId, int formId, int backGroup = 0)
        {
            //if (attitude == null)
            //    return (false, "审批内容不能为空");

            var bf = _dbContext.BillFlow
                .Include(i => i.BillFlowNode)
                .FirstOrDefault(w => w.BillId.Equals(billId) && w.FormId.Equals(formId));
            if (bf == null)
                return (false, "找不到该条流程", -1000);

            if (attitudeType != (int)AttitudeTypeEnum.撤销)
            {
                var checkResult1 = CheckThisBillFlow(bf);
                if (!checkResult1.isOk)
                    return (false, checkResult1.msg, -1000);
                var checkResult2 = CheckBillFlowNodeIsChecked(bf.Id, bf);
                if (checkResult2.code != (int)AttitudeEnum.未审批) // 如果不是 未审批，就不能继续下面的审批了哦
                    return (false, checkResult2.msg, -1000);
            }

            // 审批动作
            if (attitudeType.Equals((int)AttitudeTypeEnum.转到下一步))
                // 同意转到下一步
                return BillFlowAgreeAction(bf);

            if (attitudeType.Equals((int)AttitudeTypeEnum.作废))

                // 作废操作
                return BillFlowInvalidAction(bf);

            if (attitudeType.Equals((int)AttitudeTypeEnum.退回))
            {
                // 退回操作
                // if BackGroup==0，退回到制单 else：退回到节点
                if (backGroup.Equals(0))
                    return BillFlowBackToCreate(bf);
                else
                    return BillFlowBackAction(bf, backGroup);
            }
            if (attitudeType.Equals((int)AttitudeTypeEnum.撤销))
            {
                // 撤销
                return BillFlowRevoke(bf);
            }

            return (false, "没有该类型", -1000);
        }

        /// <summary>
        /// 验证流程待审批节点中当前用户是否审批
        /// </summary>
        /// <param name="billFlowId">流程Id</param>
        /// <param name="billFlow">流程对象</param>
        /// <returns>true:是未审批，false:是错误</returns>
        public (bool isOk, string msg, int code) CheckBillFlowNodeIsChecked(int billFlowId, BillFlow billFlow = null)
        {
            if (billFlow == null)
            {
                billFlow = _dbContext.BillFlow
                        .Include(i => i.BillFlowNode)
                        .FirstOrDefault(w => w.Id.Equals(billFlowId));
            }

            var checkResult1 = CheckThisBillFlow(billFlow);
            if (!checkResult1.isOk)
                return (checkResult1.isOk, checkResult1.msg, billFlow.State);
            // 获取正在审批节点
            var currentNode = billFlow.BillFlowNode.Where(w => w.IsCurrentState).ToList();
            if (currentNode.Any())
            {
                // 节点中审批用户是我的项 和我 授权 审批的 人
                var myId = _principalAccessor.Claim().Id;
                var myBillFlowNode = currentNode.Where(a => a.PersonId.Equals(myId));
                if (myBillFlowNode.Any())
                {
                    // 已审批 和 未审批
                    // 全部isChecked = 已经审批
                    // 含有一个未审批 = 未审批
                    if (myBillFlowNode.Where(w => w.IsChecked).Count() == myBillFlowNode.Count())
                        return (false, "已经审批过了，不要重复审批", (int)AttitudeEnum.已审批);
                    else
                        return (true, "未审批", (int)AttitudeEnum.未审批);
                    //return (false, "同一个审批节点不能含有多个相同的审批人和授权审批的人", (int)AttitudeEnum.同一节点含有相同审批人);
                }
                else
                    return (false, "待审批节点中没有我的的审批用户", (int)AttitudeEnum.待审批中没有我的审批);
            }
            else
                return (false, "没有待审批，已完成", (int)AttitudeEnum.没有待审批节点已完成);
        }

        /// <summary>
        /// 检测流程数据的合法性
        /// </summary>
        /// <param name="billFlow"></param>
        /// <returns></returns>
        public (bool isOk, string msg) CheckThisBillFlow(BillFlow billFlow = null)
        {
            if (billFlow == null)
            {
                return (false, "数据不能为空");
            }
            if (billFlow == null)
                return (false, "流程数据为空");
            if (billFlow.State == (int)AttitudeTypeEnum.制单)
                return (false, "该流程还在制单状态，无法操作");
            if (billFlow.State == (int)AttitudeTypeEnum.作废)
                return (false, "该流程已经作废");
            if (billFlow.State == (int)AttitudeTypeEnum.完成)
                return (false, "该流程已经结束");
            return (true, "");
        }
        // 获取当前待审批人节点中我的审批项
        public List<BillFlowNode> GetBillFlowNodeByBillId(int billId, int formId, BillFlow billFlow = null)
        {
            if (billFlow == null)
            {
                billFlow = _dbContext.BillFlow
                        .Include(i => i.BillFlowNode)
                        .FirstOrDefault(w => w.CompanyId == _systemService.GetCurrentSelectedCompanyId()
                        && w.BillId.Equals(billId)
                        && w.FormId.Equals(formId)
                        );
            }
            // 当前审批节点
            var currentNode = billFlow.BillFlowNode.Where(w => w.IsCurrentState).ToList();
            var myBillFlowNode = currentNode.Where(a => a.PersonId.Equals(_principalAccessor.Claim().Id)).ToList();
            return myBillFlowNode;
        }
        /// <summary>
        /// 流程--1 同意下一步
        /// </summary>
        /// <param name="bf">流程</param>
        /// <returns></returns>
        public (bool isOk, string msg, int state) BillFlowAgreeAction(BillFlow bf)
        {
            // 当前审批节点
            var currentNode = bf.BillFlowNode.Where(w => w.IsCurrentState).ToList();
            // 下一个节点
            List<BillFlowNode> nextNode = null;
            //所有审批节点分组去重，取group
            var groups = bf.BillFlowNode.Select(s => s.Group).Distinct();
            // 如果找到下一个节点 nextNode
            var queryGroups = groups.Where(g => g > currentNode.First().Group).OrderBy(ob => ob);
            if (queryGroups.Any())
            {
                nextNode = bf.BillFlowNode.Where(w => w.Group == queryGroups.First()).ToList();
            }

            // 检测当前审批节点中是否包含当前审批用户
            var myBillFlowNode = currentNode.Where(a => a.PersonId.Equals(_principalAccessor.Claim().Id));
            // 取当前节点中的，我的审批用户。
            //var _billFlowNode = billFlowNode.FirstOrDefault();
            foreach (var _billFlowNode in myBillFlowNode)
            {
                //1.修改billFlowNode 表相关字段
                _billFlowNode.IsChecked = true; // 设置 是否已审批
                //_billFlowNode.IsCurrentState = false; // 取消当前审批节点
                _billFlowNode.CheckupPersonId = _principalAccessor.Claim().Id;
                _billFlowNode.CheckupDate = DateTime.Now; // 设置审批时间
                _billFlowNode.Remark = "审批备注";
                _dbContext.Update(_billFlowNode);
            }

            List<Remind> list = new List<Remind>();
            bool nextGroupIsMe = false;
            // mode 模式 1.全部通过 2.一人通过

            // 如果当前审批节点，就我一个审批用户 或者 节点模式 一个人通过 或者 全部通过并且还有
            var isLastFinish = myBillFlowNode.First().Mode.Equals(1) && currentNode.Where(w => !w.IsChecked).Count() == 0;
            if (currentNode.Count() == 1 || myBillFlowNode.First().Mode.Equals(2) || isLastFinish)
            {
                // 修改当前节点为 不是待审批节点
                currentNode.ForEach(item =>
                {
                    item.IsCurrentState = false;
                    _dbContext.Update(item);
                });
                var formObject = _dbContext.Form.FirstOrDefault(f => f.Id.Equals(bf.FormId));

                bf.LastEditDate = DateTime.Now;
                bf.LastEditUserId = _principalAccessor.Claim().Id;
                bf.LastEditUserName = _principalAccessor.Claim().Name;
                // 【修改下一个节点为待审批节点】
                // 如果 下一个节点为null。说明当前节点是最后一个节点。直接完成节点。
                if (nextNode == null)
                {
                    // 直接完成审批
                    // 步骤 1.修改主流程 billFlow表中 state 状态为 10000 完成
                    // 步骤 2.修改单据 state 状态为 10000 完成

                    // 1.修改主流程状态
                    bf.State = (int)BillStateEnum.Finish;
                    _dbContext.Update(bf);
                }
                else
                {
                    // 如果下一个节点审批的人，还是我
                    // 并且没有特殊操作 option==0
                    // 并且是同一个state状态
                    // 才能一起操作
                    if (nextNode.Count == 1 
                        && nextNode.First().PersonId.Equals(_principalAccessor.Claim().Id)
                        && nextNode.Any(a=>a.Option==0)
                        && nextNode.First().State.Equals(currentNode.First().State))
                    {
                        nextGroupIsMe = true;
                    }

                    // 下一个节点修改成待审批状态

                    var GradeList = _commonService.GetCacheList<Dictionary>().Where(w => w.Type.Equals("flowNodeGrade"));
                    foreach (var _node in nextNode)
                    {
                        _node.IsCurrentState = true;
                        _node.ReceiveDate = DateTime.Now;
                        _dbContext.Update(_node);

                        if (!nextGroupIsMe) // 下一个节点，还是我审批的话，就不发通知消息
                        {
                            // 拼接通知消息
                            Remind messageData = new Remind();
                            messageData.Title = $"【{GradeList.FirstOrDefault(f => f.Value.Equals(_node.Grade)).Name}】审批提示";
                            var maxDaysStr = _node.MaxDays > 0 ? "审批时间限制为【" + _node.MaxDays + "】后" : "";
                            string formName = "";

                            if (formObject != null)
                                formName = formObject.Name;
                            var content = $"{formName} 单号为：{bf.BillNumber}，有一条待审批信息,请及时处理。{maxDaysStr}";
                            messageData.Content = content;
                            messageData.IsTop = false;
                            messageData.ReceiverId = _node.PersonId;
                            messageData.SenderId = 0;
                            messageData.SendTime = DateTime.Now;
                            messageData.SendName = "系统";
                            messageData.InUse = true;
                            messageData.PageId = formObject?.PageId;
                            messageData.BillId = bf.BillId;
                            messageData.BillFlowId = bf.Id;
                            list.Add(messageData);
                        }
                    }

                    // 更新流程 状态
                    bf.State = nextNode.First().State;
                    _dbContext.Update(bf);
                }
                // 同步修改单据状态
                _dbContext.Database.ExecuteSqlRaw("update " + formObject.Value + $" set state = {bf.State},lastEditUserId={_principalAccessor.Claim().Id},LastEditUserName='{_principalAccessor.Claim().Name}',LastEditDate='{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}' where id={bf.BillId}");
            }
            //保存修改,当前审批节点的用户审批完成
            if (list.Count > 0)
                _dbContext.AddRange(list);
            bool success = _dbContext.SaveChanges() > 0;
            if (success)
            {
                // 如果我审批的时候 下一个节点 是我，那么自动审批下一个节点
                if (nextGroupIsMe)
                    BillFlowNextAction((int)AttitudeTypeEnum.转到下一步, bf.BillId, bf.FormId);

                return (true, "审批成功", bf.State); // 成功就是表示已审批
            }
            else
                return (false, $"流程保存失败;", -1000);
        }
        /// <summary>
        /// 流程 自动审批
        /// （当前流程审批组中模式为 
        /// 1.全部通过审批的
        /// 2.maxdays有限时审批的，
        /// 3.option 为0
        /// 没有特殊操作的 才会自动审批）
        /// </summary>
        /// <param name="bf"></param>
        /// <returns></returns>
        public (bool isOk, string msg) BillFlowAutoFinishAction(BillFlow bf)
        {
            // 当前审批节点
            var currentNode = bf.BillFlowNode.Where(w => w.IsCurrentState).ToList();
            // 下一个节点
            List<BillFlowNode> nextNode = null;
            //所有审批节点分组去重，取group
            var groups = bf.BillFlowNode.Select(s => s.Group).Distinct();
            // 如果找到下一个节点 nextNode
            var queryGroups = groups.Where(g => g > currentNode.First().Group).OrderBy(ob => ob);
            if (queryGroups.Any())
            {
                nextNode = bf.BillFlowNode.Where(w => w.Group == queryGroups.First()).ToList();
            }

            // 获取未审批的 list
            var currentGroupNoChecked = currentNode.Where(w => !w.IsChecked);
            // 如果该组模式要是全部通过或者 模式为一个人通过并且是只有一个未审核人
            if (currentNode.First().Mode.Equals(1) || (currentNode.First().Mode.Equals(2) && currentGroupNoChecked.Count() == 1))
            {
                if (currentGroupNoChecked.Count() == 0)
                {
                    // 已经审批人的数量 和 该组是数量相同 - 异常
                    return (false, "数据异常，当前组 未审核人的数量不应该是0");
                }
                foreach (var c in currentGroupNoChecked)
                {
                    if (c.MaxDays != 0 && c.Option == 0) // 没有特殊操作和限最大审批天数的
                    {
                        if (c.ReceiveDate?.AddDays(c.MaxDays) < DateTime.Now)
                        {
                            // 自动审批的node

                            //c.IsCurrentState = false;
                            c.IsChecked = true; // 设置 是否已审批                   
                            c.CheckupPersonId = 0;
                            c.CheckupDate = DateTime.Now;
                            c.IsAutoChecked = true;
                            c.Remark = "机器自动审批";
                            _dbContext.Update(c);
                        }
                    }
                }
            }

            // ？？？这里有问题！！！！？？？
            bool flag = true;
            foreach (var item in currentGroupNoChecked)
            {
                if (!item.IsChecked)
                {
                    // 查找没有审核的 节点。
                    flag = false;
                }
            }

            List<Remind> list = new List<Remind>();

            // 同节点有 没有审核的节点，就不能下一步

            if (flag) // 当前审批节点 都是已经审核的节点。
            {
                foreach (var item in currentNode)
                {
                    item.IsCurrentState = false;
                    _dbContext.Update(item);
                }
                var formObject = _dbContext.Form.FirstOrDefault(f => f.Id.Equals(bf.FormId));
                if (nextNode == null)
                {
                    return (false, "当前审批节点是最后一个审批节点，不允许自动审批结束");
                }
                else
                {
                    // 下一个节点修改成待审批状态

                    var GradeList = _commonService.GetCacheList<Dictionary>().Where(w => w.Type.Equals("flowNodeGrade"));
                    foreach (var _node in nextNode)
                    {
                        _node.IsCurrentState = true;
                        _node.ReceiveDate = DateTime.Now;
                        _dbContext.Update(_node);

                        // 拼接通知消息
                        Remind messageData = new Remind();
                        messageData.Title = $"【{GradeList.FirstOrDefault(f => f.Value.Equals(_node.Grade)).Name}】审批提示";
                        var maxDaysStr = _node.MaxDays > 0 ? "审批时间限制为【" + _node.MaxDays + "】后" : "";
                        string formName = "";
                        if (formObject != null)
                            formName = formObject.Name;
                        var content = $"{formName} 单号为：{bf.BillNumber}，有一条待审批信息，请及时处理。{maxDaysStr}";
                        messageData.Content = content;
                        messageData.IsTop = false;
                        messageData.ReceiverId = _node.PersonId;
                        messageData.SenderId = 0;
                        messageData.SendTime = DateTime.Now;
                        messageData.SendName = "系统";
                        messageData.InUse = true;
                        messageData.PageId = formObject?.PageId;
                        messageData.BillId = bf.BillId;
                        messageData.BillFlowId = bf.Id;
                        list.Add(messageData);
                    }
                    // 更新流程 状态
                    bf.State = nextNode.First().State;
                    _dbContext.Update(bf);
                }
                // 同步修改单据状态
                _dbContext.Database.ExecuteSqlRaw("update " + formObject.Value + $" set state = {bf.State},lastEditUserId={_principalAccessor.Claim().Id},LastEditUserName='{_principalAccessor.Claim().Name}',LastEditDate='{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}' where id={bf.BillId}");
            }
            if (list.Count > 0)
                _dbContext.AddRange(list); // 添加消息
            //保存修改,当前审批节点的用户审批完成
            bool success = _dbContext.SaveChanges() > 0;
            if (success)
                return (true, bf.State.ToString()); // 成功就是表示已审批
            else
                return (false, $"流程自动保存失败;");
        }
        /// <summary>
        /// 流程--2 作废
        /// </summary>
        /// <param name="bf">流程数据</param>
        /// <returns></returns>
        public (bool isOk, string msg, int state) BillFlowInvalidAction(BillFlow bf)
        {
            bf.State = (int)AttitudeTypeEnum.作废;
            bf.LastEditDate = DateTime.Now;
            bf.LastEditUserId = _principalAccessor.Claim().Id;
            bf.LastEditUserName = _principalAccessor.Claim().Name;
            _dbContext.Update(bf);

            // 同步修改单据状态
            var formObject = _dbContext.Form.FirstOrDefault(f => f.Id.Equals(bf.FormId));
            _dbContext.Database.ExecuteSqlRaw("update " + formObject.Value + $" set state = {bf.State},lastEditUserId={_principalAccessor.Claim().Id},LastEditUserName='{_principalAccessor.Claim().Name}',LastEditDate='{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}' where id={bf.BillId}");

            if (_dbContext.SaveChanges() > 0)
            {
                return (true, "作废成功", (int)AttitudeTypeEnum.作废);
            }
            else
            {
                return (false, "作废失败", -1000);
            }
        }
        /// <summary>
        /// 流程 3--退回到节点
        /// </summary>
        /// <param name="group">退回到的节点group值</param>
        /// <param name="bf">流程数据</param>
        /// <returns></returns>
        public (bool isOk, string msg, int state) BillFlowBackAction(BillFlow bf, int group)
        {
            // 0. check 退回条件
            // 1.检测是否还有该group 节点
            // 2.先把节点的 iscurrentState 和 ischecked 改为false
            // 3.修改grop 节点上的 iscurrentState
            // 4.修改状态单据上的 状态

            // check 不能退回到已支付的前面的节点。
            var payNodeQuery = bf.BillFlowNode.Where(w => w.IsChecked && w.State.Equals((int)BillStateEnum.payment));
            if(payNodeQuery.Any())
            {
                // 如果退回的到已支付前面，不允许退回。
                if(payNodeQuery.First().Group>group)
                {
                    return (false, "退回失败，不能退回到已支付完成前面的节点。", bf.State);
                }
            }
            // 1.
            var q1 = bf.BillFlowNode.Where(w => w.Group.Equals(group));
            if (q1.Any())
            {
                // 设置节点状态，将退回到的节点及之后的节点审批状态清空，将退回到的节点设为当前节点
                var q2 = bf.BillFlowNode.Where(w => w.Group >= group);
                foreach (var item in q2)
                {
                    if (item.Group.Equals(group))
                        item.IsCurrentState = true;
                    else
                        item.IsCurrentState = false;
                    item.IsChecked = false;
                    item.IsAutoChecked = false;
                    item.CheckupPersonId = null;
                    item.ReceiveDate = DateTime.Now;
                    item.CheckupDate = null;
                    _dbContext.Update(item);
                }
                //// 2
                //var q2 = bf.BillFlowNode.Where(w => w.IsCurrentState);
                //foreach (var item in q2)
                //{
                //    item.IsCurrentState = false;
                //    item.IsChecked = false;
                //    item.IsAutoChecked = false;
                //    item.CheckupPersonId = null;
                //    _dbContext.Update(item);
                //}
                //// 3
                //foreach (var item in q1)
                //{
                //    item.IsCurrentState = true;
                //    item.IsChecked = false;
                //    _dbContext.Update(item);
                //}

                // 4 修改状态单据上的 状态
                bf.State = q1.FirstOrDefault().State;
                bf.LastEditDate = DateTime.Now;
                bf.LastEditUserId = _principalAccessor.Claim().Id;
                bf.LastEditUserName = _principalAccessor.Claim().Name;
                _dbContext.Update(bf);

                // 同步修改单据状态
                var formObject = _dbContext.Form.FirstOrDefault(f => f.Id.Equals(bf.FormId));
                _dbContext.Database.ExecuteSqlRaw("update " + formObject.Value + $" set state = {bf.State},lastEditUserId={_principalAccessor.Claim().Id},LastEditUserName='{_principalAccessor.Claim().Name}',LastEditDate='{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}' where id={bf.BillId}");
                if (_dbContext.SaveChanges() > 0)
                    return (true, "退回成功", bf.State);
                else
                    return (false, "退回失败", -1000);
            }
            else
            {
                return (false, "没有找到退回的节点", -1000);
            }
        }
        /// <summary>
        /// 流程 4--退回到制单
        /// </summary>
        /// <param name="bf">流程数据</param>
        /// <returns></returns>
        public (bool isOk, string msg, int state) BillFlowBackToCreate(BillFlow bf)
        {
            // 步骤
            // 1.修改待审批节点都为 false
            // 2.修改流程状态为 0
            var q2 = bf.BillFlowNode.Where(w => w.IsCurrentState);
            foreach (var item in q2)
            {
                item.IsCurrentState = false;
                item.IsChecked = false;
                item.IsAutoChecked = false;
                item.CheckupPersonId = null;
                _dbContext.Update(item);
            }
            bf.State = 0;
            bf.LastEditDate = DateTime.Now;
            bf.LastEditUserId = _principalAccessor.Claim().Id;
            bf.LastEditUserName = _principalAccessor.Claim().Name;
            _dbContext.Update(bf);
            // 同步修改单据状态
            var formObject = _dbContext.Form.FirstOrDefault(f => f.Id.Equals(bf.FormId));
            _dbContext.Database.ExecuteSqlRaw("update " + formObject.Value + $" set state = {bf.State},lastEditUserId={_principalAccessor.Claim().Id},LastEditUserName='{_principalAccessor.Claim().Name}',LastEditDate='{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}' where id={bf.BillId}");
            if (_dbContext.SaveChanges() > 0)
                return (true, "退回成功", bf.State);
            else
                return (false, "退回失败", -1000);
        }
        /// <summary>
        /// 流程 5-- 撤销
        /// </summary>
        /// <param name="bf"></param>
        /// <returns></returns>
        public (bool isOk, string msg, int state) BillFlowRevoke(BillFlow bf)
        {
            //步骤 （审批完成，如果待审批节点，还没有人审批，就可以撤销）
            // 1.获取待审批节点，
            // 2.获取上一个审批节点
            // 3.如果上一个节点是我审批的，并且 待审批节点还没有一个人审批，就可以撤销
            // 4.判断上一个节点的模式，如果mode==全部通过，正常撤销
            // 5.如果是mode == 一人通过  如果审批节点是多人，则无法撤销。

            var checkResult = CheckBillFlowRevoke(bf);
            if (!checkResult.isOk)
                return (false, checkResult.msg, -1000);

            // 1
            var currentNode = bf.BillFlowNode.Where(w => w.IsCurrentState).ToList();

            // 2
            var g = currentNode.FirstOrDefault().Group; // 待审批节点group
            var g2 = bf.BillFlowNode.Select(s => s.Group)
                .Distinct()
                .Where(w => w < g)
                .OrderByDescending(o => o)
                .FirstOrDefault(); // 获取上一个节点的 group

            var preNode = bf.BillFlowNode.Where(w => w.Group == g2);// pre Node

            // 3
            var myBillFlowNode = preNode.Where(a => a.PersonId.Equals(_principalAccessor.Claim().Id));

            // 4 判断上一个节点模式 1.全部通过 2.一人通过
            //如果模式为一人通过，并且节点审批人大于1，就无法审批
            //if (preNode.FirstOrDefault().Mode == 2 && preNode.Count() > 1)
            //    return (false, "该节点为多人审批，且模式为:一人通过审批，就通过。所以无法撤销。其他人已经审批。");

            // 进行撤销 上一个审批节点，我审批的项设置为 未审批状态
            foreach (var bfn in myBillFlowNode)
            {
                bfn.IsCurrentState = true;
                bfn.IsChecked = false;
                bfn.CheckupDate = null;
                bfn.CheckupPersonId = null;
                bfn.IsAutoChecked = false;
                bfn.ReceiveDate = null;
                _dbContext.Update(bfn);
            }

            // 02 待审批节点 设置
            foreach (var bfn in currentNode)
            {
                bfn.IsCurrentState = false;
                bfn.IsAutoChecked = false;
                _dbContext.Update(bfn);
            }

            //03 更改流程和表单状态 为 撤回节点的表单状态
            bf.State = myBillFlowNode.First().State;
            bf.LastEditDate = DateTime.Now;
            bf.LastEditUserId = _principalAccessor.Claim().Id;
            bf.LastEditUserName = _principalAccessor.Claim().Name;
            _dbContext.Update(bf);

            // 04 同步修改单据状态
            var formObject = _dbContext.Form.FirstOrDefault(f => f.Id.Equals(bf.FormId));
            _dbContext.Database.ExecuteSqlRaw("update " + formObject.Value + $" set state = {bf.State},lastEditUserId={_principalAccessor.Claim().Id},LastEditUserName='{_principalAccessor.Claim().Name}',LastEditDate='{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}' where id={bf.BillId}");

            if (_dbContext.SaveChanges() > 0)
                return (true, "撤销成功", bf.State);
            else
                return (false, "撤销失败", -1000);
        }
        /// <summary>
        /// 流程 5-- 撤销 判断是否能撤销
        /// </summary>
        /// <param name="bf"></param>
        /// <returns></returns>
        public (bool isOk, string msg) CheckBillFlowRevoke(BillFlow bf = null)
        {
            //　验证流程
            var checkResult1 = CheckThisBillFlow(bf);
            if (!checkResult1.isOk)
                return (checkResult1.isOk, checkResult1.msg);
            // 1
            var currentNode = bf.BillFlowNode.Where(w => w.IsCurrentState).ToList();
            if (!currentNode.Any())
            {
                return (false, "没有待审批节点");
            }
            // 2
            var g = currentNode.FirstOrDefault().Group; // 待审批节点group
            var g2 = bf.BillFlowNode.Select(s => s.Group)
                .Distinct()
                .Where(w => w < g)
                .OrderByDescending(o => o)
                .FirstOrDefault(); // 获取上一个节点的 group

            var preNode = bf.BillFlowNode.Where(w => w.Group == g2);// pre Node

            // 3
            if (preNode.Any())
            {
                var myBillFlowNode = preNode.Where(a => a.PersonId.Equals(_principalAccessor.Claim().Id));
                if (myBillFlowNode.Any() && !currentNode.Any(a => a.IsChecked))
                {
                    // 4 判断上一个节点模式 1.全部通过 2.一人通过

                    //如果模式为一人通过，并且节点审批人大于1，就无法审批
                    if (preNode.FirstOrDefault().Mode == 2 && preNode.Count() > 1)
                        return (false, "该节点为多人审批，且模式为:一人通过审批，就通过。所以无法撤销。其他人已经审批。");
                    else
                        return (true, "可以撤销");
                }
                else
                    return (false, "上一个节点中，没有我审批的项目，无法撤销");
            }
            else
                return (false, "没有上一个节点，无法撤销");
        }


        /// <summary>
        /// 根据单据billFlowId查找 流程数据
        /// </summary>
        /// <param name="billFlowId">单据表中 billFlowId</param>
        /// <returns>BillFlow</returns>
        public BillFlow GetBillFlowById(int billFlowId)
        {
            int companyId = _systemService.GetCurrentSelectedCompanyId();
            var billFlowInfo = _dbContext.BillFlow
                .Include(i => i.BillFlowNode)
                .Include(i => i.Form)
                .ThenInclude(t => t.FormState).ThenInclude(t=>t.FormStateOption)
                .AsNoTracking().FirstOrDefault(f => f.Id == billFlowId);

            billFlowInfo.BillFlowNode.ForEach(item =>
            {
                item.Person = _commonService.GetCachePersonBasicInfoList(companyId).FirstOrDefault(f => f.Id == item.PersonId);
                item.Role = _commonService.GetCacheList<Role>().FirstOrDefault(f => f.Id == item.RoleId);
            });
            billFlowInfo.BillFlowNode = billFlowInfo.BillFlowNode.OrderBy(o => o.Group).ToList();
            billFlowInfo.Form.FormState = billFlowInfo.Form.FormState.OrderBy(o => o.Value).ToList();
            return billFlowInfo;
        }
        /// <summary>
        /// 根据单据billId查找 流程数据
        /// </summary>
        /// <param name="billId">单据表中 Id</param>
        /// <param name="formId">单据表中 formId</param>
        /// <returns></returns>
        public BillFlow GetBillFlowByBillId(int billId, int formId)
        {
            int companyId = _systemService.GetCurrentSelectedCompanyId();
            var billFlowInfo = _dbContext.BillFlow
                .Include(i => i.BillFlowNode)
                .Include(i => i.Form)
                .ThenInclude(t => t.FormState)
                .AsNoTracking().FirstOrDefault(f => f.BillId.Equals(billId)
                && f.FormId.Equals(formId)
                && f.CompanyId == _systemService.GetCurrentSelectedCompanyId());

            billFlowInfo.BillFlowNode.ForEach(item =>
            {
                item.Person = _commonService.GetCachePersonBasicInfoList(companyId).FirstOrDefault(f => f.Id == item.PersonId);
                item.Role = _commonService.GetCacheList<Role>().FirstOrDefault(f => f.Id == item.RoleId);
            });
            billFlowInfo.BillFlowNode = billFlowInfo.BillFlowNode.OrderBy(o => o.Group).ToList();
            billFlowInfo.Form.FormState = billFlowInfo.Form.FormState.OrderBy(o => o.Value).ToList();
            return billFlowInfo;
        }
        /// <summary>
        /// 根据表单内容生成审批流程（可定义不同状态分属不同组织机构），返回生成结果
        /// </summary>
        /// <typeparam name="T">表单类型</typeparam>
        /// <param name="billObj">表单数据</param>
        /// <param name="refresh">是否重新生成流程</param>
        /// <param name="orgIds">组织机构</param>
        /// <param name="stateOrgs">单据状态及对应组织机构列表</param>
        /// <returns>item1为是否成功，item2为错误消息</returns>
        public Tuple<bool, string> GetBillFlow<T>(T billObj, bool refresh, int[] orgIds, params KeyValuePair<int, int[]>[] stateOrgs) where T : class
        {
            int companyId = _systemService.GetCurrentSelectedCompanyId();

            //直接获取已保存的流程，不重新生成
            var billFlowId = Common.GetPropertyValue<T, int>(billObj, "BillFlowId");
            var billFlowInfo = _dbContext.BillFlow
                .Include(i => i.BillFlowNode)
                .Include(i => i.Form)
                .ThenInclude(t => t.FormState)
                .AsNoTracking().FirstOrDefault(f => f.Id == billFlowId);
            if (billFlowInfo != null)
            {
                // 排序
                billFlowInfo.BillFlowNode = billFlowInfo.BillFlowNode.OrderBy(o => o.Group).ToList();
                billFlowInfo.Form.FormState = billFlowInfo.Form.FormState.OrderBy(o => o.Value).ToList();
            }
            if (!refresh)
            {
                billFlowInfo?.BillFlowNode.ForEach(item =>
                {
                    item.Person = _commonService.GetCachePersonBasicInfoList(companyId).FirstOrDefault(f => f.Id == item.PersonId);
                    item.Role = _commonService.GetCacheList<Role>().FirstOrDefault(f => f.Id == item.RoleId);
                });
                // 更新表单中的流程属性值
                Utils.Common.SetPropertyValue<T>(billObj, "BillFlow", billFlowInfo);
                return new Tuple<bool, string>(true, "流程获取成功");
            }

            // 获取组织机构Id
            List<int> orgIdList = new List<int>();

            //判断 单据中必须含有OrgId字段的，没有的继续往下走
            var prop = typeof(T).GetProperty("OrgId");
            if (orgIds.Length == 0 && prop != null)
            {
                var orgId = Common.GetPropertyValue<T, int>(billObj, "OrgId");
                if (orgId != 0)
                    orgIdList.Add(orgId);
                else
                    throw new Exception("未找到组织机构信息");
            }

            // 获取流程Id
            var flowId = GetFlowId(billObj);
            if (flowId == 0)
                return new Tuple<bool, string>(false, "未匹配到相应审批流");

            // 获取已有审批流，如果不存在重新定义
            var billFlow = billFlowInfo ?? new BillFlow();
            int createUserId = _principalAccessor.Claim().Id;
            string createUserName = _principalAccessor.Claim().Name;

            var billId = Common.GetPropertyValue<T, int>(billObj, "Id");
            var billNumber = Common.GetPropertyValue<T, string>(billObj, "Number");

            // 设置流程中的部分字段值
            billFlow.FlowId = flowId;
            billFlow.CompanyId = companyId;
            billFlow.CreateUserId = createUserId;
            billFlow.CreateUserName = createUserName;
            billFlow.BillId = billId;
            billFlow.BillNumber = billNumber;
            if (billFlow.BillFlowNode == null) billFlow.BillFlowNode = new List<BillFlowNode>();
            billFlow.BillFlowNode.ForEach(item => item.IsFixedNode = false);

            var flow = _dbContext.Flow
                .Include(i => i.FlowNode)
                .Include(i => i.Form)
                .ThenInclude(t => t.FormState)
                .AsNoTracking()
                .First(f => f.Id.Equals(flowId));

            billFlow.FormId = flow.FormId;
            billFlow.Form = flow.Form;

            List<FlowNode> flowNodes = flow.FlowNode.OrderBy(o => o.State).ThenBy(o => o.Group).ToList();
            if (flowNodes.Count > 0)
            {
                // 所有可能出现的组织机构
                List<int> allOrgIdList = new List<int>();
                // 如果存在组织机构OrgId
                if (prop != null)
                    allOrgIdList = allOrgIdList.Union(orgIdList).ToList();

                foreach (var item in stateOrgs)
                {
                    allOrgIdList = allOrgIdList.Union(item.Value).ToList();
                }

                if (allOrgIdList.Count > 0)
                    allOrgIdList = _systemService.GetOrgWithParents(allOrgIdList.ToArray()).Select(s => s.Id).ToList();

                var allRoleIdList = flowNodes.Where(w => w.RoleId != 0).Select(s => s.RoleId).ToList();
                // 查询所有可能用到的角色审核范围
                List<UserCheckupOrganization> ucoList = _dbContext.UserCheckupOrganization
                    .Include(i => i.Person)
                    .Where(w => allRoleIdList.Contains(w.RoleId) && allOrgIdList.Contains(w.OrganizationId))
                    .AsNoTracking().ToList();
                // 查询所有可能用到的角色信息
                var roleList = _commonService.GetCacheList<Role>().Where(w => allRoleIdList.Contains(w.Id)).ToList();

                foreach (var flowNode in flowNodes)
                {
                    BillFlowNode node;
                    if (flowNode.RoleType == (int)Enums.FlowRoleType.Role) //按审核角色匹配
                    {
                        var orgs = orgIdList;
                        if (stateOrgs.Any(a => a.Key == flowNode.State))
                            orgs = stateOrgs.FirstOrDefault(f => f.Key == flowNode.State).Value.ToList();

                        foreach (var orgId in orgs)
                        {
                            node = billFlow.BillFlowNode.FirstOrDefault(f => f.State == flowNode.State && f.RoleType == flowNode.RoleType && f.RoleId == flowNode.RoleId && f.Group == flowNode.Group && f.OrgId == orgId);
                            if (node == null)
                            {
                                node = new BillFlowNode();
                                billFlow.BillFlowNode.Add(node);
                            }
                            node.OrgId = orgId;
                            node.Group = flowNode.Group;
                            node.State = flowNode.State;
                            node.Option = flowNode.Option;
                            node.Mode = flowNode.Mode;
                            node.RoleType = flowNode.RoleType;
                            node.RoleId = flowNode.RoleId;
                            node.Role = roleList.FirstOrDefault(f => f.Id == flowNode.RoleId);
                            node.MaxDays = flowNode.MaxDays;
                            node.Grade = flowNode.Grade;
                            node.PersonList = GetRolePersonList(ucoList, flowNode.RoleId, orgId).Select(s => s.ToBasicInfo()).ToList();
                            if (node.PersonList.Count == 1)
                                node.PersonId = node.PersonList[0].Id;
                            node.NodeType = 0;
                            node.IsFixedNode = true;
                            node.CreateUserId = _principalAccessor.Claim().Id;
                            node.CreateUserName = _principalAccessor.Claim().Name;
                        }
                    }
                    else
                    {
                        node = billFlow.BillFlowNode.FirstOrDefault(f => f.State == flowNode.State && f.RoleType == flowNode.RoleType && f.Group == flowNode.Group);
                        if (node == null)
                        {
                            node = new BillFlowNode();
                            billFlow.BillFlowNode.Add(node);
                        }
                        switch (flowNode.RoleType)
                        {
                            case (int)Enums.FlowRoleType.CreateUser:
                                node.PersonId = createUserId;
                                node.PersonList = new List<Person> { _commonService.GetCachePersonBasicInfoList(companyId).First(f => f.Id == createUserId) };

                                break;
                            case (int)Enums.FlowRoleType.CheckPerson:
                                if (node.PersonId != 0)
                                    node.PersonList = new List<Person> { _commonService.GetCachePersonBasicInfoList(companyId).First(f => f.Id == node.PersonId) };
                                break;
                            default:
                                break;
                        }
                        node.Group = flowNode.Group;
                        node.State = flowNode.State;
                        node.Option = flowNode.Option;
                        node.Mode = flowNode.Mode;
                        node.RoleType = flowNode.RoleType;
                        node.RoleId = flowNode.RoleId;
                        node.MaxDays = flowNode.MaxDays;
                        node.Grade = flowNode.Grade;
                        node.NodeType = 0;
                        node.IsFixedNode = true;
                        node.CreateUserId = _principalAccessor.Claim().Id;
                        node.CreateUserName = _principalAccessor.Claim().Name;
                    }
                }
            }
            billFlow.BillFlowNode.RemoveAll(r => !r.IsFixedNode && r.NodeType == 0);
            billFlow.BillFlowNode.ForEach(item =>
            {
                if (item.NodeType == 1)
                    item.PersonList = new List<Person> { _commonService.GetCachePersonBasicInfoList(companyId).First(f => f.Id == item.PersonId) };
            });
            // 更新表单中的流程属性值
            Utils.Common.SetPropertyValue<T>(billObj, "BillFlow", billFlow);

            return new Tuple<bool, string>(true, "获取成功");
        }

        /// <summary>
        /// 根据角色及组织机构，获取对应的可审核人员
        /// </summary>
        /// <param name="ucoList">所有角色及人员列表</param>
        /// <param name="roleId">角色Id</param>
        /// <param name="orgId">组织机构Id</param>
        /// <returns>人员列表</returns>
        private List<Person> GetRolePersonList(List<UserCheckupOrganization> ucoList, int roleId, int orgId)
        {
            var orgIdList = _systemService.GetOrgWithParents(orgId).Select(s => s.Id).ToList();
            var personList = ucoList.Where(w => w.RoleId.Equals(roleId) && orgIdList.Contains(w.OrganizationId)).Select(s => s.Person).DistinctBy(d => d.Id).ToList();
            return personList;
        }
        /// <summary>
        /// 根据表单数据返回对应的流程Id
        /// </summary>
        /// <typeparam name="T">表单类型</typeparam>
        /// <param name="billObj">表单数据</param>
        /// <returns>流程Id</returns>
        public int GetFlowId<T>(T billObj) where T : class
        {
            int companyId = _systemService.GetCurrentSelectedCompanyId();

            //单据对应的表名
            string tableName = Utils.Common.GetTableName<T>();
            //typeof(T).GetAttributeValue((TableAttribute ta) => ta.Name);
            //查询该表单对应的所有流程
            //List<Flow> flows = _dbContext.Flow
            //    .Where(w => w.Form.Value.Equals(tableName) && w.CompanyId == companyId && w.InUse)
            //    //.Include(i => i.FlowCondition)
            //    //.ThenInclude(i => i.FormFlowField) // 不能对应时会造成FlowCondition无法查询
            //    .OrderBy(o => o.Sort)
            //    .ToList();
            ////查询表单所有对应的条件
            //var flowIds = flows.Select(s => s.Id).ToList();
            //var flowConditions = _dbContext.FlowCondition
            //    .Include(i => i.FormFlowField)
            //    .Where(w => flowIds.Contains(w.FlowId))
            //    //.DefaultIfEmpty()
            //    .AsNoTracking().ToList();


            //foreach (var flow in flows)
            //{
            //    if (!flowConditions.Any())
            //        return flow.Id;                

            //    flow.FlowCondition = flowConditions.Where(w => w.FlowId == flow.Id).ToList();
            //    //if (flow.FlowCondition.Count == 0)
            //    //    return flow.Id;

            //    var check = CheckCondition(flow.FlowCondition, 0, billObj);
            //    if (check)
            //        return flow.Id;
            //}

            //查询该表单对应的所有流程
            List<Flow> flows = _dbContext.Flow
                .Where(w => w.Form.Value.Equals(tableName) && w.CompanyId == companyId && w.InUse)
                .Include(i => i.FlowCondition)
                .ThenInclude(i => i.FormFlowField)
                .OrderBy(o => o.Sort)
                .ToList();
            foreach (var flow in flows)
            {
                if (!flow.FlowCondition.Any())
                    return flow.Id;

                var check = CheckCondition(flow.FlowCondition, 0, billObj);
                if (check)
                    return flow.Id;
            }

            return 0;
        }

        /// <summary>
        /// 流程条件匹配
        /// </summary>
        /// <param name="conditionList">当前流程所有匹配条件</param>
        /// <param name="PCode">父节点</param>
        /// <param name="billObj">表单数据</param>
        /// <param name="logicOperator">逻辑运算符</param>
        /// <returns>bool</returns>
        private bool CheckCondition<T>(List<FlowCondition> conditionList, int PCode, T billObj, string logicOperator = "&&") where T : class
        {
            var conditions = conditionList.Where(w => w.PCode == PCode).ToList();
            bool result = true, currentResult;

            foreach (var condition in conditions)
            {
                if (condition.IsLeaf)
                {
                    var fv = GetFieldVelue(billObj, condition.FormFlowField.Field);
                    currentResult = CheckField(fv, condition.Value, condition.Operator, condition.FormFlowField.FieldType, condition.FormFlowField.DicType);
                }
                else
                {
                    currentResult = CheckCondition(conditionList, condition.Code, billObj, condition.Operator);
                }

                if (logicOperator == "||" && currentResult)
                    return true;
                if (logicOperator != "||" && !currentResult)
                    return false;
            }
            return result;
        }

        /// <summary>
        /// 查询对象的某一属性值
        /// </summary>
        /// <param name="billObj">对象</param>
        /// <param name="field">属性（可查询级联属性，如Organization.Name）</param>
        /// <returns>属性值</returns>
        public object GetFieldVelue<T>(T billObj, string field) where T : class
        {
            string[] atts = field.Split(".");
            object obj = billObj;
            PropertyInfo prop;
            foreach (var att in atts)
            {
                if (obj == null) return null;

                prop = obj.GetType().GetProperty(att);
                if (prop != null)
                    obj = prop.GetValue(obj);
                else
                    return null;
            }

            return obj;
        }
        #endregion

        #region 流程保存、提交
        /// <summary>
        /// 检验流程中人员是否完整
        /// </summary>
        /// <param name="billFlow"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
        public bool CheckBillFlow(BillFlow billFlow, out string msg)
        {
            msg = string.Empty;
            if (billFlow == null)
            {
                msg = "未匹配到审批流程";
                return false;
            }
            if (billFlow.BillFlowNode.Any(a => a.PersonId == 0))
            {
                msg = "人员信息不完整";
                return false;
            }

            return true;
        }

        /// <summary>
        /// 流程保存
        /// </summary>
        /// <param name="billFlow">流程对象</param>
        /// <returns>流程对象</returns>
        public BillFlow SaveBillFlow(BillFlow billFlow)
        {
            if (billFlow.FormId == 0 || billFlow.BillId == 0)
                throw new Exception("未设置流程中单据信息");

            int createUserId = _principalAccessor.Claim().Id;
            string createUserName = _principalAccessor.Claim().Name;

            var query = _dbContext.BillFlow.FirstOrDefault(f => f.Id == billFlow.Id || f.BillId == billFlow.BillId && f.FormId == billFlow.FormId);
            // 流程不存在时执行插入操作
            if (query == null)
            {
                billFlow.CompanyId = _systemService.GetCurrentSelectedCompanyId();
                billFlow.CreateUserId = createUserId;
                billFlow.CreateUserName = createUserName;
                billFlow.LastEditUserId = createUserId;
                billFlow.LastEditUserName = createUserName;
                billFlow.BillFlowNode.ForEach(item =>
                {
                    item.CreateUserId = createUserId;
                    item.CreateUserName = createUserName;
                    if (item.NodeType == 1) item.Remark = "制单时添加";
                });

                _dbContext.Add(billFlow);
            }
            else //存在时更新
            {
                query.CompanyId = billFlow.CompanyId;
                query.BillId = billFlow.BillId;
                query.FlowId = billFlow.FlowId;
                query.BillNumber = billFlow.BillNumber;
                query.Summary = billFlow.Summary;
                query.Amount = billFlow.Amount;
                query.State = billFlow.State;
                query.LastEditUserId = createUserId;
                query.LastEditUserName = createUserName;
                query.LastEditDate = DateTime.Now;
                _dbContext.Update(query);
                billFlow.Id = query.Id;
                billFlow.CreateUserId = query.CreateUserId;
                billFlow.CreateUserName = query.CreateUserName;
                billFlow.CreateDate = query.CreateDate;

                //流程节点处理
                var query2 = _dbContext.BillFlowNode.Where(w => w.BillFlowId == query.Id).ToList();
                List<int> bfnIdList = new List<int>();
                foreach (var item in billFlow.BillFlowNode)
                {
                    if (item.Id != 0) //更新节点
                    {
                        bfnIdList.Add(item.Id);
                        var q = query2.First(f => f.Id.Equals(item.Id));
                        q.Group = item.Group;
                        q.Mode = item.Mode;
                        q.RoleType = item.RoleType;
                        q.RoleId = item.RoleId;
                        q.State = item.State;
                        q.Option = item.Option;
                        q.MaxDays = item.MaxDays;
                        q.OrgId = item.OrgId;
                        q.PersonId = item.PersonId;
                        q.Grade = item.Grade;
                        //q.ReceiveDate = item.ReceiveDate;
                        //q.IsCurrentState = item.IsCurrentState;
                        //q.IsChecked = item.IsChecked;
                        //q.IsAutoChecked = item.IsAutoChecked;
                        q.NodeType = item.NodeType;
                        _dbContext.Update(q);
                    }
                    else //新增节点
                    {
                        item.BillFlowId = query.Id;
                        item.CreateUserId = createUserId;
                        item.CreateUserName = createUserName;
                        if (item.NodeType == 1) item.Remark = "制单时添加";
                        _dbContext.Add(item);
                    }
                }
                //删除已删除的节点
                _dbContext.RemoveRange(query2.Where(w => !bfnIdList.Contains(w.Id) && w.Id != 0));
            }
            _dbContext.SaveChanges();

            return billFlow;
        }

        /// <summary>
        /// 向某一个流程中的 流程状态节点中 批量添加审核人
        /// </summary>
        /// <returns></returns>
        public List<BillFlowNode> AddBillFlowNodeInTheFormState(int[] personIds, BillFlowNode bf)
        {
            List<BillFlowNode> list = new List<BillFlowNode>();
            foreach (var personId in personIds)
            {
                bf.PersonId = personId;
                list.Add(new BillFlowNode
                {
                    Group = bf.Group,
                    Mode = 1,
                    RoleType = 3,
                    RoleId = 0,
                    State = bf.State,
                    Option = 0,
                    MaxDays = 0,
                    OrgId = 0,
                    PersonId = personId,
                    //ReceiveDate = null,
                    IsCurrentState = false,
                    IsChecked = false,
                    IsAutoChecked = false,
                    Remark = "批量添加审核人",
                    CreateDate = DateTime.Now,
                    CreateUserId = _principalAccessor.Claim().Id,
                    CreateUserName = _principalAccessor.Claim().Name
                });
            }
            return list;
        }

        /// <summary>
        /// 提交流程
        /// </summary>
        /// <param name="billFlow">流程对象</param>
        /// <returns>BillFlow</returns>
        public BillFlow SubmitBillFlow(BillFlow billFlow)
        {
            if (!CheckBillFlow(billFlow, out string msg))
            {
                throw new Exception(msg);
            }
            int result = 0;
            billFlow = SaveBillFlow(billFlow);
            int currentUserId = _principalAccessor.Claim().Id;
            string currentUserName = _principalAccessor.Claim().Name;

            // 意见表中添加制单人信息
            _dbContext.Add(new Attitude
            {
                FormId = billFlow.FormId,
                BillId = billFlow.BillId,
                Type = 1,
                Title = "提交人",
                Content = "提交单据",
                Operation = "提交单据",
                //ReceiveDate = DateTime.Now,
                CreateDate = DateTime.Now,
                CreateUserId = currentUserId,
                CreateUserName = currentUserName,
                LastEditDate = DateTime.Now,
                LastEditUserId = currentUserId,
                LastEditUserName = currentUserName,
            });
            List<Remind> list = new List<Remind>();
            // 根据状态逐步查询
            var formStates = _dbContext.FormState.Where(w => w.FormId == billFlow.FormId && w.InUse && w.Value > 0).AsNoTracking().OrderBy(o => o.Value).ToList();
            foreach (var state in formStates)
            {
                if (state.IsCheckup && billFlow.BillFlowNode.Any(a => a.State == state.Value))
                {

                    var q = _dbContext.BillFlow.Include(i => i.BillFlowNode).FirstOrDefault(f => f.Id == billFlow.Id);
                    var group = q.BillFlowNode.Min(m => m.Group);
                    q.State = state.Value;
                    q.LastEditDate = DateTime.Now;
                    q.LastEditUserId = currentUserId;
                    q.LastEditUserName = currentUserName;
                    var formObject = _dbContext.Form.FirstOrDefault(f => f.Id.Equals(q.FormId));
                    var GradeList = _commonService.GetCacheList<Dictionary>().Where(w => w.Type.Equals("flowNodeGrade"));
                    q.BillFlowNode.ForEach(item =>
                    {
                        if (item.Group == group)
                        {
                            item.ReceiveDate = DateTime.Now;
                            item.IsCurrentState = true;

                            Remind messageData = new Remind();
                            messageData.Title = $"【{GradeList.FirstOrDefault(f => f.Value.Equals(item.Grade)).Name}】审批提示";
                            var maxDaysStr = item.MaxDays > 0 ? "审批时间限制为【" + item.MaxDays + "】后" : "";
                            string formName = "";

                            if (formObject != null)
                                formName = formObject.Name;
                            var content = $"{formName} 单号为：{q.BillNumber}，有一条待审批信息,请及时处理。{maxDaysStr}";
                            messageData.Content = content;
                            messageData.IsTop = false;
                            messageData.ReceiverId = item.PersonId;
                            messageData.SenderId = 0;
                            messageData.SendTime = DateTime.Now;
                            messageData.SendName = "系统";
                            messageData.InUse = true;
                            messageData.PageId = formObject?.PageId;
                            messageData.BillId = q.BillId;
                            messageData.BillFlowId = q.Id;
                            list.Add(messageData);
                        }
                        else
                        {
                            item.IsCurrentState = false;
                        }
                        item.IsChecked = false;
                        item.IsAutoChecked = false;
                        item.CheckupPersonId = null;
                        item.CheckupDate = null;
                    });
                    _dbContext.Update(q);
                    if (list.Count > 0)
                        _dbContext.AddRange(list);

                    _dbContext.SaveChanges();

                    return q;
                }
                result = state.Value;
            }

            var q2 = _dbContext.BillFlow.FirstOrDefault(f => f.Id == billFlow.Id);
            q2.State = result;
            q2.LastEditDate = DateTime.Now;
            q2.LastEditUserId = currentUserId;
            q2.LastEditUserName = currentUserName;
            _dbContext.Update(q2);

            _dbContext.SaveChanges();


            return q2;
        }
        #endregion

        #region attitude 审批记录
        /// <summary>
        /// 根据单据Id获取审批记录列表
        /// </summary>
        /// <param name="billId">单据Id</param>
        /// <param name="formId">表单Id</param>
        /// <param name="type">记录类型。1：审批记录类型 2：其他记录类型</param>
        /// <returns></returns>
        public List<Attitude> GetAttitudeByBillId(int billId, int formId, int type = 1)
        {
            var query = _dbContext.Attitude
                .Where(w => w.BillId.Equals(billId) && w.FormId.Equals(formId) && w.Type.Equals(type))
                .OrderByDescending(o => o.LastEditDate)
                .AsNoTracking()
                .ToList();
            return query;
        }
        #endregion

        #region attitude 流程审批
        /// <summary>
        /// 添加审批意见（调用这个方法必须用事务）
        /// </summary>
        /// <param name="attitude">审批表单对象</param>
        /// <returns></returns>
        public (bool isOk, string msg) AddAttitude(Attitude attitude)
        {
            if (string.IsNullOrWhiteSpace(attitude.Content))
                return (false, "审批内容不能为空");
            if (attitude.BillId == 0 || attitude.FormId == 0)
                return (false, "流程id 或者 formid 参数错误");

            var billFlowNodes = GetBillFlowNodeByBillId(attitude.BillId, attitude.FormId);
            string title = attitude.Title;
            if (billFlowNodes.Count() > 0)
            {
                var roleType = billFlowNodes.FirstOrDefault().RoleType;
                var roleId = billFlowNodes.FirstOrDefault().RoleId;
                if (roleType == 1)
                    title = _systemService.GetRole(roleId).First().Name;
                else
                    title = _systemService.GetDictionary("flowRoleType").FirstOrDefault(f => f.Value.Equals(roleType)).Name;
                attitude.ReceiveDate = billFlowNodes.FirstOrDefault().ReceiveDate;
            }

            attitude.Title = title;
            attitude.CreateUserId = _principalAccessor.Claim().Id;
            attitude.CreateUserName = _principalAccessor.Claim().Name;
            attitude.LastEditUserId = _principalAccessor.Claim().Id;
            attitude.LastEditUserName = _principalAccessor.Claim().Name;

            if (attitude.AttitudeType != (int)AttitudeTypeEnum.退回)
            {
                // 审批类型不是 退回的 操作动作说明，由后端拼接字符串
                var attitudeTypeList = _systemService.GetDictionary("attitudeType");
                attitude.Operation = attitudeTypeList.FirstOrDefault(f => f.Value == attitude.AttitudeType).Name;
            }

            if (attitude.AttitudeType != (int)AttitudeTypeEnum.只填写意见不转下一步) // 2：不是 只填写意见不转下一步
            {
                // 1.******执行审批********
                var result = BillFlowNextAction(attitude.AttitudeType, attitude.BillId, attitude.FormId, attitude.BackGroup);
                if (!result.isOk)
                {
                    // 执行审批出现问题，就返回
                    return (false, result.msg);
                }
            }

            if (attitude.AttitudeType == (int)AttitudeTypeEnum.只填写意见不转下一步) {
                var formObject = _dbContext.Form.FirstOrDefault(f => f.Id.Equals(attitude.FormId));
                _dbContext.Database.ExecuteSqlRaw("update " + formObject.Value + $" set lastEditUserId={_principalAccessor.Claim().Id},LastEditUserName='{_principalAccessor.Claim().Name}',LastEditDate='{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}' where id={attitude.BillId}");
            }

            _dbContext.Attitude.Add(attitude);
            if (_dbContext.SaveChanges() > 0)
            {
                // 添加文件
                _systemService.AddFiles<Attitude>(attitude.FileList, attitude.Id);
                return (true, "保存成功");
            }
            else
                return (false, "保存失败");

        }

        #endregion

        #region 流程中字段类型及可用操作

        /// <summary>
        /// 返回字段类型及可用的操作
        /// </summary>
        /// <returns></returns>
        public List<FieldType> GetFieldTypeList()
        {
            List<FieldType> types = new List<FieldType>();
            //审批流程中可能用到的数据类型
            string[] strTypes = { "int", "decimal", "string", "bool", "datetime", "organization", "dictionary" };
            foreach (var type in strTypes)
            {
                types.Add(new FieldType(type));
            }
            return types;
        }

        private bool CheckField(object FieldValue, object ConditionValue, string Operator, string FieldType, string DicType = "")
        {
            switch (FieldType)
            {
                case "int":
                case "decimal":
                    return Operator switch
                    {
                        "＞" => Common.GetDouble(FieldValue) > Common.GetDouble(ConditionValue),
                        "≥" => Common.GetDouble(FieldValue) >= Common.GetDouble(ConditionValue),
                        "＜" => Common.GetDouble(FieldValue) < Common.GetDouble(ConditionValue),
                        "≤" => Common.GetDouble(FieldValue) <= Common.GetDouble(ConditionValue),
                        "＝" => Common.GetDouble(FieldValue) == Common.GetDouble(ConditionValue),
                        "≠" => Common.GetDouble(FieldValue) != Common.GetDouble(ConditionValue),
                        _ => false,
                    };
                case "string":
                    return Operator switch
                    {
                        "包含" => Common.GetString(FieldValue).Contains(Common.GetString(ConditionValue)),
                        "不包含" => !Common.GetString(FieldValue).Contains(Common.GetString(ConditionValue)),
                        "＝" => Common.GetString(FieldValue).Equals(Common.GetString(ConditionValue)),
                        "≠" => !Common.GetString(FieldValue).Equals(Common.GetString(ConditionValue)),
                        _ => false,
                    };
                case "bool":
                    return Operator switch
                    {
                        "＝" => ((bool)FieldValue).Equals(Convert.ToBoolean(ConditionValue)),
                        _ => false,
                    };
                case "datetime":
                    return Operator switch
                    {
                        "＞" => Common.GetDate(FieldValue) > Common.GetDate(ConditionValue),
                        "≥" => Common.GetDate(FieldValue) >= Common.GetDate(ConditionValue),
                        "＜" => Common.GetDate(FieldValue) < Common.GetDate(ConditionValue),
                        "≤" => Common.GetDate(FieldValue) <= Common.GetDate(ConditionValue),
                        "＝" => Common.GetDate(FieldValue) == Common.GetDate(ConditionValue),
                        "≠" => Common.GetDate(FieldValue) != Common.GetDate(ConditionValue),
                        _ => false,
                    };
                case "organization":
                    return Operator switch
                    {
                        "属于" => _systemService.GetOrgWithChildren(((string)ConditionValue).Split(",").Select(s => int.Parse(s)).ToArray()).Any(a => a.Id == Common.GetInt(FieldValue)),
                        "不属于" => !_systemService.GetOrgWithChildren(((string)ConditionValue).Split(",").Select(s => int.Parse(s)).ToArray()).Any(a => a.Id == Common.GetInt(FieldValue)),
                        _ => false,
                    };
                case "dictionary":
                    return Operator switch
                    {
                        "属于" => _systemService.GetDictionaryWithChildren(DicType, ((string)ConditionValue).Split(",").Select(s => int.Parse(s)).ToArray()).Any(a => a.Value == Common.GetInt(FieldValue)),
                        "不属于" => !_systemService.GetDictionaryWithChildren(DicType, ((string)ConditionValue).Split(",").Select(s => int.Parse(s)).ToArray()).Any(a => a.Value == Common.GetInt(FieldValue)),
                        _ => false,
                    };
                default:
                    return false;
            }
        }
        #endregion

        #region 流程表单
        /// <summary>
        /// 根据表单名获取表单状态
        /// </summary>
        /// <param name="TableName">表单名</param>
        /// <param name="DefaultValue">默认值</param>
        /// <returns></returns>
        public List<FormStateResult> GetFormState(string TableName,int DefaultValue)
        {
            var query = _dbContext.Form
                .Include(t => t.FormState)
                .FirstOrDefault(t => t.Value.Equals(TableName) && t.InUse);
            List<FormStateResult> result = new List<FormStateResult>();
            if (query != null)
            {
                query.FormState.ForEach(e =>
                {
                    if (e.InUse)
                    {
                        result.Add(new FormStateResult()
                        {
                            Name = e.Name,
                            Value = e.Value,
                            IsCheck = e.Value.Equals(DefaultValue)
                        });
                    }
                });
            }
            return result;
        }
        #endregion
        #endregion
    }
}
