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
            
            //var constt = CreateConstituentWithAllProperties(donor);
            //Console.WriteLine("created const! id: " + constt.id.ToString());
            //Console.ReadLine();

            var constt = Find(4618);
            AddGiftToConstituent(donation, ref constt);

            //UpdateConstituent(donor, ref constt, "Just adding a test note, b/c i'm testing!");

            //var donors = Find(donor, true, true, false, false, false);
            //constituent constt;

            //if (donors.Any())
            //{
            //    if (donors.Count == 1)
            //    {
            //        constt = donors.First();
            //        UpdateConstituent(donor, ref constt);
            //    }
            //    else
            //    {
            //        switch (_howToHandleOverMatch)
            //        {
            //            case Enum_HowToHandleOverMatch.Use_Oldest_Record:
            //                constt = donors.OrderBy(x => x.updateDate).First();
            //                UpdateConstituent(donor, ref constt);
            //                break;
            //            case Enum_HowToHandleOverMatch.Use_Newest_Record:
            //                constt = donors.OrderByDescending(x => x.updateDate).First();
            //                UpdateConstituent(donor, ref constt);
            //                break;
            //            case Enum_HowToHandleOverMatch.Create_New_Record:
            //                constt = CreateConstituentWithAllProperties(donor);
            //                break;
            //            case Enum_HowToHandleOverMatch.Create_New_Record_And_Be_Notified:
            //                constt = CreateConstituentWithAllProperties(donor);
            //                break;
            //            default:
            //                throw new ArgumentOutOfRangeException();
            //        }
            //    }
            //}
            //else
            //{
            //    //create new donor
            //    constt = CreateConstituentWithAllProperties(donor);
            //}
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
                //FName, Lname, Phone
                donors = Find(donation.Donor, true, true, false, true, false);
                if (donors.Any())
                {
                    Console.WriteLine("Located donor(s) by matching FName, LName, & Phone. ");
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
            if (!donors.Any())
            {
                //FName, Lname
                donors = Find(donation.Donor, true, true, false, false, false);
                if (donors.Any())
                {
                    Console.WriteLine("Located donor(s) by matching FName, LName. ");
                }
            }

            if (donors.Any())
            {

                if (donors.Count == 1)
                {
                    constt = donors.First();
                    //UpdateConstituent(d.Donor, constt);
                    Console.WriteLine("Found 1 donor, perfect match! ");
                    UpdateConstituent(donation.Donor, ref constt);
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

        private static abstractCustomizableEntityEntry AddCommunicationPreferences(Donor d)
        {
            //add opt in preferences to donor
            var fm = new abstractCustomizableEntityEntry()
            {
                key = "communicationPreferences",
                value = new customField()
                {
                    name = "communicationPreferences",
                    value = d.NesletterOptIn ? "Opt In" : "Unknown",
                    entityType = "constituent",
                    sequenceNumber = 0,
                    dataType = 0,
                }
            };

            return fm;
        }

        private static abstractCustomizableEntityEntry AddSourceCodeToConstintuent(Donor d)
        {
            //add source code to donor
            var fm = new abstractCustomizableEntityEntry()
            {
                key = "source",
                value = new customField()
                {
                    name = "source",
                    value = SourceCodePickList().First().itemName,
                    entityType = "constituent",
                    sequenceNumber = 0,
                    dataType = 0,
                }
            };

            return fm;
        }

        private static constituent CreateConstituentWithAllProperties(Donor donor)
        {
            var constt = CreateConstituent(donor);

            if (constt == null) throw new Exception("Unable to create new constituent.");

            AddPhoneToConstituent(donor, ref constt, true, true);
            AddEmailToConstituent(donor, ref constt, true, true);

            var billing = donor.DonorAddresses.FirstOrDefault(x => x.AddressType == Enum_AddressType.Billing);
            if (billing != null) AddAddressToConstituent(billing, ref constt, true, true);

            var shipping = donor.DonorAddresses.FirstOrDefault(x => x.AddressType == Enum_AddressType.Shipping);
            if (shipping != null) AddAddressToConstituent(shipping, ref constt, true, true);

            return constt;
        }

        private static constituent CreateConstituent(Donor d)
        {
            var constt = new constituent();
                
            try
            {
                var fieldMap = new abstractCustomizableEntityEntry[2];

                fieldMap[0] = AddSourceCodeToConstintuent(d);
                fieldMap[1] = AddCommunicationPreferences(d);

                constt.firstName = d.FName;
                constt.lastName = d.LName;
                constt.constituentType = "individual";
                constt.customFieldMap = fieldMap;
                constt.inactive = false;
                constt.createDate = DateTime.Now;
                constt.updateDate = DateTime.Now;
                
                var request = new SaveOrUpdateConstituentRequest { constituent = constt };
                var response = _client.SaveOrUpdateConstituent(request);
               
                return response.constituent;
            }
            catch (FaultException exception)
            {
                Type exType = exception.GetType();
                MessageFault mf = exception.CreateMessageFault();
                Console.WriteLine("Error when creating constituent // " + exception.Message);
                if (mf.HasDetail)
                {
                    var detailedMessage = mf.GetDetail<System.Xml.Linq.XElement>();
                    String message = detailedMessage.FirstNode.ToString();
                    Console.WriteLine("More details // " + message);
                }
            }
            catch (Exception exc)
            {
                var txt = exc;
            }
            return null;
        }

        /// <summary>
        /// retrieves pick list of motivation codes
        /// </summary>
        /// <returns></returns>
        public static List<picklistItem> TouchPointCodePickList()
        {
            return GetPickLists("entryType").ToList();
        }

        /// <summary>
        /// retrieves pick list of motivation codes
        /// </summary>
        /// <returns></returns>
        public static List<picklistItem> MotivationCodePickList()
        {
            return GetPickLists("motivationCode").ToList();
        }

        /// <summary>
        /// retrieves pick list of motivation codes
        /// </summary>
        /// <returns></returns>
        public static List<picklistItem> FundNamePickList()
        {
            return GetPickLists("projectCode").ToList();
        }

        /// <summary>
        /// retrieves pick list of source codes.
        /// </summary>
        /// <returns></returns>
        public static List<picklistItem> SourceCodePickList()
        {
            return GetPickLists("customFieldMap[source]").ToList();
        }

        /// <summary>
        /// retrieves items from pick list.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        private static IEnumerable<picklistItem> GetPickLists(string name)
        {
            var req = new GetPickListByNameRequest() { includeInactive = false, name = name };

            var resp = _client.GetPickListByName(req);
            var pics = resp.picklist;
            var sources = pics.picklistItems;

            return sources;
        }

        private static void AddCommunicationHistoryToDonor(string note, ref constituent c)
        {
            try
            {
                var req = new SaveOrUpdateCommunicationHistoryRequest();
                req.constituentId = c.id;
                
                var history = new communicationHistory();
                history.comments = note;
                history.communicationHistoryType = "MANUAL";
                history.constituentId = c.id;
                history.createDate = DateTime.Now;
                history.entryType = "Note"; //pick list during setup, let admin decide.
                history.updateDate = DateTime.Now;
                history.customFieldMap = new abstractCustomizableEntityEntry[0];

                req.communicationHistory = history;

                var resp = _client.SaveOrUpdateCommunicationHistory(req);
                var his = resp.communicationHistory;
            }
            catch (FaultException exception)
            {
                Type exType = exception.GetType();
                MessageFault mf = exception.CreateMessageFault();
                Console.WriteLine("Error when add phone to constituent // " + exception.Message);
                if (mf.HasDetail)
                {
                    var detailedMessage = mf.GetDetail<System.Xml.Linq.XElement>();
                    String message = detailedMessage.FirstNode.ToString();
                    Console.WriteLine("More details // " + message);
                }
            }
            catch (Exception exc)
            {
                var txt = exc;
            }
        }

        private static void UpdateConstituent(Donor d, ref constituent c, string noteHistory = "")
        {
            //update fields
            c.lastName = d.LName;
            c.firstName = d.FName;
            c.updateDate = DateTime.Now;
            
            var request = new SaveOrUpdateConstituentRequest { constituent = c };
            var response = _client.SaveOrUpdateConstituent(request);
            var nc = response.constituent;

            AddPhoneToConstituent(d, ref c, true, true);
            AddEmailToConstituent(d, ref c, true, true);

            var billing = d.DonorAddresses.FirstOrDefault(x => x.AddressType == Enum_AddressType.Billing);
            if (billing != null) AddAddressToConstituent(billing, ref c, true, true);

            var shipping = d.DonorAddresses.FirstOrDefault(x => x.AddressType == Enum_AddressType.Shipping);
            if (shipping != null) AddAddressToConstituent(shipping, ref c, true, true);

            if (!string.IsNullOrEmpty(noteHistory))
            {
                AddCommunicationHistoryToDonor(noteHistory, ref c);
            }

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
                    createDate = DateTime.Now,
                    updateDate = DateTime.Now,
                    customFieldMap = new abstractCustomizableEntityEntry[0],
                };

                //check if is null or already has phone.
                if (constt.phones == null)
                {
                    constt.phones = new phone[1];
                    constt.phones[0] = phone;
                }
                else
                {
                    var newmax = constt.phones.Count() + 1;
                    var newArray = new phone[newmax];
                    constt.phones.CopyTo(newArray, 0);
                    newArray[newmax - 1] = phone;
                    constt.phones = newArray;
                }

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
                Console.WriteLine("Error when add phone to constituent // " + exception.Message);
                if (mf.HasDetail)
                {
                    var detailedMessage = mf.GetDetail<System.Xml.Linq.XElement>();
                    String message = detailedMessage.FirstNode.ToString();
                    Console.WriteLine("More details // " + message);
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
                    createDate = DateTime.Now,
                    updateDate = DateTime.Now,
                    customFieldMap = new abstractCustomizableEntityEntry[0],
                };

                if (constt.emails == null)
                {
                    constt.emails = new email[1];
                    constt.emails[0] = email;
                }
                else
                {
                    var newmax = constt.emails.Count() + 1;
                    var newArray = new email[newmax];
                    constt.emails.CopyTo(newArray, 0);
                    newArray[newmax - 1] = email;
                    constt.emails = newArray;
                }

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
                Console.WriteLine("Error when adding email to constituent // " + exception.Message);
                if (mf.HasDetail)
                {
                    var detailedMessage = mf.GetDetail<System.Xml.Linq.XElement>();
                    String message = detailedMessage.FirstNode.ToString();
                    Console.WriteLine("More details // " + message);
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
                address.createDate = DateTime.Now;
                address.updateDate = DateTime.Now;
                address.customFieldMap = new abstractCustomizableEntityEntry[0];
                
                if (constt.addresses == null)
                {
                    constt.addresses = new address[1];
                    constt.addresses[0] = address;
                }
                else
                {
                    var newmax = constt.addresses.Count() + 1;
                    var newArray = new address[newmax];
                    constt.addresses.CopyTo(newArray, 0);
                    newArray[newmax - 1] = address;
                    constt.addresses = newArray;
                }

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
                Console.WriteLine("Error when adding address to constituent // " + exception.Message);
                if (mf.HasDetail)
                {
                    var detailedMessage = mf.GetDetail<System.Xml.Linq.XElement>();
                    String message = detailedMessage.FirstNode.ToString();
                    Console.WriteLine("More details // " + message);
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

                ps.paymentType = PaymentType.Cash;
                ps.constituentId = constt.id;
                g.paymentSource = ps;

                var billing = donation.Donor.DonorAddresses.FirstOrDefault(x => x.AddressType == Enum_AddressType.Billing);
                var nowDate = DateTime.Now;

                g.createDate = nowDate;
                g.updateDate = nowDate;
                g.currencyCode = "USD";


                g.address = TranslateAddress(billing, false);
                g.amount = donation.Amount;
                g.authCode = donation.AuthorizationNumber;
                g.comments = "Processed by RaiseDonors using card ending in x" + donation.Last4ofCC;
                if (!string.IsNullOrEmpty(donation.Comment)) g.comments += "Donor provided comment: " + donation.Comment;
                g.customFieldMap = fieldMap;
                g.constituentId = constt.id;
                g.deductible = false;
                g.deductibleAmount = 0.00m;
                g.donationDate = nowDate;
                g.email = constt.primaryEmail;
                g.giftStatus = "Paid";  //Paid, Pending, Not Paid 
                g.paymentMessage = "Processed thru RaiseDonors";
                g.paymentType = PaymentType.Cash;
                g.phone = constt.primaryPhone;
                g.transactionDate = nowDate;
                g.transactionDateSpecified = true;
                g.txRefNum = donation.TransactionId;

                
                var distLines = new List<distributionLine>();
                foreach (var d in donation.DonationFundAllocations)
                {
                    //read project codes, and use in setup on which one to use.
                    var fieldMaps = new List<abstractCustomizableEntityEntry>
                                        {
                                            createCustomFieldMap("anonymous", 0, constt.id, "distributionline", "anonymous", "false"),
                                            createCustomFieldMap("recognitionName", 0, constt.id, "distributionline", "recognitionName", donation.Donor.FullName),
                                            createCustomFieldMap("totalAdjustedAmount", 0, constt.id, "distributionline", "totalAdjustedAmount", d.Amount.ToString("0.00")),
                                            createCustomFieldMap("taxDeductible", 0, constt.id, "distributionline", "taxDeductible", "true"),
                                            createCustomFieldMap("source", 0, constt.id, "source", "source", donation.SourceCode),
                                        };

                    var dLine = new distributionLine
                    {
                        customFieldMap = fieldMaps.ToArray(),
                        amount = d.Amount,
                        other_motivationCode = donation.MotivationCode,
                        percentage = Math.Round((d.Amount / donation.Amount) * 100, 2, MidpointRounding.AwayFromZero),
                        motivationCode = donation.MotivationCode,  //what motivated to give - don't use this one
                        projectCode = d.Fund.Name //gift designation? 
                    };
                    distLines.Add(dLine);
                }

                //source-code, use customfield on the gift.
                //project, and source will throw error if not pre-existing.

                //throws error 
                //Value cannot be null.
                //Parameter name: source
                g.distributionLines = distLines.ToArray();

                var request = new SaveOrUpdateGiftRequest { gift = g, constituentId = constt.id };
                var response = _client.SaveOrUpdateGift(request);
                
                Console.WriteLine("Successfully sent gift to OLOD, gift ID: " + response.gift.id + ".");
            }
            catch (FaultException exception)
            {
                Type exType = exception.GetType();
                MessageFault mf = exception.CreateMessageFault();
                Console.WriteLine("Error when adding gift to constituent // " + exception.Message);
                if (mf.HasDetail)
                {
                    var detailedMessage = mf.GetDetail<System.Xml.Linq.XElement>();
                    String message = detailedMessage.FirstNode.ToString();
                    Console.WriteLine("More details // " + message);
                }
            }
            catch (Exception exc)
            {
                var text = exc;
            }
            
        }

        private static abstractCustomizableEntityEntry createCustomFieldMap(string key, long datatype, long entityId, string entityType, string name, string value)
        {
            //read project codes, and use in setup on which one to use.
            var fm = new abstractCustomizableEntityEntry { key = key };

            var cf = new customField
            {
                dataType = datatype,
                entityId = entityId,
                entityType = entityType,
                name = name,
                value = value
            };

            fm.value = cf;

            return fm;
        }
      
        private static constituent Find(long Id)
        {
            var req = new GetConstituentByIdRequest() {id = Id};
            var resp = _client.GetConstituentById(req);

            return resp.constituent;
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
                Console.WriteLine("Error when finding constituent // " + exception.Message);
                if (mf.HasDetail)
                {
                    var detailedMessage = mf.GetDetail<System.Xml.Linq.XElement>();
                    String message = detailedMessage.FirstNode.ToString();
                    Console.WriteLine("More details // " + message);
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

        protected static string LookupCountryCodeFromPickList(string ourCountryName)
        {
            var req = new GetPickListByNameRequest {includeInactive = false, name = "country"};

            var resp = _client.GetPickListByName(req);
            var picks = resp.picklist;
            var countries = picks.picklistItems;

            //our country look up
            var countryDto = CountryFinder.FindCountry(ourCountryName);

            foreach (var c in countries)
            {
                if (c.itemName.Trim().ToLower() == countryDto.Iso2.Trim().ToLower()) return c.itemName;
                if (c.itemName.Trim().ToLower() == countryDto.Iso3.Trim().ToLower()) return c.itemName;
                if (c.itemName.Trim().ToLower() == countryDto.Name.Trim().ToLower()) return c.itemName;

                if (c.defaultDisplayValue.Trim().ToLower() == countryDto.Iso2.Trim().ToLower()) return c.itemName;
                if (c.defaultDisplayValue.Trim().ToLower() == countryDto.Iso3.Trim().ToLower()) return c.itemName;
                if (c.defaultDisplayValue.Trim().ToLower() == countryDto.Name.Trim().ToLower()) return c.itemName;

                if (c.longDescription.Trim().ToLower() == countryDto.Iso2.Trim().ToLower()) return c.itemName;
                if (c.longDescription.Trim().ToLower() == countryDto.Iso3.Trim().ToLower()) return c.itemName;
                if (c.longDescription.Trim().ToLower() == countryDto.Name.Trim().ToLower()) return c.itemName;
            }

            return "US";
        }

        #region Init Objects
        private static readonly System.Random getrandom = new System.Random();
        public static int NewRandom(int numDigits)
        {
            switch (numDigits)
            {
                case 1:
                    return getrandom.Next(0, 9);
                case 2:
                    return getrandom.Next(10, 99);
                case 3:
                    return getrandom.Next(100, 999);
                case 4:
                    return getrandom.Next(1000, 9999);
                case 5:
                    return getrandom.Next(10000, 99999);
                default:
                    return getrandom.Next(100000, 999999);
            }
        }

        public int NewRandomBetween(int lower, int higher)
        {
            return getrandom.Next(lower, higher);
        }

        public static void InitObjects(ref Donation donation, ref Donor donor)
        {
            donor = new Pocos.Donor
                        {
                            FName = "Sally" + NewRandom(5),
                            LName = "Sues" + NewRandom(5),
                            Phone = "(972) 220-1234",
                            Email = "Sally" + NewRandom(5) + "@email.com",
                            NesletterOptIn = true,
                        };
            donor.DonorAddresses.Add(CreateDonorAddress(Enum_AddressType.Billing, donor));
            donor.DonorAddresses.Add(CreateDonorAddress(Enum_AddressType.Shipping, donor));
            CreateDonation(ref donation, ref donor);
        }

        public static void CreateDonation(ref Donation donation, ref Donor donor)
        {
            donation = new Pocos.Donation
            {
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

            for (int i = 0; i < 5; i++)
            {
                var amt = (decimal)NewRandom(2);
                var alloc = new DonationFundAllocation()
                                {
                                    Amount = amt,
                                    Donation = donation,
                                    DonationId = donation.Id,
                                    Fund = new Fund()
                                               {
                                                   Active = true,
                                                   Code = "",
                                                   Description = "",
                                                   Id = i,
                                                   IsDeleted = false,
                                                   Name = FundNamePickList()[i].itemName,
                                               },
                                    FundId = i,
                                };
                donation.DonationFundAllocations.Add(alloc);
            }

            donation.Amount = donation.DonationFundAllocations.Sum(x => x.Amount);
        }

        protected static DonorAddress CreateDonorAddress(Enum_AddressType type, Pocos.Donor d)
        {
            var a = new Pocos.DonorAddress
            {
                Address1 = "a123 Main St.",
                AddressType = type,
                City = "Dallas",
                State = "TX",
                Zip = "75287",
                Country = LookupCountryCodeFromPickList("us"),
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
                Address1 = "a123 Plaza Way",
                AddressType = type,
                City = "Dallas",
                State = "TX",
                Zip = "75287",
                Country = LookupCountryCodeFromPickList("us"),
                FName = d.FName,
                LName = d.LName,
                Email = d.Email,
                Phone = d.Phone
            };

            return add;
        }

       
        #endregion
    }
}
