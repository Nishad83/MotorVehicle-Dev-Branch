using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
 
namespace DAL
{
    public class DAL_CommonCls :IDisposable
    {
        ANDAPPEntities AndEnt = new ANDAPPEntities();

        #region Added By Pratik
        //added by pratik
        public List<VARIANT_ANDAPP> GetAllVariantbyModelID(long modelid)
        {
            List<VARIANT_ANDAPP> variant = new List<VARIANT_ANDAPP>();
            try
            {
                variant = (from db in AndEnt.VARIANT_ANDAPP
                           where db.modelid == modelid && db.status == true
                           select db).ToList();

            }
            catch (Exception Ex)
            {
                Console.WriteLine(Ex.Message.ToString());
            }
            return variant;
        }

        #endregion
        public List<COMPANY_WISE_STATE_MASTER> GetStateCompanywise(long comid)
        {
            List<COMPANY_WISE_STATE_MASTER> model = new List<COMPANY_WISE_STATE_MASTER>();
            try
            {
                model = AndEnt.SP_COMPANY_WISE_STATE_MASTER(comid).Select(x => new COMPANY_WISE_STATE_MASTER { compid = x.compid, statename = x.statename, value = x.value, id = (long)x.mstateid, status = x.status }).ToList();
            }
            catch (Exception Ex)
            {
                Console.WriteLine(Ex.Message.ToString());
            }
            return model;
        }
        public List<ADDONMASTER> GetAddonList()
        {
            List<ADDONMASTER> data = new List<ADDONMASTER>();
            data = AndEnt.ADDONMASTERs.Where(x => x.status == true && x.type == "CHECK").ToList();
            return data;
        }
        public List<STATEMASTER> GetAllState(long pospstateid)
        {
            List<STATEMASTER> state = new List<STATEMASTER>();
            try
            {
                //long pospstateid = pospstateid;
                //state = (from db in AndEnt.STATEMASTERs select db).ToList();
                state = AndEnt.STATEMASTERs.OrderByDescending(x => x.stateid == pospstateid).ThenBy(x => x.statename).ToList();
                //state = (from db in AndEnt.STATEMASTERs select db).ToList();
            }
            catch (Exception Ex)
            {
                Console.WriteLine(Ex.Message.ToString());
            }
            return state;
        }

           public List<RTOMASTER_ANDAPP> GetAllRto(long stateid,string name)
        {

            List<RTOMASTER_ANDAPP> rto = new List<RTOMASTER_ANDAPP>();
            try
            {
                  rto = (from db in AndEnt.RTOMASTER_ANDAPP
                           where db.stateid == stateid && db.status == true
                           select db).ToList();
            }
            catch (Exception Ex)
            {
                Console.WriteLine(Ex.Message.ToString());
            }
            return rto;
        }


        public List<MAKE_ANDAPP> GetAllMake()
        {
            List<MAKE_ANDAPP> make = new List<MAKE_ANDAPP>();
            try
            {
                    make = (from db in AndEnt.MAKE_ANDAPP
                            where db.status == true
                            select db).ToList();
                
            }
            catch (Exception Ex)
            {
                Console.WriteLine(Ex.Message.ToString());
            }
            return make;
        }
        public List<VW_TOP10MAKE> GetMakeWithLogo()
        {
            List<VW_TOP10MAKE> make = new List<VW_TOP10MAKE>();
            try
            {
                make = (from db in AndEnt.VW_TOP10MAKE
                        
                        select db).OrderByDescending(x => x.MOST_FREQUENT).ToList();
                //make = (from db in AndEnt.MAKE_ANDAPP
                //        where db.status == true && db.haslogo==true
                //        select db).ToList();
            }
            catch (Exception Ex)
            {
                Console.WriteLine(Ex.Message.ToString());
            }
            return make;
        }
        public List<MODEL_ANDAPP> GetAllModel(long makeid)
        {
            List<SP_GETTOP10MODELBYMAKEID> topmodel = new List<SP_GETTOP10MODELBYMAKEID>();

            List<MODEL_ANDAPP> model = new List<MODEL_ANDAPP>();
            try
            {

                topmodel = AndEnt.SP_GETTOP10MODELBYMAKEID(makeid).OrderByDescending(x => x.MOST_FREQUENT).ToList();
                var id = topmodel.Select(x => x.modelid).ToList();

                //var listid= id.ToArray();
                if (id!=null)
                {
                    model = (from db in AndEnt.MODEL_ANDAPP
                             where db.makeid == makeid && db.status == true
                             select db).OrderByDescending(x => id.Contains(x.modelid)).ToList();
                }
                else
                {
                    model = (from db in AndEnt.MODEL_ANDAPP
                             where db.makeid == makeid && db.status == true
                             select db).ToList();
                }
              
               
            }
            catch (Exception Ex)
            {
                Console.WriteLine(Ex.Message.ToString());
            }
            return model;
        }
        public List<SP_GETFUELTYPE_BY_MODELID> GetFuel(long modelid)
        {
            List<SP_GETFUELTYPE_BY_MODELID> model = new List<SP_GETFUELTYPE_BY_MODELID>();
            try
            {
                model = AndEnt.SP_GETFUELTYPE_BY_MODELID1(modelid).ToList();

            }
            catch (Exception Ex)
            {
                Console.WriteLine(Ex.Message.ToString());
            }
            return model;
        }
        public List<VARIANT_ANDAPP> GetAllVariant(long modelid,string fueltype)
        {
            List<SP_GETTOP10VARIANTBYMODELID_Result> topvariant = new List<SP_GETTOP10VARIANTBYMODELID_Result>();
            List<VARIANT_ANDAPP> variant = new List<VARIANT_ANDAPP>();
            try
            {
                topvariant = AndEnt.SP_GETTOP10VARIANTBYMODELID(modelid, fueltype).OrderByDescending(x => x.MOST_FREQUENT).ToList();
                var id = topvariant.Select(x => x.variantid).ToList();
                if (id!=null)
                {
                    variant = (from db in AndEnt.VARIANT_ANDAPP
                               where db.modelid == modelid && db.fueltype == fueltype && db.status == true
                               select db).OrderByDescending(x => id.Contains(x.variantid)).ToList();
                }
                else
                {
                    variant = (from db in AndEnt.VARIANT_ANDAPP
                               where db.modelid == modelid && db.fueltype == fueltype && db.status == true
                               select db).ToList();
                }
          
               
            }
            catch (Exception Ex)
            {
                Console.WriteLine(Ex.Message.ToString());
            }
            return variant;
        }

        public List<INSURANCECOMPANY> Get_InsCompany()
        {
            List<INSURANCECOMPANY> objlist = new List<INSURANCECOMPANY>();
              objlist = (from db in AndEnt.INSURANCECOMPANies
                           where db.status == true && db.isactive==true
                         orderby db.companyname
                         select db).ToList();

            return objlist;
        }

        public List<SP_GETFUELTYPE_BY_MODELID> Get_FuelTYpe(long modelid)
        {
            List<SP_GETFUELTYPE_BY_MODELID> objlist = new List<SP_GETFUELTYPE_BY_MODELID>();
            objlist =AndEnt.SP_GETFUELTYPE_BY_MODELID1(modelid).ToList();
            return objlist;
        }



        public List<NOMINEE_RELATIONSHIP> GetNomineeRelation(long comid)
        {
            List<NOMINEE_RELATIONSHIP> model = new List<NOMINEE_RELATIONSHIP>();
            try
            {
                model = AndEnt.SP_COMPANY_WISE_NOMINEERELATION(comid).Select(x => new NOMINEE_RELATIONSHIP { comid = x.comid, name = x.name, value = x.value, id = x.id, status = x.status }).ToList();

            }
            catch (Exception Ex)
            {
                Console.WriteLine(Ex.Message.ToString());
            }
            return model;
        }

        public List<OCCUPATION> GetOccupation(long comid)
        {
            List<OCCUPATION> model = new List<OCCUPATION>();
            try
            {
                model = AndEnt.SP_COMPANY_WISE_OCCUPATION(comid).Select(x => new OCCUPATION { comid = x.comid, name = x.name, value = x.value, id = x.id, status = x.status }).ToList();
            }
            catch (Exception Ex)
            {
                Console.WriteLine(Ex.Message.ToString());
            }
            return model;
        }
        public List<MARITALSTATUSMASTER> GetMarritalStatus(long comid)
        {
            List<MARITALSTATUSMASTER> model = new List<MARITALSTATUSMASTER>();
            try
            {
                model = AndEnt.SP_COMPANY_WISE_MARITALSTATUS(comid).Select(x => new MARITALSTATUSMASTER { compid = x.compid, name = x.name, value = x.value, id = x.id, status = x.status }).ToList();
            }
            catch (Exception Ex)
            {
                Console.WriteLine(Ex.Message.ToString());
            }
            return model;
        }
        public List<SALUTATIONMASTER> GetSalutation(long comid)
        {
            List<SALUTATIONMASTER> model = new List<SALUTATIONMASTER>();
            try
            {
                model = AndEnt.SP_COMPANY_WISE_SALUTATION(comid).Select(x => new SALUTATIONMASTER { compid = x.compid, name = x.name, value = x.value, id = x.id, status = x.status }).ToList();
            }
            catch (Exception Ex)
            {
                Console.WriteLine(Ex.Message.ToString());
            }
            return model;
        }


        public List<FINANCERMASTER> GetFinancerLIst(long comid)
        {
            List<FINANCERMASTER> model = new List<FINANCERMASTER>();
            try
            {
                model = AndEnt.SP_COMPANY_WISE_FINANCERMASTER(comid).Select(x => new FINANCERMASTER { comid = x.comid, name = x.name, value = x.value, id = x.id, status = x.status }).ToList();
            }
            catch (Exception Ex)
            {
                Console.WriteLine(Ex.Message.ToString());
            }
            return model;
        }
        public string GetCompanyIdbyName(string comp)
        {
            string compname = "";
            try
            {
                compname = AndEnt.INSURANCECOMPANies.Where(x => x.shortname == comp && x.status == true && x.isactive == true).FirstOrDefault().id.ToString();
            }
            catch (Exception Ex)
            {
                Console.WriteLine(Ex.Message.ToString());
            }
            return compname;
        }
        public int GetModelid(int variantid)
        {
            var model = (int)AndEnt.VARIANT_ANDAPP.Where(x => x.variantid == variantid).FirstOrDefault().modelid;
            return model;
        }
        public void InsertEnqDetails(string req, string res, string enqId, int type, string firstquote, long enqby)
        {
            AndEnt.SP_POLICY_ENQUIRYDETAILS(enqId, req, res, type, firstquote,enqby);
            //AndEnt.SaveChanges();
        }
        public POLICY_ENQUIRY GetQuoteByEnqNo(string enqno)
        {
            POLICY_ENQUIRY request = new POLICY_ENQUIRY();
            request = AndEnt.POLICY_ENQUIRY.Where(x => x.enquiryno == enqno).OrderByDescending(x => x.enquiryid).FirstOrDefault();
            return request;
        }
        private bool disposedValue; // To detect redundant calls
        // IDisposable
        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposedValue)
            {
                if (disposing)
                {
                }
            }
            this.disposedValue = true;
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
   
}
