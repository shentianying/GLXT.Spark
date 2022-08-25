using GLXT.Spark.Entity;
using GLXT.Spark.Entity.RSGL;
using GLXT.Spark.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
        public IActionResult GetResourcesInfo()
        {
            int companyId = _systemService.GetCurrentSelectedCompanyId();

            //基本信息
            var companyInfo = _dbContext.AccountSet
                  .FirstOrDefault(w => w.Id.Equals(companyId));

            //人员数量
            int iPeopleCount = _dbContext.Person
                .Where(w => w.IsUser && w.CompanyId.Equals(companyId)).Count();
            //访客数量
            int iVisitorCount = _dbContext.Visitor
                .Where(w => w.CompanyId.Equals(companyId)).Count();
            //监控数量
            int iMoniorCount = _dbContext.Monitor
                .Where(w => w.CompanyId.Equals(companyId)).Count();
            //意向已签数量
            int iEnterpriseCount = _dbContext.Contract
                .Include(i => i.Enterprise)
                .Where(w => w.CompanyId.Equals(companyId)).Count();

            return Ok(new { 
                code = StatusCodes.Status200OK, 
                data = companyInfo,
                iPeopleCount = iPeopleCount, 
                iVisitorCount= iVisitorCount,
                iMoniorCount = iMoniorCount
            });
        }

        /// <summary>
        /// 获取
        /// </summary>
        /// <returns></returns>
        [HttpPost, Route("GetStatisticsInfo")]
        public IActionResult GetStatisticsInfo()
        {
            int companyId = _systemService.GetCurrentSelectedCompanyId();

            //基本信息
            var companyInfo = _dbContext.AccountSet
                  .FirstOrDefault(w => w.Id.Equals(companyId));
            //人员数量
            int iPeopleCount = _dbContext.Person
                .Where(w => w.IsUser && w.CompanyId.Equals(companyId)).Count();
            //访客数量
            int iVisitorCount = _dbContext.Visitor
                .Where(w => w.CompanyId.Equals(companyId)).Count();
            //监控数量
            int iMoniorCount = _dbContext.Monitor
                .Where(w => w.CompanyId.Equals(companyId)).Count();
            //意向已签数量
            int iEnterpriseCount = _dbContext.Contract
                .Include(i => i.Enterprise)
                .Where(w => w.CompanyId.Equals(companyId)).Count();

            return Ok(new
            {
                code = StatusCodes.Status200OK,
                data = companyInfo,
                iPeopleCount = iPeopleCount,
                iVisitorCount = iVisitorCount,
                iMoniorCount = iMoniorCount
            });
        }
    }
}
