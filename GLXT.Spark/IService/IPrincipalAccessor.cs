using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using GLXT.Spark.Model.Person;

namespace GLXT.Spark.IService
{
    /// <summary>
    /// 获取 当前 用户 claims 信息
    /// </summary>
    public interface IPrincipalAccessor
    {
        ClaimsModel Claim();
    }
}
