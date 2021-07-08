using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace QSC_Test_Automation
{
    /// <summary>
    /// Interaction logic for Properties.xaml
    /// </summary>
    public partial class QATProperties : Window, INotifyPropertyChanged
    {     
        DBConnection connect = new DBConnection();
        DataTable tble = new DataTable();

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
        public QATProperties(TreeViewExplorer sourceTreeViewExplorer)
        {
            try
            {
                this.DataContext = this;
                InitializeComponent();

                if (this.WindowState == WindowState.Normal)
                {
                    b.HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled;
                }
                else
                {
                    b.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
                }

                isSaved = false;
                label1.Text = sourceTreeViewExplorer.ItemType;
                ContentName.Text = sourceTreeViewExplorer.ItemName;
                CreatorName.Text = sourceTreeViewExplorer.Createdby;
                ImporterName.Text = sourceTreeViewExplorer.Importedby;

                if (sourceTreeViewExplorer.Createdon != null & sourceTreeViewExplorer.Createdon != DateTime.Parse("1/1/1900 12:00:00 AM"))
                    CreatedDate.Text = sourceTreeViewExplorer.Createdon.ToString();
                else
                    CreatedDate.Text = string.Empty;

                if (sourceTreeViewExplorer.ImportedOn != null & sourceTreeViewExplorer.ImportedOn != DateTime.Parse("1/1/1900 12:00:00 AM"))
                    ImporterDate.Text = sourceTreeViewExplorer.ImportedOn.ToString();
                else
                    ImporterDate.Text = string.Empty;

                ModifierName.Text = sourceTreeViewExplorer.Modifiedby;
                if (sourceTreeViewExplorer.Modifiedon != null & sourceTreeViewExplorer.Modifiedon != DateTime.Parse("1/1/1900 12:00:00 AM"))

                    ModifiedDate.Text = sourceTreeViewExplorer.Modifiedon.ToString();
                else
                    ModifiedDate.Text = string.Empty;

                txtSummary.Text = Summary = sourceTreeViewExplorer.Summary;
                testID.Text = sourceTreeViewExplorer.ItemKey.ToString();
                //txtcategory.Text = sourceTreeViewExplorer.Category;
                CategorySelected = Category = sourceTreeViewExplorer.Category;
                string query = string.Empty;
                if (String.Equals(sourceTreeViewExplorer.ItemType, "TestSuite"))
                {
                    List<TreeViewExplorer> testSuiteList = connect.ReadTreeTable(QatConstants.DbTestPlanTable, sourceTreeViewExplorer.ItemKey, null, null);
                    testSuiteList.ForEach(item => AssociatedItem.Add(item.ItemName));
                    Associated.Text = "Related TP  :";
                }
                if (String.Equals(sourceTreeViewExplorer.ItemType, "TestPlan"))
                {
                    List<TreeViewExplorer> testPlanList = connect.ReadTreeTable(QatConstants.DbTestSuiteTable, sourceTreeViewExplorer.ItemKey, null, null);
                    testPlanList.ForEach(item => AssociatedItem.Add(item.ItemName));
                    Associated.Text = "Related TS  :";
                }
                if (String.Equals(sourceTreeViewExplorer.ItemType, "TestCase"))
                {
                    List<TreeViewExplorer> testCaseList = connect.ReadTreeTableTC(QatConstants.DbTestPlanTable, sourceTreeViewExplorer.ItemKey, null, null);
                    testCaseList.ForEach(item => AssociatedItem.Add(item.ItemName));
                    Associated.Text = "Related TP  :";
                }
                Dictionary<string,int> categoryList = new Dictionary<string, int>();
                categoryList = connect.ReadFilterItemList(QatConstants.DbCategoryColumnName);
                //categoryList = connect.ReadCategoryfromTreeTable();
                string[] alphaNumericSortedCategory = categoryList.Keys.ToArray();
                Array.Sort(alphaNumericSortedCategory, new AlphanumComparatorFaster());
                List<string> sortedcategoryList = new List<string>(alphaNumericSortedCategory.ToList());
                combocategory.ItemsSource = sortedcategoryList;
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
                if (Properties.Settings.Default.TesterName.ToString() != string.Empty)
                {
                    string itemNewName = txtSummary.Text;
                    DateTime? modifiedTime = DateTime.Now;
                    string editorName = Properties.Settings.Default.TesterName.ToString().TrimEnd();
                    string categoryName = txtcategory.Text.Trim();

                    //List<string> list = connect.ReadCategoryfromTreeTable();
                    //var cal = list.Find(s => s.Equals(categoryName, StringComparison.CurrentCultureIgnoreCase));

                    //if(cal == null || cal != string.Empty)
                    //{
                    //    categoryName = cal;
                    //}

                    if (!String.Equals(categoryName, QatConstants.TveDesignerOtherCatHeader, StringComparison.CurrentCultureIgnoreCase))
                    {
                        if (Summary != itemNewName || Category != categoryName)
                        {
                            string query = "Update " + label1.Text + " set Summary=@itemName,ModifiedOn=@editdate,Modifiedby=@editname,category=@categoryvalue where " + label1.Text + "ID='" + testID.Text + "'";
                            connect.InsertCommandWithParameter1(query, "@itemName", itemNewName, "@editdate", modifiedTime, "@editname", editorName, "@categoryvalue", categoryName);

                            isSaved = true;
                            Summary = itemNewName;
                            Modifiedon = modifiedTime;
                            Modifiedby = editorName;
                            Category = categoryName;
                        }

                        this.Close();
                    }
                    else
                    {
                        MessageBox.Show("Category name should not be an Other Category", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                }
                else
                {
                    MessageBox.Show("Please enter Tester name in the preferences", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }
            catch(Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                //MessageBox.Show("Exception\n " + ex.Message, "Error Code - ECxxxxx", MessageBoxButton.OK, MessageBoxImage.Error);
                this.Close();
            }

        }

        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            try
            {
                if (Regex.IsMatch(e.Text, @"[\\/:*?<>|""[\]&]"))
                {
                    e.Handled = true;
                }
                else
                {
                    e.Handled = false;
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

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
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

        private string CategorySelectedValue = null;
        public string CategorySelected
        {
            get { return CategorySelectedValue; }
            set { CategorySelectedValue = value; OnPropertyChanged("CategorySelected"); }
        }

        private List<string> AssociatedItemValue = new List<string>();
        public List<string> AssociatedItem
        {
            get { return AssociatedItemValue; }
            set
            {
                AssociatedItemValue = value;
                OnPropertyChanged("AssociatedItem");
            }
        }

        private bool _isSaved = false;
        public bool isSaved
        {
            get { return _isSaved; }
            set { _isSaved = value; OnPropertyChanged("isSaved"); }
        }

        private void Window_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            try
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
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                //MessageBox.Show("Exception\n " + ex.Message, "QAT Error Code - EC03021", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
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
                //MessageBox.Show("Exception\n " + ex.Message, "QAT Error Code - EC03021", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void QATprop_keyup(object sender, KeyEventArgs e)
        {
            Window target = (Window)sender;

            if (e.Key == Key.Escape)
            {
                target.Close();
            }

        }

        private void TextBox_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            var textBox = e.OriginalSource as TextBox;

            if (textBox != null && e.Key == Key.Tab)
            {
                textBox.SelectAll();
            }
        }
        private void Window_StateChanged(object sender, EventArgs e)
        {
            //RunnerSlider.Value = 0;
            if (this.WindowState == WindowState.Normal)
            {
                b.HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled;
               
            }
            else
            {
                b.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
               
            }
        }

        private void RunnerSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if(RunnerSlider.Value>0)
            {
                b.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
            }
            else
            {
                b.HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled;
            }
        }
    }
}
