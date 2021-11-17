using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Linq;
using System.Security.Claims;
using GLXT.Spark.IService;

namespace GLXT.Spark.Filters
{
    public class PermissionFilter : IAuthorizationFilter
    {
        private readonly ISystemService _systemService;
        
        public PermissionFilter(ISystemService systemService)
        {
            _systemService = systemService;
        }
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            // 如果这个接口需要验证接口权限
            if (context.ActionDescriptor.EndpointMetadata.Any(item => item is RequirePermissionAttribute))
            {
                var controllerName = context.RouteData.Values["controller"].ToString()?.ToLower().Trim();
                var actionName = context.RouteData.Values["action"].ToString()?.ToLower().Trim();

                var userId = context.HttpContext.User.FindFirst(ClaimTypes.Sid).Value;
                //获取用户的角色
                var roleIds = context.HttpContext.User.FindFirst(ClaimTypes.Role).Value;
                // string 数组 转化 int 数组
                var convertIntArr = Array.ConvertAll<string, int>(roleIds.Split(',', StringSplitOptions.None), s => int.Parse(s));

                // 如果是超级管理员，直接过滤不处理权限
                //if (convertIntArr.Any(s => s == 1))
                //{
                //    return;
                //}
                // 根据角色获取 角色权限List
                var rolePermitList = _systemService.GetRolePermitByRoleId(convertIntArr);
                rolePermitList.ForEach((a) =>
                {
                    var x = a.Permit.Controller.ToString();
                    var y = a.Permit.Action.ToString();
                });
                // 根据 control and action 判断有没有权限
                if (rolePermitList.Any(w =>
                w.Permit.Controller.ToLower().Trim().Equals(controllerName)
                && w.Permit.Action.ToLower().Trim().Equals(actionName)))
                {
                    // true 从你所拥有的权限中查到了权限，就继续执行
                    return;
                }
                else
                {
                    // false 找不到权限，直接返回报错
                    context.Result = new RedirectResult("/error404");
                    return;
                }
            }
            else // 不需要验证的接口直接return通过
                return;
        }
    }
}
