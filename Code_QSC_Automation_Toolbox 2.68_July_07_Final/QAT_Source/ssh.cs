using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Renci.SshNet;
using System.Security.Cryptography;
using System.Threading;

namespace QSC_Test_Automation
{
    class ssh
    {
        SshClient ssh_client = null;
              
        public bool session(string strIP, string command,string sshPrivatekeyFilePath, out  string[] strReturnQue)
        {
            strReturnQue = new string[3];           
            string remarks = string.Empty;
            int keyfileretrycount = 0;
            int retrycount_EmptyResponse = 0;  
            try
            {
                ////decide which private key file to use 
                if(sshPrivatekeyFilePath == string.Empty)
                 sshPrivatekeyFilePath = Path.Combine(Properties.Settings.Default.ServerPath, "sshkey2.txt");   

                retryWithNewKeyfile:

                if (connect(strIP, sshPrivatekeyFilePath, out remarks))
                {
                    bool isvalidcommand = send_receive(command, ssh_client, out strReturnQue);

                    if(retrycount_EmptyResponse == 0 && isvalidcommand == false && (strReturnQue[1] == "Message type 80"))
                    {
                        Thread.Sleep(10000);
                        goto retryWithNewKeyfile;
                    }

                    strReturnQue[2] = sshPrivatekeyFilePath;
                    return isvalidcommand;
                }
                else
                {
                    if(remarks== "Message type 80")
                    {
                        Thread.Sleep(10000);
                        goto retryWithNewKeyfile;
                    }
                    if(remarks== "Permission denied (publickey)" && keyfileretrycount == 0)
                    {
                        ////change private key file to recheck                      
                        FileInfo filepath = new FileInfo(sshPrivatekeyFilePath);
                        if (filepath.Name == "sshkey2.txt")
                            sshPrivatekeyFilePath = Path.Combine(Properties.Settings.Default.ServerPath, "sshkey.txt");
                        else
                            sshPrivatekeyFilePath = Path.Combine(Properties.Settings.Default.ServerPath, "sshkey2.txt");

                        keyfileretrycount++;
                        goto retryWithNewKeyfile;
                    }

                    strReturnQue[1] = remarks;
                    return false;
                }                
            }
            catch(Exception ex)
            {
                DeviceDiscovery.WriteToLogFile(ex.Message);

                strReturnQue[1] = ex.Message;

                if (ssh_client != null)
                { ssh_client.Disconnect(); }
                return false;
            }
        }

        private bool connect(string strIP,string sshPrivatekeyFilePath, out string remarks)
        {
            remarks = string.Empty;

            try
            {
                PrivateKeyFile keyFile = null;
                //string keyfilePath = Path.Combine(Properties.Settings.Default.ServerPath,"sshkey.txt");
                FileInfo dirInfo = new FileInfo(sshPrivatekeyFilePath);

                if (Directory.Exists(dirInfo.DirectoryName))
                {
                    if (checkreadaccess(dirInfo.DirectoryName))
                    {
                        if (File.Exists(dirInfo.FullName))
                        {
                            try
                            {
                                string originalData = string.Empty;
                                using (StreamReader read = new StreamReader(dirInfo.FullName))
                                {
                                    var decryptInput = read.ReadToEnd();
                                    originalData = decrypts(decryptInput);
                                }

                                byte[] byteArray = Encoding.ASCII.GetBytes(originalData);
                                MemoryStream stream = new MemoryStream(byteArray);

                                keyFile = new PrivateKeyFile(stream);

                                //if (!Directory.Exists(@"C:\Users\" + Environment.UserName + @"\AppData\Local\Temp\QSys Temp Files\Temp_" + exid + @"\SSHPrivateKey\"))
                                //    Directory.CreateDirectory(@"C:\Users\" + Environment.UserName + @"\AppData\Local\Temp\QSys Temp Files\Temp_" + exid + @"\SSHPrivateKey\");

                                //using (StreamWriter write = new StreamWriter(@"C:\Users\" + Environment.UserName + @"\AppData\Local\Temp\QSys Temp Files\Temp_" + exid + @"\SSHPrivateKey\key_Temp.txt", false))
                                //{
                                //    write.Write(originalData);
                                //}

                                //keyFile = new PrivateKeyFile(@"C:\Users\" + Environment.UserName + @"\AppData\Local\Temp\QSys Temp Files\Temp_" + exid + @"\SSHPrivateKey\key_Temp.txt");
                                
                            }
                            catch (Exception ex)
                            {
                                DeviceDiscovery.WriteToLogFile("Invalid ssh key file");
                                remarks = "Invalid ssh key file";
                            }
                        }
                        else
                        {
                            DeviceDiscovery.WriteToLogFile("ssh Key file not exist");
                            remarks = "ssh Key file not exist";
                        }
                    }
                    else
                    {
                        DeviceDiscovery.WriteToLogFile("ssh Key file folder access denied");
                        remarks = "Ssh Key file folder access denied";
                    }
                }
                else
                {
                    DeviceDiscovery.WriteToLogFile("Ssh Key file folder not exist");
                    remarks = "Ssh Key file folder not exist";
                }
                
                if(remarks != string.Empty)
                {
                    if (ssh_client != null)
                    { ssh_client.Disconnect(); }
                    return false;
                }

                var keyFiles = new[] { keyFile };
                var username = "root";
                var port = 22;

                var methods = new List<AuthenticationMethod>();
                //methods.Add(new PasswordAuthenticationMethod("root", "decline"));

                try
                {
                    methods.Add(new PrivateKeyAuthenticationMethod(username, keyFiles));
                }
                catch
                {
                    DeviceDiscovery.WriteToLogFile("Incorrect ssh credential");
                    remarks = "Incorrect ssh credential";
                    if (ssh_client != null)ssh_client.Disconnect();
                    return false;
                }
                int retrycount = 0;
                var con = new ConnectionInfo(strIP, port, username, methods.ToArray());
                //var con = new ConnectionInfo(strIP, username, methods.ToArray());
                ssh_client = new SshClient(con);

                connectAgain:
                ssh_client.Connect();
                if (ssh_client.IsConnected)
                    return true;
                else
                {
                    DeviceDiscovery.WriteToLogFile("Unable to establish ssh client connection--------- ssh_client.IsConnected is false------------- " + "Ip:" + strIP + " Port:" + port + " username:" + username);
                    if (retrycount < 5)
                    {
                        Thread.Sleep(2000);
                        retrycount++;
                        goto connectAgain;                      
                    }
                    remarks = "Unable to establish ssh client connection";
                    return false;
                }
            }
            catch (Exception ex)
            {
                if(ex.Message.Contains("Message type 80"))
                {
                    DeviceDiscovery.WriteToLogFile("While SSH Connect ---"+ex.Message);
                    remarks = "Message type 80";
                }
                else  if (ex.HResult == -2147467259)
                {
                    DeviceDiscovery.WriteToLogFile("QSC device Connection Refused");
                    remarks = "QSC device Connection Refused";
                }
                else if (ex.Message.Contains("No such host is known"))
                {
                    DeviceDiscovery.WriteToLogFile("Device is not available in the network : " + strIP);
                    remarks = "Device is not available in the network : " + strIP;
                }
                else if (ex.Message.Contains("Socket read operation has timed out"))
                {
                    DeviceDiscovery.WriteToLogFile("Invalid ssh port");
                    remarks = "Invalid ssh port";
                }
                else if (ex.Message.Contains("Permission denied (publickey)"))
                {
                    DeviceDiscovery.WriteToLogFile(ex.Message);
                    remarks = "Permission denied (publickey)";
                }
                else
                {
                    DeviceDiscovery.WriteToLogFile(ex.Message);
                    remarks = "Could not connect ssh";
                }

                if (ssh_client != null)
                { ssh_client.Disconnect(); }
                return false;
            }
            //finally
            //{
            //    if (Directory.Exists(@"C:\Users\" + Environment.UserName + @"\AppData\Local\Temp\QSys Temp Files\Temp_" + exid + @"\SSHPrivateKey") && File.Exists(@"C:\Users\" + Environment.UserName + @"\AppData\Local\Temp\QSys Temp Files\Temp_" + exid + @"\SSHPrivateKey\key_Temp.txt"))
            //        File.Delete(@"C:\Users\" + Environment.UserName + @"\AppData\Local\Temp\QSys Temp Files\Temp_" + exid + @"\SSHPrivateKey\key_Temp.txt");
            //}
        }

        public static string decrypts(string cipherText)
        {
            try
            {
                string EncryptionKey = "QSCencryptionPrivateKeyFordevelopingpurpose";
                cipherText = cipherText.Replace(" ", "+");
                byte[] cipherBytes = Convert.FromBase64String(cipherText);

                using (Aes encryptor = Aes.Create())
                {
                    Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(EncryptionKey, new byte[] { 0x6a, 0x61, 0x73, 0x6d, 0x69, 0x6e, 0x69, 0x6e, 0x66, 0x6f, 0x74, 0x65, 0x63, 0x68 });
                    encryptor.Key = pdb.GetBytes(32);
                    encryptor.IV = pdb.GetBytes(16);
                    using (MemoryStream ms = new MemoryStream())
                    {
                        using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateDecryptor(), CryptoStreamMode.Write))
                        {
                            cs.Write(cipherBytes, 0, cipherBytes.Length);
                            cs.Close();
                        }

                        cipherText = Encoding.Unicode.GetString(ms.ToArray());
                    }
                }
            }
            catch(Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Decryption is failed : " + ex.Message);
            }

            return cipherText;
        }

        public bool send_receive(string command, SshClient ssh_client,out string[] strReturnQue)
        {
            bool isSshCommdsendSuccess = false;
            strReturnQue = new string[3];
            SshCommand result = ssh_client.CreateCommand(command);
            try
            {              
                result.CommandTimeout = TimeSpan.FromSeconds(Properties.Settings.Default.Ssh_Command_Execution_timeout);
                result.Execute();

                //ssh_client.ConnectionInfo.Timeout = TimeSpan.FromSeconds(Properties.Settings.Default.Ssh_Command_Execution_timeout); 
                //var result = ssh_client.RunCommand(command);

                if (result.ExitStatus == 0)
                    isSshCommdsendSuccess = true;

                if (result.Result != null && result.Result != string.Empty)
                    strReturnQue[0]= result.Result.ToString();

                //if (result.Error.Contains("shell-init: error retrieving current directory: getcwd: cannot access parent directories: Inappropriate ioctl for device\n"))
                //{

                //}

                if(!isSshCommdsendSuccess &&(result.Result == null || result.Result == string.Empty))
                {
                    strReturnQue[1] = "Invalid ssh command";
                }
                else if ((!isSshCommdsendSuccess))
                {
                    DeviceDiscovery.WriteToLogFile("SSH Sending command failed: " + result.Result.ToString());
                    strReturnQue[1] = result.Result.ToString();
                }

                ssh_client.Disconnect();
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("timed out"))
                {
                    if (result.ExitStatus == 0)
                        isSshCommdsendSuccess = true;

                    if (result.Result != null && result.Result != string.Empty)
                        strReturnQue[0] = result.Result.ToString();

                    if (!isSshCommdsendSuccess && (result.Result == null || result.Result == string.Empty))
                    {
                        strReturnQue[1] = "Invalid ssh command";
                    }
                    else if ((!isSshCommdsendSuccess))
                    {
                        DeviceDiscovery.WriteToLogFile("SSH Sending command failed: " + result.Result.ToString());
                        strReturnQue[1] = result.Result.ToString();
                    }
                }
                else if (ex.Message.Contains("Message type 80"))
                {
                    DeviceDiscovery.WriteToLogFile("While sending SSh Command ----" + ex.Message);
                    strReturnQue[1] = "Message type 80";
                }
                else
                {
                    DeviceDiscovery.WriteToLogFile(ex.Message);
                    strReturnQue[1] = "Invalid ssh command";
                }
                if (ssh_client != null)
                { ssh_client.Disconnect(); }
            }

            return isSshCommdsendSuccess;
        }

        public bool checkreadaccess(string path_folder)
        {
            DirectoryInfo directory = new DirectoryInfo(path_folder);
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

            DeviceDiscovery.WriteToLogFile("Read access denied for Ssh private key file path");
            return false;
        }
    }
}
