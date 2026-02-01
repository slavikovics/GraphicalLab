using System;
using System.Collections.Generic;
using System.ComponentModel;
using Avalonia.Media.Imaging;

namespace GraphicalLab.Services.DebugControlService;

public interface IDebuggableBitmapControl : INotifyPropertyChanged, INotifyPropertyChanging
{
    string StepsCountText { get; set; }
    bool IsGridVisible { get; set; }
    bool IsDebugEnabled { get; set; }
    bool IsNextStepAvailable { get; }
    event Action WritableBitmapChanged;
    void AddPoints(List<Pixel> points);
    void ClearBitmap();
    void HandleDebugNextStep();
    void SetPixel(Pixel pixel);
    WriteableBitmap GetBitmap();
    int GetBitmapWidth();
    int GetBitmapHeight();
}