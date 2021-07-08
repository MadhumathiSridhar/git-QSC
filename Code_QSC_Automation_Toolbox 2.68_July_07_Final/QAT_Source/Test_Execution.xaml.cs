namespace QSC_Test_Automation
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using System.Windows.Media;
    using System.Globalization;
    using System.Text;
    using System.Xml;
    using System.Net;
    using System.Text.RegularExpressions;
    using System.Configuration;
    using System.IO;
    using System.Diagnostics;
    using System.Threading;
    using QRAPI;
    using System.Windows.Shapes;
    using System.Windows.Data;
    using Utility;
    using System.ComponentModel;
    using System.Windows.Media.Imaging;
    using System.Windows.Threading;
    using System.Runtime.InteropServices;
    using Microsoft.Win32;    ///// <summary>
    using System.Collections.ObjectModel;    ///// Interaction logic for MainWindow.xaml
    using System.Threading.Tasks;
    using System.Data.SqlClient;

    ///// </summary>
    public partial class Test_Execution : Window,INotifyPropertyChanged
    {
        ExecutionProcess executionProcess = new ExecutionProcess();
        public Thread executionThread = null;
        AutoResetEvent autoevent = null;

        DUT_Configuration DUT_config = null;
   
        DatatableValues dbvalues = new DatatableValues();

        public List<DUT_DeviceItem> selectedDutDeviceItemList = new List<DUT_DeviceItem>();
        public List<DUT_DeviceItem> selectedExternalDeviceItemList = new List<DUT_DeviceItem>();

        public List<ExecutionLoop> selectedExecutionLoopList = new List<ExecutionLoop>();

        bool isExecutionPaused = false;
        public static bool IsAlreadyOpened = false;
        public string sortingOrders = string.Empty;
        public string sortingOrders_OriginalList = string.Empty;

        TreeViewExplorer treeViewExplorerExecutionRootItem = null;
        List<TreeViewExplorer> treeViewExplorerExecutionRootList = new List<TreeViewExplorer>();

        List<TreeViewExplorer> treeviewSelectionForCopy = new List<TreeViewExplorer>();
        List<TreeViewExplorer> treeViewExplorerInventoryList = new List<TreeViewExplorer>();
        List<TreeViewExplorer> treeViewExplorerSearchList = new List<TreeViewExplorer>();


        Dictionary<int, TreeViewExplorer> treeViewExplorerInventoryDictionary = new Dictionary<int, TreeViewExplorer>();
        List<TreeViewExplorer> selectedItemsInventoryToExecution = new List<TreeViewExplorer>();

        //Dictionary<int, TreeViewExplorer> selectedItemsInventoryToExecution = new Dictionary<int, TreeViewExplorer>();
        Dictionary<int, TreeViewExplorer> selectedItemsInventory = new Dictionary<int, TreeViewExplorer>();
        Dictionary<int, TreeViewExplorer> selectedItemsExecution = new Dictionary<int, TreeViewExplorer>();
        Dictionary<int, TreeViewExplorer> selectedItemsnonTS = new Dictionary<int, TreeViewExplorer>();
        Dictionary<int,int> selectedItemsnonTS_hash = new Dictionary<int, int>();
        ReportDBConnection report_connection = new ReportDBConnection();

        List<string> designerActionCoreName = new List<string>();
        System.Data.DataTable tble_rep = new System.Data.DataTable();

        bool skipMouseReleaseButton = false;
        bool isInventorySearchListSelected = false;

        bool InProcess = false;
        public bool autoevent_Reset_Delay = false;    

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool GetCursorPos(ref Win32Point pt);

        [StructLayout(LayoutKind.Sequential)]
        internal struct Win32Point
        {
            public Int32 X;
            public Int32 Y;
        };

        QatMessageBox QMessageBox = null;

        bool loadConfigFileInRunner = false;

        private bool msgAuto = false;
        public bool MsgAuto
        {
            get
            {
                return msgAuto;
            }
            set
            {
                msgAuto = value;
                OnPropertyChanged("MsgAuto");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        void OnPropertyChanged(string property)
        {
            try
            {
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs(property));
                }

            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                // MessageBox.Show("Exception\n " + ex.Message, "Error Code - ECxxxxx", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        public Test_Execution(bool loadConfigFile=false)
        {
            try
            {
                //DeviceDiscovery.Cancelclick = false;
                this.InitializeComponent();
                this.DataContext = this;
                Timer dutStatusUpdate = new Timer(new TimerCallback(DutStatusTimer));
                dutStatusUpdate.Change(0,10);

                if (Properties.Settings.Default.DebugMode)
                {
                    MemoryMonitorvisible = Visibility.Visible;
                    Timer memoryMonitorUpdate = new Timer(new TimerCallback(MemoryMonitorTimer));
                    memoryMonitorUpdate.Change(0, 1000);
                }
                else
                {
                    MemoryMonitorvisible = Visibility.Collapsed;
                }

                DeviceDiscovery.WriteToLogFile("***************************************************************");
                DeviceDiscovery.WriteToLogFile("QAT Starts.....");
             

                if (DeviceDiscovery.AvailableDeviceList.Count > 0)
                {
                    chk_ConfigurationFile.Foreground = Brushes.LightGreen;
                    //chk_ConfigurationFile.ClearValue(Button.ToolTipProperty);
                }
                else
                {
                    chk_ConfigurationFile.Foreground = Brushes.Red;                 
                }

                DBConnection connection = new DBConnection();
                bool designer_exits = connection.DataBaseConnection();
                ReportDBConnection report_connection = new ReportDBConnection();
                bool report_exists = report_connection.Report_DataBaseConnection();
                GrafanaDBConnection grafana_connection = new GrafanaDBConnection();
                bool grafana_exists = grafana_connection.DataBaseConnection();
                ////Add db dbversion to database


                if (designer_exits && report_exists && grafana_exists)
                {
                    string query = "SELECT Count(*) FROM INFORMATION_SCHEMA.TABLES";
                    SqlCommand cmd = new SqlCommand(query, connection.CreateConnection());
                    connection.OpenConnection();
                    int columnCount_Designer = Convert.ToInt32(cmd.ExecuteScalar());
                    connection.CloseConnection();
                   
                    cmd = new SqlCommand(query, report_connection.Report_CreateConnection_ForTables());
                    report_connection.Report_OpenConnection();
                    int columnCount_Runner = Convert.ToInt32(cmd.ExecuteScalar());
                    report_connection.Report_CloseConnection();

                    cmd = new SqlCommand(query, grafana_connection.CreateConnection());
                    grafana_connection.OpenConnection();
                    int columnCount_grafana = Convert.ToInt32(cmd.ExecuteScalar());
                    grafana_connection.CloseConnection();

                    bool dbversion = getdbversion();

                    if (!dbversion || columnCount_Designer == 0)
                    {
                        dbvalues.Get_tabledata();
                        connection.CloseConnection();

                        dbvalues.Get_TestSuitetabledata();
                        dbvalues.Get_TestModuletabledata();
                        dbvalues.Create_LinkingTables();
                        dbvalues.Create_TATables();                      
                    }

                    if(!dbversion || columnCount_Runner == 0)
                        report_connection.Create_Report_Tables();

                    if (!dbversion || columnCount_grafana == 0)
                        grafana_connection.Create_Tables();

                    DeviceDiscovery.getnetpair();
                }
                else
                {
                    Application.Current.Shutdown();
                }

                loadConfigFileInRunner = loadConfigFile;
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                //MessageBox.Show("Exception\n " + ex.Message, "Error Code - EC15001", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private string dutStatusValue = "Test Message";
        private string loopValue = string.Empty;
        private string ExecutionPaused = string.Empty;
        private DateTime RemainingTime;
        private TimeSpan CurrentRemainingTime;
        public string durationType = string.Empty;
        public bool  getdbversion()
        {
            dbvalues.Create_dbversion();
            System.Reflection.Assembly Assembly_object = System.Reflection.Assembly.GetExecutingAssembly();
            Version QATversion = Assembly_object.GetName().Version;
            bool dbversion = false;
            Version versionvalue = null;
            try
            {
                DBConnection dbConnect = new DBConnection();
                DataTable tbl1 = new DataTable();
                DataTableReader read1 = null;
                string query = "Select * from DBversion";

                SqlCommand sqlCmd1 = new SqlCommand(query, dbConnect.CreateConnection());
                SqlDataAdapter sqlDa1 = new SqlDataAdapter(sqlCmd1);
                sqlDa1.Fill(tbl1);
                read1 = tbl1.CreateDataReader();
                if (read1.HasRows)
                {
                    while (read1.Read())
                    {
                        if (read1.GetString(0).Trim() != string.Empty)
                        {
                            versionvalue = Version.Parse(read1.GetString(0));
                            if (QATversion <= versionvalue)
                                dbversion = true;
                            else
                            {
                                query = "update TOP(1) DBversion set version = ('" + QATversion + "')";                           
                                SqlCommand cmd = new SqlCommand(query, dbConnect.CreateConnection());
                                SqlDataAdapter dataAdapter = new SqlDataAdapter(cmd);
                                DataTable tbl2 = new DataTable();
                                dataAdapter.Fill(tbl2);
                            }

                        }
                        else
                        {
                            query = "update  TOP(1) DBversion set version = ('" + QATversion + "')";
                            SqlCommand cmd = new SqlCommand(query, dbConnect.CreateConnection());
                            SqlDataAdapter dataAdapter = new SqlDataAdapter(cmd);
                            DataTable tbl2 = new DataTable();
                            dataAdapter.Fill(tbl2);
                        }

                    }
                }
                else
                {
                    query = "Insert into DBversion values('" + QATversion + "')";
                    SqlCommand cmd = new SqlCommand(query, dbConnect.CreateConnection());
                    SqlDataAdapter dataAdapter = new SqlDataAdapter(cmd);
                    DataTable tbl2 = new DataTable();
                    dataAdapter.Fill(tbl2);
                }
                return dbversion;
            }
            catch (Exception ex)
            {

                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
               
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                return dbversion;
            }
        }
        private void loopValue1(string str)
        {
            this.Dispatcher.Invoke((Action)(() =>
            {
                loopValue = str;
            }));
        }

        private void RemainingTime1(DateTime str)
        {
            this.Dispatcher.Invoke((Action)(() =>
            {
                RemainingTime = str;
            }));
        }

        private void durationType1(string str)
        {
            this.Dispatcher.Invoke((Action)(() =>
            {
                durationType = str;
            }));
        }

        private void MemoryMonitorTimer(object state)
        {
            OnPropertyChanged("MemoryMonitor");
        }

        private string memoryMonitorValue = "0";

        public string MemoryMonitor
        {
            get
             {
                try
                {
                    if (Properties.Settings.Default.DebugMode)
                    {
                        var proc = System.Diagnostics.Process.GetCurrentProcess();
                        var mbUsed = (proc.PrivateMemorySize64 / 1024) / 1024;
                        var diagnostic = "Memory: " + mbUsed.ToString() + " MB";

                        //proc.Refresh();
                        //var ff = ((proc.WorkingSet64 / 1024) / 1024).ToString(CultureInfo.InvariantCulture);

                        string prcName = Process.GetCurrentProcess().ProcessName;
                        var counter_Exec = new PerformanceCounter("Process", "Working Set - Private", prcName);
                        var tskmgr1 = (counter_Exec.RawValue / 1024.0) / 1024;
                        var tskmgr = tskmgr1.ToString();
                        return "Diagnostic " + diagnostic + "  Tskmgr: " + tskmgr + " MB";

                        //ManagementObject wmiService;
                        //wmiService = new ManagementObject("Win32_Service.Name='" + scTemp.ServiceName + "'");
                        //object o = wmiService.GetPropertyValue("ProcessId");

                        //int processId = (int)((UInt32)o);
                        //Process process = Process.GetProcessById(processId);
                        //var cc = process.VirtualMemorySize64.ToString();
                    }
                    else { return string.Empty; }
                }catch(Exception ex) { return string.Empty; }
            }
            set
            {
                memoryMonitorValue = value;
                OnPropertyChanged("MemoryMonitor");
            }
        }

        private Visibility memoryMonitorVisibility = Visibility.Collapsed;

        public Visibility MemoryMonitorvisible
        {
            get
            {
                return memoryMonitorVisibility;
            }
            set
            {
                memoryMonitorVisibility = value;
                OnPropertyChanged("MemoryMonitorvisible");
            }
        }

        public string DutStatus
        {
            get
            {
                if (loopValue == "Duration")
                {
                    if (ExecutionPaused != "Paused" && ExecutionPaused != "Cancelled")
                    {
                        TimeStamp.Visibility = Visibility.Visible;
                        TimeSpan span = RemainingTime - DateTime.Now;
                        if (durationType == "Hour")
                        {
                            if (span.Hours >= 0 && span.Minutes >= 0 && span.Seconds >= 0)
                            {
                                if(span.Days>0)
                                    return ((span.Hours).ToString() + " : " + (span.Minutes).ToString() + " : " + (span.Seconds).ToString() + " + "+(span.Days).ToString()+" day");
                                else
                                    return ((span.Hours).ToString() + " : " + (span.Minutes).ToString() + " : " + (span.Seconds).ToString() + " ");
                            }
                            else
                            { return "Waiting for execution completion"; }
                        }
                        else if (durationType == "Minute")
                        {
                            if (span.Hours > 0 && span.Minutes >= 0 && span.Seconds >= 0)
                            {
                                if (span.Days > 0)
                                    return ((span.Hours).ToString() + " : " + (span.Minutes).ToString() + " : " + (span.Seconds).ToString() + " :+ " + (span.Days).ToString() + " day");
                                else
                                    return ((span.Hours).ToString() + " : " + (span.Minutes).ToString() + " : " + (span.Seconds).ToString() + " ");
                            }
                            else if (span.Minutes >= 0 && span.Seconds >= 0)
                                return ((span.Minutes).ToString() + " : " + (span.Seconds).ToString() + " ");
                            else
                            { return "Waiting for execution completion"; }
                        }
                    }
                    else
                    {
                        if (ExecutionPaused == "Paused")
                            return string.Empty;
                        if (ExecutionPaused == "Cancelled")
                            return string.Empty;
                    }
                }

                return string.Empty;
            }
            set
            {
                dutStatusValue = value;
                OnPropertyChanged("DutStatus");
            }
        }

        void DutStatusTimer(object state)
        {
            OnPropertyChanged("DutStatus");
        }

        public void DiscoveryCompleteEvent()
        {
            try
            {
                if (loadConfigFileInRunner)
                    return;

                if (autoevent == null)
                {

                    this.Dispatcher.Invoke((Action)(() =>
                    {
                    //if (TreeViewExecution.IsEnabled)

                    if (treeViewExplorerExecutionRootItem.IsEnabled)
                            UpdateNetPairingList(true, true);

                    }));
                }
            }
            catch (Exception ex)
            {
                if (ex.Message != "The wait completed due to an abandoned mutex.")
                {
                    DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                    //MessageBox.Show("Exception\n " + ex.Message, "Error Code - EC15001", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void TestAutomationToolWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                QMessageBox = new QatMessageBox(this);

                DeviceDiscovery.startUpWindow.Dispatcher.Invoke((Action)(() =>
                {
                    if (DeviceDiscovery.startUpWindow.IsVisible)
                        DeviceDiscovery.startUpWindow.Close();
                }));

                if (txt_Search.Text == string.Empty)
                {
                    Cancelbutton.Visibility = Visibility.Hidden;
                }
                else
                {
                    Cancelbutton.Visibility = Visibility.Visible;
                }

                treeViewExplorerExecutionRootItem = new TreeViewExplorer(0, QatConstants.TveExecutionInventoryTitle, QatConstants.TveDesignerHeaderItemType, this, null,null, null, null, null, null, null, null, null,0, true);

                treeViewExplorerExecutionRootItem.IsExpanded = true;
                treeViewExplorerExecutionRootItem.IsCheckedVisibility = Visibility.Hidden;
                treeViewExplorerExecutionRootItem.ItemTextBox.FontWeight = FontWeights.Bold;
                treeViewExplorerExecutionRootList.Add(treeViewExplorerExecutionRootItem);
                TreeViewExecution.DataContext = treeViewExplorerExecutionRootList;

                SetupTreeViewrFromDB(true, true, "Date Created Ascending");
                sortingOrders_OriginalList = "Date Created Ascending";

                MenuAscending.IsChecked = Contextascending.IsChecked = true;
                Menuascendingname.IsChecked = Contextascendingname.IsChecked = false;
                Menuascendingcreatedon.IsChecked = Contextascendingcreatedon.IsChecked = true;


                MenuDecending.IsChecked = Contextdescending.IsChecked = false;
                Menudecendingname.IsChecked = Contextdescendingname.IsChecked = false;
                Menudecendingcreatedon.IsChecked = Contextdescendingcreatedon.IsChecked = false;
                                               
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                //MessageBox.Show("Exception\n " + ex.Message, "Error Code - EC15002", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Btn_CreateNewTestSuite_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DeviceDiscovery.CreateDesignerWindow(false);
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                //MessageBox.Show("Exception\n " + ex.Message, "Error Code - EC15003", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Btn_emailReport_Click(object sender, RoutedEventArgs e)
        {
            try
            {               
                Email_Report mail = new Email_Report();
                mail.Owner = this;
                mail.ShowDialog();
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                //MessageBox.Show("Exception\n " + ex.Message, "Error Code - EC15005", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Btn_DelayForExecution_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ExecutionDelay exeDelay = new ExecutionDelay(treeViewExplorerExecutionRootItem);
                exeDelay.ShowDialog();
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                // MessageBox.Show("Exception\n " + ex.Message, "Error Code - EC15006", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void txt_Search_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (txt_Search.Text == string.Empty)
                {
                    //CollapseTree();
                    Cancelbutton.Visibility = Visibility.Hidden;
                    if (sortingOrders_OriginalList != sortingOrders)
                    {

                        Sorting_InventoryList(treeViewExplorerInventoryList, sortingOrders,true);
                        sortingOrders_OriginalList = sortingOrders;
                    }
                    else
                    {
                        TreeViewInventory.DataContext = null;
                        TreeViewInventory.DataContext = treeViewExplorerInventoryList;

                    }
                    isInventorySearchListSelected = false;
                    treeViewExplorerSearchList.Clear();
                }
                else
                {
                    Cancelbutton.Visibility = Visibility.Visible;
                    this.Btn_Search_Click(null, null);
                }
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                //MessageBox.Show("Exception\n " + ex.Message, "Error Code - EC15007", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CollapseTree()
        {
            foreach (TreeViewExplorer testSuite in treeViewExplorerInventoryList)
            {
                testSuite.IsExpanded = false;
            }
        }
      
        private void Chk_ConfigurationFile_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                designerActionCoreName.Clear();
                var designerActionStatus = getDesignerActionContainsTCandCoreName(treeViewExplorerExecutionRootItem);
                if (designerActionStatus.Item1)
                {
                    designerActionCoreName = designerActionStatus.Item2;
                }
                if (UpdateNetPairingList(true, true) > 0)
                {
                    DUT_config = new DUT_Configuration();
                    DUT_config.Owner = this;

                    DUT_DeviceItem[] alphaNumericSortedComponentType = selectedDutDeviceItemList.ToArray();
                    Array.Sort(alphaNumericSortedComponentType, new AlphanumComparatorFastDut());
                    selectedDutDeviceItemList = new List<DUT_DeviceItem>(alphaNumericSortedComponentType.ToList());
                    ObservableCollection<DUT_DeviceItem> dutDeviceItemForDatacontext = new ObservableCollection<DUT_DeviceItem>(selectedDutDeviceItemList); 
                    ObservableCollection<DUT_DeviceItem> dutDeviceItemForDatacontext1 = new ObservableCollection<DUT_DeviceItem>(selectedExternalDeviceItemList);
                    DUT_config.dataGrid_ConfigFile.DataContext = dutDeviceItemForDatacontext;
                    DUT_config.dataGrid_GeneratorConfigFile.DataContext = dutDeviceItemForDatacontext1;

                    if (selectedExternalDeviceItemList.Count > 0)
                    {
                        DUT_config.External_devices_configuration.Visibility = Visibility.Visible;
                        DUT_config.dataGrid_GeneratorConfigFile.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        DUT_config.External_devices_configuration.Visibility = Visibility.Collapsed;
                        DUT_config.dataGrid_GeneratorConfigFile.Visibility = Visibility.Collapsed;
                    }
                    DeviceDiscovery.selectedDutDeviceItemListColor = selectedDutDeviceItemList;                  
                    DUT_config.ShowDialog();
                }
                else
                {
                    MessageBox.Show("No Test Plan is available in the Execution Inventory");
                }
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                //MessageBox.Show("Exception\n " + ex.Message, "Error Code - EC15008", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public Tuple<bool, List<string>> getDesignerActionContainsTCandCoreName(TreeViewExplorer treeViewExplorerExecutionRootItem)
        {
            try
            {
                string query = string.Empty;
                bool designerStatus = false;

                List<string> QATMessage = new List<string>();
                List<Int32> allCaseID = new List<Int32>();
                List<Int32> allplanID = new List<Int32>();
                List<Int32> allsuiteID = new List<Int32>();
                List<Int32> designerCaseID = new List<Int32>();
                List<Int32> InitialdesignerCaseID = new List<Int32>();
                List<string> designerCaseName = new List<string>();
                List<string> designerCasePlanID = new List<string>();
                List<Int32> checkedTestPlanID = new List<Int32>();
                List<string> deviceName = new List<string>();
                List<string> deviceType = new List<string>();
                DBConnection QscDatabase = new DBConnection();

                if (treeViewExplorerExecutionRootItem.Children.Count > 0)
                {
                    foreach (TreeViewExplorer Suite in treeViewExplorerExecutionRootItem.Children)
                    {
                        if ((Suite.IsChecked == true || Suite.IsChecked == null) && (Suite.IsChecked != false))
                        {
                            allsuiteID.Add(Suite.ItemKey);
                            foreach (TreeViewExplorer Plan in Suite.Children)
                            {
                                if ((Plan.IsChecked == true || Plan.IsChecked == null) && (Plan.IsChecked != false))
                                {
                                    allplanID.Add(Plan.ItemKey);
                                    foreach (TreeViewExplorer Case in Plan.Children)
                                    {
                                        if ((Case.IsChecked == true || Case.IsChecked == null) && (Case.IsChecked != false))
                                        {
                                            allCaseID.Add(Case.ItemKey);
                                        }
                                    }
                                }
                            }
                        }
                    }

                    if (allCaseID.Count > 0)
                    {
                        foreach (Int32 currentCaseID in allCaseID)
                        {
                            query = "select ActionType from TestAction where TCID='" + currentCaseID + "'";
                            System.Data.DataTable tble = QscDatabase.SelectDTWithParameter(query, string.Empty, string.Empty);
                            DataTableReader read = tble.CreateDataReader();
                            while (read.Read())
                            {
                                if ((read[0].ToString().Contains("Designer Action")))
                                {
                                    if (!InitialdesignerCaseID.Contains(currentCaseID))
                                        InitialdesignerCaseID.Add(currentCaseID);
                                }
                            }
                        }

                        foreach (Int32 currentDesignerCaseID in InitialdesignerCaseID)
                        {
                            query = "select ConnectDesigner from DesignerAction where TCID='" + currentDesignerCaseID + "'";
                            System.Data.DataTable tble = QscDatabase.SelectDTWithParameter(query, string.Empty, string.Empty);
                            DataTableReader read = tble.CreateDataReader();
                            while (read.Read())
                            {
                                if (read[0] != System.DBNull.Value)
                                {
                                    if ((read[0].ToString() == ("True")))
                                    {
                                        if (!designerCaseID.Contains(currentDesignerCaseID))
                                            designerCaseID.Add(currentDesignerCaseID);
                                    }
                                }
                            }
                        }

                        if (designerCaseID.Count > 0)
                        {
                            foreach (Int32 DACaseID in designerCaseID)
                            {
                                query = "select Testcasename,TPID from Testcase where TestcaseID='" + DACaseID + "'";
                                System.Data.DataTable tble = QscDatabase.SelectDTWithParameter(query, string.Empty, string.Empty);
                                DataTableReader read = tble.CreateDataReader();
                                while (read.Read())
                                {
                                    if (!designerCaseName.Contains(read[0].ToString()))
                                        designerCaseName.Add(read[0].ToString());
                                    if (!designerCasePlanID.Contains(read[1].ToString()))
                                        designerCasePlanID.Add(read[1].ToString());
                                }
                            }
                        }

                        if (designerCaseName.Count > 0)
                        {
                            foreach (string DACaseName in designerCaseName)
                            {
                                foreach (TreeViewExplorer Suite in treeViewExplorerExecutionRootItem.Children)
                                {
                                    if ((Suite.IsChecked == true || Suite.IsChecked == null) && (Suite.IsChecked != false))
                                    {
                                        foreach (TreeViewExplorer Plan in Suite.Children)
                                        {
                                            if ((Plan.IsChecked == true || Plan.IsChecked == null) && (Plan.IsChecked != false))
                                            {
                                                foreach (TreeViewExplorer Case in Plan.Children)
                                                {
                                                    if ((Case.IsChecked == true || Case.IsChecked == null) && (Case.IsChecked != false))
                                                    {
                                                        if (String.Equals(Case.ItemName, DACaseName))
                                                        {
                                                            if (!checkedTestPlanID.Contains(Case.Parent.ItemKey))
                                                                checkedTestPlanID.Add(Case.Parent.ItemKey);
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }


                        if (checkedTestPlanID.Count > 0)
                        {
                            foreach (Int32 PlanID in checkedTestPlanID)
                            {
                                query = "select DeviceType,DeviceModel,DeviceNameInDesign from DesignInventory where DesignID in(select DesignID from TPDesignLinkTable where TPID = '" + PlanID + "')";
                                System.Data.DataTable tble = QscDatabase.SelectDTWithParameter(query, string.Empty, string.Empty);
                                DataTableReader read = tble.CreateDataReader();
                                while (read.Read())
                                {
                                    if (read[0] != System.DBNull.Value)
                                    {
                                        if ((read[0].ToString() == "Core") || (read[0].ToString() == "core"))
                                        {
                                            if (read[2] != System.DBNull.Value)
                                            {
                                                if (!deviceName.Contains(read[2].ToString()))
                                                    deviceName.Add(read[2].ToString());
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }


                        //string query12 = "select distinct devicesname from TelnetAction where TCID in ('" + string.Join("','", allCaseID) + "')";
                        string Vudeogenfetch = "select distinct x.devicesname from (select devicesname from  TelnetAction where TCID in ('" + string.Join("','", allCaseID) + "') union select devicesname from  TelenetMonitor where TPID in ('" + string.Join("','", allplanID) + "') union select devicesname from  TelenetMonitor where TSID in ('" + string.Join("','", allsuiteID) + "')) x where x.devicesname like '%Video Gen%'";
                        //string query13 = "select distinct devicesname from TelnetAction where TCID in ('" + string.Join("','", allCaseID) + "')";
                        //string query14 = "select distinct devicesname from TelnetAction where TCID in ('" + string.Join("','", allCaseID) + "')";
                        System.Data.DataTable Vudeogentable = QscDatabase.SelectDTWithParameter(Vudeogenfetch, string.Empty, string.Empty);
                        DataTableReader readvalues = Vudeogentable.CreateDataReader();
                        while (readvalues.Read())
                        {
                            string[] Getname_model = new string[] { };

                            if (readvalues[0] != null && readvalues[0].ToString().Contains("Video Gen"))
                            {
                                string[] Genname_model = Regex.Split(readvalues[0].ToString(), ",");

                                foreach (string value in Genname_model)
                                {
                                    if (value.Contains("Video Gen"))
                                    {
                                    Getname_model = Regex.Split(value.ToString(), "-");

                                        if (Getname_model.Count() > 1 && Getname_model[0] != null && Getname_model[1] != null)
                                        {
                                            Dictionary<string, DUT_DeviceItem> duplicateExternalDUT = new Dictionary<string, DUT_DeviceItem>();
                                            foreach (DUT_DeviceItem deviceItem in selectedExternalDeviceItemList)
                                            {
                                            duplicateExternalDUT.Add(deviceItem.VideoGen + deviceItem.GenModel, deviceItem);
                                            }

                                            if (!duplicateExternalDUT.Keys.Contains(Getname_model[0] + Getname_model[1]))
                                            {
                                                DUT_DeviceItem GenDUTitem = new DUT_DeviceItem();
                                            GenDUTitem.VideoGen = Getname_model[0];
                                            GenDUTitem.GenModel = Getname_model[1];
                                            selectedExternalDeviceItemList.Add(GenDUTitem);
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        if (Vudeogentable.Rows.Count < 1)
                        selectedExternalDeviceItemList.Clear();
                    
                }

                if (deviceName.Count > 0)
                {
                    designerStatus = true;
                    QATMessage = deviceName;
                }

                return new Tuple<bool, List<string>>(designerStatus, QATMessage);
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                ExecutionMessageBox("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                return new Tuple<bool, List<string>>(false, null);
            }
        }

        public bool designerActionPresent(string selectedCorename)
        {
            bool presentStatus = false;
            try
            {
                if (designerActionCoreName.Any(s => s.Equals(selectedCorename, StringComparison.OrdinalIgnoreCase)))
                {
                    presentStatus = true;
                }

                return presentStatus;
            }
            catch (Exception ex)
            {
                return presentStatus;
            }


        }

        private void btn_ExecutionWindow_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DeviceDiscovery.CreateRunnerWindow(false);
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                // MessageBox.Show("Exception\n " + ex.Message, "Error Code - EC15010", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExecutionWindow()
        {
            try
            {
                Test_Execution automation_Tool = new Test_Execution();
                automation_Tool.Show();
                System.Windows.Threading.Dispatcher.Run();
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                //
            }
        }

        private void btn_Execute_Click(object sender, RoutedEventArgs e)
        {
            if (!InProcess)
            {
                ServerDetails check = new ServerDetails();
                if (File.Exists(check.txtDesignVersion.Text))
                {
                    bool Export_Running = DeviceDiscovery.IsExportRunning();
                    bool Import_Running = DeviceDiscovery.IsImportRunning();

                    if (!Export_Running)
                    {
                        MessageBox.Show("Please wait for Export process to finish & start execution ", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                    if (!Import_Running)
                    {
                        MessageBox.Show("Please wait for Import process to finish & start execution", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    //    return;

                    StartExecution();
                }

                else
                {
                    MessageBox.Show("The Q-SYS designer does not exist in the designer path", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }
        }

        private void StartExecution()
        {
            if (treeViewExplorerExecutionRootItem.IsEnabled == false && isExecutionPaused == false)
                return;

            //if (TreeViewExecution.IsEnabled == false && isExecutionPaused == false)
            //    return;
            
            try
                {
                txtLoopExecutionCount.Text = retainvalue;
             
                if (isExecutionPaused)
                {
                    if (ExecutionPaused == "Paused")
                    {                                                 
                        TimeSpan s = new TimeSpan(CurrentRemainingTime.Days,CurrentRemainingTime.Hours, CurrentRemainingTime.Minutes, CurrentRemainingTime.Seconds);
                        DeviceDiscovery.InputTime = DateTime.Now.Add(s);                                         
                        RemainingTime = DeviceDiscovery.InputTime;
                        ExecutionPaused = "";
                    }
					
                    isExecutionPaused = false;
                    if (autoevent != null)
                        autoevent.Set();

                    ExecutionCompleteEnable("resume");
                    /* old logic comment
                    btn_Execute.IsEnabled = false;
                    Btn_Pause.IsEnabled = true;
                    btn_Cancel.IsEnabled = true;
                    */

                    ExecutionPaused = "";
                }
                else
                {

                    UpdateNetPairingList(true, true);
                    ExecutionCompleteEnable("start");
                    isExecutionPaused = false;
                    // old logic comment
                    /*
                    btn_Execute.IsEnabled = false;
                    Btn_Pause.IsEnabled = true;
                    btn_Cancel.IsEnabled = true;
                    */

                    ExecutionPaused = "";




                    //IsEnabled = false;
                    //TreeViewExecution.IsEnabled = false;

                    // old logic comment
                    /*	
                     	treeViewExplorerExecutionRootItem.IsEnabled = false;
                        chk_ConfigurationFile.IsEnabled = false;
                        btn_Execute.IsEnabled = false;
                        Btn_Execution_Delay.IsEnabled = false;
                        btn_loop.IsEnabled = false;
                        btn_emailReport.IsEnabled = false;
                        
                     */


                    autoevent = new AutoResetEvent(true);
                    executionThread = new Thread(() => executionProcess.StartExecutionThread(treeViewExplorerExecutionRootItem, selectedDutDeviceItemList, ExecutionComplete, ExecutionMessageBox, UpdateExecutionStatus, autoevent, ExecutionLoopCount, loopValue1, RemainingTime1, durationType1, this, selectedExternalDeviceItemList));
                    executionThread.Start();
                }
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                //MessageBox.Show("Exception\n \n" + ex.Message, "QAT Error Code - ECxxxx", MessageBoxButton.OK, MessageBoxImage.Error);
                if (!btn_Execute.IsEnabled)
                    btn_Execute.IsEnabled = true;
            }

            finally
            {
                //if (!btn_Execute.IsEnabled)
                //    btn_Execute.IsEnabled = true;
            }
        }

        private void Btn_Pause_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!InProcess)
                {
                    InProcess = true;
                    autoevent_Reset_Delay = true;
                    ExecutionCompleteEnable("during pausing");
                    Task.Factory.StartNew(() => { PauseTask(); });
                }
                else
                {
                    e.Handled = true;
                }
                
            }
            catch (Exception ex)
            {
                autoevent_Reset_Delay = false;
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                //MessageBox.Show("Exception\n \n" + ex.Message, "QAT Error Code - ECxxxx", MessageBoxButton.OK, MessageBoxImage.Error);

            }

        }

        private void PauseTask()
        {
            try
            {
                this.Dispatcher.Invoke(() =>
                {
                    StartWaitForUpdate();
                });
                ExecutionPaused = "Paused";
                CurrentRemainingTime = RemainingTime - DateTime.Now;
                if (autoevent != null)
                    autoevent.WaitOne();
                if (executionThread != null && autoevent != null)
                {
                    isExecutionPaused = true;
                    this.Dispatcher.Invoke(() =>
                    {
                        txtLoopExecutionCount.Text = "Execution Paused";
                    });
                }
                this.Dispatcher.Invoke(() =>
                {
                    EndWaitForUpdate();
                });
                InProcess = false;
                autoevent_Reset_Delay = false;
            }
            catch(Exception ex)
            {
				autoevent_Reset_Delay = false;
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
            }
        }

        private void btn_Cancel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!InProcess)
                {
                    if (autoevent != null && !autoevent.WaitOne(1))
                    {
                        autoevent.Set();
                    }
                    Task.Factory.StartNew(() => { CancelTask(); });
                }
                else
                {
                    e.Handled = true;
                }
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                //MessageBox.Show("Exception\n \n" + ex.Message, "QAT Error Code - ECxxxx", MessageBoxButton.OK, MessageBoxImage.Error);

            }
        }

        private void CancelTask()
        {
            try
            {
                ExecutionPaused = "Cancelled";
                isExecutionPaused = false;
                this.Dispatcher.Invoke(() =>
                {
                    StartWaitForUpdate();
                    ExecutionCompleteEnable("during cancel");
                });
                
                
                MsgAuto = false;
                if (executionThread != null)
                    executionThread.Abort();
                executionThread = null;
                this.Dispatcher.Invoke(() =>
                {
                    EndWaitForUpdate();
                });

            }
            catch(Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
            }
        }

        private void ExecutionComplete(string state)
        {
            try
            {
                if (autoevent != null)
                    autoevent.Set();
                if (autoevent != null)
                    autoevent.WaitOne();
                this.Dispatcher.Invoke((Action)(() =>
                {
                    ExecutionCompleteEnable(state);
                }));
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                //MessageBox.Show("Exception\n \n" + ex.Message, "QAT Error Code - ECxxxx", MessageBoxButton.OK, MessageBoxImage.Error);

            }

        }

        private void ExecutionCompleteEnable(string state)
        {
            try
            {
                if(state=="completed")
                {
                    treeViewExplorerExecutionRootItem.IsEnabled = true;
                    btn_Execute.IsEnabled = true;
                    Btn_Pause.IsEnabled = false;
                    btn_Cancel.IsEnabled = false;
                    Btn_Execution_Delay.IsEnabled = true;
                    chk_ConfigurationFile.IsEnabled = true;
                    btn_loop.IsEnabled = true;
                    btn_emailReport.IsEnabled = true;
                    txtLoopExecutionCount.Text = "";
                    executionThread = null;
                    if (autoevent != null)
                        autoevent.Dispose();
                    autoevent = null;
                }
                if(state=="start" || state=="resume")
                {
                    treeViewExplorerExecutionRootItem.IsEnabled = false;
                    btn_Execute.IsEnabled = false;
                    Btn_Pause.IsEnabled = true;
                    btn_Cancel.IsEnabled = true;
                    Btn_Execution_Delay.IsEnabled = false;
                    chk_ConfigurationFile.IsEnabled = false;
                    btn_loop.IsEnabled = false;
                    btn_emailReport.IsEnabled = false;
                }
                if(state=="during pausing")
                {
                    treeViewExplorerExecutionRootItem.IsEnabled = false;
                    btn_Execute.IsEnabled = true;
                    Btn_Pause.IsEnabled = false;
                    btn_Cancel.IsEnabled = true;
                    Btn_Execution_Delay.IsEnabled = false;
                    chk_ConfigurationFile.IsEnabled = false;
                    btn_loop.IsEnabled = false;
                    btn_emailReport.IsEnabled = false;
                    txtLoopExecutionCount.Text = "Execution is being paused , please wait...";
                   
                }
                if (state == "during cancel")
                {
                    treeViewExplorerExecutionRootItem.IsEnabled = false;
                    btn_Execute.IsEnabled = false;
                    Btn_Pause.IsEnabled = false;
                    btn_Cancel.IsEnabled = false;
                    Btn_Execution_Delay.IsEnabled = false;
                    chk_ConfigurationFile.IsEnabled = false;
                    btn_loop.IsEnabled = false;
                    btn_emailReport.IsEnabled = false;
                    txtLoopExecutionCount.Text = "Execution Cancelled";
                    if (autoevent != null)
                        autoevent.Dispose();
                    autoevent = null;
                }
                if (state=="cancel finished")
                {
                    treeViewExplorerExecutionRootItem.IsEnabled = true;
                    btn_Execute.IsEnabled = true;
                    Btn_Pause.IsEnabled = false;
                    btn_Cancel.IsEnabled = false;
                    Btn_Execution_Delay.IsEnabled = true;
                    chk_ConfigurationFile.IsEnabled = true;
                    btn_loop.IsEnabled = true;
                    btn_emailReport.IsEnabled = true;
                    txtLoopExecutionCount.Text = "Execution Cancelled";
                    if (autoevent != null)
                        autoevent.Dispose();
                    autoevent = null;
                }
            }           
             catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                //MessageBox.Show("Exception\n \n" + ex.Message, "QAT Error Code - ECxxxx", MessageBoxButton.OK, MessageBoxImage.Error);

            }
        }

        private Tuple <MessageBoxResult, string> ExecutionMessageBox(string messageBoxTest, string messageBoxCaption, MessageBoxButton messageBoxButton, MessageBoxImage messageBoxImage)
        {          
            MessageBoxResult result = MessageBoxResult.None;
            string verifyUserRemarksText = string.Empty;           
            try
            {
                if (autoevent != null)
                    autoevent.Set();
                if (autoevent != null)
                    autoevent.WaitOne();
                this.Dispatcher.Invoke((Action)(() =>
                {
                    if ((messageBoxCaption == "Test Suite Execution Summary")||(messageBoxCaption == "DUT Configuration Warning") ||(messageBoxCaption == "User action Alert") || (messageBoxCaption == "User verification Alert"))
                    {
                  
                            QMessageBox = new QatMessageBox(this, MsgAuto);
							
                        result = QMessageBox.Show(messageBoxTest, messageBoxCaption, messageBoxButton, messageBoxImage);
                        verifyUserRemarksText = QMessageBox.UserVerifyremarksText;
                        if (btn_Execute.IsEnabled == false && messageBoxCaption == "Test Suite Execution Summary")
                            btn_Execute.IsEnabled = true;
                    }
                    else
                    {
                        if (MsgAuto)
                        {
                            if (messageBoxButton == MessageBoxButton.OK || messageBoxButton == MessageBoxButton.OKCancel)
                            {
                                result= MessageBoxResult.OK;
                            }
                            if (messageBoxButton == MessageBoxButton.YesNo || messageBoxButton == MessageBoxButton.YesNoCancel)
                            {
                                result= MessageBoxResult.Yes;
                            }
                        }
                        else
                        {
                            result = MessageBox.Show(this, messageBoxTest, messageBoxCaption, messageBoxButton, messageBoxImage);
                        }
                    }

                }));

                if (autoevent != null)
                    autoevent.Set();
                if (autoevent != null)
                    autoevent.WaitOne();

                return new Tuple<MessageBoxResult, string>(result, verifyUserRemarksText);
            }

            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                //MessageBox.Show("Exception\n \n" + ex.Message, "QAT Error Code - ECxxxx", MessageBoxButton.OK, MessageBoxImage.Error);
                return new Tuple<MessageBoxResult, string>(result, verifyUserRemarksText);
            }
        }

        private bool UpdateExecutionStatus(TreeViewExplorer PlanExecution, string StatusInfo)
        {
            bool result = false;
            try
            {

                if (autoevent != null)
                    autoevent.Set();
                if (autoevent != null)
                    autoevent.WaitOne();
                this.Dispatcher.Invoke((Action)(() =>
                {
                    PlanExecution.UpdatestatusInfo = StatusInfo;
                    result = true;
                }));

                return result;
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                //MessageBox.Show("Exception\n \n" + ex.Message, "QAT Error Code - ECxxxx", MessageBoxButton.OK, MessageBoxImage.Error);
                return result;
            }
            
        }

        private string retainvalue = string.Empty;
        private string ExecutionLoopCount(string LoopValue)
        {
            string str = string.Empty;        
            try
            {
                if (autoevent != null)
                    autoevent.Set();
                if (autoevent != null)
                    autoevent.WaitOne();

                this.Dispatcher.Invoke((Action)(() =>
                {
                    str = LoopValue;
                    txtLoopExecutionCount.Text = str;
                    retainvalue = str;
                }));

                //this.Dispatcher.Invoke((Action)(() =>
                //{
                //    if (LoopValue.Contains("Remaining time for Script Execution : "))
                //    {
                //        str = LoopValue.Substring(0, LoopValue.IndexOf("Remaining time for Script Execution : "));
                //        txtLoopExecutionCount.Text = str;
                //        scriptupdate.Height = 23;
                //        scriptupdate.Text = LoopValue.Substring(LoopValue.IndexOf("Remaining time for Script Execution : "), (LoopValue.Length - LoopValue.IndexOf("Remaining time for Script Execution : ")));
                //        retainvalue = str;
                //    }

                //    else
                //    {
                //        scriptupdate.Height = 0;
                //        scriptupdate.Text = "";
                //        str = LoopValue;
                //        txtLoopExecutionCount.Text = str;
                //        retainvalue = str;
                //    }
                //}));

                if (autoevent != null)
                    autoevent.Set();
                if (autoevent != null)
                    autoevent.WaitOne();

                return str;
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                //MessageBox.Show("Exception\n \n" + ex.Message, "QAT Error Code - ECxxxx", MessageBoxButton.OK, MessageBoxImage.Error);
                return str;
            }

        }

        private void TestAutomationToolWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                if (MsgAuto)
                {
                    MsgAuto = false;
                    StartWaitForUpdate();

                    if (executionThread != null)
                        executionThread.Abort();

                    ExecutionCompleteEnable("cancel finished");

                    DeviceDiscovery.RemoveEvent(DiscoveryCompleteEvent);

                    EndWaitForUpdate();

                    DeviceDiscovery.WriteToLogFile("QAT Ended................");
                }
                else
                {
                    MessageBoxResult result = MessageBox.Show(this, "Are you sure you want to close?", "QAT-Runner Closing", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (result == MessageBoxResult.No)
                    {
                        e.Cancel = true;
                    }
                    else if (result == MessageBoxResult.Yes)
                    {
                        StartWaitForUpdate();

                        if (executionThread != null)
                            executionThread.Abort();

                        ExecutionCompleteEnable("cancel finished");

                        DeviceDiscovery.RemoveEvent(DiscoveryCompleteEvent);

                        EndWaitForUpdate();

                        DeviceDiscovery.WriteToLogFile("QAT Ended................");
                    }
                }
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                //MessageBox.Show("Exception\n \n" + ex.Message, "QAT Error Code - ECxxxx", MessageBoxButton.OK, MessageBoxImage.Error);
                DeviceDiscovery.WriteToLogFile("QAT Ended................");
            }
        }

        private void TestAutomationToolWindow_ContentRendered(object sender, EventArgs e)
        {
            //Checking whether server path exists and permission status
            try
            {
                this.Title= this.Title + " ----->Connected Server :" + QatConstants.SelectedServer; 

                StartWaitForUpdate();
                DeviceDiscovery.AddEvent(DiscoveryCompleteEvent);
                EndWaitForUpdate();

                if (!Directory.Exists(QatConstants.QATServerPath))
                {
                    MessageBox.Show("Invalid Server path", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                else
                {
                    ServerDetails folderreadcheck = new ServerDetails();
                    bool hasreadAccess = folderreadcheck.hasWriteAccessToFolder(QatConstants.QATServerPath);
                    folderreadcheck.Close();
                    if (!hasreadAccess)
                    {
                        MessageBox.Show("The server path entered is read only", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }

                if (loadConfigFileInRunner)
                {
                    if (File.Exists(DeviceDiscovery.ConfigFileName))
                    {
                        StartWaitForUpdate();
                        while (!DeviceDiscovery.IsDeviceDiscoveryComplete) ;
                        EndWaitForUpdate();

                        ImportConfigFile(DeviceDiscovery.ConfigFileName);
                        loadConfigFileInRunner = false;
                        //if (!DeviceDiscovery.CheckRunningThreadStatus())
                        //    return;
                        // DeviceDiscovery.AddRunningThreadStatus(executionThread.ManagedThreadId, executionThread.ThreadState, "Execution");
                        bool Export_Running = DeviceDiscovery.IsExportRunning();
                        bool Import_Running = DeviceDiscovery.IsImportRunning();

                        if (!Export_Running)
                        {
                            MessageBox.Show("Please wait for Export process to finish & start execution ", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }
                        if (!Import_Running)
                        {
                            MessageBox.Show("Please wait for Import process to finish & start execution", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }
                        this.MsgAuto = true;

                        StartExecution();
                    }
                    else
                    {
                        MessageBox.Show("Config File  " + DeviceDiscovery.ConfigFileName + " does not exists.", "QAT Error Message", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                //string getmessagestring = "SERVER NAME---->" + QatConstants.SelectedServer + Environment.NewLine + "SERVER path---->" + QatConstants.QATServerPath + Environment.NewLine + "SERVER designer db---->" + QatConstants.DbDatabaseName+ Environment.NewLine + "SERVER report db---->" + QatConstants.Report_DbDatabaseName + Environment.NewLine+ "SERVER release path---->" + QatConstants.ReleaseFolderPAth + Environment.NewLine + " Report link NAME---->" + QatConstants.Reportpath;
                //MessageBox.Show(getmessagestring, "Check server", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                //MessageBox.Show("Exception\n " + ex.Message, "Error Code - EC15033", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void TreeViewInventory_Drop(object sender, DragEventArgs e)
        {
            try
            {

                if (_dragdropWindow != null)
                {
                    _dragdropWindow.Close();
                    _dragdropWindow = null;
                }

                if (e.Data == null)
                    return;

                DragDropItem dragData = (DragDropItem)e.Data.GetData(typeof(DragDropItem));
                if (dragData == null)
                    return;

                if (dragData.DragSourceType == "ExecutionTreeView" && dragData.DragSourceHashCode == this.GetHashCode())
                {
                foreach (TreeViewExplorer tree in selectedItemsExecution.Values)
                {
                    if (treeViewExplorerExecutionRootItem.Children.Contains(tree))
                        treeViewExplorerExecutionRootItem.RemoveChildren(tree);
                    }

                if (treeViewExplorerExecutionRootItem.Children.Count == 0)
                {
                    this.chk_ConfigurationFile.IsEnabled = false;
                    this.btn_loop.IsEnabled = false;
                        this.btn_emailReport.IsEnabled = false;
                        btn_Execute.IsEnabled = false;
                        Btn_Execution_Delay.IsEnabled = false;
                        treeViewExplorerExecutionRootItem.IsCheckedVisibility = Visibility.Hidden;
                }
                else
                    treeViewExplorerExecutionRootItem.IsCheckedVisibility = Visibility.Visible;

                TreeViewExecution.DataContext = null;
                TreeViewExecution.DataContext = treeViewExplorerExecutionRootList;
            
                selectedItemsExecution.Clear();
            }
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                //MessageBox.Show("Exception\n \n" + ex.Message, "QAT Error Code - ECxxxx", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }        
     
        private void btn_report_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //System.Diagnostics.Process.Start("http://localhost/REP/");
                System.Diagnostics.Process.Start(QatConstants.Reportpath);
                // DeviceDiscovery.CreateReportWindow();
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                //MessageBox.Show(ex.ToString());
                //  System.Diagnostics.Process.Start("http://1uscmsqadbd01/SQA_QAT_Server/");
            }
        }

        private void SearchMouceClick(object sender, MouseEventArgs e)
        {
            try
            {
                this.txt_Search.Focus();
                this.txt_Search.IsTabStop = true;
                this.txt_Search.UpdateLayout();
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                //MessageBox.Show("Exception\n \n" + ex.Message, "QAT Error Code - ECxxxx", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ServerMenuItem_Click(object sender, RoutedEventArgs e)
        {
            DeviceDiscovery.CreatePreferenceWindow();

            //OpenPreferences();
        }

        private void OpenPreferences()
        {
            try
            {
                ServerDetails serverInvoke = new ServerDetails();
                serverInvoke.Owner = this;
                serverInvoke.ShowDialog();
                serverInvoke.Close();
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                // MessageBox.Show("Exception\n " + ex.Message, "Error Code - EC15034", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public int UpdateNetPairingList(bool isTestPlanUpdateEnabled, bool isNetPairingUpdateEnabled)
        {
            try
            {

                List<TreeViewExplorer> testPlansInExecution = new List<TreeViewExplorer>();

                testPlansInExecution.Clear();
                if (treeViewExplorerExecutionRootItem == null)
                    return 0;

                foreach (TreeViewExplorer SuiteExecution in treeViewExplorerExecutionRootItem.Children)
                {
                    foreach (TreeViewExplorer PlanExecution in SuiteExecution.Children)
                    {
                        testPlansInExecution.Add(PlanExecution);
                    }
                }

                if (testPlansInExecution.Count > 0)
                {
                    if (isTestPlanUpdateEnabled)
                    {
                        DBConnection QscDatabase = new DBConnection();
                        List<DUT_DeviceItem> updatedDutDeviceItemList = new List<DUT_DeviceItem>();
                        Dictionary<String, DUT_DeviceItem> existingDutDeviceItemList = new Dictionary<string, DUT_DeviceItem>();
                        Dictionary<String, DUT_DeviceItem> selectedDutDeviceItemDictionary = new Dictionary<String, DUT_DeviceItem>(StringComparer.OrdinalIgnoreCase);

                        foreach (DUT_DeviceItem item in selectedDutDeviceItemList)
                        {
                         //   if(!item.ItemDeviceType.Contains("Camera"))
                            existingDutDeviceItemList.Add(item.ItemDeviceType + item.ItemDeviceName + item.ItemDeviceModel, item);
                        }

                        updatedDutDeviceItemList = QscDatabase.GetDesignDetails(testPlansInExecution);

                        selectedDutDeviceItemList.Clear();
                        DeviceDiscovery.DoubleCoreList.Clear();
                        foreach (DUT_DeviceItem item in updatedDutDeviceItemList)
                        {
                            string deviceModel = string.Empty;
                            if ((item.ItemDeviceModel.StartsWith("PS")))
                            {
                                deviceModel = item.ItemDeviceModel.Remove(item.ItemDeviceModel.Length - 1);
                            }
                            else if ((item.ItemDeviceModel.StartsWith("TSC-7")))
                            {
                                deviceModel = item.ItemDeviceModel;
                                int index = deviceModel.IndexOf("7");
                                if (index > 0)
                                    deviceModel = deviceModel.Substring(0, index + 1);

                            }
                            else
                            {
                                deviceModel = item.ItemDeviceModel;
                            }

                            if ((DeviceDiscovery.Netpair_devicesSupported.Contains(deviceModel) || deviceModel.Contains("Core")))
                            {
                                if (!DeviceDiscovery.DoubleCoreList.Keys.Contains(item.ItemDeviceType + item.ItemDeviceName + item.ItemDeviceModel + item.ItemPrimaryorBackup + item.Itemlinked) && item.Itemlinked!=string.Empty)
                                {
                                    DeviceDiscovery.DoubleCoreList.Add(item.ItemDeviceType + item.ItemDeviceName + item.ItemDeviceModel + item.ItemPrimaryorBackup + item.Itemlinked, item);
                                }
                               
                                if (!selectedDutDeviceItemDictionary.Keys.Contains(item.ItemDeviceType + item.ItemDeviceName + item.ItemDeviceModel))
                                {
                                    selectedDutDeviceItemDictionary.Add(item.ItemDeviceType + item.ItemDeviceName + item.ItemDeviceModel, item);
                                    if (existingDutDeviceItemList.Keys.Contains(item.ItemDeviceType + item.ItemDeviceName + item.ItemDeviceModel))
                                    {
                                        selectedDutDeviceItemList.Add(existingDutDeviceItemList[item.ItemDeviceType + item.ItemDeviceName + item.ItemDeviceModel]);
                                        existingDutDeviceItemList[item.ItemDeviceType + item.ItemDeviceName + item.ItemDeviceModel].TestSuiteNameList = item.ParentTestPlanTreeView.Parent.ItemName;
                                        existingDutDeviceItemList[item.ItemDeviceType + item.ItemDeviceName + item.ItemDeviceModel].TestSuiteTreeViewList.Clear();
                                        existingDutDeviceItemList[item.ItemDeviceType + item.ItemDeviceName + item.ItemDeviceModel].TestSuiteTreeViewList.Add(item.ParentTestPlanTreeView.Parent);
                                    }
                                    else
                                    {
                                        selectedDutDeviceItemList.Add(item);
                                    }
                                }

                                else
                                {
                                    DUT_DeviceItem dutItem = selectedDutDeviceItemList.Find(x => x.ItemDeviceName.Equals(item.ItemDeviceName, StringComparison.CurrentCultureIgnoreCase) && x.ItemDeviceType.Equals(item.ItemDeviceType, StringComparison.CurrentCultureIgnoreCase) && x.ItemDeviceModel.Equals(item.ItemDeviceModel, StringComparison.CurrentCultureIgnoreCase));
                                    if (dutItem != null && dutItem.TestSuiteTreeViewList.Find(x => x.ItemName.Equals(item.ParentTestPlanTreeView.Parent.ItemName, StringComparison.CurrentCultureIgnoreCase)) == null)
                                    {
                                        dutItem.TestSuiteTreeViewList.Add(item.ParentTestPlanTreeView.Parent);
                                        dutItem.TestSuiteNameList = dutItem.TestSuiteNameList + ", " + item.ParentTestPlanTreeView.Parent.ItemName;
                                    }
                                }
                            }
                        }
                    }

                    if (isNetPairingUpdateEnabled)
                        DeviceDiscovery.UpdateNetPairingList(selectedDutDeviceItemList);

                }
                else
                {
                    selectedDutDeviceItemList.Clear();
                }

                bool discoveryCompletedForSelectedDevices = false;
                if(selectedDutDeviceItemList.Count > 0)
                {
                    discoveryCompletedForSelectedDevices = true;
                    foreach (DUT_DeviceItem item in selectedDutDeviceItemList)
                    {
                        if (item.ItemNetPairingSelected == null)
                            discoveryCompletedForSelectedDevices = false;
                        if((item.ItemDeviceType=="Core")|| (item.ItemDeviceType == "core"))
                        {
                            bool designerPresent = false;
                            designerPresent = designerActionPresent(item.ItemDeviceName);
                            if(designerPresent)
                            {
                                item.DesignerActionPresent = true;
                            }
                            else
                            {
                                item.DesignerActionPresent = false;
                            }
                        }
                    }
                }
                

                if (DeviceDiscovery.AvailableDeviceList.Count == 0)
                {
                    chk_ConfigurationFile.Foreground = Brushes.Red;
                  //  chk_ConfigurationFile.ToolTip = "No devices found";
                }
                else if (DeviceDiscovery.IsDeviceDiscoveryComplete)
                {
                    chk_ConfigurationFile.Foreground = Brushes.LightGreen;
                   // chk_ConfigurationFile.ClearValue(Button.ToolTipProperty);
                   // chk_ConfigurationFile.ToolTip = "All devices found";
                }
                else if (discoveryCompletedForSelectedDevices)
                {
                    chk_ConfigurationFile.Foreground = Brushes.Orange;
                   // chk_ConfigurationFile.ToolTip = "Devices for current execution found";
                }
                else if (DeviceDiscovery.AvailableDeviceList.Count > 0)
                {
                    chk_ConfigurationFile.Foreground = Brushes.Yellow;
                   // chk_ConfigurationFile.ToolTip = "some devices found";
                }

                return testPlansInExecution.Count;
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                //MessageBox.Show("Exception\n " + ex.Message, "QAT Error Code - EC15035", MessageBoxButton.OK, MessageBoxImage.Error);
                return 0;
            }
        }

        private void StartWaitForUpdate()
        {
            try
            {
                this.Cursor = Cursors.Wait;
                if (Mouse.OverrideCursor != Cursor)
                    Mouse.OverrideCursor = Cursor;
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                // MessageBox.Show("Exception\n " + ex.Message, "QAT Error Code - EC15036", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }

        private void EndWaitForUpdate()
        {
            try
            {
                this.Cursor = Cursors.Arrow;
                // The check is required to prevent cursor flickering
                if (Mouse.OverrideCursor != Cursor)
                    Mouse.OverrideCursor = null;
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                //MessageBox.Show("Exception\n " + ex.Message, "QAT Error Code - EC15037", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }

        private void btn_ExecutionLoop_Click(object sender, RoutedEventArgs e)
        {
            try
            {                
                ExecutionLoop EL = new ExecutionLoop(treeViewExplorerExecutionRootItem);
                EL.Owner = this;
                EL.ShowDialog();
                EL.DataContext = treeViewExplorerExecutionRootItem.Children;
                EL.Activate();
                if (EL.WindowState == WindowState.Minimized)
                {
                    EL.WindowState = WindowState.Normal;
                }                
            }
            catch (Exception ex)
            {

                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                //MessageBox.Show("Exception\n " + ex.Message, "QAT Error Code - ECxxxx", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }

        private void TreeViewExecution_MouseMove(object sender, MouseEventArgs e)
        {
            try
            {
                FrameworkElement sourceElement = e.OriginalSource as FrameworkElement;
                if (sourceElement == null)
                    return;

                TreeViewExplorer originalTreeViewExplorer = sourceElement.DataContext as TreeViewExplorer;
                if (originalTreeViewExplorer == null || originalTreeViewExplorer.ItemType == QatConstants.TveDesignerHeaderItemType)
                    return;

                if (treeViewExplorerExecutionRootItem != null && treeViewExplorerExecutionRootItem.Children.Count > 0 && originalTreeViewExplorer.ItemType != QatConstants.TveDesignerHeaderItemType)
                {
                    if (originalTreeViewExplorer.ItemType != QatConstants.DbTestSuiteTable)
                    {
                        originalTreeViewExplorer = originalTreeViewExplorer.Parent;
                        if (originalTreeViewExplorer.ItemType != QatConstants.DbTestSuiteTable)
                        {
                            originalTreeViewExplorer = originalTreeViewExplorer.Parent;
                        }
                    }
                }


                if (e.LeftButton == MouseButtonState.Pressed && e.OriginalSource.GetType().ToString() != "System.Windows.Controls.Primitives.Thumb")
                {
                    DataObject dataSourceObject = new DataObject();
                    dataSourceObject.SetData(this.GetHashCode().ToString(), this);
                    if (this.selectedItemsExecution != null && selectedItemsExecution.Values.Contains(originalTreeViewExplorer))
                    {
                        DragDropItem dragData = new DragDropItem();
                        dragData.DragSourceType = "ExecutionTreeView";
                        dragData.DragSourceHashCode = this.GetHashCode();
                        dragData.InventorySelectedItemsDictionary = selectedItemsExecution;
                        if (selectedItemsExecution.Count > 0)
                        {
                            CreateDragDropWindow(selectedItemsExecution);
                            DragDrop.DoDragDrop(sender as DependencyObject, new DataObject(typeof(DragDropItem), dragData), DragDropEffects.Copy);
                            if (_dragdropWindow != null)
                            {
                                _dragdropWindow.Close();
                                _dragdropWindow = null;
                            }
                        }
                    }
                    else
                    {
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                // MessageBox.Show("Exception\n " + ex.Message, "Error Code - EC16006", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void TreeViewInventory_PreviewMouseDownUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                FrameworkElement sourceElement = e.OriginalSource as FrameworkElement;
                if (sourceElement == null)
                    return;

                TreeViewExplorer originalTreeViewExplorer = sourceElement.DataContext as TreeViewExplorer;

                if (originalTreeViewExplorer == null && sourceElement.ToString() != "System.Windows.Controls.Grid")
                {
                    return;
                }
				
                if (originalTreeViewExplorer != null)
                    originalTreeViewExplorer.IsSelected = true;
                else if (originalTreeViewExplorer == null)
                {
                    TreeViewExplorer selectedTreeViewExplorer = TreeViewInventory.SelectedItem as TreeViewExplorer;
                    if (selectedTreeViewExplorer != null)
                        selectedTreeViewExplorer.IsSelected = false;
                }

                if (isInventorySearchListSelected == false)
                {
                    TreeView_MultiSelect(originalTreeViewExplorer, selectedItemsInventory, treeViewExplorerInventoryList, null, false, e.ChangedButton);
                }
                else
                {
                    TreeView_MultiSelect(originalTreeViewExplorer, selectedItemsInventory, treeViewExplorerSearchList, null, false, e.ChangedButton);
                }                   
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                // MessageBox.Show("Exception\n " + ex.Message, "Error Code - EC16001", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void TreeViewExecution_PreviewMouseDownUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                FrameworkElement sourceElement = e.OriginalSource as FrameworkElement;
                if (sourceElement == null)
                    return;

                TreeViewExplorer originalTreeViewExplorer = sourceElement.DataContext as TreeViewExplorer;

                if (originalTreeViewExplorer == null && sourceElement.ToString() != "System.Windows.Controls.Grid")
                {
                    return;
                }
                
                if (originalTreeViewExplorer == null)
                {
                    TreeViewExplorer selectedTreeViewExplorer = TreeViewExecution.SelectedItem as TreeViewExplorer;
                    if (selectedTreeViewExplorer != null)
                        selectedTreeViewExplorer.IsSelected = false;
                }

                TreeView_MultiSelect_Executionrootinventory(originalTreeViewExplorer, selectedItemsExecution, treeViewExplorerExecutionRootItem.Children, QatConstants.TveExecutionInventoryTitle, false, e.ChangedButton);

                Remove.IsEnabled = false;
                RemoveAll.IsEnabled = false;
                if (treeViewExplorerExecutionRootItem != null)
                {
                    if (treeViewExplorerExecutionRootItem.Children.Count > 0)
                    {
                        if (originalTreeViewExplorer != null)
                            if (selectedItemsInventoryToExecution.Count > 0 && (e.OriginalSource.GetType().ToString() != "System.Windows.Controls.Grid") && originalTreeViewExplorer.ItemName != "Execution Inventory")
                            {
                                Remove.IsEnabled = true;
                                RemoveAll.IsEnabled = true;

                                if ((selectedItemsInventoryToExecution.Count > 0 && (e.OriginalSource.GetType().ToString() == "System.Windows.Controls.Grid")))
                                {
                                    Remove.IsEnabled = false;
                                    RemoveAll.IsEnabled = true;
                                }
                               
                                }
                            else if(selectedItemsExecution.Count>0 && originalTreeViewExplorer.ItemName != "Execution Inventory")
                             {
                                Remove.IsEnabled = true;
                            }
                    }
                    if (treeViewExplorerExecutionRootItem.Children.Count != 0)
                    {
                        RemoveAll.IsEnabled = true;
                    }
                }
                else
                {
                    Remove.IsEnabled = false;
                    RemoveAll.IsEnabled = false;
                }

            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                //MessageBox.Show("Exception\n " + ex.Message, "Error Code - EC16003", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void TreeViewInventory_PreviewKeyDownUp(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Apps)
                {
                    e.Handled = true;
                }
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                //MessageBox.Show("Exception\n " + ex.Message, "Error Code - EC14003", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void TreeViewInventory_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {

            TreeViewExplorer originalTreeViewExplorer = TreeViewInventory.SelectedItem as TreeViewExplorer;
            if (originalTreeViewExplorer == null)
                return;

            if ((Keyboard.IsKeyDown(Key.Up)) || (Keyboard.IsKeyDown(Key.Down)) || (Keyboard.IsKeyDown(Key.Left)) || (Keyboard.IsKeyDown(Key.Right)))
            {
                if(isInventorySearchListSelected==false)
                {
                    TreeView_MultiSelect(originalTreeViewExplorer, selectedItemsInventory, treeViewExplorerInventoryList, null, true, MouseButton.Left);
                }
                else
                {
                    TreeView_MultiSelect(originalTreeViewExplorer, selectedItemsInventory, treeViewExplorerSearchList, null, true, MouseButton.Left);
                }                
            }
        }

        private void TreeViewExecution_PreviewKeyDownUp(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Apps)
                {
                    e.Handled = true;
                }
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                //MessageBox.Show("Exception\n " + ex.Message, "Error Code - EC14003", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void TreeViewExecution_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {

            TreeViewExplorer originalTreeViewExplorer = TreeViewExecution.SelectedItem as TreeViewExplorer;
            if (originalTreeViewExplorer == null)
                return;

            if ((Keyboard.IsKeyDown(Key.Up)) || (Keyboard.IsKeyDown(Key.Down)) || (Keyboard.IsKeyDown(Key.Left)) || (Keyboard.IsKeyDown(Key.Right)))
            {
                TreeView_MultiSelect_Executionrootinventory(originalTreeViewExplorer, selectedItemsExecution, treeViewExplorerExecutionRootItem.Children, QatConstants.TveExecutionInventoryTitle, true, MouseButton.Left);
            }
        }

        private void TreeView_MultiSelect(TreeViewExplorer originalTreeViewExplorer, Dictionary<int, TreeViewExplorer> selectedItems, List<TreeViewExplorer> inventoryList, string parent, bool isKeyBoardInput, MouseButton changedButton)
        {
            try
            {
                if (isKeyBoardInput == false && originalTreeViewExplorer == null)
                {
                    foreach (TreeViewExplorer item in selectedItems.Values)
                    {
                        item.IsMultiSelectOn = false;
                        item.IsSelected = false;

                        foreach (TreeViewExplorer testPlan in item.Children)
                        {
                            testPlan.IsMultiSelectOn = false;
                            testPlan.IsSelected = false;

                            foreach (TreeViewExplorer testCase in testPlan.Children)
                            {
                                testCase.IsMultiSelectOn = false;
                                testCase.IsSelected = false;
                            }
                        }
                    }
                    selectedItems.Clear();
                    return;
                }

                if (originalTreeViewExplorer.Parent != null && originalTreeViewExplorer.Parent.ItemName != parent)
                    if (originalTreeViewExplorer.Parent.Parent == null || originalTreeViewExplorer.Parent.Parent.ItemName == parent)
                        originalTreeViewExplorer = originalTreeViewExplorer.Parent;
                    else if (originalTreeViewExplorer.Parent.Parent.Parent == null || originalTreeViewExplorer.Parent.Parent.Parent.ItemName == parent)
                        originalTreeViewExplorer = originalTreeViewExplorer.Parent.Parent;

                if (originalTreeViewExplorer == null)
                    return;

                if (isKeyBoardInput == false && ((changedButton == MouseButton.Left && Mouse.LeftButton == MouseButtonState.Pressed) || (changedButton == MouseButton.Right && Mouse.RightButton == MouseButtonState.Pressed)))
                    skipMouseReleaseButton = false;

                if (isKeyBoardInput || (changedButton == MouseButton.Left && Mouse.LeftButton == MouseButtonState.Pressed && selectedItems.Count < 2) ||
                    (changedButton == MouseButton.Left && Mouse.LeftButton == MouseButtonState.Released && selectedItems.Count >= 2))
                {
                    if ((Keyboard.IsKeyDown(Key.LeftShift)) || (Keyboard.IsKeyDown(Key.RightShift)))
                    {
                        if ((selectedItems.Count > 0 && !String.Equals(selectedItems.Values.First().ItemType, QatConstants.TveDesignerHeaderItemType) && String.Equals(selectedItems.Values.First().ItemType, originalTreeViewExplorer.ItemType)) ||
                            (selectedItems.Count == 0 && !String.Equals(originalTreeViewExplorer.ItemType, QatConstants.TveDesignerHeaderItemType)))
                        {
                            int endIndex = inventoryList.IndexOf(originalTreeViewExplorer);
                            int startIndex = endIndex;
                            if (selectedItems.Count > 0)
                            {
                                TreeViewExplorer startItem = selectedItems.Values.Last();
                                startIndex = inventoryList.IndexOf(startItem);
                            }

                            if (startIndex <= endIndex)
                            {
                                startIndex += 1;
                                for (int i = startIndex; i <= endIndex; i++)
                                {
                                    if (!selectedItems.Values.Contains(inventoryList[i]))
                                    {
                                        selectedItems.Add(inventoryList[i].GetHashCode(), inventoryList[i]);
                                        inventoryList[i].IsMultiSelectOn = true;
                                    }
                                    else
                                    {
                                        selectedItems.Remove(inventoryList[i - 1].GetHashCode());
                                        inventoryList[i - 1].IsMultiSelectOn = false;
                                    }
                                }
                            }
                            else
                            {
                                startIndex -= 1;
                                for (int i = startIndex; i >= endIndex; i--)
                                {
                                    if (!selectedItems.Values.Contains(inventoryList[i]))
                                    {
                                        selectedItems.Add(inventoryList[i].GetHashCode(), inventoryList[i]);
                                        inventoryList[i].IsMultiSelectOn = true;
                                    }
                                    else
                                    {
                                        selectedItems.Remove(inventoryList[i + 1].GetHashCode());
                                        inventoryList[i + 1].IsMultiSelectOn = false;
                                    }
                                }
                            }
                        }
                    }
                    else if ((Keyboard.IsKeyDown(Key.LeftCtrl)) || (Keyboard.IsKeyDown(Key.RightCtrl)))
                    {
                        if ((selectedItems.Count > 0 && !String.Equals(selectedItems.Values.First().ItemType, QatConstants.TveDesignerHeaderItemType) && String.Equals(selectedItems.Values.First().ItemType, originalTreeViewExplorer.ItemType)) ||
                            (selectedItems.Count == 0 && !String.Equals(originalTreeViewExplorer.ItemType, QatConstants.TveDesignerHeaderItemType)))
                        {
                            if (!selectedItems.ContainsKey(originalTreeViewExplorer.GetHashCode()))
                            {
                                if (selectedItems.Count == 1)
                                    skipMouseReleaseButton = true;

                                selectedItems.Add(originalTreeViewExplorer.GetHashCode(), originalTreeViewExplorer);
                                originalTreeViewExplorer.IsMultiSelectOn = true;
                            }
                            else if (!skipMouseReleaseButton)
                            {
                                originalTreeViewExplorer.IsMultiSelectOn = false;
                                selectedItems.Remove(originalTreeViewExplorer.GetHashCode());
                            }
                        }
                    }
                    else
                    {
                        foreach (TreeViewExplorer item in selectedItems.Values)
                        {
                            item.IsMultiSelectOn = false;
                        }
                        selectedItems.Clear();
                        selectedItems.Add(originalTreeViewExplorer.GetHashCode(), originalTreeViewExplorer);
                        originalTreeViewExplorer.IsMultiSelectOn = true;
                    }
                }

                if (isKeyBoardInput == false && changedButton == MouseButton.Right && Mouse.RightButton == MouseButtonState.Pressed)
                {
                    if (!selectedItems.ContainsKey(originalTreeViewExplorer.GetHashCode()))
                    {
                        foreach (TreeViewExplorer item in selectedItems.Values)
                        {
                            item.IsMultiSelectOn = false;
                        }
                        selectedItems.Clear();
                        selectedItems.Add(originalTreeViewExplorer.GetHashCode(), originalTreeViewExplorer);
                        originalTreeViewExplorer.IsMultiSelectOn = true;
                    }
                }
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                // MessageBox.Show("Exception\n " + ex.Message, "Error Code - EC16005", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
       
        private void TreeView_MultiSelect_Executionrootinventory(TreeViewExplorer originalTreeViewExplorer, Dictionary<int, TreeViewExplorer> selectedItems, List<TreeViewExplorer> inventoryList, string parent, bool isKeyBoardInput, MouseButton changedButton)
        {
            try
            {
                if (isKeyBoardInput == false && originalTreeViewExplorer == null)
                {
                    foreach (TreeViewExplorer item in selectedItemsnonTS.Values)
                    {
                        item.IsMultiSelectOn = false;
                        item.IsSelected = false;

                        foreach (TreeViewExplorer testPlan in item.Children)
                        {
                            testPlan.IsMultiSelectOn = false;
                            testPlan.IsSelected = false;

                            foreach (TreeViewExplorer testCase in testPlan.Children)
                            {
                                testCase.IsMultiSelectOn = false;
                                testCase.IsSelected = false;
                            }
                        }
                    }
                    selectedItems.Clear();
                    selectedItemsnonTS.Clear();
                    selectedItemsnonTS_hash.Clear();
                    return;
                }

              
                TreeViewExplorer Suite_TreeViewExplorer = null;
                if (originalTreeViewExplorer.Parent != null && originalTreeViewExplorer.Parent.ItemName != parent)
                {
                    if (originalTreeViewExplorer.Parent.Parent == null || originalTreeViewExplorer.Parent.Parent.ItemName == parent)
                        Suite_TreeViewExplorer = originalTreeViewExplorer.Parent;
                    else if (originalTreeViewExplorer.Parent.Parent.Parent == null || originalTreeViewExplorer.Parent.Parent.Parent.ItemName == parent)
                        Suite_TreeViewExplorer = originalTreeViewExplorer.Parent.Parent;
                    
                }
                else
                    Suite_TreeViewExplorer = originalTreeViewExplorer;




                if (originalTreeViewExplorer == null)
                    return;

                if (isKeyBoardInput == false && ((changedButton == MouseButton.Left && Mouse.LeftButton == MouseButtonState.Pressed) || (changedButton == MouseButton.Right && Mouse.RightButton == MouseButtonState.Pressed)))
                    skipMouseReleaseButton = false;

                if (isKeyBoardInput || (changedButton == MouseButton.Left && Mouse.LeftButton == MouseButtonState.Pressed && selectedItems.Count < 2) ||
                    (changedButton == MouseButton.Left && Mouse.LeftButton == MouseButtonState.Released && selectedItems.Count >= 2))
                {
                    if ((Keyboard.IsKeyDown(Key.LeftShift)) || (Keyboard.IsKeyDown(Key.RightShift)))
                    {
                        if ((selectedItemsnonTS.Count > 0 && !String.Equals(selectedItemsnonTS.Values.First().ItemType, QatConstants.TveDesignerHeaderItemType) && String.Equals(selectedItemsnonTS.Values.First().ItemType, originalTreeViewExplorer.ItemType)&& String.Equals(selectedItemsnonTS.Values.First().Parent, originalTreeViewExplorer.Parent)) ||
                            (selectedItemsnonTS.Count == 0 && !String.Equals(originalTreeViewExplorer.ItemType, QatConstants.TveDesignerHeaderItemType)))
                        {
                            List < TreeViewExplorer >  treeparentInventoryList = originalTreeViewExplorer.Parent.Children;
                            int endIndex = treeparentInventoryList.IndexOf(originalTreeViewExplorer);
                            int startIndex = endIndex;
                            if (selectedItemsnonTS.Count > 0)
                            {
                                TreeViewExplorer startItem = selectedItemsnonTS.Values.Last();
                                startIndex = treeparentInventoryList.IndexOf(startItem);
                            }
                            else
                            {
                              
                                startIndex = endIndex = treeparentInventoryList.IndexOf(originalTreeViewExplorer);
                            }


                            if (startIndex <= endIndex)
                            {
                                startIndex += 1;
                                for (int i = startIndex; i <= endIndex; i++)
                                {
                                    if (!selectedItemsnonTS.Values.Contains(treeparentInventoryList[i]))
                                    {
                                        selectedItemsnonTS.Add(treeparentInventoryList[i].GetHashCode(), treeparentInventoryList[i]);
                                        selectedItemsnonTS_hash.Add(treeparentInventoryList[i].GetHashCode(), Suite_TreeViewExplorer.GetHashCode());
                                        treeparentInventoryList[i].IsMultiSelectOn = true;
                                    }
                                    else
                                    {
                                        selectedItemsnonTS.Remove(treeparentInventoryList[i - 1].GetHashCode());
                                        selectedItemsnonTS_hash.Remove(treeparentInventoryList[i - 1].GetHashCode());
                                        treeparentInventoryList[i - 1].IsMultiSelectOn = false;
                                    }
                                    if (!selectedItems.ContainsKey(treeparentInventoryList[i].GetHashCode()))
                                    {
                                        selectedItems.Add(treeparentInventoryList[i].GetHashCode(), treeparentInventoryList[i]);
                                    }
                                    else if ((selectedItemsnonTS_hash.Count == 0)|| (!selectedItemsnonTS_hash.Values.Contains(Suite_TreeViewExplorer.GetHashCode())))
                                    {
                                        selectedItems.Remove(treeparentInventoryList[i].GetHashCode());
                                    }
                                }
                            }
                            else
                            {
                                startIndex -= 1;
                                for (int i = startIndex; i >= endIndex; i--)
                                {
                                    if (!selectedItemsnonTS.Values.Contains(treeparentInventoryList[i]))
                                    {
                                        selectedItemsnonTS.Add(treeparentInventoryList[i].GetHashCode(), treeparentInventoryList[i]);
                                        selectedItemsnonTS_hash.Add(treeparentInventoryList[i].GetHashCode(), Suite_TreeViewExplorer.GetHashCode());
                                        treeparentInventoryList[i].IsMultiSelectOn = true;
                                    }
                                    else
                                    {
                                        selectedItemsnonTS.Remove(treeparentInventoryList[i + 1].GetHashCode());
                                        selectedItemsnonTS_hash.Remove(treeparentInventoryList[i + 1].GetHashCode());
                                        treeparentInventoryList[i + 1].IsMultiSelectOn = false;
                                    }
                                    if (!selectedItems.ContainsKey(treeparentInventoryList[i].GetHashCode()))
                                    {
                                        selectedItems.Add(treeparentInventoryList[i].GetHashCode(), treeparentInventoryList[i]);
                                    }
                                    else if ((selectedItemsnonTS_hash.Count == 0) || (!selectedItemsnonTS_hash.Values.Contains(Suite_TreeViewExplorer.GetHashCode())))
                                    {
                                        selectedItems.Remove(treeparentInventoryList[i].GetHashCode());
                                    }
                                }
                            }
                            
                        }
                        
                            else if((selectedItemsnonTS.Count > 0 && !String.Equals(selectedItemsnonTS.Values.First().ItemType, QatConstants.TveDesignerHeaderItemType) && ((!String.Equals(selectedItemsnonTS.Values.First().ItemType, originalTreeViewExplorer.ItemType))||(!String.Equals(selectedItemsnonTS.Values.First().Parent, originalTreeViewExplorer.Parent)))))
                        {
                            foreach (TreeViewExplorer item in selectedItemsnonTS.Values)
                            {
                                item.IsMultiSelectOn = false;
                            }
                            selectedItemsnonTS.Clear();
                            selectedItemsnonTS_hash.Clear();
                            selectedItemsnonTS.Add(originalTreeViewExplorer.GetHashCode(), originalTreeViewExplorer);
                            selectedItemsnonTS_hash.Add(originalTreeViewExplorer.GetHashCode(), Suite_TreeViewExplorer.GetHashCode());
                            selectedItems.Clear();
                           
                            selectedItems.Add(Suite_TreeViewExplorer.GetHashCode(), Suite_TreeViewExplorer);
                          
                            originalTreeViewExplorer.IsMultiSelectOn = true;


                        }
                    }
                    else if ((Keyboard.IsKeyDown(Key.LeftCtrl)) || (Keyboard.IsKeyDown(Key.RightCtrl)))
                    {
                        if ((selectedItemsnonTS.Count > 0 && !String.Equals(selectedItemsnonTS.Values.First().ItemType, QatConstants.TveDesignerHeaderItemType) && String.Equals(selectedItemsnonTS.Values.First().ItemType, originalTreeViewExplorer.ItemType) && String.Equals(selectedItemsnonTS.Values.First().Parent, originalTreeViewExplorer.Parent)) ||
                            (selectedItemsnonTS.Count == 0 && !String.Equals(originalTreeViewExplorer.ItemType, QatConstants.TveDesignerHeaderItemType)))
                        {
                            if (!selectedItemsnonTS.ContainsKey(originalTreeViewExplorer.GetHashCode()))
                            {
                                if (selectedItemsnonTS.Count == 1)
                                    skipMouseReleaseButton = true;

                                selectedItemsnonTS.Add(originalTreeViewExplorer.GetHashCode(), originalTreeViewExplorer);
                                selectedItemsnonTS_hash.Add(originalTreeViewExplorer.GetHashCode(), Suite_TreeViewExplorer.GetHashCode());
                                originalTreeViewExplorer.IsMultiSelectOn = true;
                            }
                            else if (!skipMouseReleaseButton)
                            {
                                originalTreeViewExplorer.IsMultiSelectOn = false;
                                selectedItemsnonTS.Remove(originalTreeViewExplorer.GetHashCode());
                                selectedItemsnonTS_hash.Remove(originalTreeViewExplorer.GetHashCode());
                            }
                            if (!selectedItems.ContainsKey(Suite_TreeViewExplorer.GetHashCode()))
                            {
                                selectedItems.Add(Suite_TreeViewExplorer.GetHashCode(), Suite_TreeViewExplorer);

                            }
                            else if ((!skipMouseReleaseButton)&& (selectedItemsnonTS_hash.Count == 0))
                            {
                                selectedItems.Remove(Suite_TreeViewExplorer.GetHashCode());
                            }
                            else if((!skipMouseReleaseButton) && (!selectedItemsnonTS_hash.Values.Contains(Suite_TreeViewExplorer.GetHashCode())))
                                 selectedItems.Remove(Suite_TreeViewExplorer.GetHashCode());

                        }
                        else if((selectedItemsnonTS.Count > 0 && !String.Equals(selectedItemsnonTS.Values.First().ItemType, QatConstants.TveDesignerHeaderItemType) && (!String.Equals(selectedItemsnonTS.Values.First().ItemType, originalTreeViewExplorer.ItemType)|| !String.Equals(selectedItemsnonTS.Values.First().Parent, originalTreeViewExplorer.Parent))))
                        {
                            foreach (TreeViewExplorer item in selectedItemsnonTS.Values)
                            {
                                item.IsMultiSelectOn = false;
                            }
                            selectedItemsnonTS.Clear();
                            selectedItemsnonTS_hash.Clear();
                            selectedItemsnonTS.Add(originalTreeViewExplorer.GetHashCode(), originalTreeViewExplorer);
                            selectedItemsnonTS_hash.Add(originalTreeViewExplorer.GetHashCode(), Suite_TreeViewExplorer.GetHashCode());
                            selectedItems.Clear();
                         
                            selectedItems.Add(Suite_TreeViewExplorer.GetHashCode(), Suite_TreeViewExplorer);
                          
                            originalTreeViewExplorer.IsMultiSelectOn = true;


                        }

                    }
                    else
                    {
                       
                        foreach (TreeViewExplorer item in selectedItemsnonTS.Values)
                        {
                            item.IsMultiSelectOn = false;
                        }
                        selectedItemsnonTS.Clear();
                        selectedItemsnonTS_hash.Clear();
                        selectedItemsnonTS.Add(originalTreeViewExplorer.GetHashCode(), originalTreeViewExplorer);
                        selectedItemsnonTS_hash.Add(originalTreeViewExplorer.GetHashCode(), Suite_TreeViewExplorer.GetHashCode());
                        selectedItems.Clear();
                        
                            selectedItems.Add(Suite_TreeViewExplorer.GetHashCode(), Suite_TreeViewExplorer);
                      
                        originalTreeViewExplorer.IsMultiSelectOn = true;
                    }
                }

                if (isKeyBoardInput == false && changedButton == MouseButton.Right && Mouse.RightButton == MouseButtonState.Pressed)
                {
                    if (!selectedItemsnonTS.ContainsKey(originalTreeViewExplorer.GetHashCode()))
                    {
                        foreach (TreeViewExplorer item in selectedItemsnonTS.Values)
                        {
                            item.IsMultiSelectOn = false;
                        }
                        selectedItemsnonTS.Clear();
                        selectedItemsnonTS_hash.Clear();
                        selectedItemsnonTS.Add(originalTreeViewExplorer.GetHashCode(), originalTreeViewExplorer);
                        selectedItemsnonTS_hash.Add(originalTreeViewExplorer.GetHashCode(), Suite_TreeViewExplorer.GetHashCode());
                        selectedItems.Clear();
                        selectedItems.Add(Suite_TreeViewExplorer.GetHashCode(), Suite_TreeViewExplorer);
                        originalTreeViewExplorer.IsMultiSelectOn = true;
                    }
                }
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                // MessageBox.Show("Exception\n " + ex.Message, "Error Code - EC16005", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void TreeViewInventory_MouseMove(object sender, MouseEventArgs e)
        {
            try
            {
                FrameworkElement sourceElement = e.OriginalSource as FrameworkElement;
                if (sourceElement == null)
                    return;

                TreeViewExplorer originalTreeViewExplorer = sourceElement.DataContext as TreeViewExplorer;
                if (originalTreeViewExplorer == null)
                    return;

                if (originalTreeViewExplorer.ItemType != QatConstants.DbTestSuiteTable)
                {
                    originalTreeViewExplorer = originalTreeViewExplorer.Parent;
                    if (originalTreeViewExplorer.ItemType != QatConstants.DbTestSuiteTable)
                    {
                        originalTreeViewExplorer = originalTreeViewExplorer.Parent;
                    }
                }

                if (e.LeftButton == MouseButtonState.Pressed && e.OriginalSource.GetType().ToString() != "System.Windows.Controls.Primitives.Thumb")
                {
                    DataObject dataSourceObject = new DataObject();
                    dataSourceObject.SetData(this.GetHashCode().ToString(), this);
                    if (this.selectedItemsInventory != null && selectedItemsInventory.Values.Contains(originalTreeViewExplorer))
                    {
                        DragDropItem dragData = new DragDropItem();
                        dragData.DragSourceType = "InventoryTreeView";
                        dragData.DragSourceHashCode = this.GetHashCode();
                        dragData.InventorySelectedItemsDictionary = selectedItemsInventory;

                        if (selectedItemsInventory.Count > 0)
                        {
                            CreateDragDropWindow(selectedItemsInventory);
                            DragDrop.DoDragDrop(sender as DependencyObject, new DataObject(typeof(DragDropItem), dragData), DragDropEffects.Copy);
                            if (_dragdropWindow != null)
                            {
                                _dragdropWindow.Close();
                                _dragdropWindow = null;
                            }
                        }

                    }
                    else
                    {
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                //MessageBox.Show("Exception\n " + ex.Message, "Error Code - EC16006", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private Window _dragdropWindow;
        private void CreateDragDropWindow(Dictionary<int, TreeViewExplorer> selectedItems)
        {
            try
            {

                FormattedText t = null;

                _dragdropWindow = new Window
                {
                    WindowStyle = WindowStyle.None,
                    AllowsTransparency = true,
                    AllowDrop = false,
                    Background = null,

                    IsHitTestVisible = false,
                    SizeToContent = SizeToContent.WidthAndHeight,
                    Topmost = true,
                    ShowInTaskbar = false

                };

                TextBlock visual = new TextBlock();


                visual.Margin = new Thickness(20, 20, 0, 0);


                if (selectedItems.Count > 1)
                {
                    visual.Text = selectedItems.Values.First().ItemName + "+ " + (selectedItems.Count - 1).ToString() + " more Items";
                }
                else
                {
                    visual.Text = selectedItems.Values.First().ItemName;
                }

                t = new FormattedText(selectedItems.Values.First().ItemName, CultureInfo.CurrentUICulture, FlowDirection.LeftToRight,
             new Typeface(selectedItems.Values.First().ItemTextBox.FontFamily, selectedItems.Values.First().ItemTextBox.FontStyle, selectedItems.Values.First().ItemTextBox.FontWeight, selectedItems.Values.First().ItemTextBox.FontStretch),
             selectedItems.Values.First().ItemTextBox.FontSize,
             Brushes.Black);

                visual.Width = t.Width + 100;
                visual.Height = t.Height + 10;

                _dragdropWindow.Content = visual;
                Win32Point w32Mouse = new Win32Point();
                GetCursorPos(ref w32Mouse);
                _dragdropWindow.Left = w32Mouse.X;
                _dragdropWindow.Top = w32Mouse.Y;
                _dragdropWindow.Show();
            }

            
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                //MessageBox.Show("Exception\n " + ex.Message, "Error Code - ECXXCDD", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }

        private void TreeViewExecution_Drop(object sender, DragEventArgs e)
        {
            try
            {
                if(!treeViewExplorerExecutionRootItem.IsEnabled)
                {
                    e.Handled = true;
                    return;
                }

                if (_dragdropWindow != null)
                {
                    _dragdropWindow.Close();
                    _dragdropWindow = null;
                }

                if (e.Data == null)
                    return;

                DragDropItem dragData = (DragDropItem)e.Data.GetData(typeof(DragDropItem));
                if (dragData == null)
                    return;

                FrameworkElement sourceElement = e.OriginalSource as FrameworkElement;
                if (sourceElement == null)
                    return;

                TreeViewExplorer targetTreeViewExplorer = sourceElement.DataContext as TreeViewExplorer;
                if(treeViewExplorerExecutionRootItem != null && treeViewExplorerExecutionRootItem.Children.Count > 0)
                {
                    if (targetTreeViewExplorer == null)
                    {
                        if (sourceElement as Border != null)
                        {
                            Border sourceBorder = sourceElement as Border;
                            if (sourceBorder.Child as ScrollViewer != null)
                                targetTreeViewExplorer = treeViewExplorerExecutionRootItem.Children.First();
                        }
                        else if (sourceElement as Grid != null)
                            targetTreeViewExplorer = treeViewExplorerExecutionRootItem.Children.Last();
                    }
                    else if (targetTreeViewExplorer.ItemType == QatConstants.TveDesignerHeaderItemType)
                    {
                        targetTreeViewExplorer = treeViewExplorerExecutionRootItem.Children.First();
                    }
                }

                if (treeViewExplorerExecutionRootItem != null && treeViewExplorerExecutionRootItem.Children.Count > 0)
                {
                    if(targetTreeViewExplorer.ItemType != QatConstants.DbTestSuiteTable)
                    {
                        targetTreeViewExplorer = targetTreeViewExplorer.Parent;
                        if (targetTreeViewExplorer.ItemType != QatConstants.DbTestSuiteTable)
                        {
                            targetTreeViewExplorer = targetTreeViewExplorer.Parent;
                        }
                    }
                }

                if (dragData.DragSourceType == "InventoryTreeView" && dragData.DragSourceHashCode == this.GetHashCode())
                {
                    if (selectedItemsInventory != null)
                    {
                        this.chk_ConfigurationFile.IsEnabled = true;
                        this.btn_loop.IsEnabled = true;
                        this.btn_emailReport.IsEnabled = true;
                        btn_Execute.IsEnabled = true;
                        Btn_Execution_Delay.IsEnabled = true;
                        foreach (TreeViewExplorer item in selectedItemsInventory.Values)
                        {
                            TreeViewExplorer addedItem = new TreeViewExplorer(item);
                            treeViewExplorerExecutionRootItem.AddChildren_withCheckbox(addedItem);
                            foreach (TreeViewExplorer testPlan in addedItem.Children)
                            {
                                testPlan.DesignNameViewIsEnabled = true;
                            }
                            selectedItemsInventoryToExecution.Add(item);
                        }
                    }

                    if (treeViewExplorerExecutionRootItem.Children.Count == 0)
                    {
                        treeViewExplorerExecutionRootItem.IsCheckedVisibility = Visibility.Hidden;
                        this.chk_ConfigurationFile.IsEnabled = false;
                        this.btn_loop.IsEnabled = false;
                        this.btn_emailReport.IsEnabled = false;
                        btn_Execute.IsEnabled = false;
                        Btn_Execution_Delay.IsEnabled = false;
                    }
                    else
                        treeViewExplorerExecutionRootItem.IsCheckedVisibility = Visibility.Visible;

                    TreeViewExecution.DataContext = null;
                    TreeViewExecution.DataContext = treeViewExplorerExecutionRootList;
                }
                else if (dragData.DragSourceType == "ExecutionTreeView" && dragData.DragSourceHashCode == this.GetHashCode())
                {
                    if (targetTreeViewExplorer != null && treeViewExplorerExecutionRootItem.Children.Contains(targetTreeViewExplorer))
                    {
                        int targetIndex = treeViewExplorerExecutionRootItem.Children.IndexOf(targetTreeViewExplorer);

                        int sourceIndex = treeViewExplorerExecutionRootItem.Children.IndexOf(selectedItemsExecution.Values.First());

                        if (sourceIndex < targetIndex && targetIndex < treeViewExplorerExecutionRootItem.Children.Count)
                            targetIndex += 1;

                        if (targetIndex < 0 || sourceIndex < 0 || sourceIndex == targetIndex)
                            return;

                        foreach (TreeViewExplorer item in selectedItemsExecution.Values)
                        {
                            if(item.ItemType== QatConstants.DbTestSuiteTable)
                            treeViewExplorerExecutionRootItem.InsertChildren(targetIndex, item, true);
                        }

                        foreach (TreeViewExplorer item in selectedItemsExecution.Values)
                        {
                            treeViewExplorerExecutionRootItem.RemoveChildren(item);
                        }

                        TreeViewExecution.DataContext = null;
                        TreeViewExecution.DataContext = treeViewExplorerExecutionRootList;
						selectedItemsExecution.Clear();
                        selectedItemsnonTS.Clear();
                        selectedItemsnonTS_hash.Clear();

                    }
                }
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                //MessageBox.Show("Exception\n " + ex.Message, "Error Code - EC16007", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void MenuItem_Remove_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                foreach (TreeViewExplorer tree in selectedItemsExecution.Values)
                {
                    if (treeViewExplorerExecutionRootItem.Children.Contains(tree))
                    {
                        if (selectedItemsInventoryToExecution.Contains(tree))
                            selectedItemsInventoryToExecution.Remove(tree);
                        //if (selectedItemsInventoryToExecution.Keys.Contains(tree.ItemKey))
                        //    selectedItemsInventoryToExecution.Remove(tree.ItemKey);
                        treeViewExplorerExecutionRootItem.RemoveChildren(tree);
                    }
                }

                if (treeViewExplorerExecutionRootItem.Children.Count == 0)
                {
                    treeViewExplorerExecutionRootItem.IsCheckedVisibility = Visibility.Hidden;
                    this.chk_ConfigurationFile.IsEnabled = false;
                    this.btn_loop.IsEnabled = false;
                    this.btn_emailReport.IsEnabled = false;
                    btn_Execute.IsEnabled = false;
                    Btn_Execution_Delay.IsEnabled = false;
                }
                else
                    treeViewExplorerExecutionRootItem.IsCheckedVisibility = Visibility.Visible;

                TreeViewExecution.DataContext = null;
                TreeViewExecution.DataContext = treeViewExplorerExecutionRootList;

                selectedItemsExecution.Clear();
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                // MessageBox.Show("Exception\n " + ex.Message, "Error Code - EC16008", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void MenuItem_RemoveAll_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                treeViewExplorerExecutionRootItem.ClearChildren();
                selectedItemsInventoryToExecution.Clear();
                treeViewExplorerExecutionRootItem.IsCheckedVisibility = Visibility.Hidden;
                this.btn_loop.IsEnabled = false;
                this.chk_ConfigurationFile.IsEnabled = false;
                this.btn_emailReport.IsEnabled = false;
                btn_Execute.IsEnabled = false;
                Btn_Execution_Delay.IsEnabled = false;
                TreeViewExecution.DataContext = null;
                TreeViewExecution.DataContext = treeViewExplorerExecutionRootList;
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                // MessageBox.Show("Exception\n " + ex.Message, "Error Code - ECxxxxx", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void Btn_Search_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sortingOrders_OriginalList != sortingOrders)
                {

                    Sorting_InventoryList(treeViewExplorerInventoryList, sortingOrders,false);
                    sortingOrders_OriginalList = sortingOrders;
                }
                var temp_explorerlist = new List<Object>(treeViewExplorerInventoryList.Select(x => x.Clone()));

                foreach(TreeViewExplorer suite in temp_explorerlist)
                {
                    var planlist = new List<Object>(suite.Children.Select(x => x.Clone()));
                    suite.ClearChildren();

                    foreach (TreeViewExplorer plan in planlist)
                    {
                        suite.AddChildren_withCheckbox(plan);
                      
                    }

                }
                //List<TreeViewExplorer> treeViewExplorerSearchList = new List<TreeViewExplorer>();
                treeViewExplorerSearchList.Clear();
                // if search text present refresh the search
                if (String.IsNullOrEmpty(txt_Search.Text) == false)
                {
                    foreach (TreeViewExplorer testSuite in temp_explorerlist)
                    {
                        //TreeViewExplorer testSuite = new TreeViewExplorer(testSuites, true);
                        CultureInfo culture1 = CultureInfo.InvariantCulture;
                        if (culture1.CompareInfo.IndexOf(testSuite.ItemName, txt_Search.Text, CompareOptions.IgnoreCase) >= 0)
                        {
                            if (!treeViewExplorerSearchList.Contains(testSuite))
                                treeViewExplorerSearchList.Add(testSuite);
                        }

                        foreach (TreeViewExplorer testPlans in testSuite.Children)
                        {
                            CultureInfo culture2 = CultureInfo.InvariantCulture;
                            if (culture2.CompareInfo.IndexOf(testPlans.ItemName, txt_Search.Text, CompareOptions.IgnoreCase) >= 0)
                            {
                                if (!treeViewExplorerSearchList.Contains(testSuite))
                                {
                                    treeViewExplorerSearchList.Add(testSuite);
                                    
                                    
                                }
                                if(!testSuite.IsExpanded)
                                    testSuite.IsExpanded = true;
                            }

                            foreach (TreeViewExplorer testCases in testPlans.Children)
                            {
                                CultureInfo culture3 = CultureInfo.InvariantCulture;
                                if (culture3.CompareInfo.IndexOf(testCases.ItemName, txt_Search.Text, CompareOptions.IgnoreCase) >= 0)
                                {
                                    if (!treeViewExplorerSearchList.Contains(testSuite))
                                    {

                                        treeViewExplorerSearchList.Add(testSuite);
                                       
                                    }
                                    if (!testSuite.IsExpanded)
                                        testSuite.IsExpanded = true;
                                    if (!testPlans.IsExpanded)
                                        testPlans.IsExpanded = true;
                                        break;                                    
                                    
                                }
                            }
                        }
                    }

                    TreeViewInventory.DataContext = null;
                    TreeViewInventory.DataContext = treeViewExplorerSearchList;
                    isInventorySearchListSelected = true;
                }
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                //MessageBox.Show("Exception\n " + ex.Message, "Error Code - EC16010", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SearchBoxKeyDown(object sender, RoutedEventArgs e)
        {
            try
            {
                txt_Search.Clear();
                isInventorySearchListSelected = false;

                if (sortingOrders_OriginalList!=sortingOrders)
                {

                    Sorting_InventoryList(treeViewExplorerInventoryList, sortingOrders,true);
                    sortingOrders_OriginalList = sortingOrders;
                }
                else { 
                TreeViewInventory.DataContext = null;
                TreeViewInventory.DataContext = treeViewExplorerInventoryList;
                
                }
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                //MessageBox.Show("Exception\n " + ex.Message, "Error Code - EC16011", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btn_Refresh_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                RefreshRunner();
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                //MessageBox.Show("Exception\n " + ex.Message, "Error Code - EC16012", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RefreshRunner()
        {
            try
            {
                //List<TreeViewExplorer> treeViewExplorerSearchList = new List<TreeViewExplorer>();

                //if (TreeViewExecution.IsEnabled)

                if(treeViewExplorerExecutionRootItem.IsEnabled)
                    SetupTreeViewrFromDB(false, true, sortingOrders);
                else
                    SetupTreeViewrFromDB(false, false, sortingOrders);

                sortingOrders_OriginalList = sortingOrders;

                selectedItemsInventory.Clear();
                selectedItemsExecution.Clear();

                // if search text present refresh the search
                if (isInventorySearchListSelected == true)
                {
                    treeViewExplorerSearchList.Clear();
                    foreach (TreeViewExplorer testSuites in treeViewExplorerInventoryList)
                    {
                        TreeViewExplorer testSuite = new TreeViewExplorer(testSuites, true);
                        CultureInfo culture1 = CultureInfo.InvariantCulture;
                        if (culture1.CompareInfo.IndexOf(testSuite.ItemName, txt_Search.Text, CompareOptions.IgnoreCase) >= 0)
                        {
                            if (!treeViewExplorerSearchList.Contains(testSuite))
                                treeViewExplorerSearchList.Add(testSuite);
                        }

                        foreach (TreeViewExplorer testPlans in testSuite.Children)
                        {
                            CultureInfo culture2 = CultureInfo.InvariantCulture;
                            if (culture2.CompareInfo.IndexOf(testPlans.ItemName, txt_Search.Text, CompareOptions.IgnoreCase) >= 0)
                            {
                                if (!treeViewExplorerSearchList.Contains(testSuite))
                                {
                                    treeViewExplorerSearchList.Add(testSuite);
                                    testSuite.IsExpanded = true;
                                }
                            }

                            foreach (TreeViewExplorer testCases in testPlans.Children)
                            {
                                CultureInfo culture3 = CultureInfo.InvariantCulture;
                                if (culture3.CompareInfo.IndexOf(testCases.ItemName, txt_Search.Text, CompareOptions.IgnoreCase) >= 0)
                                {
                                    if (!treeViewExplorerSearchList.Contains(testSuite))
                                    {
                                        treeViewExplorerSearchList.Add(testSuite);
                                        testSuite.IsExpanded = true;
                                        
                                    }
                                    testPlans.IsExpanded = true;
                                    break;
                                }
                            }
                        }
                    }
                    //List<TreeViewExplorer> temptreeViewExplorerSearchList = new List<TreeViewExplorer>();
                    //temptreeViewExplorerSearchList = treeViewExplorerSearchList.ToList();
                    //treeViewExplorerSearchList.Clear();
                    //foreach (TreeViewExplorer testSuites in temptreeViewExplorerSearchList)
                    //{
                    //    if (testSuites.IsSelected == true)
                    //        testSuites.IsSelected = false;
                    //    if (testSuites.IsMultiSelectOn == true)
                    //       testSuites.IsMultiSelectOn = false;

                    //    treeViewExplorerSearchList.Add(testSuites);
                    //}

                    TreeViewInventory.DataContext = null;
                    TreeViewInventory.DataContext = treeViewExplorerSearchList;
                }
                else
                {
                    TreeViewInventory.DataContext = null;
                    TreeViewInventory.DataContext = treeViewExplorerInventoryList;
                }
                if (treeViewExplorerExecutionRootItem == null || treeViewExplorerExecutionRootItem.Children.Count == 0)
                {
                    this.btn_loop.IsEnabled = false;
                    this.chk_ConfigurationFile.IsEnabled = false;
                    this.btn_emailReport.IsEnabled = false;
                    btn_Execute.IsEnabled = false;
                    Btn_Execution_Delay.IsEnabled = false;
                }
                //ExecutionLoop EL = Application.Current.Windows.OfType<ExecutionLoop>().FirstOrDefault() ?? new ExecutionLoop(treeViewExplorerExecutionRootItem);
                //EL.DataContext = null;
                //EL.DataContext = treeViewExplorerExecutionRootItem.Children;
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                //MessageBox.Show("Exception\n " + ex.Message, "Error Code - EC16012", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SetupTreeViewrFromDB(bool updateDataContextInventory, bool updateDataContextExecution, string sortingType)
        {
            try
            {
                List<TreeViewExplorer> sortedTestSuiteList = null;
                DBConnection QscDatabase = new DBConnection();
                treeViewExplorerInventoryDictionary.Clear();
                Dictionary<int, ArrayList> TsTpLinkTable = new Dictionary<int, ArrayList>();
                Dictionary<int, ArrayList> TpTcLinkTable = new Dictionary<int, ArrayList>();
                List<TreeViewExplorer> testSuiteTreeViewList = QscDatabase.ReadTreeTable(QatConstants.DbTestSuiteTable, 0, this, null);
                Dictionary<int, ArrayList> testPlanTreeViewList = QscDatabase.ReadTreeTable_executionWindow(QatConstants.DbTestPlanTable, 0, this);
                Dictionary<int, ArrayList> testCaseTreeViewList = QscDatabase.ReadTreeTable_executionWindow(QatConstants.DbTestCaseTable, 0, this);


                if (sortingType == "Descending")
                {
                    TreeViewExplorer[] alphaTestSuiteSorted = testSuiteTreeViewList.ToArray();
                    Array.Sort(alphaTestSuiteSorted, new AlphanumComparatorFast());
                    Array.Reverse(alphaTestSuiteSorted);
                    sortedTestSuiteList = alphaTestSuiteSorted.ToList();
                }
                else if (sortingType == "Ascending")
                {
                    TreeViewExplorer[] alphaTestSuiteSorted = testSuiteTreeViewList.ToArray();
                    Array.Sort(alphaTestSuiteSorted, new AlphanumComparatorFast());
                    sortedTestSuiteList = alphaTestSuiteSorted.ToList();
                }
                else if (sortingType == "Date Created Descending")
                {
                   
                    TreeViewExplorer[] alphaTestSuiteSorted = testSuiteTreeViewList.ToArray();
                    Array.Sort(alphaTestSuiteSorted, new AlphanumComparatorFast_DateTime());
                    Array.Reverse(alphaTestSuiteSorted);
                    sortedTestSuiteList = alphaTestSuiteSorted.ToList();
                }
                else if (sortingType == "Date Created Ascending")
                {
                    sortingOrders = "Date Created Ascending";
                    TreeViewExplorer[] alphaTestSuiteSorted = testSuiteTreeViewList.ToArray();
                    Array.Sort(alphaTestSuiteSorted, new AlphanumComparatorFast_DateTime());
                    sortedTestSuiteList = alphaTestSuiteSorted.ToList();
                }
                Dictionary<int, string> designNameList = new Dictionary<int, string>();

                designNameList = QscDatabase.ReadDesignerNameList();
                TsTpLinkTable = QscDatabase.ReadLinkTable(QatConstants.DbTestSuiteTestPlanLinkTable);
                TpTcLinkTable = QscDatabase.ReadLinkTable(QatConstants.DbTestPlanTestCaseLinkTable);

                foreach (TreeViewExplorer testSuite in sortedTestSuiteList)
                {
                    TreeViewExplorer oldTestSuite = treeViewExplorerInventoryList.Find(x => x.ItemKey == testSuite.ItemKey);
                    if (TsTpLinkTable.ContainsKey(testSuite.ItemKey))
                    {

                        List<TreeViewExplorer> testPlanList = new List<TreeViewExplorer>();

                        foreach (int Tpid in TsTpLinkTable[testSuite.ItemKey])
                        {

                            //TreeViewExplorer testPlan ;
                            //testPlan = testPlanTreeViewList[Tpid];

                         
                                TreeViewExplorer TestPlan = new TreeViewExplorer((int)testPlanTreeViewList[Tpid][0], (string)testPlanTreeViewList[Tpid][1], (string)testPlanTreeViewList[Tpid][2], this, null, (DateTime?)testPlanTreeViewList[Tpid][5], (string)testPlanTreeViewList[Tpid][6], (DateTime?)testPlanTreeViewList[Tpid][7], (string)testPlanTreeViewList[Tpid][8], (string)testPlanTreeViewList[Tpid][9], (string)testPlanTreeViewList[Tpid][10], (DateTime?)testPlanTreeViewList[Tpid][11], (string)testPlanTreeViewList[Tpid][12], (int)testPlanTreeViewList[Tpid][13],(bool)testPlanTreeViewList[Tpid][14]);

                                List<TreeViewExplorer> testCaseList = new List<TreeViewExplorer>();
                                if (TpTcLinkTable.ContainsKey(Tpid))
                                {
                                    foreach (int TcId in TpTcLinkTable[Tpid])
                                    {
                                        //TreeViewExplorer testCase;
                                        //testCase = testCaseTreeViewList[TcId];
                                        
                                            TreeViewExplorer TestCase = new TreeViewExplorer((int)testCaseTreeViewList[TcId][0], (string)testCaseTreeViewList[TcId][1], (string)testCaseTreeViewList[TcId][2], this, null, (DateTime?)testCaseTreeViewList[TcId][5], (string)testCaseTreeViewList[TcId][6], (DateTime?)testCaseTreeViewList[TcId][7], (string)testCaseTreeViewList[TcId][8], (string)testCaseTreeViewList[TcId][9], (string)testCaseTreeViewList[TcId][10], (DateTime?)testCaseTreeViewList[TcId][11], (string)testCaseTreeViewList[TcId][12], (int)testCaseTreeViewList[TcId][13], true);
                                            testCaseList.Add(TestCase);                                        
                                    }
									
                                    TestPlan.AddChildrenList_withCheckbox(testCaseList);                                    
                                }
								
                                testPlanList.Add(TestPlan);
                                if(designNameList.ContainsKey(Tpid))
                                 TestPlan.DesignName = designNameList[Tpid];                            
                        }
						
                        testSuite.AddChildrenList_withCheckbox(testPlanList);
                        if (oldTestSuite != null)
                        {
                            foreach (TreeViewExplorer testPlan in testSuite.Children)
                            {
                                TreeViewExplorer oldTestPlan = oldTestSuite.Children.Find(x => x.ItemKey == testPlan.ItemKey);
                                if (oldTestPlan != null)
                                {
                                    testPlan.IsChecked = oldTestPlan.IsChecked;
                                    testPlan.IsExpanded = oldTestPlan.IsExpanded;
                                    foreach (TreeViewExplorer testCase in testPlan.Children)
                                    {
                                        TreeViewExplorer oldTestCase = oldTestPlan.Children.Find(x => x.ItemKey == testCase.ItemKey);
                                        if (oldTestCase != null)
                                        {
                                            testCase.IsChecked = oldTestCase.IsChecked;
                                            testCase.IsExpanded = oldTestCase.IsExpanded;
                                        }
                                    }
                                }
                            }
                        }
                    }

                    if (oldTestSuite != null)
                    {
                        testSuite.IsChecked = oldTestSuite.IsChecked;
                        testSuite.IsExpanded = oldTestSuite.IsExpanded;
                    }
					
                    treeViewExplorerInventoryDictionary.Add(testSuite.ItemKey, testSuite);
                }


                //                foreach (TreeViewExplorer testSuite in sortedTestSuiteList)
                //                {
                //                    TreeViewExplorer oldTestSuite = treeViewExplorerInventoryList.Find(x => x.ItemKey == testSuite.ItemKey);
                //                    if (oldTestSuite != null)
                //                    {
                //                        testSuite.IsChecked = oldTestSuite.IsChecked;
                //                        testSuite.IsExpanded = oldTestSuite.IsExpanded;
                //                        foreach (TreeViewExplorer testPlan in testSuite.Children)
                //                        {
                //                            TreeViewExplorer oldTestPlan = oldTestSuite.Children.Find(x => x.ItemKey == testPlan.ItemKey);
                //                            if (oldTestPlan != null)
                //                            {
                //                                testPlan.IsChecked = oldTestPlan.IsChecked;
                //                                testPlan.IsExpanded = oldTestPlan.IsExpanded;
                //                                foreach (TreeViewExplorer testCase in testPlan.Children)
                //                                {
                //                                    TreeViewExplorer oldTestCase = oldTestPlan.Children.Find(x => x.ItemKey == testCase.ItemKey);
                //                                    if (oldTestCase != null)
                //                                    {
                //                                        testCase.IsChecked = oldTestCase.IsChecked;
                //                                        testCase.IsExpanded = oldTestCase.IsExpanded;
                //                                    }
                //                                }
                //                            }
                //                        }
                //                    }
                //                }

                treeViewExplorerInventoryList = sortedTestSuiteList;

                List<TreeViewExplorer> newTestSuiteList = new List<TreeViewExplorer>();
                foreach (TreeViewExplorer testSuite in treeViewExplorerExecutionRootItem.Children)
                {
                    if (treeViewExplorerInventoryDictionary.Keys.Contains(testSuite.ItemKey))
                    {
                        TreeViewExplorer newTestSuite = treeViewExplorerInventoryDictionary[testSuite.ItemKey];
                        newTestSuiteList.Add(new TreeViewExplorer(newTestSuite));
                    }
                }

                foreach (TreeViewExplorer testSuite in newTestSuiteList)
                {
                    foreach (TreeViewExplorer testPlan in testSuite.Children)
                    {
                        testPlan.DesignNameViewIsEnabled = true;
                    }
                }

                for (int i = 0; i < newTestSuiteList.Count && i < treeViewExplorerExecutionRootItem.Children.Count; i++)
                {
                    TreeViewExplorer testSuite = newTestSuiteList[i];
                    TreeViewExplorer oldTestSuite = treeViewExplorerExecutionRootItem.Children[i];

                    if (testSuite.ItemKey == oldTestSuite.ItemKey)
                    {
                        testSuite.IsChecked = oldTestSuite.IsChecked;
                        testSuite.IsExpanded = oldTestSuite.IsExpanded;
                        if (testSuite.Children != null && oldTestSuite.Children != null)
                        {
                            for (int j = 0; j < testSuite.Children.Count && j < oldTestSuite.Children.Count; j++)
                            {
                                TreeViewExplorer testPlan = testSuite.Children[j];
                                TreeViewExplorer oldTestPlan = oldTestSuite.Children[j];

                                if (testPlan.ItemKey == oldTestPlan.ItemKey)
                                {
                                    testPlan.IsChecked = oldTestPlan.IsChecked;
                                    testPlan.IsExpanded = oldTestPlan.IsExpanded;

                                    if (testPlan.Children != null && oldTestPlan.Children != null)
                                    {
                                        for (int k = 0; k < testPlan.Children.Count && k < oldTestPlan.Children.Count; k++)
                                        {
                                            TreeViewExplorer testCase = testPlan.Children[k];
                                            TreeViewExplorer oldTestCase = oldTestPlan.Children[k];

                                            if (testCase.ItemKey == oldTestCase.ItemKey)
                                            {
                                                testCase.IsChecked = oldTestCase.IsChecked;
                                                testCase.IsExpanded = oldTestCase.IsExpanded;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                if (updateDataContextExecution)
                {
                    treeViewExplorerExecutionRootItem.ClearChildren();
                    treeViewExplorerExecutionRootItem.AddChildrenList_withCheckbox(newTestSuiteList);

                    if (treeViewExplorerExecutionRootItem.Children.Count == 0)
                        treeViewExplorerExecutionRootItem.IsCheckedVisibility = Visibility.Hidden;
                    else
                        treeViewExplorerExecutionRootItem.IsCheckedVisibility = Visibility.Visible;

                    TreeViewExecution.DataContext = null;
                    TreeViewExecution.DataContext = treeViewExplorerExecutionRootList;

                }

                if (updateDataContextInventory)
                {
                    TreeViewInventory.DataContext = null;
                    TreeViewInventory.DataContext = treeViewExplorerInventoryList;
                }
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                //MessageBox.Show("Exception\n " + ex.Message, "Error Code - EC16013", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ZoomIn_Click(object sender, RoutedEventArgs e)
        {
            RunnerSlider.Value += 0.1;
        }

        private void ZoomOut_Click(object sender, RoutedEventArgs e)
        {
            RunnerSlider.Value -= 0.1;
        }

        private void ActualSize_Click(object sender, RoutedEventArgs e)
        {
            RunnerSlider.Value = 1.0;
        }

        private void TestAutomationToolWindow_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if ((Keyboard.IsKeyDown(Key.LeftShift)) || (Keyboard.IsKeyDown(Key.RightShift)))
            {
                if (e.Delta > 0)
                {
                    RunnerSlider.Value += 0.1;
                }
                else if (e.Delta < 0)
                {
                    RunnerSlider.Value -= 0.1;
                }
            }
        }

        private void TestAutomationToolWindow_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if ((Keyboard.IsKeyDown(Key.LeftShift)) || (Keyboard.IsKeyDown(Key.RightShift)))
            {
                if(e.Key == Key.Add)
                {
                    RunnerSlider.Value += 0.1;
                }
                else if(e.Key == Key.Subtract)
                {
                    RunnerSlider.Value -= 0.1;
                }
            }

            if (e.Key == Key.System)
            {
                if (e.SystemKey == Key.F)
                {
                    FileMenu.IsSubmenuOpen = true;
                    Keyboard.Focus(FileMenu);
                }
                else if (e.SystemKey == Key.E)
                {
                    EditMenu.IsSubmenuOpen = true;
                    Keyboard.Focus(EditMenu);
                }
                else if (e.SystemKey == Key.V)
                {
                    ViewMenu.IsSubmenuOpen = true;
                    Keyboard.Focus(ViewMenu);
                }
                else if (e.SystemKey == Key.H)
                {
                    HelpMenu.IsSubmenuOpen = true;
                    Keyboard.Focus(HelpMenu);
                }
                else if (e.SystemKey == Key.P)
                {
                    OpenPreferences();
                }
            }
            else if ((Keyboard.IsKeyDown(Key.LeftCtrl)) || (Keyboard.IsKeyDown(Key.RightCtrl)))
            {
                TreeViewExplorer sourceTreeViewExplorer = null;

                if (selectedItemsInventory.Count == 1)
                    sourceTreeViewExplorer = TreeViewInventory.SelectedItem as TreeViewExplorer;

                if (e.Key == Key.O && sourceTreeViewExplorer != null)
                {
                    DeviceDiscovery.CreateDesignerWindow(false, sourceTreeViewExplorer);
                }
            }
            else if (e.Key == Key.Enter && btn_Execute.IsEnabled == true)
            {
                if (!InProcess)
                {
                    ServerDetails check = new ServerDetails();
                    if (File.Exists(check.txtDesignVersion.Text))
                    {
                        ////if (!DeviceDiscovery.CheckRunningThreadStatus())
                        ////    return;
                        //  DeviceDiscovery.AddRunningThreadStatus(executionThread.ManagedThreadId, executionThread.ThreadState, "Execution");

                        bool Export_Running = DeviceDiscovery.IsExportRunning();
                        bool Import_Running = DeviceDiscovery.IsImportRunning();

                        if (!Export_Running)
                        {
                            MessageBox.Show("Please wait for Export process to finish & start execution ", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }
                        if (!Import_Running)
                        {
                            MessageBox.Show("Please wait for Import process to finish & start execution", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }

                        StartExecution();

                    }
                    else
                    {
                        MessageBox.Show("The Q-SYS designer does not exist in the designer path", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                }
            }
            else if (e.Key == Key.Escape && btn_Cancel.IsEnabled == true)
            {
                btn_Cancel_Click(null, null);
            }
        }

        private void TreeViewInventory_GiveFeedback(object sender, GiveFeedbackEventArgs e)
        {

            Win32Point w32Mouse = new Win32Point();
            GetCursorPos(ref w32Mouse);

            _dragdropWindow.Left = w32Mouse.X;
            _dragdropWindow.Top = w32Mouse.Y;
        }

        private void TreeViewExecution_GiveFeedback(object sender, GiveFeedbackEventArgs e)
        {

            Win32Point w32Mouse = new Win32Point();
            GetCursorPos(ref w32Mouse);

            _dragdropWindow.Left = w32Mouse.X;
            _dragdropWindow.Top = w32Mouse.Y;
        }

        private void TreeViewInventory_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            FrameworkElement sourceElement = e.OriginalSource as FrameworkElement;

            if (e.OriginalSource as Border != null)
            {
                if ((e.OriginalSource as Border).Child as System.Windows.Shapes.Path != null)
                    return;
            }
            else if (e.OriginalSource as System.Windows.Shapes.Path != null)
            {
                return;
            }


            TreeViewExplorer sourceTreeViewExplorer = sourceElement.DataContext as TreeViewExplorer;
            if (sourceTreeViewExplorer == null)
                return;

            string query = string.Empty;

            if (sourceTreeViewExplorer.ItemType == "TestSuite")
            {
                query = "Select * from " + QatConstants.DbTestSuiteTable + " where " + QatConstants.DbTestSuiteIDColumn + " = " + sourceTreeViewExplorer.ItemKey;

            }
            else if (sourceTreeViewExplorer.ItemType == "TestPlan")
            {
                query = "Select * from " + QatConstants.DbTestPlanTable + " where TestPlanID = " + sourceTreeViewExplorer.ItemKey;
            }
            else if (sourceTreeViewExplorer.ItemType == "TestCase")
            {
                query = "Select * from " + QatConstants.DbTestCaseTable + " where TestcaseID = " + sourceTreeViewExplorer.ItemKey;
            }

            DBConnection connection = new DBConnection();
            connection.OpenConnection();
            System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand(query, connection.CreateConnection());
            System.Data.SqlClient.SqlDataAdapter adapt = new System.Data.SqlClient.SqlDataAdapter(cmd);
            DataTable tbl = new DataTable();
            adapt.Fill(tbl);

            connection.CloseConnection();

            if (tbl.Rows.Count > 0)
            {
                DeviceDiscovery.CreateDesignerWindow(false,sourceTreeViewExplorer);
            }
            else
            {
                MessageBox.Show(sourceTreeViewExplorer.ItemName + " is not available in Designer window please refresh runner window", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }            
        }

        private void Descending_Click_Name(object sender, RoutedEventArgs e)
        {
            try
            {
                //List<TreeViewExplorer> treeViewExplorerSearchList = new List<TreeViewExplorer>();

                //if (TreeViewExecution.IsEnabled)

                //if(treeViewExplorerExecutionRootItem.IsEnabled)
                //    SetupTreeViewrFromDB(false, true, "Descending");
                //else
                //    SetupTreeViewrFromDB(false, false, "Descending");

                selectedItemsInventory.Clear();
                selectedItemsExecution.Clear();
                foreach (TreeViewExplorer testSuite in treeViewExplorerExecutionRootItem.Children)
                {
                    
                        testSuite.IsSelected = false;
                    
                        testSuite.IsMultiSelectOn = false;

                }

                // if search text present refresh the search
                if (isInventorySearchListSelected == true)
                {

                   
                    //treeViewExplorerSearchList = sortedTestSuiteList;
                    Sorting_InventoryList(treeViewExplorerSearchList, "Descending",true);
                   
                }
                else
                {
                    Sorting_InventoryList(treeViewExplorerInventoryList, "Descending",true);
                    sortingOrders_OriginalList = sortingOrders;
                    
                }
                MenuAscending.IsChecked = Contextascending.IsChecked = false;
                Menuascendingname.IsChecked = Contextascendingname.IsChecked = false;
                Menuascendingcreatedon.IsChecked = Contextascendingcreatedon.IsChecked = false;


                MenuDecending.IsChecked = Contextdescending.IsChecked = true;
                Menudecendingname.IsChecked = Contextdescendingname.IsChecked = true;
                Menudecendingcreatedon.IsChecked = Contextdescendingcreatedon.IsChecked = false;
                //ExecutionLoop EL = Application.Current.Windows.OfType<ExecutionLoop>().FirstOrDefault() ?? new ExecutionLoop(treeViewExplorerExecutionRootItem);
                //EL.DataContext = null;
                //EL.DataContext = treeViewExplorerExecutionRootItem.Children;
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                //MessageBox.Show("Exception\n " + ex.Message, "Error Code - EC16012D", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void Descending_Click_CreatedOn(object sender, RoutedEventArgs e)
        {
            try
            {
                //List<TreeViewExplorer> treeViewExplorerSearchList = new List<TreeViewExplorer>();

                //if (TreeViewExecution.IsEnabled)

                //if(treeViewExplorerExecutionRootItem.IsEnabled)
                //    SetupTreeViewrFromDB(false, true, "Descending");
                //else
                //    SetupTreeViewrFromDB(false, false, "Descending");

                selectedItemsInventory.Clear();
                selectedItemsExecution.Clear();
                foreach (TreeViewExplorer testSuite in treeViewExplorerExecutionRootItem.Children)
                {
                   
                        testSuite.IsSelected = false;
                  
                        testSuite.IsMultiSelectOn = false;

                }

                // if search text present refresh the search
                if (isInventorySearchListSelected == true)
                {
                    Sorting_InventoryList(treeViewExplorerSearchList, "Date Created Descending",true);
                
                }
                else
                {
                    Sorting_InventoryList(treeViewExplorerInventoryList, "Date Created Descending",true);
                    sortingOrders_OriginalList = sortingOrders;
                }
                MenuAscending.IsChecked = Contextascending.IsChecked =false;
                Menuascendingname.IsChecked = Contextascendingname.IsChecked = false;
                Menuascendingcreatedon.IsChecked = Contextascendingcreatedon.IsChecked = false;


                MenuDecending.IsChecked = Contextdescending.IsChecked = true;
                Menudecendingname.IsChecked = Contextdescendingname.IsChecked = false;
                Menudecendingcreatedon.IsChecked = Contextdescendingcreatedon.IsChecked =true;
                //ExecutionLoop EL = Application.Current.Windows.OfType<ExecutionLoop>().FirstOrDefault() ?? new ExecutionLoop(treeViewExplorerExecutionRootItem);
                //EL.DataContext = null;
                //EL.DataContext = treeViewExplorerExecutionRootItem.Children;
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                //MessageBox.Show("Exception\n " + ex.Message, "Error Code - EC16012D", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void Sorting_InventoryList(List<TreeViewExplorer> treeviewList, string sortType,bool updatedatacontext)
        {
            try
            {


                List<TreeViewExplorer> sortedTestSuiteList = new List<TreeViewExplorer>();

                if (sortType == "Ascending")
                {
                    sortingOrders = sortType;
                    TreeViewExplorer[] alphaTestSuiteSorted = treeviewList.ToArray();
                    Array.Sort(alphaTestSuiteSorted, new AlphanumComparatorFast());
                    sortedTestSuiteList = alphaTestSuiteSorted.ToList();
                }
                else if (sortType == "Descending")
                {
                    sortingOrders = sortType;
                    TreeViewExplorer[] alphaTestSuiteSorted = treeviewList.ToArray();
                    Array.Sort(alphaTestSuiteSorted, new AlphanumComparatorFast());
                    Array.Reverse(alphaTestSuiteSorted);
                    sortedTestSuiteList = alphaTestSuiteSorted.ToList();
                }
                else if (sortType == "Date Created Ascending")
                {
                    sortingOrders = sortType;
                    TreeViewExplorer[] alphaTestSuiteSorted = treeviewList.ToArray();
                    Array.Sort(alphaTestSuiteSorted, new AlphanumComparatorFast_DateTime());
                    sortedTestSuiteList = alphaTestSuiteSorted.ToList();
                }
                else if (sortType == "Date Created Descending")
                {
                    sortingOrders = sortType;
                    TreeViewExplorer[] alphaTestSuiteSorted = treeviewList.ToArray();
                    Array.Sort(alphaTestSuiteSorted, new AlphanumComparatorFast_DateTime());
                    Array.Reverse(alphaTestSuiteSorted);
                    sortedTestSuiteList = alphaTestSuiteSorted.ToList();
                }


                treeviewList.Clear();
                //treeViewExplorerSearchList = sortedTestSuiteList;
                foreach (TreeViewExplorer testSuites in sortedTestSuiteList)
                {

                    testSuites.IsSelected = false;

                    testSuites.IsMultiSelectOn = false;

                    treeviewList.Add(testSuites);
                }

                if(updatedatacontext)
                {
                TreeViewInventory.DataContext = null;
                TreeViewInventory.DataContext = treeviewList;
                }


            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                ///MessageBox.Show("Exception\n " + ex.Message, "Error Code - EC16012A", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void Ascending_Click_Name(object sender, RoutedEventArgs e)
        {
            try
            {
                //List<TreeViewExplorer> treeViewExplorerSearchList = new List<TreeViewExplorer>();

                //if (TreeViewExecution.IsEnabled)

                //if(treeViewExplorerExecutionRootItem.IsEnabled)
                //    SetupTreeViewrFromDB(false, true, "Ascending");
                //else
                //    SetupTreeViewrFromDB(false, false, "Ascending");

                selectedItemsInventory.Clear();
                selectedItemsExecution.Clear();
                foreach (TreeViewExplorer testSuite in treeViewExplorerExecutionRootItem.Children)
                {
                    
                        testSuite.IsSelected = false;
                   
                        testSuite.IsMultiSelectOn = false;

                }

                // if search text present refresh the search
                if (isInventorySearchListSelected == true)
                {
                    Sorting_InventoryList(treeViewExplorerSearchList, "Ascending",true);
                   
                }
                else
                {
                    Sorting_InventoryList(treeViewExplorerInventoryList, "Ascending",true);
                    sortingOrders_OriginalList = sortingOrders;
                  
                }

                MenuAscending.IsChecked = Contextascending.IsChecked = true;
                Menuascendingname.IsChecked = Contextascendingname.IsChecked = true;
                Menuascendingcreatedon.IsChecked = Contextascendingcreatedon.IsChecked =false;


                MenuDecending.IsChecked = Contextdescending.IsChecked = false;
                Menudecendingname.IsChecked = Contextdescendingname.IsChecked = false;
                Menudecendingcreatedon.IsChecked = Contextdescendingcreatedon.IsChecked = false;
                //ExecutionLoop EL = Application.Current.Windows.OfType<ExecutionLoop>().FirstOrDefault() ?? new ExecutionLoop(treeViewExplorerExecutionRootItem);
                //EL.DataContext = null;
                //EL.DataContext = treeViewExplorerExecutionRootItem.Children;
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                ///MessageBox.Show("Exception\n " + ex.Message, "Error Code - EC16012A", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void Ascending_Click_CreatedOn(object sender, RoutedEventArgs e)
        {
            try
            {
                //List<TreeViewExplorer> treeViewExplorerSearchList = new List<TreeViewExplorer>();

                //if (TreeViewExecution.IsEnabled)

                //if(treeViewExplorerExecutionRootItem.IsEnabled)
                //    SetupTreeViewrFromDB(false, true, "Ascending");
                //else
                //    SetupTreeViewrFromDB(false, false, "Ascending");

                selectedItemsInventory.Clear();
                selectedItemsExecution.Clear();

                foreach(TreeViewExplorer testSuite in treeViewExplorerExecutionRootItem.Children)
                {
                    
                        testSuite.IsSelected = false;
                   
                        testSuite.IsMultiSelectOn = false;

                }
                // if search text present refresh the search
                if (isInventorySearchListSelected == true)
                {

                    Sorting_InventoryList(treeViewExplorerSearchList, "Date Created Ascending",true);
                  
                }
                else
                {
                    Sorting_InventoryList(treeViewExplorerInventoryList, "Date Created Ascending",true);
                    sortingOrders_OriginalList = sortingOrders;
                    
                }
                MenuAscending.IsChecked = Contextascending.IsChecked = true;
                Menuascendingname.IsChecked = Contextascendingname.IsChecked = false;
                Menuascendingcreatedon.IsChecked = Contextascendingcreatedon.IsChecked = true;


                MenuDecending.IsChecked = Contextdescending.IsChecked = false;
                Menudecendingname.IsChecked = Contextdescendingname.IsChecked = false;
                Menudecendingcreatedon.IsChecked = Contextdescendingcreatedon.IsChecked = false;
                //ExecutionLoop EL = Application.Current.Windows.OfType<ExecutionLoop>().FirstOrDefault() ?? new ExecutionLoop(treeViewExplorerExecutionRootItem);
                //EL.DataContext = null;
                //EL.DataContext = treeViewExplorerExecutionRootItem.Children;
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                ///MessageBox.Show("Exception\n " + ex.Message, "Error Code - EC16012A", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void EditMenu_SubmenuOpened(object sender, RoutedEventArgs e)
        {
            try
            {
                if (selectedItemsInventory.Count ==1)
                    MenuView1.IsEnabled = true;
                else
                    MenuView1.IsEnabled = false;

            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                // MessageBox.Show("Exception\n " + ex.Message, "Error Code - ECxxxxx", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OpenMenu1Item_Click(object sender, RoutedEventArgs e)
        {
            TreeViewExplorer sourceTreeViewExplorer = null;

            if (selectedItemsInventory.Count > 0)
                sourceTreeViewExplorer = TreeViewInventory.SelectedItem as TreeViewExplorer;

            if (sourceTreeViewExplorer == null)
                return;

            DeviceDiscovery.CreateDesignerWindow(false,sourceTreeViewExplorer);
        }

        private void MenuItemView_Click(object sender, RoutedEventArgs e)
        {
            TreeViewExplorer sourceTreeViewExplorer = null;

            if (selectedItemsInventory.Count > 0)
                sourceTreeViewExplorer = TreeViewInventory.SelectedItem as TreeViewExplorer;

            if (sourceTreeViewExplorer == null)
                return;

            DeviceDiscovery.CreateDesignerWindow(false,sourceTreeViewExplorer);
        }

        private void TreeViewInventory_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            FrameworkElement sourceElement = e.OriginalSource as FrameworkElement;

            TreeViewExplorer sourceTreeViewExplorer = sourceElement.DataContext as TreeViewExplorer;
            if (sourceTreeViewExplorer == null)
                InventoryView.IsEnabled = false;
            else
                InventoryView.IsEnabled = true;

            if (selectedItemsInventory.Count == 1)
                InventoryView.IsEnabled = true;            
            else            
                InventoryView.IsEnabled = false;
            

            try
            {
                Expand_all.IsEnabled = false;
                ////check to enable expandall
                if (isInventorySearchListSelected == false)
                {
                    foreach (TreeViewExplorer childsuite in treeViewExplorerInventoryList)
                    {
                        if (childsuite.IsExpanded == false)
                        {
                            Expand_all.IsEnabled = true;
                      
                            break;
                        }

                        foreach (TreeViewExplorer childplan in childsuite.Children)
                        {
                            if (childplan.IsExpanded == false)
                            {
                                Expand_all.IsEnabled = true;
                                break;
                            }

                        }
                    }
                }
                else
                {
                    foreach (TreeViewExplorer childsuite in treeViewExplorerSearchList)
                    {
                        if (childsuite.IsExpanded == false)
                        {
                            Expand_all.IsEnabled = true;

                            break;
                        }
                        foreach (TreeViewExplorer childplan in childsuite.Children)
                        {
                            if (childplan.IsExpanded == false)
                            {
                                Expand_all.IsEnabled = true;
                                break;
                            }

                        }
                    }
                }
                collapse_all.IsEnabled = false;
                ////check to enable collapseall
                if (isInventorySearchListSelected == false)
                {
                    foreach (TreeViewExplorer childsuite in treeViewExplorerInventoryList)
                    {
                        if (childsuite.IsExpanded == true)
                        {
                            collapse_all.IsEnabled = true;
                            break;
                        }


                        foreach (TreeViewExplorer childplan in childsuite.Children)
                        {
                            if (childplan.IsExpanded == true)
                            {
                                collapse_all.IsEnabled = true;
                                break;
                            }

                        }
                    }
                }
                else
                {
                    foreach (TreeViewExplorer childsuite in treeViewExplorerSearchList)
                    {
                        if (childsuite.IsExpanded == true)
                        {
                            collapse_all.IsEnabled = true;
                            break;
                        }
                        foreach (TreeViewExplorer childplan in childsuite.Children)
                        {
                            if (childplan.IsExpanded == true)
                            {
                                collapse_all.IsEnabled = true;
                                break;
                            }

                        }
                    }
                }
            }

            catch (Exception ex)
            {

            }



        }

        private void FileMenu_SubmenuOpened(object sender, RoutedEventArgs e)
        {
            try
            {
                //if (TreeViewExecution.IsEnabled)

                if(treeViewExplorerExecutionRootItem.IsEnabled)
                {
                    if (treeViewExplorerExecutionRootItem == null || treeViewExplorerExecutionRootItem.Children.Count == 0)
                        Menu_ExportConfigFile.IsEnabled = false;
                    else
                        Menu_ExportConfigFile.IsEnabled = true;
                }
                else
                {
                    Menu_ExportConfigFile.IsEnabled = false;
                    Menu_ImportConfigFile.IsEnabled = false;
                }
                if (btn_Execute.IsEnabled == true && (treeViewExplorerExecutionRootItem != null || treeViewExplorerExecutionRootItem.Children.Count != 0))
                {
                    Menu_ImportConfigFile.IsEnabled = true;
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

        private void Menu_ExportConfigFile_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Filter = "QAT Config File|*.xml";
            saveFileDialog1.Title = "Save QAT Config File";
            saveFileDialog1.ShowDialog();

            // If the file name is not an empty string open it for saving.
            if (saveFileDialog1.FileName != "")
                CreateConfigFile(saveFileDialog1.FileName);
        }

        private void Menu_ImportConfigFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "QAT Config Files (*.xml)|*.xml";
            if (openFileDialog.ShowDialog() == true)
            {
                ImportConfigFile(openFileDialog.FileName);
            }
        }

        private void CreateConfigFile(string configFileName)
        {
            try
            {
                if (treeViewExplorerExecutionRootItem == null || treeViewExplorerExecutionRootItem.Children.Count == 0)
                    return;


                XmlWriterSettings configFileSettings = new XmlWriterSettings
                {
                    Indent = true,
                    IndentChars = "  ",
                    NewLineChars = "\r\n",
                    NewLineHandling = NewLineHandling.Replace
                };

                using (XmlWriter configFile = XmlWriter.Create(configFileName, configFileSettings))
                {
                    configFile.WriteStartElement("QatConfigFile");

                    foreach (var testSuite in treeViewExplorerExecutionRootItem.Children)
                    {
                        configFile.WriteStartElement("TestSuite");

                        configFile.WriteAttributeString("Name", testSuite.ItemName);
                        configFile.WriteAttributeString("Key", testSuite.ItemKey.ToString());
                        configFile.WriteAttributeString("IsChecked", testSuite.IsChecked.ToString());
                        configFile.WriteAttributeString("IsExpanded", testSuite.IsExpanded.ToString());

                        foreach (var testPlan in testSuite.Children)
                        {
                            configFile.WriteStartElement("TestPlan");

                            configFile.WriteAttributeString("Name", testPlan.ItemName);
                            configFile.WriteAttributeString("Key", testPlan.ItemKey.ToString());
                            configFile.WriteAttributeString("IsChecked", testPlan.IsChecked.ToString());
                            configFile.WriteAttributeString("IsExpanded", testPlan.IsExpanded.ToString());

                            foreach (var testCase in testPlan.Children)
                            {
                                configFile.WriteStartElement("TestCase");

                                configFile.WriteAttributeString("Name", testCase.ItemName);
                                configFile.WriteAttributeString("Key", testCase.ItemKey.ToString());
                                configFile.WriteAttributeString("IsChecked", testCase.IsChecked.ToString());
                                configFile.WriteAttributeString("IsExpanded", testCase.IsExpanded.ToString());

                                configFile.WriteEndElement();
                            }

                            configFile.WriteEndElement();
                        }

                        configFile.WriteEndElement();
                    }

                    foreach (DUT_DeviceItem item in selectedDutDeviceItemList)
                    {
                        configFile.WriteStartElement("DutItem");

                        configFile.WriteAttributeString("DeviceNameInDesign", item.ItemDeviceName);
                        configFile.WriteAttributeString("DeviceModel", item.ItemDeviceModel);
                        configFile.WriteAttributeString("SelectedDevice", item.ItemNetPairingSelected);
                        configFile.WriteAttributeString("RestoreDesign", item.CoreRestoreDesign.ToString());
                        configFile.WriteAttributeString("ClearLogs", item.ClearLogs.ToString());
                        configFile.WriteAttributeString("BypassStatusCheck", item.Bypass.ToString());
                        configFile.WriteEndElement();
                    }
                    foreach (DUT_DeviceItem item in selectedExternalDeviceItemList)
                    {
                        configFile.WriteStartElement("ExternalVideoDutItem");

                        configFile.WriteAttributeString("VideoGen", item.VideoGen);
                        configFile.WriteAttributeString("GenModel", item.GenModel);
                        configFile.WriteAttributeString("Gen_IP_address", item.Gen_IP_address);
                 
                        configFile.WriteEndElement();
                    }
                    foreach (GetExecutionLoop item in treeViewExplorerExecutionRootItem.DataGridCollection)
                    {
                        configFile.WriteStartElement("ExecutionLoop");

                        configFile.WriteAttributeString("TestSuite", item.TestSuiteName);
                        configFile.WriteAttributeString("TypeOfLoop", item.TypeOfLoopOption);

                        if(item.NumOfLoop!=null)
                             configFile.WriteAttributeString("NumberOfLoops", item.NumOfLoop.ToString());
                        else
                             configFile.WriteAttributeString("NumberOfLoops",string.Empty);

                        configFile.WriteAttributeString("DurationValue", item.txtDurCmb.ToString());
                        configFile.WriteAttributeString("DurationType", item.txtDurCmbSelectedValue);
                        configFile.WriteAttributeString("RedeployDesign", item.blnRedeployedDesign.ToString());

                        configFile.WriteEndElement();
                    }

                    configFile.WriteStartElement("Emaildetails");
                    configFile.WriteAttributeString("Emailoption", "AfterExecutionCompletion");
                    configFile.WriteAttributeString("IsChecked", Properties.Settings.Default.AfterExecutionCompletion.ToString());
                    configFile.WriteAttributeString("Mailid", Properties.Settings.Default.ReportEmailID);
                    configFile.WriteAttributeString("EmailSubject", Properties.Settings.Default.ReportEmailSubject);
                    configFile.WriteEndElement();

                    configFile.WriteStartElement("Emaildetails");
                    configFile.WriteAttributeString("Emailoption", "AfterTestCaseFails");
                    configFile.WriteAttributeString("IsChecked", Properties.Settings.Default.AfterTestCaseFails.ToString());

                    configFile.WriteAttributeString("Failcount", Properties.Settings.Default.TestCaseFailsCount.ToString());
                    configFile.WriteAttributeString("Mailid", Properties.Settings.Default.ReportEmailID);
                    configFile.WriteAttributeString("EmailSubject", Properties.Settings.Default.ReportEmailSubject);
                    configFile.WriteEndElement();

                    configFile.WriteStartElement("Emaildetails");
                    configFile.WriteAttributeString("Emailoption", "ApplicationCrashes");
                    configFile.WriteAttributeString("IsChecked", Properties.Settings.Default.ApplicationCrashes.ToString());

                    configFile.WriteAttributeString("Mailid", Properties.Settings.Default.ReportEmailID);
                    configFile.WriteAttributeString("EmailSubject", Properties.Settings.Default.ReportEmailSubject);
                    configFile.WriteEndElement();

                    configFile.WriteStartElement("Emaildetails");
                    configFile.WriteAttributeString("Emailoption", "AutomationPauses");
                    configFile.WriteAttributeString("IsChecked", Properties.Settings.Default.AutomationPauses.ToString());
                    configFile.WriteAttributeString("Mailid", Properties.Settings.Default.ReportEmailID);
                    configFile.WriteAttributeString("EmailSubject", Properties.Settings.Default.ReportEmailSubject);
                    configFile.WriteEndElement();

                    configFile.WriteStartElement("Emaildetails");
                    configFile.WriteAttributeString("Emailoption", "AddreportLink");
                    configFile.WriteAttributeString("IsChecked", Properties.Settings.Default.AddreportLink.ToString());
                    configFile.WriteAttributeString("Mailid", Properties.Settings.Default.ReportEmailID);
                    configFile.WriteAttributeString("EmailSubject", Properties.Settings.Default.ReportEmailSubject);
                    configFile.WriteEndElement();


                    if (treeViewExplorerExecutionRootItem.ExecutionDelay != null && treeViewExplorerExecutionRootItem.ExecutionDelay != string.Empty)
                    {
                        configFile.WriteStartElement("Delay");
                        configFile.WriteAttributeString("ExecutionDelay", treeViewExplorerExecutionRootItem.ExecutionDelay);
                        configFile.WriteAttributeString("ExecutionDelayTime", treeViewExplorerExecutionRootItem.ExecutionDelayTime);
                        configFile.WriteEndElement();
                    }
                    configFile.WriteEndElement();
                    configFile.WriteEndDocument();
                    configFile.Close();
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

        private void ImportConfigFile(string configFileName)
        {
            try
            {

                XmlDocument configFile = new XmlDocument();
                configFile.Load(configFileName);

                List<TreeViewExplorer> testSuiteList = new List<TreeViewExplorer>();

                foreach (XmlNode xmlNode in configFile.DocumentElement.ChildNodes)
                {
                    if (xmlNode.Name == "TestSuite")
                    {
                        string testSuiteName = xmlNode.Attributes["Name"].Value;
                        string testSuiteKey = xmlNode.Attributes["Key"].Value;
                        string testSuiteIsChecked = xmlNode.Attributes["IsChecked"].Value;
                        string testSuiteIsExpanded = xmlNode.Attributes["IsExpanded"].Value;

                        TreeViewExplorer testSuite = new TreeViewExplorer(Convert.ToInt32(testSuiteKey), testSuiteName, QatConstants.DbTestSuiteTable, this, null);
                        testSuite.IsExpanded = Convert.ToBoolean(testSuiteIsExpanded);
                        if(testSuiteIsChecked!=string.Empty)
                          testSuite.IsChecked = Convert.ToBoolean(testSuiteIsChecked);
                        testSuiteList.Add(testSuite);

                        foreach (XmlNode xmlNodeTestPlan in xmlNode.ChildNodes)
                        {
                            string testPlanName = xmlNodeTestPlan.Attributes["Name"].Value;
                            string testPlanKey = xmlNodeTestPlan.Attributes["Key"].Value;
                            string testPlanIsChecked = xmlNodeTestPlan.Attributes["IsChecked"].Value;
                            string testPlanIsExpanded = xmlNodeTestPlan.Attributes["IsExpanded"].Value;

                            TreeViewExplorer testPlan = new TreeViewExplorer(Convert.ToInt32(testPlanKey), testPlanName, QatConstants.DbTestSuiteTable, this, null);
                            testPlan.IsExpanded = Convert.ToBoolean(testPlanIsExpanded);
                            if(testPlanIsChecked!=string.Empty)
                              testPlan.IsChecked = Convert.ToBoolean(testPlanIsChecked);
                            testSuite.AddChildren_withCheckbox(testPlan);

                            foreach (XmlNode xmlNodeTestCase in xmlNodeTestPlan.ChildNodes)
                            {
                                string testCaseName = xmlNodeTestCase.Attributes["Name"].Value;
                                string testCaseKey = xmlNodeTestCase.Attributes["Key"].Value;
                                string testCaseIsChecked = xmlNodeTestCase.Attributes["IsChecked"].Value;
                                string testCaseIsExpanded = xmlNodeTestCase.Attributes["IsExpanded"].Value;

                                TreeViewExplorer testCase = new TreeViewExplorer(Convert.ToInt32(testCaseKey), testCaseName, QatConstants.DbTestSuiteTable, this, null);
                                testCase.IsExpanded = Convert.ToBoolean(testCaseIsExpanded);
                                testCase.IsChecked = Convert.ToBoolean(testCaseIsChecked);
                                testPlan.AddChildren_withCheckbox(testCase);
                            }
                        }
                    }
                }

                if(testSuiteList.Count==0)
                {
                    MessageBox.Show("Please select QAT config xml file to import.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                treeViewExplorerExecutionRootItem.ClearChildren();
                treeViewExplorerExecutionRootItem.AddChildrenList_withCheckbox(testSuiteList);

                if (treeViewExplorerExecutionRootItem.Children.Count == 0)
                    treeViewExplorerExecutionRootItem.IsCheckedVisibility = Visibility.Hidden;
                else
                    treeViewExplorerExecutionRootItem.IsCheckedVisibility = Visibility.Visible;

                TreeViewExecution.DataContext = null;
                TreeViewExecution.DataContext = treeViewExplorerExecutionRootList;
                RefreshRunner();

                if (treeViewExplorerExecutionRootItem.Children.Count == 0)
                {
                    treeViewExplorerExecutionRootItem.IsCheckedVisibility = Visibility.Hidden;
                    chk_ConfigurationFile.IsEnabled = false;
                    btn_loop.IsEnabled = false;
                    btn_emailReport.IsEnabled = false;
                    btn_Execute.IsEnabled = false;
                    Btn_Execution_Delay.IsEnabled = false;
                }
                else
                {
                    treeViewExplorerExecutionRootItem.IsCheckedVisibility = Visibility.Visible;
                    chk_ConfigurationFile.IsEnabled = true;
                    btn_loop.IsEnabled = true;
                    btn_emailReport.IsEnabled = true;
                    btn_Execute.IsEnabled = true;
                    Btn_Execution_Delay.IsEnabled = true;
                }


                UpdateNetPairingList(true, true);

                foreach (XmlNode xmlNode in configFile.DocumentElement.ChildNodes)
                {
                    if (xmlNode.Name == "DutItem")
                    {
                        string BypassStatusCheck = "False";
                        string deviceNameInDesign = xmlNode.Attributes["DeviceNameInDesign"].Value;
                        string deviceModel = xmlNode.Attributes["DeviceModel"].Value;
                        string selectedDevice = xmlNode.Attributes["SelectedDevice"].Value;
                        string restoreDesign = xmlNode.Attributes["RestoreDesign"].Value;
                        string clearLogs = xmlNode.Attributes["ClearLogs"].Value;
                        if (xmlNode.Attributes["BypassStatusCheck"] != null)
                            BypassStatusCheck= xmlNode.Attributes["BypassStatusCheck"].Value;

                        DUT_DeviceItem dutItem = selectedDutDeviceItemList.Find(x => x.ItemDeviceName.Equals(deviceNameInDesign, StringComparison.CurrentCultureIgnoreCase) && x.ItemDeviceModel.Equals(deviceModel, StringComparison.CurrentCultureIgnoreCase));
                        if (dutItem != null)
                        {
                            dutItem.ItemNetPairingSelected = selectedDevice;
                            dutItem.CoreRestoreDesign = Convert.ToBoolean(restoreDesign);
                            dutItem.ClearLogs = Convert.ToBoolean(clearLogs);
                            dutItem.Bypass = Convert.ToBoolean(BypassStatusCheck);
                        }
                    }
                }
                foreach (XmlNode xmlNode in configFile.DocumentElement.ChildNodes)
                {
                    if (xmlNode.Name == "ExternalVideoDutItem")
                    {
                        string VideoGen = xmlNode.Attributes["VideoGen"].Value;
                        string GenModel = xmlNode.Attributes["GenModel"].Value;
                        string Gen_IP_address = xmlNode.Attributes["Gen_IP_address"].Value;
                 

                        DUT_DeviceItem ExternaldutItem = selectedExternalDeviceItemList.Find(x => x.VideoGen.Equals(VideoGen, StringComparison.CurrentCultureIgnoreCase) && x.GenModel.Equals(GenModel, StringComparison.CurrentCultureIgnoreCase));
                        if (ExternaldutItem != null)
                        {
                            ExternaldutItem.Gen_IP_address = Gen_IP_address;
                          
                        }
                        else
                        {
                            DUT_DeviceItem createExternaldutItem = new DUT_DeviceItem();
                            createExternaldutItem.VideoGen = VideoGen;
                            createExternaldutItem.GenModel =GenModel;
                            createExternaldutItem.Gen_IP_address = Gen_IP_address;
                            if(!selectedExternalDeviceItemList.Contains(createExternaldutItem))
                                selectedExternalDeviceItemList.Add(createExternaldutItem);


                        }
                    }
                }

                foreach (XmlNode xmlNode in configFile.DocumentElement.ChildNodes)
                {
                    if (xmlNode.Name == "ExecutionLoop")
                    {
                        string testSuiteName = xmlNode.Attributes["TestSuite"].Value;
                        string typeOfLoopOption = xmlNode.Attributes["TypeOfLoop"].Value;
                        string numOfLoop = xmlNode.Attributes["NumberOfLoops"].Value;
                        string durationValue = xmlNode.Attributes["DurationValue"].Value;
                        string durationType = xmlNode.Attributes["DurationType"].Value;
                        string restoreDesign = xmlNode.Attributes["RedeployDesign"].Value;

                        GetExecutionLoop loopItem = null;
                        if (treeViewExplorerExecutionRootItem.DataGridCollection != null)
                            loopItem = treeViewExplorerExecutionRootItem.DataGridCollection.ToList().Find(x => x.TestSuiteName == testSuiteName);

                        if (loopItem == null)
                        {
                            loopItem = new GetExecutionLoop();
                            loopItem.TestSuiteName = testSuiteName;
                            treeViewExplorerExecutionRootItem.DataGridCollection.Add(loopItem);
                        }

                        if (loopItem != null)
                        {
                            loopItem.TypeOfLoopOption = typeOfLoopOption;
                            loopItem.NumOfLoop = numOfLoop;
                            loopItem.txtDurCmb = Convert.ToInt32(durationValue);
                            loopItem.txtDurCmbSelectedValue = durationType;
                            loopItem.blnRedeployedDesign = Convert.ToBoolean(restoreDesign);
                        }
                    }
                }

                foreach (XmlNode xmlNode in configFile.DocumentElement.ChildNodes)
                {
                    string Failcount = string.Empty;
                    string EmailSubject = string.Empty;
                    if (xmlNode.Name == "Emaildetails")
                    {
                        //Email_Report reportdetails = new Email_Report();
                        string Emailtype = xmlNode.Attributes["Emailoption"].Value;

                        if (Emailtype == "AfterTestCaseFails")
                            Failcount = xmlNode.Attributes["Failcount"].Value;

                        string Mailid = xmlNode.Attributes["Mailid"].Value;
                       if(xmlNode.Attributes["EmailSubject"] != null)
                        {
                            EmailSubject= xmlNode.Attributes["EmailSubject"].Value;
                        }

                        if (Emailtype == "AfterTestCaseFails")
                        {
                            //reportdetails.chk_EmailAfterXFail.IsChecked = true;
                            //reportdetails.FailureValue1.Text = Failcount;


                            Properties.Settings.Default.AfterTestCaseFails = Convert.ToBoolean((xmlNode.Attributes["IsChecked"].Value));
                            Properties.Settings.Default.TestCaseFailsCount = Convert.ToInt32(Failcount);
                        }

                        if (Emailtype == "AutomationPauses")
                        {
                            //reportdetails.Chk_EmailIfAutomationPauses.IsChecked = true;

                            Properties.Settings.Default.AutomationPauses = Convert.ToBoolean(xmlNode.Attributes["IsChecked"].Value);
                        }
                        if (Emailtype == "AfterExecutionCompletion")
                        {
                            //reportdetails.Chk_EmailAfterCompletionOfExecution.IsChecked = true;

                            Properties.Settings.Default.AfterExecutionCompletion = Convert.ToBoolean((xmlNode.Attributes["IsChecked"].Value));

                        }
                        if (Emailtype == "ApplicationCrashes")
                        {
                            //reportdetails.Chk_EmailIfQSysDesignerApplicationCrashes.IsChecked = true;


                            Properties.Settings.Default.ApplicationCrashes = Convert.ToBoolean((xmlNode.Attributes["IsChecked"].Value));
                        }
                        if(Emailtype == "AddreportLink")
                        {
                            Properties.Settings.Default.AddreportLink = Convert.ToBoolean((xmlNode.Attributes["IsChecked"].Value));
                        }

                        //reportdetails.MailTo.Text = Mailid.Trim();
                        Properties.Settings.Default.ReportEmailID = Mailid;
                        Properties.Settings.Default.ReportEmailSubject = EmailSubject;

                    }
                }


                foreach (XmlNode xmlNode in configFile.DocumentElement.ChildNodes)
                {
                    if (xmlNode.Name == "Delay")
                    {
                        //Email_Report reportdetails = new Email_Report();
                        treeViewExplorerExecutionRootItem.ExecutionDelay = xmlNode.Attributes["ExecutionDelay"].Value;
                        treeViewExplorerExecutionRootItem.ExecutionDelayTime = xmlNode.Attributes["ExecutionDelayTime"].Value;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Import config file error.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
            }
        }

        //private void Help_Click(object sender, RoutedEventArgs e)
        //{
        //    HelpManual hlp = new HelpManual();
        //    hlp.ShowHelp();
        //}

        //private void TestAutomationToolWindow_KeyDown(object sender, KeyEventArgs e)
        //{
        //    HelpManual hlp = new HelpManual();

        //    if (e.Key.ToString() == Key.F1.ToString())
        //    {
        //        hlp.ShowHelp();
        //    }
        //}

        private void TextBox_PreviewKeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            var textBox = e.OriginalSource as TextBox;

            if (textBox != null && e.Key == Key.Tab)
            {
                textBox.SelectAll();
            }
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo()
                {
                    FileName =QatConstants.HelpVideoPath,
                    UseShellExecute = true,
                    Verb = "open"
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show("Help video path is not accessible");

            }
        }

        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
            try
            {
                DeviceDiscovery.CreateOverviewWindow();
            }
            catch (Exception ex)
            {
                

            }
        }
        private void Export_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DeviceDiscovery.CreateExportWindow();
            }
            catch (Exception ex)
            {

            }
            
        }

        private void Grafana_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                GrafanaGraph grafanaWindow = new GrafanaGraph();
                grafanaWindow.Owner = this;
                grafanaWindow.ShowDialog();
                grafanaWindow.Close();
            }
            catch (Exception ex)
            {
            }
        }
        

        private void Import_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (DeviceDiscovery.IsImportAlreadyExist == false)
                {
                    DeviceDiscovery.CreateImportWindow();
                }
            }
            catch (Exception ex)
            {

            }            
        }
        
        private void MenuItem_Click_2(object sender, RoutedEventArgs e)
        {
            try
            {
                DeviceDiscovery.CreateShortcutWindow();
            }
            catch(Exception ex)
            {

            }
        }

        private void ExpandAll_Click(object sender, RoutedEventArgs e)
        {
            try {

                foreach (TreeViewExplorer testSuite in treeViewExplorerExecutionRootList)
                {
                    foreach (TreeViewExplorer childsuite in testSuite.Children)
                    {
                        childsuite.IsExpanded = true;
                        foreach (TreeViewExplorer childplan in childsuite.Children)
                        {
                            childplan.IsExpanded = true;
                        }
                    }
                }
            }
           
             catch (Exception ex)
            {

            }
        }

        private void CollapseAll_Click(object sender, RoutedEventArgs e)
        {
            foreach (TreeViewExplorer testSuite in treeViewExplorerExecutionRootList)
            {
                foreach (TreeViewExplorer childsuite in testSuite.Children)
                {
                    childsuite.IsExpanded = false;
                    foreach (TreeViewExplorer childplan in childsuite.Children)
                    {
                        childplan.IsExpanded = false;
                    }
                }
            }
        }

        private void Ascending1_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                List<TreeViewExplorer> treeViewExplorerSearchList = new List<TreeViewExplorer>();
                //TreeViewExplorer mainroot= new TreeViewExplorer();
                foreach (TreeViewExplorer testSuite in treeViewExplorerExecutionRootList)
                {
                 
                    TreeViewExplorer[] alphaTestSuiteSorted = testSuite.Children.ToArray();
                   Array.Sort(alphaTestSuiteSorted, new AlphanumComparatorFast());

                    foreach (TreeViewExplorer sample in alphaTestSuiteSorted)
                    {
                        treeViewExplorerSearchList.Add(sample);

                    }
                  
                }
                treeViewExplorerExecutionRootItem.ClearChildren();
                treeViewExplorerExecutionRootItem.AddChildrenList_withCheckbox(treeViewExplorerSearchList);

                TreeViewExecution.DataContext = null;
              
               
                    TreeViewExecution.DataContext = treeViewExplorerExecutionRootList;
            
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                ///MessageBox.Show("Exception\n " + ex.Message, "Error Code - EC16012A", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }

        private void Decending1_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                List<TreeViewExplorer> treeViewExplorerSearchList = new List<TreeViewExplorer>();

                foreach (TreeViewExplorer testSuite in treeViewExplorerExecutionRootList)
                {

                    TreeViewExplorer[] alphaTestSuiteSorted = testSuite.Children.ToArray();
                    Array.Sort(alphaTestSuiteSorted, new AlphanumComparatorFast());
                    Array.Reverse(alphaTestSuiteSorted);
                    foreach (TreeViewExplorer sample in alphaTestSuiteSorted)
                    {
                        treeViewExplorerSearchList.Add(sample);

                    }

                }
                treeViewExplorerExecutionRootItem.ClearChildren();
                treeViewExplorerExecutionRootItem.AddChildrenList_withCheckbox(treeViewExplorerSearchList);

                TreeViewExecution.DataContext = null;
                TreeViewExecution.DataContext = treeViewExplorerExecutionRootList;

            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                ///MessageBox.Show("Exception\n " + ex.Message, "Error Code - EC16012A", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }

       

        private void Expand_Inventory_Runner(object sender, RoutedEventArgs e)
        {
            try
            {
                if (isInventorySearchListSelected == false)
                {
                    foreach (TreeViewExplorer testSuite in treeViewExplorerInventoryList)
                    {
                        testSuite.IsExpanded = true;
                        foreach (TreeViewExplorer childplan in testSuite.Children)
                        {
                            childplan.IsExpanded = true;
                        }
                    }
                }
                else
                {
                    foreach (TreeViewExplorer testsuite in treeViewExplorerSearchList)
                    {
                        testsuite.IsExpanded = true;
                        foreach (TreeViewExplorer childplan in testsuite.Children)
                        {
                            childplan.IsExpanded = true;
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                ///MessageBox.Show("Exception\n " + ex.Message, "Error Code - EC16012A", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Collapse_Inventory_Runner(object sender, RoutedEventArgs e)
        {
            try
            {
                if (isInventorySearchListSelected == false)
                {
                    foreach (TreeViewExplorer testSuite in treeViewExplorerInventoryList)
                    {
                        testSuite.IsExpanded = false;
                        foreach (TreeViewExplorer childplan in testSuite.Children)
                        {
                            childplan.IsExpanded = false;
                        }

                    }
                }
                else
                {
                    foreach (TreeViewExplorer testSuite in treeViewExplorerSearchList)
                    {
                        testSuite.IsExpanded = false;
                        foreach (TreeViewExplorer childplan in testSuite.Children)
                        {
                            childplan.IsExpanded = false;
                        }

                    }
                }
            }
            catch(Exception ex)
            {

            }
        }

        private void TreeViewExecution_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            try
            {
                if (!treeViewExplorerExecutionRootItem.IsEnabled)
                {
                    e.Handled = true;
                    return;
                }
                ////check to enable expandall
                foreach (TreeViewExplorer testSuite in treeViewExplorerExecutionRootList)
                {
                    ExpandAll.IsEnabled = false;
                    foreach (TreeViewExplorer childsuite in testSuite.Children)
                    {
                        if(childsuite.IsExpanded == false)
                        {

                            ExpandAll.IsEnabled = true; 
                            break;
                        }
                       
                        foreach (TreeViewExplorer childplan in childsuite.Children)
                        {
                            if(childplan.IsExpanded == false)
                              {
                                ExpandAll.IsEnabled = true;
                                break;
                            }
                           
                        }
                    }
                }

                ////check to enable collapseall
                foreach (TreeViewExplorer testSuite in treeViewExplorerExecutionRootList)
                {
                    CollapseAll.IsEnabled = false;
                    foreach (TreeViewExplorer childsuite in testSuite.Children)
                    {
                        if (childsuite.IsExpanded == true)
                        {
                            CollapseAll.IsEnabled = true;
                            break;
                        }
                       

                        foreach (TreeViewExplorer childplan in childsuite.Children)
                        {
                            if (childplan.IsExpanded == true)
                            {
                                CollapseAll.IsEnabled = true;
                                break;
                            }
                           
                        }
                    }
                }


                //////check/uncheck 
                //bool alltrue = false;
                bool skipcheck = false;
                bool skipuncheck = false;
                if (treeViewExplorerExecutionRootList[0].Children.Count>0)
                { //////check/uncheck 
                  //bool alltrue = false;
                  
                    foreach (TreeViewExplorer treeitem in selectedItemsnonTS.Values)
                    {
                        //if (treeitem.ItemName != "Execution Inventory")
                        //{
                        if (!skipcheck)
                        {
                            if (treeitem.IsChecked == null || treeitem.IsChecked == true)
                                skipcheck = true;
                        }
                        if (!skipuncheck)
                        {
                            if (treeitem.IsChecked == null || treeitem.IsChecked != true)
                                skipuncheck = true;

                        }
                        //}
                    }
                   
                }

                CheckAll.IsEnabled = skipuncheck;
                UnCheckAll.IsEnabled = skipcheck;



            }

            catch (Exception ex)
            {

            }


           
        }

       
        private void MenuItem_UnCheckAll_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                foreach (TreeViewExplorer treeviewitem in selectedItemsnonTS.Values)
                {
                    treeviewitem.IsChecked = false;
                }
               
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                ///MessageBox.Show("Exception\n " + ex.Message, "Error Code - EC16012A", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void MenuItem_CheckAll_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                foreach (TreeViewExplorer treeviewitem in selectedItemsnonTS.Values)
                {
                    treeviewitem.IsChecked = true;
                }


            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                ///MessageBox.Show("Exception\n " + ex.Message, "Error Code - EC16012A", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void list_Execution_PreviewDragover(object sender, DragEventArgs e)
        {
            try
            {
                           						 						   
			    DragDropItem dragData = (DragDropItem)e.Data.GetData(typeof(DragDropItem));
                if (dragData == null)
                    return;

                if (dragData.DragSourceType== "ExecutionTreeView")
                { 
                FrameworkElement executionInventoryTreeview = sender as FrameworkElement;
                    if (executionInventoryTreeview == null)
                        return;
                var executionInventoryBorder = VisualTreeHelper.GetChild(executionInventoryTreeview, 0) as Border;
                    if (executionInventoryBorder == null)
                        return;
                ScrollViewer executionInventoryscrollviewer = executionInventoryBorder.Child as ScrollViewer;
                    if (executionInventoryscrollviewer == null)
                        return;

                double offset = 20;
                double tolerance = 20;
                double verticalPos = e.GetPosition(executionInventoryTreeview).Y;
                if (verticalPos < tolerance) // Top of visible list
                {
                        executionInventoryscrollviewer.ScrollToVerticalOffset(executionInventoryscrollviewer.VerticalOffset - offset);
                }
                else if (verticalPos > executionInventoryTreeview.ActualHeight - tolerance) // Bottom of visible list
                {
                        executionInventoryscrollviewer.ScrollToVerticalOffset(executionInventoryscrollviewer.VerticalOffset + offset);

                }
              
            }
          }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                ///MessageBox.Show("Exception\n " + ex.Message, "Error Code - EC16012A", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }

      
    }

    public struct QsysPairingData
    {
        public PairingData CorePairingData;
        public PairingData[] PeriphPairingData;

#region "Override Equals"

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        public override bool Equals(object other)
        {
            return (other is QsysPairingData) && Equals((QsysPairingData)other);
        }
        public static bool operator ==(QsysPairingData lhs, QsysPairingData rhs)
        {
            if (lhs.PeriphPairingData.Length != rhs.PeriphPairingData.Length) { return false; }

            for (int i = 0; i < rhs.PeriphPairingData.Length; i++)
            {
                //Debug.WriteLine("rhs.IO16PairingData.Dynamic_Primary : " + rhs.PeriphPairingData[i].Dynamic_Primary.ToString());
                //Debug.WriteLine("rhs.IO16PairingData.Dynamic_Mode : " + rhs.PeriphPairingData[i].Dynamic_Mode.ToString());
                //Debug.WriteLine("lhs.IO16PairingData.Dynamic_Mode : " + lhs.PeriphPairingData[i].Dynamic_Mode.ToString());
                if (lhs.PeriphPairingData[i].Dynamic_Mode != rhs.PeriphPairingData[i].Dynamic_Mode)
                {
                    return false;
                }
            }
            return true;
        }
        public static bool operator !=(QsysPairingData lhs, QsysPairingData rhs)
        {
            return !(rhs == lhs);
        }
#endregion
    }

    public struct PairingData
    {
        public string Device_Type;
        public string Part_Number;
        public string Primary;
        public string Backup;
        public string Has_Backup;
        public string Is_Required;
        public string Pairing;
        public eQsysPairingMode Dynamic_Mode;
        public string Dynamic_Primary;
        public string Dynamic_Backup;
    }

    public enum eQsysPairingMode
    {
        none,
        name,
        port,
    }
}
