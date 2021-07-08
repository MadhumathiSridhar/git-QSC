using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace QSC_Test_Automation
{
    /// <summary>
    /// Interaction logic for QRCM_Treeview.xaml
    /// </summary>
    public partial class QRCM_Treeview : Window
    {
        public string treeviewJSONDataToReturn = string.Empty;       
        public Dictionary<string, string[]> keysdict = new Dictionary<string, string[]>();
        public bool errorDisplayed = false;

        /// <summary>
        /// QRCM_Treeview : This method is used to create QREM/QRCM treeview window
        /// </summary>
        /// <param name="jsonString">Payload/reference data to be shown in a treeview</param>
        /// <param name="methodName">This name is displayed as a treeview headeritem</param>
        /// <param name="qatkey">This data will be only available if the jsonString has arraytype data. For arrayType QAT needs unique key to differenciate each arrays.
        /// QAT checkedIn a unique key for each array Using this variable "qatkey" </param>
        /// <param name="isAction">This variable use to differenciate action or verification</param>
        public QRCM_Treeview(string jsonString, string methodName,string qatkey, bool isAction)
        {
            try
            {
                InitializeComponent();
                             
                TreeViewExplorer tableRow = new TreeViewExplorer(null, methodName, string.Empty, Visibility.Collapsed, Visibility.Collapsed, Visibility.Collapsed, Visibility.Collapsed, null, null,false);
             
                if (qatkey != null && qatkey != string.Empty)
                {
                    string[] qat_keys = qatkey.Split('\n');

                    foreach (string item in qat_keys)
                    {
                        string[] array = item.Trim().Split(new char[] { ',', '=' });
                        keysdict.Add(array[array.Count() - 2], array);
                    }
                }
              
                if (isAction)
                    DynamicPairedTextblock.Visibility = Visibility.Collapsed;
                else
                    DynamicPairedTextblock.Visibility = Visibility.Visible;

                ConvertJsonToTree(tableRow, jsonString, isAction);
                treeViewJson.DataContext = null;
                treeViewJson.DataContext = new ObservableCollection<TreeViewExplorer> { tableRow };
            }
            catch(Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
            }
        }

        private void QRCMSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ObservableCollection<TreeViewExplorer> treeviewJSONData = treeViewJson.DataContext as ObservableCollection<TreeViewExplorer>;           

                ///////// Converting Treeview to json
                string treeviewData  = convertTreeviewToJSON(treeviewJSONData[0]);
                                
                if (treeviewData.Trim() != string.Empty)
                {
                    string jsonWithoutStatusCode = treeviewData.Trim().Replace("QATStatusCode-TreeChecked", "");
                    string[] array = jsonWithoutStatusCode.Split(new string[] { "-TreeChecked" }, StringSplitOptions.RemoveEmptyEntries);
                    if (array.Count() < 2)
                        MessageBox.Show("Please select atleast one item from treeview to continue save.", "QAT Error Message", MessageBoxButton.OK, MessageBoxImage.Error);
                    else
                    {
                        treeviewJSONDataToReturn = treeviewData;
                        this.Close();
                    }                
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

        public void ConvertJsonToTree(TreeViewExplorer treeView, string json, bool isAction)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(json))
                {
                    return;
                }
                
                Dictionary<string, object> values = new Dictionary<string, object>();
                dynamic array = new System.Web.Script.Serialization.JavaScriptSerializer().DeserializeObject(json);
                foreach (var item in array)
                {
                    //////Add statuscode as key value pair (status code has value without key: ex[200,data:{}])
                    if (item.GetType() == typeof(int))
                    {
                        values.Add("QATStatusCode", item);
                    }
                    else
                    {
                        object[] arraylist = item as object[];
                        Dictionary<string, object> objectlist = item as Dictionary<string, object>;

                        ///// if the response has no data key at the beginning wrap all the values as data key and values pair 
                        if (arraylist != null)
                        {
                            //////if response has no key and it is an array type, add data key(ex: [200,[{"name":xx,"id":yy}, {"name":zz,"id":ywy}]])
                            values.Add("data", arraylist);
                        }
                        else if (objectlist != null)
                        {
                            if (objectlist.ContainsKey("data"))
                            {
                                //////if response has data key then add data key(ex: [200,"data":{"name":xx,"id":yy}])
                                values.Add("data", objectlist["data"]);
                            }
                            else
                            {
                                //////if response has no key and it is an objetc type, add data key(ex: [200,{"name":xx,"id":yy}])
                                values.Add("data", objectlist);
                            }
                        }
                        else
                        {
                            ///// if the item is key value pair add it to dictionary 
                            ///// Specifically add below keypair to avoid any other key pair added (avoid meta key pair)
                            KeyValuePair<string, object> keyPair = item;
                            if (keyPair.Key == "QATStatusCode-TreeChecked" || keyPair.Key == "data-TreeChecked" || keyPair.Key == "QATStatusCode" || keyPair.Key == "data")
                                values.Add(keyPair.Key, keyPair.Value);
                        }
                    }
                }

                errorDisplayed = false;

                string[] checkedLimitValue = new string[3];
                foreach (KeyValuePair<string, object> property in values)
                {
                    if (errorDisplayed == true)
                        break;

                    AddTokenNodesJsonToTreeview(property, property.Key, treeView, Visibility.Collapsed, ref checkedLimitValue, isAction);
                }

                treeView.IsExpanded = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error occurred while creating treeview", "QAT Error Message", MessageBoxButton.OK, MessageBoxImage.Error);
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
            }
        }



        private void AddTokenNodesJsonToTreeview(KeyValuePair<string, object> keypair, string name, TreeViewExplorer parent, Visibility minusBtnvisible, ref string[] checkedLimitValue, bool isAction)
        {
            try
            {
                object[] arraylist = keypair.Value as object[];
                Dictionary<string, object> objectlist = keypair.Value as Dictionary<string, object>;

                ///////If value is an Array
                if (arraylist != null)
                {
                    AddArrayNodesJsonToTreeview(arraylist, name, parent, ref checkedLimitValue, isAction);
                }
                ///////If value is an object
                else if (objectlist != null)
                {
                    AddObjectNodesJsonToTreeview(objectlist, name, parent, minusBtnvisible, ref checkedLimitValue, isAction);
                }
                else
                {
                    if (name.EndsWith("-TreeChecked"))
                    {
                        checkedLimitValue[0] = keypair.Value.ToString();
                    }
                    else if (name.EndsWith("-UpLimit"))
                    {
                        checkedLimitValue[1] = keypair.Value.ToString();
                    }
                    else if (name.EndsWith("-LowLimit"))
                    {
                        checkedLimitValue[2] = keypair.Value.ToString();
                    }
                    else
                    {
                        bool isChecked = false;
                        string upperlimit = string.Empty;
                        string lowerlimit = string.Empty;
                        if (!string.IsNullOrEmpty(checkedLimitValue[0]))
                        {
                            isChecked = true;
                        }

                        if (!string.IsNullOrEmpty(checkedLimitValue[1]))
                        {
                            upperlimit = checkedLimitValue[1];
                        }

                        if (!string.IsNullOrEmpty(checkedLimitValue[2]))
                        {
                            lowerlimit = checkedLimitValue[2];
                        }

                        Visibility limitvisibility = Visibility.Collapsed;
                        ///// default datatype is string  
                        string datatype = "System.String";
                        if (keypair.Value != null)
                            datatype = keypair.Value.GetType().ToString();

                        ////if type is integer or double and method from QRCM verification, enable limit textboxes
                        if ((datatype == "System.Int32" || datatype == "System.Int64" || datatype == "System.Int16" || datatype == "System.Double" || datatype == "System.Decimal") && (keypair.Key != "QATStatusCode") && (isAction == false))
                        {
                            limitvisibility = Visibility.Visible;
                        }

                        string val = string.Empty;
                        if (keypair.Value != null)
                        {
                            val = keypair.Value.ToString();
                        }

                        bool uniqueKey = false;
                        if (keysdict.Count > 0 && parent.Parent != null && keysdict.Keys.Contains(parent.Parent.ItemName))
                        {
                            string[] keySplitValues = keysdict[parent.Parent.ItemName];

                            if (keySplitValues.Last().ToUpper().ToString() == name.ToUpper())
                                uniqueKey = true;
                        }

                        TreeViewExplorer tableRow = new TreeViewExplorer(datatype, name, val, Visibility.Visible, Visibility.Collapsed, minusBtnvisible, limitvisibility, upperlimit, lowerlimit, uniqueKey);

                        /////if array type is a string array without key value pair, need to hide checkbox(ex:data:["xxx","yyy","zzz"])
                        if (name == string.Empty)
                        {
                            tableRow.IsCheckedVisibility = Visibility.Collapsed;
                        }

                        parent.AddChildren(tableRow);

                        if (isChecked)
                            tableRow.IsChecked = false;
                        else
                            tableRow.IsChecked = true;

                        tableRow.IsChecked = isChecked;

                        /////// By default QATStatusCode is always cheked in treeview
                        if (tableRow.ItemName == "QATStatusCode")
                        {
                            //tableRow.IsChecked = true;
                            tableRow.IsCheckedVisibility = Visibility.Collapsed;
                        }

                        checkedLimitValue = new string[3];
                    }
                }
            }
            catch (Exception ex)
            {
                errorDisplayed = true;
                MessageBox.Show("Error occurred while creating treeview", "QAT Error Message", MessageBoxButton.OK, MessageBoxImage.Error);
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
            }
        }

        private void AddArrayNodesJsonToTreeview(object[] arraylist, string name, TreeViewExplorer parent, ref string[] checkedLimitValue, bool isAction)
        {
            try
            {
                string itemIsChecked = checkedLimitValue[0];
                checkedLimitValue = new string[3];
                TreeViewExplorer tableRow = new TreeViewExplorer("Array", name, string.Empty, Visibility.Collapsed, Visibility.Visible, Visibility.Collapsed, Visibility.Collapsed, null, null, false);
                parent.AddChildren(tableRow);
                tableRow.IsChecked = false;

                string arraytype = string.Empty;
                for (var i = 0; i < arraylist.Count(); i++)
                {
                    if (errorDisplayed == true)
                        break; 

                    ///if array type is a string array without key value pair, need to hide checkbox(ex:data:["xxx","yyy","zzz"])
                    arraytype = arraylist[i].GetType().ToString();
                    if (arraytype == "System.String")
                    {
                        ///// if parent checked=true, need to check all children                  
                        checkedLimitValue[0] = itemIsChecked;
                        AddTokenNodesJsonToTreeview(new KeyValuePair<string, object>(name, arraylist[i]), string.Empty, tableRow, Visibility.Visible, ref checkedLimitValue, isAction);
                    }

                    else
                        AddTokenNodesJsonToTreeview(new KeyValuePair<string, object>(name, arraylist[i]), $"[{i}]", tableRow, Visibility.Visible, ref checkedLimitValue, isAction);
                }

                ///It applies checked true while edit response(payload/reference). if array type is a string array and checked in true, then parent checked=true(ex:data:["xxx","yyy","zzz"])
                //if (arraytype == "System.String" && itemIsChecked != null && itemIsChecked != string.Empty)
                //    tableRow.IsChecked = true;
            }
            catch (Exception ex)
            {
                errorDisplayed = true;
                MessageBox.Show("Error occurred while creating treeview", "QAT Error Message", MessageBoxButton.OK, MessageBoxImage.Error);
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
            }
        }

        public void AddObjectNodesJsonToTreeview(Dictionary<string, object> obj, string name, TreeViewExplorer parent, Visibility minusBtnvisible, ref string[] checkedLimitValue, bool isAction)
        {
            try
            {
                checkedLimitValue = new string[3];
                TreeViewExplorer tableRow = new TreeViewExplorer("Object", name, string.Empty, Visibility.Collapsed, Visibility.Collapsed, minusBtnvisible, Visibility.Collapsed, null, null, false);
                parent.AddChildren(tableRow);

                foreach (KeyValuePair<string, object> property in obj)
                {
                    if (errorDisplayed == true)
                        break;

                    AddTokenNodesJsonToTreeview(property, property.Key, tableRow, Visibility.Collapsed, ref checkedLimitValue, isAction);
                }
            }
            catch (Exception ex)
            {
                errorDisplayed = true;
                MessageBox.Show("Error occurred while creating treeview", "QAT Error Message", MessageBoxButton.OK, MessageBoxImage.Error);
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
            }
        }


        public string convertTreeviewToJSON(TreeViewExplorer parentNode)
        {
            string json = string.Empty;
            try
            {
                Dictionary<string, object> JTreeDict = new Dictionary<string, object>();

                ///// treeview reconstruction. Adding treeview checked, upper limit and lower limit
                errorDisplayed = false;
                foreach (TreeViewExplorer childNode in parentNode.Children)
                {
                    if (errorDisplayed == true)
                        break;
                    AddRecursiveTreeview(childNode, ref JTreeDict);
                }

                if (errorDisplayed)
                    return string.Empty;

                ///////converting treeview to string json
                var jsSerializer = new System.Web.Script.Serialization.JavaScriptSerializer();
                json = jsSerializer.Serialize(JTreeDict);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error occured while saving treeview values", "QAT Error Message", MessageBoxButton.OK, MessageBoxImage.Error);
                DeviceDiscovery.WriteToLogFile("Exception while convert treeViewExplorer to json. Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
            }
            return json;
        }



        private void AddRecursiveTreeview(TreeViewExplorer parent, ref Dictionary<string, object> jobj)
        {
            try
            {
                if (!errorDisplayed)
                {
                    if (parent.ItemType == "Array")
                    {
                        AddArrayNodesTreeviewToJSON(parent, jobj);
                    }
                    else if (parent.ItemType == "Object")
                    {
                        AddObjectNodesTreeviewToJSON(parent, jobj);
                    }
                    else
                    {
                        //////Verifying unique key(mandatory key in an array) is empty or not
                        if (parent.IsChecked == true && parent.UniqueKeyJSON == true && (parent.ItemNameJSON == null || parent.ItemNameJSON.Trim() == string.Empty))
                        {
                            MessageBox.Show("Please enter " + parent.ItemName + " value to save", "QAT Error Message", MessageBoxButton.OK, MessageBoxImage.Error);
                            errorDisplayed = true;
                            return;
                        }

                        ////// Adding Treechecked, upperlimit, lowerlimit keys to property
                        if (parent.IsChecked == true)
                            jobj.Add(parent.ItemName.TrimEnd(':') + "-TreeChecked", "true");
                        if (!string.IsNullOrEmpty(parent.JsonUppervalue))
                        {
                            object upperValue = parent.JsonUppervalue;    
                            DatatypeConversion(parent, "upper limit", ref upperValue);
                            jobj.Add(parent.ItemName.TrimEnd(':') + "-UpLimit", upperValue);

                            if (errorDisplayed == true)
                                return;
                        }
                        if (!string.IsNullOrEmpty(parent.JsonLowervalue))
                        {
                            object lowerValue = parent.JsonLowervalue;
                            DatatypeConversion(parent, "lower limit", ref lowerValue);
                            jobj.Add(parent.ItemName.TrimEnd(':') + "-LowLimit", lowerValue);

                        }

                        if (parent.ItemNameJSON == string.Empty || parent.ItemNameJSON == null)
                            SetDefaultValues(parent);

                        ////// Adding property              
                        object ItemNameJSONVal = parent.ItemNameJSON;
                        //////properety value type error check
                        DatatypeConversion(parent,string.Empty, ref ItemNameJSONVal);
                        jobj.Add(parent.ItemName, ItemNameJSONVal);

                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error occured while saving treeview values", "QAT Error Message", MessageBoxButton.OK, MessageBoxImage.Error);
                errorDisplayed = true;
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
            }
        }

        private void DatatypeConversion(TreeViewExplorer parent,string upperLowerLimit, ref object value)
        {
            string expectedDataType = string.Empty;
            try
            {
                if (upperLowerLimit != string.Empty)
                {
                    expectedDataType = "Numbers";

                    if(upperLowerLimit == "upper limit")
                      value = Convert.ToDouble(parent.JsonUppervalue);
                    if(upperLowerLimit == "lower limit")
                        value = Convert.ToDouble(parent.JsonLowervalue);
                }
                else 
                {
                    if (parent.ItemType == "System.Int32" || parent.ItemType == "System.Int16" || parent.ItemType == "System.Int64")
                    {
                        expectedDataType = "Numbers";
                        value = Convert.ToInt64(parent.ItemNameJSON);
                    }
                    else if (parent.ItemType == "System.Double")
                    {
                        expectedDataType = "Numbers";
                        value = Convert.ToDouble(parent.ItemNameJSON);
                    }
                    else if (parent.ItemType == "System.Decimal")
                    {
                        expectedDataType = "Numbers";
                        value = Convert.ToDecimal(parent.ItemNameJSON);
                    }
                    else if (parent.ItemType == "System.Boolean")
                    {
                        expectedDataType = "True/False";
                        value = Convert.ToBoolean(parent.ItemNameJSON);
                    }
                    else if (parent.ItemType == "System.String")
                    {
                        expectedDataType = "Text";
                        value = Convert.ToString(parent.ItemNameJSON);
                    }
                    else
                    {
                        value = parent.ItemNameJSON;
                    }
                }
            }
            catch (Exception ex)
            {
                errorDisplayed = true;
                MessageBox.Show("Value was not in correct format.\nPlease enter valid data for " + parent.ItemName + " "+ upperLowerLimit + "\nValid format: " + expectedDataType, "QAT Error Message", MessageBoxButton.OK, MessageBoxImage.Error);
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
            }
        }

        private void AddArrayNodesTreeviewToJSON(TreeViewExplorer parent, object jobj)
        {
            try
            {
                object[] jarr = new object[parent.Children.Count];

                int i = 0;
                foreach (var children in parent.Children)
                {
                    if (errorDisplayed == true)
                        break;

                    //////If array doesn't have object like c:[1,2,3] (string array type value)
                    if (children.ItemName == string.Empty)
                    {
                        if (children.ItemNameJSON == null)
                            children.ItemNameJSON = string.Empty;
                        
                        jarr[i] = children.ItemNameJSON;
                    }
                    /////If array has an object
                    else
                    {
                        if (errorDisplayed == true)
                            break;

                        Dictionary<string, object> obj = new Dictionary<string, object>();
                        object objForArray = obj as object;
                        AddRecursiveTreeview(children, ref obj);
                        jarr[i] = objForArray;
                    }

                    i++;
                }

                Dictionary<string, object> array = jobj as Dictionary<string, object>;
                if ((parent.IsChecked == null || parent.IsChecked == true) && (jarr.Count()>0))
                    array.Add(parent.ItemName + "-TreeChecked", true);
                array.Add(parent.ItemName, jarr);
            }
            catch (Exception ex)
            {
                errorDisplayed = true;
                MessageBox.Show("Error occured while saving treeview values", "QAT Error Message", MessageBoxButton.OK, MessageBoxImage.Error);
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
            }

        }

        

        public void AddObjectNodesTreeviewToJSON(TreeViewExplorer parent, object jobj)
        {
            try
            {
                ///////If object within array means header removed [0],[1]
                if (parent.Parent != null && parent.Parent.ItemType == "Array")
                {
                    foreach (var children in parent.Children)
                    {
                        if (errorDisplayed == true)
                            break;
                        Dictionary<string, object> obj = jobj as Dictionary<string, object>;
                        AddRecursiveTreeview(children, ref obj);
                    }
                }
                /////If only objects means
                else
                {
                    Dictionary<string, object> objNode = new Dictionary<string, object>();

                    foreach (var children in parent.Children)
                    {
                        if (errorDisplayed == true)
                            break;
                        AddRecursiveTreeview(children, ref objNode);
                    }

                    Dictionary<string, object> obj = jobj as Dictionary<string, object>;
                    if (parent.IsChecked == null || parent.IsChecked == true)
                        obj.Add(parent.ItemName + "-TreeChecked", true);

                    obj.Add(parent.ItemName, objNode);
                }
            }
            catch (Exception ex)
            {
                errorDisplayed = true;
                MessageBox.Show("Error occured while saving treeview values", "QAT Error Message", MessageBoxButton.OK, MessageBoxImage.Error);
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
            }
        }


        private void JSONlimitTxtbox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            try
            {
                if (e.Text == "." || e.Text=="-")
                {
                    TextBox textinput = (TextBox)sender;
                    string text = textinput.Text;
                    if (text.Contains(e.Text))
                        e.Handled = true;                    
                }
                else
                {
                    Regex regex = new Regex("[^0-9]");
                    e.Handled = regex.IsMatch(e.Text);
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

        private void JSONlimitTxtbox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.V && Keyboard.Modifiers == ModifierKeys.Control)
                {
                    e.Handled = false;

                    Regex regex = new Regex("[^0-9-.]");
                    e.Handled = regex.IsMatch(Clipboard.GetText());

                    base.OnPreviewKeyDown(e);
                }
                if (e.Key == Key.Space)
                {
                    e.Handled = true;
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

        private void AddArray_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                FrameworkElement sourceElement = e.OriginalSource as FrameworkElement;
                if (sourceElement == null)
                    return;

                TreeViewExplorer sourceTreeview = sourceElement.DataContext as TreeViewExplorer;

                if (sourceTreeview == null)
                    return;

                if (sourceTreeview.Children.Count > 0)
                {
                    TreeViewExplorer newItem = new TreeViewExplorer(sourceTreeview.Children[0]);
                    var lastIndex = Regex.Split(sourceTreeview.Children.Last().ItemName, @"\[(.*?)\]");

                    if (newItem.ItemType == "Object")
                        newItem.ItemName = "[" + (Convert.ToInt32(lastIndex[1]) + 1) + "]";

                    ///if array type is a string array without key value pair ItemName becomes empty.(ex:data:["xxx","yyy","zzz"])
                    /// If itemname is empty need to hide checkbox   /////clear value textbox
                    if (newItem.ItemName==null || newItem.ItemName==string.Empty)
                    {
                        newItem.IsCheckedVisibility = Visibility.Collapsed;
                        newItem.ItemNameJSON = string.Empty;
                    }

                    ////Setting default values in value textbox based on itemtype
                    newItem = new TreeViewExplorer(newItem, newItem.ItemName);
                 
                    ////Adding new item to treeview
                    sourceTreeview.AddChildren(newItem);
                    var dataContext = treeViewJson.DataContext;
                    treeViewJson.DataContext = null;
                    treeViewJson.DataContext = dataContext;
                }                
            }
            catch(Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
            }
          
        }


        public void SetDefaultValues(TreeViewExplorer sourceTreeviewExplorer)
        {
            try
            {
                if (sourceTreeviewExplorer == null)
                    return;

                if (sourceTreeviewExplorer.ItemType == "System.Int32" || sourceTreeviewExplorer.ItemType == "System.Int16" || sourceTreeviewExplorer.ItemType == "System.Int64"||
                     sourceTreeviewExplorer.ItemType == "System.Double" || sourceTreeviewExplorer.ItemType == "System.Decimal")
                {
                        sourceTreeviewExplorer.ItemNameJSON = "0";             
                }
                else if(sourceTreeviewExplorer.ItemType == "System.Boolean")
                {
                        sourceTreeviewExplorer.ItemNameJSON = "false";                                
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


        private void DeleteArray_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                FrameworkElement sourceElement = e.OriginalSource as FrameworkElement;
                if (sourceElement == null)
                    return;

                TreeViewExplorer selectedTreeview = sourceElement.DataContext as TreeViewExplorer;

                if (selectedTreeview == null)
                    return;

                /////remove selected item from treeview
                selectedTreeview.Parent.RemoveChildren(selectedTreeview);
                var dataContext = treeViewJson.DataContext;
                treeViewJson.DataContext = null;
                treeViewJson.DataContext = dataContext;
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
            }
        }
        

        private void treeview_CheckBox_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                FrameworkElement sourceElement = e.OriginalSource as FrameworkElement;
                if (sourceElement == null)
                    return;

                TreeViewExplorer selectedTreeview = sourceElement.DataContext as TreeViewExplorer;

                if (selectedTreeview == null)
                    return;

                if (selectedTreeview != null && keysdict != null && keysdict.Count > 0 && selectedTreeview.IsChecked == true)
                {
                    if (selectedTreeview.Parent != null && selectedTreeview.Parent.Parent != null)
                    {
                        //////if parent object find mandatory key to checkIn
                        if (selectedTreeview.Parent.Parent.ItemType == "Object")
                        {
                            TreeViewExplorer selectedTreeviewParent = selectedTreeview.Parent.Parent as TreeViewExplorer;
                            TreeViewExplorer itemTocheckIn = selectedTreeviewParent.Children.Find(x => x.UniqueKeyJSON == true);
                            if (itemTocheckIn != null && itemTocheckIn.IsChecked == false)
                            {
                                itemTocheckIn.IsChecked = true;
                                //////Incase this key's parent is an array need to checked in unique key  /////below loop will do the checked in unique keys for multiple array
                                selectedTreeview = itemTocheckIn;
                            }
                        }

                        //////if parent Array find mandatory key to checkIn
                        if (selectedTreeview !=null && selectedTreeview.Parent!=null && selectedTreeview.Parent.Parent!=null && selectedTreeview.Parent.Parent.ItemType == "Array")
                        {
                            if (keysdict.Keys.Contains(selectedTreeview.Parent.Parent.ItemName))
                            {
                                string[] keyAfterSplit = keysdict[selectedTreeview.Parent.Parent.ItemName];
                                //////removing children key and reverse it to checked in loop for example:(interfaces,staticRoutes=ipadress)   keyAfterSplit=[interfaces, staticRoutes,ipadress]  remainingParentKeysToCheckIn = [staticRoutes, interfaces]                            
                                var remainingParentKeysToCheckIn = keyAfterSplit.Except(new string[] { keyAfterSplit.Last() }).Reverse();

                                foreach (string parentKey in remainingParentKeysToCheckIn)
                                {
                                    ////selectedTreeview.Parent is actually array header for example [0],[1],[2]
                                    TreeViewExplorer selectedTreeviewParent = selectedTreeview.Parent as TreeViewExplorer;
                                    if (selectedTreeviewParent.Parent.ItemName == parentKey)
                                    {
                                        TreeViewExplorer childItemTocheckIn = selectedTreeviewParent.Children.Find(x => x.UniqueKeyJSON == true);
                                        if (childItemTocheckIn != null && childItemTocheckIn.IsChecked == false)
                                        {
                                            childItemTocheckIn.IsChecked = true;
                                        }
                                    }
                                    ////selectedTreeviewParent.Parent is actually parent of array header for example staticRoutes (ex: interfaces,staticRoutes=ipadress)
                                    //////if key has multiple parent array (ex: interfaces,staticRoutes=ipadress) we have to assign selected treeview =staticRoutes to check in interfaces unique key
                                    selectedTreeview = selectedTreeviewParent.Parent;
                                }

                            }
                        }

                    }
                }
                else if (selectedTreeview != null && selectedTreeview.IsChecked == false)
                {
                    ////// if selectedTreeview is mandatory key verify if any other children is in checked state
                    if (selectedTreeview.UniqueKeyJSON == true && selectedTreeview.Parent != null)
                    {                     
                        TreeViewExplorer isAnyTreeviewChildrenCheckedIn = selectedTreeview.Parent.Children.Find(x => (x.IsChecked == true || x.IsChecked == null));
                        if (isAnyTreeviewChildrenCheckedIn != null)
                        {
                            selectedTreeview.IsChecked = true;
                            MessageBox.Show(selectedTreeview.ItemName + " is mandatory key", "QAT Error Message", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }

                    /////for overall selection uncheck maintain QATStatusCode checked in state
                    //TreeViewExplorer childItemTocheckIn = selectedTreeview.Children.Find(x => x.ItemName == "QATStatusCode");
                    //if (childItemTocheckIn != null)
                    //    childItemTocheckIn.IsChecked = true;
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
      
    }    
   
}
