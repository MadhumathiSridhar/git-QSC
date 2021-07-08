using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Reflection;
using System.Data.SqlClient;

namespace QSC_Test_Automation
{
    /// <summary>
    /// Interaction logic for SelectServer.xaml
    /// </summary>
    public partial class SelectServer : Window
    {
        public SelectServer()
        {
            InitializeComponent();

          
                string[] serverlist;
                string Allservers = Properties.Settings.Default.availableservers.ToString();
                serverlist = Allservers.Split(',');
                foreach (string server in serverlist)
                {
                    ServerCombo.Items.Add(server);
                }

            if (Properties.Settings.Default.DebugMode == true && !ServerCombo.Items.Contains("OTHERS"))
                ServerCombo.Items.Add("OTHERS");
        }

        private void ServerOkClick(object sender, RoutedEventArgs e)
        {
            try
            {
                SelectServerFunc(ServerCombo.SelectionBoxItem.ToString());
                this.Close();
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
            }
        }

        public void SelectServerFunc(string ServerName)
        {
            try
            {
                if (ServerName == null || ServerName == string.Empty)
                {
                    MessageBox.Show("Please choose server to proceed", "Warning", MessageBoxButton.OK);
                    return;
                }

                QatConstants.SelectedServer = ServerName;
                var connectionString = ConfigurationManager.ConnectionStrings["ConString"].ConnectionString;
                var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                var connectionStringsSection = (ConnectionStringsSection)config.GetSection("connectionStrings");

              switch (ServerName)
                {
                    case ("COSTAMESA_Production"):
                      {
                            connectionStringsSection.ConnectionStrings["ConString"].ConnectionString = Properties.Settings.Default.ConstringCM;
                            break;
                        }
                    case ("BOULDER_Production"):
                        {
                            connectionStringsSection.ConnectionStrings["ConString"].ConnectionString = Properties.Settings.Default.ConstringBDR;
                            break;
                        }
                    case ("JASMIN_Production"):
                        {
                            connectionStringsSection.ConnectionStrings["ConString"].ConnectionString = Properties.Settings.Default.ConstringJAS;
                            break;
                        }
                    case ("BANGALORE_Production"):
                        {
                            connectionStringsSection.ConnectionStrings["ConString"].ConnectionString = Properties.Settings.Default.ConstringBLR;
                            break;
                        }
                    case ("BOULDER_Sandbox"):
                        {
                            connectionStringsSection.ConnectionStrings["ConString"].ConnectionString = Properties.Settings.Default.ConstringBDR;
                            break;
                        }
                    case ("COSTAMESA_Sandbox"):
                        {
                            connectionStringsSection.ConnectionStrings["ConString"].ConnectionString = Properties.Settings.Default.ConstringCM;
                            break;
                        }
                    case ("JASMIN_Sandbox"):
                        {
                            connectionStringsSection.ConnectionStrings["ConString"].ConnectionString = Properties.Settings.Default.ConstringJAS;
                            break;
                        }
                    case ("BANGALORE_Sandbox"):
                        {
                            connectionStringsSection.ConnectionStrings["ConString"].ConnectionString = Properties.Settings.Default.ConstringBLR;
                            break;
                        }
                }

                Properties.Settings.Default.currentserver = ServerName;
                string finalreleasePath = QatConstants.ReleaseFolderPAth;
                string finalreportPath = QatConstants.Reportpath;
                string finalserverPath = QatConstants.QATServerPath;

                //if (Properties.Settings.Default.DebugMode)
                //{
                //    Properties.Settings.Default.QRCMServerName = Properties.Settings.Default.DebugQRCMserverName;
                //}
                //else
                //{
                //    SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(connectionStringsSection.ConnectionStrings["ConString"].ConnectionString);
                //    Properties.Settings.Default.QRCMServerName = builder.DataSource;
                //}

                Properties.Settings.Default.Save();
                config.Save();
                ConfigurationManager.RefreshSection("connectionStrings");
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
            }
        }

        //        private void CloseClick(object sender, System.ComponentModel.CancelEventArgs e)
        //        {
        //            try
        //            {
        //                if (ServerCombo.SelectionBoxItem != null && ServerCombo.SelectionBoxItem.ToString() != string.Empty)
        //                {
        //                    SelectServerFunc(ServerCombo.SelectionBoxItem.ToString());
        //                    e.Cancel = false;
        //                }
        //                else
        //                {
        //                    MessageBox.Show("Please choose server to proceed", "Warning", MessageBoxButton.OK);
        //                    e.Cancel = true;
        //                }
        //            }
        //            catch (Exception ex)
        //            {
        //                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
        //#if DEBUG
        //                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
        //#endif
        //            }
        //        }
    }



}
