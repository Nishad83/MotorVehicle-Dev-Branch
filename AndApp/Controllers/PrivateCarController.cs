using AndApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using DAL;

namespace AndApp.Controllers
{
    public class PrivateCarController : Controller
    {
        DAL_CommonCls objcls = new DAL_CommonCls();

        // GET: PrivateCar

        public ActionResult  CarDetails()
        {
            BindDropDown();
            return View();
        }
        public void BindDropDown()
        {
           ViewBag.makelist = GetMakeSelect();
            var myDate = DateTime.Now;

            List<SelectListItem> YearList = new List<SelectListItem>();
            for (int i = 0; i < 15; i++)
            {
                SelectListItem objselect = new SelectListItem();
                objselect.Text = myDate.ToString("yyyy");
                objselect.Value = myDate.Year.ToString();
                YearList.Add(objselect);
                myDate = myDate.AddYears(-1);
            }
            ViewBag.YearList = YearList.ToArray();
        }
        public JsonResult GetRto(int stateid)
        {
         var obj = objcls.GetAllRto(stateid,"");
          return Json(obj, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public JsonResult Getstate()
        {
            var obj = objcls.GetAllState().ToArray();
            return Json(obj, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetMake()
        {
            var obj = objcls.GetMakeWithLogo().ToArray();
            return Json(obj, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public JsonResult GetModel(int makeid)
        {
            var obj = objcls.GetAllModel(makeid);
            var modelselect= (from db in obj select new
            {
                id = db.modelid,
                text = db.modelname

            }).ToList();
            return Json(new { model = obj.ToArray(), selectmodel = modelselect }, JsonRequestBehavior.AllowGet);
        }
     
        public JsonResult GetVariant(int modelid,string fueltype)
        {
            var obj = objcls.GetAllVariant(modelid,fueltype);
            var variantselect = (from db in obj
                               select new
                               {
                                   id = db.variantid,
                                   text = db.variantname

                               }).ToList();
            return Json(new { variant = obj.ToArray(), selectvariant = variantselect }, JsonRequestBehavior.AllowGet);
        }
        public List<SelectListItem> GetMakeSelect()
        {
            List<SelectListItem> selectlist = new List<SelectListItem>();
            try
            {
               var data = objcls.GetAllMake();
                for (int i = 0; i < data.Count; i++)
                {
                    SelectListItem objselect = new SelectListItem();
                    objselect.Value = data[i].makeid.ToString();
                    objselect.Text = data[i].makename.ToString();
                    selectlist.Add(objselect);
                }
            }
            catch (Exception Ex)
            {
                Console.Write(Ex.Message.ToString());
              
            }
            return selectlist;
        }
        public JsonResult GetManufacturingYear()
        {
            var myDate = DateTime.Now;
            List<SelectListItem> YearList = new List<SelectListItem>();
            for (int i = 0; i < 15; i++)
            {
                SelectListItem objselect = new SelectListItem();
                objselect.Text = myDate.ToString("yyyy");
                objselect.Value = myDate.Year.ToString();
                YearList.Add(objselect);
                myDate = myDate.AddYears(-1);
            }
            return Json(YearList.ToArray(), JsonRequestBehavior.AllowGet);
        }

    }
}