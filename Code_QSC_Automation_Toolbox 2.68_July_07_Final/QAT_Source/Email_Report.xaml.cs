namespace QSC_Test_Automation
{
    //using Microsoft.Office.Interop.Excel;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;
    using System.Windows.Documents;
    using System.Windows.Input;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using System.Windows.Shapes;
    using System.Text.RegularExpressions;
    using System.Net.Mail;
    using System.Net;
    using System.Data;
    using System.Reflection;
    
    /// <summary>
    /// Interaction logic for Email_Report.xaml
    /// </summary>
    public partial class Email_Report: System.Windows.Window
    {     
        public Email_Report()
        {
            this.InitializeComponent();
            try
            {
                chk_EmailAfterXFail.IsChecked = Properties.Settings.Default.AfterTestCaseFails;
                FailureValue1.Text = Properties.Settings.Default.TestCaseFailsCount.ToString();
                Chk_EmailIfAutomationPauses.IsChecked = Properties.Settings.Default.AutomationPauses;
                Chk_EmailAfterCompletionOfExecution.IsChecked = Properties.Settings.Default.AfterExecutionCompletion;
                Chk_EmailIfQSysDesignerApplicationCrashes.IsChecked = Properties.Settings.Default.ApplicationCrashes;
                AddReportLinkInmail.IsChecked = Properties.Settings.Default.AddreportLink;
                MailTo.Text = Properties.Settings.Default.ReportEmailID;
                EmailSubject.Text = Properties.Settings.Default.ReportEmailSubject;
                //if (MailTo.Text != string.Empty && MailTo.Text.Contains("@") && MailTo.Text.Contains(".com"))
                //    Email_Apply_button.IsEnabled = true;
                //else
                //    Email_Apply_button.IsEnabled = false;

                if (Properties.Settings.Default.AfterTestCaseFails == false)
                {
                    FailureValue1.IsEnabled = false;
                }
                else
                {
                    FailureValue1.IsEnabled = true;
                }
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                //MessageBox.Show("Exception\n " + ex.Message, "QAT Error Code - EC03021", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        private void Email_Apply_button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (MailTo.Text.Trim() != string.Empty)
                {
                    if (!Get_Email_ID(MailTo.Text.Trim()))
                    {
                        MessageBox.Show("In-Valid email address found", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                }

                int tcCnt = 0;
                bool isChk = Int32.TryParse(FailureValue1.Text, out tcCnt);

                if (chk_EmailAfterXFail.IsChecked.Value && (FailureValue1.Text == string.Empty || !isChk))
                {
                    MessageBox.Show("Enter valid failure count", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (chk_EmailAfterXFail.IsChecked.Value || Chk_EmailIfAutomationPauses.IsChecked.Value || Chk_EmailAfterCompletionOfExecution.IsChecked.Value || Chk_EmailIfQSysDesignerApplicationCrashes.IsChecked.Value || AddReportLinkInmail.IsChecked.Value)
                {
                    if (MailTo.Text.Trim() == string.Empty)
                    {
                        MessageBox.Show("Enter email id", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                }

                Properties.Settings.Default.AfterTestCaseFails = chk_EmailAfterXFail.IsChecked.Value;
                Properties.Settings.Default.TestCaseFailsCount = tcCnt;
                Properties.Settings.Default.AutomationPauses = Chk_EmailIfAutomationPauses.IsChecked.Value;
                Properties.Settings.Default.AfterExecutionCompletion = Chk_EmailAfterCompletionOfExecution.IsChecked.Value;
                Properties.Settings.Default.ApplicationCrashes = Chk_EmailIfQSysDesignerApplicationCrashes.IsChecked.Value;
                Properties.Settings.Default.AddreportLink = AddReportLinkInmail.IsChecked.Value;
                Properties.Settings.Default.ReportEmailID = MailTo.Text;
                Properties.Settings.Default.ReportEmailSubject = EmailSubject.Text;
                this.Close();


                //Properties.Settings.Default.AfterTestCaseFails = chk_EmailAfterXFail.IsChecked.Value;
                //Properties.Settings.Default.TestCaseFailsCount = Convert.ToInt32(FailureValue1.Text);
                //Properties.Settings.Default.AutomationPauses = Chk_EmailIfAutomationPauses.IsChecked.Value;
                //Properties.Settings.Default.AfterExecutionCompletion = Chk_EmailAfterCompletionOfExecution.IsChecked.Value;
                //Properties.Settings.Default.ApplicationCrashes = Chk_EmailIfQSysDesignerApplicationCrashes.IsChecked.Value;

                //if (Properties.Settings.Default.AfterTestCaseFails || Properties.Settings.Default.AutomationPauses || Properties.Settings.Default.AfterExecutionCompletion || Properties.Settings.Default.ApplicationCrashes)
                //{
                //    if (MailTo.Text.Trim() != string.Empty)
                //    {
                //        if (Get_Email_ID(MailTo.Text.Trim()))
                //        {
                //            Properties.Settings.Default.ReportEmailID = MailTo.Text;
                //            this.Close();
                //        }
                //        else
                //        {
                //            MessageBox.Show("Enter valid email id");
                //        }
                //    }
                //    else
                //    {
                //        MessageBox.Show("Enter email id");
                //    }
                //}                
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                //MessageBox.Show("Exception\n " + ex.Message, "QAT Error Code - EC04001", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }      

        private void textBox1_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Handled == IsTextAllowed(e.ToString()))
                {
                    if (e.Key == Key.Space)
                    {
                        e.Handled = true;
                    }

                    base.OnPreviewKeyDown(e);
                }

                string lstrCopyandPasteTxtBox = null;
                if (e.Key == Key.V && Keyboard.Modifiers == ModifierKeys.Control)
                {
                    e.Handled = false;
                    lstrCopyandPasteTxtBox = Clipboard.GetText();
                    if (e.Handled == IsTextAllowed(lstrCopyandPasteTxtBox))
                    {
                        e.Handled = true;
                    }
                    base.OnPreviewKeyDown(e);
                }

            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                //MessageBox.Show("Exception\n " + ex.Message, "QAT Error Code - EC04002", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void textBox1_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            try
            {
                e.Handled = !IsTextAllowed(e.Text);
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                //MessageBox.Show("Exception\n " + ex.Message, "QAT Error Code - EC04003", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private static bool IsTextAllowed(string text)
        {
            Regex regex = null;

            try
            {
                regex = new Regex("[^0-9]+"); ////regex that matches disallowed text
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                //MessageBox.Show("Exception\n " + ex.Message, "QAT Error Code - EC04004", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return !regex.IsMatch(text);
        }

        private bool Get_Email_ID(string MailTo)
        {
            bool check = false;
           List<string> splitemailid = new List<string>();
            try
            {
                if (MailTo.Contains(";"))
                    splitemailid.AddRange(MailTo.Split(';'));
                else
                    splitemailid.Add(MailTo);

                splitemailid.RemoveAll(x => x == string.Empty);

                if (splitemailid.Count > 0)
                {
                    foreach (string email in splitemailid)
                    {
                        if (email.Trim() != string.Empty)
                        {
                            var addr = new System.Net.Mail.MailAddress(email.Trim());
                            if (addr.Address != email.Trim())
                            {
                                //MessageBox.Show("In-Valid email address found", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                                return check;
                            }
                        }
                    }
                }
                else
                {
                    return false;
                }

                check=true;
                return check;
            }
            catch (Exception ex)
            {
                //MessageBox.Show("In-Valid email address found", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                //MessageBox.Show("Exception\n " + ex.Message, "QAT Error Code - EC03021", MessageBoxButton.OK, MessageBoxImage.Error);
                return check;
            }
        }


        private void chk_EmailAfterXFail_Checked(object sender, RoutedEventArgs e)
        {
            FailureValue1.IsEnabled = true;
        }

        private void chk_EmailAfterXFail_Unchecked(object sender, RoutedEventArgs e)
        {
            FailureValue1.IsEnabled = false;
        }

        //private void MailTo_TextChanged(object sender, TextChangedEventArgs e)
        //{
        //    if (MailTo.Text.Contains("@") && MailTo.Text.Contains(".com"))
        //    {
        //        Email_Apply_button.IsEnabled = true;
        //    }
        //    else
        //    {
        //        Email_Apply_button.IsEnabled = false;
        //    }

        //    Properties.Settings.Default.ReportEmailID = MailTo.Text;
        //}

        private void FailureValue1_TextChanged(object sender, TextChangedEventArgs e)
        {
            
        }
    }
}
