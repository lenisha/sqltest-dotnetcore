using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CloudQuery.Models
{
    public class QueryResult
    {
        public string Message { get; set; }

        public System.Data.DataTable Results { get; set; }

        public Exception Exception { get; set; }
    }
}
