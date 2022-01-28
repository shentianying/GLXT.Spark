using GLXT.Spark.Entity;
using GLXT.Spark.Entity.WYGL;
using GLXT.Spark.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GLXT.Spark.Controllers.WYGL
{
    /// <summary>
    /// 物业管理
    /// </summary>
    [Route("api/WYGL/[controller]")]
    [ApiController]
    [Authorize]
    public class PropertyController : BaseController
    {
        private readonly DBContext _dbContext;
        private readonly ICommonService _commonService;
        private readonly ISystemService _systemService;
        public PropertyController(DBContext dbContext, ICommonService commonService, ISystemService systemService)
        {
            _dbContext = dbContext;
            _commonService = commonService;
            _systemService = systemService;
        }

        /// <summary>
        /// 获取分页列表
        /// </summary>
        /// <returns></returns>
        [HttpPost, Route("GetPropertyPaging")]
        public IActionResult GetPropertyPaging()
        {
            int companyId = _systemService.GetCurrentSelectedCompanyId();
            IQueryable<Property> query = _dbContext.Property
                .Where(w => w.CompanyId.Equals(companyId));

            return Ok(new { code = StatusCodes.Status200OK });
        }
    }
}
