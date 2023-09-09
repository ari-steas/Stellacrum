using Godot;
using System;
using System.Collections.Generic;

public partial class OptionsMenu : CanvasLayer
{
	[Signal]
	public delegate void SwitchMenuEventHandler(int toShow);

	menus ParentMenu;
	Button returnButton, applyButton;
	VBoxContainer optionsContainer;

	readonly Dictionary<string, Control> controls = new();

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		ParentMenu = GetParent<menus>();
		returnButton = FindChild("ReturnButton") as Button;
		applyButton = FindChild("ApplyButton") as Button;
		optionsContainer = FindChild("OptionsContainer") as VBoxContainer;

		returnButton.Pressed += Return;
		applyButton.Pressed += Apply;

		UpdateControls();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	void Return()
	{
		EmitSignal(SignalName.SwitchMenu, ParentMenu.prevMenu);
	}

	void Apply()
	{
		UpdateSettings();
		Return();
	}

    void VisibilityChanged()
	{
		if (Visible)
			UpdateControls();
	}

	void UpdateSettings()
	{
		GD.Print("Updating settings...");
		foreach (var control in controls)
		{
			object val = null;

			switch (control.Value)
			{
				case CheckButton c:
					val = c.ButtonPressed;
					break;
				case HSlider s:
					val = s.Value;
					break;
			}
			OptionsHelper.SetOption(control.Key, val);
		}
		GD.Print("Updated settings.");
	}

	void UpdateControls()
	{
		foreach (var control in controls.Values)
			control.GetParent().QueueFree();

		controls.Clear();
		
		foreach (var option in OptionsHelper.Options)
		{
			HBoxContainer box = new() { SizeFlagsHorizontal = Control.SizeFlags.ExpandFill };
			optionsContainer.AddChild(box);

			box.AddChild(new Label() { Text = option.Value.FriendlyName });
			
			Control control = new();
			
			switch (option.Value.Value)
			{
				case bool v:
					control = new CheckButton() { ButtonPressed = v };
					break;
				case float v:
					control = new HSlider() { Value = v, MinValue = -1.0f, MaxValue = 1.0f, Step = 0.01f, SizeFlagsHorizontal = Control.SizeFlags.ExpandFill };
					break;
				case int v:
					control = new HSlider() { Value = v, SizeFlagsHorizontal = Control.SizeFlags.ExpandFill };
					break;
			}

			controls[option.Key] = control;

			box.AddChild(control);
		}
	}
}
