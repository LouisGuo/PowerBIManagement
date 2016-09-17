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
        private static Dictionary<String, Assembly> EmbeddedAssembly = new Dictionary<String, Assembly>();

        static Program()
        {
            try
            {
                var currentAssembly = typeof(Program).Assembly;
                var allAssembly = currentAssembly.GetManifestResourceNames().
                    Where(s => s.EndsWith(".dll", StringComparison.OrdinalIgnoreCase));
                foreach (var assemblyName in allAssembly)
                {
                    using (var stream = currentAssembly.GetManifestResourceStream(assemblyName))
                    {
                        var block = new Byte[stream.Length];
                        stream.Read(block, 0, block.Length);
                        EmbeddedAssembly[assemblyName] = Assembly.Load(block);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex.ToString());
            }
            AppDomain.CurrentDomain.AssemblyResolve += (sender, args) =>
            {
                var thisAssembly = Assembly.GetExecutingAssembly();
                var name = args.Name.Substring(0, args.Name.IndexOf(',')) + ".dll";
                return EmbeddedAssembly.FirstOrDefault(k => k.Key.EndsWith(name, StringComparison.OrdinalIgnoreCase)).Value;
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
