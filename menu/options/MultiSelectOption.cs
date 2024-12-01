using CounterStrikeSharp.API.Core;
namespace SkyboxChanger;

public class MultiSelectOption : MenuOption
{
  public Action<CCSPlayerController, MultiSelectOption, WasdMyMenu> Select { get; set; } = (_, _, _) => { };

  public Action<CCSPlayerController, MultiSelectOption, WasdMyMenu> RerenderAction { get; set; } = (_, _, _) => { };

  public bool IsSelected = false;

  public override void Next(CCSPlayerController player, WasdMyMenu menu)
  {
    Select(player, this, menu);
    IsSelected = !IsSelected;
  }
  public override bool Prev(CCSPlayerController player, WasdMyMenu menu)
  {
    return false;
  }

  public override void Rerender(CCSPlayerController player, WasdMyMenu menu)
  {
    RerenderAction(player, this, menu);
  }
}
