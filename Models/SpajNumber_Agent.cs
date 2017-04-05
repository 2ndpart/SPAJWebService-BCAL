using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SPAJ.Models
{
    public class SpajNumber_Agent
    {
        public Guid TableKey { set; get; }
        public string PACKCode { set; get; }
        public string AgentCode { set; get; }
        public DateTime AssignDate { set; get; }
    }
}