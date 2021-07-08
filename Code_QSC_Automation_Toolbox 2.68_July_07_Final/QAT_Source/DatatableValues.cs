namespace QSC_Test_Automation
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.SqlClient;
    using System.Windows;

    /// <summary>
    /// Data Tables Values
    /// </summary>
    public class DatatableValues
    {
        private DBConnection QscDatabase = new DBConnection();
        private ArrayList sourcelst = new ArrayList();
        private DataTable datatable1 = new DataTable();
        private SqlCommand cmd = new SqlCommand();
        private DataSet ds = new DataSet();
        private string query = string.Empty;

        public string[] Get_tabledata()
        {
            List<string> lst = new List<string>();

            try
            {
                this.query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Testsuite') BEGIN Create table Testsuite(TestSuiteID int identity(1,1) primary key,Testsuitename nvarchar(MAX) not null) END";
                SqlDataAdapter adap = new SqlDataAdapter(this.query, this.QscDatabase.CreateConnection());
                this.QscDatabase.OpenConnection();
                adap.Fill(this.ds, "Test");

                this.query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Testsuite' AND COLUMN_NAME = 'CreatedOn') BEGIN  ALTER TABLE [dbo].[Testsuite] ADD [CreatedOn] datetime NULL DEFAULT NULL END";
                adap = new SqlDataAdapter(this.query, this.QscDatabase.CreateConnection());
                adap.Fill(this.ds, "Test");

                this.query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Testsuite' AND COLUMN_NAME = 'CreatedBy') BEGIN  ALTER TABLE [dbo].[Testsuite] ADD [CreatedBy] nvarchar(MAX) null END";
                adap = new SqlDataAdapter(this.query, this.QscDatabase.CreateConnection());
                adap.Fill(this.ds, "Test");

                this.query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Testsuite' AND COLUMN_NAME = 'ModifiedOn') BEGIN  ALTER TABLE [dbo].[Testsuite] ADD [ModifiedOn] datetime NULL DEFAULT NULL END";
                adap = new SqlDataAdapter(this.query, this.QscDatabase.CreateConnection());
                adap.Fill(this.ds, "Test");

                this.query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Testsuite' AND COLUMN_NAME = 'ModifiedBy') BEGIN  ALTER TABLE [dbo].[Testsuite] ADD [ModifiedBy] nvarchar(MAX) null END";
                adap = new SqlDataAdapter(this.query, this.QscDatabase.CreateConnection());
                adap.Fill(this.ds, "Test");

                this.query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Testsuite' AND COLUMN_NAME = 'Summary') BEGIN  ALTER TABLE [dbo].[Testsuite] ADD [Summary] nvarchar(MAX) null END";
                adap = new SqlDataAdapter(this.query, this.QscDatabase.CreateConnection());
                adap.Fill(this.ds, "Test");

                this.query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Testsuite' AND COLUMN_NAME = 'Category') BEGIN  ALTER TABLE [dbo].[Testsuite] ADD [Category] nvarchar(MAX) null END";
                adap = new SqlDataAdapter(this.query, this.QscDatabase.CreateConnection());
                adap.Fill(this.ds, "Test");

                this.query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Testsuite' AND COLUMN_NAME = 'EditedBy') BEGIN  ALTER TABLE [dbo].[Testsuite] ADD [EditedBy] nvarchar(MAX) null END";
                adap = new SqlDataAdapter(this.query, this.QscDatabase.CreateConnection());
                adap.Fill(this.ds, "Test");

                this.query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Testsuite' AND COLUMN_NAME = 'TSActioncount') BEGIN  ALTER TABLE [dbo].[Testsuite] ADD [TSActioncount] int null END";
                adap = new SqlDataAdapter(this.query, this.QscDatabase.CreateConnection());
                adap.Fill(this.ds, "Test");


                DataTable tble = null;
                DataTableReader read = null;
                tble = new DataTable();
                tble.Clear();

                this.query = "SELECT Testsuitename FROM Testsuite";
                tble = QscDatabase.SendCommand_Toreceive(this.query);
                read = tble.CreateDataReader();
                while (read.Read())
                {
                    lst.Add(read.GetString(0));
                }
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif

                //MessageBox.Show("Exception\n " + ex.Message, "QAT Error Code - EC01001", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return lst.ToArray();
        }

        public string[] Get_TestSuitetabledata()
        {
            List<string> lst_TestPlan = new List<string>();
            try
            {
                this.query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Testplan') BEGIN Create table Testplan(TestPlanID int identity(1,1) primary key,Testplanname nvarchar(MAX) not null) END";
                SqlDataAdapter adap = new SqlDataAdapter(this.query, this.QscDatabase.CreateConnection());
                this.QscDatabase.OpenConnection();
                adap.Fill(this.ds, "Test");

                this.query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Testplan' AND COLUMN_NAME = 'CreatedOn') BEGIN  ALTER TABLE [dbo].[Testplan] ADD [CreatedOn] datetime NULL DEFAULT NULL END";
                adap = new SqlDataAdapter(this.query, this.QscDatabase.CreateConnection());
                adap.Fill(this.ds, "Test");

                this.query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Testplan' AND COLUMN_NAME = 'CreatedBy') BEGIN  ALTER TABLE [dbo].[Testplan] ADD [CreatedBy] nvarchar(MAX) null END";
                adap = new SqlDataAdapter(this.query, this.QscDatabase.CreateConnection());
                adap.Fill(this.ds, "Test");

                this.query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Testplan' AND COLUMN_NAME = 'ModifiedOn') BEGIN  ALTER TABLE [dbo].[Testplan] ADD [ModifiedOn] datetime NULL DEFAULT NULL END";
                adap = new SqlDataAdapter(this.query, this.QscDatabase.CreateConnection());
                adap.Fill(this.ds, "Test");

                this.query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Testplan' AND COLUMN_NAME = 'ModifiedBy') BEGIN  ALTER TABLE [dbo].[Testplan] ADD [ModifiedBy] nvarchar(MAX) null END";
                adap = new SqlDataAdapter(this.query, this.QscDatabase.CreateConnection());
                adap.Fill(this.ds, "Test");

                this.query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Testplan' AND COLUMN_NAME = 'Summary') BEGIN  ALTER TABLE [dbo].[Testplan] ADD [Summary] nvarchar(MAX) null END";
                adap = new SqlDataAdapter(this.query, this.QscDatabase.CreateConnection());
                adap.Fill(this.ds, "Test");

                this.query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Testplan' AND COLUMN_NAME = 'Category') BEGIN  ALTER TABLE [dbo].[Testplan] ADD [Category] nvarchar(MAX) null END";
                adap = new SqlDataAdapter(this.query, this.QscDatabase.CreateConnection());
                adap.Fill(this.ds, "Test");
                
                this.query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Testplan' AND COLUMN_NAME = 'IsDeployEnable') BEGIN  ALTER TABLE [dbo].[Testplan] ADD [IsDeployEnable] nvarchar(MAX) null END";
                adap = new SqlDataAdapter(this.query, this.QscDatabase.CreateConnection());
                adap.Fill(this.ds, "Test");
                
                this.query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Testplan' AND COLUMN_NAME = 'DeployCount') BEGIN  ALTER TABLE [dbo].[Testplan] ADD [DeployCount] nvarchar(MAX) null END";
                adap = new SqlDataAdapter(this.query, this.QscDatabase.CreateConnection());
                adap.Fill(this.ds, "Test");

                this.query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Testplan' AND COLUMN_NAME = 'EditedBy') BEGIN  ALTER TABLE [dbo].[Testplan] ADD [EditedBy] nvarchar(MAX) null END";
                adap = new SqlDataAdapter(this.query, this.QscDatabase.CreateConnection());
                adap.Fill(this.ds, "Test");

                this.query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Testplan' AND COLUMN_NAME = 'ImportedOn') BEGIN  ALTER TABLE [dbo].[Testplan] ADD [ImportedOn] datetime NULL DEFAULT NULL END";
                adap = new SqlDataAdapter(this.query, this.QscDatabase.CreateConnection());
                adap.Fill(this.ds, "Test");

                this.query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Testplan' AND COLUMN_NAME = 'ImportedBy') BEGIN  ALTER TABLE [dbo].[Testplan] ADD [ImportedBy] nvarchar(MAX) null END";
                adap = new SqlDataAdapter(this.query, this.QscDatabase.CreateConnection());
                adap.Fill(this.ds, "Test");

                this.query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Testplan' AND COLUMN_NAME = 'TPActioncount') BEGIN  ALTER TABLE [dbo].[Testplan] ADD [TPActioncount] int null END";
                adap = new SqlDataAdapter(this.query, this.QscDatabase.CreateConnection());
                adap.Fill(this.ds, "Test");

                this.query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Testplan' AND COLUMN_NAME = 'IsDesign') BEGIN  ALTER TABLE [dbo].[Testplan] ADD [IsDesign] nvarchar(MAX) NOT null  DEFAULT ('True') WITH Values END";
                adap = new SqlDataAdapter(this.query, this.QscDatabase.CreateConnection());
                adap.Fill(this.ds, "Test");
           

                DataTable tble = null;
                DataTableReader read = null;
                tble = new DataTable();
                tble.Clear();

                this.query = "SELECT Testplanname FROM Testplan";
                tble = QscDatabase.SendCommand_Toreceive(this.query);
                read = tble.CreateDataReader();
                while (read.Read())
                {
                    lst_TestPlan.Add(read.GetString(0));
                }
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif

                //MessageBox.Show("Exception\n " + ex.Message, "QAT Error Code - EC01002", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return lst_TestPlan.ToArray();
        }

        public string[] Get_TestModuletabledata()
        {
            List<string> lst_TestCase = new List<string>();

            try
            {
                this.query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Testcase') BEGIN Create table Testcase(TestcaseID int identity(1,1) primary key,Testcasename nvarchar(MAX) not null) END";
                SqlDataAdapter adap = new SqlDataAdapter(this.query, this.QscDatabase.CreateConnection());
                this.QscDatabase.OpenConnection();
                adap.Fill(this.ds, "Test");

                this.query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Testcase' AND COLUMN_NAME = 'TPID') BEGIN  ALTER TABLE [dbo].[TestCase] ADD [TPID] int null END";
                adap = new SqlDataAdapter(this.query, this.QscDatabase.CreateConnection());
                adap.Fill(this.ds, "Test");

                this.query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Testcase' AND COLUMN_NAME = 'CreatedOn') BEGIN  ALTER TABLE [dbo].[Testcase] ADD [CreatedOn] datetime NULL DEFAULT NULL  END";
                adap = new SqlDataAdapter(this.query, this.QscDatabase.CreateConnection());
                adap.Fill(this.ds, "Test");

                this.query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Testcase' AND COLUMN_NAME = 'CreatedBy') BEGIN  ALTER TABLE [dbo].[Testcase] ADD [CreatedBy] nvarchar(MAX) null END";
                adap = new SqlDataAdapter(this.query, this.QscDatabase.CreateConnection());
                adap.Fill(this.ds, "Test");

                this.query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Testcase' AND COLUMN_NAME = 'ModifiedOn') BEGIN  ALTER TABLE [dbo].[Testcase] ADD [ModifiedOn] datetime NULL DEFAULT NULL END";
                adap = new SqlDataAdapter(this.query, this.QscDatabase.CreateConnection());
                adap.Fill(this.ds, "Test");

                this.query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Testcase' AND COLUMN_NAME = 'ModifiedBy') BEGIN  ALTER TABLE [dbo].[Testcase] ADD [ModifiedBy] nvarchar(MAX) null END";
                adap = new SqlDataAdapter(this.query, this.QscDatabase.CreateConnection());
                adap.Fill(this.ds, "Test");

                this.query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Testcase' AND COLUMN_NAME = 'Summary') BEGIN  ALTER TABLE [dbo].[Testcase] ADD [Summary] nvarchar(MAX) null END";
                adap = new SqlDataAdapter(this.query, this.QscDatabase.CreateConnection());
                adap.Fill(this.ds, "Test");

                this.query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Testcase' AND COLUMN_NAME = 'Category') BEGIN  ALTER TABLE [dbo].[Testcase] ADD [Category] nvarchar(MAX) null END";
                adap = new SqlDataAdapter(this.query, this.QscDatabase.CreateConnection());
                adap.Fill(this.ds, "Test");

                this.query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Testcase' AND COLUMN_NAME = 'EditedBy') BEGIN  ALTER TABLE [dbo].[Testcase] ADD [EditedBy]  nvarchar(MAX) null END";
 				adap = new SqlDataAdapter(this.query, this.QscDatabase.CreateConnection());
                adap.Fill(this.ds, "Test");

                this.query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Testcase' AND COLUMN_NAME = 'ImportedOn') BEGIN  ALTER TABLE [dbo].[Testcase] ADD [ImportedOn] datetime NULL DEFAULT NULL END";
                adap = new SqlDataAdapter(this.query, this.QscDatabase.CreateConnection());
                adap.Fill(this.ds, "Test");

                this.query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Testcase' AND COLUMN_NAME = 'ImportedBy') BEGIN  ALTER TABLE [dbo].[Testcase] ADD [ImportedBy]  nvarchar(MAX) null END";
                adap = new SqlDataAdapter(this.query, this.QscDatabase.CreateConnection());
                adap.Fill(this.ds, "Test");
				
				 this.query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Testcase' AND COLUMN_NAME = 'Actioncount') BEGIN  ALTER TABLE [dbo].[Testcase] ADD [Actioncount] int null END";
                adap = new SqlDataAdapter(this.query, this.QscDatabase.CreateConnection());
                adap.Fill(this.ds, "Test");

                DataTable tble = null;
                DataTableReader read = null;
                tble = new DataTable();
                tble.Clear();

                this.query = "SELECT Testcasename FROM Testcase";
                tble = QscDatabase.SendCommand_Toreceive(this.query);
                read = tble.CreateDataReader();
                while (read.Read())
                {
                    lst_TestCase.Add(read.GetString(0)); 
                }
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif

                //MessageBox.Show("Exception\n " + ex.Message, "QAT Error Code - EC01003", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return lst_TestCase.ToArray();
        }

        public void Create_TATables()
        {
            try
            {
                query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'TestAction') BEGIN Create table TestAction(TestActionID int identity(1,1) primary key,TCID int null,ActionTabname nvarchar(MAX) not null,ActionType nvarchar(MAX) not null,Delayvalue nvarchar(MAX) not null,DelayType nvarchar(MAX) not null,VerificationType nvarchar(MAX) not null,Errorittration nvarchar(MAX) not null,ErrorHandlingType nvarchar(MAX) not null,LogAction nvarchar(MAX) not null,ILog nvarchar(MAX) not null,IlogType nvarchar(MAX) not null,ConfigLog nvarchar(MAX) not null,ConfigLogType nvarchar(MAX) not null,EventLog nvarchar(MAX) not null,SIPLog nvarchar(MAX) not null,QsysLog nvarchar(MAX) not null,SoftPhoneLog nvarchar(MAX) not null,UCILog nvarchar(MAX) not null,KernelLog nvarchar(MAX) not null,WindowsLog nvarchar(MAX) not null,FOREIGN KEY (TCID) references testcase(TestcaseID) on delete cascade) END";
                SqlDataAdapter adap = new SqlDataAdapter(query, this.QscDatabase.CreateConnection());
                this.QscDatabase.OpenConnection();
                adap.Fill(ds, "Test");

                this.query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'TestAction' AND COLUMN_NAME = 'VerificationDelay') BEGIN  ALTER TABLE [dbo].[TestAction] ADD [VerificationDelay] nvarchar(MAX) null END";
                adap = new SqlDataAdapter(query, this.QscDatabase.CreateConnection());
                adap.Fill(ds, "Test");

              

                this.query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'TestAction' AND COLUMN_NAME = 'VerificationDelayType') BEGIN  ALTER TABLE [dbo].[TestAction] ADD [VerificationDelayType] nvarchar(MAX) null END";
                adap = new SqlDataAdapter(query, this.QscDatabase.CreateConnection());
                adap.Fill(ds, "Test");

                this.query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'TestAction' AND COLUMN_NAME = 'Rerundelay') BEGIN  ALTER TABLE [dbo].[TestAction] ADD [Rerundelay] nvarchar(MAX) null END";
                adap = new SqlDataAdapter(query, this.QscDatabase.CreateConnection());
                adap.Fill(ds, "Test");

                this.query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'TestAction' AND COLUMN_NAME = 'RerundelayType') BEGIN  ALTER TABLE [dbo].[TestAction] ADD [RerundelayType] nvarchar(MAX) null END";
                adap = new SqlDataAdapter(query, this.QscDatabase.CreateConnection());
                adap.Fill(ds, "Test");

                this.query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'TestAction' AND COLUMN_NAME = 'screenshot') BEGIN  ALTER TABLE [dbo].[TestAction] ADD [screenshot] nvarchar(MAX) null END";
                adap = new SqlDataAdapter(query, this.QscDatabase.CreateConnection());
                adap.Fill(ds, "Test");

                query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'ControlAction') BEGIN Create table ControlAction(ControlActionID int identity(1,1) primary key,TCID int null,ActionID int null,ComponentType nvarchar(MAX) not null,Componentname nvarchar(MAX) not null,ComponentProperty nvarchar(MAX) not null,ComponentChannel nvarchar(MAX) not null,ComponentValue nvarchar(MAX) not null,RampStatus nvarchar(MAX) not null,RampValue nvarchar(MAX) not null,LoopStatus nvarchar(MAX) not null,LoopStartValue nvarchar(MAX) not null,LoopEndValue nvarchar(MAX) not null,LoopIncrValue nvarchar(MAX) not null,FOREIGN KEY (TCID) references testcase(TestcaseID) on delete cascade) END";
                adap = new SqlDataAdapter(query, this.QscDatabase.CreateConnection());
                adap.Fill(ds, "Test");

                this.query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ControlAction' AND COLUMN_NAME = 'PrettyName') BEGIN  ALTER TABLE [dbo].[ControlAction] ADD [PrettyName] nvarchar(MAX) null END";
                adap = new SqlDataAdapter(this.query, this.QscDatabase.CreateConnection());
                adap.Fill(this.ds, "Test");

                this.query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ControlAction' AND COLUMN_NAME = 'InputSelectionType') BEGIN  ALTER TABLE [dbo].[ControlAction] ADD [InputSelectionType] nvarchar(MAX) null END";
                adap = new SqlDataAdapter(this.query, this.QscDatabase.CreateConnection());
                adap.Fill(this.ds, "Test");

                this.query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ControlAction' AND COLUMN_NAME = 'AllChannelControls') BEGIN  ALTER TABLE [dbo].[ControlAction] ADD [AllChannelControls] nvarchar(MAX) null END";
                adap = new SqlDataAdapter(this.query, this.QscDatabase.CreateConnection());
                adap.Fill(this.ds, "Test");

                this.query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ControlAction' AND COLUMN_NAME = 'ControlDatatype') BEGIN  ALTER TABLE [dbo].[ControlAction] ADD [ControlDatatype] nvarchar(MAX) null END";
                adap = new SqlDataAdapter(this.query, this.QscDatabase.CreateConnection());
                adap.Fill(this.ds, "Test");

                query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'ControlVerification') BEGIN Create table ControlVerification(ControlVerificationID int identity(1,1) primary key,TCID int null,ActionID int null,GetParamComponentType nvarchar(MAX) not null,GetParamComponentName nvarchar(MAX) not null,GetParamComponentProperty nvarchar(MAX) not null,GetParamComponentChannel nvarchar(MAX) not null,GetParamComponentValue nvarchar(MAX) not null,FOREIGN KEY (TCID) references testcase(TestcaseID) on delete cascade) END";  
                adap = new SqlDataAdapter(query, this.QscDatabase.CreateConnection());
                adap.Fill(ds, "Test");

                this.query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ControlVerification' AND COLUMN_NAME = 'PrettyName') BEGIN  ALTER TABLE [dbo].[ControlVerification] ADD [PrettyName] nvarchar(MAX) null END";
                adap = new SqlDataAdapter(this.query, this.QscDatabase.CreateConnection());
                adap.Fill(this.ds, "Test");

                this.query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ControlVerification' AND COLUMN_NAME = 'InputSelectionType') BEGIN  ALTER TABLE [dbo].[ControlVerification] ADD [InputSelectionType] nvarchar(MAX) null END";
                adap = new SqlDataAdapter(this.query, this.QscDatabase.CreateConnection());
                adap.Fill(this.ds, "Test");

                this.query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ControlVerification' AND COLUMN_NAME = 'MaximumLimit') BEGIN  ALTER TABLE [dbo].[ControlVerification] ADD [MaximumLimit] nvarchar(MAX) null END";
                adap = new SqlDataAdapter(this.query, this.QscDatabase.CreateConnection());
                adap.Fill(this.ds, "Test");

                this.query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ControlVerification' AND COLUMN_NAME = 'MinimumLimit') BEGIN  ALTER TABLE [dbo].[ControlVerification] ADD [MinimumLimit] nvarchar(MAX) null END";
                adap = new SqlDataAdapter(this.query, this.QscDatabase.CreateConnection());
                adap.Fill(this.ds, "Test");

                this.query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ControlVerification' AND COLUMN_NAME = 'LoopStatus') BEGIN  ALTER TABLE [dbo].[ControlVerification] ADD [LoopStatus] nvarchar(MAX) null END";
                adap = new SqlDataAdapter(this.query, this.QscDatabase.CreateConnection());
                adap.Fill(this.ds, "Test");

                this.query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ControlVerification' AND COLUMN_NAME = 'LoopStartValue') BEGIN  ALTER TABLE [dbo].[ControlVerification] ADD [LoopStartValue] nvarchar(MAX) null END";
                adap = new SqlDataAdapter(this.query, this.QscDatabase.CreateConnection());
                adap.Fill(this.ds, "Test");

                this.query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ControlVerification' AND COLUMN_NAME = 'LoopEndValue') BEGIN  ALTER TABLE [dbo].[ControlVerification] ADD [LoopEndValue] nvarchar(MAX) null END";
                adap = new SqlDataAdapter(this.query, this.QscDatabase.CreateConnection());
                adap.Fill(this.ds, "Test");

                this.query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ControlVerification' AND COLUMN_NAME = 'LoopIncrValue') BEGIN  ALTER TABLE [dbo].[ControlVerification] ADD [LoopIncrValue] nvarchar(MAX) null END";
                adap = new SqlDataAdapter(this.query, this.QscDatabase.CreateConnection());
                adap.Fill(this.ds, "Test");

                this.query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ControlVerification' AND COLUMN_NAME = 'AllChannelControls') BEGIN  ALTER TABLE [dbo].[ControlVerification] ADD [AllChannelControls] nvarchar(MAX) null END";
                adap = new SqlDataAdapter(this.query, this.QscDatabase.CreateConnection());
                adap.Fill(this.ds, "Test");

                this.query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ControlVerification' AND COLUMN_NAME = 'ControlDatatype') BEGIN  ALTER TABLE [dbo].[ControlVerification] ADD [ControlDatatype] nvarchar(MAX) null END";
                adap = new SqlDataAdapter(this.query, this.QscDatabase.CreateConnection());
                adap.Fill(this.ds, "Test");

                query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'TelnetAction') BEGIN Create table TelnetAction(TelnetActionID int identity(1,1) primary key,TCID int null,ActionID int null,telnetcommand nvarchar(MAX) not null, devicesname nvarchar(MAX) not null, FOREIGN KEY (TCID) references testcase(TestcaseID) on delete cascade) END";
                adap = new SqlDataAdapter(query, this.QscDatabase.CreateConnection());
                adap.Fill(ds, "Test");

                query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'TelnetAction' AND COLUMN_NAME = 'DeviceModel') BEGIN  ALTER TABLE [dbo].[TelnetAction] ADD [DeviceModel] nvarchar(MAX) null END";
                adap = new SqlDataAdapter(this.query, this.QscDatabase.CreateConnection());
                adap.Fill(this.ds, "Test");

                query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'TelnetVerify') BEGIN Create table TelnetVerify(TelnetVerifyID int identity(1,1) primary key,TCID int null,ActionID int null,TelnetVerificationType nvarchar(MAX) not null,comparetext nvarchar(MAX) not null,FOREIGN KEY (TCID) references testcase(TestcaseID) on delete cascade) END";  
                adap = new SqlDataAdapter(query, this.QscDatabase.CreateConnection());
                adap.Fill(ds, "Test");

                query = " IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'TelnetVerify' AND COLUMN_NAME = 'keywordType') BEGIN  ALTER TABLE [dbo].[TelnetVerify] ADD [keywordType] nvarchar(MAX) null END";
                adap = new SqlDataAdapter(query, this.QscDatabase.CreateConnection());
                adap.Fill(ds, "Test");

                query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'CECAction') BEGIN Create table CECAction(CECActionID int identity(1,1) primary key,TCID int null,ActionID int null,AdaptorName nvarchar(MAX) not null, devicename nvarchar(MAX) not null, CECCommand nvarchar(MAX) not null, Opcode nvarchar(MAX) not null, FOREIGN KEY (TCID) references testcase(TestcaseID) on delete cascade) END";
                adap = new SqlDataAdapter(query, this.QscDatabase.CreateConnection());
                adap.Fill(ds, "Test");

                query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'CECVerification') BEGIN Create table CECVerification(CECVerificationID int identity(1,1) primary key,TCID int null,ActionID int null,AdaptorName nvarchar(MAX) not null, devicename nvarchar(MAX) not null, CECCommand nvarchar(MAX) not null, Opcode nvarchar(MAX) not null,cecverifyfrom nvarchar(MAX) not null, FOREIGN KEY (TCID) references testcase(TestcaseID) on delete cascade) END";
                adap = new SqlDataAdapter(query, this.QscDatabase.CreateConnection());
                adap.Fill(ds, "Test");

                query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'UserAction') BEGIN Create table UserAction(UserActionID int identity(1,1) primary key,TCID int null, ActionID int null, UserActionText nvarchar(MAX) not null, FOREIGN KEY (TCID) references testcase(TestcaseID) on delete cascade) END";
                adap = new SqlDataAdapter(query, this.QscDatabase.CreateConnection());
                adap.Fill(ds, "Test");

                query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'UserVerification') BEGIN Create table UserVerification(UserVerificationID int identity(1,1) primary key,TCID int null, ActionID int null, UserVerifyText nvarchar(MAX) not null, FOREIGN KEY (TCID) references testcase(TestcaseID) on delete cascade) END";
                adap = new SqlDataAdapter(query, this.QscDatabase.CreateConnection());
                adap.Fill(ds, "Test");


                query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'ScriptVerification') BEGIN Create table ScriptVerification(ScriptVerificationID int identity(1,1) primary key,TCID int null,ActionID int null,DeviceName nvarchar(MAX) not null, DeviceModel nvarchar(MAX) not null, Action nvarchar(MAX) not null, Command nvarchar(MAX) not null,RegexMatch nvarchar(MAX) not null,UpperLimit nvarchar(MAX) not null, LowerLimit nvarchar(MAX) not null,LimitUnit nvarchar(MAX) not null,CheckEveryTimeValue nvarchar(MAX) not null,CheckEveryTimeUnit nvarchar(MAX) not null,DurationTimeValue nvarchar(MAX) not null,DurationTimeUnit nvarchar(MAX) not null,VerifyDesignDevices nvarchar(MAX) not null,ExecuteIterationAvailableValue nvarchar(MAX) not null, CustomCommandAction nvarchar(MAX) not null, FOREIGN KEY (TCID) references testcase(TestcaseID) on delete cascade) END";
                adap = new SqlDataAdapter(query, this.QscDatabase.CreateConnection());
                adap.Fill(ds, "Test");
                
                query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'QRVerification') BEGIN Create table QRVerification(QRVerificationID int identity(1,1) primary key,TCID int null,ActionID int null,cameraname nvarchar(MAX) not null,modelname nvarchar(MAX) not null, QRCode nvarchar(MAX) not null,verifytype nvarchar(MAX) not null, FOREIGN KEY (TCID) references testcase(TestcaseID) on delete cascade) END";
                adap = new SqlDataAdapter(query, this.QscDatabase.CreateConnection());
                adap.Fill(ds, "Test");

                query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'APVerification') BEGIN Create table APVerification(APVerificationID int identity(1,1) primary key,TCID int null,ActionID int null,VerificationType nvarchar(MAX) not null,APxPath nvarchar(MAX) not null,FOREIGN KEY (TCID) references testcase(TestcaseID) on delete cascade) END";
                //query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'APVerification') BEGIN Create table APVerification(APVerificationID int identity(1,1) primary key,TCID int null,ActionID int null,VerificationType nvarchar(MAX) not null,FOREIGN KEY (TCID) references testcase(TestcaseID) on delete cascade) END";
                adap = new SqlDataAdapter(query, this.QscDatabase.CreateConnection());
                adap.Fill(ds, "Test");

                this.query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'APVerification' AND COLUMN_NAME = 'APxPath') BEGIN  ALTER TABLE [dbo].[APVerification] ADD [APxPath] nvarchar(MAX) null END";
                adap = new SqlDataAdapter(this.query, this.QscDatabase.CreateConnection());
                adap.Fill(this.ds, "Test");

                query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'APSettings') BEGIN Create table APSettings(APSettingsID int identity(1,1) primary key,TCID int null,ActionID int null,ModeType nvarchar(MAX) not null,GeneratorON nvarchar(MAX) not null,WaveformType nvarchar(MAX) not null,CH1_LevelTrack nvarchar(MAX) not null,CH1_Level nvarchar(MAX) not null,CH1_DCOffset nvarchar(MAX) not null,CH2_Level nvarchar(MAX) not null,CH2_DCOffset nvarchar(MAX) not null,CH3_Level nvarchar(MAX) not null,CH3_DCOffset nvarchar(MAX) not null,CH4_Level nvarchar(MAX) not null,CH4_DCOffset nvarchar(MAX) not null,CH5_Level nvarchar(MAX) not null,CH5_DCOffset nvarchar(MAX) not null,CH6_Level nvarchar(MAX) not null,CH6_DCOffset nvarchar(MAX) not null,CH7_Level nvarchar(MAX) not null,CH7_DCOffset nvarchar(MAX) not null,CH8_Level nvarchar(MAX) not null,CH8_DCOffset nvarchar(MAX) not null,Freq_A nvarchar(MAX) not null,Freq_B nvarchar(MAX) not null,CH1_Enable nvarchar(MAX) not null,CH2_Enable nvarchar(MAX) not null,CH3_Enable nvarchar(MAX) not null,CH4_Enable nvarchar(MAX) not null,CH5_Enable nvarchar(MAX) not null,CH6_Enable nvarchar(MAX) not null,CH7_Enable nvarchar(MAX) not null,CH8_Enable nvarchar(MAX) not null,ReadingType nvarchar(MAX) not null,TestChannel nvarchar(MAX) not null,Delay nvarchar(MAX) not null, FOREIGN KEY (TCID) references testcase(TestcaseID) on delete cascade) END";
                adap = new SqlDataAdapter(query, this.QscDatabase.CreateConnection());
                adap.Fill(ds, "Test");
				query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'APSettings' AND COLUMN_NAME = 'SeqChannelCount') BEGIN  ALTER TABLE [dbo].[APSettings] ADD [SeqChannelCount] nvarchar(MAX) null END";
                adap = new SqlDataAdapter(query, this.QscDatabase.CreateConnection());
                adap.Fill(ds, "Test");
                query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'APSettings' AND COLUMN_NAME = 'BenchChannelCount') BEGIN  ALTER TABLE [dbo].[APSettings] ADD [BenchChannelCount] nvarchar(MAX) null END";
                adap = new SqlDataAdapter(query, this.QscDatabase.CreateConnection());
                adap.Fill(ds, "Test");
                query = "IF Exists (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'APSettings' AND COLUMN_NAME = 'ReadingType') BEGIN  ALTER TABLE [dbo].[APSettings] DROP COLUMN [ReadingType] END";
                adap = new SqlDataAdapter(query, this.QscDatabase.CreateConnection());
                adap.Fill(ds, "Test");

                query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'APBenchModeInitialSettings') BEGIN Create table APBenchModeInitialSettings(APBenchSettingsID int identity(1,1) primary key,TCID int null,ActionID int null,GeneratorON nvarchar(MAX) not null,WaveformType nvarchar(MAX) not null,CH1_LevelTrack nvarchar(MAX) not null,CH1_Level nvarchar(MAX) not null,CH1_DCOffset nvarchar(MAX) not null,Freq_A nvarchar(MAX) not null,CH1_Enable nvarchar(MAX) not null,CH2_Enable nvarchar(MAX) not null,CH3_Enable nvarchar(MAX) not null,CH4_Enable nvarchar(MAX) not null,CH5_Enable nvarchar(MAX) not null,CH6_Enable nvarchar(MAX) not null,CH7_Enable nvarchar(MAX) not null,CH8_Enable nvarchar(MAX) not null,ReadingType nvarchar(MAX) not null,FOREIGN KEY (TCID) references testcase(TestcaseID) on delete cascade) END";
                adap = new SqlDataAdapter(query, this.QscDatabase.CreateConnection());
                adap.Fill(ds, "Test");
                query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'APBenchModeInitialSettings' AND COLUMN_NAME = 'CH2_Level') BEGIN  ALTER TABLE [dbo].[APBenchModeInitialSettings] ADD [CH2_Level] nvarchar(MAX) null END";
                adap = new SqlDataAdapter(query, this.QscDatabase.CreateConnection());
                adap.Fill(ds, "Test");
                query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'APBenchModeInitialSettings' AND COLUMN_NAME = 'CH3_Level') BEGIN  ALTER TABLE [dbo].[APBenchModeInitialSettings] ADD [CH3_Level] nvarchar(MAX) null END";
                adap = new SqlDataAdapter(query, this.QscDatabase.CreateConnection());
                adap.Fill(ds, "Test");
                query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'APBenchModeInitialSettings' AND COLUMN_NAME = 'CH4_Level') BEGIN  ALTER TABLE [dbo].[APBenchModeInitialSettings] ADD [CH4_Level] nvarchar(MAX) null END";
                adap = new SqlDataAdapter(query, this.QscDatabase.CreateConnection());
                adap.Fill(ds, "Test");
                query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'APBenchModeInitialSettings' AND COLUMN_NAME = 'CH5_Level') BEGIN  ALTER TABLE [dbo].[APBenchModeInitialSettings] ADD [CH5_Level] nvarchar(MAX) null END";
                adap = new SqlDataAdapter(query, this.QscDatabase.CreateConnection());
                adap.Fill(ds, "Test");
                query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'APBenchModeInitialSettings' AND COLUMN_NAME = 'CH6_Level') BEGIN  ALTER TABLE [dbo].[APBenchModeInitialSettings] ADD [CH6_Level] nvarchar(MAX) null END";
                adap = new SqlDataAdapter(query, this.QscDatabase.CreateConnection());
                adap.Fill(ds, "Test");
                query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'APBenchModeInitialSettings' AND COLUMN_NAME = 'CH7_Level') BEGIN  ALTER TABLE [dbo].[APBenchModeInitialSettings] ADD [CH7_Level] nvarchar(MAX) null END";
                adap = new SqlDataAdapter(query, this.QscDatabase.CreateConnection());
                adap.Fill(ds, "Test");
                query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'APBenchModeInitialSettings' AND COLUMN_NAME = 'CH8_Level') BEGIN  ALTER TABLE [dbo].[APBenchModeInitialSettings] ADD [CH8_Level] nvarchar(MAX) null END";
                adap = new SqlDataAdapter(query, this.QscDatabase.CreateConnection());
                adap.Fill(ds, "Test");
                query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'APBenchModeInitialSettings' AND COLUMN_NAME = 'CH2_DCOffset') BEGIN  ALTER TABLE [dbo].[APBenchModeInitialSettings] ADD [CH2_DCOffset] nvarchar(MAX) null END";
                adap = new SqlDataAdapter(query, this.QscDatabase.CreateConnection());
                adap.Fill(ds, "Test");
                query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'APBenchModeInitialSettings' AND COLUMN_NAME = 'CH3_DCOffset') BEGIN  ALTER TABLE [dbo].[APBenchModeInitialSettings] ADD [CH3_DCOffset] nvarchar(MAX) null END";
                adap = new SqlDataAdapter(query, this.QscDatabase.CreateConnection());
                adap.Fill(ds, "Test");
                query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'APBenchModeInitialSettings' AND COLUMN_NAME = 'CH4_DCOffset') BEGIN  ALTER TABLE [dbo].[APBenchModeInitialSettings] ADD [CH4_DCOffset] nvarchar(MAX) null END";
                adap = new SqlDataAdapter(query, this.QscDatabase.CreateConnection());
                adap.Fill(ds, "Test");
                query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'APBenchModeInitialSettings' AND COLUMN_NAME = 'CH5_DCOffset') BEGIN  ALTER TABLE [dbo].[APBenchModeInitialSettings] ADD [CH5_DCOffset] nvarchar(MAX) null END";
                adap = new SqlDataAdapter(query, this.QscDatabase.CreateConnection());
                adap.Fill(ds, "Test");
                query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'APBenchModeInitialSettings' AND COLUMN_NAME = 'CH6_DCOffset') BEGIN  ALTER TABLE [dbo].[APBenchModeInitialSettings] ADD [CH6_DCOffset] nvarchar(MAX) null END";
                adap = new SqlDataAdapter(query, this.QscDatabase.CreateConnection());
                adap.Fill(ds, "Test");
                query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'APBenchModeInitialSettings' AND COLUMN_NAME = 'CH7_DCOffset') BEGIN  ALTER TABLE [dbo].[APBenchModeInitialSettings] ADD [CH7_DCOffset] nvarchar(MAX) null END";
                adap = new SqlDataAdapter(query, this.QscDatabase.CreateConnection());
                adap.Fill(ds, "Test");
                query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'APBenchModeInitialSettings' AND COLUMN_NAME = 'CH8_DCOffset') BEGIN  ALTER TABLE [dbo].[APBenchModeInitialSettings] ADD [CH8_DCOffset] nvarchar(MAX) null END";
                adap = new SqlDataAdapter(query, this.QscDatabase.CreateConnection());
                adap.Fill(ds, "Test");
                query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'APBenchModeInitialSettings' AND COLUMN_NAME = 'ChannelCount') BEGIN  ALTER TABLE [dbo].[APBenchModeInitialSettings] ADD [ChannelCount] nvarchar(MAX) null END";
                adap = new SqlDataAdapter(query, this.QscDatabase.CreateConnection());
                adap.Fill(ds, "Test");
                query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'APBenchModeInitialSettings' AND COLUMN_NAME = 'FreqB') BEGIN  ALTER TABLE [dbo].[APBenchModeInitialSettings] ADD [FreqB] nvarchar(MAX) null END";
                adap = new SqlDataAdapter(query, this.QscDatabase.CreateConnection());
                adap.Fill(ds, "Test");
                query = "IF Exists (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'APBenchModeInitialSettings' AND COLUMN_NAME = 'ReadingType') BEGIN  ALTER TABLE [dbo].[APBenchModeInitialSettings] DROP COLUMN [ReadingType] END";
                adap = new SqlDataAdapter(query, this.QscDatabase.CreateConnection());
                adap.Fill(ds, "Test");

                query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'APSeqModeInitialSettings') BEGIN Create table APSeqModeInitialSettings(APSeqSettingsID int identity(1,1) primary key,TCID int null,ActionID int null,GeneratorON nvarchar(MAX) not null,WaveformType nvarchar(MAX) not null,TestChannel nvarchar(MAX) not null, CH1_LevelTrack nvarchar(MAX) not null,CH1_Level nvarchar(MAX) not null,CH1_DCOffset nvarchar(MAX) not null,Freq_A nvarchar(MAX) not null,Freq_B nvarchar(MAX) not null,CH1_Enable nvarchar(MAX) not null,CH2_Enable nvarchar(MAX) not null,CH3_Enable nvarchar(MAX) not null,CH4_Enable nvarchar(MAX) not null,CH5_Enable nvarchar(MAX) not null,CH6_Enable nvarchar(MAX) not null,CH7_Enable nvarchar(MAX) not null,CH8_Enable nvarchar(MAX) not null,Delay nvarchar(MAX) not null,FOREIGN KEY (TCID) references testcase(TestcaseID) on delete cascade) END";
                adap = new SqlDataAdapter(query, this.QscDatabase.CreateConnection());
                adap.Fill(ds, "Test");
                query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'APSeqModeInitialSettings' AND COLUMN_NAME = 'CH2_Level') BEGIN  ALTER TABLE [dbo].[APSeqModeInitialSettings] ADD [CH2_Level] nvarchar(MAX) null END";
                adap = new SqlDataAdapter(query, this.QscDatabase.CreateConnection());
                adap.Fill(ds, "Test");
                query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'APSeqModeInitialSettings' AND COLUMN_NAME = 'CH3_Level') BEGIN  ALTER TABLE [dbo].[APSeqModeInitialSettings] ADD [CH3_Level] nvarchar(MAX) null END";
                adap = new SqlDataAdapter(query, this.QscDatabase.CreateConnection());
                adap.Fill(ds, "Test");
                query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'APSeqModeInitialSettings' AND COLUMN_NAME = 'CH4_Level') BEGIN  ALTER TABLE [dbo].[APSeqModeInitialSettings] ADD [CH4_Level] nvarchar(MAX) null END";
                adap = new SqlDataAdapter(query, this.QscDatabase.CreateConnection());
                adap.Fill(ds, "Test");
                query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'APSeqModeInitialSettings' AND COLUMN_NAME = 'CH5_Level') BEGIN  ALTER TABLE [dbo].[APSeqModeInitialSettings] ADD [CH5_Level] nvarchar(MAX) null END";
                adap = new SqlDataAdapter(query, this.QscDatabase.CreateConnection());
                adap.Fill(ds, "Test");
                query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'APSeqModeInitialSettings' AND COLUMN_NAME = 'CH6_Level') BEGIN  ALTER TABLE [dbo].[APSeqModeInitialSettings] ADD [CH6_Level] nvarchar(MAX) null END";
                adap = new SqlDataAdapter(query, this.QscDatabase.CreateConnection());
                adap.Fill(ds, "Test");
                query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'APSeqModeInitialSettings' AND COLUMN_NAME = 'CH7_Level') BEGIN  ALTER TABLE [dbo].[APSeqModeInitialSettings] ADD [CH7_Level] nvarchar(MAX) null END";
                adap = new SqlDataAdapter(query, this.QscDatabase.CreateConnection());
                adap.Fill(ds, "Test");
                query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'APSeqModeInitialSettings' AND COLUMN_NAME = 'CH8_Level') BEGIN  ALTER TABLE [dbo].[APSeqModeInitialSettings] ADD [CH8_Level] nvarchar(MAX) null END";
                adap = new SqlDataAdapter(query, this.QscDatabase.CreateConnection());
                adap.Fill(ds, "Test");
                query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'APSeqModeInitialSettings' AND COLUMN_NAME = 'CH2_DCOffset') BEGIN  ALTER TABLE [dbo].[APSeqModeInitialSettings] ADD [CH2_DCOffset] nvarchar(MAX) null END";
                adap = new SqlDataAdapter(query, this.QscDatabase.CreateConnection());
                adap.Fill(ds, "Test");
                query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'APSeqModeInitialSettings' AND COLUMN_NAME = 'CH3_DCOffset') BEGIN  ALTER TABLE [dbo].[APSeqModeInitialSettings] ADD [CH3_DCOffset] nvarchar(MAX) null END";
                adap = new SqlDataAdapter(query, this.QscDatabase.CreateConnection());
                adap.Fill(ds, "Test");
                query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'APSeqModeInitialSettings' AND COLUMN_NAME = 'CH4_DCOffset') BEGIN  ALTER TABLE [dbo].[APSeqModeInitialSettings] ADD [CH4_DCOffset] nvarchar(MAX) null END";
                adap = new SqlDataAdapter(query, this.QscDatabase.CreateConnection());
                adap.Fill(ds, "Test");
                query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'APSeqModeInitialSettings' AND COLUMN_NAME = 'CH5_DCOffset') BEGIN  ALTER TABLE [dbo].[APSeqModeInitialSettings] ADD [CH5_DCOffset] nvarchar(MAX) null END";
                adap = new SqlDataAdapter(query, this.QscDatabase.CreateConnection());
                adap.Fill(ds, "Test");
                query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'APSeqModeInitialSettings' AND COLUMN_NAME = 'CH6_DCOffset') BEGIN  ALTER TABLE [dbo].[APSeqModeInitialSettings] ADD [CH6_DCOffset] nvarchar(MAX) null END";
                adap = new SqlDataAdapter(query, this.QscDatabase.CreateConnection());
                adap.Fill(ds, "Test");
                query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'APSeqModeInitialSettings' AND COLUMN_NAME = 'CH7_DCOffset') BEGIN  ALTER TABLE [dbo].[APSeqModeInitialSettings] ADD [CH7_DCOffset] nvarchar(MAX) null END";
                adap = new SqlDataAdapter(query, this.QscDatabase.CreateConnection());
                adap.Fill(ds, "Test");
                query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'APSeqModeInitialSettings' AND COLUMN_NAME = 'CH8_DCOffset') BEGIN  ALTER TABLE [dbo].[APSeqModeInitialSettings] ADD [CH8_DCOffset] nvarchar(MAX) null END";
                adap = new SqlDataAdapter(query, this.QscDatabase.CreateConnection());
                adap.Fill(ds, "Test");
                query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'APSeqModeInitialSettings' AND COLUMN_NAME = 'ChannelConunt') BEGIN  ALTER TABLE [dbo].[APSeqModeInitialSettings] ADD [ChannelConunt] nvarchar(MAX) null END";
                adap = new SqlDataAdapter(query, this.QscDatabase.CreateConnection());
                adap.Fill(ds, "Test");


                query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'APLevelAndGainInitialSettings') BEGIN Create table APLevelAndGainInitialSettings(APGainSettingsID int identity(1,1) primary key,TCID int null,ActionID int null,GeneratorON nvarchar(MAX) not null,WaveformType nvarchar(MAX) not null, CH1_LevelTrack nvarchar(MAX) not null,CH1_Level nvarchar(MAX) not null,CH1_DCOffset nvarchar(MAX) not null,Freq_A nvarchar(MAX) not null,CH1_Enable nvarchar(MAX) not null,CH2_Enable nvarchar(MAX) not null,CH3_Enable nvarchar(MAX) not null,CH4_Enable nvarchar(MAX) not null,CH5_Enable nvarchar(MAX) not null,CH6_Enable nvarchar(MAX) not null,CH7_Enable nvarchar(MAX) not null,CH8_Enable nvarchar(MAX) not null,FOREIGN KEY (TCID) references testcase(TestcaseID) on delete cascade) END";
                adap = new SqlDataAdapter(query, this.QscDatabase.CreateConnection());
                adap.Fill(ds, "Test");
				query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'APLevelAndGainInitialSettings' AND COLUMN_NAME = 'CH2_Level') BEGIN  ALTER TABLE [dbo].[APLevelAndGainInitialSettings] ADD [CH2_Level] nvarchar(MAX) null END";
                adap = new SqlDataAdapter(query, this.QscDatabase.CreateConnection());
                adap.Fill(ds, "Test");
                query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'APLevelAndGainInitialSettings' AND COLUMN_NAME = 'CH3_Level') BEGIN  ALTER TABLE [dbo].[APLevelAndGainInitialSettings] ADD [CH3_Level] nvarchar(MAX) null END";
                adap = new SqlDataAdapter(query, this.QscDatabase.CreateConnection());
                adap.Fill(ds, "Test");
                query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'APLevelAndGainInitialSettings' AND COLUMN_NAME = 'CH4_Level') BEGIN  ALTER TABLE [dbo].[APLevelAndGainInitialSettings] ADD [CH4_Level] nvarchar(MAX) null END";
                adap = new SqlDataAdapter(query, this.QscDatabase.CreateConnection());
                adap.Fill(ds, "Test");
                query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'APLevelAndGainInitialSettings' AND COLUMN_NAME = 'CH5_Level') BEGIN  ALTER TABLE [dbo].[APLevelAndGainInitialSettings] ADD [CH5_Level] nvarchar(MAX) null END";
                adap = new SqlDataAdapter(query, this.QscDatabase.CreateConnection());
                adap.Fill(ds, "Test");
                query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'APLevelAndGainInitialSettings' AND COLUMN_NAME = 'CH6_Level') BEGIN  ALTER TABLE [dbo].[APLevelAndGainInitialSettings] ADD [CH6_Level] nvarchar(MAX) null END";
                adap = new SqlDataAdapter(query, this.QscDatabase.CreateConnection());
                adap.Fill(ds, "Test");
                query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'APLevelAndGainInitialSettings' AND COLUMN_NAME = 'CH7_Level') BEGIN  ALTER TABLE [dbo].[APLevelAndGainInitialSettings] ADD [CH7_Level] nvarchar(MAX) null END";
                adap = new SqlDataAdapter(query, this.QscDatabase.CreateConnection());
                adap.Fill(ds, "Test");
                query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'APLevelAndGainInitialSettings' AND COLUMN_NAME = 'CH8_Level') BEGIN  ALTER TABLE [dbo].[APLevelAndGainInitialSettings] ADD [CH8_Level] nvarchar(MAX) null END";
                adap = new SqlDataAdapter(query, this.QscDatabase.CreateConnection());
                adap.Fill(ds, "Test");
                query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'APLevelAndGainInitialSettings' AND COLUMN_NAME = 'CH2_DCOffset') BEGIN  ALTER TABLE [dbo].[APLevelAndGainInitialSettings] ADD [CH2_DCOffset] nvarchar(MAX) null END";
                adap = new SqlDataAdapter(query, this.QscDatabase.CreateConnection());
                adap.Fill(ds, "Test");
                query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'APLevelAndGainInitialSettings' AND COLUMN_NAME = 'CH3_DCOffset') BEGIN  ALTER TABLE [dbo].[APLevelAndGainInitialSettings] ADD [CH3_DCOffset] nvarchar(MAX) null END";
                adap = new SqlDataAdapter(query, this.QscDatabase.CreateConnection());
                adap.Fill(ds, "Test");
                query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'APLevelAndGainInitialSettings' AND COLUMN_NAME = 'CH4_DCOffset') BEGIN  ALTER TABLE [dbo].[APLevelAndGainInitialSettings] ADD [CH4_DCOffset] nvarchar(MAX) null END";
                adap = new SqlDataAdapter(query, this.QscDatabase.CreateConnection());
                adap.Fill(ds, "Test");
                query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'APLevelAndGainInitialSettings' AND COLUMN_NAME = 'CH5_DCOffset') BEGIN  ALTER TABLE [dbo].[APLevelAndGainInitialSettings] ADD [CH5_DCOffset] nvarchar(MAX) null END";
                adap = new SqlDataAdapter(query, this.QscDatabase.CreateConnection());
                adap.Fill(ds, "Test");
                query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'APLevelAndGainInitialSettings' AND COLUMN_NAME = 'CH6_DCOffset') BEGIN  ALTER TABLE [dbo].[APLevelAndGainInitialSettings] ADD [CH6_DCOffset] nvarchar(MAX) null END";
                adap = new SqlDataAdapter(query, this.QscDatabase.CreateConnection());
                adap.Fill(ds, "Test");
                query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'APLevelAndGainInitialSettings' AND COLUMN_NAME = 'CH7_DCOffset') BEGIN  ALTER TABLE [dbo].[APLevelAndGainInitialSettings] ADD [CH7_DCOffset] nvarchar(MAX) null END";
                adap = new SqlDataAdapter(query, this.QscDatabase.CreateConnection());
                adap.Fill(ds, "Test");
                query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'APLevelAndGainInitialSettings' AND COLUMN_NAME = 'CH8_DCOffset') BEGIN  ALTER TABLE [dbo].[APLevelAndGainInitialSettings] ADD [CH8_DCOffset] nvarchar(MAX) null END";
                adap = new SqlDataAdapter(query, this.QscDatabase.CreateConnection());
                adap.Fill(ds, "Test");
                query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'APLevelAndGainInitialSettings' AND COLUMN_NAME = 'ChannelCount') BEGIN  ALTER TABLE [dbo].[APLevelAndGainInitialSettings] ADD [ChannelCount] nvarchar(MAX) null END";
                adap = new SqlDataAdapter(query, this.QscDatabase.CreateConnection());
                adap.Fill(ds, "Test");
                query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'APLevelAndGainInitialSettings' AND COLUMN_NAME = 'FreqB') BEGIN  ALTER TABLE [dbo].[APLevelAndGainInitialSettings] ADD [FreqB] nvarchar(MAX) null END";
                adap = new SqlDataAdapter(query, this.QscDatabase.CreateConnection());
                adap.Fill(ds, "Test");
                query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'APLevelAndGainInitialSettings' AND COLUMN_NAME = 'InputChCount') BEGIN  ALTER TABLE [dbo].[APLevelAndGainInitialSettings] ADD [InputChCount] nvarchar(MAX) null END";
                adap = new SqlDataAdapter(query, this.QscDatabase.CreateConnection());
                adap.Fill(ds, "Test");


                query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'LevelAndGain') BEGIN Create table LevelAndGain(LevelAndGainID int identity(1,1) primary key,TCID int null,ActionID int null,GeneratorON nvarchar(MAX) not null,WaveformType nvarchar(MAX) not null,CH1_LevelTrack nvarchar(MAX) not null,CH1_Level nvarchar(MAX) not null,CH1_DCOffset nvarchar(MAX) not null,CH2_Level nvarchar(MAX) not null,CH2_DCOffset nvarchar(MAX) not null,CH3_Level nvarchar(MAX) not null,CH3_DCOffset nvarchar(MAX) not null,CH4_Level nvarchar(MAX) not null,CH4_DCOffset nvarchar(MAX) not null,CH5_Level nvarchar(MAX) not null,CH5_DCOffset nvarchar(MAX) not null,CH6_Level nvarchar(MAX) not null,CH6_DCOffset nvarchar(MAX) not null,CH7_Level nvarchar(MAX) not null,CH7_DCOffset nvarchar(MAX) not null,CH8_Level nvarchar(MAX) not null,CH8_DCOffset nvarchar(MAX) not null,Freq nvarchar(MAX) not null,CH1_Enable nvarchar(MAX) not null,CH2_Enable nvarchar(MAX) not null,CH3_Enable nvarchar(MAX) not null,CH4_Enable nvarchar(MAX) not null,CH5_Enable nvarchar(MAX) not null,CH6_Enable nvarchar(MAX) not null,CH7_Enable nvarchar(MAX) not null,CH8_Enable nvarchar(MAX) not null,FOREIGN KEY (TCID) references testcase(TestcaseID) on delete cascade) END";
                adap = new SqlDataAdapter(query, this.QscDatabase.CreateConnection());
                adap.Fill(ds, "Test");

                query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'LevelAndGain' AND COLUMN_NAME = 'GainCh1') BEGIN  ALTER TABLE [dbo].[LevelAndGain] ADD [GainCh1] nvarchar(MAX) null END";
                adap = new SqlDataAdapter(query, this.QscDatabase.CreateConnection());
                adap.Fill(ds, "Test");
                query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'LevelAndGain' AND COLUMN_NAME = 'GainCh2') BEGIN  ALTER TABLE [dbo].[LevelAndGain] ADD [GainCh2] nvarchar(MAX) null END";
                adap = new SqlDataAdapter(query, this.QscDatabase.CreateConnection());
                adap.Fill(ds, "Test");
                query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'LevelAndGain' AND COLUMN_NAME = 'GainCh3') BEGIN  ALTER TABLE [dbo].[LevelAndGain] ADD [GainCh3] nvarchar(MAX) null END";
                adap = new SqlDataAdapter(query, this.QscDatabase.CreateConnection());
                adap.Fill(ds, "Test");
                query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'LevelAndGain' AND COLUMN_NAME = 'GainCh4') BEGIN  ALTER TABLE [dbo].[LevelAndGain] ADD [GainCh4] nvarchar(MAX) null END";
                adap = new SqlDataAdapter(query, this.QscDatabase.CreateConnection());
                adap.Fill(ds, "Test");
                query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'LevelAndGain' AND COLUMN_NAME = 'GainCh5') BEGIN  ALTER TABLE [dbo].[LevelAndGain] ADD [GainCh5] nvarchar(MAX) null END";
                adap = new SqlDataAdapter(query, this.QscDatabase.CreateConnection());
                adap.Fill(ds, "Test");
                query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'LevelAndGain' AND COLUMN_NAME = 'GainCh6') BEGIN  ALTER TABLE [dbo].[LevelAndGain] ADD [GainCh6] nvarchar(MAX) null END";
                adap = new SqlDataAdapter(query, this.QscDatabase.CreateConnection());
                adap.Fill(ds, "Test");
                query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'LevelAndGain' AND COLUMN_NAME = 'GainCh7') BEGIN  ALTER TABLE [dbo].[LevelAndGain] ADD [GainCh7] nvarchar(MAX) null END";
                adap = new SqlDataAdapter(query, this.QscDatabase.CreateConnection());
                adap.Fill(ds, "Test");
                query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'LevelAndGain' AND COLUMN_NAME = 'GainCh8') BEGIN  ALTER TABLE [dbo].[LevelAndGain] ADD [GainCh8] nvarchar(MAX) null END";
                adap = new SqlDataAdapter(query, this.QscDatabase.CreateConnection());
                adap.Fill(ds, "Test");
                query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'LevelAndGain' AND COLUMN_NAME = 'ChannelCount') BEGIN  ALTER TABLE [dbo].[LevelAndGain] ADD [ChannelCount] nvarchar(MAX) null END";
                adap = new SqlDataAdapter(query, this.QscDatabase.CreateConnection());
                adap.Fill(ds, "Test");
                query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'LevelAndGain' AND COLUMN_NAME = 'FreqB') BEGIN  ALTER TABLE [dbo].[LevelAndGain] ADD [FreqB] nvarchar(MAX) null END";
                adap = new SqlDataAdapter(query, this.QscDatabase.CreateConnection());
                adap.Fill(ds, "Test");
                query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'LevelAndGain' AND COLUMN_NAME = 'UpperGainCh1') BEGIN  ALTER TABLE [dbo].[LevelAndGain] ADD [UpperGainCh1] nvarchar(MAX) null END";
                adap = new SqlDataAdapter(query, this.QscDatabase.CreateConnection());
                adap.Fill(ds, "Test");
                query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'LevelAndGain' AND COLUMN_NAME = 'UpperGainCh2') BEGIN  ALTER TABLE [dbo].[LevelAndGain] ADD [UpperGainCh2] nvarchar(MAX) null END";
                adap = new SqlDataAdapter(query, this.QscDatabase.CreateConnection());
                adap.Fill(ds, "Test");
                query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'LevelAndGain' AND COLUMN_NAME = 'UpperGainCh3') BEGIN  ALTER TABLE [dbo].[LevelAndGain] ADD [UpperGainCh3] nvarchar(MAX) null END";
                adap = new SqlDataAdapter(query, this.QscDatabase.CreateConnection());
                adap.Fill(ds, "Test");
                query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'LevelAndGain' AND COLUMN_NAME = 'UpperGainCh4') BEGIN  ALTER TABLE [dbo].[LevelAndGain] ADD [UpperGainCh4] nvarchar(MAX) null END";
                adap = new SqlDataAdapter(query, this.QscDatabase.CreateConnection());
                adap.Fill(ds, "Test");
                query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'LevelAndGain' AND COLUMN_NAME = 'UpperGainCh5') BEGIN  ALTER TABLE [dbo].[LevelAndGain] ADD [UpperGainCh5] nvarchar(MAX) null END";
                adap = new SqlDataAdapter(query, this.QscDatabase.CreateConnection());
                adap.Fill(ds, "Test");
                query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'LevelAndGain' AND COLUMN_NAME = 'UpperGainCh6') BEGIN  ALTER TABLE [dbo].[LevelAndGain] ADD [UpperGainCh6] nvarchar(MAX) null END";
                adap = new SqlDataAdapter(query, this.QscDatabase.CreateConnection());
                adap.Fill(ds, "Test");
                query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'LevelAndGain' AND COLUMN_NAME = 'UpperGainCh7') BEGIN  ALTER TABLE [dbo].[LevelAndGain] ADD [UpperGainCh7] nvarchar(MAX) null END";
                adap = new SqlDataAdapter(query, this.QscDatabase.CreateConnection());
                adap.Fill(ds, "Test");
                query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'LevelAndGain' AND COLUMN_NAME = 'UpperGainCh8') BEGIN  ALTER TABLE [dbo].[LevelAndGain] ADD [UpperGainCh8] nvarchar(MAX) null END";
                adap = new SqlDataAdapter(query, this.QscDatabase.CreateConnection());
                adap.Fill(ds, "Test");
                query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'LevelAndGain' AND COLUMN_NAME = 'LowerGainCh1') BEGIN  ALTER TABLE [dbo].[LevelAndGain] ADD [LowerGainCh1] nvarchar(MAX) null END";
                adap = new SqlDataAdapter(query, this.QscDatabase.CreateConnection());
                adap.Fill(ds, "Test");
                query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'LevelAndGain' AND COLUMN_NAME = 'LowerGainCh2') BEGIN  ALTER TABLE [dbo].[LevelAndGain] ADD [LowerGainCh2] nvarchar(MAX) null END";
                adap = new SqlDataAdapter(query, this.QscDatabase.CreateConnection());
                adap.Fill(ds, "Test");
                query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'LevelAndGain' AND COLUMN_NAME = 'LowerGainCh3') BEGIN  ALTER TABLE [dbo].[LevelAndGain] ADD [LowerGainCh3] nvarchar(MAX) null END";
                adap = new SqlDataAdapter(query, this.QscDatabase.CreateConnection());
                adap.Fill(ds, "Test");
                query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'LevelAndGain' AND COLUMN_NAME = 'LowerGainCh4') BEGIN  ALTER TABLE [dbo].[LevelAndGain] ADD [LowerGainCh4] nvarchar(MAX) null END";
                adap = new SqlDataAdapter(query, this.QscDatabase.CreateConnection());
                adap.Fill(ds, "Test");
                query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'LevelAndGain' AND COLUMN_NAME = 'LowerGainCh5') BEGIN  ALTER TABLE [dbo].[LevelAndGain] ADD [LowerGainCh5] nvarchar(MAX) null END";
                adap = new SqlDataAdapter(query, this.QscDatabase.CreateConnection());
                adap.Fill(ds, "Test");
                query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'LevelAndGain' AND COLUMN_NAME = 'LowerGainCh6') BEGIN  ALTER TABLE [dbo].[LevelAndGain] ADD [LowerGainCh6] nvarchar(MAX) null END";
                adap = new SqlDataAdapter(query, this.QscDatabase.CreateConnection());
                adap.Fill(ds, "Test");
                query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'LevelAndGain' AND COLUMN_NAME = 'LowerGainCh7') BEGIN  ALTER TABLE [dbo].[LevelAndGain] ADD [LowerGainCh7] nvarchar(MAX) null END";
                adap = new SqlDataAdapter(query, this.QscDatabase.CreateConnection());
                adap.Fill(ds, "Test");
                query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'LevelAndGain' AND COLUMN_NAME = 'LowerGainCh8') BEGIN  ALTER TABLE [dbo].[LevelAndGain] ADD [LowerGainCh8] nvarchar(MAX) null END";
                adap = new SqlDataAdapter(query, this.QscDatabase.CreateConnection());
                adap.Fill(ds, "Test");
                query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'LevelAndGain' AND COLUMN_NAME = 'InputChCount') BEGIN  ALTER TABLE [dbo].[LevelAndGain] ADD [InputChCount] nvarchar(MAX) null END";
                adap = new SqlDataAdapter(query, this.QscDatabase.CreateConnection());
                adap.Fill(ds, "Test");

                query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'APFrequencyResponseInitialSettings') BEGIN Create table APFrequencyResponseInitialSettings(APFrqSettingsID int identity(1,1) primary key,TCID int null,ActionID int null,StartGenOn nvarchar(MAX) not null,StartFreq nvarchar(MAX) not null, StopFreq nvarchar(MAX) not null,Level nvarchar(MAX) not null,CH1Level nvarchar(MAX) not null,CH2Level nvarchar(MAX) not null,CH3Level nvarchar(MAX) not null,CH4Level nvarchar(MAX) not null,CH5Level nvarchar(MAX) not null,CH6Level nvarchar(MAX) not null,CH7Level nvarchar(MAX) not null,CH8Level nvarchar(MAX) not null,OutChCount nvarchar(MAX) not null,FOREIGN KEY (TCID) references testcase(TestcaseID) on delete cascade) END";
                adap = new SqlDataAdapter(query, this.QscDatabase.CreateConnection());
                adap.Fill(ds, "Test");

                query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'APFrequencyResponse') BEGIN Create table APFrequencyResponse(APFrqSettingsID int identity(1,1) primary key,TCID int null,ActionID int null,StartGenOn nvarchar(MAX) not null,StartFreq nvarchar(MAX) not null, StopFreq nvarchar(MAX) not null,Level nvarchar(MAX) not null,CH1Level nvarchar(MAX) not null,CH2Level nvarchar(MAX) not null,CH3Level nvarchar(MAX) not null,CH4Level nvarchar(MAX) not null,CH5Level nvarchar(MAX) not null,CH6Level nvarchar(MAX) not null,CH7Level nvarchar(MAX) not null,CH8Level nvarchar(MAX) not null,OutChCount nvarchar(MAX) not null,FOREIGN KEY (TCID) references testcase(TestcaseID) on delete cascade) END";
                adap = new SqlDataAdapter(query, this.QscDatabase.CreateConnection());
                adap.Fill(ds, "Test");
                query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'APFrequencyResponse' AND COLUMN_NAME = 'VerificationLocation') BEGIN  ALTER TABLE [dbo].[APFrequencyResponse] ADD [VerificationLocation] nvarchar(MAX) null END";
                adap = new SqlDataAdapter(query, this.QscDatabase.CreateConnection());
                adap.Fill(ds, "Test");

                query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'APPhaseInitialSettings') BEGIN Create table APPhaseInitialSettings(APPhaseSettingsID int identity(1,1) primary key,TCID int null,ActionID int null,GeneratorOn nvarchar(MAX) not null,TrackCh nvarchar(MAX) not null, Ch1Level nvarchar(MAX) not null,Ch2Level nvarchar(MAX) not null,CH3Level nvarchar(MAX) not null,CH4Level nvarchar(MAX) not null,CH5Level nvarchar(MAX) not null,CH6Level nvarchar(MAX) not null,CH7Level nvarchar(MAX) not null,CH8Level nvarchar(MAX) not null,FrequencyA nvarchar(MAX) not null,Ch1Enable nvarchar(MAX) not null,Ch2Enable nvarchar(MAX) not null,Ch3Enable nvarchar(MAX) not null,Ch4Enable nvarchar(MAX) not null,Ch5Enable nvarchar(MAX) not null,Ch6Enable nvarchar(MAX) not null,Ch7Enable nvarchar(MAX) not null,Ch8Enable nvarchar(MAX) not null, RefCahnnel nvarchar(MAX) not null, MeterRange nvarchar(MAX) not null,OutChCount nvarchar(MAX) not null,InChCount nvarchar(MAX) not null,FOREIGN KEY (TCID) references testcase(TestcaseID) on delete cascade) END";
                adap = new SqlDataAdapter(query, this.QscDatabase.CreateConnection());
                adap.Fill(ds, "Test");

                query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'APPhaseSettings') BEGIN Create table APPhaseSettings(APPhaseSettingsID int identity(1,1) primary key,TCID int null,ActionID int null,GeneratorOn nvarchar(MAX) not null,TrackCh nvarchar(MAX) not null, Ch1Level nvarchar(MAX) not null,Ch2Level nvarchar(MAX) not null,CH3Level nvarchar(MAX) not null,CH4Level nvarchar(MAX) not null,CH5Level nvarchar(MAX) not null,CH6Level nvarchar(MAX) not null,CH7Level nvarchar(MAX) not null,CH8Level nvarchar(MAX) not null,FrequencyA nvarchar(MAX) not null,Ch1Enable nvarchar(MAX) not null,Ch2Enable nvarchar(MAX) not null,Ch3Enable nvarchar(MAX) not null,Ch4Enable nvarchar(MAX) not null,Ch5Enable nvarchar(MAX) not null,Ch6Enable nvarchar(MAX) not null,Ch7Enable nvarchar(MAX) not null,Ch8Enable nvarchar(MAX) not null, RefCahnnel nvarchar(MAX) not null, MeterRange nvarchar(MAX) not null,OutChCount nvarchar(MAX) not null,InChCount nvarchar(MAX) not null,FOREIGN KEY (TCID) references testcase(TestcaseID) on delete cascade) END";
                adap = new SqlDataAdapter(query, this.QscDatabase.CreateConnection());
                adap.Fill(ds, "Test");				
				query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'APPhaseSettings' AND COLUMN_NAME = 'VerificationLocation') BEGIN  ALTER TABLE [dbo].[APPhaseSettings] ADD [VerificationLocation] nvarchar(MAX) null END";
                adap = new SqlDataAdapter(query, this.QscDatabase.CreateConnection());
                adap.Fill(ds, "Test");

                query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'APSteppedFreqSweepInitialSettings') BEGIN Create table APSteppedFreqSweepInitialSettings(APSteppeedFreqSettingsID int identity(1,1) primary key,TCID int null,ActionID int null,GeneratorOn nvarchar(MAX) not null,StartFrequency nvarchar(MAX) not null, StopFrequency nvarchar(MAX) not null,Sweep nvarchar(MAX) not null,Points nvarchar(MAX) not null,CH1Level nvarchar(MAX) not null,Ch1Enable nvarchar(MAX) not null,Ch2Enable nvarchar(MAX) not null,Ch3Enable nvarchar(MAX) not null,Ch4Enable nvarchar(MAX) not null,Ch5Enable nvarchar(MAX) not null,Ch6Enable nvarchar(MAX) not null,Ch7Enable nvarchar(MAX) not null,Ch8Enable nvarchar(MAX) not null, PhaseRefCahnnel nvarchar(MAX) not null, OutChCount nvarchar(MAX) not null,InChCount nvarchar(MAX) not null,FOREIGN KEY (TCID) references testcase(TestcaseID) on delete cascade) END";
                adap = new SqlDataAdapter(query, this.QscDatabase.CreateConnection());
                adap.Fill(ds, "Test");

                query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'APSteppedFreqSweepSettings') BEGIN Create table APSteppedFreqSweepSettings(APSteppeedFreqSettingsID int identity(1,1) primary key,TCID int null,ActionID int null,GeneratorOn nvarchar(MAX) not null,StartFrequency nvarchar(MAX) not null, StopFrequency nvarchar(MAX) not null,Sweep nvarchar(MAX) not null,Points nvarchar(MAX) not null,CH1Level nvarchar(MAX) not null,Ch1Enable nvarchar(MAX) not null,Ch2Enable nvarchar(MAX) not null,Ch3Enable nvarchar(MAX) not null,Ch4Enable nvarchar(MAX) not null,Ch5Enable nvarchar(MAX) not null,Ch6Enable nvarchar(MAX) not null,Ch7Enable nvarchar(MAX) not null,Ch8Enable nvarchar(MAX) not null, PhaseRefCahnnel nvarchar(MAX) not null, OutChCount nvarchar(MAX) not null,InChCount nvarchar(MAX) not null,FOREIGN KEY (TCID) references testcase(TestcaseID) on delete cascade) END";
                adap = new SqlDataAdapter(query, this.QscDatabase.CreateConnection());
                adap.Fill(ds, "Test");

                query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'APSteppedFreqSweepSettings' AND COLUMN_NAME = 'VerificationLocation') BEGIN  ALTER TABLE [dbo].[APSteppedFreqSweepSettings] ADD [VerificationLocation] nvarchar(MAX) null END";
                adap = new SqlDataAdapter(query, this.QscDatabase.CreateConnection());
                adap.Fill(ds, "Test");

                query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'APTHDNInitialSettings') BEGIN Create table APTHDNInitialSettings(APPTHDNSettingsID int identity(1,1) primary key,TCID int null,ActionID int null,GeneratorOn nvarchar(MAX) not null,TrackCh nvarchar(MAX) not null, Ch1Level nvarchar(MAX) not null,Ch2Level nvarchar(MAX) not null,CH3Level nvarchar(MAX) not null,CH4Level nvarchar(MAX) not null,CH5Level nvarchar(MAX) not null,CH6Level nvarchar(MAX) not null,CH7Level nvarchar(MAX) not null,CH8Level nvarchar(MAX) not null,Frequency nvarchar(MAX) not null,Ch1Enable nvarchar(MAX) not null,Ch2Enable nvarchar(MAX) not null,Ch3Enable nvarchar(MAX) not null,Ch4Enable nvarchar(MAX) not null,Ch5Enable nvarchar(MAX) not null,Ch6Enable nvarchar(MAX) not null,Ch7Enable nvarchar(MAX) not null,Ch8Enable nvarchar(MAX) not null, LowPassFilter nvarchar(MAX) not null, HighPassFilter nvarchar(MAX) not null, Weighting nvarchar(MAX) not null, TuningMode nvarchar(MAX) not null, OutChCount nvarchar(MAX) not null, FOREIGN KEY (TCID) references testcase(TestcaseID) on delete cascade) END";
                adap = new SqlDataAdapter(query, this.QscDatabase.CreateConnection());
                adap.Fill(ds, "Test");

                query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'APTHDNSettings') BEGIN Create table APTHDNSettings(APPTHDNSettingsID int identity(1,1) primary key,TCID int null,ActionID int null,GeneratorOn nvarchar(MAX) not null,TrackCh nvarchar(MAX) not null, Ch1Level nvarchar(MAX) not null,Ch2Level nvarchar(MAX) not null,CH3Level nvarchar(MAX) not null,CH4Level nvarchar(MAX) not null,CH5Level nvarchar(MAX) not null,CH6Level nvarchar(MAX) not null,CH7Level nvarchar(MAX) not null,CH8Level nvarchar(MAX) not null,Frequency nvarchar(MAX) not null,Ch1Enable nvarchar(MAX) not null,Ch2Enable nvarchar(MAX) not null,Ch3Enable nvarchar(MAX) not null,Ch4Enable nvarchar(MAX) not null,Ch5Enable nvarchar(MAX) not null,Ch6Enable nvarchar(MAX) not null,Ch7Enable nvarchar(MAX) not null,Ch8Enable nvarchar(MAX) not null, LowPassFilter nvarchar(MAX) not null, HighPassFilter nvarchar(MAX) not null, Weighting nvarchar(MAX) not null, TuningMode nvarchar(MAX) not null, OutChCount nvarchar(MAX) not null, FOREIGN KEY (TCID) references testcase(TestcaseID) on delete cascade) END";
                adap = new SqlDataAdapter(query, this.QscDatabase.CreateConnection());
                adap.Fill(ds, "Test");

                query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'APTHDNInitialSettings' AND COLUMN_NAME = 'TxtLowPassValue') BEGIN  ALTER TABLE [dbo].[APTHDNInitialSettings] ADD [TxtLowPassValue] nvarchar(MAX) null END";
                adap = new SqlDataAdapter(query, this.QscDatabase.CreateConnection());
                adap.Fill(ds, "Test");

                query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'APTHDNInitialSettings' AND COLUMN_NAME = 'TxtHighPassValue') BEGIN  ALTER TABLE [dbo].[APTHDNInitialSettings] ADD [TxtHighPassValue] nvarchar(MAX) null END";
                adap = new SqlDataAdapter(query, this.QscDatabase.CreateConnection());
                adap.Fill(ds, "Test");

                query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'APTHDNInitialSettings' AND COLUMN_NAME = 'FilterFrequency') BEGIN  ALTER TABLE [dbo].[APTHDNInitialSettings] ADD [FilterFrequency] nvarchar(MAX) null END";
                adap = new SqlDataAdapter(query, this.QscDatabase.CreateConnection());
                adap.Fill(ds, "Test");

                query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'APTHDNSettings' AND COLUMN_NAME = 'TxtLowPassValue') BEGIN  ALTER TABLE [dbo].[APTHDNSettings] ADD [TxtLowPassValue] nvarchar(MAX) null END";
                adap = new SqlDataAdapter(query, this.QscDatabase.CreateConnection());
                adap.Fill(ds, "Test");

                query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'APTHDNSettings' AND COLUMN_NAME = 'TxtHighPassValue') BEGIN  ALTER TABLE [dbo].[APTHDNSettings] ADD [TxtHighPassValue] nvarchar(MAX) null END";
                adap = new SqlDataAdapter(query, this.QscDatabase.CreateConnection());
                adap.Fill(ds, "Test");

                query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'APTHDNSettings' AND COLUMN_NAME = 'FilterFrequency') BEGIN  ALTER TABLE [dbo].[APTHDNSettings] ADD [FilterFrequency] nvarchar(MAX) null END";
                adap = new SqlDataAdapter(query, this.QscDatabase.CreateConnection());
                adap.Fill(ds, "Test");
				
				query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'APTHDNSettings' AND COLUMN_NAME = 'VerificationLocation') BEGIN  ALTER TABLE [dbo].[APTHDNSettings] ADD [VerificationLocation] nvarchar(MAX) null END";
                adap = new SqlDataAdapter(query, this.QscDatabase.CreateConnection());
                adap.Fill(ds, "Test");

                query = "if not exists (select * from sys.objects where [type] = 'TR' and [name] = 'Actiondelete') exec('CREATE TRIGGER Actiondelete ON TestAction AFTER DELETE AS DELETE FROM controlAction where ActionID IN(SELECT deleted.TestActionID FROM deleted) DELETE FROM controlverification where ActionID IN(SELECT deleted.TestActionID FROM deleted) DELETE FROM TelnetAction where ActionID IN(SELECT deleted.TestActionID FROM deleted) DELETE FROM TelnetVerify where ActionID IN(SELECT deleted.TestActionID FROM deleted)  DELETE FROM Firmwareaction where ActionID IN(SELECT deleted.TestActionID FROM deleted) DELETE FROM APSettings where ActionID IN(SELECT deleted.TestActionID FROM deleted) DELETE FROM APVerification where ActionID IN(SELECT deleted.TestActionID FROM deleted) DELETE FROM LevelAndGain where ActionID IN(SELECT deleted.TestActionID FROM deleted)')";
                //query = "if not exists (select * from sys.objects where [type] = 'TR' and [name] = 'Actiondelete') exec('CREATE TRIGGER Actiondelete ON TestAction AFTER DELETE AS DELETE FROM controlAction where ActionID IN(SELECT deleted.TestActionID FROM deleted) DELETE FROM controlverification where ActionID IN(SELECT deleted.TestActionID FROM deleted) DELETE FROM TelnetAction where ActionID IN(SELECT deleted.TestActionID FROM deleted) DELETE FROM TelnetVerify where ActionID IN(SELECT deleted.TestActionID FROM deleted)  DELETE FROM Firmwareaction where ActionID IN(SELECT deleted.TestActionID FROM deleted) DELETE FROM APSettings where ActionID IN(SELECT deleted.TestActionID FROM deleted) DELETE FROM APVerification where ActionID IN(SELECT deleted.TestActionID FROM deleted) DELETE FROM LevelAndGain where ActionID IN(SELECT deleted.TestActionID FROM deleted) DELETE FROM APBenchModeInitialSettings where ActionID IN(SELECT deleted.TestActionID FROM deleted) DELETE FROM APLevelAndGainInitialSettings where ActionID IN(SELECT deleted.TestActionID FROM deleted) DELETE FROM APSeqModeInitialSettings where ActionID IN(SELECT deleted.TestActionID FROM deleted) DELETE FROM APFrequencyResponse where ActionID IN(SELECT deleted.TestActionID FROM deleted) DELETE FROM APFrequencyResponseInitialSettings where ActionID IN(SELECT deleted.TestActionID FROM deleted)')";
                adap = new SqlDataAdapter(query, this.QscDatabase.CreateConnection());
                adap.Fill(ds, "Test");

                query = "if exists (select * from sys.objects where [type] = 'TR' and [name] = 'Actiondelete') exec('Alter TRIGGER Actiondelete ON TestAction AFTER DELETE AS DELETE FROM controlAction where ActionID IN(SELECT deleted.TestActionID FROM deleted) DELETE FROM controlverification where ActionID IN(SELECT deleted.TestActionID FROM deleted) DELETE FROM TelnetAction where ActionID IN(SELECT deleted.TestActionID FROM deleted) DELETE FROM TelnetVerify where ActionID IN(SELECT deleted.TestActionID FROM deleted) DELETE FROM Firmwareaction where ActionID IN(SELECT deleted.TestActionID FROM deleted) DELETE FROM APSettings where ActionID IN(SELECT deleted.TestActionID FROM deleted) DELETE FROM APVerification where ActionID IN(SELECT deleted.TestActionID FROM deleted) DELETE FROM LevelAndGain where ActionID IN(SELECT deleted.TestActionID FROM deleted) DELETE FROM APBenchModeInitialSettings where ActionID IN(SELECT deleted.TestActionID FROM deleted) DELETE FROM APLevelAndGainInitialSettings where ActionID IN(SELECT deleted.TestActionID FROM deleted) DELETE FROM APSeqModeInitialSettings where ActionID IN(SELECT deleted.TestActionID FROM deleted) DELETE FROM APFrequencyResponse where ActionID IN(SELECT deleted.TestActionID FROM deleted) DELETE FROM APFrequencyResponseInitialSettings where ActionID IN(SELECT deleted.TestActionID FROM deleted) DELETE FROM APPhaseInitialSettings where ActionID IN(SELECT deleted.TestActionID FROM deleted) DELETE FROM APPhaseSettings where ActionID IN(SELECT deleted.TestActionID FROM deleted) DELETE FROM APSteppedFreqSweepInitialSettings where ActionID IN(SELECT deleted.TestActionID FROM deleted) DELETE FROM APSteppedFreqSweepSettings where ActionID IN(SELECT deleted.TestActionID FROM deleted) DELETE FROM APTHDNInitialSettings where ActionID IN(SELECT deleted.TestActionID FROM deleted) DELETE FROM APTHDNSettings where ActionID IN(SELECT deleted.TestActionID FROM deleted) DELETE FROM logVerification where ActionID IN(SELECT deleted.TestActionID FROM deleted) DELETE FROM NetpairingAction where ActionID IN(SELECT deleted.TestActionID FROM deleted) DELETE FROM Responsalyzer where ActionID IN(SELECT deleted.TestActionID FROM deleted) DELETE FROM DesignerAction where ActionID IN(SELECT deleted.TestActionID FROM deleted) DELETE FROM QRCMAction where ActionID IN(SELECT deleted.TestActionID FROM deleted) DELETE FROM QRCMVerification where ActionID IN(SELECT deleted.TestActionID FROM deleted) DELETE FROM CECAction where ActionID IN(SELECT deleted.TestActionID FROM deleted) DELETE FROM CECVerification where ActionID IN(SELECT deleted.TestActionID FROM deleted)DELETE FROM QRVerification where ActionID IN(SELECT deleted.TestActionID FROM deleted)')";
                adap = new SqlDataAdapter(query, this.QscDatabase.CreateConnection());
                adap.Fill(ds, "Test");

                query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'designtable') BEGIN Create table designtable(DesignID int identity(1,1) primary key,Designname nvarchar(max) not null) END";
                adap = new SqlDataAdapter(query, this.QscDatabase.CreateConnection());
                adap.Fill(ds, "Test");

                this.query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'designtable' AND COLUMN_NAME = 'TPID') BEGIN  ALTER TABLE [dbo].[designtable] ADD [TPID] int null END";
                adap = new SqlDataAdapter(this.query, this.QscDatabase.CreateConnection());
                adap.Fill(this.ds, "Test");

                query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'DesignInventory') BEGIN Create table DesignInventory(DesignID int null,FOREIGN KEY (DesignID) references designtable(DesignID) on delete cascade,  DeviceType nvarchar(MAX) not null, DeviceModel nvarchar(MAX) not null, DeviceNameInDesign  nvarchar(MAX) not null) END";
                adap = new SqlDataAdapter(query, this.QscDatabase.CreateConnection());
                adap.Fill(ds, "Test");

                query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'TPDesignLinkTable') BEGIN Create table TPDesignLinkTable(TPID int null,DesignID int null,FOREIGN KEY (TPID) references testplan(TestPlanID) on delete cascade,FOREIGN KEY (DesignID) references designtable(DesignID) on delete cascade) END";
                adap = new SqlDataAdapter(query, this.QscDatabase.CreateConnection());
                adap.Fill(ds, "Test");

                query = "if not exists (select * from sys.objects where [type] = 'TR' and [name] = 'designdelete') exec('CREATE TRIGGER designdelete ON TPDesignLinkTable AFTER DELETE AS DELETE FROM designtable where DesignID IN(SELECT deleted.DesignID FROM deleted)')";
                adap = new SqlDataAdapter(query, this.QscDatabase.CreateConnection());
                adap.Fill(ds, "Test");

                query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'TCInitialization') BEGIN Create table TCInitialization(DesignID int null,ComponentType nvarchar(MAX) null,ComponentName nvarchar(MAX) null,Control nvarchar(MAX) null,SpecControlID  nvarchar(MAX) null,Type nvarchar(MAX) null,MinValue nvarchar(MAX) null,MaxValue nvarchar(MAX) null,DefaultValue nvarchar(MAX) null,DefaultPosition nvarchar(MAX) null,DefaultString nvarchar(MAX) null, InitialValue nvarchar(MAX) null, InitialString nvarchar(MAX) null, InitialPosition nvarchar(MAX) null,FOREIGN KEY (DesignID) references designtable(DesignID) on delete cascade) END";
                adap = new SqlDataAdapter(query, this.QscDatabase.CreateConnection());
                adap.Fill(ds, "Test");

                this.query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'TCInitialization' AND COLUMN_NAME = 'PrettyName') BEGIN  ALTER TABLE [dbo].[TCInitialization] ADD [PrettyName] nvarchar(MAX) null END";
                adap = new SqlDataAdapter(this.query, this.QscDatabase.CreateConnection());
                adap.Fill(this.ds, "Test");

                this.query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'TCInitialization' AND COLUMN_NAME = 'Subtype') BEGIN  ALTER TABLE [dbo].[TCInitialization] ADD [Subtype] nvarchar(MAX) null END";
                adap = new SqlDataAdapter(this.query, this.QscDatabase.CreateConnection());
                adap.Fill(this.ds, "Test");

                this.query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'TCInitialization' AND COLUMN_NAME = 'ArraySize') BEGIN  ALTER TABLE [dbo].[TCInitialization] ADD [ArraySize] nvarchar(MAX) null END";
                adap = new SqlDataAdapter(this.query, this.QscDatabase.CreateConnection());
                adap.Fill(this.ds, "Test");

                this.query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'TCInitialization' AND COLUMN_NAME = 'ControlDirection') BEGIN  ALTER TABLE [dbo].[TCInitialization] ADD [ControlDirection] nvarchar(MAX) null END";
                adap = new SqlDataAdapter(this.query, this.QscDatabase.CreateConnection());
                adap.Fill(this.ds, "Test");

                this.query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'TCInitialization' AND COLUMN_NAME = 'ClassName') BEGIN  ALTER TABLE [dbo].[TCInitialization] ADD [ClassName] nvarchar(MAX) null END";
                adap = new SqlDataAdapter(this.query, this.QscDatabase.CreateConnection());
                adap.Fill(this.ds, "Test");

                this.query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'TCInitialization' AND COLUMN_NAME = 'ComponentPrettyName') BEGIN  ALTER TABLE [dbo].[TCInitialization] ADD [ComponentPrettyName] nvarchar(MAX) null END";
                adap = new SqlDataAdapter(this.query, this.QscDatabase.CreateConnection());
                adap.Fill(this.ds, "Test");

                this.query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'TCInitialization' AND COLUMN_NAME = 'NetworkName') BEGIN  ALTER TABLE [dbo].[TCInitialization] ADD [NetworkName] nvarchar(MAX) null END";
                adap = new SqlDataAdapter(this.query, this.QscDatabase.CreateConnection());
                adap.Fill(this.ds, "Test");

                query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Firmwareaction') BEGIN Create table Firmwareaction(FirmwareactionID int identity(1,1) primary key,TCID int null,ActionID int null,FirmwareActionType nvarchar(MAX) not null, FirmwareLocation nvarchar(MAX) not null, Firmwareupdatedate nvarchar(MAX) not null,FOREIGN KEY (TCID) references testcase(TestcaseID) on delete cascade) END";
                adap = new SqlDataAdapter(query, this.QscDatabase.CreateConnection());
                adap.Fill(ds, "Test");

                this.query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Firmwareaction' AND COLUMN_NAME = 'InstallationType') BEGIN  ALTER TABLE [dbo].[Firmwareaction] ADD [InstallationType] nvarchar(MAX) null END";
                adap = new SqlDataAdapter(this.query, this.QscDatabase.CreateConnection());
                adap.Fill(this.ds, "Test");

                this.query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Firmwareaction' AND COLUMN_NAME = 'FirmwareTime') BEGIN  ALTER TABLE [dbo].[Firmwareaction] ADD [FirmwareTime] nvarchar(MAX) null END";
                adap = new SqlDataAdapter(this.query, this.QscDatabase.CreateConnection());
                adap.Fill(this.ds, "Test");

                this.query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Firmwareaction' AND COLUMN_NAME = 'MeasureFirmUpgradeTime') BEGIN  ALTER TABLE [dbo].[Firmwareaction] ADD [MeasureFirmUpgradeTime] nvarchar(MAX) null END";
                adap = new SqlDataAdapter(this.query, this.QscDatabase.CreateConnection());
                adap.Fill(this.ds, "Test");

                query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'BackgroundMonitoring') BEGIN Create table BackgroundMonitoring(BackgroundMonitorID int identity(1,1) primary key,TSID int null,TPID int null,MonitorFrequency nvarchar(MAX) not null,MonitorDuration nvarchar(MAX) not null,MonitorDurationType nvarchar(MAX) not null,ErrorHandlingType nvarchar(MAX) not null,Rerunittration nvarchar(MAX) not null,RerunDuration nvarchar(MAX) not null,RerunDurationType nvarchar(MAX) not null,MonitorType nvarchar(MAX) not null) END";  //,FOREIGN KEY (TPID) references testplan(TestplanID)
                adap = new SqlDataAdapter(query, this.QscDatabase.CreateConnection());
                adap.Fill(ds, "Test");

                query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'InventoryMonitor') BEGIN Create table InventoryMonitor(InventoryMonitorID int identity(1,1) primary key,BMID int null,TSID int null,TPID int null,SelectedLogsType nvarchar(MAX) not null,FOREIGN KEY (BMID) references BackgroundMonitoring(BackgroundMonitorID) on delete cascade ) END";  //,FOREIGN KEY (TSID) references testsuite(TestsuiteID),FOREIGN KEY (TPID) references testplan(TestplanID)
                adap = new SqlDataAdapter(query, this.QscDatabase.CreateConnection());
                adap.Fill(ds, "Test");

                query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'LogMonitor1') BEGIN Create table LogMonitor1(LogMonitorID int identity(1,1)primary key,BMID int null,TSID int null,TPID int null,ilog nvarchar(MAX) not null,ilog_combobox nvarchar(MAX) not null,ilog_text nvarchar(MAX) not null,kernellog nvarchar(MAX) not null ,kernel_logcombobox nvarchar(MAX) not null,kernellog_text nvarchar(MAX) not null,eventlog nvarchar(MAX) not null ,eventlog_text nvarchar(MAX) not null,configuratorlog nvarchar(MAX) not null ,configuratorlog_text nvarchar(MAX) not null,siplog nvarchar(MAX) not null ,siplog_text nvarchar(MAX) not null,QSYSapplog nvarchar(MAX) not null ,QSYSapplog_text nvarchar(MAX) not null,UCIlog nvarchar(MAX) not null ,UCIlog_text nvarchar(MAX) not null,softphonelog nvarchar(MAX) not null ,softphonelog_text nvarchar(MAX) not null,Windows_eventlog nvarchar(MAX) not null ,Windows_eventlog_text nvarchar(MAX) not null,FOREIGN KEY (BMID) references BackgroundMonitoring(BackgroundMonitorID) on delete cascade ) END";
                //query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'APVerification') BEGIN Create table APVerification(APVerificationID int identity(1,1) primary key,TCID int null,ActionID int null,VerificationType nvarchar(MAX) not null,FOREIGN KEY (TCID) references testcase(TestcaseID) on delete cascade) END";
                adap = new SqlDataAdapter(query, this.QscDatabase.CreateConnection());
                adap.Fill(ds, "Test");
                
                query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'TelenetMonitor') BEGIN Create table TelenetMonitor(TelenetMonitorID int identity(1,1) primary key,BMID int null,TSID int null,TPID int null,Telenetcommand nvarchar(MAX) not null,Devicesname nvarchar(MAX) not null,VerifyType nvarchar(MAX) not null,Comparetext nvarchar(MAX) not null,FOREIGN KEY (BMID) references BackgroundMonitoring(BackgroundMonitorID) on delete cascade ) END";  //,FOREIGN KEY (TSID) references testsuite(TestsuiteID),FOREIGN KEY (TPID) references testplan(TestplanID)
                adap = new SqlDataAdapter(query, this.QscDatabase.CreateConnection());
                adap.Fill(ds, "Test");

                this.query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'TelenetMonitor' AND COLUMN_NAME = 'TelenetCheckedStatus') BEGIN  ALTER TABLE [dbo].[TelenetMonitor] ADD [TelenetCheckedStatus] nvarchar(MAX) null END";
                adap = new SqlDataAdapter(this.query, this.QscDatabase.CreateConnection());
                adap.Fill(this.ds, "Test");

                this.query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'TelenetMonitor' AND COLUMN_NAME = 'DeviceModel') BEGIN  ALTER TABLE [dbo].[TelenetMonitor] ADD [DeviceModel] nvarchar(MAX) null END";
                adap = new SqlDataAdapter(this.query, this.QscDatabase.CreateConnection());
                adap.Fill(this.ds, "Test");

                query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'TelenetMonitor' AND COLUMN_NAME = 'keywordType') BEGIN  ALTER TABLE [dbo].[TelenetMonitor] ADD [keywordType] nvarchar(MAX) null END";
                adap = new SqlDataAdapter(query, this.QscDatabase.CreateConnection());
                adap.Fill(ds, "Test");

                query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'ControlMonitor') BEGIN Create table ControlMonitor(ControlMonitorID int identity(1,1) primary key,BMID int null,TSID int null,TPID int null,ComponentType nvarchar(MAX) not null,ComponentName nvarchar(MAX) not null,ComponentProperty nvarchar(MAX) not null,ComponentChannel nvarchar(MAX) not null,ComponentValue nvarchar(MAX) not null,FOREIGN KEY (BMID) references BackgroundMonitoring(BackgroundMonitorID) on delete cascade ) END";  //,FOREIGN KEY (TSID) references testsuite(TestsuiteID),FOREIGN KEY (TPID) references testplan(TestplanID)
                adap = new SqlDataAdapter(query, this.QscDatabase.CreateConnection());
                adap.Fill(ds, "Test");

                this.query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ControlMonitor' AND COLUMN_NAME = 'PrettyName') BEGIN  ALTER TABLE [dbo].[ControlMonitor] ADD [PrettyName] nvarchar(MAX) null END";
                adap = new SqlDataAdapter(this.query, this.QscDatabase.CreateConnection());
                adap.Fill(this.ds, "Test");

                this.query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ControlMonitor' AND COLUMN_NAME = 'ControlMonitorCheckedStatus') BEGIN  ALTER TABLE [dbo].[ControlMonitor] ADD [ControlMonitorCheckedStatus] nvarchar(MAX) null END";
                adap = new SqlDataAdapter(this.query, this.QscDatabase.CreateConnection());
                adap.Fill(this.ds, "Test");

                this.query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ControlMonitor' AND COLUMN_NAME = 'ValueType') BEGIN  ALTER TABLE [dbo].[ControlMonitor] ADD [ValueType] nvarchar(MAX) null END";
                adap = new SqlDataAdapter(this.query, this.QscDatabase.CreateConnection());
                adap.Fill(this.ds, "Test");

                this.query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ControlMonitor' AND COLUMN_NAME = 'MaximumLimit') BEGIN  ALTER TABLE [dbo].[ControlMonitor] ADD [MaximumLimit] nvarchar(MAX) null END";
                adap = new SqlDataAdapter(this.query, this.QscDatabase.CreateConnection());
                adap.Fill(this.ds, "Test");

                this.query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ControlMonitor' AND COLUMN_NAME = 'MinimumLimit') BEGIN  ALTER TABLE [dbo].[ControlMonitor] ADD [MinimumLimit] nvarchar(MAX) null END";
                adap = new SqlDataAdapter(this.query, this.QscDatabase.CreateConnection());
                adap.Fill(this.ds, "Test");

                this.query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ControlMonitor' AND COLUMN_NAME = 'LoopCheckedStatus') BEGIN  ALTER TABLE [dbo].[ControlMonitor] ADD [LoopCheckedStatus] nvarchar(MAX) null END";
                adap = new SqlDataAdapter(this.query, this.QscDatabase.CreateConnection());
                adap.Fill(this.ds, "Test");

                this.query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ControlMonitor' AND COLUMN_NAME = 'Loop_start') BEGIN  ALTER TABLE [dbo].[ControlMonitor] ADD [Loop_start] nvarchar(MAX) null END";
                adap = new SqlDataAdapter(this.query, this.QscDatabase.CreateConnection());
                adap.Fill(this.ds, "Test");

                this.query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ControlMonitor' AND COLUMN_NAME = 'Loop_End') BEGIN  ALTER TABLE [dbo].[ControlMonitor] ADD [Loop_End] nvarchar(MAX) null END";
                adap = new SqlDataAdapter(this.query, this.QscDatabase.CreateConnection());
                adap.Fill(this.ds, "Test");

                this.query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ControlMonitor' AND COLUMN_NAME = 'Loop_Increament') BEGIN  ALTER TABLE [dbo].[ControlMonitor] ADD [Loop_Increament] nvarchar(MAX) null END";
                adap = new SqlDataAdapter(this.query, this.QscDatabase.CreateConnection());
                adap.Fill(this.ds, "Test");

                this.query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ControlMonitor' AND COLUMN_NAME = 'AllChannels') BEGIN  ALTER TABLE [dbo].[ControlMonitor] ADD [AllChannels] nvarchar(MAX) null END";
                adap = new SqlDataAdapter(this.query, this.QscDatabase.CreateConnection());
                adap.Fill(this.ds, "Test");

                this.query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ControlMonitor' AND COLUMN_NAME = 'ControlDatatype') BEGIN  ALTER TABLE [dbo].[ControlMonitor] ADD [ControlDatatype] nvarchar(MAX) null END";
                adap = new SqlDataAdapter(this.query, this.QscDatabase.CreateConnection());
                adap.Fill(this.ds, "Test");

                query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'TPMonitorLinkTable') BEGIN Create table TPMonitorLinkTable(TPID int null,BMID int null,FOREIGN KEY (TPID) references testplan(TestPlanID) on delete cascade,FOREIGN KEY (BMID) references BackgroundMonitoring(BackgroundMonitorID) on delete cascade) END";
                adap = new SqlDataAdapter(query, this.QscDatabase.CreateConnection());
                adap.Fill(ds, "Test");

                query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'TSMonitorLinkTable') BEGIN Create table TSMonitorLinkTable(TSID int null,BMID int null,FOREIGN KEY (TSID) references testsuite(TestSuiteID) on delete cascade,FOREIGN KEY (BMID) references BackgroundMonitoring(BackgroundMonitorID) on delete cascade) END";
                adap = new SqlDataAdapter(query, this.QscDatabase.CreateConnection());
                adap.Fill(ds, "Test");

                query = "if not exists (select * from sys.objects where [type] = 'TR' and [name] = 'TPBMIDdelete') exec('CREATE TRIGGER TPBMIDdelete ON TPMonitorLinkTable AFTER DELETE AS DELETE FROM BackgroundMonitoring where BackgroundMonitorID IN(SELECT deleted.BMID FROM deleted)')";
                adap = new SqlDataAdapter(query, this.QscDatabase.CreateConnection());
                adap.Fill(ds, "Test");

                query = "if not exists (select * from sys.objects where [type] = 'TR' and [name] = 'TSBMIDdelete') exec('CREATE TRIGGER TSBMIDdelete ON TSMonitorLinkTable AFTER DELETE AS DELETE FROM BackgroundMonitoring where BackgroundMonitorID IN(SELECT deleted.BMID FROM deleted)')";
                adap = new SqlDataAdapter(query, this.QscDatabase.CreateConnection());
                adap.Fill(ds, "Test");

                query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Netpairlist') BEGIN Create table Netpairlist(NetPair_deviceslist nvarchar(MAX) not null) END";  //,FOREIGN KEY (TSID) references testsuite(TestsuiteID),FOREIGN KEY (TPID) references testplan(TestplanID)
                adap = new SqlDataAdapter(query, this.QscDatabase.CreateConnection());
                adap.Fill(ds, "Test");

                query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'logVerification') BEGIN Create table logVerification(logVerificationID int identity(1,1)primary key,TCID int null,ActionID int null,ilog nvarchar(MAX) not null,ilog_combobox nvarchar(MAX) not null,ilog_text nvarchar(MAX) not null,kernellog nvarchar(MAX) not null ,kernel_logcombobox nvarchar(MAX) not null,kernellog_text nvarchar(MAX) not null,eventlog nvarchar(MAX) not null ,eventlog_text nvarchar(MAX) not null,configuratorlog nvarchar(MAX) not null ,configuratorlog_text nvarchar(MAX) not null,siplog nvarchar(MAX) not null ,siplog_text nvarchar(MAX) not null,QSYSapplog nvarchar(MAX) not null ,QSYSapplog_text nvarchar(MAX) not null,UCIlog nvarchar(MAX) not null ,UCIlog_text nvarchar(MAX) not null,softphonelog nvarchar(MAX) not null ,softphonelog_text nvarchar(MAX) not null,Windows_eventlog nvarchar(MAX) not null ,Windows_eventlog_text nvarchar(MAX) not null,FOREIGN KEY (TCID) references testcase(TestcaseID) on delete cascade) END";
                //query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'APVerification') BEGIN Create table APVerification(APVerificationID int identity(1,1) primary key,TCID int null,ActionID int null,VerificationType nvarchar(MAX) not null,FOREIGN KEY (TCID) references testcase(TestcaseID) on delete cascade) END";
                adap = new SqlDataAdapter(query, this.QscDatabase.CreateConnection());
                adap.Fill(ds, "Test");

                query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'PcapVerification') BEGIN Create table PcapVerification(pcapVerificationID int identity(1,1)primary key,TCID int null,ActionID int null,Pcap_CaptureTime nvarchar(MAX) not null,Pcap_CaptureUnit nvarchar(MAX) not null,Pcap_ProtocolName nvarchar(MAX) not null, Pcap_FieldText nvarchar(MAX) not null,FOREIGN KEY (ActionID) references TestAction(TestActionID) on delete cascade) END";
                adap = new SqlDataAdapter(query, this.QscDatabase.CreateConnection());
                adap.Fill(ds, "Test");
				
                query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'UsbAction') BEGIN Create table UsbAction(UsbActionID int identity(1,1) primary key,TCID int null,ActionID int null,BridgeName nvarchar(MAX) not null,AudioType nvarchar(MAX) not null,ComponentPrettyName nvarchar(MAX) not null,FOREIGN KEY (TCID) references testcase(TestcaseID) on delete cascade) END";
                adap = new SqlDataAdapter(query, this.QscDatabase.CreateConnection());
                adap.Fill(ds, "Test");

                this.query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'UsbAction' AND COLUMN_NAME = 'DefaultOption') BEGIN  ALTER TABLE [dbo].[UsbAction] ADD [DefaultOption] nvarchar(MAX) null END";
                adap = new SqlDataAdapter(this.query, this.QscDatabase.CreateConnection());
                adap.Fill(this.ds, "Test");

                query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'UsbVerify') BEGIN Create table UsbVerify(UsbVerifyID int identity(1,1) primary key,TCID int null,ActionID int null,BridgeName nvarchar(MAX) not null,AudioType nvarchar(MAX) not null,ComponentPrettyName nvarchar(MAX) not null,FOREIGN KEY (TCID) references testcase(TestcaseID) on delete cascade) END";
                adap = new SqlDataAdapter(query, this.QscDatabase.CreateConnection());
                adap.Fill(ds, "Test");

                this.query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'UsbVerify' AND COLUMN_NAME = 'DefaultOption') BEGIN  ALTER TABLE [dbo].[UsbVerify] ADD [DefaultOption] nvarchar(MAX) null END";
                adap = new SqlDataAdapter(this.query, this.QscDatabase.CreateConnection());
                adap.Fill(this.ds, "Test");

                query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'NetpairingAction') BEGIN Create table NetpairingAction(NetPairActionID int identity(1,1) primary key,TCID int null,ActionID int null,DeviceType nvarchar(MAX) not null,DeviceNameInDesign nvarchar(MAX) not null,NetPairing nvarchar(MAX) not null) END";  //,FOREIGN KEY (TSID) references testsuite(TestsuiteID),FOREIGN KEY (TPID) references testplan(TestplanID)
                adap = new SqlDataAdapter(query, this.QscDatabase.CreateConnection());
                adap.Fill(ds, "Test");

                 query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'DesignerAction') BEGIN Create table DesignerAction(DesignerActionID int identity(1,1) primary key,TCID int null,ActionID int null,ConnectDesigner nvarchar(MAX) not null,DisconnectDesigner nvarchar(MAX) not null,EmulateDesigner nvarchar(MAX) not null) END";  //,FOREIGN KEY (TSID) references testsuite(TestsuiteID),FOREIGN KEY (TPID) references testplan(TestplanID)
                adap = new SqlDataAdapter(query, this.QscDatabase.CreateConnection());
                adap.Fill(ds, "Test");
                  query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Responsalyzer') BEGIN Create table Responsalyzer(ResponsalyzerID int identity(1,1) primary key,TCID int null,ActionID int null,ResponsalyzerName nvarchar(MAX) not null,GraphType nvarchar(MAX) not null,VerificationFileLocation nvarchar(MAX) not null,FOREIGN KEY (TCID) references testcase(TestcaseID) on delete cascade) END";
                adap = new SqlDataAdapter(query, this.QscDatabase.CreateConnection());
                adap.Fill(ds, "Test");

                this.query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'DesignInventory' AND COLUMN_NAME = 'PrimaryorBackup') BEGIN  ALTER TABLE [dbo].[DesignInventory] ADD PrimaryorBackup nvarchar(MAX) null END";
                adap = new SqlDataAdapter(this.query, this.QscDatabase.CreateConnection());
                adap.Fill(this.ds, "Test");

                this.query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'DesignInventory' AND COLUMN_NAME = 'Backup_to_primary') BEGIN  ALTER TABLE [dbo].[DesignInventory] ADD Backup_to_primary nvarchar(MAX) null END";
                adap = new SqlDataAdapter(this.query, this.QscDatabase.CreateConnection());
                adap.Fill(this.ds, "Test");

                this.query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'DesignerAction' AND COLUMN_NAME = 'newdesign') BEGIN  ALTER TABLE [dbo].[DesignerAction] ADD newdesign nvarchar(MAX) null END";
                adap = new SqlDataAdapter(this.query, this.QscDatabase.CreateConnection());
                adap.Fill(this.ds, "Test");

                this.query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'DesignerAction' AND COLUMN_NAME = 'DeployMonitoring') BEGIN  ALTER TABLE [dbo].[DesignerAction] ADD DeployMonitoring nvarchar(MAX) null,NumberOfTimesDeploy nvarchar(MAX) null END";
                adap = new SqlDataAdapter(this.query, this.QscDatabase.CreateConnection());
                adap.Fill(this.ds, "Test");

                this.query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'DesignerAction' AND COLUMN_NAME = 'Timeout') BEGIN  ALTER TABLE [dbo].[DesignerAction] ADD Timeout nvarchar(MAX) null, TimeoutUnit nvarchar(MAX) null END";
                adap = new SqlDataAdapter(this.query, this.QscDatabase.CreateConnection());
                adap.Fill(this.ds, "Test");

                this.query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'DesignerAction' AND COLUMN_NAME = 'Loadfromcore') BEGIN  ALTER TABLE [dbo].[DesignerAction] ADD Loadfromcore nvarchar(MAX) null END";
                adap = new SqlDataAdapter(this.query, this.QscDatabase.CreateConnection());
                adap.Fill(this.ds, "Test");
				
                query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'QRCMInitialization') BEGIN Create table QRCMInitialization(Project_Name nvarchar(MAX) not null,Build_version nvarchar(MAX) not null,Reference_Version nvarchar(MAX) null,Method_Name nvarchar(MAX) null,Actual_method_name nvarchar(MAX) null,Input_arguments_Tooltip nvarchar(MAX) null, Api_reference nvarchar(MAX) null, HasPreMethod nvarchar(MAX) null, PreMethodName nvarchar(MAX) null, PreMethodUserKey nvarchar(MAX) null,PreMethodActualKey nvarchar(MAX) null,ArgumentMappingIndex nvarchar(MAX) null, CoreModel nvarchar(MAX) null, IsActionTrue nvarchar(MAX) null,TabGroupName nvarchar(MAX) null, Api_Payload nvarchar(MAX) null,IsPayloadAvailable nvarchar(MAX) null,Reference_key nvarchar(MAX) null,Payload_key nvarchar(MAX) null,Merge_data nvarchar(MAX) null) END";
                adap = new SqlDataAdapter(query, this.QscDatabase.CreateConnection());
                adap.Fill(ds, "Test");

                query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'QRCMAction') BEGIN Create table QRCMAction(QRCMActionID int identity(1,1) primary key,TCID int null,ActionID int null,Project_Name nvarchar(MAX) not null,Build_version nvarchar(MAX) not null,Reference_Version nvarchar(MAX) not null, Device_name nvarchar(MAX) not null,Device_model nvarchar(MAX) not null,Method_name nvarchar(MAX) not null, Actual_method_name nvarchar(MAX) not null, Input_arguments nvarchar(MAX) not null,Input_arguments_Tooltip nvarchar(MAX) not null,ReferenceFilePath nvarchar(MAX) not null,PayloadFilePath nvarchar(MAX) not null,HasPreMethod nvarchar(MAX) not null, PreMethodName nvarchar(MAX) not null,PreMethodUserKey nvarchar(MAX) not null,PreMethodActualKey nvarchar(MAX) not null,ArgumentMappingIndex int null,IsPayloadAvailable nvarchar(MAX) not null,Reference_key nvarchar(MAX) not null, Payload_key nvarchar(MAX) not null,Merge_data nvarchar(MAX) not null, FOREIGN KEY (TCID) references testcase(TestcaseID) on delete cascade) END";
                adap = new SqlDataAdapter(query, this.QscDatabase.CreateConnection());
                adap.Fill(ds, "Test");

                query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'QRCMVerification') BEGIN Create table QRCMVerification(QRCMVerificationID int identity(1,1) primary key,TCID int null,ActionID int null,Project_Name nvarchar(MAX) not null,Build_version nvarchar(MAX) not null,Reference_Version nvarchar(MAX) not null, Device_name nvarchar(MAX) not null,Device_model nvarchar(MAX) not null,Method_name nvarchar(MAX) not null,Actual_method_name nvarchar(MAX) not null,Input_arguments nvarchar(MAX) not null,Input_arguments_Tooltip nvarchar(MAX) not null,ReferenceFilePath nvarchar(MAX) not null,PayloadFilePath nvarchar(MAX) not null,HasPreMethod nvarchar(MAX) not null,PreMethodName nvarchar(MAX) not null, PreMethodUserKey nvarchar(MAX) not null,PreMethodActualKey nvarchar(MAX) not null,ArgumentMappingIndex int null,IsPayloadAvailable nvarchar(MAX) not null,Reference_key nvarchar(MAX) not null,Payload_key nvarchar(MAX) not null,  FOREIGN KEY (TCID) references testcase(TestcaseID) on delete cascade) END";
                adap = new SqlDataAdapter(query, this.QscDatabase.CreateConnection());
                adap.Fill(ds, "Test");

                //this.query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'DesignerAction' AND COLUMN_NAME = 'NumberOfTimesDeploy') BEGIN  ALTER TABLE [dbo].[DesignerAction] ADD NumberOfTimesDeploy nvarchar(MAX) null END";
                //adap = new SqlDataAdapter(this.query, this.QscDatabase.CreateConnection());
                //adap.Fill(this.ds, "Test");

                query = "ALTER TABLE NetpairingAction  WITH NOCHECK Add FOREIGN KEY (TCID) references testcase(TestcaseID) on delete cascade";  //,FOREIGN KEY (TSID) references testsuite(TestsuiteID),FOREIGN KEY (TPID) references testplan(TestplanID)
                adap = new SqlDataAdapter(query, this.QscDatabase.CreateConnection());
                adap.Fill(ds, "Test");
				
                query = "ALTER TABLE DesignerAction WITH NOCHECK Add FOREIGN KEY (TCID) references testcase(TestcaseID) on delete cascade";  //,FOREIGN KEY (TSID) references testsuite(TestsuiteID),FOREIGN KEY (TPID) references testplan(TestplanID)
                adap = new SqlDataAdapter(query, this.QscDatabase.CreateConnection());
                adap.Fill(ds, "Test");

                

                this.QscDatabase.CloseConnection();
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif

                //MessageBox.Show("Exception\n " + ex.Message, "QAT Error Code - EC01004", MessageBoxButton.OK, MessageBoxImage.Error);

                this.QscDatabase.CloseConnection();
            }
        }

        public void Create_dbversion()
        {
            try
            {
                query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'DBversion') BEGIN Create table DBversion(version nvarchar(MAX) not null) END";
                SqlDataAdapter adap = new SqlDataAdapter(query, this.QscDatabase.CreateConnection());
                this.QscDatabase.OpenConnection();
                adap.Fill(ds, "Test");
                this.QscDatabase.CloseConnection();


            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif

                //MessageBox.Show("Exception\n " + ex.Message, "QAT Error Code - EC01005", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        public void Create_LinkingTables()
        {
            try
            {
                query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'TSTPLinkTable') BEGIN Create table TSTPLinkTable(TSID int null,TPID int null,FOREIGN KEY (TSID) references testsuite(TestSuiteID)  on delete cascade,FOREIGN KEY (TPID) references testplan(TestPlanID) on delete cascade) END";
                SqlDataAdapter adap = new SqlDataAdapter(query, this.QscDatabase.CreateConnection());
                this.QscDatabase.OpenConnection();
                adap.Fill(ds, "Test");

                query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'TSTPLinkTable' AND COLUMN_NAME = 'TSTPID') BEGIN  ALTER TABLE [dbo].[TSTPLinkTable] ADD [TSTPID] int identity(1,1) primary key END";
                adap = new SqlDataAdapter(query, this.QscDatabase.CreateConnection());
                adap.Fill(ds, "Test");

                query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'TPTCLinkTable') BEGIN Create table TPTCLinkTable(TPID int null,TCID int null,FOREIGN KEY (TPID) references testplan(TestPlanID) on delete cascade,FOREIGN KEY (TCID) references testcase(TestcaseID) on delete cascade ) END";
                adap = new SqlDataAdapter(query, this.QscDatabase.CreateConnection());
                adap.Fill(ds, "Test");

                query = "IF Not Exists (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'TPTCLinkTable' AND COLUMN_NAME = 'TPTCID') BEGIN  ALTER TABLE [dbo].[TPTCLinkTable] ADD [TPTCID] int identity(1,1) primary key END";
                adap = new SqlDataAdapter(query, this.QscDatabase.CreateConnection());
                adap.Fill(ds, "Test");
                                
                this.QscDatabase.CloseConnection();
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif

                //MessageBox.Show("Exception\n " + ex.Message, "QAT Error Code - EC01005", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}