using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GLXT.Spark.Entity;
using GLXT.Spark.Entity.XTGL;
using GLXT.Spark.Filters;
using GLXT.Spark.IService;
using GLXT.Spark.Utils;

namespace GLXT.Spark.Controllers.XTGL
{
    /// <summary>
    /// 菜单页面
    /// </summary>
    [Route("api/XTGL/[controller]")]
    [ApiController]
    [Authorize]
    public class PageController : ControllerBase
    {
        private readonly DBContext _dbContext;
        private readonly ICommonService _commonService;
        private readonly ISystemService _systemService;
        public PageController(DBContext dbContext, ICommonService commonService, ISystemService systemService)
        {
            _dbContext = dbContext;
            _commonService = commonService;
            _systemService = systemService;
        }

        #region Page
        /// <summary>
        /// 获取菜单列表
        /// </summary>
        /// <returns></returns>
        [HttpGet, Route("GetPageList")]
        [RequirePermission]
        public IActionResult GetPageList()
        {
            var query = _systemService.GetPageList();
            return Ok(new { code = StatusCodes.Status200OK, data =query });
        }
        /// <summary>
        /// 初始化为角色添加页面权限
        /// 1.获取所有页面数据
        /// 2.获取页面对应的权限数据
        /// </summary>
        /// <returns></returns>
        [HttpGet, Route("GetPageAndPermit")]
        public IActionResult GetPageAndPermit()
        {
            var pageData = _systemService.GetPageList();
            //pageData=pageData.ToList();
            var permitData = _systemService.GetPermitList();
            return Ok(new { code = StatusCodes.Status200OK, data = new { pageData= pageData, permitData= permitData } });
        }
        /// <summary>
        /// 获取所有 菜单和按钮
        /// </summary>
        /// <returns></returns>
        [HttpGet, Route("GetList")]
        public IActionResult GetList()
        {
            var res = _dbContext.Page.OrderBy(o => o.Sort).ToList();
            return Ok(new { code = StatusCodes.Status200OK, data = res });
        }
        [HttpPost, Route("Add")]
        [RequirePermission]
        public IActionResult Add(Page Page)
        {
            _dbContext.Page.Add(Page);
            if (_dbContext.SaveChanges() > 0)
            {
                return Ok(new { code = StatusCodes.Status200OK, message = "保存成功" });
            }
            else
            {
                return Ok(new { code = StatusCodes.Status400BadRequest, message = "保存失败" });
            }
        }
        [HttpPut, Route("Update")]
        [RequirePermission]
        public IActionResult Update(Page Page)
        {
            _dbContext.Entry(Page).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
            if (_dbContext.SaveChanges() > 0)
                return Ok(new { code = StatusCodes.Status200OK, message = "保存成功" });
            else
                return Ok(new { code = StatusCodes.Status400BadRequest, message = "保存失败" });
        }

        [HttpDelete, Route("Delete")]
        public IActionResult Delete(int id)
        {
            var query = _dbContext.Page.Find(id);
            if (query != null)
            {
                _dbContext.Page.Remove(query);
                _dbContext.SaveChanges();
                return Ok(new { code = StatusCodes.Status200OK, message = "删除成功" });
            }
            else
            {
                return Ok(new { code = StatusCodes.Status400BadRequest, message = "没找到该数据，删除失败" });
            }
        }
        #endregion

        #region Permit 权限
        [HttpGet, Route("GetPermit")]
        [RequirePermission]
        public IActionResult GetPermit([FromQuery]Permit permit)
        {
            IQueryable<Permit> query = _dbContext.Permit.OrderBy(o=>o.Sort).Include(i=>i.Page);
            if(permit.PageId!=0)
                query= query.Where(w => w.PageId.Equals(permit.PageId));
            var result = query.AsNoTracking().ToList();
            return Ok(new { code = StatusCodes.Status200OK, data = result });
        }
        [HttpPost, Route("AddPermit")]
        [RequirePermission]
        public IActionResult AddPermit(Permit permit)
        {
            // 判断不允许添加重复action
            //if (_dbContext.Permit.Any(w => w.Controller.ToLower().Equals(permit.Controller.ToLower()) && w.Action.ToLower().Equals(permit.Action.ToLower())))
            //{
            //    return Ok(new { code = StatusCodes.Status400BadRequest, message = $"该{permit.Controller}下已经存在{permit.Action}了，请不要重复添加" });
            //}
            _dbContext.Add(permit);
            if (_dbContext.SaveChanges() > 0)
            {
                return Ok(new { code = StatusCodes.Status200OK, message = "保存成功" });
            }
            else
            {
                return Ok(new { code = StatusCodes.Status400BadRequest, message = "保存失败" });
            }
        }
        [HttpPatch, Route("UpdatePermit")]
        [RequirePermission]
        public IActionResult UpdatePermit(Permit permit)
        {
            //if (_dbContext.Permit.Any(w => w.Controller.ToLower().Equals(permit.Controller.ToLower()) && w.Action.ToLower().Equals(permit.Action.ToLower()) && w.Id != permit.Id))
            //{
            //    return Ok(new { code = StatusCodes.Status400BadRequest, message = $"该{permit.Controller}下已经存在{permit.Action}了，请不要重复添加" });
            //}
            var query = _dbContext.Permit.Find(permit.Id);
                query.Name = permit.Name;
                query.Code = permit.Code;
                query.IsView = permit.IsView;
                query.Controller = permit.Controller;
                query.Action = permit.Action;
                query.PageId = permit.PageId;
                query.Sort = permit.Sort;
            _dbContext.Update(query);
            if (_dbContext.SaveChanges() > 0)
                return Ok(new { code = StatusCodes.Status200OK, message = "保存成功" });
            else
                return Ok(new { code = StatusCodes.Status400BadRequest, message = "保存失败" });
        }
        [HttpDelete, Route("DeletePermit")]
        [RequirePermission]
        public IActionResult DeletePermit(int id)
        {
            var query = _dbContext.Permit.Find(id);
            _dbContext.Remove(query);
            if (_dbContext.SaveChanges() > 0)
                return Ok(new { code = StatusCodes.Status200OK, message = "保存成功" });
            else
                return Ok(new { code = StatusCodes.Status400BadRequest, message = "保存失败" });
        }
        #endregion
    }
}