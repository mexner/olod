﻿//------------------------------------------------------------------------------
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
    
    public partial class Donor
    {
        public Donor()
        {
            this.Donations = new HashSet<Donation>();
            this.DonorAddresses = new HashSet<DonorAddress>();
        }
    
        public long Id { get; set; }
        public string FName { get; set; }
        public string LName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public bool NesletterOptIn { get; set; }
        public string Notes { get; set; }
    
        public virtual ICollection<Donation> Donations { get; set; }
        public virtual ICollection<DonorAddress> DonorAddresses { get; set; }
    }
}
