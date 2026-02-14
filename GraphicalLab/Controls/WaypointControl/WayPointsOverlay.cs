using System;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Styling;

namespace GraphicalLab.Controls.WaypointControl;

public class WayPointsOverlay : ItemsControl
{
    public static readonly StyledProperty<ICommand?> CanvasClickCommandProperty =
        AvaloniaProperty.Register<WayPointsOverlay, ICommand?>(nameof(CanvasClickCommand));

    public ICommand? CanvasClickCommand
    {
        get => GetValue(CanvasClickCommandProperty);
        set => SetValue(CanvasClickCommandProperty, value);
    }

    public static readonly StyledProperty<ICommand?> ItemClickedCommandProperty =
        AvaloniaProperty.Register<WayPointsOverlay, ICommand?>(nameof(ItemClickedCommand));

    public ICommand? ItemClickedCommand
    {
        get => GetValue(ItemClickedCommandProperty);
        set => SetValue(ItemClickedCommandProperty, value);
    }

    public static readonly StyledProperty<ICommand?> ItemDraggedCommandProperty =
        AvaloniaProperty.Register<WayPointsOverlay, ICommand?>(nameof(ItemDraggedCommand));

    public ICommand? ItemDraggedCommand
    {
        get => GetValue(ItemDraggedCommandProperty);
        set => SetValue(ItemDraggedCommandProperty, value);
    }

    private readonly Canvas _canvas;

    protected override Type StyleKeyOverride => typeof(ItemsControl);

    public WayPointsOverlay()
    {
        _canvas = new Canvas
        {
            Background = new SolidColorBrush(Colors.Transparent)
        };

        ItemsPanel = new FuncTemplate<Panel>(() => _canvas)!;
        _canvas.PointerPressed += OnCanvasPointerPressed;

        ItemTemplate = new FuncDataTemplate<WaypointModel>((model, ns) =>
            new WaypointControl
            {
                [!WaypointControl.ClickCommandProperty] = new Binding(nameof(ItemClickedCommand))
                {
                    RelativeSource = new RelativeSource
                        { Mode = RelativeSourceMode.FindAncestor, AncestorType = typeof(WayPointsOverlay) }
                },
                [!WaypointControl.DragCommandProperty] = new Binding(nameof(ItemDraggedCommand))
                {
                    RelativeSource = new RelativeSource
                        { Mode = RelativeSourceMode.FindAncestor, AncestorType = typeof(WayPointsOverlay) },
                }
            });

        var containerStyle = new Style(x => x.OfType<ContentPresenter>())
        {
            Setters =
            {
                new Setter(Canvas.LeftProperty, new MultiBinding
                {
                    Bindings =
                    {
                        new Binding(nameof(WaypointModel.X)),
                        new Binding("Bounds.Width")
                        {
                            RelativeSource = new RelativeSource
                                { Mode = RelativeSourceMode.FindAncestor, AncestorType = typeof(Canvas) }
                        }
                    },
                    Converter = new RelativeToAbsoluteConverter(),
                    ConverterParameter = 8.0
                }),
                new Setter(Canvas.TopProperty, new MultiBinding
                {
                    Bindings =
                    {
                        new Binding(nameof(WaypointModel.Y)),
                        new Binding("Bounds.Height")
                        {
                            RelativeSource = new RelativeSource
                                { Mode = RelativeSourceMode.FindAncestor, AncestorType = typeof(Canvas) }
                        }
                    },
                    Converter = new RelativeToAbsoluteConverter(),
                    ConverterParameter = 8.0
                })
            }
        };
        Styles.Add(containerStyle);
    }

    private void OnCanvasPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (ReferenceEquals(e.Source, _canvas) && e.GetCurrentPoint(_canvas).Properties.IsLeftButtonPressed)
        {
            var pos = e.GetPosition(_canvas);

            if (_canvas.Bounds.Width > 0 && _canvas.Bounds.Height > 0)
            {
                var relativePos = new Point(
                    pos.X / _canvas.Bounds.Width,
                    pos.Y / _canvas.Bounds.Height
                );

                CanvasClickCommand?.Execute(relativePos);
                e.Handled = true;
            }
        }
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        _canvas.PointerPressed -= OnCanvasPointerPressed;
    }
}