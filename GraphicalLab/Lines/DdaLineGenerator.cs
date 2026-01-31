using System;
using System.Collections.Generic;

namespace GraphicalLab.Lines;

public static class DdaLineGenerator
{
    public static List<Pixel> DrawLine(Pixel start, Pixel end, uint color)
    {
        List<Pixel> newPoints = [];
    
        if (start.X == end.X && start.Y == end.Y)
        {
            newPoints.Add(start);
            return newPoints;
        }
    
        int x1 = start.X, y1 = start.Y;
        int x2 = end.X, y2 = end.Y;
        
        int dx = x2 - x1;
        int dy = y2 - y1;
        int length = Math.Max(Math.Abs(dx), Math.Abs(dy));
        
        double xIncrement = (double)dx / length;
        double yIncrement = (double)dy / length;
        
        double x = x1;
        double y = y1;
        
        newPoints.Add(new Pixel((int)Math.Round(x), (int)Math.Round(y), color));
    
        for (int i = 0; i < length; i++)
        {
            x += xIncrement;
            y += yIncrement;
            newPoints.Add(new Pixel((int)Math.Round(x), (int)Math.Round(y), color));
        }
    
        return newPoints;
    }
}