namespace AndWebApi.Controllers
{
    #region namespace
    using AndApp;
    using AndWebApi.Models;
    using AndApp.Utilities;
    using DAL;
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Threading;
    using System.Web.Http;
    #endregion
    public class TwoWheelerController : ApiController
    {
        #region CompanyWiseQuotation

        /// <summary>
        /// Create a method for getting future quotation. 
        /// </summary>
        /// <param name="QuoteModel">Pass object of quote model.</param>
        /// <returns>Return object type of response model.</returns>
        [HttpPost]
        public Response FGI(Quotation QuoteModel)
        {
            Response res = new Response();
            if (!ModelState.IsValid)
            {
                AndWebApi.FGI.TwoWheeler twfgservice = new AndWebApi.FGI.TwoWheeler();
                Thread thread = new Thread(() => { res = twfgservice.GetQuoteRequest(QuoteModel); });
                thread.Start();
                thread.Join();
                LogU.WriteLog("QualifyCompany");
            }
            else
            {
                res.Status = Status.Fail;
                res.ErrorMsg = "Error while getting future generali quotation !!!";
                res.FinalPremium = 0;
                LogU.WriteLog("FGI >> future generali quotation !!!");
            }

            res.CompanyName = Company.FGI.ToString();
            return res;
        }

        /// <summary>
        /// Create a method for getting digit quotation. 
        /// </summary>
        /// <param name="QuoteModel">Pass object of quote model.</param>
        /// <returns>Return object type of response model.</returns>
        [HttpPost]
        public Response DIGIT(Quotation QuoteModel)
        {
            Response res = new Response();
            if (!ModelState.IsValid)
            {
                AndWebApi.DIGIT.TwoWheeler twdigitservice = new AndWebApi.DIGIT.TwoWheeler();
                Thread thread = new Thread(() => { res = twdigitservice.GetQuoteRequest(QuoteModel); });
                thread.Start();
                thread.Join();
                LogU.WriteLog("QualifyCompany>> Digit");
            }
            else
            {
                res.Status = Status.Fail;
                res.ErrorMsg = "Error while getting digit quotation !!!";
                res.FinalPremium = 0;
                LogU.WriteLog("DIGIT >> digit quotation !!!");
            }

            res.CompanyName = Company.DIGIT.ToString();
            return res;
        }

        /// <summary>
        /// Create a method for getting bharti quotation. 
        /// </summary>
        /// <param name="QuoteModel">Pass object of quote model.</param>
        /// <returns>Return object type of response model.</returns>
        [HttpPost]
        public Response BHARTI(Quotation QuoteModel)
        {
            Response res = new Response();
            if (!ModelState.IsValid)
            {
                AndWebApi.BHARTI.TwoWheeler twbhartiservice = new AndWebApi.BHARTI.TwoWheeler();
                Thread thread = new Thread(() => { res = twbhartiservice.GetQuoteRequest(QuoteModel); });
                thread.Start();
                thread.Join();
                LogU.WriteLog("QualifyCompany");
            }
            else
            {
                res.Status = Status.Fail;
                res.ErrorMsg = "Error while getting bharti quotation !!!";
                res.FinalPremium = 0;
                LogU.WriteLog("BHARTI >> bharati axa quotation !!!");
            }

            res.CompanyName = Company.BHARTI.ToString();
            return res;
        } 

        #endregion

        /// <summary>
        /// create a method for proposal.
        /// </summary>
        /// <param name="Promodel"></param>
        /// <returns></returns>
        public Response Proposal(Quotation Promodel)
        {
            Response ProRes = new Response();
            switch (Promodel.CompanyName)
            {
                case Company.BAJAJ:
                    break;
                case Company.DIGIT:
                    break;
                case Company.FGI:
                    AndWebApi.FGI.PrivateCar fgProservice = new AndWebApi.FGI.PrivateCar();
                    ProRes = fgProservice.GetProposalRequest(Promodel);
                    break;
                case Company.ICICI:
                    break;
                case Company.BHARTI:
                    break;
                case Company.RAHEJA:
                    break;
                default:
                    break;
            }
            return ProRes;
        }

        /// <summary>
        /// create a method for getting policy pdf.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public Response PolicyPdf(Quotation model)
        {
            Response PolicyPdfRes = new Response();
            switch (model.CompanyName)
            {
                case Company.BAJAJ:
                    break;
                case Company.DIGIT:
                    break;
                case Company.FGI:
                    break;
                case Company.ICICI:
                    break;
                case Company.BHARTI:
                    break;
                case Company.RAHEJA:
                    break;
                default:
                    break;
            }
            return PolicyPdfRes;
        }
    }
}