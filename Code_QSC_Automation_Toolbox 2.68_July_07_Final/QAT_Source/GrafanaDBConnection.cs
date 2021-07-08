namespace QSC_Test_Automation
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Data;
    using System.Data.SqlClient;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows;

    class GrafanaDBConnection
    {
        private SqlConnection grafanaConnect = null;

        public GrafanaDBConnection()
        {
            CultureInfo ci = CultureInfo.CreateSpecificCulture(CultureInfo.CurrentCulture.Name);
            ci.DateTimeFormat.ShortDatePattern = "MM-dd-yyyy";
            Thread.CurrentThread.CurrentCulture = ci;
        }

        public bool DataBaseConnection()
        {
            try
            {
                using (this.grafanaConnect = new SqlConnection(ConfigurationManager.ConnectionStrings["ConString"].ToString()))
                {
                    this.grafanaConnect.Open();
                    string query = "IF NOT EXISTS (SELECT name FROM master.dbo.sysdatabases WHERE name = '" + QatConstants.Grafana_DbDatabaseName + "') CREATE DATABASE " + QatConstants.Grafana_DbDatabaseName + "";
                    SqlCommand cmd = new SqlCommand(query, this.grafanaConnect);
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
                this.grafanaConnect = new SqlConnection(ConfigurationManager.ConnectionStrings["ConString"].ToString() + ";Database=" + QatConstants.Grafana_DbDatabaseName + "");
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

            return this.grafanaConnect;
        }

        public void OpenConnection()
        {
            try
            {
                if (grafanaConnect.State != ConnectionState.Open)
                    this.grafanaConnect.Open();
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
                if (grafanaConnect.State != ConnectionState.Closed)
                    this.grafanaConnect.Close();
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

        public void Create_Tables()
        {
            try
            {
                DataSet ds = new DataSet();

                string query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'DataPointsMappingTable') BEGIN Create table DataPointsMappingTable(ScriptMappingID int identity(1,1) primary key,ExecID int null,Testsuitename nvarchar(MAX) null,Testplanname nvarchar(MAX) null,Testcasename nvarchar(MAX) null,Testactionname nvarchar(MAX) null,ScriptType nvarchar(MAX) null,ScriptStartTime nvarchar(MAX) null,ReleaseVersion nvarchar(MAX) null,DesignName nvarchar(MAX) null,TagName nvarchar(MAX) null) END";
                SqlDataAdapter adap = new SqlDataAdapter(query, this.CreateConnection());
                this.OpenConnection();
                adap.Fill(ds, "Test");

                query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'DataPointsTable') BEGIN Create table DataPointsTable(DatapointsID int identity(1,1) primary key,ScriptMappingID int null,Iteration int null,ScriptDatapoint float null,FOREIGN KEY (ScriptMappingID) references DataPointsMappingTable(ScriptMappingID) on delete cascade) END";
                adap = new SqlDataAdapter(query, this.CreateConnection());
                adap.Fill(ds, "Test");

                query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'DataPointsMappingTable' AND COLUMN_NAME = 'DataStartTime') BEGIN  ALTER TABLE [dbo].[DataPointsMappingTable] ADD [DataStartTime] nvarchar(MAX) null, [DataEndTime] nvarchar(MAX) null,[Average] float null,[Minimum] float null,[Maximum] float null, [devicename] nvarchar(MAX) null END";
                adap = new SqlDataAdapter(query, this.CreateConnection());
                adap.Fill(ds, "Test");

                this.CloseConnection();
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                //MessageBox.Show("Exception\n" + error.Message, "QSC_Automation_Toolbox", MessageBoxButton.OK, MessageBoxImage.Error);
                this.CloseConnection();
            }
        }

        public Tuple<bool, DataTable> SendCommand_Toreceive(string cmdtext)
        {
            DataTable datatable = new DataTable();

            try
            {
                SqlCommand com = new SqlCommand(cmdtext, this.CreateConnection());
                SqlDataAdapter adap = new SqlDataAdapter(com);
                this.OpenConnection();
                adap.Update(datatable);
                adap.Fill(datatable);
                this.CloseConnection();
                return new Tuple<bool, DataTable>(true, datatable);
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                //MessageBox.Show("Exception\n " + error.Message, "QSC_Automation_Toolbox", MessageBoxButton.OK, MessageBoxImage.Error);

                return new Tuple<bool, DataTable>(false, datatable);

            }
        }
    }
}
