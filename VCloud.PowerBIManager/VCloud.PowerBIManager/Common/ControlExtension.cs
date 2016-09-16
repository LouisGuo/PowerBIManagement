using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace VCloud.PowerBIManager
{
    public static class ControlExtension
    {
        public static void UIThread(this Control control, Action code)
        {
            if (control.InvokeRequired)
            {
                control.Invoke(code);
            }
            else
            {
                code.Invoke();
            }
        }
    }
}
