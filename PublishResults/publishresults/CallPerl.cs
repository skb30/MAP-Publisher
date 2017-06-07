using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;using System.Diagnostics;

namespace PublishResults

    
{
    class CallPerl
    {
        private string scriptName;

        // constructor
        public CallPerl(string scriptName)
        {
            this.scriptName = scriptName;
        }

        public void runPerlScript()
        {
            // try to execute a perl script
            ProcessStartInfo perlStartInfo = new ProcessStartInfo(@"perl.exe");
            perlStartInfo.Arguments = @"c:\hello.pl " + "scott " + "joe";
            perlStartInfo.UseShellExecute = false;
            perlStartInfo.RedirectStandardOutput = true;
            perlStartInfo.RedirectStandardError = true;
            perlStartInfo.CreateNoWindow = false;

            Process perl = new Process();
            perl.StartInfo = perlStartInfo;
            perl.Start();
            perl.WaitForExit();
            string output = perl.StandardOutput.ReadToEnd();
            string error = perl.StandardError.ReadToEnd();

            System.Windows.MessageBox.Show(output);
            //System.Windows.MessageBox.Show(error);
        }
    }
}
