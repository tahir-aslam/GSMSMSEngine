using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scenario.GSMSMSEngine.Model
{
    public class SMSQueue
    {
        public int id { get; set; }
        public int receiver_id { get; set; }
        public int receiver_type_id { get; set; }
        public string receiver_cell_no { get; set; }
        public string receiver_name { get; set; }
        public string sms_message { get; set; }
        public string sms_type { get; set; }
        public int sms_type_id { get; set; }
        public int sms_length { get; set; }
        public DateTime? date_time { get; set; }
        public int sort_order { get; set; }
        public string created_by { get; set; }
        public int emp_id { get; set; }
        public string sms_status { get; set; }

        public int class_id { get; set; }
        public string class_name { get; set; }
        public int section_id { get; set; }
        public string section_name { get; set; }

        public string is_sent { get; set; }
        public string is_periority { get; set; }
        public int isEncoded { get; set; }
        public int isSynchronized { get; set; }
        public string sender_cell_no { get; set; }
        public string sender_com_port { get; set; }

        public int institute_id { get; set; }
        public string institute_name { get; set; }
        public string institute_cell { get; set; }
        public DateTime? created_date_time { get; set; }
        public DateTime? downloaded_date_time { get; set; }
        public DateTime? updated_date_time { get; set; }        
    }
}
