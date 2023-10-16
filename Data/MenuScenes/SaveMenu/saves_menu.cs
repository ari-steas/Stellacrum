using Godot;
using Godot.Collections;
using System;

public partial class saves_menu : CanvasLayer
{
	[Signal]
	public delegate void SSwitchMenuEventHandler(int toShow);

	ItemList SavesList;
	SaveInfoContainer SaveContainer;
	menus menus;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Visible = false;

		menus = GetParent<menus>();

		(FindChild("DeleteWorldButton") as Button).Pressed += DeletePress;
		(FindChild("LoadWorldButton") as Button).Pressed += LoadPress;
		SavesList = FindChild("SavesList") as ItemList;
		SaveContainer = FindChild("SaveInfoContainer") as SaveInfoContainer;

		SaveContainer._nameLabel.FocusExited += UpdateWorldName;
		SaveContainer._descriptionLabel.FocusExited += UpdateWorldDescription;
	}

    void UpdateWorldName()
	{
        WorldLoader.CurrentSave.SetName(SaveContainer._nameLabel.Text);
		SavesList.Update();
	}

    void UpdateWorldDescription()
    {
        WorldLoader.CurrentSave.SetDescription(SaveContainer._descriptionLabel.Text);
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
	{
		
	}

	private void OnVisibilityChanged()
	{
		if (Visible)
			Input.MouseMode = Input.MouseModeEnum.Visible;
		GD.Print("Visible: " + Visible);
	}

	private void _GoToMenu(int menu)
	{
		menus._SwitchMenu(menu);
	}

	private void _StartGame()
	{
        menus._SwitchMenu(6);
        WorldLoader.LoadWorld(menus.FindChild("GameScene") as GameScene);
    }

    private void DeletePress()
	{
		WorldLoader.Delete(WorldLoader.CurrentSave);
		SavesList.Update();
	}

	private void LoadPress()
	{
		SavesList.Update();
	}
}
