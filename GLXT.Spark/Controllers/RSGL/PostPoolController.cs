using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GLXT.Spark.Entity;
using GLXT.Spark.Entity.RSGL;
using GLXT.Spark.Entity.XTGL;
using GLXT.Spark.Filters;
using GLXT.Spark.IService;
using GLXT.Spark.Utils;
using GLXT.Spark.ViewModel.RSGL;

namespace GLXT.Spark.Controllers.RSGL
{

    /// <summary>
    /// 岗位池
    /// </summary>
    [Route("api/RSGL/[controller]")]
    [ApiController]
    [Authorize]
    public class PostPoolController : BaseController
    {
        private readonly DBContext _dbContext;
        private readonly ICommonService _commonService;
        private readonly ISystemService _systemService;
        public PostPoolController(DBContext dbContext, ICommonService commonService, ISystemService systemService)
        {
            _dbContext = dbContext;
            _commonService = commonService;
            _systemService = systemService;
        }

        /// <summary>
        /// 获取分页列表
        /// </summary>
        /// <returns></returns>
        [HttpGet, Route("GetPostPoolPaging")]
        //[RequirePermission]
        public IActionResult GetPostPoolPaging(int currentPage, int pageSize, string name = "")
        {
            int companyId = _systemService.GetCurrentSelectedCompanyId();
            var query = _commonService.GetPostPoolList(name).Where(w => w.CompanyId.Equals(companyId));
            int count = query.Count();

            var result = query.Skip((currentPage - 1) * pageSize).Take(pageSize).ToList();
            //判断是否有数据，若无则返回第一页
            if (result.Count() == 0)
            {
                currentPage = 1;
                result = query.Skip((currentPage - 1) * pageSize).Take(pageSize).ToList();
            }

            List<PostPoolListViewModel> plvm = new List<PostPoolListViewModel>();
            foreach (var item in result)
            {
                PostPoolListViewModel ppvm = new PostPoolListViewModel();
                string orgName = string.Empty;
                if (item.OrgId != 0)
                {
                    orgName = _systemService.GetOrgStringByLeafNode(item.OrgId);
                }
                ppvm.Id = item.Id;
                ppvm.Name = item.Name;
                ppvm.Category = item.Category;
                ppvm.CompanyId = item.CompanyId;
                ppvm.OrgName = orgName;
                ppvm.InUse = item.InUse;
                plvm.Add(ppvm);
            }

            var categoryList = _systemService.GetDictionary("postPoolCategory");
            return Ok(new { code = StatusCodes.Status200OK, data = plvm, count = count, categoryList = categoryList });

            //IQueryable<PostPool> query = _dbContext.PostPool
            //    //.Include(o => o.Organization)
            //    .Where(w => w.InUse);
            //if (!string.IsNullOrWhiteSpace(name))
            //{
            //    query = query.Where(w => w.Name.Contains(name));
            //}
            //int count = query.Count();

            //query = query.Skip((currentPage - 1) * pageSize)
            //    .Take(pageSize);
            //var categoryList = _systemService.GetDictionary("postPoolCategory");
            //return Ok(new { code = StatusCodes.Status200OK, data = query, count = count, categoryList = categoryList });
        }
        /// <summary>
        /// 获取全部列表
        /// </summary>
        /// <returns></returns>
        [HttpGet, Route("GetPostPoolList")]
        public IActionResult GetPostPoolList(string name = "")
        {
            var query = _dbContext.PostPool
                .Where(w => w.InUse).OrderByDescending(o => o.LastEditDate);

            return Ok(new { code = StatusCodes.Status200OK, data = query });
        }

        /// <summary>
        /// 编辑表单/根据id获取岗位池信息
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet, Route("GetPostPoolById")]
        public IActionResult GetPostPoolById(int? id)
        {
            if (id.HasValue)
            {
                var postPoolData = _commonService.GetPostPoolById(id.Value);
                if (postPoolData != null)
                {
                    List<Dictionary> Dictionaries = new List<Dictionary>();
                    #region 获取字典信息
                    //if (postPoolData.Category != null)
                    //{
                        Dictionaries = _dbContext.Dictionary.Where(w => w.Type.Equals("postPoolCategory") && w.Value.Equals(postPoolData.Category)).ToList();
                    //}

                    #endregion
                    string orgStr = string.Empty;
                    if (postPoolData.OrgId != 0)
                    {
                        orgStr = _systemService.GetOrgStringByLeafNode(postPoolData.OrgId);
                    }
                    return Ok(new
                    {
                        code = StatusCodes.Status200OK,
                        data = postPoolData,
                        dictionaries = Dictionaries,
                        orgName = orgStr
                    });

                }
                else
                {
                    return Ok(new { code = StatusCodes.Status400BadRequest, message = "数据为空" });
                }

            }

            return Ok(new { code = StatusCodes.Status400BadRequest, message = "参数错误" });

        }

        /// <summary>
        /// 根据岗位池明细信息
        /// </summary>
        /// <returns></returns>
        [HttpGet, Route("GetPostPoolDetailList")]
        public IActionResult GetPostPoolDetailList()
        {
            var query = _dbContext
                .PostPoolDetail
                .Include(i => i.Post)
                .Include(t => t.PostPool)
                .AsNoTracking();
            //.FirstOrDefault(w => w.Id.Equals(id));
            return Ok(new
            {
                code = StatusCodes.Status200OK,
                data = query
            });
        }

        /// <summary>
        /// 添加岗位池
        /// </summary>
        /// <param name="PostPool"></param>
        /// <returns></returns>
        [HttpPost, Route("AddPostPool")]
        [RequirePermission]
        public IActionResult AddPostPool(PostPool PostPool)
        {
            if (_dbContext.PostPool.Any(w => w.OrgId.Equals(PostPool.OrgId) && w.OrgId != 0))
            {
                return Ok(new { code = StatusCodes.Status400BadRequest, message = "该岗位池已经存在，请不要重复添加" });
            }
            else if (_dbContext.PostPool.Any(w => w.Category.Equals(PostPool.Category) && w.OrgId == 0))
            {
                return Ok(new { code = StatusCodes.Status400BadRequest, message = "该岗位池已经存在，请不要重复添加" });
            }
            PostPool.CompanyId = _systemService.GetCurrentSelectedCompanyId();
            PostPool.CreateUserId = GetUserId();
            PostPool.CreateUserName = GetUserName();
            PostPool.LastEditUserId = GetUserId();
            PostPool.LastEditUserName = GetUserName();
            _dbContext.Add(PostPool);
            if (_dbContext.SaveChanges() > 0)
            {
                if (!PostPool.OrgId.Equals(0))
                {
                    var query = _dbContext.Organization.Find(PostPool.OrgId);
                    query.CategoryId = PostPool.Category;
                    query.LastEditDate = DateTime.Now;
                    query.LastEditUserId = GetUserId();
                    query.LastEditUserName = GetUserName();
                    _dbContext.Update(query);
                    _dbContext.SaveChanges();

                }

                PostPool.PostPoolDetail.ForEach(f =>
                {
                    //添加角色
                    Role role = new Role();
                    role.Name = PostPool.Name + "-" + _commonService.GetPostNameById(f.PostId);
                    role.Status = 1;
                    role.CompanyId = _systemService.GetCurrentSelectedCompanyId();
                    _dbContext.Add(role);
                    _dbContext.SaveChanges();

                    f.RoleId = role.Id;
                    _dbContext.Update(f);
                    _dbContext.SaveChanges();

                });
                return Ok(new { code = StatusCodes.Status200OK, message = "操作成功" });
            }
            else
                return Ok(new { code = StatusCodes.Status400BadRequest, message = "操作失败" });
        }

        /// <summary>
        /// 更新岗位池
        /// </summary>
        /// <param name="PostPool"></param>
        /// <returns></returns>
        [HttpPut, Route("PutPostPool")]
        [RequirePermission]
        public IActionResult PutPostPool(PostPool PostPool)
        {
            if (_dbContext.PostPool.Any(w => w.OrgId.Equals(PostPool.OrgId) && w.OrgId != 0 && w.Id != PostPool.Id))
            {
                return Ok(new { code = StatusCodes.Status400BadRequest, message = "该岗位池已经存在，请不要重复添加" });
            }
            else if (_dbContext.PostPool.Any(w => w.Category.Equals(PostPool.Category) && w.OrgId == 0 && w.Id != PostPool.Id))
            {
                return Ok(new { code = StatusCodes.Status400BadRequest, message = "该岗位池已经存在，请不要重复添加" });
            }
            var query1 = _dbContext.PostPool.Find(PostPool.Id);
            //string[] orgArrOld = query1.OrgIds.Split(',', StringSplitOptions.None).ToArray();
            query1.OrgId = PostPool.OrgId;
            query1.Category = PostPool.Category;
            query1.InUse = PostPool.InUse;
            query1.LastEditUserId = GetUserId();
            query1.LastEditUserName = GetUserName();
            query1.LastEditDate = DateTime.Now;

            _dbContext.Update(query1);
            _dbContext.SaveChanges();

            var queryDetail = _dbContext.PostPoolDetail.Where(w => w.PostPoolId.Equals(PostPool.Id)).AsNoTracking().ToList();
            foreach (var item in queryDetail)
            {
                //如果传过来的数据原来有就修改，原来的不存在就删除原始数据
                var q4 = PostPool.PostPoolDetail.FirstOrDefault(w => w.Id.Equals(item.Id));
                if (q4 == null)
                {
                    //_dbContext.Remove(item); // 删除
                    item.InUse = false;
                    _dbContext.Update(item);

                    if (_dbContext.SaveChanges()>0)
                    {
                        var queryRoleDel = _dbContext.Role.Find(item.RoleId);
                        queryRoleDel.Status = 0;
                        _dbContext.Update(queryRoleDel);
                        _dbContext.SaveChanges();
                    }


                }
                else
                {
                    //修改
                    int oldPostId = item.PostId;
                    item.RoleId = q4.RoleId;
                    item.PostId = q4.PostId;
                    item.Qualifications = q4.Qualifications;
                    item.PostDuty = q4.PostDuty;
                    item.AdminLeaderId = q4.AdminLeaderId;
                    item.LineLeaderId = q4.LineLeaderId;
                    _dbContext.Update(item);

                    if (_dbContext.SaveChanges()>0)
                    {
                        if (q4.PostId != oldPostId)
                        {
                            //同步更新角色表
                            var queryRoleUpdate = _dbContext.Role.Find(q4.RoleId);
                            queryRoleUpdate.Name = PostPool.Name + "-" + _commonService.GetPostNameById(q4.PostId);
                            _dbContext.Update(queryRoleUpdate);
                            _dbContext.SaveChanges();

                        }
                    }
                    
                }
            }

            PostPool.PostPoolDetail.ForEach(f =>
            {
                //如果，岗位池明细Id=0则是新增
                if (f.Id == 0)
                {
                    //添加角色
                    Role role = new Role();
                    role.Name = PostPool.Name + "-" + _commonService.GetPostNameById(f.PostId);
                    role.Status = 1;
                    role.CompanyId = _systemService.GetCurrentSelectedCompanyId();
                    _dbContext.Add(role);
                    _dbContext.SaveChanges();

                    f.RoleId = role.Id;
                    f.PostPoolId = PostPool.Id;
                    _dbContext.Add(f);
                    _dbContext.SaveChanges();

                }

            });

            //更新组织机构表里的岗位池         
            if (!PostPool.OrgId.Equals(0))
            {
                var query = _dbContext.Organization.Find(PostPool.OrgId);
                query.CategoryId = PostPool.Category;
                query.LastEditDate = DateTime.Now;
                query.LastEditUserId = GetUserId();
                query.LastEditUserName = GetUserName();
                _dbContext.Update(query);
                _dbContext.SaveChanges();

            }

            return Ok(new { code = StatusCodes.Status200OK, data = PostPool, success = true, message = "更新成功" });
        }

        /// <summary>
        /// 删除岗位池
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete, Route("DeletePostPool")]
        public IActionResult DeletePostPool(int id)
        {
            var query = _dbContext.PostPool.Find(id);
            query.InUse = false;
            _dbContext.Update(query);
            if (_dbContext.SaveChanges() > 0)
                return Ok(new { code = StatusCodes.Status200OK, message = "删除成功" });
            else
                return Ok(new { code = StatusCodes.Status400BadRequest, message = "删除失败" });
        }

        /// <summary>
        /// 根据id获取岗位池详情
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet, Route("GetPostPoolDetailById")]
        public IActionResult GetPostPoolDetailById(int? id)
        {
            if (id.HasValue)
            {
                var query = _dbContext
                .PostPoolDetail
                .AsNoTracking()
                .FirstOrDefault(w => w.Id.Equals(id.Value));
                return Ok(new
                {
                    code = StatusCodes.Status200OK,
                    data = query
                });
            }
            else
            {
                return Ok(new { code = StatusCodes.Status400BadRequest, message = "参数错误" });
            }

        }

        /// <summary>
        /// 岗位池信息编辑数据接口
        /// </summary>
        /// <returns></returns>
        [HttpGet, Route("GetPostPoolEditInit")]
        public IActionResult GetPostPoolEditInit()
        {
            var result1 = _systemService.GetDictionary("postPoolCategory");//类比数据
            var result2 = _systemService.GetOrganizationList(new Organization()); // 所有的公司部门组织机构数据
            var result3 = _dbContext
                .PostPoolDetail
                .Include(i => i.Post)
                .Include(t => t.PostPool)
                .AsNoTracking(); //领导数据
            return Ok(new { code = StatusCodes.Status200OK, data1 = result1, data2 = result2, data3 = result3 });
        }

        /// <summary>
        /// 根据组织机构id获取岗位列表
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet, Route("GetPostListByOrgId")]
        public IActionResult GetPostListByOrgId(int? id)
        {
            if (id.HasValue)
            {
                var postData = _commonService.GetPostListByOrgId(id.Value);
                return Ok(new { code = StatusCodes.Status200OK, data = postData });
                //var query = _dbContext.Organization.Find(id);
                //if(query.CategoryId == 1)
                //{
                //    var postData = _dbContext.PostPoolDetail
                //        .Include(i => i.Post)
                //        .Include(t => t.PostPool)
                //        .AsNoTracking()
                //        .Where(w => w.PostPool.OrgId.Equals(id));

                //    return Ok(new { code = StatusCodes.Status200OK, data = postData });
                //}
                //else
                //{
                //    var postData = _dbContext.PostPoolDetail
                //        .Include(i => i.Post)
                //        .Include(t => t.PostPool)
                //        .AsNoTracking()
                //        .Where(w => w.PostPool.Category.Equals(query.CategoryId));
                //    return Ok(new { code = StatusCodes.Status200OK, data = postData });
                //}

            }
            else
                return Ok(new { code = StatusCodes.Status400BadRequest, message = "参数错误" });
        }
    }

}
