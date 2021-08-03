using AndApp;
using AndWebApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml;

namespace AndWebApi.DIGIT
{
    public class Health
    {
        public Response GetQuoteRequest(Quotation model)
        {
            Response resModel = new Response();
            XmlDocument doc = new XmlDocument();
            try
            {

            }
            catch (Exception ex)
            {
                resModel.Status = Status.Fail;
                resModel.ErrorMsg = ex.ToString();
            }
            return resModel;
        }
    }
}