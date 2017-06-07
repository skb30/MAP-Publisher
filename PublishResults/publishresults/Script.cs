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
    class Script
    {
        private PersistenceManager pm;
        int rc;
        private readonly int error = -1;
        private readonly int warning = 1;
        private readonly int success = 0;

        public Script(PersistenceManager pm)
        {
            this.pm = pm;
            Logger.write2log(success, "Script instance created");
        }

        public long addScript(long suiteID, string suiteRecord, string suiteName)
        {
            string currDir = Directory.GetCurrentDirectory();
            // get the script name 
            string scriptName = Util.getScriptInfo(suiteRecord, RegPatterns.getScriptPattern());

            // change to script directory
            rc = Util.changeDir(suiteName + @"/" + scriptName);
            // get the script source code
            string scriptSource = Util.getSourceContents(scriptName + ".pl");
            // get the start time 
            string scriptStartTime = Util.getScriptInfo(suiteRecord, RegPatterns.getStartTimePattern());
            // convert the start time to run time format
            DateTime runtime = Util.convertMAPdate(scriptStartTime);
            scriptStartTime = runtime.ToString("MMMM dd, yyyy hh:mm:ss");
            // get the end time
            string scriptEndTime = Util.getScriptInfo(suiteRecord, RegPatterns.getEndTimePattern());
            DateTime endtime = Util.convertMAPdate(scriptEndTime);
            scriptEndTime = endtime.ToString("MMMM dd, yyyy hh:mm:ss");
            // calcuate elasped time
            string elaspedTime = Util.calculateElapsed(scriptStartTime, scriptEndTime);
            // get status
            string status = Util.getScriptStatus(suiteRecord);
            // get the log contents
            string log = Util.getFileContents("runlog.txt");

            // commit to db
            long scriptID = pm.InsertScript(suiteID, scriptName, scriptSource, scriptStartTime, scriptEndTime, elaspedTime, status, log);

            // add the expected files to the db
            Util.loadArtifacts(scriptID, pm, "exp_data", "*.txt");
            Util.loadArtifacts(scriptID, pm, "current_data", "*.txt");
            Util.loadArtifacts(scriptID, pm, "Differences", "*.txt");

            // go back to the current directory because we're leaving
            rc = Util.changeDir(currDir);

            return scriptID;
        }
    }
}
