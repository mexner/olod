using System;
using System.Linq;

namespace Pocos
{
    public partial class Donor 
    {
        public string FullName
        {
            get { return FName + " " + LName; }
        }

        public bool HasAddressType(Enum_AddressType type)
        {
            return DonorAddresses.Any(x => x.AddressType == type); 
        }

        public DonorAddress GetAddress(Enum_AddressType type)
        {
            if (HasAddressType(type))
                return DonorAddresses.First(x => x.AddressType == type);
            else
            {
                throw new Exception("Donor does not have address type.");
            }
        }

        public bool HasCompleteInformation(bool fname, bool lname, bool email, bool phone, bool billingAddress, bool shippingAddress)
        {
            if (billingAddress && shippingAddress)
                return HasCompleteInformation(fname, lname, email, phone, true, true, true, true, true, true, true, true, true, true);

            return billingAddress ?
                HasCompleteInformation(fname, lname, email, phone, true, true, true, true, true, false, false, false, false, false) :
                HasCompleteInformation(fname, lname, email, phone, false, false, false, false, false, true, true, true, true, true);
        }


        public bool HasCompleteInformation(bool fname, bool lname, bool email, bool phone, bool add1, bool country,
                                           bool city, bool state, bool zip, bool shipAdd1, bool shipCity, bool shipState,
                                           bool shipZip, bool shipCountry)
        {
            if (fname && string.IsNullOrEmpty(FName)) return false;
            if (lname && string.IsNullOrEmpty(LName)) return false;
            if (email && string.IsNullOrEmpty(Email)) return false;
            if (phone && string.IsNullOrEmpty(Phone)) return false;

            var billing = DonorAddresses.FirstOrDefault(x => x.AddressType == Enum_AddressType.Billing);
            if (billing != null)
            {
                if (add1 && string.IsNullOrEmpty(billing.Address1)) return false;
                if (country && string.IsNullOrEmpty(billing.Country)) return false;
                if (city && string.IsNullOrEmpty(billing.City)) return false;
                if (state && string.IsNullOrEmpty(billing.State)) return false;
                if (zip && string.IsNullOrEmpty(billing.Zip)) return false;
            }
            else
            {
                if (add1 || country || city || state || zip) return false;
            }

            var shipping = DonorAddresses.FirstOrDefault(x => x.AddressType == Enum_AddressType.Billing);
            if (shipping != null)
            {
                if (shipAdd1 && string.IsNullOrEmpty(shipping.Address1)) return false;
                if (shipCountry && string.IsNullOrEmpty(shipping.Country)) return false;
                if (shipCity && string.IsNullOrEmpty(shipping.City)) return false;
                if (shipState && string.IsNullOrEmpty(shipping.State)) return false;
                if (shipZip && string.IsNullOrEmpty(shipping.Zip)) return false;
            }
            else
            {
                if (shipAdd1 || shipCountry || shipCity || shipState || shipZip) return false;
            }

            return true;
        }
    }
}
