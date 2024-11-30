using System.Drawing;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Cvars;
using CounterStrikeSharp.API.Modules.Entities;
using CounterStrikeSharp.API.Modules.Events;
using CounterStrikeSharp.API.Modules.Memory;
using CounterStrikeSharp.API.Modules.Memory.DynamicFunctions;
using CounterStrikeSharp.API.Modules.Menu;
using CounterStrikeSharp.API.Modules.Utils;

namespace SkyboxChanger;

public class SkyboxChanger : BasePlugin, IPluginConfig<SkyboxConfig>
{
  public override string ModuleName => "Skybox Changer";
  public override string ModuleVersion => "1.0.1";
  public override string ModuleAuthor => "samyyc";

  public SkyboxConfig Config { get; set; } = new();

  public required MyMenuManager MenuManager { get; set; } = new();

  public required EnvManager EnvManager { get; set; } = new();

  private static SkyboxChanger? _Instance { get; set; }

  MemoryFunctionVoid<nint, uint, nint, nint, nint> a;

  public CNetworkOriginCellCoordQuantizedVector origin;

  public override unsafe void Load(bool hotReload)
  {
    _Instance = this;
    RegisterListener<Listeners.OnServerPrecacheResources>(OnServerPrecacheResources);
    // RegisterListener<Listeners.CheckTransmit>(OnCheckTransmit);
    // RegisterListener<Listeners.OnClientPutInServer>(EnvManager.OnPlayerJoin);
    RegisterListener<Listeners.OnMapEnd>(EnvManager.Clear);
    RegisterListener<Listeners.OnEntitySpawned>((entity) =>
    {
      if (origin == null && entity.DesignerName == "skybox_reference")
      {
        origin = new CSkyboxReference(entity.Handle).CBodyComponent.SceneNode.Origin;
      }
      if (entity.DesignerName == "sky_camera")
      {
        if (entity.PrivateVScripts != null)
        {
          // new CSkyCamera(entity.Handle).Teleport(new Vector(0, 0, 0));
          // new CSkyCamera(entity.Handle).AcceptInput("ActivateSkybox");
          // Console.WriteLine("ACTIVATEDEDDDDDDDDDDDD");
        }
        else
        {
        }
      }
      if (entity.DesignerName == "env_sky")
      {
        if (entity.PrivateVScripts == null)
        {

          // entity.Remove();
        }
      }

    });
    // RegisterListener<Listeners.OnMapStart>((map) =>
    // {
    //   Utilities.FindAllEntitiesByDesignerName<CSkyCamera>("env_skycamera").ToList().ForEach(camera =>
    //   {
    //     camera.AcceptInput("Kill");
    //   });
    // });
    RegisterListener<Listeners.OnEntityCreated>((entity) =>
    {
      if (entity.DesignerName == "sky_camera" || entity.DesignerName == "env_sky")
      {
        if (EnvManager._NextSettingPlayer != -1)
        {
          entity.PrivateVScripts = EnvManager._NextSettingPlayer.ToString();
        }
      }
    });
    // a.Hook(Myhook, HookMode.Pre);
    // RegisterEventHandler<EventRoundStart>((@event, info) =>
    // {
    //   EnvManager.Initialize();
    //   return HookResult.Continue;
    // });
    RegisterEventHandler<EventPlayerTeam>((@event, info) =>
    {
      if (@event.Userid == null) return HookResult.Continue;
      foreach (var sky in Utilities.FindAllEntitiesByDesignerName<CEnvSky>("env_sky"))
      {
        if (sky.PrivateVScripts == @event.Userid.Slot.ToString()) return HookResult.Continue;
      }
      EnvManager.OnPlayerJoin(@event.Userid.Slot);
      return HookResult.Continue;
    });
    InitializeMenuSystem(hotReload);
  }

  static nint aa;
  static uint b;
  static nint c;
  static nint d;

  static bool control = true;


  [DllImport("mytest.dll")]
  public static extern nint ExecKeyValue(nint p);

  public unsafe HookResult Myhook(DynamicHook hook)
  {
    Console.WriteLine("A: " + hook.GetParam<nint>(0));
    aa = hook.GetParam<nint>(0);
    Console.WriteLine("B: " + hook.GetParam<uint>(1));
    b = hook.GetParam<uint>(1);
    Console.WriteLine("C: " + hook.GetParam<nint>(2));
    c = hook.GetParam<nint>(2);
    Console.WriteLine("D: " + hook.GetParam<nint>(3));
    Console.WriteLine("DDREF = " + Unsafe.Read<ulong>((void*)hook.GetParam<nint>(3)));
    d = hook.GetParam<nint>(3);
    Console.WriteLine("E: " + hook.GetParam<nint>(4));
    var vec = new CNetworkOriginCellCoordQuantizedVector(d);
    Console.WriteLine(vec.Vector);
    Console.WriteLine(vec.OutsideWorld);
    ExecKeyValue(hook.GetParam<nint>(4));
    Console.WriteLine();
    // Console.WriteLine("F: " + hook.GetParam<uint>(5));
    return HookResult.Continue;
  }

  private void OnCheckTransmit(CCheckTransmitInfoList infoList)
  {
    EnvManager.OnCheckTransmit(infoList);
  }

  private void InitializeMenuSystem(bool hotReload)
  {
    RegisterEventHandler<EventPlayerActivate>((@event, info) =>
    {
      if (@event.Userid != null)
      {
        MenuManager.AddPlayer(@event.Userid.Slot, new MyMenuPlayer { Player = @event.Userid, Buttons = 0 });
        Console.WriteLine(MenuManager.GetPlayer(0));
      }
      return HookResult.Continue;
    });
    RegisterEventHandler<EventPlayerDisconnect>((@event, info) =>
    {
      if (@event.Userid != null)
      {
        MenuManager.RemovePlayer(@event.Userid.Slot);
      }
      return HookResult.Continue;
    });
    RegisterListener<Listeners.OnTick>(() =>
    {
      MenuManager.Update();
    });
    if (hotReload)
    {
      MenuManager.ReloadPlayer();
    }
  }

  public override void Unload(bool hotReload)
  {
    // a.Unhook(Myhook, HookMode.Pre);
  }

  public static SkyboxChanger GetInstance()
  {
    if (_Instance == null)
    {
      throw new Exception("SkyboxChanger is not loaded");
    }

    return _Instance;
  }


  public void OnConfigParsed(SkyboxConfig config)
  {
    Config = config;
  }

  public void OnServerPrecacheResources(ResourceManifest manifest)
  {
    foreach (var skybox in Config.Skyboxs)
    {
      if (skybox.Value.Name == "")
      {
        skybox.Value.Name = skybox.Key;
      }
      manifest.AddResource(skybox.Value.Material);
    }
  }

  [ConsoleCommand("css_sky")]
  [ConsoleCommand("css_skybox")]
  [CommandHelper(0, "Change skybox", CommandUsage.CLIENT_ONLY)]
  [RequiresPermissionsOr("@skybox/change", "@skybox/changeall")]
  public unsafe void SkyboxCommand(CCSPlayerController player, CommandInfo info)
  {
    WasdMyMenu globalMenu = new WasdMyMenu { Title = Localizer["menu.title"] };
    WasdMyMenu personalMenu = new WasdMyMenu { Title = Localizer["menu.title"] };
    Config.Skyboxs.Values.ToList().ForEach(skybox =>
    {
      globalMenu.AddOption(new SelectOption
      {
        Text = skybox.Name,
        Select = (player, option, menu) =>
        {
          var result = EnvManager.SetGlobalSkybox(skybox);
          if (result)
          {
            player.PrintToChat(Localizer["change.success"]);
          }
          else
          {
            player.PrintToChat(Localizer["change.failed"]);
          }
          option.IsSelected = true; // reverse
        }
      });
      personalMenu.AddOption(new SelectOption
      {
        Text = skybox.Name,
        Select = (player, option, menu) =>
        {
          var result = EnvManager.SetSkybox(player, skybox);
          if (result)
          {
            player.PrintToChat(Localizer["change.success"]);
          }
          else
          {
            player.PrintToChat(Localizer["change.failed"]);
          }
          option.IsSelected = true; // reverse
        }
      });
    });
    if (AdminManager.PlayerHasPermissions(player, ["@skybox/changeall"]))
    {
      WasdMyMenu targetMenu = new WasdMyMenu { Title = Localizer["menu.title"] };
      targetMenu.AddOption(new SubMenuOption { Text = Localizer["menu.setforall"], NextMenu = globalMenu });
      targetMenu.AddOption(new SubMenuOption { Text = Localizer["menu.setforself"], NextMenu = personalMenu });
      MenuManager.OpenMainMenu(player, targetMenu);
    }
    else
    {
      MenuManager.OpenMainMenu(player, personalMenu);
    }
  }

  [DllImport("mytest.dll")]
  public static extern nint GetKeyValue();
  [DllImport("mytest.dll")]
  public static extern void Exec(nint handle, nint a, ushort b, nint c, nint d);



  [ConsoleCommand("css_test")]
  public unsafe void TestCommand(CCSPlayerController player, CommandInfo info)
  {
    Helper.SpawnSkyboxReference();

    // a.Unhook(Myhook, HookMode.Pre);
    // a.Invoke
    // control = true;
    // Exec(a.Handle, aa, (ushort)b, c, d);
    // IntPtr ptr = Marshal.AllocHGlobal(0x30);
    // for (int i = 0; i < 0x30; i++)
    // {
    //   Marshal.WriteByte(ptr, i, 0);
    // }
    // CNetworkOriginCellCoordQuantizedVector vec = new(ptr);
    // vec.OutsideWorld = 0;
    // // vec.CellX = origin.CellX;
    // // vec.CellY = origin.CellY;
    // // vec.CellZ = origin.CellZ;
    // // vec.X = origin.X;
    // // vec.Y = origin.Y;
    // // vec.Z = origin.Z;
    // a.Invoke(aa, b, c, ptr, GetKeyValue());


    // var refer = Utilities.CreateEntityByName<CSkyboxReference>("skybox_reference")!;
    // refer.DispatchSpawn();
    // var prefab = Utilities.CreateEntityByName<CPointPrefab>("point_prefab")!;
    // prefab.TargetMapName = "prefabs/de_vertigo/skybox2";
    // prefab.FixupNames = true;
    // prefab.LoadDynamic = true;
    // prefab.DispatchSpawn();
    // prefab.LoadDynamic = true;

    // prefab.DispatchSpawn();

    // Utilities.FindAllEntitiesByDesignerName<CSkyCamera>("sky_camera").ToList().ForEach(c =>
    // {
    //   c.Remove();
    // });
    // EnvManager.b = false;
    // var skyCamera = Utilities.CreateEntityByName<CSkyCamera>("sky_camera")!;
    // skyCamera.SkyboxData.Scale = 16;
    // skyCamera.PrivateVScripts = "aa";
    // skyCamera.DispatchSpawn();

    // Server.NextFrame(() =>
    // {
    //   CPointPrefab
    //   var skyboxReference = Utilities.CreateEntityByName<CSkyboxReference>("skybox_reference")!;
    //   var ptr = GetKeyValue();
    //   Console.WriteLine(ptr);
    //   VirtualFunctions.CBaseEntity_DispatchSpawn(skyboxReference.Handle, ptr);
    //   skyboxReference.SkyCamera.Raw = skyCamera.EntityHandle.Raw;
    //   Console.WriteLine(skyboxReference.SkyCamera.Raw);
    //   skyCamera.AcceptInput("ActivateSkybox");

    // });

    // skyboxReference.Teleport(player.PlayerPawn.Value.CBodyComponent.SceneNode.AbsOrigin);
    // Utilities.FindAllEntitiesByDesignerName<CSkyCamera>("sky_camera").ToList().ForEach(c =>
    // {
    //   c.Remove();
    // });

    // CSkyCamera skyCamera = Utilities.CreateEntityByName<CSkyCamera>("sky_camera")!;
    // skyCamera.DispatchSpawn();
    // skyCamera.SkyboxData.Scale = 16;
    // skyCamera.PrivateVScripts = "a";
    // skyCamera.AddEntityIOEvent("ActivateSkybox");
    // skyCamera.AcceptInput("ActivateSkybox");
  }
  [ConsoleCommand("css_t2")]
  public unsafe void Test2Command(CCSPlayerController player, CommandInfo info)
  {
    Utilities.FindAllEntitiesByDesignerName<CSkyCamera>("sky_camera").ToList().ForEach(camera =>
          {
            camera.Remove();
          });
    Utilities.FindAllEntitiesByDesignerName<CEnvSky>("env_sky").ToList().ForEach(camera =>
    {
      camera.Remove();
    });
    Utilities.FindAllEntitiesByDesignerName<CEnvCubemapFog>("env_cubemap_fog").ToList().ForEach(camera =>
          {
            camera.Remove();
          });
  }
}

[StructLayout(LayoutKind.Sequential)]
struct Unk
{
  public uint unk1;
  public uint pad1;
  public uint pad2;
  public uint pad3;
}