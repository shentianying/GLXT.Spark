using GLXT.Spark.Entity;
using GLXT.Spark.Entity.ZSGL;
using GLXT.Spark.IService;
using GLXT.Spark.ViewModel.ZSGL;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GLXT.Spark.Controllers.ZSGL
{
    /// <summary>
    /// 意向企业
    /// </summary>
    [Route("api/ZSGL/[controller]")]
    [ApiController]
    [Authorize]
    public class EnterpriseController : BaseController
    {
        private readonly DBContext _dbContext;
        private readonly ICommonService _commonService;
        private readonly ISystemService _systemService;
        public EnterpriseController(DBContext dbContext, ICommonService commonService, ISystemService systemService)
        {
            _dbContext = dbContext;
            _commonService = commonService;
            _systemService = systemService;
        }

        /// <summary>
        /// 获取分页列表
        /// </summary>
        /// <returns></returns>
        [HttpPost, Route("GetEnterprisePaging")]
        public IActionResult GetEnterprisePaging(EnterpriseSearchViewModel esvm)
        {
            int companyId = _systemService.GetCurrentSelectedCompanyId();
            IQueryable<Enterprise> query = _dbContext.Enterprise
                .Where(w => w.CompanyId.Equals(companyId));

            if (!string.IsNullOrEmpty(esvm.name))
                query = query.Where(w => w.CompanyName.Contains(esvm.name));

            if(esvm.operationStates?.Length>0)
                query = query.Where(w => esvm.operationStates.Contains(w.OperationState));

            if (esvm.amount1.HasValue)
                query = query.Where(w => w.RegCapital >= esvm.amount1);

            if (esvm.amount2.HasValue)
                query = query.Where(w => w.RegCapital <= esvm.amount2);

            // 分页
            if (esvm.currentPage == 0 || esvm.pageSize == 0)
            {
                return Ok(new { code = StatusCodes.Status400BadRequest, errorMsg = "页码与页数数值需正确！" });
            }
            else
            {
                int count = query.Count();
                var query_result = query.Skip((esvm.currentPage - 1) * esvm.pageSize)
                    .Take(esvm.pageSize);
                //判断是否有数据，若无则返回第一页
                if (query_result.Count() == 0)
                {
                    esvm.currentPage = 1;
                    query_result = query.Skip((esvm.currentPage - 1) * esvm.pageSize)
                        .Take(esvm.pageSize);
                }

                var operationStateList = _systemService.GetDictionary("OperationState");//经营状态

                List<object> result = new List<object>();
                foreach (var q in query_result)
                {
                    result.Add(new
                    {
                        q.Id,
                        q.CompanyName,
                        q.SetDate,
                        q.LegalPerson,
                        operationState = operationStateList.FirstOrDefault(t => t.Value.Equals(q.OperationState))?.Name,
                        q.EmployeeNum,
                        q.Output,
                        q.RegCapital,
                        q.LinkMan,
                        q.LinkTel,
                        q.OfficialNet,
                        q.Email,
                        q.InUse,
                        q.CreateUserName,
                        q.CreateDate,
                        q.LastEditUserName,
                        q.LastEditDate
                    });
                }


                return Ok(new
                {
                    code = StatusCodes.Status200OK,
                    data = result,
                    count = count,
                    operationStateList = operationStateList
                });
            }
        }

        /// <summary>
        /// 意向企业列表
        /// </summary>
        /// <param></param>
        /// <returns></returns>
        [HttpGet, Route("GetEnterpriseList")]
        //[RequirePermission]
        public IActionResult GetEnterpriseList(int? type, string name = "")
        {
            int companyId = _systemService.GetCurrentSelectedCompanyId();
            var query = _dbContext.Enterprise
                .Where(w => w.CompanyId.Equals(companyId));
            // 名称搜索
            if (!string.IsNullOrWhiteSpace(name))
                query = query.Where(w => w.CompanyName.Contains(name));
            if (type.HasValue)
                query = query.Where(w => w.EnterpriseType.Equals(type.Value));

            var enterpriseTypeList = _systemService.GetDictionary("EnterpriseType");

            var result = query.Select(s => new
            {
                s.Id,
                s.CompanyName,
                s.LegalPerson,
                s.LinkMan,
                s.LinkTel
            }).AsNoTracking().ToList();
            return Ok(new { code = StatusCodes.Status200OK, data = result, enterpriseTypeList = enterpriseTypeList });
        }

        /// <summary>
        /// 初始化编辑页面
        /// </summary>
        /// <param></param>
        /// <returns></returns>
        [HttpGet, Route("InitEnterprise")]
        //[RequirePermission]
        public IActionResult InitEnterprise()
        {

            var operationStateList = _systemService.GetDictionary("OperationState");//            
            var enterpriseTypeList = _systemService.GetDictionary("EnterpriseType");//
            return Ok(new
            {
                code = StatusCodes.Status200OK,
                operationStateList = operationStateList,
                enterpriseTypeList = enterpriseTypeList
            });
        }

        /// <summary>
        /// 编辑页面 根据id获取意向企业信息
        /// </summary>
        /// <param name="id">意向企业id</param>
        /// <returns></returns>
        [HttpGet, Route("GetEnterpriseById")]
        //[RequirePermission]
        public IActionResult GetEnterpriseById(int id)
        {
            var enterprise = _dbContext.Enterprise
                  .FirstOrDefault(w => w.Id.Equals(id));

            if (enterprise == null)
            {
                return Ok(new { code = StatusCodes.Status400BadRequest, message = "数据为空" });
            }

            return Ok(new
            {
                code = StatusCodes.Status200OK,
                data = enterprise
            });
        }

        /// <summary>
        /// 添加意向企业
        /// </summary>
        /// <param name="enterprise">对象</param>
        /// <returns></returns>
        [HttpPost, Route("AddEnterprise")]
        //[RequirePermission]
        public IActionResult AddEnterprise(Enterprise enterprise)
        {
            enterprise.CreateUserId = GetUserId();
            enterprise.CreateUserName = GetUserName();
            enterprise.LastEditUserId = GetUserId();
            enterprise.LastEditUserName = GetUserName();
            _dbContext.Add(enterprise);
            if (_dbContext.SaveChanges() > 0)
                return Ok(new { code = StatusCodes.Status200OK, message = "操作成功", data = enterprise });

            else
                return Ok(new { code = StatusCodes.Status400BadRequest, message = "添加失败" });
        }

        /// <summary>
        /// 更新
        /// </summary>
        /// <param name="enterprise"></param>
        /// <returns></returns>
        [HttpPut, Route("PutEnterprise")]
        public IActionResult PutEnterprise(Enterprise enterprise)
        {

            var query1 = _dbContext.Enterprise.Find(enterprise.Id);

            if (query1 != null)
            {
                query1.CompanyName = enterprise.CompanyName;
                query1.LegalPerson = enterprise.LegalPerson;
                query1.EmployeeNum = enterprise.EmployeeNum;
                query1.Output = enterprise.Output;
                query1.Tax = enterprise.Tax;
                query1.LinkMan = enterprise.LinkMan;
                query1.LinkTel = enterprise.LinkTel;
                query1.OfficialNet = enterprise.OfficialNet;
                query1.Email = enterprise.Email;
                query1.Area = enterprise.Area;
                query1.Address = enterprise.Address;
                query1.FormerName = enterprise.FormerName;
                query1.OperationState = enterprise.OperationState;
                query1.RegCapital = enterprise.RegCapital;
                query1.PaidCapital = enterprise.PaidCapital;
                query1.Occupation = enterprise.Occupation;
                query1.UniSocialCreditCode = enterprise.UniSocialCreditCode;
                query1.TaxNum = enterprise.TaxNum;
                query1.BusinessLicense = enterprise.BusinessLicense;
                query1.OrgCode = enterprise.OrgCode;
                query1.SetDate = enterprise.SetDate;
                query1.StartDate = enterprise.StartDate;
                query1.EndDate = enterprise.EndDate;
                query1.EnterpriseType = enterprise.EnterpriseType;
                query1.CheckDate = enterprise.CheckDate;
                query1.InUse = enterprise.InUse;
                query1.Remark = enterprise.Remark;
                query1.LastEditUserId = GetUserId();
                query1.LastEditUserName = GetUserName();
                query1.LastEditDate = DateTime.Now;

                _dbContext.Update(query1);
                if (_dbContext.SaveChanges() > 0)
                    return Ok(new { code = StatusCodes.Status200OK, message = "操作成功", data = enterprise });
                else
                    return Ok(new { code = StatusCodes.Status400BadRequest, message = "更新失败" });
            }
            else
            {
                return Ok(new { code = StatusCodes.Status400BadRequest, message = "查无此单据" });
            }
        }

        /// <summary>
        /// 作废
        /// </summary>
        /// <param name="id">意向企业id</param>
        /// <returns></returns>
        [HttpDelete, Route("DeleteEnterprise")]
        public IActionResult DeleteEnterprise(int? id)
        {
            if (id.HasValue)
            {
                var q1 = _dbContext.Enterprise
                    .FirstOrDefault(w => w.Id.Equals(id));
                q1.InUse = false;
                _dbContext.Update(q1);
                if (_dbContext.SaveChanges() > 0)
                    return Ok(new { code = StatusCodes.Status200OK, message = "操作成功" });
                else
                    return Ok(new { code = StatusCodes.Status400BadRequest, message = "操作失败" });
            }
            else
                return Ok(new { code = StatusCodes.Status400BadRequest, message = "参数错误" });
        }
    }
}
