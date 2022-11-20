using Blazored.SessionStorage;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

namespace MayCatSystem.WebUI.Services
{
    public class AccountService
    {
        public ISessionStorageService _sessionStorage { get; set; }
        public AuthenticationStateProvider _authenticationStateProvider { get; set; }

        public AccountService(ISessionStorageService sessionStorage, AuthenticationStateProvider authenticationStateProvider)
        {
            _sessionStorage = sessionStorage;
            _authenticationStateProvider = authenticationStateProvider;
        }

        public async Task<bool> Login(string username, string password)
        {
            if (username == "admin" && password == "admin")
            {
                await _sessionStorage.SetItemAsync("username", username);
                return true;
            }
            return false;
        }
    }
}
