using CommunityToolkit.Mvvm.ComponentModel;

namespace DbQueryExplorer.Models;

public partial class ColumnExpression : ObservableObject
{
    [ObservableProperty] private string _expression = string.Empty;
    [ObservableProperty] private string _alias      = string.Empty;
    [ObservableProperty] private bool   _isEnabled  = true;
}
