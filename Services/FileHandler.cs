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
            try
            {
                using (var memoryStream = new MemoryStream())
                {
                    await file.OpenReadStream(maxAllowedSize: 5000000).CopyToAsync(memoryStream);
                    // Reset the stream position to the beginning before reading from it
                    memoryStream.Seek(0, SeekOrigin.Begin);
                    return memoryStream.ToArray();
                }
            }
            catch (IOException e)
            {
                throw new IOException("Error converting file to byte array, file is likely too large", e);
            }
        }
        public async Task<string> ImageByteArrayToBase64(byte[] image)
        {
            return await Task.Run(() => Convert.ToBase64String(image));
        }
    }
}
