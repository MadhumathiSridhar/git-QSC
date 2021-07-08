using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Data;
using System.Windows;
using System.Text.RegularExpressions;

namespace QSC_Test_Automation
{
    public class CBMItems:INotifyPropertyChanged
    {
        DBConnection QscDatabase = new DBConnection();

        private List<string> componentTypeListValue = new List<string>();
        public List<string> ComponentTypeList
        {
            get { return componentTypeListValue; }
            set { componentTypeListValue = value; }
        }

        private Dictionary<string, ObservableCollection<string>> componentNameDictionaryValue = new Dictionary<string, ObservableCollection<string>>();
        public Dictionary<string, ObservableCollection<string>> ComponentNameDictionary
        {
            get { return componentNameDictionaryValue; }
            set { componentNameDictionaryValue = value; }
        }

        private Dictionary<string, ObservableCollection<string>> VerifycontrolNameDictionaryValue = new Dictionary<string, ObservableCollection<string>>();
        public Dictionary<string, ObservableCollection<string>> VerifyControlNameDictionary
        {
            get { return VerifycontrolNameDictionaryValue; }
            set { VerifycontrolNameDictionaryValue = value; }
        }


        private Dictionary<string, ObservableCollection<string>> ChannelSelectionDictionaryValue = new Dictionary<string, ObservableCollection<string>>();
        public Dictionary<string, ObservableCollection<string>> ChannelSelectionDictionary
        {
            get { return ChannelSelectionDictionaryValue; }
            set { ChannelSelectionDictionaryValue = value; }
        }

        private Dictionary<string, ObservableCollection<string>> controlNameDictionaryValue = new Dictionary<string, ObservableCollection<string>>();
        public Dictionary<string, ObservableCollection<string>> ControlNameDictionary
        {
            get { return controlNameDictionaryValue; }
            set { controlNameDictionaryValue = value; }
        }

        private Dictionary<string, string> controlTypeDictionaryValue = new Dictionary<string, string>();
        public Dictionary<string, string> ControlTypeDictionary
        {
            get { return controlTypeDictionaryValue; }
            set { controlTypeDictionaryValue = value; }
        }

        private Dictionary<string, string> channelControlTypeDictionaryValue = new Dictionary<string, string>();
        public Dictionary<string, string> channelControlTypeDictionary
        {
            get { return channelControlTypeDictionaryValue; }
            set { channelControlTypeDictionaryValue = value; }
        }


        private Dictionary<string, string> VerifychannelControlTypeDictionaryValue = new Dictionary<string, string>();
        public Dictionary<string, string> VerifychannelControlTypeDictionary
        {
            get { return VerifychannelControlTypeDictionaryValue; }
            set { VerifychannelControlTypeDictionaryValue = value; }
        }

        private Dictionary<string, string> dataTypeDictionaryValue = new Dictionary<string, string>();
        public Dictionary<string, string> dataTypeDictionary
        {
            get { return dataTypeDictionaryValue; }
            set { dataTypeDictionaryValue = value; }
        }

        private Dictionary<string, string> controlInitialValueDictionaryValue = new Dictionary<string, string>();
        public Dictionary<string, string> ControlInitialValueDictionary
        {
            get { return controlInitialValueDictionaryValue; }
            set { controlInitialValueDictionaryValue = value; }
        }

        private Dictionary<string, string> controlInitialStringDictionaryValue = new Dictionary<string, string>();
        public Dictionary<string, string> ControlInitialStringDictionary
        {
            get { return controlInitialStringDictionaryValue; }
            set { controlInitialStringDictionaryValue = value; }
        }

        private Dictionary<string, string> controlInitialPositionDictionaryValue = new Dictionary<string, string>();
        public Dictionary<string, string> ControlInitialPositionDictionary
        {
            get { return controlInitialPositionDictionaryValue; }
            set { controlInitialPositionDictionaryValue = value; }
        }

        private Dictionary<string, string> MaximumControlValueDictionaryValue = new Dictionary<string, string>();
        public Dictionary<string, string> MaximumControlValueDictionary
        {
            get { return MaximumControlValueDictionaryValue; }
            set { MaximumControlValueDictionaryValue = value; }
        }

        private Dictionary<string, string[]> controlIDDictionaryValue = new Dictionary<string, string[]>();
        public Dictionary<string, string[]> ControlIDDictionary
        {
            get { return controlIDDictionaryValue; }
            set { controlIDDictionaryValue = value; }
        }

        private Dictionary<string, string> MinimumControlValueDictionaryValue = new Dictionary<string, string>();
        public Dictionary<string, string> MinimumControlValueDictionary
        {
            get { return MinimumControlValueDictionaryValue; }
            set { MinimumControlValueDictionaryValue = value; }
        }

        private Dictionary<string, string> VerifyDataTypeDictionaryValue = new Dictionary<string, string>();
        public Dictionary<string, string> VerifyDataTypeDictionary
        {
            get { return VerifyDataTypeDictionaryValue; }
            set { VerifyDataTypeDictionaryValue = value; }
        }


        private Dictionary<string, string> VerifycontrolTypeDictionaryValue = new Dictionary<string, string>();
        public Dictionary<string, string> VerifyControlTypeDictionary
        {
            get { return VerifycontrolTypeDictionaryValue; }
            set { VerifycontrolTypeDictionaryValue = value; }
        }


        private bool ContinueWithoutdoinganythingvalue = false;
        public bool ContinueWithoutdoinganything
        {
            get
            {
                return ContinueWithoutdoinganythingvalue;
            }
            set
            {
                ContinueWithoutdoinganythingvalue = value;
                if (ContinueWithoutdoinganything == true)
                    FailureCriteria = string.Empty;
                NotifyPropertyChanged("ContinueWithoutdoinganything");
            }
        }

        private bool StoreCurrentResultvalue = true;
        public bool StoreCurrentResult
        {
            get
            {
                return StoreCurrentResultvalue;
            }
            set
            {
                StoreCurrentResultvalue = value;
                //if (StoreCurrentResult == true)
                //    FailureCriteria = string.Empty;
                NotifyPropertyChanged("StoreCurrentResult");
            }
        }

        private bool CompareValuesvalue = false;
        public bool CompareValues
        {
            get
            {
                return CompareValuesvalue;
            }
            set
            {
                CompareValuesvalue = value;
                if(CompareValuesvalue==true)
                {
                    FailureCriteriaEnable = true;
                }
                else
                {
                    FailureCriteriaEnable = false;
                }
                NotifyPropertyChanged("CompareValues");
            }
        }

        private List<string> KeywordTypeListValue = new List<string> { "Keyword exist", "Keyword not exist" };
        public List<string> KeywordTypeList
        {
            get { return KeywordTypeListValue; }
            set { KeywordTypeListValue = value; NotifyPropertyChanged("KeywordTypeList"); }
        }

        private string KeywordTypeVerifyValue = "Keyword not exist";
        public string KeywordTypeVerify
        {
            get { return KeywordTypeVerifyValue; }
            set { KeywordTypeVerifyValue = value;

                if (KeywordTypeVerify == "Keyword not exist")
                    keywordTypetooltip = "Pass when keyword text not available in telnet response";
                else if (KeywordTypeVerify == "Keyword exist")
                    keywordTypetooltip = "Pass when keyword text available in telnet response";
                NotifyPropertyChanged("KeywordTypeVerify"); }
        }

        private string keywordTypetooltipValue = "Pass when keyword text not available in telnet response";
        public string keywordTypetooltip
        {
            get { return keywordTypetooltipValue; }
            set
            {
                keywordTypetooltipValue = value;
                NotifyPropertyChanged("keywordTypetooltip");
            }
        }
        private string FailureCriteriaValue = string.Empty;
        public string FailureCriteria
        {
            get
            {
                if (FailureCriteriaValue != null)
                    return FailureCriteriaValue.TrimStart();
                else
                    return FailureCriteriaValue;
            }
            set
            {
                FailureCriteriaValue = value;
                NotifyPropertyChanged("FailureCriteria");
            }
        }

        private bool FailureCriteriaEnableValue = false;
        public bool FailureCriteriaEnable
        {
            get
            {
                return FailureCriteriaEnableValue;
            }
            set
            {
                FailureCriteriaEnableValue = value;
                NotifyPropertyChanged("FailureCriteriaEnable");
            }
        }

        private bool PauseatErrorstateValue = false;
        public bool PauseatErrorstate
        {
            get
            {
                return PauseatErrorstateValue;
            }
            set
            {
                PauseatErrorstateValue = value;
                //if (PauseatErrorstate == true)
                //    ReRunFailedTestCase = "0";
                NotifyPropertyChanged("PauseatErrorstate");
            }
        }

        private bool ContinueTestingValue = true;
        public bool ContinueTesting
        {
            get
            {
                return ContinueTestingValue;
            }
            set
            {
                ContinueTestingValue = value;
                if(ContinueTesting==true)
                {
                    ReRunFailedTestCaseEnabled = true;
                  //  ReRunFailedTestCase = "0";
                }
                else
                {
                    ReRunFailedTestCaseEnabled = false;
                    //ReRunFailedTestCase = "0";
                }
                NotifyPropertyChanged("ContinueTesting");
            }
        }


        private string ReRunFailedTestCaseValue = "0";
        public string ReRunFailedTestCase
        {
            get
            {
                return ReRunFailedTestCaseValue;
            }
            set
            {
                ReRunFailedTestCaseValue = value;
                NotifyPropertyChanged("ReRunFailedTestCase");
            }
        }

        private bool ReRunFailedTestCaseEnabledValue = false;
        public bool ReRunFailedTestCaseEnabled
        {
            get
            {
                return ReRunFailedTestCaseEnabledValue;
            }
            set
            {
                ReRunFailedTestCaseEnabledValue = value;
                NotifyPropertyChanged("ReRunFailedTestCaseEnabled");
            }
        }

        private List<string> cmb_ComponentTypeValue = new List<string>();
        public List<string> cmb_ComponentType
        {
            get
            {
                return cmb_ComponentTypeValue;
            }
            set
            {
                cmb_ComponentTypeValue = value;
                NotifyPropertyChanged("cmb_ComponentType");
            }
        }


        private string cmb_ComponentTypeSelectedItemValue = string.Empty;
        public string cmb_ComponentTypeSelectedItem
        {
            get
            {
                return cmb_ComponentTypeSelectedItemValue;
            }
            set
            {                
                cmb_ComponentTypeSelectedItemValue = value;
                NotifyPropertyChanged("cmb_ComponentTypeSelectedItem");                              
            }
        }

        private List<string> ComponentNameListValue = new List<string>();
        public List<string> ComponentNameList
        {
            get
            {
                return ComponentNameListValue;
            }
            set
            {
                ComponentNameListValue = value;
                NotifyPropertyChanged("ComponentNameList");
            }
        }

        private string ComponentNameSelectedItemValue = string.Empty;
        public string ComponentNameSelectedItem
        {
            get
            {
                return ComponentNameSelectedItemValue;
            }
            set
            {
                ComponentNameSelectedItemValue = value;
                NotifyPropertyChanged("ComponentNameSelectedItem");
            }
        }


        private List<string> PropertyListValue = new List<string>();
        public List<string> PropertyList
        {
            get
            {
                return PropertyListValue;
            }
            set
            {
                PropertyListValue = value;
                NotifyPropertyChanged("PropertyList");
            }
        }

        private string PropertySelectedItemValue = string.Empty;
        public string PropertySelectedItem
        {
            get
            {
                return PropertySelectedItemValue;
            }
            set
            {
                PropertySelectedItemValue = value;
                if (PropertySelectedItemValue != null && PropertySelectedItemValue != string.Empty)
                {
                    //sankar
                    //if ((PropertySelectedItemValue.StartsWith("CHANNEL "))|| (PropertySelectedItemValue.StartsWith("OUTPUT "))|| (PropertySelectedItemValue.StartsWith("INPUT ")) || (PropertySelectedItemValue.StartsWith("INPUT_OUTPUT ")) || (PropertySelectedItemValue.StartsWith("TAP ")) || (PropertySelectedItemValue.StartsWith("BANK_CONTROL ")) || (PropertySelectedItemValue.StartsWith("BANK_SELECT ")) || (PropertySelectedItemValue.StartsWith("GPIO_INPUT ")) || (PropertySelectedItemValue.StartsWith("GPIO_OUTPUT ")))
                    //{
                        //cmb_ChannelEnable = true;
                        //if (cmb_ChannelSelectedItem != null && cmb_ChannelSelectedItem != string.Empty)
                        //    InputSelectionEnabled = true;
                        //else
                        //    InputSelectionEnabled = false;
                    //}
                    //else
                    //{                     
                    //    cmb_ChannelEnable = false;
                    //    InputSelectionEnabled = true;
                    //}
                }
                else
                {
                        //cmb_ChannelEnable = false;
                        //InputSelectionEnabled = false;
                        //PropertyInitialValueSelectedItemEnable = false;
                }
               NotifyPropertyChanged("PropertySelectedItem");
            }
        }

        private string PropertyInitialValueSelectedItemValue = string.Empty;
        public string PropertyInitialValueSelectedItem
        {
            get
            {
                return PropertyInitialValueSelectedItemValue;
            }
            set
            {
                PropertyInitialValueSelectedItemValue = value;
                NotifyPropertyChanged("PropertyInitialValueSelectedItem");
            }
        }

        private bool CVMPauseatErrorstateValue = false;
        public bool CVMPauseatErrorstate
        {
            get
            {
                return CVMPauseatErrorstateValue;
            }
            set
            {
                CVMPauseatErrorstateValue = value;
                //if (CVMPauseatErrorstate == true)
                //    CVMReRunFailedTestCase = "0";
                NotifyPropertyChanged("CVMPauseatErrorstate");
            }
        }

        private bool CVMContinueTestingValue = true;
        public bool CVMContinueTesting
        {
            get
            {
                return CVMContinueTestingValue;
            }
            set
            {
                CVMContinueTestingValue = value;
                if (CVMContinueTesting == true)
                {
                  //  CVMReRunFailedTestCase = "0";
                    CVMReRunFailedTestCaseEnabled = true;
                }
                else
                {
                    //CVMReRunFailedTestCase = "0";
                    CVMReRunFailedTestCaseEnabled = false;
                }
                NotifyPropertyChanged("CVMContinueTesting");
            }
        }


        private string CVMReRunFailedTestCaseValue="0";
        public string CVMReRunFailedTestCase
        {
            get
            {
                return CVMReRunFailedTestCaseValue;
            }
            set
            {
                CVMReRunFailedTestCaseValue = value;
                NotifyPropertyChanged("CVMReRunFailedTestCase");
            }
        }

        private bool CVMReRunFailedTestCaseEnabledValue = false;
        public bool CVMReRunFailedTestCaseEnabled
        {
            get
            {
                return CVMReRunFailedTestCaseEnabledValue;
            }
            set
            {
                CVMReRunFailedTestCaseEnabledValue = value;
                NotifyPropertyChanged("CVMReRunFailedTestCaseEnabled");
            }
        }
        
        private ObservableCollection<string> InputSelectionComboListValue = new ObservableCollection<string> { "Set by value", "Set by string", "Set by position" };        
        public ObservableCollection<string> InputSelectionComboList
        {
            get
            {
                return InputSelectionComboListValue;
            }
            set
            {
                InputSelectionComboListValue = value;
                NotifyPropertyChanged("InputSelectionComboList");
            }
        }

        private bool InputSelectionEnabledValue = false;
        public bool InputSelectionEnabled
        {
            get
            {
                return InputSelectionEnabledValue;
            }
            set
            {
                InputSelectionEnabledValue = value;
                NotifyPropertyChanged("InputSelectionEnabled");
            }
        }

        private bool cmb_ValueSetValue = false;
        public bool cmb_ValueSet
        {
            get
            {
                return cmb_ValueSetValue;
            }
            set
            {
                cmb_ValueSetValue = value;
                NotifyPropertyChanged("cmb_ValueSet");
            }
        }

        private string InputSelectionComboSelectedItemValue = string.Empty;
        public string InputSelectionComboSelectedItem
        {
            get
            {
                return InputSelectionComboSelectedItemValue;
            }
            set
            {
                InputSelectionComboSelectedItemValue = value;
                if (cmb_ValueSet == false)
                {
                    if (InputSelectionComboSelectedItem != null && InputSelectionComboSelectedItem != string.Empty)
                    {
                        PropertyInitialValueSelectedItemEnable = true;
                        FillInitialTextBoxValue();
                    }
                    else
                    {
                        PropertyInitialValueSelectedItemEnable = false;
                    }

                    if (InputSelectionComboSelectedItem != null && InputSelectionComboSelectedItem == "Set by value")
                    {
                        MaxLimitIsEnabled = true;
                        MinLimitIsEnabled = true;
                    }
                    else
                    {
                        MaxLimitIsEnabled = false;
                        MinLimitIsEnabled = false;
                    }
                }

               
                NotifyPropertyChanged("InputSelectionComboSelectedItem");
            }
        }

        private void FillInitialTextBoxValue()
        {
             DataTable tble = new DataTable();
            DataTableReader read1 = null;
            string removechannel = string.Empty;

            if((PropertySelectedItem!=null&& PropertySelectedItem!=string.Empty)&& (cmb_ChannelSelectedItem != null && cmb_ChannelSelectedItem != string.Empty))
            {
                //if ((PropertySelectedItem.StartsWith("CHANNEL ")))
                //{
                    removechannel = cmb_ChannelSelectedItem + "~" + QscDatabase.removeQATPrefix( PropertySelectedItem);
                //}
                //else if(PropertySelectedItem.StartsWith("OUTPUT "))
                //{
                //    removechannel = cmb_ChannelSelectedItem + "~" + PropertySelectedItem.Remove(0, 7);
                //}
                //else if (PropertySelectedItem.StartsWith("INPUT "))
                //{
                //    removechannel = cmb_ChannelSelectedItem + "~" + PropertySelectedItem.Remove(0, 6);
                //}
                //else if (PropertySelectedItem.StartsWith("INPUT_OUTPUT "))
                //{
                //    removechannel = cmb_ChannelSelectedItem + "~" + PropertySelectedItem.Remove(0, 13);
                //}
                //else if (PropertySelectedItem.StartsWith("TAP "))
                //{
                //    removechannel = cmb_ChannelSelectedItem + "~" + PropertySelectedItem.Remove(0, 4);
                //}
                //else if (PropertySelectedItem.StartsWith("BANK_CONTROL_INPUT "))
                //{
                //    removechannel = cmb_ChannelSelectedItem + "~" + PropertySelectedItem.Remove(0, 19);
                //}
                //else if (PropertySelectedItem.StartsWith("BANK_CONTROL_OUTPUT "))
                //{
                //    removechannel = cmb_ChannelSelectedItem + "~" + PropertySelectedItem.Remove(0, 20);
                //}
                //else if (PropertySelectedItem.StartsWith("BANK_CONTROL_INPUT_OUTPUT "))
                //{
                //    removechannel = cmb_ChannelSelectedItem + "~" + PropertySelectedItem.Remove(0, 26);
                //}
                //else if (PropertySelectedItem.StartsWith("BANK_SELECT_INPUT "))
                //{
                //    removechannel = cmb_ChannelSelectedItem + "~" + PropertySelectedItem.Remove(0, 18);
                //}
                //else if (PropertySelectedItem.StartsWith("BANK_SELECT_OUTPUT "))
                //{
                //    removechannel = cmb_ChannelSelectedItem + "~" + PropertySelectedItem.Remove(0, 19);
                //}
                //else if (PropertySelectedItem.StartsWith("GPIO_INPUT "))
                //{
                //    removechannel = cmb_ChannelSelectedItem + "~" + PropertySelectedItem.Remove(0, 11);
                //}
                //else if (PropertySelectedItem.StartsWith("GPIO_OUTPUT "))
                //{
                //    removechannel = cmb_ChannelSelectedItem + "~" + PropertySelectedItem.Remove(0, 12);
                //}
                
            }
            else if ((PropertySelectedItem != null && PropertySelectedItem != string.Empty))
            {
                removechannel = PropertySelectedItem;
            }
            if (InputSelectionComboSelectedItem!=null&& InputSelectionComboSelectedItem == "Set by value")
            {              
                
                string query = "Select InitialValue from TCInitialization where ComponentName = ('" + ComponentNameSelectedItem + "') and PrettyName = ('" + removechannel + "') and ComponentType = ('" + cmb_ComponentTypeSelectedItem + "') ";
                tble = QscDatabase.SendCommand_Toreceive(query);
                read1 = tble.CreateDataReader();
                while (read1.Read())
                {
                    if(read1[0] != System.DBNull.Value)
                    PropertyInitialValueSelectedItem= read1.GetString(0);
                }
           }

            if (InputSelectionComboSelectedItem != null && InputSelectionComboSelectedItem == "Set by string")
            {
               
                string query = "Select InitialString from TCInitialization where ComponentName = ('" + ComponentNameSelectedItem + "') and PrettyName = ('" + removechannel + "') and ComponentType = ('" + cmb_ComponentTypeSelectedItem + "') ";
                tble = QscDatabase.SendCommand_Toreceive(query);
                read1 = tble.CreateDataReader();
                while (read1.Read())
                {
                    if (read1[0] != System.DBNull.Value)
                        PropertyInitialValueSelectedItem = read1.GetString(0);
                }
            }

            if (InputSelectionComboSelectedItem != null && InputSelectionComboSelectedItem == "Set by position")
            {
                string query = "Select InitialPosition from TCInitialization where ComponentName = ('" + ComponentNameSelectedItem + "') and PrettyName = ('" + removechannel + "') and ComponentType = ('" + cmb_ComponentTypeSelectedItem + "') ";
                tble = QscDatabase.SendCommand_Toreceive(query);
                read1 = tble.CreateDataReader();
                while (read1.Read())
                {
                    if (read1[0] != System.DBNull.Value)
                        PropertyInitialValueSelectedItem = read1.GetString(0);
                }
            }

        }

        private string MaximumLimitValue = string.Empty;
        public string MaximumLimit
        {
            get
            {
                return MaximumLimitValue;
            }
            set
            {
                MaximumLimitValue = value;
                NotifyPropertyChanged("MaximumLimit");
            }
        }

        private bool MaxLimitIsEnabledValue = false;
        public bool MaxLimitIsEnabled
        {
            get
            {
                return MaxLimitIsEnabledValue;
            }
            set
            {
                MaxLimitIsEnabledValue = value;
                NotifyPropertyChanged("MaxLimitIsEnabled");
            }
        }

        private string MinimumLimitValue = string.Empty;
        public string MinimumLimit
        {
            get
            {
                return MinimumLimitValue;
            }
            set
            {
                MinimumLimitValue = value;
                NotifyPropertyChanged("MinimumLimit");
            }
        }

        private bool MinLimitIsEnabledValue = false;
        public bool MinLimitIsEnabled
        {
            get
            {
                return MinLimitIsEnabledValue;
            }
            set
            {
                MinLimitIsEnabledValue = value;
                NotifyPropertyChanged("MinLimitIsEnabled");
            }
        }

        private List<string> cmb_ChannelValue = new List<string>();
        public List<string> cmb_Channel {
            get
            {
                return cmb_ChannelValue;
            }
            set
            {
                cmb_ChannelValue = value;
                NotifyPropertyChanged("cmb_Channel");
            }
        }

        private bool cmb_ChannelEnableValue = false;
        public bool cmb_ChannelEnable
        {
            get
            {
                return cmb_ChannelEnableValue;
            }
            set
            {
                cmb_ChannelEnableValue = value;               
                NotifyPropertyChanged("cmb_ChannelEnable");
            }
        }

        private string cmb_ChannelSelectedItemValue = string.Empty;
        public string cmb_ChannelSelectedItem
        {
            get
            {
                return cmb_ChannelSelectedItemValue;
            }
            set
            {
                cmb_ChannelSelectedItemValue = value;
                if (cmb_ChannelSelectedItemValue != null && cmb_ChannelSelectedItemValue != string.Empty)
                {
                    InputSelectionEnabled = true;
                    chk_LoopEnable = Visibility.Visible;              
                }
                else
                    chk_LoopEnable = Visibility.Hidden;             


                NotifyPropertyChanged("cmb_ChannelSelectedItem");
            }
        }

        private bool PropertyInitialValueSelectedItemEnableValue = false;
        public bool PropertyInitialValueSelectedItemEnable
        {
            get
            {
                return PropertyInitialValueSelectedItemEnableValue;
            }
            set
            {
                PropertyInitialValueSelectedItemEnableValue = value;
                NotifyPropertyChanged("PropertyInitialValueSelectedItemEnable");
            }
        }


        private Visibility chk_LoopEnableValue = Visibility.Hidden;
        public Visibility chk_LoopEnable
        {
            get
            {
                return chk_LoopEnableValue;
            }
            set
            {
                chk_LoopEnableValue = value;
                NotifyPropertyChanged("chk_LoopEnable");
            }
        }

        private bool chk_LoopCheckedValue = false;
        public bool chk_LoopChecked
        {
            get
            {
                return chk_LoopCheckedValue;
            }
            set
            {
                chk_LoopCheckedValue = value;
                if(chk_LoopCheckedValue==true)
                {
                    chk_LoopStartIsEnabled = Visibility.Visible;
                    chk_LoopEndIsEnabled = Visibility.Visible;
                    chk_LoopIncreamentIsEnabled = Visibility.Visible;
                }
                else
                {
                    txtchk_LoopStart = string.Empty;
                    txtchk_LoopEnd = string.Empty;
                    txtchk_LoopIncreament = string.Empty;
                    chk_LoopStartIsEnabled = Visibility.Hidden;
                    chk_LoopEndIsEnabled = Visibility.Hidden;
                    chk_LoopIncreamentIsEnabled = Visibility.Hidden;
                }
                NotifyPropertyChanged("chk_LoopCheckedValue");
            }
        }

        private Visibility chk_LoopStartIsEnabledValue = Visibility.Hidden;
        public Visibility chk_LoopStartIsEnabled
        {
            get
            {
                return chk_LoopStartIsEnabledValue;
            }
            set
            {
                chk_LoopStartIsEnabledValue = value;
                NotifyPropertyChanged("chk_LoopStartIsEnabled");
            }
        }

        private Visibility chk_LoopEndIsEnabledValue = Visibility.Hidden;
        public Visibility chk_LoopEndIsEnabled
        {
            get
            {
                return chk_LoopEndIsEnabledValue;
            }
            set
            {
                chk_LoopEndIsEnabledValue = value;
                NotifyPropertyChanged("chk_LoopEndIsEnabled");
            }
        }

        private Visibility chk_LoopIncreamentIsEnabledValue = Visibility.Hidden;
        public Visibility chk_LoopIncreamentIsEnabled
        {
            get
            {
                return chk_LoopIncreamentIsEnabledValue;
            }
            set
            {
                chk_LoopIncreamentIsEnabledValue = value;
                NotifyPropertyChanged("chk_LoopIncreamentIsEnabled");
            }
        }


        private string txtchk_LoopStartValue = string.Empty;
        public string txtchk_LoopStart
        {
            get
            {
                return txtchk_LoopStartValue;
            }
            set
            {
                txtchk_LoopStartValue = value;
                NotifyPropertyChanged("txtchk_LoopStart");
            }
        }

        private string txtchk_LoopEndValue = string.Empty;
        public string txtchk_LoopEnd
        {
            get
            {
                return txtchk_LoopEndValue;
            }
            set
            {
                txtchk_LoopEndValue = value;
                NotifyPropertyChanged("txtchk_LoopEnd");
            }
        }

        private string txtchk_LoopIncreamentValue = string.Empty;
        public string txtchk_LoopIncreament
        {
            get
            {
                return txtchk_LoopIncreamentValue;
            }
            set
            {
                txtchk_LoopIncreamentValue = value;
                NotifyPropertyChanged("txtchk_LoopIncreament");
            }
        }

        private bool chk_InventoryIlogValue = false;
        public bool chk_InventoryIlog
        {
            get
            {
                return chk_InventoryIlogValue;
            }
            set
            {
                chk_InventoryIlogValue = value;
                NotifyPropertyChanged("chk_InventoryIlog");
            }
        }

        private bool chk_InventoryConfigurationValue = false;
        public bool chk_InventoryConfiguration
        {
            get
            {
                return chk_InventoryConfigurationValue;
            }
            set
            {
                chk_InventoryConfigurationValue = value;
                NotifyPropertyChanged("chk_InventoryConfiguration");
            }
        }

        private bool chk_InventoryEventlogValue = false;
        public bool chk_InventoryEventlog
        {
            get
            {
                return chk_InventoryEventlogValue;
            }
            set
            {
                chk_InventoryEventlogValue = value;
                NotifyPropertyChanged("chk_InventoryEventlog");
            }
        }

        private bool InventoryPauseatErrorstateValue = false;
        public bool InventoryPauseatErrorstate
        {
            get
            {
                return InventoryPauseatErrorstateValue;
            }
            set
            {
                InventoryPauseatErrorstateValue = value;
                NotifyPropertyChanged("InventoryPauseatErrorstate");
            }
        }

        private bool LogPauseatErrorstateValue = false;
        public bool LogPauseatErrorstate
        {
            get
            {
                return LogPauseatErrorstateValue;
            }
            set
            {
                LogPauseatErrorstateValue = value;
                NotifyPropertyChanged("LogPauseatErrorstate");
            }
        }


        private bool InventoryContinueTestingValue = true;
        public bool InventoryContinueTesting
        {
            get
            {
                return InventoryContinueTestingValue;
            }
            set
            {
                InventoryContinueTestingValue = value;
                NotifyPropertyChanged("InventoryContinueTesting");

                if (value == true)
                {
                    InventoryReRunFailedTestCaseEnabled = true;
                }
                else
                {
                    InventoryReRunFailedTestCaseEnabled = false;
                }
            }
        }

        private bool LogContinueTestingValue = true;
        public bool LogContinueTesting
        {
            get
            {
                return LogContinueTestingValue;
            }
            set
            {
                LogContinueTestingValue = value;
                NotifyPropertyChanged("LogContinueTesting");

                if (value == true)
                {
                    LogReRunFailedTestCaseEnabled = true;
                }
                else
                {
                    LogReRunFailedTestCaseEnabled = false;
                }
            }
        }

        private string InventoryReRunFailedTestCaseValue = "0";
        public string InventoryReRunFailedTestCase
        {
            get
            {
                return InventoryReRunFailedTestCaseValue;
            }
            set
            {
                InventoryReRunFailedTestCaseValue = value;
                NotifyPropertyChanged("InventoryReRunFailedTestCase");
            }
        }

        private string LogReRunFailedTestCaseValue = "0";
        public string LogReRunFailedTestCase
        {
            get
            {
                return LogReRunFailedTestCaseValue;
            }
            set
            {
                LogReRunFailedTestCaseValue = value;
                NotifyPropertyChanged("LogReRunFailedTestCase");
            }
        }

        private bool InventoryReRunFailedTestCaseEnabledValue = true;
        public bool InventoryReRunFailedTestCaseEnabled
        {
            get
            {
                return InventoryReRunFailedTestCaseEnabledValue;
            }
            set
            {
                InventoryReRunFailedTestCaseEnabledValue = value;
                NotifyPropertyChanged("InventoryReRunFailedTestCaseEnabled");
            }
        }

        private bool LogReRunFailedTestCaseEnabledValue = true;
        public bool LogReRunFailedTestCaseEnabled
        {
            get
            {
                return LogReRunFailedTestCaseEnabledValue;
            }
            set
            {
                LogReRunFailedTestCaseEnabledValue = value;
                NotifyPropertyChanged("LogReRunFailedTestCaseEnabled");
            }
        }


        private string AllchannelsValue = string.Empty;
        public string Allchannels
        {
            get
            {
                return AllchannelsValue;
            }
            set
            {
                AllchannelsValue = value;
                NotifyPropertyChanged("Allchannels");
            }
        }

        private ObservableCollection<string> TelnetDeviceItemValue = new ObservableCollection<string> ();
        public ObservableCollection<string> TelnetDeviceItem
        {
            get
            {
                return TelnetDeviceItemValue;
            }
            set
            {
                TelnetDeviceItemValue = value;
                NotifyPropertyChanged("TelnetDeviceItem");
            }
        }


        private ObservableCollection<DUT_DeviceItem> TelnetDUTDeviceItemValue = new ObservableCollection<DUT_DeviceItem>();
        public ObservableCollection<DUT_DeviceItem> TelnetDutDeviceItem
        {
            get
            {
                return TelnetDUTDeviceItemValue;
            }
            set
            {
                TelnetDUTDeviceItemValue = value;
                NotifyPropertyChanged("TelnetDutDeviceItem");
            }
        }

        private ObservableCollection<CBMTestTelnetItem> SetTestTelnetListValue = new ObservableCollection<CBMTestTelnetItem> ();
        public ObservableCollection<CBMTestTelnetItem> SetTestTelnetList
        {
            get
            {
                return SetTestTelnetListValue;
            }
            set
            {               
                    SetTestTelnetListValue = value;

                NotifyPropertyChanged("SetTestTelnetList");
            }
        }       
     
        public void RemoveSetTestTelnetItem(CBMTestTelnetItem removeItem)
        {
            try
            {
                if (SetTestTelnetList.Contains(removeItem))
                    SetTestTelnetList.Remove(removeItem);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Exception\n \n" + ex.Message, "QAT Error Code - ECxxxx", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private CBMTestTelnetItem parentCBMTestTelnetItemValue = null;
        public CBMTestTelnetItem ParentCBMTestTelnetItem
        {
            get { return parentCBMTestTelnetItemValue; }
            set { parentCBMTestTelnetItemValue = value; NotifyPropertyChanged("ParentCBMTestTelnetItem"); }
        }

        private ObservableCollection<CBMTestLogItem> verifyTestLogListValue = new ObservableCollection<CBMTestLogItem> ();
        public ObservableCollection<CBMTestLogItem> VerifyTestLogList
        {
            get { return verifyTestLogListValue; }
            set { verifyTestLogListValue = value; NotifyPropertyChanged("VerifyTestLogList"); }
        }

        private CBMTestLogItem ParentCBMTestLogItemValue = null;
        public CBMTestLogItem ParentCBMTestLogItem
        {
            get { return ParentCBMTestLogItemValue; }
            set { ParentCBMTestLogItemValue = value; NotifyPropertyChanged("ParentCBMTestLogItem"); }
        }


        private ObservableCollection<CBMTestControlItem> VerifyTestControlListValue = new ObservableCollection<CBMTestControlItem> ();
        public ObservableCollection<CBMTestControlItem> VerifyTestControlList
        {
            get { return VerifyTestControlListValue; }
            set { VerifyTestControlListValue = value; NotifyPropertyChanged("VerifyTestControlList"); }
        }

        #region "InotifyPropertyChanged"

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(string property)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(property));
            }


        }
        #endregion
    } 

    public class CBMTestTelnetItem : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        void OnPropertyChanged(string property)
        {
            try
            {
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs(property));
                   
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Exception\n " + ex.Message, "Error Code - ECxxxxx", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

    
        private string TelnetCommandTextValue = null;
        public string TelnetCommandText
        {
            get { return TelnetCommandTextValue; }
            set { TelnetCommandTextValue = value; OnPropertyChanged("TelnetCommandText"); }
        }

        //private ObservableCollection<DUT_DeviceItem> telnetDeviceItemValue = new ObservableCollection<DUT_DeviceItem>();
        //public ObservableCollection<DUT_DeviceItem> TelnetDeviceItem
        //{
        //    get { return telnetDeviceItemValue; }
        //    set { telnetDeviceItemValue = value; OnPropertyChanged("TelnetDeviceItem"); }
        //}

        private string telnetVerifyTypeSelectedValue = "Compare Values";
        public string TelnetVerifyTypeSelected
        {
            get { return telnetVerifyTypeSelectedValue; }
            set
            {
                telnetVerifyTypeSelectedValue = value;
                if (value == "Compare Values")
                    TelnetFailureTextIsEnabled = true;
                else
                {
                    TelnetFailureTextIsEnabled = false;
                    TelnetFailureText = null;
                }

                OnPropertyChanged("TelnetVerifyTypeSelected");
            }
        }

        private ObservableCollection<string> telnetVerifyTypeListValue = new ObservableCollection<string> { "Continue Without Doing Anything", "Store Current Result", "Compare Values" };
        public ObservableCollection<string> TelnetVerifyTypeList
        {
            get { return telnetVerifyTypeListValue; }
            set { telnetVerifyTypeListValue = value; OnPropertyChanged("TelnetVerifyTypeList"); }
        }

        private bool telnetFailureTextIsEnabledValue = true;
        public bool TelnetFailureTextIsEnabled
        {
            get { return telnetFailureTextIsEnabledValue; }
            set { telnetFailureTextIsEnabledValue = value; OnPropertyChanged("TelnetFailureTextIsEnabled"); }
        }

        private string telnetFailureTextValue = null;
        public string TelnetFailureText
        {
            get { return telnetFailureTextValue; }
            set { telnetFailureTextValue = value; OnPropertyChanged("TelnetFailureText"); }
        }

        private ObservableCollection<DUT_DeviceItem> CBMValues = new ObservableCollection<DUT_DeviceItem>();
        public ObservableCollection<DUT_DeviceItem> CBM
        {
            get { return CBMValues; }
            set
            {
                CBMValues = value;                
                OnPropertyChanged("CBM");
            }
        }

        private ObservableCollection<string> CBMComboValues = new ObservableCollection<string>();
        public ObservableCollection<string> CBMCombo
        {
            get { return CBMComboValues; }
            set
            {
                CBMComboValues = value;
                OnPropertyChanged("CBMCombo");
            }
        }

        private Dictionary<string, string> CBMComboModelValues = new Dictionary<string, string>();
        public Dictionary<string, string> CBMComboModel
        {
            get { return CBMComboModelValues; }
            set
            {
                CBMComboModelValues = value;
                OnPropertyChanged("CBMComboModel");
            }
        }

        private string telnetSelectedDeviceValue = null;
        public string TelnetSelectedDevice
        {
            get { return telnetSelectedDeviceValue; }
            set
            {
                telnetSelectedDeviceValue = value;
                OnPropertyChanged("TelnetSelectedDevice");
            }
        }
    }

    public class CBMTestLogItem : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        void OnPropertyChanged(string property)
        {
            try
            {
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs(property));

                    //if (ParentTestActionItem != null && ParentTestActionItem.ParentTestCaseItem != null && ParentTestActionItem.ParentTestCaseItem.SaveButtonIsEnabled != true)
                    //    ParentTestActionItem.ParentTestCaseItem.SaveButtonIsEnabled = true;
                }
            }
            catch (Exception ex)
            {
                //MessageBox.Show("Exception\n " + ex.Message, "Error Code - ECxxxxx", MessageBoxButton.OK, MessageBoxImage.Error);
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif

            }
        }
        
        /////  Itemsource
        private ObservableCollection<string> Log_verification_kernellog_value = new ObservableCollection<string>();
        public ObservableCollection<string> Log_verification_kernellog
        {
            get { return Log_verification_kernellog_value; }
            set { Log_verification_kernellog_value = value; OnPropertyChanged("Log_verification_kernellog"); }
        }

        private bool iLogIsCheckedValue = false;
        public bool iLogIsChecked
        {
            get { return iLogIsCheckedValue; }
            set
            {
                iLogIsCheckedValue = value;

                if (value == true)
                {
                    iLog_combobox_enable = true;
                    ilogtext_enable = true;
                }
                else
                {
                    iLog_combobox_enable = false;
                    ilogtext_enable = false;
                }
                OnPropertyChanged("iLogIsChecked");
            }
        }

        private bool ConfiguratorIsCheckedValue = false;
        public bool ConfiguratorIsChecked
        {
            get { return ConfiguratorIsCheckedValue; }
            set
            {
                ConfiguratorIsCheckedValue = value;
                if (value == true)
                {
                    configuratorlogtext_enable = true;
                }
                else
                {
                    configuratorlogtext_enable = false;

                }
                OnPropertyChanged("ConfiguratorIsChecked");
            }
        }

        private bool EventLogIsCheckedValue = false;
        public bool EventLogIsChecked
        {
            get { return EventLogIsCheckedValue; }
            set
            {
                EventLogIsCheckedValue = value;
                if (value == true)
                {
                    eventlogtext_enable = true;
                }
                else
                {
                    eventlogtext_enable = false;

                }
                OnPropertyChanged("EventLogIsChecked");
            }
        }

        private bool SIPLogIsCheckedValue = false;
        public bool SIPLogIsChecked
        {
            get { return SIPLogIsCheckedValue; }
            set
            {
                SIPLogIsCheckedValue = value;
                if (value == true)
                {
                    siplogtext_enable = true;
                }
                else
                {
                    siplogtext_enable = false;

                }
                OnPropertyChanged("SIPLogIsChecked");
            }
        }

        private bool QsysAppLogIsCheckedValue = false;
        public bool QsysAppLogIsChecked
        {
            get { return QsysAppLogIsCheckedValue; }
            set
            {
                QsysAppLogIsCheckedValue = value;
                if (value == true)
                {
                    qsysapplogtext_enable = true;
                }
                else
                {
                    qsysapplogtext_enable = false;

                }
                OnPropertyChanged("QsysAppLogIsChecked");
            }
        }

        private bool SoftPhoneLogIsCheckedValue = false;
        public bool SoftPhoneLogIsChecked
        {
            get { return SoftPhoneLogIsCheckedValue; }
            set
            {
                SoftPhoneLogIsCheckedValue = value;
                if (value == true)
                {
                    softphonelogtext_enable = true;
                }
                else
                {
                    softphonelogtext_enable = false;

                }
                OnPropertyChanged("SoftPhoneLogIsChecked");
            }
        }

        private bool UCIViewerLogIsCheckedValue = false;
        public bool UCIViewerLogIsChecked
        {
            get { return UCIViewerLogIsCheckedValue; }
            set
            {
                UCIViewerLogIsCheckedValue = value;
                if (value == true)
                {
                    UCIlogtext_enable = true;
                }
                else
                {
                    UCIlogtext_enable = false;

                }
                OnPropertyChanged("UCIViewerLogIsChecked");
            }
        }

        private bool KernelLogIsCheckedValue = false;
        public bool KernelLogIsChecked
        {
            get { return KernelLogIsCheckedValue; }
            set
            {
                KernelLogIsCheckedValue = value;

                if (value == true)
                {
                    kernelLog_combobox_enable = true;
                    kernallogtext_enable = true;
                }
                else
                {
                    kernelLog_combobox_enable = false;
                    kernallogtext_enable = false;
                }
                OnPropertyChanged("KernelLogIsChecked");
            }
        }

        private bool WindowsEventLogsIsCheckedValue = false;
        public bool WindowsEventLogsIsChecked
        {
            get { return WindowsEventLogsIsCheckedValue; }
            set
            {
                WindowsEventLogsIsCheckedValue = value;
                if (value == true)
                {
                    windowseventlogtext_enable = true;
                }
                else
                {
                    windowseventlogtext_enable = false;

                }
                OnPropertyChanged("WindowsEventLogsIsChecked");
            }
        }

        private bool iLog_combobox_enableValue = false;
        public bool iLog_combobox_enable
        {
            get { return iLog_combobox_enableValue; }
            set
            {
                iLog_combobox_enableValue = value;
                OnPropertyChanged("iLog_combobox_enable");
            }
        }

        private string iLog_selected_itemValue = null;
        public string iLog_selected_item
        {
            get { return iLog_selected_itemValue; }
            set
            {
                iLog_selected_itemValue = value;
                OnPropertyChanged("iLog_selected_item");
            }
        }

        private string kernalLog_selected_itemValue = null;
        public string kernalLog_selected_item
        {
            get { return kernalLog_selected_itemValue; }
            set
            {
                kernalLog_selected_itemValue = value;
                OnPropertyChanged("kernalLog_selected_item");
            }
        }

        private bool kernelLog_combobox_enableValue = false;
        public bool kernelLog_combobox_enable
        {
            get { return kernelLog_combobox_enableValue; }
            set
            {
                kernelLog_combobox_enableValue = value;
                OnPropertyChanged("kernelLog_combobox_enable");
            }
        }
        /// <summary>
        /// / text Property definition for all text boxes
        /// </summary>
        private string ilogtextValue = string.Empty;
        public string ilogtext
        {
            get { return ilogtextValue.Trim(); }
            set
            {
                ilogtextValue = value;
                OnPropertyChanged("ilogtext");
            }
        }
        private string configuratorlogtextValue = string.Empty;
        public string configuratorlogtext
        {
            get { return configuratorlogtextValue.Trim(); }
            set
            {
                configuratorlogtextValue = value;
                OnPropertyChanged("configuratorlogtext");
            }
        }
        private string eventlogtextValue = string.Empty;
        public string eventlogtext
        {
            get { return eventlogtextValue.Trim(); }
            set
            {
                eventlogtextValue = value;
                OnPropertyChanged("eventlogtext");
            }
        }
        private string siplogtextValue = string.Empty;
        public string siplogtext
        {
            get { return siplogtextValue.Trim(); }
            set
            {
                siplogtextValue = value;
                OnPropertyChanged("siplogtext");
            }
        }
        private string qsysapplogtextValue = string.Empty;
        public string qsysapplogtext
        {
            get { return qsysapplogtextValue.Trim(); }
            set
            {
                qsysapplogtextValue = value;
                OnPropertyChanged("qsysapplogtext");
            }
        }
        private string softphonelogtextValue = string.Empty;
        public string softphonelogtext
        {
            get { return softphonelogtextValue.Trim(); }
            set
            {
                softphonelogtextValue = value;
                OnPropertyChanged("softphonelogtext");
            }
        }
        private string UCIlogtextValue = string.Empty;
        public string UCIlogtext
        {
            get { return UCIlogtextValue.Trim(); }
            set
            {
                UCIlogtextValue = value;
                OnPropertyChanged("UCIlogtext");
            }
        }
        private string kernallogtextValue = string.Empty;
        public string kernallogtext
        {
            get { return kernallogtextValue.Trim(); }
            set
            {
                kernallogtextValue = value;
                OnPropertyChanged("kernallogtext");
            }
        }
        private string windowseventlogtextValue = string.Empty;
        public string windowseventlogtext
        {
            get { return windowseventlogtextValue.Trim(); }
            set
            {
                windowseventlogtextValue = value;
                OnPropertyChanged("windowseventlogtext");
            }
        }
        /// <summary>
        /// /Isenabled Property definition for all text boxes
        /// </summary>

        private bool ilogtext_enableValue = false;
        public bool ilogtext_enable
        {
            get { return ilogtext_enableValue; }
            set
            {
                ilogtext_enableValue = value;
                OnPropertyChanged("ilogtext_enable");
            }
        }

        private bool configuratorlogtext_enableValue = false;
        public bool configuratorlogtext_enable
        {
            get { return configuratorlogtext_enableValue; }
            set
            {
                configuratorlogtext_enableValue = value;
                OnPropertyChanged("configuratorlogtext_enable");
            }
        }

        private bool eventlogtext_enableValue = false;
        public bool eventlogtext_enable
        {
            get { return eventlogtext_enableValue; }
            set
            {
                eventlogtext_enableValue = value;
                OnPropertyChanged("eventlogtext_enable");
            }
        }

        private bool siplogtext_enableValue = false;
        public bool siplogtext_enable
        {
            get { return siplogtext_enableValue; }
            set
            {
                siplogtext_enableValue = value;
                OnPropertyChanged("siplogtext_enable");
            }
        }

        private bool qsysapplogtext_enableValue = false;
        public bool qsysapplogtext_enable
        {
            get { return qsysapplogtext_enableValue; }
            set
            {
                qsysapplogtext_enableValue = value;
                OnPropertyChanged("qsysapplogtext_enable");
            }
        }

        private bool softphonelogtext_enableValue = false;
        public bool softphonelogtext_enable
        {
            get { return softphonelogtext_enableValue; }
            set
            {
                softphonelogtext_enableValue = value;
                OnPropertyChanged("softphonelogtext_enable");
            }
        }

        private bool UCIlogtext_enableValue = false;
        public bool UCIlogtext_enable
        {
            get { return UCIlogtext_enableValue; }
            set
            {
                UCIlogtext_enableValue = value;
                OnPropertyChanged("UCIlogtext_enable");
            }
        }

        private bool kernallogtext_enableValue = false;
        public bool kernallogtext_enable
        {
            get { return kernallogtext_enableValue; }
            set
            {
                kernallogtext_enableValue = value;
                OnPropertyChanged("kernallogtext_enable");
            }
        }

        private bool windowseventlogtext_enableValue = false;
        public bool windowseventlogtext_enable
        {
            get { return windowseventlogtext_enableValue; }
            set
            {
                windowseventlogtext_enableValue = value;
                OnPropertyChanged("windowseventlogtext_enable");
            }
        }


    }


    public class CBMTestControlItem : INotifyPropertyChanged, IDataErrorInfo
    {

        public event PropertyChangedEventHandler PropertyChanged;
        void OnPropertyChanged(string property)
        {
            try
            {
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs(property));                   
                }
            }
            catch (Exception ex)
            {
                //MessageBox.Show("Exception\n " + ex.Message, "Error Code - ECxxxxx", MessageBoxButton.OK, MessageBoxImage.Error);
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif

            }
        }

        private CBMItems parentTestActionItemValue=null;
        public CBMItems ParentTestActionItem
        {
            get { return parentTestActionItemValue; }
            set { parentTestActionItemValue = value; OnPropertyChanged("ParentTestActionItem"); }
        }

        private ObservableCollection<string> testControlComponentTypeListValue = new ObservableCollection<string>();
        public ObservableCollection<string> TestControlComponentTypeList
        {
            get { return testControlComponentTypeListValue; }
            set
            {
                try
                {


                    testControlComponentTypeListValue = value;

                    string[] alphaNumericSortedComponentType = testControlComponentTypeListValue.ToArray();
                    Array.Sort(alphaNumericSortedComponentType, new AlphanumComparatorFaster());
                    testControlComponentTypeListValue = new ObservableCollection<string>(alphaNumericSortedComponentType.ToList());


                    //testControlComponentTypeListValue = new ObservableCollection<string>(testControlComponentTypeListValue.OrderBy(a => a));
                    OnPropertyChanged("TestControlComponentTypeList");

                    if (value == null || !value.Contains(TestControlComponentTypeSelectedItem))
                    {
                        TestControlComponentTypeSelectedItem = null;
                        TestControlComponentNameSelectedItem = null;
                        TestControlPropertySelectedItem = null;
                        ChannelSelectionSelectedItem = string.Empty;
                        InputSelectionComboSelectedItem = null;
                        TestControlPropertyInitialValueSelectedItem = null;

                    }
                    else
                    {
                        if (TestControlComponentTypeSelectedItem != null)
                        {
                            string[] alphaNumericSortedComponentName = ParentTestActionItem.ComponentNameDictionary[TestControlComponentTypeSelectedItem].ToArray();
                            Array.Sort(alphaNumericSortedComponentName, new AlphanumComparatorFaster());
                            ObservableCollection<string> ComponentNameList = new ObservableCollection<string>(alphaNumericSortedComponentName.ToList());
                            //ObservableCollection<string> ComponentNameList = new ObservableCollection<string>(ParentTestActionItem.ParentTestCaseItem.ComponentNameDictionary[TestControlComponentTypeSelectedItem].OrderBy(a => a));
                            TestControlComponentNameList = ComponentNameList;
                            //TestControlComponentNameList = ParentTestActionItem.ParentTestCaseItem.ComponentNameDictionary[TestControlComponentTypeSelectedItem];//added like this upto ver 1.19

                        }


                        else
                            TestControlComponentNameList = null;

                    }
                }
                catch (Exception ex)
                {
                    DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif

                    //MessageBox.Show("Exception\n " + ex.Message, "Error Code - ECxxxxx", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private string testControlComponentTypeSelectedItemValue = null;
        public string TestControlComponentTypeSelectedItem
        {
            get { return testControlComponentTypeSelectedItemValue; }
            set
            {
                try
                {
                    testControlComponentTypeSelectedItemValue = value;

                    if (value != null)
                    {
                        string[] alphaNumericSortedComponentName = ParentTestActionItem.ComponentNameDictionary[TestControlComponentTypeSelectedItem].ToArray();
                        Array.Sort(alphaNumericSortedComponentName, new AlphanumComparatorFaster());
                        ObservableCollection<string> ComponentNameList = new ObservableCollection<string>(alphaNumericSortedComponentName.ToList());
                        //ObservableCollection<string> ComponentNameList = new ObservableCollection<string>(ParentTestActionItem.ParentTestCaseItem.ComponentNameDictionary[TestControlComponentTypeSelectedItem].OrderBy(a => a));
                        TestControlComponentNameList = ComponentNameList;
                        //TestControlPropertyInitialValueSelectedItem = null;
                    }
                    else
                    {
                        TestControlComponentNameList = null;
                    }

                    OnPropertyChanged("TestControlComponentTypeSelectedItem");
                }
                catch (Exception ex)
                {
                    //MessageBox.Show("Exception\n " + ex.Message, "Error Code - ECxxxxx", MessageBoxButton.OK, MessageBoxImage.Error);
                    DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif

                }
            }
        }

        private ObservableCollection<string> testControlComponentNameListValue = new ObservableCollection<string>();
        public ObservableCollection<string> TestControlComponentNameList
        {
            get { return testControlComponentNameListValue; }
            set
            {
                testControlComponentNameListValue = value;

                if (value == null || !value.Contains(TestControlComponentNameSelectedItem))
                    TestControlComponentNameSelectedItem = null;

                OnPropertyChanged("TestControlComponentNameList");
            }
        }

        private string testControlComponentNameSelectedItemValue = null;
        public string TestControlComponentNameSelectedItem
        {
            get { return testControlComponentNameSelectedItemValue; }
            set
            {
                try
                {
                    if (value == testControlComponentNameSelectedItemValue)
                    {
                        OnPropertyChanged("TestControlComponentNameSelectedItem");
                        return;
                    }

                    testControlComponentNameSelectedItemValue = value;
                    if (value != null)
                    {

                        string[] alphaNumericSortedComponentName = ParentTestActionItem.ControlNameDictionary[value].ToArray();
                        Array.Sort(alphaNumericSortedComponentName, new AlphanumComparatorFaster());
                        //ObservableCollection<string> ComponentcontrolList = new ObservableCollection<string>(ParentTestActionItem.ParentTestCaseItem.ControlNameDictionary[value].OrderBy(a => a));
                        ObservableCollection<string> ComponentcontrolList = new ObservableCollection<string>(alphaNumericSortedComponentName.ToList());
                        TestControlPropertyList = ComponentcontrolList;


                        string[] alphaNumericSortedControlName = ParentTestActionItem.VerifyControlNameDictionary[value].ToArray();
                        Array.Sort(alphaNumericSortedControlName, new AlphanumComparatorFaster());
                        ObservableCollection<string> ComponentverifycontrolList = new ObservableCollection<string>(alphaNumericSortedControlName.ToList());
                        //ObservableCollection<string> ComponentverifycontrolList = new ObservableCollection<string>(ParentTestActionItem.ParentTestCaseItem.VerifyControlNameDictionary[value].OrderBy(a => a));
                        VerifyTestControlPropertyList = ComponentverifycontrolList;

                        //if (ParentTestActionItem.ChannelSelectionDictionary.Count > 0)
                        //{
                        //    string[] alphaNumericSortedChannelName = ParentTestActionItem.ChannelSelectionDictionary[value].ToArray();
                        //    Array.Sort(alphaNumericSortedChannelName, new AlphanumComparatorFaster());
                        //    ObservableCollection<string> ComponentchannelList = new ObservableCollection<string>(alphaNumericSortedChannelName.ToList());
                        //    //ObservableCollection<string> ComponentchannelList = new ObservableCollection<string>(ParentTestActionItem.ParentTestCaseItem.ChannelSelectionDictionary[value].OrderBy(a => a));
                        //    ChannelSelectionList = ComponentchannelList;
                        //    ChannelSelectionList = ComponentchannelList;
                        //    channelInputList.Clear();
                        //    channelOutputList.Clear();
                        //    channelInputOutputList.Clear();
                        //    channelBankControlInputList.Clear();
                        //    channelBankControlOutputList.Clear();
                        //    channelBankControlInputOutputList.Clear();
                        //    channelBankSelectInputList.Clear();
                        //    channelBankSelectOutputList.Clear();
                        //    channelGPIOInputList.Clear();
                        //    channelGPIOOutputList.Clear();
                        //    foreach (string channelstring in ChannelSelectionList)
                        //    {

                        //        if (channelstring != null)
                        //        {
                        //            if ((channelstring.Contains("Input")) && (!(channelstring.Contains("Output"))) && (!(channelstring.Contains("Bank Control"))) && (!(channelstring.Contains("Bank Select"))) && (!(channelstring.Contains("GPIO"))))
                        //                channelInputList.Add(channelstring);
                        //            else if ((!(channelstring.Contains("Input"))) && ((channelstring.Contains("Output"))) && (!(channelstring.Contains("Bank Control"))) && (!(channelstring.Contains("Bank Select"))) && (!(channelstring.Contains("GPIO"))))
                        //                channelOutputList.Add(channelstring);
                        //            else if (((channelstring.Contains("Input"))) && ((channelstring.Contains("Output"))) && (!(channelstring.Contains("Bank Control"))) && (!(channelstring.Contains("Bank Select"))) && (!(channelstring.Contains("GPIO"))))
                        //                channelInputOutputList.Add(channelstring);
                        //            else if (((channelstring.Contains("Input"))) && (!(channelstring.Contains("Output"))) && ((channelstring.Contains("Bank Control"))) && (!(channelstring.Contains("Bank Select"))) && (!(channelstring.Contains("GPIO"))))
                        //                channelBankControlInputList.Add(channelstring);
                        //            else if ((!(channelstring.Contains("Input"))) && ((channelstring.Contains("Output"))) && ((channelstring.Contains("Bank Control"))) && (!(channelstring.Contains("Bank Select"))) && (!(channelstring.Contains("GPIO"))))
                        //                channelBankControlOutputList.Add(channelstring);
                        //            else if (((channelstring.Contains("Input"))) && ((channelstring.Contains("Output"))) && ((channelstring.Contains("Bank Control"))) && (!(channelstring.Contains("Bank Select"))) && (!(channelstring.Contains("GPIO"))))
                        //                channelBankControlInputOutputList.Add(channelstring);
                        //            else if (((channelstring.Contains("Input"))) && (!(channelstring.Contains("Output"))) && (!(channelstring.Contains("Bank Control"))) && ((channelstring.Contains("Bank Select"))) && (!(channelstring.Contains("GPIO"))))
                        //                channelBankSelectInputList.Add(channelstring);
                        //            else if ((!(channelstring.Contains("Input"))) && ((channelstring.Contains("Output"))) && (!(channelstring.Contains("Bank Control"))) && ((channelstring.Contains("Bank Select"))) && (!(channelstring.Contains("GPIO"))))
                        //                channelBankSelectOutputList.Add(channelstring);
                        //            //else if (((channelstring.Contains("Input"))) && (!(channelstring.Contains("Output"))) && (!(channelstring.Contains("Bank Control"))) && (!(channelstring.Contains("Bank Select"))) && ((channelstring.Contains("GPIO"))))
                        //            //    channelGPIOInputList.Add(channelstring);
                        //            //else if ((!(channelstring.Contains("Input"))) && ((channelstring.Contains("Output"))) && (!(channelstring.Contains("Bank Control"))) && (!(channelstring.Contains("Bank Select"))) && ((channelstring.Contains("GPIO"))))
                        //            //    channelGPIOOutputList.Add(channelstring);
                        //        }
                        //    }
                        //    string[] alphaNumericSortedInputChannelName = channelInputList.ToArray();
                        //    Array.Sort(alphaNumericSortedInputChannelName, new AlphanumComparatorFaster());
                        //    ObservableCollection<string> ComponentInputchannelList = new ObservableCollection<string>(alphaNumericSortedInputChannelName.ToList());
                        //    channelInputList = ComponentInputchannelList;

                        //    string[] alphaNumericSortedOutputChannelName = channelOutputList.ToArray();
                        //    Array.Sort(alphaNumericSortedOutputChannelName, new AlphanumComparatorFaster());
                        //    ObservableCollection<string> ComponentOutputchannelList = new ObservableCollection<string>(alphaNumericSortedOutputChannelName.ToList());
                        //    channelOutputList = ComponentOutputchannelList;

                        //    string[] alphaNumericSortedInputOutputChannelName = channelInputOutputList.ToArray();
                        //    Array.Sort(alphaNumericSortedInputOutputChannelName, new AlphanumComparatorFaster());
                        //    ObservableCollection<string> ComponentInputOutputchannelList = new ObservableCollection<string>(alphaNumericSortedInputOutputChannelName.ToList());
                        //    channelInputOutputList = ComponentInputOutputchannelList;

                        //    string[] alphaNumericSortedBankControlInputChannelName = channelBankControlInputList.ToArray();
                        //    Array.Sort(alphaNumericSortedBankControlInputChannelName, new AlphanumComparatorFaster());
                        //    ObservableCollection<string> ComponentBankControlInputchannelList = new ObservableCollection<string>(alphaNumericSortedBankControlInputChannelName.ToList());
                        //    channelBankControlInputList = ComponentBankControlInputchannelList;

                        //    string[] alphaNumericSortedBankControloutputChannelName = channelBankControlOutputList.ToArray();
                        //    Array.Sort(alphaNumericSortedBankControloutputChannelName, new AlphanumComparatorFaster());
                        //    ObservableCollection<string> ComponentBankControlOutputchannelList = new ObservableCollection<string>(alphaNumericSortedBankControloutputChannelName.ToList());
                        //    channelBankControlOutputList = ComponentBankControlOutputchannelList;

                        //    string[] alphaNumericSortedBankControlinputoutputChannelName = channelBankControlInputOutputList.ToArray();
                        //    Array.Sort(alphaNumericSortedBankControlinputoutputChannelName, new AlphanumComparatorFaster());
                        //    ObservableCollection<string> ComponentBankControlinputOutputchannelList = new ObservableCollection<string>(alphaNumericSortedBankControlinputoutputChannelName.ToList());
                        //    channelBankControlInputOutputList = ComponentBankControlinputOutputchannelList;

                        //    string[] alphaNumericSortedBankSelectInputChannelName = channelBankSelectInputList.ToArray();
                        //    Array.Sort(alphaNumericSortedBankSelectInputChannelName, new AlphanumComparatorFaster());
                        //    ObservableCollection<string> ComponentBankSelectInputchannelList = new ObservableCollection<string>(alphaNumericSortedBankSelectInputChannelName.ToList());
                        //    channelBankSelectInputList = ComponentBankSelectInputchannelList;

                        //    string[] alphaNumericSortedBankSelectoutputChannelName = channelBankSelectOutputList.ToArray();
                        //    Array.Sort(alphaNumericSortedBankSelectoutputChannelName, new AlphanumComparatorFaster());
                        //    ObservableCollection<string> ComponentBankSelectOutputchannelList = new ObservableCollection<string>(alphaNumericSortedBankSelectoutputChannelName.ToList());
                        //    channelBankSelectOutputList = ComponentBankSelectOutputchannelList;

                        //    //string[] alphaNumericSortedGPIOoutputChannelName = channelGPIOOutputList.ToArray();
                        //    //Array.Sort(alphaNumericSortedGPIOoutputChannelName, new AlphanumComparatorFaster());
                        //    //ObservableCollection<string> ComponentGPIOOutputchannelList = new ObservableCollection<string>(alphaNumericSortedGPIOoutputChannelName.ToList());
                        //    //channelGPIOOutputList = ComponentGPIOOutputchannelList;

                        //    //string[] alphaNumericSortedGPIOinputChannelName = channelGPIOInputList.ToArray();
                        //    //Array.Sort(alphaNumericSortedGPIOinputChannelName, new AlphanumComparatorFaster());
                        //    //ObservableCollection<string> ComponentGPIOInputchannelList = new ObservableCollection<string>(alphaNumericSortedGPIOinputChannelName.ToList());
                        //    //channelGPIOInputList = ComponentGPIOInputchannelList;
                        //}

                        //added like this upto ver 1.19
                        //TestControlPropertyList = ParentTestActionItem.ParentTestCaseItem.ControlNameDictionary[value];
                        //VerifyTestControlPropertyList = ParentTestActionItem.ParentTestCaseItem.VerifyControlNameDictionary[value];
                        //if (ParentTestActionItem.ParentTestCaseItem.ChannelSelectionDictionary.Count>0)
                        //ChannelSelectionList = ParentTestActionItem.ParentTestCaseItem.ChannelSelectionDictionary[value];

                    }
                    else
                    {
                        if (TestControlComponentNameList != null && TestControlComponentNameList.Count > 0)
                        {
                            ObservableCollection<string> ComponentcontrolList = new ObservableCollection<string>(ParentTestActionItem.ControlNameDictionary[TestControlComponentNameList[0]].OrderBy(a => a));
                            TestControlPropertyList = ComponentcontrolList;

                            ObservableCollection<string> ComponentverifycontrolList = new ObservableCollection<string>(ParentTestActionItem.VerifyControlNameDictionary[TestControlComponentNameList[0]].OrderBy(a => a));
                            VerifyTestControlPropertyList = ComponentverifycontrolList;

                            //if (ParentTestActionItem.ChannelSelectionDictionary.Count > 0)
                            //{
                            //    ObservableCollection<string> ComponentchannelList = new ObservableCollection<string>(ParentTestActionItem.ChannelSelectionDictionary[TestControlComponentNameList[0]].OrderBy(a => a));
                            //    ChannelSelectionList = ComponentchannelList;
                            //    ChannelSelectionList = ComponentchannelList;
                            //    channelInputList.Clear();
                            //    channelOutputList.Clear();
                            //    channelInputOutputList.Clear();
                            //    channelBankControlInputList.Clear();
                            //    channelBankControlOutputList.Clear();
                            //    channelBankControlInputOutputList.Clear();
                            //    channelBankSelectInputList.Clear();
                            //    channelBankSelectOutputList.Clear();
                            //    channelGPIOInputList.Clear();
                            //    channelGPIOOutputList.Clear();
                            //    foreach (string channelstring in ChannelSelectionList)
                            //    {

                            //        if (channelstring != null)
                            //        {
                            //            if ((channelstring.Contains("Input")) && (!(channelstring.Contains("Output"))) && (!(channelstring.Contains("Bank Control"))) && (!(channelstring.Contains("Bank Select"))) && (!(channelstring.Contains("GPIO"))))
                            //                channelInputList.Add(channelstring);
                            //            else if ((!(channelstring.Contains("Input"))) && ((channelstring.Contains("Output"))) && (!(channelstring.Contains("Bank Control"))) && (!(channelstring.Contains("Bank Select"))) && (!(channelstring.Contains("GPIO"))))
                            //                channelOutputList.Add(channelstring);
                            //            else if (((channelstring.Contains("Input"))) && ((channelstring.Contains("Output"))) && (!(channelstring.Contains("Bank Control")))&& (!(channelstring.Contains("Bank Select"))) && (!(channelstring.Contains("GPIO"))))
                            //                channelInputOutputList.Add(channelstring);
                            //            else if (((channelstring.Contains("Input"))) && (!(channelstring.Contains("Output"))) && ((channelstring.Contains("Bank Control"))) && (!(channelstring.Contains("Bank Select"))) && (!(channelstring.Contains("GPIO"))))
                            //                channelBankControlInputList.Add(channelstring);
                            //            else if ((!(channelstring.Contains("Input"))) && ((channelstring.Contains("Output"))) && ((channelstring.Contains("Bank Control"))) && (!(channelstring.Contains("Bank Select"))) && (!(channelstring.Contains("GPIO"))))
                            //                channelBankControlOutputList.Add(channelstring);
                            //            else if (((channelstring.Contains("Input"))) && ((channelstring.Contains("Output"))) && ((channelstring.Contains("Bank Control"))) && (!(channelstring.Contains("Bank Select"))) && (!(channelstring.Contains("GPIO"))))
                            //                channelBankControlInputOutputList.Add(channelstring);
                            //            else if (((channelstring.Contains("Input"))) && (!(channelstring.Contains("Output"))) && (!(channelstring.Contains("Bank Control"))) && ((channelstring.Contains("Bank Select"))) && (!(channelstring.Contains("GPIO"))))
                            //                channelBankSelectInputList.Add(channelstring);
                            //            else if ((!(channelstring.Contains("Input"))) && ((channelstring.Contains("Output"))) && (!(channelstring.Contains("Bank Control"))) && ((channelstring.Contains("Bank Select"))) && (!(channelstring.Contains("GPIO"))))
                            //                channelBankSelectOutputList.Add(channelstring);
                            //            //else if (((channelstring.Contains("Input"))) && (!(channelstring.Contains("Output"))) && (!(channelstring.Contains("Bank Control"))) && (!(channelstring.Contains("Bank Select"))) && ((channelstring.Contains("GPIO"))))
                            //            //    channelGPIOInputList.Add(channelstring);
                            //            //else if ((!(channelstring.Contains("Input"))) && ((channelstring.Contains("Output"))) && (!(channelstring.Contains("Bank Control"))) && (!(channelstring.Contains("Bank Select"))) && ((channelstring.Contains("GPIO"))))
                            //            //    channelGPIOOutputList.Add(channelstring);
                            //        }
                            //    }
                            //    string[] alphaNumericSortedInputChannelName = channelInputList.ToArray();
                            //    Array.Sort(alphaNumericSortedInputChannelName, new AlphanumComparatorFaster());
                            //    ObservableCollection<string> ComponentInputchannelList = new ObservableCollection<string>(alphaNumericSortedInputChannelName.ToList());
                            //    channelInputList = ComponentInputchannelList;

                            //    string[] alphaNumericSortedOutputChannelName = channelOutputList.ToArray();
                            //    Array.Sort(alphaNumericSortedOutputChannelName, new AlphanumComparatorFaster());
                            //    ObservableCollection<string> ComponentOutputchannelList = new ObservableCollection<string>(alphaNumericSortedOutputChannelName.ToList());
                            //    channelOutputList = ComponentOutputchannelList;

                            //    string[] alphaNumericSortedInputOutputChannelName = channelInputOutputList.ToArray();
                            //    Array.Sort(alphaNumericSortedInputOutputChannelName, new AlphanumComparatorFaster());
                            //    ObservableCollection<string> ComponentInputOutputchannelList = new ObservableCollection<string>(alphaNumericSortedInputOutputChannelName.ToList());
                            //    channelInputOutputList = ComponentInputOutputchannelList;

                            //    string[] alphaNumericSortedBankControlInputChannelName = channelBankControlInputList.ToArray();
                            //    Array.Sort(alphaNumericSortedBankControlInputChannelName, new AlphanumComparatorFaster());
                            //    ObservableCollection<string> ComponentBankControlInputchannelList = new ObservableCollection<string>(alphaNumericSortedBankControlInputChannelName.ToList());
                            //    channelBankControlInputList = ComponentBankControlInputchannelList;

                            //    string[] alphaNumericSortedBankControloutputChannelName = channelBankControlOutputList.ToArray();
                            //    Array.Sort(alphaNumericSortedBankControloutputChannelName, new AlphanumComparatorFaster());
                            //    ObservableCollection<string> ComponentBankControlOutputchannelList = new ObservableCollection<string>(alphaNumericSortedBankControloutputChannelName.ToList());
                            //    channelBankControlOutputList = ComponentBankControlOutputchannelList;

                            //    string[] alphaNumericSortedBankControlinputoutputChannelName = channelBankControlInputOutputList.ToArray();
                            //    Array.Sort(alphaNumericSortedBankControlinputoutputChannelName, new AlphanumComparatorFaster());
                            //    ObservableCollection<string> ComponentBankControlinputOutputchannelList = new ObservableCollection<string>(alphaNumericSortedBankControlinputoutputChannelName.ToList());
                            //    channelBankControlInputOutputList = ComponentBankControlinputOutputchannelList;

                            //    string[] alphaNumericSortedBankSelectInputChannelName = channelBankSelectInputList.ToArray();
                            //    Array.Sort(alphaNumericSortedBankSelectInputChannelName, new AlphanumComparatorFaster());
                            //    ObservableCollection<string> ComponentBankSelectInputchannelList = new ObservableCollection<string>(alphaNumericSortedBankSelectInputChannelName.ToList());
                            //    channelBankSelectInputList = ComponentBankSelectInputchannelList;

                            //    string[] alphaNumericSortedBankSelectoutputChannelName = channelBankSelectOutputList.ToArray();
                            //    Array.Sort(alphaNumericSortedBankSelectoutputChannelName, new AlphanumComparatorFaster());
                            //    ObservableCollection<string> ComponentBankSelectOutputchannelList = new ObservableCollection<string>(alphaNumericSortedBankSelectoutputChannelName.ToList());
                            //    channelBankSelectOutputList = ComponentBankSelectOutputchannelList;

                            //    //string[] alphaNumericSortedGPIOoutputChannelName = channelGPIOOutputList.ToArray();
                            //    //Array.Sort(alphaNumericSortedGPIOoutputChannelName, new AlphanumComparatorFaster());
                            //    //ObservableCollection<string> ComponentGPIOOutputchannelList = new ObservableCollection<string>(alphaNumericSortedGPIOoutputChannelName.ToList());
                            //    //channelGPIOOutputList = ComponentGPIOOutputchannelList;

                            //    //string[] alphaNumericSortedGPIOinputChannelName = channelGPIOInputList.ToArray();
                            //    //Array.Sort(alphaNumericSortedGPIOinputChannelName, new AlphanumComparatorFaster());
                            //    //ObservableCollection<string> ComponentGPIOInputchannelList = new ObservableCollection<string>(alphaNumericSortedGPIOinputChannelName.ToList());
                            //    //channelGPIOInputList = ComponentGPIOInputchannelList;
                            //}
                        }
                    }

                    if ((!TestControlPropertyList.Contains(TestControlPropertySelectedItem)) & ((!VerifyTestControlPropertyList.Contains(TestControlPropertySelectedItem))))
                    {
                        ChannelEnabled = false;
                        InputSelectionEnabled = false;
                        TestControlPropertySelectedItem = string.Empty;
                        TestControlPropertyInitialValueSelectedItem = null;
                        ChannelSelectionSelectedItem = string.Empty;
                        InputSelectionComboSelectedItem = string.Empty;
                    }
                    if ((TestControlPropertySelectedItem != null) & (TestControlPropertySelectedItem != string.Empty))
                    {
                        if (ParentTestActionItem.channelControlTypeDictionary.Keys.Contains(TestControlComponentNameSelectedItem + TestControlPropertySelectedItem))
                        {
                            //TestControlPropertyInitialValueSelectedItem = null;
                            ChannelEnabled = true;
                            if (ParentTestActionItem.ChannelSelectionDictionary.Count > 0)
                            {
                                string[] alphaNumericSortedChannelName = ParentTestActionItem.ChannelSelectionDictionary[TestControlComponentNameSelectedItem + TestControlPropertySelectedItem].ToArray();
                                Array.Sort(alphaNumericSortedChannelName, new AlphanumComparatorFaster());
                                ObservableCollection<string> ComponentchannelList = new ObservableCollection<string>(alphaNumericSortedChannelName.ToList());
                                //ObservableCollection<string> ComponentchannelList = new ObservableCollection<string>(ParentTestActionItem.ParentTestCaseItem.ChannelSelectionDictionary[value].OrderBy(a => a));
                                ChannelSelectionList = ComponentchannelList;
                            }
                        }
                        if (ParentTestActionItem.VerifychannelControlTypeDictionary.Keys.Contains(TestControlComponentNameSelectedItem + TestControlPropertySelectedItem))
                        {
                           // TestControlPropertyInitialValueSelectedItem = null;
                            ChannelEnabled = true;
                            if (ParentTestActionItem.ChannelSelectionDictionary.Count > 0)
                            {
                                string[] alphaNumericSortedChannelName = ParentTestActionItem.ChannelSelectionDictionary[TestControlComponentNameSelectedItem + TestControlPropertySelectedItem].ToArray();
                                Array.Sort(alphaNumericSortedChannelName, new AlphanumComparatorFaster());
                                ObservableCollection<string> ComponentchannelList = new ObservableCollection<string>(alphaNumericSortedChannelName.ToList());
                                //ObservableCollection<string> ComponentchannelList = new ObservableCollection<string>(ParentTestActionItem.ParentTestCaseItem.ChannelSelectionDictionary[value].OrderBy(a => a));
                                ChannelSelectionList = ComponentchannelList;
                            }
                        }
                        if (ParentTestActionItem.ControlTypeDictionary.Keys.Contains(TestControlComponentNameSelectedItem + TestControlPropertySelectedItem))
                        {
                            //TestControlPropertyInitialValueSelectedItem = null;
                            InputSelectionEnabled = true;
                        }
                    }
                    else
                    {
                        ChannelEnabled = false;
                        InputSelectionEnabled = false;
                    }
                    OnPropertyChanged("TestControlComponentNameSelectedItem");
                }
                catch (Exception ex)
                {
                    DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif

                    //MessageBox.Show("Exception\n " + ex.Message, "Error Code - ECxxxxx", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private ObservableCollection<string> testControlPropertyListValue = new ObservableCollection<string>();
        public ObservableCollection<string> TestControlPropertyList
        {
            get { return testControlPropertyListValue; }
            set
            {

                testControlPropertyListValue = value;
                //testControlPropertyListValue = new ObservableCollection<string>(testControlPropertyListValue.OrderBy(a => a));
                if (value == null || !value.Contains(TestControlPropertySelectedItem))
                    TestControlPropertySelectedItem = null;

                OnPropertyChanged("TestControlPropertyList");
            }
        }

        private ObservableCollection<string> VerifytestControlPropertyListValue = new ObservableCollection<string>();
        public ObservableCollection<string> VerifyTestControlPropertyList
        {
            get { return VerifytestControlPropertyListValue; }
            set
            {
                VerifytestControlPropertyListValue = value;
                //VerifytestControlPropertyListValue = new ObservableCollection<string>(VerifytestControlPropertyListValue.OrderBy(a => a));
                if (value == null || !value.Contains(TestControlPropertySelectedItem))
                    TestControlPropertySelectedItem = null;

                OnPropertyChanged("VerifyTestControlPropertyList");
            }
        }

        private string TestControlPropertySelectedItemValue = null;
        public string TestControlPropertySelectedItem
        {
            get { return TestControlPropertySelectedItemValue; }
            set
            {
                try
                {
                    if (value == TestControlPropertySelectedItemValue)
                    {
                        OnPropertyChanged("TestControlPropertySelectedItem");
                        return;
                    }

                    if (value != null)
                    {
                        InputSelectionEnabled = false;
                        ChannelEnabled = false;
                        ChannelSelectionSelectedItem = null;
                        InputSelectionComboSelectedItem = null;
                        TestControlPropertyInitialValueSelectedItem = null;
                        TestControlPropertySelectedItemValue = value;
                        DataTypeTextBlock = string.Empty;

                        if (TestControlComponentNameSelectedItem != null)
                        {
                            if (string.Equals("Control Action", "Control Action", StringComparison.CurrentCultureIgnoreCase))
                            {

                            if (ParentTestActionItem.ControlTypeDictionary.Keys.Contains(TestControlComponentNameSelectedItem + value))
                            {
                                InputSelectionEnabled = true;
                                if (ParentTestActionItem.ControlTypeDictionary[TestControlComponentNameSelectedItem + value].Contains("control_direction_ramp"))
                                {
                                    RampCheckVisibility = Visibility.Visible;
                                    ChannelEnabled = false;
                                }
                                else
                                {
                                    RampCheckVisibility = Visibility.Hidden;
                                    RampIsChecked = false;
                                    RampSetting = null;
                                }

                            }

                            else if (ParentTestActionItem.channelControlTypeDictionary.Keys.Contains(TestControlComponentNameSelectedItem + value))
                            {
                                InputSelectionEnabled = false;
                                if (ParentTestActionItem.channelControlTypeDictionary[TestControlComponentNameSelectedItem + value].Contains("control_direction_ramp"))
                                {
                                    RampCheckVisibility = Visibility.Visible;
                                    ChannelEnabled = true;


                                    //added on 21-sep-2016

                                    if (ParentTestActionItem.ChannelSelectionDictionary.Count > 0)
                                    {
                                        string[] alphaNumericSortedChannelName = ParentTestActionItem.ChannelSelectionDictionary[TestControlComponentNameSelectedItem + value].ToArray();
                                        Array.Sort(alphaNumericSortedChannelName, new AlphanumComparatorFaster());
                                        ObservableCollection<string> ComponentchannelList = new ObservableCollection<string>(alphaNumericSortedChannelName.ToList());
                                        //ObservableCollection<string> ComponentchannelList = new ObservableCollection<string>(ParentTestActionItem.ParentTestCaseItem.ChannelSelectionDictionary[value].OrderBy(a => a));
                                        ChannelSelectionList = ComponentchannelList;
                                    }

                                    //if (TestControlPropertySelectedItem.StartsWith("INPUT "))
                                    //{
                                    //    ChannelSelectionList = null;
                                    //    ChannelSelectionList = channelInputList;
                                    //}
                                    //else if (TestControlPropertySelectedItem.StartsWith("OUTPUT "))
                                    //{
                                    //    ChannelSelectionList = null;
                                    //    ChannelSelectionList = channelOutputList;
                                    //}
                                    //else if (TestControlPropertySelectedItem.StartsWith("INPUT_OUTPUT "))
                                    //{
                                    //    ChannelSelectionList = null;
                                    //    ChannelSelectionList = channelInputOutputList;
                                    //}
                                    //else if (TestControlPropertySelectedItem.StartsWith("BANK_CONTROL_INPUT "))
                                    //{
                                    //    ChannelSelectionList = null;
                                    //    ChannelSelectionList = channelBankControlInputList;
                                    //}
                                    //else if (TestControlPropertySelectedItem.StartsWith("BANK_CONTROL_OUTPUT "))
                                    //{
                                    //    ChannelSelectionList = null;
                                    //    ChannelSelectionList = channelBankControlOutputList;
                                    //}
                                    //else if (TestControlPropertySelectedItem.StartsWith("BANK_CONTROL_INPUT_OUTPUT "))
                                    //{
                                    //    ChannelSelectionList = null;
                                    //    ChannelSelectionList = channelBankControlInputOutputList;
                                    //}
                                    //else if (TestControlPropertySelectedItem.StartsWith("BANK_SELECT_INPUT "))
                                    //{
                                    //    ChannelSelectionList = null;
                                    //    ChannelSelectionList = channelBankSelectInputList;
                                    //}
                                    //else if (TestControlPropertySelectedItem.StartsWith("BANK_SELECT_OUTPUT "))
                                    //{
                                    //    ChannelSelectionList = null;
                                    //    ChannelSelectionList = channelBankSelectOutputList;
                                    //}
                                    //else if (TestControlPropertySelectedItem.StartsWith("GPIO_INPUT "))
                                    //{
                                    //    ChannelSelectionList = null;
                                    //    ChannelSelectionList = channelGPIOInputList;
                                    //}
                                    //else if (TestControlPropertySelectedItem.StartsWith("GPIO_OUTPUT "))
                                    //{
                                    //    ChannelSelectionList = null;
                                    //    ChannelSelectionList = channelGPIOOutputList;
                                    //}
                                }
                                else
                                {
                                    RampCheckVisibility = Visibility.Hidden;
                                    RampIsChecked = false;
                                    RampSetting = null;
                                }
                            }
                            }
                            if (ParentTestActionItem.VerifychannelControlTypeDictionary.Keys.Contains(TestControlComponentNameSelectedItem + value))
                            {
                                InputSelectionEnabled = false;
                                ChannelEnabled = true;

                            if (ParentTestActionItem.ChannelSelectionDictionary.Count > 0)
                            {
                                string[] alphaNumericSortedChannelName = ParentTestActionItem.ChannelSelectionDictionary[TestControlComponentNameSelectedItem + value].ToArray();
                                Array.Sort(alphaNumericSortedChannelName, new AlphanumComparatorFaster());
                                ObservableCollection<string> ComponentchannelList = new ObservableCollection<string>(alphaNumericSortedChannelName.ToList());
                                //ObservableCollection<string> ComponentchannelList = new ObservableCollection<string>(ParentTestActionItem.ParentTestCaseItem.ChannelSelectionDictionary[value].OrderBy(a => a));
                                ChannelSelectionList = ComponentchannelList;
                            }
                            //if (TestControlPropertySelectedItem.StartsWith("INPUT "))
                            //{
                            //    ChannelSelectionList = null;
                            //    ChannelSelectionList = channelInputList;
                            //}
                            //else if (TestControlPropertySelectedItem.StartsWith("OUTPUT "))
                            //{
                            //    ChannelSelectionList = null;
                            //    ChannelSelectionList = channelOutputList;
                            //}
                            //else if (TestControlPropertySelectedItem.StartsWith("INPUT_OUTPUT "))
                            //{
                            //    ChannelSelectionList = null;
                            //    ChannelSelectionList = channelInputOutputList;
                            //}
                            //else if (TestControlPropertySelectedItem.StartsWith("BANK_CONTROL_INPUT "))
                            //{
                            //    ChannelSelectionList = null;
                            //    ChannelSelectionList = channelBankControlInputList;
                            //}
                            //else if (TestControlPropertySelectedItem.StartsWith("BANK_CONTROL_OUTPUT "))
                            //{
                            //    ChannelSelectionList = null;
                            //    ChannelSelectionList = channelBankControlOutputList;
                            //}
                            //else if (TestControlPropertySelectedItem.StartsWith("BANK_CONTROL_INPUT_OUTPUT "))
                            //{
                            //    ChannelSelectionList = null;
                            //    ChannelSelectionList = channelBankControlInputOutputList;
                            //}
                            //else if (TestControlPropertySelectedItem.StartsWith("BANK_SELECT_INPUT "))
                            //{
                            //    ChannelSelectionList = null;
                            //    ChannelSelectionList = channelBankSelectInputList;
                            //}
                            //else if (TestControlPropertySelectedItem.StartsWith("BANK_SELECT_OUTPUT "))
                            //{
                            //    ChannelSelectionList = null;
                            //    ChannelSelectionList = channelBankSelectOutputList;
                            //}
                            //else if (TestControlPropertySelectedItem.StartsWith("GPIO_INPUT "))
                            //{
                            //    ChannelSelectionList = null;
                            //    ChannelSelectionList = channelGPIOInputList;
                            //}
                            //else if (TestControlPropertySelectedItem.StartsWith("GPIO_OUTPUT "))
                            //{
                            //    ChannelSelectionList = null;
                            //    ChannelSelectionList = channelGPIOOutputList;
                            //}
                        }
                        else
                        {
                            //ChannelEnabled = false;
                            //InputSelectionEnabled = true;


                                ////Added on 16-jan-2017
                                if ((ParentTestActionItem.VerifyControlTypeDictionary.Keys.Contains(TestControlComponentNameSelectedItem + value)))//&&(!(ParentTestActionItem.ParentTestCaseItem.ControlTypeDictionary.Keys.Contains(value)))
                                {
                                    if (ParentTestActionItem.VerifyControlTypeDictionary[TestControlComponentNameSelectedItem + value].Contains("control_direction_external_read"))
                                    {
                                        InputSelectionEnabled = true;
                                        ChannelEnabled = false;
                                    }
                                }
                                ////Added on 16-jan-2017
                            }
                        }
                    }
                    OnPropertyChanged("TestControlPropertySelectedItem");

                }
                catch (Exception ex)
                {
                    //MessageBox.Show("Exception\n " + ex.Message, "Error Code - ECxxxxx", MessageBoxButton.OK, MessageBoxImage.Error);
                    DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif

                }
            }
        }


        //ObservableCollection<string> channelInputList = new ObservableCollection<string>();
        //ObservableCollection<string> channelOutputList = new ObservableCollection<string>();
        //ObservableCollection<string> channelInputOutputList = new ObservableCollection<string>();
        //ObservableCollection<string> channelBankControlInputList = new ObservableCollection<string>();
        //ObservableCollection<string> channelBankControlOutputList = new ObservableCollection<string>();
        //ObservableCollection<string> channelBankControlInputOutputList = new ObservableCollection<string>();
        //ObservableCollection<string> channelBankSelectInputList = new ObservableCollection<string>();
        //ObservableCollection<string> channelBankSelectOutputList = new ObservableCollection<string>();
        //ObservableCollection<string> channelGPIOInputList = new ObservableCollection<string>();
        //ObservableCollection<string> channelGPIOOutputList = new ObservableCollection<string>();

        private ObservableCollection<string> ChannelSelectionListValue = new ObservableCollection<string>();
        public ObservableCollection<string> ChannelSelectionList
        {
            get { return ChannelSelectionListValue; }
            set
            {
                ChannelSelectionListValue = value;
                //ChannelSelectionListValue = new ObservableCollection<string>(ChannelSelectionListValue.OrderBy(a => a));
                //if (value == null || !value.Contains(ChannelSelectionSelectedItem))
                //    ChannelSelectionSelectedItem = null;

                if (LoopIsChecked)
                {
                    if (LoopStart != null && LoopStart != string.Empty)
                    {
                        if (ChannelSelectionList.Count < Convert.ToInt32(LoopStart))
                        {
                            LoopStart = string.Empty;
                        }
                    }

                    if (LoopIncrement != null && LoopIncrement != string.Empty)
                    {
                        if (LoopEnd != null && LoopEnd != string.Empty)
                        {
                            if (!((Convert.ToInt32(LoopIncrement) > 0) && (Convert.ToInt32(LoopIncrement) <= ChannelSelectionList.Count) && (Convert.ToInt32(LoopIncrement) <= Convert.ToInt32(LoopEnd))))
                            {
                                LoopIncrement = string.Empty;
                            }
                        }
                        else
                        {
                            LoopIncrement = string.Empty;
                        }
                    }

                    if (LoopEnd != null && LoopEnd != string.Empty)
                    {
                        if (ChannelSelectionList.Count < Convert.ToInt32(LoopEnd))
                        {
                            LoopEnd = string.Empty;
                        }
                    }
                }

                OnPropertyChanged("ChannelSelectionList");
            }
        }

        private string ChannelSelectionSelectedItemValue = string.Empty;
        public string ChannelSelectionSelectedItem
        {
            get { return ChannelSelectionSelectedItemValue; }
            set
            {
                try
                {
                    if (value == ChannelSelectionSelectedItemValue)
                    {
                        OnPropertyChanged("ChannelSelectionSelectedItem");
                        return;
                    }
                    if (value != null && value != string.Empty)
                    {
                        InputSelectionEnabled = true;
                        LoopCheckVisibility = Visibility.Visible;
                        InputSelectionComboSelectedItem = null;
                        TestControlPropertyInitialValueSelectedItem = null;
                        ChannelSelectionSelectedItemValue = value;

                        if (LoopStart != null && LoopStart != string.Empty)
                        {
                            if (ChannelSelectionList.Count < Convert.ToInt32(LoopStart))
                            {
                                LoopStart = string.Empty;
                            }
                        }

                        if (LoopIncrement != null && LoopIncrement != string.Empty)
                        {
                            if (LoopEnd != null && LoopEnd != string.Empty)
                            {
                                if (!((Convert.ToInt32(LoopIncrement) > 0) && (Convert.ToInt32(LoopIncrement) <= ChannelSelectionList.Count) && (Convert.ToInt32(LoopIncrement) <= Convert.ToInt32(LoopEnd))))
                                {
                                    LoopIncrement = string.Empty;
                                }
                            }
                            else
                            {
                                LoopIncrement = string.Empty;
                            }
                        }

                        if (LoopEnd != null && LoopEnd != string.Empty)
                        {
                            if (ChannelSelectionList.Count < Convert.ToInt32(LoopEnd))
                            {
                                LoopEnd = string.Empty;
                            }
                        }
                    }
                    else
                    {
                        ChannelSelectionSelectedItemValue =null;
                        LoopIsChecked = false;
                        LoopCheckVisibility = Visibility.Hidden;
                    }

                    OnPropertyChanged("ChannelSelectionSelectedItem");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Exception\n " + ex.Message, "Error Code - ECxxxxx", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }


        private List<string> InputSelectionComboListValue = new List<string> { "Set by value", "Set by string", "Set by position" };
        public List<string> InputSelectionComboList
        {
            get { return InputSelectionComboListValue; }
            private set
            {
                InputSelectionComboListValue = value;
                OnPropertyChanged("InputSelectionComboList");
            }
        }

        private string InputSelectionComboSelectedItemValue = null;
        public string InputSelectionComboSelectedItem
        {
            get { return InputSelectionComboSelectedItemValue; }
            set
            {
                try
                {
                    InputSelectionComboSelectedItemValue = value;

                    if (value != null)
                    {
                        valueIsEnabled = true;
                        string valueType = string.Empty;

                        ////
                        string valueDataType = string.Empty;
                        string controlSelected = string.Empty;
                        string selectedPrettyName = ChannelSelectionSelectedItem;
                        string selectedControlID = string.Empty;

                        if ((!string.IsNullOrEmpty(TestControlPropertySelectedItem))  & (!string.IsNullOrEmpty((ChannelSelectionSelectedItem))))//&((TestControlPropertySelectedItem.StartsWith("CHANNEL "))|| (TestControlPropertySelectedItem.StartsWith("OUTPUT "))|| (TestControlPropertySelectedItem.StartsWith("INPUT ")) || (TestControlPropertySelectedItem.StartsWith("INPUT_OUTPUT ")) || (TestControlPropertySelectedItem.StartsWith("TAP ")) || (TestControlPropertySelectedItem.StartsWith("BANK_CONTROL ")) || (TestControlPropertySelectedItem.StartsWith("BANK_SELECT ")) || (TestControlPropertySelectedItem.StartsWith("GPIO_INPUT ")) || (TestControlPropertySelectedItem.StartsWith("GPIO_OUTPUT ")))
                        {
                            string[] prettyControlName = null;
                            if(TestControlPropertySelectedItem != null)
                            ParentTestActionItem.ControlIDDictionary.TryGetValue(TestControlComponentNameSelectedItem + TestControlPropertySelectedItem + selectedPrettyName, out prettyControlName);

                            if (prettyControlName != null)
                            {
                                if(prettyControlName.Count() > 0)
                                    selectedPrettyName = prettyControlName[0];

                                if(prettyControlName.Count() > 1)
                                    selectedControlID = prettyControlName[1];
                            }

                            controlSelected = selectedPrettyName + "~" + removeQATPrefix(TestControlPropertySelectedItem);
							
                            //if((TestControlPropertySelectedItem.StartsWith("CHANNEL ")))
                            //controlSelected = ChannelSelectionSelectedItem + "~" + removeQATPrefix( TestControlPropertySelectedItem);
                            //else if ((TestControlPropertySelectedItem.StartsWith("OUTPUT ")))
                            //    controlSelected = ChannelSelectionSelectedItem + "~" + TestControlPropertySelectedItem.Remove(0, 7);
                            //else if((TestControlPropertySelectedItem.StartsWith("INPUT ")))
                            //    controlSelected = ChannelSelectionSelectedItem + "~" + TestControlPropertySelectedItem.Remove(0, 6);
                            //else if ((TestControlPropertySelectedItem.StartsWith("INPUT_OUTPUT ")))
                            //    controlSelected = ChannelSelectionSelectedItem + "~" + TestControlPropertySelectedItem.Remove(0, 13);
                            //else if ((TestControlPropertySelectedItem.StartsWith("TAP ")))
                            //    controlSelected = ChannelSelectionSelectedItem + "~" + TestControlPropertySelectedItem.Remove(0, 4);
                            //else if ((TestControlPropertySelectedItem.StartsWith("BANK_CONTROL_INPUT ")))
                            //    controlSelected = ChannelSelectionSelectedItem + "~" + TestControlPropertySelectedItem.Remove(0, 19);
                            //else if ((TestControlPropertySelectedItem.StartsWith("BANK_CONTROL_OUTPUT ")))
                            //    controlSelected = ChannelSelectionSelectedItem + "~" + TestControlPropertySelectedItem.Remove(0, 20);
                            //else if ((TestControlPropertySelectedItem.StartsWith("BANK_CONTROL_INPUT_OUTPUT ")))
                            //    controlSelected = ChannelSelectionSelectedItem + "~" + TestControlPropertySelectedItem.Remove(0, 26);
                            //else if ((TestControlPropertySelectedItem.StartsWith("BANK_SELECT_INPUT ")))
                            //    controlSelected = ChannelSelectionSelectedItem + "~" + TestControlPropertySelectedItem.Remove(0, 18);
                            //else if ((TestControlPropertySelectedItem.StartsWith("BANK_SELECT_OUTPUT ")))
                            //    controlSelected = ChannelSelectionSelectedItem + "~" + TestControlPropertySelectedItem.Remove(0, 19);
                            //else if (TestControlPropertySelectedItem.StartsWith("GPIO_INPUT "))
                            //    controlSelected = ChannelSelectionSelectedItem + "~" + TestControlPropertySelectedItem.Remove(0, 11);
                            //else if (TestControlPropertySelectedItem.StartsWith("GPIO_OUTPUT "))
                            //    controlSelected = ChannelSelectionSelectedItem + "~" + TestControlPropertySelectedItem.Remove(0, 12);
                        }
                        else
                        {
                            string[] prettyControlName = null;
                            if(TestControlComponentNameSelectedItem != null && TestControlPropertySelectedItem != null)
                                ParentTestActionItem.ControlIDDictionary.TryGetValue(TestControlComponentNameSelectedItem + TestControlPropertySelectedItem, out prettyControlName);
                            
                            //selectedPrettyName = prettyControlName[0];

                            if(prettyControlName != null && prettyControlName.Count() > 1)
                                selectedControlID = prettyControlName[1];

                            controlSelected = TestControlPropertySelectedItem;
                        }

                        if(TestControlComponentNameSelectedItem != null && controlSelected != null && selectedControlID != null)
                            ParentTestActionItem.dataTypeDictionary.TryGetValue(TestControlComponentNameSelectedItem + controlSelected + selectedControlID, out valueDataType);

                        ////
                        //ParentTestActionItem.ParentTestCaseItem.dataTypeDictionary.TryGetValue(  TestControlComponentNameSelectedItem+TestControlPropertySelectedItem, out valueType);
                        //if ((valueType == string.Empty) | (valueType == null))
                        //    ParentTestActionItem.ParentTestCaseItem.VerifyDataTypeDictionary.TryGetValue(  TestControlComponentNameSelectedItem+TestControlPropertySelectedItem, out valueType);

                        if (String.Equals("Set by value", InputSelectionComboSelectedItem, StringComparison.InvariantCultureIgnoreCase))
                        {
                            MaxLimitIsEnabled = true;
                            MinLimitIsEnabled = true;
                            if (((String.Equals("Text", valueDataType, StringComparison.InvariantCultureIgnoreCase)) || (String.Equals("Unknown", valueDataType, StringComparison.InvariantCultureIgnoreCase))))
                                valueMaxLength = 255;
                            else
                                valueMaxLength = 20;
                            if (ChannelSelectionSelectedItem == null & TestControlComponentNameSelectedItem != null & TestControlPropertySelectedItem != null)
                            {
                                string valuedict = string.Empty;
                                ParentTestActionItem.ControlInitialValueDictionary.TryGetValue(TestControlComponentNameSelectedItem + TestControlPropertySelectedItem, out valuedict);
                                TestControlPropertyInitialValueSelectedItem = valuedict;
                            }
                            else if (!string.IsNullOrEmpty(ChannelSelectionSelectedItem) & TestControlComponentNameSelectedItem != null & TestControlPropertySelectedItem != null)
                            {
                                string valuedict = string.Empty;
                                ParentTestActionItem.ControlInitialValueDictionary.TryGetValue(TestControlComponentNameSelectedItem + selectedPrettyName + "~" + removeQATPrefix(TestControlPropertySelectedItem) + selectedControlID, out valuedict);
                                TestControlPropertyInitialValueSelectedItem = valuedict;

                                //if (TestControlPropertySelectedItem.StartsWith("CHANNEL "))
                                //    TestControlPropertyInitialValueSelectedItem = ParentTestActionItem.ControlInitialValueDictionary[TestControlComponentNameSelectedItem + ChannelSelectionSelectedItem + "~" +removeQATPrefix( TestControlPropertySelectedItem)];
                                //else if (TestControlPropertySelectedItem.StartsWith("OUTPUT "))
                                //    TestControlPropertyInitialValueSelectedItem = ParentTestActionItem.ControlInitialValueDictionary[TestControlComponentNameSelectedItem + ChannelSelectionSelectedItem + "~" + TestControlPropertySelectedItem.Remove(0, 7)];
                                //else if (TestControlPropertySelectedItem.StartsWith("INPUT "))
                                //    TestControlPropertyInitialValueSelectedItem = ParentTestActionItem.ControlInitialValueDictionary[TestControlComponentNameSelectedItem + ChannelSelectionSelectedItem + "~" + TestControlPropertySelectedItem.Remove(0, 6)];
                                //else if (TestControlPropertySelectedItem.StartsWith("INPUT_OUTPUT "))
                                //    TestControlPropertyInitialValueSelectedItem = ParentTestActionItem.ControlInitialValueDictionary[TestControlComponentNameSelectedItem + ChannelSelectionSelectedItem + "~" + TestControlPropertySelectedItem.Remove(0, 13)];
                                //else if (TestControlPropertySelectedItem.StartsWith("TAP "))
                                //    TestControlPropertyInitialValueSelectedItem = ParentTestActionItem.ControlInitialValueDictionary[TestControlComponentNameSelectedItem + ChannelSelectionSelectedItem + "~" + TestControlPropertySelectedItem.Remove(0, 4)];
                                //else if (TestControlPropertySelectedItem.StartsWith("BANK_CONTROL_INPUT "))
                                //    TestControlPropertyInitialValueSelectedItem = ParentTestActionItem.ControlInitialValueDictionary[TestControlComponentNameSelectedItem + ChannelSelectionSelectedItem + "~" + TestControlPropertySelectedItem.Remove(0, 19)];
                                //else if (TestControlPropertySelectedItem.StartsWith("BANK_CONTROL_OUTPUT "))
                                //    TestControlPropertyInitialValueSelectedItem = ParentTestActionItem.ControlInitialValueDictionary[TestControlComponentNameSelectedItem + ChannelSelectionSelectedItem + "~" + TestControlPropertySelectedItem.Remove(0, 20)];
                                //else if (TestControlPropertySelectedItem.StartsWith("BANK_CONTROL_INPUT_OUTPUT "))
                                //    TestControlPropertyInitialValueSelectedItem = ParentTestActionItem.ControlInitialValueDictionary[TestControlComponentNameSelectedItem + ChannelSelectionSelectedItem + "~" + TestControlPropertySelectedItem.Remove(0, 26)];
                                //else if (TestControlPropertySelectedItem.StartsWith("BANK_SELECT_INPUT "))
                                //    TestControlPropertyInitialValueSelectedItem = ParentTestActionItem.ControlInitialValueDictionary[TestControlComponentNameSelectedItem + ChannelSelectionSelectedItem + "~" + TestControlPropertySelectedItem.Remove(0, 18)];
                                //else if (TestControlPropertySelectedItem.StartsWith("BANK_SELECT_OUTPUT "))
                                //    TestControlPropertyInitialValueSelectedItem = ParentTestActionItem.ControlInitialValueDictionary[TestControlComponentNameSelectedItem + ChannelSelectionSelectedItem + "~" + TestControlPropertySelectedItem.Remove(0, 19)];
                                //else if (TestControlPropertySelectedItem.StartsWith("GPIO_INPUT "))
                                //    TestControlPropertyInitialValueSelectedItem = ParentTestActionItem.ControlInitialValueDictionary[TestControlComponentNameSelectedItem + ChannelSelectionSelectedItem + "~" + TestControlPropertySelectedItem.Remove(0, 11)];
                                //else if (TestControlPropertySelectedItem.StartsWith("GPIO_OUTPUT "))
                                //    TestControlPropertyInitialValueSelectedItem = ParentTestActionItem.ControlInitialValueDictionary[TestControlComponentNameSelectedItem + ChannelSelectionSelectedItem + "~" + TestControlPropertySelectedItem.Remove(0, 12)];
                            }
                        }
                        else if (String.Equals("Set by string", InputSelectionComboSelectedItem, StringComparison.InvariantCultureIgnoreCase))
                        {
                            MaxLimitIsEnabled = false;
                            MinLimitIsEnabled = false;
                            valueMaxLength = 255;
                            MaximumLimit = string.Empty;
                            MinimumLimit = string.Empty;

                            if (ChannelSelectionSelectedItem == null & TestControlComponentNameSelectedItem != null & TestControlPropertySelectedItem != null)
                            {
                                string stringVal = string.Empty;
                                ParentTestActionItem.ControlInitialStringDictionary.TryGetValue(TestControlComponentNameSelectedItem + TestControlPropertySelectedItem, out stringVal);
                                TestControlPropertyInitialValueSelectedItem = stringVal;
                            }
                            else if (!string.IsNullOrEmpty(ChannelSelectionSelectedItem) & TestControlComponentNameSelectedItem != null & TestControlPropertySelectedItem != null)
                            {
                                string stringVal = string.Empty;
                                ParentTestActionItem.ControlInitialStringDictionary.TryGetValue(TestControlComponentNameSelectedItem + selectedPrettyName + "~" + removeQATPrefix(TestControlPropertySelectedItem) + selectedControlID, out stringVal);
                                TestControlPropertyInitialValueSelectedItem = stringVal;

                                //if (TestControlPropertySelectedItem.StartsWith("CHANNEL "))
                                //    TestControlPropertyInitialValueSelectedItem = ParentTestActionItem.ControlInitialStringDictionary[TestControlComponentNameSelectedItem + ChannelSelectionSelectedItem + "~" + removeQATPrefix(TestControlPropertySelectedItem)];
                                //else if (TestControlPropertySelectedItem.StartsWith("OUTPUT "))
                                //    TestControlPropertyInitialValueSelectedItem = ParentTestActionItem.ControlInitialStringDictionary[TestControlComponentNameSelectedItem + ChannelSelectionSelectedItem + "~" + TestControlPropertySelectedItem.Remove(0, 7)];
                                //else if (TestControlPropertySelectedItem.StartsWith("INPUT "))
                                //    TestControlPropertyInitialValueSelectedItem = ParentTestActionItem.ControlInitialStringDictionary[TestControlComponentNameSelectedItem + ChannelSelectionSelectedItem + "~" + TestControlPropertySelectedItem.Remove(0, 6)];
                                //else if (TestControlPropertySelectedItem.StartsWith("INPUT_OUTPUT "))
                                //    TestControlPropertyInitialValueSelectedItem = ParentTestActionItem.ControlInitialStringDictionary[TestControlComponentNameSelectedItem + ChannelSelectionSelectedItem + "~" + TestControlPropertySelectedItem.Remove(0, 13)];
                                //else if (TestControlPropertySelectedItem.StartsWith("TAP "))
                                //    TestControlPropertyInitialValueSelectedItem = ParentTestActionItem.ControlInitialStringDictionary[TestControlComponentNameSelectedItem + ChannelSelectionSelectedItem + "~" + TestControlPropertySelectedItem.Remove(0, 4)];
                                //else if (TestControlPropertySelectedItem.StartsWith("BANK_CONTROL_INPUT "))
                                //    TestControlPropertyInitialValueSelectedItem = ParentTestActionItem.ControlInitialStringDictionary[TestControlComponentNameSelectedItem + ChannelSelectionSelectedItem + "~" + TestControlPropertySelectedItem.Remove(0, 19)];
                                //else if (TestControlPropertySelectedItem.StartsWith("BANK_CONTROL_OUTPUT "))
                                //    TestControlPropertyInitialValueSelectedItem = ParentTestActionItem.ControlInitialStringDictionary[TestControlComponentNameSelectedItem + ChannelSelectionSelectedItem + "~" + TestControlPropertySelectedItem.Remove(0, 20)];
                                //else if (TestControlPropertySelectedItem.StartsWith("BANK_CONTROL_INPUT_OUTPUT "))
                                //    TestControlPropertyInitialValueSelectedItem = ParentTestActionItem.ControlInitialStringDictionary[TestControlComponentNameSelectedItem + ChannelSelectionSelectedItem + "~" + TestControlPropertySelectedItem.Remove(0, 26)];
                                //else if (TestControlPropertySelectedItem.StartsWith("BANK_SELECT_INPUT "))
                                //    TestControlPropertyInitialValueSelectedItem = ParentTestActionItem.ControlInitialStringDictionary[TestControlComponentNameSelectedItem + ChannelSelectionSelectedItem + "~" + TestControlPropertySelectedItem.Remove(0, 18)];
                                //else if (TestControlPropertySelectedItem.StartsWith("BANK_SELECT_OUTPUT "))
                                //    TestControlPropertyInitialValueSelectedItem = ParentTestActionItem.ControlInitialStringDictionary[TestControlComponentNameSelectedItem + ChannelSelectionSelectedItem + "~" + TestControlPropertySelectedItem.Remove(0, 19)];
                                //else if (TestControlPropertySelectedItem.StartsWith("GPIO_INPUT "))
                                //    TestControlPropertyInitialValueSelectedItem = ParentTestActionItem.ControlInitialStringDictionary[TestControlComponentNameSelectedItem + ChannelSelectionSelectedItem + "~" + TestControlPropertySelectedItem.Remove(0, 11)];
                                //else if (TestControlPropertySelectedItem.StartsWith("GPIO_OUTPUT "))
                                //    TestControlPropertyInitialValueSelectedItem = ParentTestActionItem.ControlInitialStringDictionary[TestControlComponentNameSelectedItem + ChannelSelectionSelectedItem + "~" + TestControlPropertySelectedItem.Remove(0, 12)];
                            }
                        }
                        else if (String.Equals("Set by position", InputSelectionComboSelectedItem, StringComparison.InvariantCultureIgnoreCase))
                        {
                            MaxLimitIsEnabled = true;
                            MinLimitIsEnabled = true;
                            valueMaxLength = 20;
							
                            if (ChannelSelectionSelectedItem == null & TestControlComponentNameSelectedItem != null & TestControlPropertySelectedItem != null)
                            {
                                string position = string.Empty;
                                ParentTestActionItem.ControlInitialPositionDictionary.TryGetValue(TestControlComponentNameSelectedItem + TestControlPropertySelectedItem, out position);
                                TestControlPropertyInitialValueSelectedItem = position;
                            }
                            else if (!string.IsNullOrEmpty(ChannelSelectionSelectedItem) & TestControlComponentNameSelectedItem != null & TestControlPropertySelectedItem != null)
                            {
                                string position = string.Empty;
                                ParentTestActionItem.ControlInitialPositionDictionary.TryGetValue(TestControlComponentNameSelectedItem + selectedPrettyName + "~" + removeQATPrefix(TestControlPropertySelectedItem) + selectedControlID, out position);
                                TestControlPropertyInitialValueSelectedItem = position;
								
                                //if (TestControlPropertySelectedItem.StartsWith("CHANNEL "))
                                //    TestControlPropertyInitialValueSelectedItem = ParentTestActionItem.ControlInitialPositionDictionary[TestControlComponentNameSelectedItem + ChannelSelectionSelectedItem + "~" + removeQATPrefix(TestControlPropertySelectedItem)];
                                //else if (TestControlPropertySelectedItem.StartsWith("OUTPUT "))
                                //    TestControlPropertyInitialValueSelectedItem = ParentTestActionItem.ControlInitialPositionDictionary[TestControlComponentNameSelectedItem + ChannelSelectionSelectedItem + "~" + TestControlPropertySelectedItem.Remove(0, 7)];
                                //else if (TestControlPropertySelectedItem.StartsWith("INPUT "))
                                //    TestControlPropertyInitialValueSelectedItem = ParentTestActionItem.ControlInitialPositionDictionary[TestControlComponentNameSelectedItem + ChannelSelectionSelectedItem + "~" + TestControlPropertySelectedItem.Remove(0, 6)];
                                //else if (TestControlPropertySelectedItem.StartsWith("INPUT_OUTPUT "))
                                //    TestControlPropertyInitialValueSelectedItem = ParentTestActionItem.ControlInitialPositionDictionary[TestControlComponentNameSelectedItem + ChannelSelectionSelectedItem + "~" + TestControlPropertySelectedItem.Remove(0, 13)];
                                //else if (TestControlPropertySelectedItem.StartsWith("TAP "))
                                //    TestControlPropertyInitialValueSelectedItem = ParentTestActionItem.ControlInitialPositionDictionary[TestControlComponentNameSelectedItem + ChannelSelectionSelectedItem + "~" + TestControlPropertySelectedItem.Remove(0, 4)];
                                //else if (TestControlPropertySelectedItem.StartsWith("BANK_CONTROL_INPUT "))
                                //    TestControlPropertyInitialValueSelectedItem = ParentTestActionItem.ControlInitialPositionDictionary[TestControlComponentNameSelectedItem + ChannelSelectionSelectedItem + "~" + TestControlPropertySelectedItem.Remove(0, 19)];
                                //else if (TestControlPropertySelectedItem.StartsWith("BANK_CONTROL_OUTPUT "))
                                //    TestControlPropertyInitialValueSelectedItem = ParentTestActionItem.ControlInitialPositionDictionary[TestControlComponentNameSelectedItem + ChannelSelectionSelectedItem + "~" + TestControlPropertySelectedItem.Remove(0, 20)];
                                //else if (TestControlPropertySelectedItem.StartsWith("BANK_CONTROL_INPUT_OUTPUT "))
                                //    TestControlPropertyInitialValueSelectedItem = ParentTestActionItem.ControlInitialPositionDictionary[TestControlComponentNameSelectedItem + ChannelSelectionSelectedItem + "~" + TestControlPropertySelectedItem.Remove(0, 26)];
                                //else if (TestControlPropertySelectedItem.StartsWith("BANK_SELECT_INPUT "))
                                //    TestControlPropertyInitialValueSelectedItem = ParentTestActionItem.ControlInitialPositionDictionary[TestControlComponentNameSelectedItem + ChannelSelectionSelectedItem + "~" + TestControlPropertySelectedItem.Remove(0, 18)];
                                //else if (TestControlPropertySelectedItem.StartsWith("BANK_SELECT_OUTPUT "))
                                //    TestControlPropertyInitialValueSelectedItem = ParentTestActionItem.ControlInitialPositionDictionary[TestControlComponentNameSelectedItem + ChannelSelectionSelectedItem + "~" + TestControlPropertySelectedItem.Remove(0, 19)];
                                //else if (TestControlPropertySelectedItem.StartsWith("GPIO_INPUT "))
                                //    TestControlPropertyInitialValueSelectedItem = ParentTestActionItem.ControlInitialPositionDictionary[TestControlComponentNameSelectedItem + ChannelSelectionSelectedItem + "~" + TestControlPropertySelectedItem.Remove(0, 11)];
                                //else if (TestControlPropertySelectedItem.StartsWith("GPIO_OUTPUT "))
                                //    TestControlPropertyInitialValueSelectedItem = ParentTestActionItem.ControlInitialPositionDictionary[TestControlComponentNameSelectedItem + ChannelSelectionSelectedItem + "~" + TestControlPropertySelectedItem.Remove(0, 12)];

                            }
                        }
                        //}

                    }
                    else
                    {
                        testControlPropertyInitialValueSelectedItemValue = null;
                        valueIsEnabled = false;
                        MaxLimitIsEnabled = false;
                        MinLimitIsEnabled = false;
                        MaximumLimit = string.Empty;
                        MinimumLimit = string.Empty;
                    }

                    OnPropertyChanged("InputSelectionComboSelectedItem");
                }
                catch (Exception ex)
                {
                    DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif

                    //MessageBox.Show("Exception\n " + ex.Message, "Error Code - ECxxxxx", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        //private List<string> InitialComboListValue = new List<string> { "False", "True" };
        //public List<string> InitialComboList
        //{
        //    get { return InitialComboListValue; }
        //    private set { InitialComboListValue = value; OnPropertyChanged("InitialComboList"); }
        //}

        //private string TestControlComboValueSelectedItemValue = null;
        //public string TestControlComboValueSelectedItem
        //{
        //    get { return TestControlComboValueSelectedItemValue; }
        //    set
        //    {
        //        TestControlComboValueSelectedItemValue = value;
        //        OnPropertyChanged("TestControlComboValueSelectedItem");
        //    }
        //}

        private string testControlPropertyInitialValueSelectedItemValue = null;
        public string TestControlPropertyInitialValueSelectedItem
        {
            get
            {

                return testControlPropertyInitialValueSelectedItemValue;
            }
            set
            {
                testControlPropertyInitialValueSelectedItemValue = value;


                OnPropertyChanged("TestControlPropertyInitialValueSelectedItem");
            }
        }

        private bool loopIsCheckedValue = false;
        public bool LoopIsChecked
        {
            get { return loopIsCheckedValue; }
            set
            {
                if (value == true)
                {
                    LoopStartValueVisibility = Visibility.Visible;
                    LoopEndValueVisibility = Visibility.Visible;
                    LoopIncrementValueVisibility = Visibility.Visible;
                }
                else
                {
                    LoopStartValueVisibility = Visibility.Hidden;
                    LoopEndValueVisibility = Visibility.Hidden;
                    LoopIncrementValueVisibility = Visibility.Hidden;
                    LoopStart = null;
                    LoopEnd = null;
                    LoopIncrement = null;
                }

                loopIsCheckedValue = value;
                OnPropertyChanged("LoopIsChecked");
            }
        }

        private string loopStartValue = null;
        public string LoopStart
        {
            get { return loopStartValue; }
            set { loopStartValue = value; OnPropertyChanged("LoopStart"); }
        }

        private Visibility loopStartVisibilityValue = Visibility.Hidden;
        public Visibility LoopStartValueVisibility
        {
            get { return loopStartVisibilityValue; }
            set { loopStartVisibilityValue = value; OnPropertyChanged("LoopStartValueVisibility"); }
        }

        private string loopEndValue = null;
        public string LoopEnd
        {
            get { return loopEndValue; }
            set { loopEndValue = value; OnPropertyChanged("LoopEnd"); }
        }

        private Visibility loopEndVisibilityValue = Visibility.Hidden;
        public Visibility LoopEndValueVisibility
        {
            get { return loopEndVisibilityValue; }
            set { loopEndVisibilityValue = value; OnPropertyChanged("LoopEndValueVisibility"); }
        }

        private string loopIncrementValue = null;
        public string LoopIncrement
        {
            get { return loopIncrementValue; }
            set { loopIncrementValue = value; OnPropertyChanged("LoopIncrement"); }
        }

        private Visibility loopIncrementVisibilityValue = Visibility.Hidden;
        public Visibility LoopIncrementValueVisibility
        {
            get { return loopIncrementVisibilityValue; }
            set { loopIncrementVisibilityValue = value; OnPropertyChanged("LoopIncrementValueVisibility"); }
        }

        private Visibility RampTextBlockVisibilityValue = Visibility.Hidden;
        public Visibility RampTextBlockVisibility
        {
            get { return RampTextBlockVisibilityValue; }
            set { RampTextBlockVisibilityValue = value; OnPropertyChanged("RampTextBlockVisibility"); }
        }

        private bool rampIsCheckedValue = false;
        public bool RampIsChecked
        {
            get { return rampIsCheckedValue; }
            set
            {
                if (value == true)
                {
                    RampSettingVisibility = Visibility.Visible;
                    RampTextBlockVisibility = Visibility.Visible;
                }
                else
                {
                    RampSettingVisibility = Visibility.Hidden;
                    RampSetting = null;
                    RampTextBlockVisibility = Visibility.Hidden;
                }

                rampIsCheckedValue = value;
                OnPropertyChanged("RampIsChecked");
            }
        }

        private string rampSettingValue = null;
        public string RampSetting
        {
            get { return rampSettingValue; }
            set { rampSettingValue = value; OnPropertyChanged("RampSetting"); }
        }

        private Visibility rampSettingVisibilityValue = Visibility.Hidden;
        public Visibility RampSettingVisibility
        {
            get { return rampSettingVisibilityValue; }
            set { rampSettingVisibilityValue = value; OnPropertyChanged("RampSettingVisibility"); }
        }

        private Visibility valueTextboxVisibilityValue = Visibility.Visible;
        public Visibility valueTextboxVisibility
        {
            get { return valueTextboxVisibilityValue; }
            set { valueTextboxVisibilityValue = value; OnPropertyChanged("valueTextboxVisibility"); }
        }

        private Visibility valueComboboxVisibilityValue = Visibility.Hidden;
        public Visibility valueComboboxVisibility
        {
            get { return valueComboboxVisibilityValue; }
            set { valueComboboxVisibilityValue = value; OnPropertyChanged("valueComboboxVisibility"); }
        }

        private Visibility rampcheckVisibilityValue = Visibility.Hidden;
        public Visibility RampCheckVisibility
        {
            get { return rampcheckVisibilityValue; }
            set { rampcheckVisibilityValue = value; OnPropertyChanged("RampCheckVisibility"); }
        }

        private Visibility loopcheckVisibilityValue = Visibility.Hidden;
        public Visibility LoopCheckVisibility
        {
            get { return loopcheckVisibilityValue; }
            set { loopcheckVisibilityValue = value; OnPropertyChanged("LoopCheckVisibility"); }
        }

        private bool ChannelEnabledValue = false;
        public bool ChannelEnabled
        {
            get { return ChannelEnabledValue; }
            set
            {
                //if (value == true)
                //    

                //else
                //{
                //    LoopCheckVisibility = Visibility.Hidden;
                //    LoopIsChecked = false;

                //}
                ChannelEnabledValue = value;
                OnPropertyChanged("ChannelEnabled");
            }
        }

        private bool InputSelectionEnabledValue = false;
        public bool InputSelectionEnabled
        {
            get { return InputSelectionEnabledValue; }
            set { InputSelectionEnabledValue = value; OnPropertyChanged("InputSelectionEnabled"); }
        }

        private bool valueIsEnabledValue = false;
        public bool valueIsEnabled
        {
            get { return valueIsEnabledValue; }
            set { valueIsEnabledValue = value; OnPropertyChanged("valueIsEnabled"); }
        }


        private string MaximumLimitValue = string.Empty;
        public string MaximumLimit
        {
            get { return MaximumLimitValue; }
            set
            {
                MaximumLimitValue = value;
                OnPropertyChanged("MaximumLimit");
            }
        }

        private string MinimumLimitValue = string.Empty;
        public string MinimumLimit
        {
            get { return MinimumLimitValue; }
            set
            {
                MinimumLimitValue = value;
                OnPropertyChanged("MinimumLimit");
            }
        }

        private bool MaxLimitIsEnabledValue = false;
        public bool MaxLimitIsEnabled
        {
            get { return MaxLimitIsEnabledValue; }
            set { MaxLimitIsEnabledValue = value; OnPropertyChanged("MaxLimitIsEnabled"); }
        }

        private bool MinLimitIsEnabledValue = false;
        public bool MinLimitIsEnabled
        {
            get { return MinLimitIsEnabledValue; }
            set { MinLimitIsEnabledValue = value; OnPropertyChanged("MinLimitIsEnabled"); }
        }

        private int valueMaxLengthValue = 20;
        public int valueMaxLength
        {
            get { return valueMaxLengthValue; }
            set { valueMaxLengthValue = value; OnPropertyChanged("valueMaxLength"); }
        }

        private string DataTypeTextBlockValue = string.Empty;
        public string DataTypeTextBlock
        {
            get { return DataTypeTextBlockValue; }
            set
            {
                DataTypeTextBlockValue = value;
                OnPropertyChanged("DataTypeTextBlock");
            }
        }

        public string Error
        {
            get
            {
                return "";
            }
        }

        public string this[string columnName]
        {
            get
            {
                try
                {
                    if ("TestControlPropertyInitialValueSelectedItem" == columnName)
                    {
                        string controlSelected = string.Empty;
                        if ((!string.IsNullOrEmpty(TestControlComponentTypeSelectedItem)) & string.IsNullOrEmpty(TestControlComponentNameSelectedItem) & (!string.IsNullOrEmpty(TestControlPropertyInitialValueSelectedItem)))
                        {
                            TestControlPropertyInitialValueSelectedItem = null;
                            return "Please select component Name";
                        }

                        string selectedPrettyName = ChannelSelectionSelectedItem;
                        string selectedControlID = string.Empty;
                        if ((TestControlPropertySelectedItem != null) & (!string.IsNullOrEmpty(TestControlPropertyInitialValueSelectedItem)))//&(ChannelSelectionSelectedItem != null)
                        {
                            if ((!string.IsNullOrEmpty(TestControlPropertySelectedItem))  & (!string.IsNullOrEmpty((ChannelSelectionSelectedItem))))//& ((TestControlPropertySelectedItem.StartsWith("CHANNEL ")) || (TestControlPropertySelectedItem.StartsWith("OUTPUT ")) || (TestControlPropertySelectedItem.StartsWith("INPUT ")) || (TestControlPropertySelectedItem.StartsWith("INPUT_OUTPUT ")) || (TestControlPropertySelectedItem.StartsWith("TAP ")) || (TestControlPropertySelectedItem.StartsWith("BANK_CONTROL ")) || (TestControlPropertySelectedItem.StartsWith("BANK_SELECT ")) || (TestControlPropertySelectedItem.StartsWith("GPIO_INPUT ")) || (TestControlPropertySelectedItem.StartsWith("GPIO_OUTPUT ")))
                            {
                                string[] prettyControlName = null;
                                if(TestControlComponentNameSelectedItem != null && selectedPrettyName != null)
                                ParentTestActionItem.ControlIDDictionary.TryGetValue(TestControlComponentNameSelectedItem + TestControlPropertySelectedItem + selectedPrettyName, out prettyControlName);

                                if (prettyControlName != null)
                                {
                                    if(prettyControlName.Count() > 0)
                                        selectedPrettyName = prettyControlName[0];

                                    if(prettyControlName.Count() > 1)
                                        selectedControlID = prettyControlName[1];
                                }

                                controlSelected = selectedPrettyName + "~" + removeQATPrefix(TestControlPropertySelectedItem);

                                //if ((TestControlPropertySelectedItem.StartsWith("CHANNEL ")))
                                //    controlSelected = ChannelSelectionSelectedItem + "~" + removeQATPrefix(TestControlPropertySelectedItem);
                                //else if ((TestControlPropertySelectedItem.StartsWith("OUTPUT ")))
                                //    controlSelected = ChannelSelectionSelectedItem + "~" + TestControlPropertySelectedItem.Remove(0, 7);
                                //else if ((TestControlPropertySelectedItem.StartsWith("INPUT ")))
                                //    controlSelected = ChannelSelectionSelectedItem + "~" + TestControlPropertySelectedItem.Remove(0, 6);
                                //else if ((TestControlPropertySelectedItem.StartsWith("INPUT_OUTPUT ")))
                                //    controlSelected = ChannelSelectionSelectedItem + "~" + TestControlPropertySelectedItem.Remove(0, 13);
                                //else if ((TestControlPropertySelectedItem.StartsWith("TAP ")))
                                //    controlSelected = ChannelSelectionSelectedItem + "~" + TestControlPropertySelectedItem.Remove(0, 4);
                                //else if ((TestControlPropertySelectedItem.StartsWith("BANK_CONTROL_INPUT ")))
                                //    controlSelected = ChannelSelectionSelectedItem + "~" + TestControlPropertySelectedItem.Remove(0, 19);
                                //else if ((TestControlPropertySelectedItem.StartsWith("BANK_CONTROL_OUTPUT ")))
                                //    controlSelected = ChannelSelectionSelectedItem + "~" + TestControlPropertySelectedItem.Remove(0, 20);
                                //else if ((TestControlPropertySelectedItem.StartsWith("BANK_CONTROL_INPUT_OUTPUT ")))
                                //    controlSelected = ChannelSelectionSelectedItem + "~" + TestControlPropertySelectedItem.Remove(0, 26);
                                //else if ((TestControlPropertySelectedItem.StartsWith("BANK_SELECT_INPUT ")))
                                //    controlSelected = ChannelSelectionSelectedItem + "~" + TestControlPropertySelectedItem.Remove(0, 18);
                                //else if ((TestControlPropertySelectedItem.StartsWith("BANK_SELECT_OUTPUT ")))
                                //    controlSelected = ChannelSelectionSelectedItem + "~" + TestControlPropertySelectedItem.Remove(0, 19);
                                //else if (TestControlPropertySelectedItem.StartsWith("GPIO_INPUT "))
                                //    controlSelected = ChannelSelectionSelectedItem + "~" + TestControlPropertySelectedItem.Remove(0, 11);
                                //else if (TestControlPropertySelectedItem.StartsWith("GPIO_OUTPUT "))
                                //    controlSelected = ChannelSelectionSelectedItem + "~" + TestControlPropertySelectedItem.Remove(0, 12);
                            }
                            else
                            {
                                string[] prettyControlName = null;
                                if(TestControlComponentNameSelectedItem != null && TestControlPropertySelectedItem != null)
                                ParentTestActionItem.ControlIDDictionary.TryGetValue(TestControlComponentNameSelectedItem + TestControlPropertySelectedItem, out prettyControlName);
                                //selectedPrettyName = prettyControlName[0];

                                if(prettyControlName != null && prettyControlName.Count() > 0)
                                    selectedControlID = prettyControlName[1];

                                controlSelected = TestControlPropertySelectedItem;
                            }

                            string valueTypeSelected = InputSelectionComboSelectedItem;

                            string maxValue = string.Empty;
                            if (TestControlComponentNameSelectedItem != null && TestControlPropertySelectedItem != null)
                            ParentTestActionItem.MaximumControlValueDictionary.TryGetValue(TestControlComponentNameSelectedItem + controlSelected + selectedControlID, out maxValue);
                            
                            string minValue = string.Empty;
                            if (TestControlComponentNameSelectedItem != null && TestControlPropertySelectedItem != null)
                            ParentTestActionItem.MinimumControlValueDictionary.TryGetValue(TestControlComponentNameSelectedItem + controlSelected + selectedControlID, out minValue);

                            string valueDataType = string.Empty;
                            if (TestControlComponentNameSelectedItem != null && TestControlPropertySelectedItem != null)
                            ParentTestActionItem.dataTypeDictionary.TryGetValue(TestControlComponentNameSelectedItem + controlSelected + selectedControlID, out valueDataType);

                            if ((valueDataType == string.Empty) | (valueDataType == null))
                            {
                                ParentTestActionItem.VerifyDataTypeDictionary.TryGetValue(TestControlComponentNameSelectedItem + controlSelected + selectedControlID, out valueDataType);
                            }

                            bool datatype = false;
                            if (String.Equals("Float", valueDataType, StringComparison.InvariantCultureIgnoreCase))
                            {
                                datatype = IsTextAllowedForDecimal(TestControlPropertyInitialValueSelectedItem);
                            }
                            else if ((String.Equals("Boolean", valueDataType, StringComparison.InvariantCultureIgnoreCase)) | (String.Equals("Trigger", valueDataType, StringComparison.InvariantCultureIgnoreCase)))
                            {
                                datatype = !IsboolAllowed(TestControlPropertyInitialValueSelectedItem);
                            }
                            else if (String.Equals("Integer", valueDataType, StringComparison.InvariantCultureIgnoreCase))
                            {
                                datatype = !IsTextAllowed(TestControlPropertyInitialValueSelectedItem);
                            }
                            else if (String.Equals("Text", valueDataType, StringComparison.InvariantCultureIgnoreCase))
                            {
                                datatype = true;
                            }
                            else if (String.Equals("Unknown", valueDataType, StringComparison.InvariantCultureIgnoreCase))
                            {
                                datatype = true;
                            }
                            if (datatype & (String.Equals("set by value", valueTypeSelected, StringComparison.CurrentCultureIgnoreCase)))
                            {
                                DataTypeTextBlock = string.Empty;
                                if ((String.Equals("set by value", valueTypeSelected, StringComparison.CurrentCultureIgnoreCase)) & (String.Equals("Float", valueDataType, StringComparison.InvariantCultureIgnoreCase)))
                                {

                                    string textBoxValue = TestControlPropertyInitialValueSelectedItem;
                                    double _textValue;
                                    if (textBoxValue == "-")
                                    {
                                        _textValue = 0;
                                    }
                                    else if (textBoxValue == ".")
                                    {
                                        _textValue = 0.0;
                                    }
                                    else
                                    {
                                        _textValue = Convert.ToDouble(textBoxValue);
                                    }

                                    double _maxValue = Convert.ToDouble(maxValue);
                                    double _minValue = Convert.ToDouble(minValue);

                                    if (_textValue < _minValue)
                                    {
                                        if (_minValue < 10)
                                        {

                                            //TestControlPropertyInitialValueSelectedItem = null; //commented on 24-jun-2016
                                            //TestControlPropertyInitialValueSelectedItem = TestControlPropertyInitialValueSelectedItem.Substring(0, TestControlPropertyInitialValueSelectedItem.Length - 1);//commented on 04-Aug-2016
                                            return "" + valueDataType + " Datatype:Value should be greater than " + _minValue + "";
                                        }
                                        else
                                            return "" + valueDataType + " Datatype:Value should be greater than " + _minValue + "";
                                    }
                                    else if (_textValue > _maxValue)
                                    {

                                        //TestControlPropertyInitialValueSelectedItem = null;  //commented on 24-jun-2016
                                        //TestControlPropertyInitialValueSelectedItem = TestControlPropertyInitialValueSelectedItem.Substring(0, TestControlPropertyInitialValueSelectedItem.Length - 1);//commented on 04-Aug-2016
                                        return "" + valueDataType + " Datatype:Value should be lesser than " + _maxValue + "";
                                    }
                                    else if (String.IsNullOrEmpty(TestControlPropertyInitialValueSelectedItem))
                                    {
                                        return "Please enter value";
                                    }
                                    return "";
                                }
                                else if ((String.Equals("set by value", valueTypeSelected, StringComparison.CurrentCultureIgnoreCase)) & (String.Equals("Integer", valueDataType, StringComparison.InvariantCultureIgnoreCase)))
                                {

                                    string textBoxValue = TestControlPropertyInitialValueSelectedItem;
                                    double _textValue;
                                    if (textBoxValue == "-")
                                    {
                                        _textValue = 0;
                                    }
                                    else
                                    {
                                        _textValue = Convert.ToDouble(textBoxValue);
                                    }

                                    double _maxValue = Convert.ToDouble(maxValue);
                                    double _minValue = Convert.ToDouble(minValue);
                                    //if ((_textValue >= _minValue) & (_textValue < _maxValue))
                                    //{
                                    //    return TestControlPropertyInitialValueSelectedItem;
                                    //}
                                    if (_textValue < _minValue)
                                    {

                                        //TestControlPropertyInitialValueSelectedItem = null;   //commented on 24-jun-2016
                                        //TestControlPropertyInitialValueSelectedItem = TestControlPropertyInitialValueSelectedItem.Substring(0, TestControlPropertyInitialValueSelectedItem.Length - 1);//commented on 04-Aug-2016
                                        return "" + valueDataType + " Datatype:Value should be greater than " + _minValue + "";
                                    }
                                    else if (_textValue > _maxValue)
                                    {

                                        //TestControlPropertyInitialValueSelectedItem = null;//commented on 24-jun-2016
                                        //TestControlPropertyInitialValueSelectedItem = TestControlPropertyInitialValueSelectedItem.Substring(0, TestControlPropertyInitialValueSelectedItem.Length - 1);//commented on 04-Aug-2016
                                        return "" + valueDataType + " Datatype:Value should be lesser than " + _maxValue + "";
                                    }
                                    else if (String.IsNullOrEmpty(TestControlPropertyInitialValueSelectedItem))
                                    {
                                        return "Please enter value";
                                    }
                                    return "";
                                }
                                else if ((String.Equals("set by value", valueTypeSelected, StringComparison.CurrentCultureIgnoreCase)) & (String.Equals("Boolean", valueDataType, StringComparison.InvariantCultureIgnoreCase)))
                                {
                                    if (TestControlPropertyInitialValueSelectedItem.Length > 1)
                                    {
                                        TestControlPropertyInitialValueSelectedItem = TestControlPropertyInitialValueSelectedItem.Remove((TestControlPropertyInitialValueSelectedItem.Length) - (TestControlPropertyInitialValueSelectedItem.Length - 1));
                                    }
                                    if (TestControlPropertyInitialValueSelectedItem.Length == 1)
                                    {

                                        string textBoxValue = TestControlPropertyInitialValueSelectedItem;
                                        double _textValue;
                                        _textValue = Convert.ToDouble(textBoxValue);
                                        double _maxValue = Convert.ToDouble(maxValue);
                                        double _minValue = Convert.ToDouble(minValue);
                                        if (_textValue > _maxValue || _textValue < _minValue)
                                        {
                                            TestControlPropertyInitialValueSelectedItem = null;  //commented on 24-jun-2016
                                            return "" + valueDataType + " Datatype:Value should be either '" + minValue + "' or '" + maxValue + "'";
                                        }
                                        else
                                        {
                                            return "";
                                        }
                                    }
                                    else
                                    {
                                        //TestControlPropertyInitialValueSelectedItem = TestControlPropertyInitialValueSelectedItem.Remove(1, 1);//commented on 04-Aug-2016
                                        return "" + valueDataType + " Datatype:Value should be either '" + minValue + "' or '" + maxValue + "'";
                                    }

                                }
                                else if ((String.Equals("set by value", valueTypeSelected, StringComparison.CurrentCultureIgnoreCase)) & (String.Equals("Trigger", valueDataType, StringComparison.InvariantCultureIgnoreCase)))
                                {
                                    if (TestControlPropertyInitialValueSelectedItem.Length > 1)
                                    {
                                        TestControlPropertyInitialValueSelectedItem = TestControlPropertyInitialValueSelectedItem.Remove((TestControlPropertyInitialValueSelectedItem.Length) - (TestControlPropertyInitialValueSelectedItem.Length - 1));
                                    }
                                    if (TestControlPropertyInitialValueSelectedItem.Length == 1)
                                    {
                                        string textBoxValue = TestControlPropertyInitialValueSelectedItem;
                                        double _textValue;
                                        _textValue = Convert.ToDouble(textBoxValue);
                                        double _maxValue = Convert.ToDouble(maxValue);
                                        double _minValue = Convert.ToDouble(minValue);
                                        if (_textValue > _maxValue || _textValue < _minValue)
                                        {
                                            TestControlPropertyInitialValueSelectedItem = null;  //commented on 24-jun-2016
                                            return "" + valueDataType + " Datatype:Value should be either '" + minValue + "' or '" + maxValue + "'";
                                        }
                                        else
                                        {
                                            return "";
                                        }
                                    }
                                    else
                                    {
                                        //TestControlPropertyInitialValueSelectedItem = TestControlPropertyInitialValueSelectedItem.Remove(1, 1);//commented on 04-Aug-2016
                                        return "" + valueDataType + " Datatype:Value should be either '" + minValue + "' or '" + maxValue + "'";
                                    }
                                }
                                else if ((String.Equals("set by value", valueTypeSelected, StringComparison.CurrentCultureIgnoreCase)) & (String.Equals("Text", valueDataType, StringComparison.InvariantCultureIgnoreCase)))
                                {
                                    DataTypeTextBlock = valueDataType + " Datatype:Please enter Text";
                                    return "";
                                    //return "" + valueDataType + " Datatype:Please enter Text";
                                }
                                else if ((String.Equals("set by value", valueTypeSelected, StringComparison.CurrentCultureIgnoreCase)) & (String.Equals("Unknown", valueDataType, StringComparison.InvariantCultureIgnoreCase)))
                                {
                                    DataTypeTextBlock = valueDataType + " Datatype";
                                    return "";
                                    //return "" + valueDataType + " Datatype";
                                }
                                return "";

                            }
                            else if ((String.Equals("set by position", valueTypeSelected, StringComparison.CurrentCultureIgnoreCase)))
                            {
                                DataTypeTextBlock = string.Empty;
                                string textBoxValue = TestControlPropertyInitialValueSelectedItem;
                                double _textValue;
                                if (textBoxValue == "-")
                                {
                                    _textValue = 0;
                                }
                                else if (textBoxValue == ".")
                                {
                                    _textValue = 0.0;
                                }
                                else if (!double.TryParse(textBoxValue, out _textValue))
                                {
                                   //TestControlPropertyInitialValueSelectedItem = TestControlPropertyInitialValueSelectedItem.Substring(0, TestControlPropertyInitialValueSelectedItem.Length - 1);//commented on 30-Jan-2017
                                    return "Enter value between -1 to 1";
                                }
                                else
                                {
                                    _textValue = Convert.ToDouble(textBoxValue);
                                }

                                double _maxValue = 1.0;
                                double _minValue = -1.0;
                                if (_textValue > _maxValue || _textValue < _minValue)
                                {
                                    //TestControlPropertyInitialValueSelectedItem = TestControlPropertyInitialValueSelectedItem.Substring(0, TestControlPropertyInitialValueSelectedItem.Length - 1);//commented on 04-Aug-2016
                                    return "Enter value between -1 to 1";
                                }
                                else
                                {
                                    return "";
                                }

                            }
                            else if ((String.Equals("set by string", valueTypeSelected, StringComparison.CurrentCultureIgnoreCase)))
                            {
                                DataTypeTextBlock = string.Empty;
                                return "";
                            }
                            else if (((!(String.Equals("Boolean", valueDataType, StringComparison.CurrentCultureIgnoreCase))) & ((!(String.Equals("Trigger", valueDataType, StringComparison.CurrentCultureIgnoreCase))))) && ((String.Equals("set by string", valueTypeSelected, StringComparison.CurrentCultureIgnoreCase))))
                            {
                                return "";
                            }
                            else if (!datatype & (String.Equals("set by value", valueTypeSelected, StringComparison.CurrentCultureIgnoreCase)) & (String.Equals("Float", valueDataType, StringComparison.CurrentCultureIgnoreCase)))
                            {
                                TestControlPropertyInitialValueSelectedItem = null;//commented on 24-jun-2016
                                return "" + valueDataType + " Datatype:Please enter numerical value";
                            }
                            else if (!datatype & (String.Equals("set by value", valueTypeSelected, StringComparison.CurrentCultureIgnoreCase)) & (String.Equals("Integer", valueDataType, StringComparison.CurrentCultureIgnoreCase)))
                            {

                                TestControlPropertyInitialValueSelectedItem = null;//commented on 24-jun-2016
                                return "" + valueDataType + " Datatype:Please enter numerical value";
                            }
                            else if (!datatype & (String.Equals("set by value", valueTypeSelected, StringComparison.CurrentCultureIgnoreCase)) & (String.Equals("Boolean", valueDataType, StringComparison.CurrentCultureIgnoreCase)))
                            {
                                TestControlPropertyInitialValueSelectedItem = null;//commented on 24-jun-2016
                                return "" + valueDataType + " Datatype:Value should be either '" + minValue + "' or '" + maxValue + "'";

                            }
                            else if (!datatype & (String.Equals("set by value", valueTypeSelected, StringComparison.CurrentCultureIgnoreCase)) & (String.Equals("Trigger", valueDataType, StringComparison.CurrentCultureIgnoreCase)))
                            {
                                TestControlPropertyInitialValueSelectedItem = null;//commented on 24-jun-2016
                                return "" + valueDataType + " Datatype:Value should be either '" + minValue + "' or '" + maxValue + "'";
                            }
                            else
                            {
                                DataTypeTextBlock = string.Empty;
                                return "";
                            }

                        }
                        else
                        {
                            if ((!String.IsNullOrEmpty(TestControlPropertySelectedItem)) & (!String.IsNullOrEmpty(InputSelectionComboSelectedItem)))
                            {
                                if (InputSelectionComboSelectedItem != "Set by value")
                                    DataTypeTextBlock = string.Empty;
                                return "Please enter value";
                            }
                            else
                                return "";
                        }
                    }
                    else if ("MaximumLimit" == columnName)
                    {
                        if (MaximumLimit.Length == 1)
                        {
                            string textBoxValue = MaximumLimit;
                            if ((textBoxValue == "-") || (textBoxValue == "+")|| (textBoxValue == "."))
                            {
                                return "Please enter Value";
                            }
                            else
                                return "";
                        }
                        else if (MaximumLimit.Length == 2)
                        {
                            string textBoxValue = MaximumLimit;
                            if ((textBoxValue == "-.") || (textBoxValue == "+."))
                            {
                                return "Please enter Value";
                            }
                            else
                                return "";
                        }
                        else
                            return "";

                    }
                    else if ("MinimumLimit" == columnName)
                    {
                        if (MinimumLimit.Length == 1)
                        {
                            string textBoxValue = MinimumLimit;
                            if ((textBoxValue == "-") || (textBoxValue == "+") || (textBoxValue == "."))
                            {
                                return "Please enter Value";
                            }
                            else
                                return "";
                        }
                        else if (MinimumLimit.Length == 2)
                        {
                            string textBoxValue = MinimumLimit;
                            if ((textBoxValue == "-.") || (textBoxValue == "+."))
                            {
                                return "Please enter Value";
                            }
                            else
                                return "";
                        }
                        else
                            return "";
                    }
                    else if ("LoopStart" == columnName)
                    {
                        if (LoopIsChecked)
                        {
                            string loopStartText = LoopStart;
                            bool validText = false;
                            if (loopStartText != null && loopStartText != string.Empty)
                            {
                                validText = IsLoopTextAllowed(loopStartText);
                            }
                            else
                                return "";

                            if (validText)
                            {
                                if ((loopStartText != string.Empty) && ((ChannelSelectionSelectedItem != string.Empty)))
                                {
                                    Int32 _textloopstart = Convert.ToInt32(loopStartText);
                                    Int32 channelCount = ChannelSelectionList.Count;
                                    if (channelCount <= 0)
                                    {
                                        return "";
                                    }
                                    else if ((_textloopstart > 0) & (_textloopstart <= channelCount))
                                    {
                                        return "";
                                    }
                                    else if (_textloopstart == 0)
                                    {
                                        LoopStart = string.Empty;
                                        return "";//return "Enter value greater than 0";
                                    }
                                    else if (_textloopstart > channelCount)
                                    {
                                        LoopStart = string.Empty;
                                        return "";//return "Enter value lesser than channel count(count:" + channelCount + ")";
                                    }
                                    else
                                    {
                                        return "";
                                    }
                                }
                                else
                                    return "";
                            }
                            else
                            {
                                LoopStart = string.Empty;
                                return "";
                            }
                        }

                        return "";
                    }
                    else if ("LoopEnd" == columnName)
                    {
                        if (LoopIsChecked)
                        {
                            string loopEndText = LoopEnd;
                            bool validText = false;
                            if (loopEndText != null && loopEndText != string.Empty)
                            {
                                validText = IsLoopTextAllowed(loopEndText);
                            }
                            else
                                return "";

                            string loopstartText = LoopStart;
                            if (validText)
                            {
                                if (((loopstartText != string.Empty)) && ((loopEndText != string.Empty)) && ((ChannelSelectionSelectedItem != string.Empty)))
                                {
                                    Int32 _textloopstart = Convert.ToInt32(loopstartText);
                                    Int32 _textloopend = Convert.ToInt32(loopEndText);
                                    Int32 channelCount = ChannelSelectionList.Count;
                                    if ((_textloopstart > 0) && (_textloopend > 0) && (_textloopend <= channelCount) && (_textloopend > _textloopstart))
                                    {
                                        return "";
                                    }
                                    else if (_textloopstart == 0)
                                    {
                                        LoopEnd = string.Empty;
                                        return "";//return "Enter LoopStart value before entering loop end";
                                    }
                                    else if ((_textloopend < _textloopstart))
                                    {
                                        if ((channelCount < 10))
                                        {
                                            LoopEnd = string.Empty;
                                        }
                                        else if ((channelCount >= 10) && (_textloopend > channelCount))
                                        {
                                            LoopEnd = string.Empty;
                                        }
                                        return "";
                                    }
                                    else if (_textloopend == 0)
                                    {
                                        LoopEnd = string.Empty;
                                        return "";//return "Enter value greater than 0";
                                    }
                                    else if (_textloopend > channelCount)
                                    {
                                        LoopEnd = string.Empty;
                                        return "";//return "Enter value lesser than or equal to channel count(count:" + channelCount + ")";
                                    }

                                    else
                                    {
                                        return "";
                                    }

                                }
                                else if ((loopstartText == string.Empty) && (loopEndText != null) && (loopEndText != string.Empty))
                                {
                                    LoopEnd = string.Empty;
                                    return "";//return "Enter LoopStart value before entering loop end";
                                }
                                else
                                    return "";
                            }
                            else
                            {
                                LoopEnd = string.Empty;
                                return "";//return "when alphebets or specialcharacters entered in loop end";
                            }
                        }
                        else
                            return "";

                    }
                    else if ("LoopIncrement" == columnName)
                    {
                        if (LoopIsChecked)
                        {
                            string loopIncrText = LoopIncrement;
                            string loopEndText = LoopEnd;
                            bool validText = false;
                            if (loopIncrText != null && loopIncrText != string.Empty)
                            {
                                validText = IsLoopTextAllowed(loopIncrText);
                            }
                            else
                                return "";

                            //if(!string.IsNullOrEmpty(ChannelSelectionSelectedItem))
                            //{
                            if (validText)
                            {
                                Int32 _textloopincr = 0;
                                if (!string.IsNullOrEmpty(loopIncrText))
                                    _textloopincr = Convert.ToInt32(loopIncrText);


                                if ((!string.IsNullOrEmpty(loopIncrText)) && ((ChannelSelectionSelectedItem != string.Empty)) && (!string.IsNullOrEmpty(LoopStart)) && (!string.IsNullOrEmpty(LoopEnd)))
                                {
                                    Int32 _textloopend = Convert.ToInt32(loopEndText);
                                    Int32 channelCount = ChannelSelectionList.Count;
                                    if ((_textloopincr > 0) && (_textloopincr <= channelCount) && (_textloopincr <= _textloopend))
                                    {
                                        return "";
                                    }
                                    else if (_textloopincr == 0)
                                    {
                                        LoopIncrement = string.Empty;
                                        return "";//return "Enter the value greater than 0";
                                    }
                                    else if ((_textloopincr > channelCount) || (_textloopincr >= _textloopend))
                                    {
                                        LoopIncrement = string.Empty;
                                        return "";//return "Enter the value lesser than or equal to channel count(count:" + channelCount + ")";
                                    }
                                    else
                                    {
                                        return "";
                                    }
                                }
                                else if (loopEndText == string.Empty && (loopIncrText != null) && (loopIncrText != string.Empty))
                                {
                                    LoopIncrement = string.Empty;
                                    return "";
                                }
                                else
                                    return "";

                            }
                            else
                            {
                                LoopIncrement = string.Empty;
                                return "";
                            }

                        }
                        else
                            return "";
                    }
                    else
                        return "";
                }
                catch (Exception ex)
                {
                    DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif

                    //MessageBox.Show("Exception\n " + ex.Message, "Error Code - ECxxxxx", MessageBoxButton.OK, MessageBoxImage.Error);
                    return "";
                }

            }
        }


        private static bool IsTextAllowedForDecimal(string text)
        {
            Regex regex = null;

            try
            {
                regex = new Regex(@"^-?[0-9]*(?:\.[0-9]*)?$");//^[-+]?[0 - 9] | 0\d * (\.\d +)?$///regex that matches disallowed text
                //regex = new Regex(@"^[-+]?[0 - 9] \d * (\.\d +)?$");//^[-+]?[0 - 9] | 0\d * (\.\d +)?$///regex that matches disallowed text
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif

                //MessageBox.Show("Exception\n " + ex.Message, "QAT Error Code - EC12019", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return regex.IsMatch(text);

        }

        private static bool IsboolAllowed(string text)
        {
            Regex regex = null;

            try
            {
                regex = new Regex("^[0-1]"); ////regex that matches disallowed text
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif

                //MessageBox.Show("Exception\n " + ex.Message, "QAT Error Code - EC12019", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return !regex.IsMatch(text);
        }

        private static bool IsTextAllowed(string text)
        {
            Regex regex = null;

            try
            {
                regex = new Regex("^[+-]?[0-9]*$"); ////regex that matches disallowed text
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif

                //MessageBox.Show("Exception\n " + ex.Message, "QAT Error Code - EC12019", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return !regex.IsMatch(text);
        }

        private static bool IsLoopTextAllowed(string text)
        {
            Regex regex = null;

            try
            {
                regex = new Regex("[^0-9]+"); ////regex that matches disallowed text
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif

                //MessageBox.Show("Exception\n " + ex.Message, "QAT Error Code - EC12019", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return !regex.IsMatch(text);

        }

        private static bool IsPositionAllowed(string text)
        {
            Regex regex = null;

            try
            {
                regex = new Regex(@"^-?(0(\.\d+)?|1(\.0+)?)$"); ////regex that matches disallowed text
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif

                //MessageBox.Show("Exception\n " + ex.Message, "QAT Error Code - EC12052", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            bool stat = regex.IsMatch(text);
            return regex.IsMatch(text);
        }

        public string removeQATPrefix(string controlWithQATPrefix)
        {
            try
            {
                string[] controlWithoutQATPrefix = new string[2];
                string channelControl = string.Empty;
                int spaceCount = controlWithQATPrefix.Count(x => x == ' ');
                string channelWithTwoTilt = controlWithQATPrefix;
                int idx = channelWithTwoTilt.IndexOf(' ');
                controlWithoutQATPrefix[0] = channelWithTwoTilt.Substring(0, idx);
                controlWithoutQATPrefix[1] = channelWithTwoTilt.Substring(idx + 1);
                return controlWithoutQATPrefix[1];
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                return string.Empty;
            }

        }
    }



}

