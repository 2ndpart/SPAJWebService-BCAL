using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.ServiceModel.Web;

namespace HtmlGeneratorServices
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IService1" in both code and config file together.
    [ServiceContract]
    public interface IService1
    {
        [OperationContract]
        [WebInvoke(Method = "GET", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Bare, UriTemplate = "getAllData")]
        List<wsHtmlForm> GetAllData();
    }

    [DataContract]
    public class wsHtmlForm
    {
        [DataMember]
        public int Id { get; set; }
        [DataMember]
        public int CFFId { get; set; }
        [DataMember]
        public string Filename { get; set; }
        [DataMember]
        public string Status { get; set; }
        [DataMember]
        public string CFFSection { get; set; }
        [DataMember]
        public DateTime ValidDate { get; set; }
        [DataMember]
        public DateTime DateCreated { get; set; }        
    }
}
