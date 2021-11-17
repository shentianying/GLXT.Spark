using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GLXT.Spark.Entity.RSGL;
using GLXT.Spark.Entity.XTGL;

namespace GLXT.Spark.ViewModel.XTGL.Users
{
    public class UsersViewModel
    {
        public int Id { get; set; }
        public int PersonId { get; set; }
        public string UserName { get; set; }
        public bool InUse { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime LastEditDate { get; set; }
        public string LastEditUserName { get; set; }
        public string CreateUserName { get; set; }
        public DateTime? ExpirationDate { get; set; }
        public string[] RoleNames { get; set; }

        // public Person Person { get; set; }
    }
}
