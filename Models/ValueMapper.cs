using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace DbQueryExplorer.Models;

public partial class ValueMapper : ObservableObject
{
    [ObservableProperty] private bool   _isEnabled    = true;
    [ObservableProperty] private string _sourceColumn = string.Empty;
    [ObservableProperty] private string _outputAlias  = string.Empty;
    [ObservableProperty] private string _defaultValue = string.Empty;

    public ObservableCollection<ValueMapping> Mappings         { get; } = new();
    public ObservableCollection<string>       AvailableColumns { get; set; } = new();
}
