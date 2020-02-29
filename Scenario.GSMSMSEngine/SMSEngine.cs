using GsmComm.GsmCommunication;
using GsmComm.PduConverter;
using GsmComm.PduConverter.SmartMessaging;
using MySql.Data.MySqlClient;
using Scenario.GSMSMSEngine.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Threading;

namespace Scenario.GSMSMSEngine
{
    public class SMSEngine
    {
        //public static SerialPort port = new SerialPort();
        private BackgroundWorker bw;

        SMSHistory sh;
        string[] messages;
        string a;
        string b;
        int error_no = 0;
        private static ushort refNumber;

        bool isWholeSent = false;
        int i = 0;

        MySql.Data.MySqlClient.MySqlConnection ConOnline{get; set; }
        MySql.Data.MySqlClient.MySqlConnection ConLocal { get; set; }
        DispatcherTimer refreshDataTimer;
        DAL.MiscDAL miscDAL;
        public bool m_IsEncoded = false;
        private bool m_IsSynchronize = true;
        private bool m_IsOnlineConnectionOpen;
        public List<SMSQueue> m_SmsNos = new List<SMSQueue>();
        public List<Modem> Modems;
        Modem SelectedComm;
        public int m_TotalSmsSent = 0;
        private ObservableCollection<ApplicationLog> _applicationLogsList;
        ApplicationLog applicationLog;
        private static object _syncLock = new object();
        string message;

        public SMSEngine()
        {
            miscDAL = new DAL.MiscDAL();
            Modems = new List<Modem>();            
            ApplicationLogsList = new ObservableCollection<ApplicationLog>();
            //Enable the cross acces to this collection elsewhere
            BindingOperations.EnableCollectionSynchronization(ApplicationLogsList, _syncLock);

            message = "Starting SMS Engine";
            AddLog(EventLevel.Information.ToString(), DateTime.Now, EventSource.Initilization.ToString(), message);

            try
            {             
                ConLocal = miscDAL.OpenLocalDatabaseConnection();
                m_SmsNos = miscDAL.GetSMSQueue(ConLocal);                
                StartDataTimer();
                StartSMSEngine();
                try
                {
                    ConOnline = miscDAL.OpenOnlineDatabaseConnection();
                    m_IsOnlineConnectionOpen = true;
                }
                catch (Exception ex)
                {
                    m_IsOnlineConnectionOpen = false;
                    message = "Online Server Exception: "+ex.Message;
                    AddLog(EventLevel.Information.ToString(), DateTime.Now, EventSource.Initilization.ToString(), message);
                    MessageBox.Show(ex.Message);
                }
            }
            catch (Exception ex)
            {
                message = "Exception: " + ex.Message;
                AddLog(EventLevel.Information.ToString(), DateTime.Now, EventSource.Initilization.ToString(), message);
                MessageBox.Show(ex.Message);
            }



        }
        public ObservableCollection<ApplicationLog> ApplicationLogsList
        {
            get { return _applicationLogsList; }
            set { _applicationLogsList = value; }
        }
        #region Send SMS

        static AutoResetEvent readNow = new AutoResetEvent(false);

        public void StartDataTimer()
        {
            refreshDataTimer = new DispatcherTimer();
            refreshDataTimer.Interval = new TimeSpan(0, 0, 3);
            refreshDataTimer.Tick += refreshDataTimer_Tick;
            refreshDataTimer.Start();
        }
        void AddLog(string level, DateTime dateTime, string source, string Message)
        {
            ApplicationLog applicationLog = new ApplicationLog()
            {
                Level = level,
                CreatedDateTime = dateTime,
                Source = source,
                Message = Message
            };
            //lock (_syncLock)
            //{
                ApplicationLogsList.Insert(0,applicationLog);
            //}
        }
        void SynchronizeData()
        {
            if (m_IsSynchronize)
            {
                m_IsSynchronize = false;
                message = "Synchronization started";
                AddLog(EventLevel.Information.ToString(), DateTime.Now, EventSource.Synchronization.ToString(), message);

                if (miscDAL.SynchronizeSMSQueue(ConLocal, ConOnline) > 0)
                {
                    message = "Synchronization completed";
                    AddLog(EventLevel.Information.ToString(), DateTime.Now, EventSource.Synchronization.ToString(), message);
                }
                else
                {
                    message = "Synchronization completed with not record updated";
                    AddLog(EventLevel.Information.ToString(), DateTime.Now, EventSource.Synchronization.ToString(), message);
                }
                m_IsSynchronize = true;
            }
        }
        void refreshDataTimer_Tick(object sender, EventArgs e)
        {
            if (m_IsSynchronize)
            {
                try
                {
                    if (m_IsOnlineConnectionOpen)
                    {
                        SynchronizeData();
                    }
                    else
                    {
                        ConOnline = miscDAL.OpenOnlineDatabaseConnection();
                    }
                }
                catch (Exception ex)
                {
                    m_IsSynchronize = true;
                    //Connection must be valid and open.
                    if (ex.Message.ToUpper().Contains("CONNECTION"))
                    {
                        ConOnline = miscDAL.OpenOnlineDatabaseConnection();
                    }
                    message = "Synchronization EXception: " + ex.Message;
                    AddLog(EventLevel.Information.ToString(), DateTime.Now, EventSource.Synchronization.ToString(), message);
                }
            }           
        }
        void AddNewModems()
        {
            message = "Add New Modems";
            AddLog(EventLevel.Information.ToString(), DateTime.Now, EventSource.AddModem.ToString(), message);
            
            foreach (var portName in SerialPort.GetPortNames())
            {
                //if (portName == "COM22" || portName == "COM23" || portName == "COM24" || portName == "COM25" || portName == "COM26" || portName == "COM27" || portName == "COM28")
                //{
                try
                {
                    if (!Modems.Exists(x => x.GsmCommMain.PortName == portName))
                    {
                        //SerialPort port = new SerialPort(portName);
                        Modem modem = new Modem();
                        modem.GsmCommMain = OpenPort(portName);
                        if (modem.GsmCommMain.IsOpen() && modem.GsmCommMain.IsConnected())
                        {
                            modem.IsFree = true;
                            Modems.Add(modem);

                            message = portName + " Successfully added and is Connected";
                            AddLog(EventLevel.Information.ToString(), DateTime.Now, EventSource.AddModem.ToString(), message);
                        }
                        else
                        {
                            //port.Close();
                            modem.GsmCommMain.Close();

                            message = portName + " Is not connected";
                            AddLog(EventLevel.Error.ToString(), DateTime.Now, EventSource.AddModem.ToString(), message);
                        }
                    }
                    else
                    {
                        message = portName + " Already Exists";
                        AddLog(EventLevel.Information.ToString(), DateTime.Now, EventSource.AddModem.ToString(), message);
                    }
                }
                catch (Exception ex)
                {
                    message = portName + " Exception: " + ex.Message;
                    AddLog(EventLevel.Error.ToString(), DateTime.Now, EventSource.AddModem.ToString(), message);
                }
                //}
            }
        }
        public void StartSMSEngine()
        {
            try
            {

                AddNewModems();

                bw = new BackgroundWorker();
                bw.WorkerReportsProgress = true;
                bw.WorkerSupportsCancellation = true;
                bw.DoWork += new DoWorkEventHandler(bw_DoWork);
                bw.ProgressChanged += new ProgressChangedEventHandler(bw_ProgressChanged);
                bw.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bw_RunWorkerCompleted);
                bw.RunWorkerAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        public GsmCommMain OpenPort(string portName)
        {
            try
            {
                //comm = new GsmCommMain("COM22", 115200, 300);
                GsmCommMain comm = new GsmCommMain(portName, 115200, 300);
                comm.Open();
                return comm;
            }
            catch (Exception ex)
            {
                //MessageBox.Show("Opening Port Exception: " + ex.Message);
                throw ex;
            }
        }
        public void StopSMSEnging()
        {
            message = "Stoping SMS Engine";
            AddLog(EventLevel.Information.ToString(), DateTime.Now, EventSource.Stopping.ToString(), message);

            foreach (var item in Modems)
            {
                try
                {
                    if (item.GsmCommMain.IsOpen())
                    {
                        message = item.GsmCommMain.PortName +" Closing";
                        AddLog(EventLevel.Information.ToString(), DateTime.Now, EventSource.Stopping.ToString(), message);

                        item.GsmCommMain.Close();
                    }
                }
                catch (Exception ex)
                {
                    message = "Stoping SMS Engine Execption: "+ex.Message;
                    AddLog(EventLevel.Error.ToString(), DateTime.Now, EventSource.Stopping.ToString(), message);

                    throw ex;
                }
            }
        }
        public void closePort()
        {
            try
            {
                if (SelectedComm.GsmCommMain.IsOpen())
                {
                    SelectedComm.GsmCommMain.Close();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        async void SendMessage(SMSQueue obj, Modem comm, BackgroundWorker worker, DoWorkEventArgs e)
        {
            await SendMessageAsync(obj, comm, worker, e);
        }
        private void bw_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;
            do
            {
                if (Modems.Count > 0)
                {                    
                    message = "Background worker started, Modems Count= " + Modems.Count;
                    AddLog(EventLevel.Information.ToString(), DateTime.Now, EventSource.bw_DoWork.ToString(), message);
                    AddLog(EventLevel.Information.ToString(), DateTime.Now, EventSource.bw_DoWork.ToString(), "Selected Modem:" + SelectedComm.GsmCommMain.PortName);

                    SelectedComm = Modems[0];
                    try
                    {
                        do
                        {
                            foreach (var smsQueue in m_SmsNos)
                            {
                                Thread.Sleep(500);
                                SendMessage(smsQueue, SelectedComm, worker, e);
                                SelectedComm = SelectModem();
                            }
                            m_SmsNos = miscDAL.GetSMSQueue(ConLocal);
                        }
                        while (true);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                    finally
                    {
                        closePort();
                    }
                }
                else
                {
                    AddNewModems();
                    message = " Modems Count= " + Modems.Count;
                    AddLog(EventLevel.Warning.ToString(), DateTime.Now, EventSource.bw_DoWork.ToString(), message);
                    Thread.Sleep(1000);
                }
            }
            while (Modems.Count == 0);
        }
        private void bw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if ((e.Cancelled == true))
            {

            }

            else if (!(e.Error == null))
            {
            }

            else
            {

                //   this.status_textblock.Text = "  Successfully Sent!";
            }
        }
        private void bw_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {            
        }
        Modem SelectModem()
        {
            try
            {
                do
                {
                    AddNewModems(); //if new exists
                    int totalModems = Modems.Count;
                    if (Modems.IndexOf(SelectedComm) == Modems.Count - 1)
                    {
                        SelectedComm = Modems[0];
                        if (SelectedComm.IsFree)
                        {
                            if (SelectedComm.TotalSmsSent <= 200)
                            {
                                if ((SelectedComm.StartTime - DateTime.Now).Minutes < 15)
                                {
                                    return SelectedComm;
                                }
                            }
                            else
                            {
                                SelectedComm.TotalSmsSent = 0;
                                SelectedComm.StartTime = DateTime.Now;
                            }
                        }
                    }
                    else
                    {
                        SelectedComm = Modems[Modems.IndexOf(SelectedComm) + 1];
                        if (SelectedComm.IsFree)
                        {
                            if (SelectedComm.TotalSmsSent <= 200)
                            {
                                if ((SelectedComm.StartTime - DateTime.Now).Minutes < 15)
                                {
                                    return SelectedComm;
                                }
                            }
                            else
                            {
                                SelectedComm.TotalSmsSent = 0;
                                SelectedComm.StartTime = DateTime.Now;
                            }
                        }
                    }
                }
                while (true);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public int ModemsCount()
        {
            if (Modems != null)
            {
                return Modems.Count();
            }
            return 0;
        }
        void UpdateModemStatus(Modem m)
        {
            foreach (var item in Modems.Where(x => x.GsmCommMain.PortName == m.GsmCommMain.PortName))
                item.IsFree = m.IsFree;
        }
        public Task SendMessageAsync(SMSQueue obj, Modem comm, BackgroundWorker worker, DoWorkEventArgs e)
        {
            return Task.Run(() => sendMsg(obj, comm, worker, e));
        }
        public bool sendMsg(SMSQueue obj, Modem comm, BackgroundWorker worker, DoWorkEventArgs e)
        {
            bool isSend = false;
            comm.IsFree = false;
            UpdateModemStatus(comm);

            message = comm.GsmCommMain.PortName+" IsFree=False";
            AddLog(EventLevel.Warning.ToString(), DateTime.Now, EventSource.SendMessage.ToString(), message);

            i = 0;

            SmsSubmitPdu[] pdu;
            if (m_IsEncoded)
            {
                pdu = CreateConcatTextMessage(obj.sms_message, true, Convert.ToString("+92" + obj.receiver_cell_no));
            }
            else
            {
                pdu = CreateConcatTextMessage(obj.sms_message, false, Convert.ToString("+92" + obj.receiver_cell_no));
            }

            for (int j = 0; j < pdu.Length; j++)
            {
                try
                {
                    if (comm.GsmCommMain.IsConnected())
                    {
                        comm.GsmCommMain.SendMessage(pdu[j]);
                        isSend = true;
                        m_TotalSmsSent++;
                        if (j + 1 == pdu.Length)
                        {
                            if (comm.TotalSmsSent == 0)
                            {
                                comm.StartTime = DateTime.Now;
                            }
                            comm.IsFree = true;
                            UpdateModemStatus(comm);
                            comm.TotalSmsSent = comm.TotalSmsSent + pdu.Length;
                            isWholeSent = true;
                            obj.sms_status = "Sent";
                            obj.updated_date_time = DateTime.Now;
                            message = comm.GsmCommMain.PortName + " IsFree=True, Receiver="+obj.receiver_cell_no+" Message:"+obj.sms_message;
                            AddLog(EventLevel.Warning.ToString(), DateTime.Now, EventSource.SendMessage.ToString(), message);
                        }                        
                    }
                    else
                    {
                        message = comm.GsmCommMain.PortName + " IsConnected=False";
                        AddLog(EventLevel.Warning.ToString(), DateTime.Now, EventSource.SendMessage.ToString(), message);

                        j--;
                        // MessageBox.Show("IsConnected=false", "Error", MessageBoxButton.OK, MessageBoxImage.Stop);
                        Thread.Sleep(1000);
                        OpenPort(comm.GsmCommMain.PortName);
                        isSend = false;
                        comm.IsFree = false;
                        UpdateModemStatus(comm);
                        obj.sms_status = "Not Sent";
                        Thread.Sleep(1000);

                        //j--;
                    }
                }
                catch (Exception ex)
                {
                    if (ex.Message.Contains("Message service error 500 occurred."))
                    {
                    }
                    //Access is denied.
                    else if (ex.Message.Contains("Access is denied."))
                    {
                    }
                    else
                    {
                    }
                    isSend = false;
                    obj.sms_status = "Not Sent";
                    comm.IsFree = false;
                    UpdateModemStatus(comm);

                    message = comm.GsmCommMain.PortName + " Exception:"+ex.Message;
                    AddLog(EventLevel.Warning.ToString(), DateTime.Now, EventSource.SendMessage.ToString(), message);
                }

            }
            // saved to sms history table whether sent or not
            //change for queue
            if (isSend)
            {
                sh = new SMSHistory();
                sh.sender_id = obj.id.ToString();
                sh.sender_name = obj.receiver_name;
                sh.class_id = obj.class_id.ToString();
                sh.class_name = obj.class_name;
                sh.section_id = obj.section_id.ToString();
                sh.section_name = obj.section_name;
                sh.cell = obj.receiver_cell_no;
                sh.msg = obj.sms_message;
                sh.sms_type = obj.sms_type;
                sh.created_by = obj.created_by;
                sh.date_time = DateTime.Now;

                if (miscDAL.InsertSMSHistory(sh,ConLocal) > 0)
                {
                    if (miscDAL.UpdateSMSQueue(obj,ConLocal) > 0)
                    {

                    }
                    else
                    {
                        MessageBox.Show("Not updated sms queue");
                    }
                }
                else
                {
                    MessageBox.Show("Sms History not inserted");
                }

            }
            return isSend;
        }
        public void setConnected()
        {
            string cmbCOM = SelectedComm.GsmCommMain.PortName;
            if (cmbCOM == "")
            {
                MessageBox.Show("Invalid Port Name");
                return;
            }
            SelectedComm.GsmCommMain = new GsmCommMain(cmbCOM, 115200, 150);
            //Cursor.Current = Cursors.Default;

            bool retry;
            do
            {
                retry = false;
                try
                {
                    //Cursor.Current = Cursors.WaitCursor;
                    SelectedComm.GsmCommMain.Open();
                    //Cursor.Current = Cursors.Default;
                    MessageBox.Show("Modem Connected Sucessfully");
                }
                catch (Exception)
                {
                    //Cursor.Current = Cursors.Default;                    
                }
            }
            while (retry);

        }
        static void DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                if (e.EventType == SerialData.Chars)
                    readNow.Set();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion


        //public static SmsSubmitPdu[] CreateConcatTextMessage(string userDataText, string destinationAddress)
        //{
        //    return SmartMessageFactory.CreateConcatTextMessage(userDataText, false, destinationAddress);
        //}

        /// <summary>
        /// Creates a concatenated text message.
        /// </summary>
        /// <param name="userDataText">The message text.</param>
        /// <param name="unicode">Specifies if the userDataText is to be encoded as Unicode. If not, the GSM 7-bit default alphabet is used.</param>
        /// <param name="destinationAddress">The message's destination address.</param>
        /// <returns>A set of <see cref="T:GsmComm.PduConverter.SmsSubmitPdu" /> objects that represent the message.</returns>
        /// <remarks>
        /// <para>A concatenated message makes it possible to exceed the maximum length of a normal message,
        /// created by splitting the message data into multiple parts.</para>
        /// <para>Concatenated messages are also known as long or multi-part messages.</para>
        /// <para>If no concatenation is necessary, a single, non-concatenated <see cref="T:GsmComm.PduConverter.SmsSubmitPdu" /> object is created.</para>
        /// </remarks>
        /// <exception cref="T:System.ArgumentException"><para>userDataText is so long that it would create more than 255 message parts.</para></exception>
        public static SmsSubmitPdu[] CreateConcatTextMessage(string userDataText, bool unicode, string destinationAddress)
        {
            string str;
            int length = 0;
            int num;
            byte[] bytes;
            SmsSubmitPdu smsSubmitPdu;
            int num1;
            byte num2;
            if (unicode)
            {
                num1 = 70;
            }
            else
            {
                num1 = 160;
            }
            int num3 = num1;
            if (unicode)
            {
                str = userDataText;
            }
            else
            {
                str = TextDataConverter.StringTo7Bit(userDataText);
            }
            if (str.Length <= num3)
            {
                if (unicode)
                {
                    smsSubmitPdu = new SmsSubmitPdu(userDataText, destinationAddress, 8);
                }
                else
                {
                    smsSubmitPdu = new SmsSubmitPdu(userDataText, destinationAddress);
                }
                SmsSubmitPdu[] smsSubmitPduArray = new SmsSubmitPdu[1];
                smsSubmitPduArray[0] = smsSubmitPdu;
                return smsSubmitPduArray;
            }
            else
            {
                ConcatMessageElement16 concatMessageElement16 = new ConcatMessageElement16(0, 0, 0);
                byte length1 = (byte)((int)SmartMessageFactory.CreateUserDataHeader(concatMessageElement16).Length);
                byte num4 = (byte)((double)length1 / 7 * 8);
                if (unicode)
                {
                    num2 = length1;
                }
                else
                {
                    num2 = num4;
                }
                byte num5 = num2;
                StringCollection stringCollections = new StringCollection();
                for (int i = 0; i < str.Length; i = i + length)
                {
                    if (!unicode)
                    {
                        if (str.Length - i < num3 - num5)
                        {
                            length = str.Length - i;
                        }
                        else
                        {
                            length = num3 - num5;
                        }
                    }
                    else
                    {
                        if (str.Length - i < (num3 * 2 - num5) / 2)
                        {
                            length = str.Length - i;
                        }
                        else
                        {
                            length = (num3 * 2 - num5) / 2;
                        }
                    }
                    string str1 = str.Substring(i, length);
                    stringCollections.Add(str1);
                }
                if (stringCollections.Count <= 255)
                {
                    SmsSubmitPdu[] smsSubmitPduArray1 = new SmsSubmitPdu[stringCollections.Count];
                    ushort num6 = CalcNextRefNumber();
                    byte num7 = 0;
                    for (int j = 0; j < stringCollections.Count; j++)
                    {
                        num7 = (byte)(num7 + 1);
                        ConcatMessageElement16 concatMessageElement161 = new ConcatMessageElement16(num6, (byte)stringCollections.Count, num7);
                        byte[] numArray = SmartMessageFactory.CreateUserDataHeader(concatMessageElement161);
                        if (unicode)
                        {
                            Encoding bigEndianUnicode = Encoding.BigEndianUnicode;
                            bytes = bigEndianUnicode.GetBytes(stringCollections[j]);
                            num = (int)bytes.Length;
                        }
                        else
                        {
                            bytes = TextDataConverter.SeptetsToOctetsInt(stringCollections[j]);
                            num = stringCollections[j].Length;
                        }
                        SmsSubmitPdu smsSubmitPdu1 = new SmsSubmitPdu();
                        smsSubmitPdu1.DestinationAddress = destinationAddress;
                        if (unicode)
                        {
                            smsSubmitPdu1.DataCodingScheme = 8;
                        }
                        smsSubmitPdu1.SetUserData(bytes, (byte)num);
                        smsSubmitPdu1.AddUserDataHeader(numArray);
                        smsSubmitPduArray1[j] = smsSubmitPdu1;
                    }
                    return smsSubmitPduArray1;
                }
                else
                {
                    throw new ArgumentException("A concatenated message must not have more than 255 parts.", "userDataText");
                }
            }
        }

        protected static ushort CalcNextRefNumber()
        {
            ushort num;
            lock (typeof(SmartMessageFactory))
            {
                num = refNumber;
                if (refNumber != 65535)
                {
                    refNumber = (ushort)(refNumber + 1);
                }
                else
                {
                    refNumber = 1;
                }
            }
            return num;
        }

    }
}
