using System;
using System.Text;
using System.ServiceModel.Description;
using System.ServiceModel;
using System.ServiceModel.Security;
using System.Security.Cryptography;

namespace OLOD_DEMO
{
    public class CustomCredentials : ClientCredentials
    {
        public CustomCredentials()
        { }

        protected CustomCredentials(CustomCredentials cc)
            : base(cc)
        {

        }

        public override System.IdentityModel.Selectors.SecurityTokenManager CreateSecurityTokenManager()
        {
            return new CustomSecurityTokenManager(this);
        }

        protected override ClientCredentials CloneCore()
        {
            return new CustomCredentials(this);
        }
    }

    internal class CustomSecurityTokenManager : ClientCredentialsSecurityTokenManager
    {
        public CustomSecurityTokenManager(CustomCredentials cred)
            : base(cred)
        {

        }

        public override System.IdentityModel.Selectors.SecurityTokenSerializer CreateSecurityTokenSerializer(System.IdentityModel.Selectors.SecurityTokenVersion version)
        {
            return new CustomTokenSerializer(System.ServiceModel.Security.SecurityVersion.WSSecurity11);
        }
    }

    internal class CustomTokenSerializer : WSSecurityTokenSerializer
    {
        public CustomTokenSerializer(System.ServiceModel.Security.SecurityVersion sv)
            : base(sv)
        {

        }

        protected override void WriteTokenCore(System.Xml.XmlWriter writer, System.IdentityModel.Tokens.SecurityToken token)
        {
            Random r = new Random();
            string tokennamespace = "o";
            DateTime created = DateTime.Now;
            string createdStr = created.ToString("yyyy-MM-ddThh:mm:ss.fffZ");
            string nonce = Convert.ToBase64String(Encoding.ASCII.GetBytes(SHA1Encrypt(created + r.Next().ToString())));
            System.IdentityModel.Tokens.UserNameSecurityToken unToken = (System.IdentityModel.Tokens.UserNameSecurityToken)token;
            writer.WriteRaw(String.Format(
                "<{0}:UsernameToken u:Id=\"" + token.Id + "\" xmlns:u=\"http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd\">" +
                "<{0}:Username>" + unToken.UserName + "</{0}:Username>" +
                "<{0}:Password Type=\"http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-username-token-profile-1.0#PasswordText\">" +
                unToken.Password + "</{0}:Password>" +
                "<{0}:Nonce EncodingType=\"http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-soap-message-security-1.0#Base64Binary\">" +
                nonce + "</{0}:Nonce>" +
                "<u:Created>" + createdStr + "</u:Created></{0}:UsernameToken>", tokennamespace));
        }

        protected String ByteArrayToString(byte[] inputArray)
        {
            StringBuilder output = new StringBuilder("");
            for (int i = 0; i < inputArray.Length; i++)
            {
                output.Append(inputArray[i].ToString("X2"));
            }
            return output.ToString();
        }
        protected String SHA1Encrypt(String phrase)
        {
            UTF8Encoding encoder = new UTF8Encoding();
            SHA1CryptoServiceProvider sha1Hasher = new SHA1CryptoServiceProvider();
            byte[] hashedDataBytes = sha1Hasher.ComputeHash(encoder.GetBytes(phrase));
            return ByteArrayToString(hashedDataBytes);
        }
    }
}
