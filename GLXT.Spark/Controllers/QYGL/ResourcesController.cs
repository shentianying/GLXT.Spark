using GLXT.Spark.Entity;
using GLXT.Spark.IService;
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
    /// 房产资源管理
    /// </summary>
    [Route("api/QYGL/[controller]")]
    [ApiController]
    [Authorize]
    public class ResourcesController : BaseController
    {
        private readonly DBContext _dbContext;
        private readonly ICommonService _commonService;
        private readonly ISystemService _systemService;
        public ResourcesController(DBContext dbContext, ICommonService commonService, ISystemService systemService)
        {
            _dbContext = dbContext;
            _commonService = commonService;
            _systemService = systemService;
        }


        /// <summary>
        /// 获取企业信息
        /// </summary>
        /// <returns></returns>
        [HttpPost, Route("GetResourcesInfo")]
        public IActionResult GetPropertyPaging()
        {
            int companyId = _systemService.GetCurrentSelectedCompanyId();

            //基本信息


            //人员数量

            //房产数量

            //车位数量

            return Ok(new { code = StatusCodes.Status200OK });
        }
    }
}
