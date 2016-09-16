using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VCloud.PowerBIManager
{
    public class Workspace
    {
        public Workspace()
        {
            this.Reports = new List<Report>();
            this.DataSets = new List<DataSet>();
        }

        public String Id { get; set; }

        public List<Report> Reports { get; set; }

        public List<DataSet> DataSets { get; set; }
    }
}
