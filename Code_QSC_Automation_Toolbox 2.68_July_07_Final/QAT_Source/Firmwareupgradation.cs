using Microsoft.Win32;
using Microsoft.Win32.TaskScheduler;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
//using System.Web.Script.Serialization;
using System.Windows;
using System.Xml;
using Utility;

namespace QSC_Test_Automation
{
    class Firmwareupgradation
    {
        Crc32 crc = new Crc32();
        //string targetpath = @"D:\QSC_Installedpath";
        string m_strUserName = string.Empty;
        string m_strPassword = "devicelock";
        string coredeviceName = string.Empty;
        public string coreLogonToken = string.Empty;

        Test_Execution testExecutionInstance = null;

        public void testExecutionInstanceCreation(Test_Execution test_exeution_instance)
        {
            testExecutionInstance = test_exeution_instance;
        }

        public MessageBoxResult messageBox(Test_Execution test_exeution_instance, string message, string warning, MessageBoxButton msgButton, MessageBoxImage img)
        {
            MessageBoxResult result = MessageBoxResult.None;
            test_exeution_instance.Dispatcher.Invoke(() =>
            {
                QatMessageBox QMessageBox = new QatMessageBox(test_exeution_instance);
                result = QMessageBox.Show(message, warning, msgButton, img);
            });

            return result;
        }

        public Tuple<string, string, string> designersoftwareInstall(string sourcepath, string InstallationType, string coreIP, string _password, List<DUT_DeviceItem> selectedDUTItem,int dupexid)
        {
            try
            {
                    //m_strUserName = _username;
                    m_strPassword = _password;
                    coredeviceName = DeviceDiscovery.GetAlldeviceNameForSelectedIP(coreIP, selectedDUTItem);

                Uri outUri;
                if (Uri.TryCreate(sourcepath, UriKind.Absolute, out outUri) && (outUri.Scheme == Uri.UriSchemeHttp || outUri.Scheme == Uri.UriSchemeHttps))
                {
                    var urlReturn = designersoftwareInstallFromURL(sourcepath, InstallationType, dupexid);
                    return urlReturn;
                }
                else
                {
                    try
                    {
                        DirectoryInfo dir = new DirectoryInfo(sourcepath);
                    }
                    catch(Exception ex)
                    {
                        DeviceDiscovery.WriteToLogFile("Invalid Path");
                        return new Tuple<string, string, string>(string.Empty, string.Empty, "Invalid Path");
                    }

                    List<string> designerproductname = new List<string>();
                    string Latest_exe_name = string.Empty;
                    string Latest_exe_version = string.Empty;
                    string applicationInstalledpath = string.Empty;
                    string installedPath = string.Empty;
                    string productname = string.Empty;
                    string destinationpath = string.Empty;
                    List<string> installStatus = new List<string>();
                    var directory = new DirectoryInfo(@"C:\Users\" + Environment.UserName + @"\AppData\Local\Temp\QSys Temp Files\Temp_" + dupexid + "\\tempswfile");
                    destinationpath = directory.ToString();


                    //GetLatestWritenFileFileInDirectory(sourcepath);
                    var Latest_exe = Getlatestexe(sourcepath, InstallationType);
                    if (Latest_exe != null)
                    {
                        if (Latest_exe.Item6 == null || Latest_exe.Item6 == string.Empty)
                        {
                            //designerproductname = string.Join(",", Latest_exe.Item5).Trim();
                            designerproductname = Latest_exe.Item5.ToList();
                            if (Latest_exe.Item1.Length.Equals(Latest_exe.Item4.Length))
                            {
                                for (int i = 0; i < Latest_exe.Item1.Length; i++)
                                {
                                    Latest_exe_name = Latest_exe.Item1[i].Trim();
                                    productname = Latest_exe.Item4[i].Trim();
                                    Latest_exe_version = Latest_exe.Item2.Trim();

                                    if (Latest_exe_name.ToUpper().Contains("Q-SYS ADMINISTRATOR") || Latest_exe_name.ToUpper().Contains("Q-SYS DESIGNER INSTALLER") || Latest_exe_name.ToUpper().Contains("Q-SYS UCI VIEWER"))
                                    {
                                        //applicationInstalledpath = GetApplictionInstallPath1(Latest_exe_version, productname);
                                        if (applicationInstalledpath == string.Empty)
                                        {
                                            sourcepath = System.IO.Path.Combine(Latest_exe.Item3.Trim(), Latest_exe_name);

                                            bool dir_copied = DirectoryCopy(sourcepath, Latest_exe_name, destinationpath);

                                            if (dir_copied)
                                            {
                                                destinationpath = System.IO.Path.Combine(destinationpath, Latest_exe_name);

                                                bool install = Installation_exe(destinationpath);
                                                //bool dir_deleted = deletelocalcopy(directory.ToString());
                                                bool dir_deleted = deletelocalcopy(destinationpath.ToString());
                                                if (install && dir_deleted)
                                                {
                                                    //DeviceDiscovery.WriteToLogFile("Installation of " + Latest_exe_name + "(version:" + Latest_exe_version + ") and Deletion of Designer setup in local directory completed successfully ");
                                                    DeviceDiscovery.WriteToLogFile("Installation of " + Latest_exe_name + " and Deletion of Designer setup in local directory completed successfully ");
                                                    installStatus.Add("true");
                                                    //return true;
                                                }
                                                else if (!dir_deleted)
                                                {
                                                    DeviceDiscovery.WriteToLogFile("Deleting Designer setup in local directory failed ");
                                                    //installStatus.Add("false");
                                                    return null;
                                                    //return false;
                                                }
                                                else
                                                {
                                                    //DeviceDiscovery.WriteToLogFile("Installation of " + Latest_exe_name + "(version:" + Latest_exe_version + ") failed ");
                                                    DeviceDiscovery.WriteToLogFile("Installation of " + Latest_exe_name + " failed ");
													installStatus.Add("false");

                                                    //return false;
                                                }
                                                destinationpath = directory.ToString();
                                            }
                                            else
                                            {
                                                DeviceDiscovery.WriteToLogFile("Copying Designer setup from server directory to local directory failed ");
                                                //installStatus.Add("false");
                                                return null;
                                                //return false;
                                            }
                                        }
                                        else
                                        {
                                            //DeviceDiscovery.WriteToLogFile("" + Latest_exe_name + "(version:" + Latest_exe_version + ") already installed in the directory:" + applicationInstalledpath + "");
                                            DeviceDiscovery.WriteToLogFile("" + Latest_exe_name + " already installed in the directory:" + applicationInstalledpath + "");
                                            //return true;
                                            installStatus.Add("true");

                                        }
                                    }
                                }
                            }

                            if ((installStatus.Count > 0) && (!installStatus.Contains("false")))
                            {
                                foreach (string item in designerproductname)
                                {
                                    if (item.Contains("Q-SYS Administrator") || item.Contains("Q-SYS UCI Viewer"))
                                    {
                                        installedPath = GetApplictionInstallPath1(Latest_exe_version, item.Trim());
                                        if(installedPath!=string.Empty)
                                        {
                                            DeviceDiscovery.WriteToLogFile("Setup installed successfully.The installation path is " + installedPath + "");
                                        }
                                        else
                                        {
                                            DeviceDiscovery.WriteToLogFile("Setup installtion failed for " + item);
                                            if (item.Contains("Q-SYS Administrator"))
                                            {
                                                return new Tuple<string, string, string>(string.Empty, string.Empty, "Error occured during installation of Q-Sys administrator software");
                                            }
                                            else
                                            {
                                                return new Tuple<string, string, string>(string.Empty, string.Empty, "Error occured during installation of UCI viewer software");
                                            }
                                        }
                                    }
                                    if (item.Contains("Q-SYS Designer"))
                                    {
                                        applicationInstalledpath = GetApplictionInstallPath1(Latest_exe_version, item.Trim());
                                    }
                                    DeviceDiscovery.WriteToLogFile("Setup Installation completed successfully");
                                }
                                   
                                    if (applicationInstalledpath != string.Empty)
                                    {
                                        DeviceDiscovery.WriteToLogFile("Firmware Upgradation started for installed setup.The installation path is " + applicationInstalledpath + "");
                                        return new Tuple<string, string, string>(applicationInstalledpath, Latest_exe_version, string.Empty);
                                        //bool coreFirmwareStatus=  upgradationIntialization(applicationInstalledpath, coreIP, , Latest_exe_version);
                                        //if(coreFirmwareStatus)
                                        //{
                                        //    return true;
                                        //}
                                        //else
                                        //{
                                        //    return false;
                                        //}
                                    }
                                    else
                                    {
                                        DeviceDiscovery.WriteToLogFile("Firmware Upgradation failed for installed setup.The installation path is not found");
                                        return null;
                                        //return false;
                                    }                                

                            }
                            else if ((installStatus.Count > 0) && (installStatus.Contains("false")))
                            {
                                return null; //return false;
                            }
                            else
                            {
                                return null;
                                //return false;
                            }
                        }
                        else
                        {
                            DeviceDiscovery.WriteToLogFile(Latest_exe.Item6);
                            return new Tuple<string, string, string>(string.Empty, string.Empty, Latest_exe.Item6);
                        }
                    }
                    else
                    {
                        DeviceDiscovery.WriteToLogFile("latest Setup not found in Server directory");
                        return null;
                        //return false;
                    }
                }
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                //MessageBox.Show("Exception\n " + ex.Message, "Error Code - ECxxxx", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
                //return false;
            }
        }

        public Tuple<string, string, string> designersoftwareInstallFromURL(string sourcepath, string InstallationType, int dupexid)
        {
            try
            {
                string designerproductname = string.Empty;
                string Latest_exe_version = string.Empty;
                List<string> installStatus = new List<string>();
                var directory = new DirectoryInfo(@"C:\Users\" + Environment.UserName + @"\AppData\Local\Temp\QSys Temp Files\Temp_" + dupexid + "\\tempswfile");
                string destinationpath = directory.ToString();

                string urlAddress = sourcepath;
                var urlDetails = FirmWareDownLoadFromURL(urlAddress, destinationpath, InstallationType);

                if (urlDetails.Item5 == string.Empty || urlDetails.Item5 == null)
                {
                    if (urlDetails.Item1 == true)
                    {
                        designerproductname = string.Join(",", urlDetails.Item4).Trim();

                        for (int i = 0; i < urlDetails.Item2.Count; i++)
                        {
                            string Latest_exe_name = System.IO.Path.GetFileName(urlDetails.Item2[i]);
                            Latest_exe_version = urlDetails.Item3[i];

                            string destinationpaths = Path.Combine(destinationpath, Latest_exe_name);
                            bool install = Installation_exe(destinationpaths);

                            bool dir_deleted = deletelocalcopy(destinationpaths.ToString());
                            if (install && dir_deleted)
                            {
                                DeviceDiscovery.WriteToLogFile("Installation of " + Latest_exe_name + "(version:" + Latest_exe_version + ") and Deletion of Designer setup in local directory completed successfully ");
                                installStatus.Add("true");
                            }
                            else if (!dir_deleted)
                            {
                                DeviceDiscovery.WriteToLogFile("Deleting Designer setup in local directory failed ");
                            }
                            else
                            {
                                DeviceDiscovery.WriteToLogFile("Installation of " + Latest_exe_name + "(version:" + Latest_exe_version + ") failed ");
                                installStatus.Add("false");
                            }
                        }
                    }

                    if ((installStatus.Count > 0) && (!installStatus.Contains("false")))
                    {
                        DeviceDiscovery.WriteToLogFile("Setup Installation completed successfully");
                        string applicationInstalledpath = GetApplictionInstallPath1(Latest_exe_version.Trim(), designerproductname);
                        if (applicationInstalledpath != string.Empty)
                        {
                            DeviceDiscovery.WriteToLogFile("Firmware Upgradation started for installed setup.The installation path is " + applicationInstalledpath + "");
                            return new Tuple<string, string, string>(applicationInstalledpath, Latest_exe_version, string.Empty);
                        }
                        else
                        {
                            DeviceDiscovery.WriteToLogFile("Firmware Upgradation failed for installed setup.The installation path is not found");
                            return new Tuple<string, string, string>(string.Empty, string.Empty, "Firmware Upgradation unsuccessful as software installation failed");
                        }

                    }
                    else if ((installStatus.Count > 0) && (installStatus.Contains("false")))
                    {
                        return null;
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    string downloadFail = urlDetails.Item5;
                    return new Tuple<string, string, string>(string.Empty, string.Empty, downloadFail);
                }
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                //MessageBox.Show("Exception\n " + ex.Message, "Error Code - ECxxxx", MessageBoxButton.OK, MessageBoxImage.Error);

                return null;
            }
        }

        private Tuple<bool, List<string>, List<string>, List<string>, string> FirmWareDownLoadFromURL(string urlAddress, string directoryName, string installationType)
        {
            List<string> filepath = new List<string>();
            List<string> versionCollection = new List<string>();
            List<string> designerproductname = new List<string>();

            try
            {
                Dictionary<string, string> exeWithSize = new Dictionary<string, string>();

                var val = GetHrefFromURI(urlAddress);

                if (val.Item1 == null || val.Item1 == string.Empty)
                {
                    DeviceDiscovery.WriteToLogFile(val.Item2);
                    return new Tuple<bool, List<string>, List<string>, List<string>, string>(true, filepath, versionCollection, designerproductname, val.Item2);
                }

                MatchCollection m1 = Regex.Matches(val.Item1, "(<td><a href=\"([^\\\"]*\\.exe)\">.*?</a></td><td.*?>.*?</td>)", RegexOptions.IgnoreCase);
                List<string> desAdminUCI = new List<string> { "Designer", "Administrator", "UCI" };

                if (m1.Count > 0)
                {
                    foreach (Match m2 in m1)
                    {
                        string filenames = m2.Groups[1].Value;

                        string txtExp = "href=\"([^\\\"]*\\.exe)\"";
                        var txtMatches = Regex.Matches(filenames, txtExp, RegexOptions.IgnoreCase);

                        string exeName = string.Empty;
                        string fileSize = string.Empty;

                        foreach (Match m in txtMatches)
                        {
                            var filename = m.Groups[1];
                            exeName = filename.Value;
                        }

                        string txtExp1 = "<td class=\"fileSize\">(.+?)</td>";
                        var txtMatches1 = Regex.Matches(filenames, txtExp1, RegexOptions.IgnoreCase);


                        foreach (Match m in txtMatches1)
                        {
                            var filename = m.Groups[1];
                            fileSize = filename.Value;
                        }

                        string ExactexeName = HttpUtility.UrlDecode(exeName);

                        if ((installationType == "Q-Sys Designer" || installationType == string.Empty) && ExactexeName.Contains("Designer Installer"))
                        {
                            if (!exeWithSize.ContainsKey(exeName))
                            {
                                exeWithSize.Add(exeName, fileSize);
                            }
                        }
                        else if (installationType == "Designer,Administrator,UCI Viewer" && (ExactexeName.Contains("Designer Installer") || exeName.Contains("Administrator") || exeName.Contains("UCI")))
                        {
                            if (!exeWithSize.ContainsKey(exeName))
                            {
                                exeWithSize.Add(exeName, fileSize);

                                if (ExactexeName.Contains("Designer Installer") && desAdminUCI.Contains("Designer"))
                                    desAdminUCI.Remove("Designer");

                                if (exeName.Contains("Administrator") && desAdminUCI.Contains("Administrator"))
                                    desAdminUCI.Remove("Administrator");

                                if (exeName.Contains("UCI") && desAdminUCI.Contains("UCI"))
                                    desAdminUCI.Remove("UCI");
                            }
                        }
                    }
                }
                else
                {
                    string txtExp = "href=\"([^\\\"]*\\.exe)\"";
                    var txtMatches = Regex.Matches(val.Item2, txtExp, RegexOptions.IgnoreCase);

                    foreach (Match m in txtMatches)
                    {
                        var filename = m.Groups[1];
                        string exeName = filename.Value;

                        string ExactexeName = HttpUtility.UrlDecode(exeName);
                        if ((installationType == "Q-Sys Designer" || installationType == string.Empty) && ExactexeName.Contains("Designer Installer"))
                        {
                            if (!exeWithSize.ContainsKey(exeName))
                                exeWithSize.Add(exeName, string.Empty);
                        }
                        else if(installationType == "Designer,Administrator,UCI Viewer" && (ExactexeName.Contains("Designer Installer") || exeName.Contains("Administrator") || exeName.Contains("UCI")))
                        {
                            if (!exeWithSize.ContainsKey(exeName))
                            {
                                exeWithSize.Add(exeName, string.Empty);

                                if (ExactexeName.Contains("Designer Installer") && desAdminUCI.Contains("Designer"))
                                    desAdminUCI.Remove("Designer");

                                if (exeName.Contains("Administrator") && desAdminUCI.Contains("Administrator"))
                                    desAdminUCI.Remove("Administrator");

                                if (exeName.Contains("UCI") && desAdminUCI.Contains("UCI"))
                                    desAdminUCI.Remove("UCI");
                            }
                        }
                    }
                }
                

                if (exeWithSize.Count > 0)
                {
                    if (installationType == "Designer,Administrator,UCI Viewer" && desAdminUCI.Count > 0)
                    {
                        return new Tuple<bool, List<string>, List<string>, List<string>, string>(true, filepath, versionCollection, designerproductname, string.Join(",", desAdminUCI) + " exe is not available");
                    }
                }
                else
                {
                    if (installationType != null && installationType != string.Empty)
                    {
                        return new Tuple<bool, List<string>, List<string>, List<string>, string>(true, filepath, versionCollection, designerproductname, installationType + " exe is not available");
                    }
                    else
                    {
                        return new Tuple<bool, List<string>, List<string>, List<string>, string>(true, filepath, versionCollection, designerproductname, "Designer exe is not available");
                    }
                }


                foreach (var softwareWithSize in exeWithSize)
                {
                    string urlAddresss = HttpUtility.UrlDecode(urlAddress + softwareWithSize.Key);
                    string fileName = System.IO.Path.GetFileName(urlAddresss);

                    if (!System.IO.Directory.Exists(directoryName))
                    {
                        System.IO.Directory.CreateDirectory(directoryName);
                    }

                    string path = System.IO.Path.Combine(directoryName, fileName);


                    //var isSuccess = FileSizeChkLoop(fileName, path, urlAddress + softwareWithSize.Key, softwareWithSize);
                    var isSuccess = FileSizeChkLoop(path, urlAddresss, softwareWithSize.Key);

                    if (!isSuccess.Item1)
                    {
                        if (isSuccess.Item2 != null && isSuccess.Item2 != string.Empty)
                            return new Tuple<bool, List<string>, List<string>, List<string>, string>(false, filepath, versionCollection, designerproductname, isSuccess.Item2);
                        else
                            return new Tuple<bool, List<string>, List<string>, List<string>, string>(false, filepath, versionCollection, designerproductname, "Error Occured while downloading the exe : " + urlAddress + softwareWithSize.Key);
                    }

                    filepath.Add(path);

                    var versInfo = FileVersionInfo.GetVersionInfo(path);
                    string versioncollection = versInfo.FileVersion;
                    versionCollection.Add(versioncollection);

                        if (versInfo.ProductName.StartsWith("Q-SYS Designer"))
                        {
                          if(!designerproductname.Contains(versInfo.ProductName))
                             designerproductname.Add(versInfo.ProductName);
                        }
                    //}
                }

                return new Tuple<bool, List<string>, List<string>, List<string>, string>(true, filepath, versionCollection, designerproductname, string.Empty);
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                string downloadStatus = "Error occured while downloading the exe";
                return new Tuple<bool, List<string>, List<string>, List<string>, string>(false, filepath, versionCollection, designerproductname, downloadStatus);
            }
        }

        private Tuple<string,string> GetHrefFromURI(string urlAddress)
        {
            string val = string.Empty;
        Loop:

            try
            {
                using (WebClient client = new WebClient())
                {
                    val = client.DownloadString(urlAddress);
                }
            }
            catch(Exception ex)
            {
                if (ex.Message.Contains("Invalid URI"))
                {
                    return new Tuple<string, string>(string.Empty, "Invalid URL");
                }
                else
                {
                    MessageBoxResult res = MessageBoxResult.None;
                    if (testExecutionInstance != null)
                    {
                        res = messageBox(testExecutionInstance, "Possible reason: Check VPN connection.\n" + urlAddress + "\nDo you want to retry?", "URL can't be reached", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                    }
                    else
                    {
                        res = MessageBox.Show("Possible reason: Check VPN connection.\n" + urlAddress + "\nDo you want to retry?", "URL can't be reached", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                    }

                    if (res == MessageBoxResult.Yes)
                    {
                        goto Loop;
                        //GetHrefFromURI(urlAddress);
                    }
                    else
                    {
                        return new Tuple<string, string>(string.Empty, "URL can't be reached : " + urlAddress);
                    }
                }
            }

            return new Tuple<string, string>(val, string.Empty);
        }

        private Tuple<bool, string> FileSizeChkLoop(string path, string urlAddress, string hrefFileName)
        {
            try
            {

            Loop1:

                int fileSize = 0;
                ///////Get File Size
                var fileDetails = GetFileSizeUsingRequest(path, urlAddress, hrefFileName);
                if (fileDetails != null && fileDetails.Item2 != null && fileDetails.Item2 != string.Empty && fileDetails.Item1 == 0)
                {
                    string downloadStatus = fileDetails.Item2;
                    MessageBoxResult res = MessageBoxResult.None;

                    if (testExecutionInstance != null)
                        res = messageBox(testExecutionInstance, downloadStatus + "\nDo you want retry?", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                    else
                        res = MessageBox.Show(downloadStatus + "\nDo you want retry?", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                    if (res == MessageBoxResult.Yes)
                    {
                        goto Loop1;
                    }
                    else
                    {
                        return new Tuple<bool, string>(false, downloadStatus);
                    }
                }
                else
                {
                    fileSize = fileDetails.Item1;
                }

            Loop2:
                var status = DownloadLoop(path, urlAddress, new KeyValuePair<string, int>(hrefFileName, fileSize));

                if (status.Item1)
                {
                    if (!FileSizeCheck(path, fileSize))
                    {
                        string downloadStatus = "Downloaded File size mismatched with server file : " + urlAddress + hrefFileName;
                        DeviceDiscovery.WriteToLogFile(downloadStatus);

                        MessageBoxResult res = MessageBoxResult.None;

                        if (testExecutionInstance != null)
                            res = messageBox(testExecutionInstance, downloadStatus + "\nDo you want retry?", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                        else
                            res = MessageBox.Show(downloadStatus + "\nDo you want retry?", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                        if (res == MessageBoxResult.Yes)
                        {
                            goto Loop2;
                        }
                        else
                        {
                            return new Tuple<bool, string>(false, downloadStatus);
                        }
                    }
                }
                else
                {
                    return new Tuple<bool, string>(false, status.Item2);
                }

                return new Tuple<bool, string>(true, string.Empty);
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif

                return new Tuple<bool, string>(false, "Error Occured while downloading the exe : " + urlAddress + hrefFileName);
            }
        }

        private Tuple<bool, string> DownloadLoop(string path, string urlAddress,KeyValuePair<string, int> softwareWithSize)
        {
            bool isVPNDisconnect = false;

            try
            {         
                if (isVPNDisconnect == false)
                {
                    deletelocalcopy(path);
                }

                Loop1:
                var exedownloadStatus = ChkDownLoadExeFromURL(path, urlAddress, softwareWithSize);
                isVPNDisconnect = exedownloadStatus.Item2;

                if (exedownloadStatus.Item1.Equals("Zero byte received"))
                {
                    MessageBoxResult res = MessageBoxResult.None;

                    if (testExecutionInstance != null)
                        res = messageBox(testExecutionInstance, "Downloaded File size mismatched with server file : " + urlAddress + softwareWithSize.Key + "\nDo you want retry?", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                    else
                        res = MessageBox.Show("Downloaded File size mismatched with server file : " + urlAddress + softwareWithSize.Key + "\nDo you want retry?", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                    if (res == MessageBoxResult.Yes)
                    {
                        goto Loop1;
                    }
                    else
                    {
                        return new Tuple<bool, string>(false, "Error Occured while downloading the exe : " + urlAddress);
                    }
                }

                if (exedownloadStatus.Item1 != null && exedownloadStatus.Item1 != string.Empty)
                {
                    if (exedownloadStatus.Item1.Contains("Invalid URL"))
                    {
                        return new Tuple<bool, string>(false, exedownloadStatus.Item1);
                    }

                    MessageBoxResult res = MessageBoxResult.None;

                    if (testExecutionInstance != null)
                        res = messageBox(testExecutionInstance, exedownloadStatus.Item1 + "\nDo you want retry?", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                    else
                        res = MessageBox.Show(exedownloadStatus.Item1 + "\nDo you want retry?", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                    if (res == MessageBoxResult.Yes)
                    {
                        goto Loop1;
                    }
                    else
                    {
                        return new Tuple<bool, string>(false, exedownloadStatus.Item1);
                    }
                }

                return new Tuple<bool, string>(true, string.Empty);
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                return new Tuple<bool, string>(false, "Error occured while downloading the exe : " + urlAddress + softwareWithSize.Key);
            }
        }

        private Tuple<string, bool> ChkDownLoadExeFromURL(string path, string urlAddress, KeyValuePair<string, int> softwareWithSize)
        {
            System.IO.FileStream saveFileStream = null;

            try
            {
            //Loop:
                long iFileSize = 0;
                int iBufferSize = 1024;
                iBufferSize *= 1000;
                long iExistLen = 0;
				  bool isskipped = false;
                if (System.IO.File.Exists(path))
                {
                    System.IO.FileInfo fINfo =
                       new System.IO.FileInfo(path);
                    iExistLen = fINfo.Length;
                }
                if (iExistLen > 0)
                    saveFileStream = new System.IO.FileStream(path,
                      System.IO.FileMode.Append, System.IO.FileAccess.Write,
                      System.IO.FileShare.ReadWrite) ;
                else
                    saveFileStream = new System.IO.FileStream(path,
                       System.IO.FileMode.Create, System.IO.FileAccess.Write,
                       System.IO.FileShare.ReadWrite) ;

                        HttpWebRequest hwRq = (HttpWebRequest)System.Net.HttpWebRequest.Create(urlAddress);
                hwRq.AddRange((int)iExistLen);

                using (HttpWebResponse hwRes = (System.Net.HttpWebResponse)hwRq.GetResponse())
                {
                    using (System.IO.Stream smRespStream = hwRes.GetResponseStream())
                    {
                        smRespStream.ReadTimeout = 60000;

                        iFileSize = hwRes.ContentLength;

                        int iByteSize;
                        byte[] downBuffer = new byte[iBufferSize];
                        string two_Read_Join_Buffer = string.Empty;

                        string[] temp_Storage_Buffer = new string[2];

                        while ((iByteSize = smRespStream.Read(downBuffer, 0, downBuffer.Length)) > 0)
                        {
                            string temp_Error_Read_Buffer = System.Text.Encoding.UTF8.GetString(downBuffer);

                            if ((temp_Error_Read_Buffer.Contains("window.Prototype && JSON") || temp_Error_Read_Buffer.Contains("java.io.IOException") || (temp_Error_Read_Buffer.Contains("<!DOCTYPE html>"))))
                            {
                                isskipped = true;
                                break;
                            }

                                if (temp_Storage_Buffer[0] != null)
                            {
                                temp_Storage_Buffer[1] = temp_Storage_Buffer[0];
                            }

                            temp_Storage_Buffer[0] = temp_Error_Read_Buffer;

                            if (temp_Storage_Buffer[1] == null)
                                two_Read_Join_Buffer = temp_Storage_Buffer[0];
                            else if (temp_Storage_Buffer[0] != null)
                                two_Read_Join_Buffer = temp_Storage_Buffer[1] + temp_Storage_Buffer[0];

                            if (isskipped == false && two_Read_Join_Buffer != string.Empty)
                            {
                                //string result = System.Text.Encoding.UTF8.GetString(downBuffer);

                                //var cc = Regex.Replace(result, @"<.*>", string.Empty);

                                if (!(two_Read_Join_Buffer.Contains("window.Prototype && JSON") || two_Read_Join_Buffer.Contains("java.io.IOException") || (two_Read_Join_Buffer.Contains("<!DOCTYPE html>"))))
                                {
                                    saveFileStream.Write(downBuffer, 0, iByteSize);
                                }
                                else
                                {
                                    isskipped = true;
                                    break;                               
                                }
                            }
                        }

                        saveFileStream.Close();

                        if (iByteSize == 0 && !FileSizeCheck(path, softwareWithSize.Value))
                            return new Tuple<string, bool>("Zero byte received", false);
                        else if (isskipped)
                            return new Tuple<string, bool>("Zero byte received", false);
                       else if (iByteSize == 0 && FileSizeCheck(path, softwareWithSize.Value))
                            return new Tuple<string, bool>(string.Empty, false);
                    }
                }

                //if (isskipped)
                //    goto Loop;

                string downloadStatus = "Error occured while downloading the exe.Check connection : " + urlAddress + softwareWithSize.Key;
                return new Tuple<string, bool>(downloadStatus, false);
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif

                if (saveFileStream != null)
                    saveFileStream.Close();

                if(ex.Message.Contains("Unable to read data from the transport connection: An established connection was aborted by the software in your host machine."))
                {
                    DeviceDiscovery.WriteToLogFile("Unable to read data from the transport connection: An established connection was aborted by the software in your host machine");
                    string downloadStatus = "Unable to read data from the transport connection: An established connection was aborted by the software in your host machine";
                    return new Tuple<string, bool>(downloadStatus, true);
                }
                else if (ex.Message.Contains("Unable to connect to the remote server"))
                {
                    DeviceDiscovery.WriteToLogFile("Unable to connect to the remote server : " + urlAddress + softwareWithSize.Key);
                    string downloadStatus = "Unable to connect to the remote server : " + urlAddress + softwareWithSize.Key;
                    return new Tuple<string, bool>(downloadStatus, false);
                }
                else if (ex.Message.Contains("Invalid URI"))
                {
                    DeviceDiscovery.WriteToLogFile("Invalid URL : " + urlAddress + softwareWithSize.Key);
                    string downloadStatus = "Invalid URL : " + urlAddress + softwareWithSize.Key;
                    return new Tuple<string, bool>(downloadStatus, false);
                }
                //else if (ex.Message.Contains("Download timed out"))
                //{
                //    DeviceDiscovery.WriteToLogFile("Download timed out : " + urlAddress + softwareWithSize.Key);
                //    downloadStatus = "Download timed out : " + urlAddress + softwareWithSize.Key;

                //    range = 0;
                //}
                else if (ex.Message.Contains("The operation has timed out"))
                {
                    DeviceDiscovery.WriteToLogFile("Download timed out : " + urlAddress + softwareWithSize.Key);
                    string downloadStatus = "Download timed out. Check connection : " + urlAddress + softwareWithSize.Key;
                    return new Tuple<string, bool>(downloadStatus, false);
                }
                else
                {
                    DeviceDiscovery.WriteToLogFile("Error occured while downloading the exe : " + urlAddress + softwareWithSize.Key);
                    string downloadStatus = "Error occured while downloading the exe.Check connection : " + urlAddress + softwareWithSize.Key;
                    return new Tuple<string, bool>(downloadStatus, false);
                }
            }
        }

        private Tuple<int, string> GetFileSizeUsingRequest(string path, string urlAddress, string fileName)
        {
            try
            {
                int iFileSize = 0;

                HttpWebRequest hwRq = (HttpWebRequest)System.Net.HttpWebRequest.Create(urlAddress);
                using (HttpWebResponse hwRes = (System.Net.HttpWebResponse)hwRq.GetResponse())
                {
                    iFileSize = (int)hwRes.ContentLength;
                }

                return new Tuple<int, string>(iFileSize, string.Empty);
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                if (ex.Message.Contains("Unable to read data from the transport connection: An established connection was aborted by the software in your host machine."))
                {
                    DeviceDiscovery.WriteToLogFile("Unable to read data from the transport connection: An established connection was aborted by the software in your host machine");
                    string downloadStatus = "Unable to read data from the transport connection: An established connection was aborted by the software in your host machine";
                    return new Tuple<int, string>(0, downloadStatus);
                }
                else if (ex.Message.Contains("Unable to connect to the remote server"))
                {
                    DeviceDiscovery.WriteToLogFile("Unable to connect to the remote server : " + urlAddress + fileName);
                    string downloadStatus = "Unable to connect to the remote server : " + urlAddress + fileName;
                    return new Tuple<int, string>(0, downloadStatus);
                }
                else if (ex.Message.Contains("Invalid URI"))
                {
                    DeviceDiscovery.WriteToLogFile("Invalid URL : " + urlAddress + fileName);
                    string downloadStatus = "Invalid URL : " + urlAddress + fileName;
                    return new Tuple<int, string>(0, downloadStatus);
                }
                else
                {
                    DeviceDiscovery.WriteToLogFile("Error occured while downloading the exe : " + urlAddress + fileName);
                    string downloadStatus = "Error occured while downloading the exe.Check connection : " + urlAddress + fileName;
                    return new Tuple<int, string>(0, downloadStatus);
                }
            }
        }

        private bool FileSizeCheck(string filePath, Int32 size)
        {
            try
            {
                //if (size != null && size != string.Empty)
                //{
                //    FileInfo info = new FileInfo(filePath);
                //    var dd = Convert.ToDouble(info.Length);

                //    var x = dd / 1024;
                //    x = x / 1024;

                //    var y = Math.Round(x, 1);

                //    string[] ss = size.Split(' ');

                //    double res = 0;

                //    if (ss != null)
                //    {
                //        Double.TryParse(ss[0], out res);
                //    }


                //    var z = Math.Round(res, 1);
                //    var ccc = Math.Round(Math.Abs(y - z), 1);

                //    if (y == z || ccc <= 0.2)
                //        return true;
                //    else
                //        return false;
                //}
                //else
                //{
                //    return true;
                //}

                //var fileSizeInMegaByte = Math.Round(Convert.ToDouble(size) / 1024.0 / 1024.0, 2);

                //FileInfo info1 = new FileInfo(filePath);

                //float lengthInK = info1.Length / 1024f / 1024f;

                //var ccc = Math.Round(Math.Abs(fileSizeInMegaByte - lengthInK), 1);

                //if (fileSizeInMegaByte == lengthInK || ccc <= 0.2)
                //    return true;
                //else
                //    return false;

                if (System.IO.File.Exists(filePath))
                {
                    System.IO.FileInfo fINfo = new System.IO.FileInfo(filePath);
                    long iExistLen = fINfo.Length;
                    if (iExistLen == size)
                        return true;
                    else
                        return false;
                }
                else
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

        public Tuple<string, string> DownLoadExeFromURL(string urlAddress, string directoryName)
        {
            string filepath = string.Empty;

            try
            {
                string fileName = HttpUtility.UrlDecode(System.IO.Path.GetFileName(urlAddress));

                if (fileName != null && fileName != string.Empty)
                {
                    if (!System.IO.Directory.Exists(directoryName))
                    {
                        System.IO.Directory.CreateDirectory(directoryName);
                    }

                    string path = System.IO.Path.Combine(directoryName, fileName);

                    try
                    {
                        var isSuccess = FileSizeChkLoop(path, urlAddress, string.Empty);

                        if (!isSuccess.Item1)
                        {
                            if (isSuccess.Item2 != null && isSuccess.Item2 != string.Empty)
                                return new Tuple<string, string>(filepath, isSuccess.Item2);
                            else
                                return new Tuple<string, string>(filepath, "Error Occured while downloading the exe : " + urlAddress);
                        }

                        var versInfo = FileVersionInfo.GetVersionInfo(path);

                        if (versInfo.ProductName != null && (versInfo.ProductName.StartsWith("Q-SYS Designer") || versInfo.ProductName.StartsWith("Q-SYS Administrator") || versInfo.ProductName.StartsWith("Q-SYS UCI Viewer")))
                        {
                            filepath = path;
                        }

                        return new Tuple<string, string>(filepath, string.Empty);
                    }
                    catch (Exception ex)
                    {
                        DeviceDiscovery.WriteToLogFile("Error ocuured while downloading the exe : " + urlAddress);
                        string downloadStatus = "Error occured while downloading the exe : " + urlAddress;
                        return new Tuple<string, string>(filepath, downloadStatus);
                    }
                }
                else
                {
                    DeviceDiscovery.WriteToLogFile("Invalid URL Address : " + urlAddress);
                    string downloadStatus = "Invalid URL Address : " + urlAddress;
                    return new Tuple<string, string>(filepath, downloadStatus);
                }
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                string downloadStatus = "Error occured while downloading the exe";
                return new Tuple<string, string>(filepath, downloadStatus);
            }
        }

        public Tuple<string[], string, string, string[], string[], string> Getlatestexe(string spath, string InstallationType)
        {
            string isAllFileExist = string.Empty;

            try
            {
                if (InstallationType != string.Empty)
                {
                    string final, Latest_versionname;
                    int indexfind = 0;

                    List<Version> ver = new List<Version>();
                    List<string> unsortedver = new List<string>();
                    List<string> unsortedfilename = new List<string>();
                    List<string> productname = new List<string>();
                    List<string> designerproductname = new List<string>();
                    List<string> designername = new List<string>();
                    string[] fileinfo = new string[] { };
                    string[] productNameArray = new string[] { };

                    DirectoryInfo dir = new DirectoryInfo(spath);
                    DirectoryInfo[] dircollection = dir.GetDirectories();
                    foreach (DirectoryInfo direct in dircollection)
                    {
                        FileInfo[] files = direct.GetFiles();
                        if (files.Length > 0)
                        {
                            foreach (FileInfo file in files)
                            {
                                var versInfo = FileVersionInfo.GetVersionInfo(System.IO.Path.Combine(direct.FullName.ToString(), file.Name));

                                String versioncollection = versInfo.FileVersion;

                                if (versInfo.ProductName != null)
                                {
                                    string setupProductName = versInfo.ProductName.Trim();
                                    if (setupProductName != null && setupProductName.StartsWith("Q-SYS"))
                                    {
                                        if (String.Equals(System.IO.Path.GetExtension(direct.FullName.ToString() + file.Name), ".exe", StringComparison.InvariantCultureIgnoreCase))
                                        {
                                            if (versioncollection != null)
                                            {
                                                ver.Add(new Version(versioncollection));
                                                unsortedver.Add(versioncollection.Trim());
                                                unsortedfilename.Add(file.FullName);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                    ver.Sort();
                    ver.Reverse();
                    final = ver[0].ToString();
                    indexfind = unsortedver.IndexOf(final);
                    Latest_versionname = unsortedfilename.ElementAt(indexfind);
                    FileInfo finalfile = new FileInfo(Latest_versionname);
                    string finaldirectoryname = finalfile.DirectoryName;
                    DirectoryInfo finaldirectory = new DirectoryInfo(finaldirectoryname);
                    FileInfo[] finalfiles = finaldirectory.GetFiles();

                    List<string> lst = new List<string>();
                    List<string> lst2 = new List<string>();
                    foreach (FileInfo file in finalfiles)
                    {
                        var filesInfo = FileVersionInfo.GetVersionInfo(System.IO.Path.Combine(finaldirectory.FullName.ToString(), file.Name));
                      
                                                
                        if (filesInfo.ProductName != null && filesInfo.ProductName != string.Empty && filesInfo.FileVersion!=null && filesInfo.FileVersion!=string.Empty)
                        {
                            if (final == filesInfo.FileVersion.Trim())
                            {
                                productname.Add(filesInfo.ProductName);
                                //if (filesInfo.ProductName.StartsWith("Q-SYS Designer") || filesInfo.ProductName.StartsWith("Q-SYS Administrator") || filesInfo.ProductName.StartsWith("Q-SYS UCI Viewer"))
                                //{
                                //    designerproductname.Add(filesInfo.ProductName);

                                //    if (filesInfo.ProductName.StartsWith("Q-SYS Designer"))
                                //      designername.Add(file.Name);
                                //}

                                if (InstallationType == "Designer,Administrator,UCI Viewer" && (filesInfo.ProductName.StartsWith("Q-SYS Designer") || filesInfo.ProductName.StartsWith("Q-SYS Administrator") || filesInfo.ProductName.StartsWith("Q-SYS UCI Viewer")))
                                {
                                    if (!lst.Contains(filesInfo.ProductName.Trim()))
                                        lst.Add(filesInfo.ProductName.Trim());

                                    designerproductname.Add(filesInfo.ProductName);
                                    designername.Add(file.Name);
                                }
                                else if (InstallationType == "Q-Sys Designer" && filesInfo.ProductName.StartsWith("Q-SYS Designer"))
                                {
                                    if (!lst.Contains(filesInfo.ProductName.Trim()))
                                        lst.Add(filesInfo.ProductName.Trim());

                                    designerproductname.Add(filesInfo.ProductName);
                                    designername.Add(file.Name);
                                }
                            }
                        }
                    }

                    if (InstallationType == "Designer,Administrator,UCI Viewer")
                    {
                        if (lst.Count < 3)
                        {
                            //var cc = lst.Where(x => x.ToUpper().StartsWith("Q-SYS DESIGNER") || x.ToUpper().StartsWith("Q-SYS ADMINISTRATOR") || x.ToUpper().StartsWith("Q-SYS UCI VIEWER")).SelectMany(x=> x).ToList();
                            
                            foreach (string item in lst)
                            {
                                if (item.ToUpper().StartsWith("Q-SYS DESIGNER"))                               
                                    lst2.Add("Designer");
                                
                                else if (item.ToUpper().StartsWith("Q-SYS ADMINISTRATOR"))                                
                                    lst2.Add("Administrator");
                                
                                else if (item.ToUpper().StartsWith("Q-SYS UCI VIEWER"))                                
                                    lst2.Add("UCI Viewer");                                
                            }

                            if (lst2.Count==2)
                            {
                                if(!lst2.Contains("Designer"))
                                    isAllFileExist = "Qsys designer version "+ final +" file is missing in the selected path";
                                else if(!lst2.Contains("Administrator"))
                                    isAllFileExist=  "Qsys Administrator version "+ final +" file is missing in the selected path";
                                else if(!lst2.Contains("UCI Viewer"))
                                    isAllFileExist = "Qsys UCI Viewer version "+ final + " file is missing in the selected path";
                            }

                            if(lst2.Count==1)
                            {
                                if (lst2.Contains("Designer"))
                                    isAllFileExist ="Qsys Administrator and Qsys UCI Viewer version "+ final + " files are missing in the selected path";
                               else if (lst2.Contains("Administrator"))
                                    isAllFileExist = "Qsys Designer and Qsys UCI Viewer version "+ final + " files are missing in the selected path";
                               else if (lst2.Contains("UCI Viewer"))
                                    isAllFileExist = "Qsys Designer and Qsys Administrator version " + final + " files are missing in the selected path";
                            }
                            //isAllFileExist = "Missing one of the exe in the selected path";
                            return new Tuple<string[], string, string, string[], string[], string>(fileinfo, final, finaldirectoryname, productNameArray, designerproductname.ToArray(), isAllFileExist);
                        }

                        fileinfo = designername.ToArray();
                        productNameArray = productname.ToArray();
                    }

                    if (InstallationType == "Q-Sys Designer")
                    {
                        if (lst.Count < 1)
                        {
                            isAllFileExist = "Qsys designer version " + final + " file is missing in the selected path";
                            return new Tuple<string[], string, string, string[], string[], string>(fileinfo, final, finaldirectoryname, productNameArray, designerproductname.ToArray(), isAllFileExist);
                        }

                        fileinfo = designername.ToArray();
                        productNameArray = designerproductname.ToArray();
                    }

                    //string[] fileinfo= finalfiles.Select(f1 => f1.Name).ToArray();
                    //string[] productNameArray = productname.ToArray();
                    DeviceDiscovery.WriteToLogFile("" + Latest_versionname + "(version:" + final + ") found in the directory:" + finaldirectoryname + " and it contains following files:" + string.Join(",", fileinfo) + "");
                    return new Tuple<string[], string, string, string[], string[], string>(fileinfo, final, finaldirectoryname, productNameArray, designerproductname.ToArray(), string.Empty);
                }
                else
                {
                    DeviceDiscovery.WriteToLogFile("Testcase doesn't contain Installation Type");
                    return null;
                }
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

        public static string GetApplictionInstallPath(string versionOfAppToFind)
        {
            string installedPath;
            string keyName;
            try
            {
                // search in: CurrentUser
                keyName = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall";
                installedPath = ExistsInSubKey(Registry.CurrentUser, keyName, "DisplayVersion", versionOfAppToFind);
                if (!string.IsNullOrEmpty(installedPath))
                {
                    DeviceDiscovery.WriteToLogFile("Designer exe installed path is:" + installedPath + "");
                    return installedPath;
                }

                // search in: LocalMachine_32
                keyName = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall";
                installedPath = ExistsInSubKey(Registry.LocalMachine, keyName, "DisplayVersion", versionOfAppToFind);
                if (!string.IsNullOrEmpty(installedPath))
                {
                    DeviceDiscovery.WriteToLogFile("Designer exe installed path is:" + installedPath + "");
                    return installedPath;
                }

                // search in: LocalMachine_64
                keyName = @"SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall";
                installedPath = ExistsInSubKey(Registry.LocalMachine, keyName, "DisplayVersion", versionOfAppToFind);
                if (!string.IsNullOrEmpty(installedPath))
                {
                    DeviceDiscovery.WriteToLogFile("Designer exe installed path is:" + installedPath + "");
                    return installedPath;
                }
                DeviceDiscovery.WriteToLogFile("Designer exe installed path not found ");
                return string.Empty;
            }
            catch(Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Designer exe installed path not found ");
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif

                return string.Empty;
            }
        }

        public static string ExistsInSubKey(RegistryKey root, string subKeyName, string attributeName, string versionOfAppToFind)
        {
            try
            {
                RegistryKey subkey;
                string displayName;
                

                using (RegistryKey key = root.OpenSubKey(subKeyName))
                {
                    if (key != null)
                    {
                        foreach (string kn in key.GetSubKeyNames())
                        {
                            using (subkey = key.OpenSubKey(kn))
                            {
                                displayName = subkey.GetValue(attributeName) as string;
                                
                                if ((versionOfAppToFind.Equals(displayName, StringComparison.OrdinalIgnoreCase) == true))
                                {
                                    DeviceDiscovery.WriteToLogFile("Designer exe installed path found ");
                                    return subkey.GetValue("InstallLocation") as string;
                                }
                            }
                        }
                    }
                }
                DeviceDiscovery.WriteToLogFile("Designer exe installed path not found ");
                return string.Empty;
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif

                //MessageBox.Show("Exception\n " + ex.Message, "Error Code - ECxxxxx", MessageBoxButton.OK, MessageBoxImage.Error);
                DeviceDiscovery.WriteToLogFile("Designer exe installed path not found ");
                return string.Empty;
            }
            
        }

        public static string GetApplictionInstallPath1(string versionOfAppToFind, string productNameOfAppToFind)
        {
            string installedPath;
            string keyName;
            try
            {
                // search in: CurrentUser
                keyName = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall";
                installedPath = ExistsInSubKey1(Registry.CurrentUser, keyName, "DisplayVersion", versionOfAppToFind, "DisplayName", productNameOfAppToFind);
                if (!string.IsNullOrEmpty(installedPath))
                {
                    DeviceDiscovery.WriteToLogFile("Designer exe installed path is:" + installedPath + "");
                    return installedPath;
                }

                // search in: LocalMachine_32
                keyName = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall";
                installedPath = ExistsInSubKey1(Registry.LocalMachine, keyName, "DisplayVersion", versionOfAppToFind, "DisplayName", productNameOfAppToFind);
                if (!string.IsNullOrEmpty(installedPath))
                {
                    DeviceDiscovery.WriteToLogFile("Designer exe installed path is:" + installedPath + "");
                    return installedPath;
                }

                // search in: LocalMachine_64
                keyName = @"SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall";
                installedPath = ExistsInSubKey1(Registry.LocalMachine, keyName, "DisplayVersion", versionOfAppToFind, "DisplayName", productNameOfAppToFind);
                if (!string.IsNullOrEmpty(installedPath))
                {
                    DeviceDiscovery.WriteToLogFile("Designer exe installed path is:" + installedPath + "");
                    return installedPath;
                }
                DeviceDiscovery.WriteToLogFile("Designer exe installed path not found ");
                return string.Empty;
            }
            catch(Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Designer exe installed path not found ");
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif

                return string.Empty;
            }
        }

        public static string ExistsInSubKey1(RegistryKey root, string subKeyName, string attributeName, string versionOfAppToFind, string attributeName1, string productNameOfAppToFind)
        {
            try
            {
                RegistryKey subkey;
                string displayName;
                string displayVersion;

                using (RegistryKey key = root.OpenSubKey(subKeyName))
                {
                    if (key != null)
                    {
                        foreach (string kn in key.GetSubKeyNames())
                        {
                            using (subkey = key.OpenSubKey(kn))
                            {
                                displayName = subkey.GetValue(attributeName1) as string;
                                displayVersion = subkey.GetValue(attributeName) as string;
                                if ((productNameOfAppToFind.Equals(displayName, StringComparison.OrdinalIgnoreCase) == true) && (versionOfAppToFind.Equals(displayVersion, StringComparison.OrdinalIgnoreCase) == true))
                                {
                                    DeviceDiscovery.WriteToLogFile("Designer exe installed path found ");
                                    return subkey.GetValue("InstallLocation") as string;
                                }
                            }
                        }
                    }
                }
                DeviceDiscovery.WriteToLogFile("Designer exe installed path not found ");
                return string.Empty;
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                //MessageBox.Show("Exception\n " + ex.Message, "Error Code - ECxxxxx", MessageBoxButton.OK, MessageBoxImage.Error);
                DeviceDiscovery.WriteToLogFile("Designer exe installed path not found ");
                return string.Empty;
            }

        }

        public static bool DirectoryCopy(string sourceDirName, string Latest_exe_name, string destDirName)
        {
            try
            {
                FileInfo file = new FileInfo(sourceDirName);

                if (!file.Exists)
                {
                    throw new FileNotFoundException(
                        "Source file does not exist or could not be found: "
                        + sourceDirName);

                }
                if (!Directory.Exists(destDirName))
                {
                    Directory.CreateDirectory(destDirName);
                }

                FileInfo temppath = new FileInfo(System.IO.Path.Combine(destDirName, file.Name));
                if (!temppath.Exists)
                {
                    file.CopyTo(temppath.FullName, false);

                }

                DeviceDiscovery.WriteToLogFile("Designer exe successfully copied to local path ");
                return true;
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                //MessageBox.Show("Exception\n " + ex.Message, "Error Code - ECxxxx", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        public bool Installation_exe(string destDirName)
        {
            try
            {
                //To Skip User Account Control setting dialog
                TaskService ts = new TaskService();
                TaskDefinition td = ts.NewTask();
                td.Principal.RunLevel = TaskRunLevel.Highest;
                td.Triggers.AddNew(TaskTriggerType.Logon);
                td.Actions.Add(new ExecAction(destDirName, null));
                ts.RootFolder.RegisterTaskDefinition("SilentInstall", td);

                //To Perform Installation silently
                Process process = new Process();
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.CreateNoWindow = false;
                process.StartInfo.FileName = destDirName;
                process.StartInfo.Arguments = string.Format(" /s /v /qb");
                process.Start();
                DeviceDiscovery.WriteToLogFile("Installation process started... ");
                DeviceDiscovery.WriteToLogFile("Installation process waiting to complete... ");
                process.WaitForExit();
                ts.RootFolder.DeleteTask("SilentInstall");
                DeviceDiscovery.WriteToLogFile("Designer exe installed successfully &  The task scheduled is also killed successfully ");
                return true;
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Installation of Designer setup failed due to Administrator rights");
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif

                return false;
            }
        }

        public bool deletelocalcopy(string destinationpath)
        {
            try
            {
                //if (!Directory.Exists(destinationpath))
                //{
                //    Directory.Delete(destinationpath);
                //}

                if (File.Exists(destinationpath))
                {
                    File.SetAttributes(destinationpath, FileAttributes.Normal);
                    File.Delete(destinationpath);
                }
                return true;
            }
            catch(Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                //MessageBox.Show("Exception\n " + ex.Message, "Error Code - ECxxxx", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        public Tuple<bool, string, string, string,string> firmwareUpgrade(string filepath, string coreIP, string installerpath, string exeversion1, string _username, string _password, List<DUT_DeviceItem> selectedDeviceItem, string fromdesigner, int dupexid, bool isTimeCal, string logpath, string coretoken)
        {
            bool isDelete = false;

            try
            {
                int xmlResponseCount = 0;
                m_strUserName = _username;
                m_strPassword = _password;
                string applicationInstalledpath = string.Empty;
                coredeviceName = DeviceDiscovery.GetAlldeviceNameForSelectedIP(coreIP, selectedDeviceItem);
                coreLogonToken = coretoken;

                if (installerpath == string.Empty && filepath != string.Empty)
                {
                    Uri outUri;
                    if (Uri.TryCreate(filepath, UriKind.Absolute, out outUri) && (outUri.Scheme == Uri.UriSchemeHttp || outUri.Scheme == Uri.UriSchemeHttps))
                    {
                        var directory = new DirectoryInfo(@"C:\Users\" + Environment.UserName + @"\AppData\Local\Temp\QSys Temp Files\Temp_"+ dupexid +"\\tempswfile");

                        var collection = DownLoadExeFromURL(filepath, directory.ToString());

                        if (collection.Item2 == null || collection.Item2 == string.Empty)
                        {
                            if (collection.Item1 != null && collection.Item1 != string.Empty)
                            {
                                filepath = collection.Item1;
                                isDelete = true;
                            }
                            else
                                filepath = string.Empty;
                        }
                        else
                        {
                            DeviceDiscovery.WriteToLogFile(collection.Item2);
                                return new Tuple<bool, string, string, string, string>(false, String.Empty, collection.Item2, string.Empty, string.Empty);
                        }
                    }

                    if (filepath != string.Empty)
                    {
                        FileInfo file = new FileInfo(filepath);
                        if (file.Exists)
                        {
                            string exeversion = string.Empty;
                            //System.IO.FileInfo fileinfo = new FileInfo(filepath);
                            var versInfo = FileVersionInfo.GetVersionInfo(filepath);
                            //String exeversion = versInfo.FileVersion.Trim();

                            string exeproductname = versInfo.ProductName.Trim();

                            if (fromdesigner == "designerinstall")
                            {
                                applicationInstalledpath = GetApplictionInstallPath1(versInfo.FileVersion.Trim(), exeproductname);
                            }
                            if (applicationInstalledpath == string.Empty)
                            {
                                DeviceDiscovery.WriteToLogFile("Designer Installation started.... ");
                                bool installStatus = Installation_exe(filepath);
                                if (installStatus)
                                {
                                    applicationInstalledpath = GetApplictionInstallPath1(versInfo.FileVersion.Trim(), exeproductname);

                                    string buildInfoFilePath = System.IO.Path.Combine(applicationInstalledpath, "build.info");

                                    if (System.IO.File.Exists(buildInfoFilePath))
                                    {
                                        using (StreamReader exeBuildVersions = new StreamReader(buildInfoFilePath))
                                        {
                                            exeversion = exeBuildVersions.ReadToEnd().Replace("\n", string.Empty);
                                        }
                                    }
                                    else
                                    {
                                        exeversion = versInfo.FileVersion.Trim();
                                    }

                                    DeviceDiscovery.WriteToLogFile("Firmware Upgradation/degradation started for installing application... ");
                                    string remarks = string.Empty;
                                    bool status = upgradationIntialization(dupexid, applicationInstalledpath, coreIP, filepath, exeversion, isTimeCal, logpath, out remarks);

                                    //if (isTimeCal && !status)
                                    //    return new Tuple<bool, string, string, string, string>(false, string.Empty, remarks, string.Empty, string.Empty);

                                    if (status)
                                    {
                                        string firmwarestatus = string.Empty;
                                        //if (!isTimeCal)
                                        //{
                                            while (firmwarestatus != "idle")
                                            {
                                                firmwarestatus = get_FirmwareLoadState(coreIP);
                                                if ((firmwarestatus == string.Empty) || (firmwarestatus == "complete"))
                                                {
                                                    xmlResponseCount++;
                                                    if (xmlResponseCount > 25)
                                                    {
                                                        DeviceDiscovery.WriteToLogFile("Firmware Upgradation status not responding");
                                                        xmlResponseCount = 0;
                                                        return new Tuple<bool, string, string, string, string>(false, string.Empty, "Firmware Upgradation status not responding", string.Empty, string.Empty);
                                                    }
                                                    //break;
                                                }
                                            //}
                                        }

                                        //if (firmwarestatus == "idle" || isTimeCal)
										if (firmwarestatus == "idle")
                                        {
                                            string firmwareversion = string.Empty;
                                            firmwareversion = XmlReadToGetDesignversion(coreIP);
                                            if (firmwareversion == exeversion)
                                            {
                                                DeviceDiscovery.WriteToLogFile("Firmware is succesfully Upgradated/downgraded to core through IP:" + coreIP + "");
                                                //return true;
                                                //HttpGet("http://" + coreIP + "/cgi-bin/password_set?password1=" + m_strPassword + "&password2=" + m_strPassword + "", "", "EC15016", coreIP, out strResponse);
                                                string installedEXePath = getInstalledEXePath(applicationInstalledpath);
                                                return new Tuple<bool, string, string, string, string>(true, installedEXePath, remarks, applicationInstalledpath, exeversion);
                                            }
                                            else
                                            {
                                                DeviceDiscovery.WriteToLogFile("Firmware Upgradation failed. Core:" + firmwareversion + " and Q-sys:" + exeversion + " versions are different");
                                                //return false;
                                                return new Tuple<bool, string, string, string, string>(false, string.Empty, "Firmware Upgradation failed. Core:" + firmwareversion + " and Q-sys:" + exeversion + " versions are different", applicationInstalledpath, exeversion);
                                            }
                                        }
                                        else
                                        {
                                            DeviceDiscovery.WriteToLogFile("Firmware Upgrade to core is taking more time. Current response from core:" + firmwarestatus + "");
                                            //return false;
                                            return new Tuple<bool, string, string, string, string>(false, string.Empty, "Firmware Upgrade to core is taking more time. Current response from core:" + firmwarestatus + "", applicationInstalledpath, exeversion);
                                        }
                                    }
                                    else
                                    {
                                        if (isTimeCal && !string.IsNullOrEmpty(remarks))
                                        {
                                            DeviceDiscovery.WriteToLogFile(remarks);
                                            return new Tuple<bool, string, string, string, string>(false, string.Empty, remarks, string.Empty, string.Empty);
                                        }
                                        else
                                        {
                                            DeviceDiscovery.WriteToLogFile("Firmware Upgrade to core is not getting started");
                                            // return false;
                                            return new Tuple<bool, string, string, string, string>(false, string.Empty, "Firmware Upgrade to core is not getting started", string.Empty, string.Empty);
                                        }
                                    }
                                }
                                else
                                {
                                    DeviceDiscovery.WriteToLogFile("Qsys Designer Installation Failed");
                                    //return false;
                                    return new Tuple<bool, string, string, string,string>(false, string.Empty, "Qsys Designer Installation Failed", applicationInstalledpath, exeversion);
                                }
                            }
                            else
                            {
                                DeviceDiscovery.WriteToLogFile("Designer is already installed...");
                                DeviceDiscovery.WriteToLogFile("Firmware Upgradation/degradation started for launching the application... ");
                                //applicationInstalledpath = filepath;
                                string remarks = string.Empty;
                                bool status = upgradationIntialization(dupexid, applicationInstalledpath, coreIP, filepath, exeversion, isTimeCal, logpath, out remarks);

                                //if (isTimeCal && !status)
                                //    return new Tuple<bool, string, string, string, string>(false, string.Empty, remarks, string.Empty, string.Empty);

                                if (status)
                                {
                                    string firmwarestatus = string.Empty;
                                    //if (!isTimeCal)
                                    //{
                                        while (firmwarestatus != "idle")
                                        {
                                            firmwarestatus = get_FirmwareLoadState(coreIP);
                                            if ((firmwarestatus == string.Empty) || (firmwarestatus == "complete"))
                                            {
                                                xmlResponseCount++;
                                                if (xmlResponseCount > 25)
                                                {
                                                    DeviceDiscovery.WriteToLogFile("Firmware Upgradation status not responding");
                                                    xmlResponseCount = 0;
                                                    return new Tuple<bool, string, string, string, string>(false, string.Empty, "Firmware Upgradation status not responding", string.Empty, string.Empty);
                                                }
                                                //break;
                                            }
                                        }
                                    //}

                                    //if (firmwarestatus == "idle" || isTimeCal)
									if (firmwarestatus == "idle")
                                    {
                                        string firmwareversion = string.Empty;
                                        firmwareversion = XmlReadToGetDesignversion(coreIP);
                                        if (firmwareversion == exeversion)
                                        {
                                            DeviceDiscovery.WriteToLogFile("Firmware is succesfully Upgradated/downgraded to core through IP:" + coreIP + "");
                                            //return true;
                                            //HttpGet("http://" + coreIP + "/cgi-bin/password_set?password1=" + m_strPassword + "&password2=" + m_strPassword + "", "", "EC15016", coreIP, out strResponse);
                                            string installedEXePath = getInstalledEXePath(applicationInstalledpath);
                                            return new Tuple<bool, string, string, string, string>(true, installedEXePath, remarks, applicationInstalledpath, exeversion);
                                        }
                                        else
                                        {
                                            DeviceDiscovery.WriteToLogFile("Firmware Upgradation failed. Core:" + firmwareversion + " and Q-sys:" + exeversion + " versions are different");
                                            //return false;
                                            return new Tuple<bool, string, string, string, string>(false, string.Empty, "Firmware Upgradation failed. Core:" + firmwareversion + " and Q-sys:" + exeversion + " versions are different", applicationInstalledpath, exeversion);
                                        }
                                    }
                                    else
                                    {
                                        DeviceDiscovery.WriteToLogFile("Firmware Upgrade to core is taking more time. Current response from core:" + firmwarestatus + "");
                                        //return false;
                                        return new Tuple<bool, string, string, string, string>(false, string.Empty, "Firmware Upgrade to core is taking more time. Current response from core:" + firmwarestatus + "", applicationInstalledpath, exeversion);
                                    }
                                }
                                else
                                {
                                    if (isTimeCal && !string.IsNullOrEmpty(remarks))
                                    {
                                        DeviceDiscovery.WriteToLogFile(remarks);
                                        return new Tuple<bool, string, string, string, string>(false, string.Empty, remarks, string.Empty, string.Empty);
                                    }
                                    else
                                    {
                                        DeviceDiscovery.WriteToLogFile("Firmware Upgrade to core is not getting started");
                                        // return false;
                                        return new Tuple<bool, string, string, string, string>(false, string.Empty, "Firmware Upgrade to core is not getting started", string.Empty, string.Empty);
                                    }
                                }
                            }

                        }
                        else
                        {
                            DeviceDiscovery.WriteToLogFile("File not available in the preffered directory");
                            //return false;
                            return new Tuple<bool, string, string, string,string>(false, string.Empty, "File not available in the preffered directory", applicationInstalledpath, exeversion1);
                        }
                    }
                    else
                    {
                        DeviceDiscovery.WriteToLogFile("File path is empty");
                        //return false;
                        return new Tuple<bool, string, string, string,string>(false, string.Empty, "File path is empty", applicationInstalledpath, exeversion1);
                    }
                }
                else if (filepath == string.Empty && installerpath != string.Empty)      //////for first option in firmware(Auto upgrade firmware by installing designer ) 
                {
                    string remarks = string.Empty;
                    bool status = upgradationIntialization(dupexid, installerpath, coreIP, filepath, exeversion1, isTimeCal, logpath, out remarks);

                    //if (isTimeCal && !status)
                    //    return new Tuple<bool, string, string, string, string>(false, string.Empty, remarks, string.Empty, string.Empty);

                    if (status)
                    {
                        string firmwarestatus = string.Empty;
                        //if (!isTimeCal)
                        //{
                            while (firmwarestatus != "idle")
                            {
                                firmwarestatus = get_FirmwareLoadState(coreIP);
                                if ((firmwarestatus == string.Empty) || (firmwarestatus == "complete"))
                                {
                                    xmlResponseCount++;
                                    if (xmlResponseCount > 25)
                                    {
                                        DeviceDiscovery.WriteToLogFile("Firmware Upgradation status not responding");
                                        xmlResponseCount = 0;
                                        return new Tuple<bool, string, string, string, string>(false, string.Empty, "Firmware Upgradation status not responding", string.Empty, string.Empty);
                                    }
                                    //break;
                                }
                            }
                        //}
						
						//if (firmwarestatus == "idle" || isTimeCal)
                        if (firmwarestatus == "idle")
                        {
                            string firmwareversion = string.Empty;
                            firmwareversion = XmlReadToGetDesignversion(coreIP);

                            string buildInfoFilePath = System.IO.Path.Combine(installerpath, "build.info");
                            string designerversion = string.Empty;

                            if (System.IO.File.Exists(buildInfoFilePath))
                            {
                                using (StreamReader exeBuildVersions = new StreamReader(buildInfoFilePath))
                                {
                                    designerversion = exeBuildVersions.ReadToEnd().Replace("\n", string.Empty);
                                }
                            }
                            else
                            {
                                designerversion = exeversion1;
                            }

                            if (firmwareversion == designerversion)
                            {
                                DeviceDiscovery.WriteToLogFile("Firmware is succesfully Upgradated/downgraded to core through IP:" + coreIP + "");
                                //return true;
                                //HttpGet("http://" + coreIP + "/cgi-bin/password_set?password1=" + m_strPassword + "&password2=" + m_strPassword + "", "", "EC15016", coreIP, out strResponse);
                                string installedEXePath = getInstalledEXePath(installerpath);
                                return new Tuple<bool, string, string, string, string>(true, installedEXePath, remarks, string.Empty, string.Empty);
                            }
                            else
                            {
                                DeviceDiscovery.WriteToLogFile("Firmware Upgradation failed. Core:" + firmwareversion + " and Q-sys:" + designerversion + " versions are different");
                                //return false;
                                return new Tuple<bool, string, string, string, string>(false, String.Empty, "Firmware Upgradation failed. Core:" + firmwareversion + " and Q-sys:" + designerversion + " versions are different", string.Empty, string.Empty);
                            }
                        }
                        else
                        {
                            DeviceDiscovery.WriteToLogFile("Firmware Upgrade to core is taking more time. Current response from core:" + firmwarestatus + "");
                            //return false;
                            return new Tuple<bool, string, string, string, string>(false, string.Empty, "Firmware Upgrade to core is taking more time. Current response from core:" + firmwarestatus + "", string.Empty, string.Empty);
                        }
                    }
                    else
                    {
                        if (isTimeCal && !string.IsNullOrEmpty(remarks))
                        {
                            DeviceDiscovery.WriteToLogFile(remarks);
                            return new Tuple<bool, string, string, string, string>(false, string.Empty, remarks, string.Empty, string.Empty);
                        }
                        else
                        {
                            DeviceDiscovery.WriteToLogFile("Firmware Upgrade to core is not getting started");
                            // return false;
                            return new Tuple<bool, string, string, string, string>(false, string.Empty, "Firmware Upgrade to core is not getting started", string.Empty, string.Empty);
                        }
                    }
                }
                else
                {
                    DeviceDiscovery.WriteToLogFile("File not available in the preffered directory");
                    //return false;
                    return new Tuple<bool, string, string, string,string>(false, string.Empty, "File not available in the preffered directory", string.Empty, string.Empty);
                }
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                //MessageBox.Show("Exception\n " + ex.Message, "Error Code - ECxxxx", MessageBoxButton.OK, MessageBoxImage.Error);
                //return false;
                return new Tuple<bool, string, string, string, string>(false, string.Empty, string.Empty, string.Empty, string.Empty);
            }
            finally
            {
                if(isDelete)
                    deletelocalcopy(filepath);
            }
        }

        public string getInstalledEXePath(string applicationInstalledpath)
        {
            try
            {
                FileInfo filepath = new FileInfo(applicationInstalledpath);
                string fullpath = filepath.FullName;
                string installedExePath = string.Empty;
                string fileName = @"Q-Sys Designer.exe";
                //firmwarePath = System.IO.Path.Combine(applicationInstalledpath, "Firmware\\core\\firmware.bin");
                string[] files = Directory.GetFiles(fullpath, fileName, SearchOption.AllDirectories);
                if (files.Length > 0)
                {
                    foreach (string exepath in files)
                    {
                        installedExePath = exepath;
                    }
                }
                else
                {
                    DeviceDiscovery.WriteToLogFile("Q-Sys Designer.exe not available in the preffered directory");
                    installedExePath = string.Empty;
                }

                return installedExePath;
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                //MessageBox.Show("Exception\n " + ex.Message, "Error Code - ECxxxx", MessageBoxButton.OK, MessageBoxImage.Error);
                return string.Empty;
            }
        }

        public bool upgradationIntialization(int excID, string applicationInstalledpath, string coreIP, string filepath, string exeversion, bool isTimeCal, string logpath, out string remarks)
        {
            remarks = string.Empty;

            try
            {
                string strResponse = string.Empty;
                string firmwarePath = string.Empty;
                string fileName = @"firmware.manifest";

                //firmwarePath = System.IO.Path.Combine(applicationInstalledpath, "Firmware\\core\\firmware.bin");
                string[] files = Directory.GetFiles(applicationInstalledpath, fileName, SearchOption.AllDirectories);

                if(files.Length==0)
                    files = Directory.GetFiles(applicationInstalledpath, @"firmware.bin", SearchOption.AllDirectories);

                if (files.Length > 0)
                {
                    foreach (string corepath in files)
                    {
                        firmwarePath = corepath;
                    }
                    FileInfo coreFimware = new FileInfo(firmwarePath);
                    if (coreFimware.Exists)
                    {
                        bool processStatus = upgradationProcess(excID, firmwarePath, coreIP, exeversion, isTimeCal, logpath, out remarks);
                        if (processStatus)
                        {
                            //if (!isTimeCal)
                            //DeviceDiscovery.WriteToLogFile("Firmware upgradation intialization completed successfully");

                            //HttpGet("http://" + coreIP + "/cgi-bin/password_set?password1=" + m_strPassword + "&password2=" + m_strPassword + "", "", "EC15016", coreIP, out strResponse);
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                    else
                    {
                        DeviceDiscovery.WriteToLogFile("Firmware not available in the preffered directory");
                        return false;
                    }
                }
                else
                {
                    bool installStatus = Installation_exe(filepath);
                    
                    if (installStatus)
                    {
                        var selectedexepath = FileVersionInfo.GetVersionInfo(filepath);
                        string exeproductname = selectedexepath.ProductName;
                        applicationInstalledpath = GetApplictionInstallPath1(exeversion, exeproductname.Trim());
                        string firmwarePath1 = string.Empty;
                        string fileName1 = @"firmware.manifest";
                        //firmwarePath = System.IO.Path.Combine(applicationInstalledpath, "Firmware\\core\\firmware.bin");
                        string[] files1 = Directory.GetFiles(applicationInstalledpath, fileName1, SearchOption.AllDirectories);
                        if(files1.Length==0)
                            files1 = Directory.GetFiles(applicationInstalledpath, @"firmware.bin", SearchOption.AllDirectories);

                        if (files1.Length > 0)
                        {
                            foreach (string s in files1)
                            {
                                FileInfo corepath = new FileInfo(s);
                                if (corepath.Exists)
                                {
                                    firmwarePath1 = s;
                                }
                            }
                            FileInfo coreFimware = new FileInfo(firmwarePath1);
                            if (coreFimware.Exists)
                            {
                                bool processStatus = upgradationProcess(excID, firmwarePath1, coreIP, exeversion, isTimeCal, logpath, out remarks);
                                if (processStatus)
                                {
                                    //if (!isTimeCal)
                                    //    DeviceDiscovery.WriteToLogFile("Firmware upgradation intialization completed successfully");
                                    //HttpGet("http://" + coreIP + "/cgi-bin/password_set?password1=" + m_strPassword + "&password2=" + m_strPassword + "", "", "EC15016", coreIP, out strResponse);
                                    return true;
                                }
                                else
                                {
                                    return false;
                                }
                            }
                            else
                            {
                                DeviceDiscovery.WriteToLogFile("Firmware not available in the preffered directory");
                                return false;
                            }

                        }
                        else
                        {
                            DeviceDiscovery.WriteToLogFile("Firmware not available in the preffered directory");
                            return false;
                        }
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                //MessageBox.Show("Exception\n " + ex.Message, "Error Code - ECxxxx", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        public Tuple<bool, string, string> firmwareUpgradebylaunch(string filepath, string coreIP, string _username, string _password, List<DUT_DeviceItem> selectedDUTItem, int excID, bool isTimeCal, string logPath, string coreToken)
        {
            try
            {
                string strResponse = string.Empty;
                int xmlResponseCount = 0;
                m_strUserName = _username;
                m_strPassword = _password;
                coreLogonToken = coreToken;
                FileInfo file = new FileInfo(filepath);
                coredeviceName = DeviceDiscovery.GetAlldeviceNameForSelectedIP(coreIP, selectedDUTItem);

                if (file.Exists)
                {
                    string exeversion = string.Empty;
                    System.IO.FileInfo fileinfo = new FileInfo(filepath);

                    string buildInfoFilePath = System.IO.Path.Combine(fileinfo.DirectoryName, "build.info");

                    if (System.IO.File.Exists(buildInfoFilePath))
                    {
                        
                        using (StreamReader exeBuildVersions = new StreamReader(buildInfoFilePath))
                        {
                            exeversion = exeBuildVersions.ReadToEnd().Replace("\n", string.Empty);
                        }
                       
                    }
                    else
                    {
                        var versInfo = FileVersionInfo.GetVersionInfo(filepath);
                        exeversion = versInfo.FileVersion.Trim();
                    }

                    //var versInfo = FileVersionInfo.GetVersionInfo(filepath);
                    //string exeversion = versInfo.FileVersion.Trim();

                    string remarks = string.Empty;
                    bool status = upgradationIntializationbylaunch(filepath, coreIP, exeversion, excID, isTimeCal, logPath, out remarks);

                    //if (isTimeCal && !status)
                    //    return new Tuple<bool, string, string>(false, string.Empty, remarks);

                    if (status)
                    {
                        string firmwarestatus = string.Empty;

                                //if (!isTimeCal)
                                //{
                                    while (firmwarestatus != "idle")
                                    {
                                        firmwarestatus = get_FirmwareLoadState(coreIP);
                                        if ((firmwarestatus == string.Empty) || (firmwarestatus == "complete"))
                                        {
                                            xmlResponseCount++;
                                            if (xmlResponseCount > 25)
                                            {
                                                DeviceDiscovery.WriteToLogFile("Firmware Upgradation status not responding");
                                                xmlResponseCount = 0;
                                                return new Tuple<bool, string, string>(false, string.Empty, remarks);
                                            }
                                            //break;
                                        }

                                        //Thread.Sleep(5000);
                                    }
                                //}
								
						//if (firmwarestatus == "idle" || isTimeCal)
                        if (firmwarestatus == "idle")
                        {
                            string firmwareversion = string.Empty;
                            firmwareversion = XmlReadToGetDesignversion(coreIP);
                            if (firmwareversion == exeversion)
                            {
                                DeviceDiscovery.WriteToLogFile("Firmware is succesfully Upgradated/downgraded to core through IP:" + coreIP + "");
                                //HttpGet("http://" + coreIP + "/cgi-bin/password_set?password1=" + m_strPassword + "&password2=" + m_strPassword + "", "", "EC15016", coreIP, out strResponse);
                                //return true;
                                return new Tuple<bool, string, string>(true, filepath, remarks);
                            }
                            else
                            {
                                DeviceDiscovery.WriteToLogFile("Firmware Upgradation failed. Current version in core:" + firmwareversion + "");
                                //return false;
                                return new Tuple<bool, string, string>(false, String.Empty, "Firmware Upgradation failed. Core:" + firmwareversion + " and Q-sys:" + exeversion + " versions are different");
                            }
                        }
                        else
                        {
                            DeviceDiscovery.WriteToLogFile("Firmware Upgradation failed. Current response from core:" + firmwarestatus + "");
                            //return false;
                            return new Tuple<bool, string, string>(false, String.Empty, remarks);
                        }
                    }
                    else
                    {
                        DeviceDiscovery.WriteToLogFile("Firmware Upgradation failed. Due to upgradation intialization failure");
                        //return false;
                        return new Tuple<bool, string, string>(false, String.Empty, remarks);
                    }
                }
                else
                {
                    DeviceDiscovery.WriteToLogFile("File not available in the preffered directory");
                    //return false;
                    return new Tuple<bool, string, string>(false, String.Empty, string.Empty);
                }
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                //MessageBox.Show("Exception\n " + ex.Message, "Error Code - ECxxxx", MessageBoxButton.OK, MessageBoxImage.Error);
                //return false;
                return new Tuple<bool, string, string>(false, String.Empty, string.Empty);
            }
        }

        public bool upgradationIntializationbylaunch(string applicationInstalledpath, string coreIP, string exeversion,int excId, bool isTimeCal, string logpath, out string remarks)
        {
            remarks = string.Empty;

            try
            {
                string strResponse = string.Empty;
                FileInfo filepath = new FileInfo(applicationInstalledpath);
                string fullpath = filepath.DirectoryName;
                string firmwarePath = string.Empty;
                string fileName = @"firmware.manifest";
                //firmwarePath = System.IO.Path.Combine(applicationInstalledpath, "Firmware\\core\\firmware.bin");
                string[] files = Directory.GetFiles(fullpath, fileName, SearchOption.AllDirectories);
                if (files.Length == 0)
                    files = Directory.GetFiles(fullpath, @"firmware.bin", SearchOption.AllDirectories);

                if (files.Length > 0)
                {
                    foreach (string corepath in files)
                    {
                        firmwarePath = corepath;
                    }
                    FileInfo coreFimware = new FileInfo(firmwarePath);
                    if (coreFimware.Exists)
                    {
                        bool processStatus = upgradationProcess(excId, firmwarePath, coreIP, exeversion, isTimeCal, logpath, out remarks);
                        if (processStatus)
                        {
                            //if(!isTimeCal)
                            //    DeviceDiscovery.WriteToLogFile("Firmware upgradation intialization completed successfully");

                            //HttpGet("http://" + coreIP + "/cgi-bin/password_set?password1=" + m_strPassword + "&password2=" + m_strPassword + "", "", "EC15016", coreIP, out strResponse);
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                    else
                    {
                        DeviceDiscovery.WriteToLogFile("Firmware not available in the preffered directory");
                        return false;
                    }
                }
                else
                {
                    DeviceDiscovery.WriteToLogFile("Firmware.bin not available in the preffered directory");
                    return false;
                }
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                //MessageBox.Show("Exception\n " + ex.Message, "Error Code - ECxxxx", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        public bool upgradationProcess(int excID, string firmwarePath, string CoreIP, string exeversion, bool isTimeCal, string logpath, out string remarks)
        {
            remarks = string.Empty;

            try
            {
                //////Get core hash id 
                var coreHashID = Deviceshashid(CoreIP);
                bool isCoreGreaterThanVersion9 = coreHashID.Item1;

                /////Get local hash id
                List<Dictionary<string, string>> firmwarePutList = new List<Dictionary<string, string>>();
                var strLocalHash = strGetLocalHash(isCoreGreaterThanVersion9, firmwarePath, out firmwarePutList);

                /////firmware upgrade new method or old method decided using useNewFirmwarePut boolean
                bool useNewFirmwarePut = strLocalHash.Item1;

                ////// firmware not supported error message 
                if(strLocalHash.Item3!=string.Empty)
                {
                    remarks = strLocalHash.Item3;
                    return false;
                }


                if (strLocalHash.Item2 != string.Empty && coreHashID.Item2 != string.Empty)
                {
                    bool updationStatus = false;

                    ////If both versions are equal skip firmware upgrade
                    if (strLocalHash.Item2.Equals(coreHashID.Item2))
                    {
                        DeviceDiscovery.WriteToLogFile("Firmware is already installed in the core");
                        return true;
                    }
                                 
                    /////from 9.1 and above ==> to 9.1 and above : new firmware pushed to core using Http Put method                          
                    if (useNewFirmwarePut)                    
                        updationStatus = set_FirmwareProgramNew(excID, CoreIP, firmwarePutList, strLocalHash.Item4, isTimeCal, logpath, out remarks);

                    /////from 9.1 and below ==> to 9.1 and below : old firmware pushed to core using Http post method   
                    else
                        updationStatus = set_FirmwareProgram(excID, CoreIP, strLocalHash.Item2, strLocalHash.Item4, exeversion, isTimeCal, logpath, out remarks);
                    

                    if (updationStatus)
                    {
                        DeviceDiscovery.WriteToLogFile("firmware upgradation to coreIP:" + CoreIP + " with HashValue:" + strLocalHash.Item2 + " is completed successfully");
                        return true;
                    }
                    else
                    {
                        DeviceDiscovery.WriteToLogFile("firmware upgradation failed coreIP:" + CoreIP + ",HashValue:" + strLocalHash.Item2 + "");
                        return false;
                    }
                }
                else
                {                  
                    if (strLocalHash.Item2 == string.Empty)
                    { DeviceDiscovery.WriteToLogFile("LocalHash not available"); }
                    else
                    { DeviceDiscovery.WriteToLogFile("Core Hash not available"); }
                    return false;
                }
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                //MessageBox.Show("Exception\n " + ex.Message, "Error Code - ECxxxx", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        public Tuple<bool, string,string, string> strGetLocalHash(bool isCoreNewVersion,string strPath, out List<Dictionary<string, string>> firmwarePutList)
        {
            firmwarePutList = new List<Dictionary<string, string>>();
            try
            {
                string errorMsg = string.Empty;
                bool useNewFirmwarePut = false;
                string hashContent = string.Empty;                

                string filename = Path.GetFileName(strPath);
                if (isCoreNewVersion && filename == "firmware.manifest")////ex:9.1 or above to 9.2 or vice versa
                {
                    useNewFirmwarePut = true;
                    firmwarePutList = ReadLocalHashDetailsNewFirmware(strPath, out hashContent);
                }
                else if ((isCoreNewVersion && filename == "firmware.bin") || (!isCoreNewVersion && filename == "firmware.bin"))
                {
                    hashContent = get_Local_Hash(strPath);
                }
                else if (!isCoreNewVersion && filename == "firmware.manifest")
                {
                    //////if coreversion is old and local designer is not having firmware.bin(has core squash files) that means local firmware version is Greater than 9.1
                    //////9.0 and below versions only support firmware upgrade for 9.1 version. If local version more than 9.1 it won't support firmware upgrade directly. But 9.2 to 9.0 firmware downgrade ==> supported                    
                    FileInfo fileinfo = new FileInfo(strPath);
                    string binFilepath = Path.Combine(fileinfo.DirectoryName, "firmware.bin");

                    if (File.Exists(binFilepath))/////ex:from 9.0 or below to 9.1 
                    {
                        hashContent = get_Local_Hash(binFilepath);
                        strPath = binFilepath;
                    }
                    else/////ex: from 9.0 or below to 9.2 
                    {                    
                        errorMsg = "Firmware Upgrade not supported from 9.0 or below versions to 9.2";
                    }
                }


                return new Tuple<bool, string, string, string>(useNewFirmwarePut, hashContent,errorMsg, strPath);
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                //MessageBox.Show("Exception\n " + ex.Message, "QAT Error Code - EC03021", MessageBoxButton.OK, MessageBoxImage.Error);
                return new Tuple<bool, string, string, string>(false,string.Empty,string.Empty, strPath);
            }
        }

        public string get_Local_Hash(string strFile)
        {
            try
            {
                string m_Local_Hash = crc.CalculateHash(strFile);
                DeviceDiscovery.WriteToLogFile("firmware's local HashValue is " + m_Local_Hash + "");
                return m_Local_Hash;
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                //MessageBox.Show("Exception\n " + ex.Message, "QAT Error Code - EC03021", MessageBoxButton.OK, MessageBoxImage.Error);
                return string.Empty;
            }
        }

        public Tuple<bool, string> Deviceshashid(string devicesip)
        {
            bool isCoreGreaterThanVersion9 = false;
            try
            {
                Int32 count = 0;
                string strResponse = string.Empty;
                string coreHash = string.Empty;
                if (devicesip != string.Empty & devicesip != "Not Applicable")
                {
                    //// try to get hash value using new api
                    isCoreGreaterThanVersion9 = GetHashCoreNew(devicesip, out strResponse);
                    
                    //// if new api not worked try to get hash value using old api
                    if (isCoreGreaterThanVersion9 == false)
                    {
                        while ((!HttpGet("http://" + devicesip + "/cgi-bin/hw_config?firmware_hash", m_strPassword, devicesip, string.Empty, "application/x-www-form-urlencoded", out strResponse)) && count < 5)
                        {
                            Thread.Sleep(2000);
                            count = count + 1;
                        }

                        /////incase error message returned from httpget set empty in hash 
                        if (strResponse == "401" || strResponse == "404")
                            strResponse = string.Empty;
                    }
                }

                coreHash = strResponse;
                DeviceDiscovery.WriteToLogFile("Core's local HashValue is " + coreHash + " get from the coreIP:" + devicesip + "");
                return new Tuple<bool, string>(isCoreGreaterThanVersion9, coreHash);
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                //MessageBox.Show("Exception\n " + ex.Message, "QAT Error Code - EC03021", MessageBoxButton.OK, MessageBoxImage.Error);
                return new Tuple<bool, string>(isCoreGreaterThanVersion9, string.Empty);
            }
        }

        public bool GetHashCoreNew(string devicesip, out string hash)
        {
            bool success = false;
            hash = string.Empty;
            try
            {               
                string strResponse = string.Empty;
                bool result = false;
                
                result = HttpGet("http://" + devicesip + "/api-qsd/v0/firmware", string.Empty, devicesip, coreLogonToken, "application/json", out strResponse);

                /////// if unauthorised log in for one time and try again
                if (strResponse == "401")
                {
                    string token = string.Empty;
                    Corelogon(devicesip);
                    result = HttpGet("http://" + devicesip + "/api-qsd/v0/firmware", string.Empty, devicesip, coreLogonToken, "application/json", out strResponse);
                }

                //////if strResponse not equal to 404 that means the new url request worked fine
                if (strResponse != "404")
                    success = true;

                if (result && strResponse != null && strResponse != string.Empty)
                {
                    dynamic array = new System.Web.Script.Serialization.JavaScriptSerializer().DeserializeObject(strResponse);

                    foreach (var item in array)
                    {
                        if(String.Equals(item.Key.ToString(),"manifest", StringComparison.CurrentCultureIgnoreCase))
                        {
                            foreach (var childItem in item.Value)
                            {
                                hash = childItem["hash"];                             
                                break;
                            }
                            if (hash != string.Empty)
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
            return success;
        }




        public bool Corelogon(string devicesip)
        {
            bool msg = false;          
            bool success = false;
            System.Net.HttpWebRequest req = null;
			
            try
            {
				string strParameters = "{\"username\":\"" + Properties.Settings.Default.DeviceUsername.ToString() + "\",\"password\":\"" + m_strPassword + "\"}";
            	string strURI = "http://" + devicesip + "/api/v0/logon";
                req = (HttpWebRequest)System.Net.WebRequest.Create(strURI);
                req.Timeout = 30000;
                req.ReadWriteTimeout = 30000;
                req.ContentType = "application/json";
                req.Method = "POST";              

                Byte[] retBytes = System.Text.Encoding.ASCII.GetBytes(strParameters);
                req.ContentLength = retBytes.Length;

                using (System.IO.Stream outStream = req.GetRequestStream())
                {
                    outStream.Write(retBytes, 0, retBytes.Length);
                    outStream.Close();
                }
                using (HttpWebResponse resp = (HttpWebResponse)req.GetResponse())
                {
                    if(resp.StatusCode == HttpStatusCode.OK || resp.StatusCode == HttpStatusCode.Created)
                    {
                        using (System.IO.StreamReader sr = new System.IO.StreamReader(resp.GetResponseStream()))
                        {
                            //getting token value
                            var obj = new System.Web.Script.Serialization.JavaScriptSerializer { MaxJsonLength = Int32.MaxValue }.Deserialize<dynamic>(sr.ReadToEnd().Trim());
                            if (obj.Count > 0)
                            {
                                foreach (var response in obj)
                                {
                                    coreLogonToken = response.Value;
                                }
                            }
                            success = true;
                        }
                    }                   
                }

                req.Abort();
                return success;
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                ExecutionMessageBox("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif              
                req.Abort();
                return success;
            }
        }




        public byte[] get_Firmware_Data(string strFile)
        {
            try
            {
                FileInfo file = new FileInfo(strFile);
                if (file.Exists)
                {
                    byte[] byteFirmImg = null;
                    byteFirmImg = FileToByteArray(strFile);
                    DeviceDiscovery.WriteToLogFile("Local Firmware is successfully converted to binary ");
                    return byteFirmImg;
                }
                else
                {
                    DeviceDiscovery.WriteToLogFile("Local Firmware is Not available ");
                    return null;
                }
            }
            catch(Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                DeviceDiscovery.WriteToLogFile("Local Firmware is Not available ");
                return null;
            }
        }

        public byte[] FileToByteArray(string fileName)
        {
            try
            {
                using (FileStream fs = new FileStream(fileName,
                                               FileMode.Open,
                                               FileAccess.Read))
                {
                    byte[] buff = null;

                    using (BinaryReader br = new BinaryReader(fs))
                    {
                        long numBytes = new FileInfo(fileName).Length;
                        buff = br.ReadBytes((int)numBytes);


                        DeviceDiscovery.WriteToLogFile("Local Firmware to binary conversion completed ");
                        return buff;
                    }
                }
            }
            catch (Exception ex)
            {

                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                //MessageBox.Show("Exception\n " + ex.Message, "Error Code - ECxxxx", MessageBoxButton.OK, MessageBoxImage.Error);
                DeviceDiscovery.WriteToLogFile("Local Firmware to binary conversion failed ");
                return null;
            }
        }

        public string XmlReadToGetDeviceName(string strIP)
        {
            string deviceName = string.Empty;
            try
            {
                XmlDocument xml = new XmlDocument();
                if (strIP != string.Empty & strIP != "Not Applicable")
                {
                    xml = DeviceDiscovery.XmlLoadUsingHttp("http://" + strIP + "//cgi-bin/status_xml", "XmlReadToGetDesignversion");//device_name
                    if (xml == null)
                    {
                        xml = DeviceDiscovery.XmlLoadUsingHttp("http://" + strIP + "/cgi-bin/status_xml", "XmlReadToGetDesignversion");//device_name
                    }

                    if (xml == null)
                        return string.Empty;

                    XmlNode node = xml.SelectSingleNode("status/device_name");
                    deviceName = node.InnerText;
                    DeviceDiscovery.WriteToLogFile("device IP:" + strIP + ",DeviceName:" + deviceName + " ");
                }

                return deviceName;
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                ExecutionMessageBox("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                if (ex.Message.Contains("The remote server returned an error: (401) Unauthorized"))
                {
                    DeviceDiscovery.WriteToLogFile("Invalid Username or Password");
                }
                else
                {
                    DeviceDiscovery.WriteToLogFile("Exception during device_name fetching");
                }
                DeviceDiscovery.WriteToLogFile("device_name not retrieved ");
                return string.Empty;
            }
        }

        private Tuple<bool, string> HttpPostActual(string strURI, string firmwarePath, out string strResponse, string strIP, string exeversion)
        {
            strResponse = string.Empty;
            int i = 0;
            Tuple<bool, string> resp = new Tuple<bool, string>(false, string.Empty);

            try
            {
            Loop1:
                resp = HttpPost(strURI, firmwarePath, out strResponse, strIP, exeversion);
                i++;

                if (!resp.Item1 && i <= 2 && resp.Item2.Contains("Unable to write data to the transport connection"))
                {
                    if (i == 1)
                        Thread.Sleep(5000);
                    else if (i == 2)
                        Thread.Sleep(20000);

                    DeviceDiscovery.WriteToLogFile("HttpPost retry started for iteration " + i);
                    goto Loop1;
                }
            }
            catch(Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                ExecutionMessageBox("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
            }

            return resp;
        }

        public bool set_FirmwareProgram(int excID, string strIP, string strLocalHash, string firmwarePath, string exeversion, bool isTimeCal, string logpath, out string remarks)
        {
            remarks = string.Empty;

            try
            {
                string strURIPath = string.Empty;

                if (strIP != string.Empty & strIP != "Not Applicable")
                    strURIPath = "http://" + strIP + "/cgi-bin/package_install?hash=" + strLocalHash + "&v2_license_support=1";

                if (strURIPath != string.Empty)
                {
                    string strReponse = "";

                    if (isTimeCal)
                    {
                        DateTime pushStart = DateTime.Now;
                        DateTime? pushEnd = null;
                        DateTime? writeStart = null;
                        DateTime? writeEnd = null;
                        DateTime? rebootStart = null;
                        DateTime? rebootEnd = null;

                        string coreName = XmlReadToGetDeviceName(strIP);

                        var resp = HttpPostActual(strURIPath, firmwarePath, out strReponse, strIP, exeversion);

                        if (resp.Item1)
                        {
                            Debug.WriteLine("QSys_Web_RW.set_FirmwareProgram.strResponse = " + strReponse);
                            if (strReponse == "Started")
                            {
                                pushEnd = DateTime.Now;
                                DeviceDiscovery.WriteToLogFile("Firmware is successfully posted to core and upgradation started.... ");
                                //return true;
                            }
                        }
                        else
                        {
                            if (string.IsNullOrEmpty(resp.Item2))
                            {
                                DeviceDiscovery.WriteToLogFile("Error occured while post firmware to core");
                                remarks = "Error occured while post firmware to core";
                                return false;
                            }
                            else
                            {
                                DeviceDiscovery.WriteToLogFile("Unable to post firmware to core." + resp.Item2);
                                remarks = "Unable to post firmware to core." + resp.Item2;
                                return false;
                            }
                        }

                        if (pushEnd != null)
                        {
                            string firmwarestatus = string.Empty;
                            DateTime maxExecuTime = DateTime.Now.AddSeconds(Properties.Settings.Default.FirmwareTime_upperlimit);
                            writeStart = DateTime.Now;

                            while (maxExecuTime > DateTime.Now)
                            {
                                firmwarestatus = get_FirmwareLoadStateWithoutSleep(strIP);

                                if (firmwarestatus.ToLower().Trim() == "complete" || firmwarestatus.ToLower().StartsWith("Update Finished"))
                                {
                                    DeviceDiscovery.WriteToLogFile("Firmware write is successfully done");
                                    writeEnd = rebootStart = DateTime.Now;
                                }
                                else if (firmwarestatus.ToLower().Trim() == "idle")
                                {
                                    DeviceDiscovery.WriteToLogFile("Core is rebooted successfully after firmware upgradation");
                                    rebootEnd = DateTime.Now;
                                    break;
                                }
                            }

                            TimeSpan? pushTotal = pushEnd - pushStart;
                            TimeSpan? writeTotal = writeEnd - writeStart;
                            TimeSpan? rebootTotal = rebootEnd - rebootStart;
                            TimeSpan? totalUpgrade = rebootEnd - pushStart;

                            var directory = new DirectoryInfo(logpath + "\\FirmwareTimeCal");

                            if (!directory.Exists)
                            {
                                Directory.CreateDirectory(directory.FullName);
                            }

                            List<string> outPutInsec = new List<string>();
                            List<string> remarksList = new List<string>();
                            if (pushTotal != null)
                                outPutInsec.Add("PushTime : " + pushTotal.Value.TotalSeconds + " seconds");

                            if (writeTotal != null)
                                outPutInsec.Add("WriteTime : " + writeTotal.Value.TotalSeconds + " seconds");
                            else
                                remarksList.Add("firmware write time");

                            if (rebootTotal != null)
                                outPutInsec.Add("RebootTime : " + rebootTotal.Value.TotalSeconds + " seconds");
                            else
                                remarksList.Add("reboot time");

                            if (totalUpgrade != null)
                                outPutInsec.Add("TotalTime : " + totalUpgrade.Value.TotalSeconds + " seconds");
                            else
                                remarksList.Add("total time");

                            using (StreamWriter write = new StreamWriter(directory.FullName + "\\" + coreName + "CoreFirmwareTime.txt"))
                            {
                                write.Write(string.Join("\n", outPutInsec));
                            }

                            if (remarksList.Count == 0)
                            {
                                remarks = string.Join("<br/>", outPutInsec);
                                //bool isSuccess = HttpPostFile(strIP, "http://" + strIP + "/api/v0/cores/self/media/Audio/", directory.FullName + "\\CoreFirmwareTime.txt", "http://" + strIP + "/audio-files//Audio");
                                return true;
                            }
                            else
                            {
                                if (firmwarestatus.ToLower().Trim() != "idle")
                                {
                                    int timeInmin = Properties.Settings.Default.FirmwareTime_upperlimit / 60;
                                    int remainSec = Properties.Settings.Default.FirmwareTime_upperlimit % 60;

                                    remarks = "Unable to complete firmware upgrade while calculating " + string.Join(", ", remarksList) + ". Maximum time reached " + timeInmin + " mins : " + remainSec + " sec";
                                    return false;
                                }
                                else
                                {
                                    remarks = "Firmware time measurement failed while calculating " + string.Join(", ", remarksList);
                                    return false;
                                }
                            }

                            //if (string.IsNullOrEmpty(remarks))
                            //{
                            //    bool isSuccess = HttpPostFile(strIP, "http://" + strIP + "/api/v0/cores/self/media/Audio/", directory.FullName + "\\CoreFirmwareTime.txt", "http://" + strIP + "/audio-files//Audio");
                            //    return true;
                            //}
                            //else
                            //    return false;
                        }
                        else
                        {
                            DeviceDiscovery.WriteToLogFile("Firmware is not posted to core and upgradation failed.... ");
                            remarks = "Unable to post firmware to core";
                            return false;
                        }
                    }
                    else
                    {
                        var resp = HttpPostActual(strURIPath, firmwarePath, out strReponse, strIP, exeversion);

                        if (resp.Item1)
                        {
                            Debug.WriteLine("QSys_Web_RW.set_FirmwareProgram.strResponse = " + strReponse);
                            if (strReponse == "Started")
                            {
                                DeviceDiscovery.WriteToLogFile("Firmware is successfully posted to core and upgradation started.... ");
                                return true;
                            }
                        }
                        else
                        {
                            DeviceDiscovery.WriteToLogFile("Firmware is not posted to core and upgradation failed");
                            remarks = "Firmware is not posted to core and upgradation failed";
                            return false;
                        }

                    }
                }
                else
                {
                    DeviceDiscovery.WriteToLogFile("Core IP is unavailable");
                    remarks = "Core IP is unavailable";
                    return false;
                }

                DeviceDiscovery.WriteToLogFile("Firmware is not posted to core and upgradation failed.... ");
                return false;
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                //MessageBox.Show("Exception\n " + ex.Message, "Error Code - ECxxxxx", MessageBoxButton.OK, MessageBoxImage.Error);
                DeviceDiscovery.WriteToLogFile("Firmware is not posted to core and upgradation failed.... ");

                if (isTimeCal)
                    remarks = "Error occurred so Firmware is not posted to core and upgradation failed";

                return false;
            }
        }


        public List<Dictionary<string, string>> ReadLocalHashDetailsNewFirmware(string strPath, out string localCorehashValue)
        {
            List<Dictionary<string, string>> firmwareDetailsArray = new List<Dictionary<string, string>>();
            localCorehashValue = string.Empty;
            try
            {
                using (StreamReader read = new StreamReader(strPath))
                {
                    dynamic array = new System.Web.Script.Serialization.JavaScriptSerializer().DeserializeObject(read.ReadToEnd());

                    foreach (Dictionary<string, object> item in array)
                    {
                        Dictionary<string, string> firmwareDetails = new Dictionary<string, string>();

                        firmwareDetails.Add("fileName", item["fileName"].ToString());
                        firmwareDetails.Add("signature", item["signature"].ToString());
                        firmwareDetails.Add("hash", item["hash"].ToString());
                        firmwareDetails.Add("contentType", item["contentType"].ToString());
                        firmwareDetails.Add("reboot", item["reboot"].ToString());
                        firmwareDetails.Add("package", item["package"].ToString());
                        firmwareDetailsArray.Add(firmwareDetails);
                        /////core hash assigning
                        if (item["package"].ToString() == "coreOS")
                            localCorehashValue = item["hash"].ToString();
                    }
                }

                

            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Error occurred while reading Firmware.manifest file:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
            }

            return firmwareDetailsArray;
        }


        private Tuple<bool, string, string[]> HttpPut(string strURI, string firmwarePath, string strIP,string contentType, out string strResponse)
        {
            strResponse = string.Empty;
            int i = 0;
            Tuple<bool, string, string[]> resp = new Tuple<bool, string, string[]>(false, string.Empty, null);

            try
            {
                Loop1:
                resp = HttpPutActual(strURI, firmwarePath, strIP, contentType, out strResponse);
                i++;

                if (!resp.Item1 && i <= 2 && resp.Item2.Contains("Unable to write data to the transport connection"))
                {
                    if (i == 1)
                        Thread.Sleep(5000);
                    else if (i == 2)
                        Thread.Sleep(20000);

                    DeviceDiscovery.WriteToLogFile("HttpPut retry started for iteration " + i);
                    goto Loop1;
                }
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                ExecutionMessageBox("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
            }

            return resp;
        }

        public Tuple<bool, string,  string[]> HttpPutActual(string strURI, string firmwarePath, string strIP, string contentType, out string strResponse)
        {           
            strResponse = string.Empty;
            bool success = false;
            string message = string.Empty;
            System.Net.HttpWebRequest req = null;
            string[] coreResponse = new string[2];

            try
            {
                using (FileStream inStream = new FileStream(firmwarePath, FileMode.Open, FileAccess.Read))
                {
                    req = (HttpWebRequest)System.Net.WebRequest.Create(strURI);
                    req.ContentType = contentType;
                    req.Method = "PUT";
                    req.Timeout = 1000000;
                    req.ReadWriteTimeout = 300000;
                    req.AllowWriteStreamBuffering = false;
                    req.ContentLength = inStream.Length;
                    req.Headers["Authorization"] = "Bearer " + coreLogonToken;

                    //////Data to be put
                    using (System.IO.Stream outStream = req.GetRequestStream())
                    {
                        const int inBufferSize = 32768;//32kb

                        while (inStream.Position < inStream.Length)
                        {
                            byte[] chunkData = new byte[inBufferSize];
                            int chunkDataRead = inStream.Read(chunkData, 0, inBufferSize);
                            outStream.Write(chunkData, 0, chunkDataRead);
                        }

                        outStream.Flush();
                        outStream.Close();
                    }

                    ////Get response after put
                    using (HttpWebResponse resp = (HttpWebResponse)req.GetResponse())
                    {
                        if (resp == null)
                        {
                            strResponse = "";                           
                            req.Abort();                           
                        }

                        if (resp.StatusCode == HttpStatusCode.OK || resp.StatusCode == HttpStatusCode.Created || resp.StatusCode == HttpStatusCode.NoContent)
                        {
                            using (System.IO.StreamReader sr = new System.IO.StreamReader(resp.GetResponseStream()))
                            {
                                strResponse = sr.ReadToEnd().Trim();
                                req.Abort();
                                success = true;
                                DeviceDiscovery.WriteToLogFile("Core is started to respond for http put.... ");

                                dynamic array = new System.Web.Script.Serialization.JavaScriptSerializer().DeserializeObject(strResponse);
                                coreResponse[0] = array["status"]["state"].ToString();
                                coreResponse[1] = array["reboot"].ToString();
                            }
                        }
                    }
                }
            }
            catch (WebException e)
            {
                if (req != null)
                    req.Abort();
                
                DeviceDiscovery.WriteToLogFile("No Response from core Due to WebException in HTTP PUT" + e.ToString());
                strResponse = "";
                return new Tuple<bool, string, string[]>(false, e.Message, coreResponse);
            }
            catch (Exception ex)
            {
                if (req != null)
                    req.Abort();
               
                DeviceDiscovery.WriteToLogFile("No Response from core for HTTP PUT." + ex.Message.ToString());
                strResponse = "";
                return new Tuple<bool, string, string[]>(false, ex.Message, coreResponse);
            }           

            return new Tuple<bool, string, string[]>(success, message, coreResponse);
        }

        public bool set_FirmwareProgramNew(int excID, string strIP, List< Dictionary<string, string>> firmwarePutList, string firmwarePath,  bool isTimeCal, string logpath, out string remarks)
        {
            remarks = string.Empty;

            try
            {
                if (!isTimeCal)
                {
                    foreach (Dictionary<string, string> item in firmwarePutList)
                    {
                        string strURIPath = string.Empty;

                        if (strIP != string.Empty & strIP != "Not Applicable")
                            strURIPath = "http://" + strIP + "/api-qsd/v0/firmware/?signature=" + System.Web.HttpUtility.UrlEncode(item["signature"]) + "&hash=" + item["hash"] + "&reboot=" + item["reboot"];

                        FileInfo fileinfo = new FileInfo(firmwarePath);
                        string putFilepath = Path.Combine(fileinfo.DirectoryName, item["fileName"]);

                        if (strURIPath != string.Empty)
                        {
                            string strResponse = string.Empty;
                            var successStatus = HttpPut(strURIPath, putFilepath, strIP, item["contentType"], out strResponse);

                            //var successStatus = FirmwareUpgradeWithoutTimeCalculation(strURIPath, putFilepath, strIP, item["contentType"]);
                            if (!successStatus.Item1 && (successStatus.Item3 == null || successStatus.Item3[0] != "idle"))
                            {
                                remarks = successStatus.Item2;
                                return false;
                            }
                            else if(successStatus.Item1 && successStatus.Item3[1].StartsWith("Rebooting..."))
                            {  
                                ///wait until reboot starts (Before verifying idle state in firmware status, fw_status.xml should be unavailable first and recovered again)
                                Thread.Sleep(30000);
                            }
                        }
                        else
                        {
                            DeviceDiscovery.WriteToLogFile("Core IP is unavailable");
                            remarks = "Core IP is unavailable";
                            return false;
                        }
                    }

                    return true;
                }
                else
                {
                    Dictionary<string, DateTime> firmwarePushStart = new Dictionary<string, DateTime>();
                    Dictionary<string, DateTime> firmwarePushEnd = new Dictionary<string, DateTime>();
                    DateTime? rebootStart = null;
                    DateTime? rebootEnd = null;

                    string coreName = XmlReadToGetDeviceName(strIP);

                    int i = 0;
                    string firmwarestatus = string.Empty;

                    foreach (Dictionary<string, string> item in firmwarePutList)
                    {
                        i++;
                        string strURIPath = string.Empty;

                        if (strIP != string.Empty & strIP != "Not Applicable")
                            strURIPath = "http://" + strIP + "/api-qsd/v0/firmware/?signature=" + System.Web.HttpUtility.UrlEncode(item["signature"]) + "&hash=" + item["hash"] + "&reboot=" + item["reboot"];

                        FileInfo fileinfo = new FileInfo(firmwarePath);
                        string putFilepath = Path.Combine(fileinfo.DirectoryName, item["fileName"]);

                        if (strURIPath != string.Empty)
                        {
                            firmwarePushStart.Add("Firmware" + i, DateTime.Now);

                            string strReponse = "";
                            var resp = HttpPut(strURIPath, putFilepath, strIP, item["contentType"], out strReponse);

                            if (resp.Item1 && resp.Item3 != null && resp.Item3[0] == "idle")
                            {
                                firmwarePushEnd.Add("Firmware" + i, DateTime.Now);

                                if (resp.Item3[1].StartsWith("Rebooting..."))
                                {
                                    rebootStart = DateTime.Now;

                                    ///wait until reboot starts (Before verifying idle state in firmware status, fw_status.xml should be unavailable first and recovered again)
                                    Thread.Sleep(30000);

                                    DateTime? maxExecuTime = DateTime.Now.AddSeconds(Properties.Settings.Default.FirmwareTime_upperlimit);

                                    while (maxExecuTime > DateTime.Now)
                                    {
                                        firmwarestatus = get_FirmwareLoadState_New(strIP);

                                        if (firmwarestatus.ToLower().Trim() == "idle")
                                        {
                                            DeviceDiscovery.WriteToLogFile("Core is rebooted successfully after firmware upgradation");
                                            rebootEnd = DateTime.Now;
                                            break;
                                        }

                                        Thread.Sleep(5000);
                                    }
                                }

                                DeviceDiscovery.WriteToLogFile("Firmware write is successfully done for Firmware" + i);
                            }
                            else
                            {
                                if (string.IsNullOrEmpty(resp.Item2))
                                {
                                    DeviceDiscovery.WriteToLogFile("Error occured while post firmware to core");
                                    remarks = "Error occured while post firmware to core";
                                    return false;
                                }
                                else
                                {
                                    DeviceDiscovery.WriteToLogFile("Unable to post firmware to core." + resp.Item2);
                                    remarks = "Unable to post firmware to core." + resp.Item2;
                                    return false;
                                }
                            }
                        }
                        else
                        {
                            DeviceDiscovery.WriteToLogFile("Core IP is unavailable");
                            remarks = "Core IP is unavailable";
                            return false;
                        }
                    }

                    List<string> outPutInsec = new List<string>();
                    List<string> remarksList = new List<string>();
                    DateTime? firmwarePushStarttime = null;

                    for (int j = 1; j <= firmwarePutList.Count; j++)
                    {
                        if (j == 1 && firmwarePushStart.ContainsKey("Firmware" + j))
                            firmwarePushStarttime = firmwarePushStart["Firmware" + j];

                        if (firmwarePushStart.ContainsKey("Firmware" + j) && firmwarePushEnd.ContainsKey("Firmware" + j))
                        {
                            TimeSpan pushTotal = firmwarePushEnd["Firmware" + j] - firmwarePushStart["Firmware" + j];

                            if (firmwarePutList.Count == 1)
                                outPutInsec.Add("Push and WriteTime : " + pushTotal.TotalSeconds + " seconds");
                            else
                                outPutInsec.Add("Firmware " + j + " Push and WriteTime : " + pushTotal.TotalSeconds + " seconds");
                        }
                        else
                        {
                            remarksList.Add("Firmware " + j + " push and write time");
                        }
                    }

                    TimeSpan? rebootTotal = rebootEnd - rebootStart;
                    TimeSpan? totalUpgrade = rebootEnd - firmwarePushStarttime;

                    if (rebootTotal != null)
                        outPutInsec.Add("RebootTime : " + rebootTotal.Value.TotalSeconds + " seconds");
                    else
                        remarksList.Add("reboot time");

                    if (totalUpgrade != null)
                        outPutInsec.Add("TotalTime : " + totalUpgrade.Value.TotalSeconds + " seconds");
                    else
                        remarksList.Add("total time");

                    var directory = new DirectoryInfo(logpath + "\\FirmwareTimeCal");

                    if (!directory.Exists)
                    {
                        Directory.CreateDirectory(directory.FullName);
                    }

                    using (StreamWriter write = new StreamWriter(directory.FullName + "\\" + coreName + "CoreFirmwareTime.txt"))
                    {
                        write.Write(string.Join("\n", outPutInsec));
                    }

                    if (remarksList.Count == 0)
                    {
                        remarks = string.Join("<br/>", outPutInsec);
                        return true;
                    }
                    else
                    {
                        if (firmwarestatus.ToLower().Trim() != "idle")
                        {
                            int timeInmin = Properties.Settings.Default.FirmwareTime_upperlimit / 60;
                            int remainSec = Properties.Settings.Default.FirmwareTime_upperlimit % 60;

                            remarks = "Unable to complete firmware upgrade while calculating " + string.Join(", ", remarksList) + ". Maximum time reached " + timeInmin + " mins : " + remainSec + " sec";
                            return false;
                        }
                        else
                        {
                            remarks = "Firmware time measurement failed while calculating " + string.Join(", ", remarksList);
                            return false;
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
                DeviceDiscovery.WriteToLogFile("Firmware is not posted to core and upgradation failed.... ");

                if (isTimeCal)
                    remarks = "Error occurred so Firmware is not posted to core and upgradation failed";

                return false;
            }
        }
		
        private Tuple<bool, string> FirmwareUpgradeWithoutTimeCalculation(string strURIPath, string putFilepath, string strIP, string contentType)
        {
            string remarks = string.Empty;
            bool success = false;
            try
            {
                string strReponse = string.Empty;
                var resp = HttpPut(strURIPath, putFilepath, strIP,  contentType, out strReponse);

                if (resp.Item1)
                {
                    Debug.WriteLine("QSys_Web_RW.set_FirmwareProgram.strResponse = " + strReponse);

                    ////Madhu - Timing doubt
                    success = true;
                    DateTime maxExecuTime = DateTime.Now.AddSeconds(Properties.Settings.Default.FirmwareTime_upperlimit);

                    while (maxExecuTime > DateTime.Now)
                    {                     
                        string firmwarestatus = get_FirmwareLoadState_New(strIP);

                        if (firmwarestatus.ToLower().Trim() == "idle")
                        {
                            return new Tuple<bool, string>(success, string.Empty);
                        }
                    }
                }
                else
                {
                    DeviceDiscovery.WriteToLogFile("Firmware is not posted to core and upgradation failed");
                    remarks = "Firmware is not posted to core and upgradation failed";
                }
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Firmware is not posted to core and upgradation failed.... ");
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                remarks = "Error occurred so Firmware is not posted to core and upgradation failed";
            }

            return new Tuple<bool, string>(success, remarks);
        }

        public string get_FirmwareLoadState_New(string strIP)
        {           
            try
            {
                string status = string.Empty;
                string outputstring = string.Empty;
                bool result = HttpGet("http://" + strIP + "/api-qsd/v0/firmware", string.Empty, strIP, coreLogonToken, "application/json", out outputstring);

                /////// if unauthorised log in for one time and try again
                if (outputstring == "401")
                {
                    string token = string.Empty;
                    Corelogon(strIP);
                    result = HttpGet("http://" + strIP + "/api-qsd/v0/firmware", string.Empty, strIP, coreLogonToken, "application/json", out outputstring);
                }

                ////Reading status from json response
                if (result && outputstring != null && outputstring != string.Empty)
                {
                    dynamic array = new System.Web.Script.Serialization.JavaScriptSerializer().DeserializeObject(outputstring);
                    status = array["status"]["state"];                  
                }

                return status;
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("The remote server returned an error: (401) Unauthorized"))
                {
                    DeviceDiscovery.WriteToLogFile("Invalid Username or Password");
                }
                else
                {
                    DeviceDiscovery.WriteToLogFile("Exception during Firmware status fetching");
                }

#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                return string.Empty;
            }
        }



        //        private bool HttpPostFile(string strIP, string strURI, string filePath, string referer)
        //        {
        //            bool success = false;

        //            try
        //            {
        //                HttpWebRequest req = (HttpWebRequest)WebRequest.Create(strURI);
        //                string boundaryString = "-------------196629634335992786111817217361";

        //                req.Method = WebRequestMethods.Http.Post;
        //                req.ContentType = "multipart/form-data; boundary=" + boundaryString;
        //                req.KeepAlive = true;
        //                req.Credentials = System.Net.CredentialCache.DefaultCredentials;
        //                req.Accept = "application/json,text/plain,*/*";
        //                req.Referer = referer;

        //                var isNewver = firmwareVersioncheck(strIP);
        //                if (isNewver.Item1)
        //                {
        //                    string token = string.Empty;
        //                    var logonsuccess = Corelogon(strIP, m_strPassword, out token);
        //                    if (token != string.Empty)
        //                        req.Headers["Authorization"] = "Bearer " + token;
        //                }
        //                else
        //                {
        //                    SetBasicAuthHeader(ref req, m_strUserName, m_strPassword);
        //                }

        //                using (MemoryStream poststream = new MemoryStream())
        //                {
        //                    StreamWriter postDataWriter = new StreamWriter(poststream);
        //                    postDataWriter.Write("\r\n--" + boundaryString + "\r\n");
        //                    postDataWriter.Write("Content-Disposition: form-data;" + "name=\"{0}\";" + "filename=\"{1}\"" + "\r\nContent-Type: {2}\r\n\r\n", "media", System.IO.Path.GetFileName(filePath), "text/plain");
        //                    postDataWriter.Flush();

        //                    using (FileStream inStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
        //                    {
        //                        const int inBufferSize = 32768;//32kb
        //                        while (inStream.Position < inStream.Length)
        //                        {
        //                            byte[] chunkData = new byte[inBufferSize];
        //                            int chunkDataRead = inStream.Read(chunkData, 0, inBufferSize);
        //                            poststream.Write(chunkData, 0, chunkDataRead);
        //                        }

        //                        postDataWriter.Write("\r\n--" + boundaryString + "--\r\n");
        //                        postDataWriter.Flush();

        //                        req.ContentLength = poststream.Length;

        //                        using (Stream s = req.GetRequestStream())
        //                        {
        //                            poststream.WriteTo(s);
        //                        }
        //                    }
        //                }

        //                using (System.Net.WebResponse resp = req.GetResponse())
        //                {
        //                    if (resp == null)
        //                    {
        //                        req.Abort();
        //                    }
        //                    else if (((HttpWebResponse)resp).StatusCode.ToString().ToUpper() == "CREATED" || ((HttpWebResponse)resp).StatusCode.ToString().ToUpper() == "CONTINUE" || ((HttpWebResponse)resp).StatusCode.ToString().ToUpper() == "OK")
        //                    {
        //                        success = true;
        //                    }
        //                }
        //            }
        //            catch (Exception ex)
        //            {
        //                Debug.WriteLine("Error in Web_RW.HttpPost: no response from " + strURI.ToString() + "\n\r" + ex.ToString());
        //                DeviceDiscovery.WriteToLogFile("No Response from core for HTTP Post." + ex.ToString());
        //            }

        //            return success;
        //        }

        //        public Tuple<bool, string> firmwareVersioncheck(string ipaddress)
        //        {
        //            bool isnewversion = true;
        //            string getFirmwareVersion = string.Empty;
        //            try
        //            {
        //                getFirmwareVersion = XmlReadToGetDesignversion(ipaddress);
        //                string regex = Regex.Match(getFirmwareVersion, @"\d.*").Value;
        //                Version firmwarever = new Version(regex);
        //                if (firmwarever < new Version(Properties.Settings.Default.CompareFirmwareVersion))
        //                    isnewversion = false;

        //            }
        //            catch (Exception ex)
        //            {
        //                isnewversion = true;
        //            }

        //            return new Tuple<bool, string>(isnewversion, getFirmwareVersion);
        //        }

        //        public Tuple<bool, bool, string> Corelogon(string ipaddress, string userpassword, out string token)
        //        {
        //            string strResponse = string.Empty;
        //            string strParameters = "{\"username\":\"" + Properties.Settings.Default.DeviceUsername.ToString() + "\",\"password\":\"" + userpassword + "\"}";
        //            try
        //            {
        //                var success = HttpPost_json("http://" + ipaddress + "/api/v0/logon", strParameters, out strResponse);

        //                //get Token value
        //                if (success.Item1)
        //                {
        //                    var obj = new JavaScriptSerializer { MaxJsonLength = Int32.MaxValue }.Deserialize<dynamic>(strResponse);
        //                    if (obj.Count > 0)
        //                    {
        //                        foreach (var response in obj)
        //                        {
        //                            strResponse = response.Value;
        //                        }
        //                    }
        //                }

        //                token = strResponse;
        //                return success;
        //            }
        //            catch (Exception ex)
        //            {
        //                token = string.Empty;
        //                return new Tuple<bool, bool, string>(false, false, string.Empty);
        //            }
        //        }

        //        public Tuple<bool, bool, string> HttpPost_json(string strURI, string strParameters, out string strResponse)
        //        {
        //            Tuple<bool, bool, string> Check = new Tuple<bool, bool, string>(false, false, string.Empty);
        //            Int32 RetryCount = 0;
        //            strResponse = "";

        //            try
        //            {
        //                while (RetryCount < 5)
        //                {
        //                    Check = HttpPostactual_json(strURI, strParameters, out strResponse);
        //                    if (Check.Item1 || Check.Item3 == "404" || Check.Item3 == "403")
        //                    {
        //                        break;
        //                    }

        //                    RetryCount++;
        //                };
        //                return Check;
        //            }

        //            catch (Exception ex)
        //            {
        //                return Check;
        //            }
        //        }

        //        public Tuple<bool, bool, string> HttpPostactual_json(string strURI, string strParameters, out string strResponse)
        //        {
        //            bool msg = false;
        //            strResponse = string.Empty;
        //            bool success = false;
        //            System.Net.HttpWebRequest req = (HttpWebRequest)System.Net.WebRequest.Create(strURI);
        //            try
        //            {
        //                req.Timeout = 15000;
        //                req.ReadWriteTimeout = 15000;
        //                req.ContentType = "application/json";
        //                req.Method = "POST";

        //                Byte[] retBytes = System.Text.Encoding.ASCII.GetBytes(strParameters);
        //                req.ContentLength = retBytes.Length;

        //                using (System.IO.Stream outStream = req.GetRequestStream())
        //                {
        //                    outStream.Write(retBytes, 0, retBytes.Length);
        //                    outStream.Close();
        //                }
        //                using (HttpWebResponse resp = (HttpWebResponse)req.GetResponse())
        //                {
        //                    success = HttpStatusCodeCheck(resp, "POST", out strResponse);
        //                }

        //                req.Abort();
        //                return new Tuple<bool, bool, string>(success, msg, string.Empty);
        //            }
        //            catch (Exception ex)
        //            {
        //                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
        //#if DEBUG
        //                ExecutionMessageBox("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
        //#endif
        //                strResponse = "";
        //                string errorMsg = string.Empty;
        //                if (ex.Message == "Unable to connect to the remote server" || ex.Message == "The operation has timed out")
        //                {
        //                    msg = true;
        //                }

        //                if (ex.Message == "The remote server returned an error: (404) Not Found.")
        //                {
        //                    errorMsg = "404";
        //                }

        //                if (ex.Message == "The remote server returned an error: (403) Forbidden.")
        //                {
        //                    msg = true;
        //                    errorMsg = "403";
        //                }

        //                req.Abort();
        //                return new Tuple<bool, bool, string>(success, msg, errorMsg);
        //            }
        //        }

        //        private bool HttpStatusCodeCheck(HttpWebResponse response, string methodName, out string strResponse)
        //        {
        //            strResponse = string.Empty;
        //            try
        //            {
        //                if (response == null)
        //                    return false;

        //                if (((methodName == "GET") && (response.StatusCode == HttpStatusCode.OK))
        //                    || ((methodName == "POST") && (response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.Created))
        //                    || ((methodName == "PUT") && (response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.Created || response.StatusCode == HttpStatusCode.NoContent)))
        //                {
        //                    using (System.IO.StreamReader sr = new System.IO.StreamReader(response.GetResponseStream()))
        //                    {
        //                        strResponse = sr.ReadToEnd().Trim();
        //                        return true;
        //                    }
        //                }

        //                return false;
        //            }
        //            catch (Exception ex)
        //            {
        //                return false;
        //            }
        //        }

        public Tuple<bool, string> HttpPost(string strURI, string firmwarePath, out string strResponse, string strIP, string exeversion)
        {
            System.Net.HttpWebRequest req = null;

            try
            {
                using (FileStream inStream = new FileStream(firmwarePath, FileMode.Open, FileAccess.Read))
                {
                    //Getting core current password
                    DeviceDiscovery.WriteToLogFile("####Core password before HTTP POST####");
                    //getCorepassword(strIP);

                    req = (HttpWebRequest)System.Net.WebRequest.Create(strURI);

                    req.Credentials = new NetworkCredential(m_strUserName, m_strPassword);
                    req.PreAuthenticate = true;
                    req.UserAgent = "Upload Test";
                    req.Method = "POST";
                    req.AllowWriteStreamBuffering = false;
                    req.Timeout = 1000000;
                    req.ContentType = "application/x-www-form-urlencoded";
                    req.ContentLength = inStream.Length;
                    req.ReadWriteTimeout = 240000;
                    SetBasicAuthHeader(ref req, m_strUserName, m_strPassword);
                    DeviceDiscovery.WriteToLogFile("Core username & password before HTTP POST:" + m_strUserName + "," + m_strPassword + "");


                    // Need to setup Authentication Header.
                    //SetBasicAuthHeader(ref req, "admin", m_strPassword);
                    using (System.IO.Stream outStream = req.GetRequestStream())
                    {
                        const int inBufferSize = 32768;//32kb

                        while (inStream.Position < inStream.Length)
                        {
                            byte[] chunkData = new byte[inBufferSize];
                            int chunkDataRead = inStream.Read(chunkData, 0, inBufferSize);
                            outStream.Write(chunkData, 0, chunkDataRead);
                            //Console.WriteLine("-------" + inStream.Position + "-----" + inStream.Length);
                        }

                        outStream.Flush();
                        outStream.Close();
                    }
                    using (System.Net.WebResponse resp = req.GetResponse())
                    {
                        if (resp == null)
                        {
                            strResponse = "";
                            resp.Close();
                            req.Abort();
                            return new Tuple<bool, string>(false, string.Empty);
                        }

                        using (System.IO.StreamReader sr = new System.IO.StreamReader(resp.GetResponseStream()))
                        {
                            strResponse = sr.ReadToEnd().Trim();
                            resp.Close();
                            req.Abort();
                            DeviceDiscovery.WriteToLogFile("Core is started to respond for http post.... ");
                            return new Tuple<bool, string>(true, string.Empty);
                        }
                    }
                }
            }
            catch (WebException e)
            {
                DeviceDiscovery.WriteToLogFile("####Core password during timeout exception at HTTP POST####");

                if (req != null)
                    req.Abort();

                //getCorepassword(strIP);
                if (e.Status == WebExceptionStatus.Timeout)
                {
                    DeviceDiscovery.WriteToLogFile("No Response from core Due to Timeout in HTTP Post " + e.ToString());
                    DeviceDiscovery.WriteToLogFile("Exception in " + e.TargetSite.Name + ". Message:" + e.Message);
#if DEBUG
                MessageBox.Show("Exception(Core is not responding for firmware upgrade ) in " + e.TargetSite.Name + ". Message:" + e.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                    //MessageBox.Show("Exception\n Core is not responding for firmware upgrade " + e.Message, "Error Code - ECxxxxF", MessageBoxButton.OK, MessageBoxImage.Error);


                    string coreResponse = string.Empty;
                    coreResponse = get_FirmwareLoadState(strIP);
                    if ((coreResponse == "fw_write") || (coreResponse == "complete") || (coreResponse == "idle") || (coreResponse == "fpga_write") || (coreResponse == "initializing") || (coreResponse == "Started"))
                    {
                        strResponse = "Started";
                        return new Tuple<bool, string>(true, string.Empty);
                    }
                    //else
                    //{
                    //    Thread.Sleep(120000);
                    //    if ((coreResponse == "fw_write") || (coreResponse == "complete") || (coreResponse == "idle") || (coreResponse == "fpga_write") || (coreResponse == "initializing") || (coreResponse == "Started"))
                    //    {
                    //        strResponse = "Started";
                    //        return true;
                    //    }
                    //}
                }
                else
                {
                    DeviceDiscovery.WriteToLogFile("No Response from core Due to WebException in HTTP Post" + e.ToString());
                }
                strResponse = "";
                return new Tuple<bool, string>(false, e.Message);
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("####Core password during timeout exception at HTTP POST####");
                if (req != null)
                    req.Abort();
                //getCorepassword(strIP);
                Debug.WriteLine("Error in Web_RW.HttpPost: no response from " + strURI.ToString() + "\n\r" + ex.ToString());
                DeviceDiscovery.WriteToLogFile("No Response from core for HTTP Post." + ex.ToString());
                strResponse = "";
                return new Tuple<bool, string>(false, ex.Message);
            }
        }

        //public void getCorepassword(string strIP)
        //{
        //    try
        //    {
        //        Telnet_Comm TPC = new Telnet_Comm();
        //        string[] strResponseArray = new string[0];
        //        string[] strMessageQue = new string[] { "", "root", "decline", "cat /usr/qsc/www/cgi-bin/httpd.conf" };
        //        TPC.session(strIP, strMessageQue, string.Empty, out strResponseArray);
        //        DeviceDiscovery.WriteToLogFile("Core's password is:" + strResponseArray[3]);
        //    }
        //    catch(Exception ex)
        //    {
        //        DeviceDiscovery.WriteToLogFile("Error occur during core password get" + ex.ToString());
        //    }
        //}

        public void SetBasicAuthHeader(ref HttpWebRequest req, string username, string password)
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
                Debug.WriteLine("Web_RW.SetBasicAuthHeader: HttpWebRequest parameter object is null!");
            }

            string strAuth = username + ":" + password;

            try
            {
                strAuth = Convert.ToBase64String(Encoding.Default.GetBytes(strAuth));
            }
            catch (Exception ex)
            {
                // Output to debug, but then do nothing.  ** LOG THIS LATER **
                Debug.WriteLine("Web_RW.SetBasicAuthHeader: " + ex.Message);
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif

            }

            req.Headers["Authorization"] = "Basic " + strAuth;
            DeviceDiscovery.WriteToLogFile("Username & password set successfully for HTTP Post ");
        }

        public bool HttpGet(string strURI, string m_password, string deviceIP,string token,string contentType, out string strResponse)
        {
            System.Net.HttpWebRequest req = (HttpWebRequest)System.Net.WebRequest.Create(strURI);
            try
            {
            
                req.ContentType = contentType;
                req.Method = "Get";
                req.Timeout = 15000;
                req.ReadWriteTimeout = 5000;

                // Need to setup Authentication Header.
                if(token!=string.Empty)
                {
                    req.Headers["Authorization"] = "Bearer " + token;
                    req.Accept = "application/json";
                }
                else
                  SetBasicAuthHeader(ref req, m_strUserName, m_password);

                using (System.Net.WebResponse resp = req.GetResponse())
                {
                    if (resp == null)
                    {
                        strResponse = "";
                        return false;
                    }

                    using (System.IO.StreamReader sr = new System.IO.StreamReader(resp.GetResponseStream()))
                    {
                        strResponse = sr.ReadToEnd().Trim();
                        sr.Close();
                        //resp.Close();
                        req.Abort();
                        DeviceDiscovery.WriteToLogFile("Core is started to respond for http Get.... ");
                        return true;
                    }                        
                }                   
            }          
            catch (Exception ex)
            {
                strResponse = "";
                if (ex.Message.Contains("The remote server returned an error: (401) Unauthorized"))
                {
                    strResponse = "401";
                    DeviceDiscovery.WriteToLogFile("Exception\n Invalid Username or Password" + ex.Message+ "Error Code - EC15024");
                }
                else if (ex.Message.Contains("The remote server returned an error: (404) Not Found"))
                {
                    strResponse = "404";
                }
                else
                {
                    if (ex.Message != "Thread was being aborted." && ex.Message != "Unable to connect to the remote server")
                        
                        DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif

                    if (ex.Message == "Unable to connect to the remote server")
                    {
                        DeviceDiscovery.WriteToLogFile("Exception\n " + coredeviceName + " Device is not available so firmware action is not continued");
                    }
                }
            
                DeviceDiscovery.WriteToLogFile("No Response from core for HTTP GET.... ");
                req.Abort();
                return false;
            }
        }

        public string get_FirmwareLoadState(string strIP)
        {
            string strReponse = string.Empty;
            try
            {
                XmlDocument xml = new XmlDocument();
                if(strIP != string.Empty & strIP != "Not Applicable")
                {
                    xml = DeviceDiscovery.XmlLoadUsingHttp("http://" + strIP + "/fw_status.xml", "get_FirmwareLoadState");
                    Thread.Sleep(5000);
                    if (xml == null)
                    {
                        DeviceDiscovery.WriteToLogFile("No response from XML file after firmware loaded");
                        return string.Empty;
                    }

                    XmlNode node = xml.SelectSingleNode("status/state");
                    strReponse = node.InnerText;
                    DeviceDiscovery.WriteToLogFile("Core's current status is:" + strReponse + "");
                }

                return strReponse;
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("The remote server returned an error: (401) Unauthorized"))
                {
                    DeviceDiscovery.WriteToLogFile("Invalid Username or Password");
                }
                else
                {
                    DeviceDiscovery.WriteToLogFile("Exception during Firmware status fetching");
                }
                return string.Empty;
            }
        }

		public string get_FirmwareLoadStateWithoutSleep(string strIP)
        {
            string strReponse = string.Empty;
            try
            {
                XmlDocument xml = new XmlDocument();
                if (strIP != string.Empty & strIP != "Not Applicable")
                {
                    xml = DeviceDiscovery.XmlLoadUsingHttp("http://" + strIP + "/fw_status.xml", "get_FirmwareLoadState");
                    //Thread.Sleep(5000);
                    if (xml == null)
                    {
                        DeviceDiscovery.WriteToLogFile("No response from XML file after firmware loaded");
                        return string.Empty;
                    }

                    XmlNode node = xml.SelectSingleNode("status/state");
                    strReponse = node.InnerText;
                    DeviceDiscovery.WriteToLogFile("Core's current status is:" + strReponse + "");
                }

                return strReponse;
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("The remote server returned an error: (401) Unauthorized"))
                {
                    DeviceDiscovery.WriteToLogFile("Invalid Username or Password");
                }
                else
                {
                    DeviceDiscovery.WriteToLogFile("Exception during Firmware status fetching");
                }
                return string.Empty;
            }
        }
		
        public string XmlReadToGetDesignversion(string strIP)
        {
            string currentVersion = string.Empty;
            try
            {
                XmlDocument xml = new XmlDocument();
                if(strIP != String.Empty & strIP != "Not Applicable")
                {
                    xml = DeviceDiscovery.XmlLoadUsingHttp("http://" + strIP + "//cgi-bin/status_xml", "XmlReadToGetDesignversion");
                    if(xml==null)
                    {
                        xml = DeviceDiscovery.XmlLoadUsingHttp("http://" + strIP + "/cgi-bin/status_xml", "XmlReadToGetDesignversion");
                    }

                    if (xml == null)
                        return string.Empty;

                    XmlNode node = xml.SelectSingleNode("status/firmware_version");
                    currentVersion = node.InnerText;
                    DeviceDiscovery.WriteToLogFile("Core's firmware version is" + currentVersion + "");
                }
                   
                return currentVersion;
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("The remote server returned an error: (401) Unauthorized"))
                {
                    DeviceDiscovery.WriteToLogFile("Invalid Username or Password");
                }
                else
                {
                    DeviceDiscovery.WriteToLogFile("Exception during Firmware Version fetching");
                }
                return string.Empty;
            }
        }
    }
}
