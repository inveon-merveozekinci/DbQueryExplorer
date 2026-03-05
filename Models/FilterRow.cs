using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace DbQueryExplorer.Models;

public partial class FilterRow : ObservableObject
{
    [ObservableProperty]
    private bool _isEnabled = true;

    [ObservableProperty]
    private string _columnName = string.Empty;

    [ObservableProperty]
    private string _selectedOperator = "=";

    [ObservableProperty]
    private string _value = string.Empty;

    [ObservableProperty]
    private bool _valueInputVisible = true;

    public ObservableCollection<string> AvailableColumns { get; set; } = new();

    public static readonly IReadOnlyList<string> Operators =
    [
        "=", "!=", ">", "<", ">=", "<=",
        "LIKE", "NOT LIKE",
        "IS NULL", "IS NOT NULL",
        "IN"
    ];

    partial void OnSelectedOperatorChanged(string value)
    {
        ValueInputVisible = value is not ("IS NULL" or "IS NOT NULL");
    }
}
