using Godot;
using Godot.Collections;
using System;

public partial class MenuButtonPivot : Node2D
{
	[Signal]
	public delegate void FullscreenEventHandler();
	[Signal]
	public delegate void SavesMenuEventHandler();
	[Signal]
	public delegate void OptionsMenuEventHandler();

	private int selection = 1;
	private float _desiredRotation = 0;
	private float _maxSpeed = 0.1f;
	private Array<Label> _buttons = new Array<Label>();

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		foreach (Label l in GetChildren())
		{
			l.FocusMode = Control.FocusModeEnum.Click;
			_buttons.Add(l);
		}

		foreach (var button in _buttons)
		{
			if (button != _buttons[selection])
				button.Modulate = new Color(0xAAAAAAFF);
			else
				button.Modulate = new Color(0xFFFFFFFF);
		}

		GD.Print($"Current selection: {_buttons[selection].Name} ({selection+1} of {_buttons.Count})");
	}

	private ulong _lastMove = 0;

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (!GetParent<CanvasLayer>().Visible)
			return;
			
		if ((Input.GetLastMouseVelocity().X > 150 || Input.IsActionPressed("button_right")))
			_rotate(-Mathf.Pi/2);

		if ((Input.GetLastMouseVelocity().X < -150 || Input.IsActionPressed("button_left")))
			_rotate(Mathf.Pi/2);

		if (Input.IsActionJustPressed("ui_accept"))
			_MainScreenHandler();

			
		Rotate((float)(Math.Clamp(_desiredRotation - Rotation, -_maxSpeed, _maxSpeed)*delta)*100f);
		foreach (var button in _buttons)
			button.Rotation = -Rotation;
	}

	private void _MainScreenHandler()
	{
		switch (selection)
		{
			case 0:
				GetTree().Quit();
				break;
			case 1:
				EmitSignal(SignalName.SavesMenu);
				break;
			case 2:
				EmitSignal(SignalName.OptionsMenu);
				break;
		}
	}

	private void _rotate(float amount)
	{
		if (Time.GetTicksMsec() - _lastMove > 200)
		{
			if (amount > 0)
			{
				if (selection <= 0)
					return;
				selection--;
			}
			else if (amount < 0)
			{
				if (_buttons.Count - 1 <= selection)
					return;
				selection++;
			}

			//GD.Print($"Current selection: {_buttons[selection].Name} ({selection+1} of {_buttons.Count})");
			_desiredRotation += amount;
			foreach (var button in _buttons)
			{
				if (button != _buttons[selection])
					button.Modulate = new Color(0xAAAAAAFF);
				else
					button.Modulate = new Color(0xFFFFFFFF);
			}

			_lastMove = Time.GetTicksMsec();
		}
	}
}
