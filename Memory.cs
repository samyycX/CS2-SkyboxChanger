using System.Reflection.Metadata.Ecma335;
using System.Runtime.InteropServices;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Memory;
using CounterStrikeSharp.API.Modules.Memory.DynamicFunctions;
using SkyboxChanger;

class MemoryManager
{
  private static IntPtr pSpawnGroupMgrGameSystemReallocatingFactory = 0;

  private static T PlatformExecute<T>(Func<T> OnWindows, Func<T> OnLinux)
  {
    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
    {
      return OnWindows();
    }
    else
    {
      return OnLinux();
    }
  }

  private static void PlatformExecute(Action? OnWindows = null, Action? OnLinux = null)
  {
    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
    {
      if (OnWindows != null) OnWindows();
    }
    else
    {
      if (OnLinux != null) OnLinux();
    }
  }

  public static void Load()
  {

  }

  public static void Unload()
  {
  }

  public static void CreateLoadingSpawnGroupAndSpawnEntities(uint spawnGroupHandle, bool bSynchronouslySpawnEntities, bool bConfirmResourcesLoaded, IntPtr pEntityKeyValues)
  {
    var pSpawnGroupMgrGameSystem = FindSpawnGroupMgrGameSystem();
    var CreateLoadingSpawnGroup = VirtualFunction.Create<nint, uint, byte, byte, nint, nint>(pSpawnGroupMgrGameSystem, 3);

    var pLoadingSpawnGroup = CreateLoadingSpawnGroup.Invoke(
        pSpawnGroupMgrGameSystem,
        spawnGroupHandle,
        Convert.ToByte(bSynchronouslySpawnEntities),
        Convert.ToByte(bConfirmResourcesLoaded),
        pEntityKeyValues);

    var SpawnEntities = VirtualFunction.CreateVoid<nint>(pLoadingSpawnGroup, 10);
    SpawnEntities.Invoke(pLoadingSpawnGroup);
  }

  public static void RemoveCachedFactory()
  {
    pSpawnGroupMgrGameSystemReallocatingFactory = 0;
  }

  private static unsafe IntPtr FindSpawnGroupMgrGameSystem()
  {
    if (pSpawnGroupMgrGameSystemReallocatingFactory != 0)
    {
      return **(nint**)(pSpawnGroupMgrGameSystemReallocatingFactory + 3 * 8);
    }

    var pFirstSig = GameData.GetSignature("IGameSystem_InitAllSystems_pFirst");
    var server = Path.Join(Server.GameDirectory, Constants.GameBinaryPath, Constants.ModulePrefix + "server" + Constants.ModuleSuffix);
    var pFirstOpcodeAddr = NativeAPI.FindSignature(server, pFirstSig) + 3;
    var offset = *(uint*)pFirstOpcodeAddr;
    // CBaseGameSystemFactory**
    var pFirst = pFirstOpcodeAddr + 4 + offset;
    // CBaseGameSystemFactory*
    var pFactoryList = *(nint*)pFirst;
    while (pFactoryList != 0 && pFactoryList != 0xFFFFFFFF && pFactoryList != -1L)
    {
      var pName = *(nint*)(pFactoryList + 0x10);
      string? name = Marshal.PtrToStringAnsi(pName);
      if (name == "SpawnGroupManagerGameSystem")
      {
        pSpawnGroupMgrGameSystemReallocatingFactory = pFactoryList;
        return **(nint**)(pSpawnGroupMgrGameSystemReallocatingFactory + 3 * 8);
      }
      var pNext = *(nint*)(pFactoryList + 0x8);
      pFactoryList = pNext;
    }

    throw new Exception("FAILED TO FIND SpawnGroupManagerGameSystem. The game ma  y have been updated, please report it to the author.");
  }


}