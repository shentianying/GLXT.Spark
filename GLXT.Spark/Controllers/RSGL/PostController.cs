using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using GLXT.Spark.Entity;
using GLXT.Spark.Entity.RSGL;
using GLXT.Spark.Entity.XTGL;
using GLXT.Spark.Enums;
using GLXT.Spark.Filters;
using GLXT.Spark.IService;
using GLXT.Spark.Utils;
using GLXT.Spark.ViewModel.RSGL;

namespace GLXT.Spark.Controllers.RSGL
{
    /// <summary>
    /// 岗位
    /// </summary>
    [Route("api/RSGL/[controller]")]
    [ApiController]
    [Authorize]
    public class PostController : BaseController
    {
        private readonly DBContext _dbContext;
        private readonly ICommonService _commonService;
        private readonly ISystemService _systemService;
        public PostController(DBContext dbContext, ICommonService commonService,ISystemService systemService)
        {
            _dbContext = dbContext;
            _commonService = commonService;
            _systemService = systemService;
        }

        /// <summary>
        /// 获取分页列表
        /// </summary>
        /// <returns></returns>
        [HttpPost, Route("GetPostPaging")]
        [RequirePermission]
        public IActionResult GetPostPaging(PostSearchViewModel psvm)
        {
            int companyId = _systemService.GetCurrentSelectedCompanyId();
            IQueryable<Post> query = _dbContext.Post
                .Where(w => w.InUse && w.CompanyId.Equals(companyId))
                .OrderBy(o => o.RankRangeMin)
                .ThenBy(t => t.RankRangeMax);
            if (!string.IsNullOrWhiteSpace(psvm.name))
            {
                query = query.Where(w => w.Name.Contains(psvm.name));
            }
            if (psvm.postSequenceIds.Length>0) 
            {
                query = query.Where(w => psvm.postSequenceIds.Contains(w.PostSequenceID));
            }
            if (psvm.bussinessLineIds.Length > 0)
            {
                query = query.Where(w => psvm.bussinessLineIds.Contains(w.BussinessLineID));
            }
            int count = query.Count();
            var result = query.Skip((psvm.currentPage - 1) * psvm.pageSize)
                .Take(psvm.pageSize);
            //判断是否有数据，若无则返回第一页
            if (result.Count() == 0)
            {
                psvm.currentPage = 1;
                result = query.Skip((psvm.currentPage - 1) * psvm.pageSize)
                .Take(psvm.pageSize);
            }
            var postSequence = _systemService.GetDictionary("postSequence");
            var bussinessLine = _systemService.GetDictionary("bussinessLine");
            return Ok(new { code = StatusCodes.Status200OK, data = result, count = count, postSequenceList = postSequence, bussinessLineList = bussinessLine });
        }
        /// <summary>
        /// 获取全部列表
        /// </summary>
        /// <returns></returns>
        [HttpGet, Route("GetPostList")]
        public IActionResult GetPostList(string name="")
        {
            var result = _commonService.GetPostList(name);
            return Ok(new { code = StatusCodes.Status200OK, data = result });
        }
        
        /// <summary>
        /// 添加岗位信息
        /// </summary>
        /// <param name="post"></param>
        /// <returns></returns>
        [HttpPost, Route("AddPost")]
        [RequirePermission]
        public IActionResult AddPost(Post post)
        {
            if (_dbContext.Post.Any(w => w.Name.Equals(post.Name)))
            {
                return Ok(new { code = StatusCodes.Status400BadRequest, message = "岗位名称已经存在，请不要重复添加" });
            }
            post.CompanyId = _systemService.GetCurrentSelectedCompanyId();
            post.CreateUserId = int.Parse(User.FindFirst(ClaimTypes.Sid).Value);
            post.CreateUserName = User.FindFirst(ClaimTypes.Name).Value;
            post.LastEditUserId = int.Parse(User.FindFirst(ClaimTypes.Sid).Value);
            post.LastEditUserName = User.FindFirst(ClaimTypes.Name).Value;
            _dbContext.Add(post);
            if (_dbContext.SaveChanges() > 0)
                return Ok(new { code = StatusCodes.Status200OK, message = "添加成功" });
            else
                return Ok(new { code = StatusCodes.Status400BadRequest, message = "添加失败" });
        }
        /// <summary>
        /// 修改岗位信息
        /// </summary>
        /// <param name="post"></param>
        /// <returns></returns>
        [HttpPut, Route("PutPost")]
        // [RequirePermission]
        public IActionResult PutPost(Post post)
        {
            if (_dbContext.Post.Any(w => w.Name.Equals(post.Name) && w.Id != post.Id))
            {
                return Ok(new { code = StatusCodes.Status400BadRequest, message = "岗位名称已经存在，请不要重复添加" });
            }
            var query = _dbContext.Post.Find(post.Id);

                query.Name = post.Name;
                query.InUse = post.InUse;
                query.PostSequenceID = post.PostSequenceID;
                query.BussinessLineID = post.BussinessLineID;
                query.RankRangeMin = post.RankRangeMin;
                query.RankRangeMax = post.RankRangeMax;

            _dbContext.Update(query);
            if (_dbContext.SaveChanges() > 0)
                return Ok(new { code = StatusCodes.Status200OK, message = "更新成功" });
            else
                return Ok(new { code = StatusCodes.Status400BadRequest, message = "更新失败" });
        }

        /// <summary>
        /// 删除岗位信息
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete, Route("DeletePost")]
        public IActionResult DeletePost(int id)
        {
            var query = _dbContext.Post.Find(id);
            query.InUse = false;
            _dbContext.Update(query);
            if (_dbContext.SaveChanges() > 0)
                return Ok(new { code = StatusCodes.Status200OK, message = "删除成功" });
            else
                return Ok(new { code = StatusCodes.Status400BadRequest, message = "删除失败" });
        }

        /// <summary>
        /// 根据ID获取岗位信息
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet,Route("GetPostById")]
        public IActionResult GetPostById(int? id)
        {
            if(id.HasValue){
                var query = _dbContext.Post.FirstOrDefault(w => w.Id.Equals(id.Value));
                if(query != null)
                {
                    #region 获取字典信息
                    List<int> dict_ids = new List<int>();
                    if (!string.IsNullOrWhiteSpace(query.PostSequenceID.ToString()))
                        dict_ids.Add(query.PostSequenceID);
                    if (!string.IsNullOrWhiteSpace(query.BussinessLineID.ToString()))
                        dict_ids.Add(query.BussinessLineID);
                    var Dictionaries = _dbContext.Dictionary.Where(t => dict_ids.Contains(t.Id)).ToList();
                    #endregion
                    return Ok(new { code = StatusCodes.Status200OK, data = query, dictionaries = Dictionaries });
                }
                else
                    return Ok(new { code = StatusCodes.Status400BadRequest, message = "数据为空" });
            }
            else
                return Ok(new { code = StatusCodes.Status400BadRequest, message = "获取失败" });

        }
    }
}
