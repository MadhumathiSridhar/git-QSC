using System;
using System.Collections.Generic;
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
using System.Threading;
using System.Windows.Threading;
using System.Windows.Media.Animation;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using System.Net;
using System.Data;
using System.Globalization;
using System.Web.Script.Serialization;
//using System.ComponentModel;

namespace QSC_Test_Automation
{
    /// <summary>
    /// Interaction logic for Core_DateAndTime.xaml
    /// </summary>
    public partial class Core_DateAndTime : Window
    {
        ExecutionProcess execProcess = new ExecutionProcess();
        string versionCore = string.Empty;
        public Core_DateAndTime(DUT_DeviceItem DutDeviceItem)
        {
            InitializeComponent();

            try
            {
                if (DutDeviceItem != null)
                {
                    this.DataContext = DutDeviceItem.CoreDateTimeList;
                    versionCore = DutDeviceItem.ItemCurrentBuild;
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

        public Core_DateAndTime()
        {

        }

        private void Btn_PoolPlus_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Core_DateTime coreDateTimeItem = null;

                Button selectedButton = sender as Button;
                if (selectedButton != null && selectedButton.DataContext != null && String.Equals(selectedButton.DataContext.GetType().ToString(), "QSC_Test_Automation.Core_DateTime"))
                    coreDateTimeItem = (Core_DateTime)selectedButton.DataContext;

                if (coreDateTimeItem != null)
                {
                    coreDateTimeItem.SaveButtonIsEnabled = true;
                    PoolPlus poolWindow = new PoolPlus();
                    poolWindow.Owner = this;
                    poolWindow.ShowDialog();

                    if (poolWindow.poolPlusName != string.Empty)
                    {
                        coreDateTimeItem.PoolListViewItems.Add(poolWindow.poolPlusName);

                        if (coreDateTimeItem.PoolListViewItems.Count == 1)
                            coreDateTimeItem.PoolListSelectedIndex = 0;
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
        }

        private void Btn_PoolMinus_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Core_DateTime coreDateTimeItem = null;

                Button selectedButton = sender as Button;
                if (selectedButton != null && selectedButton.DataContext != null && String.Equals(selectedButton.DataContext.GetType().ToString(), "QSC_Test_Automation.Core_DateTime"))
                    coreDateTimeItem = (Core_DateTime)selectedButton.DataContext;

                if (coreDateTimeItem != null)
                {
                    coreDateTimeItem.SaveButtonIsEnabled = true;

                    if (coreDateTimeItem.PoolListSelectedItem != null)
                    {
                        int selectedIndex = coreDateTimeItem.PoolListSelectedIndex;
                        coreDateTimeItem.PoolListViewItems.Remove(coreDateTimeItem.PoolListSelectedItem);
                        int totalItemCnt = coreDateTimeItem.PoolListViewItems.Count;
                        if (totalItemCnt != 0 && totalItemCnt == selectedIndex)
                        {
                            coreDateTimeItem.PoolListSelectedIndex = totalItemCnt - 1;
                        }

                        if (totalItemCnt != 0 && totalItemCnt > selectedIndex)
                        {
                            coreDateTimeItem.PoolListSelectedIndex = selectedIndex;
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
        }

        private void btn_DateTimeSave_Click(object sender, RoutedEventArgs e)
        {
            string coretoken = string.Empty;
            try
            {
                Core_DateTime coreDateTimeItem = null;

                Button selectedButton = sender as Button;
                if (selectedButton != null && selectedButton.DataContext != null && String.Equals(selectedButton.DataContext.GetType().ToString(), "QSC_Test_Automation.Core_DateTime"))
                    coreDateTimeItem = (Core_DateTime)selectedButton.DataContext;

                if (coreDateTimeItem != null)
                {
					/////ParentDUTdeviceItem added to check QREM / QRCM device 
                    DUT_DeviceItem parentDUTdeviceItem = null;

                    if (coreDateTimeItem.ParentDUTDeviceItem != null)
                    {
                        parentDUTdeviceItem = coreDateTimeItem.ParentDUTDeviceItem;
                    }

                    bool isQREMTrue = false;

                    if (parentDUTdeviceItem != null && parentDUTdeviceItem.ItemNetPairingList != null && parentDUTdeviceItem.ItemNetPairingList.Keys.ToList().Contains(parentDUTdeviceItem.ItemNetPairingSelected) && (parentDUTdeviceItem.ItemNetPairingList[parentDUTdeviceItem.ItemNetPairingSelected] != "Localdevice"))
                        isQREMTrue = true;
                        
                    if(!isQREMTrue)
                    {
                        string outresponse = string.Empty;
                        ///////logon core to get tokenkey  
                        var isaccessopen = AccessOpen(coreDateTimeItem.DateTimeIPAddress, out outresponse);
                        if (outresponse == "404")
                        {
                            MessageBox.Show("The remote server returned an error: (404) URL Not Found.", "Error Code - EC15024", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }

                        if (isaccessopen.Item1 == false)
                        {
                            bool result = Corelogon(coreDateTimeItem.DateTimeIPAddress, Properties.Settings.Default.DevicePassword.ToString(), out outresponse);
                            if (outresponse == "404")
                            {
                                MessageBox.Show("Please enter correct Username/password in preferences to change core date and time.", "Error Code - EC15024", MessageBoxButton.OK, MessageBoxImage.Error);
                                return;
                            }
                            if (result == true && outresponse != string.Empty)
                                coretoken = outresponse;
                        }
                    }

					//////Set Date and Time To Designer
                    bool issaved = SetDateAndTime(coreDateTimeItem, isQREMTrue, coretoken);
                    if (!issaved)
                        return;

					///////Update NTP
                    bool issavedconfig = UpdateNTP(coreDateTimeItem, isQREMTrue, coretoken);
                    if (issavedconfig)
                    {
                        MessageBox.Show("Date / Time set!", "QAT message", MessageBoxButton.OK, MessageBoxImage.Information);
                        this.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                if (ex.Message == "Unable to connect to the remote server")
                {
                    MessageBox.Show("Device not available in network", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                else if (ex.Message.Contains("The remote server returned an error: (401) Unauthorized"))
                {
                    MessageBox.Show("Device password does not match preference password", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                else
                {
                    DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                }
            }
        }

        private bool UpdateNTP(Core_DateTime coreDateTimeItem, bool isQREMtrue, string coretokenorQREMtoken)
        {
            try
            {
                string response = string.Empty;
                bool issavedconfig = false;
                StringBuilder sbNetworkConfig = new StringBuilder();
                string enableNTP = string.Empty;

                if (coreDateTimeItem.NTPChecked == true)
                    enableNTP = "yes";
                else
                    enableNTP = "no";

                sbNetworkConfig.Append(@"{" + "\"enabled\":\"" + enableNTP + "\",\"servers\":[");
                string poolnamelist = string.Empty;
                foreach (string poolName in coreDateTimeItem.PoolListViewItems)
                {
                    poolnamelist += "\"" + poolName + "\",";
                }

                if (poolnamelist.EndsWith(","))
                    poolnamelist = poolnamelist.Remove(poolnamelist.Count() - 1);

                sbNetworkConfig.Append(poolnamelist + "]}");

				////If QRCM means old httpput otherwise new httpput based on uniqueid
                if (!isQREMtrue)
                {                    
                    issavedconfig = HttpPut_and_Post_json("http://" + coreDateTimeItem.DateTimeIPAddress + "/api/v0/cores/self/config/ntp", sbNetworkConfig.ToString(), "PUT", true, coretokenorQREMtoken, out response);
                }
                else
                {
                    if (coreDateTimeItem.ParentDUTDeviceItem != null && coreDateTimeItem.ParentDUTDeviceItem.QREMcoredetails != null && coreDateTimeItem.ParentDUTDeviceItem.QREMcoredetails.Count() > 1)
                    {
                        string coreUniqueID = coreDateTimeItem.ParentDUTDeviceItem.QREMcoredetails[1];
                        issavedconfig = HttpPut_and_Post_json("https://" + Properties.Settings.Default.QREMreflectLink + "/api/v0/cores/" + coreUniqueID + "/config/ntp", sbNetworkConfig.ToString(), "PUT", true, DeviceDiscovery.QREM_Token, out response);
                    }
                }

                return issavedconfig;
            }
            catch(Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                return false;
            }
        }

        private bool SetDateAndTime(Core_DateTime coreDateTimeValues, bool isQREMTrue, string coretoken)
        {
            try
            {
                if (coreDateTimeValues != null)
                {
                    string selectedZone = string.Empty;
                    if (coreDateTimeValues.TimeZoneSelectedItem != null)
                    {
                        selectedZone = coreDateTimeValues.TimeZoneSelectedItem.TZName;
                    }

                    DateTime guiDateTime = coreDateTimeValues.CalendarSelectedDate.Value;
                    DateTime dt;

                    string selectedtime = coreDateTimeValues.CoreCurrentTime;

                    if ((DateTime.TryParseExact(selectedtime, "h:mm tt", CultureInfo.InvariantCulture, DateTimeStyles.None, out dt)) || (DateTime.TryParseExact(selectedtime, "h:m tt", CultureInfo.InvariantCulture, DateTimeStyles.None, out dt)) ||

                        (DateTime.TryParseExact(selectedtime, "hh:m tt", CultureInfo.InvariantCulture, DateTimeStyles.None, out dt)))
                    {
                        string[] Time_split = selectedtime.Split(' ');
                        Time_split[0] = timestringformat(Time_split[0], null, null, Time_Selection.CaretIndex);
                        selectedtime = Time_split[0] + " " + Time_split[1];
                    }
                    else if ((!DateTime.TryParseExact(selectedtime, "hh:mm tt", CultureInfo.InvariantCulture, DateTimeStyles.None, out dt)))
                    {
                        MessageBox.Show("Invalid Time\n", "Warning", MessageBoxButton.OK, MessageBoxImage.Error);
                        return false;
                    }
                    else
                    {
                        MessageBox.Show("Invalid Time\n", "Warning", MessageBoxButton.OK, MessageBoxImage.Error);
                        return false;
                    }

                    string datetime = guiDateTime.ToString("yyyy-MM-dd") + " " + selectedtime;
                    DateTime myDate = DateTime.ParseExact(datetime, "yyyy-MM-dd hh:mm tt", System.Globalization.CultureInfo.InvariantCulture);

                    string datetimes = myDate.ToString("yyyy/MM/dd HH:mm:ss");

					////If QRCM means old httpput otherwise new httpput based on core uniqueid
                    string response = string.Empty;
                    string sbNetworkConfig = string.Empty;

                    if (!isQREMTrue)
                    {
                        if (selectedZone != string.Empty && selectedZone != null)
                            sbNetworkConfig = "{\"date\":\"" + datetimes + "\",\"timezone\":\"" + selectedZone + "\"}";
                        else
                            sbNetworkConfig = "{\"date\":\"" + datetimes + "\"}";

                        bool isSaved = HttpPut_and_Post_json("http://" + coreDateTimeValues.DateTimeIPAddress + "/api/v0/cores/self/config/time", sbNetworkConfig.ToString(), "PUT", true, coretoken, out response);
                        return isSaved;
                    }
                    else
                    {
                        if (coreDateTimeValues.ParentDUTDeviceItem != null && coreDateTimeValues.ParentDUTDeviceItem.QREMcoredetails != null && coreDateTimeValues.ParentDUTDeviceItem.QREMcoredetails.Count() > 1)
                        {
                            if (!string.IsNullOrEmpty(selectedZone))
                                sbNetworkConfig = "{\"dateTimeUTC\": \"" + datetimes + "\",\"timezone\": \"" + selectedZone + "\"}";
                            else
                                sbNetworkConfig = "{\"dateTimeUTC\": \"" + datetimes + "\"}";

                            string coreUniqueID = coreDateTimeValues.ParentDUTDeviceItem.QREMcoredetails[1];

                            bool datetimeset = HttpPut_and_Post_json("https://" + Properties.Settings.Default.QREMreflectLink + "/api/v0/cores/" + coreUniqueID + "/config/time", sbNetworkConfig, "PUT", true, DeviceDiscovery.QREM_Token, out response);
                            return datetimeset;
                        }
                        else
                            return false;
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


        public bool HttpPut_and_Post_json(string strURI, string strParameters, string method, bool isnewVersion, string coretoken, out string strResponse)
        {          
            strResponse = string.Empty;
            bool success = false;
            System.Net.HttpWebRequest req = (HttpWebRequest)System.Net.WebRequest.Create(strURI);
            try
            {  
                if (isnewVersion)
                {
                    req.ContentType = "application/json";
                    if (coretoken != string.Empty)
                        req.Headers["Authorization"] = "Bearer " + coretoken;
                }
                else
                {
                    req.ContentType = "application/x-www-form-urlencoded";               
                    SetBasicAuthHeader(ref req, "admin", Properties.Settings.Default.DevicePassword);
                }
                req.Method = method;
                req.Timeout = 30000;
                req.ReadWriteTimeout = 30000;

                Byte[] retBytes = System.Text.Encoding.ASCII.GetBytes(strParameters);
                req.ContentLength = retBytes.Length;             

                using (System.IO.Stream outStream = req.GetRequestStream())
                {
                    outStream.Write(retBytes, 0, retBytes.Length);
                }

                using (HttpWebResponse resp = (HttpWebResponse)req.GetResponse())
                {
                    success = HttpStatusCodeCheck(resp, method, out strResponse);
                }
                return success;
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                strResponse = "";
                if (ex.Message == "Unable to connect to the remote server")
                {
                    MessageBox.Show("Device not available in network", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                else if (ex.Message.Contains("The remote server returned an error: (401) Unauthorized"))
                {
                    strResponse = "401";                                    
                }
                else if (ex.Message.Contains("The remote server returned an error: (404) Not Found"))
                {
                    strResponse = "404";
                }             
                req.Abort();
                return false;
            }
        }

        public Tuple<bool, bool> AccessOpen(string ipaddress, out string strResponse)
        {
            bool state = false;
            strResponse = string.Empty;
            Tuple<bool, bool> value = new Tuple<bool, bool>(false, false);
            try
            {
                value = HttpGetactual("http://" + ipaddress + "/api/v0/cores/self/access_mode", ipaddress, out strResponse);

                if (strResponse == "401" || strResponse == "404")
                {
                    state = false;
                }
                else if ((value.Item1) && (strResponse != string.Empty))
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
                return new Tuple<bool, bool>(state, value.Item2);
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                return new Tuple<bool, bool>(state, value.Item2);
            }
        }
        
        public Tuple<bool, bool> HttpGetactual(string strURI, string deviceIP, out string strResponse)
        {
            bool msg = false;
            strResponse = string.Empty;
            bool success = false;
            System.Net.HttpWebRequest req = (HttpWebRequest)System.Net.WebRequest.Create(strURI);
            try
            {
                req.ContentType = "application/json";
                req.Method = "GET";
                req.Timeout = 15000;
                req.ReadWriteTimeout = 15000;
             
                using (HttpWebResponse resp = (HttpWebResponse)req.GetResponse())
                {
                    success = HttpStatusCodeCheck(resp, "GET", out strResponse);
                    return  new Tuple<bool, bool> (success, msg);
                }
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                ExecutionMessageBox("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif

                if (ex.Message.Contains("The remote server returned an error: (401) Unauthorized"))
                {
                    strResponse = "401";
                }
                else if (ex.Message.Contains("The remote server returned an error: (404) Not Found"))
                {
                    strResponse = "404";
                }
                else if (ex.Message == "Unable to connect to the remote server" || ex.Message == "The operation has timed out")
                {
                    msg = true;
                }
                req.Abort();
                return  new Tuple<bool, bool>(success, msg);
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


        public bool Corelogon(string ipaddress, string userpassword, out string strResponse)
        {
            strResponse = string.Empty;
            //string outToken = string.Empty;
            bool success = false;
            string strParameters = "{\"username\":\"" + Properties.Settings.Default.DeviceUsername.ToString() + "\",\"password\":\"" + userpassword + "\"}";
            try
            {
                success = HttpPut_and_Post_json("http://" + ipaddress + "/api/v0/logon", strParameters, "POST", true, string.Empty, out strResponse);

                //get Token value
                if (success)
                {
                    var obj = new JavaScriptSerializer { MaxJsonLength = Int32.MaxValue }.Deserialize<dynamic>(strResponse);
                    if (obj.Count > 0)
                    {
                        foreach (var response in obj)
                        {
                            strResponse = response.Value;
                        }
                    }
                }

                return success;
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                return success;
            }
        }



        private void SetBasicAuthHeader(ref HttpWebRequest req, string username, string password)
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
                // Output to debug, but then do nothing.  ** LOG THIS LATER **
                //Debug.WriteLine("Web_RW.SetBasicAuthHeader: " + ex.Message);
            }

            req.Headers["Authorization"] = "Basic " + strAuth;
        }



        private void UpDown_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Core_DateTime coreDateTimeItem = null;

                Button selectedButton = sender as Button;
                if (selectedButton != null && selectedButton.DataContext != null && String.Equals(selectedButton.DataContext.GetType().ToString(), "QSC_Test_Automation.Core_DateTime"))
                    coreDateTimeItem = (Core_DateTime)selectedButton.DataContext;

                if (coreDateTimeItem != null)
                {
                    TimeTextboxUpdate(sender, coreDateTimeItem.CoreCurrentTime, null, coreDateTimeItem);
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

        private void TimeTextboxUpdate(object sender, string CURTIME, KeyEventArgs e, Core_DateTime coreDateTimeItems)
        {
            try
            {
                string Sender_Type = sender.GetType().ToString();
                string TIME = string.Empty;
                var cursor_position = Time_Selection.SelectionStart;
                FrameworkElement button = null;

                if (Sender_Type == "System.Windows.Controls.Button")
                {
                    button = sender as Button;
                }
                else if (Sender_Type == "System.Windows.Controls.TextBox")
                {
                    button = sender as TextBox;
                }
                string[] Time_split = CURTIME.Split(' ');

                if (Time_split[0].Length != 5)
                {
                    Time_split[0] = timestringformat(Time_split[0], null, null, Time_Selection.CaretIndex);
                }

                TimeSpan Change_Hours = TimeSpan.FromHours(1);
                TimeSpan Change_minutes = TimeSpan.FromMinutes(1);
                DateTime dt;

                if (DateTime.TryParseExact(Time_split[0], "hh:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out dt))
                {
                    if ((button.Name == "Up" || (button.Name == "Time_Selection" && e.Key == Key.Up)) && cursor_position <= 2)
                    {
                        TimeSpan Update_time = TimeSpan.Parse(Time_split[0]);
                        if (Update_time.Hours < 12)
                        {
                            Update_time = Update_time.Add(Change_Hours);
                        }
                        else
                        {
                            Update_time = Update_time.Subtract(TimeSpan.FromHours(11));
                        }
                        CURTIME = Update_time.ToString() + " " + Time_split[1];
                        CURTIME = CURTIME.Remove(5, 3);
                    }
                    else if ((button.Name == "Down" || (button.Name == "Time_Selection" && e.Key == Key.Down)) && cursor_position <= 2)
                    {
                        TimeSpan Update_time = TimeSpan.Parse(Time_split[0]);
                        if (Update_time.Hours < 2)
                        {
                            Update_time = Update_time.Add(TimeSpan.FromHours(11));
                        }
                        else
                        {
                            Update_time = Update_time.Subtract(Change_Hours);
                        }
                        CURTIME = Update_time.ToString() + " " + Time_split[1];
                        CURTIME = CURTIME.Remove(5, 3);
                    }
                    else if ((button.Name == "Up" || (button.Name == "Time_Selection" && e.Key == Key.Up)) && cursor_position > 2 && cursor_position <= 5)
                    {
                        TimeSpan Update_time = TimeSpan.Parse(Time_split[0]);
                        if (Update_time.Minutes == 59 && Update_time.Hours == 12)
                        {
                            Update_time = TimeSpan.Parse("01:00");
                        }
                        else
                        {
                            Update_time = Update_time.Add(Change_minutes);
                        }
                        CURTIME = Update_time.ToString() + " " + Time_split[1];
                        CURTIME = CURTIME.Remove(5, 3);
                    }
                    else if ((button.Name == "Down" || (button.Name == "Time_Selection" && e.Key == Key.Down)) && cursor_position > 2 && cursor_position <= 5)
                    {
                        TimeSpan Update_time = TimeSpan.Parse(Time_split[0]);
                        if (Update_time.Minutes == 0 && Update_time.Hours == 1)
                        {
                            Update_time = TimeSpan.Parse("12:59");
                        }
                        else
                        {
                            Update_time = Update_time.Subtract(Change_minutes);
                        }
                        CURTIME = Update_time.ToString() + " " + Time_split[1];
                        CURTIME = CURTIME.Remove(5, 3);
                    }
                    else if ((button.Name == "Down" || button.Name == "Up" || button.Name == "Time_Selection") && cursor_position > 5)
                    {
                        
                        if (e==null || e.Key==Key.Up || e.Key==Key.Down )
                        {
                            if (Time_split[1].ToString() == "AM")
                                CURTIME = CURTIME.Replace('A', 'P');
                            if (Time_split[1].ToString() == "PM")
                                CURTIME = CURTIME.Replace('P', 'A');
                        }
                        else
                        {
                            if(e.Key==Key.A)
                            {
                                CURTIME = CURTIME.Replace('P', 'A');
                            }
                            if(e.Key==Key.P)
                            {
                                CURTIME = CURTIME.Replace('A', 'P');
                            }
                        }
                    }
                    coreDateTimeItems.CoreCurrentTime = CURTIME;
                    Time_Selection.CaretIndex = cursor_position;
                }
                else
                {
                    MessageBox.Show("Invalid Time\n", "Warning", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
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

        private string timestringformat(string Time_split, string inputtext, string daynight, int index)
        {
            try
            {
                string[] c = Time_split.Split(':');
                if (c[0].Length == 1)
                {
                    c[0] = string.Format("{0:00}", Convert.ToInt16(c[0]));
                    //c[0] = c[0].Remove(index, 1);
                    //c[0] = c[0].Insert(index, inputtext);
                }
                if (c[0].Length == 0)
                {
                    c[0] = string.Format("{0:00}", Convert.ToInt16(inputtext));
                }
                if (c[1].Length == 1)
                {
                    c[1] = string.Format("{0:00}", Convert.ToInt16(c[1]));
                    //c[1] = c[1].Remove(index-3, 1);
                    //c[1] = c[1 ].Insert(index-3, inputtext);
                }
                if (c[1].Length == 0)
                {
                    c[1] = string.Format("{0:00}", Convert.ToInt16(inputtext));
                    //c[0] = c[0].Remove(index, 1);
                    //c[0] = c[0].Insert(index, inputtext);
                }

                return (c[0] + ":" + c[1]);
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                return string.Empty;
            }
        }

        private void Time_Selection_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                Core_DateTime coreDateTimeItem = null;
                FrameworkElement sourceElement = sender as FrameworkElement;
                if (sourceElement == null)
                    return;

                if (sourceElement != null && sourceElement.DataContext != null && String.Equals(sourceElement.DataContext.GetType().ToString(), "QSC_Test_Automation.Core_DateTime"))
                    coreDateTimeItem = (Core_DateTime)sourceElement.DataContext;

                if (coreDateTimeItem != null)
                {
                    int colonindex = coreDateTimeItem.CoreCurrentTime.IndexOf(":");

                    if (e.Key == Key.Up)
                    {
                        TimeTextboxUpdate(sender, coreDateTimeItem.CoreCurrentTime, e, coreDateTimeItem);
                    }
                    if (e.Key == Key.Down)
                    {
                        TimeTextboxUpdate(sender, coreDateTimeItem.CoreCurrentTime, e, coreDateTimeItem);
                    }
                    if (e.Key == Key.A || e.Key == Key.P && Time_Selection.CaretIndex > 5)
                    {
                        TimeTextboxUpdate(sender, coreDateTimeItem.CoreCurrentTime, e, coreDateTimeItem);
                    }

                    if (e.Key == Key.Back)
                    {
                        if (Time_Selection.SelectedText != string.Empty)
                        {
                            if (Time_Selection.SelectedText.Contains(":") || Time_Selection.SelectedText.Contains("A") || Time_Selection.SelectedText.Contains("M") || Time_Selection.SelectedText.Contains("P") || Time_Selection.SelectedText.Contains(" "))
                            {
                                e.Handled = true;
                            }
                        }
                        else
                        {
                            string Backchar = Time_Selection.Text[Time_Selection.CaretIndex - 1].ToString();

                            if (Backchar == ":" || Backchar == "P" || Backchar == "A" || Backchar == "M" || Backchar == " ")
                            {
                                e.Handled = true;
                                return;
                            }

                            e.Handled = false;
                            return;
                        }


                       

                    }

                    if (e.Key == Key.Delete)
                    {

                        if (Time_Selection.SelectedText != string.Empty)
                        {
                            if (Time_Selection.SelectedText.Contains(":") || Time_Selection.SelectedText.Contains("A") || Time_Selection.SelectedText.Contains("M") || Time_Selection.SelectedText.Contains("P") || Time_Selection.SelectedText.Contains(" "))
                            {
                                e.Handled = true;
                            }
                        }
                        else
                        {
                            string Delchar = Time_Selection.Text[Time_Selection.CaretIndex].ToString();

                            if (Delchar == ":" || Delchar == "P" || Delchar == "A" || Delchar == "M" || Delchar == " ")
                            {
                                e.Handled = true;
                                return;
                            }

                            e.Handled = false;
                        }
                        

                       
                    }



                    if (e.Key == Key.Space || e.Key==Key.Insert)
                    {
                        e.Handled = true;
                        return;
                    }





                    if (e.Key == Key.Right && Time_Selection.CaretIndex == Time_Selection.Text.IndexOf(":"))
                    {

                        Time_Selection.SelectionLength = 0;
                        string[] hoursplit = Time_Selection.Text.Split(':');
                        if (hoursplit[0].Length == 0)
                        {
                            hoursplit[0] = "01";
                            Time_Selection.Text = hoursplit[0] + ":" + hoursplit[1];
                        }
                        if (hoursplit[1].Length > 0)
                        {
                            string[] k = hoursplit[1].Split(' ');
                            if (k[0].Length == 0)
                            {
                                k[0] = "00";
                                Time_Selection.Text = hoursplit[0] + ":" + k[0] + " " + k[1];
                            }

                        }
                    }

                    if (e.Key == Key.Left && Time_Selection.CaretIndex - 1 == Time_Selection.Text.IndexOf(":"))
                    {
                        Time_Selection.SelectionLength = 0;
                        string[] hoursplit = Time_Selection.Text.Split(':');
                        if (hoursplit[0].Length == 0)
                        {
                            hoursplit[0] = "01";
                            Time_Selection.Text = hoursplit[0] + ":" + hoursplit[1];
                        }
                        if (hoursplit[1].Length > 0)
                        {
                            string[] k = hoursplit[1].Split(' ');
                            if (k[0].Length == 0)
                            {
                                k[0] = "00";
                                Time_Selection.Text = hoursplit[0] + ":" + k[0] + " " + k[1];
                            }

                        }
                    }


                    if ((Keyboard.IsKeyDown(Key.LeftCtrl)) || (Keyboard.IsKeyDown(Key.RightCtrl)) || ((Keyboard.IsKeyDown(Key.LeftShift)) || (Keyboard.IsKeyDown(Key.RightShift))))
                    {
                        switch (e.Key)
                        {
                            case Key.A:
                                allowselection();
                                e.Handled = true;
                                break;
                            case Key.Right:
                                allowselection();
                                e.Handled = true;
                                break;
                            case Key.Left:
                                allowselection();
                                e.Handled = true;
                                break;
                            case Key.V:
                                e.Handled = true;
                                break;
                            case Key.C:
                                e.Handled = true;
                                break;
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
        }



        private void allowselection()
        {
            int index = Time_Selection.CaretIndex;

            if (index <= 2)
            {
                Time_Selection.SelectionStart = 0;
                Time_Selection.Select(0, 2);

               
            }
            if (index > 2 && index <= 4)
            {
                Time_Selection.SelectionStart = 3;
                Time_Selection.SelectionLength = 2;
             
            }
            if (index >= 5)
            {
                Time_Selection.SelectionStart = 6;
                Time_Selection.SelectionLength = 2;
               
            }
        }

        private void Time_Selection_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            try
            {
                e.Handled = !IsTextAllowed(e.Text);
                if (e.Handled)
                    return;
                string Matchtext = string.Empty;
                var textbox = e.OriginalSource as TextBox;
				
				
				  if (Time_Selection.SelectedText != string.Empty)
                {
                    if (Time_Selection.SelectedText.Contains(":") || Time_Selection.SelectedText.Contains("A") || Time_Selection.SelectedText.Contains("M") || Time_Selection.SelectedText.Contains("P") || Time_Selection.SelectedText.Contains(" "))
                    {
                        e.Handled = true;
                        return;
                    }
                }

                if(Time_Selection.SelectedText==string.Empty && e.Text!=null && Time_Selection.Text.Length==8)
                {
                    e.Handled = true;
                    return;
                }

                   Matchtext = Timeformat();
                int Index1 = textbox.Text.IndexOf(':');
                int Textindex = textbox.CaretIndex;



                switch (Textindex)
                {
                    case 0:
                        Matchtext = Matchtext.Remove(Textindex, 1);
                        if (Matchtext[1].ToString() != ":")
                            Matchtext = Matchtext.Insert(Textindex + 1, e.Text);
                        else
                            Matchtext = Matchtext.Insert(Textindex, e.Text);
                        break;

                    case 1:
                        Matchtext = Matchtext.Remove(Textindex - 1, 1);
                        Matchtext = Matchtext.Insert(Textindex, e.Text);
                        break;

                    case 3:
                        Matchtext = Matchtext.Remove(Textindex, 1);
                        Matchtext = Matchtext.Insert(Textindex, e.Text);
                        break;

                    case 4:
                        Matchtext = Matchtext.Remove(Textindex - 1, 1);
                        Matchtext = Matchtext.Insert(Textindex, e.Text);
                        break;

                }

                if ((Regex.IsMatch(Matchtext, @"^(0[1-9]|1[0-2]):[0-5][0-9] [ap]m$", RegexOptions.IgnoreCase) || textbox.Text[0].ToString()==":") && Textindex!=2 && Textindex<5)
                    
                    {
                   
                        e.Handled = false;

                    }
                    else
                    {
                
                         e.Handled = true;
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

        private void CalendarDate_GotMouseCapture(object sender, MouseEventArgs e)
        {
            try
            {
                UIElement originalElement = e.OriginalSource as UIElement;
                if (originalElement != null && originalElement.GetType().ToString() != "System.Windows.Controls.Button" && originalElement.GetType().ToString() != "System.Windows.Controls.Primitives.CalendarButton")
                {
                    originalElement.ReleaseMouseCapture();
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
		
		 private string Timeformat()
        {
            string hh = string.Empty;
            string mm = string.Empty;
            string[] splittime1 = Time_Selection.Text.Split(':');
            string[] splittime3 = splittime1[1].Split(' ');
            if (splittime1[0] == " " || splittime1[0] == string.Empty)
            {
                 hh = "00";
            }
            else
            {
                 hh = string.Format("{0:00}", Convert.ToInt16(splittime1[0]));
            }
            if (splittime3[0] == " " || splittime3[0] == string.Empty)
            {
                 mm = "00";
            }
            else
            {
                 mm = string.Format("{0:00}", Convert.ToInt16(splittime3[0]));
            }
            return hh + ":" + mm + " " + splittime3[1];
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
                //MessageBox.Show("Exception\n " + ex.Message, "QAT Error Code - EC12019", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return !regex.IsMatch(text);
        }


        private void Time_Selection_MouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                var textbox = sender as TextBox;
                textbox.SelectionLength = 0;
                string[] hoursplit = textbox.Text.Split(':');

                if (hoursplit[0].Length == 0)
                {
                    hoursplit[0] = "01";
                    textbox.Text = hoursplit[0] + ":" + hoursplit[1];
                }

                if (hoursplit[1].Length > 0)
                {
                    string[] k = hoursplit[1].Split(' ');
                    if (k[0].Length == 0)
                    {
                        k[0] = "00";
                        textbox.Text = hoursplit[0] + ":" + k[0] + " " + k[1];
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
        }

        private void Time_Selection_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            try
            {
                var textbox = sender as TextBox;
                if ((textbox.CaretIndex <= 2 && textbox.SelectionLength < 2) || (textbox.CaretIndex > 2 && textbox.CaretIndex <= 5 && textbox.SelectionLength < 2) || (textbox.CaretIndex > 5 && textbox.SelectionLength < 2))
                {
                    e.Handled = false;
                }
                else
                {
                    e.Handled = true;
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

        private void Up_Click(object sender, RoutedEventArgs e)
        {

        }
    }

    public class ComboBoxItem
    {
        public string DisplayText { get; set; }
        public bool? IsHeader { get; set; }
        public string TZName { get; set; }
    }
}

