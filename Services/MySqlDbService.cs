using System.Data;
using System.Text;
using DbQueryExplorer.Models;
using MySqlConnector;

namespace DbQueryExplorer.Services;

public sealed class MySqlDbService : IDbService
{
    private readonly MySqlConnection _conn;

    public MySqlDbService(ConnectionInfo info)
    {
        var builder = new MySqlConnectionStringBuilder
        {
            Server = info.Server,
            Port = (uint)info.Port,
            Database = info.Database,
            UserID = info.Username,
            Password = info.Password,
            AllowZeroDateTime = true,
            ConvertZeroDateTime = true,
            ConnectionTimeout = 15
        };
        _conn = new MySqlConnection(builder.ConnectionString);
    }

    public async Task ConnectAsync()
    {
        if (_conn.State != ConnectionState.Open)
            await _conn.OpenAsync();
    }

    public async Task<List<string>> GetTablesAsync()
    {
        await EnsureOpenAsync();
        var tables = new List<string>();
        using var cmd = _conn.CreateCommand();
        cmd.CommandText = "SHOW FULL TABLES WHERE Table_type = 'BASE TABLE'";
        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
            tables.Add(reader.GetString(0));
        return tables;
    }

    public async Task<List<ColumnInfo>> GetColumnsAsync(string tableName)
    {
        await EnsureOpenAsync();
        var columns = new List<ColumnInfo>();
        using var cmd = _conn.CreateCommand();
        cmd.CommandText = $"DESCRIBE `{tableName}`";
        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
            columns.Add(new ColumnInfo { Name = reader.GetString(0), DataType = reader.GetString(1) });
        return columns;
    }

    public async Task<DataTable> QueryAsync(string tableName, IEnumerable<FilterRow> filters, IEnumerable<string>? columns = null, CancellationToken ct = default)
    {
        await EnsureOpenAsync();
        var (sql, parameters) = BuildQuery($"`{tableName}`", filters, "`", columns);
        using var cmd = _conn.CreateCommand();
        cmd.CommandText = sql;
        foreach (var (key, val) in parameters)
            cmd.Parameters.AddWithValue(key, val);

        var dt = new DataTable();
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        dt.Load(reader);
        return dt;
    }

    public async Task<DataTable> ExecuteRawSqlAsync(string sql, Dictionary<string, object>? parameters = null, CancellationToken ct = default)
    {
        await EnsureOpenAsync();
        using var cmd = _conn.CreateCommand();
        cmd.CommandText = sql;
        cmd.CommandTimeout = 120;
        if (parameters != null)
            foreach (var (key, val) in parameters)
                cmd.Parameters.AddWithValue(key, val ?? DBNull.Value);
        var dt = new DataTable();
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        dt.Load(reader);
        return dt;
    }

    private async Task EnsureOpenAsync()
    {
        if (_conn.State != ConnectionState.Open)
            await _conn.OpenAsync();
    }

    private static (string Sql, Dictionary<string, object> Params) BuildQuery(
        string quotedTable, IEnumerable<FilterRow> filters, string q, IEnumerable<string>? columns = null)
    {
        var parameters = new Dictionary<string, object>();
        var colList = columns?.ToList();
        var selectPart = (colList is { Count: > 0 })
            ? string.Join(", ", colList.Select(c => $"`{c}`"))
            : "*";
        var sb = new StringBuilder($"SELECT {selectPart} FROM {quotedTable}");
        var active = filters.Where(f => f.IsEnabled && !string.IsNullOrWhiteSpace(f.ColumnName)).ToList();

        if (active.Count > 0)
        {
            sb.Append(" WHERE ");
            var conditions = new List<string>();
            int i = 0;
            foreach (var f in active)
            {
                var col = $"{q}{f.ColumnName}{q}";
                var op = f.SelectedOperator;

                if (op is "IS NULL")       { conditions.Add($"{col} IS NULL"); continue; }
                if (op is "IS NOT NULL")   { conditions.Add($"{col} IS NOT NULL"); continue; }

                if (op is "IN")
                {
                    var parts = (f.Value ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries);
                    var pnames = parts.Select((_, idx) => $"@p{i + idx}").ToList();
                    for (int j = 0; j < parts.Length; j++)
                        parameters[$"@p{i + j}"] = parts[j].Trim();
                    i += parts.Length;
                    conditions.Add($"{col} IN ({string.Join(", ", pnames)})");
                    continue;
                }

                var pname = $"@p{i++}";
                var val = (object?)(f.Value ?? "");
                if (op is "LIKE" or "NOT LIKE") val = $"%{val}%";
                parameters[pname] = val ?? DBNull.Value;
                conditions.Add($"{col} {op} {pname}");
            }
            sb.Append(string.Join(" AND ", conditions));
        }

        return (sb.ToString(), parameters);
    }

    public void Dispose() => _conn.Dispose();
}
