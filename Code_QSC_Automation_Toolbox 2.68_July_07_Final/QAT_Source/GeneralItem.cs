using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Windows;

namespace QSC_Test_Automation
{

    public static class QatConstants
    {
        private static string _SelectedServer = string.Empty;
        public static string SelectedServer { get  { return _SelectedServer; } set {_SelectedServer=value; } }
        public static string TveExecutionInventoryTitle { get { return "Execution Inventory"; } }
        public static string TveExportInventoryTitle { get { return "Export Inventory"; } }
        public static string TveImportInventoryTitle { get { return "Import Inventory"; } }
        public static string TveDesignerTestSuiteTitle { get { return "Test Suites"; } }
        public static string TveDesignerTestPlanTitle { get { return "Test Plans"; } }
        public static string TveDesignerTestCaseTitle { get { return "Test Cases"; } }
        public static string TveDesignerHeaderItemType { get { return "Header"; } }
        public static int TveDesignerTestSuiteID { get { return 1; } }
        public static int TveDesignerTestPlanID { get { return 2; } }
        public static int TveDesignerTestCaseID { get { return 3; } }
        public static int TveDesignerTestSuiteIndex { get { return 0; } }
        public static int TveDesignerTestPlanIndex { get { return 1; } }
        public static int TveDesignerTestCaseIndex { get { return 2; } }
        public static string TveDesignerCatHeaderItemType { get { return "Category Header"; } }
        public static string TveDesignerOtherCatHeader { get { return "Other Category"; } }
		
        public static string DbDatabaseName {
            get
            {
                if ((SelectedServer != string.Empty && SelectedServer == "COSTAMESA_Sandbox") | (SelectedServer != string.Empty && SelectedServer == "JASMIN_Sandbox") | (SelectedServer != string.Empty && SelectedServer == "BOULDER_Sandbox") | (SelectedServer != string.Empty && SelectedServer == "BANGALORE_Sandbox"))
                    return Properties.Settings.Default.DesignerDB_SANDBOX.ToString() ;
                else
                    return Properties.Settings.Default.DesignerDB.ToString();
            }

        }
		 public static string Report_DbDatabaseName {  get {
                if ((SelectedServer != string.Empty && SelectedServer == "COSTAMESA_Sandbox") | (SelectedServer != string.Empty && SelectedServer == "JASMIN_Sandbox") | (SelectedServer != string.Empty && SelectedServer == "BOULDER_Sandbox") | (SelectedServer != string.Empty && SelectedServer == "BANGALORE_Sandbox"))
                    return Properties.Settings.Default.RunnerDB_SANDBOX.ToString();
                else
                    return Properties.Settings.Default.RunnerDB.ToString();} }

        public static string Grafana_DbDatabaseName
        {
            get
            {
                if ((SelectedServer != string.Empty && SelectedServer == "COSTAMESA_Sandbox") | (SelectedServer != string.Empty && SelectedServer == "JASMIN_Sandbox") | (SelectedServer != string.Empty && SelectedServer == "BOULDER_Sandbox") | (SelectedServer != string.Empty && SelectedServer == "BANGALORE_Sandbox"))
                    return Properties.Settings.Default.GrafanaDB_SANDBOX.ToString();
                else
                    return Properties.Settings.Default.GrafanaDB.ToString();
            }
        }

        public static string PcName
        {
            get
            {
                string pcName = string.Empty;

                if (SelectedServer != string.Empty && SelectedServer == "OTHERS")
                {
                    pcName = Properties.Settings.Default.OthersHostMachineName.ToString();
                }
                else
                {
                    if (!string.IsNullOrEmpty(System.Configuration.ConfigurationManager.ConnectionStrings["ConString"].ToString()))
                    {
                        var connectionString = new System.Data.SqlClient.SqlConnectionStringBuilder(System.Configuration.ConfigurationManager.ConnectionStrings["ConString"].ToString());
                        pcName = connectionString.DataSource;
                       
                    }
                }
                return pcName;
            }
        }
               

        private static System.Collections.ObjectModel.ObservableCollection<string> _QRCMversionListValue = new System.Collections.ObjectModel.ObservableCollection<string>();
        public static System.Collections.ObjectModel.ObservableCollection<string> QRCMversionList
        {
            get { return _QRCMversionListValue; }
            set { _QRCMversionListValue = value;/* OnPropertyChanged("QRCMversionList");*/ }
        }



        public static string DbTestSuiteTable { get { return "TestSuite"; } }
        public static string DbTestSuiteNameColumn { get { return "TestSuiteName"; } }
        public static string DbTestSuiteIDColumn { get { return "TestSuiteID"; } }
        public static string DbTestSuiteTestPlanLinkTable { get { return "TSTPLinkTable"; } }
        public static string DbTestSuiteLinkTableID { get { return "TSID"; } }
        public static string DbTestSuiteChildCount { get { return "TSActioncount"; } }
        public static string DbTestPlanTable { get { return "TestPlan"; } }
        public static string DbTestPlanNameColumn { get { return "TestPlanName"; } }
        public static string DbTestPlanIDColumn { get { return "TestPlanID"; } }
        public static string DbTestPlanTestCaseLinkTable { get { return "TPTCLinkTable"; } }
        public static string DbTestPlanLinkTablePrimaryKeyColumn  { get { return "TPTCID"; } }
        public static string DbTestSuiteLinkTablePrimaryKeyColumn { get { return "TSTPID"; } }
        public static string DbTestPlanLinkTableID { get { return "TPID"; } }
        public static string DbTestPlanDesignLinkTable { get { return "TPDesignLinkTable"; } }
        public static string DbTestPlanChildCount { get { return "TPActioncount"; } }
        public static string DbTestDesignTable { get { return "DesignTable"; } }
        public static string DbTestDesignNameColumn { get { return "DesignName"; } }
        public static string DbTestCaseTable { get { return "TestCase"; } }
        public static string DbTestCaseNameColumn { get { return "TestCaseName"; } }
        public static string DbTestCaseIDColumn { get { return "TestCaseID"; } }
        public static string DbTestCaseLinkTableID { get { return "TCID"; } }
        public static string DbTestActionTable { get { return "TestAction"; } }
        public static string DbTestActionTableTestActionID { get { return "TestActionID"; } }
        public static string DbTestDesignLinkTableID { get { return "DesignID"; } }
        public static string DbCategoryColumnName { get { return "Category"; } }
        public static string DbCreatedByColumnName { get { return "CreatedBy"; } }
        public static string DbModifiedByColumnName { get { return "ModifiedBy"; } }
        //public static List<string> NetPairableDeviceList { get { return new List<string> { "I/O-22", "DPA4.2Q", "DPA4.3Q", "DPA4.5Q", "I/O-Frame", "I/O-Frame8S", "PS-400H", "PS-800H", "PS-1600H", "PS-1650H", "TSC-8", "TSC-3" }; } }
        public static List<string> TestFirmwareItemFirmwareUpdateTypeDisplayList { get { return new List<string> { "Automatically update when new version of SW available", "Start auto update with new version of SW at", "Upgrade/Downgrade by installing application", "Upgrade/Downgrade by launching application" }; } }
        public static List<string> TestFirmwareItemFirmwareUpdateTypeDatabaseList { get { return new List<string> { "AutoUpdateSoftware", "UpdateSoftwareAt", "FirmwareByInstall", "FirmwareByLaunch" }; } }
        //public static string QATbuildversion { get { return " [build 1.15]"; } }
        //private static string serverpath = System.IO.Path.Combine(Properties.Settings.Default.Path.ToString());

        //public static string QATServerPath
        //{
        //    get { return serverpath; }
        //    set { serverpath = value; }
        //}

        //public static string QATServerPath { get { return @"F:\\"; } }
        //public static string QATServerPath { get { return @"\\TESTINGPROJECT\Sharing\QSC_SourceCode_Phase1\Sneha\SQA_QAT_Server_Designer"; } }
        //public static string QATServerPath { get { return @"\\testingproject\Sharing\QSC_SourceCode_Phase1\paramesh\server path"; } }
        //public static string QATServerPath { get { return @"E:\testfordesignfolder"; } }
        //public static string QATServerPath { get { return @"E:\test"; } }
        //public static string QATServerPath { get { return @"\\testingproject\Sharing\QSC_SourceCode_Phase1\Sneha\SQA_QAT_Server_Designer"; } }



        public static string ApIKey
        {
            get
            {
                if (SelectedServer != string.Empty && (SelectedServer == "BOULDER_Production" || SelectedServer == "BOULDER_Sandbox"))
                    return Properties.Settings.Default.API_key_BDR.ToString();
                else if (SelectedServer != string.Empty && (SelectedServer == "JASMIN_Production" || SelectedServer == "JASMIN_Sandbox"))
                    return Properties.Settings.Default.API_key_JAS.ToString();
                else if (SelectedServer != string.Empty && SelectedServer == "OTHERS")
                    return Properties.Settings.Default.API_key_Others.ToString();
                else if (SelectedServer != string.Empty && (SelectedServer == "COSTAMESA_Production" || SelectedServer == "COSTAMESA_Sandbox"))
                    return Properties.Settings.Default.API_key_CM.ToString();
                else if (SelectedServer != string.Empty && (SelectedServer == "BANGALORE_Production" || SelectedServer == "BANGALORE_Sandbox"))
                    return Properties.Settings.Default.API_key_BLR.ToString();
                else
                    return Properties.Settings.Default.API_key_CM.ToString();
            }
        }


        public static string QATServerPath
        {
            get
            {
                switch (SelectedServer)
                {
                    case ("COSTAMESA_Production"):
                        {
                            Properties.Settings.Default.ServerPath = Properties.Settings.Default.ServerPathCM.ToString().Trim();
                            return @"" + Properties.Settings.Default.ServerPathCM.ToString().Trim();
                        }
                    case ("BOULDER_Production"):
                        {
                            Properties.Settings.Default.ServerPath = Properties.Settings.Default.ServerPathBDR.ToString().Trim();
                            return @"" + Properties.Settings.Default.ServerPathBDR.ToString().Trim();
                        }
                    case ("JASMIN_Production"):
                        {
                            Properties.Settings.Default.ServerPath = Properties.Settings.Default.ServerPathJAS.ToString().Trim();
                            return @"" + Properties.Settings.Default.ServerPathJAS.ToString().Trim();
                        }
                    case ("BANGALORE_Production"):
                        {
                            Properties.Settings.Default.ServerPath = Properties.Settings.Default.ServerPathBLR.ToString().Trim();
                            return @"" + Properties.Settings.Default.ServerPathBLR.ToString().Trim();
                        }
                    case ("BOULDER_Sandbox"):
                        {
                            Properties.Settings.Default.ServerPath = Properties.Settings.Default.SandboxPathBDR.ToString().Trim();
                            return @"" + Properties.Settings.Default.SandboxPathBDR.ToString().Trim();

                        }
                    case ("COSTAMESA_Sandbox"):
                        {
                            Properties.Settings.Default.ServerPath = Properties.Settings.Default.SandboxPathCM.ToString().Trim();
                            return @"" + Properties.Settings.Default.SandboxPathCM.ToString().Trim();

                        }
                    case ("JASMIN_Sandbox"):
                        {
                            Properties.Settings.Default.ServerPath = Properties.Settings.Default.SandboxPathJAS.ToString().Trim();
                            return @"" + Properties.Settings.Default.SandboxPathJAS.ToString().Trim();
                        }
                    case ("BANGALORE_Sandbox"):
                        {
                            Properties.Settings.Default.ServerPath = Properties.Settings.Default.SandboxPathBLR.ToString().Trim();
                            return @"" + Properties.Settings.Default.SandboxPathBLR.ToString().Trim();
                        }
                    case "OTHERS":
                        {
                            Properties.Settings.Default.ServerPath = Properties.Settings.Default.ServerPathOthers.ToString().Trim();
                            return @"" + Properties.Settings.Default.ServerPathOthers.ToString().Trim();
                        }
                    default:
                        {
                            return @"" + Properties.Settings.Default.ServerPath.ToString().Trim();
                        }
                }
                   
            }
        }
        public static string Reportpath
        {
            get
            {
                switch (SelectedServer)
                {
                    case ("COSTAMESA_Production"):
                        {
                      
                            Properties.Settings.Default.Reportlink = Properties.Settings.Default.ReportlinkCM.ToString().Trim();
                            return @"" + Properties.Settings.Default.ReportlinkCM.ToString().Trim();
                        }
                    case ("BOULDER_Production"):
                        {
                            Properties.Settings.Default.Reportlink = Properties.Settings.Default.ReportlinkBDR.ToString().Trim();
                            return @"" + Properties.Settings.Default.ReportlinkBDR.ToString().Trim();
                        }
                    case ("JASMIN_Production"):
                        {
                            Properties.Settings.Default.Reportlink = Properties.Settings.Default.ReportlinkJAS.ToString().Trim();
                            return @"" + Properties.Settings.Default.ReportlinkJAS.ToString().Trim();
                        }
                    case ("BANGALORE_Production"):
                        {
                            Properties.Settings.Default.Reportlink = Properties.Settings.Default.ReportlinkBLR.ToString().Trim();
                            return @"" + Properties.Settings.Default.ReportlinkBLR.ToString().Trim();
                        }
                    case ("COSTAMESA_Sandbox"):
                        {
                            Properties.Settings.Default.Reportlink = Properties.Settings.Default.ReportlinkCM_sandbox.ToString().Trim();
                            return @"" + Properties.Settings.Default.ReportlinkCM_sandbox.ToString().Trim();
                        }
                    case ("JASMIN_Sandbox"):
                        {
                            Properties.Settings.Default.Reportlink = Properties.Settings.Default.ReportlinkJAS_sandbox.ToString().Trim();
                            return @"" + Properties.Settings.Default.ReportlinkJAS_sandbox.ToString().Trim();
                        }
                    case ("BOULDER_Sandbox"):
                        {
                            Properties.Settings.Default.Reportlink = Properties.Settings.Default.ReportlinkBDR_sandbox.ToString().Trim();
                            return @"" + Properties.Settings.Default.ReportlinkBDR_sandbox.ToString().Trim();
                        }
                    case ("BANGALORE_Sandbox"):
                        {
                            Properties.Settings.Default.Reportlink = Properties.Settings.Default.ReportlinkBLR_sandbox.ToString().Trim();
                            return @"" + Properties.Settings.Default.ReportlinkBLR_sandbox.ToString().Trim();
                        }
                    case ("OTHERS"):
                        {
                            Properties.Settings.Default.Reportlink = Properties.Settings.Default.ReportlinkOthers.ToString().Trim();
                            return @"" + Properties.Settings.Default.ReportlinkOthers.ToString().Trim();
                        }


                    default:
                        {
                            return @"" + Properties.Settings.Default.Reportlink.ToString().Trim();
                        }
                }

            }
        }

        public static string ReleaseFolderPAth
        {
            get
            {
                switch (SelectedServer)
                {
                    case ("COSTAMESA_Production"):
                        {

                            Properties.Settings.Default.ReleaseFolder = Properties.Settings.Default.ReleaseFolderCM.ToString().Trim();
                            return @"" + Properties.Settings.Default.ReleaseFolderCM.ToString().Trim();
                        }
                    case ("COSTAMESA_Sandbox"):
                        {
                            Properties.Settings.Default.ReleaseFolder = Properties.Settings.Default.ReleaseFolderCM.ToString().Trim();
                            return @"" + Properties.Settings.Default.ReleaseFolderCM.ToString().Trim();
                        }
                    case ("BOULDER_Production"):
                        {
                            Properties.Settings.Default.ReleaseFolder = Properties.Settings.Default.ReleaseFolderBDR.ToString().Trim();
                            return @"" + Properties.Settings.Default.ReleaseFolderBDR.ToString().Trim();
                        }
                    case ("BOULDER_Sandbox"):
                        {
                            Properties.Settings.Default.ReleaseFolder = Properties.Settings.Default.ReleaseFolderBDR.ToString().Trim();
                            return @"" + Properties.Settings.Default.ReleaseFolderBDR.ToString().Trim();

                        }
                    case ("JASMIN_Production"):
                        {
                            Properties.Settings.Default.ReleaseFolder = Properties.Settings.Default.ReleaseFolderJAS.ToString().Trim();
                            return @"" + Properties.Settings.Default.ReleaseFolderJAS.ToString().Trim();
                        }

                    case ("JASMIN_Sandbox"):
                        {
                            Properties.Settings.Default.ReleaseFolder = Properties.Settings.Default.ReleaseFolderJAS.ToString().Trim();
                            return @"" + Properties.Settings.Default.ReleaseFolderJAS.ToString().Trim();
                        }
                    case ("BANGALORE_Production"):
                        {
                            Properties.Settings.Default.ReleaseFolder = Properties.Settings.Default.ReleaseFolderBLR.ToString().Trim();
                            return @"" + Properties.Settings.Default.ReleaseFolderBLR.ToString().Trim();
                        }
                    case ("BANGALORE_Sandbox"):
                        {
                            Properties.Settings.Default.ReleaseFolder = Properties.Settings.Default.ReleaseFolderBLR.ToString().Trim();
                            return @"" + Properties.Settings.Default.ReleaseFolderBLR.ToString().Trim();
                        }
                    case ("OTHERS"):
                        {
                            Properties.Settings.Default.ReleaseFolder = Properties.Settings.Default.ReleaseFolderOthers.ToString().Trim();
                            return @"" + Properties.Settings.Default.ReleaseFolderOthers.ToString().Trim();
                        }


                    default:
                        {
                            return @"" + Properties.Settings.Default.ReleaseFolder.ToString().Trim();
                        }
                             
                }
            }
        }

        public static string HelpVideoPath { get { return @"" + Properties.Settings.Default.YouTubeLink.ToString().Trim() + ""; } }
    }

    public class DragDropItem
    {
        public String DragSourceType = null;
        public int DragSourceHashCode = 0;
        public List<TreeViewExplorer> SelectedItems = new List<TreeViewExplorer>();
        public TestPlanItem SourceTestPlanItem = null;
        public TestSuiteItem SourceTestSuiteItem = null;
        public TestActionItem sourceTestActionItem = null;
        public Dictionary<int, TreeViewExplorer> InventorySelectedItemsDictionary = new Dictionary<int, TreeViewExplorer>();
    }

    public class TestHeaderItem : INotifyPropertyChanged
    {
        DBConnection QscDataBase = new DBConnection();
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
                //MessageBox.Show("Exception\n " + ex.Message, "Error Code - ECxxxxx", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public TestHeaderItem(TreeViewExplorer sourceTreeViewItem)
        {
            try
            {
                TestItemName = sourceTreeViewItem.ItemName;
                if (String.Equals(sourceTreeViewItem.ItemName, "Test Suites"))
                {
                    TestItemName = "Test Suites List";
                }
                else if (String.Equals(sourceTreeViewItem.ItemName, "Test Plans"))
                {
                    TestItemName = "Test Plans List";
                }
                else if (String.Equals(sourceTreeViewItem.ItemName, "Test Cases"))
                {
                    TestItemName = "Test Cases List";
                }
                TestItemList = sourceTreeViewItem.Children;
                TestItemParent = sourceTreeViewItem;
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                //MessageBox.Show("Exception\n " + ex.Message, "Error Code - ECxxxxx", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public Visibility TestHeaderGridVisibility
        {
            get { return Visibility.Visible; }
        }

        private bool isSelectedValue = true;
        public bool IsSelected
        {
            get { return isSelectedValue; }
            set
            {
                isSelectedValue = value;
                OnPropertyChanged("IsSelected");
            }
        }

        private List<TreeViewExplorer> testItemListValue = new List<TreeViewExplorer>();
        public List<TreeViewExplorer> TestItemList
        {
            get { return testItemListValue; }
            set
            {
                testItemListValue = value;
                OnPropertyChanged("TestItemList");
            }
        }

        private TreeViewExplorer testItemParentValue = null;
        public TreeViewExplorer TestItemParent
        {
            get { return testItemParentValue; }
            set
            {
                testItemParentValue = value;
                OnPropertyChanged("TestItemParent");
            }
        }

        public string TestItemHeaderName
        {
            get
            {
                if (string.IsNullOrEmpty(testItemNameValue))
                    return "Header List";
                else
                    return testItemNameValue;
            }
        }

        private string testItemNameValue = null;
        public string TestItemName
        {
            get { return testItemNameValue; }
            set
            {
                testItemNameValue = value;
                OnPropertyChanged("TestItemName");
                OnPropertyChanged("TestItemHeaderName");
            }
        }
    }

    public class TestBlankItem : INotifyPropertyChanged
    {
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
                //MessageBox.Show("Exception\n " + ex.Message, "Error Code - ECxxxxx", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public TestBlankItem()
        {
            try
            {
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                //MessageBox.Show("Exception\n " + ex.Message, "Error Code - ECxxxxx", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public string TestItemHeaderName
        {
            get
            {
                return "Blank Page";
            }
        }

        private bool isSelectedValue = true;
        public bool IsSelected
        {
            get { return isSelectedValue; }
            set
            {
                isSelectedValue = value;
                OnPropertyChanged("IsSelected");
            }
        }

    }

}
