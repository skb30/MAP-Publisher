using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace PublishResults
{
    class Logger
    {
        static string  path2log;
        static TextBox tb;
        public Logger(string path2log /*TextBox tb */) 
        {
            DirectoryInfo directoryInfo = System.IO.Directory.GetParent(path2log);
            Logger.path2log = directoryInfo.ToString();
            //Logger.tb = tb;
        }
        public static void write2log(int rc, string msg)
        {
            string status = " *** ";

            if (rc < 0)
            {
                status = " +++ ";

            }
            else if (rc > 0)
            {
                status = " WWW ";
            }

            // Append to the log file

            using (StreamWriter sw = File.AppendText(path2log + @"\publisher.txt"))
            {
                try
                {
                    sw.WriteLine(DateTime.Now.ToString() + ": " + status + msg + status);
                    //tb.AppendText(DateTime.Now.ToString() + ": " + status + msg + status);

                }
                catch (Exception e)
                {
                    write2log(-1, e.ToString());
                }
            }
        }

    }
}
