using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SqlClient;
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
    /// Interaction logic for ContinousBackgroundMonitoring.xaml
    /// </summary>
    public partial class ContinousBackgroundMonitoring : Window
    {
        private string mstrInventoryMonitor = string.Empty;
        private string mstrLogMonitor = string.Empty;
        private string mstrTelnetMonitor = string.Empty;
        private string mstrControlValueMonitor = string.Empty;
        private DataTable datatable1 = new DataTable();
        private SqlDataAdapter adap = null;
        private DataTableReader read = null;
        private DataTable ReadDT = new DataTable();
        private DataTable ReadDT1 = new DataTable();
        private DataTable ReadDT2 = new DataTable();
        private DataTable tble = new DataTable();
        private Int32 desgnid = 0;
        private string CVMComponentType = string.Empty;
        private string CVMComponentName = string.Empty;
        private string CVMComponentProperty = string.Empty;
        private string CVMComponentChannel = string.Empty;
        private string CVMComponentValue = string.Empty;
        private DBConnection connect = new DBConnection();      
        DBConnection QscDatabase = new DBConnection();
        List<DUT_DeviceItem> dut = new List<DUT_DeviceItem>();


        private CBMItems cbmitemsvalue  = null;
        private TestSuiteItem sourceTestSuiteItem = null;
        private TestPlanItem sourceTestPlanItem = null;
        private bool clearValue = false;
        private bool novalue = false;
        public ContinousBackgroundMonitoring(TestSuiteItem parentTestSuiteItem, TestPlanItem parentTestPlanItem)
        {
            try
            {
                sourceTestSuiteItem = parentTestSuiteItem;
                sourceTestPlanItem = parentTestPlanItem;
                if (sourceTestSuiteItem != null)
                    cbmitemsvalue = sourceTestSuiteItem.BackgroundMonitoring;
                else if (sourceTestPlanItem != null)
                    cbmitemsvalue = sourceTestPlanItem.BackgroundMonitoring;

                cbmitemsvalue.TelnetDutDeviceItem = new ObservableCollection<DUT_DeviceItem>();
                cbmitemsvalue.TelnetDeviceItem = new ObservableCollection<string>();


                InitializeComponent();
                //Inventory_grpbx.IsEnabled = false;
                //Log_grpbx.IsEnabled = false;
                Telnet_grpbx.IsEnabled = false;
                Control_grpbx.IsEnabled = false;
                telnetverifydata.IsEnabled = false;
                ErrorHandlingdata.IsEnabled = false;
                LogErrorHandling.IsEnabled = false;
                CVMErrorHandlingdata.IsEnabled = false;
                btn_SelectActionPlus.Visibility = Visibility.Hidden;
                btn_SelectVerificationPlus.Visibility = Visibility.Hidden;

                if (sourceTestSuiteItem != null)
                {
                    grb_ControlValueMonitoring.Visibility = Visibility.Hidden;
                    grb_ControlValueMonitoring.IsEnabled = false;
                    CVM.Visibility = Visibility.Collapsed;
                    InventoryMonitor.Visibility = Visibility.Collapsed;
                    LogMonitor.Visibility = Visibility.Collapsed;
                }
                else
                {
                    CVM.Visibility = Visibility.Visible;
                    InventoryMonitor.Visibility = Visibility.Visible;
                    LogMonitor.Visibility = Visibility.Visible;
                }

                novalue = false;
                ReadDataFromTable();
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
           
        private void Telnet_Checked(object sender, RoutedEventArgs e)
        {
            try
            {                
                if (cbmitemsvalue.CompareValues==true)
                {
                    cbmitemsvalue.FailureCriteriaEnable = true;
                }

                if(cbmitemsvalue.ContinueTesting==true)
                {
                    cbmitemsvalue.ReRunFailedTestCaseEnabled = true;
                }
               
              
                string testplannamefrom = string.Empty;
                mstrTelnetMonitor = "Telnet Monitor";
                Telnet_grpbx.IsEnabled = true;
                telnetverifydata.IsEnabled = true;
                ErrorHandlingdata.IsEnabled = true;
                AddDutDevices();
                TelnetMonitor.IsChecked = true;
                btn_SelectActionPlus.Visibility = Visibility.Visible;
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

        private CBMTestTelnetItem AddSetTestTelnetItem()
        {
            CBMTestTelnetItem setTestTelnetItem = new CBMTestTelnetItem();

            try
            {
                ObservableCollection<DUT_DeviceItem> telnetDeviceList = new ObservableCollection<DUT_DeviceItem>();
                foreach (var item in cbmitemsvalue.TelnetDutDeviceItem)
                {
                    telnetDeviceList.Add(new DUT_DeviceItem(item));
                    if (!setTestTelnetItem.CBMCombo.Contains("All devices"))
                    {
                        setTestTelnetItem.CBMCombo.Add("All devices");
                        if (!setTestTelnetItem.CBMCombo.Contains("Video Gen1-PGAVHD"))
                        setTestTelnetItem.CBMCombo.Add("Video Gen1-PGAVHD");
                    }
                    if (!setTestTelnetItem.CBMCombo.Contains(item.ItemDeviceName))
                        setTestTelnetItem.CBMCombo.Add(item.ItemDeviceName);
                    if (!setTestTelnetItem.CBMComboModel.Keys.Contains(item.ItemDeviceName))
                        setTestTelnetItem.CBMComboModel.Add(item.ItemDeviceName, item.ItemDeviceModel);
                }

                setTestTelnetItem.CBM = telnetDeviceList;
                cbmitemsvalue.SetTestTelnetList.Add(setTestTelnetItem);
                return setTestTelnetItem;
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif

                //MessageBox.Show("Exception\n " + ex.Message, "Error Code - ECxxxxx", MessageBoxButton.OK, MessageBoxImage.Error);
                return setTestTelnetItem;
            }
        }

        private void Control_Value_Monitoring_Checked(object sender, RoutedEventArgs e)
        {
            bool TpSaveEnable = false;
            if (sourceTestPlanItem.SaveButtonIsEnabled==true)
                TpSaveEnable = true;

            Control_grpbx.IsEnabled = true;
            CVMErrorHandlingdata.IsEnabled = true;
             btn_SelectVerificationPlus.Visibility = Visibility.Visible;
            try
            {
                VerifyControlContentDataTemplareData.DataContext = cbmitemsvalue;
                CVMErrorHandlingdata.DataContext = cbmitemsvalue;
                DataTableReader read1 = null;
                mstrControlValueMonitor = "Control Value Monitoring";
                Control_grpbx.IsEnabled = true;

                if(cbmitemsvalue.CVMContinueTesting==true)
                {                   
                    cbmitemsvalue.CVMReRunFailedTestCaseEnabled = true;
                }
                
                if (sourceTestPlanItem != null)
                {
                    Int32 tpid = 0;
                    string query = "select testplanid from Testplan where testplanname=@TPName";
                    tble = QscDatabase.SelectDTWithParameter(query, "@TPName", sourceTestPlanItem.TestItemName);
                    //tble = QscDatabase.SendCommand_Toreceive(query);
                    read1 = tble.CreateDataReader();
                    while (read1.Read())
                    {
                        tpid = read1.GetInt32(0);
                    }

                    tble.Clear();

                    query = "select DesignID from TPDesignLinktable where TPID=('" + tpid + "')";
                    tble = QscDatabase.SendCommand_Toreceive(query);
                    read1 = tble.CreateDataReader();

                    while (read1.Read())
                    {
                        desgnid = read1.GetInt32(0);
                    }

                    query = "select ComponentType from TCInitialization where designid=('" + desgnid + "')";
                    DataTableReader read2 = null;
                    tble = QscDatabase.SendCommand_Toreceive(query);
                    read2 = tble.CreateDataReader();
                    List<string> componenttype = new List<string>();
                    cbmitemsvalue.cmb_ComponentType.Clear();
                    while (read2.Read())
                    {
                        componenttype.Add(read2.GetString(0));
                    }

                    foreach (string s in componenttype)
                    {
                        if (!cbmitemsvalue.cmb_ComponentType.Contains(s))
                        {
                            cbmitemsvalue.cmb_ComponentType.Add(s);
                            cbmitemsvalue.ComponentTypeList.Add(s);
                        }
                    }
                   

                    GetDesignComponentDetails();
                }

                if(TpSaveEnable==false)
                sourceTestPlanItem.SaveButtonIsEnabled = false;
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

      
        private void Log_Unchecked(object sender, RoutedEventArgs e)
        {
            //Log_grpbx.IsEnabled = false;
        }

        private void Telnet_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                mstrTelnetMonitor = string.Empty;
                Telnet_grpbx.IsEnabled = false;
                telnetverifydata.IsEnabled = false;
                ErrorHandlingdata.IsEnabled = false;
                btn_SelectActionPlus.Visibility = Visibility.Hidden;
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

        private void Control_Value_Monitoring_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                mstrControlValueMonitor = string.Empty;
                Control_grpbx.IsEnabled = false;
                CVMErrorHandlingdata.IsEnabled = false;
                btn_SelectVerificationPlus.Visibility = Visibility.Hidden;
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

        private void TelnetDevicesFetch_ContinousBackgroundMoniting(string TPname)
        {
            try
            {
                string query = "select * from DesignInventory where DesignID in(select DesignID from TPDesignLinkTable where TPID in(select TestPlanID from Testplan where Testplanname in (@TPName)))";
                DataTable tble = QscDatabase.SelectDTWithParameter(query, "@TPName", TPname);

                //DataTable tble = QscDatabase.SendCommand_Toreceive(query);
                DataTableReader read = tble.CreateDataReader();
                List<string> Telnet_deviceType = new List<string>();
                List<string> Telnet_deviceModel = new List<string>();
                List<string> Telnet_deviceNameInDesign = new List<string>();
                
                DUT_DeviceItem d = null;
                //dut=new List<DUT_DeviceItem>();
                while (read.Read())
                {
                    if(read[2].ToString().StartsWith("TSC-7"))
                    {
                        string deviceModel = read[2].ToString();
                        int index = deviceModel.IndexOf("7");
                        if (index > 0)
                            deviceModel = deviceModel.Substring(0, index + 1);
                        Telnet_deviceModel.Add(deviceModel);
                    }
                    else
                    {
                        Telnet_deviceModel.Add(read[2].ToString());
                    }
                    
                    Telnet_deviceNameInDesign.Add(read[3].ToString());
                 
                }

                string deviceCompare = DeviceDiscovery.Netpair_devicesSupported;//added for dynamic paring filter
                List<string> Telnet_deviceNameInDesignFiltered = new List<string>();//added for dynamic paring filter
                List<string> Telnet_deviceModelInDesignFiltered = new List<string>();//added for dynamic paring filter

                List<string> existing_devices = new List<string>();

                for (int i = 0; i < Telnet_deviceModel.Count; i++)//added for dynamic paring filter
                {
                    if (deviceCompare.Contains(Telnet_deviceModel[i]) | Telnet_deviceModel[i].Contains("Core"))//added for dynamic paring filter
                    {
                        Telnet_deviceNameInDesignFiltered.Add(Telnet_deviceNameInDesign[i]);//added for dynamic paring filter
                        Telnet_deviceModelInDesignFiltered.Add(Telnet_deviceModel[i]);
                    }
                }

                string[] Array_Telnet_deviceNameInDesign = Telnet_deviceNameInDesignFiltered.ToArray();//added for dynamic paring filter   
                string[] Array_Telnet_deviceModelInDesign = Telnet_deviceModelInDesignFiltered.ToArray();//added for dynamic paring filter   


                //  cbmitemsvalue.TelnetDutDeviceItem = new ObservableCollection<DUT_DeviceItem>();
                for (int k = 0; k < Array_Telnet_deviceNameInDesign.Length; k++)
                {
                   d= new DUT_DeviceItem();

                    if (!cbmitemsvalue.TelnetDeviceItem.Contains(Array_Telnet_deviceNameInDesign[k]))
                    {
                        cbmitemsvalue.TelnetDeviceItem.Add(Array_Telnet_deviceNameInDesign[k]);
                        d.ItemDeviceName = Array_Telnet_deviceNameInDesign[k].ToString();
                        d.ItemDeviceModel = Array_Telnet_deviceModelInDesign[k].ToString();

                        dut.Add(d);
                        cbmitemsvalue.TelnetDutDeviceItem.Add(d);
                    }
                    //telnetdevicesfind.UpdateLayout();                   
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

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (TelnetMonitor.IsChecked == true)
                {

                    string TelnetDevices = null;
                    string TelnetMonitorType = "Telnet Monitor";
                    string TelnetCommand = string.Empty;

                    //Telnet Verify

                    string Telnetverifytype = string.Empty;
                    if (cbmitemsvalue.ContinueWithoutdoinganything == true)
                    {
                        Telnetverifytype = "Continue Without Doing Anything";
                    }
                    if (cbmitemsvalue.StoreCurrentResult == true)
                    {
                        Telnetverifytype = "Store Current Result";
                    }
                    if (cbmitemsvalue.CompareValues == true)
                    {
                        Telnetverifytype = "Compare Values";
                    }

                    string comparetext = cbmitemsvalue.FailureCriteria;

                    //Error Handling

                    string TelnetErrorHandlingSelectedType = string.Empty;
                    string TelnettxtIteration = cbmitemsvalue.ReRunFailedTestCase;
                    string TelnettxtWaitTime = string.Empty;
                    string TelnetcmbSelectedItem = string.Empty;
                    string TelnetRadiobtncontinueTesting = string.Empty;
                    if (cbmitemsvalue.ContinueTesting == true)
                    {
                        TelnetErrorHandlingSelectedType = "Continue Testing";
                    }
                    else
                    {
                        TelnetErrorHandlingSelectedType = "pause";
                    }
                    //if (TelnetErrorHandlingCombo.SelectedItem != null)
                    //{
                    //    //TelnetcmbSelectedItem = ((ComboBoxItem)TelnetErrorHandlingCombo.SelectedItem).Content.ToString();
                    //}  

                    //Background
                    string Frequency = string.Empty;
                    string inputs = string.Empty;
                    string cmbSelectedItemValue = string.Empty;
                    //if (TelnetBckComboBox.SelectedItem != null)
                    //{
                    //    cmbSelectedItemValue = ((ComboBoxItem)TelnetBckComboBox.SelectedItem).Content.ToString();
                    //}

                    string Telnetcheckedstatus = "True";
                    //Insert Telnet
                    string query = string.Empty;
                    //delete the rows
                    if (Telnetverifytype == "Compare Values" && (comparetext == null || comparetext.Trim() == string.Empty))
                    {
                        MessageBox.Show("Please enter Compare Values in Ssh/Telnet Verification\n", "QAT Error Message", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                    foreach (CBMTestTelnetItem testtelnetitem in cbmitemsvalue.SetTestTelnetList)
                    {
                       if(testtelnetitem.TelnetSelectedDevice==null || testtelnetitem.TelnetSelectedDevice==string.Empty)
                        { 
                            MessageBox.Show("Select device in Ssh/Telnet action \n", "QAT Error Message", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }

                        if (testtelnetitem.TelnetCommandText == null || testtelnetitem.TelnetCommandText.Trim() == string.Empty)
                        {
                            MessageBox.Show("Enter command in Ssh/Telnet action \n", "QAT Error Message", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }
                        else
                        {
                            testtelnetitem.TelnetCommandText=testtelnetitem.TelnetCommandText.Trim();
                        }

                    }
                    if (sourceTestSuiteItem != null)
                    {
                        query = "Insert into BackgroundMonitoring values('" + sourceTestSuiteItem.TestSuiteID + "','" + 0 + "','" + Frequency + "','" + inputs + "','" + cmbSelectedItemValue + "','" + TelnetErrorHandlingSelectedType + "','" + TelnettxtIteration + "','" + TelnettxtWaitTime + "','" + TelnetcmbSelectedItem + "','" + TelnetMonitorType + "')";

                        DeleteRowInTable(sourceTestSuiteItem.TestSuiteID.ToString(), "TSID", TelnetMonitorType);
                        if (cbmitemsvalue.SetTestTelnetList.Count > 0)
                        {
                            InsertintoTelnetTables(query, TelnetMonitorType, TelnetDevices, "TSID", sourceTestSuiteItem.TestSuiteID.ToString(), TelnetCommand, Telnetverifytype, comparetext, Telnetcheckedstatus);
                        }
                    }

                    if (sourceTestPlanItem != null)
                    {
                        query = "Insert into BackgroundMonitoring values('" + 0 + "','" + sourceTestPlanItem.TestPlanID + "','" + Frequency + "','" + inputs + "','" + cmbSelectedItemValue + "','" + TelnetErrorHandlingSelectedType + "','" + TelnettxtIteration + "','" + TelnettxtWaitTime + "','" + TelnetcmbSelectedItem + "','" + TelnetMonitorType + "')";

                        DeleteRowInTable(sourceTestPlanItem.TestPlanID.ToString(), "TPID", TelnetMonitorType);
                        if (cbmitemsvalue.SetTestTelnetList.Count > 0)
                        {
                            InsertintoTelnetTables(query, TelnetMonitorType, TelnetDevices, "TPID", sourceTestPlanItem.TestPlanID.ToString(), TelnetCommand, Telnetverifytype, comparetext, Telnetcheckedstatus);
                        }
                    }
                }
                else
                {
                    if (TelnetMonitor.IsChecked == false)
                    {
                        btn_SelectActionPlus.Visibility = Visibility.Hidden;
                        //cbmitemsvalue.ParentCBMTestTelnetItem.TelnetCommandText = string.Empty;
                        cbmitemsvalue.CompareValues = true;
                        cbmitemsvalue.ContinueTesting = true;
                        cbmitemsvalue.FailureCriteria = string.Empty;

                        //Insert Telnet
                        string query = string.Empty;
                        //delete the rows
                        if (sourceTestSuiteItem != null)
                        {
                            DeleteRowInTable(sourceTestSuiteItem.TestSuiteID.ToString(), "TSID", "Telnet Monitor");
                            query = "delete from TelenetMonitor where TSID = '" + sourceTestSuiteItem.TestSuiteID.ToString() + "'";
                        }
                        if (sourceTestPlanItem != null)
                        {
                            DeleteRowInTable(sourceTestPlanItem.TestPlanID.ToString(), "TPID", "Telnet Monitor");
                            query = "delete from TelenetMonitor where TPID = '" + sourceTestPlanItem.TestPlanID.ToString() + "'";
                        }
                        adap = new SqlDataAdapter(query, this.connect.CreateConnection());
                        this.connect.OpenConnection();
                        adap.Update(datatable1);
                        adap.Fill(datatable1);
                        this.connect.CloseConnection();
                    }
                }

                //Control_Value_Monitoring
                if (Control_Value_Monitoring.IsChecked == true)
                {
                    string ControlValueMonitoringcheckedstatus = "True";
                    string CVMMonitorType = "Control Value Monitoring";
                    //Error Handling
                    string CVMErrorHandlingSelectedType = string.Empty;
                    string CVMtxtIteration = cbmitemsvalue.CVMReRunFailedTestCase;
                    string CVMtxtWaitTime = string.Empty;
                    string CVMcmbSelectedItem = string.Empty;
                    bool CVMRadiobtncontinueTesting = cbmitemsvalue.CVMContinueTesting;
                    if (CVMRadiobtncontinueTesting == true)
                    {
                        CVMErrorHandlingSelectedType = "Continue Testing";
                    }
                    else
                    {
                        CVMErrorHandlingSelectedType = "pause";
                    }

                    //Background
                    string CVMFrequency = string.Empty;
                    string CVMinputs = string.Empty;
                    string CVMcmbSelectedItemValue = string.Empty;

                    //Insert the rows
                    string query = string.Empty;


                    //if (cbmitemsvalue.PropertyInitialValueSelectedItem == string.Empty)
                    //    novalue = true;
                    //else
                    //    novalue = false;

                    //if (novalue == false)
                    //{
                    //delete the rows        


                    foreach (CBMTestControlItem controlItem in cbmitemsvalue.VerifyTestControlList)
                    {
                        if (string.IsNullOrEmpty(controlItem.TestControlComponentNameSelectedItem))
                        {
                            MessageBox.Show("Select Verification Component Name in Action Tab \n", "QAT Error Message", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }
						
                        if (string.IsNullOrEmpty(controlItem.TestControlPropertySelectedItem))
                        {
                            MessageBox.Show("Select Verification Control Value in Action Tab  \n", "QAT Error Message", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }

                        if (((string.IsNullOrEmpty(controlItem.TestControlPropertyInitialValueSelectedItem))))// & (controlItem.TestControlComboValueSelectedItem == null)))
                        {
                            MessageBox.Show("Select Verification Property Value in Action Tab \n", "QAT Error Message", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }
                        if (controlItem.TestControlPropertyInitialValueSelectedItem != null)
                        {
                            bool valid = IsValid(this);
                            if (!valid)
                            {
                                MessageBox.Show("Please enter valid Verification Property Value in Action Tab \n", "QAT Error Message", MessageBoxButton.OK, MessageBoxImage.Error);
                                return;
                            }
                        }
						
						if (controlItem.ChannelEnabled && string.IsNullOrEmpty(controlItem.ChannelSelectionSelectedItem))
						{
                            MessageBox.Show("Select Verification Channel Value in Action Tab \n", "QAT Error Message", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }

                        if (controlItem.LoopIsChecked)
                        {
                            if (string.IsNullOrEmpty(controlItem.LoopStart) || string.IsNullOrEmpty(controlItem.LoopEnd) || string.IsNullOrEmpty(controlItem.LoopIncrement))
                            {
                                MessageBox.Show("Please enter LoopStart/ Loopend/ LoopIncrement value in Verification Property in Action Tab \n", "QAT Error Message", MessageBoxButton.OK, MessageBoxImage.Error);
                                return;
                            }

                            if ((!(string.IsNullOrEmpty(controlItem.LoopStart))) & (!(string.IsNullOrEmpty(controlItem.LoopEnd))))
                            {
                                Int32 loop_start = Convert.ToInt32(controlItem.LoopStart);
                                Int32 loop_end = Convert.ToInt32(controlItem.LoopEnd);
                                if (loop_start > loop_end)
                                {
                                    MessageBox.Show("Please enter Loopend value greater than Loopstart value in Verification Property in Action Tab \n", "QAT Error Message", MessageBoxButton.OK, MessageBoxImage.Error);
                                    return;
                                }
                            }

                            if (!(string.IsNullOrEmpty(controlItem.LoopEnd)) & !(string.IsNullOrEmpty(controlItem.LoopIncrement)))
                            {
                                Int32 loop_inc = Convert.ToInt32(controlItem.LoopIncrement);
                                Int32 loop_end = Convert.ToInt32(controlItem.LoopEnd);
                                if (loop_inc > loop_end)
                                {
                                    MessageBox.Show("Please enter LoopIncrement value smaller than Loopend value in Verification Property in Action Tab \n", "QAT Error Message", MessageBoxButton.OK, MessageBoxImage.Error);
                                    return;
                                }
                            }
                        }

                        if ((!(string.IsNullOrEmpty(controlItem.MaximumLimit))) & (!(string.IsNullOrEmpty(controlItem.MinimumLimit))))
                        {
                            double Maxlimit = Convert.ToDouble(controlItem.MaximumLimit);
                            double Minlimit = Convert.ToDouble(controlItem.MinimumLimit);
                            if (Maxlimit < Minlimit)
                            {
                                MessageBox.Show("Please enter Upper limit value greater than Lower limit value in Verification Property in Action Tab \n", "QAT Error Message", MessageBoxButton.OK, MessageBoxImage.Error);
                                return;
                            }
                        }
                    }
                    if (sourceTestPlanItem != null)
                    {
                        query = "Insert into BackgroundMonitoring values('" + 0 + "','" + sourceTestPlanItem.TestPlanID + "','" + CVMFrequency + "','" + CVMinputs + "','" + CVMcmbSelectedItemValue + "','" + CVMErrorHandlingSelectedType + "','" + CVMtxtIteration + "','" + CVMtxtWaitTime + "','" + CVMcmbSelectedItem + "','" + CVMMonitorType + "')";
                        DeleteRowInTable(sourceTestPlanItem.TestPlanID.ToString(), "TPID", CVMMonitorType);

                        if (cbmitemsvalue.VerifyTestControlList.Count > 0)
                        {
                            bool controlvalue = InsertintoTables(query, CVMMonitorType, null, "TPID", sourceTestPlanItem.TestPlanID.ToString(), CVMComponentType, CVMComponentName, CVMComponentProperty, cbmitemsvalue.cmb_ChannelSelectedItem, CVMComponentValue, ControlValueMonitoringcheckedstatus, cbmitemsvalue.InputSelectionComboSelectedItem, cbmitemsvalue.MaximumLimit, cbmitemsvalue.MinimumLimit, cbmitemsvalue.chk_LoopChecked, cbmitemsvalue.txtchk_LoopStart, cbmitemsvalue.txtchk_LoopEnd, cbmitemsvalue.txtchk_LoopIncreament, cbmitemsvalue.Allchannels);
                            if (!controlvalue)
                                return;
                        }
                    }
                    //}
                    else
                    {
                        MessageBox.Show("Values should not be empty", "QAT BM");
                    }
                }
                else
                {
                    if (Control_Value_Monitoring.IsChecked == false)
                    {
                        btn_SelectVerificationPlus.Visibility = Visibility.Hidden;
                        novalue = false;
                        cbmitemsvalue.cmb_ComponentTypeSelectedItem = null;
                        cbmitemsvalue.ComponentNameSelectedItem = null;
                        cbmitemsvalue.PropertySelectedItem = null;
                        cbmitemsvalue.cmb_ChannelSelectedItem = null;
                        cbmitemsvalue.InputSelectionComboSelectedItem = null;
                        cbmitemsvalue.PropertyInitialValueSelectedItem = string.Empty;
                        cbmitemsvalue.MaximumLimit = string.Empty;
                        cbmitemsvalue.MinimumLimit = string.Empty;
                        cbmitemsvalue.chk_LoopEnable = Visibility.Hidden;
                        cbmitemsvalue.txtchk_LoopStart = string.Empty;
                        cbmitemsvalue.txtchk_LoopEnd = string.Empty;
                        cbmitemsvalue.txtchk_LoopIncreament = string.Empty;
                        cbmitemsvalue.Allchannels = string.Empty;
                        cbmitemsvalue.CVMContinueTesting = true;
                        cbmitemsvalue.CVMReRunFailedTestCase = string.Empty;

                        if (sourceTestPlanItem != null)
                        {
                            //Delete the rows
                            string query = string.Empty;
                            DeleteRowInTable(sourceTestPlanItem.TestPlanID.ToString(), "TPID", "Control Value Monitoring");
                            query = "delete from ControlMonitor where TPID = '" + sourceTestPlanItem.TestPlanID.ToString() + "'";
                            adap = new SqlDataAdapter(query, this.connect.CreateConnection());
                            this.connect.OpenConnection();
                            adap.Update(datatable1);
                            adap.Fill(datatable1);
                            this.connect.CloseConnection();
                        }
                    }
                }

                if (InventoryMonitoring.IsChecked == true)
                {
                    string errorHandlingSelectedType = string.Empty;
                    if (cbmitemsvalue.InventoryContinueTesting == true)
                    {
                        errorHandlingSelectedType = "Continue Testing";
                    }
                    else
                    {
                        errorHandlingSelectedType = "pause";
                    }

                    string txtFailedTC = cbmitemsvalue.InventoryReRunFailedTestCase;
                    string txtWaitTime = string.Empty;
                    string cmbSelectedItem = string.Empty;
                    string MonitorType = "Inventory Monitoring";

                    if (sourceTestPlanItem != null)
                    {
                        string query = "Insert into BackgroundMonitoring values('" + 0 + "','" + sourceTestPlanItem.TestPlanID + "','" + string.Empty + "','" + string.Empty + "','" + string.Empty + "','" + errorHandlingSelectedType + "','" + txtFailedTC + "','" + txtWaitTime + "','" + cmbSelectedItem + "','" + MonitorType + "')";

                        DeleteRowInTable(sourceTestPlanItem.TestPlanID.ToString(), "TPID", MonitorType);
                        InsertintoTables(query, MonitorType, null, "TPID", sourceTestPlanItem.TestPlanID.ToString(), null, null, null, null, null, null, null, null, null, false, null, null, null, null);
                    }
                }
                else
                {
                    cbmitemsvalue.InventoryContinueTesting = true;
                    cbmitemsvalue.InventoryPauseatErrorstate = false;
                    cbmitemsvalue.InventoryReRunFailedTestCase = string.Empty;

                    string query = string.Empty;
                    //delete the rows
                    if (sourceTestPlanItem != null)
                    {
                        DeleteRowInTable(sourceTestPlanItem.TestPlanID.ToString(), "TPID", "Inventory Monitoring");
                        query = "delete from InventoryMonitor where TPID = '" + sourceTestPlanItem.TestPlanID.ToString() + "'";
                    }

                    if (query != string.Empty)
                    {
                        adap = new SqlDataAdapter(query, this.connect.CreateConnection());
                        this.connect.OpenConnection();
                        adap.Update(datatable1);
                        adap.Fill(datatable1);
                        this.connect.CloseConnection();
                    }
                }

                if (LogMonitoring.IsChecked == true)
                {
                    foreach (CBMTestLogItem testlogitem in cbmitemsvalue.VerifyTestLogList)
                    {
                        if (testlogitem.iLogIsChecked != true && testlogitem.ConfiguratorIsChecked != true && testlogitem.EventLogIsChecked != true && testlogitem.KernelLogIsChecked != true && testlogitem.QsysAppLogIsChecked != true && testlogitem.SIPLogIsChecked != true && testlogitem.SoftPhoneLogIsChecked != true && testlogitem.UCIViewerLogIsChecked != true && testlogitem.WindowsEventLogsIsChecked != true)
                        {
                            MessageBox.Show("Log Verification is empty, please select atleast one logs \n", "QAT Error Message", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }

                        if (testlogitem.iLogIsChecked == true && (testlogitem.iLog_selected_item == null || testlogitem.iLog_selected_item == string.Empty))
                        {
                            MessageBox.Show("Select iLog device \n", "QAT Error Message", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }

                        if (testlogitem.iLogIsChecked == true && (testlogitem.ilogtext == null || testlogitem.ilogtext == string.Empty))
                        {
                            MessageBox.Show("Select iLog command \n", "QAT Error Message", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }

                        if (testlogitem.KernelLogIsChecked == true && (testlogitem.kernalLog_selected_item == null || testlogitem.kernalLog_selected_item == string.Empty))
                        {
                            MessageBox.Show("Select KernalLog device \n", "QAT Error Message", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }

                        if (testlogitem.KernelLogIsChecked == true && (testlogitem.kernallogtext == null || testlogitem.kernallogtext == string.Empty))
                        {
                            MessageBox.Show("Select KernalLog command \n", "QAT Error Message", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }

                        if (testlogitem.ConfiguratorIsChecked == true && (testlogitem.configuratorlogtext == null || testlogitem.configuratorlogtext == string.Empty))
                        {
                            MessageBox.Show("Select ConfiguratorLog command \n", "QAT Error Message", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }

                        if (testlogitem.ConfiguratorIsChecked == true && (testlogitem.configuratorlogtext == null || testlogitem.configuratorlogtext == string.Empty))
                        {
                            MessageBox.Show("Select ConfiguratorLog command \n", "QAT Error Message", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }

                        if (testlogitem.EventLogIsChecked == true && (testlogitem.eventlogtext == null || testlogitem.eventlogtext == string.Empty))
                        {
                            MessageBox.Show("Select EventLog command \n", "QAT Error Message", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }

                        if (testlogitem.SIPLogIsChecked == true && (testlogitem.siplogtext == null || testlogitem.siplogtext == string.Empty))
                        {
                            MessageBox.Show("Select SIP Log command \n", "QAT Error Message", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }

                        if (testlogitem.QsysAppLogIsChecked == true && (testlogitem.qsysapplogtext == null || testlogitem.qsysapplogtext == string.Empty))
                        {
                            MessageBox.Show("Select QSysAppLog command \n", "QAT Error Message", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }

                        if (testlogitem.UCIViewerLogIsChecked == true && (testlogitem.UCIlogtext == null || testlogitem.UCIlogtext == string.Empty))
                        {
                            MessageBox.Show("Select UCILog command \n", "QAT Error Message", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }

                        if (testlogitem.WindowsEventLogsIsChecked == true && (testlogitem.windowseventlogtext == null || testlogitem.windowseventlogtext == string.Empty))
                        {
                            MessageBox.Show("Select WindowsEventLog command \n", "QAT Error Message", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }
                    }

                    string errorHandlingSelectedType = string.Empty;
                    if (cbmitemsvalue.LogContinueTesting == true)
                    {
                        errorHandlingSelectedType = "Continue Testing";
                    }
                    else
                    {
                        errorHandlingSelectedType = "pause";
                    }
                    string txtFailedTC = cbmitemsvalue.LogReRunFailedTestCase;
                    string txtWaitTime = string.Empty;
                    string cmbSelectedItem = string.Empty;
                    string MonitorType = "Log Monitoring";

                    if (sourceTestPlanItem != null)
                    {
                        string query = "Insert into BackgroundMonitoring values('" + 0 + "','" + sourceTestPlanItem.TestPlanID + "','" + string.Empty + "','" + string.Empty + "','" + string.Empty + "','" + errorHandlingSelectedType + "','" + txtFailedTC + "','" + txtWaitTime + "','" + cmbSelectedItem + "','" + MonitorType + "')";

                        DeleteRowInTable(sourceTestPlanItem.TestPlanID.ToString(), "TPID", MonitorType);
                        InsertintoTables(query, MonitorType, null, "TPID", sourceTestPlanItem.TestPlanID.ToString(), null, null, null, null, null, null, null, null, null, false, null, null, null, null);
                    }
                }
                else
                {
                    cbmitemsvalue.LogContinueTesting = true;
                    cbmitemsvalue.LogPauseatErrorstate = false;
                    cbmitemsvalue.LogReRunFailedTestCase = string.Empty;

                    string query = string.Empty;
                    //delete the rows
                    if (sourceTestPlanItem != null)
                    {
                        DeleteRowInTable(sourceTestPlanItem.TestPlanID.ToString(), "TPID", "Log Monitoring");
                        query = "delete from LogMonitor1 where TPID = '" + sourceTestPlanItem.TestPlanID.ToString() + "'";
                    }

                    if (query != string.Empty)
                    {
                        adap = new SqlDataAdapter(query, this.connect.CreateConnection());
                        this.connect.OpenConnection();
                        adap.Update(datatable1);
                        adap.Fill(datatable1);
                        this.connect.CloseConnection();
                    }
                }

                this.Close();
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

        private void DeleteRowInTable(string mstrID, string Name, string MonitorType)
        {
            try
            {
                //Delete Rows
                string query = string.Empty;
                query = "delete from BackgroundMonitoring where " + Name + " = '" + mstrID + "'and MonitorType= '" + MonitorType + "'";
                adap = new SqlDataAdapter(query, this.connect.CreateConnection());
                this.connect.OpenConnection();
                adap.Update(datatable1);
                adap.Fill(datatable1);
                this.connect.CloseConnection();
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

        private void InsertintoTelnetTables(string query, string MonitorType, string TelnetDevices, string Name, string mstrID, String TelnetCommand, string Telnetverifytype, string comparetext, string Telnetcheckedstatus)
        {
            Int32 BMid = 0;
            try
            {
                //BackgroundMonitoring Insert
                adap = new SqlDataAdapter(query, this.connect.CreateConnection());
                this.connect.OpenConnection();
                adap.Update(datatable1);
                adap.Fill(datatable1);
                this.connect.CloseConnection();

                //BackgroundMonitoring select
                query = string.Empty;
                query = "select BackgroundMonitorID from BackgroundMonitoring where "+ Name + " = '" + mstrID + "'";
                adap = new SqlDataAdapter(query, this.connect.CreateConnection());
                this.connect.OpenConnection();
                adap.Update(datatable1);
                adap.Fill(datatable1);
                this.connect.CloseConnection();

                if (datatable1.Rows.Count > 0)
                {
                    read = datatable1.CreateDataReader();

                    while (read.Read())
                    {
                        BMid = Convert.ToInt32(read.GetInt32(0));
                    }
                }
                //Link creation

                Int32 selectedID = Convert.ToInt32(mstrID);
                backgroundMonitorLinkTableandTPTS(Name, BMid, selectedID);

                //Insert Values
                query = string.Empty;
                datatable1 = new DataTable();


                string keywordCompareType = cbmitemsvalue.KeywordTypeVerify;

                foreach (CBMTestTelnetItem setTestTelnetItem in cbmitemsvalue.SetTestTelnetList)
                {
                    string telnetDeviceName = null;
                    string telnetDeviceModel = null;

                    if (setTestTelnetItem.TelnetSelectedDevice == "All devices")
                    {
                        foreach (string deviceName in setTestTelnetItem.CBMCombo)
                        {
                            if (deviceName != "All devices" && !deviceName.StartsWith("Video Gen"))
                            {
                                telnetDeviceName += deviceName + ",";

                                if (setTestTelnetItem.CBMComboModel.Keys.Contains(deviceName))
                                    telnetDeviceModel += setTestTelnetItem.CBMComboModel[deviceName] + ",";
                                else
                                    telnetDeviceModel += "NA";
                            }
                        }

                        telnetDeviceName = telnetDeviceName.TrimEnd(',');
                        telnetDeviceModel = telnetDeviceModel.TrimEnd(',');
                    }
                    else
                    {
                        telnetDeviceName = setTestTelnetItem.TelnetSelectedDevice;

                        if (setTestTelnetItem.CBMComboModel.Keys.Contains(setTestTelnetItem.TelnetSelectedDevice))
                            telnetDeviceModel = setTestTelnetItem.CBMComboModel[setTestTelnetItem.TelnetSelectedDevice];
                        else
                            telnetDeviceModel = "NA";
                    }

                    //string telnetDeviceName = null;
                    //if (setTestTelnetItem.TelnetSelectedDevice == "All devices")
                    //{
                    //    foreach (string deviceName in setTestTelnetItem.CBMCombo)
                    //    {
                    //        if (deviceName != "All devices")
                    //            telnetDeviceName += deviceName + ",";
                    //    }

                    //    telnetDeviceName = telnetDeviceName.TrimEnd(',');
                    //}
                    //else
                    //{
                    //    telnetDeviceName = setTestTelnetItem.TelnetSelectedDevice;
                    //}

                    TelnetCommand = setTestTelnetItem.TelnetCommandText;

                   if (MonitorType == "Telnet Monitor")
                    {
                        if (sourceTestSuiteItem != null)
                        {
                            query = "Insert into TelenetMonitor values('" + BMid + "','" + sourceTestSuiteItem.TestSuiteID + "','" + 0 + "',@TelnetDeviceName,'" + telnetDeviceName + "','" + Telnetverifytype + "',@CompareText,'" + Telnetcheckedstatus + "','" + telnetDeviceModel + "','"+ keywordCompareType + "')";
                        }
                        else if (sourceTestPlanItem != null)
                        {
                            query = "Insert into TelenetMonitor values('" + BMid + "','" + 0 + "','" + sourceTestPlanItem.TestPlanID + "',@TelnetDeviceName,'" + telnetDeviceName + "','" + Telnetverifytype + "',@CompareText,'" + Telnetcheckedstatus + "','" + telnetDeviceModel + "','" + keywordCompareType + "')";
                        }
                    }

                    SqlCommand command = new SqlCommand(query, this.connect.CreateConnection());
                    this.connect.OpenConnection();
                    command.Parameters.AddWithValue("@TelnetDeviceName", TelnetCommand);
                    command.Parameters.AddWithValue("@CompareText", comparetext);
                    SqlDataAdapter dap = new SqlDataAdapter(command);
                    dap.Fill(datatable1);

                    //adap = new SqlDataAdapter(query, this.connect.CreateConnection());
                    //adap.Update(datatable1);
                    //adap.Fill(datatable1);

                    this.connect.CloseConnection();
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

        private void backgroundMonitorLinkTableandTPTS(string tabletype, Int32 BMID, Int32 mstrID)
        {
            try
            {
                string query = string.Empty;
                if (tabletype == "TSID")
                {
                    query = "Insert into TSMonitorLinkTable values('" + mstrID + "','" + BMID + "')";
                }
                else if (tabletype == "TPID")
                {
                    query = "Insert into TPMonitorLinkTable values('" + mstrID + "','" + BMID + "')";
                }
                adap = new SqlDataAdapter(query, this.connect.CreateConnection());
                this.connect.OpenConnection();
                adap.Update(datatable1);
                adap.Fill(datatable1);
                this.connect.CloseConnection();
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

        private void TextBox_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            var textBox = e.OriginalSource as TextBox;

            if (textBox != null && e.Key == Key.Tab)
            {
                textBox.SelectAll();
            }
        }

        private void ReadDataFromTable()
        {
            try
            {
                string query = string.Empty;
                //string InventoryQuery = string.Empty;
                string TelnetQuery = string.Empty;
                string LogQuery = string.Empty;
                DataTable readmain = new DataTable();
                DataTableReader read1 = null;
                if (sourceTestSuiteItem != null)
                {
                    query = "Select * from BackgroundMonitoring where TSID='" + sourceTestSuiteItem.TestSuiteID + "'";
                    //InventoryQuery = "Select SelectedLogsType from InventoryMonitor where TSID='" + sourceTestSuiteItem.TestSuiteID + "'";
                    TelnetQuery = "Select * from TelenetMonitor where TSID='" + sourceTestSuiteItem.TestSuiteID + "'";
                }
                if (sourceTestPlanItem != null)
                {
                    query = "Select * from BackgroundMonitoring where TPID='" + sourceTestPlanItem.TestPlanID + "'";
                    //InventoryQuery = "Select SelectedLogsType from InventoryMonitor where TPID='" + sourceTestPlanItem.TestPlanID + "'";
                    TelnetQuery = "Select * from TelenetMonitor where TPID='" + sourceTestPlanItem.TestPlanID + "'";
                    LogQuery= "Select * from LogMonitor1 where TPID='" + sourceTestPlanItem.TestPlanID + "'";
                }

                if (query != string.Empty)
                {
                    adap = new SqlDataAdapter(query, this.connect.CreateConnection());
                    this.connect.OpenConnection();
                    adap.Fill(readmain);
                }

                if (readmain.Rows.Count > 0)
                {
                    read1 = readmain.CreateDataReader();

                    while (read1.Read())
                    {
                        string MonitorType = read1.GetString(10);

                        //if (MonitorType == "Inventory Monitor")
                        //{
                        //    InventoryMonitorValue(InventoryQuery, readmain);
                        //}

                        //Control Value Monitoring
                        if (MonitorType == "Control Value Monitoring")
                        {
                            if (sourceTestSuiteItem != null)
                            {
                                ControlMonitorValue(readmain, sourceTestSuiteItem.TestSuiteID.ToString());
                            }
                            else if (sourceTestPlanItem != null)
                            {
                                ControlMonitorValue(readmain, sourceTestPlanItem.TestPlanID.ToString());
                            }
                        }

                        if (MonitorType == "Telnet Monitor")
                        {
                            TelnetMonitorValue(TelnetQuery, readmain);                                             
                        }

                        if(MonitorType == "Inventory Monitoring")
                        {
                            InventoryMonitorValue(query, readmain);
                        }
                        if(MonitorType == "Log Monitoring")
                        {
                            LogMonitorValue(query, readmain, LogQuery);
                        }
                    }
                    this.connect.CloseConnection();
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

        private void LogMonitorValue(string query, DataTable readmain,string LogQuery)
        {
            try
            {
                AddDutDevices();
                ReadLogMonitor(LogQuery);
                DataTableReader read_Telnet;
                read_Telnet = readmain.CreateDataReader();
                while (read_Telnet.Read())
                {
                    string MonitorType = read_Telnet.GetString(10);
                    if (MonitorType == "Log Monitoring")
                    {                       
                        string inventRadiobtn = string.Empty;
                        inventRadiobtn = read_Telnet.GetString(6);
                        if (inventRadiobtn == "Continue Testing")
                        {
                            cbmitemsvalue.LogContinueTesting = true;
                            cbmitemsvalue.LogPauseatErrorstate = false;
                        }
                        else
                        {
                            cbmitemsvalue.LogContinueTesting = false;
                            cbmitemsvalue.LogPauseatErrorstate = true;
                        }

                        cbmitemsvalue.LogReRunFailedTestCase = read_Telnet.GetString(7);
                    }
                }
                LogMonitoring.IsChecked = true;
                LogErrorHandling.DataContext = cbmitemsvalue;
             
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif

                //MessageBox.Show("Exception\n " + ex.Message, "Error Code - ECxxxxxI", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ReadLogMonitor(string telnetQuery)
        {
            try
            {
                DataTableReader read_Telnet;
                DataTable readLog = new DataTable();
                if (telnetQuery != string.Empty)
                {
                    adap = new SqlDataAdapter(telnetQuery, this.connect.CreateConnection());
                    this.connect.OpenConnection();
                    adap.Fill(readLog);
                }
                read_Telnet = readLog.CreateDataReader();
                cbmitemsvalue.VerifyTestLogList.Clear();
                while (read_Telnet.Read())
                {
                    CBMTestLogItem testLogItem = AddVerifyTestLogItem();

                    if (String.Equals(read_Telnet.GetString(4).ToString(), "True", StringComparison.CurrentCultureIgnoreCase))
                        testLogItem.iLogIsChecked = true;
                    else
                        testLogItem.iLogIsChecked = false;

                    if (testLogItem.iLogIsChecked)
                    {
                        testLogItem.iLog_combobox_enable = true;
                        testLogItem.iLog_selected_item = read_Telnet.GetString(5).ToString();
                        testLogItem.ilogtext = read_Telnet.GetString(6).ToString();
                    }
                    ///kernellog

                    if (String.Equals(read_Telnet.GetString(7).ToString(), "True", StringComparison.CurrentCultureIgnoreCase))
                        testLogItem.KernelLogIsChecked = true;
                    else
                        testLogItem.KernelLogIsChecked = false;

                    if (testLogItem.KernelLogIsChecked)
                    {
                        testLogItem.kernelLog_combobox_enable = true;
                        testLogItem.kernalLog_selected_item = read_Telnet.GetString(8).ToString();
                        testLogItem.kernallogtext = read_Telnet.GetString(9).ToString();
                    }
                    ///eventlog
                    if (String.Equals(read_Telnet.GetString(10).ToString(), "True", StringComparison.CurrentCultureIgnoreCase))
                        testLogItem.EventLogIsChecked = true;

                    else
                        testLogItem.EventLogIsChecked = false;
                    if (testLogItem.EventLogIsChecked)
                        testLogItem.eventlogtext = read_Telnet.GetString(11).ToString();
                    /// configuratorlog
                    if (String.Equals(read_Telnet.GetString(12).ToString(), "True", StringComparison.CurrentCultureIgnoreCase))
                        testLogItem.ConfiguratorIsChecked = true;

                    else
                        testLogItem.ConfiguratorIsChecked = false;
                    if (testLogItem.ConfiguratorIsChecked)
                        testLogItem.configuratorlogtext = read_Telnet.GetString(13).ToString();

                    /// siplog
                    if (String.Equals(read_Telnet.GetString(14).ToString(), "True", StringComparison.CurrentCultureIgnoreCase))
                        testLogItem.SIPLogIsChecked = true;

                    else
                        testLogItem.SIPLogIsChecked = false;
                    if (testLogItem.SIPLogIsChecked)
                        testLogItem.siplogtext = read_Telnet.GetString(15).ToString();


                    /// qsysapplog

                    if (String.Equals(read_Telnet.GetString(16).ToString(), "True", StringComparison.CurrentCultureIgnoreCase))
                        testLogItem.QsysAppLogIsChecked = true;

                    else
                        testLogItem.QsysAppLogIsChecked = false;
                    if (testLogItem.QsysAppLogIsChecked)
                        testLogItem.qsysapplogtext = read_Telnet.GetString(17).ToString();


                    /// ucilog

                    if (String.Equals(read_Telnet.GetString(18).ToString(), "True", StringComparison.CurrentCultureIgnoreCase))
                        testLogItem.UCIViewerLogIsChecked = true;

                    else
                        testLogItem.UCIViewerLogIsChecked = false;
                    if (testLogItem.UCIViewerLogIsChecked)
                        testLogItem.UCIlogtext = read_Telnet.GetString(19).ToString();


                    /// softphonelog

                    if (String.Equals(read_Telnet.GetString(20).ToString(), "True", StringComparison.CurrentCultureIgnoreCase))
                        testLogItem.SoftPhoneLogIsChecked = true;

                    else
                        testLogItem.SoftPhoneLogIsChecked = false;
                    if (testLogItem.SoftPhoneLogIsChecked)
                        testLogItem.softphonelogtext = read_Telnet.GetString(21).ToString();

                    /// windowseventlog
                    if (String.Equals(read_Telnet.GetString(22).ToString(), "True", StringComparison.CurrentCultureIgnoreCase))
                        testLogItem.WindowsEventLogsIsChecked = true;

                    else
                        testLogItem.WindowsEventLogsIsChecked = false;
                    if (testLogItem.WindowsEventLogsIsChecked)
                        testLogItem.windowseventlogtext = read_Telnet.GetString(23).ToString();
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

        private void InventoryMonitorValue(string TelnetQuery, DataTable TelRead)
        {
            try
            {
                DataTableReader read_Telnet;
                read_Telnet = TelRead.CreateDataReader();
                while (read_Telnet.Read())
                {
                    string MonitorType = read_Telnet.GetString(10);
                    if (MonitorType == "Inventory Monitoring")
                    {
                        InventoryMonitoring.IsChecked = true;
                        string inventRadiobtn = string.Empty;
                        inventRadiobtn = read_Telnet.GetString(6);
                        if (inventRadiobtn == "Continue Testing")
                        {
                            cbmitemsvalue.InventoryContinueTesting = true;
                            cbmitemsvalue.InventoryPauseatErrorstate = false;
                        }
                        else
                        {
                            cbmitemsvalue.InventoryContinueTesting = false;
                            cbmitemsvalue.InventoryPauseatErrorstate = true;
                        }

                        cbmitemsvalue.InventoryReRunFailedTestCase = read_Telnet.GetString(7);
                    }
                }

                InventoryErrorHandling.DataContext = cbmitemsvalue;
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif

                //MessageBox.Show("Exception\n " + ex.Message, "Error Code - ECxxxxxI", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void TelnetMonitorValue(string TelnetQuery, DataTable TelRead)
        {
            try
            {
                DataTableReader read_Telnet;
                read_Telnet = TelRead.CreateDataReader();
                while (read_Telnet.Read())
                {
                    string MonitorType = read_Telnet.GetString(10);

                    if (MonitorType == "Telnet Monitor")
                    {
                        
                        string TelnetRadiobtn = string.Empty;
                        TelnetRadiobtn = read_Telnet.GetString(6);
                        if (TelnetRadiobtn == "Continue Testing")
                        {
                            cbmitemsvalue.ContinueTesting = true;
                            cbmitemsvalue.PauseatErrorstate = false;

                        }
                        else
                        {
                            cbmitemsvalue.ContinueTesting = false;
                            cbmitemsvalue.PauseatErrorstate = true;
                        }
                        //Error Handling
                        cbmitemsvalue.ReRunFailedTestCase = read_Telnet.GetString(7);
                        //TelnetErrorHandlingText2.Text = read_Telnet.GetString(8);
                        //TelnetErrorHandlingCombo.SelectedValue = read_Telnet.GetString(9);
                        //TelnetMonitor.IsChecked = true;
                        ReadTelnetMonitor(TelnetQuery);
                    }
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

        private void ReadTelnetMonitor(string TelnetQuery)
        {
            DataTable Telnetdatatable = null;
            SqlDataAdapter adap = null;
            DataTableReader telread = null;
            try
            {
                Telnetdatatable = new DataTable();
                Telnetdatatable.Clear();
                adap = new SqlDataAdapter(TelnetQuery, this.connect.CreateConnection());
                this.connect.OpenConnection();
                adap.Fill(Telnetdatatable);
                if (Telnetdatatable.Rows.Count > 0)
                {
                    telread = Telnetdatatable.CreateDataReader();
                    cbmitemsvalue.SetTestTelnetList.Clear();
                    while (telread.Read())
                    {
                        string Telnetcheckedstatus = telread.GetString(8);
                        if (Telnetcheckedstatus == "True")
                        {
                            TelnetMonitor.IsChecked = true;
                            CBMTestTelnetItem testTelnetItem = AddSetTestTelnetItem();
                                                       
                            testTelnetItem.TelnetCommandText = telread.GetString(4);
                            cbmitemsvalue.FailureCriteria = telread.GetString(7);
                            string Deviceselected = telread.GetString(5);
                            string[] Deviceselected1 = Deviceselected.Split(',');
                            
                            if (Deviceselected1.Count() > 1)
                                testTelnetItem.TelnetSelectedDevice = "All devices";
                            else
                                testTelnetItem.TelnetSelectedDevice = Deviceselected;
                            
                            string str = telread.GetString(6);
                            if (str == "Continue Without Doing Anything")
                            {
                                cbmitemsvalue.ContinueWithoutdoinganything = true;
                                cbmitemsvalue.StoreCurrentResult = false;
                                cbmitemsvalue.CompareValues = false;
                                cbmitemsvalue.FailureCriteria = string.Empty;
                            }
                            else if (str == "Store Current Result")
                            {
                                cbmitemsvalue.ContinueWithoutdoinganything = false;
                                cbmitemsvalue.StoreCurrentResult = true;
                                cbmitemsvalue.CompareValues = false;
                            }
                            else
                            {
                                cbmitemsvalue.ContinueWithoutdoinganything = false;
                                cbmitemsvalue.StoreCurrentResult = false;
                                cbmitemsvalue.CompareValues = true;
                               if(telread[10] != System.DBNull.Value)
                                  cbmitemsvalue.KeywordTypeVerify = telread.GetString(10).ToString();
                            }
                        }
                        else
                        {
                            TelnetMonitor.IsChecked = false;
                        }

                        //if (Device_to_add.Children.Count==0)
                        //{
                        //    cbmitemsvalue.TelnetCommandText = string.Empty;
                        //    cbmitemsvalue.FailureCriteria = string.Empty;
                        //    cbmitemsvalue.ReRunFailedTestCase = string.Empty;
                        //    TelnetMonitor.IsChecked = false;
                        //    //MessageBox.Show("No TestPlan are associated to this TestSuite");
                        //}
                    }
                    this.connect.CloseConnection();
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
        
        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                MessageBoxResult btnresult = MessageBox.Show("Are you sure you want to close","QAT-Background Monitor", MessageBoxButton.YesNo, MessageBoxImage.Information);
                if (btnresult == MessageBoxResult.Yes)
                {
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif

                //MessageBox.Show("Exception\n " + ex.Message, "QAT Error Code - EC12099", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void cmb_ComponentName_DropDownOpened(object sender, EventArgs e)
        {
            try
            {               
                DataTableReader read1 = null;
                if (sourceTestPlanItem != null)
                {
                    string selected_componenttype = cbmitemsvalue.cmb_ComponentTypeSelectedItem;
                    string query = "select ComponentName from TCInitialization where ComponentType=('" + selected_componenttype + "') and designid=('" + desgnid + "')";
                    tble = QscDatabase.SendCommand_Toreceive(query);
                    read1 = tble.CreateDataReader();

                    List<string> componentname = new List<string>();
                    while (read1.Read())
                    {
                        componentname.Add(read1.GetString(0));
                    }
                    cbmitemsvalue.ComponentNameList.Clear();
                    //cbmitemsvalue.ComponentNameSelectedItem = null;
                    foreach (string s in componentname)
                    {
                        if (!cbmitemsvalue.ComponentNameList.Contains(s))
                        {
                            cbmitemsvalue.ComponentNameList.Add(s);

                        }
                    }
                   
                    componentname.Clear();
                }

                clearValue = true;
                VerifyControlContentDataTemplareData.DataContext = null;
                VerifyControlContentDataTemplareData.DataContext = cbmitemsvalue;
                clearValue = false;
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
        
        private void cmb_Property_DropDownOpened(object sender, EventArgs e)
        {
            try
            {
                DataTableReader read1 = null;
                string selected_componentname = cbmitemsvalue.ComponentNameSelectedItem;
                string query = "select PrettyName,ControlDirection from TCInitialization where ComponentName=('" + selected_componentname + "') and designid=('" + desgnid + "') order by PrettyName asc";
                tble = QscDatabase.SendCommand_Toreceive(query);
                read1 = tble.CreateDataReader();
                List<string> componentproperty = new List<string>();               

                while (read1.Read())
                {
                    if (read1[0] != System.DBNull.Value&&read1[1] != System.DBNull.Value)
                    {
                        if(read1.GetString(1).Contains("control_direction_external_read"))
                        {
                            string[] Channel_split = new string[2];
                            if (read1.GetString(0).Contains("~"))/*(((read1.GetString(0).Contains("Channel"))|| (read1.GetString(0).Contains("Output"))|| (read1.GetString(0).Contains("Input")) || (read1.GetString(0).Contains("Tap")) || (read1.GetString(0).Contains("Bank Control")) || (read1.GetString(0).Contains("Bank Select")) || (read1.GetString(0).Contains("Bank Control")) || (read1.GetString(0).Contains("GPIO"))) && (read1.GetString(0).Contains("~")))*/
                            {
                                string channelControl = string.Empty;
                                int tiltCount = read1.GetString(0).Count(x => x == '~');
                                string channelWithTwoTilt = read1.GetString(0);
                                int idx = channelWithTwoTilt.LastIndexOf('~');
                                Channel_split[0] = channelWithTwoTilt.Substring(0, idx);
                                Channel_split[1] = channelWithTwoTilt.Substring(idx + 1);

                                channelControl =QscDatabase.addQATPrefixToControl(read1.GetString(0)) + Channel_split[1];
                                if (!componentproperty.Contains(channelControl))
                                {
                                    componentproperty.Add(channelControl);
                                }
                                //int tiltCount = read1.GetString(0).Count(x => x == '~');
                                //if (tiltCount == 1)
                                //{
                                //    string channelControl = string.Empty;
                                //    string[] lstchannel = read1.GetString(0).Split('~');
                                //    if (!cbmitemsvalue.cmb_Channel.Contains(lstchannel[0]))
                                //        cbmitemsvalue.cmb_Channel.Add(lstchannel[0]);
                                //    if (read1.GetString(0).Contains("Channel"))
                                //        channelControl = "CHANNEL " + lstchannel[1];
                                //    else if ((read1.GetString(0).Contains("Output"))&(!(read1.GetString(0).Contains("Input"))) & (!(read1.GetString(0).Contains("Bank Control"))) & (!(read1.GetString(0).Contains("Bank Select"))) & (!(read1.GetString(0).Contains("GPIO"))))
                                //        channelControl = "OUTPUT " + lstchannel[1];
                                //    else if ((read1.GetString(0).Contains("Input")) & (!(read1.GetString(0).Contains("Output"))) & (!(read1.GetString(0).Contains("Bank Control")))& (!(read1.GetString(0).Contains("Bank Select"))) & (!(read1.GetString(0).Contains("GPIO"))))
                                //        channelControl = "INPUT " + lstchannel[1];
                                //    else if ((read1.GetString(0).Contains("Input")) & ((read1.GetString(0).Contains("Output"))) & ((read1.GetString(0).StartsWith("Output"))) & (!(read1.GetString(0).Contains("Bank Control"))) & (!(read1.GetString(0).Contains("GPIO"))))
                                //        channelControl = "OUTPUT " + lstchannel[1];
                                //    else if ((read1.GetString(0).Contains("Input")) & ((read1.GetString(0).Contains("Output"))) & ((read1.GetString(0).StartsWith("Input"))) & (!(read1.GetString(0).Contains("Bank Control"))) & (!(read1.GetString(0).Contains("GPIO"))))
                                //        channelControl = "INPUT " + lstchannel[1];
                                //    else if (read1.GetString(0).Contains("Tap"))
                                //        channelControl = "TAP " + lstchannel[1];
                                //    else if ((read1.GetString(0).Contains("Bank Control"))& (read1.GetString(0).Contains("Input")) & (!(read1.GetString(0).Contains("Output"))))
                                //        channelControl = "BANK_CONTROL_INPUT " + lstchannel[1];
                                //    else if ((read1.GetString(0).Contains("Bank Control")) & (read1.GetString(0).Contains("Output")) & (!(read1.GetString(0).Contains("Input"))))
                                //        channelControl = "BANK_CONTROL_OUTPUT " + lstchannel[1];
                                //    else if ((read1.GetString(0).Contains("Bank Control")) & (read1.GetString(0).Contains("Output")) & (read1.GetString(0).Contains("Input")))
                                //        channelControl = "BANK_CONTROL_INPUT_OUTPUT " + lstchannel[1];
                                //    else if ((read1.GetString(0).Contains("Bank Select")) & (read1.GetString(0).Contains("Input")))
                                //        channelControl = "BANK_SELECT_INPUT " + lstchannel[1];
                                //    else if ((read1.GetString(0).Contains("Bank Select")) & (read1.GetString(0).Contains("Output")))
                                //        channelControl = "BANK_SELECT_OUTPUT " + lstchannel[1];
                                //    else if ((read1.GetString(0).Contains("GPIO")) & (read1.GetString(0).Contains("Input")) & (!(read1.GetString(0).Contains("Out"))))
                                //        channelControl = "GPIO_INPUT " + lstchannel[1];
                                //    else if ((read1.GetString(0).Contains("GPIO")) & (read1.GetString(0).Contains("Out"))&(!(read1.GetString(0).Contains("Input"))))
                                //        channelControl = "GPIO_OUTPUT " + lstchannel[1];
                                //    if (!componentproperty.Contains(channelControl))
                                //    {
                                //        componentproperty.Add(channelControl);
                                //    }
                                //}
                                //else if (tiltCount == 2)
                                //{
                                //    string channelWithTwoTilt = read1.GetString(0);
                                //    int idx = channelWithTwoTilt.LastIndexOf('~');
                                //    string channelControl = string.Empty;
                                //    string[] lstchannel = new string[2];
                                //    lstchannel[0]= channelWithTwoTilt.Substring(0,idx);
                                //    lstchannel[1] = channelWithTwoTilt.Substring(idx + 1);
                                //    if (!cbmitemsvalue.cmb_Channel.Contains(lstchannel[0]))
                                //        cbmitemsvalue.cmb_Channel.Add(lstchannel[0]);
                                //    if (read1.GetString(0).Contains("Channel"))
                                //        channelControl = "CHANNEL " + lstchannel[1];
                                //    else if ((read1.GetString(0).Contains("Output")) & (!(read1.GetString(0).Contains("Input"))) & (!(read1.GetString(0).Contains("Bank Control"))) & (!(read1.GetString(0).Contains("Bank Select"))) & (!(read1.GetString(0).Contains("GPIO"))))
                                //        channelControl = "OUTPUT " + lstchannel[1];
                                //    else if ((read1.GetString(0).Contains("Input")) & (!(read1.GetString(0).Contains("Output"))) & (!(read1.GetString(0).Contains("Bank Control"))) & (!(read1.GetString(0).Contains("Bank Select"))) & (!(read1.GetString(0).Contains("GPIO"))))
                                //        channelControl = "INPUT " + lstchannel[1];
                                //    else if ((read1.GetString(0).Contains("Input")) & ((read1.GetString(0).Contains("Output"))) & (!(read1.GetString(0).Contains("Bank Control"))) & (!(read1.GetString(0).Contains("GPIO"))))
                                //        channelControl = "INPUT_OUTPUT " + lstchannel[1];
                                //    else if (read1.GetString(0).Contains("Tap"))
                                //        channelControl = "TAP " + lstchannel[1];
                                //    else if ((read1.GetString(0).Contains("Bank Control")) & (read1.GetString(0).Contains("Input")) & (!(read1.GetString(0).Contains("Output"))))
                                //        channelControl = "BANK_CONTROL_INPUT " + lstchannel[1];
                                //    else if ((read1.GetString(0).Contains("Bank Control")) & (read1.GetString(0).Contains("Output")) & (!(read1.GetString(0).Contains("Input"))))
                                //        channelControl = "BANK_CONTROL_OUTPUT " + lstchannel[1];
                                //    else if ((read1.GetString(0).Contains("Bank Control")) & (read1.GetString(0).Contains("Output")) & (read1.GetString(0).Contains("Input")))
                                //        channelControl = "BANK_CONTROL_INPUT_OUTPUT " + lstchannel[1];
                                //    else if ((read1.GetString(0).Contains("Bank Select")) & (read1.GetString(0).Contains("Input")))
                                //        channelControl = "BANK_SELECT_INPUT " + lstchannel[1];
                                //    else if ((read1.GetString(0).Contains("Bank Select")) & (read1.GetString(0).Contains("Output")))
                                //        channelControl = "BANK_SELECT_OUTPUT " + lstchannel[1];
                                //    else if ((read1.GetString(0).Contains("GPIO")) & (read1.GetString(0).Contains("Input")) & (!(read1.GetString(0).Contains("Out"))))
                                //        channelControl = "GPIO_INPUT " + lstchannel[1];
                                //    else if ((read1.GetString(0).Contains("GPIO")) & (read1.GetString(0).Contains("Out")) & (!(read1.GetString(0).Contains("Input"))))
                                //        channelControl = "GPIO_OUTPUT " + lstchannel[1];
                                //    if (!componentproperty.Contains(channelControl))
                                //    {
                                //        componentproperty.Add(channelControl);
                                //    }

                                //}
                                //else if (tiltCount == 3)
                                //{
                                //    string channelWithTwoTilt = read1.GetString(0);
                                //    int idx = channelWithTwoTilt.LastIndexOf('~');
                                //    string channelControl = string.Empty;
                                //    string[] lstchannel = new string[2];
                                //    lstchannel[0] = channelWithTwoTilt.Substring(0, idx);
                                //    lstchannel[1] = channelWithTwoTilt.Substring(idx + 1);
                                //    if (!cbmitemsvalue.cmb_Channel.Contains(lstchannel[0]))
                                //        cbmitemsvalue.cmb_Channel.Add(lstchannel[0]);
                                //    if (read1.GetString(0).Contains("Channel"))
                                //        channelControl = "CHANNEL " + lstchannel[1];
                                //    else if ((read1.GetString(0).Contains("Output")) & (!(read1.GetString(0).Contains("Input"))) & (!(read1.GetString(0).Contains("Bank Control"))) & (!(read1.GetString(0).Contains("Bank Select"))) & (!(read1.GetString(0).Contains("GPIO"))))
                                //        channelControl = "OUTPUT " + lstchannel[1];
                                //    else if ((read1.GetString(0).Contains("Input")) & (!(read1.GetString(0).Contains("Output"))) & (!(read1.GetString(0).Contains("Bank Control"))) & (!(read1.GetString(0).Contains("Bank Select"))) & (!(read1.GetString(0).Contains("GPIO"))))
                                //        channelControl = "INPUT " + lstchannel[1];
                                //    else if ((read1.GetString(0).Contains("Input")) & ((read1.GetString(0).Contains("Output"))) & (!(read1.GetString(0).Contains("Bank Control"))) & (!(read1.GetString(0).Contains("GPIO"))))
                                //        channelControl = "INPUT_OUTPUT " + lstchannel[1];
                                //    else if (read1.GetString(0).Contains("Tap"))
                                //        channelControl = "TAP " + lstchannel[1];
                                //    else if ((read1.GetString(0).Contains("Bank Control")) & (read1.GetString(0).Contains("Input")) & (!(read1.GetString(0).Contains("Output"))))
                                //        channelControl = "BANK_CONTROL_INPUT " + lstchannel[1];
                                //    else if ((read1.GetString(0).Contains("Bank Control")) & (read1.GetString(0).Contains("Output")) & (!(read1.GetString(0).Contains("Input"))))
                                //        channelControl = "BANK_CONTROL_OUTPUT " + lstchannel[1];
                                //    else if ((read1.GetString(0).Contains("Bank Control")) & (read1.GetString(0).Contains("Output")) & (read1.GetString(0).Contains("Input")))
                                //        channelControl = "BANK_CONTROL_INPUT_OUTPUT " + lstchannel[1];
                                //    else if ((read1.GetString(0).Contains("Bank Select")) & (read1.GetString(0).Contains("Input")))
                                //        channelControl = "BANK_SELECT_INPUT " + lstchannel[1];
                                //    else if ((read1.GetString(0).Contains("Bank Select")) & (read1.GetString(0).Contains("Output")))
                                //        channelControl = "BANK_SELECT_OUTPUT " + lstchannel[1];
                                //    else if ((read1.GetString(0).Contains("GPIO")) & (read1.GetString(0).Contains("Input")) & (!(read1.GetString(0).Contains("Out"))))
                                //        channelControl = "GPIO_INPUT " + lstchannel[1];
                                //    else if ((read1.GetString(0).Contains("GPIO")) & (read1.GetString(0).Contains("Out")) & (!(read1.GetString(0).Contains("Input"))))
                                //        channelControl = "GPIO_OUTPUT " + lstchannel[1];
                                //    if (!componentproperty.Contains(channelControl))
                                //    {
                                //        componentproperty.Add(channelControl);
                                //    }

                                //}


                            }
                            else
                            {
                                componentproperty.Add(read1.GetString(0));
                            }
                        }
                        
                    }
                }
                cbmitemsvalue.PropertyList.Clear();
                foreach (string s in componentproperty)
                {
                    if (!cbmitemsvalue.PropertyList.Contains(s))
                    {
                        if (!s.Contains("<") & s != string.Empty)
                        {
                            cbmitemsvalue.PropertyList.Add(s);

                        }
                    }
                }               

                componentproperty.Clear();

                clearValue = true;
                VerifyControlContentDataTemplareData.DataContext = null;
                VerifyControlContentDataTemplareData.DataContext = cbmitemsvalue;
                clearValue = false;

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

        private bool InsertintoTables(string query, string MonitorType, string log, string Name, string mstrID, string ComponentType, string ComponentName, string ComponentProperty, string ComponentChannel, string componentValue, string ControlValueMonitoringcheckedstatus, string ValueType, string MaximumLimit, string MinimumLimit, bool LoopCheckedStatus, string Loop_Start, string Loop_End, string Loop_Increament, string AllChannels)
        {
            Int32 BMid = 0;
            try
            {
                //BackgroundMonitoring Insert
                adap = new SqlDataAdapter(query, this.connect.CreateConnection());
                this.connect.OpenConnection();
                adap.Update(datatable1);
                adap.Fill(datatable1);
                this.connect.CloseConnection();

                //BackgroundMonitoring select
                query = string.Empty;
                query = "select BackgroundMonitorID from [BackgroundMonitoring] where " + Name + " = '" + mstrID + "'";
                adap = new SqlDataAdapter(query, this.connect.CreateConnection());
                this.connect.OpenConnection();
                adap.Update(datatable1);
                adap.Fill(datatable1);
                this.connect.CloseConnection();
                if (datatable1.Rows.Count > 0)
                {
                    read = datatable1.CreateDataReader();

                    while (read.Read())
                    {
                        BMid = Convert.ToInt32(read.GetInt32(0));
                    }
                }

                //Link creation

                Int32 selectedID = Convert.ToInt32(mstrID);
                backgroundMonitorLinkTableandTPTS(Name, BMid, selectedID);

                //Insert Values
                query = string.Empty;
                datatable1 = new DataTable();
                if (MonitorType == "Inventory Monitoring")
                {
                    if (sourceTestPlanItem != null)
                    {
                        query = "Insert into InventoryMonitor values('" + BMid + "','" + 0 + "','" + sourceTestPlanItem.TestPlanID + "','" + string.Empty + "')";
                        adap = new SqlDataAdapter(query, this.connect.CreateConnection());
                        this.connect.OpenConnection();
                        adap.Update(datatable1);
                        adap.Fill(datatable1);
                        this.connect.CloseConnection();
                    }
                }
                else if (MonitorType == "Control Value Monitoring")
                {

                    if (sourceTestPlanItem != null)
                    {
                        foreach (CBMTestControlItem verifyTestControlItem in cbmitemsvalue.VerifyTestControlList)
                        {                          
                            this.connect.CreateConnection();

                            string compType = verifyTestControlItem.TestControlComponentTypeSelectedItem;
                            string compName = verifyTestControlItem.TestControlComponentNameSelectedItem;

                            //string verifyTestControlValues = BMid + "','";
                            //verifyTestControlValues += 0 + "',";
                            //verifyTestControlValues += sourceTestPlanItem.TestPlanID + "',";
                            //verifyTestControlValues += compType + ",";
                            //verifyTestControlValues += compName + ",";

                            string propertyname = string.Empty;
                            string allchannelvalues = string.Empty;
                            string valueDataType = string.Empty;
                            string selectedPrettyName = verifyTestControlItem.ChannelSelectionSelectedItem;
                            string selectedPretty = verifyTestControlItem.TestControlPropertySelectedItem;

                            if (verifyTestControlItem.TestControlPropertySelectedItem != null)
                            {
                                if ((!string.IsNullOrEmpty(verifyTestControlItem.ChannelSelectionSelectedItem)))
                                {
                                    string[] controlPrettyName = null;
                                    verifyTestControlItem.ParentTestActionItem.ControlIDDictionary.TryGetValue(compName + verifyTestControlItem.TestControlPropertySelectedItem + verifyTestControlItem.ChannelSelectionSelectedItem, out controlPrettyName);

                                    if (controlPrettyName != null)
                                    {
                                        if(controlPrettyName.Count() > 0)
                                            selectedPrettyName = controlPrettyName[0];

                                        if(controlPrettyName.Count() > 1)
                                            propertyname = controlPrettyName[1];
                                    }

                                    string removechannel = selectedPrettyName + "~" + QscDatabase.removeQATPrefix(selectedPretty);

                                    cbmitemsvalue.VerifyDataTypeDictionary.TryGetValue(verifyTestControlItem.TestControlComponentNameSelectedItem + removechannel + propertyname, out valueDataType);

                                    //verifyTestControlValues += propertyname + ",'";

                                    if ((verifyTestControlItem.LoopIsChecked) && (verifyTestControlItem.LoopStart != string.Empty) && (verifyTestControlItem.LoopEnd != string.Empty) && (verifyTestControlItem.LoopIncrement != string.Empty))
                                    {
                                        if (verifyTestControlItem.ChannelSelectionList.Count > 0)
                                        {
                                            List<string> channelcontrols = new List<string>();
                                            foreach (string channels in verifyTestControlItem.ChannelSelectionList)
                                            {
                                                string[] localcontrolPretty = null;
                                                verifyTestControlItem.ParentTestActionItem.ControlIDDictionary.TryGetValue(compName + verifyTestControlItem.TestControlPropertySelectedItem + channels, out localcontrolPretty);
                                                //string localPrettyName = controlPretty[0];

                                                string localControlID = string.Empty;
                                                if(localcontrolPretty != null && localcontrolPretty.Count() > 1)
                                                    localControlID = localcontrolPretty[1];

                                                channelcontrols.Add(localControlID);

                                                //string intial = string.Empty;
                                                //intial = channels + "~" + QscDatabase.removeQATPrefix(selectedPretty);
                                                //channelcontrols.Add(propertyname);
                                            }

                                            allchannelvalues = string.Join("|", channelcontrols.ToArray());
                                        }
                                        else
                                        {
                                            allchannelvalues = string.Empty;
                                        }
                                    }
                                    else
                                        allchannelvalues = string.Empty;
                                }
                                else
                                {
                                    string[] controlPrettyArray = null;
                                    verifyTestControlItem.ParentTestActionItem.ControlIDDictionary.TryGetValue(verifyTestControlItem.TestControlComponentNameSelectedItem + verifyTestControlItem.TestControlPropertySelectedItem, out controlPrettyArray);

                                    if (controlPrettyArray != null)
                                    {
                                        if(controlPrettyArray.Count() > 0)
                                            selectedPretty = controlPrettyArray[0];

                                        if(controlPrettyArray.Count() > 1)
                                            propertyname = controlPrettyArray[1];
                                    }

                                    allchannelvalues = string.Empty;
                                    cbmitemsvalue.VerifyDataTypeDictionary.TryGetValue(verifyTestControlItem.TestControlComponentNameSelectedItem + verifyTestControlItem.TestControlPropertySelectedItem + propertyname, out valueDataType);
                                }
                            }

                            if (string.IsNullOrEmpty(compType))
                            {
                                compType = string.Empty;
                            }
                            if (string.IsNullOrEmpty(compName))
                            {
                                compName = string.Empty;
                            }
                            if (string.IsNullOrEmpty(propertyname))
                            {
                                propertyname = string.Empty;
                            }
                            if (string.IsNullOrEmpty(verifyTestControlItem.TestControlPropertyInitialValueSelectedItem))
                            {
                                verifyTestControlItem.TestControlPropertyInitialValueSelectedItem = string.Empty;
                            }

                            if (string.IsNullOrEmpty(selectedPrettyName))
                                selectedPrettyName = string.Empty;

                            if (string.IsNullOrEmpty(selectedPretty))
                                selectedPretty = string.Empty;

                            query = "Insert into ControlMonitor values('" + BMid + "','" + 0 + "','" + sourceTestPlanItem.TestPlanID + "',@ComponentType,@ComponentName,@propertyname,@selectedPrettyName,@ValueString,@selectedPretty,'" + ControlValueMonitoringcheckedstatus + "','" + verifyTestControlItem.InputSelectionComboSelectedItem + "','" + verifyTestControlItem.MaximumLimit + "','" + verifyTestControlItem.MinimumLimit + "','" + verifyTestControlItem.LoopIsChecked + "','" + verifyTestControlItem.LoopStart + "','" + verifyTestControlItem.LoopEnd + "','" + verifyTestControlItem.LoopIncrement + "','" + allchannelvalues + "','" + valueDataType + "')";

                            SqlCommand command = new SqlCommand(query, this.connect.CreateConnection());
                            this.connect.OpenConnection();
                            command.Parameters.AddWithValue("@ComponentType", compType);
                            command.Parameters.AddWithValue("@ComponentName", compName);
                            command.Parameters.AddWithValue("@propertyname", propertyname);
                            command.Parameters.AddWithValue("@selectedPrettyName", selectedPrettyName);
                            command.Parameters.AddWithValue("@selectedPretty", selectedPretty);
                            command.Parameters.AddWithValue("@ValueString", verifyTestControlItem.TestControlPropertyInitialValueSelectedItem.Trim());
                            SqlDataAdapter dap = new SqlDataAdapter(command);
                            dap.Fill(datatable1);                      
                            this.connect.CloseConnection();
                        }
                    }
                }

                if(MonitorType == "Log Monitoring")
                {
                    if (sourceTestPlanItem != null)
                    {
                        datatable1.Clear();
                        query = "Insert into LogMonitor1 values('" + BMid + "','" + 0 + "','" + sourceTestPlanItem.TestPlanID + "','" +cbmitemsvalue.ParentCBMTestLogItem.iLogIsChecked + "','" + cbmitemsvalue.ParentCBMTestLogItem.iLog_selected_item + "',@ilogText,'" + cbmitemsvalue.ParentCBMTestLogItem.KernelLogIsChecked + "','" + cbmitemsvalue.ParentCBMTestLogItem.kernalLog_selected_item + "',@kernelText,'" + cbmitemsvalue.ParentCBMTestLogItem.EventLogIsChecked + "',@EventLogText,'" + cbmitemsvalue.ParentCBMTestLogItem.ConfiguratorIsChecked + "',@ConfigText,'" + cbmitemsvalue.ParentCBMTestLogItem.SIPLogIsChecked + "',@SPILogText,'" + cbmitemsvalue.ParentCBMTestLogItem.QsysAppLogIsChecked + "',@qsysapplogtext,'" + cbmitemsvalue.ParentCBMTestLogItem.UCIViewerLogIsChecked + "',@UCIlogtext,'" + cbmitemsvalue.ParentCBMTestLogItem.SoftPhoneLogIsChecked + "','0','" + cbmitemsvalue.ParentCBMTestLogItem.WindowsEventLogsIsChecked + "',@windowseventlogtext)";

                        SqlCommand command = new SqlCommand(query, this.connect.CreateConnection());
                        this.connect.OpenConnection();
                        command.Parameters.AddWithValue("@ilogText", cbmitemsvalue.ParentCBMTestLogItem.ilogtext);
                        if (cbmitemsvalue.ParentCBMTestLogItem.ilogtext == null)
                        {
                            command.Parameters[0].Value = string.Empty;
                        }

                        command.Parameters.AddWithValue("@kernelText", cbmitemsvalue.ParentCBMTestLogItem.kernallogtext);
                        if (cbmitemsvalue.ParentCBMTestLogItem.kernallogtext == null)
                        {
                            command.Parameters[1].Value = string.Empty;
                        }

                        command.Parameters.AddWithValue("@EventLogText", cbmitemsvalue.ParentCBMTestLogItem.eventlogtext);
                        if (cbmitemsvalue.ParentCBMTestLogItem.eventlogtext == null)
                        {
                            command.Parameters[2].Value = string.Empty;
                        }

                        command.Parameters.AddWithValue("@ConfigText", cbmitemsvalue.ParentCBMTestLogItem.configuratorlogtext);
                        if (cbmitemsvalue.ParentCBMTestLogItem.configuratorlogtext == null)
                        {
                            command.Parameters[3].Value = string.Empty;
                        }

                        command.Parameters.AddWithValue("@SPILogText", cbmitemsvalue.ParentCBMTestLogItem.siplogtext);
                        if (cbmitemsvalue.ParentCBMTestLogItem.siplogtext == null)
                        {
                            command.Parameters[4].Value = string.Empty;
                        }

                        command.Parameters.AddWithValue("@qsysapplogtext", cbmitemsvalue.ParentCBMTestLogItem.qsysapplogtext);
                        if (cbmitemsvalue.ParentCBMTestLogItem.qsysapplogtext == null)
                        {
                            command.Parameters[5].Value = string.Empty;
                        }

                        command.Parameters.AddWithValue("@UCIlogtext", cbmitemsvalue.ParentCBMTestLogItem.UCIlogtext);
                        if (cbmitemsvalue.ParentCBMTestLogItem.UCIlogtext == null)
                        {
                            command.Parameters[6].Value = string.Empty;
                        }

                        command.Parameters.AddWithValue("@windowseventlogtext", cbmitemsvalue.ParentCBMTestLogItem.windowseventlogtext);
                        if (cbmitemsvalue.ParentCBMTestLogItem.windowseventlogtext == null)
                        {
                            command.Parameters[7].Value = string.Empty;
                        }


                        SqlDataAdapter dap = new SqlDataAdapter(command);
                        dap.Fill(datatable1);
                        this.connect.CloseConnection();
                    }
                }

                return true;

            }
            catch (Exception ex)
             {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif

                //MessageBox.Show("Exception\n " + ex.Message, "Error Code - ECxxxxx", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        public static bool IsValid(DependencyObject parent)
        {
            try
            {
                if (Validation.GetHasError(parent))
                    return false;

                // Validate all the bindings on the children
                for (int i = 0; i != VisualTreeHelper.GetChildrenCount(parent); ++i)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(parent, i);
                    if (!IsValid(child)) { return false; }
                }

                return true;
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif

                //MessageBox.Show("Exception\n \n" + ex.Message, "QAT Error Code - ECxxxx", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        private void ControlMonitorValue(DataTable ConRead, string tableID)
        {
            try
            {
                DataTableReader read_control;
                string query = string.Empty;
                read_control = ConRead.CreateDataReader();
                while (read_control.Read())
                {
                    string MonitorType = read_control.GetString(10);
                    if (MonitorType == "Control Value Monitoring")
                    {
                        
                        //CVMFreq.Text = read_control.GetString(3);
                        //if (CVMFreq.Text != string.Empty || CVMInput.Text != string.Empty)
                        //{
                        //    CVMBackground_chbx1.IsChecked = true;
                        //}

                        //CVMInput.Text = read_control.GetString(4);
                        //CVMBckComboBox.SelectedValue = read_control.GetString(5);
                        string Radiobtn = string.Empty;
                        Radiobtn = read_control.GetString(6);
                        if (Radiobtn == "Continue Testing")
                        {
                            cbmitemsvalue.CVMContinueTesting = true;
                            cbmitemsvalue.CVMPauseatErrorstate = false;
                        }
                        else
                        {
                            cbmitemsvalue.CVMPauseatErrorstate = true;
                            cbmitemsvalue.CVMContinueTesting = false;
                        }
                        //Error Handling
                       // CVMErrorHandlingText1.Text = read_control.GetString(7);
                        cbmitemsvalue.CVMReRunFailedTestCase = read_control.GetString(7);
                        //CVMErrorHandlingCombo.SelectedValue = read_control.GetString(9);
                        string CVMQuery = "Select * from ControlMonitor where TPID='" + tableID + "'";
                        ReadControlMonitor(CVMQuery);
                    }
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

        private void ReadControlMonitor(string ControlQuery)
        {
            DataTable controldatatable = null;
            SqlDataAdapter adap = null;
            DataTableReader conread = null;
            try
            {
                string status = string.Empty;
                
                controldatatable = new DataTable();
                controldatatable.Clear();
                adap = new SqlDataAdapter(ControlQuery, this.connect.CreateConnection());
                this.connect.OpenConnection();
                adap.Fill(controldatatable);
                cbmitemsvalue.VerifyTestControlList.Clear();
                if (controldatatable.Rows.Count > 0)
                {
                    conread = controldatatable.CreateDataReader();
                    if (conread.HasRows)
                    {
                        while (conread.Read())
                    {
                        status = conread.GetString(10);
                        if (status == "False")
                        {
                            Control_Value_Monitoring.IsChecked = false;
                            return;
                        }
                        else
                        {
                            Control_Value_Monitoring.IsChecked = true;                            
                        }
                        
                        if (conread.GetString(4).ToString()!=string.Empty && conread[4] != System.DBNull.Value)
                        {
                                //if(!cbmitemsvalue.cmb_ComponentType.Contains(conread.GetString(4)))
                                //cbmitemsvalue.cmb_ComponentType.Add(conread.GetString(4));
                                cbmitemsvalue.cmb_ComponentTypeSelectedItem = conread.GetString(4);



                                CBMTestControlItem testControlItem = AddVerifyTestControlItem();

                                //foreach (var verifyTestControl in cbmitemsvalue.VerifyTestControlList)
                                //{
                                //    verifyTestControl.TestControlComponentTypeList = new ObservableCollection<string>(cbmitemsvalue.ComponentTypeList);
                                //    verifyTestControl.InputSelectionEnabled = false;
                                //    verifyTestControl.ChannelEnabled = false;
                                //    verifyTestControl.MaxLimitIsEnabled = false;
                                //    verifyTestControl.MinLimitIsEnabled = false;
                                //    verifyTestControl.MinimumLimit = string.Empty;
                                //    verifyTestControl.MaximumLimit = string.Empty;
                                //}

                                string componentType = conread.GetString(4).ToString();
                                if (testControlItem.TestControlComponentTypeList.Contains(componentType, StringComparer.CurrentCultureIgnoreCase))
                                {
                                    testControlItem.TestControlComponentTypeSelectedItem = componentType;

                                    string componentName = conread.GetString(5).ToString();
                                    
                                    if (testControlItem.TestControlComponentNameList.Contains(componentName, StringComparer.CurrentCultureIgnoreCase))
                                    {
                                        testControlItem.TestControlComponentNameSelectedItem = componentName;

                                        string componentProperty = string.Empty;
                                        if (conread[9] != System.DBNull.Value)
                                        {
                                            componentProperty = conread.GetString(9).ToString().Trim();
                                            if (componentProperty.Contains("~")) /*(((componentProperty.Contains("Channel"))|| (componentProperty.Contains("Output"))|| (componentProperty.Contains("Input")) || (componentProperty.Contains("Tap")) || (componentProperty.Contains("Bank Control")) || (componentProperty.Contains("Bank Select")) || (componentProperty.Contains("GPIO"))) & (componentProperty.Contains("~")))*/
                                            {
                                                string[] channelSplit = new string[2];
                                                string channelControl = string.Empty;
                                                int tiltCount = componentProperty.Count(x => x == '~');
                                                string channelWithTwoTilt = componentProperty;
                                                int idx = channelWithTwoTilt.LastIndexOf('~');
                                                channelSplit[0] = channelWithTwoTilt.Substring(0, idx);
                                                channelSplit[1] = channelWithTwoTilt.Substring(idx + 1);
                                                string QATPrefix = QscDatabase.addQATPrefixToControl(componentProperty);//Added on 30-sep-2016
                                                if (!string.IsNullOrEmpty(QATPrefix))//Added on 30-sep-2016
                                                    componentProperty = channelSplit[1];
                                            }
                                        }

                                        string controlValue = string.Empty;
                                        if(conread[6] != null && conread.GetValue(6) != null && conread.GetValue(6).ToString() != string.Empty)
                                        {
                                            controlValue = conread.GetValue(6).ToString();
                                        }
                                                                                
                                        if (testControlItem.VerifyTestControlPropertyList.Contains(componentProperty, StringComparer.CurrentCultureIgnoreCase))
                                        {
                                            testControlItem.TestControlPropertySelectedItem = componentProperty;
                                        }
                                        else if (controlValue != string.Empty && testControlItem.VerifyTestControlPropertyList.Contains(componentProperty + " [" + controlValue + "]", StringComparer.CurrentCultureIgnoreCase))
                                        {
                                            testControlItem.TestControlPropertySelectedItem = componentProperty + " [" + controlValue + "]";
                                        }

                                        string selectionType = string.Empty;

                                        string selectedChannel = string.Empty;
                                        if (conread[7] != System.DBNull.Value)
                                        {
                                            selectedChannel = conread.GetString(7).ToString();
                                        }

                                        if (testControlItem.ChannelSelectionList.Contains(selectedChannel, StringComparer.CurrentCultureIgnoreCase))
                                        {
                                            testControlItem.ChannelSelectionSelectedItem = selectedChannel;
                                        }
                                        else if (testControlItem.ChannelSelectionList.Contains(selectedChannel + " [" + controlValue + "]", StringComparer.CurrentCultureIgnoreCase))
                                        {
                                            testControlItem.ChannelSelectionSelectedItem = selectedChannel + " [" + controlValue + "]";
                                        }

                                        if (conread[11] != System.DBNull.Value)
                                        {
                                            selectionType = conread.GetString(11).ToString();
                                            if (String.Equals("Set by string", selectionType, StringComparison.CurrentCultureIgnoreCase))
                                            {
                                                testControlItem.InputSelectionComboSelectedItem = selectionType;
                                                testControlItem.TestControlPropertyInitialValueSelectedItem = conread.GetString(8).ToString();
                                            }

                                            else if (String.Equals("Set by value", selectionType, StringComparison.CurrentCultureIgnoreCase))
                                            {
                                                testControlItem.InputSelectionComboSelectedItem = selectionType;
                                                testControlItem.TestControlPropertyInitialValueSelectedItem = conread.GetString(8).ToString();
                                            }
                                            else if (String.Equals("Set by position", selectionType, StringComparison.CurrentCultureIgnoreCase))
                                            {
                                                testControlItem.InputSelectionComboSelectedItem = selectionType;
                                                testControlItem.TestControlPropertyInitialValueSelectedItem = conread.GetString(8).ToString();
                                            }
                                        }

                                        if (conread[12] != System.DBNull.Value)
                                        {
                                            testControlItem.MaximumLimit = conread.GetString(12).ToString();
                                        }
                                        if (conread[13] != System.DBNull.Value)
                                        {
                                            testControlItem.MinimumLimit = conread.GetString(13).ToString();
                                        }

                                        if (testControlItem.ChannelSelectionList.Count > 0)
                                        {
                                            if (conread[14] != System.DBNull.Value)
                                            {
                                                if (String.Equals(conread.GetString(14).ToString(), "True", StringComparison.CurrentCultureIgnoreCase))
                                                    testControlItem.LoopIsChecked = true;
                                                else
                                                    testControlItem.LoopIsChecked = false;
                                            }

                                            if (conread[15] != System.DBNull.Value)
                                            {
                                                testControlItem.LoopStart = conread.GetString(15).ToString();
                                            }

                                            if (conread[16] != System.DBNull.Value && conread[16].ToString() != string.Empty)
                                            {
                                                if (testControlItem.LoopIsChecked && testControlItem.ChannelSelectionList.Count >= Convert.ToInt32(conread[16]))
                                                    testControlItem.LoopEnd = conread[16].ToString();
                                                else
                                                    testControlItem.LoopEnd = string.Empty;
                                            }

                                            if (conread[17] != System.DBNull.Value && conread[17].ToString() != string.Empty)
                                            {
                                                if (testControlItem.LoopIsChecked && testControlItem.LoopEnd != null && testControlItem.LoopEnd != string.Empty)
                                                {
                                                    if ((Convert.ToInt32(conread[17]) > 0) && (Convert.ToInt32(conread[17]) <= testControlItem.ChannelSelectionList.Count) && (Convert.ToInt32(conread[17]) <= Convert.ToInt32(testControlItem.LoopEnd)))
                                                    {
                                                        testControlItem.LoopIncrement = conread[17].ToString();
                                                    }
                                                }
                                                else
                                                {
                                                    testControlItem.LoopIncrement = string.Empty;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    this.connect.CloseConnection();
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

        private void cmb_ComponentType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                string s = cbmitemsvalue.cmb_ComponentTypeSelectedItem;
                if(clearValue==false)
                {
                    cbmitemsvalue.ComponentNameList.Clear();
                    cbmitemsvalue.PropertyList.Clear();
                    cbmitemsvalue.ComponentNameSelectedItem = null;
                    cbmitemsvalue.PropertySelectedItem = null;

                    cbmitemsvalue.cmb_Channel.Clear();
                    cbmitemsvalue.cmb_ChannelSelectedItem = string.Empty;

                   // cbmitemsvalue.InputSelectionComboList.Clear();
                    cbmitemsvalue.InputSelectionComboSelectedItem = null;

                    cbmitemsvalue.cmb_Channel.Clear();
                    cbmitemsvalue.cmb_ChannelSelectedItem = null;

                    cbmitemsvalue.PropertyInitialValueSelectedItem = string.Empty;

                    cbmitemsvalue.MaximumLimit = string.Empty;
                    cbmitemsvalue.MinimumLimit = string.Empty;

                    cbmitemsvalue.cmb_ComponentTypeSelectedItem = s;
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

        private void CBM_ContentRendered(object sender, EventArgs e)
        {
            try
            {
                // Telnetactiondata.DataContext = null;
                telnetverifydata.DataContext = null;
                ErrorHandlingdata.DataContext = null;

                //Telnetactiondata.DataContext = cbmitemsvalue;
                telnetverifydata.DataContext = cbmitemsvalue;
                ErrorHandlingdata.DataContext = cbmitemsvalue;

                VerifyControlContentDataTemplareData.DataContext = null;
                CVMErrorHandlingdata.DataContext = null;
                clearValue = true;
                VerifyControlContentDataTemplareData.DataContext = cbmitemsvalue;
                CVMErrorHandlingdata.DataContext = cbmitemsvalue;
                clearValue = false;
                InventoryErrorHandling.DataContext = cbmitemsvalue;
                LogErrorHandling.DataContext = cbmitemsvalue;
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

        private void cmb_ComponentName_DropDownClosed(object sender, EventArgs e)
        {
            try
            {
                if (cbmitemsvalue.ComponentNameSelectedItem == null)
                {
                    cbmitemsvalue.PropertyList.Clear();
                    cbmitemsvalue.PropertySelectedItem = null;
                    cbmitemsvalue.PropertyInitialValueSelectedItem = null;
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

        private static bool IsboolAllowed(string text)
        {
            Regex regex = null;

            try
            {
                regex = new Regex("^[0-1]"); ////regex that matches disallowed text
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

        private void cmb_Value_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            try
            {
                FrameworkElement sourceElement = sender as FrameworkElement;
                if (sourceElement == null)
                    return;

                TestCaseItem sourceTestCaseItem = sourceElement.DataContext as TestCaseItem;
                if (sourceTestCaseItem == null)
                    return;

                if (sourceTestCaseItem != null)
                {
                    TestControlItem originalTestControlItem = null;
                    if (e.OriginalSource.GetType().ToString() == "System.Windows.Controls.TextBox")
                    {
                        var sourceTextBox = (TextBox)e.OriginalSource;
                        if (sourceTextBox != null && sourceTextBox.DataContext != null && String.Equals(sourceTextBox.DataContext.GetType().ToString(), "QSC_Test_Automation.TestControlItem"))
                            originalTestControlItem = (TestControlItem)sourceTextBox.DataContext;
                        string controlSelected = string.Empty;
                        if ((!string.IsNullOrEmpty(originalTestControlItem.TestControlPropertySelectedItem)))// & (!string.IsNullOrEmpty((originalTestControlItem.ChannelSelectionSelectedItem))))
                        {
                            if ((!string.IsNullOrEmpty(originalTestControlItem.TestControlPropertySelectedItem))  & (!string.IsNullOrEmpty((originalTestControlItem.ChannelSelectionSelectedItem))))//& ((originalTestControlItem.TestControlPropertySelectedItem.StartsWith("CHANNEL ")) || (originalTestControlItem.TestControlPropertySelectedItem.StartsWith("OUTPUT ")) || (originalTestControlItem.TestControlPropertySelectedItem.StartsWith("INPUT ")) || (originalTestControlItem.TestControlPropertySelectedItem.StartsWith("INPUT_OUTPUT ")) || (originalTestControlItem.TestControlPropertySelectedItem.StartsWith("TAP ")) || (originalTestControlItem.TestControlPropertySelectedItem.StartsWith("BANK_CONTROL ")) || (originalTestControlItem.TestControlPropertySelectedItem.StartsWith("BANK_SELECT ")) || (originalTestControlItem.TestControlPropertySelectedItem.StartsWith("GPIO_INPUT ")) || (originalTestControlItem.TestControlPropertySelectedItem.StartsWith("GPIO_OUTPUT ")))
                            {
                                //if (originalTestControlItem.TestControlPropertySelectedItem.StartsWith("CHANNEL "))
                                    controlSelected = originalTestControlItem.ChannelSelectionSelectedItem + "~" +QscDatabase.removeQATPrefix( originalTestControlItem.TestControlPropertySelectedItem);
                                //else if (originalTestControlItem.TestControlPropertySelectedItem.StartsWith("OUTPUT "))
                                //    controlSelected = originalTestControlItem.ChannelSelectionSelectedItem + "~" + originalTestControlItem.TestControlPropertySelectedItem.Remove(0, 7);
                                //else if (originalTestControlItem.TestControlPropertySelectedItem.StartsWith("INPUT "))
                                //    controlSelected = originalTestControlItem.ChannelSelectionSelectedItem + "~" + originalTestControlItem.TestControlPropertySelectedItem.Remove(0, 6);
                                //else if (originalTestControlItem.TestControlPropertySelectedItem.StartsWith("INPUT_OUTPUT "))
                                //    controlSelected = originalTestControlItem.ChannelSelectionSelectedItem + "~" + originalTestControlItem.TestControlPropertySelectedItem.Remove(0, 13);
                                //else if (originalTestControlItem.TestControlPropertySelectedItem.StartsWith("TAP "))
                                //    controlSelected = originalTestControlItem.ChannelSelectionSelectedItem + "~" + originalTestControlItem.TestControlPropertySelectedItem.Remove(0, 4);
                                //else if (originalTestControlItem.TestControlPropertySelectedItem.StartsWith("BANK_CONTROL_INPUT "))
                                //    controlSelected = originalTestControlItem.ChannelSelectionSelectedItem + "~" + originalTestControlItem.TestControlPropertySelectedItem.Remove(0, 19);
                                //else if (originalTestControlItem.TestControlPropertySelectedItem.StartsWith("BANK_CONTROL_OUTPUT "))
                                //    controlSelected = originalTestControlItem.ChannelSelectionSelectedItem + "~" + originalTestControlItem.TestControlPropertySelectedItem.Remove(0, 20);
                                //else if (originalTestControlItem.TestControlPropertySelectedItem.StartsWith("BANK_CONTROL_INPUT_OUTPUT "))
                                //    controlSelected = originalTestControlItem.ChannelSelectionSelectedItem + "~" + originalTestControlItem.TestControlPropertySelectedItem.Remove(0, 26);
                                //else if (originalTestControlItem.TestControlPropertySelectedItem.StartsWith("BANK_SELECT_INPUT "))
                                //    controlSelected = originalTestControlItem.ChannelSelectionSelectedItem + "~" + originalTestControlItem.TestControlPropertySelectedItem.Remove(0, 18);
                                //else if (originalTestControlItem.TestControlPropertySelectedItem.StartsWith("BANK_SELECT_OUTPUT "))
                                //    controlSelected = originalTestControlItem.ChannelSelectionSelectedItem + "~" + originalTestControlItem.TestControlPropertySelectedItem.Remove(0, 19);
                                //else if (originalTestControlItem.TestControlPropertySelectedItem.StartsWith("GPIO_INPUT "))
                                //    controlSelected = originalTestControlItem.ChannelSelectionSelectedItem + "~" + originalTestControlItem.TestControlPropertySelectedItem.Remove(0, 11);
                                //else if (originalTestControlItem.TestControlPropertySelectedItem.StartsWith("GPIO_OUTPUT "))
                                //    controlSelected = originalTestControlItem.ChannelSelectionSelectedItem + "~" + originalTestControlItem.TestControlPropertySelectedItem.Remove(0, 12);
                            }
                            else
                            {
                                controlSelected = originalTestControlItem.TestControlPropertySelectedItem;
                            }

                            if ((sourceTestCaseItem.dataTypeDictionary.Keys.Contains(originalTestControlItem.TestControlComponentNameSelectedItem+controlSelected)) | (sourceTestCaseItem.VerifyDataTypeDictionary.Keys.Contains(originalTestControlItem.TestControlComponentNameSelectedItem + controlSelected)))
                            {
                                string valueType = string.Empty;
                                sourceTestCaseItem.dataTypeDictionary.TryGetValue(originalTestControlItem.TestControlComponentNameSelectedItem + controlSelected, out valueType);
                                if ((valueType == string.Empty) | (valueType == null))
                                {
                                    sourceTestCaseItem.VerifyDataTypeDictionary.TryGetValue(originalTestControlItem.TestControlComponentNameSelectedItem + controlSelected, out valueType);
                                }
                                if ((String.Equals("Float", valueType, StringComparison.InvariantCultureIgnoreCase)) & ((String.Equals("set by value", originalTestControlItem.InputSelectionComboSelectedItem, StringComparison.InvariantCultureIgnoreCase))))// | (String.Equals("set by position", originalTestControlItem.InputSelectionComboSelectedItem, StringComparison.InvariantCultureIgnoreCase))))
                                {
                                    TextBox btn = (TextBox)sender;
                                    string str = btn.Text;
                                    int SelectionSort = btn.SelectionStart;

                                    if (keyValue == "Decimal" || keyValue == "OemPeriod")
                                    {
                                        if (!str.Contains('.') == true)
                                        {
                                            e.Handled = !IsTextAllowedForDecimal(e.Text);

                                        }
                                    }

                                    if ((keyValue == "Subtract" || keyValue == "OemMinus") & SelectionSort == 0)
                                    {

                                        if (!str.Contains('-') == true)
                                        {
                                            e.Handled = !IsTextAllowedForDecimal(e.Text);
                                        }

                                    }
                                    else
                                    {
                                        if (keyValue != "Subtract")
                                        {
                                            if (keyValue != "OemMinus")
                                            {
                                                e.Handled = !IsTextAllowedForDecimal(e.Text);
                                            }
                                            else
                                            {
                                                e.Handled = true;
                                            }
                                        }
                                        else
                                        {
                                            e.Handled = true;
                                        }
                                    }
                                }
                                else if (String.Equals("set by position", originalTestControlItem.InputSelectionComboSelectedItem, StringComparison.InvariantCultureIgnoreCase))// | (String.Equals("set by position", originalTestControlItem.InputSelectionComboSelectedItem, StringComparison.InvariantCultureIgnoreCase))))
                                {
                                    TextBox btn = (TextBox)sender;
                                    string str = btn.Text;
                                    int SelectionSort = btn.SelectionStart;

                                    if (keyValue == "Decimal" || keyValue == "OemPeriod")
                                    {
                                        if (!str.Contains('.') == true)
                                        {
                                            e.Handled = !IsTextAllowedForDecimal(e.Text);

                                        }
                                    }

                                    if ((keyValue == "Subtract" || keyValue == "OemMinus") & SelectionSort == 0)
                                    {

                                        if (!str.Contains('-') == true)
                                        {
                                            e.Handled = !IsTextAllowedForDecimal(e.Text);
                                        }

                                    }
                                    else
                                    {
                                        if (keyValue != "Subtract")
                                        {
                                            if (keyValue != "OemMinus")
                                            {
                                                e.Handled = !IsTextAllowedForDecimal(e.Text);
                                            }
                                            else
                                            {
                                                e.Handled = true;
                                            }
                                        }
                                        else
                                        {
                                            e.Handled = true;
                                        }
                                    }

                                }
                                else if ((String.Equals("Integer", valueType, StringComparison.InvariantCultureIgnoreCase)) & ((String.Equals("set by value", originalTestControlItem.InputSelectionComboSelectedItem, StringComparison.InvariantCultureIgnoreCase))))// | (String.Equals("set by position", originalTestControlItem.InputSelectionComboSelectedItem, StringComparison.InvariantCultureIgnoreCase))))
                                {
                                    e.Handled = false;

                                }
                                else if ((String.Equals("Unknown", valueType, StringComparison.InvariantCultureIgnoreCase)) & ((String.Equals("set by value", originalTestControlItem.InputSelectionComboSelectedItem, StringComparison.InvariantCultureIgnoreCase))))// | (String.Equals("set by position", originalTestControlItem.InputSelectionComboSelectedItem, StringComparison.InvariantCultureIgnoreCase))))
                                {
                                    e.Handled = false;

                                }
                                else if ((String.Equals("Text", valueType, StringComparison.InvariantCultureIgnoreCase)) & ((String.Equals("set by value", originalTestControlItem.InputSelectionComboSelectedItem, StringComparison.InvariantCultureIgnoreCase))))// | (String.Equals("set by position", originalTestControlItem.InputSelectionComboSelectedItem, StringComparison.InvariantCultureIgnoreCase))))
                                {
                                    e.Handled = false;

                                }
                                else if ((String.Equals("Boolean", valueType, StringComparison.InvariantCultureIgnoreCase)) & ((String.Equals("set by value", originalTestControlItem.InputSelectionComboSelectedItem, StringComparison.InvariantCultureIgnoreCase)) | (String.Equals("set by position", originalTestControlItem.InputSelectionComboSelectedItem, StringComparison.InvariantCultureIgnoreCase))))
                                {
                                    TextBox btn = (TextBox)sender;
                                    string str = btn.Text;
                                    if (str.Length == 0)
                                    {
                                        if (keyValue == "NumPad1" || keyValue == "D1")
                                        {
                                            if (!str.Contains('1') == true)
                                            {
                                                e.Handled = IsboolAllowed(e.Text);
                                            }
                                            else
                                            {
                                                e.Handled = true;
                                            }
                                        }
                                        else if (keyValue == "NumPad0" || keyValue == "D0")
                                        {
                                            if (!str.Contains('0') == true)
                                            {
                                                e.Handled = IsboolAllowed(e.Text);
                                            }
                                            else
                                            {
                                                e.Handled = true;
                                            }
                                        }
                                        else
                                        {
                                            e.Handled = false;
                                        }
                                    }
                                    else
                                    {
                                        e.Handled = false;
                                    }

                                }
                                else if ((String.Equals("Trigger", valueType, StringComparison.InvariantCultureIgnoreCase)) & ((String.Equals("set by value", originalTestControlItem.InputSelectionComboSelectedItem, StringComparison.InvariantCultureIgnoreCase))))// | (String.Equals("set by position", originalTestControlItem.InputSelectionComboSelectedItem, StringComparison.InvariantCultureIgnoreCase))))
                                {
                                    TextBox btn = (TextBox)sender;
                                    string str = btn.Text;
                                    if (str.Length == 0)
                                    {
                                        if (keyValue == "NumPad1" || keyValue == "D1")
                                        {
                                            if (!str.Contains('1') == true)
                                            {
                                                e.Handled = IsboolAllowed(e.Text);
                                            }
                                            else
                                            {
                                                e.Handled = true;
                                            }
                                        }
                                        else if (keyValue == "NumPad0" || keyValue == "D0")
                                        {
                                            if (!str.Contains('0') == true)
                                            {
                                                e.Handled = IsboolAllowed(e.Text);
                                            }
                                            else
                                            {
                                                e.Handled = true;
                                            }
                                        }
                                        else
                                        {
                                            e.Handled = false;
                                        }
                                    }
                                    else
                                    {
                                        e.Handled = false;
                                    }

                                }
                                else
                                {
                                    e.Handled = false;
                                }
                            }

                            //}
                            //else if ((string.IsNullOrEmpty(originalTestControlItem.TestControlPropertySelectedItem)) & (string.IsNullOrEmpty((originalTestControlItem.ChannelSelectionSelectedItem))))
                            //{
                            //    e.Handled = true;  
                            //}
                            //else if ((string.IsNullOrEmpty(originalTestControlItem.TestControlPropertySelectedItem)))
                            //{
                            //    e.Handled = true;
                            //}
                            //else if ((string.IsNullOrEmpty(originalTestControlItem.ChannelSelectionSelectedItem)) & (!originalTestControlItem.ChannelEnabled))
                            //{
                            //    e.Handled = true; 
                            //}
                        }
                        else
                        {
                            e.Handled = true;
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

                //MessageBox.Show("Exception\n " + ex.Message, "QAT Error Code - EC12110", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        private string keyValue = null;

        private void cmb_Value_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                FrameworkElement sourceElement = sender as FrameworkElement;
                if (sourceElement == null)
                    return;

                TestCaseItem sourceTestCaseItem = sourceElement.DataContext as TestCaseItem;
                if (sourceTestCaseItem == null)
                    return;

                if (sourceTestCaseItem != null)
                {
                    TestControlItem originalTestControlItem = null;
                    if (e.OriginalSource.GetType().ToString() == "System.Windows.Controls.TextBox")
                    {
                        var sourceTextBox = (TextBox)e.OriginalSource;
                        if (sourceTextBox != null && sourceTextBox.DataContext != null && String.Equals(sourceTextBox.DataContext.GetType().ToString(), "QSC_Test_Automation.TestControlItem"))
                            originalTestControlItem = (TestControlItem)sourceTextBox.DataContext;
                        string controlSelected = string.Empty;
                        if ((!string.IsNullOrEmpty(originalTestControlItem.TestControlPropertySelectedItem)))
                        {

                            if ((!string.IsNullOrEmpty(originalTestControlItem.TestControlPropertySelectedItem))  & (!string.IsNullOrEmpty((originalTestControlItem.ChannelSelectionSelectedItem))))//& ((originalTestControlItem.TestControlPropertySelectedItem.StartsWith("CHANNEL ")) || (originalTestControlItem.TestControlPropertySelectedItem.StartsWith("OUTPUT ")) || (originalTestControlItem.TestControlPropertySelectedItem.StartsWith("INPUT ")) || (originalTestControlItem.TestControlPropertySelectedItem.StartsWith("INPUT_OUTPUT ")) || (originalTestControlItem.TestControlPropertySelectedItem.StartsWith("TAP ")) || (originalTestControlItem.TestControlPropertySelectedItem.StartsWith("BANK_CONTROL ")) || (originalTestControlItem.TestControlPropertySelectedItem.StartsWith("BANK_SELECT ")) || (originalTestControlItem.TestControlPropertySelectedItem.StartsWith("GPIO_INPUT ")) || (originalTestControlItem.TestControlPropertySelectedItem.StartsWith("GPIO_OUTPUT ")))
                            {
                                //if (originalTestControlItem.TestControlPropertySelectedItem.StartsWith("CHANNEL "))
                                    controlSelected = originalTestControlItem.ChannelSelectionSelectedItem + "~" +QscDatabase.removeQATPrefix( originalTestControlItem.TestControlPropertySelectedItem);
                                //else if (originalTestControlItem.TestControlPropertySelectedItem.StartsWith("OUTPUT "))
                                //    controlSelected = originalTestControlItem.ChannelSelectionSelectedItem + "~" + originalTestControlItem.TestControlPropertySelectedItem.Remove(0, 7);
                                //else if (originalTestControlItem.TestControlPropertySelectedItem.StartsWith("INPUT "))
                                //    controlSelected = originalTestControlItem.ChannelSelectionSelectedItem + "~" + originalTestControlItem.TestControlPropertySelectedItem.Remove(0, 6);
                                //else if (originalTestControlItem.TestControlPropertySelectedItem.StartsWith("INPUT_OUTPUT "))
                                //    controlSelected = originalTestControlItem.ChannelSelectionSelectedItem + "~" + originalTestControlItem.TestControlPropertySelectedItem.Remove(0, 13);
                                //else if (originalTestControlItem.TestControlPropertySelectedItem.StartsWith("TAP "))
                                //    controlSelected = originalTestControlItem.ChannelSelectionSelectedItem + "~" + originalTestControlItem.TestControlPropertySelectedItem.Remove(0, 4);
                                //else if (originalTestControlItem.TestControlPropertySelectedItem.StartsWith("BANK_CONTROL_INPUT "))
                                //    controlSelected = originalTestControlItem.ChannelSelectionSelectedItem + "~" + originalTestControlItem.TestControlPropertySelectedItem.Remove(0, 19);
                                //else if (originalTestControlItem.TestControlPropertySelectedItem.StartsWith("BANK_CONTROL_OUTPUT "))
                                //    controlSelected = originalTestControlItem.ChannelSelectionSelectedItem + "~" + originalTestControlItem.TestControlPropertySelectedItem.Remove(0, 20);
                                //else if (originalTestControlItem.TestControlPropertySelectedItem.StartsWith("BANK_CONTROL_INPUT_OUTPUT "))
                                //    controlSelected = originalTestControlItem.ChannelSelectionSelectedItem + "~" + originalTestControlItem.TestControlPropertySelectedItem.Remove(0, 26);
                                //else if (originalTestControlItem.TestControlPropertySelectedItem.StartsWith("BANK_SELECT_INPUT "))
                                //    controlSelected = originalTestControlItem.ChannelSelectionSelectedItem + "~" + originalTestControlItem.TestControlPropertySelectedItem.Remove(0, 18);
                                //else if (originalTestControlItem.TestControlPropertySelectedItem.StartsWith("BANK_SELECT_OUTPUT "))
                                //    controlSelected = originalTestControlItem.ChannelSelectionSelectedItem + "~" + originalTestControlItem.TestControlPropertySelectedItem.Remove(0, 19);
                                //else if (originalTestControlItem.TestControlPropertySelectedItem.StartsWith("GPIO_INPUT "))
                                //    controlSelected = originalTestControlItem.ChannelSelectionSelectedItem + "~" + originalTestControlItem.TestControlPropertySelectedItem.Remove(0, 11);
                                //else if (originalTestControlItem.TestControlPropertySelectedItem.StartsWith("GPIO_OUTPUT "))
                                //    controlSelected = originalTestControlItem.ChannelSelectionSelectedItem + "~" + originalTestControlItem.TestControlPropertySelectedItem.Remove(0, 12);
                            }
                            else
                            {
                                controlSelected = originalTestControlItem.TestControlPropertySelectedItem;
                            }

                            TextBox btn = (TextBox)sender;
                            string str = btn.Text;

                            keyValue = e.Key.ToString();

                            int SelectionSort = btn.SelectionStart;

                            if ((sourceTestCaseItem.dataTypeDictionary.Keys.Contains(originalTestControlItem.TestControlComponentNameSelectedItem + controlSelected)) | (sourceTestCaseItem.VerifyDataTypeDictionary.Keys.Contains(originalTestControlItem.TestControlComponentNameSelectedItem + controlSelected)))
                            {
                                string valueType = string.Empty;
                                sourceTestCaseItem.dataTypeDictionary.TryGetValue(originalTestControlItem.TestControlComponentNameSelectedItem + controlSelected, out valueType);
                                if ((valueType == string.Empty) | (valueType == null))
                                {
                                    sourceTestCaseItem.VerifyDataTypeDictionary.TryGetValue(originalTestControlItem.TestControlComponentNameSelectedItem + controlSelected, out valueType);
                                }
                                if ((String.Equals("Float", valueType, StringComparison.InvariantCultureIgnoreCase)) & ((String.Equals("set by value", originalTestControlItem.InputSelectionComboSelectedItem, StringComparison.InvariantCultureIgnoreCase))))
                                {
                                    if (keyValue == "Decimal" || keyValue == "OemPeriod")
                                    {
                                        if (!str.Contains('.') == true)
                                        {

                                            if (e.Handled == IsTextAllowedForDecimal(e.ToString()))
                                            {
                                                if (e.Key == Key.Space)
                                                {
                                                    e.Handled = true;
                                                }
                                                base.OnPreviewKeyDown(e);
                                            }
                                        }
                                        else
                                        {
                                            e.Handled = true;
                                        }
                                    }

                                    if ((keyValue == "Subtract" || keyValue == "OemMinus") && SelectionSort == 0)
                                    {
                                        if (!str.Contains('-') == true)
                                        {

                                            if (e.Handled == IsTextAllowedForDecimal(e.ToString()))
                                            {
                                                if (e.Key == Key.Space)
                                                {
                                                    e.Handled = true;
                                                }
                                                base.OnPreviewKeyDown(e);
                                            }
                                        }
                                        else
                                        {
                                            e.Handled = true;
                                        }
                                    }
                                    else
                                    {
                                        if (keyValue != "Subtract")
                                        {
                                            if (keyValue != "OemMinus")
                                            {
                                                if (e.Handled == IsTextAllowedForDecimal(e.ToString()))
                                                {
                                                    if (e.Key == Key.Space)
                                                    {
                                                        e.Handled = true;
                                                    }
                                                    base.OnPreviewKeyDown(e);
                                                }
                                            }
                                            else
                                            {
                                                e.Handled = true;
                                            }

                                        }
                                        else
                                        {
                                            e.Handled = true;
                                        }

                                    }
                                }
                            }

                            //}
                            //else if ((string.IsNullOrEmpty(originalTestControlItem.TestControlPropertySelectedItem)) & (string.IsNullOrEmpty((originalTestControlItem.ChannelSelectionSelectedItem))))
                            //{
                            //    MessageBox.Show("Select Control and Channel property", "QAT Error Code - EC12111A", MessageBoxButton.OK, MessageBoxImage.Error);
                            //}
                            //else if ((string.IsNullOrEmpty(originalTestControlItem.TestControlPropertySelectedItem)))
                            //{
                            //    MessageBox.Show("Select Control property", "QAT Error Code - EC12111B", MessageBoxButton.OK, MessageBoxImage.Error);
                            //}
                            //else if ((string.IsNullOrEmpty(originalTestControlItem.ChannelSelectionSelectedItem)) & (!originalTestControlItem.ChannelEnabled))
                            //{
                            //    MessageBox.Show("Select Channel property", "QAT Error Code - EC12111C", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                        else
                        {
                            e.Handled = false;
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

                //MessageBox.Show("Exception\n " + ex.Message, "QAT Error Code - EC12111", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private static bool IsTextAllowedForDecimal(string text)
        {
            Regex regex = null;

            try
            {
                regex = new Regex(@"^-?[0-9]*(?:\.[0-9]*)?$");///regex that matches disallowed text

            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif

                //MessageBox.Show("Exception\n " + ex.Message, "QAT Error Code - EC12019", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return regex.IsMatch(text);

        }

        private void RerunthefailedTestCase_textBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
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

                //MessageBox.Show("Exception\n " + ex.Message, "QAT Error Code - ECxxxxx", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RerunthefailedTestCase_textBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                string lstrCopyandPasteTxtBox = null;
                if (e.Handled == IsTextAllowed(e.ToString()))
                {
                    if (e.Key == Key.Space)
                    {
                        e.Handled = true;
                    }
                    base.OnPreviewKeyDown(e);
                }
                //copy and Paste               
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

                //MessageBox.Show("Exception\n " + ex.Message, "QAT Error Code - ECxxxxx", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        private static bool IsTextAllowed(string text)
        {
            Regex regex = null;

            try
            {
                regex = new Regex("[^0-9]+"); ////regex that matches disallowed text
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif

                //MessageBox.Show("Exception\n " + ex.Message, "QAT Error Code - ECxxxxx", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return !regex.IsMatch(text);
        }

        private void cmb_UpperLimit_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                TextBox btn = (TextBox)sender;
                string str = btn.Text;
                keyValue = e.Key.ToString();
                int SelectionSort = btn.SelectionStart;
                if (keyValue == "V" && Keyboard.Modifiers == ModifierKeys.Control)
                {
                    e.Handled = false;
                    string cliptext = Clipboard.GetText();
                    if (e.Handled == IsTextAllowed(cliptext))
                    {
                        e.Handled = true;
                    }
                    base.OnPreviewKeyDown(e);
                }
                if (keyValue == "Decimal" || keyValue == "OemPeriod")
                {
                    if (!str.Contains('.') == true)
                    {

                        if (e.Handled == IsTextAllowedForDecimal(e.ToString()))
                        {
                            if (e.Key == Key.Space)
                            {
                                e.Handled = true;
                            }
                            base.OnPreviewKeyDown(e);
                        }
                    }
                    else
                    {
                        e.Handled = true;
                    }
                }

                if ((keyValue == "Subtract" || keyValue == "OemMinus") && SelectionSort == 0)
                {
                    if (!str.Contains('-') == true)
                    {

                        if (e.Handled == IsTextAllowedForDecimal(e.ToString()))
                        {
                            if (e.Key == Key.Space)
                            {
                                e.Handled = true;
                            }
                            base.OnPreviewKeyDown(e);
                        }
                    }
                    else
                    {
                        e.Handled = true;
                    }
                }
                else
                {
                    if (keyValue != "Subtract")
                    {
                        if (keyValue != "OemMinus")
                        {
                            if (e.Handled == IsTextAllowedForDecimal(e.ToString()))
                            {
                                if (e.Key == Key.Space)
                                {
                                    e.Handled = true;
                                }
                                base.OnPreviewKeyDown(e);
                            }
                        }
                        else
                        {
                            e.Handled = true;
                        }
                    }
                    else
                    {
                        e.Handled = true;
                    }
                }

            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif

                //MessageBox.Show("Exception\n " + ex.Message, "QAT Error Code - EC12111", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void cmb_UpperLimit_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            try
            {
                TextBox btn = (TextBox)sender;
                string str = btn.Text;
                int SelectionSort = btn.SelectionStart;

                if (keyValue == "Decimal" || keyValue == "OemPeriod")
                {
                    if (!str.Contains('.') == true)
                    {
                        e.Handled = !IsTextAllowedForDecimal(e.Text);

                    }
                }

                if ((keyValue == "Subtract" || keyValue == "OemMinus") & SelectionSort == 0)
                {

                    if (!str.Contains('-') == true)
                    {
                        e.Handled = !IsTextAllowedForDecimal(e.Text);
                    }

                }
                else
                {
                    if (keyValue != "Subtract")
                    {
                        if (keyValue != "OemMinus")
                        {
                            e.Handled = !IsTextAllowedForDecimal(e.Text);
                        }
                        else
                        {
                            e.Handled = true;
                        }
                    }
                    else
                    {
                        e.Handled = true;
                    }
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

        private void cmb_LowerLimit_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                TextBox btn = (TextBox)sender;
                string str = btn.Text;
                keyValue = e.Key.ToString();
                int SelectionSort = btn.SelectionStart;
                if (keyValue == "V" && Keyboard.Modifiers == ModifierKeys.Control)
                {
                    e.Handled = false;
                    string cliptext = Clipboard.GetText();
                    if (e.Handled == IsTextAllowed(cliptext))
                    {
                        e.Handled = true;
                    }
                    base.OnPreviewKeyDown(e);
                }
                if (keyValue == "Decimal" || keyValue == "OemPeriod")
                {
                    if (!str.Contains('.') == true)
                    {

                        if (e.Handled == IsTextAllowedForDecimal(e.ToString()))
                        {
                            if (e.Key == Key.Space)
                            {
                                e.Handled = true;
                            }
                            base.OnPreviewKeyDown(e);
                        }
                    }
                    else
                    {
                        e.Handled = true;
                    }
                }

                if ((keyValue == "Subtract" || keyValue == "OemMinus") && SelectionSort == 0)
                {
                    if (!str.Contains('-') == true)
                    {

                        if (e.Handled == IsTextAllowedForDecimal(e.ToString()))
                        {
                            if (e.Key == Key.Space)
                            {
                                e.Handled = true;
                            }
                            base.OnPreviewKeyDown(e);
                        }
                    }
                    else
                    {
                        e.Handled = true;
                    }
                }
                else
                {
                    if (keyValue != "Subtract")
                    {
                        if (keyValue != "OemMinus")
                        {
                            if (e.Handled == IsTextAllowedForDecimal(e.ToString()))
                            {
                                if (e.Key == Key.Space)
                                {
                                    e.Handled = true;
                                }
                                base.OnPreviewKeyDown(e);
                            }
                        }
                        else
                        {
                            e.Handled = true;
                        }
                    }
                    else
                    {
                        e.Handled = true;
                    }
                }

            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif

                //MessageBox.Show("Exception\n " + ex.Message, "QAT Error Code - EC12111", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void cmb_LowerLimit_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            try
            {
                TextBox btn = (TextBox)sender;
                string str = btn.Text;
                int SelectionSort = btn.SelectionStart;

                if (keyValue == "Decimal" || keyValue == "OemPeriod")
                {
                    if (!str.Contains('.') == true)
                    {
                        e.Handled = !IsTextAllowedForDecimal(e.Text);

                    }
                }

                if ((keyValue == "Subtract" || keyValue == "OemMinus") & SelectionSort == 0)
                {

                    if (!str.Contains('-') == true)
                    {
                        e.Handled = !IsTextAllowedForDecimal(e.Text);
                    }

                }
                else
                {
                    if (keyValue != "Subtract")
                    {
                        if (keyValue != "OemMinus")
                        {
                            e.Handled = !IsTextAllowedForDecimal(e.Text);
                        }
                        else
                        {
                            e.Handled = true;
                        }
                    }
                    else
                    {
                        e.Handled = true;
                    }
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

        private void Inventory_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                InventoryErrorHandling.DataContext = cbmitemsvalue;
                Inventory_grpbx.IsEnabled = true;
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

        private void Inventory_Unchecked(object sender, RoutedEventArgs e)
        {
            Inventory_grpbx.IsEnabled = false;
        }

        private void btn_SetTelnetMoveDown_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                FrameworkElement sourceElement = sender as FrameworkElement;
                if (sourceElement == null)
                    return;

                CBMTestTelnetItem originalTestTelnetItem = sourceElement.DataContext as CBMTestTelnetItem;
                if (originalTestTelnetItem == null)
                    return;

                int index = cbmitemsvalue.SetTestTelnetList.IndexOf(originalTestTelnetItem);
                CBMTestTelnetItem targetTestTelnetItem = null;

                if (index < cbmitemsvalue.SetTestTelnetList.Count - 1)
                {
                    targetTestTelnetItem = cbmitemsvalue.SetTestTelnetList[index + 1];
                    cbmitemsvalue.SetTestTelnetList[index + 1] = originalTestTelnetItem;
                    cbmitemsvalue.SetTestTelnetList[index] = targetTestTelnetItem;                   
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

        private void btn_SetTelnetMoveUp_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                FrameworkElement sourceElement = sender as FrameworkElement;
                if (sourceElement == null)
                    return;

                CBMTestTelnetItem originalTestTelnetItem = sourceElement.DataContext as CBMTestTelnetItem;
                if (originalTestTelnetItem == null)
                    return;

                int index = cbmitemsvalue.SetTestTelnetList.IndexOf(originalTestTelnetItem);
                CBMTestTelnetItem targetTestTelnetItem = null;

                if (index > 0)
                {
                    targetTestTelnetItem = cbmitemsvalue.SetTestTelnetList[index - 1];
                    cbmitemsvalue.SetTestTelnetList[index - 1] = originalTestTelnetItem;
                    cbmitemsvalue.SetTestTelnetList[index] = targetTestTelnetItem;       
                }
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif

                //MessageBox.Show("Exception\n " + ex.Message, "QAT Error Code - ECxxxxx", MessageBoxButton.OK, MessageBoxImage.Error);
                //DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
            }
        }

        private void btn_SetTelnetCopy_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                FrameworkElement sourceElement = sender as FrameworkElement;
                if (sourceElement == null)
                    return;

                CBMTestTelnetItem originalTestTelnetItem = sourceElement.DataContext as CBMTestTelnetItem;
                if (originalTestTelnetItem == null)
                    return;

                AddSetTestTelnetItem(originalTestTelnetItem);               

            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif

            }
        }

        private void btn_SetTelnetMinus_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                CBMTestTelnetItem originalTestTelnetItem = null;

                Button selectedComboBox = sender as Button;
                if (selectedComboBox != null && selectedComboBox.DataContext != null && String.Equals(selectedComboBox.DataContext.GetType().ToString(), "QSC_Test_Automation.CBMTestTelnetItem"))
                    originalTestTelnetItem = (CBMTestTelnetItem)selectedComboBox.DataContext;

                if (originalTestTelnetItem == null)
                    return;

                RemoveSetTestTelnetItem(originalTestTelnetItem);
            }
            catch (Exception ex)
            {
                //MessageBox.Show("Exception\n \n" + ex.Message, "QAT Error Code - ECxxxx", MessageBoxButton.OK, MessageBoxImage.Error);
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif

            }
        }

        private void btn_SetTelnetAdd_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                AddSetTestTelnetItem();
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

        public void RemoveSetTestTelnetItem(CBMTestTelnetItem removeItem)
        {
            try
            {
                if (cbmitemsvalue.SetTestTelnetList.Contains(removeItem))
                    cbmitemsvalue.SetTestTelnetList.Remove(removeItem);
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

        public CBMTestTelnetItem AddSetTestTelnetItem(CBMTestTelnetItem sourceTestTelnetItem)
        {
            CBMTestTelnetItem setTestTelnetItem = CopyTestTelnetItem(sourceTestTelnetItem);
            cbmitemsvalue.SetTestTelnetList.Add(setTestTelnetItem);
            return setTestTelnetItem;
        }

        public CBMTestTelnetItem CopyTestTelnetItem(CBMTestTelnetItem sourceTestTelnetItem)
        {
            CBMTestTelnetItem targetTestTelnetItem = new CBMTestTelnetItem();           
            targetTestTelnetItem.TelnetCommandText = sourceTestTelnetItem.TelnetCommandText;
            targetTestTelnetItem.CBMCombo = sourceTestTelnetItem.CBMCombo;
            targetTestTelnetItem.CBMComboModel = sourceTestTelnetItem.CBMComboModel;

            targetTestTelnetItem.TelnetSelectedDevice = sourceTestTelnetItem.TelnetSelectedDevice;

            List<DUT_DeviceItem> telnetDeviceItemList = new List<DUT_DeviceItem>();
            foreach (var item in sourceTestTelnetItem.CBM)
            {
                telnetDeviceItemList.Add(new DUT_DeviceItem(item));
            }

            targetTestTelnetItem.CBM = new ObservableCollection<DUT_DeviceItem>(telnetDeviceItemList);

            return targetTestTelnetItem;
        }

        private void LogMonitoring_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                tabItemVerifyLog.IsEnabled = true;
                LogErrorHandling.IsEnabled = true;
                LogErrorHandling.DataContext = cbmitemsvalue;
                AddDutDevices();
                if (cbmitemsvalue.VerifyTestLogList.Count < 1)
                    AddVerifyTestLogItem();
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

        private void AddDutDevices()
        {
            try
            {
                string TP_name = string.Empty;
                if (sourceTestSuiteItem != null)
                {
                    string TelnetQuery = string.Empty;
                    TelnetQuery = "Select TPID from TSTPLinkTable where TSID='" + sourceTestSuiteItem.TestSuiteID + "'";
                    adap = new SqlDataAdapter(TelnetQuery, this.connect.CreateConnection());
                    this.connect.OpenConnection();
                    ReadDT = new DataTable();

                    adap.Fill(ReadDT);
                    for (int k = 0; k < ReadDT.Rows.Count; k++)
                    {

                        read = null;
                        read = ReadDT.CreateDataReader();
                        while (read.Read())
                        {
                            var cell = ReadDT.Rows[k][0];
                            TelnetQuery = "Select Testplanname from Testplan where TestPlanID='" + cell + "'";
                            adap = new SqlDataAdapter(TelnetQuery, this.connect.CreateConnection());
                            this.connect.OpenConnection();
                            ReadDT1 = new DataTable();
                            adap.Fill(ReadDT1);
                            if (ReadDT1.Rows.Count > 0)
                            {
                                read = null;
                                read = ReadDT1.CreateDataReader();
                                while (read.Read())
                                {
                                    TP_name = read.GetString(0);
                                    TelnetDevicesFetch_ContinousBackgroundMoniting(TP_name);
                                }
                            }
                        }
                    }
                }

                if (sourceTestPlanItem != null)
                {
                    TelnetDevicesFetch_ContinousBackgroundMoniting(sourceTestPlanItem.TestItemNameCopy);
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

        private void LogMonitoring_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                tabItemVerifyLog.IsEnabled = false;
                LogErrorHandling.IsEnabled = false;
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

        public CBMTestLogItem AddVerifyTestLogItem()
        {
            CBMTestLogItem verifyTestLogItem = new CBMTestLogItem();
            try
            {
                ObservableCollection<string> eventloglog = new ObservableCollection<string>();
                if (cbmitemsvalue.TelnetDutDeviceItem.Count > 0)
                {
                    eventloglog.Add("All devices");
                }
                foreach (var item in cbmitemsvalue.TelnetDutDeviceItem)
                {
                    eventloglog.Add(item.ItemDeviceName);
                }
                verifyTestLogItem.Log_verification_kernellog = eventloglog;

                cbmitemsvalue.VerifyTestLogList.Add(verifyTestLogItem);
                cbmitemsvalue.ParentCBMTestLogItem = verifyTestLogItem;
                return verifyTestLogItem;
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif

                //MessageBox.Show("Exception\n \n" + ex.Message, "QAT Error Code - ECxxxx", MessageBoxButton.OK, MessageBoxImage.Error);
                return verifyTestLogItem;
            }
        }

        private void btn_VerifyControlMinus_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                CBMTestControlItem originalTestTelnetItem = null;

                Button selectedComboBox = sender as Button;
                if (selectedComboBox != null && selectedComboBox.DataContext != null && String.Equals(selectedComboBox.DataContext.GetType().ToString(), "QSC_Test_Automation.CBMTestControlItem"))
                    originalTestTelnetItem = (CBMTestControlItem)selectedComboBox.DataContext;

                if (originalTestTelnetItem == null)
                    return;

                RemoveVerifyTestControlItem(originalTestTelnetItem);
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

        private void btn_VerifyControlCopy_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                FrameworkElement sourceElement = sender as FrameworkElement;
                if (sourceElement == null)
                    return;

                CBMTestControlItem originalTestControlItem = sourceElement.DataContext as CBMTestControlItem;
                if (originalTestControlItem == null)
                    return;

                AddVerifyTestControlItem(originalTestControlItem);
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
            }
        }

        private void btn_VerifyControlMoveUp_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                FrameworkElement sourceElement = sender as FrameworkElement;
                if (sourceElement == null)
                    return;

                CBMTestControlItem originalTestControlItem = sourceElement.DataContext as CBMTestControlItem;
                if (originalTestControlItem == null)
                    return;

                int index = originalTestControlItem.ParentTestActionItem.VerifyTestControlList.IndexOf(originalTestControlItem);
                CBMTestControlItem targetTestControlItem = null;

                if (index > 0)
                {
                    targetTestControlItem = originalTestControlItem.ParentTestActionItem.VerifyTestControlList[index - 1];
                    originalTestControlItem.ParentTestActionItem.VerifyTestControlList[index - 1] = originalTestControlItem;
                    originalTestControlItem.ParentTestActionItem.VerifyTestControlList[index] = targetTestControlItem;
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

        private void btn_VerifyControlMoveDown_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                FrameworkElement sourceElement = sender as FrameworkElement;
                if (sourceElement == null)
                    return;

                CBMTestControlItem originalTestControlItem = sourceElement.DataContext as CBMTestControlItem;
                if (originalTestControlItem == null)
                    return;

                int index = originalTestControlItem.ParentTestActionItem.VerifyTestControlList.IndexOf(originalTestControlItem);
                CBMTestControlItem targetTestControlItem = null;

                if (index < originalTestControlItem.ParentTestActionItem.VerifyTestControlList.Count - 1)
                {
                    targetTestControlItem = originalTestControlItem.ParentTestActionItem.VerifyTestControlList[index + 1];
                    originalTestControlItem.ParentTestActionItem.VerifyTestControlList[index + 1] = originalTestControlItem;
                    originalTestControlItem.ParentTestActionItem.VerifyTestControlList[index] = targetTestControlItem;                   
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

        private void btn_SelectVerificationPlus_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                AddVerifyTestControlItem();
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

        public CBMTestControlItem AddVerifyTestControlItem()
        {
            CBMTestControlItem verifyTestControlItem = new CBMTestControlItem();

            try
            {
                ObservableCollection<string> ComponentTypeList = new ObservableCollection<string>(cbmitemsvalue.cmb_ComponentType.OrderBy(a => a));
                verifyTestControlItem.TestControlComponentTypeList = ComponentTypeList;
                cbmitemsvalue.VerifyTestControlList.Add(verifyTestControlItem);
                verifyTestControlItem.ParentTestActionItem = cbmitemsvalue;
                return verifyTestControlItem;
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif

                //MessageBox.Show("Exception\n \n" + ex.Message, "QAT Error Code - ECxxxx", MessageBoxButton.OK, MessageBoxImage.Error);
                return verifyTestControlItem;
            }
        }
        
        public void GetDesignComponentDetails()
        {
            try
            {
                DataTable dataTable = new DataTable();
                DataTableReader dataTableReader = null;
                string query = "select * from designtable where DesignID in(select DesignID from TPDesignLinkTable where TPID = ('" + sourceTestPlanItem.TestPlanID + "'))";

                adap = new SqlDataAdapter(query, this.connect.CreateConnection());
                this.connect.OpenConnection();

                adap.Update(dataTable);
                adap.Fill(dataTable);
             
                dataTableReader = dataTable.CreateDataReader();

                if (dataTableReader.HasRows)
                {
                    while (dataTableReader.Read())
                    {
                        sourceTestPlanItem.DesignID = dataTableReader.GetInt32(0);
                        sourceTestPlanItem.DesignName = dataTableReader.GetString(1).ToString();
                    }
                }

                //query = "Select * from (select *, row_number() OVER(PARTITION BY ComponentName,ComponentType, Control, PrettyName ORDER BY ComponentName,ComponentType, Control, PrettyName DESC) as repeatRowCount from TCInitialization where DesignID = " + sourceTestPlanItem.DesignID + " and (ControlDirection like '%control_direction_external_read%' or ControlDirection like '%control_direction_external_write%') and Control not like '%%metadata%' and PrettyName is not null and PrettyName != '' and PrettyName not like '%<%') as a where a.repeatRowCount = 1";

                //query = "select * from TCInitialization where designid=('" + sourceTestPlanItem.DesignID + "') order by PrettyName asc";//CONVERT(INT,SUBSTRING(prettyname,PATINDEX('%[0-9]%',prettyname),LEN(prettyname)))
                
				query = "select * from TCInitialization where (designid=('" + sourceTestPlanItem.DesignID + "') and (ControlDirection like '%control_direction_external_read%' or ControlDirection like '%control_direction_external_write%') and Control not like '%%metadata%' and PrettyName is not null and PrettyName != '' and PrettyName not like '%<%') order by PrettyName asc";//CONVERT(INT,SUBSTRING(prettyname,PATINDEX(' %[0-9]%',prettyname),LEN(prettyname)))
                
				dataTable = new DataTable();
                dataTableReader = null;
                adap = new SqlDataAdapter(query, this.connect.CreateConnection());
                adap.Update(dataTable);
                adap.Fill(dataTable);
				
				if (dataTable.Rows.Count > 0)
                {
                    var duplicaDT = dataTable.AsEnumerable().GroupBy(row => new { Compname = row.Field<string>("ComponentName"), compType = row.Field<string>("ComponentType"), control = row.Field<string>("Control"), prettyName = row.Field<string>("PrettyName") }).Select(group => group.First()).CopyToDataTable();
                    dataTable = duplicaDT;
                }
				
                dataTableReader = dataTable.CreateDataReader();

                cbmitemsvalue.cmb_ComponentType.Clear();
                cbmitemsvalue.ComponentNameDictionary.Clear();
                cbmitemsvalue.ControlNameDictionary.Clear();
                cbmitemsvalue.ControlTypeDictionary.Clear();
                cbmitemsvalue.ControlInitialValueDictionary.Clear();
                cbmitemsvalue.ControlInitialStringDictionary.Clear();
                cbmitemsvalue.ControlInitialPositionDictionary.Clear();
                cbmitemsvalue.VerifyControlNameDictionary.Clear();
                cbmitemsvalue.dataTypeDictionary.Clear();
                cbmitemsvalue.VerifyDataTypeDictionary.Clear();
                cbmitemsvalue.ChannelSelectionDictionary.Clear();
                cbmitemsvalue.channelControlTypeDictionary.Clear();
                cbmitemsvalue.VerifychannelControlTypeDictionary.Clear();
                cbmitemsvalue.MaximumControlValueDictionary.Clear();
                cbmitemsvalue.MinimumControlValueDictionary.Clear();
				cbmitemsvalue.ControlIDDictionary.Clear();				
            
                if (dataTableReader.HasRows)
                {
                    while (dataTableReader.Read())
                    {
                        string componentType = dataTableReader.GetString(1).ToString();
                        string componentName = dataTableReader.GetString(2).ToString();
                        string controlName = string.Empty;
                        string controlDirection = string.Empty;
                        if (dataTableReader[14] != System.DBNull.Value)
                        {
                            controlName = dataTableReader.GetString(14).ToString();
                        }

                        int writeDuplicateCount = 0;
                        int readDuplicateCount = 0;

                        try
                        {
                            string componentName1 = componentName.Replace("'", "''");
                            string componentType1 = componentType.Replace("'", "''");
                            string controlName1 = controlName.Replace("'", "''");

                            DataRow[] writeresults = dataTable.Select("ComponentName = '" + componentName1 + "' AND ComponentType = '" + componentType1 + "' AND PrettyName = '" + controlName1 + "'");

                            if (writeresults != null && writeresults.Count() > 1)
                            {
                                DataRow[] writeresultss = dataTable.Select("ComponentName = '" + componentName1 + "' AND ComponentType = '" + componentType1 + "' AND PrettyName = '" + controlName1 + "'AND ControlDirection like '%control_direction_external_write%'");
                                if (writeresultss != null)
                                    writeDuplicateCount = writeresultss.Count();

                                DataRow[] readresultss = dataTable.Select("ComponentName = '" + componentName1 + "' AND ComponentType = '" + componentType1 + "' AND PrettyName = '" + controlName1 + "'AND ControlDirection like '%control_direction_external_read%'");
                                if (readresultss != null)
                                    readDuplicateCount = readresultss.Count();
                            }
                        }
                        catch (Exception ex)
                        {

                        }

                        //DataRow[] writeresults = dataTable.Select("ComponentName = '" + componentName + "' AND ComponentType = '" + componentType + "' AND PrettyName = '" + controlName + "'");

                        //if (writeresults != null && writeresults.Count() > 1)
                        //{
                        //    DataRow[] writeresultss = dataTable.Select("ComponentName = '" + componentName + "' AND ComponentType = '" + componentType + "' AND PrettyName = '" + controlName + "'AND ControlDirection like '%control_direction_external_write%'");
                        //    if (writeresultss != null)
                        //        writeDuplicateCount = writeresultss.Count();

                        //    DataRow[] readresultss = dataTable.Select("ComponentName = '" + componentName + "' AND ComponentType = '" + componentType + "' AND PrettyName = '" + controlName + "'AND ControlDirection like '%control_direction_external_read%'");
                        //    if (readresultss != null)
                        //        readDuplicateCount = readresultss.Count();
                        //}

                        string controlType = string.Empty;
                        if (dataTableReader[15] != System.DBNull.Value)
                        {
                            controlType = dataTableReader.GetString(15).ToString();
                        }

                        string min_ControlValue = string.Empty;
                        if (dataTableReader[6] != System.DBNull.Value)
                        {
                            min_ControlValue = dataTableReader.GetString(6).ToString();
                        }

                        string max_ControlValue = string.Empty;
                        if (dataTableReader[7] != System.DBNull.Value)
                        {
                            max_ControlValue = dataTableReader.GetString(7).ToString();
                        }

                        string controlInitialValue = string.Empty;
                        if (dataTableReader[11] != System.DBNull.Value)
                        {
                            controlInitialValue = dataTableReader.GetString(11).ToString();
                        }

                        string controlInitialString = string.Empty;
                        if (dataTableReader[12] != System.DBNull.Value)
                        {
                            controlInitialString = dataTableReader.GetString(12).ToString();
                        }

                        string controlInitialPosition = string.Empty;
                        if (dataTableReader[13] != System.DBNull.Value)
                        {
                            controlInitialPosition = dataTableReader.GetString(13).ToString();
                        }
                        string filterMetadata = string.Empty;
                        if (dataTableReader[3] != System.DBNull.Value)
                        {
                            filterMetadata = dataTableReader.GetString(3).ToString();
                        }
                        if (!cbmitemsvalue.cmb_ComponentType.Contains(componentType))
                            cbmitemsvalue.cmb_ComponentType.Add(componentType);

                        if (!cbmitemsvalue.ComponentNameDictionary.Keys.Contains(componentType))
                            cbmitemsvalue.ComponentNameDictionary.Add(componentType, new ObservableCollection<string>());

                        if (!cbmitemsvalue.ControlNameDictionary.Keys.Contains(componentName))
                        {
                            cbmitemsvalue.ControlNameDictionary.Add(componentName, new ObservableCollection<string>());
                        }

                        if (!cbmitemsvalue.VerifyControlNameDictionary.Keys.Contains(componentName))
                            cbmitemsvalue.VerifyControlNameDictionary.Add(componentName, new ObservableCollection<string>());
                        
                        if (!cbmitemsvalue.ComponentNameDictionary[componentType].Contains(componentName))
                            cbmitemsvalue.ComponentNameDictionary[componentType].Add(componentName);


                        if (dataTableReader[17] != System.DBNull.Value)
                        {
                            controlDirection = dataTableReader.GetString(17).ToString();

                            #region Write
                            if (controlDirection.Contains("control_direction_external_write"))
                            {
                                string controlNameduplicate = controlName;
                                
                                string[] Channel_split = new string[2];
                                string channelControl = string.Empty;

                                if (controlNameduplicate.Contains("~")) /*(((controlName.Contains("Channel"))|| (controlName.Contains("Output"))|| (controlName.Contains("Input")) || (controlName.Contains("Tap")) || (controlName.Contains("Bank Control")) || (controlName.Contains("Bank Select")) || (controlName.Contains("GPIO"))) & (controlName.Contains("~")))*/
                                {
                                    int tiltCount = controlNameduplicate.Count(x => x == '~');
                                    string channelWithTwoTilt = controlNameduplicate;
                                    int idx = channelWithTwoTilt.LastIndexOf('~');
                                    Channel_split[0] = channelWithTwoTilt.Substring(0, idx);
                                    Channel_split[1] = channelWithTwoTilt.Substring(idx + 1);

                                    string QATPrefix = QscDatabase.addQATPrefixToControl(controlNameduplicate);
                                    if (!string.IsNullOrEmpty(QATPrefix))
                                        channelControl = QATPrefix + Channel_split[1];
                                }

                                if (!(string.IsNullOrEmpty(channelControl)))
                                {
                                    if (!cbmitemsvalue.channelControlTypeDictionary.Keys.Contains(componentName + channelControl))
                                        cbmitemsvalue.channelControlTypeDictionary.Add(componentName + channelControl, controlDirection);

                                    if (!cbmitemsvalue.ControlTypeDictionary.Keys.Contains(componentName + controlNameduplicate))
                                        cbmitemsvalue.ControlTypeDictionary.Add(componentName + controlNameduplicate, controlDirection);
                                    
                                    if ((!cbmitemsvalue.dataTypeDictionary.Keys.Contains(componentName + controlNameduplicate + filterMetadata)))
                                        cbmitemsvalue.dataTypeDictionary.Add(componentName + controlNameduplicate + filterMetadata, controlType);

                                    if ((!cbmitemsvalue.ControlNameDictionary[componentName].Contains(channelControl)))
                                        cbmitemsvalue.ControlNameDictionary[componentName].Add(channelControl);

                                    if (!cbmitemsvalue.ChannelSelectionDictionary.Keys.Contains(componentName + channelControl))
                                            cbmitemsvalue.ChannelSelectionDictionary.Add(componentName + channelControl, new ObservableCollection<string>());

                                    if (writeDuplicateCount > 1)
                                    {
                                        if ((!cbmitemsvalue.ChannelSelectionDictionary[componentName + channelControl].Contains(Channel_split[0] + " [" + filterMetadata + "]")))
                                            cbmitemsvalue.ChannelSelectionDictionary[componentName + channelControl].Add(Channel_split[0] + " [" + filterMetadata + "]");

                                        if (!cbmitemsvalue.ControlIDDictionary.Keys.Contains(componentName + channelControl + Channel_split[0] + " [" + filterMetadata + "]"))
                                            cbmitemsvalue.ControlIDDictionary.Add(componentName + channelControl + Channel_split[0] + " [" + filterMetadata + "]", new string[2]);

                                        string[] array = new string[] { Channel_split[0], filterMetadata };
                                        if (cbmitemsvalue.ControlIDDictionary[componentName + channelControl + Channel_split[0] + " [" + filterMetadata + "]"] != (array))
                                            cbmitemsvalue.ControlIDDictionary[componentName + channelControl + Channel_split[0] + " [" + filterMetadata + "]"] = array;
                                    }
                                    else
                                    {
                                        if ((!cbmitemsvalue.ChannelSelectionDictionary[componentName + channelControl].Contains(Channel_split[0])))
                                            cbmitemsvalue.ChannelSelectionDictionary[componentName + channelControl].Add(Channel_split[0]);

                                        if (!cbmitemsvalue.ControlIDDictionary.Keys.Contains(componentName + channelControl + Channel_split[0]))
                                            cbmitemsvalue.ControlIDDictionary.Add(componentName + channelControl + Channel_split[0], new string[2]);

                                        string[] array = new string[] { Channel_split[0], filterMetadata };
                                        if (cbmitemsvalue.ControlIDDictionary[componentName + channelControl + Channel_split[0]] != (array))
                                            cbmitemsvalue.ControlIDDictionary[componentName + channelControl + Channel_split[0]] = array;
                                    }

                                    if (!cbmitemsvalue.ControlInitialValueDictionary.Keys.Contains(componentName + controlNameduplicate + filterMetadata))
                                        cbmitemsvalue.ControlInitialValueDictionary.Add(componentName + controlNameduplicate + filterMetadata, controlInitialValue);

                                    if (!cbmitemsvalue.ControlInitialStringDictionary.Keys.Contains(componentName + controlNameduplicate + filterMetadata))
                                        cbmitemsvalue.ControlInitialStringDictionary.Add(componentName + controlNameduplicate + filterMetadata, controlInitialString);

                                    if (!cbmitemsvalue.ControlInitialPositionDictionary.Keys.Contains(componentName + controlNameduplicate + filterMetadata))
                                        cbmitemsvalue.ControlInitialPositionDictionary.Add(componentName + controlNameduplicate + filterMetadata,controlInitialPosition);

                                    if (!cbmitemsvalue.MinimumControlValueDictionary.Keys.Contains(componentName + controlNameduplicate + filterMetadata))
                                        cbmitemsvalue.MinimumControlValueDictionary.Add(componentName + controlNameduplicate + filterMetadata, min_ControlValue);

                                    if (!cbmitemsvalue.MaximumControlValueDictionary.Keys.Contains(componentName + controlNameduplicate + filterMetadata))
                                        cbmitemsvalue.MaximumControlValueDictionary.Add(componentName + controlNameduplicate + filterMetadata, max_ControlValue);
                                }
                                else
                                {
                                    if (writeDuplicateCount > 1)
                                        controlNameduplicate = controlName + " [" + filterMetadata + "]";

                                    if (!cbmitemsvalue.ControlTypeDictionary.Keys.Contains(componentName + controlNameduplicate))
                                        cbmitemsvalue.ControlTypeDictionary.Add(componentName + controlNameduplicate, controlDirection);

                                    if ((!cbmitemsvalue.dataTypeDictionary.Keys.Contains(componentName + controlNameduplicate + filterMetadata)))
                                        cbmitemsvalue.dataTypeDictionary.Add(componentName + controlNameduplicate + filterMetadata, controlType);

                                    if ((!cbmitemsvalue.ControlNameDictionary[componentName].Contains(controlNameduplicate)))
                                        cbmitemsvalue.ControlNameDictionary[componentName].Add(controlNameduplicate);

                                    if (!cbmitemsvalue.ControlInitialValueDictionary.Keys.Contains(componentName + controlNameduplicate ))
                                        cbmitemsvalue.ControlInitialValueDictionary.Add(componentName + controlNameduplicate, controlInitialValue);

                                    if (!cbmitemsvalue.ControlInitialStringDictionary.Keys.Contains(componentName + controlNameduplicate))
                                        cbmitemsvalue.ControlInitialStringDictionary.Add(componentName + controlNameduplicate, controlInitialString);

                                    if (!cbmitemsvalue.ControlInitialPositionDictionary.Keys.Contains(componentName + controlNameduplicate))
                                        cbmitemsvalue.ControlInitialPositionDictionary.Add(componentName + controlNameduplicate, controlInitialPosition);

                                    if (!cbmitemsvalue.MinimumControlValueDictionary.Keys.Contains(componentName + controlNameduplicate + filterMetadata))
                                        cbmitemsvalue.MinimumControlValueDictionary.Add(componentName + controlNameduplicate + filterMetadata, min_ControlValue);

                                    if (!cbmitemsvalue.MaximumControlValueDictionary.Keys.Contains(componentName + controlNameduplicate + filterMetadata))
                                        cbmitemsvalue.MaximumControlValueDictionary.Add(componentName + controlNameduplicate + filterMetadata, max_ControlValue);

                                    if (!cbmitemsvalue.ControlIDDictionary.Keys.Contains(componentName + controlNameduplicate))
                                        cbmitemsvalue.ControlIDDictionary.Add(componentName + controlNameduplicate, new string[2]);

                                    string[] array = new string[] { controlName, filterMetadata };
                                    if (cbmitemsvalue.ControlIDDictionary[componentName + controlNameduplicate] != (array))
                                        cbmitemsvalue.ControlIDDictionary[componentName + controlNameduplicate] = array;
                                }
                            }
                            #endregion

                            #region Read
                            if (controlDirection.Contains("control_direction_external_read"))// | controlDirection.Contains("control_direction_ramp")| controlDirection.Contains("control_direction_snapshot"))
                            {
                                string controlNameduplicate = controlName;

                                string channelControl = string.Empty;
                                string[] Channel_split = new string[2];

                                if (controlNameduplicate.Contains("~"))
                                {
                                    int tiltCount = controlNameduplicate.Count(x => x == '~');
                                    string channelWithTwoTilt = controlNameduplicate;
                                    int idx = channelWithTwoTilt.LastIndexOf('~');
                                    Channel_split[0] = channelWithTwoTilt.Substring(0, idx);
                                    Channel_split[1] = channelWithTwoTilt.Substring(idx + 1);
                                    string QATPrefix = QscDatabase.addQATPrefixToControl(controlNameduplicate);
                                    if (!string.IsNullOrEmpty(QATPrefix))
                                        channelControl = QATPrefix + Channel_split[1];
                                }

                                if (!(string.IsNullOrEmpty(channelControl)))
                                {
                                    if (!cbmitemsvalue.VerifychannelControlTypeDictionary.Keys.Contains(componentName + channelControl))
                                        cbmitemsvalue.VerifychannelControlTypeDictionary.Add(componentName + channelControl, controlDirection);

                                    if (!cbmitemsvalue.VerifyControlTypeDictionary.Keys.Contains(componentName + controlNameduplicate))
                                        cbmitemsvalue.VerifyControlTypeDictionary.Add(componentName + controlNameduplicate, controlDirection);

                                    if ((!cbmitemsvalue.VerifyDataTypeDictionary.Keys.Contains(componentName + controlNameduplicate + filterMetadata)))
                                        cbmitemsvalue.VerifyDataTypeDictionary.Add(componentName + controlNameduplicate + filterMetadata, controlType);

                                    if ((!cbmitemsvalue.VerifyControlNameDictionary[componentName].Contains(channelControl)))
                                        cbmitemsvalue.VerifyControlNameDictionary[componentName].Add(channelControl);
                                    
                                    if (!cbmitemsvalue.ChannelSelectionDictionary.Keys.Contains(componentName + channelControl))
                                        cbmitemsvalue.ChannelSelectionDictionary.Add(componentName + channelControl, new ObservableCollection<string>());

                                    if (readDuplicateCount > 1)
                                    {
                                        if ((!cbmitemsvalue.ChannelSelectionDictionary[componentName + channelControl].Contains(Channel_split[0] + " [" + filterMetadata + "]")))
                                            cbmitemsvalue.ChannelSelectionDictionary[componentName + channelControl].Add(Channel_split[0] + " [" + filterMetadata + "]");

                                        if (!cbmitemsvalue.ControlIDDictionary.Keys.Contains(componentName + channelControl + Channel_split[0] + " [" + filterMetadata + "]"))
                                            cbmitemsvalue.ControlIDDictionary.Add(componentName + channelControl + Channel_split[0] + " [" + filterMetadata + "]", new string[2]);

                                        string[] array = new string[] { Channel_split[0], filterMetadata };
                                        if (cbmitemsvalue.ControlIDDictionary[componentName + channelControl + Channel_split[0] + " [" + filterMetadata + "]"] != (array))
                                            cbmitemsvalue.ControlIDDictionary[componentName + channelControl + Channel_split[0] + " [" + filterMetadata + "]"] = array;
                                    }
                                    else
                                    {
                                        if ((!cbmitemsvalue.ChannelSelectionDictionary[componentName + channelControl].Contains(Channel_split[0])))
                                            cbmitemsvalue.ChannelSelectionDictionary[componentName + channelControl].Add(Channel_split[0]);

                                        if (!cbmitemsvalue.ControlIDDictionary.Keys.Contains(componentName + channelControl + Channel_split[0]))
                                            cbmitemsvalue.ControlIDDictionary.Add(componentName + channelControl + Channel_split[0], new string[2]);

                                        string[] array = new string[] { Channel_split[0], filterMetadata };
                                        if (cbmitemsvalue.ControlIDDictionary[componentName + channelControl + Channel_split[0]] != (array))
                                            cbmitemsvalue.ControlIDDictionary[componentName + channelControl + Channel_split[0]] = array;
                                    }
                                                                        
                                    if (!cbmitemsvalue.ControlInitialValueDictionary.Keys.Contains(componentName + controlNameduplicate + filterMetadata))
                                        cbmitemsvalue.ControlInitialValueDictionary.Add(componentName + controlNameduplicate + filterMetadata, controlInitialValue);

                                    if (!cbmitemsvalue.ControlInitialStringDictionary.Keys.Contains(componentName + controlNameduplicate + filterMetadata))
                                        cbmitemsvalue.ControlInitialStringDictionary.Add(componentName + controlNameduplicate + filterMetadata, controlInitialString);

                                    if (!cbmitemsvalue.ControlInitialPositionDictionary.Keys.Contains(componentName + controlNameduplicate + filterMetadata))
                                        cbmitemsvalue.ControlInitialPositionDictionary.Add(componentName + controlNameduplicate + filterMetadata, controlInitialPosition);

                                    if (!cbmitemsvalue.MinimumControlValueDictionary.Keys.Contains(componentName + controlNameduplicate + filterMetadata))
                                        cbmitemsvalue.MinimumControlValueDictionary.Add(componentName + controlNameduplicate + filterMetadata, min_ControlValue);

                                    if (!cbmitemsvalue.MaximumControlValueDictionary.Keys.Contains(componentName + controlNameduplicate + filterMetadata))
                                        cbmitemsvalue.MaximumControlValueDictionary.Add(componentName + controlNameduplicate + filterMetadata, max_ControlValue);
                                }
                                else
                                {
                                    if (readDuplicateCount > 1)
                                        controlNameduplicate = controlName + " [" + filterMetadata + "]";

                                    if (!cbmitemsvalue.VerifyControlTypeDictionary.Keys.Contains(componentName + controlNameduplicate))
                                        cbmitemsvalue.VerifyControlTypeDictionary.Add(componentName + controlNameduplicate, controlDirection);

                                    if (!cbmitemsvalue.VerifyDataTypeDictionary.Keys.Contains(componentName + controlNameduplicate + filterMetadata))
                                        cbmitemsvalue.VerifyDataTypeDictionary.Add(componentName + controlNameduplicate + filterMetadata, controlType);

                                    if ((!cbmitemsvalue.VerifyControlNameDictionary[componentName].Contains(controlNameduplicate)))
                                        cbmitemsvalue.VerifyControlNameDictionary[componentName].Add(controlNameduplicate);

                                    if (!cbmitemsvalue.ControlInitialValueDictionary.Keys.Contains(componentName + controlNameduplicate))
                                        cbmitemsvalue.ControlInitialValueDictionary.Add(componentName + controlNameduplicate, controlInitialValue);

                                    if (!cbmitemsvalue.ControlInitialStringDictionary.Keys.Contains(componentName + controlNameduplicate))
                                        cbmitemsvalue.ControlInitialStringDictionary.Add(componentName + controlNameduplicate, controlInitialString);

                                    if (!cbmitemsvalue.ControlInitialPositionDictionary.Keys.Contains(componentName + controlNameduplicate))
                                        cbmitemsvalue.ControlInitialPositionDictionary.Add(componentName + controlNameduplicate, controlInitialPosition);

                                    if (!cbmitemsvalue.MinimumControlValueDictionary.Keys.Contains(componentName + controlNameduplicate + filterMetadata))
                                        cbmitemsvalue.MinimumControlValueDictionary.Add(componentName + controlNameduplicate + filterMetadata, min_ControlValue);

                                    if (!cbmitemsvalue.MaximumControlValueDictionary.Keys.Contains(componentName + controlNameduplicate + filterMetadata))
                                        cbmitemsvalue.MaximumControlValueDictionary.Add(componentName + controlNameduplicate + filterMetadata, max_ControlValue);

                                    if (!cbmitemsvalue.ControlIDDictionary.Keys.Contains(componentName + controlNameduplicate))
                                        cbmitemsvalue.ControlIDDictionary.Add(componentName + controlNameduplicate, new string[2]);

                                    string[] array = new string[] { controlName, filterMetadata };
                                    if (cbmitemsvalue.ControlIDDictionary[componentName + controlNameduplicate] != (array))
                                        cbmitemsvalue.ControlIDDictionary[componentName + controlNameduplicate] = array;
                                }
                            }

                            #endregion
                        }
                    }
                }



                List<string> readcontrolsremovedlist = new List<string>(cbmitemsvalue.cmb_ComponentType);
                //foreach (var testActionItem in cbmitemsvalue.TestActionItemList)
                //{
                    //foreach (var setTestControl in testActionItem.SetTestControlList)
                    //{

                        var index1 = 0;
                        for (; index1 < readcontrolsremovedlist.Count;)
                        {
                            ObservableCollection<string> ComponentNameList = cbmitemsvalue.ComponentNameDictionary[readcontrolsremovedlist[index1]];
                            if (ComponentNameList.Count < 2)
                            {
                                foreach (string componentName in ComponentNameList)
                                {
                                    ObservableCollection<string> emptystring;
                                    Dictionary<string, ObservableCollection<string>> ComponentcontrolList = cbmitemsvalue.ControlNameDictionary;
                                    ComponentcontrolList.TryGetValue(componentName, out emptystring);

                                    if (emptystring.Count == 0)
                                    {
                                        if (readcontrolsremovedlist != null)
                                        {
                                            var index = readcontrolsremovedlist.IndexOf(readcontrolsremovedlist[index1]);
                                            if (index >= 0)
                                            {
                                                readcontrolsremovedlist.RemoveAt(index);
                                            }
                                        }
                                        index1++;
                                    }
                                    else
                                    {
                                        index1++;
                                    }
                                }
                            }
                            else
                            {
                                foreach (string componentName in ComponentNameList)
                                {
                                    ObservableCollection<string> emptystring;
                                    Dictionary<string, ObservableCollection<string>> ComponentcontrolList = cbmitemsvalue.ControlNameDictionary;
                                    ComponentcontrolList.TryGetValue(componentName, out emptystring);

                                    if (emptystring.Count == 0)
                                    {
                                        if (readcontrolsremovedlist != null)
                                        {
                                            var index = readcontrolsremovedlist.IndexOf(readcontrolsremovedlist[index1]);
                                            if (index >= 0)
                                            {
                                                readcontrolsremovedlist.RemoveAt(index);
                                                break;
                                            }
                                        }

                                    }

                                }
                                index1++;
                            }

                        }
                        //setTestControl.TestControlComponentTypeList = new ObservableCollection<string>(readcontrolsremovedlist);
                        //setTestControl.InputSelectionEnabled = false;
                        //setTestControl.ChannelEnabled = false;

                        //setTestControl.RampCheckVisibility = Visibility.Hidden;
                    //}
                    foreach (var verifyTestControl in cbmitemsvalue.VerifyTestControlList)
                    {
                        verifyTestControl.TestControlComponentTypeList = new ObservableCollection<string>(cbmitemsvalue.cmb_ComponentType);
                        verifyTestControl.InputSelectionEnabled = false;
                        verifyTestControl.ChannelEnabled = false;
                        verifyTestControl.MaxLimitIsEnabled = false;
                        verifyTestControl.MinLimitIsEnabled = false;
                        verifyTestControl.MinimumLimit = string.Empty;
                        verifyTestControl.MaximumLimit = string.Empty;
                    }
               // }

                this.connect.CloseConnection();
             
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif

                //MessageBox.Show("Exception\n \n" + ex.Message, "QAT Error Code - EC02022", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void RemoveVerifyTestControlItem(CBMTestControlItem removeItem)
        {
            try
            {
                if (cbmitemsvalue.VerifyTestControlList.Contains(removeItem))
                    cbmitemsvalue.VerifyTestControlList.Remove(removeItem);
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

        public CBMTestControlItem AddVerifyTestControlItem(CBMTestControlItem sourceTestControlItem)
        {
            CBMTestControlItem verifyTestControlItem = CopyTestControlItem(sourceTestControlItem);
            //cbmitemsvalue.VerifyTestControlList.Add(verifyTestControlItem);
            cbmitemsvalue.VerifyTestControlList.Insert(cbmitemsvalue.VerifyTestControlList.IndexOf(sourceTestControlItem),verifyTestControlItem);
            return verifyTestControlItem;
        }

        public CBMTestControlItem CopyTestControlItem(CBMTestControlItem sourceTestControlItem)
        {

            CBMTestControlItem targetTestControlItem = new CBMTestControlItem();
            try
            {
                targetTestControlItem.ParentTestActionItem = sourceTestControlItem.ParentTestActionItem;

                if (sourceTestControlItem.TestControlComponentTypeList != null)
                    targetTestControlItem.TestControlComponentTypeList = new ObservableCollection<string>(sourceTestControlItem.TestControlComponentTypeList);

                targetTestControlItem.TestControlComponentTypeSelectedItem = sourceTestControlItem.TestControlComponentTypeSelectedItem;

                if (sourceTestControlItem.TestControlComponentNameList != null)
                    targetTestControlItem.TestControlComponentNameList = new ObservableCollection<string>(sourceTestControlItem.TestControlComponentNameList);
                targetTestControlItem.TestControlComponentNameSelectedItem = sourceTestControlItem.TestControlComponentNameSelectedItem;

                if (sourceTestControlItem.TestControlPropertyList != null)
                    targetTestControlItem.TestControlPropertyList = new ObservableCollection<string>(sourceTestControlItem.TestControlPropertyList);

                if (sourceTestControlItem.VerifyTestControlPropertyList != null)
                    targetTestControlItem.VerifyTestControlPropertyList = new ObservableCollection<string>(sourceTestControlItem.VerifyTestControlPropertyList);
                targetTestControlItem.TestControlPropertySelectedItem = sourceTestControlItem.TestControlPropertySelectedItem;

                if (sourceTestControlItem.ChannelSelectionList != null)
                    targetTestControlItem.ChannelSelectionList = new ObservableCollection<string>(sourceTestControlItem.ChannelSelectionList);
                targetTestControlItem.ChannelSelectionSelectedItem = sourceTestControlItem.ChannelSelectionSelectedItem;
                //setTestControlItem.InputSelectionComboList = new ObservableCollection<string>(sourceTestControlItem.InputSelectionComboList);     // Constant List
                targetTestControlItem.InputSelectionComboSelectedItem = sourceTestControlItem.InputSelectionComboSelectedItem;
                targetTestControlItem.TestControlPropertyInitialValueSelectedItem = sourceTestControlItem.TestControlPropertyInitialValueSelectedItem;

                targetTestControlItem.LoopIsChecked = sourceTestControlItem.LoopIsChecked;
                targetTestControlItem.LoopStart = sourceTestControlItem.LoopStart;
                targetTestControlItem.LoopStartValueVisibility = sourceTestControlItem.LoopStartValueVisibility;
                targetTestControlItem.LoopEnd = sourceTestControlItem.LoopEnd;
                targetTestControlItem.LoopEndValueVisibility = sourceTestControlItem.LoopEndValueVisibility;
                targetTestControlItem.LoopIncrement = sourceTestControlItem.LoopIncrement;
                targetTestControlItem.LoopIncrementValueVisibility = sourceTestControlItem.LoopIncrementValueVisibility;

                targetTestControlItem.RampIsChecked = sourceTestControlItem.RampIsChecked;
                targetTestControlItem.RampSetting = sourceTestControlItem.RampSetting;
                targetTestControlItem.RampSettingVisibility = sourceTestControlItem.RampSettingVisibility;
                targetTestControlItem.valueTextboxVisibility = sourceTestControlItem.valueTextboxVisibility;
                targetTestControlItem.valueComboboxVisibility = sourceTestControlItem.valueComboboxVisibility;
                targetTestControlItem.RampCheckVisibility = sourceTestControlItem.RampCheckVisibility;
                targetTestControlItem.LoopCheckVisibility = sourceTestControlItem.LoopCheckVisibility;
                targetTestControlItem.ChannelEnabled = sourceTestControlItem.ChannelEnabled;
                targetTestControlItem.InputSelectionEnabled = sourceTestControlItem.InputSelectionEnabled;
                targetTestControlItem.valueIsEnabled = sourceTestControlItem.valueIsEnabled;
                targetTestControlItem.MaximumLimit = sourceTestControlItem.MaximumLimit;
                targetTestControlItem.MinimumLimit = sourceTestControlItem.MinimumLimit;
                targetTestControlItem.MaxLimitIsEnabled = sourceTestControlItem.MaxLimitIsEnabled;
                targetTestControlItem.MinLimitIsEnabled = sourceTestControlItem.MinLimitIsEnabled;

                return targetTestControlItem;
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif

                //MessageBox.Show("Exception\n \n" + ex.Message, "QAT Error Code - ECxxxx", MessageBoxButton.OK, MessageBoxImage.Error);
                return targetTestControlItem;
            }
        }

        private void LoopStart_textBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            try
            {
                FrameworkElement sourceElement = sender as FrameworkElement;
                if (sourceElement == null)
                    return;

                CBMTestControlItem sourceTestCaseItem = sourceElement.DataContext as CBMTestControlItem;
                if (sourceTestCaseItem == null)
                    return;
                if (sourceTestCaseItem != null)
                {
                    CBMTestControlItem originalTestControlItem = null;
                    if (e.OriginalSource.GetType().ToString() == "System.Windows.Controls.TextBox")
                    {
                        var sourceTextBox = (TextBox)e.OriginalSource;
                        if (sourceTextBox != null && sourceTextBox.DataContext != null && String.Equals(sourceTextBox.DataContext.GetType().ToString(), "QSC_Test_Automation.CBMTestControlItem"))
                            originalTestControlItem = (CBMTestControlItem)sourceTextBox.DataContext;
                        string loopStartText = e.Text;
                        e.Handled = IsTextAllowed(loopStartText);
                        if (e.Handled)
                        {
                            if ((originalTestControlItem.ChannelSelectionSelectedItem != string.Empty) && ((originalTestControlItem.ChannelSelectionList.Count > 0)))// & (!string.IsNullOrEmpty
                            {
                                Int32 _textloopstart = Convert.ToInt32(loopStartText);
                                Int32 channelCount = originalTestControlItem.ChannelSelectionList.Count;
                                if ((_textloopstart > 0) & (_textloopstart < channelCount))
                                {
                                    e.Handled = false;
                                }
                                else if (_textloopstart == 0)
                                {
                                    e.Handled = false;
                                }
                                else if (_textloopstart == channelCount)
                                {
                                    e.Handled = false;
                                }
                                else
                                {
                                    e.Handled = true;
                                }
                            }
                        }
                        else
                        {
                            e.Handled = true;
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

                //MessageBox.Show("Exception\n " + ex.Message, "QAT Error Code - EC12104", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoopStart_textBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                string lstrCopyandPasteTxtBox = null;
                if (e.Handled == IsTextAllowed(e.ToString()))
                {
                    if (e.Key == Key.Space)
                    {
                        e.Handled = true;
                    }
                    base.OnPreviewKeyDown(e);
                }
                //copy and Paste               
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

                //MessageBox.Show("Exception\n " + ex.Message, "QAT Error Code - EC12105", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoopIncrement_textBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            try
            {
                FrameworkElement sourceElement = sender as FrameworkElement;
                if (sourceElement == null)
                    return;

                CBMTestControlItem sourceTestCaseItem = sourceElement.DataContext as CBMTestControlItem;
                if (sourceTestCaseItem == null)
                    return;

                if (sourceTestCaseItem != null)
                {
                    CBMTestControlItem originalTestControlItem = null;
                    if (e.OriginalSource.GetType().ToString() == "System.Windows.Controls.TextBox")
                    {
                        var sourceTextBox = (TextBox)e.OriginalSource;
                        if (sourceTextBox != null && sourceTextBox.DataContext != null && String.Equals(sourceTextBox.DataContext.GetType().ToString(), "QSC_Test_Automation.CBMTestControlItem"))
                            originalTestControlItem = (CBMTestControlItem)sourceTextBox.DataContext;
                        string loopIncrText = e.Text;
                        e.Handled = IsTextAllowed(loopIncrText);
                        if (e.Handled)
                        {
                            if ((originalTestControlItem.ChannelSelectionSelectedItem != string.Empty) && ((originalTestControlItem.ChannelSelectionList.Count > 0)) && ((!string.IsNullOrEmpty(originalTestControlItem.LoopStart))) && ((!string.IsNullOrEmpty(originalTestControlItem.LoopEnd))))// & (!string.IsNullOrEmpty((originalTestControlItem.ChannelSelectionSelectedItem))))
                            {
                                Int32 _textloopIncr = Convert.ToInt32(loopIncrText);
                                Int32 channelCount = originalTestControlItem.ChannelSelectionList.Count;
                                if ((_textloopIncr > 0) & (_textloopIncr < channelCount))
                                {
                                    e.Handled = false;
                                }
                                else if (_textloopIncr == 0)
                                {
                                    e.Handled = false;
                                }
                                else if (_textloopIncr == channelCount)
                                {
                                    e.Handled = false;
                                }
                                else
                                {
                                    e.Handled = false;
                                }
                            }
                            else
                            {
                                e.Handled = true;
                            }
                        }
                        else
                        {
                            e.Handled = true;
                        }
                        //string loopStartText = originalTestControlItem.LoopStart;

                    }
                }

            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif

                //MessageBox.Show("Exception\n " + ex.Message, "QAT Error Code - EC12107", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoopIncrement_textBox_PreviewKeyDown(object sender, KeyEventArgs e)
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

                //MessageBox.Show("Exception\n " + ex.Message, "QAT Error Code - EC12109", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoopEnd_textBox_PreviewKeyDown(object sender, KeyEventArgs e)
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

                //MessageBox.Show("Exception\n " + ex.Message, "QAT Error Code - EC12108", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoopEnd_textBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            try
            {
                FrameworkElement sourceElement = sender as FrameworkElement;
                if (sourceElement == null)
                    return;

                CBMTestControlItem sourceTestCaseItem = sourceElement.DataContext as CBMTestControlItem;
                if (sourceTestCaseItem == null)
                    return;

                if (sourceTestCaseItem != null)
                {
                    CBMTestControlItem originalTestControlItem = null;
                    if (e.OriginalSource.GetType().ToString() == "System.Windows.Controls.TextBox")
                    {
                        var sourceTextBox = (TextBox)e.OriginalSource;
                        if (sourceTextBox != null && sourceTextBox.DataContext != null && String.Equals(sourceTextBox.DataContext.GetType().ToString(), "QSC_Test_Automation.CBMTestControlItem"))
                            originalTestControlItem = (CBMTestControlItem)sourceTextBox.DataContext;
                        string loopEndText = e.Text;
                        e.Handled = IsTextAllowed(loopEndText);
                        if (e.Handled)
                        {
                            if ((originalTestControlItem.ChannelSelectionSelectedItem != string.Empty) && ((originalTestControlItem.ChannelSelectionList.Count > 0)) && ((!string.IsNullOrEmpty(originalTestControlItem.LoopStart))))// & (!string.IsNullOrEmpty((originalTestControlItem.ChannelSelectionSelectedItem))))
                            {
                                Int32 _textloopstart = Convert.ToInt32(originalTestControlItem.LoopStart);
                                Int32 _textloopend = Convert.ToInt32(loopEndText);
                                Int32 channelCount = originalTestControlItem.ChannelSelectionList.Count;
                                if ((_textloopend > 0) && (_textloopend <= channelCount) && (_textloopend > _textloopstart))
                                {
                                    e.Handled = false;
                                }
                                else if (_textloopstart == 0)
                                {
                                    e.Handled = false;
                                }
                                else
                                {
                                    e.Handled = false;
                                }
                            }
                            else
                            {
                                e.Handled = false;
                            }
                        }
                        else
                        {
                            e.Handled = true;
                        }
                        //string loopStartText = originalTestControlItem.LoopStart;

                    }
                }

            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif

                //MessageBox.Show("Exception\n " + ex.Message, "QAT Error Code - EC12106", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void cbm_keydown(object sender, KeyEventArgs e)
        {
            Window target = (Window)sender;

            if (e.Key == Key.Escape)
            {
                Cancel_Click(null, null);
            }
        }
    }
}

