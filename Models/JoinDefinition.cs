using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace DbQueryExplorer.Models;

public partial class JoinDefinition : ObservableObject
{
    public static IReadOnlyList<string> JoinTypes { get; } =
        new[] { "INNER JOIN", "LEFT JOIN", "RIGHT JOIN", "FULL OUTER JOIN" };

    [ObservableProperty] private bool   _isEnabled   = true;
    [ObservableProperty] private string _joinType    = "INNER JOIN";
    [ObservableProperty] private string _tableName   = string.Empty;
    [ObservableProperty] private string _tableAlias  = string.Empty;
    [ObservableProperty] private string _leftColumn  = string.Empty;
    [ObservableProperty] private string _rightColumn = string.Empty;

    public ObservableCollection<JoinExtraCondition> ExtraConditions  { get; } = new();
    public ObservableCollection<string>             AvailableColumns { get; set; } = new();
}
