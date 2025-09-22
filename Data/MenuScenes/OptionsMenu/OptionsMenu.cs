using Godot;
using System;
using System.Collections.Generic;
using System.Threading;
using Stellacrum.Data.MenuScenes;

public partial class OptionsMenu : CanvasLayer, IMenuPage
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
        (FindChild("ResetButton") as Button).Pressed += Reset;

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

    void Reset()
    {
        OptionsHelper.ResetDefaults();
		UpdateControls();
    }

	void UpdateSettings()
	{
		foreach (var control in controls)
		{
			object val = null;

			switch (control.Value)
			{
				case CheckButton c:
					val = c.ButtonPressed;
					break;
				case HSlider s:
					val = (float) s.Value;
					break;
			}
			OptionsHelper.SetOption(control.Key, val);
		}

		FileAccess fileAccess = FileAccess.Open("user://options.json", FileAccess.ModeFlags.Write);
		fileAccess.StoreString(Json.Stringify(OptionsHelper.Save()));
		GD.Print($"Saved {OptionsHelper.Options.Count} options.\n");
	}

	void UpdateControls()
	{
		foreach (var control in optionsContainer.GetChildren())
			control.QueueFree();

		controls.Clear();
		
		foreach (var option in OptionsHelper.Options)
		{
			Label label = new() { Text = option.Value.FriendlyName };
         
            Control control = new();
			
			switch (option.Value.Value)
			{
				case bool v:
					control = new CheckButton() { ButtonPressed = v };
                    HBoxContainer box = new() { SizeFlagsHorizontal = Control.SizeFlags.ExpandFill };
                    optionsContainer.AddChild(box);
                    box.AddChild(label);
                    box.AddChild(control);
                    break;
				case float v:
					control = new HSlider() { MinValue = option.Value.sliderRange.X, MaxValue = option.Value.sliderRange.Y, Step = 0.05f, Value = v, SizeFlagsHorizontal = Control.SizeFlags.ExpandFill };
                    CreateSliderLabelAction((HSlider) control, label, 2);
                    optionsContainer.AddChild(label);
                    optionsContainer.AddChild(control);
                    break;
				case int v:
					control = new HSlider() { MinValue = option.Value.sliderRange.X, MaxValue = option.Value.sliderRange.Y, Step = 1, Value = v, SizeFlagsHorizontal = Control.SizeFlags.ExpandFill };
                    CreateSliderLabelAction((HSlider) control, label);
                    optionsContainer.AddChild(label);
                    optionsContainer.AddChild(control);
                    break;
			}

			controls[option.Key] = control;
        }
	}

	private void CreateSliderLabelAction(HSlider control, Label label, int places = 0)
	{
		string baseText = label.Text + ": ";
        label.Text = baseText + Math.Round(control.Value, places).ToString();

        control.GuiInput += (ie) => { label.Text = baseText + Math.Round(control.Value, places); };
	}

    public void OnOpened()
    {
        UpdateControls();
    }
}
