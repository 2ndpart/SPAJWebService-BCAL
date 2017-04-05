using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.SqlClient;

namespace HtmlGeneratorServices.ServicesHelper
{
    public class LogWriter
    {
        public static void WriteLog(string LogMessage)
        {
            string insertLogStatement = "INSERT INTO LogMessage VALUES(@logText,@logDate)";
            SqlConnection conn = E_Submission.ESubmissionHelper.Database.GetSqlConnection();
            conn.Open();
            SqlCommand command = new SqlCommand(insertLogStatement, conn);
            command.Parameters.AddWithValue("@logText", LogMessage);
            command.Parameters.AddWithValue("@logDate", DateTime.Now);
            command.ExecuteNonQuery();
            conn.Close();
        }
    }
}