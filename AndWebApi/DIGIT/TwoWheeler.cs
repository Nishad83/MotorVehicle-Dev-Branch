
namespace AndWebApi.DIGIT
{
    #region namespace
    using AndApp;
    using AndWebApi.Models;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;
    #endregion

    public class TwoWheeler
    {
        public Response GetQuoteRequest(Quotation model)
        {
            Response resModel = new Response();
            try
            {
            }
            catch (Exception ex)
            {
                resModel.Status = Status.Fail;
                resModel.ErrorMsg = ex.ToString();
                throw;
            }
            return resModel;
        }
        public Response GetQuoteResponse(string res)
        {
            Response resModel = new Response();
            try
            {

            }
            catch (Exception ex)
            {
                resModel.Status = Status.Fail;
                resModel.ErrorMsg = ex.ToString();
                throw;
            }
            return resModel;
        }
    }
}