using System;
using System.Reactive;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;

namespace BeforeAfterImageView.Controls;

public partial class BeforeAfterImageViewer : UserControl
{
    public static readonly StyledProperty<IImage?> LeftSourceProperty =
        AvaloniaProperty.Register<BeforeAfterImageViewer, IImage?>(nameof(LeftSource));

    public static readonly StyledProperty<IImage?> RightSourceProperty =
        AvaloniaProperty.Register<BeforeAfterImageViewer, IImage?>(nameof(RightSource));

    private double _splitPosition = 100; // Position in viewport coordinates
    private bool _isDragging;

    public BeforeAfterImageViewer()
    {
        InitializeComponent();
        
        DragHandleOverlay.PointerPressed += DragHandle_PointerPressed;
        DragHandleOverlay.PointerMoved += DragHandle_PointerMoved;
        DragHandleOverlay.PointerReleased += DragHandle_PointerReleased;

        ZoomBorder.ZoomChanged += (_, _) => UpdateClip();
        ZoomBorder.PropertyChanged += (_, e) =>
        {
            if (e.Property.Name == nameof(ZoomBorder.OffsetX) || 
                e.Property.Name == nameof(ZoomBorder.OffsetY))
            {
                UpdateClip();
            }
        };
        
        this.GetObservable(BoundsProperty)
            .Subscribe(_ =>
            {
                UpdateClip();
            }); 

        this.GetObservable(LeftSourceProperty)
            .Subscribe(Observer.Create<IImage?>(v =>
            {
                LeftImage.Source = v;
            }));

        this.GetObservable(RightSourceProperty)
            .Subscribe(Observer.Create<IImage?>(v =>
            {
                RightImage.Source = v;
            }));


    }

    public IImage? LeftSource
    {
        get => GetValue(LeftSourceProperty);
        set => SetValue(LeftSourceProperty, value);
    }

    public IImage? RightSource
    {
        get => GetValue(RightSourceProperty);
        set => SetValue(RightSourceProperty, value);
    }

    private void UpdateClip()
    {
        if (ZoomBorder == null || RightImage == null) return;

        var zoomX = ZoomBorder.ZoomX;
        var zoomY = ZoomBorder.ZoomY;
        
        if (zoomX <= 0 || zoomY <= 0) return;
        
        
        // Convert split position from viewport to ZoomBorder coordinates
        var splitPositionX=  (_splitPosition - ZoomBorder.OffsetX) / zoomX;
        var splitPositionY= (0 - ZoomBorder.OffsetY) / zoomY;
        var newWidth = (ZoomBorder.Bounds.Width - _splitPosition) / zoomX;
        var newHeight = ZoomBorder.Bounds.Height / zoomY;
        
        // Clip the right image to only show the portion after the split
        var xGrid = ImageGrid.Bounds.X;
        var xImg = LeftImage.Bounds.X;
        var deltaX = xImg - xGrid; 
        
        RightImage.Clip = new RectangleGeometry(
            new Rect(splitPositionX - deltaX, splitPositionY, newWidth, newHeight));

        // Update drag handle position (already in viewport coordinates)
        DragHandleOverlay.Margin = new Thickness(_splitPosition, 0, 0, 0);
    }

    private void DragHandle_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        _isDragging = true;
        e.Pointer.Capture(DragHandleOverlay);
        e.Handled = true;
    }

    private void DragHandle_PointerMoved(object? sender, PointerEventArgs e)
    {
        if (!_isDragging) return;

        var position = e.GetPosition(this);
        _splitPosition = Math.Clamp(position.X, 0, Bounds.Width - 10);
        
        UpdateClip();
        e.Handled = true;
    }

    private void DragHandle_PointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (!_isDragging) return;

        e.Pointer.Capture(null);
        _isDragging = false;
        e.Handled = true;
    }

}