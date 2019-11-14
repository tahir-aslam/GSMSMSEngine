using GsmComm.GsmCommunication;
using GsmComm.PduConverter;
using GsmComm.PduConverter.SmartMessaging;
using Scenario.GSMSMSEngine.Model;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace Scenario.GSMSMSEngine
{
    public class SMSEngine
    {
        public static SerialPort port = new SerialPort();
        private BackgroundWorker bw;

        SMSHistory sh;
        string[] messages;
        string a;
        string b;
        int error_no = 0;
        public GsmCommMain comm;
        private static ushort refNumber;

        bool isWholeSent = false;
        int i = 0;

        DispatcherTimer refreshDataTimer;
        DAL.MiscDAL miscDAL;
        public bool m_IsEncoded = false;
        public List<SMSQueue> m_SmsNos = new List<SMSQueue>();
        public int m_TotalSmsSent = 0;

        public SMSEngine()
        {
            miscDAL = new DAL.MiscDAL();

            try
            {
                m_SmsNos = miscDAL.GetSMSQueue();
                StartSMSEngine();
                StartDataTimer();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        #region Send SMS

        static AutoResetEvent readNow = new AutoResetEvent(false);

        public void StartDataTimer()
        {
            refreshDataTimer = new DispatcherTimer();
            refreshDataTimer.Interval = new TimeSpan(0, 0, 5);
            refreshDataTimer.Tick += refreshDataTimer_Tick;
            refreshDataTimer.Start();
        }

        void refreshDataTimer_Tick(object sender, EventArgs e)
        {
            //m_SmsNos = miscDAL.GetSMSQueue().Except(m_SmsNos).ToList();
        }
        public void StartSMSEngine()
        {
            try
            {
                foreach (var portName in SerialPort.GetPortNames())
                {
                    if (portName == "COM22")
                    {
                        SerialPort port = new SerialPort(portName);
                        if (port.IsOpen)
                        {

                        }
                        else
                        {
                            //port.Close();
                            port.Dispose();
                        }
                    }
                }

                openPort();
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
        public void openPort()
        {
            try
            {
                //comm = new GsmCommMain("COM22", 115200, 300);
                comm = new GsmCommMain("COM22", 115200, 300);
                comm.Open();
            }
            catch (Exception ex)
            {
                //MessageBox.Show("Opening Port Exception: " + ex.Message);
            }
        }
        public void closePort()
        {
            try
            {
                if (comm.IsOpen())
                {
                    comm.Close();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        private void bw_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;
            try
            {
                do
                {
                    foreach (var smsQueue in m_SmsNos)
                    {

                        Thread.Sleep(500);

                        if (sendMsg(smsQueue, worker, e))
                        {                            
                            //MessageBox.Show("Message has sent successfully", "Successfully Sent", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        else
                        {
                            //MessageBox.Show("Failed to send message", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }

                    }
                    m_SmsNos = miscDAL.GetSMSQueue();
                    Thread.Sleep(1000);                    
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
        public bool sendMsg(SMSQueue obj, BackgroundWorker worker, DoWorkEventArgs e)
        {
            bool isSend = false;
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
                    if (comm.IsConnected())
                    {
                        comm.SendMessage(pdu[j]);
                        isSend = true;
                        m_TotalSmsSent++;
                        if (j + 1 == pdu.Length)
                        {
                            isWholeSent = true;
                            obj.sms_status = "Sent";
                        }
                    }
                    else
                    {
                        j--;
                        // MessageBox.Show("IsConnected=false", "Error", MessageBoxButton.OK, MessageBoxImage.Stop);
                        Thread.Sleep(1000);
                        openPort();
                        isSend = false;
                        obj.sms_status = "Not Sent";
                        Thread.Sleep(1000);

                        //j--;
                    }
                }
                catch (Exception ex)
                {
                    if (ex.Message == "Message service error 50 occurred.")
                    {
                    }
                    else 
                    {
                    }                    
                    isSend = false;
                    obj.sms_status = "Not Sent";
                }

            }
            // saved to sms history table whether sent or not

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

            if (miscDAL.InsertSMSHistory(sh) > 0)
            {
                if (miscDAL.UpdateSMSQueue(obj) > 0)
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


            return isSend;
        }
        public void setConnected()
        {
            string cmbCOM = "COM22";
            if (cmbCOM == "")
            {
                MessageBox.Show("Invalid Port Name");
                return;
            }
            comm = new GsmCommMain(cmbCOM, 115200, 150);
            //Cursor.Current = Cursors.Default;

            bool retry;
            do
            {
                retry = false;
                try
                {
                    //Cursor.Current = Cursors.WaitCursor;
                    comm.Open();
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
