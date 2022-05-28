using GLXT.Spark.Entity;
using GLXT.Spark.Entity.GGGL;
using GLXT.Spark.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GLXT.Spark.Controllers.GGGL
{
    /// <summary>
    /// 广告管理
    /// </summary>
    [Route("api/GGGL/[controller]")]
    [ApiController]
    [Authorize]
    public class AdvertisementController : BaseController
    {
        private readonly DBContext _dbContext;
        private readonly ICommonService _commonService;
        private readonly ISystemService _systemService;
        public AdvertisementController(DBContext dbContext, ICommonService commonService, ISystemService systemService)
        {
            _dbContext = dbContext;
            _commonService = commonService;
            _systemService = systemService;
        }

        /// <summary>
        /// 获取分页列表
        /// </summary>
        /// <returns></returns>
        [HttpPost, Route("GetAdvertisementPaging")]
        public IActionResult GetAdvertisementPaging()
        {
            int companyId = _systemService.GetCurrentSelectedCompanyId();
            IQueryable<Advertisement> query = _dbContext.Advertisement
                .Where(w => w.CompanyId.Equals(companyId));

            return Ok(new { code = StatusCodes.Status200OK });
        }

        /// <summary>
        /// 编辑页面 根据id获取广告信息
        /// </summary>
        /// <param name="id">广告id</param>
        /// <returns></returns>
        [HttpGet, Route("GetAdvertisementById")]
        //[RequirePermission]
        public IActionResult GetAdvertisementById(int id)
        {
            var advertisement = _dbContext.Advertisement
                  .FirstOrDefault(w => w.Id.Equals(id));

            if (advertisement == null)
            {
                return Ok(new { code = StatusCodes.Status400BadRequest, message = "数据为空" });
            }

            return Ok(new
            {
                code = StatusCodes.Status200OK,
                data = advertisement
            });
        }

        /// <summary>
        /// 添加广告
        /// </summary>
        /// <param name="advertisement">对象</param>
        /// <returns></returns>
        [HttpPost, Route("AddAdvertisement")]
        //[RequirePermission]
        public IActionResult AddAdvertisement(Advertisement advertisement)
        {
            advertisement.CreateUserId = GetUserId();
            advertisement.CreateUserName = GetUserName();
            advertisement.LastEditUserId = GetUserId();
            advertisement.LastEditUserName = GetUserName();
            _dbContext.Add(advertisement);
            if (_dbContext.SaveChanges() > 0)
                return Ok(new { code = StatusCodes.Status200OK, message = "操作成功", data = advertisement });

            else
                return Ok(new { code = StatusCodes.Status400BadRequest, message = "添加失败" });
        }

        /// <summary>
        /// 更新
        /// </summary>
        /// <param name="advertisement"></param>
        /// <returns></returns>
        [HttpPut, Route("PutAdvertisement")]
        public IActionResult PutAdvertisement(Advertisement advertisement)
        {

            var query1 = _dbContext.Advertisement.Find(advertisement.Id);

            if (query1 != null)
            {
                query1.Title = advertisement.Title;
                query1.Content = advertisement.Content;
                query1.Type = advertisement.Type;
                query1.StartDate = advertisement.StartDate;
                query1.EndDate = advertisement.EndDate;
                query1.Remark = advertisement.Remark;
                query1.LastEditUserId = GetUserId();
                query1.LastEditUserName = GetUserName();
                query1.LastEditDate = DateTime.Now;

                _dbContext.Update(query1);
                if (_dbContext.SaveChanges() > 0)
                    return Ok(new { code = StatusCodes.Status200OK, message = "操作成功", data = advertisement });
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
        /// <param name="id">广告id</param>
        /// <returns></returns>
        [HttpDelete, Route("DeleteAdvertisement")]
        public IActionResult DeleteAdvertisement(int? id)
        {
            if (id.HasValue)
            {
                var q1 = _dbContext.Advertisement
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
