using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HtmlGeneratorServices.Models
{
    public class GetPackCodeListResult
    {
        public String agentCode { set; get; }
        private List<string> packCodeList;
        private List<DateTime> assignDate;

        public List<string> PackCodeList
        {
            get { return packCodeList; }
            set { packCodeList = value; }
        }

        public List<DateTime> AssignDate
        {
            get { return assignDate; }
            set { assignDate = value; }
        }
    }
}