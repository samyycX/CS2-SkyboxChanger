using System.Drawing;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Entities;
using CounterStrikeSharp.API.Modules.Memory;
using CounterStrikeSharp.API.Modules.Memory.DynamicFunctions;

namespace SkyboxChanger;

public class Helper
{

  public static bool IsPlayerSkybox(int slot, CEnvSky sky)
  {
    return slot == -1 || sky.PrivateVScripts == "skyboxchanger_" + slot;
  }

  public static void Initialize()
  {

  }

  public static unsafe IntPtr FindMaterialByPath(string material)
  {
    if (material.EndsWith("_c"))
    {
      material = material.Substring(0, material.Length - 2);
    }
    IntPtr pIMaterialSystem2 = NativeAPI.GetValveInterface(0, "VMaterialSystem2_001");
    var FindOrCreateFromResource = VirtualFunction.Create<IntPtr, IntPtr, string, IntPtr>(pIMaterialSystem2, 14);
    IntPtr outMaterial = 0;
    IntPtr pOutMaterial = (nint)(&outMaterial);
    IntPtr materialptr3;
    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
    {
      materialptr3 = FindOrCreateFromResource.Invoke(pIMaterialSystem2, pOutMaterial, material);
    }
    else
    {
      materialptr3 = FindOrCreateFromResource.Invoke(pOutMaterial, 0, material);
    }
    if (materialptr3 == 0)
    {
      return 0;
    }
    return *(IntPtr*)materialptr3; // CMaterial*** -> CMaterial** (InfoForResourceTypeIMaterial2)
  }

  public static unsafe void SpawnSkybox(int slot, string fogTargetName, string material)
  {
    var skycameras = Utilities.FindAllEntitiesByDesignerName<CSkyCamera>("sky_camera");
    uint spawngrouphandle = 0;
    if (skycameras.Count() != 0) // has 3d skybox
    {
      var skycamera = skycameras.First();
      spawngrouphandle = *(uint*)(skycamera.Entity!.Handle + 0x34);
      MemoryManager.CreateLoadingSpawnGroupAndSpawnEntities(spawngrouphandle, true, true, KvLib.MakeKeyValue(fogTargetName, "skyboxchanger_" + slot, material));
    }
    else
    {
      var sky = Utilities.CreateEntityByName<CEnvSky>("env_sky");
      sky.PrivateVScripts = "skyboxchanger_" + slot;
      sky.DispatchSpawn();
      Server.NextFrame(() =>
      {
        ChangeSkybox(slot, null, 1f, Color.White);
      });
    }
  }

  public static unsafe bool ChangeSkybox(int slot, Skybox? skybox = null, float? brightness = null, Color? color = null)
  {
    // materialptr2 : CMaterial2** = InfoForResourceTypeIMaterial2


    var sky = Utilities.GetEntityFromIndex<CEnvSky>(SkyboxChanger.GetInstance().EnvManager.SpawnedSkyboxes[slot])!;
    if (skybox != null)
    {
      var materialptr2 = FindMaterialByPath(skybox.Material);
      if (materialptr2 == 0)
      {
        return false;
      }
      Unsafe.Write((void*)sky.SkyMaterial.Handle, materialptr2);
      Unsafe.Write((void*)sky.SkyMaterialLightingOnly.Handle, materialptr2);
      Utilities.SetStateChanged(sky, "CEnvSky", "m_hSkyMaterial");
      Utilities.SetStateChanged(sky, "CEnvSky", "m_hSkyMaterialLightingOnly");
    }

    if (color != null)
    {
      sky.TintColor = (Color)color;
    }
    sky.BrightnessScale = brightness ?? skybox?.Brightness ?? sky.BrightnessScale;
    var colorData = skybox?.Color?.Split(" ");
    if (colorData != null && colorData.Length == 4)
    {
      var r = int.Parse(colorData[0]);
      var g = int.Parse(colorData[1]);
      var b = int.Parse(colorData[2]);
      var a = int.Parse(colorData[3]);
      sky.TintColor = Color.FromArgb(a, r, g, b);
    }
    Utilities.SetStateChanged(sky, "CEnvSky", "m_vTintColor");
    Utilities.SetStateChanged(sky, "CEnvSky", "m_flBrightnessScale");
    return true;
  }

  public static bool PlayerHasPermission(CCSPlayerController player, string[]? permissions, string[]? permissionsOr)
  {

    if (permissions != null)
    {
      foreach (string perm in permissions)
      {
        if (perm.StartsWith("@"))
        {
          if (!AdminManager.PlayerHasPermissions(player, [perm]))
          {
            return false;
          }
        }
        else if (perm.StartsWith("#"))
        {
          if (!AdminManager.PlayerInGroup(player, [perm]))
          {
            return false;
          }
        }
        else
        {
          ulong steamId;
          if (!ulong.TryParse(perm, out steamId))
          {
            throw new FormatException($"Unknown SteamID64 format: {perm}");
          }
          else
          {
            if (player.SteamID != steamId)
            {
              return false;
            }
          }
        }
      }
    }

    if (permissionsOr != null)
    {
      foreach (string perm in permissionsOr)
      {
        if (perm.StartsWith("@"))
        {
          if (AdminManager.PlayerHasPermissions(player, perm))
          {
            return true;
          }
        }
        else if (perm.StartsWith("#"))
        {
          if (AdminManager.PlayerInGroup(player, perm))
          {
            return true;
          }
        }
        else
        {
          ulong steamId;
          if (!ulong.TryParse(perm, out steamId))
          {
            throw new FormatException($"Unknown SteamID64 format: {perm}");
          }
          else
          {
            if (player.SteamID == steamId)
            {
              return true;
            }
          }
        }
      }
    }

    return permissionsOr == null || permissionsOr.Length == 0;
  }
}