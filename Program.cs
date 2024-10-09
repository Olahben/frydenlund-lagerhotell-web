using Lagerhotell;
using Lagerhotell.Services;
using LagerhotellAPI.Models.DomainModels.Validators;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddOidcAuthentication(options =>
{
    options.ProviderOptions.Authority = $"https://{builder.Configuration["Auth0:Domain"]}";
    options.ProviderOptions.ClientId = builder.Configuration["Auth0:ClientId"];
    options.ProviderOptions.DefaultScopes.Add("openid");
    options.ProviderOptions.DefaultScopes.Add("profile");
    options.ProviderOptions.DefaultScopes.Add("offline_access");
    options.ProviderOptions.DefaultScopes.Add("roles");

    options.UserOptions.RoleClaim = "https://lagerhotell.com/roles";
});


builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddMudServices();
builder.Services.AddScoped<SessionService>();
builder.Services.AddScoped<AuthenticationStateProvider, AuthStateProviderService>();
builder.Services.AddScoped<AuthStateProviderService>();
builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<LocationService>();
builder.Services.AddScoped<WarehouseHotelService>();
builder.Services.AddScoped<LagerhotellXService>();
builder.Services.AddScoped<FileHandler>();
builder.Services.AddScoped<StorageUnitService>();
builder.Services.AddScoped<StorageUnitValidator>();
builder.Services.AddScoped<Lagerhotell.Services.AuthenticateUserService>();
builder.Services.AddScoped<TextHandler>();
builder.Services.AddScoped<AssetService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<UserServiceLogin>();
builder.Services.AddScoped<OrderService>();
builder.Services.AddScoped<CompanyUserService>();
builder.Services.AddScoped<CompanyUserValidator>();
builder.Services.AddSingleton<Lagerhotell.Services.State.AppState>();
builder.Services.AddScoped<Auth0Service>();
builder.Services.AddScoped<UserServiceSignup>();

await builder.Build().RunAsync();