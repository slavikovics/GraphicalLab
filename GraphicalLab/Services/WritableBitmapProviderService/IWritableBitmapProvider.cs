using Avalonia.Media.Imaging;
using GraphicalLab.Models;

namespace GraphicalLab.Services.WritableBitmapProviderService;

public interface IWritableBitmapProvider
{
    void SetPixel(Pixel pixel);
    void ClearBitmap();
    int GetBitmapWidth();
    int GetBitmapHeight();
    WriteableBitmap GetBitmap();
}