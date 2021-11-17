using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using GLXT.Spark.Entity;
using GLXT.Spark.Entity.XTGL;
using GLXT.Spark.Filters;

namespace GLXT.Spark.Controllers.XTGL
{
    /// <summary>
    /// 账套信息
    /// </summary>
    [Route("api/XTGL/[controller]")]
    [ApiController]
    [Authorize]
    public class AccountSetController : ControllerBase
    {
        private readonly DBContext _dbContext;
        private readonly IMemoryCache _memoryCache;
        public AccountSetController(DBContext dbContext, IMemoryCache memoryCache)
        {
            _dbContext = dbContext;
            _memoryCache = memoryCache;
        }
        /// <summary>
        /// 获取
        /// </summary>
        /// <returns></returns>
        [HttpGet, Route("GetAccountSet")]
        [RequirePermission]
        public IActionResult GetAccountSet(int currentPage, int pageSize, string name = "", bool isMenu = false)
        {
            // var userinfo=_memoryCache.Get("USERINFO_" + User.FindFirst(ClaimTypes.Sid).Value);
            IQueryable<AccountSet> query = _dbContext.AccountSet;
            if (!string.IsNullOrEmpty(name))
                query = query.Where(w => w.Name.Contains(name));
            int count = query.Count();
            query = query.OrderByDescending(x => x.Id).Skip((currentPage - 1) * pageSize).Take(pageSize);
            return Ok(new { code = StatusCodes.Status200OK, data = query.ToList(), count = count });
        }
        /// <summary>
        /// 添加
        /// </summary>
        /// <param name="accountSet"></param>
        /// <returns></returns>
        [HttpPost, Route("AddAccountSet")]
        [RequirePermission]
        public IActionResult AddAccountSet(AccountSet accountSet)
        {
            _dbContext.Add(accountSet);
            if (_dbContext.SaveChanges() > 0)
                return Ok(new { code = StatusCodes.Status200OK, message = "添加成功" });
            else
                return Ok(new { code = StatusCodes.Status400BadRequest, message = "添加失败" });
        }
        /// <summary>
        /// 更新
        /// </summary>
        /// <param name="accountSet"></param>
        /// <returns></returns>
        [HttpPut, Route("PutAccountSet")]
        [RequirePermission]
        public IActionResult PutAccountSet(AccountSet accountSet)
        {
            _dbContext.Entry(accountSet).State = EntityState.Modified;
            if (_dbContext.SaveChanges() > 0)
                return Ok(new { code = StatusCodes.Status200OK, message = "更新成功" });
            else
                return Ok(new { code = StatusCodes.Status400BadRequest, message = "更新失败" });
        }
        
        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete, Route("DeleteAccountSet")]
        public IActionResult DeleteAccountSet(int id)
        {
            var query = _dbContext.AccountSet.Find(id);
            query.InUse = false;
            _dbContext.Update(query);
            if (_dbContext.SaveChanges() > 0)
                return Ok(new { code = StatusCodes.Status200OK, message = "删除成功" });
            else
                return Ok(new { code = StatusCodes.Status400BadRequest, message = "删除失败" });
        }

        /// <summary>
        /// 根据组织机构Id获取其对应的账套
        /// </summary>
        /// <param name="orgId"></param>
        /// <returns></returns>
        [HttpGet, Route("GetAccountSetByOrgId")]
        public IActionResult GetAccountSetByOrgId(int orgId = 0)
        {
            if (orgId == 0)
            {
                var accountList = _dbContext.AccountSet.Where(w => w.InUse).ToList();
                return Ok(new { code = StatusCodes.Status200OK, message = "获取成功", data = accountList });
            }
            else
            {
                List<string> accountSetIds = _dbContext.Organization.Where(w => w.Id.Equals(orgId)).Select(s => s.AccountSetIds).FirstOrDefault().Split(",").ToList();//accountSetIds.Split(",").ToList();
                var ids = accountSetIds.Select<string, int>(q => Convert.ToInt32(q));
                var accountList = _dbContext.AccountSet.Where(w => ids.Contains(w.Id)).ToList();
                return Ok(new { code = StatusCodes.Status200OK, message = "获取成功", data = accountList });
            }
            
        }


    }
}