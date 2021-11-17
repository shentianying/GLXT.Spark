using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GLXT.Spark.Entity;
using GLXT.Spark.Entity.XTGL;
using GLXT.Spark.Filters;
using GLXT.Spark.IService;

namespace GLXT.Spark.Controllers.XTGL
{
    /// <summary>
    /// 信息数据
    /// </summary>
    [Route("api/XTGL/[controller]")]
    [ApiController]
    [Authorize]
    public class MessageController : BaseController
    {
        private readonly DBContext _dbContext;
        private readonly ICommonService _commonService;
        private readonly ISystemService _systemService;
        private readonly IBillFlowService _billFlowService;
        public MessageController(DBContext dbContext, ICommonService commonService,ISystemService systemService, IBillFlowService billFlowService)
        {
            _commonService = commonService;
            _dbContext = dbContext;
            _systemService = systemService;
            _billFlowService = billFlowService;
        }
        #region Remind 通知提醒
        /// <summary>
        /// 信息列表分页
        /// </summary>
        /// <returns></returns>
        [HttpGet, Route("GetRemindPaging")]
        [RequirePermission]
        public IActionResult GetRemindPaging(int currentPage, int pageSize, bool? isRead, int? type,string title="" )
        {
            var query = _dbContext.Remind
                .OrderByDescending(o =>o.IsTop).ThenByDescending(o=>o.SendTime)
                .Where(w => w.ReceiverId.Equals(GetUserId()));
            if(!string.IsNullOrWhiteSpace(title))
                query = query.Where(w => w.Title.Contains(title));

            if (isRead.HasValue)
                query = query.Where(w => w.IsRead.Equals(isRead));

            if (type.HasValue)
                query = query.Where(w => w.Type .Equals(type));
            
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

            var noReadCount = _dbContext.Remind
                .Where(w => w.ReceiverId.Equals(GetUserId()) && !w.IsRead).Count();

            return Ok(new { code = StatusCodes.Status200OK, data = result.ToList(), count = count, noReadCount= noReadCount });
        }

        /// <summary>
        /// 发送系统消息
        /// </summary>
        /// <param name="MessageData"></param>
        /// <returns></returns>
        [HttpPost, Route("SendRemind")]
        //[RequirePermission]
        public IActionResult SendRemind(List<Remind> MessageData)
        {
            foreach (var msg in MessageData)
            {
                if (msg.Type.Equals(0))
                {
                    msg.Title = "系统消息";
                }
                if (string.IsNullOrWhiteSpace(msg.Title))
                {
                    return Ok(new { code = StatusCodes.Status400BadRequest, message = "消息名称不能为空" });
                }
                if (string.IsNullOrWhiteSpace(msg.Content))
                {
                    return Ok(new { code = StatusCodes.Status400BadRequest, message = "消息内容不能为空" });
                }
                //msg.Content = msg.Content;
                msg.IsTop = false;
                //msg.ReceiverId = GetUserId();
                if (msg.Type.Equals(0)) // 0 系统消息
                    msg.SenderId = 0;
                else
                    msg.SenderId = GetUserId();
                msg.SendName = GetUserName();
                msg.SendTime = DateTime.Now;
                msg.InUse = true;
                _dbContext.Add(msg);
            }
            if (_dbContext.SaveChanges()>0)
            {
                return Ok(new { code = StatusCodes.Status200OK, message = "发送成功" });
            }
            else
                return Ok(new { code = StatusCodes.Status400BadRequest, message = "发送失败" });
        }
        /// <summary>
        /// 根据流程Id 获取消息信息
        /// </summary>
        /// <returns></returns>
        [HttpGet, Route("GetRemindByBillFlowId")]
        [RequirePermission]
        public IActionResult GetRemindByBillFlowId(int? billFlowId)
        {
            var query = _dbContext.Remind
                .OrderByDescending(o => o.SendTime)
                .Where(w => w.BillFlowId.Equals(billFlowId));



            return Ok(new { code = StatusCodes.Status200OK, data = query.ToList()});
        }
        /// <summary>
        /// 更新信息列表
        /// </summary>
        /// <param name="msgId">信息对象</param>
        /// <returns></returns>
        [HttpPut, Route("PutIsRead")]
        //[RequirePermission]
        public IActionResult PutIsRead(int msgId)
        {
            if(msgId>0)
            {
                var messages = _dbContext.Remind.FirstOrDefault(w => w.InUse && w.ReceiverId.Equals(GetUserId()) && w.Id.Equals(msgId) && !w.IsRead);
                if(messages!=null)
                {
                    messages.IsRead = true;
                    messages.ReadTime = DateTime.Now;
                    _dbContext.Update(messages);
                }else
                    return Ok(new { code = StatusCodes.Status400BadRequest, message = "设置失败" });
            }
            else
            {
                var myMessage = _dbContext.Remind.Where(w => w.InUse && w.ReceiverId.Equals(GetUserId()) && !w.IsRead).AsNoTracking();
                if(!myMessage.Any())
                {
                    return Ok(new { code = StatusCodes.Status400BadRequest, message = "没有未读消息了" });
                }
                foreach (var message in myMessage)
                {
                    message.IsRead = true;
                    message.ReadTime = DateTime.Now;
                    _dbContext.Update(message);
                }
            }
            if (_dbContext.SaveChanges() > 0)
                return Ok(new { code = StatusCodes.Status200OK, message = "设置成功" });
            else
                return Ok(new { code = StatusCodes.Status400BadRequest, message = "设置失败" });
        }
        /// <summary>
        /// 验证消息然后自动登录
        /// </summary>
        /// <returns></returns>
        [HttpGet, Route("CheckRemind")]
        [AllowAnonymous]
        public IActionResult CheckRemind(int? id, string str = "")
        {
            if (!id.HasValue || (string.IsNullOrEmpty(str) && str.Length == 3))
                return Ok(new { code = StatusCodes.Status400BadRequest, message = "参数错误" });

            var msg = _dbContext.Remind.FirstOrDefault(f => f.Id.Equals(id) && f.Str.Equals(str));
            if (msg == null)
                return Ok(new { code = StatusCodes.Status400BadRequest, message = "找不到此信息" });


            msg.IsRead = true;
            msg.ReadTime = DateTime.Now;
            _dbContext.Update(msg);

            var query = _dbContext.Person
                .Include(i => i.UserRoles)
                .FirstOrDefault(w => w.Id.Equals(msg.ReceiverId) && w.InUse);

            if (query == null)
                return Ok(new { code = StatusCodes.Status400BadRequest, message = "没这个用户" });

            if (!query.IsUser)
            {
                return Ok(new { code = StatusCodes.Status400BadRequest, message = "账号未开通" });
            }
            if (query.ExpirationDate < DateTime.Now)
            {
                return Ok(new { code = StatusCodes.Status400BadRequest, message = "账号已过期" });
            }

            var logList = _dbContext.Log
                .Where(w => w.PersonId.Equals(query.Id) && w.OnLine);
            foreach (var item in logList)
            {
                item.LogoutDate = item.ActiveDate;
                item.OnLine = false;
            }
            _dbContext.UpdateRange(logList);

            string ipAddress = HttpContext.Connection.RemoteIpAddress.ToString();
            Log log = new Log() { PersonId = query.Id, IPAddress = ipAddress };
            _dbContext.Add(log);
            _dbContext.SaveChanges();
            string token = _systemService.GetToken(query, log.Id);

            return Ok(
                new
                {
                    code = StatusCodes.Status200OK,
                    data = new { token = token, expire = 60 },
                    pageId = msg.PageId,
                    billId = msg.BillId,
                    billFlowId= msg.BillFlowId
                });

        }

        #endregion

        #region message 和 messageUser 信息和用户信息
        /// <summary>
        /// 发送系统消息
        /// </summary>
        /// <param name="msg">消息对象</param>
        /// <returns></returns>
        [HttpPost, Route("SendMessage")]
        //[RequirePermission]
        public IActionResult SendMessage(Message msg)
        {
            if (msg.Type.Equals(0))
            {
                msg.Title = "系统消息";
            }
            if (string.IsNullOrWhiteSpace(msg.Title))
            {
                return Ok(new { code = StatusCodes.Status400BadRequest, message = "消息名称不能为空" });
            }
            if (string.IsNullOrWhiteSpace(msg.Content))
            {
                return Ok(new { code = StatusCodes.Status400BadRequest, message = "消息内容不能为空" });
            }
            if (msg.Type.Equals(0)) // 0 系统消息
                msg.SenderId = 0;
            else
                msg.SenderId = GetUserId();
            msg.SendName = GetUserName();
            msg.SendTime = DateTime.Now;
            msg.InUse = true;

            foreach (var mu in msg.MessageUser)
            {
                mu.IsRead = false;
                mu.ReceiverId = mu.ReceiverId;
            }
            _dbContext.Add(msg);


            if (_dbContext.SaveChanges() > 0)
                return Ok(new { code = StatusCodes.Status200OK, message = "发送成功" });
            else
                return Ok(new { code = StatusCodes.Status400BadRequest, message = "发送失败" });
        }
        /// <summary>
        /// 根据流程Id 获取 消息
        /// </summary>
        /// <returns></returns>
        [HttpGet, Route("GetMessageByBillFlowId")]
        public IActionResult GetMessageByBillFlowId(int billFlowId)
        {
            var query = _dbContext.Message
                .OrderByDescending(o => o.SendTime)
                .Where(w => w.InUse&&w.BillFlowId.Equals(billFlowId)&&w.Type==3).AsNoTracking().ToList();
            var query1 = _dbContext.Message
                .OrderByDescending(o => o.SendTime)
                .Where(w => w.InUse && w.BillFlowId.Equals(billFlowId)&& w.Type==4&&w.MessageUser.Any(a=>a.ReceiverId.Equals(GetUserId())))
                .AsNoTracking().ToList();

            return Ok(new { code = StatusCodes.Status200OK, pubData = query,priData=query1 });
        }
        /// <summary>
        /// 获取我的动态信息
        /// </summary>
        /// <returns></returns>
        [HttpGet, Route("GetMyMessageUser")]
        public IActionResult GetMyMessageUser(int billFlowId=0,int billId=0)
        {

            var query = _dbContext.MessageUser.Include(i => i.Message)
                .Where(w => w.Message.InUse && w.ReceiverId.Equals(GetUserId()))
                .AsNoTracking().ToList();
            return Ok(new { code = StatusCodes.Status200OK, data = query });
        }
        #endregion

        #region 异常信息
        /// <summary>
        /// 异常信息
        /// </summary>
        /// <returns></returns>
        [HttpGet, Route("GetExceptionsPaging")]
        [RequirePermission]
        public IActionResult GetExceptionsPaging(int currentPage, int pageSize)
        {
            IQueryable<SystemExceptions> query = _dbContext.SystemExceptions
                .OrderByDescending(o=>o.CreateTime);

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

            return Ok(new { code = StatusCodes.Status200OK, data = result.ToList(), count = count });
        }
        #endregion
    }
}