using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using GraphicalLab.Models;

namespace GraphicalLab.Services.WritableBitmapProviderService;

public class WritableBitmapProvider : IWritableBitmapProvider
{
    private readonly int _bitmapWidth;
    private readonly int _bitmapHeight;
    private readonly WriteableBitmap _bitmap;

    public WritableBitmapProvider(int bitmapWidth = 300, int bitmapHeight = 300)
    {
        _bitmapWidth = bitmapWidth;
        _bitmapHeight = bitmapHeight;
        _bitmap = new(
            new PixelSize(_bitmapWidth, _bitmapHeight),
            new Vector(96, 96),
            PixelFormat.Bgra8888,
            AlphaFormat.Premul);
    }
    
    public unsafe void SetPixel(Pixel pixel)
    {
        if (pixel.X < 0 || pixel.X >= _bitmapWidth || pixel.Y < 0 || pixel.Y >= _bitmapHeight)
            return;

        using var fb = _bitmap.Lock();
        uint* buffer = (uint*)fb.Address;
        int stride = fb.RowBytes / 4;

        buffer[pixel.Y * stride + pixel.X] = pixel.Color;
    }
    
    public unsafe void ClearBitmap()
    {
        using var fb = _bitmap.Lock();
        uint* buffer = (uint*)fb.Address;
        int stride = fb.RowBytes / 4;

        for (int x = 0; x < _bitmapWidth; x++)
        {
            for (int y = 0; y < _bitmapHeight; y++)
            {
                buffer[y * stride + x] = 0;
            }
        }
    }
    
    public int GetBitmapWidth()
    {
        return _bitmapWidth;
    }

    public int GetBitmapHeight()
    {
        return _bitmapHeight;
    }
    
    public WriteableBitmap GetBitmap()
    {
        return _bitmap;
    }
}