namespace RmtoSync.Data;

public sealed class SqlQueryLoader
{
    private readonly string _queriesRoot;
    private readonly Dictionary<string, string> _cache = new(StringComparer.OrdinalIgnoreCase);

    public SqlQueryLoader(IHostEnvironment environment)
    {
        _queriesRoot = Path.Combine(AppContext.BaseDirectory, "Queries");
        if (!Directory.Exists(_queriesRoot))
        {
            var devRoot = Path.GetFullPath(Path.Combine(
                environment.ContentRootPath,
                "..", "..", "RmtoSync", "Queries"));

            if (Directory.Exists(devRoot))
                _queriesRoot = devRoot;
        }
    }

    public string Load(string queryName)
    {
        if (_cache.TryGetValue(queryName, out var cached))
            return cached;

        var path = Path.Combine(_queriesRoot, $"{queryName}.sql");
        if (!File.Exists(path))
            throw new FileNotFoundException($"SQL query file not found: {path}", path);

        var sql = File.ReadAllText(path);
        _cache[queryName] = sql;
        return sql;
    }
}
