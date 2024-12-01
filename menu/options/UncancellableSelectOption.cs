using CounterStrikeSharp.API.Core;
namespace SkyboxChanger;

public class UncancellableSelectOption : SelectOption
{
  public override void Next(CCSPlayerController player, WasdMyMenu menu)
  {
    Select(player, this, menu);
    IsSelected = true;
    menu.Options.ForEach(option =>
    {
      if (option != this && option is UncancellableSelectOption)
      {
        ((UncancellableSelectOption)option).IsSelected = false;
      }
    });
  }

  public new UncancellableSelectOption SetAdditionalProperty(string key, dynamic value)
  {
    AdditionalProperties[key] = value;
    return this;
  }
}
