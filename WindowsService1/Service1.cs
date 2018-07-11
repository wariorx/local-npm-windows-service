using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Windows.Forms;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.IO;

//TODO: 
// Allow for config to be used to set PORT (-p) flag for local-npm command
// Add event logging
namespace WindowsService1
{
    public partial class Service1 : ServiceBase
    {

        private Process p = null;
        private const string POWERSHELL = "powershell.exe";
        private const string PROCESS = "local-npm";
        private FileStream log = null;

        public Service1()
        {
            InitializeComponent();

            InitializeLog();
        }

        protected override void OnStart(string[] args)
        {
            WriteToLog("Service-Start received");
            StartShellAndNpm();

            return;

        }

        protected override void OnStop()
        {
            WriteToLog("Service-Stop received");

            if (this.p != null && !this.p.HasExited)
            {
                WriteToLog("Stopping the spawned processes");

                StopLocalNpm();

                StopPowerShell();

                base.ExitCode = 0;
            }

            else
            {
                WriteToLog("FAILED process was started, but exited before service was stopped");
                base.ExitCode = 1;
            }

            CloseLog();

            base.OnStop();

            return;
        }

        /// <summary>
        /// Spawns a powershell instance and runs local-npm on it
        /// </summary>
        /// <param name="path"></param>
        private void StartShellAndNpm()
        {
            WriteToLog("Starting powershell with local-npm running");

            ProcessStartInfo info = SetProcessInfo();

            this.p = System.Diagnostics.Process.Start(info);

            if (this.p != null && !p.HasExited)
                WriteToLog("Local-npm started");
            else
            {
                WriteToLog("FAILED to start local-npm");
                base.ExitCode = 1;
                base.OnStop(); //stop process, since it failed to start
            }

            return;
        }

        /// <summary>
        /// Set the process's start info based on the config
        /// TODO: have it read from config rather than hardcoded values
        /// </summary>
        /// <param name="path"></param>
        /// <returns name="info"></returns>
        private ProcessStartInfo SetProcessInfo()
        {
            WriteToLog("Setting process info");

            ProcessStartInfo info = new ProcessStartInfo();

            info.CreateNoWindow = false;
            info.UseShellExecute = true;
            info.FileName = POWERSHELL;
            info.Arguments = PROCESS;

            return info;

        }

        [System.Runtime.InteropServices.DllImport("User32.dll")]
            static extern int SetForegroundWindow(IntPtr point);
        /// <summary>
        /// Stops local-npm running in the shell spawned by this service
        /// </summary>
        private void StopLocalNpm()
        {
            WriteToLog("Stopping local-npm");

            IntPtr h = p.MainWindowHandle;
            SetForegroundWindow(h);
            SendKeys.SendWait("^c"); //send cntrl + c to powershell
            SendKeys.SendWait("y~");  //confirm, send y and then enter


            WriteToLog("Stopped local-npm");

            return;
        }

        /// <summary>
        /// Stops powershell instance spawned by this service
        /// </summary>
        private void StopPowerShell()
        {
            WriteToLog("Stopping Powershell");

            this.p.Kill();

            WriteToLog("Stopped Powershell");

            return;
        }

        //TODO: read log path from config file rather than hard coded string
        private void InitializeLog()
        {
            
            this.log = new FileStream("C:\\Users\\rcalcam\\Documents\\local-npm-service.log", FileMode.OpenOrCreate, FileAccess.ReadWrite);

            return;
        }

        private byte[] StringToByteArray(string s)
        {
            return new UTF8Encoding(true).GetBytes(s);
        }

        private void WriteToLog(string s)
        {
           byte[] a = StringToByteArray(s);
           this.log.Write( a, 0, a.Length );

            byte[] newline = Encoding.ASCII.GetBytes(Environment.NewLine);
            log.Write(newline, 0, newline.Length);
        }

        private void CloseLog()
        {
            WriteToLog("Closing Log");

            this.log.Close();
        }
    }
}
