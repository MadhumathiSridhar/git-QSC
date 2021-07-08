namespace QSC_Test_Automation.USBPlayBack
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.IO;
    using System.Xml.Serialization;
    using QSC_Test_Automation.USBPlayBack.CoreAudioApi;

    [XmlRoot, Serializable]
    public class Settings
    {
        public static Settings newSettings()
        {
            return new Settings
            {
                DefaultDataFlow = EDataFlow.eRender,
                ShowHardwareName = true,
                QuickSwitchShowOSD = true,
            };
        }

        [XmlIgnore]
        private static readonly string settingsxml = USBMainPage.AppDataRoot + "Settings.xml";

        [XmlElement]
        public EDataFlow DefaultDataFlow;

        [XmlElement]
        public bool DefaultMultimediaAndComm;

        [XmlElement]
        public bool ShowHardwareName;

        [XmlElement]
        public bool QuickSwitchShowOSD;
                
        [XmlElement]
        public List<CDevice> Device;
 
        public class CDevice
        {
            [XmlAttribute]
            public string DeviceID;

            [XmlAttribute]
            public bool HideFromList;
        }

        internal void Save()
        {
            try
            {
                var xs = new XmlSerializer(typeof(Settings));
                using (var tw = new StreamWriter(settingsxml))
                    xs.Serialize(tw, this);
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                ExecutionMessageBox("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
            }
        }

        internal static Settings Load()
        {
            try
            {
                var xs = new XmlSerializer(typeof(Settings));
                using (var fileStream = new StreamReader(settingsxml))
                    return (Settings)xs.Deserialize(fileStream);
            }
            catch
            {
                var newsettings = newSettings();
                newsettings.Save();
                return newsettings;
            }
        }
    }
}
