using Scenario.SMSGateWay.Model;
using System;
using System.Collections.Generic;
using System.IO.Ports;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using Scenario.GSMSMSEngine;

namespace Scenario.SMSGateWay
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        SMSEngine m_SMSEngine;
        DispatcherTimer refreshDataTimer;

        public MainWindow()
        {
            InitializeComponent();
            //LoadModems();
        }

        void LoadModems()
        {
            Modem modem;
            ModemsList = new List<Modem>();   
            foreach (var portName in SerialPort.GetPortNames())
            {
                modem = new Modem()
                {
                     COM = portName
                };
                ModemsList.Add(modem);                
            }
            v_ModemsDataGrid.ItemsSource = ModemsList;

            this.DataContext = ModemsList;
        }
        //private void CheckBox_Checked(object sender, RoutedEventArgs e)
        //{
        //    var checkBox = sender as CheckBox;
        //    v_ModemsDataGrid.SelectedItem = e.Source;
        //    Modem obj = new Modem();
        //    obj = (Modem)v_ModemsDataGrid.SelectedItem;
            
        //}

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (v_MenuTabControl.SelectedIndex == 1)
            {
                LoadModems();
            }
        }

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {

        }

        private List<Modem> _modemsList;

        public List<Modem> ModemsList
        {
            get
            {
                return _modemsList;
            }
            set
            {
                _modemsList = value;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            m_SMSEngine = new SMSEngine();
            //v_log_datagrid.ItemsSource = m_SMSEngine.m_ApplicationLogsList.OrderByDescending(x=>x.CreatedDateTime);
            this.DataContext = m_SMSEngine;
            StartRefreshTimer();
            v_MessageBox.Text = "Started";
        }

        void StartRefreshTimer()
        {
            refreshDataTimer = new DispatcherTimer();
            refreshDataTimer.Interval = new TimeSpan(0, 0, 2);
            refreshDataTimer.Tick += refreshDataTimer_Tick;
            refreshDataTimer.Start();
        }

        void refreshDataTimer_Tick(object sender, EventArgs e)
        {
            v_TotalSmsSent.Text = m_SMSEngine.m_TotalSmsSent.ToString();
            v_TotalSmsQueued.Text = m_SMSEngine.m_SmsNos.Count().ToString();
            v_TotalModems.Text = m_SMSEngine.ModemsCount().ToString();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            try
            {
                m_SMSEngine.StopSMSEnging();
                v_MessageBox.Text = "Stopped";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
