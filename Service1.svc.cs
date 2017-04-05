using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using PetaPoco;
using System.ServiceModel.Web;
using System.Reflection;
using System.IO;

namespace HtmlGeneratorServices
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "Service1" in code, svc and config file together.
    public class Service1 : IService1
    {
        public void DoWork()
        {
        }

        public List<wsHtmlForm> GetAllData()
        {
            try
            {
                List<wsHtmlForm> myList = new List<wsHtmlForm>();

                Database myDB = new Database("ConnString");

                string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Path.Combine("SQLFiles", "SelectValidHtmlFiles.txt"));
                                        
                string query = File.ReadAllText(path);                

                myList = myDB.Query<wsHtmlForm>(query).ToList();

                List<wsHtmlForm> results = new List<wsHtmlForm>();

                foreach (var cust in myList)
                {                    
                    results.Add(new wsHtmlForm()
                    {
                        Id = cust.Id,
                        CFFId = cust.CFFId,
                        Filename = cust.Filename,
                        Status = cust.Status,
                        CFFSection = cust.CFFSection,
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
    }
}
