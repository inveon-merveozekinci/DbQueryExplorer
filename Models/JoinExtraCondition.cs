using CommunityToolkit.Mvvm.ComponentModel;

namespace DbQueryExplorer.Models;

public partial class JoinExtraCondition : ObservableObject
{
    [ObservableProperty] private string _column = string.Empty;
    [ObservableProperty] private string _value  = string.Empty;
}
