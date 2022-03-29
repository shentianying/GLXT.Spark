using GLXT.Spark.Entity;
using GLXT.Spark.Entity.ZSGL;
using GLXT.Spark.IService;
using GLXT.Spark.ViewModel.ZSGL;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GLXT.Spark.Controllers.ZSGL
{
    /// <summary>
    /// 招商资讯管理
    /// </summary>
    [Route("api/ZSGL/[controller]")]
    [ApiController]
    [Authorize]
    public class BussinessInformationController : BaseController
    {
        private readonly DBContext _dbContext;
        private readonly ICommonService _commonService;
        private readonly ISystemService _systemService;
        public BussinessInformationController(DBContext dbContext, ICommonService commonService, ISystemService systemService)
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
        public IActionResult GetBussinessInformationPaging(BussinessInformationSearchViewModel svm)
        {
            int companyId = _systemService.GetCurrentSelectedCompanyId();
            IQueryable<BussinessInformation> query = _dbContext.BussinessInformation
                .Where(w => w.CompanyId.Equals(companyId));

            if (svm.type.HasValue)
            {
                query = query.Where(w => w.InformationType.Equals(svm.type));
            }

            if (string.IsNullOrEmpty(svm.keyName))
            {
                query = query.Where(w => w.Content.Contains(svm.keyName) || w.Title.Contains(svm.keyName));
            }
            int count = query.Count();
            query = query.OrderByDescending(x => x.Id).Skip((svm.currentPage - 1) * svm.pageSize).Take(svm.pageSize);
            return Ok(new { code = StatusCodes.Status200OK, data = query, count = count });
        }
    }
}
