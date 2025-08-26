using System.Windows;
using System.Windows.Media;
using Microsoft.Xaml.Behaviors;

namespace WpfAppG2RoundedBorder;

public class G2RoundRectClipBehaviour : Behavior<UIElement>
{
    private readonly PathGeometry _pathGeometry = new();

    private Geometry? _originClip;


    #region CornerRadius

    public double CornerRadius
    {
        get => (double)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    public static double GetCornerRadius(DependencyObject element) =>
        element != null
            ? (double)element.GetValue(CornerRadiusProperty)
            : throw new ArgumentNullException(nameof(element));

    public static void SetCornerRadius(DependencyObject element,
                                       double           value
    )
    {
        if (element != null)
        {
            element.SetValue(CornerRadiusProperty, (object)value);
        }
        else
        {
            throw new ArgumentNullException(nameof(element));
        }
    }

    public static readonly DependencyProperty CornerRadiusProperty =
        DependencyProperty.Register(
                                    nameof(CornerRadius),
                                    typeof(double),
                                    typeof(G2RoundRectClipBehaviour),
                                    new FrameworkPropertyMetadata(4D, FrameworkPropertyMetadataOptions.Inherits, CornerRadiusPropertyChanged)
                                   );


    private static void CornerRadiusPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is G2RoundRectClipBehaviour thisG2RoundRectClipBehaviour &&
            //e.OldValue is double oldValue                              &&
            e.NewValue is double newValue)
        {
            thisG2RoundRectClipBehaviour.UpdatePathGeometry();
        }
    }

    #endregion

    private void UpdatePathGeometry()
    {
        if (AssociatedObject is null)
        {
            return;
        }

        var width  = AssociatedObject.RenderSize.Width;
        var height = AssociatedObject.RenderSize.Height;

        var widthCornerRadius  = Math.Min(width  / 2d, CornerRadius);
        var heightCornerRadius = Math.Min(height / 2d, CornerRadius);

        var points = new List<Point>()
                     {
                         new(0, heightCornerRadius),
                         new(widthCornerRadius, 0),
                         new(width / 2d, 0),
                         new(width - widthCornerRadius, 0),
                         new(width, heightCornerRadius),
                         new(width, height / 2d),
                         new(width, height - heightCornerRadius),
                         new(width         - widthCornerRadius, height),
                         new(width / 2d, height),
                         new(widthCornerRadius, height),
                         new(0, height - heightCornerRadius),
                         new(0, height / 2d),
                     };

        var pathFigureCollection = new PathFigureCollection
                                   {
                                       new PathFigure()
                                       {
                                           IsClosed   = true,
                                           StartPoint = new Point(0, height / 2d),

                                           Segments = new PathSegmentCollection([new PolyBezierSegment(points, false)])
                                       }
                                   };
        _pathGeometry.Figures  = pathFigureCollection;
        _pathGeometry.FillRule = FillRule.EvenOdd;
        AssociatedObject.Clip  = _pathGeometry;
    }


    protected override void OnAttached()
    {
        base.OnAttached();
        _originClip           = AssociatedObject.Clip;
        AssociatedObject.Clip = _pathGeometry;

        if (AssociatedObject is FrameworkElement fe)
        {
            fe.SizeChanged += Fe_SizeChanged;
        }
    }

    private void Fe_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        UpdatePathGeometry();
    }

    protected override void OnDetaching()
    {
        base.OnDetaching();
        AssociatedObject.Clip = _originClip;
        if (AssociatedObject is FrameworkElement fe)
        {
            fe.SizeChanged -= Fe_SizeChanged;
        }
    }
}