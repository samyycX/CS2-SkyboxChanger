using System.Drawing;
using System.Runtime.InteropServices;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Memory.DynamicFunctions;
using CounterStrikeSharp.API.Modules.Utils;

namespace SkyboxChanger;

public class SkyboxChanger : BasePlugin, IPluginConfig<SkyboxConfig>
{
  public override string ModuleName => "Skybox Changer";
  public override string ModuleVersion => "1.1.0";
  public override string ModuleAuthor => "samyyc";

  public SkyboxConfig Config { get; set; } = new();

  public required MyMenuManager MenuManager { get; set; } = new();

  public required EnvManager EnvManager { get; set; } = new();

  private static SkyboxChanger? _Instance { get; set; }

  public MemoryFunctionVoid? SpawnPrefabEntities { get; set; }

  public override unsafe void Load(bool hotReload)
  {
    KvLib.SetDllImportResolver();
    _Instance = this;
    RegisterListener<Listeners.OnServerPrecacheResources>(OnServerPrecacheResources);
    RegisterListener<Listeners.CheckTransmit>(OnCheckTransmit);
    var server = Path.Join(Server.GameDirectory, Constants.GameBinaryPath, Constants.ModulePrefix + "server" + Constants.ModuleSuffix);
    RegisterListener<Listeners.OnMapEnd>(EnvManager.Clear);
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
    RegisterEventHandler<EventPlayerSpawned>((@event, info) =>
    {
      if (@event.Userid == null) return HookResult.Continue;
      if (@event.Userid.IsBot || @event.Userid.IsHLTV) return HookResult.Continue;
      foreach (var sky in Utilities.FindAllEntitiesByDesignerName<CEnvSky>("env_sky"))
      {
        if (sky.PrivateVScripts == @event.Userid.Slot.ToString()) return HookResult.Continue;
      }
      EnvManager.OnPlayerJoin(@event.Userid.Slot);
      return HookResult.Continue;
    });
    RegisterListener<Listeners.OnClientDisconnect>(slot =>
    {
      EnvManager.OnPlayerLeave(slot);
    });
    InitializeMenuSystem(hotReload);
    Helper.Initialize();
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
    WasdMyMenu globalSkyboxSubmenu = new WasdMyMenu { Title = Localizer["menu.title"] };
    WasdMyMenu personalSkyboxSubmenu = new WasdMyMenu { Title = Localizer["menu.title"] };
    globalMenu.AddOption(new SubMenuOption { Text = Localizer["menu.title"], NextMenu = globalSkyboxSubmenu });
    personalMenu.AddOption(new SubMenuOption { Text = Localizer["menu.title"], NextMenu = personalSkyboxSubmenu });
    Config.Skyboxs.Values.ToList().ForEach(skybox =>
    {
      globalSkyboxSubmenu.AddOption(new SelectOption
      {
        Text = skybox.Name,
        Select = (player, option, menu) =>
        {
          var result = EnvManager.SetSkybox(-1, skybox);
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
      personalSkyboxSubmenu.AddOption(new SelectOption
      {
        Text = skybox.Name,
        Select = (player, option, menu) =>
        {
          var result = EnvManager.SetSkybox(player.Slot, skybox);
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

    WasdMyMenu personalColorMenu = new WasdMyMenu { Title = Localizer["menu.tintcolor"] };
    WasdMyMenu globalColorMenu = new WasdMyMenu { Title = Localizer["menu.tintcolor"] };

    foreach (var knownColor in (KnownColor[])Enum.GetValues(typeof(KnownColor)))
    {
      if (Convert.ToInt32(knownColor) <= 26) continue;
      personalColorMenu.AddOption(new SelectOption
      {
        Text = knownColor.ToString(),
        Select = (player, option, menu) =>
        {
          EnvManager.SetTintColor(player.Slot, Color.FromKnownColor(knownColor));
        }
      });
      globalColorMenu.AddOption(new SelectOption
      {
        Text = knownColor.ToString(),
        Select = (player, option, menu) =>
        {
          EnvManager.SetTintColor(-1, Color.FromKnownColor(knownColor));
        }
      });
    };



    SubMenuOption personalColor = new SubMenuOption { Text = Localizer["menu.tintcolor"], NextMenu = personalColorMenu };
    SubMenuOption globalColor = new SubMenuOption { Text = Localizer["menu.tintcolor"], NextMenu = globalColorMenu };
    personalMenu.AddOption(personalColor);
    globalMenu.AddOption(globalColor);

    NumberOption personalBrightnessOption = new NumberOption
    {
      Text = $"- {Localizer["menu.brightness"]} @value +",
      Value = 1,
      OnUpdate = (player, option, menu, value) =>
      {
        EnvManager.SetBrightness(player.Slot, value);
      }
    };

    NumberOption globalBrightnessOption = new NumberOption
    {
      Text = $"- {Localizer["menu.brightness"]} @value +",
      Value = 1,
      OnUpdate = (player, option, menu, value) =>
      {
        EnvManager.SetBrightness(-1, value);
      }
    };
    personalMenu.AddOption(personalBrightnessOption);
    globalMenu.AddOption(globalBrightnessOption);



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
  [DllImport("libc.so.6", EntryPoint = "toupper")]
  private static extern int CharUpper(int c);
  [DllImport("kvlib.so")]
  public static extern void NativeInitialize(nint pGameEntitySystem);
  [ConsoleCommand("css_test")]
  public unsafe void TestCommand(CCSPlayerController player, CommandInfo info)
  {
    var textToChange = "Hello Internet of Things!";
    var inputCharacterArray = textToChange.ToCharArray();

    // array of chars to hold the capitalised text
    var outputCharacterArray = new char[inputCharacterArray.Length];

    for (int i = 0; i < inputCharacterArray.Length; i++)
    {
      var charToByte = (byte)inputCharacterArray[i];
      outputCharacterArray[i] = (char)CharUpper(charToByte);
    }

    Console.WriteLine($"Original text is {textToChange}");
    Console.WriteLine($"Changed text is {new string(outputCharacterArray)}");
    NativeInitialize(0);
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