using System.Data;
using DbQueryExplorer.Models;

namespace DbQueryExplorer.Services;

public interface IDbService : IDisposable
{
    Task ConnectAsync();
    Task<List<string>> GetTablesAsync();
    Task<List<ColumnInfo>> GetColumnsAsync(string tableName);
    Task<DataTable> QueryAsync(string tableName, IEnumerable<FilterRow> filters, IEnumerable<string>? columns = null, CancellationToken ct = default);
    Task<DataTable> ExecuteRawSqlAsync(string sql, Dictionary<string, object>? parameters = null, CancellationToken ct = default);
}
