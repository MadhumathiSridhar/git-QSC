using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace QSC_Test_Automation
{
    /// <summary>
    /// Interaction logic for QAToverview.xaml
    /// </summary>
    public partial class QAToverview : Window
    {
        public QAToverview()
        {
            InitializeComponent();
            
            About.Text = "                                     The QSC Automation Tool (QAT) is designed to perform automated testing for all QSC devices through LAN. The Tool consists of QAT - Designer window to create Test suites(TS), Test plans(TP) and Test cases(TC).User can view, modify, search, sort and delete the TS / TP / TC in QAT - Designer.The QAT - Runner executes the Test suites and execution results can be viewed in QAT - Reports.The tool communicates with Q - SYS Designer software to perform execution.User has the option to pause, resume, abort and set delay for execution  in QAT - Runner.Loop option allows the user to run specific TS for a specific number of times or for a specific duration.User can Drag and Drop option to select the required TS for execution from LHS and drop at  RHS.QAT reports is web based and can be viewed in any standard browser.QAT - Reports maintains the records of execution which can be viewed day wise, weekly or monthly.Email reports option will send out a mail to the  previously mentioned e - mail ID once the Execution is completed.Completion of execution is indicated to the user by way of an execution summary window containing the results of execution.DUT configuration discovers the details of devices connected to the QSC device.The status of devices found is indicated through different colors.User can configure the server  path, QSC device credentials, Q - SYS Designer version etc in Preferences window.The tool requires MYSQL software for establishing database.QAT can handle multiple instances  of execution simultaneously";
           
        }
    }
}
