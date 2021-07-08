namespace QSC_Test_Automation
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Data;
    using System.Data.SqlClient;
    using System.Linq;
    using System.Text;
    using System.Windows;
    using System.IO;
    using System.Windows.Controls;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.Threading;
    using System.Text.RegularExpressions;
    using System.Collections;
    using System.Windows.Input;
    class DBConnection
    {
        private SqlConnection connect = null;

        public DBConnection()
        {
            CultureInfo ci = CultureInfo.CreateSpecificCulture(CultureInfo.CurrentCulture.Name);
            ci.DateTimeFormat.ShortDatePattern = "MM-dd-yyyy";
            Thread.CurrentThread.CurrentCulture = ci;
        }

        public bool DataBaseConnection()
        {
            try
            {
                using (this.connect = new SqlConnection(ConfigurationManager.ConnectionStrings["ConString"].ToString()))
                {
                    this.connect.Open();
                    string query = "IF NOT EXISTS (SELECT name FROM master.dbo.sysdatabases WHERE name = '" + QatConstants.DbDatabaseName + "') CREATE DATABASE " + QatConstants.DbDatabaseName + "";
                    SqlCommand cmd = new SqlCommand(query, this.connect);
                    int value = Convert.ToInt32(cmd.ExecuteScalar());
                    return true;
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show("SQL Exception\n No Server available for the supplied credentials\n Check SQL-Server Sourcename, Database Username & Password \n " + ex.Message, "QAT Error Code - EC02001", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif

                //MessageBox.Show("Exception\n " + ex.Message, "QAT Error Code - EC02002", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        public SqlConnection CreateConnection()
        {
            try
            {
                this.connect = new SqlConnection(ConfigurationManager.ConnectionStrings["ConString"].ToString() + ";Database=" + QatConstants.DbDatabaseName + "");
            }
            catch (SqlException ex)
            {
                MessageBox.Show("SQL Exception\n " + ex.Message, "QAT Error Code - EC02003", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif

                //MessageBox.Show("Exception\n " + ex.Message, "QAT Error Code - EC02004", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return this.connect;
        }

        public void OpenConnection()
        {
            try
            {
                if(connect.State != ConnectionState.Open)
                    this.connect.Open();
            }
            catch (SqlException ex)
            {
                MessageBox.Show("SQL Exception\n " + ex.Message, "QAT Error Code - EC02005", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif

                //MessageBox.Show("Exception\n " + ex.Message, "QAT Error Code - EC02006", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void CloseConnection()
        {
            try
            {
                if (connect.State != ConnectionState.Closed)
                    this.connect.Close();
            }
            catch (SqlException ex)
            {
                MessageBox.Show("SQL Exception\n " + ex.Message, "QAT Error Code - EC02007", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif

                //MessageBox.Show("Exception\n " + ex.Message, "QAT Error Code - EC02008", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public DataTable SendCommand_Toreceive(string cmdtext)
        {
            DataTable datatable = null;
            SqlDataAdapter adap = null;
            DataSet ds = new DataSet();
            try
            {
                datatable = new DataTable();
                datatable.Clear();
                adap = new SqlDataAdapter(cmdtext, CreateConnection());
                OpenConnection();
                adap.Fill(datatable);
                CloseConnection();
            }
            catch (Exception ex)
            {
                                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif

                //MessageBox.Show("Exception\n " + ex.Message, "QAT Error Code - EC10001", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return datatable;
        }


        public Dictionary<string, int> ReadFilterItemList(string filterItem)
        {
            Dictionary<string, int> FilterItemList = new Dictionary<string, int>(StringComparer.CurrentCultureIgnoreCase);
            try
            {

                string query = null;
                                   
                CreateConnection();
                OpenConnection();
                query = "select " + filterItem + " from "+ QatConstants.DbTestSuiteTable + " Union All select " + filterItem + " from " +QatConstants.DbTestPlanTable+ " Union All select " + filterItem + " from "+ QatConstants.DbTestCaseTable;
                SqlDataAdapter dataAdapt = new SqlDataAdapter(query, connect);
                DataTable dataTable = new DataTable();
                dataAdapt.Fill(dataTable);

                for (int i = 0; i < dataTable.Rows.Count;i++)
                { 

                    if(dataTable.Rows[i][0].ToString() != String.Empty )
                    {
                        if (!FilterItemList.ContainsKey(dataTable.Rows[i][0].ToString()))
                        {
                            FilterItemList.Add(dataTable.Rows[i][0].ToString(),1);
                        }
                        else
                        {
                            FilterItemList[dataTable.Rows[i][0].ToString()] = FilterItemList[dataTable.Rows[i][0].ToString()] + 1;
                        }
                           
                    }
          
                }

                return FilterItemList;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Exception\n Reading record failed\n" + ex.Message, "QAT Error Code - EC02010", MessageBoxButton.OK, MessageBoxImage.Error);
                return new Dictionary<string, int>();
            }
            finally
            {
                CloseConnection();
            }
            ////return FilterItemList;
        }
        // Set parentPrimaryKey to 0 to get all values from childTableName
        public List<TreeViewExplorer> ReadTreeTable(string childTableName, int parentPrimaryKey, Test_Execution executionWindow, Test_Designer designerWindow)
        {
            List<TreeViewExplorer> tableValues = new List<TreeViewExplorer>();
            try
            {
                string query = null;
                string parentLinkTable = null;
                string childIDColumn = null;
                string parentIDColumn = null;
                string primaryLinkKeyColumn = null;

                DataTable dataTable = new DataTable();
                DataTableReader dataTableReader = null;

                CreateConnection();
                OpenConnection();

                if (childTableName == QatConstants.DbTestPlanTable)
                {
                    parentLinkTable = QatConstants.DbTestSuiteTestPlanLinkTable;
                    parentIDColumn = QatConstants.DbTestSuiteLinkTableID;
                    childIDColumn = QatConstants.DbTestPlanLinkTableID;
                    primaryLinkKeyColumn = QatConstants.DbTestSuiteLinkTablePrimaryKeyColumn;
                }
                else if (childTableName == QatConstants.DbTestCaseTable)
                {
                    parentLinkTable = QatConstants.DbTestPlanTestCaseLinkTable;
                    parentIDColumn = QatConstants.DbTestPlanLinkTableID;
                    childIDColumn = QatConstants.DbTestCaseLinkTableID;
                    primaryLinkKeyColumn = QatConstants.DbTestPlanLinkTablePrimaryKeyColumn;
                }
                else if (childTableName == QatConstants.DbTestSuiteTable)
                {
                    parentLinkTable = QatConstants.DbTestSuiteTestPlanLinkTable;
                    parentIDColumn = QatConstants.DbTestPlanLinkTableID;
                    childIDColumn = QatConstants.DbTestSuiteLinkTableID;
                    primaryLinkKeyColumn = QatConstants.DbTestSuiteLinkTablePrimaryKeyColumn;
                }
                //else if (childTableName == QatConstants.DbTestScriptTable)
                //{
                //    parentLinkTable = QatConstants.DbTestSuiteTestPlanLinkTable;
                //    parentIDColumn = QatConstants.DbTestPlanLinkTableID;
                //    childIDColumn = QatConstants.DbTestSuiteLinkTableID;
                //}

                if (parentPrimaryKey == 0)
                {
                    query = "select * from " + childTableName;

                    tableValues.AddRange(GetTreeValues(query, childTableName, executionWindow, designerWindow));
                }
                else
                {
                    //query = "select * from " + childTableName + " as c join " + parentLinkTable + " as p on p." + childIDColumn + " = c." + childTableName + "ID where p." + parentIDColumn + " = " + parentPrimaryKey.ToString();

                    query = "select " + childIDColumn + " from  " + parentLinkTable + " where " + parentIDColumn + " = " + parentPrimaryKey.ToString() +" order by "+ primaryLinkKeyColumn;
                    SqlDataAdapter dataAdapt = new SqlDataAdapter(query, connect);
                    dataAdapt.Fill(dataTable);
                    dataTableReader = dataTable.CreateDataReader();

                    if (dataTableReader.HasRows)
                    {
                        while (dataTableReader.Read())
                        {
                            if (dataTableReader[0] != System.DBNull.Value)
                            {
                                string childID = dataTableReader.GetValue(0).ToString();
                                
                                query = "select * from " + childTableName + " where " + childTableName + "ID in (" + childID + ")";

                                tableValues.AddRange(GetTreeValues(query, childTableName, executionWindow, designerWindow));
                            }
                        }
                    }
                }

                CloseConnection();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Exception\n Reading record failed\n" + ex.Message, "QAT Error Code - EC02010", MessageBoxButton.OK, MessageBoxImage.Error);
                return new List<TreeViewExplorer>();
            }

            return tableValues;
        }
        public Dictionary<int, ArrayList> ReadTreeTable_executionWindow(string childTableName, int parentPrimaryKey, Test_Execution executionWindow)
        {
            Dictionary<int, ArrayList> tableValues = new Dictionary<int, ArrayList>();
            try
            {
                string query = null;
                string parentLinkTable = null;
                string childIDColumn = null;
                string parentIDColumn = null;

                DataTable dataTable = new DataTable();
                //DataTableReader dataTableReader = null;

                CreateConnection();
                OpenConnection();

                if (childTableName == QatConstants.DbTestPlanTable)
                {
                    parentLinkTable = QatConstants.DbTestSuiteTestPlanLinkTable;
                    parentIDColumn = QatConstants.DbTestSuiteLinkTableID;
                    childIDColumn = QatConstants.DbTestPlanLinkTableID;
                }
                else if (childTableName == QatConstants.DbTestCaseTable)
                {
                    parentLinkTable = QatConstants.DbTestPlanTestCaseLinkTable;
                    parentIDColumn = QatConstants.DbTestPlanLinkTableID;
                    childIDColumn = QatConstants.DbTestCaseLinkTableID;
                }
                else if (childTableName == QatConstants.DbTestSuiteTable)
                {
                    parentLinkTable = QatConstants.DbTestSuiteTestPlanLinkTable;
                    parentIDColumn = QatConstants.DbTestPlanLinkTableID;
                    childIDColumn = QatConstants.DbTestSuiteLinkTableID;
                }

                if (parentPrimaryKey == 0)
                {
                    query = "select * from " + childTableName;

                    tableValues=(GetTreeValues_executionWindow(query, childTableName, executionWindow));
                }
               
                CloseConnection();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Exception\n Reading record failed\n" + ex.Message, "QAT Error Code - EC02010", MessageBoxButton.OK, MessageBoxImage.Error);
                return new Dictionary<int, ArrayList>();
            }

            return tableValues;
        }
      

        public List<TreeViewExplorer> ReadCategoryTreeTable(string childTableName, string parentName, Test_Execution executionWindow, Test_Designer designerWindow)
        {
            List<TreeViewExplorer> tableValues = new List<TreeViewExplorer>();

            try
            {
                DataTable dataTable = new DataTable();
                CreateConnection();
                OpenConnection();

                if (parentName == QatConstants.TveDesignerOtherCatHeader)
                    parentName = string.Empty;

                string query = "select * from " + childTableName + " where Category = @CategoryName";

                List<string> parameter = new List<string> { "@CategoryName"};
                List<string> parameterValue = new List<string> { parentName };


                tableValues.AddRange(GetTreeWithChildCountValues(query, childTableName, parameter, parameterValue, executionWindow, designerWindow));

                CloseConnection();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Exception\n Reading record failed\n" + ex.Message, "QAT Error Code - EC02010", MessageBoxButton.OK, MessageBoxImage.Error);
                return new List<TreeViewExplorer>();
            }

            return tableValues;
        }

        private List<TreeViewExplorer> GetTreeWithChildCountValues(string query, string childTableName, List<string> parameter, List<string> parameterVal, Test_Execution executionWindow, Test_Designer designerWindow)
        {
            List<TreeViewExplorer> tableValues = new List<TreeViewExplorer>();

            try
            {
                DataTable dataTable = new DataTable();
                SqlCommand cmd = new SqlCommand(query, connect);

                for (int i = 0; i < parameter.Count; i++)
                {
                    cmd.Parameters.AddWithValue(parameter[i], parameterVal[i]);
                }

                SqlDataAdapter dataAdapter = new SqlDataAdapter(cmd);
                dataAdapter.Fill(dataTable);
                DataTableReader dataTableReader = dataTable.CreateDataReader();

                if (dataTableReader.HasRows)
                {
                    while (dataTableReader.Read())
                    {
                        string createdby = string.Empty;
                        DateTime? createdon = null;
                        DateTime? modifiedon = null;
                        DateTime? importedon = null;
                        string modifiedby = string.Empty;
                        string summary = string.Empty;
                        string category = string.Empty;
                        string importedby = string.Empty;
                        int childcountview = 0;
                        bool hasDesign = true;

                        if (childTableName != QatConstants.DbTestCaseTable)
                        {
                            if (dataTableReader[2] != System.DBNull.Value)
                                createdon = dataTableReader.GetDateTime(2);
                            if (dataTableReader[3] != System.DBNull.Value)
                                createdby = dataTableReader.GetString(3);
                            if (dataTableReader[4] != System.DBNull.Value)
                                modifiedon = dataTableReader.GetDateTime(4);
                            if (dataTableReader[5] != System.DBNull.Value)
                                modifiedby = dataTableReader.GetString(5);
                            if (dataTableReader[6] != System.DBNull.Value)
                                summary = dataTableReader.GetString(6);
                            if (dataTableReader[7] != System.DBNull.Value)
                                category = dataTableReader.GetString(7);
                            if (childTableName == QatConstants.DbTestPlanTable)
                            {
                                if (dataTableReader[11] != System.DBNull.Value)
                                    importedon = dataTableReader.GetDateTime(11);
                                if (dataTableReader[12] != System.DBNull.Value)
                                    importedby = dataTableReader.GetString(12);
                                if (dataTableReader[13] != System.DBNull.Value)
                                    childcountview = dataTableReader.GetInt32(13);
                                if (dataTableReader[14] != System.DBNull.Value)
                                    hasDesign = Convert.ToBoolean(dataTableReader.GetString(14));
                            }
                            if (childTableName == QatConstants.DbTestSuiteTable)
                            {
                                ////if (dataTableReader[9] != System.DBNull.Value)
                                //    importedon = dataTableReader.GetDateTime(9);
                                ////if (dataTableReader[10] != System.DBNull.Value)
                                //    importedby = dataTableReader.GetString(10);
                                if (dataTableReader[9] != System.DBNull.Value)
                                    childcountview = dataTableReader.GetInt32(9);
                                importedon = null;
                                importedby = null;
                            }
                        }

                        if (childTableName == QatConstants.DbTestCaseTable)
                        {
                            if (dataTableReader[3] != System.DBNull.Value)
                                createdon = dataTableReader.GetDateTime(3);
                            if (dataTableReader[4] != System.DBNull.Value)
                                createdby = dataTableReader.GetString(4);
                            if (dataTableReader[5] != System.DBNull.Value)
                                modifiedon = dataTableReader.GetDateTime(5);
                            if (dataTableReader[6] != System.DBNull.Value)
                                modifiedby = dataTableReader.GetString(6);
                            if (dataTableReader[7] != System.DBNull.Value)
                                summary = dataTableReader.GetString(7);
                            if (dataTableReader[8] != System.DBNull.Value)
                                category = dataTableReader.GetString(8);
                            if (dataTableReader[10] != System.DBNull.Value)
                                importedon = dataTableReader.GetDateTime(10);
                            if (dataTableReader[11] != System.DBNull.Value)
                                importedby = dataTableReader.GetString(11);
                            if (dataTableReader[12] != System.DBNull.Value)
                                childcountview = dataTableReader.GetInt32(12);

                        }

                        TreeViewExplorer tableRow = new TreeViewExplorer(dataTableReader.GetInt32(0), dataTableReader.GetString(1).ToString(), childTableName, executionWindow, designerWindow, createdon, createdby, modifiedon, modifiedby, summary, category, importedon, importedby, childcountview, hasDesign);
                        tableRow.ChildrenCountViewIsEnabled = true;
                        tableValues.Add(tableRow);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Exception\n Reading record failed\n" + ex.Message, "QAT Error Code - EC02010", MessageBoxButton.OK, MessageBoxImage.Error);
                return new List<TreeViewExplorer>();
            }

            return tableValues;
        }
        
        private List<TreeViewExplorer> GetTreeValues(string query,string childTableName, Test_Execution executionWindow, Test_Designer designerWindow)
        {
            List<TreeViewExplorer> tableValues = new List<TreeViewExplorer>();

            try
            {
                DataTable dataTable = new DataTable();
                SqlDataAdapter dataAdapter = new SqlDataAdapter(query, connect);
                dataAdapter.Fill(dataTable);
                DataTableReader dataTableReader = dataTable.CreateDataReader();

                if (dataTableReader.HasRows)
                {
                    while (dataTableReader.Read())
                    {
                        string createdby = string.Empty;
                        DateTime? createdon = null;
                        DateTime? modifiedon = null;
                        DateTime? importedon = null;
                        string modifiedby = string.Empty;
                        string summary = string.Empty;
                        string category = string.Empty;
                        string importedby = string.Empty;
                        int childcountview = 0;
                        bool hasDesign = true;

                        if (childTableName != QatConstants.DbTestCaseTable)
                        {
                            if (dataTableReader[2] != System.DBNull.Value)
                                createdon = dataTableReader.GetDateTime(2);
                            if (dataTableReader[3] != System.DBNull.Value)
                                createdby = dataTableReader.GetString(3);
                            if (dataTableReader[4] != System.DBNull.Value)
                                modifiedon = dataTableReader.GetDateTime(4);
                            if (dataTableReader[5] != System.DBNull.Value)
                                modifiedby = dataTableReader.GetString(5);
                            if (dataTableReader[6] != System.DBNull.Value)
                                summary = dataTableReader.GetString(6);
                            if (dataTableReader[7] != System.DBNull.Value)
                                category = dataTableReader.GetString(7);
                            if (childTableName == QatConstants.DbTestPlanTable)
                            {
                                if (dataTableReader[11] != System.DBNull.Value)
                                    importedon = dataTableReader.GetDateTime(11);
                                if (dataTableReader[12] != System.DBNull.Value)
                                    importedby = dataTableReader.GetString(12);
                                if (dataTableReader[13] != System.DBNull.Value)
                                    childcountview = dataTableReader.GetInt32(13);
                                if (dataTableReader[14] != System.DBNull.Value)
                                    hasDesign = Convert.ToBoolean(dataTableReader.GetString(14));
                            }
                            if (childTableName == QatConstants.DbTestSuiteTable)
                            {
                                ////if (dataTableReader[9] != System.DBNull.Value)
                                //    importedon = dataTableReader.GetDateTime(9);
                                ////if (dataTableReader[10] != System.DBNull.Value)
                                //    importedby = dataTableReader.GetString(10);
                                if (dataTableReader[9] != System.DBNull.Value)
                                    childcountview = dataTableReader.GetInt32(9);
                                importedon = null;
                                importedby = null;
                            }
                        }

                        if (childTableName == QatConstants.DbTestCaseTable)
                        {
                            if (dataTableReader[3] != System.DBNull.Value)
                                createdon = dataTableReader.GetDateTime(3);
                            if (dataTableReader[4] != System.DBNull.Value)
                                createdby = dataTableReader.GetString(4);
                            if (dataTableReader[5] != System.DBNull.Value)
                                modifiedon = dataTableReader.GetDateTime(5);
                            if (dataTableReader[6] != System.DBNull.Value)
                                modifiedby = dataTableReader.GetString(6);
                            if (dataTableReader[7] != System.DBNull.Value)
                                summary = dataTableReader.GetString(7);
                            if (dataTableReader[8] != System.DBNull.Value)
                                category = dataTableReader.GetString(8);
                            if (dataTableReader[10] != System.DBNull.Value)
                                importedon = dataTableReader.GetDateTime(10);
                            if (dataTableReader[11] != System.DBNull.Value)
                                importedby = dataTableReader.GetString(11);
                            if (dataTableReader[12] != System.DBNull.Value)
                                childcountview = dataTableReader.GetInt32(12);
                        
                        }

                        TreeViewExplorer tableRow = new TreeViewExplorer(dataTableReader.GetInt32(0), dataTableReader.GetString(1).ToString(), childTableName, executionWindow, designerWindow, createdon, createdby, modifiedon, modifiedby, summary, category, importedon, importedby, childcountview, hasDesign);
                        //tableRow.ChildrenCountViewIsEnabled = true;
                        tableValues.Add(tableRow);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Exception\n Reading record failed\n" + ex.Message, "QAT Error Code - EC02010", MessageBoxButton.OK, MessageBoxImage.Error);
                return new List<TreeViewExplorer>();
            }

            return tableValues;
        }
        private Dictionary<int, ArrayList> GetTreeValues_executionWindow(string query, string childTableName, Test_Execution executionWindow)
        {
            Dictionary<int, ArrayList> tableValues = new Dictionary<int, ArrayList>();

            try
            {
                DataTable dataTable = new DataTable();
                SqlDataAdapter dataAdapter = new SqlDataAdapter(query, connect);
                dataAdapter.Fill(dataTable);
                DataTableReader dataTableReader = dataTable.CreateDataReader();

                if (dataTableReader.HasRows)
                {
                    while (dataTableReader.Read())
                    {
                        string createdby = string.Empty;
                        DateTime? createdon = null;
                        DateTime? modifiedon = null;
                        DateTime? importedon = null;
                        string modifiedby = string.Empty;
                        string summary = string.Empty;
                        string category = string.Empty;
                        string importedby = string.Empty;
                        int childcountview = 0;
                        bool hasDesign = true;

                        if (childTableName != QatConstants.DbTestCaseTable)
                        {
                           
                            if (dataTableReader[2] != System.DBNull.Value)
                                createdon = dataTableReader.GetDateTime(2);
                            if (dataTableReader[3] != System.DBNull.Value)
                                createdby = dataTableReader.GetString(3);
                            if (dataTableReader[4] != System.DBNull.Value)
                                modifiedon = dataTableReader.GetDateTime(4);
                            if (dataTableReader[5] != System.DBNull.Value)
                                modifiedby = dataTableReader.GetString(5);
                            if (dataTableReader[6] != System.DBNull.Value)
                                summary = dataTableReader.GetString(6);
                            if (dataTableReader[7] != System.DBNull.Value)
                                category = dataTableReader.GetString(7);
                            if (childTableName == QatConstants.DbTestPlanTable)
                            {
                                if (dataTableReader[11] != System.DBNull.Value)
                                    importedon = dataTableReader.GetDateTime(11);
                                if (dataTableReader[12] != System.DBNull.Value)
                                    importedby = dataTableReader.GetString(12);
                                if (dataTableReader[13] != System.DBNull.Value)
                                    childcountview = dataTableReader.GetInt32(13);
                                if (dataTableReader[14] != System.DBNull.Value)
                                    hasDesign = Convert.ToBoolean(dataTableReader.GetString(14));
                            }
                            if (childTableName == QatConstants.DbTestSuiteTable)
                            {
                                
                                if (dataTableReader[9] != System.DBNull.Value)
                                    childcountview = dataTableReader.GetInt32(9);
                                importedon = null;
                                importedby = null;
                            }
                        }

                        if (childTableName == QatConstants.DbTestCaseTable)
                        {
                            if (dataTableReader[3] != System.DBNull.Value)
                                createdon = dataTableReader.GetDateTime(3);
                            if (dataTableReader[4] != System.DBNull.Value)
                                createdby = dataTableReader.GetString(4);
                            if (dataTableReader[5] != System.DBNull.Value)
                                modifiedon = dataTableReader.GetDateTime(5);
                            if (dataTableReader[6] != System.DBNull.Value)
                                modifiedby = dataTableReader.GetString(6);
                            if (dataTableReader[7] != System.DBNull.Value)
                                summary = dataTableReader.GetString(7);
                            if (dataTableReader[8] != System.DBNull.Value)
                                category = dataTableReader.GetString(8);
                            if (dataTableReader[10] != System.DBNull.Value)
                                importedon = dataTableReader.GetDateTime(10);
                            if (dataTableReader[11] != System.DBNull.Value)
                                importedby = dataTableReader.GetString(11);
                            if (dataTableReader[12] != System.DBNull.Value)
                                childcountview = dataTableReader.GetInt32(12);

                        }
                       
                        ArrayList tableRow = new ArrayList() { dataTableReader.GetInt32(0), dataTableReader.GetString(1).ToString(), childTableName, executionWindow, null, createdon, createdby, modifiedon, modifiedby, summary, category, importedon, importedby, childcountview, hasDesign };
                       
                        tableValues.Add(dataTableReader.GetInt32(0),tableRow);
                        

                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Exception\n Reading record failed\n" + ex.Message, "QAT Error Code - EC02010", MessageBoxButton.OK, MessageBoxImage.Error);
                return new Dictionary<int, ArrayList>();
            }

            return tableValues;
        }
     

        
        
        public List<TreeViewExplorer> ReadTreeTableTC(string childTableName, int parentPrimaryKey, Test_Execution executionWindow, Test_Designer designerWindow)
        {
            List<TreeViewExplorer> tableValues = new List<TreeViewExplorer>();
            try
            {
                string query = null;
                string parentLinkTable = null;
                string childIDColumn = null;
                string parentIDColumn = null;

                DataTable dataTable = new DataTable();
                DataTableReader dataTableReader = null;
                dataTable.Clear();

                CreateConnection();
                OpenConnection();

                if (childTableName == QatConstants.DbTestPlanTable)
                {
                    parentLinkTable = QatConstants.DbTestPlanTestCaseLinkTable;
                    parentIDColumn = QatConstants.DbTestCaseLinkTableID;
                    childIDColumn = QatConstants.DbTestPlanLinkTableID;
                }

                if (parentPrimaryKey == 0)
                    query = "select * from " + childTableName;
                else
                    query = "select * from " + childTableName + " as c join " + parentLinkTable + " as p on p." + childIDColumn + " = c." + childTableName + "ID where p." + parentIDColumn + " = " + parentPrimaryKey.ToString();

                SqlDataAdapter dataAdapter = new SqlDataAdapter(query, connect);
                dataAdapter.Fill(dataTable);
                dataTableReader = dataTable.CreateDataReader();

                if (dataTableReader.HasRows)
                {
                    while (dataTableReader.Read())
                    {
                        TreeViewExplorer tableRow = new TreeViewExplorer(dataTableReader.GetInt32(0), dataTableReader.GetString(1).ToString(), childTableName, executionWindow, designerWindow,null,null,null,null,null, null,null,null,0,true);
                        tableValues.Add(tableRow);
                    }
                }

                CloseConnection();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Exception\n Reading record failed\n" + ex.Message, "QAT Error Code - EC02010", MessageBoxButton.OK, MessageBoxImage.Error);
                return new List<TreeViewExplorer>();
            }

            return tableValues;
        }

        public List<TreeViewExplorer> ReadTestCaseLinkedToTestPlan(TreeViewExplorer testPlanTreeViewExplorer)
        {
            List<TreeViewExplorer> tableValues = new List<TreeViewExplorer>();
            try
            {
                string query = null;

                DataTable dataTable = new DataTable();
                DataTableReader dataTableReader = null;
                dataTable.Clear();

                CreateConnection();
                OpenConnection();

                query = "select * from TestCase where TPID = '" + testPlanTreeViewExplorer.ItemKey + "'";

                SqlDataAdapter dataAdapter = new SqlDataAdapter(query, connect);
                dataAdapter.Fill(dataTable);
                dataTableReader = dataTable.CreateDataReader();

                if (dataTableReader.HasRows)
                {
                    while (dataTableReader.Read())
                    {
                        TreeViewExplorer tableRow = new TreeViewExplorer(dataTableReader.GetInt32(0), dataTableReader.GetString(1).ToString(), QatConstants.DbTestCaseTable, null, null, null, null, null, null, null, null,null,null,0,true);
                        tableValues.Add(tableRow);
                    }
                }

                CloseConnection();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Exception\n Reading record failed\n" + ex.Message, "QAT Error Code - EC02010", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return tableValues;
        }

        public TreeViewExplorer CopyTreeItem(TreeViewExplorer sourceTableItem)
        {
            TreeViewExplorer createdTableItem = null;

            try
            {

                if (String.Equals(sourceTableItem.ItemType, QatConstants.DbTestCaseTable))
                {
                    TestCaseItem sourceTestCaseItem = new TestCaseItem(sourceTableItem, false);
                    if (sourceTestCaseItem.TestPlanSelected == null)
                    {
                        MessageBox.Show("Tase Case \"" + sourceTableItem.ItemName + "\" is not copied as it is not associated with Test Plan", "Warning", MessageBoxButton.OK, MessageBoxImage.Error);
                        return null;
                    }
                    else
                    {
                        sourceTestCaseItem.TestItemName = CreateCopyItemName(sourceTableItem.ItemName, sourceTableItem.ItemType, QatConstants.DbTestCaseNameColumn);
                        sourceTestCaseItem.TestCaseID = 0;
                        sourceTestCaseItem.IsNewTestCase = true;

                        WriteTestCaseItemToDB(sourceTestCaseItem, sourceTableItem);

                        SendCommand_Toreceive("update TestCase set EditedBy= '" + string.Empty + "' where TestCaseID = '" + sourceTestCaseItem.TestCaseID + "'");

                        //sourceTableItem.Parent.ChildrenCountForView += 1;

                        return new TreeViewExplorer(sourceTestCaseItem.TestCaseID, sourceTestCaseItem.TestItemName, QatConstants.DbTestCaseTable, sourceTableItem.ExecutionWindow, sourceTableItem.DesignerWindow, sourceTestCaseItem.Createdon, sourceTestCaseItem.Createdby, null, null, null, sourceTestCaseItem.Category, null,null,0, true);
                    }
                }

                CreateConnection();
                OpenConnection();

                createdTableItem = CreateCopyOfTreeItem(sourceTableItem);

                if (createdTableItem == null)
                    return createdTableItem;


                if (String.Equals(createdTableItem.ItemType, QatConstants.DbTestSuiteTable))
                {
                    CreateCopyOfLinkedChildTreeItem(createdTableItem, sourceTableItem);
                }
                else if (String.Equals(createdTableItem.ItemType, QatConstants.DbTestPlanTable))
                {
                    CreateCopyOfLinkedChildTreeItem(createdTableItem, sourceTableItem);
                    CreateCopyOfDesign(createdTableItem, sourceTableItem);
                }

                CloseConnection();

                //sourceTableItem.Parent.ChildrenCountForView += 1;

            }
            catch (Exception ex)
            {
                MessageBox.Show("Exception\n Creating record failed\n" + ex.Message, "QAT Error Code - EC02012", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }

            return createdTableItem;
        }

        private TreeViewExplorer CreateCopyOfTreeItem(TreeViewExplorer sourceTableItem)
        {
            TreeViewExplorer createdTableItem = null;

            try
            {
                List<string> similarNames = new List<string>();
                string itemNewName = null;

                string query = null;
                string itemTableName = null;
                string itemNameColumn = null;
                string itemIDColumn = null;
                string newDesignName = null;
                string olddesignName = null;
                string PreferencesServerPath = QatConstants.QATServerPath + "\\Designs" + "\\";

                if (sourceTableItem.ItemType == QatConstants.DbTestSuiteTable)
                {
                    itemTableName = QatConstants.DbTestSuiteTable;
                    itemNameColumn = QatConstants.DbTestSuiteNameColumn;
                    itemIDColumn = QatConstants.DbTestSuiteLinkTableID;
                }
                else if (sourceTableItem.ItemType == QatConstants.DbTestPlanTable)
                {
                    itemTableName = QatConstants.DbTestPlanTable;
                    itemNameColumn = QatConstants.DbTestPlanNameColumn;
                    itemIDColumn = QatConstants.DbTestPlanLinkTableID;
                }
                else if (sourceTableItem.ItemType == QatConstants.DbTestCaseTable)
                {
                    itemTableName = QatConstants.DbTestCaseTable;
                    itemNameColumn = QatConstants.DbTestCaseNameColumn;
                    itemIDColumn = QatConstants.DbTestPlanLinkTableID;
                }
                else
                {
                    return null;
                }                    

                itemNewName = CreateCopyItemName(sourceTableItem.ItemName, sourceTableItem.ItemType, itemNameColumn);
                QatMessageBox QatMessageBox = new QatMessageBox(sourceTableItem.DesignerWindow);

                // design is able to create in the design server path or not
                if (sourceTableItem.ItemType == QatConstants.DbTestPlanTable)
                {                    
                    int Designid = 0;                   
                    query = "select " + QatConstants.DbTestDesignLinkTableID + " from " + QatConstants.DbTestPlanDesignLinkTable + " where " + QatConstants.DbTestPlanLinkTableID + "=('" + sourceTableItem.ItemKey + "')";
                    DataTable dataTable1 = new DataTable();
                    DataTableReader dataTableReader1 = null;
                    SqlDataAdapter dataAdapter = new SqlDataAdapter(query, connect);
                    dataAdapter.Fill(dataTable1);
                    dataTableReader1 = dataTable1.CreateDataReader();
                    if (dataTableReader1.HasRows)
                    {
                        if (dataTableReader1.Read())
                            Designid = dataTableReader1.GetInt32(0);
                    }
                    if(Designid!=0)
                    {
                        query = "Select Designname FROM designtable where DesignID='" + Designid + "'";
                        dataTable1 = new DataTable();
                        dataTableReader1 = null;
                        dataAdapter = new SqlDataAdapter(query, connect);
                        dataAdapter.Fill(dataTable1);
                        dataTableReader1 = dataTable1.CreateDataReader();
                        if (dataTableReader1.HasRows)
                        {
                            if (dataTableReader1.Read())
                            {
                                olddesignName = dataTableReader1.GetString(0).ToString();
                            }
                        }
                    }
                  
                    newDesignName = olddesignName.Substring(sourceTableItem.ItemName.Length + 6);
                    if (newDesignName.IndexOf("_") > 0)
                        newDesignName = newDesignName.Substring(newDesignName.IndexOf("_"));
                    //designname = designname.Remove(0, sourceTableItem.ItemName.Length + 4);
                    //designname = "QAT_" + itemNewName.Trim() + designname;
                    newDesignName = "QAT_" + itemNewName.Trim() + "_V1" + newDesignName;                        
                    newDesignName = Path.Combine(PreferencesServerPath, newDesignName);
                    try
                    {
                        FileInfo sample = new FileInfo(newDesignName);
                    }
                    catch
                    {
                        if (Mouse.OverrideCursor != null)
                            Mouse.OverrideCursor = Cursors.Arrow;
                        QatMessageBox.Show("Testplan/DesignName is too long.\n" + "\n " + olddesignName + "\n" + "\nPlease rename and copy again.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return null;
                    }
                }



                Tuple<bool,bool, string> deploySettings = new Tuple<bool,bool, string> (false,true, string.Empty);
                if (itemTableName == "TestPlan")
                {
                    deploySettings = GetDeployItems(sourceTableItem.ItemName, sourceTableItem.ItemType, itemNameColumn);                    
                }

                //TreeViewExplorer propertiesTreeItem = ReadTreeTableChildrenitem(sourceTableItem.ItemType, sourceTableItem.ItemKey);

                DateTime? creationTime = DateTime.Now;
                string creatorName = Properties.Settings.Default.TesterName.ToString().TrimEnd();
                //string categoryName = Properties.Settings.Default.Category.ToString().TrimEnd();
                string categoryName = sourceTableItem.Category;

                if (itemTableName == "TestPlan")
                    query = "Insert into " + itemTableName + " Values (@itemName,@Createdate,@CreateBy,'" + null + "','" + string.Empty + "','" + string.Empty + "',@categoryvalue,'" + deploySettings.Item1 + "','" + deploySettings.Item3 + "','" + string.Empty + "','" + null + "','" + null + "','"+ sourceTableItem.ChildrenCountForView + "','" + deploySettings.Item2 + "')";
                else
                    query = "Insert into " + itemTableName + " Values (@itemName,@Createdate,@CreateBy,'" + null + "','" + string.Empty + "','" + string.Empty + "',@categoryvalue,'" + string.Empty + "','" + sourceTableItem.ChildrenCountForView + "')";
                InsertCommandWithParameter1(query, "@itemName", itemNewName, "@Createdate", creationTime, "@CreateBy", creatorName,"@categoryvalue", categoryName);


                query = "select * from " + itemTableName + " where " + itemNameColumn + "=@itemName";

                //DataTable dataTable = new DataTable();
                //SqlDataAdapter dataAdapter = new SqlDataAdapter(query, connect);
                //dataAdapter.Fill(dataTable);

                DataTable dataTable = SelectDTWithParameter(query, "@itemName", itemNewName);
                DataTableReader dataTableReader = dataTable.CreateDataReader();

                if (dataTableReader.HasRows)
                {
                    bool isDesignChecked = true;
                    if (dataTableReader.Read())
                    {
                        createdTableItem = new TreeViewExplorer(dataTableReader.GetInt32(0), dataTableReader.GetString(1).ToString(), itemTableName, sourceTableItem.ExecutionWindow, sourceTableItem.DesignerWindow, creationTime, creatorName, null, null, null, categoryName, null, null, 0, true);
                        if (sourceTableItem.ItemType == QatConstants.DbTestPlanTable && dataTableReader[14] != System.DBNull.Value && dataTableReader[14].ToString() != string.Empty)
                            isDesignChecked = Convert.ToBoolean(dataTableReader.GetString(14));
                    }
                    if (sourceTableItem.ItemType == QatConstants.DbTestSuiteTable || (sourceTableItem.ItemType == QatConstants.DbTestPlanTable && isDesignChecked))
                    {
                        createBMcopy(itemTableName, dataTableReader.GetInt32(0), sourceTableItem.ItemKey);
                    }
                }

                //if (newDesignName != null && newDesignName != string.Empty && olddesignName!=null && olddesignName!=string.Empty)
                //{
                //    newDesignName=newDesignName.Remove(0,PreferencesServerPath.Length);
                //    createdTableItem.DesignName = newDesignName;
                //    sourceTableItem.DesignName = olddesignName;
                //}
            }
            catch (Exception ex)
            {
                MessageBox.Show("Exception\n Creating record failed\n" + ex.Message, "QAT Error Code - EC02013", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }

            return createdTableItem;
        }

        private Tuple<bool,bool,string> GetDeployItems(string itemName, string itemTableName, string itemNameColumn)
        {
            bool isDeployEnable = false;
            string deployCount = string.Empty;
            bool isDesignChecked = true;

            try
            {                
                itemName = itemName.Trim();
                // Check if the name already exists in the Database. If exists, add Copy to the name
                string query = "select IsDeployEnable, DeployCount,IsDesign from " + itemTableName + " where " + itemNameColumn + "=@ItemName";
                DataTable dataTable = SelectDTWithParameter(query, "@ItemName", itemName);
                DataTableReader dataTableReader = dataTable.CreateDataReader();

                if (dataTableReader.HasRows)
                {
                    while (dataTableReader.Read())
                    {
                        if (dataTableReader[0] != System.DBNull.Value)
                        {
                            isDeployEnable = Convert.ToBoolean(dataTableReader.GetValue(0));
                        }
                        if (dataTableReader[1] != System.DBNull.Value)
                        {
                            deployCount = dataTableReader.GetValue(1).ToString();
                        }
                        if (dataTableReader[2] != System.DBNull.Value && dataTableReader[2].ToString() != string.Empty)
                        {
                            isDesignChecked = Convert.ToBoolean(dataTableReader.GetValue(2));
                        }
                        return new Tuple<bool,bool,string>(isDeployEnable, isDesignChecked, deployCount);
                    }
                }

                return new Tuple<bool,bool, string>(isDeployEnable, isDesignChecked, deployCount);
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif

                //MessageBox.Show("Exception\n \n" + ex.Message, "QAT Error Code - EC02019", MessageBoxButton.OK, MessageBoxImage.Error);
                return new Tuple<bool,bool, string>(isDeployEnable, isDesignChecked, deployCount);
            }
        }

        private void createBMcopy(string TableType, Int32 newTableId, Int32 oldTableID)
        {
            try
            {
                string query = string.Empty;
                string querys = string.Empty;
                List<Int32> oldBMID = new List<Int32>();
                List<Int32> newBMID = new List<Int32>();
                if (TableType == "TestSuite")
                {
                    query = "select BackgroundMonitorID from BackgroundMonitoring where TSID='" + oldTableID + "'";
                }
                else if (TableType == "TestPlan")
                {
                    query = "select BackgroundMonitorID from BackgroundMonitoring where TPID='" + oldTableID + "'";
                }
                if(connect==null)
                CreateConnection();
                OpenConnection();
                DataTable dataTable = new DataTable();
                DataTableReader dataTableReader = null;
                SqlDataAdapter dataAdapter = new SqlDataAdapter(query, connect);
                dataAdapter.Fill(dataTable);
                dataTableReader = dataTable.CreateDataReader();

                if (dataTableReader.HasRows)
                {
                    while (dataTableReader.Read())
                    {
                        oldBMID.Add(dataTableReader.GetInt32(0));
                    }
                }

                if (oldBMID.Count > 0)
                {
                    if (TableType == "TestSuite")
                    {
                        querys = "INSERT INTO BackgroundMonitoring(TSID, TPID, MonitorFrequency, MonitorDuration,MonitorDurationType,ErrorHandlingType,Rerunittration,RerunDuration,RerunDurationType,MonitorType) SELECT '" + newTableId + "', TPID, MonitorFrequency, MonitorDuration,MonitorDurationType,ErrorHandlingType,Rerunittration,RerunDuration,RerunDurationType,MonitorType FROM BackgroundMonitoring WHERE TSID='" + oldTableID + "'";
                    }
                    if (TableType == "TestPlan")
                    {
                        querys = "INSERT INTO BackgroundMonitoring(TSID, TPID, MonitorFrequency, MonitorDuration,MonitorDurationType,ErrorHandlingType,Rerunittration,RerunDuration,RerunDurationType,MonitorType) SELECT TSID,'" + newTableId + "', MonitorFrequency, MonitorDuration,MonitorDurationType,ErrorHandlingType,Rerunittration,RerunDuration,RerunDurationType,MonitorType FROM BackgroundMonitoring WHERE TPID='" + oldTableID + "'";
                    }
                    SqlCommand command = new SqlCommand(querys, connect);
                    command.ExecuteScalar();


                    if (TableType == "TestSuite")
                    {
                        query = "select BackgroundMonitorID from BackgroundMonitoring where TSID='" + newTableId + "'";
                    }
                    else if (TableType == "TestPlan")
                    {
                        query = "select BackgroundMonitorID from BackgroundMonitoring where TPID='" + newTableId + "'";
                    }
                    dataTable.Clear();
                    dataTableReader.Close();
                    dataAdapter = new SqlDataAdapter(query, connect);
                    dataAdapter.Fill(dataTable);
                    dataTableReader = dataTable.CreateDataReader();

                    if (dataTableReader.HasRows)
                    {
                        while (dataTableReader.Read())
                        {
                            newBMID.Add(dataTableReader.GetInt32(0));
                        }
                    }
                    Int32[] new_BMID = newBMID.ToArray();
                    Int32[] old_BMID = oldBMID.ToArray();



                    for (int p = 0; p < new_BMID.Length; p++)
                    {
                        if (TableType == "TestSuite")
                        {
                            //querys = "INSERT INTO ControlMonitor(BMID, TSID, TPID, ComponentType, ComponentName, ComponentProperty, ComponentChannel, ComponentValue, PrettyName, ControlMonitorCheckedStatus,ValueType,MaximumLimit,MinimumLimit,LoopCheckedStatus,Loop_start,Loop_End,Loop_Increament,AllChannels) SELECT '" + new_BMID[p] + "', '" + newTableId + "',TPID,  ComponentType, ComponentName, ComponentProperty, ComponentChannel, ComponentValue, PrettyName, ControlMonitorCheckedStatus,ValueType,MaximumLimit,MinimumLimit,LoopCheckedStatus,Loop_start,Loop_End,Loop_Increament,AllChannels FROM ControlMonitor WHERE BMID ='" + old_BMID[p] + "' and TSID='" + oldTableID + "'";
                            //command = new SqlCommand(querys, connect);
                            //command.ExecuteScalar();
                            //querys = "INSERT INTO InventoryMonitor(BMID, TSID, TPID, SelectedLogsType) SELECT '" + new_BMID[p] + "', '" + newTableId + "',TPID,  SelectedLogsType FROM InventoryMonitor WHERE BMID ='" + old_BMID[p] + "' and TSID='" + oldTableID + "'";
                            //command = new SqlCommand(querys, connect);
                            //command.ExecuteScalar();
                            //querys = "INSERT INTO LogMonitor(BMID, TSID, TPID, SelectedLogsType, Devicename, TextToDevice, Commontext) SELECT '" + new_BMID[p] + "', '" + newTableId + "',TPID,  SelectedLogsType, Devicename, TextToDevice, Commontext FROM LogMonitor WHERE BMID ='" + old_BMID[p] + "' and TSID='" + oldTableID + "'";
                            //command = new SqlCommand(querys, connect);
                            //command.ExecuteScalar();
                            querys = "INSERT INTO TelenetMonitor(BMID, TSID, TPID, Telenetcommand, Devicesname, VerifyType, Comparetext,TelenetCheckedStatus, keywordType) SELECT '" + new_BMID[p] + "', '" + newTableId + "', TPID, Telenetcommand, Devicesname, VerifyType, Comparetext, TelenetCheckedStatus, keywordType FROM TelenetMonitor WHERE BMID ='" + old_BMID[p] + "' and TSID='" + oldTableID + "'";
                            command = new SqlCommand(querys, connect);
                            command.ExecuteScalar();
                            querys = "INSERT INTO TSMonitorLinkTable( TSID, BMID) Values ('" + newTableId + "','" + new_BMID[p] + "')";
                            command = new SqlCommand(querys, connect);
                            command.ExecuteScalar();
                        }
                        else if (TableType == "TestPlan")
                        {
                            querys = "INSERT INTO ControlMonitor(BMID, TSID, TPID, ComponentType, ComponentName, ComponentProperty, ComponentChannel, ComponentValue, PrettyName, ControlMonitorCheckedStatus,ValueType,MaximumLimit,MinimumLimit,LoopCheckedStatus,Loop_start,Loop_End,Loop_Increament,AllChannels ) SELECT '" + new_BMID[p] + "', TSID,'" + newTableId + "',  ComponentType, ComponentName, ComponentProperty, ComponentChannel, ComponentValue, PrettyName, ControlMonitorCheckedStatus,ValueType,MaximumLimit,MinimumLimit,LoopCheckedStatus,Loop_start,Loop_End,Loop_Increament,AllChannels  FROM ControlMonitor WHERE BMID ='" + old_BMID[p] + "' and TPID='" + oldTableID + "'";
                            command = new SqlCommand(querys, connect);
                            command.ExecuteScalar();
                            querys = "INSERT INTO InventoryMonitor(BMID, TSID, TPID, SelectedLogsType) SELECT '" + new_BMID[p] + "',TSID, '" + newTableId + "',  SelectedLogsType FROM InventoryMonitor WHERE BMID ='" + old_BMID[p] + "' and TPID='" + oldTableID + "'";
                            command = new SqlCommand(querys, connect);
                            command.ExecuteScalar();
                            querys = "INSERT INTO LogMonitor1(BMID, TSID, TPID, ilog, ilog_combobox, ilog_text, kernellog,kernel_logcombobox,kernellog_text,eventlog,eventlog_text,configuratorlog,configuratorlog_text,siplog,siplog_text,QSYSapplog,QSYSapplog_text,UCIlog,UCIlog_text,softphonelog,softphonelog_text,Windows_eventlog,Windows_eventlog_text) SELECT '" + new_BMID[p] + "', TSID, '" + newTableId + "',  ilog, ilog_combobox, ilog_text, kernellog,kernel_logcombobox,kernellog_text,eventlog,eventlog_text,configuratorlog,configuratorlog_text,siplog,siplog_text,QSYSapplog,QSYSapplog_text,UCIlog,UCIlog_text,softphonelog,softphonelog_text,Windows_eventlog,Windows_eventlog_text FROM LogMonitor1 WHERE BMID ='" + old_BMID[p] + "' and TPID='" + oldTableID + "'";
                            command = new SqlCommand(querys, connect);
                            command.ExecuteScalar();
                            querys = "INSERT INTO TelenetMonitor(BMID, TSID, TPID, Telenetcommand, Devicesname, VerifyType, Comparetext,TelenetCheckedStatus,keywordType) SELECT '" + new_BMID[p] + "', TSID, '" + newTableId + "', Telenetcommand, Devicesname, VerifyType, Comparetext, TelenetCheckedStatus, keywordType FROM TelenetMonitor WHERE BMID ='" + old_BMID[p] + "' and TPID='" + oldTableID + "'";
                            command = new SqlCommand(querys, connect);
                            command.ExecuteScalar();
                            querys = "INSERT INTO TPMonitorLinkTable( TPID, BMID) Values ('" + newTableId + "','" + new_BMID[p] + "')";
                            command = new SqlCommand(querys, connect);
                            command.ExecuteScalar();
                        }


                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Exception\n Creating record failed\n" + ex.Message, "QAT Error Code - ECxxxxx", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }

        private void CreateCopyOfLinkedChildTreeItem(TreeViewExplorer createdTableItem, TreeViewExplorer sourceTableItem)
        {
            List<int> childPrimaryKeyList = new List<int>();

            try
            {
                string query = null;
                string itemLinkTable = null;
                string itemIDColumn = null;
                string childTableName = null;
                string childIDColumn = null;

                if (sourceTableItem.ItemType == QatConstants.DbTestSuiteTable)
                {
                    itemLinkTable = QatConstants.DbTestSuiteTestPlanLinkTable;
                    itemIDColumn = QatConstants.DbTestSuiteLinkTableID;
                    childTableName = QatConstants.DbTestPlanTable;
                    childIDColumn = QatConstants.DbTestPlanLinkTableID;
                }
                else if (sourceTableItem.ItemType == QatConstants.DbTestPlanTable)
                {
                    itemLinkTable = QatConstants.DbTestPlanTestCaseLinkTable;
                    itemIDColumn = QatConstants.DbTestPlanLinkTableID;
                    childTableName = QatConstants.DbTestCaseTable;
                    childIDColumn = QatConstants.DbTestCaseLinkTableID;
                }
                else
                {
                    return;
                }

                // Get list of child items linked to the source parent item

                ////query = "select * from " + childTableName + " as c join " + itemLinkTable + " as p on p." + childIDColumn + " = c." + childTableName + "ID where p." + itemIDColumn + " = " + sourceTableItem.ItemKey.ToString();

                DataTable dataTable = new DataTable();
                query = "select " + childIDColumn + " from  " + itemLinkTable + " where " + itemIDColumn + " = " + sourceTableItem.ItemKey.ToString();
                SqlDataAdapter dataAdapt = new SqlDataAdapter(query, connect);
                dataAdapt.Fill(dataTable);
                DataTableReader dataTableReader = dataTable.CreateDataReader();

                if (dataTableReader.HasRows)
                {
                    while (dataTableReader.Read())
                    {
                        if (dataTableReader[0] != System.DBNull.Value)
                        {
                            string childID = dataTableReader.GetValue(0).ToString();

                            query = "select * from " + childTableName + " where " + childTableName + "ID in (" + childID + ")";
                            
                            DataTable table = new DataTable();
                            SqlDataAdapter dataAdapter = new SqlDataAdapter(query, connect);
                            dataAdapter.Fill(table);
                            DataTableReader dataTableRead = table.CreateDataReader();

                            if (dataTableRead.HasRows)
                            {
                                while (dataTableRead.Read())
                                    childPrimaryKeyList.Add(dataTableRead.GetInt32(0));
                            }
                        }
                    }
                }

                // Copy the childitems of the source item to the new item created
                //string queryValues = null;

                //if(childPrimaryKeyList.Count > 0)
                //{
                //    foreach (int childPrimaryKey in childPrimaryKeyList)
                //        queryValues += "(" + createdTableItem.ItemKey.ToString() + "," + childPrimaryKey.ToString() + "),";

                //    queryValues = queryValues.TrimEnd(',');

                //    query = "Insert into " + itemLinkTable + "(" + itemIDColumn + "," + childIDColumn + ") values" + queryValues;
                //    SqlCommand command = new SqlCommand(query, connect);
                //    command.ExecuteScalar();
                //}
                var Querytable = new DataTable();
                Querytable.Columns.Add(itemIDColumn, typeof(int));
                Querytable.Columns.Add(childIDColumn, typeof(int));
                foreach (int childPrimaryKey in childPrimaryKeyList)
                {
                    Querytable.Rows.Add(createdTableItem.ItemKey.ToString(), childPrimaryKey.ToString());
                }
                using (var bulkCopy = new SqlBulkCopy(connect))
                {
                    bulkCopy.DestinationTableName = "dbo." + itemLinkTable;
                    bulkCopy.WriteToServer(Querytable);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Exception\n Creating link record failed\n" + ex.Message, "QAT Error Code - EC02014", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CreateCopyOfDesign(TreeViewExplorer createdTableItem, TreeViewExplorer sourceTableItem)
        {
            try
            {
                int sourceDesignID = 0;
                string sourceDesignName = null;
                int createdDesignID = 0;
                string createdDesignName = null;

                string query = null;
                string itemLinkTable = null;
                string itemIDColumn = null;
                string itemDesignIDColumn = null;
                string itemDesignTable = null;
                string itemDesignNameColumn = null;
                //DataTableReader dataTableReader = null;
                TestPlanItem sourcePlanItem = new TestPlanItem(sourceTableItem, false);

                if (sourceTableItem.ItemType == QatConstants.DbTestPlanTable)
                {
                    itemLinkTable = QatConstants.DbTestPlanDesignLinkTable;
                    itemIDColumn = QatConstants.DbTestPlanLinkTableID;
                    itemDesignIDColumn = QatConstants.DbTestDesignLinkTableID;
                    itemDesignTable = QatConstants.DbTestDesignTable;
                    itemDesignNameColumn = QatConstants.DbTestDesignNameColumn;
                }
                else
                    return;


                if (sourcePlanItem.IsDesignChecked == false)
                {
                    createdDesignName = "QAT_" + createdTableItem.ItemName + "_V1_NO_QSYS_DESIGN";                   
                    query = "insert into " + itemDesignTable + " values (@designName, '" + createdTableItem.ItemKey + "')";
                    InsertCommandWithParameter(query, "@designName", createdDesignName);

                    query = "select " + itemDesignIDColumn + " from " + itemDesignTable + " where " + itemDesignNameColumn + "=@designName";
                    DataTable dataTable1 = SelectDTWithParameter(query, "@designName", createdDesignName);
                    DataTableReader dataTableReader = dataTable1.CreateDataReader();
                   
                    if (dataTableReader.HasRows)
                    {
                        if (dataTableReader.Read())
                            createdDesignID = dataTableReader.GetInt32(0);
                    }
                    
                    query = "Insert into " + itemLinkTable + "(" + itemIDColumn + "," + itemDesignIDColumn + ") values(" + createdTableItem.ItemKey + "," + createdDesignID + ")";
                    SqlCommand command = new SqlCommand(query, connect);
                    command.ExecuteScalar();
                }
                else
                {                    
                    query = "select " + itemDesignIDColumn + " from " + itemLinkTable + " where " + itemIDColumn + "=('" + sourceTableItem.ItemKey + "')";

                    DataTable dataTable = new DataTable();
                    DataTableReader dataTableReader = null;
                    SqlDataAdapter dataAdapter = new SqlDataAdapter(query, connect);
                    dataAdapter.Fill(dataTable);
                    dataTableReader = dataTable.CreateDataReader();

                    if (dataTableReader.HasRows)
                    {
                        if (dataTableReader.Read())
                            sourceDesignID = dataTableReader.GetInt32(0);
                    }

                    if (sourceDesignID != 0)
                    {
                        query = "select " + itemDesignNameColumn + " from " + itemDesignTable + " where " + itemDesignIDColumn + "=('" + sourceDesignID + "')";

                        dataTable = new DataTable();
                        dataTableReader = null;
                        dataAdapter = new SqlDataAdapter(query, connect);
                        dataAdapter.Fill(dataTable);
                        dataTableReader = dataTable.CreateDataReader();

                        if (dataTableReader.HasRows)
                        {
                            if (dataTableReader.Read())
                                sourceDesignName = dataTableReader.GetString(0);
                        }

                        createdDesignName = sourceDesignName.Substring(sourceTableItem.ItemName.Length + 6);
                        if (createdDesignName.IndexOf("_") > 0)
                            createdDesignName = createdDesignName.Substring(createdDesignName.IndexOf("_"));

                        createdDesignName = "QAT_" + createdTableItem.ItemName + "_V1" + createdDesignName;

                        //createdDesignName = createdTableItem.DesignName;
                        //sourceDesignName = sourceTableItem.DesignName;

                        //query = "insert into "+ itemDesignTable + " values('" + createdDesignName + "')";
                        //SqlCommand command = new SqlCommand(query, connect);
                        //command.ExecuteScalar();

                        query = "insert into " + itemDesignTable + " values (@designName, '" + createdTableItem.ItemKey + "')";
                        InsertCommandWithParameter(query, "@designName", createdDesignName);
                        //if (Properties.Settings.Default.Path.ToString() != string.Empty)
                        //{
                        string PreferencesServerPath = QatConstants.QATServerPath + "\\Designs" + "\\";
                        if (File.Exists(PreferencesServerPath + sourceDesignName))
                        {
                            System.IO.File.Copy(PreferencesServerPath + sourceDesignName + "", PreferencesServerPath + createdDesignName + "");
                            FileInfo fileInformation = new FileInfo(PreferencesServerPath + createdDesignName + "");
                            fileInformation.IsReadOnly = true;
                        }
                        //}

                        query = "select " + itemDesignIDColumn + " from " + itemDesignTable + " where " + itemDesignNameColumn + "=@designName";
                        DataTable dataTable1 = SelectDTWithParameter(query, "@designName", createdDesignName);
                        dataTableReader = dataTable1.CreateDataReader();

                        //query = "select "+ itemDesignIDColumn + " from "+ itemDesignTable + " where "+ itemDesignNameColumn + "=('" + createdDesignName + "')";
                        //dataTable = new DataTable();
                        //dataTableReader = null;
                        //dataAdapter = new SqlDataAdapter(query, connect);
                        //dataAdapter.Fill(dataTable);
                        //dataTableReader = dataTable.CreateDataReader();

                        if (dataTableReader.HasRows)
                        {
                            if (dataTableReader.Read())
                                createdDesignID = dataTableReader.GetInt32(0);
                        }


                        query = "Insert into " + itemLinkTable + "(" + itemIDColumn + "," + itemDesignIDColumn + ") values(" + createdTableItem.ItemKey + "," + createdDesignID + ")";
                        SqlCommand command = new SqlCommand(query, connect);
                        command.ExecuteScalar();

                        query = "INSERT INTO DesignInventory(DesignId, Devicetype, DeviceModel, Devicenameindesign,PrimaryorBackup,Backup_to_primary) SELECT '" + createdDesignID + "', Devicetype, DeviceModel, Devicenameindesign,PrimaryorBackup,Backup_to_primary FROM DesignInventory WHERE DesignId='" + sourceDesignID + "'";
                        command = new SqlCommand(query, connect);
                        command.ExecuteScalar();



                        query = "INSERT INTO TCInitialization(DesignID, ComponentType, ComponentName,Control,SpecControlID,Type,MinValue,MaxValue,DefaultValue,DefaultPosition,DefaultString,InitialValue,InitialString,InitialPosition,PrettyName,Subtype,ArraySize,ControlDirection,ClassName,ComponentPrettyName,NetworkName) SELECT '" + createdDesignID + "',ComponentType, ComponentName,Control,SpecControlID,Type,MinValue,MaxValue,DefaultValue,DefaultPosition,DefaultString,InitialValue,InitialString,InitialPosition,PrettyName,Subtype,ArraySize,ControlDirection,ClassName,ComponentPrettyName,NetworkName FROM TCInitialization WHERE DesignId='" + sourceDesignID + "'";
                        //query = "INSERT INTO TCInitialization(DesignID, ComponentType, ComponentName,Control,SpecControlID,Type,MinValue,MaxValue,DefaultValue,DefaultPosition,DefaultString,InitialValue,InitialString,InitialPosition,PrettyName) SELECT '" + createdDesignID + "',ComponentType, ComponentName,Control,SpecControlID,Type,MinValue,MaxValue,DefaultValue,DefaultPosition,DefaultString,InitialValue,InitialString,InitialPosition,PrettyName FROM TCInitialization WHERE DesignId='" + sourceDesignID + "'";
                        command = new SqlCommand(query, connect);
                        command.ExecuteScalar();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Exception\n Creating Design record failed\n" + ex.Message, "QAT Error Code - EC02015", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public bool DeleteTreeItem(TreeViewExplorer sourceTableItem)
        {
            try
            {
                string query = null;
                string itemTableName = null;
                string itemIDColumn = null;
                SqlCommand command = null;
                List<int> LinkTestsuiteid = new List<int>();
                List<int> LinkTestplanid = new List<int>();

                QatMessageBox QMessageBox = new QatMessageBox(sourceTableItem.DesignerWindow);

                if (sourceTableItem.ItemType == QatConstants.DbTestSuiteTable)
                {
                    itemTableName = QatConstants.DbTestSuiteTable;
                    itemIDColumn = QatConstants.DbTestSuiteIDColumn;
                    List<TreeViewExplorer> testPlanList = ReadTreeTable(QatConstants.DbTestPlanTable, sourceTableItem.ItemKey, null, null);
                    List<string> testPlanStringList = new List<string>();
                    testPlanList.ForEach(item => testPlanStringList.Add(item.ItemName));
                    string TP_list = string.Join("\n", testPlanStringList.ToArray());

                    if (testPlanList.Count > 0)
                    {
                        //MessageBoxResult result = MessageBox.Show("Testcase: \n" + TC_list + "\n are associated with the testplan: " + sourceTableItem.ItemName + ".\n Do you want to continue delete ?", "Delete Warning", MessageBoxButton.OKCancel, MessageBoxImage.Warning);
                    
                        MessageBoxResult result = QMessageBox.Show("No of Test Plans associated: " + testPlanList.Count.ToString() + "\n\nName of Test Plans associated:\n" + TP_list  + "\n\nDo you want to continue deleting Test Suite : " + sourceTableItem.ItemName + " ?", "Delete Warning", MessageBoxButton.OKCancel, MessageBoxImage.Warning);
                        if((result == MessageBoxResult.Cancel) || (result == MessageBoxResult.None))
                            return false;
                    }
                    else
                    {
                        MessageBoxResult result = QMessageBox.Show("No Testplans are associated with test Suite: " + sourceTableItem.ItemName + "\n Do you want to continue delete ?", "Delete Warning", MessageBoxButton.OKCancel, MessageBoxImage.Warning);

                        if ((result == MessageBoxResult.Cancel) || (result == MessageBoxResult.None))
                            return false;
                    }
                }
                else if (sourceTableItem.ItemType == QatConstants.DbTestPlanTable)
                {
                    List<TreeViewExplorer> testCaseList = ReadTreeTable(QatConstants.DbTestCaseTable, sourceTableItem.ItemKey, null, null);
                    List<string> testCaseStringList = new List<string>();
                    testCaseList.ForEach(item => testCaseStringList.Add(item.ItemName));
                    string TC_list = string.Join("\n", testCaseStringList.ToArray());

                    if (testCaseList.Count > 0)
                    {
                        //MessageBoxResult result = MessageBox.Show("Testcase: \n" + TC_list + "\n are associated with the testplan: " + sourceTableItem.ItemName + ".\n Do you want to continue delete ?", "Delete Warning", MessageBoxButton.OKCancel, MessageBoxImage.Warning);
                     
                        MessageBoxResult result = QMessageBox.Show("No of Test Cases associated: " + testCaseList.Count.ToString() + "\n\nName of Test Cases associated:\n" + TC_list + "\n\nDo you want to continue deleting Test Plan : " + sourceTableItem.ItemName + " ?", "Delete Warning", MessageBoxButton.OKCancel, MessageBoxImage.Warning);
                        if ((result == MessageBoxResult.Cancel) || (result == MessageBoxResult.None))
                            return false;
                    }
                    else
                    {
                        MessageBoxResult result = QMessageBox.Show("No Testcases are associated with test plan: " + sourceTableItem.ItemName + "\n Do you want to continue delete ?", "Delete Warning", MessageBoxButton.OKCancel, MessageBoxImage.Warning);

                        if ((result == MessageBoxResult.Cancel) || (result == MessageBoxResult.None))
                            return false;
                    }

                    CreateConnection();
                    OpenConnection();
                    query = "select " + QatConstants.DbTestSuiteLinkTableID + " FROM " + QatConstants.DbTestSuiteTestPlanLinkTable + " where " + QatConstants.DbTestPlanLinkTableID + " = '" + sourceTableItem.ItemKey + "'";
                    DataTable dataTable = new DataTable();
                    DataTableReader dataTableReader = null;
                    SqlDataAdapter dataAdapter = new SqlDataAdapter(query, connect);
                    dataAdapter.Fill(dataTable);
                    dataTableReader = dataTable.CreateDataReader();

                    if (dataTableReader.HasRows)
                    {
                        while (dataTableReader.Read())
                            LinkTestsuiteid.Add(dataTableReader.GetInt32(0));
                    }

                    CloseConnection();
                    
                    itemTableName = QatConstants.DbTestPlanTable;
                    itemIDColumn = QatConstants.DbTestPlanIDColumn;
                }
                else if (sourceTableItem.ItemType == QatConstants.DbTestCaseTable)
                {
                    List<TreeViewExplorer> testCaseList = ReadTreeTableTC(QatConstants.DbTestPlanTable, sourceTableItem.ItemKey, null, null);
                    List<string> testCaseStringList = new List<string>();
                    testCaseList.ForEach(item => testCaseStringList.Add(item.ItemName));
                    string TC_list = string.Join("\n", testCaseStringList.ToArray());

                    if (testCaseList.Count > 0)
                    {
                       
                        MessageBoxResult result = QMessageBox.Show("No of Test Plans associated: "+testCaseList.Count.ToString()+"\n\nName of Test Plans associated:\n" +TC_list + "\n\nDo you want to continue deleting Test Case : " + sourceTableItem.ItemName + " ?", "Delete Warning", MessageBoxButton.OKCancel, MessageBoxImage.Warning);

                        if ((result == MessageBoxResult.Cancel) || (result == MessageBoxResult.None))
                            return false;
                    }
                    else
                    {
                        MessageBoxResult result = QMessageBox.Show("No Test plans are associated with testcase: " + sourceTableItem.ItemName + "\n Do you want to continue delete ?", "Delete Warning", MessageBoxButton.OKCancel, MessageBoxImage.Warning);

                        if ((result == MessageBoxResult.Cancel) || (result == MessageBoxResult.None))
                            return false;
                    }

                    itemTableName = QatConstants.DbTestCaseTable;
                    itemIDColumn = QatConstants.DbTestCaseIDColumn;
                    
                    CreateConnection();
                    OpenConnection();
                    int actioncount = 0;
                    query = "select " + QatConstants.DbTestPlanLinkTableID + " FROM " + QatConstants.DbTestPlanTestCaseLinkTable + " where " + QatConstants.DbTestCaseLinkTableID + " = '" + sourceTableItem.ItemKey + "'";
                    DataTable dataTable = new DataTable();
                    DataTableReader dataTableReader = null;
                    SqlDataAdapter dataAdapter = new SqlDataAdapter(query, connect);
                    dataAdapter.Fill(dataTable);
                    dataTableReader = dataTable.CreateDataReader();

                    if (dataTableReader.HasRows)
                    {
                        while (dataTableReader.Read())
                            LinkTestplanid.Add(dataTableReader.GetInt32(0));
                    }                  

                    CloseConnection();
                }
                else
                    return false;

                CreateConnection();
                OpenConnection();

                if (sourceTableItem.ItemType == QatConstants.DbTestPlanTable)
                {
                    int desgnId = 0;
                    List<string> designNameList = new List<string>();
                    string filePath = null;

                    string PreferencesServerPath = QatConstants.QATServerPath + "\\Designs" + "\\";
                    //string PreferencesServerPath = Properties.Settings.Default.Path.ToString() + "\\";

                    query = "Select DesignID FROM TPDesignLinkTable where TPID='" + sourceTableItem.ItemKey + "'";

                    DataTable dataTable = new DataTable();
                    DataTableReader dataTableReader = null;
                    SqlDataAdapter dataAdapter = new SqlDataAdapter(query, connect);
                    dataAdapter.Fill(dataTable);
                    dataTableReader = dataTable.CreateDataReader();

                    if (dataTableReader.HasRows)
                    {
                        if (dataTableReader.Read())
                            desgnId = dataTableReader.GetInt32(0);
                    }

                    query = "Select Designname FROM designtable where TPID='" + sourceTableItem.ItemKey + "'";

                    dataTable = new DataTable();
                    dataTableReader = null;
                    dataAdapter = new SqlDataAdapter(query, connect);
                    dataAdapter.Fill(dataTable);
                    dataTableReader = dataTable.CreateDataReader();

                    if (dataTableReader.HasRows)
                    {
                        while (dataTableReader.Read())
                            designNameList.Add(dataTableReader.GetString(0).ToString());
                    }

                    foreach (string designName in designNameList)
                    {
                        if (PreferencesServerPath != string.Empty && designName != string.Empty && designName != null)
                        {
                            filePath = Path.Combine(PreferencesServerPath, designName);
                            if (File.Exists(filePath))
                            {
                                File.SetAttributes(filePath, FileAttributes.Normal);
                                File.Delete(filePath);
                            }
                        }
                    }

                    // Delete Design
                    query = "delete from designtable where TPID='" + sourceTableItem.ItemKey + "'";
                    command = new SqlCommand(query, connect);
                    command.ExecuteScalar();

                }
                else if(sourceTableItem.ItemType == QatConstants.DbTestCaseTable)
                {
                    query = "select APxPath from APVerification where TCID = '" + sourceTableItem.ItemKey + "'";
                    DeleteFileFromServer(query, "Audio Precision", "AP Project Files");


                    query = "select WaveformType from APSettings where TCID = '" + sourceTableItem.ItemKey + "'";
                    DeleteFileFromServer(query, "Audio Precision", "AP Waveform Files");


                    query = "select WaveformType from LevelAndGain where TCID = '" + sourceTableItem.ItemKey + "'";
                    DeleteFileFromServer(query, "Audio Precision", "AP Waveform Files");


                    query = "select VerificationLocation from APFrequencyResponse where TCID = '" + sourceTableItem.ItemKey + "'";
                    DeleteFileFromServer(query, "Audio Precision", "Verification Files");


                    query = "select VerificationLocation from APPhaseSettings where TCID = '" + sourceTableItem.ItemKey + "'";
                    DeleteFileFromServer(query, "Audio Precision", "Verification Files");


                    query = "select VerificationLocation from APSteppedFreqSweepSettings where TCID = '" + sourceTableItem.ItemKey + "'";
                    DeleteFileFromServer(query, "Audio Precision", "Verification Files");


                    query = "select VerificationLocation from APTHDNSettings where TCID = '" + sourceTableItem.ItemKey + "'";
                    DeleteFileFromServer(query, "Audio Precision", "Verification Files");
                  
                    query = "select VerificationFileLocation from Responsalyzer where TCID = '" + sourceTableItem.ItemKey + "'";
                    DeleteFileFromServer(query, "Responsalyzer", "Reference Files");

                    ////QRCM file case delete 
                    string filepath = Path.Combine(QatConstants.QATServerPath, "QRCM_Files", sourceTableItem.ItemKey +".txt");

                    if (File.Exists(filepath))
                    {
                        File.SetAttributes(filepath, FileAttributes.Normal);
                        File.Delete(filepath);
                    }
                }
                
                // Delete record and get the ID
                query = "delete from " + itemTableName + " where " + itemIDColumn + "='" + sourceTableItem.ItemKey + "'";
                command = new SqlCommand(query, connect);
                command.ExecuteScalar();

                CloseConnection();

                if(sourceTableItem.Parent.ChildrenCountForView > 0)
                    sourceTableItem.Parent.ChildrenCountForView -= 1;


                if (sourceTableItem.ItemType == QatConstants.DbTestPlanTable)
                {
                    foreach (int tsid in LinkTestsuiteid)
                    {
                        int actioncount = 0;
                        query = "Select * FROM " + QatConstants.DbTestSuiteTestPlanLinkTable + " where " + QatConstants.DbTestSuiteLinkTableID + " = '" + tsid + "'";
                        DataTable dataTable = new DataTable();
                        CreateConnection();
                        OpenConnection();
                        SqlDataAdapter dataAdapter = new SqlDataAdapter(query, connect);
                        dataAdapter.Fill(dataTable);
                        if (dataTable.Rows.Count > 0)
                            actioncount = dataTable.Rows.Count;

                        //actioncount = actioncount - 1;
                        query = "update " + QatConstants.DbTestSuiteTable + " set " + QatConstants.DbTestSuiteChildCount + " = '" + actioncount + "' where " + QatConstants.DbTestSuiteIDColumn + " = '" + tsid + "'";
                        //query = "update " + itemLinkTable + " set " + itemLinkcolumn + " = "+ actioncount +" where  TestPlanID = '" + tpid + "'";
                        command = new SqlCommand(query, connect);
                        command.ExecuteScalar();

                        CloseConnection();
                    }
                }
                else if (sourceTableItem.ItemType == QatConstants.DbTestCaseTable)
                {
                    foreach (int tpid in LinkTestplanid)
                    {
                        int actioncount = 0;
                        query = "Select * FROM " + QatConstants.DbTestPlanTestCaseLinkTable + " where " + QatConstants.DbTestPlanLinkTableID + " = '" + tpid + "'";
                        DataTable dataTable = new DataTable();
                        CreateConnection();
                        OpenConnection();
                        SqlDataAdapter dataAdapter = new SqlDataAdapter(query, connect);
                        dataAdapter.Fill(dataTable);
                        if (dataTable.Rows.Count > 0)
                            actioncount = dataTable.Rows.Count;

                        //actioncount = actioncount - 1;
                        query = "update " + QatConstants.DbTestPlanTable + " set " + QatConstants.DbTestPlanChildCount + " = '" + actioncount + "' where " + QatConstants.DbTestPlanIDColumn + " = '" + tpid + "'";
                        //query = "update " + itemLinkTable + " set " + itemLinkcolumn + " = "+ actioncount +" where  TestPlanID = '" + tpid + "'";
                        command = new SqlCommand(query, connect);
                        command.ExecuteScalar();

                        CloseConnection();
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Exception\n Creating record failed\n" + ex.Message, "QAT Error Code - EC02016", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        public bool RenameTreeItem(TreeViewExplorer sourceTableItem)
        {
            try
            {
                string query = null;
                string itemTableName = null;
                string itemIDColumn = null;
                string itemNameColumn = null;
                string itemDesignIDColumn = null;
                string itemDesignTable = null;
                string itemDesignNameColumn = null;
                bool returnStatus = false;
                bool exception = false;


                if (sourceTableItem.ItemType == QatConstants.DbTestSuiteTable)
                {
                    itemTableName = QatConstants.DbTestSuiteTable;
                    itemNameColumn = QatConstants.DbTestSuiteNameColumn;
                    itemIDColumn = QatConstants.DbTestSuiteIDColumn;
                }
                else if (sourceTableItem.ItemType == QatConstants.DbTestPlanTable)
                {
                    itemTableName = QatConstants.DbTestPlanTable;
                    itemNameColumn = QatConstants.DbTestPlanNameColumn;
                    itemIDColumn = QatConstants.DbTestPlanIDColumn;
                    itemDesignIDColumn = QatConstants.DbTestDesignLinkTableID;
                    itemDesignTable = QatConstants.DbTestDesignTable;
                    itemDesignNameColumn = QatConstants.DbTestDesignNameColumn;
                }
                else if (sourceTableItem.ItemType == QatConstants.DbTestCaseTable)
                {
                    itemTableName = QatConstants.DbTestCaseTable;
                    itemNameColumn = QatConstants.DbTestCaseNameColumn;
                    itemIDColumn = QatConstants.DbTestCaseIDColumn;
                }
                else
                {
                    return false;
                }

                if (sourceTableItem.ItemName.Trim() == "")
                {
                    MessageBox.Show("Name is empty\n", "QAT Error Message", MessageBoxButton.OK, MessageBoxImage.Error);
                    sourceTableItem.ItemName = sourceTableItem.copyItemName;
                    sourceTableItem.copyItemName = null;
                    return false;
                }

                if (String.Equals(sourceTableItem.ItemName.Trim(), sourceTableItem.copyItemName))
                {
                    sourceTableItem.ItemName = sourceTableItem.copyItemName;
                    sourceTableItem.copyItemName = null;
                    return false;
                }

                //sourceTableItem.ItemName = sourceTableItem.ItemTextBox.Text.Trim();
                sourceTableItem.ItemName = sourceTableItem.ItemName.Trim();

                if (Regex.IsMatch(sourceTableItem.ItemName, @"[\\/:*?<>'%|""[\]&]"))
                {
                    MessageBox.Show("The " + sourceTableItem.ItemType + " name can't contains any of the following characters: \n  " + @"\ / : * ? & "" < > ' % [ ] |", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }

                string exists = ChkNameExistinInventoryList(sourceTableItem.ItemName);

                if (exists != null)
                {
                    MessageBox.Show("Name already exists in " + exists + " list.\nPlease rename", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                string PreferencesServerPath = QatConstants.QATServerPath + "\\Designs" + "\\";
                if (sourceTableItem.ItemType == QatConstants.DbTestPlanTable)
                {
                    CreateConnection();
                    OpenConnection();
                    int Designid = 0;
                    string DesignName = null;

                    query = "Select DesignID FROM TPDesignLinkTable where TPID='" + sourceTableItem.ItemKey + "'";
                    DataTable dataTable = new DataTable();
                    DataTableReader dataTableReader = null;
                    SqlDataAdapter dataAdapter = new SqlDataAdapter(query, connect);
                    dataAdapter.Fill(dataTable);
                    dataTableReader = dataTable.CreateDataReader();
                    if (dataTableReader.HasRows)
                    {
                        if (dataTableReader.Read())
                            Designid = dataTableReader.GetInt32(0);
                    }

                    query = "Select Designname FROM designtable where DesignID='" + Designid + "'";

                    dataTable = new DataTable();
                    dataTableReader = null;
                    dataAdapter = new SqlDataAdapter(query, connect);
                    dataAdapter.Fill(dataTable);
                    dataTableReader = dataTable.CreateDataReader();
                    if (dataTableReader.HasRows)
                    {
                        if (dataTableReader.Read())
                        {
                            DesignName = dataTableReader.GetString(0).ToString();
                        }
                    }
                    CloseConnection();

                    int len = 0;
                    len = ("QAT_" + sourceTableItem.copyItemName).Length;
                    DesignName = DesignName.Remove(0, len);

                  

                    string name = Path.Combine(PreferencesServerPath, "QAT_" + sourceTableItem.ItemName + DesignName);
                     exception = fileinformation(name);
                }
            

                if (exception == false)
                {
                    // Rename record and get the ID
                    if (!String.Equals(sourceTableItem.ItemName, sourceTableItem.copyItemName) & exists == null)
                    {
                        CreateConnection();
                        OpenConnection();

                       

                        DateTime? modifiedTime = DateTime.Now;
                        string editorName = Properties.Settings.Default.TesterName.ToString().TrimEnd();
                        query = "update " + itemTableName + " set " + itemNameColumn + " =@TSname,ModifiedOn=@editdate,ModifiedBy=@editBy where " + itemIDColumn + " = '" + sourceTableItem.ItemKey + "'";
                        InsertCommandWithParameter1(query, "@TSname", sourceTableItem.ItemName.Trim(), "@editdate", modifiedTime, "@editBy", editorName, string.Empty, string.Empty);
                        sourceTableItem.Modifiedon = modifiedTime;
                        sourceTableItem.Modifiedby = editorName;
                        if (sourceTableItem.ItemType == QatConstants.DbTestPlanTable)
                        {
                            int desgnId = 0;
                            string newDesignName = null;

                            //string PreferencesServerPath = QatConstants.QATServerPath + "\\Designs" + "\\";
                            //string PreferencesServerPath = Properties.Settings.Default.Path.ToString() + "\\";

                            query = "Select DesignID FROM TPDesignLinkTable where TPID='" + sourceTableItem.ItemKey + "'";
                            DataTable dataTable = new DataTable();
                            DataTableReader dataTableReader = null;
                            SqlDataAdapter dataAdapter = new SqlDataAdapter(query, connect);
                            dataAdapter.Fill(dataTable);
                            dataTableReader = dataTable.CreateDataReader();

                            if (dataTableReader.HasRows)
                            {
                                if (dataTableReader.Read())
                                    desgnId = dataTableReader.GetInt32(0);
                            }

                            query = "Select * FROM designtable where TPID='" + sourceTableItem.ItemKey + "'";

                            dataTable = new DataTable();
                            dataTableReader = null;
                            dataAdapter = new SqlDataAdapter(query, connect);
                            dataAdapter.Fill(dataTable);
                            dataTableReader = dataTable.CreateDataReader();

                            if (dataTableReader.HasRows)
                            {
                                while (dataTableReader.Read())
                                {
                                    int deisgnID = dataTableReader.GetInt32(0);
                                    string oldDesignName = dataTableReader.GetString(1).ToString();

                                    newDesignName = oldDesignName.Remove(0, sourceTableItem.copyItemName.Length + 4);
                                    newDesignName = "QAT_" + sourceTableItem.ItemName + newDesignName;

                                    string name = Path.Combine(PreferencesServerPath, newDesignName);
                                    bool except = false;
                                    try
                                    {
                                        FileInfo sample = new FileInfo(name);
                                    }
                                    catch (Exception ex)
                                    {
                                        except = true;
                                    }

                                    if (except == false)
                                    {
                                        if (PreferencesServerPath != string.Empty && oldDesignName != string.Empty && oldDesignName != null)
                                        {
                                            if (File.Exists(PreferencesServerPath + oldDesignName))
                                            {
                                                System.IO.File.Move(PreferencesServerPath + oldDesignName + "", PreferencesServerPath + newDesignName + "");
                                                FileInfo fileInformation = new FileInfo(PreferencesServerPath + newDesignName + "");
                                                fileInformation.IsReadOnly = true;
                                            }

                                        query = "update " + itemDesignTable + " set " + itemDesignNameColumn + " = @newDesign where DesignID = '" + deisgnID + "'";
                                        //SqlCommand cmd = new SqlCommand(query, connect);
                                        //cmd.ExecuteScalar();
                                        InsertCommandWithParameter(query, "@newDesign", newDesignName);
                                        }
                                    }
                                }
                            }


                        }

                        CloseConnection();
                        returnStatus = true;
                    }
                    else
                    {
                        sourceTableItem.ItemName = sourceTableItem.copyItemName;
                    }
                }
                else
                {
                    sourceTableItem.ItemName = sourceTableItem.copyItemName;
                }

                sourceTableItem.copyItemName = null;
                return returnStatus;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Exception\n Creating record failed\n" + ex.Message, "QAT Error Code - EC02018", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }
        private bool fileinformation(string path)
        {
            bool ExceptionIsTrue = false;
            try
            {
                FileInfo sample = new FileInfo(path);
            }
            catch
            {
                ExceptionIsTrue = true;
                MessageBox.Show("Name is too long.Unable to update in design server path, Please rename", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            return ExceptionIsTrue;
        }

        private string CreateCopyItemName(string itemName, string itemTableName, string itemNameColumn)
        {
            try
            {
                List<string> similarNames = new List<string>();
                string itemNewName = null;

                itemName = itemName.Trim();

                // Check if the name already exists in the Database. If exists, add Copy to the name
                string query = "select " + itemNameColumn + " from " + itemTableName + " where " + itemNameColumn + " like '%' + @ItemName + '%'";

                //DataTable dataTable = new DataTable();
                //DataTableReader dataTableReader = null;
                //SqlDataAdapter dataAdapter = new SqlDataAdapter(query, connect);
                //dataAdapter.Fill(dataTable);

                DataTable dataTable = SelectDTWithParameter(query, "@ItemName", itemName);
                DataTableReader dataTableReader = dataTable.CreateDataReader();

                if (dataTableReader.HasRows)
                {
                    while (dataTableReader.Read())
                    {
                        similarNames.Add(dataTableReader.GetString(0).ToString());
                    }
                }

                itemNewName = itemName;
                if (similarNames.Contains(itemNewName, StringComparer.CurrentCultureIgnoreCase))
                {
                    itemNewName = itemNewName + " - Copy";
                    if (similarNames.Contains(itemNewName, StringComparer.CurrentCultureIgnoreCase))
                    {
                        for (int i = 2; ; i++)
                        {
                            if (!similarNames.Contains(itemNewName + " (" + i + ")", StringComparer.CurrentCultureIgnoreCase))
                            {
                                itemNewName = itemNewName + " (" + i + ")";
                                break;
                            }

                        }
                    }
                }
                return itemNewName;
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif

                //MessageBox.Show("Exception\n \n" + ex.Message, "QAT Error Code - EC02019", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }

        public string CheckNameExists(string itemName, string itemTableName, string itemNameColumn)
        {
            SqlCommand cmd = new SqlCommand();
            try
            {
                CreateConnection();
                OpenConnection();

                itemName = itemName.Trim();

                //// Check if the name already exists in the Database.

                string query = "select " + itemNameColumn + " from " + itemTableName + " where " + itemNameColumn + " = @ItemName";

                cmd = new SqlCommand(query, connect);
                cmd.Parameters.AddWithValue("@ItemName", itemName);
                DataTable dataTable = new DataTable();
                DataTableReader dataTableReader = null;
                SqlDataAdapter dataAdapter = new SqlDataAdapter(cmd);
                dataAdapter.Fill(dataTable);
                dataTableReader = dataTable.CreateDataReader();

                if (dataTableReader.HasRows)
                {
                    while (dataTableReader.Read())
                    {
                        if (String.Equals(dataTableReader.GetString(0).ToString(), itemName, StringComparison.CurrentCultureIgnoreCase))
                        {
                            return itemTableName;
                        }
                    }
                }

                //// Check if the name already exists in the Database.
                //string query = "select " + itemNameColumn + " from " + itemTableName + " where " + itemNameColumn + " = '" + itemName + "'";

                //DataTable dataTable = new DataTable();
                //DataTableReader dataTableReader = null;
                //SqlDataAdapter dataAdapter = new SqlDataAdapter(query, connect);
                //dataAdapter.Fill(dataTable);
                //dataTableReader = dataTable.CreateDataReader();

                //if (dataTableReader.HasRows)
                //{
                //    while (dataTableReader.Read())
                //    {
                //        if (String.Equals(dataTableReader.GetString(0).ToString(), itemName, StringComparison.CurrentCultureIgnoreCase))
                //        {
                //                return itemTableName;
                //        }
                //    }
                //}
                CloseConnection();
                return null;
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif

                //MessageBox.Show("Exception\n \n" + ex.Message, "QAT Error Code - EC02020", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }

        public string ChkNameExistinInventoryList(string selectedText)
        {
            try
            {
                string inventoryExist = null;
                inventoryExist = CheckNameExists(selectedText, QatConstants.DbTestSuiteTable, QatConstants.DbTestSuiteNameColumn);
                if (inventoryExist == null)
                {
                    inventoryExist = CheckNameExists(selectedText, QatConstants.DbTestPlanTable, QatConstants.DbTestPlanNameColumn);

                    if (inventoryExist == null)
                    {
                        inventoryExist = CheckNameExists(selectedText, QatConstants.DbTestCaseTable, QatConstants.DbTestCaseNameColumn);
                    }
                }

                return inventoryExist;
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif

                //MessageBox.Show("Exception\n \n" + ex.Message, "QAT Error Code - ECxxxx", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }

        }

        public List<DUT_DeviceItem> GetDesignDetails(List<TreeViewExplorer> testPlanList)
        {
            try
            {
                CreateConnection();
                OpenConnection();

                List<DUT_DeviceItem> dutDeviceItemList = new List<DUT_DeviceItem>();

                foreach (TreeViewExplorer item in testPlanList)
                {
                    string query = "select * from DesignInventory where DesignID in(select DesignID from TPDesignLinkTable where TPID = ('" + item.ItemKey + "'))";

                    DataTable dataTable = new DataTable();
                    DataTableReader dataTableReader = null;
                    SqlDataAdapter dataAdapter = new SqlDataAdapter(query, connect);
                    dataAdapter.Fill(dataTable);
                    dataTableReader = dataTable.CreateDataReader();
                    
                    if (dataTableReader.HasRows)
                    {
                        while (dataTableReader.Read())
                        {
                            DUT_DeviceItem deviceItem = new DUT_DeviceItem();
                            deviceItem.ItemDeviceType = dataTableReader.GetString(1).ToString();

                            if (!deviceItem.ItemDeviceType.ToLower().Contains("camera"))
                            {
                                deviceItem.ItemDeviceModel = dataTableReader.GetString(2).ToString();
                                deviceItem.ItemDeviceName = dataTableReader.GetString(3).ToString();
                                if (dataTableReader[4] != System.DBNull.Value)
                                    deviceItem.ItemPrimaryorBackup = dataTableReader.GetString(4).ToString();
                                if (dataTableReader[5] != System.DBNull.Value)
                                    deviceItem.Itemlinked = dataTableReader.GetString(5).ToString();

                                deviceItem.ParentTestPlanTreeView = item;
                                if (item.Parent != null)
                                {
                                    deviceItem.TestSuiteTreeViewList.Add(item.Parent);
                                    deviceItem.TestSuiteNameList = item.Parent.ItemName;
                                }

                                string strResponse = string.Empty;

                                //    HttpGet("http://" + "172.16.4.72" + "/cgi-bin/id_mode?mode", string.Empty, "EC15011-ID", "172.16.4.72", out strResponse);
                                //if (strResponse == "mode=off")
                                //{
                                //    deviceItem.blnIDColor = "Black";
                                //}
                                //else
                                //{
                                //    deviceItem.blnIDColor = "Red";
                                //}

                                //////ItemNetPairingListForXAML used in combobox binding
                                deviceItem.ItemNetPairingList.Add("Not Applicable", "Localdevice");
                                deviceItem.ItemNetPairingList_duplicate.Add("Not Applicable", "Localdevice");
                                deviceItem.ItemNetPairingListForXAML.Add("Not Applicable");
                                //deviceItem.ItemNetPairingListWithQREM.Add("Not Applicable", false);
                                deviceItem.ItemPrimaryIPList.Add("Not Applicable");
                                deviceItem.ItemSecondaryIPList.Add("Not Applicable");
                                //deviceItem.VideoGen = "HI";

                                dutDeviceItemList.Add(deviceItem);
                            }
                        }
                    }
                }

                CloseConnection();
                return dutDeviceItemList;
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                //MessageBox.Show("Exception\n \n" + ex.Message, "QAT Error Code - EC02021", MessageBoxButton.OK, MessageBoxImage.Error);
                return new List<DUT_DeviceItem>();
            }
        }

        public void GetDesignComponentDetails(TestCaseItem sourceTestCaseItem)
        {
            try
            {
                CreateConnection();
                OpenConnection();

                string query = "select * from designtable where DesignID in(select DesignID from TPDesignLinkTable where TPID = ('" + sourceTestCaseItem.TestPlanSelected.ItemKey + "'))";
                DataTable dataTable = new DataTable();
                DataTableReader dataTableReader = null;
                SqlDataAdapter dataAdapter = new SqlDataAdapter(query, connect);
                dataAdapter.Fill(dataTable);
                dataTableReader = dataTable.CreateDataReader();

                if (dataTableReader.HasRows)
                {
                    while (dataTableReader.Read())
                    {
                        sourceTestCaseItem.DesignIDSelected = dataTableReader.GetInt32(0);
                        sourceTestCaseItem.DesignNameSelected = dataTableReader.GetString(1).ToString();
                    }
                }

                //query = "Select * from (select *, row_number() OVER(PARTITION BY ComponentName,ComponentType, Control, PrettyName ORDER BY ComponentName,ComponentType, Control, PrettyName DESC) as repeatRowCount from TCInitialization where DesignID = " + sourceTestCaseItem.DesignIDSelected + " and (ControlDirection like '%control_direction_external_read%' or ControlDirection like '%control_direction_external_write%') and Control not like '%%metadata%' and PrettyName is not null and PrettyName != '' and PrettyName not like '%<%') as a where a.repeatRowCount = 1";

                query = "select * from TCInitialization where (designid=('" + sourceTestCaseItem.DesignIDSelected + "') and (ControlDirection like '%control_direction_external_read%' or ControlDirection like '%control_direction_external_write%') and Control not like '%%metadata%' and PrettyName is not null and PrettyName != '' and PrettyName not like '<%>') order by PrettyName asc";//CONVERT(INT,SUBSTRING(prettyname,PATINDEX(' %[0-9]%',prettyname),LEN(prettyname)))
                dataTable = new DataTable();
                dataTableReader = null;
                dataAdapter = new SqlDataAdapter(query, connect);
                dataAdapter.Fill(dataTable);

                if (dataTable.Rows.Count > 0)
                {
                    var duplicaDT = dataTable.AsEnumerable().GroupBy(row => new { Compname = row.Field<string>("ComponentName"), compType = row.Field<string>("ComponentType"), control = row.Field<string>("Control"), prettyName = row.Field<string>("PrettyName"), controlDirect = row.Field<string>("ControlDirection") }).Select(group => group.First()).CopyToDataTable();
                    dataTable = duplicaDT;
                }

                dataTableReader = dataTable.CreateDataReader();

                sourceTestCaseItem.ComponentTypeList.Clear();
                sourceTestCaseItem.ComponentNameDictionary.Clear();
                sourceTestCaseItem.ControlNameDictionary.Clear();
                sourceTestCaseItem.ControlTypeDictionary.Clear();
                sourceTestCaseItem.ControlInitialValueDictionary.Clear();
                sourceTestCaseItem.ControlInitialStringDictionary.Clear();
                sourceTestCaseItem.ControlInitialPositionDictionary.Clear();
                sourceTestCaseItem.ControlIDDictionary.Clear();
                sourceTestCaseItem.VerifyControlNameDictionary.Clear();
                sourceTestCaseItem.dataTypeDictionary.Clear();
                sourceTestCaseItem.VerifyDataTypeDictionary.Clear();
                sourceTestCaseItem.ChannelSelectionDictionary.Clear();
                sourceTestCaseItem.channelControlTypeDictionary.Clear();
                sourceTestCaseItem.VerifychannelControlTypeDictionary.Clear();
                sourceTestCaseItem.MaximumControlValueDictionary.Clear();
                sourceTestCaseItem.MinimumControlValueDictionary.Clear();
                sourceTestCaseItem.ResponsalyzerNameList.Clear();
                sourceTestCaseItem.ResponsalyzerTypeList.Clear();
                sourceTestCaseItem.UsbAudioDeviceList.Clear();
                sourceTestCaseItem.UsbAudioBridgeList.Clear();     

                if (dataTableReader.HasRows)
                {
                    while (dataTableReader.Read())
                    {
                        string componentType = dataTableReader.GetString(1).ToString();
                        string componentName = dataTableReader.GetString(2).ToString();
                        string controlName = string.Empty;
                        if (dataTableReader[14] != System.DBNull.Value)
                        {
                            controlName = dataTableReader.GetString(14).ToString();
                        }

                        ///////Get repeated Read and write count:
                        int writeDuplicateCount = 0;
                        int readDuplicateCount = 0;

                        string controlID = string.Empty;
                        if (dataTableReader[3] != System.DBNull.Value)
                        {
                            controlID = dataTableReader.GetString(3).ToString();
                        }

                        try
                        {
                            string componentName1 = componentName.Replace("'", "''");
                            string componentType1 = componentType.Replace("'", "''");
                            string controlName1 = controlName.Replace("'", "''");

                            DataRow[] writeresults = dataTable.Select("ComponentName = '" + componentName1 + "' AND ComponentType = '" + componentType1 + "' AND PrettyName = '" + controlName1 + "'");

                            if (writeresults != null && writeresults.Count() > 1)
                            {
                                DataRow[] writeresultss = dataTable.Select("ComponentName = '" + componentName1 + "' AND ComponentType = '" + componentType1 + "' AND PrettyName = '" + controlName1 + "'AND ControlDirection like '%control_direction_external_write%'");
                                if (writeresultss != null)
                                {
                                    if (writeresultss.Count() > 1)
                                    {
                                        var result = dataTable.Select("ComponentName = '" + componentName1 + "' AND ComponentType = '" + componentType1 + "' AND PrettyName = '" + controlName1 + "'AND ControlDirection like '%control_direction_external_write%' AND Control='" + controlID + "'");
                                        if (result.Count() == writeresultss.Count())
                                            writeDuplicateCount = 1;
                                        else
                                            writeDuplicateCount = writeresultss.Count();
                                    }
                                    else
                                    {
                                        writeDuplicateCount = writeresultss.Count();
                                    }
                                }

                                DataRow[] readresultss = dataTable.Select("ComponentName = '" + componentName1 + "' AND ComponentType = '" + componentType1 + "' AND PrettyName = '" + controlName1 + "'AND ControlDirection like '%control_direction_external_read%'");
                                if (readresultss != null)
                                {
                                    if (readresultss.Count() > 1)
                                    {
                                        var result = dataTable.Select("ComponentName = '" + componentName1 + "' AND ComponentType = '" + componentType1 + "' AND PrettyName = '" + controlName1 + "'AND ControlDirection like '%control_direction_external_read%' AND Control='" + controlID + "'");

                                        if (result.Count() == readresultss.Count())
                                            readDuplicateCount = 1;
                                        else
                                            readDuplicateCount = readresultss.Count();
                                    }
                                    else
                                    {
                                        readDuplicateCount = readresultss.Count();
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {

                        }

                        //DataRow[] writeresults = dataTable.Select("ComponentName = '" + componentName + "' AND ComponentType = '" + componentType + "' AND PrettyName = '" + controlName + "'");

                        //if (writeresults != null && writeresults.Count() > 1)
                        //{
                        //    DataRow[] writeresultss = dataTable.Select("ComponentName = '" + componentName + "' AND ComponentType = '" + componentType + "' AND PrettyName = '" + controlName + "'AND ControlDirection like '%control_direction_external_write%'");
                        //    if (writeresultss != null)
                        //        writeDuplicateCount = writeresultss.Count();

                        //    DataRow[] readresultss = dataTable.Select("ComponentName = '" + componentName + "' AND ComponentType = '" + componentType + "' AND PrettyName = '" + controlName + "'AND ControlDirection like '%control_direction_external_read%'");
                        //    if (readresultss != null)
                        //        readDuplicateCount = readresultss.Count();
                        //}

                        string controlDirection = string.Empty;
                        ObservableCollection<string> responsalyzerType = new ObservableCollection<string> { "Frequency Vs Magnitude", "Frequency Vs Phase" };

                        if (string.Equals("responsalyzer", componentType, StringComparison.CurrentCultureIgnoreCase))
                        {
                            if (!sourceTestCaseItem.ResponsalyzerNameList.Contains(componentName))
                                sourceTestCaseItem.ResponsalyzerNameList.Add(componentName);
                            foreach (string typeName in responsalyzerType)
                            {
                                if (!sourceTestCaseItem.ResponsalyzerTypeList.Contains(typeName))
                                    sourceTestCaseItem.ResponsalyzerTypeList.Add(typeName);
                            }
                        }

                        string controlType = string.Empty;
                        if (dataTableReader[15] != System.DBNull.Value)
                        {
                            controlType = dataTableReader.GetString(15).ToString();
                        }

                        string[] BridgeName = null;
                        string actualbridgename = string.Empty;
                        string displayusbname = string.Empty;
                        if (dataTableReader[19] != System.DBNull.Value)
                        {
                            string prettyName = dataTableReader.GetString(19).ToString();
                            BridgeName = prettyName.Split('\n');
                            if (prettyName.Trim() != string.Empty)
                            {
                                if (BridgeName.Count() == 1)
                                {
                                    if (dataTableReader[20] != System.DBNull.Value)
                                    {
                                        string NetworkName = dataTableReader.GetString(20).ToString();
                                        if (NetworkName.Trim() != string.Empty)
                                        {
                                            string[] test = prettyName.Split(new string[] { NetworkName }, StringSplitOptions.None);
                                            displayusbname = test[0].Trim();
                                            actualbridgename = NetworkName;
                                        }
                                        else
                                        {
                                            string[] test1 = prettyName.Split(new string[] { " In" }, StringSplitOptions.None);
                                            if (test1.Count() > 1)
                                            {
                                                displayusbname = test1[0] + " In";
                                                actualbridgename = test1[1];
                                            }
                                            string[] test2 = prettyName.Split(new string[] { " Out" }, StringSplitOptions.None);
                                            if (test2.Count() > 1)
                                            {
                                                displayusbname = test2[0] + " Out";
                                                actualbridgename = test2[1];
                                            }
                                        }
                                    }

                                    // BridgeName = prettyName.Split(new string[] { "In" }, StringSplitOptions.None);
                                    //  BridgeName[0] = BridgeName[0] + " In"; 
                                }
                                else if (BridgeName.Count() > 1)
                                {
                                    actualbridgename = BridgeName[1];
                                    if (dataTableReader[20] != System.DBNull.Value)
                                    {
                                        string NetworkName = dataTableReader.GetString(20).ToString();
                                        if (NetworkName.Trim() != string.Empty)
                                        {
                                            string[] test = BridgeName[0].Split(new string[] { NetworkName }, StringSplitOptions.None);
                                            displayusbname = test[0].Trim();
                                            //actualbridgename = BridgeName[1].Trim();
                                        }
                                        else
                                        {
                                            string[] test1 = prettyName.Split(new string[] { " In" }, StringSplitOptions.None);
                                            if (test1.Count() > 1)
                                            {
                                                displayusbname = test1[0] + " In";
                                                //actualbridgename = BridgeName[1].Trim();
                                            }
                                            string[] test2 = prettyName.Split(new string[] { " Out" }, StringSplitOptions.None);
                                            if (test2.Count() > 1)
                                            {
                                                displayusbname = test2[0] + " Out";
                                                //actualbridgename = BridgeName[1].Trim();
                                            }
                                        }
                                    }
                                }
                            }
                            //if (BridgeName.Count() == 1)
                            //{
                            // BridgeName = prettyName.Split(new string[] { "Out" }, StringSplitOptions.None);
                            // BridgeName[0] = BridgeName[0] + " Out";
                            //}
                        }
                        if (dataTableReader[18] != System.DBNull.Value)
                        {
                            string CheckTR = dataTableReader.GetString(18).ToString();
                            if (CheckTR.StartsWith("usb_receiver"))
                            {
                                if (!sourceTestCaseItem.UsbAudioDeviceList.Contains(new Tuple<string, string>(displayusbname, actualbridgename)))
                                {
                                    sourceTestCaseItem.UsbAudioDeviceList.Add(new Tuple<string, string>(displayusbname, "Recording"));
                                    sourceTestCaseItem.UsbAudioBridgeList.Add(new Tuple<string, string>(displayusbname, actualbridgename));

                                    //}
                                }
                            }
                            if (CheckTR.StartsWith("usb_transmitter"))
                            {
                                if (!sourceTestCaseItem.UsbAudioDeviceList.Contains(new Tuple<string, string>(displayusbname, actualbridgename)))
                                {
                                    sourceTestCaseItem.UsbAudioDeviceList.Add(new Tuple<string, string>(displayusbname, "Playback"));
                                    sourceTestCaseItem.UsbAudioBridgeList.Add(new Tuple<string, string>(displayusbname, actualbridgename));
                                }
                            }
                            CheckTR = string.Empty;
                        }


                        string min_ControlValue = string.Empty;
                        if (dataTableReader[6] != System.DBNull.Value)
                        {
                            min_ControlValue = dataTableReader.GetString(6).ToString();
                        }

                        string max_ControlValue = string.Empty;
                        if (dataTableReader[7] != System.DBNull.Value)
                        {
                            max_ControlValue = dataTableReader.GetString(7).ToString();
                        }

                        string controlInitialValue = string.Empty;
                        if (dataTableReader[11] != System.DBNull.Value)
                        {
                            controlInitialValue = dataTableReader.GetString(11).ToString();
                        }

                        string controlInitialString = string.Empty;
                        if (dataTableReader[12] != System.DBNull.Value)
                        {
                            controlInitialString = dataTableReader.GetString(12).ToString();
                        }

                        string controlInitialPosition = string.Empty;
                        if (dataTableReader[13] != System.DBNull.Value)
                        {
                            controlInitialPosition = dataTableReader.GetString(13).ToString();
                        }
                      //  string controlID = string.Empty;
                      //  if (dataTableReader[3] != System.DBNull.Value)
                      //  {
                      //      controlID = dataTableReader.GetString(3).ToString();
                      //  }
                        if (!sourceTestCaseItem.ComponentTypeList.Contains(componentType))
                            sourceTestCaseItem.ComponentTypeList.Add(componentType);

                        if (!sourceTestCaseItem.ComponentNameDictionary.Keys.Contains(componentType))
                            sourceTestCaseItem.ComponentNameDictionary.Add(componentType, new ObservableCollection<string>());

                        if (!sourceTestCaseItem.ControlNameDictionary.Keys.Contains(componentName))
                        {
                            sourceTestCaseItem.ControlNameDictionary.Add(componentName, new ObservableCollection<string>());
                        }

                        //if (!sourceTestCaseItem.ChannelSelectionDictionary.Keys.Contains(componentName))
                        //    sourceTestCaseItem.ChannelSelectionDictionary.Add(componentName, new ObservableCollection<string>());


                        if (!sourceTestCaseItem.VerifyControlNameDictionary.Keys.Contains(componentName))
                            sourceTestCaseItem.VerifyControlNameDictionary.Add(componentName, new ObservableCollection<string>());

                        //if (!sourceTestCaseItem.ControlTypeDictionary.Keys.Contains(componentName))
                        //    sourceTestCaseItem.ControlTypeDictionary.Add(componentName, new ObservableCollection<string>());

                        if (!sourceTestCaseItem.ComponentNameDictionary[componentType].Contains(componentName))
                            sourceTestCaseItem.ComponentNameDictionary[componentType].Add(componentName);

                        if (dataTableReader[17] != System.DBNull.Value)
                        {
                            controlDirection = dataTableReader.GetString(17).ToString();

                            #region Write
                            if (controlDirection.Contains("control_direction_external_write"))// | controlDirection.Contains("control_direction_ramp")| controlDirection.Contains("control_direction_snapshot"))
                            {
                                string controlNameduplicate = controlName;

                                string[] Channel_split = new string[2];
                                string channelControl = string.Empty;

                                if (controlNameduplicate.Contains("~")) /*(((controlName.Contains("Channel")) || (controlName.Contains("Output")) || (controlName.Contains("Input")) || (controlName.Contains("Tap")) || (controlName.Contains("Bank Control")) || (controlName.Contains("Bank Select")) || (controlName.Contains("GPIO"))) & (controlName.Contains("~")))*/
                                {
                                    int tiltCount = controlNameduplicate.Count(x => x == '~');
                                    string channelWithTwoTilt = controlNameduplicate;
                                    int idx = channelWithTwoTilt.LastIndexOf('~');
                                    Channel_split[0] = channelWithTwoTilt.Substring(0, idx);
                                    Channel_split[1] = channelWithTwoTilt.Substring(idx + 1);

                                    string QATPrefix = addQATPrefixToControl(controlNameduplicate);
                                    if (!string.IsNullOrEmpty(QATPrefix))
                                        channelControl = QATPrefix + Channel_split[1];
                                }

                                if (!(string.IsNullOrEmpty(channelControl)))
                                {
                                    if (!sourceTestCaseItem.channelControlTypeDictionary.Keys.Contains(componentName + channelControl))
                                        sourceTestCaseItem.channelControlTypeDictionary.Add(componentName + channelControl, controlDirection);

                                    if (!sourceTestCaseItem.ControlTypeDictionary.Keys.Contains(componentName + controlNameduplicate))
                                        sourceTestCaseItem.ControlTypeDictionary.Add(componentName + controlNameduplicate, controlDirection);

                                    if ((!sourceTestCaseItem.dataTypeDictionary.Keys.Contains(componentName + controlNameduplicate + controlID)))
                                        sourceTestCaseItem.dataTypeDictionary.Add(componentName + controlNameduplicate + controlID, controlType);

                                    if ((!sourceTestCaseItem.ControlNameDictionary[componentName].Contains(channelControl)))
                                        sourceTestCaseItem.ControlNameDictionary[componentName].Add(channelControl);

                                    if (!sourceTestCaseItem.ChannelSelectionDictionary.Keys.Contains(componentName + channelControl))
                                        sourceTestCaseItem.ChannelSelectionDictionary.Add(componentName + channelControl, new ObservableCollection<string>());

                                    if (writeDuplicateCount > 1)
                                    {
                                        if ((!sourceTestCaseItem.ChannelSelectionDictionary[componentName + channelControl].Contains(Channel_split[0] + " [" + controlID + "]")))
                                            sourceTestCaseItem.ChannelSelectionDictionary[componentName + channelControl].Add(Channel_split[0] + " [" + controlID + "]");

                                        if (!sourceTestCaseItem.ControlIDDictionary.Keys.Contains(componentName + channelControl + Channel_split[0] + " [" + controlID + "]"))
                                            sourceTestCaseItem.ControlIDDictionary.Add(componentName + channelControl + Channel_split[0] + " [" + controlID + "]", new string[2]);

                                        string[] prettyControlAry = new string[] { Channel_split[0], controlID };
                                        if (sourceTestCaseItem.ControlIDDictionary[componentName + channelControl + Channel_split[0] + " [" + controlID + "]"] != (prettyControlAry))
                                            sourceTestCaseItem.ControlIDDictionary[componentName + channelControl + Channel_split[0] + " [" + controlID + "]"] = prettyControlAry;
                                    }
                                    else
                                    {
                                        if (!sourceTestCaseItem.ChannelSelectionDictionary[componentName + channelControl].Contains(Channel_split[0]))
                                            sourceTestCaseItem.ChannelSelectionDictionary[componentName + channelControl].Add(Channel_split[0]);

                                        if (!sourceTestCaseItem.ControlIDDictionary.Keys.Contains(componentName + channelControl + Channel_split[0]))
                                            sourceTestCaseItem.ControlIDDictionary.Add(componentName + channelControl + Channel_split[0], new string[2]);

                                        string[] prettyControlID = new string[] { Channel_split[0], controlID };
                                        if (sourceTestCaseItem.ControlIDDictionary[componentName + channelControl + Channel_split[0]] != prettyControlID)
                                            sourceTestCaseItem.ControlIDDictionary[componentName + channelControl + Channel_split[0]] = prettyControlID;
                                    }

                                    if (!sourceTestCaseItem.ControlInitialValueDictionary.Keys.Contains(componentName + controlNameduplicate + controlID))
                                        sourceTestCaseItem.ControlInitialValueDictionary.Add(componentName + controlNameduplicate + controlID, controlInitialValue);

                                    if (!sourceTestCaseItem.ControlInitialStringDictionary.Keys.Contains(componentName + controlNameduplicate + controlID))
                                        sourceTestCaseItem.ControlInitialStringDictionary.Add(componentName + controlNameduplicate + controlID, controlInitialString);

                                    if (!sourceTestCaseItem.ControlInitialPositionDictionary.Keys.Contains(componentName + controlNameduplicate + controlID))
                                        sourceTestCaseItem.ControlInitialPositionDictionary.Add(componentName + controlNameduplicate + controlID, controlInitialPosition);

                                    if (!sourceTestCaseItem.MinimumControlValueDictionary.Keys.Contains(componentName + controlNameduplicate + controlID))
                                        sourceTestCaseItem.MinimumControlValueDictionary.Add(componentName + controlNameduplicate + controlID, min_ControlValue);

                                    if (!sourceTestCaseItem.MaximumControlValueDictionary.Keys.Contains(componentName + controlNameduplicate + controlID))
                                        sourceTestCaseItem.MaximumControlValueDictionary.Add(componentName + controlNameduplicate + controlID, max_ControlValue);
                                }
                                else
                                {
                                    if (writeDuplicateCount > 1)
                                        controlNameduplicate = controlName + " [" + controlID + "]";

                                    if (!sourceTestCaseItem.ControlTypeDictionary.Keys.Contains(componentName + controlNameduplicate))
                                        sourceTestCaseItem.ControlTypeDictionary.Add(componentName + controlNameduplicate, controlDirection);

                                    if ((!sourceTestCaseItem.dataTypeDictionary.Keys.Contains(componentName + controlNameduplicate + controlID)))
                                        sourceTestCaseItem.dataTypeDictionary.Add(componentName + controlNameduplicate + controlID, controlType);

                                    if ((!sourceTestCaseItem.ControlNameDictionary[componentName].Contains(controlNameduplicate)))
                                        sourceTestCaseItem.ControlNameDictionary[componentName].Add(controlNameduplicate);

                                    if (!sourceTestCaseItem.ControlInitialValueDictionary.Keys.Contains(componentName + controlNameduplicate))
                                        sourceTestCaseItem.ControlInitialValueDictionary.Add(componentName + controlNameduplicate, controlInitialValue);

                                    if (!sourceTestCaseItem.ControlInitialStringDictionary.Keys.Contains(componentName + controlNameduplicate))
                                        sourceTestCaseItem.ControlInitialStringDictionary.Add(componentName + controlNameduplicate, controlInitialString);

                                    if (!sourceTestCaseItem.ControlInitialPositionDictionary.Keys.Contains(componentName + controlNameduplicate))
                                        sourceTestCaseItem.ControlInitialPositionDictionary.Add(componentName + controlNameduplicate, controlInitialPosition);

                                    if (!sourceTestCaseItem.MinimumControlValueDictionary.Keys.Contains(componentName + controlNameduplicate + controlID))
                                        sourceTestCaseItem.MinimumControlValueDictionary.Add(componentName + controlNameduplicate + controlID, min_ControlValue);

                                    if (!sourceTestCaseItem.MaximumControlValueDictionary.Keys.Contains(componentName + controlNameduplicate + controlID))
                                        sourceTestCaseItem.MaximumControlValueDictionary.Add(componentName + controlNameduplicate + controlID, max_ControlValue);

                                    if (!sourceTestCaseItem.ControlIDDictionary.Keys.Contains(componentName + controlNameduplicate))
                                        sourceTestCaseItem.ControlIDDictionary.Add(componentName + controlNameduplicate, new string[2]);

                                    string[] prettyControlAry = new string[] { controlName, controlID };
                                    if (sourceTestCaseItem.ControlIDDictionary[componentName + controlNameduplicate] != prettyControlAry)
                                        sourceTestCaseItem.ControlIDDictionary[componentName + controlNameduplicate] = prettyControlAry;
                                }
                            }

                            #endregion

                            #region Read
                            if (controlDirection.Contains("control_direction_external_read"))// | controlDirection.Contains("control_direction_ramp")| controlDirection.Contains("control_direction_snapshot"))
                            {
                                string controlNameduplicate = controlName;

                                string channelControl = string.Empty;
                                string[] Channel_split = new string[2];

                                if (controlNameduplicate.Contains("~"))
                                {
                                    int tiltCount = controlNameduplicate.Count(x => x == '~');
                                    string channelWithTwoTilt = controlNameduplicate;
                                    int idx = channelWithTwoTilt.LastIndexOf('~');
                                    Channel_split[0] = channelWithTwoTilt.Substring(0, idx);
                                    Channel_split[1] = channelWithTwoTilt.Substring(idx + 1);
                                    string QATPrefix = addQATPrefixToControl(controlNameduplicate);
                                    if (!string.IsNullOrEmpty(QATPrefix))
                                        channelControl = QATPrefix + Channel_split[1];
                                }

                                if (!(string.IsNullOrEmpty(channelControl)))
                                {
                                    if (!sourceTestCaseItem.VerifychannelControlTypeDictionary.Keys.Contains(componentName + channelControl))
                                        sourceTestCaseItem.VerifychannelControlTypeDictionary.Add(componentName + channelControl, controlDirection);

                                    if (!sourceTestCaseItem.VerifyControlTypeDictionary.Keys.Contains(componentName + controlNameduplicate))
                                        sourceTestCaseItem.VerifyControlTypeDictionary.Add(componentName + controlNameduplicate, controlDirection);

                                    if ((!sourceTestCaseItem.VerifyDataTypeDictionary.Keys.Contains(componentName + controlNameduplicate + controlID)))
                                        sourceTestCaseItem.VerifyDataTypeDictionary.Add(componentName + controlNameduplicate + controlID, controlType);

                                    if ((!sourceTestCaseItem.VerifyControlNameDictionary[componentName].Contains(channelControl)))
                                        sourceTestCaseItem.VerifyControlNameDictionary[componentName].Add(channelControl);

                                    if (!sourceTestCaseItem.ChannelSelectionDictionary.Keys.Contains(componentName + channelControl))
                                        sourceTestCaseItem.ChannelSelectionDictionary.Add(componentName + channelControl, new ObservableCollection<string>());

                                    if (readDuplicateCount > 1)
                                    {
                                        if ((!sourceTestCaseItem.ChannelSelectionDictionary[componentName + channelControl].Contains(Channel_split[0] + " [" + controlID + "]")))
                                            sourceTestCaseItem.ChannelSelectionDictionary[componentName + channelControl].Add(Channel_split[0] + " [" + controlID + "]");

                                        if (!sourceTestCaseItem.ControlIDDictionary.Keys.Contains(componentName + channelControl + Channel_split[0] + " [" + controlID + "]"))
                                            sourceTestCaseItem.ControlIDDictionary.Add(componentName + channelControl + Channel_split[0] + " [" + controlID + "]", new string[2]);

                                        string[] array = new string[] { Channel_split[0], controlID };
                                        if (sourceTestCaseItem.ControlIDDictionary[componentName + channelControl + Channel_split[0] + " [" + controlID + "]"] != (array))
                                            sourceTestCaseItem.ControlIDDictionary[componentName + channelControl + Channel_split[0] + " [" + controlID + "]"] = array;
                                    }
                                    else
                                    {
                                        if ((!sourceTestCaseItem.ChannelSelectionDictionary[componentName + channelControl].Contains(Channel_split[0])))
                                            sourceTestCaseItem.ChannelSelectionDictionary[componentName + channelControl].Add(Channel_split[0]);

                                        if (!sourceTestCaseItem.ControlIDDictionary.Keys.Contains(componentName + channelControl + Channel_split[0]))
                                            sourceTestCaseItem.ControlIDDictionary.Add(componentName + channelControl + Channel_split[0], new string[2]);

                                        string[] prettyControlID = new string[] { Channel_split[0], controlID };
                                        if (sourceTestCaseItem.ControlIDDictionary[componentName + channelControl + Channel_split[0]] != prettyControlID)
                                            sourceTestCaseItem.ControlIDDictionary[componentName + channelControl + Channel_split[0]] = prettyControlID;
                                    }

                                    if (!sourceTestCaseItem.ControlInitialValueDictionary.Keys.Contains(componentName + controlNameduplicate + controlID))
                                        sourceTestCaseItem.ControlInitialValueDictionary.Add(componentName + controlNameduplicate + controlID, controlInitialValue);

                                    if (!sourceTestCaseItem.ControlInitialStringDictionary.Keys.Contains(componentName + controlNameduplicate + controlID))
                                        sourceTestCaseItem.ControlInitialStringDictionary.Add(componentName + controlNameduplicate + controlID, controlInitialString);

                                    if (!sourceTestCaseItem.ControlInitialPositionDictionary.Keys.Contains(componentName + controlNameduplicate + controlID))
                                        sourceTestCaseItem.ControlInitialPositionDictionary.Add(componentName + controlNameduplicate + controlID, controlInitialPosition);

                                    if (!sourceTestCaseItem.MinimumControlValueDictionary.Keys.Contains(componentName + controlNameduplicate + controlID))
                                        sourceTestCaseItem.MinimumControlValueDictionary.Add(componentName + controlNameduplicate + controlID, min_ControlValue);

                                    if (!sourceTestCaseItem.MaximumControlValueDictionary.Keys.Contains(componentName + controlNameduplicate + controlID))
                                        sourceTestCaseItem.MaximumControlValueDictionary.Add(componentName + controlNameduplicate + controlID, max_ControlValue);
                                }
                                else
                                {
                                    if (readDuplicateCount > 1)
                                        controlNameduplicate = controlName + " [" + controlID + "]";

                                    if (!sourceTestCaseItem.VerifyControlTypeDictionary.Keys.Contains(componentName + controlNameduplicate))
                                        sourceTestCaseItem.VerifyControlTypeDictionary.Add(componentName + controlNameduplicate, controlDirection);

                                    if (!sourceTestCaseItem.VerifyDataTypeDictionary.Keys.Contains(componentName + controlNameduplicate + controlID))
                                        sourceTestCaseItem.VerifyDataTypeDictionary.Add(componentName + controlNameduplicate + controlID, controlType);

                                    if ((!sourceTestCaseItem.VerifyControlNameDictionary[componentName].Contains(controlNameduplicate)))
                                        sourceTestCaseItem.VerifyControlNameDictionary[componentName].Add(controlNameduplicate);

                                    if (!sourceTestCaseItem.ControlInitialValueDictionary.Keys.Contains(componentName + controlNameduplicate))
                                        sourceTestCaseItem.ControlInitialValueDictionary.Add(componentName + controlNameduplicate, controlInitialValue);

                                    if (!sourceTestCaseItem.ControlInitialStringDictionary.Keys.Contains(componentName + controlNameduplicate))
                                        sourceTestCaseItem.ControlInitialStringDictionary.Add(componentName + controlNameduplicate, controlInitialString);

                                    if (!sourceTestCaseItem.ControlInitialPositionDictionary.Keys.Contains(componentName + controlNameduplicate))
                                        sourceTestCaseItem.ControlInitialPositionDictionary.Add(componentName + controlNameduplicate, controlInitialPosition);

                                    if (!sourceTestCaseItem.MinimumControlValueDictionary.Keys.Contains(componentName + controlNameduplicate + controlID))
                                        sourceTestCaseItem.MinimumControlValueDictionary.Add(componentName + controlNameduplicate + controlID, min_ControlValue);

                                    if (!sourceTestCaseItem.MaximumControlValueDictionary.Keys.Contains(componentName + controlNameduplicate + controlID))
                                        sourceTestCaseItem.MaximumControlValueDictionary.Add(componentName + controlNameduplicate + controlID, max_ControlValue);

                                    if (!sourceTestCaseItem.ControlIDDictionary.Keys.Contains(componentName + controlNameduplicate))
                                        sourceTestCaseItem.ControlIDDictionary.Add(componentName + controlNameduplicate, new string[2]);

                                    string[] prettyControlID = new string[] { controlName, controlID };
                                    if (sourceTestCaseItem.ControlIDDictionary[componentName + controlNameduplicate] != prettyControlID)
                                        sourceTestCaseItem.ControlIDDictionary[componentName + controlNameduplicate] = prettyControlID;
                                }
                            }

                            #endregion
                        }
                    }
                }
                ObservableCollection<string> TempCustomControlDisableList = new ObservableCollection<string>();
                List<string> readcontrolsremovedlist = new List<string>(sourceTestCaseItem.ComponentTypeList);
                foreach (var testActionItem in sourceTestCaseItem.TestActionItemList)
                {
                    foreach (var setTestControl in testActionItem.SetTestControlList)
                    {
                        foreach (string compstr in readcontrolsremovedlist.ToList())
                        {
                            ObservableCollection<string> ComponentNameList = sourceTestCaseItem.ComponentNameDictionary[compstr];
                            if (ComponentNameList.Count < 2)
                            {
                                foreach (string componentName in ComponentNameList)
                                {
                                    ObservableCollection<string> emptystring;
                                    Dictionary<string, ObservableCollection<string>> ComponentcontrolList = sourceTestCaseItem.ControlNameDictionary;
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
                                    Dictionary<string, ObservableCollection<string>> ComponentcontrolList = sourceTestCaseItem.ControlNameDictionary;
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

                            if (TempCustomControlDisableList.Count > 0 && compstr== "Custom Controls")
                            {
                                var CheckMatch = ComponentNameList.ToList().Except(TempCustomControlDisableList.ToList());
                                if (CheckMatch.Count() == 0 && readcontrolsremovedlist.Count() > 0)
                                {
                                    readcontrolsremovedlist.Remove(compstr);
                                }
                            }
                        }
                        setTestControl.CustomControlDisableList = TempCustomControlDisableList;
                        setTestControl.TestControlComponentTypeList = new ObservableCollection<string>(readcontrolsremovedlist);
                        //setTestControl.InputSelectionEnabled = false;
                        //setTestControl.ChannelEnabled = false;

                        //setTestControl.RampCheckVisibility = Visibility.Hidden;
                    }

                    foreach (var verifyTestControl in testActionItem.VerifyTestControlList)
                    {
                        verifyTestControl.TestControlComponentTypeList = new ObservableCollection<string>(sourceTestCaseItem.ComponentTypeList);
                        //verifyTestControl.InputSelectionEnabled = false;
                        //verifyTestControl.ChannelEnabled = false;
                        //verifyTestControl.MaxLimitIsEnabled = false;
                        //verifyTestControl.MinLimitIsEnabled = false;
                        //verifyTestControl.MinimumLimit = string.Empty;
                        //verifyTestControl.MaximumLimit = string.Empty;
                    }

                    foreach (var SetTestUsb in testActionItem.SetTestUsbList)
                    {
                        SetTestUsb.UsbAudioBridgeTypeSelectedItem = null;
                        SetTestUsb.UsbAudioDeviceSelectedItem = null;
                        SetTestUsb.UsbAudioTypeSelectedItem = null;
                        SetTestUsb.UsbDefaultDeviceOptionSelectedItem = null;
                        SetTestUsb.UsbAudioDeviceList = sourceTestCaseItem.UsbAudioDeviceList;
                        SetTestUsb.UsbAudioBridgeList = sourceTestCaseItem.UsbAudioBridgeList;
                        SetTestUsb.UsbAudioBridgeDeviceComboEnable = false;
                        foreach (Tuple<string, string> brdname in sourceTestCaseItem.UsbAudioBridgeList)
                        {
                            if (!SetTestUsb.BridgeList.Contains(brdname.Item2))
                                SetTestUsb.BridgeList.Add(brdname.Item2);
                        }
                    }

                    foreach (var VerifyTestUsb in testActionItem.VerifyTestUsbList)
                    {
                        VerifyTestUsb.UsbAudioBridgeTypeSelectedItem = null;
                        VerifyTestUsb.UsbAudioDeviceSelectedItem = null;
                        VerifyTestUsb.UsbAudioTypeSelectedItem = null;
                        VerifyTestUsb.UsbDefaultDeviceOptionSelectedItem = null;
                        VerifyTestUsb.UsbAudioDeviceList = sourceTestCaseItem.UsbAudioDeviceList;
                        VerifyTestUsb.UsbAudioBridgeList = sourceTestCaseItem.UsbAudioBridgeList;
                        VerifyTestUsb.UsbAudioBridgeDeviceComboEnable = false;
                        foreach (Tuple<string, string> brdname in sourceTestCaseItem.UsbAudioBridgeList)
                        {
                            if (!VerifyTestUsb.BridgeList.Contains(brdname.Item2))
                                VerifyTestUsb.BridgeList.Add(brdname.Item2);
                        }
                    }

                    foreach (var VerifyTestResponsalyzer in testActionItem.verifyTestResponsalyzerList)
                    {
                        VerifyTestResponsalyzer.TestResponsalyzerNameSelectedItem = null;
                        VerifyTestResponsalyzer.TestResponsalyzerTypeSelectedItem = null;
                        VerifyTestResponsalyzer.TestResponsalyzerNameList = sourceTestCaseItem.ResponsalyzerNameList;
                        VerifyTestResponsalyzer.TestResponsalyzerTypeList = sourceTestCaseItem.ResponsalyzerTypeList;
                    }


                }

             
                
                CloseConnection();

                List<DUT_DeviceItem> allDeviceItemList = GetDesignDetails(new List<TreeViewExplorer> { sourceTestCaseItem.TestPlanSelected });

                foreach (DUT_DeviceItem deviceItem in allDeviceItemList)
                {
                    deviceItem.ParentTestCaseItem = sourceTestCaseItem;
                }
                sourceTestCaseItem.SelectedDeviceItemList.Clear();
                foreach (var item in allDeviceItemList)
                {
                    string deviceModel = string.Empty;
                    if ((item.ItemDeviceModel.StartsWith("PS")))
                    {
                        deviceModel = item.ItemDeviceModel.Remove(item.ItemDeviceModel.Length - 1);
                    }
                    else if ((item.ItemDeviceModel.StartsWith("TSC-7")))
                    {
                        //deviceModel = item.ItemDeviceModel.Remove(item.ItemDeviceModel.Length - 1);
                        deviceModel = item.ItemDeviceModel;
                        int index = deviceModel.IndexOf("7");
                        if (index > 0)
                            deviceModel = deviceModel.Substring(0, index + 1);
                    }
                    else
                    {
                        deviceModel = item.ItemDeviceModel;
                    }

                    if ((DeviceDiscovery.Netpair_devicesSupported.Contains(deviceModel) || deviceModel.Contains("Core")))
                        sourceTestCaseItem.SelectedDeviceItemList.Add(item);
                }
              
                foreach (var testActionItem in sourceTestCaseItem.TestActionItemList)
                {
                    foreach (var setTestTelnet in testActionItem.SetTestTelnetList)
                    {
                        string TelnetSelectedDeviceCopy = setTestTelnet.TelnetSelectedDevice;
                        setTestTelnet.TelnetDeviceItem.Clear();
                        setTestTelnet.TelnetSelectedDeviceItem.Clear();
                        setTestTelnet.TelnetSelectedDeviceModel.Clear();
                        setTestTelnet.TelnetSelectedDevice = string.Empty;
                        if (TelnetSelectedDeviceCopy== "All devices" || TelnetSelectedDeviceCopy.Contains("Video Gen"))
                            setTestTelnet.TelnetSelectedDevice = TelnetSelectedDeviceCopy;

                        if (sourceTestCaseItem.SelectedDeviceItemList.Count > 0)
                        {
                            setTestTelnet.TelnetSelectedDeviceItem.Add("All devices");
                            if (!setTestTelnet.TelnetSelectedDeviceItem.Contains("Video Gen1-PGAVHD"))
                                setTestTelnet.TelnetSelectedDeviceItem.Add("Video Gen1-PGAVHD");
                        }

                        ObservableCollection<DUT_DeviceItem> telnetDeviceList = new ObservableCollection<DUT_DeviceItem>();
                        foreach (var item in sourceTestCaseItem.SelectedDeviceItemList)
                        {
                            telnetDeviceList.Add(new DUT_DeviceItem(item));
                            setTestTelnet.TelnetSelectedDeviceItem.Add(item.ItemDeviceName);
                            if(item.ItemDeviceName== TelnetSelectedDeviceCopy)
                            {
                                setTestTelnet.TelnetSelectedDevice = TelnetSelectedDeviceCopy;
                            }               

                            if (!setTestTelnet.TelnetSelectedDeviceModel.Keys.Contains(item.ItemDeviceName))
                                setTestTelnet.TelnetSelectedDeviceModel.Add(item.ItemDeviceName, item.ItemDeviceModel);
                        }

                        setTestTelnet.TelnetDeviceItem = telnetDeviceList;
                    }


                    foreach (var VerifyTestQRCM in testActionItem.VerifyTestQRCMList)
                    {
                        string seletedDeviceName = VerifyTestQRCM.QRCM_DeviceSelectedItem;                    
                        VerifyTestQRCM.VerifyQRCM_DevicesList.Clear();
                        VerifyTestQRCM.QRCM_DeviceModel.Clear();
                      

                        foreach (var item in sourceTestCaseItem.SelectedDeviceItemList)
                        {
                            if (item.ItemDeviceType.ToUpper() == "CORE")
                            {
                                if (item.ItemPrimaryorBackup.ToUpper() == "PRIMARY" && !VerifyTestQRCM.VerifyQRCM_DevicesList.Contains("Primary core"))
                                {
                                    VerifyTestQRCM.VerifyQRCM_DevicesList.Add("Primary core");
                                    VerifyTestQRCM.QRCM_DeviceModel.Add("Primary core", item.ItemDeviceModel);
                                }
                                else if (item.ItemPrimaryorBackup.ToUpper() == "BACKUP" && !VerifyTestQRCM.VerifyQRCM_DevicesList.Contains("Backup core"))
                                {
                                    VerifyTestQRCM.VerifyQRCM_DevicesList.Add("Backup core");
                                    VerifyTestQRCM.QRCM_DeviceModel.Add("Backup core", item.ItemDeviceModel);
                                }                              
                            }
                        }

                        if (VerifyTestQRCM.VerifyQRCM_DevicesList.Contains(seletedDeviceName))
                            VerifyTestQRCM.QRCM_DeviceSelectedItem = seletedDeviceName;                       

                    }

                    foreach (var SetTestQRCM in testActionItem.SetTestQRCMActionList)
                    {
                        string seletedDeviceName = SetTestQRCM.QRCM_DeviceSelectedItem;                     
                        SetTestQRCM.ActionQRCM_DevicesList.Clear();
                        SetTestQRCM.QRCM_DeviceModel.Clear();                   

                        foreach (var item in sourceTestCaseItem.SelectedDeviceItemList)
                        {
                            if (item.ItemDeviceType.ToUpper() == "CORE")
                            {
                                if (item.ItemPrimaryorBackup.ToUpper() == "PRIMARY" && !SetTestQRCM.ActionQRCM_DevicesList.Contains("Primary core"))
                                {
                                    SetTestQRCM.ActionQRCM_DevicesList.Add("Primary core");
                                    SetTestQRCM.QRCM_DeviceModel.Add("Primary core", item.ItemDeviceModel);
                                }
                                else if (item.ItemPrimaryorBackup.ToUpper() == "BACKUP" && !SetTestQRCM.ActionQRCM_DevicesList.Contains("Backup core"))
                                {
                                    SetTestQRCM.ActionQRCM_DevicesList.Add("Backup core");
                                    SetTestQRCM.QRCM_DeviceModel.Add("Backup core", item.ItemDeviceModel);
                                }                               
                            }
                        }

                        if (SetTestQRCM.ActionQRCM_DevicesList.Contains(seletedDeviceName))
                            SetTestQRCM.QRCM_DeviceSelectedItem = seletedDeviceName;                      
                    }

                    foreach (var setTestScript in testActionItem.VerifyTestScriptList)
                    {
                        if (setTestScript.VerifyScriptActionSelectedItem == "Deploy Monitoring" || setTestScript.VerifyScriptActionSelectedItem == "CPU Monitoring" || setTestScript.VerifyScriptActionSelectedItem == "LoadFromCore Monitoring")
                        {
                            setTestScript.ScriptSelectedDeviceItem.Clear();

                            foreach (var item in sourceTestCaseItem.SelectedDeviceItemList)
                            {
                                if (item.ItemDeviceModel.ToLower().Contains("core") && item.ItemPrimaryorBackup == "primary")
                                    setTestScript.ScriptSelectedDeviceItem.Add(item.ItemDeviceName);
                            }

                            if (setTestScript.ScriptSelectedDeviceItem.Count > 0)
                                setTestScript.DevicenamelistSelectedItem = setTestScript.ScriptSelectedDeviceItem[0];
                        }
                    }
                }

                foreach (var testActionItem in sourceTestCaseItem.TestActionItemList)
                {
                    foreach (var setTestNetPairing in testActionItem.SetTestNetPairingList)
                    {
                        setTestNetPairing.DutDeviceList.Clear();
                        List<DUT_DeviceItem> deviceList = new List<DUT_DeviceItem>();
                        foreach (var item in sourceTestCaseItem.SelectedDeviceItemList)
                        {
                            if (item.ItemDeviceType != "Core")
                                deviceList.Add(new DUT_DeviceItem(item));
                        }
                        DeviceDiscovery.UpdateNetPairingList(deviceList);
                        setTestNetPairing.DutDeviceList = new ObservableCollection<DUT_DeviceItem>(deviceList);
                    }
                }

                //foreach (var testActionItem in sourceTestCaseItem.TestActionItemList)
                //{
                //    foreach (var testSaveLogItem in testActionItem.TestSaveLogItemList)
                //    {
                //        testSaveLogItem.iLogDeviceItem.Clear();
                //        ObservableCollection<DUT_DeviceItem> iLogDeviceList = new ObservableCollection<DUT_DeviceItem>();
                //        foreach (var item in sourceTestCaseItem.SelectedDeviceItemList)
                //        {
                //            iLogDeviceList.Add(new DUT_DeviceItem(item));
                //        }
                //        testSaveLogItem.iLogDeviceItem = iLogDeviceList;
                //    }
                //}

                foreach (var testActionItem in sourceTestCaseItem.TestActionItemList)
                {
                    foreach (var testSaveLogItem in testActionItem.VerifyTestLogList)
                    {
                        testSaveLogItem.Log_verification_kernellog.Clear();
                        ObservableCollection<string> eventloglog = new ObservableCollection<string>();
                        if (sourceTestCaseItem.SelectedDeviceItemList.Count > 0)
                        {
                            eventloglog.Add("All devices");
                        }
                        foreach (var item in sourceTestCaseItem.SelectedDeviceItemList)
                        {

                            eventloglog.Add(item.ItemDeviceName);

                        }
                        testSaveLogItem.Log_verification_kernellog = eventloglog;
                    }


                }
                foreach (var testverifyitem in sourceTestCaseItem.TestActionItemList)
                {
                    foreach (var verifytestQR in testverifyitem.VerifyTestQRList)
                    {
                        string CameraSelectedItemcopy = verifytestQR.CameraSelectedItem;
                        verifytestQR.CameraList.Clear();
                        verifytestQR.CameraList1.Clear();
                        verifytestQR.CameraSelectedItem = null;
                        verifytestQR.QRverifytype = string.Empty;

                        foreach (var item in sourceTestCaseItem.SelectedDeviceItemList)
                        {
                            if (item.ItemDeviceType.ToUpper() == "CAMERA")
                            {
                                if (item.ItemDeviceName == CameraSelectedItemcopy)
                                {
                                    verifytestQR.CameraSelectedItem = CameraSelectedItemcopy;
                                }
                                verifytestQR.CameraList.Add(item.ItemDeviceName, item.ItemDeviceModel);
                                verifytestQR.CameraList1.Add(item.ItemDeviceName);
                            }
                        }
                    }
                }

                //foreach (var testActionItem in sourceTestCaseItem.TestActionItemList)
                //{
                //    foreach (var testSaveLogItem in testActionItem.TestSaveLogItemList)
                //    {
                //        testSaveLogItem.ConfiguratorLogDeviceItem.Clear();
                //        ObservableCollection<DUT_DeviceItem> configuratorDeviceList = new ObservableCollection<DUT_DeviceItem>();
                //        foreach (var item in sourceTestCaseItem.SelectedDeviceItemList)
                //        {
                //            if (item.ItemDeviceModel.Contains("Core"))
                //                configuratorDeviceList.Add(new DUT_DeviceItem(item));
                //        }
                //        testSaveLogItem.ConfiguratorLogDeviceItem = configuratorDeviceList;
                //    }
                //}
                


            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                //MessageBox.Show("Exception\n \n" + ex.Message, "QAT Error Code - EC02022", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public bool channelPresentInList(string channelname,List<string>channelList)
        {
            bool presentStatus = false;
            try
            {
                if (channelList.Any(s => s.Equals(channelname, StringComparison.OrdinalIgnoreCase)))
                {
                    presentStatus = true;
                }

                return presentStatus;
            }
            catch (Exception ex)
            {
                return presentStatus;
            }

        }

        public bool WriteTestCaseItemToDB(TestCaseItem sourceTestCaseItem, TreeViewExplorer copyItem)
        {
            try
            {
                string query = null;
                DataTable dataTable = null;
                DataTableReader dataTableReader = null;
                SqlDataAdapter dataAdapter = null;
                SqlCommand command = null;

                CreateConnection();
                OpenConnection();

                string itemTableName = QatConstants.DbTestCaseTable;
                string itemNameColumn = QatConstants.DbTestCaseNameColumn;
                int testPlanSelectedItemKey = 0;

                if (sourceTestCaseItem.TestPlanSelected != null)
                    testPlanSelectedItemKey = sourceTestCaseItem.TestPlanSelected.ItemKey;

                sourceTestCaseItem.TestItemName = sourceTestCaseItem.TestItemName.Trim();

                if (sourceTestCaseItem.IsNewTestCase)
                {
                    // Create new record and get the ID
                    DateTime creationTime = DateTime.Now;
                    sourceTestCaseItem.Createdon = creationTime;

                    string creatorName = Properties.Settings.Default.TesterName.ToString().TrimEnd();
                    sourceTestCaseItem.Createdby = creatorName;
                    string categoryName = string.Empty;
                    if (sourceTestCaseItem.IsTestCaseCreatedFromTestPlan)
                    {
                        query = "Select Category FROM " + QatConstants.DbTestPlanTable + " where " + QatConstants.DbTestPlanIDColumn + "='" + sourceTestCaseItem.ParentTestPlanItem.TestPlanID + "'";
                        categoryName = ReadSingleValueFromDB(query);
                        sourceTestCaseItem.Category = categoryName;
                    }
                    else
                    {
                        if (copyItem == null && sourceTestCaseItem.Category != null)
                            categoryName = sourceTestCaseItem.Category;
                        else
                        {
                            if (copyItem != null && copyItem.Category != null)
                            {
                                categoryName = copyItem.Category.TrimEnd();
                            }
                            else
                            {
                                categoryName = string.Empty;
                            }
                        }
                    }
                    sourceTestCaseItem.Category = categoryName;
                    query = "Insert into " + itemTableName + " Values (@TCname,'" + testPlanSelectedItemKey + "',@Createdate,@CreateBy,'" + null + "','" + string.Empty + "','" + string.Empty + "',@categoryvalue,@EditedBy,'" + null + "','" + null + "'," + sourceTestCaseItem.TestActionItemList.Count + ")";
                    InsertCommandWithParameter2(query, "@TCname", sourceTestCaseItem.TestItemName.Trim(), "@Createdate", creationTime, "@CreateBy", creatorName, "@categoryvalue", categoryName, "@EditedBy", creatorName);
                    //Action_countquery= "UPDATE Testcase SET Actioncount = " + sourceTestCaseItem.TestActionItemList.Count + " WHERE TestcaseID =" + sourceTestCaseItem.TestCaseID + "";
                    //Testcase_fillActioncount(Action_countquery);

                    //query = "Insert into " + itemTableName + " Values (@TCname,'" + testPlanSelectedItemKey+ "')";
                    //command = new SqlCommand(query, connect);
                    //command.ExecuteScalar();
                    //InsertCommandWithParameter(query, "@TCname", sourceTestCaseItem.TestItemName.Trim());

                    query = "select * from " + itemTableName + " where " + itemNameColumn + "=@TCname";
                    dataTable = new DataTable();
                    //dataTableReader = null;
                    //dataAdapter = new SqlDataAdapter(query, connect);
                    //dataAdapter.Fill(dataTable);
                    dataTable = SelectDTWithParameter(query, "@TCname", sourceTestCaseItem.TestItemName.Trim());
                    dataTableReader = dataTable.CreateDataReader();

                    if (dataTableReader.HasRows)
                    {
                        if (dataTableReader.Read())
                            sourceTestCaseItem.TestCaseID = dataTableReader.GetInt32(0);
                    }

                    sourceTestCaseItem.TestItemNameCopy = sourceTestCaseItem.TestItemName.Trim();
                    sourceTestCaseItem.IsNewTestCase = false;
                }
                else
                {
                    // Update Tase Case Name and Associated Test Plan ID
                    DateTime? modifiedTime = DateTime.Now;
                    string editorName = Properties.Settings.Default.TesterName.ToString().TrimEnd();
                    string categoryName = GetCategoryNameFromDB(sourceTestCaseItem.TestCaseID, itemTableName);
                    query = "update " + itemTableName + " set " + itemNameColumn + " =@TCname, TPID = '" + testPlanSelectedItemKey + "',ModifiedOn=@editdate,ModifiedBy=@editBy,category=@categoryvalue,EditedBy=@editedBy,Actioncount = " + sourceTestCaseItem.TestActionItemList.Count + " where TestCaseID = '" + sourceTestCaseItem.TestCaseID + "'";
                    InsertCommandWithParameter2(query, "@TCname", sourceTestCaseItem.TestItemName.Trim(), "@editdate", modifiedTime, "@editBy", editorName, "@categoryvalue", categoryName, "@editedBy", editorName);
                    sourceTestCaseItem.Modifiedon = modifiedTime;
                    sourceTestCaseItem.Modifiedby = editorName;
                    sourceTestCaseItem.Category = categoryName;
                    //Action_countquery = "UPDATE Testcase SET Actioncount = " + sourceTestCaseItem.TestActionItemList.Count + " WHERE TestcaseID =" + sourceTestCaseItem.TestCaseID + "";
                    //Testcase_fillActioncount(Action_countquery);
                    //query = "update " + itemTableName + " set TestItemName =@TCname, TPID = '" + testPlanSelectedItemKey + "' where TestCaseID = '" + sourceTestCaseItem.TestCaseID + "'";
                    //command = new SqlCommand(query, connect);
                    //command.ExecuteScalar();
                    //InsertCommandWithParameter(query, "@TCname", sourceTestCaseItem.TestItemName.Trim());


                    List<int> actionIDList = new List<int>();
                    query = "Select TestActionID FROM TestAction where TCID='" + sourceTestCaseItem.TestCaseID + "'";

                    dataTable = new DataTable();
                    dataTableReader = null;
                    dataAdapter = new SqlDataAdapter(query, connect);
                    dataAdapter.Fill(dataTable);
                    dataTableReader = dataTable.CreateDataReader();

                    if (dataTableReader.HasRows)
                    {
                        while (dataTableReader.Read())
                            actionIDList.Add(dataTableReader.GetInt32(0));
                    }

                    foreach (TestActionItem item in sourceTestCaseItem.TestActionItemList)
                    {
                        if (actionIDList.Contains(item.TestActionID))
                            actionIDList.Remove(item.TestActionID);
                    }

                    //// Delete actions associated with Test Case which are deleted.
                    foreach (int item in actionIDList)
                    {
                        ///////Delete all files

                        query = "select APxPath from APVerification where TCID = '" + sourceTestCaseItem.TestCaseID + "' and ActionID = '" + item + "'";
                        DeleteFileFromServer(query, "Audio Precision", "AP Project Files");


                        query = "select WaveformType from APSettings where TCID = '" + sourceTestCaseItem.TestCaseID + "' and ActionID = '" + item + "'";
                        DeleteFileFromServer(query, "Audio Precision", "AP Waveform Files");

                        query = "select VerificationType from APVerification where TCID = '" + sourceTestCaseItem.TestCaseID + "' and ActionID = '" + item + "'";
                        string verificationType = ReadSingleValueFromDB(query);

                        query = null;

                        if (verificationType == "Level and Gain")
                        {
                            query = "select WaveformType from LevelAndGain where TCID = '" + sourceTestCaseItem.TestCaseID + "' and ActionID = '" + item + "'";
                        }
                        else if (verificationType == "Frequency sweep")
                        {
                            query = "select VerificationLocation from APFrequencyResponse where TCID = '" + sourceTestCaseItem.TestCaseID + "' and ActionID = '" + item + "'";
                        }
                        else if (verificationType == "Phase")
                        {
                            query = "select VerificationLocation from APPhaseSettings where TCID = '" + sourceTestCaseItem.TestCaseID + "' and ActionID = '" + item + "'";
                        }
                        else if (verificationType == "Stepped Frequency Sweep")
                        {
                            query = "select VerificationLocation from APSteppedFreqSweepSettings where TCID = '" + sourceTestCaseItem.TestCaseID + "' and ActionID = '" + item + "'";
                        }
                        else if (verificationType == "THD+N")
                        {
                            query = "select VerificationLocation from APTHDNSettings where TCID = '" + sourceTestCaseItem.TestCaseID + "' and ActionID = '" + item + "'";
                        }

                        if (query != null && query != string.Empty)
                            DeleteFileFromServer(query, "Audio Precision", "Verification Files");

                        query = "select VerificationFileLocation from Responsalyzer where TCID = '" + sourceTestCaseItem.TestCaseID + "' and ActionID = '" + item + "'";
                        DeleteFileFromServer(query, "Responsalyzer", "Reference Files");

                        query = "delete from TestAction where TestActionID = '" + item + "' and TCID = '" + sourceTestCaseItem.TestCaseID + "'";
                        command = new SqlCommand(query, connect);
                        command.ExecuteScalar();
                    }

                    sourceTestCaseItem.TestItemNameCopy = sourceTestCaseItem.TestItemName.Trim();

                    //////QRCM files delete 
                    string filepathQRCM = QatConstants.QATServerPath + "\\QRCM_Files" + "\\" + sourceTestCaseItem.TestCaseID + ".txt";
                    if (File.Exists(filepathQRCM))
                    {
                        File.SetAttributes(filepathQRCM, FileAttributes.Normal);
                        File.Delete(filepathQRCM);
                    }

                }

                //var yy =xx.ForEach(x=>x.SetTestQRCMActionList.Select(y=>y.SetPayloadContent!=null))
                //var s1 = sourceTestCaseItem.TestActionItemList.Select(s => s.SetTestQRCMActionList.Select(y => (y.SetPayloadContent!=null) &&( y.SetPayloadContent != string.Empty)).ToList();
                //List<TestActionItem> QRCMitems = sourceTestCaseItem.TestActionItemList.Where(x => (x.ActionSelected == "QRCM Action") || (x.VerificationSelected == "QRCM Verification")).ToList();
             
           




                foreach (TestActionItem actionItem in sourceTestCaseItem.TestActionItemList)
                {
                    int oldTAID = actionItem.TestActionID;

                    List<string> filestoDelete = new List<string>();
                    List<string> fileToDeleteResponse = new List<string>();

                    query = "select APxPath from APVerification where TCID = '" + sourceTestCaseItem.TestCaseID + "' and ActionID = '" + oldTAID + "'";
                    string oldapxFile = ReadSingleValueFromDB(query);
                    if (oldapxFile != null && oldapxFile != string.Empty)
                        filestoDelete.Add(Path.Combine("Audio Precision", "AP Project Files", oldapxFile));

                    query = "select WaveformType from APSettings where TCID = '" + sourceTestCaseItem.TestCaseID + "' and ActionID = '" + oldTAID + "'";
                    string settingsWaveform = ReadSingleValueFromDB(query);
                    if (settingsWaveform != null && settingsWaveform != string.Empty)
                        filestoDelete.Add(Path.Combine("Audio Precision", "AP Waveform Files", settingsWaveform));

                    query = "select VerificationType from APVerification where TCID = '" + sourceTestCaseItem.TestCaseID + "' and ActionID = '" + oldTAID + "'";
                    string verificationType = ReadSingleValueFromDB(query);

                    query = null;

                    if (verificationType == "Level and Gain")
                    {
                        query = "select WaveformType from LevelAndGain where TCID = '" + sourceTestCaseItem.TestCaseID + "' and ActionID = '" + oldTAID + "'";
                    }
                    else if (verificationType == "Frequency sweep")
                    {
                        query = "select VerificationLocation from APFrequencyResponse where TCID = '" + sourceTestCaseItem.TestCaseID + "' and ActionID = '" + oldTAID + "'";
                    }
                    else if (verificationType == "Phase")
                    {
                        query = "select VerificationLocation from APPhaseSettings where TCID = '" + sourceTestCaseItem.TestCaseID + "' and ActionID = '" + oldTAID + "'";
                    }
                    else if (verificationType == "Stepped Frequency Sweep")
                    {
                        query = "select VerificationLocation from APSteppedFreqSweepSettings where TCID = '" + sourceTestCaseItem.TestCaseID + "' and ActionID = '" + oldTAID + "'";
                    }
                    else if (verificationType == "THD+N")
                    {
                        query = "select VerificationLocation from APTHDNSettings where TCID = '" + sourceTestCaseItem.TestCaseID + "' and ActionID = '" + oldTAID + "'";
                    }

                    if (query != null && query != string.Empty)
                    {
                        string deletefile = ReadSingleValueFromDB(query);
                        if (deletefile != null && deletefile != string.Empty)
                            filestoDelete.Add(Path.Combine("Audio Precision", "Verification Files", deletefile));
                    }

                    query = "select VerificationFileLocation from Responsalyzer where TCID = '" + sourceTestCaseItem.TestCaseID + "' and ActionID = '" + oldTAID + "'";
                    fileToDeleteResponse = ReadValuesFromDB(query);

                    query = "select ResponsalyzerID from Responsalyzer where ActionID = '" + oldTAID + "'";
                    List<string> responsalyzerID = ReadValuesFromDB(query);


                    // Delete actions associated with Test Case
                    query = "delete from TestAction where TestActionID = '" + actionItem.TestActionID + "' and TCID = '" + sourceTestCaseItem.TestCaseID + "'";
                    command = new SqlCommand(query, connect);
                    command.ExecuteScalar();


                    string actionValues = sourceTestCaseItem.TestCaseID + "',";
                    actionValues += "@ActionItemName" + ",'";
                    if (actionItem.ActionSelected == "Ssh/Telnet Action")
                        actionValues += "Telnet Action" + "','";
                    else
                        actionValues += actionItem.ActionSelected + "','";

                    actionValues += actionItem.ActionDelaySetting + "','";
                    actionValues += actionItem.ActionDelayUnitSelected + "','";

                    if (actionItem.VerificationSelected == "Ssh/Telnet Verification")
                        actionValues += "Telnet Verification" + "','";
                    else
                        actionValues += actionItem.VerificationSelected + "','";
                    
                    actionValues += actionItem.ActionErrorHandlingReRunCount + "','";
                    actionValues += actionItem.ActionErrorHandlingTypeSelected + "','";
                    //actionValues +=actionItem.sc

                    string iLogDeviceName = string.Empty;
                    string configuratorLogDeviceName = string.Empty;

                    foreach (var testSaveLogItem in actionItem.TestSaveLogItemList)
                    {
                        if (testSaveLogItem.saveQsysylogPeripheralSelection && testSaveLogItem.ActionSaveLogEventSelected != "Never Save logs")
                            actionValues += testSaveLogItem.ActionSaveLogEventSelected + " WithQsysLogPeripherals','";
                        else
                            actionValues += testSaveLogItem.ActionSaveLogEventSelected + "','";

                        actionValues += testSaveLogItem.ActionLogiLogIsChecked + "',";                        

                        foreach (DUT_DeviceItem deviceItem in testSaveLogItem.iLogDeviceItem)
                        {
                            if (deviceItem.iLogIsChecked)
                                iLogDeviceName += deviceItem.ItemDeviceName + ",";
                        }

                        if (iLogDeviceName != string.Empty)
                            iLogDeviceName = iLogDeviceName.TrimEnd(',');

                        actionValues += "@iLogDeviceName" + ",'";

                        actionValues += testSaveLogItem.ActionLogConfiguratorIsChecked + "',";

                        foreach (DUT_DeviceItem deviceItem in testSaveLogItem.ConfiguratorLogDeviceItem)
                        {
                            if (deviceItem.ConfiguratorLogIsChecked)
                                configuratorLogDeviceName += deviceItem.ItemDeviceName + ",";
                        }

                        if (configuratorLogDeviceName != string.Empty)
                            configuratorLogDeviceName = configuratorLogDeviceName.TrimEnd(',');

                        actionValues += "@configDeviceName" + ",'";

                        actionValues += testSaveLogItem.ActionLogEvenetLogIsChecked + "','";
                        actionValues += testSaveLogItem.ActionLogSipLogIsChecked + "','";
                        actionValues += testSaveLogItem.ActionLogQsysAppLogIsChecked + "','";
                        actionValues += testSaveLogItem.ActionLogSoftPhoneLogIsChecked + "','";
                        actionValues += testSaveLogItem.ActionLogUciViewerLogIsChecked + "','";
                        actionValues += testSaveLogItem.ActionLogKernelLogIsChecked + "','";
                        actionValues += testSaveLogItem.ActionLogWindowsEventLogIsChecked + "','";

                        actionValues += actionItem.Verificationdelay + "','";
                        actionValues += actionItem.VerificationdelayType + "','";
                        actionValues += actionItem.Rerundelay + "','";
                        actionValues += actionItem.RerundelayType + "','";
                        actionValues += testSaveLogItem.screenshotselection;
      //                  if(actionItem.cecVerificationbox_selected!=null)
      //                  actionValues += "','"+actionItem.cecVerificationbox_selected;
						//else
						//  actionValues += "','"+string.Empty;
                    }

                    //query = "Insert into TestAction values('" + actionValues + "')";
                    //command = new SqlCommand(query, connect);
                    //command.ExecuteScalar();

                    query = "Insert into TestAction values('" + actionValues + "')";
                    command = new SqlCommand(query, connect);
                    DataTable tbls = new DataTable();
                    command.Parameters.AddWithValue("@ActionItemName", actionItem.TestActionItemName);
                    command.Parameters.AddWithValue("@iLogDeviceName", iLogDeviceName);
                    command.Parameters.AddWithValue("@configDeviceName", configuratorLogDeviceName);
                    SqlDataAdapter dapt = new SqlDataAdapter(command);
                    dapt.Fill(tbls);

                    //Get Test Action ID
                    query = "select TestActionID from TestAction where ActionTabName = @ActionName and TCID = '" + sourceTestCaseItem.TestCaseID + "'";
                    dataTable = SelectDTWithParameter(query, "@ActionName", actionItem.TestActionItemName);

                    //dataTable = new DataTable();
                    //dataTableReader = null;
                    //dataAdapter = new SqlDataAdapter(query, connect);
                    //dataAdapter.Fill(dataTable);
                    dataTableReader = dataTable.CreateDataReader();

                    if (dataTableReader.HasRows)
                    {
                        if (dataTableReader.Read())
                            actionItem.TestActionID = dataTableReader.GetInt32(0);
                    }

                    foreach (TestControlItem setTestControlItem in actionItem.SetTestControlList)
                    {
                        string compType = setTestControlItem.TestControlComponentTypeSelectedItem;
                        string compName = setTestControlItem.TestControlComponentNameSelectedItem;

                        string setTestControlValues = sourceTestCaseItem.TestCaseID + "','";
                        setTestControlValues += actionItem.TestActionID + "',";
                        setTestControlValues += "@ComponentType" + ",";
                        setTestControlValues += "@ComponentName" + ",";

                        string propertyname = string.Empty;
                        string allchannelvalues = string.Empty;
                        string valueDataType = string.Empty;
                        string selectedPrettyName = setTestControlItem.ChannelSelectionSelectedItem;
                        string selectedPretty = setTestControlItem.TestControlPropertySelectedItem;

                        if ((!string.IsNullOrEmpty(setTestControlItem.ChannelSelectionSelectedItem))&& !string.IsNullOrEmpty(setTestControlItem.TestControlPropertySelectedItem))//&& ((setTestControlItem.TestControlPropertySelectedItem.StartsWith("CHANNEL "))|| (setTestControlItem.TestControlPropertySelectedItem.StartsWith("OUTPUT "))|| (setTestControlItem.TestControlPropertySelectedItem.StartsWith("INPUT ")) || (setTestControlItem.TestControlPropertySelectedItem.StartsWith("INPUT_OUTPUT ")) || (setTestControlItem.TestControlPropertySelectedItem.StartsWith("TAP ")) || (setTestControlItem.TestControlPropertySelectedItem.StartsWith("BANK_CONTROL ")) || (setTestControlItem.TestControlPropertySelectedItem.StartsWith("BANK_SELECT ")) || (setTestControlItem.TestControlPropertySelectedItem.StartsWith("GPIO_INPUT ")) || (setTestControlItem.TestControlPropertySelectedItem.StartsWith("GPIO_OUTPUT ")))
                        {
                            string[] controlPretty = null;
                            setTestControlItem.ParentTestActionItem.ParentTestCaseItem.ControlIDDictionary.TryGetValue(compName + setTestControlItem.TestControlPropertySelectedItem + setTestControlItem.ChannelSelectionSelectedItem, out controlPretty);

                            if (controlPretty != null)
                            {
                                if(controlPretty.Count() > 0)
                                    selectedPrettyName = controlPretty[0];

                                if(controlPretty.Count() > 1)
                                    propertyname = controlPretty[1];
                            }

                            string removechannel = selectedPrettyName + "~" + removeQATPrefix(selectedPretty);

                            actionItem.ParentTestCaseItem.dataTypeDictionary.TryGetValue(setTestControlItem.TestControlComponentNameSelectedItem + removechannel + propertyname, out valueDataType);
                            
                            //string removechannel = string.Empty;
                            //removechannel = setTestControlItem.ChannelSelectionSelectedItem + "~" + removeQATPrefix( setTestControlItem.TestControlPropertySelectedItem);
                            //if (setTestControlItem.TestControlPropertySelectedItem.StartsWith("CHANNEL "))
                            //     removechannel = setTestControlItem.ChannelSelectionSelectedItem+"~"+setTestControlItem.TestControlPropertySelectedItem.Remove(0, 8);
                            //else if (setTestControlItem.TestControlPropertySelectedItem.StartsWith("OUTPUT "))
                            //    removechannel = setTestControlItem.ChannelSelectionSelectedItem + "~" + setTestControlItem.TestControlPropertySelectedItem.Remove(0, 7);
                            //else if (setTestControlItem.TestControlPropertySelectedItem.StartsWith("INPUT "))
                            //    removechannel = setTestControlItem.ChannelSelectionSelectedItem + "~" + setTestControlItem.TestControlPropertySelectedItem.Remove(0, 6);
                            //else if (setTestControlItem.TestControlPropertySelectedItem.StartsWith("INPUT_OUTPUT "))
                            //    removechannel = setTestControlItem.ChannelSelectionSelectedItem + "~" + setTestControlItem.TestControlPropertySelectedItem.Remove(0, 13);
                            //else if (setTestControlItem.TestControlPropertySelectedItem.StartsWith("TAP "))
                            //    removechannel = setTestControlItem.ChannelSelectionSelectedItem + "~" + setTestControlItem.TestControlPropertySelectedItem.Remove(0, 4);
                            //else if (setTestControlItem.TestControlPropertySelectedItem.StartsWith("BANK_CONTROL_INPUT "))
                            //    removechannel = setTestControlItem.ChannelSelectionSelectedItem + "~" + setTestControlItem.TestControlPropertySelectedItem.Remove(0, 19);
                            //else if (setTestControlItem.TestControlPropertySelectedItem.StartsWith("BANK_CONTROL_OUTPUT "))
                            //    removechannel = setTestControlItem.ChannelSelectionSelectedItem + "~" + setTestControlItem.TestControlPropertySelectedItem.Remove(0, 20);
                            //else if (setTestControlItem.TestControlPropertySelectedItem.StartsWith("BANK_CONTROL_INPUT_OUTPUT "))
                            //    removechannel = setTestControlItem.ChannelSelectionSelectedItem + "~" + setTestControlItem.TestControlPropertySelectedItem.Remove(0, 26);
                            //else if (setTestControlItem.TestControlPropertySelectedItem.StartsWith("BANK_SELECT_INPUT "))
                            //    removechannel = setTestControlItem.ChannelSelectionSelectedItem + "~" + setTestControlItem.TestControlPropertySelectedItem.Remove(0, 18);
                            //else if (setTestControlItem.TestControlPropertySelectedItem.StartsWith("BANK_SELECT_OUTPUT "))
                            //    removechannel = setTestControlItem.ChannelSelectionSelectedItem + "~" + setTestControlItem.TestControlPropertySelectedItem.Remove(0, 19);
                            //else if (setTestControlItem.TestControlPropertySelectedItem.StartsWith("GPIO_INPUT "))
                            //    removechannel = setTestControlItem.ChannelSelectionSelectedItem + "~" + setTestControlItem.TestControlPropertySelectedItem.Remove(0, 11);
                            //else if (setTestControlItem.TestControlPropertySelectedItem.StartsWith("GPIO_OUTPUT "))
                            //    removechannel = setTestControlItem.ChannelSelectionSelectedItem + "~" + setTestControlItem.TestControlPropertySelectedItem.Remove(0, 12);

                            //actionItem.ParentTestCaseItem.dataTypeDictionary.TryGetValue(setTestControlItem.TestControlComponentNameSelectedItem+removechannel, out valueDataType);
                            //propertyname = GetActualProperty(setTestControlItem.TestControlComponentTypeSelectedItem, setTestControlItem.TestControlComponentNameSelectedItem, removechannel, sourceTestCaseItem.DesignIDSelected);
                            setTestControlValues += "@propertyname" + ",";
                            if((setTestControlItem.LoopIsChecked)&&(setTestControlItem.LoopStart!=string.Empty)&& (setTestControlItem.LoopEnd != string.Empty)&& (setTestControlItem.LoopIncrement != string.Empty))
                            {
                                if(setTestControlItem.ChannelSelectionList.Count>0)
                                {
                                    List<string> channelcontrols = new List<string>();
                                    foreach (string channels in setTestControlItem.ChannelSelectionList)
                                    {
                                        string[] localcontrolPretty = null;
                                        setTestControlItem.ParentTestActionItem.ParentTestCaseItem.ControlIDDictionary.TryGetValue(compName + setTestControlItem.TestControlPropertySelectedItem + channels, out localcontrolPretty);
                                        //string localPrettyName = controlPretty[0];

                                        string localControlID = string.Empty;
                                        if (localcontrolPretty != null && localcontrolPretty.Count() > 1)
                                            localControlID = localcontrolPretty[1];

                                        channelcontrols.Add(localControlID);

                                        //string intial = string.Empty;
                                        //if (setTestControlItem.TestControlPropertySelectedItem.StartsWith("CHANNEL "))
                                        //    intial = channels + "~" + removeQATPrefix(setTestControlItem.TestControlPropertySelectedItem);
                                        //else if (setTestControlItem.TestControlPropertySelectedItem.StartsWith("OUTPUT "))
                                        //    intial = channels + "~" + setTestControlItem.TestControlPropertySelectedItem.Remove(0, 7);
                                        //else if (setTestControlItem.TestControlPropertySelectedItem.StartsWith("INPUT "))
                                        //    intial = channels + "~" + setTestControlItem.TestControlPropertySelectedItem.Remove(0, 6);
                                        //else if (setTestControlItem.TestControlPropertySelectedItem.StartsWith("INPUT_OUTPUT "))
                                        //    intial = channels + "~" + setTestControlItem.TestControlPropertySelectedItem.Remove(0, 13);
                                        //else if (setTestControlItem.TestControlPropertySelectedItem.StartsWith("TAP "))
                                        //    intial = channels + "~" + setTestControlItem.TestControlPropertySelectedItem.Remove(0, 4);
                                        //else if (setTestControlItem.TestControlPropertySelectedItem.StartsWith("BANK_CONTROL_INPUT "))
                                        //    intial = channels + "~" + setTestControlItem.TestControlPropertySelectedItem.Remove(0, 19);
                                        //else if (setTestControlItem.TestControlPropertySelectedItem.StartsWith("BANK_CONTROL_OUTPUT "))
                                        //    intial = channels + "~" + setTestControlItem.TestControlPropertySelectedItem.Remove(0, 20);
                                        //else if (setTestControlItem.TestControlPropertySelectedItem.StartsWith("BANK_CONTROL_INPUT_OUTPUT "))
                                        //    intial = channels + "~" + setTestControlItem.TestControlPropertySelectedItem.Remove(0, 26);
                                        //else if (setTestControlItem.TestControlPropertySelectedItem.StartsWith("BANK_SELECT_INPUT "))
                                        //    intial = channels + "~" + setTestControlItem.TestControlPropertySelectedItem.Remove(0, 18);
                                        //else if (setTestControlItem.TestControlPropertySelectedItem.StartsWith("BANK_SELECT_OUTPUT "))
                                        //    intial = channels + "~" + setTestControlItem.TestControlPropertySelectedItem.Remove(0, 19);
                                        //else if (setTestControlItem.TestControlPropertySelectedItem.StartsWith("GPIO_INPUT "))
                                        //    intial = channels + "~" + setTestControlItem.TestControlPropertySelectedItem.Remove(0, 11);
                                        //else if (setTestControlItem.TestControlPropertySelectedItem.StartsWith("GPIO_OUTPUT "))
                                        //    intial = channels + "~" + setTestControlItem.TestControlPropertySelectedItem.Remove(0, 12);
                                        //channelcontrols.Add(GetActualProperty(setTestControlItem.TestControlComponentTypeSelectedItem, setTestControlItem.TestControlComponentNameSelectedItem, intial, sourceTestCaseItem.DesignIDSelected));
                                    }
                                    allchannelvalues = string.Join("|", channelcontrols.ToArray());
                                }
                                else
                                {
                                    allchannelvalues = string.Empty;
                                }
                            }
                            else
                                allchannelvalues = string.Empty;
                        }
                        else
                        {
                            string[] controlPrettyArray = null;
                            setTestControlItem.ParentTestActionItem.ParentTestCaseItem.ControlIDDictionary.TryGetValue(setTestControlItem.TestControlComponentNameSelectedItem + setTestControlItem.TestControlPropertySelectedItem, out controlPrettyArray);

                            if (controlPrettyArray != null)
                            {
                                if(controlPrettyArray.Count() > 0)
                                    selectedPretty = controlPrettyArray[0];
                                if (controlPrettyArray.Count() > 1)
                                    propertyname = controlPrettyArray[1];
                            }

                            //propertyname = GetActualProperty(setTestControlItem.TestControlComponentTypeSelectedItem, setTestControlItem.TestControlComponentNameSelectedItem, setTestControlItem.TestControlPropertySelectedItem, sourceTestCaseItem.DesignIDSelected);
                            setTestControlValues += "@propertyname" + ",";
                            actionItem.ParentTestCaseItem.dataTypeDictionary.TryGetValue(setTestControlItem.TestControlComponentNameSelectedItem+setTestControlItem.TestControlPropertySelectedItem + propertyname, out valueDataType);
                            allchannelvalues = string.Empty;
                        }
                                                
                        setTestControlValues += "@selectedPrettyName" + ",";
                        setTestControlValues += "@Valuename" + ",'";
                        setTestControlValues += setTestControlItem.RampIsChecked + "','";
                        setTestControlValues += setTestControlItem.RampSetting + "','";
                        setTestControlValues += setTestControlItem.LoopIsChecked + "','";
                        setTestControlValues += setTestControlItem.LoopStart + "','";
                        setTestControlValues += setTestControlItem.LoopEnd + "','";
                        setTestControlValues += setTestControlItem.LoopIncrement + "',";

                        setTestControlValues += "@prettyName" + ",'";
                        setTestControlValues += setTestControlItem.InputSelectionComboSelectedItem + "','";
                        setTestControlValues += allchannelvalues + "','";
                        setTestControlValues += valueDataType;

                        //query = "Insert into ControlAction values('" + setTestControlValues + "')";
                        //command = new SqlCommand(query, connect);
                        //command.ExecuteScalar();

                        query = "Insert into ControlAction values('" + setTestControlValues + "')";
                        command = new SqlCommand(query, connect);
                        DataTable tbl = new DataTable();
                        if(string.IsNullOrEmpty(compType))
                        {
                            compType = string.Empty;
                        }
                        if (string.IsNullOrEmpty(compName))
                        {
                            compName = string.Empty;
                        }
                        if (string.IsNullOrEmpty(propertyname))
                        {
                            propertyname = string.Empty;
                        }
                        if (string.IsNullOrEmpty(setTestControlItem.TestControlPropertyInitialValueSelectedItem))
                        {
                            setTestControlItem.TestControlPropertyInitialValueSelectedItem = string.Empty;
                        }
                        if (selectedPrettyName == null)
                            selectedPrettyName = string.Empty;
                        if (selectedPretty == null)
                            selectedPretty = string.Empty;

                        command.Parameters.AddWithValue("@ComponentType", compType);
                        command.Parameters.AddWithValue("@ComponentName", compName);
                        command.Parameters.AddWithValue("@propertyname", propertyname);
                        command.Parameters.AddWithValue("@selectedPrettyName", selectedPrettyName);
                        command.Parameters.AddWithValue("@prettyName", selectedPretty);
                        command.Parameters.AddWithValue("@Valuename", setTestControlItem.TestControlPropertyInitialValueSelectedItem.Trim());

                        SqlDataAdapter dap = new SqlDataAdapter(command);
                        dap.Fill(tbl);
                    }

                    foreach (TestControlItem verifyTestControlItem in actionItem.VerifyTestControlList)
                    {
                        string compType = verifyTestControlItem.TestControlComponentTypeSelectedItem;
                        string compName = verifyTestControlItem.TestControlComponentNameSelectedItem;
                        

                        string verifyTestControlValues = sourceTestCaseItem.TestCaseID + "','";
                        verifyTestControlValues += actionItem.TestActionID + "',";
                        verifyTestControlValues += "@ComponentType" + ",";
                        verifyTestControlValues += "@ComponentName" + ",";

                        string propertyname = string.Empty;
                        string allchannelvalues = string.Empty;
                        string valueDataType = string.Empty;
                        string selectedPrettyName = verifyTestControlItem.ChannelSelectionSelectedItem;
                        string selectedPretty = verifyTestControlItem.TestControlPropertySelectedItem;

                        if ((!string.IsNullOrEmpty(verifyTestControlItem.ChannelSelectionSelectedItem)) && (!string.IsNullOrEmpty(verifyTestControlItem.TestControlPropertySelectedItem)))
                        {
                            string[] controlPrettyName = null;
                            verifyTestControlItem.ParentTestActionItem.ParentTestCaseItem.ControlIDDictionary.TryGetValue(compName + verifyTestControlItem.TestControlPropertySelectedItem + verifyTestControlItem.ChannelSelectionSelectedItem, out controlPrettyName);

                            if (controlPrettyName != null)
                            {
                                if(controlPrettyName.Count() > 0)
                                    selectedPrettyName = controlPrettyName[0];

                                if(controlPrettyName.Count() > 1)
                                    propertyname = controlPrettyName[1];
                            }

                            string removechannel = selectedPrettyName + "~" + removeQATPrefix(selectedPretty);
							
                            //string removechannel = string.Empty;
                            //removechannel = verifyTestControlItem.ChannelSelectionSelectedItem + "~" + removeQATPrefix(verifyTestControlItem.TestControlPropertySelectedItem);
                            //if (verifyTestControlItem.TestControlPropertySelectedItem.StartsWith("CHANNEL "))
                            //    removechannel = verifyTestControlItem.ChannelSelectionSelectedItem + "~" + verifyTestControlItem.TestControlPropertySelectedItem.Remove(0, 8);
                            //else if (verifyTestControlItem.TestControlPropertySelectedItem.StartsWith("OUTPUT "))
                            //    removechannel = verifyTestControlItem.ChannelSelectionSelectedItem + "~" + verifyTestControlItem.TestControlPropertySelectedItem.Remove(0, 7);
                            //else if (verifyTestControlItem.TestControlPropertySelectedItem.StartsWith("INPUT "))
                            //    removechannel = verifyTestControlItem.ChannelSelectionSelectedItem + "~" + verifyTestControlItem.TestControlPropertySelectedItem.Remove(0, 6);
                            //else if (verifyTestControlItem.TestControlPropertySelectedItem.StartsWith("INPUT_OUTPUT "))
                            //    removechannel = verifyTestControlItem.ChannelSelectionSelectedItem + "~" + verifyTestControlItem.TestControlPropertySelectedItem.Remove(0, 13);
                            //else if (verifyTestControlItem.TestControlPropertySelectedItem.StartsWith("TAP "))
                            //    removechannel = verifyTestControlItem.ChannelSelectionSelectedItem + "~" + verifyTestControlItem.TestControlPropertySelectedItem.Remove(0, 4);
                            //else if (verifyTestControlItem.TestControlPropertySelectedItem.StartsWith("BANK_CONTROL_INPUT "))
                            //    removechannel = verifyTestControlItem.ChannelSelectionSelectedItem + "~" + verifyTestControlItem.TestControlPropertySelectedItem.Remove(0, 19);
                            //else if (verifyTestControlItem.TestControlPropertySelectedItem.StartsWith("BANK_CONTROL_OUTPUT "))
                            //    removechannel = verifyTestControlItem.ChannelSelectionSelectedItem + "~" + verifyTestControlItem.TestControlPropertySelectedItem.Remove(0, 20);
                            //else if (verifyTestControlItem.TestControlPropertySelectedItem.StartsWith("BANK_CONTROL_INPUT_OUTPUT "))
                            //    removechannel = verifyTestControlItem.ChannelSelectionSelectedItem + "~" + verifyTestControlItem.TestControlPropertySelectedItem.Remove(0, 26);
                            //else if (verifyTestControlItem.TestControlPropertySelectedItem.StartsWith("BANK_SELECT_INPUT "))
                            //    removechannel = verifyTestControlItem.ChannelSelectionSelectedItem + "~" + verifyTestControlItem.TestControlPropertySelectedItem.Remove(0, 18);
                            //else if (verifyTestControlItem.TestControlPropertySelectedItem.StartsWith("BANK_SELECT_OUTPUT "))
                            //    removechannel = verifyTestControlItem.ChannelSelectionSelectedItem + "~" + verifyTestControlItem.TestControlPropertySelectedItem.Remove(0, 19);
                            //else if (verifyTestControlItem.TestControlPropertySelectedItem.StartsWith("GPIO_INPUT "))
                            //    removechannel = verifyTestControlItem.ChannelSelectionSelectedItem + "~" + verifyTestControlItem.TestControlPropertySelectedItem.Remove(0, 11);
                            //else if (verifyTestControlItem.TestControlPropertySelectedItem.StartsWith("GPIO_OUTPUT "))
                            //    removechannel = verifyTestControlItem.ChannelSelectionSelectedItem + "~" + verifyTestControlItem.TestControlPropertySelectedItem.Remove(0, 12);
                            //actionItem.ParentTestCaseItem.VerifyDataTypeDictionary.TryGetValue(verifyTestControlItem.TestControlComponentNameSelectedItem+removechannel, out valueDataType);
                            //propertyname = GetActualProperty(verifyTestControlItem.TestControlComponentTypeSelectedItem, verifyTestControlItem.TestControlComponentNameSelectedItem, removechannel, sourceTestCaseItem.DesignIDSelected);
                            
                            actionItem.ParentTestCaseItem.VerifyDataTypeDictionary.TryGetValue(verifyTestControlItem.TestControlComponentNameSelectedItem + removechannel + propertyname, out valueDataType);

                            verifyTestControlValues += "@propertyname" + ",";

                            if ((verifyTestControlItem.LoopIsChecked) && (verifyTestControlItem.LoopStart != string.Empty) && (verifyTestControlItem.LoopEnd != string.Empty) && (verifyTestControlItem.LoopIncrement != string.Empty))
                            {
                                if (verifyTestControlItem.ChannelSelectionList.Count > 0)
                                {
                                    List<string> channelcontrols = new List<string>();
                                    foreach (string channels in verifyTestControlItem.ChannelSelectionList)
                                    {
										string[] localcontrolPretty = null;
                                        verifyTestControlItem.ParentTestActionItem.ParentTestCaseItem.ControlIDDictionary.TryGetValue(compName + verifyTestControlItem.TestControlPropertySelectedItem + channels, out localcontrolPretty);
                                        //string localPrettyName = controlPretty[0];

                                        string localControlID = string.Empty;
                                        if (localcontrolPretty != null && localcontrolPretty.Count() > 1)
                                            localControlID = localcontrolPretty[1];

                                        channelcontrols.Add(localControlID);
										
                                        //string intial = string.Empty;
                                        //intial = channels + "~" + removeQATPrefix(verifyTestControlItem.TestControlPropertySelectedItem);
                                        
										//if (verifyTestControlItem.TestControlPropertySelectedItem.StartsWith("CHANNEL "))
                                        //    intial = channels + "~" + verifyTestControlItem.TestControlPropertySelectedItem.Remove(0, 8);
                                        //else if (verifyTestControlItem.TestControlPropertySelectedItem.StartsWith("OUTPUT "))
                                        //    intial = channels + "~" + verifyTestControlItem.TestControlPropertySelectedItem.Remove(0, 7);
                                        //else if (verifyTestControlItem.TestControlPropertySelectedItem.StartsWith("INPUT "))
                                        //    intial = channels + "~" + verifyTestControlItem.TestControlPropertySelectedItem.Remove(0, 6);
                                        //else if (verifyTestControlItem.TestControlPropertySelectedItem.StartsWith("INPUT_OUTPUT "))
                                        //    intial = channels + "~" + verifyTestControlItem.TestControlPropertySelectedItem.Remove(0, 13);
                                        //else if (verifyTestControlItem.TestControlPropertySelectedItem.StartsWith("TAP "))
                                        //    intial = channels + "~" + verifyTestControlItem.TestControlPropertySelectedItem.Remove(0, 4);
                                        //else if (verifyTestControlItem.TestControlPropertySelectedItem.StartsWith("BANK_CONTROL_INPUT "))
                                        //    intial = channels + "~" + verifyTestControlItem.TestControlPropertySelectedItem.Remove(0, 19);
                                        //else if (verifyTestControlItem.TestControlPropertySelectedItem.StartsWith("BANK_CONTROL_OUTPUT "))
                                        //    intial = channels + "~" + verifyTestControlItem.TestControlPropertySelectedItem.Remove(0, 20);
                                        //else if (verifyTestControlItem.TestControlPropertySelectedItem.StartsWith("BANK_CONTROL_INPUT_OUTPUT "))
                                        //    intial = channels + "~" + verifyTestControlItem.TestControlPropertySelectedItem.Remove(0, 26);
                                        //else if (verifyTestControlItem.TestControlPropertySelectedItem.StartsWith("BANK_SELECT_INPUT "))
                                        //    intial = channels + "~" + verifyTestControlItem.TestControlPropertySelectedItem.Remove(0, 18);
                                        //else if (verifyTestControlItem.TestControlPropertySelectedItem.StartsWith("BANK_SELECT_OUTPUT "))
                                        //    intial = channels + "~" + verifyTestControlItem.TestControlPropertySelectedItem.Remove(0, 19);
                                        //else if (verifyTestControlItem.TestControlPropertySelectedItem.StartsWith("GPIO_INPUT "))
                                        //    intial = channels + "~" + verifyTestControlItem.TestControlPropertySelectedItem.Remove(0, 11);
                                        //else if (verifyTestControlItem.TestControlPropertySelectedItem.StartsWith("GPIO_OUTPUT "))
                                        //    intial = channels + "~" + verifyTestControlItem.TestControlPropertySelectedItem.Remove(0, 12);
                                        //channelcontrols.Add(GetActualProperty(verifyTestControlItem.TestControlComponentTypeSelectedItem, verifyTestControlItem.TestControlComponentNameSelectedItem, intial, sourceTestCaseItem.DesignIDSelected));
                                    }

                                    allchannelvalues = string.Join("|", channelcontrols.ToArray());
                                }
                                else
                                {
                                    allchannelvalues = string.Empty;
                                }
                            }
                            else
                                allchannelvalues = string.Empty;
                        }
                        else
                        {
                            string[] controlPrettyArray = null;
                            verifyTestControlItem.ParentTestActionItem.ParentTestCaseItem.ControlIDDictionary.TryGetValue(verifyTestControlItem.TestControlComponentNameSelectedItem + verifyTestControlItem.TestControlPropertySelectedItem, out controlPrettyArray);

                            if (controlPrettyArray != null)
                            {
                                if(controlPrettyArray.Count() > 0)
                                    selectedPretty = controlPrettyArray[0];

                                if(controlPrettyArray.Count() > 1)
                                    propertyname = controlPrettyArray[1];
                            }

                            verifyTestControlValues += "@propertyname" + ",";
                            allchannelvalues = string.Empty;
                            actionItem.ParentTestCaseItem.VerifyDataTypeDictionary.TryGetValue(verifyTestControlItem.TestControlComponentNameSelectedItem + verifyTestControlItem.TestControlPropertySelectedItem + propertyname, out valueDataType);
                        }
                        // Channel is not set
                        
                        verifyTestControlValues += "@selectedPrettyName" + ",";
                        verifyTestControlValues += "@Valuename" + ",";
                        verifyTestControlValues += "@selectedPretty" + ",'";
                        verifyTestControlValues += verifyTestControlItem.InputSelectionComboSelectedItem + "','";
                        verifyTestControlValues += verifyTestControlItem.MaximumLimit + "','";
                        verifyTestControlValues += verifyTestControlItem.MinimumLimit + "','";
                        verifyTestControlValues += verifyTestControlItem.LoopIsChecked + "','";
                        verifyTestControlValues += verifyTestControlItem.LoopStart + "','";
                        verifyTestControlValues += verifyTestControlItem.LoopEnd + "','";
                        verifyTestControlValues += verifyTestControlItem.LoopIncrement + "','";
                        verifyTestControlValues += allchannelvalues + "','";
                        verifyTestControlValues += valueDataType;

                        query = "Insert into ControlVerification values('" + verifyTestControlValues + "')";
                        command = new SqlCommand(query, connect);
                        DataTable tbl = new DataTable();

                        if (string.IsNullOrEmpty(compType))
                        {
                            compType = string.Empty;
                        }
                        if (string.IsNullOrEmpty(compName))
                        {
                            compName = string.Empty;
                        }
                        if (string.IsNullOrEmpty(propertyname))
                        {
                            propertyname = string.Empty;
                        }
                        if (string.IsNullOrEmpty(verifyTestControlItem.TestControlPropertyInitialValueSelectedItem))
                        {
                            verifyTestControlItem.TestControlPropertyInitialValueSelectedItem = string.Empty;
                        }

                        if (string.IsNullOrEmpty(selectedPrettyName))
                            selectedPrettyName = string.Empty;

                        if (string.IsNullOrEmpty(selectedPretty))
                            selectedPretty = string.Empty;

                        command.Parameters.AddWithValue("@ComponentType", compType);
                        command.Parameters.AddWithValue("@ComponentName", compName);
                        command.Parameters.AddWithValue("@propertyname", propertyname);
                        command.Parameters.AddWithValue("@selectedPrettyName", selectedPrettyName);
                        command.Parameters.AddWithValue("@Valuename", verifyTestControlItem.TestControlPropertyInitialValueSelectedItem.Trim());
                        command.Parameters.AddWithValue("@selectedPretty", selectedPretty);
                        SqlDataAdapter dap = new SqlDataAdapter(command);
                        dap.Fill(tbl);

                        //command.ExecuteScalar();
                    }

                    foreach (TestTelnetItem setTestTelnetItem in actionItem.SetTestTelnetList)
                    {
                        string setTestTelnetValues = sourceTestCaseItem.TestCaseID + "','";
                        setTestTelnetValues += actionItem.TestActionID + "',";
                        setTestTelnetValues += "@TelnetCommand" + ",'";
                        
                        string telnetDeviceName = null;
                        string telnetDeviceModel = null;

                        if (setTestTelnetItem.TelnetSelectedDevice == "All devices")
                        {
                            foreach (string deviceName in setTestTelnetItem.TelnetSelectedDeviceItem)
                            {
                                if (deviceName != "All devices" && !deviceName.StartsWith("Video Gen"))
                                {
                                    telnetDeviceName += deviceName + ",";

                                    if (setTestTelnetItem.TelnetSelectedDeviceModel.Keys.Contains(deviceName))
                                        telnetDeviceModel += setTestTelnetItem.TelnetSelectedDeviceModel[deviceName] + ",";
                                    else
                                        telnetDeviceModel += "NA";
                                }
                            }

                            telnetDeviceName = telnetDeviceName.TrimEnd(',');
                            telnetDeviceModel = telnetDeviceModel.TrimEnd(','); 
                        }
                        else
                        {
                            if (setTestTelnetItem.TelnetSelectedDevice != null)
                            {
                                telnetDeviceName = setTestTelnetItem.TelnetSelectedDevice;

                                if (setTestTelnetItem.TelnetSelectedDeviceModel.Keys.Contains(setTestTelnetItem.TelnetSelectedDevice))
                                    telnetDeviceModel = setTestTelnetItem.TelnetSelectedDeviceModel[setTestTelnetItem.TelnetSelectedDevice];
                                else
                                    telnetDeviceModel = "NA";
                            }
                            else
                            {
                                telnetDeviceName = string.Empty;
                                telnetDeviceModel = string.Empty;
                            }
                        }

                        setTestTelnetValues += telnetDeviceName + "','" + telnetDeviceModel;

                        //query = "Insert into TelnetAction values('" + setTestTelnetValues + "')";
                        //command = new SqlCommand(query, connect);
                        //command.ExecuteScalar();

                        query = "Insert into TelnetAction values('" + setTestTelnetValues + "')";
                        command = new SqlCommand(query, connect);
                        DataTable tbl = new DataTable();

                        if (setTestTelnetItem.TelnetCommand == null)
                            setTestTelnetItem.TelnetCommand = string.Empty;

                        command.Parameters.AddWithValue("@TelnetCommand", setTestTelnetItem.TelnetCommand);
                        SqlDataAdapter dap = new SqlDataAdapter(command);
                        dap.Fill(tbl);
                    }

                    foreach (TestCECItem setTestCECItem in actionItem.SetTestCECList)
                    {
                        string setTestcecValues = sourceTestCaseItem.TestCaseID + "','";
                        setTestcecValues += actionItem.TestActionID + "','";
                        setTestcecValues += "'" + ",'";
                        setTestcecValues += setTestCECItem.DeviceselectionSelecetdItem + "','";
                        setTestcecValues += setTestCECItem.CECCommandListSelectedItem + "',@opcode";
                        

                        query = "Insert into CECAction values('" + setTestcecValues + ")";
                        command = new SqlCommand(query, connect);
                        DataTable tbl = new DataTable();

                        if (setTestCECItem.CECActionOpcode == null)
                            setTestCECItem.CECActionOpcode = string.Empty;

                        if (setTestCECItem.CECCommandListSelectedItem == "Others")
                            command.Parameters.AddWithValue("@opcode", setTestCECItem.CECActionOpcode);
                        else
                            command.Parameters.AddWithValue("@opcode", string.Empty);

                        SqlDataAdapter dap = new SqlDataAdapter(command);
                        dap.Fill(tbl);
                    }
                  
                    foreach (TestVerifyCECItem setTestCECItem in actionItem.VerifyTestCECList)
                    {
                     
                        string setTestcecValues = sourceTestCaseItem.TestCaseID + "','";
                        setTestcecValues += actionItem.TestActionID + "','";
                        setTestcecValues += "'" + ",'";
                        setTestcecValues += "'" + ",'";
                        setTestcecValues +="',@opcode";
                        //setTestcecValues +="@opcode"+"'";
                        if (actionItem.cecVerificationbox_selected != null)
                            setTestcecValues += ",'" + actionItem.cecVerificationbox_selected;
                        else
                            setTestcecValues += ",'" + string.Empty;

                        query = "Insert into CECVerification values('" + setTestcecValues + "')";
                        command = new SqlCommand(query, connect);
                        DataTable tbl = new DataTable();

                        if (setTestCECItem.CECverificationOpcode == null)
                            setTestCECItem.CECverificationOpcode = string.Empty;

                        command.Parameters.AddWithValue("@opcode", setTestCECItem.CECverificationOpcode);
               

                        SqlDataAdapter dap = new SqlDataAdapter(command);
                        dap.Fill(tbl);
                    }

                    foreach (TestUserActionItem setTestUserItem in actionItem.SetTestUserActionList)
                    {
                        string setTestUserValues = sourceTestCaseItem.TestCaseID + "','";
                        setTestUserValues += actionItem.TestActionID + "',";                     
                        setTestUserValues += "@userText";

                        query = "Insert into UserAction values('" + setTestUserValues + ")";
                        command = new SqlCommand(query, connect);
                        DataTable tbl = new DataTable();

                        command.Parameters.AddWithValue("@userText", setTestUserItem.ActionUserText.Trim());

                        SqlDataAdapter adap = new SqlDataAdapter(command);
                        adap.Fill(tbl);
                    }

                    foreach (TestUserVerifyItem verifyTestUserItem in actionItem.VerifyTestUserList)
                    {
                        string verifyTestUserValues = sourceTestCaseItem.TestCaseID + "','";
                        verifyTestUserValues += actionItem.TestActionID + "',";                  
                        verifyTestUserValues += "@userText";                                            

                        query = "Insert into UserVerification values('" + verifyTestUserValues + ")";
                        command = new SqlCommand(query, connect);
                        DataTable tbl = new DataTable();
                        
                        command.Parameters.AddWithValue("@userText", verifyTestUserItem.VerifyUserText.Trim());

                        SqlDataAdapter adap = new SqlDataAdapter(command);
                        adap.Fill(tbl);
                    }

                    ///// To Decide QRCM file need or not 
                   bool IsQRCMFileNeed = false;
                   int qrcmAction = actionItem.SetTestQRCMActionList.Where(x => x.SetPayloadContent != null && x.SetPayloadContent != string.Empty).Count();
                   if (qrcmAction > 0)                        
                         IsQRCMFileNeed = true;
                        
                   int qrcmVerify = actionItem.VerifyTestQRCMList.Where(x => x.SetReferenceContent != null && x.SetReferenceContent != string.Empty).Count();
                   if (qrcmVerify > 0)                        
                        IsQRCMFileNeed = true;
                        
                    if (IsQRCMFileNeed)
                    {
                        ///// QRCM file write
                        string serverPath = QatConstants.QATServerPath + "\\QRCM_Files";

                        if (!Directory.Exists(serverPath))
                        {
                            Directory.CreateDirectory(serverPath);
                        }
                        string QRCMfileName = sourceTestCaseItem.TestCaseID + ".txt";
                        string filePathqrcm = serverPath + "\\" + QRCMfileName;

                        using (StreamWriter writer = new StreamWriter(filePathqrcm, true))
                        {
                            foreach (TestActionQRCMItem setTestQRCMItem in actionItem.SetTestQRCMActionList)
                            {
                                QRCMInitialValues initialValues = setTestQRCMItem.QRCM_MethodsInitialValues.Where(x => x.MethodNameUserView == setTestQRCMItem.QRCM_MethodsSelectedItem).First();
                                string setTestQRCMValues = sourceTestCaseItem.TestCaseID + "','";
                                setTestQRCMValues += actionItem.TestActionID + "','";
                                setTestQRCMValues += initialValues.ProjectName + "','";
                                setTestQRCMValues += initialValues.Buildversion + "','";
                                setTestQRCMValues += initialValues.ReferenceVersion + "','";
                                setTestQRCMValues += setTestQRCMItem.QRCM_DeviceSelectedItem + "','";
                                setTestQRCMValues += setTestQRCMItem.QRCM_DeviceModel[setTestQRCMItem.QRCM_DeviceSelectedItem] + "','";
                                setTestQRCMValues += setTestQRCMItem.QRCM_MethodsSelectedItem + "','";
                                setTestQRCMValues += initialValues.Actual_method_name + "',";
                                setTestQRCMValues += "@argumentsText" + ",'";
                                setTestQRCMValues += initialValues.Input_arguments_Tooltip + "','";

                                ////ReferenceFilePath
                                setTestQRCMValues += string.Empty + "','";

                                ////PayloadFilePath                            
                                setTestQRCMValues += QRCMfileName + "','";

                                setTestQRCMValues += initialValues.HasPreMethod + "','";
                                setTestQRCMValues += initialValues.PreMethodName + "','";
                                setTestQRCMValues += initialValues.PreMethodUserKey + "','";
                                setTestQRCMValues += initialValues.PreMethodActualKey + "','";
                                setTestQRCMValues += initialValues.ArgumentMappingIndex + "','";
                                setTestQRCMValues += initialValues.IsPayloadAvailable + "','";
                                setTestQRCMValues += initialValues.Reference_key + "','";
                                setTestQRCMValues += initialValues.Payload_key + "','";
                                setTestQRCMValues += initialValues.Merge_data + "'";

                                query = "Insert into QRCMAction (TCID,ActionID,Project_Name,Build_version,Reference_Version,Device_name,Device_model,Method_name,Actual_method_name,Input_arguments,Input_arguments_Tooltip,ReferenceFilePath,PayloadFilePath,HasPreMethod,PreMethodName,PreMethodUserKey,PreMethodActualKey,ArgumentMappingIndex,IsPayloadAvailable,Reference_key,Payload_key,Merge_data) values('" + setTestQRCMValues + ");SELECT CONVERT(int,SCOPE_IDENTITY())";
                                command = new SqlCommand(query, connect);
                                DataTable tbl = new DataTable();

                                if (setTestQRCMItem.ActionUserArguments != null)
                                    command.Parameters.AddWithValue("@argumentsText", setTestQRCMItem.ActionUserArguments.Trim());
                                else
                                    command.Parameters.AddWithValue("@argumentsText", string.Empty);

                                SqlDataAdapter adap = new SqlDataAdapter(command);
                                adap.Fill(tbl);


                                if (setTestQRCMItem.SetPayloadBtnIsEnabled && setTestQRCMItem.SetPayloadContent != null && setTestQRCMItem.SetPayloadContent != string.Empty)
                                {
                                    int qrcmUniqueId = 0;
                                    if (tbl.Rows.Count > 0)
                                    {
                                        DataTableReader read1 = tbl.CreateDataReader();

                                        while (read1.Read())
                                        {
                                            qrcmUniqueId = Convert.ToInt32(read1.GetValue(0));
                                        }
                                    }

                                    string QRCMcontentToSave = "{\"Action_" + qrcmUniqueId + "\":" + setTestQRCMItem.SetPayloadContent + "}\n:QAT_Ref_Pay:";
                                    writer.WriteLine(QRCMcontentToSave);
                                }
                            }

                            foreach (TestVerifyQRCMItem verifyTestQRCMItem in actionItem.VerifyTestQRCMList)
                            {
                                QRCMInitialValues initialValues = verifyTestQRCMItem.QRCM_MethodsInitialValues.Where(x => x.MethodNameUserView == verifyTestQRCMItem.QRCM_MethodsSelectedItem).First();
                                string verifyTestQRCMValues = sourceTestCaseItem.TestCaseID + "','";
                                verifyTestQRCMValues += actionItem.TestActionID + "','";
                                verifyTestQRCMValues += initialValues.ProjectName + "','";
                                verifyTestQRCMValues += initialValues.Buildversion + "','";
                                verifyTestQRCMValues += initialValues.ReferenceVersion + "','";
                                verifyTestQRCMValues += verifyTestQRCMItem.QRCM_DeviceSelectedItem + "','";
                                verifyTestQRCMValues += verifyTestQRCMItem.QRCM_DeviceModel[verifyTestQRCMItem.QRCM_DeviceSelectedItem] + "','";
                                verifyTestQRCMValues += verifyTestQRCMItem.QRCM_MethodsSelectedItem + "','";
                                verifyTestQRCMValues += initialValues.Actual_method_name + "',";
                                verifyTestQRCMValues += "@argumentsText" + ",'";
                                verifyTestQRCMValues += initialValues.Input_arguments_Tooltip + "','";

                                //ReferenceFilePath                              
                                verifyTestQRCMValues += QRCMfileName + "','";

                                //PayloadFilePath
                                verifyTestQRCMValues += string.Empty + "','";

                                verifyTestQRCMValues += initialValues.HasPreMethod + "','";
                                verifyTestQRCMValues += initialValues.PreMethodName + "','";
                                verifyTestQRCMValues += initialValues.PreMethodUserKey + "','";
                                verifyTestQRCMValues += initialValues.PreMethodActualKey + "','";
                                verifyTestQRCMValues += initialValues.ArgumentMappingIndex + "','";
                                verifyTestQRCMValues += initialValues.IsPayloadAvailable + "','";
                                verifyTestQRCMValues += initialValues.Reference_key + "','";
                                verifyTestQRCMValues += initialValues.Payload_key + "'";

                                query = "Insert into QRCMVerification(TCID,ActionID,Project_Name,Build_version,Reference_Version,Device_name,Device_model,Method_name,Actual_method_name,Input_arguments,Input_arguments_Tooltip,ReferenceFilePath,PayloadFilePath,HasPreMethod,PreMethodName,PreMethodUserKey,PreMethodActualKey,ArgumentMappingIndex,IsPayloadAvailable,Reference_key,Payload_key) values('" + verifyTestQRCMValues + ");SELECT CONVERT(int,SCOPE_IDENTITY())";
                                command = new SqlCommand(query, connect);
                                DataTable tbl = new DataTable();

                                if (verifyTestQRCMItem.VerifyUserArguments != null)
                                    command.Parameters.AddWithValue("@argumentsText", verifyTestQRCMItem.VerifyUserArguments.Trim());
                                else
                                    command.Parameters.AddWithValue("@argumentsText", string.Empty);

                                SqlDataAdapter adap = new SqlDataAdapter(command);
                                adap.Fill(tbl);

                                if (verifyTestQRCMItem.SetReferenceBtnIsEnabled && verifyTestQRCMItem.SetReferenceContent != null && verifyTestQRCMItem.SetReferenceContent != string.Empty)
                                {
                                    int qrcmUniqueId = 0;
                                    if (tbl.Rows.Count > 0)
                                    {
                                        DataTableReader read1 = tbl.CreateDataReader();

                                        while (read1.Read())
                                        {
                                            qrcmUniqueId = Convert.ToInt32(read1.GetValue(0));
                                        }
                                    }

                                    string QRCMcontentToSave = "{\"Verification_" + qrcmUniqueId + "\":" + verifyTestQRCMItem.SetReferenceContent + "}\n:QAT_Ref_Pay:";
                                    writer.WriteLine(QRCMcontentToSave);
                                }
                            }
                        }
                    }
                    else
                    {
                        foreach (TestActionQRCMItem setTestQRCMItem in actionItem.SetTestQRCMActionList)
                        {
                            QRCMInitialValues initialValues = setTestQRCMItem.QRCM_MethodsInitialValues.Where(x => x.MethodNameUserView == setTestQRCMItem.QRCM_MethodsSelectedItem).First();
                            string setTestQRCMValues = sourceTestCaseItem.TestCaseID + "','";
                            setTestQRCMValues += actionItem.TestActionID + "','";
                            setTestQRCMValues += initialValues.ProjectName + "','";
                            setTestQRCMValues += initialValues.Buildversion + "','";
                            setTestQRCMValues += initialValues.ReferenceVersion + "','";
                            setTestQRCMValues += setTestQRCMItem.QRCM_DeviceSelectedItem + "','";
                            setTestQRCMValues += setTestQRCMItem.QRCM_DeviceModel[setTestQRCMItem.QRCM_DeviceSelectedItem] + "','";
                            setTestQRCMValues += setTestQRCMItem.QRCM_MethodsSelectedItem + "','";
                            setTestQRCMValues += initialValues.Actual_method_name + "',";
                            setTestQRCMValues += "@argumentsText" + ",'";
                            setTestQRCMValues += initialValues.Input_arguments_Tooltip + "','";

                            ////ReferenceFilePath
                            setTestQRCMValues += string.Empty + "','";

                            ////PayloadFilePath                          
                            setTestQRCMValues += string.Empty + "','";

                            setTestQRCMValues += initialValues.HasPreMethod + "','";
                            setTestQRCMValues += initialValues.PreMethodName + "','";
                            setTestQRCMValues += initialValues.PreMethodUserKey + "','";
                            setTestQRCMValues += initialValues.PreMethodActualKey + "','";
                            setTestQRCMValues += initialValues.ArgumentMappingIndex + "','";
                            setTestQRCMValues += initialValues.IsPayloadAvailable + "','";
                            setTestQRCMValues += initialValues.Reference_key + "','";
                            setTestQRCMValues += initialValues.Payload_key + "','";
                            setTestQRCMValues += initialValues.Merge_data + "'";

                            query = "Insert into QRCMAction (TCID,ActionID,Project_Name,Build_version,Reference_Version,Device_name,Device_model,Method_name,Actual_method_name,Input_arguments,Input_arguments_Tooltip,ReferenceFilePath,PayloadFilePath,HasPreMethod,PreMethodName,PreMethodUserKey,PreMethodActualKey,ArgumentMappingIndex,IsPayloadAvailable,Reference_key,Payload_key,Merge_data) values('" + setTestQRCMValues + ");SELECT CONVERT(int,SCOPE_IDENTITY())";
                            command = new SqlCommand(query, connect);
                            DataTable tbl = new DataTable();

                            if (setTestQRCMItem.ActionUserArguments != null)
                                command.Parameters.AddWithValue("@argumentsText", setTestQRCMItem.ActionUserArguments.Trim());
                            else
                                command.Parameters.AddWithValue("@argumentsText", string.Empty);

                            SqlDataAdapter adap = new SqlDataAdapter(command);
                            adap.Fill(tbl);
                        }

                        foreach (TestVerifyQRCMItem verifyTestQRCMItem in actionItem.VerifyTestQRCMList)
                        {
                            QRCMInitialValues initialValues = verifyTestQRCMItem.QRCM_MethodsInitialValues.Where(x => x.MethodNameUserView == verifyTestQRCMItem.QRCM_MethodsSelectedItem).First();
                            string verifyTestQRCMValues = sourceTestCaseItem.TestCaseID + "','";
                            verifyTestQRCMValues += actionItem.TestActionID + "','";
                            verifyTestQRCMValues += initialValues.ProjectName + "','";
                            verifyTestQRCMValues += initialValues.Buildversion + "','";
                            verifyTestQRCMValues += initialValues.ReferenceVersion + "','";
                            verifyTestQRCMValues += verifyTestQRCMItem.QRCM_DeviceSelectedItem + "','";
                            verifyTestQRCMValues += verifyTestQRCMItem.QRCM_DeviceModel[verifyTestQRCMItem.QRCM_DeviceSelectedItem] + "','";
                            verifyTestQRCMValues += verifyTestQRCMItem.QRCM_MethodsSelectedItem + "','";
                            verifyTestQRCMValues += initialValues.Actual_method_name + "',";
                            verifyTestQRCMValues += "@argumentsText" + ",'";
                            verifyTestQRCMValues += initialValues.Input_arguments_Tooltip + "','";

                            //ReferenceFilePath                          
                            verifyTestQRCMValues += string.Empty + "','";

                            //PayloadFilePath
                            verifyTestQRCMValues += string.Empty + "','";

                            verifyTestQRCMValues += initialValues.HasPreMethod + "','";
                            verifyTestQRCMValues += initialValues.PreMethodName + "','";
                            verifyTestQRCMValues += initialValues.PreMethodUserKey + "','";
                            verifyTestQRCMValues += initialValues.PreMethodActualKey + "','";
                            verifyTestQRCMValues += initialValues.ArgumentMappingIndex + "','";
                            verifyTestQRCMValues += initialValues.IsPayloadAvailable + "','";
                            verifyTestQRCMValues += initialValues.Reference_key + "','";
                            verifyTestQRCMValues += initialValues.Payload_key + "'";

                            query = "Insert into QRCMVerification(TCID,ActionID,Project_Name,Build_version,Reference_Version,Device_name,Device_model,Method_name,Actual_method_name,Input_arguments,Input_arguments_Tooltip,ReferenceFilePath,PayloadFilePath,HasPreMethod,PreMethodName,PreMethodUserKey,PreMethodActualKey,ArgumentMappingIndex,IsPayloadAvailable,Reference_key,Payload_key) values('" + verifyTestQRCMValues + ");SELECT CONVERT(int,SCOPE_IDENTITY())";
                            command = new SqlCommand(query, connect);
                            DataTable tbl = new DataTable();

                            if (verifyTestQRCMItem.VerifyUserArguments != null)
                                command.Parameters.AddWithValue("@argumentsText", verifyTestQRCMItem.VerifyUserArguments.Trim());
                            else
                                command.Parameters.AddWithValue("@argumentsText", string.Empty);

                            SqlDataAdapter adap = new SqlDataAdapter(command);
                            adap.Fill(tbl);
                        }
                    }
                    

                    foreach (TestScriptVerification verifyTestScriptItem in actionItem.VerifyTestScriptList)
                    {
                        string verifyTestScriptValues = sourceTestCaseItem.TestCaseID + "','";
                        verifyTestScriptValues += actionItem.TestActionID + "','";
                        verifyTestScriptValues += verifyTestScriptItem.DevicenamelistSelectedItem + "','";
                        verifyTestScriptValues += verifyTestScriptItem.DeviceModel + "',";
                        verifyTestScriptValues += "@Action,";
                        verifyTestScriptValues += "@Command,";
                        verifyTestScriptValues += "@RegexMatch,'";
                        verifyTestScriptValues += verifyTestScriptItem.Upperlimit + "','";
                        verifyTestScriptValues += verifyTestScriptItem.Lowerlimit + "','";
                        verifyTestScriptValues += verifyTestScriptItem.LimitUnitSelectedItem + "','";
                        verifyTestScriptValues += actionItem.Script_checktimeTextbox + "','";
                        verifyTestScriptValues += actionItem.Script_ChecktimeUnitSelected + "','";
                        verifyTestScriptValues += actionItem.Script_DurationTextbox + "','";
                        verifyTestScriptValues += actionItem.Script_DurationTimeUnitSelected + "','";
                        verifyTestScriptValues += verifyTestScriptItem.VerifyDesignDevicesIsChecked + "','";
                        verifyTestScriptValues += actionItem.ExecuteIterationChkboxIsChecked;
                        verifyTestScriptValues += "','" + string.Empty + "";

                        query = "Insert into ScriptVerification values('" + verifyTestScriptValues + "')";
                        command = new SqlCommand(query, connect);
                        DataTable tbl = new DataTable();

                        string action = verifyTestScriptItem.VerifyScriptActionSelectedItem;
                        if (verifyTestScriptItem.VerifyScriptActionSelectedItem == "CPU Monitoring")
                            action += "-" + verifyTestScriptItem.CPUNumberSelectedItem;

                        command.Parameters.AddWithValue("@Action", action);
                        command.Parameters.AddWithValue("@Command", verifyTestScriptItem.CustomCommand);
                        command.Parameters.AddWithValue("@RegexMatch", verifyTestScriptItem.RegexMatch);

                        SqlDataAdapter adap = new SqlDataAdapter(command);
                        adap.Fill(tbl);
                    }

                    foreach (TestVerifyQRItem verifyTestQRItem in actionItem.VerifyTestQRList)
                    {

                        string verifyTestQRValues = sourceTestCaseItem.TestCaseID + "','";

                        verifyTestQRValues += actionItem.TestActionID + "','";
                        verifyTestQRValues += verifyTestQRItem.CameraSelectedItem + "','";
                        if (verifyTestQRItem.CameraSelectedItem != null)
                        {
                            verifyTestQRValues += verifyTestQRItem.CameraList[verifyTestQRItem.CameraSelectedItem] + "',";
                        }
                        else
                        {
                            verifyTestQRValues += "',";
                        }

                        verifyTestQRValues += "@opcode" + ",'";
                        verifyTestQRValues += verifyTestQRItem.QRverifytype + "'";


                        query = "Insert into QRVerification values('" + verifyTestQRValues + ")";
                        command = new SqlCommand(query, connect);
                        DataTable tbl = new DataTable();

                        if (verifyTestQRItem.QRverificationcode == null)
                            verifyTestQRItem.QRverificationcode = string.Empty;

                        command.Parameters.AddWithValue("@opcode", verifyTestQRItem.QRverificationcode.Trim());


                        SqlDataAdapter dap = new SqlDataAdapter(command);
                        dap.Fill(tbl);
                    }
                    foreach (TestTelnetItem verifyTestTelnetItem in actionItem.VerifyTestTelnetList)
                    {
                        string verifyTestTelnetValues = sourceTestCaseItem.TestCaseID + "','";
                        verifyTestTelnetValues += actionItem.TestActionID + "','";
                        verifyTestTelnetValues += verifyTestTelnetItem.TelnetVerifyTypeSelected + "',";
                        verifyTestTelnetValues += "@TelnetFailureText";
                        verifyTestTelnetValues += ",'" + verifyTestTelnetItem.KeywordTypeVerify;
                        //query = "Insert into TelnetVerify values('" + verifyTestTelnetValues + "')";
                        //command = new SqlCommand(query, connect);
                        //command.ExecuteScalar();

                        query = "Insert into TelnetVerify values('" + verifyTestTelnetValues + "')";
                        command = new SqlCommand(query, connect);
                        DataTable tbl = new DataTable();
                        if (verifyTestTelnetItem.TelnetFailureText == null)
                            verifyTestTelnetItem.TelnetFailureText = string.Empty;
                        command.Parameters.AddWithValue("@TelnetFailureText", verifyTestTelnetItem.TelnetFailureText);
                        SqlDataAdapter dap = new SqlDataAdapter(command);
                        dap.Fill(tbl);
                    }

                    foreach (TestUsbAudioBridging verifyTestUsbItem in actionItem.VerifyTestUsbList)
                    {
                        query = "Insert into UsbVerify values('" + sourceTestCaseItem.TestCaseID + "','" + actionItem.TestActionID + "','" + verifyTestUsbItem.UsbAudioBridgeTypeSelectedItem + "','" + verifyTestUsbItem.UsbAudioTypeSelectedItem + "','" + verifyTestUsbItem.UsbAudioDeviceSelectedItem + "','" + verifyTestUsbItem.UsbDefaultDeviceOptionSelectedItem + "')";
                        command = new SqlCommand(query, connect);
                        DataTable tbl = new DataTable();
                        SqlDataAdapter dap = new SqlDataAdapter(command);
                        dap.Fill(tbl);
                    }
                    foreach (var verifyTestlogItem in actionItem.VerifyTestLogList)
                    {
                        string verifyTestlogItemValues = sourceTestCaseItem.TestCaseID + "','";
                        verifyTestlogItemValues += actionItem.TestActionID + "','";
                        verifyTestlogItemValues += verifyTestlogItem.iLogIsChecked + "','";
                        verifyTestlogItemValues += verifyTestlogItem.iLog_selected_item + "',";
                        verifyTestlogItemValues += "@ilogtext" + ",'";


                        verifyTestlogItemValues += verifyTestlogItem.KernelLogIsChecked + "','";
                        verifyTestlogItemValues += verifyTestlogItem.kernalLog_selected_item + "',";
                        verifyTestlogItemValues += "@kernallogtext" + ",'";

                        verifyTestlogItemValues += verifyTestlogItem.EventLogIsChecked + "',";
                        verifyTestlogItemValues += "@eventlogtext" + ",'";

                        verifyTestlogItemValues += verifyTestlogItem.ConfiguratorIsChecked + "',";
                        verifyTestlogItemValues += "@configuratorlogtext" + ",'";

                        verifyTestlogItemValues += verifyTestlogItem.SIPLogIsChecked + "',";
                        verifyTestlogItemValues += "@siplogtext" + ",'";

                        verifyTestlogItemValues += verifyTestlogItem.QsysAppLogIsChecked + "',";
                        verifyTestlogItemValues += "@qsysapplogtext" + ",'";

                        verifyTestlogItemValues += verifyTestlogItem.UCIViewerLogIsChecked + "',";
                        verifyTestlogItemValues += "@UCIlogtext" + ",'";

                        verifyTestlogItemValues += verifyTestlogItem.SoftPhoneLogIsChecked + "',";
                        verifyTestlogItemValues += "@softphonelogtext" + ",'";

                        verifyTestlogItemValues += verifyTestlogItem.WindowsEventLogsIsChecked + "',";
                        verifyTestlogItemValues += "@windowseventlogtext";

                        //query = "Insert into logverification values('" + verifyTestlogItemValues + "')";
                        //command = new SqlCommand(query, connect);
                        //command.ExecuteScalar();

                        query = "Insert into logverification values('" + verifyTestlogItemValues + ")";
                        command = new SqlCommand(query, connect);
                        DataTable tbl = new DataTable();
                        if (verifyTestlogItem.ilogtext == null)
                            verifyTestlogItem.ilogtext = string.Empty;
                        if (verifyTestlogItem.kernallogtext == null)
                            verifyTestlogItem.kernallogtext = string.Empty;
                        if (verifyTestlogItem.eventlogtext == null)
                            verifyTestlogItem.eventlogtext = string.Empty;
                        if (verifyTestlogItem.configuratorlogtext == null)
                            verifyTestlogItem.configuratorlogtext = string.Empty;
                        if (verifyTestlogItem.siplogtext == null)
                            verifyTestlogItem.siplogtext = string.Empty;
                        if (verifyTestlogItem.qsysapplogtext == null)
                            verifyTestlogItem.qsysapplogtext = string.Empty;
                        if (verifyTestlogItem.UCIlogtext == null)
                            verifyTestlogItem.UCIlogtext = string.Empty;
                        if (verifyTestlogItem.softphonelogtext == null)
                            verifyTestlogItem.softphonelogtext = string.Empty;
                        if (verifyTestlogItem.windowseventlogtext == null)
                            verifyTestlogItem.windowseventlogtext = string.Empty;

                        command.Parameters.AddWithValue("@ilogtext", verifyTestlogItem.ilogtext);
                        command.Parameters.AddWithValue("@kernallogtext", verifyTestlogItem.kernallogtext);
                        command.Parameters.AddWithValue("@eventlogtext", verifyTestlogItem.eventlogtext);
                        command.Parameters.AddWithValue("@configuratorlogtext", verifyTestlogItem.configuratorlogtext);
                        command.Parameters.AddWithValue("@siplogtext", verifyTestlogItem.siplogtext);
                        command.Parameters.AddWithValue("@qsysapplogtext", verifyTestlogItem.qsysapplogtext);
                        command.Parameters.AddWithValue("@UCIlogtext", verifyTestlogItem.UCIlogtext);
                        command.Parameters.AddWithValue("@softphonelogtext", verifyTestlogItem.softphonelogtext);
                        command.Parameters.AddWithValue("@windowseventlogtext", verifyTestlogItem.windowseventlogtext);
                        SqlDataAdapter dap = new SqlDataAdapter(command);
                        dap.Fill(tbl);

                        if (verifyTestlogItem.PcapLogIsChecked)
                        {
                            //string pcapLogItemValues = newlogverifyId.ToString() + "','";
                            string pcapLogItemValues = sourceTestCaseItem.TestCaseID + "','";
                            pcapLogItemValues += actionItem.TestActionID + "','";

                            if (verifyTestlogItem.PcaplogDelaySetting != null && verifyTestlogItem.PcaplogDelaySetting != string.Empty)
                                pcapLogItemValues += verifyTestlogItem.PcaplogDelaySetting + "','";
                            else
                                pcapLogItemValues += "3" + "','";

                            if (verifyTestlogItem.PcapDelayUnitSelected != null && verifyTestlogItem.PcapDelayUnitSelected != string.Empty)
                                pcapLogItemValues += verifyTestlogItem.PcapDelayUnitSelected + "'";
                            else
                                pcapLogItemValues += "Sec" + "'";

                            foreach (var pcapItem in verifyTestlogItem.SetTestPcapList)
                            {
                                query = "Insert into PcapVerification values('" + pcapLogItemValues + ",@PcapProtocolName,@PcapFieldText)";
                                command = new SqlCommand(query, connect);
                                DataTable tbl1 = new DataTable();

                                if (pcapItem.PcapProtocolName.Trim() != null)
                                    command.Parameters.AddWithValue("@PcapProtocolName", pcapItem.PcapProtocolName.Trim() + ";" + verifyTestlogItem.PcapSelectLanComboSelecteditem + ";" + verifyTestlogItem.PcapSelectFilterComboSelecteditem + ";" + verifyTestlogItem.PcapFilterByIP.Trim() + ";" + verifyTestlogItem.PcapNotFilterByIP.ToString().ToLower());
                                else
                                    command.Parameters.AddWithValue("@PcapProtocolName", string.Empty);

                                if (pcapItem.PcapFieldText.Trim() != null)
                                    command.Parameters.AddWithValue("@PcapFieldText", pcapItem.PcapFieldText.Trim());
                                else
                                    command.Parameters.AddWithValue("@PcapFieldText", string.Empty);

                                SqlDataAdapter dap1 = new SqlDataAdapter(command);
                                dap1.Fill(tbl);
                            }
                        }
                    }

                    foreach (TestFirmwareItem setTestFirmwareItem in actionItem.SetTestFirmwareList)
                    {
                        string setTestFirmwareValues = sourceTestCaseItem.TestCaseID + "','";
                        setTestFirmwareValues += actionItem.TestActionID + "','";

                        int index = QatConstants.TestFirmwareItemFirmwareUpdateTypeDisplayList.IndexOf(setTestFirmwareItem.FirmwareTypeSelected);
                        if (index < 0)
                            setTestFirmwareValues += string.Empty + "',";
                        else
                            setTestFirmwareValues += QatConstants.TestFirmwareItemFirmwareUpdateTypeDatabaseList[index] + "',";

                        setTestFirmwareValues += "@BrowseLocation" + ",'";
                        if (setTestFirmwareItem.FirmwareTypeSelected == "Start auto update with new version of SW at")
                        {
                            setTestFirmwareValues += setTestFirmwareItem.FirmwareDate + "','";
                        }
                        else
                        {
                            setTestFirmwareValues += string.Empty + "','";
                        }
                        if ((setTestFirmwareItem.FirmwareTypeSelected == "Start auto update with new version of SW at") | (setTestFirmwareItem.FirmwareTypeSelected == "Automatically update when new version of SW available"))
                        {
                            setTestFirmwareValues += setTestFirmwareItem.InstallSelectionComboSelectedItem + "','";
                        }
                        else
                        {
                            setTestFirmwareValues += string.Empty + "','";
                        }
                        if ((setTestFirmwareItem.FirmwareTypeSelected == "Start auto update with new version of SW at"))
                        {
                            setTestFirmwareValues += setTestFirmwareItem.TimeSelectionComboSelectedItem;
                        }
                        else
                        {
                            setTestFirmwareValues += string.Empty;
                        }

                        setTestFirmwareValues += "','" + setTestFirmwareItem.MeasureFirmwareUpTime.ToString();

                        //query = "Insert into FirmwareAction values('" + setTestFirmwareValues + "')";
                        //command = new SqlCommand(query, connect);
                        //command.ExecuteScalar();

                        query = "Insert into FirmwareAction values('" + setTestFirmwareValues + "')";
                        command = new SqlCommand(query, connect);
                        DataTable tbl = new DataTable();
                        if (setTestFirmwareItem.FirmwareBrowseLocation == null)
                            setTestFirmwareItem.FirmwareBrowseLocation = string.Empty;
                        command.Parameters.AddWithValue("@BrowseLocation", setTestFirmwareItem.FirmwareBrowseLocation);
                        SqlDataAdapter dap = new SqlDataAdapter(command);
                        dap.Fill(tbl);
                    }


                    foreach (TestNetPairingItem NetPairAction in actionItem.SetTestNetPairingList)
                    {
                        foreach (DUT_DeviceItem device in NetPairAction.DutDeviceList)
                        {
                            query = "Insert into NetpairingAction values('" + sourceTestCaseItem.TestCaseID + "','" + actionItem.TestActionID + "','" + device.ItemDeviceType + "','" + device.ItemDeviceName + "','" + device.ItemNetPairingSelected + "')";
                            command = new SqlCommand(query, connect);
                            DataTable tbl = new DataTable();
                            SqlDataAdapter dap = new SqlDataAdapter(command);
                            dap.Fill(tbl);
                        }
                    }

                    foreach (TestDesignerItem DesignAction in actionItem.SetTestDesignerList)
                    {
                        string noofTimesdeployed = string.Empty;
                        if (DesignAction.ChkNoOfTimeDeployCheck || DesignAction.Loadfromcore)
                            noofTimesdeployed = DesignAction.NoOfTimesDeployed;

                        query = "Insert into DesignerAction values('" + sourceTestCaseItem.TestCaseID + "','" + actionItem.TestActionID + "','" + DesignAction.ConnectDesigner.ToString() + "','" + DesignAction.DisconnectDesigner.ToString() + "','" + DesignAction.EmulateDesigner.ToString() + "','" + DesignAction.newdesigncheck.ToString() + "','" + DesignAction.ChkNoOfTimeDeployCheck + "','" + noofTimesdeployed + "','" + DesignAction.DesignerTimeout + "','" +DesignAction.DesignerTimeoutUnitSelected + "','" + DesignAction.Loadfromcore + "')";

                        command = new SqlCommand(query, connect);
                        DataTable tbl = new DataTable();
                        SqlDataAdapter dap = new SqlDataAdapter(command);
                        dap.Fill(tbl);
                    }

                    foreach (TestUsbAudioBridging UsbAudio in actionItem.SetTestUsbList)
                    {
                        query = "Insert into UsbAction values('" + sourceTestCaseItem.TestCaseID + "','" + actionItem.TestActionID + "','" + UsbAudio.UsbAudioBridgeTypeSelectedItem + "','" + UsbAudio.UsbAudioTypeSelectedItem + "','" + UsbAudio.UsbAudioDeviceSelectedItem + "','" + UsbAudio.UsbDefaultDeviceOptionSelectedItem + "')";
                        command = new SqlCommand(query, connect);
                        DataTable tbl = new DataTable();
                        SqlDataAdapter dap = new SqlDataAdapter(command);
                        dap.Fill(tbl);
                    }


                    Dictionary<string, string> renamekeyval = new Dictionary<string, string>();

                    foreach (TestResponsalyzerItem setResponsalyzerAction in actionItem.verifyTestResponsalyzerList)
                    {
                        string responsalyzerFile = string.Empty;
                        string filepath = setResponsalyzerAction.TestResponsalyzerVerificationFile;
                        string filename = Path.GetFileName(filepath);


                        //string val = Path.GetFileNameWithoutExtension(filepath);
                        //string extenstion = Path.GetExtension(filepath);
                        //int count = ++i;
                        //val = val + "_" + sourceTestCaseItem.TestCaseID + "_" + actionItem.TestActionID + "_" + count.ToString() + extenstion;

                        //responsalyzerFile = setResponsalyzerAction.TestResponsalyzerVerificationFile;
                        //responsalyzerFile = StoreResponsalyzerFile(responsalyzerFile, sourceTestCaseItem.TestCaseID, actionItem.TestActionID, val);

                        string setTestResponsalyzerValues = sourceTestCaseItem.TestCaseID + "','";
                        setTestResponsalyzerValues += actionItem.TestActionID + "',";
                        setTestResponsalyzerValues += "@ResponsalyzerName" + ",";
                        setTestResponsalyzerValues += "@ResponsalyzerGraphSelected" + ",";
                        setTestResponsalyzerValues += "@TextFileLocation";
                        query = "Insert into Responsalyzer values('" + setTestResponsalyzerValues + ");SELECT CONVERT(int,SCOPE_IDENTITY())";
                        command = new SqlCommand(query, connect);
                        DataTable tbl = new DataTable();

                        command.Parameters.AddWithValue("@ResponsalyzerName", setResponsalyzerAction.TestResponsalyzerNameSelectedItem);
                        command.Parameters.AddWithValue("@ResponsalyzerGraphSelected", setResponsalyzerAction.TestResponsalyzerTypeSelectedItem);
                        command.Parameters.AddWithValue("@TextFileLocation", responsalyzerFile);
                        SqlDataAdapter dap = new SqlDataAdapter(command);
                        dap.Fill(tbl);

                        int newId = 0;
                        if (tbl.Rows.Count > 0)
                        {
                            DataTableReader read1 = tbl.CreateDataReader();

                            while (read1.Read())
                            {
                                newId = Convert.ToInt32(read1.GetValue(0));
                            }
                        }

                        if (setResponsalyzerAction.CopyItemSource == true && filepath != null && filepath != string.Empty && File.Exists(filepath) == false)
                        {
                            if (renamekeyval.Keys.Contains(filepath))
                                filepath = Path.Combine(QatConstants.QATServerPath, "Responsalyzer", "Reference Files", renamekeyval[filepath]);
                            else
                                filepath = Path.Combine(QatConstants.QATServerPath, "Responsalyzer", "Reference Files", filepath);
                        }

                        responsalyzerFile = SaveFilesInServerResponse(copyItem, filepath, filename, setResponsalyzerAction.IsNewReferenceFile, sourceTestCaseItem.TestCaseID, actionItem.TestActionID, oldTAID, "Responsalyzer", "Reference Files", responsalyzerID, newId, setResponsalyzerAction.CopyItemSource);
                        setResponsalyzerAction.TestResponsalyzerVerificationFile = responsalyzerFile;
                        setResponsalyzerAction.IsNewReferenceFile = false;
                        setResponsalyzerAction.CopyItemSource = false;

                        if (!renamekeyval.Keys.Contains(filename))
                            renamekeyval.Add(filename, responsalyzerFile);


                        if (newId > 0)
                        {
                            tbl.Clear();
                            query = "update Responsalyzer set VerificationFileLocation = @TextFileLocation where ResponsalyzerID = '" + newId + "'";
                            command = new SqlCommand(query, connect);
                            command.Parameters.AddWithValue("@TextFileLocation", responsalyzerFile);
                            SqlDataAdapter adapter = new SqlDataAdapter(command);
                            adapter.Fill(tbl);
                        }
                    }

                    foreach (string file in fileToDeleteResponse)
                    {
                        if (file != null && file != string.Empty)
                        {
                            string path = Path.Combine(QatConstants.QATServerPath, "Responsalyzer", "Reference Files", file);

                            if (File.Exists(path))
                            {
                                File.SetAttributes(path, FileAttributes.Normal);
                                File.Delete(path);
                            }
                        }
                    }


                    foreach (TestApxItem testApItem in actionItem.VerifyTestApxList)
                    {
                        /////Save APx Settings
                        List<string> waveList = new List<string> { "Sine", "Sine, Dual", "Sine, Var Phase", "Noise", "IMD", "Browse for file..." };

                        string ModeType = testApItem.APxSettingsList[0].cmbTypeOfMode;
                        if (ModeType == "SequenceMode")
                        {
                            var lst = waveList.Where(x => x.Equals(testApItem.APxSettingsList[0].cmbSeqWaveForm)).Count();
                            string seqPaths = testApItem.APxSettingsList[0].cmbSeqWaveForm;

                            if (lst == 0)
                            {
                                string path = testApItem.APxSettingsList[0].WaveFilePathList.Find(x => x.Contains(testApItem.APxSettingsList[0].cmbSeqWaveForm));

                                bool val = false;
                                if (testApItem.APxSettingsList[0].isNewWaveform.Keys.Contains(testApItem.APxSettingsList[0].cmbSeqWaveForm))
                                {
                                    val = testApItem.APxSettingsList[0].isNewWaveform[testApItem.APxSettingsList[0].cmbSeqWaveForm];

                                    testApItem.APxSettingsList[0].isNewWaveform[testApItem.APxSettingsList[0].cmbSeqWaveForm] = false;
                                }

                                seqPaths = SaveFilesInServer(copyItem, path, testApItem.APxSettingsList[0].cmbSeqWaveForm, val, sourceTestCaseItem.TestCaseID, actionItem.TestActionID, oldTAID, "Audio Precision", "AP Waveform Files");

                                if (testApItem.APxSettingsList[0].cmbSeqWaveForm != null && testApItem.APxSettingsList[0].cmbSeqWaveForm != string.Empty)
                                {
                                    testApItem.APxSettingsList[0].cmb_SeqWaveformList.Clear();
                                    testApItem.APxSettingsList[0].cmb_SeqWaveformList = new ObservableCollection<string>(waveList);
                                    testApItem.APxSettingsList[0].cmb_SeqWaveformList.Add(seqPaths);

                                    testApItem.APxSettingsList[0].isNewWaveform.Clear();
                                    testApItem.APxSettingsList[0].isNewWaveform.Add(seqPaths, false);
                                }
                            }

                            query = "Insert into APSettings values('" + sourceTestCaseItem.TestCaseID + "','" + actionItem.TestActionID + "','" + ModeType + "','" + testApItem.APxSettingsList[0].ChkSeqGenON + "',@WaveType,'" + testApItem.APxSettingsList[0].ChkSeqTrackCh + "','" + testApItem.APxSettingsList[0].TxtSeqLevelCh1 + "','" + testApItem.APxSettingsList[0].TxtSeqLevelDcCh1 + "','" + testApItem.APxSettingsList[0].TxtSeqLevelCh2 + "','" + testApItem.APxSettingsList[0].TxtSeqLevelDcCh2 + "','" + testApItem.APxSettingsList[0].TxtSeqLevelCh3 + "','" + testApItem.APxSettingsList[0].TxtSeqLevelDcCh3 + "','" + testApItem.APxSettingsList[0].TxtSeqLevelCh4 + "','" + testApItem.APxSettingsList[0].TxtSeqLevelDcCh4 + "','" + testApItem.APxSettingsList[0].TxtSeqLevelCh5 + "','" + testApItem.APxSettingsList[0].TxtSeqLevelDcCh5 + "','" + testApItem.APxSettingsList[0].TxtSeqLevelCh6 + "','" + testApItem.APxSettingsList[0].TxtSeqLevelDcCh6 + "','" + testApItem.APxSettingsList[0].TxtSeqLevelCh7 + "','" + testApItem.APxSettingsList[0].TxtSeqLevelDcCh7 + "','" + testApItem.APxSettingsList[0].TxtSeqLevelCh8 + "','" + testApItem.APxSettingsList[0].TxtSeqLevelDcCh8 + "','" + testApItem.APxSettingsList[0].TxtSeqFreqA + "','" + testApItem.APxSettingsList[0].TxtSeqFreqB + "','" + testApItem.APxSettingsList[0].ChkSeqCh1Enable + "','" + testApItem.APxSettingsList[0].ChkSeqCh2Enable + "','" + testApItem.APxSettingsList[0].ChkSeqCh3Enable + "','" + testApItem.APxSettingsList[0].ChkSeqCh4Enable + "','" + testApItem.APxSettingsList[0].ChkSeqCh5Enable + "','" + testApItem.APxSettingsList[0].ChkSeqCh6Enable + "','" + testApItem.APxSettingsList[0].ChkSeqCh7Enable + "','" + testApItem.APxSettingsList[0].ChkSeqCh8Enable + "','" + testApItem.APxSettingsList[0].cmbSeqTestChannel + "','" + testApItem.APxSettingsList[0].TxtSeqDelay + "','" + testApItem.APxSettingsList[0].SeqSetupCount + "', '" + string.Empty + "')";
                            command = new SqlCommand(query, connect);
                            command.Parameters.AddWithValue("@WaveType", seqPaths);
                            command.ExecuteScalar();

                            testApItem.APxSettingsList[0].cmbSeqWaveForm = seqPaths;
                        }
                        else if (ModeType == "BenchMode")
                        {
                            var lst = waveList.Where(x => x.Equals(testApItem.APxSettingsList[0].cmb_BenchWaveform)).Count();
                            string benchPaths = testApItem.APxSettingsList[0].cmb_BenchWaveform;

                            if (lst == 0)
                            {
                                string path = testApItem.APxSettingsList[0].WaveFilePathList.Find(x => x.Contains(testApItem.APxSettingsList[0].cmb_BenchWaveform));

                                bool val = false;
                                if (testApItem.APxSettingsList[0].isNewWaveform.Keys.Contains(testApItem.APxSettingsList[0].cmb_BenchWaveform))
                                {
                                    val = testApItem.APxSettingsList[0].isNewWaveform[testApItem.APxSettingsList[0].cmb_BenchWaveform];

                                    testApItem.APxSettingsList[0].isNewWaveform[testApItem.APxSettingsList[0].cmb_BenchWaveform] = false;
                                }

                                benchPaths = SaveFilesInServer(copyItem, path, testApItem.APxSettingsList[0].cmb_BenchWaveform, val, sourceTestCaseItem.TestCaseID, actionItem.TestActionID, oldTAID, "Audio Precision", "AP Waveform Files");

                                if (testApItem.APxSettingsList[0].cmb_BenchWaveform != null && testApItem.APxSettingsList[0].cmb_BenchWaveform != string.Empty)
                                {
                                    testApItem.APxSettingsList[0].cmb_BenchWaveformList.Clear();
                                    testApItem.APxSettingsList[0].cmb_BenchWaveformList = new ObservableCollection<string>(waveList);
                                    testApItem.APxSettingsList[0].cmb_BenchWaveformList.Add(benchPaths);

                                    testApItem.APxSettingsList[0].isNewWaveform.Clear();
                                    testApItem.APxSettingsList[0].isNewWaveform.Add(benchPaths, false);
                                }
                            }

                            query = "Insert into APSettings values('" + sourceTestCaseItem.TestCaseID + "','" + actionItem.TestActionID + "','" + ModeType + "','" + testApItem.APxSettingsList[0].ChkBenchGenON + "',@WaveType,'" + testApItem.APxSettingsList[0].chkBx_BenchLevelTrackCh + "','" + testApItem.APxSettingsList[0].txt_BenchLevelCh1 + "','" + testApItem.APxSettingsList[0].txt_BenchDcOffsetCh1 + "','" + testApItem.APxSettingsList[0].txt_BenchLevelCh2 + "','" + testApItem.APxSettingsList[0].txt_BenchDcOffsetCh2 + "','" + testApItem.APxSettingsList[0].txt_BenchLevelCh3 + "','" + testApItem.APxSettingsList[0].txt_BenchDcOffsetCh3 + "','" + testApItem.APxSettingsList[0].txt_BenchLevelCh4 + "','" + testApItem.APxSettingsList[0].txt_BenchDcOffsetCh4 + "','" + testApItem.APxSettingsList[0].txt_BenchLevelCh5 + "','" + testApItem.APxSettingsList[0].txt_BenchDcOffsetCh5 + "','" + testApItem.APxSettingsList[0].txt_BenchLevelCh6 + "','" + testApItem.APxSettingsList[0].txt_BenchDcOffsetCh6 + "','" + testApItem.APxSettingsList[0].txt_BenchLevelCh7 + "','" + testApItem.APxSettingsList[0].txt_BenchDcOffsetCh7 + "','" + testApItem.APxSettingsList[0].txt_BenchLevelCh8 + "','" + testApItem.APxSettingsList[0].txt_BenchDcOffsetCh8 + "','" + testApItem.APxSettingsList[0].txt_BenchfrequencyA + "','" + testApItem.APxSettingsList[0].txt_BenchfrequencyB + "','" + testApItem.APxSettingsList[0].ChkBenchCh1Enable + "','" + testApItem.APxSettingsList[0].ChkBenchCh2Enable + "','" + testApItem.APxSettingsList[0].ChkBenchCh3Enable + "','" + testApItem.APxSettingsList[0].ChkBenchCh4Enable + "','" + testApItem.APxSettingsList[0].ChkBenchCh5Enable + "','" + testApItem.APxSettingsList[0].ChkBenchCh6Enable + "','" + testApItem.APxSettingsList[0].ChkBenchCh7Enable + "','" + testApItem.APxSettingsList[0].ChkBenchCh8Enable + "','" + string.Empty + "','" + string.Empty + "','" + string.Empty + "', '" + testApItem.APxSettingsList[0].BenchSetupCount + "')";
                            command = new SqlCommand(query, connect);
                            command.Parameters.AddWithValue("@WaveType", benchPaths);
                            command.ExecuteScalar();

                            testApItem.APxSettingsList[0].cmb_BenchWaveform = benchPaths;
                        }

                        /////Save Apx Verification

                        string gainVerify = testApItem.cmbTypeOfVerfication;

                        string apPath = SaveFilesInServer(copyItem, testApItem.APxBrowseLocation, testApItem.APxLocationTimeStamp, testApItem.isAPXFileLoaded, sourceTestCaseItem.TestCaseID, actionItem.TestActionID, oldTAID, "Audio Precision", "AP Project Files");

                        query = "Insert into APVerification values('" + sourceTestCaseItem.TestCaseID + "','" + actionItem.TestActionID + "', '" + gainVerify + "','" + apPath + "')";
                        command = new SqlCommand(query, connect);
                        command.ExecuteScalar();

                        testApItem.isAPXFileLoaded = false;
                        testApItem.APxLocationTimeStamp = apPath;

                        if (gainVerify == "Level and Gain")
                        {
                            var lst = waveList.Where(x => x.Equals(testApItem.APxLevelAndGainList[0].cmb_GainWaveform)).Count();
                            string gainPaths = testApItem.APxLevelAndGainList[0].cmb_GainWaveform;

                            if (lst == 0)
                            {
                                string path = testApItem.APxLevelAndGainList[0].WaveFilePathList.Find(x => x.Contains(testApItem.APxLevelAndGainList[0].cmb_GainWaveform));

                                bool val = false;
                                if (testApItem.APxLevelAndGainList[0].isNewWaveform.Keys.Contains(testApItem.APxLevelAndGainList[0].cmb_GainWaveform))
                                {
                                    val = testApItem.APxLevelAndGainList[0].isNewWaveform[testApItem.APxLevelAndGainList[0].cmb_GainWaveform];

                                    testApItem.APxLevelAndGainList[0].isNewWaveform[testApItem.APxLevelAndGainList[0].cmb_GainWaveform] = false;
                                }

                                gainPaths = SaveFilesInServer(copyItem, path, testApItem.APxLevelAndGainList[0].cmb_GainWaveform, val, sourceTestCaseItem.TestCaseID, actionItem.TestActionID, oldTAID, "Audio Precision", "AP Waveform Files");

                                if (testApItem.APxLevelAndGainList[0].cmb_GainWaveform != null && testApItem.APxLevelAndGainList[0].cmb_GainWaveform != string.Empty)
                                {
                                    testApItem.APxLevelAndGainList[0].cmbGainWaveformList.Clear();
                                    testApItem.APxLevelAndGainList[0].cmbGainWaveformList = new ObservableCollection<string>(waveList);
                                    testApItem.APxLevelAndGainList[0].cmbGainWaveformList.Add(gainPaths);

                                    testApItem.APxLevelAndGainList[0].isNewWaveform.Clear();
                                    testApItem.APxLevelAndGainList[0].isNewWaveform.Add(gainPaths, false);
                                }
                            }

                            query = "Insert into LevelAndGain values('" + sourceTestCaseItem.TestCaseID + "','" + actionItem.TestActionID + "','" + testApItem.APxLevelAndGainList[0].ChkGainGenON + "',@WaveType,'" + testApItem.APxLevelAndGainList[0].chkBx_GainLevelTrackCh + "','" + testApItem.APxLevelAndGainList[0].txt_GainCh1Level + "','" + testApItem.APxLevelAndGainList[0].txt_GainDcCh1Offset + "','" + testApItem.APxLevelAndGainList[0].txt_GainCh2Level + "','" + testApItem.APxLevelAndGainList[0].txt_GainDcCh2Offset + "','" + testApItem.APxLevelAndGainList[0].txt_GainCh3Level + "','" + testApItem.APxLevelAndGainList[0].txt_GainDcCh3Offset + "','" + testApItem.APxLevelAndGainList[0].txt_GainCh4Level + "','" + testApItem.APxLevelAndGainList[0].txt_GainDcCh4Offset + "','" + testApItem.APxLevelAndGainList[0].txt_GainCh5Level + "','" + testApItem.APxLevelAndGainList[0].txt_GainDcCh5Offset + "','" + testApItem.APxLevelAndGainList[0].txt_GainCh6Level + "','" + testApItem.APxLevelAndGainList[0].txt_GainDcCh6Offset + "','" + testApItem.APxLevelAndGainList[0].txt_GainCh7Level + "','" + testApItem.APxLevelAndGainList[0].txt_GainDcCh7Offset + "','" + testApItem.APxLevelAndGainList[0].txt_GainCh8Level + "','" + testApItem.APxLevelAndGainList[0].txt_GainDcCh8Offset + "','" + testApItem.APxLevelAndGainList[0].txt_GainfrequencyA + "','" + testApItem.APxLevelAndGainList[0].btn_GainCh1 + "','" + testApItem.APxLevelAndGainList[0].btn_GainCh2 + "','" + testApItem.APxLevelAndGainList[0].btn_GainCh3 + "','" + testApItem.APxLevelAndGainList[0].btn_GainCh4 + "','" + testApItem.APxLevelAndGainList[0].btn_GainCh5 + "','" + testApItem.APxLevelAndGainList[0].btn_GainCh6 + "','" + testApItem.APxLevelAndGainList[0].btn_GainCh7 + "','" + testApItem.APxLevelAndGainList[0].btn_GainCh8 + "','" + testApItem.APxLevelAndGainList[0].TxtGainCh1 + "','" + testApItem.APxLevelAndGainList[0].TxtGainCh2 + "','" + testApItem.APxLevelAndGainList[0].TxtGainCh3 + "','" + testApItem.APxLevelAndGainList[0].TxtGainCh4 + "','" + testApItem.APxLevelAndGainList[0].TxtGainCh5 + "','" + testApItem.APxLevelAndGainList[0].TxtGainCh6 + "','" + testApItem.APxLevelAndGainList[0].TxtGainCh7 + "','" + testApItem.APxLevelAndGainList[0].TxtGainCh8 + "','" + testApItem.APxLevelAndGainList[0].SeqGainSetupCount + "','" + testApItem.APxLevelAndGainList[0].txt_GainfrequencyB + "','" + testApItem.APxLevelAndGainList[0].TxtUpToleranceGainCh1 + "','" + testApItem.APxLevelAndGainList[0].TxtUpToleranceGainCh2 + "','" + testApItem.APxLevelAndGainList[0].TxtUpToleranceGainCh3 + "','" + testApItem.APxLevelAndGainList[0].TxtUpToleranceGainCh4 + "','" + testApItem.APxLevelAndGainList[0].TxtUpToleranceGainCh5 + "','" + testApItem.APxLevelAndGainList[0].TxtUpToleranceGainCh6 + "','" + testApItem.APxLevelAndGainList[0].TxtUpToleranceGainCh7 + "','" + testApItem.APxLevelAndGainList[0].TxtUpToleranceGainCh8 + "','" + testApItem.APxLevelAndGainList[0].TxtLowToleranceGainCh1 + "','" + testApItem.APxLevelAndGainList[0].TxtLowToleranceGainCh2 + "','" + testApItem.APxLevelAndGainList[0].TxtLowToleranceGainCh3 + "','" + testApItem.APxLevelAndGainList[0].TxtLowToleranceGainCh4 + "','" + testApItem.APxLevelAndGainList[0].TxtLowToleranceGainCh5 + "','" + testApItem.APxLevelAndGainList[0].TxtLowToleranceGainCh6 + "','" + testApItem.APxLevelAndGainList[0].TxtLowToleranceGainCh7 + "','" + testApItem.APxLevelAndGainList[0].TxtLowToleranceGainCh8 + "','" + testApItem.APxLevelAndGainList[0].GainInputChCount + "')";
                            command = new SqlCommand(query, connect);
                            command.Parameters.AddWithValue("@WaveType", gainPaths);
                            command.ExecuteScalar();

                            testApItem.APxLevelAndGainList[0].cmb_GainWaveform = gainPaths;
                        }
                        else if (gainVerify == "Frequency sweep")
                        {
                            string freqPath = string.Empty;
                            if (testApItem.APxFreqResponseList[0].txtFreqVerification != null && testApItem.APxFreqResponseList[0].txtFreqVerification != string.Empty)
                            {
                                freqPath = SaveFilesInServer(copyItem, testApItem.APxFreqResponseList[0].txtFreqVerificationpath, testApItem.APxFreqResponseList[0].txtFreqVerification, testApItem.APxFreqResponseList[0].isVerficationFileLoaded, sourceTestCaseItem.TestCaseID, actionItem.TestActionID, oldTAID, "Audio Precision", "Verification Files");
                                testApItem.APxFreqResponseList[0].isVerficationFileLoaded = false;
                                testApItem.APxFreqResponseList[0].txtFreqVerification = freqPath;
                            }

                            query = "Insert into APFrequencyResponse values('" + sourceTestCaseItem.TestCaseID + "','" + actionItem.TestActionID + "','" + testApItem.APxFreqResponseList[0].StartGenON + "','" + testApItem.APxFreqResponseList[0].txtStartFreq + "','" + testApItem.APxFreqResponseList[0].txtStopFreq + "','" + testApItem.APxFreqResponseList[0].txtLevel + "','" + testApItem.APxFreqResponseList[0].IsEnableCh1 + "','" + testApItem.APxFreqResponseList[0].IsEnableCh2 + "','" + testApItem.APxFreqResponseList[0].IsEnableCh3 + "','" + testApItem.APxFreqResponseList[0].IsEnableCh4 + "','" + testApItem.APxFreqResponseList[0].IsEnableCh5 + "','" + testApItem.APxFreqResponseList[0].IsEnableCh6 + "','" + testApItem.APxFreqResponseList[0].IsEnableCh7 + "','" + testApItem.APxFreqResponseList[0].IsEnableCh8 + "','" + testApItem.APxFreqResponseList[0].OutChannelCount + "',@path)";
                            command = new SqlCommand(query, connect);
                            command.Parameters.AddWithValue("@path", freqPath);
                            command.ExecuteScalar();
                        }
                        else if (gainVerify == "Phase")
                        {
                            string phasePath = string.Empty;
                            if (testApItem.APxInterChPhaseList[0].txtPhaseVerification != null && testApItem.APxInterChPhaseList[0].txtPhaseVerification != string.Empty)
                            {
                                phasePath = SaveFilesInServer(copyItem, testApItem.APxInterChPhaseList[0].txtPhaseVerificationPath, testApItem.APxInterChPhaseList[0].txtPhaseVerification, testApItem.APxInterChPhaseList[0].isVerficationFileLoaded, sourceTestCaseItem.TestCaseID, actionItem.TestActionID, oldTAID, "Audio Precision", "Verification Files");
                                testApItem.APxInterChPhaseList[0].isVerficationFileLoaded = false;
                                testApItem.APxInterChPhaseList[0].txtPhaseVerification = phasePath;
                            }

                            query = "Insert into APPhaseSettings values('" + sourceTestCaseItem.TestCaseID + "','" + actionItem.TestActionID + "','" + testApItem.APxInterChPhaseList[0].ChkGenON + "','" + testApItem.APxInterChPhaseList[0].SteppedTrackChannel + "','" + testApItem.APxInterChPhaseList[0].LevelCh1 + "','" + testApItem.APxInterChPhaseList[0].LevelCh2 + "','" + testApItem.APxInterChPhaseList[0].LevelCh3 + "','" + testApItem.APxInterChPhaseList[0].LevelCh4 + "','" + testApItem.APxInterChPhaseList[0].LevelCh5 + "','" + testApItem.APxInterChPhaseList[0].LevelCh6 + "','" + testApItem.APxInterChPhaseList[0].LevelCh7 + "','" + testApItem.APxInterChPhaseList[0].LevelCh8 + "','" + testApItem.APxInterChPhaseList[0].TxtFreqA + "','" + testApItem.APxInterChPhaseList[0].Isch1Enable + "','" + testApItem.APxInterChPhaseList[0].Isch2Enable + "','" + testApItem.APxInterChPhaseList[0].Isch3Enable + "','" + testApItem.APxInterChPhaseList[0].Isch4Enable + "','" + testApItem.APxInterChPhaseList[0].Isch5Enable + "','" + testApItem.APxInterChPhaseList[0].Isch6Enable + "','" + testApItem.APxInterChPhaseList[0].Isch7Enable + "','" + testApItem.APxInterChPhaseList[0].Isch8Enable + "','" + testApItem.APxInterChPhaseList[0].CmbRefChannelSelected + "','" + testApItem.APxInterChPhaseList[0].MeterRangeSelected + "','" + testApItem.APxInterChPhaseList[0].OutChannelCount + "','" + testApItem.APxInterChPhaseList[0].InChannelCount + "',@path)";
                            command = new SqlCommand(query, connect);
                            command.Parameters.AddWithValue("@path", phasePath);
                            command.ExecuteScalar();
                        }
                        else if (gainVerify == "Stepped Frequency Sweep")
                        {
                            string stepPath = string.Empty;

                            if (testApItem.APxSteppedFreqSweepList[0].txtSteppedFreqVerification != null && testApItem.APxSteppedFreqSweepList[0].txtSteppedFreqVerification != string.Empty)
                            {
                                stepPath = SaveFilesInServer(copyItem, testApItem.APxSteppedFreqSweepList[0].txtSteppedFreqVerificationpath, testApItem.APxSteppedFreqSweepList[0].txtSteppedFreqVerification, testApItem.APxSteppedFreqSweepList[0].isVerficationFileLoaded, sourceTestCaseItem.TestCaseID, actionItem.TestActionID, oldTAID, "Audio Precision", "Verification Files");
                                testApItem.APxSteppedFreqSweepList[0].isVerficationFileLoaded = false;
                                testApItem.APxSteppedFreqSweepList[0].txtSteppedFreqVerification = stepPath;
                            }

                            query = "Insert into APSteppedFreqSweepSettings values('" + sourceTestCaseItem.TestCaseID + "','" + actionItem.TestActionID + "','" + testApItem.APxSteppedFreqSweepList[0].ChkGenON + "','" + testApItem.APxSteppedFreqSweepList[0].StartFrequency + "','" + testApItem.APxSteppedFreqSweepList[0].StopFrequency + "','" + testApItem.APxSteppedFreqSweepList[0].SelectedSweep + "','" + testApItem.APxSteppedFreqSweepList[0].Steppedpoints + "','" + testApItem.APxSteppedFreqSweepList[0].SteppedLevel + "','" + testApItem.APxSteppedFreqSweepList[0].IsEnableCh1 + "','" + testApItem.APxSteppedFreqSweepList[0].IsEnableCh2 + "','" + testApItem.APxSteppedFreqSweepList[0].IsEnableCh3 + "','" + testApItem.APxSteppedFreqSweepList[0].IsEnableCh4 + "','" + testApItem.APxSteppedFreqSweepList[0].IsEnableCh5 + "','" + testApItem.APxSteppedFreqSweepList[0].IsEnableCh6 + "','" + testApItem.APxSteppedFreqSweepList[0].IsEnableCh7 + "','" + testApItem.APxSteppedFreqSweepList[0].IsEnableCh8 + "','" + testApItem.APxSteppedFreqSweepList[0].cmbPhaseRefChannel + "','" + testApItem.APxSteppedFreqSweepList[0].OutChannelCount + "','" + testApItem.APxSteppedFreqSweepList[0].InChCount + "',@path)";
                            command = new SqlCommand(query, connect);
                            command.Parameters.AddWithValue("@path", stepPath);
                            command.ExecuteScalar();
                        }
                        else if (gainVerify == "THD+N")
                        {
                            string thdnPath = string.Empty;
                            if (testApItem.APxTHDNList[0].txtTHDNVerification != null && testApItem.APxTHDNList[0].txtTHDNVerification != string.Empty)
                            {
                                thdnPath = SaveFilesInServer(copyItem, testApItem.APxTHDNList[0].txtTHDNVerificationPath, testApItem.APxTHDNList[0].txtTHDNVerification, testApItem.APxTHDNList[0].isVerficationFileLoaded, sourceTestCaseItem.TestCaseID, actionItem.TestActionID, oldTAID, "Audio Precision", "Verification Files");
                                testApItem.APxTHDNList[0].isVerficationFileLoaded = false;
                                testApItem.APxTHDNList[0].txtTHDNVerification = thdnPath;
                            }

                            query = "Insert into APTHDNSettings values('" + sourceTestCaseItem.TestCaseID + "','" + actionItem.TestActionID + "','" + testApItem.APxTHDNList[0].ChkGenON + "','" + testApItem.APxTHDNList[0].chkBx_ThdnLevelTrackCh + "','" + testApItem.APxTHDNList[0].txtCh1Content + "','" + testApItem.APxTHDNList[0].txtCh2Content + "','" + testApItem.APxTHDNList[0].txtCh3Content + "','" + testApItem.APxTHDNList[0].txtCh4Content + "','" + testApItem.APxTHDNList[0].txtCh5Content + "','" + testApItem.APxTHDNList[0].txtCh6Content + "','" + testApItem.APxTHDNList[0].txtCh7Content + "','" + testApItem.APxTHDNList[0].txtCh8Content + "','" + testApItem.APxTHDNList[0].txt_THDfrequency + "','" + testApItem.APxTHDNList[0].btn_THDCh1 + "','" + testApItem.APxTHDNList[0].btn_THDCh2 + "','" + testApItem.APxTHDNList[0].btn_THDCh3 + "','" + testApItem.APxTHDNList[0].btn_THDCh4 + "','" + testApItem.APxTHDNList[0].btn_THDCh5 + "','" + testApItem.APxTHDNList[0].btn_THDCh6 + "','" + testApItem.APxTHDNList[0].btn_THDCh7 + "','" + testApItem.APxTHDNList[0].btn_THDCh8 + "','" + testApItem.APxTHDNList[0].cmb_THDLowPassFilter + "','" + testApItem.APxTHDNList[0].cmb_THDHighPassFilter + "','" + testApItem.APxTHDNList[0].cmb_THDWeighting + "','" + testApItem.APxTHDNList[0].cmb_THDTuningMode + "','" + testApItem.APxTHDNList[0].OutChannelCount + "', '" + testApItem.APxTHDNList[0].TxtLowpass + "', '" + testApItem.APxTHDNList[0].TxtHighpass + "', '" + testApItem.APxTHDNList[0].txt_THDfrequency + "',@path)";
                            command = new SqlCommand(query, connect);
                            command.Parameters.AddWithValue("@path", thdnPath);
                            command.ExecuteScalar();
                        }


                        ///////Save Initial Settings

                        query = "Insert into APSeqModeInitialSettings values('" + sourceTestCaseItem.TestCaseID + "','" + actionItem.TestActionID + "','" + testApItem.APxInitialSettingsList[0].ChkSeqGenON + "','" + testApItem.APxInitialSettingsList[0].cmbSeqWaveForm + "','" + testApItem.APxInitialSettingsList[0].cmbSeqTestChannel + "','" + testApItem.APxInitialSettingsList[0].ChkSeqTrackCh + "','" + testApItem.APxInitialSettingsList[0].TxtSeqLevelCh1 + "','" + testApItem.APxInitialSettingsList[0].TxtSeqLevelDcCh1 + "','" + testApItem.APxInitialSettingsList[0].TxtSeqFreqA + "','" + testApItem.APxInitialSettingsList[0].TxtSeqFreqB + "','" + testApItem.APxInitialSettingsList[0].ChkSeqCh1Enable + "','" + testApItem.APxInitialSettingsList[0].ChkSeqCh2Enable + "','" + testApItem.APxInitialSettingsList[0].ChkSeqCh3Enable + "','" + testApItem.APxInitialSettingsList[0].ChkSeqCh4Enable + "','" + testApItem.APxInitialSettingsList[0].ChkSeqCh5Enable + "','" + testApItem.APxInitialSettingsList[0].ChkSeqCh6Enable + "','" + testApItem.APxInitialSettingsList[0].ChkSeqCh7Enable + "','" + testApItem.APxInitialSettingsList[0].ChkSeqCh8Enable + "','" + testApItem.APxInitialSettingsList[0].TxtSeqDelay + "', '" + testApItem.APxInitialSettingsList[0].TxtSeqLevelCh2 + "','" + testApItem.APxInitialSettingsList[0].TxtSeqLevelCh3 + "','" + testApItem.APxInitialSettingsList[0].TxtSeqLevelCh4 + "','" + testApItem.APxInitialSettingsList[0].TxtSeqLevelCh5 + "','" + testApItem.APxInitialSettingsList[0].TxtSeqLevelCh6 + "','" + testApItem.APxInitialSettingsList[0].TxtSeqLevelCh7 + "','" + testApItem.APxInitialSettingsList[0].TxtSeqLevelCh8 + "','" + testApItem.APxInitialSettingsList[0].TxtSeqLevelDcCh2 + "','" + testApItem.APxInitialSettingsList[0].TxtSeqLevelDcCh3 + "','" + testApItem.APxInitialSettingsList[0].TxtSeqLevelDcCh4 + "','" + testApItem.APxInitialSettingsList[0].TxtSeqLevelDcCh5 + "','" + testApItem.APxInitialSettingsList[0].TxtSeqLevelDcCh6 + "','" + testApItem.APxInitialSettingsList[0].TxtSeqLevelDcCh7 + "','" + testApItem.APxInitialSettingsList[0].TxtSeqLevelDcCh8 + "','" + testApItem.APxInitialSettingsList[0].SeqSetupCount + "')";
                        command = new SqlCommand(query, connect);
                        command.ExecuteScalar();

                        query = "Insert into APBenchModeInitialSettings values('" + sourceTestCaseItem.TestCaseID + "','" + actionItem.TestActionID + "','" + testApItem.APxInitialSettingsList[0].ChkBenchGenON + "','" + testApItem.APxInitialSettingsList[0].cmb_BenchWaveform + "','" + testApItem.APxInitialSettingsList[0].chkBx_BenchLevelTrackCh + "','" + testApItem.APxInitialSettingsList[0].txt_BenchLevelCh1 + "','" + testApItem.APxInitialSettingsList[0].txt_BenchDcOffsetCh1 + "','" + testApItem.APxInitialSettingsList[0].txt_BenchfrequencyA + "','" + testApItem.APxInitialSettingsList[0].ChkBenchCh1Enable + "','" + testApItem.APxInitialSettingsList[0].ChkBenchCh2Enable + "','" + testApItem.APxInitialSettingsList[0].ChkBenchCh3Enable + "','" + testApItem.APxInitialSettingsList[0].ChkBenchCh4Enable + "','" + testApItem.APxInitialSettingsList[0].ChkBenchCh5Enable + "','" + testApItem.APxInitialSettingsList[0].ChkBenchCh6Enable + "','" + testApItem.APxInitialSettingsList[0].ChkBenchCh7Enable + "','" + testApItem.APxInitialSettingsList[0].ChkBenchCh8Enable + "','" + testApItem.APxInitialSettingsList[0].txt_BenchLevelCh2 + "','" + testApItem.APxInitialSettingsList[0].txt_BenchLevelCh3 + "','" + testApItem.APxInitialSettingsList[0].txt_BenchLevelCh4 + "','" + testApItem.APxInitialSettingsList[0].txt_BenchLevelCh5 + "','" + testApItem.APxInitialSettingsList[0].txt_BenchLevelCh6 + "','" + testApItem.APxInitialSettingsList[0].txt_BenchLevelCh7 + "','" + testApItem.APxInitialSettingsList[0].txt_BenchLevelCh8 + "','" + testApItem.APxInitialSettingsList[0].txt_BenchDcOffsetCh2 + "','" + testApItem.APxInitialSettingsList[0].txt_BenchDcOffsetCh3 + "','" + testApItem.APxInitialSettingsList[0].txt_BenchDcOffsetCh4 + "','" + testApItem.APxInitialSettingsList[0].txt_BenchDcOffsetCh5 + "','" + testApItem.APxInitialSettingsList[0].txt_BenchDcOffsetCh6 + "','" + testApItem.APxInitialSettingsList[0].txt_BenchDcOffsetCh7 + "','" + testApItem.APxInitialSettingsList[0].txt_BenchDcOffsetCh8 + "','" + testApItem.APxInitialSettingsList[0].BenchSetupCount + "','" + testApItem.APxInitialSettingsList[0].txt_BenchfrequencyB + "')";
                        command = new SqlCommand(query, connect);
                        command.ExecuteScalar();

                        query = "Insert into APLevelAndGainInitialSettings values('" + sourceTestCaseItem.TestCaseID + "','" + actionItem.TestActionID + "','" + testApItem.APxInitialLevelAndGainList[0].ChkGainGenON + "','" + testApItem.APxInitialLevelAndGainList[0].cmb_GainWaveform + "','" + testApItem.APxInitialLevelAndGainList[0].chkBx_GainLevelTrackCh + "','" + testApItem.APxInitialLevelAndGainList[0].txt_GainCh1Level + "','" + testApItem.APxInitialLevelAndGainList[0].txt_GainDcCh1Offset + "','" + testApItem.APxInitialLevelAndGainList[0].txt_GainfrequencyA + "','" + testApItem.APxInitialLevelAndGainList[0].btn_GainCh1 + "','" + testApItem.APxInitialLevelAndGainList[0].btn_GainCh2 + "','" + testApItem.APxInitialLevelAndGainList[0].btn_GainCh3 + "','" + testApItem.APxInitialLevelAndGainList[0].btn_GainCh4 + "','" + testApItem.APxInitialLevelAndGainList[0].btn_GainCh5 + "','" + testApItem.APxInitialLevelAndGainList[0].btn_GainCh6 + "','" + testApItem.APxInitialLevelAndGainList[0].btn_GainCh7 + "','" + testApItem.APxInitialLevelAndGainList[0].btn_GainCh8 + "','" + testApItem.APxInitialLevelAndGainList[0].txt_GainCh2Level + "','" + testApItem.APxInitialLevelAndGainList[0].txt_GainCh3Level + "','" + testApItem.APxInitialLevelAndGainList[0].txt_GainCh4Level + "','" + testApItem.APxInitialLevelAndGainList[0].txt_GainCh5Level + "','" + testApItem.APxInitialLevelAndGainList[0].txt_GainCh6Level + "','" + testApItem.APxInitialLevelAndGainList[0].txt_GainCh7Level + "','" + testApItem.APxInitialLevelAndGainList[0].txt_GainCh8Level + "','" + testApItem.APxInitialLevelAndGainList[0].txt_GainDcCh2Offset + "','" + testApItem.APxInitialLevelAndGainList[0].txt_GainDcCh3Offset + "','" + testApItem.APxInitialLevelAndGainList[0].txt_GainDcCh4Offset + "','" + testApItem.APxInitialLevelAndGainList[0].txt_GainDcCh5Offset + "','" + testApItem.APxInitialLevelAndGainList[0].txt_GainDcCh6Offset + "','" + testApItem.APxInitialLevelAndGainList[0].txt_GainDcCh7Offset + "','" + testApItem.APxInitialLevelAndGainList[0].txt_GainDcCh8Offset + "', '" + testApItem.APxInitialLevelAndGainList[0].SeqGainSetupCount + "','" + testApItem.APxInitialLevelAndGainList[0].txt_GainfrequencyB + "','" + testApItem.APxInitialLevelAndGainList[0].GainInputChCount + "')";
                        command = new SqlCommand(query, connect);
                        command.ExecuteScalar();

                        query = "Insert into APFrequencyResponseInitialSettings values('" + sourceTestCaseItem.TestCaseID + "','" + actionItem.TestActionID + "','" + testApItem.APxInitialFreqResponseList[0].StartGenON + "','" + testApItem.APxInitialFreqResponseList[0].txtStartFreq + "','" + testApItem.APxInitialFreqResponseList[0].txtStopFreq + "','" + testApItem.APxInitialFreqResponseList[0].txtLevel + "','" + testApItem.APxInitialFreqResponseList[0].IsEnableCh1 + "','" + testApItem.APxInitialFreqResponseList[0].IsEnableCh2 + "','" + testApItem.APxInitialFreqResponseList[0].IsEnableCh3 + "','" + testApItem.APxInitialFreqResponseList[0].IsEnableCh4 + "','" + testApItem.APxInitialFreqResponseList[0].IsEnableCh5 + "','" + testApItem.APxInitialFreqResponseList[0].IsEnableCh6 + "','" + testApItem.APxInitialFreqResponseList[0].IsEnableCh7 + "','" + testApItem.APxInitialFreqResponseList[0].IsEnableCh8 + "','" + testApItem.APxInitialFreqResponseList[0].OutChannelCount + "')";
                        command = new SqlCommand(query, connect);
                        command.ExecuteScalar();

                        query = "Insert into APPhaseInitialSettings values('" + sourceTestCaseItem.TestCaseID + "','" + actionItem.TestActionID + "','" + testApItem.APxInitialInterChPhaseList[0].ChkGenON + "','" + testApItem.APxInitialInterChPhaseList[0].SteppedTrackChannel + "','" + testApItem.APxInitialInterChPhaseList[0].LevelCh1 + "','" + testApItem.APxInitialInterChPhaseList[0].LevelCh2 + "','" + testApItem.APxInitialInterChPhaseList[0].LevelCh3 + "','" + testApItem.APxInitialInterChPhaseList[0].LevelCh4 + "','" + testApItem.APxInitialInterChPhaseList[0].LevelCh5 + "','" + testApItem.APxInitialInterChPhaseList[0].LevelCh6 + "','" + testApItem.APxInitialInterChPhaseList[0].LevelCh7 + "','" + testApItem.APxInitialInterChPhaseList[0].LevelCh8 + "','" + testApItem.APxInitialInterChPhaseList[0].TxtFreqA + "','" + testApItem.APxInitialInterChPhaseList[0].Isch1Enable + "','" + testApItem.APxInitialInterChPhaseList[0].Isch2Enable + "','" + testApItem.APxInitialInterChPhaseList[0].Isch3Enable + "','" + testApItem.APxInitialInterChPhaseList[0].Isch4Enable + "','" + testApItem.APxInitialInterChPhaseList[0].Isch5Enable + "','" + testApItem.APxInitialInterChPhaseList[0].Isch6Enable + "','" + testApItem.APxInitialInterChPhaseList[0].Isch7Enable + "','" + testApItem.APxInitialInterChPhaseList[0].Isch8Enable + "','" + testApItem.APxInitialInterChPhaseList[0].CmbRefChannelSelected + "','" + testApItem.APxInitialInterChPhaseList[0].MeterRangeSelected + "','" + testApItem.APxInitialInterChPhaseList[0].OutChannelCount + "','" + testApItem.APxInitialInterChPhaseList[0].InChannelCount + "')";
                        command = new SqlCommand(query, connect);
                        command.ExecuteScalar();

                        query = "Insert into APSteppedFreqSweepInitialSettings values('" + sourceTestCaseItem.TestCaseID + "','" + actionItem.TestActionID + "','" + testApItem.APxInitialSteppedFreqSweepList[0].ChkGenON + "','" + testApItem.APxInitialSteppedFreqSweepList[0].StartFrequency + "','" + testApItem.APxInitialSteppedFreqSweepList[0].StopFrequency + "','" + testApItem.APxInitialSteppedFreqSweepList[0].SelectedSweep + "','" + testApItem.APxInitialSteppedFreqSweepList[0].Steppedpoints + "','" + testApItem.APxInitialSteppedFreqSweepList[0].SteppedLevel + "','" + testApItem.APxInitialSteppedFreqSweepList[0].IsEnableCh1 + "','" + testApItem.APxInitialSteppedFreqSweepList[0].IsEnableCh2 + "','" + testApItem.APxInitialSteppedFreqSweepList[0].IsEnableCh3 + "','" + testApItem.APxInitialSteppedFreqSweepList[0].IsEnableCh4 + "','" + testApItem.APxInitialSteppedFreqSweepList[0].IsEnableCh5 + "','" + testApItem.APxInitialSteppedFreqSweepList[0].IsEnableCh6 + "','" + testApItem.APxInitialSteppedFreqSweepList[0].IsEnableCh7 + "','" + testApItem.APxInitialSteppedFreqSweepList[0].IsEnableCh8 + "','" + testApItem.APxInitialSteppedFreqSweepList[0].cmbPhaseRefChannel + "','" + testApItem.APxInitialSteppedFreqSweepList[0].OutChannelCount + "','" + testApItem.APxInitialSteppedFreqSweepList[0].InChCount + "')";
                        command = new SqlCommand(query, connect);
                        command.ExecuteScalar();

                        query = "Insert into APTHDNInitialSettings values('" + sourceTestCaseItem.TestCaseID + "','" + actionItem.TestActionID + "','" + testApItem.APxTHDNList[0].ChkGenON + "','" + testApItem.APxTHDNList[0].chkBx_ThdnLevelTrackCh + "','" + testApItem.APxTHDNList[0].txtCh1Content + "','" + testApItem.APxTHDNList[0].txtCh2Content + "','" + testApItem.APxTHDNList[0].txtCh3Content + "','" + testApItem.APxTHDNList[0].txtCh4Content + "','" + testApItem.APxTHDNList[0].txtCh5Content + "','" + testApItem.APxTHDNList[0].txtCh6Content + "','" + testApItem.APxTHDNList[0].txtCh7Content + "','" + testApItem.APxTHDNList[0].txtCh8Content + "','" + testApItem.APxTHDNList[0].txt_THDfrequency + "','" + testApItem.APxTHDNList[0].btn_THDCh1 + "','" + testApItem.APxTHDNList[0].btn_THDCh2 + "','" + testApItem.APxTHDNList[0].btn_THDCh3 + "','" + testApItem.APxTHDNList[0].btn_THDCh4 + "','" + testApItem.APxTHDNList[0].btn_THDCh5 + "','" + testApItem.APxTHDNList[0].btn_THDCh6 + "','" + testApItem.APxTHDNList[0].btn_THDCh7 + "','" + testApItem.APxTHDNList[0].btn_THDCh8 + "','" + testApItem.APxTHDNList[0].cmb_THDLowPassFilter + "','" + testApItem.APxTHDNList[0].cmb_THDHighPassFilter + "','" + testApItem.APxTHDNList[0].cmb_THDWeighting + "','" + testApItem.APxTHDNList[0].cmb_THDTuningMode + "','" + testApItem.APxTHDNList[0].OutChannelCount + "', '" + testApItem.APxTHDNList[0].TxtLowpass + "', '" + testApItem.APxTHDNList[0].TxtHighpass + "', '" + testApItem.APxTHDNList[0].txt_THDfrequency + "')";
                        command = new SqlCommand(query, connect);
                        command.ExecuteScalar();
                    }

                    foreach (string file in filestoDelete)
                    {
                        if (file != null && file != string.Empty)
                        {
                            string path = Path.Combine(QatConstants.QATServerPath, file);

                            if (File.Exists(path))
                            {
                                File.SetAttributes(path, FileAttributes.Normal);
                                File.Delete(path);
                            }
                        }
                    }

                }
                
                /////// QRCM file change to read access 
                string qrcmFilePath = Path.Combine(QatConstants.QATServerPath, "QRCM_Files", sourceTestCaseItem.TestCaseID + ".txt");
                if (File.Exists(qrcmFilePath))
                {
                    FileInfo fileInformation = new FileInfo(qrcmFilePath);
                    fileInformation.IsReadOnly = true;
                }
                
                CloseConnection();

                return true;
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("There is not enough space on the disk"))
                {
                    MessageBox.Show("Not enough space on the server disk. Reference / Project files are not saved.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }

                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                //MessageBox.Show("Exception\n \n" + ex.Message, "QAT Error Code - EC02023", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }



     

        private string ReadSingleValueFromDB(string query)
        {
            string returnVal = string.Empty;

            DataTable dataTable = new DataTable();
            SqlDataAdapter dataAdapter = new SqlDataAdapter(query, connect);
            dataAdapter.Fill(dataTable);
            DataTableReader dataTableReader = dataTable.CreateDataReader();

            if (dataTableReader.HasRows)
            {
                while (dataTableReader.Read())
                    returnVal = dataTableReader.GetValue(0).ToString();
            }

            return returnVal;
        }

        private List<string> ReadValuesFromDB(string query)
        {
            List<string> returnVal = new List<string>();

            DataTable dataTable = new DataTable();
            SqlDataAdapter dataAdapter = new SqlDataAdapter(query, connect);
            dataAdapter.Fill(dataTable);
            DataTableReader dataTableReader = dataTable.CreateDataReader();

            if (dataTableReader.HasRows)
            {
                while (dataTableReader.Read())
                {
                    if(dataTableReader.GetValue(0) != null && dataTableReader.GetValue(0).ToString() != string.Empty)
                        returnVal.Add(dataTableReader.GetValue(0).ToString());
                }
            }

            return returnVal;
        }

        private void DeleteFileFromServer(string query, string parameter1, string parameter2)
        {
            DataTable dataTable = new DataTable();
            DataTableReader dataTableReader = null;
            SqlDataAdapter dataAdapter = new SqlDataAdapter(query, connect);
            dataAdapter.Fill(dataTable);
            dataTableReader = dataTable.CreateDataReader();

            if (dataTableReader.HasRows)
            {
                while (dataTableReader.Read())
                {
                    string appath = dataTableReader.GetValue(0).ToString();

                    if (appath != null && appath != string.Empty)
                    {
                        string filepath = Path.Combine(QatConstants.QATServerPath, parameter1, parameter2, appath);

                        if (File.Exists(filepath))
                        {
                            File.SetAttributes(filepath, FileAttributes.Normal);
                            File.Delete(filepath);
                        }
                    }
                }
            }
        }
            

        private string SaveFilesInServer(TreeViewExplorer copyItem, string apxBrowseLocation, string apxLocationTimeStamp, bool isApxFileLoaded, int testCaseID, int newactionID, int oldTAID, string parameter1, string parameter2)
        {
            string apPath = string.Empty;
            string extension = Path.GetExtension(apxLocationTimeStamp);
            string name = apPath = apxLocationTimeStamp;

            if (copyItem == null)
            {
                int len = 0;
                if (name.EndsWith("_" + testCaseID + "_" + oldTAID + extension))
                {
                    len = ("_" + testCaseID + "_" + oldTAID + extension).Length;
                    apPath = apxLocationTimeStamp = name.Remove(name.Length - len, len) + extension;
                }

                if (apPath.EndsWith(extension))
                {
                    string names = Path.GetFileNameWithoutExtension(apxLocationTimeStamp);
                    apPath = names + "_" + testCaseID + "_" + newactionID + extension;
                }

                if (isApxFileLoaded == true)
                {
                    if (apxBrowseLocation != null && apxBrowseLocation != string.Empty && apxLocationTimeStamp != null & apxLocationTimeStamp != string.Empty)
                    {
                        StoreAPTimeStampFile(("\\" + parameter1 + "\\" + parameter2 + "\\"), apxBrowseLocation, apPath);
                    }
                }
                else
                {
                    string location = string.Empty;
                    if (len != 0)
                    {
                        string ss = Path.GetFileNameWithoutExtension(apxLocationTimeStamp) + "_" + testCaseID + "_" + oldTAID + extension;
                        location = Path.Combine(QatConstants.QATServerPath, parameter1, parameter2, ss);
                    }
                    else
                    {
                        location = Path.Combine(QatConstants.QATServerPath, parameter1, parameter2, apxLocationTimeStamp);
                    }

                    RenameServerFiles(("\\" + parameter1 + "\\" + parameter2 + "\\"), location, apPath);
                }
            }
            else
            {
                int len = 0;
                if (name.EndsWith("_" + copyItem.ItemKey + "_" + oldTAID + extension))
                {
                    len = ("_" + copyItem.ItemKey + "_" + oldTAID + extension).Length;
                    apPath = apxLocationTimeStamp = name.Remove(name.Length - len, len) + extension;
                }

                if (apPath.EndsWith(extension))
                {
                    string names = Path.GetFileNameWithoutExtension(apxLocationTimeStamp);
                    apPath = names + "_" + testCaseID + "_" + newactionID + extension;
                }

                string location = string.Empty;
                if (len != 0)
                {
                    string ss = Path.GetFileNameWithoutExtension(apxLocationTimeStamp) + "_" + copyItem.ItemKey + "_" + oldTAID + extension;
                    location = Path.Combine(QatConstants.QATServerPath, parameter1, parameter2, ss);
                }
                else
                {
                    location = Path.Combine(QatConstants.QATServerPath, parameter1, parameter2, apxLocationTimeStamp);
                }

                StoreAPTimeStampFile(("\\" + parameter1 + "\\" + parameter2+ "\\"), location, apPath);

            }

            return apPath;
        }

        private string SaveFilesInServerResponse(TreeViewExplorer copyItem, string apxBrowseLocation, string apxLocationTimeStamp, bool isApxFileLoaded, int testCaseID, int newactionID, int oldTAID, string parameter1, string parameter2, List<string> responseID, int newRespID, bool isCopiedItem)
        {
            string apPath = string.Empty;
            string extension = Path.GetExtension(apxLocationTimeStamp);
            string name = apPath = apxLocationTimeStamp;

            if (copyItem == null)
            {
                int len = 0;
                string oldRespID = string.Empty;
                if (responseID.Count > 0)
                {
                    foreach (string respID in responseID)
                    {
                        if (name.EndsWith("_" + testCaseID + "_" + oldTAID + "_" + respID + extension))
                        {
                            oldRespID = respID;

                            len = ("_" + testCaseID + "_" + oldTAID + "_" + respID + extension).Length;
                            apPath = apxLocationTimeStamp = name.Remove(name.Length - len, len) + extension;

                            break;
                        }
                    }
                }

                if (apPath.EndsWith(extension))
                {
                    string names = Path.GetFileNameWithoutExtension(apxLocationTimeStamp);
                    apPath = names + "_" + testCaseID + "_" + newactionID + "_" + newRespID + extension;
                }

                if (isApxFileLoaded == true || isCopiedItem == true)
                {
                    if (apxBrowseLocation != null && apxBrowseLocation != string.Empty && apxLocationTimeStamp != null & apxLocationTimeStamp != string.Empty)
                    {
                        StoreAPTimeStampFile(("\\" + parameter1 + "\\" + parameter2 + "\\"), apxBrowseLocation, apPath);
                    }
                }
                else
                {
                    string location = string.Empty;
                    if (len != 0)
                    {
                        string ss = Path.GetFileNameWithoutExtension(apxLocationTimeStamp) + "_" + testCaseID + "_" + oldTAID + "_" + oldRespID + extension;
                        location = Path.Combine(QatConstants.QATServerPath, parameter1, parameter2, ss);
                    }
                    else
                    {
                        location = Path.Combine(QatConstants.QATServerPath, parameter1, parameter2, apxLocationTimeStamp);
                    }

                    RenameServerFiles(("\\" + parameter1 + "\\" + parameter2 + "\\"), location, apPath);
                }
            }
            else
            {
                int len = 0;
                string oldRespID = string.Empty;

                if (responseID.Count > 0)
                {
                    foreach (string respID in responseID)
                    {
                        if (name.EndsWith("_" + copyItem.ItemKey + "_" + oldTAID + "_" + respID + extension))
                        {
                            oldRespID = respID;

                            len = ("_" + copyItem.ItemKey + "_" + oldTAID + "_" + respID + extension).Length;
                            apPath = apxLocationTimeStamp = name.Remove(name.Length - len, len) + extension;

                            break;
                        }
                    }
                }

                if (apPath.EndsWith(extension))
                {
                    string names = Path.GetFileNameWithoutExtension(apxLocationTimeStamp);
                    apPath = names + "_" + testCaseID + "_" + newactionID + "_" + newRespID + extension;
                }

                string location = string.Empty;
                if (len != 0)
                {
                    string ss = Path.GetFileNameWithoutExtension(apxLocationTimeStamp) + "_" + copyItem.ItemKey + "_" + oldTAID + "_" + oldRespID + extension;
                    location = Path.Combine(QatConstants.QATServerPath, parameter1, parameter2, ss);
                }
                else
                {
                    location = Path.Combine(QatConstants.QATServerPath, parameter1, parameter2, apxLocationTimeStamp);
                }

                StoreAPTimeStampFile(("\\" + parameter1 + "\\" + parameter2 + "\\"), location, apPath);
            }

            return apPath;
        }

        public void StoreAPFile(string FilePath, string apFile)
        {
            try
            {
                FileInfo path = new FileInfo(apFile);
                string ap_path = path.FullName;
                string apfileName = path.Name;

            //if (Properties.Settings.Default.Path.ToString() != string.Empty)
            //{
                string PreferencesServerPath = QatConstants.QATServerPath + FilePath;
                if (!Directory.Exists(PreferencesServerPath))
                {
                    Directory.CreateDirectory(PreferencesServerPath);
                }

                    if (!File.Exists(PreferencesServerPath + apfileName))
                    {
                        File.Copy(ap_path + "", PreferencesServerPath + apfileName + "");
                        FileInfo fileInformation = new FileInfo(PreferencesServerPath + apfileName + "");
                        fileInformation.IsReadOnly = true;
                    }
                //}
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                //MessageBox.Show("Exception\n \n" + ex.Message, "QAT Error Code - EC02035", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void StoreAPTimeStampFile(string FilePath, string apFile, string timeStampFile)
        {
            try
            {
                if ((timeStampFile != null & timeStampFile != string.Empty))
                {
                    string PreferencesServerPath = QatConstants.QATServerPath + FilePath;
                    if (!Directory.Exists(PreferencesServerPath))
                    {
                        Directory.CreateDirectory(PreferencesServerPath);
                    }

                    if (File.Exists(PreferencesServerPath + timeStampFile))
                    {
                        FileInfo fileInfo = new FileInfo(PreferencesServerPath + timeStampFile);
                        fileInfo.IsReadOnly = false;
                    }

                    File.Copy(apFile, PreferencesServerPath + timeStampFile, true);
                    FileInfo fileInformation = new FileInfo(PreferencesServerPath + timeStampFile + "");
                    fileInformation.IsReadOnly = true;
                }
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                //MessageBox.Show("Exception\n \n" + ex.Message, "QAT Error Code - EC02036", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void RenameServerFiles(string FilePath, string apFile, string timeStampFile)
        {
            try
            {
                if ((timeStampFile != null & timeStampFile != string.Empty))
                {
                    string PreferencesServerPath = QatConstants.QATServerPath + FilePath;
                    if (!Directory.Exists(PreferencesServerPath))
                    {
                        Directory.CreateDirectory(PreferencesServerPath);
                    }

                    if (File.Exists(apFile))
                    {
                        //FileInfo fileInfo = new FileInfo(apFile);
                        //fileInfo.IsReadOnly = false;

                        File.Move(apFile, PreferencesServerPath + timeStampFile);

                        //FileInfo fileInformation = new FileInfo(PreferencesServerPath + timeStampFile );
                        //fileInformation.IsReadOnly = true;
                    }
                }
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                //MessageBox.Show("Exception\n \n" + ex.Message, "QAT Error Code - EC02036", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

//        public string GetActualProperty(string type, string name, string prettyname, int currentDesignID)
//        {
//            string actualProperty = string.Empty;
//            try
//            {
//                //string controlVal = string.Empty;
//                var controlPretty = SplitPrettyNameControlID(prettyname);

//                if(controlPretty.Item1 != null && controlPretty.Item1 != string.Empty)
//                    prettyname = controlPretty.Item1;

//                actualProperty = controlPretty.Item2;

//                if (actualProperty == null || actualProperty == string.Empty)
//                {
//                    string query = "select Control from TCInitialization where ComponentType=@compType and ComponentName=@compName and PrettyName=@pretyName and designid=('" + currentDesignID + "')";

//                    DataTable dataTable = new DataTable();
//                    DataTableReader dataTableReader = null;
//                    if (string.IsNullOrEmpty(type))
//                    {
//                        type = string.Empty;
//                    }
//                    if (string.IsNullOrEmpty(name))
//                    {
//                        name = string.Empty;
//                    }
//                    if (string.IsNullOrEmpty(prettyname))
//                    {
//                        prettyname = string.Empty;
//                    }

//                    //string selectedPrettyName = prettyname;


//                    SqlCommand cmd = new SqlCommand(query, connect);
//                    cmd.Parameters.AddWithValue("@compType", type);
//                    cmd.Parameters.AddWithValue("@compName", name);
//                    cmd.Parameters.AddWithValue("@pretyName", prettyname);
//                    SqlDataAdapter dataAdapter = new SqlDataAdapter(cmd);
//                    dataAdapter.Fill(dataTable);
//                    //SqlDataAdapter dataAdapter = new SqlDataAdapter(query, connect);
//                    //dataAdapter.Fill(dataTable);
//                    dataTableReader = dataTable.CreateDataReader();

//                    if (dataTableReader.HasRows)
//                    {
//                        while (dataTableReader.Read())
//                        {
//                            actualProperty = dataTableReader.GetString(0).ToString();
//                        }
//                    }
//                }

//                return actualProperty;
//            }
//            catch (Exception ex)
//            {
//                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
//#if DEBUG
//                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
//#endif
//                //MessageBox.Show("Exception\n \n" + ex.Message, "QAT Error Code - EC02024", MessageBoxButton.OK, MessageBoxImage.Error);
//                CloseConnection();
//                return actualProperty;
//            }
//        }

        public bool ReadTestCaseFromDB(TestCaseItem sourceTestCaseItem)
        {
            CreateConnection();
            OpenConnection();

            try
            {
                bool errorDisplyedQRCM = false;
                string query = "select * from TestAction where TCID ='" + sourceTestCaseItem.TestCaseID + "' order by TestActionID";
                DataTable dataTable = new DataTable();
                DataTableReader dataTableReader = null;
                SqlDataAdapter dataAdapter = new SqlDataAdapter(query, connect);
                dataAdapter.Fill(dataTable);
                dataTableReader = dataTable.CreateDataReader();

                if (dataTableReader.HasRows)
                {
                    while (dataTableReader.Read())
                    {
                        TestActionItem newTestActionItem = sourceTestCaseItem.AddTestAction();

                        if (newTestActionItem == null)
                            continue;

                        newTestActionItem.TestActionID = dataTableReader.GetInt32(0);
                        newTestActionItem.TestActionItemName = dataTableReader.GetString(2).ToString();

                        string selectedAction = dataTableReader.GetString(3).ToString();
                        if(String.Equals(selectedAction, "Usb Action", StringComparison.CurrentCultureIgnoreCase))
                        {
                            newTestActionItem.ActionSelected = "USB Action";
                        }
                        else if (String.Equals(selectedAction, "Telnet Action", StringComparison.CurrentCultureIgnoreCase))
                        {
                            newTestActionItem.ActionSelected = "Ssh/Telnet Action";
                        }
                        else
                        {
                            newTestActionItem.ActionSelected = selectedAction;
                        }

                        //newTestActionItem.ActionSelected = dataTableReader.GetString(3).ToString();
                        newTestActionItem.ActionDelaySetting = dataTableReader.GetString(4).ToString();
                        newTestActionItem.ActionDelayUnitSelected = dataTableReader.GetString(5).ToString();
                        string verificationSelected = dataTableReader.GetString(6).ToString();


                        if (dataTableReader[21] != System.DBNull.Value)
                            newTestActionItem.Verificationdelay = dataTableReader.GetString(21).ToString();

                        if (dataTableReader[22] != System.DBNull.Value)
                            newTestActionItem.VerificationdelayType = dataTableReader.GetString(22).ToString();

                        if (dataTableReader[23] != System.DBNull.Value)
                            newTestActionItem.Rerundelay = dataTableReader.GetString(23).ToString();

                        if (dataTableReader[24] != System.DBNull.Value)
                            newTestActionItem.RerundelayType = dataTableReader.GetString(24).ToString();

                        if (String.Equals(verificationSelected, "Telnet Verification", StringComparison.CurrentCultureIgnoreCase))
                        {
                            newTestActionItem.VerificationSelected = "Ssh/Telnet Verification";
                        }
                        else
                        {
                            if (sourceTestCaseItem.TestVerificationList.Contains(verificationSelected, StringComparer.CurrentCultureIgnoreCase))
                                newTestActionItem.VerificationSelected = verificationSelected;
                            else
                                newTestActionItem.VerificationSelected = null;
                        }

                        newTestActionItem.ActionErrorHandlingReRunCount = dataTableReader.GetString(7).ToString();
                        newTestActionItem.ActionErrorHandlingTypeSelected = dataTableReader.GetString(8).ToString();


                        newTestActionItem.RemoveAllSetAndVerifyTestControlItems();

                        TestSaveLogItem newTestSaveLogItem = newTestActionItem.AddTestSaveLogItem();

                        if (newTestActionItem.VerificationSelected == "Script Verification")
                        {
                            newTestSaveLogItem.ScreenShotIsEnable = false;
                            if (newTestSaveLogItem.ActionSaveLogEventList.Contains("Save during Error"))
                                newTestSaveLogItem.ActionSaveLogEventList.Remove("Save during Error");
                            if (newTestSaveLogItem.ActionSaveLogEventList.Contains("Save logs always"))
                                newTestSaveLogItem.ActionSaveLogEventList.Remove("Save logs always");
                        }
                       
                                                
                        string actionSaveLogEventSelected = dataTableReader.GetString(9).ToString();
                        if (String.Equals(actionSaveLogEventSelected, "Save during Error", StringComparison.CurrentCultureIgnoreCase))
                        {
                            newTestSaveLogItem.ActionSaveLogEventSelected = "Save during Error";
                        }
                        else if (String.Equals(actionSaveLogEventSelected, "Save logs always", StringComparison.CurrentCultureIgnoreCase))
                        {
                            newTestSaveLogItem.ActionSaveLogEventSelected = "Save logs always";
                        }
                        else if (String.Equals(actionSaveLogEventSelected, "Never Save logs", StringComparison.CurrentCultureIgnoreCase))
                        {
                            newTestSaveLogItem.ActionSaveLogEventSelected = "Never Save logs";
                        }
                        else if (String.Equals(actionSaveLogEventSelected, "Save during Error WithQsysLogPeripherals", StringComparison.CurrentCultureIgnoreCase))
                        {
                            newTestSaveLogItem.ActionSaveLogEventSelected = "Save during Error";
                            newTestSaveLogItem.saveQsysylogPeripheralSelection = true;
                        }
                        else if (String.Equals(actionSaveLogEventSelected, "Save logs always WithQsysLogPeripherals", StringComparison.CurrentCultureIgnoreCase))
                        {
                            newTestSaveLogItem.ActionSaveLogEventSelected = "Save logs always";
                            newTestSaveLogItem.saveQsysylogPeripheralSelection = true;
                        }
                        else
                        {
                            newTestSaveLogItem.ActionSaveLogEventSelected = null;
                        }

                        var temp = dataTableReader.GetString(10).ToString();
                        if (String.Equals(dataTableReader.GetString(10).ToString(), "True", StringComparison.CurrentCultureIgnoreCase))
                            newTestSaveLogItem.ActionLogiLogIsChecked = true;
                        else
                            newTestSaveLogItem.ActionLogiLogIsChecked = false;

                        if (newTestSaveLogItem.ActionLogiLogIsChecked)
                        {
                            string iLogDeviceName = dataTableReader.GetString(11).ToString();
                            List<string> iLogDeviceNameList = iLogDeviceName.Split(',').ToList();

                            foreach (string deviceName in iLogDeviceNameList)
                            {
                                foreach (DUT_DeviceItem deviceItem in newTestSaveLogItem.iLogDeviceItem)
                                {
                                    if (String.Equals(deviceItem.ItemDeviceName, deviceName, StringComparison.CurrentCultureIgnoreCase))
                                        deviceItem.iLogIsChecked = true;
                                }
                            }
                        }

                        if (String.Equals(dataTableReader.GetString(12).ToString(), "True", StringComparison.CurrentCultureIgnoreCase))
                            newTestSaveLogItem.ActionLogConfiguratorIsChecked = true;
                        else
                            newTestSaveLogItem.ActionLogConfiguratorIsChecked = false;

                        if (newTestSaveLogItem.ActionLogConfiguratorIsChecked)
                        {
                            string configuratorLogDeviceName = dataTableReader.GetString(13).ToString();
                            List<string> configuratorLogDeviceNameList = configuratorLogDeviceName.Split(',').ToList();

                            foreach (string deviceName in configuratorLogDeviceNameList)
                            {
                                foreach (DUT_DeviceItem deviceItem in newTestSaveLogItem.ConfiguratorLogDeviceItem)
                                {
                                    if (String.Equals(deviceItem.ItemDeviceName, deviceName, StringComparison.CurrentCultureIgnoreCase))
                                        deviceItem.ConfiguratorLogIsChecked = true;
                                }
                            }
                        }

                        if (String.Equals(dataTableReader.GetString(14).ToString(), "True", StringComparison.CurrentCultureIgnoreCase))
                            newTestSaveLogItem.ActionLogEvenetLogIsChecked = true;
                        else
                            newTestSaveLogItem.ActionLogEvenetLogIsChecked = false;

                        if (String.Equals(dataTableReader.GetString(15).ToString(), "True", StringComparison.CurrentCultureIgnoreCase))
                            newTestSaveLogItem.ActionLogSipLogIsChecked = true;
                        else
                            newTestSaveLogItem.ActionLogSipLogIsChecked = false;

                        if (String.Equals(dataTableReader.GetString(16).ToString(), "True", StringComparison.CurrentCultureIgnoreCase))
                            newTestSaveLogItem.ActionLogQsysAppLogIsChecked = true;
                        else
                            newTestSaveLogItem.ActionLogQsysAppLogIsChecked = false;

                        if (String.Equals(dataTableReader.GetString(17).ToString(), "True", StringComparison.CurrentCultureIgnoreCase))
                            newTestSaveLogItem.ActionLogSoftPhoneLogIsChecked = true;
                        else
                            newTestSaveLogItem.ActionLogSoftPhoneLogIsChecked = false;

                        if (String.Equals(dataTableReader.GetString(18).ToString(), "True", StringComparison.CurrentCultureIgnoreCase))
                            newTestSaveLogItem.ActionLogUciViewerLogIsChecked = true;
                        else
                            newTestSaveLogItem.ActionLogUciViewerLogIsChecked = false;

                        if (String.Equals(dataTableReader.GetString(19).ToString(), "True", StringComparison.CurrentCultureIgnoreCase))
                            newTestSaveLogItem.ActionLogKernelLogIsChecked = true;
                        else
                            newTestSaveLogItem.ActionLogKernelLogIsChecked = false;

                        if (String.Equals(dataTableReader.GetString(20).ToString(), "True", StringComparison.CurrentCultureIgnoreCase))
                            newTestSaveLogItem.ActionLogWindowsEventLogIsChecked = true;
                        else
                            newTestSaveLogItem.ActionLogWindowsEventLogIsChecked = false;

                        if (dataTableReader[25] != System.DBNull.Value )
                        {
                            if (String.Equals(dataTableReader.GetString(25).ToString(), "True", StringComparison.CurrentCultureIgnoreCase))
                                newTestSaveLogItem.screenshotselection = true;
                            else
                                newTestSaveLogItem.screenshotselection = false;
                        }


                    }
                }

                int QRCMItemCount = sourceTestCaseItem.TestActionItemList.Where(x => (x.ActionSelected == "QRCM Action") || (x.VerificationSelected == "QRCM Verification")).Count();
                Dictionary<string, string> values = new Dictionary<string, string>();
                if (QRCMItemCount > 0)
                {
                    string QRCMfilePath = QatConstants.QATServerPath + "\\QRCM_Files\\" + sourceTestCaseItem.TestCaseID + ".txt";
                
                    if (File.Exists(QRCMfilePath))
                    { 
                        using (StreamReader read = new StreamReader(QRCMfilePath))
                        {                            
                            string filepathOutput = string.Empty;
                            string totalactionlines = string.Empty;
                            while ((filepathOutput = read.ReadLine()) != null)
                            {
                                if (filepathOutput == ":QAT_Ref_Pay:")
                                {
                                    if (totalactionlines != string.Empty)
                                    {                                                                
                                        dynamic array = new System.Web.Script.Serialization.JavaScriptSerializer().DeserializeObject(totalactionlines);

                                        foreach (var item in array)
                                        {
                                            string value = new System.Web.Script.Serialization.JavaScriptSerializer().Serialize(item.Value);
                                            values.Add(item.Key, value);
                                        }
                                    }
                                                                     
                                    totalactionlines = string.Empty;
                                }
                                else
                                {
                                    totalactionlines += filepathOutput;
                                }
                            }
                        }



                    }

                }
				
                foreach (TestActionItem actionItem in sourceTestCaseItem.TestActionItemList)
                {

                    if (String.Equals(actionItem.ActionSelected, "Control Action", StringComparison.CurrentCultureIgnoreCase))
                    {
                        query = "select * from ControlAction where TCID ='" + sourceTestCaseItem.TestCaseID + "' and ActionID = '" + actionItem.TestActionID + "' order by ControlActionID";
                        dataTable = new DataTable();
                        dataTableReader = null;
                        dataAdapter = new SqlDataAdapter(query, connect);
                        dataAdapter.Fill(dataTable);
                        dataTableReader = dataTable.CreateDataReader();

                        if (dataTableReader.HasRows)
                        {
                            while (dataTableReader.Read())
                            {
                                TestControlItem testControlItem = actionItem.AddSetTestControlItem();

                                string componentType = dataTableReader.GetString(3).ToString();
                                if (testControlItem.TestControlComponentTypeList.Contains(componentType, StringComparer.CurrentCultureIgnoreCase))
                                {
                                    testControlItem.TestControlComponentTypeSelectedItem = componentType;

                                    string componentName = dataTableReader.GetString(4).ToString();

                                    string controlValue = string.Empty;
                                    if (dataTableReader.GetValue(5) != null)
                                        controlValue = dataTableReader.GetValue(5).ToString();

                                    if (testControlItem.TestControlComponentNameList.Contains(componentName, StringComparer.CurrentCultureIgnoreCase))
                                    {
                                        testControlItem.TestControlComponentNameSelectedItem = componentName;

                                        string componentProperty = string.Empty;
                                        if (dataTableReader[14] != System.DBNull.Value)
                                        {
                                            componentProperty = dataTableReader.GetString(14).ToString();
                                            if (componentProperty.Contains("~")) /*(((componentProperty.Contains("Channel")) || (componentProperty.Contains("Output")) || (componentProperty.Contains("Input")) || (componentProperty.Contains("Tap")) || (componentProperty.Contains("Bank Control")) || (componentProperty.Contains("Bank Select")) || (componentProperty.Contains("GPIO"))) & (componentProperty.Contains("~")))*/
                                            {
                                                string[] channelSplit = new string[2];
                                                string channelControl = string.Empty;
                                                int tiltCount = componentProperty.Count(x => x == '~');
                                                string channelWithTwoTilt = componentProperty;
                                                int idx = channelWithTwoTilt.LastIndexOf('~');
                                                channelSplit[0] = channelWithTwoTilt.Substring(0, idx);
                                                channelSplit[1] = channelWithTwoTilt.Substring(idx + 1);
                                                string QATPrefix = addQATPrefixToControl(componentProperty);//Added on 30-sep-2016
                                                if(!string.IsNullOrEmpty(QATPrefix))//Added on 30-sep-2016
                                                    componentProperty = channelSplit[1];
                                            }
                                        }

                                        if (testControlItem.TestControlPropertyList.Contains(componentProperty, StringComparer.CurrentCultureIgnoreCase))
                                        {
                                            testControlItem.TestControlPropertySelectedItem = componentProperty;
                                        }
                                        else if (testControlItem.TestControlPropertyList.Contains(componentProperty + " [" + controlValue  + "]", StringComparer.CurrentCultureIgnoreCase))
                                        {
                                            testControlItem.TestControlPropertySelectedItem = componentProperty + " [" + controlValue + "]";
                                        }

                                        string selectedChannel = string.Empty;
                                        if (dataTableReader[6] != System.DBNull.Value)
                                        {
                                            selectedChannel = dataTableReader.GetString(6).ToString();
                                        }

                                        if (testControlItem.ChannelSelectionList.Contains(selectedChannel, StringComparer.CurrentCultureIgnoreCase))
                                        {
                                            testControlItem.ChannelSelectionSelectedItem = selectedChannel;
                                        }
                                        else if (testControlItem.ChannelSelectionList.Contains(selectedChannel + " [" + controlValue + "]", StringComparer.CurrentCultureIgnoreCase))
                                        {
                                            testControlItem.ChannelSelectionSelectedItem = selectedChannel + " [" + controlValue + "]";
                                        }

                                        string selectionType = string.Empty;
                                        if (dataTableReader[15] != System.DBNull.Value)
                                        {
                                            selectionType = dataTableReader.GetString(15).ToString();
                                            if (String.Equals("Set by string", selectionType, StringComparison.CurrentCultureIgnoreCase))
                                            {
                                                testControlItem.InputSelectionComboSelectedItem = selectionType;
                                                testControlItem.TestControlPropertyInitialValueSelectedItem = dataTableReader.GetString(7).ToString();
                                            }

                                            else if (String.Equals("Set by value", selectionType, StringComparison.CurrentCultureIgnoreCase))
                                            {
                                                testControlItem.InputSelectionComboSelectedItem = selectionType;
                                                testControlItem.TestControlPropertyInitialValueSelectedItem = dataTableReader.GetString(7).ToString();
                                            }
                                            else if (String.Equals("Set by position", selectionType, StringComparison.CurrentCultureIgnoreCase))
                                            {
                                                testControlItem.InputSelectionComboSelectedItem = selectionType;
                                                testControlItem.TestControlPropertyInitialValueSelectedItem = dataTableReader.GetString(7).ToString();
                                            }
                                            //if (!testControlItem.InputSelectionComboList.Contains(selectionType, StringComparer.CurrentCultureIgnoreCase))
                                            //    testControlItem.InputSelectionComboSelectedItem = selectionType;
                                        }
                                    }
                                }

                                if (String.Equals(dataTableReader.GetString(8).ToString(), "True", StringComparison.CurrentCultureIgnoreCase))
                                    testControlItem.RampIsChecked = true;
                                else
                                    testControlItem.RampIsChecked = false;

                                testControlItem.RampSetting = dataTableReader.GetString(9).ToString();

                                if (testControlItem.ChannelSelectionList.Count > 0)
                                {
                                    if (String.Equals(dataTableReader.GetString(10).ToString(), "True", StringComparison.CurrentCultureIgnoreCase))
                                        testControlItem.LoopIsChecked = true;
                                    else
                                        testControlItem.LoopIsChecked = false;

                                    if (testControlItem.LoopIsChecked)
                                    {
                                        if (!string.IsNullOrEmpty(dataTableReader.GetString(11)) && testControlItem.ChannelSelectionList.Count >= Convert.ToInt32(dataTableReader.GetString(11)))
                                        {
                                            testControlItem.LoopStart = dataTableReader.GetString(11);

                                            if (dataTableReader.GetValue(12) != null && dataTableReader.GetValue(12).ToString() != string.Empty && testControlItem.ChannelSelectionList.Count >= Convert.ToInt32(dataTableReader.GetValue(12)))
                                                testControlItem.LoopEnd = dataTableReader.GetString(12);
                                            else
                                                testControlItem.LoopEnd = string.Empty;

                                            if (dataTableReader.GetValue(13) != null && dataTableReader.GetValue(13).ToString() != string.Empty && testControlItem.LoopEnd != null && testControlItem.LoopEnd != string.Empty)
                                            {
                                                if ((Convert.ToInt32(dataTableReader.GetValue(13)) > 0) && (Convert.ToInt32(dataTableReader.GetValue(13)) <= testControlItem.ChannelSelectionList.Count) && (Convert.ToInt32(dataTableReader.GetValue(13)) <= Convert.ToInt32(testControlItem.LoopEnd)))
                                                {
                                                    testControlItem.LoopIncrement = dataTableReader.GetString(13);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                    if (String.Equals(actionItem.ActionSelected, "Ssh/Telnet Action", StringComparison.CurrentCultureIgnoreCase))
                    {
                        query = "select * from TelnetAction where TCID ='" + sourceTestCaseItem.TestCaseID + "' and ActionID = '" + actionItem.TestActionID + "'";
                        dataTable = new DataTable();
                        dataTableReader = null;
                        dataAdapter = new SqlDataAdapter(query, connect);
                        dataAdapter.Fill(dataTable);
                        dataTableReader = dataTable.CreateDataReader();
                        if (dataTableReader.HasRows)
                        {
                            while (dataTableReader.Read())
                            {
                                TestTelnetItem testTelnetItem = actionItem.AddSetTestTelnetItem();
                                testTelnetItem.TelnetCommand = dataTableReader.GetString(3).ToString();

                                string telnetdevicemodel = dataTableReader.GetString(5).ToString();

                                string telnetDeviceName = dataTableReader.GetString(4).ToString();
                                List<string> telnetDeviceNameList = telnetDeviceName.Split(',').ToList();
                                if (telnetDeviceNameList.Count > 1)
                                {
                                    if (telnetDeviceNameList.Count() == testTelnetItem.TelnetSelectedDeviceModel.Count())
                                    {                          
                                        List<string> telnetDeviceModelList = telnetdevicemodel.Split(',').ToList();

                                        for (int i = 0; i < testTelnetItem.TelnetSelectedDeviceModel.Count; i++)
                                        {
                                            if (testTelnetItem.TelnetSelectedDeviceModel.Keys.Contains(telnetDeviceNameList[i]) && testTelnetItem.TelnetSelectedDeviceModel[telnetDeviceNameList[i]] == telnetDeviceModelList[i])
                                                testTelnetItem.TelnetSelectedDevice = "All devices";
                                            else
                                            {
                                                testTelnetItem.TelnetSelectedDevice = null;
                                                break;
                                            }
                                        }
                                    }                                                    
                                }
                                else
                                {
                                    if (testTelnetItem.TelnetSelectedDeviceModel.Keys.Contains(telnetDeviceName) && testTelnetItem.TelnetSelectedDeviceModel[telnetDeviceName] == telnetdevicemodel)
                                    {
                                        testTelnetItem.TelnetSelectedDevice = telnetDeviceName;
                                    }                                       
                                }
                            }
                        }
                    }

                    if (string.Equals(actionItem.ActionSelected, "CEC Action", StringComparison.CurrentCultureIgnoreCase))
                    {
                        query = "select * from CECAction where TCID ='" + sourceTestCaseItem.TestCaseID + "' and ActionID = '" + actionItem.TestActionID + "'";
                        dataTable = new DataTable();
                        dataTableReader = null;
                        dataAdapter = new SqlDataAdapter(query, connect);
                        dataAdapter.Fill(dataTable);
                        dataTableReader = dataTable.CreateDataReader();
                        if (dataTableReader.HasRows)
                        {
                            while (dataTableReader.Read())
                            {
                                TestCECItem testTelnetItem = actionItem.AddSetTestCECItem();
                                testTelnetItem.DeviceselectionSelecetdItem = dataTableReader.GetString(4).ToString();
                                testTelnetItem.CECCommandListSelectedItem = dataTableReader.GetString(5).ToString();

                                if(testTelnetItem.CECCommandListSelectedItem == "Others")
                                    testTelnetItem.CECActionOpcode = dataTableReader.GetString(6).ToString();
                            }
                        }
                    }
                    if (string.Equals(actionItem.VerificationSelected, "CEC Verification", StringComparison.CurrentCultureIgnoreCase))
                    {
                        query = "select * from CECVerification where TCID ='" + sourceTestCaseItem.TestCaseID + "' and ActionID = '" + actionItem.TestActionID + "'";
                        dataTable = new DataTable();
                        dataTableReader = null;
                        dataAdapter = new SqlDataAdapter(query, connect);
                        dataAdapter.Fill(dataTable);
                        dataTableReader = dataTable.CreateDataReader();
                        if (dataTableReader.HasRows)
                        {
                            while (dataTableReader.Read())
                            {
                                TestVerifyCECItem testTelnetItem = actionItem.AddVerifyTestCECItem();
                               
                                    testTelnetItem.CECverificationOpcode = dataTableReader.GetString(6).ToString();

                                if (dataTableReader[7] != System.DBNull.Value)
                                    actionItem.cecVerificationbox_selected = dataTableReader.GetString(7).ToString();
                            }
                        }
                    }

                    if (string.Equals(actionItem.ActionSelected, "User Action", StringComparison.CurrentCultureIgnoreCase))
                    {
                        query = "select * from UserAction where TCID ='" + sourceTestCaseItem.TestCaseID + "' and ActionID = '" + actionItem.TestActionID + "'";
                        dataTable = new DataTable();
                        dataTableReader = null;
                        dataAdapter = new SqlDataAdapter(query, connect);
                        dataAdapter.Fill(dataTable);
                        dataTableReader = dataTable.CreateDataReader();
                        if (dataTableReader.HasRows)
                        {
                            while (dataTableReader.Read())
                            {
                                TestUserActionItem testUserItem = actionItem.AddSetTestUserItem();
                                testUserItem.ActionUserText = dataTableReader.GetString(3).ToString();  
                            }
                        }
                    }

                    if (string.Equals(actionItem.VerificationSelected, "User Verification", StringComparison.CurrentCultureIgnoreCase))
                    {
                        query = "select * from UserVerification where TCID ='" + sourceTestCaseItem.TestCaseID + "' and ActionID = '" + actionItem.TestActionID + "'";
                        dataTable = new DataTable();
                        dataTableReader = null;
                        dataAdapter = new SqlDataAdapter(query, connect);
                        dataAdapter.Fill(dataTable);
                        dataTableReader = dataTable.CreateDataReader();
                        if (dataTableReader.HasRows)
                        {
                            while (dataTableReader.Read())
                            {
                                TestUserVerifyItem testUserItem = actionItem.AddTestUserVerifyItem();
                                testUserItem.VerifyUserText = dataTableReader.GetString(3).ToString();
                            }
                        }
                    }

                    if (string.Equals(actionItem.ActionSelected, "QRCM Action", StringComparison.CurrentCultureIgnoreCase))
                    {
                        query = "select QRCMActionID,Build_version,Device_name,Method_name,Input_arguments,PayloadFilePath from QRCMAction where TCID ='" + sourceTestCaseItem.TestCaseID + "' and ActionID = '" + actionItem.TestActionID + "'";
                        dataTable = new DataTable();
                        dataTableReader = null;
                        dataAdapter = new SqlDataAdapter(query, connect);
                        dataAdapter.Fill(dataTable);
                        dataTableReader = dataTable.CreateDataReader();
                        if (dataTableReader.HasRows)
                        {
                            while (dataTableReader.Read())
                            {
                                string deviceSelected = string.Empty;
                                string methodSelected = string.Empty;
                                string actionUniqueID = string.Empty;

                                if (dataTableReader[0] != System.DBNull.Value)
                                    actionUniqueID = dataTableReader.GetInt32(0).ToString();

                                if (actionItem.ActionQRCMVersionSelected == null || actionItem.ActionQRCMVersionSelected == string.Empty)
                                {
                                    if (dataTableReader[1] != System.DBNull.Value)
                                    {
                                        string buildversion = dataTableReader.GetString(1).ToString();
                                        if (actionItem.ActionQRCMVersionList.Contains(buildversion))
                                        { actionItem.ActionQRCMVersionSelected = buildversion; actionItem.ActionQRCMPreVerSelected = actionItem.ActionQRCMVersionSelected; }

                                    }
                                }

                                TestActionQRCMItem testQRCMItem = actionItem.AddSetTestQRCMItem();

                                if (dataTableReader[2] != System.DBNull.Value)
                                {
                                    deviceSelected = dataTableReader.GetString(2).ToString();
                                    if (testQRCMItem.ActionQRCM_DevicesList.Contains(deviceSelected))
                                        testQRCMItem.QRCM_DeviceSelectedItem = deviceSelected;
                                }

                                if (dataTableReader[3] != System.DBNull.Value)
                                {
                                    methodSelected = dataTableReader.GetString(3).ToString();
                                    if (testQRCMItem.ActionQRCM_MethodsList.Contains(methodSelected))
                                        testQRCMItem.QRCM_MethodsSelectedItem = methodSelected;
                                }

                                if (dataTableReader[4] != System.DBNull.Value)
                                    testQRCMItem.ActionUserArguments = dataTableReader.GetString(4).ToString();

                                //////assigning payload values for each actions using actionprimary key
                                if (dataTableReader[5] != System.DBNull.Value && dataTableReader.GetString(5).ToString() != string.Empty)
                                {
                                    if (values.ContainsKey("Action_" + actionUniqueID))
                                    {
                                        testQRCMItem.SetPayloadContent = values["Action_" + actionUniqueID];
                                    }
                                }
                                else if (testQRCMItem.SetPayloadBtnIsEnabled)
                                {
                                    if (!errorDisplyedQRCM && sourceTestCaseItem.IsEditModeEnabled)
                                    {
                                        MessageBox.Show("Payload/Reference file path is not available", "QAT Error Message", MessageBoxButton.OK, MessageBoxImage.Error);
                                        errorDisplyedQRCM = true;
                                    }
                                }
                            }
                        }
                    }

                    if (string.Equals(actionItem.VerificationSelected, "QRCM Verification", StringComparison.CurrentCultureIgnoreCase))
                    {
                        query = "select QRCMVerificationID,Build_version,Device_name,Method_name,Input_arguments,ReferenceFilePath from QRCMVerification where TCID ='" + sourceTestCaseItem.TestCaseID + "' and ActionID = '" + actionItem.TestActionID + "'";
                        dataTable = new DataTable();
                        dataTableReader = null;
                        dataAdapter = new SqlDataAdapter(query, connect);
                        dataAdapter.Fill(dataTable);
                        dataTableReader = dataTable.CreateDataReader();
                        if (dataTableReader.HasRows)
                        {
                            while (dataTableReader.Read())
                            {
                                string deviceSelected = string.Empty;
                                string methodSelected = string.Empty;

                                string verifyUniqueID = string.Empty;

                                if (dataTableReader[0] != System.DBNull.Value)
                                    verifyUniqueID = dataTableReader.GetInt32(0).ToString();


                                if (actionItem.VerifyQRCMVersionSelected == null || actionItem.VerifyQRCMVersionSelected == string.Empty)
                                {
                                    if (dataTableReader[1] != System.DBNull.Value)
                                    {
                                        string buildversion = dataTableReader.GetString(1).ToString();
                                        if (actionItem.VerifyQRCMVersionList.Contains(buildversion))
                                        {
                                            actionItem.VerifyQRCMVersionSelected = buildversion;
                                            actionItem.VerifyQRCMPreVerSelected = actionItem.VerifyQRCMVersionSelected;
                                        }
                                    }
                                }

                                TestVerifyQRCMItem testQRCMItem = actionItem.AddVerifyTestQRCMItem();

                                if (dataTableReader[2] != System.DBNull.Value)
                                {
                                    deviceSelected = dataTableReader.GetString(2).ToString();
                                    if (testQRCMItem.VerifyQRCM_DevicesList.Contains(deviceSelected))
                                        testQRCMItem.QRCM_DeviceSelectedItem = deviceSelected;
                                }

                                if (dataTableReader[3] != System.DBNull.Value)
                                {
                                    methodSelected = dataTableReader.GetString(3).ToString();
                                    if (testQRCMItem.VerifyQRCM_MethodsList.Contains(methodSelected))
                                        testQRCMItem.QRCM_MethodsSelectedItem = methodSelected;
                                }

                                if (dataTableReader[4] != System.DBNull.Value)
                                    testQRCMItem.VerifyUserArguments = dataTableReader.GetString(4).ToString();

                                //////assigning reference values for each verification rows using verification primary key
                                if (dataTableReader[5] != System.DBNull.Value && dataTableReader.GetString(5).ToString() != string.Empty)
                                {
                                    if (values.ContainsKey("Verification_" + verifyUniqueID))
                                    {
                                        testQRCMItem.SetReferenceContent = values["Verification_" + verifyUniqueID];
                                    }
                                }
                                else if (testQRCMItem.SetReferenceBtnIsEnabled)
                                {
                                    if (!errorDisplyedQRCM && sourceTestCaseItem.IsEditModeEnabled)
                                    {
                                        MessageBox.Show("Payload/Reference file path is not available", "QAT Error Message", MessageBoxButton.OK, MessageBoxImage.Error);
                                        errorDisplyedQRCM = true;
                                    }
                                }
                            }
                        }
                    }                  

                    if (string.Equals(actionItem.VerificationSelected, "Script Verification", StringComparison.CurrentCultureIgnoreCase))
                    {
                        query = "select * from ScriptVerification where TCID ='" + sourceTestCaseItem.TestCaseID + "' and ActionID = '" + actionItem.TestActionID + "'";
                        dataTable = new DataTable();
                        dataTableReader = null;
                        dataAdapter = new SqlDataAdapter(query, connect);
                        dataAdapter.Fill(dataTable);
                        dataTableReader = dataTable.CreateDataReader();
                        if (dataTableReader.HasRows)
                        {
                            while (dataTableReader.Read())
                            {                               
                                TestScriptVerification testScriptVerifyItem =actionItem.AddVerifyTestScriptItem();

                                if (dataTableReader[3] != System.DBNull.Value)
                                    testScriptVerifyItem.DevicenamelistSelectedItem = dataTableReader.GetString(3).ToString();
                                if (dataTableReader[4] != System.DBNull.Value)
                                    testScriptVerifyItem.DeviceModel = dataTableReader.GetString(4).ToString();

                                if (dataTableReader[5] != System.DBNull.Value && (dataTableReader.GetString(5).StartsWith("CPU Monitoring")))
                                {
                                    string[] cpuVal = dataTableReader.GetString(5).ToString().Split('-');
                                    testScriptVerifyItem.VerifyScriptActionSelectedItem = cpuVal[0];
									if(cpuVal.Count() > 1)
										testScriptVerifyItem.CPUNumberSelectedItem = cpuVal[1];
                                }
                                else
                                    testScriptVerifyItem.VerifyScriptActionSelectedItem = dataTableReader.GetString(5).ToString();
                                
                                if (testScriptVerifyItem.DeviceModel != null && testScriptVerifyItem.DeviceModel != string.Empty && testScriptVerifyItem.DeviceModel.ToUpper().StartsWith("CORE") && testScriptVerifyItem.VerifyScriptActionSelectedItem != "CPU Monitoring" && testScriptVerifyItem.VerifyScriptActionSelectedItem != "Deploy Monitoring" && testScriptVerifyItem.VerifyScriptActionSelectedItem != "LoadFromCore Monitoring")
                                    testScriptVerifyItem.VerifyDesignDevicesVisibility = Visibility.Visible;
                                else
                                    testScriptVerifyItem.VerifyDesignDevicesVisibility = Visibility.Collapsed;

                                if (dataTableReader[6] != System.DBNull.Value)
                                    testScriptVerifyItem.CustomCommand = dataTableReader.GetString(6).ToString();
                                if (dataTableReader[7] != System.DBNull.Value)
                                    testScriptVerifyItem.RegexMatch = dataTableReader.GetString(7).ToString();
                                if (dataTableReader[8] != System.DBNull.Value)
                                    testScriptVerifyItem.Upperlimit = dataTableReader.GetString(8).ToString();
                                if (dataTableReader[9] != System.DBNull.Value)
                                    testScriptVerifyItem.Lowerlimit = dataTableReader.GetString(9).ToString();
                                if (dataTableReader[10] != System.DBNull.Value)
                                    testScriptVerifyItem.LimitUnitSelectedItem = dataTableReader.GetString(10).ToString();
                                if (dataTableReader[11] != System.DBNull.Value)
                                    actionItem.Script_checktimeTextbox = dataTableReader.GetString(11).ToString();
                                if (dataTableReader[12] != System.DBNull.Value)
                                    actionItem.Script_ChecktimeUnitSelected = dataTableReader.GetString(12).ToString();
                                if (dataTableReader[13] != System.DBNull.Value)
                                    actionItem.Script_DurationTextbox = dataTableReader.GetString(13).ToString();
                                if (dataTableReader[14] != System.DBNull.Value)
                                    actionItem.Script_DurationTimeUnitSelected = dataTableReader.GetString(14).ToString();
                                if (dataTableReader[15] != System.DBNull.Value)
                                    testScriptVerifyItem.VerifyDesignDevicesIsChecked = Convert.ToBoolean(dataTableReader.GetString(15));
                                if (dataTableReader[16] != System.DBNull.Value)
                                    actionItem.ExecuteIterationChkboxIsChecked = Convert.ToBoolean(dataTableReader.GetString(16));
                            }
                        }
                    }



                    if (string.Equals(actionItem.VerificationSelected, "QR code Verification", StringComparison.CurrentCultureIgnoreCase))
                    {
                        query = "select * from QRVerification where TCID ='" + sourceTestCaseItem.TestCaseID + "' and ActionID = '" + actionItem.TestActionID + "'";
                        dataTable = new DataTable();
                        dataTableReader = null;
                        dataAdapter = new SqlDataAdapter(query, connect);
                        dataAdapter.Fill(dataTable);
                        dataTableReader = dataTable.CreateDataReader();
                        if (dataTableReader.HasRows)
                        {
                            while (dataTableReader.Read())
                            {
                                TestVerifyQRItem testverifyQRcodeItem = actionItem.AddVerifyTestQRItem();
                                string cameraselecteditem = string.Empty;
                                if (dataTableReader.GetString(3).ToString() != null)
                                     cameraselecteditem= dataTableReader.GetString(3).ToString();
                                if(testverifyQRcodeItem.CameraList1.Contains(cameraselecteditem, StringComparer.CurrentCultureIgnoreCase))
                                {
                                    testverifyQRcodeItem.CameraSelectedItem = cameraselecteditem;
                                }                               
                             
                                if (dataTableReader.GetString(5).ToString() != null)
                                    testverifyQRcodeItem.QRverificationcode= dataTableReader.GetString(5).ToString().Trim();
                                if (dataTableReader.GetString(6).ToString() != null)
                                    testverifyQRcodeItem.QRverifytype = dataTableReader.GetString(6).ToString();

                            }
                        }
                    }

                    if (String.Equals(actionItem.ActionSelected, "Firmware Action", StringComparison.CurrentCultureIgnoreCase))
                    {
                        query = "select * from FirmwareAction where TCID ='" + sourceTestCaseItem.TestCaseID + "' and ActionID = '" + actionItem.TestActionID + "'";
                        dataTable = new DataTable();
                        dataTableReader = null;
                        dataAdapter = new SqlDataAdapter(query, connect);
                        dataAdapter.Fill(dataTable);
                        dataTableReader = dataTable.CreateDataReader();
                        if (dataTableReader.HasRows)
                        {
                            while (dataTableReader.Read())
                            {
                                TestFirmwareItem testFirmwareItem = actionItem.AddSetTestFirmwareItem();

                                string firmwareTypeSelected = dataTableReader.GetString(3).ToString();

                                int index = QatConstants.TestFirmwareItemFirmwareUpdateTypeDatabaseList.IndexOf(firmwareTypeSelected);
                                if (index < 0)
                                    testFirmwareItem.FirmwareTypeSelected = null;
                                else
                                    testFirmwareItem.FirmwareTypeSelected = QatConstants.TestFirmwareItemFirmwareUpdateTypeDisplayList[index];

                                testFirmwareItem.FirmwareBrowseLocation = dataTableReader.GetString(4).ToString();

                                testFirmwareItem.FirmwareDate = dataTableReader.GetString(5).ToString();

                                if (dataTableReader[6] != System.DBNull.Value)
                                {
                                    if(dataTableReader.GetString(6).ToString()!=string.Empty)
                                    testFirmwareItem.InstallSelectionComboSelectedItem = dataTableReader.GetString(6).ToString();
                                }

                                if (dataTableReader[7] != System.DBNull.Value)
                                {
                                    testFirmwareItem.TimeSelectionComboSelectedItem = dataTableReader.GetString(7).ToString();
                                }

                                if (dataTableReader[8] != System.DBNull.Value)
                                {
                                    testFirmwareItem.MeasureFirmwareUpTime = Convert.ToBoolean(dataTableReader.GetString(8));
                                }
                            }
                        }
                    }

                    //if (String.Equals(actionItem.ActionSelected, "Designer Action", StringComparison.CurrentCultureIgnoreCase))
                    //{
                    //    actionItem.AddSetTestDesignerItem();
                    //}

                    if (String.Equals(actionItem.ActionSelected, "Net Pairing Action", StringComparison.CurrentCultureIgnoreCase))
                    {
                      
                        query = "select * from NetpairingAction where TCID ='" + sourceTestCaseItem.TestCaseID + "' and ActionID = '" + actionItem.TestActionID + "'";
                        dataTable = new DataTable();
                        dataTableReader = null;
                        dataAdapter = new SqlDataAdapter(query, connect);
                        dataAdapter.Fill(dataTable);
                        dataTableReader = dataTable.CreateDataReader();
                        

                        TestNetPairingItem testnetpairingitem = actionItem.AddSetTestNetPairingItem();

                        if (dataTableReader.HasRows)
                        {
                            while (dataTableReader.Read())
                            {
                                if (testnetpairingitem.DutDeviceList.Count > 0)
                                {
                                    string itemDeviceType = string.Empty;
                                    string itemDeviceName = string.Empty;
                                    string itemNetPairingSelected = string.Empty;

                                    if (dataTableReader[3] != System.DBNull.Value)                                    
                                        itemDeviceType = dataTableReader.GetString(3).ToString();                                    
                                    if (dataTableReader[4] != System.DBNull.Value)                                    
                                        itemDeviceName = dataTableReader.GetString(4).ToString();                                    
                                    if (dataTableReader[5] != System.DBNull.Value)                                    
                                        itemNetPairingSelected = dataTableReader.GetString(5).ToString();
                                    

                                    DUT_DeviceItem currentItem =  testnetpairingitem.DutDeviceList.Where(x => x.ItemDeviceType == itemDeviceType && x.ItemDeviceName == itemDeviceName).FirstOrDefault();
                                    if (currentItem != null)
                                        currentItem.ItemNetPairingSelected = itemNetPairingSelected;


                            
                                 
                                }
                            }
                        }
                     }

                    if (String.Equals(actionItem.ActionSelected, "Designer Action", StringComparison.CurrentCultureIgnoreCase))
                    {
                        query = "select * from DesignerAction where TCID ='" + sourceTestCaseItem.TestCaseID + "' and ActionID = '" + actionItem.TestActionID + "'";
                        dataTable = new DataTable();
                        dataTableReader = null;
                        dataAdapter = new SqlDataAdapter(query, connect);
                        dataAdapter.Fill(dataTable);
                        dataTableReader = dataTable.CreateDataReader();
                     

                        TestDesignerItem testDesigneritem = actionItem.AddSetTestDesignerItem();

                        if (dataTableReader.HasRows)
                        {
                            while (dataTableReader.Read())
                            {
                                //if (testDesigneritem.ParentTestActionItem.SetTestDesignerList.Count > 0)

                                if (dataTableReader[3] != System.DBNull.Value)
                                {
                                    testDesigneritem.ConnectDesigner = Convert.ToBoolean(dataTableReader.GetString(3));
                                }

                                if (dataTableReader[4] != System.DBNull.Value)
                                {
                                    testDesigneritem.DisconnectDesigner = Convert.ToBoolean(dataTableReader.GetString(4));
                                }

                                if (dataTableReader[5] != System.DBNull.Value)
                                {
                                    testDesigneritem.EmulateDesigner = Convert.ToBoolean(dataTableReader.GetString(5));
                                }

                                if (dataTableReader[6] != System.DBNull.Value && dataTableReader[6].ToString() != string.Empty)
                                {
                                    testDesigneritem.newdesigncheck = Convert.ToBoolean(dataTableReader.GetString(6));
                                }

                                if (dataTableReader[7] != System.DBNull.Value && dataTableReader[7].ToString() != string.Empty)
                                {
                                    testDesigneritem.ChkNoOfTimeDeployCheck = Convert.ToBoolean(dataTableReader.GetString(7));
                                }

                                if (dataTableReader[8] != System.DBNull.Value && dataTableReader[8].ToString() != string.Empty)
                                {
                                    testDesigneritem.NoOfTimesDeployed = dataTableReader.GetString(8);
                                }

                                if (dataTableReader[11] != System.DBNull.Value && dataTableReader[11].ToString() != string.Empty)
                                {
                                    testDesigneritem.Loadfromcore = Convert.ToBoolean(dataTableReader.GetString(11));
                                }

                                if (dataTableReader[9] != System.DBNull.Value && dataTableReader[9].ToString() != string.Empty)
                                {
                                    testDesigneritem.DesignerTimeout = dataTableReader.GetString(9);
                                }

                                if (dataTableReader[10] != System.DBNull.Value && dataTableReader[10].ToString() != string.Empty)
                                {
                                    testDesigneritem.DesignerTimeoutUnitSelected = dataTableReader.GetString(10);
                                }
                            }
                        }                        
                    }

                    if (String.Equals(actionItem.ActionSelected, "Usb Action", StringComparison.CurrentCultureIgnoreCase))
                    {
                        query = "select * from UsbAction where TCID ='" + sourceTestCaseItem.TestCaseID + "' and ActionID = '" + actionItem.TestActionID + "'";
                        dataTable = new DataTable();
                        dataTableReader = null;
                        dataAdapter = new SqlDataAdapter(query, connect);
                        dataAdapter.Fill(dataTable);
                        dataTableReader = dataTable.CreateDataReader();

                        if (dataTableReader.HasRows)
                        {
                            while (dataTableReader.Read())
                            {
                                //if (testDesigneritem.ParentTestActionItem.SetTestDesignerList.Count > 0)

                                TestUsbAudioBridging testusbitem = actionItem.AddSetTestUsbItem();

                                if (dataTableReader[3] != System.DBNull.Value)
                                {
                                    testusbitem.UsbAudioBridgeTypeSelectedItem = dataTableReader.GetString(3);
                                }
                                if (dataTableReader[4] != System.DBNull.Value)
                                {
                                    testusbitem.UsbAudioTypeSelectedItem = dataTableReader.GetString(4);
                                }
                                if (dataTableReader[5] != System.DBNull.Value)
                                {
                                    testusbitem.UsbAudioDeviceSelectedItem = dataTableReader.GetString(5);
                                }
                                if (dataTableReader[6] != System.DBNull.Value)
                                {
                                    testusbitem.UsbDefaultDeviceOptionSelectedItem = dataTableReader.GetString(6);
                                }
                            }
                        }

                    }

                    if (String.Equals(actionItem.VerificationSelected, "Control Verification", StringComparison.CurrentCultureIgnoreCase))
                    {
                        query = "select * from ControlVerification where TCID ='" + sourceTestCaseItem.TestCaseID + "' and ActionID = '" + actionItem.TestActionID + "' order by ControlVerificationID";
                        dataTable = new DataTable();
                        dataTableReader = null;
                        dataAdapter = new SqlDataAdapter(query, connect);
                        dataAdapter.Fill(dataTable);
                        dataTableReader = dataTable.CreateDataReader();

                        if (dataTableReader.HasRows)
                        {
                            while (dataTableReader.Read())
                            {
                                TestControlItem testControlItem = actionItem.AddVerifyTestControlItem();

                                string componentType = dataTableReader.GetString(3).ToString();
                                if (testControlItem.TestControlComponentTypeList.Contains(componentType, StringComparer.CurrentCultureIgnoreCase))
                                {
                                    testControlItem.TestControlComponentTypeSelectedItem = componentType;

                                    string componentName = dataTableReader.GetString(4).ToString();
                                    if (testControlItem.TestControlComponentNameList.Contains(componentName, StringComparer.CurrentCultureIgnoreCase))
                                    {
                                        testControlItem.TestControlComponentNameSelectedItem = componentName;

                                        string componentProperty = string.Empty;
                                        if (dataTableReader[8] != System.DBNull.Value)
                                        {
                                            componentProperty = dataTableReader.GetString(8).ToString();
                                            if (componentProperty.Contains("~"))/*(((componentProperty.Contains("Channel"))|| (componentProperty.Contains("Output"))|| (componentProperty.Contains("Input")) || (componentProperty.Contains("Tap")) || (componentProperty.Contains("Bank Control")) || (componentProperty.Contains("Bank Select")) || (componentProperty.Contains("GPIO"))) & (componentProperty.Contains("~")))*/
                                            {
                                                string[] channelSplit = new string[2];
                                                string channelControl = string.Empty;
                                                int tiltCount = componentProperty.Count(x => x == '~');
                                                string channelWithTwoTilt = componentProperty;
                                                int idx = channelWithTwoTilt.LastIndexOf('~');
                                                channelSplit[0] = channelWithTwoTilt.Substring(0, idx);
                                                channelSplit[1] = channelWithTwoTilt.Substring(idx + 1);
                                                string QATPrefix = addQATPrefixToControl(componentProperty);//Added on 30-sep-2016
                                                if (!string.IsNullOrEmpty(QATPrefix))//Added on 30-sep-2016
                                                    componentProperty = channelSplit[1];

                                            }
                                        }

                                        string controlValue = string.Empty;
                                        if (dataTableReader.GetValue(5) != null)
                                            controlValue = dataTableReader.GetValue(5).ToString();

                                        //if (dataTableReader[9] != System.DBNull.Value)
                                        //{
                                        //    testControlItem.InputSelectionComboSelectedItem = dataTableReader.GetString(9).ToString();
                                        //}
                                        if (testControlItem.VerifyTestControlPropertyList.Contains(componentProperty, StringComparer.CurrentCultureIgnoreCase))
                                        {
                                            //testControlItem.TestControlPropertySelectedItem = componentProperty;
                                            //testControlItem.TestControlPropertyInitialValueSelectedItem = dataTableReader.GetString(7).ToString();
                                            testControlItem.TestControlPropertySelectedItem = componentProperty;
                                            //if (testControlItem.valueComboboxVisibility == Visibility.Visible)
                                            //    testControlItem.TestControlComboValueSelectedItem = dataTableReader.GetString(7).ToString();
                                            //else if (testControlItem.valueTextboxVisibility == Visibility.Visible)
                                            //testControlItem.TestControlPropertyInitialValueSelectedItem = dataTableReader.GetString(7).ToString();
                                        }
                                        else if (testControlItem.VerifyTestControlPropertyList.Contains(componentProperty + " [" + controlValue + "]", StringComparer.CurrentCultureIgnoreCase))
                                        {
                                            testControlItem.TestControlPropertySelectedItem = componentProperty + " [" + controlValue + "]";
                                        }
										
                                        string selectionType = string.Empty;

                                        string selectedChannel = string.Empty;
                                        if (dataTableReader[6] != System.DBNull.Value)
                                        {
                                            selectedChannel = dataTableReader.GetString(6).ToString();
                                        }

                                        if (testControlItem.ChannelSelectionList.Contains(selectedChannel, StringComparer.CurrentCultureIgnoreCase))
                                        {
                                            testControlItem.ChannelSelectionSelectedItem = selectedChannel;
                                        }
                                        else if (testControlItem.ChannelSelectionList.Contains(selectedChannel + " [" + controlValue + "]", StringComparer.CurrentCultureIgnoreCase))
                                        {
                                            testControlItem.ChannelSelectionSelectedItem = selectedChannel + " [" + controlValue + "]";
                                        }

                                        if (dataTableReader[9] != System.DBNull.Value)
                                        {
                                            selectionType = dataTableReader.GetString(9).ToString();
                                            if (String.Equals("Set by string", selectionType, StringComparison.CurrentCultureIgnoreCase))
                                            {
                                                testControlItem.InputSelectionComboSelectedItem = selectionType;
                                                testControlItem.TestControlPropertyInitialValueSelectedItem = dataTableReader.GetString(7).ToString();
                                            }

                                            else if (String.Equals("Set by value", selectionType, StringComparison.CurrentCultureIgnoreCase))
                                            {
                                                testControlItem.InputSelectionComboSelectedItem = selectionType;
                                                testControlItem.TestControlPropertyInitialValueSelectedItem = dataTableReader.GetString(7).ToString();
                                            }
                                            else if (String.Equals("Set by position", selectionType, StringComparison.CurrentCultureIgnoreCase))
                                            {
                                                testControlItem.InputSelectionComboSelectedItem = selectionType;
                                                testControlItem.TestControlPropertyInitialValueSelectedItem = dataTableReader.GetString(7).ToString();
                                            }
                                            //if (!testControlItem.InputSelectionComboList.Contains(selectionType, StringComparer.CurrentCultureIgnoreCase))
                                            //    testControlItem.InputSelectionComboSelectedItem = selectionType;
                                        }

                                        if (dataTableReader[10] != System.DBNull.Value)
                                        {
                                            testControlItem.MaximumLimit = dataTableReader.GetString(10).ToString();
                                        }
                                        if (dataTableReader[11] != System.DBNull.Value)
                                        {
                                            testControlItem.MinimumLimit = dataTableReader.GetString(11).ToString();
                                        }

                                        if (testControlItem.ChannelSelectionList.Count > 0)
                                        {
                                            if (dataTableReader[12] != System.DBNull.Value)
                                            {
                                                if (String.Equals(dataTableReader.GetString(12).ToString(), "True", StringComparison.CurrentCultureIgnoreCase))
                                                    testControlItem.LoopIsChecked = true;
                                                else
                                                    testControlItem.LoopIsChecked = false;
                                            }

                                            if (testControlItem.LoopIsChecked)
                                            {
                                                if (dataTableReader[13] != System.DBNull.Value && testControlItem.ChannelSelectionList.Count >= Convert.ToInt32(dataTableReader.GetString(13)))
                                                {
                                                    testControlItem.LoopStart = dataTableReader.GetString(13).ToString();

                                                    if (dataTableReader[14] != System.DBNull.Value && dataTableReader[14].ToString() != string.Empty)
                                                    {
                                                        if (testControlItem.ChannelSelectionList.Count >= Convert.ToInt32(dataTableReader.GetString(14)))
                                                            testControlItem.LoopEnd = dataTableReader.GetString(14).ToString();
                                                        else
                                                            testControlItem.LoopEnd = string.Empty;
                                                    }

                                                    if (dataTableReader[15] != System.DBNull.Value && dataTableReader[15].ToString() != string.Empty)
                                                    {
                                                        if (testControlItem.LoopEnd != null && testControlItem.LoopEnd != string.Empty)
                                                        {
                                                            if ((Convert.ToInt32(dataTableReader.GetString(15)) > 0) && (Convert.ToInt32(dataTableReader.GetString(15)) <= testControlItem.ChannelSelectionList.Count) && (Convert.ToInt32(dataTableReader.GetString(15)) <= Convert.ToInt32(testControlItem.LoopEnd)))
                                                            {
                                                                testControlItem.LoopIncrement = dataTableReader.GetString(15);
                                                            }
                                                        }
                                                        else
                                                        {
                                                            testControlItem.LoopIncrement = string.Empty;
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                    if (String.Equals(actionItem.VerificationSelected, "Ssh/Telnet Verification", StringComparison.CurrentCultureIgnoreCase))
                    {
                        query = "select * from TelnetVerify where TCID ='" + sourceTestCaseItem.TestCaseID + "' and ActionID = '" + actionItem.TestActionID + "'";
                        dataTable = new DataTable();
                        dataTableReader = null;
                        dataAdapter = new SqlDataAdapter(query, connect);
                        dataAdapter.Fill(dataTable);
                        dataTableReader = dataTable.CreateDataReader();
                        if (dataTableReader.HasRows)
                        {
                            while (dataTableReader.Read())
                            {
                                TestTelnetItem testTelnetItem = actionItem.AddVerifyTestTelnetItem();

                                string verifyType = dataTableReader.GetString(3).ToString();
                                if (testTelnetItem.TelnetVerifyTypeList.Contains(verifyType, StringComparer.CurrentCultureIgnoreCase))
                                    testTelnetItem.TelnetVerifyTypeSelected = verifyType;

                                testTelnetItem.TelnetFailureText = dataTableReader.GetString(4).ToString();

                                if (testTelnetItem.TelnetVerifyTypeSelected == "Compare Values" && dataTableReader[5] != System.DBNull.Value)
                                    testTelnetItem.KeywordTypeVerify = dataTableReader.GetString(5).ToString();
                            }
                        }
                    }

                    if (String.Equals(actionItem.VerificationSelected, "USB Verification", StringComparison.CurrentCultureIgnoreCase))
                    {
                        query = "select * from UsbVerify where TCID ='" + sourceTestCaseItem.TestCaseID + "' and ActionID = '" + actionItem.TestActionID + "'";
                        dataTable = new DataTable();
                        dataTableReader = null;
                        dataAdapter = new SqlDataAdapter(query, connect);
                        dataAdapter.Fill(dataTable);
                        dataTableReader = dataTable.CreateDataReader();
                        if (dataTableReader.HasRows)
                        {
                            while (dataTableReader.Read())
                            {
                                TestUsbAudioBridging testUsbItem = actionItem.AddVerifyTestUsbItem();

                                if (dataTableReader[3] != System.DBNull.Value)
                                {
                                    testUsbItem.UsbAudioBridgeTypeSelectedItem = dataTableReader.GetString(3);
                                }
                                if (dataTableReader[4] != System.DBNull.Value)
                                {
                                    testUsbItem.UsbAudioTypeSelectedItem = dataTableReader.GetString(4);
                                }
                                if (dataTableReader[5] != System.DBNull.Value)
                                {
                                    testUsbItem.UsbAudioDeviceSelectedItem = dataTableReader.GetString(5);
                                }
                                if (dataTableReader[6] != System.DBNull.Value)
                                {
                                    testUsbItem.UsbDefaultDeviceOptionSelectedItem = dataTableReader.GetString(6);
                                }
                            }
                        }
                    }
                    if (String.Equals(actionItem.VerificationSelected, "LUA Text Verification", StringComparison.CurrentCultureIgnoreCase))
                    {
                        actionItem.AddVerifyTestLuaItem();
                    }

                    if (String.Equals(actionItem.VerificationSelected, "Log Verification", StringComparison.CurrentCultureIgnoreCase))
                    {
                        query = "select * from logverification where TCID ='" + sourceTestCaseItem.TestCaseID + "' and ActionID = '" + actionItem.TestActionID + "'";
                        dataTable = new DataTable();
                        dataTableReader = null;
                        dataAdapter = new SqlDataAdapter(query, connect);
                        dataAdapter.Fill(dataTable);
                        dataTableReader = dataTable.CreateDataReader();
                        if (dataTableReader.HasRows)
                        {
                            while (dataTableReader.Read())
                            {
                                TestLogItem testLogItem = actionItem.AddVerifyTestLogItem();
                                /// ilog
                                if (String.Equals(dataTableReader.GetString(3).ToString(), "True", StringComparison.CurrentCultureIgnoreCase))
                                    testLogItem.iLogIsChecked = true;
                                else
                                    testLogItem.iLogIsChecked = false;

                                if (testLogItem.iLogIsChecked)
                                {
                                    testLogItem.iLog_combobox_enable = true;
                                    testLogItem.iLog_selected_item = dataTableReader.GetString(4).ToString();
                                    testLogItem.ilogtext = dataTableReader.GetString(5).ToString();
                                }
                                ///kernellog

                                if (String.Equals(dataTableReader.GetString(6).ToString(), "True", StringComparison.CurrentCultureIgnoreCase))
                                    testLogItem.KernelLogIsChecked = true;
                                else
                                    testLogItem.KernelLogIsChecked = false;

                                if (testLogItem.KernelLogIsChecked)
                                {
                                    testLogItem.kernelLog_combobox_enable = true;
                                    testLogItem.kernalLog_selected_item = dataTableReader.GetString(7).ToString();
                                    testLogItem.kernallogtext = dataTableReader.GetString(8).ToString();
                                }
                                ///eventlog
                                if (String.Equals(dataTableReader.GetString(9).ToString(), "True", StringComparison.CurrentCultureIgnoreCase))
                                    testLogItem.EventLogIsChecked = true;

                                else
                                    testLogItem.EventLogIsChecked = false;
                                if (testLogItem.EventLogIsChecked)
                                    testLogItem.eventlogtext = dataTableReader.GetString(10).ToString();
                                /// configuratorlog
                                if (String.Equals(dataTableReader.GetString(11).ToString(), "True", StringComparison.CurrentCultureIgnoreCase))
                                    testLogItem.ConfiguratorIsChecked = true;

                                else
                                    testLogItem.ConfiguratorIsChecked = false;
                                if (testLogItem.ConfiguratorIsChecked)
                                    testLogItem.configuratorlogtext = dataTableReader.GetString(12).ToString();

                                /// siplog
                                if (String.Equals(dataTableReader.GetString(13).ToString(), "True", StringComparison.CurrentCultureIgnoreCase))
                                    testLogItem.SIPLogIsChecked = true;

                                else
                                    testLogItem.SIPLogIsChecked = false;
                                if (testLogItem.SIPLogIsChecked)
                                    testLogItem.siplogtext = dataTableReader.GetString(14).ToString();


                                /// qsysapplog

                                if (String.Equals(dataTableReader.GetString(15).ToString(), "True", StringComparison.CurrentCultureIgnoreCase))
                                    testLogItem.QsysAppLogIsChecked = true;

                                else
                                    testLogItem.QsysAppLogIsChecked = false;
                                if (testLogItem.QsysAppLogIsChecked)
                                    testLogItem.qsysapplogtext = dataTableReader.GetString(16).ToString();


                                /// ucilog

                                if (String.Equals(dataTableReader.GetString(17).ToString(), "True", StringComparison.CurrentCultureIgnoreCase))
                                    testLogItem.UCIViewerLogIsChecked = true;

                                else
                                    testLogItem.UCIViewerLogIsChecked = false;
                                if (testLogItem.UCIViewerLogIsChecked)
                                    testLogItem.UCIlogtext = dataTableReader.GetString(18).ToString();


                                /// softphonelog

                                if (String.Equals(dataTableReader.GetString(19).ToString(), "True", StringComparison.CurrentCultureIgnoreCase))
                                    testLogItem.SoftPhoneLogIsChecked = true;

                                else
                                    testLogItem.SoftPhoneLogIsChecked = false;
                                if (testLogItem.SoftPhoneLogIsChecked)
                                    testLogItem.softphonelogtext = dataTableReader.GetString(20).ToString();

                                /// windowseventlog
                                if (String.Equals(dataTableReader.GetString(21).ToString(), "True", StringComparison.CurrentCultureIgnoreCase))
                                    testLogItem.WindowsEventLogsIsChecked = true;

                                else
                                    testLogItem.WindowsEventLogsIsChecked = false;
                                if (testLogItem.WindowsEventLogsIsChecked)
                                    testLogItem.windowseventlogtext = dataTableReader.GetString(22).ToString();


                                //////Pcap
                                //string logVerificationID = dataTableReader.GetValue(0).ToString();
                                string tcID = dataTableReader.GetValue(1).ToString();
                                string actionID = dataTableReader.GetValue(2).ToString();

                                query = "select * from PcapVerification where TCID ='" + sourceTestCaseItem.TestCaseID + "' and ActionID = '" + actionItem.TestActionID + "'";
                                dataTable = new DataTable();
                                dataTableReader = null;
                                dataAdapter = new SqlDataAdapter(query, connect);
                                dataAdapter.Fill(dataTable);
                                dataTableReader = dataTable.CreateDataReader();
                                if (dataTableReader.HasRows)
                                {
                                    dataTableReader.Read();

                                    if (dataTableReader.GetString(3) != null)
                                        testLogItem.PcaplogDelaySetting = dataTableReader.GetString(3).ToString();

                                    if (dataTableReader.GetString(4) != null)
                                        testLogItem.PcapDelayUnitSelected = dataTableReader.GetString(4).ToString();

                                    Loop1:
                                    PcapItem pcapItem = new PcapItem();
                                    pcapItem.ParentTestLogItem = testLogItem;

                                    if (dataTableReader.GetString(5) != null)
                                    {
                                        string protocolName_txtbox_Value = dataTableReader.GetString(5).ToString().Trim();

                                        string[] splitvalues = protocolName_txtbox_Value.Split(';'); 
                                        pcapItem.PcapProtocolName = splitvalues[0];

                                        if(splitvalues.Count() > 1)
                                        {
                                            testLogItem.PcapSelectLanComboSelecteditem = splitvalues[1];
                                            testLogItem.PcapSelectFilterComboSelecteditem = splitvalues[2];
                                            testLogItem.PcapFilterByIP = splitvalues[3];
                                            testLogItem.PcapNotFilterByIP = Convert.ToBoolean(splitvalues[4]);
                                        }
                                    }

                                    if (dataTableReader.GetString(6) != null)
                                    {
                                        pcapItem.PcapFieldText = dataTableReader.GetString(6).ToString().Trim();
                                    }

                                    testLogItem.SetTestPcapList.Add(pcapItem);

                                    while (dataTableReader.Read())
                                    {
                                        goto Loop1;
                                    }

                                    testLogItem.PcapLogIsChecked = true;
                                }
                                else
                                    testLogItem.PcapLogIsChecked = false;                                
                            }
                        }
                        //actionItem.AddVerifyTestLogItem();
                    }

                    if (String.Equals(actionItem.VerificationSelected, "Responsalyzer", StringComparison.CurrentCultureIgnoreCase))
                    {
                        query = "select * from Responsalyzer where TCID ='" + sourceTestCaseItem.TestCaseID + "' and ActionID = '" + actionItem.TestActionID + "'";
                        dataTable = new DataTable();
                        dataTableReader = null;
                        dataAdapter = new SqlDataAdapter(query, connect);
                        dataAdapter.Fill(dataTable);
                        dataTableReader = dataTable.CreateDataReader();
                        if (dataTableReader.HasRows)
                        {
                            while (dataTableReader.Read())
                            {
                                TestResponsalyzerItem testResponsalyzerItem = actionItem.AddTestResponsalyzerItem();
                                testResponsalyzerItem.TestResponsalyzerNameSelectedItem = dataTableReader.GetValue(3).ToString();
                                testResponsalyzerItem.TestResponsalyzerTypeSelectedItem = dataTableReader.GetValue(4).ToString();
                                testResponsalyzerItem.TestResponsalyzerVerificationFile = dataTableReader.GetValue(5).ToString();
                            }
                        }
                        //actionItem.AddVerifyTestLogItem();
                    }

                    if (String.Equals(actionItem.VerificationSelected, "Audio Precision Verification", StringComparison.CurrentCultureIgnoreCase))
                    {
                        //////
                        query = "Select * from APSettings where TCID = " + sourceTestCaseItem.TestCaseID + " and ActionID =" + actionItem.TestActionID + "";
                        dataTable = new DataTable();
                        dataTableReader = null;
                        dataAdapter = new SqlDataAdapter(query, connect);
                        dataAdapter.Fill(dataTable);
                        dataTableReader = dataTable.CreateDataReader();
                        if (dataTableReader.HasRows)
                        {
                            while (dataTableReader.Read())
                            {
                                TestApxItem testApxItem = actionItem.AddVerifyTestApxItem();
                                testApxItem.ApSettingsVisibility = Visibility.Visible;
                                testApxItem.ApVerifyVisibility = Visibility.Visible;

                                string modeType = dataTableReader.GetValue(3).ToString();
                                testApxItem.APxSettingsList[0].cmbTypeOfMode = modeType;

                                if (modeType == "BenchMode")
                                {
                                    testApxItem.APxSettingsList[0].ChkBenchGenON = Convert.ToBoolean(dataTableReader.GetValue(4));
                                    if (!testApxItem.APxSettingsList[0].cmb_BenchWaveformList.Contains(dataTableReader.GetValue(5).ToString()))
                                        testApxItem.APxSettingsList[0].cmb_BenchWaveformList.Add(dataTableReader.GetValue(5).ToString());

                                    testApxItem.APxSettingsList[0].cmb_BenchWaveform = dataTableReader.GetValue(5).ToString();
                                    testApxItem.APxSettingsList[0].BenchSetupCount = Convert.ToInt32(dataTableReader.GetValue(36));
                                    testApxItem.APxSettingsList[0].chkBx_BenchLevelTrackCh = Convert.ToBoolean(dataTableReader.GetValue(6));
                                    testApxItem.APxSettingsList[0].txt_BenchLevelCh1 = dataTableReader.GetValue(7).ToString();
                                    testApxItem.APxSettingsList[0].txt_BenchDcOffsetCh1 = dataTableReader.GetValue(8).ToString();
                                    testApxItem.APxSettingsList[0].txt_BenchLevelCh2 = dataTableReader.GetValue(9).ToString();
                                    testApxItem.APxSettingsList[0].txt_BenchDcOffsetCh2 = dataTableReader.GetValue(10).ToString();
                                    testApxItem.APxSettingsList[0].txt_BenchLevelCh3 = dataTableReader.GetValue(11).ToString();
                                    testApxItem.APxSettingsList[0].txt_BenchDcOffsetCh3 = dataTableReader.GetValue(12).ToString();
                                    testApxItem.APxSettingsList[0].txt_BenchLevelCh4 = dataTableReader.GetValue(13).ToString();
                                    testApxItem.APxSettingsList[0].txt_BenchDcOffsetCh4 = dataTableReader.GetValue(14).ToString();
                                    testApxItem.APxSettingsList[0].txt_BenchLevelCh5 = dataTableReader.GetValue(15).ToString();
                                    testApxItem.APxSettingsList[0].txt_BenchDcOffsetCh5 = dataTableReader.GetValue(16).ToString();
                                    testApxItem.APxSettingsList[0].txt_BenchLevelCh6 = dataTableReader.GetValue(17).ToString();
                                    testApxItem.APxSettingsList[0].txt_BenchDcOffsetCh6 = dataTableReader.GetValue(18).ToString();
                                    testApxItem.APxSettingsList[0].txt_BenchLevelCh7 = dataTableReader.GetValue(19).ToString();
                                    testApxItem.APxSettingsList[0].txt_BenchDcOffsetCh7 = dataTableReader.GetValue(20).ToString();
                                    testApxItem.APxSettingsList[0].txt_BenchLevelCh8 = dataTableReader.GetValue(21).ToString();
                                    testApxItem.APxSettingsList[0].txt_BenchDcOffsetCh8 = dataTableReader.GetValue(22).ToString();
                                    testApxItem.APxSettingsList[0].txt_BenchfrequencyA = dataTableReader.GetValue(23).ToString();
                                    testApxItem.APxSettingsList[0].txt_BenchfrequencyB = dataTableReader.GetValue(24).ToString();

                                    testApxItem.APxSettingsList[0].ChkBenchCh1Enable = Convert.ToBoolean(dataTableReader.GetValue(25));
                                    testApxItem.APxSettingsList[0].ChkBenchCh2Enable = Convert.ToBoolean(dataTableReader.GetValue(26));
                                    testApxItem.APxSettingsList[0].ChkBenchCh3Enable = Convert.ToBoolean(dataTableReader.GetValue(27));
                                    testApxItem.APxSettingsList[0].ChkBenchCh4Enable = Convert.ToBoolean(dataTableReader.GetValue(28));
                                    testApxItem.APxSettingsList[0].ChkBenchCh5Enable = Convert.ToBoolean(dataTableReader.GetValue(29));
                                    testApxItem.APxSettingsList[0].ChkBenchCh6Enable = Convert.ToBoolean(dataTableReader.GetValue(30));
                                    testApxItem.APxSettingsList[0].ChkBenchCh7Enable = Convert.ToBoolean(dataTableReader.GetValue(31));
                                    testApxItem.APxSettingsList[0].ChkBenchCh8Enable = Convert.ToBoolean(dataTableReader.GetValue(32));


                                    SetInitialSeqModeSettings(sourceTestCaseItem.TestCaseID, actionItem.TestActionID, testApxItem);
                                }
                                else if (modeType == "SequenceMode")
                                {
                                    testApxItem.APxSettingsList[0].ChkSeqGenON = Convert.ToBoolean(dataTableReader.GetValue(4));
                                    if (!testApxItem.APxSettingsList[0].cmb_SeqWaveformList.Contains(dataTableReader.GetValue(5).ToString()))
                                        testApxItem.APxSettingsList[0].cmb_SeqWaveformList.Add(dataTableReader.GetValue(5).ToString());

                                    testApxItem.APxSettingsList[0].cmbSeqWaveForm = dataTableReader.GetValue(5).ToString();
                                    testApxItem.APxSettingsList[0].SeqSetupCount = Convert.ToInt32(dataTableReader.GetValue(35));
                                    testApxItem.APxSettingsList[0].ChkSeqTrackCh = Convert.ToBoolean(dataTableReader.GetValue(6));
                                    testApxItem.APxSettingsList[0].TxtSeqLevelCh1 = dataTableReader.GetValue(7).ToString();
                                    testApxItem.APxSettingsList[0].TxtSeqLevelDcCh1 = dataTableReader.GetValue(8).ToString();
                                    testApxItem.APxSettingsList[0].TxtSeqLevelCh2 = dataTableReader.GetValue(9).ToString();
                                    testApxItem.APxSettingsList[0].TxtSeqLevelDcCh2 = dataTableReader.GetValue(10).ToString();
                                    testApxItem.APxSettingsList[0].TxtSeqLevelCh3 = dataTableReader.GetValue(11).ToString();
                                    testApxItem.APxSettingsList[0].TxtSeqLevelDcCh3 = dataTableReader.GetValue(12).ToString();
                                    testApxItem.APxSettingsList[0].TxtSeqLevelCh4 = dataTableReader.GetValue(13).ToString();
                                    testApxItem.APxSettingsList[0].TxtSeqLevelDcCh4 = dataTableReader.GetValue(14).ToString();
                                    testApxItem.APxSettingsList[0].TxtSeqLevelCh5 = dataTableReader.GetValue(15).ToString();
                                    testApxItem.APxSettingsList[0].TxtSeqLevelDcCh5 = dataTableReader.GetValue(16).ToString();
                                    testApxItem.APxSettingsList[0].TxtSeqLevelCh6 = dataTableReader.GetValue(17).ToString();
                                    testApxItem.APxSettingsList[0].TxtSeqLevelDcCh6 = dataTableReader.GetValue(18).ToString();
                                    testApxItem.APxSettingsList[0].TxtSeqLevelCh7 = dataTableReader.GetValue(19).ToString();
                                    testApxItem.APxSettingsList[0].TxtSeqLevelDcCh7 = dataTableReader.GetValue(20).ToString();
                                    testApxItem.APxSettingsList[0].TxtSeqLevelCh8 = dataTableReader.GetValue(21).ToString();
                                    testApxItem.APxSettingsList[0].TxtSeqLevelDcCh8 = dataTableReader.GetValue(22).ToString();
                                    testApxItem.APxSettingsList[0].TxtSeqFreqA = dataTableReader.GetValue(23).ToString();
                                    testApxItem.APxSettingsList[0].TxtSeqFreqB = dataTableReader.GetValue(24).ToString();
                                    testApxItem.APxSettingsList[0].ChkSeqCh1Enable = Convert.ToBoolean(dataTableReader.GetValue(25));
                                    testApxItem.APxSettingsList[0].ChkSeqCh2Enable = Convert.ToBoolean(dataTableReader.GetValue(26));
                                    testApxItem.APxSettingsList[0].ChkSeqCh3Enable = Convert.ToBoolean(dataTableReader.GetValue(27));
                                    testApxItem.APxSettingsList[0].ChkSeqCh4Enable = Convert.ToBoolean(dataTableReader.GetValue(28));
                                    testApxItem.APxSettingsList[0].ChkSeqCh5Enable = Convert.ToBoolean(dataTableReader.GetValue(29));
                                    testApxItem.APxSettingsList[0].ChkSeqCh6Enable = Convert.ToBoolean(dataTableReader.GetValue(30));
                                    testApxItem.APxSettingsList[0].ChkSeqCh7Enable = Convert.ToBoolean(dataTableReader.GetValue(31));
                                    testApxItem.APxSettingsList[0].ChkSeqCh8Enable = Convert.ToBoolean(dataTableReader.GetValue(32));
                                    testApxItem.APxSettingsList[0].cmbSeqTestChannel = dataTableReader.GetValue(33).ToString();
                                    testApxItem.APxSettingsList[0].TxtSeqDelay = dataTableReader.GetValue(34).ToString();

                                    SetInitialBenchModeSettings(sourceTestCaseItem.TestCaseID, actionItem.TestActionID, testApxItem);
                                }

                                string verifyType = string.Empty;
                                string verificationPath = string.Empty;
                                query = "Select VerificationType, APxPath from APVerification where TCID= " + sourceTestCaseItem.TestCaseID + " and ActionID =" + actionItem.TestActionID + "";
                                dataTable = new DataTable();
                                dataTableReader = null;
                                dataAdapter = new SqlDataAdapter(query, connect);
                                dataAdapter.Fill(dataTable);
                                dataTableReader = dataTable.CreateDataReader();
                                if (dataTableReader.HasRows)
                                {
                                    while (dataTableReader.Read())
                                    {
                                        verifyType = dataTableReader.GetString(0).ToString();
                                        testApxItem.APxLocationTimeStamp = verificationPath = dataTableReader.GetString(1).ToString();
                                        //string val = Path.GetFileNameWithoutExtension(verificationPath);
                                        //testApxItem.APxLocationTimeStamp = val + "_" + sourceTestCaseItem.TestCaseID + "_" + actionItem.TestActionID + ".approjx";
                                        testApxItem.cmbTypeOfVerfication = verifyType;
                                    }
                                }

                                if (verifyType == "Level and Gain")
                                {
                                    query = "Select * from LevelAndGain where TCID= " + sourceTestCaseItem.TestCaseID + " and ActionID =" + actionItem.TestActionID + "";
                                    dataTable = new DataTable();
                                    dataTableReader = null;
                                    dataAdapter = new SqlDataAdapter(query, connect);
                                    dataAdapter.Fill(dataTable);
                                    dataTableReader = dataTable.CreateDataReader();
                                    if (dataTableReader.HasRows)
                                    {
                                        while (dataTableReader.Read())
                                        {
                                            testApxItem.APxLevelAndGainList[0].ChkGainGenON = Convert.ToBoolean(dataTableReader.GetValue(3));
                                            if (!testApxItem.APxLevelAndGainList[0].cmbGainWaveformList.Contains(dataTableReader.GetValue(4).ToString()))
                                                testApxItem.APxLevelAndGainList[0].cmbGainWaveformList.Add(dataTableReader.GetValue(4).ToString());

                                            testApxItem.APxLevelAndGainList[0].cmb_GainWaveform = dataTableReader.GetValue(4).ToString();
                                            testApxItem.APxLevelAndGainList[0].SeqGainSetupCount = Convert.ToInt32(dataTableReader.GetValue(39));

                                            if (dataTableReader.GetValue(57) != null & dataTableReader.GetValue(57).ToString() != string.Empty)
                                                testApxItem.APxLevelAndGainList[0].GainInputChCount = Convert.ToInt32(dataTableReader.GetValue(57));

                                            testApxItem.APxLevelAndGainList[0].chkBx_GainLevelTrackCh = Convert.ToBoolean(dataTableReader.GetValue(5));
                                            testApxItem.APxLevelAndGainList[0].txt_GainCh1Level = dataTableReader.GetValue(6).ToString();
                                            testApxItem.APxLevelAndGainList[0].txt_GainDcCh1Offset = dataTableReader.GetValue(7).ToString();
                                            testApxItem.APxLevelAndGainList[0].txt_GainCh2Level = dataTableReader.GetValue(8).ToString();
                                            testApxItem.APxLevelAndGainList[0].txt_GainDcCh2Offset = dataTableReader.GetValue(9).ToString();
                                            testApxItem.APxLevelAndGainList[0].txt_GainCh3Level = dataTableReader.GetValue(10).ToString();
                                            testApxItem.APxLevelAndGainList[0].txt_GainDcCh3Offset = dataTableReader.GetValue(11).ToString();
                                            testApxItem.APxLevelAndGainList[0].txt_GainCh4Level = dataTableReader.GetValue(12).ToString();
                                            testApxItem.APxLevelAndGainList[0].txt_GainDcCh4Offset = dataTableReader.GetValue(13).ToString();
                                            testApxItem.APxLevelAndGainList[0].txt_GainCh5Level = dataTableReader.GetValue(14).ToString();
                                            testApxItem.APxLevelAndGainList[0].txt_GainDcCh5Offset = dataTableReader.GetValue(15).ToString();
                                            testApxItem.APxLevelAndGainList[0].txt_GainCh6Level = dataTableReader.GetValue(16).ToString();
                                            testApxItem.APxLevelAndGainList[0].txt_GainDcCh6Offset = dataTableReader.GetValue(17).ToString();
                                            testApxItem.APxLevelAndGainList[0].txt_GainCh7Level = dataTableReader.GetValue(18).ToString();
                                            testApxItem.APxLevelAndGainList[0].txt_GainDcCh7Offset = dataTableReader.GetValue(19).ToString();
                                            testApxItem.APxLevelAndGainList[0].txt_GainCh8Level = dataTableReader.GetValue(20).ToString();
                                            testApxItem.APxLevelAndGainList[0].txt_GainDcCh8Offset = dataTableReader.GetValue(21).ToString();
                                            testApxItem.APxLevelAndGainList[0].txt_GainfrequencyA = dataTableReader.GetValue(22).ToString();
                                            testApxItem.APxLevelAndGainList[0].txt_GainfrequencyB = dataTableReader.GetValue(40).ToString();

                                            testApxItem.APxLevelAndGainList[0].btn_GainCh1 = Convert.ToBoolean(dataTableReader.GetValue(23));
                                            testApxItem.APxLevelAndGainList[0].btn_GainCh2 = Convert.ToBoolean(dataTableReader.GetValue(24));
                                            testApxItem.APxLevelAndGainList[0].btn_GainCh3 = Convert.ToBoolean(dataTableReader.GetValue(25));
                                            testApxItem.APxLevelAndGainList[0].btn_GainCh4 = Convert.ToBoolean(dataTableReader.GetValue(26));
                                            testApxItem.APxLevelAndGainList[0].btn_GainCh5 = Convert.ToBoolean(dataTableReader.GetValue(27));
                                            testApxItem.APxLevelAndGainList[0].btn_GainCh6 = Convert.ToBoolean(dataTableReader.GetValue(28));
                                            testApxItem.APxLevelAndGainList[0].btn_GainCh7 = Convert.ToBoolean(dataTableReader.GetValue(29));
                                            testApxItem.APxLevelAndGainList[0].btn_GainCh8 = Convert.ToBoolean(dataTableReader.GetValue(30));
                                            testApxItem.APxLevelAndGainList[0].TxtGainCh1 = dataTableReader.GetValue(31).ToString();
                                            testApxItem.APxLevelAndGainList[0].TxtGainCh2 = dataTableReader.GetValue(32).ToString();
                                            testApxItem.APxLevelAndGainList[0].TxtGainCh3 = dataTableReader.GetValue(33).ToString();
                                            testApxItem.APxLevelAndGainList[0].TxtGainCh4 = dataTableReader.GetValue(34).ToString();
                                            testApxItem.APxLevelAndGainList[0].TxtGainCh5 = dataTableReader.GetValue(35).ToString();
                                            testApxItem.APxLevelAndGainList[0].TxtGainCh6 = dataTableReader.GetValue(36).ToString();
                                            testApxItem.APxLevelAndGainList[0].TxtGainCh7 = dataTableReader.GetValue(37).ToString();
                                            testApxItem.APxLevelAndGainList[0].TxtGainCh8 = dataTableReader.GetValue(38).ToString();

                                            testApxItem.APxLevelAndGainList[0].TxtUpToleranceGainCh1 = dataTableReader.GetValue(41).ToString();
                                            testApxItem.APxLevelAndGainList[0].TxtUpToleranceGainCh2 = dataTableReader.GetValue(42).ToString();
                                            testApxItem.APxLevelAndGainList[0].TxtUpToleranceGainCh3 = dataTableReader.GetValue(43).ToString();
                                            testApxItem.APxLevelAndGainList[0].TxtUpToleranceGainCh4 = dataTableReader.GetValue(44).ToString();
                                            testApxItem.APxLevelAndGainList[0].TxtUpToleranceGainCh5 = dataTableReader.GetValue(45).ToString();
                                            testApxItem.APxLevelAndGainList[0].TxtUpToleranceGainCh6 = dataTableReader.GetValue(46).ToString();
                                            testApxItem.APxLevelAndGainList[0].TxtUpToleranceGainCh7 = dataTableReader.GetValue(47).ToString();
                                            testApxItem.APxLevelAndGainList[0].TxtUpToleranceGainCh8 = dataTableReader.GetValue(48).ToString();

                                            testApxItem.APxLevelAndGainList[0].TxtLowToleranceGainCh1 = dataTableReader.GetValue(49).ToString();
                                            testApxItem.APxLevelAndGainList[0].TxtLowToleranceGainCh2 = dataTableReader.GetValue(50).ToString();
                                            testApxItem.APxLevelAndGainList[0].TxtLowToleranceGainCh3 = dataTableReader.GetValue(51).ToString();
                                            testApxItem.APxLevelAndGainList[0].TxtLowToleranceGainCh4 = dataTableReader.GetValue(52).ToString();
                                            testApxItem.APxLevelAndGainList[0].TxtLowToleranceGainCh5 = dataTableReader.GetValue(53).ToString();
                                            testApxItem.APxLevelAndGainList[0].TxtLowToleranceGainCh6 = dataTableReader.GetValue(54).ToString();
                                            testApxItem.APxLevelAndGainList[0].TxtLowToleranceGainCh7 = dataTableReader.GetValue(55).ToString();
                                            testApxItem.APxLevelAndGainList[0].TxtLowToleranceGainCh8 = dataTableReader.GetValue(56).ToString();

                                        }
                                    }

                                    SetLevelAndGainSettings(sourceTestCaseItem.TestCaseID, actionItem.TestActionID, testApxItem);
                                    SetInitialInterChannelPhaseSettings(sourceTestCaseItem.TestCaseID, actionItem.TestActionID, testApxItem);
                                    SetInitialFreqResponseSettings(sourceTestCaseItem.TestCaseID, actionItem.TestActionID, testApxItem);
                                    SetInitialSteppedFreqSweepSettings(sourceTestCaseItem.TestCaseID, actionItem.TestActionID, testApxItem);
                                    SetInitialTHDNSettings(sourceTestCaseItem.TestCaseID, actionItem.TestActionID, testApxItem);
                                }
                                else if (verifyType == "Frequency sweep")
                                {
                                    query = "Select * from APFrequencyResponse where TCID= " + sourceTestCaseItem.TestCaseID + " and ActionID =" + actionItem.TestActionID + "";
                                    dataTable = new DataTable();
                                    dataTableReader = null;
                                    dataAdapter = new SqlDataAdapter(query, connect);
                                    dataAdapter.Fill(dataTable);
                                    dataTableReader = dataTable.CreateDataReader();
                                    if (dataTableReader.HasRows)
                                    {
                                        while (dataTableReader.Read())
                                        {
                                            testApxItem.APxFreqResponseList[0].StartGenON = Convert.ToBoolean(dataTableReader.GetValue(3));
                                            testApxItem.APxFreqResponseList[0].txtStartFreq = dataTableReader.GetValue(4).ToString();
                                            testApxItem.APxFreqResponseList[0].txtStopFreq = dataTableReader.GetValue(5).ToString();
                                            testApxItem.APxFreqResponseList[0].txtLevel = dataTableReader.GetValue(6).ToString();
                                            testApxItem.APxFreqResponseList[0].IsEnableCh1 = Convert.ToBoolean(dataTableReader.GetValue(7));
                                            testApxItem.APxFreqResponseList[0].IsEnableCh2 = Convert.ToBoolean(dataTableReader.GetValue(8));
                                            testApxItem.APxFreqResponseList[0].IsEnableCh3 = Convert.ToBoolean(dataTableReader.GetValue(9));
                                            testApxItem.APxFreqResponseList[0].IsEnableCh4 = Convert.ToBoolean(dataTableReader.GetValue(10));
                                            testApxItem.APxFreqResponseList[0].IsEnableCh5 = Convert.ToBoolean(dataTableReader.GetValue(11));
                                            testApxItem.APxFreqResponseList[0].IsEnableCh6 = Convert.ToBoolean(dataTableReader.GetValue(12));
                                            testApxItem.APxFreqResponseList[0].IsEnableCh7 = Convert.ToBoolean(dataTableReader.GetValue(13));
                                            testApxItem.APxFreqResponseList[0].IsEnableCh8 = Convert.ToBoolean(dataTableReader.GetValue(14));
                                            testApxItem.APxFreqResponseList[0].OutChannelCount = Convert.ToInt32(dataTableReader.GetValue(15));
                                            if (dataTableReader.GetValue(16) != null)
                                                testApxItem.APxFreqResponseList[0].txtFreqVerification = dataTableReader.GetValue(16).ToString();
                                            else
                                                testApxItem.APxFreqResponseList[0].txtFreqVerification = string.Empty;
                                        }
                                    }

                                    SetFreqResponseSettings(sourceTestCaseItem.TestCaseID, actionItem.TestActionID, testApxItem);
                                    SetInitialLevelAndGainSettings(sourceTestCaseItem.TestCaseID, actionItem.TestActionID, testApxItem);
                                    SetInitialInterChannelPhaseSettings(sourceTestCaseItem.TestCaseID, actionItem.TestActionID, testApxItem);
                                    SetInitialSteppedFreqSweepSettings(sourceTestCaseItem.TestCaseID, actionItem.TestActionID, testApxItem);
                                    SetInitialTHDNSettings(sourceTestCaseItem.TestCaseID, actionItem.TestActionID, testApxItem);
                                }
                                else if (verifyType == "Phase")
                                {
                                    query = "Select * from APPhaseSettings where TCID= " + sourceTestCaseItem.TestCaseID + " and ActionID =" + actionItem.TestActionID + "";
                                    dataTable = new DataTable();
                                    dataTableReader = null;
                                    dataAdapter = new SqlDataAdapter(query, connect);
                                    dataAdapter.Fill(dataTable);
                                    dataTableReader = dataTable.CreateDataReader();
                                    if (dataTableReader.HasRows)
                                    {
                                        while (dataTableReader.Read())
                                        {
                                            testApxItem.APxInterChPhaseList[0].OutChannelCount = Convert.ToInt32(dataTableReader.GetValue(24));
                                            testApxItem.APxInterChPhaseList[0].InChannelCount = Convert.ToInt32(dataTableReader.GetValue(25));
                                            testApxItem.APxInterChPhaseList[0].ChkGenON = Convert.ToBoolean(dataTableReader.GetValue(3));
                                            testApxItem.APxInterChPhaseList[0].SteppedTrackChannel = Convert.ToBoolean(dataTableReader.GetValue(4));
                                            testApxItem.APxInterChPhaseList[0].LevelCh1 = dataTableReader.GetValue(5).ToString();
                                            testApxItem.APxInterChPhaseList[0].LevelCh2 = dataTableReader.GetValue(6).ToString();
                                            testApxItem.APxInterChPhaseList[0].LevelCh3 = dataTableReader.GetValue(7).ToString();
                                            testApxItem.APxInterChPhaseList[0].LevelCh4 = dataTableReader.GetValue(8).ToString();
                                            testApxItem.APxInterChPhaseList[0].LevelCh5 = dataTableReader.GetValue(9).ToString();
                                            testApxItem.APxInterChPhaseList[0].LevelCh6 = dataTableReader.GetValue(10).ToString();
                                            testApxItem.APxInterChPhaseList[0].LevelCh7 = dataTableReader.GetValue(11).ToString();
                                            testApxItem.APxInterChPhaseList[0].LevelCh8 = dataTableReader.GetValue(12).ToString();

                                            testApxItem.APxInterChPhaseList[0].TxtFreqA = dataTableReader.GetValue(13).ToString();

                                            testApxItem.APxInterChPhaseList[0].Isch1Enable = Convert.ToBoolean(dataTableReader.GetValue(14));
                                            testApxItem.APxInterChPhaseList[0].Isch2Enable = Convert.ToBoolean(dataTableReader.GetValue(15));
                                            testApxItem.APxInterChPhaseList[0].Isch3Enable = Convert.ToBoolean(dataTableReader.GetValue(16));
                                            testApxItem.APxInterChPhaseList[0].Isch4Enable = Convert.ToBoolean(dataTableReader.GetValue(17));
                                            testApxItem.APxInterChPhaseList[0].Isch5Enable = Convert.ToBoolean(dataTableReader.GetValue(18));
                                            testApxItem.APxInterChPhaseList[0].Isch6Enable = Convert.ToBoolean(dataTableReader.GetValue(19));
                                            testApxItem.APxInterChPhaseList[0].Isch7Enable = Convert.ToBoolean(dataTableReader.GetValue(20));
                                            testApxItem.APxInterChPhaseList[0].Isch8Enable = Convert.ToBoolean(dataTableReader.GetValue(21));

                                            testApxItem.APxInterChPhaseList[0].CmbRefChannelSelected = dataTableReader.GetValue(22).ToString();
                                            testApxItem.APxInterChPhaseList[0].MeterRangeSelected = dataTableReader.GetValue(23).ToString();

                                            if (dataTableReader.GetValue(26) != null)
                                                testApxItem.APxInterChPhaseList[0].txtPhaseVerification = dataTableReader.GetValue(26).ToString();
                                            else
                                                testApxItem.APxInterChPhaseList[0].txtPhaseVerification = string.Empty;
                                        }
                                    }

                                    SetInterChannelPhaseSettings(sourceTestCaseItem.TestCaseID, actionItem.TestActionID, testApxItem);
                                    SetInitialLevelAndGainSettings(sourceTestCaseItem.TestCaseID, actionItem.TestActionID, testApxItem);
                                    SetInitialFreqResponseSettings(sourceTestCaseItem.TestCaseID, actionItem.TestActionID, testApxItem);
                                    SetInitialSteppedFreqSweepSettings(sourceTestCaseItem.TestCaseID, actionItem.TestActionID, testApxItem);
                                    SetInitialTHDNSettings(sourceTestCaseItem.TestCaseID, actionItem.TestActionID, testApxItem);
                                }
                                else if (verifyType == "Stepped Frequency Sweep")
                                {
                                    query = "Select * from APSteppedFreqSweepSettings where TCID= " + sourceTestCaseItem.TestCaseID + " and ActionID =" + actionItem.TestActionID + "";
                                    dataTable = new DataTable();
                                    dataTableReader = null;
                                    dataAdapter = new SqlDataAdapter(query, connect);
                                    dataAdapter.Fill(dataTable);
                                    dataTableReader = dataTable.CreateDataReader();
                                    if (dataTableReader.HasRows)
                                    {
                                        while (dataTableReader.Read())
                                        {
                                            testApxItem.APxSteppedFreqSweepList[0].OutChannelCount = Convert.ToInt32(dataTableReader.GetValue(18));
                                            testApxItem.APxSteppedFreqSweepList[0].InChCount = Convert.ToInt32(dataTableReader.GetValue(19));
                                            testApxItem.APxSteppedFreqSweepList[0].ChkGenON = Convert.ToBoolean(dataTableReader.GetValue(3));
                                            testApxItem.APxSteppedFreqSweepList[0].StartFrequency = dataTableReader.GetValue(4).ToString();
                                            testApxItem.APxSteppedFreqSweepList[0].StopFrequency = dataTableReader.GetValue(5).ToString();
                                            testApxItem.APxSteppedFreqSweepList[0].SelectedSweep = dataTableReader.GetValue(6).ToString();
                                            testApxItem.APxSteppedFreqSweepList[0].Steppedpoints = Convert.ToInt32(dataTableReader.GetValue(7));
                                            testApxItem.APxSteppedFreqSweepList[0].SteppedLevel = dataTableReader.GetValue(8).ToString();
                                            testApxItem.APxSteppedFreqSweepList[0].IsEnableCh1 = Convert.ToBoolean(dataTableReader.GetValue(9));
                                            testApxItem.APxSteppedFreqSweepList[0].IsEnableCh2 = Convert.ToBoolean(dataTableReader.GetValue(10));
                                            testApxItem.APxSteppedFreqSweepList[0].IsEnableCh3 = Convert.ToBoolean(dataTableReader.GetValue(11));
                                            testApxItem.APxSteppedFreqSweepList[0].IsEnableCh4 = Convert.ToBoolean(dataTableReader.GetValue(12));
                                            testApxItem.APxSteppedFreqSweepList[0].IsEnableCh5 = Convert.ToBoolean(dataTableReader.GetValue(13));
                                            testApxItem.APxSteppedFreqSweepList[0].IsEnableCh6 = Convert.ToBoolean(dataTableReader.GetValue(14));
                                            testApxItem.APxSteppedFreqSweepList[0].IsEnableCh7 = Convert.ToBoolean(dataTableReader.GetValue(15));
                                            testApxItem.APxSteppedFreqSweepList[0].IsEnableCh8 = Convert.ToBoolean(dataTableReader.GetValue(16));
                                            testApxItem.APxSteppedFreqSweepList[0].cmbPhaseRefChannel = dataTableReader.GetValue(17).ToString();
                                            if (dataTableReader.GetValue(18) != null)
												testApxItem.APxSteppedFreqSweepList[0].txtSteppedFreqVerification = dataTableReader.GetValue(20).ToString();
											else
												testApxItem.APxSteppedFreqSweepList[0].txtSteppedFreqVerification = string.Empty;
                                        }
                                    }

                                    SetSteppedFreqSweepSettings(sourceTestCaseItem.TestCaseID, actionItem.TestActionID, testApxItem);
                                    SetInitialLevelAndGainSettings(sourceTestCaseItem.TestCaseID, actionItem.TestActionID, testApxItem);
                                    SetInitialFreqResponseSettings(sourceTestCaseItem.TestCaseID, actionItem.TestActionID, testApxItem);
                                    SetInitialInterChannelPhaseSettings(sourceTestCaseItem.TestCaseID, actionItem.TestActionID, testApxItem);
                                    SetInitialTHDNSettings(sourceTestCaseItem.TestCaseID, actionItem.TestActionID, testApxItem);
                                }
                                else if (verifyType == "THD+N")
                                {
                                    query = "Select * from APTHDNSettings where TCID= " + sourceTestCaseItem.TestCaseID + " and ActionID =" + actionItem.TestActionID + "";
                                    dataTable = new DataTable();
                                    dataTableReader = null;
                                    dataAdapter = new SqlDataAdapter(query, connect);
                                    dataAdapter.Fill(dataTable);
                                    dataTableReader = dataTable.CreateDataReader();
                                    if (dataTableReader.HasRows)
                                    {
                                        while (dataTableReader.Read())
                                        {
                                            testApxItem.APxTHDNList[0].OutChannelCount = Convert.ToInt32(dataTableReader.GetValue(26));
                                            testApxItem.APxTHDNList[0].ChkGenON = Convert.ToBoolean(dataTableReader.GetValue(3));
                                            testApxItem.APxTHDNList[0].chkBx_ThdnLevelTrackCh = Convert.ToBoolean(dataTableReader.GetValue(4));
                                            testApxItem.APxTHDNList[0].txtCh1Content = dataTableReader.GetValue(5).ToString();
                                            testApxItem.APxTHDNList[0].txtCh2Content = dataTableReader.GetValue(6).ToString();
                                            testApxItem.APxTHDNList[0].txtCh3Content = dataTableReader.GetValue(7).ToString();
                                            testApxItem.APxTHDNList[0].txtCh4Content = dataTableReader.GetValue(8).ToString();
                                            testApxItem.APxTHDNList[0].txtCh5Content = dataTableReader.GetValue(9).ToString();
                                            testApxItem.APxTHDNList[0].txtCh6Content = dataTableReader.GetValue(10).ToString();
                                            testApxItem.APxTHDNList[0].txtCh7Content = dataTableReader.GetValue(11).ToString();
                                            testApxItem.APxTHDNList[0].txtCh8Content = dataTableReader.GetValue(12).ToString();
                                            testApxItem.APxTHDNList[0].txt_THDfrequency = dataTableReader.GetValue(13).ToString();

                                            testApxItem.APxTHDNList[0].btn_THDCh1 = Convert.ToBoolean(dataTableReader.GetValue(14));
                                            testApxItem.APxTHDNList[0].btn_THDCh2 = Convert.ToBoolean(dataTableReader.GetValue(15));
                                            testApxItem.APxTHDNList[0].btn_THDCh3 = Convert.ToBoolean(dataTableReader.GetValue(16));
                                            testApxItem.APxTHDNList[0].btn_THDCh4 = Convert.ToBoolean(dataTableReader.GetValue(17));
                                            testApxItem.APxTHDNList[0].btn_THDCh5 = Convert.ToBoolean(dataTableReader.GetValue(18));
                                            testApxItem.APxTHDNList[0].btn_THDCh6 = Convert.ToBoolean(dataTableReader.GetValue(19));
                                            testApxItem.APxTHDNList[0].btn_THDCh7 = Convert.ToBoolean(dataTableReader.GetValue(20));
                                            testApxItem.APxTHDNList[0].btn_THDCh8 = Convert.ToBoolean(dataTableReader.GetValue(21));

                                            testApxItem.APxTHDNList[0].cmb_THDLowPassFilter = dataTableReader.GetValue(22).ToString();
                                            testApxItem.APxTHDNList[0].cmb_THDHighPassFilter = dataTableReader.GetValue(23).ToString();
                                            testApxItem.APxTHDNList[0].cmb_THDWeighting = dataTableReader.GetValue(24).ToString();
                                            testApxItem.APxTHDNList[0].cmb_THDTuningMode = dataTableReader.GetValue(25).ToString();
                                            testApxItem.APxTHDNList[0].TxtLowpass = dataTableReader.GetValue(27).ToString();
                                            testApxItem.APxTHDNList[0].TxtHighpass = dataTableReader.GetValue(28).ToString();
                                            testApxItem.APxTHDNList[0].txtFilterFrequency = dataTableReader.GetValue(29).ToString();

                                            if (dataTableReader.GetValue(30) != null)
                                                testApxItem.APxTHDNList[0].txtTHDNVerification = dataTableReader.GetValue(30).ToString();
                                            else
                                                testApxItem.APxTHDNList[0].txtTHDNVerification = string.Empty;
                                        }
                                    }

                                    SetTHDNSettings(sourceTestCaseItem.TestCaseID, actionItem.TestActionID, testApxItem);
                                    SetInitialLevelAndGainSettings(sourceTestCaseItem.TestCaseID, actionItem.TestActionID, testApxItem);
                                    SetInitialFreqResponseSettings(sourceTestCaseItem.TestCaseID, actionItem.TestActionID, testApxItem);
                                    SetInitialInterChannelPhaseSettings(sourceTestCaseItem.TestCaseID, actionItem.TestActionID, testApxItem);
                                    SetInitialSteppedFreqSweepSettings(sourceTestCaseItem.TestCaseID, actionItem.TestActionID, testApxItem);
                                }
                                else
                                {
                                    SetInitialLevelAndGainSettings(sourceTestCaseItem.TestCaseID, actionItem.TestActionID, testApxItem);
                                    SetInitialFreqResponseSettings(sourceTestCaseItem.TestCaseID, actionItem.TestActionID, testApxItem);
                                    SetInitialInterChannelPhaseSettings(sourceTestCaseItem.TestCaseID, actionItem.TestActionID, testApxItem);
                                    SetInitialSteppedFreqSweepSettings(sourceTestCaseItem.TestCaseID, actionItem.TestActionID, testApxItem);
                                    SetInitialTHDNSettings(sourceTestCaseItem.TestCaseID, actionItem.TestActionID, testApxItem);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                //MessageBox.Show("Exception\n \n" + ex.Message, "QAT Error Code - EC02025", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            CloseConnection();
            return true;
        }

        private void SetInitialSeqModeSettings(int testCaseID, int testActionID, TestApxItem testApxItem)
        {
            try
            {
                string query = "Select * from APSeqModeInitialSettings where TCID= " + testCaseID + " and ActionID =" + testActionID + "";
                DataTable dataTable = new DataTable();
                DataTableReader dataTableReader = null;
                SqlDataAdapter dataAdapter = new SqlDataAdapter(query, connect);
                dataAdapter.Fill(dataTable);
                dataTableReader = dataTable.CreateDataReader();
                if (dataTableReader.HasRows)
                {
                    while (dataTableReader.Read())
                    {
                        testApxItem.APxInitialSettingsList[0].ChkSeqGenON = testApxItem.APxSettingsList[0].ChkSeqGenON = Convert.ToBoolean(dataTableReader.GetValue(3));
                        if (!testApxItem.APxInitialSettingsList[0].cmb_SeqWaveformList.Contains(dataTableReader.GetValue(4).ToString()))
                            testApxItem.APxInitialSettingsList[0].cmb_SeqWaveformList.Add(dataTableReader.GetValue(4).ToString());
                        if (!testApxItem.APxSettingsList[0].cmb_SeqWaveformList.Contains(dataTableReader.GetValue(4).ToString()))
                            testApxItem.APxSettingsList[0].cmb_SeqWaveformList.Add(dataTableReader.GetValue(4).ToString());

                        testApxItem.APxInitialSettingsList[0].cmbSeqWaveForm = testApxItem.APxSettingsList[0].cmbSeqWaveForm = dataTableReader.GetValue(4).ToString();
                        testApxItem.APxInitialSettingsList[0].cmbSeqTestChannel = testApxItem.APxSettingsList[0].cmbSeqTestChannel = dataTableReader.GetValue(5).ToString();
                        testApxItem.APxInitialSettingsList[0].SeqSetupCount = testApxItem.APxSettingsList[0].SeqSetupCount = Convert.ToInt32(dataTableReader.GetValue(34));
                        testApxItem.APxInitialSettingsList[0].ChkSeqTrackCh = testApxItem.APxSettingsList[0].ChkSeqTrackCh = Convert.ToBoolean(dataTableReader.GetValue(6));
                        testApxItem.APxInitialSettingsList[0].TxtSeqLevelCh1 = testApxItem.APxSettingsList[0].TxtSeqLevelCh1 = dataTableReader.GetValue(7).ToString();
                        testApxItem.APxInitialSettingsList[0].TxtSeqLevelDcCh1 = testApxItem.APxSettingsList[0].TxtSeqLevelDcCh1 = dataTableReader.GetValue(8).ToString();

                        testApxItem.APxInitialSettingsList[0].TxtSeqLevelCh2 = testApxItem.APxSettingsList[0].TxtSeqLevelCh2 = dataTableReader.GetValue(20).ToString();
                        testApxItem.APxInitialSettingsList[0].TxtSeqLevelCh3 = testApxItem.APxSettingsList[0].TxtSeqLevelCh3 = dataTableReader.GetValue(21).ToString();
                        testApxItem.APxInitialSettingsList[0].TxtSeqLevelCh4 = testApxItem.APxSettingsList[0].TxtSeqLevelCh4 = dataTableReader.GetValue(22).ToString();
                        testApxItem.APxInitialSettingsList[0].TxtSeqLevelCh5 = testApxItem.APxSettingsList[0].TxtSeqLevelCh5 = dataTableReader.GetValue(23).ToString();
                        testApxItem.APxInitialSettingsList[0].TxtSeqLevelCh6 = testApxItem.APxSettingsList[0].TxtSeqLevelCh6 = dataTableReader.GetValue(24).ToString();
                        testApxItem.APxInitialSettingsList[0].TxtSeqLevelCh7 = testApxItem.APxSettingsList[0].TxtSeqLevelCh7 = dataTableReader.GetValue(25).ToString();
                        testApxItem.APxInitialSettingsList[0].TxtSeqLevelCh8 = testApxItem.APxSettingsList[0].TxtSeqLevelCh8 = dataTableReader.GetValue(26).ToString();
                        testApxItem.APxInitialSettingsList[0].TxtSeqLevelDcCh2 = testApxItem.APxSettingsList[0].TxtSeqLevelDcCh2 = dataTableReader.GetValue(27).ToString();
                        testApxItem.APxInitialSettingsList[0].TxtSeqLevelDcCh3 = testApxItem.APxSettingsList[0].TxtSeqLevelDcCh3 = dataTableReader.GetValue(28).ToString();
                        testApxItem.APxInitialSettingsList[0].TxtSeqLevelDcCh4 = testApxItem.APxSettingsList[0].TxtSeqLevelDcCh4 = dataTableReader.GetValue(29).ToString();
                        testApxItem.APxInitialSettingsList[0].TxtSeqLevelDcCh5 = testApxItem.APxSettingsList[0].TxtSeqLevelDcCh5 = dataTableReader.GetValue(30).ToString();
                        testApxItem.APxInitialSettingsList[0].TxtSeqLevelDcCh6 = testApxItem.APxSettingsList[0].TxtSeqLevelDcCh6 = dataTableReader.GetValue(31).ToString();
                        testApxItem.APxInitialSettingsList[0].TxtSeqLevelDcCh7 = testApxItem.APxSettingsList[0].TxtSeqLevelDcCh7 = dataTableReader.GetValue(32).ToString();
                        testApxItem.APxInitialSettingsList[0].TxtSeqLevelDcCh8 = testApxItem.APxSettingsList[0].TxtSeqLevelDcCh8 = dataTableReader.GetValue(33).ToString();

                        testApxItem.APxInitialSettingsList[0].TxtSeqFreqA = testApxItem.APxSettingsList[0].TxtSeqFreqA = dataTableReader.GetValue(9).ToString();
                        testApxItem.APxInitialSettingsList[0].TxtSeqFreqB = testApxItem.APxSettingsList[0].TxtSeqFreqB = dataTableReader.GetValue(10).ToString();
                        testApxItem.APxInitialSettingsList[0].ChkSeqCh1Enable = testApxItem.APxSettingsList[0].ChkSeqCh1Enable = Convert.ToBoolean(dataTableReader.GetValue(11));
                        testApxItem.APxInitialSettingsList[0].ChkSeqCh2Enable = testApxItem.APxSettingsList[0].ChkSeqCh2Enable = Convert.ToBoolean(dataTableReader.GetValue(12));
                        testApxItem.APxInitialSettingsList[0].ChkSeqCh3Enable = testApxItem.APxSettingsList[0].ChkSeqCh3Enable = Convert.ToBoolean(dataTableReader.GetValue(13));
                        testApxItem.APxInitialSettingsList[0].ChkSeqCh4Enable = testApxItem.APxSettingsList[0].ChkSeqCh4Enable = Convert.ToBoolean(dataTableReader.GetValue(14));
                        testApxItem.APxInitialSettingsList[0].ChkSeqCh5Enable = testApxItem.APxSettingsList[0].ChkSeqCh5Enable = Convert.ToBoolean(dataTableReader.GetValue(15));
                        testApxItem.APxInitialSettingsList[0].ChkSeqCh6Enable = testApxItem.APxSettingsList[0].ChkSeqCh6Enable = Convert.ToBoolean(dataTableReader.GetValue(16));
                        testApxItem.APxInitialSettingsList[0].ChkSeqCh7Enable = testApxItem.APxSettingsList[0].ChkSeqCh7Enable = Convert.ToBoolean(dataTableReader.GetValue(17));
                        testApxItem.APxInitialSettingsList[0].ChkSeqCh8Enable = testApxItem.APxSettingsList[0].ChkSeqCh8Enable = Convert.ToBoolean(dataTableReader.GetValue(18));
                        testApxItem.APxInitialSettingsList[0].TxtSeqDelay = testApxItem.APxSettingsList[0].TxtSeqDelay = dataTableReader.GetValue(19).ToString();
                    }
                }

                query = "Select * from APBenchModeInitialSettings where TCID= " + testCaseID + " and ActionID =" + testActionID + "";
                DataTable tbl = new DataTable();
                DataTableReader dataReader = null;
                SqlDataAdapter dataAdap = new SqlDataAdapter(query, connect);
                dataAdap.Fill(tbl);
                dataReader = tbl.CreateDataReader();
                if (dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        testApxItem.APxInitialSettingsList[0].ChkBenchGenON = Convert.ToBoolean(dataReader.GetValue(3));
                        if (!testApxItem.APxInitialSettingsList[0].cmb_BenchWaveformList.Contains(dataReader.GetValue(4).ToString()))
                            testApxItem.APxInitialSettingsList[0].cmb_BenchWaveformList.Add(dataReader.GetValue(4).ToString());

                        testApxItem.APxInitialSettingsList[0].cmb_BenchWaveform = dataReader.GetValue(4).ToString();
                        testApxItem.APxInitialSettingsList[0].BenchSetupCount = Convert.ToInt32(dataReader.GetValue(31));
                        testApxItem.APxInitialSettingsList[0].chkBx_BenchLevelTrackCh = Convert.ToBoolean(dataReader.GetValue(5));
                        testApxItem.APxInitialSettingsList[0].txt_BenchLevelCh1 = dataReader.GetValue(6).ToString();
                        testApxItem.APxInitialSettingsList[0].txt_BenchDcOffsetCh1 = dataReader.GetValue(7).ToString();

                        testApxItem.APxInitialSettingsList[0].txt_BenchLevelCh2 = dataReader.GetValue(17).ToString();
                        testApxItem.APxInitialSettingsList[0].txt_BenchLevelCh3 = dataReader.GetValue(18).ToString();
                        testApxItem.APxInitialSettingsList[0].txt_BenchLevelCh4 = dataReader.GetValue(19).ToString();
                        testApxItem.APxInitialSettingsList[0].txt_BenchLevelCh5 = dataReader.GetValue(20).ToString();
                        testApxItem.APxInitialSettingsList[0].txt_BenchLevelCh6 = dataReader.GetValue(21).ToString();
                        testApxItem.APxInitialSettingsList[0].txt_BenchLevelCh7 = dataReader.GetValue(22).ToString();
                        testApxItem.APxInitialSettingsList[0].txt_BenchLevelCh8 = dataReader.GetValue(23).ToString();
                        testApxItem.APxInitialSettingsList[0].txt_BenchDcOffsetCh2 = dataReader.GetValue(24).ToString();
                        testApxItem.APxInitialSettingsList[0].txt_BenchDcOffsetCh3 = dataReader.GetValue(25).ToString();
                        testApxItem.APxInitialSettingsList[0].txt_BenchDcOffsetCh4 = dataReader.GetValue(26).ToString();
                        testApxItem.APxInitialSettingsList[0].txt_BenchDcOffsetCh5 = dataReader.GetValue(27).ToString();
                        testApxItem.APxInitialSettingsList[0].txt_BenchDcOffsetCh6 = dataReader.GetValue(28).ToString();
                        testApxItem.APxInitialSettingsList[0].txt_BenchDcOffsetCh7 = dataReader.GetValue(29).ToString();
                        testApxItem.APxInitialSettingsList[0].txt_BenchDcOffsetCh8 = dataReader.GetValue(30).ToString();

                        testApxItem.APxInitialSettingsList[0].txt_BenchfrequencyA = dataReader.GetValue(8).ToString();
                        testApxItem.APxInitialSettingsList[0].txt_BenchfrequencyB = dataReader.GetValue(32).ToString();

                        testApxItem.APxInitialSettingsList[0].ChkBenchCh1Enable = Convert.ToBoolean(dataReader.GetValue(9));
                        testApxItem.APxInitialSettingsList[0].ChkBenchCh2Enable = Convert.ToBoolean(dataReader.GetValue(10));
                        testApxItem.APxInitialSettingsList[0].ChkBenchCh3Enable = Convert.ToBoolean(dataReader.GetValue(11));
                        testApxItem.APxInitialSettingsList[0].ChkBenchCh4Enable = Convert.ToBoolean(dataReader.GetValue(12));
                        testApxItem.APxInitialSettingsList[0].ChkBenchCh5Enable = Convert.ToBoolean(dataReader.GetValue(13));
                        testApxItem.APxInitialSettingsList[0].ChkBenchCh6Enable = Convert.ToBoolean(dataReader.GetValue(14));
                        testApxItem.APxInitialSettingsList[0].ChkBenchCh7Enable = Convert.ToBoolean(dataReader.GetValue(15));
                        testApxItem.APxInitialSettingsList[0].ChkBenchCh8Enable = Convert.ToBoolean(dataReader.GetValue(16));
                    }
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

        private void SetInitialBenchModeSettings(int testCaseID, int testActionID, TestApxItem testApxItem)
        {
            try
            {
                string query = "Select * from APSeqModeInitialSettings where TCID= " + testCaseID + " and ActionID =" + testActionID + "";
                DataTable dataTable = new DataTable();
                DataTableReader dataTableReader = null;
                SqlDataAdapter dataAdapter = new SqlDataAdapter(query, connect);
                dataAdapter.Fill(dataTable);
                dataTableReader = dataTable.CreateDataReader();
                if (dataTableReader.HasRows)
                {
                    while (dataTableReader.Read())
                    {
                        testApxItem.APxInitialSettingsList[0].ChkSeqGenON = Convert.ToBoolean(dataTableReader.GetValue(3));
                        if (!testApxItem.APxInitialSettingsList[0].cmb_SeqWaveformList.Contains(dataTableReader.GetValue(4).ToString()))
                            testApxItem.APxInitialSettingsList[0].cmb_SeqWaveformList.Add(dataTableReader.GetValue(4).ToString());

                        testApxItem.APxInitialSettingsList[0].cmbSeqWaveForm = dataTableReader.GetValue(4).ToString();
                        testApxItem.APxInitialSettingsList[0].cmbSeqTestChannel = dataTableReader.GetValue(5).ToString();
                        testApxItem.APxInitialSettingsList[0].SeqSetupCount = Convert.ToInt32(dataTableReader.GetValue(34));
                        testApxItem.APxInitialSettingsList[0].ChkSeqTrackCh = Convert.ToBoolean(dataTableReader.GetValue(6));
                        testApxItem.APxInitialSettingsList[0].TxtSeqLevelCh1 = dataTableReader.GetValue(7).ToString();
                        testApxItem.APxInitialSettingsList[0].TxtSeqLevelDcCh1 = dataTableReader.GetValue(8).ToString();
                        testApxItem.APxInitialSettingsList[0].TxtSeqLevelCh2 = dataTableReader.GetValue(20).ToString();
                        testApxItem.APxInitialSettingsList[0].TxtSeqLevelCh3 = dataTableReader.GetValue(21).ToString();
                        testApxItem.APxInitialSettingsList[0].TxtSeqLevelCh4 = dataTableReader.GetValue(22).ToString();
                        testApxItem.APxInitialSettingsList[0].TxtSeqLevelCh5 = dataTableReader.GetValue(23).ToString();
                        testApxItem.APxInitialSettingsList[0].TxtSeqLevelCh6 = dataTableReader.GetValue(24).ToString();
                        testApxItem.APxInitialSettingsList[0].TxtSeqLevelCh7 = dataTableReader.GetValue(25).ToString();
                        testApxItem.APxInitialSettingsList[0].TxtSeqLevelCh8 = dataTableReader.GetValue(26).ToString();
                        testApxItem.APxInitialSettingsList[0].TxtSeqLevelDcCh2 = dataTableReader.GetValue(27).ToString();
                        testApxItem.APxInitialSettingsList[0].TxtSeqLevelDcCh3 = dataTableReader.GetValue(28).ToString();
                        testApxItem.APxInitialSettingsList[0].TxtSeqLevelDcCh4 = dataTableReader.GetValue(29).ToString();
                        testApxItem.APxInitialSettingsList[0].TxtSeqLevelDcCh5 = dataTableReader.GetValue(30).ToString();
                        testApxItem.APxInitialSettingsList[0].TxtSeqLevelDcCh6 = dataTableReader.GetValue(31).ToString();
                        testApxItem.APxInitialSettingsList[0].TxtSeqLevelDcCh7 = dataTableReader.GetValue(32).ToString();
                        testApxItem.APxInitialSettingsList[0].TxtSeqLevelDcCh8 = dataTableReader.GetValue(33).ToString();
                        testApxItem.APxInitialSettingsList[0].TxtSeqFreqA = dataTableReader.GetValue(9).ToString();
                        testApxItem.APxInitialSettingsList[0].TxtSeqFreqB = dataTableReader.GetValue(10).ToString();
                        testApxItem.APxInitialSettingsList[0].ChkSeqCh1Enable = Convert.ToBoolean(dataTableReader.GetValue(11));
                        testApxItem.APxInitialSettingsList[0].ChkSeqCh2Enable = Convert.ToBoolean(dataTableReader.GetValue(12));
                        testApxItem.APxInitialSettingsList[0].ChkSeqCh3Enable = Convert.ToBoolean(dataTableReader.GetValue(13));
                        testApxItem.APxInitialSettingsList[0].ChkSeqCh4Enable = Convert.ToBoolean(dataTableReader.GetValue(14));
                        testApxItem.APxInitialSettingsList[0].ChkSeqCh5Enable = Convert.ToBoolean(dataTableReader.GetValue(15));
                        testApxItem.APxInitialSettingsList[0].ChkSeqCh6Enable = Convert.ToBoolean(dataTableReader.GetValue(16));
                        testApxItem.APxInitialSettingsList[0].ChkSeqCh7Enable = Convert.ToBoolean(dataTableReader.GetValue(17));
                        testApxItem.APxInitialSettingsList[0].ChkSeqCh8Enable = Convert.ToBoolean(dataTableReader.GetValue(18));
                        testApxItem.APxInitialSettingsList[0].TxtSeqDelay = dataTableReader.GetValue(19).ToString();
                    }
                }

                query = "Select * from APBenchModeInitialSettings where TCID= " + testCaseID + " and ActionID =" + testActionID + "";
                DataTable tbl = new DataTable();
                DataTableReader dataReader = null;
                SqlDataAdapter dataAdap = new SqlDataAdapter(query, connect);
                dataAdap.Fill(tbl);
                dataReader = tbl.CreateDataReader();
                if (dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        testApxItem.APxInitialSettingsList[0].ChkBenchGenON = testApxItem.APxSettingsList[0].ChkBenchGenON = Convert.ToBoolean(dataReader.GetValue(3));
                        if (!testApxItem.APxInitialSettingsList[0].cmb_BenchWaveformList.Contains(dataTableReader.GetValue(4).ToString()))
                            testApxItem.APxInitialSettingsList[0].cmb_BenchWaveformList.Add(dataTableReader.GetValue(4).ToString());
                        if (!testApxItem.APxSettingsList[0].cmb_BenchWaveformList.Contains(dataTableReader.GetValue(4).ToString()))
                            testApxItem.APxSettingsList[0].cmb_BenchWaveformList.Add(dataTableReader.GetValue(4).ToString());

                        testApxItem.APxInitialSettingsList[0].cmb_BenchWaveform = testApxItem.APxSettingsList[0].cmb_BenchWaveform = dataReader.GetValue(4).ToString();
                        testApxItem.APxInitialSettingsList[0].BenchSetupCount = testApxItem.APxSettingsList[0].BenchSetupCount = Convert.ToInt32(dataReader.GetValue(31));
                        testApxItem.APxInitialSettingsList[0].chkBx_BenchLevelTrackCh = testApxItem.APxSettingsList[0].chkBx_BenchLevelTrackCh = Convert.ToBoolean(dataReader.GetValue(5));
                        testApxItem.APxInitialSettingsList[0].txt_BenchLevelCh1 = testApxItem.APxSettingsList[0].txt_BenchLevelCh1 = dataReader.GetValue(6).ToString();
                        testApxItem.APxInitialSettingsList[0].txt_BenchDcOffsetCh1 = testApxItem.APxSettingsList[0].txt_BenchDcOffsetCh1 = dataReader.GetValue(7).ToString();

                        testApxItem.APxInitialSettingsList[0].txt_BenchLevelCh2 = testApxItem.APxSettingsList[0].txt_BenchLevelCh2 = dataReader.GetValue(17).ToString();
                        testApxItem.APxInitialSettingsList[0].txt_BenchLevelCh3 = testApxItem.APxSettingsList[0].txt_BenchLevelCh3 = dataReader.GetValue(18).ToString();
                        testApxItem.APxInitialSettingsList[0].txt_BenchLevelCh4 = testApxItem.APxSettingsList[0].txt_BenchLevelCh4 = dataReader.GetValue(19).ToString();
                        testApxItem.APxInitialSettingsList[0].txt_BenchLevelCh5 = testApxItem.APxSettingsList[0].txt_BenchLevelCh5 = dataReader.GetValue(20).ToString();
                        testApxItem.APxInitialSettingsList[0].txt_BenchLevelCh6 = testApxItem.APxSettingsList[0].txt_BenchLevelCh6 = dataReader.GetValue(21).ToString();
                        testApxItem.APxInitialSettingsList[0].txt_BenchLevelCh7 = testApxItem.APxSettingsList[0].txt_BenchLevelCh7 = dataReader.GetValue(22).ToString();
                        testApxItem.APxInitialSettingsList[0].txt_BenchLevelCh8 = testApxItem.APxSettingsList[0].txt_BenchLevelCh8 = dataReader.GetValue(23).ToString();

                        testApxItem.APxInitialSettingsList[0].txt_BenchDcOffsetCh2 = testApxItem.APxSettingsList[0].txt_BenchDcOffsetCh2 = dataReader.GetValue(24).ToString();
                        testApxItem.APxInitialSettingsList[0].txt_BenchDcOffsetCh3 = testApxItem.APxSettingsList[0].txt_BenchDcOffsetCh3 = dataReader.GetValue(25).ToString();
                        testApxItem.APxInitialSettingsList[0].txt_BenchDcOffsetCh4 = testApxItem.APxSettingsList[0].txt_BenchDcOffsetCh4 = dataReader.GetValue(26).ToString();
                        testApxItem.APxInitialSettingsList[0].txt_BenchDcOffsetCh5 = testApxItem.APxSettingsList[0].txt_BenchDcOffsetCh5 = dataReader.GetValue(27).ToString();
                        testApxItem.APxInitialSettingsList[0].txt_BenchDcOffsetCh6 = testApxItem.APxSettingsList[0].txt_BenchDcOffsetCh6 = dataReader.GetValue(28).ToString();
                        testApxItem.APxInitialSettingsList[0].txt_BenchDcOffsetCh7 = testApxItem.APxSettingsList[0].txt_BenchDcOffsetCh7 = dataReader.GetValue(29).ToString();
                        testApxItem.APxInitialSettingsList[0].txt_BenchDcOffsetCh8 = testApxItem.APxSettingsList[0].txt_BenchDcOffsetCh8 = dataReader.GetValue(30).ToString();

                        testApxItem.APxInitialSettingsList[0].txt_BenchfrequencyA = testApxItem.APxSettingsList[0].txt_BenchfrequencyA = dataReader.GetValue(8).ToString();
                        testApxItem.APxInitialSettingsList[0].txt_BenchfrequencyB = testApxItem.APxSettingsList[0].txt_BenchfrequencyB = dataReader.GetValue(32).ToString();

                        testApxItem.APxInitialSettingsList[0].ChkBenchCh1Enable = testApxItem.APxSettingsList[0].ChkBenchCh1Enable = Convert.ToBoolean(dataReader.GetValue(9));
                        testApxItem.APxInitialSettingsList[0].ChkBenchCh2Enable = testApxItem.APxSettingsList[0].ChkBenchCh2Enable = Convert.ToBoolean(dataReader.GetValue(10));
                        testApxItem.APxInitialSettingsList[0].ChkBenchCh3Enable = testApxItem.APxSettingsList[0].ChkBenchCh3Enable = Convert.ToBoolean(dataReader.GetValue(11));
                        testApxItem.APxInitialSettingsList[0].ChkBenchCh4Enable = testApxItem.APxSettingsList[0].ChkBenchCh4Enable = Convert.ToBoolean(dataReader.GetValue(12));
                        testApxItem.APxInitialSettingsList[0].ChkBenchCh5Enable = testApxItem.APxSettingsList[0].ChkBenchCh5Enable = Convert.ToBoolean(dataReader.GetValue(13));
                        testApxItem.APxInitialSettingsList[0].ChkBenchCh6Enable = testApxItem.APxSettingsList[0].ChkBenchCh6Enable = Convert.ToBoolean(dataReader.GetValue(14));
                        testApxItem.APxInitialSettingsList[0].ChkBenchCh7Enable = testApxItem.APxSettingsList[0].ChkBenchCh7Enable = Convert.ToBoolean(dataReader.GetValue(15));
                        testApxItem.APxInitialSettingsList[0].ChkBenchCh8Enable = testApxItem.APxSettingsList[0].ChkBenchCh8Enable = Convert.ToBoolean(dataReader.GetValue(16));
                    }
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

        private void SetLevelAndGainSettings(int testCaseID, int testActionID, TestApxItem testApxItem)
        {
            try
            {
                string query = "Select * from APLevelAndGainInitialSettings where TCID= " + testCaseID + " and ActionID =" + testActionID + "";
                DataTable dataTable = new DataTable();
                DataTableReader dataTableReader = null;
                SqlDataAdapter dataAdapter = new SqlDataAdapter(query, connect);
                dataAdapter.Fill(dataTable);
                dataTableReader = dataTable.CreateDataReader();
                if (dataTableReader.HasRows)
                {
                    while (dataTableReader.Read())
                    {
                        testApxItem.APxInitialLevelAndGainList[0].ChkGainGenON = Convert.ToBoolean(dataTableReader.GetValue(3));
                        if (!testApxItem.APxInitialLevelAndGainList[0].cmbGainWaveformList.Contains(dataTableReader.GetValue(4).ToString()))
                            testApxItem.APxInitialLevelAndGainList[0].cmbGainWaveformList.Add(dataTableReader.GetValue(4).ToString());

                        testApxItem.APxInitialLevelAndGainList[0].cmb_GainWaveform = dataTableReader.GetValue(4).ToString();
                        testApxItem.APxInitialLevelAndGainList[0].SeqGainSetupCount = Convert.ToInt32(dataTableReader.GetValue(31));

                        if (dataTableReader.GetValue(33) != null & dataTableReader.GetValue(33).ToString() != string.Empty)
                            testApxItem.APxInitialLevelAndGainList[0].GainInputChCount = Convert.ToInt32(dataTableReader.GetValue(33));

                        testApxItem.APxInitialLevelAndGainList[0].chkBx_GainLevelTrackCh = Convert.ToBoolean(dataTableReader.GetValue(5));
                        testApxItem.APxInitialLevelAndGainList[0].txt_GainCh1Level = dataTableReader.GetValue(6).ToString();
                        testApxItem.APxInitialLevelAndGainList[0].txt_GainDcCh1Offset = dataTableReader.GetValue(7).ToString();

                        testApxItem.APxInitialLevelAndGainList[0].txt_GainCh2Level = dataTableReader.GetValue(17).ToString();
                        testApxItem.APxInitialLevelAndGainList[0].txt_GainCh3Level = dataTableReader.GetValue(18).ToString();
                        testApxItem.APxInitialLevelAndGainList[0].txt_GainCh4Level = dataTableReader.GetValue(19).ToString();
                        testApxItem.APxInitialLevelAndGainList[0].txt_GainCh5Level = dataTableReader.GetValue(20).ToString();
                        testApxItem.APxInitialLevelAndGainList[0].txt_GainCh6Level = dataTableReader.GetValue(21).ToString();
                        testApxItem.APxInitialLevelAndGainList[0].txt_GainCh7Level = dataTableReader.GetValue(22).ToString();
                        testApxItem.APxInitialLevelAndGainList[0].txt_GainCh8Level = dataTableReader.GetValue(23).ToString();

                        testApxItem.APxInitialLevelAndGainList[0].txt_GainDcCh2Offset = dataTableReader.GetValue(24).ToString();
                        testApxItem.APxInitialLevelAndGainList[0].txt_GainDcCh3Offset = dataTableReader.GetValue(25).ToString();
                        testApxItem.APxInitialLevelAndGainList[0].txt_GainDcCh4Offset = dataTableReader.GetValue(26).ToString();
                        testApxItem.APxInitialLevelAndGainList[0].txt_GainDcCh5Offset = dataTableReader.GetValue(27).ToString();
                        testApxItem.APxInitialLevelAndGainList[0].txt_GainDcCh6Offset = dataTableReader.GetValue(28).ToString();
                        testApxItem.APxInitialLevelAndGainList[0].txt_GainDcCh7Offset = dataTableReader.GetValue(29).ToString();
                        testApxItem.APxInitialLevelAndGainList[0].txt_GainDcCh8Offset = dataTableReader.GetValue(30).ToString();

                        testApxItem.APxInitialLevelAndGainList[0].txt_GainfrequencyA = dataTableReader.GetValue(8).ToString();
                        testApxItem.APxInitialLevelAndGainList[0].txt_GainfrequencyB = dataTableReader.GetValue(32).ToString();

                        testApxItem.APxInitialLevelAndGainList[0].btn_GainCh1 = Convert.ToBoolean(dataTableReader.GetValue(9));
                        testApxItem.APxInitialLevelAndGainList[0].btn_GainCh2 = Convert.ToBoolean(dataTableReader.GetValue(10));
                        testApxItem.APxInitialLevelAndGainList[0].btn_GainCh3 = Convert.ToBoolean(dataTableReader.GetValue(11));
                        testApxItem.APxInitialLevelAndGainList[0].btn_GainCh4 = Convert.ToBoolean(dataTableReader.GetValue(12));
                        testApxItem.APxInitialLevelAndGainList[0].btn_GainCh5 = Convert.ToBoolean(dataTableReader.GetValue(13));
                        testApxItem.APxInitialLevelAndGainList[0].btn_GainCh6 = Convert.ToBoolean(dataTableReader.GetValue(14));
                        testApxItem.APxInitialLevelAndGainList[0].btn_GainCh7 = Convert.ToBoolean(dataTableReader.GetValue(15));
                        testApxItem.APxInitialLevelAndGainList[0].btn_GainCh8 = Convert.ToBoolean(dataTableReader.GetValue(16));
                    }
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

        private void SetInitialLevelAndGainSettings(int testCaseID, int testActionID, TestApxItem testApxItem)
        {
            try
            {
                string query = "Select * from APLevelAndGainInitialSettings where TCID= " + testCaseID + " and ActionID =" + testActionID + "";
                DataTable dataTable = new DataTable();
                DataTableReader dataTableReader = null;
                SqlDataAdapter dataAdapter = new SqlDataAdapter(query, connect);
                dataAdapter.Fill(dataTable);
                dataTableReader = dataTable.CreateDataReader();
                if (dataTableReader.HasRows)
                {
                    while (dataTableReader.Read())
                    {
                        testApxItem.APxInitialLevelAndGainList[0].ChkGainGenON = testApxItem.APxLevelAndGainList[0].ChkGainGenON = Convert.ToBoolean(dataTableReader.GetValue(3));
                        if (!testApxItem.APxInitialLevelAndGainList[0].cmbGainWaveformList.Contains(dataTableReader.GetValue(4).ToString()))
                        {
                            testApxItem.APxInitialLevelAndGainList[0].cmbGainWaveformList.Add(dataTableReader.GetValue(4).ToString());
                        }

                        if (!testApxItem.APxLevelAndGainList[0].cmbGainWaveformList.Contains(dataTableReader.GetValue(4).ToString()))
                        {
                            testApxItem.APxLevelAndGainList[0].cmbGainWaveformList.Add(dataTableReader.GetValue(4).ToString());
                        }

                        testApxItem.APxInitialLevelAndGainList[0].cmb_GainWaveform = testApxItem.APxLevelAndGainList[0].cmb_GainWaveform = dataTableReader.GetValue(4).ToString();
                        testApxItem.APxInitialLevelAndGainList[0].SeqGainSetupCount = testApxItem.APxLevelAndGainList[0].SeqGainSetupCount = Convert.ToInt32(dataTableReader.GetValue(31));

                        if (dataTableReader.GetValue(33) != null & dataTableReader.GetValue(33).ToString() != string.Empty)
                        {
                            testApxItem.APxInitialLevelAndGainList[0].GainInputChCount = Convert.ToInt32(dataTableReader.GetValue(33));
                            testApxItem.APxLevelAndGainList[0].GainInputChCount = Convert.ToInt32(dataTableReader.GetValue(33));
                        }

                        testApxItem.APxInitialLevelAndGainList[0].chkBx_GainLevelTrackCh = testApxItem.APxLevelAndGainList[0].chkBx_GainLevelTrackCh = Convert.ToBoolean(dataTableReader.GetValue(5));
                        testApxItem.APxInitialLevelAndGainList[0].txt_GainCh1Level = testApxItem.APxLevelAndGainList[0].txt_GainCh1Level = dataTableReader.GetValue(6).ToString();
                        testApxItem.APxInitialLevelAndGainList[0].txt_GainDcCh1Offset = testApxItem.APxLevelAndGainList[0].txt_GainDcCh1Offset = dataTableReader.GetValue(7).ToString();

                        testApxItem.APxInitialLevelAndGainList[0].txt_GainCh2Level = testApxItem.APxLevelAndGainList[0].txt_GainCh2Level = dataTableReader.GetValue(17).ToString();
                        testApxItem.APxInitialLevelAndGainList[0].txt_GainCh3Level = testApxItem.APxLevelAndGainList[0].txt_GainCh3Level = dataTableReader.GetValue(18).ToString();
                        testApxItem.APxInitialLevelAndGainList[0].txt_GainCh4Level = testApxItem.APxLevelAndGainList[0].txt_GainCh4Level = dataTableReader.GetValue(19).ToString();
                        testApxItem.APxInitialLevelAndGainList[0].txt_GainCh5Level = testApxItem.APxLevelAndGainList[0].txt_GainCh5Level = dataTableReader.GetValue(20).ToString();
                        testApxItem.APxInitialLevelAndGainList[0].txt_GainCh6Level = testApxItem.APxLevelAndGainList[0].txt_GainCh6Level = dataTableReader.GetValue(21).ToString();
                        testApxItem.APxInitialLevelAndGainList[0].txt_GainCh7Level = testApxItem.APxLevelAndGainList[0].txt_GainCh7Level = dataTableReader.GetValue(22).ToString();
                        testApxItem.APxInitialLevelAndGainList[0].txt_GainCh8Level = testApxItem.APxLevelAndGainList[0].txt_GainCh8Level = dataTableReader.GetValue(23).ToString();

                        testApxItem.APxInitialLevelAndGainList[0].txt_GainDcCh2Offset = testApxItem.APxLevelAndGainList[0].txt_GainDcCh2Offset = dataTableReader.GetValue(24).ToString();
                        testApxItem.APxInitialLevelAndGainList[0].txt_GainDcCh3Offset = testApxItem.APxLevelAndGainList[0].txt_GainDcCh3Offset = dataTableReader.GetValue(25).ToString();
                        testApxItem.APxInitialLevelAndGainList[0].txt_GainDcCh4Offset = testApxItem.APxLevelAndGainList[0].txt_GainDcCh4Offset = dataTableReader.GetValue(26).ToString();
                        testApxItem.APxInitialLevelAndGainList[0].txt_GainDcCh5Offset = testApxItem.APxLevelAndGainList[0].txt_GainDcCh5Offset = dataTableReader.GetValue(27).ToString();
                        testApxItem.APxInitialLevelAndGainList[0].txt_GainDcCh6Offset = testApxItem.APxLevelAndGainList[0].txt_GainDcCh6Offset = dataTableReader.GetValue(28).ToString();
                        testApxItem.APxInitialLevelAndGainList[0].txt_GainDcCh7Offset = testApxItem.APxLevelAndGainList[0].txt_GainDcCh7Offset = dataTableReader.GetValue(29).ToString();
                        testApxItem.APxInitialLevelAndGainList[0].txt_GainDcCh8Offset = testApxItem.APxLevelAndGainList[0].txt_GainDcCh8Offset = dataTableReader.GetValue(30).ToString();

                        testApxItem.APxInitialLevelAndGainList[0].txt_GainfrequencyA = testApxItem.APxLevelAndGainList[0].txt_GainfrequencyA = dataTableReader.GetValue(8).ToString();
                        testApxItem.APxInitialLevelAndGainList[0].txt_GainfrequencyB = testApxItem.APxLevelAndGainList[0].txt_GainfrequencyB = dataTableReader.GetValue(32).ToString();

                        testApxItem.APxInitialLevelAndGainList[0].btn_GainCh1 = testApxItem.APxLevelAndGainList[0].btn_GainCh1 = Convert.ToBoolean(dataTableReader.GetValue(9));
                        testApxItem.APxInitialLevelAndGainList[0].btn_GainCh2 = testApxItem.APxLevelAndGainList[0].btn_GainCh2 = Convert.ToBoolean(dataTableReader.GetValue(10));
                        testApxItem.APxInitialLevelAndGainList[0].btn_GainCh3 = testApxItem.APxLevelAndGainList[0].btn_GainCh3 = Convert.ToBoolean(dataTableReader.GetValue(11));
                        testApxItem.APxInitialLevelAndGainList[0].btn_GainCh4 = testApxItem.APxLevelAndGainList[0].btn_GainCh4 = Convert.ToBoolean(dataTableReader.GetValue(12));
                        testApxItem.APxInitialLevelAndGainList[0].btn_GainCh5 = testApxItem.APxLevelAndGainList[0].btn_GainCh5 = Convert.ToBoolean(dataTableReader.GetValue(13));
                        testApxItem.APxInitialLevelAndGainList[0].btn_GainCh6 = testApxItem.APxLevelAndGainList[0].btn_GainCh6 = Convert.ToBoolean(dataTableReader.GetValue(14));
                        testApxItem.APxInitialLevelAndGainList[0].btn_GainCh7 = testApxItem.APxLevelAndGainList[0].btn_GainCh7 = Convert.ToBoolean(dataTableReader.GetValue(15));
                        testApxItem.APxInitialLevelAndGainList[0].btn_GainCh8 = testApxItem.APxLevelAndGainList[0].btn_GainCh8 = Convert.ToBoolean(dataTableReader.GetValue(16));
                    }
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

        private void SetFreqResponseSettings(int testCaseID, int testActionID, TestApxItem testApxItem)
        {
            try
            {
                string query = "Select * from APFrequencyResponseInitialSettings where TCID= " + testCaseID + " and ActionID =" + testActionID + "";
                DataTable dataTable = new DataTable();
                DataTableReader dataTableReader = null;
                SqlDataAdapter dataAdapter = new SqlDataAdapter(query, connect);
                dataAdapter.Fill(dataTable);
                dataTableReader = dataTable.CreateDataReader();
                if (dataTableReader.HasRows)
                {
                    while (dataTableReader.Read())
                    {
                        testApxItem.APxInitialFreqResponseList[0].StartGenON = Convert.ToBoolean(dataTableReader.GetValue(3));
                        testApxItem.APxInitialFreqResponseList[0].txtStartFreq = dataTableReader.GetValue(4).ToString();
                        testApxItem.APxInitialFreqResponseList[0].txtStopFreq = dataTableReader.GetValue(5).ToString();
                        testApxItem.APxInitialFreqResponseList[0].txtLevel = dataTableReader.GetValue(6).ToString();
                        testApxItem.APxInitialFreqResponseList[0].IsEnableCh1 = Convert.ToBoolean(dataTableReader.GetValue(7));
                        testApxItem.APxInitialFreqResponseList[0].IsEnableCh2 = Convert.ToBoolean(dataTableReader.GetValue(8));
                        testApxItem.APxInitialFreqResponseList[0].IsEnableCh3 = Convert.ToBoolean(dataTableReader.GetValue(9));
                        testApxItem.APxInitialFreqResponseList[0].IsEnableCh4 = Convert.ToBoolean(dataTableReader.GetValue(10));
                        testApxItem.APxInitialFreqResponseList[0].IsEnableCh5 = Convert.ToBoolean(dataTableReader.GetValue(11));
                        testApxItem.APxInitialFreqResponseList[0].IsEnableCh6 = Convert.ToBoolean(dataTableReader.GetValue(12));
                        testApxItem.APxInitialFreqResponseList[0].IsEnableCh7 = Convert.ToBoolean(dataTableReader.GetValue(13));
                        testApxItem.APxInitialFreqResponseList[0].IsEnableCh8 = Convert.ToBoolean(dataTableReader.GetValue(14));
                        testApxItem.APxInitialFreqResponseList[0].OutChannelCount = Convert.ToInt32(dataTableReader.GetValue(15));
                    }
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

        private void SetInitialFreqResponseSettings(int testCaseID, int testActionID, TestApxItem testApxItem)
        {
            try
            {
                string query = "Select * from APFrequencyResponseInitialSettings where TCID= " + testCaseID + " and ActionID =" + testActionID + "";
                DataTable dataTable = new DataTable();
                DataTableReader dataTableReader = null;
                SqlDataAdapter dataAdapter = new SqlDataAdapter(query, connect);
                dataAdapter.Fill(dataTable);
                dataTableReader = dataTable.CreateDataReader();
                if (dataTableReader.HasRows)
                {
                    while (dataTableReader.Read())
                    {

                        testApxItem.APxInitialFreqResponseList[0].StartGenON = testApxItem.APxFreqResponseList[0].StartGenON = Convert.ToBoolean(dataTableReader.GetValue(3));
                        testApxItem.APxInitialFreqResponseList[0].txtStartFreq = testApxItem.APxFreqResponseList[0].txtStartFreq = dataTableReader.GetValue(4).ToString();
                        testApxItem.APxInitialFreqResponseList[0].txtStopFreq = testApxItem.APxFreqResponseList[0].txtStopFreq = dataTableReader.GetValue(5).ToString();
                        testApxItem.APxInitialFreqResponseList[0].txtLevel = testApxItem.APxFreqResponseList[0].txtLevel = dataTableReader.GetValue(6).ToString();
                        testApxItem.APxInitialFreqResponseList[0].IsEnableCh1 = testApxItem.APxFreqResponseList[0].IsEnableCh1 = Convert.ToBoolean(dataTableReader.GetValue(7));
                        testApxItem.APxInitialFreqResponseList[0].IsEnableCh2 = testApxItem.APxFreqResponseList[0].IsEnableCh2 = Convert.ToBoolean(dataTableReader.GetValue(8));
                        testApxItem.APxInitialFreqResponseList[0].IsEnableCh3 = testApxItem.APxFreqResponseList[0].IsEnableCh3 = Convert.ToBoolean(dataTableReader.GetValue(9));
                        testApxItem.APxInitialFreqResponseList[0].IsEnableCh4 = testApxItem.APxFreqResponseList[0].IsEnableCh4 = Convert.ToBoolean(dataTableReader.GetValue(10));
                        testApxItem.APxInitialFreqResponseList[0].IsEnableCh5 = testApxItem.APxFreqResponseList[0].IsEnableCh5 = Convert.ToBoolean(dataTableReader.GetValue(11));
                        testApxItem.APxInitialFreqResponseList[0].IsEnableCh6 = testApxItem.APxFreqResponseList[0].IsEnableCh6 = Convert.ToBoolean(dataTableReader.GetValue(12));
                        testApxItem.APxInitialFreqResponseList[0].IsEnableCh7 = testApxItem.APxFreqResponseList[0].IsEnableCh7 = Convert.ToBoolean(dataTableReader.GetValue(13));
                        testApxItem.APxInitialFreqResponseList[0].IsEnableCh8 = testApxItem.APxFreqResponseList[0].IsEnableCh8 = Convert.ToBoolean(dataTableReader.GetValue(14));
                        testApxItem.APxInitialFreqResponseList[0].OutChannelCount = testApxItem.APxFreqResponseList[0].OutChannelCount = Convert.ToInt32(dataTableReader.GetValue(15));
                    }
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

        private void SetInterChannelPhaseSettings(int testCaseID, int testActionID, TestApxItem testApxItem)
        {
            try
            {
                string query = "Select * from APPhaseInitialSettings where TCID= " + testCaseID + " and ActionID =" + testActionID + "";
                DataTable dataTable = new DataTable();
                DataTableReader dataTableReader = null;
                SqlDataAdapter dataAdapter = new SqlDataAdapter(query, connect);
                dataAdapter.Fill(dataTable);
                dataTableReader = dataTable.CreateDataReader();
                if (dataTableReader.HasRows)
                {
                    while (dataTableReader.Read())
                    {
                        testApxItem.APxInitialInterChPhaseList[0].OutChannelCount = Convert.ToInt32(dataTableReader.GetValue(24));
                        testApxItem.APxInitialInterChPhaseList[0].InChannelCount = Convert.ToInt32(dataTableReader.GetValue(25));

                        testApxItem.APxInitialInterChPhaseList[0].ChkGenON = Convert.ToBoolean(dataTableReader.GetValue(3));
                        testApxItem.APxInitialInterChPhaseList[0].SteppedTrackChannel = Convert.ToBoolean(dataTableReader.GetValue(4));
                        testApxItem.APxInitialInterChPhaseList[0].LevelCh1 = dataTableReader.GetValue(5).ToString();
                        testApxItem.APxInitialInterChPhaseList[0].LevelCh2 = dataTableReader.GetValue(6).ToString();
                        testApxItem.APxInitialInterChPhaseList[0].LevelCh3 = dataTableReader.GetValue(7).ToString();
                        testApxItem.APxInitialInterChPhaseList[0].LevelCh4 = dataTableReader.GetValue(8).ToString();
                        testApxItem.APxInitialInterChPhaseList[0].LevelCh5 = dataTableReader.GetValue(9).ToString();
                        testApxItem.APxInitialInterChPhaseList[0].LevelCh6 = dataTableReader.GetValue(10).ToString();
                        testApxItem.APxInitialInterChPhaseList[0].LevelCh7 = dataTableReader.GetValue(11).ToString();
                        testApxItem.APxInitialInterChPhaseList[0].LevelCh8 = dataTableReader.GetValue(12).ToString();

                        testApxItem.APxInitialInterChPhaseList[0].TxtFreqA = dataTableReader.GetValue(13).ToString();

                        testApxItem.APxInitialInterChPhaseList[0].Isch1Enable = Convert.ToBoolean(dataTableReader.GetValue(14));
                        testApxItem.APxInitialInterChPhaseList[0].Isch2Enable = Convert.ToBoolean(dataTableReader.GetValue(15));
                        testApxItem.APxInitialInterChPhaseList[0].Isch3Enable = Convert.ToBoolean(dataTableReader.GetValue(16));
                        testApxItem.APxInitialInterChPhaseList[0].Isch4Enable = Convert.ToBoolean(dataTableReader.GetValue(17));
                        testApxItem.APxInitialInterChPhaseList[0].Isch5Enable = Convert.ToBoolean(dataTableReader.GetValue(18));
                        testApxItem.APxInitialInterChPhaseList[0].Isch6Enable = Convert.ToBoolean(dataTableReader.GetValue(19));
                        testApxItem.APxInitialInterChPhaseList[0].Isch7Enable = Convert.ToBoolean(dataTableReader.GetValue(20));
                        testApxItem.APxInitialInterChPhaseList[0].Isch8Enable = Convert.ToBoolean(dataTableReader.GetValue(21));

                        testApxItem.APxInitialInterChPhaseList[0].CmbRefChannelSelected = dataTableReader.GetValue(22).ToString();
                        testApxItem.APxInitialInterChPhaseList[0].MeterRangeSelected = dataTableReader.GetValue(23).ToString();
                    }
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

        private void SetInitialInterChannelPhaseSettings(int testCaseID, int testActionID, TestApxItem testApxItem)
        {
            try
            {
                string query = "Select * from APPhaseInitialSettings where TCID= " + testCaseID + " and ActionID =" + testActionID + "";
                DataTable dataTable = new DataTable();
                DataTableReader dataTableReader = null;
                SqlDataAdapter dataAdapter = new SqlDataAdapter(query, connect);
                dataAdapter.Fill(dataTable);
                dataTableReader = dataTable.CreateDataReader();
                if (dataTableReader.HasRows)
                {
                    while (dataTableReader.Read())
                    {
                        testApxItem.APxInitialInterChPhaseList[0].OutChannelCount = testApxItem.APxInterChPhaseList[0].OutChannelCount = Convert.ToInt32(dataTableReader.GetValue(24));
                        testApxItem.APxInitialInterChPhaseList[0].InChannelCount = testApxItem.APxInterChPhaseList[0].InChannelCount = Convert.ToInt32(dataTableReader.GetValue(25));

                        testApxItem.APxInitialInterChPhaseList[0].ChkGenON = testApxItem.APxInterChPhaseList[0].ChkGenON = Convert.ToBoolean(dataTableReader.GetValue(3));
                        testApxItem.APxInitialInterChPhaseList[0].SteppedTrackChannel = testApxItem.APxInterChPhaseList[0].SteppedTrackChannel = Convert.ToBoolean(dataTableReader.GetValue(4));
                        testApxItem.APxInitialInterChPhaseList[0].LevelCh1 = testApxItem.APxInterChPhaseList[0].LevelCh1 = dataTableReader.GetValue(5).ToString();
                        testApxItem.APxInitialInterChPhaseList[0].LevelCh2 = testApxItem.APxInterChPhaseList[0].LevelCh2 = dataTableReader.GetValue(6).ToString();
                        testApxItem.APxInitialInterChPhaseList[0].LevelCh3 = testApxItem.APxInterChPhaseList[0].LevelCh3 = dataTableReader.GetValue(7).ToString();
                        testApxItem.APxInitialInterChPhaseList[0].LevelCh4 = testApxItem.APxInterChPhaseList[0].LevelCh4 = dataTableReader.GetValue(8).ToString();
                        testApxItem.APxInitialInterChPhaseList[0].LevelCh5 = testApxItem.APxInterChPhaseList[0].LevelCh5 = dataTableReader.GetValue(9).ToString();
                        testApxItem.APxInitialInterChPhaseList[0].LevelCh6 = testApxItem.APxInterChPhaseList[0].LevelCh6 = dataTableReader.GetValue(10).ToString();
                        testApxItem.APxInitialInterChPhaseList[0].LevelCh7 = testApxItem.APxInterChPhaseList[0].LevelCh7 = dataTableReader.GetValue(11).ToString();
                        testApxItem.APxInitialInterChPhaseList[0].LevelCh8 = testApxItem.APxInterChPhaseList[0].LevelCh8 = dataTableReader.GetValue(12).ToString();

                        testApxItem.APxInitialInterChPhaseList[0].TxtFreqA = testApxItem.APxInterChPhaseList[0].TxtFreqA = dataTableReader.GetValue(13).ToString();

                        testApxItem.APxInitialInterChPhaseList[0].Isch1Enable = testApxItem.APxInterChPhaseList[0].Isch1Enable = Convert.ToBoolean(dataTableReader.GetValue(14));
                        testApxItem.APxInitialInterChPhaseList[0].Isch2Enable = testApxItem.APxInterChPhaseList[0].Isch2Enable = Convert.ToBoolean(dataTableReader.GetValue(15));
                        testApxItem.APxInitialInterChPhaseList[0].Isch3Enable = testApxItem.APxInterChPhaseList[0].Isch3Enable = Convert.ToBoolean(dataTableReader.GetValue(16));
                        testApxItem.APxInitialInterChPhaseList[0].Isch4Enable = testApxItem.APxInterChPhaseList[0].Isch4Enable = Convert.ToBoolean(dataTableReader.GetValue(17));
                        testApxItem.APxInitialInterChPhaseList[0].Isch5Enable = testApxItem.APxInterChPhaseList[0].Isch5Enable = Convert.ToBoolean(dataTableReader.GetValue(18));
                        testApxItem.APxInitialInterChPhaseList[0].Isch6Enable = testApxItem.APxInterChPhaseList[0].Isch6Enable = Convert.ToBoolean(dataTableReader.GetValue(19));
                        testApxItem.APxInitialInterChPhaseList[0].Isch7Enable = testApxItem.APxInterChPhaseList[0].Isch7Enable = Convert.ToBoolean(dataTableReader.GetValue(20));
                        testApxItem.APxInitialInterChPhaseList[0].Isch8Enable = testApxItem.APxInterChPhaseList[0].Isch8Enable = Convert.ToBoolean(dataTableReader.GetValue(21));

                        testApxItem.APxInitialInterChPhaseList[0].CmbRefChannelSelected = testApxItem.APxInterChPhaseList[0].CmbRefChannelSelected = dataTableReader.GetValue(22).ToString();
                        testApxItem.APxInitialInterChPhaseList[0].MeterRangeSelected = testApxItem.APxInterChPhaseList[0].MeterRangeSelected = dataTableReader.GetValue(23).ToString();
                    }
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

        private void SetSteppedFreqSweepSettings(int testCaseID, int testActionID, TestApxItem testApxItem)
        {
            try
            {
                string query = "Select * from APSteppedFreqSweepInitialSettings where TCID= " + testCaseID + " and ActionID =" + testActionID + "";
                DataTable dataTable = new DataTable();
                DataTableReader dataTableReader = null;
                SqlDataAdapter dataAdapter = new SqlDataAdapter(query, connect);
                dataAdapter.Fill(dataTable);
                dataTableReader = dataTable.CreateDataReader();
                if (dataTableReader.HasRows)
                {
                    while (dataTableReader.Read())
                    {
                        testApxItem.APxInitialSteppedFreqSweepList[0].OutChannelCount = Convert.ToInt32(dataTableReader.GetValue(18));
                        testApxItem.APxInitialSteppedFreqSweepList[0].InChCount = Convert.ToInt32(dataTableReader.GetValue(19));
                        testApxItem.APxInitialSteppedFreqSweepList[0].ChkGenON = Convert.ToBoolean(dataTableReader.GetValue(3));
                        testApxItem.APxInitialSteppedFreqSweepList[0].StartFrequency = dataTableReader.GetValue(4).ToString();
                        testApxItem.APxInitialSteppedFreqSweepList[0].StopFrequency = dataTableReader.GetValue(5).ToString();
                        testApxItem.APxInitialSteppedFreqSweepList[0].SelectedSweep = dataTableReader.GetValue(6).ToString();
                        testApxItem.APxInitialSteppedFreqSweepList[0].Steppedpoints = Convert.ToInt32(dataTableReader.GetValue(7));
                        testApxItem.APxInitialSteppedFreqSweepList[0].SteppedLevel = dataTableReader.GetValue(8).ToString();
                        testApxItem.APxInitialSteppedFreqSweepList[0].IsEnableCh1 = Convert.ToBoolean(dataTableReader.GetValue(9));
                        testApxItem.APxInitialSteppedFreqSweepList[0].IsEnableCh2 = Convert.ToBoolean(dataTableReader.GetValue(10));
                        testApxItem.APxInitialSteppedFreqSweepList[0].IsEnableCh3 = Convert.ToBoolean(dataTableReader.GetValue(11));
                        testApxItem.APxInitialSteppedFreqSweepList[0].IsEnableCh4 = Convert.ToBoolean(dataTableReader.GetValue(12));
                        testApxItem.APxInitialSteppedFreqSweepList[0].IsEnableCh5 = Convert.ToBoolean(dataTableReader.GetValue(13));
                        testApxItem.APxInitialSteppedFreqSweepList[0].IsEnableCh6 = Convert.ToBoolean(dataTableReader.GetValue(14));
                        testApxItem.APxInitialSteppedFreqSweepList[0].IsEnableCh7 = Convert.ToBoolean(dataTableReader.GetValue(15));
                        testApxItem.APxInitialSteppedFreqSweepList[0].IsEnableCh8 = Convert.ToBoolean(dataTableReader.GetValue(16));
                        testApxItem.APxInitialSteppedFreqSweepList[0].cmbPhaseRefChannel = dataTableReader.GetValue(17).ToString();
                    }
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

        private void SetInitialSteppedFreqSweepSettings(int testCaseID, int testActionID, TestApxItem testApxItem)
        {
            try
            {
                string query = "Select * from APSteppedFreqSweepInitialSettings where TCID= " + testCaseID + " and ActionID =" + testActionID + "";
                DataTable dataTable = new DataTable();
                DataTableReader dataTableReader = null;
                SqlDataAdapter dataAdapter = new SqlDataAdapter(query, connect);
                dataAdapter.Fill(dataTable);
                dataTableReader = dataTable.CreateDataReader();
                if (dataTableReader.HasRows)
                {
                    while (dataTableReader.Read())
                    {
                        testApxItem.APxInitialSteppedFreqSweepList[0].OutChannelCount = testApxItem.APxSteppedFreqSweepList[0].OutChannelCount = Convert.ToInt32(dataTableReader.GetValue(18));
                        testApxItem.APxInitialSteppedFreqSweepList[0].InChCount = testApxItem.APxSteppedFreqSweepList[0].InChCount = Convert.ToInt32(dataTableReader.GetValue(19));
                        testApxItem.APxInitialSteppedFreqSweepList[0].ChkGenON = testApxItem.APxSteppedFreqSweepList[0].ChkGenON = Convert.ToBoolean(dataTableReader.GetValue(3));
                        testApxItem.APxInitialSteppedFreqSweepList[0].StartFrequency = testApxItem.APxSteppedFreqSweepList[0].StartFrequency = dataTableReader.GetValue(4).ToString();
                        testApxItem.APxInitialSteppedFreqSweepList[0].StopFrequency = testApxItem.APxSteppedFreqSweepList[0].StopFrequency = dataTableReader.GetValue(5).ToString();
                        testApxItem.APxInitialSteppedFreqSweepList[0].SelectedSweep = testApxItem.APxSteppedFreqSweepList[0].SelectedSweep = dataTableReader.GetValue(6).ToString();
                        testApxItem.APxInitialSteppedFreqSweepList[0].Steppedpoints = testApxItem.APxSteppedFreqSweepList[0].Steppedpoints = Convert.ToInt32(dataTableReader.GetValue(7));
                        testApxItem.APxInitialSteppedFreqSweepList[0].SteppedLevel = testApxItem.APxSteppedFreqSweepList[0].SteppedLevel = dataTableReader.GetValue(8).ToString();
                        testApxItem.APxInitialSteppedFreqSweepList[0].IsEnableCh1 = testApxItem.APxSteppedFreqSweepList[0].IsEnableCh1 = Convert.ToBoolean(dataTableReader.GetValue(9));
                        testApxItem.APxInitialSteppedFreqSweepList[0].IsEnableCh2 = testApxItem.APxSteppedFreqSweepList[0].IsEnableCh2 = Convert.ToBoolean(dataTableReader.GetValue(10));
                        testApxItem.APxInitialSteppedFreqSweepList[0].IsEnableCh3 = testApxItem.APxSteppedFreqSweepList[0].IsEnableCh3 = Convert.ToBoolean(dataTableReader.GetValue(11));
                        testApxItem.APxInitialSteppedFreqSweepList[0].IsEnableCh4 = testApxItem.APxSteppedFreqSweepList[0].IsEnableCh4 = Convert.ToBoolean(dataTableReader.GetValue(12));
                        testApxItem.APxInitialSteppedFreqSweepList[0].IsEnableCh5 = testApxItem.APxSteppedFreqSweepList[0].IsEnableCh5 = Convert.ToBoolean(dataTableReader.GetValue(13));
                        testApxItem.APxInitialSteppedFreqSweepList[0].IsEnableCh6 = testApxItem.APxSteppedFreqSweepList[0].IsEnableCh6 = Convert.ToBoolean(dataTableReader.GetValue(14));
                        testApxItem.APxInitialSteppedFreqSweepList[0].IsEnableCh7 = testApxItem.APxSteppedFreqSweepList[0].IsEnableCh7 = Convert.ToBoolean(dataTableReader.GetValue(15));
                        testApxItem.APxInitialSteppedFreqSweepList[0].IsEnableCh8 = testApxItem.APxSteppedFreqSweepList[0].IsEnableCh8 = Convert.ToBoolean(dataTableReader.GetValue(16));
                        testApxItem.APxInitialSteppedFreqSweepList[0].cmbPhaseRefChannel = testApxItem.APxSteppedFreqSweepList[0].cmbPhaseRefChannel = dataTableReader.GetValue(17).ToString();
                    }
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

        private void SetTHDNSettings(int testCaseID, int testActionID, TestApxItem testApxItem)
        {
            try
            {
                string query = "Select * from APTHDNInitialSettings where TCID= " + testCaseID + " and ActionID =" + testActionID + "";
                DataTable dataTable = new DataTable();
                DataTableReader dataTableReader = null;
                SqlDataAdapter dataAdapter = new SqlDataAdapter(query, connect);
                dataAdapter.Fill(dataTable);
                dataTableReader = dataTable.CreateDataReader();
                if (dataTableReader.HasRows)
                {
                    while (dataTableReader.Read())
                    {
                        testApxItem.APxInitialTHDNList[0].OutChannelCount = Convert.ToInt32(dataTableReader.GetValue(26));
                        testApxItem.APxInitialTHDNList[0].ChkGenON = Convert.ToBoolean(dataTableReader.GetValue(3));
                        testApxItem.APxInitialTHDNList[0].chkBx_ThdnLevelTrackCh = Convert.ToBoolean(dataTableReader.GetValue(4));
                        testApxItem.APxInitialTHDNList[0].txtCh1Content = dataTableReader.GetValue(5).ToString();
                        testApxItem.APxInitialTHDNList[0].txtCh2Content = dataTableReader.GetValue(6).ToString();
                        testApxItem.APxInitialTHDNList[0].txtCh3Content = dataTableReader.GetValue(7).ToString();
                        testApxItem.APxInitialTHDNList[0].txtCh4Content = dataTableReader.GetValue(8).ToString();
                        testApxItem.APxInitialTHDNList[0].txtCh5Content = dataTableReader.GetValue(9).ToString();
                        testApxItem.APxInitialTHDNList[0].txtCh6Content = dataTableReader.GetValue(10).ToString();
                        testApxItem.APxInitialTHDNList[0].txtCh7Content = dataTableReader.GetValue(11).ToString();
                        testApxItem.APxInitialTHDNList[0].txtCh8Content = dataTableReader.GetValue(12).ToString();
                        testApxItem.APxInitialTHDNList[0].txt_THDfrequency = dataTableReader.GetValue(13).ToString();

                        testApxItem.APxInitialTHDNList[0].btn_THDCh1 = Convert.ToBoolean(dataTableReader.GetValue(14));
                        testApxItem.APxInitialTHDNList[0].btn_THDCh2 = Convert.ToBoolean(dataTableReader.GetValue(15));
                        testApxItem.APxInitialTHDNList[0].btn_THDCh3 = Convert.ToBoolean(dataTableReader.GetValue(16));
                        testApxItem.APxInitialTHDNList[0].btn_THDCh4 = Convert.ToBoolean(dataTableReader.GetValue(17));
                        testApxItem.APxInitialTHDNList[0].btn_THDCh5 = Convert.ToBoolean(dataTableReader.GetValue(18));
                        testApxItem.APxInitialTHDNList[0].btn_THDCh6 = Convert.ToBoolean(dataTableReader.GetValue(19));
                        testApxItem.APxInitialTHDNList[0].btn_THDCh7 = Convert.ToBoolean(dataTableReader.GetValue(20));
                        testApxItem.APxInitialTHDNList[0].btn_THDCh8 = Convert.ToBoolean(dataTableReader.GetValue(21));

                        testApxItem.APxInitialTHDNList[0].cmb_THDLowPassFilter = dataTableReader.GetValue(22).ToString();
                        testApxItem.APxInitialTHDNList[0].cmb_THDHighPassFilter = dataTableReader.GetValue(23).ToString();
                        testApxItem.APxInitialTHDNList[0].cmb_THDWeighting = dataTableReader.GetValue(24).ToString();
                        testApxItem.APxInitialTHDNList[0].cmb_THDTuningMode = dataTableReader.GetValue(25).ToString();
                        testApxItem.APxInitialTHDNList[0].TxtLowpass = dataTableReader.GetValue(27).ToString();
                        testApxItem.APxInitialTHDNList[0].TxtHighpass = dataTableReader.GetValue(28).ToString();
                        testApxItem.APxInitialTHDNList[0].txtFilterFrequency = dataTableReader.GetValue(29).ToString();
                    }
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

        private void SetInitialTHDNSettings(int testCaseID, int testActionID, TestApxItem testApxItem)
        {
            try
            {
                string query = "Select * from APTHDNInitialSettings where TCID= " + testCaseID + " and ActionID =" + testActionID + "";
                DataTable dataTable = new DataTable();
                DataTableReader dataTableReader = null;
                SqlDataAdapter dataAdapter = new SqlDataAdapter(query, connect);
                dataAdapter.Fill(dataTable);
                dataTableReader = dataTable.CreateDataReader();
                if (dataTableReader.HasRows)
                {
                    while (dataTableReader.Read())
                    {
                        testApxItem.APxInitialTHDNList[0].OutChannelCount = testApxItem.APxTHDNList[0].OutChannelCount = Convert.ToInt32(dataTableReader.GetValue(26));
                        testApxItem.APxInitialTHDNList[0].ChkGenON = testApxItem.APxTHDNList[0].ChkGenON = Convert.ToBoolean(dataTableReader.GetValue(3));
                        testApxItem.APxInitialTHDNList[0].chkBx_ThdnLevelTrackCh = testApxItem.APxTHDNList[0].chkBx_ThdnLevelTrackCh = Convert.ToBoolean(dataTableReader.GetValue(4));
                        testApxItem.APxInitialTHDNList[0].txtCh1Content = testApxItem.APxTHDNList[0].txtCh1Content = dataTableReader.GetValue(5).ToString();
                        testApxItem.APxInitialTHDNList[0].txtCh2Content = testApxItem.APxTHDNList[0].txtCh2Content = dataTableReader.GetValue(6).ToString();
                        testApxItem.APxInitialTHDNList[0].txtCh3Content = testApxItem.APxTHDNList[0].txtCh3Content = dataTableReader.GetValue(7).ToString();
                        testApxItem.APxInitialTHDNList[0].txtCh4Content = testApxItem.APxTHDNList[0].txtCh4Content = dataTableReader.GetValue(8).ToString();
                        testApxItem.APxInitialTHDNList[0].txtCh5Content = testApxItem.APxTHDNList[0].txtCh5Content = dataTableReader.GetValue(9).ToString();
                        testApxItem.APxInitialTHDNList[0].txtCh6Content = testApxItem.APxTHDNList[0].txtCh6Content = dataTableReader.GetValue(10).ToString();
                        testApxItem.APxInitialTHDNList[0].txtCh7Content = testApxItem.APxTHDNList[0].txtCh7Content = dataTableReader.GetValue(11).ToString();
                        testApxItem.APxInitialTHDNList[0].txtCh8Content = testApxItem.APxTHDNList[0].txtCh8Content = dataTableReader.GetValue(12).ToString();
                        testApxItem.APxInitialTHDNList[0].txt_THDfrequency = testApxItem.APxTHDNList[0].txt_THDfrequency = dataTableReader.GetValue(13).ToString();

                        testApxItem.APxInitialTHDNList[0].btn_THDCh1 = testApxItem.APxTHDNList[0].btn_THDCh1 = Convert.ToBoolean(dataTableReader.GetValue(14));
                        testApxItem.APxInitialTHDNList[0].btn_THDCh2 = testApxItem.APxTHDNList[0].btn_THDCh2 = Convert.ToBoolean(dataTableReader.GetValue(15));
                        testApxItem.APxInitialTHDNList[0].btn_THDCh3 = testApxItem.APxTHDNList[0].btn_THDCh3 = Convert.ToBoolean(dataTableReader.GetValue(16));
                        testApxItem.APxInitialTHDNList[0].btn_THDCh4 = testApxItem.APxTHDNList[0].btn_THDCh4 = Convert.ToBoolean(dataTableReader.GetValue(17));
                        testApxItem.APxInitialTHDNList[0].btn_THDCh5 = testApxItem.APxTHDNList[0].btn_THDCh5 = Convert.ToBoolean(dataTableReader.GetValue(18));
                        testApxItem.APxInitialTHDNList[0].btn_THDCh6 = testApxItem.APxTHDNList[0].btn_THDCh6 = Convert.ToBoolean(dataTableReader.GetValue(19));
                        testApxItem.APxInitialTHDNList[0].btn_THDCh7 = testApxItem.APxTHDNList[0].btn_THDCh7 = Convert.ToBoolean(dataTableReader.GetValue(20));
                        testApxItem.APxInitialTHDNList[0].btn_THDCh8 = testApxItem.APxTHDNList[0].btn_THDCh8 = Convert.ToBoolean(dataTableReader.GetValue(21));

                        testApxItem.APxInitialTHDNList[0].cmb_THDLowPassFilter = testApxItem.APxTHDNList[0].cmb_THDLowPassFilter = dataTableReader.GetValue(22).ToString();
                        testApxItem.APxInitialTHDNList[0].cmb_THDHighPassFilter = testApxItem.APxTHDNList[0].cmb_THDHighPassFilter = dataTableReader.GetValue(23).ToString();
                        testApxItem.APxInitialTHDNList[0].cmb_THDWeighting = testApxItem.APxTHDNList[0].cmb_THDWeighting = dataTableReader.GetValue(24).ToString();
                        testApxItem.APxInitialTHDNList[0].cmb_THDTuningMode = testApxItem.APxTHDNList[0].cmb_THDTuningMode = dataTableReader.GetValue(25).ToString();
                        testApxItem.APxInitialTHDNList[0].TxtLowpass = testApxItem.APxTHDNList[0].TxtLowpass = dataTableReader.GetValue(27).ToString();
                        testApxItem.APxInitialTHDNList[0].TxtHighpass = testApxItem.APxTHDNList[0].TxtHighpass = dataTableReader.GetValue(28).ToString();
                        testApxItem.APxInitialTHDNList[0].txtFilterFrequency = testApxItem.APxTHDNList[0].txtFilterFrequency = dataTableReader.GetValue(29).ToString();
                    }
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

        public int ReadTestPlanKeyForTestCase(TestCaseItem sourceTestCaseItem)
        {
            int planKey = 0;

            try
            {

                CreateConnection();
                OpenConnection();

                string query = "select * from TestCase where TestCaseID ='" + sourceTestCaseItem.TestCaseID + "'";
                DataTable dataTable = new DataTable();
                DataTableReader dataTableReader = null;
                SqlDataAdapter dataAdapter = new SqlDataAdapter(query, connect);
                dataAdapter.Fill(dataTable);
                dataTableReader = dataTable.CreateDataReader();

                if (dataTableReader.HasRows)
                {
                    if (dataTableReader.Read())
                    {
                        try
                        {
                            planKey = dataTableReader.GetInt32(2);
                        }
                        catch (Exception)
                        {
                            DeviceDiscovery.WriteToLogFile("No Test Plan is associated with Test Case: " + sourceTestCaseItem.TestItemName);
                            planKey = 0;
                        }
                    }
                }

                CloseConnection();
                return planKey;
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif

                //MessageBox.Show("Exception\n \n" + ex.Message, "QAT Error Code - ECxxxx", MessageBoxButton.OK, MessageBoxImage.Error);
                return planKey;
            }
        }

        public bool WriteTestPlanItemToDB(TestPlanItem sourceTestPlanItem)
        {
            try
            {
                string query = null;
                DataTable dataTable = null;
                DataTableReader dataTableReader = null;
                SqlCommand command = null;
                SqlDataAdapter dataAdapter = null;

                CreateConnection();
                OpenConnection();

                string itemTableName = QatConstants.DbTestPlanTable;
                string itemNameColumn = QatConstants.DbTestPlanNameColumn;

                List<string> designNameList = new List<string>();

                sourceTestPlanItem.TestItemName = sourceTestPlanItem.TestItemName.Trim();
                //if (sourceTestPlanItem.IsDesignChecked == true)
                //{
                if (sourceTestPlanItem.TestItemNameCopy != null && sourceTestPlanItem.TestItemNameCopy != string.Empty /*&& sourceTestPlanItem.IsNewTestDesign==false*/)
                {
                    if (sourceTestPlanItem.IsNewTestPlan == false && sourceTestPlanItem.IsNewTestDesign == false && (!String.Equals(sourceTestPlanItem.TestItemNameCopy, sourceTestPlanItem.TestItemName.Trim())))
                    {
                        sourceTestPlanItem.DesignName = sourceTestPlanItem.DesignName.Remove(0, sourceTestPlanItem.TestItemNameCopy.Length + 4);
                        sourceTestPlanItem.DesignName = "QAT_" + sourceTestPlanItem.TestItemName.Trim() + sourceTestPlanItem.DesignName;
                    }
                    else if (sourceTestPlanItem.IsDesignChecked && ((sourceTestPlanItem.IsNewTestPlan == true && (!String.Equals(sourceTestPlanItem.TestItemNameCopy, sourceTestPlanItem.TestItemName.Trim()))) ||
                               sourceTestPlanItem.IsNewTestDesign == true && (!String.Equals(sourceTestPlanItem.TestItemNameCopy, sourceTestPlanItem.TestItemName.Trim()))))
                    {
                        if (sourceTestPlanItem.DesignFileName != null && sourceTestPlanItem.DesignFileName != string.Empty)
                        {
                            sourceTestPlanItem.DesignName = "QAT_" + sourceTestPlanItem.TestItemName.Trim() + "_V" + (sourceTestPlanItem.DesignNameList.Count + 1).ToString() + "_" + (Path.GetFileNameWithoutExtension(sourceTestPlanItem.DesignFileName).Trim() + Path.GetExtension(sourceTestPlanItem.DesignFileName));
                        }
                    }
                }
                else
                {
                    if (sourceTestPlanItem.IsNewTestPlan == true && sourceTestPlanItem.IsDesignChecked)
                        sourceTestPlanItem.DesignName = "QAT_" + sourceTestPlanItem.TestItemName.Trim() + "_V" + (sourceTestPlanItem.DesignNameList.Count + 1).ToString() + "_" + (Path.GetFileNameWithoutExtension(sourceTestPlanItem.DesignFileName).Trim() + Path.GetExtension(sourceTestPlanItem.DesignFileName));
                }
                    if(sourceTestPlanItem.IsDesignChecked== false)
                       sourceTestPlanItem.DesignName = "QAT_" + sourceTestPlanItem.TestItemName.Trim() + "_V" + (sourceTestPlanItem.DesignNameList.Count + 1).ToString() + "_NO_QSYS_DESIGN";

                //}

                //if (sourceTestPlanItem.DesignName != null && sourceTestPlanItem.DesignName.Contains("NO design"))
                //    sourceTestPlanItem.IsNewTestDesign = false;

                //if (sourceTestPlanItem.IsDesignChecked == false)
                //{
                //    if(sourceTestPlanItem.IsNewTestDesign)
                //     sourceTestPlanItem.DesignName = "QAT_" + sourceTestPlanItem.TestItemName.Trim() + "_V" + (sourceTestPlanItem.DesignNameList.Count + 1).ToString() + "_No design_Test Script.qsys";
                //    else 
                //        sourceTestPlanItem.DesignName = "QAT_" + sourceTestPlanItem.TestItemName.Trim() + "_V" + (sourceTestPlanItem.DesignNameList.Count + 1).ToString() + "_No design_Test Script.qsys";
                //}               

                string ServerPath = QatConstants.QATServerPath + "\\Designs" + "\\";
                string name = Path.Combine(ServerPath, sourceTestPlanItem.DesignName);
                bool exception = fileinformation(name);
                if (exception == true)
                {
                    if (sourceTestPlanItem.IsNewTestPlan)
                    {
                        sourceTestPlanItem.TestItemNameCopy = sourceTestPlanItem.TestItemName;
                    }
                    else
                    {
                        sourceTestPlanItem.TestItemName = sourceTestPlanItem.TestItemNameCopy;
                        sourceTestPlanItem.DesignName = sourceTestPlanItem.DesignNameCopy;
                        sourceTestPlanItem.IsNewTestDesign = false;
                    }
                    return false;
                }

                if (sourceTestPlanItem.IsNewTestDesign)
                {
                    //try
                    //{
                    //if (Properties.Settings.Default.Path.ToString() != string.Empty)
                    //{
                    if (sourceTestPlanItem.IsDesignChecked == true)
                    {
                        string PreferencesServerPaths = QatConstants.QATServerPath + "\\Designs" + "\\";
                        //string PreferencesServerPath = Properties.Settings.Default.Path.ToString() + "\\";
                        if (File.Exists(sourceTestPlanItem.DesignFilePath))
                        {
                            System.IO.File.Copy(sourceTestPlanItem.DesignFilePath + "", PreferencesServerPaths + sourceTestPlanItem.DesignName + "");
                            FileInfo fileInformation = new FileInfo(PreferencesServerPaths + sourceTestPlanItem.DesignName + "");
                            fileInformation.IsReadOnly = true;
                        }
                    }

                    //}
                    //catch (Exception ex)
                    //{
                    //    if (Mouse.OverrideCursor != null)
                    //        Mouse.OverrideCursor = Cursors.Arrow;
                    //    if (ex.Message.Contains("There is not enough space on the disk"))
                    //        DBconnectionsMessageBox("Unable to update design file in the server path. There is not enough space on the disk.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    //    else
                    //        DBconnectionsMessageBox("Unable to update design file in the server path." + ex.Message, "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    //    return false;
                    //}
                }

                if (sourceTestPlanItem.IsNewTestPlan)
                {
                    
                    //else
                    //{
                        // Create new record and get the ID
                        DateTime? creationTime = DateTime.Now;
                        string creatorName = Properties.Settings.Default.TesterName.ToString().TrimEnd();
                        string categoryName = Properties.Settings.Default.Category.ToString().TrimEnd();//@categoryvalue
                        query = "Insert into " + itemTableName + " Values (@TPname,@Createdate,@CreateBy,'" + null + "','" + string.Empty + "','" + string.Empty + "',@categoryvalue,'" + sourceTestPlanItem.IsNoOfDeployChecked + "','" + sourceTestPlanItem.SelectedDeployItem + "', @EditedBy,'" + null + "','" + null + "','" + sourceTestPlanItem.TestCaseList.Count + "','"+ sourceTestPlanItem.IsDesignChecked + "')";
                        InsertCommandWithParameter2(query, "@TPname", sourceTestPlanItem.TestItemName.Trim(), "@Createdate", creationTime, "@CreateBy", creatorName, "@categoryvalue", categoryName, "@EditedBy", creatorName);
                    	sourceTestPlanItem.Createdon = creationTime;
                    	sourceTestPlanItem.Createdby = creatorName;
                    	sourceTestPlanItem.Category = categoryName;

                        //query = "Insert into " + itemTableName + " Values (@TPname)";
                        //InsertCommandWithParameter(query, "@TPname", sourceTestPlanItem.TestItemName.Trim());

                        query = "select * from " + itemTableName + " where " + itemNameColumn + "=@TPname";
                        dataTable = new DataTable();
                        dataTable = SelectDTWithParameter(query, "@TPname", sourceTestPlanItem.TestItemName.Trim());
                        dataTableReader = dataTable.CreateDataReader();

                        if (dataTableReader.HasRows)
                        {
                            if (dataTableReader.Read())
                                sourceTestPlanItem.TestPlanID = dataTableReader.GetInt32(0);
                        }

                        sourceTestPlanItem.TestItemNameCopy = sourceTestPlanItem.TestItemName.Trim();
                    //}
                }
                else
                {
                    //sourceTestPlanItem.DesignName = sourceTestPlanItem.DesignName.Remove(0, sourceTestPlanItem.TestItemNameCopy.Length + 4);
                    //sourceTestPlanItem.DesignName = "QAT_" + sourceTestPlanItem.TestItemName.Trim() + sourceTestPlanItem.DesignName;

                    //string ServerPath = QatConstants.QATServerPath + "\\Designs" + "\\";
                    //string name = Path.Combine(ServerPath, sourceTestPlanItem.DesignName);
                    //bool exception = fileinformation(name);

                    //if (exception == false)
                    //{ 
                        // Modify record 
                    DateTime? modifiedTime = DateTime.Now;
                    string editorName = Properties.Settings.Default.TesterName.ToString().TrimEnd();
                    string categoryName = GetCategoryNameFromDB(sourceTestPlanItem.TestPlanID, itemTableName);
                    query = "update " + itemTableName + " set " + itemNameColumn + " =@TPname,ModifiedOn=@editdate,ModifiedBy=@editBy,category=@categoryvalue,IsDeployEnable='" + sourceTestPlanItem.IsNoOfDeployChecked + "',DeployCount='" + sourceTestPlanItem.SelectedDeployItem + "',EditedBy=@editedBy,TPActioncount = '" + sourceTestPlanItem.TestCaseList.Count + "',IsDesign='" + sourceTestPlanItem.IsDesignChecked + "' where TestPlanID = '" + sourceTestPlanItem.TestPlanID + "'";
                    InsertCommandWithParameter2(query, "@TPname", sourceTestPlanItem.TestItemName.Trim(), "@editdate", modifiedTime, "@editBy", editorName, "@categoryvalue", categoryName, "@editedBy", editorName);
                    sourceTestPlanItem.Modifiedon = modifiedTime;
                    sourceTestPlanItem.Modifiedby = editorName;
                    sourceTestPlanItem.Category = categoryName;

                    //query = "update " + itemTableName + " set " + itemNameColumn + " =@TPname where TestPlanID = '" + sourceTestPlanItem.TestPlanID + "'";
                    //InsertCommandWithParameter(query, "@TPname", sourceTestPlanItem.TestItemName.Trim());

                    query = "delete from TPTCLinkTable where TPID = '" + sourceTestPlanItem.TestPlanID + "'";
                    command = new SqlCommand(query, connect);
                    command.ExecuteScalar();

                    if (!String.Equals(sourceTestPlanItem.TestItemNameCopy, sourceTestPlanItem.TestItemName.Trim()))
                    {
                        query = "Select Designname FROM designtable where TPID='" + sourceTestPlanItem.TestPlanID + "'";

                        dataTable = new DataTable();
                        dataTableReader = null;
                        dataAdapter = new SqlDataAdapter(query, connect);
                        dataAdapter.Fill(dataTable);
                        dataTableReader = dataTable.CreateDataReader();

                        if (dataTableReader.HasRows)
                        {
                            while (dataTableReader.Read())
                                designNameList.Add(dataTableReader.GetString(0).ToString());
                        }

                        foreach (string oldDesignName in designNameList)
                        {
                            string PreferencesServerPath = QatConstants.QATServerPath + "\\Designs" + "\\";
                            //string PreferencesServerPath = Properties.Settings.Default.Path.ToString() + "\\";
                            string newDesignName = null;

                            try
                            {
                                if (oldDesignName.Contains(sourceTestPlanItem.TestItemNameCopy))
                                {
                                    newDesignName = oldDesignName.Remove(0, sourceTestPlanItem.TestItemNameCopy.Length + 4);
                                    newDesignName = "QAT_" + sourceTestPlanItem.TestItemName.Trim() + newDesignName;
                                }
                                else
                                {
                                    newDesignName = oldDesignName;
                                }
                            }
                            catch(Exception ex){ }                       
                         
                            string name1 = Path.Combine(PreferencesServerPath, newDesignName);
                            try
                            {
                                FileInfo sample = new FileInfo(name1);

                                if (PreferencesServerPath != string.Empty && oldDesignName != string.Empty && oldDesignName != null)
                                {
                                    if (File.Exists(PreferencesServerPath + oldDesignName))
                                    {
                                        System.IO.File.Move(PreferencesServerPath + oldDesignName + "", PreferencesServerPath + newDesignName + "");
                                        FileInfo fileInformation = new FileInfo(PreferencesServerPath + newDesignName + "");
                                        fileInformation.IsReadOnly = true;
                                    }

                                //query = "update designtable set DesignName = @newDesign where Designname = '" + oldDesignName + "'";
                                //InsertCommandWithParameter(query, "@newDesign", newDesignName);

                                    query = "update designtable set DesignName = @newDesign where Designname = @oldDesign";
                                    InsertCommandWith2Parameters(query, "@newDesign", newDesignName, "@oldDesign", oldDesignName);
                                }
                            }
                            catch (Exception ex) { }
                        }
                    }
                //}
                //    else
                //    {
                //        sourceTestPlanItem.TestItemName = sourceTestPlanItem.TestItemNameCopy;
                //        sourceTestPlanItem.DesignName = sourceTestPlanItem.DesignNameCopy;
                //        return false;
                //    }
                }

                if (sourceTestPlanItem.IsNewTestDesign)
                {
                    //sourceTestPlanItem.DesignName = "QAT_" + sourceTestPlanItem.TestItemName.Trim() + "_V" + (sourceTestPlanItem.DesignNameList.Count + 1).ToString() + "_" + sourceTestPlanItem.DesignFileName;

                    query = "insert into designtable values (@designName, '" + sourceTestPlanItem.TestPlanID + "')";
                    InsertCommandWithParameter(query, "@designName", sourceTestPlanItem.DesignName);

                    query = "select designid from designtable where designname=@designName";
                    dataTable = SelectDTWithParameter(query, "@designName", sourceTestPlanItem.DesignName);
                    dataTableReader = dataTable.CreateDataReader();

                    if (dataTableReader.HasRows)
                    {
                        if (dataTableReader.Read())
                            sourceTestPlanItem.DesignID = dataTableReader.GetInt32(0);
                    }

                    if (sourceTestPlanItem.IsNewTestPlan)
                    {
                        query = "Insert into TPDesignLinkTable values('" + sourceTestPlanItem.TestPlanID + "','" + sourceTestPlanItem.DesignID + "')";
                        command = new SqlCommand(query, connect);
                        command.ExecuteScalar();
                    }
                    else
                    {
                        //query = "select Designname from designtable where DesignID in (select DesignID from TPDesignLinkTable where TPID = '" + sourceTestPlanItem.TestPlanID + "')";
                        //DataTable dataTable1 = SelectDTWithParameter(query, string.Empty, string.Empty);
                        //DataTableReader dataTableReader1 = dataTable1.CreateDataReader();
                        //if (dataTableReader1.HasRows)
                        //{
                        //    if (dataTableReader1.Read())
                        //    {
                        //        string designName = dataTableReader1.GetValue(0).ToString();

                        //        if(designName != null && designName != string.Empty)
                        //       {
                        //            try
                        //            {
                        //                string PreferencesServer = Path.Combine(QatConstants.QATServerPath, "Designs", designName);

                        //                if (File.Exists(PreferencesServer))
                        //                {
                        //                    FileInfo fileInformation = new FileInfo(PreferencesServer);
                        //                    fileInformation.IsReadOnly = false;
                        //                    File.Delete(PreferencesServer);
                        //                }
                        //            }
                        //            catch { }
                        //        }
                        //   }
                        //}

                        query = "update TPDesignLinkTable set DesignID = " + sourceTestPlanItem.DesignID + " where TPID = '" + sourceTestPlanItem.TestPlanID + "'";
                        command = new SqlCommand(query, connect);
                        command.ExecuteScalar();

                        //query = "Delete from designtable where TPID = '" + sourceTestPlanItem.TestPlanID + "' and DesignID not in ('" + sourceTestPlanItem.DesignID + "')";
                        //InsertCommandWithParameter(query, string.Empty, string.Empty);
                        if (sourceTestPlanItem.IsDesignChecked == true)
                        {
                            query = "Delete from TCInitialization where DesignID in (select DesignID from designtable where TPID = '" + sourceTestPlanItem.TestPlanID + "' and DesignID not in (select DesignID from TPDesignLinkTable where TPID = '" + sourceTestPlanItem.TestPlanID + "'))";
                            InsertCommandWithParameter(query, string.Empty, string.Empty);
                        }
                    }

                    if (sourceTestPlanItem.IsDesignChecked == true)
                    {
                        if (sourceTestPlanItem.DesignComponent.Rows.Count > 0)
                        {
                            for (int i = 0; i < sourceTestPlanItem.DesignComponent.Rows.Count; i++)
                            {
                                sourceTestPlanItem.DesignComponent.Rows[i][0] = sourceTestPlanItem.DesignID;
                            }

                            using (SqlBulkCopy bulkcopy = new SqlBulkCopy(connect))
                            {
                                bulkcopy.DestinationTableName = "dbo.TCInitialization";
                                bulkcopy.WriteToServer(sourceTestPlanItem.DesignComponent);
                            }
                        }

                        string[] inventory_list = sourceTestPlanItem.DesignInventory.ToArray();
                        string value_String = string.Empty;
                        string value_String1 = string.Empty;
                        for (int j = 0; j < inventory_list.Length - 1; j++)
                        {
                            value_String = "('" + sourceTestPlanItem.DesignID + "'," + inventory_list[j];
                            value_String1 += value_String + ",";
                        }

                        value_String = "('" + sourceTestPlanItem.DesignID + "'," + inventory_list[inventory_list.Length - 1];
                        value_String1 += value_String;

                        query = "Insert into DesignInventory(DesignID,DeviceType,DeviceModel,DeviceNameInDesign,PrimaryorBackup,Backup_to_primary) values" + value_String1;
                        command = new SqlCommand(query, connect);
                        command.ExecuteScalar();
                    }
                   
                    //try
                    //{
                    //    //if (Properties.Settings.Default.Path.ToString() != string.Empty)
                    //    //{
                    //    string PreferencesServerPath = QatConstants.QATServerPath + "\\Designs" + "\\";
                    //    //string PreferencesServerPath = Properties.Settings.Default.Path.ToString() + "\\";
                    //    if (File.Exists(sourceTestPlanItem.DesignFilePath))
                    //    {
                    //        System.IO.File.Copy(sourceTestPlanItem.DesignFilePath + "", PreferencesServerPath + sourceTestPlanItem.DesignName + "");
                    //        FileInfo fileInformation = new FileInfo(PreferencesServerPath + sourceTestPlanItem.DesignName + "");
                    //        fileInformation.IsReadOnly = true;
                    //    }
                    //}
                    //catch (Exception ex)
                    //{
                    //    if (Mouse.OverrideCursor != null)
                    //        Mouse.OverrideCursor = Cursors.Arrow;
                    //    if (ex.Message.Contains("There is not enough space on the disk"))
                    //        DBconnectionsMessageBox("Unable to update design file in the server path. There is not enough space on the disk.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    //    else
                    //        DBconnectionsMessageBox("Unable to update design file in the server path." + ex.Message, "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    //    return false;
                    //}
                    //}
                }

                if (sourceTestPlanItem.TestCaseList.Count > 0)
                {
                    //string queryValues = null;
                    
                    var TPTCLinkQueryTable = new DataTable();
                    TPTCLinkQueryTable.Columns.Add("TPID", typeof(int));
                    TPTCLinkQueryTable.Columns.Add("TCID", typeof(int));
                    foreach (TreeViewExplorer item in sourceTestPlanItem.TestCaseList)
                    {
                        query = "Select * from " + QatConstants.DbTestCaseTable + " where TestcaseID = " + item.ItemKey;
                        SqlCommand cmd = new SqlCommand(query, connect);
                        SqlDataAdapter adap = new SqlDataAdapter(cmd);
                        DataTable tbl = new DataTable();
                        adap.Fill(tbl);
                        if (tbl.Rows.Count > 0)
                        {
                            TPTCLinkQueryTable.Rows.Add(sourceTestPlanItem.TestPlanID.ToString(), item.ItemKey.ToString());
                        }
                    }
                    using (var bulkCopy = new SqlBulkCopy(connect))
                    {
                        bulkCopy.DestinationTableName = "dbo.TPTCLinkTable";
                        bulkCopy.WriteToServer(TPTCLinkQueryTable);
                    }
                }

                sourceTestPlanItem.TestItemNameCopy = sourceTestPlanItem.TestItemName.Trim();
                sourceTestPlanItem.IsNewTestPlan = false;
                sourceTestPlanItem.DesignNameCopy = sourceTestPlanItem.DesignName;
                sourceTestPlanItem.IsNewTestDesign = false;
                sourceTestPlanItem.DesignNameList = GetDesignListFromDB(sourceTestPlanItem.TestItemName, QatConstants.DbTestPlanTable);

                CloseConnection();

                return true;
            }
            catch (Exception ex)
            {
                if(!ex.Message.Contains("There is not enough space on the disk"))
                {
                    if (sourceTestPlanItem.IsNewTestDesign)
                    {
                        try
                        {
                            string PreferencesServerPaths = QatConstants.QATServerPath + "\\Designs" + "\\";
                            //string PreferencesServerPath = Properties.Settings.Default.Path.ToString() + "\\";
                            if (File.Exists(PreferencesServerPaths + sourceTestPlanItem.DesignName))
                            {
                                FileInfo fileInformation = new FileInfo(PreferencesServerPaths + sourceTestPlanItem.DesignName);
                                fileInformation.IsReadOnly = false;
                                System.IO.File.Delete(PreferencesServerPaths + sourceTestPlanItem.DesignName);
                            }
                        }
                        catch (Exception exc) { }

                        if (Mouse.OverrideCursor != null)
                            Mouse.OverrideCursor = Cursors.Arrow;
                        DBconnectionsMessageBox("Unable to update design file in the server path.\n" + ex.Message, "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
                else
                {
                    if (Mouse.OverrideCursor != null)
                        Mouse.OverrideCursor = Cursors.Arrow;
                    DBconnectionsMessageBox("Unable to update design file in the server path.\nThere is not enough space on the disk.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                }

                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif

                //MessageBox.Show("Exception\n \n" + ex.Message, "QAT Error Code - EC02026", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        public bool WriteTestSuiteItemToDB(TestSuiteItem sourceTestSuiteItem)
        {
            try
            {
                string query = null;
                DataTable dataTable = null;
                DataTableReader dataTableReader = null;
                SqlCommand command = null;

                CreateConnection();
                OpenConnection();

                string itemTableName = QatConstants.DbTestSuiteTable;
                string itemNameColumn = QatConstants.DbTestSuiteNameColumn;

                sourceTestSuiteItem.TestItemName = sourceTestSuiteItem.TestItemName.Trim();

                if (sourceTestSuiteItem.IsNewTestSuite)
                {
                    // Create new record and get the ID
                    DateTime? creationTime = DateTime.Now;
                    string creatorName = Properties.Settings.Default.TesterName.ToString().TrimEnd();
                    string categoryName = Properties.Settings.Default.Category.ToString().TrimEnd();
                    query = "Insert into " + itemTableName + " Values (@TSname,@Createdate,@CreateBy,'" + null + "','" + string.Empty + "','" + string.Empty + "',@categoryvalue,@EditedBy,'" + sourceTestSuiteItem.TestPlanList.Count + "')";
                    InsertCommandWithParameter2(query, "@TSname", sourceTestSuiteItem.TestItemName.Trim(),"@Createdate", creationTime, "@CreateBy", creatorName, "@categoryvalue", categoryName, "@EditedBy", creatorName);
                    sourceTestSuiteItem.Createdon = creationTime;
                    sourceTestSuiteItem.Createdby = creatorName;
                    sourceTestSuiteItem.Category = categoryName;

                    query = "select * from " + itemTableName + " where " + itemNameColumn + "=@TSname";
                    dataTable = new DataTable();
                    dataTable = SelectDTWithParameter(query, "@TSname", sourceTestSuiteItem.TestItemName.Trim());
                    dataTableReader = dataTable.CreateDataReader();

                    if (dataTableReader.HasRows)
                    {
                        if (dataTableReader.Read())
                            sourceTestSuiteItem.TestSuiteID = dataTableReader.GetInt32(0);
                    }

                    sourceTestSuiteItem.TestItemNameCopy = sourceTestSuiteItem.TestItemName.Trim();
                    sourceTestSuiteItem.IsNewTestSuite = false;
                    
                }
                else
                {
                    // Modify record 
                    DateTime? modifiedTime = DateTime.Now;
                    string editorName = Properties.Settings.Default.TesterName.ToString().TrimEnd();
                    string categoryName = GetCategoryNameFromDB(sourceTestSuiteItem.TestSuiteID, itemTableName);

                    query = "update " + itemTableName + " set " + itemNameColumn + " =@TSname,ModifiedOn=@editdate,ModifiedBy=@editBy,Category=@categoryvalue,EditedBy=@editedBy,TSActioncount= '" + sourceTestSuiteItem.TestPlanList.Count + "' where TestSuiteID = '" + sourceTestSuiteItem.TestSuiteID + "'";
                    InsertCommandWithParameter2(query, "@TSname", sourceTestSuiteItem.TestItemName.Trim(), "@editdate", modifiedTime, "@editBy", editorName, "@categoryvalue", categoryName, "@editedBy", editorName);
                    sourceTestSuiteItem.Modifiedon = modifiedTime;
                    sourceTestSuiteItem.Modifiedby = editorName;
                    sourceTestSuiteItem.Category = categoryName;

                    sourceTestSuiteItem.TestItemNameCopy = sourceTestSuiteItem.TestItemName.Trim();
                    query = "delete from TSTPLinkTable where TSID = '" + sourceTestSuiteItem.TestSuiteID + "'";
                    command = new SqlCommand(query, connect);
                    command.ExecuteScalar();

                }

                if (sourceTestSuiteItem.TestPlanList.Count > 0)
                {
                    //string queryValues = null;

                    //foreach (TreeViewExplorer item in sourceTestSuiteItem.TestPlanList)
                    //{
                    //    query = "Select * from " + QatConstants.DbTestPlanTable + " where TestPlanID = " + item.ItemKey;
                    //    SqlCommand cmd = new SqlCommand(query, connect);
                    //    SqlDataAdapter adap = new SqlDataAdapter(cmd);
                    //    DataTable tbl = new DataTable();
                    //    adap.Fill(tbl);
                    //    if (tbl.Rows.Count > 0)
                    //    {
                    //        queryValues += "(" + sourceTestSuiteItem.TestSuiteID.ToString() + "," + item.ItemKey.ToString() + "),";
                    //    }
                    //}
                    //if (queryValues != string.Empty && queryValues != null)
                    //{
                    //    queryValues = queryValues.TrimEnd(',');

                    //    query = "Insert into TSTPLinkTable ( TSID, TPID ) values" + queryValues;
                    //    command = new SqlCommand(query, connect);
                    //    command.ExecuteScalar();
                    //}
                    var TSTPLinkQueryTable = new DataTable();
                    TSTPLinkQueryTable.Columns.Add("TSID", typeof(int));
                    TSTPLinkQueryTable.Columns.Add("TPID", typeof(int));

                    foreach (TreeViewExplorer item in sourceTestSuiteItem.TestPlanList)
                    {
                        query = "Select * from " + QatConstants.DbTestPlanTable + " where TestPlanID = " + item.ItemKey;
                        SqlCommand cmd = new SqlCommand(query, connect);
                        SqlDataAdapter adap = new SqlDataAdapter(cmd);
                        DataTable tbl = new DataTable();
                        adap.Fill(tbl);
                        if (tbl.Rows.Count > 0)
                        {
                            TSTPLinkQueryTable.Rows.Add(sourceTestSuiteItem.TestSuiteID.ToString(), item.ItemKey.ToString());
                        }
                    }
                    using (var bulkCopy = new SqlBulkCopy(connect))
                    {
                        bulkCopy.DestinationTableName = "dbo.TSTPLinkTable";
                        bulkCopy.WriteToServer(TSTPLinkQueryTable);
                    }
                }

                CloseConnection();

                return true;
            }
            catch (Exception ex)
            {

                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                //MessageBox.Show("Exception\n \n" + ex.Message, "QAT Error Code - EC02027", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }
        
        public void InsertCommandWithParameter(string query, string parameter, string parmValue)
        {
            try
            {
                DataTable tble = new DataTable();
                SqlDataAdapter adap = new SqlDataAdapter();

                if (connect == null)
                    CreateConnection();

                SqlCommand cmd = new SqlCommand(query, connect);
                cmd.Parameters.AddWithValue(parameter, parmValue);
                adap = new SqlDataAdapter(cmd);
                adap.Update(tble);
                adap.Fill(tble);
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                //MessageBox.Show("Exception\n \n" + ex.Message, "QAT Error Code - EC02028", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void InsertCommandWithParameter1(string query, string parameter, string parmValue, string parameter1, DateTime? parmValue1, string parameter2, string parmValue2, string parameter3, string parmValue3)
        {
            try
            {
                DataTable tble = new DataTable();
                SqlDataAdapter adap = new SqlDataAdapter();

                if (connect == null)
                    CreateConnection();

                SqlCommand cmd = new SqlCommand(query, connect);
                cmd.Parameters.AddWithValue(parameter, parmValue);
                cmd.Parameters.AddWithValue(parameter1, parmValue1);
                cmd.Parameters.AddWithValue(parameter2, parmValue2);
                cmd.Parameters.AddWithValue(parameter3, parmValue3);
                adap = new SqlDataAdapter(cmd);
                adap.Update(tble);
                adap.Fill(tble);
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                //MessageBox.Show("Exception\n \n" + ex.Message, "QAT Error Code - EC02029", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void InsertCommandWithParameter2(string query, string parameter, string parmValue, string parameter1, DateTime? parmValue1, string parameter2, string parmValue2, string parameter3, string parmValue3, string parameter4, string parmValue4)
        {
            try
            {
                DataTable tble = new DataTable();
                SqlDataAdapter adap = new SqlDataAdapter();

                if (connect == null)
                    CreateConnection();

                SqlCommand cmd = new SqlCommand(query, connect);
                cmd.Parameters.AddWithValue(parameter, parmValue);
                cmd.Parameters.AddWithValue(parameter1, parmValue1);
                cmd.Parameters.AddWithValue(parameter2, parmValue2);
                cmd.Parameters.AddWithValue(parameter3, parmValue3);
                cmd.Parameters.AddWithValue(parameter4, parmValue4);
                adap = new SqlDataAdapter(cmd);
                adap.Update(tble);
                adap.Fill(tble);
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                //MessageBox.Show("Exception\n \n" + ex.Message, "QAT Error Code - EC02030", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void InsertCommandWith2Parameters(string query, string parameter, string parmValue, string parameter2, string parmValue2)
        {
            try
            {
                DataTable tble = new DataTable();
                SqlDataAdapter adap = new SqlDataAdapter();

                if (connect == null)
                    CreateConnection();

                SqlCommand cmd = new SqlCommand(query, connect);
                cmd.Parameters.AddWithValue(parameter, parmValue);
                cmd.Parameters.AddWithValue(parameter2, parmValue2);
                adap = new SqlDataAdapter(cmd);
                adap.Update(tble);
                adap.Fill(tble);
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                //MessageBox.Show("Exception\n \n" + ex.Message, "QAT Error Code - EC02031", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public DataTable SelectDTWithParameter(string query, string parameter, string parmValue)
        {
            DataTable tble = new DataTable();
            try
            {
                if (connect == null)
                    CreateConnection();

                SqlCommand cmd = new SqlCommand(query, connect);

                if (parameter != null)
                    cmd.Parameters.AddWithValue(parameter, parmValue);

                SqlDataAdapter dataAdapter = new SqlDataAdapter(cmd);
                dataAdapter.Fill(tble);
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                //MessageBox.Show("Exception\n \n" + ex.Message, "QAT Error Code - EC02032", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return tble;
        }

        public Tuple<string, bool> GetDesignNameFromDB(string itemName, string itemType)
        {
            string designName = string.Empty;
            bool IsDesignChecked = true;
            string query = string.Empty;
            try
            {
                CreateConnection();
                OpenConnection();

                if (itemType == QatConstants.DbTestPlanTable)
                {
                    query = "select * from designtable where DesignID in(select DesignID from TPDesignLinkTable where TPID in(select TestPlanID from Testplan where Testplanname in (@TPname)))";
                }
                else if (itemType == QatConstants.DbTestCaseTable)
                {
                    query = "select * from designtable where DesignID in(select DesignID from TPDesignLinkTable where TPID in(select TPID from Testcase where Testcasename in (@TPname)))";
                }
                DataTable tble = SelectDTWithParameter(query, "@TPname", itemName);
                //DataTable tble = QscDatabase.SendCommand_Toreceive(query);
                DataTableReader read = tble.CreateDataReader();
                while (read.Read())
                {
                    designName = read[1].ToString();
                }

                if (itemType == QatConstants.DbTestPlanTable)
                    query = "Select IsDesign from Testplan where TestPlanID in(select TestPlanID from Testplan where Testplanname in (@TPname))";
                else  if (itemType == QatConstants.DbTestCaseTable)
                    query = "Select IsDesign from Testplan where TestPlanID in(select TPID from Testcase where Testcasename in (@TPname))";
                tble = SelectDTWithParameter(query, "@TPname", itemName);
                DataTableReader read1 = tble.CreateDataReader();
                while (read1.Read())
                {
                    if(read1[0]!= System.DBNull.Value)
                        IsDesignChecked = Convert.ToBoolean(read1[0].ToString());
                }

                CloseConnection();
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                //MessageBox.Show("Exception\n \n" + ex.Message, "QAT Error Code - EC02033", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return new Tuple<string, bool>( designName, IsDesignChecked);
        }
        public Dictionary<int,string> ReadDesignerNameList()
        {
       
            string query = string.Empty;
            Dictionary<int, string> designNameList = new Dictionary<int, string>();
            try
            {
                CreateConnection();
                OpenConnection();

                query = "SELECT designtable.TPID,designtable.Designname FROM designtable INNER JOIN TPDesignLinkTable ON designtable.DesignID = TPDesignLinkTable.DesignID";
                DataTable dataTable = new DataTable();
                SqlDataAdapter dataAdapter = new SqlDataAdapter(query, connect);
                dataAdapter.Fill(dataTable);
                DataTableReader dataTableReader = dataTable.CreateDataReader();
                while (dataTableReader.Read())
                {
                    if (dataTableReader[0] != System.DBNull.Value && dataTableReader[1] != System.DBNull.Value)
                        designNameList.Add(dataTableReader.GetInt32(0), dataTableReader.GetString(1));

                }

                CloseConnection();
            }
            catch (Exception ex)
            {
                new Dictionary<int, string>();
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                //MessageBox.Show("Exception\n \n" + ex.Message, "QAT Error Code - EC02033", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return designNameList;
        }
        public Dictionary<int,ArrayList> ReadLinkTable(string parentLinkTable)
        {
            Dictionary<int, ArrayList> linkTable_values = new Dictionary<int, ArrayList>();
            try
            {
                CreateConnection();
                OpenConnection();
                string query = string.Empty;
                string parentIDColumn = null;
                string childIDColumn = null;
           

                if (parentLinkTable == QatConstants.DbTestSuiteTestPlanLinkTable)
                {
                    parentIDColumn = QatConstants.DbTestSuiteLinkTableID;
                    childIDColumn = QatConstants.DbTestPlanLinkTableID;
                  

                }
                else if (parentLinkTable == QatConstants.DbTestPlanTestCaseLinkTable)
                {                    
                    parentIDColumn = QatConstants.DbTestPlanLinkTableID;
                    childIDColumn = QatConstants.DbTestCaseLinkTableID;
                   
                }
               
                
                query = "SELECT " + parentIDColumn + ","+ childIDColumn + " FROM "+parentLinkTable;
                DataTable dataTable = new DataTable();
                SqlDataAdapter dataAdapter = new SqlDataAdapter(query, connect);
                dataAdapter.Fill(dataTable);
                DataTableReader dataTableReader = dataTable.CreateDataReader();
                if (dataTableReader.HasRows)
                {
                    while (dataTableReader.Read())
                    {

                        if (dataTableReader[0] != System.DBNull.Value && dataTableReader[1] != System.DBNull.Value)
                        {
                          
                            if (!linkTable_values.ContainsKey(dataTableReader.GetInt32(0)))
                             {
                                ArrayList childList = new ArrayList();
                                childList.Add(dataTableReader.GetInt32(1));
                                linkTable_values.Add(dataTableReader.GetInt32(0), childList);
                               
                            }
                            else
                            {
                                linkTable_values[dataTableReader.GetInt32(0)].Add(dataTableReader.GetInt32(1));
                                    
                            }
                          
                        }
                    }
                }

                CloseConnection();
            }
            catch (Exception ex)
            {
               
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                //MessageBox.Show("Exception\n \n" + ex.Message, "QAT Error Code - EC02033", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return linkTable_values;
        }

        public string GetCategoryNameFromDB(Int32 itemKey, string itemType)
        {
            string categoryName = string.Empty;
            string query = string.Empty;
            try
            {
                CreateConnection();
                OpenConnection();

                if (itemType == QatConstants.DbTestSuiteTable)
                {
                    query = "select Category from Testsuite where TestSuiteID="+ itemKey;
                }
                else if (itemType == QatConstants.DbTestPlanTable)
                {
                    query = "select Category from Testplan where TestPlanID=" + itemKey;
                }
                else if (itemType == QatConstants.DbTestCaseTable)
                {
                    query = "select Category from Testcase where TestCaseID=" + itemKey;
                }
                if (connect == null)
                    CreateConnection();
                DataTable tble = new DataTable();
                SqlCommand cmd = new SqlCommand(query, connect);
                SqlDataAdapter dataAdapter = new SqlDataAdapter(cmd);
                dataAdapter.Fill(tble);
                
                //DataTable tble = QscDatabase.SendCommand_Toreceive(query);
                DataTableReader read = tble.CreateDataReader();
                while (read.Read())
                {
                    if (read[0] != System.DBNull.Value)
                    {
                        categoryName = read[0].ToString();
                    }
                        
                }

                //CloseConnection();
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                //MessageBox.Show("Exception\n \n" + ex.Message, "QAT Error Code - EC02033", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return categoryName;
        }

        public List<string> GetDesignListFromDB(string itemName, string itemType)
        {
            List<string> designList = new List<string>();
            string query = string.Empty;
            try
            {
                CreateConnection();
                OpenConnection();

                if (itemType == QatConstants.DbTestPlanTable)
                {
                    query = "select * from designtable where TPID in(select TestPlanID from Testplan where Testplanname in (@TPname))";
                }
                else if (itemType == QatConstants.DbTestCaseTable)
                {
                    query = "select * from designtable where TPID in(select TPID from Testcase where Testcasename in (@TPname))";
                }
                DataTable tble = SelectDTWithParameter(query, "@TPname", itemName);
                //DataTable tble = QscDatabase.SendCommand_Toreceive(query);
                DataTableReader read = tble.CreateDataReader();
                while (read.Read())
                {
                    designList.Add(read[1].ToString());
                }

                CloseConnection();
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                //MessageBox.Show("Exception\n \n" + ex.Message, "QAT Error Code - EC02034", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return designList;
        }

        public int ReadTreeTableChildrenCount(string childTableName, int parentPrimaryKey)
        {
            int childrenCount = 0;
            try
            {
                string query = null;
                string parentLinkTable = null;
                string childIDColumn = null;
                string parentIDColumn = null;

                DataTable dataTable = new DataTable();
                DataTableReader dataTableReader = null;
                dataTable.Clear();

                CreateConnection();
                OpenConnection();

                if (childTableName == QatConstants.DbTestPlanTable)
                {
                    parentLinkTable = QatConstants.DbTestSuiteTestPlanLinkTable;
                    parentIDColumn = QatConstants.DbTestSuiteLinkTableID;
                    childIDColumn = QatConstants.DbTestPlanLinkTableID;
                }
                else if (childTableName == QatConstants.DbTestCaseTable)
                {
                    parentLinkTable = QatConstants.DbTestPlanTestCaseLinkTable;
                    parentIDColumn = QatConstants.DbTestPlanLinkTableID;
                    childIDColumn = QatConstants.DbTestCaseLinkTableID;
                }
                else if (childTableName == QatConstants.DbTestActionTable)
                {
                    parentLinkTable = QatConstants.DbTestActionTable;
                    parentIDColumn = QatConstants.DbTestCaseLinkTableID;
                    childIDColumn = QatConstants.DbTestActionTableTestActionID;
                }

                query = "select * from " + childTableName + " as c join " + parentLinkTable + " as p on p." + childIDColumn + " = c." + childTableName + "ID where p." + parentIDColumn + " = " + parentPrimaryKey.ToString();

                SqlDataAdapter dataAdapter = new SqlDataAdapter(query, connect);
                dataAdapter.Fill(dataTable);
                dataTableReader = dataTable.CreateDataReader();

                if (dataTableReader.HasRows)
                {
                    while (dataTableReader.Read())
                    {
                        childrenCount++;
                    }
                }

                CloseConnection();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Exception\n Reading record failed\n" + ex.Message, "QAT Error Code - EC02010", MessageBoxButton.OK, MessageBoxImage.Error);
                return childrenCount;
            }

            return childrenCount;
        }

        public DataTable Get_testcase_Actioncount_null(string query)
        {
            DataTable dataTable = new DataTable();
            try
            {
                //DataTable getnames = new DataTable();

                CreateConnection();
                OpenConnection();
                SqlDataAdapter dataAdapter = new SqlDataAdapter(query, connect);
                dataAdapter.Fill(dataTable);
                //getnames= dataTable.Rows.Cast<DataRow>().ToList();

                CloseConnection();
                return dataTable;

            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
                return dataTable;
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                // MessageBox.Show("Exception\n " + ex.Message, "Error Code - EC14025", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }

        public int Testcase_childcount(string query)
        {
            int childrenCount = 0;
            try
            {
                DataTable dataTable = new DataTable();
                CreateConnection();
                OpenConnection();
                SqlDataAdapter dataAdapter = new SqlDataAdapter(query, connect);
                dataAdapter.Fill(dataTable);
                
                if (dataTable != null)
                    childrenCount = dataTable.Rows.Count;

                CloseConnection();

                return childrenCount;
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
                return childrenCount;
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                // MessageBox.Show("Exception\n " + ex.Message, "Error Code - EC14025", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void Testcase_fillActioncount(string query)
        {
            try
            {
                //int childrenCount = 0;
                //DataTableReader dataTableReader = null;
                DataTable dataTable = new DataTable();
                CreateConnection();
                OpenConnection();
                SqlDataAdapter dataAdapter = new SqlDataAdapter(query, connect);
                dataAdapter.Fill(dataTable);

                CloseConnection();
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
                //return childrenCount;
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                // MessageBox.Show("Exception\n " + ex.Message, "Error Code - EC14025", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            //dataTableReader = dataTable.CreateDataReader();

            //if (dataTableReader.HasRows)
            //{
            //    while (dataTableReader.Read())
            //    {
            //        childrenCount++;
            //    }
            //}

            //return childrenCount;

        }
        

        public Tuple<bool,string> GetDeployValuesFromDB(string itemName, string itemType)
        {
            bool isdeployEnable = false;         
            string deployCount = string.Empty;
            List<string> designList = new List<string>();
            string query = string.Empty;
            try
            {
                if (itemType == QatConstants.DbTestPlanTable)
                {
                    CreateConnection();
                    OpenConnection();
                    query = "select IsDeployEnable,DeployCount from Testplan where Testplanname =@TPname";
                    DataTable tble = SelectDTWithParameter(query, "@TPname", itemName);
                    DataTableReader read = tble.CreateDataReader();
                    while (read.Read())
                    {
                        if (read[0] != null & read[0].ToString() != string.Empty)  
                            isdeployEnable = Convert.ToBoolean(read[0]);                         
                        if(read[1] != null)
                            deployCount = read[1].ToString();                      
                    }

                    CloseConnection();
                }

                return new Tuple<bool, string>(isdeployEnable,deployCount);
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                //MessageBox.Show("Exception\n " + ex.Message, "Error Code - EC02013", MessageBoxButton.OK, MessageBoxImage.Error);
                return new Tuple<bool, string>(isdeployEnable, deployCount);
            }
        }

        public void AddChildItemToParent(List<TreeViewExplorer> sourceTableItem, TreeViewExplorer targetTableItem)
        {
            List<int> childPrimaryKeyList = new List<int>();

            try
            {
                string query = null;
                string itemLinkTable = null;
                string itemIDColumn = null;
                string childTableName = null;
                string childIDColumn = null;
                string itemTableName = null;
                string itemactionCount = null;
                string itemTablePrimaryKey = null;
                string itemNameColumn = null;

                if (targetTableItem.ItemType == QatConstants.DbTestSuiteTable)
                {
                    itemLinkTable = QatConstants.DbTestSuiteTestPlanLinkTable;
                    itemIDColumn = QatConstants.DbTestSuiteLinkTableID;
                    childTableName = QatConstants.DbTestPlanTable;
                    childIDColumn = QatConstants.DbTestPlanLinkTableID;
                    itemTableName = QatConstants.DbTestSuiteTable;
                    itemactionCount = QatConstants.DbTestSuiteChildCount;
                    itemTablePrimaryKey = QatConstants.DbTestSuiteIDColumn;
                    itemNameColumn = QatConstants.DbTestSuiteNameColumn;
                }
                else if (targetTableItem.ItemType == QatConstants.DbTestPlanTable)
                {
                    itemLinkTable = QatConstants.DbTestPlanTestCaseLinkTable;
                    itemIDColumn = QatConstants.DbTestPlanLinkTableID;
                    childTableName = QatConstants.DbTestCaseTable;
                    childIDColumn = QatConstants.DbTestCaseLinkTableID;
                    itemTableName = QatConstants.DbTestPlanTable;
                    itemactionCount = QatConstants.DbTestPlanChildCount;
                    itemTablePrimaryKey = QatConstants.DbTestPlanIDColumn;
                    itemNameColumn = QatConstants.DbTestPlanNameColumn;
                }
                else
                {
                    return;
                }

                CreateConnection();
                OpenConnection();

                foreach (TreeViewExplorer item in sourceTableItem)
                {
                    query = "Insert into " + itemLinkTable + "(" + itemIDColumn + "," + childIDColumn + ") values (" + targetTableItem.ItemKey.ToString() + "," + item.ItemKey.ToString() + ")";
                    SqlCommand command = new SqlCommand(query, connect);
                    command.ExecuteScalar();

                    int childCount = 0;
                    query = "Select * from " + itemLinkTable + " where " + itemIDColumn + " = " + targetTableItem.ItemKey.ToString();
                    DataTable dataTable = new DataTable();
                    SqlDataAdapter dataAdapter = new SqlDataAdapter(query, connect);
                    dataAdapter.Fill(dataTable);

                    if(dataTable.Rows.Count > 0)
                    {
                        childCount = dataTable.Rows.Count;
                    }

                    targetTableItem.ChildrenCountForView = childCount;

                    query = "Update " + itemTableName + " set " + itemactionCount + " = " + childCount + " where " + itemTablePrimaryKey + " = " + targetTableItem.ItemKey;
                    SqlCommand cmd = new SqlCommand(query, connect);
                    cmd.ExecuteScalar();
                }

                DateTime? modifiedTime = DateTime.Now;
                string editorName = Properties.Settings.Default.TesterName.ToString().TrimEnd();
                query = "update " + itemTableName + " set " + itemNameColumn + " =@TSname,ModifiedOn=@editdate,ModifiedBy=@editBy,EditedBy=@editedBy where " + itemTablePrimaryKey + " = '" + targetTableItem.ItemKey + "'";
                InsertCommandWithParameter1(query, "@TSname", targetTableItem.ItemName.Trim(), "@editdate", modifiedTime, "@editBy", editorName, "@editedBy", editorName);
                CloseConnection();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Exception\n Creating link record failed\n" + ex.Message, "QAT Error Code - EC02014", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public string GetEditedByItem(TreeViewExplorer sourceTableItem)
        {
            try
            {
                string query = null;
                string itemTable = null;
                string itemIDColumn = null;
                string editedBy = string.Empty;

                if (sourceTableItem.ItemType == QatConstants.DbTestSuiteTable)
                {
                    itemTable = QatConstants.DbTestSuiteTable;
                    itemIDColumn = QatConstants.DbTestSuiteIDColumn;
                }
                else if (sourceTableItem.ItemType == QatConstants.DbTestPlanTable)
                {
                    itemTable = QatConstants.DbTestPlanTable;
                    itemIDColumn = QatConstants.DbTestPlanIDColumn;
                }
                else if (sourceTableItem.ItemType == QatConstants.DbTestCaseTable)
                {
                    itemTable = QatConstants.DbTestCaseTable;
                    itemIDColumn = QatConstants.DbTestCaseIDColumn;
                }
                else
                {
                    return string.Empty;
                }

                CreateConnection();
                OpenConnection();

                query = "select EditedBy from " + itemTable + " where " + itemIDColumn + "='" + sourceTableItem.ItemKey + "'";
                DataTable dataTable = new DataTable();
                DataTableReader dataTableReader = null;
                SqlDataAdapter dataAdapter = new SqlDataAdapter(query, connect);
                dataAdapter.Fill(dataTable);
                dataTableReader = dataTable.CreateDataReader();

                if (dataTableReader.HasRows)
                {
                    while (dataTableReader.Read())
                    {
                        if(dataTableReader[0] != System.DBNull.Value)
                            editedBy = dataTableReader.GetString(0);
                    }
                }

                CloseConnection();

                return editedBy;
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

        public void SetEditedByItem(TreeViewExplorer sourceTableItem, string editedBy)
        {
            try
            {
                string itemTable = null;
                string itemIDColumn = null;

                if(sourceTableItem==null && editedBy==string.Empty)
                {
                    return;
                }

                if (sourceTableItem.ItemType == QatConstants.DbTestSuiteTable)
                {
                    itemTable = QatConstants.DbTestSuiteTable;
                    itemIDColumn = QatConstants.DbTestSuiteIDColumn;
                }
                else if (sourceTableItem.ItemType == QatConstants.DbTestPlanTable)
                {
                    itemTable = QatConstants.DbTestPlanTable;
                    itemIDColumn = QatConstants.DbTestPlanIDColumn;
                }
                else if (sourceTableItem.ItemType == QatConstants.DbTestCaseTable)
                {
                    itemTable = QatConstants.DbTestCaseTable;
                    itemIDColumn = QatConstants.DbTestCaseIDColumn;
                }
                else
                {
                    return;
                }

                SendCommand_Toreceive("update "+itemTable+" set EditedBy= '" + editedBy + "' where "+itemIDColumn+" = '" + sourceTableItem.ItemKey + "'");

            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
            }
        }

        //public string StoreResponsalyzerFile(string responsalyzerFile, int testCaseID, int tabID)
        //{
            //try
            //{
            //    string FilePath = "\\Responsalyzer" + "\\Reference Files\\";
            //    FileInfo path = new FileInfo(responsalyzerFile);
            //    string responsalyzerpath = path.FullName;
            //    string responsalyzerfileName = path.Name;
            //    string startingString = testCaseID + "_" + tabID + "_";

            //    //if (Properties.Settings.Default.Path.ToString() != string.Empty)
            //    //{
            //    string PreferencesServerPath = QatConstants.QATServerPath + FilePath;
            //    if (!Directory.Exists(PreferencesServerPath))
            //    {
            //        Directory.CreateDirectory(PreferencesServerPath);
            //    }

            //    if (!File.Exists(PreferencesServerPath + startingString + responsalyzerfileName))
            //    {
            //        File.Copy(responsalyzerpath + "", PreferencesServerPath + startingString + responsalyzerfileName + "");
            //        FileInfo fileInformation = new FileInfo(PreferencesServerPath + startingString + responsalyzerfileName + "");
            //        fileInformation.IsReadOnly = true;
            //    }
            //    //return PreferencesServerPath + startingString + responsalyzerfileName;
            //    return startingString+responsalyzerfileName;
            //    //}
            //}
            //try
            //{
              //  FileInfo path = new FileInfo(responsalyzerFile);
                //string resp_path = path.FullName;
                //string RespName = path.Name;
                //string FilePath = "\\Responsalyzer" + "\\Reference Files\\";
                //if (Properties.Settings.Default.Path.ToString() != string.Empty)
                //{
                //string PreferencesServerPath = QatConstants.QATServerPath + FilePath;
                //if (!Directory.Exists(PreferencesServerPath))
                //{
                //    Directory.CreateDirectory(PreferencesServerPath);
                //}

                //if (!File.Exists(PreferencesServerPath + RespName))
                //{
                //    File.Copy(resp_path + "", PreferencesServerPath + RespName + "");
                //    FileInfo fileInformation = new FileInfo(PreferencesServerPath + RespName + "");
               //     fileInformation.IsReadOnly = true;
                //}
                //return RespName;
                //}
            //}
           // catch (Exception ex)
           // {
           //     DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
              //  MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                //MessageBox.Show("Exception\n \n" + ex.Message, "QAT Error Code - ECxxxxRA", MessageBoxButton.OK, MessageBoxImage.Error);
              //  return string.Empty;
           // }
       // }

         public string addQATPrefixToControl(string controlName)
        {
            try
            {
                string QATPrefixFile = Properties.Settings.Default.QATPrefix.ToString();
                string controlWithQATPrefix = string.Empty;
                List<string> outerSplittedLines = new List<string>();
                List<string> innerSplittedLines = new List<string>();
                List<string> containsList = new List<string>();
                List<string> notContainsList = new List<string>();
                List<string> startsWithList = new List<string>();
                List<string> replaceList = new List<string>();

                outerSplittedLines = QATPrefixFile.Split(';').ToList();
                outerSplittedLines.RemoveAll(str => String.IsNullOrEmpty(str));
                if (outerSplittedLines.Count > 0)
                {
                    foreach (string compare in outerSplittedLines)
                    {
                        innerSplittedLines = compare.Split(',').ToList();
                        foreach (string match in innerSplittedLines)
                        {
                            if (match.StartsWith("-"))
                            {
                                notContainsList.Add(match.Remove(0, 1));
                            }
                            if (match.StartsWith("*"))
                            {
                                startsWithList.Add(match.Remove(0, 1));
                            }
                            if (match.StartsWith("="))
                            {
                                replaceList.Add(match.Remove(0, 1));
                            }
                            if ((!match.Contains("-")) && (!match.Contains("*")) && (!match.Contains("=")))
                            {
                                containsList.Add(match);
                            }
                        }
                        var result = compareAndVerify(controlName, notContainsList, startsWithList, replaceList, containsList);// tiltCountList
                        if (result.Item1)
                        {
                            controlWithQATPrefix = result.Item2;
                            notContainsList.Clear();
                            startsWithList.Clear();
                            replaceList.Clear();
                            containsList.Clear();
                            break;
                        }
                        else
                        {
                            notContainsList.Clear();
                            startsWithList.Clear();
                            replaceList.Clear();
                            containsList.Clear();
                        }
                    }
                }
                return controlWithQATPrefix;
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

        public Tuple<bool,string> compareAndVerify(string controlName,List<string>notContainsList, List<string>startsWithList, List<string>replaceList, List<string>containsList)//List<string> tiltCountList
        {
            try
            {
                bool _notContains = false;
                bool _startsWith = false;
                bool _contains = false;
                int notContainsCount = 0;
                int startsWithCount = 0;
                int containsCount = 0;
                int tiltCount = controlName.Count(x => x == '~');

                if (notContainsList.Count > 0)
                {
                    foreach (string notContains in notContainsList)
                    {
                        if (!controlName.Contains(notContains))
                        {
                            notContainsCount++;
                        }
                    }
                }
                if (startsWithList.Count > 0)
                {
                    foreach (string startsWith in startsWithList)
                    {
                        if (controlName.StartsWith(startsWith))
                        {
                            startsWithCount++;
                        }
                    }
                }
                if (containsList.Count > 0)
                {
                    foreach (string contains in containsList)
                    {
                        if (controlName.Contains(contains))
                        {
                            containsCount++;
                        }
                    }
                }

                if (notContainsCount == notContainsList.Count)
                {
                    _notContains = true;
                }
                if (startsWithCount == startsWithList.Count)
                {
                    _startsWith = true;
                }
                if (containsCount == containsList.Count)
                {
                    _contains = true;
                }
                if (_contains && _startsWith && _notContains)
                {
                    return new Tuple<bool, string>(true, replaceList[0]);
                }

                else
                {
                    return new Tuple<bool, string>(false, replaceList[0]);
                }
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                return new Tuple<bool, string>(false, replaceList[0]);
            }      
        }
        private MessageBoxResult DBconnectionsMessageBox(string messageBoxTest, string messageBoxCaption, MessageBoxButton messageBoxButton, MessageBoxImage messageBoxImage)
        {

            MessageBoxResult result = MessageBoxResult.None;
            try
            {

                QatMessageBox Qmsg = new QatMessageBox(DeviceDiscovery.DesignerWindow);
                Qmsg.Topmost = false;
               

                result = Qmsg.Show(messageBoxTest, messageBoxCaption, messageBoxButton, messageBoxImage);
                return result;
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif

                return result;
            }
        }

        public string removeQATPrefix(string controlWithQATPrefix)
        {
            try
            {
                // 16Hz,*16Hz,= 16Hz; 31.5Hz,*31.5Hz,= 31.5Hz; 63Hz,*63Hz,= 63Hz; 125Hz,*125Hz,= 125Hz; 250Hz,*250Hz,= 250Hz; 500Hz,*500Hz,= 500Hz; 1kHz,*1kHz,= 1kHz; 2kHz,*2kHz,= 2kHz; 4kHz,*4kHz,= 4kHz; 8kHz,*8kHz,= 8kHz; 16kHz,*16kHz,= 16kHz; Amplifier Status,*Amplifier Status,= AMPLIFIERSTATUS; APM,*APM,= APM; Assignable Fader,*Assignable Fader,= ASSIGNABLEFADER; Aux,*Aux,= AUX; Backup Engaged,*Backup Engaged,= BACKUPENGAGED; Band,*Band,= BAND; Bank Control, Input,-Output,*Bank Control,= BANK_CONTROL_INPUT; Bank Control, Output, Input,*Bank Control,= BANK_CONTROL_INPUT_OUTPUT; Bank Control, Output,-Input,*Bank Control,= BANK_CONTROL_OUTPUT; Bank Select, Input,*Bank Select,= BANK_SELECT_INPUT; Bank Select, Output,*Bank Select,= BANK_SELECT_OUTPUT; BGM,*BGM,= BGM; Button,*Button,= BUTTON; Call Control,*Call Control,= CALLCONTROL; Call History,*Call History,= CALLHISTORY; Call Status,*Call Status,= CALLSTATUS; Center Master Meter,*Center Master Meter,= CENTERMASTERMETER; Channel,*Channel,= CHANNEL; Chassis Fan,*Chassis Fan,= CHASSISFAN; Command,*Command,= COMMAND; Compressor / Limiter Gain Reduction,*Compressor / Limiter Gain Reduction,= COMPRESSOR / LIMITERGAINREDUCTION; Configuration,*Configuration,= CONFIGURATION; DCS,*DCS,= DCS; Dim,*Dim,= DIM; DTMF,*DTMF,= DTMF; Dynamics Input Level,*Dynamics Input Level,= DYNAMICSINPUTLEVEL; Dynamics Output Level,*Dynamics Output Level,= DYNAMICSOUTPUTLEVEL; Encoder,*Encoder,= ENCODER; Gate / Expander Gain Reduction,*Gate / Expander Gain Reduction,= GATE / EXPANDERGAINREDUCTION; GPIO,Input,-Out,*GPIO,= GPIO_INPUT; GPIO,Out,-Input,*GPIO,= GPIO_OUTPUT; High Band,*High Band,= HIGHBAND; Input,-Output,*Input,-Bank Control,-Bank Select,-GPIO,= INPUT; Input,Output,*Input,-Bank Control,-GPIO,= INPUT; Last,*Last,= LAST; Left Master Meter,*Left Master Meter,= LEFTMASTERMETER; Left,*Left,= LEFT; Line Status,*Line Status,= LINESTATUS; Load,*Load,= LOAD; Low Band,*Low Band,= LOWBAND; Manual Backup Engage,*Manual Backup Engage,= MANUALBACKUPENGAGE; Master,*Master,= MASTER; Match,*Match,= MATCH; Mid Band,*Mid Band,= MIDBAND; Mute Group,*Mute Group,= MUTEGROUP; Notch,*Notch,= NOTCH; Output,-Input,*Output,-Bank Control,-Bank Select,-GPIO,= OUTPUT; Output,Input,*Output,-Bank Control,-GPIO,= OUTPUT; PA,*PA,= PA; Parametric Eq Band,*Parametric Eq Band,= PARAMETRICEQBAND; PinPad,*PinPad,= PINPAD; Point,*Point,= POINT; Program,*Program,= PROGRAM; Right Master Meter,*Right Master Meter,= RIGHTMASTERMETER; Right,*Right,= RIGHT; Room,*Room,= ROOM; Save,*Save,= SAVE; Statistics,*Statistics,= STATISTICS; Sub,*Sub,= SUB; Tap,*Tap,= TAP; Test,*Test,= TEST; Text display,*Text display,= TEXTDISPLAY; Wall,*Wall,= WALL; Zone,*Zone,= ZONE;
                //Bank Control,Input,-Output,*Bank Control,=BANK_CONTROL_INPUT ;Bank Control,Output,Input,*Bank Control,=BANK_CONTROL_INPUT_OUTPUT ;Bank Control,Output,-Input,*Bank Control,=BANK_CONTROL_OUTPUT ;Bank Select,Input,*Bank Select,=BANK_SELECT_INPUT ;Bank Select,Output,*Bank Select,=BANK_SELECT_OUTPUT ;Channel,*Channel,=CHANNEL ; GPIO,Input,-Out,*GPIO,=GPIO_INPUT ;GPIO,Out,-Input,*GPIO,=GPIO_OUTPUT ;Input,-Output,*Input,-Bank Control,-Bank Select,-GPIO,=INPUT ;Input,Output,*Input,-Bank Control,-Bank Select,-GPIO,=INPUT ;Output,-Input,*Output,-Bank Control,-Bank Select,-GPIO,=OUTPUT ;Output,Input,*Output,-Bank Control,-Bank Select,-GPIO,=OUTPUT ;Tap,*Tap,= TAP ;
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

        public int ReadTreeTableChildrenCount(int itemPrimaryID, string tblName, string condition, string cntID)
        {
            int count = 0;
            try
            {
                CreateConnection();
                OpenConnection();

                string query = "select count(" + cntID + ") from " + tblName +" where " + condition + " = " + itemPrimaryID;
                DataTable dataTable = new DataTable();
                DataTableReader dataTableReader = null;
                SqlDataAdapter dataAdapter = new SqlDataAdapter(query, connect);
                dataAdapter.Fill(dataTable);
                dataTableReader = dataTable.CreateDataReader();

                if (dataTableReader.HasRows)
                {
                    while (dataTableReader.Read())
                    {
                        if (dataTableReader[0] != System.DBNull.Value)
                            count = dataTableReader.GetInt32(0);
                    }
                }

                CloseConnection();
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                // MessageBox.Show("Exception\n \n" + ex.Message, "QAT Error Code - EC14017F", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return count;
        }
    }

    public class AlphanumComparatorFastDut : IComparer
    {
        public int Compare(object x, object y)
        {
            try
            {
                DUT_DeviceItem one = x as DUT_DeviceItem;
                DUT_DeviceItem two = y as DUT_DeviceItem;
                string s1 = one.ItemDeviceName;
                if (s1 == null)
                {
                    return 0;
                }
                string s2 = two.ItemDeviceName;
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


                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                //MessageBox.Show("Exception\n \n" + ex.Message, "QAT Error Code - EC14017F", MessageBoxButton.OK, MessageBoxImage.Error);
                return 0;

            }

        }
    }
}
