using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using Portal.Models;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;

namespace Portal.Authentication
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly HttpClient _client;
        private readonly AuthenticationStateProvider _authStateProvider;
        private readonly ILocalStorageService _localStorage;

        public AuthenticationService(HttpClient client,
                                     AuthenticationStateProvider authStateProvider,
                                     ILocalStorageService localStorage)
        {
            _client = client;
            _authStateProvider = authStateProvider;
            _localStorage = localStorage;
        }

        public async Task<AuthenticatedUserModel> Login(AuthenticationUserModel userForAuthentication)
        {
            // data is the payload in jwt token
            var data = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("grant_type","password"),
                //userForAuthentication request model
                new KeyValuePair<string, string>("username",userForAuthentication.Email),
                new KeyValuePair<string, string>("password", userForAuthentication.Password)
            });

            var authResult = await _client.PostAsync("https://localhost:5001/token", data);
            var authContent = await authResult.Content.ReadAsStringAsync();

            if (authResult.IsSuccessStatusCode == false)
            {
                return null;
            }

            //AuthenticatedUserModel respond model
            var result = JsonSerializer.Deserialize<AuthenticatedUserModel>(
                authContent,
                //trong truong hop khong control api co the dung option nay tweak resut
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            //store result.Access_Token in LocalStorage with key authToken (cached info for offline use)
            await _localStorage.SetItemAsync("authToken", result.Access_Token);

            //Notify system user state has changes call NotifyUserAuthentication and passing Access_Token
            ((AuthStateProvider)_authStateProvider).NotifyUserAuthentication(result.Access_Token);

            //whenever user HttpClient thi add header Authorization bearer access_token
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", result.Access_Token);

            return result;
        }

        public async Task LogOut()
        {
            await _localStorage.RemoveItemAsync("authToken");
            //casting AuthStateProvider child class of AuthenticationStateProvider
            //has extra class NotifyUserLogOut && NotifyUserAuthentication
            ((AuthStateProvider)_authStateProvider).NotifyUserLogOut();

            _client.DefaultRequestHeaders.Authorization = null;
        }
    }
}
