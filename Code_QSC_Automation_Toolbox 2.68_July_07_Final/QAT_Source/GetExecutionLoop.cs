using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Collections.ObjectModel;

namespace QSC_Test_Automation
{
    public class GetExecutionLoop : INotifyPropertyChanged
    {
        private string _TypeOfLoopOption = "System.Windows.Controls.ComboBoxItem: ";
        private string _TestSuiteName;
        public string TestSuiteName
        {
            get
            {
                return _TestSuiteName;
            }
            set
            {
                _TestSuiteName = value;
                NotifyPropertyChanged("TestSuiteName");
            }
        }

        private ObservableCollection<string> _cmbitems = new ObservableCollection<string> { string.Empty, "Number Of Times", "Duration" };
        public ObservableCollection<string> cmbitems
        {
            get
            {
                return _cmbitems;
            }
            set
            {
                _cmbitems = value;
                NotifyPropertyChanged("cmbitems");
            }
        }

        public string TypeOfLoopOption
        {
            get
            {
                return _TypeOfLoopOption;
            }
            set
            {
                _TypeOfLoopOption = value;
                int i = _TypeOfLoopOption.Length;
                if(i>15)
                {
                    _TypeOfLoopOption = _TypeOfLoopOption.Remove(0, 38);
                }
                
                if (_TypeOfLoopOption == string.Empty)
                {
                    _TypeOfLoopOptionIndex = 0;
                    blnNumOfLoop = false;
                    blnNumOfLoopCmb = false;
                    Width = "0";

                }
                if (_TypeOfLoopOption == "Number Of Times")
                {
                    _TypeOfLoopOptionIndex = 1;
                    blnNumOfLoop = true;
                    blnNumOfLoopCmb = false;
                    Width = "250";
                    NumOfLoop = string.Empty;
                }
                if (_TypeOfLoopOption == "Duration")
                {
                    _TypeOfLoopOptionIndex = 2;
                    blnNumOfLoop = true;
                    blnNumOfLoopCmb = true;
                    Width = "125";
                    NumOfLoop = string.Empty;                  
                }

                NotifyPropertyChanged("TypeOfLoopOption");
            }
        }

        private int _TypeOfLoopOptionIndex = 0;
        public int TypeOfLoopOptionIndex
        {
            get
            {
                return _TypeOfLoopOptionIndex;
            }
            set
            {
                _TypeOfLoopOptionIndex = value;
                NotifyPropertyChanged("TypeOfLoopOptionIndex");
            }
        }


        private string _NumOfLoop;
        public string NumOfLoop
        {
            get
            {
                return _NumOfLoop;
            }
            set
            {
                _NumOfLoop = value;
                NotifyPropertyChanged("NumOfLoop");

            }
        }

        private bool _blnNumOfLoop = false;
        public bool blnNumOfLoop
        {
            get
            {
                return _blnNumOfLoop;
            }
            set
            {
                _blnNumOfLoop = value;
                NotifyPropertyChanged("blnNumOfLoop");

            }
        }

        private bool _blnNumOfLoopCmb = false;
        public bool blnNumOfLoopCmb
        {
            get
            {
                return _blnNumOfLoopCmb;
            }
            set
            {
                _blnNumOfLoopCmb = value;
                NotifyPropertyChanged("blnNumOfLoopCmb");
            }
        }

        private string _Width = "0";
        public string Width
        {
            get
            {
                return _Width;
            }
            set
            {
                _Width = value;
                NotifyPropertyChanged("Width");
            }
        }


        private int _txtDurCmb=0;
        public int txtDurCmb 
        {
            get
            {
                return _txtDurCmb;
            }
            set
            {
                _txtDurCmb = value;
                if (_txtDurCmbSelectedValue == "Hour")
                {
                    _txtDurCmb = 0;
                }
                if (_txtDurCmbSelectedValue == "Minute")
                {
                    _txtDurCmb = 1;
                }
                NotifyPropertyChanged("txtDurCmb");
            }
        }

        private string _txtDurCmbSelectedValue = "Hour";
        public string txtDurCmbSelectedValue
        {
            get
            {
                return _txtDurCmbSelectedValue;
            }
            set
            {
                _txtDurCmbSelectedValue = value;
                int i = txtDurCmbSelectedValue.Length;
                if (i > 15)
                {
                    _txtDurCmbSelectedValue = _txtDurCmbSelectedValue.Remove(0, 38);
                }
                if (_txtDurCmbSelectedValue == "Hour")
                {
                    txtDurCmb = 0;
                }
                if (_txtDurCmbSelectedValue == "Minute")
                {
                    txtDurCmb = 1;
                }
                NotifyPropertyChanged("txtDurCmbSelectedValue");
            }
        }

        private bool _blnRedeployedDesign = true;
        public bool blnRedeployedDesign
        {
            get
            {
                return _blnRedeployedDesign;
            }
            set
            {
                _blnRedeployedDesign = value;
                NotifyPropertyChanged("blnRedeployedDesign");

            }
        }

        private bool _blnGrid = false;
        public bool blnGrid
        {
            get
            {
                return _blnGrid;
            }
            set
            {
                _blnGrid = value;
                NotifyPropertyChanged("blnGrid");

            }
        }


        #region "InotifyPropertyChanged"

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(string property)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(property));
            }


        }
        #endregion
    }


   
}
