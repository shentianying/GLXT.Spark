using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GLXT.Spark.Filters
{
    // 拒绝请求特性attribute ------------ true:拒绝，false: 允许
    public class RequirePermissionAttribute : Attribute
    {
        private readonly bool _require;
        public RequirePermissionAttribute() { }
        public RequirePermissionAttribute(bool require)
        {
            _require = require;
        }
        // 是否允许请求接口
        public bool RejectRequest
        {
            get { return _require; }
        }

    }
}
