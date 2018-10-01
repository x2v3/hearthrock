using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Hearthrock.Diagnostics
{
    internal class LocalLogFile
    {
        public LocalLogFile(string filename)
        {
            this.filename = filename;
        }

        public static LocalLogFile Create(string filename)
        {
            if (File.Exists(filename))
            {
                File.Delete(filename);
            }
            var dir = Path.GetDirectoryName(filename);
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            return new LocalLogFile(filename);
        }

        private readonly string filename;

        public void WriteLog(string content)
        {
            try
            {
                if (!File.Exists(filename))
                {
                    var f = File.Create(filename);
                    f.Close();
                    f.Dispose();
                }
                //var s = File.Open(filename, FileMode.Append, FileAccess.ReadWrite, FileShare.Read);
                //var fs = new StreamWriter(s);
                //fs.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss").PadLeft(30, '*').PadRight(60, '*'));
                //fs.WriteLine(content);
                //fs.WriteLine("".PadRight(50, '#'));
                //fs.Close();
                //fs.Dispose();
                var sb = new StringBuilder();
                sb.AppendLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss").PadLeft(30, '*').PadRight(60, '*'));
                sb.AppendLine(content);
                sb.AppendLine("".PadRight(50, '#'));
                File.AppendAllText(filename, sb.ToString());
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public void WriteLog(Exception ex)
        {
            var content = ex.ToString();
            WriteLog(content);
        }
    }
}