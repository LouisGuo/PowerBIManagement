using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace VCloud.PowerBIManager
{
    public static class Logger
    {
        private static readonly HashSet<TextBoxBase> consolesToPrint = new HashSet<TextBoxBase>();

        public static void Info(String message)
        {
            //message = DateTime.Now.ToString() + ": Info \r\n" + message;
            //Flash(message);
        }

        public static void Print(String message)
        {
            //message = new String('-', 10) + message;
            //Flash(message);
        }

        private static void Flash(String message)
        {
            message = message + "\r\n\r\n";
            FileHelper.AppendLog(message);
            foreach (var console in consolesToPrint)
            {
                if (!console.IsDisposed)
                    console.UIThread(() =>
                    {
                        console.AppendText(message);
                        console.SelectionStart = console.TextLength;
                        console.ScrollToCaret();
                    });
            }
        }

        public static void Error(String message)
        {
            message = DateTime.Now.ToString() + ": Error \r\n" + message;
            Flash(message);
        }

        public static void AddConsole(TextBoxBase textBox)
        {
            consolesToPrint.Add(textBox);
        }

        public static void RemoveConsole(TextBoxBase textBox)
        {
            consolesToPrint.Remove(textBox);
        }
    }
}
