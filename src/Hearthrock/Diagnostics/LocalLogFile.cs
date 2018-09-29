using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Hearthrock.Diagnostics
{
    class LocalLogFile
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
            var fs = File.CreateText(filename);
            fs.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss").PadLeft(30,'*').PadRight(60,'*'));
            fs.WriteLine(content);
            fs.WriteLine("".PadRight(50,'#'));
            fs.Close();
            fs.Dispose();
        }

        
        public void WriteLog(Exception ex)
        {
            var content = ex.ToString();
            WriteLog(content);
        }
    }
}
