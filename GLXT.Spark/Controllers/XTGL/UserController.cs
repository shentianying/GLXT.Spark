using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GLXT.Spark.Entity;
using GLXT.Spark.Entity.RSGL;
using GLXT.Spark.Entity.XTGL;
using GLXT.Spark.IService;

namespace GLXT.Spark.Controllers.XTGL
{
    /// <summary>
    /// 用户信息
    /// </summary>
    [Route("api/XTGL/[controller]")]
    [ApiController]
    public class UserController : BaseController
    {
        private readonly DBContext _dbContext;
        private readonly IMapper _mapper;
        private readonly ICommonService _commonService;
        private readonly ISystemService _systemService;
        public UserController(DBContext dbContext, ICommonService commonService, IMapper mapper, ISystemService systemService)
        {
            _dbContext = dbContext;
            _commonService = commonService;
            _mapper = mapper;
            _systemService = systemService;
        }

        /// <summary>
        /// 登录获取 Token
        /// </summary>
        /// <param name="person">用户名和密码对象</param>
        /// <returns></returns>
        [HttpPost, Route("Login")]
        [AllowAnonymous]
        public IActionResult Login(Person person)
        {
            var md5PassWord = Utils.Common.GetEncryptPassword(person.Password);

            var query = _dbContext.Person
                .Include(i => i.UserRoles)
                .Where(w => w.Number.Equals(person.Number) && w.Password.Equals(md5PassWord) && w.InUse)
                .FirstOrDefault();
            if (query != null)
            {
                if (!query.IsUser)
                {
                    return Ok(new { code = StatusCodes.Status400BadRequest, message = "账号未开通" });
                }
                if (query.ExpirationDate < DateTime.Now)
                {
                    return Ok(new { code = StatusCodes.Status400BadRequest, message = "账号已过期" });
                }

                var logList = _dbContext.Log
                    .Where(w => w.PersonId.Equals(query.Id) && w.OnLine);
                foreach (var item in logList)
                {
                    item.LogoutDate = item.ActiveDate;
                    item.OnLine = false;
                }
                _dbContext.UpdateRange(logList);

                string ipAddress = HttpContext.Connection.RemoteIpAddress.ToString();
                Log log = new Log() { PersonId = query.Id, IPAddress = ipAddress };
                _dbContext.Add(log);
                _dbContext.SaveChanges();

                return Ok(
                    new
                    {
                        code = StatusCodes.Status200OK,
                        data = new { token = _systemService.GetToken(query, log.Id), expire = 60 }
                    });
            }
            else
                return Ok(new { code = StatusCodes.Status400BadRequest, message = "登录名或者密码错误" });
        }
        /// <summary>
        /// 初始化用户信息
        /// </summary>
        /// <returns></returns>
        [HttpGet, Route("Info")]
        public IActionResult GetInfo()
        {
            var userId = GetUserId();
            var query = _dbContext.Person
                .Include(i => i.UserOrganizationList)
                .ThenInclude(i => i.Company)
                .FirstOrDefault(w => w.Id.Equals(userId));
            //获取所有的页面
            var pageList = _systemService.GetPageList();
            //当前用户所属的角色
            var userRoleList = _systemService.GetUserRole(query.Id);
            var roleIds = userRoleList.Select(s => s.RoleId).ToArray();
            var userOrg = query.UserOrganizationList
                .GroupBy(g => new { g.CompanyId, g.Company.Name, g.Selected })
                .Select(s => new
                {
                    s.Key.CompanyId,
                    s.Key.Name,
                    s.Key.Selected
                    //s.OrganizationId
                });
            //根据当前用户id 获取角色权限

            var rolePermitList = _systemService.GetRolePermitByRoleId(roleIds);
            var messageNoReadCount = _dbContext.Remind.Where(w => w.ReceiverId.Equals(GetUserId()) && !w.IsRead).Count();
            return Ok(
                new
                {
                    code = StatusCodes.Status200OK,
                    data = new
                    {
                        Info = new
                        {
                            uid = query.Id,
                            name = query.Name,
                            role = roleIds,
                            avatar = query.Avatar,
                            userOrg = userOrg,//用户公司的权限
                            orgList = _commonService.GetCacheList<Organization>(),
                            userName = query.Number,
                            rolePermitList = rolePermitList
                            
                        },
                        NoReadCount = messageNoReadCount,
                        pageList = pageList
                    }
                }); ; ;
        }

        [HttpGet, Route("LogOut")]
        public IActionResult LoginOut()
        {
            if (User.Claims.Any()) { 
            var log = _dbContext.Log.FirstOrDefault(w => w.Id.Equals(GetUser().LogId));
                if(log!=null)
                {
                    log.LogoutDate = DateTime.Now;
                    log.OnLine = false;
                    _dbContext.Update(log);
                    _dbContext.SaveChanges();
                }
            }
            return Ok(new { code = StatusCodes.Status200OK, data = "success" });
        }

        /// <summary>
        /// 修改密码
        /// </summary>
        /// <param name="oldPassWord">旧密码</param>
        /// <param name="newPassWord">新密码</param>
        /// <returns></returns>
        [HttpPatch, Route("resetPassWord")]
        public IActionResult ResetPassWord(string oldPassWord, string newPassWord)
        {
            var userId = GetUserId();
            oldPassWord = Utils.Common.GetEncryptPassword(oldPassWord);
            newPassWord = Utils.Common.GetEncryptPassword(newPassWord);

            var query = _dbContext.Person.Where(w => w.Id == userId && w.Password == oldPassWord).FirstOrDefault();
            if (query != null)
            {
                query.Password = newPassWord;
                _dbContext.Update(query);
                if (_dbContext.SaveChanges() > 0)
                    return Ok(new { code = StatusCodes.Status200OK, message = "修改成功" });
                else
                    return Ok(new { code = StatusCodes.Status400BadRequest, message = "修改失败" });
            }
            else
            {
                return Ok(new { code = StatusCodes.Status400BadRequest, message = "老密码错误" });
            }
        }


    }
}
