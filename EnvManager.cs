using System.Drawing;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;

namespace SkyboxChanger;

public class EnvData
{
  public Skybox? Skybox { get; set; }
  public CEnvSky? Sky { get; set; }
  public CSkyCamera? SkyCamera { get; set; }
  public CSkyboxReference? SkyboxReference { get; set; }
}

public class SkyCameraData
{
  public required Vector Origin { get; set; }
  public required short Scale { get; set; }
}
public class EnvManager
{
  private string _DefaultPrefab = "";

  public int _NextSettingPlayer = -1;
  public void OnPlayerJoin(int slot)
  {
    _NextSettingPlayer = slot;
    Helper.SpawnSkyboxReference(_DefaultPrefab);
  }

  public void OnPlayerLeave(int slot)
  {
    Utilities.FindAllEntitiesByDesignerName<CEnvSky>("env_sky").ToList().ForEach(sky =>
    {
      if (sky.PrivateVScripts == slot.ToString())
      {
        sky.Remove();
      }
    });
    Utilities.FindAllEntitiesByDesignerName<CSkyCamera>("sky_camera").ToList().ForEach(sky =>
    {
      if (sky.PrivateVScripts == slot.ToString())
      {
        sky.Remove();
      }
    });
  }

  public void Clear()
  {
    _NextSettingPlayer = -1;
    _DefaultPrefab = "";
  }

  public void SetMapPrefab(string prefab)
  {
    if (_DefaultPrefab != "") return;
    _DefaultPrefab = prefab;
  }

  public bool SetSkybox(int slot, Skybox skybox)
  {
    return Helper.ChangeSkybox(slot, skybox);
  }

  public void SetBrightness(int slot, float value)
  {
    Utilities.FindAllEntitiesByDesignerName<CEnvSky>("env_sky").ToList().ForEach(sky =>
    {
      if (slot == -1 || sky.PrivateVScripts == slot.ToString())
      {
        sky.BrightnessScale = value;
        Utilities.SetStateChanged(sky, "CEnvSky", "m_flBrightnessScale");
      }
    });
  }

  public void SetTintColor(int slot, Color color)
  {
    Utilities.FindAllEntitiesByDesignerName<CEnvSky>("env_sky").ToList().ForEach(sky =>
    {
      if (slot == -1 || sky.PrivateVScripts == slot.ToString())
      {
        sky.TintColor = color;
        Utilities.SetStateChanged(sky, "CEnvSky", "m_vTintColor");
      }
    });
  }
  public void OnCheckTransmit(CCheckTransmitInfoList infoList)
  {
    var skys = Utilities.FindAllEntitiesByDesignerName<CEnvSky>("env_sky").ToList();
    var skycameras = Utilities.FindAllEntitiesByDesignerName<CSkyCamera>("sky_camera").ToList();
    var skyreferences = Utilities.FindAllEntitiesByDesignerName<CSkyboxReference>("skybox_reference").ToList();
    foreach ((CCheckTransmitInfo info, CCSPlayerController? player) in infoList)
    {
      if (player == null) continue;
      skycameras.ForEach(skyCamera =>
        {
          if (skyCamera.PrivateVScripts != null && skyCamera.PrivateVScripts != player.Slot.ToString())
          {
            info.TransmitAlways.Remove(skyCamera.Index);
            info.TransmitEntities.Remove(skyCamera.Index);
          }
        });
      skys.ForEach(sky =>
      {
        if (sky.PrivateVScripts != player.Slot.ToString())
        {
          info.TransmitAlways.Remove(sky.Index);
          info.TransmitEntities.Remove(sky.Index);
        }
      });
    }
  }
}