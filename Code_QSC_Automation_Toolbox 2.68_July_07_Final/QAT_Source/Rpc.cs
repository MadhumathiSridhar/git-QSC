using QSC_Test_Automation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Windows;
using System.Windows.Input;

//using Newtonsoft.Json;

namespace QRAPI
{
    public partial class Rpc
    {
        //public string resp;
        //public string opresp;
        //public string ipresp;
        static int id = 100;

        public object ReadResponseObject(NetworkStream stream)
        {
            string opresp = string.Empty;
            try
            {
                List<byte> bytes = new List<byte>();
                while (true)
                {
                    byte b = (byte)stream.ReadByte();
                    if (b == 0) break;
                    bytes.Add(b);
                }

                string resp = Encoding.UTF8.GetString(bytes.ToArray());
                opresp = Encoding.UTF8.GetString(bytes.ToArray());
                Console.WriteLine("response : \r\n{0}", resp);
                var obj = new JavaScriptSerializer { MaxJsonLength = Int32.MaxValue }.DeserializeObject(resp);
                Console.WriteLine("  parse OK\r\n");
                return obj;
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine("  parse error : {0}\r\n", ex.Message);
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif

                return null;
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif

                return null;
            }
        }
        public string Send_check(string ip, string method, object data, bool checkcursor)
        {
            //bool returnStatus = false;
            string resp = string.Empty;
            int trial = 0;
            bool ack = false;

            try
            {
                //bool isAvailable = false;
                //isAvailable = PingHost(ip, 1710);
                TcpClient client = new TcpClient(ip, 1710) { ReceiveTimeout = 5000 };
                if (client.Connected)
                {                
                    var stream = client.GetStream();

                    // send logon
                    //Send(stream, "Logon", new { User = "NewUser", Password = "1000" });
                    while ((!ack) && (trial < 4))
                    {
                        ack = Send(stream, method, data);
                        if (ack)
                        {
                            // read response

                            var response = ReadResponse(stream);
                            if (response.Item1)
                                resp = response.Item2;
                            string expr = "\"DesignCode\":\"(.+?)\"";
                            string mc = Regex.Match(resp, expr).Groups[1].Value;
                            resp = mc;

                        }
                        else
                        {
                            trial++;
                        }
                    }
                }
                else
                {

                    resp = string.Empty;
                }
            }
            catch (Exception ex)
            {
                if (checkcursor == true)
                    Mouse.OverrideCursor = null;

                //if(ex.Message.Contains("No connection could be made because the target machine actively refused it"))
                //MessageBox.Show("Exception\n Core is refusing the control commands", "QAT Error Code - EC07005", MessageBoxButton.OK, MessageBoxImage.Error);
                //else
                //    MessageBox.Show("Exception\n"+ex.Message, "QAT Error Code - EC07005", MessageBoxButton.OK, MessageBoxImage.Error);

                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif


                resp = string.Empty;
            }
            return resp;
        }
        public Tuple<bool,string> ReadResponse(NetworkStream stream)
        {
            string resp = string.Empty;
            string ipresp = string.Empty;
            try
            {
                List<byte> bytes = new List<byte>();
                while (true)
                {
                    byte b = (byte)stream.ReadByte();
                    if ((b == 0)) break;
                    if(b==255)
                    {
                        break;
                    }
                    bytes.Add(b);
                }

                resp = Encoding.UTF8.GetString(bytes.ToArray());
                ipresp = Encoding.UTF8.GetString(bytes.ToArray());
                //Console.WriteLine("response : \r\n{0}", resp);
                new JavaScriptSerializer { MaxJsonLength = Int32.MaxValue }.DeserializeObject(resp);
                Console.WriteLine("  parse OK\r\n");
                return new Tuple< bool,string> (true, resp);
            }
            catch (ArgumentException ex)
            {
                //Console.WriteLine("  parse error : {0}\r\n", ex.Message);
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                //MessageBox.Show("Exception\n " + ex.Message, "QAT Error Code - EC07003", MessageBoxButton.OK, MessageBoxImage.Error);
                return new Tuple<bool, string>(true, resp);
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif

                return new Tuple<bool, string>(false, resp);
            }
        }

        public bool Send(NetworkStream stream, string method, object data)
        {
            try
            {
                var rpc = new
                {
                    jsonrpc = "2.0",
                    method = method,
                    @params = data,
                    id = id++
                };
               
                string str = new JavaScriptSerializer { MaxJsonLength = Int32.MaxValue }.Serialize(rpc);
                //string str = "{\"jsonrpc\":\"2.0\",\"method\":\"Control.Get\",\"params\":[\")f;&amp;b;j,@!^A&quot;c?8&apos;Vm1_bypass\"],\"id\":100}";
                byte[] bs = Encoding.UTF8.GetBytes(str);
                stream.Write(bs, 0, bs.Length);
                stream.WriteByte(0); // null terminate
                return true;
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                //MessageBox.Show("Exception\n " + ex.Message, "QAT Error Code - EC07004", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }            
        }

        
        public Tuple<bool, string> Send_Emulation_check(string ip, string method, string checkdesignName, string checkdesigncode)
        {
            bool returnStatus = false;
            string resp = string.Empty;
            int trial = 0;
            bool ack = false;
            string platform = string.Empty;
            string designName = string.Empty;
            string designCode = string.Empty;
            bool isEmulator = false;
            try
            {             
                TcpClient client = new TcpClient(ip, 1710) { ReceiveTimeout = 5000 };
                if (client.Connected == true)
                {                  
                    var stream = client.GetStream();
                    while ((!ack) & (trial < 4))
                    {
                        ack = Send(stream, method,"");
                        if (ack)
                        {
                            // read response
                            bool responseStatus = false;
                            var response = ReadResponse(stream);
                            responseStatus = response.Item1;
                            if (responseStatus)
                            {
                              
                                dynamic result = new JavaScriptSerializer { MaxJsonLength = Int32.MaxValue }.DeserializeObject(response.Item2);
                                foreach (var item in result)
                                {
                                    if(item.Key== "params" && item.Value.Count>0)
                                    {
                                        foreach(var childitem in item.Value)
                                        {
                                            if(childitem.Key== "Platform")                                            
                                                platform = childitem.Value;
                                            if (childitem.Key == "DesignName")
                                                designName = childitem.Value;
                                            if (childitem.Key == "DesignCode")
                                                designCode = childitem.Value;
                                            if (childitem.Key == "IsEmulator")
                                                isEmulator = childitem.Value;                                          
                                        }
                                    }
                                }

                                if (checkdesignName.EndsWith(".qsys"))
                                    checkdesignName= checkdesignName.Remove(checkdesignName.Length - 5, 5);

                                if(platform== "Emulator" && isEmulator==true )
                                {
                                    if (designName == checkdesignName)
                                    {                                       
                                        if(designCode == checkdesigncode)
                                            returnStatus = true;
                                        else
                                            resp = "Design code is wrong : " + designCode;
                                    }
                                    else
                                    {
                                        resp = "Design name is wrong : " + designName;
                                    }                                   
                                }
                                else
                                {
                                    resp = "isEmulator parameter is false in response";
                                }
                               
                            }                      
                          
                        }
                        else
                        {
                            trial++;
                            Thread.Sleep(5000);
                        }
                    }
                }
                else
                {
                    returnStatus = false;
                    resp = "Emulation error.";
                }
            }
            catch (Exception ex)
            {
                returnStatus = false;
                resp = "Emulation error.";

                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif

            }
            return new Tuple<bool, string>(returnStatus, resp);
        }

        public Tuple<bool,string> Send(string ip, string method, object data, bool checkcursor, string fromfunction)
        {
            bool returnStatus = false;
            string resp = string.Empty;
            int trial = 0;
            bool ack = false;
            int responseTrial = 0;
            int mainresponseTrial = 0;
            string responseString = string.Empty;
            try
            {
                //bool isAvailable = false;
                //isAvailable = PingHost(ip, 1710);
                TcpClient client = new TcpClient(ip, 1710) { ReceiveTimeout = 5000 };             
                if (client.Connected == true)
                {
                    if(fromfunction== "TCInitialization")
                    {
                        var streamsetTranslate = client.GetStream();
                        bool acknowledge = Send(streamsetTranslate, "Control.SetTranslate", false);
                        if (acknowledge)
                        {
                            int readcount = 0;
                            Recheck:
                            var response1 = ReadResponse(streamsetTranslate);
                            if(response1.Item1 && readcount == 0)
                            {
                                readcount++;
                                goto Recheck;
                            }
                        }
                    }
                    // send logon
                    //Send(stream, "Logon", new { User = "NewUser", Password = "1000" });
                    var stream = client.GetStream();
                    while ((!ack) & (trial < 4))
                    {
                        ack = Send(stream, method, data);
                        if (ack)
                        {
                            // read response
                            RecheckResult:
                            bool responseStatus = false;
                            var response = ReadResponse(stream);
                            responseStatus = response.Item1;
                            if (responseStatus)
                            {
                               
                                var Respkey = new JavaScriptSerializer { MaxJsonLength = Int32.MaxValue }.Deserialize<jsonReadControlList>(response.Item2);
                                if (Respkey.result != null || Respkey.error != null)
                                {
                                    returnStatus = true;
                                    resp = response.Item2.Trim();
                                }
                                else if (Respkey.result == null && mainresponseTrial < 40)
                                {
                                    mainresponseTrial++;
                                    Thread.Sleep(2000);
                                    goto RecheckResult;
                                }
                                else
                                {
                                    returnStatus = false;
                                    resp = string.Empty;
                                }
                            }
                            else if (mainresponseTrial < 40)
                            {
                                mainresponseTrial++;
                                Thread.Sleep(2000);
                                goto RecheckResult;
                            }
                            else
                            {
                                returnStatus = false;
                                resp = string.Empty;
                            }
                        }
                        else
                        {
                            trial = trial + 1;
                            Thread.Sleep(5000);
                        }
                    }
                }
                else
                {
                    returnStatus = false;
                    resp = "Tcp client connection failed at port 1710.";
                }
            }
            catch (Exception ex)
            {
                if(checkcursor==true)
                Mouse.OverrideCursor = null;

                //if(ex.Message.Contains("No connection could be made because the target machine actively refused it"))
                //MessageBox.Show("Exception\n Core is refusing the control commands", "QAT Error Code - EC07005", MessageBoxButton.OK, MessageBoxImage.Error);
                //else
                //    MessageBox.Show("Exception\n"+ex.Message, "QAT Error Code - EC07005", MessageBoxButton.OK, MessageBoxImage.Error);

                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif

                returnStatus = false;
                resp = string.Empty;
            }
            return new Tuple< bool,string> (returnStatus, resp);
        }

        public static bool PingHost(string _HostURI, int _PortNumber)
        {
            try
            {
                var client = new TcpClient();
                var result = client.BeginConnect(_HostURI, _PortNumber, null, null);

                var success = result.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(1));

                if (!success)
                {
                    DeviceDiscovery.WriteToLogFile("Error pinging host:'" + _HostURI + ":" + _PortNumber.ToString() + "'");
                    return false;
                }

                // we have connected
                client.EndConnect(result);
                return true;
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Error pinging host:'" + _HostURI + ":" + _PortNumber.ToString() + "'");
                return false;
            }
        }
    }
}
