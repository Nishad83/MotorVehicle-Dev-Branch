using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AndWebApi.Models
{
    public class ResponseModel
    {
        public bool success;

        public string message;
        public object data { get; set; }
        public string enquiryid { get; set; }
    }

}