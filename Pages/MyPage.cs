using Lagerhotell.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using MudBlazor;

namespace Lagerhotell.Pages
{
    public class MyPage : ComponentBase
    {
        [Inject]
        protected ISnackbar Snackbar { get; set; }
        [Inject]
        protected IDialogService DialogService { get; set; }
        [Inject]
        protected NavigationManager NavigationManager { get; set; }
        [Inject]
        public SessionService SessionService { get; set; }
        //[Inject]
        // public AuthenticationStateProvider AuthenticationStateProvider { get; set; }
        protected override async Task OnInitializedAsync()
        {
            var userJwt = await SessionService.GetJwtFromLocalStorage();
            /*if (userJwt != null)
            {
                var authState = await AuthenticationStateProvider
            .GetAuthenticationStateAsync();
                var user = authState.User;
                if (user.Identity.IsAuthenticated)
                {

                }
                // AuthStateProvider handles the case where the JWT is present but the user is not authenticated
            }*/
        }
    }
}
