using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Memory;
using CounterStrikeSharp.API.Modules.Memory.DynamicFunctions;

namespace SkyboxChanger;

public class Helper
{


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
  public static unsafe bool ChangeSkybox(string material)
  {
    // materialptr2 : CMaterial2** = InfoForResourceTypeIMaterial2
    var materialptr2 = FindMaterialByPath(material);
    if (materialptr2 == 0)
    {
      return false;
    }
    foreach (var sky in Utilities.FindAllEntitiesByDesignerName<CEnvSky>("env_sky"))
    {
      Unsafe.Write((void*)sky.SkyMaterial.Handle, materialptr2);
      Utilities.SetStateChanged(sky, "CEnvSky", "m_hSkyMaterial");
    }
    return true;
  }
}