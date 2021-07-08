using System;
using System.Collections.Generic;
using System.Text;

using System.Net;
using System.Net.Sockets;
using System.Diagnostics;
using System.Threading;
using System.IO;
using QSC_Test_Automation;

namespace Utility
{
    public class Telnet_Comm
    {

        public Telnet_Comm()
        {
            //Telnet_Comm tc = new Telnet_Comm();
            //string[] strResponseArray = new string[0];
            //string[] strMessageQue = new string[] { "", "root", "decline", "cat /var/etc/hw_config" };
            //tc.session("172.16.4.72", strMessageQue, out strResponseArray);
        }

        ~Telnet_Comm()
        {
            try
            {
                if (m_Client != null)
                {
                    Thread.Sleep(500);
                    m_Client.Close();
                    DeviceDiscovery.WriteToLogFile("telnet is closed at start at constructor");
                }
            }
            catch
            {
                //this could happen to you
            }
        }

        byte[] m_bytData;
        NetworkStream m_Stream;
        string m_strResponseData = "";
        Int32 m_intNumBytes = 0;

        int m_intPort = 23;
        int m_videointPort = 6133;

        System.Net.Sockets.TcpClient m_Client;

        public void connect_to_server(string strIP, bool isVideoGenTrue)
        {
            IPAddress ipAdd = IPAddress.Parse(strIP);
            try
            {
                if (m_Client != null)
                {
                    Thread.Sleep(500);
                    m_Client.Close();
                    DeviceDiscovery.WriteToLogFile("telnet is closed at start at connect to server try block");
                }
            }
            catch
            {
                DeviceDiscovery.WriteToLogFile("telnet hits connect to server catch");
            }
            finally
            {
                Thread.Sleep(QSC_Test_Automation.Properties.Settings.Default.Dev_mode_ssh_delay);
                m_Client = new System.Net.Sockets.TcpClient();

                if (!isVideoGenTrue)
                    m_Client.Connect(strIP, m_intPort);
                else
                    m_Client.Connect(strIP, m_videointPort);

            }
        }

        public bool connected()
        {
            return m_Client.Connected;
        }

        public void send_message(string strMessage)
        {
            try
            {
                m_bytData = System.Text.Encoding.ASCII.GetBytes(strMessage);

                //get a client stream for reading and writing
                m_Stream = m_Client.GetStream();

                //send the message to the connected TcpServer
                m_Stream.Write(m_bytData, 0, m_bytData.Length);
            }
            catch (Exception ex)
            {
                if (m_Client != null)
                {
                    Thread.Sleep(500);
                    m_Client.Close();
                    DeviceDiscovery.WriteToLogFile("telnet is closed at send message catch");
                }
                //DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                //MessageBox.Show("Exception\n " + ex.Message, "QAT Error Code - EC03021", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public string get_response()
        {
			string str = string.Empty;

            try
            {
                m_bytData = new byte[1024];

                using (MemoryStream memoryStream = new MemoryStream())
                {
                    m_Stream.ReadTimeout = 10000;

                    int iByteSize = 0;
                    do
                    {
                        iByteSize = m_Stream.Read(m_bytData, 0, m_bytData.Length);
                        memoryStream.Write(m_bytData, 0, iByteSize);
                    } while (m_Stream.DataAvailable);

                    str = Encoding.ASCII.GetString(memoryStream.ToArray(), 0, (int)memoryStream.Length);
                    //int a= str.IndexOf("\r\n\u001b[01;32mroot");    

                    string[] seperator =null;
                    if (str.Contains("\r\n\u001b"))
                        seperator =  new string[] { "\r\n\u001b" };
                    else
                     seperator =  new string[] { "\r\n~ #" };    
                                  
                    string[] result = str.Split(seperator, StringSplitOptions.None);
                    if (result[0] != null && result[0] != string.Empty)
                        str = result[0];                

                }

                return str;
            }
            catch (Exception ex)
            {
                if (m_Client != null)
                {
                    Thread.Sleep(500);
                    m_Client.Close();
                    DeviceDiscovery.WriteToLogFile("telnet is closed at getresponse catch");
                }
                if (ex.HResult== -2146232800)
                {
                    return "Unable to read data from the transport connection";
                }
                return "";
            }
			
            //m_bytData = new byte[1024];

            ////string to store the response ASCII representation
            //m_strResponseData = null;

            ////read the first batch of the TcpServer response bytes

            //try
            //{
            //    m_Stream.ReadTimeout = 10000;
            //    m_intNumBytes = m_Stream.Read(m_bytData, 0, m_bytData.Length);
            //    m_strResponseData = System.Text.Encoding.ASCII.GetString(m_bytData, 0, m_intNumBytes);
            //    return m_strResponseData;
            //}
            //catch (Exception ex)
            //{
            //return "";
            //}
        }

        public bool session(string strIP, string[] strMessageQue, string firstcommand, bool isVideoGenTrue, out string[] strReturnQue)
        {
            strReturnQue = new string[strMessageQue.Length];

            try
            {                
                connect_to_server(strIP, isVideoGenTrue);
                Thread.Sleep(200);
                if (connected())
                {
                    for (int i = 0; i < strMessageQue.Length; i++)
                    {
                        send_message(strMessageQue[i] + "\r" + "");

                        //if (i == 2)
                        //{
                        //    for (int j = 0; j < 10; j++)
                        //    {
                        //        //Thread.Sleep(500);

                        //        Debug.WriteLine(strIP + j);

                        //        strReturnQue[i] = get_response();

                        //        if (strReturnQue[i].Contains("Login incorrect"))
                        //        {
                        //            break;
                        //        }
                        //        else if (strReturnQue[i].Contains("root@"))
                        //        {
                        //            break;
                        //        }
                        //    }
                        //}
                        //else
                        //{

                        //Thread.Sleep(500);

                        strReturnQue[i] = get_response();

                        int k = 0;
                        while (strReturnQue[i]=="\r\n" && k < 5)
                        {
                            k++;
                            //Thread.Sleep(500);
                            strReturnQue[i] = get_response();
                        }

                        //}
                    }

                    //for (int i = 0; i < strMessageQue.Length; i++)
                    //{
                    //    send_message(strMessageQue[i] + "\r" + "");

                    //    if (i == 2)
                    //    {
                    //        //for (int j = 0; j < 5; j++)
                    //        //{
                    //        Thread.Sleep(4000);
                    //        //strReturnQue[i] = get_response();
                    //        //strReturnQue[i] = get_response();
                    //        //if (strReturnQue[i].Contains("Login incorrect"))
                    //        //{
                    //        //    break;
                    //        //}
                    //        //else if (strReturnQue[i].Contains("root@"))
                    //        //{
                    //        //    break;
                    //        //}
                    //        //}
                    //    }
                    //    else
                    //    {
                    //        Thread.Sleep(500);
                    //    }

                    //    //if (i == 3)

                    //    strReturnQue[i] = get_response();
                    //}
                    if (m_Client != null)
                    {
                        Thread.Sleep(500);
                        m_Client.Close();
                    //    m_Client = null;
                        DeviceDiscovery.WriteToLogFile("telnet is closed properly");                   }

                }
                else
                {
                    if (m_Client != null)
                    {
                        Thread.Sleep(500);
                        m_Client.Close();
                        DeviceDiscovery.WriteToLogFile("telnet is closed as connected returns false");
                    }
                }
            }
            catch (Exception e)
            {
               
                    if (m_Client != null)
                    {
                    Thread.Sleep(500);
                        m_Client.Close();
                    DeviceDiscovery.WriteToLogFile("telnet is closed at session catch");
                }

                    if ((isVideoGenTrue)&& (e.HResult == -2147467259))
                        strReturnQue[1] = "Connection Refused";
                if ((!isVideoGenTrue) && (e.HResult == -2147467259))
                    strReturnQue[1] = "QSC device Connection Refused";


                Debug.WriteLine("Error in Telnet_Comm: session: " + e.ToString());
                return false;
            }

            return true;
        }
    }
}
