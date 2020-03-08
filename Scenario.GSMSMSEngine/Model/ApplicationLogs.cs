using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scenario.GSMSMSEngine.Model
{
    public class ApplicationLog
    {
        public string Level { get; set; }
        public DateTime CreatedDateTime { get; set; }
        public string Source { get; set; }
        public string Message { get; set; }
        public int EventID { get; set; }        
        public string COMPortName { get; set; }
        public string TaskCategory { get; set; }
    }

    public enum EventLevel
    {
        Information,
        Warning,
        Error
    }

    public enum EventSource
    {
        AddModem,
        SelectModem,
        bw_DoWork,
        Initilization,
        Stopping,
        Thread,
        SendMessage,
        Synchronization
    }
}
