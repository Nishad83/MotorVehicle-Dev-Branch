namespace AndWebApi.Models
{
    #region namespace
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;
    #endregion
    public class VehicleAddressDetails
    {

        /// <summary>
        /// get or set district.
        /// </summary>
        public string district { get; set; }
        /// <summary>
        /// get or set address one.
        /// </summary>
        public string Address1 { get; set; }

        /// <summary>
        /// get or set address two.
        /// </summary>
        public string Address2 { get; set; }

        /// <summary>
        /// get or set address three.
        /// </summary>
        public string Address3 { get; set; }
        public string Area { get; set; }

        /// <summary>
        /// get or set pincode.
        /// </summary>
        public string Pincode { get; set; }

        /// <summary>
        /// get or set country.
        /// </summary>
        public string Country { get; set; }

        /// <summary>
        /// get or set state.
        /// </summary>
        public string State { get; set; }

        /// <summary>
        /// get or set city.
        /// </summary>
        public string City { get; set; }
        public string Statecode { get; set; }
        public string Citycode { get; set; }

        public RequestType RequestType { get; set; }
    }

    public class CustomerAddressDetails 
    {
        /// <summary>
        /// get or set registration address is same as vehicle address. 
        /// </summary>
        public bool IsRegistrationAddressSame { get; set; }

        /// <summary>
        /// get or set address one.
        /// </summary>
        public string Address1 { get; set; }

        /// <summary>
        /// get or set address two.
        /// </summary>
        public string Address2 { get; set; }

        /// <summary>
        /// get or set address three.
        /// </summary>
        public string Address3 { get; set; }
        public string Area { get; set; }
        /// <summary>
        /// get or set pincode.
        /// </summary>
        public string Pincode { get; set; }

        /// <summary>
        /// get or set country.
        /// </summary>
        public string Country { get; set; }

        /// <summary>
        /// get or set state.
        /// </summary>
        public string State { get; set; }

        /// <summary>
        /// get or set city.
        /// </summary>
        public string City { get; set; }

        public RequestType RequestType { get; set; }

        public string Statecode { get; set; }
        public string Citycode { get; set; }
        public string district { get; set; }
    }
}