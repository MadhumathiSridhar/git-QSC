using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Windows;
using System.Data;
using System.Collections.ObjectModel;

namespace QSC_Test_Automation
{
    public class TestSuiteItem : INotifyPropertyChanged
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

                    if (SaveButtonIsEnabled != true && property != "SaveButtonIsEnabled" && property != "TestItemHeaderName" && isSkipSaveButtonEnable == false && property != "TestSuiteSettingsVisible" && property != "Category" && property != "Modifiedby" && property != "Modifiedon" && property != "Summary")
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

                    undoPropertyStack.Add(new Tuple<string, object, object, object> ( property, propertyVariable, oldValue, newValue));

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
                // MessageBox.Show("Exception\n " + ex.Message, "Error Code - ECxxxxx", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void UndoProperty()
        {
            if(undoPropertyStack.Count > 0)
            {
                Tuple<string, object, object, object> undoItem = undoPropertyStack.Last();
                if(undoItem.Item1 == "TestPlanListAddList")
                {
                    List<TreeViewExplorer> newValue = undoItem.Item4 as List<TreeViewExplorer>;
                    foreach (TreeViewExplorer item in newValue)
                    {
                        TestPlanList.Remove(item);
                    }
                }
                else if (undoItem.Item1 == "TestPlanListAddItem")
                {
                    TreeViewExplorer newValue = undoItem.Item4 as TreeViewExplorer;
                    TestPlanList.Remove(newValue);
                }
                else if (undoItem.Item1 == "TestPlanListInsertList")
                {
                    Tuple<int, List<TreeViewExplorer>> newValue = undoItem.Item4 as Tuple<int, List<TreeViewExplorer>>;
                    foreach (TreeViewExplorer item in newValue.Item2)
                    {
                        TestPlanList.Remove(item);
                    }
                }
                else if (undoItem.Item1 == "TestPlanListMoveList")
                {
                    Tuple<List<int>, List<TreeViewExplorer>> oldValue = undoItem.Item3 as Tuple<List<int>, List<TreeViewExplorer>>;
                    Tuple<int, List<TreeViewExplorer>> newValue = undoItem.Item4 as Tuple<int, List<TreeViewExplorer>>;
                    foreach (TreeViewExplorer item in newValue.Item2)
                    {
                        TestPlanList.Remove(item);
                    }
                    List<int> indexList = oldValue.Item1 as List<int>;
                    int i = 0;
                    foreach (TreeViewExplorer item in oldValue.Item2)
                    {
                        TestPlanList.Insert(indexList[i], item);
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
                        TestPlanList.Insert(indexList[i], item);
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
                        TestPlanList.Add(item);
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
                        TestPlanList.Add(item);
                    }
                }
                else if (redoItem.Item1 == "TestPlanListAddItem")
                {
                    TreeViewExplorer newValue = redoItem.Item4 as TreeViewExplorer;
                    TestPlanList.Add(newValue);
                }
                else if (redoItem.Item1 == "TestPlanListInsertList")
                {
                    Tuple<int, List<TreeViewExplorer>> newValue = redoItem.Item4 as Tuple<int, List<TreeViewExplorer>>;
                    int index = Convert.ToInt32(newValue.Item1);
                    foreach (TreeViewExplorer item in newValue.Item2)
                    {
                        TestPlanList.Insert(index-(newValue.Item2.Count), item);
                        index++;
                    }
                }
                else if (redoItem.Item1 == "TestPlanListMoveList")
                {
                    Tuple<List<int>, List<TreeViewExplorer>> oldValue = redoItem.Item3 as Tuple<List<int>, List<TreeViewExplorer>>;
                    Tuple<int, List<TreeViewExplorer>> newValue = redoItem.Item4 as Tuple<int, List<TreeViewExplorer>>;

                    foreach (var item in newValue.Item2)
                    {
                        TestPlanList.Insert(newValue.Item1, item);
                    }
                    foreach (var item in oldValue.Item2)
                    {
                        TestPlanList.Remove(item);
                    }
                }
                else if (redoItem.Item1 == "TestPlanListRemoveList")
                {
                    Tuple<List<int>, List<TreeViewExplorer>> oldValue = redoItem.Item3 as Tuple<List<int>, List<TreeViewExplorer>>;
                    foreach (TreeViewExplorer item in oldValue.Item2)
                    {
                        TestPlanList.Remove(item);
                    }
                }
                else if (redoItem.Item1 == "TestPlanListRemoveItem")
                {
                }
                else if (redoItem.Item1 == "TestPlanListRemoveAll")
                {
                    TestPlanList.Clear();
                }

                OnPropertyChanged(redoItem.Item1);

                undoPropertyStack.Add(redoItem);
                redoPropertyStack.Remove(redoItem);
            }
        }

        public TestSuiteItem()
        {
            try
            {
                IsEditModeEnabled = true;
                TestPlanListContextMenuVisibility = Visibility.Visible;
                TestSuiteSettingsVisible = Visibility.Hidden;
                ItemNameTextBoxIsEnabled = true;
                CreateNewTestPlanIsEnabled = false;
                BackgroundVerfificationButtonIsEnabled = false;
                TestPlanListIsEnabled = true;
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

        public TestSuiteItem(TreeViewExplorer sourceTestSuite, bool isEditModeEnabled)
        {
            try
            {
                if (isEditModeEnabled)
                {
                    IsEditModeEnabled = true;
                    TestPlanListContextMenuVisibility = Visibility.Visible;
                    ItemNameTextBoxIsEnabled = true;
                    TestSuiteSettingsVisible = Visibility.Visible;
                    CreateNewTestPlanIsEnabled = true;
                    BackgroundVerfificationButtonIsEnabled = true;
                    TestPlanListIsEnabled = true;
                }
                else
                {
                    IsEditModeEnabled = false;
                    TestPlanListContextMenuVisibility = Visibility.Hidden;
                    ItemNameTextBoxIsEnabled = false;
                    TestSuiteSettingsVisible = Visibility.Visible;
                    SaveCloseVisibility = Visibility.Hidden;
                    CreateNewTestPlanIsEnabled = false;
                    BackgroundVerfificationButtonIsEnabled = false;
                    TestPlanListIsEnabled = false;
                }
                IsNewTestSuite = false;
                TestSuiteTreeViewExplorer = sourceTestSuite;
                TestItemName = sourceTestSuite.ItemName;
                TestItemNameCopy = sourceTestSuite.ItemName;
                TestSuiteID = sourceTestSuite.ItemKey;
                Createdby = sourceTestSuite.Createdby;
                Createdon = sourceTestSuite.Createdon;
                Modifiedby = sourceTestSuite.Modifiedby;
                Modifiedon = sourceTestSuite.Modifiedon;
                Summary = sourceTestSuite.Summary;

                if (sourceTestSuite.Category != null)
                    Category = sourceTestSuite.Category;
                else
                    Category = sourceTestSuite.Category = string.Empty;

                //string query = string.Empty;
                //if (sourceTestSuite.ItemType == QatConstants.DbTestSuiteTable)
                //    query = "select Category from " + QatConstants.DbTestSuiteTable + " where " + QatConstants.DbTestPlanIDColumn + " = " + sourceTestSuite.ItemKey;

                //string categoryval = string.Empty;
                //if (query != string.Empty)
                //{
                //    DataTable dataTable = QscDataBase.SendCommand_Toreceive(query);
                //    DataTableReader read1 = dataTable.CreateDataReader();

                //    while (read1.Read())
                //    {
                //        categoryval = read1.GetValue(0).ToString();
                //    }

                //    Category = sourceTestSuite.Category = categoryval;
                //}

                TestPlanList = new ObservableCollection<TreeViewExplorer>(QscDataBase.ReadTreeTable(QatConstants.DbTestPlanTable, sourceTestSuite.ItemKey, null, null));
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

        public void TestPlanListAddList(List<TreeViewExplorer> itemList)
        {
            List<TreeViewExplorer> newItemList = new List<TreeViewExplorer>();

            foreach (TreeViewExplorer item in itemList)
            {
                TreeViewExplorer newItem = new TreeViewExplorer(item);
                TestPlanList.Add(newItem);
                newItemList.Add(newItem);
            }
            OnPropertyChanged("TestPlanListAddList", null, null, newItemList);
            //SaveButtonIsEnabled = true;
        }

        public void TestPlanListAddItem(TreeViewExplorer item)
        {
            TreeViewExplorer newItem = new TreeViewExplorer(item);
            TestPlanList.Add(newItem);
            OnPropertyChanged("TestPlanListAddItem", null, null, newItem);
            //SaveButtonIsEnabled = true;
        }

        public void TestPlanListInsertList(int index, List<TreeViewExplorer> itemList)
        {
            List<TreeViewExplorer> newItemList = new List<TreeViewExplorer>();

            foreach (var item in itemList)
            {
                TreeViewExplorer newItem = new TreeViewExplorer(item);
                TestPlanList.Insert(index, newItem);
                newItemList.Add(newItem);
                index++;
            }
            OnPropertyChanged("TestPlanListInsertList", null, null, new Tuple<int, List<TreeViewExplorer>>(index, newItemList));
            //SaveButtonIsEnabled = true;
        }

        public void TestPlanListMoveList(int index, List<TreeViewExplorer> itemList)
        {
            List<TreeViewExplorer> newItemList = new List<TreeViewExplorer>();
            List<int> indexList = new List<int>();

            foreach (var item in itemList)
            {
                indexList.Add(TestPlanList.IndexOf(item));
                TreeViewExplorer newItem = new TreeViewExplorer(item);
                TestPlanList.Insert(index, newItem);
                newItemList.Add(newItem);
                TestPlanList.Remove(item);
            }

            var sortedList = indexList.Zip(newItemList, (x, y) => new { x, y }).OrderBy(pair => pair.x).ToList();
            indexList = sortedList.Select(pair => pair.x).ToList();
            newItemList = sortedList.Select(pair => pair.y).ToList();

            OnPropertyChanged("TestPlanListMoveList", null, new Tuple<List<int>, List<TreeViewExplorer>>(indexList,itemList), new Tuple<int,List<TreeViewExplorer>>(index,newItemList));
            //SaveButtonIsEnabled = true;
        }

        public void TestPlanListRemoveList(List<TreeViewExplorer> itemList)
        {
            List<int> indexList = new List<int>();
            foreach (TreeViewExplorer item in itemList)
            {
                indexList.Add(TestPlanList.IndexOf(item));
                TestPlanList.Remove(item);
            }

            var sortedList = indexList.Zip(itemList, (x, y) => new { x, y }).OrderBy(pair => pair.x).ToList();
            indexList = sortedList.Select(pair => pair.x).ToList();
            itemList = sortedList.Select(pair => pair.y).ToList();

            OnPropertyChanged("TestPlanListRemoveList", null, new Tuple<List<int>, List<TreeViewExplorer>>(indexList, itemList), null);
            //SaveButtonIsEnabled = true;
        }

        public void TestPlanListRemoveItem(TreeViewExplorer item)
        {
            TestPlanList.Remove(item);
            //OnPropertyChanged("TestPlanListRemoveItem", null, null, item);
            //SaveButtonIsEnabled = true;
        }

        public void TestPlanListRemoveAll()
        {
            List<TreeViewExplorer> testPlanListCopy = new List<TreeViewExplorer>(TestPlanList);
            TestPlanList.Clear();
            OnPropertyChanged("TestPlanListRemoveAll", null, testPlanListCopy, null);
            //SaveButtonIsEnabled = true;
        }

        public Visibility TestSuiteGridVisibility
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
                        return "*TS: New Test Suite";
                    else
                        return "*TS: " + testItemNameValue;
                }
                else
                {
                    if (string.IsNullOrEmpty(testItemNameValue))
                        return "TS: New Test Suite";
                    else
                        return "TS: " + testItemNameValue;
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

        private int testSuiteIDValue = 0;
        public int TestSuiteID
        {
            get { return testSuiteIDValue; }
            set
            {
                testSuiteIDValue = value;
                OnPropertyChanged("TestSuiteID");
            }
        }

        private TreeViewExplorer testSuiteTreeViewExplorerValue = null;
        public TreeViewExplorer TestSuiteTreeViewExplorer
        {
            get { return testSuiteTreeViewExplorerValue; }
            set
            {
                testSuiteTreeViewExplorerValue = value;
                OnPropertyChanged("TestSuiteTreeViewExplorer");
            }
        }

        private bool isNewTestSuiteValue = true;
        public bool IsNewTestSuite
        {
            get { return isNewTestSuiteValue; }
            set
            {
                isNewTestSuiteValue = value;
                OnPropertyChanged("IsNewTestSuite");
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

        private ObservableCollection<TreeViewExplorer> testPlanListValue = new ObservableCollection<TreeViewExplorer>();
        public ObservableCollection<TreeViewExplorer> TestPlanList
        {
            get { return testPlanListValue; }
            set { testPlanListValue = value; OnPropertyChanged("TestPlanList"); }
        }

        private bool testPlanListIsEnabledValue = false;
        public bool TestPlanListIsEnabled
        {
            get { return testPlanListIsEnabledValue; }
            set { testPlanListIsEnabledValue = value; OnPropertyChanged("TestPlanListIsEnabled"); }
        }

        private Visibility testPlanListContextMenuVisibilityValue = Visibility.Hidden;
        public Visibility TestPlanListContextMenuVisibility
        {
            get { return testPlanListContextMenuVisibilityValue; }
            set { testPlanListContextMenuVisibilityValue = value; OnPropertyChanged("TestPlanListContextMenuVisibility"); }
        }

        private List<TreeViewExplorer> testPlanSelectedListValue = new List<TreeViewExplorer>();
        public List<TreeViewExplorer> TestPlanSelectedList
        {
            get { return testPlanSelectedListValue; }
            set { testPlanSelectedListValue = value; }
        }
      
        private bool createNewTestPlanIsEnabledValue = false;
        public bool CreateNewTestPlanIsEnabled
        {
            get { return createNewTestPlanIsEnabledValue; }
            set { createNewTestPlanIsEnabledValue = value; OnPropertyChanged("CreateNewTestPlanIsEnabled"); }
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

        private List<TreeViewExplorer> testPlanListForCutCopyValue = new List<TreeViewExplorer>();
        public List<TreeViewExplorer> TestPlanListForCutCopy
        {
            get { return testPlanListForCutCopyValue; }
            set { testPlanListForCutCopyValue = value; }
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

        private Visibility TestSuiteSettingsVisibleValue = Visibility.Hidden;
        public Visibility TestSuiteSettingsVisible
        {
            get { return TestSuiteSettingsVisibleValue; }
            set { TestSuiteSettingsVisibleValue = value; OnPropertyChanged("TestSuiteSettingsVisible"); }
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
