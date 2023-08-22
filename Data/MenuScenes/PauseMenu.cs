using Godot;
using System;

public partial class PauseMenu : CanvasLayer
{
	[Signal]
	public delegate void SwitchMenuEventHandler(int toShow);

	Button MenuButton, OptionsButton, ReturnButton;
	GameScene gameScene;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		MenuButton = FindChild("MenuButton") as Button;
		OptionsButton = FindChild("OptionsButton") as Button;
		ReturnButton = FindChild("ReturnButton") as Button;
		gameScene = GetParent().FindChild("GameScene") as GameScene;

		MenuButton.Pressed += MenuPress;
		OptionsButton.Pressed += OptionsPress;
		ReturnButton.Pressed += ReturnPress;
	}

	public override void _Process(double delta)
	{
		if (Visible && Input.IsActionPressed("Pause"))
		{
			Input.ActionRelease("Pause");
			EmitSignal(SignalName.SwitchMenu, 0);
		}
	}

	void MenuPress()
	{
		EmitSignal(SignalName.SwitchMenu, 1);
		gameScene.Save();
		gameScene.Visible = false;
		gameScene.Close();
	}

	void OptionsPress()
	{
		//EmitSignal(SignalName.SwitchMenu, 2);
	}

	void ReturnPress()
	{
		EmitSignal(SignalName.SwitchMenu, 0);
	}
}
