using System;
using System.Collections.Generic;
using System.ComponentModel;
using Avalonia.Media.Imaging;
using GraphicalLab.Models;

namespace GraphicalLab.Services.DebugControlService;

public interface IDebuggableBitmapControl : INotifyPropertyChanged, INotifyPropertyChanging
{
    string StepsCountText { get; set; }
    bool IsGridVisible { get; set; }
    bool IsDebugEnabled { get; set; }
    bool IsNextStepAvailable { get; }
    event Action WritableBitmapChanged;
    void AddPoints(List<Pixel> points);
    void AddPointsToCenter(List<Pixel> points);
    void ClearBitmap(bool redraw = false);
    void HandleDebugNextStep();
    void HandleBulk(int count);
    void SetPixel(Pixel pixel);
    uint[,] GetPixelMatrix();
    WriteableBitmap GetBitmap();
    int GetBitmapWidth();
    int GetBitmapHeight();
}