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
    /// 赛事安排
    /// </summary>
    [Route("api/HDGL/[controller]")]
    [ApiController]
    [Authorize]
    public class MatchController : BaseController
    {
        private readonly DBContext _dbContext;
        private readonly ICommonService _commonService;
        private readonly ISystemService _systemService;
        public MatchController(DBContext dbContext, ICommonService commonService, ISystemService systemService)
        {
            _dbContext = dbContext;
            _commonService = commonService;
            _systemService = systemService;
        }

        /// <summary>
        /// 获取分页列表
        /// </summary>
        /// <returns></returns>
        [HttpPost, Route("GetBussinessInformationPaging")]
        public IActionResult GetBussinessInformationPaging()
        {
            int companyId = _systemService.GetCurrentSelectedCompanyId();
            IQueryable<Match> query = _dbContext.Match
                .Where(w => w.CompanyId.Equals(companyId));

            return Ok(new { code = StatusCodes.Status200OK });
        }

        /// <summary>
        /// 编辑页面 根据id获取意向企业信息
        /// </summary>
        /// <param name="id">意向企业id</param>
        /// <returns></returns>
        [HttpGet, Route("GetMatchById")]
        //[RequirePermission]
        public IActionResult GetMatchById(int id)
        {
            var match = _dbContext.Match
                  .FirstOrDefault(w => w.Id.Equals(id));

            if (match == null)
            {
                return Ok(new { code = StatusCodes.Status400BadRequest, message = "数据为空" });
            }

            return Ok(new
            {
                code = StatusCodes.Status200OK,
                data = match
            });
        }

        /// <summary>
        /// 添加意向企业
        /// </summary>
        /// <param name="match">对象</param>
        /// <returns></returns>
        [HttpPost, Route("AddMatch")]
        //[RequirePermission]
        public IActionResult AddMatch(Match match)
        {
            match.CreateUserId = GetUserId();
            match.CreateUserName = GetUserName();
            match.LastEditUserId = GetUserId();
            match.LastEditUserName = GetUserName();
            _dbContext.Add(match);
            if (_dbContext.SaveChanges() > 0)
                return Ok(new { code = StatusCodes.Status200OK, message = "操作成功", data = match });

            else
                return Ok(new { code = StatusCodes.Status400BadRequest, message = "添加失败" });
        }

        /// <summary>
        /// 更新
        /// </summary>
        /// <param name="match"></param>
        /// <returns></returns>
        [HttpPut, Route("PutMatch")]
        public IActionResult PutMatch(Match match)
        {

            var query1 = _dbContext.Match.Find(match.Id);

            if (query1 != null)
            {
                query1.Title = match.Title;
                query1.Content = match.Content;
                query1.Type = match.Type;
                query1.StartDate = match.StartDate;
                query1.EndDate = match.EndDate;
                query1.Remark = match.Remark;
                query1.LastEditUserId = GetUserId();
                query1.LastEditUserName = GetUserName();
                query1.LastEditDate = DateTime.Now;
                query1.LastEditUserId = GetUserId();
                query1.LastEditUserName = GetUserName();
                query1.LastEditDate = DateTime.Now;

                _dbContext.Update(query1);
                if (_dbContext.SaveChanges() > 0)
                    return Ok(new { code = StatusCodes.Status200OK, message = "操作成功", data = match });
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
        [HttpDelete, Route("DeleteMatch")]
        public IActionResult DeleteMatch(int? id)
        {
            if (id.HasValue)
            {
                var q1 = _dbContext.Match
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
