using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Configuration;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Threading.Tasks;
using TRMDesktopUI.Library.Api;

namespace Portal.Authentication
{
    public class AuthStateProvider : AuthenticationStateProvider
    {
        private readonly HttpClient _httpClient;
        private readonly ILocalStorageService _localStorage;
        private readonly IConfiguration _config;
        private readonly IAPIHelper _apiHelper;

        // _anonymous (not loggedIn) use this in place don't know what is the user is
        private readonly AuthenticationState _anonymous;

        public AuthStateProvider(HttpClient httpClient,
                                 ILocalStorageService localStorage,
                                 IConfiguration config,
                                 IAPIHelper apiHelper)
        {
            _httpClient = httpClient;
            _localStorage = localStorage;
            _config = config;
            _apiHelper = apiHelper;
            _anonymous = new AuthenticationState(
                new ClaimsPrincipal(
                    new ClaimsIdentity()));
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            string authTokenStorageKey = _config["authTokenStorageKey"];

            var token = await _localStorage.GetItemAsync<string>(authTokenStorageKey);

            if (string.IsNullOrWhiteSpace(token))
            {
                //looking for authToken key in _localStorage if IsNullOrWhiteSpace return _anonymous
                return _anonymous;
            }
            //when start the web page if your still login 
            //notify the system
            bool isAuthenticated = await NotifyUserAuthentication(token);

            if (isAuthenticated == false)
            {
                return _anonymous;
            }

            //add authorization for httpclient headers (for web portal)
            //specify who is talking using https protocol
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", token);

            return new AuthenticationState(
                new ClaimsPrincipal(
                    new ClaimsIdentity(JwtParser.ParseClaimsFromJwT(token)
                        , "jwtAuthType")));
        }

        public async Task<bool> NotifyUserAuthentication(string token)
        {
            Task<AuthenticationState> authState;
            bool isAuthenticatedOutput;
            try
            {
                //API is a stateless
                //add authorization for api (each endpoint)
                await _apiHelper.GetLoggedInUserInfo(token);
                //create new user with token authorization
                var authenticateUser = new ClaimsPrincipal(
                        new ClaimsIdentity(JwtParser.ParseClaimsFromJwT(token)
                            , "jwtAuthType"));
                //new authentication state with new user
                authState = Task.FromResult(new AuthenticationState(authenticateUser));
                //event change the status of user when user log in
                NotifyAuthenticationStateChanged(authState);
                isAuthenticatedOutput = true;
            }
            catch (Exception ex)
            {
                //expire token
                Console.WriteLine(ex);
                await NotifyUserLogOut();
                isAuthenticatedOutput = false;
            }
            return isAuthenticatedOutput;
        }

        public async Task NotifyUserLogOut()
        {
            string authTokenStorageKey = _config["authTokenStorageKey"];
            //remove bad token
            await _localStorage.RemoveItemAsync(authTokenStorageKey);
            // //new state you are log off
            var authState = Task.FromResult(_anonymous);
            //clear the authorization for api
            _apiHelper.LogOffUser();
            //clear the authorization for web portal
            _httpClient.DefaultRequestHeaders.Authorization = null;
            //event change the status of user when user log out
            NotifyAuthenticationStateChanged(authState);
        }
    }
}
