using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CecSharp;
using System.Globalization;
using System.IO;
using System.Threading;

namespace QSC_Test_Automation
{
    class CecSharpClient : CecCallbackMethods
    {
        bool isActionStartEnded = false;
        int executionID = 0;
        bool isValidateDestination = false;

        public CecSharpClient()
        {
            try
            {
                Config = new LibCECConfiguration();
                Config.DeviceTypes.Types[0] = CecDeviceType.RecordingDevice;
                Config.DeviceName = "CEC Tester";
                Config.ClientVersion = LibCECConfiguration.CurrentVersion;
                Config.SetCallbacks(this);
                LogLevel = (int)CecLogLevel.All;

                Lib = new LibCecSharp(Config);

                //Config.BaseDevice = CecLogicalAddress.PlaybackDevice1;

                //Lib.SetConfiguration(Config);

                Lib.InitVideoStandalone();

                Console.WriteLine("CEC Parser created - libCEC version " + Lib.VersionToString(Config.ServerVersion));
            }
            catch(Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                ExecutionMessageBox("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
            }
        }

        string osdOpcode = string.Empty;
        public override int ReceiveCommand(CecCommand command)
        {
            try
            {
                if (isActionStartEnded)
                {
                    using (StreamWriter write = new StreamWriter(@"C:\Users\" + Environment.UserName + @"\AppData\Local\Temp\QSys Temp Files\Temp_" + executionID + @"\CECLog.txt", true))
                    {
                        string totalOpcode = string.Empty;

                        int firstbyteinInt = (int)command.Initiator;
                        totalOpcode += firstbyteinInt.ToString("X");

                        int secondbyteinInt = (int)command.Destination;
                        totalOpcode += secondbyteinInt.ToString("X");

                        int opcodeinInt = (int)command.Opcode;
                        byte[] b3 = new byte[1];

                        b3[0] = (byte)opcodeinInt;
                        totalOpcode += ":" + BitConverter.ToString(b3);

                        for (int i=0; i < command.Parameters.Size; i++)
                        {
                            int parameterinInt = (int)command.Parameters.Data[i];
                            byte[] b4 = new byte[1];

                            b4[0] = (byte)parameterinInt;
                            totalOpcode += ":" + BitConverter.ToString(b4);
                        }

                        write.WriteLine(DateTime.Now + " *CEC* " + totalOpcode);
                    }
                }

                if (isValidateDestination && command.Opcode == CecOpcode.SetOsdName && command.Destination == CecLogicalAddress.RecordingDevice1)
                {
                    string totalOpcode = string.Empty;

                    for (int i = 0; i < command.Parameters.Size; i++)
                    {
                        int parameterinInt = (int)command.Parameters.Data[i];
                        byte[] byt = new byte[1];

                        byt[0] = (byte)parameterinInt;
                        totalOpcode += BitConverter.ToString(byt);
                    }

                    osdOpcode = totalOpcode;
                }
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                ExecutionMessageBox("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
            }

            return 0;
        }

        public override int ReceiveLogMessage(CecLogMessage message)
        {
//            try
//            {
//                if (isActionStartEnded)
//                {
//                    using (StreamWriter write = new StreamWriter(@"C:\Users\" + Environment.UserName + @"\AppData\Local\Temp\QSys Temp Files\Command1.txt", true))
//                    {
//                        write.WriteLine(DateTime.Now + " *CEC* " + message.Message);
//                    }
//                }
//            }
//            catch(Exception ex)
//            {
//                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
//#if DEBUG
//                ExecutionMessageBox("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
//#endif
//            }

            return 0;
        }

        public bool Connect(int timeout)
        {
            try
            {
                CecAdapter[] adapters = Lib.FindAdapters(string.Empty);
                if (adapters.Length > 0)
                    return Connect(adapters[0].ComPort, timeout);
                else
                {
                    Console.WriteLine("Did not find any CEC adapters");
                    return false;
                }
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                ExecutionMessageBox("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                return false;
            }
        }

        public bool Connect(string port, int timeout)
        {
            try
            {
                return Lib.Open(port, timeout);
            }
            catch(Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                ExecutionMessageBox("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                return false;
            }
        }

        public void Close()
        {
            try
            {
                Lib.Close();
            }
            catch(Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                ExecutionMessageBox("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
            }
        }

        public CecLogicalAddress GetInitialDevice()
        {
            try
            {
                CecLogicalAddresses addresses = Lib.GetActiveDevices();
                for (int iPtr = 0; iPtr < addresses.Addresses.Length; iPtr++)
                {
                    CecLogicalAddress address = (CecLogicalAddress)iPtr;
                    if (!addresses.IsSet(address))
                        continue;

                    CecVendorId iVendorId = Lib.GetDeviceVendorId(address);

                    if (iVendorId == CecVendorId.PulseEight)
                    {
                        return address;
                    }
                }
            }
            catch(Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                ExecutionMessageBox("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
            }

            return CecLogicalAddress.RecordingDevice1;
        }

        public CecLogicalAddress GetDesinationDevice()
        {
            try
            {
                osdOpcode = string.Empty;

                CecLogicalAddresses addresses = Lib.GetActiveDevices();
                for (int iPtr = 0; iPtr < addresses.Addresses.Length; iPtr++)
                {
                    CecLogicalAddress address = (CecLogicalAddress)iPtr;
                    if (!addresses.IsSet(address) || (address != CecLogicalAddress.PlaybackDevice1 && address != CecLogicalAddress.PlaybackDevice2 && address != CecLogicalAddress.PlaybackDevice3))
                        continue;

                    int selectDevice = (int)address;

                    string opcode = "1" + selectDevice.ToString() + ":46";

                    string[] splitOpcode = opcode.Replace(" ", string.Empty).Split(':');
                    CecCommand bytes = new CecCommand();
                    for (int iPtr1 = 0; iPtr1 < splitOpcode.Length; iPtr1++)
                    {
                        bytes.PushBack(byte.Parse(splitOpcode[iPtr1], System.Globalization.NumberStyles.HexNumber));
                    }

                    isValidateDestination = true;
                    Lib.Transmit(bytes);
                    Thread.Sleep(5000);
                    isValidateDestination = false;

                    string osd_Opcode = osdOpcode;
                    string sb = string.Empty;
                    for (var i = 0; i < osd_Opcode.Length; i += 2)
                    {
                        var hexChar = osd_Opcode.Substring(i, 2);
                        sb += ((char)Convert.ToByte(hexChar, 16));
                    }

                    if (sb.Contains("VST"))
                    {
                        return address;
                    }
                }

                return CecLogicalAddress.PlaybackDevice1;
            }
            catch(Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                ExecutionMessageBox("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                return CecLogicalAddress.PlaybackDevice1;
            }
        }

        public bool CECCommandsExecution(string command, string device, string opcode, CecLogicalAddress addr)
        {            
            bool isSuccess = false;

            try
            {
                if (command != "Others")
                {
                    string selecDevice = string.Empty;
                    if (device == "Streamer")
                    {
                        int val = (int)addr; 
                        selecDevice = val.ToString();
                    }
                    else
                        selecDevice = "F";


                    if (command == "Standby")
                    {
                        //////14:36                                    

                        opcode = "1" + selecDevice + ":36";
                    }
                    //else if (command == "Poweron")
                    //{
                    //    ////////////14:44:40  - user control pressed
                    //    ///////////14:45 - User control released

                    //    var powerstatus = Lib.GetDevicePowerStatus(device);

                    //    if (powerstatus != CecPowerStatus.On)
                    //    {
                    //        string opcode1 = "1" + selecDevice + ":44:40";
                    //        string[] splitOpcode1 = opcode1.Replace(" ", string.Empty).Split(':');
                    //        CecCommand bytess = new CecCommand();
                    //        for (int iPtr = 0; iPtr < splitOpcode1.Length; iPtr++)
                    //        {
                    //            bytess.PushBack(byte.Parse(splitOpcode1[iPtr], System.Globalization.NumberStyles.HexNumber));
                    //        }

                    //        isSuccess.Add(Lib.Transmit(bytess));

                    //        string opcode2 = "1" + selecDevice + ":45";

                    //        string[] splitOpcode2 = opcode2.Replace(" ", string.Empty).Split(':');
                    //        CecCommand bytes2 = new CecCommand();
                    //        for (int iPtr = 0; iPtr < splitOpcode2.Length; iPtr++)
                    //        {
                    //            bytes2.PushBack(byte.Parse(splitOpcode2[iPtr], System.Globalization.NumberStyles.HexNumber));
                    //        }

                    //        isSuccess.Add(Lib.Transmit(bytes2));
                    //    }
                    //    else
                    //    {
                    //        opcode = null;
                    //    }
                    //}
                    //else if(command == "Image View On")
                    //{
                    //    opcode = "1" + selecDevice + ":04";

                    //}
                    //else if(command == "Active Source")
                    //{
                    //    opcode = "1" + selecDevice + ":82:10:00";

                    //}
                    else if (command == "Give Physical Address")
                    {
                        ////////14:83

                        opcode = "1" + selecDevice + ":83";
                    }
                    else if (command == "Give OSD Name")
                    {
                        //////14:46

                        opcode = "1" + selecDevice + ":46";
                    }
                    else if (command == "Get CEC Version")
                    {
                        //////14:9F

                        opcode = "1" + selecDevice + ":9F";
                    }
                    else if (command == "Give Device Power Status")
                    {
                        //////14:8F

                        opcode = "1" + selecDevice + ":8F";
                    }
                    else if (command == "Give Deck Status")
                    {
                        //////14:1A:01

                        opcode = "1" + selecDevice + ":1A:01";
                    }
                }

                if (opcode != null && opcode != string.Empty)
                {
                    string[] splitOpcode = opcode.Replace(" ", string.Empty).Split(':');
                    CecCommand bytes = new CecCommand();
                    for (int iPtr = 0; iPtr < splitOpcode.Length; iPtr++)
                    {
                        bytes.PushBack(byte.Parse(splitOpcode[iPtr], System.Globalization.NumberStyles.HexNumber));
                    }

                    isSuccess = Lib.Transmit(bytes);
                }
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                ExecutionMessageBox("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
            }

            return isSuccess;
        }
        

        public void SetLogEnable(bool isStart, int Exid)
        {
            try
            {
                executionID = Exid;
                isActionStartEnded = isStart;
            }
            catch(Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                ExecutionMessageBox("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
            }
        }

        public bool AdaptorConnectionCheck()
        {
            try
            {
                return Lib.PingAdapter();
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                ExecutionMessageBox("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                return false;
            }
        }

        private int LogLevel;
        private LibCecSharp Lib;
        private LibCECConfiguration Config;
    }
}
