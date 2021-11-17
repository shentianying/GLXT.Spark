using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GLXT.Spark.Entity;
using GLXT.Spark.Entity.XTGL;
using GLXT.Spark.Filters;
using GLXT.Spark.IService;
using GLXT.Spark.Utils;
using GLXT.Spark.ViewModel.RSGL;
using GLXT.Spark.ViewModel.RSGL.Person;

namespace GLXT.Spark.Controllers.XTGL
{
    /// <summary>
    /// 组织机构
    /// </summary>
    [Route("api/XTGL/[controller]")]
    [ApiController]
    [Authorize]
    public class OrganizationController : BaseController
    {
        private readonly DBContext _dbContext;
        private readonly ICommonService _commonService;
        private readonly ISystemService _systemService;
        public OrganizationController(DBContext dbContext, ICommonService commonService, ISystemService systemService)
        {
            _dbContext = dbContext;
            _commonService = commonService;
            _systemService = systemService;
        }

        /// <summary>
        /// 分页组织机构
        /// </summary>
        /// <returns></returns>
        [HttpGet, Route("GetOrganizationPaging")]
        public IActionResult GetOrganizationPaging(int currentPage, int pageSize, string name = "")
        {
            IQueryable<Organization> query = _dbContext.Organization;
            if (!string.IsNullOrEmpty(name))
                query = query.Where(w => w.Name.Contains(name));
            int count = query.Count();
            var result = query.OrderBy(x => x.Sort).Skip((currentPage - 1) * pageSize).Take(pageSize).ToList();
            //判断是否有数据，若无则返回第一页
            if (result.Count() == 0)
            {
                currentPage = 1;
                result = query.OrderBy(x => x.Sort).Skip((currentPage - 1) * pageSize).Take(pageSize).ToList();
            }

            return Ok(new { code = StatusCodes.Status200OK, data = result, count = count });
        }
        /// <summary>
        /// 获取组织机构
        /// </summary>
        /// <param name="organization"></param>
        /// <returns></returns>
        [HttpGet, Route("GetOrganizationList")]
        [RequirePermission]
        public IActionResult GetOrganizationList([FromQuery]Organization organization)
        {
            var query =_systemService.GetOrganizationList(organization);
            return Ok(new { code = StatusCodes.Status200OK, data = query });
        }

        /// <summary>
        /// 获取当前用户所在公司的所有组织机构
        /// </summary>
        /// <returns></returns>
        [HttpGet, Route("GetCurrentCompanyOrgList")]
        public IActionResult GetCurrentCompanyOrgList()
        {
            int companyId = _systemService.GetCurrentSelectedCompanyId();
            var list = _systemService.GetComOrgWithChildren(companyId,companyId)
                .OrderBy(o => o.Sort)
                .Select(s => new
                {
                    s.Id,
                    pid = s.PId,
                    s.Name
                });
            return Ok(new { code = StatusCodes.Status200OK, data = list });
        }
            

        /// <summary>
        /// 获取当前用户所在公司的组织机构
        /// </summary>
        /// <param name="osvm"></param>
        /// <returns></returns>
        [HttpPost, Route("GetComOrganizationList")]
        //[RequirePermission]
        public IActionResult GetComOrganizationList(OrganizationSearchViewModel osvm)
        {
            int companyId = _systemService.GetCurrentSelectedCompanyId();
            var OrgList = _systemService.GetOrganizationList(osvm);
            if (!string.IsNullOrWhiteSpace(osvm.name) || osvm.isProject.HasValue || osvm.categoryIds.Length > 0)
            {
                List<int> childOrg = new List<int>();
                foreach (var org in OrgList)
                {
                    childOrg.Add(org.Id);
                }
                //childOrg = _systemService.GetOrgWithChildrenIds(childOrg.ToArray());

                OrgList = _systemService.GetOrgWithParents(_systemService.GetOrganizationList(new Organization()), _systemService.GetOrgWithChildrenIds(childOrg.ToArray()).ToArray());
            }

            //var userOrgList = _systemService.GetUserOrganizationList(new List<int> { GetUserId() });

            //if (userOrgList == null)
            //    return null;

            //// 获取用户数据范围orgIds下的所有子节点
            //var orgIds = userOrgList.Select(s => s.OrganizationId).ToArray();
            var comOrglist = _systemService.GetWithChildren(OrgList, companyId);
            //var list = _systemService.GetWithChildren(OrgList, orgIds);
                       
            return Ok(new { code = StatusCodes.Status200OK, data = comOrglist });
        }

        /// <summary>
        /// 获取当前用户所在公司的组织机构和人员组成
        /// </summary>
        /// <returns></returns>
        [HttpPost, Route("GetComOrganizationAndPersonList")]
        //[RequirePermission]
        public IActionResult GetComOrganizationAndPersonList(OrganizationSearchViewModel osvm)
        {
            int companyId = _systemService.GetCurrentSelectedCompanyId();
            
            List<Organization> OrgList = new List<Organization>();
            if (osvm.orgIds.Length > 0)
                // OrgList = _systemService.GetOrgWithParents(osvm.orgIds);
                OrgList = _systemService.GetOrgRootToParent(osvm.orgIds);
            else
                OrgList = _systemService.GetOrganizationList(new Organization());
            //根据岗位和名称查找到对应的组织机构集合
            List<int> PersonOrgIds = _commonService.GetPersonOrgIds(osvm.postId, osvm.name);
            List<PersonOrganizationViewModel> plvm = new List<PersonOrganizationViewModel>();
            if (PersonOrgIds.Count() > 0)
            {
                OrgList = _systemService.GetOrgWithParents(OrgList, PersonOrgIds.ToArray());
                foreach (var item in OrgList)
                {
                    PersonOrganizationViewModel povm = new PersonOrganizationViewModel();
                    povm.Id = item.Id;
                    povm.PId = item.PId;
                    povm.Name = item.Name;
                    povm.personList = _commonService.GetPersonListByOrgId(item.Id, osvm.postId, osvm.name);

                    plvm.Add(povm);
                }
            }                
            else
                OrgList = null;
            

            
            
            // var personOrglist = _systemService.GetWithChildren(plvm, companyId);
            var postList = _commonService.GetPostList();

            return Ok(new { code = StatusCodes.Status200OK, data = plvm, data1 = postList });
        }

        /// <summary>
        /// 添加组织机构
        /// </summary>
        /// <param name="organization"></param>
        /// <returns></returns>
        [HttpPost, Route("AddOrganization")]
        [RequirePermission]
        public IActionResult AddOrganization(Organization organization)
        {
            if (organization.PId != 0)
            {
                var orgParent = _dbContext.Organization.Find(organization.PId);
                if (orgParent.AccountSetIds.Contains(organization.AccountSetIds))
                {
                    organization.CreateUserId = GetUserId();
                    organization.CreateUserName = GetUserName();
                    organization.LastEditUserId = GetUserId();
                    organization.LastEditUserName = GetUserName();

                    _dbContext.Add(organization);
                    if (_dbContext.SaveChanges() > 0)
                        return Ok(new { code = StatusCodes.Status200OK, message = "添加成功" });
                    else
                        return Ok(new { code = StatusCodes.Status400BadRequest, message = "添加失败" });
                }
                else
                {
                    return Ok(new { code = StatusCodes.Status400BadRequest, message = "子节点的账套选择不能超过父节点" });
                }
            }
            else
            {
                organization.CreateUserId = GetUserId();
                organization.CreateUserName = GetUserName();
                organization.LastEditUserId = GetUserId();
                organization.LastEditUserName = GetUserName();

                _dbContext.Add(organization);
                if (_dbContext.SaveChanges() > 0)
                    return Ok(new { code = StatusCodes.Status200OK, message = "添加成功" });
                else
                    return Ok(new { code = StatusCodes.Status400BadRequest, message = "添加失败" });
            }
            
            
        }
        /// <summary>
        /// 更新组织机构
        /// </summary>
        /// <param name="organization"></param>
        /// <returns></returns>
        [HttpPut, Route("PutOrganization")]
        [RequirePermission]
        public IActionResult PutOrganization(Organization organization)
        {
            if (organization.PId != 0)
            {
                var orgParent = _dbContext.Organization.Find(organization.PId);
                if (orgParent.AccountSetIds.Contains(organization.AccountSetIds))
                {
                    var query = _dbContext.Organization.Find(organization.Id);

                    query.PId = organization.PId;
                    query.Name = organization.Name;
                    query.ShortName = organization.ShortName;
                    query.AccountSetIds = organization.AccountSetIds;
                    query.CategoryId = organization.CategoryId;
                    query.Sort = organization.Sort;
                    query.Optional = organization.Optional;
                    query.IsProject = organization.IsProject;
                    query.InUse = organization.InUse;

                    query.IsSum = organization.IsSum;

                    query.LastEditDate = DateTime.Now;
                    query.LastEditUserId = GetUserId();
                    query.LastEditUserName = GetUserName();
                    _dbContext.Update(query);
                    if (_dbContext.SaveChanges() > 0)
                        return Ok(new { code = StatusCodes.Status200OK, message = "更新成功" });
                    else
                        return Ok(new { code = StatusCodes.Status400BadRequest, message = "更新失败" });
                }
                else
                {
                    return Ok(new { code = StatusCodes.Status400BadRequest, message = "子节点的账套选择不能超过父节点" });
                }
            }
            else
            {
                var query = _dbContext.Organization.Find(organization.Id);

                query.PId = organization.PId;
                query.Name = organization.Name;
                query.ShortName = organization.ShortName;
                query.AccountSetIds = organization.AccountSetIds;
                query.CategoryId = organization.CategoryId;
                query.Sort = organization.Sort;
                query.Optional = organization.Optional;
                query.IsProject = organization.IsProject;
                query.InUse = organization.InUse;

                query.IsSum = organization.IsSum;

                query.LastEditDate = DateTime.Now;
                query.LastEditUserId = GetUserId();
                query.LastEditUserName = GetUserName();
                _dbContext.Update(query);
                if (_dbContext.SaveChanges() > 0)
                    return Ok(new { code = StatusCodes.Status200OK, message = "更新成功" });
                else
                    return Ok(new { code = StatusCodes.Status400BadRequest, message = "更新失败" });
            }
            
        }
        /// <summary>
        /// 更新状态
        /// </summary>
        /// <param name="id">id</param>
        /// <param name="status">状态 0|1</param>
        /// <returns></returns>
        [HttpPatch, Route("PatchOrganization")]
        public IActionResult PatchOrganization(int id, int status)
        {
            return Ok(new { code = StatusCodes.Status400BadRequest, message = "更新失败" });
        }

        /// <summary>
        /// 删除组织机构
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete, Route("DeleteOrganization")]
        public IActionResult DeleteOrganization(int id)
        {
            var query = _dbContext.Organization.Find(id);
            _dbContext.Remove(query);
            if (_dbContext.SaveChanges() > 0)
                return Ok(new { code = StatusCodes.Status200OK, message = "删除成功" });
            else
                return Ok(new { code = StatusCodes.Status400BadRequest, message = "删除失败" });
        }

        /// <summary>
        /// 获取当前用户所在公司的项目的上级所有组织机构
        /// </summary>
        /// <returns></returns>
        [HttpGet, Route("GetOrgWithProject")]
        public IActionResult GetOrgWithProject()
        {
            //var orgList = _systemService.GetOrgWithProject(_systemService.GetCurrentSelectedCompanyId());
            var orgList = _systemService.GetOrgWithChildren(_systemService.GetCurrentSelectedCompanyId()).Where(w => !w.IsProject).ToList();
            return Ok(new { code = StatusCodes.Status200OK, data = orgList });
        }

    }
}