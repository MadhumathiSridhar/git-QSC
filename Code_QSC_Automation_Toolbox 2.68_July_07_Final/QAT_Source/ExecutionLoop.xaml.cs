using System;
using System.Collections.Generic;
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
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Windows.Controls.Primitives;
using System.Data;
using System.Text.RegularExpressions;

namespace QSC_Test_Automation
{
    /// <summary>
    /// Interaction logic for ExecutionLoop.xaml
    /// </summary>
    public partial class ExecutionLoop : Window
    {
        #region

        /// <summary>
        /// Initial Value Decleration
        /// </summary>
        public List<string> lstTestSuiteName = new List<string>();
        public List<string> LoopingOption = new List<string>();
        public List<string> lstDuration = new List<string>();
        public List<string> DurationType = new List<string>();
        public List<bool> RedeployedDesign = new List<bool>();

        private string lstrTestSuiteName = string.Empty;
        private string lstrTypeOfLooping = string.Empty;
        private string lstrDuration = string.Empty;
        private string lstrDurationType = string.Empty;
        private bool lboolRedeployedDesign = false;

        private ObservableCollection<GetExecutionLoop> Value = new ObservableCollection<GetExecutionLoop>();
        public ObservableCollection<GetExecutionLoop> lcol = null;

        TreeViewExplorer t = null;

        private int TypeOfLoopOption = 0;
        private int txtDurCmbIndex = 0;
        #endregion


        /// <summary>
        /// constructor Pasing the selected TestSuite names
        /// </summary>
        /// <param name="treeViewExplorerInventoryList">Contains TestSuite Names</param>
        public ExecutionLoop(TreeViewExplorer treeViewExplorerInventoryList)
        {
            InitializeComponent();
            t = treeViewExplorerInventoryList;
            try
            {
                if (t.DataGridCollection != null&& t.DataGridCollection.Count>0)
                {
                    lcol = t.DataGridCollection;
                    LoopExecution_dataGrid.Items.Clear();

                    if (lcol != null && lcol.Count > 0)
                    {
                        DeviceDiscovery.WriteToLogFile("Loop Execution TestSuite Count: " + lcol.Count);
                        foreach (TreeViewExplorer SuiteName in treeViewExplorerInventoryList.Children)
                        {
                            foreach (var item in lcol)
                            {
                                if (item.TestSuiteName == SuiteName.ItemName)
                                {
                                    if(lstrTestSuiteName != item.TestSuiteName)
                                    {
                                        if(SuiteName.ItemName==item.TestSuiteName)
                                        {
                                            if (!lstTestSuiteName.Contains(SuiteName.ItemName))
                                            {
                                                DeviceDiscovery.WriteToLogFile("TestSuite Name: " + SuiteName.ItemName);
                                                DeviceDiscovery.WriteToLogFile("Type Of Loop Selected: " + item.TypeOfLoopOption);
                                                DeviceDiscovery.WriteToLogFile("Value set for execution: " + item.NumOfLoop);
                                                DeviceDiscovery.WriteToLogFile("Duration Type: " + item.txtDurCmbSelectedValue);
                                                DeviceDiscovery.WriteToLogFile("Redeployed design: " + item.blnRedeployedDesign);
                                                //lstTestSuiteName.Add(item.TestSuiteName);
                                                lstTestSuiteName.Add(SuiteName.ItemName);
                                                LoopingOption.Add(item.TypeOfLoopOption);
                                                lstDuration.Add(item.NumOfLoop);
                                                DurationType.Add(item.txtDurCmbSelectedValue);
                                                RedeployedDesign.Add(item.blnRedeployedDesign);
                                                break;
                                            }
                                        }
                                    }                                   
                                }
                            }
                        }
                        foreach (TreeViewExplorer SuiteName in treeViewExplorerInventoryList.Children)
                        {
                            if (!lstTestSuiteName.Contains(SuiteName.ItemName))
                            {
                                DeviceDiscovery.WriteToLogFile("Test Suite Name: " + SuiteName.ItemName);
                                lstTestSuiteName.Add(SuiteName.ItemName);
                                LoopingOption.Add("");
                                lstDuration.Add("");
                                DurationType.Add("Hour");
                                RedeployedDesign.Add(true);
                            }
                        }
                        for (int j = 0; j < lstTestSuiteName.Count; j++)
                        {
                            LoopExecution_dataGrid.Items.Add(new GetExecutionLoop() { TestSuiteName = lstTestSuiteName[j], TypeOfLoopOption = LoopingOption[j], NumOfLoop = lstDuration[j], txtDurCmbSelectedValue = DurationType[j], blnRedeployedDesign = RedeployedDesign[j] });
                        }
                    }
                }
                else
                {
                    DeviceDiscovery.WriteToLogFile("Execution TestSuite Count: " + treeViewExplorerInventoryList.Children.Count);
                    foreach (TreeViewExplorer SuiteName in treeViewExplorerInventoryList.Children)
                    {
                        if (!lstTestSuiteName.Contains(SuiteName.ItemName))
                        {
                            DeviceDiscovery.WriteToLogFile("TestSuite Name: " + SuiteName.ItemName);
                            lstTestSuiteName.Add(SuiteName.ItemName);
                        }
                    }

                    for (int i = 0; i < lstTestSuiteName.Count; i++)
                    {
                        LoopExecution_dataGrid.Items.Add(new GetExecutionLoop() { TestSuiteName = lstTestSuiteName[i] });
                    }
                }  
                
                if(t.lastremvalues==true)
                {
                    LastExecValues.IsChecked = true;
                }
                else
                {
                    LastExecValues.IsChecked = false;
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


        /// <summary>
        /// Ok Button Click Stores Selected Values into collection
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (t.DataGridCollection == null)
                {
                    t.DataGridCollection = new ObservableCollection<GetExecutionLoop>();
                }

                bool ValueCheck = false;             
                
                if (Value != null && Value.Count > 0)
                {
                    foreach (var item in Value)
                    {
                        if (item.TestSuiteName != string.Empty)
                        {
                            if (item.TypeOfLoopOption == "Number Of Times")
                            {
                                if (item.NumOfLoop == string.Empty)
                                {
                                    DeviceDiscovery.WriteToLogFile("Number Of Times values are empty: " + item.NumOfLoop);
                                    ValueCheck = true;
                                    break;
                                }
                                else
                                {                                   
                                    if(item.NumOfLoop.Length>4 )
                                    {
                                        MessageBox.Show("Only 4 digits allowed", "Information", MessageBoxButton.OK, MessageBoxImage.Warning);
                                        return;
                                    }
                                    if (Convert.ToInt16(item.NumOfLoop) == 0)
                                    {
                                        MessageBox.Show("Invalid digit", "Information", MessageBoxButton.OK, MessageBoxImage.Warning);
                                        return;
                                    }
                                }
                            }
                            if (item.TypeOfLoopOption == "Duration")
                            {
                                if (item.NumOfLoop == string.Empty)
                                {
                                    DeviceDiscovery.WriteToLogFile("duration values are empty: " + item.NumOfLoop);
                                    ValueCheck = true;
                                    break;
                                }
                                else
                                {
                                    if (item.NumOfLoop.Length > 4)
                                    {
                                        MessageBox.Show("Only 4 digits allowed", "Information", MessageBoxButton.OK, MessageBoxImage.Warning);
                                        return;
                                    }
                                    if (Convert.ToInt16(item.NumOfLoop) == 0)
                                    {
                                        MessageBox.Show("Invalid digit", "Information", MessageBoxButton.OK, MessageBoxImage.Warning);
                                        return;
                                    }
                                }
                            }
                            if (item.TypeOfLoopOption == string.Empty)
                            {
                                item.NumOfLoop = string.Empty;
                            }
                        }
                    }

                    if (ValueCheck == false)
                    {
                        t.DataGridCollection = Value;
                        this.Close();
                    }
                    else
                    {
                        MessageBox.Show("Few Fields are empty", "Information", MessageBoxButton.OK, MessageBoxImage.Information);                       
                        t.DataGridCollection = null;                                             
                    }

                }

                if(LastExecValues.IsChecked== false)
                {
                    t.DataGridCollection = null;
                    t.lastremvalues = false;
                }
                else
                {
                    t.lastremvalues = true;
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

     
        /// <summary>
        /// Type Of Loop Selection Changed Event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cmb_LoopingOption_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                GetExecutionLoop originalDeviceItem = null;
                ComboBox selectedComboBox = sender as ComboBox;
                if (selectedComboBox != null && String.Equals(selectedComboBox.DataContext.GetType().ToString(), "QSC_Test_Automation.GetExecutionLoop"))
                    originalDeviceItem = (GetExecutionLoop)selectedComboBox.DataContext;

                if (originalDeviceItem == null)
                    return;

                lstrTestSuiteName = originalDeviceItem.TestSuiteName;

                TypeOfLoopOption = originalDeviceItem.TypeOfLoopOptionIndex;
                if (TypeOfLoopOption == 0)
                {
                    lstrTypeOfLooping = originalDeviceItem.TypeOfLoopOption;
                    lboolRedeployedDesign = originalDeviceItem.blnRedeployedDesign;
                    lstrDurationType = originalDeviceItem.txtDurCmbSelectedValue;
                    ComboBoxValuesStoring();
                }
                if (TypeOfLoopOption == 1)
                {
                    lstrTypeOfLooping = originalDeviceItem.TypeOfLoopOption;
                    lstrDuration = string.Empty;
                    lboolRedeployedDesign = originalDeviceItem.blnRedeployedDesign;
                    lstrDurationType = originalDeviceItem.txtDurCmbSelectedValue;
                    ComboBoxValuesStoring();
                }
                if (TypeOfLoopOption == 2)
                {
                    lstrTypeOfLooping = originalDeviceItem.TypeOfLoopOption;
                    lstrDuration = string.Empty;
                    lstrDurationType = originalDeviceItem.txtDurCmbSelectedValue;
                    lboolRedeployedDesign = originalDeviceItem.blnRedeployedDesign;
                    ComboBoxValuesStoring();
                }
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                //MessageBox.Show("Exception\n " + ex.Message, "QAT Error Code - ECxxxxx", MessageBoxButton.OK, MessageBoxImage.Error);
            }            
        }       


        /// <summary>
        /// Duration Combobox selection changed Event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cmbDur_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                GetExecutionLoop originalDeviceItem = null;

                ComboBox selectedComboBox = sender as ComboBox;
                if (selectedComboBox != null && String.Equals(selectedComboBox.DataContext.GetType().ToString(), "QSC_Test_Automation.GetExecutionLoop"))
                    originalDeviceItem = (GetExecutionLoop)selectedComboBox.DataContext;

                if (originalDeviceItem == null)
                    return;
                lstrTypeOfLooping = originalDeviceItem.TypeOfLoopOption;
                lstrTestSuiteName = originalDeviceItem.TestSuiteName;
                lboolRedeployedDesign = originalDeviceItem.blnRedeployedDesign;
                if (lstrTypeOfLooping == "No Loop")
                {
                    lstrDuration = originalDeviceItem.NumOfLoop;
                    lstrDurationType = originalDeviceItem.txtDurCmbSelectedValue;
                    lboolRedeployedDesign = originalDeviceItem.blnRedeployedDesign;
                    ValuesStoring();
                }
                if (lstrTypeOfLooping == "Number Of Times")
                {
                    lstrDuration = originalDeviceItem.NumOfLoop;
                    lstrDurationType = originalDeviceItem.txtDurCmbSelectedValue;
                    lboolRedeployedDesign = originalDeviceItem.blnRedeployedDesign;
                    ValuesStoring();
                }
                if (lstrTypeOfLooping == "Duration")
                {
                    lstrDuration = originalDeviceItem.NumOfLoop;
                    txtDurCmbIndex = originalDeviceItem.txtDurCmb;
                    lstrDurationType = originalDeviceItem.txtDurCmbSelectedValue;
                    lboolRedeployedDesign = originalDeviceItem.blnRedeployedDesign;
                    ValuesStoring();
                }
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                //MessageBox.Show("Exception\n " + ex.Message, "QAT Error Code - ECxxxxx", MessageBoxButton.OK, MessageBoxImage.Error);
            }          

        }


        /// <summary>
        /// TextBox Values are getting stored in collections
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtDur_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                GetExecutionLoop originalDeviceItem = null;
                TextBox selectedTextBox = sender as TextBox;
                if (selectedTextBox != null && String.Equals(selectedTextBox.DataContext.GetType().ToString(), "QSC_Test_Automation.GetExecutionLoop"))
                    originalDeviceItem = (GetExecutionLoop)selectedTextBox.DataContext;

                if (originalDeviceItem == null)
                    return;

                string lTypeOfLooping = originalDeviceItem.TypeOfLoopOption;

                lstrTestSuiteName = originalDeviceItem.TestSuiteName;
                lboolRedeployedDesign = originalDeviceItem.blnRedeployedDesign;
                if (lTypeOfLooping == "Number Of Times")
                {
                    lstrDuration = originalDeviceItem.NumOfLoop;
                    lstrDurationType = originalDeviceItem.txtDurCmbSelectedValue;
                    ValueStoresforTextBox(lTypeOfLooping);
                }
                if (lTypeOfLooping == "Duration")
                {
                    lstrDuration = originalDeviceItem.NumOfLoop;
                    lstrDurationType = originalDeviceItem.txtDurCmbSelectedValue;
                    ValueStoresforTextBox(lTypeOfLooping);
                }
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                //MessageBox.Show("Exception\n " + ex.Message, "QAT Error Code - ECxxxxx", MessageBoxButton.OK, MessageBoxImage.Error);
            }                   
        }


        /// <summary>
        /// Value entered in TextBox stored in collection
        /// </summary>
        /// <param name="lTypeOfLooping">Type of Loop Selected in ComboBox</param>
        private void ValueStoresforTextBox(string lTypeOfLooping)
        {
            try
            {
                string TestSuiteName = string.Empty;
                if (lstrTypeOfLooping != null || lstrDuration != string.Empty)
                {
                    for (int row = 0; row <= LoopExecution_dataGrid.Items.Count - 1; row++)
                    {
                        DataGridRow rows = this.LoopExecution_dataGrid.ItemContainerGenerator.ContainerFromIndex(row) as DataGridRow;
                        DataGridCell exid = DataGridHelper.GetCell(LoopExecution_dataGrid, row, 0);
                        TestSuiteName = ((TextBlock)exid.Content).Text.ToString();

                        if (TestSuiteName == lstrTestSuiteName)
                        {
                            foreach (var item in Value)
                            {
                                if (item.TestSuiteName == lstrTestSuiteName)
                                {
                                    Value.Remove(item);
                                    break;
                                }
                            }
                            Value.Add(new GetExecutionLoop() { TestSuiteName = lstrTestSuiteName, TypeOfLoopOption = lTypeOfLooping, NumOfLoop = lstrDuration, txtDurCmbSelectedValue = lstrDurationType, blnRedeployedDesign = lboolRedeployedDesign });
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
                //MessageBox.Show("Exception\n " + ex.Message, "QAT Error Code - ECxxxxx", MessageBoxButton.OK, MessageBoxImage.Error);
            }            
        }


        /// <summary>
        /// Redeployment Design Values stores in Collection
        /// </summary>
        private void ValuesStoring()
        {
            try
            {
                string TestSuiteName = string.Empty;
                if (lstrTypeOfLooping != null || lstrDuration != string.Empty)
                {
                    for (int row = 0; row <= LoopExecution_dataGrid.Items.Count - 1; row++)
                    {
                        DataGridRow rows = this.LoopExecution_dataGrid.ItemContainerGenerator.ContainerFromIndex(row) as DataGridRow;

                        DataGridCell exid = DataGridHelper.GetCell(LoopExecution_dataGrid, row, 0);
                        TestSuiteName = ((TextBlock)exid.Content).Text.ToString();

                        if (TestSuiteName == lstrTestSuiteName)
                        {
                            foreach (var item in Value)
                            {
                                if (item.TestSuiteName == lstrTestSuiteName)
                                {
                                    Value.Remove(item);
                                    break;
                                }
                            }
                            Value.Add(new GetExecutionLoop() { TestSuiteName = lstrTestSuiteName, TypeOfLoopOption = lstrTypeOfLooping, NumOfLoop = lstrDuration, txtDurCmbSelectedValue = lstrDurationType, blnRedeployedDesign = lboolRedeployedDesign });
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
                //MessageBox.Show("Exception\n " + ex.Message, "QAT Error Code - ECxxxxx", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            
        }


        /// <summary>
        /// Funtion stores values in collections based on combobox selection
        /// </summary>
        private void ComboBoxValuesStoring()
        {
            try
            {
                string TestSuiteName = string.Empty;

                if (lstrTypeOfLooping != null)
                {
                    for (int row = 0; row <= LoopExecution_dataGrid.Items.Count - 1; row++)
                    {
                        DataGridRow rows = this.LoopExecution_dataGrid.ItemContainerGenerator.ContainerFromIndex(row) as DataGridRow;

                        DataGridCell exid = DataGridHelper.GetCell(LoopExecution_dataGrid, row, 0);
                        TestSuiteName = ((TextBlock)exid.Content).Text.ToString();

                        if (TestSuiteName == lstrTestSuiteName)
                        {
                            foreach (var item in Value)
                            {
                                if (item.TestSuiteName == lstrTestSuiteName)
                                {
                                    Value.Remove(item);
                                    break;
                                }
                            }
                            Value.Add(new GetExecutionLoop() { TestSuiteName = lstrTestSuiteName, TypeOfLoopOption = lstrTypeOfLooping, NumOfLoop = lstrDuration, txtDurCmbSelectedValue = lstrDurationType, blnRedeployedDesign = lboolRedeployedDesign });
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
                //MessageBox.Show("Exception\n " + ex.Message, "QAT Error Code - ECxxxxx", MessageBoxButton.OK, MessageBoxImage.Error);
            }
           
        }


        /// <summary>
        /// Validating the Text Input allow only numeric
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtDur_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            try
            {
                e.Handled = !IsTextAllowed(e.Text);
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                //MessageBox.Show("Exception\n " + ex.Message, "QAT Error Code - EC12018", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        /// <summary>
        /// Validating the Text Input allow only numeric
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtDur_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Handled == IsTextAllowed(e.ToString()))
                {
                    if (e.Key == Key.Space)
                    {
                        e.Handled = true;
                    }
                    base.OnPreviewKeyDown(e);
                }

                // Copy and Paste
                string lstrCopyandPasteTxtBox = null;
                if (e.Key == Key.V && Keyboard.Modifiers == ModifierKeys.Control)
                {
                    e.Handled = false;
                    lstrCopyandPasteTxtBox = Clipboard.GetText();
                    if (e.Handled == IsTextAllowed(lstrCopyandPasteTxtBox))
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
                //MessageBox.Show("Exception\n " + ex.Message, "QAT Error Code - EC12112", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        /// <summary>
        /// IsTextAllowed validates the input values
        /// </summary>
        /// <param name="text">Values entered in TextBox</param>
        /// <returns></returns>
        private static bool IsTextAllowed(string text)
        {
            Regex regex = null;

            try
            {
                regex = new Regex("[^0-9]+"); ///regex that matches disallowed text
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                //MessageBox.Show("Exception\n " + ex.Message, "QAT Error Code - EC12019", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return !regex.IsMatch(text);
        }


        /// <summary>
        /// Redeployment CheckBox checked status
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                GetExecutionLoop originalDeviceItem = null;
                CheckBox selectedComboBox = sender as CheckBox;
                if (selectedComboBox != null && String.Equals(selectedComboBox.DataContext.GetType().ToString(), "QSC_Test_Automation.GetExecutionLoop"))
                    originalDeviceItem = (GetExecutionLoop)selectedComboBox.DataContext;

                if (originalDeviceItem == null)
                    return;
                lstrTestSuiteName = originalDeviceItem.TestSuiteName;
                lstrTypeOfLooping = originalDeviceItem.TypeOfLoopOption;
                lstrDuration = originalDeviceItem.NumOfLoop;
                lstrDurationType = originalDeviceItem.txtDurCmbSelectedValue;
                lboolRedeployedDesign = originalDeviceItem.blnRedeployedDesign;

                ValuesStoring();
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


        /// <summary>
        /// Redeployment UncheckBox checked status
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                GetExecutionLoop originalDeviceItem = null;
                CheckBox selectedComboBox = sender as CheckBox;
                if (selectedComboBox != null && String.Equals(selectedComboBox.DataContext.GetType().ToString(), "QSC_Test_Automation.GetExecutionLoop"))
                    originalDeviceItem = (GetExecutionLoop)selectedComboBox.DataContext;

                if (originalDeviceItem == null)
                    return;
                lstrTestSuiteName = originalDeviceItem.TestSuiteName;
                lstrTypeOfLooping = originalDeviceItem.TypeOfLoopOption;
                lstrDuration = originalDeviceItem.NumOfLoop;
                lstrDurationType = originalDeviceItem.txtDurCmbSelectedValue;
                lboolRedeployedDesign = originalDeviceItem.blnRedeployedDesign;
                ValuesStoring();
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
    }

    #region "DataGridHelper"

    static class DataGridHelper
    {
        static public DataGridCell GetCell(DataGrid dg, int row, int column)
        {
            try
            {
                DataGridRow rowContainer = GetRow(dg, row);

                if (rowContainer != null)
                {
                    DataGridCellsPresenter presenter = GetVisualChild<DataGridCellsPresenter>(rowContainer);

                    // try to get the cell but it may possibly be virtualized
                    DataGridCell cell = (DataGridCell)presenter.ItemContainerGenerator.ContainerFromIndex(column);
                    if (cell == null)
                    {
                        // now try to bring into view and retreive the cell
                        dg.ScrollIntoView(rowContainer, dg.Columns[column]);
                        cell = (DataGridCell)presenter.ItemContainerGenerator.ContainerFromIndex(column);
                    }

                    return cell;
                }
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                //MessageBox.Show("Exception\n " + ex.Message, "QAT Error Code - EC03012", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return null;
        }

        static public DataGridRow GetRow(DataGrid dg, int index)
        {
            DataGridRow row = (DataGridRow)dg.ItemContainerGenerator.ContainerFromIndex(index);
            try
            {
                if (row == null)
                {
                    // may be virtualized, bring into view and try again
                    dg.ScrollIntoView(dg.Items[index]);
                    row = (DataGridRow)dg.ItemContainerGenerator.ContainerFromIndex(index);
                }
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                //MessageBox.Show("Exception\n " + ex.Message, "QAT Error Code - EC03013", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return row;
        }

        static T GetVisualChild<T>(Visual parent) where T : Visual
        {
            T child = default(T);
            try
            {
                int numVisuals = VisualTreeHelper.GetChildrenCount(parent);
                for (int i = 0; i < numVisuals; i++)
                {
                    Visual v = (Visual)VisualTreeHelper.GetChild(parent, i);
                    child = v as T;
                    if (child == null)
                    {
                        child = GetVisualChild<T>(v);
                    }
                    if (child != null)
                    {
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                //MessageBox.Show("Exception\n " + ex.Message, "QAT Error Code - EC03014", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return child;
        }

        public static T FindChild<T>(DependencyObject parent, string childName) where T : DependencyObject
        {
            if (parent == null) return null;

            T foundChild = null;

            try
            {
                int childrenCount = VisualTreeHelper.GetChildrenCount(parent);
                for (int i = 0; i < childrenCount; i++)
                {
                    var child = VisualTreeHelper.GetChild(parent, i);
                    T childType = child as T;
                    if (childType == null)
                    {
                        foundChild = FindChild<T>(child, childName);
                        if (foundChild != null) break;
                    }
                    else if (!string.IsNullOrEmpty(childName))
                    {
                        var frameworkElement = child as FrameworkElement;
                        if (frameworkElement != null && frameworkElement.Name == childName)
                        {
                            foundChild = (T)child;
                            break;
                        }
                    }
                    else
                    {
                        foundChild = (T)child;
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                //MessageBox.Show("Exception\n " + ex.Message, "QAT Error Code - EC03015", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return foundChild;
        }

    }
    #endregion
}
