using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace QSC_Test_Automation
{
    public class TreeViewExplorer : INotifyPropertyChanged,ICloneable
    {
        public string copyItemName = null;

        private TreeViewExplorer parentValue;
        private TextBox textBoxItemValue = new TextBox();
        private int itemKeyValue;
        private string ExecutionStatusValue="Pass";
        private string ExecutionBackgroundstartValue = "Pass";
        private string ExecutionBackgroundendValue = "Pass";
        private string ExecutionIncompleteStatusValue = "Incomplete";
        private string itemTypeValue;
        private string itemNameValue;

        private bool? isCheckedValue = true;
        private Visibility isCheckedVisibilityValue = Visibility.Visible;
        private Visibility isImageVisibilityValue = Visibility.Collapsed;

        private bool isExpandedValue = false;
        private bool isEnabledValue = true;
        private bool isSelectedValue = false;
        private bool isMultiSelectOnValue = false;

        private bool isNewMenuItemEnabledValue = true;
        private bool isOpenMenuItemEnabledValue = true;
        private bool isEditMenuItemEnabledValue = true;
        private bool isRenameMenuItemEnabledValue = true;
        private bool isDeleteMenuItemEnabledValue = true;
        private bool isCopyMenuItemEnabledValue = true;
        private bool isPasteMenuItemEnabledValue = false;
        private bool isPropertiesMenuItemEnabledValue = true;
        private bool isRefreshMenuItemEnabledValue = true;
        private bool IssortByMenuItemVisibleEnabledValue = false;

        private Visibility isNewMenuItemVisibleValue = Visibility.Collapsed;
        private Visibility isOpenMenuItemVisibleValue = Visibility.Collapsed;
        private Visibility isEditMenuItemVisibleValue = Visibility.Collapsed;
        private Visibility isRenameMenuItemVisibleValue = Visibility.Collapsed;
        private Visibility isDeleteMenuItemVisibleValue = Visibility.Collapsed;
        private Visibility isCopyMenuItemVisibleValue = Visibility.Collapsed;
        private Visibility isPasteMenuItemVisibleValue = Visibility.Collapsed;      
        private Visibility isRefreshMenuItemVisibleValue = Visibility.Visible;
        private Visibility IssortByMenuItemVisibleValue = Visibility.Collapsed;

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
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                //MessageBox.Show("Exception\n " + ex.Message, "Error Code - EC18001", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public object Clone()
        {
            return MemberwiseClone();
        }

        private void TextBoxItem_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                    RenameItem();
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                //MessageBox.Show("Exception\n " + ex.Message, "Error Code - EC18002", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void ItemTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsRenameModeEnabled)
            ItemName = ItemTextBox.Text;
        }


        private void ItemTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            try
            {
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
                if (ItemTextBox.Focusable == true)
                {
                    IsRenameModeEnabled = false;
                    ItemTextBox.IsHitTestVisible = false;
                    ItemTextBox.Focusable = false;
                    ItemTextBox.Cursor = Cursors.Arrow;
                    ItemTextBox.Background = Brushes.Transparent;
                    ItemTextBox.SelectionLength = 0;

                    DBConnection QscDatabase = new DBConnection();

                    QscDatabase.RenameTreeItem(this);
                    if (DesignerWindow != null)
                    {
                        DesignerWindow.OpenTreeViewItem(this, false);
                        DesignerWindow.isRenameModeEnabled = false;
                    }
                    ItemTextBox.IsReadOnly = true;

                    if (textBoxItemValue.IsReadOnly && ChildrenCountViewIsEnabled)
                        textBoxItemValue.Text = ItemName + " (" + ChildrenCountForView + ")";
                    else if (textBoxItemValue.IsReadOnly && DesignNameViewIsEnabled)
                        textBoxItemValue.Text = ItemName + " (" + DesignName + ")";
                    else
                        textBoxItemValue.Text = ItemName;

                }
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                // MessageBox.Show("Exception\n " + ex.Message, "Error Code - EC18004", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public TreeViewExplorer(int itemKey, string itemName, string itemType, Test_Execution executionWindow, Test_Designer designerWindow, DateTime? createdon,string createdby, DateTime? modifiedon,string modifiedby,string summary,string category, DateTime? importedon,string importedby,int childrenCountForView,bool hasDesign)
        {
            try
            {
                ItemKey = itemKey;
                ItemName = itemName;
                ItemType = itemType;
                ExecutionWindow = executionWindow;
                DesignerWindow = designerWindow;
                Createdon = createdon;
                Createdby = createdby;
                Modifiedon = modifiedon;
                Modifiedby = modifiedby;
                Summary = summary;
                Category = category;
                ImportedOn = importedon;
                Importedby = importedby;
                ChildrenCountForView = childrenCountForView;
                HasDesign = hasDesign;

                Initialize(this);
                SetParent();
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                //MessageBox.Show("Exception\n " + ex.Message, "Error Code - EC18005", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public TreeViewExplorer(int itemKey, string itemName, string itemType, Test_Execution executionWindow, Test_Designer designerWindow)
        {
            try
            {
                ItemKey = itemKey;
                ItemName = itemName;
                ItemType = itemType;
                ExecutionWindow = executionWindow;
                DesignerWindow = designerWindow;
                Initialize(this);
                SetParent();
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                //MessageBox.Show("Exception\n " + ex.Message, "Error Code - EC18005", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public TreeViewExplorer(TreeViewExplorer sourceItem)
        {
            try
            {
                Initialize(sourceItem);

                if (sourceItem.Children.Count > 0)
                {
                    foreach (var item in sourceItem.Children)
                        AddChildren_withCheckbox(new TreeViewExplorer(item));
                }
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                //MessageBox.Show("Exception\n " + ex.Message, "Error Code - EC18006", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public TreeViewExplorer(TreeViewExplorer sourceItem,string itemName)
        {
            try
            {
                ///if array type is a string array without key value pair ItemName becomes empty.(ex:data:["xxx","yyy","zzz"])
                /// If itemname is empty need to hide checkbox
                if (sourceItem.ItemName == null || sourceItem.ItemName == string.Empty)                
                    sourceItem.IsCheckedVisibility = Visibility.Collapsed;
                
                ////Setting default values in value textbox based on itemtype
                SetDefaultValues(sourceItem);

                ////After setting default values in value textbox need to assign ItemNameJSON value otherwise it will shows empty in GUI
                ItemName =  itemName;
                ItemNameJSON = sourceItem.ItemNameJSON;
             
                ////Actual initialization starts here
                Initialize(sourceItem);

                if (sourceItem.Children.Count > 0)
                {
                    foreach (var item in sourceItem.Children)
                        AddChildren_withCheckbox(new TreeViewExplorer(item,item.ItemName));
                }
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                //MessageBox.Show("Exception\n " + ex.Message, "Error Code - EC18006", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void SetDefaultValues(TreeViewExplorer sourceTreeviewExplorer)
        {
            try
            {
                if (sourceTreeviewExplorer == null)
                    return;

                if (sourceTreeviewExplorer.ItemType == "System.Int32" || sourceTreeviewExplorer.ItemType == "System.Int16" || sourceTreeviewExplorer.ItemType == "System.Int64" ||
                     sourceTreeviewExplorer.ItemType == "System.Double" || sourceTreeviewExplorer.ItemType == "System.Decimal")
                {                    
                    sourceTreeviewExplorer.ItemNameJSON = "0";    
                }
                else if (sourceTreeviewExplorer.ItemType == "System.Boolean")
                {                   
                    sourceTreeviewExplorer.ItemNameJSON = "false";                               
                }
                else if(sourceTreeviewExplorer.ItemType=="System.String")
                {
                    sourceTreeviewExplorer.ItemNameJSON = string.Empty;
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


        private void Initialize(TreeViewExplorer sourceItem)
        {
            try
            {
                ItemTextBox.Background = Brushes.Transparent;
                ItemTextBox.BorderThickness = new Thickness(0);
                ItemTextBox.Margin = new Thickness(0);
                ItemTextBox.Padding = new Thickness(0);
                ItemTextBox.IsReadOnly = true;
                ItemTextBox.Focusable = false;
                ItemTextBox.SelectionLength = 0;
                ItemTextBox.Cursor = Cursors.Arrow;
                ItemTextBox.Height = Double.NaN;
                ItemTextBox.MinHeight = 0;
                ItemTextBox.KeyDown += TextBoxItem_KeyDown;
                ItemTextBox.LostFocus += ItemTextBox_LostFocus;
                ItemTextBox.TextChanged += ItemTextBox_TextChanged;
                ItemTextBox.ContextMenu = null;
                ItemTextBox.IsHitTestVisible = false;
                ItemTextBox.Text = ItemName;

                ////<Group> This group values newly added for QRCM treeview
                ItemTextBoxJsonValue.Text = ItemNameJSON;
                ItemTextBoxJsonValue.TextChanged += ItemTextBoxJSON_TextChanged;
                //ItemTextBoxJsonValue.TextWrapping = TextWrapping.Wrap;
                ItemTextBoxJsonValue.MinWidth = 30;
                ItemTextBoxJsonValue.Height = 22;
                ItemTextBoxJsonValue.TextAlignment = TextAlignment.Center;
                ItemTextBoxJsonValue.AcceptsReturn = true;
                //// </Group>    
                
                Children = new List<TreeViewExplorer>();
                
               
                ItemKey = sourceItem.ItemKey;
                ItemName = sourceItem.ItemName;
                ItemType = sourceItem.ItemType;

                ///<Group> This group values newly added for QRCM treeview
                UniqueKeyJSON = sourceItem.UniqueKeyJSON;
                IsCheckedVisibility = sourceItem.IsCheckedVisibility;
                ItemNameJSON = sourceItem.ItemNameJSON;
                PlusBtnJsonVisibility = sourceItem.PlusBtnJsonVisibility;
                MinusBtnJsonVisibility = sourceItem.MinusBtnJsonVisibility;
                ItemTextBoxJsonVisibility = sourceItem.ItemTextBoxJsonVisibility;
                ItemTextBoxJsonUpperLowerVisibility = sourceItem.ItemTextBoxJsonUpperLowerVisibility;
                /// </Group>                

                ExecutionWindow = sourceItem.ExecutionWindow;
                DesignerWindow = sourceItem.DesignerWindow;
                DesignName = sourceItem.DesignName;
                IsExpanded = sourceItem.IsExpanded;
                IsChecked = sourceItem.IsChecked;
                HasDesign = sourceItem.HasDesign;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Exception\n " + ex.Message, "Error Code - EC18007", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public TreeViewExplorer(TreeViewExplorer sourceItem, bool isGetValue)
        {
            try
            {
                InitializeCategory(sourceItem);

                if (isGetValue == true && sourceItem.Children.Count > 0)
                {
                    foreach (var item in sourceItem.Children)
                        AddChildren_withCheckbox(new TreeViewExplorer(item, true));
                }
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                //MessageBox.Show("Exception\n " + ex.Message, "Error Code - EC18006", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void InitializeCategory(TreeViewExplorer sourceItem)
        {
            try
            {
                ItemTextBox.Background = Brushes.Transparent;
                ItemTextBox.BorderThickness = new Thickness(0);
                ItemTextBox.Margin = new Thickness(0);
                ItemTextBox.Padding = new Thickness(0);
                ItemTextBox.IsReadOnly = true;
                ItemTextBox.Focusable = false;
                ItemTextBox.SelectionLength = 0;
                ItemTextBox.Cursor = Cursors.Arrow;
                ItemTextBox.Height = Double.NaN;
                ItemTextBox.MinHeight = 0;
                ItemTextBox.KeyDown += TextBoxItem_KeyDown;
                ItemTextBox.LostFocus += ItemTextBox_LostFocus;
                ItemTextBox.TextChanged += ItemTextBox_TextChanged;
                ItemTextBox.ContextMenu = null;
                ItemTextBox.IsHitTestVisible = false;
                ItemTextBox.Text = ItemName;

                Children = new List<TreeViewExplorer>();

                //ItemKey = sourceItem.ItemKey;
                //ItemName = sourceItem.ItemName;
                //ItemType = sourceItem.ItemType;
                //ExecutionWindow = sourceItem.ExecutionWindow;
                //DesignerWindow = sourceItem.DesignerWindow;
                //DesignName = sourceItem.DesignName;
                //IsExpanded = sourceItem.IsExpanded;
                //IsChecked = sourceItem.IsChecked;


                Category = sourceItem.Category;
                ChildrenCountForView = sourceItem.ChildrenCountForView;
                ChildrenCountViewIsEnabled = sourceItem.ChildrenCountViewIsEnabled;
                Createdby = sourceItem.Createdby;
                Createdon = sourceItem.Createdon;
                DelayValues = sourceItem.DelayValues;
                DesignName = sourceItem.DesignName;
                DesignNameViewIsEnabled = sourceItem.DesignNameViewIsEnabled;
                DesignerWindow = sourceItem.DesignerWindow;
                ExecutionBackgroundend = sourceItem.ExecutionBackgroundend;
                ExecutionBackgroundstart = sourceItem.ExecutionBackgroundstart;
                ExecutionBackgroundend = sourceItem.ExecutionBackgroundend;
                ExecutionDelay = sourceItem.ExecutionDelay;
                ExecutionDelayTime = sourceItem.ExecutionDelayTime;
                ExecutionIncompleteStatus = sourceItem.ExecutionIncompleteStatus;
                ExecutionStatus = sourceItem.ExecutionStatus;
                ExecutionWindow = sourceItem.ExecutionWindow;
                Importedby = sourceItem.Importedby;
                ImportedOn = sourceItem.ImportedOn;
                IsChecked = sourceItem.IsChecked;
                IsCheckedVisibility = sourceItem.IsCheckedVisibility;
                IsDeleteMenuItemEnabled = sourceItem.IsDeleteMenuItemEnabled;
                IsDeleteMenuItemVisible = sourceItem.IsDeleteMenuItemVisible;
                IsEditMenuItemEnabled = sourceItem.IsEditMenuItemEnabled;
                IsEditMenuItemVisible = sourceItem.IsEditMenuItemVisible;
                IsExpanded = sourceItem.IsExpanded;
                IsImageVisibility = sourceItem.IsImageVisibility;
                IsMultiSelectOn = sourceItem.IsMultiSelectOn;
                IsNewMenuItemEnabled = sourceItem.IsNewMenuItemEnabled;
                IsNewMenuItemVisible = sourceItem.IsNewMenuItemVisible;
                IsOpenMenuItemEnabled = sourceItem.IsOpenMenuItemEnabled;
                IsOpenMenuItemVisible = sourceItem.IsOpenMenuItemVisible;
                IsPasteMenuItemEnabled = sourceItem.IsPasteMenuItemEnabled;
                IsPasteMenuItemVisible = sourceItem.IsPasteMenuItemVisible;
                IsPropertiesMenuItemEnabled = sourceItem.IsPropertiesMenuItemEnabled;
                IsRefreshMenuItemEnabled = sourceItem.IsRefreshMenuItemEnabled;
                IsRefreshMenuItemVisible = sourceItem.IsRefreshMenuItemVisible;
                IsRenameMenuItemEnabled = sourceItem.IsRenameMenuItemEnabled;
                IsRenameMenuItemVisible = sourceItem.IsRenameMenuItemVisible;
                IsRenameModeEnabled = sourceItem.IsRenameModeEnabled;
                IsSelected = sourceItem.IsSelected;
                IssortByMenuItemVisible = sourceItem.IssortByMenuItemVisible;
                IssortByMenuItemVisibleEnabled = sourceItem.IssortByMenuItemVisibleEnabled;
                ItemKey = sourceItem.ItemKey;
                ItemName = sourceItem.ItemName;
                ItemTextBox = sourceItem.ItemTextBox;
                ItemType = sourceItem.ItemType;
                lastremvalues = sourceItem.lastremvalues;
                Modifiedby = sourceItem.Modifiedby;
                Modifiedon = sourceItem.Modifiedon;
                Parent = sourceItem.Parent;
                Summary = sourceItem.Summary;
                UpdatestatusInfo = sourceItem.UpdatestatusInfo;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Exception\n " + ex.Message, "Error Code - EC18007", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void AddChildrenList(List<TreeViewExplorer> childrenList)
        {
            try
            {
                Children.AddRange(childrenList);
                foreach (TreeViewExplorer children in Children)
                {
                  
                        children.Parent = this;
                        //VerifyCheckState();
                    
                }
               
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                //MessageBox.Show("Exception\n " + ex.Message, "Error Code - EC18008", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
		
        public void AddChildrenList_withCheckbox(List<TreeViewExplorer> childrenList)
        {
            try
            {
                Children.AddRange(childrenList);
                foreach (TreeViewExplorer children in Children)
                {
                    children.Parent = this;
                     VerifyCheckState();
                    
                }
                
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                //MessageBox.Show("Exception\n " + ex.Message, "Error Code - EC18008", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
		
        public void  AddChildren(TreeViewExplorer children)
        {
            try
            {
                if (!Children.Contains(children))
                {
                    Children.Add(children);
                    children.Parent = this;
                    //VerifyCheckState();
                }
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                //MessageBox.Show("Exception\n " + ex.Message, "Error Code - EC18009", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
		
        public void AddChildren_withCheckbox(TreeViewExplorer children)
        {
            try
            {
                if (!Children.Contains(children))
                {
                    Children.Add(children);
                    children.Parent = this;
                    VerifyCheckState();
                }
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                //MessageBox.Show("Exception\n " + ex.Message, "Error Code - EC18009", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void InsertChildren(int index, TreeViewExplorer item, bool isDesignNameViewEnabled)
        {
            TreeViewExplorer newItem = new TreeViewExplorer(item);
            if (isDesignNameViewEnabled)
            {
                foreach (TreeViewExplorer testPlan in newItem.Children)
                {
                    testPlan.DesignNameViewIsEnabled = true;
                }
            }

            Children.Insert(index, newItem);
            newItem.Parent = this;
        }

        public void RemoveChildrenList(List<TreeViewExplorer> childrenList)
        {
            try
            {
                foreach (TreeViewExplorer children in childrenList)
                    RemoveChildren(children);
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                //MessageBox.Show("Exception\n " + ex.Message, "Error Code - EC18010", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void RemoveChildren(TreeViewExplorer children)
        {
            try
            {
                if (Children.Contains(children))
                {
                    Children.Remove(children);
                    VerifyCheckState();
                }
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                //MessageBox.Show("Exception\n " + ex.Message, "Error Code - EC18011", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void ClearChildren()
        {
            try
            {
                Children = new List<TreeViewExplorer>();
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                //MessageBox.Show("Exception\n " + ex.Message, "Error Code - EC18012", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SetParent()
        {
            try
            {
                foreach (TreeViewExplorer children in Children)
                {
                    children.Parent = this;
                    children.SetParent();
                }
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                //MessageBox.Show("Exception\n " + ex.Message, "Error Code - EC18013", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void SetExecutionWindow(Test_Execution executionWindow)
        {
            try
            {
                ExecutionWindow = executionWindow;
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                //MessageBox.Show("Exception\n " + ex.Message, "Error Code - EC18014", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void SetDesignerWindow(Test_Designer designerWindow)
        {
            try
            {
                DesignerWindow = designerWindow;
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                //MessageBox.Show("Exception\n " + ex.Message, "Error Code - EC18015", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public List<TreeViewExplorer> Children { get; private set; }

        public Test_Execution ExecutionWindow { get; private set; }

        public Test_Designer DesignerWindow { get; private set; }

        public TreeViewExplorer Parent
        {
            get { return parentValue; }
            private set { parentValue = value; }
        }

        public TextBox ItemTextBox
        {
            get { return textBoxItemValue; }
            set { textBoxItemValue = value; OnPropertyChanged("ItemTextBox"); }
        }

        private int childrenCountValue = 0;
        public int ChildrenCountForView
        {
            get { return childrenCountValue; }
            set
            {
                childrenCountValue = value;

                if (textBoxItemValue.IsReadOnly && ChildrenCountViewIsEnabled)
                    textBoxItemValue.Text = ItemName + " (" + ChildrenCountForView + ")";
                else if (textBoxItemValue.IsReadOnly && DesignNameViewIsEnabled)
                    textBoxItemValue.Text = ItemName + " (" + DesignName + ")";
                else
                    textBoxItemValue.Text = ItemName; 

                OnPropertyChanged("ChildrenCountForView");
            }
        }

        private bool childrenCountViewIsEnabledValue = false;
        public bool ChildrenCountViewIsEnabled
        {
            get { return childrenCountViewIsEnabledValue; }
            set
            {
                childrenCountViewIsEnabledValue = value;

                if (textBoxItemValue.IsReadOnly && ChildrenCountViewIsEnabled)
                    textBoxItemValue.Text = ItemName + " (" + ChildrenCountForView + ")";
                else if (textBoxItemValue.IsReadOnly && DesignNameViewIsEnabled)
                    textBoxItemValue.Text = ItemName + " (" + DesignName + ")";
                else
                    textBoxItemValue.Text = ItemName;

                OnPropertyChanged("ChildrenCountViewIsEnabled");
            }
        }

        private bool isRenameModeEnabledValue = false;
        public bool IsRenameModeEnabled
        {
            get { return isRenameModeEnabledValue; }
            set { isRenameModeEnabledValue = value; OnPropertyChanged("IsRenameModeEnabled"); }
        }

        public int ItemKey
        {
            get { return itemKeyValue; }
            set { itemKeyValue = value; OnPropertyChanged("ItemKey"); }
        }

        public string ExecutionStatus
        {
            get { return ExecutionStatusValue; }
            set { ExecutionStatusValue = value; OnPropertyChanged("ExecutionStatus"); }
        }
        
        public string ExecutionIncompleteStatus
        {
            get { return ExecutionIncompleteStatusValue; }
            set { ExecutionIncompleteStatusValue = value; OnPropertyChanged("ExecutionIncompleteStatus"); }
        }

        public string ExecutionBackgroundstart
        {
            get { return ExecutionBackgroundstartValue; }
            set { ExecutionBackgroundstartValue = value; OnPropertyChanged("ExecutionBackgroundstart"); }
        }


        public string ExecutionBackgroundend
        {
            get { return ExecutionBackgroundendValue; }
            set { ExecutionBackgroundendValue = value; OnPropertyChanged("ExecutionBackgroundend"); }
        }

        public string ItemName
        {
            get { return itemNameValue; }
            set
            {
                itemNameValue = value;

                if (textBoxItemValue.IsReadOnly && ChildrenCountViewIsEnabled)
                    textBoxItemValue.Text = ItemName + " (" + ChildrenCountForView + ")";
                else if (textBoxItemValue.IsReadOnly && DesignNameViewIsEnabled)
                    textBoxItemValue.Text = ItemName + " (" + DesignName + ")";
                else
                    textBoxItemValue.Text = ItemName;

                OnPropertyChanged("ItemName");
        }
        }

        private string isUpdatestatusInfoValue;     //for PAss/Fail Indicatior

        private string designNameValue = null;
        public string DesignName
        {
            get { return designNameValue; }
            set
            {
                designNameValue = value;

                if (textBoxItemValue.IsReadOnly && ChildrenCountViewIsEnabled)
                    textBoxItemValue.Text = ItemName + " (" + ChildrenCountForView + ")";
                else if (textBoxItemValue.IsReadOnly && DesignNameViewIsEnabled)
                    textBoxItemValue.Text = ItemName + " (" + DesignName + ")";
                else
                    textBoxItemValue.Text = ItemName;

                OnPropertyChanged("DesignName");
            }
        }

        private bool designNameViewIsEnabledValue = false;
        public bool DesignNameViewIsEnabled
        {
            get { return designNameViewIsEnabledValue; }
            set
            {
                designNameViewIsEnabledValue = value;

                if (textBoxItemValue.IsReadOnly && ChildrenCountViewIsEnabled)
                    textBoxItemValue.Text = ItemName + " (" + ChildrenCountForView + ")";
                else if (textBoxItemValue.IsReadOnly && DesignNameViewIsEnabled)
                {
                    textBoxItemValue.Text = ItemName + " [ Design: " + DesignName + " ]";
                  
                }
                else
                    textBoxItemValue.Text = ItemName;

                OnPropertyChanged("DesignNameViewIsEnabled");
            }
        }

        public string UpdatestatusInfo
        {
            get { return isUpdatestatusInfoValue; }
            set
            {
                isUpdatestatusInfoValue = value;
                OnPropertyChanged("UpdatestatusInfo");
            }
        }
        private bool hasDesignValue = true;
        public bool HasDesign
        {
            get { return hasDesignValue; }
            set
            {
                hasDesignValue = value;
                OnPropertyChanged("HasDesign");
            }
        }
        public string ItemType
        {
            get { return itemTypeValue; }
            set { itemTypeValue = value; OnPropertyChanged("ItemType"); }
        }

        private string CreatedbyValue = null;
        public string Createdby
        {
            get { return CreatedbyValue; }
            set { CreatedbyValue = value; OnPropertyChanged("Createdby"); }
        }

        private DateTime? CreatedonValue =null ;
        public DateTime? Createdon
        {
            get { return CreatedonValue; }
            set { CreatedonValue = value; OnPropertyChanged("Createdon"); }
        }

        private string ModifiedbyValue = null;
        public string Modifiedby
        {
            get { return ModifiedbyValue; }
            set { ModifiedbyValue = value; OnPropertyChanged("Modifiedby"); }
        }
        private string ImportedbyValue = null;
        public string Importedby
        {
            get { return ImportedbyValue; }
            set { ImportedbyValue = value; OnPropertyChanged("Importedby"); }
        }
        private DateTime? ModifiedonValue =null;
        public DateTime? Modifiedon
        {
            get { return ModifiedonValue; }
            set { ModifiedonValue = value; OnPropertyChanged("Modifiedon"); }
        }

        private DateTime? ImportedOnValue = null;
        public DateTime? ImportedOn
        {
            get { return ImportedOnValue; }
            set { ImportedOnValue = value; OnPropertyChanged("ImportedOn"); }
        }

        private string SummaryValue = null;
        public string Summary
        {
            get { return SummaryValue; }
            set { SummaryValue = value; OnPropertyChanged("Summary"); }
        }

        private string ExecutionDelayTimeValue ="Min";
        public string ExecutionDelayTime
        {
            get {             
                return ExecutionDelayTimeValue;
            }
            set {
                ExecutionDelayTimeValue = value;   
                OnPropertyChanged("ExecutionDelayTime");
            }
        }


        private string ExecutionDelayValue = null;
        public string ExecutionDelay
        {
            get { return ExecutionDelayValue; }
            set
            {              
                ExecutionDelayValue = value;
                OnPropertyChanged("ExecutionDelay");
            }
        }

        private ObservableCollection<string> _DelayValues = new ObservableCollection<string> { "Hour", "Min", "Sec" };
        public ObservableCollection<string> DelayValues
        {
            get
            {
                return _DelayValues;
            }
            set
            {
                _DelayValues = value;
                OnPropertyChanged("DelayValues");
            }
        }

        private string CategoryValue = null;
        public string Category
        {
            get { return CategoryValue; }
            set { CategoryValue = value; OnPropertyChanged("Category"); }
        }

        public bool? IsChecked
        {
            get { return isCheckedValue; }
            set { SetIsChecked(value, true, true); }
        }

        void SetIsChecked(bool? value, bool updateChildrern, bool updateParent)
        {
            if (value == isCheckedValue)
                return;

            isCheckedValue = value;

            if (updateChildrern && isCheckedValue.HasValue && Children != null)
                Children.ForEach(c => c.SetIsChecked(isCheckedValue, true, false));
            
            if (updateParent && Parent != null)
                Parent.VerifyCheckState();

            OnPropertyChanged("IsChecked");
        }

        void VerifyCheckState()
        {
            bool? parentState = null;
            for (int i = 0; i < Children.Count; i++)
            {
                bool? currentState = Children[i].isCheckedValue;
                if (i == 0)
                {
                    parentState = currentState;
                }
                else if (parentState != currentState)
                {
                    parentState = null;
                    break;
                }
            }

            SetIsChecked(parentState, false, true);
        }

        public Visibility IsCheckedVisibility
        {
            get { return isCheckedVisibilityValue; }
            set
            {
                isCheckedVisibilityValue = value;
                OnPropertyChanged("IsCheckedVisibility");
            }
        }

        public Visibility IsImageVisibility
        {
            get { return isImageVisibilityValue; }
            set
            {
                isImageVisibilityValue = value;
                OnPropertyChanged("IsImageVisibility");
            }
        }

        public bool IsExpanded
        {
            get { return isExpandedValue; }
            set
            {
                isExpandedValue = value;
                OnPropertyChanged("IsExpanded");
            }
        }
  
        public bool IsEnabled
        {
            get { return isEnabledValue; }
            set
            {
                isEnabledValue = value;
                OnPropertyChanged("IsEnabled");
            }
        }
        public bool IsSelected
        {
            get { return isSelectedValue; }
            set
            {
                isSelectedValue = value;
                OnPropertyChanged("IsSelected");
            }
        }

        public bool IsMultiSelectOn
        {
            get { return isMultiSelectOnValue; }
            set
            {
                isMultiSelectOnValue = value;
                OnPropertyChanged("IsMultiSelectOn");
            }
        }

        public bool IsNewMenuItemEnabled
        {
            get { return isNewMenuItemEnabledValue; }
            set
            {
                isNewMenuItemEnabledValue = value;
                OnPropertyChanged("IsNewMenuItemEnabled");
            }
        }

        public bool IsOpenMenuItemEnabled
        {
            get { return isOpenMenuItemEnabledValue; }
            set
            {
                isOpenMenuItemEnabledValue = value;
                OnPropertyChanged("IsOpenMenuItemEnabled");
            }
        }

        public bool IsEditMenuItemEnabled
        {
            get { return isEditMenuItemEnabledValue; }
            set
            {
                isEditMenuItemEnabledValue = value;
                OnPropertyChanged("IsEditMenuItemEnabled");
            }
        }

        public bool IsRenameMenuItemEnabled
        {
            get { return isRenameMenuItemEnabledValue; }
            set
            {
                isRenameMenuItemEnabledValue = value;
                OnPropertyChanged("IsRenameMenuItemEnabled");
            }
        }

        public bool IsDeleteMenuItemEnabled
        {
            get { return isDeleteMenuItemEnabledValue; }
            set
            {
                isDeleteMenuItemEnabledValue = value;
                OnPropertyChanged("IsDeleteMenuItemEnabled");
            }
        }

        public bool IsCopyMenuItemEnabled
        {
            get { return isCopyMenuItemEnabledValue; }
            set
            {
                isCopyMenuItemEnabledValue = value;
                OnPropertyChanged("IsCopyMenuItemEnabled");
            }
        }

        public bool IsPasteMenuItemEnabled
        {
            get { return isPasteMenuItemEnabledValue; }
            set
            {
                isPasteMenuItemEnabledValue = value;
                OnPropertyChanged("IsPasteMenuItemEnabled");
            }
        }

        public bool IsPropertiesMenuItemEnabled
        {
            get { return isPropertiesMenuItemEnabledValue; }
            set
            {
                isPropertiesMenuItemEnabledValue = value;
                OnPropertyChanged("IsPropertiesMenuItemEnabled");
            }
        }

        public bool IsRefreshMenuItemEnabled
        {
            get { return isRefreshMenuItemEnabledValue; }
            set
            {
                isRefreshMenuItemEnabledValue = value;
                OnPropertyChanged("IsRefreshMenuItemEnabled");
            }
        }

        public bool IssortByMenuItemVisibleEnabled
        {
            get { return IssortByMenuItemVisibleEnabledValue; }
            set
            {
                IssortByMenuItemVisibleEnabledValue = value;
                OnPropertyChanged("IssortByMenuItemVisibleEnabled");
            }
        }


        public Visibility IsNewMenuItemVisible
        {
            get { return isNewMenuItemVisibleValue; }
            set
            {
                isNewMenuItemVisibleValue = value;
                OnPropertyChanged("IsNewMenuItemVisible");
            }
        }

        public Visibility IsOpenMenuItemVisible
        {
            get { return isOpenMenuItemVisibleValue; }
            set
            {
                isOpenMenuItemVisibleValue = value;
                OnPropertyChanged("IsOpenMenuItemVisible");
            }
        }

        public Visibility IsEditMenuItemVisible
        {
            get { return isEditMenuItemVisibleValue; }
            set
            {
                isEditMenuItemVisibleValue = value;
                OnPropertyChanged("IsEditMenuItemVisible");
            }
        }

        public Visibility IsRenameMenuItemVisible
        {
            get { return isRenameMenuItemVisibleValue; }
            set
            {
                isRenameMenuItemVisibleValue = value;
                OnPropertyChanged("IsRenameMenuItemVisible");
            }
        }

        public Visibility IsDeleteMenuItemVisible
        {
            get { return isDeleteMenuItemVisibleValue; }
            set
            {
                isDeleteMenuItemVisibleValue = value;
                OnPropertyChanged("IsDeleteMenuItemVisible");
            }
        }

        public Visibility IsCopyMenuItemVisible
        {
            get { return isCopyMenuItemVisibleValue; }
            set
            {
                isCopyMenuItemVisibleValue = value;
                OnPropertyChanged("IsCopyMenuItemVisible");
            }
        }

        public Visibility IsPasteMenuItemVisible
        {
            get { return isPasteMenuItemVisibleValue; }
            set
            {
                isPasteMenuItemVisibleValue = value;
                OnPropertyChanged("IsPasteMenuItemVisible");
            }
        }

      

        public Visibility IsRefreshMenuItemVisible
        {
            get { return isRefreshMenuItemVisibleValue; }
            set
            {
                isRefreshMenuItemVisibleValue = value;
                OnPropertyChanged("IsRefreshMenuItemVisible");
            }
        }

        public Visibility IssortByMenuItemVisible
        {
            get { return IssortByMenuItemVisibleValue; }
            set
            {
                IssortByMenuItemVisibleValue = value;
                OnPropertyChanged("IssortByMenuItemVisible");
            }
        }

        private ObservableCollection<GetExecutionLoop> _dataGridCollection = new ObservableCollection<GetExecutionLoop>();

        public ObservableCollection<GetExecutionLoop> DataGridCollection
        {
            get { return _dataGridCollection; }
            set { _dataGridCollection = value; }
        }


        private bool _lastremvalues = true;
        public bool lastremvalues
        {
            get { return _lastremvalues; }
            set { _lastremvalues = value; }
        }

        private int _PASS = 0;
        public int PASS
        {
            get { return _PASS; }
            set
            {
                _PASS = value;
                OnPropertyChanged("PASS");
            }
        }

        private int _FAIL = 0;
        public int FAIL
        {
            get { return _FAIL; }
            set
            {
                _FAIL = value;
                OnPropertyChanged("FAIL");
            }
        }

        private int _EXEC = 0;
        public int EXEC
        {
            get { return _EXEC; }
            set
            {
                _EXEC = value;
                OnPropertyChanged("EXEC");
            }
        }

        private int _INCOM = 0;
        public int INCOM
        {
            get { return _INCOM; }
            set
            {
                _INCOM = value;
                OnPropertyChanged("INCOM");
            }
        }

        private bool _SummaryAdded = false;
        public bool SummaryAdded
        {
            get { return _SummaryAdded; }
            set
            {
                _SummaryAdded = value;
                OnPropertyChanged("SummaryAdded");
            }
        }

        private int _EXID = 0;
        public int EXID
        {
            get { return _EXID; }
            set
            {
                _EXID = value;
                OnPropertyChanged("EXID");
            }
        }

        private string _TYPELOOP = string.Empty;
        public string TYPELOOP
        {
            get { return _TYPELOOP; }
            set
            {
                _TYPELOOP = value;
                OnPropertyChanged("TYPELOOP");
            }
        }

        private bool _TCINCOM = false;
        public bool TCINCOM
        {
            get { return _TCINCOM; }
            set
            {
                _TCINCOM = value;
                OnPropertyChanged("TCINCOM");
            }
        }

        private List<string> _TCBackGroundResult = new List<string>();
        public List<string> TCBackGroundResult
        {
            get { return _TCBackGroundResult; }
            set
            {
                _TCBackGroundResult = value;
                OnPropertyChanged("TCBackGroundResult");
            }
        }

       

        private Dictionary<int,string> _RerunTime = new Dictionary<int, string>();
        public Dictionary<int, string> RerunTime
        {
            get { return _RerunTime; }
            set
            {
                _RerunTime = value;
                OnPropertyChanged("RerunTime");
            }
        }

        private int _incompleteTP = 0;
        public int incompleteTPID
        {
            get { return _incompleteTP; }
            set
            {
                _incompleteTP = value;
                OnPropertyChanged("incompleteTPID");
            }
        }

        private string _incompleteStatus = string.Empty;
        public string incompleteStatus
        {
            get { return _incompleteStatus; }
            set
            {
                _incompleteStatus = value;
                OnPropertyChanged("incompleteStatus");
            }
        }

        private bool _CECActionChk = false;
        public bool CECActionChk
        {
            get { return _CECActionChk; }
            set
            {
                _CECActionChk = value;
                OnPropertyChanged("CECActionChk");
            }
        }

        private string itemNameJSON;

        public string ItemNameJSON
        {
            get { return itemNameJSON; }
            set
            {
                itemNameJSON = value;
                OnPropertyChanged("ItemNameJSON");
            }
        }

        private bool uniqueKeyJSON;

        public bool UniqueKeyJSON
        {
            get { return uniqueKeyJSON; }
            set
            {
                uniqueKeyJSON = value;
                OnPropertyChanged("UniqueKeyJSON");
            }
        }

        private Visibility itemTextBoxJsonVisibility = Visibility.Collapsed;

        public Visibility ItemTextBoxJsonVisibility
        {
            get { return itemTextBoxJsonVisibility; }
            set
            {
                itemTextBoxJsonVisibility = value;
                OnPropertyChanged("ItemTextBoxJsonVisibility");
            }
        }

        private TextBox textBoxItemJsonValue = new TextBox();

        public TextBox ItemTextBoxJsonValue
        {
            get { return textBoxItemJsonValue; }
            set { textBoxItemJsonValue = value; OnPropertyChanged("ItemTextBoxJsonValue"); }
        }

        private Visibility plusBtnJsonVisibility = Visibility.Collapsed;

        public Visibility PlusBtnJsonVisibility
        {
            get { return plusBtnJsonVisibility; }
            set
            {
                plusBtnJsonVisibility = value;
                OnPropertyChanged("plusBtnJsonVisibility");
            }
        }

        private Visibility minusBtnJsonVisibility = Visibility.Collapsed;

        public Visibility MinusBtnJsonVisibility
        {
            get { return minusBtnJsonVisibility; }
            set
            {
                minusBtnJsonVisibility = value;
                OnPropertyChanged("MinusBtnJsonVisibility");
            }
        }

        private string jsonUppervalue = string.Empty;

        public string JsonUppervalue
        {
            get { return jsonUppervalue; }
            set { jsonUppervalue = value; OnPropertyChanged("JsonUppervalue"); }
        }

        private Visibility itemTextBoxJsonUpperVisibility = Visibility.Collapsed;

        public Visibility ItemTextBoxJsonUpperLowerVisibility
        {
            get { return itemTextBoxJsonUpperVisibility; }
            set
            {
                itemTextBoxJsonUpperVisibility = value;
                OnPropertyChanged("ItemTextBoxJsonUpperLowerVisibility");
            }
        }

        private string jsonLowervalue = string.Empty;

        public string JsonLowervalue
        {
            get { return jsonLowervalue; }
            set { jsonLowervalue = value; OnPropertyChanged("JsonLowervalue"); }
        }

        private void ItemTextBoxJSON_TextChanged(object sender, TextChangedEventArgs e)
        {
            ItemNameJSON = ItemTextBoxJsonValue.Text;
        }
          

        public TreeViewExplorer(string itemType, string itemName, string itemNameJSON, Visibility itemJsonVisibility, Visibility plusbtnVisibility, Visibility minbtnVisibility, Visibility upperlowerlimitVisible, string upperLimit, string lowerLimit, bool uniqueKey)
        {
            try
            {
                ItemType = itemType;
                ItemName = itemName;
                ItemNameJSON = itemNameJSON;               
                ItemTextBoxJsonVisibility = itemJsonVisibility;
                PlusBtnJsonVisibility = plusbtnVisibility;
                MinusBtnJsonVisibility = minbtnVisibility;
                ItemTextBoxJsonUpperLowerVisibility = upperlowerlimitVisible;


                UniqueKeyJSON = uniqueKey;
                JsonUppervalue = upperLimit;
                JsonLowervalue = lowerLimit;

                Initialize(this);
                SetParent();
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                //MessageBox.Show("Exception\n " + ex.Message, "Error Code - EC18005", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
