namespace QSC_Test_Automation.USBPlayBack.CoreAudioApi
{
    using System;
    using System.Runtime.InteropServices;
    using QSC_Test_Automation.USBPlayBack.CoreAudioApi.Interfaces;

    internal class MMDevice
    {
        private readonly IMMDevice _RealDevice;
        private PropertyStore _PropertyStore;

        private PropertyStore GetPropertyInformation()
        {
            IPropertyStore propstore;
            Marshal.ThrowExceptionForHR(_RealDevice.OpenPropertyStore(EStgmAccess.STGM_READ, out propstore));
            return new PropertyStore(propstore);
        }

        public string FriendlyName
        {
            get
            {
                if (_PropertyStore == null)
                    _PropertyStore = GetPropertyInformation();

                var nameGuid = USBMainPage.settings.ShowHardwareName
                    ? PolicyKEY.PKEY_Device_FriendlyName
                    : PolicyKEY.PKEY_Device_DeviceDesc;

                if (_PropertyStore.Contains(nameGuid))
                    return (string)_PropertyStore[nameGuid].PropVariant.GetValue();
                return "Unknown";
            }
        }

        public string DeviceFriendlyName
        {
            get
            {
                if (_PropertyStore == null)
                    _PropertyStore = GetPropertyInformation();

                var nameGuid = USBMainPage.settings.ShowHardwareName
                    ? PolicyKEY.PKEY_DeviceClass_FriendlyName
                    : PolicyKEY.PKEY_Device_DeviceDesc;

                if (_PropertyStore.Contains(nameGuid))
                    return (string)_PropertyStore[nameGuid].PropVariant.GetValue();
                return "Unknown";
            }
        }

        public string ID
        {
            get
            {
                string Result;
                Marshal.ThrowExceptionForHR(_RealDevice.GetId(out Result));
                return Result;
            }
        }

        internal MMDevice(IMMDevice realDevice)
        {
            _RealDevice = realDevice;
        }
    }

    internal class MMDeviceCollection
    {
        private readonly IMMDeviceCollection _MMDeviceCollection;

        public int Count
        {
            get
            {
                uint result;
                Marshal.ThrowExceptionForHR(_MMDeviceCollection.GetCount(out result));
                return (int)result;
            }
        }

        public MMDevice this[int index]
        {
            get
            {
                IMMDevice result;
                _MMDeviceCollection.Item((uint)index, out result);
                return new MMDevice(result);
            }
        }

        internal MMDeviceCollection(IMMDeviceCollection parent)
        {
            _MMDeviceCollection = parent;
        }
    }

    [ComImport, Guid("BCDE0395-E52F-467C-8E3D-C4579291692E")]
    internal class _MMDeviceEnumerator
    {
    }

    internal class MMDeviceEnumerator
    {
        private readonly IMMDeviceEnumerator _realEnumerator = new _MMDeviceEnumerator() as IMMDeviceEnumerator;

        public MMDeviceCollection EnumerateAudioEndPoints(EDataFlow dataFlow, EDeviceState dwStateMask)
        {
            IMMDeviceCollection result;
            Marshal.ThrowExceptionForHR(_realEnumerator.EnumAudioEndpoints(dataFlow, dwStateMask, out result));
            return new MMDeviceCollection(result);
        }

        public MMDevice GetDefaultAudioEndpoint(EDataFlow dataFlow, ERole role)
        {
            IMMDevice _Device;
            Marshal.ThrowExceptionForHR(_realEnumerator.GetDefaultAudioEndpoint(dataFlow, role, out _Device));
            return new MMDevice(_Device);
        }

        public MMDevice GetDevice(string deviceId)
        {
            IMMDevice deviceFromId;
            Marshal.ThrowExceptionForHR(_realEnumerator.GetDevice(deviceId, out deviceFromId));
            return new MMDevice(deviceFromId);
        }
    }
}
