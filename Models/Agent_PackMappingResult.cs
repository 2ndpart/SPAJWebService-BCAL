using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HtmlGeneratorServices.Models
{
    public class Agent_PackMappingResult
    {
        public String PackCode { set; get; }
        public DateTime AssignDate { set; get; }
        public String SPAJBeginAllocation { set; get; }
        public String SPAJEndAllocation { set; get; }
    }
}