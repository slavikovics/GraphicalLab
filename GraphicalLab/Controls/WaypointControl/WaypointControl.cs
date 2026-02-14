using System;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.VisualTree;

namespace GraphicalLab.Controls.WaypointControl;

public class WaypointControl : Ellipse
{
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

        // Сохраняем позицию начала перетаскивания в абсолютных координатах Canvas
        _dragStartPosition = e.GetPosition(_canvas);

        // Сохраняем размер Canvas в момент начала перетаскивания
        _canvasSizeAtDragStart = _canvas.Bounds.Size;

        // Сохраняем начальные относительные координаты модели
        _initialRelativeX = model.X;
        _initialRelativeY = model.Y;

        e.Pointer.Capture(this);
        e.Handled = true;
    }

    private void OnPointerMoved(object? sender, PointerEventArgs e)
    {
        if (!_isDragging || _canvas == null) return;

        var model = DataContext as WaypointModel;
        if (model == null) return;

        // Текущая позиция мыши в абсолютных координатах Canvas
        var currentAbsolutePosition = e.GetPosition(_canvas);

        // Вычисляем смещение в абсолютных пикселях
        var deltaX = currentAbsolutePosition.X - _dragStartPosition.X;
        var deltaY = currentAbsolutePosition.Y - _dragStartPosition.Y;

        // Конвертируем абсолютное смещение в относительное (с учетом текущего размера Canvas)
        // Используем размер Canvas на момент начала перетаскивания для консистентности
        double relativeDeltaX = _canvasSizeAtDragStart.Width > 0
            ? deltaX / _canvasSizeAtDragStart.Width
            : 0;
        double relativeDeltaY = _canvasSizeAtDragStart.Height > 0
            ? deltaY / _canvasSizeAtDragStart.Height
            : 0;

        // Вычисляем новую относительную позицию
        double newRelativeX = _initialRelativeX + relativeDeltaX;
        double newRelativeY = _initialRelativeY + relativeDeltaY;

        // Обновляем модель (ограничение координат происходит в сеттере модели)
        model.X = newRelativeX;
        model.Y = newRelativeY;

        DragCommand?.Execute(model);
        e.Handled = true;
    }

    private void OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (!_isDragging || _canvas == null) return;

        var model = DataContext as WaypointModel;
        if (model == null) return;

        _isDragging = false;
        e.Pointer.Capture(null);

        // Проверяем, было ли это нажатие (не перетаскивание)
        var endPosition = e.GetPosition(_canvas);
        var dragDistance = Math.Sqrt(
            Math.Pow(endPosition.X - _dragStartPosition.X, 2) +
            Math.Pow(endPosition.Y - _dragStartPosition.Y, 2)
        );

        // Если движение было минимальным, считаем это кликом
        if (dragDistance < 5) // Порог чувствительности
        {
            ClickCommand?.Execute(model);
        }

        e.Handled = true;
    }
}