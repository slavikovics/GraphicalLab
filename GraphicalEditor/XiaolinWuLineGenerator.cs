using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using Avalonia.Media;

namespace GraphicalEditor;

public static class XiaolinWuLineGenerator
{
    private static double FractionalPart(double x)
        => x - Math.Floor(x);

    private static double RFractionalPart(double x)
        => 1.0 - FractionalPart(x);

    public static List<Pixel> DrawLine(Pixel start, Pixel end, uint color)
    {
        var pixels = new List<Pixel>();

        double x1 = start.X;
        double y1 = start.Y;
        double x2 = end.X;
        double y2 = end.Y;

        bool steep = Math.Abs(y2 - y1) > Math.Abs(x2 - x1);

        if (steep)
        {
            (x1, y1) = (y1, x1);
            (x2, y2) = (y2, x2);
        }

        if (x1 > x2)
        {
            (x1, x2) = (x2, x1);
            (y1, y2) = (y2, y1);
        }

        double dx = x2 - x1;
        double dy = y2 - y1;
        double gradient = dx == 0 ? 1 : dy / dx;

        double xEnd = Math.Round(x1);
        double yEnd = y1 + gradient * (xEnd - x1);
        double xGap = RFractionalPart(x1 + 0.5);

        int xPixel1 = (int)xEnd;
        int yPixel1 = (int)Math.Floor(yEnd);

        if (steep)
        {
            pixels.Add(new Pixel(yPixel1, xPixel1, color, RFractionalPart(yEnd) * xGap));
            pixels.Add(new Pixel(yPixel1 + 1, xPixel1, color, FractionalPart(yEnd) * xGap));
        }
        else
        {
            pixels.Add(new Pixel(xPixel1, yPixel1, color, RFractionalPart(yEnd) * xGap));
            pixels.Add(new Pixel(xPixel1, yPixel1 + 1, color, FractionalPart(yEnd) * xGap));
        }

        double intery = yEnd + gradient;

        xEnd = Math.Round(x2);
        yEnd = y2 + gradient * (xEnd - x2);
        xGap = FractionalPart(x2 + 0.5);

        int xPixel2 = (int)xEnd;
        int yPixel2 = (int)Math.Floor(yEnd);

        if (steep)
        {
            pixels.Add(new Pixel(yPixel2, xPixel2, color, RFractionalPart(yEnd) * xGap));
            pixels.Add(new Pixel(yPixel2 + 1, xPixel2, color, FractionalPart(yEnd) * xGap));
        }
        else
        {
            pixels.Add(new Pixel(xPixel2, yPixel2, color, RFractionalPart(yEnd) * xGap));
            pixels.Add(new Pixel(xPixel2, yPixel2 + 1, color, FractionalPart(yEnd) * xGap));
        }

        if (steep)
        {
            for (int x = xPixel1 + 1; x < xPixel2; x++)
            {
                int y = (int)Math.Floor(intery);
                pixels.Add(new Pixel(y, x, color, RFractionalPart(intery)));
                pixels.Add(new Pixel(y + 1, x, color, FractionalPart(intery)));
                intery += gradient;
            }
        }
        else
        {
            for (int x = xPixel1 + 1; x < xPixel2; x++)
            {
                int y = (int)Math.Floor(intery);
                pixels.Add(new Pixel(x, y, color, RFractionalPart(intery)));
                pixels.Add(new Pixel(x, y + 1, color, FractionalPart(intery)));
                intery += gradient;
            }
        }

        return pixels;
    }
}