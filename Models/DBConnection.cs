using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CloudQuery.Models
{
    public class DBConnection
    {
        public string ConnectionName { get; set; }
        public string DBServer { get; set; }
        public string DBName { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
    }
}
