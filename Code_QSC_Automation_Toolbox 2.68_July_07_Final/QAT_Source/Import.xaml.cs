using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml;
using System.Xml.Linq;

namespace QSC_Test_Automation
{
    /// <summary>
    /// Interaction logic for Import.xaml
    /// </summary>
    public partial class Import : Window,INotifyPropertyChanged
    {
        List<TreeViewExplorer> sourceimportTreeViewlist = new List<TreeViewExplorer>();
        List<TreeViewExplorer> searchTreeviewlst = new List<TreeViewExplorer>();
        bool isInventorySearchListSelected = false;
        public string sortingOrders = string.Empty;
        public string sortingOrders_OriginalList = string.Empty;

        string importPath = string.Empty;
        string importTCinitPath = string.Empty;
        string xmlversion = string.Empty;
        QatMessageBox Qmsg = null;
        DBConnection dbConnection = new DBConnection();
      //  ProgressControl Progress = null;
        public Thread StartImport = null;
        CopyReplace copyreplc = null;     
        XmlReader reader = null;
        Mutex executionMutex = null;
        bool DoWaitone = false;
        int ActualProgress = 0;
        int Inc = 0;
        string currentVerFileLength = "2.14";
        string filePath = string.Empty;

        public Import()
        {
            try
            {
                InitializeComponent();
                ImportEnable();
                Import_Ok.IsEnabled = false;
                ProgressUpdate.DataContext = this;
                TextUpdate.DataContext = this;
                executionMutex = new Mutex(false);
            }
            catch(Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
            }
        }
        
        public event PropertyChangedEventHandler PropertyChanged;

        private void Onchange(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private string _min = "0";
        private string _max = "100";
        private string _progressvalue = "0";
        private string _percentage =string.Empty;
        private string _testplanCount = string.Empty;
        int Incplan = 0;

        public string Progressvalue
        {
            get { return _progressvalue; }
            set
            {
                if (_progressvalue == value)
                {
                    return;
                }
                _progressvalue = value;
                Percentage ="Importing "+Incplan+"/"+ TestplanCount + " testplans";
                Onchange("Progressvalue");

            }
        }
        public string Min
        {
            get { return _min; }
            set
            {
                _min = value;
                Onchange("Min");
            }
        }

        public string Max
        {
            get { return _max; }
            set
            {
                _max = value;
                Onchange("Max");
            }
        }

        public string TestplanCount
        {
            get { return _testplanCount; }
            set
            {
                _testplanCount = value;
                Onchange("TestplanCount");
            }
        }

        public string Percentage
        {
            get
            {
                return _percentage;
            }
            set
            {
                _percentage = value;
                Onchange("Percentage");
            }
        }

        private void ImportBrowse_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                OpenFileDialog filedialog = new OpenFileDialog();
                filedialog.Multiselect = false;
                filedialog.Filter = "Zip file (*.zip)|*.zip";
                if (filedialog.ShowDialog() == true)
                {
                    Mouse.OverrideCursor = Cursors.Wait;
                    txt_browse.Text = filedialog.FileName;
                    txt_Search.Clear();
                    ImportTreeview(filedialog);
                    sortingOrders_OriginalList = "None";
                    Mouse.OverrideCursor = Cursors.Arrow;
                    ImportEnable();
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

        private void ImportTreeview(OpenFileDialog filedialog)
        {
            try
            {
                sourceimportTreeViewlist.Clear();
                TreeViewImport.DataContext = null;

                string sourceDirFilePath = filedialog.FileName;

                var directorycreate = new DirectoryInfo(@"C:\Users\" + Environment.UserName + @"\AppData\Local\Temp\QATImport");
                if (directorycreate.Exists)
                {
                    DeleteFolder(directorycreate);
                }

                Directory.CreateDirectory(directorycreate.FullName);

                ZipFile.ExtractToDirectory(sourceDirFilePath, directorycreate.FullName);

                filePath = importPath = Path.Combine(directorycreate.FullName, "ExportXML.xml");
                importTCinitPath = Path.Combine(directorycreate.FullName, "ExportTCInitialization.xml");

                ///////////Read XML file       
                if (!File.Exists(filePath))
                {
                    ImportMessageBox("Export file doest not exist, Please select valid zip file", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                XmlReaderSettings settings = new XmlReaderSettings
                {
                    ConformanceLevel = ConformanceLevel.Auto,
                    CheckCharacters = false,
                    IgnoreWhitespace = true
                };

                using (XmlReader xmlversionreader = XmlReader.Create(filePath, settings))
                {
                    xmlversionreader.ReadToDescendant("QATVersion");

                    xmlversionreader.Read();
                    xmlversion = xmlversionreader.Value;
                }

                string currentVersion = "2.5";

                var version1 = new Version(xmlversion);
                var version2 = new Version(currentVersion);

                var result = version1.CompareTo(version2);

                if (result < 0)
                {
                    ImportMessageBox("Import supported from files exported using QAT 2.5 version and above", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                TreeViewExplorer importRootItem = new TreeViewExplorer(0, QatConstants.TveImportInventoryTitle, QatConstants.TveDesignerHeaderItemType, null, null, null, null, null, null, null, null, null, null, 0, true);

                importRootItem.IsExpanded = true;
                importRootItem.IsCheckedVisibility = Visibility.Visible;
                importRootItem.ItemTextBox.FontWeight = FontWeights.Bold;
                sourceimportTreeViewlist.Add(importRootItem);


                //////////Testcase id with name
                
                Dictionary<string, string> testcaseVal = new Dictionary<string, string>();
                Dictionary<string, string> testplanVal = new Dictionary<string, string>();
                Dictionary<string, string> tptclinkVal = new Dictionary<string, string>();


                using (XmlReader mainxmlreader = XmlReader.Create(filePath, settings))
                {
                    while (mainxmlreader.Read())
                    {
                        if ((mainxmlreader.NodeType == XmlNodeType.Element) && (mainxmlreader.Name == "Testcase") && mainxmlreader.HasAttributes)
                        {
                            string tcID = string.Empty;
                            string tcName = string.Empty;

                            for (int attId = 0; attId < mainxmlreader.AttributeCount; attId++)
                            {
                                mainxmlreader.MoveToAttribute(attId);

                                if (mainxmlreader.Name == "TestcaseID")
                                {
                                    tcID = mainxmlreader.Value;
                                }
                                else if (mainxmlreader.Name == "Testcasename")
                                {
                                    tcName = mainxmlreader.Value;
                                }
                            }

                            if (!testcaseVal.Keys.Contains(tcID))
                                testcaseVal.Add(tcID, tcName);
                        }
                        else if ((mainxmlreader.NodeType == XmlNodeType.Element) && (mainxmlreader.Name == "Testplan") && mainxmlreader.HasAttributes)
                        {
                            string tpID = string.Empty;
                            string tpName = string.Empty;

                            for (int attId = 0; attId < mainxmlreader.AttributeCount; attId++)
                            {
                                mainxmlreader.MoveToAttribute(attId);

                                if (mainxmlreader.Name == "TestPlanID")
                                {
                                    tpID = mainxmlreader.Value;
                                }
                                else if (mainxmlreader.Name == "Testplanname")
                                {
                                    tpName = mainxmlreader.Value;
                                }
                            }

                            if (!testplanVal.Keys.Contains(tpID))
                                testplanVal.Add(tpID, tpName);
                        }
                        else if ((mainxmlreader.NodeType == XmlNodeType.Element) && (mainxmlreader.Name == "TPTCLinkTable") && mainxmlreader.HasAttributes)
                        {
                            string tpID = string.Empty;
                            string tcID = string.Empty;

                            for (int attId = 0; attId < mainxmlreader.AttributeCount; attId++)
                            {
                                mainxmlreader.MoveToAttribute(attId);

                                if (mainxmlreader.Name == "TPID")
                                {
                                    tpID = mainxmlreader.Value;
                                }
                                else if (mainxmlreader.Name == "TCID")
                                {
                                    tcID = mainxmlreader.Value;
                                }
                            }

                            if (!tptclinkVal.Keys.Contains(tpID))
                            {
                                tptclinkVal.Add(tpID, tcID);
                            }
                            else
                            {
                                string val = tptclinkVal[tpID];
                                string tcIDs = val + "," + tcID;
                                tptclinkVal[tpID] = tcIDs;
                            }
                        }
                    }
                }

                foreach (KeyValuePair<string, string> testplanDetails in testplanVal)
                {
                    string testplanID = testplanDetails.Key;

                    TreeViewExplorer tpTreeviewRow = new TreeViewExplorer(Convert.ToInt32(testplanID), testplanDetails.Value, "Testplan", null, null, null, null, null, null, null, null, null, null, 0, true);

                    if (tptclinkVal.Keys.Contains(testplanID))
                    {
                        var mapping = tptclinkVal[testplanID];
                        string[] testcaseID = mapping.Split(',');

                        foreach (string tcID in testcaseID)
                        {
                            int testcasesID = Convert.ToInt32(tcID);
                            string testcasename = testcaseVal[tcID];
                            TreeViewExplorer tcTreeviewRow = new TreeViewExplorer(testcasesID, testcasename, "Testcase", null, null, null, null, null, null, null, null, null, null, 0, true);
                            tpTreeviewRow.AddChildren_withCheckbox(tcTreeviewRow);
                        }
                    }

                    importRootItem.AddChildren_withCheckbox(tpTreeviewRow);
                }

                TreeViewImport.DataContext = null;
                TreeViewImport.DataContext = sourceimportTreeViewlist;
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
            }
        }
        
        private void Import_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //if (!DeviceDiscovery.CheckRunningThreadStatus())
                //    return;

                ImportDisable();
                var treeHeader = TreeViewImport.DataContext as List<TreeViewExplorer>;

                if (treeHeader == null)
                {
                    ImportMessageBox("Please select zip file to Import", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    ImportEnable();
                    return;
                }
                else if (treeHeader.Count <= 0 || treeHeader[0].Children.Count == 0 || treeHeader[0].IsChecked == false)
                {
                    ImportMessageBox("Please select testplan to Import", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    ImportEnable();
                    return;
                }
                else if (Properties.Settings.Default.TesterName.Trim().ToString() == string.Empty)
                {
                    ImportMessageBox("Please Enter Testername in preferences", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    //ExportMessageBox("Please Enter Testername in preferences", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    ImportEnable();
                    return;
                }
                else if (!Directory.Exists(QatConstants.QATServerPath))
                {
                    ImportMessageBox("Invalid Server path", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    ImportEnable();
                    return;
                }

                bool hasreadAccess = hasWriteAccessToFolder(QatConstants.QATServerPath);
                if (!hasreadAccess)
                {
                    ImportMessageBox("The server path is read only", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    ImportEnable();
                    return;
                }


                //Progress = new ProgressControl(this);
                Progressvalue = "0";
                Percentage =string.Empty;
                //ImportDisable();
                bool Export_Running = DeviceDiscovery.IsExportRunning();
                bool Execution_Running = DeviceDiscovery.IsExecutionRunning();

                if (!Export_Running)
                {
                    ImportMessageBox("Please wait for Export process to finish & start import ", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    ImportEnable();
                    return;
                }
                if (!Execution_Running)
                {
                    ImportMessageBox("Please wait for Execution process to finish & start import", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    ImportEnable();
                    return;
                }


                StartImport = new Thread(CallImportFunctionThread);
                //StartImport.ApartmentState = ApartmentState.MTA;
               // StartImport.IsBackground = true;
               StartImport.Start();

            }
            catch (Exception ex)
            {
                ImportEnable();
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
            }
        }     

        private void ImportEnable()
        {
            try
            {
                if (TreeViewImport.Items != null && TreeViewImport.Items.Count > 0)
                {
                    var treeHeader = TreeViewImport.Items[0] as TreeViewExplorer;
                    treeHeader.IsEnabled = true;
                }

                txt_browse.IsEnabled = true;
                Import_Browse.IsEnabled = true;
                Import_Ok.IsEnabled = true;
                
                //AbortImport.IsEnabled = false;
            }
            catch(Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
            }
        }

        private void ImportDisable()
        {
            try
            {
                if (TreeViewImport.Items != null && TreeViewImport.Items.Count > 0)
                {
                    var treeHeader = TreeViewImport.Items[0] as TreeViewExplorer;
                    treeHeader.IsEnabled = false;
                }

                txt_browse.IsEnabled = false;
                Import_Browse.IsEnabled = false;
                Import_Ok.IsEnabled = false;
                
                //AbortImport.IsEnabled = true;
            }
            catch(Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
            }
        }

        private void CallImportFunctionThread()
        {
            try
            {

                //  DeviceDiscovery.AddRunningThreadStatus(StartImport.ManagedThreadId, StartImport.ThreadState,"Import");
                executionMutex = null;
                executionMutex = new Mutex(false);
                ActualProgress = 0;

                executionMutex.WaitOne();


                int TestPlanSelectedCount = 0;
                int TestCaseSelectedCount = 0;
                int TestPlanWithoutTestCaseSelectedCount = 0;
                this.Dispatcher.Invoke(() =>
                {
                    ImportDisable();
                    //if (Progress.IsVisible == false)
                    //    Progress = new ProgressControl(this);
                    //Progress.Min = "0";
                    Min = "0";

                    //Progress.Show();
                });

                List<bool> msgTrue = new List<bool>();
                List<TreeViewExplorer> importTreeLst = new List<QSC_Test_Automation.TreeViewExplorer>();

                this.Dispatcher.Invoke(() =>
                {
                    importTreeLst = TreeViewImport.DataContext as List<TreeViewExplorer>;
                });

                if (importTreeLst != null && importTreeLst.Count != 0)
                {
                    SqlConnection connect = this.dbConnection.CreateConnection();
                    this.dbConnection.OpenConnection();

                    this.Dispatcher.Invoke(() =>
                    {
                        copyreplc = new CopyReplace();

                    });

                    Dictionary<string, string> planValues = new Dictionary<string, string>();
                    Dictionary<string, string> caseValues = new Dictionary<string, string>();
                    List<string> plans = new List<string>();
                    List<string> cases = new List<string>();
                    List<string> editedBy = new List<string>();
                    List<string> notExistDesign = new List<string>();
                    bool ZeroTestCase = false;

                    Dictionary<int, string[]> designDetails = new Dictionary<int, string[]>();

                    ////to check duplicate entry and edited by check
                    foreach (TreeViewExplorer PlanExecution in importTreeLst[0].Children)
                    {
                        bool? current1;
                        if ((((current1 = PlanExecution.IsChecked) == true) || ((current1 = PlanExecution.IsChecked) == null)) && (current1 = PlanExecution.IsChecked) != false)
                        {
                            TestPlanSelectedCount++;
                            if (PlanExecution.Children.Count() == 0)
                                TestPlanWithoutTestCaseSelectedCount++;
                            string query = "select EditedBy from Testplan where Testplanname = @TPName";
                            DataTable plntbl = SelectDataTableValue(query, connect, "@TPName", PlanExecution.ItemName);

                            if (plntbl.Rows.Count > 0)
                            {
                                if (!plans.Contains(PlanExecution.ItemName))
                                    plans.Add(PlanExecution.ItemName);

                                string val = plntbl.Rows[0][0].ToString();
                                if (val != null && val != string.Empty)
                                    if (!editedBy.Contains(val))
                                        editedBy.Add(val);
                            }

                            string modifiedDesignName = string.Empty;
                            var directorycreate = new DirectoryInfo(@"C:\Users\" + Environment.UserName + @"\AppData\Local\Temp\QATImport");
                            XmlReaderSettings readerSettings = new XmlReaderSettings { CheckCharacters = false, IgnoreWhitespace = true };
                            string filePath = importPath = Path.Combine(directorycreate.FullName, "ExportXML.xml"); 
                            string path = Path.Combine(directorycreate.FullName, "FileSettings.xml");
                            
                            XmlReaderSettings settings = new XmlReaderSettings
                            {
                                ConformanceLevel = ConformanceLevel.Auto,
                                CheckCharacters = false,
                                IgnoreWhitespace = true
                            };

                            string designID = string.Empty;

                            using (XmlReader xmlversionreader = XmlReader.Create(filePath, settings))
                            {
                                xmlversionreader.ReadToDescendant("TPDesignLinkTable");

                                do
                                {
                                    bool chk = false;
                                    if (xmlversionreader.NodeType == XmlNodeType.Element && xmlversionreader.Name == "TPDesignLinkTable" && xmlversionreader.HasAttributes && xmlversionreader.GetAttribute("TPID") != null && xmlversionreader.GetAttribute("TPID") == PlanExecution.ItemKey.ToString())
                                    {
                                        for (int attId = 0; attId < xmlversionreader.AttributeCount; attId++)
                                        {
                                            xmlversionreader.MoveToAttribute(attId);

                                            if (xmlversionreader.Name == "DesignID")
                                            {
                                                designID = xmlversionreader.Value;
                                                chk = true;
                                                break;
                                            }
                                        }
                                    }

                                    if (chk == true)
                                        break;
                                }
                                while (xmlversionreader.Read());
                            }

                            string oldDesignName = string.Empty;

                            using (XmlReader xmldesignNamereader = XmlReader.Create(filePath, settings))
                            {
                                xmldesignNamereader.ReadToDescendant("designtable");

                                do
                                {
                                    bool chk = false;

                                    if (xmldesignNamereader.NodeType == XmlNodeType.Element && xmldesignNamereader.Name == "designtable" && xmldesignNamereader.HasAttributes && xmldesignNamereader.GetAttribute("DesignID") != null && xmldesignNamereader.GetAttribute("DesignID") == designID)
                                    {
                                        for (int attId = 0; attId < xmldesignNamereader.AttributeCount; attId++)
                                        {
                                            xmldesignNamereader.MoveToAttribute(attId);

                                            if (xmldesignNamereader.Name == "Designname")
                                            {
                                                oldDesignName = xmldesignNamereader.Value;
                                                chk = true;
                                                break;
                                            }
                                        }
                                    }

                                    if (chk == true)
                                        break;
                                } while (xmldesignNamereader.Read());
                            }

                            var version1 = new Version(xmlversion);
                            var version2 = new Version(currentVerFileLength);

                            var result = version1.CompareTo(version2);

                            if (result < 0)
                            {
                                ////////////Old

                                modifiedDesignName = oldDesignName;
                            }
                            else
                            {
                                ///////New

                                using (XmlReader reader = XmlReader.Create(path, readerSettings))
                                {
                                    //reader.ReadStartElement("DesignFiles");

                                    while (reader.Read())
                                    {
                                        if (reader.NodeType == XmlNodeType.Element && reader.Name == "DesignFile" && reader.HasAttributes && reader.GetAttribute("TPID") != null && reader.GetAttribute("TPID") == PlanExecution.ItemKey.ToString())
                                        {
                                            for (int attId = 0; attId < reader.AttributeCount; attId++)
                                            {
                                                reader.MoveToAttribute(attId);

                                                if (reader.Name == "DummyName")
                                                {
                                                    modifiedDesignName = reader.Value;
                                                    break;
                                                }
                                            }
                                        }
                                    }
                                }
                            }

                            string isdesign = "true";
                            using (XmlReader xmlversionreader = XmlReader.Create(filePath, settings))
                            {
                                xmlversionreader.ReadToDescendant("Testplan");

                                while (xmlversionreader.Read()) 
                                {
                                    bool chk = false;
                                    if (xmlversionreader.NodeType == XmlNodeType.Element && xmlversionreader.Name == "Testplan" && xmlversionreader.HasAttributes && xmlversionreader.GetAttribute("IsDesign") != null && xmlversionreader.GetAttribute("TestPlanID") != null && xmlversionreader.GetAttribute("TestPlanID") == PlanExecution.ItemKey.ToString())
                                    {
                                        for (int attId = 0; attId < xmlversionreader.AttributeCount; attId++)
                                        {
                                            xmlversionreader.MoveToAttribute(attId);

                                            if (xmlversionreader.Name == "IsDesign")
                                            {
                                                if (xmlversionreader.Value != null)
                                                    isdesign = xmlversionreader.Value;
                                                chk = true;
                                                break;
                                            }
                                        }
                                    }

                                    if (chk == true)
                                        break;
                                }
                               
                            }


                            if (oldDesignName != null && oldDesignName != string.Empty)
                            {
                                designDetails.Add(PlanExecution.ItemKey, new string[] { designID, modifiedDesignName, oldDesignName });
                                string designPath = Path.Combine(directorycreate.FullName, "Designs");

                                string designFileWithPath = Path.Combine(designPath, modifiedDesignName);
                                
                                if (!File.Exists(designFileWithPath) && Convert.ToBoolean(isdesign) == true)
                                {
                                    notExistDesign.Add(oldDesignName);
                                }
                            }

                            foreach (TreeViewExplorer testcaseExplore in PlanExecution.Children)
                            {
                                bool? current3;
                                if ((((current3 = testcaseExplore.IsChecked) == true) || ((current3 = testcaseExplore.IsChecked) == null)) && (current3 = testcaseExplore.IsChecked) != false)
                                {
                                    TestCaseSelectedCount++;

                           
                                    string caseQuery = "select EditedBy from Testcase where Testcasename = @TCName";
                                    DataTable caseTbl = SelectDataTableValue(caseQuery, connect, "@TCName", testcaseExplore.ItemName);

                                    if (caseTbl.Rows.Count > 0)
                                    {
                                        if (!cases.Contains(testcaseExplore.ItemName))
                                            cases.Add(testcaseExplore.ItemName);

                                        string val = caseTbl.Rows[0][0].ToString();

                                        if (val != null && val != string.Empty)
                                            if (!editedBy.Contains(val))
                                                editedBy.Add(val);
                                    }
                                }
                            }
                        }
                    }

                    bool iseditedTrue = false;
                    if (editedBy.Count > 0)
                    {
                        string text = string.Join("\r\n", editedBy);

                        MessageBoxResult result = ImportMessageBox("Some Testplans/Testcases are being Edited by" + "\r\n" + "\r\n" + text + "\r\n" + "\r\n" + " Do you want to continue Import ?", "Import Alert", MessageBoxButton.OKCancel, MessageBoxImage.Warning);
                        if (result == MessageBoxResult.OK)
                            iseditedTrue = true;
                        if (result == MessageBoxResult.Cancel)
                            iseditedTrue = false;
                    }
                    else
                    {
                        iseditedTrue = true;
                    }

                    if (iseditedTrue == true)
                    {
                        int i = plans.Count;
                        bool isBreak = false;

                        foreach (var val in plans)
                        {
                            i = i - 1;

                            if (copyreplc.Checkbox == false)
                            {
                                if (i <= 0)
                                {
                                    copyreplc.ChkBoxVisibility = Visibility.Collapsed;
                                }
                                else
                                {
                                    copyreplc.Conflicts = i.ToString();
                                }

                                copyreplc.Filename = val + " is already exist";

                                this.Dispatcher.Invoke(() =>
                                {
                                    copyreplc.Headername = "TestPlan:";
                                    copyreplc.ShowDialog();
                                });
                            }

                            if (copyreplc.Actionchoosen == "Cancel")
                            {
                                isBreak = true;
                                break;
                            }

                            planValues.Add(val, copyreplc.Actionchoosen);

                        }

                        if (isBreak == false)
                        {
                            this.Dispatcher.Invoke(() =>
                            {
                                copyreplc = new CopyReplace();
                            });

                            i = cases.Count;
                            foreach (var val in cases)
                            {
                                i = i - 1;

                                if (copyreplc.Checkbox == false)
                                {
                                    if (i <= 0)
                                    {
                                        copyreplc.ChkBoxVisibility = Visibility.Collapsed;
                                    }
                                    else
                                    {
                                        copyreplc.Conflicts = i.ToString();
                                    }

                                    copyreplc.Filename = val + " is already exist";

                                    this.Dispatcher.Invoke(() =>
                                    {
                                        copyreplc.Headername = "TestCase:";
                                        copyreplc.ShowDialog();
                                    });
                                }

                                if (copyreplc.Actionchoosen == "Cancel")
                                {
                                    isBreak = true;
                                    break;
                                }

                                caseValues.Add(val, copyreplc.Actionchoosen);
                            }

                            this.Dispatcher.Invoke(() =>
                            {
                                copyreplc.Close();
                            });

                            if (isBreak == false)
                            {
                                bool isdesignExistTrue = false;

                                if (notExistDesign.Count > 0)
                                {
                                    string text = string.Join("\r\n", notExistDesign);

                                    MessageBoxResult result = ImportMessageBox("Following Designs are missing in the zip file" + "\r\n" + "\r\n" + text + "\r\n" + "\r\n" + " Do you want to import ?", "Design missing Alert", MessageBoxButton.OKCancel, MessageBoxImage.Warning);
                                    if (result == MessageBoxResult.OK)
                                        isdesignExistTrue = true;
                                    else if (result == MessageBoxResult.Cancel)
                                        isdesignExistTrue = false;
                                }
                                else
                                {
                                    isdesignExistTrue = true;
                                }

                                if (isdesignExistTrue == true)
                                {
                                    Dictionary<string, string> testCaseRepetedID = new Dictionary<string, string>();
                                    List<string> dontdeleteTC = new List<string>();
                                    

                                    if (TestCaseSelectedCount == 0)
                                    {
                                        Max = TestPlanSelectedCount.ToString();
                                        ZeroTestCase = true;
                                    }
                                    else
                                    {
                                        Max = TestCaseSelectedCount.ToString();
                                    }

                                    if (TestPlanWithoutTestCaseSelectedCount > 0 && !ZeroTestCase)
                                    {
                                        Max = (TestCaseSelectedCount + TestPlanWithoutTestCaseSelectedCount).ToString();
                                    }

                                    Dictionary<string,string> lengthyFile = new Dictionary<string, string>();
                                    foreach (TreeViewExplorer PlanExecution in importTreeLst[0].Children)
                                    {
                                        DoWaitone = true;
                                        executionMutex.ReleaseMutex();
                                        executionMutex.WaitOne();
                                        //Progress.Max = sourceimportTreeViewlist[0].Children.Count().ToString();
                                        //Max = TestPlanSelectedCount.ToString();
                                        
                                        
                                        TestplanCount = TestPlanSelectedCount.ToString();
                                        
                                        bool? current3;
                                        if ((((current3 = PlanExecution.IsChecked) == true) || ((current3 = PlanExecution.IsChecked) == null)) && (current3 = PlanExecution.IsChecked) != false)
                                        {

                                            DeviceDiscovery.WriteToLogFile("Test Plan import is started: " + PlanExecution.ItemName);
                                            ++Incplan;

                                            if (ZeroTestCase)
                                                Progressvalue=(++Inc).ToString();
                                            if(PlanExecution.Children.Count==0 && !ZeroTestCase)
                                                Progressvalue = (++Inc).ToString();


                                            //-----------------------------------------------------------TestPlan start ---------------------------------------------------------------------------//

                                            string nameExistQuery = "select TestPlanID from Testplan where Testplanname =@planName";
                                            string tpIDExist = SelectSingleValue(nameExistQuery, connect, "@planName", PlanExecution.ItemName);
                                            //  Progressvalue = (++Inc).ToString();
                                           
                                            if (tpIDExist == string.Empty || tpIDExist == null)
                                            {
                                                var item = TestplanImport(PlanExecution, connect, tpIDExist, testCaseRepetedID, dontdeleteTC, string.Empty, caseValues, lengthyFile, designDetails);
                                                testCaseRepetedID = item.Item1;
                                                dontdeleteTC = item.Item2;
                                                lengthyFile = item.Item4;
                                                msgTrue.Add(item.Item3);
                                            }
                                            else
                                            {
                                                if (planValues.Keys.Contains(PlanExecution.ItemName))
                                                {
                                                    string planmsgType = planValues[PlanExecution.ItemName];

                                                    DeviceDiscovery.WriteToLogFile("Message type is : " + planmsgType);

                                                    if (planmsgType == "CopyAndReplace")
                                                    {
                                                        var item = TestplanImport(PlanExecution, connect, tpIDExist, testCaseRepetedID, dontdeleteTC, planmsgType, caseValues, lengthyFile, designDetails);
                                                        testCaseRepetedID = item.Item1;
                                                        dontdeleteTC = item.Item2;
                                                        msgTrue.Add(item.Item3);
                                                        lengthyFile = item.Item4;
                                                    }
                                                    else if (planmsgType == "KeepBoth")
                                                    {
                                                        var item = TestplanImport(PlanExecution, connect, string.Empty, testCaseRepetedID, dontdeleteTC, planmsgType, caseValues, lengthyFile, designDetails);
                                                        testCaseRepetedID = item.Item1;
                                                        dontdeleteTC = item.Item2;
                                                        msgTrue.Add(item.Item3);
                                                        lengthyFile = item.Item4;
                                                    }
                                                }
                                                else
                                                {
                                                    var item = TestplanImport(PlanExecution, connect, string.Empty, testCaseRepetedID, dontdeleteTC, "KeepBoth", caseValues, lengthyFile, designDetails);
                                                    testCaseRepetedID = item.Item1;
                                                    dontdeleteTC = item.Item2;
                                                    msgTrue.Add(item.Item3);
                                                    lengthyFile = item.Item4;
                                                }
                                            }

                                            DeviceDiscovery.WriteToLogFile("Test Plan import is ended: " + PlanExecution.ItemName);
                                            //ActualProgress = Convert.ToInt32(Progressvalue);
                                        }

                                        executionMutex.ReleaseMutex();
                                        executionMutex.WaitOne();

                                        //this.Dispatcher.Invoke(() =>
                                        //{
                                        //    if (ActualProgress != 0)
                                        //    {
                                        //        double RequiredProgress = Convert.ToDouble(Max);
                                        //        double PercentRequired = (ActualProgress / RequiredProgress) * 100;
                                        //        Percentage = Convert.ToInt32(PercentRequired).ToString() + "%";
                                        //        //if (Percentage == "100%")
                                        //        //{

                                        //        //}
                                        //        executionMutex.ReleaseMutex();
                                        //        executionMutex.WaitOne();
                                        //    }
                                        //});

                                    }

                                    if (lengthyFile.Count > 0)
                                        ImportMessageBox("The specified server path/file name/both are too long, so the following files are not coppied,\n\t" + String.Join("\n\t", lengthyFile.Keys.ToArray()), "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);

                                    //TCwithnoActioncount();
                                    //TPwithnoActioncount();
                                    executionMutex.ReleaseMutex();
                                    executionMutex.WaitOne();
                                }
                            }
                        }
                    }
                    this.Dispatcher.Invoke(() =>
                    {
                        ImportEnable();
                    });
                        this.dbConnection.CloseConnection();
                    this.Dispatcher.Invoke(() =>
                    {
                       
                        this.Focus();
                       // if (ActualProgress == Convert.ToInt32(Max))
                       // {

                            Progressvalue = "0";
                            Percentage = string.Empty;
                            Incplan = 0;
                            Inc = 0;
                            TestPlanSelectedCount = 0;
                            TestCaseSelectedCount = 0;
                            DoWaitone = false;
                            executionMutex = null;
                            ZeroTestCase = false;

                            if (msgTrue.Count == 0 || msgTrue.Contains(false))
                                ImportMessageBox("Import unsuccessful \nDesigner window will auto refresh if active", "Information", MessageBoxButton.OK, MessageBoxImage.Warning);
                            else
                                ImportMessageBox("Import successfully completed \nDesigner window will auto refresh if active", "Information", MessageBoxButton.OK, MessageBoxImage.Information);

                             ImportEnable();
                       // }
                    });

                    abortimport();
                    //  Progress.Close();
                }
            }
            catch (Exception ex)
            {
                this.Dispatcher.Invoke(() =>
                {
                    ImportEnable();
                });
             
                if (StartImport != null)
                    StartImport = null;

                if (DeviceDiscovery.DesignerWindow != null)
                    DeviceDiscovery.CreateDesignerWindow(true);

                abortimport();

                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
            }
        }

        public void abortimport()
        {

            if (StartImport != null && StartImport.ThreadState != ThreadState.Aborted)
            {
                if (StartImport.ThreadState == ThreadState.Suspended)
                {
                    StartImport.Resume();
                    StartImport.Abort();
                }
                else { StartImport.Abort(); }
            }
        }

        private Tuple<Dictionary<string, string>, List<string>, bool, Dictionary<string, string>> TestplanImport(TreeViewExplorer PlanExecution, SqlConnection connect, string ExistTPID, Dictionary<string, string> testCaseRepetedID, List<string> dontdelete_TC, string msgType, Dictionary<string,string> caseValues, Dictionary<string, string> lengthyFiles, Dictionary<int, string[]> designDetails)
        {
            Dictionary<string, string> tcRepeatedID = testCaseRepetedID;
            List<string> dontdeleteTC = dontdelete_TC;
            bool msgTrue = false;

            try
            {
                string oldTPID = PlanExecution.ItemKey.ToString();

                List <string> tblItemChk = new List<string>();
                tblItemChk.Add("TestPlanID");

                DeviceDiscovery.WriteToLogFile("Fetching testplan Design details");

                string newTPID = string.Empty;
                var testplanvalues = ComparexmlWithDBTCTP(filePath, oldTPID, connect, tblItemChk, "Testplan", "TestPlanID", "Testplanname", msgType, true);
                 
                XmlReaderSettings settings = new XmlReaderSettings
                {
                    ConformanceLevel = ConformanceLevel.Auto,
                    CheckCharacters = false,
                    IgnoreWhitespace = true
                };

                string oldDesignID = string.Empty;
                string oldDesignName = string.Empty;
                string dummyDesignName = string.Empty;

                if (designDetails != null && designDetails.Count > 0 && designDetails.Keys.Contains(PlanExecution.ItemKey))
                {
                    string[] designDetail = designDetails[PlanExecution.ItemKey];
                    oldDesignID = designDetail[0];
                    dummyDesignName = designDetail[1];
                    oldDesignName = designDetail[2];
                }
                else
                {
                    return new Tuple<Dictionary<string, string>, List<string>, bool, Dictionary<string, string>>(tcRepeatedID, dontdeleteTC, msgTrue, lengthyFiles);
                }

                string oldPlanName = testplanvalues.Item4;
                string newPlanName = testplanvalues.Item5;      
                string designNametocopy = oldDesignName;
                
                if (newPlanName != string.Empty && newPlanName != null)
                {
                    string newDesignName = designNametocopy.Remove(0, oldPlanName.Length + 4);
                    designNametocopy = "QAT_" + newPlanName + newDesignName;
                }

                var directorycreate = new DirectoryInfo(@"C:\Users\" + Environment.UserName + @"\AppData\Local\Temp\QATImport");
                string designPath = Path.Combine(directorycreate.FullName, "Designs");

                string serverPath = QatConstants.QATServerPath;

                if (!Directory.Exists(designPath))
                {
                    Directory.CreateDirectory(designPath);
                }
                                  
                string serverwithdesignPath = string.Empty;
                string newServerDesignPath = string.Empty;

                try
                {
                    string designFileWithPath = Path.Combine(designPath, dummyDesignName);  

                    if (File.Exists(designFileWithPath))
                    {
                        serverwithdesignPath = Path.Combine(serverPath, "Designs");

                        if (!Directory.Exists(serverwithdesignPath))
                        {
                            Directory.CreateDirectory(serverwithdesignPath);
                        }

                        if (msgType == "CopyAndReplace")
                        {
                            string query = "select Designname from designtable where DesignID in (select DesignID from TPDesignLinkTable where TPID in (select TestPlanID from Testplan where Testplanname = @TPName))";
                            DataTable tble = dbConnection.SelectDTWithParameter(query, "@TPName", PlanExecution.ItemName);
                            if (tble.Rows.Count > 0)
                            {
                                DataTableReader read = tble.CreateDataReader();

                                while (read.Read())
                                {
                                    string designname = read.GetValue(0).ToString();

                                    string pathtodelete = Path.Combine(serverwithdesignPath, designname);

                                    if (File.Exists(pathtodelete))
                                    {
                                        FileInfo fileInformations = new FileInfo(pathtodelete);
                                        fileInformations.IsReadOnly = false;
                                        File.Delete(pathtodelete);
                                    }
                                }
                            }
                        }

                        string planNameToTruncate = string.Empty;
                        if (newPlanName != string.Empty && newPlanName != null)
                        {
                            planNameToTruncate = newPlanName;
                        }
                        else
                        {
                            planNameToTruncate = oldPlanName;
                        }

                        /////////////////////////Change Design Version as V1  
                        try
                        {
                            string fileName = designNametocopy;

                            string file = fileName.Remove(0, planNameToTruncate.Length + 5);

                            int difference = fileName.Length - file.Length + 1 + file.Split('_')[0].Length;
                            
                            string[] split = fileName.Substring(0, difference).Split('_');
                            split[split.Count() - 2] = "V1";                    

                            string dontTruncateString = string.Join("_", split);
                            string truncateString = fileName.Substring(difference, fileName.Length - difference - 5);

                            string versionChangedFileName = dontTruncateString + truncateString + ".qsys";

                            serverwithdesignPath = Path.Combine(serverwithdesignPath, versionChangedFileName);

                            newServerDesignPath = TruncateDesignName(serverwithdesignPath, planNameToTruncate, dontTruncateString, truncateString);

                        }
                        catch (Exception ex)
                        {
                            serverwithdesignPath = Path.Combine(serverwithdesignPath, designNametocopy);
                        }


                        if (newServerDesignPath != null && newServerDesignPath != string.Empty)
                        {
                            if (File.Exists(newServerDesignPath))
                            {
                                FileInfo fileInformations = new FileInfo(newServerDesignPath);
                                fileInformations.IsReadOnly = false;
                            }

                            File.Copy(designFileWithPath, newServerDesignPath, true);
                            FileInfo fileInformation = new FileInfo(newServerDesignPath);
                            fileInformation.IsReadOnly = true;
                        }
                        else
                        {
                            if (File.Exists(serverwithdesignPath))
                            {
                                FileInfo fileInformations = new FileInfo(serverwithdesignPath);
                                fileInformations.IsReadOnly = false;
                            }

                            File.Copy(designFileWithPath, serverwithdesignPath, true);
                            FileInfo fileInformation = new FileInfo(serverwithdesignPath);
                            fileInformation.IsReadOnly = true;
                        }
                    }                     
                }
                catch(Exception ex)
                {
                    if(ex.Message.Contains("The specified path, file name, or both are too long."))
                    {
                        if(!lengthyFiles.Keys.Contains(oldDesignName))
                            lengthyFiles.Add(oldDesignName, PlanExecution.ItemName);
                    }
                } 

                //// Importing Values to database from xml
                if (testplanvalues.Item3 == true)
                {
                    msgTrue = true;

                    string designfield = string.Join(",", testplanvalues.Item1.Keys);
                    string designval = string.Join(",", testplanvalues.Item1.Values);
                    
                    if (designfield != string.Empty && designfield != null)
                    {
                        string query = "Insert into Testplan (" + designfield + ") values (" + designval + ");SELECT CONVERT(int,SCOPE_IDENTITY())";
                        newTPID = InsertInToDB(query, connect, testplanvalues.Item1.Values.ToList(), testplanvalues.Item2);

                        if(newTPID != null && newTPID != string.Empty)
                        {
                            if (testplanvalues.Item6.Count > 0)
                            {
                                if (testplanvalues.Item6.Contains("ImportedBy"))
                                {
                                    string updateQuery = "Update Testplan set ImportedBy = @ImportedBy where TestPlanID = " + newTPID;
                                    SelectSingleValue(updateQuery, connect, "ImportedBy", Properties.Settings.Default.TesterName);
                                }

                                if (testplanvalues.Item6.Contains("ImportedOn"))
                                {
                                    string updateQuery = "Update Testplan set ImportedOn = @ImportedOn where TestPlanID = " + newTPID;
                                    SelectSingleTPTCValue(updateQuery, connect, "ImportedOn", DateTime.Now);
                                }
                            }
                        }
                    }
                }
                
                if (newTPID != string.Empty && newTPID != null)
                {

                    ////------------------------------------DesignStart-------------------------------------------------------------//

                    DeviceDiscovery.WriteToLogFile("DesignTable is started");

                    if (oldDesignID != string.Empty && oldDesignID != null)
                    {
                        #region designtable

                        tblItemChk.Clear();
                        tblItemChk.Add("DesignID");
                        tblItemChk.Add("TPID");
                        
                        string designName = string.Empty;

                        if (newServerDesignPath != null && newServerDesignPath != string.Empty)
                        {
                            designName = Path.GetFileName(newServerDesignPath);
                        }
                        else if(serverwithdesignPath != null && serverwithdesignPath != string.Empty)
                        {
                            designName = Path.GetFileName(serverwithdesignPath);
                        }
                        else
                        {
                            designName = oldDesignName;
                        }

                        var designtblValue = ComparexmlWithDB(oldDesignID, connect, tblItemChk, "designtable", "DesignID", testplanvalues.Item4, testplanvalues.Item5, designName);
                        string newdesignID = string.Empty;

                        if (designtblValue.Item3 == true)
                        {
                            string designfield = string.Join(",", designtblValue.Item1.Keys);
                            string designval = string.Join(",", designtblValue.Item1.Values);

                            string query = string.Empty;

                            if (designfield != string.Empty && designfield != null)
                                query = "Insert into designtable (TPID, " + designfield + ") values (" + newTPID + "," + designval + ");SELECT CONVERT(int,SCOPE_IDENTITY())";
                            else
                                query = "Insert into designtable (TPID) values (" + newTPID + ");SELECT CONVERT(int,SCOPE_IDENTITY())";

                            newdesignID = InsertInToDB(query, connect, designtblValue.Item1.Values.ToList(), designtblValue.Item2);

                        }

                        #endregion

                        #region TPDesignLinkTable, DesignInventory, TCInitialization

                        if (newdesignID != string.Empty)
                        {
                            tblItemChk.Clear();
                            tblItemChk.Add("DesignID");
                            tblItemChk.Add("TPID");

                            var tptodesignValue = ComparexmlWithDBFor2Query(oldTPID, oldDesignID, connect, tblItemChk, "TPDesignLinkTable", "TPID", "DesignID");
                           
                            if (tptodesignValue.Item3 == true)
                            {
                                string tpdesignlinkfield = string.Join(",", tptodesignValue.Item1.Keys);
                                string tpdesignlinkval = string.Join(",", tptodesignValue.Item1.Values);

                                string query = string.Empty;

                                if (tpdesignlinkfield != string.Empty && tpdesignlinkfield != null)
                                    query = "Insert into TPDesignLinkTable (TPID, DesignID, " + tpdesignlinkfield + ") values (" + newTPID + "," + newdesignID + "," + tpdesignlinkval + ")";
                                else
                                    query = "Insert into TPDesignLinkTable (TPID, DesignID) values (" + newTPID + "," + newdesignID + ")";

                                InsertInToDB(query, connect, tptodesignValue.Item1.Values.ToList(), tptodesignValue.Item2);

                            }

                            InsertBulkData(connect, oldDesignID, newdesignID, "DesignInventory", "DesignID");
                             
                            string currentVersion = "2.8";

                            var version1 = new Version(xmlversion);
                            var version2 = new Version(currentVersion);

                            var results = version1.CompareTo(version2);

                            if (results < 0)
                            {
                                InsertBulkData(connect, oldDesignID, newdesignID, "TCInitialization", "DesignID");             
                            }
                            else
                            {
                                InsertBulkDataForTCInit(connect, oldDesignID, newdesignID, "TCInitialization", "DesignID");
                            }
                        }

                        #endregion
                    }

                    DeviceDiscovery.WriteToLogFile("Design table is ended");


                    ////---------------------------------------Monitor started-------------------------------------------------------//

                    #region BackgroundMonitoring
                    DeviceDiscovery.WriteToLogFile("BackgroundMonitoring is started");

                    using (XmlReader xmltoMonitorlinkreader = XmlReader.Create(filePath, settings))
                    {
                        xmltoMonitorlinkreader.ReadToDescendant("TPMonitorLinkTable");

                        do
                        {
                            if (xmltoMonitorlinkreader.NodeType == XmlNodeType.Element && xmltoMonitorlinkreader.Name == "TPMonitorLinkTable" && xmltoMonitorlinkreader.HasAttributes && xmltoMonitorlinkreader.GetAttribute("TPID") != null && xmltoMonitorlinkreader.GetAttribute("TPID") == oldTPID)
                            {
                                string oldBackGroundID = string.Empty;

                                for (int attId = 0; attId < xmltoMonitorlinkreader.AttributeCount; attId++)
                                {
                                    xmltoMonitorlinkreader.MoveToAttribute(attId);

                                    if (xmltoMonitorlinkreader.Name == "BMID")
                                    {
                                        oldBackGroundID = xmltoMonitorlinkreader.Value;
                                        break;
                                    }
                                }

                                if (oldBackGroundID != null && oldBackGroundID != string.Empty)
                                {
                                    string oldBMID = oldBackGroundID;

                                    string newBMID = string.Empty;

                                    tblItemChk.Clear();
                                    tblItemChk.Add("BackgroundMonitorID");
                                    tblItemChk.Add("TSID");
                                    tblItemChk.Add("TPID");

                                    var compVal = ComparexmlWithDB(oldBMID, connect, tblItemChk, "BackgroundMonitoring", "BackgroundMonitorID", null, null, null);

                                    if (compVal.Item3 == true)
                                    {
                                        string bmtblfield = string.Join(",", compVal.Item1.Keys);
                                        string bmtblval = string.Join(",", compVal.Item1.Values);

                                        string query = string.Empty;

                                        if (bmtblfield != string.Empty && bmtblfield != null)
                                            query = "Insert into BackgroundMonitoring (TSID, TPID, " + bmtblfield + ") values (0," + newTPID + "," + bmtblval + ");SELECT CONVERT(int,SCOPE_IDENTITY())";
                                        else
                                            query = "Insert into BackgroundMonitoring (TSID, TPID) values (0," + newTPID + ")";

                                        newBMID = InsertInToDB(query, connect, compVal.Item1.Values.ToList(), compVal.Item2);

                                    }

                                    if (newBMID != string.Empty)
                                    {
                                        #region TPMonitorLinkTable

                                        tblItemChk.Clear();
                                        tblItemChk.Add("BMID");
                                        tblItemChk.Add("TPID");

                                        var compbmVal = ComparexmlWithDB(oldBMID, connect, tblItemChk, "TPMonitorLinkTable", "BMID", null, null, null);

                                        if (compbmVal.Item3 == true)
                                        {
                                            string bmfield = string.Join(",", compbmVal.Item1.Keys);
                                            string bmval = string.Join(",", compbmVal.Item1.Values);

                                            string tomonitorquery = string.Empty;

                                            if (bmfield != string.Empty && bmfield != null)
                                                tomonitorquery = "Insert into TPMonitorLinkTable (TPID, BMID, " + bmfield + ") values (" + newTPID + "," + newBMID + "," + bmval + ")";
                                            else
                                                tomonitorquery = "Insert into TPMonitorLinkTable (TPID, BMID) values (" + newTPID + "," + newBMID + ")";

                                            InsertInToDB(tomonitorquery, connect, compbmVal.Item1.Values.ToList(), compbmVal.Item2);

                                        }

                                        #endregion

                                        #region ControlMonitor

                                        InsertBulkBackgroundDatas(connect, oldBMID, oldTPID, newBMID, newTPID, "ControlMonitor", "ControlMonitorID", "BMID", "TSID", "TPID");

                                        #endregion

                                        #region TelenetMonitor

                                        InsertBulkBackgroundDatas(connect, oldBMID, oldTPID, newBMID, newTPID, "TelenetMonitor", "TelenetMonitorID", "BMID", "TSID", "TPID");

                                        #endregion

                                        #region LogMonitor1

                                        InsertBulkBackgroundDatas(connect, oldBMID, oldTPID, newBMID, newTPID, "LogMonitor1", "LogMonitorID", "BMID", "TSID", "TPID");

                                        #endregion

                                        #region InventoryMonitor

                                        InsertBulkBackgroundDatas(connect, oldBMID, oldTPID, newBMID, newTPID, "InventoryMonitor", "InventoryMonitorID", "BMID", "TSID", "TPID");

                                        #endregion

                                    }
                                }
                            }
                        } while (xmltoMonitorlinkreader.Read());
                    }

                    DeviceDiscovery.WriteToLogFile("BackgroundMonitoring is ended");

                    #endregion

                    ////------------------------------------Testcase started---------------------------------------------------------------------//

                    #region Testcase
                    Dictionary<string, string> tcIDList = new Dictionary<string, string>();
                    Dictionary<string, string> tcIDWithMsgType = new Dictionary<string, string>();

                    foreach (TreeViewExplorer testcaseExplore in PlanExecution.Children)
                    {
                        bool? current3;
                        if ((((current3 = testcaseExplore.IsChecked) == true) || ((current3 = testcaseExplore.IsChecked) == null)) && (current3 = testcaseExplore.IsChecked) != false)
                        {
                            DeviceDiscovery.WriteToLogFile("Test case import is started : " + testcaseExplore.ItemName);

                             Progressvalue = (++Inc).ToString();
                            string oldTCID = testcaseExplore.ItemKey.ToString();

                            if (!tcRepeatedID.Keys.Contains(oldTCID))
                            {
                                string nameExistQuery = "select TestcaseID from Testcase where Testcasename =@caseName";
                                string ExisttcID = SelectSingleValue(nameExistQuery, connect, "@caseName", testcaseExplore.ItemName);
                                
                                if (ExisttcID == string.Empty || ExisttcID == null)
                                {
                                    if (!caseValues.Keys.Contains(testcaseExplore.ItemName))
                                    {
                                        if (!tcIDList.Keys.Contains(oldTCID))
                                        {
                                            tcIDList.Add(oldTCID, testcaseExplore.ItemName);
                                            tcIDWithMsgType.Add(oldTCID, string.Empty);
                                        }
                                    }
                                    else
                                    {
                                        DeviceDiscovery.WriteToLogFile("Message type is : " + caseValues[testcaseExplore.ItemName]);
                                    }
                                }
                                else
                                {
                                    if (caseValues.Keys.Contains(testcaseExplore.ItemName))
                                    {
                                        string caseMsgType = caseValues[testcaseExplore.ItemName];

                                        DeviceDiscovery.WriteToLogFile("Message type is : " + caseMsgType);

                                        if (caseMsgType == "CopyAndReplace")
                                        {
                                            List<string> deleteFile = new List<string>();

                                            try
                                            {
                                                string query = "select VerificationType from TestAction where TCID in (select TestcaseID from Testcase where Testcasename = @tcName) and (VerificationType = 'Audio Precision Verification' or  VerificationType = 'Responsalyzer'  or VerificationType='QRCM Verification')";
                                                DataTable verificationType = SelectDataTableValue(query, connect, "@tcName", testcaseExplore.ItemName);

                                                for (int i = 0; i < verificationType.Rows.Count; i++)
                                                {
                                                    if (verificationType.Rows[i][0].ToString() != null)
                                                    {
                                                        if (verificationType.Rows[i][0].ToString() == "Audio Precision Verification")
                                                        {
                                                            query = "select APxPath from APVerification where TCID in (select TestcaseID from Testcase where Testcasename = @tcName)";
                                                            DataTable oldapxFile = SelectDataTableValue(query, connect, "@tcName", testcaseExplore.ItemName);
                                                            for (int j = 0; j < oldapxFile.Rows.Count; j++)
                                                            {
                                                                if (oldapxFile.Rows[j][0] != null && oldapxFile.Rows[j][0].ToString() != string.Empty)
                                                                    deleteFile.Add(Path.Combine(QatConstants.QATServerPath, "Audio Precision", "AP Project Files", oldapxFile.Rows[j][0].ToString()));
                                                            }

                                                            query = "select WaveformType from APSettings where TCID in (select TestcaseID from Testcase where Testcasename = @tcName)";
                                                            DataTable settingsWaveform = SelectDataTableValue(query, connect, "@tcName", testcaseExplore.ItemName);
                                                            for (int j = 0; j < settingsWaveform.Rows.Count; j++)
                                                            {
                                                                if (settingsWaveform.Rows[j][0] != null && settingsWaveform.Rows[j][0].ToString() != string.Empty)
                                                                    deleteFile.Add(Path.Combine(QatConstants.QATServerPath, "Audio Precision", "AP Waveform Files", settingsWaveform.Rows[j][0].ToString()));
                                                            }

                                                            query = "select VerificationType from APVerification where TCID  in (select TestcaseID from Testcase where Testcasename = @tcName)";
                                                            DataTable verifyType = SelectDataTableValue(query, connect, "@tcName", testcaseExplore.ItemName);
                                                            for (int j = 0; j < verifyType.Rows.Count; j++)
                                                            {
                                                                if (verifyType.Rows[j][0] != null && verifyType.Rows[j][0].ToString() != string.Empty)
                                                                {
                                                                    string verifiType = verifyType.Rows[j][0].ToString();

                                                                    query = null;

                                                                    if (verifiType == "Level and Gain")
                                                                    {
                                                                        query = "select WaveformType from LevelAndGain where TCID in (select TestcaseID from Testcase where Testcasename = @tcName)";
                                                                    }
                                                                    else if (verifiType == "Frequency sweep")
                                                                    {
                                                                        query = "select VerificationLocation from APFrequencyResponse where TCID in (select TestcaseID from Testcase where Testcasename = @tcName)";
                                                                    }
                                                                    else if (verifiType == "Phase")
                                                                    {
                                                                        query = "select VerificationLocation from APPhaseSettings where TCID in (select TestcaseID from Testcase where Testcasename = @tcName)";
                                                                    }
                                                                    else if (verifiType == "Stepped Frequency Sweep")
                                                                    {
                                                                        query = "select VerificationLocation from APSteppedFreqSweepSettings where TCID in (select TestcaseID from Testcase where Testcasename = @tcName)";
                                                                    }
                                                                    else if (verifiType == "THD+N")
                                                                    {
                                                                        query = "select VerificationLocation from APTHDNSettings where TCID in (select TestcaseID from Testcase where Testcasename = @tcName)";
                                                                    }

                                                                    if (query != null && query != string.Empty)
                                                                    {
                                                                        DataTable val = SelectDataTableValue(query, connect, "@tcName", testcaseExplore.ItemName);
                                                                        for (int k = 0; k < val.Rows.Count; k++)
                                                                        {
                                                                            if (val.Rows[k][0] != null && val.Rows[k][0].ToString() != string.Empty)
                                                                            {
                                                                                if (verifiType == "Level and Gain")
                                                                                {
                                                                                    deleteFile.Add(Path.Combine(QatConstants.QATServerPath, "Audio Precision", "AP Waveform Files", val.Rows[k][0].ToString()));
                                                                                }
                                                                                else
                                                                                {
                                                                                    deleteFile.Add(Path.Combine(QatConstants.QATServerPath, "Audio Precision", "Verification Files", val.Rows[k][0].ToString()));
                                                                                }
                                                                            }
                                                                        }
                                                                    }
                                                                }
                                                            }
                                                        }
                                                        else if (verificationType.Rows[i][0].ToString() == "Responsalyzer")
                                                        {
                                                            query = "select VerificationFileLocation from Responsalyzer where TCID in (select TestcaseID from Testcase where Testcasename = @tcName)";
                                                            DataTable val = SelectDataTableValue(query, connect, "@tcName", testcaseExplore.ItemName);
                                                            for (int k = 0; k < val.Rows.Count; k++)
                                                            {
                                                                if (val.Rows[k][0] != null && val.Rows[k][0].ToString() != string.Empty)
                                                                {
                                                                    deleteFile.Add(Path.Combine(QatConstants.QATServerPath, "Responsalyzer", "Reference Files", val.Rows[k][0].ToString()));
                                                                }
                                                            }
                                                        }
                                                        ///////// Adding QRCM files list to delete --------Copy and replace option
                                                        else if (verificationType.Rows[i][0].ToString() == "QRCM Verification")
                                                        {
                                                            query = "select ReferenceFilePath from QRCMVerification where TCID in (select TestcaseID from Testcase where Testcasename = @tcName)";
                                                            DataTable val = SelectDataTableValue(query, connect, "@tcName", testcaseExplore.ItemName);
                                                            if(val.Rows.Count>0 &&  val.Rows[0][0] != null && val.Rows[0][0].ToString() != string.Empty)
                                                            {
                                                                string fileFullpath = Path.Combine(QatConstants.QATServerPath, "QRCM_Files", val.Rows[0][0].ToString());
                                                                if(!deleteFile.Contains(fileFullpath))
                                                                    deleteFile.Add(fileFullpath);
                                                            }
                                                        }
                                                    }
                                                }


                                                ///////// Adding QRCM files list to delete --------Copy and replace option                                             
                                                query = "select ActionType from TestAction where TCID in (select TestcaseID from Testcase where Testcasename = @tcName) and (ActionType = 'QRCM Action')";
                                                DataTable actionType = SelectDataTableValue(query, connect, "@tcName", testcaseExplore.ItemName);
                                                for (int i = 0; i < actionType.Rows.Count; i++)
                                                {
                                                    if (actionType.Rows[i][0].ToString() != null && actionType.Rows[i][0].ToString() == "QRCM Action")
                                                    {                                                       
                                                        query = "select PayloadFilePath from QRCMAction where TCID in (select TestcaseID from Testcase where Testcasename = @tcName)";
                                                        DataTable value = SelectDataTableValue(query, connect, "@tcName", testcaseExplore.ItemName);
                                                        if (value.Rows.Count > 0 && value.Rows[0][0] != null && value.Rows[0][0].ToString() != string.Empty)
                                                        {
                                                            string fileFullpath = Path.Combine(QatConstants.QATServerPath, "QRCM_Files", value.Rows[0][0].ToString());

                                                            ///// Skip adding, If file already added in  "QRCM Verification"
                                                            if (!deleteFile.Contains(fileFullpath))
                                                                deleteFile.Add(fileFullpath);
                                                        }
                                                    }
                                                }

                                                
                                            }
                                            catch { }

                                            if (ExisttcID != string.Empty)
                                            {
                                                bool rowtpexist = false;
                                                string querys = "select distinct TPID from TPTCLinkTable where TCID = " + ExisttcID;
                                                DataTable tbls = SelectDataTableValue(querys, connect, null, null);
                                                if (tbls.Rows.Count > 0)
                                                {
                                                    DataTableReader read = tbls.CreateDataReader();

                                                    while (read.Read())
                                                    {
                                                        if (ExistTPID != read.GetValue(0).ToString())
                                                        {
                                                            rowtpexist = true;
                                                            break;
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    rowtpexist = false;
                                                }

                                                if (rowtpexist == false)
                                                {
                                                    ////////Delete Tc
                                                    string tcdeleteQuery = "delete from Testcase where TestcaseID = " + ExisttcID;
                                                    InsertInToDB(tcdeleteQuery, connect, null, null);
                                                }
                                            }

                                            if (!tcIDList.Keys.Contains(oldTCID))
                                            {
                                                tcIDList.Add(oldTCID, testcaseExplore.ItemName);
                                                tcIDWithMsgType.Add(oldTCID, caseMsgType);
                                            }

                                            try
                                            {
                                                foreach (string filename in deleteFile)
                                                {
                                                    if (File.Exists(filename))
                                                    {
                                                        File.SetAttributes(filename, FileAttributes.Normal);
                                                        File.Delete(filename);
                                                    }
                                                }
                                            }
                                            catch { }

                                        }
                                        else if (caseMsgType == "KeepBoth")
                                        {
                                            dontdeleteTC.Add(ExisttcID);

                                            if (!tcIDList.Keys.Contains(oldTCID))
                                            {
                                                tcIDList.Add(oldTCID, testcaseExplore.ItemName);
                                                tcIDWithMsgType.Add(oldTCID, caseMsgType);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (!tcIDList.Keys.Contains(oldTCID))
                                        {
                                            tcIDList.Add(oldTCID, testcaseExplore.ItemName);
                                            tcIDWithMsgType.Add(oldTCID, string.Empty);
                                        }
                                    }
                                }
                            }
                            else
                            {
                                string tptcquery = "Insert into TPTCLinkTable values('" + newTPID + "','" + tcRepeatedID[oldTCID] + "')";
                                InsertInToDB(tptcquery, connect, null, null);
                            }

                            DeviceDiscovery.WriteToLogFile("Test case import is ended : " + testcaseExplore.ItemName);
                          
                        }
                    }

                    var tcdetail = UpdateImportTestCase(connect, oldTPID, newTPID, tcIDList, tcRepeatedID, tcIDWithMsgType, lengthyFiles);
                    tcRepeatedID = tcdetail.Item1;
                    lengthyFiles = tcdetail.Item2;

                    #endregion

                    #region TS TP linking

                    if (msgType == "CopyAndReplace" && ExistTPID != string.Empty && ExistTPID != null)
                    {
                        string query = "select TCID from TPTCLinkTable where TPID = " + ExistTPID;
                        DataTable tbl = SelectDataTableValue(query, connect, null, null);
                        if (tbl.Rows.Count > 0)
                        {
                            DataTableReader read1 = tbl.CreateDataReader();

                            while (read1.Read())
                            {
                                string tcid = read1.GetValue(0).ToString();

                                if (!dontdeleteTC.Contains(tcid))
                                {
                                    bool rowsexist = false;
                                    string tpquery = "select distinct TPID from TPTCLinkTable where TCID = " + tcid;
                                    DataTable tbls = SelectDataTableValue(tpquery, connect, null, null);
                                    if (tbls.Rows.Count > 0)
                                    {
                                        DataTableReader read = tbls.CreateDataReader();

                                        while (read.Read())
                                        {
                                            if (ExistTPID != read.GetValue(0).ToString())
                                            {
                                                rowsexist = true;
                                                break;
                                            }
                                        }
                                    }

                                    if (rowsexist == false)
                                    {
                                        ////////Delete Tc
                                        string tcdeleteQuery = "delete from Testcase where TestcaseID = " + tcid;
                                        InsertInToDB(tcdeleteQuery, connect, null, null);
                                    }
                                }
                            }
                        }
                        
                        string tstcQuery = "select TSID from TSTPLinkTable where TPID = " + ExistTPID;
                        DataTable TSIDs = SelectDataTableValue(tstcQuery, connect, null, null);
                        
                        string querys = "delete from Testplan where TestPlanID = " + ExistTPID;
                        InsertInToDB(querys, connect, null, null);

                        if (TSIDs.Rows.Count > 0)
                        {
                            DataTableReader read = TSIDs.CreateDataReader();
                            while (read.Read())
                            {
                                if (read[0] != System.DBNull.Value)
                                {
                                    string TSID = read.GetValue(0).ToString();

                                    string linkQuery = "Insert into TSTPLinkTable (TSID, TPID) values (" + TSID + "," + newTPID + ")";
                                    InsertInToDB(linkQuery, connect, null, null);
                                }
                            }
                        }
                    }

                    #endregion

                    TPwithnoActioncount(newTPID);
                }
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
            }

            return new Tuple<Dictionary<string, string>, List<string>, bool, Dictionary<string, string>>(tcRepeatedID, dontdeleteTC, msgTrue, lengthyFiles);
        }

        private Tuple<Dictionary<string,string>, Dictionary<string, string>> UpdateImportTestCase(SqlConnection connect, string oldTPID, string newTPID, Dictionary<string, string> oldTCIDWithCaseName, Dictionary<string, string> tcRepeatedID, Dictionary<string, string> msgType, Dictionary<string, string> lengthFileMsg)
        {
            Dictionary<string, string> testcaseRepeatedID = tcRepeatedID;

            try
            {
                List<string> tblItemChk = new List<string>();

                tblItemChk.Add("TestcaseID");
                tblItemChk.Add("TPID");

                XmlReaderSettings settings = new XmlReaderSettings
                {
                    ConformanceLevel = ConformanceLevel.Auto,
                    CheckCharacters = false,
                    IgnoreWhitespace = true
                };

                List<string> tpDBFilelds = new List<string>();
                Dictionary<string, string> newTCIDList = new Dictionary<string, string>();

                using (XmlReader xmlread = XmlReader.Create(filePath, settings))
                {
                    xmlread.ReadToDescendant("Testcase");

                    do
                    {
                        if (xmlread.NodeType == XmlNodeType.Element && xmlread.Name == "Testcase" && xmlread.HasAttributes && xmlread.GetAttribute("TestcaseID") != null && oldTCIDWithCaseName.Keys.Contains(xmlread.GetAttribute("TestcaseID")))
                        {
                            string oldTCIDs = string.Empty;
                            string oldName = string.Empty;
                            string newName = string.Empty;

                            Dictionary<string, string> ParameterValueWithFieldValue = new Dictionary<string, string>();
                            List<string> ParameterValue = new List<string>();
                            List<string> fieldsvalue = new List<string>();

                            for (int i = 0; i < xmlread.AttributeCount; i++)
                            {
                                xmlread.MoveToAttribute(i);

                                if (xmlread.Name == "TestcaseID")
                                {
                                    oldTCIDs = xmlread.Value;
                                }

                                if (!tblItemChk.Contains(xmlread.Name))
                                {
                                    ParameterValueWithFieldValue.Add(xmlread.Name, "@" + xmlread.Name);

                                    string value = xmlread.Value;

                                    if (xmlread.Name != "ImportedBy" && xmlread.Name != "ImportedOn" && xmlread.Name == "Testcasename" && (msgType[oldTCIDs] == "KeepBoth" || (msgType[oldTCIDs] == "CopyAndReplace")))
                                    {
                                        string nameexistquery = "select " + "Testcasename" + " from " + "Testcase" + " where " + "Testcasename" + " like '%' + @ItemName + '%'";
                                        value = CreateCopyItemName(connect, nameexistquery, "@ItemName", value);

                                        oldName = xmlread.Value;
                                        newName = value;
                                    }

                                    if(xmlread.Name == "ImportedBy")
                                    {
                                        value = Properties.Settings.Default.TesterName;
                                    }

                                    if(xmlread.Name == "ImportedOn")
                                    {
                                        value = DateTime.Now.ToString("MM-dd-yyyy hh:mm:ss tt");
                                        //value = DateTime.Now.ToString();
                                    }

                                    ParameterValue.Add(value);
                                }



                                fieldsvalue.Add(xmlread.Name);
                            }

                            string columnQuery = "select COLUMN_NAME from INFORMATION_SCHEMA.COLUMNS where TABLE_NAME = '" + "Testcase" + "'";
                            DataTable table = SelectDataTableValue(columnQuery, connect, null, null);
                            tpDBFilelds = table.Rows.OfType<DataRow>().Select(dr => dr.Field<string>(0)).ToList();


                            List<string> dbdifference = tpDBFilelds.Except(fieldsvalue).ToList();
                            List<string> importdifference = fieldsvalue.Except(tpDBFilelds).ToList();

                            foreach (string dif in importdifference)
                            {
                                ParameterValueWithFieldValue.Remove(dif);
                            }

                            string field = string.Join(",", ParameterValueWithFieldValue.Keys);
                            string val = string.Join(",", ParameterValueWithFieldValue.Values);

                            if (field != string.Empty && field != null)
                            {
                                string query = "Insert into Testcase (TPID," + field + ") values (" + newTPID + "," + val + ");SELECT CONVERT(int,SCOPE_IDENTITY())";
                                string newTCIDs = InsertInToDB(query, connect, ParameterValueWithFieldValue.Values.ToList(), ParameterValue);
                                newTCIDList.Add(oldTCIDs, newTCIDs);
                                testcaseRepeatedID.Add(oldTCIDs, newTCIDs);
                            }
                        }
                    } while (xmlread.Read());
                }


                if (newTCIDList.Count > 0)
                {
                    tblItemChk.Clear();
                    tblItemChk.Add("TCID");
                    tblItemChk.Add("TPID");
                    tblItemChk.Add("TPTCID");

                    using (XmlReader xmlread = XmlReader.Create(filePath, settings))
                    {
                        xmlread.ReadToDescendant("TPTCLinkTable");

                        do
                        {
                            if (xmlread.NodeType == XmlNodeType.Element && xmlread.Name == "TPTCLinkTable" && xmlread.HasAttributes && xmlread.GetAttribute("TPID") != null && xmlread.GetAttribute("TPID") == oldTPID && xmlread.GetAttribute("TCID") != null && oldTCIDWithCaseName.Keys.Contains(xmlread.GetAttribute("TCID")))
                            {
                                Dictionary<string, string> ParameterValueWithFieldValue = new Dictionary<string, string>();
                                List<string> ParameterValue = new List<string>();
                                List<string> fieldsvalue = new List<string>();
                                string oldTCIDs = string.Empty;

                                for (int i = 0; i < xmlread.AttributeCount; i++)
                                {
                                    xmlread.MoveToAttribute(i);

                                    if (!tblItemChk.Contains(xmlread.Name))
                                    {
                                        ParameterValueWithFieldValue.Add(xmlread.Name, "@" + xmlread.Name);

                                        ParameterValue.Add(xmlread.Value);
                                    }

                                    if (xmlread.Name == "TCID")
                                    {
                                        oldTCIDs = xmlread.Value;
                                    }

                                    fieldsvalue.Add(xmlread.Name);
                                }

                                string columnQuery = "select COLUMN_NAME from INFORMATION_SCHEMA.COLUMNS where TABLE_NAME = '" + "TPTCLinkTable" + "'";
                                DataTable table = SelectDataTableValue(columnQuery, connect, null, null);
                                List<string> tpDBFileldss = table.Rows.OfType<DataRow>().Select(dr => dr.Field<string>(0)).ToList();

                                List<string> dbdifference = tpDBFileldss.Except(fieldsvalue).ToList();
                                List<string> importdifference = fieldsvalue.Except(tpDBFileldss).ToList();

                                foreach (string dif in importdifference)
                                {
                                    ParameterValueWithFieldValue.Remove(dif);
                                }

                                string field = string.Join(",", ParameterValueWithFieldValue.Keys);
                                string val = string.Join(",", ParameterValueWithFieldValue.Values);



                                if (field != string.Empty && field != null)
                                {
                                    string query = "Insert into TPTCLinkTable (TPID, TCID, " + field + ") values (" + newTPID + "," + newTCIDList[oldTCIDs] + "," + val + ")";
                                    InsertInToDB(query, connect, ParameterValueWithFieldValue.Values.ToList(), ParameterValue);
                                }
                                else
                                {
                                    string query = "Insert into TPTCLinkTable (TPID, TCID) values (" + newTPID + "," + newTCIDList[oldTCIDs] + ")";
                                    InsertInToDB(query, connect, ParameterValueWithFieldValue.Values.ToList(), ParameterValue);
                                }
                            }
                        } while (xmlread.Read());
                    }

                    tblItemChk.Clear();
                    tblItemChk.Add("TestActionID");
                    tblItemChk.Add("TCID");

                    var directorycreate = new DirectoryInfo(@"C:\Users\" + Environment.UserName + @"\AppData\Local\Temp\QATImport");
                    string[] fileType = new string[5] { "Sine", "Sine, Dual", "Sine, Var Phase", "Noise", "IMD" };

                    Dictionary<string, string> actionList = new Dictionary<string, string>();
                    Dictionary<string, string> actionwithCaseOldList = new Dictionary<string, string>();

                    using (XmlReader xmlreader = XmlReader.Create(filePath, settings))
                    {
                        xmlreader.ReadToDescendant("TestAction");

                        do
                        {
                            if (xmlreader.NodeType == XmlNodeType.Element && xmlreader.Name == "TestAction" && xmlreader.HasAttributes && xmlreader.GetAttribute("TCID") != null && oldTCIDWithCaseName.Keys.Contains(xmlreader.GetAttribute("TCID")))
                            {
                                Dictionary<string, string> ParameterValueWithFieldValue = new Dictionary<string, string>();
                                List<string> ParameterValue = new List<string>();
                                List<string> fieldsvalue = new List<string>();
                                string oldTCIDs = string.Empty;
                                string oldActionID = string.Empty;
                                string newActionID = string.Empty;
                                string actionType = string.Empty;
                                string verifyType = string.Empty;

                                for (int i = 0; i < xmlreader.AttributeCount; i++)
                                {
                                    xmlreader.MoveToAttribute(i);

                                    if (!tblItemChk.Contains(xmlreader.Name))
                                    {
                                        ParameterValueWithFieldValue.Add(xmlreader.Name, "@" + xmlreader.Name);

                                        ParameterValue.Add(xmlreader.Value);
                                    }

                                    if (xmlreader.Name == "TCID")
                                        oldTCIDs = xmlreader.Value;
                                    if (xmlreader.Name == "TestActionID")
                                        oldActionID = xmlreader.Value;

                                    if (xmlreader.Name == "ActionType")
                                        actionType = xmlreader.Value;
                                    else if (xmlreader.Name == "VerificationType")
                                        verifyType = xmlreader.Value;

                                    fieldsvalue.Add(xmlreader.Name);
                                }

                                string columnQuery = "select COLUMN_NAME from INFORMATION_SCHEMA.COLUMNS where TABLE_NAME = '" + "TestAction" + "'";
                                DataTable table = SelectDataTableValue(columnQuery, connect, null, null);
                                List<string> tpDBFileldsss = table.Rows.OfType<DataRow>().Select(dr => dr.Field<string>(0)).ToList();

                                List<string> dbdifference = tpDBFileldsss.Except(fieldsvalue).ToList();
                                List<string> importdifference = fieldsvalue.Except(tpDBFileldsss).ToList();

                                foreach (string dif in importdifference)
                                {
                                    ParameterValueWithFieldValue.Remove(dif);
                                }

                                string field = string.Join(",", ParameterValueWithFieldValue.Keys);
                                string val = string.Join(",", ParameterValueWithFieldValue.Values);

                                string newTCID = newTCIDList[oldTCIDs];

                                if (field != string.Empty && field != null)
                                {
                                    string query = "Insert into TestAction (TCID, " + field + ") values (" + newTCID + "," + val + ");SELECT CONVERT(int, SCOPE_IDENTITY())";
                                    newActionID = InsertInToDB(query, connect, ParameterValueWithFieldValue.Values.ToList(), ParameterValue);
                                }
                                else
                                {
                                    string query = "Insert into TestAction (TCID) values (" + newTCID + ");SELECT CONVERT(int, SCOPE_IDENTITY())";
                                    newActionID = InsertInToDB(query, connect, ParameterValueWithFieldValue.Values.ToList(), ParameterValue);
                                }

                                actionList.Add(oldActionID, newActionID);
                                actionwithCaseOldList.Add(oldActionID, oldTCIDs);
                            }
                        } while (xmlreader.Read());
                    }

                    lengthFileMsg = InsertXMLBulk(connect, newTCIDList, actionList, actionwithCaseOldList, "ControlAction", "ControlActionID", "TCID", "ActionID", lengthFileMsg, oldTCIDWithCaseName);

                }
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
            }

            return new Tuple<Dictionary<string, string>, Dictionary<string, string>>(testcaseRepeatedID, lengthFileMsg);
        }

        private string TruncateDesignName(string filePath, string planName, string dontTruncateString, string truncateString)
        {
            string newFilePath = string.Empty;

            try
            {
                try
                {
                    string filepathLength = Path.GetFullPath(filePath);
                }
                catch (Exception ex)
                {
                    if (ex.Message.Contains("The specified path, file name, or both are too long."))
                    {
                        int deletedDiff = filePath.Length - 259;

                        if (truncateString.Length > deletedDiff)
                        {
                            string sss = dontTruncateString + truncateString.Remove(truncateString.Length - deletedDiff) + ".qsys";

                            string[] sdss = filePath.Split(Path.DirectorySeparatorChar);

                            string[] newFile = sdss.Take(sdss.Count() - 1).ToArray();

                            string fff = String.Join("//", newFile);
                            newFilePath = Path.Combine(fff, sss);
                        }
                    }
                }
            }
            catch(Exception ex)
            {

            }

            return newFilePath;
        }

        private Tuple<string, Dictionary<string, string>> CreateServerFiles(DirectoryInfo directorycreate, string dummyName, string actualName, string filepath1, string filepath2, string oldTCID, string oldActionID, string newTCID, string newActionID, string oldRespID, string newRespID, Dictionary<string, string> lengthFileMsg, string testCaseName)
        {
            string editedFileName = string.Empty;

            try
            {
                string filename = editedFileName = actualName;
                string extension = Path.GetExtension(filename);

                int len = 0;
                if (filepath1 == "Responsalyzer" && filename.EndsWith("_" + oldTCID + "_" + oldActionID + "_" + oldRespID + extension))
                {
                    len = ("_" + oldTCID + "_" + oldActionID + "_" + oldRespID + extension).Length;
                    editedFileName = filename.Remove(filename.Length - len, len) + extension;
                }
                else if(filename.EndsWith("_" + oldTCID + "_" + oldActionID + extension))
                {
                    len = ("_" + oldTCID + "_" + oldActionID + extension).Length;
                    editedFileName = filename.Remove(filename.Length - len, len) + extension;
                }

                if (editedFileName.EndsWith(extension))
                {
                    string names = Path.GetFileNameWithoutExtension(editedFileName);
                    if(filepath1 == "Responsalyzer")
                    {
                        editedFileName = names + "_" + newTCID + "_" + newActionID + "_" + newRespID  + extension; 
                    }
                    else
                    {
                        editedFileName = names + "_" + newTCID + "_" + newActionID + extension;  
                    }
                }

                try
                {
                    string apPath = Path.Combine(directorycreate.FullName, filepath1, filepath2);

                    string serverPath = QatConstants.QATServerPath;

                    if (Directory.Exists(apPath))
                    {
                        Directory.CreateDirectory(apPath);
                    }

                    string fileWithPath = Path.Combine(apPath, dummyName);

                    if (File.Exists(fileWithPath))
                    {
                        string serverPathWithoutFile = string.Empty;
                        string serverwithdesignPath = serverPathWithoutFile = Path.Combine(serverPath, filepath1, filepath2);

                        if (!Directory.Exists(serverwithdesignPath))
                        {
                            Directory.CreateDirectory(serverwithdesignPath);
                        }

                        serverwithdesignPath = Path.Combine(serverwithdesignPath, editedFileName);

                        string truncatedFilePath = TruncateTestcaseFiles(fileWithPath, serverwithdesignPath, editedFileName, newTCID, newActionID, newRespID, oldRespID, serverPathWithoutFile);

                        if (truncatedFilePath != null && truncatedFilePath != string.Empty)
                        {
                            if (File.Exists(truncatedFilePath))
                            {
                                FileInfo fileInfo = new FileInfo(truncatedFilePath);
                                fileInfo.IsReadOnly = false;
                            }

                            File.Copy(fileWithPath, truncatedFilePath, true);
                            FileInfo fileInformations = new FileInfo(truncatedFilePath);
                            fileInformations.IsReadOnly = true;

                            editedFileName = Path.GetFileName(truncatedFilePath);
                        }
                        else
                        {
                            if (File.Exists(serverwithdesignPath))
                            {
                                FileInfo fileInfo = new FileInfo(serverwithdesignPath);
                                fileInfo.IsReadOnly = false;
                            }

                            File.Copy(fileWithPath, serverwithdesignPath, true);
                            FileInfo fileInformations = new FileInfo(serverwithdesignPath);
                            fileInformations.IsReadOnly = true;
                        }
                    }
                    else
                    {
                        DeviceDiscovery.WriteToLogFile("File copy is failed : " + actualName);
                    }
                }
                catch (Exception ex)
                {
                    if (ex.Message.Contains("The specified path, file name, or both are too long"))
                    {
                        /////////////////MessageBox

                        if (!lengthFileMsg.Keys.Contains(editedFileName))
                            lengthFileMsg.Add(editedFileName, testCaseName);
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

            return new Tuple<string, Dictionary<string, string>>(editedFileName, lengthFileMsg);
        }
                 
        private string TruncateTestcaseFiles(string sourceFilePath, string destinationPath, string correctfileName, string newTCID, string newActionID, string newRespID,string oldRespID,string serverPathWithoutFile)
        {
            string truncatedFilePath = string.Empty;

            try
            {
                string filepathLength = Path.GetFullPath(destinationPath);
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("The specified path, file name, or both are too long."))
                {
                    truncatedFilePath = TruncateTestCaseFilesetting(sourceFilePath,destinationPath, correctfileName, newTCID, newActionID, newRespID, oldRespID, serverPathWithoutFile);
                }
            }

            return truncatedFilePath;     
        }

        private string TruncateTestCaseFilesetting(string sourceFilePath, string destinationPath, string correctfileName, string newTCID, string newActionID, string newRespID, string oldRespID, string serverPathWithoutFile)
        {
            string truncatedFilePath = string.Empty;

            try
            {
                string fileName = Path.GetFileName(sourceFilePath);
                string extension = Path.GetExtension(correctfileName);

                string endsIDremoved = string.Empty;

                if (oldRespID != null)
                {
                    if (correctfileName.EndsWith("_" + newTCID + "_" + newActionID + "_" + newRespID + extension))
                    {
                        int len = ("_" + newTCID + "_" + newActionID + "_" + newRespID + extension).Length;
                        endsIDremoved = correctfileName.Remove(correctfileName.Length - len, len);
                    }
                }
                else
                {
                    if (correctfileName.EndsWith("_" + newTCID + "_" + newActionID + extension))
                    {
                        int len = ("_" + newTCID + "_" + newActionID + extension).Length;
                        endsIDremoved = correctfileName.Remove(correctfileName.Length - len, len);
                    }
                }

                int deletedlength = destinationPath.Length - 259;

                if(endsIDremoved.Length > deletedlength)
                {
                    string truncatedfileName = string.Empty;

                    if (oldRespID != null)
                    {
                        truncatedfileName = endsIDremoved.Remove(endsIDremoved.Length - deletedlength) + "_" + newTCID + "_" + newActionID + "_" + newRespID + extension;
                    }
                    else  
                    {
                        truncatedfileName = endsIDremoved.Remove(endsIDremoved.Length - deletedlength) + "_" + newTCID + "_" + newActionID + extension;    
                    }

                    truncatedFilePath = Path.Combine(serverPathWithoutFile, truncatedfileName);
                }
            }
            catch(Exception ex)
            {

            }

            return truncatedFilePath;
        }

        private Tuple<Dictionary<string, string>, List<string>, bool> ComparexmlWithDB(string oldID, SqlConnection connect, List<string> tblItemChk, string tblName, string tblItemKey, string oldTPName, string newTPName, string newDesignName)
        {
            bool chkDataExist = false;
            List<string> fieldsvalue = new List<string>();
            List<string> ParameterValue = new List<string>();
            Dictionary<string, string> ParameterValueWithFieldValue = new Dictionary<string, string>();

            try
            {
                XmlReaderSettings settings = new XmlReaderSettings
                {
                    ConformanceLevel = ConformanceLevel.Auto,
                    CheckCharacters = false,
                    IgnoreWhitespace = true
                };

                using (XmlReader xmlread = XmlReader.Create(filePath, settings))
                {
                    xmlread.ReadToDescendant(tblName);
                    do
                    {
                        if (xmlread.NodeType == XmlNodeType.Element && xmlread.Name == tblName && xmlread.HasAttributes && xmlread.GetAttribute(tblItemKey) != null && xmlread.GetAttribute(tblItemKey) == oldID)
                        {
                            for (int i = 0; i < xmlread.AttributeCount; i++)
                            {
                                xmlread.MoveToAttribute(i);

                                if (!tblItemChk.Contains(xmlread.Name))
                                {
                                    ParameterValueWithFieldValue.Add(xmlread.Name, "@" + xmlread.Name);

                                    string value = xmlread.Value;

                                    if (tblName == "designtable" && xmlread.Name == "Designname" && newDesignName != null && newDesignName != string.Empty)
                                    {
                                        value = newDesignName;
                                    }

                                    ParameterValue.Add(value);
                                }

                                fieldsvalue.Add(xmlread.Name);
                            }

                            chkDataExist = true;
                            break;
                        }
                    } while (xmlread.Read());
                }

                if (chkDataExist == true)
                {
                    string columnQuery = "select COLUMN_NAME from INFORMATION_SCHEMA.COLUMNS where TABLE_NAME = '" + tblName + "'";
                    DataTable table = SelectDataTableValue(columnQuery, connect, null, null);
                    List<string> tpDBFilelds = table.Rows.OfType<DataRow>().Select(dr => dr.Field<string>(0)).ToList();

                    List<string> dbdifference = tpDBFilelds.Except(fieldsvalue).ToList();
                    List<string> importdifference = fieldsvalue.Except(tpDBFilelds).ToList();

                    foreach (string dif in importdifference)
                    {
                        ParameterValueWithFieldValue.Remove(dif);
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

            return new Tuple<Dictionary<string, string>, List<string>, bool>(ParameterValueWithFieldValue, ParameterValue, chkDataExist);
        }
        
        private Tuple<Dictionary<string, string>, List<string>, bool, string, string, List<string>> ComparexmlWithDBTCTP(string filePath, string oldID, SqlConnection connect, List<string> tblItemChk, string tblName, string tblItemKey, string tblItemName, string msgType, bool isNew)
        {
            bool chkDataExist = false;
            List<string> fieldsvalue = new List<string>();
            List<string> ParameterValue = new List<string>();
            Dictionary<string, string> ParameterValueWithFieldValue = new Dictionary<string, string>();
            string oldName = string.Empty;
            string newName = string.Empty;
            List<string> tpDBFilelds = new List<string>();

            try
            {
                XmlReaderSettings settings = new XmlReaderSettings
                {
                    ConformanceLevel = ConformanceLevel.Auto,
                    CheckCharacters = false,
                    IgnoreWhitespace = true
                };

                using (XmlReader xmlread = XmlReader.Create(filePath, settings))
                {
                    xmlread.ReadToDescendant(tblName);

                    //while (xmlread.Read())
                    //{

                    do
                    {
                        if (xmlread.NodeType == XmlNodeType.Element && xmlread.Name == tblName && xmlread.HasAttributes && xmlread.GetAttribute(tblItemKey) != null)
                        {
                            string valu = xmlread.GetAttribute(tblItemKey);
                            if (valu == oldID.ToString())
                            {
                                for (int i = 0; i < xmlread.AttributeCount; i++)
                                {
                                    xmlread.MoveToAttribute(i);

                                    if (!tblItemChk.Contains(xmlread.Name) && xmlread.Name != "ImportedBy" && xmlread.Name != "ImportedOn")
                                    {
                                        ParameterValueWithFieldValue.Add(xmlread.Name, "@" + xmlread.Name);

                                        string value = xmlread.Value;

                                        if (xmlread.Name == tblItemName && (msgType == "KeepBoth" || (msgType == "CopyAndReplace" && isNew == false)))
                                        {
                                            string nameexistquery = "select " + tblItemName + " from " + tblName + " where " + tblItemName + " like '%' + @ItemName + '%'";
                                            value = CreateCopyItemName(connect, nameexistquery, "@ItemName", value);

                                            newName = value;
                                        }

                                        if (xmlread.Name == tblItemName)
                                        {
                                            oldName = xmlread.Value;
                                        }

                                        ParameterValue.Add(value);
                                    }

                                    fieldsvalue.Add(xmlread.Name);
                                }

                                chkDataExist = true;
                                break;
                            }
                        }
                    }
                    while (xmlread.Read());

                        //}
                    }

                if (chkDataExist == true)
                {
                    string columnQuery = "select COLUMN_NAME from INFORMATION_SCHEMA.COLUMNS where TABLE_NAME = '" + tblName + "'";
                    DataTable table = SelectDataTableValue(columnQuery, connect, null, null);
                    tpDBFilelds = table.Rows.OfType<DataRow>().Select(dr => dr.Field<string>(0)).ToList();


                    List<string> dbdifference = tpDBFilelds.Except(fieldsvalue).ToList();
                    List<string> importdifference = fieldsvalue.Except(tpDBFilelds).ToList();

                    foreach (string dif in importdifference)
                    {
                        ParameterValueWithFieldValue.Remove(dif);
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

            return new Tuple<Dictionary<string, string>, List<string>, bool, string, string, List<string>>(ParameterValueWithFieldValue, ParameterValue, chkDataExist, oldName, newName, tpDBFilelds);
        } 
      
        private Tuple<Dictionary<string, string>, List<string>, bool, string, string> ComparexmlWithDBFor2Query(string oldTPID, string oldTCID, SqlConnection connect, List<string> tblItemChk, string tblName, string tblTPItemKey, string tblTCItemKey)
        {
            bool chkDataExist = false;
            List<string> fieldsvalue = new List<string>();
            List<string> ParameterValue = new List<string>();
            Dictionary<string, string> ParameterValueWithFieldValue = new Dictionary<string, string>();
            string actionType = string.Empty;
            string verifyType = string.Empty;

            try
            {
                XmlReaderSettings settings = new XmlReaderSettings
                {
                    ConformanceLevel = ConformanceLevel.Auto,
                    CheckCharacters = false,
                    IgnoreWhitespace = true
                };

                using (XmlReader xmlread = XmlReader.Create(filePath, settings))
                {
                    xmlread.ReadToDescendant(tblName);

                    do
                    {
                        if (xmlread.NodeType == XmlNodeType.Element && xmlread.Name == tblName && xmlread.HasAttributes && xmlread.GetAttribute(tblTPItemKey) != null && xmlread.GetAttribute(tblTPItemKey) == oldTPID && xmlread.GetAttribute(tblTCItemKey) != null && xmlread.GetAttribute(tblTCItemKey) == oldTCID)
                        {
                            for (int i = 0; i < xmlread.AttributeCount; i++)
                            {
                                xmlread.MoveToAttribute(i);

                                if (!tblItemChk.Contains(xmlread.Name))
                                {
                                    ParameterValueWithFieldValue.Add(xmlread.Name, "@" + xmlread.Name);

                                    ParameterValue.Add(xmlread.Value);
                                }

                                if (xmlread.Name == "ActionType")
                                    actionType = xmlread.Value;
                                else if (xmlread.Name == "VerificationType")
                                    verifyType = xmlread.Value;

                                fieldsvalue.Add(xmlread.Name);
                            }

                            chkDataExist = true;
                            break;
                        }
                    } while (xmlread.Read());
                }

                if (chkDataExist == true)
                {
                    string columnQuery = "select COLUMN_NAME from INFORMATION_SCHEMA.COLUMNS where TABLE_NAME = '" + tblName + "'";
                    DataTable table = SelectDataTableValue(columnQuery, connect, null, null);
                    List<string> tpDBFilelds = table.Rows.OfType<DataRow>().Select(dr => dr.Field<string>(0)).ToList();

                    List<string> dbdifference = tpDBFilelds.Except(fieldsvalue).ToList();
                    List<string> importdifference = fieldsvalue.Except(tpDBFilelds).ToList();

                    foreach (string dif in importdifference)
                    {
                        ParameterValueWithFieldValue.Remove(dif);
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

            return new Tuple<Dictionary<string, string>, List<string>, bool, string, string>(ParameterValueWithFieldValue, ParameterValue, chkDataExist, actionType, verifyType);
        }
        
        public void InsertBulkData(SqlConnection connect, string oldDesignID, string newDesignID, string tblName, string tblKey)
        {
            try
            {
                XmlReaderSettings settings = new XmlReaderSettings
                {
                    ConformanceLevel = ConformanceLevel.Auto,
                    CheckCharacters = false,
                    IgnoreWhitespace = true
                };

                DataTable tbl = new DataTable();
                bool val = false;

                using (XmlReader xmlread = XmlReader.Create(filePath, settings))
                {
                    xmlread.ReadToDescendant(tblName);

                    do
                    {
                        if (xmlread.NodeType == XmlNodeType.Element && xmlread.Name == tblName && xmlread.HasAttributes && xmlread.GetAttribute(tblKey) != null && xmlread.GetAttribute(tblKey) == oldDesignID)
                        {
                            if (val == false)
                            {
                                for (int j = 0; j < xmlread.AttributeCount; j++)
                                {
                                    val = true;
                                    xmlread.MoveToAttribute(j);

                                    if (xmlread.Name == tblKey)
                                    {
                                        tbl.Columns.Add(new DataColumn(xmlread.Name, typeof(SqlInt32)));
                                    }
                                    else
                                    {
                                        tbl.Columns.Add(new DataColumn(xmlread.Name, typeof(string)));
                                    }
                                }
                            }

                            DataRow dr = tbl.NewRow();
                            dr.BeginEdit();

                            for (int attId = 0; attId < xmlread.AttributeCount; attId++)
                            {
                                xmlread.MoveToAttribute(attId);

                                if (xmlread.Name == tblKey)
                                {
                                    dr[xmlread.Name] = Convert.ToInt32(newDesignID);
                                }
                                else
                                {
                                    dr[xmlread.Name] = xmlread.Value;
                                }
                            }


                            dr.EndEdit();

                            tbl.Rows.Add(dr);
                        }
                    } while (xmlread.Read());

                    if (xmlread != null)
                    {
                        xmlread.Dispose();
                        xmlread.Close();
                    }
                }

                List<string> columnNames = tbl.Columns.Cast<DataColumn>().Select(x => x.ColumnName).ToList();

                string columnQuery = "select COLUMN_NAME from INFORMATION_SCHEMA.COLUMNS where TABLE_NAME = '" + tblName + "'";
                DataTable table = SelectDataTableValue(columnQuery, connect, null, null);
                List<string> tpDBFilelds = table.Rows.OfType<DataRow>().Select(dr => dr.Field<string>(0)).ToList();

                List<string> dbdifference = tpDBFilelds.Except(columnNames).ToList();
                List<string> importdifference = columnNames.Except(tpDBFilelds).ToList();

                foreach (string dif in importdifference)
                {
                    tbl.Columns.Remove(dif);
                }

                using (SqlBulkCopy bulkcopy = new SqlBulkCopy(connect))
                {
                    bulkcopy.DestinationTableName = "dbo." + tblName;
                    bulkcopy.WriteToServer(tbl);
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
          
        public void InsertBulkDataForTCInit(SqlConnection connect, string oldDesignID, string newDesignID, string tblName, string tblKey)
        {
            try
            {
                XmlReaderSettings settings = new XmlReaderSettings
                {
                    ConformanceLevel = ConformanceLevel.Auto,
                    CheckCharacters = false
                };

                DataTable tbl_TCInit = new DataTable();
                bool val = false;
                using (reader = XmlReader.Create(importTCinitPath, settings))
                {
                    reader.ReadStartElement("TCInitialization");

                    do
                    {
                        if (reader.NodeType == XmlNodeType.Element)
                        {
                            if (reader.HasAttributes && reader.GetAttribute(tblKey) != null && reader.GetAttribute(tblKey) == oldDesignID)
                            {
                                if (val == false)
                                {
                                    for (int j = 0; j < reader.AttributeCount; j++)
                                    {
                                        val = true;
                                        reader.MoveToAttribute(j);

                                        if (reader.Name == tblKey)
                                        {
                                            tbl_TCInit.Columns.Add(new DataColumn(reader.Name, typeof(SqlInt32)));
                                        }
                                        else
                                        {
                                            tbl_TCInit.Columns.Add(new DataColumn(reader.Name, typeof(string)));
                                        }
                                    }
                                }

                                DataRow dr = tbl_TCInit.NewRow();
                                dr.BeginEdit();

                                for (int attId = 0; attId < reader.AttributeCount; attId++)
                                {
                                    reader.MoveToAttribute(attId);

                                    if (reader.Name == tblKey)
                                    {
                                        dr[reader.Name] = Convert.ToInt32(newDesignID);
                                    }
                                    else
                                    {
                                        dr[reader.Name] = reader.Value;
                                    }
                                }


                                dr.EndEdit();

                                tbl_TCInit.Rows.Add(dr);
                            }
                        }
                    } while (reader.Read());

                    if (reader != null)
                    {
                        reader.Dispose();
                        reader.Close();
                    }
                }

                List<string> columnNames = tbl_TCInit.Columns.Cast<DataColumn>().Select(x => x.ColumnName).ToList();

                string columnQuery = "select COLUMN_NAME from INFORMATION_SCHEMA.COLUMNS where TABLE_NAME = '" + tblName + "'";
                DataTable table = SelectDataTableValue(columnQuery, connect, null, null);
                List<string> tpDBFilelds = table.Rows.OfType<DataRow>().Select(dr => dr.Field<string>(0)).ToList();

                List<string> dbdifference = tpDBFilelds.Except(columnNames).ToList();
                List<string> importdifference = columnNames.Except(tpDBFilelds).ToList();

                foreach (string dif in importdifference)
                {
                    tbl_TCInit.Columns.Remove(dif);
                }

                using (SqlBulkCopy bulkcopy = new SqlBulkCopy(connect))
                {
                    bulkcopy.DestinationTableName = "dbo." + tblName;
                    bulkcopy.WriteToServer(tbl_TCInit);
                }
            }
            catch (Exception ex)
            {
                if (reader != null)
                {
                    reader.Dispose();
                    reader.Close();
                }

                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
            }
        }

        private Tuple<bool, DataTable> ReadXMLValues(bool tblHeaderExist, XmlReader xmlread, DataTable tbl, Dictionary<string,string> oldTCIDwithnewTCID, Dictionary<string,string> oldActionIDwithNewActionID)
        {
            try
            {
                if (tblHeaderExist == false)
                {
                    for (int j = 0; j < xmlread.AttributeCount; j++)
                    {
                        tblHeaderExist = true;
                        xmlread.MoveToAttribute(j);

                        if (xmlread.Name != "TCID" && xmlread.Name != "ActionID")
                        {
                            tbl.Columns.Add(new DataColumn(xmlread.Name, typeof(string)));
                        }
                        else
                        {
                            tbl.Columns.Add(new DataColumn(xmlread.Name, typeof(SqlInt32)));
                        }
                    }
                }

                DataRow dr = tbl.NewRow();
                dr.BeginEdit();

                for (int attId = 0; attId < xmlread.AttributeCount; attId++)
                {
                    xmlread.MoveToAttribute(attId);

                    if (xmlread.Name != "TCID" && xmlread.Name != "ActionID")
                    {
                        dr[xmlread.Name] = xmlread.Value;
                    }
                    else
                    {
                        if (xmlread.Name == "TCID")
                            dr[xmlread.Name] = Convert.ToInt32(oldTCIDwithnewTCID[xmlread.Value]);
                        else if (xmlread.Name == "ActionID")
                            dr[xmlread.Name] = Convert.ToInt32(oldActionIDwithNewActionID[xmlread.Value]);
                    }
                }

                dr.EndEdit();

                tbl.Rows.Add(dr);
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
            }

            return new Tuple<bool, DataTable>(tblHeaderExist, tbl);
        }

        private Tuple<bool, DataTable, Dictionary<string,string>> ReadDatasFromXML(bool Verify, XmlReader xmlread, DataTable tbl, Dictionary<string,string> oldTCIDwithnewTCID, Dictionary<string,string> oldActionIDwithNewActionID, Dictionary<string,string> oldTCIDWithCaseName,Dictionary<string,string> lengthFileMsg, int result, string FiletypeFileld, string tblID, string par1, string par2, string xmlHeader)
        {
            try
            {
                string[] fileType = new string[5] { "Sine", "Sine, Dual", "Sine, Var Phase", "Noise", "IMD" };

                if (Verify == false)
                {
                    for (int j = 0; j < xmlread.AttributeCount; j++)
                    {
                        Verify = true;
                        xmlread.MoveToAttribute(j);

                        if (xmlread.Name != "TCID" && xmlread.Name != "ActionID")
                        {
                            tbl.Columns.Add(new DataColumn(xmlread.Name, typeof(string)));
                        }
                        else
                        {
                            tbl.Columns.Add(new DataColumn(xmlread.Name, typeof(SqlInt32)));
                        }
                    }
                }

                DataRow dr = tbl.NewRow();
                dr.BeginEdit();

                string oldTCID = string.Empty;
                string newTCID = string.Empty;
                string oldActionID = string.Empty;
                string newActionID = string.Empty;
                string oldapsettingsID = string.Empty;

                for (int attId = 0; attId < xmlread.AttributeCount; attId++)
                {
                    xmlread.MoveToAttribute(attId);
                    string apverify = string.Empty;

                    if (xmlread.Name == FiletypeFileld && xmlread.Value != null)
                    {
                        if (FiletypeFileld == "WaveformType")
                        {
                            if (!fileType.Contains(xmlread.Value))
                            {
                                var respVal = SaveServerPath(result, xmlread.Value, oldTCID, oldActionID, newTCID, newActionID, oldapsettingsID, null, lengthFileMsg, oldTCIDWithCaseName[oldTCID], par1, par2, xmlHeader, tblID);
                                dr[xmlread.Name] = respVal.Item1;
                                lengthFileMsg = respVal.Item2;
                            }
                            else
                            {
                                dr[xmlread.Name] = xmlread.Value;
                            }
                        }
                        else
                        {
                            var respVal = SaveServerPath(result, xmlread.Value, oldTCID, oldActionID, newTCID, newActionID, oldapsettingsID, null, lengthFileMsg, oldTCIDWithCaseName[oldTCID], par1, par2, xmlHeader, tblID);
                            dr[xmlread.Name] = respVal.Item1;
                            lengthFileMsg = respVal.Item2;
                        }
                    }
                    else if (xmlread.Name == tblID)
                    {
                        oldapsettingsID = xmlread.Value;
                    }
                    else if (xmlread.Name == "TCID")
                    {
                        oldTCID = xmlread.Value;
                        newTCID = oldTCIDwithnewTCID[xmlread.Value];
                        dr[xmlread.Name] = Convert.ToInt32(newTCID);
                    }
                    else if (xmlread.Name == "ActionID")
                    {
                        oldActionID = xmlread.Value;
                        newActionID = oldActionIDwithNewActionID[xmlread.Value];
                        dr[xmlread.Name] = Convert.ToInt32(newActionID);
                    }
                    else if (xmlread.Name != "TCID" && xmlread.Name != "ActionID")
                    {
                        dr[xmlread.Name] = xmlread.Value;
                    }
                }

                dr.EndEdit();

                tbl.Rows.Add(dr);
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
            }

            return new Tuple<bool, DataTable, Dictionary<string,string>>(Verify, tbl, lengthFileMsg); 
        }

        private void WriteToDB(DataTable tbl, SqlConnection connect, string tblName)
        {
            try
            {
                List<string> columnNames = tbl.Columns.Cast<DataColumn>().Select(x => x.ColumnName).ToList();

                string columnQuery = "select COLUMN_NAME from INFORMATION_SCHEMA.COLUMNS where TABLE_NAME = '" + tblName + "'";
                DataTable table = SelectDataTableValue(columnQuery, connect, null, null);
                List<string> tpDBFilelds = table.Rows.OfType<DataRow>().Select(dr => dr.Field<string>(0)).ToList();

                List<string> dbdifference = tpDBFilelds.Except(columnNames).ToList();
                List<string> importdifference = columnNames.Except(tpDBFilelds).ToList();

                foreach (string dif in importdifference)
                {
                    tbl.Columns.Remove(dif);
                }

                using (SqlBulkCopy bulkcopy = new SqlBulkCopy(connect))
                {
                    bulkcopy.DestinationTableName = "dbo." + tblName;
                    bulkcopy.WriteToServer(tbl);
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

        public Dictionary<string, string> InsertXMLBulk(SqlConnection connect, Dictionary<string, string> oldTCIDwithnewTCID, Dictionary<string, string> oldActionIDwithNewActionID, Dictionary<string, string> oldTAIDwithOldTCID, string tblName, string tblKey, string tblTCKeyName, string tblActionKeyName, Dictionary<string, string> lengthFileMsg, Dictionary<string, string> oldTCIDWithCaseName)
        {
            try
            {
                XmlReaderSettings settings = new XmlReaderSettings
                {
                    CheckCharacters = false,
                    IgnoreWhitespace = true
                };

                DataTable ControlActiontbl = new DataTable();
                DataTable TelnetActiontbl = new DataTable();
                DataTable FirmwareActiontbl = new DataTable();
                DataTable DesignerActiontbl = new DataTable();
                DataTable NetpairingActiontbl = new DataTable();
                DataTable USBActiontbl = new DataTable();
                DataTable CECActiontbl = new DataTable();

                DataTable ControlVerificationtbl = new DataTable();
                DataTable TelnetVerificationtbl = new DataTable();
                DataTable logVerificationtbl = new DataTable();
                DataTable pcapVerificationtbl = new DataTable();
                DataTable USBVerificationtbl = new DataTable();
                DataTable CECVerificationtbl = new DataTable();
                DataTable QRVerificationtbl = new DataTable();
                DataTable ApsettingsVerificationtbl = new DataTable();
                DataTable APBenchModeInitialVerificationtbl = new DataTable();
                DataTable APSeqModeInitialVerificationtbl = new DataTable();
                DataTable APVerificationtbl = new DataTable();
                DataTable APGainInitialtbl = new DataTable();
                DataTable APGainVerificationtbl = new DataTable();
                DataTable APFrequencyResponseInitialtbl = new DataTable();
                DataTable APFrequencyResponsetbl = new DataTable();
                DataTable APPhaseInitialtbl = new DataTable();
                DataTable APPhasetbl = new DataTable();
                DataTable APSteppedFreqSweepInitialtbl = new DataTable();
                DataTable APSteppedFreqSweeptbl = new DataTable();
                DataTable APTHDNInitialtbl = new DataTable();
                DataTable APTHDNtbl = new DataTable();
                DataTable scriptVerificationtbl = new DataTable();
                DataTable userActiontbl = new DataTable();
                DataTable userVerifytbl = new DataTable();

                bool ControlActionval = false;
                bool TelnetActionval = false;
                bool FirmwareActionval = false;
                bool DesignerActionval = false;
                bool NetpairActionval = false;
                bool USBActionval = false;
                bool CECActionval = false;
                bool ControlVerifyval = false;
                bool TelnetVerify = false;
                bool logVerify = false;
                bool pcapVerify = false;
                bool USBVerify = false;
                bool CECVerify = false;
                bool QRVerify = false;
                bool ApsettingsVerify = false;
                bool APBenchModeInitialVerify = false;
                bool APSeqModeInitialVerify = false;
                bool APVerify = false;
                bool APGainInitialverify = false;
                bool APGainVerify = false;
                bool APFrequencyResponseInitialverify = false;
                bool APFrequencyResponseverify = false;
                bool APPhaseInitialverify = false;
                bool APPhaseverify = false;
                bool APSteppedFreqSweepInitialverify = false;
                bool APSteppedFreqSweepverify = false;
                bool APTHDNInitialverify = false;
                bool APTHDNverify = false;
                bool scriptVerifyval = false;
                bool userActionval = false;
                bool userVerifyval = false;
                List<string> qrcmNewFilesPath = new List<string>();                

                string[] fileType = new string[5] { "Sine", "Sine, Dual", "Sine, Var Phase", "Noise", "IMD" };
                var directorycreate = new DirectoryInfo(@"C:\Users\" + Environment.UserName + @"\AppData\Local\Temp\QATImport");
                var version1 = new Version(xmlversion);
                var version2 = new Version(currentVerFileLength);
                var result = version1.CompareTo(version2);

                using (XmlReader xmlread = XmlReader.Create(filePath, settings))
                {
                    //xmlread.ReadToDescendant(tblName);

                    while (xmlread.Read())
                    {
                        if (xmlread.NodeType == XmlNodeType.Element && xmlread.HasAttributes && xmlread.GetAttribute("ActionID") != null && oldActionIDwithNewActionID.Keys.Contains(xmlread.GetAttribute("ActionID")))
                        {
                            if (xmlread.Name == "ControlAction")
                            {
                                var controlaction = ReadXMLValues(ControlActionval, xmlread, ControlActiontbl, oldTCIDwithnewTCID, oldActionIDwithNewActionID);
                                ControlActionval = controlaction.Item1;
                                ControlActiontbl = controlaction.Item2;
                            }
                            else if (xmlread.Name == "TelnetAction")
                            {
                                var telnetaction = ReadXMLValues(TelnetActionval, xmlread, TelnetActiontbl, oldTCIDwithnewTCID, oldActionIDwithNewActionID);
                                TelnetActionval = telnetaction.Item1;
                                TelnetActiontbl = telnetaction.Item2;
                            }
                            else if (String.Equals(xmlread.Name, "FirmwareAction", StringComparison.CurrentCultureIgnoreCase))
                            {
                                var action = ReadXMLValues(FirmwareActionval, xmlread, FirmwareActiontbl, oldTCIDwithnewTCID, oldActionIDwithNewActionID);
                                FirmwareActionval = action.Item1;
                                FirmwareActiontbl = action.Item2;
                            }
                            else if (xmlread.Name == "DesignerAction")
                            {
                                var action = ReadXMLValues(DesignerActionval, xmlread, DesignerActiontbl, oldTCIDwithnewTCID, oldActionIDwithNewActionID);
                                DesignerActionval = action.Item1;
                                DesignerActiontbl = action.Item2;
                            }
                            else if (xmlread.Name == "NetpairingAction")
                            {
                                var action = ReadXMLValues(NetpairActionval, xmlread, NetpairingActiontbl, oldTCIDwithnewTCID, oldActionIDwithNewActionID);
                                NetpairActionval = action.Item1;
                                NetpairingActiontbl = action.Item2;
                            }
                            else if (String.Equals(xmlread.Name, "UsbAction", StringComparison.CurrentCultureIgnoreCase))
                            {
                                var action = ReadXMLValues(USBActionval, xmlread, USBActiontbl, oldTCIDwithnewTCID, oldActionIDwithNewActionID);
                                USBActionval = action.Item1;
                                USBActiontbl = action.Item2;
                            }
                             else if (xmlread.Name == "CECAction")
                            {
                                var controlaction = ReadXMLValues(CECActionval, xmlread, CECActiontbl, oldTCIDwithnewTCID, oldActionIDwithNewActionID);
                                CECActionval = controlaction.Item1;
                                CECActiontbl = controlaction.Item2;
                            }
                            else if (xmlread.Name == "ControlVerification")
                            {
                                var verify = ReadXMLValues(ControlVerifyval, xmlread, ControlVerificationtbl, oldTCIDwithnewTCID, oldActionIDwithNewActionID);
                                ControlVerifyval = verify.Item1;
                                ControlVerificationtbl = verify.Item2;
                            }
                            else if (xmlread.Name == "TelnetVerify")
                            {
                                var verify = ReadXMLValues(TelnetVerify, xmlread, TelnetVerificationtbl, oldTCIDwithnewTCID, oldActionIDwithNewActionID);
                                TelnetVerify = verify.Item1;
                                TelnetVerificationtbl = verify.Item2;
                            }
                            else if (xmlread.Name == "logVerification")
                            {
                                var verify = ReadXMLValues(logVerify, xmlread, logVerificationtbl, oldTCIDwithnewTCID, oldActionIDwithNewActionID);
                                logVerify = verify.Item1;
                                logVerificationtbl = verify.Item2;
                            }
                            else if(xmlread.Name == "PcapVerification")
                            {
                                var verify = ReadXMLValues(pcapVerify, xmlread, pcapVerificationtbl, oldTCIDwithnewTCID, oldActionIDwithNewActionID);
                                pcapVerify = verify.Item1;
                                pcapVerificationtbl = verify.Item2;
                            }
                            else if (xmlread.Name == "UsbVerify")
                            {
                                var verify = ReadXMLValues(USBVerify, xmlread, USBVerificationtbl, oldTCIDwithnewTCID, oldActionIDwithNewActionID);
                                USBVerify = verify.Item1;
                                USBVerificationtbl = verify.Item2;                                
                            }
                            else if (xmlread.Name == "CECVerification")
                            {
                                var verify = ReadXMLValues(CECVerify, xmlread, CECVerificationtbl, oldTCIDwithnewTCID, oldActionIDwithNewActionID);
                                CECVerify = verify.Item1;
                                CECVerificationtbl = verify.Item2;
                            }
                            else if (xmlread.Name == "QRVerification")
                            {
                                var verify = ReadXMLValues(QRVerify, xmlread, QRVerificationtbl, oldTCIDwithnewTCID, oldActionIDwithNewActionID);
                                QRVerify = verify.Item1;
                                QRVerificationtbl = verify.Item2;
                            }
                            else if (xmlread.Name == "APBenchModeInitialSettings")
                            {
                                var verify = ReadXMLValues(APBenchModeInitialVerify, xmlread, APBenchModeInitialVerificationtbl, oldTCIDwithnewTCID, oldActionIDwithNewActionID);
                                APBenchModeInitialVerify = verify.Item1;
                                APBenchModeInitialVerificationtbl = verify.Item2;
                            }
                            else if (xmlread.Name == "APSeqModeInitialSettings")
                            {
                                var verify = ReadXMLValues(APSeqModeInitialVerify, xmlread, APSeqModeInitialVerificationtbl, oldTCIDwithnewTCID, oldActionIDwithNewActionID);
                                APSeqModeInitialVerify = verify.Item1;
                                APSeqModeInitialVerificationtbl = verify.Item2;                                
                            }
                            else if (xmlread.Name == "APLevelAndGainInitialSettings")
                            {
                                var verify = ReadXMLValues(APGainInitialverify, xmlread, APGainInitialtbl, oldTCIDwithnewTCID, oldActionIDwithNewActionID);
                                APGainInitialverify = verify.Item1;
                                APGainInitialtbl = verify.Item2;
                            }
                            else if (xmlread.Name == "APFrequencyResponseInitialSettings")
                            {
                                var verify = ReadXMLValues(APFrequencyResponseInitialverify, xmlread, APFrequencyResponseInitialtbl, oldTCIDwithnewTCID, oldActionIDwithNewActionID);
                                APFrequencyResponseInitialverify = verify.Item1;
                                APFrequencyResponseInitialtbl = verify.Item2;                                
                            }
                            else if (xmlread.Name == "APPhaseInitialSettings")
                            {
                                var verify = ReadXMLValues(APPhaseInitialverify, xmlread, APPhaseInitialtbl, oldTCIDwithnewTCID, oldActionIDwithNewActionID);
                                APPhaseInitialverify = verify.Item1;
                                APPhaseInitialtbl = verify.Item2;
                            }
                            else if (xmlread.Name == "APSteppedFreqSweepInitialSettings")
                            {
                                var verify = ReadXMLValues(APSteppedFreqSweepInitialverify, xmlread, APSteppedFreqSweepInitialtbl, oldTCIDwithnewTCID, oldActionIDwithNewActionID);
                                APSteppedFreqSweepInitialverify = verify.Item1;
                                APSteppedFreqSweepInitialtbl = verify.Item2;
                            }
                            else if (xmlread.Name == "APTHDNInitialSettings")
                            {
                                var verify = ReadXMLValues(APTHDNInitialverify, xmlread, APTHDNInitialtbl, oldTCIDwithnewTCID, oldActionIDwithNewActionID);
                                APTHDNInitialverify = verify.Item1;
                                APTHDNInitialtbl = verify.Item2;
                            }
                            else if (xmlread.Name == "APSettings")
                            {
                                var verify = ReadDatasFromXML(ApsettingsVerify, xmlread, ApsettingsVerificationtbl, oldTCIDwithnewTCID, oldActionIDwithNewActionID, oldTCIDWithCaseName, 
                                    lengthFileMsg, result, "WaveformType", "APSettingsID", "Audio Precision", "AP Waveform Files", "APWaveFile");
                                ApsettingsVerify = verify.Item1;
                                ApsettingsVerificationtbl = verify.Item2;
                                lengthFileMsg = verify.Item3;                               
                            }
                            else if (xmlread.Name == "APVerification")
                            {
                                var verify = ReadDatasFromXML(APVerify, xmlread, APVerificationtbl, oldTCIDwithnewTCID, oldActionIDwithNewActionID, oldTCIDWithCaseName,
                                    lengthFileMsg, result, "APxPath", "APVerificationID", "Audio Precision", "AP Project Files", "APProjectFile");
                                APVerify = verify.Item1;
                                APVerificationtbl = verify.Item2;
                                lengthFileMsg = verify.Item3;                                
                            }
                            else if (xmlread.Name == "LevelAndGain")
                            {
                                var verify = ReadDatasFromXML(APGainVerify, xmlread, APGainVerificationtbl, oldTCIDwithnewTCID, oldActionIDwithNewActionID, oldTCIDWithCaseName,
                                    lengthFileMsg, result, "WaveformType", "LevelAndGainID", "Audio Precision", "AP Waveform Files", "APWaveFile");
                                APGainVerify = verify.Item1;
                                APGainVerificationtbl = verify.Item2;
                                lengthFileMsg = verify.Item3;                                
                            }
                            else if (xmlread.Name == "APFrequencyResponse")
                            {
                                var verify = ReadDatasFromXML(APFrequencyResponseverify, xmlread, APFrequencyResponsetbl, oldTCIDwithnewTCID, oldActionIDwithNewActionID, oldTCIDWithCaseName,
                                   lengthFileMsg, result, "VerificationLocation", "APFrqSettingsID", "Audio Precision", "Verification Files", "APVerificationFile");
                                APFrequencyResponseverify = verify.Item1;
                                APFrequencyResponsetbl = verify.Item2;
                                lengthFileMsg = verify.Item3;
                            }
                            else if (xmlread.Name == "APPhaseSettings")
                            {
                                var verify = ReadDatasFromXML(APPhaseverify, xmlread, APPhasetbl, oldTCIDwithnewTCID, oldActionIDwithNewActionID, oldTCIDWithCaseName,
                                   lengthFileMsg, result, "VerificationLocation", "APPhaseSettingsID", "Audio Precision", "Verification Files", "APVerificationFile");
                                APPhaseverify = verify.Item1;
                                APPhasetbl = verify.Item2;
                                lengthFileMsg = verify.Item3;                                 
                            }
                            else if (xmlread.Name == "APSteppedFreqSweepSettings")
                            {
                                var verify = ReadDatasFromXML(APSteppedFreqSweepverify, xmlread, APSteppedFreqSweeptbl, oldTCIDwithnewTCID, oldActionIDwithNewActionID, oldTCIDWithCaseName,
                                   lengthFileMsg, result, "VerificationLocation", "APSteppeedFreqSettingsID", "Audio Precision", "Verification Files", "APVerificationFile");
                                APSteppedFreqSweepverify = verify.Item1;
                                APSteppedFreqSweeptbl = verify.Item2;
                                lengthFileMsg = verify.Item3;                               
                            }
                            else if (xmlread.Name == "APTHDNSettings")
                            {
                                var verify = ReadDatasFromXML(APTHDNverify, xmlread, APTHDNtbl, oldTCIDwithnewTCID, oldActionIDwithNewActionID, oldTCIDWithCaseName,
                                   lengthFileMsg, result, "VerificationLocation", "APPTHDNSettingsID", "Audio Precision", "Verification Files", "APVerificationFile");
                                APTHDNverify = verify.Item1;
                                APTHDNtbl = verify.Item2;
                                lengthFileMsg = verify.Item3;
                            }
                            else if (xmlread.Name == "Responsalyzer")
                            {
                                string oldTCID = string.Empty;
                                string newTCID = string.Empty;
                                string oldActionID = string.Empty;
                                string newActionID = string.Empty;
                                string oldRespID = string.Empty;

                                //List<string> fieldName = new List<string>();
                                //List<string> fieldValue = new List<string>();
                                Dictionary<string, string> parameterwithfieldValue = new Dictionary<string, string>();
                                Dictionary<string, string> fieldValue = new Dictionary<string, string>();
                                string oldFileName = string.Empty;

                                for (int attId = 0; attId < xmlread.AttributeCount; attId++)
                                {
                                    xmlread.MoveToAttribute(attId);

                                    if (xmlread.Name == "VerificationFileLocation" && xmlread.Value != null)
                                    {
                                        oldFileName = xmlread.Value;
                                    }

                                    if (xmlread.Name == "ResponsalyzerID")
                                    {
                                        oldRespID = xmlread.Value;
                                    }
                                    else if (xmlread.Name == "TCID")
                                    {
                                        parameterwithfieldValue.Add(xmlread.Name, "@" + xmlread.Name);
                                        fieldValue.Add(xmlread.Name, oldTCIDwithnewTCID[xmlread.Value]);

                                        oldTCID = xmlread.Value;
                                        newTCID = oldTCIDwithnewTCID[xmlread.Value];
                                    }
                                    else if (xmlread.Name == "ActionID")
                                    {
                                        parameterwithfieldValue.Add(xmlread.Name, "@" + xmlread.Name);
                                        fieldValue.Add(xmlread.Name, oldActionIDwithNewActionID[xmlread.Value]);

                                        oldActionID = xmlread.Value;
                                        newActionID = oldActionIDwithNewActionID[xmlread.Value];
                                    }
                                    else if (xmlread.Name != "TCID" && xmlread.Name != "ActionID")
                                    {
                                        parameterwithfieldValue.Add(xmlread.Name, "@" + xmlread.Name);
                                        fieldValue.Add(xmlread.Name, xmlread.Value);
                                    }

                                }

                                string columnQuery = "select COLUMN_NAME from INFORMATION_SCHEMA.COLUMNS where TABLE_NAME = '" + "Responsalyzer" + "'";
                                DataTable table = SelectDataTableValue(columnQuery, connect, null, null);
                                List<string> tpDBFilelds = table.Rows.OfType<DataRow>().Select(dr => dr.Field<string>(0)).ToList();

                                List<string> importdifference = parameterwithfieldValue.Keys.Except(tpDBFilelds).ToList();

                                foreach (string dif in importdifference)
                                {
                                    parameterwithfieldValue.Remove(dif);
                                    fieldValue.Remove(dif);
                                }

                                /////////Insert Responsalyzer table
                                string insertQuery = "insert into Responsalyzer (" + string.Join(",", parameterwithfieldValue.Keys) + ") values (" + string.Join(",", parameterwithfieldValue.Values) + ") SELECT CONVERT(int, SCOPE_IDENTITY())";

                                string newResponseID = InsertInToDB(insertQuery, connect, parameterwithfieldValue.Values.ToList(), fieldValue.Values.ToList());


                                //////////Save files in server
                                var respVal = SaveServerPath(result, oldFileName, oldTCID, oldActionID, newTCID, newActionID, oldRespID, newResponseID, lengthFileMsg, oldTCIDWithCaseName[oldTCID], "Responsalyzer", "Reference Files", "ResponsalyzerFile", "ResponsalyzerID");
                                string newFileName = respVal.Item1;
                                lengthFileMsg = respVal.Item2;

                                ////Update verification file field
                                string query = "update Responsalyzer set VerificationFileLocation = @FileName where ResponsalyzerID = '" + newResponseID + "'";
                                InsertInToDB(query, connect, new List<string> { "@FileName" }, new List<string> { newFileName });

                            }
                             else if (xmlread.Name == "ScriptVerification")
                            {
                                var verify = ReadXMLValues(scriptVerifyval, xmlread, scriptVerificationtbl, oldTCIDwithnewTCID, oldActionIDwithNewActionID);
                                scriptVerifyval = verify.Item1;
                                scriptVerificationtbl = verify.Item2;
                            }
                            else if (xmlread.Name == "UserAction")
                            {
                                var userAction = ReadXMLValues(userActionval, xmlread, userActiontbl, oldTCIDwithnewTCID, oldActionIDwithNewActionID);
                                userActionval = userAction.Item1;
                                userActiontbl = userAction.Item2;
                            }
                            else if (xmlread.Name == "UserVerification")
                            {
                                var userVerify = ReadXMLValues(userVerifyval, xmlread, userVerifytbl, oldTCIDwithnewTCID, oldActionIDwithNewActionID);
                                userVerifyval = userVerify.Item1;
                                userVerifytbl = userVerify.Item2;
                            }
                            else if (xmlread.Name == "QRCMAction")
                            {
                                //// Here'QRCMAction' is actual table name to write content to database
                                ////  and 'QRCMFile' is Attribute name in file settings file for QRCMaction and verification
                                //// 'QRCM_Files' is the directory name given to save qrcm files to server path

                                var qrcmActionResult =  ReadxmlValuesAndSaveFileQRCM(xmlread, oldTCIDwithnewTCID, oldActionIDwithNewActionID,connect, "QRCMAction", "QRCMFile", "QRCM_Files");
                                if (qrcmActionResult.Item1)
                                {
                                    if (qrcmActionResult.Item2 !=string.Empty && !qrcmNewFilesPath.Contains(qrcmActionResult.Item2))
                                        qrcmNewFilesPath.Add(qrcmActionResult.Item2);
                                }
                            }
                            else if (xmlread.Name == "QRCMVerification")
                            {
                                //// Here 'QRCMVerification' is actual table name to write content to database
                                ////  and 'QRCMFile' is Attribute name in file settings file for QRCMaction and verification
                                ////  'QRCM_Files' is the directory name given to save qrcm files to server path
                                var qrcmVerifyResult = ReadxmlValuesAndSaveFileQRCM(xmlread, oldTCIDwithnewTCID, oldActionIDwithNewActionID, connect, "QRCMVerification", "QRCMFile", "QRCM_Files");
                                if (qrcmVerifyResult.Item1)
                                {
                                    if (qrcmVerifyResult.Item2 != string.Empty && !qrcmNewFilesPath.Contains(qrcmVerifyResult.Item2))
                                        qrcmNewFilesPath.Add(qrcmVerifyResult.Item2);
                                }                              
                            }
                        }
                    }
                }

                //////If QRCM files available, For each QRCM files: Change file access to read only
                if (qrcmNewFilesPath.Count>0)
                {                  
                    foreach(string file in qrcmNewFilesPath)
                    {
                        FileInfo fileInformations = new FileInfo(file);
                        fileInformations.IsReadOnly = true;
                    }
                }


                if (ControlActiontbl.Rows.Count > 0)
                {
                    WriteToDB(ControlActiontbl, connect, "ControlAction");
                }

                if (TelnetActiontbl.Rows.Count > 0)
                {
                    WriteToDB(TelnetActiontbl, connect, "TelnetAction"); 
                }

                if (FirmwareActiontbl.Rows.Count > 0)
                {
                    WriteToDB(FirmwareActiontbl, connect, "FirmwareAction");                    
                }

                if (DesignerActiontbl.Rows.Count > 0)
                {
                    WriteToDB(DesignerActiontbl, connect, "DesignerAction");
                }

                if (NetpairingActiontbl.Rows.Count > 0)
                {
                    WriteToDB(NetpairingActiontbl, connect, "NetpairingAction");
                }

                if (USBActiontbl.Rows.Count > 0)
                {
                    WriteToDB(USBActiontbl, connect, "USBAction");
                }

                if (CECActiontbl.Rows.Count > 0)
                {
                    WriteToDB(CECActiontbl, connect, "CECAction");
                }
                if (ControlVerificationtbl.Rows.Count > 0)
                {
                    WriteToDB(ControlVerificationtbl, connect, "ControlVerification");
                }

                if (TelnetVerificationtbl.Rows.Count > 0)
                {
                    WriteToDB(TelnetVerificationtbl, connect, "TelnetVerify");
                }

                if (logVerificationtbl.Rows.Count > 0)
                {
                    WriteToDB(logVerificationtbl, connect, "logVerification");
                }

                if (pcapVerificationtbl.Rows.Count > 0)
                {
                    WriteToDB(pcapVerificationtbl, connect, "PcapVerification");
                }

                if (USBVerificationtbl.Rows.Count > 0)
                {
                    WriteToDB(USBVerificationtbl, connect, "UsbVerify"); 
                }
                if (CECVerificationtbl.Rows.Count > 0)
                {
                    WriteToDB(CECVerificationtbl, connect, "CECVerification");
                }
                if (QRVerificationtbl.Rows.Count > 0)
                {
                    WriteToDB(QRVerificationtbl, connect, "QRVerification");
                }
                if (APBenchModeInitialVerificationtbl.Rows.Count > 0)
                {
                    WriteToDB(APBenchModeInitialVerificationtbl, connect, "APBenchModeInitialSettings");
                }

                if (APSeqModeInitialVerificationtbl.Rows.Count > 0)
                {
                    WriteToDB(APSeqModeInitialVerificationtbl, connect, "APSeqModeInitialSettings");
                }

                if (APGainInitialtbl.Rows.Count > 0)
                {
                    WriteToDB(APGainInitialtbl, connect, "APLevelAndGainInitialSettings");
                }

                if (APFrequencyResponseInitialtbl.Rows.Count > 0)
                {
                    WriteToDB(APFrequencyResponseInitialtbl, connect, "APFrequencyResponseInitialSettings");
                }

                if (APPhaseInitialtbl.Rows.Count > 0)
                {
                    WriteToDB(APPhaseInitialtbl, connect, "APPhaseInitialSettings");
                }

                if (APSteppedFreqSweepInitialtbl.Rows.Count > 0)
                {
                    WriteToDB(APSteppedFreqSweepInitialtbl, connect, "APSteppedFreqSweepInitialSettings");
                }

                if (APTHDNInitialtbl.Rows.Count > 0)
                {
                    WriteToDB(APTHDNInitialtbl, connect, "APTHDNInitialSettings"); 
                }

                if (ApsettingsVerificationtbl.Rows.Count > 0)
                {
                    WriteToDB(ApsettingsVerificationtbl, connect, "APSettings");
                }

                if (APVerificationtbl.Rows.Count > 0)
                {
                    WriteToDB(APVerificationtbl, connect, "APVerification");
                }

                if (APGainVerificationtbl.Rows.Count > 0)
                {
                    WriteToDB(APGainVerificationtbl, connect, "LevelAndGain"); 
                }                                    

                if (APFrequencyResponsetbl.Rows.Count > 0)
                {
                    WriteToDB(APFrequencyResponsetbl, connect, "APFrequencyResponse");
                }

                if (APPhasetbl.Rows.Count > 0)
                {
                    WriteToDB(APPhasetbl, connect, "APPhaseSettings");
                }

                if (APSteppedFreqSweeptbl.Rows.Count > 0)
                {
                    WriteToDB(APSteppedFreqSweeptbl, connect, "APSteppedFreqSweepSettings");
                }

                if (APTHDNtbl.Rows.Count > 0)
                {
                    WriteToDB(APTHDNtbl, connect, "APTHDNSettings");
                }

                if (scriptVerificationtbl.Rows.Count > 0)
                {
                    WriteToDB(scriptVerificationtbl, connect, "ScriptVerification");
                }

                if (userActiontbl.Rows.Count > 0)
                {
                    WriteToDB(userActiontbl, connect, "UserAction");
                }

                if (userVerifytbl.Rows.Count > 0)
                {
                    WriteToDB(userVerifytbl, connect, "UserVerification");
                }
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
            }

            return lengthFileMsg;
        }

        
        private Tuple<bool,string> ReadxmlValuesAndSaveFileQRCM(XmlReader xmlread, Dictionary<string, string> oldTCIDwithnewTCID, Dictionary<string, string> oldActionIDwithNewActionID, SqlConnection connect,string actionName, string tagName2, string filepath)
        {
            bool success = false;
            string newFileFullPath = string.Empty;
            try
            {
                string oldTCID = string.Empty;
                string newTCID = string.Empty;
                string oldActionID = string.Empty;
                string newActionID = string.Empty;
                string oldQRCMprimaryKey = string.Empty;
                string newQRCMprimaryKey = string.Empty;

                Dictionary<string, string> parameterwithfieldValue = new Dictionary<string, string>();
                Dictionary<string, string> fieldValue = new Dictionary<string, string>();
                string oldFileName = string.Empty;

                for (int attId = 0; attId < xmlread.AttributeCount; attId++)
                {
                    xmlread.MoveToAttribute(attId);

                    if (xmlread.Value != null && ((actionName== "QRCMAction" && xmlread.Name == "PayloadFilePath") || (actionName == "QRCMVerification" && xmlread.Name == "ReferenceFilePath")))
                    {
                        oldFileName = xmlread.Value;
                    }

                    if ((actionName == "QRCMAction" && xmlread.Name == "QRCMActionID") || (actionName == "QRCMVerification" && xmlread.Name == "QRCMVerificationID"))
                    {
                        oldQRCMprimaryKey = xmlread.Value;
                    }
                    else if (xmlread.Name == "TCID")
                    {
                        parameterwithfieldValue.Add(xmlread.Name, "@" + xmlread.Name);
                        fieldValue.Add(xmlread.Name, oldTCIDwithnewTCID[xmlread.Value]);

                        oldTCID = xmlread.Value;
                        newTCID = oldTCIDwithnewTCID[xmlread.Value];
                    }
                    else if (xmlread.Name == "ActionID")
                    {
                        parameterwithfieldValue.Add(xmlread.Name, "@" + xmlread.Name);
                        fieldValue.Add(xmlread.Name, oldActionIDwithNewActionID[xmlread.Value]);

                        oldActionID = xmlread.Value;
                        newActionID = oldActionIDwithNewActionID[xmlread.Value];
                    }
                    else
                    {
                        parameterwithfieldValue.Add(xmlread.Name, "@" + xmlread.Name);
                        fieldValue.Add(xmlread.Name, xmlread.Value);
                    }
                }

                string columnQuery = "select COLUMN_NAME from INFORMATION_SCHEMA.COLUMNS where TABLE_NAME = '" + actionName + "'";
                DataTable table = SelectDataTableValue(columnQuery, connect, null, null);
                List<string> tpDBFilelds = table.Rows.OfType<DataRow>().Select(dr => dr.Field<string>(0)).ToList();

                List<string> importdifference = parameterwithfieldValue.Keys.Except(tpDBFilelds).ToList();

                foreach (string dif in importdifference)
                {
                    parameterwithfieldValue.Remove(dif);
                    fieldValue.Remove(dif);
                }

                ///////// Insert into Database
                string insertQuery = "Insert into "+ actionName + "(" + string.Join(",", parameterwithfieldValue.Keys) + ") values (" + string.Join(",", parameterwithfieldValue.Values) + ") SELECT CONVERT(int, SCOPE_IDENTITY())";
                newQRCMprimaryKey = InsertInToDB(insertQuery, connect, parameterwithfieldValue.Values.ToList(), fieldValue.Values.ToList());

                ///////// if action/verification has file, update file name into DB and save that file into server path else skip saving
                if (oldFileName != string.Empty)
                {
                    ////Update filepath field in DB: update filename using new primary key for the current inserted row
                    string newFileName = newTCID + ".txt";
                    if (actionName == "QRCMAction")
                    {                       
                        string query = "update QRCMAction set PayloadFilePath = @FileName where QRCMActionID = '" + newQRCMprimaryKey + "'";
                        InsertInToDB(query, connect, new List<string> { "@FileName" }, new List<string> { newFileName });
                    }
                    else
                    {
                        string query = "update QRCMVerification set ReferenceFilePath = @FileName where QRCMVerificationID = '" + newQRCMprimaryKey + "'";
                        InsertInToDB(query, connect, new List<string> { "@FileName" }, new List<string> { newFileName });
                    }


                    ///////save file in serverpath starts
                   var result= SaveFileQRCM(tagName2, actionName, filepath, oldTCID, newTCID, oldQRCMprimaryKey, newQRCMprimaryKey);
                   success= result.Item1;
                   newFileFullPath = result.Item2;
                }
                else
                {
                    //// If this Action don't have any file and only db write done successfully then success= true 
                    success = true;
                }

            
            }
            catch (Exception ex)
            {
                success = false;
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
            }

            return new Tuple<bool, string>(success,newFileFullPath);
        }

        private Tuple<bool, string> SaveFileQRCM(string tagName2, string actionName, string filepath,string oldTCID, string newTCID, string oldQRCMprimaryKey,string newQRCMprimaryKey)
        {
            bool isSuccess = false;
            string newFileFullPath = string.Empty;
            try
            {            
                var directorycreate = new DirectoryInfo(@"C:\Users\" + Environment.UserName + @"\AppData\Local\Temp\QATImport");

                string filesettingsXMLpath = Path.Combine(directorycreate.FullName, "FileSettings.xml");
                XmlReaderSettings settings = new XmlReaderSettings
                {
                    CheckCharacters = false,
                    IgnoreWhitespace = true
                };

                using (XmlReader reader = XmlReader.Create(filesettingsXMLpath, settings))
                {
                    do
                    {
                        //////Find QRCM file row for the current testcase id 
                        if (reader.NodeType == XmlNodeType.Element && reader.Name == tagName2 && reader.HasAttributes && reader.GetAttribute("TCID") != null && reader.GetAttribute("TCID") == oldTCID)
                        {
                            string dummyName = string.Empty;
                            string actualfileName = string.Empty;

                            for (int attId1 = 0; attId1 < reader.AttributeCount; attId1++)
                            {
                                reader.MoveToAttribute(attId1);

                                if (reader.Name == "DummyName")
                                {
                                    dummyName = reader.Value;
                                }
                                else if (reader.Name == "ActualName")
                                {
                                    actualfileName = reader.Value;
                                }
                            }

                            ////////  if file name available in file settings file start file writing

                            if (dummyName != null && dummyName != string.Empty && actualfileName != null && actualfileName != string.Empty)
                            {
                                string outputString = string.Empty;
                                string keyToFindInDummyFile = string.Empty;
                                string keyToReplaceInNewFile = string.Empty;
                                string extension = Path.GetExtension(actualfileName);
                                if (extension == null || extension == string.Empty)
                                    extension = ".txt";

                                string editedFileName = newTCID + extension;
                                if (actionName == "QRCMAction")
                                {
                                    keyToFindInDummyFile = "Action_" + oldQRCMprimaryKey;
                                    keyToReplaceInNewFile = "Action_" + newQRCMprimaryKey;
                                }
                                else if (actionName == "QRCMVerification")
                                {
                                    keyToFindInDummyFile = "Verification_" + oldQRCMprimaryKey;
                                    keyToReplaceInNewFile = "Verification_" + newQRCMprimaryKey;
                                }


                                ////////////// Reading content from dummy file (from extracted zip)
                                string dummyFilepath = Path.Combine(directorycreate.FullName, filepath, dummyName);

                                if (File.Exists(dummyFilepath))
                                {
                                    using (StreamReader read = new StreamReader(dummyFilepath))
                                    {
                                        string filepathOutput = string.Empty;
                                        string totalactionlines = string.Empty;
                                        while ((filepathOutput = read.ReadLine()) != null)
                                        {
                                            if (filepathOutput == ":QAT_Ref_Pay:")
                                            {
                                                if (totalactionlines != string.Empty)
                                                {
                                                    dynamic array = new System.Web.Script.Serialization.JavaScriptSerializer().DeserializeObject(totalactionlines);

                                                    foreach (var item in array)
                                                    {
                                                        if (item.Key == keyToFindInDummyFile)
                                                        {
                                                            ////////// break foreach loop if current action content found
                                                            outputString = new System.Web.Script.Serialization.JavaScriptSerializer().Serialize(item.Value);
                                                            break;
                                                        }
                                                    }
                                                }

                                                totalactionlines = string.Empty;
                                            }
                                            else
                                            {
                                                totalactionlines += filepathOutput;
                                            }

                                            ////////// break while loop if current action content found
                                            if (outputString != string.Empty)
                                                break;
                                        }
                                    }
                                }


                                if (outputString != string.Empty)
                                {
                                    //////Replacing action unique key
                                    string contentToWrite = "{\"" + keyToReplaceInNewFile + "\":" + outputString + "}\n:QAT_Ref_Pay:";                                                                     
                                    string serverPath = Path.Combine(QatConstants.QATServerPath, filepath);
                                    newFileFullPath = Path.Combine(serverPath, editedFileName);

                                    //////Creating QRCM folder if not exists
                                    if (!Directory.Exists(serverPath))
                                    {
                                        Directory.CreateDirectory(serverPath);
                                    }

                                    ////// Create or append actual file 
                                    using (StreamWriter writer = new StreamWriter(newFileFullPath, true))
                                    {
                                        writer.WriteLine(contentToWrite);
                                    }

                                    isSuccess = true;
                                    break;
                                }
                            }
                        }
                    } while (reader.Read());
                }
            
            }
            catch (Exception ex)
            {
                isSuccess = false;
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
            }
            return new Tuple<bool, string>(isSuccess, newFileFullPath);
        }



        private Tuple<string, Dictionary<string, string>> SaveServerPath(int result, string xmlValue, string oldTCID, string oldActionID, string newTCID, string newActionID, string oldRespID, string newRespID, Dictionary<string, string> lengthFileMsg, string testcaseName, string par1, string par2, string par3, string parameterID)
        {
            string apverify = string.Empty;

            try
            {
                var directorycreate = new DirectoryInfo(@"C:\Users\" + Environment.UserName + @"\AppData\Local\Temp\QATImport"); 

                if (result < 0)
                {
                    var getVal = CreateServerFiles(directorycreate, xmlValue, xmlValue, par1, par2, oldTCID, oldActionID, newTCID, newActionID, oldRespID, newRespID, lengthFileMsg, testcaseName);
                    apverify = getVal.Item1;
                    lengthFileMsg = getVal.Item2;
                }
                else
                {
                    string filesettingsXMLpath = Path.Combine(directorycreate.FullName, "FileSettings.xml");
                    XmlReaderSettings settings = new XmlReaderSettings
                    {
                        CheckCharacters = false,
                        IgnoreWhitespace = true
                    };

                    using (XmlReader reader = XmlReader.Create(filesettingsXMLpath, settings))
                    {
                        do
                        {
                            if (((par1 == "Responsalyzer" || par2 == "AP Waveform Files") && reader.NodeType == XmlNodeType.Element && reader.Name == par3 && reader.HasAttributes && reader.GetAttribute("TAID") != null && reader.GetAttribute("TAID") == oldActionID.ToString() && reader.GetAttribute(parameterID) != null && reader.GetAttribute(parameterID) == oldRespID.ToString())
                                || par1 != "Responsalyzer" && par2 != "AP Waveform Files" && reader.NodeType == XmlNodeType.Element && reader.Name == par3 && reader.HasAttributes && reader.GetAttribute("TAID") != null && reader.GetAttribute("TAID") == oldActionID.ToString())
                            {
                                string dummyName = string.Empty;
                                string actualName = string.Empty;

                                for (int attId1 = 0; attId1 < reader.AttributeCount; attId1++)
                                {
                                    reader.MoveToAttribute(attId1);

                                    if (reader.Name == "DummyName")
                                    {
                                        dummyName = reader.Value;
                                    }
                                    else if (reader.Name == "ActualName")
                                    {
                                        actualName = reader.Value;
                                    }
                                }

                                if (dummyName != null && dummyName != string.Empty && actualName != null && actualName != string.Empty)
                                {
                                    var getVal = CreateServerFiles(directorycreate, dummyName, actualName, par1, par2, oldTCID, oldActionID, newTCID, newActionID, oldRespID, newRespID, lengthFileMsg, testcaseName);
                                    apverify = getVal.Item1;
                                    lengthFileMsg = getVal.Item2;
                                }
                            }
                        } while (reader.Read());
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

            return new Tuple<string, Dictionary<string, string>>(apverify, lengthFileMsg);
        }
         
        public void InsertBulkBackgroundDatas(SqlConnection connect, string oldBMID, string oldTPID, string newBMID, string newTPID, string tblName, string tblKey, string tblBMKeyName, string tblTSKeyName, string tblTPKeyName)
        {
            try
            {
                XmlReaderSettings settings = new XmlReaderSettings
                {
                    ConformanceLevel = ConformanceLevel.Auto,
                    CheckCharacters = false,
                    IgnoreWhitespace = true
                };

                DataTable tbl = new DataTable();
                bool val = false;

                using (XmlReader xmlread = XmlReader.Create(filePath, settings))
                {
                    xmlread.ReadToDescendant(tblName);

                    do
                    {
                        if (xmlread.NodeType == XmlNodeType.Element && xmlread.Name == tblName && xmlread.HasAttributes && xmlread.GetAttribute(tblBMKeyName) != null && xmlread.GetAttribute(tblBMKeyName) == oldBMID)
                        {
                            if (val == false)
                            {
                                for (int j = 0; j < xmlread.AttributeCount; j++)
                                {
                                    val = true;
                                    xmlread.MoveToAttribute(j);

                                    if (xmlread.Name == tblKey && xmlread.Name == tblBMKeyName && xmlread.Name == tblTSKeyName && xmlread.Name == tblTPKeyName)
                                    {
                                        tbl.Columns.Add(new DataColumn(xmlread.Name, typeof(SqlInt32)));
                                    }
                                    else
                                    {
                                        tbl.Columns.Add(new DataColumn(xmlread.Name, typeof(string)));
                                    }
                                }
                            }

                            DataRow dr = tbl.NewRow();
                            dr.BeginEdit();

                            for (int attId = 0; attId < xmlread.AttributeCount; attId++)
                            {
                                xmlread.MoveToAttribute(attId);

                                if (xmlread.Name != tblBMKeyName && xmlread.Name != tblTSKeyName && xmlread.Name != tblTPKeyName)
                                {
                                    dr[xmlread.Name] = xmlread.Value;
                                }
                                else
                                {
                                    if (xmlread.Name == tblBMKeyName)
                                        dr[xmlread.Name] = Convert.ToInt32(newBMID);
                                    else if (xmlread.Name == tblTSKeyName)
                                        dr[xmlread.Name] = 0;
                                    else if (xmlread.Name == tblTPKeyName)
                                        dr[xmlread.Name] = Convert.ToInt32(newTPID);
                                }
                            }


                            dr.EndEdit();

                            tbl.Rows.Add(dr);
                        }
                    } while (xmlread.Read());

                    if (xmlread != null)
                    {
                        xmlread.Dispose();
                        xmlread.Close();
                    }
                }

                List<string> columnNames = tbl.Columns.Cast<DataColumn>().Select(x => x.ColumnName).ToList();

                string columnQuery = "select COLUMN_NAME from INFORMATION_SCHEMA.COLUMNS where TABLE_NAME = '" + tblName + "'";
                DataTable table = SelectDataTableValue(columnQuery, connect, null, null);
                List<string> tpDBFilelds = table.Rows.OfType<DataRow>().Select(dr => dr.Field<string>(0)).ToList();

                List<string> dbdifference = tpDBFilelds.Except(columnNames).ToList();
                List<string> importdifference = columnNames.Except(tpDBFilelds).ToList();

                foreach (string dif in importdifference)
                {
                    tbl.Columns.Remove(dif);
                }

                using (SqlBulkCopy bulkcopy = new SqlBulkCopy(connect))
                {
                    bulkcopy.DestinationTableName = "dbo." + tblName;
                    bulkcopy.WriteToServer(tbl);
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
           
        private string CreateCopyItemName(SqlConnection connect, string query, string itemparameter, string itemName)
        {
            try
            {
                List<string> similarNames = new List<string>();
                string itemNewName = null;

                itemName = itemName.Trim();

                //////string query = "select TestPlanID from Testplan where Testplanname like '%' + @ItemName + '%'";

                DataTable dataTable = SelectDataTableValue(query, connect, itemparameter, itemName);
                DataTableReader dataTableReader = dataTable.CreateDataReader();

                if (dataTableReader.HasRows)
                {
                    while (dataTableReader.Read())
                    {
                        similarNames.Add(dataTableReader.GetValue(0).ToString());
                    }
                }

                itemNewName = itemName;
                if (similarNames.Contains(itemNewName, StringComparer.CurrentCultureIgnoreCase))
                {
                    itemNewName = itemNewName + "_import";
                    if (similarNames.Contains(itemNewName, StringComparer.CurrentCultureIgnoreCase))
                    {
                        for (int i = 2; ; i++)
                        {
                            if (!similarNames.Contains(itemNewName + " (" + i + ")", StringComparer.CurrentCultureIgnoreCase))
                            {
                                itemNewName = itemNewName + " (" + i + ")";
                                break;
                            }

                        }
                    }
                }

                return itemNewName;
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif

                //MessageBox.Show("Exception\n \n" + ex.Message, "QAT Error Code - EC02019", MessageBoxButton.OK, MessageBoxImage.Error);
                
                return null;
            }
        }

        private string InsertInToDB(string query, SqlConnection connect, List<string> parameter, List<string> parameterValue)
        {
            string id = string.Empty;

            try
            {
                DataTable tble = new DataTable();

                SqlCommand cmd = new SqlCommand(query, connect);

                if (parameter != null)
                {
                    for (int i = 0; i < parameter.Count; i++)
                    {
                        cmd.Parameters.AddWithValue(parameter[i], parameterValue[i]);
                    }
                }

                SqlDataAdapter adap = new SqlDataAdapter(cmd);
                adap.Update(tble);
                adap.Fill(tble);

                if (tble.Rows.Count > 0)
                {
                    DataTableReader read1 = tble.CreateDataReader();

                    while (read1.Read())
                    {
                        id = read1.GetValue(0).ToString();
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

            return id;
        }

        private string SelectSingleValue(string query, SqlConnection connect, string parameter, string parameterValue)
        {
            string id = string.Empty;

            try
            {
                DataTable tbl = new DataTable();
                SqlCommand cmd = new SqlCommand(query, connect);

                if (parameter != null)
                {
                    cmd.Parameters.AddWithValue(parameter, parameterValue);
                }

                SqlDataAdapter adapt = new SqlDataAdapter(cmd);
                adapt.Update(tbl);
                adapt.Fill(tbl);

                if (tbl.Rows.Count > 0)
                {
                    DataTableReader read1 = tbl.CreateDataReader();

                    while (read1.Read())
                    {
                        id = read1.GetValue(0).ToString();
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

            return id;
        }

        private string SelectSingleTPTCValue(string query, SqlConnection connect, string parameter, DateTime parameterValue)
        {
            string id = string.Empty;

            try
            {
                DataTable tbl = new DataTable();
                SqlCommand cmd = new SqlCommand(query, connect);

                if (parameter != null)
                {
                    cmd.Parameters.AddWithValue(parameter, parameterValue);
                }

                SqlDataAdapter adapt = new SqlDataAdapter(cmd);
                adapt.Update(tbl);
                adapt.Fill(tbl);

                if (tbl.Rows.Count > 0)
                {
                    DataTableReader read1 = tbl.CreateDataReader();

                    while (read1.Read())
                    {
                        id = read1.GetValue(0).ToString();
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

            return id;
        }

        private DataTable SelectDataTableValue(string query, SqlConnection connect, string parameter, string parameterValue)
        {
            DataTable tbl = new DataTable();

            try
            {
                SqlCommand cmd = new SqlCommand(query, connect);

                if (parameter != null && parameter != string.Empty)
                {
                    cmd.Parameters.AddWithValue(parameter, parameterValue);
                }

                SqlDataAdapter adapt = new SqlDataAdapter(cmd);
                adapt.Update(tbl);
                adapt.Fill(tbl);
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
            }

            return tbl;
        }
        
        private void AbortClick(object sender, RoutedEventArgs e)
        {
            try
            {
                StartImport.Abort();
                ImportEnable();
                //if (Progress != null)
                //    Progress.Close();
                return;
            }
            catch(Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                DeviceDiscovery.importwindowcount = 1;
                //if (Percentage != "100%" && StartImport != null)
                //{
                //Thread.Sleep(1000000);
                if (StartImport != null)
                {
                    StartImport.Suspend();
                MessageBoxResult result = ImportMessageBox("Are you sure cancel Importing ? ", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                    if (result.ToString() == "Yes")
                    {
                        //try
                        //{
                        ImportMessageBox("Please wait till the current testplan is being imported ", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                        StartImport.Resume();
                        if (executionMutex != null && DoWaitone == true)
                        {
                            //DeviceDiscovery.importwindowcount = 1;
                            executionMutex.WaitOne();
                            executionMutex = null;
                            //DeviceDiscovery.importwindowcount = 0;

                        }
                        else
                        {
                            //executionMutex.ReleaseMutex();
                            executionMutex = null;

                        }
                        if (StartImport != null && StartImport.ThreadState != ThreadState.Aborted)
                        {
                            StartImport.Resume();
                            StartImport.Abort();
                        }
                        if (reader != null)
                        {
                            reader.Dispose();
                            reader.Close();
                        }

                        var directorycreate = new DirectoryInfo(@"C:\Users\" + Environment.UserName + @"\AppData\Local\Temp\QATImport");
                        if (directorycreate.Exists)
                        {
                            DeleteFolder(directorycreate);
                        }

                        if (Directory.Exists(directorycreate.FullName))
                            Directory.Delete(directorycreate.FullName);
                      DeviceDiscovery.importwindowcount = 0;
                    }
                    else
                    {

                        StartImport.Resume();
                        e.Cancel = true;
                        DeviceDiscovery.importwindowcount = 1;
                    }
               }
                else
                {
                    DeviceDiscovery.importwindowcount = 0;
                }

          }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
                //if(e.Cancel!=null)
                //{
                    if (e.Cancel == true)
                        DeviceDiscovery.importwindowcount = 1;
                    if (e.Cancel == false)
                        DeviceDiscovery.importwindowcount = 0;
                //}
               
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
            }
        }


        private void DeleteFolder(DirectoryInfo maintempdir)
        {
            try
            {
                foreach (FileInfo files in maintempdir.GetFiles())
                {
                    files.Attributes &= ~FileAttributes.ReadOnly;
                    files.Delete();
                }

                foreach (DirectoryInfo dir in maintempdir.GetDirectories())
                {
                    DeleteFolder(dir);
                    dir.Delete();
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

        //        private void TCwithnoActioncount()
        //        {
        //            try
        //            {
        //                string null_tc_query = null;
        //                string Action_nos_query = null;
        //                string Actioncount_update = null;
        //                null_tc_query = "select TestcaseID from Testcase  where Actioncount is null";
        //                var Actioncountnull = dbConnection.Get_testcase_Actioncount_null(null_tc_query);
        //                foreach (DataRow actioncount in Actioncountnull.Rows)
        //                {
        //                    var TCID = actioncount.ItemArray[0];
        //                    Action_nos_query = "select * from TestAction as c join TestAction as p on p.TestActionID = c.TestActionID where p.TCID =" + TCID + "";

        //                    //query = "select * from " + childTableName + " as c join " + parentLinkTable + " as p on p." + childIDColumn + " = c." + childTableName + "ID where p." + parentIDColumn + " = " + parentPrimaryKey.ToString();
        //                    var Testcase_childcount = dbConnection.Testcase_childcount(Action_nos_query);
        //                    Actioncount_update = "UPDATE Testcase SET Actioncount = " + Testcase_childcount + " WHERE TestcaseID =" + TCID + "";
        //                    dbConnection.Testcase_fillActioncount(Actioncount_update);
        //                }
        //            }
        //            catch (Exception ex)
        //            {
        //                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
        //#if DEBUG
        //                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
        //#endif
        //                //MessageBox.Show("Exception\n " + ex.Message, "QAT Error Code - EC14023", MessageBoxButton.OK, MessageBoxImage.Error);
        //                //return false;
        //            }
        //        }

        private void TPwithnoActioncount(string tpID)
        {
            try
            {
                if (tpID != null && tpID != string.Empty)
                {
                    string Action_nos_query = "select * from TPTCLinkTable where TPID =" + tpID + "";
                    var Testcase_childcount = dbConnection.Testcase_childcount(Action_nos_query);
                    string Actioncount_update = "UPDATE Testplan SET TPActioncount = " + Testcase_childcount + " WHERE TestPlanID =" + tpID + "";
                    dbConnection.Testcase_fillActioncount(Actioncount_update);
                }
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                //MessageBox.Show("Exception\n " + ex.Message, "QAT Error Code - EC14023", MessageBoxButton.OK, MessageBoxImage.Error);
                //return false;
            }
        }
		
        private MessageBoxResult ImportMessageBox(string messageBoxTest, string messageBoxCaption, MessageBoxButton messageBoxButton, MessageBoxImage messageBoxImage)
        {

            MessageBoxResult result = MessageBoxResult.None;
            try
            {
                this.Dispatcher.Invoke(() =>
                {
                    Qmsg = new QatMessageBox(this);
                    Qmsg.Topmost = false;
                });

                result = Qmsg.Show(messageBoxTest, messageBoxCaption, messageBoxButton, messageBoxImage);
                return result;
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif

                return result;
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

        private void TestAction_Previewkeydown(object sender, KeyEventArgs e)
        {
            try
            {
                if ((Keyboard.IsKeyDown(Key.LeftShift)) || (Keyboard.IsKeyDown(Key.RightShift)))
                {
                    if (e.Key == Key.Add)
                    {
                        RunnerSlider.Value += 0.1;
                    }
                    else if (e.Key == Key.Subtract)
                    {
                        RunnerSlider.Value -= 0.1;
                    }
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

     

        private void Import_Window_Loaded(object sender, RoutedEventArgs e)
        {

            this.Title = this.Title + " ----->Connected to Server :" + QatConstants.SelectedServer;
        }

        private void Ascending_Click_Name(object sender, RoutedEventArgs e)
        {
            try
            {
                if (isInventorySearchListSelected == true)
                {
                    SetSorting("Ascending By Name", searchTreeviewlst);
                }
                else
                {
                    SetSorting("Ascending By Name", sourceimportTreeViewlist);
                    sortingOrders_OriginalList = sortingOrders;
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
        
        private void Descending_Click_Name(object sender, RoutedEventArgs e)
        {
            try
            {
                if (isInventorySearchListSelected == true)
                {
                    SetSorting("Descending By Name", searchTreeviewlst);
                }
                else
                {
                    SetSorting("Descending By Name", sourceimportTreeViewlist);
                    sortingOrders_OriginalList = sortingOrders;
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

        private void SetSorting(string sortType, List<TreeViewExplorer> importTreeviewLst)
        {
            try
            {
                List<TreeViewExplorer> treeViewExplorerList = new List<TreeViewExplorer>();
                TreeViewExplorer importIventoryItem = new TreeViewExplorer(0, QatConstants.TveImportInventoryTitle, QatConstants.TveDesignerHeaderItemType, null, null, null, null, null, null, null, null, null, null, 0, true);

                importIventoryItem.IsExpanded = true;
                importIventoryItem.IsCheckedVisibility = Visibility.Visible;
                importIventoryItem.ItemTextBox.FontWeight = FontWeights.Bold;

                if (importTreeviewLst.Count > 0 && importTreeviewLst[0].Children.Count > 0)
                {
                    TreeViewExplorer[] alphaTestSuiteSorted = importTreeviewLst[0].Children.ToArray();

                    if (sortType == "Ascending By Name")
                    {
                        Array.Sort(alphaTestSuiteSorted, new AlphanumComparatorFast());
                        sortingOrders = sortType;

                        Contextascending.IsChecked = true;
                        Contextdescending.IsChecked = false;
                    }
                    else if (sortType == "Descending By Name")
                    {
                        Array.Sort(alphaTestSuiteSorted, new AlphanumComparatorFast());
                        Array.Reverse(alphaTestSuiteSorted);
                        sortingOrders = sortType;

                        Contextascending.IsChecked = false;
                        Contextdescending.IsChecked = true;
                    }

                    treeViewExplorerList = alphaTestSuiteSorted.ToList();
                }

                importIventoryItem.ClearChildren();
                importIventoryItem.AddChildrenList_withCheckbox(treeViewExplorerList);

                importTreeviewLst.Clear();
                importTreeviewLst.Add(importIventoryItem);
                TreeViewImport.DataContext = null;
                TreeViewImport.DataContext = importTreeviewLst;
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

        private void TreeViewImport_ContextMenuOpening(object sender, System.Windows.Controls.ContextMenuEventArgs e)
        {
            try
            {
                bool isTreeviewEnabled = false;
                if (TreeViewImport.Items != null && TreeViewImport.Items.Count > 0)
                {
                    var treeviewHeader = TreeViewImport.Items[0] as TreeViewExplorer;
                    isTreeviewEnabled = treeviewHeader.IsEnabled;
                }

                if ((sourceimportTreeViewlist != null && sourceimportTreeViewlist.Count == 0) || isTreeviewEnabled == false)
                    e.Handled = true;
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

        private void txt_Search_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            try
            {
                if (sourceimportTreeViewlist != null && sourceimportTreeViewlist.Count > 0)
                {
                    if (txt_Search.Text != string.Empty)
                    {
                        if (sortingOrders_OriginalList != sortingOrders)
                        {
                            SetSorting(sortingOrders, sourceimportTreeViewlist);
                            sortingOrders_OriginalList = sortingOrders;
                        }

                        searchTreeviewlst.Clear();
                        Cancelbutton.Visibility = Visibility.Visible;
                        TreeViewExplorer treeViewExplorerSearchList = new TreeViewExplorer(0, QatConstants.TveImportInventoryTitle, QatConstants.TveDesignerHeaderItemType, null, null, null, null, null, null, null, null, null, null, 0, true);
                        treeViewExplorerSearchList.IsExpanded = true;
                        treeViewExplorerSearchList.IsCheckedVisibility = Visibility.Visible;
                        treeViewExplorerSearchList.ItemTextBox.FontWeight = FontWeights.Bold;

                        foreach (TreeViewExplorer inventHeader in sourceimportTreeViewlist)
                        {
                            foreach (TreeViewExplorer testPlans in inventHeader.Children)
                            {
                                CultureInfo culture2 = CultureInfo.InvariantCulture;
                                if (culture2.CompareInfo.IndexOf(testPlans.ItemName, txt_Search.Text, CompareOptions.IgnoreCase) >= 0)
                                {
                                    if (!treeViewExplorerSearchList.Children.Contains(testPlans))
                                    {
                                        treeViewExplorerSearchList.AddChildren_withCheckbox(testPlans);
                                    }
                                }

                                foreach (TreeViewExplorer testCases in testPlans.Children)
                                {
                                    CultureInfo culture3 = CultureInfo.InvariantCulture;
                                    if (culture3.CompareInfo.IndexOf(testCases.ItemName, txt_Search.Text, CompareOptions.IgnoreCase) >= 0)
                                    {
                                        if (!treeViewExplorerSearchList.Children.Contains(testPlans))
                                        {
                                            treeViewExplorerSearchList.AddChildren_withCheckbox(testPlans);
                                        }
										
                                        break;
                                    }
                                }
                            }
                        }

                        if (treeViewExplorerSearchList.Children.Count <= 0)
                            treeViewExplorerSearchList.IsChecked = false;

                        searchTreeviewlst.Add(treeViewExplorerSearchList);

                        TreeViewImport.DataContext = null;
                        TreeViewImport.DataContext = searchTreeviewlst;
                        isInventorySearchListSelected = true;
                    }
                    else
                    {
                        Cancelbutton.Visibility = Visibility.Hidden;
                        isInventorySearchListSelected = false;

                        if (sortingOrders_OriginalList != sortingOrders)
                        {
                            SetSorting(sortingOrders, sourceimportTreeViewlist);
                            sortingOrders_OriginalList = sortingOrders;
                        }
                        else
                        {
                            TreeViewExplorer treeViewExplorerList = new TreeViewExplorer(0, QatConstants.TveImportInventoryTitle, QatConstants.TveDesignerHeaderItemType, null, null, null, null, null, null, null, null, null, null, 0, true);
                            treeViewExplorerList.IsExpanded = true;
                            treeViewExplorerList.IsCheckedVisibility = Visibility.Visible;
                            treeViewExplorerList.ItemTextBox.FontWeight = FontWeights.Bold;

                            foreach (TreeViewExplorer inventHeader in sourceimportTreeViewlist)
                            {
                                foreach (TreeViewExplorer testPlans in inventHeader.Children)
                                {
                                    treeViewExplorerList.AddChildren_withCheckbox(testPlans);
                                }
                            }

                            sourceimportTreeViewlist.Clear();
                            sourceimportTreeViewlist.Add(treeViewExplorerList);

                            TreeViewImport.DataContext = null;
                            TreeViewImport.DataContext = sourceimportTreeViewlist;
                        }
                    }
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

        private void TextBox_PreviewKeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            try
            {
                var textBox = e.OriginalSource as TextBox;

                if (textBox != null && e.Key == Key.Tab)
                {
                    textBox.SelectAll();
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

        private void Cancelbutton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                txt_Search.Clear();
                isInventorySearchListSelected = false;

                if (sourceimportTreeViewlist.Count > 0)
                {
                    if (sortingOrders_OriginalList != sortingOrders)
                    {
                        SetSorting(sortingOrders, sourceimportTreeViewlist);
                        sortingOrders_OriginalList = sortingOrders;
                    }
                    else
                    {
                        TreeViewExplorer treeViewExplorerList = new TreeViewExplorer(0, QatConstants.TveImportInventoryTitle, QatConstants.TveDesignerHeaderItemType, null, null, null, null, null, null, null, null, null, null, 0, true);
                        treeViewExplorerList.IsExpanded = true;
                        treeViewExplorerList.IsCheckedVisibility = Visibility.Visible;
                        treeViewExplorerList.ItemTextBox.FontWeight = FontWeights.Bold;

                        foreach (TreeViewExplorer inventHeader in sourceimportTreeViewlist)
                        {
                            foreach (TreeViewExplorer testPlans in inventHeader.Children)
                            {
                                treeViewExplorerList.AddChildren_withCheckbox(testPlans);
                            }
                        }

                        sourceimportTreeViewlist.Clear();
                        sourceimportTreeViewlist.Add(treeViewExplorerList);

                        TreeViewImport.DataContext = null;
                        TreeViewImport.DataContext = sourceimportTreeViewlist;
                    }
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
    }
}
