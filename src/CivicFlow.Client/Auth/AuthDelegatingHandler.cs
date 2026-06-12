using System.Net;
using Microsoft.AspNetCore.Components;

namespace CivicFlow.Client.Auth;

public class AuthDelegatingHandler(NavigationManager nav) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var response = await base.SendAsync(request, cancellationToken);

        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            var returnUrl = Uri.EscapeDataString(nav.Uri);
            nav.NavigateTo($"/login?returnUrl={returnUrl}");
        }

        return response;
    }
}
