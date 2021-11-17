using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using GLXT.Spark.Entity;
using GLXT.Spark.Entity.RSGL;
using GLXT.Spark.Entity.XTGL;
using GLXT.Spark.Enums;
using GLXT.Spark.IService;
using GLXT.Spark.Model;
using GLXT.Spark.Model.Flow;
using GLXT.Spark.Utils;

namespace GLXT.Spark.Service
{
    public class CommonService : ICommonService
    {
        private readonly DBContext _dbContext;
        private readonly IMapper _mapper;
        private readonly IMemoryCache _memoryCache;
        private readonly TokenManagement _tokenManagement;
        private readonly AppSettingModel _appSettingModel;
        private readonly IPrincipalAccessor _principalAccessor;
        //private readonly ISystemService _systemService;

        public CommonService(
            DBContext dBContext,
            IMapper mapper,
            IMemoryCache memoryCache,
            IOptions<TokenManagement> tokenManagement,
            IOptions<AppSettingModel> appSettingModel,
            //ISystemService systemService,
            IPrincipalAccessor principalAccessor)
        {
            _dbContext = dBContext;
            _mapper = mapper;
            _memoryCache = memoryCache;
            _principalAccessor = principalAccessor;
            _tokenManagement = tokenManagement.Value;
            _appSettingModel = appSettingModel.Value;
            //_systemService = systemService;
        }


        #region rsgl 人事管理

        #region Person 人员信息

        #region 生成工号
        private static object _lock = new object();
        /// <summary>
        /// 人员信息生成工号
        /// </summary>
        /// <returns></returns>
        public string GenerateUserNumber()
        {
            lock (_lock)
            {
                if (_dbContext.Person.Any())
                {
                    return _dbContext.Person.Max(m => int.Parse(m.Number)).ToString();
                }
                else
                    return "100000";
            };
        }
        #endregion
        public List<Person> GetPersonList()
        {
            var persons = _dbContext.Person
                .Include(i => i.Post)
                .Where(w => w.InUse);

            return persons.ToList();
        }
        /// <summary>
        /// 查询对应组织机构下的人员组成
        /// </summary>
        /// <param name="orgId"></param>
        /// <param name="postId"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public List<PersonPost> GetPersonListByOrgId(int orgId,int postId=0,string name="")
        {
            var persons = _dbContext.PersonPost
                .Include(i => i.Post)
                .Include(i=> i.Person)
                .Where(w => w.OrgId.Equals(orgId) && w.InUse); 
            if (!string.IsNullOrWhiteSpace(name))
            {
                persons = persons.Where(w => w.Person.Name.Contains(name) || w.Person.Number.Contains(name));
            }
            if (postId > 0)
            {
                persons = persons.Where(w => w.PostId.Equals(postId));
            }
            

            return persons.ToList();
        }

        /// <summary>
        /// 根据人员信息查询对应的组织机构
        /// </summary>
        /// <param name="postId"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public List<int> GetPersonOrgIds(int postId = 0, string name = "")
        {
            var persons = _dbContext.PersonPost
                .Include(i => i.Post)
                .Include(i => i.Person)
                .Where(w =>w.InUse);
            if (!string.IsNullOrWhiteSpace(name))
            {
                persons = persons.Where(w => w.Person.Name.Contains(name) || w.Person.Number.Contains(name));
            }
            if (postId > 0)
            {
                persons = persons.Where(w => w.PostId.Equals(postId));
            }

            List<int> childOrgs = new List<int>();
            foreach (var person in persons)
            {
                childOrgs.Add(person.OrgId);
            }
            

            return childOrgs;
        }

        /// <summary>
        /// 查询对应组织机构下的岗位
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public List<PostPoolDetail> GetPostListByOrgId(int id)
        {
            var query = _dbContext.Organization.Find(id);
            if (query.CategoryId == 1)
            {
                var postData0 = _dbContext.PostPoolDetail
                    .Include(i => i.Post)
                    .Include(t => t.PostPool)
                    .AsNoTracking()
                    .Where(w => w.PostPool.OrgId.Equals(id) && w.InUse);

                return postData0.ToList();
            }
            else
            {
                var postData1 = _dbContext.PostPoolDetail
                    .Include(i => i.Post)
                    .Include(t => t.PostPool)
                    .AsNoTracking()
                    .Where(w => w.PostPool.Category.Equals(query.CategoryId) && w.InUse);
                return postData1.ToList();
            }
        }

        #endregion

        #region Post 岗位信息
        public List<Post> GetPostList(string name = "")
        {
            IQueryable<Post> query = _dbContext.Post
                .OrderBy(o => o.RankRangeMin)
                .Where(w => w.InUse);
            if (!string.IsNullOrWhiteSpace(name))
            {
                query = query.Where(w => w.Name.Contains(name));
            }
            return query.ToList();
        }

        public string GetPostNameById(int id)
        {
            var query = _dbContext.Post
                .AsNoTracking()
                .FirstOrDefault(w => w.Id.Equals(id));
            if (query == null)
                return "";
            else
                return query.Name;

        }
        #endregion

        #region PostPool 岗位池信息
        public PostPool GetPostPoolById(int id)
        {
            var query = _dbContext
                .PostPool
                .AsNoTracking()
                .FirstOrDefault(w => w.Id.Equals(id));
            if (query == null)
                return null;

            var query2 = _dbContext
                .PostPoolDetail
                .Include(t => t.Post)
                .AsNoTracking()
                .Where(w => w.PostPoolId.Equals(id) && w.InUse);

            query.PostPoolDetail = query2.ToList();

            return query;
        }

        public List<PostPool> GetPostPoolList(string name = "")
        {
            var query = _dbContext.PostPool
                .Where(w => w.InUse);
            if (!string.IsNullOrWhiteSpace(name))
            {
                query = query.Where(w => w.Name.Contains(name));
            }

            return query.ToList();
        }

        public List<PostPoolDetail> GetPostPoolDetailByOrg(int orgId) 
        {
            var query = _dbContext.Organization.Find(orgId);
            if (query.CategoryId == 1)
            {
                var postData = _dbContext.PostPoolDetail
                    .Include(i => i.Post)
                    .Include(t => t.PostPool)
                    .AsNoTracking()
                    .Where(w => w.PostPool.OrgId.Equals(orgId));

                return postData.ToList();
            }
            else
            {
                var postData = _dbContext.PostPoolDetail
                    .Include(i => i.Post)
                    .Include(t => t.PostPool)
                    .AsNoTracking()
                    .Where(w => w.PostPool.Category.Equals(query.CategoryId));
                return postData.ToList();
            }
        }
        #endregion

        #region 用户岗位

        /// <summary>
        /// 根据人员id查询其他岗位信息（不包括主岗位）
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public List<PersonPost> GetOtherPersonPostById(int id)
        {
            var query = _dbContext.PersonPost
                .Include(i =>i.Post)
                .Include(i => i.Role)
                .Include(i => i.Organization)
                .Include(i => i.PostPoolDetail)
                .ThenInclude(t =>t.PostPool)
                .Include(i => i.PostPoolDetail)
                .ThenInclude(t => t.Post)
                .Where(w => w.PersonId.Equals(id) && w.InUse && !w.IsMain);

            return query.ToList();
        }
        #endregion

        #endregion      

        #region 泛型 缓存方法
        /// <summary>
        /// 查询并缓存人员基本信息
        /// </summary>
        /// <param name="companyId">公司Id</param>
        /// <returns>List</returns>
        public List<Person> GetCachePersonBasicInfoList(int companyId)
        {
            string cacheKey = _appSettingModel.CachePrifix + "Person" + companyId.ToString();
            if (!_memoryCache.TryGetValue(cacheKey, out List<Person> cacheEntry))
            {
                cacheEntry = _dbContext.Person.Where(w => w.CompanyId == companyId).Select(s => s.ToBasicInfo()).AsNoTracking().ToList();
                _memoryCache.Set(cacheKey, cacheEntry, new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(10)));
            }
            return cacheEntry;
        }
        public List<T> GetCacheList<T>() where T : class
        {
            string cacheKey = _appSettingModel.CachePrifix + typeof(T).Name;
            if (!_memoryCache.TryGetValue(cacheKey, out List<T> cacheEntry))
            {
                cacheEntry = _dbContext.Set<T>().AsNoTracking().ToList();
                _memoryCache.Set(cacheKey, cacheEntry, new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromSeconds(30)));
            }
            return cacheEntry;
        }
        public void SetCache<T>(string keyName, List<T> list)
        {
            string cacheKey = _appSettingModel.CachePrifix + typeof(T).Name + keyName;
            _memoryCache.Set(cacheKey, list, new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromSeconds(30)));
        }
        public List<T> GetCache<T>(string keyName)
        {
            string cacheKey = _appSettingModel.CachePrifix + typeof(T).Name + keyName;
            var result = _memoryCache.Get(cacheKey);
            return (List<T>)result;
        }
        public void RemoveCache<T>(string keyName)
        {
            string cacheKey = _appSettingModel.CachePrifix + typeof(T).Name + keyName;
            _memoryCache.Remove(cacheKey);

        }
        public void RemoveCache<T>() where T : class
        {
            string cacheKey = _appSettingModel.CachePrifix + typeof(T).Name;
            _memoryCache.Remove(cacheKey);
        }

        #endregion


    }
}
