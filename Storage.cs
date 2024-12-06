using Dapper;
using MySqlConnector;

namespace SkyboxChanger;

public class SkyData
{
  public ulong SteamID { get; set; }
  public string Skybox { get; set; } = "";
  public float Brightness { get; set; } = 1.0f;
  public int Color { get; set; } = int.MaxValue;

  public bool HasSkybox()
  {
    return Skybox != "";
  }
}

public class Storage
{
  private string _DbConnString { get; set; }
  private string _Table { get; set; } = "";

  private List<SkyData> _PlayerStorage { get; set; } = new();

  public Storage(string host, int port, string user, string password, string database, string tablePrefix)
  {
    _DbConnString = $"Host={host};Port={port};User={user};Password={password};Database={database}";
    _Table = tablePrefix + "playerstorage";
    using MySqlConnection connection = ConnectAsync().Result;
    var createTableQuery = $"CREATE TABLE IF NOT EXISTS `{_Table}` ( `steamid` BIGINT NOT NULL, `skybox` VARCHAR(255) NOT NULL, `brightness` FLOAT DEFAULT 1.0, `color` INT NOT NULL, PRIMARY KEY (`steamid`)) ENGINE = InnoDB;";
    connection.Execute(createTableQuery);
    Load();
  }

  public async Task<MySqlConnection> ConnectAsync()
  {
    MySqlConnection connection = new(_DbConnString);
    await connection.OpenAsync();
    return connection;
  }

  public void ExecuteAsync(string query, object? parameters)
  {
    Task.Run(async () =>
    {
      using MySqlConnection connection = await ConnectAsync();
      await connection.ExecuteAsync(query, parameters);
    });
  }

  public void Load()
  {
    _PlayerStorage.Clear();
    using MySqlConnection connection = ConnectAsync().Result;
    var result = connection.Query<SkyData>($"SELECT * FROM `{_Table}`;");
    _PlayerStorage.AddRange(result);
  }

  public void Save()
  {
    foreach (var data in _PlayerStorage)
    {
      ExecuteAsync($"INSERT INTO `{_Table}` (`steamid`, `skybox`, `brightness`, `color`) VALUES (@SteamID, @Skybox, @Brightness, @Color) ON DUPLICATE KEY UPDATE `skybox` = @Skybox, `brightness` = @Brightness, `color` = @Color;", data);
    }
  }

  public SkyData GetPlayerSkydata(ulong steamid)
  {
    var data = _PlayerStorage.Find((data) => data.SteamID == steamid);
    if (data == null)
    {
      data = new SkyData { SteamID = steamid };
      _PlayerStorage.Add(data);
    }
    return data;
  }
}