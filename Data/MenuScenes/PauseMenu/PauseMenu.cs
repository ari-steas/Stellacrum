using Godot;
using System;

public partial class PauseMenu : CanvasLayer
{
	[Signal]
	public delegate void SwitchMenuEventHandler(int toShow);

	Button MenuButton, OptionsButton, ReturnButton, SaveButton, QuitDesktopButton;
	GameScene gameScene;
	Label InfoOutputLabel;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		MenuButton = FindChild("MenuButton") as Button;
		OptionsButton = FindChild("OptionsButton") as Button;
		ReturnButton = FindChild("ReturnButton") as Button;
		SaveButton = FindChild("SaveButton") as Button;
        QuitDesktopButton = FindChild("QuitDesktopButton") as Button;
		InfoOutputLabel = FindChild("InfoOutputLabel") as Label;
		gameScene = GetParent().FindChild("GameScene") as GameScene;

		MenuButton.Pressed += MenuPress;
		OptionsButton.Pressed += OptionsPress;
		ReturnButton.Pressed += ReturnPress;
		SaveButton.Pressed += SavePress;
        QuitDesktopButton.Pressed += QuitDesktopPress;
	}

	public override void _PhysicsProcess(double delta)
	{
		if (!Visible)
			return;

		if(Input.IsActionPressed("Pause"))
		{
			Input.ActionRelease("Pause");
			EmitSignal(SignalName.SwitchMenu, 0);
		}

		if (_nextDeleteTime != 0 && DateTime.Now.Ticks > _nextDeleteTime)
		{
			InfoOutputLabel.Text = InfoOutputLabel.Text.Substring(InfoOutputLabel.Text.IndexOf('\n') + 1);

			if (InfoOutputLabel.Text.Length > 0)
				_nextDeleteTime = DateTime.Now.Ticks + 4_000_000;
			else
				_nextDeleteTime = 0;
		}
	}

    private void MenuPress()
	{
		EmitSignal(SignalName.SwitchMenu, 1);
		gameScene.Visible = false;
		gameScene.Close();
	}

	long _nextDeleteTime = 0;

    private void SavePress()
	{
		gameScene.Save();
		InfoOutputLabel.Text += "Saved world to " + WorldLoader.CurrentSave.Path + "\n";

		// Wait 4 seconds to remove info
		if (_nextDeleteTime == 0)
			_nextDeleteTime = DateTime.Now.Ticks + 4_000_000;
	}

    private void OptionsPress()
	{
		EmitSignal(SignalName.SwitchMenu, 4);
	}

    private void ReturnPress()
	{
		EmitSignal(SignalName.SwitchMenu, 0);
	}

    private void QuitDesktopPress()
    {
        GetTree().Quit();
    }
}
