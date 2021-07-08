using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace QSC_Test_Automation
{
    class Responsalyzer
    {
        //System.Net.Sockets.TcpClient clientSocket = new System.Net.Sockets.TcpClient();
        const int port = 1702;
        ReportDBConnection report_connection = new ReportDBConnection();
        System.Data.DataTable tble = new System.Data.DataTable();

        public Tuple<string,string> ExecuteResponsalyzer(string responsalyzerName, string graphSelected, string coreIP,string FileLocation,int actionID, int exid, string testSuiteName, string testPlanName, string testCaseName, string testActionName, Int32 CaseExecutionUniqueID, Int32 ActionTabCaseAlogPlanExecutionUniqueID, Int32 SuiteExecutionUniqueID)
        {
            try
            {
                string starttime = DateTime.Now.ToString();
                string remarksForFailure = string.Empty;
                string expectedCoordinates = "Not Applicable";
                string measuredCoordinates = "Not Applicable";
                string result = "Fail";
                string XYplotsValue = string.Empty;
                bool status = false;
               
                Double[,] actualGraphValues = new Double[0, 0];
                Double[,] refLowerGraphValues = new Double[0, 0];
                Double[,] refUpperGraphValues = new Double[0, 0];


                var actualGraphPlots = getActualXaxisVsYaxisPlots(responsalyzerName, graphSelected, coreIP);
                if(actualGraphPlots.Item1.Trim()!=string.Empty)
                {
                    XYplotsValue = actualGraphPlots.Item1.Trim();
                    actualGraphValues = ReturnXYReferenceValue(actualGraphPlots.Item1.Trim(), actualGraphValues);
                }
                else
                {
                    remarksForFailure = actualGraphPlots.Item2.Trim();

                   string query = "Insert into TempTestCaseActionTabTable values('" + exid + "','Fail',@TSName, @TPName, @TCName,@TAName,'" + starttime + "','" + DateTime.Now.ToString() + "','" + string.Empty + "','Responsalyzer Verification:" + (actionID+1) +"', '" + remarksForFailure + "','Not Applicable','Not Applicable','" + CaseExecutionUniqueID + "','" + ActionTabCaseAlogPlanExecutionUniqueID + "','" + SuiteExecutionUniqueID + "','')";
                    tble = report_connection.Report_SendCommand_Toreceive(query, "@TSName", testSuiteName, "@TPName", testPlanName, "@TCName", testCaseName, "@TAName", testActionName, string.Empty, string.Empty, string.Empty, string.Empty);

                    return new Tuple<string,string>(result, XYplotsValue);
                }
               

                var referenceGraphPlots = getReferenceXaxisVsYaxisPlots(FileLocation);
                if(referenceGraphPlots!=null)
                {
                    refLowerGraphValues = referenceGraphPlots.Item1;
                    refUpperGraphValues = referenceGraphPlots.Item2;
                    if(referenceGraphPlots.Item3.Trim()!=string.Empty)
                    {
                        remarksForFailure = referenceGraphPlots.Item3.Trim();
                        string query = "Insert into TempTestCaseActionTabTable values('" + exid + "','Fail',@TSName, @TPName, @TCName,@TAName,'" + starttime + "','" + DateTime.Now.ToString() + "','" + string.Empty + "','Responsalyzer Verification:" + (actionID + 1) + "', '" + remarksForFailure + "','Not Applicable','Not Applicable','" + CaseExecutionUniqueID + "','" + ActionTabCaseAlogPlanExecutionUniqueID + "','" + SuiteExecutionUniqueID + "','')";
                        tble = report_connection.Report_SendCommand_Toreceive(query, "@TSName", testSuiteName, "@TPName", testPlanName, "@TCName", testCaseName, "@TAName", testActionName, string.Empty, string.Empty, string.Empty, string.Empty);
                        return new Tuple<string, string>(result, XYplotsValue);
                    }
                    
                }
                if ((actualGraphValues.Length >0) & (refUpperGraphValues.Length >0) & (refLowerGraphValues.Length > 0))
                {
                    starttime = DateTime.Now.ToString();
                    var finalResults = VerifyResults(actualGraphValues, refLowerGraphValues, refUpperGraphValues);
                    status = finalResults.Item1;
                    if(!finalResults.Item1)
                    {
                        remarksForFailure = finalResults.Item2.Trim();
                        expectedCoordinates = finalResults.Item3.Trim();
                        measuredCoordinates = finalResults.Item4.Trim();
                    }
                   
                }
                if (status)
                {
                    string query = "Insert into TempTestCaseActionTabTable values('" + exid + "','Pass',@TSName, @TPName, @TCName,@TAName,'" + starttime + "','" + DateTime.Now.ToString() + "','" + string.Empty + "','Responsalyzer Verification :" + (actionID + 1) + "', '" + string.Empty + "','Not Applicable','Not Applicable','" + CaseExecutionUniqueID + "','" + ActionTabCaseAlogPlanExecutionUniqueID + "','" + SuiteExecutionUniqueID + "','')";
                    tble = report_connection.Report_SendCommand_Toreceive(query, "@TSName", testSuiteName, "@TPName", testPlanName, "@TCName", testCaseName, "@TAName", testActionName, string.Empty, string.Empty, string.Empty, string.Empty);
                    result = "Pass";

                }
                else
                {
                    string query = "Insert into TempTestCaseActionTabTable values('" + exid + "','Fail',@TSName, @TPName, @TCName,@TAName,'" + starttime + "','" + DateTime.Now.ToString() + "','" + string.Empty + "','Responsalyzer Verification :" + (actionID + 1) + "', '" + remarksForFailure + "','" + expectedCoordinates + "','" + measuredCoordinates + "','" + CaseExecutionUniqueID + "','" + ActionTabCaseAlogPlanExecutionUniqueID + "','" + SuiteExecutionUniqueID + "','')";
                    tble = report_connection.Report_SendCommand_Toreceive(query, "@TSName", testSuiteName, "@TPName", testPlanName, "@TCName", testCaseName, "@TAName", testActionName, string.Empty, string.Empty, string.Empty, string.Empty);
                }


                return new Tuple<string, string>(result, XYplotsValue);
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif


                string query = "Insert into TempTestCaseActionTabTable values('" + exid + "','Fail',@TSName, @TPName, @TCName,@TAName,'" + DateTime.Now.ToString() + "','" + DateTime.Now.ToString() + "','" + string.Empty + "','Responsalyzer Verification :" + (actionID + 1) + "', 'Error occured during executing Responsalyzer action','Not Applicable','Not Applicable','" + CaseExecutionUniqueID + "','" + ActionTabCaseAlogPlanExecutionUniqueID + "','" + SuiteExecutionUniqueID + "','')";
                tble = report_connection.Report_SendCommand_Toreceive(query, "@TSName", testSuiteName, "@TPName", testPlanName, "@TCName", testCaseName, "@TAName", testActionName, string.Empty, string.Empty, string.Empty, string.Empty);
                return new Tuple<string, string>("Fail", "No response received from core");
            }
            
        }

        private Tuple<string, string> getActualXaxisVsYaxisPlots(string responsalyzerName, string graphSelected, string coreIP)
        {
            string remarksForFailure = string.Empty;
            try
            {        
                List<string> xaxisVsyaxis = new List<string>();
                string xaxisCommand = string.Empty;
                string yaxisCommand = string.Empty;
                string xaxisReturnData = string.Empty;
                string yaxisReturnData = string.Empty;
                string xaxisVsyaxisPlots = string.Empty;
                byte[] xaxisOutStream;
                byte[] yaxisOutStream;
                byte[] xaxisInStream = new byte[1000768];
                byte[] yaxisInStream = new byte[1000768];

                bool isconnect = false;

                if ((string.Equals(graphSelected, "Frequency Vs Magnitude", StringComparison.CurrentCultureIgnoreCase))&(responsalyzerName!=string.Empty))
                {
                    xaxisCommand = responsalyzerName + "frequency";
                    yaxisCommand = responsalyzerName + "magnitude";
                }
                if ((string.Equals(graphSelected, "Frequency Vs Phase", StringComparison.CurrentCultureIgnoreCase)) & (responsalyzerName != string.Empty))
                {
                    xaxisCommand = responsalyzerName + "frequency";
                    yaxisCommand = responsalyzerName + "phase";
                }

                using (System.Net.Sockets.TcpClient clientSocket = new System.Net.Sockets.TcpClient())
                {
                    isconnect = isClientConnected(clientSocket);
                    if (!isconnect)
                    {
                        if (!clientSocket.Connected)
                            clientSocket.Connect(coreIP, port);
                    }


                    NetworkStream serverStream = clientSocket.GetStream();
                    if (xaxisCommand != string.Empty & yaxisCommand != string.Empty)
                    {
                        xaxisOutStream = System.Text.Encoding.ASCII.GetBytes("cg \"" + xaxisCommand + "\"\n");
                        serverStream.Write(xaxisOutStream, 0, xaxisOutStream.Length);
                        serverStream.Read(xaxisInStream, 0, (int)clientSocket.ReceiveBufferSize);
                        xaxisReturnData = System.Text.Encoding.ASCII.GetString(xaxisInStream);

                        yaxisOutStream = System.Text.Encoding.ASCII.GetBytes("cg \"" + yaxisCommand + "\"\n");
                        serverStream.Write(yaxisOutStream, 0, yaxisOutStream.Length);
                        serverStream.Read(yaxisInStream, 0, (int)clientSocket.ReceiveBufferSize);
                        yaxisReturnData = System.Text.Encoding.ASCII.GetString(yaxisInStream);


                        xaxisVsyaxis = returnxandyArray(xaxisReturnData, yaxisReturnData);
                        if (xaxisVsyaxis != null)
                        {
                            xaxisVsyaxisPlots = string.Join(",", xaxisVsyaxis);
                        }
                    }
                }

                    
                return new Tuple<string, string>(xaxisVsyaxisPlots, remarksForFailure);
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif

                if (ex.Message.Contains("No connection could be made because the target machine actively refused it"))
                {
                    remarksForFailure = "No connection could be made because Core is actively refused it.";
                }
                else
                remarksForFailure = "Core is not responding";
                return new Tuple<string, string>(string.Empty, remarksForFailure);
            }

        }

        private Tuple<Double[,], Double[,],string> getReferenceXaxisVsYaxisPlots(string FileLocation)
        {
            Double[,] refLowerVal = new Double[0, 0];
            Double[,] refUpperVal = new Double[0, 0];
            string remarksForFailure =string.Empty;
            try
            {
                string xaxisVsyaxisPlots = string.Empty;
                List<string> lines = System.IO.File.ReadAllLines(FileLocation).Select(line => string.Join("", line.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries))).Where(arg => !string.IsNullOrWhiteSpace(arg)).ToList();
                if(lines.Count > 0)
                {
                   

                    foreach (string line in lines)
                    {
                        if (line.Contains("Lower"))
                        {
                            refLowerVal = new Double[0, 0];
                            refLowerVal = ReturnXYReferenceValue(line, refLowerVal);
                        }

                        if (line.Contains("Upper"))
                        {
                            refUpperVal = new Double[0, 0];
                            refUpperVal = ReturnXYReferenceValue(line, refLowerVal);
                        }
                    }
                }
                return new Tuple<Double[,], Double[,],string>(refLowerVal, refUpperVal, remarksForFailure);
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif

                remarksForFailure = "Error occurs during retrieving values from reference file";
                return new Tuple<Double[,], Double[,], string>(refLowerVal, refUpperVal, remarksForFailure);
            }
            
        }

        private List<string> returnxandyArray(string xaxisResponse, string yaxisResponse)
        {
            try
            {
                List<string> xaxisVsyaxis = new List<string>();
                string[] initialXaxisArray;
                string[] finalXaxisArray;
                string[] initialYaxisArray;
                string[] finalYaxisArray;

                if ((xaxisResponse != string.Empty) & (yaxisResponse != string.Empty))
                {
                    initialXaxisArray = xaxisResponse.Split(' ');
                    Int32 xaxisArrayCount = Convert.ToInt32(initialXaxisArray[3]);
                    finalXaxisArray = new string[xaxisArrayCount];
                    Array.Copy(initialXaxisArray, 4, finalXaxisArray, 0, xaxisArrayCount);

                    initialYaxisArray = yaxisResponse.Split(' ');
                    Int32 yaxisArrayCount = Convert.ToInt32(initialYaxisArray[3]);
                    finalYaxisArray = new string[yaxisArrayCount];
                    Array.Copy(initialYaxisArray, 4, finalYaxisArray, 0, yaxisArrayCount);

                    if (finalXaxisArray.Length == finalYaxisArray.Length)
                    {
                        for (Int32 index = 0; index < finalXaxisArray.Length; index++)
                        {
                            xaxisVsyaxis.Add("(" + finalXaxisArray[index] + "," + finalYaxisArray[index] + ")");
                        }

                    }
                    return xaxisVsyaxis;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif

                return null;
            }
        }

        private bool isClientConnected(TcpClient clientSocket)
        {
            try
            {
                IPGlobalProperties ipProperties = IPGlobalProperties.GetIPGlobalProperties();

                TcpConnectionInformation[] tcpConnections = ipProperties.GetActiveTcpConnections();

                foreach (TcpConnectionInformation c in tcpConnections)
                {
                    TcpState stateOfConnection = c.State;

                    if (c.LocalEndPoint.Equals(clientSocket.Client.LocalEndPoint) && c.RemoteEndPoint.Equals(clientSocket.Client.RemoteEndPoint))
                    {
                        if (stateOfConnection == TcpState.Established)
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }

                    }

                }

                return false;

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

        private Tuple<bool,string,string,string> VerifyResults(Double[,] actValue, Double[,] refLowerValue, Double[,] refUpperValue)
        {
            string remarksForFailure = string.Empty;
            string expectedCoordinates = "Not Applicable";
            string measuredCoordinates = "Not Applicable";
            try
            {              
                Double[,] actualList = actValue;
                Double[,] referenceLowerList = refLowerValue;
                Double[,] referenceUpperList = refUpperValue;

                Double[,] expectedLowerList = CalculateExpectedValues(actualList, referenceLowerList);
                Double[,] expectedUpperList = CalculateExpectedValues(actualList, referenceUpperList);

                bool compareLowerResult = true;
                bool compareUpperResult = true;
                Double[,] compareFaildActualValue = new Double[1, 2];
                Double[,] compareFaildExpectedValue = new Double[1, 2];

                for (int i = 0; i < actualList.Length / 2; i++)
                {
                    if (actualList[i, 1] < expectedLowerList[i, 1])
                    {
                        compareLowerResult = false;

                        compareFaildActualValue[0, 0] = actualList[i, 0];
                        compareFaildActualValue[0, 1] = actualList[i, 1];

                        compareFaildExpectedValue[0, 0] = expectedLowerList[i, 0];
                        compareFaildExpectedValue[0, 1] = expectedLowerList[i, 1];
                        break;
                    }

                    if (actualList[i, 1] > expectedUpperList[i, 1])
                    {
                        compareUpperResult = false;

                        compareFaildActualValue[0, 0] = actualList[i, 0];
                        compareFaildActualValue[0, 1] = actualList[i, 1];

                        compareFaildExpectedValue[0, 0] = expectedUpperList[i, 0];
                        compareFaildExpectedValue[0, 1] = expectedUpperList[i, 1];
                        break;
                    }
                }

                if (!compareLowerResult)
                {
                    Console.Write("Failed Lower Limit. Actual Value: {0}, {1}. Expected Value: {2}, {3}.", compareFaildActualValue[0, 0], compareFaildActualValue[0, 1], compareFaildExpectedValue[0, 0], compareFaildExpectedValue[0, 1]);
                    remarksForFailure= "Failed due to comparing Actual Value with Lower Limit";
                    expectedCoordinates = "X= " + compareFaildExpectedValue[0, 0] + ",Y= " + compareFaildExpectedValue[0, 1];
                    measuredCoordinates = "X= " + compareFaildActualValue[0, 0] + ",Y= " + compareFaildActualValue[0, 1] ;
                    return new Tuple<bool, string,string,string> (false, remarksForFailure, expectedCoordinates, measuredCoordinates);
                }
                if (!compareUpperResult)
                {
                    Console.Write("Failed Upper Limit. Actual Value: {0}, {1}. Expected Value: {2}, {3}.", compareFaildActualValue[0, 0], compareFaildActualValue[0, 1], compareFaildExpectedValue[0, 0], compareFaildExpectedValue[0, 1]);
                    remarksForFailure = "Failed due to comparing Actual Value with Upper Limit";
                    expectedCoordinates = "X= " + compareFaildExpectedValue[0, 0] + ",Y= " + compareFaildExpectedValue[0, 1];
                    measuredCoordinates = "X= " + compareFaildActualValue[0, 0] + ",Y= " + compareFaildActualValue[0, 1] ;
                    return new Tuple<bool, string, string, string>(false, remarksForFailure, expectedCoordinates, measuredCoordinates);
                }

                return new Tuple<bool, string, string, string>(true, remarksForFailure, expectedCoordinates, measuredCoordinates);
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif

                remarksForFailure = "Error Occurs While Comparing actual and reference values.";
                return new Tuple<bool, string, string, string>(false, remarksForFailure, expectedCoordinates, measuredCoordinates);
            }
        }

        private Double LinearInterploation(Double start_x, Double start_y, Double end_x, Double end_y, Double input_x)
        {

            Double input_y = 0.0;
            try
            {
                input_y = start_y + (end_y - start_y) * (input_x - start_x) / (end_x - start_x);
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif

            }

            return input_y;
        }

        private double[,] CalculateExpectedValues(double[,] actual, double[,] reference)
        {
            Double[,] expectedValues = new double[actual.Length / 2, 2];
            try
            {
                if (reference.Length / 2 < 2)
                    return expectedValues;

                for (int i = 0; i < actual.Length / 2; i++)
                {
                    Double start_x = reference[0, 0];
                    Double end_x = reference[0, 1];
                    Double start_y = reference[1, 0];
                    Double end_y = reference[1, 1];

                    for (int j = 1; j < reference.Length / 2; j++)
                    {
                        start_x = reference[j - 1, 0];
                        start_y = reference[j - 1, 1];
                        end_x = reference[j, 0];
                        end_y = reference[j, 1];
                        if (actual[i, 0] < reference[j, 0])
                            break;
                    }

                    expectedValues[i, 0] = actual[i, 0];
                    expectedValues[i, 1] = LinearInterploation(start_x, start_y, end_x, end_y, actual[i, 0]);
                }
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif

            }
            return expectedValues;
        }

        private Double[,] ReturnXYReferenceValue(string line, Double[,] refereneceVal)
        {
            try
            {
                Regex regex = new Regex(@"\(([^)]*)\)");
                MatchCollection matches = regex.Matches(line);

                refereneceVal = new Double[matches.Count, 2];
                for (int j = 0; j < matches.Count; j++)
                {
                    string replaceVal = matches[j].Value.Replace("(", string.Empty).Replace(")", string.Empty);
                    string[] splitVal = replaceVal.Split(',');

                    double xAxisLowerVal = 0;
                    if (splitVal != null && splitVal.Count() > 0 && splitVal[0] != null && splitVal[0] != string.Empty && double.TryParse(splitVal[0], out xAxisLowerVal))
                        refereneceVal[j, 0] = xAxisLowerVal;

                    double yAxisLowerVal = 0;
                    if (splitVal != null && splitVal.Count() > 1 && splitVal[1] != null && splitVal[1] != string.Empty && double.TryParse(splitVal[1], out yAxisLowerVal))
                        refereneceVal[j, 1] = yAxisLowerVal;
                }

                return refereneceVal;
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif

                return refereneceVal;
            }
        }
    }
}
