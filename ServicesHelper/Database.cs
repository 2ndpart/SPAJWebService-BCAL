using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.SqlClient;
using System.Configuration;

namespace E_Submission.ESubmissionHelper
{
    public class Database
    {
        public static SqlConnection GetSqlConnection()
        {
            string connectionString = ConfigurationManager.ConnectionStrings["ConnStringDev"].ConnectionString;
            SqlConnection conn = new SqlConnection(connectionString);
            return conn;
        }
        public static void SaveOrUpdate(string query)
        {
            SqlConnection connection = GetSqlConnection();
            connection.Open();
            SqlCommand command = new SqlCommand(query, connection);
            command.ExecuteNonQuery();
            connection.Close();
            connection.Dispose();
        }
        public static PetaPoco.Database GetPetaPocoDB()
        {
            return new PetaPoco.Database("ConnStringDev");
        }
    }
}