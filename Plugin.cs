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
}