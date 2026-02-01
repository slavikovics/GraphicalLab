using Avalonia.Media.Imaging;

namespace GraphicalLab.Services.WritableBitmapProviderService;

public interface IWritableBitmapProvider
{
    void SetPixel(Pixel pixel);
    void ClearBitmap();
    int GetBitmapWidth();
    int GetBitmapHeight();
    WriteableBitmap GetBitmap();
}