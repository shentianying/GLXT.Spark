using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace GLXT.Spark.Model
{
    public class PageModel
    {
        public int Id { get; set; }
        /// <summary>
        /// 父Id
        /// </summary>
        public int Pid { get; set; }
        /// <summary>
        /// 排序
        /// </summary>
        public int Sort { get; set; }

        /// <summary>
        /// 路由名字
        /// </summary>
        [StringLength(40)]
        public string RouterName { get; set; }
        /// <summary>
        /// 路由重定向地址
        /// </summary>
        [StringLength(40)]
        public string RouterRedirect { get; set; }
        /// <summary>
        /// 路由路径地址
        /// </summary>
        [StringLength(40)]
        public string RouterPath { get; set; }
        /// <summary>
        /// 路由组件
        /// </summary>
        /// 
        [StringLength(40)]
        public string RouterComponent { get; set; }
        /// <summary>
        /// 路由图标
        /// </summary>
        /// 
        [StringLength(40)]
        public string RouterIcon { get; set; }
        /// <summary>
        /// 路由页面标题
        /// </summary>
        /// 
        [StringLength(40)]
        public string RouterTitle { get; set; }
        /// <summary>
        /// 是否隐藏
        /// </summary>
        public bool RouterHidden { get; set; } = false;
        ///// <summary>
        ///// 是否是菜单  1： 菜单 | 0： 不是菜单
        ///// </summary>
        //public bool IsMenu { get; set; } = true;
        ///// <summary>
        ///// Controller名称
        ///// </summary>
        //public string ControllerName { get; set; }
        ///// <summary>
        ///// Action名称
        ///// </summary>
        //public string ActionName { get; set; }
    }
}
