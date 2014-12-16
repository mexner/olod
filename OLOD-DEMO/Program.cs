using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using OLOD_DEMO.OLODWebService;

namespace OLOD_DEMO
{
    class Program
    {
        protected static OrangeLeapClient _client;
        private static string _apiUsername = ConfigurationSettings.AppSettings["api_username"];
        private static string _apiPassword = ConfigurationSettings.AppSettings["api_password"];
        
        public static void Main(string[] args)
        {
            _client = new OrangeLeapClient();
            EstablishConnection();
            FindConstituent();
        }

        protected static List<constituent> FindConstituent()
        {
            var results = new List<constituent>();

            var request = new FindConstituentsRequest();
            request.firstName = "chris";
            request.lastName = "mechsner";

            //VALIDATION ERROR
            //request.primaryEmail = new email() { emailAddress = "chris@ascendio.com", primary = true};
            //request.primaryPhone = new phone() { number = "123-123-1234", primary = true};

            //VALIDATION ERROR
            //request.primaryAddress = new address
            //                                 {
            //                                     postalCode = 75019,
            //                                     addressLine1 = "123 Test Main St.",
            //                                     city = "Dallas",
            //                                 };
            
            try
            {
                var response = _client.FindConstituents(request);

                //only grab active accounts
                response = response.Where(x => !x.deleted && !x.inactive).ToArray();

                if (response.Any())
                {
                    results.AddRange(response);
                }
            }
            catch (System.ServiceModel.FaultException exc)
            {
                var txt = exc;
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
    }
}
