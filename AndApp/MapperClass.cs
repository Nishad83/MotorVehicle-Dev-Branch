using AutoMapper;
namespace AndApp
{

    public  class MapperClass
    {
       

        public void AutoMapperBase()
        {
            Mapper.CreateMap<AndApp.Models.Response, AndWebApi.Models.Response>();
            Mapper.CreateMap<AndApp.Models.AddonCover, AndWebApi.Models.AddonCover>();
            Mapper.CreateMap<AndApp.Models.VehicleAddressDetails, AndWebApi.Models.VehicleAddressDetails>();
            Mapper.CreateMap<AndApp.Models.ClientDetails, AndWebApi.Models.ClientDetails>();
            Mapper.CreateMap<AndApp.Models.CompanyWiseRefference, AndWebApi.Models.CompanyWiseRefference>();
            Mapper.CreateMap<AndApp.Models.CoverageDetails, AndWebApi.Models.CoverageDetails>();

            Mapper.CreateMap<AndApp.Models.DiscountDetails, AndWebApi.Models.DiscountDetails>();
            Mapper.CreateMap<AndApp.Models.PremiumBreakUpDetails, AndWebApi.Models.PremiumBreakUpDetails>();
            Mapper.CreateMap<AndApp.Models.QualifyCompany, AndWebApi.Models.QualifyCompany>();
            Mapper.CreateMap<AndApp.Models.Quotation, AndWebApi.Models.Quotation>();
            Mapper.CreateMap<AndApp.Models.PreviousPolicyDetails, AndWebApi.Models.PreviousPolicyDetails>();
        }
    }
}