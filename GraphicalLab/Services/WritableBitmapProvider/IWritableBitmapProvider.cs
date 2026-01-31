using Avalonia.Controls;
using Avalonia.Media.Imaging;

namespace GraphicalLab;

public interface IWritableBitmapProvider
{
    void SetPixel(Pixel pixel);
    void ClearBitmap(Image image);
    int GetBitmapWidth();
    int GetBitmapHeight();
    WriteableBitmap GetBitmap();
}