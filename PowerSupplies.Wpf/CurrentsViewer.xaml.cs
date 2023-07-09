using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Media;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Wpf;
using PowerSupplies.Core;
using LinearAxis = OxyPlot.Axes.LinearAxis;
using LineSeries = OxyPlot.Series.LineSeries;
using TimeSpanAxis = OxyPlot.Axes.TimeSpanAxis;

namespace PowerSupplies.Wpf;

public partial class CurrentsViewer
{
    #region Color
    public static readonly DependencyProperty ColorProperty;
    public Color Color
    {
        get => (Color) GetValue(ColorProperty);
        set => SetValue(ColorProperty, value);
    }

    private static void ColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is CurrentsViewer viewer)
        {
            viewer.ColorChanged();
        }
    }

    private void ColorChanged()
    {
        _series.Color = Color.ToOxyColor();
        View.InvalidatePlot(false);
    }
    #endregion

    #region Points
    public static readonly DependencyProperty PointsProperty;
    public IEnumerable<IReadOnlyCurrentPoint> Points
    {
        get => (IEnumerable<IReadOnlyCurrentPoint>) GetValue(PointsProperty);
        set => SetValue(PointsProperty, value);
    }

    private static void PointsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is CurrentsViewer viewer)
        {
            viewer.PointChanged(e.OldValue);
        }
    }

    private void PointChanged(object old)
    {
        _series.ItemsSource = Points;

        if (old is INotifyCollectionChanged oldCollectionChanged)
        {
            oldCollectionChanged.CollectionChanged -= UpdateChart;
        }

        if (Points is INotifyCollectionChanged collectionChanged)
        {
            collectionChanged.CollectionChanged += UpdateChart;
        }
    }
    #endregion

    static CurrentsViewer()
    {
        ColorProperty = DependencyProperty.Register(
            nameof(Color),
            typeof(Color),
            typeof(CurrentsViewer),
            new FrameworkPropertyMetadata(Colors.Black, ColorChanged)
        );

        PointsProperty = DependencyProperty.Register(
            nameof(Points),
            typeof(IEnumerable<IReadOnlyCurrentPoint>),
            typeof(CurrentsViewer),
            new FrameworkPropertyMetadata(default(IEnumerable<IReadOnlyCurrentPoint>), PointsChanged)
        );
    }

    public CurrentsViewer()
    {
        InitializeComponent();

        View.Model = new PlotModel();

        View.Model.Axes.Add(new TimeSpanAxis
        {
            Position = AxisPosition.Bottom,
            Title = Properties.Resources.Time,
            IsPanEnabled = false,
            MajorGridlineStyle = LineStyle.Dot,
            IsZoomEnabled = false
        });

        View.Model.Axes.Add(new LinearAxis
        {
            Position = AxisPosition.Left,
            Title = Properties.Resources.Current,
            IsPanEnabled = false,
            MajorGridlineStyle = LineStyle.Dot,
            IsZoomEnabled = false
        });

        _series = new LineSeries
        {
            MarkerStrokeThickness = 2.0,
            Color = Color.ToOxyColor(),
            DataFieldX = nameof(IReadOnlyCurrentPoint.Time),
            DataFieldY = nameof(IReadOnlyCurrentPoint.Value)
        };

        View.Model.Series.Add(_series);
        View.InvalidatePlot();
    }

    private void UpdateChart(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (DateTime.Now.Subtract(_lastUpdateTime) > Interval)
        {
            View.InvalidatePlot();
            _lastUpdateTime = DateTime.Now;
        }
    }
    private DateTime _lastUpdateTime = DateTime.Now;

    private readonly LineSeries _series;
    private static readonly TimeSpan Interval = TimeSpan.FromSeconds(1.0);
}