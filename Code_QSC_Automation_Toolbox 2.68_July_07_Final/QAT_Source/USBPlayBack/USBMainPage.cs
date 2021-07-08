using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using QSC_Test_Automation.USBPlayBack.CoreAudioApi;
using System.IO;

namespace QSC_Test_Automation.USBPlayBack
{
    class USBMainPage
    {
        internal static readonly string AppDataRoot = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\AudioSwitch\\";
        internal static Settings settings;

        [XmlIgnore]
        private readonly string settingsxml = USBMainPage.AppDataRoot + "Settings.xml";

        public USBMainPage()
        {
            try
            {
                if (!Directory.Exists(AppDataRoot))
                    Directory.CreateDirectory(AppDataRoot);

                settings = Settings.Load();
            }
            catch(Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                ExecutionMessageBox("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
            }
        }

        //public MMDevice GetDefaultDevice(EDataFlow dataflow)
        //{
        //    MMDevice lst = null;

        //    if (dataflow != EDataFlow.eAll && dataflow != EDataFlow.EDataFlow_enum_count)
        //    {
        //        lst = EndPoints.GetDefaultMMDevice(dataflow);
        //    }

        //    return lst;
        //}

        //public List<MMDevice> GetAllDeviceList(EDataFlow dataflow)
        //{
        //    var lst = EndPoints.GetAllDeviceList(dataflow);

        //    return lst;
        //}
    }
}
