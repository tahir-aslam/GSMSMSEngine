using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Scenario.SMSGateWay.Model
{
    public class Modem : INotifyPropertyChanged
    {

        public int ID { get; set; }
        private string _COM;
        public string COM
        {
            get { return _COM; }
            set { _COM = value; OnPropertyChanged("COM"); }
        }
        public string Brand { get; set; }
        public string Model { get; set; }
        public string IMEI { get; set; }
        public string SMSC { get; set; }
        public string Battery { get; set; }
        public string Signal { get; set; }

        private bool _isChecked;

        public event PropertyChangedEventHandler PropertyChanged;

        public bool IsChecked
        {
            get
            {
                return _isChecked;
            }
            set
            {
                _isChecked = value;
                OnPropertyChanged("IsChecked");
            }
        }
        public string IsActive { get; set; }

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
