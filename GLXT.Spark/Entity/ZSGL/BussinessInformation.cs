﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace GLXT.Spark.Entity.ZSGL
{
    /// <summary>
    /// 招商资讯
    /// </summary>
    [Table("zsglBussinessInformation")]
    public class BussinessInformation : BaseCreateUser
    {
        ///<summary>
        ///int:主键ID
        ///</summary>
        public int Id { get; set; }

        /// <summary>
        /// 公司Id
        /// </summary>
        public int CompanyId { get; set; }

        /// <summary>
        /// int：资讯类型
        /// </summary>
        public int InformationType { get; set; }

        /// <summary>
        /// string：资讯标题
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// string：资讯内容
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// string：链接
        /// </summary>
        public string Url { get; set; }
    }
}
