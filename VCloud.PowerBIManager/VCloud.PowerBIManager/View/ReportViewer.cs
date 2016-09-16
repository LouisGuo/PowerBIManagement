using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace VCloud.PowerBIManager
{
    public partial class ReportViewer : Form
    {
        public ReportViewer(WorkspaceCollection collection, String workspaceId, String reportId, String reportName)
        {
            InitializeComponent();
            try
            {
                var powerBI = new PowerBIManager(collection);
                var accessToken = powerBI.GetAccessToken(workspaceId, reportId);
                this.Text = String.Format("ReportViewer-{0}", reportName);
                this.webBrowser1.DocumentText = FileHelper.GetReportContent(reportId, accessToken);
            }
            catch (Exception ex)
            {
                FileHelper.AppendLog(String.Format("Error: open report viewer error, details: {0}", ex));
            }
        }
    }
}
