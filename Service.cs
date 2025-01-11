using System.Drawing;
using CounterStrikeSharp.API.Core;

namespace SkyboxChanger;

public class Service
{
  private Storage _Storage { get; set; }

  private SkyboxChanger _Plugin { get; set; }

  public Service(SkyboxChanger plugin, string host, int port, string user, string password, string database, string tablePrefix)
  {
    _Plugin = plugin;
    _Storage = new Storage(host, port, user, password, database, tablePrefix);
  }

  public bool SetSkybox(CCSPlayerController player, string index)
  {
    _Storage.GetPlayerSkydata(player.SteamID).Skybox = index;
    Skybox skybox = _Plugin.Config.Skyboxs[index];
    if (skybox.Brightness != null)
    {
      _Plugin.EnvManager.SetBrightness(player.Slot, skybox.Brightness.Value);
      _Storage.GetPlayerSkydata(player.SteamID).Brightness = skybox.Brightness.Value;
    }
    if (skybox.Color != null)
    {
      var colorData = skybox.Color.Split(" ");
      if (colorData.Length == 4)
      {
        var r = int.Parse(colorData[0]);
        var g = int.Parse(colorData[1]);
        var b = int.Parse(colorData[2]);
        var a = int.Parse(colorData[3]);
        _Plugin.EnvManager.SetTintColor(player.Slot, Color.FromArgb(a, r, g, b));
        _Storage.GetPlayerSkydata(player.SteamID).Color = Color.FromArgb(a, r, g, b).ToArgb();
      }
    }
    return _Plugin.EnvManager.SetSkybox(player.Slot, skybox);
  }

  public void SetBrightness(CCSPlayerController player, float brightness)
  {
    _Storage.GetPlayerSkydata(player.SteamID).Brightness = brightness;
    _Plugin.EnvManager.SetBrightness(player.Slot, brightness);
  }

  public void SetTintColor(CCSPlayerController player, Color color)
  {
    _Storage.GetPlayerSkydata(player.SteamID).Color = color.ToArgb();
    _Plugin.EnvManager.SetTintColor(player.Slot, color);
  }

  public Skybox? GetPlayerSkybox(CCSPlayerController player)
  {
    var skyboxData = _Storage.GetPlayerSkydata(player.SteamID);
    return _Plugin.Config.Skyboxs.GetValueOrDefault(skyboxData.Skybox);
  }

  public float GetPlayerBrightness(CCSPlayerController player)
  {
    return _Storage.GetPlayerSkydata(player.SteamID).Brightness;
  }

  public Color GetPlayerColor(CCSPlayerController player)
  {
    return Color.FromArgb(_Storage.GetPlayerSkydata(player.SteamID).Color);
  }

  public Skybox? GetMapDefaultSkybox(string map)
  {
    var maps = _Plugin.Config.MapDefault;
    if (maps == null) return null;
    if (maps.ContainsKey(map)) return _Plugin.Config.Skyboxs[maps[map]];
    if (maps.ContainsKey("*")) return _Plugin.Config.Skyboxs[maps["*"]];
    return null;
  }

  public void Save(ulong? steamid = null)
  {
    _Storage.Save(steamid);
  }

}