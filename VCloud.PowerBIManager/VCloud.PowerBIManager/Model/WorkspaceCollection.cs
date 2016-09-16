using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VCloud.PowerBIManager
{
  public  class WorkspaceCollection
    {
        public WorkspaceCollection()
        {
            this.Workspaces = new List<Workspace>();
        }

        public String Name { get; set; }

        public String AccessKey { get; set; }

        public List<Workspace> Workspaces { get; set; }
    }
}
