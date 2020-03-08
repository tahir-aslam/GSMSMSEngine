using GsmComm.GsmCommunication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scenario.GSMSMSEngine.Model
{
   public class Modem
    {
        public GsmCommMain GsmCommMain { get; set; }

        public bool IsFree { get; set; }        
        public int TotalSmsSent { get; set; }
        public DateTime StartTime { get; set; } 
    }


}
