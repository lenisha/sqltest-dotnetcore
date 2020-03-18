using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Text.Encodings.Web;
using System.Data.SqlClient;
using System.Web;
using System.IO;

namespace CloudQuery.Controllers
{
    public class EditorController : Controller
    {
        private const string AzureSecretsMountPath = "/etc/sqlservice";

        public IActionResult Index()
        {
            // let's set ViewData value for Connections
            if (ViewData["Connections"] == null) ViewData["Connections"] = new List<CloudQuery.Models.DBConnection>();

            return View();
        }

        public IActionResult OnPostExecuteQuery(string query, string connections)
        {
            CloudQuery.Models.QueryResult qr = new Models.QueryResult();
            qr.Message = "Query execution incomplete";

            bool isMSI = "true".CompareTo(System.Environment.GetEnvironmentVariable("MSI")) == 0;
            bool isAE =  "true".CompareTo(System.Environment.GetEnvironmentVariable("ODBC_ALWAYSENCRYPTED")) ==0;

            // get the connectionObject
            var conn = Newtonsoft.Json.JsonConvert.DeserializeObject<CloudQuery.Models.DBConnection>(connections); 
            if ( conn.DBName.StartsWith("#") )
                PopulateConnectionDataFromMount(conn);          
            string connectionString = string.Format("Server=tcp:{0}.database.windows.net,1433;Database={1};Uid={2}@{0};Pwd={3};Encrypt=yes;TrustServerCertificate=no;Connection Timeout=30;", conn.DBServer, conn.DBName, conn.Username, conn.Password);
            if ( isMSI )
               connectionString = string.Format("Server=tcp:{0}.database.windows.net,1433;Database={1};Uid={2}@{0};Authentication=ActiveDirectoryMsi;Encrypt=yes;TrustServerCertificate=no;Connection Timeout=30", conn.DBServer, conn.DBName, conn.Username);
            if ( isMSI && isAE )
               connectionString = string.Format("Server=tcp:{0}.database.windows.net,1433;Database={1};Uid={2};Authentication=ActiveDirectoryMsi;Encrypt=yes;TrustServerCertificate=no;Connection Timeout=30;ColumnEncryption=Enabled;KeyStoreAuthentication=KeyVaultManagedIdentity;KeyStorePrincipalId={2}", conn.DBServer, conn.DBName, conn.Username);
            
            connectionString = "Driver={ODBC Driver 17 for SQL Server};" + connectionString;
            Console.WriteLine($"ConnectionString: {connectionString}");

            try
            {
                int rowsReturned = 0;
                System.Data.DataSet ds;

                using (System.Data.Odbc.OdbcDataAdapter adapter = new System.Data.Odbc.OdbcDataAdapter(query, connectionString))
                {
                    ds = new System.Data.DataSet();
                    rowsReturned = adapter.Fill(ds);
                }

                // only selects will return a Table of results;
                if (ds.Tables.Count == 0) // not a table
                {
                    qr.Message = "Query completed successfully";
                }
                else if (ds.Tables.Count == 1)
                {
                    // return the results
                    qr.Results = ds.Tables[0];

                    // return rowsAffected for message
                    qr.Message = string.Format("{0} rows returned.", rowsReturned);
                }
            }
            catch (Exception ex)
            {
                qr.Exception = ex;
                qr.Message = ex.Message;               
            }

            ViewData["results"] = qr;
            return View("Index");
        }

        void  PopulateConnectionDataFromMount(CloudQuery.Models.DBConnection conn) 
        {
            Console.WriteLine($"Processing secrets: {AzureSecretsMountPath}");

            string data = System.IO.File.ReadAllText(AzureSecretsMountPath +"/username");
            Console.WriteLine($"Secret 'username' has value '{data}'");
            conn.Username = data;
            data = System.IO.File.ReadAllText(AzureSecretsMountPath +"/password");
            conn.Password = data;
            data = System.IO.File.ReadAllText(AzureSecretsMountPath +"/azuresqlservername");
            conn.DBServer = data;
            conn.DBName = conn.DBName.Substring(1);
        }
    }

}