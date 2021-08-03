using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AndWebApi.Models
{
    public class CommonModels
    {
        public class UserSessionDetails
        {
            public long userid { get; set; }
            public string username { get; set; }
            public string mobileno { get; set; }
        }
    }
}
