using CommunityToolkit.Mvvm.ComponentModel;

namespace DbQueryExplorer.Models;

public partial class SelectableColumn : ObservableObject
{
    public string Name     { get; init; } = string.Empty;
    public string DataType { get; init; } = string.Empty;

    [ObservableProperty] private bool _isSelected = true;

    public override string ToString() => Name;
}
