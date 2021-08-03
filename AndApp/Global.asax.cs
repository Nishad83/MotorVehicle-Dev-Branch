using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace AndApp
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            Mapper.CreateMap<AndApp.Models.Response, AndWebApi.Models.Response>();
            Mapper.CreateMap<AndApp.Models.CustomIDV, AndWebApi.Models.CustomIDV>();
            Mapper.CreateMap<AndApp.Models.AddonCover, AndWebApi.Models.AddonCover>();
            Mapper.CreateMap<AndApp.Models.VehicleAddressDetails, AndWebApi.Models.VehicleAddressDetails>();
            Mapper.CreateMap<AndApp.Models.ClientDetails, AndWebApi.Models.ClientDetails>();
            Mapper.CreateMap<AndApp.Models.CompanyWiseRefference, AndWebApi.Models.CompanyWiseRefference>();
            Mapper.CreateMap<AndApp.Models.CoverageDetails, AndWebApi.Models.CoverageDetails>();
            Mapper.CreateMap<AndApp.Models.PaymentRequest, AndWebApi.Models.PaymentRequest>();
            Mapper.CreateMap<AndApp.Models.AddCRMData, AndWebApi.Models.AddCRMData>();
            Mapper.CreateMap<AndApp.Models.CommonModels, AndWebApi.Models.CommonModels>();
            Mapper.CreateMap<AndApp.Models.PremiumDetails, AndWebApi.Models.PremiumDetails>();
            Mapper.CreateMap<AndApp.Models.DiscountDetails, AndWebApi.Models.DiscountDetails>();
            Mapper.CreateMap<AndApp.Models.PremiumBreakUpDetails, AndWebApi.Models.PremiumBreakUpDetails>();
            Mapper.CreateMap<AndApp.Models.QualifyCompany, AndWebApi.Models.QualifyCompany>();
            Mapper.CreateMap<AndApp.Models.Quotation, AndWebApi.Models.Quotation>();
            Mapper.CreateMap<AndApp.Models.PreviousPolicyDetails, AndWebApi.Models.PreviousPolicyDetails>();
            Mapper.CreateMap<AndApp.Models.PreviousTPPolicyDetails, AndWebApi.Models.PreviousTPPolicyDetails>();
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }
    }
}
