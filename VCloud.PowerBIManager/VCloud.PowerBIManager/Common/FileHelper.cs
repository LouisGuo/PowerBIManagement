using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VCloud.PowerBIManager
{
    public static class FileHelper
    {
        private static readonly String historyPath = "History.dat";

        private static readonly String logPath = "log.txt";

        private static readonly Object hisLocker = new Object();

        private static readonly Object logLocker = new Object();

        private static readonly String ReportTemplate;

        static FileHelper()
        {
            try
            {
                using (var reportStream = typeof(FileHelper).Assembly.GetManifestResourceStream("VCloud.PowerBIManager.View.ReportViewer.html"))
                using (var reader = new StreamReader(reportStream))
                {
                    ReportTemplate = reader.ReadToEnd();
                }
            }
            catch (Exception ex)
            {
                ReportTemplate = ex.ToString();
                AppendLog(String.Format("Error: read report template", ex));
            }
        }

        public static String GetReportContent(String reportId, String accessToken)
        {
            return ReportTemplate.Replace("{2027efc6-a308-4632-a775-b9a9186f087c}", reportId).
                         Replace("{AB4DD0C7-C87D-46D4-A787-921C470D27B9}", accessToken);
        }

        public static List<WorkspaceCollection> GetHistoryWorkspaceCollections()
        {
            lock (hisLocker)
            {
                var result = new List<WorkspaceCollection>();
                if (File.Exists(historyPath))
                {
                    using (var reader = new StreamReader(historyPath, Encoding.UTF8))
                    {
                        var str = reader.ReadToEnd();
                        result = JsonSerializer.ConvertStringToObj<List<WorkspaceCollection>>(str);
                    }
                }
                return result;
            }
        }

        public static void UpdateHistoryWorkspaceCollections(List<WorkspaceCollection> collections)
        {
            lock (hisLocker)
            {
                using (var writer = new StreamWriter(historyPath, false, Encoding.UTF8))
                {
                    var str = JsonSerializer.ConvertObjToString(collections);
                    writer.Write(str);
                }
            }
        }

        public static void AppendLog(String message)
        {
            lock (logLocker)
            {
                using (var writer = new StreamWriter(logPath, true, Encoding.UTF8))
                {
                    writer.WriteLine(message);
                }
            }
        }
    }
}
