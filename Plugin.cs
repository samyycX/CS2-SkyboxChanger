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
  public override string ModuleVersion => "1.2.2";
  public override string ModuleAuthor => "samyyc";

  public SkyboxConfig Config { get; set; } = new();

  public required MyMenuManager MenuManager { get; set; } = new();

  public required EnvManager EnvManager { get; set; } = new();

  public required Service Service { get; set; }
  private static SkyboxChanger? _Instance { get; set; }

  public MemoryFunctionVoid? SpawnPrefabEntities { get; set; }

  public override unsafe void Load(bool hotReload)
  {
    KvLib.SetDllImportResolver();
    _Instance = this;
    RegisterListener<Listeners.OnServerPrecacheResources>(OnServerPrecacheResources);
    RegisterListener<Listeners.CheckTransmit>(OnCheckTransmit);
    RegisterListener<Listeners.OnMapStart>((map) =>
    {
      var skybox = Service.GetMapDefaultSkybox(map);
      if (skybox != null)
      {
        var defaultSkybox = new Skybox
        {
          Name = Localizer["menu.defaultskybox"],
          Material = skybox.Material,
        };
        Config.Skyboxs.Add("", defaultSkybox);
      }
    });
    RegisterListener<Listeners.OnMapEnd>(() =>
    {
      EnvManager.Clear();
      Service.Save();
    });
    RegisterListener<Listeners.OnServerPreFatalShutdown>(() =>
    {
      Service.Save();
    });
    RegisterListener<Listeners.OnEntityCreated>((entity) =>
    {
      if (entity.DesignerName == "sky_camera" || entity.DesignerName == "env_sky")
      {
        if (EnvManager._NextSettingPlayer != -1)
        {
          entity.PrivateVScripts = EnvManager._NextSettingPlayer.ToString();
          if (entity.DesignerName == "env_sky")
          {
            Server.NextFrame(() =>
            {
              var player = Utilities.GetPlayerFromSlot(EnvManager._NextSettingPlayer)!;
              var skybox = Service.GetPlayerSkybox(player);
              var brightness = Service.GetPlayerBrightness(player);
              var color = Service.GetPlayerColor(player);
              if (skybox != null)
              {
                EnvManager.SetSkybox(EnvManager._NextSettingPlayer, skybox);
              }
              EnvManager.SetBrightness(EnvManager._NextSettingPlayer, brightness);
              EnvManager.SetTintColor(EnvManager._NextSettingPlayer, color);
            });
          }
        }
      }
    });
    // RegisterListener<Listeners.OnClientPutInServer>((slot) =>
    // {
    //   var player = Utilities.GetPlayerFromSlot(slot);
    //   AddTimer(2f + 0.05f * slot, () =>
    //   {
    //     Server.NextFrame(() =>
    //     {
    //       foreach (var sky in Utilities.FindAllEntitiesByDesignerName<CEnvSky>("env_sky"))
    //       {
    //         if (sky.PrivateVScripts == slot.ToString()) return;
    //       }
    //       EnvManager.OnPlayerJoin(slot);
    //     });
    //   });
    //   return;
    // });
    RegisterEventHandler<EventPlayerTeam>((@event, info) =>
    {
      // if (@event.Team == (int)CsTeam.None) return HookResult.Continue;
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
      Service.Save(Utilities.GetPlayerFromSlot(slot)?.AuthorizedSteamID?.SteamId64);
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
    RegisterListener<Listeners.OnMapEnd>(() =>
    {
      MenuManager.ClearPlayer();
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
    Service.Save();
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
    if (Config.Version == 1)
    {
      throw new Exception("Please update your config version. Changed: 'Database' field.");
    }
    Service = new Service(this, Config.Database.Host, Config.Database.Port, Config.Database.User, Config.Database.Password, Config.Database.Database, Config.Database.TablePrefix);
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
  public unsafe void SkyboxCommand(CCSPlayerController player, CommandInfo info)
  {
    if (Config.MenuPermission != "" && Config.MenuPermission != null && !AdminManager.PlayerHasPermissions(player, [Config.MenuPermission]))
    {
      return;
    }
    WasdMyMenu personalMenu = new WasdMyMenu { Title = Localizer["menu.title"] };
    WasdMyMenu personalSkyboxSubmenu = new WasdMyMenu { Title = Localizer["menu.title"] };
    personalMenu.AddOption(new SubMenuOption { Text = Localizer["menu.title"], NextMenu = personalSkyboxSubmenu });
    var skyboxes = Config.Skyboxs.ToList();
    skyboxes.RemoveAll(kv => kv.Key == "");
    if (Config.Skyboxs.ContainsKey(""))
    {
      var def = Config.Skyboxs[""];
      skyboxes.Insert(0, new KeyValuePair<string, Skybox>("", def));
    }
    skyboxes.ForEach(skybox =>
    {
      if (!Helper.PlayerHasPermission(player, skybox.Value.Permissions, skybox.Value.PermissionsOr)) return;
      personalSkyboxSubmenu.AddOption(new SelectOption
      {
        Text = skybox.Value.Name,
        Select = (player, option, menu) =>
        {
          var result = Service.SetSkybox(player, skybox.Key);
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
    foreach (var knownColor in (KnownColor[])Enum.GetValues(typeof(KnownColor)))
    {
      if (Color.FromKnownColor(knownColor).IsSystemColor) continue;
      personalColorMenu.AddOption(new SelectOption
      {
        Text = knownColor.ToString(),
        Select = (player, option, menu) =>
        {
          Service.SetTintColor(player, Color.FromKnownColor(knownColor));
        }
      });
    };



    SubMenuOption personalColor = new SubMenuOption { Text = Localizer["menu.tintcolor"], NextMenu = personalColorMenu };
    personalMenu.AddOption(personalColor);

    NumberOption personalBrightnessOption = new NumberOption
    {
      Text = $"- {Localizer["menu.brightness"]} @value +",
      Value = Service.GetPlayerBrightness(player),
      OnUpdate = (player, option, menu, value) =>
      {
        Service.SetBrightness(player, value);
      }
    };
    personalMenu.AddOption(personalBrightnessOption);
    MenuManager.OpenMainMenu(player, personalMenu);
  }
}