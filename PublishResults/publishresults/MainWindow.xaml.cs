using System;
using System.Collections;
using System.IO;
using System.Data;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Security.Policy;
using System.Security.Principal;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using MySql.Data.MySqlClient;
using System.Text.RegularExpressions;

namespace PublishResults
{
    public class OptionsLoader
    {
        private readonly MainWindow _mainWindow;
        private readonly int error = -1;
        private readonly int warning = 1;
        private readonly int success = 0;

        public OptionsLoader(MainWindow mainWindow)
        {
            this._mainWindow = mainWindow;
        }

        public void Seed(String database)
        {
            Util.write2log(success, "*********************************************************************************************************");
            Util.write2log(success, "Starting MAP Publisher..");
            Util.write2log(success, "*********************************************************************************************************");
            string db;
            if (database.Equals("PROD"))
            {
                db = "`MAP2`";
            }
            else
            {
                db = "`MAP2-TEST`";
            }

            Util.write2log(success, "Publishing to Database: " + db);

            String myConnection = "server=usmghcentos65;port=3306;uid=scottba;pwd=qamap;database=MAP2";
            MySqlConnection myConn = new MySqlConnection(myConnection);
            myConn.Open();

            // load the product box with products from the database
            string stm = "SELECT * FROM " + db + ".product WHERE visible = '1' ORDER BY product.name ASC, product.release ASC";
            // Product[] products = Load_Obj_Box(myConn, stm);
            string[] products = Load_Combo_Box(myConn, stm);

            // load the os box with os release from the database
            stm = "SELECT * FROM " + db + ".os ORDER BY os.version ASC";
            string[] osversions = Load_Combo_Box(myConn, stm);

            // load the LPAR box with LPARs from the database
            stm = "SELECT * FROM  " + db + ".lpar ORDER BY lpar.name ASC";
            string[] lpars = Load_Combo_Box(myConn, stm);

            // load the Subsytem box with Subsystems from the database
            stm = "SELECT * FROM  " + db + ".subsystem ORDER BY subsystem.name ASC, subsystem.version ASC";
            string[] subsystems = Load_Combo_Box(myConn, stm);

            // load the Site box with Sites from the database
            stm = "SELECT * FROM  " + db + ".site ORDER BY site.name ASC";
            string[] sites = Load_Combo_Box(myConn, stm);

            string buildInfo = Load_Build_Info();

            string buildName = Load_Build_Name();

            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() => PopulateOptions(products, osversions, lpars, subsystems, sites, buildInfo, buildName)));
            myConn.Close();
        }

        private void PopulateOptions(string[] products, string[] osVersions, string[] lpars, string[] subsystems, string[] sites, string buildInfo, string buildName)
        {

            _mainWindow.ProductBox.ItemsSource = new ObservableCollection<string>(products.ToList());
            _mainWindow.OsVersionBox.ItemsSource = new ObservableCollection<string>(osVersions.ToList());
            _mainWindow.LparBox.ItemsSource = new ObservableCollection<string>(lpars.ToList());
            _mainWindow.SubsystemBox.ItemsSource = new ObservableCollection<string>(subsystems.ToList());
            _mainWindow.SiteBox.ItemsSource = new ObservableCollection<string>(sites.ToList());
            _mainWindow.BuildInfo.Text = buildInfo;
            _mainWindow.BuildName.Text = buildName;

            //preload defaults
            _mainWindow.ProductBox.SelectedIndex = 0;
            _mainWindow.OsVersionBox.SelectedIndex = 0;
            _mainWindow.LparBox.SelectedIndex = 0;
            _mainWindow.SubsystemBox.SelectedIndex = 0;
            _mainWindow.SiteBox.SelectedIndex = 0;

            _mainWindow.MainGrid.IsEnabled = true;


        }
        private string Load_Build_Name()
        {
            string firstRecord, buildName;
            using (StreamReader reader = new StreamReader("suitelog.txt"))
            {
                firstRecord = reader.ReadLine();
                Match header = Regex.Match(firstRecord, RegPatterns.getHeaderPattern(), RegexOptions.IgnoreCase);
                if (header.Success)
                {
                    // buildInfo group.
                    buildName = header.Groups[2].Value;
                }
                else
                {
                    buildName = "Unable to find suite-log header record!";
                }
            }

            return buildName;
        }
        private string Load_Build_Info()
        {
            string firstRecord, buildInfo;
            using (StreamReader reader = new StreamReader("suitelog.txt"))
            {
                firstRecord = reader.ReadLine();
                Match header = Regex.Match(firstRecord, RegPatterns.getHeaderPattern(), RegexOptions.IgnoreCase);
                if (header.Success)
                {
                    // buildInfo group.
                    buildInfo = header.Groups[1].Value;
                    //buildNum  = header.Groups[2].Value;
                }
                else
                {
                    buildInfo = "Unable to find suite-log header record!";
                }
            }

            return buildInfo;
        }
        private string[] Load_Combo_Box(MySqlConnection myConn, string stm)
        {
            MySqlCommand cmd = new MySqlCommand(stm, myConn);
            MySqlDataReader rdr = null;
            rdr = cmd.ExecuteReader();

            string[] records = new string[1000];

            int i = 0;
            while (rdr.Read())
            {
                records[i] = rdr.GetString(0) + " - " + rdr.GetString(1) + " - " + rdr.GetString(2);
                i++;
            }
            rdr.Close();
            return records;

        }
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly int error = -1;
        private readonly int warning = 1;
        private readonly int success = 0;
        BackgroundWorker regression_bw = new BackgroundWorker { WorkerReportsProgress = true, WorkerSupportsCancellation = true };
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Submit_Click(object sender, RoutedEventArgs e)
        {
            // create an object that contains the regression data that needs to be 
            // passed to other threads
            RegressionData regressionData = new RegressionData();

            try
            {
                // re-initialize the log file
                File.Delete("publisher.txt");

                // we need to pass this data to the background thread that will load the database

                // load the object 
                regressionData.pv = ProductBox.SelectedValue.ToString();
                regressionData.prod = ProductBox.SelectedItem.ToString();
                regressionData.path2log = Location.Content.ToString();
                regressionData.os = OsVersionBox.SelectedItem.ToString();
                regressionData.lpar = LparBox.SelectedItem.ToString();
                regressionData.sub = SubsystemBox.SelectedItem.ToString();
                regressionData.site = SiteBox.SelectedItem.ToString();
                regressionData.buildInfo = BuildInfo.Text;
                regressionData.regName = BuildName.Text;
                regressionData.testDB = (bool)DB_TEST.IsChecked;
                regressionData.prodDB = (bool)DB_PROD.IsChecked;
                regressionData.tb = LogMessages;

                // log it
                Util.write2log(success, "Creating Regression for product: " + regressionData.prod);
                Util.write2log(success, "Using Suite log: " + regressionData.path2log);
                Util.write2log(success, "OS version: " + regressionData.os);
                Util.write2log(success, "LPAR: " + regressionData.lpar);
                Util.write2log(success, "Subsytem: " + regressionData.sub);
                Util.write2log(success, "Site: " + regressionData.site);
                Util.write2log(success, "BuildInfo: " + regressionData.buildInfo);
                Util.write2log(success, "Build Name: " + regressionData.regName);
            }
            catch (NullReferenceException)
            {
                MessageBox.Show("Please select the required options.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            catch (IOException io)
            {
                io.Message.ToString();
            }
            //BackgroundWorker bw = new BackgroundWorker { WorkerReportsProgress = true, WorkerSupportsCancellation = true };
            regression_bw.DoWork += bw_addRegression;

            regression_bw.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bw_RunWorkerCompleted);
            // turn on progress bar
            MainProgress.IsIndeterminate = true;
            Submit.IsEnabled = false;
            regression_bw.RunWorkerAsync(regressionData);

        }
        private int getID(string record)
        {
            // get only the IDs
            string idMask = @"(\d+)\s-\s";
            string sid;
            int id;

            Match match = Regex.Match(record, idMask, RegexOptions.IgnoreCase);
            if (match.Success)
            {
                sid = match.Groups[1].Value;
            }
            else
            {
                sid = "-1";
            }
            id = Convert.ToInt32(sid);
            return id;
        }

        private void OpenAbout(object sender, RoutedEventArgs e)
        {
            Window about = new About();
            about.Show();
        }


        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Init();
        }

        private void bw_Init(object sender, DoWorkEventArgs e)
        {
            var worker = sender as BackgroundWorker;

            if (worker != null && worker.CancellationPending)
            {
                e.Cancel = true;
            }
            else
            {
                var optionsLoader = new OptionsLoader(this);
                //optionsLoader.Seed("TEST");
            }
        }

        private void bw_SeedTest(object sender, DoWorkEventArgs e)
        {
            var worker = sender as BackgroundWorker;

            if (worker != null && worker.CancellationPending)
            {
                e.Cancel = true;
            }
            else
            {
                // Perform the time consuming operation
                var optionsLoader = new OptionsLoader(this);
                optionsLoader.Seed("TEST");
            }
        }

        private void bw_SeedProduction(object sender, DoWorkEventArgs e)
        {
            var worker = sender as BackgroundWorker;

            if (worker != null && worker.CancellationPending)
            {
                e.Cancel = true;
            }
            else
            {
                var optionsLoader = new OptionsLoader(this);
                optionsLoader.Seed("PROD");
            }
        }

        //private void bw_ProgressChanged(object sender, ProgressChangedEventArgs e)
        //{
        //    MainProgress.Value = e.ProgressPercentage;
        //}

        private void bw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            // turn off the progress bar
            MainProgress.IsIndeterminate = false;


            if (!(e.Error == null))
            {
                System.Windows.MessageBox.Show(e.Error.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            MainGrid.IsEnabled = true;
            Submit.IsEnabled = true;
        }

        private void Path_Button_Clicked(object sender, RoutedEventArgs e)
        {
            // Configure open file dialog box
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.FileName = "suitelog"; // Default file name
            dlg.DefaultExt = ".txt"; // Default file extension
            dlg.Filter = "MAP logs (.txt)|*.txt"; // Filter files by extension 



            // Show open file dialog box
            Nullable<bool> result = dlg.ShowDialog();

            // Process open file dialog box results 
            if (result == true)
            {
                // Open document 
                string filename = dlg.FileName;
                Location.Content = filename;
                // get the path from the filename
                string pathname = Path.GetDirectoryName(filename);
                // Change the current directory.
                Environment.CurrentDirectory = (pathname);
            }

        }
        private void Init()
        {
            /* 
             * Launch the app without loading the boxes. The boxes will get loaded when the user 
             * selects which database they want to work with. 
             */

            // setup the console log 
            //Console.SetWindowSize(80, 43);
            //Console.SetWindowPosition(0, 0);
            //Console.Title = "Publish Results Log Window";
            //Console.ForegroundColor = ConsoleColor.Green;

            var optionsLoader = new OptionsLoader(this);
            // create a background process
            BackgroundWorker bw = new BackgroundWorker { WorkerReportsProgress = true, WorkerSupportsCancellation = true };

            // add an event handler to the DoWork event
            bw.DoWork += bw_Init;

            // add an event handler to the RunWorker event
            bw.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bw_RunWorkerCompleted);
            // turn on progress bar
            // set IsIndeterminate to true because there is no way of knowing the current progress of this operation
            MainProgress.IsIndeterminate = true;

            // start the background process
            bw.RunWorkerAsync();

        }
        private void bw_addRegression(object sender, DoWorkEventArgs e)
        {
            try
            {
                // call a perl script test
                /*
                CallPerl cp = new CallPerl(@"c:\hello.pl");
                cp.runPerlScript();
                */

                /*
                 get the data from the UI drop-downs. Because this is a background process we need to get
                 the arguments from the object we loaded from the UI process. 
                 */
                var regressionData = (RegressionData)e.Argument;

                // get the IDs 
                int prodID = getID(regressionData.pv);
                int osID = getID(regressionData.os);
                int lparID = getID(regressionData.lpar);
                int subsysID = getID(regressionData.sub);
                int siteID = getID(regressionData.site);
                bool testDB = regressionData.testDB;
                string dbName;

                // what database did the user select
                if (testDB)
                {
                    dbName = "MAP2-TEST";
                }
                else
                {
                    dbName = "MAP2";
                }

                Regression r = new Regression(regressionData.path2log);
                Logger l = new Logger(regressionData.path2log);
                long regID = r.createRegression(dbName, prodID, siteID, osID, lparID, subsysID, regressionData.buildInfo, regressionData.regName);

                Logger.write2log(success, "Results for [" + r.BuildName + "] successfully added to regression table with regressionID of: " + regID + ".");
                Logger.write2log(success, "*************************Publish Results Ended ****************************");

            }
            catch (Exception ev)
            {

                Logger.write2log(error, ev.Message);
                Logger.write2log(error, ev.StackTrace);
            }

        }
        private void Populate(bool production)
        {

            BackgroundWorker bw = new BackgroundWorker { WorkerReportsProgress = true, WorkerSupportsCancellation = true };
            if (production)
            {
                bw.DoWork += bw_SeedProduction;
            }
            else
            {
                bw.DoWork += bw_SeedTest;
            }

            bw.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bw_RunWorkerCompleted);
            // turn on progress bar
            MainProgress.IsIndeterminate = true;
            bw.RunWorkerAsync();
        }

        private void DB_TEST_Checked(object sender, RoutedEventArgs e)
        {
            // check to see if the path to the suitelog has been set
            string path = Directory.GetCurrentDirectory();

            string suitelog = path + @"\suitelog.txt";

            if (File.Exists(suitelog))
            {
                Populate(false);
            }
            else
            {
                System.Windows.MessageBox.Show("Unable to find the suitelog. Make sure your path is set correctly.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                DB_TEST.IsChecked = false;
                return;

            }
        }

        private void DB_PROD_Checked(object sender, RoutedEventArgs e)
        {
            // check to see if the path to the suitelog has been set
            string path = Directory.GetCurrentDirectory();

            string suitelog = path + @"\suitelog.txt";

            if (File.Exists(suitelog))
            {
                Populate(true);
            }
            else
            {
                System.Windows.MessageBox.Show("Unable to find the suitelog. Make sure your path is set correctly. ");
                DB_TEST.IsChecked = false;
                return;

            }
        }

        private void Log_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //MessageBox.Show(Directory.GetCurrentDirectory());
                LogMessages.Text = (File.ReadAllText("publisher.txt"));
            }
            catch (Exception ev)
            {
                ev.ToString();
            }

        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.MessageBox.Show("Cancel Button Clicked. ");
            if (regression_bw.WorkerSupportsCancellation == true)
            {

                regression_bw.CancelAsync();
            }
        }
    }
}
