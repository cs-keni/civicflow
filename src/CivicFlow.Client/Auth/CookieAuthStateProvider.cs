using System.Security.Claims;
using CivicFlow.Client.Models;
using CivicFlow.Client.Services;
using Microsoft.AspNetCore.Components.Authorization;

namespace CivicFlow.Client.Auth;

public class CookieAuthStateProvider(CivicFlowApiClient api) : AuthenticationStateProvider
{
    private static readonly AuthenticationState Anonymous = new(new ClaimsPrincipal(new ClaimsIdentity()));
    private AuthenticationState _cached = Anonymous;

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        try
        {
            var user = await api.GetCurrentUserAsync();
            _cached = user is not null ? BuildState(user) : Anonymous;
        }
        catch
        {
            _cached = Anonymous;
        }
        return _cached;
    }

    public void NotifyLogin(UserDto user)
    {
        _cached = BuildState(user);
        NotifyAuthenticationStateChanged(Task.FromResult(_cached));
    }

    public void NotifyLogout()
    {
        _cached = Anonymous;
        NotifyAuthenticationStateChanged(Task.FromResult(_cached));
    }

    private static AuthenticationState BuildState(UserDto user)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Name, user.FullName)
        };
        claims.AddRange(user.Roles.Select(r => new Claim(ClaimTypes.Role, r)));

        var identity = new ClaimsIdentity(claims, "cookie");
        return new AuthenticationState(new ClaimsPrincipal(identity));
    }
}
