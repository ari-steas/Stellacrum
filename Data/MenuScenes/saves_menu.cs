using Godot;
using Godot.Collections;
using System;

public partial class saves_menu : CanvasLayer
{
	[Signal]
	public delegate void SSwitchMenuEventHandler(int toShow);

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Visible = false;
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
		WorldLoader.LoadWorld();
		EmitSignal(SignalName.SSwitchMenu, 0);
	}
}
