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
using System.Windows.Forms;
using Microsoft.Win32.SafeHandles;
using System.Runtime.InteropServices;
using System.Threading;
using System.ComponentModel;

namespace Scenario.SMSGateWay
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        SMSEngine m_SMSEngine;
        DispatcherTimer refreshDataTimer;
        bool m_IsRestartEnabled = true;
        bool m_IsRefreshDataTimerEnabled = true;

        public MainWindow()
        {
            InitializeComponent();
            Thread.Sleep(1000);
            StartSMSEngine();
            //LoadModems();
            Console.WriteLine("MainWindow() Constructor");
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

        void StartSMSEngine()
        {
            try
            {
                Thread.Sleep(2000);
                m_SMSEngine = new SMSEngine();
                //v_log_datagrid.ItemsSource = m_SMSEngine.m_ApplicationLogsList.OrderByDescending(x=>x.CreatedDateTime);
                this.DataContext = m_SMSEngine;                
                v_MessageBox.Text = "Started";
            }
            catch (Exception ex)
            {
                throw ex;
            }            
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //StartSMSEngine();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message);
            }
        }

        void StartRefreshTimer()
        {
            refreshDataTimer = new DispatcherTimer();
            refreshDataTimer.Interval = new TimeSpan(0, 0, 1);
            refreshDataTimer.Tick += refreshDataTimer_Tick;
            refreshDataTimer.Start();
        }

        void refreshDataTimer_Tick(object sender, EventArgs e)
        {
            if (m_IsRefreshDataTimerEnabled)
            {
                try
                {
                    m_IsRefreshDataTimerEnabled = false;
                    v_TotalSmsSent.Text = m_SMSEngine.m_TotalSmsSent.ToString();
                    v_TotalSmsQueued.Text = m_SMSEngine.m_SmsNos.Count().ToString();
                    v_TotalModems.Text = m_SMSEngine.ModemsCount().ToString();
                    v_TotalSmsSentMonthly.Text = m_SMSEngine.m_TotalSmsSentMonthly.ToString();

                    if (m_SMSEngine.m_IsWindowCloseEnabled)
                    {
                        m_SMSEngine.m_IsWindowCloseEnabled = false;
                        m_IsRestartEnabled = false;
                        Restart();
                    }                    
                }
                catch (Exception ex)
                { 
                    System.Windows.MessageBox.Show(ex.Message);
                }
                finally
                {
                    m_IsRefreshDataTimerEnabled = true;
                }
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            try
            {
                m_IsRestartEnabled = false;
                try
                {
                    m_SMSEngine.StopSMSEnging();
                    m_SMSEngine = null;
                }
                catch (Exception ex)
                {
                }
                v_MessageBox.Text = "Stopped";
                this.Close();
            }
            catch (Exception ex)
            {
                m_SMSEngine = null;
                System.Windows.MessageBox.Show(ex.Message);
            }
        }

        private void DisablePorts_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.Registry.SetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\USBSTOR", "Start", 4, Microsoft.Win32.RegistryValueKind.DWord);
        }
        
        private void RestartApplication_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Restart();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message);
            }
        }

        void Restart()
        {
            try
            {
                m_IsRestartEnabled = false;
                m_IsRefreshDataTimerEnabled = false;
                refreshDataTimer.Stop();
                refreshDataTimer = null;
                m_SMSEngine.StopSMSEnging();
                v_MessageBox.Text = "Stopped";
            }
            catch (Exception ex)
            {                
                //System.Windows.MessageBox.Show(ex.Message);
            }
            finally
            {
                m_SMSEngine = null;
                //restart applic\ation                
                System.Diagnostics.Process.Start(System.Windows.Application.ResourceAssembly.Location);
                System.Windows.Application.Current.Shutdown();
                //this.Close();
                //MainWindow window = new MainWindow();
                //window.Show();
                //StartSMSEngine();
                Console.WriteLine("Restart Finished");
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            try
            {
                Restart();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message);
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            if (m_IsRestartEnabled)
            {
                try
                {
                    Restart();
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show(ex.Message);
                }
            }
        }

        #region sleep and awake

        private void SleepAndStartSystem_Click(object sender, RoutedEventArgs e)
        {
            //for awake
            SetWaitForWakeUpTime();
            //For Sleep
            System.Windows.Forms.Application.SetSuspendState(PowerState.Suspend, false, false);
        }
        [DllImport("kernel32.dll")]
        public static extern SafeWaitHandle CreateWaitableTimer(IntPtr lpTimerAttributes,
                                                                 bool bManualReset,
                                                               string lpTimerName);
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetWaitableTimer(SafeWaitHandle hTimer,
                                                    [In] ref long pDueTime,
                                                              int lPeriod,
                                                           IntPtr pfnCompletionRoutine,
                                                           IntPtr lpArgToCompletionRoutine,
                                                             bool fResume);

        void SetWaitForWakeUpTime()
        {
            DateTime utc = DateTime.Now.AddSeconds(5);
            long duetime = utc.ToFileTime();
            //long duetime = -300000000;
            using (SafeWaitHandle handle = CreateWaitableTimer(IntPtr.Zero, true, "MyWaitabletimer"))
            {
                if (SetWaitableTimer(handle, ref duetime, 0, IntPtr.Zero, IntPtr.Zero, true))
                {
                    using (EventWaitHandle wh = new EventWaitHandle(false, EventResetMode.AutoReset))
                    {
                        wh.SafeWaitHandle = handle;
                        wh.WaitOne();
                    }
                }
                else
                {
                    throw new Win32Exception(Marshal.GetLastWin32Error());
                }
            }

            // You could make it a recursive call here, setting it to 1 hours time or similar
            Console.WriteLine("Wake up call");
            Console.ReadLine();
        }
        #endregion

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                StartRefreshTimer();
            }
            catch (Exception ex)
            {

            }
            finally
            {
                Console.WriteLine("Window_Loaded()");
            }
        }
    }
}
