namespace QSC_Test_Automation.USBPlayBack.CoreAudioApi
{
    using System;

    /// <summary>
    /// Device State
    /// </summary>
    [Flags]
    public enum EDeviceState
    {
        /// <summary>
        /// DEVICE_STATE_ACTIVE
        /// </summary>
        Active = 0x00000001,
        /// <summary>
        /// DEVICE_STATE_DISABLED
        /// </summary>
        Disabled = 0x00000002,
        /// <summary>
        /// DEVICE_STATE_NOTPRESENT 
        /// </summary>
        NotPresent = 0x00000004,
        /// <summary>
        /// DEVICE_STATE_UNPLUGGED
        /// </summary>
        Unplugged = 0x00000008,
        /// <summary>
        /// DEVICE_STATEMASK_ALL
        /// </summary>
        All = 0x0000000F
    }

    public enum EDataFlow
    {
        eRender = 0,
        eCapture = 1,
        eAll = 2,
        EDataFlow_enum_count = 3
    }

    public enum ERole
    {
        eConsole = 0,
        eMultimedia = 1,
        eCommunications = 2,
        ERole_enum_count = 3
    }
}