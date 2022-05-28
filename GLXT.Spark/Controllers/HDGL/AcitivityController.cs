using GLXT.Spark.Entity;
using GLXT.Spark.Entity.HDGL;
using GLXT.Spark.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GLXT.Spark.Controllers.HDGL
{
    /// <summary>
    /// 商场活动
    /// </summary>
    [Route("api/HDGL/[controller]")]
    [ApiController]
    [Authorize]
    public class AcitivityController : BaseController
    {
        private readonly DBContext _dbContext;
        private readonly ICommonService _commonService;
        private readonly ISystemService _systemService;
        public AcitivityController(DBContext dbContext, ICommonService commonService, ISystemService systemService)
        {
            _dbContext = dbContext;
            _commonService = commonService;
            _systemService = systemService;
        }

        /// <summary>
        /// 获取分页列表
        /// </summary>
        /// <returns></returns>
        [HttpPost, Route("GetAcitivityPaging")]
        public IActionResult GetAcitivityPaging()
        {
            int companyId = _systemService.GetCurrentSelectedCompanyId();
            IQueryable<Acitivity> query = _dbContext.Acitivity
                .Where(w => w.CompanyId.Equals(companyId));

            return Ok(new { code = StatusCodes.Status200OK });
        }

        /// <summary>
        /// 编辑页面 根据id获取商场活动信息
        /// </summary>
        /// <param name="id">商场活动id</param>
        /// <returns></returns>
        [HttpGet, Route("GetAcitivityById")]
        //[RequirePermission]
        public IActionResult GetAcitivityById(int id)
        {
            var acitivity = _dbContext.Acitivity
                  .FirstOrDefault(w => w.Id.Equals(id));

            if (acitivity == null)
            {
                return Ok(new { code = StatusCodes.Status400BadRequest, message = "数据为空" });
            }

            return Ok(new
            {
                code = StatusCodes.Status200OK,
                data = acitivity
            });
        }

        /// <summary>
        /// 添加商场活动
        /// </summary>
        /// <param name="acitivity">对象</param>
        /// <returns></returns>
        [HttpPost, Route("AddAcitivity")]
        //[RequirePermission]
        public IActionResult AddAcitivity(Acitivity acitivity)
        {
            acitivity.CreateUserId = GetUserId();
            acitivity.CreateUserName = GetUserName();
            acitivity.LastEditUserId = GetUserId();
            acitivity.LastEditUserName = GetUserName();
            _dbContext.Add(acitivity);
            if (_dbContext.SaveChanges() > 0)
                return Ok(new { code = StatusCodes.Status200OK, message = "操作成功", data = acitivity });

            else
                return Ok(new { code = StatusCodes.Status400BadRequest, message = "添加失败" });
        }

        /// <summary>
        /// 更新
        /// </summary>
        /// <param name="acitivity"></param>
        /// <returns></returns>
        [HttpPut, Route("PutAcitivity")]
        public IActionResult PutAcitivity(Acitivity acitivity)
        {

            var query1 = _dbContext.Acitivity.Find(acitivity.Id);

            if (query1 != null)
            {
                query1.Title = acitivity.Title;
                query1.Content = acitivity.Content;
                query1.Type = acitivity.Type;
                query1.StartDate = acitivity.StartDate;
                query1.EndDate  = acitivity.EndDate;
                query1.Remark = acitivity.Remark;
                query1.LastEditUserId = GetUserId();
                query1.LastEditUserName = GetUserName();
                query1.LastEditDate = DateTime.Now;

                _dbContext.Update(query1);
                if (_dbContext.SaveChanges() > 0)
                    return Ok(new { code = StatusCodes.Status200OK, message = "操作成功", data = acitivity });
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
        /// <param name="id">商场活动id</param>
        /// <returns></returns>
        [HttpDelete, Route("DeleteAcitivity")]
        public IActionResult DeleteAcitivity(int? id)
        {
            if (id.HasValue)
            {
                var q1 = _dbContext.Acitivity
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
