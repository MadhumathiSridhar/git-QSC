using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Collections.ObjectModel;
using System.Linq;
using System.Diagnostics;

namespace QSC_Test_Automation
{
    public class DUT_DeviceItem : INotifyPropertyChanged
    {

        private String itemDeviceTypeValue = null;
        private String itemDeviceModelValue = null;
        private String itemDeviceNameValue = null;
        private String itemPrimaryorBackupValue = null;
        private String itemlinkedValue = null;        
        private string[] QREMCorevalue = null;
        private List<string> itemDeviceNameValue1 = new List<string>();
        private Dictionary<string,string> itemNetPairingListValue = new Dictionary<string, string>();
        private Dictionary<string, string> itemNetPairingListValue_duplicate = new Dictionary<string, string>();
        private ObservableCollection<string> itemNetPairingListForXAML = new ObservableCollection<string>();
        //private Dictionary<string, bool> itemNetPairingListQREMValue = new Dictionary<string, bool>();
        private String itemNetPairingSelectedValue = null;
        private String itemCurrentBuildValue = null;
        private String itemCurrentDesignValue = null;
        private List<String> itemPrimaryIPListValue = new List<string>();
        private String itemPrimaryIPSelectedValue = null;
        private List<String> itemSecondaryIPListValue = new List<string>();
        private String itemSecondaryIPSelectedValue = null;
        private bool CoreRestoreDesignValue = false;
        private Visibility CoreRestoreDesignVisibilityValue = Visibility.Collapsed;
        private Visibility ClearLogVisibilityValue = Visibility.Collapsed;
        private bool ClearLogsValue = false;
        private bool designerActionPresent = false;
        private bool BypassValue = false;
        private string QREMTokenVal = string.Empty;

        public event PropertyChangedEventHandler PropertyChanged;
        void OnPropertyChanged(string property)
        {
            try
            {
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs(property));
                    if (ParentTestCaseItem != null && ParentTestCaseItem.SaveButtonIsEnabled != true)
                        ParentTestCaseItem.SaveButtonIsEnabled = true;
                }
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                //MessageBox.Show("Exception\n " + ex.Message, "Error Code - EC20001", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public DUT_DeviceItem()
        {
            CoreDateTimeSettings();
        }

        public DUT_DeviceItem(DUT_DeviceItem sourceItem)
        {
            try
            {
                ItemDeviceType = sourceItem.ItemDeviceType;
                ItemDeviceModel = sourceItem.ItemDeviceModel;
                ItemDeviceName = sourceItem.ItemDeviceName;
                ParentTestCaseItem = sourceItem.ParentTestCaseItem;
                CoreRestoreDesign = sourceItem.CoreRestoreDesign;
                CoreRestoreDesignVisibility = sourceItem.CoreRestoreDesignVisibility;
                
                iLogIsChecked = sourceItem.iLogIsChecked;
                ConfiguratorLogIsChecked = sourceItem.ConfiguratorLogIsChecked;

                if (!ItemNetPairingList.Keys.ToList().Contains("Not Applicable"))
                {
                    ItemNetPairingList.Add("Not Applicable", "Localdevice");
                    ItemNetPairingList_duplicate.Add("Not Applicable", "Localdevice");
                    ItemNetPairingListForXAML.Add("Not Applicable");
                }
                if (!ItemNetPairingList.Keys.ToList().Contains("None"))
                {
                    ItemNetPairingList.Add("None", "Localdevice");
                    ItemNetPairingList_duplicate.Add("None", "Localdevice");
                    ItemNetPairingListForXAML.Add("None");
                }
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                //MessageBox.Show("Exception\n " + ex.Message, "QAT Error Code - EC03021", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public String ItemDeviceType
        {
            get { return itemDeviceTypeValue; }
            set
            {
                itemDeviceTypeValue = value;

                if (itemDeviceTypeValue == "Core" || itemDeviceTypeValue == "core")
                {
                    CoreRestoreDesignVisibilityValue = Visibility.Visible;
                    CoreDateTimeVisibility = Visibility.Visible;
                }
                else
                {
                    CoreRestoreDesignVisibilityValue = Visibility.Hidden;
                    CoreDateTimeVisibility = Visibility.Hidden;
                }
                if (itemDeviceTypeValue == "Camera" || itemDeviceTypeValue == "Camera")
                {
                    ClearLogVisibilityValue = Visibility.Hidden;
                    IDButtonVisibility = Visibility.Hidden;
                }
                else
                {
                    ClearLogVisibilityValue = Visibility.Visible;
                    IDButtonVisibility = Visibility.Visible;
                }
                //IDButtonVisibility

                OnPropertyChanged("ItemDeviceType");
            }
        }

        private string VideoGenValue = string.Empty;
        public string VideoGen
        {
            get { return VideoGenValue; }
            set
            {
                VideoGenValue = value;
                OnPropertyChanged("VideoGen");
            }
        }
        private string GenModelValue = string.Empty;
        public string GenModel
        {
            get { return GenModelValue; }
            set
            {
                GenModelValue = value;
                OnPropertyChanged("GenModel");
            }
        }

        private string Gen_IP_addressValue = string.Empty;
        public string Gen_IP_address
        {
            get { return Gen_IP_addressValue; }
            set
            {
                Gen_IP_addressValue = value;
                OnPropertyChanged("Gen_IP_address");
            }
        }



      


        public String ItemDeviceModel
        {
            get { return itemDeviceModelValue; }
            set { itemDeviceModelValue = value; OnPropertyChanged("ItemDeviceModel"); }

        }
        public String ItemDeviceName
        {
            get { return itemDeviceNameValue; }
            set { itemDeviceNameValue = value; OnPropertyChanged("ItemDeviceName"); }
        }

        public String ItemPrimaryorBackup
        {
            get { return itemPrimaryorBackupValue; }
            set { itemPrimaryorBackupValue = value; OnPropertyChanged("ItemPrimaryorBackup"); }
        }

        public String Itemlinked
        {
            get { return itemlinkedValue; }
            set { itemlinkedValue = value; OnPropertyChanged("Itemlinked"); }
        }

        public Dictionary<string, string> ItemNetPairingList
        {
            get { return itemNetPairingListValue; }
            set
            {
                itemNetPairingListValue = value; OnPropertyChanged("ItemNetPairingList");
            }
        }

        public ObservableCollection<string> ItemNetPairingListForXAML
        {
            get { return itemNetPairingListForXAML; }
            set
            {
                itemNetPairingListForXAML = value; OnPropertyChanged("ItemNetPairingListForXAML");
            }
        }

        public Dictionary<string, string> ItemNetPairingList_duplicate
        {
            get { return itemNetPairingListValue_duplicate; }
            set
            {
                itemNetPairingListValue_duplicate = value; OnPropertyChanged("ItemNetPairingList_duplicate");
            }
        }

        public string[] QREMcoredetails
        {
            get { return QREMCorevalue; }
            set { QREMCorevalue = value; OnPropertyChanged("QREMcoredetails"); }
        }

        public String ItemNetPairingSelected
        {
            get { return itemNetPairingSelectedValue; }
            set
            {
                itemNetPairingSelectedValue = value;
                if (value == null)
                { blnIDColor = "black"; }
                OnPropertyChanged("ItemNetPairingSelected");

            }
        }
        public String ItemCurrentBuild
        {
            get { return itemCurrentBuildValue; }
            set { itemCurrentBuildValue = value; OnPropertyChanged("ItemCurrentBuild"); }
        }
        public String ItemCurrentDesign
        {
            get { return itemCurrentDesignValue; }
            set { itemCurrentDesignValue = value; OnPropertyChanged("ItemCurrentDesign"); }
        }
        public List<String> ItemPrimaryIPList
        {
            get { return itemPrimaryIPListValue; }
            set { itemPrimaryIPListValue = value; OnPropertyChanged("ItemPrimaryIPList"); }
        }
        public String ItemPrimaryIPSelected
        {
            get { return itemPrimaryIPSelectedValue; }
            set { itemPrimaryIPSelectedValue = value; OnPropertyChanged("ItemPrimaryIPSelected"); }
        }
        public List<String> ItemSecondaryIPList
        {
            get { return itemSecondaryIPListValue; }
            set { itemSecondaryIPListValue = value; OnPropertyChanged("ItemSecondaryIPList"); }
        }
        public String ItemSecondaryIPSelected
        {
            get { return itemSecondaryIPSelectedValue; }
            set { itemSecondaryIPSelectedValue = value; OnPropertyChanged("ItemSecondaryIPSelected"); }
        }

        private TestCaseItem parentTestCaseItemValue = null;
        public TestCaseItem ParentTestCaseItem
        {
            get { return parentTestCaseItemValue; }
            set { parentTestCaseItemValue = value; OnPropertyChanged("ParentTestCaseItem"); }
        }

        ////private bool telnetIsCheckedValue = false;
        ////public bool TelnetIsChecked
        ////{
        ////    get { return telnetIsCheckedValue; }
        ////    set { telnetIsCheckedValue = value; OnPropertyChanged("TelnetIsChecked"); }
        ////}

        private bool iLogIsCheckedValue = false;
        public bool iLogIsChecked
        {
            get { return iLogIsCheckedValue; }
            set
            {
                iLogIsCheckedValue = value;
                OnPropertyChanged("iLogIsChecked");
            }
        }

        private bool configuratorLogIsCheckedValue = false;
        public bool ConfiguratorLogIsChecked
        {
            get { return configuratorLogIsCheckedValue; }
            set
            {
                configuratorLogIsCheckedValue = value;
                OnPropertyChanged("ConfiguratorLogIsChecked");
            }
        }

        private string blnIDColorValue = string.Empty;
        public string blnIDColor
        {
            get
            {
                return blnIDColorValue;
            }
            set
            {
                blnIDColorValue = value;
                OnPropertyChanged("blnIDColor");
            }
        }
        private string BtnDateColorValue = string.Empty;
        public string BtnDateColor
        {
            get
            {
                return BtnDateColorValue;
            }
            set
            {
                BtnDateColorValue = value;
                OnPropertyChanged("BtnDateColor");
            }
        }
  

        public bool CoreRestoreDesign
        {
            get { return CoreRestoreDesignValue; }
            set
            {
                CoreRestoreDesignValue = value;
                OnPropertyChanged("CoreRestoreDesign");
            }
        }

        public Visibility CoreRestoreDesignVisibility
        {
            get { return CoreRestoreDesignVisibilityValue; }
            set
            {
                CoreRestoreDesignVisibilityValue = value;
                OnPropertyChanged("CoreRestoreDesignVisibility");
            }
        }

        public Visibility ClearLogVisibility
        {
            get { return ClearLogVisibilityValue; }
            set
            {
                ClearLogVisibilityValue = value;
                OnPropertyChanged("ClearLogVisibility");
            }
        }

        public bool ClearLogs
        {
            get { return ClearLogsValue; }
            set
            {
                ClearLogsValue = value;
                OnPropertyChanged("ClearLogs");
            }
        }
        public bool Bypass
        {
            get { return BypassValue; }
            set
            {
                BypassValue = value;
                OnPropertyChanged("Bypass");
            }
        }

        public bool DesignerActionPresent
        {
            get { return designerActionPresent; }
            set
            {
                designerActionPresent = value;
                OnPropertyChanged("DesignerActionPresent");
            }
        }

        private List<TreeViewExplorer> testSuiteTreeViewListValue = new List<TreeViewExplorer>();
        public List<TreeViewExplorer> TestSuiteTreeViewList
        {
            get { return testSuiteTreeViewListValue; }
            set { testSuiteTreeViewListValue = value; OnPropertyChanged("TestSuiteTreeViewList"); }
        }

        private string testSuiteNameListValue = null;
        public string TestSuiteNameList
        {
            get { return testSuiteNameListValue; }
            set { testSuiteNameListValue = value; OnPropertyChanged("TestSuiteNameList"); }
        }

        private Visibility CoreDateTimeVisibilityValue = Visibility.Collapsed;
        public Visibility CoreDateTimeVisibility
        {
            get { return CoreDateTimeVisibilityValue; }
            set
            {
                CoreDateTimeVisibilityValue = value;
                OnPropertyChanged("CoreDateTimeVisibility");
            }
        }

        private string deviceDateTimeValue = string.Empty;
        public string DeviceDateTime
        {
            get { return deviceDateTimeValue; }
            set
            {
                deviceDateTimeValue = value;
                OnPropertyChanged("DeviceDateTime");
            }
        }

        private string DeviceDateValue = string.Empty;
        public string DeviceDate
        {
            get { return DeviceDateValue; }
            set
            {
                DeviceDateValue = value;
                OnPropertyChanged("DeviceDate");
            }
        }

        private Core_DateTime coreDateTimeValue = new Core_DateTime();
        public Core_DateTime CoreDateTimeList
        {
            get { return coreDateTimeValue; }
            set
            {
                coreDateTimeValue = value;
                OnPropertyChanged("CoreDateTimeList");
            }
        }

        private Visibility IDButtonVisibilityValue = Visibility.Collapsed;
        public Visibility IDButtonVisibility
        {
            get { return IDButtonVisibilityValue; }
            set
            {
                IDButtonVisibilityValue = value;
                OnPropertyChanged("IDButtonVisibility");
            }
        }

        public Core_DateTime CoreDateTimeSettings()
        {
            Core_DateTime setCoreDatetimeSettings = new Core_DateTime();
            setCoreDatetimeSettings.ParentDUTDeviceItem = this;
            CoreDateTimeList=setCoreDatetimeSettings;
            return setCoreDatetimeSettings;
        }

        private TreeViewExplorer parentTestPlanTreeViewValue = null;
        public TreeViewExplorer ParentTestPlanTreeView
        {
            get { return parentTestPlanTreeViewValue; }
            set { parentTestPlanTreeViewValue = value; OnPropertyChanged("ParentTestPlanTreeView"); }
        }
    }

    public class Core_DateTime: INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public bool isSkipSaveButtonEnable = false;

        void OnPropertyChanged(string property)
        {
            try
            {
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs(property));

                    if (SaveButtonIsEnabled != true && property != "SaveButtonIsEnabled" && property != "PoolListSelectedItem" && isSkipSaveButtonEnable == false)
                        SaveButtonIsEnabled = true;

                    isSkipSaveButtonEnable = false;
                }
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                //MessageBox.Show("Exception\n " + ex.Message, "Error Code - EC20001", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool saveButtonIsEnabledValue = false;
        public bool SaveButtonIsEnabled
        {
            get { return saveButtonIsEnabledValue; }
            set { saveButtonIsEnabledValue = value; OnPropertyChanged("SaveButtonIsEnabled"); OnPropertyChanged("TestItemHeaderName"); }
        }

        private string dateTimeIPAddressValue = null;
        public string DateTimeIPAddress
        {
            get { return dateTimeIPAddressValue; }
            set
            {
                dateTimeIPAddressValue = value;
                OnPropertyChanged("DateTimeIPAddress");
            }
        }

        private DateTime? calendarDisplayDateValue = null;
        public DateTime? CalendarDisplayDate
        {
            get { return calendarDisplayDateValue; }
            set
            {
                calendarDisplayDateValue = value;
                OnPropertyChanged("CalendarDisplayDate");
            }
        }

        private DateTime? calendarSelectedDateValue = null;
        public DateTime? CalendarSelectedDate
        {
            get { return calendarSelectedDateValue; }
            set
            {
                calendarSelectedDateValue = value;
                OnPropertyChanged("CalendarSelectedDate");
            }
        }

        private string coreCurrentTimeValue = null;
        public string CoreCurrentTime
        {
            get { return coreCurrentTimeValue; }
            set
            {
                coreCurrentTimeValue = value;
                OnPropertyChanged("CoreCurrentTime");
            }
        }

        private double clockSecondsAngleValue = 0;
        public double ClockSecondsAngle
        {
            get { return clockSecondsAngleValue; }
            set
            {
                clockSecondsAngleValue = value;
                OnPropertyChanged("ClockSecondsAngle");
            }
        }

        private double clockMinuteAngleValue = 0;
        public double ClockMinuteAngle
        {
            get { return clockMinuteAngleValue; }
            set
            {
                clockMinuteAngleValue = value;
                OnPropertyChanged("ClockMinuteAngle");
            }
        }

        private double clockHourAngleValue = 0;
        public double ClockHourAngle
        {
            get { return clockHourAngleValue; }
            set
            {
                clockHourAngleValue = value;
                OnPropertyChanged("ClockHourAngle");
            }
        }

        private bool? Datetime_state_Value=null; 
        public bool? Datetime_state
        {
            get { return Datetime_state_Value; }
            set
            {
                Datetime_state_Value = value;
                OnPropertyChanged("Datetime_state");
              
            }
        }

        private bool NTPCheckedValue = false;
        public bool NTPChecked
        {
            get { return NTPCheckedValue; }
            set
            {
                NTPCheckedValue = value;
                OnPropertyChanged("NTPChecked");
                if(value == true)
                {
                    NTPListVisibility = Visibility.Visible;
                    PoolAddVisibility = Visibility.Visible;
                    PoolRemoveVisibility = Visibility.Visible;
                }
                else
                {
                    NTPListVisibility = Visibility.Collapsed;
                    PoolAddVisibility = Visibility.Collapsed;
                    PoolRemoveVisibility = Visibility.Collapsed;
                }
            }
        }

        private Visibility NTPEnableVisibilityValue = Visibility.Collapsed;
        public Visibility NTPListVisibility
        {
            get { return NTPEnableVisibilityValue; }
            set
            {
                NTPEnableVisibilityValue = value;
                OnPropertyChanged("NTPListVisibility");
            }
        }

        private Visibility PoolAddVisibilityValue = Visibility.Collapsed;
        public Visibility PoolAddVisibility
        {
            get { return PoolAddVisibilityValue; }
            set
            {
                PoolAddVisibilityValue = value;
                OnPropertyChanged("PoolAddVisibility");
            }
        }

        private Visibility PoolRemoveVisibilityValue = Visibility.Collapsed;
        public Visibility PoolRemoveVisibility
        {
            get { return PoolRemoveVisibilityValue; }
            set
            {
                PoolRemoveVisibilityValue = value;
                OnPropertyChanged("PoolRemoveVisibility");
            }
        }

        private ObservableCollection<string> PoolListViewValue = new ObservableCollection<string>();
        public ObservableCollection<string> PoolListViewItems
        {
            get { return PoolListViewValue; }
            set { PoolListViewValue = value; OnPropertyChanged("PoolListViewItems"); }
        }

        private string PoolListSelectedItemValue = null;
        public string PoolListSelectedItem
        {
            get { return PoolListSelectedItemValue; }
            set
            {
                PoolListSelectedItemValue = value;
                OnPropertyChanged("PoolListSelectedItem");
            }
        }

        private ObservableCollection<ComboBoxItem> TimeZoneItemsourceValue = new ObservableCollection<ComboBoxItem>();
        public ObservableCollection<ComboBoxItem> TimeZoneItemsource
        {
            get { return TimeZoneItemsourceValue; }
            set { TimeZoneItemsourceValue = value; OnPropertyChanged("TimeZoneItemsource"); }
        }

        private ComboBoxItem TimeZoneSelectedItemValue = null;
        public ComboBoxItem TimeZoneSelectedItem
        {
            get { return TimeZoneSelectedItemValue; }
            set
            {
                TimeZoneSelectedItemValue = value;
                OnPropertyChanged("TimeZoneSelectedItem");
            }
        }

        private string TimeZonesValue = "[{\"Africa\": [\"Abidjan\",\"Accra\",\"Addis Ababa\",\"Algiers\",\"Asmara\",\"Asmera\",\"Bamako\",\"Bangui\",\"Banjul\",\"Bissau\",\"Blantyre\",\"Brazzaville\",\"Bujumbura\",\"Cairo\",\"Casablanca\",\"Ceuta\",\"Conakry\",\"Dakar\",\"Dar es Salaam\",\"Djibouti\",\"Douala\",\"El Aaiun\",\"Freetown\",\"Gaborone\",\"Harare\",\"Johannesburg\",\"Juba\",\"Kampala\",\"Khartoum\",\"Kigali\",\"Kinshasa\",\"Lagos\",\"Libreville\",\"Lome\",\"Luanda\",\"Lubumbashi\",\"Lusaka\",\"Malabo\",\"Maputo\",\"Maseru\",\"Mbabane\",\"Mogadishu\",\"Monrovia\",\"Nairobi\",\"Ndjamena\",\"Niamey\",\"Nouakchott\",\"Ouagadougou\",\"Porto - Novo\",\"Sao Tome\",\"Timbuktu\",\"Tripoli\",\"Tunis\",\"Windhoek\"]},{\"America\": [\"Adak\",\"Anchorage\",\"Anguilla\",\"Antigua\",\"Araguaina\",\"Aruba\",\"Asuncion\",\"Atikokan\",\"Atka\",\"Bahia\",\"Bahia Banderas\",\"Barbados\",\"Belem\",\"Belize\",\"Blanc - Sablon\",\"Boa Vista\",\"Bogota\",\"Boise\",\"Buenos Aires\",\"Cambridge Bay\",\"Campo Grande\",\"Cancun\",\"Caracas\",\"Catamarca\",\"Cayenne\",\"Cayman\",\"Chicago\",\"Chihuahua\",\"Coral Harbour\",\"Cordoba\",\"Costa Rica\",\"Creston\",\"Cuiaba\",\"Curacao\",\"Danmarkshavn\",\"Dawson\",\"Dawson Creek\",\"Denver\",\"Detroit\",\"Dominica\",\"Edmonton\",\"Eirunepe\",\"El Salvador\",\"Ensenada\",\"Fort Nelson\",\"Fort Wayne\",\"Fortaleza\",\"Glace Bay\",\"Godthab\",\"Goose Bay\",\"Grand Turk\",\"Grenada\",\"Guadeloupe\",\"Guatemala\",\"Guayaquil\",\"Guyana\",\"Halifax\",\"Havana\",\"Hermosillo\",\"Indianapolis\",\"Inuvik\",\"Iqaluit\",\"Jamaica\",\"Jujuy\",\"Juneau\",\"Knox IN\",\"Kralendijk\",\"La Paz\",\"Lima\",\"Los Angeles\",\"Louisville\",\"Lower Princes\",\"Maceio\",\"Managua\",\"Manaus\",\"Marigot\",\"Martinique\",\"Matamoros\",\"Mazatlan\",\"Mendoza\",\"Menominee\",\"Merida\",\"Metlakatla\",\"Mexico City\",\"Miquelon\",\"Moncton\",\"Monterrey\",\"Montevideo\",\"Montreal\",\"Montserrat\",\"Nassau\",\"New York\",\"Nipigon\",\"Nome\",\"Noronha\",\"Ojinaga\",\"Panama\",\"Pangnirtung\",\"Paramaribo\",\"Phoenix\",\"Port - au - Prince\",\"Port of Spain\",\"Porto Acre\",\"Porto Velho\",\"Puerto Rico\",\"Punta Arenas\",\"Rainy River\",\"Rankin Inlet\",\"Recife\",\"Regina\",\"Resolute\",\"Rio Branco\",\"Rosario\",\"Santa Isabel\",\"Santarem\",\"Santiago\",\"Santo Domingo\",\"Sao Paulo\",\"Scoresbysund\",\"Shiprock\",\"Sitka\",\"St Barthelemy\",\"St Johns\",\"St Kitts\",\"St Lucia\",\"St Thomas\",\"St Vincent\",\"Swift Current\",\"Tegucigalpa\",\"Thule\",\"Thunder Bay\",\"Tijuana\",\"Toronto\",\"Tortola\",\"Vancouver\",\"Virgin\",\"Whitehorse\",\"Winnipeg\",\"Yakutat\",\"Yellowknife\"]},{\"America/Argentina\": [\"Buenos Aires\",\"Catamarca\",\"ComodRivadavia\",\"Cordoba\",\"Jujuy\",\"La Rioja\",\"Mendoza\",\"Rio Gallegos\",\"Salta\",\"San Juan\",\"San Luis\",\"Tucuman\",\"Ushuaia\"]},{\"America/Indiana\":[\"Indianapolis\",\"Knox\",\"Marengo\",\"Petersburg\",\"Tell City\",\"Vevay\",\"Vincennes\",\"Winamac\"]},{\"America/Kentucky\":[\"Louisville\",\"Monticello\"]},{\"America/North Dakota\":[\"Beulah\",\"Center\",\"New Salem\"]},{\"Antarctica\":[\"Casey\",\"Davis\",\"DumontDUrville\",\"Macquarie\",\"Mawson\",\"McMurdo\",\"Palmer\",\"Rothera\",\"South Pole\",\"Syowa\",\"Troll\",\"Vostok\"]},{\"Arctic\":[\"Longyearbyen\"]},{\"Asia\":[\"Aden\",\"Almaty\",\"Amman\",\"Anadyr\",\"Aqtau\",\"Aqtobe\",\"Ashgabat\",\"Ashkhabad\",\"Atyrau\",\"Baghdad\",\"Bahrain\",\"Baku\",\"Bangkok\",\"Barnaul\",\"Beirut\",\"Bishkek\",\"Brunei\",\"Calcutta\",\"Chita\",\"Choibalsan\",\"Chongqing\",\"Chungking\",\"Colombo\",\"Dacca\",\"Damascus\",\"Dhaka\",\"Dili\",\"Dubai\",\"Dushanbe\",\"Famagusta\",\"Gaza\",\"Harbin\",\"Hebron\",\"Ho Chi Minh\",\"Hong Kong\",\"Hovd\",\"Irkutsk\",\"Istanbul\",\"Jakarta\",\"Jayapura\",\"Jerusalem\",\"Kabul\",\"Kamchatka\",\"Karachi\",\"Kashgar\",\"Kathmandu\",\"Katmandu\",\"Khandyga\",\"Kolkata\",\"Krasnoyarsk\",\"Kuala Lumpur\",\"Kuching\",\"Kuwait\",\"Macao\",\"Macau\",\"Magadan\",\"Makassar\",\"Manila\",\"Muscat\",\"Nicosia\",\"Novokuznetsk\",\"Novosibirsk\",\"Omsk\"]},{\"Asia\":[\"Aden\",\"Almaty\",\"Amman\",\"Anadyr\",\"Aqtau\",\"Aqtobe\",\"Ashgabat\",\"Ashkhabad\",\"Atyrau\",\"Baghdad\",\"Bahrain\",\"Baku\",\"Bangkok\",\"Barnaul\",\"Beirut\",\"Bishkek\",\"Brunei\",\"Calcutta\",\"Chita\",\"Choibalsan\",\"Chongqing\",\"Chungking\",\"Colombo\",\"Dacca\",\"Damascus\",\"Dhaka\",\"Dili\",\"Dubai\",\"Dushanbe\",\"Famagusta\",\"Gaza\",\"Harbin\",\"Hebron\",\"Ho Chi Minh\",\"Hong Kong\",\"Hovd\",\"Irkutsk\",\"Istanbul\",\"Jakarta\",\"Jayapura\",\"Jerusalem\",\"Kabul\",\"Kamchatka\",\"Karachi\",\"Kashgar\",\"Kathmandu\",\"Katmandu\",\"Khandyga\",\"Kolkata\",\"Krasnoyarsk\",\"Kuala Lumpur\",\"Kuching\",\"Kuwait\",\"Macao\",\"Macau\",\"Magadan\",\"Makassar\",\"Manila\",\"Muscat\",\"Nicosia\",\"Novokuznetsk\",\"Novosibirsk\",\"Omsk\",\"Oral\",\"Phnom Penh\",\"Pontianak\",\"Pyongyang\",\"Qatar\",\"Qostanay\",\"Qyzylorda\",\"Rangoon\",\"Riyadh\",\"Saigon\",\"Sakhalin\",\"Samarkand\",\"Seoul\",\"Shanghai\",\"Singapore\",\"Srednekolymsk\",\"Taipei\",\"Tashkent\",\"Tbilisi\",\"Tbilisi\",\"Tehran\",\"Tel Aviv\",\"Thimbu\",\"Thimphu\",\"Tokyo\",\"Tomsk\",\"Ujung Pandang\",\"Ulaanbaatar\",\"Ulaanbaatar\",\"Ulan Bator\",\"Urumqi\",\"Ust - Nera\",\"Vientiane\",\"Vladivostok\",\"Yakutsk\",\"Yangon\",\"Yekaterinburg\",\"Yerevan\"]},{\"Atlantic\":[\"Azores\",\"Bermuda\",\"Canary\",\"Cape Verde\",\"Faeroe\",\"Faroe\",\"Jan Mayen\",\"Madeira\",\"Reykjavik\",\"South Georgia\",\"St Helena\",\"Stanley\"]},{\"Australia\":[\"ACT\",\"Adelaide\",\"Brisbane\",\"Broken Hill\",\"Canberra\",\"Currie\",\"Darwin\",\"Darwin\",\"Eucla\",\"Hobart\",\"LHI\",\"Lindeman\",\"Lord Howe\",\"Melbourne\",\"NSW\",\"North\",\"Perth\",\"Queensland\",\"South\",\"Sydney\",\"Tasmania\",\"Victoria\",\"West\",\"Yancowinna\"]},{\"Europe\":[\"Amsterdam\",\"Andorra\",\"Astrakhan\",\"Athens\",\"Belfast\",\"Belgrade\",\"Berlin\",\"Bratislava\",\"Brussels\",\"Bucharest\",\"Budapest\",\"Busingen\",\"Chisinau\",\"Copenhagen\",\"Dublin\",\"Gibraltar\",\"Guernsey\",\"Helsinki\",\"Isle of Man\",\"Istanbul\",\"Jersey\",\"Kaliningrad\",\"Kiev\",\"Kirov\",\"Kirov\",\"Lisbon\",\"Ljubljana\",\"London\",\"Luxembourg\",\"Madrid\",\"Malta\",\"Mariehamn\",\"Minsk\",\"Monaco\",\"Moscow\",\"Nicosia\",\"Oslo\",\"Paris\",\"Podgorica\",\"Prague\",\"Riga\",\"Rome\",\"Samara\",\"San Marino\",\"Sarajevo\",\"Saratov\",\"Simferopol\",\"Skopje\",\"Sofia\",\"Stockholm\",\"Tallinn\",\"Tirane\",\"Tiraspol\",\"Ulyanovsk\",\"Uzhgorod\",\"Vaduz\",\"Vatican\",\"Vienna\",\"Vilnius\",\"Volgograd\",\"Warsaw\",\"Zagreb\",\"Zaporozhye\",\"Zurich\"]},{\"Indian\":[\"Antananarivo\",\"Chagos\",\"Christmas\",\"Cocos\",\"Comoro\",\"Kerguelen\",\"Mahe\",\"Maldives\",\"Mauritius\",\"Mauritius\",\"Mauritius\",\"Mayotte\",\"Reunion\"]},{\"Pacific\":[\"Apia\",\"Auckland\",\"Bougainville\",\"Chatham\",\"Chuuk\",\"Easter\",\"Efate\",\"Enderbury\",\"Fakaofo\",\"Fiji\",\"Funafuti\",\"Funafuti\",\"Funafuti\",\"Galapagos\",\"Gambier\",\"Guadalcanal\",\"Guam\",\"Honolulu\",\"Johnston\",\"Kiritimati\",\"Kosrae\",\"Kwajalein\",\"Majuro\",\"Marquesas\",\"Midway\",\"Nauru\",\"Niue\",\"Norfolk\",\"Noumea\",\"Pago Pago\",\"Palau\",\"Pitcairn\",\"Pohnpei\",\"Ponape\",\"Port Moresby\",\"Rarotonga\",\"Saipan\",\"Samoa\",\"Tahiti\",\"Tarawa\",\"Tongatapu\",\"Truk\",\"Wake\",\"Wallis\",\"Yap\"]}]";
        public string TimeZoneJSON
        {
            get { return TimeZonesValue; }
            set
            {
                TimeZonesValue = value;
                OnPropertyChanged("TimeZoneJSON");
            }
        }

        private int PoolListSelectedIndexValue = 0;
        public int PoolListSelectedIndex
        {
            get { return PoolListSelectedIndexValue; }
            set
            {
                PoolListSelectedIndexValue = value;
                isSkipSaveButtonEnable = true;
                OnPropertyChanged("PoolListSelectedIndex");
            }
        }

        private DUT_DeviceItem parentDUTDeviceItemValue = null;
        public DUT_DeviceItem ParentDUTDeviceItem
        {
            get { return parentDUTDeviceItemValue; }
            set { parentDUTDeviceItemValue = value; OnPropertyChanged("ParentDUTDeviceItem"); }
        }
    }    
}
