using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
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

  private static SkyboxChanger? _Instance { get; set; }

  public override void Load(bool hotReload)
  {
    _Instance = this;
    RegisterListener<Listeners.OnServerPrecacheResources>(OnServerPrecacheResources);
    InitializeMenuSystem(hotReload);
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
  [RequiresPermissions("@skybox/change")]
  public unsafe void SkyboxCommand(CCSPlayerController player, CommandInfo info)
  {
    WasdMyMenu myMenu = new WasdMyMenu { Title = Localizer["menu.title"] };
    Config.Skyboxs.Values.ToList().ForEach(skybox =>
    {
      myMenu.AddOption(new SelectOption
      {
        Text = skybox.Name,
        Select = (player, option, menu) =>
        {
          var result = Helper.ChangeSkybox(skybox.Material);
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
    MenuManager.OpenMainMenu(player, myMenu);
  }
}
