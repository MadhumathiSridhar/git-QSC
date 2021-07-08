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
using System.ComponentModel;

namespace QSC_Test_Automation
{
    /// <summary>
    /// Interaction logic for MessageBox.xaml
    /// </summary>
    public partial class QatMessageBox : Window, INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler PropertyChanged;
        private MessageBoxResult returnMessageBoxResult;

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
                //MessageBox.Show("Exception\n " + ex.Message, "Error Code - ECxxxxx", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private string messageBodyValue = null;
        public string MessageBody
        {
            get
            {
                return messageBodyValue;
            }
            set
            {
                messageBodyValue = value;
                OnPropertyChanged("MessageBody");
            }
        }

        private string messageTitleValue = null;
        public string MessageTitle
        {
            get
            {
                return messageTitleValue;
            }
            set
            {
                messageTitleValue = value;
                OnPropertyChanged("MessageTitle");
            }
        }

        private string messageImageValue = null;
        public string MessageImage
        {
            get
            {
                return messageImageValue;
            }
            set
            {
                messageImageValue = value;
                OnPropertyChanged("MessageImage");
            }
        }

        private MessageBoxButton messageBottonValue = MessageBoxButton.OK;
        public MessageBoxButton MessageButton
        {
            get
            {
                return messageBottonValue;
            }
            set
            {
                messageBottonValue = value;
                OnPropertyChanged("MessageButton");
            }
        }

        private bool msgAuto = false;
        public bool MsgAuto
        {
            get
            {
                return msgAuto;
            }
            set
            {
                msgAuto = value;
                OnPropertyChanged("MsgAuto");
            }
        }

        public QatMessageBox(Window owner,bool IsTaskShedulerOn=false)
        {
            InitializeComponent();
            Owner = owner;
            this.DataContext = this;
            MsgAuto = IsTaskShedulerOn;
        }

        public MessageBoxResult Show(string messageBody, string messageTitle, MessageBoxButton messageBoxButton, MessageBoxImage messageBoxImage)
        {
            InitializeComponent();

            if(MsgAuto && (messageBoxButton==MessageBoxButton.OK || messageBoxButton==MessageBoxButton.OKCancel))
            { 
                return MessageBoxResult.OK;
            }
            if (MsgAuto && (messageBoxButton == MessageBoxButton.YesNo || messageBoxButton == MessageBoxButton.YesNoCancel))
            {
                return MessageBoxResult.Yes;
            }

            try
            {
                this.Dispatcher.Invoke(() =>
                {
                    MessageBody = messageBody;
                    MessageTitle = messageTitle;
                    MessageButton = messageBoxButton;

                if (messageBoxImage == MessageBoxImage.Error)
                    MessageImage = "/Images/Failed.png";
                else if (messageBoxImage == MessageBoxImage.Error)
                    MessageImage = "/Images/Incomplete.png";
                else if (messageBoxImage == MessageBoxImage.Warning)
                    MessageImage = "/Images/Incomplete.png";
                else if (messageBoxImage == MessageBoxImage.Information)
                    MessageImage = "/Images/pass.png";



                    if (MessageButton == MessageBoxButton.OK)
                    {
                        Button1.Content = "OK";
                        Button1.Margin = new Thickness(0, 0, 0, 2);
                        Button2.Visibility = Visibility.Collapsed;
                        Button3.Visibility = Visibility.Collapsed;                      
                    }
                    else if (MessageButton == MessageBoxButton.OKCancel)
                    {
                        if (messageTitle == "User action Alert")
                        {
                            Button1.Content = "Done";
                            Button2.Content = "Cancel";                          
                        }
                        else if (messageTitle == "User verification Alert")
                        {
                            Button1.Content = "Pass";
                            Button2.Content = "Fail";
                            remarks_label.Visibility = Visibility.Visible;
                            remarks_txtBx.Visibility = Visibility.Visible;
                        }
                        else
                        {
                            Button1.Content = "OK";
                            Button2.Content = "Cancel";                           
                        }
                        
                        Button1.Margin = new Thickness(0, 0, 60, 2);                      
                        Button2.Margin = new Thickness(60, 0, 0, 2);
                        Button3.Visibility = Visibility.Collapsed;                       
                    }
                    else if (MessageButton == MessageBoxButton.YesNo)
                    {
                        Button1.Content = "Yes";
                        Button1.Margin = new Thickness(0, 0, 60, 2);
                        Button2.Content = "No";
                        Button2.Margin = new Thickness(60, 0, 0, 2);
                        Button3.Visibility = Visibility.Collapsed;                      
                    }
                    else if (MessageButton == MessageBoxButton.YesNoCancel)
                    {
                        Button1.Content = "Yes";
                        Button1.Margin = new Thickness(0, 0, 120, 2);
                        Button2.Content = "No";
                        Button2.Margin = new Thickness(0, 0, 0, 2);
                        Button3.Content = "Cancel";
                        Button3.Margin = new Thickness(120, 0, 0, 2);                   
                    }

                    ShowDialog();
                });
                return returnMessageBoxResult;
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                //MessageBox.Show("Exception\n " + ex.Message, "QAT Error Code - EC03021", MessageBoxButton.OK, MessageBoxImage.Error);

                return returnMessageBoxResult;
            }
        }


        private string userVerifyremarksTextValue = string.Empty;
        public string UserVerifyremarksText
        {
            get
            {
                return userVerifyremarksTextValue;
            }
            set
            {
                userVerifyremarksTextValue = value;
                OnPropertyChanged("UserVerifyremarksText");
            }
        }


        private void Button1_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (MessageButton == MessageBoxButton.OK || MessageButton == MessageBoxButton.OKCancel)
                {
                    returnMessageBoxResult = MessageBoxResult.OK;
                }
                else if (MessageButton == MessageBoxButton.YesNo || MessageButton == MessageBoxButton.YesNoCancel)
                {
                    returnMessageBoxResult = MessageBoxResult.Yes;
                }

                Close();
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

        private void Button2_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (MessageButton == MessageBoxButton.OKCancel)
                {
                    returnMessageBoxResult = MessageBoxResult.Cancel;
                }
                else if (MessageButton == MessageBoxButton.YesNo || MessageButton == MessageBoxButton.YesNoCancel)
                {
                    returnMessageBoxResult = MessageBoxResult.No;
                }

                Close();
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

        private void Button3_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (MessageButton == MessageBoxButton.YesNoCancel)
                {
                    returnMessageBoxResult = MessageBoxResult.Cancel;
                }

                Close();
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
    }
}
