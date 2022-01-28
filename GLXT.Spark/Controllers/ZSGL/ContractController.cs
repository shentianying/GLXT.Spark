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
    /// 合同签订
    /// </summary>
    [Route("api/ZSGL/[controller]")]
    [ApiController]
    [Authorize]
    public class ContractController : BaseController
    {
        private readonly DBContext _dbContext;
        private readonly ICommonService _commonService;
        private readonly ISystemService _systemService;
        public ContractController(DBContext dbContext, ICommonService commonService, ISystemService systemService)
        {
            _dbContext = dbContext;
            _commonService = commonService;
            _systemService = systemService;
        }

        /// <summary>
        /// 获取分页列表
        /// </summary>
        /// <returns></returns>
        [HttpPost, Route("GetContractPaging")]
        public IActionResult GetContractPaging(ContractSearchViewModel svm)
        {
            int companyId = _systemService.GetCurrentSelectedCompanyId();
            IQueryable<Contract> query = _dbContext.Contract
                .Include(i => i.Enterprise)
                .Where(w => w.CompanyId.Equals(companyId));
            if (!string.IsNullOrEmpty(svm.name))
                query = query.Where(w => w.Enterprise.CompanyName.Contains(svm.name));

            // 分页
            if (svm.currentPage == 0 || svm.pageSize == 0)
            {
                return Ok(new { code = StatusCodes.Status400BadRequest, errorMsg = "页码与页数数值需正确！" });
            }
            else
            {
                int count = query.Count();
                var query_result = query.Skip((svm.currentPage - 1) * svm.pageSize)
                    .Take(svm.pageSize);
                //判断是否有数据，若无则返回第一页
                if (query_result.Count() == 0)
                {
                    svm.currentPage = 1;
                    query_result = query.Skip((svm.currentPage - 1) * svm.pageSize)
                        .Take(svm.pageSize);
                }

                var operationStateList = _systemService.GetDictionary("OperationState");//经营状态

                List<object> result = new List<object>();
                foreach (var q in query_result)
                {
                    result.Add(new
                    {
                        q.Id,
                        q.Enterprise.CompanyName,
                        q.Enterprise.SetDate,
                        q.Enterprise.LegalPerson,
                        operationState = operationStateList.FirstOrDefault(t => t.Value.Equals(q.Enterprise.OperationState))?.Name,
                        q.Enterprise.LinkMan,
                        q.Enterprise.LinkTel,
                        q.RegionId,
                        q.StartDate,
                        q.EndDate,
                        state = DateTime.Today<q.StartDate?0:(DateTime.Today>q.EndDate?-1:1),
                        q.IsForever,
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
        /// 编辑页面 根据id获取意向企业信息
        /// </summary>
        /// <param name="id">意向企业id</param>
        /// <returns></returns>
        [HttpGet, Route("GetContractById")]
        //[RequirePermission]
        public IActionResult GetContractById(int id)
        {
            var contract = _dbContext.Contract
                  .FirstOrDefault(w => w.Id.Equals(id));

            if (contract == null)
            {
                return Ok(new { code = StatusCodes.Status400BadRequest, message = "数据为空" });
            }

            contract.UpFile = _dbContext.UpFile
                    .Where(w => w.TableId.Equals(id) && w.TableName.Equals(Utils.Common.GetTableName<Contract>()))
                    .AsNoTracking().ToList();

            return Ok(new
            {
                code = StatusCodes.Status200OK,
                data = contract
            });
        }

        /// <summary>
        /// 添加意向企业
        /// </summary>
        /// <param name="contract">对象</param>
        /// <returns></returns>
        [HttpPost, Route("AddContract")]
        //[RequirePermission]
        public IActionResult AddContract(Contract contract)
        {
            //判断是否重复
            int q1 = _dbContext.Contract.Where(w => w.EnterpriseId.Equals(contract.EnterpriseId) && w.RegionId.Equals(contract.RegionId)).Count();
            if (q1 > 0)
                return Ok(new { code = StatusCodes.Status400BadRequest, message = "在该区域与该企业已经签订合同" });

            contract.Number = _systemService.GetNewBillNumber<Contract>("HT" + DateTime.Today.ToString("yyMM"), 4);
            contract.CreateUserId = GetUserId();
            contract.CreateUserName = GetUserName();
            contract.LastEditUserId = GetUserId();
            contract.LastEditUserName = GetUserName();
            _dbContext.Add(contract);
            if (_dbContext.SaveChanges() > 0)
            {
                _systemService.AddFiles<Contract>(contract.FileList, contract.Id);
                return Ok(new { code = StatusCodes.Status200OK, message = "操作成功", data = contract });
            }                
            else
                return Ok(new { code = StatusCodes.Status400BadRequest, message = "添加失败" });
        }

        /// <summary>
        /// 更新
        /// </summary>
        /// <param name="contract"></param>
        /// <returns></returns>
        [HttpPut, Route("PutContract")]
        public IActionResult PutContract(Contract contract)
        {

            var query1 = _dbContext.Contract.Find(contract.Id);

            if (query1 != null)
            {
                query1.StartDate = contract.StartDate;
                query1.EndDate = contract.EndDate;
                query1.IsForever = contract.IsForever;
                query1.LastEditUserId = GetUserId();
                query1.LastEditUserName = GetUserName();
                query1.LastEditDate = DateTime.Now;

                _dbContext.Update(query1);
                if (_dbContext.SaveChanges() > 0)
                {
                    _systemService.UpdateFile<Contract>(contract.FileList, contract.Id);
                    return Ok(new { code = StatusCodes.Status200OK, message = "操作成功", data = contract });
                }                    
                else
                    return Ok(new { code = StatusCodes.Status400BadRequest, message = "更新失败" });
            }
            else
            {
                return Ok(new { code = StatusCodes.Status400BadRequest, message = "查无此单据" });
            }
        }
    }
}
