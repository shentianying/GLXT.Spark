using Hei.Captcha;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using GLXT.Spark.Entity;
using GLXT.Spark.Hubs;
using GLXT.Spark.Model;
using GLXT.Spark.Utils;

namespace GLXT.Spark.Controllers
{
    /// <summary>
    /// 信息提示
    /// </summary>
    [Route("/api/[controller]")]
    [ApiController]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class HomeController : ControllerBase
    {
        private readonly IMemoryCache _cache;
        private readonly AppSettingModel _appSettingModel;
        private readonly DBContext _dbContext;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IHubContext<MsgHub> _hubContext;
        private readonly SecurityCodeHelper _securityCode;


        public HomeController(DBContext dbContext, 
            IMemoryCache cache,
            IWebHostEnvironment webHostEnvironment,
            IOptions<AppSettingModel> appSettingModel,
            SecurityCodeHelper securityCode,
            IHubContext<MsgHub> hubContext)
        {
            _dbContext = dbContext;
            _webHostEnvironment = webHostEnvironment;
            _cache = cache;
            _appSettingModel = appSettingModel.Value;
            _hubContext = hubContext;
            _securityCode = securityCode;
        }
        /// <summary>
        /// 系统异常返回函数
        /// </summary>
        /// <returns></returns>
        [HttpGet,Route("/error400")]
        public IActionResult Error400()
        {
            return Ok(new { code = StatusCodes.Status400BadRequest, message = "400:系统错误" });
        }
        /// <summary>
        /// 404 菜单没有权限
        /// </summary>
        /// <returns></returns>
        [HttpGet, Route("/error404")]
        [AllowAnonymous]
        public IActionResult Error404()
        {
            return Ok(new { code = StatusCodes.Status404NotFound, message = "404:菜单没有权限" });
        }

        /// <summary>
        /// 403 菜单按钮没权限
        /// </summary>
        /// <returns></returns>
        [HttpGet, Route("/error403")]
        [AllowAnonymous]
        public IActionResult Error403()
        {
            return Ok(new { code = StatusCodes.Status403Forbidden,data =new { },message = "403:页面没有权限，数据不显示" });
        }
        /// <summary>
        /// 401 按钮没权限
        /// </summary>
        /// <returns></returns>
        [HttpGet, Route("/error401")]
        [AllowAnonymous]
        public IActionResult Error401()
        {
            return Ok(new { code = StatusCodes.Status401Unauthorized, data = new { }, message = "401:此按钮没有权限" });
        }



        #region 验证码
        /// <summary>
        /// 泡泡中文验证码 
        /// </summary>
        /// <returns></returns>
        [HttpGet, Route("GetBubbleCode")]
        [AllowAnonymous]
        public IActionResult BubbleCode()
        {
            var code = _securityCode.GetRandomCnText(2);
            var imgbyte = _securityCode.GetBubbleCodeByte(code);

            return File(imgbyte, "image/png");
        }

        /// <summary>
        /// 数字字母组合验证码
        /// </summary>
        /// <returns></returns>
        [HttpGet, Route("GetHybridCode")]
        [AllowAnonymous]
        public IActionResult HybridCode()
        {
            var code = _securityCode.GetRandomEnDigitalText(4);
            var imgbyte = _securityCode.GetEnDigitalCodeByte(code);

            return File(imgbyte, "image/png");
        }

        /// <summary>
        /// gif泡泡中文验证码 
        /// </summary>
        /// <returns></returns>
        [HttpGet, Route("GetGifBubbleCode")]
        [AllowAnonymous]
        public IActionResult GifBubbleCode()
        {
            var code = _securityCode.GetRandomCnText(2);
            var imgbyte = _securityCode.GetGifBubbleCodeByte(code);

            return File(imgbyte, "image/gif");
        }

        /// <summary>
        /// gif数字字母组合验证码
        /// </summary>
        /// <returns></returns>
        [HttpGet, Route("GetGifHybridCode")]
        [AllowAnonymous]
        public IActionResult GifHybridCode([FromQuery] string jobNumber)
        {
            var code = _securityCode.GetRandomEnDigitalText(4);

            if (!string.IsNullOrEmpty(jobNumber))
            {
                _cache.Set(jobNumber + "code", code.ToLower(), DateTimeOffset.Now.AddMinutes(2));
            }
            else
                return BadRequest("工号不能为空");

            var imgbyte = _securityCode.GetGifEnDigitalCodeByte(code);

            return File(imgbyte, "image/gif");
        }
        [HttpGet, Route("CheckImageCode")]
        [AllowAnonymous]
        public IActionResult CheckImageCode(string jobNumber, string code)
        {
            var cacheCode = _cache.Get(jobNumber + "code") == null ? "" : _cache.Get(jobNumber + "code").ToString();
            if (string.IsNullOrEmpty(cacheCode))
                return Ok(new
                {
                    code = StatusCodes.Status200OK,
                    success = false,
                    message = "图形验证码已经失效"
                });
            if (cacheCode.Equals(code))
            {
                return Ok(new
                {
                    code = StatusCodes.Status200OK,
                    success = true,
                    message = "验证成功",
                });

            }
            else
            {
                return Ok(new
                {
                    code = StatusCodes.Status200OK,
                    success = false,
                    message = "图形验证码错误"
                });
            }
        }
        #endregion

    }
}