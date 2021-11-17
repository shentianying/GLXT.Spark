using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using GLXT.Spark.Entity;
using GLXT.Spark.Entity.XTGL;
using GLXT.Spark.IService;
using GLXT.Spark.Model;
using GLXT.Spark.Utils;
using GLXT.Spark.ViewModel.XTGL.UpFile;

namespace GLXT.Spark.Controllers.XTGL
{
    /// <summary>
    /// 上传文件controller
    /// </summary>
    [Route("/api/XTGL/[controller]")]
    [ApiController]
    [Authorize]
    public class UpFileController : BaseController
    {
        private readonly DBContext _dbContext;
        private readonly ICommonService _commonService;
        private readonly AppSettingModel _appSettingModel;
        private readonly ISystemService _systemService;
        public UpFileController(DBContext dbContext, ICommonService commonService, IOptions<AppSettingModel> appSettingModel, ISystemService systemService)
        {
            _dbContext = dbContext;
            _commonService = commonService;
            _appSettingModel = appSettingModel.Value;
            _systemService = systemService;
        }

        /// <summary>
        /// 上传图片
        /// </summary>
        /// <returns></returns>
        [HttpPost, Route("UpLoadImage")]
        public IActionResult UpLoadImage()
        {
            //var formFile = Request.Form.Files[0];
            //var dic = Request.Form["dic"];
            //var tableName = Request.Form["tableName"];
            //var columnName = Request.Form["columnName"];

            //if (string.IsNullOrEmpty(dic) || string.IsNullOrEmpty(tableName) || string.IsNullOrEmpty(columnName))
            //{
            //    return BadRequest();
            //}

            //var Extension = Path.GetExtension(formFile.FileName).ToLower();
            //ArrayList ExtensionList = new ArrayList() { ".jpg", ".jpeg", ".gif", ".png" };
            //if (!ExtensionList.Contains(Extension))
            //{
            //    return BadRequest();
            //}


            //var result = Common.UpLoadSingleFile(formFile, dic, _webHostEnvironment.WebRootPath);
            return Ok(new { data = "" });
        }
        /// <summary>
        /// 上传文件
        /// </summary>
        /// <returns></returns>
        [HttpPost, Route("UpLoadFile")]
        public async Task<IActionResult> UpLoadFile()
        {
            var formFile = Request.Form.Files[0];
            if (formFile.Length > 0)
            {
                var fileName = Path.GetFileName(formFile.FileName).ToLower();
                var extension = Path.GetExtension(fileName).ToLower();
                ArrayList extensionList = new ArrayList() { ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx", ".zip", ".rar", ".7z", ".png", ".gif", ".jpg", ".jpeg", ".bmp", ".pdf", ".dwg", ".mp3", ".wav", ".mp4", ".txt" };
                if (!extensionList.Contains(extension))
                {
                    return BadRequest();
                }
                // 新名字
                string newFileName = Guid.NewGuid() + extension;
                int componyId = _systemService.GetCurrentSelectedCompanyId();
                // 目录路径
                string dirPath = Path.Combine(_appSettingModel.DirPath, componyId + "", DateTime.Now.Year + "", DateTime.Now.Month + "",DateTime.Now.Day+"");
                if (!Directory.Exists(dirPath))
                    Directory.CreateDirectory(dirPath);

                //文件路径
                string filePath = Path.Combine(dirPath, newFileName);

                // 上传
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await formFile.CopyToAsync(stream);
                }
                // 保存到临时表中
                var upFileTemp = new UpFileTemp
                {
                    FileName = fileName,
                    FileType = extension,
                    FileValue = newFileName,
                    FilePath = dirPath,
                    FileSize = formFile.Length

                };
                _dbContext.Add(upFileTemp);
                if (_dbContext.SaveChanges() > 0)
                    return Ok(new { code = StatusCodes.Status200OK, data = upFileTemp.Id });
                else
                    return Ok(new { code = StatusCodes.Status400BadRequest, message = "上传失败" });
            }
            else
            {
                return Ok(new { code = StatusCodes.Status400BadRequest, message = "上传失败，文件不能为空" });
            }

        }
        /// <summary>
        /// 获取文件流
        /// </summary>
        /// <param name="addFlag">是否是新加的文件</param>
        /// <param name="id">id</param>
        /// <returns></returns>
        [HttpGet, Route("GetFile")]
        public async Task<IActionResult> GetFile(bool addFlag, int? id)
        {
            if (id.HasValue)
            {
                string path = "";
                if (addFlag)
                {
                    var file = _dbContext.UpFileTemp.Find(id.Value);
                    path = Path.Combine(file.FilePath, file.FileValue);
                }
                else
                {
                    var file = _dbContext.UpFile.Find(id.Value);
                    path = Path.Combine(file.FilePath, file.FileValue);
                }

                if (!System.IO.File.Exists(path))
                    return Ok(new { code = StatusCodes.Status404NotFound, message = "文件未找到！" });

                var result = await Task.Run(() =>
                {
                    return PhysicalFile(@$"{path}", "application/octet-stream");
                });
                return result;
            }
            else
                return BadRequest();
        }

        /// <summary>
        /// 删除临时上传文件表(定时任务使用)
        /// </summary>
        /// <returns></returns>
        [HttpGet, Route("DeleteUpFileTemp")]
        [AllowAnonymous]
        public IActionResult DeleteUpFileTemp()
        {
            //删除临时文件 并且 和记录
            var query = _dbContext.UpFileTemp.Where(w => w.CreateDate < DateTime.Now.AddDays(-1)).AsNoTracking().ToList();
            query.ForEach(e =>
            {
                string path = Path.Combine(e.FilePath, e.FileValue);
                if (System.IO.File.Exists(path))
                    System.IO.File.Delete(path);
            });
            _dbContext.RemoveRange(query);
            if (_dbContext.SaveChanges() > 0)
                return Ok(new { code = StatusCodes.Status200OK, message = "删除成功" });
            else
                return Ok(new { code = StatusCodes.Status400BadRequest, message = "删除失败" });
        }
        /// <summary>
        /// 修改文件名
        /// </summary>
        /// <param name="dyc">接收的对象</param>
        /// <returns></returns>
        [HttpPut, Route("PutFileName")]
        //[RequirePermission]
        public IActionResult PutFileName(dynamic dyc)
        {
            if (dyc.id == null || dyc.newName == null || dyc.addFlag == null)
            {
                return BadRequest();
            }
            int id = dyc.id;
            string newName = dyc.newName;
            bool addFlag = dyc.addFlag;

            if (addFlag)
            {
                var q1 = _dbContext.UpFileTemp.Find(id);
                    q1.FileName = newName;
                _dbContext.Update(q1);
            }
            else
            {
                var q1 = _dbContext.UpFile.Find(id);
                    q1.FileName = newName;
                _dbContext.Update(q1);
            }
            if (_dbContext.SaveChanges() > 0)
                return Ok(new { code = StatusCodes.Status200OK, message = "保存成功" });
            else
                return Ok(new { code = StatusCodes.Status400BadRequest, message = "保存失败" });
        }

    }
}
