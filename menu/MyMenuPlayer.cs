using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;

namespace SkyboxChanger;

public class MyMenuPlayer
{
  public required CCSPlayerController Player { get; set; }
  public Stack<WasdMyMenu> Menus { get; set; } = new();

  public PlayerButtons Buttons { get; set; }

  public string CenterHtml { get; set; } = "";

  public void OpenMainMenu(WasdMyMenu menu)
  {
    Menus.Clear();
    Menus.Push(menu);
    Render();
  }

  public void CloseMenu()
  {
    Menus.Clear();
  }

  public void OpenSubMenu(WasdMyMenu menu)
  {
    Menus.Push(menu);
    Render();
  }

  public void CloseSubMenu()
  {
    Menus.Pop();
    Render();
  }

  public void ScrollUp()
  {
    Menus.Peek().ScrollUp();
    Render();
  }

  public void ScrollDown()
  {
    Menus.Peek().ScrollDown();
    Render();
  }

  public void Next()
  {
    Menus.Peek().Next(Player);
    if (HasMenu()) Render();
    else CenterHtml = "";
  }

  public void Prev()
  {
    if (Menus.Count > 1)
    {
      CloseSubMenu();
    }
  }
  public void ToTop()
  {
    Menus.Peek().ToTop();
    Render();
  }
  public void ToSelected()
  {
    Menus.Peek().ToSelected();
    Render();
  }

  public bool HasMenu()
  {
    return Menus.Count > 0;
  }

  public void Render()
  {
    CenterHtml = Menus.Peek().Render();
  }

  public void Rerender()
  {
    Menus.ElementAt(Menus.Count - 1).Rerender(Player); // the root menu should contains all the path to submenus and eventually update them all
  }

}