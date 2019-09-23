using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Text.Encodings.Web;
using System.Data.SqlClient;
using System.Web;

namespace CloudQuery.Controllers
{
    public class EditorController : Controller
    {
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

            // get the connectionObject
            var conn = Newtonsoft.Json.JsonConvert.DeserializeObject<CloudQuery.Models.DBConnection>(connections);            
            string connectionString = string.Format("Server=tcp:{0},1433;Database={1};Uid={2}@{0};Pwd={3};Encrypt=yes;TrustServerCertificate=no;Connection Timeout=30;", conn.DBServer, conn.DBName, conn.Username, conn.Password);
            connectionString = "Driver={ODBC Driver 17 for SQL Server};" + connectionString;

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
    }
}