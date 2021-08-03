namespace AndWebApi.Controllers
{
    #region namespace
    using AndApp;
    using AndApp.Utilities;
    using Controllers;
    using DAL;
    using Models;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using RestSharp;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Data;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Net.Security;
    using System.Threading;
    using System.Web.Http;
    using System.Web.Http.ModelBinding;
    #endregion

    public class PrivateCarController : ApiController
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



            if (ModelState.IsValid)
            {



                if (QuoteModel.PreviousPolicyDetails != null && QuoteModel.PreviousPolicyDetails.CompanyId == 5)
                {
                    res.Status = Status.Fail;
                    res.ErrorMsg = "Renewal Is Not Provide By FGI";
                    res.FinalPremium = 0;
                    LogU.WriteLog("PrivateCarController >> FGI >> Error in FGI quotation !!!");
                }
                else if (QuoteModel.PreviousTPPolicyDetails != null && QuoteModel.PreviousTPPolicyDetails.CompanyId == 5)
                {
                    res.Status = Status.Fail;
                    res.ErrorMsg = "Renewal Is Not Provide By FGI";
                    res.FinalPremium = 0;
                    LogU.WriteLog("PrivateCarController >> FGI >> Error in FGI quotation !!!");
                }
                else
                {
                    AndWebApi.FGI.PrivateCar pcfgservice = new AndWebApi.FGI.PrivateCar();
                    //res = pcfgservice.GetQuoteRequest(QuoteModel);
                    Thread thread = new Thread(() => { res = pcfgservice.GetQuoteRequest(QuoteModel); });
                    thread.Start();
                    thread.Join();
                }
               
            }
            else
            {
                var message = string.Join(" | ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                res.ErrorMsg = message;
                res.FinalPremium = 0;
                LogU.WriteLog("PrivateCarController >> FGI >>" + message);
            }
            res.Product = Product.Motor;
            res.SubProduct = SubProduct.PrivateCar;
            res.CompanyName = Company.FGI.ToString();

            return res;
        }

        [HttpPost]
        public Response RAHEJA(Quotation QuoteModel)
        {
            Response res = new Response();
            if (ModelState.IsValid)
            {
                if (QuoteModel.PreviousPolicyDetails != null && QuoteModel.PreviousPolicyDetails.CompanyId == 16)
                {
                    res.Status = Status.Fail;
                    res.ErrorMsg = "Renewal Is Not Provide By RAHEJA";
                    res.FinalPremium = 0;
                    LogU.WriteLog("PrivateCarController >> RAHEJA >> Error in RAHEJA quotation !!!");
                }
                else if (QuoteModel.PreviousTPPolicyDetails != null && QuoteModel.PreviousTPPolicyDetails.CompanyId == 16)
                {
                    res.Status = Status.Fail;
                    res.ErrorMsg = "Renewal Is Not Provide By RAHEJA";
                    res.FinalPremium = 0;
                    LogU.WriteLog("PrivateCarController >> RAHEJA >> Error in RAHEJA quotation !!!");
                }
                else
                {
                        
              
                AndWebApi.RAHEJA.PrivateCar pcrjservice = new AndWebApi.RAHEJA.PrivateCar();
                Thread thread = new Thread(() => { res = pcrjservice.GetQuoteRequest(QuoteModel); });
                thread.Start();
                thread.Join();
                }
            }
            else
            {
                var message = string.Join(" | ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                res.ErrorMsg = message;
                res.FinalPremium = 0;
                LogU.WriteLog("PrivateCarController >> RAHEJA >>" + message);
            }
            res.Product = Product.Motor;
            res.SubProduct = SubProduct.PrivateCar;
            res.CompanyName = Company.RAHEJA.ToString();

            return res;
        }



        /// <summary>
        /// Create a method for getting icici quotation. 
        /// </summary>
        /// <param name="QuoteModel">Pass object of quote model.</param>
        /// <returns>Return object type of response model.</returns>
        [HttpPost]
        public Response ICICI(Quotation QuoteModel)
        {
            Response res = new Response();
            if (ModelState.IsValid)
            {
                if (QuoteModel.PreviousPolicyDetails != null && QuoteModel.PreviousPolicyDetails.CompanyId == 9)
                {
                    res.Status = Status.Fail;
                    res.ErrorMsg = "Renewal Is Not Provide By ICICI";
                    res.FinalPremium = 0;
                    LogU.WriteLog("PrivateCarController >> ICICI >> Error in digit quotation !!!");
                }
                else if (QuoteModel.PreviousTPPolicyDetails != null && QuoteModel.PreviousTPPolicyDetails.CompanyId == 9)
                {
                    res.Status = Status.Fail;
                    res.ErrorMsg = "Renewal Is Not Provide By ICICI";
                    res.FinalPremium = 0;
                    LogU.WriteLog("PrivateCarController >> ICICI >> Error in digit quotation !!!");
                }
                else
                {
                    AndWebApi.ICICI.PrivateCar pciciciservice = new AndWebApi.ICICI.PrivateCar();
                    Thread thread = new Thread(() => { res = pciciciservice.GetQuoteRequest(QuoteModel); });
                    thread.Start();
                    thread.Join();
                }
              
            }
            else
            {
                res.Status = Status.Fail;
                res.ErrorMsg = "Error while getting ICICI quotation !!!";
                res.FinalPremium = 0;
                LogU.WriteLog("PrivateCarController >> ICICI >> Error in digit quotation !!!");
            }

            res.CompanyName = Company.ICICI.ToString();


            return res;
        }
        // <summary>
        // Create a method for getting digit quotation.
        // </summary>
        // <param name = "QuoteModel" > Pass object of quote model.</param>
        // <returns>Return object type of response model.</returns>

        [HttpPost]
        public Response DIGIT(Quotation QuoteModel)
        {
            Response res = new Response();
            if (ModelState.IsValid)
            {

                if (QuoteModel.PreviousPolicyDetails != null && QuoteModel.PreviousPolicyDetails.CompanyId == 16)
                {
                    res.Status = Status.Fail;
                    res.ErrorMsg = "Renewal Is Not Provide By RAHEJA";
                    res.FinalPremium = 0;
                    LogU.WriteLog("PrivateCarController >> RAHEJA >> Error in RAHEJA quotation !!!");
                }
                else if (QuoteModel.PreviousTPPolicyDetails != null && QuoteModel.PreviousTPPolicyDetails.CompanyId == 16)
                {
                    res.Status = Status.Fail;
                    res.ErrorMsg = "Renewal Is Not Provide By RAHEJA";
                    res.FinalPremium = 0;
                    LogU.WriteLog("PrivateCarController >> RAHEJA >> Error in RAHEJA quotation !!!");
                }
                else
                {
                    if (QuoteModel.PreviousPolicyDetails != null && QuoteModel.PreviousPolicyDetails.CompanyId == 6)
                    {
                        res.Status = Status.Fail;
                        res.ErrorMsg = "Renewal Is Not Provide By DIGIT";
                        res.FinalPremium = 0;
                        LogU.WriteLog("PrivateCarController >> DIGIT >> Error in DIGIT quotation !!!");
                    }
                    else if (QuoteModel.PreviousTPPolicyDetails != null && QuoteModel.PreviousTPPolicyDetails.CompanyId == 6)
                    {
                        res.Status = Status.Fail;
                        res.ErrorMsg = "Renewal Is Not Provide By DIGIT";
                        res.FinalPremium = 0;
                        LogU.WriteLog("PrivateCarController >> DIGIT >> Error in DIGIT quotation !!!");
                    }
                    else
                    {

                        AndWebApi.DIGIT.PrivateCar pcdigitservice = new AndWebApi.DIGIT.PrivateCar();
                        Thread thread = new Thread(() => { res = pcdigitservice.GetQuoteRequest(QuoteModel); });
                        thread.Start();
                        thread.Join();
                    }
                }
            }
            else
            {
                var message = string.Join(" | ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                res.ErrorMsg = message;
                res.FinalPremium = 0;
                LogU.WriteLog("PrivateCarController >> DIGIT >>" + message);
            }

            res.CompanyName = Company.DIGIT.ToString();

            return res;
        }

        [HttpPost]
        public Response TATA(Quotation QuoteModel)
        {
            Response res = new Response();
            if (ModelState.IsValid)
            {

                if (QuoteModel.PreviousPolicyDetails != null && QuoteModel.PreviousPolicyDetails.CompanyId == 22)
                {
                    res.Status = Status.Fail;
                    res.ErrorMsg = "Renewal Is Not Provide By TATA";
                    res.FinalPremium = 0;
                    LogU.WriteLog("PrivateCarController >> TATA >> Error in TATA quotation !!!");
                }
                else if (QuoteModel.PreviousTPPolicyDetails != null && QuoteModel.PreviousTPPolicyDetails.CompanyId == 22)
                {
                    res.Status = Status.Fail;
                    res.ErrorMsg = "Renewal Is Not Provide By TATA";
                    res.FinalPremium = 0;
                    LogU.WriteLog("PrivateCarController >> TATA >> Error in TATA quotation !!!");
                }
                else
                {
                    AndWebApi.TATA.PrivateCar pctataservice = new AndWebApi.TATA.PrivateCar();
                    Thread thread = new Thread(() => { res = pctataservice.GetQuoteRequest(QuoteModel); });
                    thread.Start();
                    thread.Join();
                }
            }
            else
            {
                var message = string.Join(" | ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                res.ErrorMsg = message;
                res.FinalPremium = 0;
                LogU.WriteLog("PrivateCarController >> TATA >>" + message);
            }

            res.CompanyName = Company.TATA.ToString();

            return res;
        }

        //Create a method for getting KOTAK quotation
        [HttpPost]
        public Response KOTAK(Quotation QuoteModel)
        {
            Response res = new Response();
            if (ModelState.IsValid)
            {
                //AndWebApi.KOTAK.PrivateCar pckotakservice = new AndWebApi.KOTAK.PrivateCar();
                //Thread thread = new Thread(() => { res = pckotakservice.GetQuoteRequest(QuoteModel); });
                //thread.Start();
                //thread.Join();
            }
            else
            {
                var message = string.Join(" | ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                res.ErrorMsg = message;
                res.FinalPremium = 0;
                LogU.WriteLog("PrivateCarController >> KOTAK >>" + message);
            }

            res.CompanyName = Company.KOTAK.ToString();

            return res;
        }



        // <summary>
        // Create a method for getting bharti quotation. 
        // </summary>
        // <param name="QuoteModel">Pass object of quote model.</param>
        // <returns>Return object type of response model.</returns>
        [HttpPost]
        public Response BHARTI(Quotation QuoteModel)
        {
            Response res = new Response();
            if (ModelState.IsValid)
            {
                AndWebApi.BHARTI.PrivateCar pcbhartiservice = new AndWebApi.BHARTI.PrivateCar();
                Thread thread = new Thread(() => { res = pcbhartiservice.GetProposalRequest(QuoteModel); });
                thread.Start();
                thread.Join();
            }
            else
            {
                var message = string.Join(" | ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                res.ErrorMsg = message;
                res.FinalPremium = 0;
                LogU.WriteLog("PrivateCarController >> BHARTI >> Error in bharati axa quotation !!!");
            }

            res.CompanyName = Company.BHARTI.ToString();

            return res;
        }


        // <summary>
        // Create a method for getting HDFC quotation.
        // </summary>
        // <param name = "QuoteModel" > Pass object of quote model.</param>
        // <returns>Return object type of response model.</returns>

        [HttpPost]
        public Response HDFC(Quotation QuoteModel)
        {
            Response res = new Response();
            if (ModelState.IsValid)
            {

                if (QuoteModel.PreviousPolicyDetails != null && QuoteModel.PreviousPolicyDetails.CompanyId == 8)
                {
                    res.Status = Status.Fail;
                    res.ErrorMsg = "Renewal Is Not Provide By HDFC";
                    res.FinalPremium = 0;
                    LogU.WriteLog("PrivateCarController >> HDFC >> Error in HDFC quotation !!!");
                }
                else if (QuoteModel.PreviousTPPolicyDetails != null && QuoteModel.PreviousTPPolicyDetails.CompanyId == 8)
                {
                    res.Status = Status.Fail;
                    res.ErrorMsg = "Renewal Is Not Provide By HDFC";
                    res.FinalPremium = 0;
                    LogU.WriteLog("PrivateCarController >> HDFC >> Error in HDFC quotation !!!");
                }
                else
                {

                    AndWebApi.HDFC.PrivateCar phdfcservice = new AndWebApi.HDFC.PrivateCar();
                    Thread thread = new Thread(() => { res = phdfcservice.GetQuoteRequest(QuoteModel); });
                    thread.Start();
                    thread.Join();
                }
            }
            else
            {
                var message = string.Join(" | ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                res.ErrorMsg = message;
                res.FinalPremium = 0;
                LogU.WriteLog("PrivateCarController >> HDFC >>" + message);
            }

            res.CompanyName = Company.HDFC.ToString();

            return res;
        }
        /// <summary>
        /// Create a method for getting RELIANCE quotation.
        /// </summary>
        /// <param name="QuoteModel">Pass object of quote model.</param>
        /// <returns>Return object type of response model.</returns>
        //[HttpPost]
        //public Response RELIANCE(Quotation QuoteModel)
        //{
        //    Response res = new Response();
        //    if (!ModelState.IsValid)
        //    {
        //        AndWebApi.RELIANCE.PrivateCar pcbhartiservice = new AndWebApi.RELIANCE.PrivateCar();
        //        Thread thread = new Thread(() => { res = pcbhartiservice.GetQuoteRequest(QuoteModel); });
        //        thread.Start();
        //        thread.Join();
        //    }
        //    else
        //    {
        //        var message = string.Join(" | ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
        //        res.ErrorMsg = message;
        //        res.FinalPremium = 0;
        //        LogU.WriteLog("PrivateCarController >> RELIANCE >>" + message);
        //    }
        //    //else
        //    //{
        //    // res.Status = Status.Fail;
        //    // res.ErrorMsg = "Error while getting RELIANCE quotation !!!";
        //    // res.FinalPremium = 0;
        //    // LogU.WriteLog("PrivateCarController >> RELIANCE >> Error in RELIANCE quotation !!!");
        //    //}

        //    res.CompanyName = Company.RELIANCE;
        //    return res;
        //}
        #endregion

        /// <summary>
        /// create a method for proposal.
        /// </summary>
        /// <param name="Promodel"></param>
        /// <returns></returns>
        /// 
        [HttpPost]
        public Response Proposal(Quotation Promodel)
        {
            Response ProRes = new Response();
            switch (Promodel.CompanyName)
            {
                case Company.BAJAJ:
                    break;
                case Company.DIGIT:
                    AndWebApi.DIGIT.PrivateCar digitPropservice = new AndWebApi.DIGIT.PrivateCar();
                    ProRes = digitPropservice.GetProposalRequest(Promodel);
                    break;
                case Company.FGI:
                    AndWebApi.FGI.PrivateCar fgProservice = new AndWebApi.FGI.PrivateCar();
                    ProRes = fgProservice.GetProposalRequest(Promodel);
                    break;
                case Company.ICICI:
                    AndWebApi.ICICI.PrivateCar iciciProservice = new AndWebApi.ICICI.PrivateCar();
                    ProRes = iciciProservice.GetProposalRequest(Promodel);
                    break;

                case Company.TATA:
                    AndWebApi.TATA.PrivateCar tataProservice = new AndWebApi.TATA.PrivateCar();
                    ProRes = tataProservice.GetProposalRequest(Promodel);
                    break;

                case Company.BHARTI:
                    break;
                case Company.RAHEJA:
                    AndWebApi.RAHEJA.PrivateCar RAHEJAProservice = new AndWebApi.RAHEJA.PrivateCar();
                    ProRes = RAHEJAProservice.GetProposalRequest(Promodel);
                    break;

                case Company.HDFC:
                    AndWebApi.HDFC.PrivateCar hdfcProservice = new AndWebApi.HDFC.PrivateCar();
                    ProRes = hdfcProservice.GetProposalRequest(Promodel);
                    break;
                default:
                    break;
            }
            return ProRes;
        }

        public string PaymentRequestDetails(PaymentRequest reqModel)
        {
            string PaymentData = string.Empty;
            switch (reqModel.CompanyName)
            {
                case Company.BAJAJ:
                    break;
                case Company.DIGIT:
                    AndWebApi.DIGIT.PrivateCar digitPropservice = new AndWebApi.DIGIT.PrivateCar();
                    PaymentData = digitPropservice.GetPaymentParameter(reqModel);
                    break;
                //case Company.FGI:
                //    FGI.PrivateCar fgProservice = new FGI.PrivateCar();
                //    //PaymentData = fgProservice.GetPaymentParameter(reqModel);
                //    break;
                case Company.ICICI:
                    AndWebApi.ICICI.PrivateCar iciciPropservice = new AndWebApi.ICICI.PrivateCar();
                    PaymentData = iciciPropservice.GetPaymentParameter(reqModel);
                    break;

                case Company.TATA:
                    AndWebApi.TATA.PrivateCar tataPropservice = new AndWebApi.TATA.PrivateCar();
                    PaymentData = tataPropservice.GetPaymentParameter(reqModel);
                    break;

                case Company.BHARTI:
                    break;
                case Company.RAHEJA:
                    AndWebApi.RAHEJA.PrivateCar RAHEJAPayservice = new AndWebApi.RAHEJA.PrivateCar();
                    PaymentData = RAHEJAPayservice.GetPaymentParameter(reqModel);
                    break;
                default:
                    break;
            }
            return PaymentData;
        }
        //public string PaymentRequestDetails(Quotation reqModel, Response resModel)
        //{
        //    string PaymentData = string.Empty;
        //    switch (reqModel.CompanyName)
        //    {
        //        case Company.BAJAJ:
        //            break;
        //        case Company.DIGIT:
        //            break;
        //        case Company.FGI:
        //            FGI.PrivateCar fgProservice = new FGI.PrivateCar();
        //            PaymentData = fgProservice.GetPaymentParameter(reqModel, resModel);
        //            break;
        //        case Company.ICICI:
        //            break;
        //        case Company.BHARTI:
        //            break;
        //        case Company.RAHEJA:
        //            break;
        //        default:
        //            break;
        //    }
        //    return PaymentData;
        //}

        /// <summary>
        /// create a method for getting policy pdf.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public Response PolicyPdf(Quotation model)
        {
            string PaymentData = string.Empty;
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
                case Company.HDFC:
                    break;
                default:
                    break;
            }
            return PolicyPdfRes;
        }

        public Response ProposalAfterPayment(PaymentRequest reqModel)
        {
            Response PolicyPdfRes = new Response();
            switch (reqModel.CompanyName)
            {
                case Company.FGI:
                    AndWebApi.FGI.PrivateCar FGIProafteservice = new AndWebApi.FGI.PrivateCar();
                    PolicyPdfRes = FGIProafteservice.GetProposalRequestAfterPayment(reqModel);
                    break;
                case Company.RAHEJA:
                    AndWebApi.RAHEJA.PrivateCar RAHEJAProafteservice = new AndWebApi.RAHEJA.PrivateCar();
                    PolicyPdfRes = RAHEJAProafteservice.GetPolicyno(reqModel);
                    break;

            }
            return PolicyPdfRes;
        }

        public string PaymentMapping(PaymentRequest reqModel)
        {
            string PolicyPdfRes = "";
            AndWebApi.ICICI.PrivateCar ICICProafteservice = new AndWebApi.ICICI.PrivateCar();
            PolicyPdfRes = ICICProafteservice.PaymentMapping(reqModel);

            return PolicyPdfRes;
        }



        [HttpGet]
        public string DIGITPolicyPdf(string applicationid)
        {
            AndWebApi.DIGIT.PrivateCar digitPropservice = new AndWebApi.DIGIT.PrivateCar();
            var PolicyPdfRes = digitPropservice.GeneratePolicyPDF(applicationid);
            return PolicyPdfRes;
        }

        [HttpGet]
        public string DIGITPolicySearch(string policyno)
        {
            AndWebApi.DIGIT.PrivateCar digitPropservice = new AndWebApi.DIGIT.PrivateCar();
            var PolicyPdfRes = digitPropservice.GetPolicyStatus(policyno);
            return PolicyPdfRes;
        }




    }
}