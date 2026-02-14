using System;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;

namespace GraphicalLab.Controls.WaypointControl;

public class WaypointControl : Ellipse
{
    private DispatcherTimer? _throttleTimer;
    private Point _lastProcessedPosition;
    private bool _hasPendingUpdate;

    public static readonly StyledProperty<ICommand?> ClickCommandProperty =
        AvaloniaProperty.Register<WaypointControl, ICommand?>(nameof(ClickCommand));

    public ICommand? ClickCommand
    {
        get => GetValue(ClickCommandProperty);
        set => SetValue(ClickCommandProperty, value);
    }

    public static readonly StyledProperty<ICommand?> DragCommandProperty =
        AvaloniaProperty.Register<WaypointControl, ICommand?>(nameof(DragCommand));

    public ICommand? DragCommand
    {
        get => GetValue(DragCommandProperty);
        set => SetValue(DragCommandProperty, value);
    }

    private bool _isDragging;
    private Point _dragStartPosition;
    private double _initialRelativeX, _initialRelativeY;
    private Canvas? _canvas;
    private Size _canvasSizeAtDragStart;

    public WaypointControl()
    {
        Width = 16;
        Height = 16;
        Fill = new SolidColorBrush(Color.FromArgb(30, 255, 255, 255));

        PointerPressed += OnPointerPressed;
        PointerMoved += OnPointerMoved;
        PointerReleased += OnPointerReleased;

        _throttleTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(10)
        };
        _throttleTimer.Tick += OnThrottleTimerTick;
    }

    private void OnThrottleTimerTick(object? sender, EventArgs e)
    {
        if (!_hasPendingUpdate || !_isDragging || _canvas == null)
        {
            _throttleTimer?.Stop();
            return;
        }

        UpdatePosition(_lastProcessedPosition);
        _hasPendingUpdate = false;
    }

    private void UpdatePosition(Point currentAbsolutePosition)
    {
        var model = DataContext as WaypointModel;
        if (model == null) return;

        var deltaX = currentAbsolutePosition.X - _dragStartPosition.X;
        var deltaY = currentAbsolutePosition.Y - _dragStartPosition.Y;

        double relativeDeltaX = _canvasSizeAtDragStart.Width > 0
            ? deltaX / _canvasSizeAtDragStart.Width
            : 0;
        double relativeDeltaY = _canvasSizeAtDragStart.Height > 0
            ? deltaY / _canvasSizeAtDragStart.Height
            : 0;

        double newRelativeX = _initialRelativeX + relativeDeltaX;
        double newRelativeY = _initialRelativeY + relativeDeltaY;

        model.X = newRelativeX;
        model.Y = newRelativeY;

        DragCommand?.Execute(model);
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        _canvas = this.FindAncestorOfType<Canvas>();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        _canvas = null;
    }

    private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (_canvas == null || !e.GetCurrentPoint(this).Properties.IsLeftButtonPressed) return;

        var model = DataContext as WaypointModel;
        if (model == null) return;

        _isDragging = true;

        _dragStartPosition = e.GetPosition(_canvas);
        _canvasSizeAtDragStart = _canvas.Bounds.Size;

        _initialRelativeX = model.X;
        _initialRelativeY = model.Y;

        e.Pointer.Capture(this);
        e.Handled = true;
    }

    private void OnPointerMoved(object? sender, PointerEventArgs e)
    {
        if (!_isDragging || _canvas == null) return;

        var currentPosition = e.GetPosition(_canvas);
        _lastProcessedPosition = currentPosition;
        _hasPendingUpdate = true;

        if (!_throttleTimer!.IsEnabled)
            _throttleTimer.Start();

        e.Handled = true;
    }

    private void OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (!_isDragging || _canvas == null) return;

        _throttleTimer?.Stop();
        _hasPendingUpdate = false;

        var model = DataContext as WaypointModel;
        if (model == null) return;

        _isDragging = false;
        e.Pointer.Capture(null);

        var endPosition = e.GetPosition(_canvas);
        var dragDistance = Math.Sqrt(
            Math.Pow(endPosition.X - _dragStartPosition.X, 2) +
            Math.Pow(endPosition.Y - _dragStartPosition.Y, 2)
        );

        if (dragDistance < 5)
        {
            ClickCommand?.Execute(model);
        }

        e.Handled = true;
    }
}