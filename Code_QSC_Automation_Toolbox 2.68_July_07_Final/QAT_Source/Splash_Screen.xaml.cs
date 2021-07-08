namespace QSC_Test_Automation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;
    using System.Windows.Documents;
    using System.Windows.Input;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using System.Windows.Navigation;
    using System.Windows.Shapes;

    /// <summary>
    /// Interaction logic for Splash_Screen.xaml
    /// </summary>
    public partial class Splash_Screen : Window
    {
        public Splash_Screen()
        {
            this.InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                DBConnection connection = new DBConnection();   
                if (!connection.DataBaseConnection())
                {
                    MessageBox.Show("Unable to connect SQL database", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    Application.Current.Shutdown();
                    return;
                }
				
				DeviceDiscovery.startUpWindow = this;
                DeviceDiscovery.CreateLogFile();
				
                DeviceDiscovery.StartDiscoveryThread();
                if(DeviceDiscovery.ConfigFileName == null)
                    DeviceDiscovery.CreateRunnerWindow(false);
                else
                    DeviceDiscovery.CreateRunnerWindow(true);

                //Firmwareupgradation up = new Firmwareupgradation();
                //up.designersoftwareInstallFromURL("http://urda:8080/job/release/30/", "Q-Sys Designer");
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
