using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Data;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Security.Policy;
using System.Security.Principal;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using MySql.Data.MySqlClient;
using System.Text.RegularExpressions;

namespace PublishResults
{
    public class PersistenceManager
    {
        string connectionString;
        string db;
        private readonly int error   = -1;
        private readonly int warning = 1;
        private readonly int success = 0;

        public PersistenceManager(string connectionString, string db)
        {
            this.connectionString = connectionString;
            this.db = db;
        }

        public long InsertRegression(int prodID, int siteID, int osID, int lparID, int subsysID, string buildInfo, string name, string runtime, string endtime, string elapsed, string rts, int passed, int failed)
        {
            MySqlConnection myConn = new MySqlConnection(this.connectionString);
            myConn.Open();
            long id;

            try
            {
                var stm = @"INSERT INTO `" + db + "`.regression (productID, siteID, osID, lparID, subsystemID, name, buildInfo, start, end, elapsed, rts, passed, failed) VALUES(@productID, @siteID, @osID, @lparID, @subsystemID, @name, @buildInfo, @start, @end, @elapsed, @rts, @passed, @failed)";
                MySqlCommand cmd = new MySqlCommand(stm, myConn);
                
                cmd.Parameters.AddWithValue("@productID", prodID);
                cmd.Parameters.AddWithValue("@siteID", siteID);
                cmd.Parameters.AddWithValue("@osID", osID);
                cmd.Parameters.AddWithValue("@lparID", lparID);
                cmd.Parameters.AddWithValue("@subsystemID", subsysID);
                cmd.Parameters.AddWithValue("@name", name);
                cmd.Parameters.AddWithValue("@buildInfo", buildInfo);
                cmd.Parameters.AddWithValue("@start", runtime);
                cmd.Parameters.AddWithValue("@end", endtime);
                cmd.Parameters.AddWithValue("@elapsed", elapsed);
                cmd.Parameters.AddWithValue("@rts", rts);
                cmd.Parameters.AddWithValue("@passed", passed);
                cmd.Parameters.AddWithValue("@failed", failed);

                id = cmd.ExecuteNonQuery();

                // get the inserted record ID
                id = cmd.LastInsertedId;
  
                if (id > 1)
                {
                    Logger.write2log(success, name + ":  added to regression table with ID: " + id + " and productID of: " + prodID);
                }
                else
                {
                    Logger.write2log(error, "Unable to add: " + name + " to regression table.");
                }
            }
            catch (Exception ev)
            {
                Logger.write2log(error, ev.Message);
                id = -1;
            }
            myConn.Close();
            return id;
        }
        public int setStatusCount(long regID, string type, int count)
        {
            MySqlConnection myConn = new MySqlConnection(this.connectionString);
            myConn.Open();
            int rc;

            try
            {
                var stm = @"UPDATE `" + 
                    db +
                    "`.regression SET " +
                    type +
                    "= @count WHERE regressionID = @regressionID";
                MySqlCommand cmd = new MySqlCommand(stm, myConn);

                cmd.Parameters.AddWithValue("@regressionID", regID);
                cmd.Parameters.AddWithValue("@count", count);
                rc = cmd.ExecuteNonQuery();
            }
            catch (Exception ev)
            {
                Logger.write2log(-1, ev.Message);
                rc = -1;

            }
            myConn.Close();
            return rc;
        }

        public long updateSuite(long suiteID, string endTime, string elasped, int passed, int failed)
        {
            MySqlConnection myConn = new MySqlConnection(this.connectionString);
            myConn.Open();
            int rc;

            try
            {
                var stm = @"UPDATE `" + db + @"`.suite SET end = @end, elapsed = @elapsed, passed = @passed, failed = @failed  WHERE suiteID = @suiteID";
                MySqlCommand cmd = new MySqlCommand(stm, myConn);

                cmd.Parameters.AddWithValue("@suiteID", suiteID);
                cmd.Parameters.AddWithValue("@end", endTime);
                cmd.Parameters.AddWithValue("@elapsed", elasped);
                cmd.Parameters.AddWithValue("@passed", passed);
                cmd.Parameters.AddWithValue("@failed", failed);

                rc = cmd.ExecuteNonQuery();

                // get the inserted record ID
                //long suiteID = cmd.LastInsertedId;
                
            }
            catch (Exception ev)
            {
                Logger.write2log(error, ev.Message);
                rc =  -1;

            }
            myConn.Close();
            return rc;
        }
        public long InsertArtifact(long scriptID, string table, string name, string contents)
        {
            MySqlConnection myConn = new MySqlConnection(this.connectionString);
            myConn.Open();
            long aID;
            
            // get only the file name
            string fileName =  Path.GetFileName(name);
            // map directory name to table name 
            switch (table) 
            {
                case "exp_data":
                    table = "exp_files";
                    break;
                case "current_data":
                    table = "current";
                    break;
                case "Differences":
                    table = "differences";
                    break;
                default:
                    Logger.write2log(error, "Unable to map " + table );
                    break;
            }
            try
            {
                var stm = @"INSERT INTO `" + db + @"`." + table + @" (scriptID, name, contents,visible) VALUES(@scriptID, @name, @contents, @visible)";
                MySqlCommand cmd = new MySqlCommand(stm, myConn);

                cmd.Parameters.AddWithValue("@scriptID", scriptID);
                cmd.Parameters.AddWithValue("@name", fileName);
                cmd.Parameters.AddWithValue("@contents", contents);
                cmd.Parameters.AddWithValue("@visible", 1);

                cmd.ExecuteNonQuery();

                // get the inserted record ID
                aID = cmd.LastInsertedId;
                if (aID > 1)
                {
                    Logger.write2log(success, name + ":  added to " + table + " with ID: " + aID + " and scriptID of: " + scriptID);
                }
                else
                {
                    Logger.write2log(error, "Unable to add: " + name + " to " +  table + ".");
                }
            }
            catch (Exception ev)
            {
                Logger.write2log(error,ev.Message);
                aID = -1;
            }
            myConn.Close();
            return aID;
        }
        public long InsertSuite(long regID, string name, string start, string end, string elapsed)
        {
            MySqlConnection myConn = new MySqlConnection(this.connectionString);
            myConn.Open();

            long suiteID;

            try
            {
                var stm = @"INSERT INTO `" + db + @"`.suite (regressionID, name, start, end, elapsed) VALUES(@regressionID, @name, @start, @end, @elapsed)";
                MySqlCommand cmd = new MySqlCommand(stm, myConn);

                cmd.Parameters.AddWithValue("@regressionID", regID);
                cmd.Parameters.AddWithValue("@name", name);
                cmd.Parameters.AddWithValue("@start", start);
                cmd.Parameters.AddWithValue("@end", end);
                cmd.Parameters.AddWithValue("@elapsed", elapsed);

                cmd.ExecuteNonQuery();

                // get the inserted record ID
                suiteID = cmd.LastInsertedId;

                if (suiteID > 1)
                {
                    Logger.write2log(success, "Suite: " + name + " added to suite table with ID: " + suiteID + " and regressionID of: " + regID);
                }
                else
                {
                    Logger.write2log(error, "Unable to add suite: " + name + " to suite table.");
                }
            }
            catch (Exception ev)
            {
                Logger.write2log(error, ev.Message);
                suiteID = -1;
            }
            myConn.Close();
            return suiteID;
        }
        public int getStatusCount(long regID, string status)
        {

            var stm = "SELECT  COUNT(*) AS total ";
            stm = stm + "FROM `" + db + @"`.regression, `" + db + @"`.suite, `" + db + @"`.script ";
            stm = stm + "WHERE  ";
            stm = stm + "regression.regressionID = suite.regressionID ";
            stm = stm + "AND suite.suiteID = script.suiteID ";
            stm = stm + "AND regression.regressionID = @regID ";
            stm = stm + "AND script.status = @status ";

            MySqlConnection myConn = new MySqlConnection(this.connectionString);
            myConn.Open();
          
            MySqlCommand cmd = new MySqlCommand(stm, myConn);
            cmd.Parameters.AddWithValue("@regID", regID);
            cmd.Parameters.AddWithValue("@status", status);
            MySqlDataReader rdr = null;

            string strNum = "";
            int numVal = -1;
            try
            {
                rdr = cmd.ExecuteReader();
         
                if (rdr.Read())
                {
                    strNum = rdr.GetString(0);

                }
            }
            catch (Exception ev)
            {
                Logger.write2log(error, ev.Message);
            }
            rdr.Close();
            numVal = Convert.ToInt32(strNum);
            return numVal;
        }
        public long InsertScript(long suiteID, string name, string source, string start, string end, string elapsed, string status, string log)
        {
            MySqlConnection myConn = new MySqlConnection(this.connectionString);
            myConn.Open();
            long scriptID;

            int rc = 0;

            try
            {
                var stm = @"INSERT INTO `" + db + @"`.script (suiteID, name, source, start, end, elapsed, status, log) VALUES(@suiteID, @name, @source, @start, @end, @elapsed, @status, @log)";
                MySqlCommand cmd = new MySqlCommand(stm, myConn);

                cmd.Parameters.AddWithValue("@suiteID", suiteID);
                cmd.Parameters.AddWithValue("@name", name);
                cmd.Parameters.AddWithValue("@source", source);
                cmd.Parameters.AddWithValue("@start", start);
                cmd.Parameters.AddWithValue("@end", end);
                cmd.Parameters.AddWithValue("@elapsed", elapsed);
                cmd.Parameters.AddWithValue("@status", status);
                cmd.Parameters.AddWithValue("@log", log);

                rc = cmd.ExecuteNonQuery();

                // get the inserted record ID
                scriptID = cmd.LastInsertedId;
                if (scriptID > 1)
                {
                    Logger.write2log(success, "Script " + name + " added to script table with ID: " + scriptID + " and suiteID of: " + suiteID);
                }
                else
                {
                    Logger.write2log(error, "Unable to add script: " + name + " to script table.");
                }
            }
            catch (Exception ev)
            {
                Logger.write2log(error, ev.Message);
                scriptID = -1;
            }
            myConn.Close();
            return scriptID;
        }

    }
}
        
