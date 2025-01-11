using System.Drawing;
using System.Runtime.CompilerServices;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Memory;
using CounterStrikeSharp.API.Modules.Utils;

namespace SkyboxChanger;

public class EnvManager
{
  public string DefaultMaterial { get; set; } = "";

  public string? CubemapFogPointedSkyName { get; set; } = null;

  public Dictionary<int, int> SpawnedSkyboxes = new();
  public unsafe void InitializeSkyboxForPlayer(CCSPlayerController player)
  {
    Helper.SpawnSkybox(player.Slot, CubemapFogPointedSkyName ?? "", DefaultMaterial);

    Skybox? skybox = SkyboxChanger.GetInstance().Service.GetPlayerSkybox(player);
    float brightness = SkyboxChanger.GetInstance().Service.GetPlayerBrightness(player);
    Color color = SkyboxChanger.GetInstance().Service.GetPlayerColor(player);


    // after 2 tick avoid conflict with SpawnSkybox initialization
    Server.NextFrame(() =>
    {
      Server.NextFrame(() =>
      {
        Helper.ChangeSkybox(player.Slot, skybox, brightness, color);
      });
    });
  }

  public unsafe void OnPlayerLeave(int slot)
  {
    var index = SpawnedSkyboxes[slot];
    CEnvSky sky = Utilities.GetEntityFromIndex<CEnvSky>(index)!;
    nint ptr = Helper.FindMaterialByPath("materials/notexist.vmat");
    Unsafe.Write((void*)sky.SkyMaterial.Handle, ptr);
    Unsafe.Write((void*)sky.SkyMaterialLightingOnly.Handle, ptr);
    SpawnedSkyboxes.Remove(slot);
    sky.Remove();
  }

  public void Shutdown()
  {
    DefaultMaterial = "";
    CubemapFogPointedSkyName = null;
    SkyboxChanger.GetInstance().Config.Skyboxs.Remove("");
    SpawnedSkyboxes.Clear();
  }

  public bool SetSkybox(int slot, Skybox skybox)
  {
    return Helper.ChangeSkybox(slot, skybox);
  }

  public void SetBrightness(int slot, float value)
  {
    Helper.ChangeSkybox(slot, null, value, null);
  }

  public void SetTintColor(int slot, Color color)
  {
    Helper.ChangeSkybox(slot, null, null, color);
  }
  public void OnCheckTransmit(CCheckTransmitInfoList infoList)
  {
    foreach ((CCheckTransmitInfo info, CCSPlayerController? player) in infoList)
    {
      if (player == null) continue;
      SpawnedSkyboxes.Values.ToList().ForEach(index =>
      {
        info.TransmitAlways.Remove(index);
        info.TransmitEntities.Remove(index);
      });
      if (!SpawnedSkyboxes.ContainsKey(player.Slot)) continue;
      var index = SpawnedSkyboxes[player.Slot];
      info.TransmitAlways.Add(index);
      info.TransmitEntities.Add(index);
    }
  }
}