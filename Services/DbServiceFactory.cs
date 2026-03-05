using DbQueryExplorer.Models;

namespace DbQueryExplorer.Services;

public static class DbServiceFactory
{
    public static IDbService Create(ConnectionInfo info) =>
        info.DbType switch
        {
            DatabaseType.MySQL  => new MySqlDbService(info),
            DatabaseType.MSSQL  => new MsSqlDbService(info),
            _                    => throw new NotSupportedException($"Unsupported DB type: {info.DbType}")
        };
}
