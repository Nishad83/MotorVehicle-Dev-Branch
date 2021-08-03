using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AndWebApi
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
    //public enum Company
    //{
    //    /// <summary>
    //    /// Bajaj Allianz General Insurance Co. Ltd.
    //    /// </summary>
    //    BAJAJ = 1,

    //    /// <summary>
    //    /// HDFC ERGO Health Insurance Co.Ltd.
    //    /// </summary>
    //    HDFC_HEALTH = 2,

    //    /// <summary>
    //    /// Bharti Axa General Insurance Co. Ltd.
    //    /// </summary>
    //    BHARTI = 3,

    //    /// <summary>
    //    /// Cholamandalam Ms General Insurance Co. Ltd.
    //    /// </summary>
    //    CHOLA = 4,

    //    /// <summary>
    //    /// Future Generali India Insurance Co. Ltd.
    //    /// </summary>
    //    FGI = 5,

    //    /// <summary>
    //    /// Go Digit General Insurance Limited.
    //    /// </summary>
    //    DIGIT = 6,

    //    /// <summary>
    //    /// Gujrat Government Insurance Fund.
    //    /// </summary>
    //    GUJRAT = 7,

    //    /// <summary>
    //    /// HDFC ERGO General Insurance Co. Ltd.
    //    /// </summary>
    //    HDFC = 8,

    //    /// <summary>
    //    /// ICICI Lombard General Insurance Co. Ltd.
    //    /// </summary>
    //    ICICI = 9,

    //    /// <summary>
    //    /// Iffco Tokio General Insurance Co. Ltd.
    //    /// </summary>
    //    ITGI = 10,

    //    /// <summary>
    //    /// Kotak Mahindra General Insurance.
    //    /// </summary>
    //    KOTAK = 11,

    //    /// <summary>
    //    /// L&T General Insurance Co. Ltd.
    //    /// </summary>
    //    LANDT = 12,

    //    /// <summary>
    //    /// Liberty General Insurance Ltd.
    //    /// </summary>
    //    LIBERTY = 13,

    //    /// <summary>
    //    /// Magma Hdi General Insurance Co. Ltd.
    //    /// </summary>
    //    MAGMA = 14,

    //    /// <summary>
    //    /// National Insurance Co. Ltd.
    //    /// </summary>
    //    NATIONAL = 15,

    //    /// <summary>
    //    /// Raheja Qbe General Insurance Co. Ltd.
    //    /// </summary>
    //    RAHEJA = 16,

    //    /// <summary>
    //    /// Reliance General Insurance Co. Ltd.
    //    /// </summary>
    //    RELIANCE = 17,

    //    /// <summary>
    //    /// Religare General Insurance Co.Ltd.
    //    /// </summary>
    //    CARE = 18,

    //    /// <summary>
    //    /// Royal Sundaram Alliance Insurance Co. Ltd.
    //    /// </summary>
    //    ROYAL = 19,

    //    /// <summary>
    //    /// Sbi General Insurance Co. Ltd.
    //    /// </summary>
    //    SBI = 20,

    //    /// <summary>
    //    /// Shriram General Insurance Co. Ltd.
    //    /// </summary>
    //    SHRIRAM = 21,

    //    /// <summary>
    //    /// Tata Aig General Insurance Co. Ltd.
    //    /// </summary>
    //    TATA = 22,

    //    /// <summary>
    //    /// The New India Assurance Co. Ltd.
    //    /// </summary>
    //    NIA = 23,

    //    /// <summary>
    //    /// The Oriental Insurance Co. Ltd.
    //    /// </summary>
    //    ORIENTAL = 24,

    //    /// <summary>
    //    /// United India Insurance Co. Ltd.
    //    /// </summary>
    //    UNITED = 25,

    //    /// <summary>
    //    /// Universal Sompo General Insurance Co. Ltd.
    //    /// </summary>
    //    SOMPO = 26

    //}

    public enum Status
    {
        Success = 1,
        Fail = 2,
        Unknown = 3
    }

}