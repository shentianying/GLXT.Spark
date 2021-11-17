using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GLXT.Spark.ViewModel.XTGL.Material
{
    public class MaterialCategoryViewModel
    {
        public int Id { get; set; }
        public string[] CategoryNameArray { get; set; }
        public string Unit { get; set; }
        public bool IsMain { get; set; }
        public int PId { get; set; }
        public int OrganizationId { get; set; }
    }
}
