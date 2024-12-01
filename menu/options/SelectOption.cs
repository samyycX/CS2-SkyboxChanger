using CounterStrikeSharp.API.Core;
namespace SkyboxChanger;

public class SelectOption : MenuOption
{
  public Action<CCSPlayerController, SelectOption, WasdMyMenu> Select { get; set; } = (_, _, _) => { };

  public Action<CCSPlayerController, SelectOption, WasdMyMenu> RerenderAction { get; set; } = (_, _, _) => { };

  public Dictionary<string, dynamic> AdditionalProperties = new();
  public bool IsSelected = false;

  public override void Next(CCSPlayerController player, WasdMyMenu menu)
  {
    Select(player, this, menu);
    IsSelected = !IsSelected;
    if (IsSelected)
    {
      menu.Options.ForEach(option =>
      {
        if (option != this && option is SelectOption)
        {
          ((SelectOption)option).IsSelected = false;
        }
      });
    }
  }

  public override bool Prev(CCSPlayerController player, WasdMyMenu menu)
  {
    return false;
  }

  public override void Rerender(CCSPlayerController player, WasdMyMenu menu)
  {
    RerenderAction(player, this, menu);
  }

  public SelectOption SetAdditionalProperty(string key, dynamic value)
  {
    AdditionalProperties[key] = value;
    return this;
  }


}
