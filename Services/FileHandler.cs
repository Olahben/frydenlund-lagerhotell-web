using Microsoft.AspNetCore.Components.Forms;

namespace Lagerhotell.Services
{
    public class FileHandler
    {
        public async Task<byte[]> ConvertToByteArray(IBrowserFile file)
        {
            using (var memoryStream = new MemoryStream())
            {
                await file.OpenReadStream().CopyToAsync(memoryStream);
                return memoryStream.ToArray();
            }
        }
    }
}
