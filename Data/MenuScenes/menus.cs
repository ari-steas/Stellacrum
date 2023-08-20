using Godot;
using Godot.Collections;
using System;

public partial class menus : Node2D
{
	[Signal]
	public delegate void ToggleActiveEventHandler(bool active);

	private Array<Node> _children;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_children = GetChildren();
		_SwitchMenu(1);

		TextureLoader.StartLoad("res://Assets/Images/");
	}

	private void _Fullscreen()
	{
		DisplayServer.WindowSetMode((DisplayServer.WindowGetMode() == DisplayServer.WindowMode.ExclusiveFullscreen) ? DisplayServer.WindowMode.Windowed : DisplayServer.WindowMode.ExclusiveFullscreen);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		GetWindow().Title = $"Stellacrum | {Engine.GetFramesPerSecond()}fps {Engine.PhysicsTicksPerSecond}tps";

	}

	private void _SwitchMenu(int toShow)
	{
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
			if (_children[i] is CanvasLayer)
			{
				GD.Print($"Set {i} ({_children[i].Name}) to {(i == toShow - 1)}");
				((CanvasLayer) _children[i]).Visible = (i == toShow - 1);
			}
	}
}
