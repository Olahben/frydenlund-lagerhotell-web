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

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddMudServices();
builder.Services.AddScoped<SessionService>();
builder.Services.AddScoped<AuthStateProviderService>();
builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<AuthenticationStateProvider, AuthStateProviderService>();
builder.Services.AddScoped<LocationService>();
builder.Services.AddScoped<WarehouseHotelService>();
builder.Services.AddScoped<LagerhotellXService>();
builder.Services.AddScoped<FileHandler>();
builder.Services.AddScoped<StorageUnitService>();
builder.Services.AddScoped<StorageUnitValidator>();
builder.Services.AddScoped<TextHandler>();
builder.Services.AddScoped<AssetService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<UserServiceLogin>();
builder.Services.AddScoped<OrderService>();
builder.Services.AddSingleton<Lagerhotell.Services.State.AppState>();

await builder.Build().RunAsync();