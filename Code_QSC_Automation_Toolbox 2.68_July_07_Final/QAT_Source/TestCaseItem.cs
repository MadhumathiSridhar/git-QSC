using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Data;

namespace QSC_Test_Automation
{
    public class TestCaseItem : INotifyPropertyChanged
    {
        DBConnection QscDataBase = new DBConnection();
        public event PropertyChangedEventHandler PropertyChanged;

        bool isSkipSaveButtonEnable = false;
        void OnPropertyChanged(string property)
        {
            try
            {
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs(property));

                    if (SaveButtonIsEnabled != true && property != "SaveButtonIsEnabled" && property != "TestItemHeaderName" && isSkipSaveButtonEnable == false && property != "TestCaseSettingsVisible" && property != "Category" && property != "Modifiedby" && property != "Modifiedon" && property != "Summary")
                        SaveButtonIsEnabled = true;

                    isSkipSaveButtonEnable = false;
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

        public TestCaseItem()
        {
            List<TreeViewExplorer> sortedTestPlanList = null;
            try
            {
                IsEditModeEnabled = true;
                IsEditModeEnabled = true;
                ItemNameTextBoxIsEnabled = true;
                SelectTestPlanIsEnabled = true;
                ActionTabGridIsEnabled = true;
                TestCaseSettingsVisible = Visibility.Hidden;



                //           if(orders== "No order" || orders == string.Empty)
                //           {
                //               TestPlanList = new ObservableCollection<TreeViewExplorer>(QscDataBase.ReadTreeTable(QatConstants.DbTestPlanTable, 0, null, null));
                //           }
                //           else if (orders == "Ascending")
                //           {
                QRCMInitialValuesUpdate();
                TestPlanList = new ObservableCollection<TreeViewExplorer>(QscDataBase.ReadTreeTable(QatConstants.DbTestPlanTable, 0, null, null));
                TreeViewExplorer[] alphaTestPlanSorted = TestPlanList.ToArray();
                Array.Sort(alphaTestPlanSorted, new AlphanumComparatorFast());
                sortedTestPlanList = alphaTestPlanSorted.ToList();
                TestPlanList = new ObservableCollection<TreeViewExplorer>(sortedTestPlanList.ToList());
                //           }
                //           else if (orders == "Descending")
                //           {
                //               TestPlanList = new ObservableCollection<TreeViewExplorer>(QscDataBase.ReadTreeTable(QatConstants.DbTestPlanTable, 0, null, null));
                //               TreeViewExplorer[] alphaTestPlanSorted = TestPlanList.ToArray();
                //               Array.Sort(alphaTestPlanSorted, new AlphanumComparatorFast());
                //               Array.Reverse(alphaTestPlanSorted);
                //               sortedTestPlanList = alphaTestPlanSorted.ToList();
                //               TestPlanList = new ObservableCollection<TreeViewExplorer>(sortedTestPlanList.ToList());
                //           }
                //           else if (orders == "Date Created Ascending")
                //           {
                //ascending = false;
                //               TestPlanList = new ObservableCollection<TreeViewExplorer>(QscDataBase.ReadTreebydatecreated(QatConstants.DbTestPlanTable, 0, null, null, ascending));
                //           }
                //           else if(orders== "Date Created Descending")
                //           {
                //               ascending = true;
                //               TestPlanList = new ObservableCollection<TreeViewExplorer>(QscDataBase.ReadTreebydatecreated(QatConstants.DbTestPlanTable, 0, null, null, ascending));
                //           }
                AddTestAction();
                SelectedActionItem = TestActionItemList[0];
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

        public TestCaseItem(TreeViewExplorer sourceTestCase, bool isEditModeEnabled)
        {
            try
            {
                List<TreeViewExplorer> sortedTestPlanList = null;
                if (isEditModeEnabled)
                {
                    IsEditModeEnabled = true;
                    ItemNameTextBoxIsEnabled = true;
                    SelectTestPlanIsEnabled = true;
                    ActionTabGridIsEnabled = true;
                    TestCaseSettingsVisible = Visibility.Visible;
                }
                else
                {
                    IsEditModeEnabled = false;
                    ItemNameTextBoxIsEnabled = false;
                    SaveCloseVisibility = Visibility.Hidden;
                    TestCaseSettingsVisible = Visibility.Visible;
                    SelectTestPlanIsEnabled = false;
                    ActionTabGridIsEnabled = false;
                    SaveButtonIsEnabled = false;
                    TestActionTabPlusButtonVisibility = Visibility.Hidden;
                }

                IsNewTestCase = false;
                TestCaseTreeViewExplorer = sourceTestCase;
                TestItemName = sourceTestCase.ItemName;
                TestItemNameCopy = sourceTestCase.ItemName;
                TestCaseID = sourceTestCase.ItemKey;
                Createdby = sourceTestCase.Createdby;
                Createdon = sourceTestCase.Createdon;
                Modifiedby = sourceTestCase.Modifiedby;
                Modifiedon = sourceTestCase.Modifiedon;
                Summary = sourceTestCase.Summary;

                if(sourceTestCase.Category != null)
                    Category = sourceTestCase.Category;
                else
                    Category = sourceTestCase.Category = string.Empty;

                //string query = string.Empty;

                //if (sourceTestCase.ItemType == QatConstants.DbTestCaseTable)
                //    query = "select Category from " + QatConstants.DbTestCaseTable + " where " + QatConstants.DbTestCaseIDColumn + " = " + sourceTestCase.ItemKey;

                //string categoryval = string.Empty;
                //if (query != string.Empty)
                //{
                //    DataTable dataTable = QscDataBase.SendCommand_Toreceive(query);
                //    DataTableReader read1 = dataTable.CreateDataReader();

                //    while (read1.Read())
                //    {
                //        categoryval = read1.GetValue(0).ToString();
                //    }

                //    Category = sourceTestCase.Category = categoryval;
                //}


                //           if (orders == "No order" || orders == string.Empty)
                //           {
                //               TestPlanList = new ObservableCollection<TreeViewExplorer>(QscDataBase.ReadTreeTable(QatConstants.DbTestPlanTable, 0, null, null));
                //           }
                //           else if (orders == "Ascending")
                //           {
                TestPlanList = new ObservableCollection<TreeViewExplorer>(QscDataBase.ReadTreeTable(QatConstants.DbTestPlanTable, 0, null, null));
                TreeViewExplorer[] alphaTestPlanSorted = TestPlanList.ToArray();
                Array.Sort(alphaTestPlanSorted, new AlphanumComparatorFast());
                sortedTestPlanList = alphaTestPlanSorted.ToList();
                TestPlanList = new ObservableCollection<TreeViewExplorer>(sortedTestPlanList.ToList());
                //           }
                //           else if (orders == "Descending")
                //           {
                //               TestPlanList = new ObservableCollection<TreeViewExplorer>(QscDataBase.ReadTreeTable(QatConstants.DbTestPlanTable, 0, null, null));
                //               TreeViewExplorer[] alphaTestPlanSorted = TestPlanList.ToArray();
                //               Array.Sort(alphaTestPlanSorted, new AlphanumComparatorFast());
                //               Array.Reverse(alphaTestPlanSorted);
                //               sortedTestPlanList = alphaTestPlanSorted.ToList();
                //               TestPlanList = new ObservableCollection<TreeViewExplorer>(sortedTestPlanList.ToList());
                //           }
                //           else if (orders == "Date Created Ascending")
                //           {
                //ascending = false;
                //               TestPlanList = new ObservableCollection<TreeViewExplorer>(QscDataBase.ReadTreebydatecreated(QatConstants.DbTestPlanTable, 0, null, null, ascending));
                //           }
                //           else if (orders == "Date Created Descending")
                //           {
                //               ascending = true;
                //               TestPlanList = new ObservableCollection<TreeViewExplorer>(QscDataBase.ReadTreebydatecreated(QatConstants.DbTestPlanTable, 0, null, null, ascending));
                //           }

                List<TreeViewExplorer> testPlanSelectionList = new List<TreeViewExplorer>(TestPlanList);
                int planKey = QscDataBase.ReadTestPlanKeyForTestCase(this);
                TestPlanSelected = testPlanSelectionList.Find(item => item.ItemKey == planKey);
                QRCMInitialValuesUpdate();
                if (IsEditModeEnabled && TestPlanSelected == null)
                {
                    ItemNameTextBoxIsEnabled = false;
                    ActionTabGridIsEnabled = false;
                    TestActionTabPlusButtonVisibility = Visibility.Hidden;
                }
                else
                {
                    QscDataBase.ReadTestCaseFromDB(this);
                    SelectedActionItem = TestActionItemList[0];
                    isSkipSaveButtonEnable = false;
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

        public TestCaseItem(TestPlanItem sourceTestPlan)
        {
            try
            {
                IsEditModeEnabled = true;
                ItemNameTextBoxIsEnabled = true;
                SelectTestPlanIsEnabled = false;
                ActionTabGridIsEnabled = true;
                ParentTestPlanItem = sourceTestPlan;
                IsTestCaseCreatedFromTestPlan = true;
                TestCaseSettingsVisible = Visibility.Hidden;

                TestPlanList = new ObservableCollection<TreeViewExplorer>(QscDataBase.ReadTreeTable(QatConstants.DbTestPlanTable, 0, null, null));
                AddTestAction();

                List<TreeViewExplorer> testPlanSelectionList = new List<TreeViewExplorer>(TestPlanList);
                TestPlanSelected = testPlanSelectionList.Find(item => item.ItemKey == sourceTestPlan.TestPlanID);
                QRCMInitialValuesUpdate();

                SelectedActionItem = TestActionItemList[0];
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

        public void QRCMInitialValuesUpdate ()
        {
            try
            {
                DBConnection dbConnection = new DBConnection();
                dbConnection.CreateConnection();
                dbConnection.OpenConnection();

                string  query = "select * from QRCMInitialization where Project_Name='" + "QRCM" + "'";
                DataTable dataTable = new DataTable();
                System.Data.SqlClient.SqlDataAdapter dataAdapter = new System.Data.SqlClient.SqlDataAdapter(query, dbConnection.CreateConnection());
                dataAdapter.Fill(dataTable);
                DataTableReader dataTableReader = dataTable.CreateDataReader();

                if (dataTableReader.HasRows)
                {
                    while (dataTableReader.Read())
                    {
                        string buildversion = string.Empty;
                        if (dataTableReader[1] != System.DBNull.Value)
                            buildversion = dataTableReader.GetString(1).ToString().Trim();

                        if (!QRCMDictionary.Keys.Contains(buildversion))
                            QRCMDictionary.Add(buildversion, new ObservableCollection<QRCMInitialValues>());

                        QRCMInitialValues qrcmValues = new QRCMInitialValues();

                        if (dataTableReader[0] != System.DBNull.Value)
                            qrcmValues.ProjectName = dataTableReader.GetString(0).ToString().Trim();

                        if (dataTableReader[1] != System.DBNull.Value)
                            qrcmValues.Buildversion = dataTableReader.GetString(1).ToString().Trim();

                        if (dataTableReader[2] != System.DBNull.Value)
                            qrcmValues.ReferenceVersion = dataTableReader.GetString(2).ToString().Trim();

                        if (dataTableReader[3] != System.DBNull.Value)
                            qrcmValues.MethodNameUserView = dataTableReader.GetString(3).ToString().Trim();

                        if (dataTableReader[4] != System.DBNull.Value)
                            qrcmValues.Actual_method_name = dataTableReader.GetString(4).ToString().Trim();

                        if (dataTableReader[5] != System.DBNull.Value)
                            qrcmValues.Input_arguments_Tooltip = dataTableReader.GetString(5).ToString().Trim();

                        if (dataTableReader[6] != System.DBNull.Value)
                            qrcmValues.ApiReference = dataTableReader.GetString(6).ToString().Trim();

                        if (dataTableReader[7] != System.DBNull.Value && dataTableReader.GetString(7).ToString().Trim() != string.Empty)
                            qrcmValues.HasPreMethod = Convert.ToBoolean(dataTableReader.GetString(7).ToString().Trim());

                        if (dataTableReader[8] != System.DBNull.Value)
                            qrcmValues.PreMethodName = dataTableReader.GetString(8).ToString().Trim();

                        if (dataTableReader[9] != System.DBNull.Value)
                            qrcmValues.PreMethodUserKey = dataTableReader.GetString(9).ToString().Trim();

                        if (dataTableReader[10] != System.DBNull.Value)
                            qrcmValues.PreMethodActualKey = dataTableReader.GetString(10).ToString().Trim();

                        if (dataTableReader[11] != System.DBNull.Value && dataTableReader.GetString(11).Trim() != string.Empty)
                            qrcmValues.ArgumentMappingIndex = Convert.ToInt32(dataTableReader.GetString(11).Trim());

                        if (dataTableReader[12] != System.DBNull.Value)
                            qrcmValues.CoreModel = dataTableReader.GetString(12).ToString().Trim();

                        if (dataTableReader[13] != System.DBNull.Value && dataTableReader.GetString(13).ToString().Trim() != string.Empty)
                            qrcmValues.IsActionTrue = Convert.ToBoolean(dataTableReader.GetString(13).ToString().Trim());

                        if (dataTableReader[14] != System.DBNull.Value)
                            qrcmValues.TabGroupName = dataTableReader.GetString(14).ToString().Trim();

                        if (dataTableReader[15] != System.DBNull.Value)
                            qrcmValues.Api_Payload = dataTableReader.GetString(15).ToString().Trim();

                        if (dataTableReader[16] != System.DBNull.Value && dataTableReader.GetString(16).Trim() != string.Empty)
                            qrcmValues.IsPayloadAvailable = Convert.ToBoolean(dataTableReader.GetString(16).Trim());

                        if (dataTableReader[17] != System.DBNull.Value)
                            qrcmValues.Reference_key = dataTableReader.GetString(17).ToString().Trim();

                        if (dataTableReader[18] != System.DBNull.Value)
                            qrcmValues.Payload_key = dataTableReader.GetString(18).ToString().Trim();

                        if (dataTableReader[19] != System.DBNull.Value)
                            qrcmValues.Merge_data = Convert.ToBoolean(dataTableReader.GetString(19).ToString().Trim());

                        //////////Adding all values for each method readed from QRCMInitialization table into QRCMDictionary.
                        //////////QRCMDictionary has all versions data
                        if (!QRCMDictionary[buildversion].Contains(qrcmValues))
                            QRCMDictionary[buildversion].Add(qrcmValues);

                    }
                }

                dbConnection.CloseConnection();
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif            
            }
        }
        



        public TestActionItem AddTestAction()
        {
            try
            {
                TestActionItem testActionItem = new TestActionItem(this);

                string itemName = string.Format("Tab {0}", TestActionItemList.Count+1);
                List<string> actionItemNameList = new List<string>();

                foreach (TestActionItem item in TestActionItemList)
                    actionItemNameList.Add(item.TestActionItemName);

                if (actionItemNameList.Contains(itemName))
                {
                    for (int i = TestActionItemList.Count+2; ; i++)
                    {
                        itemName = string.Format("Tab {0}", i);
                        if (!actionItemNameList.Contains(itemName))
                            break;
                    }
                }

                testActionItem.TestActionItemName = itemName;
                TestActionItemList.Insert(TestActionItemList.Count, testActionItem);

                if (IsEditModeEnabled)
                {
                    if (TestActionItemList.Count == 1)
                    {
                        TestActionItemList[0].TestActionTabDeleteButtonVisibility = Visibility.Hidden;
                    }
                    else if (TestActionItemList.Count == 2)
                    {
                        TestActionItemList[0].TestActionTabDeleteButtonVisibility = Visibility.Visible;
                    }
                }
                else
                {
                    testActionItem.TestActionTabDeleteButtonVisibility = Visibility.Hidden;
                }

                SelectedActionItem = testActionItem;
                return testActionItem;
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                //MessageBox.Show("Exception\n " + ex.Message, "Error Code - ECxxxxx", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }

        public void RemoveTestActionItem(TestActionItem testActionItem)
        {
            try
            {
                if (TestActionItemList.Contains(testActionItem))
                {
                    TestActionItemList.Remove(testActionItem);
                    SaveButtonIsEnabled = true;
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

        public void InsertTestActionItem(int index, TestActionItem testActionItem)
        {
            try
            {
                    TestActionItemList.Insert(index, testActionItem);
                    SaveButtonIsEnabled = true;
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

        public Visibility TestCaseGridVisibility
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
                isSkipSaveButtonEnable = true;
                OnPropertyChanged("IsSelected");
            }
        }

        public string TestItemHeaderName
        {
            get
            {
                if (SaveButtonIsEnabled)
                {
                    if (string.IsNullOrEmpty(testItemNameValue))
                        return "*TC: New Test Case";
                    else
                        return "*TC: " + testItemNameValue;
                }
                else
                {
                    if (string.IsNullOrEmpty(testItemNameValue))
                        return "TC: New Test Case";
                    else
                        return "TC: " + testItemNameValue;
                }
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

        private string testItemNameCopyValue = null;
        public string TestItemNameCopy
        {
            get { return testItemNameCopyValue; }
            set
            {
                testItemNameCopyValue = value;
                OnPropertyChanged("TestCaseNameCopy");
            }
        }

        private int testCaseIDValue = 0;
        public int TestCaseID
        {
            get { return testCaseIDValue; }
            set
            {
                testCaseIDValue = value;
                OnPropertyChanged("TestCaseID");
            }
        }

        private TreeViewExplorer testCaseTreeViewExplorerValue = null;
        public TreeViewExplorer TestCaseTreeViewExplorer
        {
            get { return testCaseTreeViewExplorerValue; }
            set
            {
                testCaseTreeViewExplorerValue = value;
                OnPropertyChanged("TestCaseTreeViewExplorer");
            }
        }

        private bool isNewTestCaseValue = true;
        public bool IsNewTestCase
        {
            get { return isNewTestCaseValue; }
            set
            {
                isNewTestCaseValue = value;
                OnPropertyChanged("IsNewTestCase");
            }
        }

        private bool isEditModeEnabledValue = false;
        public bool IsEditModeEnabled
        {
            get { return isEditModeEnabledValue; }
            set
            {
                isEditModeEnabledValue = value;
                OnPropertyChanged("IsEditModeEnabled");
            }
        }

        private bool selectTestPlanIsEnabledValue = true;
        public bool SelectTestPlanIsEnabled
        {
            get { return selectTestPlanIsEnabledValue; }
            set { selectTestPlanIsEnabledValue = value; OnPropertyChanged("SelectTestPlanIsEnabled"); }
        }

        private Visibility testActionTabPlusButtonVisibilityValue = Visibility.Visible;
        public Visibility TestActionTabPlusButtonVisibility
        {
            get { return testActionTabPlusButtonVisibilityValue; }
            set
            {
                testActionTabPlusButtonVisibilityValue = value;
                OnPropertyChanged("TestActionTabPlusButtonVisibility");
            }
        }

        private bool isTestCaseCreatedFromTestPlanValue = false;
        public bool IsTestCaseCreatedFromTestPlan
        {
            get { return isTestCaseCreatedFromTestPlanValue; }
            set { isTestCaseCreatedFromTestPlanValue = value; OnPropertyChanged("IsTestCaseCreatedFromTestPlan"); }
        }

        private TestPlanItem parentTestPlanItemValue = null;
        public TestPlanItem ParentTestPlanItem
        {
            get { return parentTestPlanItemValue; }
            set { parentTestPlanItemValue = value; OnPropertyChanged("ParentTestPlanItem"); }
        }

        private bool actionTabGridIsEnabledValue = false;
        public bool ActionTabGridIsEnabled
        {
            get { return actionTabGridIsEnabledValue; }
            set
            {
                actionTabGridIsEnabledValue = value;

                OnPropertyChanged("ActionTabGridIsEnabled");
            }
        }

        private bool saveButtonIsEnabledValue = false;
        public bool SaveButtonIsEnabled
        {
            get { return saveButtonIsEnabledValue; }
            set { saveButtonIsEnabledValue = value; OnPropertyChanged("SaveButtonIsEnabled"); OnPropertyChanged("TestItemHeaderName"); }
        }

        private bool cancelButtonIsEnabledValue = true;
        public bool CancelButtonIsEnabled
        {
            get { return cancelButtonIsEnabledValue; }
            set { cancelButtonIsEnabledValue = value; OnPropertyChanged("CancelButtonIsEnabled"); OnPropertyChanged("TestItemHeaderName"); }
        }

        private bool saveButtonIsDefaultValue = true;
        public bool SaveButtonIsDefault
        {
            get { return saveButtonIsDefaultValue; }
            set { saveButtonIsDefaultValue = value; OnPropertyChanged("SaveButtonIsDefault"); }
        }

        private Visibility saveCloseVisibilityValue = Visibility.Visible;
        public Visibility SaveCloseVisibility
        {
            get { return saveCloseVisibilityValue; }
            set { saveCloseVisibilityValue = value; OnPropertyChanged("SaveCloseVisibility"); }
        }

        private bool itemNameTextBoxIsEnabledValue = false;
        public bool ItemNameTextBoxIsEnabled
        {
            get { return itemNameTextBoxIsEnabledValue; }
            set { itemNameTextBoxIsEnabledValue = value; OnPropertyChanged("ItemNameTextBoxIsEnabled"); }
        }

        private ObservableCollection<TreeViewExplorer> testPlanListValue = new ObservableCollection<TreeViewExplorer>();
        public ObservableCollection<TreeViewExplorer> TestPlanList
        {
            get { return testPlanListValue; }
            set { testPlanListValue = value; OnPropertyChanged("TestPlanList"); }
        }

        private TreeViewExplorer testPlanSelectedValue = null;
        public TreeViewExplorer TestPlanSelected
        {
            get { return testPlanSelectedValue; }
            set
            {

                if(testPlanSelectedValue == null && IsNewTestCase == false && ActionTabGridIsEnabled == false && IsEditModeEnabled && value != null)
                {
                    testPlanSelectedValue = value;
                    QscDataBase.GetDesignComponentDetails(this);
                    QscDataBase.ReadTestCaseFromDB(this);
                    ItemNameTextBoxIsEnabled = true;
                    ActionTabGridIsEnabled = true;
                    TestActionTabPlusButtonVisibility = Visibility.Visible;
                }
                else if (value != null)
                {
                    testPlanSelectedValue = value;
                    QscDataBase.GetDesignComponentDetails(this);
                }
                OnPropertyChanged("TestPlanSelected");
            }
        }

        private string designNameSelectedValue = null;
        public string DesignNameSelected
        {
            get { return designNameSelectedValue; }
            set { designNameSelectedValue = value; OnPropertyChanged("DesignNameSelected"); }
        }

        private int designIDSelectedValue = 0;
        public int DesignIDSelected
        {
            get { return designIDSelectedValue; }
            set { designIDSelectedValue = value; OnPropertyChanged("DesignIDSelected"); }
        }

        private ObservableCollection<DUT_DeviceItem> selectedDeviceItemListValue = new ObservableCollection<DUT_DeviceItem>();
        public ObservableCollection<DUT_DeviceItem> SelectedDeviceItemList
        {
            get { return selectedDeviceItemListValue; }
            set {
                selectedDeviceItemListValue = value;
                DUT_DeviceItem[] alphaNumericSortedComponentType = selectedDeviceItemListValue.ToArray();
                Array.Sort(alphaNumericSortedComponentType, new AlphanumComparatorFastDut());
                selectedDeviceItemListValue = new ObservableCollection<DUT_DeviceItem>(alphaNumericSortedComponentType.ToList());
                OnPropertyChanged("SelectedDeviceItemList");
            }
        }

        private TestActionItem selectedActionItemValue = null;
        public TestActionItem SelectedActionItem
        {
            get { return selectedActionItemValue; }
            set
            {
                selectedActionItemValue = value;
                isSkipSaveButtonEnable = true;
                OnPropertyChanged("SelectedActionItem");
            }
        }

        private ObservableCollection<TestActionItem> testActionItemListValue = new ObservableCollection<TestActionItem>();
        public ObservableCollection<TestActionItem> TestActionItemList
        {
            get { return testActionItemListValue; }
            private set { testActionItemListValue = value; OnPropertyChanged("TestActionItemList"); }
        }

        private ObservableCollection<string> testActionListValue = new ObservableCollection<string> { "Control Action", "Ssh/Telnet Action", "Firmware Action", "Designer Action", "Net Pairing Action", "USB Action", "CEC Action","User Action", "QRCM Action", "Skip Action" };
        public ObservableCollection<String> TestActionList
        {
            get { return testActionListValue; }
            private set { testActionListValue = value; OnPropertyChanged("TestActionList"); }
        }

        private ObservableCollection<string> testVerificationListValue = new ObservableCollection<string> { "Control Verification", "Ssh/Telnet Verification", "Log Verification", "Audio Precision Verification", "Responsalyzer", "USB Verification", "CEC Verification", "QR code Verification","Script Verification","User Verification", "QRCM Verification", "Skip Verification" };
        public ObservableCollection<String> TestVerificationList
        {
            get { return testVerificationListValue; }
            private set { testVerificationListValue = value; OnPropertyChanged("TestVerificationList"); }
        }

        private List<string> componentTypeListValue = new List<string>();
        public List<string> ComponentTypeList
        {
            get { return componentTypeListValue; }
            set { componentTypeListValue = value; }
        }

        private List<Tuple<string, string>> UsbAudioDeviceListValue = new List<Tuple<string, string>>();
        public List<Tuple<string, string>> UsbAudioDeviceList
        {
            get { return UsbAudioDeviceListValue; }
            set { UsbAudioDeviceListValue = value; }
        }

        private List<Tuple<string, string>> UsbAudioBridgeListValue = new List<Tuple<string, string>>();
        public List<Tuple<string, string>> UsbAudioBridgeList
        {
            get { return UsbAudioBridgeListValue; }
            set { UsbAudioBridgeListValue = value; }
        }

        private Dictionary<string, ObservableCollection<QRCMInitialValues>> qrcmDictionaryValue = new Dictionary<string, ObservableCollection<QRCMInitialValues>>();
        public Dictionary<string, ObservableCollection<QRCMInitialValues>> QRCMDictionary
        {
            get { return qrcmDictionaryValue; }
            set { qrcmDictionaryValue = value; }
        }


        private Dictionary<string, ObservableCollection<string>> componentNameDictionaryValue = new Dictionary<string, ObservableCollection<string>>();
        public Dictionary<string, ObservableCollection<string>> ComponentNameDictionary
        {
            get { return componentNameDictionaryValue; }
            set { componentNameDictionaryValue = value; }
        }

        private Dictionary<string, ObservableCollection<string>> controlNameDictionaryValue = new Dictionary<string, ObservableCollection<string>>();
        public Dictionary<string, ObservableCollection<string>> ControlNameDictionary
        {
            get { return controlNameDictionaryValue; }
            set { controlNameDictionaryValue = value; }
        }

        private Dictionary<string, string[]> controlIDDictionaryValue = new Dictionary<string, string[]>();
        public Dictionary<string, string[]> ControlIDDictionary
        {
            get { return controlIDDictionaryValue; }
            set { controlIDDictionaryValue = value; }
        }

        private Dictionary<string, string> controlTypeDictionaryValue = new Dictionary<string, string>();
        public Dictionary<string, string> ControlTypeDictionary
        {
            get { return controlTypeDictionaryValue; }
            set { controlTypeDictionaryValue = value; }
        }

        private Dictionary<string, string> channelControlTypeDictionaryValue = new Dictionary<string, string>();
        public Dictionary<string, string> channelControlTypeDictionary
        {
            get { return channelControlTypeDictionaryValue; }
            set { channelControlTypeDictionaryValue = value; }
        }

        private Dictionary<string, string> VerifychannelControlTypeDictionaryValue = new Dictionary<string, string>();
        public Dictionary<string, string> VerifychannelControlTypeDictionary
        {
            get { return VerifychannelControlTypeDictionaryValue; }
            set { VerifychannelControlTypeDictionaryValue = value; }
        }

        private Dictionary<string, string> dataTypeDictionaryValue = new Dictionary<string, string>();
        public Dictionary<string, string> dataTypeDictionary
        {
            get { return dataTypeDictionaryValue; }
            set { dataTypeDictionaryValue = value; }
        }

        private Dictionary<string, string> controlInitialValueDictionaryValue = new Dictionary<string, string>();
        public Dictionary<string, string> ControlInitialValueDictionary
        {
            get { return controlInitialValueDictionaryValue; }
            set { controlInitialValueDictionaryValue = value; }
        }

        private Dictionary<string, string> controlInitialStringDictionaryValue = new Dictionary<string, string>();
        public Dictionary<string, string> ControlInitialStringDictionary
        {
            get { return controlInitialStringDictionaryValue; }
            set { controlInitialStringDictionaryValue = value; }
        }

        private Dictionary<string, string> controlInitialPositionDictionaryValue = new Dictionary<string, string>();
        public Dictionary<string, string> ControlInitialPositionDictionary
        {
            get { return controlInitialPositionDictionaryValue; }
            set { controlInitialPositionDictionaryValue = value; }
        }

        private Dictionary<string, string> MinimumControlValueDictionaryValue = new Dictionary<string, string>();
        public Dictionary<string, string> MinimumControlValueDictionary
        {
            get { return MinimumControlValueDictionaryValue; }
            set { MinimumControlValueDictionaryValue = value; }
        }

        private Dictionary<string, string> MaximumControlValueDictionaryValue = new Dictionary<string, string>();
        public Dictionary<string, string> MaximumControlValueDictionary
        {
            get { return MaximumControlValueDictionaryValue; }
            set { MaximumControlValueDictionaryValue = value; }
        }

        private Dictionary<string, ObservableCollection<string>> VerifycontrolNameDictionaryValue = new Dictionary<string, ObservableCollection<string>>();
        public Dictionary<string, ObservableCollection<string>> VerifyControlNameDictionary
        {
            get { return VerifycontrolNameDictionaryValue; }
            set { VerifycontrolNameDictionaryValue = value; }
        }

        private Dictionary<string, string> VerifycontrolTypeDictionaryValue = new Dictionary<string, string>();
        public Dictionary<string, string> VerifyControlTypeDictionary
        {
            get { return VerifycontrolTypeDictionaryValue; }
            set { VerifycontrolTypeDictionaryValue = value; }
        }

        private Dictionary<string, string> VerifyDataTypeDictionaryValue = new Dictionary<string, string>();
        public Dictionary<string, string> VerifyDataTypeDictionary
        {
            get { return VerifyDataTypeDictionaryValue; }
            set { VerifyDataTypeDictionaryValue = value; }
        }
        
        private Dictionary<string, ObservableCollection<string>> ChannelSelectionDictionaryValue = new Dictionary<string, ObservableCollection<string>>();
        public Dictionary<string, ObservableCollection<string>> ChannelSelectionDictionary
        {
            get { return ChannelSelectionDictionaryValue; }
            set { ChannelSelectionDictionaryValue = value; }
        }

        private string CreatedbyValue = null;
        public string Createdby
        {
            get { return CreatedbyValue; }
            set { CreatedbyValue = value; OnPropertyChanged("Createdby"); }
        }

        private DateTime? CreatedonValue = null;
        public DateTime? Createdon
        {
            get { return CreatedonValue; }
            set { CreatedonValue = value; OnPropertyChanged("Createdon"); }
        }

        private string ModifiedbyValue = null;
        public string Modifiedby
        {
            get { return ModifiedbyValue; }
            set { ModifiedbyValue = value; OnPropertyChanged("Modifiedby"); }
        }

        private DateTime? ModifiedonValue = null;
        public DateTime? Modifiedon
        {
            get { return ModifiedonValue; }
            set { ModifiedonValue = value; OnPropertyChanged("Modifiedon"); }
        }

        private string SummaryValue = null;
        public string Summary
        {
            get { return SummaryValue; }
            set { SummaryValue = value; OnPropertyChanged("Summary"); }
        }

        private string CategoryValue = null;
        public string Category
        {
            get { return CategoryValue; }
            set { CategoryValue = value; OnPropertyChanged("Category"); }
        }

        private Visibility TestCaseSettingsVisibleValue = Visibility.Hidden;
        public Visibility TestCaseSettingsVisible
        {
            get { return TestCaseSettingsVisibleValue; }
            set { TestCaseSettingsVisibleValue = value; OnPropertyChanged("TestCaseSettingsVisible"); }
        }

        private ObservableCollection<string> responsalyzerNameListValue = new ObservableCollection<string>();
        public ObservableCollection<string> ResponsalyzerNameList
        {
            get { return responsalyzerNameListValue; }
            set { responsalyzerNameListValue = value; }
        }

        private ObservableCollection<string> responsalyzerTypeListValue = new ObservableCollection<string>();
        public ObservableCollection<string> ResponsalyzerTypeList
        {
            get { return responsalyzerTypeListValue; }
            private set { responsalyzerTypeListValue = value; OnPropertyChanged("TestResponsalyzerTypeList"); }
        }
    }
}
