using GLXT.Spark.Entity;
using GLXT.Spark.Entity.QYGL;
using GLXT.Spark.IService;
using GLXT.Spark.ViewModel.QYGL;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GLXT.Spark.Controllers.QYGL
{
    /// <summary>
    /// 访客管理
    /// </summary>
    [Route("api/QYGL/[controller]")]
    [ApiController]
    [Authorize]
    public class VisitorController : BaseController
    {
        private readonly DBContext _dbContext;
        private readonly ICommonService _commonService;
        private readonly ISystemService _systemService;
        public VisitorController(DBContext dbContext, ICommonService commonService, ISystemService systemService)
        {
            _dbContext = dbContext;
            _commonService = commonService;
            _systemService = systemService;
        }

        /// <summary>
        /// 获取分页列表
        /// </summary>
        /// <returns></returns>
        [HttpPost, Route("GetVisitorPaging")]
        public IActionResult GetVisitorPaging(VisitorSearchViewModel svm)
        {
            int companyId = _systemService.GetCurrentSelectedCompanyId();
            IQueryable<Visitor> query = _dbContext.Visitor
                .Where(w => w.CompanyId.Equals(companyId));

            if (!string.IsNullOrEmpty(svm.name))
                query = query.Where(w => w.Name.Contains(svm.name));

            if (!string.IsNullOrEmpty(svm.tel))
                query = query.Where(w => w.Telphone.Contains(svm.tel));

            if (!string.IsNullOrEmpty(svm.carNum))
                query = query.Where(w => w.CarNum.Contains(svm.carNum));

            if (!string.IsNullOrEmpty(svm.receivor))
                query = query.Where(w => w.ReceivePerson.Contains(svm.receivor));

            if (svm.date1.HasValue)
                query = query.Where(w => w.VisitTime >= svm.date1);

            if (svm.date2.HasValue)
                query = query.Where(w => w.VisitTime <= svm.date2);

            // 分页
            if (svm.currentPage == 0 || svm.pageSize == 0)
            {
                return Ok(new { code = StatusCodes.Status400BadRequest, errorMsg = "页码与页数数值需正确！" });
            }
            else
            {
                int count = query.Count();
                var query_result = query.Skip((svm.currentPage - 1) * svm.pageSize)
                    .Take(svm.pageSize);
                //判断是否有数据，若无则返回第一页
                if (query_result.Count() == 0)
                {
                    svm.currentPage = 1;
                    query_result = query.Skip((svm.currentPage - 1) * svm.pageSize)
                        .Take(svm.pageSize);
                }

                List<object> result = new List<object>();
                foreach (var q in query_result)
                {
                    result.Add(new
                    {
                        q.Id,
                        q.Name,
                        q.Telphone,
                        q.OrderTime,
                        q.OrderSuccessTime,
                        q.VisitTime,
                        q.LeaveTime,
                        q.OrgId,
                        q.RegionId,
                        q.ReceivePerson,
                        q.ReceiveTel,
                        q.VisitReason,
                        q.PeerNum,
                        q.CreateUserName,
                        q.CreateDate,
                        q.LastEditUserName,
                        q.LastEditDate
                    });
                }


                return Ok(new
                {
                    code = StatusCodes.Status200OK,
                    data = result,
                    count = count
                });
            }
        }

        /// <summary>
        /// 编辑页面 根据id获取意向企业信息
        /// </summary>
        /// <param name="id">意向企业id</param>
        /// <returns></returns>
        [HttpGet, Route("GetVisitorById")]
        //[RequirePermission]
        public IActionResult GetVisitorById(int id)
        {
            var visitor = _dbContext.Visitor
                  .FirstOrDefault(w => w.Id.Equals(id));

            if (visitor == null)
            {
                return Ok(new { code = StatusCodes.Status400BadRequest, message = "数据为空" });
            }

            return Ok(new
            {
                code = StatusCodes.Status200OK,
                data = visitor
            });
        }

        /// <summary>
        /// 添加意向企业
        /// </summary>
        /// <param name="visitor">对象</param>
        /// <returns></returns>
        [HttpPost, Route("AddVisitor")]
        //[RequirePermission]
        public IActionResult AddVisitor(Visitor visitor)
        {
            visitor.CreateUserId = GetUserId();
            visitor.CreateUserName = GetUserName();
            visitor.LastEditUserId = GetUserId();
            visitor.LastEditUserName = GetUserName();
            _dbContext.Add(visitor);
            if (_dbContext.SaveChanges() > 0)
                return Ok(new { code = StatusCodes.Status200OK, message = "操作成功", data = visitor });

            else
                return Ok(new { code = StatusCodes.Status400BadRequest, message = "添加失败" });
        }

        /// <summary>
        /// 更新
        /// </summary>
        /// <param name="visitor"></param>
        /// <returns></returns>
        [HttpPut, Route("PutVisitor")]
        public IActionResult PutVisitor(Visitor visitor)
        {

            var query1 = _dbContext.Visitor.Find(visitor.Id);

            if (query1 != null)
            {
                query1.Name = visitor.Name;
                query1.Telphone = visitor.Telphone;
                query1.OrderTime = visitor.OrderTime;
                query1.OrderSuccessTime = visitor.OrderSuccessTime;
                query1.VisitTime = visitor.VisitTime;
                query1.LeaveTime = visitor.LeaveTime;
                query1.OrgId = visitor.OrgId;
                query1.RegionId = visitor.RegionId;
                query1.ReceivePerson = visitor.ReceivePerson;
                query1.ReceiveTel = visitor.ReceiveTel;
                query1.CarNum = visitor.CarNum;
                query1.VisitReason = visitor.VisitReason;
                query1.PeerNum = visitor.PeerNum;
                query1.LastEditUserId = GetUserId();
                query1.LastEditUserName = GetUserName();
                query1.LastEditDate = DateTime.Now;

                _dbContext.Update(query1);
                if (_dbContext.SaveChanges() > 0)
                    return Ok(new { code = StatusCodes.Status200OK, message = "操作成功", data = visitor });
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
        /// <param name="id">意向企业id</param>
        /// <returns></returns>
        [HttpDelete, Route("DeleteVisitor")]
        public IActionResult DeleteVisitor(int? id)
        {
            if (id.HasValue)
            {
                var q1 = _dbContext.Visitor
                    .FirstOrDefault(w => w.Id.Equals(id));
                _dbContext.Remove(q1);
                if (_dbContext.SaveChanges() > 0)
                    return Ok(new { code = StatusCodes.Status200OK, message = "操作成功" });
                else
                    return Ok(new { code = StatusCodes.Status400BadRequest, message = "操作失败" });
            }
            else
                return Ok(new { code = StatusCodes.Status400BadRequest, message = "参数错误" });
        }
    }
}
