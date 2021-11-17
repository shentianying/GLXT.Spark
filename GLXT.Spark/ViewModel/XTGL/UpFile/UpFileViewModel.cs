using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GLXT.Spark.ViewModel.XTGL.UpFile
{
    public class UpFileViewModel<T> where T:class
    {
        public List<FileList> FileList { get; set; }
        public T t { get; set; }
    }
}
