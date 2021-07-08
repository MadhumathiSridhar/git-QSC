namespace QSC_Test_Automation
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Data;
    using System.Linq;
    using System.Windows;
    using System.Diagnostics;
    using System.Runtime.InteropServices;
    using System.IO;

    using System.Threading;
    using Microsoft.VisualBasic;
    using Properties;
    using Microsoft.Win32.TaskScheduler;

    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        [DllImport("user32.dll")]
        private static extern int SetForegroundWindow(IntPtr hWnd);
        private const int SW_SHOWNORMAL = 1;
        private const int SW_SHOWMAXIMIZED = 3;
        private const int SW_RESTORE = 9;
        private const int SW_MINIMIZE = 6;
        [DllImport("user32.dll")]

        private static extern
     bool IsIconic(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern
       bool ShowWindowAsync(IntPtr hWnd, int nCmdShow);



        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        private void Application_Exit(object sender, ExitEventArgs e)
        {
           
              
                Application.Current.Shutdown();

        }
        


          public Tuple<bool, string,string> Findlatestexe(string Releasepath, string currentversion)

        {

            string temp_path = string.Empty;
            bool UpdateisTrue = false;
            string Updatepath = string.Empty;
            string updateversion_name = string.Empty;
            try
            {
                  Version newversion = new Version();
                  DirectoryInfo dir = new DirectoryInfo(Releasepath);
                  //DirectoryInfo[] dircollection = dir.GetDirectories();
                  //  foreach (DirectoryInfo direct in dircollection)
                  //  {
                        FileInfo[] Totalfiles = dir.GetFiles();
                        if (Totalfiles.Length > 0)
                        {
                        foreach (FileInfo currentfile in Totalfiles)
                        {
                           
                            var versionInformation = FileVersionInfo.GetVersionInfo(System.IO.Path.Combine(dir.FullName.ToString(), currentfile.Name));
                            if ((versionInformation.ProductName != null) & (versionInformation.ProductName != string.Empty))
                            {
                                if (versionInformation.ProductName.StartsWith("QSC Automation Toolbox"))
                                {
                                    Version temp_version = Version.Parse(versionInformation.FileVersion);
                                    if (temp_version != null)
                                    {
                                        if (temp_version > newversion)
                                        {
                                            newversion = temp_version;
                                           temp_path = currentfile.FullName;
                                            updateversion_name=versionInformation.ProductVersion;

                                        }
                                    }
                                }
                            }
                        }

                    }

                //}
               
                if (newversion > (Version.Parse(currentversion)))
                {
                    UpdateisTrue = true;
                    Updatepath = temp_path;
                }

                return new Tuple<bool, string,string>(UpdateisTrue, Updatepath, updateversion_name);

            }
            catch(Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif

                return new Tuple<bool, string,string>(UpdateisTrue, Updatepath, updateversion_name);

            }
        }

        public bool callinstall(string destDirName)
        {
            Process process = new Process();
            try
            {
                //To Skip User Account Control setting dialog
                //TaskService ts = new TaskService();
                //TaskDefinition td = ts.NewTask();
                //td.Principal.RunLevel = TaskRunLevel.Highest;
                //td.Triggers.AddNew(TaskTriggerType.Logon);
                //td.Actions.Add(new ExecAction(destDirName, null));
                //ts.RootFolder.RegisterTaskDefinition("SilentInstall", td);

                //To Perform Installation silently
               
                //process.StartInfo.UseShellExecute = true;
                //process.StartInfo.CreateNoWindow = false;
                process.StartInfo.FileName = destDirName;
                //process.StartInfo.Arguments = string.Format(" /s /v /qb");
                process.Start();
                Environment.Exit(0);
               return true;
            }
            catch (Exception ex)
            {
                if (!process.HasExited)
                {
                    process.Close();
                }
                //DeviceDiscovery.WriteToLogFile("Installation of Designer setup failed due to Administrator rights");
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif

                return false;
            }
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            int count = 0;
            try
            {            
                foreach (Process clsProcess in Process.GetProcesses())
                {
                    if (clsProcess.ProcessName.ToString() == "QSC_Test_Automation")
                    {
                        count++;
                        if (count > 1)
                        {
                            IntPtr hWnd = clsProcess.MainWindowHandle;
                            if (IsIconic(hWnd))
                            {
                                ShowWindowAsync(hWnd, SW_RESTORE);
                                // ShowWindow(clsProcess.MainWindowHandle, SW_RESTORE);

                                SetForegroundWindow(clsProcess.MainWindowHandle);
                            }

                            Environment.Exit(0);
                        }
                    }
                }

                //bool isPrefUpgrade = Settings.Default.upgradeedr;

                if (Settings.Default.upgradeedr)
                {
                    Settings.Default.Upgrade();
                    SelectServer serverinstance = new SelectServer();
                    serverinstance.ShowDialog();

                    Settings.Default.upgradeedr = false;
                    Settings.Default.Save();
                    //Settings.Default.Reload();

         
                }
                else
                {
                    if(Settings.Default.DebugMode == false && Settings.Default.ServerSwitch == false)
                    {
                        QatConstants.SelectedServer = Settings.Default.currentserver;
                        string finalreleasePath = QatConstants.ReleaseFolderPAth;
                        string finalreportPath = QatConstants.Reportpath;
                        string finalserverPath = QatConstants.QATServerPath;
                    }
                    else
                    {
                        SelectServer serverinstance = new SelectServer();
                        serverinstance.ShowDialog();
                    }
                }


                /// prefrence upgrade code ends here


                // /////// Application update alert code starts here
                //if (count<1)
                //{
                if (e.Args.Length != 1)
                {
                    string ReleaseFolderPath = string.Empty;
                    ReleaseFolderPath = @QatConstants.ReleaseFolderPAth;
                    System.Reflection.Assembly Assembly_object = System.Reflection.Assembly.GetExecutingAssembly();
                    FileVersionInfo Fileinfo_assembly = FileVersionInfo.GetVersionInfo(Assembly_object.Location);
                    string version = Fileinfo_assembly.FileVersion;

                    if (Directory.Exists(ReleaseFolderPath))
                    {
                        var Updateresult = Findlatestexe(ReleaseFolderPath, version);
                        if (Updateresult.Item1)
                        {

                            MessageBoxResult result = MessageBox.Show("Newer Version of QSC Automation Toolbox " + Updateresult.Item3.ToString() + " is available, Do you want to update?", "Update Alert", MessageBoxButton.OKCancel, MessageBoxImage.Information);
                            if (result == MessageBoxResult.OK)

                            {
                                ////invoke code goes here
                                bool callsuccess = callinstall(Updateresult.Item2);
                                if (callsuccess)
                                    Environment.Exit(0);

                            }
                        }
                    }
                }               

                if (e.Args.Length == 1)
                {
                    DeviceDiscovery.ConfigFileName = e.Args[0];
                }

  				//string keyfilePath = Path.Combine(Settings.Default.ServerPath, "SshKey");
      //          if (!Directory.Exists(keyfilePath))
      //              if (hasWriteAccessToFolder(Settings.Default.ServerPath))
      //                  Directory.CreateDirectory(keyfilePath);

            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif


            }
            //}

            ////////// Application update alert code starts here

        }
        public bool hasWriteAccessToFolder(string folderPath)
        {
            try
            {
                System.Security.AccessControl.DirectorySecurity ds = Directory.GetAccessControl(folderPath);

                if (ds != null)
                {
                    using (FileStream fs = new FileStream(folderPath + "\\tempFile.tmp", FileMode.CreateNew, FileAccess.Write))
                    {
                        fs.WriteByte(0xff);
                    }

                    if (File.Exists(folderPath + "\\tempFile.tmp"))
                    {
                        File.Delete(folderPath + "\\tempFile.tmp");
                        return true;
                    }
                }

                return false;
            }
            catch (UnauthorizedAccessException)
            {
                return false;
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                //MessageBox.Show("Exception\n " + ex.Message, "QAT Error Code - EC08001", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }
    }

  }



    