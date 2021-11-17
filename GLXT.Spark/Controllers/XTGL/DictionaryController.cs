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

namespace GLXT.Spark.Controllers.XTGL
{
    /// <summary>
    /// 字典数据
    /// </summary>
    [Route("api/XTGL/[controller]")]
    [ApiController]
    [Authorize]
    public class DictionaryController : BaseController
    {
        private readonly DBContext _dbContext;
        private readonly ICommonService _commonService;
        private readonly ISystemService _systemService;
        public DictionaryController(DBContext dbContext, ICommonService commonService,ISystemService systemService)
        {
            _commonService = commonService;
            _dbContext = dbContext;
            _systemService = systemService;
        }

        /// <summary>
        /// 字典列表分页
        /// </summary>
        /// <returns></returns>
        [HttpGet, Route("GetDictionaryPaging")]
        [RequirePermission]
        public IActionResult GetDictionaryPaging(int currentPage, int pageSize, string type = "")
        {
            var query = _dbContext.Dictionary
                .OrderBy(o => o.Type)
                .ThenBy(t => t.Sort)
                .Where(w => w.InUse);
            if (!string.IsNullOrWhiteSpace(type))
                query = query.Where(w => w.Type.Contains(type) || w.TypeName.Contains(type));

            int count = query.Count();
            var result = query.Skip((currentPage - 1) * pageSize)
                .Take(pageSize);
            //判断是否有数据，若无则返回第一页
            if (result.Count() == 0)
            {
                currentPage = 1;
                result = query.Skip((currentPage - 1) * pageSize)
                .Take(pageSize);
            }

            var q1 = result.GroupJoin(_dbContext.Organization, dic => dic.CompanyId, org => org.Id, (dic, org) => new { Dictionary = dic, Organization = org })
                .SelectMany(s => s.Organization.DefaultIfEmpty(),
                (dic, org) => new
                {
                    dic.Dictionary.Id,
                    dic.Dictionary.Name,
                    dic.Dictionary.PId,
                    dic.Dictionary.Type,
                    dic.Dictionary.Value,
                    companyName = org.Name,
                    dic.Dictionary.InUse,
                    dic.Dictionary.Remark,
                    dic.Dictionary.Sort,
                    dic.Dictionary.TypeName,
                    dic.Dictionary.CompanyId
                });

            //获取类型分组后数据
            var typeList = _dbContext.Dictionary
                .GroupBy(g => new { g.Type,g.TypeName })
                .Select(s => new { s.Key.Type,s.Key.TypeName })
                .ToList();
            return Ok(new { code = StatusCodes.Status200OK, data1 = q1.ToList(), data2 = typeList, count = count });
        }
        /// <summary>
        /// 根据类型获取字典列表
        /// </summary>
        /// <param name="type"></param>
        /// <param name="companyId"></param>
        /// <returns></returns>
        [HttpGet, Route("GetDictionaryList")]
        public IActionResult GetDictionaryList([FromQuery] string type = "", [FromQuery] int companyId = 0)
        {
            var query = _systemService.GetDictionary(type);
            return Ok(new { code = StatusCodes.Status200OK, data = query });
        }
        /// <summary>
        /// 获取字典类型分组列表
        /// </summary>
        /// <returns></returns>
        [HttpGet, Route("GetDictionaryGroupType")]
        public IActionResult GetDictionaryGroupType()
        {
            var typeList = _dbContext.Dictionary
                .GroupBy(g => g.Type)
                .Select(s => s.Key)
                .ToList();
            return Ok(new { code = StatusCodes.Status200OK, data = typeList });
        }
        /// <summary>
        /// 编辑 初始化信息
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        [HttpGet, Route("InitDictionary")]
        public IActionResult InitDictionary([FromQuery] string type = "")
        {
            var query1 = _systemService.GetDictionary(type);
            var query2 = _systemService.GetOrganizationList(new Organization { PId = 0 });
            return Ok(new { code = StatusCodes.Status200OK, data1 = query1, data2 = query2 });
        }

        /// <summary>
        /// 添加字典列表
        /// </summary>
        /// <param name="dictionaryData"></param>
        /// <returns></returns>
        [HttpPost, Route("AddDictionary")]
        [RequirePermission]
        public IActionResult AddDictionary(Dictionary dictionaryData)
        {
            var query = _dbContext.Dictionary.Any(w => w.InUse
            && w.Value.Equals(dictionaryData.Value)
            && w.Type.Equals(dictionaryData.Type));
            if (query)
                return Ok(new { code = StatusCodes.Status400BadRequest, message = "同一个类型type下,value值不能重复" });

            var q1 = _dbContext.Dictionary.FirstOrDefault(w => w.Type.Equals(dictionaryData.Type) && w.TypeName != "");
            if (q1 != null)
                dictionaryData.TypeName = q1.TypeName;
            else
                dictionaryData.TypeName = "";
            
            _dbContext.Add(dictionaryData);
            if (_dbContext.SaveChanges() > 0)
            {
                _commonService.RemoveCache<Dictionary>(); // 移除缓存
                return Ok(new { code = StatusCodes.Status200OK, message = "添加成功" });
            }
            else
                return Ok(new { code = StatusCodes.Status400BadRequest, message = "添加失败" });
        }
        /// <summary>
        /// 更新字典列表
        /// </summary>
        /// <param name="dictionaryData"></param>
        /// <returns></returns>
        [HttpPut, Route("PutDictionary")]
        [RequirePermission]
        public IActionResult PutDictionary(Dictionary dictionaryData)
        {
            var q = _dbContext.Dictionary.Any(w => w.InUse
            && w.Value.Equals(dictionaryData.Value)
            && w.Type.Equals(dictionaryData.Type)
            && w.Id != dictionaryData.Id
            );
            if (q)
                return Ok(new { code = StatusCodes.Status400BadRequest, message = "同一个类型type下,value值不能重复" });

            var query = _dbContext.Dictionary.Find(dictionaryData.Id);
            query.PId = dictionaryData.PId;
            query.Name = dictionaryData.Name;
            query.Remark = dictionaryData.Remark;
            query.CompanyId = dictionaryData.CompanyId;
            query.Type = dictionaryData.Type;
            var q1 = _dbContext.Dictionary.FirstOrDefault(w => w.Type.Equals(dictionaryData.Type) && w.TypeName != "");
            if(q1!=null)
            {
                query.TypeName = q1.TypeName;
            }else
            {
                query.TypeName = "";
            }
            //query.TypeName = dictionaryData.TypeName;
            query.Value = dictionaryData.Value;
            query.Sort = dictionaryData.Sort;
            query.InUse = dictionaryData.InUse;
            _dbContext.Update(query);
            if (_dbContext.SaveChanges() > 0)
            {
                _commonService.RemoveCache<Dictionary>(); // 移除缓存
                return Ok(new { code = StatusCodes.Status200OK, message = "更新成功" });
            }
            else
                return Ok(new { code = StatusCodes.Status400BadRequest, message = "更新失败" });
        }
        /// <summary>
        /// 根据类型修改类型名称
        /// </summary>
        /// <param name="o">动态类型</param>
        /// <returns></returns>
        [HttpPut, Route("PutDictionaryTypeName")]
        public IActionResult PutDictionaryTypeName(dynamic o)
        {
            string type = o["type"];
            string typeName = o["typeName"];
            var list = _dbContext.Dictionary.Where(w => w.Type.Equals(type)).ToList();
            foreach (var dic in list)
            {
                dic.TypeName = typeName;
                _dbContext.Update(dic);
            }
            if (_dbContext.SaveChanges() > 0)
            {
                _commonService.RemoveCache<Dictionary>(); // 移除缓存
                return Ok(new { code = StatusCodes.Status200OK, message = "更新成功" });
            }
            else
                return Ok(new { code = StatusCodes.Status400BadRequest, message = "更新失败" });
        }
        /// <summary>
        /// 删除字典列表
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete, Route("DeleteDictionary")]
        public IActionResult DeleteDictionary(int id)
        {
            var query = _dbContext.Dictionary.Find(id);
            query.InUse = false;
            _dbContext.Update(query);
            if (_dbContext.SaveChanges() > 0)
            {
                _commonService.RemoveCache<Dictionary>(); // 移除缓存
                return Ok(new { code = StatusCodes.Status200OK, message = "删除成功" });
            }
            else
                return Ok(new { code = StatusCodes.Status400BadRequest, message = "删除失败" });
        }

        /// <summary>
        /// 根据类型串获取字典列表
        /// </summary>
        /// <param name="types">类型列表以,分隔</param>
        /// <returns></returns>
        [HttpPost, Route("GetDictionarysByTypes")]
        public IActionResult GetDictionarysByTypes([FromQuery] string types)
        {
            List<string> paras = types.Split(",").ToList();
            var query = _dbContext.Dictionary.Where(t => types.Contains(t.Type)).ToList();
            return Ok(new { code = StatusCodes.Status200OK, data = query });
        }

    }
}