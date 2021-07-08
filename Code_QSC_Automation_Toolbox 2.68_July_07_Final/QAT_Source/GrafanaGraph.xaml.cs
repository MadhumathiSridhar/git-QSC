using System;
using System.Windows;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SqlClient;
using System.Net;
using System.Web.Script.Serialization;
using System.IO;
using System.Diagnostics;
//using System.Collections;

namespace QSC_Test_Automation
{
    /// <summary>
    /// Interaction logic for GrafanaGraph.xaml
    /// </summary>
    public partial class GrafanaGraph : Window, INotifyPropertyChanged
    {
        GrafanaDBConnection grafana_connection = new GrafanaDBConnection();
        System.Drawing.Color[] colorsList = new System.Drawing.Color[] { System.Drawing.Color.Blue, System.Drawing.Color.Yellow, System.Drawing.Color.Green, System.Drawing.Color.Pink, System.Drawing.Color.Orange, System.Drawing.Color.Brown, System.Drawing.Color.Violet, System.Drawing.Color.GreenYellow, System.Drawing.Color.Lavender, System.Drawing.Color.Olive };
        int colorsCnt = 0;

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
            }
        }



        public GrafanaGraph()
        {
            try
            {
                this.InitializeComponent();          
                ReadData();              
                this.DataContext = this;
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif               
            }
        }

        private ObservableCollection<GrafanaTagSettings> tagItemListValue = new ObservableCollection<GrafanaTagSettings>();
        public ObservableCollection<GrafanaTagSettings> TagItemList
        {
            get { return tagItemListValue; }
            set { tagItemListValue = value; OnPropertyChanged("TagItemList"); }
        }

        private ObservableCollection<GrafanaGUI> grafanaGUI_ItemsListValue = new ObservableCollection<GrafanaGUI>();
        public ObservableCollection<GrafanaGUI> GrafanaGUI_ItemsList
        {
            get { return grafanaGUI_ItemsListValue; }
            set { grafanaGUI_ItemsListValue = value; OnPropertyChanged("GrafanaGUI_ItemsList"); }
        }
        
        private DataTable dbValue = new DataTable();
        public DataTable datafromDB
        {
            get { return dbValue; }
            set { dbValue = value; OnPropertyChanged("datafromDB"); }
        }

        private bool ReadData()
        {
            string query = string.Empty;
            GrafanaTagSettings tagitem = new GrafanaTagSettings();
            GrafanaGUI grafanaguiItem = new GrafanaGUI();
            try
            {

                grafanaguiItem.ScriptNamelist.Add("None");
                grafanaguiItem.ReleaseVersionlist.Add("None");
                grafanaguiItem.DesignNamelist.Add("None");

                query = "Select * from DataPointsMappingTable";
                DataTable dataTable = new DataTable();
                SqlDataAdapter dataAdapter = new SqlDataAdapter(query, grafana_connection.CreateConnection());
                grafana_connection.OpenConnection();
                dataAdapter.Fill(dataTable);
                datafromDB = dataTable;
                DataTableReader dataTableReader = dataTable.CreateDataReader();
                while (dataTableReader.Read())
                { 
                    if (dataTableReader[1] != System.DBNull.Value && dataTableReader[1].ToString() != string.Empty && (!tagitem.ExecutionIDlist.Contains(dataTableReader.GetInt32(1).ToString())))
                        tagitem.ExecutionIDlist.Add(dataTableReader[1].ToString());

                    if (dataTableReader[6] != System.DBNull.Value && dataTableReader[6].ToString() != string.Empty && (!grafanaguiItem.ScriptNamelist.Contains(dataTableReader.GetString(6))))
                        grafanaguiItem.ScriptNamelist.Add(dataTableReader.GetString(6));

                    if (dataTableReader[8] != System.DBNull.Value && dataTableReader[8].ToString() != string.Empty && (!grafanaguiItem.ReleaseVersionlist.Contains(dataTableReader.GetString(8))))
                        grafanaguiItem.ReleaseVersionlist.Add(dataTableReader.GetString(8));

                    if (dataTableReader[9] != System.DBNull.Value && dataTableReader[9].ToString() != string.Empty && (!grafanaguiItem.DesignNamelist.Contains(dataTableReader.GetString(9))))
                        grafanaguiItem.DesignNamelist.Add(dataTableReader.GetString(9));

                    if (dataTableReader[10] != System.DBNull.Value && dataTableReader[10].ToString()!= string.Empty && (!grafanaguiItem.TagNameList.Contains(dataTableReader.GetString(10))))
                        grafanaguiItem.TagNameList.Add(dataTableReader.GetString(10));
                }
                
                tagitem.ParentTestActionItem = this;
                grafanaguiItem.ParentTestActionItem = this;
                
                //////sort tag name list in grafana GUI
                if (grafanaguiItem.TagNameList != null && grafanaguiItem.TagNameList.Count > 1)                
                    grafanaguiItem.TagNameList = new ObservableCollection<string>(grafanaguiItem.TagNameList.OrderBy(x => x));
                

                TagItemList.Add(tagitem);
                GrafanaGUI_ItemsList.Add(grafanaguiItem);

                if (tagitem.ExecutionIDlist.Count > 0)
                    tagitem.ExecutionIDlistSelectedItem = tagitem.ExecutionIDlist.Last();

                grafana_connection.CloseConnection();                       
                
                return true;
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                return false;
            }
        }

        public bool Get_Selected_ExecId_Details(string execid)
        {
            try
            {
               var rowCollections = datafromDB.AsEnumerable().Where(dr => dr.Field<int>("ExecID") == Convert.ToInt32(execid));

                foreach(DataRow row in rowCollections)
                {
                    if(!string.IsNullOrEmpty(row[2].ToString()) && (!TagItemList[0].TestSuiteNamelist.Contains(row[2].ToString())))
                    TagItemList[0].TestSuiteNamelist.Add(row[2].ToString());
                }

                return true;
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                return false;
            }
        }


        public bool Get_Selected_Testsuite_Details(string execid, string testSuiteName)
        {
            try
            {
                var rowCollections = datafromDB.AsEnumerable().Where(dr => dr.Field<int>("ExecID") == Convert.ToInt32(execid) && dr.Field<string>("Testsuitename") == testSuiteName);

                foreach (DataRow row in rowCollections)
                {
                    if (!string.IsNullOrEmpty(row[3].ToString()) && (!TagItemList[0].TestplanNamelist.Contains(row[3].ToString())))
                        TagItemList[0].TestplanNamelist.Add(row[3].ToString());
                }

                return true;
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                return false;
            }
        }

        public bool Get_Selected_Testplan_Details(string execid, string testSuiteName, string testplanName)
        {
            try
            {

                var rowCollections = datafromDB.AsEnumerable().Where(dr => dr.Field<int>("ExecID") == Convert.ToInt32(execid) && dr.Field<string>("Testsuitename") == testSuiteName && dr.Field<string>("Testplanname") == testplanName);

                foreach (DataRow row in rowCollections)
                {
                    if (!string.IsNullOrEmpty(row[4].ToString()) && (!TagItemList[0].TestCaseNamelist.Contains(row[4].ToString())))
                        TagItemList[0].TestCaseNamelist.Add(row[4].ToString());
                }

                return true;
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                return false;
            }
        }


        public bool Get_Selected_Testcase_Details(string execid, string testSuiteName, string testplanName, string testcaseName)
        {
            try
            {

                var rowCollections = datafromDB.AsEnumerable().Where(dr => dr.Field<int>("ExecID") == Convert.ToInt32(execid) && dr.Field<string>("Testsuitename") == testSuiteName && dr.Field<string>("Testplanname") == testplanName && dr.Field<string>("Testcasename") == testcaseName);

                foreach (DataRow row in rowCollections)
                {
                    if (!string.IsNullOrEmpty(row[5].ToString()) && (!TagItemList[0].TestActionNamelist.Contains(row[5].ToString())))
                        TagItemList[0].TestActionNamelist.Add(row[5].ToString());
                }

                return true;
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                return false;
            }
        }

        public bool Get_Selected_TestAction_Details(string execid, string testSuiteName, string testplanName, string testcaseName, string testActionName)
        {
            try
            {

                var rowCollections = datafromDB.AsEnumerable().Where(dr => dr.Field<int>("ExecID") == Convert.ToInt32(execid) && dr.Field<string>("Testsuitename") == testSuiteName && dr.Field<string>("Testplanname") == testplanName 
                                     && dr.Field<string>("Testcasename") == testcaseName && dr.Field<string>("Testactionname") == testActionName);

                foreach (DataRow row in rowCollections)
                {
                    if (!string.IsNullOrEmpty(row[6].ToString()) && (!TagItemList[0].ScriptTypelist.Contains(row[6].ToString())))
                        TagItemList[0].ScriptTypelist.Add(row[6].ToString());
                }

                return true;
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                return false;
            }
        }


        public bool Get_Selected_scriptType_Details(string execid, string testSuiteName, string testplanName, string testcaseName, string testActionName, string scriptType)
        {
            try
            {

                var rowCollections = datafromDB.AsEnumerable().Where(dr => dr.Field<int>("ExecID") == Convert.ToInt32(execid) && dr.Field<string>("Testsuitename") == testSuiteName && dr.Field<string>("Testplanname") == testplanName
                                     && dr.Field<string>("Testcasename") == testcaseName && dr.Field<string>("Testactionname") == testActionName && dr.Field<string>("ScriptType") == scriptType);

                foreach (DataRow row in rowCollections)
                {
                    if (!string.IsNullOrEmpty(row[7].ToString()) && (!TagItemList[0].TabStartTimelist.Contains(row[7].ToString())))
                        TagItemList[0].TabStartTimelist.Add(row[7].ToString());
                }

                if (TagItemList[0].TabStartTimelist.Count > 0)
                    TagItemList[0].TabStartTimelistSelectedItem = TagItemList[0].TabStartTimelist.First();

                return true;
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                return false;
            }
        }


        public bool Get_Selected_TabstartTime_Details(string execid, string testSuiteName, string testplanName, string testcaseName, string testActionName, string scriptType, string tabStartTime)
        {
            try
            {
                DataRow row = datafromDB.AsEnumerable().Where(dr => dr.Field<int>("ExecID") == Convert.ToInt32(execid) && dr.Field<string>("Testsuitename") == testSuiteName && dr.Field<string>("Testplanname") == testplanName
                                     && dr.Field<string>("Testcasename") == testcaseName && dr.Field<string>("Testactionname") == testActionName && dr.Field<string>("ScriptType") == scriptType && dr.Field<string>("ScriptStartTime") == tabStartTime).First();

                if (!string.IsNullOrEmpty(row[10].ToString()))
                    TagItemList[0].TagNameText= row[10].ToString();           

                return true;
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                return false;
            }
        }


        public bool Get_Selected_filteredDetails(string scriptName, string releaseversion, string designName)
        {
            try
            {
                GrafanaGUI_ItemsList[0].TagNameList.Clear();
                string filter = string.Empty;
                string query = string.Empty;

                if (scriptName != null && scriptName != string.Empty && scriptName!= "None")
                    filter = "ScriptType ='" + scriptName+"' and";

                if (releaseversion != null && releaseversion != string.Empty && releaseversion != "None")
                    filter += " ReleaseVersion ='" + releaseversion + "' and";

                if (designName != null && designName != string.Empty && designName!= "None")
                    filter += " DesignName ='" + designName + "'";

                if (filter.EndsWith("and"))
                    filter = filter.Remove(filter.Length - 3);

                if(filter!=string.Empty)
                     query = "Select TagName from DataPointsMappingTable where " + filter;
                else
                    query = "Select TagName from DataPointsMappingTable";

                DataTable dataTable = new DataTable();
                SqlDataAdapter dataAdapter = new SqlDataAdapter(query, grafana_connection.CreateConnection());
                grafana_connection.OpenConnection();
                dataAdapter.Fill(dataTable);             
                DataTableReader dataTableReader = dataTable.CreateDataReader();
                while (dataTableReader.Read())
                {
                    if (dataTableReader[0] != System.DBNull.Value && dataTableReader[0].ToString() != string.Empty && (!GrafanaGUI_ItemsList[0].TagNameList.Contains(dataTableReader.GetString(0))))
                        GrafanaGUI_ItemsList[0].TagNameList.Add(dataTableReader.GetString(0));
                }

                grafana_connection.CloseConnection();


                if(GrafanaGUI_ItemsList[0].TagNameList!= null && GrafanaGUI_ItemsList[0].TagNameList.Count>1)
                {
                    GrafanaGUI_ItemsList[0].TagNameList = new ObservableCollection<string>(GrafanaGUI_ItemsList[0].TagNameList.OrderBy(i => i));
                }

                return true;
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                return false;
            }
        }


             

        private void btn_Add_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                QatMessageBox Qmsg = new QatMessageBox(this);

                if(string.IsNullOrEmpty(TagItemList[0].TagNameText))
                {
                    Qmsg.Show("Tag name is empty", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                if (string.IsNullOrEmpty(TagItemList[0].ExecutionIDlistSelectedItem))
                {
                    Qmsg.Show("Please select execution ID", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                if (string.IsNullOrEmpty(TagItemList[0].TestSuiteNamelistSelectedItem))
                {
                    Qmsg.Show("Please select testsuite name", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                if (string.IsNullOrEmpty(TagItemList[0].TestplanNamelistSelectedItem))
                {
                    Qmsg.Show("Please select testplan name", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                if (string.IsNullOrEmpty(TagItemList[0].TestCaseNamelistSelectedItem))
                {
                    Qmsg.Show("Please select testcase name", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                if (string.IsNullOrEmpty(TagItemList[0].TestActionNamelistSelectedItem))
                {
                    Qmsg.Show("Please select testaction name", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                if (string.IsNullOrEmpty(TagItemList[0].ScriptTypeSelectedItem))
                {
                    Qmsg.Show("Please select script type", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                if (string.IsNullOrEmpty(TagItemList[0].TabStartTimelistSelectedItem))
                {
                    Qmsg.Show("Please select action start time", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                string query = "Select TagName from DataPointsMappingTable where TagName != ''";
                DataTable dataTable1 = new DataTable();
                SqlDataAdapter dataAdapter1 = new SqlDataAdapter(query, grafana_connection.CreateConnection());
                grafana_connection.OpenConnection();
                dataAdapter1.Fill(dataTable1);
                DataTableReader dataTableReader = dataTable1.CreateDataReader();
                grafana_connection.CloseConnection();
                List<string> tagNamelst = dataTable1.AsEnumerable().Select(x => x[0].ToString()).ToList();

                if (!tagNamelst.Contains(TagItemList[0].TagNameText.Trim(), StringComparer.CurrentCultureIgnoreCase))
                {

                    ////update in database
                    query = "Update DataPointsMappingTable set TagName = @TagName where ExecID='" + TagItemList[0].ExecutionIDlistSelectedItem + "'and Testsuitename='" + TagItemList[0].TestSuiteNamelistSelectedItem + "'and Testplanname='" + TagItemList[0].TestplanNamelistSelectedItem
                          + "'and Testcasename='" + TagItemList[0].TestCaseNamelistSelectedItem + "'and Testactionname='" + TagItemList[0].TestActionNamelistSelectedItem + "'and ScriptType='" + TagItemList[0].ScriptTypeSelectedItem + "'and ScriptStartTime='" + TagItemList[0].TabStartTimelistSelectedItem + "'";

                    Dictionary<string, string>tagnameParameter = new Dictionary<string, string>();
                    tagnameParameter.Add("@TagName", TagItemList[0].TagNameText.Trim());                  

                    var result = dbread_method(query, tagnameParameter);

                    //DataTable dataTable = new DataTable();
                    //SqlDataAdapter dataAdapter = new SqlDataAdapter(query, grafana_connection.CreateConnection());
                    //grafana_connection.OpenConnection();
                    //dataAdapter.Fill(dataTable);
                    //grafana_connection.CloseConnection();

                    ////update in Grafana GUI (update fitered values)
                    Get_Selected_filteredDetails(GrafanaGUI_ItemsList[0].ScriptNamelistSelectedItem, GrafanaGUI_ItemsList[0].ReleaseVersionlistSelectedItem, GrafanaGUI_ItemsList[0].DesignNamelistSelectedItem);


                    ////update in datatable list (readed while window opening for the first time)
                    DataRow datarow = datafromDB.AsEnumerable().Where(dr => dr.Field<int>("ExecID") == Convert.ToInt32(TagItemList[0].ExecutionIDlistSelectedItem) && dr.Field<string>("Testsuitename") == TagItemList[0].TestSuiteNamelistSelectedItem && dr.Field<string>("Testplanname") == TagItemList[0].TestplanNamelistSelectedItem
                                   && dr.Field<string>("Testcasename") == TagItemList[0].TestCaseNamelistSelectedItem && dr.Field<string>("Testactionname") == TagItemList[0].TestActionNamelistSelectedItem && dr.Field<string>("ScriptType") == TagItemList[0].ScriptTypeSelectedItem && dr.Field<string>("ScriptStartTime") == TagItemList[0].TabStartTimelistSelectedItem).First();

                    if(datarow!=null)
                       datarow[10] = TagItemList[0].TagNameText.Trim();

                   
                    Qmsg.Show("Tag name updated successfully", "Information", MessageBoxButton.OK, MessageBoxImage.Information);

                }
                else
                {
                 
                   Qmsg.Show("Tag name already exist", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);                   
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

        private void btn_viewGrafana_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                QatMessageBox Qmsg = new QatMessageBox(this);
                string Dashboard_Title = GrafanaGUI_ItemsList[0].DashboardTitleText.Trim();
                Dictionary<string, Tuple<List<string>, string, List<string>>> tag_Inputs = new Dictionary<string, Tuple<List<string>, string, List<string>>>();
                if (!string.IsNullOrEmpty(Dashboard_Title))
                {
                    if (!string.IsNullOrEmpty(Properties.Settings.Default.BrowserPath.ToString()))
                    {
                        if (ListviewSelectedItems.Count > 0)
                        {
                            foreach (var tagname in ListviewSelectedItems)
                            {
                                List<string> sql_query_list = new List<string>();
                                string query = "Select ScriptMappingID,ScriptType,DataStartTime,DataEndTime,Average,Minimum,Maximum from DataPointsMappingTable where Tagname= @tagName";

                                Dictionary<string, string> tagnameParameter = new Dictionary<string, string>();
                                tagnameParameter.Add("@tagName", tagname);

                                var tag_Details_datatable = dbread_method(query, tagnameParameter);
                                if (tag_Details_datatable.Item1)
                                {
                                    DataTableReader dataTableReader = tag_Details_datatable.Item2.CreateDataReader();
                                    string get_id = string.Empty;
                                    string Script_Name = string.Empty;
                                    string tracename = string.Empty;

                                    while (dataTableReader.Read())
                                    {
                                        if (dataTableReader[0] != System.DBNull.Value && dataTableReader[0].ToString() != string.Empty)
                                        {
                                            get_id = dataTableReader[0].ToString();
                                        }

                                        if (dataTableReader[1] != System.DBNull.Value && dataTableReader[1].ToString() != string.Empty)
                                        {
                                            Script_Name = dataTableReader[1].ToString();

                                            if(Script_Name == "Deploy_Total")
                                            {
                                                Script_Name = "Deploy_Time";
                                            }

                                            if (Properties.Settings.Default.GrafanaPlotly)
                                            {
                                                tracename = GetTraceName(dataTableReader, get_id);
                                                sql_query_list.Add("from DataPointsTable where ScriptMappingID =" + get_id + " ORDER BY DatapointsID");
                                            }
                                            else
                                            {
                                                sql_query_list.Add("Select Iteration as time,ScriptDatapoint as value,'" + Script_Name + "'as metric from DataPointsTable where ScriptMappingID =" + get_id + " ORDER BY DatapointsID");
                                            }
                                        }
                                    }

                                    tag_Inputs.Add(tagname, new Tuple<List<string>, string, List<string>>(sql_query_list, Script_Name, new List<string> { tracename }));
                                }
                                else
                                {
                                    Qmsg.Show("Error in sqlquery for Tagname" + tag_Details_datatable.Item3.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                                }

                            }
                            if (tag_Inputs.Count > 0)
                            {
                                var Dashboard = Create_Dashboard(tag_Inputs, Dashboard_Title);
                            }
                            else
                            {
                                Qmsg.Show("Unable to collect datas from Grafana DB", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                        }
                        else
                        {
                            Qmsg.Show("Please select atleast one Tag to view Graph in Grafana", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                        }

                        //DeletDashboard(grafanaobjects, out resp); }

                    }
                    else
                    { Qmsg.Show("Please locate Browser Application path in preferences to view Grafana", "Error", MessageBoxButton.OK, MessageBoxImage.Error); }

                }
                else { Qmsg.Show("Please enter Grafana Dashboard Title", "Error", MessageBoxButton.OK, MessageBoxImage.Error); }


            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message.ToString());
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif               
            }
        }

        private string GetTraceName(DataTableReader dataTableReader, string scriptID)
        {
            string tracename = string.Empty;

            try
            {
                string startTime = string.Empty;
                string endTime = string.Empty;
                string average = string.Empty;
                string minimum = string.Empty;
                string maximum = string.Empty;
                TimeSpan? span = null;
                List<string> tracenamelst = new List<string>();

                if (dataTableReader[2] != System.DBNull.Value && dataTableReader[2].ToString() != string.Empty)
                    startTime = dataTableReader[2].ToString();
                if (dataTableReader[3] != System.DBNull.Value && dataTableReader[3].ToString() != string.Empty)
                    endTime = dataTableReader[3].ToString();

                if (!string.IsNullOrEmpty(startTime) && !string.IsNullOrEmpty(endTime))
                {
                    span = (Convert.ToDateTime(endTime) - Convert.ToDateTime(startTime));
                	tracename = "Execution Time: " + span.Value + " ";

                	//if (span.Value.TotalHours > 24)
                	//    tracename = "Execution Time:" + Math.Round(span.Value.TotalDays, 4).ToString() + "Days;";
                	//else if (span.Value.TotalMinutes > 60)
                	//    tracename = "Execution Time:" + Math.Round(span.Value.TotalHours, 4).ToString() + "hrs;";
                	//else if (span.Value.TotalSeconds > 60)
                	//    tracename = "Execution Time:" + Math.Round(span.Value.TotalMinutes, 4).ToString() + "mins;";
                	//else
                	//    tracename = "Execution Time:" + Math.Round(span.Value.TotalSeconds, 4).ToString() + "sec;";
            	}

            	if (dataTableReader[4] != System.DBNull.Value && dataTableReader[4].ToString() != string.Empty)
                	tracenamelst.Add("Avg: " + Math.Round(Convert.ToDouble(dataTableReader[4]), 1).ToString());
            	if (dataTableReader[5] != System.DBNull.Value && dataTableReader[5].ToString() != string.Empty)
                	tracenamelst.Add("Min: " + Math.Round(Convert.ToDouble(dataTableReader[5]), 1).ToString());
            	if (dataTableReader[6] != System.DBNull.Value && dataTableReader[6].ToString() != string.Empty)
                	tracenamelst.Add("Max: " + Math.Round(Convert.ToDouble(dataTableReader[6]), 1).ToString());

                if (tracenamelst.Count != 3)
                {
                    string[] avgMinMax = ReadAvgMinMaxValueFromDatapoints(scriptID);

                    if (string.IsNullOrEmpty(avgMinMax[3]))
                    {
                        string query = "Update DataPointsMappingTable set Average = " + avgMinMax[0] + ", Minimum =" + avgMinMax[1] + ", Maximum=" + avgMinMax[2] + " where ScriptMappingID = " + scriptID;
                        dbread_method(query, null);

                        avgMinMax = ReadAvgMinMaxValueFromDatapoints(scriptID);
                        if (!string.IsNullOrEmpty(avgMinMax[0]))
                            tracenamelst.Add("Avg: " + avgMinMax[0]);
                        if (!string.IsNullOrEmpty(avgMinMax[1]))
                            tracenamelst.Add("Min: " + avgMinMax[1]);
                        if (!string.IsNullOrEmpty(avgMinMax[2]))
                            tracenamelst.Add("Max: " + avgMinMax[2]);
                    }
                }

                if (tracenamelst.Count > 0)
                    tracename += string.Join(" ", tracenamelst);
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                        MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
            }

            return tracename;
        }

        public string[] ReadAvgMinMaxValueFromDatapoints(string scriptMappingID)
        {
            string remarks = string.Empty;
            string avgValue = string.Empty;
            string minValue = string.Empty;
            string maxValue = string.Empty;

            try
            {
                string query = "Select Avg(ScriptDatapoint), Min(ScriptDatapoint), Max(ScriptDatapoint) from DataPointsTable where ScriptMappingID =" + scriptMappingID;
                var tag_Details_datatable = dbread_method(query, null);
                if (tag_Details_datatable.Item1)
                {
                    DataTableReader dataTableReader = tag_Details_datatable.Item2.CreateDataReader();
                    {
                        while (dataTableReader.Read())
                        {
                            if (dataTableReader[0] != System.DBNull.Value && dataTableReader[0].ToString() != string.Empty)
                            {
                                avgValue = dataTableReader[0].ToString();
                            }

                            if (dataTableReader[1] != System.DBNull.Value && dataTableReader[1].ToString() != string.Empty)
                            {
                                minValue = dataTableReader[1].ToString();
                            }

                            if (dataTableReader[2] != System.DBNull.Value && dataTableReader[2].ToString() != string.Empty)
                            {
                                maxValue = dataTableReader[2].ToString();
                            }
                        }
                    }
                }
                else
                {
                    remarks = "Error occured while retrive Average, Minimum and Maximum values";
                }
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                        MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                remarks = "Error occured while retrive Average, Minimum and Maximum values";
            }

            return new string[] { avgValue, minValue, maxValue, remarks };
        }


        public Tuple<bool, string> Create_Dashboard(Dictionary<string, Tuple<List<string>, string, List<string>>> Panel_query, string title)
           {
            Dictionary<string, string> grafanaobjects = new Dictionary<string, string>();
            try
            {
                QatMessageBox Qmsg = new QatMessageBox(this);

                var panel_Object = Create_Panel(Panel_query);
            if (panel_Object.Item1)
            {
                var Dashboard_Object = Dashboard_creation(panel_Object.Item2, title);
                if (Dashboard_Object.Item1)
                {
                    string jsonbody = string.Empty;
                   // string key = "eyJrIjoiVUpJTm1zODJ5cnVZZVZZdHBOTlY4dFl3ZUlVYzUwTFEiLCJuIjoidGVzdHNxYSIsImlkIjoxfQ==";
                    string key = QatConstants.ApIKey;
                    grafanaobjects.Add("createurl", "/api/dashboards/db");
                    grafanaobjects.Add("Deleteurl", "/api/dashboards/uid/");
                    grafanaobjects.Add("portno", "3000");
                    grafanaobjects.Add("ipaddr", QatConstants.PcName.ToString());
                    grafanaobjects.Add("apikey", key);
                    
                    dynamic dashboard_Deserialize = new JavaScriptSerializer().DeserializeObject(Dashboard_Object.Item2);
                    string dashboard_Json = new JavaScriptSerializer().Serialize(dashboard_Deserialize);

                    grafanaobjects.Add("Grafana", dashboard_Json);

                    var post_response = HttpPostactual_json(grafanaobjects);

                    if (post_response.Item1)
                    {
                        grafanaobjects.Add("current_uid", post_response.Item2["uid"]);
                            grafanaobjects.Add("url", post_response.Item2["url"]);
                            var call_success=call_Grafana(grafanaobjects);
                            if(call_success.Item1)
                            {
                               // bool test = false;
                               //if(test)
                               // {
                               //     var delete_Success = DeletDashboard(grafanaobjects);
                               //     if (delete_Success.Item1)
                               //     {
                               //         Qmsg.Show("success", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                               //     }
                               //     else
                               //     {
                               //         Qmsg.Show("Error when Dashboard Delete" + delete_Success.Item2.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                               //     }
                               // }
                               // else
                               // {
                                    Qmsg.Show("Dashboard created successfully", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                                //}
                                
                                    
                            }
                            else                             
                            {
                                Qmsg.Show("Error when Dashboard invoke" + call_success.Item2.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);

                            }
                        
                        }
                    else
                    {
							grafanaobjects.Add("current_uid", "");
                            if(post_response.Item2["remarks"].ToString().Contains("name-exists"))
                                Qmsg.Show("Dashboard with same title exist. \nPlease enter different dashboard title", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            else
                                Qmsg.Show("Error when posting Dashboard " + post_response.Item2["remarks"].ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }

                }
                else
                {
                        Qmsg.Show("Error in Dashboard creation" + Dashboard_Object.Item3.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                 }
            }
            else
            {
                    Qmsg.Show("Error in panel creation" + panel_Object.Item3.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
           }

                return new Tuple<bool, string>(true, grafanaobjects["current_uid"]);
        }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                return new Tuple<bool, string>(false, grafanaobjects["current_uid"]);
            }
        }

        public Tuple<bool,string> call_Grafana(Dictionary<string, string> Grafana_call)
        {
            try
            {

                using (Process process = new Process())
                {
                    ProcessStartInfo startInfo = new ProcessStartInfo();
                    startInfo.UseShellExecute = false;
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.FileName = Properties.Settings.Default.BrowserPath.ToString();
                    process.StartInfo.Arguments = "http://" + Grafana_call["ipaddr"].ToString() + ":" + Grafana_call["portno"].ToString() + Grafana_call["url"];
                    process.Start();
                }
                return new Tuple<bool, string>(true,string.Empty);
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);

#endif
                return new Tuple<bool, string>(false, ex.Message.ToString());
            }
        }
        public Tuple<bool, DataTable, string>dbread_method(string query, Dictionary<string,string> parameterValues)
        {
            DataTable dataTable = new DataTable();
            try
            {
               
                SqlCommand command = new SqlCommand(query, grafana_connection.CreateConnection());

                if (parameterValues != null)
                {
                    foreach (var item in parameterValues)
                    {
                        command.Parameters.AddWithValue(item.Key, item.Value);
                    }
                }

                SqlDataAdapter adap = new SqlDataAdapter(command);
                grafana_connection.OpenConnection();            
                adap.Fill(dataTable);
                grafana_connection.CloseConnection();
                return new Tuple<bool, DataTable, string>(true, dataTable, string.Empty);

            }
            catch(Exception ex)
            {
                return new Tuple<bool, DataTable,string>(false, dataTable,ex.Message.ToString());
            }
            finally
            {
                
                    grafana_connection.CloseConnection();
            }
        }
       
        private void btn_RemoveTag_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                QatMessageBox Qmsg = new QatMessageBox(this);
                if ( ListviewSelectedItems.Count==0)
                {
                    Qmsg.Show("Please select tag name to remove", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                string tagsNames = string.Join("\n", ListviewSelectedItems);               
                MessageBoxResult msgresult = Qmsg.Show("Are you sure you want to remove the selected tags? \n" + tagsNames, "Warning", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (msgresult == MessageBoxResult.Yes)
                {
                    string querytaglist = string.Empty;

                List<string> copylist = new List<string>(ListviewSelectedItems);              

                foreach (string tag in copylist)
                {
                    querytaglist += " TagName='" + tag + "' OR";                 
                }

                if(querytaglist.EndsWith("OR"))
                    querytaglist = querytaglist.Remove(querytaglist.Length - 2);
                
                string query = "Update DataPointsMappingTable set TagName ='"+string.Empty+"' where" + querytaglist;                

                DataTable dataTable = new DataTable();
                SqlDataAdapter dataAdapter = new SqlDataAdapter(query, grafana_connection.CreateConnection());
                grafana_connection.OpenConnection();
                dataAdapter.Fill(dataTable);
                grafana_connection.CloseConnection();

                foreach (string tag in copylist)
                {
                    GrafanaGUI_ItemsList[0].TagNameList.Remove(tag);
                    datafromDB.AsEnumerable().Where(x => x.Field<string>("TagName") == tag).First().SetField("TagName", string.Empty);

                        if (tag == TagItemList[0].TagNameText)
                            TagItemList[0].TagNameText = string.Empty;
                    }
                    
                  
                    Qmsg.Show("Tag name removed successfully", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    return;
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


        private void lstview_Tagname_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                ListView sourceListView = sender as ListView;
                ListviewSelectedItems.Clear();

                foreach (string item in sourceListView.SelectedItems)
                {
                    ListviewSelectedItems.Add(item);
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

        private List<string> listviewSelectedItemsValue = new List<string>();
        public List<string> ListviewSelectedItems
        {
            get { return listviewSelectedItemsValue; }
            set { listviewSelectedItemsValue = value; OnPropertyChanged("ListviewSelectedItems"); }
        }
        public Tuple<bool, Dictionary<string, string>> HttpPostactual_json(Dictionary<string, string> grafanaobjects)
        {
            Dictionary<string, string> output = new Dictionary<string, string>();
            string strResponse = string.Empty;
            bool success = false;
            string uid = string.Empty;
            string url = string.Empty;
            string status = string.Empty;
            string uriconstruct = "http://" + grafanaobjects["ipaddr"].ToString() + ":" + grafanaobjects["portno"].ToString() + grafanaobjects["createurl"].ToString();
            System.Net.HttpWebRequest req = (HttpWebRequest)System.Net.WebRequest.Create(uriconstruct);
            try
            {

                req.Timeout = 25000;
                req.ReadWriteTimeout = 25000;
                req.ContentType = "application/json";
                req.Method = "POST";
                req.Accept = "application/json";
                req.Headers["Authorization"] = "Bearer " + grafanaobjects["apikey"].ToString();


                Byte[] retBytes = System.Text.Encoding.ASCII.GetBytes(grafanaobjects["Grafana"].ToString());
                req.ContentLength = retBytes.Length;

                using (System.IO.Stream outStream = req.GetRequestStream())
                {
                    outStream.Write(retBytes, 0, retBytes.Length);
                    outStream.Close();
                }
                using (HttpWebResponse resp = (HttpWebResponse)req.GetResponse())
                {
                    //success = HttpStatusCodeCheck(resp, "POST", out strResponse);
                    //if (success)
                    //{
                        using (System.IO.StreamReader sr = new System.IO.StreamReader(resp.GetResponseStream()))
                        {
                            strResponse = sr.ReadToEnd().Trim();
                          

                        var resultValue = new System.Web.Script.Serialization.JavaScriptSerializer { MaxJsonLength = Int32.MaxValue }.Deserialize<JsonGrafana>(strResponse);
                        output.Add("uid", resultValue.uid);
                        output.Add("url", resultValue.url);
                        output.Add("status", resultValue.status);
                            req.Abort();

                        }
                    //}

                }
                return new Tuple<bool, Dictionary<string, string>>(true, output);

            }
            catch (WebException ex)
            {
                if (ex.Response != null)
                {
                    using (StreamReader temp = new StreamReader(ex.Response.GetResponseStream()))
                    {
                        var resp = temp.ReadToEnd().ToString();
                        output.Add("remarks", resp);
                    }
                }
                else
                {
                    output.Add("remarks", ex.Message.ToString());
                }
                req.Abort();
                return new Tuple<bool, Dictionary<string, string>>(false, output);
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                ExecutionMessageBox("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                output.Add("remarks", ex.ToString());
                return new Tuple<bool, Dictionary<string, string>>(false, output);
            }
        }
            private bool HttpStatusCodeCheck(HttpWebResponse response, string methodName, out string strResponse)
        {
            strResponse = string.Empty;
            try
            {
                if (response == null)
                    return false;

                if (((methodName == "GET") && (response.StatusCode == HttpStatusCode.OK))
                    || ((methodName == "POST") && (response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.Created))
                    || ((methodName == "PUT") && (response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.Created || response.StatusCode == HttpStatusCode.NoContent)))
                {
                    using (System.IO.StreamReader sr = new System.IO.StreamReader(response.GetResponseStream()))
                    {
                        strResponse = sr.ReadToEnd().Trim();
                        return true;
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        public Tuple<bool, string> DeletDashboard(Dictionary<string, string> grafanaobjects)
        {
            //bool msg = false;
            string strResponse = string.Empty;
            //bool success = false;
            string uid = string.Empty;
            string url = string.Empty;
            string status = string.Empty;
            string der = grafanaobjects["current_uid"].Replace("\"", "");
            string uriconstruct = "http://" + grafanaobjects["ipaddr"].ToString() + ":" + grafanaobjects["portno"].ToString() + grafanaobjects["Deleteurl"].ToString() + der;
            System.Net.HttpWebRequest req = (HttpWebRequest)System.Net.WebRequest.Create(uriconstruct);
            try
            {

                req.Timeout = 25000;
                req.ReadWriteTimeout = 25000;
                req.ContentType = "application/json";
                req.Method = "DELETE";
                req.Accept = "application/json";
                req.Headers["Authorization"] = "Bearer " + grafanaobjects["apikey"].ToString();

                using (HttpWebResponse resp = (HttpWebResponse)req.GetResponse())
                {
               
                    using (System.IO.StreamReader sr = new System.IO.StreamReader(resp.GetResponseStream()))
                    {
                        strResponse = sr.ReadToEnd().Trim();                 
                         var resultValue = new System.Web.Script.Serialization.JavaScriptSerializer { MaxJsonLength = Int32.MaxValue }.Deserialize<JsonGrafanaDelete>(strResponse);
                        uid = resultValue.title;

                        req.Abort();
                    }
                }


                return new Tuple<bool, string>(true, string.Empty);

            }
            catch (WebException ex)
            {
                string resp = string.Empty;
                using (StreamReader temp = new StreamReader(ex.Response.GetResponseStream()))
                {
                     resp = temp.ReadToEnd().ToString();
                  
                }
                req.Abort();            

                return new Tuple<bool, string>(false, resp);
            }
        }
        //string url
        public Tuple<bool,string,string> panels_creation(string query, string db, string panel_title, int no_of_panels)
        {
            string panelobject = string.Empty;
            try
            {
                 panelobject = "{\r\n \"aliasColors\": {}," +
                    "\r\n \"bars\": false," +
                    "\r\n \"dashLength\": 10," +
                    "\r\n      \"dashes\": false," +
                    "\r\n      \"datasource\": \"" + db + "\"," +
                    "\r\n      \"fieldConfig\": {\r\n\"" +
                                      "defaults\": {\r\n \"" +
                                      "custom\": {\r\n \"align\": null\r\n}," +
                                      "\r\n \"mappings\": []," +
                                      "\r\n \"thresholds\": {\r\n\"mode\": \"absolute\"," +
                                      "         \r\n\"steps\": [\r\n  {\r\n \"color\": \"green\",\r\n\"value\": null\r\n}," +
                                      "         \r\n {\r\n \"color\": \"red\",\r\n\"value\": 80\r\n}\r\n ]\r\n }\r\n}," +
                                      "\r\n\"overrides\": []\r\n}," +
                    "\r\n \"fill\": 1," +
                    "\r\n \"fillGradient\": 0," +
                    "\r\n\"gridPos\": {\r\n \"h\": 9," +
                                        "\r\n \"w\": 12," +
                                        "\r\n\"x\": 0," +
                                        "\r\n \"y\": 0\r\n }," +
                    "\r\n\"hiddenSeries\": false," +
                    "\r\n\"id\": "+no_of_panels+"," +
                    "\r\n\"legend\": {\r\n\"avg\": true,\r\n\"current\": true,\r\n\"max\": true,\r\n\"min\": false,\r\n\"show\": true,\r\n\"total\": true,\r\n\"values\": true\r\n}," +
                    "\r\n\"lines\": true," +
                    "\r\n\"linewidth\": 1," +
                    "\r\n\"nullPointMode\": \"null\"," +
                    "\r\n\"options\": {\r\n\"dataLinks\": []\r\n}," +
                    "\r\n\"percentage\": false," +
                    "\r\n\"pluginVersion\": \"7.0.1\"," +
                    "\r\n\"pointradius\": 2," +
                    "\r\n\"points\": false," +
                    "\r\n\"renderer\": \"flot\"," +
                    "\r\n\"seriesOverrides\": []," +
                    "\r\n\"spaceLength\": 10," +
                    "\r\n\"stack\": false," +
                    "\r\n\"steppedLine\": false," +
                    "\r\n\"targets\": [\r\n" + query + "\r\n]," +
                    "\r\n\"thresholds\": []," +
                    "\r\n\"timeFrom\": null," +
                    "\r\n\"timeRegions\": []," +
                    "\r\n\"timeShift\": null," +
                    "\r\n\"title\": \"" + panel_title + "\"," +
                    "\r\n\"tooltip\": {\r\n\"shared\": true,\r\n\"sort\": 0,\r\n\"value_type\": \"individual\"\r\n}," +
                    "\r\n\"type\": \"graph\"," +
                    "\r\n\"xaxis\": {\r\n\"buckets\": null,\r\n\"mode\": \"time\",\r\n\"name\": null,\r\n\"show\": true,\r\n\"values\": []\r\n}," +
                    "\r\n\"yaxes\": [\r\n{\r\n\"format\": \"short\",\r\n\"label\": null,\r\n\"logBase\": 1,\r\n\"max\": null,\r\n\"min\": null,\r\n\"show\": true\r\n},\r\n{\r\n\"format\": \"short\",\r\n\"label\": null,\r\n\"logBase\": 1,\r\n\"max\": null,\r\n\"min\": null,\r\n\"show\": true\r\n}\r\n]," +
                    "\r\n\"yaxis\": {\r\n\"align\": false,\r\n\"alignLevel\": null\r\n} \r\n}";
                
                return new Tuple<bool, string,string>(true, panelobject,string.Empty); 


            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                return new Tuple<bool, string,string>(false, panelobject,ex.Message.ToString());
            }
            
        }

        public Tuple<bool, string, string> PanelCreation_Plotly(string query, string db, string panel_title, int no_of_panels, string traces, string xAxisTitle, string yAxisTitle)
        {
            string panelobject = string.Empty;
            try
            {
                if (yAxisTitle.StartsWith("Deploy"))
                    yAxisTitle += " in sec";
                else if (yAxisTitle.StartsWith("CPU"))
                    yAxisTitle += " in %";

                panelobject = "{\r\n \"datasource\": \"" + db + "\"," +
                   "\r\n      \"fieldConfig\": {\r\n\"" +
                        "defaults\": {\r\n\"" +
                        "custom\": {}" +
                    "\r\n}," +
        "\r\n\"overrides\": []" +
    "\r\n}," +
      "\r\n\"gridPos\": {" +
        "\r\n\"h\": 8," +
        "\r\n\"w\": 14," +
        "\r\n\"x\": 0," +
        "\r\n\"y\": 0" +
      "\r\n}," +
      "\r\n\"id\": " + no_of_panels + "," +
      "\r\n\"pconfig\": {" +
        "\r\n\"fixScale\": \"\"," +
        "\r\n\"layout\": {" +
          "\r\n\"dragmode\": \"\"," +
          "\r\n\"font\": {" +
          "\r\n\"family\":  \"\\\"Open Sans\\\", Helvetica, Arial, sans-serif\"" +
          "\r\n}," +
          "\r\n\"hovermode\": \"closest\"," +
          "\r\n\"legend\": {" +
          "\r\n\"orientation\": \"h\"" +
          "\r\n}," +
          "\r\n\"showlegend\": true," +
          "\r\n\"xaxis\": {" +
          "\r\n\"rangemode\": \"normal\"," +
          "\r\n\"showgrid\": true," +
          "\r\n\"title\": \"" + xAxisTitle + "\"," +
          "\r\n\"type\": \"\"," +
          "\r\n\"zeroline\": true" +
          "\r\n}," +
          "\r\n\"yaxis\": {" +
          "\r\n\"rangemode\": \"normal\"," +
          "\r\n\"showgrid\": true," +
          "\r\n\"title\": \"" + yAxisTitle + "\"," +
          "\r\n\"type\": \"\"," +
          "\r\n\"zeroline\": true" +
          "\r\n}," +
          "\r\n\"zaxis\": {" +
            "\r\n\"rangemode\": \"normal\"," +
            "\r\n\"showgrid\": true," +
            "\r\n\"type\": \"linear\"," +
            "\r\n\"zeroline\": false" +
          "\r\n}" +
        "\r\n}," +
        "\r\n\"loadFromCDN\": false," +
        "\r\n\"settings\": {" +
          "\r\n\"displayModeBar\": true," +
          "\r\n\"type\": \"scatter\"" +
        "\r\n}," +
        "\r\n\"showAnnotations\": true," +

        "\r\n\"traces\": [" + traces +
        "\r\n]" +

      "\r\n}," +
      "\r\n\"targets\": [\r\n" + query + "\r\n]," +
      "\r\n\"timeFrom\": null," +
      "\r\n\"timeShift\": null," +
      "\r\n\"title\": \"" + panel_title + "\"," +
      "\r\n\"type\": \"natel-plotly-panel\"," +
      "\r\n\"version\": 1" +
    "\r\n}";

                return new Tuple<bool, string, string>(true, panelobject, string.Empty);


            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                return new Tuple<bool, string, string>(false, panelobject, ex.Message.ToString());
            }

        }

        public Tuple<bool, string, string> TraceCreation_Plotly(string traceName, string xAxisName, string yAxisName, string lineColor)
        {
            string traceobject = string.Empty;
            try
            {
                traceobject = "\r\n{" +
            "\r\n\"mapping\": {" +
              "\r\n\"color\": \"" + yAxisName + "\"," +
              "\r\n\"size\": null," +
              "\r\n\"text\": null," +
              "\r\n\"x\": \"" + xAxisName + "\"," +
              "\r\n\"y\": \"" + yAxisName + "\"," +
              "\r\n\"z\": null" +
            "\r\n}," +
            "\r\n\"name\": \"" + traceName + "\"," +
            "\r\n\"settings\": {" +
              "\r\n\"color_option\": \"ramp\"," +
              "\r\n\"line\": {" +
                "\r\n\"color\": \"" + lineColor + "\"," +
                "\r\n\"dash\": \"solid\"," +
                "\r\n\"shape\": \"linear\"," +
                "\r\n\"width\": 3" +
              "\r\n}," +
              "\r\n\"marker\": {" +
                "\r\n\"color\": \"#33B5E5\"," +
                "\r\n\"colorscale\": \"YlOrRd\"," +
                "\r\n\"line\": {" +
                  "\r\n\"color\": \"#DDD\"," +
                  "\r\n\"width\": 0" +
                "\r\n}," +
                "\r\n\"showscale\": false," +
                "\r\n\"size\": 10," +
                "\r\n\"sizemin\": 3," +
                "\r\n\"sizemode\": \"diameter\"," +
                "\r\n\"sizeref\": 0.2," +
                "\r\n\"symbol\": \"circle\"" +
              "\r\n}" +
            "\r\n}," +
            "\r\n\"show\": {" +
              "\r\n\"line\": true," +
              "\r\n\"lines\": true," +
              "\r\n\"markers\": false" +
            "\r\n}" +
         "\r\n}";

                return new Tuple<bool, string, string>(true, traceobject, string.Empty);


            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                return new Tuple<bool, string, string>(false, traceobject, ex.Message.ToString());
            }

        }

        public Tuple<bool,string,string> targets_creation(string tagname, string q_no, int map_key)
        {
            string Temp_Query_construct = string.Empty;
            try
            {
                //string q_no = ((char)query_No).ToString().ToUpper();
                string queryconstruct = tagname;
                 Temp_Query_construct = "{\r\n\"alias\": \"\"," +
                                                     "\r\n\"format\": \"time_series\"," +
                                                          "\r\n\"rawSql\": \"" + queryconstruct + "\"," +
                                                              "\r\n\"refId\": \"" + q_no + "\"\r\n}";
                return new Tuple<bool,string,string>(true,Temp_Query_construct,string.Empty);


            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                return new Tuple<bool, string,string>(false, Temp_Query_construct,ex.Message.ToString());
            }
        }

        private Tuple<bool, string, string> Create_Panel(Dictionary<string, Tuple<List<string>, string, List<string>>> Panel_query)
        {

            string uid = string.Empty;
            string url = string.Empty;
            //string Dashboard_Title = "nqwew ppt1";
            string DB_name = QatConstants.Grafana_DbDatabaseName;

            string panel = string.Empty;
            int map_Key = 1;
            string panel_collection = string.Empty;

            try
            {
                foreach (var panelname in Panel_query)
                {
                    string targetCollection = string.Empty;
                    string traceCollection = string.Empty;
                    int queryidentifier = 65;
                    string queries = string.Empty;
                    string panelnameq = panelname.Key.ToString() +" - " + DateTime.UtcNow.ToString();
                    int i = 0;
                    for (int j=0; j< panelname.Value.Item1.Count; j++)
                    {
                        string q_no = ((char)queryidentifier).ToString().ToUpper();
                        string selectquery = string.Empty;
                        //string selectquery = "Select Iteration as time, Datapoints as " + q_no + "Datapoint, Iteration as " + q_no + "iteration " + valu;
                        if(Properties.Settings.Default.GrafanaPlotly)
                            selectquery = "Select Iteration as time, ScriptDatapoint as " + q_no + "Datapoint, Iteration as " + q_no + "iteration " + panelname.Value.Item1[j];
                        else
                            selectquery = panelname.Value.Item1[j];

                        var collect_query = targets_creation(selectquery, q_no, map_Key);
                        if (collect_query.Item1)
                        {
                            if (i > 0)
                                targetCollection += ",\r\n";
                            targetCollection += collect_query.Item2;                            
                        }
                        else
                        {
                            return new Tuple<bool, string, string>(false, panel_collection, collect_query.Item3.ToString());
                        }

                        if (Properties.Settings.Default.GrafanaPlotly)
                        {
                            string colorinHex = System.Drawing.ColorTranslator.ToHtml(System.Drawing.Color.FromArgb(colorsList[colorsCnt].ToArgb()));
                            colorsCnt++;

                            if (colorsCnt == (colorsList.Count() - 1))
                                colorsCnt = 0;

                            var traceQuery = TraceCreation_Plotly(panelname.Value.Item3[j], q_no + "iteration", q_no + "Datapoint", colorinHex);
                            if (traceQuery.Item1)
                            {
                                if (i > 0)
                                    traceCollection += ",\r\n";
                                traceCollection += traceQuery.Item2;
                                queryidentifier++;
                                i++;
                            }
                            else
                            {
                                return new Tuple<bool, string, string>(false, panel_collection, traceQuery.Item3.ToString());
                            }
                        }
                        else
                        {
                            queryidentifier++;
                            i++;
                        }
                    }

                    if (Properties.Settings.Default.GrafanaPlotly)
                    {
                        var collect_panel = PanelCreation_Plotly(targetCollection, DB_name, panelnameq, map_Key, traceCollection, "No Of Iteration", panelname.Value.Item2);
                        if (collect_panel.Item1)
                        {
                            if (map_Key > 1)
                                panel_collection += ",\r\n";
                            panel_collection += collect_panel.Item2;

                            i++;
                            map_Key++;
                        }
                        else
                        {
                            return new Tuple<bool, string, string>(false, panel_collection, collect_panel.Item3.ToString());
                        }
                    }
                    else
                    {
                        var collect_panel = panels_creation(targetCollection, DB_name, panelnameq, map_Key);
                        if (collect_panel.Item1)
                        {
                            if (map_Key > 1)
                                panel_collection += ",\r\n";
                            panel_collection += collect_panel.Item2;

                            i++;
                            map_Key++;
                        }
                        else
                        {
                            return new Tuple<bool, string, string>(false, panel_collection, collect_panel.Item3.ToString());
                        }
                    }
                }

                return new Tuple<bool, string, string>(true, panel_collection, string.Empty);
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                return new Tuple<bool, string, string>(false, panel_collection, ex.Message.ToString());

            }
        }
        
        public Tuple<bool,string,string> Dashboard_creation(string p_obj, string title)
        {
          
            string json = string.Empty;
            try
            {
                 json = "{ \"dashboard\": " +
                          " {\"id\": null," +
                          "\r\n\"uid\": null," +
                          "\r\n\"editable\": true," +
                          "\r\n\"links\": []," +
                          "\r\n\"panels\": [\r\n" + p_obj + "\r\n ]," +
                          "\r\n\"refresh\": false," +
                          "\r\n\"schemaVersion\": 25," +
                          "\r\n\"style\": \"dark\"," +
                          "\r\n\"tags\": []," +
                          "\r\n\"templating\": {\r\n\"list\": []\r\n}," +
                          "\r\n\"time\": {\r\n\"from\": \"1970-01-01T00:00:00.001Z\",\r\n\"to\": \"1970-01-01T00:00:00.010Z\"\r\n  }," +
                          "\r\n\"timepicker\": {\r\n\"refresh_intervals\": [\r\n\"10s\",\r\n\"30s\",\r\n\"1m\",\r\n\"5m\",\r\n\"15m\",\r\n\"30m\",\r\n\"1h\",\r\n\"2h\",\r\n\"1d\"\r\n]\r\n}," +
                          "\r\n\"timezone\":\"utc\"," +
                          "\r\n\"title\": \"" + title + "\"," +
                          "\r\n\"version\": 0," +
                          "\r\n\"folderId\": 0," +
                          "\r\n\"overwrite\": false}" +
                          "}";

              

                return new Tuple<bool, string,string>(true, json,string.Empty);


            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                return new Tuple<bool, string,string>(false, json,ex.Message.ToString());
            }
        }


//        public void DeletDashboard(Dictionary<string, string> grafanaobjects, out string weburl)
//        {
//            try
//            {
                
//                Httpdeleteactual_json(grafanaobjects);
               
//            }
//            catch (Exception ex)
//            {
//                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
//#if DEBUG
//                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
//#endif
//            }
           
//        }
        public class GrafanaTagSettings : INotifyPropertyChanged
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
                    DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                }
            }
                    

            private GrafanaGraph parentTestActionItemValue = null;
            public GrafanaGraph ParentTestActionItem
            {
                get { return parentTestActionItemValue; }
                set { parentTestActionItemValue = value; OnPropertyChanged("ParentTestActionItem"); }
            }

            private ObservableCollection<string> executionIDlistValue = new ObservableCollection<string>();
            public ObservableCollection<string> ExecutionIDlist
            {
                get { return executionIDlistValue; }
                set { executionIDlistValue = value; OnPropertyChanged("ExecutionIDlist"); }
            }

            private ObservableCollection<string> testSuiteNamelistValue = new ObservableCollection<string>();
            public ObservableCollection<string> TestSuiteNamelist
            {
                get { return testSuiteNamelistValue; }
                set { testSuiteNamelistValue = value; OnPropertyChanged("TestSuiteNamelist"); }
            }

            private ObservableCollection<string> testplanNamelistValue = new ObservableCollection<string>();
            public ObservableCollection<string> TestplanNamelist
            {
                get { return testplanNamelistValue; }
                set { testplanNamelistValue = value; OnPropertyChanged("TestplanNamelist"); }
            }

            private ObservableCollection<string> testCaseNamelistValue = new ObservableCollection<string>();
            public ObservableCollection<string> TestCaseNamelist
            {
                get { return testCaseNamelistValue; }
                set { testCaseNamelistValue = value; OnPropertyChanged("TestCaseNamelist"); }
            }

            private ObservableCollection<string> testActionNamelistValue = new ObservableCollection<string>();
            public ObservableCollection<string> TestActionNamelist
            {
                get { return testActionNamelistValue; }
                set { testActionNamelistValue = value; OnPropertyChanged("TestActionNamelist"); }
            }

            private ObservableCollection<string> scriptTypelistValue = new ObservableCollection<string>();
            public ObservableCollection<string> ScriptTypelist
            {
                get { return scriptTypelistValue; }
                set { scriptTypelistValue = value; OnPropertyChanged("ScriptTypelist"); }
            }

            private ObservableCollection<string> tabStartTimelistValue = new ObservableCollection<string>();
            public ObservableCollection<string> TabStartTimelist
            {
                get { return tabStartTimelistValue; }
                set { tabStartTimelistValue = value; OnPropertyChanged("TabStartTimelist"); }
            }

            private string tagNameTextValue = string.Empty;
            public string TagNameText
            {
                get { return tagNameTextValue; }
                set { tagNameTextValue = value; OnPropertyChanged("TagNameText"); }
            }

            private string execIDlistSelectedItemValue = string.Empty;
            public string ExecutionIDlistSelectedItem
            {
                get { return execIDlistSelectedItemValue; }
                set
                {
                    execIDlistSelectedItemValue = value;
                    TestSuiteNamelist.Clear();
                    TestplanNamelist.Clear();
                    TestCaseNamelist.Clear();
                    TestActionNamelist.Clear();
                    ScriptTypelist.Clear();
                    TabStartTimelist.Clear();
                    TagNameText = string.Empty;

                    if (value != null && value != string.Empty)
                        ParentTestActionItem.Get_Selected_ExecId_Details(execIDlistSelectedItemValue);

                    OnPropertyChanged("ExecutionIDlistSelectedItem");
                }
            }

            private string suitelistSelectedItemValue = string.Empty;
            public string TestSuiteNamelistSelectedItem
            {
                get { return suitelistSelectedItemValue; }
                set
                {
                    suitelistSelectedItemValue = value;
                    TestplanNamelist.Clear();
                    TestCaseNamelist.Clear();
                    TestActionNamelist.Clear();
                    ScriptTypelist.Clear();
                    TabStartTimelist.Clear();
                    TagNameText = string.Empty;

                    if (value != null && value != string.Empty)
                        ParentTestActionItem.Get_Selected_Testsuite_Details(ExecutionIDlistSelectedItem, suitelistSelectedItemValue);

                    OnPropertyChanged("TestSuiteNamelistSelectedItem");
                }
            }

            private string planlistSelectedItemValue = string.Empty;
            public string TestplanNamelistSelectedItem
            {
                get { return planlistSelectedItemValue; }
                set
                {
                    planlistSelectedItemValue = value;
                    TestCaseNamelist.Clear();
                    TestActionNamelist.Clear();
                    ScriptTypelist.Clear();
                    TabStartTimelist.Clear();
                    TagNameText = string.Empty;

                    if (value != null && value != string.Empty)
                        ParentTestActionItem.Get_Selected_Testplan_Details(ExecutionIDlistSelectedItem, TestSuiteNamelistSelectedItem, planlistSelectedItemValue);

                    OnPropertyChanged("TestplanNamelistSelectedItem");
                }
            }

            private string caselistSelectedItemValue = string.Empty;
            public string TestCaseNamelistSelectedItem
            {
                get { return caselistSelectedItemValue; }
                set
                {
                    caselistSelectedItemValue = value;
                    TestActionNamelist.Clear();
                    ScriptTypelist.Clear();
                    TabStartTimelist.Clear();
                    TagNameText = string.Empty;

                    if (value != null && value != string.Empty)
                        ParentTestActionItem.Get_Selected_Testcase_Details(ExecutionIDlistSelectedItem, TestSuiteNamelistSelectedItem, TestplanNamelistSelectedItem, caselistSelectedItemValue);

                    OnPropertyChanged("TestCaseNamelistSelectedItem");
                }
            }

            private string actionlistSelectedItemValue = string.Empty;
            public string TestActionNamelistSelectedItem
            {
                get { return actionlistSelectedItemValue; }
                set
                {
                    actionlistSelectedItemValue = value;
                    ScriptTypelist.Clear();
                    TabStartTimelist.Clear();
                    TagNameText = string.Empty;

                    if (value != null && value != string.Empty)
                        ParentTestActionItem.Get_Selected_TestAction_Details(ExecutionIDlistSelectedItem, TestSuiteNamelistSelectedItem, TestplanNamelistSelectedItem, TestCaseNamelistSelectedItem, actionlistSelectedItemValue);

                    OnPropertyChanged("TestActionNamelistSelectedItem");
                }
            }

            private string scriptTypeSelectedItemValue = string.Empty;
            public string ScriptTypeSelectedItem
            {
                get { return scriptTypeSelectedItemValue; }
                set
                {
                    scriptTypeSelectedItemValue = value;
                    TabStartTimelist.Clear();
                    TagNameText = string.Empty;

                    if (value != null && value != string.Empty)
                        ParentTestActionItem.Get_Selected_scriptType_Details(ExecutionIDlistSelectedItem, TestSuiteNamelistSelectedItem, TestplanNamelistSelectedItem, TestCaseNamelistSelectedItem, TestActionNamelistSelectedItem, scriptTypeSelectedItemValue);

                    OnPropertyChanged("ScriptTypeSelectedItem");
                }
            }

            private string tabStartTimelistSelectedItemValue = string.Empty;
            public string TabStartTimelistSelectedItem
            {
                get { return tabStartTimelistSelectedItemValue; }
                set
                {
                    tabStartTimelistSelectedItemValue = value;
                    TagNameText = string.Empty;

                    if(value != null && value != string.Empty)
                       ParentTestActionItem.Get_Selected_TabstartTime_Details(ExecutionIDlistSelectedItem, TestSuiteNamelistSelectedItem, TestplanNamelistSelectedItem, TestCaseNamelistSelectedItem, TestActionNamelistSelectedItem, ScriptTypeSelectedItem, tabStartTimelistSelectedItemValue);

                    OnPropertyChanged("TabStartTimelistSelectedItem");
                }
            }            
        }

        
        public class GrafanaGUI : INotifyPropertyChanged
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
                    DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                }
            }

            private GrafanaGraph parentTestActionItemValue = null;
            public GrafanaGraph ParentTestActionItem
            {
                get { return parentTestActionItemValue; }
                set { parentTestActionItemValue = value; OnPropertyChanged("ParentTestActionItem"); }
            }

            private ObservableCollection<string> scriptNamelistValue = new ObservableCollection<string>();
            public ObservableCollection<string> ScriptNamelist
            {
                get { return scriptNamelistValue; }
                set { scriptNamelistValue = value; OnPropertyChanged("ScriptNamelist"); }
            }

            private ObservableCollection<string> releaseVersionlistValue = new ObservableCollection<string>();
            public ObservableCollection<string> ReleaseVersionlist
            {
                get { return releaseVersionlistValue; }
                set { releaseVersionlistValue = value; OnPropertyChanged("ReleaseVersionlist"); }
            }

            private ObservableCollection<string> designNamelistValue = new ObservableCollection<string>();
            public ObservableCollection<string> DesignNamelist
            {
                get { return designNamelistValue; }
                set { designNamelistValue = value; OnPropertyChanged("DesignNamelist"); }
            }

            private string scriptNamelistSelectedItemValue = string.Empty;
            public string ScriptNamelistSelectedItem
            {
                get { return scriptNamelistSelectedItemValue; }
                set
                {
                    scriptNamelistSelectedItemValue = value;
                    ParentTestActionItem.Get_Selected_filteredDetails(ScriptNamelistSelectedItem, ReleaseVersionlistSelectedItem, DesignNamelistSelectedItem);
                    OnPropertyChanged("ScriptNamelistSelectedItem");
                }
            }

            private string releaseVerlistSelectedItemValue = string.Empty;
            public string ReleaseVersionlistSelectedItem
            {
                get { return releaseVerlistSelectedItemValue; }
                set
                {
                    releaseVerlistSelectedItemValue = value;
                    ParentTestActionItem.Get_Selected_filteredDetails(ScriptNamelistSelectedItem, ReleaseVersionlistSelectedItem, DesignNamelistSelectedItem);
                    OnPropertyChanged("ReleaseVersionlistSelectedItem");
                }
            }

            private string designNamelistSelectedItemValue = string.Empty;
            public string DesignNamelistSelectedItem
            {
                get { return designNamelistSelectedItemValue; }
                set
                {
                    designNamelistSelectedItemValue = value;
                    ParentTestActionItem.Get_Selected_filteredDetails(ScriptNamelistSelectedItem, ReleaseVersionlistSelectedItem, DesignNamelistSelectedItem);
                    OnPropertyChanged("DesignNamelistSelectedItem");
                }
            }

            private ObservableCollection<string> tagNameListValue = new ObservableCollection<string>();
            public ObservableCollection<string> TagNameList
            {
                get { return tagNameListValue; }
                set { tagNameListValue = value; OnPropertyChanged("TagNameList"); }
            }

            private string dashboardTitleTextValue = string.Empty;
            public string DashboardTitleText
            {
                get { return dashboardTitleTextValue; }
                set { dashboardTitleTextValue = value; OnPropertyChanged("DashboardTitleText"); }
            }

        }

        private void Compare_Click(object sender, RoutedEventArgs e)
        {

            try
            {
                QatMessageBox Qmsg = new QatMessageBox(this);
                string Dashboard_Title = GrafanaGUI_ItemsList[0].DashboardTitleText.Trim();
                Dictionary<string, Tuple<List<string>, string, List<string>>> tag_Inputs = new Dictionary<string, Tuple<List<string>, string, List<string>>>();
                if (!string.IsNullOrEmpty(Dashboard_Title))
                {
                    if (!string.IsNullOrEmpty(Properties.Settings.Default.BrowserPath.ToString()))
                    {
                        List<string> sql_query_list = new List<string>();
                        string Script_Name = string.Empty;

                        if (ListviewSelectedItems.Count > 0)
                        {

                            //string combine_all_tagnames = "'" + string.Join("','", ListviewSelectedItems) + "'";
                            //string[] array = ListviewSelectedItems.ToArray();                        
                            //List<string> parametersList = new List<string>(combine_all_tagnames.Split(','));

                            //List<string> parametersList = new List<string>();
                            //for (int i=0; i< ListviewSelectedItems.Count(); i++)                            
                            //    parametersList.Add("@parameter_" + i);



                            Dictionary<string, string> tagnameParameter = new Dictionary<string, string>();
                            for (int i = 0; i < ListviewSelectedItems.Count(); i++)
                                tagnameParameter.Add("@parameter_" + i, ListviewSelectedItems[i]);

                            string combine_all_tagnames = string.Join(",", tagnameParameter.Keys);

                            string combined_tag_query = "Select distinct (ScriptType) from DataPointsMappingTable where Tagname in ("+ combine_all_tagnames+")";  
                            var IsDistinct = dbread_method(combined_tag_query, tagnameParameter);
                         
                            if (IsDistinct.Item1 && IsDistinct.Item2.Rows.Count == 1)
                            {
                                List<string> tracenamelist = new List<string>();

                                foreach (var tagname in ListviewSelectedItems)
                                {
                                    string tag_Details = "Select ScriptMappingID,ScriptType,DataStartTime,DataEndTime,Average,Minimum,Maximum from DataPointsMappingTable where Tagname= @tagname";
                                    tagnameParameter = new Dictionary<string, string>();
                                    tagnameParameter.Add("@tagname", tagname);

                                    var tag_Details_datatable = dbread_method(tag_Details, tagnameParameter);
                                    if (tag_Details_datatable.Item1)
                                    {
                                        DataTableReader dataTableReader = tag_Details_datatable.Item2.CreateDataReader();

                                        while (dataTableReader.Read())
                                        {
                                            string get_id = string.Empty;

                                            if (dataTableReader[0] != System.DBNull.Value && dataTableReader[0].ToString() != string.Empty)
                                            {
                                                get_id = dataTableReader[0].ToString();
                                                //sql_query_list.Add("Select Iteration as time,ScriptDatapoint as value,'" + tagname + "'as metric from DataPointsTable where ScriptMappingID =" + get_id + " ORDER BY DatapointsID");
                                                if (Properties.Settings.Default.GrafanaPlotly)
                                                    sql_query_list.Add("from DataPointsTable where ScriptMappingID =" + get_id + " ORDER BY DatapointsID");
                                                else
                                                    sql_query_list.Add("Select Iteration as time,ScriptDatapoint as value,'" + tagname + "'as metric from DataPointsTable where ScriptMappingID =" + get_id + " ORDER BY DatapointsID");

                                            }
                                            if (dataTableReader[1] != System.DBNull.Value && dataTableReader[1].ToString() != string.Empty)
                                            {
                                                Script_Name = dataTableReader[1].ToString();

                                                if (Script_Name == "Deploy_Total")
                                                {
                                                    Script_Name = "Deploy_Time";
                                                }
                                            }
                                            
                                            string tracename = tagname;

                                            if (Properties.Settings.Default.GrafanaPlotly)
                                            {
                                                tracename += "(" + GetTraceName(dataTableReader, get_id) + ")";
                                            }

                                            tracenamelist.Add(tracename);
                                        }
                                    }
                                    else
                                    {
                                        Qmsg.Show("Error in sqlquery for Tagname" + tag_Details_datatable.Item3.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                                    }
                                }

                                tag_Inputs.Add(Script_Name, new Tuple<List<string>, string, List<string>>(sql_query_list, Script_Name, tracenamelist));

                                if (tag_Inputs.Count > 0)
                                {
                                    var Dashboard = Create_Dashboard(tag_Inputs, Dashboard_Title);
                                }
                                else
                                {
                                    Qmsg.Show("Unable to collect datas from Grafana DB", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                                }
                            }
                            else
                            {
                                Qmsg.Show("Please select same script name measurement data for comparison", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                            }
                        }
                        else
                        {
                            Qmsg.Show("Please select Tag to view Graph in Grafana", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                        }

                    }
                    else
                    { Qmsg.Show("Please locate Browser Application path in preferences to view Grafana", "Error", MessageBoxButton.OK, MessageBoxImage.Error); }
                }                
                else { Qmsg.Show("Please enter Grafana Dashboard Title", "Error", MessageBoxButton.OK, MessageBoxImage.Error); }

            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message.ToString());
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif               
            }
        }

        private void DashboardTitle_txtbox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            try
            {
                System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex(@"[\\""]");
                if (regex.IsMatch(e.Text))
                    e.Handled = true;
            }
            catch (Exception ex)
            {               
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif               
            }
        }

        private void DashboardTitle_txtbox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            try
            {

                if (e.Key == Key.V && Keyboard.Modifiers == ModifierKeys.Control)
                {    
                    System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex(@"[\\""]");

                    if (regex.IsMatch(Clipboard.GetText()))
                        e.Handled = true;

                  base.OnPreviewKeyDown(e);
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

    public class JsonGrafana
    {
        public string status { get; set; }
        public string uid { get; set; }
        public string url { get; set; }
    }

    public class JsonGrafanaDelete
    {
        public string title { get; set; }
    }
}
