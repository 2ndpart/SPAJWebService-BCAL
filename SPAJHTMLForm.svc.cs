using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;
using System.Text;
using HtmlGeneratorServices.Models;
using PetaPoco;
using System.IO;

namespace HtmlGeneratorServices
{
    [ServiceContract(Namespace = "")]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    public class SPAJHTMLForm
    {
        [OperationContract]
        [WebInvoke(Method = "GET",
            BodyStyle = WebMessageBodyStyle.WrappedRequest,
            ResponseFormat = WebMessageFormat.Json
         )]

        public List<SPAJHTMLFormModel> GetAllData()
        {
            try
            {
                List<SPAJHTMLFormModel> myList = new List<SPAJHTMLFormModel>();

                Database myDB = new Database("ConnStringProd");

                string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Path.Combine("SQLFiles", "SelectValidSPAJHTMLFiles.txt"));

                string query = File.ReadAllText(path);

                myList = myDB.Query<SPAJHTMLFormModel>(query).ToList();

                List<SPAJHTMLFormModel> results = new List<SPAJHTMLFormModel>();

                foreach (var cust in myList)
                {
                    results.Add(new SPAJHTMLFormModel()
                    {
                        Id = cust.Id,
                        SPAJId = cust.SPAJId,
                        FolderName = cust.FolderName,
                        FileName = cust.FileName,
                        FileNameIndo = cust.FileNameIndo,
                        //Base64File = cust.Base64File,
                        Status = cust.Status,
                        SPAJSection = cust.SPAJSection,
                        ValidDate = cust.ValidDate,
                        DateCreated = cust.DateCreated
                    });
                }
                return results;
            }
            catch (Exception ex)
            {
                //  Return any exception messages back to the Response header
                OutgoingWebResponseContext response = WebOperationContext.Current.OutgoingResponse;
                response.StatusCode = System.Net.HttpStatusCode.InternalServerError;
                response.StatusDescription = ex.Message.Replace("\r\n", "");
                return null;
            }
        }

        [OperationContract]
        [WebInvoke(Method = "GET",
            BodyStyle = WebMessageBodyStyle.WrappedRequest,
            ResponseFormat = WebMessageFormat.Json
         )]
        public SPAJHTML_Form GetHtmlFile(string fileName)
        {
            try
            {
                byte[] file;

                string base64 = "";

                Database myDB = new Database("ConnStringProd");

                string pathForSQLFiles = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Path.Combine("SQLFiles", "SelectValidSPAJHtmlFileByFileName.txt"));

                string query = File.ReadAllText(pathForSQLFiles);

                var selectResult = myDB.Query<SPAJHTML_Form>(query, new { FileName = fileName }).SingleOrDefault();

                string pathForHTMLFiles = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Path.Combine("HTMLFiles", selectResult.FolderName, fileName));

                //using (var stream = new FileStream("D:\\" + selectResult.FolderName + "\\" + fileName + "", FileMode.Open, FileAccess.Read))
                //{
                //    using (var reader = new BinaryReader(stream))
                //    {
                //        file = reader.ReadBytes((int)stream.Length);
                //    }
                //}

                using (var stream = new FileStream(pathForHTMLFiles, FileMode.Open, FileAccess.Read))
                {
                    using (var reader = new BinaryReader(stream))
                    {
                        file = reader.ReadBytes((int)stream.Length);
                    }
                }

                base64 = Convert.ToBase64String(file);

                string pathForSQLFiles2 = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Path.Combine("SQLFiles", "UpdateSPAJHTMLFile.txt"));

                string query2 = File.ReadAllText(pathForSQLFiles2);

                myDB.Update<SPAJHTML_Form>(query2, new { Base64File = base64, Filename = fileName });

                return new SPAJHTML_Form()
                {
                    FolderName = selectResult.FolderName,
                    FileName = fileName,                    
                    Base64File = base64
                };
            }
            catch (Exception ex)
            {
                //  Return any exception messages back to the Response header
                OutgoingWebResponseContext response = WebOperationContext.Current.OutgoingResponse;
                response.StatusCode = System.Net.HttpStatusCode.InternalServerError;
                response.StatusDescription = ex.Message.Replace("\r\n", "");
                return null;
                throw;
            }
        }

        public class SPAJHTML_Form
        {
            public string FolderName { get; set; }            
            public string FileName { get; set; }
            public string FileNameIndo { get; set; }
            public string Base64File { get; set; }
        }
    }
}
