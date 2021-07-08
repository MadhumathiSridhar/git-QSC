using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.IO;
using System.Threading;
using System.Windows;
using QSC_Test_Automation;

namespace DUT_QSys
{
    /// <summary>
    /// Use to communicate with Q-Sys Designer GUI.
    /// This is NOT needed to send/recieve messages to the control engine.
    /// This IS needed to push a new design to a Q-Sys Core.
    /// </summary>
    public class QSys_Designer_Comm
    {
        [DllImport("user32.dll")]
        public static extern bool IsIconic(int Hwnd);

        Process qDesignProcess;
        ProcessStartInfo qDesignArgs = new ProcessStartInfo();
        //private QSys_Web_RW QWeb = new QSys_Web_RW();
        
         public enum SHOW_WINDOW 
         {
            SW_HIDE = 0,
            SW_SHOWNORMAL = 1,
            SW_NORMAL = 1,
            SW_SHOWMINIMIZED = 2,
            SW_SHOWMAXIMIZED = 3,
            SW_MAXIMIZE = 3,
            SW_SHOWNOACTIVATE = 4,
            SW_SHOW = 5,
            SW_MINIMIZE = 6,
            SW_SHOWMINNOACTIVE = 7,
            SW_SHOWNA = 8,
            SW_RESTORE = 9,
            SW_SHOWDEFAULT = 10,
            SW_FORCEMINIMIZE = 11,
            SW_MAX = 11,
         }

        [DllImport("User32")] 
        private static extern int ShowWindow(int hwnd, int nCmdShow);

        public QSys_Designer_Comm()
        { }

        ~QSys_Designer_Comm()
        {
            try { }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                //MessageBox.Show("Exception\n " + ex.Message, "QAT Error Code - EC06009", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Initialize path for Qsys Application
        /// </summary>
        /// <param name="QSD32">32-bit version</param>
        /// <param name="QSD64">64-bit version</param>
        public bool bInitialize_Design_Path(string strExePath)
        {
            qDesignArgs.FileName = "";

            try
            {
                if (File.Exists(strExePath))
                {
                    qDesignArgs.FileName = strExePath;
                    return true;
                }
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                //MessageBox.Show("Exception\n " + ex.Message, "QAT Error Code - EC06001", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return false;
        }

        /// <summary>
        /// Launches new instance of Q-Sys Designer, loads specified design and pushes that design 
        /// to the Core (within the design)
        /// </summary>
        /// <param name="strDesignPath">Path to Q-Sys Designer design file.</param>
        public void Load_and_Launch(string strDesignPath)
        {
            try
            {
                Kill();
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                //MessageBox.Show("Exception\n " + ex.Message, "QAT Error Code - EC06002", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            try
            {
                qDesignProcess = new Process();
                //qDesignArgs.Arguments = @"" + strDesignPath + " /dev /deploy" + "";
                qDesignArgs.Arguments = "\"" + strDesignPath + "\"" + " /dev /deploy";
                qDesignProcess = Process.Start(qDesignArgs);
                Debug.WriteLine("Qsys_Designer_Comm.Load_and_Launch : Initializing Qsys Designer Complete");
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                //MessageBox.Show("Exception\n " + ex.Message, "QAT Error Code - EC06003", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        //public void Connect_to_Running_Design(string strCoreIP)
        //{
        //    QSysDeviceStatus QDS = new QSysDeviceStatus();
        //    if (QWeb.get_QSysDeviceStatus(strCoreIP, ref QDS))
        //    {
        //        qDesignArgs.Arguments = "/compileId:" + QDS.DesignStatus.Code_Name;
        //        qDesignProcess = Process.Start(qDesignArgs);
        //    }
        //}

        /// <summary>
        /// Launches new instance of Q-Sys Designer, and emulates specified design. No Core is needed.
        /// </summary>
        /// <param name="strDesignPath">Path to Q-Sys Designer design file.</param>
        public void Emulate(string strDesignPath, string Filenamedesign)
        {
            try
            {

                if (qDesignProcess != null)
                {
                    qDesignProcess.Kill();
                }
                qDesignProcess = new Process();
                qDesignArgs.Arguments = strDesignPath;
                qDesignArgs.FileName = Filenamedesign;
                qDesignProcess = Process.Start(qDesignArgs);   //changed at Sean's recommendation to close open instances of app, 2/23/12 JG
                                                               //Hide();
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                //MessageBox.Show("Exception\n " + ex.Message, "QAT Error Code - EC06004", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Hide the Q-Sys Designer Window. Process must be running.
        /// </summary>
        public void Hide()
        {
            try
            {
                if (!qDesignProcess.HasExited)
                {
                    Debug.WriteLine("Q Design busy? : " + qDesignProcess.WaitForInputIdle());
                    while (!qDesignProcess.WaitForInputIdle())
                    {
                        Debug.WriteLine("Q Design busy? : " + qDesignProcess.WaitForInputIdle());
                    }
                    ShowWindow((int)qDesignProcess.MainWindowHandle, (int)SHOW_WINDOW.SW_HIDE);
                }
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                //MessageBox.Show("Exception\n " + ex.Message, "QAT Error Code - EC06005", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Show the Q-Sys Designer Window (that is hidden). Process must be running.
        /// </summary>
        public void Show()
        {
            try
            {
                if (!qDesignProcess.HasExited)
                {
                    ShowWindow((int)qDesignProcess.MainWindowHandle, (int)SHOW_WINDOW.SW_MAXIMIZE);
                }
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                //MessageBox.Show("Exception\n " + ex.Message, "QAT Error Code - EC06006", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Discovers other Designer processes and attempts to kill them.
        ///     This addresses the Windows XP bug and the Designer grabbing UDP sockets and not freeing them.
        /// </summary>
        public void KillOtherDesigners()
        {
            try
            {
                Process[] otherDesigners = Process.GetProcessesByName("Q-Sys Designer");

                if (otherDesigners.Length > 0)
                {
                    try
                    {
                        foreach (Process designProcess in otherDesigners)
                        {
                            if (!designProcess.HasExited)
                            {
                                designProcess.Kill();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif

                    }
                }
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                //MessageBox.Show("Exception\n " + ex.Message, "QAT Error Code - EC06007", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// End Q-Sys Designer process - Does NOT kill the control engine on the Core.
        /// </summary>
        public void Kill()
        {
            try
            {
                if (qDesignProcess != null)
                {
                    if (!qDesignProcess.HasExited)
                        qDesignProcess.Kill();
                }
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                //MessageBox.Show("Exception\n " + ex.Message, "QAT Error Code - EC06008", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
