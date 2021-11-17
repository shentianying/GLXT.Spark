using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
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
    /// 城市数据
    /// </summary>
    [Route("api/XTGL/[controller]")]
    [ApiController]
    [Authorize]
    public class CityController : BaseController
    {
        private readonly DBContext _dbContext;
        private readonly ICommonService _commonService;
        private readonly ISystemService _systemService;
        public CityController(DBContext dbContext, ICommonService commonService,ISystemService systemService)
        {
            _commonService = commonService;
            _dbContext = dbContext;
            _systemService = systemService;
        }

        /// <summary>
        /// 城市列表分页
        /// </summary>
        /// <returns></returns>
        [HttpGet, Route("GetCityPaging")]
        [RequirePermission]
        public IActionResult GetCityPaging(int currentPage, int pageSize)
        {
            var query = _dbContext.City
                .OrderBy(o => o.Sort)
                .Where(w => w.InUse);

            int count = query.Count();
            var result = query.Skip((currentPage - 1) * pageSize)
                .Take(pageSize);
            //判断是否有数据，若无则返回第一页
            if (result.Count() == 0)
            {
                currentPage = 1;
                result = query.Skip((currentPage - 1) * pageSize)
                .Take(pageSize);
            }

            return Ok(new { code = StatusCodes.Status200OK, data = result.ToList(), count = count });
        }
        /// <summary>
        /// 获取城市列表
        /// </summary>
        /// <param name="pid"></param>
        /// <returns></returns>
        [HttpGet, Route("GetCityList")]
        [RequirePermission]
        public IActionResult GetCityList(int? pid)
        {
            var q1 = _systemService.GetCity();
            return Ok(new { code = StatusCodes.Status200OK, data = q1.ToList() });
        }
        /// <summary>
        /// 添加城市列表
        /// </summary>
        /// <param name="CityData"></param>
        /// <returns></returns>
        [HttpPost, Route("AddCity")]
        //[RequirePermission]
        public IActionResult AddCity(City CityData)
        {
            var query = _dbContext.City.Any(w => w.InUse 
            && w.Name.Equals(CityData.Name));

            _dbContext.Add(CityData);
            if (_dbContext.SaveChanges() > 0) {
                _commonService.RemoveCache<City>(); // 移除缓存
                return Ok(new { code = StatusCodes.Status200OK, message = "添加成功" });
            }
            else
                return Ok(new { code = StatusCodes.Status400BadRequest, message = "添加失败" });
        }
        /// <summary>
        /// 更新城市列表
        /// </summary>
        /// <param name="CityData"></param>
        /// <returns></returns>
        [HttpPut, Route("PutCity")]
        //[RequirePermission]
        public IActionResult PutCity(City CityData)
        {
            var q = _dbContext.City.Any(w => w.InUse
            && w.Name.Equals(CityData.Name)
            && w.Pid.Equals(CityData.Pid)
            &&w.Id!=CityData.Id
            );
            if(q)
                return Ok(new { code = StatusCodes.Status400BadRequest, message = "同一个分类下面名称不能重复" });

            var query = _dbContext.City.Find(CityData.Id);



                query.Name = CityData.Name;;

                query.Sort = CityData.Sort;
 
                query.InUse = CityData.InUse;
            _dbContext.Update(query);
            if (_dbContext.SaveChanges() > 0)
            {
                _commonService.RemoveCache<City>(); // 移除缓存
                return Ok(new { code = StatusCodes.Status200OK, message = "更新成功" });
            }
            else
                return Ok(new { code = StatusCodes.Status400BadRequest, message = "更新失败" });
        }

        /// <summary>
        /// 删除城市列表
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete, Route("DeleteCity")]
        public IActionResult DeleteCity(int id)
        {
            var query = _dbContext.City.Find(id);
            query.InUse = false;
            _dbContext.Update(query);
            if (_dbContext.SaveChanges() > 0)
            {
                _commonService.RemoveCache<City>(); // 移除缓存
                return Ok(new { code = StatusCodes.Status200OK, message = "删除成功" });
            }
            else
                return Ok(new { code = StatusCodes.Status400BadRequest, message = "删除失败" });
        }
    }
}