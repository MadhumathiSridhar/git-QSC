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
    /// Interaction logic for CopyReplace.xaml
    /// </summary>
    public partial class CopyReplace : Window,INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

      

        private void Onchange(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        private string filename = "testplan";
        private string actionchoosen = string.Empty;
        private bool checkbox = false;
        private string conflicts = string.Empty;
        private string headername = string.Empty;
        private Visibility chkVisibility = Visibility.Visible;


        public string Filename
        {
            get { return filename; }
            set
            {
                filename = value;
                Onchange("Filename");
            }
        }

        public string Headername
        {
            get { return headername; }
            set
            {
                headername = value;
                Onchange("Headername");
            }
        }

        public string Actionchoosen
        {
            get { return actionchoosen; }
            set
            {
                actionchoosen = value;
                Onchange("Actionchoosen");
            }
        }

        public bool Checkbox
        {
            get { return checkbox; }
            set
            {
                checkbox = value;
                Onchange("Checkbox");
            }
        }

        public string Conflicts
        {
            get { return ("Do this for the next " + conflicts+" Conflicts"); }
            set
            {
                conflicts = value;
                Onchange("Conflicts");
            }
        }


        public Visibility ChkBoxVisibility
        {
            get { return chkVisibility;  }
            set
            {
                chkVisibility = value;
                Onchange("ChkBoxVisibility");
            }
        }

        public CopyReplace()
        {
            InitializeComponent();
            this.DataContext = this;
        }

        private void CopyReplaceClick(object sender, RoutedEventArgs e)
        {
            Actionchoosen = "CopyAndReplace";
            this.Hide();
        }

        private void DontCopyClick(object sender, RoutedEventArgs e)
        {
            Actionchoosen = "DontCopy";
            this.Hide();
        }

        private void KeepBothClick(object sender, RoutedEventArgs e)
        {
            Actionchoosen = "KeepBoth";
            this.Hide();
        }

        //private void SkipClick(object sender, RoutedEventArgs e)
        //{
        //    Actionchoosen = "Skip";
        //    this.Hide();
        //}

        private void CancelClick(object sender, RoutedEventArgs e)
        {
            Actionchoosen = "Cancel";
            this.Hide();
        }

        private void CopyReplaceClose(object sender, CancelEventArgs e)
        {
            Actionchoosen = "Cancel";
        }

        private void CopyReplaceClick(object sender, MouseButtonEventArgs e)
        {
            Actionchoosen = "CopyAndReplace";
            this.Hide();
        }

        private void DontCopyClick(object sender, MouseButtonEventArgs e)
        {
            Actionchoosen = "DontCopy";
            this.Hide();
        }

        private void KeepBothClick(object sender, MouseButtonEventArgs e)
        {
            Actionchoosen = "KeepBoth";
            this.Hide();
        }
    }
}
