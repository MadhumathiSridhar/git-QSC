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
using System.Data;

namespace QSC_Test_Automation
{
    public class TestPlanItem : INotifyPropertyChanged
    {
        DBConnection QscDataBase = new DBConnection();
        public event PropertyChangedEventHandler PropertyChanged;
        List<Tuple<string, object, object, object>> undoPropertyStack = new List<Tuple<string, object, object, object>>();
        List<Tuple<string, object, object, object>> redoPropertyStack = new List<Tuple<string, object, object, object>>();

        bool isSkipSaveButtonEnable = false;
        void OnPropertyChanged(string property)
        {
            try
            {
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs(property));

                    if (SaveButtonIsEnabled != true && property != "SaveButtonIsEnabled" && property != "TestItemHeaderName" && isSkipSaveButtonEnable == false && property != "TestPlanSettingsVisible" && property != "Category" && property != "Modifiedby" && property != "Modifiedon" && property != "Summary")
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

        void OnPropertyChanged(string property, object propertyVariable, object oldValue, object newValue)
        {
            try
            {
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs(property));

                    undoPropertyStack.Add(new Tuple<string, object, object, object>(property, propertyVariable, oldValue, newValue));

                    redoPropertyStack.Clear();

                    if (SaveButtonIsEnabled != true && property != "SaveButtonIsEnabled" && property != "TestItemHeaderName" && isSkipSaveButtonEnable == false && property != "TestSuiteSettingsVisible")
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
                //MessageBox.Show("Exception\n " + ex.Message, "Error Code - ECxxxxx", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void UndoProperty()
        {
            if (undoPropertyStack.Count > 0)
            {
                Tuple<string, object, object, object> undoItem = undoPropertyStack.Last();
                if (undoItem.Item1 == "TestPlanListAddList")
                {
                    List<TreeViewExplorer> newValue = undoItem.Item4 as List<TreeViewExplorer>;
                    foreach (TreeViewExplorer item in newValue)
                    {
                        TestCaseList.Remove(item);
                    }
                }
                else if (undoItem.Item1 == "TestPlanListAddItem")
                {
                    TreeViewExplorer newValue = undoItem.Item4 as TreeViewExplorer;
                    TestCaseList.Remove(newValue);
                }
                else if (undoItem.Item1 == "TestPlanListInsertList")
                {
                    Tuple<int, List<TreeViewExplorer>> newValue = undoItem.Item4 as Tuple<int, List<TreeViewExplorer>>;
                    foreach (TreeViewExplorer item in newValue.Item2)
                    {
                        TestCaseList.Remove(item);
                    }
                }
                else if (undoItem.Item1 == "TestPlanListMoveList")
                {
                    Tuple<List<int>, List<TreeViewExplorer>> oldValue = undoItem.Item3 as Tuple<List<int>, List<TreeViewExplorer>>;
                    Tuple<int, List<TreeViewExplorer>> newValue = undoItem.Item4 as Tuple<int, List<TreeViewExplorer>>;
                    foreach (TreeViewExplorer item in newValue.Item2)
                    {
                        TestCaseList.Remove(item);
                    }
                    List<int> indexList = oldValue.Item1 as List<int>;
                    int i = 0;
                    foreach (TreeViewExplorer item in oldValue.Item2)
                    {
                        TestCaseList.Insert(indexList[i], item);
                        i++;
                    }
                }
                else if (undoItem.Item1 == "TestPlanListRemoveList")
                {
                    Tuple<List<int>, List<TreeViewExplorer>> oldValue = undoItem.Item3 as Tuple<List<int>, List<TreeViewExplorer>>;
                    List<int> indexList = oldValue.Item1;
                    int i = 0;
                    foreach (TreeViewExplorer item in oldValue.Item2)
                    {
                        TestCaseList.Insert(indexList[i], item);
                        i++;
                    }
                }
                else if (undoItem.Item1 == "TestPlanListRemoveItem")
                {
                }
                else if (undoItem.Item1 == "TestPlanListRemoveAll")
                {
                    List<TreeViewExplorer> oldValue = undoItem.Item3 as List<TreeViewExplorer>;
                    foreach (TreeViewExplorer item in oldValue)
                    {
                        TestCaseList.Add(item);
                    }
                }

                OnPropertyChanged(undoItem.Item1);

                undoPropertyStack.Remove(undoItem);
                redoPropertyStack.Add(undoItem);
            }
        }

        public void RedoProperty()
        {
            if (redoPropertyStack.Count > 0)
            {
                Tuple<string, object, object, object> redoItem = redoPropertyStack.Last();
                if (redoItem.Item1 == "TestPlanListAddList")
                {
                    List<TreeViewExplorer> newValue = redoItem.Item4 as List<TreeViewExplorer>;
                    foreach (TreeViewExplorer item in newValue)
                    {
                        TestCaseList.Add(item);
                    }
                }
                else if (redoItem.Item1 == "TestPlanListAddItem")
                {
                    TreeViewExplorer newValue = redoItem.Item4 as TreeViewExplorer;
                    TestCaseList.Add(newValue);
                }
                else if (redoItem.Item1 == "TestPlanListInsertList")
                {
                    Tuple<int, List<TreeViewExplorer>> newValue = redoItem.Item4 as Tuple<int, List<TreeViewExplorer>>;
                    int index = Convert.ToInt32(newValue.Item1);
                    foreach (TreeViewExplorer item in newValue.Item2)
                    {
                       
                        TestCaseList.Insert(index-(newValue.Item2.Count), item);
                        index++;
                    }
                }
                else if (redoItem.Item1 == "TestPlanListMoveList")
                {
                    Tuple<List<int>, List<TreeViewExplorer>> oldValue = redoItem.Item3 as Tuple<List<int>, List<TreeViewExplorer>>;
                    Tuple<int, List<TreeViewExplorer>> newValue = redoItem.Item4 as Tuple<int, List<TreeViewExplorer>>;

                    foreach (var item in newValue.Item2)
                    {
                        TestCaseList.Insert(newValue.Item1, item);
                    }
                    foreach (var item in oldValue.Item2)
                    {
                        TestCaseList.Remove(item);
                    }
                }
                else if (redoItem.Item1 == "TestPlanListRemoveList")
                {
                    Tuple<List<int>, List<TreeViewExplorer>> oldValue = redoItem.Item3 as Tuple<List<int>, List<TreeViewExplorer>>;
                    foreach (TreeViewExplorer item in oldValue.Item2)
                    {
                        TestCaseList.Remove(item);
                    }
                }
                else if (redoItem.Item1 == "TestPlanListRemoveItem")
                {
                }
                else if (redoItem.Item1 == "TestPlanListRemoveAll")
                {
                    TestCaseList.Clear();
                }

                OnPropertyChanged(redoItem.Item1);

                undoPropertyStack.Add(redoItem);
                redoPropertyStack.Remove(redoItem);
            }
        }

        public TestPlanItem()
        {
            try
            {
                IsEditModeEnabled = true;
                TestCaseListContextMenuVisibility = Visibility.Visible;
                TestPlanSettingsVisible = Visibility.Hidden;
                ItemNameTextBoxIsEnabled = true;
                CreateNewTestCaseIsEnabled = false;
                DesignBrowseButtonIsEnabled = true;
                BackgroundVerfificationButtonIsEnabled = false;
                DeployChkBoxEnable = true;
                SelectedDeployItemEnable = false;
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

        public TestPlanItem(TestSuiteItem sourceTestSuiteItem)
        {
            try
            {
                IsEditModeEnabled = true;
                TestCaseListContextMenuVisibility = Visibility.Visible;
                TestPlanSettingsVisible = Visibility.Hidden;
                ItemNameTextBoxIsEnabled = true;
                CreateNewTestCaseIsEnabled = false;
                DesignBrowseButtonIsEnabled = true;
                BackgroundVerfificationButtonIsEnabled = false;
                IsTestPlanCreatedFromTestSuite = true;
                ParentTestSuiteItem = sourceTestSuiteItem;
                DeployChkBoxEnable = true;
                SelectedDeployItemEnable = false;
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

        public TestPlanItem(TreeViewExplorer sourceTestPlan, bool isEditModeEnabled)
        {
            try
            {
                if (isEditModeEnabled)
                {
                    IsEditModeEnabled = true;
                    TestCaseListContextMenuVisibility = Visibility.Visible;
                    TestPlanSettingsVisible = Visibility.Visible;
                    ItemNameTextBoxIsEnabled = true;
                    CreateNewTestCaseIsEnabled = true;
                    DesignBrowseButtonIsEnabled = true;
                    BackgroundVerfificationButtonIsEnabled = true;
                    TestCaseListIsEnabled = true;
                    DeployChkBoxEnable = true;
                    SelectedDeployItemEnable = true;
                    HasDesignCheckBoxIsEnabled = true;
                }
                else
                {
                    IsEditModeEnabled = false;
                    TestCaseListContextMenuVisibility = Visibility.Collapsed;
                    ItemNameTextBoxIsEnabled = false;
                    SaveCloseVisibility = Visibility.Hidden;
                    TestPlanSettingsVisible = Visibility.Visible;
                    CreateNewTestCaseIsEnabled = false;
                    DesignBrowseButtonIsEnabled = false;
                    BackgroundVerfificationButtonIsEnabled = false;
                    DeployChkBoxEnable = false;
                    SelectedDeployItemEnable = false;
                    HasDesignCheckBoxIsEnabled = false;
                }

                IsNewTestPlan = false;
                IsNewTestDesign = false;
                TestPlanTreeViewExplorer = sourceTestPlan;
                TestItemName = sourceTestPlan.ItemName;
                TestItemNameCopy = sourceTestPlan.ItemName;
                TestPlanID = sourceTestPlan.ItemKey;
                Createdby = sourceTestPlan.Createdby;
                Createdon = sourceTestPlan.Createdon;
                Modifiedby = sourceTestPlan.Modifiedby;
                Modifiedon = sourceTestPlan.Modifiedon;
                Summary = sourceTestPlan.Summary;
                
                if (sourceTestPlan.Category != null)
                    Category = sourceTestPlan.Category;
                else
                    Category = sourceTestPlan.Category = string.Empty;

                //string query = string.Empty;
                //if (sourceTestPlan.ItemType == QatConstants.DbTestPlanTable)
                //    query = "select Category from " + QatConstants.DbTestPlanTable + " where " + QatConstants.DbTestPlanIDColumn + " = " + sourceTestPlan.ItemKey;
                
                //string categoryval = string.Empty;
                //if (query != string.Empty)
                //{
                //    DataTable dataTable = QscDataBase.SendCommand_Toreceive(query);
                //    DataTableReader read1 = dataTable.CreateDataReader();

                //    while (read1.Read())
                //    {
                //        categoryval = read1.GetValue(0).ToString();
                //    }

                //    Category = sourceTestPlan.Category = categoryval;
                //}

                TestCaseList = new ObservableCollection<TreeViewExplorer>(QscDataBase.ReadTreeTable(QatConstants.DbTestCaseTable, sourceTestPlan.ItemKey, null, null));
                var result = QscDataBase.GetDesignNameFromDB(sourceTestPlan.ItemName, sourceTestPlan.ItemType);

                DesignNameCopy = DesignName = result.Item1;

                IsDesignChecked = result.Item2;
                
                DesignNameList = QscDataBase.GetDesignListFromDB(sourceTestPlan.ItemName, sourceTestPlan.ItemType);

                var deployValues = QscDataBase.GetDeployValuesFromDB(sourceTestPlan.ItemName, sourceTestPlan.ItemType);
                IsNoOfDeployChecked = deployValues.Item1;               
                SelectedDeployItem = deployValues.Item2;               
                //IsScriptChecked = deployValues.Item2;
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

        public void TestCaseListAddList(List<TreeViewExplorer> itemList)
        {
            List<TreeViewExplorer> newItemList = new List<TreeViewExplorer>();

            foreach (TreeViewExplorer item in itemList)
            {
                TreeViewExplorer newItem = new TreeViewExplorer(item);
                TestCaseList.Add(newItem);
                newItemList.Add(newItem);
            }
            OnPropertyChanged("TestPlanListAddList", null, null, newItemList);
            //SaveButtonIsEnabled = true;
        }

        public void TestCaseListAddItem(TreeViewExplorer item)
        {
            TreeViewExplorer newItem = new TreeViewExplorer(item);
            TestCaseList.Add(newItem);
            OnPropertyChanged("TestPlanListAddItem", null, null, newItem);
            //SaveButtonIsEnabled = true;
        }

        public void TestCaseListInsertList(int index, List<TreeViewExplorer> itemList)
        {
            List<TreeViewExplorer> newItemList = new List<TreeViewExplorer>();

            foreach (var item in itemList)
            {
                TreeViewExplorer newItem = new TreeViewExplorer(item);
                TestCaseList.Insert(index, newItem);
                newItemList.Add(newItem);
                index++;
            }
            OnPropertyChanged("TestPlanListInsertList", null, null, new Tuple<int, List<TreeViewExplorer>>(index, newItemList));
            //SaveButtonIsEnabled = true;
        }

        public void TestCaseListMoveList(int index, List<TreeViewExplorer> itemList)
        {
            List<TreeViewExplorer> newItemList = new List<TreeViewExplorer>();
            List<int> indexList = new List<int>();

            foreach (var item in itemList)
            {
                indexList.Add(TestCaseList.IndexOf(item));
                TreeViewExplorer newItem = new TreeViewExplorer(item);
                TestCaseList.Insert(index, newItem);
                newItemList.Add(newItem);
                TestCaseList.Remove(item);
            }

            var sortedList = indexList.Zip(newItemList, (x, y) => new { x, y }).OrderBy(pair => pair.x).ToList();
            indexList = sortedList.Select(pair => pair.x).ToList();
            newItemList = sortedList.Select(pair => pair.y).ToList();

            OnPropertyChanged("TestPlanListMoveList", null, new Tuple<List<int>, List<TreeViewExplorer>>(indexList, itemList), new Tuple<int, List<TreeViewExplorer>>(index, newItemList));
            //SaveButtonIsEnabled = true;
        }

        public void TestCaseListRemoveList(List<TreeViewExplorer> itemList)
        {
            List<int> indexList = new List<int>();
            foreach (TreeViewExplorer item in itemList)
            {
                indexList.Add(TestCaseList.IndexOf(item));
                TestCaseList.Remove(item);
            }

            var sortedList = indexList.Zip(itemList, (x, y) => new { x, y }).OrderBy(pair => pair.x).ToList();
            indexList = sortedList.Select(pair => pair.x).ToList();
            itemList = sortedList.Select(pair => pair.y).ToList();

            OnPropertyChanged("TestPlanListRemoveList", null, new Tuple<List<int>, List<TreeViewExplorer>>(indexList, itemList), null);
            //SaveButtonIsEnabled = true;
        }

        public void TestCaseListRemoveItem(TreeViewExplorer item)
        {
            TestCaseList.Remove(item);
            //OnPropertyChanged("TestPlanListRemoveItem", null, null, item);
            //SaveButtonIsEnabled = true;
        }

        public void TestCaseListRemoveAll()
        {
            List<TreeViewExplorer> testPlanListCopy = new List<TreeViewExplorer>(TestCaseList);
            TestCaseList.Clear();
            OnPropertyChanged("TestPlanListRemoveAll", null, testPlanListCopy, null);
            //SaveButtonIsEnabled = true;
        }

        public Visibility TestPlanGridVisibility
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
                        return "*TP: New Test Plan";
                    else
                        return "*TP: " + testItemNameValue;
                }
                else
                {
                    if (string.IsNullOrEmpty(testItemNameValue))
                        return "TP: New Test Plan";
                    else
                        return "TP: " + testItemNameValue;
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
                OnPropertyChanged("TestItemNameCopy");
            }
        }

        private int testPlanIDValue = 0;
        public int TestPlanID
        {
            get { return testPlanIDValue; }
            set
            {
                testPlanIDValue = value;
                OnPropertyChanged("TestPlanID");
            }
        }

        private TreeViewExplorer testPlanTreeViewExplorerValue = null;
        public TreeViewExplorer TestPlanTreeViewExplorer
        {
            get { return testPlanTreeViewExplorerValue; }
            set
            {
                testPlanTreeViewExplorerValue = value;
                OnPropertyChanged("TestCaseTreeViewExplorer");
            }
        }

        private bool isNewTestPlanValue = true;
        public bool IsNewTestPlan
        {
            get { return isNewTestPlanValue; }
            set
            {
                isNewTestPlanValue = value;
                OnPropertyChanged("IsNewTestPlan");
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

        private bool isNewTestDesignValue = true;
        public bool IsNewTestDesign
        {
            get { return isNewTestDesignValue; }
            set
            {
                isNewTestDesignValue = value;
                OnPropertyChanged("IsNewTestDesign");
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

        private bool designBrowseButtonIsEnabledValue = false;
        public bool DesignBrowseButtonIsEnabled
        {
            get { return designBrowseButtonIsEnabledValue; }
            set { designBrowseButtonIsEnabledValue = value; OnPropertyChanged("DesignBrowseButtonIsEnabled"); }
        }

        private bool isTestPlanCreatedFromTestSuiteValue = false;
        public bool IsTestPlanCreatedFromTestSuite
        {
            get { return isTestPlanCreatedFromTestSuiteValue; }
            set { isTestPlanCreatedFromTestSuiteValue = value; OnPropertyChanged("IsTestPlanCreatedFromTestSuite"); }
        }

        private TestSuiteItem parentTestSuiteItemValue = null;
        public TestSuiteItem ParentTestSuiteItem
        {
            get { return parentTestSuiteItemValue; }
            set { parentTestSuiteItemValue = value; OnPropertyChanged("ParentTestSuiteItem"); }
        }

        private bool backgroundVerfificationButtonIsEnabledValue = false;
        public bool BackgroundVerfificationButtonIsEnabled
        {
            get { return backgroundVerfificationButtonIsEnabledValue; }
            set { backgroundVerfificationButtonIsEnabledValue = value; OnPropertyChanged("BackgroundVerfificationButtonIsEnabled"); }
        }

        private CBMItems backgroundMonitoringValue = new CBMItems();
        public CBMItems BackgroundMonitoring
        {
            get { return backgroundMonitoringValue; }
            set { backgroundMonitoringValue = value; OnPropertyChanged("BackgroundMonitoring"); }
        }

        private ObservableCollection<TreeViewExplorer> testCaseListValue = new ObservableCollection<TreeViewExplorer>();
        public ObservableCollection<TreeViewExplorer> TestCaseList
        {
            get { return testCaseListValue; }
            set { testCaseListValue = value; OnPropertyChanged("TestCaseList"); }
        }

        private bool testCaseListIsEnabledValue = false;
        public bool TestCaseListIsEnabled
        {
            get { return testCaseListIsEnabledValue; }
            set { testCaseListIsEnabledValue = value; OnPropertyChanged("TestCaseListIsEnabled"); }
        }

        private Visibility testCaseListContextMenuVisibilityValue = Visibility.Hidden;
        public Visibility TestCaseListContextMenuVisibility
        {
            get { return testCaseListContextMenuVisibilityValue; }
            set { testCaseListContextMenuVisibilityValue = value; OnPropertyChanged("TestCaseListContextMenuVisibility"); }
        }

        private List<TreeViewExplorer> testCaseSelectedListValue = new List<TreeViewExplorer>();
        public List<TreeViewExplorer> TestCaseSelectedList
        {
            get { return testCaseSelectedListValue; }
            set { testCaseSelectedListValue = value;}
        }

        private bool createNewTestCaseIsEnabledValue = false;
        public bool CreateNewTestCaseIsEnabled
        {
            get { return createNewTestCaseIsEnabledValue; }
            set { createNewTestCaseIsEnabledValue = value; OnPropertyChanged("CreateNewTestCaseIsEnabled"); }
        }

        private bool isDesignCheckedValue = true;
        public bool IsDesignChecked
        {
            get { return isDesignCheckedValue; }
            set
            {
                isDesignCheckedValue = value;
                OnPropertyChanged("IsDesignChecked");

                if (IsEditModeEnabled)
                {
                    if (value == false)
                    {
                        BackgroundVerfificationButtonIsEnabled = false;
                        DeployChkBoxEnable = false;
                        SelectedDeployItemEnable = false;
                        DesignBrowseButtonIsEnabled = false;                       
                    }
                    else if (value == true)
                    {
                        if (!IsNewTestPlan)
                        {
                            BackgroundVerfificationButtonIsEnabled = true;
                            SelectedDeployItemEnable = true;
                        }                       
                        DeployChkBoxEnable = true;
                        DesignBrowseButtonIsEnabled = true;
                    }
                }
            }
        }

        private bool hasDesignCheckBoxIsEnabledValue = true;
        public bool HasDesignCheckBoxIsEnabled
        {
            get { return hasDesignCheckBoxIsEnabledValue; }
            set { hasDesignCheckBoxIsEnabledValue = value; OnPropertyChanged("HasDesignCheckBoxIsEnabled"); }
        }

        private string designNameValue = null;
        public string DesignName
        {
            get { return designNameValue; }
            set { designNameValue = value; OnPropertyChanged("DesignName"); }
        }

        private string designNameCopyValue = null;
        public string DesignNameCopy
        {
            get { return designNameCopyValue; }
            set { designNameCopyValue = value; OnPropertyChanged("DesignNameCopy"); }
        }

        private int designIDValue = 0;
        public int DesignID
        {
            get { return designIDValue; }
            set { designIDValue = value; OnPropertyChanged("DesignID"); }
        }

        private List<string> designNameListValue = new List<string>();
        public List<string> DesignNameList
        {
            get { return designNameListValue; }
            set { designNameListValue = value; OnPropertyChanged("DesignNameList"); }
        }

        private string designFileNameValue = null;
        public string DesignFileName
        {
            get { return designFileNameValue; }
            set { designFileNameValue = value; }
        }

        private string designFilePathValue = null;
        public string DesignFilePath
        {
            get { return designFilePathValue; }
            set { designFilePathValue = value; }
        }

        private DataTable designComponentValue = new DataTable();
        public DataTable DesignComponent
        {
            get { return designComponentValue; }
            set { designComponentValue = value; }
        }

        private List<string> designInventoryValue = new List<string>();
        public List<string> DesignInventory
        {
            get { return designInventoryValue; }
            set { designInventoryValue = value; }
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

        private List<TreeViewExplorer> testCaseListForCutCopyValue = new List<TreeViewExplorer>();
        public List<TreeViewExplorer> TestCaseListForCutCopy
        {
            get { return testCaseListForCutCopyValue; }
            set { testCaseListForCutCopyValue = value; }
        }

        private bool isCutMenuSelectedValue = false;
        public bool IsCutMenuSelected
        {
            get { return isCutMenuSelectedValue; }
            set { isCutMenuSelectedValue = value; }
        }

        private bool isCutMenuEnabledValue = false;
        public bool IsCutMenuEnabled
        {
            get { return isCutMenuEnabledValue; }
            set { isCutMenuEnabledValue = value; isSkipSaveButtonEnable = true; OnPropertyChanged("IsCutMenuEnabled"); }
        }

        private bool isCopyMenuEnabledValue = false;
        public bool IsCopyMenuEnabled
        {
            get { return isCopyMenuEnabledValue; }
            set { isCopyMenuEnabledValue = value; isSkipSaveButtonEnable = true; OnPropertyChanged("IsCopyMenuEnabled"); }
        }

        private bool isPasteMenuEnabledValue = false;
        public bool IsPasteMenuEnabled
        {
            get { return isPasteMenuEnabledValue; }
            set { isPasteMenuEnabledValue = value; isSkipSaveButtonEnable = true; OnPropertyChanged("IsPasteMenuEnabled"); }
        }

        private bool isRemoveMenuEnabledValue = false;
        public bool IsRemoveMenuEnabled
        {
            get { return isRemoveMenuEnabledValue; }
            set { isRemoveMenuEnabledValue = value; isSkipSaveButtonEnable = true; OnPropertyChanged("IsRemoveMenuEnabled"); }
        }

        private bool isRemoveAllMenuEnabledValue = false;
        public bool IsRemoveAllMenuEnabled
        {
            get { return isRemoveAllMenuEnabledValue; }
            set { isRemoveAllMenuEnabledValue = value; isSkipSaveButtonEnable = true; OnPropertyChanged("IsRemoveAllMenuEnabled"); }
        }

        private bool isNoOfDeployCheckedValue = false;
        public bool IsNoOfDeployChecked
        {
            get { return isNoOfDeployCheckedValue; }
            set
            {
                isNoOfDeployCheckedValue = value;
                OnPropertyChanged("IsNoOfDeployChecked");

                if (DeployChkBoxEnable == true)
                {
                    if (value == true)
                    {
                        SelectedDeployItem = "2";
                        SelectedDeployItemEnable = true;
                    }
                    else
                    {
                        SelectedDeployItem = null;
                        SelectedDeployItemEnable = false;
                    }
                }
            }
        }

        private bool deployChkBoxEnableValue = false;
        public bool DeployChkBoxEnable
        {
            get { return deployChkBoxEnableValue; }
            set { deployChkBoxEnableValue = value; OnPropertyChanged("DeployChkBoxEnable"); }
        }

        private string selectedDeployItemValue = null;
        public string SelectedDeployItem
        {
            get { return selectedDeployItemValue; }
            set { selectedDeployItemValue = value; OnPropertyChanged("SelectedDeployItem"); }
        }

        private bool selectedDeployEnableValue = false;
        public bool SelectedDeployItemEnable
        {
            get { return selectedDeployEnableValue; }
            set { selectedDeployEnableValue = value; OnPropertyChanged("SelectedDeployItemEnable"); }
        }

        private List<string> deployItemListValue = new List<string> {"1", "2", "3", "4", "5", "6", "7", "8", "9", "10" };
        public List<string> DeployItemList
        {
            get { return deployItemListValue; }
            set { deployItemListValue = value; OnPropertyChanged("DeployItemList"); }
        }

        private Visibility TestPlanSettingsVisibleValue = Visibility.Hidden;
        public Visibility TestPlanSettingsVisible
        {
            get { return TestPlanSettingsVisibleValue; }
            set { TestPlanSettingsVisibleValue = value; OnPropertyChanged("TestPlanSettingsVisible"); }
        }

        private int previousFindIndexValue = -1;
        public int PreviousFindIndex
        {
            get { return previousFindIndexValue; }
            set { previousFindIndexValue = value; }
        }

        private string previousFindTextValue = null;
        public string PreviousFindText
        {
            get { return previousFindTextValue; }
            set { previousFindTextValue = value; }
        }


    }
}

