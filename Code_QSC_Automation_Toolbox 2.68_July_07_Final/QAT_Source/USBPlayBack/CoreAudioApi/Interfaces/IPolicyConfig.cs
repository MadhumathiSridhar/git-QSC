namespace QSC_Test_Automation.USBPlayBack.CoreAudioApi.Interfaces
{
    using System;
    using System.Runtime.InteropServices;

    [Guid("F8679F50-850A-41CF-9C72-430F290290C8"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IPolicyConfig
    {
        [PreserveSig]
        int GetMixFormat(string pszDeviceName, IntPtr ppFormat);

        [PreserveSig]
        int GetDeviceFormat(string pszDeviceName, bool bDefault, IntPtr ppFormat);

        [PreserveSig]
        int ResetDeviceFormat(string pszDeviceName);

        [PreserveSig]
        int SetDeviceFormat(string pszDeviceName, IntPtr pEndpointFormat, IntPtr MixFormat);

        [PreserveSig]
        int GetProcessingPeriod(string pszDeviceName, bool bDefault, IntPtr pmftDefaultPeriod, IntPtr pmftMinimumPeriod);

        [PreserveSig]
        int SetProcessingPeriod(string pszDeviceName, IntPtr pmftPeriod);

        [PreserveSig]
        int GetShareMode(string pszDeviceName, IntPtr pMode);

        [PreserveSig]
        int SetShareMode(string pszDeviceName, IntPtr mode);

        [PreserveSig]
        int GetPropertyValue(string pszDeviceName, bool bFxStore, IntPtr key, IntPtr pv);

        [PreserveSig]
        int SetPropertyValue(string pszDeviceName, bool bFxStore, IntPtr key, IntPtr pv);

        [PreserveSig]
        int SetDefaultEndpoint(string pszDeviceName, ERole role);

        [PreserveSig]
        int SetEndpointVisibility(string pszDeviceName, bool bVisible);
    }

    [Guid("00000000-0000-0000-C000-000000000046"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IPolicyConfig10
    {
        [PreserveSig]
        int GetMixFormat(string pszDeviceName, IntPtr ppFormat);

        [PreserveSig]
        int GetDeviceFormat(string pszDeviceName, bool bDefault, IntPtr ppFormat);

        [PreserveSig]
        int ResetDeviceFormat(string pszDeviceName);

        [PreserveSig]
        int SetDeviceFormat(string pszDeviceName, IntPtr pEndpointFormat, IntPtr MixFormat);

        [PreserveSig]
        int GetProcessingPeriod(string pszDeviceName, bool bDefault, IntPtr pmftDefaultPeriod, IntPtr pmftMinimumPeriod);

        [PreserveSig]
        int SetProcessingPeriod(string pszDeviceName, IntPtr pmftPeriod);

        [PreserveSig]
        int GetShareMode(string pszDeviceName, IntPtr pMode);

        [PreserveSig]
        int SetShareMode(string pszDeviceName, IntPtr mode);

        [PreserveSig]
        int GetPropertyValue(string pszDeviceName, bool bFxStore, IntPtr key, IntPtr pv);

        [PreserveSig]
        int SetPropertyValue(string pszDeviceName, bool bFxStore, IntPtr key, IntPtr pv);

        [PreserveSig]
        int SetDefaultEndpoint(string pszDeviceName, ERole role);

        [PreserveSig]
        int SetEndpointVisibility(string pszDeviceName, bool bVisible);
    }

    [Guid("568B9108-44BF-40B4-9006-86AFE5B5A620"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IPolicyConfigVista
    {
        [PreserveSig]
        int GetMixFormat(string pszDeviceName, IntPtr ppFormat);

        [PreserveSig]
        int GetDeviceFormat(string pszDeviceName, bool bDefault, IntPtr ppFormat);

        [PreserveSig]
        int ResetDeviceFormat(string pszDeviceName);

        [PreserveSig]
        int SetDeviceFormat(string pszDeviceName, IntPtr pEndpointFormat, IntPtr MixFormat);

        [PreserveSig]
        int GetProcessingPeriod(string pszDeviceName, bool bDefault, IntPtr pmftDefaultPeriod, IntPtr pmftMinimumPeriod);

        [PreserveSig]
        int SetProcessingPeriod(string pszDeviceName, IntPtr pmftPeriod);

        [PreserveSig]
        int GetShareMode(string pszDeviceName, IntPtr pMode);

        [PreserveSig]
        int SetShareMode(string pszDeviceName, IntPtr mode);

        [PreserveSig]
        int GetPropertyValue(string pszDeviceName, bool bFxStore, IntPtr key, IntPtr pv);

        [PreserveSig]
        int SetPropertyValue(string pszDeviceName, bool bFxStore, IntPtr key, IntPtr pv);

        [PreserveSig]
        int SetDefaultEndpoint(string pszDeviceName, ERole role);

        [PreserveSig]
        int SetEndpointVisibility(string pszDeviceName, bool bVisible);
    }

    [Guid("886d8eeb-8cf2-4446-8d02-cdba1dbdcf99"),
     InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IPropertyStore
    {
        [PreserveSig]
        int GetCount(out Int32 count);
        [PreserveSig]
        int GetAt(int iProp, out PropertyKey pkey);
        [PreserveSig]
        int GetValue(ref PropertyKey key, out PropVariant pv);
        [PreserveSig]
        int SetValue(ref PropertyKey key, ref PropVariant propvar);
        [PreserveSig]
        int Commit();
    };

    
	[StructLayout(LayoutKind.Sequential)]
    public struct PROPERTYKEY
    {
        /// <summary>
        /// A unique GUID for the property.
        /// </summary>
        public Guid fmtid;

        /// <summary>
        /// A property identifier (PID).
        /// </summary>
        public int pid;
    }
}
