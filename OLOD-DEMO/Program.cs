using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using OLOD_DEMO.OLODWebService;
using Pocos;

namespace OLOD_DEMO
{
    class Program
    {
        protected static OrangeLeapClient _client;
        private static string _apiUsername = ConfigurationSettings.AppSettings["api_username"];
        private static string _apiPassword = ConfigurationSettings.AppSettings["api_password"];
        private static Enum_HowToHandleOverMatch _howToHandleOverMatch = Enum_HowToHandleOverMatch.Use_Oldest_Record;
        
        public static void Main(string[] args)
        {
            _client = new OrangeLeapClient();
            EstablishConnection();

            var donor = new Pocos.Donor();
            var donation = new Pocos.Donation();
            InitObjects(ref donation, ref donor);

            SendDonation(donation);
        }
        
        public static void SendDonation(Donation donation)
        {
            constituent constt;

            Console.WriteLine("Looking to match donor in OLOD. ");

            //FName, LName, Email, Phone, Address
            var donors = Find(donation.Donor, true, true, true, true, true);
            if (donors.Any())
            {
                Console.WriteLine("Located donor(s) by matching FName, LName, Email, Phone, & Address. ");
            }


            if (!donors.Any())
            {
                //Fname, Lname, Email, Address
                donors = Find(donation.Donor, true, true, true, false, true);
                if (donors.Any())
                {
                    Console.WriteLine("Located donor(s) by matching FName, LName, Email, & Address. ");
                }
            }
            if (!donors.Any())
            {
                //FName, LName, Email
                donors = Find(donation.Donor, true, true, true, false, false);
                if (donors.Any())
                {
                    Console.WriteLine("Located donor(s) by matching FName, LName, & Email. ");
                }
            }
            if (!donors.Any())
            {
                //FName, Lname, Address
                donors = Find(donation.Donor, true, true, false, false, true);
                if (donors.Any())
                {
                    Console.WriteLine("Located donor(s) by matching FName, LName, & Address. ");
                }
            }
            if (!donors.Any())
            {
                //Email
                donors = Find(donation.Donor, false, false, true, false, false);
                if (donors.Any())
                {
                    Console.WriteLine("Located donor(s) by matching Email. ");
                }
            }

            if (donors.Any())
            {

                if (donors.Count == 1)
                {
                    constt = donors.First();
                    //UpdateConstituent(d.Donor, constt);
                    Console.WriteLine("Found 1 donor, perfect match! ");
                }
                else
                {
                    Console.WriteLine("Found " + donors.Count + " donors, attempting to resolve. <br />");
                    Console.WriteLine("<ol>");
                    foreach (var d in donors)
                    {
                        Console.WriteLine("<li>");
                        Console.WriteLine(d.id + " | " + d.firstName + " " + d.lastName + " | " +
                                         d.primaryEmail + " | " + d.primaryPhone + " | " +
                                         d.primaryAddress.addressLine1 + ", " + d.primaryAddress.city + ", " +
                                         d.primaryAddress.stateProvince + " " + d.primaryAddress.postalCode +
                                         "<br />" + d.updateDate.ToString());
                        Console.WriteLine("</li>");
                    }
                    Console.WriteLine("</ol>");

                    switch (_howToHandleOverMatch)
                    {
                        case Enum_HowToHandleOverMatch.Use_Oldest_Record:
                            constt = donors.OrderBy(x => x.updateDate).First();
                            Console.WriteLine("Resolving by using oldest account. ");
                            UpdateConstituent(donation.Donor, ref constt);
                            break;
                        case Enum_HowToHandleOverMatch.Use_Newest_Record:
                            constt = donors.OrderByDescending(x => x.updateDate).First();
                            Console.WriteLine("Resolving by using newest account. ");
                            UpdateConstituent(donation.Donor, ref constt);
                            break;
                        case Enum_HowToHandleOverMatch.Create_New_Record:
                            constt = CreateConstituent(donation.Donor);
                            Console.WriteLine("Resolving by creating a new account. ");
                            break;
                        case Enum_HowToHandleOverMatch.Create_New_Record_And_Be_Notified:
                            Console.WriteLine("Resolving by using oldest account & emailing admin staff. ");
                            constt = CreateConstituent(donation.Donor);
                            SendOverMatchEmail(donors, donation);
                            UpdateConstituent(donation.Donor, ref constt);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
            }
            else
            {
                Console.WriteLine("No donor match found, creating new account.");
                //create new donor
                constt = CreateConstituent(donation.Donor);
                Console.WriteLine("Account created, new ID: " + constt.id);
            }

            //now we have our constituent in OLOD, lets add our new donation.
            Console.WriteLine("Sending gift to OLOD.");
            AddGiftToConstituent(donation, ref constt);
        }

        private static void SendOverMatchEmail(IReadOnlyCollection<constituent> constts, Donation d)
        {
            var subj = "OLOD Integration OverMatch (" + constts.Count + ")";
            var body = "Here is a list of all the matches we located for this donor.  <br /><br />";
            body += "<ol>";
            foreach (var c in constts)
            {
                body += "<li>Name: " + c.firstName + " " + c.lastName + "<br />";
                body += "Id: " + c.id + "<br />";
                body += "Email: " + c.emails.FirstOrDefault(x => x.primary) + "<br />";
                body += "Phone: " + c.phones.FirstOrDefault(x => x.primary) + "<br />";

                var add = c.addresses.FirstOrDefault(x => x.primary);
                if (add != null)
                {
                    var a = add.addressLine1 + ", " + add.city + ", " + add.stateProvince + ", " + add.postalCode + ", " +
                            add.country;
                    body += "Address: " + a + "<br />";
                }
                body += "</li>";
            }

            body += "<br />You'll need to take action to manually add this donation to the appropriate constituent below. <br /><br />";
           
            var email = new System.Net.Mail.MailMessage
            {
                Subject = subj,
                Body = body,
                IsBodyHtml = true
            };
            email.To.Add("info@some-email.com");

            //send email
        }

        private static constituent CreateConstituent(Donor d)
        {
            var constt = new constituent();
                
            try
            {
                var site = new site();
                var fieldMap = new abstractCustomizableEntityEntry[0];

                site.name = "RaiseDonors";

                constt.firstName = d.FName;
                constt.lastName = d.LName;
                constt.constituentType = "individual";
                constt.site = site;
                constt.customFieldMap = fieldMap;

                AddPhoneToConstituent(d, ref constt, true, false);
                AddEmailToConstituent(d, ref constt, true, false);

                var billing = d.DonorAddresses.FirstOrDefault(x => x.AddressType == Enum_AddressType.Billing);
                if (billing != null) AddAddressToConstituent(billing, ref constt, true, false);

                var shipping = d.DonorAddresses.FirstOrDefault(x => x.AddressType == Enum_AddressType.Shipping);
                if (shipping != null) AddAddressToConstituent(shipping, ref constt, true, false);

                var request = new SaveOrUpdateConstituentRequest {constituent = constt};

                var response = _client.SaveOrUpdateConstituent(request);
                return response.constituent;
            }
            catch (FaultException exception)
            {
                Type exType = exception.GetType();
                MessageFault mf = exception.CreateMessageFault();
                if (mf.HasDetail)
                {
                    var detailedMessage = mf.GetDetail<System.Xml.Linq.XElement>();
                    String message = detailedMessage.FirstNode.ToString();
                }
            }
            catch (Exception exc)
            {
                var txt = exc;
            }
            return null;
        }

        private static void UpdateConstituent(Donor d, ref constituent constt)
        {
            //update fields
        }

        private static void AddPhoneToConstituent(Donor d, ref constituent constt, bool setPrimary = false, bool SendToApi = false)
        {
            try
            {
                var phone = new phone
                {
                    number = d.Phone,
                    inactive = false,
                    primary = setPrimary,
                };

                constt.phones.ToList().Add(phone);

                if (SendToApi)
                {
                    var saveOrUpdateConstituentRequest = new SaveOrUpdateConstituentRequest { constituent = constt };
                    _client.SaveOrUpdateConstituent(saveOrUpdateConstituentRequest);
                }
            }
            catch (FaultException exception)
            {
                Type exType = exception.GetType();
                MessageFault mf = exception.CreateMessageFault();
                if (mf.HasDetail)
                {
                    var detailedMessage = mf.GetDetail<System.Xml.Linq.XElement>();
                    String message = detailedMessage.FirstNode.ToString();
                }
            }
            catch (Exception exc)
            {
                var txt = exc;
            }
        }

        private static void AddEmailToConstituent(Donor d, ref constituent constt, bool setPrimary = false, bool SendToApi = false)
        {
            try
            {
                var email = new email
                {
                    emailAddress = d.Email,
                    emailDisplay = d.FullName,
                    primary = setPrimary,
                };

                constt.emails.ToList().Add(email);

                if (SendToApi)
                {
                    var saveOrUpdateConstituentRequest = new SaveOrUpdateConstituentRequest { constituent = constt };
                    _client.SaveOrUpdateConstituent(saveOrUpdateConstituentRequest);
                }
            }
            catch (FaultException exception)
            {
                Type exType = exception.GetType();
                MessageFault mf = exception.CreateMessageFault();
                if (mf.HasDetail)
                {
                    var detailedMessage = mf.GetDetail<System.Xml.Linq.XElement>();
                    String message = detailedMessage.FirstNode.ToString();
                }
            }
            catch (Exception exc)
            {
                var txt = exc;
            }
        }

        private static void AddAddressToConstituent(Address add, ref constituent constt, bool setPrimary = false, bool sendToApi = false)
        {
            try
            {
                var address = TranslateAddress(add, setPrimary);
                constt.addresses.ToList().Add(address);

                if (sendToApi)
                {
                    var saveOrUpdateConstituentRequest = new SaveOrUpdateConstituentRequest { constituent = constt };
                    _client.SaveOrUpdateConstituent(saveOrUpdateConstituentRequest);
                }
            }
            catch (FaultException exception)
            {
                Type exType = exception.GetType();
                MessageFault mf = exception.CreateMessageFault();
                if (mf.HasDetail)
                {
                    var detailedMessage = mf.GetDetail<System.Xml.Linq.XElement>();
                    String message = detailedMessage.FirstNode.ToString();
                }
            }
            catch (Exception exc)
            {
                var txt = exc;
            }
        }

        private static address TranslateAddress(Address add, bool setPrimary = false)
        {
            var address = new address
            {
                addressLine1 = add.Address1,
                city = add.City,
                stateProvince = add.State,
                country = add.Country,
                postalCode = add.Zip,
                customFieldMap = new abstractCustomizableEntityEntry[0],
                primary = setPrimary,
                inactive = false,
            };

            return address;
        }

        private static void AddGiftToConstituent(Donation donation, ref constituent constt)
        {
            /* 12/16/2014
             * For now, send all of our donations over as "CASH" until OLOD's API can handle pre-processed donations.            * 
             * 
             */

            try
            {
                var g = new gift();
                var ps = new paymentSource();
                var fieldMap = new abstractCustomizableEntityEntry[0];

                ps.constituentId = constt.id;
                ps.paymentType = PaymentType.Cash;
                // donation.PaymentMethod == Enum_PaymentMethod.ACH
                //                                                 ? PaymentType.Check
                //                                                 : PaymentType.CreditCard;
                //ps.creditCardHolderName = donation.Name;
                //ps.creditCardType = Gateway.Misc.GetCreditCardBrandName(donation.CardBrand);
                //ps.creditCardNumberDisplay = donation.Last4ofCC;

                var billing = donation.Donor.DonorAddresses.FirstOrDefault(x => x.AddressType == Enum_AddressType.Billing);
                var nowDate = DateTime.Now;

                g.address = TranslateAddress(billing, false);
                g.amount = donation.Amount;
                g.authCode = donation.AuthorizationNumber;
                g.comments = "Processed by RaiseDonors using card ending in x" + donation.Last4ofCC;
                if (!string.IsNullOrEmpty(donation.Comment)) g.comments += "Donor provided comment: " + donation.Comment;
                //g.createDate = nowDate;
                //g.createDateSpecified = true;
                g.currencyCode = "USD";
                g.customFieldMap = fieldMap;
                g.constituentId = constt.id;
                g.deductible = false;
                g.deductibleAmount = 0.00m;
                //g.donationDate = nowDate;
                //g.donationDateSpecified = true;
                g.email = constt.primaryEmail;
                g.giftStatus = "Paid";  //Paid, Pending, Not Paid 
                g.paymentMessage = "Processed thru RaiseDonors";
                //g.paymentSource = ps;
                g.paymentType = PaymentType.Cash;
                g.phone = constt.primaryPhone;
                g.transactionDate = nowDate;
                g.transactionDateSpecified = true;
                g.txRefNum = donation.TransactionId;

                var s = new site { name = "sandbox" };
                g.site = s;

                //read project codes, and use in setup on which one to use.
                var dLine = new distributionLine
                {
                    customFieldMap = fieldMap,
                    amount = donation.Amount,
                    other_motivationCode = donation.MotivationCode,
                    percentage = 100,
                    motivationCode = "",  //what motivated to give - don't use this one
                    projectCode = "RaiseDonors" //gift designation? 
                };

                //source-code, use customfield on the gift.
                //project, and source will throw error if not pre-existing.

                //throws error 
                //Value cannot be null.
                //Parameter name: source
                g.distributionLines.ToList().Add(dLine);

                var request = new SaveOrUpdateGiftRequest { gift = g, constituentId = constt.id };
                var response = _client.SaveOrUpdateGift(request);

                Console.WriteLine("Successfully sent gift to OLOD, gift ID: " + response.gift.id + ".");
            }
            catch (FaultException exception)
            {
                Type exType = exception.GetType();
                MessageFault mf = exception.CreateMessageFault();
                if (mf.HasDetail)
                {
                    var detailedMessage = mf.GetDetail<System.Xml.Linq.XElement>();
                    String message = detailedMessage.FirstNode.ToString();
                }
            }
            catch (Exception exc)
            {
                var text = exc;
            }
            
        }
      
        private static List<constituent> Find(Donor d, bool includeFname = true, bool includeLname = true, bool includeEmail = true, bool includePhone = true, bool includeAddress = true)
        {
            var results = new List<constituent>();
            try
            {
                var request = new FindConstituentsRequest();
                if (includeFname && !string.IsNullOrEmpty(d.FName)) request.firstName = d.FName;
                if (includeLname && !string.IsNullOrEmpty(d.LName)) request.lastName = d.LName;

                //VALIDATION ERROR
                if (includeEmail && !string.IsNullOrEmpty(d.Email)) 
                    request.primaryEmail = new email() 
                                                    { 
                                                        emailAddress = d.Email, 
                                                        primary = true, 
                                                        createDate = DateTime.Now, 
                                                        updateDate = DateTime.Now, 
                                                        customFieldMap = new abstractCustomizableEntityEntry[0]
                                                    };

                if (includePhone && !string.IsNullOrEmpty(d.Phone)) 
                    request.primaryPhone = new phone()
                                                    {
                                                        number = d.Phone,
                                                        primary = true, 
                                                        createDate = DateTime.Now, 
                                                        updateDate = DateTime.Now, 
                                                        customFieldMap = new abstractCustomizableEntityEntry[0]
                                                    };
                //VALIDATION ERROR
                var billingAddress = d.GetAddress(Enum_AddressType.Billing);
                if (billingAddress != null && includeAddress)
                {
                    request.primaryAddress = new address
                                                 {
                                                     postalCode = billingAddress.Zip,
                                                     addressLine1 = billingAddress.Address1,
                                                     city = billingAddress.City,
                                                     createDate = DateTime.Now, 
                                                     updateDate = DateTime.Now, 
                                                     customFieldMap = new abstractCustomizableEntityEntry[0]
                                                };
                }

            
                var response = _client.FindConstituents(request);

                //only grab active accounts
                response = response.Where(x => !x.deleted && !x.inactive).ToArray();

                if (response.Any())
                {
                    results.AddRange(response);
                }
            }
            catch (FaultException exception)
            {
                Type exType = exception.GetType();
                MessageFault mf = exception.CreateMessageFault();
                if (mf.HasDetail)
                {
                    var detailedMessage = mf.GetDetail<System.Xml.Linq.XElement>();
                    String message = detailedMessage.FirstNode.ToString();
                }
            }
            catch (System.Web.Services.Protocols.SoapException exception)
            {
                var y = exception;
            }
            catch (Exception ex)
            {
                var x = ex;
            }

            return results;
        }

        protected static void EstablishConnection()
        {
            _client.ChannelFactory.Endpoint.Behaviors.Remove<System.ServiceModel.Description.ClientCredentials>();
            _client.ChannelFactory.Endpoint.Behaviors.Add(new CustomCredentials());

            if (_client.ClientCredentials != null)
            {
                _client.ClientCredentials.UserName.UserName = _apiUsername;
                _client.ClientCredentials.UserName.Password = _apiPassword;
            }
        }

        #region Init Objects
        protected static DonorAddress CreateDonorAddress(Enum_AddressType type, Pocos.Donor d)
        {
            var a = new Pocos.DonorAddress
            {
                Address1 = "123 Main St.",
                AddressType = type,
                City = "Dallas",
                State = "TX",
                Zip = "75287",
                Country = "USA",
                FName = d.FName,
                LName = d.LName,
                Email = d.Email,
                Phone = d.Phone
            };
            return a;
        }

        protected static DonationAddress CreateAddress(Enum_AddressType type, Pocos.Donor d)
        {
            var add = new Pocos.DonationAddress
            {
                Address1 = "123 Plaza Way",
                AddressType = type,
                City = "Dallas",
                State = "TX",
                Zip = "75287",
                Country = "USA",
                FName = d.FName,
                LName = d.LName,
                Email = d.Email,
                Phone = d.Phone
            };

            return add;
        }

        public static void InitObjects(ref Donation donation, ref Donor donor)
        {
            donor = new Pocos.Donor
            {
                FName = "Sally",
                LName = "Sue",
                Phone = "123-123-1234",
                Email = "Sally@email.com",
            };
            donor.DonorAddresses.Add(CreateDonorAddress(Enum_AddressType.Billing, donor));
            donor.DonorAddresses.Add(CreateDonorAddress(Enum_AddressType.Shipping, donor));

            donation = new Pocos.Donation
            {
                Amount = 100m,
                AuthorizationNumber = "ABC123_Auth#",
                CardBrand = Enum_CardBrand.MasterCard,
                Comment = "Test Comment",
            };
            donation.DonationAddresses.Add(CreateAddress(Enum_AddressType.Billing, donor));
            donation.DonationAddresses.Add(CreateAddress(Enum_AddressType.Shipping, donor));
            donation.Donor = donor;
            donation.Email = donor.Email;
            donation.GatewayName = "PTC";
            donation.GatewayTransDetails = "GatewayTransDetails_here";
            donation.Last4ofCC = "1234";
            donation.MotivationCode = "custom-motivation-code";
            donation.Name = donor.FullName;
            donation.Notes = "Test from RaiseDonors";
            donation.PaymentMethod = Enum_PaymentMethod.CC;
            donation.Phone = donor.Phone;
            donation.SourceCode = "custom-source-code";
            donation.Status = Enum_ChargeStatus.Approved;
            donation.TestMode = false;
            donation.TransactionId = "123-xyz-456-abc";
        }
        #endregion
    }
}
