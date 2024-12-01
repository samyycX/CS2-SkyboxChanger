using CounterStrikeSharp.API.Core;
namespace SkyboxChanger;

public class NumberOption : MenuOption
{
  public float Value { get; set; } = 0f;

  public float Interval { get; set; } = 0.1f;
  public float MaxValue { get; set; } = float.MaxValue;
  public float MinValue { get; set; } = 0;
  public Action<CCSPlayerController, NumberOption, WasdMyMenu, float> OnUpdate { get; set; } = (_, _, _, _) => { };

  public override void Next(CCSPlayerController player, WasdMyMenu menu)
  {
    if (Value + Interval < MaxValue)
    {
      Value += Interval;
      OnUpdate(player, this, menu, Value);
    }
  }

  public override bool Prev(CCSPlayerController player, WasdMyMenu menu)
  {
    if (Value - Interval > MinValue)
    {
      Value -= Interval;
      OnUpdate(player, this, menu, Value);
    }
    return true;
  }

  public override void Rerender(CCSPlayerController player, WasdMyMenu menu)
  {

  }
}