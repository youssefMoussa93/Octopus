using Microsoft.Data.Sqlite;

namespace Octopus.Services;

public class SavedRepo
{
    public int Id { get; set; }
    public string Path { get; set; } = "";
    public string Name { get; set; } = "";
    public DateTime AddedAt { get; set; }
}

public class RepoStore
{
    private readonly string _connectionString;

    public RepoStore()
    {
        var dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Octopus");
        Directory.CreateDirectory(dir);
        var dbPath = Path.Combine(dir, "repos.db");
        _connectionString = $"Data Source={dbPath}";
        Init();
    }

    private void Init()
    {
        using var conn = Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = """
            CREATE TABLE IF NOT EXISTS Repos (
                Id      INTEGER PRIMARY KEY AUTOINCREMENT,
                Path    TEXT NOT NULL UNIQUE,
                Name    TEXT NOT NULL DEFAULT '',
                AddedAt TEXT NOT NULL
            )
            """;
        cmd.ExecuteNonQuery();
    }

    public async Task<List<SavedRepo>> GetAllAsync()
    {
        using var conn = Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT Id, Path, Name, AddedAt FROM Repos ORDER BY AddedAt DESC";
        using var reader = await cmd.ExecuteReaderAsync();
        var list = new List<SavedRepo>();
        while (await reader.ReadAsync())
            list.Add(Map(reader));
        return list;
    }

    public async Task<SavedRepo> SaveAsync(string path)
    {
        var name = System.IO.Path.GetFileName(path.TrimEnd('/', '\\'));
        using var conn = Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = """
            INSERT INTO Repos (Path, Name, AddedAt)
            VALUES ($path, $name, $at)
            ON CONFLICT(Path) DO NOTHING;
            SELECT Id, Path, Name, AddedAt FROM Repos WHERE Path = $path;
            """;
        cmd.Parameters.AddWithValue("$path", path);
        cmd.Parameters.AddWithValue("$name", name);
        cmd.Parameters.AddWithValue("$at", DateTime.UtcNow.ToString("o"));
        using var reader = await cmd.ExecuteReaderAsync();
        await reader.ReadAsync();
        return Map(reader);
    }

    public async Task DeleteAsync(int id)
    {
        using var conn = Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "DELETE FROM Repos WHERE Id = $id";
        cmd.Parameters.AddWithValue("$id", id);
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task RenameAsync(int id, string name)
    {
        using var conn = Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "UPDATE Repos SET Name = $name WHERE Id = $id";
        cmd.Parameters.AddWithValue("$name", name);
        cmd.Parameters.AddWithValue("$id", id);
        await cmd.ExecuteNonQueryAsync();
    }

    private SqliteConnection Open()
    {
        var conn = new SqliteConnection(_connectionString);
        conn.Open();
        return conn;
    }

    private static SavedRepo Map(SqliteDataReader r) => new()
    {
        Id = r.GetInt32(0),
        Path = r.GetString(1),
        Name = r.GetString(2),
        AddedAt = DateTime.Parse(r.GetString(3))
    };
}
