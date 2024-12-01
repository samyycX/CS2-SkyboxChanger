using System.Text;
using CounterStrikeSharp.API.Core;

namespace SkyboxChanger;

public abstract class MenuOption
{
  public string Text { get; set; } = "";

  public abstract void Next(CCSPlayerController player, WasdMyMenu menu);
  public abstract bool Prev(CCSPlayerController player, WasdMyMenu menu);

  public virtual void Rerender(CCSPlayerController player, WasdMyMenu menu) { }
}

public class WasdMyMenu
{
  const int MAX_OPTIONS = 4;

  public string Title { get; set; } = "";
  public List<MenuOption> Options { get; set; } = new();

  public int StartOffset = 0;

  public int SelectedOption = 0;

  public void AddOption(MenuOption option)
  {
    Options.Add(option);
  }

  public void ScrollDown()
  {
    if (Options.Count == 0)
    {
      return;
    }
    SelectedOption = (SelectedOption + 1) % Options.Count;

    if (SelectedOption < StartOffset)
    {
      StartOffset = SelectedOption;
    }

    if (SelectedOption >= StartOffset + MAX_OPTIONS)
    {
      StartOffset = SelectedOption - MAX_OPTIONS + 1;
    }

  }

  public void ScrollUp()
  {
    if (Options.Count == 0)
    {
      return;
    }
    SelectedOption = (SelectedOption - 1 + Options.Count) % Options.Count;

    if (SelectedOption < StartOffset)
    {
      StartOffset = SelectedOption;
    }

    if (SelectedOption >= StartOffset + MAX_OPTIONS)
    {
      StartOffset = SelectedOption - MAX_OPTIONS + 1;
    }
  }

  public void ToTop()
  {
    SelectedOption = 0;
    StartOffset = 0;
  }

  public void ToSelected()
  {
    SelectedOption = Options.FindIndex(option => option is SelectOption && ((SelectOption)option).IsSelected);
    if (SelectedOption == -1)
    {
      return;
    }
    if (SelectedOption < StartOffset)
    {
      StartOffset = SelectedOption;
    }

    if (SelectedOption >= StartOffset + MAX_OPTIONS)
    {
      StartOffset = SelectedOption - MAX_OPTIONS + 1;
    }
  }

  public void Next(CCSPlayerController player)
  {
    if (Options.Count == 0)
    {
      return;
    }
    Options[SelectedOption].Next(player, this);
  }

  public bool Prev(CCSPlayerController player)
  {
    if (Options.Count == 0)
    {
      return false;
    }
    return Options[SelectedOption].Prev(player, this);
  }

  public void Rerender(CCSPlayerController player)
  {
    Options.ForEach(option => option.Rerender(player, this));
  }

  public string Render()
  {
    StringBuilder builder = new StringBuilder();
    builder.AppendLine($"<font color='#3b62d9'>{Title}</u></font color='white'>");
    builder.AppendLine("<br>");
    for (int i = StartOffset; i < StartOffset + MAX_OPTIONS; i++)
    {
      if (i >= Options.Count)
      {
        builder.AppendLine("<br>");
        continue;
      }
      var text = Options[i].Text;
      if (Options[i] is NumberOption)
      {
        var value = ((NumberOption)Options[i]).Value;

        text = text.Replace("@value", value.ToString("F1"));

      }

      if (i == SelectedOption)
      {
        builder.AppendLine($"<font color='#ccacfc'>⋆ {text}</font> <br>");
        continue;
      }
      if (Options[i] is SelectOption && ((SelectOption)Options[i]).IsSelected)
      {
        builder.AppendLine($"<font color='#7219f7'>⋆ {text}</font> <br>");
        continue;
      }
      if (Options[i] is MultiSelectOption && ((MultiSelectOption)Options[i]).IsSelected)
      {
        builder.AppendLine($"<font color='#7219f7'>⋆ {text}</font> <br>");
        continue;
      }
      builder.AppendLine($"<font color='white'>{text}</font> <br>");

    }

    builder.AppendLine($"<font class='fontSize-s' color='#9ee1f0'>{SkyboxChanger.GetInstance().Localizer["menu.instruction"]}</font><br>");
    builder.AppendLine("</div>");
    return builder.ToString();

  }

}