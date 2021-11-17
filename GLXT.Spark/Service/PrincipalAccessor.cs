using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using GLXT.Spark.IService;
using GLXT.Spark.Model.Person;

namespace GLXT.Spark.Service
{
    public class PrincipalAccessor : IPrincipalAccessor
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        public PrincipalAccessor(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public ClaimsModel Claim()
        {
            var User = _httpContextAccessor.HttpContext.User;
            if (User != null)
            {
                string Name = User.Claims.FirstOrDefault(c=>c.Type== ClaimTypes.Name)?.Value;
                string Role = User.FindFirst(ClaimTypes.Role)?.Value;

                string sid = User.FindFirst(ClaimTypes.Sid)?.Value;
                int Id = string.IsNullOrEmpty(sid)?0:int.Parse(sid);

                string Number = User.FindFirst("Number")?.Value;

                string lId = User.FindFirst("LogId")?.Value;
                int LogId = string.IsNullOrEmpty(lId) ? 0 : int.Parse(lId);
                //int LogId = int.Parse(User != null ? User.FindFirst("LogId").Value : "0");

                return new ClaimsModel
                {
                    Id = Id,
                    Name = Name,
                    Role = Role,
                    Number = Number,
                    LogId = LogId
                };
            }
            else
                return null;
            
        }
    }
}
