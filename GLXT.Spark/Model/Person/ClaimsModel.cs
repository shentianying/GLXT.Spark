using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GLXT.Spark.Model.Person
{
    public class ClaimsModel
    {
        public int Id { get; set; } // id号
        public string Name { get; set; }// 姓名
        public string Number { get; set; } // 工号
        public string Role { get; set; } // 角色，多个逗号隔开
        /// <summary>
        /// 登录日志ID
        /// </summary>
        public int LogId { get; set; } 
    }
}
