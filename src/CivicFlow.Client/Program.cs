using CivicFlow.Client;
using CivicFlow.Client.Auth;
using CivicFlow.Client.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// ── HTTP client ───────────────────────────────────────────────────────────────
// AuthDelegatingHandler intercepts 401 and redirects to /login
builder.Services.AddTransient<AuthDelegatingHandler>();
builder.Services.AddHttpClient<CivicFlowApiClient>(client =>
    client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress))
    .AddHttpMessageHandler<AuthDelegatingHandler>();

// ── Auth ──────────────────────────────────────────────────────────────────────
builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<CookieAuthStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(
    sp => sp.GetRequiredService<CookieAuthStateProvider>());

await builder.Build().RunAsync();
