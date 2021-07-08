namespace QSC_Test_Automation
{
    using System;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    /// <summary>
    /// Interaction logic for TabRename.xaml
    /// </summary>
    public partial class TabRename : Window
    {
        public TabRename()
        {
            this.InitializeComponent();
        }

        private void BtnOK_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.DialogResult = true;
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                //MessageBox.Show("Exception\n " + ex.Message, "QAT Error Code - EC11001", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void TextBox_PreviewKeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            var textBox = e.OriginalSource as TextBox;

            if (textBox != null && e.Key == Key.Tab)
            {
                textBox.SelectAll();
            }
        }
    }
}
