using Microsoft.AspNetCore.Components;
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

    }
}
