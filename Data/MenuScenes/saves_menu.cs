using Godot;
using Godot.Collections;
using System;

public partial class saves_menu : CanvasLayer
{
	[Signal]
	public delegate void SSwitchMenuEventHandler(int toShow);

	ItemList SavesList;
	SaveInfoContainer SaveContaner;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Visible = false;

		(FindChild("DeleteWorldButton") as Button).Pressed += DeletePress;
		(FindChild("LoadWorldButton") as Button).Pressed += LoadPress;
		SavesList = FindChild("SavesList") as ItemList;
		SaveContaner = FindChild("SaveInfoContainer") as SaveInfoContainer;

		SaveContaner._nameLabel.TextChanged += UpdateWorldName;
	}

    void UpdateWorldName(string newText)
	{
		WorldLoader.CurrentSave.SetName(newText);
		GD.Print(newText);
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
		EmitSignal(SignalName.SSwitchMenu, menu);
	}

	private void _StartGame()
	{
		WorldLoader.LoadWorld(GetParent().FindChild("GameScene") as GameScene);
		EmitSignal(SignalName.SSwitchMenu, 0);
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
