namespace DbQueryExplorer.Models;

public class ConnectionProfile
{
    public string Name     { get; set; } = string.Empty;
    public string DbType   { get; set; } = "MySQL";
    public string Server   { get; set; } = "localhost";
    public int    Port     { get; set; } = 3306;
    public string Database { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;

    public ConnectionInfo ToConnectionInfo() => new()
    {
        DbType   = DbType.Equals("MSSQL", StringComparison.OrdinalIgnoreCase)
                        ? DatabaseType.MSSQL : DatabaseType.MySQL,
        Server   = Server,
        Port     = Port,
        Database = Database,
        Username = Username,
        Password = Password
    };

    public override string ToString() => Name;
}

public class ConnectionsConfig
{
    public List<ConnectionProfile> Connections { get; set; } = new();
}
