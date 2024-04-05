using Microsoft.AspNetCore.Components.Forms;

namespace Lagerhotell.Services
{
    public class FileHandler
    {

        /// <summary>
        /// Converts a IBrowserFile to a byte array
        /// </summary>
        /// <param name="file"></param>
        /// <returns>byte array</returns>
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
