using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.ComponentModel;
using System.Windows;
using Microsoft.Win32;
using System.Text.RegularExpressions;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Input;
using System.Collections;
using System.Windows.Documents;
using System.Windows.Data;
using System.Globalization;

namespace QSC_Test_Automation
{
    public class TestActionItem : INotifyPropertyChanged
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

                    if (ParentTestCaseItem != null && ParentTestCaseItem.SaveButtonIsEnabled == false && isSkipSaveButtonEnable == false)
                        ParentTestCaseItem.SaveButtonIsEnabled = true;

                    isSkipSaveButtonEnable = false;
                }
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                // MessageBox.Show("Exception\n " + ex.Message, "Error Code - ECxxxxx", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public TestActionItem(TestCaseItem parentTestCaseItem)
        {
            try
            {
                ParentTestCaseItem = parentTestCaseItem;
                AddSetTestControlItem();
                AddSetTestTelnetItem();
                AddSetTestCECItem();
                AddSetTestUserItem();
               
                AddSetTestFirmwareItem();
                AddSetTestDesignerItem();
                AddSetTestNetPairingItem();
                AddSetTestUsbItem();

                AddVerifyTestControlItem();
                AddVerifyTestTelnetItem();
                AddVerifyTestUsbItem();
                AddVerifyTestLuaItem();
                AddVerifyTestLogItem();
                AddVerifyTestApxItem();
                AddVerifyTestCECItem();
                AddVerifyTestScriptItem();
                AddVerifyTestQRItem();
                AddTestUserVerifyItem();
              

                AddTestSaveLogItem();

                AddTestResponsalyzerItem();

                ActionSelected = ParentTestCaseItem.TestActionList[0];
                VerificationSelected = null;

                InitializeTestCaseNameTextBox();
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                // MessageBox.Show("Exception\n \n" + ex.Message, "QAT Error Code - ECxxxx", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void InitializeTestCaseNameTextBox()
        {
            TestActionItemNameTextBox.Background = Brushes.Transparent;
            TestActionItemNameTextBox.BorderThickness = new Thickness(0);
            TestActionItemNameTextBox.Margin = new Thickness(0);
            TestActionItemNameTextBox.Padding = new Thickness(0);
            TestActionItemNameTextBox.IsReadOnly = true;
            TestActionItemNameTextBox.Focusable = false;
            TestActionItemNameTextBox.SelectionLength = 0;
            TestActionItemNameTextBox.Cursor = Cursors.Arrow;
            TestActionItemNameTextBox.Height = Double.NaN;
            TestActionItemNameTextBox.MinHeight = 0;
            TestActionItemNameTextBox.KeyDown += TestCaseNameTextBox_KeyDown;
            TestActionItemNameTextBox.LostFocus += TestCaseNameTextBox_LostFocus;
            TestActionItemNameTextBox.TextChanged += TestCaseNameTextBox_TextChanged;
            TestActionItemNameTextBox.PreviewTextInput += TestCaseNameTextBox_PreviewTextInput;
            TestActionItemNameTextBox.MouseDoubleClick += TestCaseNameTextBox_MouseDoubleClick;
            TestActionItemNameTextBox.PreviewKeyDown += TestActionItemNameTextBox_PreviewKeyDown;
            TestActionItemNameTextBox.LostKeyboardFocus += TestActionItemNameTextBox_LostKeyboardFocus;
            TestActionItemNameTextBox.ContextMenu = null;
            TestActionItemNameTextBox.IsHitTestVisible = false;
        }

        private void TestActionItemNameTextBox_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            try
            {
                TextBox Text = e.OriginalSource as TextBox;
                if (Text != null)
                    RemoveAdornerFromTextBox(Text);
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                // MessageBox.Show("Exception\n \n" + ex.Message, "QAT Error Code - ECxxxx", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }

        private void TestActionItemNameTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            try
            { 
            string lstrCopyandPasteTxtBox = null;
            TextBox Text = e.OriginalSource as TextBox;
            AdornerLayer TxtAdornerLayer = null;
            var rect = Text.GetRectFromCharacterIndex(Text.CaretIndex);
            var point = rect.BottomRight;



            if (Text != null)
            {
                TxtAdornerLayer = AdornerLayer.GetAdornerLayer(Text);
                Warning = new TextblockWarning(TxtAdornerLayer, point.X, point.Y);
            }



            if (e.Key == Key.V && Keyboard.Modifiers == ModifierKeys.Control)
            {
                lstrCopyandPasteTxtBox = Clipboard.GetText();
                if (Regex.IsMatch(lstrCopyandPasteTxtBox, @"[\\/:*?<>'%|""[\]&]"))
                {
                    if (TxtAdornerLayer != null && Text != null)
                    {
                        Warning.Remove(TxtAdornerLayer, Text);
                        TxtAdornerLayer.Add(new TextblockWarning(Text, point.X, point.Y));

                    }
                    e.Handled = true;
                }
                else
                {
                    Warning.Remove(TxtAdornerLayer, Text);
                    e.Handled = false;
                }

              
            }
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                // MessageBox.Show("Exception\n \n" + ex.Message, "QAT Error Code - ECxxxx", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void TestCaseNameTextBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {

            TestActionItemNameCopy = TestActionItemName;
            TestActionItemNameTextBox.IsReadOnly = false;
            TestActionItemNameTextBox.Focusable = true;
            TestActionItemNameTextBox.Cursor = Cursors.IBeam;
            TestActionItemNameTextBox.Background = Brushes.LightGray;
            ParentTestCaseItem.SaveButtonIsDefault = false;

            e.Handled = true;
        }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                // MessageBox.Show("Exception\n \n" + ex.Message, "QAT Error Code - ECxxxx", MessageBoxButton.OK, MessageBoxImage.Error);
            }
}

        private void TestCaseNameTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    RenameItem();
                    e.Handled = true;
                }
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                // MessageBox.Show("Exception\n " + ex.Message, "Error Code - EC18002", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void TestCaseNameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                TextBox Text = e.OriginalSource as TextBox;
                if (Text != null)
                    RemoveAdornerFromTextBox(Text);
                TestActionItemName = TestActionItemNameTextBox.Text;
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                // MessageBox.Show("Exception\n \n" + ex.Message, "QAT Error Code - ECxxxx", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
     
        TextblockWarning Warning = null;
        private void TestCaseNameTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            try
            {
                TextBox Text = e.OriginalSource as TextBox;
                AdornerLayer TxtAdornerLayer = null;
                var rect = Text.GetRectFromCharacterIndex(Text.CaretIndex);
                var point = rect.BottomRight;
                if (Text != null)
                {
                    TxtAdornerLayer = AdornerLayer.GetAdornerLayer(Text);
                    Warning = new TextblockWarning(TxtAdornerLayer, point.X, point.Y);
                }

                if (Regex.IsMatch(e.Text, @"[\\/:*?<>'%|""[\]&]"))
                {
                    if (TxtAdornerLayer != null && Text != null)
                    {
                        Warning.Remove(TxtAdornerLayer, Text);
                        TxtAdornerLayer.Add(new TextblockWarning(Text, point.X, point.Y));
                    }
                    e.Handled = true;
                }
                else
                {
                    Warning.Remove(TxtAdornerLayer, Text);
                    e.Handled = false;
                }
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                // MessageBox.Show("Exception\n \n" + ex.Message, "QAT Error Code - ECxxxx", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }

   
        private void RemoveAdornerFromTextBox(TextBox Text)
        {
            try
            {
                AdornerLayer TxtAdornerLayer = null;
                TxtAdornerLayer = AdornerLayer.GetAdornerLayer(Text);
                if (Warning != null)
                    Warning.Remove(TxtAdornerLayer, Text);
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                // MessageBox.Show("Exception\n " + ex.Message, "Error Code - EC18003", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void TestCaseNameTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                TextBox Text = e.OriginalSource as TextBox;
                if(Text!=null)
                RemoveAdornerFromTextBox(Text);
                if (TestActionItemNameTextBox.IsVisible == true)
                    RenameItem();
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                // MessageBox.Show("Exception\n " + ex.Message, "Error Code - EC18003", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void RenameItem()
        {
            try
            {
                if (TestActionItemNameTextBox.Focusable == true)
                {
                    TestActionItemNameTextBox.IsHitTestVisible = false;
                    TestActionItemNameTextBox.IsReadOnly = true;
                    TestActionItemNameTextBox.Focusable = false;
                    TestActionItemNameTextBox.Cursor = Cursors.Arrow;
                    TestActionItemNameTextBox.Background = Brushes.Transparent;
                    TestActionItemNameTextBox.SelectionLength = 0;

                    TestActionItemNameTextBox.Text = TestActionItemNameTextBox.Text.Trim();
                    TestActionItemName = TestActionItemNameTextBox.Text;

                    if (TestActionItemName.Trim() == string.Empty)
                    {
                        MessageBox.Show("Name is empty\n", "Warning", MessageBoxButton.OK, MessageBoxImage.Error);
                        TestActionItemName = TestActionItemNameCopy;
                        TestActionItemNameTextBox.Text = TestActionItemNameCopy;
                    }
                    else if (TestActionItemName.Count() > 100)
                    {
                        MessageBox.Show("Name should be less than 100 characters\n", "Warning", MessageBoxButton.OK, MessageBoxImage.Error);
                        TestActionItemName = TestActionItemNameCopy;
                        TestActionItemNameTextBox.Text = TestActionItemNameCopy;
                    }
                    else
                    {
                        foreach (TestActionItem item in ParentTestCaseItem.TestActionItemList)
                        {
                            if (!item.Equals(this))
                            {
                                if (String.Equals(item.TestActionItemName, TestActionItemName, StringComparison.CurrentCultureIgnoreCase))
                                {
                                    MessageBox.Show("Name already exists.\nPlease rename", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                                    TestActionItemName = TestActionItemNameCopy;
                                    TestActionItemNameTextBox.Text = TestActionItemNameCopy;
                                    break;
                                }
                            }
                        }
						
                        ParentTestCaseItem.SaveButtonIsDefault = true;
                        TestActionItemNameCopy = TestActionItemName;
                    }

                }
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                // MessageBox.Show("Exception\n " + ex.Message, "Error Code - ECxxxxx", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void RemoveAllSetAndVerifyTestControlItems()
        {
            try
            {
                SetTestControlList.Clear();
                SetTestTelnetList.Clear();
                SetTestCECList.Clear();
                SetTestFirmwareList.Clear();
                SetTestDesignerList.Clear();
                SetTestNetPairingList.Clear();
                SetTestUsbList.Clear();
                SetTestQRCMActionList.Clear();
                SetTestUserActionList.Clear();
                VerifyTestUserList.Clear();
                VerifyTestControlList.Clear();
                VerifyTestTelnetList.Clear();
                VerifyTestCECList.Clear();
                VerifyTestScriptList.Clear();
                VerifyTestUsbList.Clear();
                VerifyTestLuaList.Clear();
                VerifyTestLogList.Clear();
                VerifyTestApxList.Clear();
                VerifyTestQRList.Clear();
                VerifyTestQRCMList.Clear();

                TestSaveLogItemList.Clear();
                verifyTestResponsalyzerList.Clear();
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                //MessageBox.Show("Exception\n \n" + ex.Message, "QAT Error Code - ECxxxx", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public TestControlItem AddSetTestControlItem()
        {

            TestControlItem setTestControlItem = new TestControlItem();
            try
            {
                setTestControlItem.ParentTestActionItem = this;
                ObservableCollection<string> TempCustomControlDisableList = new ObservableCollection<string>();
                List<string> readcontrolsremovedlist = new List<string>(ParentTestCaseItem.ComponentTypeList);
                foreach (string compstr in readcontrolsremovedlist.ToList())
                {
                    ObservableCollection<string> ComponentNameList = ParentTestCaseItem.ComponentNameDictionary[compstr];
                    if (ComponentNameList.Count < 2)
                    {
                        foreach (string componentName in ComponentNameList)
                        {
                            ObservableCollection<string> emptystring;
                            Dictionary<string, ObservableCollection<string>> ComponentcontrolList = ParentTestCaseItem.ControlNameDictionary;
                            ComponentcontrolList.TryGetValue(componentName, out emptystring);

                            if (emptystring.Count == 0)
                            {
                                if (readcontrolsremovedlist != null)
                                {
                                    readcontrolsremovedlist.Remove(compstr);
                                }  
                            } 
                        }
                    }
                    else
                    {
                        foreach (string componentName in ComponentNameList)
                        {
                            ObservableCollection<string> emptystring;
                            Dictionary<string, ObservableCollection<string>> ComponentcontrolList = ParentTestCaseItem.ControlNameDictionary;
                            ComponentcontrolList.TryGetValue(componentName, out emptystring);

                            if (emptystring.Count == 0)
                            {
                                if (readcontrolsremovedlist != null)
                                {
                                    if (compstr == "Custom Controls")
                                    {
                                        TempCustomControlDisableList.Add(componentName);
                                    }
                                    else
                                    {
                                        readcontrolsremovedlist.Remove(compstr);
                                        break;
                                    }
                                }
                            }
                        }
                        
                    }
                    if (TempCustomControlDisableList.Count > 0 && compstr == "Custom Controls")
                    {
                        var CheckMatch = ComponentNameList.ToList().Except(TempCustomControlDisableList.ToList());
                        if (CheckMatch.Count() == 0 && readcontrolsremovedlist.Count() > 0)
                        {
                            readcontrolsremovedlist.Remove(compstr);
                        }
                    }
                }
                setTestControlItem.CustomControlDisableList = TempCustomControlDisableList;
                ObservableCollection<string> ComponentTypeList = new ObservableCollection<string>(readcontrolsremovedlist.OrderBy(a => a));
                //ObservableCollection<string> ComponentTypeList = new ObservableCollection<string>(ParentTestCaseItem.ComponentTypeList.OrderBy(a => a));
                setTestControlItem.TestControlComponentTypeList = ComponentTypeList;
                //setTestControlItem.TestControlComponentTypeList = new ObservableCollection<string>(ParentTestCaseItem.ComponentTypeList);//added like this upto ver 1.19
                //setTestControlItem.InputSelectionEnabled = false;
                SetTestControlList.Add(setTestControlItem);

                if(SetTestControlList.Count< 2)
                {
                    TestActionGridHeight = new GridLength(130, GridUnitType.Pixel);
                }
                        else if (SetTestControlList.Count == 2)
                {
                    TestActionGridHeight = new GridLength(260, GridUnitType.Pixel);
                }
                else if (SetTestControlList.Count > 2)
                {
                    TestActionGridHeight = new GridLength(380, GridUnitType.Pixel);
                }
                return setTestControlItem;
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                //MessageBox.Show("Exception\n " + ex.Message, "QAT Error Code - EC03021", MessageBoxButton.OK, MessageBoxImage.Error);
                return setTestControlItem;
            }
        }

        public TestControlItem AddSetTestControlItem(TestControlItem sourceTestControlItem)
        {
            TestControlItem setTestControlItem = CopyTestControlItem(sourceTestControlItem);
            SetTestControlList.Insert(SetTestControlList.IndexOf(sourceTestControlItem), setTestControlItem);
            if (SetTestControlList.Count < 2)
            {
                TestActionGridHeight = new GridLength(130, GridUnitType.Pixel);
            }
            else if (SetTestControlList.Count == 2)
            {
                TestActionGridHeight = new GridLength(260, GridUnitType.Pixel);
            }
            else if (SetTestControlList.Count > 2)
            {
                TestActionGridHeight = new GridLength(380, GridUnitType.Pixel);
            }
            return setTestControlItem;
        }

        public void RemoveSetTestControlItem(TestControlItem removeItem)
        {
            try
            {
                if (SetTestControlList.Contains(removeItem))
                    SetTestControlList.Remove(removeItem);
                if (SetTestControlList.Count < 2)
                {
                    TestActionGridHeight = new GridLength(130, GridUnitType.Pixel);
                }
                else if (SetTestControlList.Count == 2)
                {
                    TestActionGridHeight = new GridLength(260, GridUnitType.Pixel);
                }
                else if (SetTestControlList.Count > 2)
                {
                    TestActionGridHeight = new GridLength(380, GridUnitType.Pixel);
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

        public TestTelnetItem AddSetTestTelnetItem()
        {
            TestTelnetItem setTestTelnetItem = new TestTelnetItem();

            try
            {
                setTestTelnetItem.ParentTestActionItem = this;

                ObservableCollection<DUT_DeviceItem> telnetDeviceList = new ObservableCollection<DUT_DeviceItem>();
                foreach (var item in ParentTestCaseItem.SelectedDeviceItemList)
                {
                    telnetDeviceList.Add(new DUT_DeviceItem(item));
                    if (!setTestTelnetItem.TelnetSelectedDeviceItem.Contains("All devices"))
                        setTestTelnetItem.TelnetSelectedDeviceItem.Add("All devices");
                    if (!setTestTelnetItem.TelnetSelectedDeviceItem.Contains("Video Gen1-PGAVHD"))
                        setTestTelnetItem.TelnetSelectedDeviceItem.Add("Video Gen1-PGAVHD");
                    if (!setTestTelnetItem.TelnetSelectedDeviceItem.Contains(item.ItemDeviceName))
                        setTestTelnetItem.TelnetSelectedDeviceItem.Add(item.ItemDeviceName);
                    if (!setTestTelnetItem.TelnetSelectedDeviceModel.Keys.Contains(item.ItemDeviceName))
                        setTestTelnetItem.TelnetSelectedDeviceModel.Add(item.ItemDeviceName, item.ItemDeviceModel);
                }

                setTestTelnetItem.TelnetDeviceItem = telnetDeviceList;

                SetTestTelnetList.Add(setTestTelnetItem);
                return setTestTelnetItem;
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                //MessageBox.Show("Exception\n " + ex.Message, "QAT Error Code - EC03021", MessageBoxButton.OK, MessageBoxImage.Error);
                return setTestTelnetItem;
            }
        }


        public TestActionQRCMItem AddSetTestQRCMItem()
        {
            TestActionQRCMItem setTestQRCMItem = new TestActionQRCMItem();
            try
            {
                setTestQRCMItem.ParentTestActionItem = this;
                foreach (var item in ParentTestCaseItem.SelectedDeviceItemList)
                {
                    if (item.ItemDeviceType.ToUpper() == "CORE")
                    {
                        if (item.ItemPrimaryorBackup.ToUpper() == "PRIMARY" && !setTestQRCMItem.ActionQRCM_DevicesList.Contains("Primary core"))
                        {
                            setTestQRCMItem.ActionQRCM_DevicesList.Add("Primary core");
                            setTestQRCMItem.QRCM_DeviceModel.Add("Primary core", item.ItemDeviceModel);
                        }
                        else if (item.ItemPrimaryorBackup.ToUpper() == "BACKUP" && !setTestQRCMItem.ActionQRCM_DevicesList.Contains("Backup core"))
                        {
                            setTestQRCMItem.ActionQRCM_DevicesList.Add("Backup core");
                            setTestQRCMItem.QRCM_DeviceModel.Add("Backup core", item.ItemDeviceModel);
                        }                    
                    }                 
                }

                ////////for new action get all method names and corresponding values from QRCMDictionary
                if (setTestQRCMItem.ParentTestActionItem.ActionQRCMVersionSelected != null && setTestQRCMItem.ParentTestActionItem.ActionQRCMVersionSelected != string.Empty)
                {
                    if (ParentTestCaseItem.QRCMDictionary.Count > 0 && ParentTestCaseItem.QRCMDictionary.Keys.Contains(setTestQRCMItem.ParentTestActionItem.ActionQRCMVersionSelected))
                    {
                        ObservableCollection<QRCMInitialValues> CurrentVersionValues = ParentTestCaseItem.QRCMDictionary[setTestQRCMItem.ParentTestActionItem.ActionQRCMVersionSelected];
                        List<string> methodNames = new List<string>();
                        foreach (QRCMInitialValues item in CurrentVersionValues)
                        {
                            if (item.IsActionTrue)
                            {
                                methodNames.Add(item.MethodNameUserView);
                                setTestQRCMItem.QRCM_MethodsInitialValues.Add(item);
                            }
                        }
                        string[] methodNamesArraylist = methodNames.ToArray();
                        Array.Sort(methodNamesArraylist, new AlphanumComparatorFaster());
                        ObservableCollection<string> methodsNameList = new ObservableCollection<string>(methodNamesArraylist.ToList());
                        setTestQRCMItem.ActionQRCM_MethodsList = methodsNameList;
                    }
                }                

                SetTestQRCMActionList.Add(setTestQRCMItem);
                return setTestQRCMItem;
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                //MessageBox.Show("Exception\n " + ex.Message, "QAT Error Code - EC03021", MessageBoxButton.OK, MessageBoxImage.Error);
                return setTestQRCMItem;
            }
        }

        public TestActionQRCMItem AddSetTestQRCMItem(TestActionQRCMItem sourceTestQRCMItem)
        {
            try
            {
                TestActionQRCMItem setTestQRCMItem = CopyTestQRCMItem(sourceTestQRCMItem);             
                SetTestQRCMActionList.Insert(SetTestQRCMActionList.IndexOf(sourceTestQRCMItem), setTestQRCMItem);
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
            }
            return sourceTestQRCMItem;
        }

        public TestActionQRCMItem CopyTestQRCMItem(TestActionQRCMItem sourceTestQRCMItem)
        {
            TestActionQRCMItem targetTestQRCMItem = new TestActionQRCMItem();
            try
            {
                targetTestQRCMItem.ParentTestActionItem = sourceTestQRCMItem.ParentTestActionItem;

                if (sourceTestQRCMItem.ActionQRCM_DevicesList != null)
                    targetTestQRCMItem.ActionQRCM_DevicesList = new ObservableCollection<string>(sourceTestQRCMItem.ActionQRCM_DevicesList);

                if (sourceTestQRCMItem.ActionQRCM_MethodsList != null)
                    targetTestQRCMItem.ActionQRCM_MethodsList = new ObservableCollection<string>(sourceTestQRCMItem.ActionQRCM_MethodsList);

                if (sourceTestQRCMItem.QRCM_MethodsInitialValues != null)
                    targetTestQRCMItem.QRCM_MethodsInitialValues = new ObservableCollection<QRCMInitialValues>(sourceTestQRCMItem.QRCM_MethodsInitialValues);

                targetTestQRCMItem.QRCM_MethodsSelectedItem = sourceTestQRCMItem.QRCM_MethodsSelectedItem;
                targetTestQRCMItem.QRCM_DeviceSelectedItem = sourceTestQRCMItem.QRCM_DeviceSelectedItem;
                targetTestQRCMItem.ActionUserArguments = sourceTestQRCMItem.ActionUserArguments;
                targetTestQRCMItem.ArgumentsTextboxIsEnabled = sourceTestQRCMItem.ArgumentsTextboxIsEnabled;
                targetTestQRCMItem.SetPayloadBtnIsEnabled = sourceTestQRCMItem.SetPayloadBtnIsEnabled;
                targetTestQRCMItem.QRCM_DeviceModel = new Dictionary<string, string>( sourceTestQRCMItem.QRCM_DeviceModel);
                targetTestQRCMItem.SetPayloadContent = sourceTestQRCMItem.SetPayloadContent;

                return targetTestQRCMItem;
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                //MessageBox.Show("Exception\n " + ex.Message, "QAT Error Code - EC03021", MessageBoxButton.OK, MessageBoxImage.Error);
                return targetTestQRCMItem;
            }
        }

        public void RemoveSetTestQRCMItem(TestActionQRCMItem removeItem)
        {
            try
            {
                if (SetTestQRCMActionList.Contains(removeItem))
                    SetTestQRCMActionList.Remove(removeItem);
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

        public TestVerifyQRCMItem AddVerifyTestQRCMItem(TestVerifyQRCMItem sourceTestQRCMItem)
        {
            try
            {
                TestVerifyQRCMItem VerifyTestQRCMItem = CopyTestVerifyQRCMItem(sourceTestQRCMItem);          
                VerifyTestQRCMList.Insert(VerifyTestQRCMList.IndexOf(sourceTestQRCMItem), VerifyTestQRCMItem);
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
            }
            return sourceTestQRCMItem;
        }

        public TestVerifyQRCMItem CopyTestVerifyQRCMItem(TestVerifyQRCMItem sourceTestQRCMItem)
        {
            TestVerifyQRCMItem targetTestQRCMItem = new TestVerifyQRCMItem();

            try
            {
                targetTestQRCMItem.ParentTestActionItem = sourceTestQRCMItem.ParentTestActionItem;

                if (sourceTestQRCMItem.VerifyQRCM_MethodsList != null)
                    targetTestQRCMItem.VerifyQRCM_MethodsList = new ObservableCollection<string>(sourceTestQRCMItem.VerifyQRCM_MethodsList);

                if (sourceTestQRCMItem.VerifyQRCM_DevicesList != null)
                    targetTestQRCMItem.VerifyQRCM_DevicesList = new ObservableCollection<string>(sourceTestQRCMItem.VerifyQRCM_DevicesList);

                if (sourceTestQRCMItem.QRCM_MethodsInitialValues != null)
                    targetTestQRCMItem.QRCM_MethodsInitialValues = new ObservableCollection<QRCMInitialValues>(sourceTestQRCMItem.QRCM_MethodsInitialValues);


                targetTestQRCMItem.QRCM_MethodsSelectedItem = sourceTestQRCMItem.QRCM_MethodsSelectedItem;
                targetTestQRCMItem.QRCM_DeviceSelectedItem = sourceTestQRCMItem.QRCM_DeviceSelectedItem;
                targetTestQRCMItem.VerifyUserArguments = sourceTestQRCMItem.VerifyUserArguments;
                targetTestQRCMItem.ArgumentsTextboxIsEnabled = sourceTestQRCMItem.ArgumentsTextboxIsEnabled;
                targetTestQRCMItem.SetReferenceBtnIsEnabled = sourceTestQRCMItem.SetReferenceBtnIsEnabled;
                targetTestQRCMItem.QRCM_DeviceModel = new Dictionary<string, string>(sourceTestQRCMItem.QRCM_DeviceModel);
                targetTestQRCMItem.SetReferenceContent = sourceTestQRCMItem.SetReferenceContent;
                
                return targetTestQRCMItem;
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                //MessageBox.Show("Exception\n " + ex.Message, "QAT Error Code - EC03021", MessageBoxButton.OK, MessageBoxImage.Error);
                return targetTestQRCMItem;
            }
        }

        public void RemoveVerifyTestQRCMItem(TestVerifyQRCMItem removeItem)
        {
            try
            {
                if (VerifyTestQRCMList.Contains(removeItem))
                    VerifyTestQRCMList.Remove(removeItem);
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



        public TestUserActionItem AddSetTestUserItem()
        {
            TestUserActionItem setTestUserItem = new TestUserActionItem();
            try
            {
                setTestUserItem.ParentTestActionItem = this;

                SetTestUserActionList.Add(setTestUserItem);

                return setTestUserItem;
            }
            catch(Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                //MessageBox.Show("Exception\n " + ex.Message, "QAT Error Code - EC03021", MessageBoxButton.OK, MessageBoxImage.Error);
                return setTestUserItem;
            }
        }


        public TestUserActionItem AddSetTestUserItem(TestUserActionItem sourceTestUserItem)
        {
            try
            {
                TestUserActionItem setTestUserItem = CopyTestUserItem(sourceTestUserItem);
                SetTestUserActionList.Add(setTestUserItem);
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
            }
            return sourceTestUserItem;
        }

        public TestUserVerifyItem AddVerifyTestUserItem(TestUserVerifyItem sourceTestUserItem)
        {
            try
            {
                TestUserVerifyItem VerifyTestUserItem = CopyTestVerifyUserItem(sourceTestUserItem);
                VerifyTestUserList.Add(VerifyTestUserItem);
            }
            catch(Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
            }
            return sourceTestUserItem;
        }

        public TestUserActionItem CopyTestUserItem(TestUserActionItem sourceTestUserItem)
        {
            TestUserActionItem targetTestUserItem = new TestUserActionItem();
            try
            {
                targetTestUserItem.ParentTestActionItem = sourceTestUserItem.ParentTestActionItem;
                targetTestUserItem.ActionUserText = sourceTestUserItem.ActionUserText;
                return targetTestUserItem;
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                //MessageBox.Show("Exception\n " + ex.Message, "QAT Error Code - EC03021", MessageBoxButton.OK, MessageBoxImage.Error);
                return targetTestUserItem;
            }
        }

        public TestUserVerifyItem CopyTestVerifyUserItem(TestUserVerifyItem sourceTestUserItem)
        {
            TestUserVerifyItem targetTestUserItem = new TestUserVerifyItem();

            try
            {
                targetTestUserItem.ParentTestActionItem = sourceTestUserItem.ParentTestActionItem;
                targetTestUserItem.VerifyUserText = sourceTestUserItem.VerifyUserText;
                return targetTestUserItem;
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                //MessageBox.Show("Exception\n " + ex.Message, "QAT Error Code - EC03021", MessageBoxButton.OK, MessageBoxImage.Error);
                return targetTestUserItem;
            }
        }

        public TestCECItem AddSetTestCECItem()
        {
            TestCECItem setTestCECItem = new TestCECItem();

            try
            {
                setTestCECItem.ParentTestActionItem = this;

                ObservableCollection<string> telnetDeviceList = new ObservableCollection<string>();
                setTestCECItem.Deviceselection.Clear();
                //setTestCECItem.Deviceselection.Add("0-TV");
                //setTestCECItem.Deviceselection.Add("1-Recording Device 1");
                //setTestCECItem.Deviceselection.Add("2-Recording Device 2");
                //setTestCECItem.Deviceselection.Add("3-Tuner 1");
                //setTestCECItem.Deviceselection.Add("4-Playback Device 1");
                //setTestCECItem.Deviceselection.Add("5-Audio System");
                //setTestCECItem.Deviceselection.Add("6-Tuner 2");
                //setTestCECItem.Deviceselection.Add("7-Tuner 3");
                //setTestCECItem.Deviceselection.Add("8-Playback Device 2");
                //setTestCECItem.Deviceselection.Add("9-Recording Device 3");
                //setTestCECItem.Deviceselection.Add("10-Tuner 4");
                //setTestCECItem.Deviceselection.Add("11-Playback Device 3");
                //setTestCECItem.Deviceselection.Add("12-Reserved");
                //setTestCECItem.Deviceselection.Add("13-Reserved");
                //setTestCECItem.Deviceselection.Add("14-Specific Use");
                //setTestCECItem.Deviceselection.Add("15-Unregistered/Broadcast");
                setTestCECItem.Deviceselection.Add("Streamer");
                setTestCECItem.Deviceselection.Add("Broadcast");

                setTestCECItem.CECCommandList.Clear();
                setTestCECItem.CECCommandList.Add("Standby");
                //setTestCECItem.CECCommandList.Add("Poweron");
                setTestCECItem.CECCommandList.Add("Give Physical Address");
                setTestCECItem.CECCommandList.Add("Give OSD Name");
                setTestCECItem.CECCommandList.Add("Get CEC Version");
                setTestCECItem.CECCommandList.Add("Give Device Power Status");
                setTestCECItem.CECCommandList.Add("Give Deck Status");
                setTestCECItem.CECCommandList.Add("Others");

                SetTestCECList.Add(setTestCECItem);
                if (SetTestCECList.Count < 2)
                {
                    TestActionGridHeight = new GridLength(68, GridUnitType.Pixel);
                }
                else if (SetTestCECList.Count == 2)
                {
                    TestActionGridHeight = new GridLength(136, GridUnitType.Pixel);
                }
                else if (SetTestCECList.Count > 2)
                {
                    TestActionGridHeight = new GridLength(204, GridUnitType.Pixel);
                }
                return setTestCECItem;
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                //MessageBox.Show("Exception\n " + ex.Message, "QAT Error Code - EC03021", MessageBoxButton.OK, MessageBoxImage.Error);
                return setTestCECItem;
            }
        }

        public TestCECItem AddSetTestCECItem(TestCECItem sourceTestCECItem)
        {
            TestCECItem setTestCECItem = CopyTestCECItem(sourceTestCECItem);
            
            SetTestCECList.Add(setTestCECItem);

            if (SetTestCECList.Count < 2)
            {
                TestActionGridHeight = new GridLength(68, GridUnitType.Pixel);
            }
            else if (SetTestCECList.Count == 2)
            {
                TestActionGridHeight = new GridLength(136, GridUnitType.Pixel);
            }
            else if (SetTestCECList.Count > 2)
            {
                TestActionGridHeight = new GridLength(204, GridUnitType.Pixel);
            }

            return sourceTestCECItem;
        }

        public TestTelnetItem AddSetTestTelnetItem(TestTelnetItem sourceTestTelnetItem)
        {
            TestTelnetItem setTestTelnetItem = CopyTestTelnetItem(sourceTestTelnetItem);
            SetTestTelnetList.Add(setTestTelnetItem);
            return setTestTelnetItem;
        }

        public void RemoveSetTestTelnetItem(TestTelnetItem removeItem)
        {
            try
            {
                if (SetTestTelnetList.Contains(removeItem))
                    SetTestTelnetList.Remove(removeItem);
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                //MessageBox.Show("Exception\n \n" + ex.Message, "QAT Error Code - ECxxxx", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public TestFirmwareItem AddSetTestFirmwareItem()
        {
            TestFirmwareItem setTestFirmwareItem = new TestFirmwareItem();
            try
            {
                setTestFirmwareItem.ParentTestActionItem = this;
                SetTestFirmwareList.Add(setTestFirmwareItem);
                return setTestFirmwareItem;
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                //MessageBox.Show("Exception\n " + ex.Message, "QAT Error Code - EC03021", MessageBoxButton.OK, MessageBoxImage.Error);
                return setTestFirmwareItem;
            }
        }

        public void RemoveSetTestFirmwareItem(TestFirmwareItem removeItem)
        {
            try
            {
                if (SetTestFirmwareList.Contains(removeItem))
                    SetTestFirmwareList.Remove(removeItem);
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                // MessageBox.Show("Exception\n \n" + ex.Message, "QAT Error Code - ECxxxx", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public TestDesignerItem AddSetTestDesignerItem()
        {
            TestDesignerItem setTestDesignerItem = new TestDesignerItem();
            try
            {
                setTestDesignerItem.ParentTestActionItem = this;
                SetTestDesignerList.Add(setTestDesignerItem);
                return setTestDesignerItem;
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                //MessageBox.Show("Exception\n " + ex.Message, "QAT Error Code - EC03021", MessageBoxButton.OK, MessageBoxImage.Error);
                return setTestDesignerItem;
            }
        }

        public void RemoveSetTestDesignerItem(TestDesignerItem removeItem)
        {
            try
            {
                if (SetTestDesignerList.Contains(removeItem))
                    SetTestDesignerList.Remove(removeItem);
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

        public TestNetPairingItem AddSetTestNetPairingItem()
        {
            TestNetPairingItem setTestNetPairingItem = new TestNetPairingItem();
            try
            {
                setTestNetPairingItem.ParentTestActionItem = this;

                List<DUT_DeviceItem> deviceList = new List<DUT_DeviceItem>();
                foreach (var item in ParentTestCaseItem.SelectedDeviceItemList)
                {
                    if (item.ItemDeviceType != "Core")
                    {
                        deviceList.Add(new DUT_DeviceItem(item));
                    }
                }

                DeviceDiscovery.UpdateNetPairingList(deviceList);

                setTestNetPairingItem.DutDeviceList = new ObservableCollection<DUT_DeviceItem>(deviceList);

                SetTestNetPairingList.Add(setTestNetPairingItem);
                return setTestNetPairingItem;
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                //MessageBox.Show("Exception\n " + ex.Message, "QAT Error Code - EC03021", MessageBoxButton.OK, MessageBoxImage.Error);
                return setTestNetPairingItem;
            }
        }

        public void RemoveSetTestNetPairingItem(TestNetPairingItem removeItem)
        {
            try
            {
                if (SetTestNetPairingList.Contains(removeItem))
                    SetTestNetPairingList.Remove(removeItem);
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

        public TestControlItem AddVerifyTestControlItem()
        {
            TestControlItem verifyTestControlItem = new TestControlItem();
            try
            {
                verifyTestControlItem.ParentTestActionItem = this;
                ObservableCollection<string> ComponentTypeList = new ObservableCollection<string>(ParentTestCaseItem.ComponentTypeList.OrderBy(a => a));
                verifyTestControlItem.TestControlComponentTypeList = ComponentTypeList;
                //verifyTestControlItem.TestControlComponentTypeList = new ObservableCollection<string>(ParentTestCaseItem.ComponentTypeList);//added like this upto ver 1.19
                VerifyTestControlList.Add(verifyTestControlItem);

                if (SetTestControlList != null && VerifyTestControlList.Count <= SetTestControlList.Count)
                {
                    verifyTestControlItem.TestControlComponentTypeSelectedItem = SetTestControlList[VerifyTestControlList.Count - 1].TestControlComponentTypeSelectedItem;
                    verifyTestControlItem.TestControlComponentNameSelectedItem = SetTestControlList[VerifyTestControlList.Count - 1].TestControlComponentNameSelectedItem;
                    verifyTestControlItem.TestControlPropertySelectedItem = SetTestControlList[VerifyTestControlList.Count - 1].TestControlPropertySelectedItem;
                    verifyTestControlItem.ChannelSelectionSelectedItem = SetTestControlList[VerifyTestControlList.Count - 1].ChannelSelectionSelectedItem;
                    verifyTestControlItem.InputSelectionComboSelectedItem = SetTestControlList[VerifyTestControlList.Count - 1].InputSelectionComboSelectedItem;
                    verifyTestControlItem.TestControlPropertyInitialValueSelectedItem = SetTestControlList[VerifyTestControlList.Count - 1].TestControlPropertyInitialValueSelectedItem;
                    if (SetTestControlList[VerifyTestControlList.Count - 1].LoopIsChecked)
                    {
                        verifyTestControlItem.LoopIsChecked = SetTestControlList[VerifyTestControlList.Count - 1].LoopIsChecked;
                        verifyTestControlItem.LoopStart = SetTestControlList[VerifyTestControlList.Count - 1].LoopStart;
                        verifyTestControlItem.LoopEnd = SetTestControlList[VerifyTestControlList.Count - 1].LoopEnd;
                        verifyTestControlItem.LoopIncrement = SetTestControlList[VerifyTestControlList.Count - 1].LoopIncrement;
                    }
                    
                    //verifyTestControlItem.TestControlComboValueSelectedItem = SetTestControlList[VerifyTestControlList.Count - 1].TestControlComboValueSelectedItem;

                }
                if (VerifyTestControlList.Count < 2)
                {
                    TestVerificationGridHeight = new GridLength(130, GridUnitType.Pixel);
                }
                else if (VerifyTestControlList.Count == 2)
                {
                    TestVerificationGridHeight = new GridLength(260, GridUnitType.Pixel);
                }
                else if (VerifyTestControlList.Count > 2)
                {
                    TestVerificationGridHeight = new GridLength(380, GridUnitType.Pixel);
                }
                return verifyTestControlItem;
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                //MessageBox.Show("Exception\n " + ex.Message, "QAT Error Code - EC03021", MessageBoxButton.OK, MessageBoxImage.Error);
                return verifyTestControlItem;
            }
        }

        public TestControlItem AddVerifyTestControlItem(TestControlItem sourceTestControlItem)
        {
            TestControlItem verifyTestControlItem = CopyTestControlItem(sourceTestControlItem);
            VerifyTestControlList.Insert(VerifyTestControlList.IndexOf(sourceTestControlItem), verifyTestControlItem);
            //VerifyTestControlList.Add(verifyTestControlItem);
            if (VerifyTestControlList.Count < 2)
            {
                TestVerificationGridHeight = new GridLength(130, GridUnitType.Pixel);
            }
            else if (VerifyTestControlList.Count == 2)
            {
                TestVerificationGridHeight = new GridLength(260, GridUnitType.Pixel);
            }
            else if (VerifyTestControlList.Count > 2)
            {
                TestVerificationGridHeight = new GridLength(380, GridUnitType.Pixel);
            }
            return verifyTestControlItem;
        }

        public void RemoveVerifyTestControlItem(TestControlItem removeItem)
        {
            try
            {
                if (VerifyTestControlList.Contains(removeItem))
                    VerifyTestControlList.Remove(removeItem);
                if (VerifyTestControlList.Count < 2)
                {
                    TestVerificationGridHeight = new GridLength(130, GridUnitType.Pixel);
                }
                else if (VerifyTestControlList.Count == 2)
                {
                    TestVerificationGridHeight = new GridLength(260, GridUnitType.Pixel);
                }
                else if (VerifyTestControlList.Count > 2)
                {
                    TestVerificationGridHeight = new GridLength(380, GridUnitType.Pixel);
                }
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                //MessageBox.Show("Exception\n \n" + ex.Message, "QAT Error Code - ECxxxx", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        public TestVerifyQRCMItem AddVerifyTestQRCMItem()
        {
            TestVerifyQRCMItem verifyTestQRCMItem = new TestVerifyQRCMItem();

            try
            {
                verifyTestQRCMItem.ParentTestActionItem = this;  
                
                foreach (var item in ParentTestCaseItem.SelectedDeviceItemList)
                {
                    if (item.ItemDeviceType.ToUpper() == "CORE")
                    {
                        if (item.ItemPrimaryorBackup.ToUpper() == "PRIMARY" && !verifyTestQRCMItem.VerifyQRCM_DevicesList.Contains("Primary core"))
                        {
                            verifyTestQRCMItem.VerifyQRCM_DevicesList.Add("Primary core");
                            verifyTestQRCMItem.QRCM_DeviceModel.Add("Primary core", item.ItemDeviceModel);
                        }
                        else if (item.ItemPrimaryorBackup.ToUpper() == "BACKUP" && !verifyTestQRCMItem.VerifyQRCM_DevicesList.Contains("Backup core"))
                        {
                            verifyTestQRCMItem.VerifyQRCM_DevicesList.Add("Backup core");
                            verifyTestQRCMItem.QRCM_DeviceModel.Add("Backup core", item.ItemDeviceModel);
                        }
                    }                
                }

                if (verifyTestQRCMItem.ParentTestActionItem.VerifyQRCMVersionSelected!= null && verifyTestQRCMItem.ParentTestActionItem.VerifyQRCMVersionSelected != string.Empty)
                {
                    if (ParentTestCaseItem.QRCMDictionary.Count > 0 && ParentTestCaseItem.QRCMDictionary.Keys.Contains(verifyTestQRCMItem.ParentTestActionItem.VerifyQRCMVersionSelected))
                    {
                        ObservableCollection<QRCMInitialValues> CurrentVersionValues = ParentTestCaseItem.QRCMDictionary[verifyTestQRCMItem.ParentTestActionItem.VerifyQRCMVersionSelected];
                        List<string> methodNames = new List<string>();
                        foreach (QRCMInitialValues item in CurrentVersionValues)
                        {
                            if (!item.IsActionTrue)
                            {
                                methodNames.Add(item.MethodNameUserView);
                                verifyTestQRCMItem.QRCM_MethodsInitialValues.Add(item);
                            }
                        }
                        string[] methodNamesArraylist = methodNames.ToArray();
                        Array.Sort(methodNamesArraylist, new AlphanumComparatorFaster());
                        ObservableCollection<string> methodsNameList = new ObservableCollection<string>(methodNamesArraylist.ToList());
                        verifyTestQRCMItem.VerifyQRCM_MethodsList = methodsNameList;
                    }
                }
               
                
                VerifyTestQRCMList.Add(verifyTestQRCMItem); 
                return verifyTestQRCMItem;
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                //MessageBox.Show("Exception\n " + ex.Message, "QAT Error Code - EC03021", MessageBoxButton.OK, MessageBoxImage.Error);
                return verifyTestQRCMItem;
            }
        }


        public TestTelnetItem AddVerifyTestTelnetItem()
        {
            TestTelnetItem verifyTestTelnetItem = new TestTelnetItem();

            try
            {
                verifyTestTelnetItem.ParentTestActionItem = this;
                VerifyTestTelnetList.Add(verifyTestTelnetItem);
                return verifyTestTelnetItem;
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                //MessageBox.Show("Exception\n " + ex.Message, "QAT Error Code - EC03021", MessageBoxButton.OK, MessageBoxImage.Error);
                return verifyTestTelnetItem;
            }
        }

        public void RemoveVerifyTestTelnetItem(TestTelnetItem removeItem)
        {
            try
            {
                if (VerifyTestTelnetList.Contains(removeItem))
                    VerifyTestTelnetList.Remove(removeItem);
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                //MessageBox.Show("Exception\n \n" + ex.Message, "QAT Error Code - ECxxxx", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        public TestVerifyCECItem AddVerifyTestCECItem(TestVerifyCECItem sourceTestCECItem)
        {
            TestVerifyCECItem VerifyTestCECItem = CopyTestVerifyCECItem(sourceTestCECItem);

            VerifyTestCECList.Add(VerifyTestCECItem);

            if (VerifyTestCECList.Count < 2)
            {
                TestVerificationGridHeight = new GridLength(68, GridUnitType.Pixel);
            }
            else if (VerifyTestCECList.Count == 2)
            {
                TestVerificationGridHeight = new GridLength(136, GridUnitType.Pixel);
            }
            else if (VerifyTestCECList.Count > 2)
            {
                TestVerificationGridHeight = new GridLength(204, GridUnitType.Pixel);
            }

            return sourceTestCECItem;
        }

        public TestVerifyQRItem AddVerifyTestQRItem(TestVerifyQRItem sourceTestQRItem)
        {
            TestVerifyQRItem VerifyTestQRItem = CopyTestVerifyQRItem(sourceTestQRItem);

            VerifyTestQRList.Add(VerifyTestQRItem);

            if (VerifyTestQRList.Count < 2)
            {
                TestVerificationGridHeight = new GridLength(68, GridUnitType.Pixel);
            }
            else if (VerifyTestQRList.Count == 2)
            {
                TestVerificationGridHeight = new GridLength(136, GridUnitType.Pixel);
            }
            else if (VerifyTestQRList.Count > 2)
            {
                TestVerificationGridHeight = new GridLength(204, GridUnitType.Pixel);
            }

            return sourceTestQRItem;
        }
        public TestVerifyCECItem CopyTestVerifyCECItem(TestVerifyCECItem sourceTestCECItem)
        {
            TestVerifyCECItem targetTestCECItem = new TestVerifyCECItem();
          
            try
            {
                targetTestCECItem.ParentTestActionItem = sourceTestCECItem.ParentTestActionItem;
                targetTestCECItem.CECverificationOpcode = sourceTestCECItem.CECverificationOpcode;
                return targetTestCECItem;
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                //MessageBox.Show("Exception\n " + ex.Message, "QAT Error Code - EC03021", MessageBoxButton.OK, MessageBoxImage.Error);
                return targetTestCECItem;
            }
        }
        public TestVerifyQRItem CopyTestVerifyQRItem(TestVerifyQRItem sourceTestQRItem)
        {
            TestVerifyQRItem targetTestQRItem = new TestVerifyQRItem();

            try
            {
                targetTestQRItem.ParentTestActionItem = sourceTestQRItem.ParentTestActionItem;
                targetTestQRItem.CameraList = sourceTestQRItem.CameraList;
                targetTestQRItem.CameraList1 = sourceTestQRItem.CameraList1;
                targetTestQRItem.CameraSelectedItem = sourceTestQRItem.CameraSelectedItem;
                targetTestQRItem.QRverificationcode = sourceTestQRItem.QRverificationcode;
                targetTestQRItem.QRverifytype = sourceTestQRItem.QRverifytype;

                return targetTestQRItem;
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                //MessageBox.Show("Exception\n " + ex.Message, "QAT Error Code - EC03021", MessageBoxButton.OK, MessageBoxImage.Error);
                return targetTestQRItem;
            }
        }
       

         public TestVerifyQRItem AddVerifyTestQRItem()
        {
            TestVerifyQRItem verifyTestQRItem = new TestVerifyQRItem();

            try
            {
                verifyTestQRItem.ParentTestActionItem = this;
                foreach (var item in ParentTestCaseItem.SelectedDeviceItemList)
                {
                    if (item.ItemDeviceType.ToUpper() == "CAMERA")
                    {
                        verifyTestQRItem.CameraList.Add(item.ItemDeviceName, item.ItemDeviceModel);
                        verifyTestQRItem.CameraList1.Add(item.ItemDeviceName);
                    }
                }
                VerifyTestQRList.Add(verifyTestQRItem);
                if (VerifyTestQRList.Count < 2)
                {
                    TestVerificationGridHeight = new GridLength(68, GridUnitType.Pixel);
                }
                else if (VerifyTestQRList.Count == 2)
                {
                    TestVerificationGridHeight = new GridLength(136, GridUnitType.Pixel);
                }
                else if (VerifyTestQRList.Count > 2)
                {
                    TestVerificationGridHeight = new GridLength(204, GridUnitType.Pixel);
                }

                return verifyTestQRItem;
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                //MessageBox.Show("Exception\n " + ex.Message, "QAT Error Code - EC03021", MessageBoxButton.OK, MessageBoxImage.Error);
                return verifyTestQRItem;
            }
        }



        public TestUserVerifyItem AddTestUserVerifyItem()
        {
            TestUserVerifyItem testUserVerifyItem = new TestUserVerifyItem();

            try
            {
                testUserVerifyItem.ParentTestActionItem = this;
                VerifyTestUserList.Add(testUserVerifyItem);
             

                return testUserVerifyItem;
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                //MessageBox.Show("Exception\n " + ex.Message, "QAT Error Code - EC03021", MessageBoxButton.OK, MessageBoxImage.Error);
                return testUserVerifyItem;
            }
        }

        public TestVerifyCECItem AddVerifyTestCECItem()
        {
            TestVerifyCECItem verifyTestcecItem = new TestVerifyCECItem();

            try
            {
                verifyTestcecItem.ParentTestActionItem = this;
                VerifyTestCECList.Add(verifyTestcecItem);
                if (VerifyTestCECList.Count < 2)
                {
                    TestVerificationGridHeight = new GridLength(68, GridUnitType.Pixel);
                }
                else if (VerifyTestCECList.Count == 2)
                {
                    TestVerificationGridHeight = new GridLength(136, GridUnitType.Pixel);
                }
                else if (VerifyTestCECList.Count > 2)
                {
                    TestVerificationGridHeight = new GridLength(204, GridUnitType.Pixel);
                }

                return verifyTestcecItem;
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                //MessageBox.Show("Exception\n " + ex.Message, "QAT Error Code - EC03021", MessageBoxButton.OK, MessageBoxImage.Error);
                return verifyTestcecItem;
            }
        }

        public TestUsbAudioBridging AddVerifyTestUsbItem()
        {
            TestUsbAudioBridging verifyTestUsbItem = new TestUsbAudioBridging();

            try
            {
                verifyTestUsbItem.ParentTestActionItem = this;
                verifyTestUsbItem.UsbAudioDeviceList = ParentTestCaseItem.UsbAudioDeviceList;
                verifyTestUsbItem.UsbAudioBridgeList = ParentTestCaseItem.UsbAudioBridgeList;
                VerifyTestUsbList.Add(verifyTestUsbItem);

                if (SetTestUsbList != null && VerifyTestUsbList.Count <= SetTestUsbList.Count)
                {
                    verifyTestUsbItem.UsbAudioBridgeTypeSelectedItem = SetTestUsbList[VerifyTestUsbList.Count - 1].UsbAudioBridgeTypeSelectedItem;
                    verifyTestUsbItem.UsbAudioTypeSelectedItem = SetTestUsbList[VerifyTestUsbList.Count - 1].UsbAudioTypeSelectedItem;
                    verifyTestUsbItem.UsbAudioDeviceSelectedItem = SetTestUsbList[VerifyTestUsbList.Count - 1].UsbAudioDeviceSelectedItem;
                    verifyTestUsbItem.UsbAudioBridgeDeviceComboEnable = SetTestUsbList[VerifyTestUsbList.Count - 1].UsbAudioBridgeDeviceComboEnable;
                    verifyTestUsbItem.UsbDefaultDeviceOptionSelectedItem = SetTestUsbList[VerifyTestUsbList.Count - 1].UsbDefaultDeviceOptionSelectedItem;

                }
                if (VerifyTestUsbList.Count < 2)
                {
                    TestVerificationGridHeight = new GridLength(68, GridUnitType.Pixel);
                }
                else if (VerifyTestUsbList.Count == 2)
                {
                    TestVerificationGridHeight = new GridLength(136, GridUnitType.Pixel);
                }
                else if (VerifyTestUsbList.Count > 2)
                {
                    TestVerificationGridHeight = new GridLength(204, GridUnitType.Pixel);
                }

                return verifyTestUsbItem;
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                //MessageBox.Show("Exception\n " + ex.Message, "QAT Error Code - EC03021", MessageBoxButton.OK, MessageBoxImage.Error);
                return verifyTestUsbItem;
            }
        }

        public TestUsbAudioBridging AddVerifyTestUsbItem(TestUsbAudioBridging sourceTestUsbItem)
        {
            TestUsbAudioBridging verifyTestUsbItem = CopyTestUsbItem(sourceTestUsbItem);
            VerifyTestUsbList.Add(verifyTestUsbItem);
            if (VerifyTestUsbList.Count < 2)
            {
                TestVerificationGridHeight = new GridLength(68, GridUnitType.Pixel);
            }
            else if (VerifyTestUsbList.Count == 2)
            {
                TestVerificationGridHeight = new GridLength(136, GridUnitType.Pixel);
            }
            else if (VerifyTestUsbList.Count > 2)
            {
                TestVerificationGridHeight = new GridLength(204, GridUnitType.Pixel);
            }
            return verifyTestUsbItem;
        }

        public void RemoveVerifyTestUsbItem(TestUsbAudioBridging removeItem)
        {
            try
            {
                if (VerifyTestUsbList.Contains(removeItem))
                    VerifyTestUsbList.Remove(removeItem);
                if (VerifyTestUsbList.Count < 2)
                {
                    TestVerificationGridHeight = new GridLength(68, GridUnitType.Pixel);
                }
                else if (VerifyTestUsbList.Count == 2)
                {
                    TestVerificationGridHeight = new GridLength(136, GridUnitType.Pixel);
                }
                else if (VerifyTestUsbList.Count > 2)
                {
                    TestVerificationGridHeight = new GridLength(204, GridUnitType.Pixel);
                }
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                //MessageBox.Show("Exception\n \n" + ex.Message, "QAT Error Code - ECxxxx", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        

        public void RemoveVerifyScriptItem(TestScriptVerification removeItem)
        {
            try
            {
                if (VerifyTestScriptList.Contains(removeItem))
                    VerifyTestScriptList.Remove(removeItem);

                if (VerifyTestScriptList.Count < 2)
                {
                    TestVerificationGridHeight = new GridLength(160, GridUnitType.Pixel);
                }
                else if (VerifyTestScriptList.Count == 2)
                {
                    TestVerificationGridHeight = new GridLength(300, GridUnitType.Pixel);
                }
                else if (VerifyTestScriptList.Count > 2)
                {
                    TestVerificationGridHeight = new GridLength(450, GridUnitType.Pixel);
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

        public TestScriptVerification AddVerifyTestScriptItem(TestScriptVerification sourceTestScriptItem)
        {
            TestScriptVerification verifyTestScriptItem = CopyVerifyScriptItem(sourceTestScriptItem);
            try
            {
                VerifyTestScriptList.Insert(VerifyTestScriptList.IndexOf(sourceTestScriptItem) + 1, verifyTestScriptItem);

                if (VerifyTestScriptList.Count < 2)
                {
                    TestVerificationGridHeight = new GridLength(160, GridUnitType.Pixel);
                }
                else if (VerifyTestScriptList.Count == 2)
                {
                    TestVerificationGridHeight = new GridLength(300, GridUnitType.Pixel);
                }
                else if (VerifyTestScriptList.Count > 2)
                {
                    TestVerificationGridHeight = new GridLength(450, GridUnitType.Pixel);
                }
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
            }
            return verifyTestScriptItem;
        }

        public TestScriptVerification CopyVerifyScriptItem(TestScriptVerification sourceScriptItem)
        {
            TestScriptVerification targetScriptItem = new TestScriptVerification();

            try
            {
                targetScriptItem.ParentTestActionItem = sourceScriptItem.ParentTestActionItem;
                targetScriptItem.Devicenamelist = new ObservableCollection<string>(sourceScriptItem.Devicenamelist);
                targetScriptItem.DevicenameWithModel = new Dictionary<string, string>(sourceScriptItem.DevicenameWithModel);
                if (!string.IsNullOrEmpty(sourceScriptItem.DevicenamelistSelectedItem))
                    targetScriptItem.DevicenamelistSelectedItem = sourceScriptItem.DevicenamelistSelectedItem;
                targetScriptItem.VerifyScriptAction = new ObservableCollection<string>(sourceScriptItem.VerifyScriptAction);
                
                if (!string.IsNullOrEmpty(sourceScriptItem.CPUNumberSelectedItem))
                    targetScriptItem.CPUNumberSelectedItem = sourceScriptItem.CPUNumberSelectedItem;

                if (!string.IsNullOrEmpty(sourceScriptItem.VerifyScriptActionSelectedItem))
                    targetScriptItem.VerifyScriptActionSelectedItem = sourceScriptItem.VerifyScriptActionSelectedItem;

                if (!string.IsNullOrEmpty(sourceScriptItem.CustomCommand))
                    targetScriptItem.CustomCommand = sourceScriptItem.CustomCommand;

                targetScriptItem.CustomCommandTextboxIsEnabled = sourceScriptItem.CustomCommandTextboxIsEnabled;
                targetScriptItem.RegexMatch = sourceScriptItem.RegexMatch;
                targetScriptItem.RegexTextboxIsEnabled = sourceScriptItem.RegexTextboxIsEnabled;
                targetScriptItem.Lowerlimit = sourceScriptItem.Lowerlimit;
                targetScriptItem.LimitUnitlist = new List<string>(sourceScriptItem.LimitUnitlist);
                targetScriptItem.Upperlimit = sourceScriptItem.Upperlimit;

                if (!string.IsNullOrEmpty(sourceScriptItem.LimitUnitSelectedItem))
                    targetScriptItem.LimitUnitSelectedItem = sourceScriptItem.LimitUnitSelectedItem;

                targetScriptItem.VerifyDesignDevicesIsChecked = sourceScriptItem.VerifyDesignDevicesIsChecked;
                targetScriptItem.VerifyDesignDevicesVisibility = sourceScriptItem.VerifyDesignDevicesVisibility;
                targetScriptItem.CPUVisibility = sourceScriptItem.CPUVisibility;

                return targetScriptItem;
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif              
                return targetScriptItem;
            }
        }

        public TestScriptVerification AddVerifyTestScriptItem()
        {
            TestScriptVerification verifyTestScriptItem = new TestScriptVerification();

            try
            {
                verifyTestScriptItem.ParentTestActionItem = this;
                //Array sortedDeviceList = DeviceDiscovery.AvailableDeviceList.ToArray();
                //Array.Sort(sortedDeviceList);

                foreach (string item in DeviceDiscovery.availableDeviceList_script)
                {
                    string[] splitvalues = item.Split(',');
                    if (!splitvalues[0].ToUpper().StartsWith("PTZ") && (!verifyTestScriptItem.Devicenamelist.Contains(splitvalues[1])))
                    {
                        verifyTestScriptItem.Devicenamelist.Add(splitvalues[1]);
                    }

                    if (!verifyTestScriptItem.DevicenameWithModel.Keys.Contains(splitvalues[1].ToLower()) && !string.IsNullOrEmpty(splitvalues[1].ToLower()))
                        verifyTestScriptItem.DevicenameWithModel.Add(splitvalues[1].ToLower(), splitvalues[0].ToLower());
                }

                VerifyTestScriptList.Add(verifyTestScriptItem);

                if (VerifyTestScriptList.Count < 2)
                {
                    TestVerificationGridHeight = new GridLength(160, GridUnitType.Pixel);
                }
                else if (VerifyTestScriptList.Count == 2)
                {
                    TestVerificationGridHeight = new GridLength(300, GridUnitType.Pixel);
                }
                else if (VerifyTestScriptList.Count > 2)
                {
                    TestVerificationGridHeight = new GridLength(450, GridUnitType.Pixel);
                }

                return verifyTestScriptItem;

            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                //MessageBox.Show("Exception\n " + ex.Message, "QAT Error Code - EC03021", MessageBoxButton.OK, MessageBoxImage.Error);
                return verifyTestScriptItem;
            }
        }




        public TestLuaItem AddVerifyTestLuaItem()
        {
            TestLuaItem verifyTestLuaItem = new TestLuaItem();
            try
            {
                verifyTestLuaItem.ParentTestActionItem = this;
                VerifyTestLuaList.Add(verifyTestLuaItem);
                return verifyTestLuaItem;
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                //MessageBox.Show("Exception\n " + ex.Message, "QAT Error Code - EC03021", MessageBoxButton.OK, MessageBoxImage.Error);

                return verifyTestLuaItem;
            }
        }

        public void RemoveVerifyTestLuaItem(TestLuaItem removeItem)
        {
            try
            {
                if (VerifyTestLuaList.Contains(removeItem))
                    VerifyTestLuaList.Remove(removeItem);
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

        public TestLogItem AddVerifyTestLogItem()
        {
            TestLogItem verifyTestLogItem = new TestLogItem();

            try
            {
                verifyTestLogItem.ParentTestActionItem = this;
                ObservableCollection<string> eventloglog = new ObservableCollection<string>();
                if (ParentTestCaseItem.SelectedDeviceItemList.Count > 0)
                {
                    eventloglog.Add("All devices");
                }
                foreach (var item in ParentTestCaseItem.SelectedDeviceItemList)
                {

                    eventloglog.Add(item.ItemDeviceName);

                }
                verifyTestLogItem.Log_verification_kernellog = eventloglog;

                VerifyTestLogList.Add(verifyTestLogItem);
                return verifyTestLogItem;
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                //MessageBox.Show("Exception\n " + ex.Message, "QAT Error Code - EC03021", MessageBoxButton.OK, MessageBoxImage.Error);
                return verifyTestLogItem;
            }
        }

        public void RemoveVerifyTestLogItem(TestLogItem removeItem)
        {
            try
            {
                if (VerifyTestLogList.Contains(removeItem))
                    VerifyTestLogList.Remove(removeItem);
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

        public TestApxItem AddVerifyTestApxItem()
        {
            TestApxItem verifyTestApxItem = new TestApxItem();

            try
            {
                verifyTestApxItem.ParentTestActionItem = this;
                VerifyTestApxList.Add(verifyTestApxItem);
                return verifyTestApxItem;
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                //MessageBox.Show("Exception\n " + ex.Message, "QAT Error Code - EC03021", MessageBoxButton.OK, MessageBoxImage.Error);
                return verifyTestApxItem;
            }
        }

        public void RemoveVerifyTestApxItem(TestApxItem removeItem)
        {
            try
            {
                if (VerifyTestApxList.Contains(removeItem))
                    VerifyTestApxList.Remove(removeItem);
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

        public TestSaveLogItem AddTestSaveLogItem()
        {
            TestSaveLogItem tsestSaveLogItem = new TestSaveLogItem();
            try
            {
                tsestSaveLogItem.ParentTestActionItem = this;

                //ObservableCollection<DUT_DeviceItem> iLogDeviceList = new ObservableCollection<DUT_DeviceItem>();
                //foreach (var item in ParentTestCaseItem.SelectedDeviceItemList)
                //{
                //    iLogDeviceList.Add(new DUT_DeviceItem(item));
                //}

                //tsestSaveLogItem.iLogDeviceItem = iLogDeviceList;

                //ObservableCollection<DUT_DeviceItem> configuratorDeviceList = new ObservableCollection<DUT_DeviceItem>();
                //foreach (var item in ParentTestCaseItem.SelectedDeviceItemList)
                //{
                //    if (item.ItemDeviceModel.Contains("Core"))
                //        configuratorDeviceList.Add(new DUT_DeviceItem(item));
                //}

                //tsestSaveLogItem.ConfiguratorLogDeviceItem = configuratorDeviceList;

                TestSaveLogItemList.Add(tsestSaveLogItem);
                return tsestSaveLogItem;
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                //MessageBox.Show("Exception\n " + ex.Message, "QAT Error Code - EC03021", MessageBoxButton.OK, MessageBoxImage.Error);

                return tsestSaveLogItem;
            }
        }

        public void RemoveTestSaveLogItem(TestSaveLogItem removeItem)
        {
            try
            {
                if (TestSaveLogItemList.Contains(removeItem))
                    TestSaveLogItemList.Remove(removeItem);
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

        public TestResponsalyzerItem AddTestResponsalyzerItem()
        {
            TestResponsalyzerItem testResponsalyzerItem = new TestResponsalyzerItem();

            try
            {
                testResponsalyzerItem.ParentTestActionItem = this;
                testResponsalyzerItem.TestResponsalyzerNameList = ParentTestCaseItem.ResponsalyzerNameList;
                testResponsalyzerItem.TestResponsalyzerTypeList = ParentTestCaseItem.ResponsalyzerTypeList;
                verifyTestResponsalyzerList.Add(testResponsalyzerItem);
                return testResponsalyzerItem;
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                //MessageBox.Show("Exception\n " + ex.Message, "QAT Error Code - EC03021", MessageBoxButton.OK, MessageBoxImage.Error);
                return testResponsalyzerItem;
            }
        }

        public void RemoveTestResponsalyzerItem(TestResponsalyzerItem removeItem)
        {
            try
            {
                if (verifyTestResponsalyzerList.Contains(removeItem))
                    verifyTestResponsalyzerList.Remove(removeItem);
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

        public TestResponsalyzerItem AddVerifyTestResponsalyzerItem(TestResponsalyzerItem sourceTestResponsalyzerItem)
        {
            TestResponsalyzerItem verifyTestResponsalyzerItem = CopyTestResponsalyzerItem(sourceTestResponsalyzerItem);
            verifyTestResponsalyzerList.Add(verifyTestResponsalyzerItem);
            return verifyTestResponsalyzerItem;
        }

        public TestUsbAudioBridging AddSetTestUsbItem()
        {
            TestUsbAudioBridging setTestUsbItem = new TestUsbAudioBridging();
            try
            {
                setTestUsbItem.ParentTestActionItem = this;
                //Dictionary<string,string> AudioDeviceList = new Dictionary<string, string>(ParentTestCaseItem.UsbAudioDeviceList.OrderBy(a => a));
               
                setTestUsbItem.UsbAudioDeviceList = ParentTestCaseItem.UsbAudioDeviceList;
               // var temp = ParentTestCaseItem.UsbAudioBridgeList.OrderBy(key => key.Key);
               // var SortedBridgeList = temp.ToDictionary((keyItem) => keyItem.Key, (valueItem) => valueItem.Value);
                setTestUsbItem.UsbAudioBridgeList = ParentTestCaseItem.UsbAudioBridgeList;
              
                SetTestUsbList.Add(setTestUsbItem);
                if (SetTestUsbList.Count < 2)
                {
                    TestActionGridHeight = new GridLength(68, GridUnitType.Pixel);
                }
                else if (SetTestUsbList.Count == 2)
                {
                    TestActionGridHeight = new GridLength(136, GridUnitType.Pixel);
                }
                else if (SetTestUsbList.Count > 2)
                {
                    TestActionGridHeight = new GridLength(204, GridUnitType.Pixel);
                }
                return setTestUsbItem;
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                //MessageBox.Show("Exception\n " + ex.Message, "QAT Error Code - EC03021", MessageBoxButton.OK, MessageBoxImage.Error);
                return setTestUsbItem;
            }
        }

        public TestUsbAudioBridging AddSetTestUsbItem(TestUsbAudioBridging sourceTestUsbItem)
        {
            TestUsbAudioBridging setTestUsbItem = CopyTestUsbItem(sourceTestUsbItem);
            SetTestUsbList.Add(setTestUsbItem);
            if (SetTestUsbList.Count < 2)
            {
                TestActionGridHeight = new GridLength(68, GridUnitType.Pixel);
            }
            else if (SetTestUsbList.Count == 2)
            {
                TestActionGridHeight = new GridLength(136, GridUnitType.Pixel);
            }
            else if (SetTestUsbList.Count > 2)
            {
                TestActionGridHeight = new GridLength(204, GridUnitType.Pixel);
            }
            return sourceTestUsbItem;
        }

        public void RemoveSetTestUsbItem(TestUsbAudioBridging removeItem)
        {
            try
            {
                if (SetTestUsbList.Contains(removeItem))
                    SetTestUsbList.Remove(removeItem);
                if (SetTestUsbList.Count < 2)
                {
                    TestActionGridHeight = new GridLength(68, GridUnitType.Pixel);
                }
                else if (SetTestUsbList.Count == 2)
                {
                    TestActionGridHeight = new GridLength(136, GridUnitType.Pixel);
                }
                else if (SetTestUsbList.Count > 2)
                {
                    TestActionGridHeight = new GridLength(204, GridUnitType.Pixel);
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


        public void RemoveSetTestUserItem(TestUserActionItem removeItem)
        {
            try
            {
                if (SetTestUserActionList.Contains(removeItem))
                    SetTestUserActionList.Remove(removeItem);               
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

        public void RemoveVerifyTestUserItem(TestUserVerifyItem removeItem)
        {
            try
            {
                if (VerifyTestUserList.Contains(removeItem))
                    VerifyTestUserList.Remove(removeItem);            
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


        public void RemoveSetTestCECItem(TestCECItem removeItem)
        {
            try
            {
                if (SetTestCECList.Contains(removeItem))
                    SetTestCECList.Remove(removeItem);
                if (SetTestCECList.Count < 2)
                {
                    TestActionGridHeight = new GridLength(68, GridUnitType.Pixel);
                }
                else if (SetTestCECList.Count == 2)
                {
                    TestActionGridHeight = new GridLength(136, GridUnitType.Pixel);
                }
                else if (SetTestCECList.Count > 2)
                {
                    TestActionGridHeight = new GridLength(204, GridUnitType.Pixel);
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

        public void RemoveVerifyTestCECItem(TestVerifyCECItem removeItem)
        {
            try
            {
                if (VerifyTestCECList.Contains(removeItem))
                    VerifyTestCECList.Remove(removeItem);
                if (VerifyTestCECList.Count < 2)
                {
                    TestVerificationGridHeight = new GridLength(68, GridUnitType.Pixel);
                }
                else if (VerifyTestCECList.Count == 2)
                {
                    TestVerificationGridHeight = new GridLength(136, GridUnitType.Pixel);
                }
                else if (VerifyTestCECList.Count > 2)
                {
                    TestVerificationGridHeight = new GridLength(204, GridUnitType.Pixel);
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
        public void RemoveVerifyTestQRItem(TestVerifyQRItem removeItem)
        {
            try
            {
                if (VerifyTestQRList.Contains(removeItem))
                    VerifyTestQRList.Remove(removeItem);
                if (VerifyTestQRList.Count < 2)
                {
                    TestVerificationGridHeight = new GridLength(68, GridUnitType.Pixel);
                }
                else if (VerifyTestQRList.Count == 2)
                {
                    TestVerificationGridHeight = new GridLength(136, GridUnitType.Pixel);
                }
                else if (VerifyTestQRList.Count > 2)
                {
                    TestVerificationGridHeight = new GridLength(204, GridUnitType.Pixel);
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
     
        public TestResponsalyzerItem CopyTestResponsalyzerItem(TestResponsalyzerItem sourceTestResponsalyzerItem)
        {
            TestResponsalyzerItem targetTestResponsalyzerItem = new TestResponsalyzerItem();
            try
            {
                targetTestResponsalyzerItem.ParentTestActionItem = sourceTestResponsalyzerItem.ParentTestActionItem;

                if (sourceTestResponsalyzerItem.TestResponsalyzerNameList != null)
                    targetTestResponsalyzerItem.TestResponsalyzerNameList = new ObservableCollection<string>(sourceTestResponsalyzerItem.TestResponsalyzerNameList);

                targetTestResponsalyzerItem.TestResponsalyzerNameSelectedItem = sourceTestResponsalyzerItem.TestResponsalyzerNameSelectedItem;

                if (sourceTestResponsalyzerItem.TestResponsalyzerTypeList != null)
                    targetTestResponsalyzerItem.TestResponsalyzerTypeList = new ObservableCollection<string>(sourceTestResponsalyzerItem.TestResponsalyzerTypeList);
                targetTestResponsalyzerItem.TestResponsalyzerTypeSelectedItem = sourceTestResponsalyzerItem.TestResponsalyzerTypeSelectedItem;

                targetTestResponsalyzerItem.TestResponsalyzerVerificationFile = sourceTestResponsalyzerItem.TestResponsalyzerVerificationFile;
                targetTestResponsalyzerItem.CopyItemSource = true;

                return targetTestResponsalyzerItem;
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                //MessageBox.Show("Exception\n " + ex.Message, "QAT Error Code - EC03021", MessageBoxButton.OK, MessageBoxImage.Error);
                return targetTestResponsalyzerItem;
            }
        }

        public TestControlItem CopyTestControlItem(TestControlItem sourceTestControlItem)
        {
            TestControlItem targetTestControlItem = new TestControlItem();

            try
            {
                targetTestControlItem.ParentTestActionItem = sourceTestControlItem.ParentTestActionItem;

                if (sourceTestControlItem.TestControlComponentTypeList != null)
                    targetTestControlItem.TestControlComponentTypeList = new ObservableCollection<string>(sourceTestControlItem.TestControlComponentTypeList);

                if (sourceTestControlItem.CustomControlDisableList != null)
                    targetTestControlItem.CustomControlDisableList = new ObservableCollection<string>(sourceTestControlItem.CustomControlDisableList);

                targetTestControlItem.TestControlComponentTypeSelectedItem = sourceTestControlItem.TestControlComponentTypeSelectedItem;

                if (sourceTestControlItem.TestControlComponentNameList != null)
                    targetTestControlItem.TestControlComponentNameList = new ObservableCollection<string>(sourceTestControlItem.TestControlComponentNameList);
                targetTestControlItem.TestControlComponentNameSelectedItem = sourceTestControlItem.TestControlComponentNameSelectedItem;

                if (sourceTestControlItem.TestControlPropertyList != null)
                    targetTestControlItem.TestControlPropertyList = new ObservableCollection<string>(sourceTestControlItem.TestControlPropertyList);

                if (sourceTestControlItem.VerifyTestControlPropertyList != null)
                    targetTestControlItem.VerifyTestControlPropertyList = new ObservableCollection<string>(sourceTestControlItem.VerifyTestControlPropertyList);
                targetTestControlItem.TestControlPropertySelectedItem = sourceTestControlItem.TestControlPropertySelectedItem;

                if (sourceTestControlItem.ChannelSelectionList != null)
                    targetTestControlItem.ChannelSelectionList = new ObservableCollection<string>(sourceTestControlItem.ChannelSelectionList);
                targetTestControlItem.ChannelSelectionSelectedItem = sourceTestControlItem.ChannelSelectionSelectedItem;
                //setTestControlItem.InputSelectionComboList = new ObservableCollection<string>(sourceTestControlItem.InputSelectionComboList);     // Constant List
                targetTestControlItem.InputSelectionComboSelectedItem = sourceTestControlItem.InputSelectionComboSelectedItem;
                targetTestControlItem.TestControlPropertyInitialValueSelectedItem = sourceTestControlItem.TestControlPropertyInitialValueSelectedItem;

                targetTestControlItem.LoopIsChecked = sourceTestControlItem.LoopIsChecked;
                targetTestControlItem.LoopStart = sourceTestControlItem.LoopStart;
                targetTestControlItem.LoopStartValueVisibility = sourceTestControlItem.LoopStartValueVisibility;
                targetTestControlItem.LoopEnd = sourceTestControlItem.LoopEnd;
                targetTestControlItem.LoopEndValueVisibility = sourceTestControlItem.LoopEndValueVisibility;
                targetTestControlItem.LoopIncrement = sourceTestControlItem.LoopIncrement;
                targetTestControlItem.LoopIncrementValueVisibility = sourceTestControlItem.LoopIncrementValueVisibility;

                targetTestControlItem.RampIsChecked = sourceTestControlItem.RampIsChecked;
                targetTestControlItem.RampSetting = sourceTestControlItem.RampSetting;
                targetTestControlItem.RampSettingVisibility = sourceTestControlItem.RampSettingVisibility;
                targetTestControlItem.valueTextboxVisibility = sourceTestControlItem.valueTextboxVisibility;
                targetTestControlItem.valueComboboxVisibility = sourceTestControlItem.valueComboboxVisibility;
                targetTestControlItem.RampCheckVisibility = sourceTestControlItem.RampCheckVisibility;
                targetTestControlItem.LoopCheckVisibility = sourceTestControlItem.LoopCheckVisibility;
                targetTestControlItem.ChannelEnabled = sourceTestControlItem.ChannelEnabled;
                targetTestControlItem.InputSelectionEnabled = sourceTestControlItem.InputSelectionEnabled;
                targetTestControlItem.valueIsEnabled = sourceTestControlItem.valueIsEnabled;
                targetTestControlItem.MaximumLimit = sourceTestControlItem.MaximumLimit;
                targetTestControlItem.MinimumLimit = sourceTestControlItem.MinimumLimit;
                targetTestControlItem.MaxLimitIsEnabled = sourceTestControlItem.MaxLimitIsEnabled;
                targetTestControlItem.MinLimitIsEnabled = sourceTestControlItem.MinLimitIsEnabled;

                return targetTestControlItem;
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                //MessageBox.Show("Exception\n " + ex.Message, "QAT Error Code - EC03021", MessageBoxButton.OK, MessageBoxImage.Error);
                return targetTestControlItem;
            }
        }

        public TestTelnetItem CopyTestTelnetItem(TestTelnetItem sourceTestTelnetItem)
        {
            TestTelnetItem targetTestTelnetItem = new TestTelnetItem();
            try
            {
                targetTestTelnetItem.ParentTestActionItem = sourceTestTelnetItem.ParentTestActionItem;

                targetTestTelnetItem.TelnetCommand = sourceTestTelnetItem.TelnetCommand;
                targetTestTelnetItem.TelnetSelectedDeviceItem = sourceTestTelnetItem.TelnetSelectedDeviceItem;
                targetTestTelnetItem.TelnetSelectedDeviceModel = sourceTestTelnetItem.TelnetSelectedDeviceModel;

                targetTestTelnetItem.TelnetSelectedDevice = sourceTestTelnetItem.TelnetSelectedDevice;

                List<DUT_DeviceItem> telnetDeviceItemList = new List<DUT_DeviceItem>();
                foreach (var item in sourceTestTelnetItem.TelnetDeviceItem)
                {
                    telnetDeviceItemList.Add(new DUT_DeviceItem(item));
                }
                targetTestTelnetItem.TelnetDeviceItem = new ObservableCollection<DUT_DeviceItem>(telnetDeviceItemList);
                targetTestTelnetItem.TelnetVerifyTypeSelected = sourceTestTelnetItem.TelnetVerifyTypeSelected;
                targetTestTelnetItem.TelnetVerifyTypeList = new ObservableCollection<string>(sourceTestTelnetItem.TelnetVerifyTypeList);

                targetTestTelnetItem.TelnetFailureTextIsEnabled = sourceTestTelnetItem.TelnetFailureTextIsEnabled;
                targetTestTelnetItem.TelnetFailureText = sourceTestTelnetItem.TelnetFailureText;

                return targetTestTelnetItem;
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                //MessageBox.Show("Exception\n " + ex.Message, "QAT Error Code - EC03021", MessageBoxButton.OK, MessageBoxImage.Error);
                return targetTestTelnetItem;
            }
        }
        public TestCECItem CopyTestCECItem(TestCECItem sourceTestCECItem)
        {
            TestCECItem targetTestCECItem = new TestCECItem();
            try
            {
                targetTestCECItem.ParentTestActionItem = sourceTestCECItem.ParentTestActionItem;

                targetTestCECItem.CECCommandList = sourceTestCECItem.CECCommandList;
                targetTestCECItem.CECCommandListSelectedItem = sourceTestCECItem.CECCommandListSelectedItem;
                targetTestCECItem.Deviceselection = sourceTestCECItem.Deviceselection;
                targetTestCECItem.DeviceselectionSelecetdItem = sourceTestCECItem.DeviceselectionSelecetdItem;
                targetTestCECItem.CECActionOpcode = sourceTestCECItem.CECActionOpcode;
                targetTestCECItem.CECActionOpcodeVisibility = sourceTestCECItem.CECActionOpcodeVisibility;
                return targetTestCECItem;
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                //MessageBox.Show("Exception\n " + ex.Message, "QAT Error Code - EC03021", MessageBoxButton.OK, MessageBoxImage.Error);
                return targetTestCECItem;
            }
        }
     
        public TestUsbAudioBridging CopyTestUsbItem(TestUsbAudioBridging sourceTestUsbItem)
        {
            TestUsbAudioBridging targetTestUsbItem = new TestUsbAudioBridging();
            try
            {
                targetTestUsbItem.ParentTestActionItem = sourceTestUsbItem.ParentTestActionItem;

                List<string> usbVal = new List<string>();
                foreach(string usbEachVal in sourceTestUsbItem.UsbAudioDeviceDisplay)
                {
                    usbVal.Add(usbEachVal);
                }

                List<string> djfhjd = new List<string>();

                djfhjd.AddRange(sourceTestUsbItem.UsbAudioDeviceDisplay);

                targetTestUsbItem.UsbAudioDeviceDisplay = new ObservableCollection<string>(usbVal);

                targetTestUsbItem.UsbAudioTypeSelectedItem = sourceTestUsbItem.UsbAudioTypeSelectedItem;

                targetTestUsbItem.UsbAudioBridgeTypeSelectedItem = sourceTestUsbItem.UsbAudioBridgeTypeSelectedItem;

                targetTestUsbItem.UsbAudioDeviceSelectedItem = sourceTestUsbItem.UsbAudioDeviceSelectedItem;

                targetTestUsbItem.UsbAudioBridgeDeviceComboEnable = sourceTestUsbItem.UsbAudioBridgeDeviceComboEnable;

                targetTestUsbItem.UsbDefaultDeviceOptionSelectedItem = sourceTestUsbItem.UsbDefaultDeviceOptionSelectedItem;

                targetTestUsbItem.UsbAudioDeviceList = sourceTestUsbItem.UsbAudioDeviceList;

                targetTestUsbItem.UsbAudioBridgeList = sourceTestUsbItem.UsbAudioBridgeList;

                //targetTestUsbItem.UsbAudioDeviceTypeList = sourceTestUsbItem.UsbAudioDeviceTypeList;
              
                return targetTestUsbItem;
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                //MessageBox.Show("Exception\n " + ex.Message, "QAT Error Code - EC03021", MessageBoxButton.OK, MessageBoxImage.Error);
                return targetTestUsbItem;
            }
        }
        private TestCaseItem parentTestCaseItemValue = null;
        public TestCaseItem ParentTestCaseItem
        {
            get { return parentTestCaseItemValue; }
            set { parentTestCaseItemValue = value; OnPropertyChanged("ParentTestCaseItem"); }
        }

        public bool ActionTabGridIsEnabled
        {
            get
            {
                if (ParentTestCaseItem != null)
                    return ParentTestCaseItem.ActionTabGridIsEnabled;
                else
                    return false;
            }
        }

        public ObservableCollection<String> TestActionList
        {
            get
            {
                if (ParentTestCaseItem != null)
                    return ParentTestCaseItem.TestActionList;
                else
                    return null;
            }
        }

        public ObservableCollection<String> TestVerificationList
        {
            get
            {
                if (ParentTestCaseItem != null)
                    return ParentTestCaseItem.TestVerificationList;
                else
                    return null;
            }
        }

        private string testActionItemNameValue = null;
        public String TestActionItemName
        {
            get { return testActionItemNameValue; }
            set { TestActionItemNameTextBox.Text = value; testActionItemNameValue = value; OnPropertyChanged("TestActionItemName"); }
        }

        private string testActionItemNameCopyValue = null;
        public String TestActionItemNameCopy
        {
            get { return testActionItemNameCopyValue; }
            set { testActionItemNameCopyValue = value; OnPropertyChanged("TestActionItemNameCopy"); }
        }

        private TextBox testCaseNameTextBoxValue = new TextBox();
        public TextBox TestActionItemNameTextBox
        {
            get { return testCaseNameTextBoxValue; }
            set { testCaseNameTextBoxValue = value; TestActionItemName = testCaseNameTextBoxValue.Text; OnPropertyChanged("TestCaseNameTextBox"); }
        }

        private int testActionIDValue = 0;
        public int TestActionID
        {
            get { return testActionIDValue; }
            set
            {
                testActionIDValue = value;
                OnPropertyChanged("TestActionID");
            }
        }

        private Visibility testActionTabItemNameVisibilityValue = Visibility.Visible;
        public Visibility TestActionTabItemNameVisibility
        {
            get { return testActionTabItemNameVisibilityValue; }
            set
            {
                testActionTabItemNameVisibilityValue = value;
                OnPropertyChanged("TestActionTabItemNameVisibility");
            }
        }

        private Visibility testActionTabDeleteButtonVisibilityValue = Visibility.Visible;
        public Visibility TestActionTabDeleteButtonVisibility
        {
            get { return testActionTabDeleteButtonVisibilityValue; }
            set
            {
                testActionTabDeleteButtonVisibilityValue = value;
                OnPropertyChanged("TestActionTabDeleteButtonVisibility");
            }
        }

        private ObservableCollection<TestControlItem> setTestControlListValue = new ObservableCollection<TestControlItem>();
        public ObservableCollection<TestControlItem> SetTestControlList
        {
            get { return setTestControlListValue; }
            set { setTestControlListValue = value; OnPropertyChanged("SetTestControlList"); }
        }

        private ObservableCollection<TestTelnetItem> setTestTelnetListValue = new ObservableCollection<TestTelnetItem>();
        public ObservableCollection<TestTelnetItem> SetTestTelnetList
        {
            get { return setTestTelnetListValue; }
            set { setTestTelnetListValue = value; OnPropertyChanged("SetTestTelnetList"); }
        }

        private ObservableCollection<TestCECItem> setTestCECListValue = new ObservableCollection<TestCECItem>();
        public ObservableCollection<TestCECItem> SetTestCECList
        {
            get { return setTestCECListValue; }
            set { setTestCECListValue = value; OnPropertyChanged("SetTestCECList"); }
        }

        private ObservableCollection<TestUserActionItem> setTestUserActionListValue = new ObservableCollection<TestUserActionItem>();
        public ObservableCollection<TestUserActionItem> SetTestUserActionList
        {
            get { return setTestUserActionListValue; }
            set { setTestUserActionListValue = value; OnPropertyChanged("SetTestUserActionList"); }
        }
             
        private ObservableCollection<TestUserVerifyItem> verifyTestUserListValue = new ObservableCollection<TestUserVerifyItem>();
        public ObservableCollection<TestUserVerifyItem> VerifyTestUserList
        {
            get { return verifyTestUserListValue; }
            set { verifyTestUserListValue = value; OnPropertyChanged("VerifyTestUserList"); }
        }

        private ObservableCollection<TestActionQRCMItem> setTestQRCMActionListValue = new ObservableCollection<TestActionQRCMItem>();
        public ObservableCollection<TestActionQRCMItem> SetTestQRCMActionList
        {
            get { return setTestQRCMActionListValue; }
            set { setTestQRCMActionListValue = value; OnPropertyChanged("SetTestQRCMActionList"); }
        }
        private ObservableCollection<TestVerifyQRCMItem> verifyTestQRCMListValue = new ObservableCollection<TestVerifyQRCMItem>();
        public ObservableCollection<TestVerifyQRCMItem> VerifyTestQRCMList
        {
            get { return verifyTestQRCMListValue; }
            set { verifyTestQRCMListValue = value; OnPropertyChanged("VerifyTestQRCMList"); }
        }

        private ObservableCollection<TestFirmwareItem> setTestFirmwareListValue = new ObservableCollection<TestFirmwareItem>();
        public ObservableCollection<TestFirmwareItem> SetTestFirmwareList
        {
            get { return setTestFirmwareListValue; }
            set { setTestFirmwareListValue = value; OnPropertyChanged("SetTestFirmwareList"); }
        }

        private ObservableCollection<TestDesignerItem> setTestDesignerListValue = new ObservableCollection<TestDesignerItem>();
        public ObservableCollection<TestDesignerItem> SetTestDesignerList
        {
            get { return setTestDesignerListValue; }
            set { setTestDesignerListValue = value; OnPropertyChanged("SetTestDesignerList"); }
        }

        private ObservableCollection<TestNetPairingItem> setTestNetPairingListValue = new ObservableCollection<TestNetPairingItem>();
        public ObservableCollection<TestNetPairingItem> SetTestNetPairingList
        {
            get { return setTestNetPairingListValue; }
            set { setTestNetPairingListValue = value; OnPropertyChanged("SetTestNetPairingList"); }
        }

        private ObservableCollection<TestUsbAudioBridging> setTestUsbListValue = new ObservableCollection<TestUsbAudioBridging>();
        public ObservableCollection<TestUsbAudioBridging> SetTestUsbList
        {
            get { return setTestUsbListValue; }
            set { setTestUsbListValue = value; OnPropertyChanged("SetTestUsbList"); }
        }

        private ObservableCollection<TestControlItem> verifyTestControlListValue = new ObservableCollection<TestControlItem>();
        public ObservableCollection<TestControlItem> VerifyTestControlList
        {
            get { return verifyTestControlListValue; }
            set { verifyTestControlListValue = value; OnPropertyChanged("VerifyTestControlList"); }
        }

        private ObservableCollection<TestTelnetItem> verifyTestTelnetListValue = new ObservableCollection<TestTelnetItem>();
        public ObservableCollection<TestTelnetItem> VerifyTestTelnetList
        {
            get { return verifyTestTelnetListValue; }
            set { verifyTestTelnetListValue = value; OnPropertyChanged("VerifyTestTelnetList"); }
        }

        private ObservableCollection<TestVerifyQRItem> VerifyTestQRListValue = new ObservableCollection<TestVerifyQRItem>();
        public ObservableCollection<TestVerifyQRItem> VerifyTestQRList
        {
            get { return VerifyTestQRListValue; }
            set { VerifyTestQRListValue = value; OnPropertyChanged("VerifyTestQRList"); }
        }

        private ObservableCollection<TestVerifyCECItem> VerifyTestCECListValue = new ObservableCollection<TestVerifyCECItem>();
        public ObservableCollection<TestVerifyCECItem> VerifyTestCECList
        {
            get { return VerifyTestCECListValue; }
            set { VerifyTestCECListValue = value; OnPropertyChanged("VerifyTestCECList"); }
        }

        private ObservableCollection<TestScriptVerification> VerifyTestScriptListValue = new ObservableCollection<TestScriptVerification>();
        public ObservableCollection<TestScriptVerification> VerifyTestScriptList
        {
            get { return VerifyTestScriptListValue; }
            set { VerifyTestScriptListValue = value; OnPropertyChanged("VerifyTestScriptList"); }
        }

        private ObservableCollection<TestUsbAudioBridging> verifyTestUsbListValue = new ObservableCollection<TestUsbAudioBridging>();
        public ObservableCollection<TestUsbAudioBridging> VerifyTestUsbList
        {
            get { return verifyTestUsbListValue; }
            set { verifyTestUsbListValue = value; OnPropertyChanged("VerifyTestUsbList"); }
        }

        private ObservableCollection<TestLuaItem> verifyTestLuaListValue = new ObservableCollection<TestLuaItem>();
        public ObservableCollection<TestLuaItem> VerifyTestLuaList
        {
            get { return verifyTestLuaListValue; }
            set { verifyTestLuaListValue = value; OnPropertyChanged("VerifyTestLuaList"); }
        }

        private ObservableCollection<TestLogItem> verifyTestLogListValue = new ObservableCollection<TestLogItem>();
        public ObservableCollection<TestLogItem> VerifyTestLogList
        {
            get { return verifyTestLogListValue; }
            set { verifyTestLogListValue = value; OnPropertyChanged("VerifyTestLogList"); }
        }

        private ObservableCollection<TestApxItem> verifyTestApxListValue = new ObservableCollection<TestApxItem>();
        public ObservableCollection<TestApxItem> VerifyTestApxList
        {
            get
            {
                return verifyTestApxListValue;
            }
            set { verifyTestApxListValue = value; OnPropertyChanged("VerifyTestApxList"); }
        }

        private ObservableCollection<TestSaveLogItem> testSaveLogItemListValue = new ObservableCollection<TestSaveLogItem>();
        public ObservableCollection<TestSaveLogItem> TestSaveLogItemList
        {
            get { return testSaveLogItemListValue; }
            set { testSaveLogItemListValue = value; OnPropertyChanged("TestSaveLogItemList"); }
        }

        private ObservableCollection<TestResponsalyzerItem> verifyTestResponsalyzerListValue = new ObservableCollection<TestResponsalyzerItem>();
        public ObservableCollection<TestResponsalyzerItem> verifyTestResponsalyzerList
        {
            get { return verifyTestResponsalyzerListValue; }
            set { verifyTestResponsalyzerListValue = value; OnPropertyChanged("verifyTestResponsalyzerList"); }
        }

        private string actionSelectedValue = "Control Action";
        public String ActionSelected
        {
            get { return actionSelectedValue; }
            set
            {
                actionSelectedValue = value;
                OnPropertyChanged("ActionSelected");

                SetTestControlList.Clear();
                SetTestTelnetList.Clear();
                SetTestCECList.Clear();
                SetTestFirmwareList.Clear();
                SetTestDesignerList.Clear();
                SetTestNetPairingList.Clear();
                SetTestUsbList.Clear();
                SetTestUserActionList.Clear();
                SetTestQRCMActionList.Clear();

                SetTestControlIsSelected = false;
                SetTestTelnetIsSelected = false;
                SetTestCECIsSelected = false;
                SetTestFirmwareIsSelected = false;
                SetTestDesignerIsSelected = false;
                SetTestNetPairingIsSelected = false;
                SetTestUsbIsSelected = false;
                SetTestUserActionIsSelected = false;
                SetTestQRCMIsSelected = false;
                SetTestSkipIsSelected = false;

                switch (value)
                {
                    case "Control Action":
                        if (SetTestControlList.Count < 2)
                        {
                            TestActionGridHeight = new GridLength(130, GridUnitType.Pixel);
                        }
                        else if (SetTestControlList.Count == 2)
                        {
                            TestActionGridHeight = new GridLength(260, GridUnitType.Pixel);
                        }
                        else if (SetTestControlList.Count > 2)
                        {
                            TestActionGridHeight = new GridLength(380, GridUnitType.Pixel);
                        }

                        AddSetTestControlItem();
                        SetTestControlIsSelected = true;
                        SelectActionPlusButtonVisibility = Visibility.Visible;
                        if (VerificationSelected != "Control Verification")
                            VerificationSelected = "Control Verification";
                        //VerifyTestControlList.Clear();
                        if (ParentTestCaseItem.IsEditModeEnabled)
                        {
                            SelectActionPlusButtonVisibility = Visibility.Visible;
                            //SelectActionMinusButtonVisibility = Visibility.Visible;
                        }
                        else
                        {
                            SelectActionPlusButtonVisibility = Visibility.Hidden;
                            //SelectActionMinusButtonVisibility = Visibility.Hidden;
                        }
                        break;
                    case "Ssh/Telnet Action":
                        TestActionGridHeight = new GridLength(0, GridUnitType.Auto);
                        AddSetTestTelnetItem();
                        SetTestTelnetIsSelected = true;
                        SelectActionPlusButtonVisibility = Visibility.Visible;
                        if (VerificationSelected != "Ssh/Telnet Verification")
                        {
                            VerificationSelected = "Ssh/Telnet Verification";
                        }

                        if (ParentTestCaseItem.IsEditModeEnabled)
                        {
                            SelectActionPlusButtonVisibility = Visibility.Visible;
                            //SelectActionMinusButtonVisibility = Visibility.Visible;
                        }
                        else
                        {
                            SelectActionPlusButtonVisibility = Visibility.Hidden;
                            //SelectActionMinusButtonVisibility = Visibility.Hidden;
                        }
                        break;
                    case "CEC Action":
                        if (SetTestCECList.Count < 2)
                        {
                            TestActionGridHeight = new GridLength(68, GridUnitType.Pixel);
                        }
                        else if (SetTestCECList.Count == 2)
                        {
                            TestActionGridHeight = new GridLength(136, GridUnitType.Pixel);
                        }
                        else if (SetTestCECList.Count > 2)
                        {
                            TestActionGridHeight = new GridLength(204, GridUnitType.Pixel);
                        }
                        //TestActionGridHeight = new GridLength(0, GridUnitType.Auto);
                        AddSetTestCECItem();
                        SetTestCECIsSelected = true;
                        SelectActionPlusButtonVisibility = Visibility.Visible;
                        if (ParentTestCaseItem.IsEditModeEnabled)
                        {
                            SelectActionPlusButtonVisibility = Visibility.Visible;
                            //SelectActionMinusButtonVisibility = Visibility.Visible;
                        }
                        else
                        {
                            SelectActionPlusButtonVisibility = Visibility.Hidden;
                            //SelectActionMinusButtonVisibility = Visibility.Hidden;
                        }

                        if (VerificationSelected != "CEC Verification")
                        {
                            VerificationSelected = "CEC Verification";
                        }
                        break;
                    case "Firmware Action":
                        TestActionGridHeight = new GridLength(0, GridUnitType.Auto);
                        AddSetTestFirmwareItem();
                        SetTestFirmwareIsSelected = true;
                        SelectActionPlusButtonVisibility = Visibility.Collapsed;
                        break;
                    case "Designer Action":
                        TestActionGridHeight = new GridLength(0, GridUnitType.Auto);
                        AddSetTestDesignerItem();
                        SetTestDesignerIsSelected = true;
                        SelectActionPlusButtonVisibility = Visibility.Collapsed;
                        break;
                    case "Net Pairing Action":
                        TestActionGridHeight = new GridLength(0, GridUnitType.Auto);
                        AddSetTestNetPairingItem();
                        SetTestNetPairingIsSelected = true;
                        SelectActionPlusButtonVisibility = Visibility.Collapsed;
                        break;
                    case "USB Action":
                        if (SetTestUsbList.Count < 2)
                        {
                            TestActionGridHeight = new GridLength(68, GridUnitType.Pixel);
                        }
                        else if (SetTestUsbList.Count == 2)
                        {
                            TestActionGridHeight = new GridLength(136, GridUnitType.Pixel);
                        }
                        else if (SetTestUsbList.Count > 2)
                        {
                            TestActionGridHeight = new GridLength(204, GridUnitType.Pixel);
                        }
                        AddSetTestUsbItem();
                        SetTestUsbIsSelected = true;
                        SelectActionPlusButtonVisibility = Visibility.Visible;
                        //if (VerificationSelected != "Telnet Verification")
                        //{
                        //    VerificationSelected = "Telnet Verification";
                        //}

                        if (ParentTestCaseItem.IsEditModeEnabled)
                        {
                            SelectActionPlusButtonVisibility = Visibility.Visible;
                            //SelectActionMinusButtonVisibility = Visibility.Visible;
                        }
                        else
                        {
                            SelectActionPlusButtonVisibility = Visibility.Hidden;
                            //SelectActionMinusButtonVisibility = Visibility.Hidden;
                        }

                        if (VerificationSelected != "USB Verification")
                        {
                            VerificationSelected = "USB Verification";
                        }
                        break;
                    case "User Action":
                        TestActionGridHeight = new GridLength(0, GridUnitType.Auto);
                        AddSetTestUserItem();
                        SetTestUserActionIsSelected = true;
                        //SelectActionPlusButtonVisibility = Visibility.Visible;
                        if (ParentTestCaseItem.IsEditModeEnabled)
                        {
                            SelectActionPlusButtonVisibility = Visibility.Visible;
                        }
                        else
                        {
                            SelectActionPlusButtonVisibility = Visibility.Hidden;
                        }
                        break;
                    case "QRCM Action":
                        TestActionGridHeight = new GridLength(0, GridUnitType.Auto);                        
                        AddSetTestQRCMItem();
                        SetTestQRCMIsSelected = true;
                        if (ParentTestCaseItem.IsEditModeEnabled)
                        {
                            SelectActionPlusButtonVisibility = Visibility.Visible;
                        }
                        else
                        {
                            SelectActionPlusButtonVisibility = Visibility.Hidden;
                        }
                        break;

                    case "Skip Action":
                        TestActionGridHeight = new GridLength(0, GridUnitType.Auto);
                        SetTestSkipIsSelected = true;
                        SelectActionPlusButtonVisibility = Visibility.Collapsed;
                        break;
                    default:
                        TestActionGridHeight = new GridLength(0, GridUnitType.Auto);
                        break;
                }

                if (ActionSelected == "QRCM Action")
                {
                    ActionQRCMVersionVisibility = Visibility.Visible;
                }
                else
                {
                    ActionQRCMVersionVisibility = Visibility.Hidden;
                }


                if (ParentTestCaseItem.IsEditModeEnabled)
                {
                    if (ActionSelected == "USB Action")
                    {
                        usbActionText = Visibility.Visible;
                    }
                    else
                    {
                        usbActionText = Visibility.Hidden;
                    }
                }
            }
        }

        private Visibility usbActionTextValue = Visibility.Hidden;
        public Visibility usbActionText
        {
            get { return usbActionTextValue; }
            set
            {
                usbActionTextValue = value;
                OnPropertyChanged("usbActionText");
            }
        }

        private Visibility usbVerificationTextValue = Visibility.Hidden;
        public Visibility usbVerificationText
        {
            get { return usbVerificationTextValue; }
            set
            {
                usbVerificationTextValue = value;
                OnPropertyChanged("usbVerificationText");
            }
        }

        private Visibility cecVerificationboxvalue = Visibility.Hidden;
        public Visibility cecVerificationbox
        {
            get { return cecVerificationboxvalue; }
            set
            {
                cecVerificationboxvalue = value;
                OnPropertyChanged("cecVerificationbox");
            }
        }
        private List<string> verifylogfromvalue = new List<string> { "Tab start time", "Test Case start time", "Test Plan start time" };
        public List<string> verifylogfrom
        {
            get { return verifylogfromvalue; }
            set
            {
                verifylogfromvalue = value;
                OnPropertyChanged("verifylogfrom");
            }
        }
        private string cecVerificationbox_selectedvalue = "Tab start time";
        public string cecVerificationbox_selected
        {
            get { return cecVerificationbox_selectedvalue; }
            set
            {
                cecVerificationbox_selectedvalue = value;
                OnPropertyChanged("cecVerificationbox_selected");
            }
        }
        private Visibility cectextvalue = Visibility.Hidden;
        public Visibility cectext
        {
            get { return cectextvalue; }
            set
            {
                cectextvalue = value;
                OnPropertyChanged("cectext");
            }
        }
        private Visibility selectActionPlusButtonVisibilityValue = Visibility.Collapsed;
        public Visibility SelectActionPlusButtonVisibility
        {
            get { return selectActionPlusButtonVisibilityValue; }
            private set { selectActionPlusButtonVisibilityValue = value; OnPropertyChanged("SelectActionPlusButtonVisibility"); }
        }

        private bool setTestControlIsSelectedValue = false;
        public bool SetTestControlIsSelected
        {
            get { return setTestControlIsSelectedValue; }
            private set { setTestControlIsSelectedValue = value; OnPropertyChanged("SetTestControlIsSelected"); }
        }

        private Visibility setTestControlVisibilityValue = Visibility.Collapsed;
        public Visibility SetTestControlVisibility
        {
            get { return setTestControlVisibilityValue; }
            private set { setTestControlVisibilityValue = value; OnPropertyChanged("SetTestControlVisibility"); }
        }

        private bool setTestTelnetIsSelectedValue = false;
        public bool SetTestTelnetIsSelected
        {
            get { return setTestTelnetIsSelectedValue; }
            private set { setTestTelnetIsSelectedValue = value; OnPropertyChanged("SetTestTelnetIsSelected"); }
        }

        private Visibility setTestTelnetVisibilityValue = Visibility.Collapsed;
        public Visibility SetTestTelnetVisibility
        {
            get { return setTestTelnetVisibilityValue; }
            private set { setTestTelnetVisibilityValue = value; OnPropertyChanged("SetTestTelnetVisibility"); }
        }

        private bool setTestCECIsSelectedValue = false;
        public bool SetTestCECIsSelected
        {
            get { return setTestCECIsSelectedValue; }
            private set { setTestCECIsSelectedValue = value; OnPropertyChanged("SetTestCECIsSelected"); }
        }

        private Visibility setTestCECVisibilityValue = Visibility.Collapsed;
        public Visibility SetTestCECVisibility
        {
            get { return setTestCECVisibilityValue; }
            private set { setTestCECVisibilityValue = value; OnPropertyChanged("SetTestCECVisibility"); }
        }

        private bool setTestUserActionIsSelectedValue = false;
        public bool SetTestUserActionIsSelected
        {
            get { return setTestUserActionIsSelectedValue; }
            private set { setTestUserActionIsSelectedValue = value; OnPropertyChanged("SetTestUserActionIsSelected"); }
        }

        private Visibility setTestUserActionVisibilityValue = Visibility.Collapsed;
        public Visibility SetTestUserActionVisibility
        {
            get { return setTestUserActionVisibilityValue; }
            private set { setTestUserActionVisibilityValue = value; OnPropertyChanged("SetTestUserActionVisibility"); }
        }
        private bool setTestQRCMIsSelectedValue = false;
        public bool SetTestQRCMIsSelected
        {
            get { return setTestQRCMIsSelectedValue; }
            private set { setTestQRCMIsSelectedValue = value; OnPropertyChanged("SetTestQRCMIsSelected"); }
        }

        private Visibility setTestQRCMVisibilityValue = Visibility.Collapsed;
        public Visibility SetTestQRCMVisibility
        {
            get { return setTestQRCMVisibilityValue; }
            private set { setTestQRCMVisibilityValue = value; OnPropertyChanged("SetTestQRCMVisibility"); }
        }

        private bool setTestFirmwareIsSelectedValue = false;
        public bool SetTestFirmwareIsSelected
        {
            get { return setTestFirmwareIsSelectedValue; }
            private set { setTestFirmwareIsSelectedValue = value; OnPropertyChanged("SetTestFirmwareIsSelected"); }
        }

        private Visibility setTestFirmwareVisibilityValue = Visibility.Collapsed;
        public Visibility SetTestFirmwareVisibility
        {
            get { return setTestFirmwareVisibilityValue; }
            private set { setTestFirmwareVisibilityValue = value; OnPropertyChanged("SetTestFirmwareVisibility"); }
        }

        private bool setTestDesignerIsSelectedValue = false;
        public bool SetTestDesignerIsSelected
        {
            get { return setTestDesignerIsSelectedValue; }
            private set { setTestDesignerIsSelectedValue = value; OnPropertyChanged("SetTestDesignerIsSelected"); }
        }

        private Visibility setTestDesignerVisibilityValue = Visibility.Collapsed;
        public Visibility SetTestDesignerVisibility
        {
            get { return setTestDesignerVisibilityValue; }
            private set { setTestDesignerVisibilityValue = value; OnPropertyChanged("SetTestDesignerVisibility"); }
        }

        private bool setTestNetPairingIsSelectedValue = false;
        public bool SetTestNetPairingIsSelected
        {
            get { return setTestNetPairingIsSelectedValue; }
            private set { setTestNetPairingIsSelectedValue = value; OnPropertyChanged("SetTestNetPairingIsSelected"); }
        }

        private Visibility setTestNetPairingVisibilityValue = Visibility.Collapsed;
        public Visibility SetTestNetPairingVisibility
        {
            get { return setTestNetPairingVisibilityValue; }
            private set { setTestNetPairingVisibilityValue = value; OnPropertyChanged("SetTestNetPairingVisibility"); }
        }
		
		  private bool setTestUsbIsSelectedValue = false;
        public bool SetTestUsbIsSelected
        {
            get { return setTestUsbIsSelectedValue; }
            private set { setTestUsbIsSelectedValue = value; OnPropertyChanged("SetTestUsbIsSelected"); }
        }

        private Visibility setTestUsbVisibilityValue = Visibility.Collapsed;
        public Visibility SetTestUsbVisibility
        {
            get { return setTestUsbVisibilityValue; }
            private set { setTestUsbVisibilityValue = value; OnPropertyChanged("SetTestUsbVisibility"); }
        }


        private bool setTestSkipIsSelectedValue = false;
        public bool SetTestSkipIsSelected
        {
            get { return setTestSkipIsSelectedValue; }
            private set { setTestSkipIsSelectedValue = value; OnPropertyChanged("SetTestSkipIsSelected"); }
        }

        private Visibility setTestSkipVisibilityValue = Visibility.Collapsed;
        public Visibility SetTestSkipVisibility
        {
            get { return setTestSkipVisibilityValue; }
            private set { setTestSkipVisibilityValue = value; OnPropertyChanged("SetTestSkipVisibility"); }
        }

        private Visibility LoglabeltextvisibilityValue = Visibility.Hidden;
        public Visibility Loglabeltextvisibility
        {
            get { return LoglabeltextvisibilityValue; }
            set
            {
                LoglabeltextvisibilityValue = value;
                OnPropertyChanged("Loglabeltextvisibility");
            }
        }

        private string verificationSelectedValue = "Control Verification";
        public String VerificationSelected
        {
            get { return verificationSelectedValue; }
            set
            {
                verificationSelectedValue = value;
                OnPropertyChanged("VerificationSelected");

                ActionGridHeight = new GridLength(0, GridUnitType.Auto);
                VerifyGridHeight = new GridLength(0, GridUnitType.Auto);

                VerifyTestControlList.Clear();
                VerifyTestTelnetList.Clear();
                VerifyTestCECList.Clear();
                VerifyTestUsbList.Clear();
                VerifyTestLuaList.Clear();
                VerifyTestLogList.Clear();
                VerifyTestApxList.Clear();
                verifyTestResponsalyzerList.Clear();
                VerifyTestQRList.Clear();
                VerifyTestScriptList.Clear();
                VerifyTestUserList.Clear();
                VerifyTestQRCMList.Clear();

                VerifyTestControlIsSelected = false;
                VerifyTestTelnetIsSelected = false;
                VerifyTestCECIsSelected = false;
                VerifyTestUsbIsSelected = false;
                VerifyTestLuaIsSelected = false;
                VerifyTestLogIsSelected = false;
                VerifyTestApxIsSelected = false;
                VerifyTestResponalyzerIsSelected = false;
                VerifyTestSkipIsSelected = false;
                VerifyTestQRIsSelected = false;
                VerifyTestScriptIsSelected = false;
                UserVerificationIsSelected = false;
                VerifyTestQRCMIsSelected = false;

                switch (value)
                {
                    case "Control Verification":
                        if (VerifyTestControlList.Count < 2)
                        {
                            TestVerificationGridHeight = new GridLength(130, GridUnitType.Pixel);
                        }
                        else if (VerifyTestControlList.Count == 2)
                        {
                            TestVerificationGridHeight = new GridLength(260, GridUnitType.Pixel);
                        }
                        else if (VerifyTestControlList.Count > 2)
                        {
                            TestVerificationGridHeight = new GridLength(380, GridUnitType.Pixel);
                        }
                        AddVerifyTestControlItem();
                        VerifyTestControlIsSelected = true;
                        SelectVerificationPlusButtonVisibility = Visibility.Visible;
                        if (ParentTestCaseItem.IsEditModeEnabled)
                        {
                            SelectVerificationPlusButtonVisibility = Visibility.Visible;
                            //SelectActionMinusButtonVisibility = Visibility.Visible;
                        }
                        else
                        {
                            SelectVerificationPlusButtonVisibility = Visibility.Hidden;
                            //SelectActionMinusButtonVisibility = Visibility.Hidden;
                        }
                        break;
                    case "Ssh/Telnet Verification":
                        TestVerificationGridHeight = new GridLength(0, GridUnitType.Auto);
                        AddVerifyTestTelnetItem();
                        VerifyTestTelnetIsSelected = true;
                        SelectVerificationPlusButtonVisibility = Visibility.Collapsed;
                        break;
                    case "CEC Verification":
                        if (VerifyTestCECList.Count < 2)
                        {
                            TestVerificationGridHeight = new GridLength(68, GridUnitType.Pixel);
                        }
                        else if (VerifyTestCECList.Count == 2)
                        {
                            TestVerificationGridHeight = new GridLength(136, GridUnitType.Pixel);
                        }
                        else if (VerifyTestCECList.Count > 2)
                        {
                            TestVerificationGridHeight = new GridLength(204, GridUnitType.Pixel);
                        }
                        //TestVerificationGridHeight = new GridLength(0, GridUnitType.Auto);
                        AddVerifyTestCECItem();
                        VerifyTestCECIsSelected = true;
                        //SelectVerificationPlusButtonVisibility = Visibility.Collapsed;
                        SelectVerificationPlusButtonVisibility = Visibility.Visible;
                        if (ParentTestCaseItem.IsEditModeEnabled)
                        {
                            SelectVerificationPlusButtonVisibility = Visibility.Visible;
                            //SelectActionMinusButtonVisibility = Visibility.Visible;
                        }
                        else
                        {
                            SelectVerificationPlusButtonVisibility = Visibility.Hidden;
                            //SelectActionMinusButtonVisibility = Visibility.Hidden;
                        }
                        break;
                    case "QR code Verification":
                        if (VerifyTestQRList.Count < 2)
                        {
                            TestVerificationGridHeight = new GridLength(68, GridUnitType.Pixel);
                        }
                        else if (VerifyTestQRList.Count == 2)
                        {
                            TestVerificationGridHeight = new GridLength(136, GridUnitType.Pixel);
                        }
                        else if (VerifyTestQRList.Count > 2)
                        {
                            TestVerificationGridHeight = new GridLength(204, GridUnitType.Pixel);
                        }
                        //TestVerificationGridHeight = new GridLength(0, GridUnitType.Auto);
                        AddVerifyTestQRItem();
                        VerifyTestQRIsSelected = true;
                        //SelectVerificationPlusButtonVisibility = Visibility.Collapsed;
                        SelectVerificationPlusButtonVisibility = Visibility.Visible;
                        if (ParentTestCaseItem.IsEditModeEnabled)
                        {
                            SelectVerificationPlusButtonVisibility = Visibility.Visible;
                            //SelectActionMinusButtonVisibility = Visibility.Visible;
                        }
                        else
                        {
                            SelectVerificationPlusButtonVisibility = Visibility.Hidden;
                            //SelectActionMinusButtonVisibility = Visibility.Hidden;
                        }
                        break;

                    case "USB Verification":
                        if (VerifyTestUsbList.Count < 2)
                        {
                            TestVerificationGridHeight = new GridLength(68, GridUnitType.Pixel);
                        }
                        else if (VerifyTestUsbList.Count == 2)
                        {
                            TestVerificationGridHeight = new GridLength(136, GridUnitType.Pixel);
                        }
                        else if (VerifyTestUsbList.Count > 2)
                        {
                            TestVerificationGridHeight = new GridLength(204, GridUnitType.Pixel);
                        }
                        AddVerifyTestUsbItem();
                        VerifyTestUsbIsSelected = true;
                        SelectVerificationPlusButtonVisibility = Visibility.Visible;
                        if (ParentTestCaseItem.IsEditModeEnabled)
                        {
                            SelectVerificationPlusButtonVisibility = Visibility.Visible;
                            //SelectActionMinusButtonVisibility = Visibility.Visible;
                        }
                        else
                        {
                            SelectVerificationPlusButtonVisibility = Visibility.Hidden;
                            //SelectActionMinusButtonVisibility = Visibility.Hidden;
                        }
                        break;
                    case "LUA Text Verification":
                        TestVerificationGridHeight = new GridLength(0, GridUnitType.Auto); 
                        AddVerifyTestLuaItem();
                        VerifyTestLuaIsSelected = true;
                        SelectVerificationPlusButtonVisibility = Visibility.Collapsed;
                        break;
                    case "Log Verification":
                        TestVerificationGridHeight = new GridLength(0, GridUnitType.Auto); ;
                        AddVerifyTestLogItem();
                        VerifyTestLogIsSelected = true;
                        SelectVerificationPlusButtonVisibility = Visibility.Collapsed;
                        break;
                    case "Audio Precision Verification":
                        TestVerificationGridHeight = new GridLength(0, GridUnitType.Auto); 
                        string installationstatus = GetApplicationInstallPath("Audio Precision APx500");
                        if (installationstatus == string.Empty)
                        {
                            MessageBox.Show("Please Install Audio Precision APx500 4.1 software before creating Audio Precision Verification", "Warning", MessageBoxButton.OK, MessageBoxImage.Information);
                            break;
                        }
                        else
                        {
                            var result = new Version(installationstatus).CompareTo(new Version("4.1"));
                            if (result != 0)
                            {
                                MessageBox.Show("Please Install Audio Precision APx500 4.1 software before creating Audio Precision Verification", "Warning", MessageBoxButton.OK, MessageBoxImage.Information);
                                break;
                            }
                            else
                            {
                                AddVerifyTestApxItem();
                                VerifyTestApxIsSelected = true;
                                SelectVerificationPlusButtonVisibility = Visibility.Collapsed;
                                break;
                            }
                        }
                    case "Script Verification":
                        TestVerificationGridHeight = new GridLength(0, GridUnitType.Auto); 
                        AddVerifyTestScriptItem();
                        VerifyTestScriptIsSelected = true;
                        if (ParentTestCaseItem.IsEditModeEnabled)                        
                            SelectVerificationPlusButtonVisibility = Visibility.Visible; 
                        else                        
                            SelectVerificationPlusButtonVisibility = Visibility.Hidden;                         
                        
                        break;
                    case "User Verification":
                        TestVerificationGridHeight = new GridLength(0, GridUnitType.Auto);
                        AddTestUserVerifyItem();
                        UserVerificationIsSelected = true;
                        if (ParentTestCaseItem.IsEditModeEnabled)
                        {
                            SelectVerificationPlusButtonVisibility = Visibility.Visible;                            
                        }
                        else
                        {
                            SelectVerificationPlusButtonVisibility = Visibility.Hidden;                          
                        }
                        break;
                    case "QRCM Verification":
                        TestVerificationGridHeight = new GridLength(0, GridUnitType.Auto);
                        AddVerifyTestQRCMItem();
                        VerifyTestQRCMIsSelected = true;
                        if (ParentTestCaseItem.IsEditModeEnabled)
                        {
                            SelectVerificationPlusButtonVisibility = Visibility.Visible;
                        }
                        else
                        {
                            SelectVerificationPlusButtonVisibility = Visibility.Hidden;
                        }
                        break;
                    case "Skip Verification":
                        TestVerificationGridHeight = new GridLength(0, GridUnitType.Auto);
                        VerifyTestSkipIsSelected = true;
                        SelectVerificationPlusButtonVisibility = Visibility.Collapsed;
                        break;

                    case "Responsalyzer":
                        TestVerificationGridHeight = new GridLength(0, GridUnitType.Auto); 
                        AddTestResponsalyzerItem();
                        VerifyTestResponalyzerIsSelected = true;
                        SelectVerificationPlusButtonVisibility = Visibility.Visible;
                        if (ParentTestCaseItem.IsEditModeEnabled)
                        {
                            SelectVerificationPlusButtonVisibility = Visibility.Visible;
                            //SelectActionMinusButtonVisibility = Visibility.Visible;
                        }
                        else
                        {
                            SelectVerificationPlusButtonVisibility = Visibility.Hidden;
                            //SelectActionMinusButtonVisibility = Visibility.Hidden;
                        }
                        break;

                    default:
                        TestVerificationGridHeight = new GridLength(0, GridUnitType.Auto);
                        break;
                }

                if (VerificationSelected == "QRCM Verification")
                {
                    VerifyQRCMVersionVisibility = Visibility.Visible;
                }
                else
                {
                    VerifyQRCMVersionVisibility = Visibility.Hidden;
                }


                if (ParentTestCaseItem.IsEditModeEnabled)
                {
                    if (VerificationSelected == "USB Verification")
                    {
                        usbVerificationText = Visibility.Visible;
                    }
                    else
                    {
                        usbVerificationText = Visibility.Hidden;
                    }
                    if (VerificationSelected == "Log Verification")
                    {
                        Loglabeltextvisibility = Visibility.Visible;
                    }
                    else
                    {
                        Loglabeltextvisibility = Visibility.Hidden;
                    }
                }
                else
                {
                    Loglabeltextvisibility = Visibility.Hidden;
                }

                if (VerificationSelected == "CEC Verification")
                {
                    cectext = Visibility.Visible;
                    cecVerificationbox = Visibility.Visible;
                }
                else
                {
                    cectext = Visibility.Hidden;
                    cecVerificationbox = Visibility.Hidden;
                }

                if (VerificationSelected == "Script Verification")
                {
                    ScriptExecuteIterationChkbxVisibility = Visibility.Visible;
                    ScriptExecuteIterationChkbxEnable = true;
                    this.TestSaveLogItemList[0].ScreenShotIsEnable = false;
                           

                    if (TestSaveLogItemList[0].ActionSaveLogEventList.Contains("Save during Error"))
                        TestSaveLogItemList[0].ActionSaveLogEventList.Remove("Save during Error");
                    if (TestSaveLogItemList[0].ActionSaveLogEventList.Contains("Save logs always"))
                        TestSaveLogItemList[0].ActionSaveLogEventList.Remove("Save logs always");

                    TestSaveLogItemList[0].ActionSaveLogEventSelected = "Never Save logs";
                }
                else
                {
                    Script_checktimeTextbox = string.Empty;
                    Script_DurationTextbox = string.Empty;
                    ExecuteIterationChkboxIsChecked = false;
                    ScriptExecuteIterationChkbxVisibility = Visibility.Hidden;
                    this.TestSaveLogItemList[0].ScreenShotIsEnable = true;
                   
                    if (!TestSaveLogItemList[0].ActionSaveLogEventList.Contains("Save during Error"))
                        TestSaveLogItemList[0].ActionSaveLogEventList.Add("Save during Error");
                    if (!TestSaveLogItemList[0].ActionSaveLogEventList.Contains("Save logs always"))
                        TestSaveLogItemList[0].ActionSaveLogEventList.Add("Save logs always");
                    TestSaveLogItemList[0].ActionSaveLogEventSelected = "Save during Error";
                }
            }
        }

        public static string GetApplicationInstallPath(string NameOfAppToFind)
        {
            string installedPath;
            string keyName;
            try
            {
                // search in: CurrentUser
                keyName = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall";
                installedPath = ExistsInSubKey(Registry.CurrentUser, keyName, "DisplayName", NameOfAppToFind);
                if (!string.IsNullOrEmpty(installedPath))
                {
                    DeviceDiscovery.WriteToLogFile("Designer exe installed path is: Audio Precision APx500 " + installedPath + "");
                    return installedPath;
                }

                // search in: LocalMachine_32
                keyName = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall";
                installedPath = ExistsInSubKey(Registry.LocalMachine, keyName, "DisplayName", NameOfAppToFind);
                if (!string.IsNullOrEmpty(installedPath))
                {
                    DeviceDiscovery.WriteToLogFile("Designer exe installed path is: Audio Precision APx500 " + installedPath + "");
                    return installedPath;
                }

                // search in: LocalMachine_64
                keyName = @"SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall";
                installedPath = ExistsInSubKey(Registry.LocalMachine, keyName, "DisplayName", NameOfAppToFind);
                if (!string.IsNullOrEmpty(installedPath))
                {
                    DeviceDiscovery.WriteToLogFile("Designer exe installed path is: Audio Precision APx500 " + installedPath + "");
                    return installedPath;
                }

                //Search in: for accessing 64bit registry
                RegistryKey localKey64 = RegistryKey.OpenBaseKey(Microsoft.Win32.RegistryHive.LocalMachine, RegistryView.Registry64);
                keyName = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall";
                installedPath = ExistsInSubKey(localKey64, keyName, "DisplayName", NameOfAppToFind);
                if (!string.IsNullOrEmpty(installedPath))
                {
                    DeviceDiscovery.WriteToLogFile("Designer exe installed path is: Audio Precision APx500 " + installedPath + "");
                    return installedPath;
                }

                //Search in: for accessing 32bit registry
                RegistryKey localKey32 = RegistryKey.OpenBaseKey(Microsoft.Win32.RegistryHive.LocalMachine, RegistryView.Registry32);
                keyName = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall";
                installedPath = ExistsInSubKey(localKey32, keyName, "DisplayName", NameOfAppToFind);
                if (!string.IsNullOrEmpty(installedPath))
                {
                    DeviceDiscovery.WriteToLogFile("Designer exe installed path is: Audio Precision APx500 " + installedPath + "");
                    return installedPath;
                }


                DeviceDiscovery.WriteToLogFile("Designer exe installed path not found ");
                return string.Empty;
            }
            catch(Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                DeviceDiscovery.WriteToLogFile("Designer exe installed path not found ");
                return string.Empty;
            }
        }

        public static string ExistsInSubKey(RegistryKey root, string subKeyName, string attributeName, string versionOfAppToFind)
        {
            try
            {
                RegistryKey subkey;
                string displayName;

                using (RegistryKey key = root.OpenSubKey(subKeyName))
                {
                    if (key != null)
                    {
                        foreach (string kn in key.GetSubKeyNames())
                        {
                            using (subkey = key.OpenSubKey(kn))
                            {
                                displayName = subkey.GetValue(attributeName) as string;                             
                                if (displayName != null && versionOfAppToFind != null && displayName!=string.Empty)
                                {
                                    if (displayName.ToUpper().StartsWith(versionOfAppToFind.ToUpper()))
                                    {
                                        DeviceDiscovery.WriteToLogFile("Audio Precision software installed path found ");
                                        //MessageBox.Show(subkey.GetValue("DisplayVersion") as string);

                                        var result = new Version(subkey.GetValue("DisplayVersion") as string).CompareTo(new Version("4.1"));

                                        if(result == 0)
                                            return subkey.GetValue("DisplayVersion") as string;
                                    }
                                }
                            }
                        }
                    }
                }

                DeviceDiscovery.WriteToLogFile("Audio Precision software installed path not found");
                return string.Empty;
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                //MessageBox.Show("Exception\n " + ex.Message, "Error Code - ECxxxxx", MessageBoxButton.OK, MessageBoxImage.Error);
                DeviceDiscovery.WriteToLogFile("Designer exe installed path not found ");
                return string.Empty;
            }
        }

        private Visibility selectVerificationPlusButtonVisibilityValue = Visibility.Collapsed;
        public Visibility SelectVerificationPlusButtonVisibility
        {
            get { return selectVerificationPlusButtonVisibilityValue; }
            private set { selectVerificationPlusButtonVisibilityValue = value; OnPropertyChanged("SelectVerificationPlusButtonVisibility"); }
        }

        private bool verifyTestControlIsSelectedValue = false;
        public bool VerifyTestControlIsSelected
        {
            get { return verifyTestControlIsSelectedValue; }
            private set { verifyTestControlIsSelectedValue = value; OnPropertyChanged("VerifyTestControlIsSelected"); }
        }

        private Visibility verifyTestControlVisibilityValue = Visibility.Collapsed;
        public Visibility VerifyTestControlVisibility
        {
            get { return verifyTestControlVisibilityValue; }
            private set { verifyTestControlVisibilityValue = value; OnPropertyChanged("VerifyTestControlVisibility"); }
        }

        private bool verifyTestTelnetIsSelectedValue = false;
        public bool VerifyTestTelnetIsSelected
        {
            get { return verifyTestTelnetIsSelectedValue; }
            private set { verifyTestTelnetIsSelectedValue = value; OnPropertyChanged("VerifyTestTelnetIsSelected"); }
        }

        private Visibility verifyTestTelnetVisibilityValue = Visibility.Collapsed;
        public Visibility VerifyTestTelnetVisibility
        {
            get { return verifyTestTelnetVisibilityValue; }
            private set { verifyTestTelnetVisibilityValue = value; OnPropertyChanged("VerifyTestTelnetVisibility"); }
        }

        private bool verifyTestQRCMIsSelectedValue = false;
        public bool VerifyTestQRCMIsSelected
        {
            get { return verifyTestQRCMIsSelectedValue; }
            private set { verifyTestQRCMIsSelectedValue = value; OnPropertyChanged("VerifyTestQRCMIsSelected"); }
        }

        private Visibility verifyTestQRCMVisibilityValue = Visibility.Collapsed;
        public Visibility VerifyTestQRCMVisibility
        {
            get { return verifyTestQRCMVisibilityValue; }
            private set { verifyTestQRCMVisibilityValue = value; OnPropertyChanged("VerifyTestQRCMVisibility"); }
        }
        private bool userVerificationIsSelectedValue = false;
        public bool UserVerificationIsSelected
        {
            get { return userVerificationIsSelectedValue; }
            private set { userVerificationIsSelectedValue = value; OnPropertyChanged("UserVerificationIsSelected"); }
        }

        private Visibility verifyTestUserVisibilityValue = Visibility.Collapsed;
        public Visibility VerifyTestUserVisibility
        {
            get { return verifyTestUserVisibilityValue; }
            private set { verifyTestUserVisibilityValue = value; OnPropertyChanged("VerifyTestUserVisibility"); }
        }        

        private bool VerifyTestCECIsSelectedValue = false;
        public bool VerifyTestCECIsSelected
        {
            get { return VerifyTestCECIsSelectedValue; }
            private set { VerifyTestCECIsSelectedValue = value; OnPropertyChanged("VerifyTestCECIsSelected"); }
        }

        private bool VerifyTestQRIsSelectedValue = false;
        public bool VerifyTestQRIsSelected
        {
            get { return VerifyTestQRIsSelectedValue; }
            private set { VerifyTestQRIsSelectedValue = value; OnPropertyChanged("VerifyTestQRIsSelected"); }
        }
        private Visibility VerifyTestCECVisibilityValue = Visibility.Collapsed;
        public Visibility VerifyTestCECVisibility
        {
            get { return VerifyTestCECVisibilityValue; }
            private set { VerifyTestCECVisibilityValue = value; OnPropertyChanged("VerifyTestCECVisibility"); }
        }
        private Visibility VerifyTestQRVisibilityValue = Visibility.Collapsed;
        public Visibility VerifyTestQRVisibility
        {
            get { return VerifyTestQRVisibilityValue; }
            private set { VerifyTestQRVisibilityValue = value; OnPropertyChanged("VerifyTestQRVisibility"); }
        }
        private bool verifyTestUsbIsSelectedValue = false;
        public bool VerifyTestUsbIsSelected
        {
            get { return verifyTestUsbIsSelectedValue; }
            private set { verifyTestUsbIsSelectedValue = value; OnPropertyChanged("VerifyTestUsbIsSelected"); }
        }

        private Visibility verifyTestUsbVisibilityValue = Visibility.Collapsed;
        public Visibility VerifyTestUsbVisibility
        {
            get { return verifyTestUsbVisibilityValue; }
            private set { verifyTestUsbVisibilityValue = value; OnPropertyChanged("VerifyTestUsbVisibility"); }
        }
        private bool verifyTestLuaIsSelectedValue = false;
        public bool VerifyTestLuaIsSelected
        {
            get { return verifyTestLuaIsSelectedValue; }
            private set { verifyTestLuaIsSelectedValue = value; OnPropertyChanged("VerifyTestLuaIsSelected"); }
        }

        private Visibility verifyTestLuaVisibilityValue = Visibility.Collapsed;
        public Visibility VerifyTestLuaVisibility
        {
            get { return verifyTestLuaVisibilityValue; }
            private set { verifyTestLuaVisibilityValue = value; OnPropertyChanged("VerifyTestLuaVisibility"); }
        }

        private bool verifyTestLogIsSelectedValue = false;
        public bool VerifyTestLogIsSelected
        {
            get { return verifyTestLogIsSelectedValue; }
            private set { verifyTestLogIsSelectedValue = value; OnPropertyChanged("VerifyTestLogIsSelected"); }
        }

        private Visibility verifyTestLogVisibilityValue = Visibility.Collapsed;
        public Visibility VerifyTestLogVisibility
        {
            get { return verifyTestLogVisibilityValue; }
            private set { verifyTestLogVisibilityValue = value; OnPropertyChanged("VerifyTestLogVisibility"); }
        }

        private bool verifyTestApxIsSelectedValue = false;
        public bool VerifyTestApxIsSelected
        {
            get { return verifyTestApxIsSelectedValue; }
            private set { verifyTestApxIsSelectedValue = value; OnPropertyChanged("VerifyTestApxIsSelected"); }
        }

        private Visibility verifyTestApxVisibilityValue = Visibility.Collapsed;
        public Visibility VerifyTestApxVisibility
        {
            get { return verifyTestApxVisibilityValue; }
            private set { verifyTestApxVisibilityValue = value; OnPropertyChanged("VerifyTestApxVisibility"); }
        }

        private bool verifyTestResponalyzerIsSelectedValue = false;
        public bool VerifyTestResponalyzerIsSelected
        {
            get { return verifyTestResponalyzerIsSelectedValue; }
            private set { verifyTestResponalyzerIsSelectedValue = value; OnPropertyChanged("VerifyTestResponalyzerIsSelected"); }
        }

        private Visibility verifyTestResponalyzerVisibilityValue = Visibility.Collapsed;
        public Visibility VerifyTestResponalyzerVisibility
        {
            get { return verifyTestResponalyzerVisibilityValue; }
            private set { verifyTestResponalyzerVisibilityValue = value; OnPropertyChanged("VerifyTestResponalyzerVisibility"); }
        }

        private bool verifyTestSkipIsSelectedValue = false;
        public bool VerifyTestSkipIsSelected
        {
            get { return verifyTestSkipIsSelectedValue; }
            private set { verifyTestSkipIsSelectedValue = value; OnPropertyChanged("VerifyTestSkipIsSelected"); }
        }

        private Visibility verifyTestSkipVisibilityValue = Visibility.Collapsed;
        public Visibility VerifyTestSkipVisibility
        {
            get { return verifyTestSkipVisibilityValue; }
            private set { verifyTestSkipVisibilityValue = value; OnPropertyChanged("VerifyTestSkipVisibility"); }
        }

        private string VerificationdelayValue = null;
        public string Verificationdelay
        {
            get { return VerificationdelayValue; }
            set { VerificationdelayValue = value; OnPropertyChanged("Verificationdelay"); }
        }

        private string RerundelayValue = null;
        public string Rerundelay
        {
            get { return RerundelayValue; }
            set { RerundelayValue = value; OnPropertyChanged("Rerundelay"); }
        }

        private List<string> actionDelayUnitListValue = new List<string> { "Hour", "Min", "Sec", "msec" };
        public List<string> ActionDelayUnitList
        {
            get { return actionDelayUnitListValue; }
            set { actionDelayUnitListValue = value; OnPropertyChanged("ActionDelayUnitList"); }
        }

        private ObservableCollection<string> actionQRCMVersionListValue = new ObservableCollection<string>();
        public ObservableCollection<string> ActionQRCMVersionList
        {
            get
            {
                actionQRCMVersionListValue = QatConstants.QRCMversionList;
                return actionQRCMVersionListValue;
            }
            set
            {
                actionQRCMVersionListValue = value;
                actionQRCMVersionListValue = QatConstants.QRCMversionList;
                OnPropertyChanged("ActionQRCMVersionList");
            }
        }
		   
        private ObservableCollection<string> verifyQRCMVersionListValue = new ObservableCollection<string>();
        public ObservableCollection<string> VerifyQRCMVersionList
        {
            get
            {
                verifyQRCMVersionListValue = QatConstants.QRCMversionList;
                return verifyQRCMVersionListValue;
            }
            set
            {
                verifyQRCMVersionListValue = value;
                verifyQRCMVersionListValue = QatConstants.QRCMversionList;
                OnPropertyChanged("VerifyQRCMVersionList");
            }
        }

        private string actionQRCMVersionSelectedValue = null;
        public string ActionQRCMVersionSelected
        {
            get
            {
                return actionQRCMVersionSelectedValue;
            }
            set
            {
                actionQRCMVersionSelectedValue = value;
                OnPropertyChanged("ActionQRCMVersionSelected");
            }
        }

        private string actionQRCMPreVerselectedValue = null;
        public string ActionQRCMPreVerSelected
        {
            get
            {
                return actionQRCMPreVerselectedValue;
            }
            set
            {
                actionQRCMPreVerselectedValue = value;               
            }
        }

        private string verifyQRCMVersionSelectedValue = null;
        public string VerifyQRCMVersionSelected
        {
            get
            {
                return verifyQRCMVersionSelectedValue;
            }
            set
            {
                verifyQRCMVersionSelectedValue = value;
                OnPropertyChanged("VerifyQRCMVersionSelected");
            }
        }

        private string verifyQRCMPreVerselectedValue = null;
        public string VerifyQRCMPreVerSelected
        {
            get
            {
                return verifyQRCMPreVerselectedValue;
            }
            set
            {
                verifyQRCMPreVerselectedValue = value;
            }
        }

        private Visibility actionQRCMVersionVisibilityValue = Visibility.Collapsed;
        public Visibility ActionQRCMVersionVisibility
        {
            get { return actionQRCMVersionVisibilityValue; }
            set { actionQRCMVersionVisibilityValue = value; OnPropertyChanged("ActionQRCMVersionVisibility"); }
        }
        private Visibility verifyQRCMVersionVisibilityValue = Visibility.Collapsed;
        public Visibility VerifyQRCMVersionVisibility
        {
            get { return verifyQRCMVersionVisibilityValue; }
            set { verifyQRCMVersionVisibilityValue = value; OnPropertyChanged("VerifyQRCMVersionVisibility"); }
        }

        private bool VerifyTestScriptIsSelectedValue = false;
        public bool VerifyTestScriptIsSelected
        {
            get { return VerifyTestScriptIsSelectedValue; }
            set { VerifyTestScriptIsSelectedValue = value; OnPropertyChanged("VerifyTestScriptIsSelected"); }
        }

        private Visibility VerifyTestScriptVisibilityValue = Visibility.Collapsed;
        public Visibility VerifyTestScriptVisibility
        {
            get { return VerifyTestScriptVisibilityValue; }
            set { VerifyTestScriptVisibilityValue = value; OnPropertyChanged("VerifyTestScriptVisibility"); }
        }

        private List<string> Script_checktimeUnitListValue = new List<string> { "Hour", "Min", "Sec" };
        public List<string> Script_checktimeUnitList
        {
            get { return Script_checktimeUnitListValue; }
            set { Script_checktimeUnitListValue = value; OnPropertyChanged("Script_checktimeUnitList"); }
        }

        private string Script_ChecktimeUnitSelectedValue = "Min";
        public string Script_ChecktimeUnitSelected
        {
            get { return Script_ChecktimeUnitSelectedValue; }
            set { Script_ChecktimeUnitSelectedValue = value; OnPropertyChanged("Script_ChecktimeUnitSelected"); }
        }

        private List<string> Script_durationtimeUnitListValue = new List<string> { "Day", "Hour", "Min" };
        public List<string> Script_durationtimeUnitList
        {
            get { return Script_durationtimeUnitListValue; }
            set { Script_durationtimeUnitListValue = value; OnPropertyChanged("Script_durationtimeUnitList"); }
        }

        private string Script_DurationTimeUnitSelectedValue = "Hour";
        public string Script_DurationTimeUnitSelected
        {
            get { return Script_DurationTimeUnitSelectedValue; }
            set { Script_DurationTimeUnitSelectedValue = value; OnPropertyChanged("Script_DurationTimeUnitSelected"); }
        }

        private string Script_checktimeTextboxValue = string.Empty;
        public string Script_checktimeTextbox
        {
            get { return Script_checktimeTextboxValue; }
            set { Script_checktimeTextboxValue = value; OnPropertyChanged("Script_checktimeTextbox"); }
        }

        private string Script_DurationTextboxValue = string.Empty;
        public string Script_DurationTextbox
        {
            get { return Script_DurationTextboxValue; }
            set { Script_DurationTextboxValue = value; OnPropertyChanged("Script_DurationTextbox"); }
        }

        private Visibility ScriptCheckTimeVisibilityvalue = Visibility.Hidden;
        public Visibility ScriptCheckTimeVisibility
        {
            get { return ScriptCheckTimeVisibilityvalue; }
            set { ScriptCheckTimeVisibilityvalue = value; OnPropertyChanged("ScriptCheckTimeVisibility"); }
        }

        private Visibility ScriptExecuteIterationChkbxVisibilityValue = Visibility.Hidden;
        public Visibility ScriptExecuteIterationChkbxVisibility
        {
            get { return ScriptExecuteIterationChkbxVisibilityValue; }
            set { ScriptExecuteIterationChkbxVisibilityValue = value; OnPropertyChanged("ScriptExecuteIterationChkbxVisibility"); }
        }


        private bool scriptExecuteIterationChkbxEnableValue = true;
        public bool ScriptExecuteIterationChkbxEnable
        {
            get { return scriptExecuteIterationChkbxEnableValue; }
            set { scriptExecuteIterationChkbxEnableValue = value; OnPropertyChanged("ScriptExecuteIterationChkbxEnable"); }
        }

        private bool executeIterationChkboxIsCheckedValue = false;
        public bool ExecuteIterationChkboxIsChecked
        {
            get { return executeIterationChkboxIsCheckedValue; }
            set
            {
                executeIterationChkboxIsCheckedValue = value;
                OnPropertyChanged("ExecuteIterationChkboxIsChecked");
                if (value == true)
                {
                    ScriptCheckTimeVisibility = Visibility.Visible;
                    //ScriptCheckTimeIsEnable = false;
                }
                else
                {
                    ScriptCheckTimeVisibility = Visibility.Hidden;
                    //ScriptCheckTimeIsEnable = true;
                }
            }
        }

      
        private string actionDelayUnitSelectedValue = "Min";
        public string ActionDelayUnitSelected
        {
            get { return actionDelayUnitSelectedValue; }
            set { actionDelayUnitSelectedValue = value; OnPropertyChanged("ActionDelayUnitSelected"); }
        }
        private string VerificationdelayTypeValue = "Min";
        public string VerificationdelayType
        {
            get { return VerificationdelayTypeValue; }
            set { VerificationdelayTypeValue = value; OnPropertyChanged("VerificationdelayType"); }
        }

        private string RerundelayTypeValue = "Min";
        public string RerundelayType
        {
            get { return RerundelayTypeValue; }
            set { RerundelayTypeValue = value; OnPropertyChanged("RerundelayType"); }
        }


        private string actionDelaySettingValue = null;
        public string ActionDelaySetting
        {
            get { return actionDelaySettingValue; }
            set { actionDelaySettingValue = value; OnPropertyChanged("ActionDelaySetting"); }
        }

        private string actionErrorHandlingTypeSelectedValue = "Continue Testing";
        public string ActionErrorHandlingTypeSelected
        {
            get { return actionErrorHandlingTypeSelectedValue; }
            set
            {
                actionErrorHandlingTypeSelectedValue = value;
                if (value == "Continue Testing")
                    ActionErrorHandlingReRunIsEnabled = true;
                else
                    ActionErrorHandlingReRunIsEnabled = false;

                OnPropertyChanged("ActionErrorHandlingTypeSelected");
            }
        }

        private ObservableCollection<string> actionErrorHandlingTypeTypeListValue = new ObservableCollection<string> { "Pause at error state", "Continue Testing" };
        public ObservableCollection<string> ActionErrorHandlingTypeTypeList
        {
            get { return actionErrorHandlingTypeTypeListValue; }
            set { actionErrorHandlingTypeTypeListValue = value; OnPropertyChanged("ActionErrorHandlingTypeTypeList"); }
        }

        private bool actionErrorHandlingReRunIsEnabledValue = true;
        public bool ActionErrorHandlingReRunIsEnabled
        {
            get { return actionErrorHandlingReRunIsEnabledValue; }
            set { actionErrorHandlingReRunIsEnabledValue = value; OnPropertyChanged("ActionErrorHandlingReRunIsEnabled"); }
        }

        private string actionErrorHandlingReRunCountValue = "0";
        public string ActionErrorHandlingReRunCount
        {
            get { return actionErrorHandlingReRunCountValue; }
            set { actionErrorHandlingReRunCountValue = value; OnPropertyChanged("ActionErrorHandlingReRunCount"); }
        }

        private Double actionGridMaxHeightValue = 225;
        public Double ActionGridMaxHeight
        {
            get { return actionGridMaxHeightValue; }
            set { actionGridMaxHeightValue = value; isSkipSaveButtonEnable = true; OnPropertyChanged("ActionGridMaxHeight"); isSkipSaveButtonEnable = false; }
        }

        private Double actionGridMinHeightValue = 0;
        public Double ActionGridMinHeight
        {
            get { return actionGridMinHeightValue; }
            set { actionGridMinHeightValue = value; isSkipSaveButtonEnable = true; OnPropertyChanged("ActionGridMinHeight"); isSkipSaveButtonEnable = false; }
        }

        private GridLength actionGridHeightValue = new GridLength(0,GridUnitType.Auto);
        public GridLength ActionGridHeight
        {
            get { return actionGridHeightValue; }
            set { actionGridHeightValue = value; isSkipSaveButtonEnable = true; OnPropertyChanged("ActionGridHeight"); isSkipSaveButtonEnable = false; }
        }

        private Double verifyGridMaxHeightValue = 225;
        public Double VerifyGridMaxHeight
        {
            get { return verifyGridMaxHeightValue; }
            set { verifyGridMaxHeightValue = value; isSkipSaveButtonEnable = true; OnPropertyChanged("VerifyGridMaxHeight"); isSkipSaveButtonEnable = false; }
        }

        private Double verifyGridMinHeightValue = 0;
        public Double VerifyGridMinHeight
        {
            get { return verifyGridMinHeightValue; }
            set { verifyGridMinHeightValue = value; isSkipSaveButtonEnable = true; OnPropertyChanged("VerifyGridMinHeight"); isSkipSaveButtonEnable = false; }
        }

        private GridLength verifyGridHeightValue = new GridLength(0, GridUnitType.Auto);
        public GridLength VerifyGridHeight
        {
            get { return verifyGridHeightValue; }
            set { verifyGridHeightValue = value; isSkipSaveButtonEnable = true; OnPropertyChanged("VerifyGridHeight"); isSkipSaveButtonEnable = false; }
        }

        private Double testActionGridMinHeightValue = 0;
        public Double TestActionGridMinHeight
        {
            get { return testActionGridMinHeightValue; }
            set { testActionGridMinHeightValue = value; isSkipSaveButtonEnable = true; OnPropertyChanged("TestActionGridMinHeight"); isSkipSaveButtonEnable = false; }
        }

        private GridLength testActionGridHeightValue = new GridLength(0, GridUnitType.Pixel);
        public GridLength TestActionGridHeight
        {
            get { return testActionGridHeightValue; }
            set { testActionGridHeightValue = value; isSkipSaveButtonEnable = true; OnPropertyChanged("TestActionGridHeight"); isSkipSaveButtonEnable = false; }
        }

        private Double testVerificationionGridMinHeightValue = 0;
        public Double TestVerificationGridMinHeight
        {
            get { return testVerificationionGridMinHeightValue; }
            set { testVerificationionGridMinHeightValue = value; isSkipSaveButtonEnable = true; OnPropertyChanged("TestVerificationGridMinHeight"); isSkipSaveButtonEnable = false; }
        }

        private GridLength testVerificationGridHeightValue = new GridLength(0, GridUnitType.Pixel);
        public GridLength TestVerificationGridHeight
        {
            get { return testVerificationGridHeightValue; }
            set { testVerificationGridHeightValue = value; isSkipSaveButtonEnable = true; OnPropertyChanged("TestVerificationGridHeight"); isSkipSaveButtonEnable = false; }
        }

        private string previousFindIndexValue = "-1.-1.-1";
        public string PreviousFindIndex
        {
            get { return previousFindIndexValue; }
            set { previousFindIndexValue = value; }
        }

        private string previousFindTextValue = null;
        public string PreviousFindText
        {
            get { return previousFindTextValue; }
            set { previousFindTextValue = value; }
        }
    }

    public class TestControlItem : INotifyPropertyChanged, IDataErrorInfo
    {

        public event PropertyChangedEventHandler PropertyChanged;

      public  bool isSkipSaveButtonEnable = false;
        void OnPropertyChanged(string property)
        {
            try
            {
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs(property));

                    if (ParentTestActionItem != null && ParentTestActionItem.ParentTestCaseItem != null && isSkipSaveButtonEnable == false && ParentTestActionItem.ParentTestCaseItem.SaveButtonIsEnabled != true)
                        ParentTestActionItem.ParentTestCaseItem.SaveButtonIsEnabled = true;

                    isSkipSaveButtonEnable = false;
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

        private TestActionItem parentTestActionItemValue = null;
        public TestActionItem ParentTestActionItem
        {
            get { return parentTestActionItemValue; }
            set { parentTestActionItemValue = value; OnPropertyChanged("ParentTestActionItem"); }
        }

        private ObservableCollection<string> _CustomControlDisableList = new ObservableCollection<string>();
        public ObservableCollection<string> CustomControlDisableList
        {
            get { return _CustomControlDisableList; }
            set { _CustomControlDisableList = value; OnPropertyChanged("CustomControlDisableList"); }
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

                        TestControlPropertyList = null;
                        VerifyTestControlPropertyList = null;

                        ChannelSelectionSelectedItem = null;
                        InputSelectionComboSelectedItem = null;
                        TestControlPropertyInitialValueSelectedItem = null;

                    }
                    else
                    {
                        if (TestControlComponentTypeSelectedItem != null)
                        {
                            string[] alphaNumericSortedComponentName = ParentTestActionItem.ParentTestCaseItem.ComponentNameDictionary[TestControlComponentTypeSelectedItem].ToArray();
                            Array.Sort(alphaNumericSortedComponentName, new AlphanumComparatorFaster());
                            ObservableCollection<string> ComponentNameList = new ObservableCollection<string>(alphaNumericSortedComponentName.ToList());
                            //ObservableCollection<string> ComponentNameList = new ObservableCollection<string>(ParentTestActionItem.ParentTestCaseItem.ComponentNameDictionary[TestControlComponentTypeSelectedItem].OrderBy(a => a));
                            TestControlComponentNameList = ComponentNameList;
                            //TestControlComponentNameList = ParentTestActionItem.ParentTestCaseItem.ComponentNameDictionary[TestControlComponentTypeSelectedItem];//added like this upto ver 1.19

                        }
                        else
                        {
                            TestControlComponentNameList = null;
                        }
                           

                        if ((!string.IsNullOrEmpty(TestControlComponentNameSelectedItem)))
                        {

                            string[] alphaNumericSortedComponentName = ParentTestActionItem.ParentTestCaseItem.ControlNameDictionary[TestControlComponentNameSelectedItem].ToArray();
                            Array.Sort(alphaNumericSortedComponentName, new AlphanumComparatorFaster());
                            //ObservableCollection<string> ComponentcontrolList = new ObservableCollection<string>(ParentTestActionItem.ParentTestCaseItem.ControlNameDictionary[value].OrderBy(a => a));
                            ObservableCollection<string> ComponentcontrolList = new ObservableCollection<string>(alphaNumericSortedComponentName.ToList());
                            TestControlPropertyList = ComponentcontrolList;


                            string[] alphaNumericSortedControlName = ParentTestActionItem.ParentTestCaseItem.VerifyControlNameDictionary[TestControlComponentNameSelectedItem].ToArray();
                            Array.Sort(alphaNumericSortedControlName, new AlphanumComparatorFaster());
                            ObservableCollection<string> ComponentverifycontrolList = new ObservableCollection<string>(alphaNumericSortedControlName.ToList());
                            //ObservableCollection<string> ComponentverifycontrolList = new ObservableCollection<string>(ParentTestActionItem.ParentTestCaseItem.VerifyControlNameDictionary[value].OrderBy(a => a));
                            VerifyTestControlPropertyList = ComponentverifycontrolList;
                        }
                        //else
                        //{
                        //    TestControlPropertyList = null;
                        //    VerifyTestControlPropertyList = null;
                        //}

                        if ((!string.IsNullOrEmpty(TestControlPropertySelectedItem)))
                        {
                            if (ParentTestActionItem.ParentTestCaseItem.channelControlTypeDictionary.Keys.Contains(TestControlComponentNameSelectedItem + TestControlPropertySelectedItem))
                            {
                                //TestControlPropertyInitialValueSelectedItem = null;
                                ChannelEnabled = true;
                                if (ParentTestActionItem.ParentTestCaseItem.ChannelSelectionDictionary.Count > 0)
                                {
                                    if (ParentTestActionItem.ParentTestCaseItem.ChannelSelectionDictionary[TestControlComponentNameSelectedItem + TestControlPropertySelectedItem].Contains(ChannelSelectionSelectedItem))
                                    {
                                        string[] alphaNumericSortedChannelName = ParentTestActionItem.ParentTestCaseItem.ChannelSelectionDictionary[TestControlComponentNameSelectedItem + TestControlPropertySelectedItem].ToArray();
                                        Array.Sort(alphaNumericSortedChannelName, new AlphanumComparatorFaster());
                                        ObservableCollection<string> ComponentchannelList = new ObservableCollection<string>(alphaNumericSortedChannelName.ToList());
                                        //ObservableCollection<string> ComponentchannelList = new ObservableCollection<string>(ParentTestActionItem.ParentTestCaseItem.ChannelSelectionDictionary[value].OrderBy(a => a));
                                        ChannelSelectionList = ComponentchannelList;
                                    }
                                    else
                                    {
                                        ChannelSelectionSelectedItem = string.Empty;
                                        string[] alphaNumericSortedChannelName = ParentTestActionItem.ParentTestCaseItem.ChannelSelectionDictionary[TestControlComponentNameSelectedItem + TestControlPropertySelectedItem].ToArray();
                                        Array.Sort(alphaNumericSortedChannelName, new AlphanumComparatorFaster());
                                        ObservableCollection<string> ComponentchannelList = new ObservableCollection<string>(alphaNumericSortedChannelName.ToList());
                                        //ObservableCollection<string> ComponentchannelList = new ObservableCollection<string>(ParentTestActionItem.ParentTestCaseItem.ChannelSelectionDictionary[value].OrderBy(a => a));
                                        ChannelSelectionList = ComponentchannelList;
                                    }
                                }
                            }
                            if (ParentTestActionItem.ParentTestCaseItem.VerifychannelControlTypeDictionary.Keys.Contains(TestControlComponentNameSelectedItem + TestControlPropertySelectedItem))
                            {
                                //TestControlPropertyInitialValueSelectedItem = null;
                                ChannelEnabled = true;
                                if (ParentTestActionItem.ParentTestCaseItem.ChannelSelectionDictionary.Count > 0)
                                {
                                    if (ParentTestActionItem.ParentTestCaseItem.ChannelSelectionDictionary[TestControlComponentNameSelectedItem + TestControlPropertySelectedItem].Contains(ChannelSelectionSelectedItem))
                                    {
                                        string[] alphaNumericSortedChannelName = ParentTestActionItem.ParentTestCaseItem.ChannelSelectionDictionary[TestControlComponentNameSelectedItem + TestControlPropertySelectedItem].ToArray();
                                        Array.Sort(alphaNumericSortedChannelName, new AlphanumComparatorFaster());
                                        ObservableCollection<string> ComponentchannelList = new ObservableCollection<string>(alphaNumericSortedChannelName.ToList());
                                        //ObservableCollection<string> ComponentchannelList = new ObservableCollection<string>(ParentTestActionItem.ParentTestCaseItem.ChannelSelectionDictionary[value].OrderBy(a => a));
                                        ChannelSelectionList = ComponentchannelList;
                                    }
                                    else
                                    {
                                        ChannelSelectionSelectedItem = string.Empty;
                                        string[] alphaNumericSortedChannelName = ParentTestActionItem.ParentTestCaseItem.ChannelSelectionDictionary[TestControlComponentNameSelectedItem + TestControlPropertySelectedItem].ToArray();
                                        Array.Sort(alphaNumericSortedChannelName, new AlphanumComparatorFaster());
                                        ObservableCollection<string> ComponentchannelList = new ObservableCollection<string>(alphaNumericSortedChannelName.ToList());
                                        //ObservableCollection<string> ComponentchannelList = new ObservableCollection<string>(ParentTestActionItem.ParentTestCaseItem.ChannelSelectionDictionary[value].OrderBy(a => a));
                                        ChannelSelectionList = ComponentchannelList;
                                    }
                                }
                            }
                            if (ParentTestActionItem.ParentTestCaseItem.ControlTypeDictionary.Keys.Contains(TestControlComponentNameSelectedItem + TestControlPropertySelectedItem))
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
                    if (value == testControlComponentTypeSelectedItemValue)
                    {
                        OnPropertyChanged("TestControlComponentTypeSelectedItem");
                        return;
                    }
                    testControlComponentTypeSelectedItemValue = value;

                    if (value != null)
                    {
                        string[] alphaNumericSortedComponentName = ParentTestActionItem.ParentTestCaseItem.ComponentNameDictionary[TestControlComponentTypeSelectedItem].ToArray();
                        Array.Sort(alphaNumericSortedComponentName, new AlphanumComparatorFaster());
                        ObservableCollection<string> ComponentNameList = new ObservableCollection<string>(alphaNumericSortedComponentName.ToList());
                        //ObservableCollection<string> ComponentNameList = new ObservableCollection<string>(ParentTestActionItem.ParentTestCaseItem.ComponentNameDictionary[TestControlComponentTypeSelectedItem].OrderBy(a => a));
                        TestControlComponentNameList = ComponentNameList;
                    }
                    else
                    {
                        TestControlComponentNameList = null;
                    }
                    //TestControlPropertyInitialValueSelectedItem = null;
                    RampCheckVisibility = Visibility.Hidden;
                    RampIsChecked = false;
                    RampSetting = null;
                    OnPropertyChanged("TestControlComponentTypeSelectedItem");
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

                        string[] alphaNumericSortedComponentName = ParentTestActionItem.ParentTestCaseItem.ControlNameDictionary[value].ToArray();
                        Array.Sort(alphaNumericSortedComponentName, new AlphanumComparatorFaster());
                        //ObservableCollection<string> ComponentcontrolList = new ObservableCollection<string>(ParentTestActionItem.ParentTestCaseItem.ControlNameDictionary[value].OrderBy(a => a));
                        ObservableCollection<string> ComponentcontrolList = new ObservableCollection<string>(alphaNumericSortedComponentName.ToList());
                        TestControlPropertyList = ComponentcontrolList;


                        string[] alphaNumericSortedControlName = ParentTestActionItem.ParentTestCaseItem.VerifyControlNameDictionary[value].ToArray();
                        Array.Sort(alphaNumericSortedControlName, new AlphanumComparatorFaster());
                        ObservableCollection<string> ComponentverifycontrolList = new ObservableCollection<string>(alphaNumericSortedControlName.ToList());
                        //ObservableCollection<string> ComponentverifycontrolList = new ObservableCollection<string>(ParentTestActionItem.ParentTestCaseItem.VerifyControlNameDictionary[value].OrderBy(a => a));
                        VerifyTestControlPropertyList = ComponentverifycontrolList;
                      

                    }
                    else
                    {
                        if (TestControlComponentNameList!=null &&TestControlComponentNameList.Count > 0)
                        {
                            ObservableCollection<string> ComponentcontrolList = new ObservableCollection<string>(ParentTestActionItem.ParentTestCaseItem.ControlNameDictionary[TestControlComponentNameList[0]].OrderBy(a => a));
                            TestControlPropertyList = ComponentcontrolList;

                            ObservableCollection<string> ComponentverifycontrolList = new ObservableCollection<string>(ParentTestActionItem.ParentTestCaseItem.VerifyControlNameDictionary[TestControlComponentNameList[0]].OrderBy(a => a));
                            VerifyTestControlPropertyList = ComponentverifycontrolList;
                          
                        }
                    }

                    if ((!TestControlPropertyList.Contains(TestControlPropertySelectedItem))&((!VerifyTestControlPropertyList.Contains(TestControlPropertySelectedItem))))
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
                        if (ParentTestActionItem.ParentTestCaseItem.channelControlTypeDictionary.Keys.Contains(TestControlComponentNameSelectedItem + TestControlPropertySelectedItem))
                        {
                            //TestControlPropertyInitialValueSelectedItem = null;
                            ChannelEnabled = true;
                            if (ParentTestActionItem.ParentTestCaseItem.channelControlTypeDictionary[TestControlComponentNameSelectedItem + TestControlPropertySelectedItem].Contains("control_direction_ramp"))
                            {
                                RampCheckVisibility = Visibility.Visible;
                            }
                            if (ParentTestActionItem.ParentTestCaseItem.ChannelSelectionDictionary.Count > 0)
                            {
                                if (ParentTestActionItem.ParentTestCaseItem.ChannelSelectionDictionary[TestControlComponentNameSelectedItem + TestControlPropertySelectedItem].Contains(ChannelSelectionSelectedItem))
                                {
                                    string[] alphaNumericSortedChannelName = ParentTestActionItem.ParentTestCaseItem.ChannelSelectionDictionary[TestControlComponentNameSelectedItem + TestControlPropertySelectedItem].ToArray();
                                    Array.Sort(alphaNumericSortedChannelName, new AlphanumComparatorFaster());
                                    ObservableCollection<string> ComponentchannelList = new ObservableCollection<string>(alphaNumericSortedChannelName.ToList());
                                    //ObservableCollection<string> ComponentchannelList = new ObservableCollection<string>(ParentTestActionItem.ParentTestCaseItem.ChannelSelectionDictionary[value].OrderBy(a => a));
                                    ChannelSelectionList = ComponentchannelList;
                                }
                                else
                                {
                                    ChannelSelectionSelectedItem = string.Empty;
                                    string[] alphaNumericSortedChannelName = ParentTestActionItem.ParentTestCaseItem.ChannelSelectionDictionary[TestControlComponentNameSelectedItem + TestControlPropertySelectedItem].ToArray();
                                    Array.Sort(alphaNumericSortedChannelName, new AlphanumComparatorFaster());
                                    ObservableCollection<string> ComponentchannelList = new ObservableCollection<string>(alphaNumericSortedChannelName.ToList());
                                    //ObservableCollection<string> ComponentchannelList = new ObservableCollection<string>(ParentTestActionItem.ParentTestCaseItem.ChannelSelectionDictionary[value].OrderBy(a => a));
                                    ChannelSelectionList = ComponentchannelList;
                                }
                            }
                        }
                        if (ParentTestActionItem.ParentTestCaseItem.VerifychannelControlTypeDictionary.Keys.Contains(TestControlComponentNameSelectedItem + TestControlPropertySelectedItem))
                        {
                            //TestControlPropertyInitialValueSelectedItem = null;
                            ChannelEnabled = true;
                            if (ParentTestActionItem.ParentTestCaseItem.ChannelSelectionDictionary.Count > 0)
                            {
                                if (ParentTestActionItem.ParentTestCaseItem.ChannelSelectionDictionary[TestControlComponentNameSelectedItem + TestControlPropertySelectedItem].Contains(ChannelSelectionSelectedItem))
                                {
                                    string[] alphaNumericSortedChannelName = ParentTestActionItem.ParentTestCaseItem.ChannelSelectionDictionary[TestControlComponentNameSelectedItem + TestControlPropertySelectedItem].ToArray();
                                    Array.Sort(alphaNumericSortedChannelName, new AlphanumComparatorFaster());
                                    ObservableCollection<string> ComponentchannelList = new ObservableCollection<string>(alphaNumericSortedChannelName.ToList());
                                    //ObservableCollection<string> ComponentchannelList = new ObservableCollection<string>(ParentTestActionItem.ParentTestCaseItem.ChannelSelectionDictionary[value].OrderBy(a => a));
                                    ChannelSelectionList = ComponentchannelList;
                                }
                                else
                                {
                                    ChannelSelectionSelectedItem = string.Empty;
                                    string[] alphaNumericSortedChannelName = ParentTestActionItem.ParentTestCaseItem.ChannelSelectionDictionary[TestControlComponentNameSelectedItem + TestControlPropertySelectedItem].ToArray();
                                    Array.Sort(alphaNumericSortedChannelName, new AlphanumComparatorFaster());
                                    ObservableCollection<string> ComponentchannelList = new ObservableCollection<string>(alphaNumericSortedChannelName.ToList());
                                    //ObservableCollection<string> ComponentchannelList = new ObservableCollection<string>(ParentTestActionItem.ParentTestCaseItem.ChannelSelectionDictionary[value].OrderBy(a => a));
                                    ChannelSelectionList = ComponentchannelList;
                                }
                            }
                        }
                        if (ParentTestActionItem.ParentTestCaseItem.ControlTypeDictionary.Keys.Contains(TestControlComponentNameSelectedItem + TestControlPropertySelectedItem))
                        {
                            //TestControlPropertyInitialValueSelectedItem = null;
                            InputSelectionEnabled = true;
                            if (ParentTestActionItem.ParentTestCaseItem.ControlTypeDictionary[TestControlComponentNameSelectedItem + TestControlPropertySelectedItem].Contains("control_direction_ramp"))
                            {
                                RampCheckVisibility = Visibility.Visible;
                            }
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
                {
                    TestControlPropertySelectedItem = null;
                    //ChannelSelectionSelectedItem = null;
                    //InputSelectionComboSelectedItem = null;
                    //TestControlPropertyInitialValueSelectedItem = null;
                    //InputSelectionEnabled = false;
                    //ChannelEnabled = false;
                }


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

                        if (string.Equals(ParentTestActionItem.ActionSelected, "Control Action", StringComparison.CurrentCultureIgnoreCase))
                        {
                            if (ParentTestActionItem.ParentTestCaseItem.ControlTypeDictionary.Keys.Contains(TestControlComponentNameSelectedItem + value))
                            {
                                InputSelectionEnabled = true;
                                if (ParentTestActionItem.ParentTestCaseItem.ControlTypeDictionary[TestControlComponentNameSelectedItem + value].Contains("control_direction_ramp"))
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
                            else if (ParentTestActionItem.ParentTestCaseItem.channelControlTypeDictionary.Keys.Contains(TestControlComponentNameSelectedItem + value))
                            {
                                InputSelectionEnabled = false;
                                if (value != string.Empty)
                                {
                                    if (ParentTestActionItem.ParentTestCaseItem.channelControlTypeDictionary[TestControlComponentNameSelectedItem + value].Contains("control_direction_ramp"))
                                    {
                                        RampCheckVisibility = Visibility.Visible;
                                        ChannelEnabled = true;

                                        //added on 21-sep-2016

                                        if (ParentTestActionItem.ParentTestCaseItem.ChannelSelectionDictionary.Count > 0)
                                        {
                                            string[] alphaNumericSortedChannelName = ParentTestActionItem.ParentTestCaseItem.ChannelSelectionDictionary[TestControlComponentNameSelectedItem + value].ToArray();
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
                                        ChannelEnabled = true;
                                    }
                                }
                                else
                                {
                                    ChannelEnabled = false;
                                    RampCheckVisibility = Visibility.Hidden;
                                    RampIsChecked = false;
                                    RampSetting = null;
                                    InputSelectionEnabled = true;
                                }
                            }
                        }

                        if (ParentTestActionItem.ParentTestCaseItem.VerifychannelControlTypeDictionary.Keys.Contains(TestControlComponentNameSelectedItem + value))
                        {
                            if (value != string.Empty)
                            {
                                InputSelectionEnabled = false;
                                ChannelEnabled = true;
                                if (ParentTestActionItem.ParentTestCaseItem.ChannelSelectionDictionary.Count > 0)
                                {
                                    string[] alphaNumericSortedChannelName = ParentTestActionItem.ParentTestCaseItem.ChannelSelectionDictionary[TestControlComponentNameSelectedItem + value].ToArray();
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
                                ChannelEnabled = false;
                                InputSelectionEnabled = true;
                            }
                        }
                        else ///Added on 25-jul-2016
                        {
                            if ((ParentTestActionItem.ParentTestCaseItem.VerifyControlTypeDictionary.Keys.Contains(TestControlComponentNameSelectedItem + value)))//&&(!(ParentTestActionItem.ParentTestCaseItem.ControlTypeDictionary.Keys.Contains(value)))
                            {
                                if (ParentTestActionItem.ParentTestCaseItem.VerifyControlTypeDictionary[TestControlComponentNameSelectedItem + value].Contains("control_direction_external_read"))
                                {
                                    InputSelectionEnabled = true;
                                    ChannelEnabled = false;
                                }
                            }
                        }
                    }

                    OnPropertyChanged("TestControlPropertySelectedItem");
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

                if (LoopIsChecked)
                {
                    if (LoopStart != null && LoopStart != string.Empty)
                    {
                        if (ChannelSelectionList.Count < Convert.ToInt32(LoopStart))
                        {
                            LoopStart = string.Empty;
                        }
                    }

                    if (LoopEnd != null && LoopEnd != string.Empty)
                    {
                        if (ChannelSelectionList.Count < Convert.ToInt32(LoopEnd))
                        {
                            LoopEnd = string.Empty;
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
                }

                OnPropertyChanged("ChannelSelectionList");
            }
        }

        private string ChannelSelectionSelectedItemValue = null;
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

                        if (LoopIsChecked)
                        {
                            if (LoopStart != null && LoopStart != string.Empty)
                            {
                                if (ChannelSelectionList.Count < Convert.ToInt32(LoopStart))
                                {
                                    LoopStart = string.Empty;
                                }
                            }

                            if (LoopEnd != null && LoopEnd != string.Empty)
                            {
                                if (ChannelSelectionList.Count < Convert.ToInt32(LoopEnd))
                                {
                                    LoopEnd = string.Empty;
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
                        }
                    }
                    else
                    {
                        ChannelSelectionSelectedItemValue = null;
                        LoopIsChecked = false;
                        LoopCheckVisibility = Visibility.Hidden;
                    }


                    OnPropertyChanged("ChannelSelectionSelectedItem");
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
                    if (value == InputSelectionComboSelectedItemValue)
                    {
                        OnPropertyChanged("InputSelectionComboSelectedItem");
                        return;
                    }

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

                        if ((!string.IsNullOrEmpty(TestControlPropertySelectedItem)) & (!string.IsNullOrEmpty((selectedPrettyName))))//& ((TestControlPropertySelectedItem.StartsWith("CHANNEL "))|| (TestControlPropertySelectedItem.StartsWith("OUTPUT "))|| (TestControlPropertySelectedItem.StartsWith("INPUT ")) || (TestControlPropertySelectedItem.StartsWith("INPUT_OUTPUT ")) || (TestControlPropertySelectedItem.StartsWith("TAP ")) || (TestControlPropertySelectedItem.StartsWith("BANK_CONTROL ")) || (TestControlPropertySelectedItem.StartsWith("BANK_SELECT ")) || (TestControlPropertySelectedItem.StartsWith("GPIO_INPUT ")) || (TestControlPropertySelectedItem.StartsWith("GPIO_OUTPUT ")))
                        {
							string[] prettyControlName = new string[2];
                            ParentTestActionItem.ParentTestCaseItem.ControlIDDictionary.TryGetValue(TestControlComponentNameSelectedItem + TestControlPropertySelectedItem + selectedPrettyName, out prettyControlName);

                            if (prettyControlName != null)
                            {
                                if (prettyControlName.Count() > 0)
                                    selectedPrettyName = prettyControlName[0];
                                if (prettyControlName.Count() > 1)
                                    selectedControlID = prettyControlName[1];
                            }

                            controlSelected = selectedPrettyName + "~" + removeQATPrefix(TestControlPropertySelectedItem);

                            //if (TestControlPropertySelectedItem.StartsWith("CHANNEL "))
                            //    controlSelected = ChannelSelectionSelectedItem + "~" + removeQATPrefix(TestControlPropertySelectedItem);
                            //else if (TestControlPropertySelectedItem.StartsWith("OUTPUT "))
                            //    controlSelected = ChannelSelectionSelectedItem + "~" + TestControlPropertySelectedItem.Remove(0, 7);
                            //else if (TestControlPropertySelectedItem.StartsWith("INPUT "))
                            //    controlSelected = ChannelSelectionSelectedItem + "~" + TestControlPropertySelectedItem.Remove(0, 6);
                            //else if (TestControlPropertySelectedItem.StartsWith("INPUT_OUTPUT "))
                            //    controlSelected = ChannelSelectionSelectedItem + "~" + TestControlPropertySelectedItem.Remove(0, 13);
                            //else if (TestControlPropertySelectedItem.StartsWith("TAP "))
                            //    controlSelected = ChannelSelectionSelectedItem + "~" + TestControlPropertySelectedItem.Remove(0, 4);
                            //else if (TestControlPropertySelectedItem.StartsWith("BANK_CONTROL_INPUT "))
                            //    controlSelected = ChannelSelectionSelectedItem + "~" + TestControlPropertySelectedItem.Remove(0, 19);
                            //else if (TestControlPropertySelectedItem.StartsWith("BANK_CONTROL_OUTPUT "))
                            //    controlSelected = ChannelSelectionSelectedItem + "~" + TestControlPropertySelectedItem.Remove(0, 20);
                            //else if (TestControlPropertySelectedItem.StartsWith("BANK_CONTROL_INPUT_OUTPUT "))
                            //    controlSelected = ChannelSelectionSelectedItem + "~" + TestControlPropertySelectedItem.Remove(0, 26);
                            //else if (TestControlPropertySelectedItem.StartsWith("BANK_SELECT_INPUT "))
                            //    controlSelected = ChannelSelectionSelectedItem + "~" + TestControlPropertySelectedItem.Remove(0, 18);
                            //else if (TestControlPropertySelectedItem.StartsWith("BANK_SELECT_OUTPUT "))
                            //    controlSelected = ChannelSelectionSelectedItem + "~" + TestControlPropertySelectedItem.Remove(0, 19);
                            //else if (TestControlPropertySelectedItem.StartsWith("GPIO_INPUT "))
                            //    controlSelected = ChannelSelectionSelectedItem + "~" + TestControlPropertySelectedItem.Remove(0, 11);
                            //else if (TestControlPropertySelectedItem.StartsWith("GPIO_OUTPUT "))
                            //    controlSelected = ChannelSelectionSelectedItem + "~" + TestControlPropertySelectedItem.Remove(0, 12);
                        }
                        else
                        {
                            string[] prettyControlName = new string[2];
                            ParentTestActionItem.ParentTestCaseItem.ControlIDDictionary.TryGetValue(TestControlComponentNameSelectedItem + TestControlPropertySelectedItem, out prettyControlName);
                            //selectedPrettyName = prettyControlName[0];

                            if (prettyControlName != null && prettyControlName.Count() > 1)
                                selectedControlID = prettyControlName[1];

                            controlSelected = TestControlPropertySelectedItem;
                        }
						
                        ParentTestActionItem.ParentTestCaseItem.dataTypeDictionary.TryGetValue(TestControlComponentNameSelectedItem + controlSelected + selectedControlID, out valueDataType);

                        ////
                        //ParentTestActionItem.ParentTestCaseItem.dataTypeDictionary.TryGetValue(TestControlComponentNameSelectedItem +TestControlPropertySelectedItem, out valueType);
                        //if ((valueType == string.Empty) | (valueType == null))
                        //    ParentTestActionItem.ParentTestCaseItem.VerifyDataTypeDictionary.TryGetValue(TestControlComponentNameSelectedItem +TestControlPropertySelectedItem, out valueType);

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
                                TestControlPropertyInitialValueSelectedItem = ParentTestActionItem.ParentTestCaseItem.ControlInitialValueDictionary[TestControlComponentNameSelectedItem + TestControlPropertySelectedItem];
                                
                            }
                            else if (!string.IsNullOrEmpty(ChannelSelectionSelectedItem) & TestControlComponentNameSelectedItem != null & TestControlPropertySelectedItem != null)
                            {
								TestControlPropertyInitialValueSelectedItem = ParentTestActionItem.ParentTestCaseItem.ControlInitialValueDictionary[TestControlComponentNameSelectedItem + selectedPrettyName + "~" + removeQATPrefix(TestControlPropertySelectedItem) + selectedControlID];

                                //if (TestControlPropertySelectedItem.StartsWith("CHANNEL "))
                                //    TestControlPropertyInitialValueSelectedItem = ParentTestActionItem.ParentTestCaseItem.ControlInitialValueDictionary[TestControlComponentNameSelectedItem + ChannelSelectionSelectedItem + "~" + removeQATPrefix(TestControlPropertySelectedItem)];
                                //else if (TestControlPropertySelectedItem.StartsWith("OUTPUT "))
                                //    TestControlPropertyInitialValueSelectedItem = ParentTestActionItem.ParentTestCaseItem.ControlInitialValueDictionary[TestControlComponentNameSelectedItem + ChannelSelectionSelectedItem + "~" + TestControlPropertySelectedItem.Remove(0, 7)];
                                //else if (TestControlPropertySelectedItem.StartsWith("INPUT "))
                                //    TestControlPropertyInitialValueSelectedItem = ParentTestActionItem.ParentTestCaseItem.ControlInitialValueDictionary[TestControlComponentNameSelectedItem + ChannelSelectionSelectedItem + "~" + TestControlPropertySelectedItem.Remove(0, 6)];
                                //else if (TestControlPropertySelectedItem.StartsWith("INPUT_OUTPUT "))
                                //    TestControlPropertyInitialValueSelectedItem = ParentTestActionItem.ParentTestCaseItem.ControlInitialValueDictionary[TestControlComponentNameSelectedItem + ChannelSelectionSelectedItem + "~" + TestControlPropertySelectedItem.Remove(0, 13)];
                                //else if (TestControlPropertySelectedItem.StartsWith("TAP "))
                                //    TestControlPropertyInitialValueSelectedItem = ParentTestActionItem.ParentTestCaseItem.ControlInitialValueDictionary[TestControlComponentNameSelectedItem + ChannelSelectionSelectedItem + "~" + TestControlPropertySelectedItem.Remove(0, 4)];
                                //else if (TestControlPropertySelectedItem.StartsWith("BANK_CONTROL_INPUT "))
                                //    TestControlPropertyInitialValueSelectedItem = ParentTestActionItem.ParentTestCaseItem.ControlInitialValueDictionary[TestControlComponentNameSelectedItem + ChannelSelectionSelectedItem + "~" + TestControlPropertySelectedItem.Remove(0, 19)];
                                //else if (TestControlPropertySelectedItem.StartsWith("BANK_CONTROL_OUTPUT "))
                                //    TestControlPropertyInitialValueSelectedItem = ParentTestActionItem.ParentTestCaseItem.ControlInitialValueDictionary[TestControlComponentNameSelectedItem + ChannelSelectionSelectedItem + "~" + TestControlPropertySelectedItem.Remove(0, 20)];
                                //else if (TestControlPropertySelectedItem.StartsWith("BANK_CONTROL_INPUT_OUTPUT "))
                                //    TestControlPropertyInitialValueSelectedItem = ParentTestActionItem.ParentTestCaseItem.ControlInitialValueDictionary[TestControlComponentNameSelectedItem + ChannelSelectionSelectedItem + "~" + TestControlPropertySelectedItem.Remove(0, 26)];
                                //else if (TestControlPropertySelectedItem.StartsWith("BANK_SELECT_INPUT "))
                                //    TestControlPropertyInitialValueSelectedItem = ParentTestActionItem.ParentTestCaseItem.ControlInitialValueDictionary[TestControlComponentNameSelectedItem + ChannelSelectionSelectedItem + "~" + TestControlPropertySelectedItem.Remove(0, 18)];
                                //else if (TestControlPropertySelectedItem.StartsWith("BANK_SELECT_OUTPUT "))
                                //    TestControlPropertyInitialValueSelectedItem = ParentTestActionItem.ParentTestCaseItem.ControlInitialValueDictionary[TestControlComponentNameSelectedItem + ChannelSelectionSelectedItem + "~" + TestControlPropertySelectedItem.Remove(0, 19)];
                                //else if (TestControlPropertySelectedItem.StartsWith("GPIO_INPUT "))
                                //    TestControlPropertyInitialValueSelectedItem = ParentTestActionItem.ParentTestCaseItem.ControlInitialValueDictionary[TestControlComponentNameSelectedItem + ChannelSelectionSelectedItem + "~" + TestControlPropertySelectedItem.Remove(0, 11)];
                                //else if (TestControlPropertySelectedItem.StartsWith("GPIO_OUTPUT "))
                                //    TestControlPropertyInitialValueSelectedItem = ParentTestActionItem.ParentTestCaseItem.ControlInitialValueDictionary[TestControlComponentNameSelectedItem + ChannelSelectionSelectedItem + "~" + TestControlPropertySelectedItem.Remove(0, 12)];
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
                                TestControlPropertyInitialValueSelectedItem = ParentTestActionItem.ParentTestCaseItem.ControlInitialStringDictionary[TestControlComponentNameSelectedItem + TestControlPropertySelectedItem];
                            }
                            else if (!string.IsNullOrEmpty(ChannelSelectionSelectedItem) & TestControlComponentNameSelectedItem != null & TestControlPropertySelectedItem != null)
                            {
								TestControlPropertyInitialValueSelectedItem = ParentTestActionItem.ParentTestCaseItem.ControlInitialStringDictionary[TestControlComponentNameSelectedItem + selectedPrettyName + "~" + removeQATPrefix(TestControlPropertySelectedItem) + selectedControlID];

                                //if (TestControlPropertySelectedItem.StartsWith("CHANNEL "))
                                //    TestControlPropertyInitialValueSelectedItem = ParentTestActionItem.ParentTestCaseItem.ControlInitialStringDictionary[TestControlComponentNameSelectedItem + ChannelSelectionSelectedItem + "~" + removeQATPrefix(TestControlPropertySelectedItem)];
                                //else if (TestControlPropertySelectedItem.StartsWith("OUTPUT "))
                                //    TestControlPropertyInitialValueSelectedItem = ParentTestActionItem.ParentTestCaseItem.ControlInitialStringDictionary[TestControlComponentNameSelectedItem + ChannelSelectionSelectedItem + "~" + TestControlPropertySelectedItem.Remove(0, 7)];
                                //else if (TestControlPropertySelectedItem.StartsWith("INPUT "))
                                //    TestControlPropertyInitialValueSelectedItem = ParentTestActionItem.ParentTestCaseItem.ControlInitialStringDictionary[TestControlComponentNameSelectedItem + ChannelSelectionSelectedItem + "~" + TestControlPropertySelectedItem.Remove(0, 6)];
                                //else if (TestControlPropertySelectedItem.StartsWith("INPUT_OUTPUT "))
                                //    TestControlPropertyInitialValueSelectedItem = ParentTestActionItem.ParentTestCaseItem.ControlInitialStringDictionary[TestControlComponentNameSelectedItem + ChannelSelectionSelectedItem + "~" + TestControlPropertySelectedItem.Remove(0, 13)];
                                //else if (TestControlPropertySelectedItem.StartsWith("TAP "))
                                //    TestControlPropertyInitialValueSelectedItem = ParentTestActionItem.ParentTestCaseItem.ControlInitialStringDictionary[TestControlComponentNameSelectedItem + ChannelSelectionSelectedItem + "~" + TestControlPropertySelectedItem.Remove(0, 4)];
                                //else if (TestControlPropertySelectedItem.StartsWith("BANK_CONTROL_INPUT "))
                                //    TestControlPropertyInitialValueSelectedItem = ParentTestActionItem.ParentTestCaseItem.ControlInitialStringDictionary[TestControlComponentNameSelectedItem + ChannelSelectionSelectedItem + "~" + TestControlPropertySelectedItem.Remove(0, 19)];
                                //else if (TestControlPropertySelectedItem.StartsWith("BANK_CONTROL_OUTPUT "))
                                //    TestControlPropertyInitialValueSelectedItem = ParentTestActionItem.ParentTestCaseItem.ControlInitialStringDictionary[TestControlComponentNameSelectedItem + ChannelSelectionSelectedItem + "~" + TestControlPropertySelectedItem.Remove(0, 20)];
                                //else if (TestControlPropertySelectedItem.StartsWith("BANK_CONTROL_INPUT_OUTPUT "))
                                //    TestControlPropertyInitialValueSelectedItem = ParentTestActionItem.ParentTestCaseItem.ControlInitialStringDictionary[TestControlComponentNameSelectedItem + ChannelSelectionSelectedItem + "~" + TestControlPropertySelectedItem.Remove(0, 26)];
                                //else if (TestControlPropertySelectedItem.StartsWith("BANK_SELECT_INPUT "))
                                //    TestControlPropertyInitialValueSelectedItem = ParentTestActionItem.ParentTestCaseItem.ControlInitialStringDictionary[TestControlComponentNameSelectedItem + ChannelSelectionSelectedItem + "~" + TestControlPropertySelectedItem.Remove(0, 18)];
                                //else if (TestControlPropertySelectedItem.StartsWith("BANK_SELECT_OUTPUT "))
                                //    TestControlPropertyInitialValueSelectedItem = ParentTestActionItem.ParentTestCaseItem.ControlInitialStringDictionary[TestControlComponentNameSelectedItem + ChannelSelectionSelectedItem + "~" + TestControlPropertySelectedItem.Remove(0, 19)];
                                //else if (TestControlPropertySelectedItem.StartsWith("GPIO_INPUT "))
                                //    TestControlPropertyInitialValueSelectedItem = ParentTestActionItem.ParentTestCaseItem.ControlInitialStringDictionary[TestControlComponentNameSelectedItem + ChannelSelectionSelectedItem + "~" + TestControlPropertySelectedItem.Remove(0, 11)];
                                //else if (TestControlPropertySelectedItem.StartsWith("GPIO_OUTPUT "))
                                //    TestControlPropertyInitialValueSelectedItem = ParentTestActionItem.ParentTestCaseItem.ControlInitialStringDictionary[TestControlComponentNameSelectedItem + ChannelSelectionSelectedItem + "~" + TestControlPropertySelectedItem.Remove(0, 12)];
                            }
                        }
                    else if (String.Equals("Set by position", InputSelectionComboSelectedItem, StringComparison.InvariantCultureIgnoreCase))
                    {
                            MaxLimitIsEnabled = true;
                            MinLimitIsEnabled = true;
                            valueMaxLength = 20;
                            //MaximumLimit = string.Empty;
                            //MinimumLimit = string.Empty;
                            if (ChannelSelectionSelectedItem == null & TestControlComponentNameSelectedItem != null & TestControlPropertySelectedItem != null)
                        {
                            TestControlPropertyInitialValueSelectedItem = ParentTestActionItem.ParentTestCaseItem.ControlInitialPositionDictionary[TestControlComponentNameSelectedItem + TestControlPropertySelectedItem];
                        }
                        else if (!string.IsNullOrEmpty(ChannelSelectionSelectedItem) & TestControlComponentNameSelectedItem != null & TestControlPropertySelectedItem != null)
                        {
								TestControlPropertyInitialValueSelectedItem = ParentTestActionItem.ParentTestCaseItem.ControlInitialPositionDictionary[TestControlComponentNameSelectedItem + selectedPrettyName + "~" + removeQATPrefix(TestControlPropertySelectedItem) + selectedControlID];

                                //if (TestControlPropertySelectedItem.StartsWith("CHANNEL "))
                                //    TestControlPropertyInitialValueSelectedItem = ParentTestActionItem.ParentTestCaseItem.ControlInitialPositionDictionary[TestControlComponentNameSelectedItem + ChannelSelectionSelectedItem + "~" + removeQATPrefix(TestControlPropertySelectedItem)];
                                //else if (TestControlPropertySelectedItem.StartsWith("OUTPUT "))
                                //    TestControlPropertyInitialValueSelectedItem = ParentTestActionItem.ParentTestCaseItem.ControlInitialPositionDictionary[TestControlComponentNameSelectedItem + ChannelSelectionSelectedItem + "~" + TestControlPropertySelectedItem.Remove(0, 7)];
                                //else if (TestControlPropertySelectedItem.StartsWith("INPUT "))
                                //    TestControlPropertyInitialValueSelectedItem = ParentTestActionItem.ParentTestCaseItem.ControlInitialPositionDictionary[TestControlComponentNameSelectedItem + ChannelSelectionSelectedItem + "~" + TestControlPropertySelectedItem.Remove(0, 6)];
                                //else if (TestControlPropertySelectedItem.StartsWith("INPUT_OUTPUT "))
                                //    TestControlPropertyInitialValueSelectedItem = ParentTestActionItem.ParentTestCaseItem.ControlInitialPositionDictionary[TestControlComponentNameSelectedItem + ChannelSelectionSelectedItem + "~" + TestControlPropertySelectedItem.Remove(0, 13)];
                                //else if (TestControlPropertySelectedItem.StartsWith("TAP "))
                                //    TestControlPropertyInitialValueSelectedItem = ParentTestActionItem.ParentTestCaseItem.ControlInitialPositionDictionary[TestControlComponentNameSelectedItem + ChannelSelectionSelectedItem + "~" + TestControlPropertySelectedItem.Remove(0, 4)];
                                //else if (TestControlPropertySelectedItem.StartsWith("BANK_CONTROL_INPUT "))
                                //    TestControlPropertyInitialValueSelectedItem = ParentTestActionItem.ParentTestCaseItem.ControlInitialPositionDictionary[TestControlComponentNameSelectedItem + ChannelSelectionSelectedItem + "~" + TestControlPropertySelectedItem.Remove(0, 19)];
                                //else if (TestControlPropertySelectedItem.StartsWith("BANK_CONTROL_OUTPUT "))
                                //    TestControlPropertyInitialValueSelectedItem = ParentTestActionItem.ParentTestCaseItem.ControlInitialPositionDictionary[TestControlComponentNameSelectedItem + ChannelSelectionSelectedItem + "~" + TestControlPropertySelectedItem.Remove(0, 20)];
                                //else if (TestControlPropertySelectedItem.StartsWith("BANK_CONTROL_INPUT_OUTPUT "))
                                //    TestControlPropertyInitialValueSelectedItem = ParentTestActionItem.ParentTestCaseItem.ControlInitialPositionDictionary[TestControlComponentNameSelectedItem + ChannelSelectionSelectedItem + "~" + TestControlPropertySelectedItem.Remove(0, 26)];
                                //else if (TestControlPropertySelectedItem.StartsWith("BANK_SELECT_INPUT "))
                                //    TestControlPropertyInitialValueSelectedItem = ParentTestActionItem.ParentTestCaseItem.ControlInitialPositionDictionary[TestControlComponentNameSelectedItem + ChannelSelectionSelectedItem + "~" + TestControlPropertySelectedItem.Remove(0, 18)];
                                //else if (TestControlPropertySelectedItem.StartsWith("BANK_SELECT_OUTPUT "))
                                //    TestControlPropertyInitialValueSelectedItem = ParentTestActionItem.ParentTestCaseItem.ControlInitialPositionDictionary[TestControlComponentNameSelectedItem + ChannelSelectionSelectedItem + "~" + TestControlPropertySelectedItem.Remove(0, 19)];
                                //else if (TestControlPropertySelectedItem.StartsWith("GPIO_INPUT "))
                                //    TestControlPropertyInitialValueSelectedItem = ParentTestActionItem.ParentTestCaseItem.ControlInitialPositionDictionary[TestControlComponentNameSelectedItem + ChannelSelectionSelectedItem + "~" + TestControlPropertySelectedItem.Remove(0, 11)];
                                //else if (TestControlPropertySelectedItem.StartsWith("GPIO_OUTPUT "))
                                //    TestControlPropertyInitialValueSelectedItem = ParentTestActionItem.ParentTestCaseItem.ControlInitialPositionDictionary[TestControlComponentNameSelectedItem + ChannelSelectionSelectedItem + "~" + TestControlPropertySelectedItem.Remove(0, 12)];
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
                    // MessageBox.Show("Exception\n " + ex.Message, "Error Code - ECxxxxx", MessageBoxButton.OK, MessageBoxImage.Error);
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
                if (value == testControlPropertyInitialValueSelectedItemValue)
                {
                    OnPropertyChanged("TestControlPropertyInitialValueSelectedItem");
                    return;
                }
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

        private int valueMaxLengthValue =20;
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
                isSkipSaveButtonEnable = true;
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
                                ParentTestActionItem.ParentTestCaseItem.ControlIDDictionary.TryGetValue(TestControlComponentNameSelectedItem + TestControlPropertySelectedItem + selectedPrettyName, out prettyControlName);

                                if (prettyControlName != null)
                                {
                                    if(prettyControlName.Count() > 0)
                                        selectedPrettyName = prettyControlName[0];

                                    if (prettyControlName.Count() > 1)
                                        selectedControlID = prettyControlName[1];
                                }

                                controlSelected = selectedPrettyName + "~" + removeQATPrefix(TestControlPropertySelectedItem);

                                //if (TestControlPropertySelectedItem.StartsWith("CHANNEL "))
                                //    controlSelected = ChannelSelectionSelectedItem + "~" + removeQATPrefix(TestControlPropertySelectedItem);
                                //else if (TestControlPropertySelectedItem.StartsWith("OUTPUT "))
                                //    controlSelected = ChannelSelectionSelectedItem + "~" + TestControlPropertySelectedItem.Remove(0, 7);
                                //else if (TestControlPropertySelectedItem.StartsWith("INPUT "))
                                //    controlSelected = ChannelSelectionSelectedItem + "~" + TestControlPropertySelectedItem.Remove(0, 6);
                                //else if (TestControlPropertySelectedItem.StartsWith("INPUT_OUTPUT "))
                                //    controlSelected = ChannelSelectionSelectedItem + "~" + TestControlPropertySelectedItem.Remove(0, 13);
                                //else if (TestControlPropertySelectedItem.StartsWith("TAP "))
                                //    controlSelected = ChannelSelectionSelectedItem + "~" + TestControlPropertySelectedItem.Remove(0, 4);
                                //else if (TestControlPropertySelectedItem.StartsWith("BANK_CONTROL_INPUT "))
                                //    controlSelected = ChannelSelectionSelectedItem + "~" + TestControlPropertySelectedItem.Remove(0, 19);
                                //else if (TestControlPropertySelectedItem.StartsWith("BANK_CONTROL_OUTPUT "))
                                //    controlSelected = ChannelSelectionSelectedItem + "~" + TestControlPropertySelectedItem.Remove(0, 20);
                                //else if (TestControlPropertySelectedItem.StartsWith("BANK_CONTROL_INPUT_OUTPUT "))
                                //    controlSelected = ChannelSelectionSelectedItem + "~" + TestControlPropertySelectedItem.Remove(0, 26);
                                //else if (TestControlPropertySelectedItem.StartsWith("BANK_SELECT_INPUT "))
                                //    controlSelected = ChannelSelectionSelectedItem + "~" + TestControlPropertySelectedItem.Remove(0, 18);
                                //else if (TestControlPropertySelectedItem.StartsWith("BANK_SELECT_OUTPUT "))
                                //    controlSelected = ChannelSelectionSelectedItem + "~" + TestControlPropertySelectedItem.Remove(0, 19);
                                //else if (TestControlPropertySelectedItem.StartsWith("GPIO_INPUT "))
                                //    controlSelected = ChannelSelectionSelectedItem + "~" + TestControlPropertySelectedItem.Remove(0, 11);
                                //else if (TestControlPropertySelectedItem.StartsWith("GPIO_OUTPUT "))
                                //    controlSelected = ChannelSelectionSelectedItem + "~" + TestControlPropertySelectedItem.Remove(0, 12);
                            }
                            else
                            {
                                string[] prettyControlName = null;

                                ParentTestActionItem.ParentTestCaseItem.ControlIDDictionary.TryGetValue(TestControlComponentNameSelectedItem + TestControlPropertySelectedItem, out prettyControlName);
                                //selectedPrettyName = prettyControlName[0];

                                if(prettyControlName != null && prettyControlName.Count() > 1)
                                    selectedControlID = prettyControlName[1];

                                controlSelected = TestControlPropertySelectedItem;
                            }

                            string valueTypeSelected = InputSelectionComboSelectedItem;

                            string maxValue = string.Empty;
                            ParentTestActionItem.ParentTestCaseItem.MaximumControlValueDictionary.TryGetValue(TestControlComponentNameSelectedItem + controlSelected + selectedControlID, out maxValue);
                            
                            string minValue = string.Empty;
                            ParentTestActionItem.ParentTestCaseItem.MinimumControlValueDictionary.TryGetValue(TestControlComponentNameSelectedItem + controlSelected + selectedControlID, out minValue);
                            
                            string valueDataType = string.Empty;
                            ParentTestActionItem.ParentTestCaseItem.dataTypeDictionary.TryGetValue(TestControlComponentNameSelectedItem + controlSelected + selectedControlID, out valueDataType);

                            if ((valueDataType == string.Empty) | (valueDataType == null))
                            {
                                ParentTestActionItem.ParentTestCaseItem.VerifyDataTypeDictionary.TryGetValue(TestControlComponentNameSelectedItem + controlSelected + selectedControlID, out valueDataType);
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
                                    if (TestControlPropertyInitialValueSelectedItem.Length==1)
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
                                        //TestControlPropertyInitialValueSelectedItem=TestControlPropertyInitialValueSelectedItem.Remove(1,1);//commented on 04-Aug-2016
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
                                            //return "" + valueDataType + " Datatype:Enter '1' for action and '0' for verification";
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
                                        //return "" + valueDataType + " Datatype:Enter '1' for action and '0' for verification";
                                    }
                                }
                                else if ((String.Equals("set by value", valueTypeSelected, StringComparison.CurrentCultureIgnoreCase)) & (String.Equals("Text", valueDataType, StringComparison.InvariantCultureIgnoreCase)))
                                {
                                    DataTypeTextBlock =valueDataType + " Datatype:Please enter Text";
                                    return "";
                                }
                                else if ((String.Equals("set by value", valueTypeSelected, StringComparison.CurrentCultureIgnoreCase)) & (String.Equals("Unknown", valueDataType, StringComparison.InvariantCultureIgnoreCase)))
                                {
                                    DataTypeTextBlock =valueDataType + " Datatype";
                                    return "";
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
                                else if (!double.TryParse(textBoxValue,out _textValue))
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
                                    if ((String.Equals("Trigger", valueDataType, StringComparison.InvariantCultureIgnoreCase)))
                                        return "" + valueDataType + " Datatype:Value should be either '" + minValue + "' or '" + maxValue + "'";
                                    //return "" + valueDataType + " Datatype:Enter '1' for action and '0' for verification";
                                    else
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
                                //return "" + valueDataType + " Datatype:Enter '1' for action and '0' for verification";
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
                                if(InputSelectionComboSelectedItem!="Set by value")
                                    DataTypeTextBlock = string.Empty;
                                    return "Please enter Value";                               
                            }
                                else
                                    return "";                          

                        }
                    }
                    else if("MaximumLimit"== columnName)
                    {                       
                        if(MaximumLimit.Length==1)
                        {
                            string textBoxValue = MaximumLimit;
                            if ((textBoxValue == "-") || (textBoxValue == "+") || (textBoxValue == "."))
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
                                    if(channelCount <= 0)
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
                                    if ((_textloopincr > 0) && (_textloopincr <= channelCount) &&(_textloopincr <= _textloopend))
                                    {
                                        return "";
                                    }
                                    else if (_textloopincr == 0)
                                    {
                                        LoopIncrement = string.Empty;
                                        return "";//return "Enter the value greater than 0";
                                    }
                                    else if ((_textloopincr > channelCount)||(_textloopincr >= _textloopend))
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
                // MessageBox.Show("Exception\n " + ex.Message, "QAT Error Code - EC12019", MessageBoxButton.OK, MessageBoxImage.Error);
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
                // MessageBox.Show("Exception\n " + ex.Message, "QAT Error Code - EC12019", MessageBoxButton.OK, MessageBoxImage.Error);
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
                // MessageBox.Show("Exception\n " + ex.Message, "QAT Error Code - EC12019", MessageBoxButton.OK, MessageBoxImage.Error);
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
            bool stat= regex.IsMatch(text);
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

    public class ComboboxDisableConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if (values[2].ToString() == "Custom Controls" && !values[1].ToString().Contains("{DisconnectedItem}"))
                {
                    if (values != null && parameter.ToString() == "ComponentName")
                    {
                        string[] arr = ((IEnumerable)values[0]).Cast<object>()
                                             .Select(x => x.ToString())
                                             .ToArray();

                        var ch = arr.Where(x => x.Equals(values[1])).ToArray();
                        if (ch != null && ch.Count() > 0)
                        {
                            return true;
                        }
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                return Binding.DoNothing;
            }
        }
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return new[] { value, DependencyProperty.UnsetValue };
        }
    }

    public class AlphanumComparatorFaster : IComparer
    {
        public int Compare(object x, object y)
        {
            try
            {
                //TreeViewExplorer one = x as TreeViewExplorer;
                //TreeViewExplorer two = y as TreeViewExplorer;
                string s1 = x as string;
                if (s1 == null)
                {
                    return 0;
                }
                string s2 = y as string;
                if (s2 == null)
                {
                    return 0;
                }

                int len1 = s1.Length;
                int len2 = s2.Length;
                int marker1 = 0;
                int marker2 = 0;

                // Walk through two the strings with two markers.
                while (marker1 < len1 && marker2 < len2)
                {
                    char ch1 = s1[marker1];
                    char ch2 = s2[marker2];

                    // Some buffers we can build up characters in for each chunk.
                    char[] space1 = new char[len1];
                    int loc1 = 0;
                    char[] space2 = new char[len2];
                    int loc2 = 0;

                    // Walk through all following characters that are digits or
                    // characters in BOTH strings starting at the appropriate marker.
                    // Collect char arrays.
                    do
                    {
                        space1[loc1++] = ch1;
                        marker1++;

                        if (marker1 < len1)
                        {
                            ch1 = s1[marker1];
                        }
                        else
                        {
                            break;
                        }
                    } while (char.IsDigit(ch1) == char.IsDigit(space1[0]));

                    do
                    {
                        space2[loc2++] = ch2;
                        marker2++;

                        if (marker2 < len2)
                        {
                            ch2 = s2[marker2];
                        }
                        else
                        {
                            break;
                        }
                    } while (char.IsDigit(ch2) == char.IsDigit(space2[0]));

                    // If we have collected numbers, compare them numerically.
                    // Otherwise, if we have strings, compare them alphabetically.
                    string str1 = new string(space1);
                    string str2 = new string(space2);

                    int result;

                    if (char.IsDigit(space1[0]) && char.IsDigit(space2[0]))
                    {
                        int thisNumericChunk = int.Parse(str1);
                        int thatNumericChunk = int.Parse(str2);
                        result = thisNumericChunk.CompareTo(thatNumericChunk);
                    }
                    else
                    {
                        result = str1.CompareTo(str2);
                    }

                    if (result != 0)
                    {
                        return result;
                    }
                }
                return len1 - len2;
            }
            catch (Exception ex)
            {


                MessageBox.Show("Exception\n \n" + ex.Message, "QAT Error Code - EC14017F", MessageBoxButton.OK, MessageBoxImage.Error);
                return 0;

            }

        }
    }


    public class TestActionQRCMItem : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        void OnPropertyChanged(string property)
        {
            try
            {
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs(property));

                    if (ParentTestActionItem != null && ParentTestActionItem.ParentTestCaseItem != null && ParentTestActionItem.ParentTestCaseItem.SaveButtonIsEnabled != true)
                        ParentTestActionItem.ParentTestCaseItem.SaveButtonIsEnabled = true;
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

        private TestActionItem parentTestActionItemValue = null;
        public TestActionItem ParentTestActionItem
        {
            get { return parentTestActionItemValue; }
            set { parentTestActionItemValue = value; OnPropertyChanged("ParentTestActionItem"); }
        }


        private ObservableCollection<QRCMInitialValues> _QRCM_MethodsInitialValues = new ObservableCollection<QRCMInitialValues>();
        public ObservableCollection<QRCMInitialValues> QRCM_MethodsInitialValues
        {
            get
            {
                return _QRCM_MethodsInitialValues;
            }
            set
            {
                _QRCM_MethodsInitialValues = value;
                OnPropertyChanged("QRCM_MethodsInitialValues");
            }
        }


        private ObservableCollection<string> actionQRCM_MethodsListValue = new ObservableCollection<string>();
        public ObservableCollection<string> ActionQRCM_MethodsList
        {
            get
            {
                return actionQRCM_MethodsListValue;
            }
            set
            {
                actionQRCM_MethodsListValue = value;
                OnPropertyChanged("ActionQRCM_MethodsList");           
            }      
        }

        private string methods_QRCMSelectedItemValue = null;
        public string QRCM_MethodsSelectedItem
        {
            get
            {
                return methods_QRCMSelectedItemValue;
            }
            set
            {
                ArgumentsTextboxIsEnabled = false;
                SetPayloadBtnIsEnabled = false;
                if (value != null)
                {
                    ActionUserArguments = string.Empty;
                    QRCMInitialValues initialValuesitem = QRCM_MethodsInitialValues.Where(x => x.MethodNameUserView == value).First();
                    if (initialValuesitem.Input_arguments_Tooltip != null && initialValuesitem.Input_arguments_Tooltip.Trim() != string.Empty)
                    {
                        ArgumentsTextboxIsEnabled = true;
                        ArgumentTextBoxTooltip = initialValuesitem.Input_arguments_Tooltip.Trim();
                    }
                 
                    if (initialValuesitem.IsPayloadAvailable && initialValuesitem.Api_Payload != null && initialValuesitem.Api_Payload != string.Empty)
                    {
                        SetPayloadBtnIsEnabled = true;
                        SetPayloadContent = initialValuesitem.Api_Payload;
                    }                                      

                    if (initialValuesitem.Reference_key != null && initialValuesitem.Reference_key != string.Empty)
                        ReferenceKey = initialValuesitem.Reference_key;

                    if (initialValuesitem.Payload_key != null && initialValuesitem.Payload_key != string.Empty)
                        PayloadKey = initialValuesitem.Payload_key;                  
                }              

                methods_QRCMSelectedItemValue = value;
                OnPropertyChanged("QRCM_MethodsSelectedItem");
            }
        }

        private ObservableCollection<string> actionQRCM_DevicesListValue = new ObservableCollection<string>();
        public ObservableCollection<string> ActionQRCM_DevicesList
        {
            get
            {
                return actionQRCM_DevicesListValue;
            }
            set
            {
                actionQRCM_DevicesListValue = value;
                OnPropertyChanged("ActionQRCM_DevicesList");
            }
        }

        private string deviceQRCMSelectedItemValue = null;
        public string QRCM_DeviceSelectedItem
        {
            get
            {
                return deviceQRCMSelectedItemValue;
            }
            set
            {
                deviceQRCMSelectedItemValue = value;

                if (value != null && value != string.Empty)
                    MethodNameComboboxIsEnabled = true;
                else
                    MethodNameComboboxIsEnabled = false;

                OnPropertyChanged("QRCM_DeviceSelectedItem");
            }
        }

        private Dictionary<string, string> _QRCM_DeviceModelValue =new Dictionary<string, string>();
        public Dictionary<string, string> QRCM_DeviceModel
        {
            get
            {
                return _QRCM_DeviceModelValue;
            }
            set
            {
                _QRCM_DeviceModelValue = value;
                OnPropertyChanged("QRCM_DeviceModel");
            }
        }

        private bool methodNameComboboxIsEnabledValue = false;
        public bool MethodNameComboboxIsEnabled
        {
            get { return methodNameComboboxIsEnabledValue; }
            set { methodNameComboboxIsEnabledValue = value; OnPropertyChanged("MethodNameComboboxIsEnabled"); }
        }

        private string actionUserArgumentsValue = string.Empty;
        public string ActionUserArguments
        {
            get
            {
                return actionUserArgumentsValue;
            }
            set
            {
                actionUserArgumentsValue = value;
                OnPropertyChanged("ActionUserArguments");
            }
        }

        private string argumentTextBoxTooltipValue = "Enter user input arguments";
        public string ArgumentTextBoxTooltip
        {
            get { return argumentTextBoxTooltipValue; }
            set
            {
                argumentTextBoxTooltipValue = value;
                OnPropertyChanged("ArgumentTextBoxTooltip");
            }
        }

        private string argumentTextBoxWaterMarkValue = "Enter user input arguments";
        public string ArgumentTextBoxWaterMark
        {
            get { return argumentTextBoxWaterMarkValue; }
            set
            {
                argumentTextBoxWaterMarkValue = value;
                OnPropertyChanged("ArgumentTextBoxWaterMark");
            }
        }

        private bool argumentsTextboxIsEnabledValue = false;
        public bool ArgumentsTextboxIsEnabled
        {
            get { return argumentsTextboxIsEnabledValue; }
            set { argumentsTextboxIsEnabledValue = value; OnPropertyChanged("ArgumentsTextboxIsEnabled"); }
        }

        private bool setPayloadBtnIsEnabledValue = false;
        public bool SetPayloadBtnIsEnabled
        {
            get { return setPayloadBtnIsEnabledValue; }
            set { setPayloadBtnIsEnabledValue = value; OnPropertyChanged("SetPayloadBtnIsEnabled"); }
        }

        private string setPayloadContentValue = string.Empty;
        public string SetPayloadContent
        {
            get { return setPayloadContentValue; }
            set { setPayloadContentValue = value; OnPropertyChanged("SetPayloadContent"); }
        }

        private string referenceKeyValue = string.Empty;
        public string ReferenceKey
        {
            get { return referenceKeyValue; }
            set { referenceKeyValue = value; OnPropertyChanged("ReferenceKey"); }
        }

        private string payloadKeyValue = string.Empty;
        public string PayloadKey
        {
            get { return payloadKeyValue; }
            set { payloadKeyValue = value; OnPropertyChanged("PayloadKey"); }
        }
    }

    public class QRCMInitialValues
    {
        public string ProjectName { get; set; }
        public string Buildversion { get; set; }
        public string ReferenceVersion { get; set; }
        public string MethodNameUserView { get; set; }
        public string Actual_method_name { get; set; }
        public string ApiReference { get; set; }
        public string Input_arguments_Tooltip { get; set; }
        public bool   HasPreMethod { get; set; }
        public string PreMethodName { get; set; }
        public string PreMethodUserKey { get; set; }
        public string PreMethodActualKey { get; set; }
        public int    ArgumentMappingIndex { get; set; }
        public string CoreModel { get; set; }
        public bool   IsActionTrue { get; set; }
        public string TabGroupName { get; set; }
        public string Api_Payload { get; set; }
        public bool   IsPayloadAvailable { get; set; }  
        public string Reference_key { get; set; }
        public string Payload_key { get; set; }
        public bool Merge_data { get; set; }        
    }


    public class TestVerifyQRCMItem : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        void OnPropertyChanged(string property)
        {
            try
            {
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs(property));

                    if (ParentTestActionItem != null && ParentTestActionItem.ParentTestCaseItem != null && ParentTestActionItem.ParentTestCaseItem.SaveButtonIsEnabled != true)
                        ParentTestActionItem.ParentTestCaseItem.SaveButtonIsEnabled = true;
                }
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                MessageBox.Show("Exception\n " + ex.Message, "Error Code - ECxxxxx", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private TestActionItem parentTestActionItemValue = null;
        public TestActionItem ParentTestActionItem
        {
            get { return parentTestActionItemValue; }
            set { parentTestActionItemValue = value; OnPropertyChanged("ParentTestActionItem"); }
        }

        private ObservableCollection<QRCMInitialValues> _QRCM_MethodsInitialValues = new ObservableCollection<QRCMInitialValues>();
        public ObservableCollection<QRCMInitialValues> QRCM_MethodsInitialValues
        {
            get
            {
                return _QRCM_MethodsInitialValues;
            }
            set
            {
                _QRCM_MethodsInitialValues = value;
                OnPropertyChanged("QRCM_MethodsInitialValues");
            }
        }

        private ObservableCollection<string> verifyQRCM_MethodsListValue = new ObservableCollection<string>();
        public ObservableCollection<string> VerifyQRCM_MethodsList
        {
            get
            {
                return verifyQRCM_MethodsListValue;
            }
            set
            {
                verifyQRCM_MethodsListValue = value;
                OnPropertyChanged("VerifyQRCM_MethodsList");
            }
        }

        private string methods_QRCMSelectedItemValue = null;
        public string QRCM_MethodsSelectedItem
        {
            get
            {
                return methods_QRCMSelectedItemValue;
            }
            set
            {
			     ArgumentsTextboxIsEnabled = false;
                 SetReferenceBtnIsEnabled = false;
				 
                if (value != null)
                {
                    VerifyUserArguments = string.Empty;
                    QRCMInitialValues initialValuesitem = QRCM_MethodsInitialValues.Where(x => x.MethodNameUserView == value).First();
                    if (initialValuesitem.Input_arguments_Tooltip != null && initialValuesitem.Input_arguments_Tooltip.Trim() != string.Empty)
                    {
                        ArgumentsTextboxIsEnabled = true;
                        ArgumentTextBoxTooltip = initialValuesitem.Input_arguments_Tooltip.Trim();
                    }
                 
                    if (!initialValuesitem.IsPayloadAvailable && initialValuesitem.ApiReference != null && initialValuesitem.ApiReference != string.Empty)
                    {
                        SetReferenceBtnIsEnabled = true;
                        SetReferenceContent = initialValuesitem.ApiReference;
                    }
                                                    

                    if (initialValuesitem.Reference_key != null && initialValuesitem.Reference_key != string.Empty)
                        ReferenceKey = initialValuesitem.Reference_key;

                    if (initialValuesitem.Payload_key != null && initialValuesitem.Payload_key != string.Empty)
                        PayloadKey = initialValuesitem.Payload_key;
                }
                methods_QRCMSelectedItemValue = value;
                OnPropertyChanged("QRCM_MethodsSelectedItem");
            }
        }

        private ObservableCollection<string> verifyQRCM_DevicesListValue = new ObservableCollection<string>();
        public ObservableCollection<string> VerifyQRCM_DevicesList
        {
            get
            {
                return verifyQRCM_DevicesListValue;
            }
            set
            {
                verifyQRCM_DevicesListValue = value;
                OnPropertyChanged("VerifyQRCM_DevicesList");
            }
        }

        private string deviceQRCMSelectedItemValue = null;
        public string QRCM_DeviceSelectedItem
        {
            get
            {
                return deviceQRCMSelectedItemValue;
            }
            set
            {
                deviceQRCMSelectedItemValue = value;
                if (value != null && value != string.Empty)
                    MethodNameComboboxIsEnabled = true;
                else
                    MethodNameComboboxIsEnabled = false;

                OnPropertyChanged("QRCM_DeviceSelectedItem");
            }
        }

        private bool methodNameComboboxIsEnabledValue = false;
        public bool MethodNameComboboxIsEnabled
        {
            get { return methodNameComboboxIsEnabledValue; }
            set { methodNameComboboxIsEnabledValue = value; OnPropertyChanged("MethodNameComboboxIsEnabled"); }
        }

        private Dictionary<string, string> _QRCM_DeviceModelValue = new Dictionary<string, string>();
        public Dictionary<string, string> QRCM_DeviceModel
        {
            get
            {
                return _QRCM_DeviceModelValue;
            }
            set
            {
                _QRCM_DeviceModelValue = value;
                OnPropertyChanged("QRCM_DeviceModel");
            }
        }

        private string verifyUserArgumentsValue = null;
        public string VerifyUserArguments
        {
            get
            {
                return verifyUserArgumentsValue;
            }
            set
            {
                verifyUserArgumentsValue = value;
                OnPropertyChanged("VerifyUserArguments");
            }
        }

        private string argumentTextBoxTooltipValue = "Enter user input arguments";
        public string ArgumentTextBoxTooltip
        {
            get { return argumentTextBoxTooltipValue; }
            set
            {
                argumentTextBoxTooltipValue = value;
                OnPropertyChanged("ArgumentTextBoxTooltip");
            }
        }

        private string argumentTextBoxWaterMarkValue = "Enter user input arguments";
        public string ArgumentTextBoxWaterMark
        {
            get { return argumentTextBoxWaterMarkValue; }
            set
            {
                argumentTextBoxWaterMarkValue = value;
                OnPropertyChanged("ArgumentTextBoxWaterMark");
            }
        }

        private bool argumentsTextboxIsEnabledValue = false;
        public bool ArgumentsTextboxIsEnabled
        {
            get { return argumentsTextboxIsEnabledValue; }
            set { argumentsTextboxIsEnabledValue = value; OnPropertyChanged("ArgumentsTextboxIsEnabled"); }
        }

        private bool setReferenceBtnIsEnabledValue = false;
        public bool SetReferenceBtnIsEnabled
        {
            get { return setReferenceBtnIsEnabledValue; }
            set { setReferenceBtnIsEnabledValue = value; OnPropertyChanged("SetReferenceBtnIsEnabled"); }
        }

        private string setReferenceContentValue = string.Empty;
        public string SetReferenceContent
        {
            get { return setReferenceContentValue; }
            set { setReferenceContentValue = value; OnPropertyChanged("SetReferenceContent"); }
        }

        private string referenceKeyValue = string.Empty;
        public string ReferenceKey
        {
            get { return referenceKeyValue; }
            set { referenceKeyValue = value; OnPropertyChanged("ReferenceKey"); }
        }

        private string payloadKeyValue = string.Empty;
        public string PayloadKey
        {
            get { return payloadKeyValue; }
            set { payloadKeyValue = value; OnPropertyChanged("PayloadKey"); }
        }
    }


    public class TestUserActionItem : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        void OnPropertyChanged(string property)
        {
            try
            {
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs(property));

                    if (ParentTestActionItem != null && ParentTestActionItem.ParentTestCaseItem != null && ParentTestActionItem.ParentTestCaseItem.SaveButtonIsEnabled != true)
                        ParentTestActionItem.ParentTestCaseItem.SaveButtonIsEnabled = true;
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

        private TestActionItem parentTestActionItemValue = null;
        public TestActionItem ParentTestActionItem
        {
            get { return parentTestActionItemValue; }
            set { parentTestActionItemValue = value; OnPropertyChanged("ParentTestActionItem"); }
        }

        private string actionUserTextValue = null;
        public string ActionUserText
        {
            get
            {
                return actionUserTextValue;
            }
            set
            {
                actionUserTextValue = value;
                OnPropertyChanged("ActionUserText");
            }
        }

  

    }

    public class TestUserVerifyItem : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        void OnPropertyChanged(string property)
        {
            try
            {
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs(property));

                    if (ParentTestActionItem != null && ParentTestActionItem.ParentTestCaseItem != null && ParentTestActionItem.ParentTestCaseItem.SaveButtonIsEnabled != true)
                        ParentTestActionItem.ParentTestCaseItem.SaveButtonIsEnabled = true;
                }
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                MessageBox.Show("Exception\n " + ex.Message, "Error Code - ECxxxxx", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private TestActionItem parentTestActionItemValue = null;
        public TestActionItem ParentTestActionItem
        {
            get { return parentTestActionItemValue; }
            set { parentTestActionItemValue = value; OnPropertyChanged("ParentTestActionItem"); }
        }

        private string verifyUserTextValue = null;
        public string VerifyUserText
        {
            get
            {
                return verifyUserTextValue;
            }
            set
            {
                verifyUserTextValue = value;
                OnPropertyChanged("VerifyUserText");
            }
        }   
    }


    public class TestCECItem : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        void OnPropertyChanged(string property)
        {
            try
            {
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs(property));

                    if (ParentTestActionItem != null && ParentTestActionItem.ParentTestCaseItem != null && ParentTestActionItem.ParentTestCaseItem.SaveButtonIsEnabled != true)
                        ParentTestActionItem.ParentTestCaseItem.SaveButtonIsEnabled = true;
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

        private TestActionItem parentTestActionItemValue = null;
        public TestActionItem ParentTestActionItem
        {
            get { return parentTestActionItemValue; }
            set { parentTestActionItemValue = value; OnPropertyChanged("ParentTestActionItem"); }
        }

        //CEC Deviceselection
        private List<string> DeviceselectionValue = new List<string>();
        public List<string> Deviceselection
        {
            get
            {
                return DeviceselectionValue;
            }
            set
            {

                DeviceselectionValue = value;
                OnPropertyChanged("Deviceselection");
            }
        }


        private string DeviceselectionSelecetdItemValue = null;
        public string DeviceselectionSelecetdItem
        {
            get
            {
                return DeviceselectionSelecetdItemValue;
            }
            set
            {
                DeviceselectionSelecetdItemValue = value;
                OnPropertyChanged("DeviceselectionSelecetdItem");
            }
        }


        private List<string> CECCommandListValue = new List<string>();
        public List<string> CECCommandList
        {
            get
            {
                return CECCommandListValue;
            }
            set
            {

                CECCommandListValue = value;
                OnPropertyChanged("CECCommandList");
            }
        }

        private string CECActionOpcodeValue = null;
        public string CECActionOpcode
        {
            get
            {
                return CECActionOpcodeValue;
            }
            set
            {
                CECActionOpcodeValue = value;
                OnPropertyChanged("CECActionOpcode");
            }
        }

        private Visibility CECActionOpcodeVisibilityValue = Visibility.Collapsed;
        public Visibility CECActionOpcodeVisibility
        {
            get
            {
                return CECActionOpcodeVisibilityValue;
            }
            set
            {
                CECActionOpcodeVisibilityValue = value;
                OnPropertyChanged("CECActionOpcodeVisibility");
            }
        }

        private string CECCommandListSelectedItemValue = null;
        public string CECCommandListSelectedItem
        {
            get
            {

                return CECCommandListSelectedItemValue;
            }
            set
            {
                CECCommandListSelectedItemValue = value;
                OnPropertyChanged("CECCommandListSelectedItem");

                if(value == "Others")
                    CECActionOpcodeVisibility = Visibility.Visible;
                else
                    CECActionOpcodeVisibility = Visibility.Collapsed;
            }
        }

       
    }

    public class TestVerifyCECItem : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        void OnPropertyChanged(string property)
        {
            try
            {
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs(property));

                    if (ParentTestActionItem != null && ParentTestActionItem.ParentTestCaseItem != null && ParentTestActionItem.ParentTestCaseItem.SaveButtonIsEnabled != true)
                        ParentTestActionItem.ParentTestCaseItem.SaveButtonIsEnabled = true;
                }
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                MessageBox.Show("Exception\n " + ex.Message, "Error Code - ECxxxxx", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private TestActionItem parentTestActionItemValue = null;
        public TestActionItem ParentTestActionItem
        {
            get { return parentTestActionItemValue; }
            set { parentTestActionItemValue = value; OnPropertyChanged("ParentTestActionItem"); }
        }

        private string CECverificationOpcodeValue = null;
        public string CECverificationOpcode
        {
            get
            {
                return CECverificationOpcodeValue;
            }
            set
            {
                CECverificationOpcodeValue = value;
                OnPropertyChanged("CECverificationOpcode");
            }
        }
    }

    public class TestVerifyQRItem : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        void OnPropertyChanged(string property)
        {
            try
            {
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs(property));

                    if (ParentTestActionItem != null && ParentTestActionItem.ParentTestCaseItem != null && ParentTestActionItem.ParentTestCaseItem.SaveButtonIsEnabled != true)
                        ParentTestActionItem.ParentTestCaseItem.SaveButtonIsEnabled = true;
                }
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                MessageBox.Show("Exception\n " + ex.Message, "Error Code - ECxxxxx", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private TestActionItem parentTestActionItemValue = null;
        public TestActionItem ParentTestActionItem
        {
            get { return parentTestActionItemValue; }
            set { parentTestActionItemValue = value; OnPropertyChanged("ParentTestActionItem"); }
        }

        private string QRverificationcodeValue = null;
        public string QRverificationcode
        {
            get
            {
                return QRverificationcodeValue;
            }
            set
            {
                QRverificationcodeValue = value;
                OnPropertyChanged("CQRverificationcode");
            }
        }
        private string QRverifytypeValue = string.Empty;
        public string QRverifytype
        {
            get
            {
                return QRverifytypeValue;
            }
            set
            {
                QRverifytypeValue = value;
                OnPropertyChanged("QRverifytype");
            }
        }
        private Dictionary<string, string> CameraListValue = new Dictionary<string, string>();
        public Dictionary<string, string> CameraList
        {
            get
            {


                return CameraListValue;
            }
            set
            {
                CameraListValue = value;
                OnPropertyChanged("CameraList");

                //if (value == "Others")
                //    CECActionOpcodeVisibility = Visibility.Visible;
                //else
                //    CECActionOpcodeVisibility = Visibility.Collapsed;
            }
            //}

        }
        private ObservableCollection<string> CameraList1Value = new ObservableCollection<string>();
        public ObservableCollection<string> CameraList1
        {
            get
            {


                return CameraList1Value;
            }
            set
            {
                CameraList1Value = value;
                OnPropertyChanged("CameraList1");

                //if (value == "Others")
                //    CECActionOpcodeVisibility = Visibility.Visible;
                //else
                //    CECActionOpcodeVisibility = Visibility.Collapsed;
            }
            //}

        }


        private string CameraSelectedItemValue = null;
        public string CameraSelectedItem
        {
            get
            {
                return CameraSelectedItemValue;
            }
            set
            {
                CameraSelectedItemValue = value;
                OnPropertyChanged("CameraSelectedItem");
            }
        }
    }

    public class TestScriptVerification : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        void OnPropertyChanged(string property)
        {
            try
            {
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs(property));

                    if (ParentTestActionItem != null && ParentTestActionItem.ParentTestCaseItem != null && ParentTestActionItem.ParentTestCaseItem.SaveButtonIsEnabled != true)
                        ParentTestActionItem.ParentTestCaseItem.SaveButtonIsEnabled = true;
                }
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif               
            }
        }

        private ObservableCollection<string> DevicenamelistItemValue = new ObservableCollection<string>();
        public ObservableCollection<string> Devicenamelist
        {
            get { return DevicenamelistItemValue; }
            set { DevicenamelistItemValue = value; OnPropertyChanged("Devicenamelist"); }
        }

        private TestActionItem parentTestActionItemValue = null;
        public TestActionItem ParentTestActionItem
        {
            get { return parentTestActionItemValue; }
            set { parentTestActionItemValue = value; OnPropertyChanged("ParentTestActionItem"); }
        }

        Dictionary<string, string> DevicenameWithModelValue = new Dictionary<string, string>();
        public Dictionary<string, string> DevicenameWithModel
        {
            get { return DevicenameWithModelValue; }
            set { DevicenameWithModelValue = value; OnPropertyChanged("DevicenameWithModel"); }
        }

        private bool verifyDesignDevicesIsCheckedValue = false;
        public bool VerifyDesignDevicesIsChecked
        {
            get { return verifyDesignDevicesIsCheckedValue; }
            set { verifyDesignDevicesIsCheckedValue = value; OnPropertyChanged("VerifyDesignDevicesIsChecked"); }
        }

        private Visibility verifyDesignDevicesVisibilityValue = Visibility.Hidden;
        public Visibility VerifyDesignDevicesVisibility
        {
            get { return verifyDesignDevicesVisibilityValue; }
            set { verifyDesignDevicesVisibilityValue = value; OnPropertyChanged("VerifyDesignDevicesVisibility"); }
        }

        private string DevicenamelistSelectedItemValue = null;
        public string DevicenamelistSelectedItem
        {
            get { return DevicenamelistSelectedItemValue; }
            set
            {

                if (value != null && value != string.Empty)
                {
                    if (DevicenameWithModel.Keys.Contains(value.ToLower()) && DevicenameWithModel[value.ToLower()].StartsWith("core"))
                    {
                        DeviceModel = DevicenameWithModel[value.ToLower()];

                        if (VerifyScriptActionSelectedItem != "CPU Monitoring" && VerifyScriptActionSelectedItem != "Deploy Monitoring" && VerifyScriptActionSelectedItem != "LoadFromCore Monitoring")
                            VerifyDesignDevicesVisibility = Visibility.Visible;
                    }
                    else
                    {
                        if (DevicenameWithModel.Keys.Contains(value.ToLower()))
                            DeviceModel = DevicenameWithModel[value.ToLower()];

                        VerifyDesignDevicesVisibility = Visibility.Hidden;
                        VerifyDesignDevicesIsChecked = false;
                    }
                }


                if (VerifyScriptActionSelectedItem == "Custom Command")
                    CustomCommandTextboxIsEnabled = true;
                else
                    CustomCommandTextboxIsEnabled = false;

                DevicenamelistSelectedItemValue = value;
                OnPropertyChanged("DevicenamelistSelectedItem");
            }
        }

        private string deviceModelValue = string.Empty;
        public string DeviceModel
        {
            get { return deviceModelValue; }
            set { deviceModelValue = value; OnPropertyChanged("DeviceModel"); }
        }

        private ObservableCollection<string> VerifyScriptActionValue = new ObservableCollection<string>() { "Custom Command", "SQLITE3.db Available Space", "Memory Monitoring", "CPU Monitoring", "Deploy Monitoring", "LoadFromCore Monitoring" };
        public ObservableCollection<string> VerifyScriptAction
        {
            get { return VerifyScriptActionValue; }
            set { VerifyScriptActionValue = value; OnPropertyChanged("VerifyScriptAction"); }
        }

        private string VerifyScriptActionSelectedItemValue = null;
        public string VerifyScriptActionSelectedItem
        {
            get { return VerifyScriptActionSelectedItemValue; }
            set
             {
                if (VerifyScriptActionSelectedItem == "Deploy Monitoring" || VerifyScriptActionSelectedItem == "CPU Monitoring" || VerifyScriptActionSelectedItem == "LoadFromCore Monitoring")
                    Devicenamelist.Clear();

                VerifyScriptActionSelectedItemValue = value;
                Upperlimit = string.Empty;
                Lowerlimit = string.Empty;
                StackColumposition = 5;
                CustomCommand = string.Empty;
                RegexMatch = string.Empty;
                RegexTextboxIsEnabled = false;
                LimitUnitIsEnabled = true;
                CustomCommandTextboxIsEnabled = false;
                CPUVisibility = Visibility.Visible;
                CPUnumbercomboVisibility = Visibility.Collapsed;

                if (value == "SQLITE3.db Available Space")
                {
                    CustomCommand = "df -m";
                    RegexMatch = @"/dev/loop3\s*\d*\s*\d*\s*(\d*)\s*\d*%";
                    LimitUnitSelectedItem = "MB";
                    LimitUnitIsEnabled = false;
                    RegexTextboxIsEnabled = false;
                }
                else if (value == "Memory Monitoring")
                {
                    CustomCommand = "mem_stat.sh";
                    RegexMatch = @"Usage:\s*(.*)";
                    LimitUnitSelectedItem = "MB";
                    LimitUnitIsEnabled = false;
                    RegexTextboxIsEnabled = false;
                }
                else if (value == "Custom Command")
                {
                    CustomCommand = string.Empty;
                    RegexMatch = string.Empty;
                    LimitUnitIsEnabled = true;
                    RegexTextboxIsEnabled = true;
                    CustomCommandTextboxIsEnabled = true;
                }
                //else if(value == "CPU Monitoring")
                //{
                //    CPUVisibility = Visibility.Collapsed;
                //    StackColumposition = 2;
                //}
                else if (value == "Deploy Monitoring" || value == "CPU Monitoring" || value == "LoadFromCore Monitoring")
                {
                    if (value == "CPU Monitoring")
                        CPUnumbercomboVisibility = Visibility.Visible;

                    CPUVisibility = Visibility.Collapsed;
                    StackColumposition = 3;
                    ScriptSelectedDeviceItem.Clear();

                    if (ParentTestActionItem != null && ParentTestActionItem.ParentTestCaseItem != null && ParentTestActionItem.ParentTestCaseItem.SelectedDeviceItemList != null)
                    {
                        foreach (var item in ParentTestActionItem.ParentTestCaseItem.SelectedDeviceItemList)
                        {
                            if (item.ItemDeviceModel.ToLower().Contains("core") && item.ItemPrimaryorBackup == "primary")
                                ScriptSelectedDeviceItem.Add(item.ItemDeviceName);
                        }
                    }

                    Devicenamelist = ScriptSelectedDeviceItem;

                    if(ScriptSelectedDeviceItem.Count > 0)
                        DevicenamelistSelectedItem = ScriptSelectedDeviceItem[0];                  
                }

                if ((value == "SQLITE3.db Available Space" || value == "Memory Monitoring" || value == "Custom Command") && DevicenamelistSelectedItem != null && DevicenameWithModel.Keys.Contains(DevicenamelistSelectedItem.ToLower()) && DevicenameWithModel[DevicenamelistSelectedItem.ToLower()].StartsWith("core"))
                {
                    VerifyDesignDevicesVisibility = Visibility.Visible;
                }
                else
                {
                    VerifyDesignDevicesVisibility = Visibility.Hidden;
                }

                if (ParentTestActionItem != null && ParentTestActionItem.VerifyTestScriptList != null && ParentTestActionItem.VerifyTestScriptList.Count > 0)
                {
                    ////// Duration check box enable/disable
                    int cpuCount = ParentTestActionItem.VerifyTestScriptList.Where(x => x.VerifyScriptActionSelectedItem == "CPU Monitoring" ||  x.VerifyScriptActionSelectedItem == "Deploy Monitoring" || x.VerifyScriptActionSelectedItem == "LoadFromCore Monitoring").Count();

                    if (cpuCount > 0)
                        ParentTestActionItem.ScriptExecuteIterationChkbxEnable = false;
                    else
                        ParentTestActionItem.ScriptExecuteIterationChkbxEnable = true;
                }
                
                OnPropertyChanged("VerifyScriptActionSelectedItem");
            }
        }

        private ObservableCollection<string> CPUNumberListValue = new ObservableCollection<string>() { "0", "1", "2", "3", "All" };
        public ObservableCollection<string> CPUNumberList
        {
            get { return CPUNumberListValue; }
            set { CPUNumberListValue = value; OnPropertyChanged("CPUNumberList"); }
        }

        private string CPUNumberSelectedItemValue = "0";
        public string CPUNumberSelectedItem
        {
            get { return CPUNumberSelectedItemValue; }
            set { CPUNumberSelectedItemValue = value; OnPropertyChanged("CPUNumberSelectedItem"); }
        }

        private Visibility CPUnumbercomboVisibilityValue = Visibility.Collapsed;
        public Visibility CPUnumbercomboVisibility
        {
            get { return CPUnumbercomboVisibilityValue; }
            set { CPUnumbercomboVisibilityValue = value; OnPropertyChanged("CPUnumbercomboVisibility"); }
        }

        private Visibility CPUVisibilityValue = Visibility.Visible;
        public Visibility CPUVisibility
        {
            get { return CPUVisibilityValue; }
            set { CPUVisibilityValue = value; OnPropertyChanged("CPUVisibility"); }
        }

        private string customCommandValue = string.Empty;
        public string CustomCommand
        {
            get { return customCommandValue; }
            set { customCommandValue = value; OnPropertyChanged("CustomCommand"); }
        }

        private bool customCommandTextboxIsEnabledValue = false;
        public bool CustomCommandTextboxIsEnabled
        {
            get { return customCommandTextboxIsEnabledValue; }
            set { customCommandTextboxIsEnabledValue = value; OnPropertyChanged("CustomCommandTextboxIsEnabled"); }
        }

        private string RegexMatchValue = string.Empty;
        public string RegexMatch
        {
            get { return RegexMatchValue; }
            set { RegexMatchValue = value; OnPropertyChanged("RegexMatch"); }
        }

        private bool RegexTextboxIsEnabledValue = false;
        public bool RegexTextboxIsEnabled
        {
            get { return RegexTextboxIsEnabledValue; }
            set { RegexTextboxIsEnabledValue = value; OnPropertyChanged("RegexTextboxIsEnabled"); }
        }

        private string UpperlimitValue = string.Empty;
        public string Upperlimit
        {
            get { return UpperlimitValue; }
            set { UpperlimitValue = value; OnPropertyChanged("Upperlimit"); }
        }

        private string LowerlimitValue = string.Empty;
        public string Lowerlimit
        {
            get { return LowerlimitValue; }
            set { LowerlimitValue = value; OnPropertyChanged("Lowerlimit"); }
        }

        private List<string> limitUnitlistValue = new List<string>() { "No Unit", "KB", "MB", "GB", "%" };
        public List<string> LimitUnitlist
        {
            get { return limitUnitlistValue; }
            set { limitUnitlistValue = value; OnPropertyChanged("LimitUnitlist"); }
        }

        private string limitUnitSelectedItemValue = null;
        public string LimitUnitSelectedItem
        {
            get { return limitUnitSelectedItemValue; }
            set { limitUnitSelectedItemValue = value; OnPropertyChanged("LimitUnitSelectedItem"); }
        }

        private bool limitUnitIsEnabledValue = true;
        public bool LimitUnitIsEnabled
        {
            get { return limitUnitIsEnabledValue; }
            set { limitUnitIsEnabledValue = value; OnPropertyChanged("LimitUnitIsEnabled"); }
        }

        private int StackColumpositionValue = 5;
        public int StackColumposition
        {
            get { return StackColumpositionValue; }
            set { StackColumpositionValue = value; OnPropertyChanged("StackColumposition"); }
        }

        private Dictionary<string, string[]> deviceWithIPValue = new Dictionary<string, string[]>();
        public Dictionary<string, string[]> DeviceWithIP
        {
            get { return deviceWithIPValue; }
            set { deviceWithIPValue = value; OnPropertyChanged("DeviceWithIP"); }
        }

        private string buildVersionVal = string.Empty;
        public string BuildVersion
        {
            get { return buildVersionVal; }
            set { buildVersionVal = value; OnPropertyChanged("BuildVersion"); }
        }

        private bool skipExecutionVal = false;
        public bool SkipExecution
        {
            get { return skipExecutionVal; }
            set { skipExecutionVal = value; OnPropertyChanged("SkipExecution"); }
        }

        private List<string> notAvailableDeviceListVal = new List<string>();
        public List<string> NotAvailableDeviceList
        {
            get { return notAvailableDeviceListVal; }
            set { notAvailableDeviceListVal = value; OnPropertyChanged("NotAvailableDeviceList"); }
        }

        private bool HasInventoryVal = true;
        public bool HasInventory
        {
            get { return HasInventoryVal; }
            set { HasInventoryVal = value; OnPropertyChanged("HasInventory"); }
        }

        private ObservableCollection<string> scriptSelectedDeviceItemValue = new ObservableCollection<string>();
        public ObservableCollection<string> ScriptSelectedDeviceItem
        {
            get { return scriptSelectedDeviceItemValue; }
            set { scriptSelectedDeviceItemValue = value; OnPropertyChanged("ScriptSelectedDeviceItem"); }
        }

        private bool isUnitEnteredVal = false;
        public bool IsUnitEntered
        {
            get { return isUnitEnteredVal; }
            set { isUnitEnteredVal = value; OnPropertyChanged("IsUnitEntered"); }
        }
    }
    
    public class TestTelnetItem : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        void OnPropertyChanged(string property)
        {
            try
            {
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs(property));

                    if (ParentTestActionItem != null && ParentTestActionItem.ParentTestCaseItem != null && ParentTestActionItem.ParentTestCaseItem.SaveButtonIsEnabled != true)
                        ParentTestActionItem.ParentTestCaseItem.SaveButtonIsEnabled = true;
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

        private TestActionItem parentTestActionItemValue = null;
        public TestActionItem ParentTestActionItem
        {
            get { return parentTestActionItemValue; }
            set { parentTestActionItemValue = value; OnPropertyChanged("ParentTestActionItem"); }
        }

        private string telnetCommandValue = null;
        public string TelnetCommand
        {
            get { return telnetCommandValue; }
            set { telnetCommandValue = value; OnPropertyChanged("TelnetCommand"); }
        }

        private ObservableCollection<DUT_DeviceItem> telnetDeviceItemValue = new ObservableCollection<DUT_DeviceItem>();
        public ObservableCollection<DUT_DeviceItem> TelnetDeviceItem
        {
            get { return telnetDeviceItemValue; }
            set { telnetDeviceItemValue = value; OnPropertyChanged("TelnetDeviceItem"); }
        }

        private ObservableCollection<string> telnetSelectedDeviceItemValue = new ObservableCollection<string>();
        public ObservableCollection<string> TelnetSelectedDeviceItem
        {
            get { return telnetSelectedDeviceItemValue; }
            set { telnetSelectedDeviceItemValue = value; OnPropertyChanged("TelnetSelectedDeviceItem"); }
        }

        private Dictionary<string, string> telnetSelectedDeviceModelValue = new Dictionary<string, string>();
        public Dictionary<string, string> TelnetSelectedDeviceModel
        {
            get { return telnetSelectedDeviceModelValue; }
            set { telnetSelectedDeviceModelValue = value; OnPropertyChanged("TelnetSelectedDeviceModel"); }
        }

        private string telnetSelectedDeviceValue = null;
        public string TelnetSelectedDevice
        {
            get { return telnetSelectedDeviceValue; }
            set { telnetSelectedDeviceValue = value; OnPropertyChanged("TelnetSelectedDevice"); }
        }

        private string telnetVerifyTypeSelectedValue = "Store Current Result";
        public string TelnetVerifyTypeSelected
        {
            get { return telnetVerifyTypeSelectedValue; }
            set
            {
                telnetVerifyTypeSelectedValue = value;
                if (value == "Compare Values")
                {
                    TelnetFailureTextIsEnabled = true;
                    //TelnetFailureText = string.Empty;
                }
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
        private List<string> KeywordTypeListValue = new List<string> { "Keyword exist", "Keyword not exist" };
        public List<string> KeywordTypeList
        {
            get { return KeywordTypeListValue; }
            set { KeywordTypeListValue = value; OnPropertyChanged("KeywordTypeList"); }
        }

        private string KeywordTypeVerifyValue = "Keyword not exist";
        public string KeywordTypeVerify
        {
            get { return KeywordTypeVerifyValue; }
            set { KeywordTypeVerifyValue = value;

                 if (KeywordTypeVerify == "Keyword not exist")
                    keywordTypetooltip = "Pass when keyword text not availabe in telnet response";
                else if (KeywordTypeVerify == "Keyword exist")
                    keywordTypetooltip = "Pass when keyword text availabe in telnet response";

                OnPropertyChanged("KeywordTypeVerify"); }
        }
        private string keywordTypetooltipValue = "Pass when keyword text not available in telnet response";
        public string keywordTypetooltip
        {
            get { return keywordTypetooltipValue; }
            set { keywordTypetooltipValue = value;             
                OnPropertyChanged("keywordTypetooltip"); }
        }

        private bool telnetFailureTextIsEnabledValue = false;
        public bool TelnetFailureTextIsEnabled
        {
            get { return telnetFailureTextIsEnabledValue; }
            set { telnetFailureTextIsEnabledValue = value; OnPropertyChanged("TelnetFailureTextIsEnabled"); }
        }

        private string telnetFailureTextValue = null;
        public string TelnetFailureText
        {
            get {
                if (telnetFailureTextValue != null)
                {
                    return telnetFailureTextValue.TrimStart();
                }
                else
                {
                    return telnetFailureTextValue;
                }
            }
            set { telnetFailureTextValue = value; OnPropertyChanged("TelnetFailureText"); }
        }

        private ObservableCollection<string> CBMValues = new ObservableCollection<string>();
        public ObservableCollection<string> CBM
        {
            get { return CBMValues; }
            set { CBMValues = value; OnPropertyChanged("CBM"); }
        }
    }

    public class TestFirmwareItem : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        void OnPropertyChanged(string property)
        {
            try
            {
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs(property));

                    if (ParentTestActionItem != null && ParentTestActionItem.ParentTestCaseItem != null && ParentTestActionItem.ParentTestCaseItem.SaveButtonIsEnabled != true)
                        ParentTestActionItem.ParentTestCaseItem.SaveButtonIsEnabled = true;
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

        private TestActionItem parentTestActionItemValue = null;
        public TestActionItem ParentTestActionItem
        {
            get { return parentTestActionItemValue; }
            set { parentTestActionItemValue = value; OnPropertyChanged("ParentTestActionItem"); }
        }

        private string firmwareTypeSelectedValue = "Automatically update when new version of SW available";
        public string FirmwareTypeSelected
        {
            get { return firmwareTypeSelectedValue; }
            set
            {
                FirmwareBrowseLocation = string.Empty;
                firmwareTypeSelectedValue = value;
                if (firmwareTypeSelectedValue == "Start auto update with new version of SW at")
                {
                    selectionoption = true;
                    TimeSelectionEnabled = true;
                    InstallSelectionEnabled = true;
                }
                else if (firmwareTypeSelectedValue == "Automatically update when new version of SW available")
                {
                    InstallSelectionEnabled = true;
                    TimeSelectionEnabled = false;
                    selectionoption = false;
                }
                else
                {
                    selectionoption = false;
                    TimeSelectionEnabled = false;
                    InstallSelectionEnabled = false;
                }

                OnPropertyChanged("FirmwareTypeSelected");
            }
        }

        private string firmwareBrowseLocationValue = null;
        public string FirmwareBrowseLocation
        {
            get { return firmwareBrowseLocationValue; }
            set
            {
                firmwareBrowseLocationValue = value;
                OnPropertyChanged("FirmwareBrowseLocation");
            }
        }

        private string firmwareDateValue = null;
        public string FirmwareDate
        {
            get { return firmwareDateValue; }
            set
            {
                if(value!=null&&value!=string.Empty)
                {
                    if (value[2] == '-' && value != firmwareDateValue)
                    {
                        firmwareDateValue = value;
                        OnPropertyChanged("FirmwareDate");
                    }
                }     
            }
        }

        //selectionoption
        private bool selectionoptionvalue = false;
        public bool selectionoption
        {
            get { return selectionoptionvalue; }
            set
            {

                selectionoptionvalue = value;
                OnPropertyChanged("selectionoption");
            }
        }

        private List<string> InstallSelectionComboListValue = new List<string> { "Q-Sys Designer", "Designer,Administrator,UCI Viewer" };
        public List<string> InstallSelectionComboList
        {
            get { return InstallSelectionComboListValue; }
            private set
            {
                InstallSelectionComboListValue = value;
                OnPropertyChanged("InstallSelectionComboList");
            }
        }

        private string InstallSelectionComboSelectedItemValue = "Q-Sys Designer";
        public string InstallSelectionComboSelectedItem
        {
            get { return InstallSelectionComboSelectedItemValue; }
            set { InstallSelectionComboSelectedItemValue = value; OnPropertyChanged("InstallSelectionComboSelectedItem"); }
        }

        private bool InstallSelectionEnabledValue = true;
        public bool InstallSelectionEnabled
        {
            get { return InstallSelectionEnabledValue; }
            set { InstallSelectionEnabledValue = value; OnPropertyChanged("InstallSelectionEnabled"); }
        }


        private ObservableCollection<string> TimeSelectionComboListValue = new ObservableCollection<string> { "12:00 AM", "01:00 AM", "02:00 AM", "03:00 AM", "04:00 AM", "05:00 AM", "06:00 AM", "07:00 AM", "08:00 AM", "09:00 AM", "10:00 AM", "11:00 AM", "12:00 PM", "01:00 PM", "02:00 PM", "03:00 PM", "04:00 PM", "05:00 PM", "06:00 PM", "07:00 PM", "08:00 PM", "09:00 PM", "10:00 PM", "11:00 PM" };
        public ObservableCollection<string> TimeSelectionComboList
        {
            get { return TimeSelectionComboListValue; }
            private set
            {
                TimeSelectionComboListValue = value;
                OnPropertyChanged("TimeSelectionComboList");
            }
        }

        private string TimeSelectionComboSelectedItemValue = null;
        public string TimeSelectionComboSelectedItem
        {
            get { return TimeSelectionComboSelectedItemValue; }
            set { TimeSelectionComboSelectedItemValue = value; OnPropertyChanged("TimeSelectionComboSelectedItem"); }
        }

        private bool TimeSelectionEnabledValue = false;
        public bool TimeSelectionEnabled
        {
            get { return TimeSelectionEnabledValue; }
            set { TimeSelectionEnabledValue = value; OnPropertyChanged("TimeSelectionEnabled"); }
        }

        private ObservableCollection<string> firmwareTypeContentListValue = new ObservableCollection<string>(QatConstants.TestFirmwareItemFirmwareUpdateTypeDisplayList);
        public ObservableCollection<string> FirmwareTypeContentList
        {
            get { return firmwareTypeContentListValue; }
            set { firmwareTypeContentListValue = value; OnPropertyChanged("FirmwareTypeContentList"); }
        }

        private bool MeasureFirmwareUpTimeValue = false;
        public bool MeasureFirmwareUpTime
        {
            get { return MeasureFirmwareUpTimeValue; }
            set { MeasureFirmwareUpTimeValue = value; OnPropertyChanged("MeasureFirmwareUpTime"); }
        }
    }

    public class TestDesignerItem : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        void OnPropertyChanged(string property)
        {
            try
            {
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs(property));

                    if (ParentTestActionItem != null && ParentTestActionItem.ParentTestCaseItem != null && ParentTestActionItem.ParentTestCaseItem.SaveButtonIsEnabled != true)
                        ParentTestActionItem.ParentTestCaseItem.SaveButtonIsEnabled = true;
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

        private TestActionItem parentTestActionItemValue = null;
        public TestActionItem ParentTestActionItem
        {
            get { return parentTestActionItemValue; }
            set { parentTestActionItemValue = value; OnPropertyChanged("ParentTestActionItem"); }
        }



        private bool ConnectDesignervalue = false;
        public bool ConnectDesigner
        {
            get { return ConnectDesignervalue;}
            set
            {
                ConnectDesignervalue = value;
                if (value == true)
                {
                    if (!newdesigncheckValue)
                    {
                        Isnewdesignchecked = Visibility.Visible;
                        DesignerTimeout = "50";
                    }
                    else
                    {
                        Isnewdesignchecked = Visibility.Collapsed;
                    }
                    newdesigncheckEnable = true;
                }
                else
                {
                    Isnewdesignchecked = Visibility.Collapsed;
                }

                OnPropertyChanged("ConnectDesigner");}
        }

        private bool newdesigncheckValue = false;
        public bool newdesigncheck
        {
            get { return newdesigncheckValue; }
            set
            {
                newdesigncheckValue = value;
                if (value == true)
                {
                    TxtNoOfTimeDeployVisible = Visibility.Collapsed;
                    Isnewdesignchecked = Visibility.Collapsed;
                }
                else if (value == false && ConnectDesignervalue)
                {
                    Isnewdesignchecked = Visibility.Visible;
                    DesignerTimeout = "50";
                }
                else if (value == false && ChkNoOfTimeDeployCheck)
                {
                    TxtNoOfTimeDeployVisible = Visibility.Visible;
                    DesignerTimeout = "20";
                }

                if (newdesigncheck == true && DisconnectDesigner == true)
                {                    
                    newdesigncheck = false;
                    newdesigncheckEnable = false;
                }
                else if(newdesigncheck==true)
                {
                    IsEnabledDesignerTimeout = Visibility.Collapsed;
                    DesignerTimeout = string.Empty;
                    DisconnectDesignerEnable = false;                   
                }
                else
                {
                   if( EmulateDesigner== false)
                       IsEnabledDesignerTimeout = Visibility.Visible;

                    DisconnectDesignerEnable = true;
                }

                OnPropertyChanged("newdesigncheck");
            }
        }
        
        private Visibility Isnewdesigncheckedvalue = Visibility.Hidden;
        public Visibility Isnewdesignchecked
        {
            get { return Isnewdesigncheckedvalue; }
            set
            {
                Isnewdesigncheckedvalue = value;
                OnPropertyChanged("Isnewdesignchecked");
            }
        }

        private bool ChkNoOfTimeDeployCheckval = false;
        public bool ChkNoOfTimeDeployCheck
        {
            get { return ChkNoOfTimeDeployCheckval; }
            set
            {
                ChkNoOfTimeDeployCheckval = value;

                if (value)
                {
                    TxtNoOfTimeDeployVisible = Visibility.Visible;
                    IterationVisibility = Visibility.Visible;
                    newdesigncheckEnable = false;
                    DesignerTimeout = "20";
                    TimeoutTooltip = "Disconnect Timeout: Time to wait after f7 key press";
                }
                else
                {
                    TxtNoOfTimeDeployVisible = Visibility.Collapsed;

                    if(!Loadfromcore)
                        IterationVisibility = Visibility.Collapsed;

                    TimeoutTooltip = "Enter wait time";
                }

                OnPropertyChanged("ChkNoOfTimeDeployCheck");
            }
        }

        private bool loadfromcoreval = false;
        public bool Loadfromcore
        {
            get { return loadfromcoreval; }
            set
            {
                loadfromcoreval = value;

                if (value)
                {
                    IterationVisibility = Visibility.Visible;
                    newdesigncheckEnable = false;
                    DesignerTimeout = "20";
                }
                else if(!ChkNoOfTimeDeployCheck)
                {
                    IterationVisibility = Visibility.Collapsed;
                }

                OnPropertyChanged("Loadfromcore");
            }
        }
        private Visibility TxtNoOfTimeDeployVisibleval = Visibility.Collapsed;
        public Visibility TxtNoOfTimeDeployVisible
        {
            get { return TxtNoOfTimeDeployVisibleval; }
            set { TxtNoOfTimeDeployVisibleval = value; OnPropertyChanged("TxtNoOfTimeDeployVisible"); }
        }

        private Visibility iterationVisibilityval = Visibility.Collapsed;
        public Visibility IterationVisibility
        {
            get { return iterationVisibilityval; }
            set { iterationVisibilityval = value; OnPropertyChanged("IterationVisibility"); }
        }

        private Visibility ChkNoOfTimeDeployVisibleval = Visibility.Collapsed;
        public Visibility ChkNoOfTimeDeployVisible
        {
            get { return ChkNoOfTimeDeployVisibleval; }
            set { ChkNoOfTimeDeployVisibleval = value; OnPropertyChanged("ChkNoOfTimeDeployVisible"); }
        }

        private Visibility isEnabledDesignerTimeoutValue = Visibility.Visible;
        public Visibility IsEnabledDesignerTimeout
        {
            get { return isEnabledDesignerTimeoutValue; }
            set { isEnabledDesignerTimeoutValue = value; OnPropertyChanged("IsEnabledDesignerTimeout"); }
        }

        private string timeoutTooltipValue = "Enter wait time";
        public string TimeoutTooltip
        {
            get { return timeoutTooltipValue; }
            set { timeoutTooltipValue = value; OnPropertyChanged("TimeoutTooltip"); }
        }

        private string designerTimeoutValue = "20";
        public string DesignerTimeout
        {
            get { return designerTimeoutValue; }
            set { designerTimeoutValue = value; OnPropertyChanged("DesignerTimeout"); }
        }

        private List<string> designerTimeoutUnitValue = new List<string> {  "Min", "Sec"};
        public List<string> DesignerTimeoutUnit
        {
            get { return designerTimeoutUnitValue; }
            set { designerTimeoutUnitValue = value; OnPropertyChanged("DesignerTimeoutUnit"); }
        }

        private string designerTimeoutUnitSelectedValue =   "Sec" ;
        public string DesignerTimeoutUnitSelected
        {
            get { return designerTimeoutUnitSelectedValue; }
            set { designerTimeoutUnitSelectedValue = value; OnPropertyChanged("DesignerTimeoutUnitSelected"); }
        }

        private string NoOfTimeDeployedval =  string.Empty;
        public string NoOfTimesDeployed
        {
            get { return NoOfTimeDeployedval; }
            set { NoOfTimeDeployedval = value; OnPropertyChanged("NoOfTimesDeployed"); }
        }

        private Visibility ConnectDesignerLabelvalue = Visibility.Hidden;
        public Visibility ConnectDesignerLabel
        {
            get { return ConnectDesignerLabelvalue; }
            set { ConnectDesignerLabelvalue = value; OnPropertyChanged("ConnectDesignerLabel"); }
        }

        private bool DisconnectDesignervalue = true;
        public bool DisconnectDesigner
        {
            get { return DisconnectDesignervalue; }
			set
            {
                DisconnectDesignervalue = value;

                if (value == true)
                {
                    newdesigncheckEnable = false;
                    DesignerTimeout = "20";
                }
                OnPropertyChanged("DisconnectDesigner");
            }
		}
        private bool DisconnectDesignerEnableValue = true;
        public bool DisconnectDesignerEnable
        {
            get { return DisconnectDesignerEnableValue; }
            set { DisconnectDesignerEnableValue = value; OnPropertyChanged("DisconnectDesignerEnable");}
        }
        private bool newdesigncheckEnableValue = false;
        public bool newdesigncheckEnable
        {
            get { return newdesigncheckEnableValue; }
            set { newdesigncheckEnableValue = value;            
                OnPropertyChanged("newdesigncheckEnable"); }
        }

        private bool EmulateDesignervalue = false;
        public bool EmulateDesigner
        {
            get { return EmulateDesignervalue; }
            set
            {
                EmulateDesignervalue = value;

                if (value == true)
                {
                    IsEnabledDesignerTimeout = Visibility.Collapsed;
                    DesignerTimeout = string.Empty;
                    newdesigncheckEnable = true;
                }
                else
                {
                    if(newdesigncheck== false)
                      IsEnabledDesignerTimeout = Visibility.Visible;
                }

                OnPropertyChanged("EmulateDesigner");
            }
        }
    }

    public class TestNetPairingItem : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        void OnPropertyChanged(string property)
        {
            try
            {
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs(property));

                    if (ParentTestActionItem != null && ParentTestActionItem.ParentTestCaseItem != null && ParentTestActionItem.ParentTestCaseItem.SaveButtonIsEnabled != true)
                        ParentTestActionItem.ParentTestCaseItem.SaveButtonIsEnabled = true;
                }
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                // MessageBox.Show("Exception\n " + ex.Message, "Error Code - ECxxxxx", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private TestActionItem parentTestActionItemValue = null;
        public TestActionItem ParentTestActionItem
        {
            get { return parentTestActionItemValue; }
            set { parentTestActionItemValue = value; OnPropertyChanged("ParentTestActionItem"); }
        }

        private ObservableCollection<DUT_DeviceItem> dutDeviceListValue = new ObservableCollection<DUT_DeviceItem>();
        public ObservableCollection<DUT_DeviceItem> DutDeviceList
        {
            get { return dutDeviceListValue; }
            set
            {
                dutDeviceListValue = value;

                DUT_DeviceItem[] alphaNumericSortedComponentType = dutDeviceListValue.ToArray();
                Array.Sort(alphaNumericSortedComponentType, new AlphanumComparatorFastDut());
                dutDeviceListValue = new ObservableCollection<DUT_DeviceItem>(alphaNumericSortedComponentType.ToList());

                OnPropertyChanged("DutDeviceList");
            }
        }

    }

    public class TestUsbAudioBridging : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        void OnPropertyChanged(string property)
        {
            try
            {
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs(property));

                    if (ParentTestActionItem != null && ParentTestActionItem.ParentTestCaseItem != null && ParentTestActionItem.ParentTestCaseItem.SaveButtonIsEnabled != true)
                        ParentTestActionItem.ParentTestCaseItem.SaveButtonIsEnabled = true;
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

        private TestActionItem parentTestActionItemValue = null;
        public TestActionItem ParentTestActionItem
        {
            get { return parentTestActionItemValue; }
            set { parentTestActionItemValue = value; OnPropertyChanged("ParentTestActionItem"); }
        }

        //Collection of Usb Device List

        private List<string> UsbAudioDeviceTypeListValue = new List<string>();
        public List<string> UsbAudioDeviceTypeList
        {
            get
            {
               
                    if (!UsbAudioDeviceTypeListValue.Contains("Playback") && !UsbAudioDeviceTypeListValue.Contains("Recording"))
                    {
                        UsbAudioDeviceTypeListValue.Add("Playback");
                        UsbAudioDeviceTypeListValue.Add("Recording");
                    }
                
                return UsbAudioDeviceTypeListValue;
            }
            set
            {
                UsbAudioDeviceDisplay.Clear();
                UsbAudioDeviceTypeListValue = value;
                OnPropertyChanged("UsbAudioDeviceTypeList");
            }
        }



        private bool usbAudioBridgeDeviceComboEnable = false;
        public bool UsbAudioBridgeDeviceComboEnable
        {
            get
            {
                return usbAudioBridgeDeviceComboEnable;
            }
            set
            {
                usbAudioBridgeDeviceComboEnable = value;
                OnPropertyChanged("UsbAudioBridgeDeviceComboEnable");
            }
        }


        private string UsbAudioTypeSelectedValue = null;
        public string UsbAudioTypeSelectedItem
        {
            get { return UsbAudioTypeSelectedValue; }
            set
            {
                UsbAudioDeviceDisplay.Clear();
                UsbDefaultDevicesOption.Clear();
                UsbAudioTypeSelectedValue = value;
                if (value != string.Empty)
                {
                    UsbAudioBridgeDeviceComboEnable = true;
                }
                else
                {
                    UsbAudioBridgeDeviceComboEnable = false;
                }
                OnPropertyChanged("UsbAudioTypeSelectedItem");
                OnPropertyChanged("UsbAudioDeviceDisplay");
            }
        }

        private string UsbAudioBridgeTypeSelectedValue =null;
        public string UsbAudioBridgeTypeSelectedItem
        {
            get { return UsbAudioBridgeTypeSelectedValue; }
            set
            {
                UsbAudioDeviceDisplay.Clear();
                UsbDefaultDevicesOption.Clear();
                UsbAudioBridgeTypeSelectedValue = value;
             
                OnPropertyChanged("UsbAudioBridgeTypeSelectedItem");
                OnPropertyChanged("UsbAudioTypeSelectedItem");
                OnPropertyChanged("UsbAudioDeviceDisplay");

            }
        }



        private string UsbAudioDeviceSelectedValue = null;
        public string UsbAudioDeviceSelectedItem
        {
            get { return UsbAudioDeviceSelectedValue; }
            set
            {
                UsbAudioDeviceSelectedValue = value;
               
                OnPropertyChanged("UsbAudioDeviceSelectedItem");
                OnPropertyChanged("UsbDefaultDevicesOption");

            }
        }

        private List<Tuple<string, string>> UsbAudioDeviceListValue = new List<Tuple<string, string>>();
        public List<Tuple<string, string>> UsbAudioDeviceList
        {
            get { return UsbAudioDeviceListValue; }
            set
            {
                try
                {
                    UsbAudioDeviceListValue = value;
                    OnPropertyChanged("UsbAudioDeviceList");
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

        private ObservableCollection<string> BridgeValue = new ObservableCollection<string>();
        public ObservableCollection<string> BridgeList
        {
            get
            {

               
               // BridgeList.r
                return BridgeValue;
            }
            set
            {
              
                BridgeValue = value;
              
               
                OnPropertyChanged("BridgeList");
                OnPropertyChanged("UsbAudioDeviceTypeList");
                OnPropertyChanged("UsbAudioDeviceDisplay");
            }
        }

        private List<Tuple<string, string>> UsbAudioBridgeListValue = new List<Tuple<string, string>>();
        public List<Tuple<string, string>> UsbAudioBridgeList
        {
            get
            {
              
                return UsbAudioBridgeListValue;
            }
            set
            {
                try
                {
                    BridgeList.Clear();
                    UsbAudioBridgeListValue = value;
                    foreach(Tuple<string,string> brdname in UsbAudioBridgeListValue)
                    {
                        if(!BridgeList.Contains(brdname.Item2))
                            BridgeList.Add(brdname.Item2);
                    }
                
                    string[] alphaNumericSortedBridgeNames = BridgeList.ToArray();
                    Array.Sort(alphaNumericSortedBridgeNames, new AlphanumComparatorFaster());
                    BridgeList = new ObservableCollection<string>(alphaNumericSortedBridgeNames.ToList());


                    OnPropertyChanged("UsbAudioBridgeList");
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

     

        private ObservableCollection<string> UsbAudioDeviceDisplayValue = new ObservableCollection<string>();
        public ObservableCollection<string> UsbAudioDeviceDisplay
        {
            get {
              
                if (UsbAudioTypeSelectedItem == "Playback")
                {
                    foreach (Tuple<string, string> dictt in UsbAudioBridgeList)
                    {
                        if (dictt.Item2 == UsbAudioBridgeTypeSelectedItem)
                        {
                            foreach (Tuple<string, string> dict in UsbAudioDeviceList)
                            {

                                if (dict.Item1.ToString() == dictt.Item1.ToString() && dict.Item2 == "Playback")
                                {
                                    if (!UsbAudioDeviceDisplayValue.Contains(dict.Item1))
                                        UsbAudioDeviceDisplayValue.Add(dict.Item1);
                                }
                            }
                        }
                            
                    }
                }

                if (UsbAudioTypeSelectedItem == "Recording")
                {
                    foreach (Tuple<string, string> dictt in UsbAudioBridgeList)
                    {
                        if(dictt.Item2== UsbAudioBridgeTypeSelectedItem)
                        {
                            foreach (Tuple<string, string> dict in UsbAudioDeviceList)
                            {
                                if (dict.Item1.ToString() == dictt.Item1.ToString() &&  dict.Item2 == "Recording")
                                {
                                    if (!UsbAudioDeviceDisplayValue.Contains(dict.Item1))
                                        UsbAudioDeviceDisplayValue.Add(dict.Item1);
                                }
                            }
                        }
                      
                    }
                }
                return UsbAudioDeviceDisplayValue;

            }
            set
            {
                UsbAudioDeviceDisplayValue = value;
                OnPropertyChanged("UsbAudioDeviceDisplay");
            }
        }


        private ObservableCollection<string> UsbDefaultDevicesOptionValue = new ObservableCollection<string>();
        public ObservableCollection<string> UsbDefaultDevicesOption
        {
            get
            {

                if (!UsbDefaultDevicesOptionValue.Contains("Multimedia") && !UsbDefaultDevicesOptionValue.Contains("Communication") && !UsbDefaultDevicesOptionValue.Contains("Both"))
                {
                    UsbDefaultDevicesOptionValue.Add("Default device");
                    UsbDefaultDevicesOptionValue.Add("Default Communication");
                    UsbDefaultDevicesOptionValue.Add("Both");
                }

                return UsbDefaultDevicesOptionValue;
            }
            set
            {
                //UsbAudioDeviceDisplay.Clear();
                UsbDefaultDevicesOptionValue = value;
                OnPropertyChanged("UsbDefaultDevicesOption");
            }
        }

        private string UsbDefaultDeviceOptionSelectedValue = null;
        public string UsbDefaultDeviceOptionSelectedItem
        {
            get { return UsbDefaultDeviceOptionSelectedValue; }
            set
            {
                UsbDefaultDeviceOptionSelectedValue = value;

                OnPropertyChanged("UsbDefaultDeviceOptionSelectedItem");

            }
        }


    }
    public class TestLuaItem : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        void OnPropertyChanged(string property)
        {
            try
            {
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs(property));

                    if (ParentTestActionItem != null && ParentTestActionItem.ParentTestCaseItem != null && ParentTestActionItem.ParentTestCaseItem.SaveButtonIsEnabled != true)
                        ParentTestActionItem.ParentTestCaseItem.SaveButtonIsEnabled = true;
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

        private TestActionItem parentTestActionItemValue = null;
        public TestActionItem ParentTestActionItem
        {
            get { return parentTestActionItemValue; }
            set { parentTestActionItemValue = value; OnPropertyChanged("ParentTestActionItem"); }
        }
    }

    public class TestLogItem : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        void OnPropertyChanged(string property)
        {
            try
            {
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs(property));

                    if (ParentTestActionItem != null && ParentTestActionItem.ParentTestCaseItem != null && ParentTestActionItem.ParentTestCaseItem.SaveButtonIsEnabled != true)
                        ParentTestActionItem.ParentTestCaseItem.SaveButtonIsEnabled = true;
                }
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                // MessageBox.Show("Exception\n " + ex.Message, "Error Code - ECxxxxx", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private TestActionItem parentTestActionItemValue = null;
        public TestActionItem ParentTestActionItem
        {
            get { return parentTestActionItemValue; }
            set { parentTestActionItemValue = value; OnPropertyChanged("ParentTestActionItem"); }
        }

      /////  Itemsource
        private ObservableCollection<string> Log_verification_kernellog_value = new ObservableCollection<string> ();
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

        public Visibility PcaplabeltextvisibilityValue = Visibility.Hidden;
        public Visibility Pcaplabeltextvisibility
        {
            get { return PcaplabeltextvisibilityValue; }
            set
            {
                PcaplabeltextvisibilityValue = value;                
                OnPropertyChanged("Pcaplabeltextvisibility");
            }
        }
      
        private bool PcapLogIsCheckedValue = false;
        public bool PcapLogIsChecked
        {
            get { return PcapLogIsCheckedValue; }
            set
            {
                PcapLogIsCheckedValue = value;
                if (value == true)
                {
                    PcapCaptureTimeVisibility = true;
                    PcapCaptureUnitVisibility = true;
                    if (ParentTestActionItem.VerifyTestLogList[0].SetTestPcapList.Count == 0)
                        AddPcapItem();       
                    
                    if(ParentTestActionItem.VerifyTestLogList[0].SetTestPcapList.Count >0)
                    {
                        foreach(PcapItem item in ParentTestActionItem.VerifyTestLogList[0].SetTestPcapList)
                        {
                            item.PcapProtocolNameTxtIsEnabled = true;
                        }
                    }
                }
                else
                {
                    PcapCaptureTimeVisibility = false;
                    PcapCaptureUnitVisibility = false;
                    if (ParentTestActionItem.VerifyTestLogList[0].SetTestPcapList.Count > 0)
                    {
                        foreach (PcapItem item in ParentTestActionItem.VerifyTestLogList[0].SetTestPcapList)
                        {
                            item.PcapProtocolNameTxtIsEnabled = false;
                        }
                    }
                }             
                if (ParentTestActionItem.ParentTestCaseItem.IsEditModeEnabled && value == true)
                {
                    SelectPcapPlusButtonVisibility = Visibility.Visible;
                    Pcaplabeltextvisibility = Visibility.Visible;
                }
                else
                {
                    SelectPcapPlusButtonVisibility = Visibility.Collapsed;
                    Pcaplabeltextvisibility = Visibility.Hidden;
                }

                OnPropertyChanged("PcapLogIsChecked");
            }
        }

        private bool PcapCaptureTimeVisibilityValue = false;
        public bool PcapCaptureTimeVisibility
        {
            get { return PcapCaptureTimeVisibilityValue; }
            set { PcapCaptureTimeVisibilityValue = value; OnPropertyChanged("PcapCaptureTimeVisibility"); }
        }

        private bool PcapCaptureUnitVisibilityValue = false;
        public bool PcapCaptureUnitVisibility
        {
            get { return PcapCaptureUnitVisibilityValue; }
            set { PcapCaptureUnitVisibilityValue = value; OnPropertyChanged("PcapCaptureUnitVisibility"); }
        }

        private string PcaplogDelaySettingValue = "3";
        public string PcaplogDelaySetting
        {
            get { return PcaplogDelaySettingValue; }
            set { PcaplogDelaySettingValue = value; OnPropertyChanged("PcaplogDelaySetting"); }
        }
        
        private List<string> PcapdelayCombo_value = new List<string>() { "Hour", "Min", "Sec", "msec" };
        public List<string> PcapdelayCombo
        {
            get { return PcapdelayCombo_value; }
            set { PcapdelayCombo_value = value; OnPropertyChanged("PcapdelayCombo"); }
        }

        private string PcapDelayUnitSelectedValue = "Sec";
        public string PcapDelayUnitSelected
        {
            get { return PcapDelayUnitSelectedValue; }
            set { PcapDelayUnitSelectedValue = value; OnPropertyChanged("PcapDelayUnitSelected"); }
        }

        private List<string> PcapSelectLanCombovalue = new List<string>() { "LAN-A", "LAN-B", "AUX-A", "AUX-B" };
        public List<string> PcapSelectLanCombo
        {
            get { return PcapSelectLanCombovalue; }
            set { PcapSelectLanCombovalue = value; OnPropertyChanged("PcapSelectLanCombo"); }
        }

        private string PcapSelectLanComboSelecteditemValue = "LAN-A";
        public string PcapSelectLanComboSelecteditem
        {
            get { return PcapSelectLanComboSelecteditemValue; }
            set { PcapSelectLanComboSelecteditemValue = value; OnPropertyChanged("PcapSelectLanComboSelecteditem"); }
        }
        
        private List<string> PcapSelectFilterCombovalue = new List<string>() { "ALL", "VOIP", "SIP", "RTP", "PTP", "QDP", "HTTP", "UDP", "TCP" };
        public List<string> PcapSelectFilterCombo
        {
            get { return PcapSelectFilterCombovalue; }
            set { PcapSelectFilterCombovalue = value; OnPropertyChanged("PcapSelectFilterCombo"); }
        }

        private string PcapSelectFilterComboSelecteditemValue = "ALL";
        public string PcapSelectFilterComboSelecteditem
        {
            get { return PcapSelectFilterComboSelecteditemValue; }
            set { PcapSelectFilterComboSelecteditemValue = value; OnPropertyChanged("PcapSelectFilterComboSelecteditem"); }
        }

        private string PcapFilterByIPValue = string.Empty;
        public string PcapFilterByIP
        {
            get { return PcapFilterByIPValue; }
            set { PcapFilterByIPValue = value; OnPropertyChanged("PcapFilterByIP"); }
        }

        private bool PcapNotFilterByIPValue = false;
        public bool PcapNotFilterByIP
        {
            get { return PcapNotFilterByIPValue; }
            set { PcapNotFilterByIPValue = value; OnPropertyChanged("PcapNotFilterByIP"); }
        }

        private ObservableCollection<PcapItem> SetTestPcapListValue = new ObservableCollection<PcapItem>();
        public ObservableCollection<PcapItem> SetTestPcapList
        {
            get { return SetTestPcapListValue; }
            set { SetTestPcapListValue = value; OnPropertyChanged("SetTestPcapList"); }
        }

        private Visibility SelectPcapPlusButtonVisibilityValue = Visibility.Collapsed;
        public Visibility SelectPcapPlusButtonVisibility
        {
            get { return SelectPcapPlusButtonVisibilityValue; }
            set { SelectPcapPlusButtonVisibilityValue = value; OnPropertyChanged("SelectPcapPlusButtonVisibility"); }
        }

        public PcapItem AddPcapItem()
        {
            PcapItem setTestTelnetItem = new PcapItem();

            try
            {
                setTestTelnetItem.ParentTestLogItem = this;

                SetTestPcapList.Add(setTestTelnetItem);
           
                return setTestTelnetItem;
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                //MessageBox.Show("Exception\n " + ex.Message, "QAT Error Code - EC03021", MessageBoxButton.OK, MessageBoxImage.Error);
                return setTestTelnetItem;
            }
        }

        public PcapItem AddPcapItem(PcapItem sourcePcapItem)
        {
            PcapItem verifyPcapItem = CopyPcapItem(sourcePcapItem);
            SetTestPcapList.Insert(SetTestPcapList.IndexOf(sourcePcapItem), verifyPcapItem);
			 return verifyPcapItem;
        }

        public PcapItem CopyPcapItem(PcapItem sourcePcapItem)
        {
            PcapItem targetPcapItem = new PcapItem();

            try
            {
                targetPcapItem.ParentTestLogItem = sourcePcapItem.ParentTestLogItem;
                targetPcapItem.PcapFieldText = sourcePcapItem.PcapFieldText;
                targetPcapItem.PcapProtocolName = sourcePcapItem.PcapProtocolName;
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                //MessageBox.Show("Exception\n " + ex.Message, "QAT Error Code - EC03021", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return targetPcapItem;
        }

        public void RemovePcapItem(PcapItem removeItem)
        {
            try
            {
                if (SetTestPcapList.Contains(removeItem))
                    SetTestPcapList.Remove(removeItem);
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




    }

    public class PcapItem: INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        void OnPropertyChanged(string property)
        {
            try
            {
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs(property));

                    if (ParentTestLogItem != null && ParentTestLogItem.ParentTestActionItem.ParentTestCaseItem != null && ParentTestLogItem.ParentTestActionItem.ParentTestCaseItem.SaveButtonIsEnabled != true)
                        ParentTestLogItem.ParentTestActionItem.ParentTestCaseItem.SaveButtonIsEnabled = true;
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

        private TestLogItem ParentTestLogItemValue = null;
        public TestLogItem ParentTestLogItem
        {
            get { return ParentTestLogItemValue; }
            set { ParentTestLogItemValue = value; OnPropertyChanged("ParentTestLogItem"); }
        }

        private string ProtocolNameValue = null;
        public string PcapProtocolName
        {
            get { return ProtocolNameValue; }
            set { ProtocolNameValue = value; OnPropertyChanged("PcapProtocolName"); }
        }

        private string PcapFieldTextValue = null;
        public string PcapFieldText
        {
            get { return PcapFieldTextValue; }
            set { PcapFieldTextValue = value; OnPropertyChanged("PcapFieldText"); }
        }
        public bool PcapProtocolNameTxtIsEnabledValue = true;
        public bool PcapProtocolNameTxtIsEnabled
        {
            get { return PcapProtocolNameTxtIsEnabledValue; }
            set
            {
                PcapProtocolNameTxtIsEnabledValue = value;
                OnPropertyChanged("PcapProtocolNameTxtIsEnabled");
            }
        }
    }

    public class TestResponsalyzerItem : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        void OnPropertyChanged(string property)
        {
            try
            {
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs(property));

                    if (ParentTestActionItem != null && ParentTestActionItem.ParentTestCaseItem != null && ParentTestActionItem.ParentTestCaseItem.SaveButtonIsEnabled != true)
                        ParentTestActionItem.ParentTestCaseItem.SaveButtonIsEnabled = true;
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

        private TestActionItem parentTestActionItemValue = null;
        public TestActionItem ParentTestActionItem
        {
            get { return parentTestActionItemValue; }
            set { parentTestActionItemValue = value; OnPropertyChanged("ParentTestActionItem"); }
        }

        private ObservableCollection<string> testResponsalyzerNameListValue = new ObservableCollection<string>();
        public ObservableCollection<string> TestResponsalyzerNameList
        {
            get { return testResponsalyzerNameListValue; }
            set
            {
                testResponsalyzerNameListValue = value;

                //if (value == null || !value.Contains(TestResponsalyzerNameSelectedItem))
                //    TestResponsalyzerNameSelectedItem = null;

                //if (value != null)
                //{

                //    string[] alphaNumericSortedResponsalyzerName = ParentTestActionItem.ParentTestCaseItem.ResponsalyzerNameList.ToArray();
                //    Array.Sort(alphaNumericSortedResponsalyzerName, new AlphanumComparatorFaster());
                //    ObservableCollection<string> ResponsalyzerNameList = new ObservableCollection<string>(alphaNumericSortedResponsalyzerName.ToList());
                //    TestResponsalyzerNameList = ResponsalyzerNameList;
                //}
                string[] alphaNumericSortedComponentName = testResponsalyzerNameListValue.ToArray();
                Array.Sort(alphaNumericSortedComponentName, new AlphanumComparatorFaster());
                testResponsalyzerNameListValue = new ObservableCollection<string>(alphaNumericSortedComponentName.ToList());
                OnPropertyChanged("TestResponsalyzerNameList");
            }
        }

        private string testResponsalyzerNameSelectedItemValue = null;
        public string TestResponsalyzerNameSelectedItem
        {
            get { return testResponsalyzerNameSelectedItemValue; }
            set
            {
                try
                {
                    testResponsalyzerNameSelectedItemValue = value;                 
                    OnPropertyChanged("TestResponsalyzerNameSelectedItem");
                }
                catch (Exception ex)
                {
                    DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                    // MessageBox.Show("Exception\n " + ex.Message, "Error Code - ECxxxxx", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private ObservableCollection<string> testResponsalyzerTypeListValue = new ObservableCollection<string>();
        public ObservableCollection<string> TestResponsalyzerTypeList
        {
            get { return testResponsalyzerTypeListValue; }
            set
            {
                    testResponsalyzerTypeListValue = value;

                //if (value == null || !value.Contains(TestResponsalyzerTypeSelectedItem))
                //    TestResponsalyzerTypeSelectedItem = null;

                //if (value != null)
                //{

                //    string[] alphaNumericSortedResponsalyzerType = ParentTestActionItem.ParentTestCaseItem.ResponsalyzerTypeList.ToArray();
                //    Array.Sort(alphaNumericSortedResponsalyzerType, new AlphanumComparatorFaster());
                //    ObservableCollection<string> ResponsalyzerTypeList = new ObservableCollection<string>(alphaNumericSortedResponsalyzerType.ToList());
                //    TestResponsalyzerTypeList = ResponsalyzerTypeList;
                //}

                OnPropertyChanged("TestResponsalyzerTypeList");
            }
        }

        private string testResponsalyzerTypeSelectedItemValue = null;
        public string TestResponsalyzerTypeSelectedItem
        {
            get { return testResponsalyzerTypeSelectedItemValue; }
            set
            {
                try
                {
                    testResponsalyzerTypeSelectedItemValue = value;                 
                    OnPropertyChanged("TestResponsalyzerTypeSelectedItem");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Exception\n " + ex.Message, "Error Code - ECxxxxx", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private bool isRefFileNew = false;
        public bool IsNewReferenceFile
        {
            get { return isRefFileNew; }
            set { isRefFileNew = value; OnPropertyChanged("IsNewReferenceFile"); }
        }

        private bool copyItemSource = false;
        public bool CopyItemSource
        {
            get { return copyItemSource; }
            set { copyItemSource = value; OnPropertyChanged("CopyItemSource"); }
        }

        private string testResponsalyzerVerificationFileValue = null;
        public string TestResponsalyzerVerificationFile
        {
            get
            {

                return testResponsalyzerVerificationFileValue;
            }
            set
            {
                testResponsalyzerVerificationFileValue = value;


                OnPropertyChanged("TestResponsalyzerVerificationFile");
            }
        }
    }

    public class TestApxItem : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        void OnPropertyChanged(string property)
        {
            try
            {
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs(property));

                    if (ParentTestActionItem != null && ParentTestActionItem.ParentTestCaseItem != null && ParentTestActionItem.ParentTestCaseItem.SaveButtonIsEnabled != true)
                        ParentTestActionItem.ParentTestCaseItem.SaveButtonIsEnabled = true;
                }
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                // MessageBox.Show("Exception\n " + ex.Message, "Error Code - ECxxxxx", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public TestApxItem()
        {
            AddAPXClockModeSettingsVM();
            AddAPXLevelAndGainSettingsVM();
            AddAPXFreqResponseSettings();
            AddAPXInterChPhaseSettings();
            AddAPXSteppedFreqSweepSettings();
			AddAPXTHDNSettings();
            AddAPXClockModeInitialSettings();
            AddAPXLevelAndGainInitialSettings();
            AddAPXFreqResponseInitialSettings();
            AddAPXInterChPhaseInitialSettings();
            AddAPXSteppedFreqSweepInitialSettings();
			AddAPXInitialTHDNSettings();
        }

        private string APxBrowseLocationValue = null;
        public string APxBrowseLocation
        {
            get { return APxBrowseLocationValue; }
            set
            {
                APxBrowseLocationValue = value;
                OnPropertyChanged("APxBrowseLocation");
            }
        }

        private string apxLocationTimeStampValue = null;
        public string APxLocationTimeStamp
        {
            get { return apxLocationTimeStampValue; }
            set
            {
                apxLocationTimeStampValue = value;
                OnPropertyChanged("APxLocationTimeStamp");
            }
        }

        private bool isAPXFileLoadedValue = false;
        public bool isAPXFileLoaded
        {
            get { return isAPXFileLoadedValue; }
            set
            {
                isAPXFileLoadedValue = value;
                OnPropertyChanged("isAPXFileLoaded");
            }
        }

        private string cmbTypeOfVerficationValue = null;
        public string cmbTypeOfVerfication
        {
            get { return cmbTypeOfVerficationValue; }
            set
            {
                cmbTypeOfVerficationValue = value;
                OnPropertyChanged("cmbTypeOfVerfication");

                if (cmbTypeOfVerfication == "Level and Gain")
                {
                    GainGridVisibility = Visibility.Visible;
                    FreqResponseGridVisibility = Visibility.Collapsed;
                    SteppedFreqSweepGridVisibility = Visibility.Collapsed;
                    InterChPhaseGridVisibility = Visibility.Collapsed;
                    THDNGridVisibility = Visibility.Collapsed;
                }
                else if (cmbTypeOfVerfication == "Frequency sweep")
                {
                    GainGridVisibility = Visibility.Collapsed;
                    FreqResponseGridVisibility = Visibility.Visible;
                    SteppedFreqSweepGridVisibility = Visibility.Collapsed;
                    InterChPhaseGridVisibility = Visibility.Collapsed;
                    THDNGridVisibility = Visibility.Collapsed;
                }
                else if (cmbTypeOfVerfication == "Stepped Frequency Sweep")
                {
                    GainGridVisibility = Visibility.Collapsed;
                    FreqResponseGridVisibility = Visibility.Collapsed;
                    SteppedFreqSweepGridVisibility = Visibility.Visible;
                    InterChPhaseGridVisibility = Visibility.Collapsed;
					THDNGridVisibility = Visibility.Collapsed;
                }
                else if (cmbTypeOfVerfication == "Phase")
                {
                    GainGridVisibility = Visibility.Collapsed;
                    FreqResponseGridVisibility = Visibility.Collapsed;
                    SteppedFreqSweepGridVisibility = Visibility.Collapsed;
                    InterChPhaseGridVisibility = Visibility.Visible;
                    THDNGridVisibility = Visibility.Collapsed;
                }
				else if(cmbTypeOfVerfication == "THD+N")
                {
                    GainGridVisibility = Visibility.Collapsed;
                    FreqResponseGridVisibility = Visibility.Collapsed;
                    SteppedFreqSweepGridVisibility = Visibility.Collapsed;
                    InterChPhaseGridVisibility = Visibility.Collapsed;
                    THDNGridVisibility = Visibility.Visible;
                }
                else
                {
                    GainGridVisibility = Visibility.Collapsed;
                    FreqResponseGridVisibility = Visibility.Collapsed;
                    SteppedFreqSweepGridVisibility = Visibility.Collapsed;
                    InterChPhaseGridVisibility = Visibility.Collapsed;
                    THDNGridVisibility = Visibility.Collapsed;
                }
            }
        }

        private Visibility GainGridVisibilityValue = Visibility.Collapsed;
        public Visibility GainGridVisibility
        {
            get { return GainGridVisibilityValue; }
            set
            {
                GainGridVisibilityValue = value;
                OnPropertyChanged("GainGridVisibility");
            }
        }

        private Visibility FreqResponseGridVisibilityValue = Visibility.Collapsed;
        public Visibility FreqResponseGridVisibility
        {
            get { return FreqResponseGridVisibilityValue; }
            set
            {
                FreqResponseGridVisibilityValue = value;
                OnPropertyChanged("FreqResponseGridVisibility");
            }
        }

        private Visibility SteppedFreqSweepGridVisibilityValue = Visibility.Collapsed;
        public Visibility SteppedFreqSweepGridVisibility
        {
            get { return SteppedFreqSweepGridVisibilityValue; }
            set
            {
                SteppedFreqSweepGridVisibilityValue = value;
                OnPropertyChanged("SteppedFreqSweepGridVisibility");
            }
        }

        private Visibility InterChPhaseGridVisibilityValue = Visibility.Collapsed;
        public Visibility InterChPhaseGridVisibility
        {
            get { return InterChPhaseGridVisibilityValue; }
            set
            {
                InterChPhaseGridVisibilityValue = value;
                OnPropertyChanged("InterChPhaseGridVisibility");
            }
        }
        
        private Visibility THDNGridVisibilityValue = Visibility.Collapsed;
        public Visibility THDNGridVisibility
        {
            get { return THDNGridVisibilityValue; }
            set
            {
                THDNGridVisibilityValue = value;
                OnPropertyChanged("THDNGridVisibility");
            }
        }
        
        private ObservableCollection<APXClockModeSettings> APxSettingsListValue = new ObservableCollection<APXClockModeSettings>();
        public ObservableCollection<APXClockModeSettings> APxSettingsList
        {
            get { return APxSettingsListValue; }
            set
            {
                APxSettingsListValue = value;
                OnPropertyChanged("APxSettingsList");
            }
        }

        private ObservableCollection<APXLevelandGain_Verification> APxLevelAndGainListValue = new ObservableCollection<APXLevelandGain_Verification>();
        public ObservableCollection<APXLevelandGain_Verification> APxLevelAndGainList
        {
            get { return APxLevelAndGainListValue; }
            set
            {
                APxLevelAndGainListValue = value;
                OnPropertyChanged("APxLevelAndGainList");
            }
        }

        private ObservableCollection<APXFreqResponseVerification> APxFreqResponseListValue = new ObservableCollection<APXFreqResponseVerification>();
        public ObservableCollection<APXFreqResponseVerification> APxFreqResponseList
        {
            get { return APxFreqResponseListValue; }
            set
            {
                APxFreqResponseListValue = value;
                OnPropertyChanged(" APxFreqResponseList");
            }
        }

        private ObservableCollection<APXSteppedFreqSweepVerification> APxSteppedFreqSweepListValue = new ObservableCollection<APXSteppedFreqSweepVerification>();
        public ObservableCollection<APXSteppedFreqSweepVerification> APxSteppedFreqSweepList
        {
            get { return APxSteppedFreqSweepListValue; }
            set
            {
                APxSteppedFreqSweepListValue = value;
                OnPropertyChanged(" APxSteppedFreqSweepList");
            }
        }

        private ObservableCollection<APXSteppedFreqSweepVerification> APxInitialSteppedFreqSweepListValue = new ObservableCollection<APXSteppedFreqSweepVerification>();
        public ObservableCollection<APXSteppedFreqSweepVerification> APxInitialSteppedFreqSweepList
        {
            get { return APxInitialSteppedFreqSweepListValue; }
            set
            {
                APxInitialSteppedFreqSweepListValue = value;
                OnPropertyChanged(" APxInitialSteppedFreqSweepList");
            }
        }

        private ObservableCollection<APXInterChannelPhaseVerification> APxInterChPhaseListValue = new ObservableCollection<APXInterChannelPhaseVerification>();
        public ObservableCollection<APXInterChannelPhaseVerification> APxInterChPhaseList
        {
            get { return APxInterChPhaseListValue; }
            set
            {
                APxInterChPhaseListValue = value;
                OnPropertyChanged(" APxInterChPhaseList");
            }
        }
        
        private ObservableCollection<APXTHDNVerification> APxTHDNListValue = new ObservableCollection<APXTHDNVerification>();
        public ObservableCollection<APXTHDNVerification> APxTHDNList
        {
            get { return APxTHDNListValue; }
            set
            {
                APxTHDNListValue = value;
                OnPropertyChanged("APxTHDNList");
            }
        }

        private ObservableCollection<APXClockModeSettings> APxInitialSettingsValue = new ObservableCollection<APXClockModeSettings>();
        public ObservableCollection<APXClockModeSettings> APxInitialSettingsList
        {
            get { return APxInitialSettingsValue; }
            set
            {
                APxInitialSettingsValue = value;
                OnPropertyChanged("APxInitialSettings");
            }
        }

        private ObservableCollection<APXLevelandGain_Verification> APxInitialLevelAndGainListValue = new ObservableCollection<APXLevelandGain_Verification>();
        public ObservableCollection<APXLevelandGain_Verification> APxInitialLevelAndGainList
        {
            get { return APxInitialLevelAndGainListValue; }
            set
            {
                APxInitialLevelAndGainListValue = value;
                OnPropertyChanged("APxInitialLevelAndGainList");
            }
        }

        private ObservableCollection<APXFreqResponseVerification> APxInitialFreqResponseListValue = new ObservableCollection<APXFreqResponseVerification>();
        public ObservableCollection<APXFreqResponseVerification> APxInitialFreqResponseList
        {
            get { return APxInitialFreqResponseListValue; }
            set
            {
                APxInitialFreqResponseListValue = value;
                OnPropertyChanged(" APxInitialFreqResponseList");
            }
        }

        private ObservableCollection<APXInterChannelPhaseVerification> APxInitialInterChPhaseListValue = new ObservableCollection<APXInterChannelPhaseVerification>();
        public ObservableCollection<APXInterChannelPhaseVerification> APxInitialInterChPhaseList
        {
            get { return APxInitialInterChPhaseListValue; }
            set
            {
                APxInitialInterChPhaseListValue = value;
                OnPropertyChanged(" APxInitialInterChPhaseList");
            }
        }

        private ObservableCollection<APXTHDNVerification> APxInitialTHDNListValue = new ObservableCollection<APXTHDNVerification>();
        public ObservableCollection<APXTHDNVerification> APxInitialTHDNList
        {
            get { return APxInitialTHDNListValue; }
            set
            {
                APxInitialTHDNListValue = value;
                OnPropertyChanged(" APxInitialTHDNList");
            }
        }

        public APXClockModeSettings AddAPXClockModeSettingsVM()
        {
            APXClockModeSettings setAPXClockModeSettingsVM = new APXClockModeSettings();
            setAPXClockModeSettingsVM.ParentTestApxItem = this;
            APxSettingsList.Add(setAPXClockModeSettingsVM);
            return setAPXClockModeSettingsVM;
        }

        public APXClockModeSettings AddAPXClockModeInitialSettings()
        {
            APXClockModeSettings setAPXClockModeSettingsVM1 = new APXClockModeSettings();
            setAPXClockModeSettingsVM1.ParentTestApxItem = this;
            APxInitialSettingsList.Add(setAPXClockModeSettingsVM1);
            return setAPXClockModeSettingsVM1;
        }

        public APXLevelandGain_Verification AddAPXLevelAndGainSettingsVM()
        {
            APXLevelandGain_Verification setAPXLevelaAndGainSettingsVM = new APXLevelandGain_Verification();
            setAPXLevelaAndGainSettingsVM.ParentTestApxItem = this;
            APxLevelAndGainList.Add(setAPXLevelaAndGainSettingsVM);
            return setAPXLevelaAndGainSettingsVM;
        }

        public APXLevelandGain_Verification AddAPXLevelAndGainInitialSettings()
        {
            APXLevelandGain_Verification setAPXLevelaAndGainInitialSettings = new APXLevelandGain_Verification();
            setAPXLevelaAndGainInitialSettings.ParentTestApxItem = this;
            APxInitialLevelAndGainList.Add(setAPXLevelaAndGainInitialSettings);
            return setAPXLevelaAndGainInitialSettings;
        }

        public APXFreqResponseVerification AddAPXFreqResponseSettings()
        {
            APXFreqResponseVerification setAPXFreqRespSettings = new APXFreqResponseVerification();
            setAPXFreqRespSettings.ParentTestApxItem = this;
            APxFreqResponseList.Add(setAPXFreqRespSettings);
            return setAPXFreqRespSettings;
        }

        public APXFreqResponseVerification AddAPXFreqResponseInitialSettings()
        {
            APXFreqResponseVerification setAPXFreqRespInitialSettings = new APXFreqResponseVerification();
            setAPXFreqRespInitialSettings.ParentTestApxItem = this;
            APxInitialFreqResponseList.Add(setAPXFreqRespInitialSettings);
            return setAPXFreqRespInitialSettings;
        }

        public APXSteppedFreqSweepVerification AddAPXSteppedFreqSweepSettings()
        {
            APXSteppedFreqSweepVerification setAPXSteppedFreqSettings = new APXSteppedFreqSweepVerification();
            setAPXSteppedFreqSettings.ParentTestApxItem = this;
            APxSteppedFreqSweepList.Add(setAPXSteppedFreqSettings);
            return setAPXSteppedFreqSettings;
        }

        public APXSteppedFreqSweepVerification AddAPXSteppedFreqSweepInitialSettings()
        {
            APXSteppedFreqSweepVerification setAPXInitialSteppedFreqSettings = new APXSteppedFreqSweepVerification();
            setAPXInitialSteppedFreqSettings.ParentTestApxItem = this;
            APxInitialSteppedFreqSweepList.Add(setAPXInitialSteppedFreqSettings);
            return setAPXInitialSteppedFreqSettings;
        }
        
        public APXInterChannelPhaseVerification AddAPXInterChPhaseInitialSettings()
        {
            APXInterChannelPhaseVerification setAPXInitialPhaseSettings = new APXInterChannelPhaseVerification();
            setAPXInitialPhaseSettings.ParentTestApxItem = this;
            APxInitialInterChPhaseList.Add(setAPXInitialPhaseSettings);
            return setAPXInitialPhaseSettings;
        }
        
        public APXInterChannelPhaseVerification AddAPXInterChPhaseSettings()
        {
            APXInterChannelPhaseVerification setAPXPhaseSettings = new APXInterChannelPhaseVerification();
            setAPXPhaseSettings.ParentTestApxItem = this;
            APxInterChPhaseList.Add(setAPXPhaseSettings);
            return setAPXPhaseSettings;
        }

        public APXTHDNVerification AddAPXTHDNSettings()
        {
            APXTHDNVerification setAPXTHDNSettings = new APXTHDNVerification();
            setAPXTHDNSettings.ParentTestApxItem = this;
            APxTHDNList.Add(setAPXTHDNSettings);
            return setAPXTHDNSettings;
        }
        
        public APXTHDNVerification AddAPXInitialTHDNSettings()
        {
            APXTHDNVerification setAPXTHDNSettings = new APXTHDNVerification();
            setAPXTHDNSettings.ParentTestApxItem = this;
            APxInitialTHDNList.Add(setAPXTHDNSettings);
            return setAPXTHDNSettings;
        }

        private Visibility ApSettingsVisibilityValue = Visibility.Collapsed;
        public Visibility ApSettingsVisibility
        {
            get { return ApSettingsVisibilityValue; }
            set { ApSettingsVisibilityValue = value; OnPropertyChanged("ApSettingsVisibility"); }
        }

        private Visibility ApVerifyVisibilityValue = Visibility.Collapsed;
        public Visibility ApVerifyVisibility
        {
            get { return ApVerifyVisibilityValue; }
            set { ApVerifyVisibilityValue = value; OnPropertyChanged("ApVerifyVisibility"); }
        }

        private TestActionItem parentTestActionItemValue = null;
        public TestActionItem ParentTestActionItem
        {
            get { return parentTestActionItemValue; }
            set { parentTestActionItemValue = value; OnPropertyChanged("ParentTestActionItem"); }
        }
    }

    public class TestSaveLogItem : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        void OnPropertyChanged(string property)
        {
            try
            {
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs(property));

                    if (ParentTestActionItem != null && ParentTestActionItem.ParentTestCaseItem != null && ParentTestActionItem.ParentTestCaseItem.SaveButtonIsEnabled != true)
                        ParentTestActionItem.ParentTestCaseItem.SaveButtonIsEnabled = true;
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

        private TestActionItem parentTestActionItemValue = null;
        public TestActionItem ParentTestActionItem
        {
            get { return parentTestActionItemValue; }
            set { parentTestActionItemValue = value; OnPropertyChanged("ParentTestActionItem"); }
        }

        private bool logGridIsEnabledValue = true;
        public bool LogGridIsEnabled
        {
            get { return logGridIsEnabledValue; }
            set { logGridIsEnabledValue = value; OnPropertyChanged("LogGridIsEnabled"); }
        }

        private bool ScreenShotIsEnableValue = true;
        public bool ScreenShotIsEnable
        {
            get { return ScreenShotIsEnableValue; }
            set { ScreenShotIsEnableValue = value; OnPropertyChanged("ScreenShotIsEnable"); }
        }
        private string actionSaveLogEventSelectedValue = "Save during Error";
        public string ActionSaveLogEventSelected
        {
            get { return actionSaveLogEventSelectedValue; }
            set
            {
                actionSaveLogEventSelectedValue = value;
                if (value == "Never Save logs")
                {
                    LogGridIsEnabled = false;
                    iLogDeviceItemIsEnabled = false;
                    ConfiguratorDeviceItemIsEnabled = false;
                    saveQsysylogIsenabled = false;
                }
                else
                {
                    LogGridIsEnabled = true;
                    saveQsysylogIsenabled = true;

                    if (ActionLogiLogIsChecked == true)
                        iLogDeviceItemIsEnabled = true;
                    if (ActionLogiLogIsChecked == false)
                        iLogDeviceItemIsEnabled = false;

                    if (ActionLogConfiguratorIsChecked == true)
                        ConfiguratorDeviceItemIsEnabled = true;

                    if (ActionLogConfiguratorIsChecked == false)
                        ConfiguratorDeviceItemIsEnabled = false;
                }

                OnPropertyChanged("ActionSaveLogEventSelected");
            }
        }

        private bool screenshotselectionValue = false;
        public bool screenshotselection
        {
            get { return screenshotselectionValue; }
            set
            {
                screenshotselectionValue = value;               
                OnPropertyChanged("screenshotselection");
            }
        }

        private ObservableCollection<string> actionSaveLogEventListValue = new ObservableCollection<string> { "Save during Error", "Save logs always", "Never Save logs" };
        public ObservableCollection<string> ActionSaveLogEventList
        {
            get { return actionSaveLogEventListValue; }
            set { actionSaveLogEventListValue = value; OnPropertyChanged("ActionSaveLogEventList"); }
        }

        private bool saveQsysylogPeripheralSelectionValue = false;
        public bool saveQsysylogPeripheralSelection
        {
            get { return saveQsysylogPeripheralSelectionValue; }
            set
            {
                saveQsysylogPeripheralSelectionValue = value;
                OnPropertyChanged("saveQsysylogPeripheralSelection");
            }
        }
 
        private bool saveQsysylogIsenabledValue = true;
        public bool saveQsysylogIsenabled
        {
            get { return saveQsysylogIsenabledValue; }
            set
            {
                saveQsysylogIsenabledValue = value;
                OnPropertyChanged("saveQsysylogIsenabled");
            }
        }

        private bool actionLogiLogIsCheckedValue = false;
        public bool ActionLogiLogIsChecked
        {
            get { return actionLogiLogIsCheckedValue; }
            set
            {
                actionLogiLogIsCheckedValue = value;

                if (value == true)
                    iLogDeviceItemIsEnabled = true;
                else
                    iLogDeviceItemIsEnabled = false;

                OnPropertyChanged("ActionLogiLogIsChecked");
            }
        }

        private bool actionLogConfiguratorIsCheckedValue = false;
        public bool ActionLogConfiguratorIsChecked
        {
            get { return actionLogConfiguratorIsCheckedValue; }
            set
            {
                actionLogConfiguratorIsCheckedValue = value;

                if (value == true)
                    ConfiguratorDeviceItemIsEnabled = true;
                else
                    ConfiguratorDeviceItemIsEnabled = false;

                OnPropertyChanged("ActionLogConfiguratorIsChecked");
            }
        }

        private bool actionLogEvenetLogIsCheckedValue = false;
        public bool ActionLogEvenetLogIsChecked
        {
            get { return actionLogEvenetLogIsCheckedValue; }
            set
            {
                actionLogEvenetLogIsCheckedValue = value;
                OnPropertyChanged("ActionLogEvenetLogIsChecked");
            }
        }

        private bool actionLogSipLogIsCheckedValue = false;
        public bool ActionLogSipLogIsChecked
        {
            get { return actionLogSipLogIsCheckedValue; }
            set
            {
                actionLogSipLogIsCheckedValue = value;
                OnPropertyChanged("ActionLogSipLogIsChecked");
            }
        }

        private bool actionLogQsysAppLogIsCheckedValue = false;
        public bool ActionLogQsysAppLogIsChecked
        {
            get { return actionLogQsysAppLogIsCheckedValue; }
            set
            {
                actionLogQsysAppLogIsCheckedValue = value;
                OnPropertyChanged("ActionLogQsysAppLogIsChecked");
            }
        }

        private bool actionLogSoftPhoneLogIsCheckedValue = false;
        public bool ActionLogSoftPhoneLogIsChecked
        {
            get { return actionLogSoftPhoneLogIsCheckedValue; }
            set
            {
                actionLogSoftPhoneLogIsCheckedValue = value;
                OnPropertyChanged("ActionLogSoftPhoneLogIsChecked");
            }
        }

        private bool actionLogUciViewerLogIsCheckedValue = false;
        public bool ActionLogUciViewerLogIsChecked
        {
            get { return actionLogUciViewerLogIsCheckedValue; }
            set
            {
                actionLogUciViewerLogIsCheckedValue = value;
                OnPropertyChanged("ActionLogUciViewerLogIsChecked");
            }
        }

        private bool actionLogKernelLogIsCheckedValue = false;
        public bool ActionLogKernelLogIsChecked
        {
            get { return actionLogKernelLogIsCheckedValue; }
            set
            {
                actionLogKernelLogIsCheckedValue = value;
                OnPropertyChanged("ActionLogKernelLogIsChecked");
            }
        }

        private bool actionLogWindowsEventLogIsCheckedValue = false;
        public bool ActionLogWindowsEventLogIsChecked
        {
            get { return actionLogWindowsEventLogIsCheckedValue; }
            set
            {
                actionLogWindowsEventLogIsCheckedValue = value;
                OnPropertyChanged("ActionLogWindowsEventLogIsChecked");
            }
        }

        private bool iLogDeviceItemIsEnabledValue = false;
        public bool iLogDeviceItemIsEnabled
        {
            get { return iLogDeviceItemIsEnabledValue; }
            set
            {
                iLogDeviceItemIsEnabledValue = value;
                OnPropertyChanged("iLogDeviceItemIsEnabled");
            }
        }

        private ObservableCollection<DUT_DeviceItem> iLogDeviceItemValue = new ObservableCollection<DUT_DeviceItem>();
        public ObservableCollection<DUT_DeviceItem> iLogDeviceItem
        {
            get { return iLogDeviceItemValue; }
            set { iLogDeviceItemValue = value; OnPropertyChanged("iLogDeviceItem"); }
        }

        private bool configuratorDeviceItemIsEnabledValue = false;
        public bool ConfiguratorDeviceItemIsEnabled
        {
            get { return configuratorDeviceItemIsEnabledValue; }
            set
            {
                configuratorDeviceItemIsEnabledValue = value;
                OnPropertyChanged("ConfiguratorDeviceItemIsEnabled");
            }
        }

        private ObservableCollection<DUT_DeviceItem> configuratorLogDeviceItemValue = new ObservableCollection<DUT_DeviceItem>();
        public ObservableCollection<DUT_DeviceItem> ConfiguratorLogDeviceItem
        {
            get { return configuratorLogDeviceItemValue; }
            set { configuratorLogDeviceItemValue = value; OnPropertyChanged("ConfiguratorLogDeviceItem"); }
        }
    }

    public partial class APXClockModeSettings : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        void OnPropertyChanged(string property)
        {
            try
            {
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs(property));
                    if (ParentTestApxItem != null && ParentTestApxItem.ParentTestActionItem != null && parentTestApxItemValue.ParentTestActionItem.ParentTestCaseItem != null && ParentTestApxItem.ParentTestActionItem.ParentTestCaseItem.SaveButtonIsEnabled != true)
                        ParentTestApxItem.ParentTestActionItem.ParentTestCaseItem.SaveButtonIsEnabled = true;
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

        public APXClockModeSettings()
        {
            try
            {
                if (!cmb_BenchWaveformList.Contains("Sine"))
                    cmb_BenchWaveformList.Add("Sine");
                if (!cmb_BenchWaveformList.Contains("Sine, Dual"))
                    cmb_BenchWaveformList.Add("Sine, Dual");
                if (!cmb_BenchWaveformList.Contains("Sine, Var Phase"))
                    cmb_BenchWaveformList.Add("Sine, Var Phase");
                if (!cmb_BenchWaveformList.Contains("IMD"))
                    cmb_BenchWaveformList.Add("IMD");
                if (!cmb_BenchWaveformList.Contains("Noise"))
                    cmb_BenchWaveformList.Add("Noise");
                if (!cmb_BenchWaveformList.Contains("Browse for file..."))
                    cmb_BenchWaveformList.Add("Browse for file...");

                if (!cmb_SeqWaveformList.Contains("Sine"))
                    cmb_SeqWaveformList.Add("Sine");
                if (!cmb_SeqWaveformList.Contains("Sine, Dual"))
                    cmb_SeqWaveformList.Add("Sine, Dual");
                if (!cmb_SeqWaveformList.Contains("Sine, Var Phase"))
                    cmb_SeqWaveformList.Add("Sine, Var Phase");
                if (!cmb_SeqWaveformList.Contains("Noise"))
                    cmb_SeqWaveformList.Add("Noise");
                if (!cmb_SeqWaveformList.Contains("Browse for file..."))
                    cmb_SeqWaveformList.Add("Browse for file...");
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

        private string cmbTypeOfModeValue = null;
        public string cmbTypeOfMode
        {
            get { return cmbTypeOfModeValue; }
            set
            {
                cmbTypeOfModeValue = value;
                OnPropertyChanged("cmbTypeOfMode");

                if (cmbTypeOfModeValue == "BenchMode")
                {
                    BenchModeGridVisibility = Visibility.Visible;
                    SeqModeGridVisibility = Visibility.Collapsed;
                }
                else if (cmbTypeOfModeValue == "SequenceMode")
                {
                    BenchModeGridVisibility = Visibility.Collapsed;
                    SeqModeGridVisibility = Visibility.Visible;
                }
                else
                {
                    BenchModeGridVisibility = Visibility.Collapsed;
                    SeqModeGridVisibility = Visibility.Collapsed;
                }
            }
        }

        private string BrowseWaveformValue = null;
        public string BrowseWaveform
        {
            get { return BrowseWaveformValue; }
            set
            {
                BrowseWaveformValue = value;
                OnPropertyChanged("BrowseWaveform");
            }
        }

        private Visibility BenchModeGridVisibilityValue = Visibility.Collapsed;
        public Visibility BenchModeGridVisibility
        {
            get { return BenchModeGridVisibilityValue; }
            set
            {
                BenchModeGridVisibilityValue = value;
                OnPropertyChanged("BenchModeGridVisibility");
            }
        }

        private Visibility SeqModeGridVisibilityValue = Visibility.Collapsed;
        public Visibility SeqModeGridVisibility
        {
            get { return SeqModeGridVisibilityValue; }
            set
            {
                SeqModeGridVisibilityValue = value;
                OnPropertyChanged("SeqModeGridVisibility");
            }
        }

        private string ChkBenchGenONContentValue = "Generator Off ";
        public string ChkBenchGenONContent
        {
            get { return ChkBenchGenONContentValue; }
            set
            {
                ChkBenchGenONContentValue = value;
                OnPropertyChanged("ChkBenchGenONContent");
            }
        }

        private bool ChkBenchGenONValue = false;
        public bool ChkBenchGenON
        {
            get { return ChkBenchGenONValue; }
            set
            {
                ChkBenchGenONValue = value;
                if (ChkBenchGenONValue == false)
                    ChkBenchGenONContent = "Generator Off ";
                else
                    ChkBenchGenONContent = "Generator On  ";
                OnPropertyChanged("ChkBenchGenON");
            }
        }

        private ObservableCollection<string> cmb_BenchWaveformListValue = new ObservableCollection<string>();
        public ObservableCollection<string> cmb_BenchWaveformList
        {
            get { return cmb_BenchWaveformListValue; }
            set { cmb_BenchWaveformListValue = value; OnPropertyChanged("cmb_BenchWaveformList"); }
        }

        private string cmb_PreviousBenchWaveformValue = null;
        public string cmb_PreviousBenchWaveform
        {
            get { return cmb_PreviousBenchWaveformValue; }
            set { cmb_PreviousBenchWaveformValue = value; }
        }

        private List<string> WaveFilePathListValue = new List<string>();
        public List<string> WaveFilePathList
        {
            get { return WaveFilePathListValue; }
            set
            {
                WaveFilePathListValue = value;
                OnPropertyChanged("WaveFilePathList");
            }
        }

        private Dictionary<string, bool> isNewWaveformValue = new Dictionary<string, bool>();
        public Dictionary<string, bool> isNewWaveform
        {
            get { return isNewWaveformValue; }
            set
            {
                isNewWaveformValue = value;
                OnPropertyChanged("isNewWaveform");
            }
        }

        private string cmb_BenchWaveformValue = null;
        public string cmb_BenchWaveform
        {
            get { return cmb_BenchWaveformValue; }
            set
            {
                cmb_PreviousBenchWaveform = cmb_BenchWaveformValue;
                cmb_BenchWaveformValue = value;
                OnPropertyChanged("cmb_BenchWaveform");

                if (cmb_BenchWaveformValue == "Sine, Dual")
                {
                    txtBenchFreqBVisibility = Visibility.Visible;
                    lblBenchFreqBVisibility = Visibility.Visible;
                }
                else
                {
                    txtBenchFreqBVisibility = Visibility.Collapsed;
                    lblBenchFreqBVisibility = Visibility.Collapsed;
                }
            }
        }
        
        private string _BenchWaveFormSelectedFile = string.Empty;
        public string BenchWaveFormSelectedFile
        {
            get
            {
                return _BenchWaveFormSelectedFile;
            }
            set
            {
                _BenchWaveFormSelectedFile = value;
                OnPropertyChanged("BenchWaveFormSelectedFile");
            }
        }

        private bool chkBx_BenchLevelTrackChValue = true;
        public bool chkBx_BenchLevelTrackCh
        {
            get { return chkBx_BenchLevelTrackChValue; }
            set
            {
                chkBx_BenchLevelTrackChValue = value;
                OnPropertyChanged("chkBx_BenchLevelTrackCh");

                if (BenchSetupCount > 1)
                {
                    if (!value)
                    {
                        lblBenchCh2Visibility = Visibility.Visible;
                        txtBenchLevelCh2Visibility = Visibility.Visible;
                        txtBenchDcOffsetCh2Visibility = Visibility.Visible;
                    }
                    else
                    {
                        lblBenchCh2Visibility = Visibility.Collapsed;
                        txtBenchLevelCh2Visibility = Visibility.Collapsed;
                        txtBenchDcOffsetCh2Visibility = Visibility.Collapsed;
                    }
                }

                if (BenchSetupCount > 2)
                {
                    if (!value)
                    {
                        lblBenchCh3Visibility = Visibility.Visible;
                        txtBenchLevelCh3Visibility = Visibility.Visible;
                        txtBenchDcOffsetCh3Visibility = Visibility.Visible;
                    }
                    else
                    {
                        lblBenchCh3Visibility = Visibility.Collapsed;
                        txtBenchLevelCh3Visibility = Visibility.Collapsed;
                        txtBenchDcOffsetCh3Visibility = Visibility.Collapsed;
                    }
                }

                if (BenchSetupCount > 3)
                {
                    if (!value)
                    {
                        lblBenchCh4Visibility = Visibility.Visible;
                        txtBenchLevelCh4Visibility = Visibility.Visible;
                        txtBenchDcOffsetCh4Visibility = Visibility.Visible;
                    }
                    else
                    {
                        lblBenchCh4Visibility = Visibility.Collapsed;
                        txtBenchLevelCh4Visibility = Visibility.Collapsed;
                        txtBenchDcOffsetCh4Visibility = Visibility.Collapsed;
                    }
                }

                if (BenchSetupCount > 4)
                {
                    if (!value)
                    {
                        lblBenchCh5Visibility = Visibility.Visible;
                        txtBenchLevelCh5Visibility = Visibility.Visible;
                        txtBenchDcOffsetCh5Visibility = Visibility.Visible;
                    }
                    else
                    {
                        lblBenchCh5Visibility = Visibility.Collapsed;
                        txtBenchLevelCh5Visibility = Visibility.Collapsed;
                        txtBenchDcOffsetCh5Visibility = Visibility.Collapsed;
                    }
                }

                if (BenchSetupCount > 5)
                {
                    if (!value)
                    {
                        lblBenchCh6Visibility = Visibility.Visible;
                        txtBenchLevelCh6Visibility = Visibility.Visible;
                        txtBenchDcOffsetCh6Visibility = Visibility.Visible;
                    }
                    else
                    {
                        lblBenchCh6Visibility = Visibility.Collapsed;
                        txtBenchLevelCh6Visibility = Visibility.Collapsed;
                        txtBenchDcOffsetCh6Visibility = Visibility.Collapsed;
                    }
                }

                if (BenchSetupCount > 6)
                {
                    if (!value)
                    {
                        lblBenchCh7Visibility = Visibility.Visible;
                        txtBenchLevelCh7Visibility = Visibility.Visible;
                        txtBenchDcOffsetCh7Visibility = Visibility.Visible;
                    }
                    else
                    {
                        lblBenchCh7Visibility = Visibility.Collapsed;
                        txtBenchLevelCh7Visibility = Visibility.Collapsed;
                        txtBenchDcOffsetCh7Visibility = Visibility.Collapsed;
                    }
                }

                if (BenchSetupCount > 7)
                {
                    if (!value)
                    {
                        lblBenchCh8Visibility = Visibility.Visible;
                        txtBenchLevelCh8Visibility = Visibility.Visible;
                        txtBenchDcOffsetCh8Visibility = Visibility.Visible;
                    }
                    else
                    {
                        lblBenchCh8Visibility = Visibility.Collapsed;
                        txtBenchLevelCh8Visibility = Visibility.Collapsed;
                        txtBenchDcOffsetCh8Visibility = Visibility.Collapsed;
                    }
                }
            }
        }

        private Visibility lblBenchCh2VisibilityValue = Visibility.Collapsed;
        public Visibility lblBenchCh2Visibility
        {
            get { return lblBenchCh2VisibilityValue; }
            set
            {
                lblBenchCh2VisibilityValue = value;
                OnPropertyChanged("lblBenchCh2Visibility");
            }
        }

        private Visibility lblBenchCh3VisibilityValue = Visibility.Collapsed;
        public Visibility lblBenchCh3Visibility
        {
            get { return lblBenchCh3VisibilityValue; }
            set
            {
                lblBenchCh3VisibilityValue = value;
                OnPropertyChanged("lblBenchCh3Visibility");
            }
        }

        private Visibility lblBenchCh4VisibilityValue = Visibility.Collapsed;
        public Visibility lblBenchCh4Visibility
        {
            get { return lblBenchCh4VisibilityValue; }
            set
            {
                lblBenchCh4VisibilityValue = value;
                OnPropertyChanged("lblBenchCh4Visibility");
            }
        }

        private Visibility lblBenchCh5VisibilityValue = Visibility.Collapsed;
        public Visibility lblBenchCh5Visibility
        {
            get { return lblBenchCh5VisibilityValue; }
            set
            {
                lblBenchCh5VisibilityValue = value;
                OnPropertyChanged("lblBenchCh5Visibility");
            }
        }

        private Visibility lblBenchCh6VisibilityValue = Visibility.Collapsed;
        public Visibility lblBenchCh6Visibility
        {
            get { return lblBenchCh6VisibilityValue; }
            set
            {
                lblBenchCh6VisibilityValue = value;
                OnPropertyChanged("lblBenchCh6Visibility");
            }
        }

        private Visibility lblBenchCh7VisibilityValue = Visibility.Collapsed;
        public Visibility lblBenchCh7Visibility
        {
            get { return lblBenchCh7VisibilityValue; }
            set
            {
                lblBenchCh7VisibilityValue = value;
                OnPropertyChanged("lblBenchCh7Visibility");
            }
        }

        private Visibility lblBenchCh8VisibilityValue = Visibility.Collapsed;
        public Visibility lblBenchCh8Visibility
        {
            get { return lblBenchCh8VisibilityValue; }
            set
            {
                lblBenchCh8VisibilityValue = value;
                OnPropertyChanged("lblBenchCh8Visibility");
            }
        }

        private Visibility txtBenchLevelCh2VisibilityValue = Visibility.Collapsed;
        public Visibility txtBenchLevelCh2Visibility
        {
            get { return txtBenchLevelCh2VisibilityValue; }
            set
            {
                txtBenchLevelCh2VisibilityValue = value;
                OnPropertyChanged("txtBenchLevelCh2Visibility");
            }
        }

        private Visibility txtBenchLevelCh3VisibilityValue = Visibility.Collapsed;
        public Visibility txtBenchLevelCh3Visibility
        {
            get { return txtBenchLevelCh3VisibilityValue; }
            set
            {
                txtBenchLevelCh3VisibilityValue = value;
                OnPropertyChanged("txtBenchLevelCh3Visibility");
            }
        }

        private Visibility txtBenchLevelCh4VisibilityValue = Visibility.Collapsed;
        public Visibility txtBenchLevelCh4Visibility
        {
            get { return txtBenchLevelCh4VisibilityValue; }
            set
            {
                txtBenchLevelCh4VisibilityValue = value;
                OnPropertyChanged("txtBenchLevelCh4Visibility");
            }
        }

        private Visibility txtBenchLevelCh5VisibilityValue = Visibility.Collapsed;
        public Visibility txtBenchLevelCh5Visibility
        {
            get { return txtBenchLevelCh5VisibilityValue; }
            set
            {
                txtBenchLevelCh5VisibilityValue = value;
                OnPropertyChanged("txtBenchLevelCh5Visibility");
            }
        }

        private Visibility txtBenchLevelCh6VisibilityValue = Visibility.Collapsed;
        public Visibility txtBenchLevelCh6Visibility
        {
            get { return txtBenchLevelCh6VisibilityValue; }
            set
            {
                txtBenchLevelCh6VisibilityValue = value;
                OnPropertyChanged("txtBenchLevelCh6Visibility");
            }
        }

        private Visibility txtBenchLevelCh7VisibilityValue = Visibility.Collapsed;
        public Visibility txtBenchLevelCh7Visibility
        {
            get { return txtBenchLevelCh7VisibilityValue; }
            set
            {
                txtBenchLevelCh7VisibilityValue = value;
                OnPropertyChanged("txtBenchLevelCh7Visibility");
            }
        }

        private Visibility txtBenchLevelCh8VisibilityValue = Visibility.Collapsed;
        public Visibility txtBenchLevelCh8Visibility
        {
            get { return txtBenchLevelCh8VisibilityValue; }
            set
            {
                txtBenchLevelCh8VisibilityValue = value;
                OnPropertyChanged("txtBenchLevelCh8Visibility");
            }
        }

        private Visibility txtBenchDcOffsetCh2VisibilityValue = Visibility.Collapsed;
        public Visibility txtBenchDcOffsetCh2Visibility
        {
            get { return txtBenchDcOffsetCh2VisibilityValue; }
            set
            {
                txtBenchDcOffsetCh2VisibilityValue = value;
                OnPropertyChanged("txtBenchDcOffsetCh2Visibility");
            }
        }

        private Visibility txtBenchDcOffsetCh3VisibilityValue = Visibility.Collapsed;
        public Visibility txtBenchDcOffsetCh3Visibility
        {
            get { return txtBenchDcOffsetCh3VisibilityValue; }
            set
            {
                txtBenchDcOffsetCh3VisibilityValue = value;
                OnPropertyChanged("txtBenchDcOffsetCh3Visibility");
            }
        }

        private Visibility txtBenchDcOffsetCh4VisibilityValue = Visibility.Collapsed;
        public Visibility txtBenchDcOffsetCh4Visibility
        {
            get { return txtBenchDcOffsetCh4VisibilityValue; }
            set
            {
                txtBenchDcOffsetCh4VisibilityValue = value;
                OnPropertyChanged("txtBenchDcOffsetCh4Visibility");
            }
        }

        private Visibility txtBenchDcOffsetCh5VisibilityValue = Visibility.Collapsed;
        public Visibility txtBenchDcOffsetCh5Visibility
        {
            get { return txtBenchDcOffsetCh5VisibilityValue; }
            set
            {
                txtBenchDcOffsetCh5VisibilityValue = value;
                OnPropertyChanged("txtBenchDcOffsetCh5Visibility");
            }
        }

        private Visibility txtBenchDcOffsetCh6VisibilityValue = Visibility.Collapsed;
        public Visibility txtBenchDcOffsetCh6Visibility
        {
            get { return txtBenchDcOffsetCh6VisibilityValue; }
            set
            {
                txtBenchDcOffsetCh6VisibilityValue = value;
                OnPropertyChanged("txtBenchDcOffsetCh6Visibility");
            }
        }

        private Visibility txtBenchDcOffsetCh7VisibilityValue = Visibility.Collapsed;
        public Visibility txtBenchDcOffsetCh7Visibility
        {
            get { return txtBenchDcOffsetCh7VisibilityValue; }
            set
            {
                txtBenchDcOffsetCh7VisibilityValue = value;
                OnPropertyChanged("txtBenchDcOffsetCh7Visibility");
            }
        }

        private Visibility txtBenchDcOffsetCh8VisibilityValue = Visibility.Collapsed;
        public Visibility txtBenchDcOffsetCh8Visibility
        {
            get { return txtBenchDcOffsetCh8VisibilityValue; }
            set
            {
                txtBenchDcOffsetCh8VisibilityValue = value;
                OnPropertyChanged("txtBenchDcOffsetCh8Visibility");
            }
        }

        private Visibility lblBenchFreqBVisibilityValue = Visibility.Collapsed;
        public Visibility lblBenchFreqBVisibility
        {
            get { return lblBenchFreqBVisibilityValue; }
            set
            {
                lblBenchFreqBVisibilityValue = value;
                OnPropertyChanged("lblBenchFreqBVisibility");
            }
        }

        private Visibility txtBenchFreqBVisibilityValue = Visibility.Collapsed;
        public Visibility txtBenchFreqBVisibility
        {
            get { return txtBenchFreqBVisibilityValue; }
            set
            {
                txtBenchFreqBVisibilityValue = value;
                OnPropertyChanged("txtBenchFreqBVisibility");
            }
        }

        private string txt_BenchLevelCh1Value = null;
        public string txt_BenchLevelCh1
        {
            get { return txt_BenchLevelCh1Value; }
            set
            {
                txt_BenchLevelCh1Value = value;
                OnPropertyChanged("txt_BenchLevelCh1");
            }
        }

        private string txt_BenchLevelCh2Value = null;
        public string txt_BenchLevelCh2
        {
            get { return txt_BenchLevelCh2Value; }
            set
            {
                txt_BenchLevelCh2Value = value;
                OnPropertyChanged("txt_BenchLevelCh2");
            }
        }

        private string txt_BenchLevelCh3Value = null;
        public string txt_BenchLevelCh3
        {
            get { return txt_BenchLevelCh3Value; }
            set
            {
                txt_BenchLevelCh3Value = value;
                OnPropertyChanged("txt_BenchLevelCh3");
            }
        }

        private string txt_BenchLevelCh4Value = null;
        public string txt_BenchLevelCh4
        {
            get { return txt_BenchLevelCh4Value; }
            set
            {
                txt_BenchLevelCh4Value = value;
                OnPropertyChanged("txt_BenchLevelCh4");
            }
        }

        private string txt_BenchLevelCh5Value = null;
        public string txt_BenchLevelCh5
        {
            get { return txt_BenchLevelCh5Value; }
            set
            {
                txt_BenchLevelCh5Value = value;
                OnPropertyChanged("txt_BenchLevelCh5");
            }
        }

        private string txt_BenchLevelCh6Value = null;
        public string txt_BenchLevelCh6
        {
            get { return txt_BenchLevelCh6Value; }
            set
            {
                txt_BenchLevelCh6Value = value;
                OnPropertyChanged("txt_BenchLevelCh6");
            }
        }

        private string txt_BenchLevelCh7Value = null;
        public string txt_BenchLevelCh7
        {
            get { return txt_BenchLevelCh7Value; }
            set
            {
                txt_BenchLevelCh7Value = value;
                OnPropertyChanged("txt_BenchLevelCh7");
            }
        }

        private string txt_BenchLevelCh8Value = null;
        public string txt_BenchLevelCh8
        {
            get { return txt_BenchLevelCh8Value; }
            set
            {
                txt_BenchLevelCh8Value = value;
                OnPropertyChanged("txt_BenchLevelCh8");
            }
        }

        private string txt_BenchDcOffsetCh1Value = null;
        public string txt_BenchDcOffsetCh1
        {
            get { return txt_BenchDcOffsetCh1Value; }
            set
            {
                txt_BenchDcOffsetCh1Value = value;
                OnPropertyChanged("txt_BenchDcOffsetCh1");
            }
        }

        private string txt_BenchDcOffsetCh2Value = null;
        public string txt_BenchDcOffsetCh2
        {
            get { return txt_BenchDcOffsetCh2Value; }
            set
            {
                txt_BenchDcOffsetCh2Value = value;
                OnPropertyChanged("txt_BenchDcOffsetCh2");
            }
        }

        private string txt_BenchDcOffsetCh3Value = null;
        public string txt_BenchDcOffsetCh3
        {
            get { return txt_BenchDcOffsetCh3Value; }
            set
            {
                txt_BenchDcOffsetCh3Value = value;
                OnPropertyChanged("txt_BenchDcOffsetCh3");
            }
        }

        private string txt_BenchDcOffsetCh4Value = null;
        public string txt_BenchDcOffsetCh4
        {
            get { return txt_BenchDcOffsetCh4Value; }
            set
            {
                txt_BenchDcOffsetCh4Value = value;
                OnPropertyChanged("txt_BenchDcOffsetCh4");
            }
        }

        private string txt_BenchDcOffsetCh5Value = null;
        public string txt_BenchDcOffsetCh5
        {
            get { return txt_BenchDcOffsetCh5Value; }
            set
            {
                txt_BenchDcOffsetCh5Value = value;
                OnPropertyChanged("txt_BenchDcOffsetCh5");
            }
        }

        private string txt_BenchDcOffsetCh6Value = null;
        public string txt_BenchDcOffsetCh6
        {
            get { return txt_BenchDcOffsetCh6Value; }
            set
            {
                txt_BenchDcOffsetCh6Value = value;
                OnPropertyChanged("txt_BenchDcOffsetCh6");
            }
        }

        private string txt_BenchDcOffsetCh7Value = null;
        public string txt_BenchDcOffsetCh7
        {
            get { return txt_BenchDcOffsetCh7Value; }
            set
            {
                txt_BenchDcOffsetCh7Value = value;
                OnPropertyChanged("txt_BenchDcOffsetCh7");
            }
        }

        private string txt_BenchDcOffsetCh8Value = null;
        public string txt_BenchDcOffsetCh8
        {
            get { return txt_BenchDcOffsetCh8Value; }
            set
            {
                txt_BenchDcOffsetCh8Value = value;
                OnPropertyChanged("txt_BenchDcOffsetCh8");
            }
        }

        private string txt_BenchfrequencyAValue = null;
        public string txt_BenchfrequencyA
        {
            get { return txt_BenchfrequencyAValue; }
            set
            {
                txt_BenchfrequencyAValue = value;
                OnPropertyChanged("txt_BenchfrequencyA");
            }
        }

        private string txt_BenchfrequencyBValue = null;
        public string txt_BenchfrequencyB
        {
            get { return txt_BenchfrequencyBValue; }
            set
            {
                txt_BenchfrequencyBValue = value;
                OnPropertyChanged("txt_BenchfrequencyB");
            }
        }

        private bool ChkBenchCh1EnableValue = true;
        public bool ChkBenchCh1Enable
        {
            get { return ChkBenchCh1EnableValue; }
            set
            {
                ChkBenchCh1EnableValue = value;
                OnPropertyChanged("ChkBenchCh1Enable");
            }
        }

        private bool ChkBenchCh2EnableValue = true;
        public bool ChkBenchCh2Enable
        {
            get { return ChkBenchCh2EnableValue; }
            set
            {
                ChkBenchCh2EnableValue = value;
                OnPropertyChanged("ChkBenchCh2Enable");
            }
        }

        private bool ChkBenchCh3EnableValue = true;
        public bool ChkBenchCh3Enable
        {
            get { return ChkBenchCh3EnableValue; }
            set
            {
                ChkBenchCh3EnableValue = value;
                OnPropertyChanged("ChkBenchCh3Enable");
            }
        }

        private bool ChkBenchCh4EnableValue = true;
        public bool ChkBenchCh4Enable
        {
            get { return ChkBenchCh4EnableValue; }
            set
            {
                ChkBenchCh4EnableValue = value;
                OnPropertyChanged("ChkBenchCh4Enable");
            }
        }

        private bool ChkBenchCh5EnableValue = true;
        public bool ChkBenchCh5Enable
        {
            get { return ChkBenchCh5EnableValue; }
            set
            {
                ChkBenchCh5EnableValue = value;
                OnPropertyChanged("ChkBenchCh5Enable");
            }
        }

        private bool ChkBenchCh6EnableValue = true;
        public bool ChkBenchCh6Enable
        {
            get { return ChkBenchCh6EnableValue; }
            set
            {
                ChkBenchCh6EnableValue = value;
                OnPropertyChanged("ChkBenchCh6Enable");
            }
        }

        private bool ChkBenchCh7EnableValue = true;
        public bool ChkBenchCh7Enable
        {
            get { return ChkBenchCh7EnableValue; }
            set
            {
                ChkBenchCh7EnableValue = value;
                OnPropertyChanged("ChkBenchCh7Enable");
            }
        }

        private bool ChkBenchCh8EnableValue = true;
        public bool ChkBenchCh8Enable
        {
            get { return ChkBenchCh8EnableValue; }
            set
            {
                ChkBenchCh8EnableValue = value;
                OnPropertyChanged("ChkBenchCh8Enable");
            }
        }

        private Visibility chkBenchCh1EnableVisibleValue = Visibility.Collapsed;
        public Visibility chkBenchCh1EnableVisible
        {
            get { return chkBenchCh1EnableVisibleValue; }
            set
            {
                chkBenchCh1EnableVisibleValue = value;
                OnPropertyChanged("chkBenchCh1EnableVisible");
            }
        }

        private Visibility chkBenchCh2EnableVisibleValue = Visibility.Collapsed;
        public Visibility chkBenchCh2EnableVisible
        {
            get { return chkBenchCh2EnableVisibleValue; }
            set
            {
                chkBenchCh2EnableVisibleValue = value;
                OnPropertyChanged("chkBenchCh2EnableVisible");
            }
        }

        private Visibility chkBenchCh3EnableVisibleValue = Visibility.Collapsed;
        public Visibility chkBenchCh3EnableVisible
        {
            get { return chkBenchCh3EnableVisibleValue; }
            set
            {
                chkBenchCh3EnableVisibleValue = value;
                OnPropertyChanged("chkBenchCh3EnableVisible");
            }
        }

        private Visibility chkBenchCh4EnableVisibleValue = Visibility.Collapsed;
        public Visibility chkBenchCh4EnableVisible
        {
            get { return chkBenchCh4EnableVisibleValue; }
            set
            {
                chkBenchCh4EnableVisibleValue = value;
                OnPropertyChanged("chkBenchCh4EnableVisible");
            }
        }

        private Visibility chkBenchCh5EnableVisibleValue = Visibility.Collapsed;
        public Visibility chkBenchCh5EnableVisible
        {
            get { return chkBenchCh5EnableVisibleValue; }
            set
            {
                chkBenchCh5EnableVisibleValue = value;
                OnPropertyChanged("chkBenchCh5EnableVisible");
            }
        }

        private Visibility chkBenchCh6EnableVisibleValue = Visibility.Collapsed;
        public Visibility chkBenchCh6EnableVisible
        {
            get { return chkBenchCh6EnableVisibleValue; }
            set
            {
                chkBenchCh6EnableVisibleValue = value;
                OnPropertyChanged("chkBenchCh6EnableVisible");
            }
        }

        private Visibility chkBenchCh7EnableVisibleValue = Visibility.Collapsed;
        public Visibility chkBenchCh7EnableVisible
        {
            get { return chkBenchCh7EnableVisibleValue; }
            set
            {
                chkBenchCh7EnableVisibleValue = value;
                OnPropertyChanged("chkBenchCh7EnableVisible");
            }
        }

        private Visibility chkBenchCh8EnableVisibleValue = Visibility.Collapsed;
        public Visibility chkBenchCh8EnableVisible
        {
            get { return chkBenchCh8EnableVisibleValue; }
            set
            {
                chkBenchCh8EnableVisibleValue = value;
                OnPropertyChanged("chkBenchCh8EnableVisible");
            }
        }

        private Visibility chkSeqCh1EnableVisibleValue = Visibility.Collapsed;
        public Visibility chkSeqCh1EnableVisible
        {
            get { return chkSeqCh1EnableVisibleValue; }
            set
            {
                chkSeqCh1EnableVisibleValue = value;
                OnPropertyChanged("chkSeqCh1EnableVisible");
            }
        }

        private Visibility chkSeqCh2EnableVisibleValue = Visibility.Collapsed;
        public Visibility chkSeqCh2EnableVisible
        {
            get { return chkSeqCh2EnableVisibleValue; }
            set
            {
                chkSeqCh2EnableVisibleValue = value;
                OnPropertyChanged("chkSeqCh2EnableVisible");
            }
        }

        private Visibility chkSeqCh3EnableVisibleValue = Visibility.Collapsed;
        public Visibility chkSeqCh3EnableVisible
        {
            get { return chkSeqCh3EnableVisibleValue; }
            set
            {
                chkSeqCh3EnableVisibleValue = value;
                OnPropertyChanged("chkSeqCh3EnableVisible");
            }
        }

        private Visibility chkSeqCh4EnableVisibleValue = Visibility.Collapsed;
        public Visibility chkSeqCh4EnableVisible
        {
            get { return chkSeqCh4EnableVisibleValue; }
            set
            {
                chkSeqCh4EnableVisibleValue = value;
                OnPropertyChanged("chkSeqCh4EnableVisible");
            }
        }

        private Visibility chkSeqCh5EnableVisibleValue = Visibility.Collapsed;
        public Visibility chkSeqCh5EnableVisible
        {
            get { return chkSeqCh5EnableVisibleValue; }
            set
            {
                chkSeqCh5EnableVisibleValue = value;
                OnPropertyChanged("chkSeqCh5EnableVisible");
            }
        }

        private Visibility chkSeqCh6EnableVisibleValue = Visibility.Collapsed;
        public Visibility chkSeqCh6EnableVisible
        {
            get { return chkSeqCh6EnableVisibleValue; }
            set
            {
                chkSeqCh6EnableVisibleValue = value;
                OnPropertyChanged("chkSeqCh6EnableVisible");
            }
        }

        private Visibility chkSeqCh7EnableVisibleValue = Visibility.Collapsed;
        public Visibility chkSeqCh7EnableVisible
        {
            get { return chkSeqCh7EnableVisibleValue; }
            set
            {
                chkSeqCh7EnableVisibleValue = value;
                OnPropertyChanged("chkSeqCh7EnableVisible");
            }
        }

        private Visibility chkSeqCh8EnableVisibleValue = Visibility.Collapsed;
        public Visibility chkSeqCh8EnableVisible
        {
            get { return chkSeqCh8EnableVisibleValue; }
            set
            {
                chkSeqCh8EnableVisibleValue = value;
                OnPropertyChanged("chkSeqCh8EnableVisible");
            }
        }

        private string cmb_SeqModeReadingTypeValue = null;
        public string cmb_SeqModeReadingType
        {
            get { return cmb_SeqModeReadingTypeValue; }
            set
            {
                cmb_SeqModeReadingTypeValue = value;
                OnPropertyChanged("cmb_SeqModeReadingType");
            }
        }

        private bool ChkSeqGenONValue = false;
        public bool ChkSeqGenON
        {
            get { return ChkSeqGenONValue; }
            set
            {
                ChkSeqGenONValue = value;
                if (ChkSeqGenONValue == true)
                    ChkSeqGenONContent = "Generator On  ";
                else
                    ChkSeqGenONContent = "Generator Off ";
                OnPropertyChanged("ChkSeqGenON");
            }
        }

        private string ChkSeqGenONContentValue = "Generator Off ";
        public string ChkSeqGenONContent
        {
            get { return ChkSeqGenONContentValue; }
            set
            {
                ChkSeqGenONContentValue = value;
                OnPropertyChanged("ChkSeqGenONContent");
            }
        }

        private ObservableCollection<string> cmb_SeqWaveformListValue = new ObservableCollection<string>();
        public ObservableCollection<string> cmb_SeqWaveformList
        {
            get { return cmb_SeqWaveformListValue; }
            set { cmb_SeqWaveformListValue = value; OnPropertyChanged("cmb_SeqWaveformList"); }
        }

        private string cmb_PreviousSeqWaveformValue = null;
        public string cmb_PreviousSeqWaveform
        {
            get { return cmb_PreviousSeqWaveformValue; }
            set { cmb_PreviousSeqWaveformValue = value; }
        }

        private string cmbSeqWaveFormValue = null;
        public string cmbSeqWaveForm
        {
            get { return cmbSeqWaveFormValue; }
            set
            {
                cmb_PreviousSeqWaveform = cmbSeqWaveFormValue;
                cmbSeqWaveFormValue = value;
                OnPropertyChanged("cmbSeqWaveForm");

                if (cmbSeqWaveFormValue == "Sine, Dual")
                {
                    lblSeqFreqBVisible = Visibility.Visible;
                    TxtSeqFreqBVisible = Visibility.Visible;
                }
                else
                {
                    lblSeqFreqBVisible = Visibility.Collapsed;
                    TxtSeqFreqBVisible = Visibility.Collapsed;
                }
            }
        }
        
        private bool isWavFileLoaded = false;
        public bool IsWavFileLoaded
        {
            get
            {
                return isWavFileLoaded;
            }
            set
            {
                isWavFileLoaded = value;
                OnPropertyChanged("IsWavFileLoaded");
            }
        }

        private string _SeqWaveFormSelectedFile = string.Empty;
        public string SeqWaveFormSelectedFile
        {
            get
            {
                return _SeqWaveFormSelectedFile;
            }
            set
            {
                _SeqWaveFormSelectedFile = value;
                OnPropertyChanged("SeqWaveFormSelectedFile");
            }
        }

        private List<string> cmbSeqTestChListValue = new List<string>();
        public List<string> cmbSeqTestChList
        {
            get { return cmbSeqTestChListValue; }
            set { cmbSeqTestChListValue = value; OnPropertyChanged("cmbSeqTestChList"); }
        }

        private string cmbSeqTestChannelValue = null;
        public string cmbSeqTestChannel
        {
            get { return cmbSeqTestChannelValue; }
            set
            {
                cmbSeqTestChannelValue = value;
                OnPropertyChanged("cmbSeqTestChannel");
            }
        }

        private bool ChkSeqTrackChValue = true;
        public bool ChkSeqTrackCh
        {
            get { return ChkSeqTrackChValue; }
            set
            {
                ChkSeqTrackChValue = value;
                OnPropertyChanged("ChkSeqTrackCh");
                if (SeqSetupCount > 1)
                {
                    if (!value)
                    {
                        lblSeqCh2Visibility = Visibility.Visible;
                        TxtSeqLevelCh2Visible = Visibility.Visible;
                        TxtSeqDCOffsetCh2Visible = Visibility.Visible;
                    }
                    else
                    {
                        lblSeqCh2Visibility = Visibility.Collapsed;
                        TxtSeqLevelCh2Visible = Visibility.Collapsed;
                        TxtSeqDCOffsetCh2Visible = Visibility.Collapsed;
                    }
                }

                if (SeqSetupCount > 2)
                {
                    if (!value)
                    {
                        lblSeqCh3Visibility = Visibility.Visible;
                        TxtSeqLevelCh3Visible = Visibility.Visible;
                        TxtSeqDCOffsetCh3Visible = Visibility.Visible;
                    }
                    else
                    {
                        lblSeqCh3Visibility = Visibility.Collapsed;
                        TxtSeqLevelCh3Visible = Visibility.Collapsed;
                        TxtSeqDCOffsetCh3Visible = Visibility.Collapsed;
                    }
                }

                if (SeqSetupCount > 3)
                {
                    if (!value)
                    {
                        lblSeqCh4Visibility = Visibility.Visible;
                        TxtSeqLevelCh4Visible = Visibility.Visible;
                        TxtSeqDCOffsetCh4Visible = Visibility.Visible;
                    }
                    else
                    {
                        lblSeqCh4Visibility = Visibility.Collapsed;
                        TxtSeqLevelCh4Visible = Visibility.Collapsed;
                        TxtSeqDCOffsetCh4Visible = Visibility.Collapsed;
                    }
                }

                if (SeqSetupCount > 4)
                {
                    if (!value)
                    {
                        lblSeqCh5Visibility = Visibility.Visible;
                        TxtSeqLevelCh5Visible = Visibility.Visible;
                        TxtSeqDCOffsetCh5Visible = Visibility.Visible;
                    }
                    else
                    {
                        lblSeqCh5Visibility = Visibility.Collapsed;
                        TxtSeqLevelCh5Visible = Visibility.Collapsed;
                        TxtSeqDCOffsetCh5Visible = Visibility.Collapsed;
                    }
                }

                if (SeqSetupCount > 5)
                {
                    if (!value)
                    {
                        lblSeqCh6Visibility = Visibility.Visible;
                        TxtSeqLevelCh6Visible = Visibility.Visible;
                        TxtSeqDCOffsetCh6Visible = Visibility.Visible;
                    }
                    else
                    {
                        lblSeqCh6Visibility = Visibility.Collapsed;
                        TxtSeqLevelCh6Visible = Visibility.Collapsed;
                        TxtSeqDCOffsetCh6Visible = Visibility.Collapsed;
                    }
                }

                if (SeqSetupCount > 6)
                {
                    if (!value)
                    {
                        lblSeqCh7Visibility = Visibility.Visible;
                        TxtSeqLevelCh7Visible = Visibility.Visible;
                        TxtSeqDCOffsetCh7Visible = Visibility.Visible;
                    }
                    else
                    {
                        lblSeqCh7Visibility = Visibility.Collapsed;
                        TxtSeqLevelCh7Visible = Visibility.Collapsed;
                        TxtSeqDCOffsetCh7Visible = Visibility.Collapsed;
                    }
                }

                if (SeqSetupCount > 7)
                {
                    if (!value)
                    {
                        lblSeqCh8Visibility = Visibility.Visible;
                        TxtSeqLevelCh8Visible = Visibility.Visible;
                        TxtSeqDCOffsetCh8Visible = Visibility.Visible;
                    }
                    else
                    {
                        lblSeqCh8Visibility = Visibility.Collapsed;
                        TxtSeqLevelCh8Visible = Visibility.Collapsed;
                        TxtSeqDCOffsetCh8Visible = Visibility.Collapsed;
                    }
                }
            }
        }

        private string TxtSeqLevelCh1Value = null;
        public string TxtSeqLevelCh1
        {
            get { return TxtSeqLevelCh1Value; }
            set
            {
                TxtSeqLevelCh1Value = value;
                OnPropertyChanged("TxtSeqLevelCh1");
            }
        }

        private string TxtSeqLevelCh2Value = null;
        public string TxtSeqLevelCh2
        {
            get { return TxtSeqLevelCh2Value; }
            set
            {
                TxtSeqLevelCh2Value = value;
                OnPropertyChanged("TxtSeqLevelCh2");
            }
        }

        private string TxtSeqLevelCh3Value = null;
        public string TxtSeqLevelCh3
        {
            get { return TxtSeqLevelCh3Value; }
            set
            {
                TxtSeqLevelCh3Value = value;
                OnPropertyChanged("TxtSeqLevelCh3");
            }
        }

        private string TxtSeqLevelCh4Value = null;
        public string TxtSeqLevelCh4
        {
            get { return TxtSeqLevelCh4Value; }
            set
            {
                TxtSeqLevelCh4Value = value;
                OnPropertyChanged("TxtSeqLevelCh4");
            }
        }

        private string TxtSeqLevelCh5Value = null;
        public string TxtSeqLevelCh5
        {
            get { return TxtSeqLevelCh5Value; }
            set
            {
                TxtSeqLevelCh5Value = value;
                OnPropertyChanged("TxtSeqLevelCh5");
            }
        }

        private string TxtSeqLevelCh6Value = null;
        public string TxtSeqLevelCh6
        {
            get { return TxtSeqLevelCh6Value; }
            set
            {
                TxtSeqLevelCh6Value = value;
                OnPropertyChanged("TxtSeqLevelCh6");
            }
        }

        private string TxtSeqLevelCh7Value = null;
        public string TxtSeqLevelCh7
        {
            get { return TxtSeqLevelCh7Value; }
            set
            {
                TxtSeqLevelCh7Value = value;
                OnPropertyChanged("TxtSeqLevelCh7");
            }
        }

        private string TxtSeqLevelCh8Value = null;
        public string TxtSeqLevelCh8
        {
            get { return TxtSeqLevelCh8Value; }
            set
            {
                TxtSeqLevelCh8Value = value;
                OnPropertyChanged("TxtSeqLevelCh8");
            }
        }

        private string TxtSeqLevelDcCh1Value = null;
        public string TxtSeqLevelDcCh1
        {
            get { return TxtSeqLevelDcCh1Value; }
            set
            {
                TxtSeqLevelDcCh1Value = value;
                OnPropertyChanged("TxtSeqLevelDcCh1");
            }
        }

        private string TxtSeqLevelDcCh2Value = null;
        public string TxtSeqLevelDcCh2
        {
            get { return TxtSeqLevelDcCh2Value; }
            set
            {
                TxtSeqLevelDcCh2Value = value;
                OnPropertyChanged("TxtSeqLevelDcCh2");
            }
        }

        private string TxtSeqLevelDcCh3Value = null;
        public string TxtSeqLevelDcCh3
        {
            get { return TxtSeqLevelDcCh3Value; }
            set
            {
                TxtSeqLevelDcCh3Value = value;
                OnPropertyChanged("TxtSeqLevelDcCh3");
            }
        }

        private string TxtSeqLevelDcCh4Value = null;
        public string TxtSeqLevelDcCh4
        {
            get { return TxtSeqLevelDcCh4Value; }
            set
            {
                TxtSeqLevelDcCh4Value = value;
                OnPropertyChanged("TxtSeqLevelDcCh4");
            }
        }

        private string TxtSeqLevelDcCh5Value = null;
        public string TxtSeqLevelDcCh5
        {
            get { return TxtSeqLevelDcCh5Value; }
            set
            {
                TxtSeqLevelDcCh5Value = value;
                OnPropertyChanged("TxtSeqLevelDcCh5");
            }
        }

        private string TxtSeqLevelDcCh6Value = null;
        public string TxtSeqLevelDcCh6
        {
            get { return TxtSeqLevelDcCh6Value; }
            set
            {
                TxtSeqLevelDcCh6Value = value;
                OnPropertyChanged("TxtSeqLevelDcCh6");
            }
        }

        private string TxtSeqLevelDcCh7Value = null;
        public string TxtSeqLevelDcCh7
        {
            get { return TxtSeqLevelDcCh7Value; }
            set
            {
                TxtSeqLevelDcCh7Value = value;
                OnPropertyChanged("TxtSeqLevelDcCh7");
            }
        }

        private string TxtSeqLevelDcCh8Value = null;
        public string TxtSeqLevelDcCh8
        {
            get { return TxtSeqLevelDcCh8Value; }
            set
            {
                TxtSeqLevelDcCh8Value = value;
                OnPropertyChanged("TxtSeqLevelDcCh8");
            }
        }

        private Visibility lblSeqCh2VisibilityValue = Visibility.Collapsed;
        public Visibility lblSeqCh2Visibility
        {
            get { return lblSeqCh2VisibilityValue; }
            set
            {
                lblSeqCh2VisibilityValue = value;
                OnPropertyChanged("lblSeqCh2Visibility");
            }
        }

        private Visibility lblSeqCh3VisibilityValue = Visibility.Collapsed;
        public Visibility lblSeqCh3Visibility
        {
            get { return lblSeqCh3VisibilityValue; }
            set
            {
                lblSeqCh3VisibilityValue = value;
                OnPropertyChanged("lblSeqCh3Visibility");
            }
        }

        private Visibility lblSeqCh4VisibilityValue = Visibility.Collapsed;
        public Visibility lblSeqCh4Visibility
        {
            get { return lblSeqCh4VisibilityValue; }
            set
            {
                lblSeqCh4VisibilityValue = value;
                OnPropertyChanged("lblSeqCh4Visibility");
            }
        }

        private Visibility lblSeqCh5VisibilityValue = Visibility.Collapsed;
        public Visibility lblSeqCh5Visibility
        {
            get { return lblSeqCh5VisibilityValue; }
            set
            {
                lblSeqCh5VisibilityValue = value;
                OnPropertyChanged("lblSeqCh5Visibility");
            }
        }

        private Visibility lblSeqCh6VisibilityValue = Visibility.Collapsed;
        public Visibility lblSeqCh6Visibility
        {
            get { return lblSeqCh6VisibilityValue; }
            set
            {
                lblSeqCh6VisibilityValue = value;
                OnPropertyChanged("lblSeqCh6Visibility");
            }
        }

        private Visibility lblSeqCh7VisibilityValue = Visibility.Collapsed;
        public Visibility lblSeqCh7Visibility
        {
            get { return lblSeqCh7VisibilityValue; }
            set
            {
                lblSeqCh7VisibilityValue = value;
                OnPropertyChanged("lblSeqCh7Visibility");
            }
        }

        private Visibility lblSeqCh8VisibilityValue = Visibility.Collapsed;
        public Visibility lblSeqCh8Visibility
        {
            get { return lblSeqCh8VisibilityValue; }
            set
            {
                lblSeqCh8VisibilityValue = value;
                OnPropertyChanged("lblSeqCh8Visibility");
            }
        }

        private Visibility TxtSeqLevelCh2VisibleValue = Visibility.Collapsed;
        public Visibility TxtSeqLevelCh2Visible
        {
            get { return TxtSeqLevelCh2VisibleValue; }
            set
            {
                TxtSeqLevelCh2VisibleValue = value;
                OnPropertyChanged("TxtSeqLevelCh2Visible");
            }
        }

        private Visibility TxtSeqLevelCh3VisibleValue = Visibility.Collapsed;
        public Visibility TxtSeqLevelCh3Visible
        {
            get { return TxtSeqLevelCh3VisibleValue; }
            set
            {
                TxtSeqLevelCh3VisibleValue = value;
                OnPropertyChanged("TxtSeqLevelCh3Visible");
            }
        }

        private Visibility TxtSeqLevelCh4VisibleValue = Visibility.Collapsed;
        public Visibility TxtSeqLevelCh4Visible
        {
            get { return TxtSeqLevelCh4VisibleValue; }
            set
            {
                TxtSeqLevelCh4VisibleValue = value;
                OnPropertyChanged("TxtSeqLevelCh4Visible");
            }
        }

        private Visibility TxtSeqLevelCh5VisibleValue = Visibility.Collapsed;
        public Visibility TxtSeqLevelCh5Visible
        {
            get { return TxtSeqLevelCh5VisibleValue; }
            set
            {
                TxtSeqLevelCh5VisibleValue = value;
                OnPropertyChanged("TxtSeqLevelCh5Visible");
            }
        }

        private Visibility TxtSeqLevelCh6VisibleValue = Visibility.Collapsed;
        public Visibility TxtSeqLevelCh6Visible
        {
            get { return TxtSeqLevelCh6VisibleValue; }
            set
            {
                TxtSeqLevelCh6VisibleValue = value;
                OnPropertyChanged("TxtSeqLevelCh6Visible");
            }
        }

        private Visibility TxtSeqLevelCh7VisibleValue = Visibility.Collapsed;
        public Visibility TxtSeqLevelCh7Visible
        {
            get { return TxtSeqLevelCh7VisibleValue; }
            set
            {
                TxtSeqLevelCh7VisibleValue = value;
                OnPropertyChanged("TxtSeqLevelCh7Visible");
            }
        }

        private Visibility TxtSeqLevelCh8VisibleValue = Visibility.Collapsed;
        public Visibility TxtSeqLevelCh8Visible
        {
            get { return TxtSeqLevelCh8VisibleValue; }
            set
            {
                TxtSeqLevelCh8VisibleValue = value;
                OnPropertyChanged("TxtSeqLevelCh8Visible");
            }
        }

        private Visibility TxtSeqDCOffsetCh2VisibleValue = Visibility.Collapsed;
        public Visibility TxtSeqDCOffsetCh2Visible
        {
            get { return TxtSeqDCOffsetCh2VisibleValue; }
            set
            {
                TxtSeqDCOffsetCh2VisibleValue = value;
                OnPropertyChanged("TxtSeqDCOffsetCh2Visible");
            }
        }

        private Visibility TxtSeqDCOffsetCh3VisibleValue = Visibility.Collapsed;
        public Visibility TxtSeqDCOffsetCh3Visible
        {
            get { return TxtSeqDCOffsetCh3VisibleValue; }
            set
            {
                TxtSeqDCOffsetCh3VisibleValue = value;
                OnPropertyChanged("TxtSeqDCOffsetCh3Visible");
            }
        }

        private Visibility TxtSeqDCOffsetCh4VisibleValue = Visibility.Collapsed;
        public Visibility TxtSeqDCOffsetCh4Visible
        {
            get { return TxtSeqDCOffsetCh4VisibleValue; }
            set
            {
                TxtSeqDCOffsetCh4VisibleValue = value;
                OnPropertyChanged("TxtSeqDCOffsetCh4Visible");
            }
        }

        private Visibility TxtSeqDCOffsetCh5VisibleValue = Visibility.Collapsed;
        public Visibility TxtSeqDCOffsetCh5Visible
        {
            get { return TxtSeqDCOffsetCh5VisibleValue; }
            set
            {
                TxtSeqDCOffsetCh5VisibleValue = value;
                OnPropertyChanged("TxtSeqDCOffsetCh5Visible");
            }
        }

        private Visibility TxtSeqDCOffsetCh6VisibleValue = Visibility.Collapsed;
        public Visibility TxtSeqDCOffsetCh6Visible
        {
            get { return TxtSeqDCOffsetCh6VisibleValue; }
            set
            {
                TxtSeqDCOffsetCh6VisibleValue = value;
                OnPropertyChanged("TxtSeqDCOffsetCh6Visible");
            }
        }

        private Visibility TxtSeqDCOffsetCh7VisibleValue = Visibility.Collapsed;
        public Visibility TxtSeqDCOffsetCh7Visible
        {
            get { return TxtSeqDCOffsetCh7VisibleValue; }
            set
            {
                TxtSeqDCOffsetCh7VisibleValue = value;
                OnPropertyChanged("TxtSeqDCOffsetCh7Visible");
            }
        }

        private Visibility TxtSeqDCOffsetCh8VisibleValue = Visibility.Collapsed;
        public Visibility TxtSeqDCOffsetCh8Visible
        {
            get { return TxtSeqDCOffsetCh8VisibleValue; }
            set
            {
                TxtSeqDCOffsetCh8VisibleValue = value;
                OnPropertyChanged("TxtSeqDCOffsetCh8Visible");
            }
        }

        private Visibility lblSeqFreqBVisibleValue = Visibility.Collapsed;
        public Visibility lblSeqFreqBVisible
        {
            get { return lblSeqFreqBVisibleValue; }
            set
            {
                lblSeqFreqBVisibleValue = value;
                OnPropertyChanged("lblSeqFreqBVisible");
            }
        }

        private Visibility TxtSeqFreqBVisibleValue = Visibility.Collapsed;
        public Visibility TxtSeqFreqBVisible
        {
            get { return TxtSeqFreqBVisibleValue; }
            set
            {
                TxtSeqFreqBVisibleValue = value;
                OnPropertyChanged("TxtSeqFreqBVisible");
            }
        }

        private string TxtSeqFreqAValue = null;
        public string TxtSeqFreqA
        {
            get { return TxtSeqFreqAValue; }
            set
            {
                TxtSeqFreqAValue = value;
                OnPropertyChanged("TxtSeqFreqA");
            }
        }

        private string TxtSeqFreqBValue = null;
        public string TxtSeqFreqB
        {
            get { return TxtSeqFreqBValue; }
            set
            {
                TxtSeqFreqBValue = value;
                OnPropertyChanged("TxtSeqFreqB");
            }
        }

        private bool ChkSeqCh1EnableValue = true;
        public bool ChkSeqCh1Enable
        {
            get { return ChkSeqCh1EnableValue; }
            set
            {
                ChkSeqCh1EnableValue = value;
                OnPropertyChanged("ChkSeqCh1Enable");
            }
        }

        private bool ChkSeqCh2EnableValue = true;
        public bool ChkSeqCh2Enable
        {
            get { return ChkSeqCh2EnableValue; }
            set
            {
                ChkSeqCh2EnableValue = value;
                OnPropertyChanged("ChkSeqCh2Enable");
            }
        }

        private bool ChkSeqCh3EnableValue = true;
        public bool ChkSeqCh3Enable
        {
            get { return ChkSeqCh3EnableValue; }
            set
            {
                ChkSeqCh3EnableValue = value;
                OnPropertyChanged("ChkSeqCh3Enable");
            }
        }

        private bool ChkSeqCh4EnableValue = true;
        public bool ChkSeqCh4Enable
        {
            get { return ChkSeqCh4EnableValue; }
            set
            {
                ChkSeqCh4EnableValue = value;
                OnPropertyChanged("ChkSeqCh4Enable");
            }
        }

        private bool ChkSeqCh5EnableValue = true;
        public bool ChkSeqCh5Enable
        {
            get { return ChkSeqCh5EnableValue; }
            set
            {
                ChkSeqCh5EnableValue = value;
                OnPropertyChanged("ChkSeqCh5Enable");
            }
        }

        private bool ChkSeqCh6EnableValue = true;
        public bool ChkSeqCh6Enable
        {
            get { return ChkSeqCh6EnableValue; }
            set
            {
                ChkSeqCh6EnableValue = value;
                OnPropertyChanged("ChkSeqCh6Enable");
            }
        }

        private bool ChkSeqCh7EnableValue = true;
        public bool ChkSeqCh7Enable
        {
            get { return ChkSeqCh7EnableValue; }
            set
            {
                ChkSeqCh7EnableValue = value;
                OnPropertyChanged("ChkSeqCh7Enable");
            }
        }

        private bool ChkSeqCh8EnableValue = true;
        public bool ChkSeqCh8Enable
        {
            get { return ChkSeqCh8EnableValue; }
            set
            {
                ChkSeqCh8EnableValue = value;
                OnPropertyChanged("ChkSeqCh8Enable");
            }
        }

        private string TxtSeqDelayValue = null;
        public string TxtSeqDelay
        {
            get { return TxtSeqDelayValue; }
            set
            {
                TxtSeqDelayValue = value;
                OnPropertyChanged("TxtSeqDelay");
            }
        }

        private int BenchSetupCountValue = 0;
        public int BenchSetupCount
        {
            get { return BenchSetupCountValue; }
            set
            {
                BenchSetupCountValue = value;
                OnPropertyChanged("BenchSetupCount");

                if (value > 0)
                    chkBenchCh1EnableVisible = Visibility.Visible;
                else
                    chkBenchCh1EnableVisible = Visibility.Collapsed;

                if (value > 1)
                    chkBenchCh2EnableVisible = Visibility.Visible;
                else
                    chkBenchCh2EnableVisible = Visibility.Collapsed;

                if (value > 2)
                    chkBenchCh3EnableVisible = Visibility.Visible;
                else
                    chkBenchCh3EnableVisible = Visibility.Collapsed;

                if (value > 3)
                    chkBenchCh4EnableVisible = Visibility.Visible;
                else
                    chkBenchCh4EnableVisible = Visibility.Collapsed;

                if (value > 4)
                    chkBenchCh5EnableVisible = Visibility.Visible;
                else
                    chkBenchCh5EnableVisible = Visibility.Collapsed;

                if (value > 5)
                    chkBenchCh6EnableVisible = Visibility.Visible;
                else
                    chkBenchCh6EnableVisible = Visibility.Collapsed;

                if (value > 6)
                    chkBenchCh7EnableVisible = Visibility.Visible;
                else
                    chkBenchCh7EnableVisible = Visibility.Collapsed;

                if (value > 7)
                    chkBenchCh8EnableVisible = Visibility.Visible;
                else
                    chkBenchCh8EnableVisible = Visibility.Collapsed;
            }
        }

        private int SeqSetupCountValue = 0;
        public int SeqSetupCount
        {
            get { return SeqSetupCountValue; }
            set
            {
                SeqSetupCountValue = value;
                OnPropertyChanged("SeqSetupCount");

                if (value > 0)
                {
                    chkSeqCh1EnableVisible = Visibility.Visible;
                    if (!cmbSeqTestChList.Contains("AllChannels"))
                    {
                        cmbSeqTestChList.Add("AllChannels");
                    }

                    if (!cmbSeqTestChList.Contains("Ch1"))
                    {
                        cmbSeqTestChList.Add("Ch1");
                    }
                }
                else
                {
                    chkSeqCh1EnableVisible = Visibility.Collapsed;
                }

                if (value > 1)
                {
                    chkSeqCh2EnableVisible = Visibility.Visible;
                    if (!cmbSeqTestChList.Contains("Ch2"))
                    {
                        cmbSeqTestChList.Add("Ch2");
                    }
                }
                else
                {
                    chkSeqCh2EnableVisible = Visibility.Collapsed;
                }

                if (value > 2)
                {
                    chkSeqCh3EnableVisible = Visibility.Visible;
                    if (!cmbSeqTestChList.Contains("Ch3"))
                    {
                        cmbSeqTestChList.Add("Ch3");
                    }
                }
                else
                {
                    chkSeqCh3EnableVisible = Visibility.Collapsed;
                }

                if (value > 3)
                {
                    chkSeqCh4EnableVisible = Visibility.Visible;
                    if (!cmbSeqTestChList.Contains("Ch4"))
                    {
                        cmbSeqTestChList.Add("Ch4");
                    }
                }
                else
                {
                    chkSeqCh4EnableVisible = Visibility.Collapsed;
                }

                if (value > 4)
                {
                    chkSeqCh5EnableVisible = Visibility.Visible;
                    if (!cmbSeqTestChList.Contains("Ch5"))
                    {
                        cmbSeqTestChList.Add("Ch5");
                    }
                }
                else
                {
                    chkSeqCh5EnableVisible = Visibility.Collapsed;
                }

                if (value > 5)
                {
                    chkSeqCh6EnableVisible = Visibility.Visible;
                    if (!cmbSeqTestChList.Contains("Ch6"))
                    {
                        cmbSeqTestChList.Add("Ch6");
                    }
                }
                else
                {
                    chkSeqCh6EnableVisible = Visibility.Collapsed;
                }

                if (value > 6)
                {
                    chkSeqCh7EnableVisible = Visibility.Visible;
                    if (!cmbSeqTestChList.Contains("Ch7"))
                    {
                        cmbSeqTestChList.Add("Ch7");
                    }
                }
                else
                {
                    chkSeqCh7EnableVisible = Visibility.Collapsed;
                }

                if (value > 7)
                {
                    chkSeqCh8EnableVisible = Visibility.Visible;
                    if (!cmbSeqTestChList.Contains("Ch8"))
                    {
                        cmbSeqTestChList.Add("Ch8");
                    }
                }
                else
                {
                    chkSeqCh8EnableVisible = Visibility.Collapsed;
                }
            }
        }

        private TestApxItem parentTestApxItemValue = null;
        public TestApxItem ParentTestApxItem
        {
            get { return parentTestApxItemValue; }
            set { parentTestApxItemValue = value; OnPropertyChanged("ParentTestApxItem"); }
        }
    }

    public partial class APXLevelandGain_Verification : INotifyPropertyChanged
    {
        public APXLevelandGain_Verification()
        {
            if (!cmbGainWaveformList.Contains("Sine"))
                cmbGainWaveformList.Add("Sine");
            if (!cmbGainWaveformList.Contains("Sine, Dual"))
                cmbGainWaveformList.Add("Sine, Dual");
            if (!cmbGainWaveformList.Contains("Sine, Var Phase"))
                cmbGainWaveformList.Add("Sine, Var Phase");
            if (!cmbGainWaveformList.Contains("Browse for file..."))
                cmbGainWaveformList.Add("Browse for file...");
        }

        public event PropertyChangedEventHandler PropertyChanged;
        void OnPropertyChanged(string property)
        {
            try
            {
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs(property));
                    if (ParentTestApxItem != null && ParentTestApxItem.ParentTestActionItem != null && parentTestApxItemValue.ParentTestActionItem.ParentTestCaseItem != null && ParentTestApxItem.ParentTestActionItem.ParentTestCaseItem.SaveButtonIsEnabled != true)
                        ParentTestApxItem.ParentTestActionItem.ParentTestCaseItem.SaveButtonIsEnabled = true;
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

        private string ChkGainGenONContentValue = "Generator Off ";
        public string ChkGainGenONContent
        {
            get { return ChkGainGenONContentValue; }
            set
            {
                ChkGainGenONContentValue = value;
                OnPropertyChanged("ChkGainGenONContent");
            }
        }

        private bool ChkGainGenONValue = true;
        public bool ChkGainGenON
        {
            get { return ChkGainGenONValue; }
            set
            {
                ChkGainGenONValue = value;
                if (ChkGainGenONValue == false)
                    ChkGainGenONContent = "Generator Off ";
                else
                    ChkGainGenONContent = "Generator On  ";
                OnPropertyChanged("ChkGainGenON");
            }
        }

        private string BrowseGainWaveformValue = null;
        public string BrowseGainWaveform
        {
            get { return BrowseGainWaveformValue; }
            set
            {
                BrowseGainWaveformValue = value;
                OnPropertyChanged("BrowseGainWaveform");
            }
        }

        private ObservableCollection<string> cmbGainWaveformListValue = new ObservableCollection<string>();
        public ObservableCollection<string> cmbGainWaveformList
        {
            get { return cmbGainWaveformListValue; }
            set { cmbGainWaveformListValue = value; OnPropertyChanged("cmbGainWaveformList"); }
        }

        private string cmb_PreviousGainWaveformValue = null;
        public string cmb_PreviousGainWaveform
        {
            get { return cmb_PreviousGainWaveformValue; }
            set { cmb_PreviousGainWaveformValue = value; }
        }

        private string CmbGainWaveformValue = null;
        public string cmb_GainWaveform
        {
            get { return CmbGainWaveformValue; }
            set
            {
                cmb_PreviousGainWaveform = CmbGainWaveformValue;
                CmbGainWaveformValue = value;
                OnPropertyChanged("cmb_GainWaveform");
                if (CmbGainWaveformValue == "Sine, Dual")
                {
                    txtGainFreqBVisible = Visibility.Visible;
                    lblGainFreqBVisible = Visibility.Visible;
                }
                else
                {
                    txtGainFreqBVisible = Visibility.Collapsed;
                    lblGainFreqBVisible = Visibility.Collapsed;
                }
            }
        }

        private List<string> WaveFilePathListValue = new List<string>();
        public List<string> WaveFilePathList
        {
            get { return WaveFilePathListValue; }
            set
            {
                WaveFilePathListValue = value;
                OnPropertyChanged("WaveFilePathList");
            }
        }

        private Dictionary<string, bool> isNewWaveformValue = new Dictionary<string, bool>();
        public Dictionary<string, bool> isNewWaveform
        {
            get { return isNewWaveformValue; }
            set
            {
                isNewWaveformValue = value;
                OnPropertyChanged("isNewWaveform");
            }
        }

        private string _GainWaveFormSelectedFile = string.Empty;
        public string GainWaveFormSelectedFile
        {
            get
            {
                return _GainWaveFormSelectedFile;
            }
            set
            {
                _GainWaveFormSelectedFile = value;
                OnPropertyChanged("GainWaveFormSelectedFile");
            }
        }

        private bool ChkGainLevelTrackChValue = true;
        public bool chkBx_GainLevelTrackCh
        {
            get { return ChkGainLevelTrackChValue; }
            set
            {
                ChkGainLevelTrackChValue = value;
                OnPropertyChanged("chkBx_GainLevelTrackCh");

                if (SeqGainSetupCount > 1)
                {
                    if (!value)
                    {
                        lblGainCh2Visible = Visibility.Visible;
                        TxtGainLevelCh2Visible = Visibility.Visible;
                        TxtGainDCOffsetCh2Visible = Visibility.Visible;
                    }
                    else
                    {
                        lblGainCh2Visible = Visibility.Collapsed;
                        TxtGainLevelCh2Visible = Visibility.Collapsed;
                        TxtGainDCOffsetCh2Visible = Visibility.Collapsed;
                    }
                }

                if (SeqGainSetupCount > 2)
                {
                    if (!value)
                    {
                        lblGainCh3Visible = Visibility.Visible;
                        TxtGainLevelCh3Visible = Visibility.Visible;
                        TxtGainDCOffsetCh3Visible = Visibility.Visible;
                    }
                    else
                    {
                        lblGainCh3Visible = Visibility.Collapsed;
                        TxtGainLevelCh3Visible = Visibility.Collapsed;
                        TxtGainDCOffsetCh3Visible = Visibility.Collapsed;
                    }
                }

                if (SeqGainSetupCount > 3)
                {
                    if (!value)
                    {
                        lblGainCh4Visible = Visibility.Visible;
                        TxtGainLevelCh4Visible = Visibility.Visible;
                        TxtGainDCOffsetCh4Visible = Visibility.Visible;
                    }
                    else
                    {
                        lblGainCh4Visible = Visibility.Collapsed;
                        TxtGainLevelCh4Visible = Visibility.Collapsed;
                        TxtGainDCOffsetCh4Visible = Visibility.Collapsed;
                    }
                }

                if (SeqGainSetupCount > 4)
                {
                    if (!value)
                    {
                        lblGainCh5Visible = Visibility.Visible;
                        TxtGainLevelCh5Visible = Visibility.Visible;
                        TxtGainDCOffsetCh5Visible = Visibility.Visible;
                    }
                    else
                    {
                        lblGainCh5Visible = Visibility.Collapsed;
                        TxtGainLevelCh5Visible = Visibility.Collapsed;
                        TxtGainDCOffsetCh5Visible = Visibility.Collapsed;
                    }
                }

                if (SeqGainSetupCount > 5)
                {
                    if (!value)
                    {
                        lblGainCh6Visible = Visibility.Visible;
                        TxtGainLevelCh6Visible = Visibility.Visible;
                        TxtGainDCOffsetCh6Visible = Visibility.Visible;
                    }
                    else
                    {
                        lblGainCh6Visible = Visibility.Collapsed;
                        TxtGainLevelCh6Visible = Visibility.Collapsed;
                        TxtGainDCOffsetCh6Visible = Visibility.Collapsed;
                    }
                }

                if (SeqGainSetupCount > 6)
                {
                    if (!value)
                    {
                        lblGainCh7Visible = Visibility.Visible;
                        TxtGainLevelCh7Visible = Visibility.Visible;
                        TxtGainDCOffsetCh7Visible = Visibility.Visible;
                    }
                    else
                    {
                        lblGainCh7Visible = Visibility.Collapsed;
                        TxtGainLevelCh7Visible = Visibility.Collapsed;
                        TxtGainDCOffsetCh7Visible = Visibility.Collapsed;
                    }
                }

                if (SeqGainSetupCount > 7)
                {
                    if (!value)
                    {
                        lblGainCh8Visible = Visibility.Visible;
                        TxtGainLevelCh8Visible = Visibility.Visible;
                        TxtGainDCOffsetCh8Visible = Visibility.Visible;
                    }
                    else
                    {
                        lblGainCh8Visible = Visibility.Collapsed;
                        TxtGainLevelCh8Visible = Visibility.Collapsed;
                        TxtGainDCOffsetCh8Visible = Visibility.Collapsed;
                    }
                }
            }
        }

        private int SeqGainSetupCountValue = 0;
        public int SeqGainSetupCount
        {
            get { return SeqGainSetupCountValue; }
            set
            {
                SeqGainSetupCountValue = value;
                OnPropertyChanged("SeqGainSetupCount");

                if (value > 0)
                {
                    chkGainCh1EnableVisible = Visibility.Visible;
                    //TxtGainCh1Visible = Visibility.Visible;
                    //lblGainVerifyCh1Visible = Visibility.Visible;
                    //TxtUpToleranceGainCh1Visible = Visibility.Visible;
                    //TxtLowToleranceGainCh1Visible = Visibility.Visible;
                }
                else
                {
                    chkGainCh1EnableVisible = Visibility.Collapsed;
                    //TxtGainCh1Visible = Visibility.Collapsed;
                    //lblGainVerifyCh1Visible = Visibility.Collapsed;
                    //TxtUpToleranceGainCh1Visible = Visibility.Collapsed;
                    //TxtLowToleranceGainCh1Visible = Visibility.Collapsed;
                }

                if (value > 1)
                {
                    chkGainCh2EnableVisible = Visibility.Visible;
                    //TxtGainCh2Visible = Visibility.Visible;
                    //lblGainVerifyCh2Visible = Visibility.Visible;
                    //TxtUpToleranceGainCh2Visible = Visibility.Visible;
                    //TxtLowToleranceGainCh2Visible = Visibility.Visible;
                }
                else
                {
                    chkGainCh2EnableVisible = Visibility.Collapsed;
                    //TxtGainCh2Visible = Visibility.Collapsed;
                    //lblGainVerifyCh2Visible = Visibility.Collapsed;
                    //TxtUpToleranceGainCh2Visible = Visibility.Collapsed;
                    //TxtLowToleranceGainCh2Visible = Visibility.Collapsed;
                }

                if (value > 2)
                {
                    chkGainCh3EnableVisible = Visibility.Visible;
                    //TxtGainCh3Visible = Visibility.Visible;
                    //lblGainVerifyCh3Visible = Visibility.Visible;
                    //TxtUpToleranceGainCh3Visible = Visibility.Visible;
                    //TxtLowToleranceGainCh3Visible = Visibility.Visible;
                }
                else
                {
                    chkGainCh3EnableVisible = Visibility.Collapsed;
                    //TxtGainCh3Visible = Visibility.Collapsed;
                    //lblGainVerifyCh3Visible = Visibility.Collapsed;
                    //TxtUpToleranceGainCh3Visible = Visibility.Collapsed;
                    //TxtLowToleranceGainCh3Visible = Visibility.Collapsed;
                }

                if (value > 3)
                {
                    chkGainCh4EnableVisible = Visibility.Visible;
                    //TxtGainCh4Visible = Visibility.Visible;
                    //lblGainVerifyCh4Visible = Visibility.Visible;
                    //TxtUpToleranceGainCh4Visible = Visibility.Visible;
                    //TxtLowToleranceGainCh4Visible = Visibility.Visible;
                }
                else
                {
                    chkGainCh4EnableVisible = Visibility.Collapsed;
                    //TxtGainCh4Visible = Visibility.Collapsed;
                    //lblGainVerifyCh4Visible = Visibility.Collapsed;
                    //TxtUpToleranceGainCh4Visible = Visibility.Collapsed;
                    //TxtLowToleranceGainCh4Visible = Visibility.Collapsed;
                }

                if (value > 4)
                {
                    chkGainCh5EnableVisible = Visibility.Visible;
                    //TxtGainCh5Visible = Visibility.Visible;
                    //lblGainVerifyCh5Visible = Visibility.Visible;
                    //TxtUpToleranceGainCh5Visible = Visibility.Visible;
                    //TxtLowToleranceGainCh5Visible = Visibility.Visible;
                }
                else
                {
                    chkGainCh5EnableVisible = Visibility.Collapsed;
                    //TxtGainCh5Visible = Visibility.Collapsed;
                    //lblGainVerifyCh5Visible = Visibility.Collapsed;
                    //TxtUpToleranceGainCh5Visible = Visibility.Collapsed;
                    //TxtLowToleranceGainCh5Visible = Visibility.Collapsed;
                }

                if (value > 5)
                {
                    chkGainCh6EnableVisible = Visibility.Visible;
                    //TxtGainCh6Visible = Visibility.Visible;
                    //lblGainVerifyCh6Visible = Visibility.Visible;
                    //TxtUpToleranceGainCh6Visible = Visibility.Visible;
                    //TxtLowToleranceGainCh6Visible = Visibility.Visible;
                }
                else
                {
                    chkGainCh6EnableVisible = Visibility.Collapsed;
                    //TxtGainCh6Visible = Visibility.Collapsed;
                    //lblGainVerifyCh6Visible = Visibility.Collapsed;
                    //TxtUpToleranceGainCh6Visible = Visibility.Collapsed;
                    //TxtLowToleranceGainCh6Visible = Visibility.Collapsed;
                }

                if (value > 6)
                {
                    chkGainCh7EnableVisible = Visibility.Visible;
                    //TxtGainCh7Visible = Visibility.Visible;
                    //lblGainVerifyCh7Visible = Visibility.Visible;
                    //TxtUpToleranceGainCh7Visible = Visibility.Visible;
                    //TxtLowToleranceGainCh7Visible = Visibility.Visible;
                }
                else
                {
                    chkGainCh7EnableVisible = Visibility.Collapsed;
                    //TxtGainCh7Visible = Visibility.Collapsed;
                    //lblGainVerifyCh7Visible = Visibility.Collapsed;
                    //TxtUpToleranceGainCh7Visible = Visibility.Collapsed;
                    //TxtLowToleranceGainCh7Visible = Visibility.Collapsed;
                }

                if (value > 7)
                {
                    chkGainCh8EnableVisible = Visibility.Visible;
                    //TxtGainCh8Visible = Visibility.Visible;
                    //lblGainVerifyCh8Visible = Visibility.Visible;
                    //TxtUpToleranceGainCh8Visible = Visibility.Visible;
                    //TxtLowToleranceGainCh8Visible = Visibility.Visible;
                }
                else
                {
                    chkGainCh8EnableVisible = Visibility.Collapsed;
                    //TxtGainCh8Visible = Visibility.Collapsed;
                    //lblGainVerifyCh8Visible = Visibility.Collapsed;
                    //TxtUpToleranceGainCh8Visible = Visibility.Collapsed;
                    //TxtLowToleranceGainCh8Visible = Visibility.Collapsed;
                }
            }
        }

        private int GainInputChCountValue = 0;
        public int GainInputChCount
        {
            get { return GainInputChCountValue; }
            set
            {
                GainInputChCountValue = value;
                OnPropertyChanged("GainInputChCount");

                if (value > 0)
                {
                    TxtGainCh1Visible = Visibility.Visible;
                    lblGainVerifyCh1Visible = Visibility.Visible;
                    TxtUpToleranceGainCh1Visible = Visibility.Visible;
                    TxtLowToleranceGainCh1Visible = Visibility.Visible;
                }
                else
                {
                    TxtGainCh1Visible = Visibility.Collapsed;
                    lblGainVerifyCh1Visible = Visibility.Collapsed;
                    TxtUpToleranceGainCh1Visible = Visibility.Collapsed;
                    TxtLowToleranceGainCh1Visible = Visibility.Collapsed;
                }

                if (value > 1)
                {
                    TxtGainCh2Visible = Visibility.Visible;
                    lblGainVerifyCh2Visible = Visibility.Visible;
                    TxtUpToleranceGainCh2Visible = Visibility.Visible;
                    TxtLowToleranceGainCh2Visible = Visibility.Visible;
                }
                else
                {
                    TxtGainCh2Visible = Visibility.Collapsed;
                    lblGainVerifyCh2Visible = Visibility.Collapsed;
                    TxtUpToleranceGainCh2Visible = Visibility.Collapsed;
                    TxtLowToleranceGainCh2Visible = Visibility.Collapsed;
                }

                if (value > 2)
                {
                    TxtGainCh3Visible = Visibility.Visible;
                    lblGainVerifyCh3Visible = Visibility.Visible;
                    TxtUpToleranceGainCh3Visible = Visibility.Visible;
                    TxtLowToleranceGainCh3Visible = Visibility.Visible;
                }
                else
                {
                    TxtGainCh3Visible = Visibility.Collapsed;
                    lblGainVerifyCh3Visible = Visibility.Collapsed;
                    TxtUpToleranceGainCh3Visible = Visibility.Collapsed;
                    TxtLowToleranceGainCh3Visible = Visibility.Collapsed;
                }

                if (value > 3)
                {
                    TxtGainCh4Visible = Visibility.Visible;
                    lblGainVerifyCh4Visible = Visibility.Visible;
                    TxtUpToleranceGainCh4Visible = Visibility.Visible;
                    TxtLowToleranceGainCh4Visible = Visibility.Visible;
                }
                else
                {
                    TxtGainCh4Visible = Visibility.Collapsed;
                    lblGainVerifyCh4Visible = Visibility.Collapsed;
                    TxtUpToleranceGainCh4Visible = Visibility.Collapsed;
                    TxtLowToleranceGainCh4Visible = Visibility.Collapsed;
                }

                if (value > 4)
                {
                    TxtGainCh5Visible = Visibility.Visible;
                    lblGainVerifyCh5Visible = Visibility.Visible;
                    TxtUpToleranceGainCh5Visible = Visibility.Visible;
                    TxtLowToleranceGainCh5Visible = Visibility.Visible;
                }
                else
                {
                    TxtGainCh5Visible = Visibility.Collapsed;
                    lblGainVerifyCh5Visible = Visibility.Collapsed;
                    TxtUpToleranceGainCh5Visible = Visibility.Collapsed;
                    TxtLowToleranceGainCh5Visible = Visibility.Collapsed;
                }

                if (value > 5)
                {
                    TxtGainCh6Visible = Visibility.Visible;
                    lblGainVerifyCh6Visible = Visibility.Visible;
                    TxtUpToleranceGainCh6Visible = Visibility.Visible;
                    TxtLowToleranceGainCh6Visible = Visibility.Visible;
                }
                else
                {
                    TxtGainCh6Visible = Visibility.Collapsed;
                    lblGainVerifyCh6Visible = Visibility.Collapsed;
                    TxtUpToleranceGainCh6Visible = Visibility.Collapsed;
                    TxtLowToleranceGainCh6Visible = Visibility.Collapsed;
                }

                if (value > 6)
                {
                    TxtGainCh7Visible = Visibility.Visible;
                    lblGainVerifyCh7Visible = Visibility.Visible;
                    TxtUpToleranceGainCh7Visible = Visibility.Visible;
                    TxtLowToleranceGainCh7Visible = Visibility.Visible;
                }
                else
                {
                    TxtGainCh7Visible = Visibility.Collapsed;
                    lblGainVerifyCh7Visible = Visibility.Collapsed;
                    TxtUpToleranceGainCh7Visible = Visibility.Collapsed;
                    TxtLowToleranceGainCh7Visible = Visibility.Collapsed;
                }

                if (value > 7)
                {
                    TxtGainCh8Visible = Visibility.Visible;
                    lblGainVerifyCh8Visible = Visibility.Visible;
                    TxtUpToleranceGainCh8Visible = Visibility.Visible;
                    TxtLowToleranceGainCh8Visible = Visibility.Visible;
                }
                else
                {
                    TxtGainCh8Visible = Visibility.Collapsed;
                    lblGainVerifyCh8Visible = Visibility.Collapsed;
                    TxtUpToleranceGainCh8Visible = Visibility.Collapsed;
                    TxtLowToleranceGainCh8Visible = Visibility.Collapsed;
                }
            }
        }

        private string TxtGainCh1LevelValue = null;
        public string txt_GainCh1Level
        {
            get { return TxtGainCh1LevelValue; }
            set
            {
                TxtGainCh1LevelValue = value;
                OnPropertyChanged("txt_GainCh1Level");
            }
        }

        private string TxtGainCh2LevelValue = null;
        public string txt_GainCh2Level
        {
            get { return TxtGainCh2LevelValue; }
            set
            {
                TxtGainCh2LevelValue = value;
                OnPropertyChanged("txt_GainCh2Level");
            }
        }

        private string TxtGainCh3LevelValue = null;
        public string txt_GainCh3Level
        {
            get { return TxtGainCh3LevelValue; }
            set
            {
                TxtGainCh3LevelValue = value;
                OnPropertyChanged("txt_GainCh3Level");
            }
        }

        private string TxtGainCh4LevelValue = null;
        public string txt_GainCh4Level
        {
            get { return TxtGainCh4LevelValue; }
            set
            {
                TxtGainCh4LevelValue = value;
                OnPropertyChanged("txt_GainCh4Level");
            }
        }

        private string TxtGainCh5LevelValue = null;
        public string txt_GainCh5Level
        {
            get { return TxtGainCh5LevelValue; }
            set
            {
                TxtGainCh5LevelValue = value;
                OnPropertyChanged("txt_GainCh5Level");
            }
        }

        private string TxtGainCh6LevelValue = null;
        public string txt_GainCh6Level
        {
            get { return TxtGainCh6LevelValue; }
            set
            {
                TxtGainCh6LevelValue = value;
                OnPropertyChanged("txt_GainCh6Level");
            }
        }

        private string TxtGainCh7LevelValue = null;
        public string txt_GainCh7Level
        {
            get { return TxtGainCh7LevelValue; }
            set
            {
                TxtGainCh7LevelValue = value;
                OnPropertyChanged("txt_GainCh7Level");
            }
        }

        private string TxtGainCh8LevelValue = null;
        public string txt_GainCh8Level
        {
            get { return TxtGainCh8LevelValue; }
            set
            {
                TxtGainCh8LevelValue = value;
                OnPropertyChanged("txt_GainCh8Level");
            }
        }

        private string TxtGainDCCh1OffsetValue = null;
        public string txt_GainDcCh1Offset
        {
            get { return TxtGainDCCh1OffsetValue; }
            set
            {
                TxtGainDCCh1OffsetValue = value;
                OnPropertyChanged("txt_GainDcCh1Offset");
            }
        }

        private string TxtGainDCCh2OffsetValue = null;
        public string txt_GainDcCh2Offset
        {
            get { return TxtGainDCCh2OffsetValue; }
            set
            {
                TxtGainDCCh2OffsetValue = value;
                OnPropertyChanged("txt_GainDcCh2Offset");
            }
        }

        private string TxtGainDCCh3OffsetValue = null;
        public string txt_GainDcCh3Offset
        {
            get { return TxtGainDCCh3OffsetValue; }
            set
            {
                TxtGainDCCh3OffsetValue = value;
                OnPropertyChanged("txt_GainDcCh3Offset");
            }
        }

        private string TxtGainDCCh4OffsetValue = null;
        public string txt_GainDcCh4Offset
        {
            get { return TxtGainDCCh4OffsetValue; }
            set
            {
                TxtGainDCCh4OffsetValue = value;
                OnPropertyChanged("txt_GainDcCh4Offset");
            }
        }

        private string TxtGainDCCh5OffsetValue = null;
        public string txt_GainDcCh5Offset
        {
            get { return TxtGainDCCh5OffsetValue; }
            set
            {
                TxtGainDCCh5OffsetValue = value;
                OnPropertyChanged("txt_GainDcCh5Offset");
            }
        }

        private string TxtGainDCCh6OffsetValue = null;
        public string txt_GainDcCh6Offset
        {
            get { return TxtGainDCCh6OffsetValue; }
            set
            {
                TxtGainDCCh6OffsetValue = value;
                OnPropertyChanged("txt_GainDcCh6Offset");
            }
        }

        private string TxtGainDCCh7OffsetValue = null;
        public string txt_GainDcCh7Offset
        {
            get { return TxtGainDCCh7OffsetValue; }
            set
            {
                TxtGainDCCh7OffsetValue = value;
                OnPropertyChanged("txt_GainDcCh7Offset");
            }
        }

        private string TxtGainDCCh8OffsetValue = null;
        public string txt_GainDcCh8Offset
        {
            get { return TxtGainDCCh8OffsetValue; }
            set
            {
                TxtGainDCCh8OffsetValue = value;
                OnPropertyChanged("txt_GainDcCh8Offset");
            }
        }

        private string TxtGainfrequencyAValue = null;
        public string txt_GainfrequencyA
        {
            get { return TxtGainfrequencyAValue; }
            set
            {
                TxtGainfrequencyAValue = value;
                OnPropertyChanged("txt_GainfrequencyA");
            }
        }

        private string TxtGainfrequencyBValue = null;
        public string txt_GainfrequencyB
        {
            get { return TxtGainfrequencyBValue; }
            set
            {
                TxtGainfrequencyBValue = value;
                OnPropertyChanged("txt_GainfrequencyB");
            }
        }

        private Visibility txtGainFreqBVisibleValue = Visibility.Collapsed;
        public Visibility txtGainFreqBVisible
        {
            get { return txtGainFreqBVisibleValue; }
            set
            {
                txtGainFreqBVisibleValue = value;
                OnPropertyChanged("txtGainFreqBVisible");
            }
        }

        private Visibility lblGainFreqBVisibleValue = Visibility.Collapsed;
        public Visibility lblGainFreqBVisible
        {
            get { return lblGainFreqBVisibleValue; }
            set
            {
                lblGainFreqBVisibleValue = value;
                OnPropertyChanged("lblGainFreqBVisible");
            }
        }

        private string TxtGainCh1Value = null;
        public string TxtGainCh1
        {
            get { return TxtGainCh1Value; }
            set
            {
                TxtGainCh1Value = value;
                OnPropertyChanged("TxtGainCh1");
            }
        }

        private string TxtGainCh2Value = null;
        public string TxtGainCh2
        {
            get { return TxtGainCh2Value; }
            set
            {
                TxtGainCh2Value = value;
                OnPropertyChanged("TxtGainCh2");
            }
        }
        private string TxtGainCh3Value = null;
        public string TxtGainCh3
        {
            get { return TxtGainCh3Value; }
            set
            {
                TxtGainCh3Value = value;
                OnPropertyChanged("TxtGainCh3");
            }
        }
        private string TxtGainCh4Value = null;
        public string TxtGainCh4
        {
            get { return TxtGainCh4Value; }
            set
            {
                TxtGainCh4Value = value;
                OnPropertyChanged("TxtGainCh4");
            }
        }
        private string TxtGainCh5Value = null;
        public string TxtGainCh5
        {
            get { return TxtGainCh5Value; }
            set
            {
                TxtGainCh5Value = value;
                OnPropertyChanged("TxtGainCh5");
            }
        }
        private string TxtGainCh6Value = null;
        public string TxtGainCh6
        {
            get { return TxtGainCh6Value; }
            set
            {
                TxtGainCh6Value = value;
                OnPropertyChanged("TxtGainCh6");
            }
        }
        private string TxtGainCh7Value = null;
        public string TxtGainCh7
        {
            get { return TxtGainCh7Value; }
            set
            {
                TxtGainCh7Value = value;
                OnPropertyChanged("TxtGainCh7");
            }
        }
        private string TxtGainCh8Value = null;
        public string TxtGainCh8
        {
            get { return TxtGainCh8Value; }
            set
            {
                TxtGainCh8Value = value;
                OnPropertyChanged("TxtGainCh8");
            }
        }

        private Visibility TxtGainCh1VisibleValue = Visibility.Collapsed;
        public Visibility TxtGainCh1Visible
        {
            get { return TxtGainCh1VisibleValue; }
            set
            {
                TxtGainCh1VisibleValue = value;
                OnPropertyChanged("TxtGainCh1Visible");
            }
        }

        private Visibility TxtGainCh2VisibleValue = Visibility.Collapsed;
        public Visibility TxtGainCh2Visible
        {
            get { return TxtGainCh2VisibleValue; }
            set
            {
                TxtGainCh2VisibleValue = value;
                OnPropertyChanged("TxtGainCh2Visible");
            }
        }

        private Visibility TxtGainCh3VisibleValue = Visibility.Collapsed;
        public Visibility TxtGainCh3Visible
        {
            get { return TxtGainCh3VisibleValue; }
            set
            {
                TxtGainCh3VisibleValue = value;
                OnPropertyChanged("TxtGainCh3Visible");
            }
        }

        private Visibility TxtGainCh4VisibleValue = Visibility.Collapsed;
        public Visibility TxtGainCh4Visible
        {
            get { return TxtGainCh4VisibleValue; }
            set
            {
                TxtGainCh4VisibleValue = value;
                OnPropertyChanged("TxtGainCh4Visible");
            }
        }

        private Visibility TxtGainCh5VisibleValue = Visibility.Collapsed;
        public Visibility TxtGainCh5Visible
        {
            get { return TxtGainCh5VisibleValue; }
            set
            {
                TxtGainCh5VisibleValue = value;
                OnPropertyChanged("TxtGainCh5Visible");
            }
        }

        private Visibility TxtGainCh6VisibleValue = Visibility.Collapsed;
        public Visibility TxtGainCh6Visible
        {
            get { return TxtGainCh6VisibleValue; }
            set
            {
                TxtGainCh6VisibleValue = value;
                OnPropertyChanged("TxtGainCh6Visible");
            }
        }


        private Visibility TxtGainCh7VisibleValue = Visibility.Collapsed;
        public Visibility TxtGainCh7Visible
        {
            get { return TxtGainCh7VisibleValue; }
            set
            {
                TxtGainCh7VisibleValue = value;
                OnPropertyChanged("TxtGainCh7Visible");
            }
        }

        private Visibility TxtGainCh8VisibleValue = Visibility.Collapsed;
        public Visibility TxtGainCh8Visible
        {
            get { return TxtGainCh8VisibleValue; }
            set
            {
                TxtGainCh8VisibleValue = value;
                OnPropertyChanged("TxtGainCh8Visible");
            }
        }

        private Visibility lblGainVerifyCh1VisibleValue = Visibility.Collapsed;
        public Visibility lblGainVerifyCh1Visible
        {
            get { return lblGainVerifyCh1VisibleValue; }
            set
            {
                lblGainVerifyCh1VisibleValue = value;
                OnPropertyChanged("lblGainVerifyCh1Visible");
            }
        }

        private Visibility lblGainVerifyCh2VisibleValue = Visibility.Collapsed;
        public Visibility lblGainVerifyCh2Visible
        {
            get { return lblGainVerifyCh2VisibleValue; }
            set
            {
                lblGainVerifyCh2VisibleValue = value;
                OnPropertyChanged("lblGainVerifyCh2Visible");
            }
        }

        private Visibility lblGainVerifyCh3VisibleValue = Visibility.Collapsed;
        public Visibility lblGainVerifyCh3Visible
        {
            get { return lblGainVerifyCh3VisibleValue; }
            set
            {
                lblGainVerifyCh3VisibleValue = value;
                OnPropertyChanged("lblGainVerifyCh3Visible");
            }
        }

        private Visibility lblGainVerifyCh4VisibleValue = Visibility.Collapsed;
        public Visibility lblGainVerifyCh4Visible
        {
            get { return lblGainVerifyCh4VisibleValue; }
            set
            {
                lblGainVerifyCh4VisibleValue = value;
                OnPropertyChanged("lblGainVerifyCh4Visible");
            }
        }

        private Visibility lblGainVerifyCh5VisibleValue = Visibility.Collapsed;
        public Visibility lblGainVerifyCh5Visible
        {
            get { return lblGainVerifyCh5VisibleValue; }
            set
            {
                lblGainVerifyCh5VisibleValue = value;
                OnPropertyChanged("lblGainVerifyCh5Visible");
            }
        }

        private Visibility lblGainVerifyCh6VisibleValue = Visibility.Collapsed;
        public Visibility lblGainVerifyCh6Visible
        {
            get { return lblGainVerifyCh6VisibleValue; }
            set
            {
                lblGainVerifyCh6VisibleValue = value;
                OnPropertyChanged("lblGainVerifyCh6Visible");
            }
        }

        private Visibility lblGainVerifyCh7VisibleValue = Visibility.Collapsed;
        public Visibility lblGainVerifyCh7Visible
        {
            get { return lblGainVerifyCh7VisibleValue; }
            set
            {
                lblGainVerifyCh7VisibleValue = value;
                OnPropertyChanged("lblGainVerifyCh7Visible");
            }
        }

        private Visibility lblGainVerifyCh8VisibleValue = Visibility.Collapsed;
        public Visibility lblGainVerifyCh8Visible
        {
            get { return lblGainVerifyCh8VisibleValue; }
            set
            {
                lblGainVerifyCh8VisibleValue = value;
                OnPropertyChanged("lblGainVerifyCh8Visible");
            }
        }

        private string TxtUpToleranceGainCh1Value = null;
        public string TxtUpToleranceGainCh1
        {
            get { return TxtUpToleranceGainCh1Value; }
            set
            {
                TxtUpToleranceGainCh1Value = value;
                OnPropertyChanged("TxtUpToleranceGainCh1");
            }
        }

        private string TxtUpToleranceGainCh2Value = null;
        public string TxtUpToleranceGainCh2
        {
            get { return TxtUpToleranceGainCh2Value; }
            set
            {
                TxtUpToleranceGainCh2Value = value;
                OnPropertyChanged("TxtUpToleranceGainCh2");
            }
        }

        private string TxtUpToleranceGainCh3Value = null;
        public string TxtUpToleranceGainCh3
        {
            get { return TxtUpToleranceGainCh3Value; }
            set
            {
                TxtUpToleranceGainCh3Value = value;
                OnPropertyChanged("TxtUpToleranceGainCh3");
            }
        }

        private string TxtUpToleranceGainCh4Value = null;
        public string TxtUpToleranceGainCh4
        {
            get { return TxtUpToleranceGainCh4Value; }
            set
            {
                TxtUpToleranceGainCh4Value = value;
                OnPropertyChanged("TxtUpToleranceGainCh4");
            }
        }

        private string TxtUpToleranceGainCh5Value = null;
        public string TxtUpToleranceGainCh5
        {
            get { return TxtUpToleranceGainCh5Value; }
            set
            {
                TxtUpToleranceGainCh5Value = value;
                OnPropertyChanged("TxtUpToleranceGainCh5");
            }
        }

        private string TxtUpToleranceGainCh6Value = null;
        public string TxtUpToleranceGainCh6
        {
            get { return TxtUpToleranceGainCh6Value; }
            set
            {
                TxtUpToleranceGainCh6Value = value;
                OnPropertyChanged("TxtUpToleranceGainCh6");
            }
        }

        private string TxtUpToleranceGainCh7Value = null;
        public string TxtUpToleranceGainCh7
        {
            get { return TxtUpToleranceGainCh7Value; }
            set
            {
                TxtUpToleranceGainCh7Value = value;
                OnPropertyChanged("TxtUpToleranceGainCh7");
            }
        }

        private string TxtUpToleranceGainCh8Value = null;
        public string TxtUpToleranceGainCh8
        {
            get { return TxtUpToleranceGainCh8Value; }
            set
            {
                TxtUpToleranceGainCh8Value = value;
                OnPropertyChanged("TxtUpToleranceGainCh8");
            }
        }

        private string TxtLowToleranceGainCh1Value = null;
        public string TxtLowToleranceGainCh1
        {
            get { return TxtLowToleranceGainCh1Value; }
            set
            {
                TxtLowToleranceGainCh1Value = value;
                OnPropertyChanged("TxtLowToleranceGainCh1");
            }
        }

        private string TxtLowToleranceGainCh2Value = null;
        public string TxtLowToleranceGainCh2
        {
            get { return TxtLowToleranceGainCh2Value; }
            set
            {
                TxtLowToleranceGainCh2Value = value;
                OnPropertyChanged("TxtLowToleranceGainCh2");
            }
        }

        private string TxtLowToleranceGainCh3Value = null;
        public string TxtLowToleranceGainCh3
        {
            get { return TxtLowToleranceGainCh3Value; }
            set
            {
                TxtLowToleranceGainCh3Value = value;
                OnPropertyChanged("TxtLowToleranceGainCh3");
            }
        }

        private string TxtLowToleranceGainCh4Value = null;
        public string TxtLowToleranceGainCh4
        {
            get { return TxtLowToleranceGainCh4Value; }
            set
            {
                TxtLowToleranceGainCh4Value = value;
                OnPropertyChanged("TxtLowToleranceGainCh4");
            }
        }

        private string TxtLowToleranceGainCh5Value = null;
        public string TxtLowToleranceGainCh5
        {
            get { return TxtLowToleranceGainCh5Value; }
            set
            {
                TxtLowToleranceGainCh5Value = value;
                OnPropertyChanged("TxtLowToleranceGainCh5");
            }
        }

        private string TxtLowToleranceGainCh6Value = null;
        public string TxtLowToleranceGainCh6
        {
            get { return TxtLowToleranceGainCh6Value; }
            set
            {
                TxtLowToleranceGainCh6Value = value;
                OnPropertyChanged("TxtLowToleranceGainCh6");
            }
        }

        private string TxtLowToleranceGainCh7Value = null;
        public string TxtLowToleranceGainCh7
        {
            get { return TxtLowToleranceGainCh7Value; }
            set
            {
                TxtLowToleranceGainCh7Value = value;
                OnPropertyChanged("TxtLowToleranceGainCh7");
            }
        }

        private string TxtLowToleranceGainCh8Value = null;
        public string TxtLowToleranceGainCh8
        {
            get { return TxtLowToleranceGainCh8Value; }
            set
            {
                TxtLowToleranceGainCh8Value = value;
                OnPropertyChanged("TxtLowToleranceGainCh8");
            }
        }

        private Visibility TxtUpToleranceGainCh1VisibleValue = Visibility.Collapsed;
        public Visibility TxtUpToleranceGainCh1Visible
        {
            get { return TxtUpToleranceGainCh1VisibleValue; }
            set
            {
                TxtUpToleranceGainCh1VisibleValue = value;
                OnPropertyChanged("TxtUpToleranceGainCh1Visible");
            }
        }

        private Visibility TxtUpToleranceGainCh2VisibleValue = Visibility.Collapsed;
        public Visibility TxtUpToleranceGainCh2Visible
        {
            get { return TxtUpToleranceGainCh2VisibleValue; }
            set
            {
                TxtUpToleranceGainCh2VisibleValue = value;
                OnPropertyChanged("TxtUpToleranceGainCh2Visible");
            }
        }

        private Visibility TxtUpToleranceGainCh3VisibleValue = Visibility.Collapsed;
        public Visibility TxtUpToleranceGainCh3Visible
        {
            get { return TxtUpToleranceGainCh3VisibleValue; }
            set
            {
                TxtUpToleranceGainCh3VisibleValue = value;
                OnPropertyChanged("TxtUpToleranceGainCh3Visible");
            }
        }

        private Visibility TxtUpToleranceGainCh4VisibleValue = Visibility.Collapsed;
        public Visibility TxtUpToleranceGainCh4Visible
        {
            get { return TxtUpToleranceGainCh4VisibleValue; }
            set
            {
                TxtUpToleranceGainCh4VisibleValue = value;
                OnPropertyChanged("TxtUpToleranceGainCh4Visible");
            }
        }

        private Visibility TxtUpToleranceGainCh5VisibleValue = Visibility.Collapsed;
        public Visibility TxtUpToleranceGainCh5Visible
        {
            get { return TxtUpToleranceGainCh5VisibleValue; }
            set
            {
                TxtUpToleranceGainCh5VisibleValue = value;
                OnPropertyChanged("TxtUpToleranceGainCh5Visible");
            }
        }

        private Visibility TxtUpToleranceGainCh6VisibleValue = Visibility.Collapsed;
        public Visibility TxtUpToleranceGainCh6Visible
        {
            get { return TxtUpToleranceGainCh6VisibleValue; }
            set
            {
                TxtUpToleranceGainCh6VisibleValue = value;
                OnPropertyChanged("TxtUpToleranceGainCh6Visible");
            }
        }

        private Visibility TxtUpToleranceGainCh7VisibleValue = Visibility.Collapsed;
        public Visibility TxtUpToleranceGainCh7Visible
        {
            get { return TxtUpToleranceGainCh7VisibleValue; }
            set
            {
                TxtUpToleranceGainCh7VisibleValue = value;
                OnPropertyChanged("TxtUpToleranceGainCh7Visible");
            }
        }

        private Visibility TxtUpToleranceGainCh8VisibleValue = Visibility.Collapsed;
        public Visibility TxtUpToleranceGainCh8Visible
        {
            get { return TxtUpToleranceGainCh8VisibleValue; }
            set
            {
                TxtUpToleranceGainCh8VisibleValue = value;
                OnPropertyChanged("TxtUpToleranceGainCh8Visible");
            }
        }

        private Visibility TxtLowToleranceGainCh1VisibleValue = Visibility.Collapsed;
        public Visibility TxtLowToleranceGainCh1Visible
        {
            get { return TxtLowToleranceGainCh1VisibleValue; }
            set
            {
                TxtLowToleranceGainCh1VisibleValue = value;
                OnPropertyChanged("TxtLowToleranceGainCh1Visible");
            }
        }

        private Visibility TxtLowToleranceGainCh2VisibleValue = Visibility.Collapsed;
        public Visibility TxtLowToleranceGainCh2Visible
        {
            get { return TxtLowToleranceGainCh2VisibleValue; }
            set
            {
                TxtLowToleranceGainCh2VisibleValue = value;
                OnPropertyChanged("TxtLowToleranceGainCh2Visible");
            }
        }

        private Visibility TxtLowToleranceGainCh3VisibleValue = Visibility.Collapsed;
        public Visibility TxtLowToleranceGainCh3Visible
        {
            get { return TxtLowToleranceGainCh3VisibleValue; }
            set
            {
                TxtLowToleranceGainCh3VisibleValue = value;
                OnPropertyChanged("TxtLowToleranceGainCh3Visible");
            }
        }

        private Visibility TxtLowToleranceGainCh4VisibleValue = Visibility.Collapsed;
        public Visibility TxtLowToleranceGainCh4Visible
        {
            get { return TxtLowToleranceGainCh4VisibleValue; }
            set
            {
                TxtLowToleranceGainCh4VisibleValue = value;
                OnPropertyChanged("TxtLowToleranceGainCh4Visible");
            }
        }

        private Visibility TxtLowToleranceGainCh5VisibleValue = Visibility.Collapsed;
        public Visibility TxtLowToleranceGainCh5Visible
        {
            get { return TxtLowToleranceGainCh5VisibleValue; }
            set
            {
                TxtLowToleranceGainCh5VisibleValue = value;
                OnPropertyChanged("TxtLowToleranceGainCh5Visible");
            }
        }

        private Visibility TxtLowToleranceGainCh6VisibleValue = Visibility.Collapsed;
        public Visibility TxtLowToleranceGainCh6Visible
        {
            get { return TxtLowToleranceGainCh6VisibleValue; }
            set
            {
                TxtLowToleranceGainCh6VisibleValue = value;
                OnPropertyChanged("TxtLowToleranceGainCh6Visible");
            }
        }

        private Visibility TxtLowToleranceGainCh7VisibleValue = Visibility.Collapsed;
        public Visibility TxtLowToleranceGainCh7Visible
        {
            get { return TxtLowToleranceGainCh7VisibleValue; }
            set
            {
                TxtLowToleranceGainCh7VisibleValue = value;
                OnPropertyChanged("TxtLowToleranceGainCh7Visible");
            }
        }

        private Visibility TxtLowToleranceGainCh8VisibleValue = Visibility.Collapsed;
        public Visibility TxtLowToleranceGainCh8Visible
        {
            get { return TxtLowToleranceGainCh8VisibleValue; }
            set
            {
                TxtLowToleranceGainCh8VisibleValue = value;
                OnPropertyChanged("TxtLowToleranceGainCh1Visible");
            }
        }

        private bool BtnGainCh1Value = true;
        public bool btn_GainCh1
        {
            get { return BtnGainCh1Value; }
            set
            {
                BtnGainCh1Value = value;
                OnPropertyChanged("btn_GainCh1");
            }
        }

        private bool BtnGainCh2Value = true;
        public bool btn_GainCh2
        {
            get { return BtnGainCh2Value; }
            set
            {
                BtnGainCh2Value = value;
                OnPropertyChanged("btn_GainCh2");
            }
        }

        private bool BtnGainCh3Value = true;
        public bool btn_GainCh3
        {
            get { return BtnGainCh3Value; }
            set
            {
                BtnGainCh3Value = value;
                OnPropertyChanged("btn_GainCh3");
            }
        }

        private bool BtnGainCh4Value = true;
        public bool btn_GainCh4
        {
            get { return BtnGainCh4Value; }
            set
            {
                BtnGainCh4Value = value;
                OnPropertyChanged("btn_GainCh4");
            }
        }

        private bool BtnGainCh5Value = true;
        public bool btn_GainCh5
        {
            get { return BtnGainCh5Value; }
            set
            {
                BtnGainCh5Value = value;
                OnPropertyChanged("btn_GainCh5");
            }
        }

        private bool BtnGainCh6Value = true;
        public bool btn_GainCh6
        {
            get { return BtnGainCh6Value; }
            set
            {
                BtnGainCh6Value = value;
                OnPropertyChanged("btn_GainCh6");
            }
        }

        private bool BtnGainCh7Value = true;
        public bool btn_GainCh7
        {
            get { return BtnGainCh7Value; }
            set
            {
                BtnGainCh7Value = value;
                OnPropertyChanged("btn_GainCh7");
            }
        }

        private bool BtnGainCh8Value = true;
        public bool btn_GainCh8
        {
            get { return BtnGainCh8Value; }
            set
            {
                BtnGainCh8Value = value;
                OnPropertyChanged("btn_GainCh8");
            }
        }

        private Visibility lblGainCh2VisibleValue = Visibility.Collapsed;
        public Visibility lblGainCh2Visible
        {
            get { return lblGainCh2VisibleValue; }
            set
            {
                lblGainCh2VisibleValue = value;
                OnPropertyChanged("lblGainCh2Visible");
            }
        }

        private Visibility lblGainCh3VisibleValue = Visibility.Collapsed;
        public Visibility lblGainCh3Visible
        {
            get { return lblGainCh3VisibleValue; }
            set
            {
                lblGainCh3VisibleValue = value;
                OnPropertyChanged("lblGainCh3Visible");
            }
        }

        private Visibility lblGainCh4VisibleValue = Visibility.Collapsed;
        public Visibility lblGainCh4Visible
        {
            get { return lblGainCh4VisibleValue; }
            set
            {
                lblGainCh4VisibleValue = value;
                OnPropertyChanged("lblGainCh4Visible");
            }
        }

        private Visibility lblGainCh5VisibleValue = Visibility.Collapsed;
        public Visibility lblGainCh5Visible
        {
            get { return lblGainCh5VisibleValue; }
            set
            {
                lblGainCh5VisibleValue = value;
                OnPropertyChanged("lblGainCh5Visible");
            }
        }

        private Visibility lblGainCh6VisibleValue = Visibility.Collapsed;
        public Visibility lblGainCh6Visible
        {
            get { return lblGainCh6VisibleValue; }
            set
            {
                lblGainCh6VisibleValue = value;
                OnPropertyChanged("lblGainCh6Visible");
            }
        }

        private Visibility lblGainCh7VisibleValue = Visibility.Collapsed;
        public Visibility lblGainCh7Visible
        {
            get { return lblGainCh7VisibleValue; }
            set
            {
                lblGainCh7VisibleValue = value;
                OnPropertyChanged("lblGainCh7Visible");
            }
        }

        private Visibility lblGainCh8VisibleValue = Visibility.Collapsed;
        public Visibility lblGainCh8Visible
        {
            get { return lblGainCh8VisibleValue; }
            set
            {
                lblGainCh8VisibleValue = value;
                OnPropertyChanged("lblGainCh8Visible");
            }
        }

        private Visibility TxtGainLevelCh2VisibleValue = Visibility.Collapsed;
        public Visibility TxtGainLevelCh2Visible
        {
            get { return TxtGainLevelCh2VisibleValue; }
            set
            {
                TxtGainLevelCh2VisibleValue = value;
                OnPropertyChanged("TxtGainLevelCh2Visible");
            }
        }

        private Visibility TxtGainLevelCh3VisibleValue = Visibility.Collapsed;
        public Visibility TxtGainLevelCh3Visible
        {
            get { return TxtGainLevelCh3VisibleValue; }
            set
            {
                TxtGainLevelCh3VisibleValue = value;
                OnPropertyChanged("TxtGainLevelCh3Visible");
            }
        }

        private Visibility TxtGainLevelCh4VisibleValue = Visibility.Collapsed;
        public Visibility TxtGainLevelCh4Visible
        {
            get { return TxtGainLevelCh4VisibleValue; }
            set
            {
                TxtGainLevelCh4VisibleValue = value;
                OnPropertyChanged("TxtGainLevelCh4Visible");
            }
        }

        private Visibility TxtGainLevelCh5VisibleValue = Visibility.Collapsed;
        public Visibility TxtGainLevelCh5Visible
        {
            get { return TxtGainLevelCh5VisibleValue; }
            set
            {
                TxtGainLevelCh5VisibleValue = value;
                OnPropertyChanged("TxtGainLevelCh5Visible");
            }
        }

        private Visibility TxtGainLevelCh6VisibleValue = Visibility.Collapsed;
        public Visibility TxtGainLevelCh6Visible
        {
            get { return TxtGainLevelCh6VisibleValue; }
            set
            {
                TxtGainLevelCh6VisibleValue = value;
                OnPropertyChanged("TxtGainLevelCh6Visible");
            }
        }

        private Visibility TxtGainLevelCh7VisibleValue = Visibility.Collapsed;
        public Visibility TxtGainLevelCh7Visible
        {
            get { return TxtGainLevelCh7VisibleValue; }
            set
            {
                TxtGainLevelCh7VisibleValue = value;
                OnPropertyChanged("TxtGainLevelCh7Visible");
            }
        }

        private Visibility TxtGainLevelCh8VisibleValue = Visibility.Collapsed;
        public Visibility TxtGainLevelCh8Visible
        {
            get { return TxtGainLevelCh8VisibleValue; }
            set
            {
                TxtGainLevelCh8VisibleValue = value;
                OnPropertyChanged("TxtGainLevelCh8Visible");
            }
        }

        private Visibility TxtGainDCOffsetCh2VisibleValue = Visibility.Collapsed;
        public Visibility TxtGainDCOffsetCh2Visible
        {
            get { return TxtGainDCOffsetCh2VisibleValue; }
            set
            {
                TxtGainDCOffsetCh2VisibleValue = value;
                OnPropertyChanged("TxtGainDCOffsetCh2Visible");
            }
        }

        private Visibility TxtGainDCOffsetCh3VisibleValue = Visibility.Collapsed;
        public Visibility TxtGainDCOffsetCh3Visible
        {
            get { return TxtGainDCOffsetCh3VisibleValue; }
            set
            {
                TxtGainDCOffsetCh3VisibleValue = value;
                OnPropertyChanged("TxtGainDCOffsetCh3Visible");
            }
        }

        private Visibility TxtGainDCOffsetCh4VisibleValue = Visibility.Collapsed;
        public Visibility TxtGainDCOffsetCh4Visible
        {
            get { return TxtGainDCOffsetCh4VisibleValue; }
            set
            {
                TxtGainDCOffsetCh4VisibleValue = value;
                OnPropertyChanged("TxtGainDCOffsetCh4Visible");
            }
        }

        private Visibility TxtGainDCOffsetCh5VisibleValue = Visibility.Collapsed;
        public Visibility TxtGainDCOffsetCh5Visible
        {
            get { return TxtGainDCOffsetCh5VisibleValue; }
            set
            {
                TxtGainDCOffsetCh5VisibleValue = value;
                OnPropertyChanged("TxtGainDCOffsetCh5Visible");
            }
        }

        private Visibility TxtGainDCOffsetCh6VisibleValue = Visibility.Collapsed;
        public Visibility TxtGainDCOffsetCh6Visible
        {
            get { return TxtGainDCOffsetCh6VisibleValue; }
            set
            {
                TxtGainDCOffsetCh6VisibleValue = value;
                OnPropertyChanged("TxtGainDCOffsetCh6Visible");
            }
        }

        private Visibility TxtGainDCOffsetCh7VisibleValue = Visibility.Collapsed;
        public Visibility TxtGainDCOffsetCh7Visible
        {
            get { return TxtGainDCOffsetCh7VisibleValue; }
            set
            {
                TxtGainDCOffsetCh7VisibleValue = value;
                OnPropertyChanged("TxtGainDCOffsetCh7Visible");
            }
        }

        private Visibility TxtGainDCOffsetCh8VisibleValue = Visibility.Collapsed;
        public Visibility TxtGainDCOffsetCh8Visible
        {
            get { return TxtGainDCOffsetCh8VisibleValue; }
            set
            {
                TxtGainDCOffsetCh8VisibleValue = value;
                OnPropertyChanged("TxtGainDCOffsetCh8Visible");
            }
        }

        private Visibility chkGainCh1EnableVisibleValue = Visibility.Collapsed;
        public Visibility chkGainCh1EnableVisible
        {
            get { return chkGainCh1EnableVisibleValue; }
            set
            {
                chkGainCh1EnableVisibleValue = value;
                OnPropertyChanged("chkGainCh1EnableVisible");
            }
        }

        private Visibility chkGainCh2EnableVisibleValue = Visibility.Collapsed;
        public Visibility chkGainCh2EnableVisible
        {
            get { return chkGainCh2EnableVisibleValue; }
            set
            {
                chkGainCh2EnableVisibleValue = value;
                OnPropertyChanged("chkGainCh2EnableVisible");
            }
        }

        private Visibility chkGainCh3EnableVisibleValue = Visibility.Collapsed;
        public Visibility chkGainCh3EnableVisible
        {
            get { return chkGainCh3EnableVisibleValue; }
            set
            {
                chkGainCh3EnableVisibleValue = value;
                OnPropertyChanged("chkGainCh3EnableVisible");
            }
        }

        private Visibility chkGainCh4EnableVisibleValue = Visibility.Collapsed;
        public Visibility chkGainCh4EnableVisible
        {
            get { return chkGainCh4EnableVisibleValue; }
            set
            {
                chkGainCh4EnableVisibleValue = value;
                OnPropertyChanged("chkGainCh4EnableVisible");
            }
        }

        private Visibility chkGainCh5EnableVisibleValue = Visibility.Collapsed;
        public Visibility chkGainCh5EnableVisible
        {
            get { return chkGainCh5EnableVisibleValue; }
            set
            {
                chkGainCh5EnableVisibleValue = value;
                OnPropertyChanged("chkGainCh5EnableVisible");
            }
        }

        private Visibility chkGainCh6EnableVisibleValue = Visibility.Collapsed;
        public Visibility chkGainCh6EnableVisible
        {
            get { return chkGainCh6EnableVisibleValue; }
            set
            {
                chkGainCh6EnableVisibleValue = value;
                OnPropertyChanged("chkGainCh6EnableVisible");
            }
        }

        private Visibility chkGainCh7EnableVisibleValue = Visibility.Collapsed;
        public Visibility chkGainCh7EnableVisible
        {
            get { return chkGainCh7EnableVisibleValue; }
            set
            {
                chkGainCh7EnableVisibleValue = value;
                OnPropertyChanged("chkGainCh7EnableVisible");
            }
        }

        private Visibility chkGainCh8EnableVisibleValue = Visibility.Collapsed;
        public Visibility chkGainCh8EnableVisible
        {
            get { return chkGainCh8EnableVisibleValue; }
            set
            {
                chkGainCh8EnableVisibleValue = value;
                OnPropertyChanged("chkGainCh8EnableVisible");
            }
        }

        private TestApxItem parentTestApxItemValue = null;
        public TestApxItem ParentTestApxItem
        {
            get { return parentTestApxItemValue; }
            set { parentTestApxItemValue = value; OnPropertyChanged("ParentTestApxItem"); }
        }
    }

    public partial class APXFreqResponseVerification : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        void OnPropertyChanged(string property)
        {
            try
            {
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs(property));
                    if (ParentTestApxItem != null && ParentTestApxItem.ParentTestActionItem != null && parentTestApxItemValue.ParentTestActionItem.ParentTestCaseItem != null && ParentTestApxItem.ParentTestActionItem.ParentTestCaseItem.SaveButtonIsEnabled != true)
                        ParentTestApxItem.ParentTestActionItem.ParentTestCaseItem.SaveButtonIsEnabled = true;
                }
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                // MessageBox.Show("Exception\n " + ex.Message, "Error Code - ECxxxxx", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool ChkFreqGenONValue = true;
        public bool StartGenON
        {
            get { return ChkFreqGenONValue; }
            set
            {
                ChkFreqGenONValue = value;
                OnPropertyChanged("StartGenON");
            }
        }

        private string txtStartFreqValue = null;
        public string txtStartFreq
        {
            get { return txtStartFreqValue; }
            set
            {
                txtStartFreqValue = value;
                OnPropertyChanged("txtStartFreq");
            }
        }

        private string txtStopFreqValue = null;
        public string txtStopFreq
        {
            get { return txtStopFreqValue; }
            set
            {
                txtStopFreqValue = value;
                OnPropertyChanged("txtStopFreq");
            }
        }

        private string txtLevelValue = null;
        public string txtLevel
        {
            get { return txtLevelValue; }
            set
            {
                txtLevelValue = value;
                OnPropertyChanged("txtLevel");
            }
        }

        private bool IsEnableCh1Value = false;
        public bool IsEnableCh1
        {
            get { return IsEnableCh1Value; }
            set
            {
                IsEnableCh1Value = value;
                OnPropertyChanged("IsEnableCh1");
            }
        }

        private bool IsEnableCh2Value = false;
        public bool IsEnableCh2
        {
            get { return IsEnableCh2Value; }
            set
            {
                IsEnableCh2Value = value;
                OnPropertyChanged("IsEnableCh2");
            }
        }

        private bool IsEnableCh3Value = false;
        public bool IsEnableCh3
        {
            get { return IsEnableCh3Value; }
            set
            {
                IsEnableCh3Value = value;
                OnPropertyChanged("IsEnableCh3");
            }
        }

        private bool IsEnableCh4Value = false;
        public bool IsEnableCh4
        {
            get { return IsEnableCh4Value; }
            set
            {
                IsEnableCh4Value = value;
                OnPropertyChanged("IsEnableCh4");
            }
        }

        private bool IsEnableCh5Value = false;
        public bool IsEnableCh5
        {
            get { return IsEnableCh5Value; }
            set
            {
                IsEnableCh5Value = value;
                OnPropertyChanged("IsEnableCh5");
            }
        }

        private bool IsEnableCh6Value = false;
        public bool IsEnableCh6
        {
            get { return IsEnableCh6Value; }
            set
            {
                IsEnableCh6Value = value;
                OnPropertyChanged("IsEnableCh6");
            }
        }

        private bool IsEnableCh7Value = false;
        public bool IsEnableCh7
        {
            get { return IsEnableCh7Value; }
            set
            {
                IsEnableCh7Value = value;
                OnPropertyChanged("IsEnableCh7");
            }
        }

        private bool IsEnableCh8Value = false;
        public bool IsEnableCh8
        {
            get { return IsEnableCh8Value; }
            set
            {
                IsEnableCh8Value = value;
                OnPropertyChanged("IsEnableCh8");
            }
        }
        
        private bool isVerficationFileLoadedValue = false;
        public bool isVerficationFileLoaded
        {
            get { return isVerficationFileLoadedValue; }
            set
            {
                isVerficationFileLoadedValue = value;
                OnPropertyChanged("isVerficationFileLoaded");
            }
        }

        private Visibility Ch1EnableVisibleValue = Visibility.Collapsed;
        public Visibility Ch1EnableVisible
        {
            get { return Ch1EnableVisibleValue; }
            set
            {
                Ch1EnableVisibleValue = value;
                OnPropertyChanged("Ch1EnableVisible");
            }
        }

        private Visibility Ch2EnableVisibleValue = Visibility.Collapsed;
        public Visibility Ch2EnableVisible
        {
            get { return Ch2EnableVisibleValue; }
            set
            {
                Ch2EnableVisibleValue = value;
                OnPropertyChanged("Ch2EnableVisible");
            }
        }

        private Visibility Ch3EnableVisibleValue = Visibility.Collapsed;
        public Visibility Ch3EnableVisible
        {
            get { return Ch3EnableVisibleValue; }
            set
            {
                Ch3EnableVisibleValue = value;
                OnPropertyChanged("Ch3EnableVisible");
            }
        }

        private Visibility Ch4EnableVisibleValue = Visibility.Collapsed;
        public Visibility Ch4EnableVisible
        {
            get { return Ch4EnableVisibleValue; }
            set
            {
                Ch4EnableVisibleValue = value;
                OnPropertyChanged("Ch4EnableVisible");
            }
        }

        private Visibility Ch5EnableVisibleValue = Visibility.Collapsed;
        public Visibility Ch5EnableVisible
        {
            get { return Ch5EnableVisibleValue; }
            set
            {
                Ch5EnableVisibleValue = value;
                OnPropertyChanged("Ch5EnableVisible");
            }
        }

        private Visibility Ch6EnableVisibleValue = Visibility.Collapsed;
        public Visibility Ch6EnableVisible
        {
            get { return Ch6EnableVisibleValue; }
            set
            {
                Ch6EnableVisibleValue = value;
                OnPropertyChanged("Ch6EnableVisible");
            }
        }

        private Visibility Ch7EnableVisibleValue = Visibility.Collapsed;
        public Visibility Ch7EnableVisible
        {
            get { return Ch7EnableVisibleValue; }
            set
            {
                Ch7EnableVisibleValue = value;
                OnPropertyChanged("Ch7EnableVisible");
            }
        }

        private Visibility Ch8EnableVisibleValue = Visibility.Collapsed;
        public Visibility Ch8EnableVisible
        {
            get { return Ch8EnableVisibleValue; }
            set
            {
                Ch8EnableVisibleValue = value;
                OnPropertyChanged("Ch8EnableVisible");
            }
        }

        private int OutChannelCountValue = 0;
        public int OutChannelCount
        {
            get { return OutChannelCountValue; }
            set
            {
                OutChannelCountValue = value;
                OnPropertyChanged("OutChannelCount");

                if (value > 0)
                    Ch1EnableVisible = Visibility.Visible;
                else
                    Ch1EnableVisible = Visibility.Collapsed;

                if (value > 1)
                    Ch2EnableVisible = Visibility.Visible;
                else
                    Ch2EnableVisible = Visibility.Collapsed;

                if (value > 2)
                    Ch3EnableVisible = Visibility.Visible;
                else
                    Ch3EnableVisible = Visibility.Collapsed;

                if (value > 3)
                    Ch4EnableVisible = Visibility.Visible;
                else
                    Ch4EnableVisible = Visibility.Collapsed;

                if (value > 4)
                    Ch5EnableVisible = Visibility.Visible;
                else
                    Ch5EnableVisible = Visibility.Collapsed;

                if (value > 5)
                    Ch6EnableVisible = Visibility.Visible;
                else
                    Ch6EnableVisible = Visibility.Collapsed;

                if (value > 6)
                    Ch7EnableVisible = Visibility.Visible;
                else
                    Ch7EnableVisible = Visibility.Collapsed;

                if (value > 7)
                    Ch8EnableVisible = Visibility.Visible;
                else
                    Ch8EnableVisible = Visibility.Collapsed;
            }
        }
        
        private string txtFreqVerificationValue = null;
        public string txtFreqVerification
        {
            get { return txtFreqVerificationValue; }
            set
            {
                txtFreqVerificationValue = value;
                OnPropertyChanged("txtFreqVerification");
            }
        }

        private string txtFreqVerificationpathValue = null;
        public string txtFreqVerificationpath
        {
            get { return txtFreqVerificationpathValue; }
            set
            {
                txtFreqVerificationpathValue = value;
                OnPropertyChanged("txtFreqVerificationpath");
            }
        }

        private TestApxItem parentTestApxItemValue = null;
        public TestApxItem ParentTestApxItem
        {
            get { return parentTestApxItemValue; }
            set { parentTestApxItemValue = value; OnPropertyChanged("ParentTestApxItem"); }
        }
    }

    public partial class APXSteppedFreqSweepVerification : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        void OnPropertyChanged(string property)
        {
            try
            {
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs(property));
                    if (ParentTestApxItem != null && ParentTestApxItem.ParentTestActionItem != null && parentTestApxItemValue.ParentTestActionItem.ParentTestCaseItem != null && ParentTestApxItem.ParentTestActionItem.ParentTestCaseItem.SaveButtonIsEnabled != true)
                        ParentTestApxItem.ParentTestActionItem.ParentTestCaseItem.SaveButtonIsEnabled = true;
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

        private string ChkGenONContentValue = "Generator Off ";
        public string ChkGenONContent
        {
            get { return ChkGenONContentValue; }
            set
            {
                ChkGenONContentValue = value;
                OnPropertyChanged("ChkGenONContent");
            }
        }

        private bool ChkGenONValue = true;
        public bool ChkGenON
        {
            get { return ChkGenONValue; }
            set
            {
                ChkGenONValue = value;
                if (ChkGenONValue == false)
                    ChkGenONContent = "Generator Off ";
                else
                    ChkGenONContent = "Generator On  ";

                OnPropertyChanged("ChkGenON");
            }
        }

        private string StartFrequencyValue = null;
        public string StartFrequency
        {
            get { return StartFrequencyValue; }
            set
            {
                StartFrequencyValue = value;
                OnPropertyChanged("StartFrequency");
            }
        }

        private string StopFrequencyValue = null;
        public string StopFrequency
        {
            get { return StopFrequencyValue; }
            set
            {
                StopFrequencyValue = value;
                OnPropertyChanged("StopFrequency");
            }
        }

        private ObservableCollection<string> cmbSweepListValue = new ObservableCollection<string> { "Logarithmic", "Linear", "Custom" };
        public ObservableCollection<string> cmbSweepList
        {
            get { return cmbSweepListValue; }
            set { cmbSweepListValue = value; OnPropertyChanged("cmbSweepList"); }
        }

        private string SelectedSweepValue = null;
        public string SelectedSweep
        {
            get { return SelectedSweepValue; }
            set
            {
                SelectedSweepValue = value;
                OnPropertyChanged("SelectedSweep");
            }
        }

        private int SteppedpointsValue = 0;
        public int Steppedpoints
        {
            get { return SteppedpointsValue; }
            set
            {
                SteppedpointsValue = value;
                OnPropertyChanged("Steppedpoints");
            }
        }

        private string SteppedLevelValue = null;
        public string SteppedLevel
        {
            get { return SteppedLevelValue; }
            set
            {
                SteppedLevelValue = value;
                OnPropertyChanged("SteppedLevel");
            }
        }

        private bool IsEnableCh1Value = false;
        public bool IsEnableCh1
        {
            get { return IsEnableCh1Value; }
            set
            {
                IsEnableCh1Value = value;
                OnPropertyChanged("IsEnableCh1");
            }
        }

        private bool IsEnableCh2Value = false;
        public bool IsEnableCh2
        {
            get { return IsEnableCh2Value; }
            set
            {
                IsEnableCh2Value = value;
                OnPropertyChanged("IsEnableCh2");
            }
        }

        private bool IsEnableCh3Value = false;
        public bool IsEnableCh3
        {
            get { return IsEnableCh3Value; }
            set
            {
                IsEnableCh3Value = value;
                OnPropertyChanged("IsEnableCh3");
            }
        }

        private bool IsEnableCh4Value = false;
        public bool IsEnableCh4
        {
            get { return IsEnableCh4Value; }
            set
            {
                IsEnableCh4Value = value;
                OnPropertyChanged("IsEnableCh4");
            }
        }

        private bool IsEnableCh5Value = false;
        public bool IsEnableCh5
        {
            get { return IsEnableCh5Value; }
            set
            {
                IsEnableCh5Value = value;
                OnPropertyChanged("IsEnableCh5");
            }
        }

        private bool IsEnableCh6Value = false;
        public bool IsEnableCh6
        {
            get { return IsEnableCh6Value; }
            set
            {
                IsEnableCh6Value = value;
                OnPropertyChanged("IsEnableCh6");
            }
        }

        private bool IsEnableCh7Value = false;
        public bool IsEnableCh7
        {
            get { return IsEnableCh7Value; }
            set
            {
                IsEnableCh7Value = value;
                OnPropertyChanged("IsEnableCh7");
            }
        }

        private bool IsEnableCh8Value = false;
        public bool IsEnableCh8
        {
            get { return IsEnableCh8Value; }
            set
            {
                IsEnableCh8Value = value;
                OnPropertyChanged("IsEnableCh8");
            }
        }

        private bool isVerficationFileLoadedValue = false;
        public bool isVerficationFileLoaded
        {
            get { return isVerficationFileLoadedValue; }
            set
            {
                isVerficationFileLoadedValue = value;
                OnPropertyChanged("isVerficationFileLoaded");
            }
        }

        private Visibility Ch1VisibleValue = Visibility.Collapsed;
        public Visibility Ch1Visible
        {
            get { return Ch1VisibleValue; }
            set
            {
                Ch1VisibleValue = value;
                OnPropertyChanged("Ch1Visible");
            }
        }

        private Visibility Ch2VisibleValue = Visibility.Collapsed;
        public Visibility Ch2Visible
        {
            get { return Ch2VisibleValue; }
            set
            {
                Ch2VisibleValue = value;
                OnPropertyChanged("Ch2Visible");
            }
        }

        private Visibility Ch3VisibleValue = Visibility.Collapsed;
        public Visibility Ch3Visible
        {
            get { return Ch3VisibleValue; }
            set
            {
                Ch3VisibleValue = value;
                OnPropertyChanged("Ch3Visible");
            }
        }

        private Visibility Ch4VisibleValue = Visibility.Collapsed;
        public Visibility Ch4Visible
        {
            get { return Ch4VisibleValue; }
            set
            {
                Ch4VisibleValue = value;
                OnPropertyChanged("Ch4Visible");
            }
        }

        private Visibility Ch5VisibleValue = Visibility.Collapsed;
        public Visibility Ch5Visible
        {
            get { return Ch5VisibleValue; }
            set
            {
                Ch5VisibleValue = value;
                OnPropertyChanged("Ch5Visible");
            }
        }

        private Visibility Ch6VisibleValue = Visibility.Collapsed;
        public Visibility Ch6Visible
        {
            get { return Ch6VisibleValue; }
            set
            {
                Ch6VisibleValue = value;
                OnPropertyChanged("Ch6Visible");
            }
        }

        private Visibility Ch7VisibleValue = Visibility.Collapsed;
        public Visibility Ch7Visible
        {
            get { return Ch7VisibleValue; }
            set
            {
                Ch7VisibleValue = value;
                OnPropertyChanged("Ch7Visible");
            }
        }

        private Visibility Ch8VisibleValue = Visibility.Collapsed;
        public Visibility Ch8Visible
        {
            get { return Ch8VisibleValue; }
            set
            {
                Ch8VisibleValue = value;
                OnPropertyChanged("Ch8Visible");
            }
        }

        private int OutChannelCountValue = 0;
        public int OutChannelCount
        {
            get { return OutChannelCountValue; }
            set
            {
                OutChannelCountValue = value;
                OnPropertyChanged("OutChannelCount");

                if (value > 0)
                    Ch1Visible = Visibility.Visible;
                else
                    Ch1Visible = Visibility.Collapsed;

                if (value > 1)
                    Ch2Visible = Visibility.Visible;
                else
                    Ch2Visible = Visibility.Collapsed;

                if (value > 2)
                    Ch3Visible = Visibility.Visible;
                else
                    Ch3Visible = Visibility.Collapsed;

                if (value > 3)
                    Ch4Visible = Visibility.Visible;
                else
                    Ch4Visible = Visibility.Collapsed;

                if (value > 4)
                    Ch5Visible = Visibility.Visible;
                else
                    Ch5Visible = Visibility.Collapsed;

                if (value > 5)
                    Ch6Visible = Visibility.Visible;
                else
                    Ch6Visible = Visibility.Collapsed;

                if (value > 6)
                    Ch7Visible = Visibility.Visible;
                else
                    Ch7Visible = Visibility.Collapsed;

                if (value > 7)
                    Ch8Visible = Visibility.Visible;
                else
                    Ch8Visible = Visibility.Collapsed;

            }
        }

        private List<string> cmbPhaseRefChListValue = new List<string>();
        public List<string> cmbPhaseRefChList
        {
            get { return cmbPhaseRefChListValue; }
            set { cmbPhaseRefChListValue = value; OnPropertyChanged("cmbPhaseRefChList"); }
        }

        private string cmbPhaseRefChannelValue = null;
        public string cmbPhaseRefChannel
        {
            get { return cmbPhaseRefChannelValue; }
            set
            {
                cmbPhaseRefChannelValue = value;
                OnPropertyChanged("cmbPhaseRefChannel");
            }
        }

        private int InChCountValue = 0;
        public int InChCount
        {
            get { return InChCountValue; }
            set
            {
                InChCountValue = value;
                OnPropertyChanged("InChCount");

                if (value > 0)
                    if(!cmbPhaseRefChList.Contains("Ch1"))
                        cmbPhaseRefChList.Add("Ch1");

                if (value > 1)
                    if (!cmbPhaseRefChList.Contains("Ch2"))
                        cmbPhaseRefChList.Add("Ch2");

                if (value > 2)
                    if (!cmbPhaseRefChList.Contains("Ch3"))
                        cmbPhaseRefChList.Add("Ch3");

                if (value > 3)
                    if (!cmbPhaseRefChList.Contains("Ch4"))
                        cmbPhaseRefChList.Add("Ch4");

                if (value > 4)
                    if (!cmbPhaseRefChList.Contains("Ch5"))
                        cmbPhaseRefChList.Add("Ch5");

                if (value > 5)
                    if (!cmbPhaseRefChList.Contains("Ch6"))
                        cmbPhaseRefChList.Add("Ch6");

                if (value > 6)
                    if (!cmbPhaseRefChList.Contains("Ch7"))
                        cmbPhaseRefChList.Add("Ch7");

                if (value > 7)
                    if (!cmbPhaseRefChList.Contains("Ch8"))
                        cmbPhaseRefChList.Add("Ch8");
            }
        }

        private string txtSteppedFreqVerificationValue = null;
        public string txtSteppedFreqVerification
        {
            get { return txtSteppedFreqVerificationValue; }
            set
            {
                txtSteppedFreqVerificationValue = value;
                OnPropertyChanged("txtSteppedFreqVerification");
            }
        }

        private string txtSteppedFreqVerificationpathValue = null;
        public string txtSteppedFreqVerificationpath
        {
            get { return txtSteppedFreqVerificationpathValue; }
            set
            {
                txtSteppedFreqVerificationpathValue = value;
                OnPropertyChanged("txtSteppedFreqVerificationpath"); 
            }
        }

        private TestApxItem parentTestApxItemValue = null;
        public TestApxItem ParentTestApxItem
        {
            get { return parentTestApxItemValue; }
            set { parentTestApxItemValue = value; OnPropertyChanged("ParentTestApxItem"); }
        }
    }

    public partial class APXInterChannelPhaseVerification : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        void OnPropertyChanged(string property)
        {
            try
            {
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs(property));
                    if (ParentTestApxItem != null && ParentTestApxItem.ParentTestActionItem != null && parentTestApxItemValue.ParentTestActionItem.ParentTestCaseItem != null && ParentTestApxItem.ParentTestActionItem.ParentTestCaseItem.SaveButtonIsEnabled != true)
                        ParentTestApxItem.ParentTestActionItem.ParentTestCaseItem.SaveButtonIsEnabled = true;
                }
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                // MessageBox.Show("Exception\n " + ex.Message, "Error Code - ECxxxxx", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        private string ChkGenONContentValue = "Generator Off ";
        public string ChkGenONContent
        {
            get { return ChkGenONContentValue; }
            set
            {
                ChkGenONContentValue = value;
                OnPropertyChanged("ChkGenONContent");
            }
        }

        private bool ChkGenONValue = true;
        public bool ChkGenON
        {
            get { return ChkGenONValue; }
            set
            {
                ChkGenONValue = value;
                if (ChkGenONValue == false)
                    ChkGenONContent = "Generator Off ";
                else
                    ChkGenONContent = "Generator On  ";
                OnPropertyChanged("ChkGenON");
            }
        }

        private bool SteppedTrackChannelValue = false;
        public bool SteppedTrackChannel
        {
            get { return SteppedTrackChannelValue; }
            set
            {
                SteppedTrackChannelValue = value;
                OnPropertyChanged("SteppedTrackChannel");

                if (OutChannelCount > 1)
                {
                    if (!value)
                    {
                        lbllevelCh2Visible = Visibility.Visible;
                        TxtlevelCh2Visible = Visibility.Visible;
                    }
                    else
                    {
                        lbllevelCh2Visible = Visibility.Collapsed;
                        TxtlevelCh2Visible = Visibility.Collapsed;
                    }
                }

                if (OutChannelCount > 2)
                {
                    if (!value)
                    {
                        lbllevelCh3Visible = Visibility.Visible;
                        TxtlevelCh3Visible = Visibility.Visible;
                    }
                    else
                    {
                        lbllevelCh3Visible = Visibility.Collapsed;
                        TxtlevelCh3Visible = Visibility.Collapsed;
                    }
                }

                if (OutChannelCount > 3)
                {
                    if (!value)
                    {
                        lbllevelCh4Visible = Visibility.Visible;
                        TxtlevelCh4Visible = Visibility.Visible;
                    }
                    else
                    {
                        lbllevelCh4Visible = Visibility.Collapsed;
                        TxtlevelCh4Visible = Visibility.Collapsed;
                    }
                }

                if (OutChannelCount > 4)
                {
                    if (!value)
                    {
                        lbllevelCh5Visible = Visibility.Visible;
                        TxtlevelCh5Visible = Visibility.Visible;
                    }
                    else
                    {
                        lbllevelCh5Visible = Visibility.Collapsed;
                        TxtlevelCh5Visible = Visibility.Collapsed;
                    }
                }

                if (OutChannelCount > 5)
                {
                    if (!value)
                    {
                        lbllevelCh6Visible = Visibility.Visible;
                        TxtlevelCh6Visible = Visibility.Visible;
                    }
                    else
                    {
                        lbllevelCh6Visible = Visibility.Collapsed;
                        TxtlevelCh6Visible = Visibility.Collapsed;
                    }
                }

                if (OutChannelCount > 6)
                {
                    if (!value)
                    {
                        lbllevelCh7Visible = Visibility.Visible;
                        TxtlevelCh7Visible = Visibility.Visible;
                    }
                    else
                    {
                        lbllevelCh7Visible = Visibility.Collapsed;
                        TxtlevelCh7Visible = Visibility.Collapsed;
                    }
                }

                if (OutChannelCount > 7)
                {
                    if (!value)
                    {
                        lbllevelCh8Visible = Visibility.Visible;
                        TxtlevelCh8Visible = Visibility.Visible;
                    }
                    else
                    {
                        lbllevelCh8Visible = Visibility.Collapsed;
                        TxtlevelCh8Visible = Visibility.Collapsed;
                    }
                }
            }
        }

        private string LevelCh1Value = null;
        public string LevelCh1
        {
            get { return LevelCh1Value; }
            set
            {
                LevelCh1Value = value;
                OnPropertyChanged("LevelCh1");
            }
        }

        private string LevelCh2Value = null;
        public string LevelCh2
        {
            get { return LevelCh2Value; }
            set
            {
                LevelCh2Value = value;
                OnPropertyChanged("LevelCh2");
            }
        }

        private string LevelCh3Value = null;
        public string LevelCh3
        {
            get { return LevelCh3Value; }
            set
            {
                LevelCh3Value = value;
                OnPropertyChanged("LevelCh3");
            }
        }

        private string LevelCh4Value = null;
        public string LevelCh4
        {
            get { return LevelCh4Value; }
            set
            {
                LevelCh4Value = value;
                OnPropertyChanged("LevelCh4");
            }
        }

        private string LevelCh5Value = null;
        public string LevelCh5
        {
            get { return LevelCh5Value; }
            set
            {
                LevelCh5Value = value;
                OnPropertyChanged("LevelCh5");
            }
        }

        private string LevelCh6Value = null;
        public string LevelCh6
        {
            get { return LevelCh6Value; }
            set
            {
                LevelCh6Value = value;
                OnPropertyChanged("LevelCh6");
            }
        }

        private string LevelCh7Value = null;
        public string LevelCh7
        {
            get { return LevelCh7Value; }
            set
            {
                LevelCh7Value = value;
                OnPropertyChanged("LevelCh7");
            }
        }

        private string LevelCh8Value = null;
        public string LevelCh8
        {
            get { return LevelCh8Value; }
            set
            {
                LevelCh8Value = value;
                OnPropertyChanged("LevelCh8");
            }
        }

        private bool isVerficationFileLoadedValue = false;
        public bool isVerficationFileLoaded
        {
            get { return isVerficationFileLoadedValue; }
            set
            {
                isVerficationFileLoadedValue = value;
                OnPropertyChanged("isVerficationFileLoaded");
            }
        }

        private Visibility lbllevelCh2VisibleValue = Visibility.Collapsed;
        public Visibility lbllevelCh2Visible
        {
            get { return lbllevelCh2VisibleValue; }
            set
            {
                lbllevelCh2VisibleValue = value;
                OnPropertyChanged("lbllevelCh2Visible");
            }
        }

        private Visibility lbllevelCh3VisibleValue = Visibility.Collapsed;
        public Visibility lbllevelCh3Visible
        {
            get { return lbllevelCh3VisibleValue; }
            set
            {
                lbllevelCh3VisibleValue = value;
                OnPropertyChanged("lbllevelCh3Visible");
            }
        }

        private Visibility lbllevelCh4VisibleValue = Visibility.Collapsed;
        public Visibility lbllevelCh4Visible
        {
            get { return lbllevelCh4VisibleValue; }
            set
            {
                lbllevelCh4VisibleValue = value;
                OnPropertyChanged("lbllevelCh4Visible");
            }
        }

        private Visibility lbllevelCh5VisibleValue = Visibility.Collapsed;
        public Visibility lbllevelCh5Visible
        {
            get { return lbllevelCh5VisibleValue; }
            set
            {
                lbllevelCh5VisibleValue = value;
                OnPropertyChanged("lbllevelCh5Visible");
            }
        }

        private Visibility lbllevelCh6VisibleValue = Visibility.Collapsed;
        public Visibility lbllevelCh6Visible
        {
            get { return lbllevelCh6VisibleValue; }
            set
            {
                lbllevelCh6VisibleValue = value;
                OnPropertyChanged("lbllevelCh6Visible");
            }
        }

        private Visibility lbllevelCh7VisibleValue = Visibility.Collapsed;
        public Visibility lbllevelCh7Visible
        {
            get { return lbllevelCh7VisibleValue; }
            set
            {
                lbllevelCh7VisibleValue = value;
                OnPropertyChanged("lbllevelCh7Visible");
            }
        }

        private Visibility lbllevelCh8VisibleValue = Visibility.Collapsed;
        public Visibility lbllevelCh8Visible
        {
            get { return lbllevelCh8VisibleValue; }
            set
            {
                lbllevelCh8VisibleValue = value;
                OnPropertyChanged("lbllevelCh8Visible");
            }
        }

        private Visibility TxtlevelCh2VisibleValue = Visibility.Collapsed;
        public Visibility TxtlevelCh2Visible
        {
            get { return TxtlevelCh2VisibleValue; }
            set
            {
                TxtlevelCh2VisibleValue = value;
                OnPropertyChanged("TxtlevelCh2Visible");
            }
        }

        private Visibility TxtlevelCh3VisibleValue = Visibility.Collapsed;
        public Visibility TxtlevelCh3Visible
        {
            get { return TxtlevelCh3VisibleValue; }
            set
            {
                TxtlevelCh3VisibleValue = value;
                OnPropertyChanged("TxtlevelCh3Visible");
            }
        }

        private Visibility TxtlevelCh4VisibleValue = Visibility.Collapsed;
        public Visibility TxtlevelCh4Visible
        {
            get { return TxtlevelCh4VisibleValue; }
            set
            {
                TxtlevelCh4VisibleValue = value;
                OnPropertyChanged("TxtlevelCh4Visible");
            }
        }

        private Visibility TxtlevelCh5VisibleValue = Visibility.Collapsed;
        public Visibility TxtlevelCh5Visible
        {
            get { return TxtlevelCh5VisibleValue; }
            set
            {
                TxtlevelCh5VisibleValue = value;
                OnPropertyChanged("TxtlevelCh5Visible");
            }
        }

        private Visibility TxtlevelCh6VisibleValue = Visibility.Collapsed;
        public Visibility TxtlevelCh6Visible
        {
            get { return TxtlevelCh6VisibleValue; }
            set
            {
                TxtlevelCh6VisibleValue = value;
                OnPropertyChanged("TxtlevelCh6Visible");
            }
        }

        private Visibility TxtlevelCh7VisibleValue = Visibility.Collapsed;
        public Visibility TxtlevelCh7Visible
        {
            get { return TxtlevelCh7VisibleValue; }
            set
            {
                TxtlevelCh7VisibleValue = value;
                OnPropertyChanged("TxtlevelCh7Visible");
            }
        }

        private Visibility TxtlevelCh8VisibleValue = Visibility.Collapsed;
        public Visibility TxtlevelCh8Visible
        {
            get { return TxtlevelCh8VisibleValue; }
            set
            {
                TxtlevelCh8VisibleValue = value;
                OnPropertyChanged("TxtlevelCh8Visible");
            }
        }

        private string TxtFreqAValue = null;
        public string TxtFreqA
        {
            get { return TxtFreqAValue; }
            set
            {
                TxtFreqAValue = value;
                OnPropertyChanged("TxtFreqA");
            }
        }

        private string CmbRefChannelSelectedValue = null;
        public string CmbRefChannelSelected
        {
            get { return CmbRefChannelSelectedValue; }
            set
            {
                CmbRefChannelSelectedValue = value;
                OnPropertyChanged("CmbRefChannelSelected");
            }
        }

        private List<string> RefChannelListValue = new List<string>();
        public List<string> RefChannelList
        {
            get { return RefChannelListValue; }
            set { RefChannelListValue = value; OnPropertyChanged("RefChannelList"); }
        }

        private List<string> MeterRangeListValue = new List<string> { "0 -> 360 deg", "-90 -> 270 deg", "-180 -> 180 deg" };
        public List<string> MeterRangeList
        {
            get { return MeterRangeListValue; }
            set { MeterRangeListValue = value; OnPropertyChanged("MeterRangeList"); }
        }

        private string MeterRangeSelectedValue = null;
        public string MeterRangeSelected
        {
            get { return MeterRangeSelectedValue; }
            set
            {
                MeterRangeSelectedValue = value;
                OnPropertyChanged("MeterRangeSelected");
            }
        }

        private bool Isch1EnableValue = false;
        public bool Isch1Enable
        {
            get { return Isch1EnableValue; }
            set
            {
                Isch1EnableValue = value;
                OnPropertyChanged("Isch1Enable");
            }
        }

        private bool Isch2EnableValue = false;
        public bool Isch2Enable
        {
            get { return Isch2EnableValue; }
            set
            {
                Isch2EnableValue = value;
                OnPropertyChanged("Isch2Enable");
            }
        }

        private bool Isch3EnableValue = false;
        public bool Isch3Enable
        {
            get { return Isch3EnableValue; }
            set
            {
                Isch3EnableValue = value;
                OnPropertyChanged("Isch3Enable");
            }
        }

        private bool Isch4EnableValue = false;
        public bool Isch4Enable
        {
            get { return Isch4EnableValue; }
            set
            {
                Isch4EnableValue = value;
                OnPropertyChanged("Isch4Enable");
            }
        }

        private bool Isch5EnableValue = false;
        public bool Isch5Enable
        {
            get { return Isch5EnableValue; }
            set
            {
                Isch5EnableValue = value;
                OnPropertyChanged("Isch5Enable");
            }
        }

        private bool Isch6EnableValue = false;
        public bool Isch6Enable
        {
            get { return Isch6EnableValue; }
            set
            {
                Isch6EnableValue = value;
                OnPropertyChanged("Isch6Enable");
            }
        }

        private bool Isch7EnableValue = false;
        public bool Isch7Enable
        {
            get { return Isch7EnableValue; }
            set
            {
                Isch7EnableValue = value;
                OnPropertyChanged("Isch7Enable");
            }
        }

        private bool Isch8EnableValue = false;
        public bool Isch8Enable
        {
            get { return Isch8EnableValue; }
            set
            {
                Isch8EnableValue = value;
                OnPropertyChanged("Isch8Enable");
            }
        }


        private Visibility Ch1VisibilityValue = Visibility.Collapsed;
        public Visibility Ch1Visibility
        {
            get { return Ch1VisibilityValue; }
            set
            {
                Ch1VisibilityValue = value;
                OnPropertyChanged("Ch1Visibility");
            }
        }

        private Visibility Ch2VisibilityValue = Visibility.Collapsed;
        public Visibility Ch2Visibility
        {
            get { return Ch2VisibilityValue; }
            set
            {
                Ch2VisibilityValue = value;
                OnPropertyChanged("Ch2Visibility");
            }
        }

        private Visibility Ch3VisibilityValue = Visibility.Collapsed;
        public Visibility Ch3Visibility
        {
            get { return Ch3VisibilityValue; }
            set
            {
                Ch3VisibilityValue = value;
                OnPropertyChanged("Ch3Visibility");
            }
        }

        private Visibility Ch4VisibilityValue = Visibility.Collapsed;
        public Visibility Ch4Visibility
        {
            get { return Ch4VisibilityValue; }
            set
            {
                Ch4VisibilityValue = value;
                OnPropertyChanged("Ch4Visibility");
            }
        }

        private Visibility Ch5VisibilityValue = Visibility.Collapsed;
        public Visibility Ch5Visibility
        {
            get { return Ch5VisibilityValue; }
            set
            {
                Ch5VisibilityValue = value;
                OnPropertyChanged("Ch5Visibility");
            }
        }

        private Visibility Ch6VisibilityValue = Visibility.Collapsed;
        public Visibility Ch6Visibility
        {
            get { return Ch6VisibilityValue; }
            set
            {
                Ch6VisibilityValue = value;
                OnPropertyChanged("Ch6Visibility");
            }
        }

        private Visibility Ch7VisibilityValue = Visibility.Collapsed;
        public Visibility Ch7Visibility
        {
            get { return Ch7VisibilityValue; }
            set
            {
                Ch7VisibilityValue = value;
                OnPropertyChanged("Ch7Visibility");
            }
        }

        private Visibility Ch8VisibilityValue = Visibility.Collapsed;
        public Visibility Ch8Visibility
        {
            get { return Ch8VisibilityValue; }
            set
            {
                Ch8VisibilityValue = value;
                OnPropertyChanged("Ch8Visibility");
            }
        }

        private int OutChannelCountValue = 0;
        public int OutChannelCount
        {
            get { return OutChannelCountValue; }
            set
            {
                OutChannelCountValue = value;
                OnPropertyChanged("OutChannelCount");

                if (value > 0)
                {
                    Ch1Visibility = Visibility.Visible;
                }
                else
                {
                    Ch1Visibility = Visibility.Collapsed;
                }

                if (value > 1)
                    Ch2Visibility = Visibility.Visible;
                else
                    Ch2Visibility = Visibility.Collapsed;
                
                if (value > 2)
                    Ch3Visibility = Visibility.Visible;
                else
                    Ch3Visibility = Visibility.Collapsed;

                if (value > 3)
                    Ch4Visibility = Visibility.Visible;
                else
                    Ch4Visibility = Visibility.Collapsed;
                
                if (value > 4)
                    Ch5Visibility = Visibility.Visible;
                else
                    Ch5Visibility = Visibility.Collapsed;
                
                if (value > 5)
                    Ch6Visibility = Visibility.Visible;
                else
                    Ch6Visibility = Visibility.Collapsed;

                if (value > 6)
                    Ch7Visibility = Visibility.Visible;
                else
                    Ch7Visibility = Visibility.Collapsed;

                if (value > 7)
                    Ch8Visibility = Visibility.Visible;
                else
                    Ch8Visibility = Visibility.Collapsed;
            }
        }

        private int InChannelCountValue = 0;
        public int InChannelCount
        {
            get { return InChannelCountValue; }
            set
            {
                InChannelCountValue = value;
                OnPropertyChanged("InChannelCount");

                if (value > 0)
                {
                    if(!RefChannelList.Contains("AllChannels"))
                        RefChannelList.Add("AllChannels");
                    if (!RefChannelList.Contains("Ch1"))
                        RefChannelList.Add("Ch1");
                }

                if (value > 1)
                {
                    if (!RefChannelList.Contains("Ch2"))
                        RefChannelList.Add("Ch2");
                }

                if (value > 2)
                {
                    if (!RefChannelList.Contains("Ch3"))
                        RefChannelList.Add("Ch3");
                }

                if (value > 3)
                {
                    if (!RefChannelList.Contains("Ch4"))
                        RefChannelList.Add("Ch4");
                }

                if (value > 4)
                {
                    if (!RefChannelList.Contains("Ch5"))
                        RefChannelList.Add("Ch5");
                }

                if (value > 5)
                {
                    if (!RefChannelList.Contains("Ch6"))
                        RefChannelList.Add("Ch6");
                }

                if (value > 6)
                {
                    if (!RefChannelList.Contains("Ch7"))
                        RefChannelList.Add("Ch7");
                }

                if (value > 7)
                {
                    if (!RefChannelList.Contains("Ch8"))
                        RefChannelList.Add("Ch8");
                }
            }
        }

        private string txtPhaseVerificationValue = null;
        public string txtPhaseVerification
        {
            get { return txtPhaseVerificationValue; }
            set
            {
                txtPhaseVerificationValue = value;
                OnPropertyChanged("txtPhaseVerification");
            }
        }

        private string txtPhaseVerificationPathValue = null;
        public string txtPhaseVerificationPath
        {
            get { return txtPhaseVerificationPathValue; }
            set
            {
                txtPhaseVerificationPathValue = value;
                OnPropertyChanged("txtPhaseVerificationPath");
            }
        }

        private TestApxItem parentTestApxItemValue = null;
        public TestApxItem ParentTestApxItem
        {
            get { return parentTestApxItemValue; }
            set { parentTestApxItemValue = value; OnPropertyChanged("ParentTestApxItem"); }
        }
    }

    public partial class APXTHDNVerification : INotifyPropertyChanged   
    {
        public event PropertyChangedEventHandler PropertyChanged;
        void OnPropertyChanged(string property)
        {
            try
            {
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs(property));
                    if (ParentTestApxItem != null && ParentTestApxItem.ParentTestActionItem != null && parentTestApxItemValue.ParentTestActionItem.ParentTestCaseItem != null && ParentTestApxItem.ParentTestActionItem.ParentTestCaseItem.SaveButtonIsEnabled != true)
                        ParentTestApxItem.ParentTestActionItem.ParentTestCaseItem.SaveButtonIsEnabled = true;
                }
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                // MessageBox.Show("Exception\n " + ex.Message, "Error Code - ECxxxxx", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private string ChkGenONContentValue = "Generator Off ";
        public string ChkGenONContent
        {
            get { return ChkGenONContentValue; }
            set
            {
                ChkGenONContentValue = value;
                OnPropertyChanged("ChkGenONContent");
            }
        }

        private bool ChkGenONValue = true;
        public bool ChkGenON
        {
            get { return ChkGenONValue; }
            set
            {
                ChkGenONValue = value;
                if (ChkGenONValue == false)
                    ChkGenONContent = "Generator Off ";
                else
                    ChkGenONContent = "Generator On  ";
                OnPropertyChanged("ChkGenON");
            }
        }    

        private bool ChkThdnLevelTrackChValue = true;
        public bool chkBx_ThdnLevelTrackCh
        {
            get { return ChkThdnLevelTrackChValue; }
            set
            {
                ChkThdnLevelTrackChValue = value;
                OnPropertyChanged("chkBx_ThdnLevelTrackCh");

                if (OutChannelCount > 1)
                {
                    if (!value)
                    {
                        lblCh2Visibility = Visibility.Visible;
                        txtCh2Visibility = Visibility.Visible;
                    }
                    else
                    {
                        lblCh2Visibility = Visibility.Collapsed;
                        txtCh2Visibility = Visibility.Collapsed;
                    }
                }

                if (OutChannelCount > 2)
                {
                    if (!value)
                    {
                        lblCh3Visibility = Visibility.Visible;
                        txtCh3Visibility = Visibility.Visible;
                    }
                    else
                    {
                        lblCh3Visibility = Visibility.Collapsed;
                        txtCh3Visibility = Visibility.Collapsed;
                    }
                }

                if (OutChannelCount > 3)
                {
                    if (!value)
                    {
                        lblCh4Visibility = Visibility.Visible;
                        txtCh4Visibility = Visibility.Visible;
                    }
                    else
                    {
                        lblCh4Visibility = Visibility.Collapsed;
                        txtCh4Visibility = Visibility.Collapsed;
                    }
                }

                if (OutChannelCount > 4)
                {
                    if (!value)
                    {
                        lblCh5Visibility = Visibility.Visible;
                        txtCh5Visibility = Visibility.Visible;
                    }
                    else
                    {
                        lblCh5Visibility = Visibility.Collapsed;
                        txtCh5Visibility = Visibility.Collapsed;
                    }
                }

                if (OutChannelCount > 5)
                {
                    if (!value)
                    {
                        lblCh6Visibility = Visibility.Visible;
                        txtCh6Visibility = Visibility.Visible;
                    }
                    else
                    {
                        lblCh6Visibility = Visibility.Collapsed;
                        txtCh6Visibility = Visibility.Collapsed;
                    }
                }

                if (OutChannelCount > 6)
                {
                    if (!value)
                    {
                        lblCh7Visibility = Visibility.Visible;
                        txtCh7Visibility = Visibility.Visible;
                    }
                    else
                    {
                        lblCh7Visibility = Visibility.Collapsed;
                        txtCh7Visibility = Visibility.Collapsed;
                    }
                }

                if (OutChannelCount > 7)
                {
                    if (!value)
                    {
                        lblCh8Visibility = Visibility.Visible;
                        txtCh8Visibility = Visibility.Visible;
                    }
                    else
                    {
                        lblCh8Visibility = Visibility.Collapsed;
                        txtCh8Visibility = Visibility.Collapsed;
                    }
                }
            }
        }

        private bool isVerficationFileLoadedValue = false;
        public bool isVerficationFileLoaded
        {
            get { return isVerficationFileLoadedValue; }
            set
            {
                isVerficationFileLoadedValue = value;
                OnPropertyChanged("isVerficationFileLoaded");
            }
        }

        private Visibility lblCh1VisibilityValue = Visibility.Collapsed;
        public Visibility lblCh1Visibility
        {
            get { return lblCh1VisibilityValue; }
            set { lblCh1VisibilityValue = value; OnPropertyChanged("lblCh1Visibility"); }
        }

        private Visibility lblCh2VisibilityValue = Visibility.Collapsed;
        public Visibility lblCh2Visibility
        {
            get { return lblCh2VisibilityValue; }
            set { lblCh2VisibilityValue = value; OnPropertyChanged("lblCh2Visibility"); }
        }

        private Visibility lblCh3VisibilityValue = Visibility.Collapsed;
        public Visibility lblCh3Visibility
        {
            get { return lblCh3VisibilityValue; }
            set { lblCh3VisibilityValue = value; OnPropertyChanged("lblCh3Visibility"); }
        }

        private Visibility lblCh4VisibilityValue = Visibility.Collapsed;
        public Visibility lblCh4Visibility
        {
            get { return lblCh4VisibilityValue; }
            set { lblCh4VisibilityValue = value; OnPropertyChanged("lblCh4Visibility"); }
        }

        private Visibility lblCh5VisibilityValue = Visibility.Collapsed;
        public Visibility lblCh5Visibility
        {
            get { return lblCh5VisibilityValue; }
            set { lblCh5VisibilityValue = value; OnPropertyChanged("lblCh5Visibility"); }
        }

        private Visibility lblCh6VisibilityValue = Visibility.Collapsed;
        public Visibility lblCh6Visibility
        {
            get { return lblCh6VisibilityValue; }
            set { lblCh6VisibilityValue = value; OnPropertyChanged("lblCh6Visibility"); }
        }

        private Visibility lblCh7VisibilityValue = Visibility.Collapsed;
        public Visibility lblCh7Visibility
        {
            get { return lblCh7VisibilityValue; }
            set { lblCh7VisibilityValue = value; OnPropertyChanged("lblCh7Visibility"); }
        }
        
        private Visibility lblCh8VisibilityValue = Visibility.Collapsed;
        public Visibility lblCh8Visibility
        {
            get { return lblCh8VisibilityValue; }
            set { lblCh8VisibilityValue = value; OnPropertyChanged("lblCh8Visibility"); }
        }

        private Visibility txtCh2VisibilityValue = Visibility.Collapsed;
        public Visibility txtCh2Visibility
        {
            get { return txtCh2VisibilityValue; }
            set { txtCh2VisibilityValue = value; OnPropertyChanged("txtCh2Visibility"); }
        }

        private Visibility txtCh3VisibilityValue = Visibility.Collapsed;
        public Visibility txtCh3Visibility
        {
            get { return txtCh3VisibilityValue; }
            set { txtCh3VisibilityValue = value; OnPropertyChanged("txtCh3Visibility"); }
        }

        private Visibility txtCh4VisibilityValue = Visibility.Collapsed;
        public Visibility txtCh4Visibility
        {
            get { return txtCh4VisibilityValue; }
            set { txtCh4VisibilityValue = value; OnPropertyChanged("txtCh4Visibility"); }
        }

        private Visibility txtCh5VisibilityValue = Visibility.Collapsed;
        public Visibility txtCh5Visibility
        {
            get { return txtCh5VisibilityValue; }
            set { txtCh5VisibilityValue = value; OnPropertyChanged("txtCh5Visibility"); }
        }

        private Visibility txtCh6VisibilityValue = Visibility.Collapsed;
        public Visibility txtCh6Visibility
        {
            get { return txtCh6VisibilityValue; }
            set { txtCh6VisibilityValue = value; OnPropertyChanged("txtCh6Visibility"); }
        }

        private Visibility txtCh7VisibilityValue = Visibility.Collapsed;
        public Visibility txtCh7Visibility
        {
            get { return txtCh7VisibilityValue; }
            set { txtCh7VisibilityValue = value; OnPropertyChanged("txtCh7Visibility"); }
        }

        private Visibility txtCh8VisibilityValue = Visibility.Collapsed;
        public Visibility txtCh8Visibility
        {
            get { return txtCh8VisibilityValue; }
            set { txtCh8VisibilityValue = value; OnPropertyChanged("txtCh8Visibility"); }
        }

        private string txtCh1ContentValue = null;
        public string txtCh1Content
        {
            get { return txtCh1ContentValue; }
            set { txtCh1ContentValue = value; OnPropertyChanged("txtCh1Content"); }
        }

        private string txtCh2ContentValue = null;
        public string txtCh2Content
        {
            get { return txtCh2ContentValue; }
            set { txtCh2ContentValue = value; OnPropertyChanged("txtCh2Content"); }
        }

        private string txtCh3ContentValue = null;
        public string txtCh3Content
        {
            get { return txtCh3ContentValue; }
            set { txtCh3ContentValue = value; OnPropertyChanged("txtCh3Content"); }
        }

        private string txtCh4ContentValue = null;
        public string txtCh4Content
        {
            get { return txtCh4ContentValue; }
            set { txtCh4ContentValue = value; OnPropertyChanged("txtCh4Content"); }
        }

        private string txtCh5ContentValue = null;
        public string txtCh5Content
        {
            get { return txtCh5ContentValue; }
            set { txtCh5ContentValue = value; OnPropertyChanged("txtCh5Content"); }
        }

        private string txtCh6ContentValue = null;
        public string txtCh6Content
        {
            get { return txtCh6ContentValue; }
            set { txtCh6ContentValue = value; OnPropertyChanged("txtCh6Content"); }
        }

        private string txtCh7ContentValue = null;
        public string txtCh7Content
        {
            get { return txtCh7ContentValue; }
            set { txtCh7ContentValue = value; OnPropertyChanged("txtCh7Content"); }
        }

        private string txtCh8ContentValue = null;
        public string txtCh8Content
        {
            get { return txtCh8ContentValue; }
            set { txtCh8ContentValue = value; OnPropertyChanged("txtCh8Content"); }
        }

        private string txt_THDfrequencyValue = null;
        public string txt_THDfrequency
        {
            get { return txt_THDfrequencyValue; }
            set
            {
                txt_THDfrequencyValue = value; OnPropertyChanged("txt_THDfrequency");
            }
        }

        private List<string> cmb_THDHighPassFilterListValue = new List<string> { "Elliptic", "SignalPath", "Butterworth" };
        public List<string> cmb_THDHighPassFilterList
        {
            get { return cmb_THDHighPassFilterListValue; }
            set { cmb_THDHighPassFilterListValue = value; OnPropertyChanged("cmb_THDHighPassFilterList"); }
        }

        private string cmb_THDHighPassFilterValue = null;
        public string cmb_THDHighPassFilter
        {
            get { return cmb_THDHighPassFilterValue; }
            set
            {
                cmb_THDHighPassFilterValue = value;
                OnPropertyChanged("cmb_THDHighPassFilter");

                if (cmb_THDHighPassFilterValue == "Butterworth" | cmb_THDHighPassFilterValue == "Elliptic")
                {
                    TxtHighpassVisible = Visibility.Visible;
                }
                else
                {
                    TxtHighpassVisible = Visibility.Collapsed;
                }
            }
        }

        private List<string> cmb_THDLowPassFilterListValue = new List<string> { "Elliptic", "SignalPath", "Butterworth" };
        public List<string> cmb_THDLowPassFilterList
        {
            get { return cmb_THDLowPassFilterListValue; }
            set { cmb_THDLowPassFilterListValue = value; OnPropertyChanged("cmb_THDLowPassFilterList"); }
        }

        private string cmb_THDLowPassFilterValue = null;
        public string cmb_THDLowPassFilter
        {
            get { return cmb_THDLowPassFilterValue; }
            set
            {
                cmb_THDLowPassFilterValue = value;
                OnPropertyChanged("cmb_THDLowPassFilter");

                if (cmb_THDLowPassFilterValue == "Butterworth" | cmb_THDLowPassFilterValue == "Elliptic")
                {
                    TxtLowpassVisible = Visibility.Visible;
                }
                else
                {
                    TxtLowpassVisible = Visibility.Collapsed;
                }
            }
        }

        private List<string> cmb_THDWeightingListValue = new List<string> { "Signal Path", "A-wt.", "B-wt.", "C-wt.", "CCIR-1k", "CCIR-2k", "CCITT", "C-Message", "50 us de-emph.", "75 us de-emph.", "50 us de-emph. + A-wt.", "75 us de-emph. + A-wt." };
        public List<string> cmb_THDWeightingList
        {
            get { return cmb_THDWeightingListValue; }
            set { cmb_THDWeightingListValue = value; OnPropertyChanged("cmb_THDWeightingList"); }
        }

        private string cmb_THDWeightingValue = null;
        public string cmb_THDWeighting
        {
            get { return cmb_THDWeightingValue; }
            set
            {
                cmb_THDWeightingValue = value;
                OnPropertyChanged("cmb_THDWeighting");
            }
        }

        private List<string> cmb_THDTuningModeListValue = new List<string> { "GeneratorFrequency", "JitterGeneratorFrequency", "MeasuredFrequency", "FixedFrequency" };
        public List<string> cmb_THDTuningModeList
        {
            get { return cmb_THDTuningModeListValue; }
            set { cmb_THDTuningModeListValue = value; OnPropertyChanged("cmb_THDTuningModeList"); }
        }

        private string cmb_THDTuningModeValue = null;
        public string cmb_THDTuningMode
        {
            get { return cmb_THDTuningModeValue; }
            set
            {
                cmb_THDTuningModeValue = value;
                OnPropertyChanged("cmb_THDTuningMode");

                if (cmb_THDTuningModeValue == "FixedFrequency")
                {
                    lblFilterFrequencyVisible = Visibility.Visible;
                    txtFilterFrequencyVisible = Visibility.Visible;
                }
                else
                {
                    lblFilterFrequencyVisible = Visibility.Collapsed;
                    txtFilterFrequencyVisible = Visibility.Collapsed;
                }
            }
        }

        private bool btn_THDCh1Value = true;
        public bool btn_THDCh1
        {
            get { return btn_THDCh1Value; }
            set
            {
                btn_THDCh1Value = value;
                OnPropertyChanged("btn_THDCh1");
            }
        }

        private bool btn_THDCh2Value = true;
        public bool btn_THDCh2
        {
            get { return btn_THDCh2Value; }
            set
            {
                btn_THDCh2Value = value;
                OnPropertyChanged("btn_THDCh2");
            }
        }

        private bool btn_THDCh3Value = true;
        public bool btn_THDCh3
        {
            get { return btn_THDCh3Value; }
            set
            {
                btn_THDCh3Value = value;
                OnPropertyChanged("btn_THDCh3");
            }
        }

        private bool btn_THDCh4Value = true;
        public bool btn_THDCh4
        {
            get { return btn_THDCh4Value; }
            set
            {
                btn_THDCh4Value = value;
                OnPropertyChanged("btn_THDCh4");
            }
        }

        private bool btn_THDCh5Value = true;
        public bool btn_THDCh5
        {
            get { return btn_THDCh5Value; }
            set
            {
                btn_THDCh5Value = value;
                OnPropertyChanged("btn_THDCh5");
            }
        }

        private bool btn_THDCh6Value = true;
        public bool btn_THDCh6
        {
            get { return btn_THDCh6Value; }
            set
            {
                btn_THDCh6Value = value;
                OnPropertyChanged("btn_THDCh6");
            }
        }

        private bool btn_THDCh7Value = true;
        public bool btn_THDCh7
        {
            get { return btn_THDCh7Value; }
            set
            {
                btn_THDCh7Value = value;
                OnPropertyChanged("btn_THDCh7");
            }
        }

        private bool btn_THDCh8Value = true;
        public bool btn_THDCh8
        {
            get { return btn_THDCh8Value; }
            set
            {
                btn_THDCh8Value = value;
                OnPropertyChanged("btn_THDCh8");
            }
        }

        private Visibility chkThdnCh1EnableVisibleValue = Visibility.Collapsed;
        public Visibility chkThdnCh1EnableVisible
        {
            get { return chkThdnCh1EnableVisibleValue; }
            set
            {
                chkThdnCh1EnableVisibleValue = value;
                OnPropertyChanged("chkThdnCh1EnableVisible");
            }
        }

        private Visibility chkThdnCh2EnableVisibleValue = Visibility.Collapsed;
        public Visibility chkThdnCh2EnableVisible
        {
            get { return chkThdnCh2EnableVisibleValue; }
            set
            {
                chkThdnCh2EnableVisibleValue = value;
                OnPropertyChanged("chkThdnCh2EnableVisible");
            }
        }

        private Visibility chkThdnCh3EnableVisibleValue = Visibility.Collapsed;
        public Visibility chkThdnCh3EnableVisible
        {
            get { return chkThdnCh3EnableVisibleValue; }
            set
            {
                chkThdnCh3EnableVisibleValue = value;
                OnPropertyChanged("chkThdnCh3EnableVisible");
            }
        }

        private Visibility chkThdnCh4EnableVisibleValue = Visibility.Collapsed;
        public Visibility chkThdnCh4EnableVisible
        {
            get { return chkThdnCh4EnableVisibleValue; }
            set
            {
                chkThdnCh4EnableVisibleValue = value;
                OnPropertyChanged("chkThdnCh4EnableVisible");
            }
        }

        private Visibility chkThdnCh5EnableVisibleValue = Visibility.Collapsed;
        public Visibility chkThdnCh5EnableVisible
        {
            get { return chkThdnCh5EnableVisibleValue; }
            set
            {
                chkThdnCh5EnableVisibleValue = value;
                OnPropertyChanged("chkThdnCh5EnableVisible");
            }
        }

        private Visibility chkThdnCh6EnableVisibleValue = Visibility.Collapsed;
        public Visibility chkThdnCh6EnableVisible
        {
            get { return chkThdnCh6EnableVisibleValue; }
            set
            {
                chkThdnCh6EnableVisibleValue = value;
                OnPropertyChanged("chkThdnCh6EnableVisible");
            }
        }

        private Visibility chkThdnCh7EnableVisibleValue = Visibility.Collapsed;
        public Visibility chkThdnCh7EnableVisible
        {
            get { return chkThdnCh7EnableVisibleValue; }
            set
            {
                chkThdnCh7EnableVisibleValue = value;
                OnPropertyChanged("chkThdnCh7EnableVisible");
            }
        }

        private Visibility chkThdnCh8EnableVisibleValue = Visibility.Collapsed;
        public Visibility chkThdnCh8EnableVisible
        {
            get { return chkThdnCh8EnableVisibleValue; }
            set
            {
                chkThdnCh8EnableVisibleValue = value;
                OnPropertyChanged("chkThdnCh8EnableVisible");
            }
        }

        private int OutChannelCountValue = 0;
        public int OutChannelCount
        {
            get { return OutChannelCountValue; }
            set
            {
                OutChannelCountValue = value;
                OnPropertyChanged("OutChannelCount");

                if (value > 0)
                    chkThdnCh1EnableVisible  = Visibility.Visible;
                else
                    chkThdnCh1EnableVisible = Visibility.Collapsed;

                if (value > 1)
                    chkThdnCh2EnableVisible = Visibility.Visible;
                else
                    chkThdnCh2EnableVisible = Visibility.Collapsed;

                if (value > 2)
                    chkThdnCh3EnableVisible = Visibility.Visible;
                else
                    chkThdnCh3EnableVisible = Visibility.Collapsed;

                if (value > 3)
                    chkThdnCh4EnableVisible = Visibility.Visible;
                else
                    chkThdnCh4EnableVisible = Visibility.Collapsed;

                if (value > 4)
                    chkThdnCh5EnableVisible = Visibility.Visible;
                else
                    chkThdnCh5EnableVisible = Visibility.Collapsed;

                if (value > 5)
                    chkThdnCh6EnableVisible = Visibility.Visible;
                else
                    chkThdnCh6EnableVisible = Visibility.Collapsed;

                if (value > 6)
                    chkThdnCh7EnableVisible = Visibility.Visible;
                else
                    chkThdnCh7EnableVisible = Visibility.Collapsed;

                if (value > 7)
                    chkThdnCh8EnableVisible = Visibility.Visible;
                else
                    chkThdnCh8EnableVisible = Visibility.Collapsed;
            }
        }

        private string TxtHighpassvalue = null;
        public string TxtHighpass
        {
            get { return TxtHighpassvalue; }
            set { TxtHighpassvalue = value; OnPropertyChanged("TxtHighpass"); }
        }

        private Visibility TxtHighpassVisibleValue = Visibility.Collapsed;
        public Visibility TxtHighpassVisible
        {
            get { return TxtHighpassVisibleValue; }
            set { TxtHighpassVisibleValue = value; OnPropertyChanged("TxtHighpassVisible"); }
        }

        private string TxtLowpassvalue = null;
        public string TxtLowpass
        {
            get { return TxtLowpassvalue; }
            set { TxtLowpassvalue = value; OnPropertyChanged("TxtLowpass"); }
        }

        private Visibility TxtLowpassVisibleValue = Visibility.Collapsed;
        public Visibility TxtLowpassVisible
        {
            get { return TxtLowpassVisibleValue; }
            set { TxtLowpassVisibleValue = value; OnPropertyChanged("TxtLowpassVisible"); }
        }

        private Visibility lblFilterFrequencyVisibleValue = Visibility.Collapsed;
        public Visibility lblFilterFrequencyVisible
        {
            get { return lblFilterFrequencyVisibleValue; }
            set { lblFilterFrequencyVisibleValue = value; OnPropertyChanged("lblFilterFrequencyVisible"); }
        }

        private string txtFilterFrequencyValue = null;
        public string txtFilterFrequency
        {
            get { return txtFilterFrequencyValue; }
            set { txtFilterFrequencyValue = value; OnPropertyChanged("txtFilterFrequency"); }
        }

        private Visibility txtFilterFrequencyVisibleValue = Visibility.Collapsed;
        public Visibility txtFilterFrequencyVisible
        {
            get { return txtFilterFrequencyVisibleValue; }
            set { txtFilterFrequencyVisibleValue = value; OnPropertyChanged("txtFilterFrequencyVisible"); }
        }
        
        private string txtTHDNVerificationValue = null;
        public string txtTHDNVerification
        {
            get { return txtTHDNVerificationValue; }
            set { txtTHDNVerificationValue = value; OnPropertyChanged("txtTHDNVerification"); }
        }

        private string txtTHDNVerificationPathValue = null;
        public string txtTHDNVerificationPath
        {
            get { return txtTHDNVerificationPathValue; }
            set { txtTHDNVerificationPathValue = value; OnPropertyChanged("txtTHDNVerificationPath"); }
        }

        private TestApxItem parentTestApxItemValue = null;
        public TestApxItem ParentTestApxItem
        {
            get { return parentTestApxItemValue; }
            set { parentTestApxItemValue = value; OnPropertyChanged("ParentTestApxItem"); }
        }
    }
}

