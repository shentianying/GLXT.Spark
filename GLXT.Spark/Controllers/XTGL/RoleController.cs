using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.OracleClient;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using GLXT.Spark.Entity;
using GLXT.Spark.Entity.RSGL;
using GLXT.Spark.Entity.XTGL;
using GLXT.Spark.Filters;
using GLXT.Spark.IService;
using GLXT.Spark.Utils;
using GLXT.Spark.ViewModel.XTGL.Role;

namespace GLXT.Spark.Controllers.XTGL
{
    /// <summary>
    /// 角色
    /// </summary>
    [Route("api/XTGL/[controller]")]
    [ApiController]
    [Authorize]
    public class RoleController : BaseController
    {
        private readonly DBContext _dbContext;
        private readonly ICommonService _commonService;
        private readonly ISystemService _systemService;

        public RoleController(DBContext dbContext, ISystemService systemService, ICommonService commonService)
        {
            _dbContext = dbContext;
            _commonService = commonService;
            _systemService = systemService;
        }

        #region 角色
        /// <summary>
        /// 获取角色
        /// </summary>
        /// <returns></returns>
        [HttpGet, Route("GetRolePaging")]
        [RequirePermission]
        public IActionResult GetRolePaging(int currentPage, int pageSize, int companyId = 0, string name = "")
        {
            IQueryable<Role> query = _dbContext.Role
            .Where(w => w.CompanyId.Equals(companyId))
            .Include(i => i.UsersRoles)
            .ThenInclude(t => t.Person);
            if (!string.IsNullOrEmpty(name))
                query = query.Where(w => w.Name.Contains(name));
            int count = query.Count();
            var result = query.OrderByDescending(x => x.Id)
                .Skip((currentPage - 1) * pageSize)
                .Take(pageSize).ToList();
            //判断是否有数据，若无则返回第一页
            if (result.Count() == 0)
            {
                currentPage = 1;
                result = query.OrderByDescending(x => x.Id)
                    .Skip((currentPage - 1) * pageSize)
                    .Take(pageSize).ToList();
            }

            List<object> list = new List<object>();
            foreach (var role in result)
            {
                ArrayList al = new ArrayList();
                foreach (var item in role.UsersRoles)
                {
                    al.Add(item.Person.Name+item.Person.Number);
                }
                list.Add(new
                {
                    Id = role.Id,
                    Name = role.Name,
                    Status = role.Status,
                    Remark = role.Remark,
                    PersonList = al
                });
            }
            return Ok(new { code = StatusCodes.Status200OK, data = list, count = count });
        }
        [HttpGet, Route("GetRole")]
        public IActionResult GetRole(string name = "")
        {
            var query =_systemService.GetRole(null,name);
            return Ok(new { code = StatusCodes.Status200OK, data = query });
        }

        [HttpGet, Route("GetRoleById")]
        public IActionResult GetRoleById(int? id)
        {
            if (id.HasValue)
            {
                var query = _dbContext.Role.Find(id);
                return Ok(new { code = StatusCodes.Status200OK, data = query });
            }
            else
                return Ok(new { code = StatusCodes.Status400BadRequest, message = "添加失败" });
        }

        /// <summary>
        /// 添加角色
        /// </summary>
        /// <param name="role"></param>
        /// <returns></returns>
        [HttpPost, Route("AddRole")]
        [RequirePermission]
        public IActionResult AddRole(Role role)
        {
            _dbContext.Add(role);
            if (_dbContext.SaveChanges() > 0)
                return Ok(new { code = StatusCodes.Status200OK, message = "添加成功" });
            else
                return Ok(new { code = StatusCodes.Status400BadRequest, message = "添加失败" });
        }
        /// <summary>
        /// 更修角色
        /// </summary>
        /// <param name="role"></param>
        /// <returns></returns>
        [HttpPut, Route("PutRole")]
        [RequirePermission]
        public IActionResult PutRole(Role role)
        {
            _dbContext.Entry(role).State = EntityState.Modified;
            if (_dbContext.SaveChanges() > 0)
                return Ok(new { code = StatusCodes.Status200OK, message = "更新成功" });
            else
                return Ok(new { code = StatusCodes.Status400BadRequest, message = "更新失败" });
        }
        #endregion

        #region 用户角色
        /// <summary>
        /// 获取用户角色
        /// </summary>
        /// <returns></returns>
        [HttpGet, Route("GetUserRole")]
        public IActionResult GetUserRole(int? userId)
        {
            if (userId.HasValue)
            {
                var q1 = _dbContext.UserRole.Include(i=>i.Role)
                    .Where(w => w.UserId == userId.Value).ToList();
                return Ok(new { code = StatusCodes.Status200OK, data = q1, ua = "" });
            }
            else
                return Ok(new { code = StatusCodes.Status400BadRequest, message = "参数错误" });

        }
        /// <summary>
        /// 添加用户角色
        /// </summary>
        /// <param name="userRole">用户角色对象</param>
        /// <returns></returns>
        [HttpPost, Route("AddUserRole")]
        public IActionResult AddUserRole(UserRole userRole)
        {
            if (_dbContext.UserRole.Any(f => f.UserId.Equals(userRole.UserId) && f.RoleId.Equals(userRole.RoleId)))
            {
                return Ok(new { code = StatusCodes.Status400BadRequest, message = "您已经有该角色了，不能重复添加" });
            }
            _dbContext.UserRole.Add(userRole);
            if (_dbContext.SaveChanges() > 0)
                return Ok(new { code = StatusCodes.Status200OK, message = "保存成功" });
            else
                return Ok(new { code = StatusCodes.Status400BadRequest, message = "保存失败" });
        }
        /// <summary>
        /// 修改用户角色
        /// </summary>
        /// <param name="userRole"></param>
        /// <returns></returns>
        [HttpPut, Route("PutUserRole")]
        public IActionResult PutUserRole(UserRole userRole)
        {
            var query = _dbContext.UserRole.Find(userRole.Id);
            query.RoleId = userRole.RoleId;
            _dbContext.Update(query);
            if (_dbContext.SaveChanges() > 0)
                return Ok(new { code = StatusCodes.Status200OK, message = "更新成功" });
            else
                return Ok(new { code = StatusCodes.Status400BadRequest, message = "更新失败" });
        }
        /// <summary>
        /// 删除用户角色
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete, Route("DeleteUserRole")]
        public IActionResult DeleteUserRole(int? id)
        {
            if (id.HasValue)
            {
                var query = _dbContext.UserRole.Find(id);
                var ucoQuery = _dbContext.UserCheckupOrganization.Where(w => w.RoleId.Equals(id) && w.PersonId.Equals(query.UserId));
                _dbContext.RemoveRange(ucoQuery);
                _dbContext.Remove(query);
                
                if (_dbContext.SaveChanges() > 0)
                    return Ok(new { code = StatusCodes.Status200OK, message = "删除成功" });
                else
                    return Ok(new { code = StatusCodes.Status400BadRequest, message = "删除失败" });
            }
            else
                return Ok(new { code = StatusCodes.Status400BadRequest, message = "参数错误" });
        }

        #endregion

        #region 角色权限
        /// <summary>
        /// 添加角色权限
        /// </summary>
        /// <param name="rolePermit">角色权限对象</param>
        /// <returns></returns>
        [HttpPost, Route("AddRolePermit")]
        [RequirePermission]
        public IActionResult AddRolePermit(List<RolePermit> rolePermit)
        {
            if(rolePermit.Count<=0)
                return Ok(new { code = StatusCodes.Status400BadRequest, message = "权限不能为空" });

            var query = _dbContext.RolePermit.Where(w => w.RoleId == rolePermit[0].RoleId).ToList();
            // 删除
            foreach (var q in query)
            {
                if (!rolePermit.Any(a => a.PermitId == q.PermitId))
                {
                    _dbContext.Remove(q);
                }
            }

            foreach (var rp in rolePermit)
            {
                var hasPermit= query.Where(w => w.PermitId.Equals(rp.PermitId)); // 数据库中是否含有这条权限
                if(!hasPermit.Any())
                {
                    _dbContext.Add(rp);// 没有就添加 权限
                }
            }

            if (_dbContext.SaveChanges() > 0)
            {
                _commonService.RemoveCache<RolePermit>("GetRolePermit");
                return Ok(new { code = StatusCodes.Status200OK, message = "添加成功" });
            }
            else
                return Ok(new { code = StatusCodes.Status400BadRequest, message = "添加失败" });
        }
        /// <summary>
        /// 根据角色获取角色权限列表
        /// </summary>
        /// <param name="roleId"></param>
        /// <returns></returns>
        [HttpGet, Route("GetRolePermitByRoleId")]
        public IActionResult GetRolePermitByRoleId(string roleId)
        {
            if (!string.IsNullOrEmpty(roleId)) {
                var roleIds = roleId.Split(',', StringSplitOptions.RemoveEmptyEntries);
                var intRoleIds = Array.ConvertAll<string, int>(roleIds, s => int.Parse(s));
                var query = _systemService.GetRolePermitByRoleId(intRoleIds);
                return Ok(new { code = StatusCodes.Status200OK, data = query });
            }
            else
                return Ok(new { code = StatusCodes.Status200OK, data = "" });
        }
        #endregion

        #region 用户公司
        /// <summary>
        /// 获取全部列表
        /// </summary>
        /// <returns></returns>
        [HttpGet, Route("GetUserOrganizationList")]
        public IActionResult GetUserOrganizationList([FromQuery] UserOrganization userOrganization)
        {
            var result = _systemService.GetUserOrganizationList(new List<int> { userOrganization.UserId });
            return Ok(new { code = StatusCodes.Status200OK, data = result });
        }
        /// <summary>
        /// 获取用户公司数据和组织机构公司数据
        /// </summary>
        /// <returns></returns>
        [HttpGet, Route("GetUserDataRange")]
        public IActionResult GetUserDataRange(int userId)
        {
            // 用户范围数据
            var result1 = _systemService.GetUserOrganizationList(new List<int> { userId });
            // 所有的公司部门组织机构数据
            var result2 = _systemService.GetOrganizationList(new Organization()); 
            return Ok(new { code = StatusCodes.Status200OK, data1 = result1,data2 =result2 });
        }

        /// <summary>
        /// 获取当前用户数据范围中的组织机构数据
        /// </summary>
        /// <returns></returns>
        [HttpGet, Route("GetCurrentUserOrg")]
        //[AllowAnonymous]
        public IActionResult GetCurrentUserOrg()
        {
            int companyId = _systemService.GetCurrentSelectedCompanyId();
            var list = _systemService.GetUserOrgChildList(GetUserId());
            // 当前用户公司的组织机构数据
            var currentOrgList = _systemService.GetComOrgWithChildren(companyId,companyId);
            return Ok(new { code = StatusCodes.Status200OK, data = list, currentOrgList = currentOrgList });
        }

        /// <summary>
        /// 修改用户公司默认选中项
        /// </summary>
        /// <param name="userOrganization"></param>
        /// <returns></returns>

        [HttpPut, Route("PutUserOrganizationSelected")]
        public IActionResult PutUserOrganizationSelected(UserOrganization userOrganization)
        {
            var query = _dbContext.UserOrganization
                .Where(w => w.UserId.Equals(userOrganization.UserId));
            foreach (var item in query)
            {
                if (item.CompanyId.Equals(userOrganization.CompanyId))
                    item.Selected = true;
                else
                    item.Selected = false;
                _dbContext.Update(item);
            }
            if (_dbContext.SaveChanges() > 0)
                return Ok(new { code = StatusCodes.Status200OK, message = "修改成功" });
            else
                return Ok(new { code = StatusCodes.Status400BadRequest, message = "修改失败" });
        }
        [HttpPost, Route("AddUserOrg")]
        public IActionResult AddUserOrg(List<UserOrganization> userOrgList)
        {
            if (userOrgList.Count > 0)
            {
                // 更新用户公司查看权限
                var queryUo = _dbContext.UserOrganization.Where(w => w.UserId.Equals(userOrgList.First().UserId));
                if (queryUo.Any())
                    _dbContext.UserOrganization.RemoveRange(queryUo);
                var uoList = new List<UserOrganization>();
                int companyIdFlag = 0;
                foreach (var item in userOrgList)
                {
                    var uo = new UserOrganization();
                    uo.UserId = item.UserId;
                    uo.CompanyId = item.CompanyId;
                    uo.OrganizationId = item.OrganizationId;

                    if (companyIdFlag == 0)
                        companyIdFlag = item.CompanyId;

                    if (companyIdFlag.Equals(item.CompanyId))
                        uo.Selected = true;
                    else
                        uo.Selected = false;
                    uoList.Add(uo);
                }
                _dbContext.AddRange(uoList);
            }
            else
            {
                return Ok(new { code = StatusCodes.Status400BadRequest, message = "不能设置为空" });
            }
            if (_dbContext.SaveChanges() > 0)
                return Ok(new { code = StatusCodes.Status200OK, message = "保存成功" });
            else
                return Ok(new { code = StatusCodes.Status400BadRequest, message = "保存失败" });
        }
        #endregion

        #region 用户角色审核范围权限
        [HttpGet,Route("GetUserCheckupOrganization")]
        public IActionResult GetUserCheckupOrganization([FromQuery]UserCheckupOrganization userCheckupOrganization)
        {
            var result = _systemService.GetUserCheckupOrganization(userCheckupOrganization);
            return Ok(new { code = StatusCodes.Status200OK, data = result });
        }
        /// <summary>
        /// 初始化 用户角色审核范围权限 选项卡数据
        /// </summary>
        /// <param name="personId"></param>
        /// <returns></returns>
        [HttpGet, Route("InitUserCheckupOrganization")]
        public IActionResult InitUserCheckupOrganization(int personId)
        {
            var result1 = _systemService.GetUserCheckupOrganization(new UserCheckupOrganization { PersonId=personId});
            var result2 = _systemService.GetOrganizationList(new Organization());
            var result3 = _systemService.GetUserRole(personId);
            return Ok(new { code = StatusCodes.Status200OK, data1 = result1,data2 = result2,data3 = result3 });
        }


        [HttpPost, Route("AddUserCheckupOrganization")]
        public IActionResult AddUserCheckupOrganization(AddUserCheckupOrganizationViewModel aucoVM)
        {
            var query = _dbContext
                .UserCheckupOrganization
                .Where(a => a.PersonId.Equals(aucoVM.personId)&& a.RoleId.Equals(aucoVM.roleId));
            if (query != null)
                _dbContext.RemoveRange(query);
            if(!string.IsNullOrEmpty(aucoVM.orgIds))
            {
                List<UserCheckupOrganization> list = new List<UserCheckupOrganization>();
                foreach (var orgid in aucoVM.orgIds.Split(','))
                {
                    list.Add(new UserCheckupOrganization { 
                        PersonId= aucoVM.personId,
                        OrganizationId= int.Parse(orgid),
                        RoleId = aucoVM.roleId
                    });
                }
                _dbContext.AddRange(list);
            }

            if (_dbContext.SaveChanges() > 0)
                return Ok(new { code = StatusCodes.Status200OK, message = "添加成功" });
            else
                return Ok(new { code = StatusCodes.Status400BadRequest, message = "添加失败" });
        }

        /// <summary>
        /// 删除用户角色
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete, Route("DeleteUserCheckupOrganization")]
        public IActionResult DeleteUserCheckupOrganization(int? id)
        {
            if (id.HasValue)
            {
                var query = _dbContext.UserCheckupOrganization.Find(id);
                _dbContext.UserCheckupOrganization.Remove(query);
                if (_dbContext.SaveChanges() > 0)
                    return Ok(new { code = StatusCodes.Status200OK, message = "删除成功" });
                else
                    return Ok(new { code = StatusCodes.Status400BadRequest, message = "删除失败" });
            }
            else
                return Ok(new { code = StatusCodes.Status400BadRequest, message = "参数错误" });
        }

        #endregion

    }
}