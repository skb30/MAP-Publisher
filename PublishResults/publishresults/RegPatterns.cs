using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PublishResults
{
    public static class RegPatterns
    {
        public static string getPassedPattern()     {return @"\spassed\s"; }
        public static string getScriptPattern()     {return @"([A-Za-z0-9-_]+)\.pl.*";}
        public static string getHeaderPattern()     {return @"(.*)\#(.*)\#(.*)";}
        public static string getSuiteNamePattern()  {return @"\s+([\w-]+)\s+";}
        public static string getStatusPattern()     {return @"\spassed\s";}
        public static string getStartTimePattern()  {return @"(\d{4}-\d{2}-\d{2}\s\d{1,2}:\d{1,2}:\d{1,2})"; }
        public static string getYYMMDDMMSSpattern() {return @"(\d{4})-(\d{2})-(\d{2})\s(\d{2}):(\d{2}):(\d{2})"; }                           
        public static string getEndTimePattern()    {return @"\d{4}-\d{2}-\d{2}\s\d{1,2}:\d{1,2}:\d{1,2}\s+(\d{4}-\d{2}-\d{2}\s\d{1,2}:\d{1,2}:\d{1,2})"; }
        public static string getSuitelogPattern() { return @"suitelog.txt"; }

    }
}
