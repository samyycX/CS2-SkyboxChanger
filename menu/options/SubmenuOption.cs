using CounterStrikeSharp.API.Core;
namespace SkyboxChanger;

public class SubMenuOption : MenuOption
{
  public required WasdMyMenu NextMenu { get; set; }
  public override void Next(CCSPlayerController player, WasdMyMenu menu)
  {
    SkyboxChanger.GetInstance().MenuManager.OpenSubMenu(player, NextMenu);
  }

  public override bool Prev(CCSPlayerController player, WasdMyMenu menu)
  {
    return false;
  }

  public override void Rerender(CCSPlayerController player, WasdMyMenu menu)
  {
    NextMenu.Rerender(player);
  }
}