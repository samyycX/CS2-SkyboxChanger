using System.Drawing;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Memory;
using CounterStrikeSharp.API.Modules.Utils;
using Microsoft.Extensions.Logging;

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
  private EnvData[]? _Skys;
  private string _DefaultSkyMaterial = "";
  private SkyCameraData? _DefaultSkyCamera;

  public int _NextSettingPlayer = -1;
  public void OnPlayerJoin(int slot)
  {
    _NextSettingPlayer = slot;
    // Helper.SpawnSkyboxReference();
  }

  public void Clear()
  {
    _DefaultSkyMaterial = "";
    _NextSettingPlayer = -1;
    _Skys = new EnvData[Server.MaxPlayers];
  }

  public bool SetSkybox(CCSPlayerController player, Skybox skybox)
  {
    return Helper.ChangeSkybox(player.Slot, skybox);
  }

  public bool SetGlobalSkybox(Skybox skybox)
  {
    // _NextChangingPlayer = -1;
    // return Helper.ChangeSkybox(skybox);
    return true;
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
          // skyCamera.PrivateVScripts != null && 
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
      skyreferences.ForEach(skyboxReference =>
      {
        info.TransmitAlways.Remove(skyboxReference.Index);
        info.TransmitEntities.Remove(skyboxReference.Index);
      });
    }
  }
}