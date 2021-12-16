using GLXT.Spark.Entity;
using GLXT.Spark.IService;
using Microsoft.AspNetCore.Authorization;
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
    }
}
