using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;
using System.Text;
using PetaPoco;
using System.IO;
using E_Submission.Models;
using SPAJ.Models;
using System.Web.Script.Serialization;
using HtmlGeneratorServices.Models;
using System.Net;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using HtmlGeneratorServices.ServicesHelper;

namespace HtmlGeneratorServices
{
    [ServiceContract(Namespace = "")]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    public class Service2
    {
        //Variable Declaration
        int basePackCode = 10000;
        long baseSPAJCode = 60000000000;
        int baseAllocatedNumber = 50;
        string basicUpdateCommand = "UPDATE TBM_SPAJ_NUMBER ";
        string basicInsertCommand = "INSERT INTO TBM_SPAJ_NUMBER (SPAJCode, PACKCode, Status)";

        // Add more operations here and mark them with [OperationContract]
        [OperationContract]
        [WebInvoke(Method = "GET",
            BodyStyle = WebMessageBodyStyle.WrappedRequest,
            ResponseFormat = WebMessageFormat.Json
         )]

        public List<HtmlForm> getAllData()
        {
            try
            {
                List<HtmlForm> myList = new List<HtmlForm>();

                Database myDB = new Database("ConnStringDev");

                string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Path.Combine("SQLFiles", "SelectValidHtmlFiles.txt"));

                string query = File.ReadAllText(path);

                myList = myDB.Query<HtmlForm>(query).ToList();

                List<HtmlForm> results = new List<HtmlForm>();

                foreach (var cust in myList)
                {
                    results.Add(new HtmlForm()
                    {
                        Id = cust.Id,
                        CFFId = cust.CFFId,
                        FileNameIndo = cust.FileNameIndo,
                        FolderName = cust.FolderName,
                        FileName = cust.FileName,
                        Base64File = cust.Base64File,
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
                string message = ex.ToString();
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
        public List<ProductInformationModel> getProductInformationData()
        {
            string[] files = Directory.GetFiles("C:\\inetpub\\wwwroot\\ProductInformation");
            List<ProductInformationModel> returnVal = new List<ProductInformationModel>(); 
            foreach (string fileName in files)
            {
                ProductInformationModel model = new ProductInformationModel();
                FileInfo information = new FileInfo(fileName);
                model.DateModified = information.CreationTime.ToShortDateString();
                string[] splittedPath = information.FullName.Split('\\');
                model.FilePath = splittedPath[splittedPath.Length - 2] + "/" + splittedPath[splittedPath.Length - 1];
                model.FileSize = Convert.ToString(information.Length / 8);
                if (information.Extension == ".pdf")
                {
                    model.FileType = "Brosur";
                }
                else if (information.Extension == ".mp4")
                {
                    model.FileType = "Video";
                }
                returnVal.Add(model);
            }
            return returnVal;
        }

        [OperationContract]
        [WebInvoke(Method = "GET",
            BodyStyle = WebMessageBodyStyle.WrappedRequest,
            ResponseFormat = WebMessageFormat.Json
         )]
        public FtpCreationResult CreateBackupFtpFolder()
        {
            var remotePathName = Guid.NewGuid().ToString();
            string backupFolder = ConfigurationManager.AppSettings["ftpBackupFolder"];
            string[] folders = FTPHelper.GetFileList(backupFolder);
            FtpCreationResult result = new FtpCreationResult();
            if (folders != null)
            {
                if (folders.Length != null)
                {
                    if (folders.Contains(remotePathName))
                    {
                        result.FtpRemoteFolder = remotePathName;
                    }
                    else
                    {
                        remotePathName = FTPHelper.CreateBackupFolder(remotePathName);
                        result.FtpRemoteFolder = remotePathName;
                    }
                }
            }
            else
            {
                remotePathName = FTPHelper.CreateBackupFolder(remotePathName);
                result.FtpRemoteFolder = remotePathName;
            }
            return result;
        }

        [OperationContract]
        [WebInvoke(Method = "GET",
            BodyStyle = WebMessageBodyStyle.WrappedRequest,
            ResponseFormat = WebMessageFormat.Json
         )]
        public String UpdateOnPostUploadBackupFile(string agentCode, string backupFolder)
        {
            string updateStatement = "UPDATE Agent_Profile SET BackupFolder =@download, LastBackupUploaded=@uploadDate WHERE AgentCode='" + agentCode + "'";

            SqlConnection conn = E_Submission.ESubmissionHelper.Database.GetSqlConnection();

            if (conn.State == ConnectionState.Closed)
                conn.Open();

            SqlCommand command = new SqlCommand(updateStatement, conn);
            command.Parameters.AddWithValue("@download", backupFolder);
            command.Parameters.AddWithValue("@uploadDate", DateTime.Now);
            int updatedRowCount = command.ExecuteNonQuery();
            conn.Close();

            if (updatedRowCount >= 1)
                return "File Saved";
            else
                return "Failed updating record";
        }

        [OperationContract]
        [WebInvoke(Method = "GET",
            BodyStyle = WebMessageBodyStyle.WrappedRequest,
            ResponseFormat = WebMessageFormat.Json
         )]
        public string testConnString()
        {
            string connString = ConfigurationManager.ConnectionStrings["ConnStringProd"].ConnectionString;

            SqlConnection conn = new SqlConnection(connString);
            conn.Open();
            return connString + " " + conn.State;
        }


        [OperationContract]
        [WebInvoke(Method = "GET",
            BodyStyle = WebMessageBodyStyle.WrappedRequest,
            ResponseFormat = WebMessageFormat.Json
         )]
        public String GetBackupDownloadLink(string agentCode, string agentPassword)
        {
            string selectStatement = "SELECT BackupFolder FROM Agent_Profile WHERE AgentCode='" + agentCode + "' AND AgentPassword='" + agentPassword + "'";
            SqlConnection conn = E_Submission.ESubmissionHelper.Database.GetSqlConnection();
            string backupFolder = string.Empty;
            if (conn.State == ConnectionState.Closed)
                conn.Open();

            SqlCommand command = new SqlCommand(selectStatement, conn);
            try
            {
                backupFolder = command.ExecuteScalar().ToString();
            }
            catch (Exception ex)
            {
                backupFolder = "No Download Folder Found";
            }
            finally
            {
                conn.Close();
            }
            return backupFolder;
        }

        [OperationContract]
        [WebInvoke(Method = "GET",
            BodyStyle = WebMessageBodyStyle.WrappedRequest,
            ResponseFormat = WebMessageFormat.Json
         )]
        public String UpdateOnPostDownload(string agentCode)
        {
            string updateStatement = "UPDATE Agent_Profile SET LastBackupDownloaded = @date WHERE AgentCode=@code";
            SqlConnection conn = E_Submission.ESubmissionHelper.Database.GetSqlConnection();

            if (conn.State == ConnectionState.Closed)
                conn.Open();
            SqlCommand command = new SqlCommand(updateStatement, conn);
            command.Parameters.AddWithValue("@date", DateTime.Now);
            command.Parameters.AddWithValue("@code", agentCode);
            try
            {
                command.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                return "Error Occured";
            }
            finally
            {
                conn.Close();
            }
            
            return "Data Upadted";
        }

        public HtmlForm2 GetHtmlFile(string fileName)
        {
            try
            {
                byte[] file;

                string base64 = "";

                Database myDB = new Database("ConnStringProd");

                string pathForSQLFiles = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Path.Combine("SQLFiles", "SelectValidHtmlFileByFileName.txt"));

                string query = File.ReadAllText(pathForSQLFiles);

                var selectResult = myDB.Query<HtmlForm>(query, new { FileName = fileName }).SingleOrDefault();

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

                string pathForSQLFiles2 = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Path.Combine("SQLFiles", "UpdateHtmlFiles.txt"));

                string query2 = File.ReadAllText(pathForSQLFiles2);

                myDB.Update<HtmlForm>(query2, new { Base64File = base64, Filename = fileName });

                return new HtmlForm2()
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


        [OperationContract]
        [WebInvoke(Method = "GET",
            BodyStyle = WebMessageBodyStyle.WrappedRequest,
            ResponseFormat = WebMessageFormat.Json
         )]
        public String[] TestFunction(string oldVal, string newVal)
        {
            WebServiceWorker.SendFile("");
            Dictionary<Object, Dictionary<Object, Object>> myVar = new Dictionary<Object, Dictionary<Object, Object>>();
            Dictionary<Object, Object> values = new Dictionary<Object, Object>();
            values.Add("key1", "value1");
            values.Add("key2", Directory.GetCurrentDirectory());

            myVar.Add("TableName", values);

            TestResult result = new TestResult();
            result.Result = myVar;

            String appDomain = AppDomain.CurrentDomain.BaseDirectory;
            String rootFolder = appDomain.Split('\\')[0];
            String [] folders = Directory.GetDirectories("\\home\\Staging\\");

            //return (new String[] { AppDomain.CurrentDomain.BaseDirectory });
            return folders;
        }

        [OperationContract]
        [WebInvoke(Method = "GET",
            BodyStyle = WebMessageBodyStyle.WrappedRequest,
            ResponseFormat = WebMessageFormat.Json
         )]
        public String UpdateOnResubmitData(string spajNumber, string producName, string polisOwner, bool isHttpPost)
        {
            //email
            //string emailReceiver = System.Configuration.ConfigurationManager.AppSettings["primaryEmailReceip"];
            string emailReceiver = System.Configuration.ConfigurationManager.AppSettings["primaryEmailReceip"];
            //var db = E_Submission.ESubmissionHelper.Database.GetPetaPocoDB();
            try
            {
                string updateStatement = "UPDATE TBM_SPAJ_NUMBER SET Status = 'Submitted', SubmittedDate=@submittedDate, ProductName='" + producName + "', PolisOwner = '" + polisOwner + "' WHERE SPAJCode =" + spajNumber;
                SqlConnection conn = E_Submission.ESubmissionHelper.Database.GetSqlConnection();
                conn.Open();
                SqlCommand command = new SqlCommand(updateStatement, conn);
                command.Parameters.AddWithValue("@submittedDate", DateTime.Now);
                command.ExecuteNonQuery();
                conn.Close();

                //used to sent the attachment file to specified email
                String appDomain = AppDomain.CurrentDomain.BaseDirectory;
                String rootFolder = appDomain.Split('\\')[1];
                //String[] files = Directory.GetFiles(rootFolder + "\\Staging\\" + spajNumber);
                //String[] files = Directory.GetFiles("\\Home\\Staging\\" + spajNumber);
                MailSender.SendResubmitMail (emailReceiver, spajNumber, isHttpPost);

                return "File Saved";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        [OperationContract]
        [WebInvoke(Method = "GET",
            BodyStyle = WebMessageBodyStyle.WrappedRequest,
            ResponseFormat = WebMessageFormat.Json
         )]
        public String UpdateOnPostUploadData(string spajNumber, string producName, string polisOwner, bool isHttpPost)
        {
            //email
            //string emailReceiver = System.Configuration.ConfigurationManager.AppSettings["primaryEmailReceip"];
            string emailReceiver = System.Configuration.ConfigurationManager.AppSettings["primaryEmailReceip"];
            //var db = E_Submission.ESubmissionHelper.Database.GetPetaPocoDB();
            try
            {
                string updateStatement = "UPDATE TBM_SPAJ_NUMBER SET Status = 'Submitted', SubmittedDate=@submittedDate, ProductName='" + producName + "', PolisOwner = '" + polisOwner + "'"
                                         + " WHERE SPAJCode =" + spajNumber;
                SqlConnection conn = E_Submission.ESubmissionHelper.Database.GetSqlConnection();
                conn.Open();
                SqlCommand command = new SqlCommand(updateStatement, conn);
                command.Parameters.AddWithValue("@submittedDate", DateTime.Now);
                command.ExecuteNonQuery();
                conn.Close();

                //used to sent the attachment file to specified email
                String appDomain = AppDomain.CurrentDomain.BaseDirectory;
                String rootFolder = appDomain.Split('\\')[1];
                //String[] files = Directory.GetFiles(rootFolder + "\\Staging\\" + spajNumber);
                //String[] files = Directory.GetFiles("\\Home\\Staging\\" + spajNumber);
                MailSender.SendMail(emailReceiver, spajNumber, isHttpPost);
                             
                return "File Saved";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        [OperationContract]
        [WebInvoke(Method = "GET",
            BodyStyle = WebMessageBodyStyle.WrappedRequest,
            ResponseFormat = WebMessageFormat.Json
         )]
        public String ResetRecord(int start)
        {
            var db = E_Submission.ESubmissionHelper.Database.GetPetaPocoDB();
            string updateQuery = "DELETE FROM TBM_SPAJ_NUMBER WHERE TableKey > " + start;
            db.Execute(updateQuery, null);
            return "sucess";
        }

        [OperationContract]
        [WebInvoke(Method = "GET",
            BodyStyle = WebMessageBodyStyle.WrappedRequest,
            ResponseFormat = WebMessageFormat.Json
         )]

        public List<Agent_PackMappingResult> GetAgent_PackMapping(string agentCode)
        {
            var db = E_Submission.ESubmissionHelper.Database.GetPetaPocoDB();
            string connectionString = ConfigurationManager.ConnectionStrings["ConnStringProd"].ConnectionString;
            SqlConnection petaPocoConnection = new SqlConnection(connectionString);
            SqlDataAdapter adapter = new SqlDataAdapter("SELECT PACKCode, AssignDate FROM TBC_SPAJ_NUMBER_AGENT WHERE AgentCode=" + agentCode, petaPocoConnection);
            DataTable packCodeResult = new DataTable();
            adapter.Fill(packCodeResult);
            List<Agent_PackMappingResult> jsonObject = new List<Agent_PackMappingResult>();

            foreach (DataRow dr in packCodeResult.Rows)
            {
                Agent_PackMappingResult dataElement = new Agent_PackMappingResult();
                dataElement.PackCode = dr["PACKCode"].ToString();
                dataElement.AssignDate = (DateTime) dr["AssignDate"];

                SqlDataAdapter innerAdapter = new SqlDataAdapter("SELECT SpajCode FROM TBM_SPAJ_NUMBER WHERE PACKCode=" + dataElement.PackCode, petaPocoConnection);
                DataTable innerDataTable = new DataTable();
                innerAdapter.Fill(innerDataTable);

                dataElement.SPAJBeginAllocation = innerDataTable.Rows[0]["SpajCode"].ToString();
                dataElement.SPAJEndAllocation = innerDataTable.Rows[innerDataTable.Rows.Count - 1]["SpajCode"].ToString();

                jsonObject.Add(dataElement);
            }
            return jsonObject;
        }

        [OperationContract]
        [WebInvoke(Method = "GET",
            BodyStyle = WebMessageBodyStyle.WrappedRequest,
            ResponseFormat = WebMessageFormat.Json
         )]
        public AllocateSPAJResult AllocateSpajForAgent(string agentCode)
        {
            string resultMessage = string.Empty;
            // Check for last PACK Code stored in table TBM_SPAJ_NUMBER
            var db = E_Submission.ESubmissionHelper.Database.GetPetaPocoDB();
            List<SPAJMaster> result = new List<SPAJMaster>();
            string query = "SELECT DISTINCT(PACKCode) FROM TBM_SPAJ_NUMBER ORDER BY PACKCode DESC";
            SPAJMaster spajTableModel = db.Query<SPAJMaster>(query).First();
            long lastAllocSpaj = 0;
            AllocateSPAJResult allocateResult = new AllocateSPAJResult();
            try
            {
                if (spajTableModel.PACKCode == "0")
                {
                    //new record. This only occured when there is no spaj allocated at all.
                    //generate PACK Code
                    int newPackCode = basePackCode + 1;
                    allocateResult.SpajAllocationBegin = Convert.ToString((baseSPAJCode + 1));
                    for (int i = 1; i <= baseAllocatedNumber; i++)
                    {
                        long key = baseSPAJCode + i;
                        //string completeUpdateStatement = basicUpdateCommand + "SET PACKCode =" + newPackCode + " WHERE SPAJCode =" + key;
                        string completeInsertStatement = basicInsertCommand + " VALUES (" + key + "," + newPackCode + ",null)";
                        db.Execute(completeInsertStatement);
                        lastAllocSpaj = key;
                    }
                    allocateResult.PACKID = Convert.ToString(newPackCode);
                    allocateResult.SpajAllocationEnd = Convert.ToString(lastAllocSpaj);
                    //save mapping information into cross table
                    SpajNumber_Agent crossModels = new SpajNumber_Agent();
                    crossModels.PACKCode = newPackCode.ToString();
                    crossModels.AgentCode = agentCode.ToString();
                    crossModels.TableKey = Guid.NewGuid();
                    crossModels.AssignDate = DateTime.Now;
                    db.Insert("TBC_SPAJ_NUMBER_AGENT", crossModels);
                    resultMessage = "spaj allocated";
                }
                else
                {
                    //get last allocated PACK Code from DB
                    int lastStoredPackCode = Convert.ToInt32(spajTableModel.PACKCode);
                    int newPackCode = lastStoredPackCode + 1;

                    //get last allocated SPAJCode on current PACK
                    string spajQuery = "SELECT SPAJCode FROM TBM_SPAJ_NUMBER WHERE PACKCode =" + lastStoredPackCode + " ORDER BY SPAJCode DESC";
                    SPAJMaster queryResult = db.Query<SPAJMaster>(spajQuery).First();

                    long lastAllocSPAJNumber = Convert.ToInt64(queryResult.SPAJCode);

                    //check for agent code spajcredit count, if below 30, allocate another 50. If dont, dont allocate
                    string packQuery = "SELECT PACKCode FROM TBC_SPAJ_NUMBER_AGENT WHERE AgentCode=" + agentCode + " ORDER BY AssignDate ";
                    if (!db.Query<SpajNumber_Agent>(packQuery).Any())
                    {
                        allocateResult.SpajAllocationBegin = Convert.ToString(lastAllocSPAJNumber + 1);
                        //allocate data for new agent
                        for (int i = 1; i <= baseAllocatedNumber; i++)
                        {
                            long key = lastAllocSPAJNumber + i;
                            //string completeUpdateStatement = basicUpdateCommand + "SET PACKCode =" + newPackCode + ",Status='Allocated' WHERE SPAJCode =" + key;
                            string completeInsertStatement = basicInsertCommand + " VALUES (" + key + "," + newPackCode + ",'Allocated')";
                            db.Execute(completeInsertStatement);
                            lastAllocSpaj = key;
                        }
                        allocateResult.PACKID = Convert.ToString(newPackCode);
                        //save mapping information into cross table
                        SpajNumber_Agent crossModels = new SpajNumber_Agent();
                        crossModels.PACKCode = newPackCode.ToString();
                        crossModels.AgentCode = agentCode.ToString();
                        crossModels.TableKey = Guid.NewGuid();
                        crossModels.AssignDate = DateTime.Now;
                        db.Insert("TBC_SPAJ_NUMBER_AGENT", crossModels);
                        allocateResult.SpajAllocationEnd = Convert.ToString(lastAllocSpaj);
                    }
                    else
                    {
                        //allocate for existing agent
                        SpajNumber_Agent packQueryResult = db.Query<SpajNumber_Agent>(packQuery).First();
                        string packCode = packQueryResult.PACKCode;

                        string spajCountQuery = "SELECT COUNT (SPAJCode) FROM TBM_SPAJ_NUMBER WHERE PACKCode = " + packCode + " AND Status <> 'Submitted'";
                        int spajCountQueryResult = db.ExecuteScalar<int>(spajCountQuery);
                        allocateResult.SpajAllocationBegin = Convert.ToString(lastAllocSPAJNumber + 1);


                        for (int i = 1; i <= baseAllocatedNumber; i++)
                        {
                            long key = lastAllocSPAJNumber + i;
                            //string completeUpdateStatement = basicUpdateCommand + "SET PACKCode =" + newPackCode + ",Status='Allocated' WHERE SPAJCode =" + key;
                            string completeInsertStatement = basicInsertCommand + " VALUES (" + key + "," + newPackCode + ",'Allocated')";
                            db.Execute(completeInsertStatement);
                            lastAllocSpaj = key;
                        }
                        allocateResult.PACKID = Convert.ToString(newPackCode);
                        //save mapping information into cross table
                        SpajNumber_Agent crossModels = new SpajNumber_Agent();
                        crossModels.PACKCode = newPackCode.ToString();
                        crossModels.AgentCode = agentCode.ToString();
                        crossModels.TableKey = Guid.NewGuid();
                        crossModels.AssignDate = DateTime.Now;
                        db.Insert("TBC_SPAJ_NUMBER_AGENT", crossModels);
                        allocateResult.SpajAllocationEnd = Convert.ToString(lastAllocSpaj);
                    }
                }
            }
            catch (Exception ex)
            {
                resultMessage = ex.Message;
            }
            return allocateResult;
        }

        // Add more operations here and mark them with [OperationContract]

        [OperationContract]
        [WebInvoke(Method = "GET",
            BodyStyle = WebMessageBodyStyle.WrappedRequest,
            ResponseFormat = WebMessageFormat.Json
         )]
        public SPAJQueryResult GetAllocatedSpaj(int agentCode)
        {
            try
            {
                var db = E_Submission.ESubmissionHelper.Database.GetPetaPocoDB();
                SPAJQueryResult result = new SPAJQueryResult();
                //retreive packcode from DB which belong to this agent
                string packCodeQuery = "SELECT PACKCode FROM TBC_SPAJ_NUMBER_AGENT WHERE AgentCode =" + agentCode;
                SpajNumber_Agent packQueryResult = db.Query<SpajNumber_Agent>(packCodeQuery).First();

                //add result into model class
                result.PACKCode = packQueryResult.PACKCode;

                string spajCodeQuery = "SELECT SPAJCode FROM TBM_SPAJ_NUMBER WHERE PACKCode =" + packQueryResult.PACKCode;

                IEnumerable<SPAJMaster> temp = db.Query<SPAJMaster>(spajCodeQuery);

                result.SpajCodeStart = temp.First().SPAJCode;
                result.SpajCodeEnd = temp.Last().SPAJCode;

                return result;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        [OperationContract]
        [WebInvoke(Method = "GET",
            BodyStyle = WebMessageBodyStyle.WrappedRequest,
            ResponseFormat = WebMessageFormat.Json
         )]
        public GetPackCodeListResult GetPackList(int agentCode)
        {
            try
            {
                var db = E_Submission.ESubmissionHelper.Database.GetPetaPocoDB();

                string responseResult = string.Empty;
                GetPackCodeListResult queryResult = new GetPackCodeListResult();

                List<string> tempPackCode = new List<string>();
                List<DateTime> tempAssignDate = new List<DateTime>();
                try
                {
                    string packCodeQuery = "SELECT AgentCode, PACKCode, AssignDate FROM TBC_SPAJ_NUMBER_AGENT WHERE AgentCode =" + agentCode;
                    foreach (SpajNumber_Agent packQueryResult in db.Query<SpajNumber_Agent>(packCodeQuery))
                    {
                        queryResult.agentCode = packQueryResult.AgentCode;
                        tempPackCode.Add(packQueryResult.PACKCode);
                        tempAssignDate.Add(packQueryResult.AssignDate);
                    }
                    queryResult.AssignDate = tempAssignDate;
                    queryResult.PackCodeList = tempPackCode;

                    return queryResult;
                }
                catch (Exception ex)
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        [OperationContract]
        [WebInvoke(Method = "GET",
            BodyStyle = WebMessageBodyStyle.WrappedRequest,
            ResponseFormat = WebMessageFormat.Json
         )]
        public GetSpajListResult GetSpajList(int packcode)
        {
            try
            {
                List<string> tempResult = new List<string>();
                GetSpajListResult modelResult = new GetSpajListResult();
                try
                {
                    if (packcode > 0)
                    {
                        var db = E_Submission.ESubmissionHelper.Database.GetPetaPocoDB();
                        string packCodeQuery = "SELECT SPAJCode, PACKCode, Status FROM TBM_SPAJ_NUMBER WHERE PACKCode =" + packcode;
                        foreach (SPAJMaster packQueryResult in db.Query<SPAJMaster>(packCodeQuery))
                        {
                            modelResult.packCode = packQueryResult.PACKCode;
                            tempResult.Add(packQueryResult.SPAJCode);
                            //queryResult.Add(packQueryResult.SPAJCode);
                        }
                        modelResult.SpajList = tempResult;
                    }
                    return modelResult;
                }
                catch (Exception ex)
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        [OperationContract]
        [WebInvoke(Method = "GET",
            BodyStyle = WebMessageBodyStyle.WrappedRequest,
            ResponseFormat = WebMessageFormat.Json
         )]
        public FtpCreationResult CreateRemoteFtpFolder(string spajNumber, Boolean isHttpPost)
        {
            FtpCreationResult result = new FtpCreationResult();
            if (isHttpPost)
            {
                try
                {
                    DirectoryInfo info = Directory.CreateDirectory(@"C:\inetpub\wwwroot\SPAJFiles\" + spajNumber);
                    if (info.Exists)
                    {
                        result.FtpRemoteFolder = spajNumber;
                    }
                    else
                    {
                        result.FtpRemoteFolder = "Unable to create Folder";
                    } 
                }
                catch (Exception ex)
                {
                    result.FtpRemoteFolder = ex.Message;
                }
            }
            else
            {
                var remotePathName = spajNumber;
                string[] folders = FTPHelper.GetFileList("Staging");
                
                if (folders != null)
                {
                    if (folders.Length != null)
                    {
                        if (folders.Contains(spajNumber))
                        {
                            result.FtpRemoteFolder = remotePathName;
                        }
                        else
                        {
                            remotePathName = FTPHelper.CreateFolder(remotePathName);
                            result.FtpRemoteFolder = remotePathName;
                        }
                    }
                }
                else
                {
                    remotePathName = FTPHelper.CreateFolder(remotePathName);
                    result.FtpRemoteFolder = remotePathName;
                }
            }
            return result;
        }

        [OperationContract]
        [WebInvoke(Method = "GET",
            BodyStyle = WebMessageBodyStyle.WrappedRequest,
            ResponseFormat = WebMessageFormat.Json
         )]
        public String ReceiveStringXMLInbound(string XML)
        {
            try
            {
                StringReader reader = new StringReader(XML);
                DataSet dSet = new DataSet();
                dSet.ReadXml(reader);

                return "Data Received";
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return "Problem on receiving string";
            }
            
        }


        public class HtmlForm
        {
            public int Id { get; set; }
            public int CFFId { get; set; }
            public string FolderName { get; set; }
            public string FileNameIndo { get; set; }
            public string FileName { get; set; }
            public string Base64File { get; set; }
            public string Status { get; set; }
            public string CFFSection { get; set; }
            public DateTime ValidDate { get; set; }
            public DateTime DateCreated { get; set; }
        }

        public class HtmlForm2
        {
            //public int Id { get; set; }
            //public int CFFId { get; set; }
            public string FolderName { get; set; }
            public string FileName { get; set; }
            public string Base64File { get; set; }
            //public string Status { get; set; }
            //public string CFFSection { get; set; }
            //public DateTime ValidDate { get; set; }
            //public DateTime DateCreated { get; set; }
        }
    }
}
