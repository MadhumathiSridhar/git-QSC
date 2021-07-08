using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Text.RegularExpressions;
using System.Data.SqlClient;
using System.Data;

namespace QSC_Test_Automation
{
    /// <summary>
    /// Interaction logic for ServerDetails.xaml
    /// </summary>
    public partial class ServerDetails : Window
    {
        public static string ServerPath = string.Empty;
        SqlDataAdapter adap = null;
        private DBConnection connect = new DBConnection();
        //DBConnection dbConnect = new DBConnection();
        DataTable Datatable1 = new DataTable();
         

        public ServerDetails()
        {
            
            InitializeComponent();
            try
            {
                //txt_ServerName.Text = Properties.Settings.Default.Path.ToString();
                txt_DeviceUsername.Text = Properties.Settings.Default.DeviceUsername.ToString();
                txt_DevicePassword.Text = Properties.Settings.Default.DevicePassword.ToString();
                txt_Telnetusername.Text = Properties.Settings.Default.TelnetUserName.ToString();
                txt_Telnetpassword.Text = Properties.Settings.Default.TelnetPassword.ToString();
                txtDesignVersion.Text = Properties.Settings.Default.Designversion.ToString();
                //txtDutConfig_Delay.Text = Properties.Settings.Default.DutConfigDelay.ToString();
                txt_Qsystemppath.Text = Properties.Settings.Default.Qsystemppath.ToString();
                txt_testerName.Text = Properties.Settings.Default.TesterName.ToString();
                Custom_timeout.Text = Properties.Settings.Default.Bypass_time.ToString();
                string getQATprefix = Properties.Settings.Default.QATPrefix.ToString();
                txt_browserPath.Text = Properties.Settings.Default.BrowserPath.ToString();
                ReflectUsername_txt.Text = Properties.Settings.Default.QREMUserName.ToString();
                Reflectpassword_txt.Text = Properties.Settings.Default.QREMPassword.ToString();

                //bool Execution_Running = DeviceDiscovery.IsExecutionRunning();
                
                //if (Execution_Running)
                //{
                //    ReflectUsername_lbl.ToolTip = null;
                //    ReflectPassword_lbl.ToolTip = null;
                //    ReflectUsername_txt.IsEnabled = true;
                //    Reflectpassword_txt.IsEnabled = true;                    
                //}
                //else
                //{
                //    ReflectUsername_lbl.ToolTip = "During execution QREM change not allowed";
                //    ReflectPassword_lbl.ToolTip = "During execution QREM change not allowed";
                //    ReflectUsername_txt.IsEnabled = false;
                //    Reflectpassword_txt.IsEnabled = false;
                //}

                //txt_QATPrefix.ClearValue;
                //string temp1= temp.Replace("\r\n", string.Empty);     

                getQATprefix = getQATprefix.Replace("\n", string.Empty);
                string[] Array_QATprefix = getQATprefix.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
                //if(!temp2.Last().ToString().Contains(';'))
                //{
                //    temp2 = temp2 + ";";

                //}
                // //txt_QATPrefix.Text = string.Empty;
                string splitted_value = string.Join(";\n", Array_QATprefix);
                txt_QATPrefix.Text = splitted_value + ";";


                //txt_email_ID.Text = Properties.Settings.Default.EmailID.ToString();
                //txt_Category.Text= Properties.Settings.Default.Category.ToString();


                txt_net_pair.Text = DeviceDiscovery.Netpair_devicesSupported;
                if (Properties.Settings.Default.Qsyscheckbox.ToString() == "true")
                {
                    QsysTempPath.IsChecked = true;
                    txt_Qsystemppath.IsEnabled = true;
                }
                if (Properties.Settings.Default.ServerSwitch == true)
                {
                    Server_Switch.IsChecked = true;
                }
                if (Properties.Settings.Default.Bypass_checkvalue == true)
                {
                    Device_init_timeout.IsChecked = true;
                     Custom_timeout.IsEnabled = true;
                  }
                //if (Properties.Settings.Default.Restorecheckbox.ToString() == "true")
                //{
                //    restoredesign.IsChecked = true;

                //}
                //if (Properties.Settings.Default.Restorecheckbox.ToString() == "false")
                //{
                //    restoredesign.IsChecked = false;

                //}
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                //MessageBox.Show("Exception\n " + ex.Message, "QAT Error Code - EC03021", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        


        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //bool chkServerNameExist = GetServerName();
                bool chkTelnetNameandpassword = GetTelnetusername();
                //bool chkTelnetPassword = GetTelnetpassword();
                //bool chkDut = GetDutConfig_Delay();
                bool chkQsysTemp = GetQsystemppath();
                bool chkNetpair = Get_NET_PAIR();
                //bool chkemailid = Get_Email_ID();
                bool chkDesignversion = Get_Design_version();
                bool bypasstime = GetBypasstime();
                bool isuserNameEmpty = true;

                Properties.Settings.Default.BrowserPath = txt_browserPath.Text.Trim();             
            

                Properties.Settings.Default.DevicePassword = txt_DevicePassword.Text.Trim();
                if(txt_DeviceUsername.Text.Trim()!= string.Empty && txt_DeviceUsername.Text.Trim()!= null)
                {
                    Properties.Settings.Default.DeviceUsername = txt_DeviceUsername.Text.Trim();
                }
                else
                {
                    MessageBox.Show("Please enter device username.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    isuserNameEmpty = false;
                }

                Properties.Settings.Default.QREMPassword = Reflectpassword_txt.Text.Trim();
         
                
                Properties.Settings.Default.QREMUserName = ReflectUsername_txt.Text.Trim();                

                //Properties.Settings.Default.Designversion = txtDesignVersion.Text;
                Properties.Settings.Default.TesterName = txt_testerName.Text.Trim();
                if (Server_Switch.IsChecked == true)
                    Properties.Settings.Default.ServerSwitch = true;
                else
                    Properties.Settings.Default.ServerSwitch = false;
                string[] getprefix_value = txt_QATPrefix.Text.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries);                
                string splitted_value_save = string.Join(";", getprefix_value);
                txt_QATPrefix.Text = splitted_value_save;
                splitted_value_save = Regex.Replace(splitted_value_save, @"\t|\n|\r", "");
                Properties.Settings.Default.QATPrefix = splitted_value_save;
               
                Properties.Settings.Default.Save();
               
                if (QsysTempPath.IsChecked == false)
                {
                    chkQsysTemp = true;
                }

                if (chkTelnetNameandpassword & chkQsysTemp & chkNetpair & chkDesignversion & isuserNameEmpty)
                {
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                //MessageBox.Show("Exception\n " + ex.Message, "QAT Error Code - EC08001", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool Get_Design_version()
        {
            try
            {
                if (txtDesignVersion.Text != string.Empty)
                {
                    if (txtDesignVersion.Text.EndsWith("Q-Sys Designer.exe"))
                    {

                        bool filepather = System.IO.File.Exists(txtDesignVersion.Text);
                        if (filepather)
                        {
                            Properties.Settings.Default.Designversion = txtDesignVersion.Text.Trim();
                            return true;
                        }
                        MessageBox.Show("Q-Sys designer exe doesn't exists in the path", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return false;
                    }
                    else
                    {
                        MessageBox.Show("Please select only Q-Sys designer exe", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return false;
                    }
                }

                MessageBox.Show("Please select Q-sys designer version", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                //MessageBox.Show("Exception\n " + ex.Message, "QAT Error Code - EC03021", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        //private bool Get_Email_ID()
        //{

        //    string pattern = null;
           
        //    if (txt_email_ID.Text.Length > 0)
        //    {
        //        pattern = "^([0-9a-zA-Z]([-\\.\\w]*[0-9a-zA-Z])*@([0-9a-zA-Z][-\\w]*[0-9a-zA-Z]\\.)+[a-zA-Z]{2,9})$";

        //        if (Regex.IsMatch(txt_email_ID.Text.Trim(), pattern))
        //        {
        //            Properties.Settings.Default.EmailID = txt_email_ID.Text.Trim();
        //            return true;
        //        }
        //        else
        //        {
        //            MessageBox.Show("Please enter Valid email address", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
        //            return false;
        //        }
        //    }

        //    Properties.Settings.Default.EmailID = txt_email_ID.Text.Trim();
        //    return false;
        //}

        private void QsysTempPath_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (QsysTempPath.IsChecked == true)
                {
                    txt_Qsystemppath.IsEnabled = true;
                }
                else
                {
                    txt_Qsystemppath.IsEnabled = false;
                }
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                //MessageBox.Show("Exception\n " + ex.Message, "QAT Error Code - EC08007", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        //private bool GetServerName()
        //{
        //    if (txt_ServerName.Text == string.Empty)
        //    {
        //        MessageBox.Show("Server path is not entered", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
        //        return false;
        //    }
        //    else if (!Directory.Exists(txt_ServerName.Text))
        //    {
        //        MessageBox.Show("Invalid Server path", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
        //        return false;
        //    }
        //    else
        //    {
        //        bool validFolderChk = hasValidFolder(txt_ServerName.Text);

        //        if (validFolderChk)
        //        {
        //            bool hasreadAccess = hasWriteAccessToFolder(txt_ServerName.Text);

        //            if (hasreadAccess)
        //            {
        //                ServerPath = txt_ServerName.Text;
        //                //Properties.Settings.Default.Path = txt_ServerName.Text.Trim();
        //                return true;
        //            }
        //            else
        //            {
        //                MessageBox.Show("The server path is read only", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
        //                return false;
        //            }
        //        }
        //        else
        //        {
        //            MessageBox.Show("Invalid Server path", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
        //            return false;
        //        }
        //    }
        //}

        private bool GetTelnetusername()
        {
            bool returnvalue = false;
            try
            {
                string[] usern = null; string[] usernpass = null;
                ///check username and password is empty
                /// 
                if ((txt_Telnetusername.Text.Trim() != string.Empty)&& (txt_Telnetpassword.Text.Trim() != string.Empty))
                {
                    if ((!txt_Telnetusername.Text.Trim().Contains(',')) && (!txt_Telnetpassword.Text.Trim().Contains(',')))
                    {
                       
                        if (txt_Telnetusername.Text.Trim().Length>=30)
                        {
                            MessageBox.Show("Telnet User name should not be more than or equal to 30 characters", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return false;

                        }
                        else
                        {
                            Properties.Settings.Default.TelnetPassword = txt_Telnetpassword.Text.Trim();
                            Properties.Settings.Default.TelnetUserName = txt_Telnetusername.Text.Trim();
                            returnvalue = true;
                        }
                        
                        
                    }
                    else 
                    {
                        if (txt_Telnetusername.Text.Trim().Contains(','))
                        {
                            usern = txt_Telnetusername.Text.Trim().Split(',');
                            if (usern.Contains(string.Empty))
                            {
                                MessageBox.Show("Telnet User name should not be empty", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                              return false;
                            }
                           foreach(string value in  usern)
                                if(value.Length>=30)
                                {
                                    MessageBox.Show("Telnet User name should not be more than or equal to 30 characters", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                                    return false;
                                }
                               
                        }
                        else
                        {
                            usern = new string[1];
                            usern[0] = txt_Telnetusername.Text.Trim();
                        }
                       
                        if (txt_Telnetpassword.Text.Trim().Contains(','))
                        {
                            usernpass = txt_Telnetpassword.Text.Trim().Split(',');
                            if (usernpass.Contains(string.Empty))
                            {
                                MessageBox.Show("Telnet Password should not be empty", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                                return false;
                            }
                               
                        }
                        else
                        {
                            usernpass = new string[1];
                            usernpass[0] = txt_Telnetpassword.Text.Trim();
                        }

                        if ((!usern.Contains(string.Empty))&& (!usernpass.Contains(string.Empty)))
                        {
                            if (usernpass.Count() == usern.Count())
                            {
                                Properties.Settings.Default.TelnetPassword = txt_Telnetpassword.Text.Trim();
                                Properties.Settings.Default.TelnetUserName = txt_Telnetusername.Text.Trim();
                                returnvalue = true;
                            }
                            else
                            {
                                MessageBox.Show("Number of Telnet User name and Password mismatched", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                                return false;
                            }
                        }

                    }
              
                }
                else if((txt_Telnetusername.Text.Trim() == string.Empty)&& (txt_Telnetpassword.Text.Trim() != string.Empty))
                {
                    MessageBox.Show("Telnet username is not entered", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;

                }
                else if ((txt_Telnetpassword.Text.Trim() == string.Empty) && (txt_Telnetusername.Text.Trim() != string.Empty))
                {
                    MessageBox.Show("Telnet Password is not entered", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }
                else
                {
                    MessageBox.Show("Telnet Username and  Password is not entered", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;

                }





                return returnvalue;
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                //MessageBox.Show("Exception\n " + ex.Message, "QAT Error Code - EC03021", MessageBoxButton.OK, MessageBoxImage.Error);

                return returnvalue;
            }
        }

        private bool GetTelnetpassword()
        {
            try
            {
                if (txt_Telnetpassword.Text.Trim() != string.Empty)
                {
                    Properties.Settings.Default.TelnetPassword = txt_Telnetpassword.Text.Trim();
                    return true;
                }
                else
                {
                    MessageBox.Show("Telnet password is not entered", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }
            }
            catch(Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                //MessageBox.Show("Exception\n " + ex.Message, "QAT Error Code - EC03021", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        //private bool GetDutConfig_Delay()
        //{
        //    if (txtDutConfig_Delay.Text != string.Empty)
        //    {
        //        if (Convert.ToDouble(txtDutConfig_Delay.Text) > 0)
        //        {
        //            string dutValue = txtDutConfig_Delay.Text;

        //            if (Convert.ToDouble(txtDutConfig_Delay.Text) > 214748647)
        //            {
        //                txtDutConfig_Delay.Text = "214748647";
        //            }

        //            Properties.Settings.Default.DutConfigDelay = txtDutConfig_Delay.Text.Trim();

        //            if (Convert.ToDouble(dutValue) > 214748647)
        //            {
        //                MessageBox.Show("The Maximum allowed DUT Delay is '214748647'", "Warning", MessageBoxButton.OK, MessageBoxImage.Information);
        //            }

        //            return true;
        //        }
        //        else if ((txtDutConfig_Delay.Text != string.Empty) && (Convert.ToDouble(txtDutConfig_Delay.Text) <= 0))
        //        {
        //            MessageBox.Show("Enter UDP Delay value greater than Zero", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
        //            return false;
        //        }
        //        else if (txtDutConfig_Delay.Text == string.Empty)
        //        {
        //            MessageBox.Show("Please enter UDP Delay value in milliseconds", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
        //            return false;
        //        }

        //        return false;
        //    }

        //    Properties.Settings.Default.DutConfigDelay = txtDutConfig_Delay.Text.Trim();
        //    return true;
        //}

        private bool GetQsystemppath()
        {
            try
            {
                string qsystemptext = txt_Qsystemppath.Text;
                if (QsysTempPath.IsChecked == true)
                {
                    Properties.Settings.Default.Qsyscheckbox = "true";
                    bool ret = qsystemppath(qsystemptext);
                    Properties.Settings.Default.Save();
                    return ret;
                }
                if (QsysTempPath.IsChecked == false)
                {
                    Properties.Settings.Default.Qsyscheckbox = "false";
                    bool ret = qsystemppath(qsystemptext);
                    Properties.Settings.Default.Save();
                    return ret;
                }

                return false;
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                //MessageBox.Show("Exception\n " + ex.Message, "QAT Error Code - EC03021", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }
        private bool GetBypasstime()
        {
            try
            {
                string bypasstime = Custom_timeout.Text;
                if (Device_init_timeout.IsChecked == true)
                {
                    Properties.Settings.Default.Bypass_checkvalue = true;
                    if (Custom_timeout.Text.Trim() == string.Empty)
                    { Properties.Settings.Default.Bypass_time = 0; }
                    else
                    { Properties.Settings.Default.Bypass_time = Convert.ToInt32(Custom_timeout.Text.Trim()); }
                        
                    Properties.Settings.Default.Save();
                   
                }
                else
                {
                    Properties.Settings.Default.Bypass_checkvalue = false;
                    Properties.Settings.Default.Save();
                    
                }
                      
             

                return true;
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                //MessageBox.Show("Exception\n " + ex.Message, "QAT Error Code - EC03021", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        public bool qsystemppath(string qsystemptext)
        {
            try
            {
                if ((txt_Qsystemppath.Text != string.Empty) && (!Directory.Exists(txt_Qsystemppath.Text)) && (QsysTempPath.IsChecked == true))
                {
                    MessageBox.Show("Invalid Qsys temp path", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    Properties.Settings.Default.Qsyscheckbox = "false";
                    return false;
                }
                if (txt_Qsystemppath.Text == string.Empty)
                {
                    if (QsysTempPath.IsChecked == true)
                    {
                        MessageBox.Show("QSys Temp Path is not entered", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                        Properties.Settings.Default.Qsyscheckbox = "false";
                        return false;
                    }
                    else
                    {
                        Properties.Settings.Default.Qsystemppath = txt_Qsystemppath.Text.Trim();
                        Properties.Settings.Default.Save();
                        return true;
                    }
                }
                else
                {
                    if (QsysTempPath.IsChecked == true)
                    {
                        bool validFolderChk = hasValidFolder(txt_Qsystemppath.Text);

                        if (validFolderChk)
                        {
                            bool hasreadAccess = hasWriteAccessToFolder(txt_Qsystemppath.Text);

                            if (hasreadAccess)
                            {
                                if (txt_Qsystemppath.Text != string.Empty)
                                {
                                    Properties.Settings.Default.Qsystemppath = txt_Qsystemppath.Text.Trim();
                                    Properties.Settings.Default.Save();
                                    return true;
                                }

                                return false;
                            }

                            else { MessageBox.Show("Qsys temp path is read only", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning); }
                        }
                        return false;
                    }
                    return false;
                }
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                //MessageBox.Show("Exception\n " + ex.Message, "QAT Error Code - EC08008", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        public bool hasWriteAccessToFolder(string folderPath)
        {
            try
            {
                System.Security.AccessControl.DirectorySecurity ds = Directory.GetAccessControl(folderPath);

                if (ds != null)
                {
                    if (File.Exists(folderPath + "\\tempFile.tmp"))
                    {
                        File.Delete(folderPath + "\\tempFile.tmp");
                        return true;
                    }
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
            finally
            {
                if (File.Exists(folderPath + "\\tempFile.tmp"))
                {
                    File.Delete(folderPath + "\\tempFile.tmp");                   
                }
            }
        }

        public bool hasValidFolder(string folderPath)
        {
            try
            {
                System.Security.AccessControl.DirectorySecurity ds = Directory.GetAccessControl(folderPath);
                return true;
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif

                return false;
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if ((txt_Qsystemppath.Text == string.Empty) & (QsysTempPath.IsChecked == true))
                { Properties.Settings.Default.Qsyscheckbox = "false"; }
                this.Close();
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                //MessageBox.Show("Exception\n " + ex.Message, "QAT Error Code - EC08002", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Browse_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Microsoft.Win32.OpenFileDialog designVersion = new Microsoft.Win32.OpenFileDialog();
                designVersion.Filter = "Design exe (*.exe)|*.exe";
                if (designVersion.ShowDialog() == true)
                {
                    if (designVersion.FileName.EndsWith("Q-Sys Designer.exe"))
                    {
                        txtDesignVersion.Text = designVersion.FileName;
                    }
                    else
                    {
                        MessageBox.Show("Please select Qsys designer exe", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                //MessageBox.Show("Exception\n " + ex.Message, "QAT Error Code - EC08003", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Browser_Path_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Microsoft.Win32.OpenFileDialog path = new Microsoft.Win32.OpenFileDialog();
                path.Filter = "Exe Files (*.exe)|*.exe";
                if (path.ShowDialog() == true)
                {                   
                     txt_browserPath.Text = path.FileName;                   
                }
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                //MessageBox.Show("Exception\n " + ex.Message, "QAT Error Code - EC08003", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Txt_Delay_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            try
            {
                e.Handled = !IsTextAllowed(e.Text);
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                //MessageBox.Show("Exception\n " + ex.Message, "QAT Error Code - EC08004", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private static bool IsTextAllowed(string text)
        {
            Regex regex = null;
            try
            {
                regex = new Regex("[^0-9]"); ////regex that matches disallowed text
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                //MessageBox.Show("Exception\n " + ex.Message, "QAT Error Code - EC08005", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return !regex.IsMatch(text);
        }

        private void txt_Delay_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                string lstrCopyandPasteTxtBox = null;
                if (e.Handled == IsTextAllowed(e.ToString()))
                {
                    if (e.Key == Key.Space)
                    {
                        e.Handled = true;
                    }

                    base.OnPreviewKeyDown(e);
                }
                if (e.Key == Key.V && Keyboard.Modifiers == ModifierKeys.Control)
                {
                    e.Handled = false;
                    lstrCopyandPasteTxtBox = Clipboard.GetText();
                    if (e.Handled == IsTextAllowed(lstrCopyandPasteTxtBox))
                    {
                        e.Handled = true;
                    }
                    base.OnPreviewKeyDown(e);
                }
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                //MessageBox.Show("Exception\n " + ex.Message, "QAT Error Code - EC08006", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public bool Get_NET_PAIR()
        {
            try
            {
                if (txt_net_pair.Text.Trim() != string.Empty)
                {
                    string trimmednetpair = txt_net_pair.Text.Trim();
                    string[] Netpairdevices_splitted = trimmednetpair.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                    List<string> buffer_List = new List<string>();
                    List<string> duplicate_List = new List<string>();
                    foreach (string ter in Netpairdevices_splitted)
                    {
                        if (!buffer_List.Contains(ter))
                        { buffer_List.Add(ter); }
                        else
                        {
                            if (!duplicate_List.Contains(ter))
                            {
                                duplicate_List.Add(ter);
                            }

                        }
                    }
                    if (duplicate_List.Count == 0)
                    {
                        //var duplicateKeys = Netpairdevices_splitted.GroupBy(x => x).Where(group => group.Count() > 1).Select(group => group.);
                        //var Netpairdevices_splitted1= Netpairdevices_splitted.Distinct();
                        string query1 = "Delete FROM Netpairlist";
                        adap = new SqlDataAdapter(query1, this.connect.CreateConnection());
                        this.connect.OpenConnection();
                        adap.Update(Datatable1);
                        adap.Fill(Datatable1);
                        this.connect.CloseConnection();


                        string query = "Insert into Netpairlist values(@trimmednetpair)";
                        SqlCommand cmd = new SqlCommand(query, connect.CreateConnection());
                        cmd.Parameters.AddWithValue("@trimmednetpair", trimmednetpair);
                        SqlDataAdapter dataAdapter = new SqlDataAdapter(cmd);
                        //this.connect.OpenConnection();
                        dataAdapter.Fill(Datatable1);
                        this.connect.CloseConnection();

                        DeviceDiscovery.getnetpair();

                        return true;
                    }
                    else
                    {
                        string duplicates = string.Join("\n", duplicate_List.ToArray());
                        MessageBox.Show("Please remove duplicate entries for net pairing devices \n " + "\n" + duplicates + "", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return false;
                    }
                }
                else
                {
                    MessageBox.Show("No Net pairing devices is set", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                //MessageBox.Show("Exception\n " + ex.Message, "QAT Error Code - EC03021", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        private void Preference_keydown(object sender, KeyEventArgs e)
        {
         
            Window target = (Window)sender;
            if(e.Key==Key.Escape)
            {
                target.Close();
            }
            e.Handled = true;
            
        }

        private void TextBox_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            var textBox = e.OriginalSource as TextBox;

            if (textBox != null && e.Key == Key.Tab)
            {
                textBox.SelectAll();
            }
        }

        private void QATbutton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Microsoft.Win32.OpenFileDialog qatPrefixPath = new Microsoft.Win32.OpenFileDialog();
                qatPrefixPath.Filter = "QAT Prefix File (*.txt)|*.txt";
                if (qatPrefixPath.ShowDialog() == true)
                {
                        txt_QATPrefix.Text = qatPrefixPath.FileName;
                        
                }
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                //MessageBox.Show("Exception\n " + ex.Message, "QAT Error Code - EC08003", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }



        public string StoreQATPrefixFile(string QAT_Prefix_File)
        {
            try
            {
                string FilePath = "\\QATPrefixFile\\";
                FileInfo path = new FileInfo(QAT_Prefix_File);
                string QATPrefixpath = path.FullName;
                string QATPrefixfileName = path.Name;

                //if (Properties.Settings.Default.Path.ToString() != string.Empty)
                //{
                string PreferencesServerPath = QatConstants.QATServerPath + FilePath;
                if (!Directory.Exists(PreferencesServerPath))
                {
                    Directory.CreateDirectory(PreferencesServerPath);
                }

                if (!File.Exists(PreferencesServerPath  + QATPrefixfileName))
                {
                    File.Copy(QATPrefixpath + "", PreferencesServerPath  + QATPrefixfileName + "");
                    FileInfo fileInformation = new FileInfo(PreferencesServerPath + QATPrefixfileName + "");
                    fileInformation.IsReadOnly = true;
                }
                else
                {
                    File.SetAttributes(PreferencesServerPath + QATPrefixfileName, FileAttributes.Normal);
                    File.Delete(PreferencesServerPath + QATPrefixfileName);
                    File.Copy(QATPrefixpath + "", PreferencesServerPath + QATPrefixfileName + "");
                    FileInfo fileInformation = new FileInfo(PreferencesServerPath + QATPrefixfileName + "");
                    fileInformation.IsReadOnly = true;

                }
                return PreferencesServerPath + QATPrefixfileName;
                //}
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                //MessageBox.Show("Exception\n \n" + ex.Message, "QAT Error Code - ECxxxxRA", MessageBoxButton.OK, MessageBoxImage.Error);
                return string.Empty;
            }
        }

        private void Custom_timeout_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            try
            {
                e.Handled = !IsTextAllowed(e.Text);
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                // MessageBox.Show("Exception\n " + ex.Message, "QAT Error Code - EC12018", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Bypass_device_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (Device_init_timeout.IsChecked == true)
                {
                    Custom_timeout.IsEnabled = true;
                }
                else
                {
                    Custom_timeout.IsEnabled = false;
                }
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                //MessageBox.Show("Exception\n " + ex.Message, "QAT Error Code - EC08007", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void QRCMFileBrowse_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Microsoft.Win32.OpenFileDialog path = new Microsoft.Win32.OpenFileDialog();
                path.Filter = "Excel Files|*.xls;*.xlsx;*.xlsm";
                if (path.ShowDialog() == true)
                {
                    txt_qrcmFileUploadPath.Text = path.FileName;
                }
            }
            catch(Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
            }
        }



        private void QRCM_Upload_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (txt_qrcmFileUploadPath.Text != null && txt_qrcmFileUploadPath.Text.Trim() != string.Empty)
                {
                    Mouse.OverrideCursor = Cursors.Wait;
                    string excelfilepath = txt_qrcmFileUploadPath.Text.ToString();
                    var isSuccess= functionToreadData(excelfilepath);
                    Mouse.OverrideCursor = Cursors.Arrow;
                    if (isSuccess.Item1)
                    {
                       MessageBox.Show("QRCM file uploaded successfully", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                       DeviceDiscovery.Update_QRCMVersionList();                      
                    }
                    else
                    {
                        MessageBox.Show("QRCM file upload failed." + isSuccess.Item2, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else
                {
                    MessageBox.Show("Enter QRCM file path to upload", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
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


        public Tuple< bool,string> functionToreadData(string excelPath)
        {
            try
            {
                Microsoft.Office.Interop.Excel.Workbook xlWorkBook = null;
                Microsoft.Office.Interop.Excel.Application xlApp = new Microsoft.Office.Interop.Excel.Application();
                Microsoft.Office.Interop.Excel.Worksheet xlWorkSheet = null;
                Microsoft.Office.Interop.Excel.Range last = null;
                xlWorkBook = xlApp.Workbooks.Open(excelPath, 0, true, 5, "", "", true, Microsoft.Office.Interop.Excel.XlPlatform.xlWindows, "\t", false, false, 0, true, 1, 0);

                try
                {
                    System.Data.DataTable table = new System.Data.DataTable();
                    int rowValue;
                    int columnValue;
                    int rw = 0;
                    int cl = 0;

                    ////////Read excel workbook first sheet in datatable 
                    xlWorkSheet = xlWorkBook.Worksheets.get_Item(1);

                    last = xlWorkSheet.UsedRange;
                    rw = last.Rows.Count;
                    cl = last.Columns.Count;

                    for (rowValue = 1; rowValue <= rw; rowValue++)
                    {
                        DataRow workRow = table.NewRow();
                        for (columnValue = 1; columnValue <= cl; columnValue++)
                        {
                            var values_collect = xlWorkSheet.Cells[rowValue, columnValue].Value + "";
                            if (rowValue == 1)
                            {
                                table.Columns.Add(values_collect, typeof(string));
                            }
                            if (rowValue != 1)
                            {
                                workRow[table.Columns[columnValue - 1].ColumnName] = values_collect;
                            }
                        }

                        if (rowValue != 1)
                            table.Rows.Add(workRow);
                    }

                    /////// uploading excel data to database
                    if (table.Rows.Count > 0)
                    {
                        //////Get columns names from excel
                        List<string> columnNamesFromExcel = table.Columns.Cast<DataColumn>().Select(x => x.ColumnName).ToList();

                        if (columnNamesFromExcel.Contains("Project_Name") && columnNamesFromExcel.Contains("Build_version") && columnNamesFromExcel.Contains("Reference_Version") && columnNamesFromExcel.Contains("Method_Name"))
                        {
                            connect.OpenConnection();

                            //////Get columns names from DB - QRCMInitialization table
                            string columnQuery = "select COLUMN_NAME from INFORMATION_SCHEMA.COLUMNS where TABLE_NAME = '" + "QRCMInitialization" + "'";
                            Datatable1 = new DataTable();
                            SqlDataAdapter dataAdapter = new SqlDataAdapter(columnQuery, connect.CreateConnection());
                            dataAdapter.Fill(Datatable1);
                            List<string> columnNamesFromDB = Datatable1.Rows.OfType<DataRow>().Select(dr => dr.Field<string>(0)).ToList();                        
                            List<string> importdifference = columnNamesFromExcel.Except(columnNamesFromDB).ToList();                                                                                

                            /////Verifying column names from excel and columns names from db both or equal or not. If equal proceed DB writing else Show error message
                            if (columnNamesFromExcel.Count() == columnNamesFromDB.Count() && importdifference.Count==0)
                            {
                                ////Adding columns names into dictionary to do parameterization while inserting rows into database 
                                Dictionary<string, string> parameterwithValue = new Dictionary<string, string>();
                                for (int i = 0; i < columnNamesFromExcel.Count; i++)
                                {
                                    parameterwithValue.Add(columnNamesFromExcel[i], "@value" + i);
                                }

                                foreach (DataRow row in table.Rows)
                                {
                                    ////verifying each method in excel is already exists in database. if not exist then insert into db.
                                    //// if already exists comparing reference version in database with excel, if excel reference version is greater than db thenb delete the old method in db and insert the latest from excel
                                    string query = "SELECT Reference_Version FROM QRCMInitialization WHERE Project_Name='" + row["Project_Name"].ToString() + "' AND Build_version='" + row["Build_version"] + "' AND Method_Name='" + row["Method_Name"] + "'";

                                Datatable1 = new DataTable();
                                    dataAdapter = new SqlDataAdapter(query, connect.CreateConnection());
                                dataAdapter.Fill(Datatable1);
                                DataTableReader dataTableReader = Datatable1.CreateDataReader();

                                string referenceVer = string.Empty;
                                if (dataTableReader.HasRows)
                                {
                                    while (dataTableReader.Read())
                                        referenceVer = dataTableReader.GetString(0);
                                }
                             
                                bool skipInsert = false;
                                if (referenceVer != string.Empty)
                                {
                                    if (Convert.ToInt32(referenceVer) < Convert.ToInt32(row["Reference_Version"].ToString()))
                                    {
                                        query = "Delete from QRCMInitialization WHERE Project_Name='" + row["Project_Name"].ToString() + "' AND Build_version='" + row["Build_version"] + "' AND Method_Name='" + row["Method_Name"] + "'";
                                        Datatable1 = new DataTable();
                                        SqlCommand comm = new SqlCommand(query, connect.CreateConnection());                                       
                                        SqlDataAdapter dataAdapter2 = new SqlDataAdapter(comm);
                                        dataAdapter2.Fill(Datatable1);                                                
                                    }
                                    else
                                    {
                                        skipInsert = true;
                                    }
                                }


                                    if (skipInsert == false)
                                    {
                                        /////Database insert for each row
                                        string queryString = "IF NOT EXISTS (SELECT * FROM QRCMInitialization WHERE Project_Name='" + row["Project_Name"].ToString() + "' AND Build_version='" + row["Build_version"] + "'AND Reference_Version='" + row["Reference_Version"] + "' AND Method_Name='" + row["Method_Name"] + "')" +
                                        "INSERT INTO QRCMInitialization (" + string.Join(",", parameterwithValue.Keys.ToList()) + ")  VALUES(" + string.Join(",", parameterwithValue.Values.ToList()) + ")";

                                        SqlCommand comm = new SqlCommand(queryString, connect.CreateConnection());

                                        /////Actual value adding here for each column in a row
                                        for (int i = 0; i < columnNamesFromExcel.Count; i++)
                                        {
                                            comm.Parameters.AddWithValue(parameterwithValue[columnNamesFromExcel[i]], row[columnNamesFromExcel[i]]);
                                        }

                                        Datatable1 = new DataTable();
                                        SqlDataAdapter dataAdapter1 = new SqlDataAdapter(comm);
                                        dataAdapter1.Fill(Datatable1);
                                    }
                                }
                            }
                            else
                            {
                                connect.CloseConnection();
                                return new Tuple<bool, string>(false, " Selected file is invalid");
                            }

                            connect.CloseConnection();
                        }
                        else
                        {
                            return new Tuple<bool, string>(false, "Selected file is invalid");
                        }
                    }
                    else
                    {
                        return new Tuple<bool, string>(false, "Selected file has no data");
                    }

                    return new Tuple<bool, string>(true, string.Empty);

                }
                catch (Exception ex)
                {
                    connect.CloseConnection();
                    return new Tuple<bool, string>(false, ex.Message.ToString());
                }
                finally
                {
                    if (xlWorkBook != null)
                        xlWorkBook.Close();
                }
            }
            catch(Exception exc)
            {             
                return new Tuple<bool, string>(false, exc.Message.ToString());
            }       
        }

        private void Button_HideInventory_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (QREM_Grid.Visibility == Visibility.Visible)
                {
                    this.Height = 570;
                    QREM_Grid.Visibility = Visibility.Collapsed;
                }
                else
                {
                    this.Height = 655;
                    QREM_Grid.Visibility = Visibility.Visible;
                }
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                //MessageBox.Show("Exception\n " + ex.Message, "QAT Error Code - EC12062", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Window_MouseEnter(object sender, MouseEventArgs e)
        {
            try
            {
                bool Execution_Running = DeviceDiscovery.IsExecutionRunning();

                if (Execution_Running)
                {
                    ReflectUsername_txt.ToolTip = null;
                    Reflectpassword_txt.ToolTip = null;
                    ReflectUsername_txt.IsEnabled = true;
                    Reflectpassword_txt.IsEnabled = true;
                }
                else
                {
                    ReflectUsername_txt.ToolTip = "During execution QREM change not allowed";
                    Reflectpassword_txt.ToolTip = "During execution QREM change not allowed";
                    ReflectUsername_txt.IsEnabled = false;
                    Reflectpassword_txt.IsEnabled = false;
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
}
