using Microsoft.AspNetCore.Components.Forms;

namespace Lagerhotell.Models;

public class ImageAssetBrowserFile : IBrowserFile
{
    private readonly ImageAsset _imageAsset;

    public ImageAssetBrowserFile(ImageAsset imageAsset)
    {
        _imageAsset = imageAsset;
        Name = imageAsset.Name;
        LastModified = DateTimeOffset.Now;
        Size = imageAsset.ImageBytes.LongLength;
        ContentType = "image/jpeg";
    }

    public string Name { get; private set; }
    public DateTimeOffset LastModified { get; private set; }
    public long Size { get; private set; }
    public string ContentType { get; private set; }

    public Stream OpenReadStream(long maxAllowedSize = 5000000, CancellationToken cancellationToken = default)
    {
        if (Size > maxAllowedSize)
        {
            throw new InvalidOperationException($"The file size exceeds the allowed limit of {maxAllowedSize} bytes.");
        }
        cancellationToken.ThrowIfCancellationRequested();
        return new MemoryStream(_imageAsset.ImageBytes);
    }
}
