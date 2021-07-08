namespace QSC_Test_Automation
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Data;
    using System.Data.SqlClient;
    using System.Linq;
    using System.Text;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;
    using System.Windows.Documents;
    using System.Windows.Input;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using System.Windows.Navigation;
    using System.Windows.Shapes;
    using System.Collections.ObjectModel;
    using System.Net;
    using System.Threading;
    using System.Net.Sockets;
    using System.Xml;
    using System.Windows.Controls.Primitives;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using System.ComponentModel;
    using System.Web.Script.Serialization;
    using System.IO;

    /// <summary>
    /// Interaction logic for DUT_Configuration.xaml
    /// </summary>
    public partial class DUT_Configuration : Window, INotifyPropertyChanged
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
        
        Test_Execution parentExecutionWindow = null;

        bool AllowToolTip = false;
        string DeviceItemMouseOver = string.Empty;

        private string dutStatusValue = "Test Message";
        public string DutStatus
        {
            get
            {
                if (DeviceDiscovery.discoverySleepStartTime == DateTime.MinValue)
                    return "Device Discovery is in Progress";
                else
                {
                    TimeSpan span = DateTime.Now - DeviceDiscovery.discoverySleepStartTime;
                    return "Discovery will refresh in " + Math.Ceiling(300 - span.TotalSeconds).ToString() + " seconds";
                }
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

        public DUT_Configuration()
        {
            try
            {
                this.InitializeComponent();
                Timer dutStatusUpdate = new Timer(new TimerCallback(DutStatusTimer));
                dutStatusUpdate.Change(0, 1000);
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                //MessageBox.Show("Exception\n " + ex.Message, "QAT Error Code - EC03001", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DUT_Config_ContentRendered(object sender, EventArgs e)
        {
            Top = SystemParameters.PrimaryScreenHeight / 2 - Height / 2;
            if (Top < 0)
                Top = 0;
        }

        public void DUT_UpdateDevice()
        {     
            try
            {
                if (parentExecutionWindow == null)
                    return;

                IsEnabled = false;

                StartWaitForUpdate("Updating Current Build and Design");

                parentExecutionWindow.UpdateNetPairingList(false, true);

                EndWaitForUpdate();

                IsEnabled = true;
            }
            catch (Exception ex)
            {
                EndWaitForUpdate();

                IsEnabled = true;

                MessageBox.Show("Exception\n " + ex.Message, "QAT Error Code - EC03003", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void cmb_netPairing_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                DUT_DeviceItem originalDeviceItem = null;

                ComboBox selectedComboBox = sender as ComboBox;
                if (selectedComboBox != null && String.Equals(selectedComboBox.DataContext.GetType().ToString(), "QSC_Test_Automation.DUT_DeviceItem"))
                    originalDeviceItem = (DUT_DeviceItem)selectedComboBox.DataContext;

                if (originalDeviceItem == null)
                    return;

                string ComboboxSelectedItem = string.Empty;
                bool isQREMtrue = false;

                if (selectedComboBox.SelectedItem != null && selectedComboBox.SelectedItem.ToString() != string.Empty)
                {
                    ComboboxSelectedItem = selectedComboBox.SelectedItem.ToString();
                    originalDeviceItem.QREMcoredetails = null;

                    ////QREM collection details
                    if (originalDeviceItem.ItemNetPairingSelected != "Not Applicable" && originalDeviceItem.ItemNetPairingList != null && originalDeviceItem.ItemNetPairingList.Keys.ToList().Contains(ComboboxSelectedItem) && (originalDeviceItem.ItemNetPairingList[ComboboxSelectedItem] != "Localdevice"))
                    {
                        string[] QREMvalues = originalDeviceItem.ItemNetPairingList[ComboboxSelectedItem].Split(';');
                        originalDeviceItem.QREMcoredetails = QREMvalues;
                        isQREMtrue = true;
                    }
                }

                if (selectedComboBox.SelectedItem != null && selectedComboBox.SelectedItem.ToString() == "Not Applicable")
                {
                    //////
                    foreach (var check in originalDeviceItem.ItemNetPairingList_duplicate)
                    {
                        if (!originalDeviceItem.ItemNetPairingListForXAML.Contains(check.Key))
                            originalDeviceItem.ItemNetPairingListForXAML.Add(check.Key);

                        if (!originalDeviceItem.ItemNetPairingList.Keys.ToList().Contains(check.Key))
                            originalDeviceItem.ItemNetPairingList.Add(check.Key, check.Value);
                    }
                }
                else
                {
                    bool continued = false;
                    List<DUT_DeviceItem> list_originalDeviceItem = new List<DUT_DeviceItem>();
                    list_originalDeviceItem = new List<DUT_DeviceItem>((ObservableCollection<DUT_DeviceItem>)this.dataGrid_ConfigFile.DataContext);

                    foreach (var getlist in originalDeviceItem.ItemNetPairingList_duplicate)
                    {
                        if (!continued)
                        {
                            if ((getlist.Key != "Not Applicable") && (getlist.Key != string.Empty))
                            {
                                if (originalDeviceItem.ItemPrimaryorBackup == "backup")
                                {
                                    string get_Bname = originalDeviceItem.ItemDeviceName;
                                    string get_Pname = originalDeviceItem.Itemlinked;
                                    string get_type = originalDeviceItem.ItemDeviceType;
                                    string get_model = originalDeviceItem.ItemDeviceModel;
                                    foreach (DUT_DeviceItem eachitem in list_originalDeviceItem)
                                    {
                                        if (eachitem.ItemDeviceName.Equals(get_Pname, StringComparison.CurrentCultureIgnoreCase) && (eachitem.ItemDeviceModel == get_model) && (eachitem.ItemDeviceType == get_type) && (get_Bname.Trim().ToUpper() != get_Pname.Trim().ToUpper()))
                                        {
                                            string whatselected = eachitem.ItemNetPairingSelected;

                                            foreach (var check in originalDeviceItem.ItemNetPairingList_duplicate)
                                            {
                                                if (!originalDeviceItem.ItemNetPairingListForXAML.Contains(check.Key))
                                                    originalDeviceItem.ItemNetPairingListForXAML.Add(check.Key);

                                                if (!originalDeviceItem.ItemNetPairingList.Keys.ToList().Contains(check.Key))
                                                    originalDeviceItem.ItemNetPairingList.Add(check.Key, check.Value);
                                            }

                                            if ((whatselected != null) && (ComboboxSelectedItem != null) && (whatselected != string.Empty) && (ComboboxSelectedItem != string.Empty) && (whatselected != "Not Applicable") && (ComboboxSelectedItem != "Not Applicable"))
                                            {
                                                bool isQOriginalDeviceQREM = false;
                                                if (originalDeviceItem.QREMcoredetails != null && originalDeviceItem.QREMcoredetails.Count() > 0)
                                                {
                                                    isQOriginalDeviceQREM = true;
                                                }

                                                bool isEachDeviceQREM = false;
                                                if ((eachitem.QREMcoredetails != null && eachitem.QREMcoredetails.Count() > 0))
                                                {
                                                    isEachDeviceQREM = true;
                                                }

                                                if ((isQOriginalDeviceQREM && !isEachDeviceQREM) || (!isQOriginalDeviceQREM && isEachDeviceQREM))
                                                {
                                                    originalDeviceItem.ItemNetPairingSelected = null;
                                                    originalDeviceItem.ItemCurrentBuild = string.Empty;
                                                    originalDeviceItem.ItemCurrentDesign = string.Empty;
                                                    originalDeviceItem.blnIDColor = string.Empty;
                                                    originalDeviceItem.BtnDateColor = string.Empty;
                                                    originalDeviceItem.CoreRestoreDesign = false;
                                                    originalDeviceItem.ClearLogs = false;
                                                    originalDeviceItem.Bypass = false;
                                                    continued = true;

                                                    if (selectedComboBox.IsDropDownOpen)
                                                    {
                                                        if (!isQOriginalDeviceQREM)
                                                        { MessageBox.Show("Please select remote backup core, selected primary core is remote", "Error Code - EC15038", MessageBoxButton.OK, MessageBoxImage.Error); }
                                                        else { MessageBox.Show("Please select local backup core,  selected primary core is local", "Error Code - EC15038", MessageBoxButton.OK, MessageBoxImage.Error); }

                                                        return;
                                                    }

                                                    break;
                                                }
                                                else if ((whatselected == ComboboxSelectedItem))
                                                {
                                                    if (originalDeviceItem.ItemNetPairingList.Keys.ToList().Contains(whatselected))
                                                        originalDeviceItem.ItemNetPairingList.Remove(whatselected);

                                                    if (originalDeviceItem.ItemNetPairingListForXAML.Contains(whatselected))
                                                    {
                                                        originalDeviceItem.ItemNetPairingListForXAML.Remove(whatselected);
                                                        //originalDeviceItem.ItemNetPairingListWithQREM.Remove(whatselected);
                                                        originalDeviceItem.ItemNetPairingSelected = null;
                                                        originalDeviceItem.ItemCurrentBuild = string.Empty;
                                                        originalDeviceItem.ItemCurrentDesign = string.Empty;
                                                        originalDeviceItem.blnIDColor = string.Empty;
                                                        originalDeviceItem.BtnDateColor = string.Empty;
                                                        originalDeviceItem.CoreRestoreDesign = false;
                                                        originalDeviceItem.ClearLogs = false;
                                                        originalDeviceItem.Bypass = false;
                                                    }

                                                    continued = true;

                                                    if (selectedComboBox.IsDropDownOpen)
                                                    {
                                                        MessageBox.Show("Device " + ComboboxSelectedItem + " is already selected as Primary ", "Error Code - EC15038", MessageBoxButton.OK, MessageBoxImage.Error);
                                                        return;
                                                    }

                                                    break;
                                                }
                                            }
                                        }
                                    }
                                }

                                if (originalDeviceItem.ItemPrimaryorBackup == "primary")
                                {
                                    string get_Pname = originalDeviceItem.ItemDeviceName;
                                    string get_Bname = originalDeviceItem.Itemlinked;
                                    string get_type = originalDeviceItem.ItemDeviceType;
                                    string get_model = originalDeviceItem.ItemDeviceModel;

                                    if (!string.IsNullOrEmpty(get_Bname))
                                    {
                                        foreach (DUT_DeviceItem eachitem in list_originalDeviceItem)
                                        {
                                            if (eachitem.ItemDeviceName.Equals(get_Bname, StringComparison.CurrentCultureIgnoreCase) && (eachitem.ItemDeviceModel == get_model) && (eachitem.ItemDeviceType == get_type) && (get_Bname.Trim().ToUpper() != get_Pname.Trim().ToUpper()))
                                            {
                                                string whatselected = eachitem.ItemNetPairingSelected;

                                                foreach (var check in originalDeviceItem.ItemNetPairingList_duplicate)
                                                {
                                                    if (!originalDeviceItem.ItemNetPairingListForXAML.Contains(check.Key))
                                                        originalDeviceItem.ItemNetPairingListForXAML.Add(check.Key);

                                                    if (!originalDeviceItem.ItemNetPairingList.Keys.ToList().Contains(check.Key))
                                                        originalDeviceItem.ItemNetPairingList.Add(check.Key, check.Value);
                                                }
                                                if ((whatselected != null) && (ComboboxSelectedItem != null) && (whatselected != string.Empty) && (ComboboxSelectedItem != string.Empty) && (whatselected != "Not Applicable") && (ComboboxSelectedItem != "Not Applicable"))
                                                {
                                                    bool isQOriginalDeviceQREM = false;
                                                    if (originalDeviceItem.QREMcoredetails != null && originalDeviceItem.QREMcoredetails.Count() > 0)
                                                    {
                                                        isQOriginalDeviceQREM = true;
                                                    }

                                                    bool isEachDeviceQREM = false;
                                                    if (eachitem.QREMcoredetails != null && eachitem.QREMcoredetails.Count() > 0)
                                                    {
                                                        isEachDeviceQREM = true;
                                                    }

                                                    if ((isQOriginalDeviceQREM && !isEachDeviceQREM) || (!isQOriginalDeviceQREM && isEachDeviceQREM))
                                                    {
                                                        originalDeviceItem.ItemNetPairingSelected = null;
                                                        originalDeviceItem.ItemCurrentBuild = string.Empty;
                                                        originalDeviceItem.ItemCurrentDesign = string.Empty;
                                                        originalDeviceItem.blnIDColor = string.Empty;
                                                        originalDeviceItem.BtnDateColor = string.Empty;
                                                        originalDeviceItem.CoreRestoreDesign = false;
                                                        originalDeviceItem.ClearLogs = false;
                                                        originalDeviceItem.Bypass = false;
                                                        continued = true;

                                                        if (selectedComboBox.IsDropDownOpen)
                                                        {
                                                            if (!isQOriginalDeviceQREM)
                                                            { MessageBox.Show("Please select remote primary core, selected backup core is remote", "Error Code - EC15038", MessageBoxButton.OK, MessageBoxImage.Error); }
                                                            else { MessageBox.Show("Please select local primary core,  selected backup core is local", "Error Code - EC15038", MessageBoxButton.OK, MessageBoxImage.Error); }

                                                            return;
                                                        }

                                                        break;
                                                    }
                                                    else if (whatselected == ComboboxSelectedItem)
                                                    {
                                                        if (originalDeviceItem.ItemNetPairingList.Keys.ToList().Contains(whatselected))
                                                            originalDeviceItem.ItemNetPairingList.Remove(whatselected);

                                                        if (originalDeviceItem.ItemNetPairingListForXAML.Contains(whatselected))
                                                        {
                                                            originalDeviceItem.ItemNetPairingListForXAML.Remove(whatselected);
                                                            originalDeviceItem.ItemNetPairingSelected = null;
                                                            originalDeviceItem.ItemCurrentBuild = string.Empty;
                                                            originalDeviceItem.ItemCurrentDesign = string.Empty;
                                                            originalDeviceItem.blnIDColor = string.Empty;
                                                            originalDeviceItem.BtnDateColor = string.Empty;
                                                            originalDeviceItem.CoreRestoreDesign = false;
                                                            originalDeviceItem.ClearLogs = false;
                                                            originalDeviceItem.Bypass = false;
                                                        }

                                                        continued = true;

                                                        if (selectedComboBox.IsDropDownOpen)
                                                        {
                                                            MessageBox.Show("Device " + ComboboxSelectedItem + " is already selected as Backup ", "Error Code - EC15038", MessageBoxButton.OK, MessageBoxImage.Error);
                                                            return;
                                                        }

                                                        break;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                
                if (selectedComboBox.SelectedItem != null && selectedComboBox.SelectedItem.ToString() != string.Empty)
                {                        
					///Local collection details (IP)
                    if (originalDeviceItem.ItemNetPairingSelected != "Not Applicable" && !isQREMtrue)
                    {
                        string[] availabledevicelist = (string[])DeviceDiscovery.AvailableDeviceList.ToArray(typeof(string));
                        string[] netdevice = availabledevicelist.Where(x => x.Contains(originalDeviceItem.ItemNetPairingSelected)).ToArray();
                        originalDeviceItem.ItemPrimaryIPSelected = null;
                        originalDeviceItem.ItemSecondaryIPSelected = null;

                        foreach (string checkstring in netdevice)
                        {
                            string[] splitComponentdetails = checkstring.Split(',');
                            if ((splitComponentdetails != null) && (!string.IsNullOrEmpty(splitComponentdetails[1])) && (splitComponentdetails[1] == originalDeviceItem.ItemNetPairingSelected))
                            {
                                if (!string.IsNullOrEmpty(splitComponentdetails[2]))
                                    originalDeviceItem.ItemPrimaryIPSelected = splitComponentdetails[2];

                                if ((splitComponentdetails.Count() > 3) && (!string.IsNullOrEmpty(splitComponentdetails[3])))
                                    originalDeviceItem.ItemSecondaryIPSelected = splitComponentdetails[3];
                            }
                        }
                    }
                    else
                    {
                        originalDeviceItem.ItemPrimaryIPSelected = "Not Applicable";
                        originalDeviceItem.ItemSecondaryIPSelected = "Not Applicable";
                    }

                    ///For dynamic pair tooltip
                    if (!ComboboxSelectedItem.Contains("Not Applicable"))
                    {
                        string comboSelectedLocal = selectedComboBox.SelectedItem.ToString();

                        if (isQREMtrue && originalDeviceItem.QREMcoredetails.Count() > 0)
                            comboSelectedLocal = originalDeviceItem.QREMcoredetails[0];

                        bool result = comboSelectedLocal.Equals(originalDeviceItem.ItemDeviceName, StringComparison.InvariantCultureIgnoreCase);
                        if (AllowToolTip && !result)
                        {
                            selectedComboBox.ToolTip = "Connect to designer action will fail, if core dynamically paired";
                            selectedComboBox.SetValue(ToolTipService.IsEnabledProperty, true);
                        }
                        else
                        {
                            selectedComboBox.SetValue(ToolTipService.IsEnabledProperty, false);
                        }
                    }
                    else
                    {
                        selectedComboBox.SetValue(ToolTipService.IsEnabledProperty, false);
                    }

                    ////Based on selection Design and core details and ID mode and Datetime set color fetching
                    int index = originalDeviceItem.ItemNetPairingList.Keys.ToList().IndexOf(originalDeviceItem.ItemNetPairingSelected);

                    if (index >= 0)
                    {
                        if (isQREMtrue)
                        {
                            Core_DateTime coredatetime = new Core_DateTime();
                            originalDeviceItem.CoreDateTimeList = coredatetime;
                            GetFirmwareDesignNameAndIDmodeForReflect(originalDeviceItem);
                            GetCoreDateTime(originalDeviceItem, false, isQREMtrue, string.Empty);
                        }
                        else if (originalDeviceItem.ItemPrimaryIPSelected != "Not Applicable" | originalDeviceItem.ItemSecondaryIPSelected != "Not Applicable")
                        {
                            Core_DateTime coredatetime = new Core_DateTime();
                            originalDeviceItem.CoreDateTimeList = coredatetime;
                            string coreLogonToken = string.Empty;
                            GetCurrentBuildAndDesignVersion(originalDeviceItem, out coreLogonToken);
                            GetCoreDateTime(originalDeviceItem, false, isQREMtrue, coreLogonToken);
                        }
                        else
                        {
                            originalDeviceItem.ItemCurrentBuild = string.Empty;
                            originalDeviceItem.ItemCurrentDesign = string.Empty;
                            originalDeviceItem.blnIDColor = "Black";
                            originalDeviceItem.BtnDateColor = "Black";
                        }
                    }
                }
                else
                {
                    originalDeviceItem.ItemCurrentBuild = string.Empty;
                    originalDeviceItem.ItemCurrentDesign = string.Empty;
                    originalDeviceItem.blnIDColor = "Black";
                    originalDeviceItem.BtnDateColor = "Black";
                }
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                //MessageBox.Show("Exception\n " + ex.Message, "QAT Error Code - EC03004", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        private void GetCurrentBuildAndDesignVersion(DUT_DeviceItem originalDeviceItem, out string coreLogonToken)
        {
            coreLogonToken = string.Empty;

            try
            {
                if (!(string.Equals(originalDeviceItem.ItemDeviceType, "Camera", StringComparison.CurrentCultureIgnoreCase)))
                {
                    if (originalDeviceItem.ItemNetPairingSelected != null)
                    {
                        if (string.Equals(originalDeviceItem.ItemDeviceType, "Core", StringComparison.CurrentCultureIgnoreCase))
                        {
                            if (originalDeviceItem.ItemPrimaryIPSelected == null || originalDeviceItem.ItemPrimaryIPSelected == string.Empty || originalDeviceItem.ItemPrimaryIPSelected == "Not Applicable")
                            {
                                if (originalDeviceItem.ItemSecondaryIPSelected != null && originalDeviceItem.ItemSecondaryIPSelected != string.Empty && originalDeviceItem.ItemSecondaryIPSelected != "Not Applicable")
                                {
                                    originalDeviceItem.CoreDateTimeList.DateTimeIPAddress = originalDeviceItem.ItemSecondaryIPSelected;
                                }
                            }
                            else
                            {
                                originalDeviceItem.CoreDateTimeList.DateTimeIPAddress = originalDeviceItem.ItemPrimaryIPSelected;
                            }

                            string outresponse = string.Empty;
                            var isaccessopen = AccessOpen(originalDeviceItem.CoreDateTimeList.DateTimeIPAddress, originalDeviceItem, out outresponse);
                            if (isaccessopen.Item2)
                            {
                                MessageBox.Show("Device is not available in the network.", "Error Code - EC15024", MessageBoxButton.OK, MessageBoxImage.Warning);
                                return;
                            }

                            if (outresponse == "404")
                            {
                                MessageBox.Show("The remote server returned an error: (404) URL Not Found.", "Error Code - EC15024", MessageBoxButton.OK, MessageBoxImage.Error);
                                return;
                            }

                            if (isaccessopen.Item1 == false)
                            {
                                var result = Corelogon(originalDeviceItem.CoreDateTimeList.DateTimeIPAddress, Properties.Settings.Default.DevicePassword.ToString(), out outresponse);

                                if(outresponse == "401")
                                {
                                    MessageBox.Show("Please enter correct Username/password in preferences.", "Error Code - EC15024", MessageBoxButton.OK, MessageBoxImage.Error);
                                    return;
                                }

                                coreLogonToken = result.Item3;
                            }

                            string currentdesignName = string.Empty;
                            GetLocaldeviceSystemDetails(originalDeviceItem, coreLogonToken, out currentdesignName);
                            originalDeviceItem.ItemCurrentDesign = currentdesignName;
                                                        
                            string itemBuild = string.Empty;
                            GetLocalDeviceCoredetails(originalDeviceItem, coreLogonToken, out itemBuild);
                            originalDeviceItem.ItemCurrentBuild = itemBuild;
                            
                            string btnColor = GetIDmodeDetailForLocalRemote(originalDeviceItem, coreLogonToken);

                            if (btnColor == "Yes")
                                originalDeviceItem.blnIDColor = "LightGreen";
                            else if (btnColor == "No")
                                originalDeviceItem.blnIDColor = "Red";
                            else
                                originalDeviceItem.blnIDColor = "Black";
                        }
                        else
                        {
                            XmlDocument xml = new XmlDocument();

                            if (originalDeviceItem.ItemPrimaryIPSelected != string.Empty)
                            {
                                xml = DeviceDiscovery.XmlLoadUsingHttp("http://" + originalDeviceItem.ItemPrimaryIPSelected + "//cgi-bin/status_xml", "GetCurrentBuildAndDesignVersion");
                                if (xml == null)
                                {
                                    xml = DeviceDiscovery.XmlLoadUsingHttp("http://" + originalDeviceItem.ItemPrimaryIPSelected + "/cgi-bin/status_xml", "GetCurrentBuildAndDesignVersion");

                                }
                            }
                            else if (originalDeviceItem.ItemSecondaryIPSelected != string.Empty)
                            {
                                xml = DeviceDiscovery.XmlLoadUsingHttp("http://" + originalDeviceItem.ItemSecondaryIPSelected + "//cgi-bin/status_xml", "GetCurrentBuildAndDesignVersion");
                                if (xml == null)
                                {
                                    xml = DeviceDiscovery.XmlLoadUsingHttp("http://" + originalDeviceItem.ItemSecondaryIPSelected + "/cgi-bin/status_xml", "GetCurrentBuildAndDesignVersion");
                                }
                            }
                            else
                            {
                                originalDeviceItem.ItemCurrentBuild = null;
                                originalDeviceItem.ItemCurrentDesign = null;
                                return;
                            }

                            if (xml != null)
                            {
                                XmlNode node = xml.SelectSingleNode("status/firmware_version");
                                originalDeviceItem.ItemCurrentBuild = node.InnerText;

                                XmlNode node2 = xml.SelectSingleNode("status/id_mode");
                                if (String.Equals(node2.InnerText, "on", StringComparison.CurrentCultureIgnoreCase))
                                    originalDeviceItem.blnIDColor = "LightGreen";
                                else
                                    originalDeviceItem.blnIDColor = "red";


                                XmlNode node1 = xml.SelectSingleNode("status/design/pretty_name");
                                if (node1.InnerText != "")
                                {
                                    originalDeviceItem.ItemCurrentDesign = node1.InnerText;
                                }
                                else
                                {
                                    originalDeviceItem.ItemCurrentDesign = "No design is running";
                                }
                            }
                            else
                            {
                                originalDeviceItem.ItemCurrentBuild = "Device Not Available";

                                originalDeviceItem.ItemCurrentDesign = "Device Not Available";
                            }
                        }
                    }
                    else
                    {
                        originalDeviceItem.ItemCurrentDesign = null;
                        originalDeviceItem.ItemCurrentBuild = null;
                        originalDeviceItem.blnIDColor = "Black";
                    }
                }
            }
            catch (Exception ex)
            {
                //MessageBox.Show("Exception\n " + ex.Message, "QAT Error Code - EC03005", MessageBoxButton.OK, MessageBoxImage.Error);
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                        MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                //DeviceDiscovery.WriteToLogFile("Exception:QAT Error Code - EC03005 " + ex.Message);
            }
        }

        private bool GetLocaldeviceSystemDetails(DUT_DeviceItem originalDeviceItem, string coreLogonToken, out string currentDesignName)
        {
            bool isSuccess = false;
            currentDesignName = string.Empty;

            try
            {
                string strResponse = string.Empty;
                var outval = HttpGetactual("http://" + originalDeviceItem.CoreDateTimeList.DateTimeIPAddress + "/api/v0/systems", originalDeviceItem.CoreDateTimeList.DateTimeIPAddress, coreLogonToken, originalDeviceItem, true, false, out strResponse);

                if (outval.Item1 && !string.IsNullOrEmpty(strResponse))
                {
                    dynamic array = new System.Web.Script.Serialization.JavaScriptSerializer().DeserializeObject(strResponse);

                    if (array != null)
                    {
                        object[] objList = array as object[];

                        if (objList != null && objList.Count() > 0)
                        {
                            foreach (var obj in objList)
                            {
                                Dictionary<string, object> coreDetails = obj as Dictionary<string, object>;

                                if (coreDetails != null && coreDetails.Count > 0)
                                {
                                    foreach (var coreItems in coreDetails)
                                    {
                                        if (coreItems.Key == "revision")
                                        {
                                            Dictionary<string, object> getrevisionDetails = coreItems.Value as Dictionary<string, object>;

                                            if (getrevisionDetails != null && getrevisionDetails.Count > 0)
                                            {
                                                foreach (var revisionDetails in getrevisionDetails)
                                                {
                                                    if (revisionDetails.Key == "design")
                                                    {
                                                        Dictionary<string, object> getdesignDetails = revisionDetails.Value as Dictionary<string, object>;

                                                        foreach (var designDetails in getdesignDetails)
                                                        {
                                                            if (designDetails.Key == "name")
                                                            {
                                                                currentDesignName = designDetails.Value.ToString();
                                                                isSuccess = true;
                                                                break;
                                                            }
                                                        }
                                                    }

                                                    if (isSuccess) break;
                                                }
                                            }
                                        }

                                        if (isSuccess) break;
                                    }
                                }

                                if (isSuccess) break;
                            }
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
            }

            return isSuccess;
        }

        private bool GetLocalDeviceCoredetails(DUT_DeviceItem originalDeviceItem, string coreLogonToken, out string itemCurrentBuild)
        {
            bool isSuccess = false;
            itemCurrentBuild = string.Empty;

            try
            {
                string strResponse = string.Empty;
                var outval = HttpGetactual("http://" + originalDeviceItem.CoreDateTimeList.DateTimeIPAddress + "/api/v0/cores/self", originalDeviceItem.CoreDateTimeList.DateTimeIPAddress, coreLogonToken, originalDeviceItem, true, false, out strResponse);

                if (outval.Item1 && !string.IsNullOrEmpty(strResponse))
                {
                    dynamic array = new System.Web.Script.Serialization.JavaScriptSerializer().DeserializeObject(strResponse);

                    if (array != null)
                    {
                        Dictionary<string, object> coreDetails = array as Dictionary<string, object>;

                        if (coreDetails != null && coreDetails.Count > 0)
                        {
                            foreach (var coreItems in coreDetails)
                            {
                                if (coreItems.Key == "firmware")
                                {
                                    itemCurrentBuild = coreItems.Value.ToString();
                                    isSuccess = true;
                                    break;
                                }
                            }
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
            }

            return isSuccess;
        }
        
        private string GetIDmodeDetailForLocalRemote(DUT_DeviceItem originalDeviceItem, string coreLogonToken)
        {
            string returnString = string.Empty;

            try
            {
                string strResponse = string.Empty;
                Tuple<bool, string, bool> outval = null;

                if (originalDeviceItem.QREMcoredetails != null && originalDeviceItem.QREMcoredetails.Count() > 2)
                {
                    string id = originalDeviceItem.QREMcoredetails[1];
                    outval = HttpGetactual("https://" + Properties.Settings.Default.QREMreflectLink + "/api/v0/cores/" + id + "/config/id_mode?", string.Empty, DeviceDiscovery.QREM_Token, null, true, true, out strResponse);
                }
                else
                {
                    outval = HttpGetactual("http://" + originalDeviceItem.CoreDateTimeList.DateTimeIPAddress + "/api/v0/cores/self/config/id_mode?meta=permissions", originalDeviceItem.CoreDateTimeList.DateTimeIPAddress, coreLogonToken, originalDeviceItem, true, false, out strResponse);
                }

                if(outval != null && outval.Item1 && !string.IsNullOrEmpty(strResponse))
                {
                    dynamic array = new System.Web.Script.Serialization.JavaScriptSerializer().DeserializeObject(strResponse);

                    if (array != null && array.Count > 0)
                    {
                        foreach (var arrayitem in array)
                        {
                            if (arrayitem.Key == "data")
                            {
                                Dictionary<string, object> objdict = arrayitem.Value as Dictionary<string, object>;

                                if (objdict != null && objdict.Count > 0)
                                {
                                    foreach (var name in objdict)
                                    {
                                        if (name.Key == "mode")
                                        {
                                            bool idMode = Convert.ToBoolean(name.Value);

                                            if (idMode)
                                                returnString = "Yes";
                                            else
                                                returnString = "No";
                                        }
                                    }
                                }
                            }
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
            }

            return returnString;
        }

        private void SaveNetConfig_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.Close();
            }
            catch (Exception ex)
            {
                //MessageBox.Show("Exception\n " + ex.Message, "QAT Error Code - EC03006", MessageBoxButton.OK, MessageBoxImage.Error);
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                //DeviceDiscovery.WriteToLogFile("QAT Error Code - EC03006 - Exception: " + ex.Message);
            }
        }

        private void StartWaitForUpdate(string waitMessage)
        {
            try
            {
                //this.txtStatus.Text = waitMessage;

                this.Cursor = Cursors.Wait;
                this.dataGrid_ConfigFile.Cursor = Cursors.Wait;
                if (Mouse.OverrideCursor != Cursor)
                    Mouse.OverrideCursor = Cursor;
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                //MessageBox.Show("Exception\n " + ex.Message, "QAT Error Code - EC03008", MessageBoxButton.OK, MessageBoxImage.Error);
            }
                   
        }

        private void EndWaitForUpdate()
        {
            try
            {
                this.Cursor = Cursors.Arrow;
                dataGrid_ConfigFile.Cursor = Cursors.Arrow;
                // The check is required to prevent cursor flickering
                if (Mouse.OverrideCursor != Cursor)
                    Mouse.OverrideCursor = null;
                //this.txtStatus.Text = "";
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                //MessageBox.Show("Exception\n " + ex.Message, "QAT Error Code - EC03009", MessageBoxButton.OK, MessageBoxImage.Error);
            }           
        }

        public void ID_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string current_netpairname = string.Empty;
                string currentcolor = string.Empty;
                DUT_DeviceItem originalDeviceItem = null;

                Button IDbutton = sender as Button;
                originalDeviceItem = (DUT_DeviceItem)IDbutton.DataContext;

                if (originalDeviceItem.ItemNetPairingSelected != null & originalDeviceItem.ItemNetPairingSelected != "Not Applicable" & originalDeviceItem.ItemNetPairingSelected != string.Empty)
                {
                    string id_status = GetIDmodeDetailForLocalRemote(originalDeviceItem, string.Empty);

                    bool isQREM = false;
                    string coreid = string.Empty;

                    if (originalDeviceItem != null && originalDeviceItem.ItemNetPairingList != null && originalDeviceItem.ItemNetPairingList.Keys.ToList().Contains(originalDeviceItem.ItemNetPairingSelected) && (originalDeviceItem.ItemNetPairingList[originalDeviceItem.ItemNetPairingSelected] != "Localdevice"))
                    {
                        if (originalDeviceItem.QREMcoredetails != null && originalDeviceItem.QREMcoredetails.Count() > 1)
                        {
                            isQREM = true;
                            coreid = originalDeviceItem.QREMcoredetails[1];
                        }
                    }

                    string IPaddresstouse = string.Empty;
                    string coreToken = string.Empty;

                    if (!isQREM)
                    {
                        string primaryIPAddress = originalDeviceItem.ItemPrimaryIPSelected;
                        string secondaryIPAddress = originalDeviceItem.ItemSecondaryIPSelected;

                        if (primaryIPAddress != string.Empty && primaryIPAddress != null && primaryIPAddress != "Not Applicable")
                        {
                            IPaddresstouse = primaryIPAddress;
                        }
                        else if (secondaryIPAddress != string.Empty && secondaryIPAddress != null && secondaryIPAddress != "Not Applicable")
                        {
                            IPaddresstouse = secondaryIPAddress;
                        }

                        if (IPaddresstouse != string.Empty && IPaddresstouse != null && IPaddresstouse != "Not Applicable")
                        {
                            if (sender.GetType().FullName == "System.Windows.Controls.Button")
                            {
                                if (originalDeviceItem.ItemDeviceModel.ToUpper().Contains("CORE"))
                                {
                                    string outresponse = string.Empty;
                                    var isaccessopen = AccessOpen(IPaddresstouse, originalDeviceItem, out outresponse);
                                    if (isaccessopen.Item2)
                                    {
                                        MessageBox.Show("Device is not available in the network.", "Error Code - EC15024", MessageBoxButton.OK, MessageBoxImage.Warning);
                                        return;
                                    }
                                    if (outresponse == "404")
                                    {
                                        MessageBox.Show("The remote server returned an error: (404) URL Not Found.", "Error Code - EC15024", MessageBoxButton.OK, MessageBoxImage.Error);
                                        return;
                                    }

                                    if (isaccessopen.Item1 == false)
                                    {
                                        var result = Corelogon(IPaddresstouse, Properties.Settings.Default.DevicePassword.ToString(), out outresponse);

                                        if (outresponse == "404")
                                        {
                                            MessageBox.Show("Please enter correct Username/password in preferences to enable/disable device IDmode.", "Error Code - EC15024", MessageBoxButton.OK, MessageBoxImage.Error);
                                            return;
                                        }

                                        coreToken = result.Item3;
                                    }
                                }
                            }
                        }
                    }

                    SetIDMode(id_status, isQREM, originalDeviceItem, IPaddresstouse, coreToken, coreid);

                    current_netpairname = originalDeviceItem.ItemNetPairingSelected.ToString();
                    currentcolor = originalDeviceItem.blnIDColor;

                    foreach (DUT_DeviceItem itemname_netpair in DeviceDiscovery.selectedDutDeviceItemListColor)
                    {
                        if (itemname_netpair.ItemNetPairingSelected != null & originalDeviceItem.ItemNetPairingSelected != "Not Applicable")
                        {
                            if (itemname_netpair.ItemNetPairingSelected.ToString() == current_netpairname)
                            {
                                itemname_netpair.blnIDColor = currentcolor;
                            }
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
                //MessageBox.Show("Exception\n " + ex.Message, "QAT Error Code - EC03020", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SetIDMode(string id_status, bool isQREM, DUT_DeviceItem originalDeviceItem, string IPaddresstouse, string coreToken, string coreid)
        {
            try
            {
                string deviceName = string.Empty;
                string strResponse = string.Empty;
                Tuple<bool, string> idModesuccess = null;

                if (id_status == "No")
                {
                    string strparameters = "{\"mode\":true}";

                    if (!isQREM)
                        idModesuccess = HttpPut_Post_actual_json("http://" + IPaddresstouse + "/api/v0/cores/self/config/id_mode", "PUT", strparameters, coreToken, IPaddresstouse, originalDeviceItem, true, out strResponse);
                    else
                        idModesuccess = HttpPut_Post_actual_json("https://" + Properties.Settings.Default.QREMreflectLink + "/api/v0/cores/" + coreid + "/config/id_mode?", "PUT", strparameters, DeviceDiscovery.QREM_Token, string.Empty, null, true, out strResponse);

                    if (!string.IsNullOrEmpty(idModesuccess.Item2))
                        deviceName = idModesuccess.Item2;

                    if (idModesuccess.Item1)
                        originalDeviceItem.blnIDColor = "LightGreen";
                }
                else if (id_status == "Yes")
                {
                    string strparameters = "{\"mode\":false}";

                    if (!isQREM)
                        idModesuccess = HttpPut_Post_actual_json("http://" + IPaddresstouse + "/api/v0/cores/self/config/id_mode", "PUT", strparameters, coreToken, IPaddresstouse, originalDeviceItem, true, out strResponse);
                    else
                        idModesuccess = HttpPut_Post_actual_json("https://" + Properties.Settings.Default.QREMreflectLink + "/api/v0/cores/" + coreid + "/config/id_mode?", "PUT", strparameters, DeviceDiscovery.QREM_Token, string.Empty, null, true, out strResponse);

                    if (!string.IsNullOrEmpty(idModesuccess.Item2))
                        deviceName = idModesuccess.Item2;

                    if (idModesuccess.Item1)
                        originalDeviceItem.blnIDColor = "Red";
                }
                else
                    originalDeviceItem.blnIDColor = "Black";

            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                //MessageBox.Show("Exception\n " + ex.Message, "QAT Error Code - EC03020", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        public string XmlReadToGet_IDButton_Status(DUT_DeviceItem originalDeviceItem)
        {            
            try
            {
                XmlDocument xml = new XmlDocument();
                if (originalDeviceItem.ItemPrimaryIPSelected != string.Empty)
                {
                    xml = DeviceDiscovery.XmlLoadUsingHttp("http://" + originalDeviceItem.ItemPrimaryIPSelected + "//cgi-bin/status_xml", "XmlReadToGetIDStatus");
                    if (xml == null)                    
                        xml = DeviceDiscovery.XmlLoadUsingHttp("http://" + originalDeviceItem.ItemPrimaryIPSelected + "/cgi-bin/status_xml", "XmlReadToGetIDStatus");                                 
                }
                else if (originalDeviceItem.ItemSecondaryIPSelected != string.Empty)
                {
                    xml = DeviceDiscovery.XmlLoadUsingHttp("http://" + originalDeviceItem.ItemSecondaryIPSelected + "//cgi-bin/status_xml", "XmlReadToGetIDStatus");
                    if (xml == null)                    
                       xml = DeviceDiscovery.XmlLoadUsingHttp("http://" + originalDeviceItem.ItemSecondaryIPSelected + "/cgi-bin/status_xml", "XmlReadToGetIDStatus");                    
                }

                if(xml != null)
                {
                   XmlNode node = xml.SelectSingleNode("status/id_mode");
                   return node.InnerText;
                }            
            }
            catch (Exception ex)
            {               
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                ExecutionMessageBox("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif              
            }
            return string.Empty;
        }
        
        public Tuple<bool, bool> AccessOpen(string ipaddress, DUT_DeviceItem deviceItem, out string strResponse)
        {
            bool state = false;
            strResponse = string.Empty;       
            try
            {
                var value = HttpGetactual("http://" + ipaddress + "/api/v0/cores/self/access_mode", ipaddress, string.Empty, deviceItem, true, true, out strResponse);

                if (value.Item1 && strResponse != string.Empty && strResponse != "401" && strResponse != "404")
                {
                    var obj = new JavaScriptSerializer { MaxJsonLength = Int32.MaxValue }.Deserialize<dynamic>(strResponse);
                    if (obj.Count > 0)
                    {
                        foreach (var res in obj)
                        {
                            if ((res.Key.Contains("accessMode")) && (res.Value == "open"))
                            {
                                state = true;
                            }
                        }
                    }
                }
                return new Tuple<bool, bool>(state, value.Item3);
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                return new Tuple<bool, bool>(state, false);
            }
        }


        public Tuple<bool, string, string> Corelogon(string ipaddress, string userpassword, out string strResponse)
        {
            strResponse = string.Empty;
            string outToken = string.Empty;
            string strParameters = "{\"username\":\"" + Properties.Settings.Default.DeviceUsername.ToString() + "\",\"password\":\"" + userpassword + "\"}";
            try
            {
                var success = HttpPut_Post_json("http://" + ipaddress + "/api/v0/logon", "POST", strParameters,string.Empty, ipaddress,null, true, out strResponse);
            
                //get Token value
                if (success.Item1)
                {
                    var obj = new JavaScriptSerializer { MaxJsonLength = Int32.MaxValue }.Deserialize<dynamic>(strResponse);
                    if (obj.Count > 0)
                    {
                        foreach (var response in obj)
                        {
                            outToken = response.Value;
                        }
                    }
                }
                               
                return new Tuple<bool, string, string>(success.Item1, success.Item2, outToken);
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                return new Tuple<bool, string, string >(false, string.Empty, string.Empty);
            }
        }


        private bool HttpStatusCodeCheck(HttpWebResponse response, string methodName, out string strResponse)
        {
            strResponse = string.Empty;
            try
            {
                if (response == null)
                    return false;

                if (((methodName == "GET") && (response.StatusCode == HttpStatusCode.OK))
                    || ((methodName == "POST") && (response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.Created))
                    || ((methodName == "PUT") && (response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.Created || response.StatusCode == HttpStatusCode.NoContent)))
                {
                    using (System.IO.StreamReader sr = new System.IO.StreamReader(response.GetResponseStream()))
                    {
                        strResponse = sr.ReadToEnd().Trim();
                        return true;
                    }
                }

                return false;
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
        
        public Tuple<bool, string, bool> HttpGetactual(string strURI, string deviceIP,string logontoken, DUT_DeviceItem deviceItem,bool isnewVersion, bool isQREM, out string strResponse)
        {
            bool isdeviceNotAvailable = false;
            string deviceName = string.Empty;
            strResponse = string.Empty;
            System.Net.HttpWebRequest req = (HttpWebRequest)System.Net.WebRequest.Create(strURI);
            try
            {
                if((deviceItem != null && deviceItem.ItemDeviceModel.ToUpper().Contains("CORE") && isnewVersion) || isQREM)
                {
                    req.ContentType = "application/json";
                    if (logontoken != string.Empty)
                        req.Headers["Authorization"] = "Bearer " + logontoken;
                }
                else
                {
                    req.ContentType = "application/x-www-form-urlencoded";
                    SetBasicAuthHeader(ref req, "admin", Properties.Settings.Default.DevicePassword);
                }         
                
                req.Method = "Get";
                req.Timeout = 15000;
                req.ReadWriteTimeout = 15000;

                using (HttpWebResponse resp = (HttpWebResponse)req.GetResponse())
                {
                  bool success = HttpStatusCodeCheck(resp, "GET", out strResponse);
                  return new Tuple<bool, string, bool>(success, deviceName, isdeviceNotAvailable);
                }
            }
            catch (Exception ex)
            { 
                if (!ex.Message.Contains("The remote server returned an error: (401) Unauthorized"))
                {
                    DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG_MESSAGEBOX
                    if (ex.Message != "Thread was being aborted.")
                        MessageBox.Show("Exception\n " + ex.Message, "Error Code - EC15025", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                }

                if (!ex.Message.Contains("The remote server returned an error: (401) Unauthorized"))
                {
                    if (ex.Message != "Thread was being aborted." && ex.Message != "Unable to connect to the remote server")
                        DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif


                    if (ex.Message == "Unable to connect to the remote server" && !isQREM)
                    {
                        isdeviceNotAvailable = true;
                        List<DUT_DeviceItem> deviceItems = new List<DUT_DeviceItem>();
                        deviceItems.Add(deviceItem);
                        deviceName = DeviceDiscovery.GetAlldeviceNameForSelectedIP(deviceIP, deviceItems);
                        ////MessageBox.Show("Exception\n " + deviceName + " Device is not available", "Error Code - EC15025", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                if (ex.Message.Contains("The remote server returned an error: (401) Unauthorized"))
                {
                    strResponse = "401";
                }
                else if (ex.Message.Contains("The remote server returned an error: (404) Not Found"))
                {
                    strResponse = "404";
                }

                req.Abort();
                return new Tuple<bool, string, bool>(false, deviceName, isdeviceNotAvailable);
            }
        }

        public Tuple<bool, string> HttpPut_Post_json(string strURI, string requestMethod, string strParameters, string token , string deviceIP, DUT_DeviceItem deviceItem, bool isnewversion, out string strResponse)
        {
            Tuple<bool, string> Check = new Tuple<bool, string>(false, string.Empty);
            Int32 RetryCount = 0;
            strResponse = "";
            try
            {
                while (RetryCount < 5)
                {
                    Check = HttpPut_Post_actual_json(strURI, requestMethod, strParameters, token, deviceIP, deviceItem, isnewversion, out strResponse);
                    if ((Check.Item1) || (strResponse == "401") || (strResponse == "404"))
                    {
                        break;
                    }
                    RetryCount++;
                };
                return Check;
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                return Check;
            }
        }
        

        public Tuple<bool, string> HttpPut_Post_actual_json(string strURI, string requestMethod, string strParameters, string token, string deviceIP, DUT_DeviceItem deviceItem, bool isnewversion, out string strResponse)
        {
            string deviceName = string.Empty;
            strResponse = string.Empty;
            bool success = false;
            System.Net.HttpWebRequest req = (HttpWebRequest)System.Net.WebRequest.Create(strURI);
            try
            {
                if (isnewversion)
                {
                    req.ContentType = "application/json";
                    if (token != string.Empty)
                        req.Headers["Authorization"] = "Bearer " + token;
                }
                else
                {
                    req.ContentType = "application/x-www-form-urlencoded";
                    SetBasicAuthHeader(ref req, "admin", Properties.Settings.Default.DevicePassword);
                }
                req.Method = requestMethod;
                req.Timeout = 15000;
                req.ReadWriteTimeout = 15000;

                Byte[] retBytes = System.Text.Encoding.ASCII.GetBytes(strParameters);
                req.ContentLength = retBytes.Length;

              

                using (System.IO.Stream outStream = req.GetRequestStream())
                {
                    outStream.Write(retBytes, 0, retBytes.Length);
                }

                using (HttpWebResponse resp = (HttpWebResponse)req.GetResponse())
                {
                    success = HttpStatusCodeCheck(resp, requestMethod, out strResponse);
                }
            }
            catch (Exception ex)
            {
                strResponse = "";

                if (!ex.Message.Contains("The remote server returned an error: (401) Unauthorized"))
                {
                    DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG_MESSAGEBOX
                    if (ex.Message != "Thread was being aborted.")
                        MessageBox.Show("Exception\n " + ex.Message, "Error Code - EC15025", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                }          

                if (!ex.Message.Contains("The remote server returned an error: (401) Unauthorized"))
                {
                    if (ex.Message != "Thread was being aborted." && ex.Message != "Unable to connect to the remote server")
                        DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif

                    if (ex.Message == "Unable to connect to the remote server" && deviceItem != null && deviceIP != string.Empty)
                    {
                        List<DUT_DeviceItem> deviceItems = new List<DUT_DeviceItem>();
                        deviceItems.Add(deviceItem);
                        deviceName = DeviceDiscovery.GetAlldeviceNameForSelectedIP(deviceIP, deviceItems);                       
                    }
                }
                if (ex.Message.Contains("The remote server returned an error: (401) Unauthorized"))
                {
                    strResponse = "401";
                }
                else if (ex.Message.Contains("The remote server returned an error: (404) Not Found"))
                {
                    strResponse = "404";
                }
            }
            req.Abort();
            return new Tuple<bool, string>(success, deviceName);
        }


        private void SetBasicAuthHeader(ref HttpWebRequest req, string username, string password)
        {
            try
            {
                // Want to check for null arguments.
                if (username == null)
                {
                    username = String.Empty;
                }

                if (password == null)
                {
                    password = String.Empty;
                }

                if (req == null)
                {
                    // Nothing to do here...    ** LOG THIS LATER **
                    //Debug.WriteLine("Web_RW.SetBasicAuthHeader: HttpWebRequest parameter object is null!");
                }

                string strAuth = username + ":" + password;

                try
                {
                    strAuth = Convert.ToBase64String(Encoding.Default.GetBytes(strAuth));
                }
                catch (Exception ex)
                {
                    DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                    //if (ex.Message != "Thread was being aborted.")
                    //    MessageBox.Show("Exception\n " + ex.Message, "Error Code - EC15029", MessageBoxButton.OK, MessageBoxImage.Error);
                    // Output to debug, but then do nothing.  ** LOG THIS LATER **
                    //Debug.WriteLine("Web_RW.SetBasicAuthHeader: " + ex.Message);
                }

                req.Headers["Authorization"] = "Basic " + strAuth;
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

        private void DUT_Config_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                this.dataGrid_ConfigFile.DataContext = null;
                this.dataGrid_GeneratorConfigFile.DataContext = null;

                if (this.Owner != null && this.Owner.Owner != null)
                    this.Owner.Owner = null;
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

        private void DateAndTime_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DUT_DeviceItem originalDeviceItem = null;
                Button IDbutton = sender as Button;
                originalDeviceItem = (DUT_DeviceItem)IDbutton.DataContext;
                
                if (originalDeviceItem != null & originalDeviceItem.ItemNetPairingSelected!=null & originalDeviceItem.ItemNetPairingSelected!=string.Empty & originalDeviceItem.ItemNetPairingSelected!= "Not Applicable")
                {
                    Core_DateTime dateTime = new Core_DateTime();
                    originalDeviceItem.CoreDateTimeList = dateTime;
                    dateTime.ParentDUTDeviceItem = originalDeviceItem;
                    
                    bool isQREMTrue = false;
                    if (originalDeviceItem.ItemNetPairingList != null && originalDeviceItem.ItemNetPairingList.Keys.ToList().Contains(originalDeviceItem.ItemNetPairingSelected) && (originalDeviceItem.ItemNetPairingList[originalDeviceItem.ItemNetPairingSelected] != "Localdevice"))
                        isQREMTrue = true;

                    string coreLogonToken = string.Empty;

                    if (!isQREMTrue)
                    {
                        if (originalDeviceItem.ItemPrimaryIPSelected == null || originalDeviceItem.ItemPrimaryIPSelected == string.Empty || originalDeviceItem.ItemPrimaryIPSelected == "Not Applicable")
                        {
                            if (originalDeviceItem.ItemSecondaryIPSelected != null && originalDeviceItem.ItemSecondaryIPSelected != string.Empty && originalDeviceItem.ItemSecondaryIPSelected != "Not Applicable")
                            {
                                originalDeviceItem.CoreDateTimeList.DateTimeIPAddress = originalDeviceItem.ItemSecondaryIPSelected;
                            }
                        }
                        else
                        {
                            originalDeviceItem.CoreDateTimeList.DateTimeIPAddress = originalDeviceItem.ItemPrimaryIPSelected;
                        }

                        string outresponse = string.Empty;
                        var isaccessopen = AccessOpen(originalDeviceItem.CoreDateTimeList.DateTimeIPAddress, originalDeviceItem, out outresponse);
                        if (isaccessopen.Item2)
                        {
                            MessageBox.Show("Device is not available in the network.", "Error Code - EC15024", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }

                        if (outresponse == "404")
                        {
                            MessageBox.Show("The remote server returned an error: (404) URL Not Found.", "Error Code - EC15024", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }

                        if (isaccessopen.Item1 == false)
                        {
                            var result = Corelogon(originalDeviceItem.CoreDateTimeList.DateTimeIPAddress, Properties.Settings.Default.DevicePassword.ToString(), out outresponse);
                            if (outresponse == "404")
                            {
                                MessageBox.Show("Please enter correct Username/password in preferences to view and change core date and time.", "Error Code - EC15024", MessageBoxButton.OK, MessageBoxImage.Error);
                                return;
                            }

                            coreLogonToken = result.Item3;
                        }
                    }

                    GetCoreDateTime(originalDeviceItem, true, isQREMTrue, coreLogonToken);
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

        private Dictionary<string, string> GetQREMSystemDetails(string systemID)
        {
            Dictionary<string, string> systemDetails = new Dictionary<string, string>();

            try
            {
                string strResponse = string.Empty;
                var outval = HttpGetactual("https://" + Properties.Settings.Default.QREMreflectLink + "/api/v0/systems/" + systemID, string.Empty, DeviceDiscovery.QREM_Token, null, true, true, out strResponse);

                if (outval.Item1 && !string.IsNullOrEmpty(strResponse))
                {
                    dynamic array = new System.Web.Script.Serialization.JavaScriptSerializer().DeserializeObject(strResponse);

                    if (array != null && array.Count > 0)
                    {
                        Dictionary<string, object> coreDetails = array as Dictionary<string, object>;

                        foreach (var coreItems in coreDetails)
                        {
                            if (coreItems.Key == "revision" && coreItems.Value != null)
                            {
                                Dictionary<string, object> getrevisionDetails = coreItems.Value as Dictionary<string, object>;

                                if (getrevisionDetails != null)
                                {
                                    foreach (var revisionDetails in getrevisionDetails)
                                    {
                                        if (revisionDetails.Key == "design" && revisionDetails.Value != null)
                                        {
                                            Dictionary<string, object> getdesignDetails = revisionDetails.Value as Dictionary<string, object>;

                                            if (getdesignDetails != null && getdesignDetails.Count > 0)
                                            {
                                                foreach (var designDetails in getdesignDetails)
                                                {
                                                    if (designDetails.Key == "name" && designDetails.Value != null)
                                                    {
                                                        systemDetails.Add("DesignName", designDetails.Value.ToString() + ".qsys");
                                                    }
                                                    else if (designDetails.Key == "code" && designDetails.Value != null)
                                                    {
                                                        systemDetails.Add("DesignCode", designDetails.Value.ToString());
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            else if (coreItems.Key == "core" && coreItems.Value != null)
                            {
                                Dictionary<string, object> getcoreDetails = coreItems.Value as Dictionary<string, object>;

                                if (getcoreDetails != null)
                                {
                                    foreach (var coreDetail in getcoreDetails)
                                    {
                                        if (coreDetail.Key == "status" && coreDetail.Value != null)
                                        {
                                            Dictionary<string, object> designRunningDetails = coreDetail.Value as Dictionary<string, object>;

                                            if (designRunningDetails != null && designRunningDetails.Count > 0)
                                            {
                                                foreach (var designrunningDetails in designRunningDetails)
                                                {
                                                    if (designrunningDetails.Key == "name" && designrunningDetails.Value != null)
                                                    {
                                                        systemDetails.Add("CoreStatus", designrunningDetails.Value.ToString());
                                                    }
                                                }
                                            }
                                        }
                                        else if (coreDetail.Key == "redundancy" && coreDetail.Value != null)
                                        {
                                            Dictionary<string, object> designRunningDetails = coreDetail.Value as Dictionary<string, object>;

                                            if (designRunningDetails != null && designRunningDetails.Count > 0)
                                            {
                                                foreach (var designrunningDetails in designRunningDetails)
                                                {
                                                    if (designrunningDetails.Key == "state" && designrunningDetails.Value != null)
                                                    {
                                                        systemDetails.Add("RedundencyState", designrunningDetails.Value.ToString());
                                                    }
                                                }
                                            }
                                        }
                                        else if (coreDetail.Key == "firmware" && coreDetail.Value != null)
                                        {
                                            systemDetails.Add("FirmwareVersion", coreDetail.Value.ToString());
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                ExecutionMessageBox("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
            }

            return systemDetails;
        }
        
        private void GetFirmwareDesignNameAndIDmodeForReflect(DUT_DeviceItem originalDeviceItem)
        {
            try
            {
                ///Get Firmware and designname
                if (originalDeviceItem.QREMcoredetails != null && originalDeviceItem.QREMcoredetails.Count() > 2)
                {
                    string systemid = originalDeviceItem.QREMcoredetails[2];

                    Dictionary<string,string> designdetails = GetQREMSystemDetails(systemid);

                    if(designdetails.ContainsKey("FirmwareVersion"))
                        originalDeviceItem.ItemCurrentBuild = designdetails["FirmwareVersion"];
                    else
                        originalDeviceItem.ItemCurrentBuild = string.Empty;


                    if (designdetails.ContainsKey("DesignName"))
                        originalDeviceItem.ItemCurrentDesign = designdetails["DesignName"];
                    else
                        originalDeviceItem.ItemCurrentDesign = string.Empty;                    
                }

                /////ID mode details
                if (originalDeviceItem.QREMcoredetails.Count() > 1)
                {
                    string id_Mode = GetIDmodeDetailForLocalRemote(originalDeviceItem, string.Empty);

                    if (id_Mode == "Yes")
                        originalDeviceItem.blnIDColor = "LightGreen";
                    else if (id_Mode == "No")
                        originalDeviceItem.blnIDColor = "Red";
                    else
                        originalDeviceItem.blnIDColor = "Balck";

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
        
        private bool GetDateTimeAndTimeZone(DUT_DeviceItem originalDeviceItem, bool isSetClick, string coreLogonToken, bool isQREMTrue)
        {
            try
            {
                Tuple<bool, string, bool> dateAndTimeResponse = null;
                string strResponse = string.Empty;

                if (isQREMTrue)
                { 
                    if (originalDeviceItem.QREMcoredetails.Count()>1)
                    {
                        string id = originalDeviceItem.QREMcoredetails[1];
                        dateAndTimeResponse = HttpGetactual("https://" + Properties.Settings.Default.QREMreflectLink + "/api/v0/cores/" + id + "/config/time", string.Empty, DeviceDiscovery.QREM_Token, null, true, true, out strResponse);
                    }
                }
                else
                {
                    //////Get date time and NTP servers
                    dateAndTimeResponse = HttpGetactual("http://" + originalDeviceItem.CoreDateTimeList.DateTimeIPAddress + "/api/v0/cores/self/config/time?meta=permissions", originalDeviceItem.CoreDateTimeList.DateTimeIPAddress, coreLogonToken, originalDeviceItem, true, false, out strResponse);
                }

                if (dateAndTimeResponse != null && dateAndTimeResponse.Item1 && !string.IsNullOrEmpty(strResponse))
                {
                    dynamic array = new System.Web.Script.Serialization.JavaScriptSerializer().DeserializeObject(strResponse);
                    bool isDataAvailable = false;

                    if (array != null && array.Count > 0)
                    {
                        foreach (var arraydata in array)
                        {
                            if (arraydata.Key == "data" && arraydata.Value != null)
                            {
                                isDataAvailable = true;

                                Dictionary<string, object> arrayValue = arraydata.Value as Dictionary<string, object>;

                                if (arrayValue != null)
                                {
                                    foreach (var dataValue in arrayValue)
                                    {
                                        if (dataValue.Key == "dateTimeUTC")
                                        {
                                            var fullDateTimeWithUTC = dataValue.Value.ToString();

                                            if (fullDateTimeWithUTC != null && fullDateTimeWithUTC != string.Empty)
                                            {
                                                string[] datetime = fullDateTimeWithUTC.Split(' ');

                                                if (datetime.Count() > 1)
                                                {
                                                    string isSameAsPCUTCTime = datetime_check(datetime);

                                                    if (isSameAsPCUTCTime == "true")
                                                    {
                                                        originalDeviceItem.BtnDateColor = "LightGreen";
                                                    }
                                                    else if (isSameAsPCUTCTime == "false")
                                                    {
                                                        originalDeviceItem.BtnDateColor = "Red";
                                                    }

                                                    if (isSetClick)
                                                    {
                                                        string fullDateTime = datetime[0] + " " + datetime[1];
                                                        DateTime coreCurrentDate = DateTime.ParseExact(fullDateTime, "yyyy/MM/dd HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);

                                                        originalDeviceItem.CoreDateTimeList.CalendarDisplayDate = coreCurrentDate;
                                                        originalDeviceItem.CoreDateTimeList.CalendarSelectedDate = coreCurrentDate;

                                                        string time = coreCurrentDate.ToString("hh:mm tt", System.Globalization.DateTimeFormatInfo.InvariantInfo);
                                                        int hours = Convert.ToInt32(coreCurrentDate.ToString("hh", System.Globalization.DateTimeFormatInfo.InvariantInfo));
                                                        int minutes = Convert.ToInt32(coreCurrentDate.ToString("mm", System.Globalization.DateTimeFormatInfo.InvariantInfo));
                                                        int seconds = Convert.ToInt32(coreCurrentDate.ToString("ss", System.Globalization.DateTimeFormatInfo.InvariantInfo));

                                                        originalDeviceItem.CoreDateTimeList.CoreCurrentTime = time;

                                                        originalDeviceItem.CoreDateTimeList.ClockHourAngle = (hours * 30) + (minutes * 0.5);
                                                        originalDeviceItem.CoreDateTimeList.ClockMinuteAngle = minutes * 6;
                                                        originalDeviceItem.CoreDateTimeList.ClockSecondsAngle = seconds * 6;
                                                    }
                                                    else
                                                        break;
                                                }
                                            }
                                        }
                                        else if (dataValue.Key == "timezone" && isSetClick)
                                        {
                                            dynamic countryarray = new System.Web.Script.Serialization.JavaScriptSerializer().DeserializeObject(originalDeviceItem.CoreDateTimeList.TimeZoneJSON);

                                            if (countryarray != null)
                                            {
                                                object[] arr = countryarray as object[];

                                                if (arr != null && arr.Count() > 0)
                                                {
                                                    foreach (object countrylist in arr)
                                                    {
                                                        Dictionary<string, object> countryWithCitylist = countrylist as Dictionary<string, object>;
                                                        if (countryWithCitylist != null)
                                                        {
                                                            foreach (var countryWithCity in countryWithCitylist)
                                                            {
                                                                originalDeviceItem.CoreDateTimeList.TimeZoneItemsource.Add(new ComboBoxItem { DisplayText = countryWithCity.Key.ToString(), IsHeader = true, TZName = string.Empty });

                                                                object[] citylists = countryWithCity.Value as object[];

                                                                foreach (object city in citylists)
                                                                {
                                                                    originalDeviceItem.CoreDateTimeList.TimeZoneItemsource.Add(new ComboBoxItem { DisplayText = city.ToString(), IsHeader = false, TZName = countryWithCity.Key.ToString() + "/" + city.ToString() });
                                                                }
                                                            }
                                                        }
                                                    }

                                                    originalDeviceItem.CoreDateTimeList.TimeZoneSelectedItem = originalDeviceItem.CoreDateTimeList.TimeZoneItemsource.FirstOrDefault(x => x.TZName == dataValue.Value.ToString());
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    
                    if (!isDataAvailable)
                    {
                        originalDeviceItem.BtnDateColor = "Black";
                        return false;
                    }

                    return true;
                }
                else
                {
                    originalDeviceItem.BtnDateColor = "Black";
                    return false;
                }
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

        private bool GetNTPDetails(DUT_DeviceItem originalDeviceItem, string coreLogonToken, bool isQREMTrue)
        {
            try
            {
                string ntpResponse = string.Empty;
                Tuple<bool, string, bool> ntpResult = null;

                if (isQREMTrue)
                {
                    if (originalDeviceItem.QREMcoredetails.Count() > 1)
                    {
                        string id = originalDeviceItem.QREMcoredetails[1];
                        ntpResult = HttpGetactual("https://" + Properties.Settings.Default.QREMreflectLink + "/api/v0/cores/" + id + "/config/ntp", string.Empty, DeviceDiscovery.QREM_Token, null, true, true, out ntpResponse);
                    }
                    if (ntpResult == null || ntpResult.Item1 == false)
                        return false;
                }
                else
                {
                    ntpResult = HttpGetactual("http://" + originalDeviceItem.CoreDateTimeList.DateTimeIPAddress + "/api/v0/cores/self/config/ntp?meta=permissions", originalDeviceItem.CoreDateTimeList.DateTimeIPAddress, coreLogonToken, originalDeviceItem, true, false, out ntpResponse);
                    if (ntpResult == null || ntpResult.Item1 == false)
                        return false;
                }

                if (ntpResult != null && ntpResult.Item1 && !string.IsNullOrEmpty(ntpResponse))
                {
                    dynamic array = new System.Web.Script.Serialization.JavaScriptSerializer().DeserializeObject(ntpResponse);

                    if (array != null && array.Count > 0)
                    {
                        bool isDataAvailable = false;

                        foreach (var arrayitem in array)
                        {
                            if (arrayitem.Key == "data")
                            {
                                isDataAvailable = true;
                                Dictionary<string, object> dictData = arrayitem.Value as Dictionary<string, object>;

                                if (dictData != null)
                                {
                                    foreach (var dataValue in dictData)
                                    {
                                        if (dataValue.Key == "enabled")
                                        {
                                            bool isEnabled = false;

                                            if (dataValue.Value.ToString() == "yes")
                                                isEnabled = true;

                                            originalDeviceItem.CoreDateTimeList.NTPChecked = isEnabled;
                                        }
                                        else if (dataValue.Key == "servers")
                                        {
                                            object[] poollist = dataValue.Value as object[];
                                            originalDeviceItem.CoreDateTimeList.PoolListViewItems = new ObservableCollection<string>(Array.ConvertAll(poollist, x => x.ToString()));
                                            if (originalDeviceItem.CoreDateTimeList.PoolListViewItems != null && originalDeviceItem.CoreDateTimeList.PoolListViewItems.Count > 0)
                                                originalDeviceItem.CoreDateTimeList.PoolListSelectedIndex = 0;
                                        }
                                    }
                                }
                            }
                        }

                        if (isDataAvailable)
                            return true;
                    }
                }

                return false;
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

        private bool GetCoreDateTime(DUT_DeviceItem originalDeviceItem, bool isSetClick, bool isQREMTrue, string coreLogonToken)
        {
            try
            {
                if (originalDeviceItem.ItemDeviceModel.ToUpper().Contains("CORE"))
                {
                    bool isDatetimeSuccess = GetDateTimeAndTimeZone(originalDeviceItem, isSetClick, coreLogonToken, isQREMTrue);

                    if (!isDatetimeSuccess)
                        return false;

                    if (isSetClick)
                    {
                        bool isNTPsuccess = GetNTPDetails(originalDeviceItem, coreLogonToken, isQREMTrue);

                        if (!isNTPsuccess)
                            return false;

                        Core_DateAndTime dateAndTime = new Core_DateAndTime(originalDeviceItem);
                        dateAndTime.Owner = this;
                        dateAndTime.ShowDialog();
                        bool isSetSuccess = GetDateTimeAndTimeZone(originalDeviceItem, false, coreLogonToken, isQREMTrue);
                        if (!isSetSuccess)
                            return false;
                        else
                            return true;
                    }
                    else
                        return true;
                }
                else
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
        
        private string datetime_check(string[] datetime)
        {
          string initialvalue = string.Empty;

            try {

                string UTCtime = (datetime[2].Replace("UTC", string.Empty)).Trim();
                //TimeSpan ts = new TimeSpan(values[0], values[1], 0);

                int hoursdut = Convert.ToInt16(UTCtime.ElementAt(0).ToString() + UTCtime.ElementAt(1).ToString() + UTCtime.ElementAt(2).ToString());
                int minsdut = Convert.ToInt16(UTCtime.ElementAt(0).ToString() + UTCtime.ElementAt(3).ToString() + UTCtime.ElementAt(4).ToString());

                DateTime PCUTCtime = DateTime.UtcNow;
                PCUTCtime = PCUTCtime.AddHours((hoursdut));
                PCUTCtime = PCUTCtime.AddMinutes((minsdut));



                DateTime DeviceUTCtime = Convert.ToDateTime(datetime[0] + " " + datetime[1]);
                //DeviceUTCtime = DeviceUTCtime.ToUniversalTime().AddHours((hoursdut));
                //DeviceUTCtime = DeviceUTCtime.AddMinutes(minsdut);



                //Int32 valueoftime= DateTime.Compare(PCUTCtime, DeviceUTCtime);
                double diff = Math.Abs(PCUTCtime.Subtract(DeviceUTCtime).TotalMinutes);
                if (diff <= 5)
                {
                    initialvalue = "true";
                }
                else
                {
                    initialvalue = "false";

                }
                return initialvalue;
            }
            catch
            {
                return initialvalue;

            }
        }

        private void TextBlock_IsMouseDirectlyOverChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            try
            {
                bool isMouseOver = (bool)e.NewValue;
                if (!isMouseOver)
                    return;
                TextBlock textBlock = (TextBlock)sender;
                string NetPairingHighlighted = textBlock.DataContext.ToString();
                bool result = NetPairingHighlighted.Equals(DeviceItemMouseOver, StringComparison.InvariantCultureIgnoreCase);
                if (AllowToolTip && !result && NetPairingHighlighted != "Not Applicable")
                {
                    textBlock.ToolTip = "Connect to designer action will fail, if core dynamically paired";
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

        private void cmb_netPairing_MouseMove(object sender, MouseEventArgs e)
        {
            try
            {
                DUT_DeviceItem originalDeviceItem = null;

                ComboBox selectedComboBox = sender as ComboBox;
                if (selectedComboBox != null && String.Equals(selectedComboBox.DataContext.GetType().ToString(), "QSC_Test_Automation.DUT_DeviceItem"))
                    originalDeviceItem = (DUT_DeviceItem)selectedComboBox.DataContext;

                if (originalDeviceItem == null)
                    return;

                DeviceItemMouseOver = originalDeviceItem.ItemDeviceName;

                if (originalDeviceItem.DesignerActionPresent)
                {
                    AllowToolTip = true;
                }
                else
                {
                    AllowToolTip = false;
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

        private void cmb_netPairing_DropDownOpened(object sender, EventArgs e)
        {
            bool continued = false;

            try
            {
                DUT_DeviceItem originalDeviceItem = null;

                ComboBox selectedComboBox = sender as ComboBox;

                List<DUT_DeviceItem> list_originalDeviceItem = new List<DUT_DeviceItem>();
                list_originalDeviceItem = new List<DUT_DeviceItem>((ObservableCollection<DUT_DeviceItem>)this.dataGrid_ConfigFile.DataContext);

                if (selectedComboBox != null && String.Equals(selectedComboBox.DataContext.GetType().ToString(), "QSC_Test_Automation.DUT_DeviceItem"))
                    originalDeviceItem = (DUT_DeviceItem)selectedComboBox.DataContext;

                if (originalDeviceItem == null)
                    return;

                foreach (var getlist in originalDeviceItem.ItemNetPairingList_duplicate)
                {
                    if (!continued)
                    {
                        if ((getlist.Key != "Not Applicable") && (getlist.Key != string.Empty))
                        {
                            if (originalDeviceItem.ItemPrimaryorBackup == "backup")
                            {
                                string get_Bname = originalDeviceItem.ItemDeviceName;
                                string get_Pname = originalDeviceItem.Itemlinked;
                                string get_type = originalDeviceItem.ItemDeviceType;
                                string get_model = originalDeviceItem.ItemDeviceModel;
                                foreach (DUT_DeviceItem eachitem in list_originalDeviceItem)
                                {
                                    if (eachitem.ItemDeviceName.Equals(get_Pname, StringComparison.CurrentCultureIgnoreCase) && (eachitem.ItemDeviceModel == get_model) && (eachitem.ItemDeviceType == get_type) && (get_Bname.Trim().ToUpper() != get_Pname.Trim().ToUpper()))
                                    {
                                        string whatselected = eachitem.ItemNetPairingSelected;

                                        foreach (var check in originalDeviceItem.ItemNetPairingList_duplicate)
                                        {
                                            if (!originalDeviceItem.ItemNetPairingListForXAML.Contains(check.Key))
                                                originalDeviceItem.ItemNetPairingListForXAML.Add(check.Key);

                                            if (!originalDeviceItem.ItemNetPairingList.Keys.ToList().Contains(check.Key))
                                                originalDeviceItem.ItemNetPairingList.Add(check.Key, check.Value);
                                        }

                                        if ((whatselected != null) && (whatselected != string.Empty) && (whatselected != "Not Applicable"))
                                        {
                                            if (originalDeviceItem.ItemNetPairingList.Keys.ToList().Contains(whatselected))
                                                originalDeviceItem.ItemNetPairingList.Remove(whatselected);

                                            if (originalDeviceItem.ItemNetPairingListForXAML.Contains(whatselected))
                                                originalDeviceItem.ItemNetPairingListForXAML.Remove(whatselected);

                                            continued = true;
                                            break;
                                        }
                                    }
                                }
                            }

                            if (originalDeviceItem.ItemPrimaryorBackup == "primary")
                            {
                                string get_Pname = originalDeviceItem.ItemDeviceName;
                                string get_Bname = originalDeviceItem.Itemlinked;
                                string get_type = originalDeviceItem.ItemDeviceType;
                                string get_model = originalDeviceItem.ItemDeviceModel;
                                foreach (DUT_DeviceItem eachitem in list_originalDeviceItem)
                                {
                                    if (eachitem.ItemDeviceName.Equals(get_Bname, StringComparison.CurrentCultureIgnoreCase) && (eachitem.ItemDeviceModel == get_model) && (eachitem.ItemDeviceType == get_type) && (get_Bname.Trim().ToUpper() != get_Pname.Trim().ToUpper()))
                                    {
                                        string whatselected = eachitem.ItemNetPairingSelected;
                                        //originalDeviceItem.ItemNetPairingList.Clear();
                                        foreach (var check in originalDeviceItem.ItemNetPairingList_duplicate)
                                        {
                                            if (!originalDeviceItem.ItemNetPairingListForXAML.Contains(check.Key))
                                                originalDeviceItem.ItemNetPairingListForXAML.Add(check.Key);

                                            if(!originalDeviceItem.ItemNetPairingList.Keys.ToList().Contains(check.Key))
                                                originalDeviceItem.ItemNetPairingList.Add(check.Key, check.Value);
                                        }

                                        if ((whatselected != null) && (whatselected != string.Empty) && (whatselected != "Not Applicable"))
                                        {
                                            if (originalDeviceItem.ItemNetPairingList.Keys.ToList().Contains(whatselected))
                                                originalDeviceItem.ItemNetPairingList.Remove(whatselected);

                                            if (originalDeviceItem.ItemNetPairingListForXAML.Contains(whatselected))
                                                originalDeviceItem.ItemNetPairingListForXAML.Remove(whatselected);

                                            continued = true;
                                            break;
                                        }
                                    }
                                }
                            }
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
                //MessageBox.Show("Exception\n " + ex.Message, "QAT Error Code - EC03021", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}