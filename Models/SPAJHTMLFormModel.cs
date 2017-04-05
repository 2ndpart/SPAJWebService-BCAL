using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HtmlGeneratorServices.Models
{
    public class SPAJHTMLFormModel
    {
        public int Id { get; set; }
        public int SPAJId { get; set; }
        public string FolderName { get; set; }        
        public string FileName { get; set; }
        //public string Base64File { get; set; }
        public string FileNameIndo { get; set; }        
        public string Status { get; set; }
        public string SPAJSection { get; set; }
        public DateTime ValidDate { get; set; }
        public DateTime DateCreated { get; set; }
    }
}