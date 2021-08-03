using DAL;
using System;
using System.Web.Mvc;
using System.Linq;
using AndApp.Utilities;
using static AndApp.Models.SearchCriteria;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data.Entity.Core.Objects;
using static AndApp.Utilities.Common;

namespace AndApp.Areas.Motor.Controllers
{

    public class QuotationListController : Controller
    {
        DAL_CommonCls objcls = new DAL_CommonCls();
        ANDAPPEntities AndEnt = new ANDAPPEntities();
 
        // GET: Qutation
        [HttpGet]
        public ActionResult QuotationList()
        {
            try
            {
                QuotaionSearchCriteria model = new QuotaionSearchCriteria();
                BindData(model, 1, 10);
                FillDropDown_List();
                //SetValues(model);
                ViewBag.pageno = "1";
            }
            catch (Exception Ex)
            {
                Console.Write(Ex.Message.ToString());
                //LogU.WriteLog(Ex.InnerException.ToString());
            }
            return View(new QuotaionSearchCriteria());


        }

        [HttpPost]
        public ActionResult QuotationList(QuotaionSearchCriteria model)
        {
            try
            {
                BindData(model, 1, 10);
                FillDropDown_List();
                //SetValues(model);
                ViewBag.pageno = "1";
            }
            catch (Exception Ex)
            {
                Console.Write(Ex.Message.ToString());
                //LogU.WriteLog(Ex.InnerException.ToString());
            }
            return View(new QuotaionSearchCriteria());


        }
        public void BindData(QuotaionSearchCriteria model, int pageno, int pagesize)
        {

            try
            {
                ViewBag.error = "";
                var predicate = PredicateBuilder.True<VW_GENERATEDQUOTAIONLIST_POSPWISE>();

                //sessiondetails.userid
                //Make ID
                if (model.makeid != 0 && model.makeid != null)
                {
                    //model.variantid = 12;
                    predicate = predicate.And(i => i.makeid == model.makeid);
                }

                //Model ID
                if (model.modelid != 0 && model.modelid != null)
                {
                    //model.variantid = 12;
                    predicate = predicate.And(i => i.modelid == model.modelid);
                }

                //variant ID
                if (model.variantid != 0 && model.variantid != null)
                {
                    //model.variantid = 12;
                    predicate = predicate.And(i => i.VariantId == model.variantid.ToString());
                }

                //insurancename
                if (model.insurancename != null)
                {
                    predicate = predicate.And(i => i.Company.Contains(model.insurancename)  );
                }

                // registrationno
                if (model.registrationno != null)
                {
                    predicate = predicate.And(i => i.registrationno.ToString() == model.registrationno);
                }

                // quotationno
                if (model.quotationno != null)
                {
                    predicate = predicate.And(i => i.enquiryno.ToString() == model.quotationno.ToString());
                }

                //fromdate todate
                if (model.fromdate != null && model.todate != null)
                {
                    predicate = predicate.And(i => EntityFunctions.TruncateTime(i.createdon) >= EntityFunctions.TruncateTime(model.fromdate) && EntityFunctions.TruncateTime(i.createdon) <= EntityFunctions.TruncateTime(model.todate));
                }
                predicate = predicate.And(i => i.enquiryby==MySession.UserDetail.userid);
                //status
                //if (model.status != null)
                //{
                //    predicate = predicate.And(i => i.status == model.status);
                //}

                //var getinwardbranch = inwardbranch.Split(new Char[] { ',' }).ToArray();
                //predicate = predicate.And(x => getinwardbranch.Contains(x.andbranchid.ToString()));

                var data = AndEnt.VW_GENERATEDQUOTAIONLIST_POSPWISE.Where(predicate).Take(200).Select(i => i).ToList();

                //spTrough get data
                //List<SP_GENERATEDQUOTAIONLIST_POSPWISE_Result> modelquote = new List<SP_GENERATEDQUOTAIONLIST_POSPWISE_Result>();
                //ANDAPPEntities entity = new ANDAPPEntities();
                //modelquote = entity.SP_GENERATEDQUOTAIONLIST_POSPWISE(model.makeid).ToList();
                //spTrough get data


                //var noofrecords = data.Count.ToString();
                //var noofpages = Math.Ceiling(Convert.ToDecimal(noofrecords) / pagesize);
                //var smedata = data.OrderByDescending(x => x.policyid).Skip(pagesize * (pageno - 1)).Take(pagesize).ToList();

                //ViewBag.noofrecords = noofrecords;
                //ViewBag.noofpages = noofpages.ToString();
                ViewBag.quotedata = data;

            }
            catch (Exception Ex)
            {
                Console.Write(Ex.Message.ToString());
                //LogU.WriteLog(Ex.InnerException.ToString());
            }
        }

        public void FillDropDown_List()
        {
            try
            {
                ViewBag.FillMakeName = objcls.GetAllMake();
                ViewBag.InsuranceCompany = objcls.Get_InsCompany();

            }
            catch (Exception Ex)
            {
                Console.Write(Ex.Message);
                LogU.WriteLog("QuotationController >> FillDropDown_List >>" + Ex.Message);
            }

        }

        [HttpPost]
        public JsonResult GetModel(int makeid)
        {
            var fillmodel = objcls.GetAllModel(makeid);
            //var data = new { modelnames = fillmodel };

            return Json(fillmodel, JsonRequestBehavior.AllowGet);

        }

        [HttpPost]
        public JsonResult GetVariant(int modelid)
        {
            var fillVariant = objcls.GetAllVariantbyModelID(modelid);
            //var data = new { modelnames = fillmodel };

            return Json(fillVariant, JsonRequestBehavior.AllowGet);

        }


        // GET: Qutation/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

    }
}
