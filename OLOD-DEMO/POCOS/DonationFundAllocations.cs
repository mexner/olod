//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Pocos
{
    using System;
    using System.Collections.Generic;

    public partial class DonationFundAllocation
    {
        public DonationFundAllocation()
        {
            this.Amount = 0m;
        }

        public long Id { get; set; }
        public long DonationId { get; set; }
        public long FundId { get; set; }
        public decimal Amount { get; set; }

        public virtual Donation Donation { get; set; }
        public virtual Fund Fund { get; set; }
    }
}
