using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1
{
   public  class DBC
    {
        public DBC(string hdr) { header = hdr; }
        public string header = null;
        public string connect = null;
        public string path = null;
        public string host = null;
        public string dbname = null;
        public bool isvalid=true;
    }
}
