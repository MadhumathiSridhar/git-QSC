namespace QSC_Test_Automation.USBPlayBack
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Linq;
    using QSC_Test_Automation.USBPlayBack.CoreAudioApi;

    internal static class EndPoints
    {
        private static int DefaultDeviceID;
        internal static string DefaultDeviceName;

        internal static readonly MMDeviceEnumerator DeviceEnumerator;
        private static readonly Dictionary<int, string> DeviceIDs = new Dictionary<int, string>();
        public static readonly Dictionary<string, string> DeviceNames = new Dictionary<string, string>(); 
        private static readonly PolicyConfigClient pPolicyConfig = new PolicyConfigClient();

        static EndPoints()
        {
            try
            {
                DeviceEnumerator = new MMDeviceEnumerator();
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                ExecutionMessageBox("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
            }
        }

        internal static void RefreshDeviceList(EDataFlow renderType)
        {
            try
            {
                DeviceNames.Clear();
                DeviceIDs.Clear();

                var pDevices = DeviceEnumerator.EnumerateAudioEndPoints(renderType, EDeviceState.Active);
                var defDeviceID = DeviceEnumerator.GetDefaultAudioEndpoint(renderType, ERole.eMultimedia).ID;
                var devCount = pDevices.Count;
                var newCount = 0;

                for (var i = 0; i < devCount; i++)
                {
                    var device = pDevices[i];
                    var devID = device.ID;

                    var devSettings = USBMainPage.settings.Device.Find(x => x.DeviceID == devID);
                    if (devSettings == null || !devSettings.HideFromList)
                    {
                        var devName = device.FriendlyName;
                        DeviceNames.Add(devID, devName);
                        DeviceIDs.Add(newCount, devID);

                        if (devID == defDeviceID)
                        {
                            DefaultDeviceID = newCount;
                            DefaultDeviceName = devName;
                        }
                        newCount++;
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
        }

//        internal static string SetPrevDefault(EDataFlow rType, ERole erole)
//        {
//            try
//            {
//                RefreshDeviceList(rType);

//                if (DefaultDeviceID == 0)
//                    DefaultDeviceID = DeviceNames.Count - 1;
//                else
//                    DefaultDeviceID--;

//                SetDefaultDeviceByID(DefaultDeviceID, erole);
//            }
//            catch (Exception ex)
//            {
//                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
//#if DEBUG
//                ExecutionMessageBox("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
//#endif
//            }

//            return DeviceNames[DeviceIDs[DefaultDeviceID]];
//        }

//        internal static string SetNextDefault(EDataFlow rType, ERole erole)
//        {
//            try
//            { 
//            RefreshDeviceList(rType);

//            if (DefaultDeviceID == DeviceNames.Count - 1)
//                DefaultDeviceID = 0;
//            else
//                DefaultDeviceID++;

//            SetDefaultDeviceByID(DefaultDeviceID, erole);

//            }
//            catch (Exception ex)
//            {
//                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
//#if DEBUG
//                ExecutionMessageBox("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
//#endif
//            }

//            return DeviceNames[DeviceIDs[DefaultDeviceID]];
//        }

        internal static MMDevice GetDefaultMMDevice(EDataFlow renderType, ERole eRole)
        {
            return DeviceEnumerator.GetDefaultAudioEndpoint(renderType, eRole);
        }

        internal static List<MMDevice> GetAllDeviceList(EDataFlow dataFlow)
        {
            var devices = new List<MMDevice>();

            try
            {
                var pDevices = DeviceEnumerator.EnumerateAudioEndPoints(dataFlow, EDeviceState.Active);
                var devCount = pDevices.Count;

                for (var i = 0; i < devCount; i++)
                {
                    var device = pDevices[i];
                    devices.Add(device);
                }
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                ExecutionMessageBox("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
            }
            return devices;
        }

        internal static void SetDefaultDeviceByID(int devID, ERole erole)
        {
            try
            {
                SetDefaultDevice(DeviceIDs[devID], erole);
                DefaultDeviceID = devID;
                DefaultDeviceName = DeviceNames[DeviceIDs[devID]];
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                ExecutionMessageBox("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
            }
        }

        internal static bool SetDefaultDeviceByName(string devName, ERole erole)
        {
            try
            {
                foreach (var device in DeviceNames.Where(device => device.Value == devName))
                {
                    SetDefaultDevice(device.Key, erole);
                    DefaultDeviceName = device.Value;
                    return true;
                }

                return false;
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

        internal static void SetDefaultDevice(string devID, ERole erole)
        {
            try
            {
                pPolicyConfig.SetDefaultEndpoint(devID, erole);
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                ExecutionMessageBox("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
            }
        }

        //internal static List<string> GetAllDeviceFridlyNameList(EDataFlow dataFlow)
        //{
        //    var devices = new List<string>();
        //    var pDevices = DeviceEnumerator.EnumerateAudioEndPoints(dataFlow, EDeviceState.Active);
        //    var devCount = pDevices.Count;

        //    for (var i = 0; i < devCount; i++)
        //    {
        //        var device = pDevices[i];
        //        devices.Add(device.DeviceFriendlyName);
        //    }

        //    return devices;
        //}
    }
}
