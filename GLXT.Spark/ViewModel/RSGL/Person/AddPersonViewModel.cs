using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GLXT.Spark.Entity.XTGL;

namespace GLXT.Spark.ViewModel.RSGL.Person
{
    public class AddPersonViewModel
    {
        public GLXT.Spark.Entity.RSGL.Person person { get; set; }
        public List<UserOrganization> userOrgList { get; set; }
    }
}
