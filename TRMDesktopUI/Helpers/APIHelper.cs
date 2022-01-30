using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using TRMDesktopUI.Models;

namespace TRMDesktopUI.Helpers
{
    public class APIHelper : IAPIHelper
    {
        private HttpClient _apiClient { get; set; }

        public APIHelper()
        {
            InitalizeClient();
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
                    throw new Exception(response.ReasonPhrase);
                }
            }
        }
    }
}
