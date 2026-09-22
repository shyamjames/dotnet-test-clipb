using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.IO;

public class ClipboardItem
{
    public int Id { get; set; }
    public string Content { get; set; } = "";
    public string Type { get; set; } = "";
    public DateTime Timestamp { get; set; }
    public bool IsPinned { get; set; }
}

public static class DbHelper
{
    private static readonly string DbPath =
        Path.Combine(AppContext.BaseDirectory, "clipboard.db");
    private static readonly string ConnStr = $"Data Source={DbPath}";

    public static void Init()
    {
        using var conn = new SqliteConnection(ConnStr);
        conn.Open();
        var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            CREATE TABLE IF NOT EXISTS ClipboardItems (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Content TEXT NOT NULL,
                Type TEXT NOT NULL,
                Timestamp TEXT NOT NULL,
                IsPinned INTEGER NOT NULL DEFAULT 0
            );";
        cmd.ExecuteNonQuery();
    }

    public static void InsertItem(string content, string type)
    {
        using var conn = new SqliteConnection(ConnStr);
        conn.Open();
        var cmd = conn.CreateCommand();
        cmd.CommandText = @"INSERT INTO ClipboardItems (Content, Type, Timestamp, IsPinned)
                             VALUES ($content, $type, $ts, 0)";
        cmd.Parameters.AddWithValue("$content", content);
        cmd.Parameters.AddWithValue("$type", type);
        cmd.Parameters.AddWithValue("$ts", DateTime.Now.ToString("o"));
        cmd.ExecuteNonQuery();
    }

    public static List<ClipboardItem> GetAll()
    {
        var list = new List<ClipboardItem>();
        using var conn = new SqliteConnection(ConnStr);
        conn.Open();
        var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT Id, Content, Type, Timestamp, IsPinned FROM ClipboardItems ORDER BY IsPinned DESC, Timestamp DESC";
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            list.Add(new ClipboardItem
            {
                Id = reader.GetInt32(0),
                Content = reader.GetString(1),
                Type = reader.GetString(2),
                Timestamp = DateTime.Parse(reader.GetString(3)),
                IsPinned = reader.GetInt32(4) == 1
            });
        }
        return list;
    }

    public static List<ClipboardItem> Search(string query)
    {
        var list = new List<ClipboardItem>();
        using var conn = new SqliteConnection(ConnStr);
        conn.Open();
        var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT Id, Content, Type, Timestamp, IsPinned FROM ClipboardItems WHERE Content LIKE $q ORDER BY IsPinned DESC, Timestamp DESC";
        cmd.Parameters.AddWithValue("$q", $"%{query}%");
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            list.Add(new ClipboardItem
            {
                Id = reader.GetInt32(0),
                Content = reader.GetString(1),
                Type = reader.GetString(2),
                Timestamp = DateTime.Parse(reader.GetString(3)),
                IsPinned = reader.GetInt32(4) == 1
            });
        }
        return list;
    }

    public static void Delete(int id)
    {
        using var conn = new SqliteConnection(ConnStr);
        conn.Open();
        var cmd = conn.CreateCommand();
        cmd.CommandText = "DELETE FROM ClipboardItems WHERE Id = $id";
        cmd.Parameters.AddWithValue("$id", id);
        cmd.ExecuteNonQuery();
    }

    public static void TogglePin(int id, bool pinned)
    {
        using var conn = new SqliteConnection(ConnStr);
        conn.Open();
        var cmd = conn.CreateCommand();
        cmd.CommandText = "UPDATE ClipboardItems SET IsPinned = $p WHERE Id = $id";
        cmd.Parameters.AddWithValue("$p", pinned ? 1 : 0);
        cmd.Parameters.AddWithValue("$id", id);
        cmd.ExecuteNonQuery();
    }

    public static void ClearAll()
    {
        using var conn = new SqliteConnection(ConnStr);
        conn.Open();
        var cmd = conn.CreateCommand();
        cmd.CommandText = "DELETE FROM ClipboardItems WHERE IsPinned = 0";
        cmd.ExecuteNonQuery();
    }

    public static void InitSettings()
    {
        using var conn = new SqliteConnection(ConnStr);
        conn.Open();
        var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            CREATE TABLE IF NOT EXISTS Settings (
                Key TEXT PRIMARY KEY,
                Value TEXT NOT NULL
            );";
        cmd.ExecuteNonQuery();
    }

    public static string GetSetting(string key, string defaultValue)
    {
        using var conn = new SqliteConnection(ConnStr);
        conn.Open();
        var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT Value FROM Settings WHERE Key = $k";
        cmd.Parameters.AddWithValue("$k", key);
        var result = cmd.ExecuteScalar();
        return result?.ToString() ?? defaultValue;
    }

    public static void SetSetting(string key, string value)
    {
        using var conn = new SqliteConnection(ConnStr);
        conn.Open();
        var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            INSERT INTO Settings (Key, Value) VALUES ($k, $v)
            ON CONFLICT(Key) DO UPDATE SET Value = $v";
        cmd.Parameters.AddWithValue("$k", key);
        cmd.Parameters.AddWithValue("$v", value);
        cmd.ExecuteNonQuery();
    }

    public static void TrimToLimit(int maxEntries)
    {
        using var conn = new SqliteConnection(ConnStr);
        conn.Open();
        var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            DELETE FROM ClipboardItems 
            WHERE IsPinned = 0 AND Id NOT IN (
                SELECT Id FROM ClipboardItems 
                WHERE IsPinned = 0 
                ORDER BY Timestamp DESC 
                LIMIT $limit
            )";
        cmd.Parameters.AddWithValue("$limit", maxEntries);
        cmd.ExecuteNonQuery();
    }
}