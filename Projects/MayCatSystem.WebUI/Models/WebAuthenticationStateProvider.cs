using Microsoft.AspNetCore.Components.Authorization;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Security.Claims;
using Microsoft.AspNetCore.Components;
using Blazored.SessionStorage;

namespace MayCatSystem.WebUI.Models
{
    public class WebAuthenticationStateProvider : AuthenticationStateProvider
    {
        public ISessionStorageService _sessionStorage { get; set; }

        public WebAuthenticationStateProvider(ISessionStorageService sessionStorage)
        {
            _sessionStorage = sessionStorage;   
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var savedToken = await _sessionStorage.GetItemAsync<string>("username");

            ClaimsIdentity ClaimsIdentity;
            if (string.IsNullOrWhiteSpace(savedToken))
            {
                ClaimsIdentity = new ClaimsIdentity();
            }
            else
            {
                var claims = new List<Claim> {
                    new Claim(ClaimTypes.Name, savedToken)
                };
                ClaimsIdentity = new ClaimsIdentity(claims, "username");
            }
            return new AuthenticationState(new ClaimsPrincipal(ClaimsIdentity));
        }
    }
}
