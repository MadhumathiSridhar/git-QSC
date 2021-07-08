using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Net;
using System.Threading;
using System.Net.Sockets;
using System.Xml;
using System.Text.RegularExpressions;
using System.IO;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Threading;

namespace QSC_Test_Automation
{
    static class DeviceDiscovery
    {

        public static string Netpair_devicesSupported = string.Empty;
        public delegate void discoveryDelegate();
        public static Window startUpWindow = null;
        public static DateTime discoverySleepStartTime = DateTime.MinValue;      
        public static List<DUT_DeviceItem> selectedDutDeviceItemListColor = new List<DUT_DeviceItem>();
        public static bool Cancelclick = false;

        public static DateTime InputTime = DateTime.Now;

        public static Dictionary<string,DUT_DeviceItem> DoubleCoreList = new Dictionary<string,DUT_DeviceItem>();
        public static string ConfigFileName = null;

        static DirectoryInfo logFileDirectory = new DirectoryInfo(@"C:\Users\" + Environment.UserName + @"\AppData\Local\Temp\QSys Temp Files");
        static string logFileName = "QAT_LogFile.txt";
        static string logFilePath = null;
        static int logFileLineCount = 0;
        static object lockLogFileWrite = new object();
        static bool isDeviceDiscoveryComplete = false;

        static List<discoveryDelegate> completeEventList = new List<discoveryDelegate>();

        static List<discoveryDelegate> startEventList = new List<discoveryDelegate>();
        static List<List<string>> allComponentList = new List<List<string>>();

        static Thread discoveryThead = null;
        static ArrayList availableDeviceList = new ArrayList();
        public  static Array availableDeviceList_script = new Array[0];
        static ArrayList availableDeviceListCopy = new ArrayList();
        static int udpDelayTime = 0;
        static object lockDeviceDiscoveryThread = new object();
        static object lockDeviceDiscoveryTask = new object();
        static object lockAvailableDeviceList = new object();
        static object lockWindowCreation = new object();
        static string QREMtoken = string.Empty;

        public static void StartDiscoveryThread()
        {
            try
            {
                lock (lockDeviceDiscoveryThread)
                {
                    if (discoveryThead == null || discoveryThead.IsAlive != true)
                    {
                        discoveryThead = new Thread(() => DiscoveryLoop());
                        discoveryThead.Start();
                        //WriteToLogFile("Device Discovery Started");
                    }
                }
            }
            catch (Exception ex)
            {
                WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
            }
        }

		////Using preference user given QREM username and password we can get token for 300sec once
        private static void GetQREMToken()
        {
            try
            {
                if (!string.IsNullOrEmpty(Properties.Settings.Default.QREMUserName) && !string.IsNullOrEmpty(Properties.Settings.Default.QREMPassword))
                {
                    string stringParameter = "{\"username\": \"" + Properties.Settings.Default.QREMUserName + "\",\"password\": \"" + Properties.Settings.Default.QREMPassword + "\"}";
                    string strResponse = string.Empty;
                    bool isSuccess = HttpPostactual("https://" + Properties.Settings.Default.QREMreflectLink + "/api/qsd/v0/users/auth/oauth/password?provider=qscId", stringParameter, "POST", string.Empty, out strResponse);

                    if (isSuccess)
                    {
                        dynamic array = new System.Web.Script.Serialization.JavaScriptSerializer().DeserializeObject(strResponse);

                        if (array != null && array.Count > 0)
                        {
                            foreach (var item in array)
                            {
                                if (item.Key == "token")
                                {
                                    QREMtoken = item.Value;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
            }
        }

        private static void DiscoveryLoop()
        {
            try
            {
                while (true)
                {
                    availableDeviceListCopy.Clear();

                    GetQREMToken();

                    for (int i = 0; i < 15; i++)
                    {
                        StartDiscovery();

                        List<discoveryDelegate> completedEventlist = new List<discoveryDelegate>();
                        completedEventlist.AddRange(completeEventList);

                        foreach (discoveryDelegate CompleteEvent in completedEventlist)
                        {
                            if (CompleteEvent != null)
                                CompleteEvent();
                        }

                        Thread.Sleep(1000);
                    }

                    isDeviceDiscoveryComplete = true;

                    lock (lockAvailableDeviceList)
                    {
                        foreach (var item in (ArrayList)availableDeviceList.Clone())
                        {
                            if (!availableDeviceListCopy.Contains(item))
                                availableDeviceList.Remove(item);
                        }
                    }


                    Array sortedDeviceList = availableDeviceList.ToArray();
                    Array.Sort(sortedDeviceList);
                    availableDeviceList_script = sortedDeviceList;

                    List<discoveryDelegate> completedEventlists = new List<discoveryDelegate>();
                    completedEventlists.AddRange(completeEventList);

                    foreach (discoveryDelegate CompleteEvent in completedEventlists)
                    {
                        if (CompleteEvent != null)
                            CompleteEvent();
                    }

                    discoverySleepStartTime = DateTime.Now;
                    Thread.Sleep(300000);
                    discoverySleepStartTime = DateTime.MinValue;
                }
            }
            catch (Exception ex)
            {
                WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
            }
        }

        private static void StartDiscovery()
        {
            try
            {
                List<Task> udpPostThreadList = new List<Task>();
                XmlDocument xmldoc = new XmlDocument();
                List<string> allLocalIP = GetLocalIPAddress();
                ArrayList deviceList = new ArrayList();

                //if (Properties.Settings.Default.DutConfigDelay != string.Empty)
                //    udpDelayTime = Convert.ToInt32(Properties.Settings.Default.DutConfigDelay);
                //else
                udpDelayTime = 500;

                if (udpDelayTime < 100)
                    udpDelayTime = 100;
                else if (udpDelayTime > 60000)
                    udpDelayTime = 60000;

                string[] strUDPQuery = new string[3];
                strUDPQuery[0] = "<QDP><query_ref>device.core*</query_ref></QDP>";
                strUDPQuery[1] = "<QDP><query_ref>device.ioframe*</query_ref></QDP>";
                xmldoc.LoadXml(@"<?xml version=""1.0"" encoding=""utf-8""?>
                                <s:Envelope xmlns:s=""http://www.w3.org/2003/05/soap-envelope"" xmlns:a=""http://schemas.xmlsoap.org/ws/2004/08/addressing"">
                                <s:Header>
                                    <a:Action s:MustUnderstand=""1"">
                                    http://schemas.xmlsoap.org/ws/2005/04/discovery/Probe</a:Action>http://schemas.xmlsoap.org/ws/2005/04/discovery/Probe%3c/a:Action>>
                                       <a:MessageID>uuid:df073ed0-8d21-11e6-831a-198e94208c36</a:MessageID>             
                                                <a:To>urn:schemas-xmlsoaporg:ws:2005:04:discovery</a:To>                     
                                                       </s:Header>                      
                                                        <s:Body>                       
                                                           <d:Probe xmlns:d=""http://schemas.xmlsoap.org/ws/2005/04/discovery"" xmlns:o=""http://www.onvif.org/ver10/network/wsdl"">
                                <d:Types>o:QSCVideoDevice</d:Types>
                                </d:Probe>
                                </s:Body>
                                </s:Envelope>");
                strUDPQuery[2] = xmldoc.InnerXml.ToString();

                allComponentList.Clear();

                //WriteToLogFile("Discovery of Core started for IP: " + string.Join(", ",allLocalIP.ToArray()));

                //iterate through all the ports for Core
                foreach (string myIP in allLocalIP)
                {
                    int intPort = 2467;
                    string strIP = "224.0.23.175";

                    udpPostThreadList.Add(Task.Factory.StartNew(() => UdpPost(intPort + 0, strIP, myIP, strUDPQuery[0])));
                    udpPostThreadList.Add(Task.Factory.StartNew(() => UdpPost(intPort + 1, strIP, myIP, strUDPQuery[0])));
                    udpPostThreadList.Add(Task.Factory.StartNew(() => UdpPost(intPort + 2, strIP, myIP, strUDPQuery[0])));
                    udpPostThreadList.Add(Task.Factory.StartNew(() => UdpPost(intPort + 3, strIP, myIP, strUDPQuery[0])));
                }

                if (!Task.WaitAll(udpPostThreadList.ToArray(), udpDelayTime + 60000))
                {
                    WriteToLogFile("Discovery of Core Timed Out");
                    foreach (Task item in udpPostThreadList)
                    {
                        if (item.Status != TaskStatus.RanToCompletion)
                        {
                            int intPort = 2467 + (udpPostThreadList.IndexOf(item) % 4);
                            int index = udpPostThreadList.IndexOf(item) / 4;
                            WriteToLogFile("Task Timed for IP: " + allLocalIP[index] + " Port: " + intPort + " Task Status: " + item.Status);
                        }
                    }
                }

                udpPostThreadList.Clear();

                //WriteToLogFile("Discovery of Peripherals started for IP: " + string.Join(", ", allLocalIP.ToArray()));

                //iterate through all the ports for Peripheral
                foreach (string myIP in allLocalIP)
                {
                    int intPort = 2467;
                    string strIP = "224.0.23.175";

                    udpPostThreadList.Add(Task.Factory.StartNew(() => UdpPost(intPort + 0, strIP, myIP, strUDPQuery[1])));
                    udpPostThreadList.Add(Task.Factory.StartNew(() => UdpPost(intPort + 1, strIP, myIP, strUDPQuery[1])));
                    udpPostThreadList.Add(Task.Factory.StartNew(() => UdpPost(intPort + 2, strIP, myIP, strUDPQuery[1])));
                    udpPostThreadList.Add(Task.Factory.StartNew(() => UdpPost(intPort + 3, strIP, myIP, strUDPQuery[1])));
                }

                if (!Task.WaitAll(udpPostThreadList.ToArray(), udpDelayTime + 10000))
                {
                    WriteToLogFile("Discovery of Peripheral Timed Out");
                    foreach (Task item in udpPostThreadList)
                    {
                        if (item.Status != TaskStatus.RanToCompletion)
                        {
                            int intPort = 2467 + (udpPostThreadList.IndexOf(item) % 4);
                            int index = udpPostThreadList.IndexOf(item) / 4;
                            WriteToLogFile("Task Timed for IP: " + allLocalIP[index] + " Port: " + intPort + " Task Status: " + item.Status);
                        }
                    }
                }

                udpPostThreadList.Clear();

                //WriteToLogFile("Discovery Completed for IP: " + string.Join(", ", allLocalIP.ToArray()));
                //iterate through all the ports for camera
                foreach (string myIP in allLocalIP)
                {
                    int intPort = 3702;
                    string strIP = "239.255.255.250";

                    udpPostThreadList.Add(Task.Factory.StartNew(() => UdpPost(intPort, strIP, myIP, strUDPQuery[2])));
                    //udpPostThreadList.Add(Task.Factory.StartNew(() => UdpPost(intPort + 1, strIP, myIP, strUDPQuery[2])));
                    //udpPostThreadList.Add(Task.Factory.StartNew(() => UdpPost(intPort + 2, strIP, myIP, strUDPQuery[2])));
                    //udpPostThreadList.Add(Task.Factory.StartNew(() => UdpPost(intPort + 3, strIP, myIP, strUDPQuery[2])));
                }

                if (!Task.WaitAll(udpPostThreadList.ToArray(), udpDelayTime + 10000))
                {
                    WriteToLogFile("Discovery of Peripheral Timed Out");
                    foreach (Task item in udpPostThreadList)
                    {
                        if (item.Status != TaskStatus.RanToCompletion)
                        {
                            int intPort = 3702 + (udpPostThreadList.IndexOf(item) % 4);
                            int index = udpPostThreadList.IndexOf(item) / 4;
                            WriteToLogFile("Task Timed for IP: " + allLocalIP[index] + " Port: " + intPort + " Task Status: " + item.Status);
                        }
                    }
                }

                udpPostThreadList.Clear();


				///////Using udppost QREM device discovery will happen
                Dictionary<string, Dictionary<string, string>> QREMdevicesdetails = new Dictionary<string, Dictionary<string, string>>();

                if (!string.IsNullOrEmpty(QREM_Token))
                {
                    udpPostThreadList.Add(Task.Factory.StartNew(() => QREMdevicesdetails = QREMDeviceDiscovery("https://" + Properties.Settings.Default.QREMreflectLink + "/api/qsd/v0/cores", QREM_Token)));
                    
                    if (!Task.WaitAll(udpPostThreadList.ToArray(), 30000))
                    {
                        WriteToLogFile("Discovery of QREM Timed Out");                        
                    }

                    udpPostThreadList.Clear();
                }

                lock (lockAvailableDeviceList)
                {
                    deviceList = availableDeviceList;
                }

                lock (lockDeviceDiscoveryThread)
                {
                    foreach (List<string> componentName in allComponentList)
                    {
                        if (componentName != null)
                        {
                            foreach (string individualcompName in componentName)
                            {
                                string part_number = Regex.Match(individualcompName, @"<part_number>(.+?)</part_number>").Groups[1].Value;
                                string name = Regex.Match(individualcompName, @"<name>(.+?)</name>").Groups[1].Value.Trim();
                                string lan_a_ip = Regex.Match(individualcompName, @"<lan_a_ip>(.+?)</lan_a_ip>").Groups[1].Value;
                                string lan_b_ip = Regex.Match(individualcompName, @"<lan_b_ip>(.+?)</lan_b_ip>").Groups[1].Value;
                                
                                if (part_number != string.Empty)
                                {
                                    if (!deviceList.Contains(part_number + "," + name + "," + lan_a_ip + "," + lan_b_ip + ",Localdevice"))
                                    {
                                        deviceList.Add(part_number + "," + name + "," + lan_a_ip + "," + lan_b_ip + ",Localdevice");
                                    }

                                    if (!availableDeviceListCopy.Contains(part_number + "," + name + "," + lan_a_ip + "," + lan_b_ip + ",Localdevice"))
                                    {
                                        availableDeviceListCopy.Add(part_number + "," + name + "," + lan_a_ip + "," + lan_b_ip + ",Localdevice");
                                    }
                                }
                                else if (!(string.IsNullOrEmpty(Regex.Match((Regex.Match(individualcompName, @"<wsdd:Scopes>(.+?)</wsdd:Scopes>").Groups[1].Value), @"qsc://qsc.com/hardware/(.*)").Groups[1].Value)))
                                {
                                    string camerapart = Regex.Match((Regex.Match(individualcompName, @"<wsdd:Scopes>(.+?)</wsdd:Scopes>").Groups[1].Value), @"qsc://qsc.com/hardware/(.*)").Groups[1].Value;
                                    string cameraname = Regex.Match((Regex.Match(individualcompName, @"<wsdd:Scopes>(.+?)</wsdd:Scopes>").Groups[1].Value), @"qsc://qsc.com/name/(.+?)qsc://qsc.com/hardware/").Groups[1].Value.Trim();
                                    string cameralan_a_ip = Regex.Match((Regex.Match(individualcompName, @"<wsdd:XAddrs>(.+?)</wsdd:XAddrs>").Groups[1].Value), @"http://(.+?):").Groups[1].Value;
                                    string cameralan_b_ip = string.Empty;

                                    if (!deviceList.Contains(camerapart + "," + cameraname + "," + cameralan_a_ip + "," + cameralan_b_ip + ",Localdevice"))
                                    {
                                        deviceList.Add(camerapart + "," + cameraname + "," + cameralan_a_ip + "," + cameralan_b_ip + ",Localdevice");
                                    }

                                    if (!availableDeviceListCopy.Contains(camerapart + "," + cameraname + "," + cameralan_a_ip + "," + cameralan_b_ip + ",Localdevice"))
                                    {
                                        availableDeviceListCopy.Add(camerapart + "," + cameraname + "," + cameralan_a_ip + "," + cameralan_b_ip + ",Localdevice");
                                    }
                                }
                            }
                        }
                    }

					/////////Discoverd devices are added in devicelist for DUT configuration
                    foreach (var qremVal in QREMdevicesdetails)
                    {
                        if (!deviceList.Contains(qremVal.Value["CoreModel"] + "," + qremVal.Key + ",,," + qremVal.Value["CoreName"] + ";" + qremVal.Value["ID"] + ";" + qremVal.Value["SystemID"] + ";" + qremVal.Value["SiteID"]))
                            deviceList.Add(qremVal.Value["CoreModel"] + "," + qremVal.Key + ",,," + qremVal.Value["CoreName"] + ";"+ qremVal.Value["ID"] + ";" + qremVal.Value["SystemID"] + ";" + qremVal.Value["SiteID"]);

                        if (!availableDeviceListCopy.Contains(qremVal.Value["CoreModel"] + "," + qremVal.Key + ",,," + qremVal.Value["CoreName"] + ";" + qremVal.Value["ID"] + ";" + qremVal.Value["SystemID"] + ";" + qremVal.Value["SiteID"]))
                            availableDeviceListCopy.Add(qremVal.Value["CoreModel"] + "," + qremVal.Key + ",,," + qremVal.Value["CoreName"] + ";" + qremVal.Value["ID"] + ";" + qremVal.Value["SystemID"] + ";" + qremVal.Value["SiteID"]);
                    }

                    lock (lockAvailableDeviceList)
                    {
                        availableDeviceList = deviceList;
                    }

                    //WriteToLogFile("No. of devices found: " + availableDeviceList.Count);

                }
            }
            catch (Exception ex)
            {
                WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                        MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
            }
        }

		//////////////QREM device discovery (Only online cores)
        private static Dictionary<string, Dictionary<string, string>> QREMDeviceDiscovery(string strURI, string token)
        {
            Dictionary<string, Dictionary<string, string>> QREMdevicedetails = new Dictionary<string, Dictionary<string, string>>();

            try
            {
                string strResponse = string.Empty;
                Tuple<bool, string> outval = HttpGetQREMjson(strURI, token, out strResponse);
                if (outval != null && outval.Item1)
                {
                    dynamic array = new System.Web.Script.Serialization.JavaScriptSerializer().DeserializeObject(strResponse);

                    foreach (var item in array)
                    {
                        Dictionary<string, object> objectlist = item as Dictionary<string, object>;

                        if (objectlist != null && objectlist.Count > 0)
                        {
                            string isOffline = objectlist["offline"].ToString();

                            if (isOffline.ToLower() == "false")
                            {
                                Dictionary<string, string> values = new Dictionary<string, string>();

                                values.Add("ID", objectlist["id"].ToString());
                                string coreName = objectlist["name"].ToString();
                                values.Add("CoreName", coreName);
                                values.Add("Firmware", objectlist["firmware"].ToString());
                                string coreModel = objectlist["model"].ToString();
                                values.Add("CoreModel", objectlist["model"].ToString());

                                Dictionary<string, object> statusList = objectlist["status"] as Dictionary<string, object>;
                                values.Add("CoreStatus", statusList["message"].ToString());

                                Dictionary<string, object> designList = objectlist["design"] as Dictionary<string, object>;
                                values.Add("DesignCode", designList["code"].ToString());
                                values.Add("DesignName", designList["name"].ToString());

                                Dictionary<string, object> siteList = objectlist["site"] as Dictionary<string, object>;
                                string siteName = siteList["name"].ToString();
                                values.Add("SiteName", siteName);

                                string siteID = siteList["id"].ToString();
                                values.Add("SiteID", siteID);

                                Dictionary<string, object> systemList = objectlist["system"] as Dictionary<string, object>;
                                string systemID = systemList["id"].ToString();
                                values.Add("SystemID", systemID);

                                Dictionary<string, object> organizationList = objectlist["organization"] as Dictionary<string, object>;
                                string organizationName = organizationList["name"].ToString();
                                values.Add("organizationName", organizationName);

                                if (!QREMdevicedetails.ContainsKey(coreName + "_" + organizationName + "_" + siteName))
                                    QREMdevicedetails.Add(coreName + "_" + organizationName + "_" + siteName, values);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
            }

            return QREMdevicedetails;
        }
        
		//////////////Httpget for QREM reflect cores
        public static Tuple<bool, string> HttpGetQREMjson(string strURI, string token, out string strResponse)
        {
            string msg = string.Empty;
            strResponse = string.Empty;
            bool success = false;
            System.Net.HttpWebRequest req = null;

            try
            {
                req = (HttpWebRequest)System.Net.WebRequest.Create(strURI);
                req.ContentType = "application/json; charset=utf-8";
                req.Method = "GET";
                req.Timeout = 60000;
                req.ReadWriteTimeout = 60000;
                if (token != string.Empty)
                    req.Headers["Authorization"] = "Bearer " + token;
                
                using (HttpWebResponse resp = (HttpWebResponse)req.GetResponse())
                {
                    success = HttpStatusCodeCheck(resp, "GET", out strResponse);
                }

                req.Abort();
                return new Tuple<bool, string>(success, msg);
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                ExecutionMessageBox("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif

                strResponse = "";
                if (ex.Message == "Unable to connect to the remote server" || ex.Message == "The operation has timed out")
                {
                    msg = "Unable to connect to the remote server";
                }
                else
                {
                    msg = ex.Message;
                }

                if (req != null)
                    req.Abort();

                return new Tuple<bool, string>(success, msg);
            }
        }
        
        private static bool HttpStatusCodeCheck(HttpWebResponse response, string methodName, out string strResponse)
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
                return false;
            }
        }

        public static bool HttpPostactual(string strURI, string strParameters, string methodName, string qremToken, out string strResponse)
        {
            strResponse = string.Empty;
            bool success = false;

            try
            {
                System.Net.HttpWebRequest req = (HttpWebRequest)System.Net.WebRequest.Create(strURI);
                //req.Proxy = new System.Net.WebProxy(strProxy, true);

                req.Timeout = 30000;
                req.ReadWriteTimeout = 30000;
                req.ContentType = "application/json; charset=utf-8";
                req.Method = methodName;

                Byte[] retBytes = System.Text.Encoding.ASCII.GetBytes(strParameters);
                req.ContentLength = retBytes.Length;

                if (qremToken != string.Empty)
                    req.Headers["Authorization"] = "Bearer " + qremToken;

                using (System.IO.Stream outStream = req.GetRequestStream())
                {
                    outStream.Write(retBytes, 0, retBytes.Length);
                    outStream.Close();
                }

                using (System.Net.WebResponse resp = req.GetResponse())
                {
                    if (resp == null)
                    {
                        return success;
                    }
                    else if (((HttpWebResponse)resp).StatusCode.ToString().ToUpper() == "OK")
                    {
                        System.IO.StreamReader sr = new System.IO.StreamReader(resp.GetResponseStream());
                        strResponse = sr.ReadToEnd().Trim();
                        success = true;
                    }
                }

                return success;
            }
            catch (Exception ex)
            {
                //Debug.WriteLine("Error in Web_RW.HttpPost: no response from " + strURI.ToString() + "\n\r" + ex.ToString());
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                ExecutionMessageBox("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                return success;
            }
        }       
       

        //        private static void StartDiscovery()
        //        {
        //            try
        //            {
        //                List<Task> udpPostThreadList = new List<Task>();
        //                XmlDocument xmldoc = new XmlDocument();
        //                List<string> allLocalIP = GetLocalIPAddress();
        //                ArrayList deviceList_St1 = new ArrayList();
        //                ArrayList deviceList_St2 = new ArrayList();
        //                ArrayList deviceList_St3 = new ArrayList();
        //                ArrayList total = new ArrayList();

        //                //if (Properties.Settings.Default.DutConfigDelay != string.Empty)
        //                //    udpDelayTime = Convert.ToInt32(Properties.Settings.Default.DutConfigDelay);
        //                //else
        //                udpDelayTime = 500;

        //                if (udpDelayTime < 100)
        //                    udpDelayTime = 100;
        //                else if (udpDelayTime > 60000)
        //                    udpDelayTime = 60000;

        //                string[] strUDPQuery = new string[3];
        //                strUDPQuery[0] = "<QDP><query_ref>device.core*</query_ref></QDP>";
        //                strUDPQuery[1] = "<QDP><query_ref>device.ioframe*</query_ref></QDP>";
        //                xmldoc.LoadXml(@"<?xml version=""1.0"" encoding=""utf-8""?>
        //                        <s:Envelope xmlns:s=""http://www.w3.org/2003/05/soap-envelope"" xmlns:a=""http://schemas.xmlsoap.org/ws/2004/08/addressing"">
        //                        <s:Header>
        //                            <a:Action s:MustUnderstand=""1"">
        //                            http://schemas.xmlsoap.org/ws/2005/04/discovery/Probe</a:Action>http://schemas.xmlsoap.org/ws/2005/04/discovery/Probe%3c/a:Action>>
        //                               <a:MessageID>uuid:df073ed0-8d21-11e6-831a-198e94208c36</a:MessageID>             
        //                                        <a:To>urn:schemas-xmlsoaporg:ws:2005:04:discovery</a:To>                     
        //                                               </s:Header>                      
        //                                                <s:Body>                       
        //                                                   <d:Probe xmlns:d=""http://schemas.xmlsoap.org/ws/2005/04/discovery"" xmlns:o=""http://www.onvif.org/ver10/network/wsdl"">
        //                        <d:Types>o:QSCVideoDevice</d:Types>
        //                        </d:Probe>
        //                        </s:Body>
        //                        </s:Envelope>");
        //                strUDPQuery[2] = xmldoc.InnerXml.ToString();

        //                allComponentList.Clear();

        //                //WriteToLogFile("Discovery of Core started for IP: " + string.Join(", ",allLocalIP.ToArray()));

        //                //iterate through all the ports for Core
        //                foreach (string myIP in allLocalIP)
        //                {
        //                    int intPort = 2467;
        //                    string strIP = "224.0.23.175";

        //                    udpPostThreadList.Add(Task.Factory.StartNew(() => UdpPost(intPort + 0, strIP, myIP, strUDPQuery[0])));
        //                    udpPostThreadList.Add(Task.Factory.StartNew(() => UdpPost(intPort + 1, strIP, myIP, strUDPQuery[0])));
        //                    udpPostThreadList.Add(Task.Factory.StartNew(() => UdpPost(intPort + 2, strIP, myIP, strUDPQuery[0])));
        //                    udpPostThreadList.Add(Task.Factory.StartNew(() => UdpPost(intPort + 3, strIP, myIP, strUDPQuery[0])));
        //                }

        //                if (!Task.WaitAll(udpPostThreadList.ToArray(), udpDelayTime + 60000))
        //                {
        //                    WriteToLogFile("Discovery of Core Timed Out");
        //                    foreach (Task item in udpPostThreadList)
        //                    {
        //                        if (item.Status != TaskStatus.RanToCompletion)
        //                        {
        //                            int intPort = 2467 + (udpPostThreadList.IndexOf(item) % 4);
        //                            int index = udpPostThreadList.IndexOf(item) / 4;
        //                            WriteToLogFile("Task Timed for IP: " + allLocalIP[index] + " Port: " + intPort + " Task Status: " + item.Status);
        //                        }
        //                    }
        //                }

        //                udpPostThreadList.Clear();

        //                //          //WriteToLogFile("Discovery of Peripherals started for IP: " + string.Join(", ", allLocalIP.ToArray()));

        //                //          //iterate through all the ports for Peripheral
        //                //          foreach (string myIP in allLocalIP)
        //                //          {
        //                //              int intPort = 2467;
        //                //              string strIP = "224.0.23.175";

        //                //              udpPostThreadList.Add(Task.Factory.StartNew(() => UdpPost(intPort + 0, strIP, myIP, strUDPQuery[1])));
        //                //              udpPostThreadList.Add(Task.Factory.StartNew(() => UdpPost(intPort + 1, strIP, myIP, strUDPQuery[1])));
        //                //              udpPostThreadList.Add(Task.Factory.StartNew(() => UdpPost(intPort + 2, strIP, myIP, strUDPQuery[1])));
        //                //              udpPostThreadList.Add(Task.Factory.StartNew(() => UdpPost(intPort + 3, strIP, myIP, strUDPQuery[1])));
        //                //          }

        //                //          if (!Task.WaitAll(udpPostThreadList.ToArray(), udpDelayTime + 10000))
        //                //          {
        //                //              WriteToLogFile("Discovery of Peripheral Timed Out");
        //                //              foreach (Task item in udpPostThreadList)
        //                //              {
        //                //                  if (item.Status != TaskStatus.RanToCompletion)
        //                //                  {
        //                //                      int intPort = 2467 + (udpPostThreadList.IndexOf(item) % 4);
        //                //                      int index = udpPostThreadList.IndexOf(item) / 4;
        //                //                      WriteToLogFile("Task Timed for IP: " + allLocalIP[index] + " Port: " + intPort + " Task Status: " + item.Status);
        //                //                  }
        //                //              }
        //                //          }

        //                //          udpPostThreadList.Clear();

        //                //WriteToLogFile("Discovery Completed for IP: " + string.Join(", ", allLocalIP.ToArray()));
        //                //iterate through all the ports for camera
        //                foreach (string myIP in allLocalIP)
        //                {
        //                    int intPort = 3702;
        //                    string strIP = "239.255.255.250";

        //                    udpPostThreadList.Add(Task.Factory.StartNew(() => UdpPost(intPort, strIP, myIP, strUDPQuery[2])));
        //                    //udpPostThreadList.Add(Task.Factory.StartNew(() => UdpPost(intPort, strIP, myIP, strUDPQuery[2])));
        //                    //udpPostThreadList.Add(Task.Factory.StartNew(() => UdpPost(intPort, strIP, myIP, strUDPQuery[2])));
        //                    //udpPostThreadList.Add(Task.Factory.StartNew(() => UdpPost(intPort, strIP, myIP, strUDPQuery[2])));
        //                }

        //                if (!Task.WaitAll(udpPostThreadList.ToArray(), udpDelayTime + 10000))
        //                {
        //                    WriteToLogFile("Discovery of Peripheral Timed Out");
        //                    foreach (Task item in udpPostThreadList)
        //                    {
        //                        if (item.Status != TaskStatus.RanToCompletion)
        //                        {
        //                            int intPort = 3702 + (udpPostThreadList.IndexOf(item) % 4);
        //                            int index = udpPostThreadList.IndexOf(item) / 4;
        //                            WriteToLogFile("Task Timed for IP: " + allLocalIP[index] + " Port: " + intPort + " Task Status: " + item.Status);
        //                        }
        //                    }
        //                }

        //                udpPostThreadList.Clear();


        //                lock (lockAvailableDeviceList)
        //                {
        //                    deviceList_St1 = new ArrayList(availableDeviceList);
        //                }


        //                lock (lockDeviceDiscoveryThread)
        //                {
        //                    //using (StreamWriter write = new StreamWriter(@"C:\CoreDetails.txt"))
        //                    //{
        //                    foreach (List<string> componentName in allComponentList)
        //                    {
        //                        if (componentName != null)
        //                        {
        //                            foreach (string individualcompName in componentName)
        //                            {
        //                                string part_number = Regex.Match(individualcompName, @"<part_number>(.+?)</part_number>").Groups[1].Value;
        //                                string name = Regex.Match(individualcompName, @"<name>(.+?)</name>").Groups[1].Value.Trim();
        //                                string lan_a_ip = Regex.Match(individualcompName, @"<lan_a_ip>(.+?)</lan_a_ip>").Groups[1].Value;
        //                                string lan_b_ip = Regex.Match(individualcompName, @"<lan_b_ip>(.+?)</lan_b_ip>").Groups[1].Value;
        //                                string aux_a_ip = Regex.Match(individualcompName, @"<aux_a_ip>(.+?)</aux_a_ip>").Groups[1].Value;
        //                                string aux_b_ip = Regex.Match(individualcompName, @"<aux_b_ip>(.+?)</aux_b_ip>").Groups[1].Value;


        //                                if (part_number != string.Empty)
        //                                {
        //                                    if (!deviceList_St1.Contains(part_number + "," + name + "," + lan_a_ip + "," + lan_b_ip))
        //                                    {
        //                                        deviceList_St1.Add(part_number + "," + name + "," + lan_a_ip + "," + lan_b_ip);

        //                                        if (lan_a_ip != null && lan_a_ip != string.Empty)
        //                                        {
        //                                            deviceList_St2 = getcoredevices(lan_a_ip, deviceList_St2);
        //                                        }
        //                                        else if (lan_b_ip != null && lan_b_ip != string.Empty)
        //                                        {
        //                                            deviceList_St2 = getcoredevices(lan_b_ip, deviceList_St2);
        //                                        }

        //                                        else if (aux_a_ip != null && aux_a_ip != string.Empty)
        //                                        {
        //                                            deviceList_St2 = getcoredevices(aux_a_ip, deviceList_St2);
        //                                        }

        //                                        else if (aux_b_ip != null && aux_b_ip != string.Empty)
        //                                        {
        //                                            deviceList_St2 = getcoredevices(aux_b_ip, deviceList_St2);
        //                                        }
        //                                    }

        //                                    //if (!availableDeviceListCopy.Contains(part_number + "," + name + "," + lan_a_ip + "," + lan_b_ip))
        //                                    //{
        //                                    ////    availableDeviceListCopy.Add(part_number + "," + name + "," + lan_a_ip + "," + lan_b_ip);
        //                                    //}

        //                                    //write.WriteLine(part_number + " ," + name + " ," + lan_a_ip + " ," + lan_b_ip + " ," + aux_a_ip + " ," + aux_b_ip);
        //                                }
        //                                else if (!(string.IsNullOrEmpty(Regex.Match((Regex.Match(individualcompName, @"<wsdd:Scopes>(.+?)</wsdd:Scopes>").Groups[1].Value), @"qsc://qsc.com/hardware/(.*)").Groups[1].Value)))
        //                                {
        //                                    string camerapart = Regex.Match((Regex.Match(individualcompName, @"<wsdd:Scopes>(.+?)</wsdd:Scopes>").Groups[1].Value), @"qsc://qsc.com/hardware/(.*)").Groups[1].Value;
        //                                    string cameraname = Regex.Match((Regex.Match(individualcompName, @"<wsdd:Scopes>(.+?)</wsdd:Scopes>").Groups[1].Value), @"qsc://qsc.com/name/(.+?)qsc://qsc.com/hardware/").Groups[1].Value.Trim();
        //                                    string cameralan_a_ip = Regex.Match((Regex.Match(individualcompName, @"<wsdd:XAddrs>(.+?)</wsdd:XAddrs>").Groups[1].Value), @"http://(.+?):").Groups[1].Value;
        //                                    string cameralan_b_ip = string.Empty;


        //                                    if (!deviceList_St1.Contains(camerapart + "," + cameraname + "," + cameralan_a_ip + "," + cameralan_b_ip))
        //                                    {
        //                                        deviceList_St1.Add(camerapart + "," + cameraname + "," + cameralan_a_ip + "," + cameralan_b_ip);
        //                                    }

        //                                    //if (!availableDeviceListCopy.Contains(camerapart + "," + cameraname + "," + cameralan_a_ip + "," + cameralan_b_ip))
        //                                    //{
        //                                    //    availableDeviceListCopy.Add(camerapart + "," + cameraname + "," + cameralan_a_ip + "," + cameralan_b_ip);
        //                                    //}

        //                                    //write.WriteLine(camerapart + " ," + cameraname + " ," + cameralan_a_ip + " ," + cameralan_b_ip + " ," + string.Empty + " ," + string.Empty);
        //                                }

        //                            }
        //                        }
        //                    }

        //                    var ss = deviceList_St2.ToArray().ToList().Except(deviceList_St1.ToArray().ToList()).ToArray().Where(x => x.ToString().ToUpper().StartsWith("CORE"));
        //                    foreach (string v1 in ss)
        //                    {
        //                        string[] sss = v1.Split(',');

        //                        if (sss[2] != null && sss[2] != string.Empty)
        //                        {
        //                            deviceList_St3 = getcoredevices(sss[2], deviceList_St3);
        //                        }

        //                        if (sss[3] != null && sss[3] != string.Empty)
        //                        {
        //                            deviceList_St3 = getcoredevices(sss[3], deviceList_St3);
        //                        }
        //                    }

        //                    total = new ArrayList(deviceList_St1.ToArray().ToList().Union(deviceList_St2.ToArray().ToList()).Union(deviceList_St3.ToArray().ToList()).ToList());
        //                    availableDeviceListCopy = new ArrayList(total);
        //                    //}

        //                    //using (StreamWriter write = new StreamWriter(@"C:\Pheripherals.txt"))
        //                    //{
        //                    //    foreach (var item in total)
        //                    //    {
        //                    //        //foreach (var val in item.Value)
        //                    //        //{
        //                    //            write.WriteLine(item);
        //                    //        //}
        //                    //    }
        //                    //}

        //                    lock (lockAvailableDeviceList)
        //                    {
        //                        availableDeviceList = new ArrayList(total);
        //                    }

        //                    //WriteToLogFile("No. of devices found: " + availableDeviceList.Count);

        //                }
        //            }
        //            catch (Exception ex)
        //            {
        //                WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
        //#if DEBUG
        //                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
        //#endif
        //            }
        //        }

        public static ArrayList getcoredevices(string IP, ArrayList devicelst)
        {
            try
            {
                if (IP != null && IP != string.Empty)
                {
                    XmlDocument xml = new XmlDocument();
                    xml = XmlLoadUsingHttp("http://" + IP + "//cgi-bin//discovered_devices_xml", string.Empty);
                    if(xml==null)
                    {
                        xml = XmlLoadUsingHttp("http://" + IP + "/cgi-bin/discovered_devices_xml", string.Empty);
                    }
					
                    if (xml != null)
                    {
                        foreach (XmlNode node in xml.SelectNodes("QDP/device"))
                        {
                            if (node != null && node.InnerText != null)
                            {

                                string Checkemulator = node.SelectSingleNode("type").InnerText;
                                if (Checkemulator.ToUpper() != "EMULATOR")
                                {
                                    string name = node.SelectSingleNode("name").InnerText;
                                    string type = node.SelectSingleNode("part_number").InnerText;
                                    string lan_a_ip = node.SelectSingleNode("lan_a_ip").InnerText;
                                    string lan_b_ip = node.SelectSingleNode("lan_b_ip").InnerText;
                                    string aux_a_ip = node.SelectSingleNode("aux_a_ip").InnerText;
                                    string aux_b_ip = node.SelectSingleNode("aux_b_ip").InnerText;

                                    if(!devicelst.Contains(type + "," + name + "," + lan_a_ip + "," + lan_b_ip))
                                        devicelst.Add(type + "," + name + "," + lan_a_ip + "," + lan_b_ip);

                                    //if (!NetworkList.ContainsKey(type))
                                    //{
                                    //    NetworkList.Add(type, new List<string> { name + " ," + lan_a_ip + " ," + lan_b_ip + " ," + aux_a_ip + " ," + aux_b_ip });
                                    //}
                                    //else
                                    //{
                                    //    if (!NetworkList[type].Contains(name + " ," + lan_a_ip + " ," + lan_b_ip + " ," + aux_a_ip + " ," + aux_b_ip))
                                    //        NetworkList[type].Add(name + " ," + lan_a_ip + " ," + lan_b_ip + " ," + aux_a_ip + " ," + aux_b_ip);
                                    //}
                                }
                                //else
                                //{
                                //    string type = string.Empty;
                                //    string lan_a_ip = node.SelectSingleNode("lan_a_ip").InnerText;
                                //    string lan_b_ip = node.SelectSingleNode("lan_b_ip").InnerText;
                                //    string aux_a_ip = string.Empty;
                                //    string aux_b_ip = string.Empty;
                                //    if (!NetworkList.ContainsKey(type))
                                //    {
                                //        NetworkList.Add(type, new List<string> { name + " ," + lan_a_ip + " ," + lan_b_ip + " ," + aux_a_ip + " ," + aux_b_ip });
                                //    }
                                //    else
                                //    {
                                //        if (!NetworkList[type].Contains(name + " ," + lan_a_ip + " ," + lan_b_ip + " ," + aux_a_ip + " ," + aux_b_ip))
                                //            NetworkList[type].Add(name + " ," + lan_a_ip + " ," + lan_b_ip + " ," + aux_a_ip + " ," + aux_b_ip);

                                //    }
                                //}
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }

            return devicelst;
        }

        public static void AddEvent(discoveryDelegate CompleteEvent)
        {
            lock (lockDeviceDiscoveryThread)
            {
                completeEventList.Add(CompleteEvent);
            }
        }

        public static void RemoveEvent(discoveryDelegate CompleteEvent)
        {
            try
            {
                lock (lockDeviceDiscoveryThread)
                {
                    if (completeEventList.Contains(CompleteEvent))
                        completeEventList.Remove(CompleteEvent);

                    //if (completeEventList.Count == 0 && discoveryThead != null)
                    //    discoveryThead.Abort();
                }
            }
            catch (Exception ex)
            {
                WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif

            }

        }

        public static ArrayList AvailableDeviceList
        {

            get
            {
                lock (lockAvailableDeviceList)
                {          
                    return (ArrayList)availableDeviceList.Clone();
                }
            }
        }

        public static string QREM_Token
        {
            get
            {
                lock (lockAvailableDeviceList)
                {
                    return QREMtoken;
                }
            }
        }

        //public static Array AvailableDeviceList_Script
        //{
        //    get
        //    {              
        //        return AvailableDeviceList_Script;
        //    }
        //    set
        //    {
        //         AvailableDeviceList_Script = value;
        //    }
            
        //}

        public static bool IsDeviceDiscoveryComplete
        {
            get { return isDeviceDiscoveryComplete; }
        }

        private static List<string> GetLocalIPAddress()
        {
            List<string> totalIP = new List<string>();

            try
            {
                var host = Dns.GetHostEntry(Dns.GetHostName());
                foreach (var ip in host.AddressList)
                {
                    if (ip.AddressFamily == AddressFamily.InterNetwork)
                    {
                        totalIP.Add(ip.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
            }

            //throw new Exception("Local IP Address Not Found!");
            return totalIP;
        }

        private static void UdpPost(int port, string strIP, string strRemoteIP, string strParameters)
        {
            List<string> strResponse = new List<string>();
            UdpClient UDP_Qsys = new UdpClient();
            Dictionary<string, string> strResponse_dict = new Dictionary<string, string>();

            try
            {
                string local = IPAddress.IPv6Any.ToString();
                IPAddress IPAdd = IPAddress.Parse(strIP);
                IPAddress IPRemote = IPAddress.Parse(strRemoteIP);
                IPEndPoint RemoteIPEndpoint = new IPEndPoint(IPRemote, port);

                //UDP_Qsys = new System.Net.Sockets.UdpClient(RemoteIPEndpoint);
                UDP_Qsys.ExclusiveAddressUse = false;
                UDP_Qsys.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                UDP_Qsys.ExclusiveAddressUse = false;
                UDP_Qsys.Client.Bind(RemoteIPEndpoint);
                //UDP_Qsys.Ttl = 100;
                //UDP_Qsys.JoinMulticastGroup(IPAdd);
                UDP_Qsys.JoinMulticastGroup(IPAdd, 10);
                byte[] txBytes = System.Text.Encoding.UTF8.GetBytes(strParameters);

                UDP_Qsys.Send(txBytes, txBytes.Length, new IPEndPoint(IPAdd, port));

#if DEBUG_DEVICE_LOG
                WriteToLogFile("Task goes to sleep for IP: " + strRemoteIP + " Port: " + port + " Delay Time: "+ udpDelayTime);
#endif
                UDP_Qsys.Client.ReceiveBufferSize = 8096 * 10;

                Thread.Sleep(udpDelayTime);

                while (!(UDP_Qsys.Available == 0))
                {
                    Byte[] respBytes = UDP_Qsys.Receive(ref RemoteIPEndpoint);
                    string temp = Encoding.ASCII.GetString(respBytes);
                    string part_number = Regex.Match(temp, @"<part_number>(.+?)</part_number>").Groups[1].Value;
                    string name = Regex.Match(temp, @"<name>(.+?)</name>").Groups[1].Value.Trim();
                    string lan_a_ip = Regex.Match(temp, @"<lan_a_ip>(.+?)</lan_a_ip>").Groups[1].Value;
                    string lan_b_ip = Regex.Match(temp, @"<lan_b_ip>(.+?)</lan_b_ip>").Groups[1].Value;
                    if (part_number != string.Empty)
                    {
                        if (!strResponse_dict.ContainsKey((part_number + "," + name + "," + lan_a_ip + "," + lan_b_ip)))
                            strResponse_dict.Add((part_number + "," + name + "," + lan_a_ip + "," + lan_b_ip), temp);
                    }
                    else if (!(string.IsNullOrEmpty(Regex.Match((Regex.Match(temp, @"<wsdd:Scopes>(.+?)</wsdd:Scopes>").Groups[1].Value), @"qsc://qsc.com/hardware/(.*)").Groups[1].Value)))
                    {
                        string camerapart = Regex.Match((Regex.Match(temp, @"<wsdd:Scopes>(.+?)</wsdd:Scopes>").Groups[1].Value), @"qsc://qsc.com/hardware/(.*)").Groups[1].Value;
                        string cameraname = Regex.Match((Regex.Match(temp, @"<wsdd:Scopes>(.+?)</wsdd:Scopes>").Groups[1].Value), @"qsc://qsc.com/name/(.+?)qsc://qsc.com/hardware/").Groups[1].Value.Trim();
                        string cameralan_a_ip = Regex.Match((Regex.Match(temp, @"<wsdd:XAddrs>(.+?)</wsdd:XAddrs>").Groups[1].Value), @"http://(.+?):").Groups[1].Value;
                        string cameralan_b_ip = string.Empty;
                        if (!strResponse_dict.ContainsKey(camerapart + "," + cameraname + "," + cameralan_a_ip + "," + cameralan_b_ip))
                            strResponse_dict.Add((camerapart + "," + cameraname + "," + cameralan_a_ip + "," + cameralan_b_ip), temp);
                    }

                    //strResponse.Add(Encoding.ASCII.GetString(respBytes));

                    Thread.Sleep(10);
                }

                UDP_Qsys.Close();

#if DEBUG_DEVICE_LOG
                WriteToLogFile("Task wait for lock for IP: " + strRemoteIP + " Port: " + port);
#endif

                lock (lockDeviceDiscoveryTask)
                {
                    if (strResponse_dict != null && (strResponse_dict.Count > 0))
                        allComponentList.Add(strResponse_dict.Values.ToList());

                    //if (strResponse != null && (strResponse.Count > 0))
                    //    allComponentList.Add(strResponse);
                }

#if DEBUG_DEVICE_LOG
                WriteToLogFile("Task Completed for IP: " + strRemoteIP + " Port: " + port);
#endif
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
                WriteToLogFile("IP: " + strRemoteIP + " Port: " + port + ". ");
#if DEBUG_MESSAGEBOX
                if (!(ex.Message == "Thread was being aborted." || ex.Message == "Only one usage of each socket address (protocol/network address/port) is normally permitted"))
                    DeviceDiscovery.WriteToLogFile("Exception:QAT Error Code - EC03005 " + ex.Message);
                //MessageBox.Show("Exception\n " + ex.Message, "QAT Error Code - EC03005", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
            if(UDP_Qsys!=null)
                UDP_Qsys.Close();
            }

        }

        public static XmlDocument XmlLoadUsingHttp(string deviceUrl,string functionName)
        {
            HttpWebRequest http = null;

            try
            {
                http = (HttpWebRequest)WebRequest.Create(deviceUrl);
                XmlDocument xml = new XmlDocument();
                http.Timeout = 10000;

                if (deviceUrl.Contains("/cgi-bin/status_xml"))
                {
                    
                    string strAuth = Convert.ToBase64String(Encoding.Default.GetBytes(Properties.Settings.Default.DeviceUsername.ToString() + ":" + "devicelock"));
                    http.Headers["Authorization"] = "Basic " + strAuth;
                }

                using (HttpWebResponse response = http.GetResponse() as HttpWebResponse)
                {
                    using (Stream responseStream = response.GetResponseStream())
                    {
                        XmlTextReader reader = new XmlTextReader(responseStream);
                        xml.Load(reader);
                    }
                    //response.Close();
                    http.Abort();
                    return xml;
                }

            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG_MESSAGEBOX
                if (!String.Equals(ex.Message, "The operation has timed out") && !String.Equals(ex.Message, "Unable to connect to the remote server"))
                    MessageBox.Show("Exception\n " + ex.Message, "QAT Error Code - EC03006", MessageBoxButton.OK, MessageBoxImage.Error);
#endif

                http.Abort();
                return null;
            }            
        }

        public static void CreateLogFile()
        {
            try
            {
                logFilePath = Path.Combine(logFileDirectory.FullName, logFileName);

                if (!Directory.Exists(logFileDirectory.FullName))
                {
                    Directory.CreateDirectory(logFileDirectory.FullName);
                }

                if (!File.Exists(logFilePath))
                {
                    using (File.Create(logFilePath))
                        File.SetAttributes(logFilePath, FileAttributes.Normal);
                }

                logFileLineCount = File.ReadLines(logFilePath).Count();
            }
            catch (Exception ex)
            {
               // WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif

            }
        }

        public static void WriteToLogFile(string logMessage)
        {
            try { }
            catch (Exception ex) { }
            finally
            {
                try
                {
                    if (logFilePath == null)
                        CreateLogFile();
                    lock (lockLogFileWrite)
                    {
                        if (logFileLineCount > 4000)
                        {
                            File.WriteAllLines(logFilePath, File.ReadAllLines(logFilePath).Skip(2000));
                            logFileLineCount = File.ReadLines(logFilePath).Count();
                        }

                        using (FileStream fs = new FileStream(logFilePath, FileMode.Append, FileAccess.Write))
                        using (StreamWriter sw = new StreamWriter(fs))
                        {
                            sw.WriteLine(DateTime.Now + " :: " + logMessage);
                            logFileLineCount++;
                        }
                    }
                }
                catch (Exception ex)
                {
                    //WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                }
            }
        }

        public static void getnetpair()
        {
            try
            {
                DBConnection dbConnect = new DBConnection();
                DataTable tbl1 = new DataTable();
                DataTableReader read1 = null;
                string query = "Select * from Netpairlist";

                SqlCommand sqlCmd1 = new SqlCommand(query, dbConnect.CreateConnection());
                SqlDataAdapter sqlDa1 = new SqlDataAdapter(sqlCmd1);
                sqlDa1.Fill(tbl1);
                read1 = tbl1.CreateDataReader();
                if (read1.HasRows)
                {
                    while (read1.Read())
                    {
                        Netpair_devicesSupported = read1.GetString(0);
                    }
                }
                else
                {
                    Netpair_devicesSupported = Properties.Settings.Default.Netpairlist;
                }
            }
            catch (Exception ex)
            {
                WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
            }
        }

        // Update the NetPairing List
        public static void UpdateNetPairingList(List<DUT_DeviceItem> selectedDutDeviceItemList)
        {
            try
            {
                List<string> netPairinglName = new List<string>();
                List<string> netPairingType = new List<string>();
                List<string> netPairinglPrimaryIP = new List<string>();
                List<string> netPairingSecondaryIP = new List<string>();
				
				//////Reflect core details (coreid, corename, systemid and siteid) are added in this list
                List<string> netPairingQREMcheck = new List<string>();

                ArrayList availableDeviceList = DeviceDiscovery.AvailableDeviceList;

                foreach (string netPairingComponent in availableDeviceList)
                {
                    string[] splitComponent = netPairingComponent.Split(',');

                    netPairingType.Add(splitComponent[0]);
                    netPairinglName.Add(splitComponent[1]);

                    if(splitComponent.Count() > 2 && !string.IsNullOrEmpty(splitComponent[2]))
                        netPairinglPrimaryIP.Add(splitComponent[2]);
                    else
                        netPairinglPrimaryIP.Add(string.Empty);

                    if (splitComponent.Count() > 3 && !string.IsNullOrEmpty(splitComponent[3]))
                        netPairingSecondaryIP.Add(splitComponent[3]);
                    else
                        netPairingSecondaryIP.Add(string.Empty);

                    if (splitComponent.Count() > 4 && !string.IsNullOrEmpty(splitComponent[4]))
                        netPairingQREMcheck.Add(splitComponent[4]);
                    else
                        netPairingQREMcheck.Add(string.Empty);                    
                }

                foreach (DUT_DeviceItem item in selectedDutDeviceItemList)
                {
                    foreach (var deviceName in new Dictionary<string,string>(item.ItemNetPairingList))
                    {
                        if (!netPairinglName.Contains(deviceName.Key) & deviceName.Key != "Not Applicable" && deviceName.Key != "None")
                        {
                            item.ItemNetPairingList.Remove(deviceName.Key);
                            item.ItemNetPairingList_duplicate.Remove(deviceName.Key);
                            //item.ItemNetPairingListWithQREM.Remove(deviceName);
                            item.ItemNetPairingListForXAML.Remove(deviceName.Key);
                        }
                    }

                    for (int i = 0; i < netPairingType.Count; i++)
                    {
                        // Remove G or H from Page Station in the Device Model read from Design
                        string deviceModel = string.Empty;
                        if ((item.ItemDeviceModel.StartsWith("PS")))
                        {
                            deviceModel = item.ItemDeviceModel.Remove(item.ItemDeviceModel.Length - 1);
                        }
                        else if ((item.ItemDeviceModel.StartsWith("TSC-7")))
                        {
                            deviceModel = item.ItemDeviceModel;
                            int index = deviceModel.IndexOf("7");
                            if (index > 0)
                                deviceModel = deviceModel.Substring(0, index + 1);
                            //deviceModel = item.ItemDeviceModel.Remove(item.ItemDeviceModel.Length - 1);
                        }
                        else
                        {
                            deviceModel = item.ItemDeviceModel;
                        }

                        // Remove G or H from Page Station in the Device Model read from Network Device Discovery
                        string netPairingModel = string.Empty;
                        if ((netPairingType[i].StartsWith("PS")))
                        {
                            netPairingModel = netPairingType[i].Remove(netPairingType[i].Length - 1);
                        }
                        else if ((netPairingType[i].StartsWith("TSC-7")))
                        {
                            netPairingModel = netPairingType[i];
                            int index = netPairingModel.IndexOf("7");
                            if (index > 0)
                                netPairingModel = netPairingModel.Substring(0, index + 1);
                            //netPairingModel = netPairingType[i].Remove(netPairingType[i].Length - 1);
                        }
                        else
                        {
                            netPairingModel = netPairingType[i];
                        }
                        if (String.Equals(deviceModel, netPairingModel))
                        {
                            if (item.ItemNetPairingList.Keys.ToList().Contains(netPairinglName[i]))
                            {
                                int index = item.ItemNetPairingList.Keys.ToList().IndexOf(netPairinglName[i]);
                                if (index >= 0)
                                {
                                    item.ItemPrimaryIPList[index] = netPairinglPrimaryIP[i];
                                    item.ItemSecondaryIPList[index] = netPairingSecondaryIP[i];
                                    if (String.Equals(item.ItemNetPairingSelected, netPairinglName[i], StringComparison.CurrentCultureIgnoreCase))
                                    {
                                        item.ItemPrimaryIPSelected = netPairinglPrimaryIP[i];
                                        item.ItemSecondaryIPSelected = netPairingSecondaryIP[i];
                                    }
                                    
                                    item.ItemNetPairingList[netPairinglName[i]] = netPairingQREMcheck[i];
                                    item.ItemNetPairingList_duplicate[netPairinglName[i]] = netPairingQREMcheck[i];
                                }
                            }
                            else
                            {
                                string netPairingSelected = item.ItemNetPairingSelected;
                                string primaryIPSelected = item.ItemPrimaryIPSelected;
                                string secondaryIPSelected = item.ItemSecondaryIPSelected;

                                item.ItemNetPairingList.Add(netPairinglName[i], netPairingQREMcheck[i]);

                                if(!item.ItemNetPairingList_duplicate.ContainsKey(netPairinglName[i]))
                                    item.ItemNetPairingList_duplicate.Add(netPairinglName[i], netPairingQREMcheck[i]);

                                if(!item.ItemNetPairingListForXAML.Contains(netPairinglName[i]))
                                    item.ItemNetPairingListForXAML.Add(netPairinglName[i]);

                                item.ItemPrimaryIPList.Add(netPairinglPrimaryIP[i]);
                                item.ItemSecondaryIPList.Add(netPairingSecondaryIP[i]);
                              

                                item.ItemPrimaryIPSelected = primaryIPSelected;
                                item.ItemSecondaryIPSelected = secondaryIPSelected;
                                item.ItemNetPairingSelected = netPairingSelected;
                            }
                        }
                    }
                }

                foreach (DUT_DeviceItem item in selectedDutDeviceItemList)
                {
                    if (!item.ItemNetPairingList.Keys.ToList().Contains(item.ItemNetPairingSelected))
                    {
                        item.ItemNetPairingSelected = null;
                        item.ItemCurrentBuild = null;
                        item.ItemCurrentDesign = null;
                    }

                    // Priority logic for backup core 

                    Dictionary<string, DUT_DeviceItem> FinalDoubleCoreList = new Dictionary<string, DUT_DeviceItem>();

                    var Check = DeviceDiscovery.DoubleCoreList.Values.Where(x => x.ItemDeviceName.Equals(item.ItemDeviceName, StringComparison.CurrentCultureIgnoreCase) 
					&& x.ItemDeviceModel.Equals(item.ItemDeviceModel, StringComparison.CurrentCultureIgnoreCase) && x.ItemDeviceType.Equals(item.ItemDeviceType, StringComparison.CurrentCultureIgnoreCase) 
					 && x.Itemlinked != string.Empty &&  x.Itemlinked != null && x.ParentTestPlanTreeView.IsChecked == true).ToList();
                    if (Check.Count == 0)
                    {
                        Check = DeviceDiscovery.DoubleCoreList.Values.Where(x => x.ItemDeviceName.Equals(item.ItemDeviceName, StringComparison.CurrentCultureIgnoreCase) 
						&& x.ItemDeviceModel.Equals(item.ItemDeviceModel, StringComparison.CurrentCultureIgnoreCase) && x.ItemDeviceType.Equals(item.ItemDeviceType, StringComparison.CurrentCultureIgnoreCase) 
						&& x.Itemlinked != string.Empty && x.Itemlinked!=null).ToList();
                    }
                    if (Check.Count() > 0)
                    {
                        if (!FinalDoubleCoreList.ContainsKey(item.ItemDeviceName + item.ItemDeviceModel + item.ItemDeviceType))
                        {
                            FinalDoubleCoreList.Add(item.ItemDeviceName + item.ItemDeviceModel + item.ItemDeviceType, Check[0]);
                            item.Itemlinked = Check[0].Itemlinked;
                            item.ItemPrimaryorBackup = Check[0].ItemPrimaryorBackup;
                        }
                    }
                    else
                    {
                        item.Itemlinked = string.Empty;
                        item.ItemPrimaryorBackup = "primary";
                    }
                }

                foreach (DUT_DeviceItem item in selectedDutDeviceItemList)
                {
                    if (item.ItemNetPairingSelected == null && item.ItemNetPairingList.Keys.ToList().Contains(item.ItemDeviceName, StringComparer.CurrentCultureIgnoreCase))
                    {
                        List<string> netPairingList = item.ItemNetPairingList.Keys.ToList();
                        netPairingList.Remove("Not Applicable");
                        if (AllowConfigure(item,selectedDutDeviceItemList))
                        {
                            int index = netPairingList.FindIndex(x => String.Equals(x, item.ItemDeviceName, StringComparison.CurrentCultureIgnoreCase));
                            if (index >= 0)
                            {
                                item.ItemNetPairingSelected = netPairingList[index];
                                item.ItemPrimaryIPSelected = item.ItemPrimaryIPList[index];
                                item.ItemSecondaryIPSelected = item.ItemSecondaryIPList[index];
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
            }
        }
		
		
		  public static bool AllowConfigure(DUT_DeviceItem item,List<DUT_DeviceItem> S)
        {
            try
            {
                if (item.Itemlinked != string.Empty && item.ItemPrimaryorBackup == "primary")
                {
                    var GetBackup = S.Where(x => x.ItemDeviceName == item.Itemlinked && x.ItemPrimaryorBackup == "backup").ToList();
                    if (GetBackup.Count > 0)
                    {
                        if (GetBackup[0].ItemNetPairingSelected == null || GetBackup[0].ItemDeviceName.Equals(GetBackup[0].ItemNetPairingSelected, StringComparison.CurrentCultureIgnoreCase) || GetBackup[0].ItemNetPairingSelected==string.Empty || GetBackup[0].ItemNetPairingSelected=="Not Applicable")
                        {
                            return true;
                        }
                    }
                }
                if (item.Itemlinked != string.Empty && item.ItemPrimaryorBackup == "backup")
                {
                    var GetPrimary = S.Where(x => x.ItemDeviceName == item.Itemlinked && x.ItemPrimaryorBackup == "primary").ToList();
                    if (GetPrimary.Count > 0)
                    {
                        if (GetPrimary[0].ItemNetPairingSelected == null || GetPrimary[0].ItemDeviceName.Equals(GetPrimary[0].ItemNetPairingSelected, StringComparison.CurrentCultureIgnoreCase) || GetPrimary[0].ItemNetPairingSelected == string.Empty || GetPrimary[0].ItemNetPairingSelected == "Not Applicable")
                        {
                            return true;
                        }
                    }
                }
                if ((item.Itemlinked == string.Empty && item.ItemPrimaryorBackup == "primary") || (item.Itemlinked==null && item.ItemPrimaryorBackup==null))
                {
                    return true;
                }
                return false;

            }
            catch (Exception ex)
            {
          
                WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                return false;
            }
        }

        //private static List<DUT_DeviceItem> ModifyDutList(List<DUT_DeviceItem> selectedDutDeviceItemList)
        //{
        //    try
        //    {
        //        Dictionary<string, DUT_DeviceItem> FinalDataContext = new Dictionary<string, DUT_DeviceItem>();

        //        foreach (DUT_DeviceItem d in selectedDutDeviceItemList)
        //        {
        //            var check = DeviceDiscovery.DoubleCoreList.Values.Where(x => x.ItemDeviceName.Equals(d.ItemDeviceName, StringComparison.CurrentCultureIgnoreCase) &&

        //             x.ItemDeviceModel.Equals(d.ItemDeviceModel, StringComparison.CurrentCultureIgnoreCase) &&

        //             x.ItemDeviceType.Equals(d.ItemDeviceType, StringComparison.CurrentCultureIgnoreCase) && x.Itemlinked != string.Empty && x.ParentTestPlanTreeView.IsChecked == true).ToList();
        //            if (check.Count == 0)
        //            {
        //                check = DeviceDiscovery.DoubleCoreList.Values.Where(x => x.ItemDeviceName.Equals(d.ItemDeviceName, StringComparison.CurrentCultureIgnoreCase) &&

        //                                   x.ItemDeviceModel.Equals(d.ItemDeviceModel, StringComparison.CurrentCultureIgnoreCase) &&

        //                                   x.ItemDeviceType.Equals(d.ItemDeviceType, StringComparison.CurrentCultureIgnoreCase) && x.Itemlinked != string.Empty).ToList();
        //            }
        //            if (check.Count() > 0)
        //            {
        //                if (!FinalDataContext.ContainsKey(d.ItemDeviceName + d.ItemDeviceModel + d.ItemDeviceType))
        //                    FinalDataContext.Add(d.ItemDeviceName + d.ItemDeviceModel + d.ItemDeviceType, check[0]);
        //            }
        //        }

        //        foreach (DUT_DeviceItem d in FinalDataContext.Values)
        //        {
        //            selectedDutDeviceItemList.Where(w => w.ItemDeviceName.Equals(d.ItemDeviceName, StringComparison.CurrentCultureIgnoreCase) && w.ItemDeviceModel.Equals(d.ItemDeviceModel, StringComparison.CurrentCultureIgnoreCase) && w.ItemDeviceType.Equals(d.ItemDeviceType, StringComparison.CurrentCultureIgnoreCase)).ToList().ForEach(u =>
        //            {
        //                u.Itemlinked = d.Itemlinked;
        //                u.ItemPrimaryorBackup = d.ItemPrimaryorBackup;
        //            });
        //        }
        //        return selectedDutDeviceItemList.ToList();
        //    }
        //    catch (Exception ex)
        //    {
        //        return selectedDutDeviceItemList;
        //    }
        //}

        private static List<Window> appWindowListValue = new List<Window>();
        public static List<Window> AppWindowList
        {
            get { return appWindowListValue; }
            set { appWindowListValue = value; }
        }

        private static List<Window> runnerWindowListValue = new List<Window>();
        private static List<Window> RunnerWindowList
        {
            get { return runnerWindowListValue; }
            set { runnerWindowListValue = value; }
        }

        private static Window designerWindowValue = null;
        public static Window DesignerWindow
        {
            get { return designerWindowValue; }
            set { designerWindowValue = value; }
        }

        private static Window importWindowValue = null;
        public static Window ImportWindow
        {
            get { return importWindowValue; }
            set { importWindowValue = value; }
        }

        private static Window exportWindowValue = null;
        public static Window ExportWindow
        {
            get { return exportWindowValue; }
            set { exportWindowValue = value; }
        }

        private static Window preferenceWindowValue = null;
        private static Window PreferenceWindow
        {
            get { return preferenceWindowValue; }
            set { preferenceWindowValue = value; }
        }

        private static Window reportWindowValue = null;
        private static Window ReportWindow
        {
            get { return reportWindowValue; }
            set { reportWindowValue = value; }
        }


        private static Window overviewWindowValue = null;
        private static Window OverviewWindow
        {
            get { return overviewWindowValue; }
            set { overviewWindowValue = value; }
        }

        private static Window shortcutWindowValue = null;
        private static Window ShortcutWindow
        {
            get { return shortcutWindowValue; }
            set { shortcutWindowValue = value; }
        }


        public static void CreateRunnerWindow(bool loadConfigFile)
        {
            try
            {
                Thread runnerThead = new Thread(() => RunnerWindowThread(loadConfigFile));
                runnerThead.SetApartmentState(ApartmentState.STA);
                runnerThead.IsBackground = true;
                runnerThead.Start();
            }
            catch (Exception ex)
            {
                WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
            }
        }

        private static void RunnerWindowThread(bool loadConfigFile)
        {
            try
            {
                lock (lockWindowCreation)
                {
                    Test_Execution runnerWindow = new Test_Execution(loadConfigFile);
                    runnerWindow.Closed += RunnerWindow_Closed;
                    runnerWindow.Show();
                    RunnerWindowList.Add(runnerWindow);
                    AppWindowList.Add(runnerWindow);
                }
                Dispatcher.Run();
            }
            catch (Exception ex)
            {
                WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
            }
        }

        public static bool IsExecutionRunning()
        {
            try
            {
                Test_Execution execution = null;
                foreach (Test_Execution ts in RunnerWindowList)
                {
                    execution = ts as Test_Execution;
                    if (execution.executionThread != null)
                        return false;
                            }
                return true;
            }
            catch (Exception ex)
            {
                WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                return false;
            }
        }

        public static bool IsExportRunning()
        {
            try
            {
                Export Exp = ExportWindow as Export;
                if (Exp != null)
                {
                    if (Exp.StartExport != null)
                        return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                return true;
                WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
            }
        }

        public static bool IsImportRunning()
        {
            try
            {
                Import Imp = ImportWindow as Import;
                if (Imp != null)
                {
                    if (Imp.StartImport != null)
                        return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                return true;
                WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
            }
        }

        private static void RunnerWindow_Closed(object sender, EventArgs e)
        {
            try
            {
                Dispatcher.CurrentDispatcher.BeginInvokeShutdown(DispatcherPriority.Background);
                lock (lockWindowCreation)
                {
                    RunnerWindowList.Remove(sender as Window);
                    AppWindowList.Remove(sender as Window);
                    if (AppWindowList.Count() == 0)
                    {
                        if(discoveryThead != null)
                            discoveryThead.Abort();

                        DeviceDiscovery.startUpWindow.Dispatcher.Invoke((Action)(() =>
                        {
                            Application.Current.Shutdown();
                        }));
                    }
                }
            }
            catch (Exception ex)
            {
                WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
            }
        }

        public static void CreateDesignerWindow(bool Refresh,TreeViewExplorer sourceTreeViewExplorer = null)
        {
            try
            {
                Thread designerThead = null;

                lock (lockWindowCreation)
                {
                    if (DesignerWindow != null)
                    {
                       
                        DeviceDiscovery.DesignerWindow.Dispatcher.Invoke((Action)(() =>
                        {
                            Test_Designer designerWindow = DesignerWindow as Test_Designer;
                            if (Refresh && designerWindow!=null)
                                designerWindow.Refresh_ImportWindow(string.Empty);

                            if (sourceTreeViewExplorer != null)
                            {
                               
                                if (designerWindow != null)
                                    designerWindow.OpenTreeViewItem(sourceTreeViewExplorer, true);
                           

                            }
                            DesignerWindow.Show();
                            DesignerWindow.Activate();

                            if (DesignerWindow.WindowState == WindowState.Minimized)
                            {
                                DesignerWindow.WindowState = WindowState.Normal;
                            }
                        }));
                        //if (Refresh)
                        //{
                        //    DeviceDiscovery.DesignerWindow.Dispatcher.Invoke((Action)(() =>
                        //    {
                        //        designerWindow.SetupTreeViewDesignerFromDB(true, "No order");
                        //    }));
                        //}
                    }
                    else
                    {
                        designerThead = new Thread(() => DesignerWindowThread(sourceTreeViewExplorer));
                    }
                }

                if (designerThead != null)
                {
                    designerThead.SetApartmentState(ApartmentState.STA);
                    designerThead.IsBackground = true;
                    designerThead.Start();
                }
            }
            catch (Exception ex)
            {
                WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
            }
        }
                
        private static void DesignerWindowThread(TreeViewExplorer sourceTreeViewExplorer)
        {
            try
            {
                lock (lockWindowCreation)
                {
                    Test_Designer designerWindow = new Test_Designer();
                    if (sourceTreeViewExplorer != null)
                        designerWindow.OpenTreeViewItem(sourceTreeViewExplorer, true);

                    designerWindow.Closed += DesignerWindow_Closed;

                    designerWindow.Show();
                    designerWindow.Activate();

                    if (designerWindow.WindowState == WindowState.Minimized)
                    {
                        designerWindow.WindowState = WindowState.Normal;
                    }

                    DesignerWindow = designerWindow;
                    AppWindowList.Add(designerWindow);
                }
                Dispatcher.Run();
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
            }
        }

        private static void DesignerWindow_Closed(object sender, EventArgs e)
        {
            try
            {
                Dispatcher.CurrentDispatcher.BeginInvokeShutdown(DispatcherPriority.Background);
                lock (lockWindowCreation)
                {
                    DesignerWindow = null;
                    AppWindowList.Remove(sender as Window);
                    if (AppWindowList.Count() == 0)
                    {
                        if (discoveryThead != null)
                            discoveryThead.Abort();

                        DeviceDiscovery.startUpWindow.Dispatcher.Invoke((Action)(() =>
                        {
                            Application.Current.Shutdown();
                        }));
                    }
                }
            }
            catch (Exception ex)
            {
                WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
            }
        }

        public static void CreateOverviewWindow()
        {
            try
            {
                Thread overviewThead = null;

                lock (lockWindowCreation)
                {
                    if (OverviewWindow != null)
                    {
                        DeviceDiscovery.OverviewWindow.Dispatcher.Invoke((Action)(() =>
                        {
                            OverviewWindow.Show();
                            OverviewWindow.Activate();

                            if (OverviewWindow.WindowState == WindowState.Minimized)
                            {
                                OverviewWindow.WindowState = WindowState.Normal;
                            }
                        }));
                    }
                    else
                    {
                        overviewThead = new Thread(() => OverviewWindowThread());
                    }
                }

                if (overviewThead != null)
                {
                    overviewThead.SetApartmentState(ApartmentState.STA);
                    overviewThead.IsBackground = true;
                    overviewThead.Start();
                }
            }
            catch (Exception ex)
            {
                WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
            }
        }


        private static void OverviewWindowThread()
        {
            try
            {
                lock (lockWindowCreation)
                {
                    QAToverview overviewWindow = new QAToverview();

                    overviewWindow.Closed += overviewWindow_Closed;

                    overviewWindow.Show();
                    overviewWindow.Activate();

                    if (overviewWindow.WindowState == WindowState.Minimized)
                    {
                        overviewWindow.WindowState = WindowState.Normal;
                    }

                    OverviewWindow = overviewWindow;
                    AppWindowList.Add(overviewWindow);
                }

                Dispatcher.Run();
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
            }
        }

        private static void overviewWindow_Closed(object sender, EventArgs e)
        {
            try
            {
                Dispatcher.CurrentDispatcher.BeginInvokeShutdown(DispatcherPriority.Background);
                lock (lockWindowCreation)
                {
                    OverviewWindow = null;
                    AppWindowList.Remove(sender as Window);
                    if (AppWindowList.Count() == 0)
                    {
                        if (discoveryThead != null)
                            discoveryThead.Abort();

                        DeviceDiscovery.startUpWindow.Dispatcher.Invoke((Action)(() =>
                        {
                            Application.Current.Shutdown();
                        }));
                    }
                }
            }
            catch (Exception ex)
            {
                WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
            }
        }

        public static void CreateShortcutWindow()
        {
            try
            {
                Thread ShortcutThead = null;

                lock (lockWindowCreation)
                {
                    if (ShortcutWindow != null)
                    {
                        DeviceDiscovery.ShortcutWindow.Dispatcher.Invoke((Action)(() =>
                        {
                            ShortcutWindow.Show();
                            ShortcutWindow.Activate();

                            if (ShortcutWindow.WindowState == WindowState.Minimized)
                            {
                                ShortcutWindow.WindowState = WindowState.Normal;
                            }
                        }));
                    }
                    else
                    {
                        ShortcutThead = new Thread(() => ShortcutWindowThread());
                    }
                }

                if (ShortcutThead != null)
                {
                    ShortcutThead.SetApartmentState(ApartmentState.STA);
                    ShortcutThead.IsBackground = true;
                    ShortcutThead.Start();
                }
            }
            catch (Exception ex)
            {
                WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
            }
        }


        private static void ShortcutWindowThread()
        {
            try
            {
                lock (lockWindowCreation)
                {
                    shortcut_keys shortcutWindow = new shortcut_keys();

                    shortcutWindow.Closed += shortcutWindow_Closed;

                    shortcutWindow.Show();
                    shortcutWindow.Activate();

                    if (shortcutWindow.WindowState == WindowState.Minimized)
                    {
                        shortcutWindow.WindowState = WindowState.Normal;
                    }

                    ShortcutWindow = shortcutWindow;
                    AppWindowList.Add(shortcutWindow);
                }

                Dispatcher.Run();
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
            }
        }

        private static void shortcutWindow_Closed(object sender, EventArgs e)
        {
            try
            {
                Dispatcher.CurrentDispatcher.BeginInvokeShutdown(DispatcherPriority.Background);
                lock (lockWindowCreation)
                {
                    ShortcutWindow = null;
                    AppWindowList.Remove(sender as Window);
                    if (AppWindowList.Count() == 0)
                    {
                        if (discoveryThead != null)
                            discoveryThead.Abort();

                        DeviceDiscovery.startUpWindow.Dispatcher.Invoke((Action)(() =>
                        {
                            Application.Current.Shutdown();
                        }));
                    }
                }
            }
            catch (Exception ex)
            {
                WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
            }
        }
        public static void CreatePreferenceWindow()
        {
            try
            {
                Thread preferenceThead = null;

                lock (lockWindowCreation)
                {
                    if (PreferenceWindow != null)
                    {
                        DeviceDiscovery.PreferenceWindow.Dispatcher.Invoke((Action)(() =>
                        {
                            PreferenceWindow.Show();
                            PreferenceWindow.Activate();

                            if (PreferenceWindow.WindowState == WindowState.Minimized)
                            {
                                PreferenceWindow.WindowState = WindowState.Normal;
                            }
                        }));
                    }
                    else
                    {
                        preferenceThead = new Thread(() => PreferenceWindowThread());
                    }
                }

                if (preferenceThead != null)
                {
                    preferenceThead.SetApartmentState(ApartmentState.STA);
                    preferenceThead.IsBackground = true;
                    preferenceThead.Start();
                }
            }
            catch (Exception ex)
            {
                WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
            }
        }

        private static void PreferenceWindowThread()
        {
            try
            {
                lock (lockWindowCreation)
                {
                    ServerDetails preferenceWindow = new ServerDetails();

                    preferenceWindow.Closed += PrefernceWindow_Closed;

                    preferenceWindow.Show();
                    preferenceWindow.Activate();

                    if (preferenceWindow.WindowState == WindowState.Minimized)
                    {
                        preferenceWindow.WindowState = WindowState.Normal;
                    }

                    PreferenceWindow = preferenceWindow;
                    AppWindowList.Add(preferenceWindow);
                }

                Dispatcher.Run();
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
            }
        }

        private static void PrefernceWindow_Closed(object sender, EventArgs e)
        {
            try
            {
                Dispatcher.CurrentDispatcher.BeginInvokeShutdown(DispatcherPriority.Background);
                lock (lockWindowCreation)
                {
                    PreferenceWindow = null;
                    AppWindowList.Remove(sender as Window);
                    if (AppWindowList.Count() == 0)
                    {
                        if (discoveryThead != null)
                            discoveryThead.Abort();

                        DeviceDiscovery.startUpWindow.Dispatcher.Invoke((Action)(() =>
                        {
                            Application.Current.Shutdown();
                        }));
                    }
                }
            }
            catch (Exception ex)
            {
                WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
            }
        }

        public static int importwindowcount = 0;
        public static bool IsImportAlreadyExist = false;
        public static void CreateImportWindow()
        {
            try
            {
                if(importwindowcount!=1)
                {
                    Thread importThread = null;
                        IsImportAlreadyExist = true;

                    lock (lockWindowCreation)
                    {
                        if (ImportWindow != null)
                        {
                            DeviceDiscovery.ImportWindow.Dispatcher.Invoke((Action)(() =>
                            {
                                ImportWindow.Show();
                                ImportWindow.Activate();

                                if (ImportWindow.WindowState == WindowState.Minimized)
                                {
                                    ImportWindow.WindowState = WindowState.Normal;
                                }
                            }));
                        }
                        else
                        {
                            importThread = new Thread(() => ImportWindowThread());
                        }
                    }

                    if (importThread != null)
                    {
                        importThread.SetApartmentState(ApartmentState.STA);
                        importThread.IsBackground = true;
                        importThread.Start();
                    }
                        IsImportAlreadyExist = false;
                }
               
            }
            catch (Exception ex)
            {
                IsImportAlreadyExist = false;
                WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
            }
        }

        private static void ImportWindowThread()
        {
            try
            {
                lock (lockWindowCreation)
                {
                    Import importWindow = new Import();

                    importWindow.Closed += ImportWindow_Closed;

                    importWindow.Show();
                    importWindow.Activate();

                    if (importWindow.WindowState == WindowState.Minimized)
                    {
                        importWindow.WindowState = WindowState.Normal;
                    }

                    ImportWindow = importWindow;
                    AppWindowList.Add(importWindow);
                }

                Dispatcher.Run();
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
            }
        }

        private static void ImportWindow_Closed(object sender, EventArgs e)
        {
            try
            {
                Dispatcher.CurrentDispatcher.BeginInvokeShutdown(DispatcherPriority.Background);
                lock (lockWindowCreation)
                {
                    ImportWindow = null;
                    AppWindowList.Remove(sender as Window);
                    if (AppWindowList.Count() == 0)
                    {
                        if (discoveryThead != null)
                            discoveryThead.Abort();

                        DeviceDiscovery.startUpWindow.Dispatcher.Invoke((Action)(() =>
                        {
                            Application.Current.Shutdown();
                        }));
                    }
                    IsImportAlreadyExist = false;
                }
            }
            catch (Exception ex)
            {
                WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
            }
        }

        public static void CreateExportWindow()
        {
            try
            {
                Thread exportThread = null;

                lock (lockWindowCreation)
                {
                    if (ExportWindow != null)
                    {
                        DeviceDiscovery.ExportWindow.Dispatcher.Invoke((Action)(() =>
                        {
                            ExportWindow.Show();
                            ExportWindow.Activate();

                            if (ExportWindow.WindowState == WindowState.Minimized)
                            {
                                ExportWindow.WindowState = WindowState.Normal;
                            }
                        }));
                    }
                    else
                    {
                        exportThread = new Thread(() => ExportWindowThread());
                    }
                }

                if (exportThread != null)
                {
                    exportThread.SetApartmentState(ApartmentState.STA);
                    exportThread.IsBackground = true;
                    exportThread.Start();
                }
            }
            catch (Exception ex)
            {
                WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
            }
        }

        private static void ExportWindowThread()
        {
            try
            {
                lock (lockWindowCreation)
                {
                    Export exportWindow = new Export();

                    exportWindow.Closed += ExportWindow_Closed;

                    exportWindow.Show();
                    exportWindow.Activate();

                    if (exportWindow.WindowState == WindowState.Minimized)
                    {
                        exportWindow.WindowState = WindowState.Normal;
                    }

                    ExportWindow = exportWindow;
                    AppWindowList.Add(exportWindow);
                }

                Dispatcher.Run();
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
            }
        }

        private static void ExportWindow_Closed(object sender, EventArgs e)
        {
            try
            {
                Dispatcher.CurrentDispatcher.BeginInvokeShutdown(DispatcherPriority.Background);
                lock (lockWindowCreation)
                {
                    ExportWindow = null;
                    AppWindowList.Remove(sender as Window);
                    if (AppWindowList.Count() == 0)
                    {
                        if (discoveryThead != null)
                            discoveryThead.Abort();

                        DeviceDiscovery.startUpWindow.Dispatcher.Invoke((Action)(() =>
                        {
                            Application.Current.Shutdown();
                        }));
                    }
                }
            }
            catch (Exception ex)
            {
                WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
            }
        }
        public static string GetAlldeviceNameForSelectedIP(string deviceIP, List<DUT_DeviceItem> selectedDutDeviceItemList)
        {
            try
            {
                string temp = string.Empty;
                bool notavailable = false;

                foreach (DUT_DeviceItem item in selectedDutDeviceItemList)
                {
                    if ((item.ItemPrimaryIPSelected != null && item.ItemPrimaryIPSelected.Equals(deviceIP)) || (item.ItemSecondaryIPSelected != null && item.ItemSecondaryIPSelected.Equals(deviceIP)))
                    {
                        temp = item.ItemNetPairingSelected;
                        notavailable = true;
                        return temp;
                    }
                }

                if (!notavailable)
                    temp = "XXXXX";

                return temp;
            }
            catch (Exception ex)
            {
                //if (ex.Message != "Thread was being aborted.")
                //    MessageBox.Show("Exception\n \n" + ex.Message, "QAT Error Code - ECxxxx", MessageBoxButton.OK, MessageBoxImage.Error);
                WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                return null;
            }
        }

        public static void Update_QRCMVersionList()
        {
            DBConnection connection = new DBConnection();
            try
            {
                string query = "SELECT DISTINCT Build_version FROM QRCMInitialization";
                DataTable dataTable = new DataTable();
                System.Data.SqlClient.SqlDataAdapter dataAdapter = new System.Data.SqlClient.SqlDataAdapter(query, connection.CreateConnection());
                connection.OpenConnection();
                dataAdapter.Fill(dataTable);
                DataTableReader dataTableReader = dataTable.CreateDataReader();
                //List<string> versionListcopy = new List<string>();
                if (dataTableReader.HasRows)
                {
                    while (dataTableReader.Read())
                    {
                        if (dataTableReader[0] != System.DBNull.Value)
                        {
                            if (!QatConstants.QRCMversionList.Contains(dataTableReader.GetString(0).Trim()))
                            {
                                if (DeviceDiscovery.DesignerWindow != null)
                                {
                                    DeviceDiscovery.DesignerWindow.Dispatcher.Invoke((Action)(() =>
                                    {
                                        QatConstants.QRCMversionList.Add(dataTableReader.GetString(0).ToString().Trim());
                                    }));
                                }
                                else
                                {
                                    QatConstants.QRCMversionList.Add(dataTableReader.GetString(0).ToString().Trim());
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

            connection.CloseConnection();

        }

    }
}
