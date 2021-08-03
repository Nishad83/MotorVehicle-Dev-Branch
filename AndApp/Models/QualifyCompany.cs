using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AndApp.Models
{
    /// <summary>
    /// This class contains properties for qualify company.    
    /// </summary>
    public class QualifyCompany
    {
        /// <summary>
        /// get or set is break-in.
        /// </summary>
        public bool IsBreakin { get; set; }

        /// <summary>
        /// get or set variant id.
        /// </summary>
        public int VariantId { get; set; }

        /// <summary>
        /// get or set rto id.
        /// </summary>
        public int RtoId { get; set; }
    }
}