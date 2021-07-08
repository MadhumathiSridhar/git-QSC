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

namespace QSC_Test_Automation
{
    /// <summary>
    /// Interaction logic for TestCase_Creation_Window.xaml
    /// </summary>
    public partial class TestCase_Creation_Window : Window
    {
        private List<TabItem> tabItems;
        private TabItem tabAdd;
        private ComboBox cmb_ComponentType;
        private ComboBox cmb_ComponentName;

        public TestCase_Creation_Window()
        {
            InitializeComponent();
            tabItems = new List<TabItem>();
            tabAdd = new TabItem();
            tabAdd.Header = "+";
            tabItems.Add(tabAdd);
            tabDynamic.DataContext = tabItems;
            tabDynamic.SelectedIndex = 0;
        }

        private TabItem AddTabItem()
        {
            int count = tabItems.Count;
            TabItem tab = new TabItem();            
            tab.Header = string.Format("Action {0}", count);
            tab.Name = string.Format("Action{0}", count);
            tab.HeaderTemplate = tabDynamic.FindResource("TabHeader") as DataTemplate;

            StackPanel grd = new StackPanel();
            grd.HorizontalAlignment = HorizontalAlignment.Left;
            grd.VerticalAlignment = VerticalAlignment.Top;

            GroupBox group_action = new GroupBox();
            Grid grid_action = new Grid();
            group_action.Header = tab.Name.ToString();
            group_action.Height = 120;
            group_action.Width = 700;

            ScrollViewer scroll = new ScrollViewer();
            scroll.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            scroll.Content = grid_action;

            
            grid_action.Children.Add(new ComboBox { Height = 23, HorizontalAlignment = System.Windows.HorizontalAlignment.Left, Name = "cmb_ActionType", VerticalAlignment = System.Windows.VerticalAlignment.Top, Width = 194, Margin = new Thickness(5, 26, 0, 0), ItemsSource = new string[] { "Q-Sys Control related actions", "External device based actions", "Telnet based actions", "Delay", "Firmware Upgrade/Downgrade", "Q-SysDesigner Application related actions" }});
            
            cmb_ComponentType = new ComboBox();
            cmb_ComponentType.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            cmb_ComponentType.Name = "cmb_ComponentType";
            cmb_ComponentType.VerticalAlignment = System.Windows.VerticalAlignment.Top;
            cmb_ComponentType.Width = 120;
            cmb_ComponentType.Margin = new Thickness(5, 60, 0, 0);
            grid_action.Children.Add(cmb_ComponentType);

            cmb_ComponentName = new ComboBox();
            cmb_ComponentName.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            cmb_ComponentName.Name = "cmb_ComponentName";
            cmb_ComponentName.VerticalAlignment = System.Windows.VerticalAlignment.Top;
            cmb_ComponentName.Width = 150;
            cmb_ComponentName.Margin = new Thickness(140, 60, 0, 0);
            grid_action.Children.Add(cmb_ComponentName);

            cmb_ComponentName = new ComboBox();
            cmb_ComponentName.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            cmb_ComponentName.Name = "cmb_Propery";
            cmb_ComponentName.VerticalAlignment = System.Windows.VerticalAlignment.Top;
            cmb_ComponentName.Width = 120;
            cmb_ComponentName.Margin = new Thickness(305, 60, 0, 0);
            grid_action.Children.Add(cmb_ComponentName);

            cmb_ComponentName = new ComboBox();
            cmb_ComponentName.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            cmb_ComponentName.Name = "cmb_Value";
            cmb_ComponentName.VerticalAlignment = System.Windows.VerticalAlignment.Top;
            cmb_ComponentName.Width = 50;
            cmb_ComponentName.Margin = new Thickness(440, 60, 0, 0);
            grid_action.Children.Add(cmb_ComponentName);

            group_action.Content = scroll;
            grd.Children.Add(group_action);

            GroupBox group_delay = new GroupBox();
            Grid grid_delay = new Grid();
            group_delay.Header = "Delay before Verification";
            group_delay.Height = 75;
            group_delay.Width = 400;

            ScrollViewer scroll_delay = new ScrollViewer();
            scroll_delay.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            scroll_delay.Content = grid_delay;

            cmb_ComponentType = new ComboBox();
            cmb_ComponentType.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            cmb_ComponentType.Name = "cmb_delaytime";
            cmb_ComponentType.VerticalAlignment = System.Windows.VerticalAlignment.Top;
            cmb_ComponentType.Width = 120;
            cmb_ComponentType.Margin = new Thickness(5, 26, 0, 0);
            grid_delay.Children.Add(cmb_ComponentType);

            group_delay.Content = scroll_delay;
            grd.Children.Add(group_delay);


            tab.Content = grd;
            tabItems.Insert(count - 1, tab);

            return tab;
        }

        private void tabAdd_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            tabDynamic.DataContext = null;
            TabItem tab = this.AddTabItem();
            tabDynamic.DataContext = tabItems;
            tabDynamic.SelectedItem = tab;            
        }

        private void tabDynamic_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            TabItem tab = tabDynamic.SelectedItem as TabItem;
            if (tab == null) return;

            if (tab.Equals(tabAdd))
            {
                tabDynamic.DataContext = null;
                TabItem newTab = this.AddTabItem();
                tabDynamic.DataContext = tabItems;
                tabDynamic.SelectedItem = newTab;
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            string tabName = (sender as Button).CommandParameter.ToString();

            var item = tabDynamic.Items.Cast<TabItem>().Where(i => i.Name.Equals(tabName)).SingleOrDefault();

            TabItem tab = item as TabItem;

            if (tab != null)
            {
                if (tabItems.Count < 3)
                {
                    MessageBox.Show("Cannot remove last tab.");
                }
                else if (MessageBox.Show(string.Format("Are you sure you want to remove the tab '{0}'?", tab.Header.ToString()),
                    "Remove Tab", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    TabItem selectedTab = tabDynamic.SelectedItem as TabItem;
                    tabDynamic.DataContext = null;

                    tabItems.Remove(tab);
                    tabDynamic.DataContext = tabItems;
                    if (selectedTab == null || selectedTab.Equals(tab))
                    {
                        selectedTab = tabItems[0];
                    }
                    tabDynamic.SelectedItem = selectedTab;
                }
            }
        }

        private void comboBox1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            cmb_ComponentType.Items.Add("Hi");
            //tabItem1.Height = 1000;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

        }
    }
}
