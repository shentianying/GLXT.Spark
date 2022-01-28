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
    }
}
