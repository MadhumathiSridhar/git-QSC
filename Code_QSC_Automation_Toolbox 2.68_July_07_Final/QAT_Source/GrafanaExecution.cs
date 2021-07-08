using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace QSC_Test_Automation
{
    class GrafanaExecution
    {
        GrafanaDBConnection grafanaDBConnection = new GrafanaDBConnection();

        public string SaveDataPoints(int execID, string testsuitename, string testplanname, string testcasename, string scriptStartTime, string testactionname, string scriptType, string ReleaseVersion, string DesignName, string scriptPath)
        {
            string remarks = string.Empty;

            try
            {
                string query = "Insert into DataPointsMappingTable (ExecID,Testsuitename,Testplanname,Testcasename,Testactionname,ScriptType,ScriptStartTime,ReleaseVersion,DesignName,TagName) values('" + execID + "','" + testsuitename + "','" + testplanname + "','" + testcasename + "','" + testactionname + "','" + scriptType + "','" + scriptStartTime + "','" + ReleaseVersion + "','" + DesignName + "','" + string.Empty + "');SELECT CONVERT(int,SCOPE_IDENTITY())";
                var issuccess = grafanaDBConnection.SendCommand_Toreceive(query);
                if (issuccess.Item1)
                {
                    DataTable tbl = issuccess.Item2;
                    if (tbl != null && tbl.Rows.Count > 0 && tbl.Rows[0][0] != null && tbl.Rows[0][0].ToString() != string.Empty)
                    {
                        int scriptkey = Convert.ToInt32(tbl.Rows[0][0]);
                        int i = 1;
                        string lines = string.Empty;
                        string datastartTime = string.Empty;
                        string dataendTime = string.Empty;

                        using (StreamReader read = new StreamReader(scriptPath))
                        {
                            while ((lines = read.ReadLine()) != null)
                            {
                                if (!string.IsNullOrEmpty(lines) && !lines.StartsWith("Start Time:") && !lines.StartsWith("End Time:") && !lines.StartsWith("Unit:"))
                                {
                                    string[] failedPointSplit = lines.Split(':');

                                    if (failedPointSplit.Count() > 0 && failedPointSplit.Last() != null && failedPointSplit.Last() != string.Empty)
                                    {
                                        string originalVal = failedPointSplit.Last().Trim();
                                        string query1 = "Insert into DataPointsTable (ScriptMappingID,Iteration,ScriptDatapoint) values('" + scriptkey + "','" + i + "','" + originalVal + "')";
                                        var isSucces = grafanaDBConnection.SendCommand_Toreceive(query1);

                                        if (!issuccess.Item1)
                                        {
                                            remarks = "Error occured while inserting datapoints";
                                        }
                                    }

                                    i++;
                                }
                                else if (lines.StartsWith("Start Time:"))
                                {
                                    string[] startTimeSplit = Regex.Split(lines, "Start Time:");
                                    if (lines.StartsWith("Start Time") && startTimeSplit.Count() > 1 && startTimeSplit[1] != null && startTimeSplit[1] != string.Empty)
                                        datastartTime = startTimeSplit[1];
                                }
                                else if (lines.StartsWith("End Time:"))
                                {
                                    string[] EndTimeSplit = Regex.Split(lines, "End Time:");
                                    if (lines.StartsWith("End Time") && EndTimeSplit.Count() > 1 && EndTimeSplit[1] != null && EndTimeSplit[1] != string.Empty)
                                        dataendTime = EndTimeSplit[1]; 
                                }
                            }
                        }

                        string[] avgMinMax =  ReadAvgMinMaxValueFromDatapoints(scriptkey);
                        if (string.IsNullOrEmpty(avgMinMax[3]))
                        {
                            string isSuccessupdate = UpdateAvgMinMaxStartEndTime(scriptkey, datastartTime, dataendTime, avgMinMax[0], avgMinMax[1], avgMinMax[2]);
                            if (!string.IsNullOrEmpty(isSuccessupdate))
                                remarks = isSuccessupdate;
                        }
                        else
                        {
                            remarks = avgMinMax[3];
                        }
                    }
                }
                else
                {
                    remarks = "Error occured while inserting datapoints";
                }
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                        MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                remarks = "Exception occured while Grafana DB write " + scriptType;
            }

            return remarks;
        }

        public string[] ReadAvgMinMaxValueFromDatapoints(int scriptMappingID)
        {
            string remarks = string.Empty;
            string avgValue = string.Empty;
            string minValue = string.Empty;
            string MaxValue = string.Empty;

            try
            {
                string query = "Select Avg(ScriptDatapoint), Min(ScriptDatapoint), Max(ScriptDatapoint) from DataPointsTable where ScriptMappingID =" + scriptMappingID;
                var issuccess = grafanaDBConnection.SendCommand_Toreceive(query);
                if (issuccess.Item1)
                {
                    DataTable tbl = issuccess.Item2;
                    if (tbl != null && tbl.Rows.Count > 0 && tbl.Rows[0][0] != null && tbl.Rows[0][0].ToString() != string.Empty)
                    {
                        avgValue = tbl.Rows[0][0].ToString();
                        minValue = tbl.Rows[0][1].ToString();
                        MaxValue = tbl.Rows[0][2].ToString();
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

            return new string[] { avgValue, minValue, MaxValue, remarks };
        }

        public string UpdateAvgMinMaxStartEndTime(int scriptMappingID, string starttime, string endTime, string avgVal, string minVal, string MaxVal)
        {
            string remarks = string.Empty;

            try
            {
                string query = "Update DataPointsMappingTable set DataStartTime ='" + starttime + "',DataEndTime = '" + endTime +"',Average = '" + avgVal + "', Minimum='" + minVal + "', Maximum='" + MaxVal + "' where ScriptMappingID =" + scriptMappingID;
                var issuccess = grafanaDBConnection.SendCommand_Toreceive(query);
                if (!issuccess.Item1)
                {
                    remarks = "Error occured while update starttime, Endtime, Average, Minimum and Maximum values";
                }
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                        MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                remarks = "Error occured while update starttime, Endtime, Average, Minimum and Maximum values";
            }

            return remarks;
        }
    }
}
