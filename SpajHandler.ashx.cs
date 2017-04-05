using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using E_Submission.ESubmissionHelper;
using System.Web.Script.Serialization;
using System.Text;
using System.Web.Script.Serialization;
using SPAJ.Models;
using E_Submission.Models;
using HtmlGeneratorServices.Models;

namespace E_Submission
{
    /// <summary>
    /// Summary description for SpajHandler
    /// </summary>
    public class SpajHandler : IHttpHandler
    {
        SecurityKey securityKey = new SecurityKey();
        int basePackCode = 10000;
        long baseSPAJCode = 60000000000;
        int baseAllocatedNumber = 50;
        string basicUpdateCommand = "UPDATE TBM_SPAJ_NUMBER ";

        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "text/plain";

            string functionSelector = context.Request["operation"];
            string keyCode = context.Request["key"];

            byte[] encryptedByte = securityKey.EncryptString(Constants.API_PASSWORD);
            string test = Encoding.UTF8.GetString(encryptedByte);

            if (functionSelector == "getRemoteFtpPath")
            {
                string spajCode = context.Request["spajNumber"];
                string product = context.Request["product"];

                var obj = product + "_" + spajCode + "_" + DateTime.Now.ToFileTimeUtc();
                var json = new JavaScriptSerializer().Serialize(obj);
                context.Response.Write(json);
            }
            if (functionSelector == "onPostUpload")
            {
                string spajCode = context.Request["spajNumber"];
                //logic to update SPAJ Code table goes here
                string updateQuery = "UPDATE TBM_SPAJ_NUMBER SET Status='Submitted' WHERE SPAJCode=" + spajCode;
                //logic to update data related with product goes here
                string product = context.Request["product"];
                string fileLocation = context.Request["fileURL"];
                Database.SaveOrUpdate(updateQuery);

                string insertESubmission = "INSERT INTO TBT_SPAJ_ESUBMISSION (SPAJCode,FileLocation,SubmittedDate) VALUES (" + spajCode + ",'" + fileLocation + "','" + DateTime.Now + "')";
                Database.SaveOrUpdate(insertESubmission);
                context.Response.Write("submitted");
            }
            if (functionSelector == "allocateSpaj")
            {
                int agentCode = Convert.ToInt32(context.Request["agentCode"]);
                string resultMessage = string.Empty;
                // Check for last PACK Code stored in table TBM_SPAJ_NUMBER
                var db = Database.GetPetaPocoDB();
                List<SPAJMaster> result = new List<SPAJMaster>();
                string query = "SELECT DISTINCT(PACKCode) FROM TBM_SPAJ_NUMBER ORDER BY PACKCode DESC";
                SPAJMaster spajTableModel = db.Query<SPAJMaster>(query).First();

                try
                {
                    if (spajTableModel.PACKCode == "0")
                    {
                        //new record. This only occured when there is no spaj allocated at all.
                        //generate PACK Code
                        int newPackCode = basePackCode + 1;

                        for (int i = 1; i <= baseAllocatedNumber; i++)
                        {
                            long key = baseSPAJCode + i;
                            string completeUpdateStatement = basicUpdateCommand + "SET PACKCode =" + newPackCode + " WHERE SPAJCode =" + key;
                            db.Execute(completeUpdateStatement);
                        }

                        //save mapping information into cross table
                        SpajNumber_Agent crossModels = new SpajNumber_Agent();
                        crossModels.PACKCode = newPackCode.ToString();
                        crossModels.AgentCode = agentCode.ToString();
                        crossModels.TableKey = Guid.NewGuid();
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
                        SpajNumber_Agent packQueryResult = db.Query<SpajNumber_Agent>(packQuery).First();
                        string packCode = packQueryResult.PACKCode;

                        string spajCountQuery = "SELECT COUNT (SPAJCode) FROM TBM_SPAJ_NUMBER WHERE PACKCode = " + packCode + " AND Status <> 'Submitted'";
                        int spajCountQueryResult = db.ExecuteScalar<int>(spajCountQuery);

                        if (spajCountQueryResult < 30)
                        {
                            for (int i = 1; i <= baseAllocatedNumber; i++)
                            {
                                long key = lastAllocSPAJNumber + i;
                                string completeUpdateStatement = basicUpdateCommand + "SET PACKCode =" + newPackCode + ",Status='Allocated' WHERE SPAJCode =" + key;
                                db.Execute(completeUpdateStatement);
                            }

                            //save mapping information into cross table
                            SpajNumber_Agent crossModels = new SpajNumber_Agent();
                            crossModels.PACKCode = basePackCode.ToString();
                            crossModels.AgentCode = agentCode.ToString();
                            crossModels.TableKey = Guid.NewGuid();
                            crossModels.AssignDate = DateTime.Now;
                            db.Insert("TBC_SPAJ_NUMBER_AGENT", crossModels);
                            resultMessage = "spaj allocated";
                        }
                        else
                        {
                            resultMessage = "credit still > 30";
                        }
                    }
                }
                catch (Exception ex)
                {
                    resultMessage = ex.Message;
                }
                context.Response.ContentType = "application/json";
                context.Response.Write(resultMessage);
            }

            if (functionSelector == "GetAllocatedSpajList")
            {
                string responseResult = string.Empty;
                try
                {
                    int agentCode = Convert.ToInt32(context.Request["agentCode"]);
                    var db = Database.GetPetaPocoDB();
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

                    responseResult = new JavaScriptSerializer().Serialize(result);
                }
                catch (Exception ex)
                {
                    responseResult = new JavaScriptSerializer().Serialize(ex.Message);
                }
                context.Response.ContentType = "application/json";
                context.Response.Write(responseResult);
            }
            if (functionSelector == "GetPackCodeList")
            {
                string responseResult = string.Empty;
                GetPackCodeListResult queryResult = new GetPackCodeListResult();

                List<string> tempPackCode = new List<string>();
                List<DateTime> tempAssignDate = new List<DateTime>();
                try
                {
                    int agentCode = Convert.ToInt32(context.Request["agentCode"]);
                    var db = Database.GetPetaPocoDB();
                    string packCodeQuery = "SELECT AgentCode, PACKCode, AssignDate FROM TBC_SPAJ_NUMBER_AGENT WHERE AgentCode =" + agentCode;
                    foreach (SpajNumber_Agent packQueryResult in db.Query<SpajNumber_Agent>(packCodeQuery))
                    {
                        queryResult.agentCode = packQueryResult.AgentCode;
                        tempPackCode.Add(packQueryResult.PACKCode);
                        tempAssignDate.Add(packQueryResult.AssignDate);
                    }
                    queryResult.AssignDate = tempAssignDate;
                    queryResult.PackCodeList = tempPackCode;

                    responseResult = new JavaScriptSerializer().Serialize(queryResult);
                }
                catch (Exception ex)
                {
                    responseResult = new JavaScriptSerializer().Serialize(ex.Message);
                }
                context.Response.ContentType = "application/json";
                context.Response.Write(responseResult);
            }
            if (functionSelector == "GetSpajList")
            {
                List<string> tempResult = new List<string>();
                string responseResult;
                GetSpajListResult modelResult = new GetSpajListResult();
                try
                {
                    int packcode = Convert.ToInt32(context.Request["packCode"]);
                    var db = Database.GetPetaPocoDB();
                    string packCodeQuery = "SELECT SPAJCode, PACKCode, Status FROM TBM_SPAJ_NUMBER WHERE PACKCode =" + packcode;
                    foreach (SPAJMaster packQueryResult in db.Query<SPAJMaster>(packCodeQuery))
                    {
                        modelResult.packCode = packQueryResult.PACKCode;
                        tempResult.Add(packQueryResult.SPAJCode);
                        //queryResult.Add(packQueryResult.SPAJCode);
                    }
                    modelResult.SpajList = tempResult;
                    responseResult = new JavaScriptSerializer().Serialize(modelResult);
                }
                catch (Exception ex)
                {
                    responseResult = new JavaScriptSerializer().Serialize(ex.Message);
                }
                
                context.Response.ContentType = "application/json";
                
                context.Response.Write(responseResult);
                context.Response.Write(context.Response.ContentType);
            }
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }
}