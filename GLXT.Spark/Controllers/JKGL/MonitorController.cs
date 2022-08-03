using GLXT.Spark.Entity;
using GLXT.Spark.Entity.JKGL;
using GLXT.Spark.IService;
using GLXT.Spark.ViewModel.JKGL;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GLXT.Spark.Controllers.JKGL
{
    /// <summary>
    /// 监控管理
    /// </summary>
    [Route("api/JKGL/[controller]")]
    [ApiController]
    [Authorize]
    public class MonitorController : BaseController
    {
        private readonly DBContext _dbContext;
        private readonly ICommonService _commonService;
        private readonly ISystemService _systemService;
        public MonitorController(DBContext dbContext, ICommonService commonService, ISystemService systemService)
        {
            _dbContext = dbContext;
            _commonService = commonService;
            _systemService = systemService;
        }

        /// <summary>
        /// 获取分页列表
        /// </summary>
        /// <returns></returns>
        [HttpPost, Route("GetMonitorPaging")]
        public IActionResult GetMonitorPaging(MonitorSearchViewModel msvm)
        {
            int companyId = _systemService.GetCurrentSelectedCompanyId();
            IQueryable<Monitor> query = _dbContext.Monitor
                .Where(w => w.IsMonitor && w.CompanyId.Equals(companyId));

            if (!string.IsNullOrEmpty(msvm.name))
                query = query.Where(w => w.Name.Contains(msvm.name));

            if (!string.IsNullOrEmpty(msvm.ipAddress))
                query = query.Where(w => w.IPAddress.Contains(msvm.ipAddress));

            // 分页
            if (msvm.currentPage == 0 || msvm.pageSize == 0)
            {
                return Ok(new { code = StatusCodes.Status400BadRequest, errorMsg = "页码与页数数值需正确！" });
            }
            else
            {
                int count = query.Count();
                var query_result = query.Skip((msvm.currentPage - 1) * msvm.pageSize)
                    .Take(msvm.pageSize);
                //判断是否有数据，若无则返回第一页
                if (query_result.Count() == 0)
                {
                    msvm.currentPage = 1;
                    query_result = query.Skip((msvm.currentPage - 1) * msvm.pageSize)
                        .Take(msvm.pageSize);
                }

                //List<object> result = new List<object>();
                //foreach (var q in query_result)
                //{
                //    result.Add(new
                //    {
                //        q.Id,
                //        q.Number,
                //        q.ProjectId,
                //        orgName = q.Organization.Name,
                //        projectName = q.Project.Name,
                //        projectState = projectStateList.FirstOrDefault(t => t.Value.Equals(q.Project.StateID))?.Name,
                //        capitalType = capitalTypeList.FirstOrDefault(t => t.Value.Equals(q.CapitalType))?.Name,
                //        gatheringType = gatheringTypeList.FirstOrDefault(t => t.Value.Equals(q.GatheringType))?.Name,
                //        personName = q.Person.Name,
                //        q.GatheringDate,
                //        q.State,
                //        q.GatheringAmount,
                //        q.Remark,
                //        q.CreateUserName,
                //        q.CreateDate,
                //        q.LastEditUserName,
                //        q.LastEditDate
                //    });
                //}


                return Ok(new
                {
                    code = StatusCodes.Status200OK,
                    data = query_result,
                    count = count
                });
            }
        }

        /// <summary>
        /// 监控列表
        /// </summary>
        /// <returns></returns>
        [HttpGet, Route("GetMonitorList")]
        //[RequirePermission]
        public IActionResult GetMonitorList()
        {
            int companyId = _systemService.GetCurrentSelectedCompanyId();
            IQueryable<Monitor> query = _dbContext.Monitor
                .Where(w => w.InUse && w.CompanyId.Equals(companyId));

            int count = query.Count();

            return Ok(new
            {
                code = StatusCodes.Status200OK,
                data = query,
                count = count
            });
        }

        /// <summary>
        /// 监控节点列表
        /// </summary>
        /// <returns></returns>
        [HttpGet, Route("getAllMonitorNode")]
        //[RequirePermission]
        public IActionResult getAllMonitorNode()
        {
            int companyId = _systemService.GetCurrentSelectedCompanyId();
            IQueryable<Monitor> query = _dbContext.Monitor
                .Where(w => w.InUse && !w.IsMonitor && w.CompanyId.Equals(companyId));

            int count = query.Count();

            return Ok(new
            {
                code = StatusCodes.Status200OK,
                data = query,
                count = count
            });
        }

        /// <summary>
        /// 编辑页面 根据id获取监控信息
        /// </summary>
        /// <param name="id">监控id</param>
        /// <returns></returns>
        [HttpGet, Route("GetMonitorById")]
        //[RequirePermission]
        public IActionResult GetMonitorById(int id)
        {
            var Monitor = _dbContext.Monitor
                  .FirstOrDefault(w => w.Id.Equals(id));

            if (Monitor == null)
            {
                return Ok(new { code = StatusCodes.Status400BadRequest, message = "数据为空" });
            }            

            return Ok(new
            {
                code = StatusCodes.Status200OK,
                data = Monitor
            });
        }

        /// <summary>
        /// 添加监控
        /// </summary>
        /// <param name="Monitor">对象</param>
        /// <returns></returns>
        [HttpPost, Route("AddMonitor")]
        //[RequirePermission]
        public IActionResult AddMonitor(Monitor Monitor)
        {
            if (_dbContext.Monitor.Where(w => w.IPAddress.Equals(Monitor.IPAddress) && w.Id!=Monitor.Id).Count() > 0)
                return Ok(new { code = StatusCodes.Status400BadRequest, message = "您输入的监控IP地址冲突，请重新设置！" });
            Monitor.CreateUserId = GetUserId();
            Monitor.CreateUserName = GetUserName();
            Monitor.LastEditUserId = GetUserId();
            Monitor.LastEditUserName = GetUserName();
            _dbContext.Add(Monitor);
            if (_dbContext.SaveChanges() > 0)
                return Ok(new { code = StatusCodes.Status200OK, message = "操作成功", data = Monitor });

            else
                return Ok(new { code = StatusCodes.Status400BadRequest, message = "添加失败" });
        }

        /// <summary>
        /// 更新
        /// </summary>
        /// <param name="Monitor"></param>
        /// <returns></returns>
        [HttpPut, Route("PutMonitor")]
        public IActionResult PutMonitor(Monitor Monitor)
        {

            if (_dbContext.Monitor.Where(w => w.IPAddress.Equals(Monitor.IPAddress) && w.Id != Monitor.Id).Count() > 0)
                return Ok(new { code = StatusCodes.Status400BadRequest, message = "您输入的监控IP地址冲突，请重新设置！" });

            var query1 = _dbContext.Monitor.Find(Monitor.Id);

            if (query1 != null)
            {
                query1.Name = Monitor.Name;
                query1.PId = Monitor.PId;
                query1.IPAddress = Monitor.IPAddress;
                query1.LoginName = Monitor.LoginName;
                query1.LoginPassword = Monitor.LoginPassword;
                query1.IsMonitor = Monitor.IsMonitor;
                query1.InUse = Monitor.InUse;
                query1.Remark = Monitor.Remark;
                query1.LastEditUserId = GetUserId();
                query1.LastEditUserName = GetUserName();
                query1.LastEditDate = DateTime.Now;

                _dbContext.Update(query1);
                if (_dbContext.SaveChanges() > 0)
                    return Ok(new { code = StatusCodes.Status200OK, message = "操作成功", data = Monitor });
                else
                    return Ok(new { code = StatusCodes.Status400BadRequest, message = "更新失败" });
            }
            else
            {
                return Ok(new { code = StatusCodes.Status400BadRequest, message = "查无此单据" });
            }
        }

        /// <summary>
        /// 作废
        /// </summary>
        /// <param name="id">监控id</param>
        /// <returns></returns>
        [HttpDelete, Route("DeleteMonitor")]
        public IActionResult DeleteMonitor(int? id)
        {
            if (id.HasValue)
            {
                var q1 = _dbContext.Monitor
                    .FirstOrDefault(w => w.Id.Equals(id));
                q1.InUse = false;
                _dbContext.Update(q1);
                if (_dbContext.SaveChanges() > 0)
                    return Ok(new { code = StatusCodes.Status200OK, message = "操作成功" });
                else
                    return Ok(new { code = StatusCodes.Status400BadRequest, message = "操作失败" });
            }
            else
                return Ok(new { code = StatusCodes.Status400BadRequest, message = "参数错误" });
        }

        /// <summary>
        /// 添加监控节点
        /// </summary>
        /// <param name="Monitor">对象</param>
        /// <returns></returns>
        [HttpPost, Route("AddMonitorNode")]
        //[RequirePermission]
        public IActionResult AddMonitorNode(Monitor Monitor)
        {
            if (_dbContext.Monitor.Where(w => w.Name.Equals(Monitor.Name) && w.Id != Monitor.Id).Count() > 0)
                return Ok(new { code = StatusCodes.Status400BadRequest, message = "您输入节点名称重复，请重新设置！" });
            Monitor.CreateUserId = GetUserId();
            Monitor.CreateUserName = GetUserName();
            Monitor.LastEditUserId = GetUserId();
            Monitor.LastEditUserName = GetUserName();
            _dbContext.Add(Monitor);
            if (_dbContext.SaveChanges() > 0)
                return Ok(new { code = StatusCodes.Status200OK, message = "操作成功", data = Monitor });

            else
                return Ok(new { code = StatusCodes.Status400BadRequest, message = "添加失败" });
        }

        /// <summary>
        /// 更新节点
        /// </summary>
        /// <param name="Monitor"></param>
        /// <returns></returns>
        [HttpPut, Route("PutMonitorNode")]
        public IActionResult PutMonitorNode(Monitor Monitor)
        {

            if (_dbContext.Monitor.Where(w => w.Name.Equals(Monitor.Name) && w.Id != Monitor.Id).Count() > 0)
                return Ok(new { code = StatusCodes.Status400BadRequest, message = "您输入节点名称重复，请重新设置！" });

            var query1 = _dbContext.Monitor.Find(Monitor.Id);

            if (query1 != null)
            {
                query1.Name = Monitor.Name;
                query1.PId = Monitor.PId;
                query1.IPAddress = Monitor.IPAddress;
                query1.LoginName = Monitor.LoginName;
                query1.LoginPassword = Monitor.LoginPassword;
                query1.IsMonitor = Monitor.IsMonitor;
                query1.InUse = Monitor.InUse;
                query1.Remark = Monitor.Remark;
                query1.LastEditUserId = GetUserId();
                query1.LastEditUserName = GetUserName();
                query1.LastEditDate = DateTime.Now;

                _dbContext.Update(query1);
                if (_dbContext.SaveChanges() > 0)
                    return Ok(new { code = StatusCodes.Status200OK, message = "操作成功", data = Monitor });
                else
                    return Ok(new { code = StatusCodes.Status400BadRequest, message = "更新失败" });
            }
            else
            {
                return Ok(new { code = StatusCodes.Status400BadRequest, message = "查无此单据" });
            }
        }
    }
}
