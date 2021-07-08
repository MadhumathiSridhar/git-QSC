using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
//using QSC_Test_Automation.AP;
using System.Web;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using System.Xml;
using System.Xml.Linq;
using System.ComponentModel;
using System.Threading;
using System.Collections;
using System.Globalization;

namespace QSC_Test_Automation
{
    /// <summary>
    /// Interaction logic for Export.xaml
    /// </summary>
    public partial class Export : Window, INotifyPropertyChanged
    {
        //DBConnection QscDatabase = new DBConnection();
        //TreeViewExplorer ExportRootItem = null;
        List<TreeViewExplorer> treeViewExplorerExportList = new List<TreeViewExplorer>();
        List<TreeViewExplorer> searchTreeviewlst = new List<TreeViewExplorer>();
        bool isInventorySearchListSelected = false;
        public string sortingOrders = string.Empty;
        public string sortingOrders_OriginalList = string.Empty;
        private string status = string.Empty;
        QatMessageBox Qmsg = null;
        public Thread StartExport = null;

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
        private string _percentage = "0%";
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
                Percentage = value;
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

        public Export()
        {
            try
            {
                InitializeComponent();

              


             
            }
            catch(Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
            }
        }

        private void SetupImportTreeViewrFromDB(bool updateDataContextInventory, bool updateDataContextExecution, string sortingType, TreeViewExplorer ExportRootItem)
        {
            try
            {
                List<TreeViewExplorer> sortedTestPlanList = null;
                DBConnection QscDatabase = new DBConnection();
                Dictionary<int, string> designNameList = new Dictionary<int, string>();
                Dictionary<int, ArrayList> TpTcLinkTable = new Dictionary<int, ArrayList>();
                List<TreeViewExplorer> testPlanTreeViewList = QscDatabase.ReadTreeTable(QatConstants.DbTestPlanTable, 0, null, null);
                
                Dictionary<int, ArrayList> testCaseTreeViewList = QscDatabase.ReadTreeTable_executionWindow(QatConstants.DbTestCaseTable, 0, null);
           

                if (sortingType == "Descending")
                {
                    TreeViewExplorer[] alphaTestSuiteSorted = testPlanTreeViewList.ToArray();
                    Array.Sort(alphaTestSuiteSorted, new AlphanumComparatorFast());
                    Array.Reverse(alphaTestSuiteSorted);
                    sortedTestPlanList = alphaTestSuiteSorted.ToList();
                }
                else if (sortingType == "Ascending")
                {
                    TreeViewExplorer[] alphaTestSuiteSorted = testPlanTreeViewList.ToArray();
                    Array.Sort(alphaTestSuiteSorted, new AlphanumComparatorFast());
                    sortedTestPlanList = alphaTestSuiteSorted.ToList();
                }
                else if (sortingType == "No order")
                {
                    sortedTestPlanList = testPlanTreeViewList;
                }
                else if (sortingType == "Date Created Ascending")
                {
                    TreeViewExplorer[] alphaTestSuiteSorted = testPlanTreeViewList.ToArray();
                    Array.Sort(alphaTestSuiteSorted, new AlphanumComparatorFast_DateTime());
                    sortedTestPlanList = alphaTestSuiteSorted.ToList();

                }
                    designNameList = QscDatabase.ReadDesignerNameList();
              
                TpTcLinkTable = QscDatabase.ReadLinkTable(QatConstants.DbTestPlanTestCaseLinkTable);

               
                foreach (TreeViewExplorer testplan in sortedTestPlanList)
                {
                    
                 

                    List<TreeViewExplorer> testCaseList = new List<TreeViewExplorer>();
                   
                   if(TpTcLinkTable.ContainsKey(testplan.ItemKey))
                    {
                        foreach (int TcId in TpTcLinkTable[testplan.ItemKey])
                        {
                            
                                TreeViewExplorer TestCase = new TreeViewExplorer((int)testCaseTreeViewList[TcId][0], (string)testCaseTreeViewList[TcId][1], (string)testCaseTreeViewList[TcId][2], null, null, (DateTime?)testCaseTreeViewList[TcId][5], (string)testCaseTreeViewList[TcId][6], (DateTime?)testCaseTreeViewList[TcId][7], (string)testCaseTreeViewList[TcId][8], (string)testCaseTreeViewList[TcId][9], (string)testCaseTreeViewList[TcId][10], (DateTime?)testCaseTreeViewList[TcId][11], (string)testCaseTreeViewList[TcId][12], (int)testCaseTreeViewList[TcId][13], true);
                               TestCase.IsCheckedVisibility = Visibility.Hidden;
                                testCaseList.Add(TestCase);
                           
                        }
                        testplan.AddChildrenList_withCheckbox(testCaseList);

                    }
                    if(designNameList.ContainsKey(testplan.ItemKey))
                    testplan.DesignName = designNameList[testplan.ItemKey];

                }

                ExportRootItem.AddChildrenList_withCheckbox(sortedTestPlanList);
                if (ExportRootItem.Children.Count == 0)
                    ExportRootItem.IsCheckedVisibility = Visibility.Hidden;
                else
                    ExportRootItem.IsCheckedVisibility = Visibility.Visible;

                
                if (updateDataContextInventory)
                {
                    TreeViewExport.DataContext = null;
                    TreeViewExport.DataContext = treeViewExplorerExportList;
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

        public bool check_editedbyStatus(string TCquery, string TPquery)
        {
            bool success = false;
            //Qmsg = new QatMessageBox(this);
            try
            {
                TPquery = TPquery.TrimEnd(',');
                TCquery = TCquery.TrimEnd(',');
                string Editedin_TP = "select Testplanname,EditedBy FROM Testplan where TestPlanID in (" + TPquery + ") and EditedBy ! = ''";
                string Editedin_TC = "select Testcasename,EditedBy FROM Testcase where TestcaseID in (" + TCquery + ") and EditedBy ! = ''";
                string TP_TC_Editedvalues = dbread(Editedin_TP, Editedin_TC);
                //string TC_Editedvalues = dbread(Editedin_TC, "Testcases");

                if ((TP_TC_Editedvalues != string.Empty))
                {
                    //MessageBoxResult result = MessageBox.Show("Following are in Edit Mode"+ "\r\n" + "\r\n" + TP_Editedvalues + "\r\n" + TC_Editedvalues+ "\r\n"+" Do you want to continue export", "Update Alert", MessageBoxButton.OKCancel, MessageBoxImage.Information);
                    MessageBoxResult result = ExportMessageBox("Some Testplans/Testcases are being Edited by" + "\r\n" + "\r\n" + TP_TC_Editedvalues + "\r\n" + " Do you want to continue export ?", "Export Alert", MessageBoxButton.OKCancel, MessageBoxImage.Warning);
                    if (result == MessageBoxResult.OK)
                        success = true;
                    if (result == MessageBoxResult.Cancel)
                        success = false;
                }
                else
                {
                    success = true;
                }
                //if (TC_Editedvalues.Item1 != string.Empty)
                //{ }

            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
            }

            return success;
        }


        private string dbread(string TPquery, string TCquery)
        {
            string readvalue = string.Empty;
            string valuewithheader = string.Empty;
            string textheader = string.Empty;
            List<string> names = new List<string>();
            try
            {
                DBConnection QscDatabase = new DBConnection();
                DataTable TPquer = QscDatabase.SendCommand_Toreceive(TPquery);
                DataTableReader TPquer_read = TPquer.CreateDataReader();
                DataTable TCquer = QscDatabase.SendCommand_Toreceive(TCquery);
                DataTableReader TCquer_read = TCquer.CreateDataReader();
                while (TPquer_read.Read())
                {
                    if ((TPquer_read[0].ToString() != null) & (TPquer_read[1].ToString() != null))
                    {
                        if ((TPquer_read[0].ToString() != string.Empty) & (TPquer_read[1].ToString() != string.Empty))
                        {
                            if (!names.Contains(TPquer_read[1].ToString()))
                            {
                                //readvalue += aptypeRead[0].ToString() + "(Edited by-" + aptypeRead[1].ToString() + ")" + "\r\n";
                                readvalue += TPquer_read[1].ToString() + "\r\n";
                                names.Add(TPquer_read[1].ToString());
                                //Username += aptypeRead[1].ToString() + "\r\n";
                                //textheader = input + ":" + "\r\n";
                            }
                        }
                    }

                }

                while (TCquer_read.Read())
                {
                    if ((TCquer_read[0].ToString() != null) & (TCquer_read[1].ToString() != null))
                    {
                        if ((TCquer_read[0].ToString() != string.Empty) & (TCquer_read[1].ToString() != string.Empty))
                        {
                            if (!names.Contains(TCquer_read[1].ToString()))
                            {
                                //readvalue += aptypeRead[0].ToString() + "(Edited by-" + aptypeRead[1].ToString() + ")" + "\r\n";
                                readvalue += TCquer_read[1].ToString() + "\r\n";
                                names.Add(TCquer_read[1].ToString());
                                //Username += aptypeRead[1].ToString() + "\r\n";
                                //textheader = input + ":" + "\r\n";
                            }
                        }
                    }
                }

                valuewithheader = textheader + readvalue;
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
            }

            //return   new Tuple<string, string>(valuewithheader,string.Empty); 
            return valuewithheader;
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

        private void Export_Click(object sender, RoutedEventArgs e)
        {
            bool ExportSuccess = false;

            try
            {
                if (!Directory.Exists(QatConstants.QATServerPath))
                {
                    MessageBox.Show("Invalid Server path", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                else if (!hasWriteAccessToFolder(@QatConstants.QATServerPath))
                {
                    ExportMessageBox("Access to the server path is denied", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                else
                {

                    ///  checked if atleast 1 test case is checked
                    /// 
                    ExportDisable();

                    ////////Get TestPlanID from selection
                    List<int> tpId = new List<int>();
                    List<int> tcId = new List<int>();
                    string tpedit = string.Empty;
                    string tcedit = string.Empty;
                    Dictionary<string, string[]> serverfolderPath = new Dictionary<string, string[]>();
                    bool valid = false;
                    int TPcount = 0;
                    int I = 0;
                    int designFileCount = 0;
                    List<string> emptyDesign = new List<string>();
					
                    List<TreeViewExplorer> treeviewLst = TreeViewExport.DataContext as List<TreeViewExplorer>;

                    if (treeviewLst != null)
                    {
                        foreach (TreeViewExplorer testPlanlist in treeviewLst)
                        {
                            foreach (TreeViewExplorer testPlan in testPlanlist.Children)
                            {
                                if (testPlan.IsChecked == true)
                                {
                                    if (I != 30)
                                    {
                                        ProgressPercentage(I.ToString());
                                    }

                                    if (!valid)
                                    {
                                        valid = true;

                                    }

                                    TPcount++;

                                    if (!tpId.Contains(testPlan.ItemKey))
                                    {
                                        tpedit += "'" + testPlan.ItemKey + "'" + ",";

                                        string serverdeatils = Path.Combine("Designs", testPlan.DesignName);
                                        string extension = Path.GetExtension(testPlan.DesignName);
                                        string newdesignFile = Path.Combine("Designs", "D" + designFileCount.ToString() + extension);
                                        if (testPlan.DesignName != string.Empty)
                                        {
                                            if (!serverfolderPath.Keys.Contains(serverdeatils))
                                            {
                                                string[] val = new string[5];
                                                val[0] = newdesignFile;
                                                val[1] = testPlan.ItemKey.ToString();
                                                val[2] = string.Empty;
                                                val[3] = testPlan.HasDesign.ToString();

                                                serverfolderPath.Add(serverdeatils, val);
                                                designFileCount++;
                                            }
                                        }
                                        else
                                        {
                                            emptyDesign.Add(testPlan.ItemName);
                                        }

                                        if (!emptyDesign.Contains(testPlan.ItemName))
                                        {
                                            tpId.Add(testPlan.ItemKey);

                                            foreach (TreeViewExplorer testcase in testPlan.Children)
                                            {
                                                if (!tcId.Contains(testcase.ItemKey))
                                                {
                                                    tcId.Add(testcase.ItemKey);
                                                    tcedit += "'" + testcase.ItemKey + "'" + ",";
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                    if (emptyDesign.Count > 0)
                    {
                        ExportMessageBox("The following TestPlan designs are empty.So it cannot be Exported\n\t" + string.Join("\n\t", emptyDesign), "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);

                        if (emptyDesign.Count == TPcount)
                        {
                            ExportMessageBox("Export Unsuccessful", "Information", MessageBoxButton.OK, MessageBoxImage.Warning);
                            ExportEnable();
                            ProgressPercentage("0");
                            return;
                        }
                    }

                    //if ((TPcount<=50)&&(valid) && (textBox_exportname.Text.Trim() != string.Empty) && (Properties.Settings.Default.TesterName.Trim().ToString() != string.Empty))
                    //{
                    //if (TPcount>50)
                    //{
                    //ExportMessageBox("No of Test plans selected is " +TPcount.ToString()+". "+"Please select Maximum of 50 testplans to Export  ", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    //    ExportEnable();
                    //    ProgressPercentage("0");
                    //    return;
                    //}
                    if (!valid)
                    {
                        ExportMessageBox("Please select a test plan to export ", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                        ExportEnable();
                        ProgressPercentage("0");
                        return;
                    }
                    if (textBox_exportname.Text.Trim() == string.Empty)
                    {
                        ExportMessageBox("Please enter name to export", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                        ExportEnable();
                        ProgressPercentage("0");
                        return;
                    }
                    if (Properties.Settings.Default.TesterName.Trim().ToString() == string.Empty)
                    {
                        ExportMessageBox("Please Enter Testername in preferences", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                        ExportEnable();
                        ProgressPercentage("0");
                        return;
                    }

                    if (check_editedbyStatus(tcedit, tpedit))
                    {
                        var dialog = new System.Windows.Forms.FolderBrowserDialog();
                        //dialog.SelectedPath = @"\\testingproject\Sharing\QSC_SourceCode_Phase1\Qsys_softwares\Designer Exe's";//\Q-SYS_Designer_Installer_5.1.31


                        if (!Directory.Exists(@QatConstants.QATServerPath + "\\Export"))
                        {
                            Directory.CreateDirectory(@QatConstants.QATServerPath + "\\Export");
                        }

                        dialog.SelectedPath = @QatConstants.QATServerPath + "\\Export";
                        System.Windows.Forms.DialogResult result = dialog.ShowDialog();

                        if (result.ToString() != "Cancel")
                        {
                            string sourcepath = dialog.SelectedPath;
                            if (!hasWriteAccessToFolder(sourcepath))
                            {
                                ExportMessageBox("Selected folder dont have write access", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                                //MessageBox.Show("Selected folder dont have write access");
                                ProgressPercentage("0");
                                ExportEnable();
                                return;
                            }

                            //ExportDisable();

                            string Export_name = textBox_exportname.Text.Trim();

                            string[] savedFilepath = new string[] { @sourcepath, Export_name + ".zip" };
                            string serverPath = QatConstants.QATServerPath;
                            ProgressPercentage("30");

                            /////////Get All fileNames(Design, AP, Responsalyzer)
                            //string designqueries = "select Designname from designtable where DesignID in (select DesignID from TPDesignLinkTable where TPID in ('" + string.Join("','", tpId) + "'))";
                            //serverfolderPath = GetFilesFormDB(serverfolderPath, designqueries, "Designs", string.Empty);
                            bool Import_Running = DeviceDiscovery.IsImportRunning();
                            bool Execution_Running = DeviceDiscovery.IsExecutionRunning();

                            if (!Import_Running)
                            {
                                ExportMessageBox("Please wait for Import process to finish & start export ", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                                ExportEnable();
                                ProgressPercentage("0");
                                return;
                            }
                            if (!Execution_Running)
                            {
                                ExportMessageBox("Please wait for Execution process to finish & start export", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                                ExportEnable();
                                ProgressPercentage("0");
                                return;
                            }

                            StartExport = new Thread(() =>
                                CallExportFunctionThread(tcId, tpId, ExportSuccess, serverfolderPath, sourcepath, Export_name, savedFilepath, serverPath));
                            StartExport.Start();


                        }
                        else
                        {
                            this.Dispatcher.Invoke(() =>
                            {
                                ExportEnable();
                            });
                        }
                    }
                    else
                    {
                        ExportEnable();

                    }
                }
                //}
                //else if (TPcount>50)
                //{
                //    ExportMessageBox("No of Test plans selected is " +TPcount.ToString()+". "+"Please select Maximum of 50 testplans to Export  ", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                //    //this.IsEnabled = true;
                //}
                //else if (!valid)
                //{
                //    ExportMessageBox("Please select a test plan to export ", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                //    //this.IsEnabled = true;
                //}
                //else if ((textBox_exportname.Text.Trim() == string.Empty))
                //{
                //    ExportMessageBox("Please enter name to export", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                //    //this.IsEnabled = true;
                //}
                //else if (Properties.Settings.Default.TesterName.Trim().ToString() == string.Empty)
                //{
                //    ExportMessageBox("Please Enter Testername in preferences", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                //}

                //ExportEnable();
            }
            catch (Exception ex)
            {
                if (!ExportSuccess)
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        ExportMessageBox("Export Unsuccessful", "Information", MessageBoxButton.OK, MessageBoxImage.Warning);
                        ExportEnable();
                    });
                }
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
            }
        }

        private void CallExportFunctionThread(List<int> tcId, List<int> tpId, bool ExportSuccess, Dictionary<string, string[]> serverfolderPath, string sourcepath, string Export_name, string[] savedFilepath, string serverPath)
        {
            try
            {
                this.Dispatcher.Invoke(() =>
                {
                    ExportDisable();
                });

                DBConnection QscDatabase = new DBConnection();
                Dictionary<string, string[]> combinedServerPath = new Dictionary<string, string[]>();
                combinedServerPath = new Dictionary<string, string[]>(serverfolderPath);

                string approjectquery = "select APxPath,TCID,ActionID from APVerification where TCID in ('" + string.Join("','", tcId) + "')";
                var apxserverfolder = GetFilesFormDB(approjectquery, "Audio Precision", "AP Project Files", QscDatabase, combinedServerPath,0);

                string apsettingsquery = "select WaveformType,TCID,ActionID,APSettingsID from APSettings where TCID in ('" + string.Join("','", tcId) + "')";
                var apWavefolderPath = GetFilesFormDB(apsettingsquery, "Audio Precision", "AP Waveform Files", QscDatabase, combinedServerPath, 0);
                
                string apsettingsquery1 = "select WaveformType,TCID,ActionID,LevelAndGainID from LevelAndGain where TCID in ('" + string.Join("','", tcId) + "')";
                var apWavefolderPath1 = GetFilesFormDB(apsettingsquery1, "Audio Precision", "AP Waveform Files", QscDatabase, combinedServerPath, apWavefolderPath.Item1.Count);

                Dictionary<string, string[]> apVerificationFolderPath = new Dictionary<string, string[]>();
                
                string aptypequery = "select distinct VerificationType from APVerification where TCID in ('" + string.Join("','", tcId) + "')";
                DataTable aptypetble = QscDatabase.SendCommand_Toreceive(aptypequery);
                DataTableReader aptypeRead = aptypetble.CreateDataReader();
                while (aptypeRead.Read())
                {
                    string apType = aptypeRead[0].ToString();

                    string tableName = string.Empty;
                    if (apType == "Stepped Frequency Sweep")
                    {
                        tableName = "APSteppedFreqSweepSettings";
                    }
                    else if (apType == "Phase")
                    {
                        tableName = "APPhaseSettings";
                    }
                    else if (apType == "Frequency sweep")
                    {
                        tableName = "APFrequencyResponse";
                    }
                    else if (apType == "THD+N")
                    {
                        tableName = "APTHDNSettings";
                    }

                    if (tableName != string.Empty)
                    {
                        string frqquery = "select VerificationLocation,TCID,ActionID from " + tableName + " where TCID in ('" + string.Join("','", tcId) + "')";
                        var apVerificationFolderPaths = GetFilesFormDB(frqquery, "Audio Precision", "Verification Files", QscDatabase, combinedServerPath, apVerificationFolderPath.Count);
                        apVerificationFolderPath = apVerificationFolderPaths.Item1.Union(apVerificationFolderPath).ToDictionary(k => k.Key, v => v.Value);
                       
                    }
                }

                string Responequery = "select VerificationFileLocation,TCID,ActionID,ResponsalyzerID from Responsalyzer where TCID in ('" + string.Join("','", tcId) + "')";
                var reponseVerfyFile = GetFilesFormDB(Responequery, "Responsalyzer", "Reference Files", QscDatabase, combinedServerPath,0);

                string QRCMFilesquery = "select Distinct PayloadFilePath,TCID from QRCMAction where TCID in ('" + string.Join("','", tcId) + "') and PayloadFilePath!='' UNION select ReferenceFilePath,TCID from QRCMVerification where TCID in ('" + string.Join("','", tcId) + "') and ReferenceFilePath!=''";
                var qrcmActionFile = GetFilesFormDB(QRCMFilesquery, string.Empty, "QRCM_Files", QscDatabase, combinedServerPath, 0);
      

                ////////// Save Design, AP, Alyzer Files from server path to local temp path
                List<string> filenotExist = new List<string>();
                List<string> copynotSuccess = new List<string>();

                //string tickscreate = Convert.ToString(DateTime.Now.Ticks);
                
                var directorycreate = new DirectoryInfo(@"C:\Users\" + Environment.UserName + @"\AppData\Local\Temp\QATExport");

                if (directorycreate.Exists)
                {
                    DeleteFolder(directorycreate);
                }

                string localPath = directorycreate.FullName;
                if (!Directory.Exists(localPath))
                {
                    Directory.CreateDirectory(localPath);
                }

                
                XmlWriterSettings settings = new XmlWriterSettings
                {
                    Indent = true,
                    IndentChars = "  ",
                    NewLineChars = "\r\n",
                    NewLineHandling = NewLineHandling.Replace,
                    ConformanceLevel = ConformanceLevel.Auto,
                    CheckCharacters = false
                };

                using (XmlWriter writer = XmlWriter.Create(@localPath + "\\FileSettings.xml", settings))
                {
                    writer.WriteStartElement("XMLFiles");
                    writer.WriteStartElement("DesignFiles");
                    var returnfiles = CopyFilestoTempFolder(writer, new List<Dictionary<string, string[]>> { serverfolderPath }, serverPath, localPath, filenotExist, copynotSuccess, "DesignFile");
                    filenotExist = returnfiles.Item1;
                    copynotSuccess = returnfiles.Item2;
                    writer.WriteEndElement();


                    writer.WriteStartElement("APProjectFiles");
                    var returnApfiles = CopyFilestoTempFolder(writer, new List<Dictionary<string, string[]>> { apxserverfolder.Item1 }, serverPath, localPath, filenotExist, copynotSuccess, "APProjectFile");
                    filenotExist = returnApfiles.Item1;
                    copynotSuccess = returnApfiles.Item2;
                    writer.WriteEndElement();
                    

                    writer.WriteStartElement("APWaveFiles");
                    var returnAPWavefiles = CopyFilestoTempFolder(writer, new List<Dictionary<string, string[]>> { apWavefolderPath.Item1, apWavefolderPath1.Item1 }, serverPath, localPath, filenotExist, copynotSuccess, "APWaveFile");
                    filenotExist = returnAPWavefiles.Item1;
                    copynotSuccess = returnAPWavefiles.Item2;
                    writer.WriteEndElement();

                    writer.WriteStartElement("APVerificationFiles");
                    var returnVerifyfiles = CopyFilestoTempFolder(writer, new List<Dictionary<string, string[]>> { apVerificationFolderPath }, serverPath, localPath, filenotExist, copynotSuccess, "APVerificationFile");
                    filenotExist = returnVerifyfiles.Item1;
                    copynotSuccess = returnVerifyfiles.Item2;
                    writer.WriteEndElement();

                    writer.WriteStartElement("ResponsalyzerVerificationFiles");
                    var returnResponsefiles = CopyFilestoTempFolder(writer, new List<Dictionary<string, string[]>> { reponseVerfyFile.Item1 }, serverPath, localPath, filenotExist, copynotSuccess, "ResponsalyzerFile");
                    filenotExist = returnResponsefiles.Item1;
                    copynotSuccess = returnResponsefiles.Item2;
                    writer.WriteEndElement();

                    writer.WriteStartElement("QRCMFiles");
                    var returnQRCMfiles = CopyFilestoTempFolder(writer, new List<Dictionary<string, string[]>> { qrcmActionFile.Item1 }, serverPath, localPath, filenotExist, copynotSuccess, "QRCMFile");
                    filenotExist = returnQRCMfiles.Item1;
                    copynotSuccess = returnQRCMfiles.Item2;
                    writer.WriteEndElement();
                 
                    writer.WriteEndElement();
                }

                ///// Give warning to user if any error occurs while copy the files.
                List<string> hasErrorMsg = new List<string>();
                hasErrorMsg.AddRange(filenotExist);
                hasErrorMsg.AddRange(copynotSuccess);

                bool msgTrue = true;

                if (filenotExist.Count > 0 && copynotSuccess.Count > 0)
                {
                    MessageBoxResult res = ExportMessageBox("The following files not exist in the server path. \n\t" + string.Join("\n\t", filenotExist) + "\n\nThe following files will not be copied as file name is too large \n\t" + string.Join("\n\t", copynotSuccess) + "\n\nClick Yes to continue without the file \nNo to Cancel", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                    if (res == MessageBoxResult.No)
                        msgTrue = false;
                }
                else if(filenotExist.Count > 0 && copynotSuccess.Count == 0)
                {
                    MessageBoxResult res = ExportMessageBox("The following files not exist in the server path, \n\t" + string.Join("\n\t", filenotExist) + "\n\nClick Yes to continue without the file \nNo to Cancel", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                    if (res == MessageBoxResult.No)
                        msgTrue = false;
                }
                else if(filenotExist.Count == 0 && copynotSuccess.Count > 0)
                {
                    MessageBoxResult res = ExportMessageBox("The following files will not be copied as file name is too large \n\t" + string.Join("\n\t", copynotSuccess) + "\n\nClick Yes to continue without the file \nNo to Cancel", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                    if (res == MessageBoxResult.No)
                        msgTrue = false;
                }

                if(msgTrue == true)
                {
                    ExportSuccess = saveXML(tpId, hasErrorMsg, combinedServerPath, localPath, serverPath, savedFilepath);
                }
                else
                {
                    var direct = new DirectoryInfo(@"C:\Users\" + Environment.UserName + @"\AppData\Local\Temp\QATExport");
                    System.IO.DirectoryInfo exportdir = new DirectoryInfo(direct.FullName);

                    DeleteFolder(exportdir);
                    exportdir.Delete();
                    this.Dispatcher.Invoke(() =>
                    {
                        ExportEnable();
                        ProgressPercentage("0");
                    });

                    return;
                }

                if (ExportSuccess == true && Percentage == "100%")
                {
                    MessageBoxResult response_out = ExportMessageBox(" Export completed successfully" , "Information", MessageBoxButton.OK, MessageBoxImage.Information);
               
                }
                else
                {
                    ExportMessageBox("Export Unsuccessful", "Information", MessageBoxButton.OK, MessageBoxImage.Warning);
                    ProgressPercentage("0");
                }
                this.Dispatcher.Invoke(() =>
                {
                    ExportEnable();
                    ProgressPercentage("0");

                });
            }
            catch (Exception ex)
            {
                if (this.IsVisible)
                {
                    this.Dispatcher.Invoke(() =>
                    {

                        ExportMessageBox("Export Unsuccessful", "Information", MessageBoxButton.OK, MessageBoxImage.Warning);
                        ProgressPercentage("0");
                        ExportEnable();

                    });
                }
            }
            finally
            {
                if (StartExport != null)
                {
                    StartExport=null;
                }
            }
        }

        private Tuple<List<string>, List<string>> CopyFilestoTempFolder(XmlWriter writer,List<Dictionary<string, string[]>> serverfolderPath, string serverPath, string localPath, List<string> filenotExist, List<string> copynotSuccess, string xmlHeader)
        {
            try
            {
                foreach (var xxx in serverfolderPath)
                {
                    foreach (var folderStructure in xxx)
                    {
                        string filename1 = string.Empty;
                        string filename2 = string.Empty;

                        string serverPathWithFileName = System.IO.Path.Combine(serverPath, folderStructure.Key);
                        string localPathWithFileName = Path.Combine(localPath, folderStructure.Value[0]);

                        try
                        {
                            string localdirectorypath = Path.GetDirectoryName(localPathWithFileName);
                            if (!Directory.Exists(localdirectorypath))
                            {
                                Directory.CreateDirectory(localdirectorypath);
                            }

                            if (File.Exists(serverPathWithFileName))
                            {

                                File.Copy(serverPathWithFileName, localPathWithFileName, true);
                                File.SetAttributes(localPath, FileAttributes.Normal);

                                filename1 = Path.GetFileName(serverPathWithFileName);
                                filename2 = Path.GetFileName(localPathWithFileName);

                                if (filename1 != null && filename1 != string.Empty)
                                {
                                    writer.WriteStartElement(xmlHeader);

                                    try
                                    {
                                        writer.WriteAttributeString("ActualName", filename1);
                                        writer.WriteAttributeString("DummyName", filename2);

                                        string extension = Path.GetExtension(filename2);

                                        if (extension != ".qsys")
                                        {
                                            if (folderStructure.Value[1] != null)
                                                writer.WriteAttributeString("TCID", folderStructure.Value[1]);

                                            if (folderStructure.Value[2] != null)
                                                writer.WriteAttributeString("TAID", folderStructure.Value[2]);
                                        }
                                        else
                                        {
                                            if (folderStructure.Value[1] != null)
                                                writer.WriteAttributeString("TPID", folderStructure.Value[1]);
                                        }

                                        if (xmlHeader == "ResponsalyzerFile")
                                        {
                                            if (folderStructure.Value[3] != null)
                                                writer.WriteAttributeString("ResponsalyzerID", folderStructure.Value[3]);
                                        }

                                        if (xmlHeader == "APWaveFile")
                                        {
                                            if (folderStructure.Value[3] != null)
                                                writer.WriteAttributeString(folderStructure.Value[4], folderStructure.Value[3]);
                                        }

                                    }
                                    catch (Exception ex) { }

                                    writer.WriteEndElement();
                                }
                            }
                            else
                            {
                                if (!filenotExist.Contains(serverPathWithFileName) && Convert.ToBoolean(folderStructure.Value[3]) == true)
                                    filenotExist.Add(serverPathWithFileName);
                            }
                        }
                        catch (Exception ex)
                        {
                            if (!copynotSuccess.Contains(serverPathWithFileName))
                                copynotSuccess.Add(serverPathWithFileName);
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
            }

            return new Tuple<List<string>, List<string>>(filenotExist, copynotSuccess);
        }

        private MessageBoxResult ExportMessageBox(string messageBoxTest, string messageBoxCaption, MessageBoxButton messageBoxButton, MessageBoxImage messageBoxImage)
        {
           
            MessageBoxResult result = MessageBoxResult.None;
            try
            {
                this.Dispatcher.Invoke(() =>
                {
                    Qmsg = new QatMessageBox(this);
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

        private void ExportEnable()
        {
            try
            {
                if(TreeViewExport.Items != null && TreeViewExport.Items.Count > 0)
                {
                    var treeHeader = TreeViewExport.Items[0] as TreeViewExplorer;
                    treeHeader.IsEnabled = true;
                }
				
                Export_Ok.IsEnabled = true;
                textBox_exportname.IsEnabled = true;
            }
            catch(Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
            }
        }

        private void ExportDisable()
        {
            try
            {
                if (TreeViewExport.Items != null && TreeViewExport.Items.Count > 0)
                {
                    var treeHeader = TreeViewExport.Items[0] as TreeViewExplorer;
                    treeHeader.IsEnabled = false;
                }
				
                Export_Ok.IsEnabled = false;
                textBox_exportname.IsEnabled = false;
            }
            catch(Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
            }
        }

        private Tuple<Dictionary<string, string[]>, Dictionary<string,string[]>> GetFilesFormDB(string query, string combine1, string combine2, DBConnection QscDatabase, Dictionary<string, string[]> combinedServerPath, int count)
        {
            Dictionary<string, string[]> fileWithPath = new Dictionary<string, string[]>();

            try
            {
                string prefix = string.Empty;

                if(combine2 == "AP Project Files")
                {
                    prefix = "AP";
                }
                else if(combine2 == "AP Waveform Files")
                {
                    prefix = "AW";
                }
                else if (combine2 == "Verification Files")
                {
                    prefix = "AV";
                }
                else if (combine2 == "Reference Files")
                {
                    prefix = "RA";
                }
                else if (combine2 == "QRCM_Files")
                {
                    prefix = "QC";
                }
            
                string[] fileType = new string[5] { "Sine", "Sine, Dual", "Sine, Var Phase", "Noise", "IMD" };
                //int count = 0;
               
                DataTable designtble = QscDatabase.SendCommand_Toreceive(query);
                DataTableReader read = designtble.CreateDataReader();
                while (read.Read())
                {
                    string[] val = new string[5];

                    if (combine2 == "AP Waveform Files")
                    {
                        string fileName = read[0].ToString();
                        
                        if (fileName != string.Empty && fileName != null)
                        {
                            string extension = Path.GetExtension(fileName);

                            if (!fileType.Contains(fileName))
                            {
                                string oldserver = Path.Combine(combine1, combine2, fileName);
                                string newserver = Path.Combine(combine1, combine2, prefix + count.ToString() + extension);

                                if (!fileWithPath.Keys.Contains(oldserver))
                                {
                                    val[0] = newserver;

                                    if (read[1] != null && read[1].ToString() != string.Empty)
                                    {
                                        val[1] = read[1].ToString();
                                    }

                                    if (read[2] != null && read[2].ToString() != string.Empty)
                                    {
                                        val[2] = read[2].ToString();
                                    }

                                    if (combine1 == "Responsalyzer" || combine2 == "AP Waveform Files")
                                    {
                                        if (read[3] != null && read[3].ToString() != string.Empty)
                                        {
                                            val[3] = read[3].ToString();

                                            if(query.Contains("APSettingsID"))
                                            {
                                                val[4] = "APSettingsID";
                                            }

                                            if (query.Contains("LevelAndGainID"))
                                            {
                                                val[4] = "LevelAndGainID";
                                            }
                                        }
                                    }

                                    fileWithPath.Add(oldserver, val);

                                    if(!combinedServerPath.Keys.Contains(oldserver))
                                    {
                                        combinedServerPath.Add(oldserver, val);
                                    }

                                    count++;
                                }
                            }
                        }
                    }
                    else
                    {
                        string fileName = read[0].ToString();

                        if (fileName != string.Empty && fileName != null)
                        {
                            string extension = Path.GetExtension(fileName);

                            string oldserver = Path.Combine(combine1, combine2, fileName);
                            string newserver = Path.Combine(combine1, combine2, prefix + count.ToString() + extension);
                      
                            if (!fileWithPath.Keys.Contains(fileName))
                            {
                                val[0] = newserver;

                                if (read[1] != null && read[1].ToString() != string.Empty)
                                {
                                    val[1] = read[1].ToString();
                                }
                                if (combine2 != "QRCM_Files")
                                {
                                    if (read[2] != null && read[2].ToString() != string.Empty)
                                    {
                                        val[2] = read[2].ToString();
                                    }
                                }

                                if (combine1 == "Responsalyzer" || combine2 == "AP Waveform Files")
                                {
                                    if (read[3] != null && read[3].ToString() != string.Empty)
                                    {
                                        val[3] = read[3].ToString();
                                    }
                                }
                               
                                fileWithPath.Add(oldserver, val);

                                if (!combinedServerPath.Keys.Contains(oldserver))
                                {
                                    combinedServerPath.Add(oldserver, val);
                                }

                                count++;
                            }
                        }
                    }
                }

                return new Tuple<Dictionary<string, string[]>, Dictionary<string, string[]>>(fileWithPath, combinedServerPath);
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                return new Tuple<Dictionary<string, string[]>, Dictionary<string, string[]>>(fileWithPath, combinedServerPath);
            }
        }                        
        
        public bool saveXML(List<int> tpId, List<string> hasErrorMsg, Dictionary<string, string[]> serverfolderPath, string localPath, string serverPath, string[] savedfilepath)
        {
            try
            {
                int Inc = 0;

                XmlWriterSettings settings = new XmlWriterSettings
                {
                    Indent = true,
                    IndentChars = "  ",
                    NewLineChars = "\r\n",
                    NewLineHandling = NewLineHandling.Replace,
                    ConformanceLevel = ConformanceLevel.Auto,
                    CheckCharacters = false
                };

                DBConnection QscDatabase = new DBConnection();

                using (XmlWriter writer = XmlWriter.Create(@localPath + "\\ExportXML.xml", settings))
                {

                    SqlConnection sqlConnections = QscDatabase.CreateConnection();
                    sqlConnections.Open();

                    string sqlServerVersions = HttpUtility.HtmlEncode(sqlConnections.ServerVersion);
                    string QATVersions = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
                    string DBName_Designers = HttpUtility.HtmlEncode(QatConstants.DbDatabaseName);
                    string Exported_By = HttpUtility.HtmlEncode(Properties.Settings.Default.TesterName);

                    writer.WriteStartElement("Export");

                    writer.WriteStartElement("UserDetails");

                    writer.WriteStartElement("SQLVersion");
                    writer.WriteString(sqlServerVersions);
                    writer.WriteEndElement();

                    writer.WriteStartElement("QATVersion");
                    writer.WriteString(QATVersions);
                    writer.WriteEndElement();

                    writer.WriteStartElement("DesignerDBName");
                    writer.WriteString(DBName_Designers);
                    writer.WriteEndElement();

                    writer.WriteStartElement("ExportedDetails");

                    writer.WriteStartElement("ExportedBy");
                    writer.WriteString(Exported_By);
                    writer.WriteEndElement();

                    writer.WriteStartElement("ExportedOn");
                    writer.WriteString(DateTime.Now.ToString("MM-dd-yyyy hh:mm:ss tt"));
                    writer.WriteEndElement();

                    writer.WriteEndElement();

                    writer.WriteEndElement();

                    //------------------------------------File Details      Started ----------------------------------------------------------------------------

                    writer.WriteStartElement("FileDetails");


                    writer.WriteStartElement("Designfiles");

                    foreach (var fileNames in serverfolderPath)
                    {
                        string oldfilename = fileNames.Key;
                        string[] fileName = fileNames.Value;
                        if (fileName[0] != null && fileName[0] != string.Empty)
                        {
                            //string file_name = Path.GetFileName(fileName[0]);
                            //string oldfile_name = Path.GetFileName(serverfolderPath[fileName]);
                           string oldfile_name = Path.GetFileName(oldfilename);

                            string serverpathWithFile = Path.Combine(serverPath, fileName[0]);

                            string[] filesplit = serverpathWithFile.Split(Path.DirectorySeparatorChar);

                            if (filesplit[filesplit.Count() - 2] == "Designs")
                            {
                                //file_name = HttpUtility.HtmlEncode(file_name);
                                oldfile_name = HttpUtility.HtmlEncode(oldfile_name);

                                writer.WriteStartElement("Designfile");
                                writer.WriteAttributeString("FileName", oldfile_name);
                                //writer.WriteAttributeString("NewDesignFileName", file_name);

                                if (hasErrorMsg.Contains(serverpathWithFile))
                                {
                                    writer.WriteAttributeString("IsExportSucces", "false");
                                }
                                else
                                {
                                    writer.WriteAttributeString("IsExportSucces", "true");
                                }

                                writer.WriteEndElement();
                            }
                        }
                    }

                    writer.WriteEndElement();

                    writer.WriteStartElement("APFiles");

                    foreach (var fileName in serverfolderPath)
                    {
                        if (fileName.Key != null && fileName.Key != string.Empty)
                        {
                            string file_name = Path.GetFileName(fileName.Key);

                            string serverpathWithFile = Path.Combine(serverPath, fileName.Key);

                            string[] filesplit = serverpathWithFile.Split(Path.DirectorySeparatorChar);

                            if (filesplit[filesplit.Count() - 2] == "AP Project Files")
                            {
                                file_name = HttpUtility.HtmlEncode(file_name);

                                writer.WriteStartElement("APTemplate");
                                writer.WriteAttributeString("DesignFileName", file_name);

                                if (hasErrorMsg.Contains(serverpathWithFile))
                                {
                                    writer.WriteAttributeString("IsExportSucces", "false");
                                }
                                else
                                {
                                    writer.WriteAttributeString("IsExportSucces", "true");
                                }

                                writer.WriteEndElement();
                            }
                            else if (filesplit[filesplit.Count() - 2] == "AP Waveform Files")
                            {
                                file_name = HttpUtility.HtmlEncode(file_name);

                                writer.WriteStartElement("APGeneratorFile");
                                writer.WriteAttributeString("DesignFileName", file_name);

                                if (hasErrorMsg.Contains(serverpathWithFile))
                                {
                                    writer.WriteAttributeString("IsExportSucces", "false");
                                }
                                else
                                {
                                    writer.WriteAttributeString("IsExportSucces", "true");
                                }

                                writer.WriteEndElement();
                            }
                            else if (filesplit[filesplit.Count() - 2] == "Verification Files")
                            {
                                file_name = HttpUtility.HtmlEncode(file_name);

                                writer.WriteStartElement("APVerficationFile");
                                writer.WriteAttributeString("DesignFileName", file_name);

                                if (hasErrorMsg.Contains(serverpathWithFile))
                                {
                                    writer.WriteAttributeString("IsExportSucces", "false");
                                }
                                else
                                {
                                    writer.WriteAttributeString("IsExportSucces", "true");
                                }

                                writer.WriteEndElement();
                            }
                        }
                    }

                    writer.WriteEndElement();




                    writer.WriteStartElement("ResponsalyzerFiles");

                    foreach (var fileName in serverfolderPath)
                    {
                        if (fileName.Key != null && fileName.Key != string.Empty)
                        {
                            string file_name = Path.GetFileName(fileName.Key);

                            string serverpathWithFile = Path.Combine(serverPath, fileName.Key);

                            string[] filesplit = serverpathWithFile.Split(Path.DirectorySeparatorChar);

                            if (filesplit[filesplit.Count() - 2] == "Reference Files")
                            {
                                file_name = HttpUtility.HtmlEncode(file_name);

                                string originalFileName = Path.GetFileName(fileName.Key);
                                writer.WriteStartElement("ResponsalyzerFile");
                                writer.WriteAttributeString("FileName", originalFileName);

                                if (hasErrorMsg.Contains(serverpathWithFile))
                                {
                                    writer.WriteAttributeString("IsExportSucces", "false");
                                }
                                else
                                {
                                    writer.WriteAttributeString("IsExportSucces", "true");
                                }

                                writer.WriteEndElement();
                            }
                        }
                    }

                    writer.WriteEndElement();


                    writer.WriteEndElement();


                    //-----------------------TestPlan Started-----------------------------------------------------//

                    writer.WriteStartElement("Testplan");

                    string tpquery = "select * from Testplan where TestPlanID in ('" + string.Join("','", tpId) + "')";

                    DataTable tptble = QscDatabase.SendCommand_Toreceive(tpquery);
                    for (int k = 0; k < tptble.Rows.Count; k++)
                    {
                        writer.WriteStartElement("Testplan");

                        for (int j = 0; j < tptble.Columns.Count; j++)
                        {
                            //if (tptble.Columns[j].ColumnName == "CreatedOn")
                            //{
                            //    DateTime sdd = Convert.ToDateTime(tptble.Rows[k][j]);
                            //    writer.WriteAttributeString(tptble.Columns[j].ColumnName, sdd.ToString("yyyy-MM-dd"));
                            //}
                            //else
                            //{
                                writer.WriteAttributeString(tptble.Columns[j].ColumnName, tptble.Rows[k][j].ToString());
                            //}
                        }

                        writer.WriteEndElement();
                    }

                    writer.WriteEndElement();

                    //--------------------TestPlan         finished-----------------------------------------------------

                    List<string> tptablelist = new List<string>();
                    string testquery = "sp_fkeys 'Testplan'";
                    System.Data.DataTable tptbl = QscDatabase.SelectDTWithParameter(testquery, string.Empty, string.Empty);
                    DataTableReader dtRead = tptbl.CreateDataReader();
                    while (dtRead.Read())
                    {
                        if (dtRead[6] != System.DBNull.Value)
                        {
                            tptablelist.Add(dtRead[6].ToString());
                        }
                    }

                    foreach (string gett in tptablelist)
                    {
                        double percent65 = tptablelist.Count();

                        List<string> designtablelist = new List<string>();

                        string tpquery1 = string.Empty;
                        string querys = string.Empty;
                        if (gett == "TPDesignLinkTable")
                        {
                            tpquery1 = "sp_fkeys 'designtable'";
                            designtablelist.Add("designtable");
                            querys = "select DesignID from " + gett + " where TPID in ('" + string.Join("','", tpId) + "')";
                        }
                        else if (gett == "TPMonitorLinkTable")
                        {
                            tpquery1 = "sp_fkeys 'BackgroundMonitoring'";
                            designtablelist.Add("BackgroundMonitoring");
                            querys = "select BMID from " + gett + " where TPID in ('" + string.Join("','", tpId) + "')";
                        }
                        else if (gett == "TPTCLinkTable")
                        {
                            tpquery1 = "sp_fkeys 'Testcase'";
                            designtablelist.Add("Testcase");
                            querys = "select TCID from " + gett + " where TPID in ('" + string.Join("','", tpId) + "')";
                        }

                        System.Data.DataTable tptbl1 = QscDatabase.SelectDTWithParameter(tpquery1, string.Empty, string.Empty);
                        DataTableReader dtRead2 = tptbl1.CreateDataReader();
                        while (dtRead2.Read())
                        {
                            if (dtRead2[6] != System.DBNull.Value)
                                designtablelist.Add(dtRead2[6].ToString());
                        }

                        if(gett == "TPTCLinkTable")
                        {
                            if(designtablelist.Contains("TestAction"))
                            {
                                tpquery1 = "sp_fkeys 'TestAction'";
                                System.Data.DataTable tptbl2 = QscDatabase.SelectDTWithParameter(tpquery1, string.Empty, string.Empty);
                                DataTableReader dtRead3 = tptbl2.CreateDataReader();
                                while (dtRead3.Read())
                                {
                                    if (dtRead3[6] != System.DBNull.Value)
                                        designtablelist.Add(dtRead3[6].ToString());
                                }
                            }
                        }

                        List<int> designIdlist = new List<int>();
                        List<List<int>> designIDSplit = new List<List<int>>();
                        List<int> inDesignID = new List<int>();

                        DataTable tbles = QscDatabase.SendCommand_Toreceive(querys);
                        DataTableReader dtRead1 = tbles.CreateDataReader();
                        while (dtRead1.Read())
                        {
                            if (dtRead1[0] != System.DBNull.Value)
                            {
                                designIdlist.Add(Convert.ToInt32(dtRead1[0]));

                                inDesignID.Add(Convert.ToInt32(dtRead1[0]));

                                if (inDesignID.Count == 10)
                                {
                                    designIDSplit.Add(inDesignID);
                                    inDesignID = new List<int>();
                                }
                            }
                        }

                        designIDSplit.Add(inDesignID);

                        ////////////////////////////////////////////////////////////////////Tomorrow/////////////////////////////////////////////////////////////////

                        if (gett != "TSTPLinkTable")
                        {
                            writer.WriteStartElement(gett);

                            foreach (string designtable in designtablelist)
                            {
                                if (designtable == "TCInitialization")
                                {
                                    XMLWriterTCInitial(localPath, designIdlist, designtable, QscDatabase);

                                }
                                else
                                {
                                    string query1 = string.Empty;

                                    if (gett == "TPDesignLinkTable")
                                    {
                                        query1 = "select * from " + designtable + " where DesignID in ('" + string.Join("','", designIdlist) + "')";
                                    }
                                    else if (gett == "TPMonitorLinkTable")
                                    {
                                        if (designtable == "BackgroundMonitoring")
                                        {
                                            query1 = "select * from " + designtable + " where BackgroundMonitorID in ('" + string.Join("','", designIdlist) + "')";
                                        }
                                        else
                                        {
                                            query1 = "select * from " + designtable + " where BMID in ('" + string.Join("','", designIdlist) + "')";
                                        }
                                    }
                                    else if (gett == "TPTCLinkTable")
                                    {
                                        if (designtable == "Testcase")
                                        {
                                            query1 = "select * from " + designtable + " where TestcaseID in ('" + string.Join("','", designIdlist) + "')";
                                        }
                                        else
                                        {
                                            query1 = "select * from " + designtable + " where TCID in ('" + string.Join("','", designIdlist) + "')";
                                        }
                                    }

                                    DataTable tble1 = QscDatabase.SendCommand_Toreceive(query1);
                                    //DataTableReader Reader = tble1.CreateDataReader();

                                    writer.WriteStartElement(designtable);

                                    //File.AppendAllText(@localPath + "\\" + designtable + ".txt", "<" + designtable + ">");

                                    if (tble1.Rows.Count > 0)
                                    {
                                        for (int i = 0; i < tble1.Rows.Count; i++)
                                        {
                                            writer.WriteStartElement(designtable);

                                            for (int j = 0; j < tble1.Columns.Count; j++)
                                            {
                                                writer.WriteAttributeString(tble1.Columns[j].ColumnName, tble1.Rows[i][j].ToString());
                                            }

                                            writer.WriteEndElement();
                                        }
                                    }

                                    writer.WriteEndElement();
                                }
                            }

                            writer.WriteEndElement();
                        }

                        double per65 = (++Inc / percent65) * 65;
                        ProgressPercentage(Convert.ToInt32(per65 + 30).ToString());
                    }

                    writer.WriteEndElement();
                    writer.Close();


                    //var lastFolder = Path.GetDirectoryName(localPath);

                    ////Compress temp folder as Zip File
                    string savedfileName = string.Empty;

                    if (!savedfilepath[1].EndsWith(".zip"))
                    {
                        savedfileName = savedfilepath[1] + ".zip";
                    }
                    else
                    {
                        savedfileName = savedfilepath[1];
                    }

                    string zipPath = Path.Combine(savedfilepath[0], savedfileName);

                    if (File.Exists(zipPath))
                    {
                        //var randomName = new DirectoryInfo(lastFolder).Name;
                        string randomName = Convert.ToString(DateTime.Now.Ticks);
                        string[] savedfileNames = Regex.Split(savedfileName, ".zip");
                        zipPath = Path.Combine(savedfilepath[0], savedfileNames[0] + "_" + randomName + ".zip");
                    }

                    ZipFile.CreateFromDirectory(localPath, zipPath);
                }

                ProgressPercentage("98");
                /////Delete Temp directory

                var directorycreate = new DirectoryInfo(@"C:\Users\" + Environment.UserName + @"\AppData\Local\Temp\QATExport");
                System.IO.DirectoryInfo exportdir = new DirectoryInfo(directorycreate.FullName);

                try
                {
                    DeleteFolder(exportdir);
                    exportdir.Delete();
                    ProgressPercentage("99");
                }
                catch (Exception ex)
                {
                }


                ProgressPercentage("100");
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

        private void XMLWriterTCInitial(string localPath, List<int> designIDSplit, string designtable, DBConnection QscDatabase)
        {
            try
            {
                XmlWriterSettings settings = new XmlWriterSettings
                {
                    Indent = true,
                    IndentChars = "  ",
                    NewLineChars = "\r\n",
                    NewLineHandling = NewLineHandling.Replace,
                    ConformanceLevel = ConformanceLevel.Auto,
                    CheckCharacters = false
                };

                using (XmlWriter writer = XmlWriter.Create(@localPath + "\\ExportTCInitialization.xml", settings))
                {
                    writer.WriteStartElement("TCInitialization");

                    foreach (int desinVal in designIDSplit)
                    {
                        string query = "select * from " + designtable + " where DesignID in ('" + string.Join("','", desinVal) + "')";

                        DataTable tble1 = QscDatabase.SendCommand_Toreceive(query);
                        DataTableReader Reader = tble1.CreateDataReader();

                        for (int i = 0; i < tble1.Rows.Count; i++)
                        {
                            writer.WriteStartElement("TCInitialization");

                            for (int j = 0; j < tble1.Columns.Count; j++)
                            {
                                writer.WriteAttributeString(tble1.Columns[j].ColumnName, tble1.Rows[i][j].ToString());
                            }

                            writer.WriteEndElement();
                        }
                    }

                    writer.WriteEndElement();
                    writer.Close();
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
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
            }
        }

        private void Exportpreviewtextinput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            try
            {
                if (Regex.IsMatch(e.Text, @"[\\/:*?<>|""[\]&]"))
                {
                    e.Handled = true;
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

        private void Export_Closing(object sender, CancelEventArgs e)
        {
            try
            {
                if (Percentage != "100%" && StartExport != null && StartExport.ThreadState == 0)
                {
                    if (StartExport != null)
                        StartExport.Suspend();
                    MessageBoxResult result = ExportMessageBox("Are you sure cancel Exporting ? ", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                    if (result.ToString() == "Yes")
                    {
                        e.Cancel = false;
                        if (StartExport != null)
                        {
                            StartExport.Resume();
                            StartExport.Abort();
                        }                      

                    }
                    else
                    {
                        e.Cancel = true;
                        if (StartExport != null)
                            StartExport.Resume();
                    }
                }
                else
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        e.Cancel = false;
                    });
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

        private void ProgressPercentage(string progress)
        {
            this.Dispatcher.Invoke(() =>
            {
                Progressvalue = progress;
                Percentage = progress+"%";
            });
        }     

        private void ExportPreviewKeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                TextBox btn = (TextBox)sender;
                string str = btn.Text;
                string keyValue = e.Key.ToString();
                if (keyValue == "V" && Keyboard.Modifiers == ModifierKeys.Control)
                {
                    e.Handled = false;
                    string cliptext = Clipboard.GetText();
                    if (Regex.IsMatch(cliptext, @"[\\/:*?<>|""[\]&]"))
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

        private void ExportWindowLoaded(object sender, RoutedEventArgs e)
        {
            try
            {
                //    Mouse.OverrideCursor = Cursors.Wait;
                this.Title = this.Title + " ----->Connected Server :" + QatConstants.SelectedServer;
                TreeViewExplorer ExportRootItem = new TreeViewExplorer(0, QatConstants.TveExportInventoryTitle, QatConstants.TveDesignerHeaderItemType, null, null, null, null, null, null, null, null, null, null, 0, true);
            	ExportRootItem.IsExpanded = true;
            	ExportRootItem.IsCheckedVisibility = Visibility.Visible;
            	ExportRootItem.ItemTextBox.FontWeight = FontWeights.Bold;
            	treeViewExplorerExportList.Add(ExportRootItem);
            	TreeViewExport.DataContext = treeViewExplorerExportList;
            	ProgressUpdateEX.DataContext = this;
            	TextUpdateEX.DataContext = this;
            	SetupImportTreeViewrFromDB(true, true, "Date Created Ascending", ExportRootItem);
                Contextascending.IsChecked = true;
                Contextascendingname.IsChecked = false;
                Contextascendingcreatedon.IsChecked = true;

                Contextdescending.IsChecked = false;
                Contextdescendingname.IsChecked = false;
                Contextdescendingcreatedon.IsChecked = false;
                //   Mouse.OverrideCursor = Cursors.Arrow;
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
                    SetSorting("Ascending By Name", treeViewExplorerExportList);
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

        private void Ascending_Click_CreatedOn(object sender, RoutedEventArgs e)
        {
            try
            {
                if (isInventorySearchListSelected == true)
                {
                    SetSorting("Date Created Ascending", searchTreeviewlst);
                }
                else
                {
                    SetSorting("Date Created Ascending", treeViewExplorerExportList);
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
                    SetSorting("Descending By Name", treeViewExplorerExportList);
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

        private void Descending_Click_CreatedOn(object sender, RoutedEventArgs e)
        {
            try
            {
                if (isInventorySearchListSelected == true)
                {
                    SetSorting("Date Created Descending", searchTreeviewlst);
                }
                else
                {
                    SetSorting("Date Created Descending", treeViewExplorerExportList);
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

        private void SetSorting(string sortType, List<TreeViewExplorer> roottreeviewExplorerLst)
        {
            try
            {
                List<TreeViewExplorer> treeViewExplorerList = new List<TreeViewExplorer>();
                TreeViewExplorer InventoryRootItem = new TreeViewExplorer(0, QatConstants.TveExportInventoryTitle, QatConstants.TveDesignerHeaderItemType, null, null, null, null, null, null, null, null, null, null, 0, true);

                InventoryRootItem.IsExpanded = true;
                InventoryRootItem.IsCheckedVisibility = Visibility.Visible;
                InventoryRootItem.ItemTextBox.FontWeight = FontWeights.Bold;
                if (roottreeviewExplorerLst.Count > 0 && roottreeviewExplorerLst[0].Children.Count > 0)
                {
                    TreeViewExplorer[] alphaTestSuiteSorted = roottreeviewExplorerLst[0].Children.ToArray();

                    if (sortType == "Ascending By Name")
                    {
                        Array.Sort(alphaTestSuiteSorted, new AlphanumComparatorFast());
                        sortingOrders = sortType;

                        Contextascending.IsChecked = true;
                        Contextascendingname.IsChecked = true;
                        Contextascendingcreatedon.IsChecked = false;

                        Contextdescending.IsChecked = false;
                        Contextdescendingname.IsChecked = false;
                        Contextdescendingcreatedon.IsChecked = false;
                    }
                    else if (sortType == "Descending By Name")
                    {
                        Array.Sort(alphaTestSuiteSorted, new AlphanumComparatorFast());
                        Array.Reverse(alphaTestSuiteSorted);
                        sortingOrders = sortType;

                        Contextascending.IsChecked = false;
                        Contextascendingname.IsChecked = false;
                        Contextascendingcreatedon.IsChecked = false;

                        Contextdescending.IsChecked = true;
                        Contextdescendingname.IsChecked = true;
                        Contextdescendingcreatedon.IsChecked = false;
                    }
                    else if (sortType == "Date Created Ascending")
                    {
                        Array.Sort(alphaTestSuiteSorted, new AlphanumComparatorFast_DateTime());
                        sortingOrders = sortType;

                        Contextascending.IsChecked = true;
                        Contextascendingname.IsChecked = false;
                        Contextascendingcreatedon.IsChecked = true;

                        Contextdescending.IsChecked = false;
                        Contextdescendingname.IsChecked = false;
                        Contextdescendingcreatedon.IsChecked = false;
                    }
                    else if (sortType == "Date Created Descending")
                    {
                        Array.Sort(alphaTestSuiteSorted, new AlphanumComparatorFast_DateTime());
                        Array.Reverse(alphaTestSuiteSorted);
                        sortingOrders = sortType;

                        Contextascending.IsChecked = false;
                        Contextascendingname.IsChecked = false;
                        Contextascendingcreatedon.IsChecked = false;

                        Contextdescending.IsChecked = true;
                        Contextdescendingname.IsChecked = false;
                        Contextdescendingcreatedon.IsChecked = true;
                    }

                    treeViewExplorerList = alphaTestSuiteSorted.ToList();
                }

                InventoryRootItem.ClearChildren();
                InventoryRootItem.AddChildrenList_withCheckbox(treeViewExplorerList);

                roottreeviewExplorerLst.Clear();
                roottreeviewExplorerLst.Add(InventoryRootItem);
                TreeViewExport.DataContext = null;
                TreeViewExport.DataContext = roottreeviewExplorerLst;
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

        private void TreeViewExport_ContextMenuOpening(object sender, System.Windows.Controls.ContextMenuEventArgs e)
        {
            try
            {
                bool isTreeviewEnabled = false;
                if (TreeViewExport.Items != null && TreeViewExport.Items.Count > 0)
                {
                    var treeviewHeader = TreeViewExport.Items[0] as TreeViewExplorer;
                    isTreeviewEnabled = treeviewHeader.IsEnabled;
                }

                if ((treeViewExplorerExportList == null || treeViewExplorerExportList.Count == 0) || (treeViewExplorerExportList != null && treeViewExplorerExportList.Count != 0 && treeViewExplorerExportList[0].Children.Count == 0) || (isTreeviewEnabled == false))
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

        private void txt_Search_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (treeViewExplorerExportList != null && treeViewExplorerExportList.Count > 0)
                {
                    if (txt_Search.Text != string.Empty)
                    {
                        if (sortingOrders_OriginalList != sortingOrders)
                        {
                            SetSorting(sortingOrders, treeViewExplorerExportList);
                            sortingOrders_OriginalList = sortingOrders;
                        }

                        searchTreeviewlst.Clear();
                        Cancelbutton.Visibility = Visibility.Visible;
                        isInventorySearchListSelected = true;
                        TreeViewExplorer treeViewExplorerSearchList = new TreeViewExplorer(0, QatConstants.TveExportInventoryTitle, QatConstants.TveDesignerHeaderItemType, null, null, null, null, null, null, null, null, null, null, 0, true);
                        treeViewExplorerSearchList.IsExpanded = true;
                        treeViewExplorerSearchList.IsCheckedVisibility = Visibility.Visible;
                        treeViewExplorerSearchList.ItemTextBox.FontWeight = FontWeights.Bold;
                        searchTreeviewlst.Add(treeViewExplorerSearchList);

                        foreach (TreeViewExplorer inventHeader in treeViewExplorerExportList)
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

                        TreeViewExport.DataContext = null;
                        TreeViewExport.DataContext = searchTreeviewlst;
                    }
                    else
                    {
                        Cancelbutton.Visibility = Visibility.Hidden;
                        isInventorySearchListSelected = false;

                        if (sortingOrders_OriginalList != sortingOrders)
                        {
                            SetSorting(sortingOrders, treeViewExplorerExportList);
                            sortingOrders_OriginalList = sortingOrders;
                        }
                        else
                        {
                            TreeViewExplorer treeViewExplorerSearchList = new TreeViewExplorer(0, QatConstants.TveExportInventoryTitle, QatConstants.TveDesignerHeaderItemType, null, null, null, null, null, null, null, null, null, null, 0, true);
                            treeViewExplorerSearchList.IsExpanded = true;
                            treeViewExplorerSearchList.IsCheckedVisibility = Visibility.Visible;
                            treeViewExplorerSearchList.ItemTextBox.FontWeight = FontWeights.Bold;
                            searchTreeviewlst.Add(treeViewExplorerSearchList);

                            foreach (TreeViewExplorer inventHeader in treeViewExplorerExportList)
                            {
                                foreach (TreeViewExplorer testPlans in inventHeader.Children)
                                {
                                    treeViewExplorerSearchList.AddChildren_withCheckbox(testPlans);
                                }
                            }

                            treeViewExplorerExportList.Clear();
                            treeViewExplorerExportList.Add(treeViewExplorerSearchList);

                            TreeViewExport.DataContext = null;
                            TreeViewExport.DataContext = treeViewExplorerExportList;
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

        private void TextBox_PreviewKeyUp(object sender, KeyEventArgs e)
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

                if (treeViewExplorerExportList.Count > 0)
                {
                    if (sortingOrders_OriginalList != sortingOrders)
                    {
                        SetSorting(sortingOrders, treeViewExplorerExportList);
                        sortingOrders_OriginalList = sortingOrders;
                    }
                    else
                    {
                        TreeViewExplorer treeViewExplorerSearchList = new TreeViewExplorer(0, QatConstants.TveExportInventoryTitle, QatConstants.TveDesignerHeaderItemType, null, null, null, null, null, null, null, null, null, null, 0, true);
                        treeViewExplorerSearchList.IsExpanded = true;
                        treeViewExplorerSearchList.IsCheckedVisibility = Visibility.Visible;
                        treeViewExplorerSearchList.ItemTextBox.FontWeight = FontWeights.Bold;
                        searchTreeviewlst.Add(treeViewExplorerSearchList);

                        foreach (TreeViewExplorer inventHeader in treeViewExplorerExportList)
                        {
                            foreach (TreeViewExplorer testPlans in inventHeader.Children)
                            {
                                treeViewExplorerSearchList.AddChildren_withCheckbox(testPlans);
                            }
                        }

                        treeViewExplorerExportList.Clear();
                        treeViewExplorerExportList.Add(treeViewExplorerSearchList);

                        TreeViewExport.DataContext = null;
                        TreeViewExport.DataContext = treeViewExplorerExportList;
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
