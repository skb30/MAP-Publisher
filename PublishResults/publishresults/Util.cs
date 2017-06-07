using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Data;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Text.RegularExpressions;
using System.Windows.Threading;
using System.Windows.Forms;

namespace PublishResults
{
    public static class Util
    {

        public static string getScriptStatus(string record)
        {
            string status = "Passed";
            // open script runlog and check for "+++". If you find any then set status to false
            using (StreamReader sr = new StreamReader("runlog.txt"))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    if (line.Contains("+++"))
                    {
                        status = "Failed";
                        break;
                    }
                }
                return status;
            }
        }
        public static string getScriptInfo(string record, string pattern)
        {
            string info;
            Match column = Regex.Match(record, pattern, RegexOptions.IgnoreCase);
            if (column.Success)
            {
                info = column.Groups[1].Value.ToString();
            }
            else
            {
                write2log(-1, "Unable to locate the script " + pattern + " in this record: " + record);
                info = "Unknown";
            }
            return info;
        }
        public static string calculateElapsed(string st, string et)
        {
            string elaspedTime;
            // convert the MAP date and time string to a C# DateTime object
            DateTime start = convertMAPdate(st);
            DateTime end = convertMAPdate(et);
            // Difference in days, hours, minutes and seconds.
            TimeSpan ts = end - start;
            // Difference in days.
            int differenceInDays = ts.Days;
            int differenceInHours = ts.Hours;
            int differenceInMins = ts.Minutes;
            int differenceInSeconds = ts.Seconds;

            elaspedTime = differenceInDays + " Day(s). " +
                differenceInHours + " Hours(s). " +
                differenceInMins + " Min(s). " +
                differenceInSeconds + " Sec(s). ";

            return elaspedTime;
        }
        public static DateTime convertMAPdate(string dateTime)
        {
            int year = 1999;
            int mm = 12;
            int dd = 31;
            int hh = 09;
            int mi = 00;
            int ss = 00;

            Match t = Regex.Match(dateTime, RegPatterns.getYYMMDDMMSSpattern(), RegexOptions.IgnoreCase);
            if (t.Success)
            {
                year = Convert.ToInt32(t.Groups[1].Value);
                mm = Convert.ToInt32(t.Groups[2].Value);
                dd = Convert.ToInt32(t.Groups[3].Value);
                hh = Convert.ToInt32(t.Groups[4].Value);
                mi = Convert.ToInt32(t.Groups[5].Value);
                ss = Convert.ToInt32(t.Groups[6].Value);
            }
            else
            {
                write2log(-1, "Unknown Date: " + dateTime);
            }


            return new DateTime(year, mm, dd, hh, mi, ss);
        }
        public static int changeDir(string name)
        {
            int rc;
            try
            {
                Directory.SetCurrentDirectory(name);
                rc = 0;
            }
            catch (DirectoryNotFoundException e)
            {
                write2log(-1, "The specified directory does not exist. {0}" + e.Message);
                rc = -1;
            }
            return rc;
        }
        public static string getSourceContents(string fileName)
        {
            /*
             * Read the source file and add line numbers to each record in the file
             */
            using (StreamReader sr = new StreamReader(fileName))
            {
                string line;
                string stream = "";
                int lineNumber = 1;
                while ((line = sr.ReadLine()) != null)
                {
                    line = lineNumber + " " + line;
                    stream = stream + line + "\r";
                    lineNumber++;
                }
                return stream;
            }
        }
        public static string getFileContents(string fileName)
        {
            string file;
            try
            {
                file = File.ReadAllText(fileName);

            }
            catch (Exception e)
            {
                write2log(-1, e.Message);
                file = e.Message;
            }
            return file;

        }
        public static void loadArtifacts(long scriptID, PersistenceManager pm, string dirName, string filter)
        {

            string fileSource;
            long id;

            try
            {
                if (Directory.Exists(dirName))
                {
                    foreach (var f in Directory.EnumerateFiles(dirName, filter))
                    {
                        fileSource = Util.getFileContents(f);
                        id = pm.InsertArtifact(scriptID, dirName, f, fileSource);
                        write2log(0, f + " to table with ID: " + id);
                    }
                }
            }
            catch (UnauthorizedAccessException UAEx)
            {
                write2log(-1, UAEx.Message);
            }
            catch (PathTooLongException PathEx)
            {
                write2log(-1, PathEx.Message);
            }
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
            using (StreamWriter sw = File.AppendText("publisher.txt"))
            {
                try
                {
                    sw.WriteLine(DateTime.Now.ToString() + ": " + status + msg + status);
                    sw.Flush();
                }
                catch (Exception e)
                {
                    write2log(-1, e.ToString());
                }
            }
        }

        public static void ThreadSafe(Action action)
        {
            Dispatcher.CurrentDispatcher.Invoke(DispatcherPriority.Normal,
                new MethodInvoker(action));
        }
    }
}
