using Microsoft.JSInterop;

namespace Lagerhotell.Services.UserService
{
    public class SessionService
    {
        private readonly IJSRuntime jsRuntime;
        public SessionService(IJSRuntime jsRuntime)
        {
            this.jsRuntime = jsRuntime;
        }
        public async Task? AddJwtToLocalStorage(string jwt)
        {
            await jsRuntime.InvokeVoidAsync("localStorage.setItem", "jwtToken", jwt);
        }
    }
}
