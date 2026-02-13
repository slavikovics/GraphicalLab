using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace GraphicalLab.Controls;

public class PixelGridOverlay : Control
{
    public static readonly StyledProperty<int> BitmapWidthProperty =
        AvaloniaProperty.Register<PixelGridOverlay, int>(nameof(BitmapWidth));

    public static readonly StyledProperty<int> BitmapHeightProperty =
        AvaloniaProperty.Register<PixelGridOverlay, int>(nameof(BitmapHeight));

    public int BitmapWidth
    {
        get => GetValue(BitmapWidthProperty);
        set => SetValue(BitmapWidthProperty, value);
    }

    public int BitmapHeight
    {
        get => GetValue(BitmapHeightProperty);
        set => SetValue(BitmapHeightProperty, value);
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);

        if (BitmapWidth <= 0 || BitmapHeight <= 0)
            return;

        var pen = new Pen(new SolidColorBrush(Color.FromArgb(30, 255, 255, 255)), 1);

        double cellWidth = Bounds.Width / BitmapWidth;
        double cellHeight = Bounds.Height / BitmapHeight;
        
        for (int x = 0; x <= BitmapWidth; x++)
        {
            double px = x * cellWidth;
            context.DrawLine(pen, new Point(px, 0), new Point(px, Bounds.Height));
        }
        
        for (int y = 0; y <= BitmapHeight; y++)
        {
            double py = y * cellHeight;
            context.DrawLine(pen, new Point(0, py), new Point(Bounds.Width, py));
        }
    }
}
