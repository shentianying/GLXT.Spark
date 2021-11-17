using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using GLXT.Spark.Entity;
using GLXT.Spark.Entity.XTGL;
using GLXT.Spark.Filters;
using GLXT.Spark.IService;
using GLXT.Spark.Utils;

namespace GLXT.Spark.Controllers.XTGL
{
    /// <summary>
    /// 流程设计
    /// </summary>
    [Route("api/XTGL/[controller]")]
    [ApiController]
    [Authorize]
    public class FlowController : BaseController
    {
        private readonly DBContext _dbContext;
        private readonly ICommonService _commonService;
        private readonly IBillFlowService _billFlowService;
        private readonly ISystemService _systemService;
        public FlowController(DBContext dbContext, ICommonService commonService,IBillFlowService billFlowService,
ISystemService systemService)
        {
            _commonService = commonService;
            _billFlowService = billFlowService;
            _dbContext = dbContext;
            _systemService = systemService;
        }

        #region Flow 主表
        [HttpGet, Route("GetFlowPaging")]
        public IActionResult GetFlowPaging(int currentPage, int pageSize, int? formId, string name = "")
        {
            var companyId = _systemService.GetCurrentSelectedCompanyId();
            var query = _dbContext.Flow
                .OrderBy(o => o.Sort)
                .Include(i => i.FlowNode)
                //.ThenInclude(t=>t.Role)
                .Where(w => w.InUse && w.CompanyId.Equals(companyId));

            if (!string.IsNullOrEmpty(name))
            {
                query = query.Where(w => w.Name.Contains(name));
            }
            if (formId.HasValue)
            {
                query = query.Where(w => w.FormId.Equals(formId.Value));
            }
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
            var roleList = _systemService.GetRole(null).Select(s => new { s.Id, s.Name });
            var roleType = _systemService.GetDictionary("flowRoleType");
            return Ok(new { code = StatusCodes.Status200OK, data = result, roleList = roleList, roleType = roleType, count = count });
        }
        [HttpGet, Route("GetFlowList")]
        [RequirePermission]
        public IActionResult GetFlowList(int? formId, string name = "")
        {
            var companyId = _systemService.GetCurrentSelectedCompanyId();
            var query = _dbContext.Flow
                .OrderBy(o => o.Sort)
                .Include(i => i.FlowNode)
                .Where(w => w.InUse && w.CompanyId.Equals(companyId));
            if (!string.IsNullOrEmpty(name))
                query = query.Where(w => w.Name.Contains(name));
            if (formId.HasValue)
            {
                query = query.Where(w => w.FormId.Equals(formId.Value));
            }
            var result = query.AsNoTracking().ToList();
            var roleList = _systemService.GetRole(null).Select(s => new { s.Id, s.Name });
            var dictionary = _systemService.GetDictionary(); // 获取字典数据
            var roleType = dictionary.Where(w=>w.Type.Equals("flowRoleType"));
            var fieldType = _billFlowService.GetFieldTypeList();
            var flowNodeMode = dictionary.Where(w => w.Type.Equals("flowNodeMode"));
            
            var organization = _systemService.GetOrgWithChildren(companyId);
            var grade = dictionary.Where(w => w.Type.Equals("flowNodeGrade"));
            List<FormState> fsList = null;
            List<FormStateOption> options = null;
            if(formId.HasValue)
            {
                options = _dbContext.FormStateOption.Where(w => w.InUse && w.FormId.Equals(formId)).ToList(); // 操作类型
                fsList = _dbContext.FormState.Where(w => w.InUse && w.FormId.Equals(formId)).OrderBy(o=>o.Value).ToList();
            }
            
            IQueryable formfield = null;
            if (formId.HasValue)
            {
                formfield = _dbContext.FormFlowField
                    .Where(w => w.InUse && w.FormId.Equals(formId))
                    .Select(s => new
                    {
                        s.Id,
                        s.FormId,
                        s.Field,
                        s.FieldName,
                        s.FieldType,
                        s.DicType
                    });
            }
            return Ok(
                new
                {
                    code = StatusCodes.Status200OK,
                    data = result,
                    roleList = roleList,
                    roleType = roleType,
                    formState = fsList, // 表单状态
                    options = options, // 操作类型
                    flowNodeMode = flowNodeMode,
                    formfield = formfield,
                    fieldType = fieldType,
                    dictionary = dictionary,
                    organization = organization,
                    grade = grade
                });
        }

        [HttpPost, Route("AddFlow")]
        [RequirePermission]
        public IActionResult AddFlow(Flow flow)
        {
            if(_dbContext.Flow.Any(w => w.Name.Equals(flow.Name)&& w.FormId==flow.FormId))
            {
                return Ok(new { code = StatusCodes.Status400BadRequest, message = "流程名称不能重复" });
            }
            var maxSort = _dbContext.Flow.Where(w => w.InUse).Select(s=>s.Sort);
            double mSort = 1;
            if (maxSort.Any())
                mSort = maxSort.Max()+1;
            
            flow.CompanyId = _systemService.GetCurrentSelectedCompanyId();
            flow.CreateUserId = GetUserId();
            flow.CreateUserName = GetUserName();
            flow.LastEditUserId = GetUserId();
            flow.LastEditUserName = GetUserName();
            flow.Sort = mSort; 
            _dbContext.Add(flow);
            if (_dbContext.SaveChanges() > 0)
            {
                return Ok(new { code = StatusCodes.Status200OK, message = "添加成功" });
            }
            else
                return Ok(new { code = StatusCodes.Status400BadRequest, message = "添加失败" });
        }
        [HttpPut, Route("PutFlow")]
        [RequirePermission]
        public IActionResult PutFlow(Flow flow)
        {
            var query = _dbContext.Flow.Find(flow.Id);
            if (_dbContext.Flow.Any(w => w.Name.Equals(flow.Name)&&w.Id!=flow.Id&&w.FormId==query.FormId))
            {
                return Ok(new { code = StatusCodes.Status400BadRequest, message = "单据中的流程名称不能重复" });
            }
            // FlowNode 如果是空的，就是删除
            var flowNodeList = _dbContext.FlowNode.Where(w => w.FlowId.Equals(flow.Id)).AsNoTracking().ToList();
            if (!flow.FlowNode.Any())
                _dbContext.RemoveRange(flowNodeList);
            else
            {
                // 刷选出 保存的节点 （编辑 或者 删除）
                var saveQuery = flow.FlowNode.Where(w => w.Id > 0);
                // 刷选出添加的流程节点
                var addQuery = flow.FlowNode.Where(w => w.Id == 0);
                if (addQuery.Any())
                    _dbContext.AddRange(addQuery);
                var delQuery = flowNodeList.Where(w => !saveQuery.Any(a => a.Id == w.Id));
                if(delQuery.Any())
                    _dbContext.RemoveRange(delQuery);

                var editQuery = flowNodeList.Where(w => saveQuery.Any(a => a.Id == w.Id));
                foreach (var item in editQuery)
                {
                    var q = saveQuery.FirstOrDefault(f => f.Id == item.Id);
                    item.FlowId = flow.Id;
                    item.Group = q.Group;
                    item.State = q.State;
                    item.MaxDays = q.MaxDays;
                    item.Grade = q.Grade;
                    item.Option = q.Option;
                    item.Mode = q.Mode;
                    item.RoleId = q.RoleId;
                    item.RoleType = q.RoleType;
                    _dbContext.Update(item);
                }
            }
            query.Name = flow.Name;
            query.Remark = flow.Remark;
            query.LastEditUserId = GetUserId();
            query.LastEditUserName = GetUserName();
            query.LastEditDate = DateTime.Now;
            _dbContext.Update(query);
            if (_dbContext.SaveChanges() > 0)
            {
                return Ok(new { code = StatusCodes.Status200OK, message = "操作成功" });
            }
            else
                return Ok(new { code = StatusCodes.Status400BadRequest, message = "操作失败" });
        }
        
        /// <summary>
        /// 上移
        /// </summary>
        /// <param name="flow"></param>
        /// <returns></returns>
        [HttpPut, Route("PutFlowPrev")]
        public IActionResult PutFlowPrev(Flow flow)
        {
            if (flow != null)
            {
                var q1 = _dbContext.Flow.Find(flow.Id);
                var q2 = _dbContext.Flow
                    .OrderByDescending(f => f.Sort)
                    .FirstOrDefault(w => w.Sort < q1.Sort && w.FormId.Equals(flow.FormId) && w.CompanyId.Equals(flow.CompanyId));
                if (q2 == null)
                    return Ok(new { code = StatusCodes.Status400BadRequest, message = "已经是最上面一条啦" });
                var q1Srot = q1.Sort;
                var q2Srot = q2.Sort;
                // 分别设置对象
                q1.Sort = q2Srot;
                q2.Sort = q1Srot;
                // 修改
                _dbContext.Update(q1);
                _dbContext.Update(q2);
                // 保存
                if (_dbContext.SaveChanges() > 0)
                {
                    return Ok(new { code = StatusCodes.Status200OK, message = "保存成功" });
                }
                else
                    return Ok(new { code = StatusCodes.Status400BadRequest, message = "保存失败" });
            }
            else
                return Ok(new { code = StatusCodes.Status400BadRequest, message = "参数错误" });
        }
        /// <summary>
        /// 下移
        /// </summary>
        /// <param name="flow"></param>
        /// <returns></returns>
        [HttpPut, Route("PutFlowNext")]
        public IActionResult PutFlowNext(Flow flow)
        {
            if (flow != null)
            {
                var q1 = _dbContext.Flow.Find(flow.Id);
                var q2 = _dbContext.Flow.OrderBy(f => f.Sort).FirstOrDefault(w => w.Sort > q1.Sort && w.FormId.Equals(flow.FormId) && w.CompanyId.Equals(flow.CompanyId));
                if (q2 == null)
                    return Ok(new { code = StatusCodes.Status400BadRequest, message = "已经在最下面了" });
                var q1Srot = q1.Sort;
                var q2Srot = q2.Sort;
                // 分别设置对象
                q1.Sort = q2Srot;
                q2.Sort = q1Srot;
                // 修改
                _dbContext.Update(q1);
                _dbContext.Update(q2);
                // 保存
                if (_dbContext.SaveChanges() > 0)
                {
                    return Ok(new { code = StatusCodes.Status200OK, message = "保存成功" });
                }
                else
                    return Ok(new { code = StatusCodes.Status400BadRequest, message = "保存失败" });
            }
            else
                return Ok(new { code = StatusCodes.Status400BadRequest, message = "参数错误" });
        }
        /// <summary>
        /// 置末
        /// </summary>
        /// <param name="flow"></param>
        /// <returns></returns>
        [HttpPut, Route("PutFlowBottom")]
        public IActionResult PutFlowBottom(Flow flow)
        {
            if (flow != null)
            {
                // 最后一个数据的值
                var q1 = _dbContext.Flow.OrderByDescending(s => s.Sort).FirstOrDefault(w => w.FormId.Equals(flow.FormId) && w.CompanyId.Equals(flow.CompanyId));
                // 获得当前流程对象
                var q2 = _dbContext.Flow.Find(flow.Id);
                if (q1.Id.Equals(q2.Id))
                    return Ok(new { code = StatusCodes.Status400BadRequest, message = "已经在最下面了" });

                q2.Sort = q1.Sort + 2;
                _dbContext.Update(q2);
                if (_dbContext.SaveChanges() > 0)
                {
                    return Ok(new { code = StatusCodes.Status200OK, message = "保存成功" });
                }
                else
                    return Ok(new { code = StatusCodes.Status400BadRequest, message = "保存失败" });
            }
            else
                return Ok(new { code = StatusCodes.Status400BadRequest, message = "参数错误" });

        }
        /// <summary>
        /// 置顶
        /// </summary>
        /// <param name="flow"></param>
        /// <returns></returns>
        [HttpPut, Route("PutFlowTop")]
        public IActionResult PutFlowTop(Flow flow)
        {
            if (flow != null)
            {
                // 第一个数据的值
                var q1 = _dbContext.Flow.OrderBy(s => s.Sort).FirstOrDefault(w => w.FormId.Equals(flow.FormId) && w.CompanyId.Equals(flow.CompanyId));
                // 获得当前流程对象
                var q2 = _dbContext.Flow.Find(flow.Id);
                // 如果是同一个对象
                if (q1.Id.Equals(q2.Id))
                    return Ok(new { code = StatusCodes.Status400BadRequest, message = "已经是最上面一条啦" });
                q2.Sort = q1.Sort / 2;
                _dbContext.Update(q2);
                if (_dbContext.SaveChanges() > 0)
                {
                    return Ok(new { code = StatusCodes.Status200OK, message = "保存成功" });
                }
                else
                    return Ok(new { code = StatusCodes.Status400BadRequest, message = "保存失败" });
            }
            else
                return Ok(new { code = StatusCodes.Status400BadRequest, message = "参数错误" });
        }
        #endregion

        #region FlowCondition 流程条件

        /// <summary>
        /// 返回字段类型及可用操作符
        /// </summary>
        /// <returns></returns>
        [HttpGet, Route("GetFieldTypeList")]
        public IActionResult GetFieldTypeList()
        {
            return Ok(new { code = StatusCodes.Status200OK, data = _billFlowService.GetFieldTypeList() });
        }
        [HttpGet, Route("GetFlowCondition")]
        [RequirePermission]
        public IActionResult GetFlowCondition(int? flowId)
        {
            if(flowId.HasValue)
            {
                var query = _dbContext.FlowCondition
                .Where(w => w.FlowId.Equals(flowId))
                //.Include(i => i.FormFlowField)
                .OrderBy(o => o.Id)
                //.Select(
                //    s=>new {
                //        s.Id,
                //        s.FlowId,
                //        s.Code,
                //        s.PCode,
                //        s.Logic,
                //        s.FormFlowFieldId,
                //        s.FormFlowField,
                //        s.Operator,
                //        s.Value,
                //        s.IsLeaf
                //    })
                .AsNoTracking();
                return Ok(new { code = StatusCodes.Status200OK, data = query.ToList() });
            }
            else
                return Ok(new { code = StatusCodes.Status400BadRequest, message = "参数错误" });
            
        }
        [HttpPost, Route("AddFlowCondition")]
        [RequirePermission]
        public IActionResult AddFlowCondition(Flow flow)
        {
            var q1 = _dbContext.FlowCondition.Where(w => w.FlowId.Equals(flow.Id)).AsNoTracking();
            bool flag1 = false;
            bool flag2 = false;
            if (q1.Any())
            {
                _dbContext.RemoveRange(q1);
                flag1 = true;
            }
            
            // 如果包含条件叶子项
            if(flow.FlowCondition.Any(w=>w.IsLeaf))
            {
                _dbContext.FlowCondition.AddRange(flow.FlowCondition);
                var q2 = _dbContext.Flow.Find(flow.Id);
                q2.ConditionDescription = flow.ConditionDescription;
                _dbContext.Update(q2);
                flag2 = true;
            }
            if (flag1 || flag2)
            {
                if (_dbContext.SaveChanges() > 0)
                {
                    return Ok(new { code = StatusCodes.Status200OK, message = "保存成功" });
                }
                else
                    return Ok(new { code = StatusCodes.Status400BadRequest, message = "保存失败" });
            }else
                return Ok(new { code = StatusCodes.Status400BadRequest, message = "没有更新项" });
        }

        #endregion

        #region flowNode 流程节点
        [HttpGet, Route("GetFlowNode")]
        public IActionResult GetFlowNode(int? FlowId)
        {
            var query = _dbContext.FlowNode
                .Where(w => w.FlowId.Equals(FlowId))
                .OrderBy(o => o.Id).AsNoTracking();
            return Ok(new { code = StatusCodes.Status200OK, data = query.ToList() });
        }

        [HttpPost, Route("AddFlowNode")]
        //[RequirePermission]
        public IActionResult AddFlowNode(FlowNode FlowNode)
        {
            _dbContext.Add(FlowNode);
            _dbContext.SaveChanges();
            var query = _dbContext.FlowNode.Where(w => w.FlowId.Equals(FlowNode.FlowId));
            return Ok(new { code = StatusCodes.Status200OK, data = query.ToList(), message = "添加成功" });
            //if (_dbContext.SaveChanges() > 0)
            //{
            //    return Ok(new { code = StatusCodes.Status200OK, message = "添加成功" });
            //}
            //else
            //    return Ok(new { code = StatusCodes.Status400BadRequest, message = "添加失败" });
        }
        #endregion
    }
}
