using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Entities;
using CounterStrikeSharp.API.Modules.Memory;
using CounterStrikeSharp.API.Modules.Memory.DynamicFunctions;

namespace SkyboxChanger;

public class Helper
{


  public static MemoryFunctionVoid<nint, uint, nint, nint, nint>? UnknownSpawnPrefabEntities;

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
  [DllImport("mytest.dll")]
  public static extern nint GetKeyValue();

  public static void SpawnSkyboxReference()
  {
    if (UnknownSpawnPrefabEntities == null)
    {
      var server = Path.Join(Server.GameDirectory, Constants.GameBinaryPath, Constants.ModulePrefix + "server" + Constants.ModuleSuffix);
      UnknownSpawnPrefabEntities = new("48 8B C4 48 89 58 08 48 89 70 10 48 89 78 18 55 41 54 41 55 41 56 41 57 48 8D A8 98 FD FF FF", server);
    }
    IntPtr ptr = Marshal.AllocHGlobal(0x30);
    for (int i = 0; i < 0x30; i++)
    {
      Marshal.WriteByte(ptr, i, 0);
    }
    CNetworkOriginCellCoordQuantizedVector vec = new(ptr);
    // vec.CellX = SkyboxChanger.GetInstance().origin.CellX;
    // vec.CellY = SkyboxChanger.GetInstance().origin.CellY;
    // vec.CellZ = SkyboxChanger.GetInstance().origin.CellZ;
    // vec.X = SkyboxChanger.GetInstance().origin.X;
    // vec.Y = SkyboxChanger.GetInstance().origin.Y;
    // vec.Z = SkyboxChanger.GetInstance().origin.Z;
    UnknownSpawnPrefabEntities.Invoke(0, 0, 0, ptr, GetKeyValue());
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
      if (sky.PrivateVScripts == slot.ToString())
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