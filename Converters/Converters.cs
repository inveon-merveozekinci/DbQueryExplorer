using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace DbQueryExplorer.Converters;

[ValueConversion(typeof(bool), typeof(bool))]
public class BoolNegationConverter : IValueConverter
{
    public static readonly BoolNegationConverter Instance = new();
    public object Convert(object value, Type t, object p, CultureInfo c) => value is bool b && !b;
    public object ConvertBack(object value, Type t, object p, CultureInfo c) => value is bool b && !b;
}

[ValueConversion(typeof(bool), typeof(Visibility))]
public class BoolToVisibilityConverter : IValueConverter
{
    public static readonly BoolToVisibilityConverter Instance = new();
    public object Convert(object value, Type t, object p, CultureInfo c)
        => value is bool b && b ? Visibility.Visible : Visibility.Collapsed;
    public object ConvertBack(object value, Type t, object p, CultureInfo c)
        => value is Visibility v && v == Visibility.Visible;
}

/// <summary>bool == true  →  Collapsed,  false  →  Visible  (inverse of BoolToVisibilityConverter)</summary>
[ValueConversion(typeof(bool), typeof(Visibility))]
public class BoolNegToVisibilityConverter : IValueConverter
{
    public static readonly BoolNegToVisibilityConverter Instance = new();
    public object Convert(object value, Type t, object p, CultureInfo c)
        => value is bool b && b ? Visibility.Collapsed : Visibility.Visible;
    public object ConvertBack(object value, Type t, object p, CultureInfo c)
        => value is Visibility v && v == Visibility.Collapsed;
}

/// <summary>int == 0  →  Collapsed  (used to show the header row only when there are filters)</summary>
[ValueConversion(typeof(int), typeof(Visibility))]
public class CountToVisibilityConverter : IValueConverter
{
    public static readonly CountToVisibilityConverter Instance = new();
    public bool Invert { get; set; }
    public object Convert(object value, Type t, object p, CultureInfo c)
    {
        bool hasItems = value is int i && i > 0;
        bool show = Invert ? !hasItems : hasItems;
        return show ? Visibility.Visible : Visibility.Collapsed;
    }
    public object ConvertBack(object value, Type t, object p, CultureInfo c) => Binding.DoNothing;
}
