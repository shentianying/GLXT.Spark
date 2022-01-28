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
    }
}
