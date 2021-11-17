using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GLXT.Spark.Entity;
using GLXT.Spark.Entity.RSGL;
using GLXT.Spark.Entity.XTGL;
using GLXT.Spark.Filters;
using GLXT.Spark.IService;
using GLXT.Spark.Utils;
using GLXT.Spark.ViewModel.RSGL.Person;

namespace GLXT.Spark.Controllers.RSGL
{
    /// <summary>
    /// 用户
    /// </summary>
    [Route("api/RSGL/[controller]")]
    [ApiController]
    [Authorize]
    public class PersonController : BaseController
    {
        private readonly DBContext _dbContext;
        private readonly IMapper _mapper;
        private readonly ICommonService _commonService;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ISystemService _systemService;
        public PersonController(DBContext dbContext, ICommonService commonService, IMapper mapper, IWebHostEnvironment webHostEnvironment, IHttpContextAccessor httpContextAccessor, ISystemService systemService)
        {
            _dbContext = dbContext;
            _commonService = commonService;
            _mapper = mapper;
            _webHostEnvironment = webHostEnvironment;
            _httpContextAccessor = httpContextAccessor;
            _systemService = systemService;
        }

        /// <summary>
        /// 获取分页列表
        /// </summary>
        /// <returns></returns>
        [HttpGet, Route("GetPersonPaging")]
        [AllowAnonymous]
        public IActionResult GetPersonPaging(int currentPage, int pageSize, int? orgId, int postId = 0, string name = "")
        {
            int companyId = _systemService.GetCurrentSelectedCompanyId();
            IQueryable<Person> query = _dbContext
                .Person
                .Include(i => i.Organization)
                .Include(i => i.Post)
                //.Include(i => i.Company)
                .Where(w => w.InUse && w.CompanyId.Equals(companyId));

            if (!string.IsNullOrWhiteSpace(name))
                query = query.Where(w => w.Name.Contains(name) || w.Number.Contains(name));
            if (orgId.HasValue)
            {
                // 这里加入判断权限


                query = query.Where(w => w.OrganizationId.Equals(orgId.Value));
            }
            if (postId > 0)
            {
                query = query.Where(w => w.PostId.Equals(postId));
            }
            int count = query.Count();
            var result = query.Skip((currentPage - 1) * pageSize)
                .Take(pageSize).AsNoTracking();
            //判断是否有数据，若无则返回第一页
            if (result.Count() == 0)
            {
                currentPage = 1;
                result = query.Skip((currentPage - 1) * pageSize)
                    .Take(pageSize).AsNoTracking();
            }
            var list = _mapper.Map<List<PersonViewModel>>(result.ToList());
            var postList = _commonService.GetPostList();
            return Ok(new { code = StatusCodes.Status200OK, data = list, count = count, post = postList });
        }
        /// <summary>
        /// 获取人员列表
        /// </summary>
        /// <returns></returns>
        [HttpGet, Route("GetPersonList")]
        public IActionResult GetPersonList(int? orgId, string name = "")
        {
            int companyId = _systemService.GetCurrentSelectedCompanyId();
            var query = _dbContext.Person
                .Include(p => p.Post)
                .Include(i => i.Organization)
                .Where(w => w.InUse && w.CompanyId.Equals(companyId) && w.ExpirationDate > DateTime.Now);
            // 工号和姓名搜索
            if (!string.IsNullOrWhiteSpace(name))
                query = query.Where(w => w.Name.Contains(name) || w.Number.Contains(name));
            if (orgId.HasValue)
            {
                int[] ids = new int[1] { orgId.Value };
                var orgIds = _systemService.GetOrgWithChildren(ids).Select(s => s.Id).ToList();
                query = query.Where(w => orgIds.Contains(w.OrganizationId));
            }

            var result = query.AsNoTracking().ToList().Select(s => new
            {
                s.Id,
                s.Name,
                s.Number,
                s.OrganizationId,
                orgName = s.Organization.Name,
                s.Post
            });
            return Ok(new { code = StatusCodes.Status200OK, data = result });
        }

        /// <summary>
        /// 添加人员信息
        /// </summary>
        /// <param name="person"></param>
        /// <returns></returns>
        [HttpPost, Route("AddPerson")]
        public IActionResult AddPerson(Person person)
        {
            if (_dbContext.Person.Any(a => a.Number.Contains(person.Number)))
                return Ok(new { code = StatusCodes.Status400BadRequest, message = "此工号已存在" });

            person.CreateUserId = GetUserId();
            person.CreateUserName = GetUserName();
            person.LastEditUserId = GetUserId();
            person.LastEditUserName = GetUserName();
            person.Password = Utils.Common.GetEncryptPassword("666666");
            _dbContext.Add(person);

            //var uoList = new List<UserOrganization>();
            //int companyIdFlag = 0;
            //foreach (var item in addPersonViewModel.userOrgList)
            //{
            //    var uo = new UserOrganization();
            //    uo.UserId = addPersonViewModel.person.Id;
            //    uo.CompanyId = item.CompanyId;
            //    uo.OrganizationId = item.OrganizationId;

            //    if (companyIdFlag == 0)
            //        companyIdFlag = item.CompanyId;

            //    if (companyIdFlag.Equals(item.CompanyId))
            //        uo.Selected = true;
            //    else
            //        uo.Selected = false;
            //    uoList.Add(uo);
            //}
            //_dbContext.AddRange(uoList);
            //添加用户的默认角色
            //if(!_dbContext.UserRole.Any(a=>a.UserId.Equals(person.Id)&&a.RoleId.Equals(1)))
            //    _dbContext.Add(new UserRole {UserId= person.Id,RoleId =1 });

            if (_dbContext.SaveChanges() > 0)
            {
                var postList = _commonService.GetPostListByOrgId(person.OrganizationId);
                PersonPost personPost = new PersonPost();
                personPost.PersonId = person.Id;
                personPost.PostPoolDetailId = postList.Where(w => w.PostId.Equals(person.PostId)).Select(s => s.Id).FirstOrDefault();
                personPost.OrgId = person.OrganizationId;
                personPost.PostId = person.PostId;
                personPost.RoleId = postList.Where(w => w.PostId.Equals(person.PostId)).Select(s => s.RoleId).FirstOrDefault();
                personPost.PositionId = 0;
                personPost.IsMain = true;
                personPost.InUse = true;
                personPost.CreateUserId = GetUserId();
                personPost.CreateUserName = GetUserName();
                personPost.LastEditUserId = GetUserId();
                personPost.LastEditUserName = GetUserName();
                _dbContext.Add(personPost);
                _dbContext.SaveChanges();
                return Ok(new { code = StatusCodes.Status200OK, message = "添加成功", data = person.Id });
            }
            else
                return Ok(new { code = StatusCodes.Status400BadRequest, message = "添加失败", data = 0 });
        }
        /// <summary>
        /// 更修人员信息
        /// </summary>
        /// <param name="person"></param>
        /// <returns></returns>
        [HttpPut, Route("PutPerson")]
        //public IActionResult PutPerson(AddPersonViewModel personViewModel)
        public IActionResult PutPerson(Person person)
        {
            var query = _dbContext.Person.Find(person.Id);
            int oldPostId = query.PostId;
            int oldOrganizationId = query.OrganizationId;
            query.Name = person.Name;
            query.OrganizationId = person.OrganizationId;
            query.PostId = person.PostId;
            query.IDType = person.IDType;
            query.IDNumber = person.IDNumber;
            query.PhoneNumber = person.PhoneNumber;
            query.IDAddress = person.IDAddress;
            query.HomeAddres = person.HomeAddres;
            query.Gender = person.Gender;
            query.Nation = person.Nation;
            query.BirthDate = person.BirthDate;
            query.Remark = person.Remark;
            query.PersonTypeID = person.PersonTypeID;
            query.InUse = person.InUse;
            query.ExpirationDate = person.ExpirationDate;
            query.AWBank = person.AWBank;
            query.Account = person.Account;

            query.IsUser = person.IsUser;
            query.LastEditDate = DateTime.Now;
            query.LastEditUserId = int.Parse(User.FindFirst(ClaimTypes.Sid).Value);
            query.LastEditUserName = User.FindFirst(ClaimTypes.Name).Value;
            _dbContext.Update(query);

            // 更新用户公司查看权限
            //var queryUo = _dbContext.UserOrganization.Where(w => w.UserId.Equals(personViewModel.person.Id));
            //if(queryUo.Any())
            //    _dbContext.UserOrganization.RemoveRange(queryUo);
            //var uoList = new List<UserOrganization>();
            //int companyIdFlag = 0;
            //foreach (var item in personViewModel.userOrgList)
            //{
            //    var uo = new UserOrganization();
            //    uo.UserId = item.UserId;
            //    uo.CompanyId = item.CompanyId;
            //    uo.OrganizationId = item.OrganizationId;

            //    if (companyIdFlag == 0)
            //        companyIdFlag = item.CompanyId;

            //    if(companyIdFlag.Equals(item.CompanyId))
            //        uo.Selected = true;
            //    else
            //        uo.Selected = false;
            //    uoList.Add(uo);
            //}
            //_dbContext.AddRange(uoList);

            if (_dbContext.SaveChanges() > 0)
            {
                if (oldPostId != person.PostId || oldOrganizationId != person.OrganizationId)
                {
                    var postData = _dbContext.PersonPost.Where(W => W.PersonId.Equals(person.Id) && W.IsMain && W.InUse).FirstOrDefault();
                    var postList = _commonService.GetPostListByOrgId(person.OrganizationId);
                    if (postData != null)
                    {
                        postData.PostPoolDetailId = postList.Where(w => w.PostId.Equals(person.PostId)).Select(s => s.Id).FirstOrDefault();
                        postData.OrgId = person.OrganizationId;
                        postData.PostId = person.PostId;
                        postData.RoleId = postList.Where(w => w.PostId.Equals(person.PostId)).Select(s => s.RoleId).FirstOrDefault();
                        postData.LastEditUserId = GetUserId();
                        postData.LastEditUserName = GetUserName();
                        _dbContext.Update(postData);
                        _dbContext.SaveChanges();
                    }
                    else
                    {
                        PersonPost personPost = new PersonPost();
                        personPost.PersonId = person.Id;
                        personPost.PostPoolDetailId = postList.Where(w => w.PostId.Equals(person.PostId)).Select(s => s.Id).FirstOrDefault();
                        personPost.OrgId = person.OrganizationId;
                        personPost.PostId = person.PostId;
                        personPost.RoleId = postList.Where(w => w.PostId.Equals(person.PostId)).Select(s => s.RoleId).FirstOrDefault();
                        personPost.PositionId = 0;
                        personPost.IsMain = true;
                        personPost.InUse = true;
                        personPost.CreateUserId = GetUserId();
                        personPost.CreateUserName = GetUserName();
                        personPost.LastEditUserId = GetUserId();
                        personPost.LastEditUserName = GetUserName();
                        _dbContext.Add(personPost);
                        _dbContext.SaveChanges();
                    }
                }
                return Ok(new { code = StatusCodes.Status200OK, message = "更新成功" });
            }
            else
                return Ok(new { code = StatusCodes.Status400BadRequest, message = "更新失败" });
        }

        /// <summary>
        /// 删除人员信息
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete, Route("DeletePerson")]
        public IActionResult DeletePerson(int id)
        {
            var query = _dbContext.Person.Find(id);
            query.InUse = false;
            _dbContext.Update(query);
            if (_dbContext.SaveChanges() > 0)
                return Ok(new
                {
                    code = StatusCodes
                    .Status200OK,
                    message = "删除成功"
                });
            else
                return Ok(new { code = StatusCodes.Status400BadRequest, message = "删除失败" });
        }
        ///// <summary>
        ///// 登录获取 Token
        ///// </summary>
        ///// <param name="person">用户名和密码对象</param>
        ///// <returns></returns>
        //[HttpPost, Route("Login")]
        //[AllowAnonymous]
        //public IActionResult Login(Person person)
        //{
        //    var md5PassWord = Utils.Common.GetEncryptPassword(person.Password);
        //    var test = HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
        //    var query = _dbContext.Person
        //        .Include(i=>i.UserRoles)
        //        .Where(w => w.Number.Equals(person.Number) && w.Password.Equals(md5PassWord) && w.InUse)
        //        .FirstOrDefault();
        //    if (query != null)
        //    {
        //        if(!query.IsUser)
        //        {
        //            return Ok(new { code = StatusCodes.Status400BadRequest, message = "只有管理员才能登录" });
        //        }
        //        if (query.ExpirationDate < DateTime.Now)
        //        {
        //            return Ok(new { code = StatusCodes.Status400BadRequest, message = "账号已过期" });
        //        }

        //        return Ok(
        //            new
        //            {
        //                code = StatusCodes.Status200OK,
        //                data = new { token = _commonService.GetToken(query), expire = 60 }
        //            });
        //    }
        //    else
        //        return Ok(new { code = StatusCodes.Status400BadRequest, message = "登录名或者密码错误" });
        //}
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
            var rolePermitList = _systemService.GetRolePermitByRoleId(roleIds);
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
                            userOrgList = query.UserOrganizationList.Select(s => new { s.Id, s.CompanyId, s.OrganizationId, s.Selected }),
                            userName = query.Number,
                            rolePermitList = rolePermitList
                        },
                        pageList = pageList
                    }
                });
        }

        [HttpGet, Route("LogOut")]
        [AllowAnonymous]
        public IActionResult LoginOut()
        {
            return Ok(new { code = StatusCodes.Status200OK, data = "success" });
        }

        /// <summary>
        /// 修改密码
        /// </summary>
        /// <param name="oldPassWord">旧密码</param>
        /// <param name="newPassWord">新密码</param>
        /// <returns></returns>
        [HttpGet, Route("resetPassWord")]
        //[AllowAnonymous]
        [RequirePermission]
        public IActionResult ResetPassWord(string oldPassWord, string newPassWord)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.Sid).Value);
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
        /// <summary>
        /// 修改自己的头像
        /// </summary>
        /// <param name="resource"></param>
        /// <returns></returns>
        [HttpPatch, Route("PatchAvatar")]
        public IActionResult PatchAvatar(UpFileTemp resource)
        {
            //var q1 = _dbContext.Person.Find(GetUserId());
            //q1.Avatar = q.ResourceUrl;
            //_dbContext.Update(q1);

            //q.TableId = q1.Id;
            //_dbContext.Update(q);
            //if (_dbContext.SaveChanges() > 0)
            //    return Ok(new { code = StatusCodes.Status200OK, message = "保存成功" });
            //else
            return Ok(new { code = StatusCodes.Status400BadRequest, message = "保存失败" });
        }
        /// <summary>
        /// 设置默认密码
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPatch, Route("setDefaultPassword")]
        public IActionResult setDefaultPassword(int id)
        {
            var query = _dbContext.Person.Find(id);
            query.Password = Utils.Common.GetEncryptPassword("666666");
            _dbContext.Update(query);
            if (_dbContext.SaveChanges() > 0)
                return Ok(new { code = StatusCodes.Status200OK, message = "密码重置成功" });
            else
                return Ok(new { code = StatusCodes.Status400BadRequest, message = "密码重置失败" });
        }

        [HttpGet, Route("GetPersonById")]
        public IActionResult GetPersonById(int id)
        {
            var result = _systemService.GetPersonById(id);
            string orgStr = _systemService.GetOrgStringByLeafNode(result.OrganizationId);
            var userOrg = _systemService.GetUserOrganizationList(new List<int> { id });
            var result3 = _systemService.GetDictionary("personType"); // 人员类型
            var result4 = _systemService.GetDictionary("idType"); // 证件类型
            var result5 = _systemService.GetDictionary("AWBank"); // 开户行类型
            var postData = _commonService.GetPostListByOrgId(result.OrganizationId);

            //var userPosts = _dbContext.UserPost.OrderBy(o => o.Id).Where(w => w.UserId.Equals(id) && w.InUse);
            //IQueryable<UserPost> query = _dbContext
            //    .UserPost
            //    .Where(w => w.InUse && w.UserId.Equals(id));

            return Ok(new { code = StatusCodes.Status200OK, data = result, data1 = userOrg.Select(s => s.OrganizationId).ToArray(), orgStr = orgStr, data3 = result3, data4 = result4, data5 = result5, data6 = postData });
        }

        /// <summary>
        /// 用户信息编辑数据接口
        /// </summary>
        /// <returns></returns>
        [HttpGet, Route("GetPersonEditInit")]
        public IActionResult GetPersonEditInit(int companyId = 0)
        {
            var result1 = _commonService.GetPostList(); // 岗位数据
            var result2 = _systemService.GetOrganizationList(new Organization()); // 所有的公司部门组织机构数据
            var result3 = _systemService.GetDictionary("personType"); // 人员类型
            var result4 = _systemService.GetDictionary("idType"); // 证件类型
            var result5 = _systemService.GetDictionary("AWBank"); // 开户行类型
            return Ok(new { code = StatusCodes.Status200OK, data1 = result1, data2 = result2, data3 = result3, data4 = result4, data5 = result5 });
        }
        [HttpPost, Route("UploadAvatar")]
        public IActionResult UploadAvatar()
        {
            var formFile = Request.Form.Files[0];
            var dic = Request.Form["dic"];
            if (string.IsNullOrEmpty(dic))
                return BadRequest();

            var result = Common.UpLoadSingleFile(formFile, "images\\" + dic, _webHostEnvironment.WebRootPath, newName: GetUserId().ToString());

            var q1 = _dbContext.Person.Find(GetUserId());
            q1.Avatar = result;
            _dbContext.Update(q1);
            _dbContext.SaveChanges();
            return Ok(new { data = result });
        }

    }
}
