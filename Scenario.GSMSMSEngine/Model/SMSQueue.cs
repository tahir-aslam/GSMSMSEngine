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
        public DateTime date_time { get; set; }
        public int sort_order { get; set; }
        public string created_by { get; set; }
        public int emp_id { get; set; }
        public string sms_status { get; set; }

        public int class_id { get; set; }
        public string class_name { get; set; }
        public int section_id { get; set; }
        public string section_name { get; set; }
    }
}
