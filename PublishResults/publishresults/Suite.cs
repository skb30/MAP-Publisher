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
    class Suite
    {
        private string record;
        private string currSuite = "First Record";
        private string name;
        private string start;
        private string end;
        private string elapsed;
        private string notes;
        private int passed = 0;
        private int failed = 0;
        private int suiteCnt = 0;
        private long regressionID;
        private long suiteID;
        PersistenceManager pm;

        private readonly int error = -1;
        private readonly int warning = 1;
        private readonly int success = 0;

        //private ArrayList scripts = new ArrayList();

        
        // construct a suite
        public Suite(PersistenceManager pm, long regressionID) { this.pm = pm; this.regressionID = regressionID; }

        public void create(string suiteRecord)
        {

            // create a script 
            Script script = new Script(pm);
            // get the suite name
            Match suiteName = Regex.Match(suiteRecord, RegPatterns.getSuiteNamePattern(), RegexOptions.IgnoreCase);
            if (suiteName.Success)
            {
                this.name = suiteName.Groups[1].Value;
            }
            else
            {
                Logger.write2log(error,"Unable to locate the suite name in this record: " + suiteRecord);
                return;

            }

            
            if (currSuite.Equals(name))
            {
                // process the current suite records 

                Match p = Regex.Match(suiteRecord, RegPatterns.getStatusPattern(), RegexOptions.IgnoreCase);
                if (p.Success) {               
                    passed++;
                } else {
                    failed++;
                }
                suiteCnt++;

                // get suite end time
                Match endTime = Regex.Match(suiteRecord, RegPatterns.getStartTimePattern(), RegexOptions.IgnoreCase);
                if (endTime.Success)
                {
                    this.end = endTime.Groups[0].Value;
                }
                else { this.end = "Unable to find end time in suite records: " + suiteRecord; }
                // calculate elasped time
                elapsed = Util.calculateElapsed(start, end);

                // update database with end-time and elapsed-time
                pm.updateSuite(suiteID, end, elapsed, passed, failed);

                // add the script for this suite to the script table
                script.addScript(suiteID, suiteRecord, name);
            }

            /*
             * We have a new suite record, create a new suite record in the database and add the
             * create a script.
             */  
            
            else
            {   
                // get the suite start time 
                Match startTime = Regex.Match(suiteRecord, RegPatterns.getStartTimePattern(), RegexOptions.IgnoreCase);
                if (startTime.Success)
                {
                    this.start = startTime.Groups[0].Value;
                }
                else { this.start = "Not found"; }

                // get suite end time
                Match endTime = Regex.Match(suiteRecord, RegPatterns.getEndTimePattern(), RegexOptions.IgnoreCase);
                if (endTime.Success)
                {
                    this.end = endTime.Groups[1].Value;
                }
                else { this.end = "Unable to find end time in suite records: " + suiteRecord; }
                // calculate elasped time
                elapsed = Util.calculateElapsed(start, end);

                // Create suite record in the database and zero out counters
                suiteID = pm.InsertSuite(regressionID, name, start, end, elapsed);

                Logger.write2log(success, "Inserting Suite " + name + " with ID: " + suiteID);
                passed = 0;
                failed = 0;
                suiteCnt = 0;

                // add the script for this suite to the script table
                script.addScript(suiteID, suiteRecord, name);


            }
            currSuite = this.name;
            return;
        }
        public string getName() { return this.name; }
    }
}
