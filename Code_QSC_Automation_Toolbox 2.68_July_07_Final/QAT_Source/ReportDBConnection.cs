using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Windows;

namespace QSC_Test_Automation
{
    class ReportDBConnection
    {
        private SqlConnection Report_connect = null;
        DataSet ds = new DataSet();
        private string query;

        public bool Report_DataBaseConnection()
        {
            try
            {
                using (this.Report_connect = new SqlConnection(ConfigurationManager.ConnectionStrings["ConString"].ToString()))
                {
                    this.Report_connect.Open();
                    string query = "IF NOT EXISTS (SELECT name FROM master.dbo.sysdatabases WHERE name ='" + QatConstants.Report_DbDatabaseName + "') CREATE DATABASE " + QatConstants.Report_DbDatabaseName + "";
                    SqlCommand cmd = new SqlCommand(query, this.Report_connect);
                    int value = Convert.ToInt32(cmd.ExecuteScalar());
                    return true;
                }
            }
            catch (SqlException error)
            {
                MessageBox.Show("No Server available with the supplied credentials\n Check SQL-Server source name, Database Username & Password \n " + error.Message,  QatConstants.Report_DbDatabaseName , MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                //MessageBox.Show("Exception\n " + e.Message, QatConstants.Report_DbDatabaseName, MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        public SqlConnection Report_CreateConnection_ForTables()
        {
            try
            {
                this.Report_connect = new SqlConnection(ConfigurationManager.ConnectionStrings["ConString"].ToString() + ";Database=" +QatConstants.Report_DbDatabaseName+"");
            }
            catch (SqlException error)
            {
                throw error;
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif

            }

            return this.Report_connect;
        }

        public void Report_OpenConnection()
        {
            try
            {
                if(this.Report_connect.State != ConnectionState.Open)
                    this.Report_connect.Open();
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif

            }
        }

        public void Report_CloseConnection()
        {
            try
            {
                if (this.Report_connect.State != ConnectionState.Closed)
                    this.Report_connect.Close();
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif

            }
        }

        public void Create_Report_Tables()
        {
            try
            {
                query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'ExecutionTable') BEGIN Create table ExecutionTable(ExecutionID int identity(1,1) primary key,start_time nvarchar(MAX) not null,end_time nvarchar(MAX) not null,Total_Testsuite nvarchar(MAX) not null,Tester_Info nvarchar(MAX) not null,os_info nvarchar(MAX) not null,TestSuiteName nvarchar(MAX) not null) END";//,TypeOfLoop nvarchar(MAX) not null,Failed_Loop_Info nvarchar(MAX) not null
                SqlDataAdapter adap = new SqlDataAdapter(query, this.Report_CreateConnection_ForTables());
                this.Report_OpenConnection();
                adap.Fill(ds, "Test");

                this.query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ExecutionTable' AND COLUMN_NAME = 'TypeOfLoop') BEGIN  ALTER TABLE [dbo].[ExecutionTable] ADD [TypeOfLoop] nvarchar(MAX) null END";
                adap = new SqlDataAdapter(this.query, this.Report_CreateConnection_ForTables());
                adap.Fill(this.ds, "Test");

                this.query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ExecutionTable' AND COLUMN_NAME = 'Failed_Loop_Info') BEGIN  ALTER TABLE [dbo].[ExecutionTable] ADD [Failed_Loop_Info] nvarchar(MAX) null END";
                adap = new SqlDataAdapter(this.query, this.Report_CreateConnection_ForTables());
                adap.Fill(this.ds, "Test");

                this.query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ExecutionTable' AND COLUMN_NAME = 'User_Stopped') BEGIN  ALTER TABLE [dbo].[ExecutionTable] ADD [User_Stopped] nvarchar(MAX) null END";
                adap = new SqlDataAdapter(this.query, this.Report_CreateConnection_ForTables());
                adap.Fill(this.ds, "Test");

                query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Testsuite_Report') BEGIN Create table Testsuite_Report(ExecID int null,Testsuitename nvarchar(MAX) not null,start_time nvarchar(MAX) not null,end_time nvarchar(MAX) not null,Total_Testplan nvarchar(MAX) not null,passed_testplan nvarchar(MAX) not null,failed_testplan nvarchar(MAX) not null,Total_Testcase nvarchar(MAX) not null,passed_testcase nvarchar(MAX) not null,failed_testcase nvarchar(MAX) not null,Build_Version nvarchar(MAX) not null,Core_Info nvarchar(MAX) not null,Loopcounter_Info nvarchar(MAX) not null,Failed_Loop_Info nvarchar(MAX) not null,FOREIGN KEY (ExecID) references ExecutionTable(ExecutionID) on delete cascade) END";
                //query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Testsuite_Report') BEGIN Create table Testsuite_Report(ExecID int null,Testsuitename nvarchar(MAX) not null,start_time nvarchar(MAX) not null,end_time nvarchar(MAX) not null,Total_Testplan nvarchar(MAX) not null,passed_testplan nvarchar(MAX) not null,failed_testplan nvarchar(MAX) not null,Total_Testcase nvarchar(MAX) not null,passed_testcase nvarchar(MAX) not null,failed_testcase nvarchar(MAX) not null,Build_Version nvarchar(MAX) not null,Core_Info nvarchar(MAX) not null,Loopcounter_Info nvarchar(MAX) not null,FOREIGN KEY (ExecID) references ExecutionTable(ExecutionID) on delete cascade) END";
                adap = new SqlDataAdapter(query, this.Report_CreateConnection_ForTables());
                adap.Fill(ds, "Test");

                this.query = "IF Exists (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Testsuite_Report' AND COLUMN_NAME = 'Failed_Loop_Info') BEGIN  ALTER TABLE [dbo].[Testsuite_Report] DROP COLUMN  [Failed_Loop_Info] END";
                adap = new SqlDataAdapter(this.query, this.Report_CreateConnection_ForTables());
                adap.Fill(this.ds, "Test");

                this.query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Testsuite_Report' AND COLUMN_NAME = 'Redeployed_Design') BEGIN  ALTER TABLE [dbo].[Testsuite_Report] ADD [Redeployed_Design] nvarchar(MAX) null END";
                adap = new SqlDataAdapter(this.query, this.Report_CreateConnection_ForTables());
                adap.Fill(this.ds, "Test");

                query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Testplan_Report') BEGIN Create table Testplan_Report(ExecID int null,Testsuitename nvarchar(MAX) not null,Testplanname nvarchar(MAX) not null,start_time nvarchar(MAX) not null,end_time nvarchar(MAX) not null,Build_Version nvarchar(MAX) not null,Inventory nvarchar(MAX) not null,Total_Testcase nvarchar(MAX) not null,passed_testcase nvarchar(MAX) not null,failed_testcase nvarchar(MAX) not null,Background_verify nvarchar(MAX) not null,FOREIGN KEY (ExecID) references ExecutionTable(ExecutionID) on delete cascade) END";
                adap = new SqlDataAdapter(query, this.Report_CreateConnection_ForTables());
                adap.Fill(ds, "Test");

                query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Testcase_Report') BEGIN Create table Testcase_Report(ExecID int null,Testsuitename nvarchar(MAX) not null,Testplanname nvarchar(MAX) not null,Testcasename nvarchar(MAX) not null,Status nvarchar(MAX) not null,start_time nvarchar(MAX) not null,end_time nvarchar(MAX) not null,Build_Version nvarchar(MAX) not null,Inventory nvarchar(MAX) not null,Background_verify nvarchar(MAX) not null,Download_Log nvarchar(MAX) not null,FOREIGN KEY (ExecID) references ExecutionTable(ExecutionID) on delete cascade) END";
                adap = new SqlDataAdapter(query, this.Report_CreateConnection_ForTables());
                adap.Fill(ds, "Test");

                //TestAction_Report
                query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'TestAction_Report') BEGIN Create table TestAction_Report(ExecID int null,Testsuitename nvarchar(MAX) not null,Testplanname nvarchar(MAX) not null,Testcasename nvarchar(MAX) not null,TestActionname nvarchar(MAX) not null,Status nvarchar(MAX) not null,start_time nvarchar(MAX) not null,end_time nvarchar(MAX) not null,Download_Log nvarchar(MAX) not null,FOREIGN KEY (ExecID) references ExecutionTable(ExecutionID) on delete cascade) END";
                adap = new SqlDataAdapter(query, this.Report_CreateConnection_ForTables());
                adap.Fill(ds, "Test");


                //TempExecutionTable
                query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'TempExecutionTable') BEGIN Create table TempExecutionTable(ExecID int null,status nvarchar(MAX) not null,Testsuitename nvarchar(MAX) not null,Build nvarchar(MAX) not null,TotalTestCaseExecuted nvarchar(MAX) not null,TotalPassedTestCase nvarchar(MAX) not null,TotalFailedTestCase nvarchar(MAX) not null,TotalIncompleteTestCase nvarchar(MAX) not null,Iterations nvarchar(MAX) not null,Date nvarchar(MAX) not null,FOREIGN KEY (ExecID) references ExecutionTable(ExecutionID) on delete cascade) END";
                adap = new SqlDataAdapter(query, this.Report_CreateConnection_ForTables());
                adap.Fill(ds, "Test");

              
                query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'TempExecutionTable' AND COLUMN_NAME = 'Inprogresstime') BEGIN  ALTER TABLE [dbo].[TempExecutionTable] ADD Inprogresstime nvarchar(MAX) null END";
                adap = new SqlDataAdapter(query, this.Report_CreateConnection_ForTables());
                adap.Fill(ds, "Test");

                query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'TempExecutionTable' AND COLUMN_NAME = 'StartDateTime') BEGIN  ALTER TABLE [dbo].[TempExecutionTable] ADD StartDateTime nvarchar(MAX) null END";
                adap = new SqlDataAdapter(query, this.Report_CreateConnection_ForTables());
                adap.Fill(ds, "Test");

                query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'TempExecutionTable' AND COLUMN_NAME = 'EndDateTime') BEGIN  ALTER TABLE [dbo].[TempExecutionTable] ADD EndDateTime nvarchar(MAX) null END";
                adap = new SqlDataAdapter(query, this.Report_CreateConnection_ForTables());
                adap.Fill(ds, "Test");

                //this.query = "IF Not EXISTS(SELECT * FROM sys.types WHERE is_table_type = 1 AND name = 'TestExecutionTableType') BEGIN CREATE TYPE TestExecutionTableType AS TABLE( ExecID int null,status nvarchar(MAX) not null,TotalIncompleteTestCase nvarchar(MAX) not null,TotalTestCaseExecuted nvarchar(MAX) not null,Iterations nvarchar(MAX) null) END";
                //adap = new SqlDataAdapter(query, this.Report_CreateConnection_ForTables());
                //adap.Fill(ds, "Test");

                ////this.query = "IF Not EXISTS(SELECT * FROM sys.objects WHERE type = 'p' AND name = 'Update_TestSuiteTable')Begin exec('CREATE PROCEDURE [Update_TestSuiteTable] @tblTempSuiteTable TestSuiteTableType READONLY As BEGIN SET NOCOUNT ON;MERGE INTO TempSuiteTable c1 USING @tblTempSuiteTable  c2 ON c1.ExecID=c2.ExecID and c1.Testsuitename=c2.Testsuitename and c1.TestSuiteLoopIteratios=c2.TestSuiteLoopIteratios and c1.SuiteExecutionUniqueID=c2.SuiteExecutionUniqueID and c1.EndTime= c2.EndTime WHEN MATCHED THEN UPDATE SET c1.TotalIncompleteTestCase= c2.TotalIncompleteTestCase, c1.EndTime= c2.EndTime, c1.Remarks= c2.Remarks; END') end";
                //this.query = "IF Not EXISTS(SELECT * FROM sys.objects WHERE type = 'p' AND name = 'Update_TestExecutionTable')Begin exec('CREATE PROCEDURE [Update_TestExecutionTable] @tblTempExecutionTable TestExecutionTableType READONLY As BEGIN SET NOCOUNT ON; update TempExecutionTable set status=c2.status,TotalIncompleteTestCase= c2.TotalIncompleteTestCase, TotalTestCaseExecuted= c2.TotalTestCaseExecuted, Iterations= c2.Iterations from TempExecutionTable c1 INNER JOIN @tblTempExecutionTable c2  ON  c1.ExecID=c2.ExecID END') end";
                //adap = new SqlDataAdapter(query, this.Report_CreateConnection_ForTables());
                //adap.Fill(ds, "Test");

                //TempSuiteTable
                query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'TempSuiteTable') BEGIN Create table TempSuiteTable(ExecID int null,status nvarchar(MAX) not null,Testsuitename nvarchar(MAX) not null,TotalTestCaseExecuted nvarchar(MAX) not null,TotalPassedTestCase nvarchar(MAX) not null,TotalFailedTestCase nvarchar(MAX) not null,TotalIncompleteTestCase nvarchar(MAX) not null,BackGroundVerification nvarchar(MAX) not null,StartTime nvarchar(MAX) not null,EndTime nvarchar(MAX) not null,Build nvarchar(MAX) not null,OSVersion nvarchar(MAX) not null,FOREIGN KEY (ExecID) references ExecutionTable(ExecutionID) on delete cascade) END";
                adap = new SqlDataAdapter(query, this.Report_CreateConnection_ForTables());
                adap.Fill(ds, "Test");

                this.query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'TempSuiteTable' AND COLUMN_NAME = 'Date') BEGIN  ALTER TABLE [dbo].[TempSuiteTable] ADD [Date] nvarchar(MAX) null END";
                adap = new SqlDataAdapter(this.query, this.Report_CreateConnection_ForTables());
                adap.Fill(this.ds, "Test");

                this.query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'TempSuiteTable' AND COLUMN_NAME = 'Remarks') BEGIN  ALTER TABLE [dbo].[TempSuiteTable] ADD [Remarks] nvarchar(MAX) null END";
                adap = new SqlDataAdapter(this.query, this.Report_CreateConnection_ForTables());
                adap.Fill(this.ds, "Test");

                this.query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'TempSuiteTable' AND COLUMN_NAME = 'TestSuiteLogPath') BEGIN  ALTER TABLE [dbo].[TempSuiteTable] ADD [TestSuiteLogPath] nvarchar(MAX) null END";
                adap = new SqlDataAdapter(this.query, this.Report_CreateConnection_ForTables());
                adap.Fill(this.ds, "Test");

                //LoopIteration
                this.query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'TempSuiteTable' AND COLUMN_NAME = 'TestSuiteLoopIteratios') BEGIN  ALTER TABLE [dbo].[TempSuiteTable] ADD [TestSuiteLoopIteratios] nvarchar(MAX) null END";
                adap = new SqlDataAdapter(this.query, this.Report_CreateConnection_ForTables());
                adap.Fill(this.ds, "Test");

                //SuiteExecutionUniqueID
                this.query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'TempSuiteTable' AND COLUMN_NAME = 'SuiteExecutionUniqueID') BEGIN  ALTER TABLE [dbo].[TempSuiteTable] ADD [SuiteExecutionUniqueID] int null END";
                adap = new SqlDataAdapter(this.query, this.Report_CreateConnection_ForTables());
                adap.Fill(this.ds, "Test");

                //this.query = "IF Not EXISTS(SELECT * FROM sys.types WHERE is_table_type = 1 AND name = 'TestSuiteTableType') BEGIN CREATE TYPE TestSuiteTableType AS TABLE( ExecID int null,status nvarchar(MAX) not null,Testsuitename nvarchar(MAX) not null,TotalIncompleteTestCase nvarchar(MAX) not null,EndTime nvarchar(MAX) not null,Remarks nvarchar(MAX) null,TestSuiteLoopIteratios nvarchar(MAX) null,SuiteExecutionUniqueID int null) END";
                //adap = new SqlDataAdapter(query, this.Report_CreateConnection_ForTables());
                //adap.Fill(ds, "Test");

                ////this.query = "IF Not EXISTS(SELECT * FROM sys.objects WHERE type = 'p' AND name = 'Update_TestSuiteTable')Begin exec('CREATE PROCEDURE [Update_TestSuiteTable] @tblTempSuiteTable TestSuiteTableType READONLY As BEGIN SET NOCOUNT ON;MERGE INTO TempSuiteTable c1 USING @tblTempSuiteTable  c2 ON c1.ExecID=c2.ExecID and c1.Testsuitename=c2.Testsuitename and c1.TestSuiteLoopIteratios=c2.TestSuiteLoopIteratios and c1.SuiteExecutionUniqueID=c2.SuiteExecutionUniqueID and c1.EndTime= c2.EndTime WHEN MATCHED THEN UPDATE SET c1.TotalIncompleteTestCase= c2.TotalIncompleteTestCase, c1.EndTime= c2.EndTime, c1.Remarks= c2.Remarks; END') end";
                //this.query= "IF Not EXISTS(SELECT * FROM sys.objects WHERE type = 'p' AND name = 'Update_TestSuiteTable')Begin exec('CREATE PROCEDURE [Update_TestSuiteTable] @tblTempSuiteTable TestSuiteTableType READONLY As BEGIN SET NOCOUNT ON; update TempSuiteTable set status=c2.status,TotalIncompleteTestCase= c2.TotalIncompleteTestCase, EndTime= c2.EndTime, Remarks= c2.Remarks from TempSuiteTable c1 INNER JOIN @tblTempSuiteTable c2  ON  c1.ExecID=c2.ExecID and c1.Testsuitename=c2.Testsuitename and c1.TestSuiteLoopIteratios=c2.TestSuiteLoopIteratios and c1.SuiteExecutionUniqueID=c2.SuiteExecutionUniqueID END') end";
                //adap = new SqlDataAdapter(query, this.Report_CreateConnection_ForTables());
                //adap.Fill(ds, "Test");


                //TempTestPlanTable
                query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'TempTestPlanTable') BEGIN Create table TempTestPlanTable(ExecID int null,status nvarchar(MAX) not null,Testsuitename nvarchar(MAX) not null,Testplanname nvarchar(MAX) not null,TotalTestCaseExecuted nvarchar(MAX) not null,TotalPassedTestCase nvarchar(MAX) not null,TotalFailedTestCase nvarchar(MAX) not null,TotalIncompleteTestCase nvarchar(MAX) not null,BackGroundVerification nvarchar(MAX) not null,StartTime nvarchar(MAX) not null,EndTime nvarchar(MAX) not null,Build nvarchar(MAX) not null,OSVersion nvarchar(MAX) not null,DesignName nvarchar(MAX) not null,Inventory nvarchar(MAX) not null,FOREIGN KEY (ExecID) references ExecutionTable(ExecutionID) on delete cascade) END";
                adap = new SqlDataAdapter(query, this.Report_CreateConnection_ForTables());
                adap.Fill(ds, "Test");
				
				 query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'TempPlanBackground') BEGIN Create table TempPlanBackground(TempPlanBackground_ID int identity(1,1) primary key,ExecID int null,status nvarchar(MAX) not null,BMstartEnd  nvarchar(MAX) not null,Testsuitename nvarchar(MAX) not null,Testplanname nvarchar(MAX) not null,StartTime nvarchar(MAX) not null,EndTime nvarchar(MAX) not null,PlanExecutionUniqueID int null,SuiteAlongwithplanandcaseExecid int null,Execid_Planname_startdate nvarchar(MAX) not null,Remarks nvarchar(MAX) not null) END";
                adap = new SqlDataAdapter(this.query, this.Report_CreateConnection_ForTables());
                adap.Fill(this.ds, "Test");

                query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'TempPlanBackgroundTable') BEGIN Create table TempPlanBackgroundTable(TempPlanBackgroundTable int identity(1,1) primary key,TempPlanBG_ID int null,ExecID int null,status nvarchar(MAX) not null,BMstartEnd  nvarchar(MAX) not null,Testsuitename nvarchar(MAX) not null,Testplanname nvarchar(MAX) not null,StartTime nvarchar(MAX) not null,EndTime nvarchar(MAX) not null,Actiontype nvarchar(MAX),Expected_Value nvarchar(MAX),Actual_Value nvarchar(MAX),Remarks nvarchar(MAX) null,PlanExecutionUniqueID int null,SuiteAlongwithplanandcaseExecid int null,Execid_Planname_startdate_table nvarchar(MAX) not null FOREIGN KEY (TempPlanBG_ID) references TempPlanBackground(TempPlanBackground_ID) on delete cascade) END";
                adap = new SqlDataAdapter(this.query, this.Report_CreateConnection_ForTables());
                adap.Fill(this.ds, "Test");


                this.query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'TempTestPlanTable' AND COLUMN_NAME = 'Date') BEGIN  ALTER TABLE [dbo].[TempTestPlanTable] ADD [Date] nvarchar(MAX) null END";
                adap = new SqlDataAdapter(this.query, this.Report_CreateConnection_ForTables());
                adap.Fill(this.ds, "Test");

                this.query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'TempTestPlanTable' AND COLUMN_NAME = 'Remarks') BEGIN  ALTER TABLE [dbo].[TempTestPlanTable] ADD [Remarks] nvarchar(MAX) null END";
                adap = new SqlDataAdapter(this.query, this.Report_CreateConnection_ForTables());
                adap.Fill(this.ds, "Test");

                this.query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'TempTestPlanTable' AND COLUMN_NAME = 'TestPlanLogPath') BEGIN  ALTER TABLE [dbo].[TempTestPlanTable] ADD [TestPlanLogPath] nvarchar(MAX) null END";
                adap = new SqlDataAdapter(this.query, this.Report_CreateConnection_ForTables());
                adap.Fill(this.ds, "Test");

                //LoopIteration
                this.query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'TempTestPlanTable' AND COLUMN_NAME = 'TestPlanLoopIteratios') BEGIN  ALTER TABLE [dbo].[TempTestPlanTable] ADD [TestPlanLoopIteratios] nvarchar(MAX) null END";
                adap = new SqlDataAdapter(this.query, this.Report_CreateConnection_ForTables());
                adap.Fill(this.ds, "Test");

                //PlanExecutionUniqueID
                this.query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'TempTestPlanTable' AND COLUMN_NAME = 'PlanExecutionUniqueID') BEGIN  ALTER TABLE [dbo].[TempTestPlanTable] ADD [PlanExecutionUniqueID] int null END";
                adap = new SqlDataAdapter(this.query, this.Report_CreateConnection_ForTables());
                adap.Fill(this.ds, "Test");

                //SuiteAlongwithplanandcaseExecid
                this.query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'TempTestPlanTable' AND COLUMN_NAME = 'SuiteAlongwithplanandcaseExecid') BEGIN  ALTER TABLE [dbo].[TempTestPlanTable] ADD [SuiteAlongwithplanandcaseExecid] int null END";
                adap = new SqlDataAdapter(this.query, this.Report_CreateConnection_ForTables());
                adap.Fill(this.ds, "Test");

                this.query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'TempTestPlanTable' AND COLUMN_NAME = 'Execid_Planname_startdate') BEGIN  ALTER TABLE [dbo].[TempTestPlanTable] ADD [Execid_Planname_startdate] nvarchar(MAX) null END";
                adap = new SqlDataAdapter(this.query, this.Report_CreateConnection_ForTables());
                adap.Fill(this.ds, "Test");

                //this.query = "IF Not EXISTS(SELECT * FROM sys.types WHERE is_table_type = 1 AND name = 'TestPlanTableType') BEGIN CREATE TYPE TestPlanTableType AS TABLE( ExecID int null,status nvarchar(MAX) not null,Testsuitename nvarchar(MAX) not null,Testplanname nvarchar(MAX) not null,TotalIncompleteTestCase nvarchar(MAX) not null,EndTime nvarchar(MAX) not null,Remarks nvarchar(MAX) null,TestPlanLoopIteratios nvarchar(MAX) null,PlanExecutionUniqueID int null,SuiteAlongwithplanandcaseExecid int null) END";
                //adap = new SqlDataAdapter(query, this.Report_CreateConnection_ForTables());
                //adap.Fill(ds, "Test");

                ////this.query = "IF Not EXISTS(SELECT * FROM sys.objects WHERE type = 'p' AND name = 'Update_TestSuiteTable')Begin exec('CREATE PROCEDURE [Update_TestSuiteTable] @tblTempSuiteTable TestSuiteTableType READONLY As BEGIN SET NOCOUNT ON;MERGE INTO TempSuiteTable c1 USING @tblTempSuiteTable  c2 ON c1.ExecID=c2.ExecID and c1.Testsuitename=c2.Testsuitename and c1.TestSuiteLoopIteratios=c2.TestSuiteLoopIteratios and c1.SuiteExecutionUniqueID=c2.SuiteExecutionUniqueID and c1.EndTime= c2.EndTime WHEN MATCHED THEN UPDATE SET c1.TotalIncompleteTestCase= c2.TotalIncompleteTestCase, c1.EndTime= c2.EndTime, c1.Remarks= c2.Remarks; END') end";
                //this.query = "IF Not EXISTS(SELECT * FROM sys.objects WHERE type = 'p' AND name = 'Update_TestPlanTable')Begin exec('CREATE PROCEDURE [Update_TestPlanTable] @tblTempTestPlanTable TestPlanTableType READONLY As BEGIN SET NOCOUNT ON; update TempTestPlanTable set status=c2.status,TotalIncompleteTestCase= c2.TotalIncompleteTestCase, EndTime= c2.EndTime, Remarks= c2.Remarks from TempTestPlanTable c1 INNER JOIN @tblTempTestPlanTable c2  ON  c1.ExecID=c2.ExecID and c1.Testsuitename=c2.Testsuitename and c1.Testplanname=c2.Testplanname and c1.TestPlanLoopIteratios=c2.TestPlanLoopIteratios and c1.SuiteAlongwithplanandcaseExecid=c2.SuiteAlongwithplanandcaseExecid and c1.PlanExecutionUniqueID=c2.PlanExecutionUniqueID END') end";
                //adap = new SqlDataAdapter(query, this.Report_CreateConnection_ForTables());
                //adap.Fill(ds, "Test");


                //TempTestCaseTable      
                query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'TempTestCaseTable') BEGIN Create table TempTestCaseTable(ExecID int null,status nvarchar(MAX) not null,Testsuitename nvarchar(MAX) not null,Testplanname nvarchar(MAX) not null,Testcasename nvarchar(MAX) not null,StartTime nvarchar(MAX) not null,EndTime nvarchar(MAX) not null,Build nvarchar(MAX) not null,OSVersion nvarchar(MAX) not null,DesignName nvarchar(MAX) not null,Inventory nvarchar(MAX) not null,ExecidTabnameTC nvarchar(MAX) not null,FOREIGN KEY (ExecID) references ExecutionTable(ExecutionID) on delete cascade) END";
                adap = new SqlDataAdapter(query, this.Report_CreateConnection_ForTables());
                adap.Fill(ds, "Test");

                this.query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'TempTestCaseTable' AND COLUMN_NAME = 'Date') BEGIN  ALTER TABLE [dbo].[TempTestCaseTable] ADD [Date] nvarchar(MAX) null END";
                adap = new SqlDataAdapter(this.query, this.Report_CreateConnection_ForTables());
                adap.Fill(this.ds, "Test");

                this.query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'TempTestCaseTable' AND COLUMN_NAME = 'Remarks') BEGIN  ALTER TABLE [dbo].[TempTestCaseTable] ADD [Remarks] nvarchar(MAX) null END";
                adap = new SqlDataAdapter(this.query, this.Report_CreateConnection_ForTables());
                adap.Fill(this.ds, "Test");

                this.query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'TempTestCaseTable' AND COLUMN_NAME = 'TestCaseLogPath') BEGIN  ALTER TABLE [dbo].[TempTestCaseTable] ADD [TestCaseLogPath] nvarchar(MAX) null END";
                adap = new SqlDataAdapter(this.query, this.Report_CreateConnection_ForTables());
                adap.Fill(this.ds, "Test");

                //LoopIteration
                this.query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'TempTestCaseTable' AND COLUMN_NAME = 'TestCaseLoopIteratios') BEGIN  ALTER TABLE [dbo].[TempTestCaseTable] ADD [TestCaseLoopIteratios] nvarchar(MAX) null END";
                adap = new SqlDataAdapter(this.query, this.Report_CreateConnection_ForTables());
                adap.Fill(this.ds, "Test");

                //CaseExecutionUniqueID
                this.query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'TempTestCaseTable' AND COLUMN_NAME = 'CaseExecutionUniqueID') BEGIN  ALTER TABLE [dbo].[TempTestCaseTable] ADD [CaseExecutionUniqueID] int null END";
                adap = new SqlDataAdapter(this.query, this.Report_CreateConnection_ForTables());
                adap.Fill(this.ds, "Test");

                //CaseAlogPlanExecutionUniqueID
                this.query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'TempTestCaseTable' AND COLUMN_NAME = 'CaseAlogPlanExecutionUniqueID') BEGIN  ALTER TABLE [dbo].[TempTestCaseTable] ADD [CaseAlogPlanExecutionUniqueID] int null END";
                adap = new SqlDataAdapter(this.query, this.Report_CreateConnection_ForTables());
                adap.Fill(this.ds, "Test");

                //SuiteAlongwithplanandcaseExecid
                this.query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'TempTestCaseTable' AND COLUMN_NAME = 'SuiteAlongwithplanandcaseExecid') BEGIN  ALTER TABLE [dbo].[TempTestCaseTable] ADD [SuiteAlongwithplanandcaseExecid] int null END";
                adap = new SqlDataAdapter(this.query, this.Report_CreateConnection_ForTables());
                adap.Fill(this.ds, "Test");


                //this.query = "IF Not EXISTS(SELECT * FROM sys.types WHERE is_table_type = 1 AND name = 'TestCaseTableType') BEGIN CREATE TYPE TestCaseTableType AS TABLE( ExecID int null,status nvarchar(MAX) not null,Testsuitename nvarchar(MAX) not null,Testplanname nvarchar(MAX) not null,Testcasename nvarchar(MAX) not null,EndTime nvarchar(MAX) not null,Remarks nvarchar(MAX) null,TestCaseLoopIteratios nvarchar(MAX) null,CaseExecutionUniqueID int null,SuiteAlongwithplanandcaseExecid int null) END";
                //adap = new SqlDataAdapter(query, this.Report_CreateConnection_ForTables());
                //adap.Fill(ds, "Test");

                ////this.query = "IF Not EXISTS(SELECT * FROM sys.objects WHERE type = 'p' AND name = 'Update_TestSuiteTable')Begin exec('CREATE PROCEDURE [Update_TestSuiteTable] @tblTempSuiteTable TestSuiteTableType READONLY As BEGIN SET NOCOUNT ON;MERGE INTO TempSuiteTable c1 USING @tblTempSuiteTable  c2 ON c1.ExecID=c2.ExecID and c1.Testsuitename=c2.Testsuitename and c1.TestSuiteLoopIteratios=c2.TestSuiteLoopIteratios and c1.SuiteExecutionUniqueID=c2.SuiteExecutionUniqueID and c1.EndTime= c2.EndTime WHEN MATCHED THEN UPDATE SET c1.TotalIncompleteTestCase= c2.TotalIncompleteTestCase, c1.EndTime= c2.EndTime, c1.Remarks= c2.Remarks; END') end";
                //this.query = "IF Not EXISTS(SELECT * FROM sys.objects WHERE type = 'p' AND name = 'Update_TestCaseTable')Begin exec('CREATE PROCEDURE [Update_TestCaseTable] @tblTempTestCaseTable TestCaseTableType READONLY As BEGIN SET NOCOUNT ON; update TempTestCaseTable set status=c2.status,EndTime= c2.EndTime, Remarks= c2.Remarks from TempTestCaseTable c1 INNER JOIN @tblTempTestCaseTable c2  ON  c1.ExecID=c2.ExecID and c1.Testsuitename=c2.Testsuitename and c1.Testplanname=c2.Testplanname and c1.Testcasename=c2.Testcasename and c1.TestCaseLoopIteratios=c2.TestCaseLoopIteratios and c1.SuiteAlongwithplanandcaseExecid=c2.SuiteAlongwithplanandcaseExecid and c1.CaseExecutionUniqueID=c2.CaseExecutionUniqueID END') end";
                //adap = new SqlDataAdapter(query, this.Report_CreateConnection_ForTables());
                //adap.Fill(ds, "Test");

                //TempTestCaseActionTab                
                query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'TempTestCaseActionTab') BEGIN Create table TempTestCaseActionTab(ExecID int null,status nvarchar(MAX) not null,Testsuitename nvarchar(MAX) not null,Testplanname nvarchar(MAX) not null,Testcasename nvarchar(MAX) not null,Tabname nvarchar(MAX) not null,StartTime nvarchar(MAX) not null,EndTime nvarchar(MAX) not null,ExecidTabname nvarchar(MAX) not null,ExecidTabnameTC nvarchar(MAX) not null,FOREIGN KEY (ExecID) references ExecutionTable(ExecutionID) on delete cascade) END";
                adap = new SqlDataAdapter(query, this.Report_CreateConnection_ForTables());
                adap.Fill(ds, "Test");

                this.query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'TempTestCaseActionTab' AND COLUMN_NAME = 'Remarks') BEGIN  ALTER TABLE [dbo].[TempTestCaseActionTab] ADD [Remarks] nvarchar(MAX) null END";
                adap = new SqlDataAdapter(this.query, this.Report_CreateConnection_ForTables());
                adap.Fill(this.ds, "Test");

                //CaseActionTabExecutionUniqueID
                this.query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'TempTestCaseActionTab' AND COLUMN_NAME = 'CaseActionTabExecutionUniqueID') BEGIN  ALTER TABLE [dbo].[TempTestCaseActionTab] ADD [CaseActionTabExecutionUniqueID] int null END";
                adap = new SqlDataAdapter(this.query, this.Report_CreateConnection_ForTables());
                adap.Fill(this.ds, "Test");


                //ActionTabCaseAlogPlanExecutionUniqueID
                this.query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'TempTestCaseActionTab' AND COLUMN_NAME = 'ActionTabCaseAlogPlanExecutionUniqueID') BEGIN  ALTER TABLE [dbo].[TempTestCaseActionTab] ADD [ActionTabCaseAlogPlanExecutionUniqueID] int null END";
                adap = new SqlDataAdapter(this.query, this.Report_CreateConnection_ForTables());
                adap.Fill(this.ds, "Test");

                //SuiteAlongwithplanandcaseExecid
                this.query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'TempTestCaseActionTab' AND COLUMN_NAME = 'SuiteAlongwithplanandcaseExecid') BEGIN  ALTER TABLE [dbo].[TempTestCaseActionTab] ADD [SuiteAlongwithplanandcaseExecid] int null END";
                adap = new SqlDataAdapter(this.query, this.Report_CreateConnection_ForTables());
                adap.Fill(this.ds, "Test");

                //TempTestCaseActionTabTable
                query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'TempTestCaseActionTabTable') BEGIN Create table TempTestCaseActionTabTable(ExecID int null,status nvarchar(MAX) not null,Testsuitename nvarchar(MAX) not null,Testplanname nvarchar(MAX) not null,Testcasename nvarchar(MAX) not null,Tabname nvarchar(MAX) not null,StartTime nvarchar(MAX) not null,EndTime nvarchar(MAX) not null,VerificationValue nvarchar(MAX) not null,ExecidTabname nvarchar(MAX) not null,FOREIGN KEY (ExecID) references ExecutionTable(ExecutionID) on delete cascade) END";
                adap = new SqlDataAdapter(query, this.Report_CreateConnection_ForTables());
                adap.Fill(ds, "Test");

                this.query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'TempTestCaseActionTabTable' AND COLUMN_NAME = 'Action_Name_Values') BEGIN  ALTER TABLE [dbo].[TempTestCaseActionTabTable] ADD [Action_Name_Values] nvarchar(MAX) null END";
                adap = new SqlDataAdapter(this.query, this.Report_CreateConnection_ForTables());
                adap.Fill(this.ds, "Test");

                this.query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'TempTestCaseActionTabTable' AND COLUMN_NAME = 'Remarks') BEGIN  ALTER TABLE [dbo].[TempTestCaseActionTabTable] ADD [Remarks] nvarchar(MAX) null END";
                adap = new SqlDataAdapter(this.query, this.Report_CreateConnection_ForTables());
                adap.Fill(this.ds, "Test");

                this.query = "IF Exists (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'TempTestCaseActionTabTable' AND COLUMN_NAME = 'VerificationValue') BEGIN  ALTER TABLE [dbo].[TempTestCaseActionTabTable] DROP COLUMN  [VerificationValue] END";
                adap = new SqlDataAdapter(this.query, this.Report_CreateConnection_ForTables());
                adap.Fill(this.ds, "Test");

                this.query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'TempTestCaseActionTabTable' AND COLUMN_NAME = 'Actual_Value') BEGIN  ALTER TABLE [dbo].[TempTestCaseActionTabTable] ADD [Actual_Value] nvarchar(MAX) null END";
                adap = new SqlDataAdapter(this.query, this.Report_CreateConnection_ForTables());
                adap.Fill(this.ds, "Test");

                this.query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'TempTestCaseActionTabTable' AND COLUMN_NAME = 'Expected_Value') BEGIN  ALTER TABLE [dbo].[TempTestCaseActionTabTable] ADD [Expected_Value] nvarchar(MAX) null END";
                adap = new SqlDataAdapter(this.query, this.Report_CreateConnection_ForTables());
                adap.Fill(this.ds, "Test");

                //CaseActionTabTableExecutionUniqueID
                this.query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'TempTestCaseActionTabTable' AND COLUMN_NAME = 'CaseActionTabTableExecutionUniqueID') BEGIN  ALTER TABLE [dbo].[TempTestCaseActionTabTable] ADD [CaseActionTabTableExecutionUniqueID] int null END";
                adap = new SqlDataAdapter(this.query, this.Report_CreateConnection_ForTables());
                adap.Fill(this.ds, "Test");

                //ActionTabTableCaseAlogPlanExecutionUniqueID
                this.query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'TempTestCaseActionTabTable' AND COLUMN_NAME = 'ActionTabTableCaseAlogPlanExecutionUniqueID') BEGIN  ALTER TABLE [dbo].[TempTestCaseActionTabTable] ADD [ActionTabTableCaseAlogPlanExecutionUniqueID] int null END";
                adap = new SqlDataAdapter(this.query, this.Report_CreateConnection_ForTables());
                adap.Fill(this.ds, "Test");

                //SuiteAlongwithplanandcaseExecid
                this.query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'TempTestCaseActionTabTable' AND COLUMN_NAME = 'SuiteAlongwithplanandcaseExecid') BEGIN  ALTER TABLE [dbo].[TempTestCaseActionTabTable] ADD [SuiteAlongwithplanandcaseExecid] int null END";
                adap = new SqlDataAdapter(this.query, this.Report_CreateConnection_ForTables());
                adap.Fill(this.ds, "Test");

                this.query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'TempTestCaseActionTabTable' AND COLUMN_NAME = 'ScriptLogPath') BEGIN  ALTER TABLE [dbo].[TempTestCaseActionTabTable] ADD [ScriptLogPath] nvarchar(MAX) null END";
                adap = new SqlDataAdapter(this.query, this.Report_CreateConnection_ForTables());
                adap.Fill(this.ds, "Test");

                this.query = "IF not EXISTS(SELECT name FROM sys.indexes  WHERE name = 'Execute_index') CREATE NONCLUSTERED INDEX Execute_index  ON TempExecutionTable (ExecID)";
                adap = new SqlDataAdapter(this.query, this.Report_CreateConnection_ForTables());
                adap.SelectCommand.CommandTimeout = 0;
                adap.Fill(this.ds, "Test");

                this.query = "IF not EXISTS(SELECT name FROM sys.indexes  WHERE name = 'Suite_index') CREATE NONCLUSTERED INDEX Suite_index  ON TempSuiteTable (ExecID, SuiteExecutionUniqueID) INCLUDE(Testsuitename)";
                adap = new SqlDataAdapter(this.query, this.Report_CreateConnection_ForTables());
                adap.SelectCommand.CommandTimeout = 0;
                adap.Fill(this.ds, "Test");

                this.query = "IF not EXISTS(SELECT name FROM sys.indexes  WHERE name = 'ActionTab_index') CREATE NONCLUSTERED INDEX ActionTab_index  ON TempTestCaseActionTab (ExecID, CaseActionTabExecutionUniqueID, ActionTabCaseAlogPlanExecutionUniqueID, SuiteAlongwithplanandcaseExecid) INCLUDE(Testcasename, Tabname)";
                adap = new SqlDataAdapter(this.query, this.Report_CreateConnection_ForTables());
                adap.SelectCommand.CommandTimeout = 0;
                adap.Fill(this.ds, "Test");

                this.query = "IF not EXISTS(SELECT name FROM sys.indexes  WHERE name = 'Actionvalues_index') CREATE NONCLUSTERED INDEX Actionvalues_index  ON TempTestCaseActionTabTable (ExecID, CaseActionTabTableExecutionUniqueID, ActionTabTableCaseAlogPlanExecutionUniqueID, SuiteAlongwithplanandcaseExecid) INCLUDE(Testcasename, Tabname)";
                adap = new SqlDataAdapter(this.query, this.Report_CreateConnection_ForTables());
                adap.SelectCommand.CommandTimeout = 0;
                adap.Fill(this.ds, "Test");

                this.query = "IF not EXISTS(SELECT name FROM sys.indexes  WHERE name = 'Testcase_index') CREATE NONCLUSTERED INDEX Testcase_index  ON TempTestCaseTable (ExecID, CaseExecutionUniqueID, CaseAlogPlanExecutionUniqueID, SuiteAlongwithplanandcaseExecid) INCLUDE(Testcasename)";
                adap = new SqlDataAdapter(this.query, this.Report_CreateConnection_ForTables());
                adap.SelectCommand.CommandTimeout = 0;
                adap.Fill(this.ds, "Test");

                this.query = "IF not EXISTS(SELECT name FROM sys.indexes  WHERE name = 'Testplan_index') CREATE NONCLUSTERED INDEX Testplan_index  ON TempTestPlanTable (ExecID, PlanExecutionUniqueID, SuiteAlongwithplanandcaseExecid) INCLUDE(Testplanname)";
                adap = new SqlDataAdapter(this.query, this.Report_CreateConnection_ForTables());
                adap.SelectCommand.CommandTimeout = 0;
                adap.Fill(this.ds, "Test");

                this.Report_CloseConnection();
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                //MessageBox.Show("Exception\n" + error.Message, "QSC_Automation_Toolbox", MessageBoxButton.OK, MessageBoxImage.Error);
                this.Report_CloseConnection();
            }
        }       

        public DataTable Report_SendCommand_Toreceive(string cmdtext, string parameter1, string parameter1Value, string parameter2, string parameter2Value, string parameter3, string parameter3Value, string parameter4, string parameter4Value, string parameter5, string parameter5Value, string parameter6, string parameter6Value)
        {
            DataTable datatable = new DataTable();

            try
            {
                SqlCommand com = new SqlCommand(cmdtext, this.Report_CreateConnection_ForTables());

                if (parameter1 != string.Empty)
                    com.Parameters.AddWithValue(parameter1, parameter1Value);

                if (parameter2 != string.Empty)
                    com.Parameters.AddWithValue(parameter2, parameter2Value);

                if (parameter3 != string.Empty)
                    com.Parameters.AddWithValue(parameter3, parameter3Value);

                if (parameter4 != string.Empty)
                    com.Parameters.AddWithValue(parameter4, parameter4Value);

                if (parameter5 != string.Empty)
                    com.Parameters.AddWithValue(parameter5, parameter5Value);

                if (parameter6 != string.Empty)
                    com.Parameters.AddWithValue(parameter6, parameter6Value);

                SqlDataAdapter adap = new SqlDataAdapter(com);
                this.Report_OpenConnection();
                adap.Update(datatable);
                adap.Fill(datatable);
                this.Report_CloseConnection();
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                //MessageBox.Show("Exception\n " + error.Message, "QSC_Automation_Toolbox", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return datatable;
        }

        public DataTable Report_SendCommand_Toreceive(string cmdtext, Dictionary<string, string> parameterandVal)
        {
            DataTable datatable = new DataTable();

            try
            {
                SqlCommand com = new SqlCommand(cmdtext, this.Report_CreateConnection_ForTables());

                foreach (KeyValuePair<string, string> parwithValue in parameterandVal)
                {
                    if (parwithValue.Key != string.Empty)
                        com.Parameters.AddWithValue(parwithValue.Key, parwithValue.Value);
                }

                SqlDataAdapter adap = new SqlDataAdapter(com);
                this.Report_OpenConnection();
                adap.Update(datatable);
                adap.Fill(datatable);
                this.Report_CloseConnection();
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                //MessageBox.Show("Exception\n " + error.Message, "QSC_Automation_Toolbox", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return datatable;
        }
        
        public DataTable bulkUpdate(System.Data.DataTable tbl,string procedureName,string tempTableName)
        {
            SqlCommand cmd = new SqlCommand();
            SqlDataAdapter adap = new SqlDataAdapter();
            DataTable datatable = tbl;
            try
            {
                cmd = new SqlCommand(procedureName, this.Report_CreateConnection_ForTables());
                cmd.Parameters.AddWithValue(tempTableName, datatable);
                cmd.CommandType = CommandType.StoredProcedure;
                this.Report_OpenConnection();
                adap.SelectCommand = cmd;
                //adap.Update(datatable);
                int rowsAffected = adap.Fill(datatable);
               
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                //MessageBox.Show("Exception\n " + error.Message, "QSC_Automation_Toolbox", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                cmd.Dispose();
                this.Report_CloseConnection();
            }
            return datatable;
         }
       
    }
}
