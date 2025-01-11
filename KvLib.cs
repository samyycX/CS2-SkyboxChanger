using System.Reflection;
using System.Runtime.InteropServices;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using Microsoft.Extensions.Logging;

namespace SkyboxChanger;

public class KvLib
{

  [DllImport("kvlib", CallingConvention = CallingConvention.Cdecl)]
  public static extern void NativeInitialize(nint pGameEntitySystem);

  [DllImport("kvlib", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Auto)]
  public static extern nint NativeMakeKeyValue([MarshalAs(UnmanagedType.LPUTF8Str)] string fogTargetName, [MarshalAs(UnmanagedType.LPUTF8Str)] string vscripts, [MarshalAs(UnmanagedType.LPUTF8Str)] string material);

  public static bool Initialized { get; set; } = false;

  private static IntPtr DllImportResolver(string libraryName, Assembly assembly, DllImportSearchPath? searchPath)
  {
    if (libraryName == "kvlib")
    {
      return NativeLibrary.Load(RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "kvlib.dll" : "kvlib.so", assembly, searchPath);
    }

    return IntPtr.Zero;
  }

  public static void SetDllImportResolver()
  {
    NativeLibrary.SetDllImportResolver(Assembly.GetExecutingAssembly(), DllImportResolver);
  }

  public static unsafe void Initialize()
  {
    int entitySystemOffset = GameData.GetOffset("GameEntitySystem");
    var pGameResourceServiceServer = NativeAPI.GetValveInterface(0, "GameResourceServiceServerV001");
    var pGameEntitySystem = *(IntPtr*)(pGameResourceServiceServer + entitySystemOffset);
    var server = Path.Join(Server.GameDirectory, Constants.GameBinaryPath, Constants.ModulePrefix + "server" + Constants.ModuleSuffix);
    NativeInitialize(pGameEntitySystem);
    Initialized = true;
  }

  public static nint MakeKeyValue(string fogTargetName, string vscripts, string material)
  {
    if (!Initialized)
    {
      Initialize();
    }
    // return 0;
    return NativeMakeKeyValue(fogTargetName, vscripts, material);
  }



}