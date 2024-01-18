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

        public async Task<string> GetJwtFromLocalStorage()
        {
            var token = await jsRuntime.InvokeAsync<string>("localStorage.getItem", "jwtToken");
            return token ?? throw new Exception("Brukeren har ingen JWT");
        }
    }
}
