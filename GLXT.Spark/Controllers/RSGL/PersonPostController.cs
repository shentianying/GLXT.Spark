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
    /// 用户岗位管理
    /// </summary>
    [Route("api/RSGL/[controller]")]
    [ApiController]
    [Authorize]
    public class PersonPostController : BaseController
    {
        private readonly DBContext _dbContext;
        private readonly ICommonService _commonService;
        private readonly ISystemService _systemService;
        public PersonPostController(DBContext dbContext, ICommonService commonService, ISystemService systemService)
        {
            _dbContext = dbContext;
            _commonService = commonService;
            _systemService = systemService;
        }

        /// <summary>
        /// 获取全部列表
        /// </summary>
        /// <returns></returns>
        [HttpGet, Route("GetPersonPostList")]
        public IActionResult GetPersonPostList(string name = "")
        {
            var query = _dbContext.PersonPost.Where(w => w.InUse).OrderByDescending(o => o.LastEditDate);

            return Ok(new { code = StatusCodes.Status200OK, data = query });
        }

        /// <summary>
        /// 根据用户id获取所有岗位信息
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet, Route("GetAllPersonPostById")]
        public IActionResult GetAllPersonPostById(int? id)
        {
            if (id.HasValue)
            {
                var PersonPostData = _dbContext.PersonPost
                    .Include(i => i.Post)
                    .Include(i => i.Role)
                    .Include(i => i.Organization)
                    .Include(i => i.PostPoolDetail)
                    .ThenInclude(t => t.PostPool)
                    .Include(i => i.PostPoolDetail)
                    .ThenInclude(t => t.Post)
                    .AsNoTracking()
                    .FirstOrDefault(w => w.PersonId.Equals(id) && w.IsMain);
                var data2 = _commonService.GetOtherPersonPostById(id.Value);
                var data3 = _systemService.GetDictionary("position");

                return Ok(new
                {
                    code = StatusCodes.Status200OK,
                    data = PersonPostData,
                    data2 = data2,
                    data3 = data3
                });
                
            }

            return Ok(new { code = StatusCodes.Status400BadRequest, message = "参数错误" });
            
        }

        /// <summary>
        /// 根据用户id获取全部岗位信息
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet, Route("GetPersonPostListById")]
        public IActionResult GetPersonPostListById(int? id)
        {
            if (id.HasValue)
            {
                var PersonPostData = _dbContext.PersonPost
                    .Include(i => i.Post)
                    .Include(i => i.Role)
                    .Include(i => i.Organization)
                    .Include(i => i.PostPoolDetail)
                    .ThenInclude(t => t.PostPool)
                    .Include(i => i.PostPoolDetail)
                    .ThenInclude(t => t.Post)
                    .Where(w => w.PersonId.Equals(id) && w.InUse);
                if (PersonPostData != null)
                {
                    var result = _systemService.GetDictionary("position");
                    return Ok(new
                    {
                        code = StatusCodes.Status200OK,
                        data = PersonPostData,
                        data1 = result
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
        /// 根据id获取岗位信息
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet, Route("GetPersonPostById")]
        public IActionResult GetPersonPostById(int? id)
        {
            if (id.HasValue)
            {
                var PersonPostData = _dbContext.PersonPost.Where(w => w.Id.Equals(id) && w.InUse);
                if (PersonPostData != null)
                {
                    return Ok(new
                    {
                        code = StatusCodes.Status200OK,
                        data = PersonPostData,
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
        /// 添加用户岗位信息
        /// </summary>
        /// <param name="PersonPost"></param>
        /// <returns></returns>
        [HttpPost, Route("AddPersonPost")]
        [RequirePermission]
        public IActionResult AddPersonPost(PersonPost PersonPost)
        {
            PersonPost.CreateUserId = GetUserId();
            PersonPost.CreateUserName = GetUserName();
            PersonPost.LastEditUserId = GetUserId();
            PersonPost.LastEditUserName = GetUserName();
            _dbContext.Add(PersonPost);
            if (_dbContext.SaveChanges() > 0)
            {
                if (PersonPost.IsMain)
                {
                        var query = _dbContext.Person.Find(PersonPost.PersonId);
                        query.PostId = PersonPost.PostId;
                        query.LastEditDate = DateTime.Now;
                        query.LastEditUserId = GetUserId();
                        query.LastEditUserName = GetUserName();
                        _dbContext.Update(query);
                        _dbContext.SaveChanges();
                    
                }
                return Ok(new { code = StatusCodes.Status200OK, message = "操作成功" });
            }
            else
                return Ok(new { code = StatusCodes.Status400BadRequest, message = "操作失败" });
        }

        /// <summary>
        /// 更新用户岗位信息
        /// </summary>
        /// <param name="PersonPost"></param>
        /// <returns></returns>
        [HttpPut, Route("PutPersonPost")]
        [RequirePermission]
        public IActionResult PutPersonPost(PersonPost PersonPost)
        {
            var query1 = _dbContext.PersonPost.Find(PersonPost.Id);
            query1.OrgId = PersonPost.OrgId;
            query1.PostId = PersonPost.PostId;
            query1.PostPoolDetailId = PersonPost.PostPoolDetailId;
            query1.RoleId = PersonPost.RoleId;
            query1.PositionId = PersonPost.PositionId;
            query1.InUse = PersonPost.InUse;
            query1.LastEditUserId = GetUserId();
            query1.LastEditUserName = GetUserName();
            query1.LastEditDate = DateTime.Now;

            _dbContext.Update(query1);

            if (_dbContext.SaveChanges() > 0)
            {
                if (PersonPost.IsMain)
                {
                    var query = _dbContext.Person.Find(PersonPost.PersonId);
                    query.OrganizationId = PersonPost.OrgId;
                    query.PostId = PersonPost.PostId;
                    query.LastEditDate = DateTime.Now;
                    query.LastEditUserId = GetUserId();
                    query.LastEditUserName = GetUserName();
                    _dbContext.Update(query);
                    _dbContext.SaveChanges();

                }
                return Ok(new { code = StatusCodes.Status200OK, data = PersonPost, success = true, message = "更新成功" });
            }
            else
                return Ok(new { code = StatusCodes.Status400BadRequest, message = "操作失败" });
            
        }

        /// <summary>
        /// 删除岗位池
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete, Route("DeletePersonPost")]
        public IActionResult DeletePersonPost(int id)
        {
            var query = _dbContext.PersonPost.Find(id);
            query.InUse = false;
            _dbContext.Update(query);
            if (_dbContext.SaveChanges() > 0)
                return Ok(new { code = StatusCodes.Status200OK, message = "删除成功" });
            else
                return Ok(new { code = StatusCodes.Status400BadRequest, message = "删除失败" });
        }


        /// <summary>
        /// 用户岗位信息编辑数据接口
        /// </summary>
        /// <param name="orgId"></param>
        /// <param name="companyId"></param>
        /// <returns></returns>
        [HttpGet, Route("GetPersonPostEditInit")]
        public IActionResult GetPersonPostEditInit(int orgId,int companyId = 0)
        {
            var result2 = _systemService.GetRole(null); //角色数据
            var result3 = _systemService.GetDictionary("position");
            if (orgId!=0)
            {
                var result1 = _commonService.GetPostPoolDetailByOrg(orgId);
                return Ok(new { code = StatusCodes.Status200OK, data1 = result1, data2 = result2, data3 = result3 });
            }else
                return Ok(new { code = StatusCodes.Status200OK, data2 = result2, data3 = result3 });

        }
    }

}
