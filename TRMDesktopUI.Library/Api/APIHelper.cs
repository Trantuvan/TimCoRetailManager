using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using TRMDesktopUI.Library.Models;
using TRMDesktopUI.Models;

namespace TRMDesktopUI.Library.Api
{
    public class APIHelper : IAPIHelper
    {
        private HttpClient _apiClient;
        private readonly ILoggedInUserModel _loggedInUser;

        public APIHelper(ILoggedInUserModel loggedInUser)
        {
            InitalizeClient();
            _loggedInUser = loggedInUser;
        }

        // call this prop to access instance of _apiClient access to api
        public HttpClient ApiClient
        {
            get { return _apiClient; }
        }

        private void InitalizeClient()
        {
            string api = ConfigurationManager.AppSettings["api"];

            _apiClient = new HttpClient();
            _apiClient.BaseAddress = new Uri(api);
            _apiClient.DefaultRequestHeaders.Accept.Clear();
            //Tao header moi khi request thanh cong va tra lai json type
            _apiClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public async Task<AuthenticatedUser> Authenticate(string userName, string password)
        {
            var data = new FormUrlEncodedContent(new[]
            {
               new KeyValuePair<string, string>("grant_type","password"),
               new KeyValuePair<string, string>("username",userName),
               new KeyValuePair<string, string>("password",password)
            });

            using (HttpResponseMessage response = await _apiClient.PostAsync("/Token", data))
            {
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadAsAsync<AuthenticatedUser>();
                    return result;
                }
                else
                {
                    throw new Exception(response?.ReasonPhrase);
                }
            }
        }

        public async Task GetLoggedInUserInfo(string token)
        {
            _apiClient.DefaultRequestHeaders.Clear();
            _apiClient.DefaultRequestHeaders.Accept.Clear();
            _apiClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            // add to header token
            _apiClient.DefaultRequestHeaders.Add("Authorization", $"bearer {token}");

            using (HttpResponseMessage response = await _apiClient.GetAsync("api/User"))
            {
                if (response.IsSuccessStatusCode)
                {
                    // LoggedInUserModel come from the web api don't have Token prop
                    var result = await response.Content.ReadAsAsync<LoggedInUserModel>();

                    // mapping 2 instance of LoggedInUserModel singleton vs result (have Token prop)
                    // singleton update here it update every where
                    _loggedInUser.CreateDate = result.CreateDate;
                    _loggedInUser.LastName = result.LastName;
                    _loggedInUser.FirstName = result.FirstName;
                    _loggedInUser.EmailAddress = result.EmailAddress;
                    _loggedInUser.Id = result.Id;
                    // token came from header
                    _loggedInUser.Token = token;
                }
                else
                {
                    throw new Exception(response?.ReasonPhrase);
                }
            }
        }

        public void LogOffUser()
        {
            _apiClient.DefaultRequestHeaders.Clear();
        }
    }
}
