using System;
using System.Net.Http;
using System.Text;
using System.Web.Configuration;
using Newtonsoft.Json;
using OktaAPIShared.Models;

namespace OktaAPI.Helpers
{
    public class APIHelper
    {
        private static string _apiUrlBase;
        private static string _oktaToken;
        private static string _oktaOAuthHeaderAuth;
        private static string _oktaOAuthIssuerId;
        private static string _oktaOAuthClientId;
        private static string _oktaOAuthRedirectUri;
        
        private static HttpClient _client = new HttpClient();

        static APIHelper()
        {
            _apiUrlBase = WebConfigurationManager.AppSettings["okta:BaseUrl"];
            _oktaToken = WebConfigurationManager.AppSettings["okta:APIToken"];

            _oktaOAuthIssuerId = WebConfigurationManager.AppSettings["okta:OAuthIssuerId"];
            _oktaOAuthClientId = WebConfigurationManager.AppSettings["okta:OAuthClientId"];
            _oktaOAuthRedirectUri = WebConfigurationManager.AppSettings["okta:OAuthRedirectUri"];
            
            var oktaOAuthSecret = WebConfigurationManager.AppSettings["okta:OauthClientSecret"];
            _oktaOAuthHeaderAuth = Base64Encode($"{_oktaOAuthClientId}:{oktaOAuthSecret}");
        }

        public static string GetAllCustomers()
        {
            var sJsonResponse = JsonHelper.Get($"https://{_apiUrlBase}/api/v1/users?limit=100", _oktaToken);
            return sJsonResponse;
        }

        public static Customer GetCustomerById(string Id)
        {
            var sJsonResponse = JsonHelper.Get($"https://{_apiUrlBase}/api/v1/users/{Id}", _oktaToken);
            return JsonConvert.DeserializeObject<Customer>(sJsonResponse);
        }

        public static string GetUserIdByName(LoginViewModel model)
        {
            var sJsonResponse = JsonHelper.Get($"https://{_apiUrlBase}/api/v1/users?q={Uri.EscapeDataString(model.UserName)}", _oktaToken);
            dynamic dUsers = JsonConvert.DeserializeObject(sJsonResponse);
            if (dUsers.Count == 0) {
                sJsonResponse = AddNewUser(model);
                var oCustomer = JsonConvert.DeserializeObject<Customer>(sJsonResponse);
                return oCustomer.Id;
            }
            else if (dUsers.Count == 1)
            {
                var oUser = System.Linq.Enumerable.First(dUsers);
                return oUser.id;
            }
            return null;//more than 1 match, could add filter to improve this logic
        }

        public static dynamic AddNewUser(LoginViewModel model)
        {
            var oProfile = new Profile
            {
                Email = model.UserName,
                Login = model.UserName,
                FirstName = "No",
                LastName = "Name"
                //First & Last name is currently required, either pass them in or set fake name
                //Okta is removing this requirement soon and it won't required
            };
            var oAddCustomer = new CustomerAdd();
            oAddCustomer.Profile = oProfile;

            //Enhancement:
            //could look up a group and add group to the CustomerAdd object to organize users for this app

            //Please Note:
            //User will be in 'Pending user action', which is good for security
            //can still add MFA to 'Pending' users and they can't log into Okta
            //If you want them to be Activated then set the Password

            return JsonHelper.Post($"https://{_apiUrlBase}/api/v1/users?activate=true", JsonHelper.JsonContent(oAddCustomer), _oktaToken);
        }

        public static dynamic ListUserFactors(string userid)
        {
            var sJsonResponse = JsonHelper.Get($"https://{_apiUrlBase}/api/v1/users/{userid}/factors", _oktaToken);
            return JsonConvert.DeserializeObject(sJsonResponse);
        }

        public static dynamic SendUserSMS(SMSViewModel model, string factorId)
        {
            var sJsonResponse = JsonHelper.Post($"https://{_apiUrlBase}/api/v1/users/{model.UserId}/factors/{factorId}/verify", "{}", _oktaToken);
            
            return JsonConvert.DeserializeObject(sJsonResponse);
        }

        public static dynamic EnrollUserSMS(SMSViewModel model)
        {
            var oEnrollSMS = new EnrollSMS();
            oEnrollSMS.Profile.PhoneNumber = String.Format("+{0}-{1}", model.CountryCode, model.PhoneNumber);//Format - "+1-2223334444"

            var sJsonResponse = JsonHelper.Post($"https://{_apiUrlBase}/api/v1/users/{model.UserId}/factors", JsonHelper.JsonContent(oEnrollSMS), _oktaToken);
            
            return JsonConvert.DeserializeObject(sJsonResponse);
        }

        public static dynamic VerifyUserSMS(SMSViewModel model)
        {
            var oCode = new SendSMSPassCode();
            oCode.PassCode = model.PassCode;

            var sJsonResponse = JsonHelper.Post(model.ApiUrl, JsonHelper.JsonContent(oCode), _oktaToken);
            return JsonConvert.DeserializeObject(sJsonResponse);
        }

        public static dynamic ActivateUserSMS(SMSViewModel model)
        {
            var oActivateVerifySMS = new ActivateVerifySMS();


            //   api/v1/users/${userId}/factors/${factorId}/lifecycle/activate
            //   api/v1/users/${userId}/factors/${factorId}/verify

            var sJsonResponse = JsonHelper.Post($"https://{_apiUrlBase}/api/v1/users/{model.UserId}/factors", JsonHelper.JsonContent(oActivateVerifySMS), _oktaToken);

            return JsonConvert.DeserializeObject(sJsonResponse);
        }

        public static string GetAuthorizationURL(string oktaSessionToken)
        {
            return $"https://{_apiUrlBase}/oauth2/{_oktaOAuthIssuerId}/v1/authorize?response_type=code&client_id={_oktaOAuthClientId}&redirect_uri={_oktaOAuthRedirectUri}&scope=openid&state=af0ifjsldkj&nonce=n-0S6_WzA2Mj&sessionToken={oktaSessionToken}";
        }

        private static string Base64Encode(string plainText)
        {
            var bytes = Encoding.UTF8.GetBytes(plainText);
            return Convert.ToBase64String(bytes);
        }
    }
}