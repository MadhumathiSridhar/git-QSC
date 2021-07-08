using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Windows;
using System.Xml;

namespace QSC_Test_Automation
{
    class logs
    {
        //private TableEntity tbl = new TableEntity();
        private DBConnection connect = new DBConnection();    
        string username = "admin";

        public void HttpGetactual(string strURI,bool isnewapi, string m_password, string deviceIP, string device_username, out string strResponse, ExecutionProcess execProcess)
        {
            System.Net.HttpWebRequest req = (HttpWebRequest)System.Net.WebRequest.Create(strURI);
            try
            {
			   // Need to setup Authentication Header.
                if(isnewapi == false)
                {
                    req.ContentType = "application/x-www-form-urlencoded";
                    if (device_username != string.Empty)
                        SetBasicAuthHeader(ref req, device_username, m_password);                  
                } 
                else if(isnewapi && execProcess !=null)
                {
                    req.Headers["Authorization"] = "Bearer " + execProcess.CoreLogonToken;
                    req.ContentType = "application/json";
                }
                                          
                req.Method = "Get";
                req.Timeout = 15000;
                req.ReadWriteTimeout = 5000;          
            
                using (System.Net.WebResponse resp = req.GetResponse())
                {
                    if (resp == null)
                    {
                        strResponse = "";
                        //return false;
                    }

                    using (System.IO.StreamReader sr = new System.IO.StreamReader(resp.GetResponseStream()))
                    {
                        strResponse = sr.ReadToEnd().Trim();

                        //sr.Close();
                        //resp.Close();
                    }
                       
                }
                req.Abort();
                //return true;
            }
            catch (WebException ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif

                strResponse = "";
                if (ex.Message.Contains("The remote server returned an error: (401) Unauthorized"))
                    strResponse = "401";              
            }
            catch (Exception ex)
            {
                strResponse = "";
                if (ex.Message.Contains("The remote server returned an error: (401) Unauthorized"))                
                    strResponse = "401";
                
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif                                
                                       
            }
            req.Abort();
        }

        public void HttpGetactualQREM(string strURI, out string strResponse, string corelogonToken)
        {
            System.Net.HttpWebRequest req = (HttpWebRequest)System.Net.WebRequest.Create(strURI);
            try
            {
                req.Headers["Authorization"] = "Bearer " + corelogonToken;
                req.ContentType = "application/json";

                req.Method = "Get";
                req.Timeout = 15000;
                req.ReadWriteTimeout = 5000;

                using (System.Net.WebResponse resp = req.GetResponse())
                {
                    if (resp == null)
                    {
                        strResponse = "";
                    }

                    using (System.IO.StreamReader sr = new System.IO.StreamReader(resp.GetResponseStream()))
                    {
                        strResponse = sr.ReadToEnd().Trim();
                    }
                }

                req.Abort();
            }
            catch (WebException ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif

                strResponse = "";
                if (ex.Message.Contains("The remote server returned an error: (401) Unauthorized"))
                    strResponse = "401";
            }
            catch (Exception ex)
            {
                strResponse = "";
                if (ex.Message.Contains("The remote server returned an error: (401) Unauthorized"))
                    strResponse = "401";

                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif

            }
            req.Abort();
        }

        public bool HttpGetDigestMethod(string strURI, string devicepassword,string ipaddr, ExecutionProcess executionprocess,out string strResponse)
        {
            strResponse = string.Empty;
            try
            {               
                string cnonce = string.Empty;
                string user = username;
                string password = devicepassword;
                string realm = string.Empty;
                string nonce = string.Empty;
                string qop = string.Empty;
                string requestMethod = "Get";
                int nc = 0;
                Uri uri = new Uri(strURI);
                string dir = uri.PathAndQuery;
                var request = (HttpWebRequest)WebRequest.Create(uri);
                HttpWebResponse response;

                try
                {
                    response = (HttpWebResponse)request.GetResponse();
                    if (((HttpWebResponse)response).StatusCode.ToString().ToUpper() == "OK")
                    {
                        var responseReader = new StreamReader(response.GetResponseStream());
                        strResponse = responseReader.ReadToEnd();
                        return true;
                    }
                }
                catch (WebException ex)
                {
                    if (ex.Response == null)
                        return false;

                    var wwwAuthenticateHeader = ex.Response.Headers["WWW-Authenticate"];
                    if (wwwAuthenticateHeader != null && wwwAuthenticateHeader.Contains("Digest"))
                    {
                        realm = GrabHeaderVar("realm", wwwAuthenticateHeader);
                        nonce = GrabHeaderVar("nonce", wwwAuthenticateHeader);
                        qop = GrabHeaderVar("qop", wwwAuthenticateHeader);
                        nc = 0;
                        cnonce = new Random().Next(123400, 9999999).ToString();

                        var request2 = (HttpWebRequest)WebRequest.Create(uri);
                        request2.AllowAutoRedirect = true;
                        request2.PreAuthenticate = true;
                        request2.Method = requestMethod;
                        request2.Headers.Add("Authorization", GetDigestHeader(dir, nc, user, realm, password, nonce, cnonce, qop, requestMethod));
                        HttpWebResponse response2 = (HttpWebResponse)request2.GetResponse();
                        var reader = new StreamReader(response2.GetResponseStream());
                        strResponse = reader.ReadToEnd();

                        if (((HttpWebResponse)response2).StatusCode.ToString().ToUpper() == "OK")
                        {
                            return true;
                        }
                    }
                }
                return false;
            }
            catch (WebException ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                strResponse = string.Empty;
                HttpGetactual(strURI, false, devicepassword, ipaddr, username, out strResponse, executionprocess);

              
                return false;
            }
        }


        private string CalculateMd5Hash(string input)
        {
            try
            {
                var inputBytes = Encoding.ASCII.GetBytes(input);
                var hash = MD5.Create().ComputeHash(inputBytes);
                var sb = new StringBuilder();
                foreach (var b in hash)
                    sb.Append(b.ToString("x2"));
                return sb.ToString();
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

        private string GrabHeaderVar(string varName, string header)
        {
            try
            {
                var regHeader = new Regex(string.Format(@"{0}=""([^""]*)""", varName));
                var matchHeader = regHeader.Match(header);
                if (matchHeader.Success)
                    return matchHeader.Groups[1].Value;
                throw new ApplicationException(string.Format("Header {0} not found", varName));
            }
            catch(Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                return string.Empty;
            }
        }

        public string GetDigestHeader(string dir, int nc, string user, string realm, string password, string nonce, string cnonce, string qop, string requestmethod)
        {
            try
            {
                nc = nc + 1;
                var ha1 = CalculateMd5Hash(string.Format("{0}:{1}:{2}", user, realm, password));
                var ha2 = CalculateMd5Hash(string.Format("{0}:{1}", "GET", dir));
                var digestResponse = CalculateMd5Hash(string.Format("{0}:{1}:{2:00000000}:{3}:{4}:{5}", ha1, nonce, nc, cnonce, qop, ha2));

                return string.Format("Digest username=\"{0}\", realm=\"{1}\", nonce=\"{2}\", uri=\"{3}\", " +
                    " response=\"{4}\", qop={5}, nc={6:00000000}, cnonce=\"{7}\"",
                    user, realm, nonce, dir, digestResponse, qop, nc, cnonce);
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

        public void HttpGet(string strURI,bool isnewapi, string m_password, string deviceIP, string deviceUsername, out string strResponse, ExecutionProcess execProcess)
        {  
            strResponse = "";

            try
            {
                
                HttpGetactual(strURI, isnewapi, m_password, deviceIP, deviceUsername, out strResponse, execProcess);

                if (strResponse == "401")
                {
                    string token = string.Empty;
                    var isLogonSuccess = execProcess.Corelogon(deviceIP, m_password, out token);
					/////If primary core, assign new token in CoreLogonToken else if backup core, assign new token CoreLogonToken_Backup
                    if(execProcess.selectedCoreIPAddress== deviceIP)
                        execProcess.CoreLogonToken = token;
                    else
                        execProcess.CoreLogonToken_Backup = token;

                    HttpGetactual(strURI, isnewapi, m_password, deviceIP, deviceUsername, out strResponse, execProcess);
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
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                //if (ex.Message != "Thread was being aborted.")
                //    MessageBox.Show("Exception\n " + ex.Message, "Error Code - ECLxxxx", MessageBoxButton.OK, MessageBoxImage.Error);
                // Output to debug, but then do nothing.  ** LOG THIS LATER **
                //Debug.WriteLine("Web_RW.SetBasicAuthHeader: " + ex.Message);
            }

            req.Headers["Authorization"] = "Basic " + strAuth;
        }

        public Tuple<List<logitems>, List<logitems>> get_TP_Logs_lastline(string from, Int32 TPID, string TPname, List<DUT_DeviceItem> selectedDutDeviceItemList, string password, string TP_timestamp, ExecutionProcess execProcess, bool isQREM)
        {
            try
            {
                List<DUT_DeviceItem> dutDeviceItemList = new List<DUT_DeviceItem>();
                List<logitems> logsvalue = new List<logitems>();
                List<logitems> Exceptcore_logitems = new List<logitems>();
                List<logitems> Core_logitems = new List<logitems>();
                //string coreNameForDUT = string.Empty;
                //string corename = string.Empty;
                string query = "select * from DesignInventory where DesignID in(select DesignID from TPDesignLinkTable where TPID = ('" + TPID + "'))";

                DataTable dataTable = new DataTable();
                DataTableReader dataTableReader = null;
                SqlDataAdapter dataAdapter = new SqlDataAdapter(query, connect.CreateConnection());
                connect.OpenConnection();
                dataAdapter.Fill(dataTable);
                dataTableReader = dataTable.CreateDataReader();

                if (dataTableReader.HasRows)
                {
                    List<string> deviceItem = new List<string>();
                    while (dataTableReader.Read())
                    {
                        if (!deviceItem.Contains(dataTableReader.GetString(3).ToString()))
                            deviceItem.Add(dataTableReader.GetString(3).ToString());


                    }
                    connect.CloseConnection();

                    if (!isQREM)
                    {
                        logsvalue = GetAllIpForSelectedAction_log(deviceItem, selectedDutDeviceItemList);
                        if (logsvalue != null)
                        {
                            DateTime Date_time_writelogs_TP = DateTime.Now;
                            Exceptcore_logitems = Tplog_getcurrentline_other(logsvalue, password, from, Date_time_writelogs_TP, TP_timestamp, execProcess);
                            Core_logitems = Tplog_getcurrentline_core(logsvalue, password, from, Date_time_writelogs_TP, TP_timestamp, execProcess);
                        }
                    }
                    else
                    {
                        logsvalue = GetAllReflectDeviceDetails_log(deviceItem, selectedDutDeviceItemList);

                        if (logsvalue != null)
                        {
                            DateTime Date_time_writelogs_TP = DateTime.Now;
                            Core_logitems = Tplog_getcurrentline_Reflectcore(logsvalue, password, from, Date_time_writelogs_TP, TP_timestamp, execProcess);
                        }
                    }
                }

                return new Tuple<List<logitems>, List<logitems>>(Exceptcore_logitems, Core_logitems);
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                //if (ex.Message != "Thread was being aborted.")
                //    MessageBox.Show("Exception\n " + ex.Message, "Error Code - ECLxxxx", MessageBoxButton.OK, MessageBoxImage.Error);

                return null;
            }
        }
        
        public bool IsCorePresent(string coreipcheck)
        {
            bool success = false;
            int trial = 0;
            try
            {
                if ((coreipcheck != null) && (coreipcheck != string.Empty) && (coreipcheck != "Not Applicable"))
                {
                    while ((!success) & (trial < 4))
                    {
                        Ping ping = new Ping();
                        PingReply pingReply = ping.Send(coreipcheck);

                        if (pingReply.Status == IPStatus.Success)
                        {
                            success = true;

                        }
                        else
                        {
                            trial = trial + 1;
                            Thread.Sleep(2000);
                        }

                    }
                }
                return success;
            }
            catch { return success; }
        }

        public List<logitems> Tplog_getcurrentline_Reflectcore(List<logitems> logItemForCore, string passwordkey, string from, DateTime Date_time_writelogs, string timestamp, ExecutionProcess execProcess)
        {
            try
            {
                List<logitems> logsvalue = new List<logitems>();
                string header_event_strResponse = string.Empty;
                string event_strResponse = string.Empty;
                string[] coreID = null;
                string devicename_design = string.Empty;
                string Qsys_lastline = string.Empty;
                string UCI_lastline = string.Empty;

                if (logItemForCore != null)
                {
                    foreach (logitems ipaddress in logItemForCore)
                    {
                        if (ipaddress.coreipaddress != null)
                        {
                            if (ipaddress.coreipaddress != string.Empty && ipaddress.coreipaddress != "Not Applicable")
                            {
                                coreID = ipaddress.coreipaddress.Split(';');
                                devicename_design = ipaddress.devicenameinDesign;
                                //bool istrue = IsCorePresent(coreID);

                                //if (coreID != string.Empty && coreID != "Not Applicable" && istrue)
                                if (coreID != null && coreID.Count() > 0)
                                {
                                    HttpGetactualQREM("https://" + Properties.Settings.Default.QREMreflectLink + "/api/v0/systems/" + coreID[1] + "/events?pageSize=100&page=1", out header_event_strResponse, DeviceDiscovery.QREM_Token);
                                    if (header_event_strResponse != string.Empty)
                                    {
                                        var obj = new JavaScriptSerializer { MaxJsonLength = Int32.MaxValue }.Deserialize<dynamic>(header_event_strResponse);
                                        if (obj.Count > 0)
                                        {
                                            foreach (var res in obj)
                                            {
                                                if (res.Key == "items")
                                                {
                                                    foreach (var childobj in res.Value)
                                                    {
                                                        //if (event_strResponse == string.Empty)
                                                        event_strResponse = childobj["id"].ToString();
                                                        break;
                                                        //else if (Convert.ToInt32(childobj["id"].ToString()) > Convert.ToInt32(event_strResponse))
                                                        //    event_strResponse = childobj["id"].ToString();
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }

                                if ((from == "TP_Start") || (from == "TC_Start"))
                                {
                                    Qsys_lastline = qsyslog(from, ipaddress);
                                    UCI_lastline = qsysucilog(from, ipaddress);
                                }
                                else if ((from == "TP_End") || (from == "TC_End"))
                                {
                                    if ((ipaddress.Logtype == "Qsyslog"))
                                    {
                                        Qsys_lastline = qsyslog(from, ipaddress);
                                        UCI_lastline = qsysucilog(from, ipaddress);
                                    }
                                }

                                if (from == "TP_Start")
                                {
                                    if (header_event_strResponse != string.Empty)
                                    {
                                        logsvalue.Add(new logitems { Otherips = ipaddress.coreipaddress, Coreversion = "NEW", Logtype = "eventlog", TP_directory_log = Date_time_writelogs, TP_timestamp = timestamp, LastLine = event_strResponse, devicenameinDesign = devicename_design, coreipaddress = ipaddress.coreipaddress });
                                    }

                                    logsvalue.Add(new logitems { Otherips = "Qsyslog", Logtype = "Qsyslog", TP_directory_log = Date_time_writelogs, TP_timestamp = timestamp, LastLine = Qsys_lastline, devicenameinDesign = devicename_design, coreipaddress = ipaddress.coreipaddress });

                                    logsvalue.Add(new logitems { Otherips = "Qsys_UCI", Logtype = "Qsys_UCI", TP_directory_log = Date_time_writelogs, TP_timestamp = timestamp, LastLine = UCI_lastline, devicenameinDesign = devicename_design, coreipaddress = ipaddress.coreipaddress });

                                    logsvalue.Add(new logitems { Otherips = "Windows_event_log", Logtype = "Windows_event_log", TP_directory_log = Date_time_writelogs, TP_timestamp = timestamp, LastLine = string.Empty, devicenameinDesign = devicename_design, coreipaddress = ipaddress.coreipaddress });
                                }
                                else if (from == "TC_Start")
                                {
                                    if (ipaddress.Logtype == "eventlog")
                                    {
                                        if (header_event_strResponse != string.Empty)
                                        {
                                            logsvalue.Add(new logitems { Otherips = ipaddress.coreipaddress, Coreversion = "NEW", Logtype = "eventlog", TP_directory_log = ipaddress.TP_directory_log, TC_directory_log = Date_time_writelogs, TP_timestamp = ipaddress.TP_timestamp, TC_timestamp = timestamp, LastLine = event_strResponse, devicenameinDesign = devicename_design, coreipaddress = ipaddress.coreipaddress });
                                        }
                                    }

                                    if (ipaddress.Logtype == "Qsyslog")
                                    {
                                        logsvalue.Add(new logitems { Otherips = "Qsyslog", Logtype = "Qsyslog", TP_directory_log = ipaddress.TP_directory_log, TC_directory_log = Date_time_writelogs, TP_timestamp = ipaddress.TP_timestamp, TC_timestamp = timestamp, LastLine = Qsys_lastline, devicenameinDesign = devicename_design, coreipaddress = ipaddress.coreipaddress });
                                    }

                                    if (ipaddress.Logtype == "Qsys_UCI")
                                    {
                                        logsvalue.Add(new logitems { Otherips = "Qsys_UCI", Logtype = "Qsys_UCI", TP_directory_log = ipaddress.TP_directory_log, TC_directory_log = Date_time_writelogs, TP_timestamp = ipaddress.TP_timestamp, TC_timestamp = timestamp, LastLine = UCI_lastline, devicenameinDesign = devicename_design, coreipaddress = ipaddress.coreipaddress });
                                    }

                                    if (ipaddress.Logtype == "Windows_event_log")
                                    {
                                        logsvalue.Add(new logitems { Otherips = "Windows_event_log", Logtype = "Windows_event_log", TP_directory_log = ipaddress.TP_directory_log, TC_directory_log = Date_time_writelogs, TP_timestamp = ipaddress.TP_timestamp, TC_timestamp = timestamp, LastLine = string.Empty, devicenameinDesign = devicename_design, coreipaddress = ipaddress.coreipaddress });
                                    }
                                }
                                else if ((from == "TP_End") | (from == "TC_End"))
                                {
                                    if (ipaddress.Logtype != null)
                                    {
                                        if ((ipaddress.Logtype == "eventlog"))
                                        {
                                            if ((header_event_strResponse != string.Empty) && ipaddress.Coreversion == "NEW" && ipaddress.LastLine.Length > 0)
                                            {
                                                //  Get new events by split current response using last event id (ipaddress.LastLine)
                                                string event_strResponseEnd = string.Empty;
                                                List<object> newEvents = new List<object>();
                                                var obj = new JavaScriptSerializer { MaxJsonLength = Int32.MaxValue }.Deserialize<dynamic>(header_event_strResponse);
                                                if (obj.Count > 0)
                                                {
                                                    foreach (var res in obj)
                                                    {
                                                        if (res.Key == "items")
                                                        {
                                                            foreach (var childobj in res.Value)
                                                            {
                                                                if (Convert.ToInt32(childobj["id"].ToString()) > Convert.ToInt32(ipaddress.LastLine))
                                                                {
                                                                    newEvents.Add(childobj);
                                                                }
                                                            }

                                                            if (newEvents.Count > 0)
                                                                event_strResponseEnd = Regex.Unescape(new JavaScriptSerializer().Serialize(newEvents));
                                                        }
                                                    }
                                                }

                                                logsvalue.Add(new logitems { Otherips = ipaddress.coreipaddress, LastLine = ipaddress.LastLine, TP_directory_log = ipaddress.TP_directory_log, TC_directory_log = ipaddress.TC_directory_log, TP_timestamp = ipaddress.TP_timestamp, TC_timestamp = ipaddress.TC_timestamp, Logtype = "eventlog", Fullresponse = event_strResponseEnd, devicenameinDesign = devicename_design, coreipaddress = ipaddress.coreipaddress });

                                            }
											else
                                            {
                                                logsvalue.Add(new logitems { Otherips = ipaddress.coreipaddress, LastLine = ipaddress.LastLine, TP_directory_log = ipaddress.TP_directory_log, TC_directory_log = ipaddress.TC_directory_log, TP_timestamp = ipaddress.TP_timestamp, TC_timestamp = ipaddress.TC_timestamp, Logtype = "eventlog", Fullresponse = header_event_strResponse, devicenameinDesign = devicename_design, coreipaddress = ipaddress.coreipaddress });
                                            }
                                        }

                                        if ((ipaddress.Logtype == "Qsyslog"))
                                        {
                                            logsvalue.Add(new logitems { Otherips = "Qsyslog", LastLine = ipaddress.LastLine, TP_directory_log = ipaddress.TP_directory_log, TC_directory_log = ipaddress.TC_directory_log, TP_timestamp = ipaddress.TP_timestamp, TC_timestamp = ipaddress.TC_timestamp, Logtype = "Qsyslog", Fullresponse = Qsys_lastline, devicenameinDesign = devicename_design, coreipaddress = ipaddress.coreipaddress });
                                        }

                                        if ((ipaddress.Logtype == "Qsys_UCI"))
                                        {
                                            logsvalue.Add(new logitems { Otherips = "Qsys_UCI", LastLine = ipaddress.LastLine, TP_directory_log = ipaddress.TP_directory_log, TC_directory_log = ipaddress.TC_directory_log, TP_timestamp = ipaddress.TP_timestamp, TC_timestamp = ipaddress.TC_timestamp, Logtype = "Qsys_UCI", Fullresponse = UCI_lastline, devicenameinDesign = devicename_design, coreipaddress = ipaddress.coreipaddress });
                                        }

                                        if (ipaddress.Logtype == "Windows_event_log")
                                        {
                                            string windows_event_log = win_event(from, Date_time_writelogs);
                                            if (!string.IsNullOrEmpty(windows_event_log))
                                            {
                                                logsvalue.Add(new logitems { Otherips = "Windows_event_log", LastLine = string.Empty, TP_directory_log = ipaddress.TP_directory_log, TC_directory_log = ipaddress.TC_directory_log, TP_timestamp = ipaddress.TP_timestamp, TC_timestamp = ipaddress.TC_timestamp, Logtype = "Windows_event_log", Fullresponse = windows_event_log, devicenameinDesign = devicename_design, coreipaddress = ipaddress.coreipaddress });
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                    return logsvalue;
                }
                return null;
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                //if (ex.Message != "Thread was being aborted.")
                //    MessageBox.Show("Exception\n " + ex.Message, "Error Code - ECLxxxx", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }

        public List<logitems> Tplog_getcurrentline_core(List<logitems> iplistfor_other, string passwordkey, string from, DateTime Date_time_writelogs,string timestamp, ExecutionProcess execProcess)
        {
            try
            {
                List<logitems> logsvalue = new List<logitems>();
                string[] ilog_lineget= null;
                string[] kernallog_lineget = null;
                string[] siplog_lineget = null;
                string[] eventlog_lineget = null;
                //string[] Qsys_log_lineget = null;
                ////string[] QsysUCI_log_lineget = null;
                string ilog_strResponse = string.Empty;
                string kernel_strResponse = string.Empty;
                string sip_strResponse = string.Empty;
                string header_event_strResponse= string.Empty;
                string header_event_strResponse_old = string.Empty;
                string event_strResponse = string.Empty;
                string event_strResponse_old = string.Empty;
                //string pathforconfiglog = Path.Combine(Properties.Settings.Default.Path.ToString() + "\\system_state.qsyslog");
                //string strResponse = string.Empty;
                string iplistforcore = string.Empty;
                //string designname = string.Empty;
                string devicename_design = string.Empty;
                //string device_build = string.Empty;
                string Qsys_lastline = string.Empty;
                string UCI_lastline = string.Empty;
                //string timestamp = string.Empty;

                //string totaldevices = string.Empty;
                //passwordkey = "QSClock";

                if (iplistfor_other != null)
                {
                    foreach (logitems ipaddress in iplistfor_other)
                    {
                        if (ipaddress.coreipaddress != null)
                        {
                            if (ipaddress.coreipaddress != string.Empty && ipaddress.coreipaddress != "Not Applicable")
                            {
                                iplistforcore = ipaddress.coreipaddress;
                                //designname = ipaddress.Design_name;
                                devicename_design = ipaddress.devicenameinDesign;
                                bool istrue = IsCorePresent(iplistforcore);
                                //device_build = ipaddress.temp_build;

                                if (iplistforcore != string.Empty && iplistforcore != "Not Applicable" && istrue)
                                {
                                    if (execProcess.core_New_FirmwareVersion.Item1)
                                    {
                                        HttpGetactual("http://" + iplistforcore + "/log.txt", false, passwordkey, iplistforcore, Properties.Settings.Default.DeviceUsername, out ilog_strResponse, execProcess);
                                        HttpGetactual("http://" + iplistforcore + "/sip.txt", false, passwordkey, iplistforcore, Properties.Settings.Default.DeviceUsername, out sip_strResponse, execProcess);

                                        HttpGetactual("http://" + iplistforcore + "/cgi-bin/kernel_log",false, passwordkey, iplistforcore, Properties.Settings.Default.DeviceUsername.ToString(), out kernel_strResponse, execProcess);
                                        HttpGet("http://" + iplistforcore + "/api/v0/systems/1/events?page=1&pageSize=100", true, passwordkey, iplistforcore, Properties.Settings.Default.DeviceUsername.ToString(), out header_event_strResponse, execProcess);
                                        if (header_event_strResponse != string.Empty)
                                        {
                                            var obj = new JavaScriptSerializer { MaxJsonLength = Int32.MaxValue }.Deserialize<dynamic>(header_event_strResponse);
                                            if (obj.Count > 0)
                                            {
                                                foreach (var res in obj)  
                                                {
                                                    if (res.Key == "items")
                                                    {
                                                        foreach (var childobj in res.Value)
                                                        {
                                                            //if (event_strResponse == string.Empty)
                                                            event_strResponse = childobj["id"].ToString();
                                                            break;
                                                            //else if (Convert.ToInt32(childobj["id"].ToString()) > Convert.ToInt32(event_strResponse))
                                                            //    event_strResponse = childobj["id"].ToString();
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        HttpGetactual("http://" + iplistforcore + "/log.txt", false, passwordkey, iplistforcore, username, out ilog_strResponse, execProcess);
                                        HttpGetactual("http://" + iplistforcore + "/sip.txt", false, passwordkey, iplistforcore, username, out sip_strResponse, execProcess);

                                        HttpGetactual("http://" + iplistforcore + "/cgi-bin/kernel_log",false, passwordkey, iplistforcore, username, out kernel_strResponse, execProcess);
                                        HttpGetactual("http://" + iplistforcore + "/designs/current_design/settings/event_log_head.xml",false, passwordkey, iplistforcore, username, out header_event_strResponse_old, execProcess);
                                        if (header_event_strResponse_old != string.Empty)
                                        {
                                            string[] line3 = header_event_strResponse_old.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
                                            string[] lin1e = line3[1].Split(new string[] { "=" }, StringSplitOptions.None);
                                            string gtyu = lin1e[1].ToString();
                                            string name_header = gtyu.Remove(gtyu.Length - 2);
                                            name_header = name_header.Remove(0, 1);
                                            HttpGetactual("http://" + iplistforcore + "/designs/current_design/settings/" + name_header,false, passwordkey, iplistforcore, username, out event_strResponse_old, execProcess);
                                        }
                                    }                                
                                }

                                if ((from == "TP_Start") || (from == "TC_Start"))
                                {
                                    Qsys_lastline = qsyslog(from, ipaddress);
                                    UCI_lastline = qsysucilog(from, ipaddress);
                                }
                                else if ((from == "TP_End") || (from == "TC_End"))
                                {
                                    if ((ipaddress.Logtype == "Qsyslog"))
                                    {
                                        Qsys_lastline = qsyslog(from, ipaddress);
                                        UCI_lastline = qsysucilog(from, ipaddress);
                                    }
                                }

                                if (from == "TP_Start")
                                {
                                    if (iplistfor_other != null)
                                    {
                                        if (ilog_strResponse != string.Empty)
                                        {
                                            ilog_lineget = ilog_strResponse.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
                                            logsvalue.Add(new logitems { Otherips = iplistforcore, Logtype = "ilog", TP_directory_log = Date_time_writelogs, TP_timestamp = timestamp, LastLine = ilog_lineget.Last().ToString(), devicenameinDesign = devicename_design, coreipaddress = iplistforcore });
                                        }

                                        if (kernel_strResponse != string.Empty)
                                        {
                                            kernallog_lineget = kernel_strResponse.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
                                            logsvalue.Add(new logitems { Otherips = iplistforcore, Logtype = "kernallog", TP_directory_log = Date_time_writelogs, TP_timestamp = timestamp,LastLine = kernallog_lineget.Last().ToString(), devicenameinDesign = devicename_design, coreipaddress = iplistforcore });
                                        }

                                        if (sip_strResponse != string.Empty)
                                        {
                                            siplog_lineget = sip_strResponse.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
                                            logsvalue.Add(new logitems { Otherips = iplistforcore, Logtype = "siplog", TP_directory_log = Date_time_writelogs, TP_timestamp = timestamp, LastLine = siplog_lineget.Last().ToString(), devicenameinDesign = devicename_design, coreipaddress = iplistforcore });
                                        }

                                        if (event_strResponse_old != string.Empty)
                                        {
                                            eventlog_lineget = event_strResponse_old.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
                                            logsvalue.Add(new logitems { Otherips = iplistforcore, Coreversion = "OLD", Logtype = "eventlog", TP_directory_log = Date_time_writelogs, TP_timestamp = timestamp, LastLine = eventlog_lineget.Last().ToString(), devicenameinDesign = devicename_design, coreipaddress = iplistforcore });
                                        }
                                        else if (header_event_strResponse != string.Empty)
                                        {
                                            logsvalue.Add(new logitems { Otherips = iplistforcore, Coreversion = "NEW", Logtype = "eventlog", TP_directory_log = Date_time_writelogs, TP_timestamp = timestamp, LastLine = event_strResponse, devicenameinDesign = devicename_design, coreipaddress = iplistforcore });
                                        }    
                                                                             

                                        logsvalue.Add(new logitems { Otherips = "Qsyslog", Logtype = "Qsyslog", TP_directory_log = Date_time_writelogs, TP_timestamp = timestamp, LastLine = Qsys_lastline, devicenameinDesign = devicename_design, coreipaddress = iplistforcore });

                                        logsvalue.Add(new logitems { Otherips = "Qsys_UCI", Logtype = "Qsys_UCI", TP_directory_log = Date_time_writelogs, TP_timestamp = timestamp, LastLine = UCI_lastline, devicenameinDesign = devicename_design, coreipaddress = iplistforcore });

                                        logsvalue.Add(new logitems { Otherips = "Windows_event_log", Logtype = "Windows_event_log", TP_directory_log = Date_time_writelogs, TP_timestamp = timestamp, LastLine = string.Empty, devicenameinDesign = devicename_design, coreipaddress = iplistforcore });
                                    }
                                }
                                else if (from == "TC_Start")
                                {
                                    if (iplistfor_other != null)
                                    {
                                        if (ipaddress.Logtype == "ilog")
                                        {
                                            if (ilog_strResponse != string.Empty)
                                            {

                                                ilog_lineget = ilog_strResponse.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
                                                logsvalue.Add(new logitems { Otherips = iplistforcore, Logtype = "ilog", TP_directory_log = ipaddress.TP_directory_log, TC_directory_log = Date_time_writelogs,TP_timestamp=ipaddress.TP_timestamp,TC_timestamp=timestamp, LastLine = ilog_lineget.Last().ToString(), devicenameinDesign = devicename_design, coreipaddress = iplistforcore });
                                            }
                                        }

                                        if (ipaddress.Logtype == "kernallog")
                                        {
                                            if (kernel_strResponse != string.Empty)
                                            {
                                                kernallog_lineget = kernel_strResponse.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
                                                logsvalue.Add(new logitems { Otherips = iplistforcore, Logtype = "kernallog", TP_directory_log = ipaddress.TP_directory_log, TC_directory_log = Date_time_writelogs, TP_timestamp = ipaddress.TP_timestamp, TC_timestamp = timestamp, LastLine = kernallog_lineget.Last().ToString(), devicenameinDesign = devicename_design, coreipaddress = iplistforcore });
                                            }
                                        }

                                        if (ipaddress.Logtype == "siplog")
                                        {
                                            if (sip_strResponse != string.Empty)
                                            {
                                                siplog_lineget = sip_strResponse.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
                                                logsvalue.Add(new logitems { Otherips = iplistforcore, Logtype = "siplog", TP_directory_log = ipaddress.TP_directory_log, TC_directory_log = Date_time_writelogs, TP_timestamp = ipaddress.TP_timestamp, TC_timestamp = timestamp, LastLine = siplog_lineget.Last().ToString(), devicenameinDesign = devicename_design, coreipaddress = iplistforcore });
                                            }
                                        }

                                        if (ipaddress.Logtype == "eventlog")
                                        {
                                            if (header_event_strResponse != string.Empty)
                                            {                                                
                                                logsvalue.Add(new logitems { Otherips = iplistforcore, Coreversion="NEW", Logtype = "eventlog", TP_directory_log = ipaddress.TP_directory_log, TC_directory_log = Date_time_writelogs, TP_timestamp = ipaddress.TP_timestamp, TC_timestamp = timestamp, LastLine = event_strResponse, devicenameinDesign = devicename_design, coreipaddress = iplistforcore });
                                            }
                                            else if (event_strResponse_old != string.Empty)
                                            {
                                                eventlog_lineget = event_strResponse_old.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
                                                logsvalue.Add(new logitems { Otherips = iplistforcore, Coreversion="OLD", Logtype = "eventlog", TP_directory_log = ipaddress.TP_directory_log, TC_directory_log = Date_time_writelogs, TP_timestamp = ipaddress.TP_timestamp, TC_timestamp = timestamp, LastLine = eventlog_lineget.Last().ToString(), devicenameinDesign = devicename_design, coreipaddress = iplistforcore });
                                            }
                                        }

                                        if (ipaddress.Logtype == "Qsyslog")
                                        {                                           
                                                logsvalue.Add(new logitems { Otherips = "Qsyslog", Logtype = "Qsyslog", TP_directory_log = ipaddress.TP_directory_log, TC_directory_log = Date_time_writelogs, TP_timestamp = ipaddress.TP_timestamp, TC_timestamp = timestamp, LastLine = Qsys_lastline, devicenameinDesign = devicename_design, coreipaddress = iplistforcore }); 
                                        }

                                        if (ipaddress.Logtype == "Qsys_UCI")
                                        {
                                            logsvalue.Add(new logitems { Otherips = "Qsys_UCI", Logtype = "Qsys_UCI", TP_directory_log = ipaddress.TP_directory_log, TC_directory_log = Date_time_writelogs, TP_timestamp = ipaddress.TP_timestamp, TC_timestamp = timestamp, LastLine = UCI_lastline, devicenameinDesign = devicename_design, coreipaddress = iplistforcore });
                                        }
                                        if (ipaddress.Logtype == "Windows_event_log")
                                        {
                                            logsvalue.Add(new logitems { Otherips = "Windows_event_log", Logtype = "Windows_event_log", TP_directory_log = ipaddress.TP_directory_log, TC_directory_log = Date_time_writelogs, TP_timestamp = ipaddress.TP_timestamp, TC_timestamp = timestamp, LastLine = string.Empty, devicenameinDesign = devicename_design, coreipaddress = iplistforcore });
                                        }
                                    }
                                }

                                else if ((from == "TP_End") | (from == "TC_End"))
                                {
                                    if (iplistfor_other != null)
                                    {
                                        //foreach (logitems execution in iplistfor_other)
                                        //{
                                        if (ipaddress.Logtype != null)
                                        {
                                            if ((ipaddress.Logtype == "ilog") & (ilog_strResponse != string.Empty))
                                            {
                                                if (ipaddress.LastLine.Length > 0)
                                                {
                                                    Int32 lastlinelength = ipaddress.LastLine.Length;
                                                    string ilog_log = ilog_strResponse.Substring(ilog_strResponse.IndexOf(ipaddress.LastLine) + lastlinelength);
                                                    logsvalue.Add(new logitems { Otherips = iplistforcore, LastLine = ipaddress.LastLine, TP_directory_log = ipaddress.TP_directory_log, TC_directory_log = ipaddress.TC_directory_log,TP_timestamp=ipaddress.TP_timestamp,TC_timestamp=ipaddress.TC_timestamp, Logtype = "ilog", Fullresponse = ilog_log, devicenameinDesign = devicename_design, coreipaddress = iplistforcore });
                                                }
                                                else
                                                {
                                                    logsvalue.Add(new logitems { Otherips = iplistforcore, LastLine = ipaddress.LastLine, TP_directory_log = ipaddress.TP_directory_log, TC_directory_log = ipaddress.TC_directory_log, TP_timestamp = ipaddress.TP_timestamp, TC_timestamp = ipaddress.TC_timestamp, Logtype = "ilog", Fullresponse = ilog_strResponse, devicenameinDesign = devicename_design, coreipaddress = iplistforcore });
                                                }
                                            }

                                            if ((ipaddress.Logtype == "kernallog") & (kernel_strResponse != string.Empty))
                                            {
                                                if (ipaddress.LastLine.Length > 0)
                                                {
                                                    Int32 lastlinelength = ipaddress.LastLine.Length;
                                                    string kernallog_log = kernel_strResponse.Substring(kernel_strResponse.IndexOf(ipaddress.LastLine) + lastlinelength);
                                                    logsvalue.Add(new logitems { Otherips = iplistforcore, LastLine = ipaddress.LastLine, TP_directory_log = ipaddress.TP_directory_log, TC_directory_log = ipaddress.TC_directory_log, TP_timestamp = ipaddress.TP_timestamp, TC_timestamp = ipaddress.TC_timestamp, Logtype = "kernallog", Fullresponse = kernallog_log, devicenameinDesign = devicename_design, coreipaddress = iplistforcore });
                                                }
                                                else
                                                {
                                                    logsvalue.Add(new logitems { Otherips = iplistforcore, LastLine = ipaddress.LastLine, TP_directory_log = ipaddress.TP_directory_log, TC_directory_log = ipaddress.TC_directory_log, TP_timestamp = ipaddress.TP_timestamp, TC_timestamp = ipaddress.TC_timestamp, Logtype = "kernallog", Fullresponse = kernel_strResponse, devicenameinDesign = devicename_design, coreipaddress = iplistforcore });
                                                }
                                            }

                                            if ((ipaddress.Logtype == "siplog") & (sip_strResponse != string.Empty))
                                            {
                                                if (ipaddress.LastLine.Length > 0)
                                                {
                                                    Int32 lastlinelength = ipaddress.LastLine.Length;
                                                    string siplog_log = sip_strResponse.Substring(sip_strResponse.IndexOf(ipaddress.LastLine) + lastlinelength);
                                                    logsvalue.Add(new logitems { Otherips = iplistforcore, LastLine = ipaddress.LastLine, TP_directory_log = ipaddress.TP_directory_log, TC_directory_log = ipaddress.TC_directory_log, TP_timestamp = ipaddress.TP_timestamp, TC_timestamp = ipaddress.TC_timestamp, Logtype = "siplog", Fullresponse = siplog_log, devicenameinDesign = devicename_design, coreipaddress = iplistforcore });
                                                }
                                                else
                                                {
                                                    logsvalue.Add(new logitems { Otherips = iplistforcore, LastLine = ipaddress.LastLine, TP_directory_log = ipaddress.TP_directory_log, TC_directory_log = ipaddress.TC_directory_log, TP_timestamp = ipaddress.TP_timestamp, TC_timestamp = ipaddress.TC_timestamp, Logtype = "siplog", Fullresponse = sip_strResponse, devicenameinDesign = devicename_design, coreipaddress = iplistforcore });
                                                }
                                            }

                                            if ((ipaddress.Logtype == "eventlog"))
                                            {
                                                if ((header_event_strResponse != string.Empty) && ipaddress.Coreversion == "NEW" && ipaddress.LastLine.Length > 0)
                                                {
                                                    //  Get new events by split current response using last event id (ipaddress.LastLine)
                                                    string event_strResponseEnd = string.Empty;
                                                    List<object> newEvents = new List<object>();
                                                    var obj = new JavaScriptSerializer { MaxJsonLength = Int32.MaxValue }.Deserialize<dynamic>(header_event_strResponse);
                                                    if (obj.Count > 0)
                                                    {
                                                        foreach (var res in obj)
                                                        {
                                                            if (res.Key == "items")
                                                            {
                                                                foreach (var childobj in res.Value)
                                                                {
                                                                    if (Convert.ToInt32(childobj["id"].ToString()) > Convert.ToInt32(ipaddress.LastLine))
                                                                    {
                                                                        newEvents.Add(childobj);
                                                                    }
                                                                }

                                                                if (newEvents.Count > 0)
                                                                    event_strResponseEnd = Regex.Unescape(new JavaScriptSerializer().Serialize(newEvents));
                                                            }
                                                        }
                                                    }

                                                    logsvalue.Add(new logitems { Otherips = iplistforcore, LastLine = ipaddress.LastLine, TP_directory_log = ipaddress.TP_directory_log, TC_directory_log = ipaddress.TC_directory_log, TP_timestamp = ipaddress.TP_timestamp, TC_timestamp = ipaddress.TC_timestamp, Logtype = "eventlog", Fullresponse = event_strResponseEnd, devicenameinDesign = devicename_design, coreipaddress = iplistforcore });

                                                }
                                                else if (event_strResponse_old != string.Empty && ipaddress.Coreversion == "OLD" && ipaddress.LastLine.Length > 0)
                                                {
                                                    Int32 lastlinelength = ipaddress.LastLine.Length;
                                                    string eventlog_log = event_strResponse_old.Substring(event_strResponse_old.IndexOf(ipaddress.LastLine) + lastlinelength);
                                                    logsvalue.Add(new logitems { Otherips = iplistforcore, LastLine = ipaddress.LastLine, TP_directory_log = ipaddress.TP_directory_log, TC_directory_log = ipaddress.TC_directory_log, TP_timestamp = ipaddress.TP_timestamp, TC_timestamp = ipaddress.TC_timestamp, Logtype = "eventlog", Fullresponse = eventlog_log, devicenameinDesign = devicename_design, coreipaddress = iplistforcore });
                                                }
                                                else
                                                {
                                                    string fullres = string.Empty;
                                                    if (execProcess.core_New_FirmwareVersion.Item1)
                                                        fullres = header_event_strResponse;
                                                    else 
                                                        fullres = event_strResponse_old;                                                     

                                                    logsvalue.Add(new logitems { Otherips = iplistforcore, LastLine = ipaddress.LastLine, TP_directory_log = ipaddress.TP_directory_log, TC_directory_log = ipaddress.TC_directory_log, TP_timestamp = ipaddress.TP_timestamp, TC_timestamp = ipaddress.TC_timestamp, Logtype = "eventlog", Fullresponse = fullres, devicenameinDesign = devicename_design, coreipaddress = iplistforcore });
                                                }
                                            }

                                            if ((ipaddress.Logtype == "Qsyslog") )
                                            {                                                
                                                logsvalue.Add(new logitems { Otherips = "Qsyslog", LastLine = ipaddress.LastLine, TP_directory_log = ipaddress.TP_directory_log, TC_directory_log = ipaddress.TC_directory_log, TP_timestamp = ipaddress.TP_timestamp, TC_timestamp = ipaddress.TC_timestamp, Logtype = "Qsyslog", Fullresponse = Qsys_lastline, devicenameinDesign = devicename_design, coreipaddress = iplistforcore });
                                            }

                                            if ((ipaddress.Logtype == "Qsys_UCI"))
                                            {
                                                logsvalue.Add(new logitems { Otherips = "Qsys_UCI", LastLine = ipaddress.LastLine, TP_directory_log = ipaddress.TP_directory_log, TC_directory_log = ipaddress.TC_directory_log, TP_timestamp = ipaddress.TP_timestamp, TC_timestamp = ipaddress.TC_timestamp, Logtype = "Qsys_UCI", Fullresponse = UCI_lastline, devicenameinDesign = devicename_design, coreipaddress = iplistforcore });
                                            }

                                            if (ipaddress.Logtype == "Windows_event_log")
                                            {
                                                string windows_event_log = win_event(from, Date_time_writelogs);
                                                if (!string.IsNullOrEmpty(windows_event_log))
                                                {
                                                    logsvalue.Add(new logitems { Otherips = "Windows_event_log", LastLine = string.Empty, TP_directory_log = ipaddress.TP_directory_log, TC_directory_log = ipaddress.TC_directory_log, TP_timestamp = ipaddress.TP_timestamp, TC_timestamp = ipaddress.TC_timestamp, Logtype = "Windows_event_log", Fullresponse = windows_event_log, devicenameinDesign = devicename_design, coreipaddress = iplistforcore });
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                    return logsvalue;
                }
                return null;
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                //if (ex.Message != "Thread was being aborted.")
                //    MessageBox.Show("Exception\n " + ex.Message, "Error Code - ECLxxxx", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }

        public string qsyslog(string from, logitems details)
        {
            try
            {
                string QSysLog_lastline = string.Empty;
                //string qsys_uci_lastline = string.Empty;
                DirectoryInfo directorytoread = null;
                //DirectoryInfo directorytoread_uci = null;
                if (Properties.Settings.Default.Qsyscheckbox.ToString() == "true")
                {
                    directorytoread = new DirectoryInfo(Properties.Settings.Default.Qsystemppath.ToString());
                }
                else
                {
                    directorytoread = new DirectoryInfo(@"C:\Users\" + Environment.UserName + @"\AppData\Local\Temp\QSys Temp Files\");
                }

                if (File.Exists((directorytoread.FullName) + "\\QSysLog.txt"))
                {
                    if ((from == "TP_Start") || (from == "TC_Start"))
                    {
                        //List<string> lines = new List<string>();
                        //using (StreamReader read = new StreamReader(@directorytoread.FullName + "\\QSysLog.txt"))
                        //{
                        //    while (!read.EndOfStream)
                        //    {
                        //        string val = read.ReadLine();
                        //        if (val != "")
                        //            lines.Add(val);
                        //    }
                        //}

                        ////IEnumerable<string> lines = System.IO.File.ReadLines(@directorytoread.FullName + "\\QSysLog.txt").Where(line => line != "");
                        //string Qsys_log_lineget1 = lines.Last().ToString();
                        //lines.Clear();
                        //QSysLog_lastline = Qsys_log_lineget1;
                        
                        using (StreamReader read = new StreamReader(@directorytoread.FullName + "\\QSysLog.txt"))
                        {
                            while (!read.EndOfStream)
                            {
                                string val = read.ReadLine();
                                if (val != "")
                                    QSysLog_lastline = val;
                            }
                        }
                    }
                    else if ((from == "TP_End") || (from == "TC_End"))
                    {
                        string newstring = details.LastLine;

                        if(newstring != null && newstring != string.Empty)
                        {
                            List<string> result = new List<string>();
                            using (StreamReader read = new StreamReader(@directorytoread.FullName + "\\QSysLog.txt"))
                            {
                                bool reachedLine = false;
                                while (!read.EndOfStream)
                                {
                                    string val = read.ReadLine();

                                    if (reachedLine)
                                    {
                                        result.Add(val);
                                    }
                                    else if (val == newstring)
                                    { reachedLine = true; }
                                }
                            }

                            //var lines = System.IO.File.ReadAllLines(@directorytoread.FullName + "\\QSysLog.txt");
                            //var result = lines.SkipWhile(x => x != newstring).ToArray();
                            //if (result.Count() > 0 && result[0] == newstring)
                            //{
                            //    result = result.Skip(1).ToArray();
                            //}

                            QSysLog_lastline = string.Join("\r\n", result);
                        }
                        else
                        {
                            List<string> lines = new List<string>();
                            using (StreamReader read = new StreamReader(@directorytoread.FullName + "\\QSysLog.txt"))
                            {
                                while (!read.EndOfStream)
                                {
                                    string val = read.ReadLine();
                                    lines.Add(val);
                                }
                            }

                            //var lines = System.IO.File.ReadAllLines(@directorytoread.FullName + "\\QSysLog.txt");
                            QSysLog_lastline = string.Join("\r\n", lines);
                        }                        
                    }
                }

                return QSysLog_lastline;
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                //if (ex.Message != "Thread was being aborted.")
                //    MessageBox.Show("Exception\n \n" + ex.Message, "QAT Error Code - ECLxxxx", MessageBoxButton.OK, MessageBoxImage.Error);
                return string.Empty;
            }
        }

        public string qsysucilog(string from, logitems detailks)
        {
            bool readaccess = false;
            bool accesserror_check = false;
            string QSys_UCI_lastline = string.Empty;
            try
            {
                DirectoryInfo dir = new DirectoryInfo(@"C:\users\" + Environment.UserName);
                readaccess = checkreadaccess(dir.FullName);
                accesserror_check = check_error(dir);
                if ((readaccess) & (accesserror_check))
                {
                    foreach (FileInfo file in dir.GetFiles())
                    {
                        if (file.Name == ".qsys_uci.log")
                        {
                            string uci_log_path = file.FullName.ToString();
                            if (File.Exists(uci_log_path))
                            {
                                if ((from == "TP_Start") || (from == "TC_Start"))
                                {
                                    //List<string> lines = new List<string>();
                                    //using (StreamReader read = new StreamReader(uci_log_path))
                                    //{
                                    //    while (!read.EndOfStream)
                                    //    {
                                    //        string val = read.ReadLine();
                                    //        if (val != "")
                                    //            lines.Add(val);
                                    //    }
                                    //}

                                    ////IEnumerable<string> lines = System.IO.File.ReadLines(uci_log_path).Where(line => line != "");
                                    //string Qsys_log_lineget1 = lines.Last().ToString();
                                    //QSys_UCI_lastline = Qsys_log_lineget1;

                                    using (StreamReader read = new StreamReader(uci_log_path))
                                    {
                                        while (!read.EndOfStream)
                                        {
                                            string val = read.ReadLine();
                                            if (val != "")
                                                QSys_UCI_lastline = val;
                                        }
                                    }
                                }
                                else if ((from == "TP_End") || (from == "TC_End"))
                                {
                                    string newstring = detailks.LastLine;

                                    if (newstring != null && newstring != string.Empty)
                                    {
                                        List<string> result = new List<string>();
                                        using (StreamReader read = new StreamReader(uci_log_path))
                                        {
                                            bool reachedLine = false;
                                            while (!read.EndOfStream)
                                            {
                                                string val = read.ReadLine();

                                                if (reachedLine)
                                                {
                                                    result.Add(val);
                                                }
                                                else if (val == newstring)
                                                { reachedLine = true; }
                                            }
                                        }

                                        QSys_UCI_lastline = string.Join("\r\n", result);
                                    }
                                    else
                                    {
                                        List<string> lines = new List<string>();
                                        using (StreamReader read = new StreamReader(uci_log_path))
                                        {
                                            while (!read.EndOfStream)
                                            {
                                                string val = read.ReadLine();
                                                lines.Add(val);
                                            }
                                        }

                                        //var lines = System.IO.File.ReadAllLines(uci_log_path);
                                        QSys_UCI_lastline = string.Join("\r\n", lines);
                                    }
                                }
                            }
                        }
                    }
                }


                //DirectoryInfo dir = new DirectoryInfo(@"C:\users\");
                //DirectoryInfo[] dircollection = dir.GetDirectories();
                //FileInfo[] files = null;
                //foreach (DirectoryInfo direct in dircollection)
                //{

                //    readaccess = checkreadaccess(direct.FullName);
                //    accesserror_check = check_error(direct);
                //    if ((readaccess) & (accesserror_check))
                //    {
                //        files = direct.GetFiles(".qsys_uci.log");

                //        if (files.Length > 0)
                //        {
                //            Verify_UCI_viewer_Application();
                //            if (files[0].ToString() == ".qsys_uci.log")
                //            {
                //                string uci_log_path = files[0].FullName.ToString();
                //                if (File.Exists(uci_log_path))
                //                {
                //                    if ((from == "TP_Start") || (from == "TC_Start"))
                //                    {
                //                        List<string> lines = new List<string>();
                //                        using (StreamReader read = new StreamReader(uci_log_path))
                //                        {
                //                            while (!read.EndOfStream)
                //                            {
                //                                string val = read.ReadLine();
                //                                if (val != "")
                //                                    lines.Add(val);
                //                            }
                //                        }

                //                        //IEnumerable<string> lines = System.IO.File.ReadLines(uci_log_path).Where(line => line != "");
                //                        string Qsys_log_lineget1 = lines.Last().ToString();
                //                        QSys_UCI_lastline = Qsys_log_lineget1;
                //                    }
                //                    else if ((from == "TP_End") || (from == "TC_End"))
                //                    {
                //                        string newstring = detailks.LastLine;

                //                        if (newstring != null && newstring != string.Empty)
                //                        {
                //                            List<string> result = new List<string>();
                //                            using (StreamReader read = new StreamReader(uci_log_path))
                //                            {
                //                                bool reachedLine = false;
                //                                while (!read.EndOfStream)
                //                                {
                //                                    string val = read.ReadLine();

                //                                    if (val == newstring)
                //                                    { reachedLine = true; }
                //                                    else if (reachedLine)
                //                                    {
                //                                        result.Add(val);
                //                                    }
                //                                }
                //                            }

                //                            var lines = System.IO.File.ReadAllLines(uci_log_path);
                //                            var result1 = lines.SkipWhile(x => x != newstring).ToArray();
                //                            if (result1.Count() > 0 && result1[0] == newstring)
                //                            {
                //                                result1 = result1.Skip(1).ToArray();
                //                            }

                //                            QSys_UCI_lastline = string.Join("\r\n", result);
                //                        }
                //                        else
                //                        {
                //                            List<string> lines = new List<string>();
                //                            using (StreamReader read = new StreamReader(uci_log_path))
                //                            {
                //                                while (!read.EndOfStream)
                //                                {
                //                                    string val = read.ReadLine();
                //                                    lines.Add(val);
                //                                }
                //                            }

                //                            //var lines = System.IO.File.ReadAllLines(uci_log_path);
                //                            QSys_UCI_lastline = string.Join("\r\n", lines);
                //                        }
                //                    }
                //                }
                //            }
                //        }
                //    }
                //}

                return QSys_UCI_lastline;
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                //if (ex.Message != "Thread was being aborted.")
                //    MessageBox.Show("Exception\n \n" + ex.Message, "QAT Error Code - ECLxxxx", MessageBoxButton.OK, MessageBoxImage.Error);
                return string.Empty;
            }
        }
             

        public List<logitems> Tplog_getcurrentline_other(List<logitems> iplistfor_other, string passwordkey, string from, DateTime Date_time_writelogs, string timestamp, ExecutionProcess execProcess)
        {
            try
            {   
                List<logitems> logsvalue = new List<logitems>();
                string ilog_strResponse = string.Empty;
                string kernallog_strResponse = string.Empty;
                string[] kernallog_lineget = null;
                string[] ilog_lineget = null;
                //passwordkey = "QSClock";

                if (iplistfor_other != null)
                {
                    if (from == "TP_Start")
                    {
                        foreach (logitems ipaddress in iplistfor_other)
                        {
                            if (string.IsNullOrEmpty(ipaddress.coreipaddress))
                            {
                                //for (int i = 0; i < ipaddress.ipforlogs.Count; i++)
                                //{
                                if (ipaddress.Otherips != string.Empty & ipaddress.Otherips != "Not Applicable")
                                {    
                                    HttpGetDigestMethod("http://" + ipaddress.Otherips + "/cgi-bin/kernel_log", passwordkey, ipaddress.Otherips, execProcess, out kernallog_strResponse);     
                                    
                                    if (kernallog_strResponse != string.Empty)
                                    {
                                        kernallog_lineget = kernallog_strResponse.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
                                        logsvalue.Add(new logitems { Otherips = ipaddress.Otherips, Logtype = "kernallog", TP_directory_log = Date_time_writelogs, TP_timestamp= timestamp, LastLine = kernallog_lineget.Last().ToString(), devicenameinDesign = ipaddress.devicenameinDesign});
                                    }
                                    else
                                    {
                                        logsvalue.Add(new logitems { Otherips = ipaddress.Otherips, Logtype = "kernallog", TP_directory_log = Date_time_writelogs, TP_timestamp = timestamp, LastLine = string.Empty, devicenameinDesign = ipaddress.devicenameinDesign});
                                    }


                                    HttpGetactual("http://" + ipaddress.Otherips + "/log.txt", false, passwordkey, ipaddress.Otherips, Properties.Settings.Default.DeviceUsername, out ilog_strResponse, execProcess);

                                    if (ilog_strResponse != string.Empty)
                                    {
                                        ilog_lineget = ilog_strResponse.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
                                        logsvalue.Add(new logitems { Otherips = ipaddress.Otherips, Logtype = "ilog", TP_directory_log = Date_time_writelogs, TP_timestamp = timestamp, LastLine = ilog_lineget.Last().ToString(), devicenameinDesign = ipaddress.devicenameinDesign});
                                    }
                                    else
                                    {
                                        logsvalue.Add(new logitems { Otherips = ipaddress.Otherips, Logtype = "ilog", TP_directory_log = Date_time_writelogs, TP_timestamp = timestamp, LastLine = string.Empty, devicenameinDesign = ipaddress.devicenameinDesign});
                                    }
                                }
                                //}
                            }
                        }
                        //return logsvalue;
                    }

                    if (from == "TC_Start")
                    {
                        foreach (logitems ipaddress in iplistfor_other)
                        {
                            if (ipaddress.Otherips != string.Empty & ipaddress.Otherips != "Not Applicable")
                            {
                                if (ipaddress.Logtype == "ilog")
                                {
                                    HttpGetactual("http://" + ipaddress.Otherips + "/log.txt", false, passwordkey, ipaddress.Otherips, Properties.Settings.Default.DeviceUsername, out ilog_strResponse, execProcess);

                                    if (ilog_strResponse != string.Empty)
                                    {
                                        ilog_lineget = ilog_strResponse.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
                                        logsvalue.Add(new logitems { Otherips = ipaddress.Otherips, Logtype = "ilog", TP_directory_log = ipaddress.TP_directory_log, TC_directory_log = Date_time_writelogs, TP_timestamp=ipaddress.TP_timestamp, TC_timestamp = timestamp, LastLine = ilog_lineget.Last().ToString(), devicenameinDesign = ipaddress.devicenameinDesign});
                                    }
                                    else
                                    {
                                        logsvalue.Add(new logitems { Otherips = ipaddress.Otherips, Logtype = "ilog", TP_directory_log = ipaddress.TP_directory_log, TC_directory_log = Date_time_writelogs, TP_timestamp = ipaddress.TP_timestamp,TC_timestamp = timestamp, LastLine = string.Empty, devicenameinDesign = ipaddress.devicenameinDesign});

                                    }
                                }
                                if (ipaddress.Logtype == "kernallog")
                                {   
                                    HttpGetDigestMethod("http://" + ipaddress.Otherips + "/cgi-bin/kernel_log", passwordkey, ipaddress.Otherips, execProcess, out kernallog_strResponse);                                  

                                    if (kernallog_strResponse != string.Empty)
                                    {
                                        kernallog_lineget = kernallog_strResponse.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
                                        logsvalue.Add(new logitems { Otherips = ipaddress.Otherips, Logtype = "kernallog", TP_directory_log = ipaddress.TP_directory_log, TC_directory_log = Date_time_writelogs, TP_timestamp = ipaddress.TP_timestamp,TC_timestamp = timestamp, LastLine = kernallog_lineget.Last().ToString(), devicenameinDesign = ipaddress.devicenameinDesign});
                                    }
                                    else
                                    {
                                        logsvalue.Add(new logitems { Otherips = ipaddress.Otherips, Logtype = "kernallog", TP_directory_log = ipaddress.TP_directory_log, TC_directory_log = Date_time_writelogs, TP_timestamp = ipaddress.TP_timestamp,TC_timestamp = timestamp, LastLine = string.Empty, devicenameinDesign = ipaddress.devicenameinDesign});
                                    }
                                }
                            }
                        }
                        //return logsvalue;
                    }

                    if ((from == "TP_End") | (from == "TC_End"))
                    {
                        foreach (logitems execution in iplistfor_other)
                        {
                            if (execution.Otherips != string.Empty & execution.Otherips != "Not Applicable")
                            {
                                HttpGetDigestMethod("http://" + execution.Otherips + "/cgi-bin/kernel_log", passwordkey, execution.Otherips, execProcess,out kernallog_strResponse);
                                HttpGetactual("http://" + execution.Otherips + "/log.txt", false, passwordkey, execution.Otherips, Properties.Settings.Default.DeviceUsername, out ilog_strResponse, execProcess);

                                if (execution.Logtype != null)
                                {
                                    if ((execution.Logtype == "ilog") & (ilog_strResponse != string.Empty))
                                    {
                                        if (execution.LastLine.Length > 0)
                                        {

                                            Int32 lastlinelength = execution.LastLine.Length;
                                            string ilog_log = ilog_strResponse.Substring(ilog_strResponse.IndexOf(execution.LastLine) + lastlinelength);
                                            logsvalue.Add(new logitems { Otherips = execution.Otherips, LastLine = execution.LastLine, TP_directory_log = execution.TP_directory_log, TC_directory_log = execution.TC_directory_log, TP_timestamp = execution.TP_timestamp, TC_timestamp = execution.TC_timestamp, Logtype = "ilog", Fullresponse = ilog_log, devicenameinDesign = execution.devicenameinDesign });
                                        }
                                        else
                                        {
                                            logsvalue.Add(new logitems { Otherips = execution.Otherips, LastLine = execution.LastLine, TP_directory_log = execution.TP_directory_log, TC_directory_log = execution.TC_directory_log, TP_timestamp = execution.TP_timestamp, TC_timestamp = execution.TC_timestamp, Logtype = "ilog", Fullresponse = ilog_strResponse, devicenameinDesign = execution.devicenameinDesign });
                                        }

                                    }

                                    if ((execution.Logtype == "kernallog") & (kernallog_strResponse != string.Empty))
                                    {
                                        if (execution.LastLine.Length > 0)
                                        {
                                            Int32 lastlinelength = execution.LastLine.Length;
                                            string kernallog_log = kernallog_strResponse.Substring(kernallog_strResponse.IndexOf(execution.LastLine) + lastlinelength);
                                            logsvalue.Add(new logitems { Otherips = execution.Otherips, LastLine = execution.LastLine, TP_directory_log = execution.TP_directory_log, TC_directory_log = execution.TC_directory_log, TP_timestamp = execution.TP_timestamp, TC_timestamp = execution.TC_timestamp, Logtype = "kernallog", Fullresponse = kernallog_log, devicenameinDesign = execution.devicenameinDesign });
                                        }
                                        else
                                        {
                                            logsvalue.Add(new logitems { Otherips = execution.Otherips, LastLine = execution.LastLine, TP_directory_log = execution.TP_directory_log, TC_directory_log = execution.TC_directory_log, TP_timestamp = execution.TP_timestamp, TC_timestamp = execution.TC_timestamp, Logtype = "kernallog", Fullresponse = kernallog_strResponse, devicenameinDesign = execution.devicenameinDesign });
                                        }

                                    }
                                }
                            }
                        }

                        //logsvalue.Add(new logitems { DeviceIP = ipaddress, Logtype = "ilog", Fullresponse = kernallog_strResponse });
                    }
                }

                return logsvalue;
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                //if (ex.Message != "Thread was being aborted.")
                //    MessageBox.Show("Exception\n " + ex.Message, "Error Code - ECLxxxx", MessageBoxButton.OK, MessageBoxImage.Error);

                return null;
            }
        }
        
        public List<logitems> Logverification(logitems iplistfor_other, string passwordkey, ExecutionProcess execProcess)
        {
            List<logitems> logsvalue = new List<logitems>();
            try
            {
                string ilog_strResponse = string.Empty;
                string kernel_strResponse = string.Empty;
                string sip_strResponse = string.Empty;
                string header_event_strResponse = string.Empty;
                string event_strResponse = string.Empty;

                if (iplistfor_other != null)
                {
                    if (iplistfor_other.Logtype == "ilog" & iplistfor_other.Otherips != string.Empty & iplistfor_other.Otherips != "Not Applicable")
                    {
                        HttpGetactual("http://" + iplistfor_other.Otherips + "/log.txt", false, passwordkey, iplistfor_other.Otherips, Properties.Settings.Default.DeviceUsername, out ilog_strResponse, execProcess);

                        if (ilog_strResponse != string.Empty)
                        {
                            if (iplistfor_other.LastLine.Length > 0)
                            {
                                Int32 lastlinelength = iplistfor_other.LastLine.Length;
                                string ilog_log = ilog_strResponse.Substring(ilog_strResponse.IndexOf(iplistfor_other.LastLine) + lastlinelength);
                                logsvalue.Add(new logitems { Otherips = iplistfor_other.Otherips, LastLine = iplistfor_other.LastLine, Logtype = "ilog", TP_directory_log = iplistfor_other.TP_directory_log, TC_directory_log = iplistfor_other.TC_directory_log,TP_timestamp=iplistfor_other.TP_timestamp,TC_timestamp=iplistfor_other.TC_timestamp, Fullresponse = ilog_log, devicenameinDesign = iplistfor_other.devicenameinDesign, coreipaddress = iplistfor_other.coreipaddress });
                            }
                            else
                            {
                                logsvalue.Add(new logitems { Otherips = iplistfor_other.Otherips, LastLine = iplistfor_other.LastLine, Logtype = "ilog", TP_directory_log = iplistfor_other.TP_directory_log, TC_directory_log = iplistfor_other.TC_directory_log, TP_timestamp = iplistfor_other.TP_timestamp, TC_timestamp = iplistfor_other.TC_timestamp, Fullresponse = ilog_strResponse, devicenameinDesign = iplistfor_other.devicenameinDesign, coreipaddress = iplistfor_other.coreipaddress });
                            }
                        }
                    }
                }

                if (iplistfor_other != null)
                {
                    if (iplistfor_other.Logtype == "kernallog" & iplistfor_other.Otherips != string.Empty & iplistfor_other.Otherips != "Not Applicable")
                    {
                        if (execProcess.core_New_FirmwareVersion.Item1 && iplistfor_other.coreipaddress != null && iplistfor_other.coreipaddress != string.Empty)
                            HttpGetactual("http://" + iplistfor_other.Otherips + "/cgi-bin/kernel_log", false, passwordkey, iplistfor_other.Otherips, Properties.Settings.Default.DeviceUsername.ToString(), out kernel_strResponse, execProcess);
                        else                        
                            HttpGetDigestMethod("http://" + iplistfor_other.Otherips + "/cgi-bin/kernel_log", passwordkey, iplistfor_other.Otherips,execProcess, out kernel_strResponse);
                          
                        if (kernel_strResponse != string.Empty)
                        {
                            if (iplistfor_other.LastLine.Length > 0)
                            {
                                Int32 lastlinelength = iplistfor_other.LastLine.Length;
                                string kernallog_log = kernel_strResponse.Substring(kernel_strResponse.IndexOf(iplistfor_other.LastLine) + lastlinelength);
                                logsvalue.Add(new logitems { Otherips = iplistfor_other.Otherips, LastLine = iplistfor_other.LastLine, Logtype = "kernallog", TP_directory_log = iplistfor_other.TP_directory_log, TC_directory_log = iplistfor_other.TC_directory_log, TP_timestamp = iplistfor_other.TP_timestamp, TC_timestamp = iplistfor_other.TC_timestamp, Fullresponse = kernallog_log, devicenameinDesign = iplistfor_other.devicenameinDesign, coreipaddress = iplistfor_other.coreipaddress });
                            }
                            else
                            {
                                logsvalue.Add(new logitems { Otherips = iplistfor_other.Otherips, LastLine = iplistfor_other.LastLine, Logtype = "kernallog", TP_directory_log = iplistfor_other.TP_directory_log, TC_directory_log = iplistfor_other.TC_directory_log, TP_timestamp = iplistfor_other.TP_timestamp, TC_timestamp = iplistfor_other.TC_timestamp, Fullresponse = kernel_strResponse, devicenameinDesign = iplistfor_other.devicenameinDesign, coreipaddress = iplistfor_other.coreipaddress });
                            }
                        }
                    }
                }

                if (iplistfor_other != null)
                {
                    if (iplistfor_other.Logtype == "siplog" & iplistfor_other.Otherips != string.Empty & iplistfor_other.Otherips != "Not Applicable")
                    {
                        HttpGetactual("http://" + iplistfor_other.Otherips + "/sip.txt", false, passwordkey, iplistfor_other.Otherips, Properties.Settings.Default.DeviceUsername, out sip_strResponse, execProcess);

                        if (sip_strResponse != string.Empty)
                        {
                            if (iplistfor_other.LastLine.Length > 0)
                            {
                                Int32 lastlinelength = iplistfor_other.LastLine.Length;
                                string siplog_log = sip_strResponse.Substring(sip_strResponse.IndexOf(iplistfor_other.LastLine) + lastlinelength);
                                logsvalue.Add(new logitems { Otherips = iplistfor_other.Otherips, LastLine = iplistfor_other.LastLine, Logtype = "siplog", TP_directory_log = iplistfor_other.TP_directory_log, TC_directory_log = iplistfor_other.TC_directory_log, TP_timestamp = iplistfor_other.TP_timestamp, TC_timestamp = iplistfor_other.TC_timestamp, Fullresponse = siplog_log, devicenameinDesign = iplistfor_other.devicenameinDesign, coreipaddress = iplistfor_other.coreipaddress });
                            }
                            else
                            {
                                logsvalue.Add(new logitems { Otherips = iplistfor_other.Otherips, LastLine = iplistfor_other.LastLine, Logtype = "siplog", TP_directory_log = iplistfor_other.TP_directory_log, TC_directory_log = iplistfor_other.TC_directory_log, TP_timestamp = iplistfor_other.TP_timestamp, TC_timestamp = iplistfor_other.TC_timestamp, Fullresponse = sip_strResponse, devicenameinDesign = iplistfor_other.devicenameinDesign, coreipaddress = iplistfor_other.coreipaddress });

                            }
                        }
                    }
                }

                if (iplistfor_other != null)
                {
                    if (iplistfor_other.Logtype == "eventlog" & iplistfor_other.Otherips != string.Empty & iplistfor_other.Otherips != "Not Applicable")
                    {
                      
                        if((execProcess.core_New_FirmwareVersion.Item1 == true) && iplistfor_other.Coreversion=="NEW")
                        {
                            HttpGet("http://" + iplistfor_other.Otherips + "/api/v0/systems/1/events?page=1&pageSize=100", true, passwordkey, iplistfor_other.Otherips, string.Empty, out header_event_strResponse, execProcess);
                            if (header_event_strResponse != string.Empty)
                            {
                                if (iplistfor_other.LastLine.Length > 0)
                                {
                                    //  Get new events by split current response using last event id     
                                    List<object> newEvents = new List<object>();
                                    var obj = new JavaScriptSerializer { MaxJsonLength = Int32.MaxValue }.Deserialize<dynamic>(header_event_strResponse);
                                    if (obj.Count > 0)
                                    {
                                        foreach (var res in obj)
                                        {
                                            if (res.Key == "items")
                                            {
                                                foreach (var childobj in res.Value)
                                                {
                                                    if (Convert.ToInt32(childobj["id"].ToString()) > Convert.ToInt32(iplistfor_other.LastLine))
                                                    {
                                                        newEvents.Add(childobj);
                                                    }
                                                }

                                                if (newEvents.Count > 0)
                                                    event_strResponse = Regex.Unescape(new JavaScriptSerializer().Serialize(newEvents));
                                            }
                                        }
                                    }

                                    logsvalue.Add(new logitems { Otherips = iplistfor_other.Otherips, LastLine = iplistfor_other.LastLine, Logtype = "eventlog", TP_directory_log = iplistfor_other.TP_directory_log, TC_directory_log = iplistfor_other.TC_directory_log, TP_timestamp = iplistfor_other.TP_timestamp, TC_timestamp = iplistfor_other.TC_timestamp, Fullresponse = event_strResponse, devicenameinDesign = iplistfor_other.devicenameinDesign, coreipaddress = iplistfor_other.coreipaddress });
                                }
                                else
                                {
                                    logsvalue.Add(new logitems { Otherips = iplistfor_other.Otherips, LastLine = iplistfor_other.LastLine, Logtype = "eventlog", TP_directory_log = iplistfor_other.TP_directory_log, TC_directory_log = iplistfor_other.TC_directory_log, TP_timestamp = iplistfor_other.TP_timestamp, TC_timestamp = iplistfor_other.TC_timestamp, Fullresponse = event_strResponse, devicenameinDesign = iplistfor_other.devicenameinDesign, coreipaddress = iplistfor_other.coreipaddress });
                                }

                            }
                        }
                        else if(iplistfor_other.Coreversion == "OLD" && execProcess.core_New_FirmwareVersion.Item1 == false)
                        {
                            HttpGetactual("http://" + iplistfor_other.Otherips + "/designs/current_design/settings/event_log_head.xml",false, passwordkey, iplistfor_other.Otherips, username, out header_event_strResponse, execProcess);
                            if (header_event_strResponse != string.Empty)
                            {
                                string[] line3 = header_event_strResponse.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
                                string[] lin1e = line3[1].Split(new string[] { "=" }, StringSplitOptions.None);
                                string gtyu = lin1e[1].ToString();
                                string name_header = gtyu.Remove(gtyu.Length - 2);
                                name_header = name_header.Remove(0, 1);
                                HttpGetactual("http://" + iplistfor_other.Otherips + "/designs/current_design/settings/" + name_header,false, passwordkey, iplistfor_other.Otherips, username, out event_strResponse, execProcess);
                                if (event_strResponse != string.Empty)
                                {
                                    if (iplistfor_other.LastLine.Length > 0)
                                    {
                                        Int32 lastlinelength = iplistfor_other.LastLine.Length;
                                        string eventlog_log = event_strResponse.Substring(event_strResponse.IndexOf(iplistfor_other.LastLine) + lastlinelength);
                                        logsvalue.Add(new logitems { Otherips = iplistfor_other.Otherips, LastLine = iplistfor_other.LastLine, Logtype = "eventlog", TP_directory_log = iplistfor_other.TP_directory_log, TC_directory_log = iplistfor_other.TC_directory_log, TP_timestamp = iplistfor_other.TP_timestamp, TC_timestamp = iplistfor_other.TC_timestamp, Fullresponse = eventlog_log, devicenameinDesign = iplistfor_other.devicenameinDesign, coreipaddress = iplistfor_other.coreipaddress });
                                    }
                                    else
                                    {
                                        logsvalue.Add(new logitems { Otherips = iplistfor_other.Otherips, LastLine = iplistfor_other.LastLine, Logtype = "eventlog", TP_directory_log = iplistfor_other.TP_directory_log, TC_directory_log = iplistfor_other.TC_directory_log, TP_timestamp = iplistfor_other.TP_timestamp, TC_timestamp = iplistfor_other.TC_timestamp, Fullresponse = event_strResponse, devicenameinDesign = iplistfor_other.devicenameinDesign, coreipaddress = iplistfor_other.coreipaddress });
                                    }
                                }
                            }
                        }
                        else
                        {
                            logsvalue.Add(new logitems { Otherips = iplistfor_other.Otherips, LastLine = iplistfor_other.LastLine, Logtype = "eventlog", TP_directory_log = iplistfor_other.TP_directory_log, TC_directory_log = iplistfor_other.TC_directory_log, TP_timestamp = iplistfor_other.TP_timestamp, TC_timestamp = iplistfor_other.TC_timestamp, Fullresponse = header_event_strResponse, devicenameinDesign = iplistfor_other.devicenameinDesign, coreipaddress = iplistfor_other.coreipaddress });
                        }
                    }
                }

                if (iplistfor_other != null)
                {
                    if (iplistfor_other.Logtype == "Qsyslog")
                    {
                        string Qsys_lastline = qsyslog("TC_End", iplistfor_other);
                        logsvalue.Add(new logitems { Otherips = iplistfor_other.Otherips, LastLine = iplistfor_other.LastLine, Logtype = "Qsyslog", TP_directory_log = iplistfor_other.TP_directory_log, TC_directory_log = iplistfor_other.TC_directory_log, TP_timestamp = iplistfor_other.TP_timestamp, TC_timestamp = iplistfor_other.TC_timestamp, Fullresponse = Qsys_lastline, devicenameinDesign = iplistfor_other.devicenameinDesign, coreipaddress = iplistfor_other.coreipaddress });
                    }
                }

                if (iplistfor_other != null)
                {
                    if (iplistfor_other.Logtype == "Qsys_UCI")
                    {
                        string UCI_lastline = qsysucilog("TC_End", iplistfor_other);
                        logsvalue.Add(new logitems { Otherips = iplistfor_other.Otherips, LastLine = iplistfor_other.LastLine, Logtype = "Qsys_UCI", TP_directory_log = iplistfor_other.TP_directory_log, TC_directory_log = iplistfor_other.TC_directory_log, TP_timestamp = iplistfor_other.TP_timestamp, TC_timestamp = iplistfor_other.TC_timestamp, Fullresponse = UCI_lastline, devicenameinDesign = iplistfor_other.devicenameinDesign, coreipaddress = iplistfor_other.coreipaddress });
                    }
                }

                if (iplistfor_other != null)
                {
                    if (iplistfor_other.Logtype == "Windows_event_log")
                    {
                        string windows_event_log = win_event(string.Empty, iplistfor_other.TC_directory_log);
                        if (!string.IsNullOrEmpty(windows_event_log))
                        {
                            logsvalue.Add(new logitems { Otherips = "Windows_event_log", LastLine = string.Empty, Logtype = "Windows_event_log", TP_directory_log = iplistfor_other.TP_directory_log, TC_directory_log = iplistfor_other.TC_directory_log, TP_timestamp = iplistfor_other.TP_timestamp, TC_timestamp = iplistfor_other.TC_timestamp, Fullresponse = windows_event_log, devicenameinDesign = iplistfor_other.devicenameinDesign, coreipaddress = iplistfor_other.coreipaddress });
                        }
                    }
                }

                return logsvalue;
            }

            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                //if (ex.Message != "Thread was being aborted.")
                //    MessageBox.Show("Exception\n " + ex.Message, "Error Code - ECLxxxx", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }

        private List<logitems> GetAllReflectDeviceDetails_log(List<string> devices, List<DUT_DeviceItem> selectedDutDeviceItemList)
        {
            List<logitems> logsvalue = new List<logitems>();
            try
            {
                string coreip_name_indesign = string.Empty;
                string coreid = string.Empty;

                foreach (string finddeviceip in devices)
                {
                    foreach (DUT_DeviceItem item in selectedDutDeviceItemList)
                    {
                        if (item.ItemDeviceName.Equals(finddeviceip, StringComparison.CurrentCultureIgnoreCase))
                        {
                            if (item.QREMcoredetails != null && item.QREMcoredetails.Count() > 0)
                            {
                                if (item.ItemDeviceType.Contains("Core"))
                                {
                                    coreid = item.QREMcoredetails[1] + ";" + item.QREMcoredetails[2] + ";" + item.QREMcoredetails[3];
                                    coreip_name_indesign = item.ItemDeviceName;
                                    logsvalue.Add(new logitems { Otherips = coreid, coreipaddress = coreid, corename_indesign = coreip_name_indesign, devicenameinDesign = item.ItemDeviceName });
                                    break;
                                }
                            }
                        }
                    }
                }

                //logsvalue.Add(new logitems { ipforlogs = temp, coreipaddress = coreip, corename_indesign = coreip_name_indesign, temp_devicenameinDesign = temp_name, totaldevices_in_design = Alldevices_list, temp_build = Alldevices_build, Design_name = Design_name});

                return logsvalue;
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                //if (ex.Message != "Thread was being aborted.")
                //    MessageBox.Show("Exception\n \n" + ex.Message, "QAT Error Code - ECLxxxx", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;

            }
            //}
        }

        private List<logitems> GetAllIpForSelectedAction_log(List<string> devices, List<DUT_DeviceItem> selectedDutDeviceItemList)
        {
            List<logitems> logsvalue = new List<logitems>();
            try
            {
                string Date_time_writelogs_TP = string.Format("{0:[MM-dd-yyyy_hh-mm-ss]}", DateTime.Now);

                string coreip_name_indesign = string.Empty;
                string coreip = string.Empty;
                string Design_name = string.Empty;
                //string Coreversion = string.Empty;
                List<string> temp = new List<string>();
                List<string> temp_name = new List<string>();
                List<string> temp_build = new List<string>();
                string Alldevices_list = string.Empty;
                //string Alldevices_build = string.Empty;
                //Alldevices_list = string.Join(",", devices.ToArray());
                //Alldevices_build = string.Join(",", temp_build.ToArray());
                foreach (string finddeviceip in devices)
                {
                    foreach (DUT_DeviceItem item in selectedDutDeviceItemList)
                    {
                        if (item.ItemDeviceName.Equals(finddeviceip, StringComparison.CurrentCultureIgnoreCase))
                        {
                            if (item.ItemPrimaryIPSelected != null)
                            {
                                if (item.ItemDeviceType.Contains("Core"))
                                {
                                    coreip = item.ItemPrimaryIPSelected;
                                    coreip_name_indesign = item.ItemDeviceName;
                                    Design_name = item.ItemCurrentDesign;
                                    temp_build.Add(item.ItemCurrentBuild);
                                    logsvalue.Add(new logitems { Otherips = coreip, coreipaddress = coreip, corename_indesign = coreip_name_indesign, devicenameinDesign = item.ItemDeviceName});

                                }
                                else
                                {
                                    //if (!temp.Contains(item.ItemPrimaryIPSelected))
                                    //{
                                        temp.Add(item.ItemPrimaryIPSelected);
                                        temp_name.Add(item.ItemDeviceName);
                                        temp_build.Add(item.ItemCurrentBuild);
                                        logsvalue.Add(new logitems { Otherips = item.ItemPrimaryIPSelected, coreipaddress = string.Empty, corename_indesign = coreip_name_indesign, devicenameinDesign = item.ItemDeviceName});
                                    //}
                                }


                            }
                            else if (item.ItemSecondaryIPSelected != null)
                            {
                                if (item.ItemDeviceType.Contains("Core"))
                                {
                                    coreip = item.ItemSecondaryIPSelected;
                                    coreip_name_indesign = item.ItemDeviceName;
                                    Design_name = item.ItemCurrentDesign;
                                    temp_build.Add(item.ItemCurrentBuild);
                                    logsvalue.Add(new logitems { Otherips = coreip, coreipaddress = coreip, corename_indesign = coreip_name_indesign, devicenameinDesign = item.ItemDeviceName});
                                }
                                else
                                {
                                    //if (!temp.Contains(item.ItemSecondaryIPSelected))
                                    //{
                                        temp.Add(item.ItemSecondaryIPSelected);
                                        temp_name.Add(item.ItemDeviceName);
                                        temp_build.Add(item.ItemCurrentBuild);
                                        logsvalue.Add(new logitems { Otherips = item.ItemSecondaryIPSelected, coreipaddress = string.Empty, corename_indesign = coreip_name_indesign, devicenameinDesign = item.ItemDeviceName});
                                    //}
                                }
                            }
                        }
                    }
                }
                
                //logsvalue.Add(new logitems { ipforlogs = temp, coreipaddress = coreip, corename_indesign = coreip_name_indesign, temp_devicenameinDesign = temp_name, totaldevices_in_design = Alldevices_list, temp_build = Alldevices_build, Design_name = Design_name});
                
                return logsvalue;
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                //if (ex.Message != "Thread was being aborted.")
                //    MessageBox.Show("Exception\n \n" + ex.Message, "QAT Error Code - ECLxxxx", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;

            }
            //}
        }        
        
        public bool check_error(DirectoryInfo direct)
        {
            try
            {
                direct.GetFiles(".qsys_uci.log");
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

        public bool checkreadaccess(string path_folder)
        {
            DirectoryInfo directory = new DirectoryInfo(path_folder);
            if (directory.Exists)
            {
                try
                {
                    var acl = directory.GetAccessControl();
                    return true;
                }
                catch (UnauthorizedAccessException ex)
                {
                    if (ex.Message.Contains("read-only"))
                    {

                        return true;
                    }
                }
            }

            return false;
        }

        public string win_event(string from, DateTime from_time)
        {
            try
            {
               //string input_time= string.Format(from_time, "{0:[MM/dd/yyyy_hh:mm:ss]}");
                string win_temp = string.Empty;
                //DateTime now = Convert.ToDateTime(from_time);
                System.Diagnostics.EventLog win_event_log = new System.Diagnostics.EventLog();
                win_event_log.Log = "Application";
                foreach (System.Diagnostics.EventLogEntry logmessages in win_event_log.Entries)
                {
                    if (logmessages.TimeGenerated > from_time)
                    {
                        if (logmessages.Message.Contains("Q-Sys Designer.exe"))
                        {

                            win_temp += logmessages.Message.ToString() + Environment.NewLine;
                        }
                    }
                }

                return win_temp;
            }

            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                //if (ex.Message != "Thread was being aborted.")
                //    MessageBox.Show("Exception\n \n" + ex.Message, "QAT Error Code - ECLxxxx", MessageBoxButton.OK, MessageBoxImage.Error);
                return string.Empty;
            }
        }
    }
    public class logitems
    {
       
        public string Logtype { get; set; }
        public string LastLine { get; set; }
        public string Fullresponse { get; set; }
        public string coreipaddress { get; set; }
        public string corename_indesign { get; set; }

        public string devicenameinDesign { get; set; }
        public string totaldevices_in_design { get; set; }
        //public string Design_name { get; set; }
        public string Coreversion { get; set; }
        //public string temp_build { get; set; }

        public string Otherips { get; set; }
        public string status { get; set; }
        public string TP_timestamp { get; set; }
        public string TC_timestamp { get; set; }
        public DateTime TP_directory_log { get; set; }
        public DateTime TC_directory_log { get; set; }
    }
}
