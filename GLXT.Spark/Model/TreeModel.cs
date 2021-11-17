using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GLXT.Spark.Model
{
    public class TreeModel
    {
        public TreeModel()
        {
            Children = new List<TreeModel>();
        }
        public int Id { get; set; }
        public int Pid { get; set; }
        public string Label { get; set; }
        public List<TreeModel> Children { get; set; }
    }
}
