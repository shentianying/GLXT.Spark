using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GLXT.Spark.Entity.XTGL;
using GLXT.Spark.Entity;
using Newtonsoft.Json;

namespace GLXT.Spark.ViewModel.RSGL.Person
{
    public class PersonOrganizationViewModel
    {
        public int Id { get; set; }
        [JsonProperty(PropertyName = "pid")]
        public int PId { get; set; } = -1;
        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }

        public List<GLXT.Spark.Entity.RSGL.PersonPost> personList { get; set; } 
    }
}
