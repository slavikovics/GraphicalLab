using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Input;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GraphicalLab.Models;
using GraphicalLab.Points;
using GraphicalLab.Services.DebugControlService;
using GraphicalLab.Services.FigureLoaderService;
using GraphicalLab.Services.ToastManagerService;
using GraphicalLab.Transform;

namespace GraphicalLab.ViewModels;

public partial class TransformPageViewModel : ViewModelBase
{
    private readonly IToastManager _toastManager;
    private readonly IDebuggableBitmapControl _debuggableBitmapControl;
    private readonly IFigureLoader _figureLoader;
    private readonly Rotate _rotate;
    private readonly Move _movement;
    private readonly Scale _scaler;
    private readonly Reflection _reflection;
    private readonly Perspective _perspectiveProjection;
    private Figure? _loadedFigure;
    private Figure? _transformedFigure;

    public int BitmapWidth => _debuggableBitmapControl.GetBitmapWidth();
    public int BitmapHeight => _debuggableBitmapControl.GetBitmapHeight();
    public WriteableBitmap Bitmap => _debuggableBitmapControl.GetBitmap();

    public Image? TargetImage = null;

    [ObservableProperty] private bool _isNextStepAvailable;
    [ObservableProperty] private string _stepsCountText;
    [ObservableProperty] private int _selectedTransformIndex;

    public bool IsGridVisible
    {
        get;
        set
        {
            SetProperty(ref field, value);
            _debuggableBitmapControl.IsGridVisible = value;
        }
    }

    public bool IsDebugEnabled
    {
        get;
        set
        {
            SetProperty(ref field, value);
            _debuggableBitmapControl.IsDebugEnabled = value;
        }
    }

    [ObservableProperty] private List<string> _transformTypes =
        ["Перемещение", "Поворот", "Скалирование", "Отображение", "Перспектива"];

    [ObservableProperty] private bool _move;
    [ObservableProperty] private bool _rotated;
    [ObservableProperty] private bool _scale;
    [ObservableProperty] private bool _display;
    [ObservableProperty] private bool _perspective;

    private delegate void TransformDelegate();

    private Dictionary<int, TransformDelegate> _transformsTypesMatch = null!;

    public TransformPageViewModel(IToastManager toastManager, IDebuggableBitmapControl debuggableBitmapControl,
        IFigureLoader figureLoader, Rotate rotate, Move movement, Scale scaler, Perspective perspective, Reflection reflection)
    {
        _toastManager = toastManager;
        _debuggableBitmapControl = debuggableBitmapControl;
        _figureLoader = figureLoader;
        _perspectiveProjection = perspective;

        _rotate = rotate;
        _movement = movement;
        _scaler = scaler;
        _reflection = reflection;

        _debuggableBitmapControl.WritableBitmapChanged += UpdateImage;
        _debuggableBitmapControl.PropertyChanged += DebuggableBitmapControlOnPropertyChanged;
        PropertyChanged += OnPropertyChanged;

        InitializeTransforms();
        InitializeProperties();
    }

    private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(SelectedTransformIndex))
        {
            switch (SelectedTransformIndex)
            {
                case 0:
                    DisableSelection();
                    Move = true;
                    break;
                case 1:
                    DisableSelection();
                    Rotated = true;
                    break;
                case 2:
                    DisableSelection();
                    Scale = true;
                    break;
                case 3:
                    DisableSelection();
                    Display = true;
                    break;
                case 4:
                    DisableSelection();
                    Perspective = true;
                    break;
            }
        }
    }

    private void DisableSelection()
    {
        Move = false;
        Rotated = false;
        Scale = false;
        Display = false;
        Perspective = false;
    }

    private void DebuggableBitmapControlOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(_debuggableBitmapControl.IsNextStepAvailable))
        {
            IsNextStepAvailable = _debuggableBitmapControl.IsNextStepAvailable;
        }
        else if (e.PropertyName == nameof(_debuggableBitmapControl.StepsCountText))
        {
            StepsCountText = _debuggableBitmapControl.StepsCountText;
        }
        else if (e.PropertyName == nameof(_debuggableBitmapControl.IsDebugEnabled))
        {
            IsDebugEnabled = _debuggableBitmapControl.IsDebugEnabled;
        }
        else if (e.PropertyName == nameof(_debuggableBitmapControl.IsGridVisible))
        {
            IsGridVisible = _debuggableBitmapControl.IsGridVisible;
        }
    }

    private void InitializeProperties()
    {
        IsGridVisible = _debuggableBitmapControl.IsGridVisible;
        SelectedTransformIndex = 0;
        Move = true;
        IsNextStepAvailable = _debuggableBitmapControl.IsNextStepAvailable;
        IsDebugEnabled = _debuggableBitmapControl.IsDebugEnabled;
        StepsCountText = _debuggableBitmapControl.StepsCountText;
    }

    private void InitializeTransforms()
    {
        _transformsTypesMatch = new Dictionary<int, TransformDelegate>();
    }

    private void UpdateImage()
    {
        TargetImage?.InvalidateVisual();
    }

    private void Redraw()
    {
        if (_loadedFigure == null) return;
        //List<Pixel> pixels = _loadedFigure.Draw();
        List<Pixel> pixels = _perspectiveProjection.ApplyPerspective(_loadedFigure).Draw();


        _debuggableBitmapControl.ClearBitmap();
        _debuggableBitmapControl.AddPointsToCenter(pixels);
    }

    [RelayCommand]
    public async Task LoadFigure()
    {
        try
        {
            _loadedFigure = await _figureLoader.LoadFigure();
            _transformedFigure = _rotate.RotateFigure(_loadedFigure, 2, Direction.X);
            Redraw();
        }
        catch (Exception e)
        {
            _toastManager.ShowToast("Ошибка загрузки 3D объекта", e.Message, NotificationType.Error);
        }
    }

    [RelayCommand]
    public void MoveUp()
    {
    }

    [RelayCommand]
    public void MoveDown()
    {
    }

    [RelayCommand]
    public void RotateRight()
    {
    }

    [RelayCommand]
    public void RotateLeft()
    {
    }

    [RelayCommand]
    public void SizeIncrease()
    {
    }

    [RelayCommand]
    public void SizeDecrease()
    {
    }

    [RelayCommand]
    public void FlipHorizontal()
    {
    }

    [RelayCommand]
    public void FlipVertical()
    {
    }

    [RelayCommand]
    public void ShowPerspective()
    {
    }

    [RelayCommand]
    public void ClearBitmap()
    {
        _debuggableBitmapControl.ClearBitmap();
    }

    [RelayCommand]
    private void HandleDebugNextStep()
    {
        _debuggableBitmapControl.HandleDebugNextStep();
    }

    private void HandleRotate(KeyEventArgs e)
    {
        if (_loadedFigure == null) return;
        if (e.Key == Key.W)
        {
            _loadedFigure = _rotate.RotateFigure(_loadedFigure, 0.1, Direction.X);
            Redraw();
        }
        else if (e.Key == Key.S)
        {
            _loadedFigure = _rotate.RotateFigure(_loadedFigure, -0.1, Direction.X);
            Redraw();
        }
        else if (e.Key == Key.A)
        {
            _loadedFigure = _rotate.RotateFigure(_loadedFigure, 0.1, Direction.Y);
            Redraw();
        }
        else if (e.Key == Key.D)
        {
            _loadedFigure = _rotate.RotateFigure(_loadedFigure, -0.1, Direction.Y);
            Redraw();
        }
        else if (e.Key == Key.Q)
        {
            _loadedFigure = _rotate.RotateFigure(_loadedFigure, 0.1, Direction.Z);
            Redraw();
        }
        else if (e.Key == Key.E)
        {
            _loadedFigure = _rotate.RotateFigure(_loadedFigure, -0.1, Direction.Z);
            Redraw();
        }
    }

    private void HandleMove(KeyEventArgs e)
    {
        if (_loadedFigure == null) return;
        if (e.Key == Key.W)
        {
            _loadedFigure = _movement.MoveFigure(_loadedFigure, 0, -1, 0);
            Redraw();
        }
        else if (e.Key == Key.S)
        {
            _loadedFigure = _movement.MoveFigure(_loadedFigure, 0, 1, 0);
            Redraw();
        }
        else if (e.Key == Key.A)
        {
            _loadedFigure = _movement.MoveFigure(_loadedFigure, -1, 0, 0);
            Redraw();
        }
        else if (e.Key == Key.D)
        {
            _loadedFigure = _movement.MoveFigure(_loadedFigure, 1, 0, 0);
            Redraw();
        }
        else if (e.Key == Key.Q)
        {
            _loadedFigure = _movement.MoveFigure(_loadedFigure, 0, 0, 1);
            Redraw();
        }
        else if (e.Key == Key.E)
        {
            _loadedFigure = _movement.MoveFigure(_loadedFigure, 0, 0, -1);
            Redraw();
        }
    }

    private void HandleScale(KeyEventArgs e)
    {
        if (_loadedFigure == null) return;

        if (e.Key == Key.OemPlus)
        {
            _loadedFigure = _scaler.ScaleFigure(_loadedFigure, 1.1);
            Redraw();
        }
        else if (e.Key == Key.OemMinus)
        {
            _loadedFigure = _scaler.ScaleFigure(_loadedFigure, 0.9);
            Redraw();
        }
        else if (e.Key == Key.X)
        {
            _loadedFigure = _scaler.ScaleFigureX(_loadedFigure, 1.2);
            Redraw();
        }
        else if (e.Key == Key.Y)
        {
            _loadedFigure = _scaler.ScaleFigureY(_loadedFigure, 1.2);
            Redraw();
        }
        else if (e.Key == Key.Z)
        {
            _loadedFigure = _scaler.ScaleFigureZ(_loadedFigure, 1.2);
            Redraw();
        }
        else if (e.Key == Key.C)
        {
            _loadedFigure = _scaler.ScaleFigure(_loadedFigure, 1.0);
            Redraw();
        }
    }

    private void HandlePerspective(KeyEventArgs e)
    {
        if (_loadedFigure == null) return;

        if (e.Key == Key.P)
        {
            _loadedFigure = _perspectiveProjection.ApplyPerspective(_loadedFigure);
            Redraw();
        }
    }
    
    private void HandleReflection(KeyEventArgs e)
    {
        if (_loadedFigure == null) return;
    
        if (e.Key == Key.F)
        {
            _loadedFigure = _reflection.ReflectX(_loadedFigure);
            Redraw();
        }
        else if (e.Key == Key.G)
        {
            _loadedFigure = _reflection.ReflectY(_loadedFigure);
            Redraw();
        }
        else if (e.Key == Key.H)
        {
            _loadedFigure = _reflection.ReflectZ(_loadedFigure);
            Redraw();
        }
    }

    [RelayCommand]
    private void HandleKeyDown(KeyEventArgs e)
    {
        switch (SelectedTransformIndex)
        {
            case 0: HandleMove(e); break;
            case 1: HandleRotate(e); break;
            case 2: HandleScale(e); break;
            case 3: HandleReflection(e); break;
            case 4: HandlePerspective(e); break;
        }
    }
}