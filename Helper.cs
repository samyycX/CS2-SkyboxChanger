using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Entities;
using CounterStrikeSharp.API.Modules.Memory;
using CounterStrikeSharp.API.Modules.Memory.DynamicFunctions;

namespace SkyboxChanger;

class SpawnPrefabEntitiesRebroadcastParam
{
  public nint A;
  public float B;
  public nint C;
  public uint D;
  public nint E;
  public nint F;
  public nint G;

}

public class Helper
{
  public static MemoryFunctionVoid<nint, uint, nint, nint, nint>? SpawnPrefabEntities_Windows;

  // first param should be double, but ccs doesnt support that and it doesnt matter i think
  public static MemoryFunctionWithReturn<nint, float, nint, uint, nint, nint, nint, nint>? SpawnPrefabEntities_Linux;

  private static SpawnPrefabEntitiesRebroadcastParam? param;

  public static void Initialize()
  {
    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
    {
      SpawnPrefabEntities_Windows = new(GameData.GetSignature("SpawnPrefabEntities"));
      SpawnPrefabEntities_Windows.Hook(hook =>
      {
        string? prefab = KvLib.GetTargetMapName(hook.GetParam<nint>(4));
        if (prefab != null)
        {
          SkyboxChanger.GetInstance().EnvManager.SetMapPrefab(prefab);
        }
        return HookResult.Continue;
      }, HookMode.Pre);
    }
    else
    {
      SpawnPrefabEntities_Linux = new(GameData.GetSignature("SpawnPrefabEntities"));
      SpawnPrefabEntities_Linux.Hook(hook =>
      {
        // mismatch with ida
        nint ptr = hook.GetParam<nint>(5);
        string? prefab = KvLib.GetTargetMapName(ptr);
        if (prefab != null)
        {
          SkyboxChanger.GetInstance().EnvManager.SetMapPrefab(prefab);
        }
        return HookResult.Continue;
      }, HookMode.Pre);
    }
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
  public static void SpawnSkyboxReference(string prefab)
  {
    IntPtr ptr = Marshal.AllocHGlobal(0x30);
    for (int i = 0; i < 0x30; i++)
    {
      Marshal.WriteByte(ptr, i, 0);
    }
    CNetworkOriginCellCoordQuantizedVector vec = new(ptr);
    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
    {
      SpawnPrefabEntities_Windows!.Invoke(0, 0, 0, ptr, KvLib.MakeKeyValue(prefab));
    }
    else
    {
      // the param shit mismatch with ida
      SpawnPrefabEntities_Linux!.Invoke(0, 0, 0, 0, ptr, KvLib.MakeKeyValue(prefab), 0);
    }
  }

  public static unsafe bool ChangeSkybox(int slot, Skybox skybox)
  {
    // materialptr2 : CMaterial2** = InfoForResourceTypeIMaterial2
    var materialptr2 = FindMaterialByPath(skybox.Material);
    if (materialptr2 == 0)
    {
      return false;
    }
    Utilities.FindAllEntitiesByDesignerName<CEnvSky>("env_sky").ToList().ForEach(sky =>
    {
      if (slot == -1 || sky.PrivateVScripts == slot.ToString())
      {
        Unsafe.Write((void*)sky.SkyMaterial.Handle, materialptr2);
        Unsafe.Write((void*)sky.SkyMaterialLightingOnly.Handle, materialptr2);
        Utilities.SetStateChanged(sky, "CEnvSky", "m_hSkyMaterial");
        Utilities.SetStateChanged(sky, "CEnvSky", "m_hSkyMaterialLightingOnly");
      }
    });
    return true;
  }
}