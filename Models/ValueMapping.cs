using CommunityToolkit.Mvvm.ComponentModel;

namespace DbQueryExplorer.Models;

public partial class ValueMapping : ObservableObject
{
    [ObservableProperty] private string _sourceValue  = string.Empty;
    [ObservableProperty] private string _displayValue = string.Empty;
}
