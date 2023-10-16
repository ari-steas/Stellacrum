using Godot;
using Godot.Collections;
using System;

public partial class menus : Node2D
{
	[Signal]
	public delegate void ToggleActiveEventHandler(bool active);

	private Array<Node> _children;

	public int prevMenu = 0, currentMenu = 0;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_children = GetChildren();
		_SwitchMenu(1);

		TextureLoader.StartLoad("res://Assets/Images/");

		OptionsHelper.AddOption("fullscreen", new("Fullscreen", false, _Fullscreen));
		OptionsHelper.AddOption("fov", new("Field of View", 90, _FovSet, new(30, 120)));
		OptionsHelper.AddOption("vsync", new("Vertical Sync", false, _VSyncSet));
		OptionsHelper.AddOption("fps", new("FPS Limit", 250, _FpsSet, new(0, 1000)));

		Json json = new();
		json.Parse(FileAccess.Open("user://options.json", FileAccess.ModeFlags.Read).GetAsText());

		OptionsHelper.Load(json.Data.As<Dictionary<string, Variant>>());
	}

    private void _Fullscreen(object value)
	{
		if (value is bool fullscreen)
		{
			DisplayServer.WindowSetMode(fullscreen ? DisplayServer.WindowMode.ExclusiveFullscreen : DisplayServer.WindowMode.Windowed);
		}
	}

	private void _FovSet(object value)
	{
        if (value is int fov)
        {
			try
			{
                Camera3D camera = GetViewport().GetCamera3D();
                if (camera == null)
                    return;

                camera.Fov = fov;
                GD.Print("Set camera FOV to " + fov);
            }
            catch
			{

			}
        }
    }

	private void _FpsSet(object value)
	{
		GD.PrintErr("FPS set " + value);
		if (value is int fps)
		{
			Engine.MaxFps = fps;
			GD.Print("Set MaxFps to " + fps);
		}
	}

	private void _VSyncSet(object value)
	{
        if (value is bool vsync)
        {
			DisplayServer.WindowSetVsyncMode(vsync ? DisplayServer.VSyncMode.Enabled : DisplayServer.VSyncMode.Disabled);
			GD.Print("Set VSync to " + vsync);
        }
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		GetWindow().Title = $"Stellacrum | {Engine.GetFramesPerSecond()}fps {Engine.PhysicsTicksPerSecond}tps";
	}


	public void _SwitchMenu(int toShow)
	{
		GD.Print("Current Menu: " + toShow);
		GD.Print("Prev Menu: " + currentMenu + "\n");
		prevMenu = currentMenu;
		currentMenu = toShow;

		if (toShow == 0)
			EmitSignal(SignalName.ToggleActive, true);
		else
		{
			GD.Print("Showing menu " + _children[toShow - 1].Name);
			EmitSignal(SignalName.ToggleActive, false);
		}
		
		if (toShow > _children.Count)
			return;

		for (int i = 0; i < _children.Count; i++)
		{
			if (_children[i] is CanvasLayer)
			{
				GD.Print($"Set {i} ({_children[i].Name}) to {i == toShow - 1}");
				((CanvasLayer) _children[i]).Visible = i == toShow - 1;
			}
		}
	}
}
