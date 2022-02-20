using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Portal.Authentication
{
    public class AuthStateProvider : AuthenticationStateProvider
    {
        private readonly HttpClient _httpClient;
        private readonly ILocalStorageService _localStorage;
        // _anonymous (not loggedIn) use this in place don't know what is the user is
        private readonly AuthenticationState _anonymous;

        public AuthStateProvider(HttpClient httpClient, ILocalStorageService localStorage)
        {
            _httpClient = httpClient;
            _localStorage = localStorage;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var token = await _localStorage.GetItemAsync<string>("authToken");

            if (string.IsNullOrWhiteSpace(token))
            {
                //looking for authToken key in _localStorage if IsNullOrWhiteSpace return _anonymous
                return _anonymous;
            }

            //neu co token thi add bearer token to header by httpClient
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", token);

            return new AuthenticationState(
                new ClaimsPrincipal(
                    new ClaimsIdentity(JwtParser.ParseClaimsFromJwT(token)
                        , "jwtAuthType")));
        }

        public void NotifyUserAuthentication(string token)
        {
            var authenticateUser = new ClaimsPrincipal(
                    new ClaimsIdentity(JwtParser.ParseClaimsFromJwT(token)
                        , "jwtAuthType"));

            var authState = Task.FromResult(new AuthenticationState(authenticateUser));

            //event change the status of user when user log in
            NotifyAuthenticationStateChanged(authState);
        }

        public void NotifyUserLogOut()
        {
            var authState = Task.FromResult(_anonymous);
            //event change the status of user when user log out
            NotifyAuthenticationStateChanged(authState);
        }
    }
}
