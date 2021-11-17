using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GLXT.Spark.Entity;
using GLXT.Spark.Entity.XTGL;
using GLXT.Spark.Enums;
using GLXT.Spark.IService;


namespace GLXT.Spark.Controllers.XTGL
{
    /// <summary>
    /// 流程信息
    /// </summary>
    [Route("api/XTGL/[controller]")]
    [ApiController]
    [Authorize]
    public class BillFlowController : BaseController
    {
        private readonly DBContext _dbContext;
        private readonly ICommonService _commonService;
        private readonly IBillFlowService _billFlowService;
        private readonly ISystemService _systemService;
        public BillFlowController(DBContext dbContext,
            ICommonService commonService,
            ISystemService systemService,
            IBillFlowService billFlowService)
        {
            _dbContext = dbContext;
            _commonService = commonService;
            _billFlowService = billFlowService;
            _systemService = systemService;
        }

        #region BillFlow 流程信息

        /// <summary>
        /// 获取流程信息
        /// </summary>
        /// <param name="currentPage"></param>
        /// <param name="pageSize"></param>
        /// <param name="formId"></param>
        /// <param name="number"></param>
        /// <param name="grade">重要等级</param>
        /// <param name="state"></param>
        /// <param name="type">0我创建的 1 我待审批的 2我已经审批的</param>
        /// <returns></returns>
        [HttpGet, Route("GetBillFlowPaging")]
        public IActionResult GetBillFlowPaging(int currentPage, int pageSize, int? formId, int? state,int? grade, string number = "", int type = 0)
        {
            int companyId = _systemService.GetCurrentSelectedCompanyId();
            IQueryable<BillFlow> query = _dbContext.BillFlow
                .Include(t => t.BillFlowNode).ThenInclude(t=>t.Person)
                .Include(t => t.Form).ThenInclude(i => i.FormState)
                .Where(w => w.CompanyId.Equals(companyId))
                .OrderByDescending(o => o.LastEditDate);
            if(state.HasValue) // 审批状态
            {
                if(state.Value == 0)
                    query = query.Where(w => w.State.Equals(0));
                if (state.Value == 1)
                    query = query.Where(w => w.State>0&&w.State<10000);
                if (state.Value == 10000)
                    query = query.Where(w => w.State.Equals(10000));
            }
            // 重要程度
            if(grade.HasValue)
                query = query.Where(w => w.BillFlowNode.Any(a => a.Grade.Equals(grade)));
            
            // 表单类型
            if (formId.HasValue)
                query = query.Where(w => w.FormId.Equals(formId.Value));

            // 单据号
            if (!string.IsNullOrEmpty(number))
                query = query.Where(w => w.BillNumber.Contains(number));
            // 单据 各种 状态
            if (type.Equals(0))
                // 这是我发起的
                query = query.Where(w => w.CreateUserId.Equals(GetUserId()));
            else if (type.Equals(1))
                // 待处理(包含制单和 待处理)
                query = query.Where(w => w.BillFlowNode.Any(w => w.PersonId.Equals(GetUserId()) && w.IsCurrentState && !w.IsChecked) || (w.CreateUserId.Equals(GetUserId())&&w.State.Equals(0)));
            else if (type.Equals(2))
                // 已审批
                query = query.Where(w => w.BillFlowNode.Any(w => w.PersonId.Equals(GetUserId()) && !w.IsCurrentState && w.IsChecked));
            

            int count = query.Count();
            var result = query.Skip((currentPage - 1) * pageSize)
                .Take(pageSize).AsNoTracking().ToList();
            //判断是否有数据，若无则返回第一页
            if (result.Count() == 0)
            {
                currentPage = 1;
                result = query.Skip((currentPage - 1) * pageSize)
                    .Take(pageSize).AsNoTracking().ToList();
            }

            return Ok(new { code = StatusCodes.Status200OK, data = result, count = count });
        }

        /// <summary>
        /// 显示我的流程 数量数据
        /// </summary>
        /// <returns></returns>
        [HttpGet, Route("GetMyBillFlowShowCount")]
        public IActionResult GetMyBillFlowShowCount()
        {
            // 制单（普通） 和 待审批（普通，重要，不重要）
            int companyId = _systemService.GetCurrentSelectedCompanyId();
            
            var myState0Count = _dbContext.BillFlow.Where(w => w.CreateUserId.Equals(GetUserId()) && w.State.Equals(0)).Count();
            //var myWaitApproval = _dbContext.BillFlow.Where(w => w.CompanyId.Equals(companyId)&&w.BillFlowNode.Any(a => a.PersonId.Equals(GetUserId()) && a.IsCurrentState && !a.IsChecked));
            var query = _dbContext.BillFlowNode.Where(a => a.BillFlow.CompanyId.Equals(companyId) && a.PersonId.Equals(GetUserId()) && a.IsCurrentState && !a.IsChecked);
            var unImportant = query.Where(w => w.Grade == -1).Select(s => s.BillFlowId).Distinct().Count();
            var important = query.Where(w => w.Grade == 1).Select(s => s.BillFlowId).Distinct().Count();
            var normal = query.Where(w => w.Grade == 0).Select(s => s.BillFlowId).Distinct().Count();
            var gradeList = _commonService.GetCacheList<Dictionary>().Where(w => w.Type.Equals("flowNodeGrade")).Select(s => new { s.Name, s.Value }).ToList();
            return Ok(new { code = StatusCodes.Status200OK, data0 = normal+ myState0Count, data_1 = unImportant,data1=important, gradeList= gradeList });
        }
        /// <summary>
        /// 初始化 我的流程页面
        /// </summary>
        /// <returns></returns>
        [HttpGet, Route("InitMyBillFlow")]
        public IActionResult InitMyBillFlow()
        {
            var formList = _dbContext.Form.Where(w => w.InUse && w.NeedCheckup).Select(s=>new { s.Id,s.Name}).AsNoTracking().ToList();
            var dicList = _commonService.GetCacheList<Dictionary>().Where(w=>w.Type.Equals("flowNodeGrade")).Select(s=>new { s.Name,s.Value}).ToList();
            return Ok(new { code = StatusCodes.Status200OK, formList = formList, dicList = dicList });
        }
        /// <summary>
        /// 判断是否已经审批了
        /// </summary>
        /// <returns></returns>
        [HttpGet, Route("IsCheckUp")]
        public IActionResult IsCheckUp(int? billFlowId)
        {
            // 验证流程
            // 如果单据的状态是待审批，才能打开，否则不打开
            if (billFlowId.HasValue)
            {
                var bf = _dbContext.BillFlow
                                    .Include(i => i.BillFlowNode)
                                    .FirstOrDefault(w => w.Id.Equals(billFlowId));
                if (bf == null)
                    return Ok(new { code = StatusCodes.Status200OK, data = false, message = "找不到该条流程" });

                var checkResult1 = _billFlowService.CheckThisBillFlow(bf);
                if (!checkResult1.isOk)
                    return Ok(new { code = StatusCodes.Status200OK, data = false, message = checkResult1.msg });
                var checkResult2 = _billFlowService.CheckBillFlowNodeIsChecked(bf.Id, bf);
                if (checkResult2.code != (int)Enums.AttitudeEnum.未审批) // 如果不是 未审批，就不能继续下面的审批了哦
                    return Ok(new { code = StatusCodes.Status200OK, data = false, message = checkResult2.msg });
                return Ok(new { code = StatusCodes.Status200OK, data = true });
            }
            else
                return Ok(new { code = StatusCodes.Status200OK, data = false });
        }

        #endregion

        #region BillFlowNode 单据流程节点

        /// <summary>
        /// 编辑流程添加 BillFlowNode
        /// </summary>
        /// <param name="billFlowNodes"></param>
        /// <returns></returns>
        [HttpPost, Route("AddBillFlowNodes")]
        public IActionResult AddBillFlowNodes(List<BillFlowNode> billFlowNodes = null)
        {
            if (billFlowNodes == null || billFlowNodes.Count == 0)
            {
                return Ok(new { code = StatusCodes.Status400BadRequest, message = "没有需要保存的" });
            }
            var bf = _billFlowService.GetBillFlowById(billFlowNodes[0].BillFlowId);
            // 判断流程是否合法
            var checkResult1 = _billFlowService.CheckThisBillFlow(bf);
            if (!checkResult1.isOk)
                return Ok(new { code = StatusCodes.Status400BadRequest, message = checkResult1.msg });

            List<int> addPersonIdList = new List<int>();
            foreach (var billFlowNode in billFlowNodes)
            {
                if (billFlowNode.Id == 0) // 添加
                {
                    // 验证是否能添加流程节点
                    if (billFlowNode.Group < bf.BillFlowNode.Where(w => w.IsCurrentState).First().Group)
                        return Ok(new { code = StatusCodes.Status400BadRequest, message = "当前审批节点前面不能添加节点" });

                    // 同一个group中审核人不能重复
                    // 获取同一个节点
                    var sameNode = bf.BillFlowNode.Where(w => w.Group == billFlowNode.Group);
                    if (sameNode.Any())
                    {
                        if (sameNode.Any(a => a.PersonId.Equals(billFlowNode.PersonId)))
                            return Ok(new { code = StatusCodes.Status400BadRequest, message = "同一个节点组中审核人不能重复" });
                    }
                    // 如果是当前审批状态，就添加接收日期为当前日期
                    if (billFlowNode.IsCurrentState) 
                        billFlowNode.ReceiveDate = DateTime.Now;
                    billFlowNode.CreateUserId = GetUserId();
                    billFlowNode.CreateUserName = GetUserName();
                    billFlowNode.CreateDate = DateTime.Now;
                    _dbContext.Add(billFlowNode);

                    addPersonIdList.Add(billFlowNode.PersonId);
                }
            }

            int companyId = _systemService.GetCurrentSelectedCompanyId();

            var addInfo = "";
            if (addPersonIdList.Count > 0)
            {
                // 获取添加审核用户的名称
                var userNameAddArray = _commonService.GetCachePersonBasicInfoList(companyId)
                    .Where(w => addPersonIdList.Contains(w.Id)).Select(s => s.Name).ToArray();
                addInfo = $"新增了审核人-【{string.Join(',', userNameAddArray)}】。";
            }

            var delInfo = "";
            var delBillFlowNode = bf.BillFlowNode.Where(w => !billFlowNodes.Any(a => a.Id > 0 && a.Id == w.Id));
            if (delBillFlowNode.Any())
            {
                //验证是否可以删除
                if (delBillFlowNode.Any(a => a.NodeType == 0))
                    return Ok(new { code = StatusCodes.Status400BadRequest, message = "自动生成的节点不能删除哦！" });

                if (delBillFlowNode.Any(a => a.IsChecked))
                    return Ok(new { code = StatusCodes.Status400BadRequest, message = "已经审批过的不能删除哦！" });

                if (delBillFlowNode.Any(a => a.CreateUserId != GetUserId()))
                    return Ok(new { code = StatusCodes.Status400BadRequest, message = "有节点你不是创建人，无法删除哦！" });

                // 删除
                _dbContext.RemoveRange(delBillFlowNode);

                // 获取删除审核用户的名称
                var delPersonIdList = delBillFlowNode.Select(s => s.PersonId);
                var userNameDelArray = _commonService.GetCachePersonBasicInfoList(companyId)
                    .Where(w => delPersonIdList.Contains(w.Id)).Select(s => s.Name).ToArray();
                delInfo = $"删除了审核人-【{string.Join(',', userNameDelArray)}】。";
            }

            // 添加审批信息
            if (addInfo != "" || delInfo != "")
            {
                var attitude = new Attitude();
                attitude.FormId = bf.FormId;
                attitude.BillId = bf.BillId;
                attitude.Type = 1;
                attitude.Title = "编辑流程人";
                attitude.Content = addInfo + delInfo;
                attitude.Operation = "修改流程节点";
                attitude.ReceiveDate = DateTime.Now;
                attitude.CreateUserId = GetUserId();
                attitude.CreateUserName = GetUserName();
                attitude.CreateDate = DateTime.Now;
                attitude.LastEditUserId = GetUserId();
                attitude.LastEditUserName = GetUserName();
                attitude.LastEditDate = DateTime.Now;
                _dbContext.Add(attitude);
            }

            if (_dbContext.SaveChanges() > 0)
            {
                var result = _billFlowService.GetBillFlowById(billFlowNodes.First().BillFlowId);
                return Ok(new { code = StatusCodes.Status200OK, data = result, message = "保存成功" });
            }
            else
                return Ok(new { code = StatusCodes.Status400BadRequest, message = "保存失败" });
        }
        #endregion

        #region 流程审批 Attitude

        [HttpPost, Route("AddAttitude")]
        //[RequirePermission]
        public IActionResult AddAttitude(Attitude attitude)
        {
            // 简单验证
            if (string.IsNullOrWhiteSpace(attitude.Content))
                return Ok(new { code = StatusCodes.Status400BadRequest, message = "审批内容不能为空" });

            using var transaction = _dbContext.Database.BeginTransaction();
            try
            {


                // 获取当前节点中，我审批的项
                var billFlowNodes = _billFlowService.GetBillFlowNodeByBillId(attitude.BillId, attitude.FormId);
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
                attitude.CreateUserId = GetUserId();
                attitude.CreateUserName = GetUserName();
                attitude.LastEditUserId = GetUserId();
                attitude.LastEditUserName = GetUserName();

                if (attitude.AttitudeType != (int)AttitudeTypeEnum.退回&& attitude.AttitudeType != (int)AttitudeTypeEnum.撤销)
                {
                    // 审批类型不是 退回和 撤销 ， 操作动作说明，由后端拼接字符串
                    var attitudeTypeList = _systemService.GetDictionary("attitudeType");
                    attitude.Operation = attitudeTypeList.FirstOrDefault(f => f.Value == attitude.AttitudeType).Name;
                }

                int billFlowState = -1000;
                if (attitude.AttitudeType != (int)AttitudeTypeEnum.只填写意见不转下一步) // 2：只填写意见不转下一步 的话，就跳过
                {
                    // 1.******执行审批********
                    var result = _billFlowService.BillFlowNextAction(attitude.AttitudeType, attitude.BillId, attitude.FormId, attitude.BackGroup);
                    if (!result.isOk)
                    {
                        // 执行审批出现问题，就返回
                        transaction.Rollback();
                        return Ok(new { code = StatusCodes.Status400BadRequest, message = result.msg });
                    }
                    billFlowState = result.state;
                }
                // 2.保存审批意见
                _dbContext.Attitude.Add(attitude);

                // 只填写意见不转下一步 需要更新流程
                List<Attitude> attitudeList = null;

                if (attitude.AttitudeType == (int)AttitudeTypeEnum.只填写意见不转下一步) // 如果类型是 不转下一步
                {
                    // 修改单据状态
                    var formObject = _dbContext.Form.FirstOrDefault(f => f.Id.Equals(attitude.FormId));
                    _dbContext.Database.ExecuteSqlRaw("update " + formObject.Value + $" set lastEditUserId={GetUserId()},LastEditUserName='{GetUserName()}',LastEditDate='{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}' where id={attitude.BillId}");
                    attitudeList = _billFlowService.GetAttitudeByBillId(attitude.BillId, attitude.FormId);
                }
                    
                
                // 保存
                if (_dbContext.SaveChanges() > 0)
                {
                    // 添加文件
                    _systemService.AddFiles<Attitude>(attitude.FileList, attitude.Id);
                    transaction.Commit();
                    return Ok(new
                    {
                        code = StatusCodes.Status200OK,
                        attitudeList = attitudeList, // 流程审批列表
                        billFlowState = billFlowState,
                        message = "保存成功"
                    });
                }
                else
                {
                    transaction.Rollback();
                    return Ok(new { code = StatusCodes.Status400BadRequest, message = "保存失败" });
                }
                    
            }
            catch (Exception)
            {
                return Ok(new { code = StatusCodes.Status400BadRequest, message = "保存失败" });
            }


        }
        /// <summary>
        /// 初始化流程信息
        /// </summary>
        /// <param name="formId">表单Id</param>
        /// <param name="billFlowId">流程id</param>
        /// <param name="billId">单据id</param>
        /// <param name="toBeforeState">是否允许退到当前状态之前</param>
        /// <returns></returns>
        [HttpGet, Route("InitAttitude")]
        public IActionResult InitAttitude(bool toBeforeState, int formId=0, int billId=0, int billFlowId=0)
        {
            if (formId==0 || billId == 0 || billFlowId == 0)
                return Ok(new { code = StatusCodes.Status400BadRequest, message = "参数错误" });
            // 1.获取审批数据
            //int formId = _dbContext.BillFlow.Find(billFlowId.Value).FormId;
            var attitudeData = _billFlowService.GetAttitudeByBillId(billId, formId);
            var listIds = attitudeData.Select(s => s.Id);
            // 处理附件
            var fileList = _dbContext.UpFile
                    .Where(w => listIds.Contains(w.TableId) && w.TableName.Equals(Utils.Common.GetTableName<Attitude>()))
                    .AsNoTracking().ToList();
            foreach (var attitude in attitudeData)
            {
                attitude.UpFiles = fileList.Where(w => w.TableId.Equals(attitude.Id)).ToList();
            }

            // 退回 下拉框列表 toBeforeState：true 退回所有列表。| false 退回所在当前状态的列表
            var beforeBillFlowNodeList = _billFlowService.GetBillFlowNodeForBack(billFlowId, toBeforeState);

            var bf = _billFlowService.GetBillFlowById(billFlowId);
            (bool isOk, string msg, int code) checkState = _billFlowService.CheckBillFlowNodeIsChecked(billFlowId,bf);
            
            var isRevoke = _billFlowService.CheckBillFlowRevoke(bf); //判断是否可以撤回
            var attitudeTypeList = _systemService.GetDictionary("attitudeType")
                .OrderBy(o=>o.Sort)
                .Select(s => new { s.Name, s.Value });
            return Ok(new
            {
                code = StatusCodes.Status200OK,
                data = attitudeData,
                beforeBillFlowNodeList = beforeBillFlowNodeList, // 流程数据
                checkState = checkState,
                attitudeTypeList = attitudeTypeList,
                isRevoke = isRevoke // 是否可以撤回
            });
        }

        #endregion

        #region 定时任务 接口
        /// <summary>
        /// 【任务接口】远程调用的自动审核接口
        /// </summary>
        /// <returns></returns>
        [HttpGet, Route("BillFlowAutoChecked")]
        [AllowAnonymous]
        public IActionResult BillFlowAutoChecked()
        {
            var billFlowList = _dbContext.BillFlow.Include(i => i.BillFlowNode).Where(w => w.Id == 126).AsNoTracking().ToList();

            StringBuilder sb = new StringBuilder();
            foreach (var bf in billFlowList)
            {
                try
                {
                    var checkResult = _billFlowService.CheckThisBillFlow(bf);
                    if (!checkResult.isOk)
                    {
                  
                        sb.Append($"编号为：【{bf.BillNumber}】单据检查错误：{checkResult.msg}；");
                        // 记录错误消息
                        continue;
                    }
                    var result = _billFlowService.BillFlowAutoFinishAction(bf);
                    if (!result.isOk)
                    {
                        sb.Append($"编号为：【{bf.BillNumber}】自动审批错误：{result.msg}");
                        return Ok(new { code = StatusCodes.Status400BadRequest, isSuccess = "false", message = sb.ToString()  });
                        // 记录错误信息
                    }
                }
                catch (Exception ex)
                {
                    // 记录异常信息
                    return Ok(new { code = StatusCodes.Status400BadRequest,isSuccess="false", exception=ex.ToString(), message= sb.ToString() });
                }
            }
            return Ok(new { code = StatusCodes.Status200OK, data = "运行成功！" });
        }
        #endregion
    }
}
