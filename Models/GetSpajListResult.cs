using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HtmlGeneratorServices.Models
{
    public class GetSpajListResult
    {
        private List<string> spajList;
        public String packCode { set; get; }

        public List<string> SpajList
        {
            get { return spajList; }
            set { spajList = value; }
        }
    }
}