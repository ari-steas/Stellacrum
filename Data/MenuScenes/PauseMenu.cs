using Godot;
using System;

public partial class PauseMenu : CanvasLayer
{
	[Signal]
	public delegate void SwitchMenuEventHandler(int toShow);

	Button MenuButton, OptionsButton, ReturnButton, SaveButton;
	GameScene gameScene;
	Label InfoOutputLabel;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		MenuButton = FindChild("MenuButton") as Button;
		OptionsButton = FindChild("OptionsButton") as Button;
		ReturnButton = FindChild("ReturnButton") as Button;
		SaveButton = FindChild("SaveButton") as Button;
		InfoOutputLabel = FindChild("InfoOutputLabel") as Label;
		gameScene = GetParent().FindChild("GameScene") as GameScene;

		MenuButton.Pressed += MenuPress;
		OptionsButton.Pressed += OptionsPress;
		ReturnButton.Pressed += ReturnPress;
		SaveButton.Pressed += SavePress;
	}

	public override void _Process(double delta)
	{
		if (Visible)
		{
			if(Input.IsActionPressed("Pause"))
			{
				Input.ActionRelease("Pause");
				EmitSignal(SignalName.SwitchMenu, 0);
			}

			if (nextDeleteTime != 0 && DateTime.Now.Ticks > nextDeleteTime)
			{
				InfoOutputLabel.Text = InfoOutputLabel.Text.Substring(InfoOutputLabel.Text.IndexOf('\n') + 1);

				if (InfoOutputLabel.Text.Length > 0)
					nextDeleteTime = DateTime.Now.Ticks + 4_000_000;
				else
					nextDeleteTime = 0;
			}
		}
	}

	void MenuPress()
	{
		EmitSignal(SignalName.SwitchMenu, 1);
		gameScene.Visible = false;
		gameScene.Close();
	}

	long nextDeleteTime = 0;
	void SavePress()
	{
		gameScene.Save();
		InfoOutputLabel.Text += "Saved world to " + WorldLoader.CurrentSave.Path + "\n";

		// Wait 4 seconds to remove info
		if (nextDeleteTime == 0)
			nextDeleteTime = DateTime.Now.Ticks + 4_000_000;
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
