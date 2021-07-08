namespace QSC_Test_Automation.USBPlayBack.CoreAudioApi
{
    using System;
    using System.Runtime.InteropServices;
    using QSC_Test_Automation.USBPlayBack.CoreAudioApi.Interfaces;

    public static class PolicyKEY 
    {
        public static readonly PropertyKey PKEY_Device_FriendlyName = new PropertyKey(new Guid(0xa45c254e, 0xdf1c, 0x4efd, 0x80, 0x20, 0x67, 0xd1, 0x46, 0xa8, 0x50, 0xe0), 14);
        public static readonly PropertyKey PKEY_Device_DeviceDesc = new PropertyKey(new Guid(0xa45c254e, 0xdf1c, 0x4efd, 0x80, 0x20, 0x67, 0xd1, 0x46, 0xa8, 0x50, 0xe0), 2);
        public static readonly PropertyKey PKEY_DeviceClass_IconPath = new PropertyKey(new Guid(0x259abffc, 0x50a7, 0x47ce, 0xaf, 0x8, 0x68, 0xc9, 0xa7, 0xd7, 0x33, 0x66), 12);
        //public static readonly PropertyKey PKEY_DeviceClass_FriendlyName = new PropertyKey(new Guid(0x026e516e, unchecked((short)0xb814), 0x414b, 0x83, 0xcd, 0x85, 0x6d, 0x6f, 0xef, 0x48, 0x22), 2);
        public static readonly PropertyKey PKEY_DeviceClass_FriendlyName = new PropertyKey(new Guid("b3f8fa53-0004-438e-9003-51a46e139bfc"), 6);
    }

    [ComImport, Guid("870AF99C-171D-4F9E-AF0D-E63DF40C2BC9")]
    internal class _PolicyConfigClient
    {
    }

    public class PolicyConfigClient
    {
        private readonly IPolicyConfig _PolicyConfig;
        private readonly IPolicyConfigVista _PolicyConfigVista;
        private readonly IPolicyConfig10 _PolicyConfig10;

        public PolicyConfigClient()
        {
            try
            {
                _PolicyConfig = new _PolicyConfigClient() as IPolicyConfig;
                if (_PolicyConfig != null)
                    return;

                _PolicyConfigVista = new _PolicyConfigClient() as IPolicyConfigVista;
                if (_PolicyConfigVista != null)
                    return;

                _PolicyConfig10 = new _PolicyConfigClient() as IPolicyConfig10;
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                ExecutionMessageBox("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
            }
        }

        public void SetDefaultEndpoint(string devID, ERole eRole)
        {

            try
            {
                if (_PolicyConfig != null)
                {
                    Marshal.ThrowExceptionForHR(_PolicyConfig.SetDefaultEndpoint(devID, eRole));
                    return;
                }
                if (_PolicyConfigVista != null)
                {
                    Marshal.ThrowExceptionForHR(_PolicyConfigVista.SetDefaultEndpoint(devID, eRole));
                    return;
                }
                if (_PolicyConfig10 != null)
                {
                    Marshal.ThrowExceptionForHR(_PolicyConfig10.SetDefaultEndpoint(devID, eRole));
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
    }

    public struct PropertyKey
    {
        public Guid fmtid;
        public int pid;

        public PropertyKey(Guid guid, int Pid)
        {
            fmtid = guid;
            pid = Pid;
        }
    };

    /// <summary>
    /// Property Store class, only supports reading properties at the moment.
    /// </summary>
    public class PropertyStore
    {
        private readonly IPropertyStore _Store;

        private int Count
        {
            get
            {
                int Result;
                Marshal.ThrowExceptionForHR(_Store.GetCount(out Result));
                return Result;
            }
        }

        public bool Contains(PropertyKey compareKey)
        {
            try
            {
                for (var i = 0; i < Count; i++)
                {
                    var key = Get(i);
                    if (key.fmtid == compareKey.fmtid && key.pid == compareKey.pid)
                        return true;
                }

                return false;
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

        public PropertyStoreProperty this[PropertyKey queryKey]
        {
            get
            {
                for (var i = 0; i < Count; i++)
                {
                    var key = Get(i);
                    if (key.fmtid == queryKey.fmtid && key.pid == queryKey.pid)
                    {
                        PropVariant result;
                        Marshal.ThrowExceptionForHR(_Store.GetValue(ref key, out result));
                        return new PropertyStoreProperty(result);
                    }
                }
                return null;
            }
        }

        private PropertyKey Get(int index)
        {
            PropertyKey key;
            Marshal.ThrowExceptionForHR(_Store.GetAt(index, out key));
            return key;
        }

        internal PropertyStore(IPropertyStore store)
        {
            _Store = store;
        }
    }

    public class PropertyStoreProperty
    {
        internal PropVariant PropVariant;

        internal PropertyStoreProperty(PropVariant value)
        {
            PropVariant = value;
        }
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct PropVariant
    {
        [FieldOffset(0)]
        short vt;
        [FieldOffset(2)]
        short wReserved1;
        [FieldOffset(4)]
        short wReserved2;
        [FieldOffset(6)]
        short wReserved3;
        [FieldOffset(8)]
        sbyte cVal;
        [FieldOffset(8)]
        byte bVal;
        [FieldOffset(8)]
        short iVal;
        [FieldOffset(8)]
        ushort uiVal;
        [FieldOffset(8)]
        int lVal;
        [FieldOffset(8)]
        uint ulVal;
        [FieldOffset(8)]
        long hVal;
        [FieldOffset(8)]
        ulong uhVal;
        [FieldOffset(8)]
        float fltVal;
        [FieldOffset(8)]
        double dblVal;
        [FieldOffset(8)]
        Blob blobVal;
        [FieldOffset(8)]
        DateTime date;
        [FieldOffset(8)]
        bool boolVal;
        [FieldOffset(8)]
        int scode;
        [FieldOffset(8)]
        System.Runtime.InteropServices.ComTypes.FILETIME filetime;
        [FieldOffset(8)]
        IntPtr everything_else;

        //I'm sure there is a more efficient way to do this but this works ..for now..
        private byte[] GetBlob()
        {
            var Result = new byte[blobVal.Length];
            for (var i = 0; i < blobVal.Length; i++)
                Result[i] = Marshal.ReadByte((IntPtr)((long)(blobVal.Data) + i));
            return Result;
        }

        public object GetValue()
        {
            var ve = (VarEnum)vt;
            switch (ve)
            {
                case VarEnum.VT_I1:
                    return bVal;
                case VarEnum.VT_I2:
                    return iVal;
                case VarEnum.VT_I4:
                    return lVal;
                case VarEnum.VT_I8:
                    return hVal;
                case VarEnum.VT_INT:
                    return iVal;
                case VarEnum.VT_UI4:
                    return ulVal;
                case VarEnum.VT_LPWSTR:
                    return Marshal.PtrToStringUni(everything_else);
                case VarEnum.VT_BLOB:
                    return GetBlob();
            }
            return "FIXME Type = " + ve.ToString();
        }
    }
}
