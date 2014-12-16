using System;

namespace Pocos
{
    public partial class Address 
    {
        
        public string FullAddress
        {
            get
            {
                try
                {
                    var add = string.Empty;
                    add += Address1 + ", " + City + ", " + State + ", " + Zip + ", " + Country;

                    if (add.Trim().Replace(" ", "") == ",,,,") return "N/A";
                    return add;
                }
                catch (Exception)
                {}
                return "";
            }
        }

        public string FullAddressWithHTML
        {
            get
            {
                try
                {
                    var add = string.Empty;
                    add += "<span>" + Address1 + "</span><br /><span>" + City + ", " + State + " " + Zip + "</span><br /><span>" + Country + "</span>";

                    if (add.Trim().Replace(" ", "") == ",,,,") return "N/A";
                    return add;
                }
                catch (Exception)
                { }
                return "";
            }
        }

        public string FullName
        {
            get
            {
                try
                {
                    if(!string.IsNullOrEmpty(FName) && !string.IsNullOrEmpty(LName))
                    return FName + " " + LName;
                }
                catch (Exception)
                {}
                return "";
            }
        }

        public bool IsUSAddress
        {
            get { return Country.Trim().ToLower() == "us"; }
        }

        public bool isMilitaryAddress
        {
            get { return City.Trim().ToLower() == "apo" || City.Trim().ToLower() == "fpo" || City.Trim().ToLower() == "dpo"; }
        }



    }
}
