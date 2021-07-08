namespace QSC_Test_Automation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;
    using System.Windows.Documents;
    using System.Windows.Input;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using System.Windows.Shapes;

    /// <summary>
    /// Interaction logic for ExecutionDelay_Window.xaml
    /// </summary>
    public partial class ExecutionDelay : Window
    {
        public ExecutionDelay(TreeViewExplorer treeViewExplorerExecutionRootItem)
        {
            this.InitializeComponent();
            this.DataContext = treeViewExplorerExecutionRootItem;
        }
     
        private void Btn_Cancel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.Close();
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                //MessageBox.Show("Exception\n " + ex.Message, "QAT Error Code - EC05001", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Btn_Ok_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.Close();
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                //MessageBox.Show("Exception\n " + ex.Message, "QAT Error Code - EC05002", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Txt_Delay_PreviewTextInput(object sender, TextCompositionEventArgs e)
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
                //MessageBox.Show("Exception\n " + ex.Message, "QAT Error Code - EC05003", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private static bool IsTextAllowed(string text)
        {
            Regex regex = null;
            try
            {
                regex = new Regex("[^0-9-]+"); ////regex that matches disallowed text
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
#if DEBUG
                MessageBox.Show("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message, "QAT Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                //MessageBox.Show("Exception\n " + ex.Message, "QAT Error Code - EC05004", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return !regex.IsMatch(text);
        }

        private void txt_Delay_PreviewKeyDown(object sender, KeyEventArgs e)
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
                //for Copy and Paste
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
                //MessageBox.Show("Exception\n " + ex.Message, "QAT Error Code - EC05005", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btn_delay_keyup(object sender, KeyEventArgs e)
        {
            Window target = (Window)sender;
            if (e.Key == Key.Escape)
            {
                target.Close();
            }
            e.Handled = true;
        }

        private void TextBox_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            var textBox = e.OriginalSource as TextBox;

            if (textBox != null && e.Key == Key.Tab)
            {
                textBox.SelectAll();
            }
        }
    }
}
