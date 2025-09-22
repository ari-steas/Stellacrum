using Godot;
using System;
using Stellacrum.Data.MenuScenes;

public partial class main_menu : CanvasLayer, IMenuPage
{
	[Signal]
	public delegate void FullscreenEventHandler();
	[Signal]
	public delegate void MSwitchMenuEventHandler(int toShow);
	
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Visible = false;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	
	
	private void _GoToMenu(int menu)
	{
		EmitSignal(SignalName.MSwitchMenu, menu);
	}

    public void OnOpened()
    {
        Input.MouseMode = Input.MouseModeEnum.Captured;
    }
}
