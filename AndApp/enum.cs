using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AndApp
{
    public enum RequestType
    {
        Quote = 0,
        Fullquote = 1,
        Proposal = 2,
        PolicyPdf = 3
    }

    public enum Product
    {
        Motor = 1,
        Health = 2,
        Travel = 3
    }

    public enum SubProduct
    {
        PrivateCar = 1,
        TwoWheeler = 2,
        IndividualHealth = 3,
        FamilyHealth = 4
    }

    public enum Company
    {
        /// <summary>
        /// Bajaj Allianz General Insurance Co. Ltd.
        /// </summary>
        BAJAJ,

        /// <summary>
        /// HDFC ERGO Health Insurance Co.Ltd.
        /// </summary>
        HDFC_HEALTH,

        /// <summary>
        /// Bharti Axa General Insurance Co. Ltd.
        /// </summary>
        BHARTI,

        /// <summary>
        /// Cholamandalam Ms General Insurance Co. Ltd.
        /// </summary>
        CHOLA,

        /// <summary>
        /// Future Generali India Insurance Co. Ltd.
        /// </summary>
        FGI,

        /// <summary>
        /// Go Digit General Insurance Limited.
        /// </summary>
        DIGIT,

        /// <summary>
        /// Gujrat Government Insurance Fund.
        /// </summary>
        GUJRAT,

        /// <summary>
        /// HDFC ERGO General Insurance Co. Ltd.
        /// </summary>
        HDFC,

        /// <summary>
        /// ICICI Lombard General Insurance Co. Ltd.
        /// </summary>
        ICICI,

        /// <summary>
        /// Iffco Tokio General Insurance Co. Ltd.
        /// </summary>
        ITGI,

        /// <summary>
        /// Kotak Mahindra General Insurance.
        /// </summary>
        KOTAK,

        /// <summary>
        /// L&T General Insurance Co. Ltd.
        /// </summary>
        LANDT,

        /// <summary>
        /// Liberty General Insurance Ltd.
        /// </summary>
        LIBERTY,

        /// <summary>
        /// Magma Hdi General Insurance Co. Ltd.
        /// </summary>
        MAGMA,

        /// <summary>
        /// National Insurance Co. Ltd.
        /// </summary>
        NATIONAL,

        /// <summary>
        /// Raheja Qbe General Insurance Co. Ltd.
        /// </summary>
        RAHEJA,

        /// <summary>
        /// Reliance General Insurance Co. Ltd.
        /// </summary>
        RELIANCE,

        /// <summary>
        /// Religare General Insurance Co.Ltd.
        /// </summary>
        CARE,

        /// <summary>
        /// Royal Sundaram Alliance Insurance Co. Ltd.
        /// </summary>
        ROYAL,

        /// <summary>
        /// Sbi General Insurance Co. Ltd.
        /// </summary>
        SBI,

        /// <summary>
        /// Shriram General Insurance Co. Ltd.
        /// </summary>
        SHRIRAM,

        /// <summary>
        /// Tata Aig General Insurance Co. Ltd.
        /// </summary>
        TATA,

        /// <summary>
        /// The New India Assurance Co. Ltd.
        /// </summary>
        NIA,

        /// <summary>
        /// The Oriental Insurance Co. Ltd.
        /// </summary>
        ORIENTAL,

        /// <summary>
        /// United India Insurance Co. Ltd.
        /// </summary>
        UNITED,

        /// <summary>
        /// Universal Sompo General Insurance Co. Ltd.
        /// </summary>
        SOMPO

    }

    public enum Status
    {
        Success = 1,
        Fail = 2,
        Unknown = 3
    }

}