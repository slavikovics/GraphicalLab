using Avalonia.Controls;
using Avalonia.Media.Imaging;

namespace GraphicalLab.Services.WritableBitmapProviderService;

public interface IWritableBitmapProvider
{
    void SetPixel(Pixel pixel);
    void ClearBitmap(Image image);
    int GetBitmapWidth();
    int GetBitmapHeight();
    WriteableBitmap GetBitmap();
}