namespace DbQueryExplorer.Models;

public class ColumnInfo
{
    public string Name { get; set; } = string.Empty;
    public string DataType { get; set; } = string.Empty;

    public override string ToString() => Name;
}
