using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace VCloud.PowerBIManager
{
    static class Program
    {
        static Program()
        {
            AppDomain.CurrentDomain.AssemblyResolve += (sender, args) =>
            {
                var thisAssembly = Assembly.GetExecutingAssembly();
                FileHelper.AppendLog("Request Assembly---------------------------- " + args.Name);
                var name = args.Name.Substring(0, args.Name.IndexOf(',')) + ".dll";
                var resources = thisAssembly.GetManifestResourceNames().Where(s => s.EndsWith(name));
                if (resources.Count() > 0)
                {
                    var resourceName = resources.First();
                    using (var stream = thisAssembly.GetManifestResourceStream(resourceName))
                    {
                        if (stream == null) return null;
                        var block = new byte[stream.Length];
                        stream.Read(block, 0, block.Length);
                        var result = Assembly.Load(block);
                        FileHelper.AppendLog("Load Assembly successfully---------------------------- " + result.FullName);
                        return result;
                    }
                }
                return null;
            };
        }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainWindow());
        }
    }
}
