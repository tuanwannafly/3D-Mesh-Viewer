using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace MeshViewer.Views;

/// <summary>
/// A row used in the inspector: axis label (X/Y/Z) + modern slider + numeric TextBox.
/// Emits <see cref="ValueChanged"/> whenever the underlying value changes.
/// </summary>
public partial class AxisRowControl : UserControl
{
    public static readonly RoutedEvent ValueChangedEvent =
        EventManager.RegisterRoutedEvent(
            nameof(ValueChanged),
            RoutingStrategy.Bubble,
            typeof(RoutedPropertyChangedEventHandler<double>),
            typeof(AxisRowControl));

    public event RoutedPropertyChangedEventHandler<double> ValueChanged
    {
        add => AddHandler(ValueChangedEvent, value);
        remove => RemoveHandler(ValueChangedEvent, value);
    }

    public AxisRowControl()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Logical name of the underlying slider; used by the host window to map rows back
    /// to slider identifiers declared in the original XAML (RotateXSlider, etc.).
    /// </summary>
    public string SliderName
    {
        get => (string)GetValue(SliderNameProperty);
        set => SetValue(SliderNameProperty, value);
    }

    public static readonly DependencyProperty SliderNameProperty =
        DependencyProperty.Register(nameof(SliderName), typeof(string), typeof(AxisRowControl),
            new PropertyMetadata(string.Empty));

    public string Axis
    {
        get => (string)GetValue(AxisProperty);
        set => SetValue(AxisProperty, value);
    }

    public static readonly DependencyProperty AxisProperty =
        DependencyProperty.Register(nameof(Axis), typeof(string), typeof(AxisRowControl),
            new PropertyMetadata("X"));

    public double Minimum
    {
        get => (double)GetValue(MinimumProperty);
        set => SetValue(MinimumProperty, value);
    }

    public static readonly DependencyProperty MinimumProperty =
        DependencyProperty.Register(nameof(Minimum), typeof(double), typeof(AxisRowControl),
            new PropertyMetadata(0.0));

    public double Maximum
    {
        get => (double)GetValue(MaximumProperty);
        set => SetValue(MaximumProperty, value);
    }

    public static readonly DependencyProperty MaximumProperty =
        DependencyProperty.Register(nameof(Maximum), typeof(double), typeof(AxisRowControl),
            new PropertyMetadata(100.0));

    public double Tick
    {
        get => (double)GetValue(TickProperty);
        set => SetValue(TickProperty, value);
    }

    public static readonly DependencyProperty TickProperty =
        DependencyProperty.Register(nameof(Tick), typeof(double), typeof(AxisRowControl),
            new PropertyMetadata(1.0));

    public double Value
    {
        get => (double)GetValue(ValueProperty);
        set
        {
            var clamped = Math.Clamp(value, Minimum, Maximum);
            if (Math.Abs((double)GetValue(ValueProperty) - clamped) > double.Epsilon)
            {
                SetValue(ValueProperty, clamped);
            }
            else if (Math.Abs(value - clamped) > double.Epsilon)
            {
                // outside range: still update display to reflect clamp
                SetValue(ValueProperty, clamped);
            }
        }
    }

    public static readonly DependencyProperty ValueProperty =
        DependencyProperty.Register(nameof(Value), typeof(double), typeof(AxisRowControl),
            new FrameworkPropertyMetadata(0.0,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnValueChanged));

    /// <summary>
    /// Pure helper: clamps a value into the [Minimum, Maximum] range. Exposed so
    /// the behavior can be unit tested without instantiating a WPF control (which
    /// requires an STA thread).
    /// </summary>
    public static double ClampToRange(double value, double minimum, double maximum)
    {
        return Math.Clamp(value, minimum, maximum);
    }

    private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var c = (AxisRowControl)d;
        var args = new RoutedPropertyChangedEventArgs<double>((double)e.OldValue, (double)e.NewValue, ValueChangedEvent);
        c.RaiseEvent(args);
    }

    /// <summary>
    /// Raised when the user finishes dragging the slider or commits a value in the text box.
    /// Used to trigger transform rebuild without firing on every pixel of drag.
    /// </summary>
    public event EventHandler? ValueCommitted;

    private void ValueSlider_DragCompleted(object sender, DragCompletedEventArgs e) => ValueCommitted?.Invoke(this, EventArgs.Empty);

    private void ValueSlider_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e) => ValueCommitted?.Invoke(this, EventArgs.Empty);

    private void ValueBox_LostFocus(object sender, RoutedEventArgs e)
    {
        CommitBoxText();
        ValueCommitted?.Invoke(this, EventArgs.Empty);
    }

    private void ValueBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            CommitBoxText();
            ValueCommitted?.Invoke(this, EventArgs.Empty);
            // Move focus to viewport to avoid keyboard focus being stuck on text box
            Keyboard.ClearFocus();
        }
    }

    private void CommitBoxText()
    {
        if (double.TryParse(ValueBox.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed))
        {
            Value = parsed;
        }
        else
        {
            // Reset to current Value (which reformats via the binding's StringFormat)
            ValueBox.GetBindingExpression(TextBox.TextProperty)?.UpdateTarget();
        }
    }
}