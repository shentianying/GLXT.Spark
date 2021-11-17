using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using GLXT.Spark.Entity;
using GLXT.Spark.Entity.XTGL;
using GLXT.Spark.Filters;
using GLXT.Spark.IService;
using GLXT.Spark.Utils;

namespace GLXT.Spark.Controllers.XTGL
{
    /// <summary>
    /// 表单
    /// </summary>
    [Route("api/XTGL/[controller]")]
    [ApiController]
    [Authorize]
    public class FormController : BaseController
    {
        private readonly DBContext _dbContext;
        private readonly ICommonService _commonService;
        public FormController(DBContext dbContext, ICommonService commonService)
        {
            _commonService = commonService;
            _dbContext = dbContext;
        }

        #region Form 表单
        [HttpGet, Route("GetFormPaging")]
        [RequirePermission]
        public IActionResult GetFormPaging(int currentPage, int pageSize,string name="")
        {
            var query = _dbContext.Form
                .OrderByDescending(o => o.Id)
                .Include(i=>i.FormFlowField)
                .Include(i=>i.FormState)
                .ThenInclude(t=>t.FormStateOption)
                .Include(i=>i.Page).DefaultIfEmpty()
                .Where(w => w.InUse);
            if (!string.IsNullOrEmpty(name))
            {
                query = query.Where(w => w.Name.Contains(name));
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

            return Ok(new { code = StatusCodes.Status200OK, data = result, count = count });
        }
        [HttpGet, Route("GetFormList")]
        public IActionResult GetFormList(bool? needCheckup)
        {
            var query = _dbContext.Form.OrderByDescending(o=>o.Id).Where(w => w.InUse);
            if(needCheckup.HasValue)
            {
                query = query.Where(w => w.NeedCheckup.Equals(needCheckup.Value));
            }
            return Ok(new { code = StatusCodes.Status200OK, data = query.ToList() });
        }

        [HttpPost, Route("AddForm")]
        [RequirePermission]
        public IActionResult AddForm(Form form)
        {
            if(_dbContext.Form.Any(a => a.Value.Equals(form.Value)&&a.InUse))
            {
                return Ok(new { code = StatusCodes.Status400BadRequest, message = "已经添加过该表了，请不要重复添加" });
            }
            foreach (var item in form.FormState)
            {
                if (form.FormState.Count(a => a.Value.Equals(item.Value))>1)
                {
                    return Ok(new { code = StatusCodes.Status400BadRequest, message = $"表单状态{item.Name}的值【{item.Value}】不能重复" });
                }
            }
            form.FormFlowField.ForEach(item =>
            {
                item.CreateUserId = GetUserId();
                item.CreateUserName = GetUserName();
                item.LastEditUserId = GetUserId();
                item.LastEditUserName = GetUserName();
            });
            //form.FormState.ForEach(item =>
            //{
            //    item.FormStateOption
            //});
            form.CreateUserId = GetUserId();
            form.CreateUserName = GetUserName();
            form.LastEditUserId = GetUserId();
            form.LastEditUserName = GetUserName();
            _dbContext.Add(form);
            if (_dbContext.SaveChanges() > 0)
            {
                form.FormState.ForEach(ft =>
                {
                    if (ft.FormStateOption != null)
                    {
                        foreach (var item in ft.FormStateOption)
                        {
                            item.FormId = form.Id;
                            _dbContext.Update(item);
                        }
                    }
                });
                _dbContext.SaveChanges();
                return Ok(new { code = StatusCodes.Status200OK, message = "添加成功" });
            }
            else
                return Ok(new { code = StatusCodes.Status400BadRequest, message = "添加失败" });
        }
        [HttpPut, Route("PutForm")]
        [RequirePermission]
        public IActionResult PutForm(Form form)
        {
            if (_dbContext.Form.Any(a => a.Value.Equals(form.Value)&&a.Id!=form.Id&&a.InUse))
            {
                return Ok(new { code = StatusCodes.Status400BadRequest, message = "已经添加过该表了，请不要重复添加" });
            }
            foreach (var item in form.FormState)
            {
                if (form.FormState.Count(a => a.Value.Equals(item.Value)) > 1)
                {
                    return Ok(new { code = StatusCodes.Status400BadRequest, message = $"表单状态{item.Name}的值【{item.Value}】不能重复" });
                }
            }

            // 修改主表单
            var query = _dbContext.Form
                .Include(i => i.FormFlowField)
                .Include(i => i.FormState).ThenInclude(t => t.FormStateOption)
                .FirstOrDefault(f => f.Id.Equals(form.Id));
            if (query == null)
            {
                return Ok(new { code = StatusCodes.Status400BadRequest, message = "该表单不存在" });
            }

            query.Name = form.Name;
            query.NeedCheckup = form.NeedCheckup;
            query.InUse = form.InUse;
            query.PageId = form.PageId;
            query.Value = form.Value;
            query.LastEditUserId = GetUserId();
            query.LastEditUserName = GetUserName();
            query.LastEditDate = DateTime.Now;

            //----------------修改字段表---------------
            if (form.FormFlowField.Count() > 0)
            {
                foreach (var fff in form.FormFlowField)
                {
                    // id=0 是新增的项目
                    if (fff.Id.Equals(0))
                    {
                        query.FormFlowField.Add(fff);
                    }
                    else // id>0 修改项目
                    {
                        var q1 = query.FormFlowField.FirstOrDefault(f => f.Id.Equals(fff.Id));
                        q1.Field = fff.Field;
                        q1.DicType = fff.DicType;
                        q1.FieldName = fff.FieldName;
                        q1.FieldType = fff.FieldType;
                        q1.InUse = fff.InUse;
                        _dbContext.Update(q1);
                    }
                }
            }
            // 删除的字段
            var q3 = query.FormFlowField.Where(w => !form.FormFlowField.Any(a => a.Id.Equals(w.Id)));
            _dbContext.RemoveRange(q3);

            //----------------修改表单状态表---------------
            if (form.FormState.Count() > 0)
            {
                foreach (var fs in form.FormState)
                {
                    // id=0 是新增的项目
                    if (fs.Id.Equals(0))
                    {
                        query.FormState.Add(fs);
                    }
                    else // id>0 修改项目
                    {
                        var q2 = query.FormState.SelectMany(s=>s.FormStateOption).Where(w => w.FormStateId.Equals(fs.Id));
                        if(q2.Any())
                        {
                            _dbContext.RemoveRange(q2);
                        }
                        var q1 = query.FormState.FirstOrDefault(f => f.Id.Equals(fs.Id));
                        q1.Name = fs.Name;
                        q1.Value = fs.Value;
                        q1.IsCheckup = fs.IsCheckup;
                        q1.IsFlowNode = fs.IsFlowNode;
                        q1.InUse = fs.InUse;
                        foreach (var item in fs.FormStateOption)
                        {
                            item.FormId = q1.FormId;
                        }
                        q1.FormStateOption = fs.FormStateOption;
                        
                        _dbContext.Update(q1);
                    }
                }
            }
            // 删除的字段
            var q4 = query.FormState.Where(w => !form.FormState.Any(a => a.Id.Equals(w.Id)));
            _dbContext.RemoveRange(q4);

            _dbContext.Form.Update(query);
            if (_dbContext.SaveChanges() > 0)
                return Ok(new { code = StatusCodes.Status200OK, message = "更新成功" });
            else
                return Ok(new { code = StatusCodes.Status400BadRequest, message = "更新失败" });
        }
        #endregion

        #region FormFlowField 表单流程字段

        [HttpGet, Route("GetFormFlowFieldPaging")]
        public IActionResult GetFormFlowFieldPaging(int currentPage, int pageSize, int? formId)
        {
            var query = _dbContext.FormFlowField
                .OrderByDescending(o => o.Id)
                .Where(w => w.InUse);
            if (formId.HasValue)
            {
                query = query.Where(w => w.FormId.Equals(formId));
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

            return Ok(new { code = StatusCodes.Status200OK, data = result, count = count });
        }
        [HttpGet, Route("GetFormFlowFieldList")]
        public IActionResult GetFormFlowFieldList()
        {
            var query = _dbContext.FormFlowField.Where(w => w.InUse);
            return Ok(new { code = StatusCodes.Status200OK, data = query.ToList() });
        }

        [HttpPost, Route("AddFormFlowField")]
        //[RequirePermission]
        public IActionResult AddFormFlowField(FormFlowField formFlowField)
        {
            formFlowField.CreateUserId = GetUserId();
            formFlowField.CreateUserName = GetUserName();
            formFlowField.LastEditUserId = GetUserId();
            formFlowField.LastEditUserName = GetUserName();
            _dbContext.Add(formFlowField);
            if (_dbContext.SaveChanges() > 0)
            {
                return Ok(new { code = StatusCodes.Status200OK, message = "添加成功" });
            }
            else
                return Ok(new { code = StatusCodes.Status400BadRequest, message = "添加失败" });
        }
        [HttpPut, Route("PutFormFlowField")]
        //[RequirePermission]
        public IActionResult PutFormFlowField(FormFlowField formFlowField)
        {
            var query = _dbContext.FormFlowField.Find(formFlowField.Id);

                query.Field = formFlowField.Field;
            
                query.FieldName = formFlowField.FieldName;
            
                query.FieldType = formFlowField.FieldType;

                query.InUse = formFlowField.InUse;
            query.LastEditUserId = GetUserId();
            query.LastEditUserName = GetUserName();
            query.LastEditDate = DateTime.Now;
            _dbContext.Update(query);
            if (_dbContext.SaveChanges() > 0)
                return Ok(new { code = StatusCodes.Status200OK, message = "更新成功" });
            else
                return Ok(new { code = StatusCodes.Status400BadRequest, message = "更新失败" });
        }
        #endregion

        #region FormState 表单流程状态
        [HttpGet, Route("GetFormStateList")]
        public IActionResult GetFormStateList(int flowId)
        {
            int formId = _dbContext.Flow.Find(flowId).FormId;
            var query = _dbContext.FormState
                .Where(w => w.InUse && w.FormId.Equals(formId))
                .OrderBy(o => o.Value)
                .ToList();
            return Ok(new { code = StatusCodes.Status200OK, data = query.ToList() });
        }

        [HttpPost, Route("AddFormState")]
        //[RequirePermission]
        public IActionResult AddFormState(FormState formState)
        {
            //formState.CreateUserId = GetUserId();
            //formState.CreateUserName = GetUserName();
            //formState.LastEditUserId = GetUserId();
            //formState.LastEditUserName = GetUserName();
            _dbContext.Add(formState);
            if (_dbContext.SaveChanges() > 0)
            {
                return Ok(new { code = StatusCodes.Status200OK, message = "添加成功" });
            }
            else
                return Ok(new { code = StatusCodes.Status400BadRequest, message = "添加失败" });
        }
        [HttpPut, Route("PutFormState")]
        //[RequirePermission]
        public IActionResult PutFormState(FormState formState)
        {
            var query = _dbContext.FormFlowField.Find(formState.Id);

            //query.Field = formState.Field;

            //query.FieldName = formState.FieldName;

            //query.FieldType = formState.FieldType;

            //query.InUse = formState.InUse;
            //query.LastEditUserId = GetUserId();
            //query.LastEditUserName = GetUserName();
            //query.LastEditDate = DateTime.Now;
            _dbContext.Update(query);
            if (_dbContext.SaveChanges() > 0)
                return Ok(new { code = StatusCodes.Status200OK, message = "更新成功" });
            else
                return Ok(new { code = StatusCodes.Status400BadRequest, message = "更新失败" });
        }
        #endregion

        #region FormState 表单流程状态
        [HttpGet, Route("GetFormStateOptionList")]
        public IActionResult GetFormStateOptionList()
        {
            var query = _dbContext.FormStateOption.Where(w => w.InUse);
            return Ok(new { code = StatusCodes.Status200OK, data = query.ToList() });
        }

        [HttpPost, Route("AddFormStateOption")]
        //[RequirePermission]
        public IActionResult AddFormStateOption(FormStateOption formStateOption)
        {
            //formState.CreateUserId = GetUserId();
            //formState.CreateUserName = GetUserName();
            //formState.LastEditUserId = GetUserId();
            //formState.LastEditUserName = GetUserName();
            _dbContext.Add(formStateOption);
            if (_dbContext.SaveChanges() > 0)
            {
                return Ok(new { code = StatusCodes.Status200OK, message = "添加成功" });
            }
            else
                return Ok(new { code = StatusCodes.Status400BadRequest, message = "添加失败" });
        }
        [HttpPut, Route("PutFormStateOption")]
        //[RequirePermission]
        public IActionResult PutFormStateOption(FormStateOption formStateOption)
        {
            var query = _dbContext.FormStateOption.Find(formStateOption.Id);

            //query.Field = formState.Field;

            //query.FieldName = formState.FieldName;

            //query.FieldType = formState.FieldType;

            //query.InUse = formState.InUse;
            //query.LastEditUserId = GetUserId();
            //query.LastEditUserName = GetUserName();
            //query.LastEditDate = DateTime.Now;
            _dbContext.Update(query);
            if (_dbContext.SaveChanges() > 0)
                return Ok(new { code = StatusCodes.Status200OK, message = "更新成功" });
            else
                return Ok(new { code = StatusCodes.Status400BadRequest, message = "更新失败" });
        }
        #endregion
    }
}
