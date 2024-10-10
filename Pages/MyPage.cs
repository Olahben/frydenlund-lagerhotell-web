using Lagerhotell.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using MudBlazor;

namespace Lagerhotell.Pages
{
    public class MyPage : ComponentBase
    {
        [Inject]
        public ISnackbar Snackbar { get; set; }
        [Inject]
        public IDialogService DialogService { get; set; }
        [Inject]
        public NavigationManager NavigationManager { get; set; }
        [Inject]
        public SessionService SessionService { get; set; }
        [Inject]
        public AuthenticationStateProvider AuthenticationStateProvider { get; set; }
        [Inject]
        public AuthStateProviderService CustomAuthenticationStateProvider { get; set; }
        [Inject]
        public IConfiguration Configuration { get; set; }
        protected override async Task OnInitializedAsync()
        {
            var userJwt = await SessionService.GetJwtFromLocalStorage();
            if (userJwt != null)
            {
                var authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
                // AuthStateProvider handles the case where the JWT is present but the user is not authenticated
                var user = await CustomAuthenticationStateProvider.GetUser();
                if (user.User.FirstName != null)
                {
                    if (!user.User.IsEmailVerified)
                    {
                        if (NavigationManager.Uri != Configuration["HostSettings:HostUrl"] + "/verifiser-epost")
                        {
                            NavigationManager.NavigateTo("/verifiser-epost");
                        }
                    }
                    else
                    {
                        if (NavigationManager.Uri == Configuration["HostSettings:HostUrl"] + "/verifiser-epost")
                        {
                            NavigationManager.NavigateTo($"/user/{user.User.Id}");
                        }
                    }
                }
                else
                {
                    if (!user.CompanyUser.IsEmailVerified)
                    {
                        if (NavigationManager.Uri != Configuration["HostSettings:HostUrl"] + "/verifiser-epost")
                        {
                            NavigationManager.NavigateTo("/verifiser-epost");
                        }
                    }
                    else
                    {
                        if (NavigationManager.Uri == Configuration["HostSettings:HostUrl"] + "/verifiser-epost")
                        {
                            NavigationManager.NavigateTo($"/user/{user.CompanyUser.CompanyUserId}");
                        }
                    }
                }
            }
        }
    }
}
