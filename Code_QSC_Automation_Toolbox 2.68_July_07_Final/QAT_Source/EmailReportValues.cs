using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;

namespace QSC_Test_Automation
{
    public class EmailReportValues : INotifyPropertyChanged
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
                MessageBox.Show("Exception\n " + ex.Message, "Error Code - ECExxxxx", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool chk_EmailAfterXFailValue = false;
        public bool chk_EmailAfterXFail
        {
            get { return chk_EmailAfterXFailValue; }
            set
            {
                chk_EmailAfterXFailValue = value;
                if (chk_EmailAfterXFail == true)
                    FailureValueEnable = true;               
                else
                    FailureValueEnable = false;
                    FailureValue = 0;
                OnPropertyChanged("chk_EmailAfterXFail");
            }
        }


        private bool Chk_EmailIfAutomationPausesValue = false;
        public bool Chk_EmailIfAutomationPauses
        {
            get { return Chk_EmailIfAutomationPausesValue; }
            set
            {
                Chk_EmailIfAutomationPausesValue = value;
                OnPropertyChanged("Chk_EmailIfAutomationPauses");
            }
        }

        private bool Chk_EmailAfterCompletionOfExecutionValue = false;
        public bool Chk_EmailAfterCompletionOfExecution
        {
            get { return Chk_EmailAfterCompletionOfExecutionValue; }
            set
            {
                Chk_EmailAfterCompletionOfExecutionValue = value;
                OnPropertyChanged("Chk_EmailAfterCompletionOfExecution");
            }
        }
        
        private bool Chk_EmailIfQSysDesignerApplicationCrashesValue = false;
        public bool Chk_EmailIfQSysDesignerApplicationCrashes
        {
            get { return Chk_EmailIfQSysDesignerApplicationCrashesValue; }
            set
            {
                Chk_EmailIfQSysDesignerApplicationCrashesValue = value;
                OnPropertyChanged("Chk_EmailIfQSysDesignerApplicationCrashes");
            }
        }

        private string MailToValue = string.Empty;
        public string MailTo
        {
            get { return MailToValue; }
            set
            {
                MailToValue = value;
                if (MailToValue != string.Empty)
                {
                    if (MailToValue.Contains("@") && MailToValue.Contains(".com"))
                        ApplyEnable = true;
                    else
                        ApplyEnable = false;
                }
                else
                    ApplyEnable = false;
                    OnPropertyChanged("MailTo");
            }
        }

        private int FailureValueValue = 0;
        public int FailureValue
        {
            get { return FailureValueValue; }
            set
            {
                FailureValueValue = value;
                OnPropertyChanged("FailureValue");
            }
        }

        private bool FailureValueEnableValue = false;
        public bool FailureValueEnable
        {
            get { return FailureValueEnableValue; }
            set
            {
                FailureValueEnableValue = value;
                OnPropertyChanged("FailureValueEnable");
            }
        }

        private bool ApplyEnableValue = false;
        public bool ApplyEnable
        {
            get { return ApplyEnableValue; }
            set
            {
                ApplyEnableValue = value;
                OnPropertyChanged("ApplyEnable");
            }
        }


        private bool Get_Email_ID()
        {

            string pattern = null;

            if (MailTo.Length > 0)
            {
                pattern = "^([0-9a-zA-Z]([-\\.\\w]*[0-9a-zA-Z])*@([0-9a-zA-Z][-\\w]*[0-9a-zA-Z]\\.)+[a-zA-Z]{2,9})$";

                if (Regex.IsMatch(MailTo.Trim(), pattern))
                {
                   // Properties.Settings.Default.EmailID = MailTo.Trim();
                    return true;
                }
                else
                {
                    MessageBox.Show("Please enter Valid email address", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }
            }

            //Properties.Settings.Default.EmailID = MailTo.Trim();
            return false;
        }
    }
}
