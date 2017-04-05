using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using HtmlGeneratorServices.SinosoftWS;
using System.IO;
using System.Data;
using System.Xml.Linq;
using System.Xml;
using System.Text;
namespace HtmlGeneratorServices.ServicesHelper
{
    public class WebServiceWorker
    {
        public static int SendFile(string localFolderFileLocation)
        {
            MPOSServicePortTypeClient wsClient = new MPOSServicePortTypeClient();
          
            XmlDocument basicXML = new XmlDocument();
            basicXML.Load(@"D:\testFiles\BENEFICIARY15.xml");
            StringWriter sw = new StringWriter();
            XmlTextWriter tx = new XmlTextWriter(sw);
            basicXML.WriteTo(tx);

            string xmlString = sw.ToString();// 
            //return str;
            //string xmlString = basicXML.OuterXml;

            byte[] questionare1 = File.ReadAllBytes(@"D:\testFiles\60000000001_AngkatanBersenjata.jpg");
            byte[] images1 = File.ReadAllBytes(@"D:\testFiles\60000000001_ID1.jpg");
            byte[] images2 = File.ReadAllBytes(@"D:\testFiles\60000000001_ID2.jpg");
            byte[] questionare2 = File.ReadAllBytes(@"D:\testFiles\60000000001_Menyelam.jpg");
            byte[] pdf = File.ReadAllBytes(@"D:\testFiles\60000000001_SPAJ.pdf");

            ArrayOfBase64Binary pdfParameter = new ArrayOfBase64Binary();
            ArrayOfBase64Binary supplementary = new ArrayOfBase64Binary();
            ArrayOfBase64Binary images = new ArrayOfBase64Binary();
            ArrayOfBase64Binary questionare = new ArrayOfBase64Binary();


            pdfParameter.Add(pdf);
            images.Add(images1);
            images.Add(images2);
            questionare.Add(questionare1);
            questionare.Add(questionare2);

            string result = wsClient._SPAJToCore(xmlString, pdfParameter, supplementary, images, questionare);

            return 0;
        }

    }
}