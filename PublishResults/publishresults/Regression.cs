using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Data;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Security.Policy;
using System.Security.Principal;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using MySql.Data.MySqlClient;
using System.Text.RegularExpressions;
using System.Collections;

namespace PublishResults
{
    class Regression
    {
        private string path;
        private string buildInfo;
        private string buildName;
        private string runtime;
        private string buildNum;
        private string endTime;
        private string rts;
        private string elaspedTime;
        private int passed = 0;
        private int failed = 0;
        private long regressionID;
        private PersistenceManager pm;

        private ArrayList scripts = new ArrayList();
        private readonly int error = -1;
        private readonly int warning = 1;
        private readonly int success = 0;

        // construct me a regression
        public Regression(string path2log)
        {
            this.path = path2log;
        }


        public void setID(long regID)
        {
            this.regressionID = regID;
        }
        public long createRegression(string dbName, int prodID, int siteID, int osID, int lparID, int subsysID, string buildInfo, string buildName)
        {
            setRegressionInfo();
            string start = StartTime;
            string end = EndTime;
            string rts = RTS;
            string elasped = ElapsedTime;
            int passed = Passed;
            int failed = Failed;
            this.buildName = buildName;
            PersistenceManager pm = new PersistenceManager("server=usmghcentos65;port=3306;uid=scottba;pwd=qamap", dbName);
            // Now store the regression into the DB

            long regID = pm.InsertRegression(prodID, siteID, osID, lparID, subsysID, buildInfo, buildName, start, end, elasped, rts, passed, failed);

            // create a suite
            Suite suite = new Suite(pm, regID);


            string[] records = File.ReadAllLines(path);


            // create a suite. bypass the first record because it's the header 
            for (int index = 1; index < records.Length; index++)
            {
                suite.create(records[index]);
            }

            // we need to get the actual passed and fail counts from a query rather than a suite log

            int qpassed = pm.getStatusCount(regID, "passed");
            int qfailed = pm.getStatusCount(regID, "failed");

            if (qpassed.Equals(passed))
            {
                Logger.write2log(success, "Suite-log matched query");
            }
            else
            {

                Logger.write2log(warning, "Suite-log: " + passed + " passed and failed: " + failed);
                Logger.write2log(warning, "Query passed: " + qpassed + " failed: " + qfailed);
                Logger.write2log(success, "Updating regression table with query passed/failed counts.");
                int diff = qpassed - passed;
                pm.setStatusCount(regID, "passed", qpassed);
                pm.setStatusCount(regID, "failed", qfailed);

            }
            return regID;
        }

        private void setRegressionInfo()
        {

            int i = 0;
            int counter = 0;

            // get the regression end time
            string last = File.ReadLines(path).Last();
            endTime = Util.getScriptInfo(last, RegPatterns.getEndTimePattern());

            // get the run time settings 
            this.rts = Util.getFileContents("rts.php");


            using (StreamReader sr = new StreamReader(path))
            {

                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    // Get the build and runtime info from header record
                    if (counter == 0)
                    {


                        Match header = Regex.Match(line, RegPatterns.getHeaderPattern(), RegexOptions.IgnoreCase);
                        if (header.Success)
                        {

                            // put the header groups into vars.
                            this.buildInfo = header.Groups[1].Value;
                            this.buildNum = header.Groups[2].Value;

                            // make sure we only grab the date
                            string rt = header.Groups[3].Value;
                            Match startTime = Regex.Match(rt, RegPatterns.getStartTimePattern(), RegexOptions.IgnoreCase);
                            if (startTime.Success)
                            {
                                this.runtime = startTime.Groups[0].Value;
                            }
                            else { this.runtime = "Not found"; }
                        }
                        else
                        {
                            Logger.write2log(error, "Unknown MAP header record in: " + path);
                        }

                        // update the record count and get the next record
                        counter++;
                        continue;

                    }

                    // Get all the script names in the suite-log


                    // build the script array
                    Match scriptName = Regex.Match(line, RegPatterns.getScriptPattern(), RegexOptions.IgnoreCase);
                    if (scriptName.Success)
                    {
                        this.scripts.Add(scriptName.Groups[1].Value);
                        Match passcnt = Regex.Match(line, RegPatterns.getPassedPattern());
                        if (passcnt.Success) { passed++; } else { failed++; }

                    }
                    else { this.scripts.Add("Invalid Script Name-> " + line); }

                    i++;
                    counter++;

                }
            }

            // calcuate the time difference
            this.elaspedTime = Util.calculateElapsed(runtime, endTime);
        }

        private string setRTS() { string rts = File.ReadAllText("rts.php"); return rts; }

        /* define object properties */

        public string RTS
        {
            get { return rts; }

        }
        public string BuildInfo
        {
            get { return buildInfo; }
        }
        //public string getBuildNum() { return this.buildNum; }
        public string BuildNum
        {
            get { return buildNum; }
        }
        //public string getBuildName()  {return this.buildName; }

        public string BuildName
        {
            get { return buildName; }
        }
        public string StartTime
        {
            get { return runtime; }
        }
        //public string getStartTime()  {return this.runtime;}

        public string EndTime
        {
            get { return endTime; }
        }
        // public string getEndTime()    {return this.endTime;}
        // public string getRTS()        {return this.rts;}

        public int Passed
        {
            get { return passed; }
        }
        //public int getPassed()        { return this.passed;}
        public int Failed
        {
            get { return failed; }
        }
        //public int getFailed()        {return this.failed;}

        public ArrayList Scripts
        {
            get { return scripts; }
        }
        //public ArrayList getScripts() {return scripts;}

        public string ElapsedTime
        {
            get { return elaspedTime; }
        }
        // public string getElapsedTime(){return this.elaspedTime;}

        public string Path2Log
        {
            get { return path; }
        }
        //public string getPath2Log() { return this.path; }

    }
}
