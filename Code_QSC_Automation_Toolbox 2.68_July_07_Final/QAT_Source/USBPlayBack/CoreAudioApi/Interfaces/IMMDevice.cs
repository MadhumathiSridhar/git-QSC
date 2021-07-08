namespace QSC_Test_Automation.USBPlayBack.CoreAudioApi.Interfaces
{
    using System;
    using System.Runtime.InteropServices;

    [Guid("D666063F-1587-4E43-81F1-B948E807363F"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IMMDevice
    {
        [PreserveSig]
        int Activate(ref Guid iid, CLSCTX dwClsCtx, IntPtr pActivationParams,  [MarshalAs(UnmanagedType.IUnknown)] out object  ppInterface);
        [PreserveSig]
        int OpenPropertyStore(EStgmAccess stgmAccess, out IPropertyStore propertyStore);
        [PreserveSig]
        int GetId([MarshalAs(UnmanagedType.LPWStr)] out string ppstrId);
        [PreserveSig]
        int GetState(out EDeviceState pdwState);
    }


    [Guid("0BD7A1BE-7A1A-44DB-8397-CC5392387B5E"),
     InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IMMDeviceCollection
    {
        [PreserveSig]
        int GetCount(out uint pcDevices);
        [PreserveSig]
        int Item(uint nDevice, out IMMDevice Device);
    }

    
    [Guid("A95664D2-9614-4F35-A746-DE8DB63617E6"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IMMDeviceEnumerator
    {
        /// <summary>
        /// Generates a collection of audio endpoint devices that meet the specified criteria.
        /// </summary>
        /// <param name="dataFlow">The <see cref="EDataFlow"/> direction for the endpoint devices in the collection.</param>
        /// <param name="stateMask">One or more <see cref="EDeviceState"/> constants that indicate the state of the endpoints in the collection.</param>
        /// <param name="devices">The <see cref="IMMDeviceCollection"/> interface of the device-collection object.</param>
        /// <returns>An HRESULT code indicating whether the operation passed of failed.</returns>
        [PreserveSig]
        int EnumAudioEndpoints(
            [In] [MarshalAs(UnmanagedType.I4)] EDataFlow dataFlow,
            [In] [MarshalAs(UnmanagedType.U4)] EDeviceState stateMask,
            [Out] [MarshalAs(UnmanagedType.Interface)] out IMMDeviceCollection devices);

        /// <summary>
        /// Retrieves the default audio endpoint for the specified data-flow direction and role.
        /// </summary>
        /// <param name="dataFlow">The <see cref="EDataFlow"/> direction for the endpoint device.</param>
        /// <param name="role">The <see cref="ERole"/> of the endpoint device.</param>
        /// <param name="device">The <see cref="IMMDevice"/> interface of the default audio endpoint device.</param>
        /// <returns>An HRESULT code indicating whether the operation passed of failed.</returns>
        [PreserveSig]
        int GetDefaultAudioEndpoint(
            [In] [MarshalAs(UnmanagedType.I4)] EDataFlow dataFlow,
            [In] [MarshalAs(UnmanagedType.I4)] ERole role,
            [Out] [MarshalAs(UnmanagedType.Interface)] out IMMDevice device);

        /// <summary>
        /// Retrieves an endpoint device that is specified by an endpoint device-identification string.
        /// </summary>
        /// <param name="endpointId">The endpoint ID.</param>
        /// <param name="device">The <see cref="IMMDevice"/> interface for the specified device.</param>
        /// <returns>An HRESULT code indicating whether the operation passed of failed.</returns>
        [PreserveSig]
        int GetDevice(
            [In] [MarshalAs(UnmanagedType.LPWStr)] string endpointId,
            [Out] [MarshalAs(UnmanagedType.Interface)] out IMMDevice device);

    }

}
