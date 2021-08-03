

namespace AndWebApi.FGI
{
    #region namespace
    using AndApp;
    using AndWebApi.Models;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Web;
    using System.Xml;
    using System.Xml.XPath;
    using System.Xml.Linq;
    #endregion

    public class TwoWheeler
    {
        //public FutureService.ServiceClient Ser = new FutureService.ServiceClient();
        public Response GetQuoteRequest(Quotation model)
        {
            Response resModel = new Response();
            XmlDocument doc = new XmlDocument();
            try
            {
                string path = AppDomain.CurrentDomain.BaseDirectory;
                string filePath = Path.Combine(path, "XML/FGI/Quote.xml");
                var document = XDocument.Load(filePath);
                document.XPathSelectElement("//PolicyStartDate").Value = model.PolicyStartDate;
                document.XPathSelectElement("//PolicyEndDate").Value = model.PolicyEndDate;
                document.XPathSelectElement("//Make").Value = model.VehicleDetails.MakeCode;
                document.XPathSelectElement("//ModelCode").Value = model.VehicleDetails.ModelName;
                document.XPathSelectElement("//RegistrationNo").Value = !string.IsNullOrEmpty(model.VehicleDetails.RegistrationNumber) ? model.VehicleDetails.RegistrationNumber : string.Empty;
                document.XPathSelectElement("//RegistrationDate").Value = model.VehicleDetails.RegistrationDate;
                document.XPathSelectElement("//ManufacturingYear").Value = model.VehicleDetails.ManufaturingDate;
                document.XPathSelectElement("//IDV").Value = Convert.ToString(model.IDV);
                document.XPathSelectElement("//NewVehicle").Value = model.PolicyType.Equals("New", StringComparison.OrdinalIgnoreCase) ? "Y" : "N";
              //  var response = Ser.CreatePolicy("Motor", Convert.ToString(document));
              //  resModel = GetQuoteResponse(XDocument.Parse(response));
            }
            catch (Exception ex)
            {
                resModel.Status = Status.Fail;
                resModel.ErrorMsg = ex.ToString();
                throw;
            }
            return resModel;
        }
        public Response GetQuoteResponse(XDocument res)
        {
            Response resModel = new Response();
            try
            {
                string status = res.XPathSelectElement("//Status").Value;
                if ((!string.IsNullOrEmpty(status)) && status.Equals("Successful", StringComparison.OrdinalIgnoreCase))
                {
                    resModel.IDV = Convert.ToInt32(res.XPathSelectElement("//VehicleIDV").Value);
                    //PolicyNo = res.XPathSelectElement("//PolNo").Value;
                    //string table = res.XPathSelectElement("//Table1").Value;
                    List<XElement> xElementList = res.Descendants("Table1").ToList();
                    resModel.Status = Status.Success;
                }
                else
                {
                    resModel.Status = Status.Fail;
                    resModel.ErrorMsg = Convert.ToString(res.XPathSelectElement("//Policy/ErrorMessage"));
                }
            }
            catch (Exception ex)
            {
                resModel.Status = Status.Fail;
                resModel.ErrorMsg = ex.ToString();
                throw;
            }
            return resModel;
        }
        public Response GetProposalRequest(Quotation model)
        {
            Response resModel = new Response();
            XmlDocument doc = new XmlDocument();
            try
            {
                string path = AppDomain.CurrentDomain.BaseDirectory;
                string filePath = Path.Combine(path, "XML/FGI/Quote.xml");
                var document = XDocument.Load(filePath);
                document.XPathSelectElement("//PolicyStartDate").Value = model.PolicyStartDate;
                document.XPathSelectElement("//PolicyEndDate").Value = model.PolicyEndDate;
                document.XPathSelectElement("//Make").Value = model.VehicleDetails.MakeCode;
                document.XPathSelectElement("//ModelCode").Value = model.VehicleDetails.ModelName;
                document.XPathSelectElement("//RegistrationNo").Value = !string.IsNullOrEmpty(model.VehicleDetails.RegistrationNumber) ? model.VehicleDetails.RegistrationNumber : string.Empty;
                document.XPathSelectElement("//RegistrationDate").Value = model.VehicleDetails.RegistrationDate;
                document.XPathSelectElement("//ManufacturingYear").Value = model.VehicleDetails.ManufaturingDate;
                document.XPathSelectElement("//IDV").Value = Convert.ToString(model.IDV);
                document.XPathSelectElement("//NewVehicle").Value = model.PolicyType.Equals("New", StringComparison.OrdinalIgnoreCase) ? "Y" : "N";
               // var response = Ser.CreatePolicy("Motor", Convert.ToString(document));
                //resModel = GetProposalResponse(XDocument.Parse(response));
            }
            catch (Exception ex)
            {
                resModel.Status = Status.Fail;
                resModel.ErrorMsg = ex.ToString();
                throw;
            }
            return resModel;
        }
        public Response GetProposalResponse(XDocument res)
        {
            Response resModel = new Response();
            try
            {
                string status = res.XPathSelectElement("//Status").Value;
                if ((!string.IsNullOrEmpty(status)) && status.Equals("Successful", StringComparison.OrdinalIgnoreCase))
                {
                    resModel.IDV = Convert.ToInt32(res.XPathSelectElement("//VehicleIDV").Value);
                    //PolicyNo = res.XPathSelectElement("//PolNo").Value;
                    //string table = res.XPathSelectElement("//Table1").Value;
                    List<XElement> xElementList = res.Descendants("Table1").ToList();
                    resModel.Status = Status.Success;
                }
                else
                {
                    resModel.Status = Status.Fail;
                    resModel.ErrorMsg = Convert.ToString(res.XPathSelectElement("//Policy/ErrorMessage"));
                }
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