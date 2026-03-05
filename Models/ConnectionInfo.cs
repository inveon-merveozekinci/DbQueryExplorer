namespace DbQueryExplorer.Models;

public class ConnectionInfo
{
    public DatabaseType DbType { get; set; } = DatabaseType.MySQL;
    public string Server { get; set; } = "localhost";
    public int Port { get; set; } = 3306;
    public string Database { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public enum DatabaseType
{
    MySQL,
    MSSQL
}
